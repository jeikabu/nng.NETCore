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

    struct AsyncSendMsg
    {
        public AsyncSendMsg(nng_msg message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<bool>();
        }
        internal nng_msg message;
        internal TaskCompletionSource<bool> tcs;
    }

    struct AsyncResvMsg<T>
    {
        public AsyncResvMsg(CancellationToken token)
        {
            Source = new CancellationTokenTaskSource<T>(token);
        }
        public CancellationTokenTaskSource<T> Source;
    }

    public class SendAsyncCtx : AsyncBase
    {
        public static object Create(ISocket socket)
        {
            var ctx = new SendAsyncCtx();
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
            asyncMessage = new AsyncSendMsg(message);
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
                        return;
                    }
                    break;

                case State.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        state = State.Init;
                        nng_msg_free(asyncMessage.message);
                        asyncMessage.tcs.SetNngError(res);
                        return;
                    }
                    state = State.Init;
                    asyncMessage.tcs.SetResult(true);
                    break;
                default:
                    asyncMessage.tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        AsyncSendMsg asyncMessage;
    }


    public class ResvAsyncCtx : AsyncBase
    {
        public static object Create(ISocket socket)
        {
            var res = new ResvAsyncCtx();
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

        public async Task<nng_msg> Receive(CancellationToken token)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            if (state != State.Init)
            {
                await asyncMessage.Source.Task;
            }
            asyncMessage = new AsyncResvMsg<nng_msg>(token);
            // Trigger the async read
            callback(IntPtr.Zero);
            return await asyncMessage.Source.Task;
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
                        asyncMessage.Source.Tcs.SetNngError(ret);
                    }
                    break;
                
                case State.Recv:
                    ret = nng_aio_result(aioHandle);
                    if (ret != 0)
                    {
                        state = State.Init;
                        asyncMessage.Source.Tcs.SetNngError(ret);
                        return;
                    }
                    state = State.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    asyncMessage.Source.Tcs.SetResult(msg);
                    break;

                default:
                    asyncMessage.Source.Tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        AsyncResvMsg<nng_msg> asyncMessage;
    }
}