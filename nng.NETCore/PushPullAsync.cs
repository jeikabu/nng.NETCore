using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;

    public class SendAsyncContext<T> : AsyncBase<T>, ISendAsyncContext<T>
    {
        public static NngResult<ISendAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new SendAsyncContext<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            return NngResult<ISendAsyncContext<T>>.OkIfZero(res, context);
        }

        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public Task<NngResult<Unit>> Send(T message)
        {
            lock (sync)
            {
                CheckState();

                tcs = Extensions.CreateSendResultSource();
                State = AsyncState.Send;
                nng_aio_set_msg(aioHandle, Factory.Take(ref message));
                nng_send_aio(Socket.NngSocket, aioHandle);
                return tcs.Task;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        HandleFailedSend();
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    tcs.SetNngResult();
                    break;
                case AsyncState.Init:
                default:
                    tcs.SetException(new Exception(State.ToString()));
                    break;
            }
        }

        TaskCompletionSource<NngResult<Unit>> tcs;
    }

    public class ResvAsyncContext<T> : AsyncBase<T>, IReceiveAsyncContext<T>
    {
        public static NngResult<IReceiveAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new ResvAsyncContext<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            return NngResult<IReceiveAsyncContext<T>>.OkIfZero(res, context);
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <returns>The receive.</returns>
        /// <param name="token">Token.</param>
        public Task<NngResult<T>> Receive(CancellationToken token)
        {
            lock (sync)
            {
                CheckState();

                tcs = Extensions.CreateReceiveSource<T>(token);
                State = AsyncState.Recv;
                nng_recv_aio(Socket.NngSocket, aioHandle);
                return tcs.Task;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    tcs.TrySetResult(NngResult<T>.Ok(message));
                    break;

                case AsyncState.Init:
                default:
                    tcs.TrySetException(new Exception(State.ToString()));
                    break;
            }
        }

        CancellationTokenTaskSource<NngResult<T>> tcs;
    }
}