using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Msg
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_msg_alloc(out nng_msg message, UIntPtr size);

        public static int nng_msg_alloc(out nng_msg message, uint size = 0)
        {
            return nng_msg_alloc(out message, (UIntPtr)size);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_msg_free(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_msg_realloc(ref nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe void* nng_msg_header(nng_msg message);

        public static Span<byte> nng_msg_header_span(nng_msg message)
        {
            unsafe
            {
                return new Span<byte>(nng_msg_header(message), (int)nng_msg_header_len(message));
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr nng_msg_header_len(nng_msg message);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe void* nng_msg_body(nng_msg message);

        public static Span<byte> nng_msg_body_span(nng_msg message)
        {
            unsafe
            {
                return new Span<byte>(nng_msg_body(message), (int)nng_msg_len(message));
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr nng_msg_len(nng_msg message);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // static extern Int32 nng_msg_append(nng_msg message, byte[] data, UIntPtr size);

        // public static Int32 nng_msg_append(nng_msg message, byte[] data)
        // {
        //     return nng_msg_append(message, data, (UIntPtr)data.Length);
        // }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe Int32 nng_msg_append(nng_msg message, byte* data, UIntPtr size);

        public static Int32 nng_msg_append(nng_msg message, ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    return nng_msg_append(message, ptr, (UIntPtr)data.Length);
                }
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern Int32 nng_msg_insert(nng_msg message, byte[] data, UIntPtr size);

        public static Int32 nng_msg_insert(nng_msg message, byte[] data)
        {
            return nng_msg_insert(message, data, (UIntPtr)data.Length);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_trim(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_chop(nng_msg message, UIntPtr size);

        // [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        // static extern Int32 nng_msg_header_append(nng_msg message, byte[] data, UIntPtr size);

        // public static Int32 nng_msg_header_append(nng_msg message, byte[] data)
        // {
        //     return nng_msg_header_append(message, data, (UIntPtr)data.Length);
        // }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe Int32 nng_msg_header_append(nng_msg message, byte* data, UIntPtr size);

        public static Int32 nng_msg_header_append(nng_msg message, ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    return nng_msg_header_append(message, ptr, (UIntPtr)data.Length);
                }
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern Int32 nng_msg_header_insert(nng_msg message, byte[] data, UIntPtr size);

        public static Int32 nng_msg_header_insert(nng_msg message, byte[] data)
        {
            return nng_msg_header_insert(message, data, (UIntPtr)data.Length);
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_trim(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_chop(nng_msg message, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_append_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_insert_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_chop_u32(nng_msg message, out UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_header_trim_u32(nng_msg message, out UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_append_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_insert_u32(nng_msg message, UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_chop_u32(nng_msg message, out UInt32 data);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_msg_trim_u32(nng_msg message, out UInt32 data);

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


        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_id(nng_pipe pipe);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_bool(nng_pipe pipe, string opt, out bool val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_int(nng_pipe pipe, string opt, out int val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_ms(nng_pipe pipe, string opt, out nng_duration val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_ptr(nng_pipe pipe, string opt, out IntPtr val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_string(nng_pipe pipe, string opt, out IntPtr val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_size(nng_pipe pipe, string opt, out UIntPtr val);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nng_pipe_getopt_uint64(nng_pipe pipe, string opt, out UInt64 val);
    }
}