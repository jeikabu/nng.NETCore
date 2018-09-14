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

    public class SendReceiveAsyncContext<T> : AsyncBase<T>, ISendReceiveAsyncContext<T>
    {
        public Task<bool> Send(T message)
        {
            CheckState();

            sendTcs = new TaskCompletionSource<bool>();
            sendMessage = message;
            State = AsyncState.Send;
            nng_aio_set_msg(aioHandle, Factory.Borrow(sendMessage));
            nng_send_aio(Socket.NngSocket, aioHandle);
            return sendTcs.Task;
        }

        public Task<T> Receive(CancellationToken token)
        {
            CheckState();

            receiveTcs = new CancellationTokenTaskSource<T>(token);
            State = AsyncState.Recv;
            nng_recv_aio(Socket.NngSocket, aioHandle);
            return receiveTcs.Task;
        }

        internal void callback(IntPtr arg)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Init:
                    break;

                case AsyncState.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        Factory.Destroy(ref sendMessage);
                        sendTcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    sendTcs.SetResult(true);
                    break;

                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        receiveTcs.Tcs.TrySetNngError(res);
                        return;
                    }
                    State = AsyncState.Init;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    receiveTcs.Tcs.SetResult(message);
                    break;

                default:
                    break;
            }
        }

        TaskCompletionSource<bool> sendTcs;
        T sendMessage;
        CancellationTokenTaskSource<T> receiveTcs;
    }
}