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

    public class PubSocket : Socket, IPubSocket
    {
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

    public class SubSocket : Socket, ISubSocket
    {
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