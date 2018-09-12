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
    using static nng.Native.Socket.UnsafeNativeMethods;

    public abstract class AsyncBase<T> : IAsyncContext
    {
        public IMessageFactory<T> Factory { get; private set; }
        public ISocket Socket { get; private set; }

        protected nng_aio aioHandle = nng_aio.Null;
        public enum AsyncState
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        public AsyncState State { get; protected set; } = AsyncState.Init;
        AioCallback callbackDelegate;

        public void SetTimeout(int msTimeout)
        {
            nng_aio_set_timeout(aioHandle, new nng_duration { TimeMs = msTimeout });
        }
        public void Cancel()
        {
            nng_aio_cancel(aioHandle);
        }

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
                nng_aio_cancel(aioHandle);
                nng_aio_free(aioHandle);
                Socket.Dispose();
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public abstract class AsyncCtx<T> : AsyncBase<T>, ICtx
    {
        public nng_ctx NngCtx { get { return ctxHandle; } }
        protected nng_ctx ctxHandle;

        // public int GetCtxOpt(string name, out Span<byte> data)
        // {
        //     return nng_ctx_getopt(NngCtx, name, out data);
        // }
        public int GetCtxOpt(string name, out bool data)
        {
            return nng_ctx_getopt_bool(NngCtx, name, out data);
        }
        public int GetCtxOpt(string name, out int data)
        {
            return nng_ctx_getopt_int(NngCtx, name, out data);
        }
        public int GetCtxOpt(string name, out nng_duration data)
        {
            return nng_ctx_getopt_ms(NngCtx, name, out data);
        }
        public int GetCtxOpt(string name, out UIntPtr data)
        {
            return nng_ctx_getopt_size(NngCtx, name, out data);
        }
        // public int GetCtxOpt(string name, out string data)
        // {
        //     return nng_ctx_getopt_string(NngCtx, name, out data);
        // }
        // public int GetCtxOpt(string name, out UInt64 data)
        // {
        //     return nng_ctx_getopt_uint64(NngCtx, name, out data);
        // }
        
        public int SetCtxOpt(string name, byte[] data)
        {
            return nng_ctx_setopt(NngCtx, name, data);
        }
        public int SetCtxOpt(string name, bool data)
        {
            return nng_ctx_setopt_bool(NngCtx, name, data);
        }
        public int SetCtxOpt(string name, int data)
        {
            return nng_ctx_setopt_int(NngCtx, name, data);
        }
        public int SetCtxOpt(string name, nng_duration data)
        {
            return nng_ctx_setopt_ms(NngCtx, name, data);
        }
        public int SetCtxOpt(string name, UIntPtr data)
        {
            return nng_ctx_setopt_size(NngCtx, name, data);
        }
        // public int SetCtxOpt(string name, string data)
        // {
        //     return nng_ctx_setopt_string(NngCtx, name, data);
        // }
        // public int SetCtxOpt(string name, UInt64 data)
        // {
        //     return nng_ctx_setopt_uint64(NngCtx, name, data);
        // }

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