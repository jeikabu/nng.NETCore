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
        IPubSocket PublisherOpen();
        IPubSocket PublisherCreate(string url);
        ISubSocket SubscriberOpen();
        ISubSocket SubscriberCreate(string url);
        IPushSocket PusherOpen();
        IPushSocket PusherCreate(string url, bool isListener);
        IPullSocket PullerOpen();
        IPullSocket PullerCreate(string url, bool isListener);
        IReqSocket RequesterOpen();
        IReqSocket RequesterCreate(string url);
        IRepSocket ReplierOpen();
        IRepSocket ReplierCreate(string url);
        IListener ListenerCreate(ISocket socket, string url);
        IDialer DialerCreate(ISocket socket, string url);
    }

    public interface IAsyncContextFactory<T>
    {
        ISendAsyncContext<T> CreateSendAsyncContext(ISocket socket);
        ISubAsyncContext<T> CreateSubAsyncContext(ISocket socket);
        IReceiveAsyncContext<T> CreateReceiveAsyncContext(ISocket socket);
        IReqRepAsyncContext<T> CreateReqRepAsyncContext(ISocket socket);
        IRepReqAsyncContext<T> CreateRepReqAsyncContext(ISocket socket);
    }

    public interface IAPIFactory<T> : IMessageFactory<T>, ISocketFactory, IAsyncContextFactory<T>
    {}

}