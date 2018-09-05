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

    public class PubSocket<T> : IPubSocket
    {
        public static ISocket Create(string url)
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
            return new PubSocket<T> { Socket = socket };
        }

        public static ISendAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = Create(url);
            if (socket == null)
            {
                return null;
            }
            return SendAsyncCtx<T>.Create(socket, factory);
        }

        public nng_socket Socket { get; private set; }
    }

    public class SubSocket<T> : ISubSocket
    {
        public static ISocket Create(string url)
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
            return new SubSocket<T> { Socket = socket };
        }

        public static IReceiveAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = SubSocket<T>.Create(url);
            if (socket == null)
            {
                return null;
            }
            return ResvAsyncCtx<T>.Create(socket, factory);
        }

        public nng_socket Socket { get; private set; }
    }
}