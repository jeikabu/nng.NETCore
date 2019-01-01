using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Native.Stats
{
    using static Globals;

#if NETSTANDARD2_0
    [System.Security.SuppressUnmanagedCodeSecurity]
#endif
    public sealed class UnsafeNativeMethods
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_close(nng_socket socket);
    }
}