using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Protocols.UnsafeNativeMethods;

    /// <summary>
    /// Bus version 0 socket for bus protocol
    /// </summary>
    public class BusSocket : Socket, IBusSocket
    {
        /// <summary>
        /// Create a bus socket
        /// </summary>
        /// <returns>The open.</returns>
        public static NngResult<IBusSocket> Open()
        {
            var res = nng_bus0_open(out var socket);
            return NngResult<IBusSocket>.OkThen(res, () => new BusSocket { NngSocket = socket });
        }
    }
}