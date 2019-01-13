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

        public NngResult<IBusSocket> BusOpen() => BusSocket.Open();
        public NngResult<IReqSocket> RequesterOpen() => ReqSocket.Open();
        public NngResult<IRepSocket> ReplierOpen() => RepSocket.Open();
        public NngResult<IPubSocket> PublisherOpen() => PubSocket.Open();
        public NngResult<ISubSocket> SubscriberOpen() => SubSocket.Open();
        public NngResult<IPushSocket> PusherOpen() => PushSocket.Open();
        public NngResult<IPullSocket> PullerOpen() => PullSocket.Open();
        public IListener ListenerCreate(ISocket socket, string url) => Listener.Create(socket, url);
        public IDialer DialerCreate(ISocket socket, string url) => Dialer.Create(socket, url);

        public NngResult<TSocket> Dial<TSocket>(NngResult<TSocket> socketRes, string url) where TSocket : ISocket
        {
            if (socketRes.TryOk(out var ok))
            {
                var res = nng_dial(ok.NngSocket, url, 0);
                if (res != 0)
                    return NngResult<TSocket>.Fail(res);
            }
            return socketRes;
        }

        public NngResult<TSocket> Listen<TSocket>(NngResult<TSocket> socketRes, string url) where TSocket : ISocket
        {
            if (socketRes.TryOk(out var ok))
            {
                var res = nng_listen(ok.NngSocket, url, 0);
                if (res != 0)
                    return NngResult<TSocket>.Fail(res);
            }
            return socketRes;
        }

        #region IAsyncContextFactory
        public NngResult<ISendAsyncContext<IMessage>> CreateSendAsyncContext(ISocket socket)
        {
            var ctx = new SendAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult<ISendAsyncContext<IMessage>>.Ok(ctx);
        }
        public NngResult<IReceiveAsyncContext<IMessage>> CreateReceiveAsyncContext(ISocket socket)
        {
            var ctx = new ResvAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult<IReceiveAsyncContext<IMessage>>.Ok(ctx);
        }
        public NngResult<ISendReceiveAsyncContext<IMessage>> CreateSendReceiveAsyncContext(ISocket socket)
        {
            var ctx = new SendReceiveAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult<ISendReceiveAsyncContext<IMessage>>.Ok(ctx);
        }

        public NngResult<ISubAsyncContext<IMessage>> CreateSubAsyncContext(ISocket socket)
        {
            var ctx = new SubAsyncContext<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult<ISubAsyncContext<IMessage>>.Ok(ctx);
        }

        public NngResult<IReqRepAsyncContext<IMessage>> CreateReqRepAsyncContext(ISocket socket)
        {
            var ctx = new ReqAsyncCtx<IMessage>();
            ctx.Init(this, socket, ctx.callback);
            return NngResult<IReqRepAsyncContext<IMessage>>.Ok(ctx);
        }

        public NngResult<IRepReqAsyncContext<IMessage>> CreateRepReqAsyncContext(ISocket socket)
        {
            return RepAsyncCtx<IMessage>.Create(this, socket);
        }
        #endregion

        #region IMiscFactory
        public NngResult<IStatRoot> GetStatSnapshot()
        {
            return Stat.GetStatSnapshot();
        }
        #endregion
    }

}
