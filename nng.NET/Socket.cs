using nng.Native;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Msg.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;
    using static nng.Native.Aio.UnsafeNativeMethods;

    /// <summary>
    /// Implementation common to all nng socket types
    /// </summary>
    public abstract class NngSocket : INngSocket
    {
        public nng_socket NativeNngStruct { get; protected set; }
        public int Id => nng_socket_id(NativeNngStruct);

        INngSocket IHasSocket.Socket => this;

        public NngResult<Unit> Dial(string url, Defines.NngFlag flags = default)
            => Unit.OkIfZero(nng_dial(NativeNngStruct, url, flags));
        public NngResult<Unit> Listen(string url, Defines.NngFlag flags = default)
            => Unit.OkIfZero(nng_listen(NativeNngStruct, url, flags));
        public NngResult<INngListener> ListenWithListener(string url, Defines.NngFlag flags = default)
        {
            var res = nng_listen(NativeNngStruct, url, out var listener, flags);
            return NngResult<INngListener>.OkThen(res, () => NngListener.Create(listener));
        }
        public NngResult<INngDialer> DialWithDialer(string url, Defines.NngFlag flags = default)
        {
            var res = nng_dial(NativeNngStruct, url, out var dialer, flags);
            return NngResult<INngDialer>.OkThen(res, () => NngDialer.Create(dialer));
        }
        public NngResult<INngListener> ListenerCreate(string url)
            => NngResult<INngListener>.Ok(NngListener.Create(this, url));
        public NngResult<INngDialer> DialerCreate(string url)
            => NngResult<INngDialer>.Ok(NngDialer.Create(this, url));

        public int GetOpt(string name, out bool data)
            => nng_getopt_bool(NativeNngStruct, name, out data);
        public int GetOpt(string name, out int data)
            => nng_getopt_int(NativeNngStruct, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_getopt_ms(NativeNngStruct, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_getopt_ptr(NativeNngStruct, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_getopt_size(NativeNngStruct, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_getopt_string(NativeNngStruct, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_getopt_uint64(NativeNngStruct, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_setopt(NativeNngStruct, name, data);
        public int SetOpt(string name, bool data)
            => nng_setopt_bool(NativeNngStruct, name, data);
        public int SetOpt(string name, int data)
            => nng_setopt_int(NativeNngStruct, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_setopt_ms(NativeNngStruct, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_setopt_ptr(NativeNngStruct, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_setopt_size(NativeNngStruct, name, data);
        public int SetOpt(string name, string data)
            => nng_setopt_string(NativeNngStruct, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_setopt_uint64(NativeNngStruct, name, data);

        public NngResult<Unit> Send(ReadOnlySpan<byte> message, Defines.NngFlag flags = default)
        {
            unsafe {
                fixed (byte* ptr = &message[0])
                {
                    var res = nng_send(NativeNngStruct, (IntPtr)ptr, (UIntPtr)message.Length, flags);
                    return Unit.OkIfZero(res);
                }
            }
        }

        public NngResult<Unit> SendZeroCopy(INngAlloc message, Defines.NngFlag flags = default)
        {
            // Unconditionally set NNG_FLAG_ALLOC for "zero-copy" send
            flags = flags | Defines.NngFlag.NNG_FLAG_ALLOC;
            var res = Unit.OkIfZero(nng_send(NativeNngStruct, message.Ptr, message.Length, flags));
            if (res.IsOk())
            {
                // If call succeeds, nng takes ownership of message.
                message.Take();
            }
            return res;
        }

        public NngResult<Unit> SendMsg(INngMsg message, Defines.NngFlag flags = default)
        {
            var res = Unit.OkIfZero(nng_sendmsg(NativeNngStruct, message.NativeNngStruct, flags));
            if (res.IsOk())
            {
                // If call succeeds, nng takes ownership of message.
                var _ = message.Take();
            }
            return res;
        }

        public void Send(INngAio aio)
        {
            nng_send_aio(NativeNngStruct, aio.NativeNngStruct);
        }

        public NngResult<UIntPtr> Recv(ref INngAlloc buffer, Defines.NngFlag flags = default)
        {
            if (flags.HasFlag(Defines.NngFlag.NNG_FLAG_ALLOC))
            {
                var res = RecvZeroCopy(flags);
                return res.Into<UIntPtr>(() => res.Ok().Length);
            }
            else
            {
                if (buffer == null || buffer.Length == UIntPtr.Zero)
                    return NngResult<UIntPtr>.Err(Defines.NngErrno.EMSGSIZE);
                var ptr = buffer.Ptr;
                var size = buffer.Length;
                var res = nng_recv(NativeNngStruct, ref ptr, ref size, flags);
                return NngResult<UIntPtr>.OkIfZero(res, size);
            }
        }

        public NngResult<INngAlloc> RecvZeroCopy(Defines.NngFlag flags = default)
        {
            // Unconditionally set NNG_FLAG_ALLOC for "zero-copy" receive
            flags = flags | Defines.NngFlag.NNG_FLAG_ALLOC;
            var ptr = IntPtr.Zero;
            var size = UIntPtr.Zero;
            var res = nng_recv(NativeNngStruct, ref ptr, ref size, flags);
            return NngResult<INngAlloc>.OkThen(res, () => {
                System.Diagnostics.Debug.Assert(ptr != default && size != default);
                return NngAlloc.Create(ptr, size);
            });
        }

        public NngResult<INngMsg> RecvMsg(Defines.NngFlag flags = default)
        {
            nng_msg message;
            var res = nng_recvmsg(NativeNngStruct, out message, flags);
            return NngResult<INngMsg>.OkThen(res, () => new NngMsg(message));
        }

        public void Recv(INngAio aio)
        {
            nng_recv_aio(NativeNngStruct, aio.NativeNngStruct);
        }

        public NngResult<Unit> Notify(Defines.NngPipeEv ev, Defines.PipeEventCallback callback, IntPtr arg)
        {
            return Unit.OkIfZero(nng_pipe_notify(NativeNngStruct, ev, callback, arg));
        }

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
                int _ = nng_close(NativeNngStruct);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}
