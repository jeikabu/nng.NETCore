using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Dialer
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_create(out nng_dialer dialer, nng_socket socket, string url);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_start(nng_dialer dialer, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_close(nng_dialer dialer);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_id(nng_dialer dialer);


        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt(nng_dialer dialer, string name, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_bool(nng_dialer dialer, string name, out bool data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_int(nng_dialer dialer, string name, out Int32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_ms(nng_dialer dialer, string name, out nng_duration data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_ptr(nng_dialer dialer, string name, out IntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_size(nng_dialer dialer, string name, out UIntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_string(nng_dialer dialer, string name, out IntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_getopt_uint64(nng_dialer dialer, string name, out UInt64 data);


        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern int nng_dialer_setopt(nng_dialer dialer, string name, byte[] data, UIntPtr size);

        public static int nng_dialer_setopt(nng_dialer dialer, string name, byte[] data)
        {
            return nng_dialer_setopt(dialer, name, data, (UIntPtr)data.Length);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_bool(nng_dialer dialer, string name, bool value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_int(nng_dialer dialer, string name, Int32 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_ms(nng_dialer dialer, string name, nng_duration value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_ptr(nng_dialer dialer, string name, IntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_size(nng_dialer dialer, string name, UIntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_string(nng_dialer dialer, string name, string value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dialer_setopt_uint64(nng_dialer dialer, string name, UInt64 value);
    }
}