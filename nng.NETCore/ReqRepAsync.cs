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

    struct AsyncReqRespMsg<T>
    {
        public AsyncReqRespMsg(T message)
        {
            this.message = message;
            tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        internal T message;
        internal readonly TaskCompletionSource<T> tcs;
    }

    public class ReqAsyncCtx<T> : AsyncCtx<T>, IReqRepAsyncContext<T>
    {
        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public Task<T> Send(T message)
        {
            CheckState();
            asyncMessage = new AsyncReqRespMsg<T>(message);
            // Trigger the async send
            callback(IntPtr.Zero);
            return asyncMessage.tcs.Task;
        }

        internal void callback(IntPtr arg)
        {
            var res = 0;
            switch (State)
            {
                case AsyncState.Init:
                    State = AsyncState.Send;
                    nng_aio_set_msg(aioHandle, Factory.Borrow(asyncMessage.message));
                    nng_ctx_send(ctxHandle, aioHandle);
                    break;

                case AsyncState.Send:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        Factory.Destroy(ref asyncMessage.message);
                        asyncMessage.tcs.TrySetNngError(res);
                        State = AsyncState.Init;
                        return;
                    }
                    State = AsyncState.Recv;
                    nng_ctx_recv(ctxHandle, aioHandle);
                    break;
                case AsyncState.Recv:
                    res = nng_aio_result(aioHandle);
                    if (res != 0)
                    {
                        asyncMessage.tcs.TrySetNngError(res);
                        State = AsyncState.Init;
                        return;
                    }
                    nng_msg msg = nng_aio_get_msg(aioHandle);
                    var message = Factory.CreateMessage(msg);
                    asyncMessage.tcs.SetResult(message);
                    State = AsyncState.Init;
                    break;
            }
        }

        AsyncReqRespMsg<T> asyncMessage;
    }

    class Request<T>
    {
        public T response;
        public readonly TaskCompletionSource<T> requestTcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource<bool> replyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public class RepAsyncCtx<T> : AsyncCtx<T>, IRepReqAsyncContext<T>
    {
        public static INngResult<IRepReqAsyncContext<T>> Create(IMessageFactory<T> factory, ISocket socket)
        {
            var ctx = new RepAsyncCtx<T>();
            var res = ctx.Init(factory, socket, ctx.callback);
            if (res == 0)
            {
                // Start receive loop
                ctx.callback(IntPtr.Zero);
                return NngResult.Ok<IRepReqAsyncContext<T>>(ctx);
            }
            else
            {
                return NngResult.Fail<IRepReqAsyncContext<T>>(res);
            }
        }

        /// <summary>
        /// Receive a message.
        /// </summary>
        /// <returns>The receive.</returns>
        public Task<T> Receive()
        {
            return asyncMessage.requestTcs.Task;
        }

        /// <summary>
        /// Reply with the specified message.
        /// </summary>
        /// <returns>The reply.</returns>
        /// <param name="message">Message.</param>
        public Task<bool> Reply(T message)
        {
            System.Diagnostics.Debug.Assert(State == AsyncState.Wait);
            asyncMessage.response = message;
            // Save response TCS here to avoid race where send completes and asyncMessage replaced before we 
            // can return it
            var ret = asyncMessage.replyTcs.Task;
            // Move from wait to send state
            callback(IntPtr.Zero);
            return ret;
        }

        internal void callback(IntPtr arg)
        {
            try
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
                        asyncMessage.requestTcs.SetResult(message);
                        break;
                    case AsyncState.Wait:
                        nng_aio_set_msg(aioHandle, Factory.Borrow(asyncMessage.response));
                        State = AsyncState.Send;
                        nng_ctx_send(ctxHandle, aioHandle);
                        break;
                    case AsyncState.Send:
                        //Console.WriteLine("CB: sent");
                        res = nng_aio_result(aioHandle);
                        if (res != 0)
                        {
                            Console.WriteLine("CB: send failed");
                            Factory.Destroy(ref asyncMessage.response);
                            asyncMessage.replyTcs.TrySetNngError(res);
                        }
                        var currentReq = asyncMessage;
                        init();
                        currentReq.replyTcs.SetResult(true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        void init()
        {
            asyncMessage = new Request<T>();
            State = AsyncState.Recv;
            nng_ctx_recv(ctxHandle, aioHandle);
        }

        private RepAsyncCtx() { }

        Request<T> asyncMessage;
        object sync = new object();
    }

}