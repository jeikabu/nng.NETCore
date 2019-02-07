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
        int Start(Defines.NngFlag flags = default);
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
    /// Represents a socket that can send messages
    /// </summary>
    public interface ISendSocket : ISocket
    {
        /// <summary>
        /// Even if <c>NNG_FLAG_ALLOC</c> is specified a copy may be made.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<Unit> Send(ReadOnlySpan<byte> message, Defines.NngFlag flags = default);
        /// <summary>
        /// .
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flags"><c>NNG_FLAG_ALLOC</c> is implicit</param>
        /// <returns></returns>
        NngResult<Unit> SendZeroCopy(IMemory message, Defines.NngFlag flags = default);
        NngResult<Unit> SendMsg(IMessage message, Defines.NngFlag flags = default);
    }

    /// <summary>
    /// Represents a socket that can receive messages
    /// </summary>
    public interface IRecvSocket : ISocket
    {
        /// <summary>
        /// If <c>NNG_FLAG_ALLOC</c> is used buffer will contain newly allocated buffer.
        /// If <c>NNG_FLAG_ALLOC</c> is not used, buffer must be provided.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns>Number of bytes received and stored in buffer</returns>
        NngResult<UIntPtr> Recv(ref IMemory buffer, Defines.NngFlag flags = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flags"><c>NNG_FLAG_ALLOC</c> is implicit</param>
        /// <returns></returns>
        NngResult<IMemory> RecvZeroCopy(Defines.NngFlag flags = default);
        NngResult<IMessage> RecvMsg(Defines.NngFlag flags = default);
    }

    /// <summary>
    /// Represents publish half of publish/subscribe protocol
    /// </summary>
    public interface IPubSocket : ISendSocket, ISocket { }
    /// <summary>
    /// Represents subscribe half of publish/subscribe protocol
    /// </summary>
    public interface ISubSocket : IRecvSocket, ISocket { }
    /// <summary>
    /// Represents push half of push/pull protocol
    /// </summary>
    public interface IPushSocket : ISendSocket, ISocket { }
    /// <summary>
    /// Represents pull half of push/pull protocol
    /// </summary>
    public interface IPullSocket : IRecvSocket, ISocket { }
    /// <summary>
    /// Represents request half of request/reply protocol
    /// </summary>
    public interface IReqSocket : ISendSocket, IRecvSocket, ISocket { }
    /// <summary>
    /// Represents reply half of request/reply protocol
    /// </summary>
    public interface IRepSocket : ISendSocket, IRecvSocket, ISocket { }
    /// <summary>
    /// Represents node of bus protocol
    /// </summary>
    public interface IBusSocket : ISendSocket, IRecvSocket, ISocket { }

    /// <summary>
    /// Represents one side of 1:1 pair protocol
    /// </summary>
    public interface IPairSocket : ISendSocket, IRecvSocket, ISocket { }

    /// <summary>
    /// Represents respondent half of survey protocol
    /// </summary>
    public interface IRespondentSocket : ISendSocket, IRecvSocket, ISocket { }

    /// <summary>
    /// Represents surveyor half of survey protocol
    /// </summary>
    public interface ISurveyorSocket : ISendSocket, IRecvSocket, ISocket { }
}