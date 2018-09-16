using nng.Native;

namespace nng
{
    public interface IMessageFactory<T>
    {
        T CreateMessage();
        T CreateMessage(nng_msg message);
        nng_msg Borrow(T message);
        void Destroy(ref T message);
    }

    public interface ISocketFactory
    {
        INngResult<IBusSocket> BusOpen();
        INngResult<IReqSocket> RequesterOpen();
        INngResult<IRepSocket> ReplierOpen();
        INngResult<IPubSocket> PublisherOpen();
        INngResult<ISubSocket> SubscriberOpen();
        INngResult<IPushSocket> PusherOpen();
        INngResult<IPullSocket> PullerOpen();
        
        IListener ListenerCreate(ISocket socket, string url);
        IDialer DialerCreate(ISocket socket, string url);

        INngResult<TSocket> Dial<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket;
        INngResult<TSocket> Listen<TSocket>(INngResult<TSocket> socketRes, string url) where TSocket : ISocket;
    }

    public interface IAsyncContextFactory<T>
    {
        INngResult<ISendAsyncContext<T>> CreateSendAsyncContext(ISocket socket);
        INngResult<IReceiveAsyncContext<T>> CreateReceiveAsyncContext(ISocket socket);
        INngResult<ISendReceiveAsyncContext<T>> CreateSendReceiveAsyncContext(ISocket socket);
        INngResult<ISubAsyncContext<T>> CreateSubAsyncContext(ISocket socket);
        INngResult<IReqRepAsyncContext<T>> CreateReqRepAsyncContext(ISocket socket);
        INngResult<IRepReqAsyncContext<T>> CreateRepReqAsyncContext(ISocket socket);
    }

    public interface IAPIFactory<T> : IMessageFactory<T>, ISocketFactory, IAsyncContextFactory<T>
    {}

}