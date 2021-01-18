using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Basic
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_alloc(UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_free(IntPtr ptr, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_strdup(IntPtr str);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_strfree(IntPtr str);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern string nng_strerror(int error);
    }
}