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
        public static NngResult<ISendReceiveAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new SendReceiveAsyncContext<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            return NngResult<ISendReceiveAsyncContext<T>>.OkIfZero(res, context);
        }

        public Task<NngResult<Unit>> Send(T message)
        {
            lock (sync)
            {
                CheckState();

                sendTcs = Extensions.CreateSendResultSource();
                State = AsyncState.Send;
                nng_aio_set_msg(aioHandle, Factory.Take(ref message));
                nng_send_aio(Socket.NngSocket, aioHandle);
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
                nng_recv_aio(Socket.NngSocket, aioHandle);
                return receiveTcs.Task;
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
                        sendTcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    sendTcs.TrySetNngResult();
                    break;

                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        receiveTcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
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