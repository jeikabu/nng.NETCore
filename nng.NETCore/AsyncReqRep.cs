using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    struct AsyncReqRespMsg<T>
    {
        public AsyncReqRespMsg(T message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<T>();
        }
        internal T message;
        internal TaskCompletionSource<T> tcs;
    }

    public class ReqAsyncCtx<T> : AsyncCtx, IReqRepAsyncContext<T>
    {
        public static IReqRepAsyncContext<T> Create(ISocket socket, IMessageFactory<T> msgFactory)
        {
            var res = new ReqAsyncCtx<T>{ factory = msgFactory };
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

        public async Task<T> Send(T message)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            if (state != State.Init)
            {
                await asyncMessage.tcs.Task;
            }
            asyncMessage = new AsyncReqRespMsg<T>(message);
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
                    nng_aio_set_msg(aioHandle, factory.Borrow(asyncMessage.message));
                    nng_ctx_send(ctxHandle, aioHandle);
                    break;
                
                case State.Send:
                    ret = nng_aio_result(aioHandle);
                    if (ret != 0)
                    {
                        factory.Destroy(ref asyncMessage.message);
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
                    var message = factory.CreateMessage(msg);
                    asyncMessage.tcs.SetResult(message);
                    state = State.Init;
                    break;
            }
        }

        IMessageFactory<T> factory;
        AsyncReqRespMsg<T> asyncMessage;
    }

    class Request<T>
    {
        public T response;
        public TaskCompletionSource<T> requestTcs = new TaskCompletionSource<T>();
        public TaskCompletionSource<bool> replyTcs = new TaskCompletionSource<bool>();
    }

    public class RepAsyncCtx<T> : AsyncCtx, IRepReqAsyncContext<T>
    {
        public static IRepReqAsyncContext<T> Create(ISocket socket, IMessageFactory<T> msgFactory)
        {
            var res = new RepAsyncCtx<T>{ factory = msgFactory };
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            // Start receive loop
            res.callback(IntPtr.Zero);
            return res;
        }

        public Task<T> Receive()
        {
            return asyncMessage.requestTcs.Task;
        }

        public Task<bool> Reply(T message)
        {
            System.Diagnostics.Debug.Assert(state == State.Wait);
            asyncMessage.response = message;
            // Move from wait to send state
            callback(IntPtr.Zero);
            return asyncMessage.replyTcs.Task;
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
                            asyncMessage.requestTcs.SetException(new NngException(res));
                            state = State.Recv;
                            return;
                        }
                        state = State.Wait;
                        nng_msg msg = nng_aio_get_msg(aioHandle);
                        var message = factory.CreateMessage(msg);
                        asyncMessage.requestTcs.SetResult(message);
                        break;
                    case State.Wait:
                        nng_aio_set_msg(aioHandle, factory.Borrow(asyncMessage.response));
                        state = State.Send;
                        nng_ctx_send(ctxHandle, aioHandle);
                        break;
                    case State.Send:
                        res = nng_aio_result(aioHandle);
                        if (res != 0)
                        {
                            factory.Destroy(ref asyncMessage.response);
                            asyncMessage.replyTcs.SetException(new NngException(res));
                        }
                        var currentReq = asyncMessage;
                        init();
                        currentReq.replyTcs.SetResult(true);
                        break;
                }
            }
        }

        void init()
        {
            asyncMessage = new Request<T>();
            state = State.Recv;
            nng_ctx_recv(ctxHandle, aioHandle);
        }

        IMessageFactory<T> factory;
        Request<T> asyncMessage;
        object sync = new object();
    }

}