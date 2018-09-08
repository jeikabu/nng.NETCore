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

    public abstract class AsyncBase<T> : IAsyncContext
    {
        public IMessageFactory<T> Factory { get; private set; }
        public ISocket Socket { get; private set; }
        public nng_socket NngSocket => Socket.NngSocket;

        protected nng_aio aioHandle = nng_aio.Null;
        protected enum State
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected State state = State.Init;
        AioCallback callbackDelegate;

        internal int Init(IMessageFactory<T> factory, ISocket socket, AioCallback callback)
        {
            Factory = factory;
            Socket = socket;
            // Make a copy to ensure an auto-matically created delegate doesn't get GC'd while native code 
            // is still using it:
            // https://stackoverflow.com/questions/6193711/call-has-been-made-on-garbage-collected-delegate-in-c
            callbackDelegate = new AioCallback(callback);
            return nng_aio_alloc(out aioHandle, callbackDelegate, IntPtr.Zero);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                var __ = nng_aio_free(aioHandle);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public abstract class AsyncCtx<T> : AsyncBase<T>
    {
        protected nng_ctx ctxHandle;

        internal new int Init(IMessageFactory<T> factory, ISocket socket, AioCallback callback)
        {
            var res = base.Init(factory, socket, callback);
            if (res != 0)
            {
                return res;
            }
            return nng_ctx_open(ref ctxHandle, socket.NngSocket);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                var _ = nng_ctx_close(ctxHandle);
            }
            disposed = true;
            base.Dispose(disposing);
        }
        bool disposed = false;
    }

}