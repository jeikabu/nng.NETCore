using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Listener.UnsafeNativeMethods;

    /// <summary>
    /// Used to accept a connection from a <see cref="IDialer"/>
    /// </summary>
    public class Listener : IListener
    {
        public nng_listener NngListener { get; protected set; }

        public static IListener Create(ISocket socket, string url)
        {
            int res = nng_listener_create(out var listener, socket.NngSocket, url);
            if (res != 0)
            {
                return null;
            }
            return new Listener { NngListener = listener };
        }

        public static IListener Create(nng_listener listener)
        {
            return new Listener { NngListener = listener };
        }

        public int Start(NngFlag flags)
            => nng_listener_start(NngListener, flags);

        public int GetOpt(string name, out bool data)
            => nng_listener_getopt_bool(NngListener, name, out data);
        public int GetOpt(string name, out int data)
            => nng_listener_getopt_int(NngListener, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_listener_getopt_ms(NngListener, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_listener_getopt_ptr(NngListener, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_listener_getopt_size(NngListener, name, out data);
        public int GetOpt(string name, out nng_sockaddr data)
            => nng_listener_getopt_sockaddr(NngListener, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_listener_getopt_string(NngListener, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_listener_getopt_uint64(NngListener, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_listener_setopt(NngListener, name, data);
        public int SetOpt(string name, bool data)
            => nng_listener_setopt_bool(NngListener, name, data);
        public int SetOpt(string name, int data)
            => nng_listener_setopt_int(NngListener, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_listener_setopt_ms(NngListener, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_listener_setopt_size(NngListener, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_listener_setopt_uint64(NngListener, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_listener_setopt_ptr(NngListener, name, data);
        public int SetOpt(string name, string data)
            => nng_listener_setopt_string(NngListener, name, data);

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
                int _ = nng_listener_close(NngListener);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion

        private Listener() { }
    }
}
