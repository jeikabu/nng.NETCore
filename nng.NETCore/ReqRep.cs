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

    public class ReqSocket<T> : Socket, IReqSocket
    {
        public static ReqSocket<T> Open()
        {
            if (nng_req0_open(out var socket) != 0)
            {
                return null;
            }
            return new ReqSocket<T> { NngSocket = socket };
        }

        public static ReqSocket<T> Create(string url)
        {
            var socket = Open();
            if (socket == null)
            {
                return null;
            }
            if (nng_dial(socket.NngSocket, url, 0) != 0)
            {
                socket.Dispose();
                return null;
            }
            return socket;
        }

        private ReqSocket(){}
    }

    public class RepSocket<T> : Socket, IRepSocket
    {
        public static RepSocket<T> Open()
        {
            if (nng_rep0_open(out var socket) != 0)
            {
                return null;
            }
            return new RepSocket<T> { NngSocket = socket };
        }

        public static RepSocket<T> Create(string url)
        {
            var socket = Open();
            if (socket == null)
            {
                return null;
            }
            if (nng_listen(socket.NngSocket, url, 0) != 0)
            {
                socket.Dispose();
                return null;
            }
            return socket;
        }

        private RepSocket(){}
    }
}