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

    public class PushSocket : IPushSocket
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
            return new PushSocket { Socket = socket };
        }

        public static IAsyncContext CreateAsyncContext(string url, bool isListener)
        {
            var pushSocket = PushSocket.Create(url, isListener) as PushSocket;
            if (pushSocket == null)
            {
                return null;
            }
            return SendAsyncCtx.Create(pushSocket);
        }

        public nng_socket Socket { get; private set; }
    }

    public class PullSocket : IPullSocket
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
            return new PullSocket { Socket = socket };
        }

        public static IAsyncContext CreateAsyncContext(string url, bool isListener)
        {
            var pullSocket = PullSocket.Create(url, isListener) as PullSocket;
            if (pullSocket == null)
            {
                return null;
            }
            return ResvAsyncCtx.Create(pullSocket);
        }

        public nng_socket Socket { get; private set; }
    }
}