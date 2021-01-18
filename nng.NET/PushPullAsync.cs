using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;

    public class SendAsyncContext<T> : AsyncBase<T>, ISendAsyncContext<T>
    {
        public override INngSocket Socket => socket;
        ISendSocket socket;

        public static NngResult<ISendAsyncContext<T>> Create(IMessageFactory<T> factory, ISendSocket socket)
        {
            var context = new SendAsyncContext<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            return res.Into<ISendAsyncContext<T>>(context);
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
                Aio.SetMsg(Factory.Take(ref message));
                socket.Send(Aio);
                return tcs.Task;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = Unit.Ok;
            switch (State)
            {
                case AsyncState.Send:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        HandleFailedSend();
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res.Err());
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
        public override INngSocket Socket => socket;
        IRecvSocket socket;

        public static NngResult<IReceiveAsyncContext<T>> Create(IMessageFactory<T> factory, IRecvSocket socket)
        {
            var context = new ResvAsyncContext<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            return res.Into<IReceiveAsyncContext<T>>(context);
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
                socket.Recv(Aio);
                return tcs.Task;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = Unit.Ok;
            switch (State)
            {
                case AsyncState.Recv:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res.Err());
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = Aio.GetMsg();
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
