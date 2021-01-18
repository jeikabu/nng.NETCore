using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Dialer.UnsafeNativeMethods;

    /// <summary>
    /// Used to initiate a connection to a <see cref="INngListener"/>
    /// </summary>
    public class NngDialer : INngDialer
    {
        public nng_dialer NativeNngStruct { get; protected set; }
        public int Id => nng_dialer_id(NativeNngStruct);

        public static INngDialer Create(INngSocket socket, string url)
        {
            int res = nng_dialer_create(out var dialer, socket.NativeNngStruct, url);
            if (res != 0)
            {
                return null;
            }
            return new NngDialer { NativeNngStruct = dialer };
        }

        public static INngDialer Create(nng_dialer dialer)
        {
            return new NngDialer { NativeNngStruct = dialer };
        }

        public int Start(NngFlag flags)
            => nng_dialer_start(NativeNngStruct, flags);

        public int GetOpt(string name, out bool data)
            => nng_dialer_getopt_bool(NativeNngStruct, name, out data);
        public int GetOpt(string name, out int data)
            => nng_dialer_getopt_int(NativeNngStruct, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_dialer_getopt_ms(NativeNngStruct, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_dialer_getopt_ptr(NativeNngStruct, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_dialer_getopt_size(NativeNngStruct, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_dialer_getopt_string(NativeNngStruct, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_dialer_getopt_uint64(NativeNngStruct, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_dialer_setopt(NativeNngStruct, name, data);
        public int SetOpt(string name, bool data)
            => nng_dialer_setopt_bool(NativeNngStruct, name, data);
        public int SetOpt(string name, int data)
            => nng_dialer_setopt_int(NativeNngStruct, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_dialer_setopt_ms(NativeNngStruct, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_dialer_setopt_ptr(NativeNngStruct, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_dialer_setopt_size(NativeNngStruct, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_dialer_setopt_uint64(NativeNngStruct, name, data);
        public int SetOpt(string name, string data)
            => nng_dialer_setopt_string(NativeNngStruct, name, data);

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
                int _ = nng_dialer_close(NativeNngStruct);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion

        private NngDialer() { }
    }
}
