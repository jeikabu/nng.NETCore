/// <summary>
/// C# version of nngcat.
/// https://nanomsg.github.io/nng/man/v1.1.0/nngcat.1
/// </summary>
/// <example>
/// <code>
/// dotnet run -- --rep --listen tcp://localhost:8080 --data "42" &
/// dotnet run -- --req --dial tcp://localhost:8080 --data "what"
/// </code>
/// </example>
using Microsoft.Extensions.CommandLineUtils;
using nng;
using System;
using System.IO;
using System.Text;

namespace nngcat
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "nngcat";
            app.HelpOption("-? | -h | --help");
            var repOpt = app.Option("--rep", "Rep protocol", CommandOptionType.NoValue);
            var reqOpt = app.Option("--req", "Req protocol", CommandOptionType.NoValue);
            var listenOpt = app.Option("--bind | --listen <URL>", "Connect to the peer at the address specified by URL.", CommandOptionType.SingleValue);
            var dialOpt = app.Option("--connect | --dial <URL>", "Bind to, and accept connections from peers, at the address specified by URL.", CommandOptionType.SingleValue);
            // Receive options
            var quotedOpt = app.Option("-Q | --quoted", "Currently always enabled", CommandOptionType.NoValue);
            // Transmit options
            var dataOpt = app.Option("-D | --data <DATA>", "Use DATA for the body of outgoing messages.", CommandOptionType.SingleValue);

            app.OnExecute(() => {
                var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var ctx = new nng.NngLoadContext(path);
                var factory = nng.NngLoadContext.Init(ctx);

                if (repOpt.HasValue())
                {
                    using (var socket = factory.ReplierOpen().listenOrDial(listenOpt, dialOpt))
                    {
                        var request = socket.RecvMsg().Unwrap();
                        var str = Encoding.ASCII.GetString(request.AsSpan());
                        Console.WriteLine(str);
                        var reply = factory.CreateMessage();
                        var replyBytes = Encoding.ASCII.GetBytes(dataOpt.Value());
                        reply.Append(replyBytes);
                        socket.SendMsg(reply);
                    }
                } else if (reqOpt.HasValue())
                {
                    using (var socket = factory.RequesterOpen().listenOrDial(listenOpt, dialOpt))
                    {
                        var request = factory.CreateMessage();
                        var requestBytes = Encoding.ASCII.GetBytes(dataOpt.Value());
                        request.Append(requestBytes);
                        socket.SendMsg(request).Unwrap();
                        var reply = socket.RecvMsg().Unwrap();
                        var str = Encoding.ASCII.GetString(reply.AsSpan());
                        Console.WriteLine('"' + str + '"');
                    }
                }
                return 0;
            });

            app.Execute(args);
        }
    }

    static class SocketExt
    {
        public static T listenOrDial<T>(this NngResult<T> socket, CommandOption listenOpt, CommandOption dialOpt)
            where T: INngSocket
        {
            if (listenOpt.HasValue())
                return socket.ThenListen(listenOpt.Value()).Unwrap();
            else if (dialOpt.HasValue())
                return socket.ThenDial(dialOpt.Value()).Unwrap();
            else
                throw new ArgumentException("Must --listen or --dial");
        }
    }
}
