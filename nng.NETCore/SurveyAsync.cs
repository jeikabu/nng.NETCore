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
    public class SurveyAsyncContext<T> : AsyncBase<T>, ISendReceiveAsyncContext<T>, ICtx
    {
        public INngCtx Ctx { get; protected set; }

        public static NngResult<ISendReceiveAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new SurveyAsyncContext<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            if (res == 0)
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = AsyncCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    return NngResult<ISendReceiveAsyncContext<T>>.Ok(context);
                }
                return NngResult<ISendReceiveAsyncContext<T>>.Err(ctx.Err());
            }
            return NngResult<ISendReceiveAsyncContext<T>>.Fail(res);
        }

        /// <summary>
        /// Broadcast survey to all peer respondents.
        /// Only one survey can be outstanding at a time; sending another survey will cancel the previous one
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<NngResult<Unit>> Send(T message)
        {
            lock (sync)
            {
                CheckState();

                sendTcs = Extensions.CreateSendResultSource();
                State = AsyncState.Send;
                nng_aio_set_msg(aioHandle, Factory.Take(ref message));
                nng_ctx_send(Ctx.NngCtx, aioHandle);
                return sendTcs.Task;
            }
        }

        /// <summary>
        /// Receive replies to a survey from respondents.  Any responses received before a survey is sent or after it times out are discarded.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <c>NNG_ESTATE</c>- attempted to receive with no outstanding survey.
        /// <c>NNG_ETIMEDOUT</c>- survey timed out while waiting for replies.
        /// </returns>
        public Task<NngResult<T>> Receive(CancellationToken token)
        {
            lock (sync)
            {
                CheckState();

                receiveTcs = Extensions.CreateReceiveSource<T>(token);
                State = AsyncState.Recv;
                nng_ctx_recv(Ctx.NngCtx, aioHandle);
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
                    Console.Error.WriteLine("Survey::AioCallback: " + State);
                    State = AsyncState.Init;
                    break;
            }
        }

        protected TaskCompletionSource<NngResult<Unit>> sendTcs;
        protected CancellationTokenTaskSource<NngResult<T>> receiveTcs;
    }
}