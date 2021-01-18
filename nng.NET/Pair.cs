using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Pair version 0 socket for pair protocol
    /// </summary>
    public class Pair0Socket : NngSocket, IPairSocket
    {
        /// <summary>
        /// Create a pair socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IPairSocket> Open()
        {
            int res = nng_pair0_open(out var socket);
            if (res != 0)
            {
                return NngResult<IPairSocket>.Fail(res);
            }
            return NngResult<IPairSocket>.Ok(new Pair0Socket { NativeNngStruct = socket });
        }

        private Pair0Socket() { }
    }

    /// <summary>
    /// Pair version 1 socket for pair protocol
    /// </summary>
    public class Pair1Socket : NngSocket, IPairSocket
    {
        /// <summary>
        /// Create a pair socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IPairSocket> Open()
        {
            int res = nng_pair1_open(out var socket);
            if (res != 0)
            {
                return NngResult<IPairSocket>.Fail(res);
            }
            return NngResult<IPairSocket>.Ok(new Pair1Socket { NativeNngStruct = socket });
        }

        private Pair1Socket() { }
    }
}