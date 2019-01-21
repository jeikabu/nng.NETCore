using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// Represents an nng socket
    /// </summary>
    public interface ISocket : IOptions, IDisposable
    {
        nng_socket NngSocket { get; }

        //int GetOpt(string name, out string data);
        int GetOpt(string name, out UInt64 data);

        int SetOpt(string name, string data);
        int SetOpt(string name, UInt64 data);
    }

    public static class OptionsExt
    {
        public static int SetOpt<T>(this ISetOptions socket, string name, T data)
        {
            switch (data)
            {
                case bool boolVal:
                    return socket.SetOpt(name, boolVal);
                case int intVal:
                    return socket.SetOpt(name, intVal);
                case nng_duration durVal:
                    return socket.SetOpt(name, durVal);
                case UIntPtr sizeVal:
                    return socket.SetOpt(name, sizeVal);
            }
            return Defines.NNG_EINVAL;
        }
    }

    public interface IHasSocket
    {
        ISocket Socket { get; }
    }

    /// <summary>
    /// Represents object that can be started and stopped (via <see cref="IDisposable.Dispose"/>)
    /// </summary>
    public interface IStart : IDisposable
    {
        int Start(Defines.NngFlag flags = 0);
    }

    /// <summary>
    /// Represents nng listener
    /// </summary>
    public interface IListener : IStart, IOptions
    { }

    /// <summary>
    /// Represents nng dialer
    /// </summary>
    public interface IDialer : IStart, IOptions
    { }

    /// <summary>
    /// Represents publish half of publish/subscribe protocol
    /// </summary>
    public interface IPubSocket : ISocket { }
    /// <summary>
    /// Represents subscribe half of publish/subscribe protocol
    /// </summary>
    public interface ISubSocket : ISocket { }
    /// <summary>
    /// Represents push half of push/pull protocol
    /// </summary>
    public interface IPushSocket : ISocket { }
    /// <summary>
    /// Represents pull half of push/pull protocol
    /// </summary>
    public interface IPullSocket : ISocket { }
    /// <summary>
    /// Represents request half of request/reply protocol
    /// </summary>
    public interface IReqSocket : ISocket { }
    /// <summary>
    /// Represents reply half of request/reply protocol
    /// </summary>
    public interface IRepSocket : ISocket { }
    /// <summary>
    /// Represents node of bus protocol
    /// </summary>
    public interface IBusSocket : ISocket { }

    /// <summary>
    /// Represents one side of 1:1 pair protocol
    /// </summary>
    public interface IPairSocket : ISocket { }

    /// <summary>
    /// Represents respondent half of survey protocol
    /// </summary>
    public interface IRespondentSocket : ISocket { }

    /// <summary>
    /// Represents surveyor half of survey protocol
    /// </summary>
    public interface ISurveyorSocket : ISocket { }
}