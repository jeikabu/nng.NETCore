using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Ctx.UnsafeNativeMethods;

    public class SubAsyncCtx<T> : AsyncBase<T>, ISubAsyncContext<T>, ICtx
    {
        public INngCtx Ctx { get; protected set; }

        public static NngResult<ISubAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new SubAsyncCtx<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            if (res.IsOk())
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = AsyncCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    return NngResult<ISubAsyncContext<T>>.Ok(context);
                }
                return NngResult<ISubAsyncContext<T>>.Err(ctx.Err());
            }
            else
            {
                return NngResult<ISubAsyncContext<T>>.Fail(res.Err());
            }
        }

        public NngResult<Unit> SetResendTime(int msTimeout)
        {
            var res = nng_ctx_setopt_ms(Ctx.NngCtx, nng.Native.Defines.NNG_OPT_REQ_RESENDTIME, new nng_duration { TimeMs = msTimeout });
            return Unit.OkIfZero(res);
        }

        public Task<NngResult<T>> Receive(CancellationToken token)
        {
            lock (sync)
            {
                CheckState();

                tcs = Extensions.CreateReceiveSource<T>(token);
                State = AsyncState.Recv;
                Ctx.Recv(Aio);
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
                    var msg = Aio.GetMsg();
                    var message = Factory.CreateMessage(msg);
                    State = AsyncState.Init;
                    tcs.TrySetNngResult(message);
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