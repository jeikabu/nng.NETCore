using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    public abstract class Socket : ISocket
    {
        public nng_socket NngSocket { get; protected set; }

        public int GetOpt(string name, out bool data)
        {
            return nng_getopt_bool(NngSocket, name, out data);
        }
        public int GetOpt(string name, out int data)
        {
            return nng_getopt_int(NngSocket, name, out data);
        }
        public int GetOpt(string name, out nng_duration data)
        {
            return nng_getopt_ms(NngSocket, name, out data);
        }
        public int GetOpt(string name, out UIntPtr data)
        {
            return nng_getopt_size(NngSocket, name, out data);
        }
        public int GetOpt(string name, out UInt64 data)
        {
            return nng_getopt_uint64(NngSocket, name, out data);
        }
        // public int GetOpt(string name, out string data)
        // {
        //     return nng_getopt_string(NngSocket, name, out data);
        // }
        // public int GetOpt(string name, out void* data)
        // {
        //     return nng_getopt_ptr(NngSocket, name, out data);
        // }

        public int SetOpt(string name, byte[] data)
        {
            return nng_setopt(NngSocket, name, data);
        }
        public int SetOpt(string name, bool data)
        {
            return nng_setopt_bool(NngSocket, name, data);
        }
        public int SetOpt(string name, int data)
        {
            return nng_setopt_int(NngSocket, name, data);
        }
        public int SetOpt(string name, nng_duration data)
        {
            return nng_setopt_ms(NngSocket, name, data);
        }
        public int SetOpt(string name, UIntPtr data)
        {
            return nng_setopt_size(NngSocket, name, data);
        }
        public int SetOpt(string name, UInt64 data)
        {
            return nng_setopt_uint64(NngSocket, name, data);
        }
        public int SetOpt(string name, string data)
        {
            return nng_setopt_string(NngSocket, name, data);
        }
        // public int SetOpt(string name, IntPtr data)
        // {
        //     return nng_setopt_ptr(NngSocket, name, data);
        // }

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
                int _ = nng_close(NngSocket);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}