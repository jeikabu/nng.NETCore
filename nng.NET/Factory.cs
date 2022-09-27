using nng.Native;

namespace nng.Factories
{
    using static nng.Native.Defines;
    using static nng.Native.Msg.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    public abstract class FactoryBase : IAPIFactory<INngMsg>
    {
        public INngMsg CreateMessage()
        {
            return new NngMsg();
        }

        public INngMsg CreateMessage(nng_msg msg)
        {
            return new NngMsg(msg);
        }

        public nng_msg Take(ref INngMsg message)
        {
            var res = message.Take();
            message = null;
            return res;
        }

        public INngAlloc CreateAlloc(int size)
        {
            return NngAlloc.Create(size);
        }

        public NngResult<IBusSocket> BusOpen() => BusSocket.Open();
        public NngResult<IReqSocket> RequesterOpen() => ReqSocket.Open();
        public NngResult<IRepSocket> ReplierOpen() => RepSocket.Open();
        public NngResult<IPubSocket> PublisherOpen() => PubSocket.Open();
        public NngResult<ISubSocket> SubscriberOpen() => SubSocket.Open();
        public NngResult<IPushSocket> PusherOpen() => PushSocket.Open();
        public NngResult<IPullSocket> PullerOpen() => PullSocket.Open();
        public NngResult<IPairSocket> Pair0Open() => Pair0Socket.Open();
        public NngResult<IPairSocket> Pair1Open() => Pair1Socket.Open();
        public NngResult<IPairSocket> PairOpen() => Pair0Socket.Open();
        public NngResult<IRespondentSocket> RespondentOpen() => RespondentSocket.Open();
        public NngResult<ISurveyorSocket> SurveyorOpen() => SurveyorSocket.Open();

        #region INngAsyncFactory
        public NngResult<INngAio> CreateAio(AioCallback callback) => NngAio.Create(callback);
        public NngResult<INngCtx> CreateCtx(INngSocket socket) => NngCtx.Create(socket);
        #endregion

        #region IAsyncContextFactory

        public NngResult<ISendAsyncContext<INngMsg>> CreateSendAsyncContext(ISendSocket socket)
        {
            return SendAsyncContext<INngMsg>.Create(this, socket);
        }
        public NngResult<IReceiveAsyncContext<INngMsg>> CreateReceiveAsyncContext(IRecvSocket socket)
        {
            return ResvAsyncContext<INngMsg>.Create(this, socket);
        }
        public NngResult<ISendReceiveAsyncContext<INngMsg>> CreateSendReceiveAsyncContext(ISendRecvSocket socket, SendReceiveContextSubtype subtype)
        {
            switch (subtype)
            {
                case SendReceiveContextSubtype.Bus:
                case SendReceiveContextSubtype.Pair:
                    return SendReceiveAsyncContext<INngMsg>.Create(this, socket);
                default:
                    return NngResult<ISendReceiveAsyncContext<INngMsg>>.Err(NngErrno.EINVAL);
            }
        }

        public NngResult<ISubAsyncContext<INngMsg>> CreateSubAsyncContext(ISubSocket socket)
        {
            return SubAsyncCtx<INngMsg>.Create(this, socket);
        }

        public NngResult<IReqRepAsyncContext<INngMsg>> CreateReqRepAsyncContext(IReqSocket socket)
        {
            return ReqAsyncCtx<INngMsg>.Create(this, socket);
        }

        public NngResult<IRepReqAsyncContext<INngMsg>> CreateRepReqAsyncContext(IRepSocket socket)
        {
            return RepAsyncCtx<INngMsg>.Create(this, socket);
        }

        public NngResult<ISurveyorAsyncContext<INngMsg>> CreateSurveyorAsyncContext(ISendRecvSocket socket)
        {
            return SurveyAsyncContext<INngMsg>.Create(this, socket);
        }
        #endregion

        #region INngMiscFactory
        public NngResult<IStatRoot> GetStatSnapshot()
        {
            return NngStat.GetStatSnapshot();
        }
        #endregion

        #region INngStreamFactory
        public NngResult<INngStreamListener> StreamListenerCreate(string addr)
        {
            return StreamListener.Alloc(addr);
        }
        public NngResult<INngStreamDialer> StreamDialerCreate(string addr)
        {
            return StreamDialer.Alloc(addr);
        }
        public NngResult<INngStream> StreamFrom(INngAio aio)
        {
            return Stream.From(aio);
        }
        #endregion
    }

    namespace Compat
    {
        public class Factory : FactoryBase, ISocketFactory
        {
            public new NngResult<IPairSocket> PairOpen() => Pair0Socket.Open();
            NngResult<IPairSocket> ISocketFactory.PairOpen()
            {
                return this.PairOpen();
            }
        }
    }

    namespace Latest
    {
        public class Factory : FactoryBase, ISocketFactory
        {
            public new NngResult<IPairSocket> PairOpen() => Pair1Socket.Open();
            NngResult<IPairSocket> ISocketFactory.PairOpen()
            {
                return this.PairOpen();
            }
        }
    }
}
