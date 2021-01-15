using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Stream
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        #region nng_stream
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_free(nng_stream stream);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_close(nng_stream stream);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_send(nng_stream stream, nng_aio aio);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_recv(nng_stream stream, nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set(nng_stream stream, string name, byte[] data, UIntPtr size);
        public static Int32 nng_stream_set(nng_stream stream, string name, byte[] data)
        {
            return nng_stream_set(stream, name, data, (UIntPtr)data.Length);
        }
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_get(nng_stream dialer, string name, void *, size_t *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_bool(nng_stream dialer, string name, out bool data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_int(nng_stream dialer, string name, out Int32 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_ms(nng_stream dialer, string name, out nng_duration data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_size(nng_stream dialer, string name, out UIntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_uint64(nng_stream dialer, string name, out UInt64 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_string(nng_stream dialer, string name, out IntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_get_ptr(nng_stream dialer, string name, out IntPtr data);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_get_addr(nng_stream dialer, string name, nng_sockaddr *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_bool(nng_stream dialer, string name, bool value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_int(nng_stream dialer, string name, Int32 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_ms(nng_stream dialer, string name, nng_duration value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_size(nng_stream dialer, string name, UIntPtr value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_uint64(nng_stream dialer, string name, UInt64 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_string(nng_stream dialer, string name, string value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_set_ptr(nng_stream dialer, string name, IntPtr value);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_set_addr(nng_stream dialer, string name, const nng_sockaddr *);

        #endregion

        #region nng_stream_dialer
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_alloc(ref nng_stream_dialer dialer, string addr);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_dialer_alloc_url(ref nng_stream_dialer dialer, const nng_url *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_dialer_free(nng_stream_dialer dialer);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_dialer_close(nng_stream_dialer dialer);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_dialer_dial(nng_stream_dialer dialer, nng_aio aio);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32  nng_stream_dialer_set(nng_stream_dialer dialer, string name, byte[] data, UIntPtr size);
        public static Int32 nng_stream_dialer_set(nng_stream_dialer dialer, string name, byte[] data)
        {
            return nng_stream_dialer_set(dialer, name, data, (UIntPtr)data.Length);
        }
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_dialer_get(nng_stream_dialer dialer, string name, void *, size_t *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_bool(nng_stream_dialer dialer, string name, out bool data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_int(nng_stream_dialer dialer, string name, out Int32 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_ms(nng_stream_dialer dialer, string name, out nng_duration data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_size(nng_stream_dialer dialer, string name, out UIntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_uint64(nng_stream_dialer dialer, string name, out UInt64 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_string(nng_stream_dialer dialer, string name, out IntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_get_ptr(nng_stream_dialer dialer, string name, out IntPtr data);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_dialer_get_addr(nng_stream_dialer dialer, string name, nng_sockaddr *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_bool(nng_stream_dialer dialer, string name, bool value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_int(nng_stream_dialer dialer, string name, Int32 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_ms(nng_stream_dialer dialer, string name, nng_duration value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_size(nng_stream_dialer dialer, string name, UIntPtr value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_uint64(nng_stream_dialer dialer, string name, UInt64 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_string(nng_stream_dialer dialer, string name, string value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_dialer_set_ptr(nng_stream_dialer dialer, string name, IntPtr value);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_dialer_set_addr(nng_stream_dialer dialer, string name, const nng_sockaddr *);
        #endregion

        #region nng_stream_listener
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_alloc(ref nng_stream_listener listener, string url);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_listener_alloc_url(ref nng_stream_listener listener, const nng_url *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_listener_free(nng_stream_listener listener);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_listener_close(nng_stream_listener listener);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32  nng_stream_listener_listen(nng_stream_listener listener);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stream_listener_accept(nng_stream_listener listener, nng_aio aio);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32  nng_stream_listener_set(nng_stream_listener listener, string name, byte[] data, UIntPtr size);
        public static Int32 nng_stream_listener_set(nng_stream_listener listener, string name, byte[] data)
        {
            return nng_stream_listener_set(listener, name, data, (UIntPtr)data.Length);
        }
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_listener_get(nng_stream_listener listener, string name, void *, size_t *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_bool(nng_stream_listener listener, string name, out bool data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_int(nng_stream_listener listener, string name, out Int32 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_ms(nng_stream_listener listener, string name, out nng_duration data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_size(nng_stream_listener listener, string name, out UIntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_uint64(nng_stream_listener listener, string name, out UInt64 data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_string(nng_stream_listener listener, string name, out IntPtr data);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_get_ptr(nng_stream_listener listener, string name, out IntPtr data);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_listener_get_addr(nng_stream_listener listener, string name, nng_sockaddr *);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_bool(nng_stream_listener listener, string name, bool value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_int(nng_stream_listener listener, string name, Int32 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_ms(nng_stream_listener listener, string name, nng_duration value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_size(nng_stream_listener listener, string name, UIntPtr value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_uint64(nng_stream_listener listener, string name, UInt64 value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_string(nng_stream_listener listener, string name, string value);
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_stream_listener_set_ptr(nng_stream_listener listener, string name, IntPtr value);
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_stream_listener_set_addr(nng_stream_listener listener, string name, const nng_sockaddr * value);
        #endregion
    }
}