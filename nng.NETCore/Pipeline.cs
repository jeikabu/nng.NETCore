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

    public class PushSocket<T> : Socket, IPushSocket
    {
        public static PushSocket<T> Open()
        {
            if (nng_push0_open(out var socket) != 0)
            {
                return null;
            }
            return new PushSocket<T> { NngSocket = socket };
        }

        public static PushSocket<T> Create(string url, bool isListener)
        {
            var socket = Open();
            if (socket == null)
            {
                return null;
            }
            var res = isListener ? nng_listen(socket.NngSocket, url, 0) : nng_dial(socket.NngSocket, url, 0);
            if (res != 0)
            {
                socket.Dispose();
                return null;
            }
            return socket;
        }

        private PushSocket(){}
    }

    public class PullSocket<T> : Socket, IPullSocket
    {
        public static PullSocket<T> Open()
        {
            if (nng_pull0_open(out var socket) != 0)
            {
                return null;
            }
            return new PullSocket<T> { NngSocket = socket };
        }

        public static PullSocket<T> Create(string url, bool isListener)
        {
            var socket = Open();
            if (socket == null)
            {
                return null;
            }
            var res = isListener ? nng_listen(socket.NngSocket, url, 0) : nng_dial(socket.NngSocket, url, 0);
            if (res != 0)
            {
                socket.Dispose();
                return null;
            }
            return socket;
        }

        private PullSocket(){}
    }
}