using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Push version 0 socket for push/pull protocol
    /// </summary>
    public class PushSocket : Socket, IPushSocket
    {
        /// <summary>
        /// Create a push socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IPushSocket> Open()
        {
            var res = nng_push0_open(out var socket);
            return NngResult<IPushSocket>.OkThen(res, () => new PushSocket { NngSocket = socket });
        }

        private PushSocket() { }
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
        public static NngResult<IPullSocket> Open()
        {
            var res = nng_pull0_open(out var socket);
            return NngResult<IPullSocket>.OkThen(res, () => new PullSocket { NngSocket = socket });
        }

        private PullSocket() { }
    }
}