using nng.Native;

namespace nng.Tests
{
    using static nng.Native.Defines;
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

        public nng_msg Take(ref IMessage message)
        {
            var res = message.Take();
            message = null;
            return res;
        }

        public IMemory CreateAlloc(int size)
        {
            return Alloc.Create(size);
        }

        public NngResult<IBusSocket> BusOpen() => BusSocket.Open();
        public NngResult<IReqSocket> RequesterOpen() => ReqSocket.Open();
        public NngResult<IRepSocket> ReplierOpen() => RepSocket.Open();
        public NngResult<IPubSocket> PublisherOpen() => PubSocket.Open();
        public NngResult<ISubSocket> SubscriberOpen() => SubSocket.Open();
        public NngResult<IPushSocket> PusherOpen() => PushSocket.Open();
        public NngResult<IPullSocket> PullerOpen() => PullSocket.Open();
        public NngResult<IPairSocket> PairOpen() => Pair1Socket.Open();
        public NngResult<IRespondentSocket> RespondentOpen() => RespondentSocket.Open();
        public NngResult<ISurveyorSocket> SurveyorOpen() => SurveyorSocket.Open();

        #region IAsyncContextFactory
        public NngResult<ISendAsyncContext<IMessage>> CreateSendAsyncContext(ISocket socket)
        {
            return SendAsyncContext<IMessage>.Create(this, socket);
        }
        public NngResult<IReceiveAsyncContext<IMessage>> CreateReceiveAsyncContext(ISocket socket)
        {
            return ResvAsyncContext<IMessage>.Create(this, socket);
        }
        public NngResult<ISendReceiveAsyncContext<IMessage>> CreateSendReceiveAsyncContext(ISocket socket, SendReceiveContextSubtype subtype)
        {
            switch (subtype)
            {
                case SendReceiveContextSubtype.Bus:
                case SendReceiveContextSubtype.Pair:
                    return SendReceiveAsyncContext<IMessage>.Create(this, socket);
                case SendReceiveContextSubtype.Survey:
                    return SurveyAsyncContext<IMessage>.Create(this, socket);
                default:
                    return NngResult<ISendReceiveAsyncContext<IMessage>>.Err(NngErrno.EINVAL);
            }
        }

        public NngResult<ISubAsyncContext<IMessage>> CreateSubAsyncContext(ISocket socket)
        {
            return SubAsyncContext<IMessage>.Create(this, socket);
        }

        public NngResult<IReqRepAsyncContext<IMessage>> CreateReqRepAsyncContext(ISocket socket)
        {
            return ReqAsyncCtx<IMessage>.Create(this, socket);
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
