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

    struct AsyncPushMsg
    {
        public AsyncPushMsg(nng_msg message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<bool>();
        }
        internal nng_msg message;
        internal TaskCompletionSource<bool> tcs;
    }

    struct AsyncPullMsg
    {
        public AsyncPullMsg(int _)
        {
            tcs = new TaskCompletionSource<nng_msg>();
        }
        internal TaskCompletionSource<nng_msg> tcs;
    }

    public class PushAsyncCtx : AsyncNoCtx
    {
        public static object Create(IPushSocket socket)
        {
            var ctx = new PushAsyncCtx();
            var res = ctx.Init(socket, ctx.callback);
            if (res != 0)
            {
                return null;
            }
            return ctx;
        }

        public Task<bool> Send(nng_msg message)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            asyncMessage = new AsyncPushMsg(message);
            callback(IntPtr.Zero);
            return asyncMessage.tcs.Task;
        }

        void callback(IntPtr arg)
        {
            var res = 0;
            switch (state)
            {
                case State.Init:
                    state = State.Send;
                    nng_aio_set_msg(aioHandle, asyncMessage.message);
                    res = nng_send_aio(Socket.Socket, aioHandle);
                    if (res != 0)
                    {
                        state = State.Init;
                        asyncMessage.tcs.SetNngError(res);
                    }
                    break;

                case State.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        nng_msg_free(asyncMessage.message);
                        asyncMessage.tcs.SetNngError(res);
                    }
                    state = State.Init;
                    asyncMessage.tcs.SetResult(true);
                    break;
                default:
                    asyncMessage.tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        AsyncPushMsg asyncMessage;
    }


    public class PullAsyncCtx : AsyncNoCtx
    {
        public static object Create(IPullSocket socket)
        {
            var res = new PullAsyncCtx();
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

        public async Task<nng_msg> Receive()
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            if (state != State.Init)
            {
                await asyncMessage.tcs.Task;
            }
            asyncMessage = new AsyncPullMsg(0);
            // Trigger the async read
            callback(IntPtr.Zero);
            return await asyncMessage.tcs.Task;
        }

        void callback(IntPtr arg)
        {
            var ret = 0;
            switch (state)
            {
                case State.Init:
                    state = State.Recv;
                    ret = nng_recv_aio(Socket.Socket, aioHandle);
                    if (ret != 0)
                    {
                        state = State.Init;
                        asyncMessage.tcs.SetNngError(ret);
                    }
                    break;
                
                case State.Recv:
                    ret = nng_aio_result(aioHandle);
                    if (ret != 0)
                    {
                        asyncMessage.tcs.SetNngError(ret);
                        state = State.Init;
                        return;
                    }
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    asyncMessage.tcs.SetResult(msg);
                    state = State.Init;
                    break;

                default:
                    asyncMessage.tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        AsyncPullMsg asyncMessage;
    }
}