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

    public class PushSocket : Socket, IPushSocket
    {
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

    public class PullSocket : Socket, IPullSocket
    {
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