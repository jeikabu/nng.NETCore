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

    struct AsyncReqRespMsg
    {
        public AsyncReqRespMsg(nng_msg message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<nng_msg>();
        }
        internal nng_msg message;
        internal TaskCompletionSource<nng_msg> tcs;
    }

    public class ReqAsyncCtx : AsyncCtx
    {
        public static IAsyncContext Create(IReqSocket socket)
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
            asyncMessage = new AsyncReqRespMsg(message);
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

        AsyncReqRespMsg asyncMessage;
    }

    class Request
    {
        public nng_msg response;
        public TaskCompletionSource<nng_msg> requestTcs = new TaskCompletionSource<nng_msg>();
        public TaskCompletionSource<bool> replyTcs = new TaskCompletionSource<bool>();
    }

    public class RepAsyncCtx : AsyncCtx
    {
        public static IAsyncContext Create(IRepSocket socket)
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
            return request.requestTcs.Task;
        }

        public Task<bool> Reply(nng_msg message)
        {
            System.Diagnostics.Debug.Assert(state == State.Wait);
            request.response = message;
            // Move from wait to send state
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

}