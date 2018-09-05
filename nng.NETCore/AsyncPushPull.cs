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

    public class SendAsyncCtx<T> : AsyncBase, ISendAsyncContext<T>
    {
        public static SendAsyncCtx<T> Create(ISocket socket, IMessageFactory<T> msgFactory)
        {
            var ctx = new SendAsyncCtx<T> { factory = msgFactory };
            var res = ctx.Init(socket, ctx.callback);
            if (res != 0)
            {
                return null;
            }
            return ctx;
        }

        public Task<bool> Send(T message)
        {
            System.Diagnostics.Debug.Assert(state == State.Init);
            asyncMessage = new AsyncSendMsg<T>(message);
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
                    nng_aio_set_msg(aioHandle, factory.Borrow(asyncMessage.message));
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
                        factory.Destroy(ref asyncMessage.message);
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

        IMessageFactory<T> factory;
        AsyncSendMsg<T> asyncMessage;
    }


    public class ResvAsyncCtx<T> : AsyncBase, IReceiveAsyncContext<T>
    {
        public static IReceiveAsyncContext<T> Create(ISocket socket, IMessageFactory<T> msgFactory)
        {
            var res = new ResvAsyncCtx<T>{ factory = msgFactory };
            if (res.Init(socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

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
                    var message = factory.CreateMessage(msg);
                    asyncMessage.Source.Tcs.SetResult(message);
                    break;

                default:
                    asyncMessage.Source.Tcs.SetException(new Exception(state.ToString()));
                    break;
            }
        }

        IMessageFactory<T> factory;
        AsyncResvMsg<T> asyncMessage;
    }
}