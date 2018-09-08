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

    public interface IFactory<T> : IMessageFactory<T>
    {
        ISendAsyncContext<T> CreatePublisher(string url);

        IReceiveAsyncContext<T> CreateSubscriber(string url);

        ISendAsyncContext<T> CreatePusher(string url, bool isListener);

        IReceiveAsyncContext<T> CreatePuller(string url, bool isListener);

        IReqRepAsyncContext<T> CreateRequester(string url);

        IRepReqAsyncContext<T> CreateReplier(string url);
    }
}