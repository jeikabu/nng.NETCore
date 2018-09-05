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

    public class PushSocket<T> : IPushSocket
    {
        public static ISocket Create(string url, bool isListener)
        {
            nng_socket socket;
            if (nng_push0_open(out socket) != 0)
            {
                return null;
            }
            var res = isListener ? nng_listen(socket, url, 0) : nng_dial(socket, url, 0);
            if (res != 0)
            {
                return null;
            }
            return new PushSocket<T> { Socket = socket };
        }

        public static ISendAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url, bool isListener)
        {
            var pushSocket = PushSocket<T>.Create(url, isListener);
            if (pushSocket == null)
            {
                return null;
            }
            return SendAsyncCtx<T>.Create(pushSocket, factory);
        }

        public nng_socket Socket { get; private set; }
    }

    public class PullSocket<T> : IPullSocket
    {
        public static ISocket Create(string url, bool isListener)
        {
            nng_socket socket;
            if (nng_pull0_open(out socket) != 0)
            {
                return null;
            }
            var res = isListener ? nng_listen(socket, url, 0) : nng_dial(socket, url, 0);
            if (res != 0)
            {
                return null;
            }
            return new PullSocket<T> { Socket = socket };
        }

        public static IReceiveAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url, bool isListener)
        {
            var pullSocket = PullSocket<T>.Create(url, isListener);
            if (pullSocket == null)
            {
                return null;
            }
            return ResvAsyncCtx<T>.Create(pullSocket, factory);
        }

        public nng_socket Socket { get; private set; }
    }
}