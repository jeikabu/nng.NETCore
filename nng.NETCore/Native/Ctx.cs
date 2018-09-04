using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Ctx
{
    using static Globals;
    
    [System.Security.SuppressUnmanagedCodeSecurity]
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_ctx_open(ref nng_ctx ctx, nng_socket socket);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_ctx_close(nng_ctx ctx);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_ctx_id(nng_ctx ctx);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_ctx_recv(nng_ctx ctx, nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_ctx_send(nng_ctx ctx, nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_getopt(nng_ctx ctx, string name, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_getopt_bool(nng_ctx ctx, string name, out bool data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_getopt_int(nng_ctx ctx, string name, out Int32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_getopt_ms(nng_ctx ctx, string name, out nng_duration data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_getopt_size(nng_ctx ctx, string name, out UIntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_setopt(nng_ctx ctx, string name, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_setopt_bool(nng_ctx ctx, string name, bool value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_setopt_int(nng_ctx ctx, string name, Int32 value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_setopt_ms(nng_ctx ctx, string name, nng_duration value);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_ctx_setopt_size(nng_ctx ctx, string name, UIntPtr value);
    }
}