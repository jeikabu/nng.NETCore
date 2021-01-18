using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Publish version 0 socket for publish/subscribe protocol
    /// </summary>
    public class PubSocket : NngSocket, IPubSocket
    {
        /// <summary>
        /// Create a publish socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IPubSocket> Open()
        {
            int res = nng_pub0_open(out var socket);
            return NngResult<IPubSocket>.OkThen(res, () => new PubSocket { NativeNngStruct = socket });
        }

        private PubSocket() { }
    }

    /// <summary>
    /// Subscribe version 0 socket for publish/subscribe protocol
    /// </summary>
    public class SubSocket : NngSocket, ISubSocket
    {
        /// <summary>
        /// Create a subscribe socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<ISubSocket> Open()
        {
            var res = nng_sub0_open(out var socket);
            return NngResult<ISubSocket>.OkThen(res, () => new SubSocket { NativeNngStruct = socket });
        }

        private SubSocket() { }
    }
}
