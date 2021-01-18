using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Request version 0 socket for request/reply protocol
    /// </summary>
    public class ReqSocket : NngSocket, IReqSocket
    {
        /// <summary>
        /// Create a request socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IReqSocket> Open()
        {
            var res = nng_req0_open(out var socket);
            return NngResult<IReqSocket>.OkThen(res, () => new ReqSocket { NativeNngStruct = socket });
        }

        private ReqSocket() { }
    }

    /// <summary>
    /// Reply version 0 socket for request/reply protocol
    /// </summary>
    public class RepSocket : NngSocket, IRepSocket
    {
        /// <summary>
        /// Create a reply socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IRepSocket> Open()
        {
            var res = nng_rep0_open(out var socket);
            return NngResult<IRepSocket>.OkThen(res, () => new RepSocket { NativeNngStruct = socket });
        }

        private RepSocket() { }
    }
}