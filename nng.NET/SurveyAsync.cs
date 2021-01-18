using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    /// <summary>
    /// In the survey pattern, surveyors broadcast a survey to all respondents.  Respondents can reply within some time limit but don't have to.
    /// There can only be one survey at a time.  Responses received when there is no outstanding survey are discarded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SurveyAsyncContext<T> : AsyncBase<T>, ISurveyorAsyncContext<T>
    {
        public INngCtx Ctx { get; protected set; }
        public override INngSocket Socket => socket;
        ISendRecvSocket socket;

        public static NngResult<ISurveyorAsyncContext<T>> Create(IMessageFactory<T> factory, ISendRecvSocket socket)
        {
            var context = new SurveyAsyncContext<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            if (res.IsOk())
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = NngCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    return NngResult<ISurveyorAsyncContext<T>>.Ok(context);
                }
                return NngResult<ISurveyorAsyncContext<T>>.Err(ctx.Err());
            }
            else 
            {
                return NngResult<ISurveyorAsyncContext<T>>.Fail(res.Err());
            }
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
                Aio.SetMsg(Factory.Take(ref message));
                Ctx.Send(Aio);
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
                Ctx.Recv(Aio);
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
                    sendTcs.TrySetResult(res);
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
                    var msg = Aio.GetMsg();
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