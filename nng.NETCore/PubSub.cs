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

    public class PubSocket : IPubSocket
    {
        public static object Create(string url)
        {
            nng_socket socket;
            if (nng_pub0_open(out socket) != 0)
            {
                return null;
            }
            if (nng_listen(socket, url, 0) != 0)
            {
                return null;
            }
            return new PubSocket { Socket = socket };
        }

        public static object CreateAsyncContext(string url)
        {
            var socket = Create(url) as PubSocket;
            if (socket == null)
            {
                return null;
            }
            return SendAsyncCtx.Create(socket);
        }

        public nng_socket Socket { get; private set; }
    }

    public class SubSocket : ISubSocket
    {
        public static object Create(string url)
        {
            nng_socket socket;
            if (nng_sub0_open(out socket) != 0)
            {
                return null;
            }
            if (nng_dial(socket, url, 0) != 0)
            {
                return null;
            }
            return new SubSocket { Socket = socket };
        }

        public static object CreateAsyncContext(string url)
        {
            var socket = SubSocket.Create(url) as SubSocket;
            if (socket == null)
            {
                return null;
            }
            return ResvAsyncCtx.Create(socket);
        }

        public nng_socket Socket { get; private set; }
    }
}