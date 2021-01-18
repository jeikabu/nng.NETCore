using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Socket
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern void nng_fini();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_close(nng_socket socket);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_dial(nng_socket socket, string url, out nng_dialer dialer, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern int nng_dial(nng_socket socket, string url, IntPtr not_used, Defines.NngFlag flags);

        public static int nng_dial(nng_socket socket, string url, Defines.NngFlag flags)
        {
            return nng_dial(socket, url, IntPtr.Zero, flags);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_listen(nng_socket socket, string url, out nng_listener listener, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern int nng_listen(nng_socket socket, string url, IntPtr not_used, Defines.NngFlag flags);

        public static int nng_listen(nng_socket socket, string url, Defines.NngFlag flags)
        {
            return nng_listen(socket, url, IntPtr.Zero, flags);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_socket_id(nng_socket socket);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern Int32 nng_closeall();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern Int32 nng_setopt(nng_socket socket, string name, byte[] data, UIntPtr size);

        public static Int32 nng_setopt(nng_socket socket, string name, byte[] data)
        {
            return nng_setopt(socket, name, data, (UIntPtr)data.Length);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_bool(nng_socket socket, string name, bool value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_int(nng_socket socket, string name, Int32 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_ms(nng_socket socket, string name, nng_duration value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_size(nng_socket socket, string name, UIntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_uint64(nng_socket socket, string name, UInt64 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_string(nng_socket socket, string name, string value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_setopt_ptr(nng_socket socket, string name, IntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt(nng_socket socket, string name, out IntPtr data, out UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_bool(nng_socket socket, string name, out bool value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_int(nng_socket socket, string name, out Int32 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_ms(nng_socket socket, string name, out nng_duration value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_size(nng_socket socket, string name, out UIntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_string(nng_socket socket, string name, out IntPtr value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_uint64(nng_socket socket, string name, out UInt64 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_getopt_ptr(nng_socket socket, string name, out IntPtr value);



        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe Int32 nng_send(nng_socket socket, IntPtr data, UIntPtr size, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_recv(nng_socket socket, ref IntPtr data, ref UIntPtr size, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_sendmsg(nng_socket socket, nng_msg message, Defines.NngFlag flags);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_recvmsg(nng_socket socket, out nng_msg message, Defines.NngFlag flags);
    }
}