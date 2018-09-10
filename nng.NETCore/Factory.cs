using nng.Native;

namespace nng.Tests
{
    using static nng.Native.Msg.UnsafeNativeMethods;


    public class TestFactory : IMessageFactory<IMessage>, IFactory<IMessage>
    {
        public IMessage CreateMessage()
        {
            return new Message();
        }

        public IMessage CreateMessage(nng_msg msg)
        {
            return new Message(msg);
        }

        public nng_msg Borrow(IMessage msg)
        {
            return msg.NngMsg;
        }

        public void Destroy(ref IMessage msg)
        {
            msg.Dispose();
            msg = null;
        }

        public ISendAsyncContext<IMessage> CreatePublisher(string url)
        {
            var context = PubSocket<IMessage>.CreateAsyncContext(this, url);
            return context;
        }

        public ISubAsyncContext<IMessage> CreateSubscriber(string url)
        {
            var context = SubSocket<IMessage>.CreateAsyncContext(this, url);
            return context;
        }

        public ISendAsyncContext<IMessage> CreatePusher(string url, bool isListener)
        {
            var context = PushSocket<IMessage>.CreateAsyncContext(this, url, isListener);
            return context;
        }

        public IReceiveAsyncContext<IMessage> CreatePuller(string url, bool isListener)
        {
            var context = PullSocket<IMessage>.CreateAsyncContext(this, url, isListener);
            return context;
        }

        public IReqRepAsyncContext<IMessage> CreateRequester(string url)
        {
            var context = ReqSocket<IMessage>.CreateAsyncContext(this, url);
            return context;
        }

        public IRepReqAsyncContext<IMessage> CreateReplier(string url)
        {
            var context = RepSocket<IMessage>.CreateAsyncContext(this, url);
            return context;
        }
    }

}
