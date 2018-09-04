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

    public class NngContext
    {
        public bool NngCheck(int error,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (error == 0)
            {
                return false;
            }
            var str = nng_strerror(error);
            Console.WriteLine($"{memberName}:{sourceLineNumber} failed: {str}");
            return true;
        }
    }

    // public interface IFactory
    // {
    //     IReplySocket CreateRep();
    //     IRequestSocket CreateReq();
    // }

    // public class AsyncFactory : IFactory
    // {
    //     public IReplySocket CreateRep()
    //     {

    //     }

    //     public IRequestSocket CreateReq()
    //     {

    //     }
    // }

    public class NngException : Exception
    {
        public NngException(string message)
        : base(message)
        {
        }
        public NngException(int errorCode)
        {
            error = errorCode;
        }

        public override string Message => nng_strerror(error);

        int error = 0;
    }

    interface IAsyncContext
    {
        ISocket Socket { get; }
    }

    public abstract class AsyncCtx : IAsyncContext, IDisposable
    {
        public ISocket Socket { get; private set; }
        protected ISocket socket;
        protected nng_aio aioHandle = nng_aio.Null;
        protected nng_ctx ctxHandle;
        protected enum State
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected State state = State.Init;

        protected int Init(ISocket socket, AioCallback callback)
        {
            Socket = socket;
            var res = nng_aio_alloc(out aioHandle, callback, IntPtr.Zero);
            if (res != 0)
            {
                return res;
            }
            return nng_ctx_open(ref ctxHandle, socket.Socket);
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
                var _ = nng_ctx_close(ctxHandle);
                var __ = nng_aio_free(aioHandle);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }


    public abstract class AsyncNoCtx : IDisposable
    {
        public ISocket Socket { get; private set; }
        protected nng_aio aioHandle = nng_aio.Null;
        protected enum State
        {
            Init,
            Recv,
            Wait,
            Send,
        }
        protected State state = State.Init;

        protected int Init(ISocket socket, AioCallback callback)
        {
            Socket = socket;
            return nng_aio_alloc(out aioHandle, callback, IntPtr.Zero);
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
                nng_aio_free(aioHandle);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}