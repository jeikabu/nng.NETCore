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

    public class ReqSocket<T> : IReqSocket
    {
        public static ISocket Create(string url)
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
            return new ReqSocket<T> { Socket = socket };
        }

        public static IReqRepAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = Create(url);
            if (socket == null)
            {
                return null;
            }
            return ReqAsyncCtx<T>.Create(socket, factory);
        }

        public nng_socket Socket { get; private set; }
    }

    public class RepSocket<T> : IRepSocket
    {
        public static ISocket Create(string url)
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
            return new RepSocket<T> { Socket = socket };
        }

        public static IRepReqAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = RepSocket<T>.Create(url);
            if (socket == null)
            {
                return null;
            }
            return RepAsyncCtx<T>.Create(socket, factory);
        }

        public nng_socket Socket { get; private set; }
    }
}