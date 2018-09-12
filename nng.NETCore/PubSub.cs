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

    public class PubSocket<T> : Socket
    {
        public static PubSocket<T> Create(string url)
        {
            nng_socket socket;
            int res = nng_pub0_open(out socket);
            if (res != 0)
            {
                return null;
            }
            res = nng_listen(socket, url, 0);
            if (res != 0)
            {
                return null;
            }
            return new PubSocket<T> { NngSocket = socket };
        }

        public static ISendAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = Create(url);
            if (socket == null)
            {
                return null;
            }
            var ctx = new SendAsyncContext<T>();
            var res = ctx.Init(factory, socket, ctx.callback);
            if (res != 0)
            {
                return null;
            }
            return ctx;
        }

        private PubSocket(){}
    }

    public class SubSocket<T> : Socket
    {
        public static SubSocket<T> Create(string url)
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
            return new SubSocket<T> { NngSocket = socket };
        }

        public static ISubAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = SubSocket<T>.Create(url);
            if (socket == null)
            {
                return null;
            }
            var res = new SubAsyncContext<T>();
            if (res.Init(factory, socket, res.callback) != 0)
            {
                return null;
            }
            return res;
        }

        private SubSocket(){}
    }

    public class SubAsyncContext<T> : ResvAsyncContext<T>, ISubAsyncContext<T>
    {
    }
}