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
    /// Push version 0 socket for push/pull protocol
    /// </summary>
    public class PushSocket : Socket, IPushSocket
    {
        /// <summary>
        /// Create a push socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IPushSocket> Open()
        {
            var res = nng_push0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IPushSocket>(res);
            }
            return NngResult.Ok<IPushSocket>(new PushSocket { NngSocket = socket });
        }

        private PushSocket(){}
    }

    /// <summary>
    /// Pull version 0 socket for push/pull protocol
    /// </summary>
    public class PullSocket : Socket, IPullSocket
    {
        /// <summary>
        /// Create a pull socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IPullSocket> Open()
        {
            var res = nng_pull0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IPullSocket>(res);
            }
            return NngResult.Ok<IPullSocket>(new PullSocket { NngSocket = socket });
        }
        
        private PullSocket(){}
    }
}