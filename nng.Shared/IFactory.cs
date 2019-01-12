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
        INngResult<IBusSocket> BusOpen();
        INngResult<IReqSocket> RequesterOpen();
        INngResult<IRepSocket> ReplierOpen();
        INngResult<IPubSocket> PublisherOpen();
        INngResult<ISubSocket> SubscriberOpen();
        INngResult<IPushSocket> PusherOpen();
        INngResult<IPullSocket> PullerOpen();
        INngResult<IPairSocket> PairOpen();
        INngResult<IRespondentSocket> RespondentOpen();
        INngResult<ISurveyorSocket> SurveyorOpen();

        IListener ListenerCreate(ISocket socket, string url);
        IDialer DialerCreate(ISocket socket, string url);

        INngResult<TSocket> Dial<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket;
        INngResult<TSocket> Listen<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket;
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
        INngResult<ISendAsyncContext<T>> CreateSendAsyncContext(ISocket socket);
        INngResult<IReceiveAsyncContext<T>> CreateReceiveAsyncContext(ISocket socket);
        INngResult<ISendReceiveAsyncContext<T>> CreateSendReceiveAsyncContext(ISocket socket, SendReceiveContextSubtype subtype);
        INngResult<ISubAsyncContext<T>> CreateSubAsyncContext(ISocket socket);
        INngResult<IReqRepAsyncContext<T>> CreateReqRepAsyncContext(ISocket socket);
        INngResult<IRepReqAsyncContext<T>> CreateRepReqAsyncContext(ISocket socket);
    }

    public interface IMiscFactory
    {
        INngResult<IStatRoot> GetStatSnapshot();
    }

    public interface IAPIFactory<T> : IMessageFactory<T>, ISocketFactory, IAsyncContextFactory<T>, IMiscFactory
    { }

}