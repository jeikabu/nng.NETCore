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

    public abstract class AsyncBase : IAsyncContext, IDisposable
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
        AioCallback callbackDelegate;

        protected int Init(ISocket socket, AioCallback callback)
        {
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

    public abstract class AsyncCtx : AsyncBase
    {
        protected nng_ctx ctxHandle;

        protected new int Init(ISocket socket, AioCallback callback)
        {
            var res = base.Init(socket, callback);
            if (res != 0)
            {
                return res;
            }
            return nng_ctx_open(ref ctxHandle, socket.Socket);
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