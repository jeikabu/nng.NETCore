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

    struct AsyncSendMsg<T>
    {
        public AsyncSendMsg(T message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<bool>();
        }
        internal T message;
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

    public class SendAsyncContext<T> : AsyncBase<T>, ISendAsyncContext<T>
    {
        public Task<bool> Send(T message)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            asyncMessage = new AsyncSendMsg<T>(message);
            callback(IntPtr.Zero);
            return asyncMessage.tcs.Task;
        }

        internal void callback(IntPtr arg)
        {
            var res = 0;
            switch (state)
            {
                case State.Init:
                    state = State.Send;
                    nng_aio_set_msg(aioHandle, Factory.Borrow(asyncMessage.message));
                    nng_send_aio(Socket.NngSocket, aioHandle);
                    break;

                case State.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        state = State.Init;
                        Factory.Destroy(ref asyncMessage.message);
                        asyncMessage.tcs.TrySetNngError(res);
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

        AsyncSendMsg<T> asyncMessage;
    }


    public class ResvAsyncContext<T> : AsyncBase<T>, IReceiveAsyncContext<T>
    {
        public async Task<T> Receive(CancellationToken token)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            if (state != State.Init)
            {
                await asyncMessage.Source.Task;
            }
            asyncMessage = new AsyncResvMsg<T>(token);
            // Trigger the async read
            callback(IntPtr.Zero);
            return await asyncMessage.Source.Task;
        }

        internal void callback(IntPtr arg)
        {
            var res = 0;
            switch (state)
            {
                case State.Init:
                    state = State.Recv;
                    nng_recv_aio(Socket.NngSocket, aioHandle);
                    break;
                
                case State.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        state = State.Init;
                        asyncMessage.Source.Tcs.TrySetNngError(res);
                        return;
                    }
                    state = State.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    asyncMessage.Source.Tcs.SetResult(message);
                    break;

                default:
                    asyncMessage.Source.Tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        AsyncResvMsg<T> asyncMessage;
    }
}