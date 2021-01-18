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
        public static extern Int32 nng_stats_get(out nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern void nng_stats_free(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe char* nng_stat_name(nng_stat statsp);

        /// <summary>
        /// Makes managed copy of unmanaged string so nng_stats_free() (which frees the strings) can be called without issue.
        /// </summary>
        /// <param name="statsp"></param>
        /// <returns></returns>
        public static string nng_stat_name_string(nng_stat statsp)
        {
            unsafe
            {
                var ptr = new IntPtr(nng_stat_name(statsp));
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe char* nng_stat_desc(nng_stat statsp);

        /// <summary>
        /// Makes managed copy of unmanaged string so nng_stats_free() (which frees the strings) can be called without issue.
        /// </summary>
        /// <param name="statsp"></param>
        /// <returns></returns>
        public static string nng_stat_desc_string(nng_stat statsp)
        {
            unsafe
            {
                var ptr = new IntPtr(nng_stat_desc(statsp));
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_stat_type_enum nng_stat_type(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 nng_stat_value(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_unit_enum nng_stat_unit(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe char* nng_stat_string(nng_stat statsp);

        /// <summary>
        /// Makes managed copy of unmanaged string so nng_stats_free() (which frees the strings) can be called without issue.
        /// </summary>
        /// <param name="statsp"></param>
        /// <returns></returns>
        public static string nng_stat_string_string(nng_stat statsp)
        {
            unsafe
            {
                var ptr = new IntPtr(nng_stat_string(statsp));
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_stat nng_stat_next(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern nng_stat nng_stat_child(nng_stat statsp);

        [DllImport(NngDll, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 nng_stat_timestamp(nng_stat statsp);
    }
}