using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Ctx.UnsafeNativeMethods;
    using static nng.Native.Protocols.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    /// <summary>
    /// Request version 0 socket for request/reply protocol
    /// </summary>
    public class ReqSocket : Socket, IReqSocket
    {
        /// <summary>
        /// Create a request socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IReqSocket> Open()
        {
            var res = nng_req0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IReqSocket>(res);
            }
            return NngResult.Ok<IReqSocket>(new ReqSocket { NngSocket = socket });
        }

        private ReqSocket(){}
    }

    /// <summary>
    /// Reply version 0 socket for request/reply protocol
    /// </summary>
    public class RepSocket : Socket, IRepSocket
    {
        /// <summary>
        /// Create a reply socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IRepSocket> Open()
        {
            var res = nng_rep0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IRepSocket>(res);
            }
            return NngResult.Ok<IRepSocket>(new RepSocket { NngSocket = socket });
        }

        private RepSocket(){}
    }
}