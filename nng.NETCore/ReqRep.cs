using nng.Pinvoke;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Pinvoke.Basic;
    using static nng.Pinvoke.Ctx;
    using static nng.Pinvoke.Protocols;
    using static nng.Pinvoke.Socket;


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