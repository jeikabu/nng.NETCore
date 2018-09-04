using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Pinvoke
{
    using static Globals;
    
    sealed class Basic
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_alloc(UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_free(IntPtr ptr, UIntPtr size);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr nng_strdup(string str);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_strfree(IntPtr str);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern string nng_strerror(int error);
    }
}