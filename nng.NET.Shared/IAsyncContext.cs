using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    // public class NngContext
    // {
    //     public bool NngCheck(int error,
    //         [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    //         [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    //     {
    //         if (error == 0)
    //         {
    //             return false;
    //         }
    //         var str = nng_strerror(error);
    //         Console.WriteLine($"{memberName}:{sourceLineNumber} failed: {str}");
    //         return true;
    //     }
    // }

    /// <summary>
    /// Context for asynchronous nng operations.  Most likely involves nng_aio, only involves nng_ctx if supported by protocol.
    /// </summary>
    public interface IAsyncContext : IHasSocket, IHasAio, IDisposable
    {
        [Obsolete("Use SetTimeout() on IHasAio.Aio property")]
        void SetTimeout(int msTimeout);
        [Obsolete("Use Cancel() on IHasAio.Aio property")]
        void Cancel();
    }

    /// <summary>
    /// Context supporting asynchronously sending messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISendAsyncContext<T> : IAsyncContext
    {
        Task<NngResult<Unit>> Send(T message);
    }

    /// <summary>
    /// Context supporting asynchronously receiving messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReceiveAsyncContext<T> : IAsyncContext
    {
        Task<NngResult<T>> Receive(CancellationToken token);
    }

    /// <summary>
    /// Context supporting both asynchronously sending and receiving messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISendReceiveAsyncContext<T> : ISendAsyncContext<T>, IReceiveAsyncContext<T>
    {
    }

    /// <summary>
    /// Context with asynchronous request half of request/reply protocol
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReqRepAsyncContext<T> : IAsyncContext, IHasCtx
    {
        Task<NngResult<T>> Send(T message);
        NngResult<Unit> SetResendTime(int msTimeout);
    }

    /// <summary>
    /// Context with asynchronous reply half of request/reply protocol
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepReqAsyncContext<T> : IAsyncContext, IHasCtx
    {
        Task<NngResult<T>> Receive();
        Task<NngResult<Unit>> Reply(T message);
    }

    /// <summary>
    /// Context with subscribe half of publish/subscribe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// For publish half see <see cref="ISendAsyncContext{T}"/>
    /// </remarks>
    public interface ISubAsyncContext<T> : IReceiveAsyncContext<T>, IHasCtx
    {
    }

    /// <summary>
    /// Context with surveyor half of survey pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISurveyorAsyncContext<T> : ISendReceiveAsyncContext<T>, IHasCtx
    {
    }

    public static class AsyncContextExt
    {
        #region INngSocket.CreateAsyncContext
        public static NngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this IPushSocket socket, IAPIFactory<T> factory) => factory.CreateSendAsyncContext(socket);
        public static NngResult<IReceiveAsyncContext<T>> CreateAsyncContext<T>(this IPullSocket socket, IAPIFactory<T> factory) => factory.CreateReceiveAsyncContext(socket);
        public static NngResult<ISendReceiveAsyncContext<T>> CreateAsyncContext<T>(this IBusSocket socket, IAPIFactory<T> factory) => factory.CreateSendReceiveAsyncContext(socket, SendReceiveContextSubtype.Bus);
        public static NngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this IPubSocket socket, IAPIFactory<T> factory) => factory.CreateSendAsyncContext(socket);
        public static NngResult<ISubAsyncContext<T>> CreateAsyncContext<T>(this ISubSocket socket, IAPIFactory<T> factory) => factory.CreateSubAsyncContext(socket);
        public static NngResult<IReqRepAsyncContext<T>> CreateAsyncContext<T>(this IReqSocket socket, IAPIFactory<T> factory) => factory.CreateReqRepAsyncContext(socket);
        public static NngResult<IRepReqAsyncContext<T>> CreateAsyncContext<T>(this IRepSocket socket, IAPIFactory<T> factory) => factory.CreateRepReqAsyncContext(socket);
        public static NngResult<ISendReceiveAsyncContext<T>> CreateAsyncContext<T>(this IPairSocket socket, IAPIFactory<T> factory) => factory.CreateSendReceiveAsyncContext(socket, SendReceiveContextSubtype.Pair);
        public static NngResult<ISurveyorAsyncContext<T>> CreateAsyncContext<T>(this IRespondentSocket socket, IAPIFactory<T> factory) => factory.CreateSurveyorAsyncContext(socket);
        public static NngResult<ISurveyorAsyncContext<T>> CreateAsyncContext<T>(this ISurveyorSocket socket, IAPIFactory<T> factory) => factory.CreateSurveyorAsyncContext(socket);
        #endregion
    }
}