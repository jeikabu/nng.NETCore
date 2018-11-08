using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Aio
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        public delegate void AioCallback(IntPtr arg);
        //public delegate void AioCancelFunction(nng_aio aio, IntPtr ptr, Int32 val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_send_aio(nng_socket socket, nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_recv_aio(nng_socket socket, nng_aio aio);



        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_aio_alloc(out nng_aio aio, AioCallback callback, IntPtr arg);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_free(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_stop(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_aio_result(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr nng_aio_count(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_cancel(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_abort(nng_aio aio, int error);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_wait(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_set_msg(nng_aio aio, nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_msg nng_aio_get_msg(nng_aio aio);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_aio_set_input(nng_aio aio, UInt32 index, IntPtr param);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_aio_get_input(nng_aio aio, UInt32 index);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_aio_set_output(nng_aio aio, UInt32 index, IntPtr param);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_aio_get_output(nng_aio aio, UInt32 index);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_aio_set_timeout(nng_aio aio, nng_duration timeout);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_aio_set_iov(nng_aio aio, UInt32 count, nng_iov iov);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern bool nng_aio_begin(nng_aio aio);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern void nng_aio_finish(nng_aio aio, int error);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern void nng_aio_defer(nng_aio aio, AioCancelFunction function, IntPtr data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_sleep_aio(nng_duration duration, nng_aio aio);
    }
}