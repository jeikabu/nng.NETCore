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

    public class ReqSocket : IReqSocket
    {
        public static object Create(string url)
        {
            nng_socket socket;
            if (nng_req0_open(out socket) != 0)
            {
                return null;
            }
            if (nng_dial(socket, url, 0) != 0)
            {
                return null;
            }
            return new ReqSocket { Socket = socket };
        }

        public static object CreateAsyncContext(string url)
        {
            var reqSocket = Create(url) as ReqSocket;
            if (reqSocket == null)
            {
                return null;
            }
            return ReqAsyncCtx.Create(reqSocket);
        }

        public nng_socket Socket { get; private set; }
    }

    public class RepSocket : IRepSocket
    {
        public static object Create(string url)
        {
            nng_socket socket;
            if (nng_rep0_open(out socket) != 0)
            {
                return null;
            }
            if (nng_listen(socket, url, 0) != 0)
            {
                return null;
            }
            return new RepSocket { Socket = socket };
        }

        public static object CreateAsyncContext(string url)
        {
            var repSocket = RepSocket.Create(url) as RepSocket;
            if (repSocket == null)
            {
                return null;
            }
            return RepAsyncCtx.Create(repSocket);
        }

        public nng_socket Socket { get; private set; }
    }
}