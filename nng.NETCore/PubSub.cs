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

    public class PubSocket<T> : Socket, IPubSocket
    {
        public static PubSocket<T> Open()
        {
            int res = nng_pub0_open(out var socket);
            if (res != 0)
            {
                return null;
            }
            return new PubSocket<T> { NngSocket = socket };
        }

        public static PubSocket<T> Create(string url)
        {
            var socket = Open();
            if (socket == null)
            {
                return null;
            }
            var res = nng_listen(socket.NngSocket, url, 0);
            if (res != 0)
            {
                socket.Dispose();
                return null;
            }
            return socket;
        }

        private PubSocket(){}
    }

    public class SubSocket<T> : Socket, ISubSocket
    {
        public static SubSocket<T> Open()
        {
            if (nng_sub0_open(out var socket) != 0)
            {
                return null;
            }
            return new SubSocket<T> { NngSocket = socket };
        }

        public static SubSocket<T> Create(string url)
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

        private SubSocket(){}
    }

    public class SubAsyncContext<T> : ResvAsyncContext<T>, ISubAsyncContext<T>
    {
    }
}