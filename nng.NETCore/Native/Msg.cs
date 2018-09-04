using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Msg
{
    using static Globals;
    
    [System.Security.SuppressUnmanagedCodeSecurity]
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_msg_alloc(out nng_msg message, UIntPtr size);
        
        public static int nng_msg_alloc(out nng_msg message, uint size)
        {
            return nng_msg_alloc(out message, (UIntPtr)size);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_msg_free(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_msg_realloc(ref nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_msg_header(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr nng_msg_header_len(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_msg_body(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr nng_msg_len(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_append(nng_msg message, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_insert(nng_msg message, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_trim(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_chop(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_append(nng_msg message, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_insert(nng_msg message, IntPtr data, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_trim(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_chop(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_append_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_insert_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_chop_u32(nng_msg message, ref UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_trim_u32(nng_msg message, ref UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_append_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_insert_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_chop_u32(nng_msg message, ref UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_trim_u32(nng_msg message, ref UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_dup(out nng_msg dest, nng_msg source);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_msg_clear(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_msg_header_clear(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_msg_set_pipe(nng_msg message, nng_pipe pipe);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_pipe nng_msg_get_pipe(nng_msg message);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // public static extern int nng_msg_getopt(nng_msg message, Int32, IntPtr, UIntPtr);
    }
}