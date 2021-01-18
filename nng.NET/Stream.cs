using nng.Native;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Stream.UnsafeNativeMethods;

    public class StreamListener : INngStreamListener
    {
        nng_stream_listener listener;
        public nng_stream_listener NngStreamListener => listener;

        public static NngResult<INngStreamListener> Alloc(string addr)
        {
            var listener = nng_stream_listener.Null;
            var res = nng_stream_listener_alloc(ref listener, addr);
            return NngResult<INngStreamListener>.OkThen(res, () => new StreamListener() { listener = listener});
        }
        private StreamListener(){}

        public NngResult<Unit> Listen() => Unit.OkIfZero(nng_stream_listener_listen(listener));
        public void Accept(INngAio aio) => nng_stream_listener_accept(listener, aio.NativeNngStruct);
        public void Close() => nng_stream_listener_close(listener);

        #region IOptions
        public int GetOpt(string name, out bool data)
            => nng_stream_listener_get_bool(NngStreamListener, name, out data);
        public int GetOpt(string name, out int data)
            => nng_stream_listener_get_int(NngStreamListener, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_stream_listener_get_ms(NngStreamListener, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_stream_listener_get_ptr(NngStreamListener, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_stream_listener_get_size(NngStreamListener, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_stream_listener_get_string(NngStreamListener, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_stream_listener_get_uint64(NngStreamListener, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_stream_listener_set(NngStreamListener, name, data);
        public int SetOpt(string name, bool data)
            => nng_stream_listener_set_bool(NngStreamListener, name, data);
        public int SetOpt(string name, int data)
            => nng_stream_listener_set_int(NngStreamListener, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_stream_listener_set_ms(NngStreamListener, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_stream_listener_set_ptr(NngStreamListener, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_stream_listener_set_size(NngStreamListener, name, data);
        public int SetOpt(string name, string data)
            => nng_stream_listener_set_string(NngStreamListener, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_stream_listener_set_uint64(NngStreamListener, name, data);
        #endregion

        ~StreamListener() => Dispose(false);

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
            }
            
            nng_stream_listener_free(listener);
            listener = nng_stream_listener.Null;
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public class StreamDialer : INngStreamDialer
    {
        nng_stream_dialer dialer;
        public nng_stream_dialer NngStreamDialer => dialer;

        public static NngResult<INngStreamDialer> Alloc(string addr)
        {
            var dialer = nng_stream_dialer.Null;
            var res = nng_stream_dialer_alloc(ref dialer, addr);
            return NngResult<INngStreamDialer>.OkThen(res, () => new StreamDialer() { dialer = dialer });
        }
        private StreamDialer(){}

        public void Dial(INngAio aio) => nng_stream_dialer_dial(dialer, aio.NativeNngStruct);
        public void Close() => nng_stream_dialer_close(dialer);

        #region IOptions
        public int GetOpt(string name, out bool data)
            => nng_stream_dialer_get_bool(NngStreamDialer, name, out data);
        public int GetOpt(string name, out int data)
            => nng_stream_dialer_get_int(NngStreamDialer, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_stream_dialer_get_ms(NngStreamDialer, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_stream_dialer_get_ptr(NngStreamDialer, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_stream_dialer_get_size(NngStreamDialer, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_stream_dialer_get_string(NngStreamDialer, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_stream_dialer_get_uint64(NngStreamDialer, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_stream_dialer_set(NngStreamDialer, name, data);
        public int SetOpt(string name, bool data)
            => nng_stream_dialer_set_bool(NngStreamDialer, name, data);
        public int SetOpt(string name, int data)
            => nng_stream_dialer_set_int(NngStreamDialer, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_stream_dialer_set_ms(NngStreamDialer, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_stream_dialer_set_ptr(NngStreamDialer, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_stream_dialer_set_size(NngStreamDialer, name, data);
        public int SetOpt(string name, string data)
            => nng_stream_dialer_set_string(NngStreamDialer, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_stream_dialer_set_uint64(NngStreamDialer, name, data);
        #endregion

        ~StreamDialer() => Dispose(false);

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
            }
            
            nng_stream_dialer_free(dialer);
            dialer = nng_stream_dialer.Null;
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }

    public class Stream : INngStream
    {
        nng_stream stream;
        public nng_stream NngStream => stream;

        public static NngResult<INngStream> From(INngAio aio)
        {
            var ptr = aio.GetOutput(0);
            if (ptr == IntPtr.Zero)
            {
                return NngResult<INngStream>.Fail(-1);
            }
            else
            {
                var stream = new Stream{stream = new nng_stream(ptr)};
                return NngResult<INngStream>.Ok(stream);
            }
        }

        private Stream(){}

        public void Send(INngAio aio) => nng_stream_send(stream, aio.NativeNngStruct);

        public void Recv(INngAio aio) => nng_stream_recv(stream, aio.NativeNngStruct);

        public void Close() => nng_stream_close(stream);

        #region IOptions
        public int GetOpt(string name, out bool data)
            => nng_stream_get_bool(NngStream, name, out data);
        public int GetOpt(string name, out int data)
            => nng_stream_get_int(NngStream, name, out data);
        public int GetOpt(string name, out nng_duration data)
            => nng_stream_get_ms(NngStream, name, out data);
        public int GetOpt(string name, out IntPtr data)
            => nng_stream_get_ptr(NngStream, name, out data);
        public int GetOpt(string name, out UIntPtr data)
            => nng_stream_get_size(NngStream, name, out data);
        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_stream_get_string(NngStream, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }
        public int GetOpt(string name, out UInt64 data)
            => nng_stream_get_uint64(NngStream, name, out data);

        public int SetOpt(string name, byte[] data)
            => nng_stream_set(NngStream, name, data);
        public int SetOpt(string name, bool data)
            => nng_stream_set_bool(NngStream, name, data);
        public int SetOpt(string name, int data)
            => nng_stream_set_int(NngStream, name, data);
        public int SetOpt(string name, nng_duration data)
            => nng_stream_set_ms(NngStream, name, data);
        public int SetOpt(string name, IntPtr data)
            => nng_stream_set_ptr(NngStream, name, data);
        public int SetOpt(string name, UIntPtr data)
            => nng_stream_set_size(NngStream, name, data);
        public int SetOpt(string name, string data)
            => nng_stream_set_string(NngStream, name, data);
        public int SetOpt(string name, UInt64 data)
            => nng_stream_set_uint64(NngStream, name, data);
        #endregion

        ~Stream() => Dispose(false);

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
            }
            
            nng_stream_free(stream);
            stream = nng_stream.Null;
            disposed = true;
        }
        bool disposed = false;
        #endregion
    }
}
