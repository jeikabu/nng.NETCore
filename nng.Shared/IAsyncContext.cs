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

    // public interface IFactory
    // {
    //     IReplySocket CreateRep();
    //     IRequestSocket CreateReq();
    // }

    // public class AsyncFactory : IFactory
    // {
    //     public IReplySocket CreateRep()
    //     {

    //     }

    //     public IRequestSocket CreateReq()
    //     {

    //     }
    // }

    public interface IAsyncContext : IHasSocket, IDisposable
    {
        void SetTimeout(int msTimeout);
        void Cancel();
    }

    public interface ISendAsyncContext<T> : IAsyncContext
    {
        Task<bool> Send(T message);
    }

    public interface IReceiveAsyncContext<T> : IAsyncContext
    {
        Task<T> Receive(CancellationToken token);
    }

    public interface ISendReceiveAsyncContext<T> : ISendAsyncContext<T>, IReceiveAsyncContext<T>
    {
    }

    public interface IReqRepAsyncContext<T> : IAsyncContext
    {
        Task<T> Send(T message);
    }

    public interface IRepReqAsyncContext<T> : IAsyncContext
    {
        Task<T> Receive();
        Task<bool> Reply(T message);
    }

    public interface ICtx : IAsyncContext
    {
        nng_ctx NngCtx { get; }

        int GetCtxOpt(string name, out bool data);
        int GetCtxOpt(string name, out int data);
        int GetCtxOpt(string name, out nng_duration data);
        int GetCtxOpt(string name, out UIntPtr data);
        // int GetCtxOpt(string name, out string data);
        // int GetCtxOpt(string name, out UInt64 data);

        int SetCtxOpt(string name, byte[] data);
        int SetCtxOpt(string name, bool data);
        int SetCtxOpt(string name, int data);
        int SetCtxOpt(string name, nng_duration data);
        int SetCtxOpt(string name, UIntPtr data);
        // int SetCtxOpt(string name, string data);
        // int SetCtxOpt(string name, UInt64 data);
    }

    public interface ISubAsyncContext<T> : IReceiveAsyncContext<T>, ISubscriber
    {
    }

    public static class AsyncContextExt
    {
        #region ISocket.CreateAsyncContext
        public static INngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IPushSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IPushSocket> ok)
            {
                return factory.CreateSendAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IPushSocket> err)
            {
                return NngResult.Fail<ISendAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }
        public static INngResult<IReceiveAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IPullSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IPullSocket> ok)
            {
                return factory.CreateReceiveAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IPullSocket> err)
            {
                return NngResult.Fail<IReceiveAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }

        public static INngResult<ISendReceiveAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IBusSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IBusSocket> ok)
            {
                return factory.CreateSendReceiveAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IBusSocket> err)
            {
                return NngResult.Fail<ISendReceiveAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }

        public static INngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IPubSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IPubSocket> ok)
            {
                return factory.CreateSendAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IPubSocket> err)
            {
                return NngResult.Fail<ISendAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }
        public static INngResult<ISubAsyncContext<T>> CreateAsyncContext<T>(this INngResult<ISubSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<ISubSocket> ok)
            {
                return factory.CreateSubAsyncContext(ok.Result);
            }
            else if (socket is NngErr<ISubSocket> err)
            {
                return NngResult.Fail<ISubAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }

        public static INngResult<IReqRepAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IReqSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IReqSocket> ok)
            {
                return factory.CreateReqRepAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IReqSocket> err)
            {
                return NngResult.Fail<IReqRepAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }
        public static INngResult<IRepReqAsyncContext<T>> CreateAsyncContext<T>(this INngResult<IRepSocket> socket, IAPIFactory<T> factory)
        {
            if (socket is NngOk<IRepSocket> ok)
            {
                return factory.CreateRepReqAsyncContext(ok.Result);
            }
            else if (socket is NngErr<IRepSocket> err)
            {
                return NngResult.Fail<IRepReqAsyncContext<T>>(err.Error);
            }
            throw new Exception();
        }
        #endregion

        // #region IFactory.CreateAsyncContext
        // public static INngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this IAPIFactory<T> factory, IPushSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateSendAsyncContext(socket);
        // }
        // public static IReceiveAsyncContext<T> CreateAsyncContext<T>(this IAPIFactory<T> factory, IPullSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateReceiveAsyncContext(socket);
        // }

        // public static ISendReceiveAsyncContext<T> CreateAsyncContext<T>(this IAPIFactory<T> factory, IBusSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateSendReceiveAsyncContext(socket);
        // }

        // public static INngResult<ISendAsyncContext<T>> CreateAsyncContext<T>(this IAPIFactory<T> factory, IPubSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateSendAsyncContext(socket);
        // }
        // public static ISubAsyncContext<T> CreateAsyncContext<T>(this IAPIFactory<T> factory, ISubSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateSubAsyncContext(socket);
        // }

        // public static IReqRepAsyncContext<T> CreateAsyncContext<T>(this IAPIFactory<T> factory, IReqSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateReqRepAsyncContext(socket);
        // }
        // public static IRepReqAsyncContext<T> CreateAsyncContext<T>(this IAPIFactory<T> factory, IRepSocket socket)
        // {
        //     if (socket == null)
        //         return null;
        //     return factory.CreateRepReqAsyncContext(socket);
        // }
        // #endregion

        public static INngResult<ISendAsyncContext<T>> CreatePublisher<T>(this IAPIFactory<T> factory, string url)
        {
            var socket = factory.PublisherCreate(url);
            return socket.CreateAsyncContext(factory);
        }
        public static INngResult<ISubAsyncContext<T>> CreateSubscriber<T>(this IAPIFactory<T> factory, string url)
        {
            var socket = factory.SubscriberCreate(url);
            return socket.CreateAsyncContext(factory);
        }

        public static INngResult<ISendAsyncContext<T>> CreatePusher<T>(this IAPIFactory<T> factory, string url, bool isListener)
        {
            var res = factory.PusherCreate(url, isListener);
            return res.CreateAsyncContext(factory);
        }
        public static INngResult<IReceiveAsyncContext<T>> CreatePuller<T>(this IAPIFactory<T> factory, string url, bool isListener)
        {
            var socket = factory.PullerCreate(url, isListener);
            return socket.CreateAsyncContext(factory);
        }

        public static INngResult<IReqRepAsyncContext<T>> CreateRequester<T>(this IAPIFactory<T> factory, string url)
        {
            var socket = factory.RequesterCreate(url);
            return socket.CreateAsyncContext(factory);
        }
        public static INngResult<IRepReqAsyncContext<T>> CreateReplier<T>(this IAPIFactory<T> factory, string url)
        {
            var socket = factory.ReplierCreate(url);
            return socket.CreateAsyncContext(factory);
        }
    }
}