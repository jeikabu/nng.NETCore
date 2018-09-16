using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Defines;
    using static nng.Native.Protocols.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    public class BusSocket : Socket, IBusSocket
    {
        public static INngResult<IBusSocket> Open()
        {
            var res = nng_bus0_open(out var socket);
            if (res != 0)
            {
                return NngResult.Fail<IBusSocket>(res);
            }
            return NngResult.Ok<IBusSocket>(new BusSocket { NngSocket = socket });
        }
    }
}