using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Pinvoke
{
    using static Globals;

    public sealed class Listener
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_create(out nng_listener listener, nng_socket socket, string url);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_start(nng_listener listener, Int32 flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_close(nng_listener listener);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_id(nng_listener listener);


        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_getopt(nng_listener listener, string name, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_getopt_bool(nng_listener listener, string name, out bool data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_getopt_int(nng_listener listener, string name, out Int32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_getopt_ms(nng_listener listener, string name, out nng_duration data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_getopt_size(nng_listener listener, string name, out UIntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_setopt(nng_listener listener, string name, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_setopt_bool(nng_listener listener, string name, bool value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_setopt_int(nng_listener listener, string name, Int32 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_setopt_ms(nng_listener listener, string name, nng_duration value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listener_setopt_size(nng_listener listener, string name, UIntPtr value);
    }
}