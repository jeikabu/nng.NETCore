using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    public class SendReceiveAsyncContext<T> : AsyncBase<T>, ISendReceiveAsyncContext<T>
    {
        public override INngSocket Socket => socket;
        ISendRecvSocket socket;

        public static NngResult<ISendReceiveAsyncContext<T>> Create(IMessageFactory<T> factory, ISendRecvSocket socket)
        {
            var context = new SendReceiveAsyncContext<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            return res.Into<ISendReceiveAsyncContext<T>>(context);
        }

        public Task<NngResult<Unit>> Send(T message)
        {
            lock (sync)
            {
                CheckState();

                sendTcs = Extensions.CreateSendResultSource();
                State = AsyncState.Send;
                Aio.SetMsg(Factory.Take(ref message));
                socket.Send(Aio);
                return sendTcs.Task;
            }
        }

        public Task<NngResult<T>> Receive(CancellationToken token)
        {
            lock (sync)
            {
                CheckState();

                receiveTcs = Extensions.CreateReceiveSource<T>(token);
                State = AsyncState.Recv;
                socket.Recv(Aio);
                return receiveTcs.Task;
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
                        sendTcs.TrySetNngError(res.Err());
                        return;
                    }
                    State = AsyncState.Init;
                    sendTcs.TrySetNngResult();
                    break;

                case AsyncState.Recv:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        State = AsyncState.Init;
                        receiveTcs.TrySetNngError(res.Err());
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = Aio.GetMsg();
                    var message = Factory.CreateMessage(msg);
                    receiveTcs.TrySetNngResult(message);
                    break;

                case AsyncState.Init:
                default:
                    Console.Error.WriteLine("SendReceive::AioCallback: " + State);
                    State = AsyncState.Init;
                    break;
            }
        }

        protected TaskCompletionSource<NngResult<Unit>> sendTcs;
        protected CancellationTokenTaskSource<NngResult<T>> receiveTcs;
    }
}
