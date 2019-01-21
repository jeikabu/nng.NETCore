using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;

    public class ReqAsyncCtx<T> : AsyncBase<T>, IReqRepAsyncContext<T>, ICtx
    {
        public INngCtx Ctx { get; protected set; }

        public static NngResult<IReqRepAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new ReqAsyncCtx<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            if (res == 0)
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = AsyncCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    return NngResult<IReqRepAsyncContext<T>>.Ok(context);
                }
                return NngResult<IReqRepAsyncContext<T>>.Err(ctx.Err());
            }
            return NngResult<IReqRepAsyncContext<T>>.Fail(res);
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
                nng_aio_set_msg(aioHandle, Factory.Take(ref message));
                nng_ctx_send(Ctx.NngCtx, aioHandle);
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
                    State = AsyncState.Recv;
                    nng_ctx_recv(Ctx.NngCtx, aioHandle);
                    break;
                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        State = AsyncState.Init;
                        tcs.TrySetNngError(res);
                        return;
                    }
                    nng_msg msg = nng_aio_get_msg(aioHandle);
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

    public class RepAsyncCtx<T> : AsyncBase<T>, IRepReqAsyncContext<T>, ICtx
    {
        public static NngResult<IRepReqAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var context = new RepAsyncCtx<T> { Factory = factory, Socket = socket };
            var res = context.InitAio();
            if (res == 0)
            {
                //TODO: when get default interface methods in C#8 move this to ICtx
                var ctx = AsyncCtx.Create(socket);
                if (ctx.IsOk())
                {
                    context.Ctx = ctx.Ok();
                    // Start receive loop
                    context.AioCallback(IntPtr.Zero);
                    return NngResult<IRepReqAsyncContext<T>>.Ok(context);
                }
                return NngResult<IRepReqAsyncContext<T>>.Err(ctx.Err());
            }
            return NngResult<IRepReqAsyncContext<T>>.Fail(res);
        }

        public INngCtx Ctx { get; protected set; }

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
                nng_aio_set_msg(aioHandle, Factory.Take(ref message));
                nng_ctx_send(Ctx.NngCtx, aioHandle);
                return ret;
            }
        }

        protected override void AioCallback(IntPtr argument)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Init:
                    init();
                    break;
                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        asyncMessage.requestTcs.TrySetNngError(res);
                        State = AsyncState.Recv;
                        return;
                    }
                    State = AsyncState.Wait;
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    asyncMessage.requestTcs.SetNngResult(message);
                    break;
                case AsyncState.Wait:
                    break;
                case AsyncState.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        HandleFailedSend();
                        asyncMessage.replyTcs.TrySetNngError(res);
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
            nng_ctx_recv(Ctx.NngCtx, aioHandle);
        }

        private RepAsyncCtx() { }

        Request<T> asyncMessage;
    }

}