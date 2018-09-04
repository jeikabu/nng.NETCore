using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace nng.Pinvoke
{
    using static Globals;
    
    public sealed class Transports
    {
        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_inproc_register();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_ipc_register();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_tcp_register();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_tls_register();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_ws_register();

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 nng_wss_register();
    }
}