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


    public interface IReqSocket : ISocket
    {
    }

    public interface IRepSocket : ISocket
    {
    }

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
            return new ReqSocket { nngSocket = socket };
        }

        public object CreateAioCtx()
        {
            return ReqAsyncCtx.Create(this);
        }

        public nng_socket Socket => nngSocket;

        nng_socket nngSocket;
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
            return new RepSocket { nngSocket = socket };
        }

        public object CreateAioCtx()
        {
            return RepAsyncCtx.Create(this);
        }

        public nng_socket Socket => nngSocket;

        nng_socket nngSocket;
    }
}