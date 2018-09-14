using nng.Native;

namespace nng.Tests
{
    using static nng.Native.Msg.UnsafeNativeMethods;


    public class TestFactory : IAPIFactory<IMessage>
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

        public IBusSocket BusOpen()
            => BusSocket.Open();
        public IBusSocket BusCreate(string url, bool isListener)
            => BusSocket.Create(url, isListener);

        public IReqSocket RequesterOpen()
            => ReqSocket<IMessage>.Open();
        public IReqSocket RequesterCreate(string url)
            => ReqSocket<IMessage>.Create(url);
        public IRepSocket ReplierOpen()
            => RepSocket<IMessage>.Open();
        public IRepSocket ReplierCreate(string url)
            => RepSocket<IMessage>.Create(url);

        public IPubSocket PublisherOpen()
            => PubSocket<IMessage>.Open();
        public IPubSocket PublisherCreate(string url)
            => PubSocket<IMessage>.Create(url);
        public ISubSocket SubscriberOpen()
            => SubSocket<IMessage>.Open();
        public ISubSocket SubscriberCreate(string url)
            => SubSocket<IMessage>.Create(url);

        public IPushSocket PusherOpen()
            => PushSocket<IMessage>.Open();
        public IPushSocket PusherCreate(string url, bool isListener)
            => PushSocket<IMessage>.Create(url, isListener);
        public IPullSocket PullerOpen()
            => PullSocket<IMessage>.Open();
        public IPullSocket PullerCreate(string url, bool isListener)
            => PullSocket<IMessage>.Create(url, isListener);

        public IListener ListenerCreate(ISocket socket, string url)
            => Listener.Create(socket, url);

        public IDialer DialerCreate(ISocket socket, string url)
            => Dialer.Create(socket, url);


        #region IAsyncContextFactory
        public ISendAsyncContext<IMessage> CreateSendAsyncContext(ISocket socket)
        {
            var ctx = new SendAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return ctx;
        }
        public IReceiveAsyncContext<IMessage> CreateReceiveAsyncContext(ISocket socket)
        {
            var ctx = new ResvAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return ctx;
        }
        public ISendReceiveAsyncContext<IMessage> CreateSendReceiveAsyncContext(ISocket socket)
        {
            var ctx = new SendReceiveAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return ctx;
        }
        
        public ISubAsyncContext<IMessage> CreateSubAsyncContext(ISocket socket)
        {
            var ctx = new SubAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return ctx;
        }
        
        public IReqRepAsyncContext<IMessage> CreateReqRepAsyncContext(ISocket socket)
        {
            var ctx = new ReqAsyncCtx<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return ctx;
        }
        public IRepReqAsyncContext<IMessage> CreateRepReqAsyncContext(ISocket socket)
        {
            return RepAsyncCtx<IMessage>.Create(this, socket);
        }
        #endregion
    }

}
