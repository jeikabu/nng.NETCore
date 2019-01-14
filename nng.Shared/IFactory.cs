using nng.Native;

namespace nng
{
    /// <summary>
    /// Create nng messages
    /// </summary>
    public interface IMessageFactory<T>
    {
        T CreateMessage();
        T CreateMessage(nng_msg message);
        nng_msg Borrow(T message);
        void Destroy(ref T message);
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

        IListener ListenerCreate(ISocket socket, string url);
        IDialer DialerCreate(ISocket socket, string url);

        NngResult<TSocket> Dial<TSocket>(NngResult<TSocket> socketRes, string url) where TSocket : ISocket;
        NngResult<TSocket> Listen<TSocket>(NngResult<TSocket> socketRes, string url) where TSocket : ISocket;
    }

    public enum SendReceiveContextSubtype
    {
        Bus,
        Pair,
        Survey,
    }

    /// <summary>
    /// Create contexts for asynchronous IO (nng_aio and nng_ctx)
    /// </summary>
    public interface IAsyncContextFactory<T>
    {
        NngResult<ISendAsyncContext<T>> CreateSendAsyncContext(ISocket socket);
        NngResult<IReceiveAsyncContext<T>> CreateReceiveAsyncContext(ISocket socket);
        NngResult<ISendReceiveAsyncContext<T>> CreateSendReceiveAsyncContext(ISocket socket, SendReceiveContextSubtype subtype);
        NngResult<ISubAsyncContext<T>> CreateSubAsyncContext(ISocket socket);
        NngResult<IReqRepAsyncContext<T>> CreateReqRepAsyncContext(ISocket socket);
        NngResult<IRepReqAsyncContext<T>> CreateRepReqAsyncContext(ISocket socket);
    }

    public interface IMiscFactory
    {
        NngResult<IStatRoot> GetStatSnapshot();
    }

    public interface IAPIFactory<T> : IMessageFactory<T>, ISocketFactory, IAsyncContextFactory<T>, IMiscFactory
    { }

}