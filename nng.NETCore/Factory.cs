using nng.Native;

namespace nng.Tests
{
    using static nng.Native.Msg.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

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

        public INngResult<IBusSocket> BusOpen() => BusSocket.Open();
        public INngResult<IReqSocket> RequesterOpen() => ReqSocket.Open();
        public INngResult<IRepSocket> ReplierOpen() => RepSocket.Open();
        public INngResult<IPubSocket> PublisherOpen() => PubSocket.Open();
        public INngResult<ISubSocket> SubscriberOpen() => SubSocket.Open();
        public INngResult<IPushSocket> PusherOpen() => PushSocket.Open();
        public INngResult<IPullSocket> PullerOpen() => PullSocket.Open();
        public IListener ListenerCreate(ISocket socket, string url) => Listener.Create(socket, url);
        public IDialer DialerCreate(ISocket socket, string url) => Dialer.Create(socket, url);

        public INngResult<TSocket> Dial<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket
        {
            if (socketRes is NngOk<TSocket> ok)
            {
                var res = nng_dial(ok.Result.NngSocket, url, 0);
                if (res != 0)
                    return NngResult.Fail<TSocket>(res);
            }
            return socketRes;
        }

        public INngResult<TSocket> Listen<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket
        {
            if (socketRes is NngOk<TSocket> ok)
            {
                var res = nng_listen(ok.Result.NngSocket, url, 0);
                if (res != 0)
                    return NngResult.Fail<TSocket>(res);
            }
            return socketRes;
        }

        #region IAsyncContextFactory
        public INngResult<ISendAsyncContext<IMessage>> CreateSendAsyncContext(ISocket socket)
        {
            var ctx = new SendAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult.Ok<ISendAsyncContext<IMessage>>(ctx);
        }
        public INngResult<IReceiveAsyncContext<IMessage>> CreateReceiveAsyncContext(ISocket socket)
        {
            var ctx = new ResvAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult.Ok<IReceiveAsyncContext<IMessage>>(ctx);
        }
        public INngResult<ISendReceiveAsyncContext<IMessage>> CreateSendReceiveAsyncContext(ISocket socket)
        {
            var ctx = new SendReceiveAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult.Ok<ISendReceiveAsyncContext<IMessage>>(ctx);
        }
        
        public INngResult<ISubAsyncContext<IMessage>> CreateSubAsyncContext(ISocket socket)
        {
            var ctx = new SubAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult.Ok<ISubAsyncContext<IMessage>>(ctx);
        }
        
        public INngResult<IReqRepAsyncContext<IMessage>> CreateReqRepAsyncContext(ISocket socket)
        {
            var ctx = new ReqAsyncCtx<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult.Ok<IReqRepAsyncContext<IMessage>>(ctx);
        }

        public INngResult<IRepReqAsyncContext<IMessage>> CreateRepReqAsyncContext(ISocket socket)
        {
            return RepAsyncCtx<IMessage>.Create(this, socket);
        }
        #endregion
    }

}
