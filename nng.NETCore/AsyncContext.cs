using nng.Pinvoke;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Pinvoke.Aio;
    using static nng.Pinvoke.Basic;
    using static nng.Pinvoke.Ctx;
    using static nng.Pinvoke.Msg;

    public interface INngResource
    {

    }

    public class NngContext
    {
        public bool NngCheck(int error,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (error == 0)
            {
                return false;
            }
            var str = nng_strerror(error);
            Console.WriteLine($"{memberName}:{sourceLineNumber} failed: {str}");
            return true;
        }
    }

    public interface ISocket
    {
        nng_socket Socket { get; }
    }

    

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

    struct AsyncMsg
    {
        public AsyncMsg(nng_msg message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<nng_msg>();
        }
        internal nng_msg message;
        internal TaskCompletionSource<nng_msg> tcs;
    }

    public class ReqAsyncCtx : AsyncCtx
    {
        public static object Create(IReqSocket socket)
        {
            var res = new ReqAsyncCtx();
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

        public async Task<nng_msg> Send(nng_msg message)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            if (state != State.Init)
            {
                await asyncMessage.tcs.Task;
            }
            asyncMessage = new AsyncMsg(message);
            // Trigger the async send
            callback(IntPtr.Zero);
            return await asyncMessage.tcs.Task;
        }

        void callback(IntPtr arg)
        {
            var ret = 0;
            switch (state)
            {
                case State.Init:
                    state = State.Send;
                    nng_aio_set_msg(aioHandle, asyncMessage.message);
                    nng_ctx_send(ctxHandle, aioHandle);
                    break;
                
                case State.Send:
                    ret = nng_aio_result(aioHandle);
                    if (ret != 0)
                    {
                        nng_msg_free(asyncMessage.message);
                        asyncMessage.tcs.SetException(new NngException(ret));
                        state = State.Init;
                        return;
                    }
                    state = State.Recv;
                    nng_ctx_recv(ctxHandle, aioHandle);
                    break;
                case State.Recv:
                    ret = nng_aio_result(aioHandle);
                    if (ret != 0)
                    {
                        asyncMessage.tcs.SetException(new NngException(ret));
                        state = State.Init;
                        return;
                    }
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    asyncMessage.tcs.SetResult(msg);
                    state = State.Init;
                    break;
            }
        }

        AsyncMsg asyncMessage;
    }

    public class NngException : Exception
    {
        public NngException(string message)
        : base(message)
        {
        }
        public NngException(int errorCode)
        {
            error = errorCode;
        }

        public override string Message => nng_strerror(error);

        int error = 0;
    }

    class Request
    {
        public nng_msg response;
        public TaskCompletionSource<nng_msg> requestTcs = new TaskCompletionSource<nng_msg>();
        public TaskCompletionSource<bool> replyTcs = new TaskCompletionSource<bool>();
    }

    public class RepAsyncCtx : AsyncCtx
    {
        public static object Create(IRepSocket socket)
        {
            var res = new RepAsyncCtx();
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            // Start receive loop
            res.callback(IntPtr.Zero);
            return res;
        }

        public Task<nng_msg> Receive()
        {
            System.Diagnostics.Debug.Assert(state == State.Recv);
            return request.requestTcs.Task;
        }

        public Task<bool> Reply(nng_msg message)
        {
            System.Diagnostics.Debug.Assert(state == State.Wait);
            request.response = message;
            callback(IntPtr.Zero);
            return request.replyTcs.Task;
        }

        void callback(IntPtr arg)
        {
            lock (sync)
            {
                var res = 0;
                switch (state)
                {
                    case State.Init:
                        init();
                        break;
                    case State.Recv:
                        res = nng_aio_result(aioHandle);
                        if (res != 0)
                        {
                            request.requestTcs.SetException(new NngException(res));
                            state = State.Recv;
                            return;
                        }
                        state = State.Wait;
                        nng_msg msg = nng_aio_get_msg(aioHandle);
                        request.requestTcs.SetResult(msg);
                        break;
                    case State.Wait:
                        nng_aio_set_msg(aioHandle, request.response);
                        state = State.Send;
                        nng_ctx_send(ctxHandle, aioHandle);
                        break;
                    case State.Send:
                        res = nng_aio_result(aioHandle);
                        if (res != 0)
                        {
                            nng_msg_free(request.response);
                            request.replyTcs.SetException(new NngException(res));
                        }
                        var currentReq = request;
                        init();
                        currentReq.replyTcs.SetResult(true);
                        break;
                }
            }
        }

        void init()
        {
            request = new Request();
            state = State.Recv;
            nng_ctx_recv(ctxHandle, aioHandle);
        }

        Request request;
        object sync = new object();
    }

    public abstract class AsyncCtx : IDisposable
    {
        protected nng_aio aioHandle = nng_aio.Null;
        protected nng_ctx ctxHandle;
        protected enum State
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected State state = State.Init;

        protected int Init(ISocket socket, AioCallback callback)
        {
            var res = nng_aio_alloc(out aioHandle, callback, IntPtr.Zero);
            if (res != 0)
            {
                return res;
            }
            return nng_ctx_open(ref ctxHandle, socket.Socket);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                nng_aio_free(aioHandle);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}