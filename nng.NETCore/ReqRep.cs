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

    public class ReqSocket : Socket, IReqSocket
    {
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

    public class RepSocket : Socket, IRepSocket
    {
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