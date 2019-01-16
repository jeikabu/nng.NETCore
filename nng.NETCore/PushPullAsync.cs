using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;

    struct AsyncSendMsg<T>
    {
        public AsyncSendMsg(T message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        internal T message;
        internal readonly TaskCompletionSource<bool> tcs;
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
        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public Task<bool> Send(T message)
        {
            CheckState();

            asyncMessage = new AsyncSendMsg<T>(message);
            AioCallback(IntPtr.Zero);
            return asyncMessage.tcs.Task;
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Init:
                    State = AsyncState.Send;
                    nng_aio_set_msg(aioHandle, Factory.Borrow(asyncMessage.message));
                    nng_send_aio(Socket.NngSocket, aioHandle);
                    break;

                case AsyncState.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        Factory.Destroy(ref asyncMessage.message);
                        asyncMessage.tcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    asyncMessage.tcs.SetResult(true);
                    break;
                default:
                    asyncMessage.tcs.SetException(new Exception(State.ToString()));
                    break;
            }
        }

        AsyncSendMsg<T> asyncMessage;
    }


    public class ResvAsyncContext<T> : AsyncBase<T>, IReceiveAsyncContext<T>
    {
        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <returns>The receive.</returns>
        /// <param name="token">Token.</param>
        public async Task<T> Receive(CancellationToken token)
        {
            CheckState();

            asyncMessage = new AsyncResvMsg<T>(token);
            // Trigger the async read
            AioCallback(IntPtr.Zero);
            return await asyncMessage.Source.Task;
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Init:
                    State = AsyncState.Recv;
                    nng_recv_aio(Socket.NngSocket, aioHandle);
                    break;

                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        asyncMessage.Source.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    asyncMessage.Source.TrySetResult(message);
                    break;

                default:
                    asyncMessage.Source.TrySetException(new Exception(State.ToString()));
                    break;
            }
        }

        AsyncResvMsg<T> asyncMessage;
    }
}