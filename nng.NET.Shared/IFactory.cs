using nng.Native;
using System;

namespace nng
{
    /// <summary>
    /// Create nng messages
    /// </summary>
    public interface IMessageFactory<T>
    {
        T CreateMessage();
        T CreateMessage(nng_msg message);
        nng_msg Take(ref T message);
        INngAlloc CreateAlloc(int size);
    }

    /// <summary>
    /// Create nng protocol sockets and dialer/listener
    /// </summary>
    public interface ISocketFactory
    {
        NngResult<IBusSocket> BusOpen();
        NngResult<IReqSocket> RequesterOpen();
        NngResult<IRepSocket> ReplierOpen();
        NngResult<IPubSocket> PublisherOpen();
        NngResult<ISubSocket> SubscriberOpen();
        NngResult<IPushSocket> PusherOpen();
        NngResult<IPullSocket> PullerOpen();
        NngResult<IPairSocket> PairOpen();
        NngResult<IRespondentSocket> RespondentOpen();
        NngResult<ISurveyorSocket> SurveyorOpen();
    }

    public interface INngAsyncFactory
    {
        NngResult<INngAio> CreateAio(AioCallback callback = default);
        NngResult<INngCtx> CreateCtx(INngSocket socket);
    }

    public enum SendReceiveContextSubtype
    {
        Bus,
        Pair,
    }

    /// <summary>
    /// Create contexts for asynchronous IO (nng_aio and nng_ctx)
    /// </summary>
    public interface IAsyncContextFactory<T>
    {
        NngResult<ISendAsyncContext<T>> CreateSendAsyncContext(ISendSocket socket);
        NngResult<IReceiveAsyncContext<T>> CreateReceiveAsyncContext(IRecvSocket socket);
        NngResult<ISendReceiveAsyncContext<T>> CreateSendReceiveAsyncContext(ISendRecvSocket socket, SendReceiveContextSubtype subtype);
        NngResult<ISubAsyncContext<T>> CreateSubAsyncContext(ISubSocket socket);
        NngResult<IReqRepAsyncContext<T>> CreateReqRepAsyncContext(IReqSocket socket);
        NngResult<IRepReqAsyncContext<T>> CreateRepReqAsyncContext(IRepSocket socket);
        NngResult<ISurveyorAsyncContext<T>> CreateSurveyorAsyncContext(ISendRecvSocket socket);
    }

    public interface INngMiscFactory
    {
        NngResult<IStatRoot> GetStatSnapshot();
    }

    public interface INngStreamFactory
    {
        NngResult<INngStreamListener> StreamListenerCreate(string addr);
        NngResult<INngStreamDialer> StreamDialerCreate(string addr);
        NngResult<INngStream> StreamFrom(INngAio aio);
    }

    public interface INngApiFactory : ISocketFactory, INngAsyncFactory, INngStreamFactory, INngMiscFactory
    {}

    public interface IAPIFactory<T> : INngApiFactory, IMessageFactory<T>,  IAsyncContextFactory<T>
    { }

}