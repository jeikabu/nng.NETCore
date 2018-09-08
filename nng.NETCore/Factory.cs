using nng.Native;

namespace nng.Tests
{
    using static nng.Native.Msg.UnsafeNativeMethods;


    public class TestFactory : IMessageFactory<NngMessage>, IFactory<NngMessage>
    {
        public NngMessage CreateMessage()
        {
            var msg = new NngMessage();
            nng_msg_alloc(out msg.message, 0);
            return msg;
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
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreateSubscriber(string url)
        {
            var context = SubSocket<NngMessage>.CreateAsyncContext(this, url);
            return context;
        }

        public ISendAsyncContext<NngMessage> CreatePusher(string url, bool isListener)
        {
            var context = PushSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreatePuller(string url, bool isListener)
        {
            var context = PullSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            return context;
        }

        public IReqRepAsyncContext<NngMessage> CreateRequester(string url)
        {
            var context = ReqSocket<NngMessage>.CreateAsyncContext(this, url);
            return context;
        }

        public IRepReqAsyncContext<NngMessage> CreateReplier(string url)
        {
            var context = RepSocket<NngMessage>.CreateAsyncContext(this, url);
            return context;
        }
    }

}
