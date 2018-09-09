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
            return new PushSocket<T> { NngSocket = socket };
        }

        public static ISendAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url, bool isListener)
        {
            var socket = PushSocket<T>.Create(url, isListener);
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

        private PushSocket(){}
    }

    public class PullSocket<T> : Socket, IPullSocket
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
            return new PullSocket<T> { NngSocket = socket };
        }

        public static IReceiveAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url, bool isListener)
        {
            var socket = PullSocket<T>.Create(url, isListener);
            if (socket == null)
            {
                return null;
            }
            var ctx = new ResvAsyncContext<T>();
            if (ctx.Init(factory, socket, ctx.callback) != 0)
            {
                return null;
            }
            return ctx;
        }

        private PullSocket(){}
    }
}