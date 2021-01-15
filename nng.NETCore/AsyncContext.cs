using nng.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    /// <summary>
    /// High-level wrapper around `nng_aio`.
    /// </summary>
    public class AsyncIO : INngAio
    {
        public static NngResult<INngAio> Create(AioCallback callback)
        {
            // Make a copy to ensure an automatically created delegate doesn't get GC'd while native code 
            // is still using it:
            // https://stackoverflow.com/questions/6193711/call-has-been-made-on-garbage-collected-delegate-in-c
            // https://docs.microsoft.com/en-us/cpp/dotnet/how-to-marshal-callbacks-and-delegates-by-using-cpp-interop
            var res = nng_aio_alloc(out var aioHandle, callback, IntPtr.Zero);
            return NngResult<INngAio>.OkThen(res, () => new AsyncIO {aioHandle = aioHandle, gcHandle = GCHandle.Alloc(callback) });
        }

        public nng_aio NngAio => aioHandle;

        public void SetMsg(nng_msg msg)
        {
            nng_aio_set_msg(aioHandle, msg);
        }

        public NngResult<Unit> SetIov(Span<nng_iov> iov)
        {
            if (iov.Length > Defines.MAX_IOV_BUFFERS)
                return Unit.Err(Defines.NngErrno.EINVAL);
            return Unit.OkIfZero(nng_aio_set_iov(aioHandle, iov.ToArray()));
        }

        public void SetTimeout(int msTimeout)
        {
            SetTimeout(new nng_duration { TimeMs = msTimeout });
        }

        public void SetTimeout(nng_duration timeout)
        {
            nng_aio_set_timeout(aioHandle, timeout);
        }

        public UIntPtr Count() => nng_aio_count(aioHandle);

        public NngResult<Unit> GetResult()
        {
            return Unit.OkIfZero(nng_aio_result(aioHandle));
        }

        public nng_msg GetMsg()
        {
            return nng_aio_get_msg(aioHandle);
        }

        public IntPtr GetOutput(UInt32 index)
        {
            return nng_aio_get_output(aioHandle, index);
        }

        public void Wait()
        {
            nng_aio_wait(aioHandle);
        }

        public void Cancel()
        {
            nng_aio_cancel(aioHandle);
        }

        ~AsyncIO() => Dispose(false);

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
                nng_aio_stop(aioHandle);
            }
            
            nng_aio_free(aioHandle);
            aioHandle = nng_aio.Null;
            gcHandle.Free();
            disposed = true;
        }
        bool disposed = false;
        #endregion

        protected nng_aio aioHandle = nng_aio.Null;
        GCHandle gcHandle;
    }

    public abstract class AsyncBase<T> : IAsyncContext
    {
        public IMessageFactory<T> Factory { get; protected set; }
        public ISocket Socket { get; protected set; }

        protected enum AsyncState
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected AsyncState State { get; set; } = AsyncState.Init;

        public void SetTimeout(int msTimeout) => Aio.SetTimeout(msTimeout);
        public void Cancel() => Aio.Cancel();

        public INngAio Aio { get; protected set; }

        protected NngResult<Unit> InitAio()
        {
            var res = AsyncIO.Create(callback);
            if (res.IsOk())
            {
                Aio = res.Ok();
                return Unit.Ok;
            }
            else
            {
                return Unit.Err(res.Err());
            }
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
            var _unsentMsg = Aio.GetMsg();
        }

        // Synchronization object used for aio callbacks
        protected object sync = new object();

        #region IDisposable
        public void Dispose()
        {
            Aio?.Dispose();
        }
        #endregion
    }

    /// <summary>
    /// High-level wrapper around `nng_ctx`.
    /// </summary>
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

        public void Send(INngAio aio) =>
            nng_ctx_send(NngCtx, aio.NngAio);
        public void Recv(INngAio aio) =>
            nng_ctx_recv(NngCtx, aio.NngAio);

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