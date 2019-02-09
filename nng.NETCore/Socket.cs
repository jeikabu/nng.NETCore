using nng.Native;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Socket.UnsafeNativeMethods;

    /// <summary>
    /// Implementation common to all nng socket types
    /// </summary>
    public abstract class Socket : ISocket
    {
        public nng_socket NngSocket { get; protected set; }

        public NngResult<Unit> Dial(string url, Defines.NngFlag flags = default)
            => Unit.OkIfZero(nng_dial(NngSocket, url, flags));
        public NngResult<Unit> Listen(string url, Defines.NngFlag flags = default)
            => Unit.OkIfZero(nng_listen(NngSocket, url, flags));
        public NngResult<IListener> ListenWithListener(string url, Defines.NngFlag flags = default)
        {
            var res = nng_listen(NngSocket, url, out var listener, flags);
            return NngResult<IListener>.OkThen(res, () => Listener.Create(listener));
        }
        public NngResult<IDialer> DialWithDialer(string url, Defines.NngFlag flags = default)
        {
            var res = nng_dial(NngSocket, url, out var dialer, flags);
            return NngResult<IDialer>.OkThen(res, () => Dialer.Create(dialer));
        }
        public NngResult<IListener> ListenerCreate(string url)
            => NngResult<IListener>.Ok(Listener.Create(this, url));
        public NngResult<IDialer> DialerCreate(string url)
            => NngResult<IDialer>.Ok(Dialer.Create(this, url));

        public int GetOpt(string name, out bool data)
            => nng_getopt_bool(NngSocket, name, out data);
        public int GetOpt(string name, out int data)
            => nng_getopt_int(NngSocket, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_getopt_ms(NngSocket, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_getopt_ptr(NngSocket, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_getopt_size(NngSocket, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_getopt_string(NngSocket, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_getopt_uint64(NngSocket, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_setopt(NngSocket, name, data);
        public int SetOpt(string name, bool data)
            => nng_setopt_bool(NngSocket, name, data);
        public int SetOpt(string name, int data)
            => nng_setopt_int(NngSocket, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_setopt_ms(NngSocket, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_setopt_ptr(NngSocket, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_setopt_size(NngSocket, name, data);
        public int SetOpt(string name, string data)
            => nng_setopt_string(NngSocket, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_setopt_uint64(NngSocket, name, data);

        public NngResult<Unit> Send(ReadOnlySpan<byte> message, Defines.NngFlag flags = default)
        {
            unsafe {
                fixed (byte* ptr = &message[0])
                {
                    var res = nng_send(NngSocket, (IntPtr)ptr, (UIntPtr)message.Length, flags);
                    return Unit.OkIfZero(res);
                }
            }
        }

        public NngResult<Unit> SendZeroCopy(IMemory message, Defines.NngFlag flags = default)
        {
            // Unconditionally set NNG_FLAG_ALLOC for "zero-copy" send
            flags = flags | Defines.NngFlag.NNG_FLAG_ALLOC;
            var res = nng_send(NngSocket, message.Ptr, message.Length, flags);
            return Unit.OkIfZero(res);
        }

        public NngResult<Unit> SendMsg(IMessage message, Defines.NngFlag flags = default)
        {
            return Unit.OkIfZero(nng_sendmsg(NngSocket, message.NngMsg, flags));
        }

        public NngResult<UIntPtr> Recv(ref IMemory buffer, Defines.NngFlag flags = default)
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
                var res = nng_recv(NngSocket, ref ptr, ref size, flags);
                return NngResult<UIntPtr>.OkIfZero(res, size);
            }
        }

        public NngResult<IMemory> RecvZeroCopy(Defines.NngFlag flags = default)
        {
            // Unconditionally set NNG_FLAG_ALLOC for "zero-copy" receive
            flags = flags | Defines.NngFlag.NNG_FLAG_ALLOC;
            var ptr = IntPtr.Zero;
            var size = UIntPtr.Zero;
            var res = nng_recv(NngSocket, ref ptr, ref size, flags);
            return NngResult<IMemory>.OkThen(res, () => {
                System.Diagnostics.Debug.Assert(ptr != default && size != default);
                return Alloc.Create(ptr, size);
            });
        }

        public NngResult<IMessage> RecvMsg(Defines.NngFlag flags = default)
        {
            nng_msg message;
            var res = nng_recvmsg(NngSocket, out message, flags);
            return NngResult<IMessage>.OkThen(res, () => new Message(message));
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
                int _ = nng_close(NngSocket);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}