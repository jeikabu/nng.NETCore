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

    /// <summary>
    /// In the survey pattern, surveyors broadcast a survey to all respondents.  Respondents can reply within some time limit but don't have to.
    /// There can only be one survey at a time.  Responses received when there is no outstanding survey are discarded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SurveyAsyncContext<T> : AsyncCtx<T>, ISendReceiveAsyncContext<T>
    {
        /// <summary>
        /// Broadcast survey to all peer respondents.
        /// Only one survey can be outstanding at a time; sending another survey will cancel the previous one
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<bool> Send(T message)
        {
            CheckState();

            sendTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            sendMessage = message;
            State = AsyncState.Send;
            nng_aio_set_msg(aioHandle, Factory.Borrow(sendMessage));
            nng_ctx_send(ctxHandle, aioHandle);
            return sendTcs.Task;
        }

        /// <summary>
        /// Receive replies to a survey from respondents.  Any responses received before a survey is sent or after it times out are discarded.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <c>NNG_ESTATE</c>- attempted to receive with no outstanding survey.
        /// <c>NNG_ETIMEDOUT</c>- survey timed out while waiting for replies.
        /// </returns>
        public Task<T> Receive(CancellationToken token)
        {
            CheckState();

            receiveTcs = new CancellationTokenTaskSource<T>(token);
            State = AsyncState.Recv;
            nng_ctx_recv(ctxHandle, aioHandle);
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

        protected TaskCompletionSource<bool> sendTcs;
        protected T sendMessage;
        protected CancellationTokenTaskSource<T> receiveTcs;
    }
}