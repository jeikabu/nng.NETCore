using nng.Native;
using Xunit;

namespace nng.Tests
{
    using static nng.Native.Msg.UnsafeNativeMethods;

    class TestFactory : IMessageFactory<NngMessage>
    {
        public NngMessage CreateMsg()
        {
            Assert.Equal(0, nng_msg_alloc(out var msg, 32));
            return CreateMessage(msg);
        }

        public NngMessage CreateMessage(nng_msg msg)
        {
            return new NngMessage { message = msg };
        }

        public nng_msg Borrow(NngMessage msg)
        {
            return msg.message;
        }

        public void Destroy(ref NngMessage msg)
        {
            nng_msg_free(msg.message);
            msg = null;
        }

        public ISendAsyncContext<NngMessage> CreatePublisher(string url)
        {
            var context = PubSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreateSubscriber(string url)
        {
            var context = SubSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public ISendAsyncContext<NngMessage> CreatePusher(string url, bool isListener)
        {
            var context = PushSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            Assert.NotNull(context);
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreatePuller(string url, bool isListener)
        {
            var context = PullSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            Assert.NotNull(context);
            return context;
        }

        public IReqRepAsyncContext<NngMessage> CreateRequester(string url)
        {
            var context = ReqSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public IRepReqAsyncContext<NngMessage> CreateReplier(string url)
        {
            var context = RepSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }
    }

}
