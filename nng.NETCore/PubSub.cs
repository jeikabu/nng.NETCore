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
    /// Publish version 0 socket for publish/subscribe protocol
    /// </summary>
    public class PubSocket : Socket, IPubSocket
    {
        /// <summary>
        /// Create a publish socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<IPubSocket> Open()
        {
            int res = nng_pub0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IPubSocket>(res);
            }
            return NngResult.Ok<IPubSocket>(new PubSocket { NngSocket = socket });
        }

        private PubSocket(){}
    }

    /// <summary>
    /// Subscribe version 0 socket for publish/subscribe protocol
    /// </summary>
    public class SubSocket : Socket, ISubSocket
    {
        /// <summary>
        /// Create a subscribe socket
        /// </summary>
        /// <returns>The open.</returns>
        public static INngResult<ISubSocket> Open()
        {
            var res = nng_sub0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<ISubSocket>(res);
            }
            return NngResult.Ok<ISubSocket>(new SubSocket { NngSocket = socket });
        }

        private SubSocket(){}
    }

    public class SubAsyncContext<T> : ResvAsyncContext<T>, ISubAsyncContext<T>
    {
    }
}