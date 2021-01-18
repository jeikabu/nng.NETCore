using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Ctx.UnsafeNativeMethods;

    public class ReqAsyncCtx<T> : AsyncBase<T>, IReqRepAsyncContext<T>
    {
        public INngCtx Ctx { get; protected set; }
        public override INngSocket Socket => socket;
        IReqSocket socket;

        public static NngResult<IReqRepAsyncContext<T>> Create(IMessageFactory<T> factory, IReqSocket socket)
        {
            var context = new ReqAsyncCtx<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            if (res.IsOk())
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = NngCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    return NngResult<IReqRepAsyncContext<T>>.Ok(context);
                }
                return NngResult<IReqRepAsyncContext<T>>.Err(ctx.Err());
            }
            else
            {
                return NngResult<IReqRepAsyncContext<T>>.Fail(res.Err());
            }
        }

        public NngResult<Unit> SetResendTime(int msTimeout)
        {
            var res = nng_ctx_setopt_ms(Ctx.NativeNngStruct, nng.Native.Defines.NNG_OPT_REQ_RESENDTIME, new nng_duration { TimeMs = msTimeout });
            return Unit.OkIfZero(res);
        }

        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public Task<NngResult<T>> Send(T message)
        {
            lock (sync)
            {
                CheckState();

                State = AsyncState.Send;
                tcs = Extensions.CreateSource<T>();
                Aio.SetMsg(Factory.Take(ref message));
                Ctx.Send(Aio);
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
                    State = AsyncState.Recv;
                    Ctx.Recv(Aio);
                    break;
                case AsyncState.Recv:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res.Err());
                        return;
                    }
                    nng_msg msg = Aio.GetMsg();
                    var message = Factory.CreateMessage(msg);
                    State = AsyncState.Init;
                    tcs.SetNngResult(message);
                    break;
                case AsyncState.Init:
                default:
                    tcs.SetException(new Exception(State.ToString()));
                    break;
            }
        }

        TaskCompletionSource<NngResult<T>> tcs;
    }

    class Request<T>
    {
        public readonly TaskCompletionSource<NngResult<T>> requestTcs = Extensions.CreateSource<T>();
        public readonly TaskCompletionSource<NngResult<Unit>> replyTcs = Extensions.CreateSendResultSource();
    }

    public class RepAsyncCtx<T> : AsyncBase<T>, IRepReqAsyncContext<T>
    {
        public static NngResult<IRepReqAsyncContext<T>> Create(IMessageFactory<T> factory, IRepSocket socket)
        {
            var context = new RepAsyncCtx<T> { Factory = factory, socket = socket };
            var res = context.InitAio();
            if (res.IsOk())
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = NngCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    // Start receive loop
                    context.AioCallback(IntPtr.Zero);
                    return NngResult<IRepReqAsyncContext<T>>.Ok(context);
                }
                return NngResult<IRepReqAsyncContext<T>>.Err(ctx.Err());
            }
            else
            {
                return NngResult<IRepReqAsyncContext<T>>.Fail(res.Err());
            }
        }

        public INngCtx Ctx { get; protected set; }
        public override INngSocket Socket => socket;
        IRepSocket socket;

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <returns>The receive.</returns>
        public Task<NngResult<T>> Receive()
        {
            lock (sync)
            {
                return asyncMessage.requestTcs.Task;
            }
        }

        /// <summary>
        /// Reply with the specified message.
        /// </summary>
        /// <returns>The reply.</returns>
        /// <param name="message">Message.</param>
        public Task<NngResult<Unit>> Reply(T message)
        {
            lock (sync)
            {
                System.Diagnostics.Debug.Assert(State == AsyncState.Wait);
                State = AsyncState.Send;
                // Save response TCS here to avoid race where send completes and asyncMessage replaced before we 
                // can return it
                var ret = asyncMessage.replyTcs.Task;
                Aio.SetMsg(Factory.Take(ref message));
                Ctx.Send(Aio);
                return ret;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = Unit.Ok;
            switch (State)
            {
                case AsyncState.Init:
                    init();
                    break;
                case AsyncState.Recv:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        asyncMessage.requestTcs.TrySetNngError(res.Err());
                        State = AsyncState.Recv;
                        return;
                    }
                    State = AsyncState.Wait;
                    nng_msg msg = Aio.GetMsg();
                    var message = Factory.CreateMessage(msg);
                    asyncMessage.requestTcs.SetNngResult(message);
                    break;
                case AsyncState.Wait:
                    break;
                case AsyncState.Send:
                    res = Aio.GetResult();
                    if (res.IsErr())
                    {
                        HandleFailedSend();
                        asyncMessage.replyTcs.TrySetNngError(res.Err());
                    }
                    var currentReq = asyncMessage;
                    init();
                    currentReq.replyTcs.SetNngResult();
                    break;
            }
        }

        void init()
        {
            asyncMessage = new Request<T>();
            State = AsyncState.Recv;
            Ctx.Recv(Aio);
        }

        private RepAsyncCtx() { }

        Request<T> asyncMessage;
    }

}