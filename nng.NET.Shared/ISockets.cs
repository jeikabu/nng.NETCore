using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// Represents an NNG socket.
    /// </summary>
    public interface INngSocket : IHasSocket, IOptions, IDisposable
    {
        nng_socket NativeNngStruct { get; }
        int Id { get; }
        
        /// <summary>
        /// Create and start a listener that listens at the specified url for incoming connections from dialers.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<Unit> Listen(string url, Defines.NngFlag flags = default);
        /// <summary>
        /// Create and start dialer that dials/connects to the specified url where a listener is listening.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<Unit> Dial(string url, Defines.NngFlag flags = default);
        /// <summary>
        /// Create and start a listener and return it.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<INngListener> ListenWithListener(string url, Defines.NngFlag flags = default);
        /// <summary>
        /// Create and start a dialer and return it.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<INngDialer> DialWithDialer(string url, Defines.NngFlag flags = default);
        /// <summary>
        /// Create a listener.  It must be started to begin listening.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        NngResult<INngListener> ListenerCreate(string url);
        /// <summary>
        /// Create a dialer.  It must be started to dial and initiate a connection.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        NngResult<INngDialer> DialerCreate(string url);

        NngResult<Unit> Notify(Defines.NngPipeEv ev, Defines.PipeEventCallback callback, IntPtr arg);
    }


    public static class OptionsExt
    {
        public static int SetOpt<T>(this ISetOptions socket, string name, T data)
        {
            switch (data)
            {
                case bool value:
                    return socket.SetOpt(name, value);
                case int value:
                    return socket.SetOpt(name, value);
                case nng_duration value:
                    return socket.SetOpt(name, value);
                case IntPtr value:
                    return socket.SetOpt(name, value);
                case UIntPtr value:
                    return socket.SetOpt(name, value);
                case string value:
                    return socket.SetOpt(name, value);
                case UInt64 value:
                    return socket.SetOpt(name, value);
                
            }
            return Defines.NNG_EINVAL;
        }
    }

    /// <summary>
    /// Represents object that has, is, or is like a socket.
    /// </summary>
    /// <remarks>
    /// This includes: actual sockets, socket-like objects, as well types that are attached to 
    /// or otherwise associated a socket.
    /// </remarks>
    public interface IHasSocket
    {
        INngSocket Socket { get; }
    }

    /// <summary>
    /// Represents object that can be started and stopped (via <see cref="IDisposable.Dispose"/>)
    /// </summary>
    public interface IStart : IDisposable
    {
        int Start(Defines.NngFlag flags = default);
    }

    /// <summary>
    /// Represents NNG listener
    /// </summary>
    public interface INngListener : IStart, IOptions
    {
        int Id { get; }
        int GetOpt(string name, out nng_sockaddr data);
    }

    /// <summary>
    /// Represents NNG dialer
    /// </summary>
    public interface INngDialer : IStart, IOptions
    {
        int Id { get; }   
    }

    /// <summary>
    /// Represents a socket that can send messages
    /// </summary>
    public interface ISendSocket : INngSocket
    {
        /// <summary>
        /// Even if <c>NNG_FLAG_ALLOC</c> is specified a copy may be made.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<Unit> Send(ReadOnlySpan<byte> message, Defines.NngFlag flags = default);

        /// <summary>
        /// "Zero-copy" send.
        /// </summary>
        /// <remarks>
        /// If call succeeds, it takes ownership of `message` contents.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="flags"><c>NNG_FLAG_ALLOC</c> is implicit</param>
        /// <returns></returns>
        NngResult<Unit> SendZeroCopy(INngAlloc message, Defines.NngFlag flags = default);

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <remarks>
        /// If call succeeds, it takes ownership of `message` contents.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        NngResult<Unit> SendMsg(INngMsg message, Defines.NngFlag flags = default);

        void Send(INngAio aio);
    }

    /// <summary>
    /// Represents a socket that can receive messages
    /// </summary>
    public interface IRecvSocket : INngSocket
    {
        /// <summary>
        /// If <c>NNG_FLAG_ALLOC</c> is used buffer will contain newly allocated buffer.
        /// If <c>NNG_FLAG_ALLOC</c> is not used, buffer must be provided.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns>Number of bytes received and stored in buffer</returns>
        NngResult<UIntPtr> Recv(ref INngAlloc buffer, Defines.NngFlag flags = default);

        /// <summary>
        /// "Zero-copy" receive.
        /// </summary>
        /// <param name="flags"><c>NNG_FLAG_ALLOC</c> is implicit</param>
        /// <returns></returns>
        NngResult<INngAlloc> RecvZeroCopy(Defines.NngFlag flags = default);

        NngResult<INngMsg> RecvMsg(Defines.NngFlag flags = default);

        void Recv(INngAio aio);
    }

    public interface ISendRecvSocket : ISendSocket, IRecvSocket { }

    /// <summary>
    /// Represents publish half of publish/subscribe protocol
    /// </summary>
    public interface IPubSocket : ISendSocket { }

    /// <summary>
    /// Represents subscribe half of publish/subscribe protocol
    /// </summary>
    public interface ISubSocket : IRecvSocket { }

    /// <summary>
    /// Represents push half of push/pull protocol
    /// </summary>
    public interface IPushSocket : ISendSocket { }

    /// <summary>
    /// Represents pull half of push/pull protocol
    /// </summary>
    public interface IPullSocket : IRecvSocket { }

    /// <summary>
    /// Represents request half of request/reply protocol
    /// </summary>
    public interface IReqSocket : ISendRecvSocket { }

    /// <summary>
    /// Represents reply half of request/reply protocol
    /// </summary>
    public interface IRepSocket : ISendRecvSocket { }

    /// <summary>
    /// Represents node of bus protocol
    /// </summary>
    public interface IBusSocket : ISendRecvSocket { }

    /// <summary>
    /// Represents one side of 1:1 pair protocol
    /// </summary>
    public interface IPairSocket : ISendRecvSocket { }

    /// <summary>
    /// Represents respondent half of survey protocol
    /// </summary>
    public interface IRespondentSocket : ISendRecvSocket { }

    /// <summary>
    /// Represents surveyor half of survey protocol
    /// </summary>
    public interface ISurveyorSocket : ISendRecvSocket { }
}
