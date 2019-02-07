using nng.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    public abstract class AsyncBase<T> : IAsyncContext
    {
        public IMessageFactory<T> Factory { get; protected set; }
        public ISocket Socket { get; protected set; }

        protected nng_aio aioHandle = nng_aio.Null;
        protected enum AsyncState
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected AsyncState State { get; set; } = AsyncState.Init;

        public void SetTimeout(int msTimeout)
        {
            nng_aio_set_timeout(aioHandle, new nng_duration { TimeMs = msTimeout });
        }
        public void Cancel()
        {
            nng_aio_cancel(aioHandle);
        }

        protected int InitAio()
        {
            // Make a copy to ensure an auto-matically created delegate doesn't get GC'd while native code 
            // is still using it:
            // https://stackoverflow.com/questions/6193711/call-has-been-made-on-garbage-collected-delegate-in-c
            aioCallback = new AioCallback(callback);
            var res = nng_aio_alloc(out aioHandle, aioCallback, IntPtr.Zero);
            if (res == 0)
            {
                // FIXME: TODO: callbacks still getting GC'd.  Maybe I need to move nng_aio_free() out of Dispose()?
                var added = callbacks.TryAdd(aioCallback, true);
                System.Diagnostics.Debug.Assert(added);
            }
            return res;
        }

        protected abstract void AioCallback(IntPtr argument);
        void callback(IntPtr argument)
        {
            try
            {
                lock (sync)
                {
                    AioCallback(argument);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        protected void CheckState()
        {
            if (State != AsyncState.Init)
            {
                throw new InvalidOperationException();
            }
        }

        protected void HandleFailedSend()
        {
            var unsentMsg = nng_aio_get_msg(aioHandle);
            nng_msg_free(unsentMsg);
        }

        // Synchronization object used for aio callbacks
        protected object sync = new object();
        AioCallback aioCallback;
        // FIXME: TODO: callbacks still getting GC'd
        static ConcurrentDictionary<AioCallback, bool> callbacks = new ConcurrentDictionary<AioCallback, bool>();

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                nng_aio_cancel(aioHandle);
                nng_aio_free(aioHandle);
                var removed = callbacks.TryRemove(aioCallback, out var _);
                System.Diagnostics.Debug.Assert(removed);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public class AsyncCtx : INngCtx, IDisposable
    {
        public nng_ctx NngCtx { get; protected set; }

        public static NngResult<INngCtx> Create(ISocket socket)
        {
            var res = nng_ctx_open(out var ctx, socket.NngSocket);
            if (res != 0)
                return NngResult<INngCtx>.Fail(res);
            return NngResult<INngCtx>.Ok(new AsyncCtx { NngCtx = ctx });
        }
        private AsyncCtx() { }

        // public int GetOpt(string name, out Span<byte> data)
        // {
        //     return nng_ctx_getopt(NngCtx, name, out data);
        // }
        public int GetOpt(string name, out bool data) 
            => nng_ctx_getopt_bool(NngCtx, name, out data);
        public int GetOpt(string name, out int data)
            => nng_ctx_getopt_int(NngCtx, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_ctx_getopt_ms(NngCtx, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_ctx_getopt_ptr(NngCtx, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_ctx_getopt_size(NngCtx, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_ctx_getopt_string(NngCtx, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_ctx_getopt_uint64(NngCtx, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_ctx_setopt(NngCtx, name, data);
        public int SetOpt(string name, bool data)
            => nng_ctx_setopt_bool(NngCtx, name, data);
        public int SetOpt(string name, int data)
            => nng_ctx_setopt_int(NngCtx, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_ctx_setopt_ms(NngCtx, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_ctx_setopt_ptr(NngCtx, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_ctx_setopt_size(NngCtx, name, data);
        public int SetOpt(string name, string data)
            => nng_ctx_setopt_string(NngCtx, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_ctx_setopt_uint64(NngCtx, name, data);

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                var _ = nng_ctx_close(NngCtx);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

}