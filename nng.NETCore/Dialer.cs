using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Dialer.UnsafeNativeMethods;

    /// <summary>
    /// Used to initiate a connection to a <see cref="IListener"/>
    /// </summary>
    public class Dialer : IDialer
    {
        public nng_dialer NngDialer { get; protected set; }

        public static IDialer Create(ISocket socket, string url)
        {
            int res = nng_dialer_create(out var dialer, socket.NngSocket, url);
            if (res != 0)
            {
                return null;
            }
            return new Dialer { NngDialer = dialer };
        }

        public static IDialer Create(nng_dialer dialer)
        {
            return new Dialer { NngDialer = dialer };
        }

        public int Start(NngFlag flags)
            => nng_dialer_start(NngDialer, flags);

        public int GetOpt(string name, out bool data)
            => nng_dialer_getopt_bool(NngDialer, name, out data);
        public int GetOpt(string name, out int data)
            => nng_dialer_getopt_int(NngDialer, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_dialer_getopt_ms(NngDialer, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_dialer_getopt_ptr(NngDialer, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_dialer_getopt_size(NngDialer, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_dialer_getopt_string(NngDialer, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_dialer_getopt_uint64(NngDialer, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_dialer_setopt(NngDialer, name, data);
        public int SetOpt(string name, bool data)
            => nng_dialer_setopt_bool(NngDialer, name, data);
        public int SetOpt(string name, int data)
            => nng_dialer_setopt_int(NngDialer, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_dialer_setopt_ms(NngDialer, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_dialer_setopt_ptr(NngDialer, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_dialer_setopt_size(NngDialer, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_dialer_setopt_uint64(NngDialer, name, data);
        public int SetOpt(string name, string data)
            => nng_dialer_setopt_string(NngDialer, name, data);

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
                int _ = nng_dialer_close(NngDialer);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion

        private Dialer() { }
    }
}
