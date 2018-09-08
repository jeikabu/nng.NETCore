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
            return new PubSocket<T> { NngSocket = socket };
        }

        public static ISendAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
        {
            var socket = Create(url);
            if (socket == null)
            {
                return null;
            }
            var ctx = new SendAsyncCtx<T>();
            var res = ctx.Init(factory, socket, ctx.callback);
            if (res != 0)
            {
                return null;
            }
            return ctx;
        }

        public nng_socket NngSocket { get; private set; }

        private PubSocket(){}
    }

    public class SubSocket<T> : ISubSocket, ISubscriber
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

        public static SubAsyncContext<T> CreateAsyncContext(IMessageFactory<T> factory, string url)
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

        public nng_socket NngSocket { get; private set; }

        private SubSocket(){}
    }

    public class SubAsyncContext<T> : ResvAsyncCtx<T>, ISubscriber
    {}

    public interface ISubscriber : ISubSocket
    {
    }

    public static class Subscriber
    {
        public static bool Subscribe(this ISubscriber self, byte[] topic)
        {
            return nng_setopt(self.NngSocket, Defines.NNG_OPT_SUB_SUBSCRIBE, topic) == 0;
        }

        public static bool Unsubscribe(this ISubscriber self, byte[] topic)
        {
            return nng_setopt(self.NngSocket, Defines.NNG_OPT_SUB_UNSUBSCRIBE, topic) == 0;
        }
    }
}