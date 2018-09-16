namespace nng
{
    public static class FactoryExt
    {
        public static INngResult<IBusSocket> BusCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.BusOpen(), url, isListener);
        public static INngResult<IReqSocket> RequesterCreate<T>(this IAPIFactory<T> factory, string url) => factory.Dial(factory.RequesterOpen(), url);
        public static INngResult<IRepSocket> ReplierCreate<T>(this IAPIFactory<T> factory, string url) => factory.Listen(factory.ReplierOpen(), url);
        public static INngResult<IPubSocket> PublisherCreate<T>(this IAPIFactory<T> factory, string url) => factory.Listen(factory.PublisherOpen(), url);
        public static INngResult<ISubSocket> SubscriberCreate<T>(this IAPIFactory<T> factory, string url) => factory.Dial(factory.SubscriberOpen(), url);
        public static INngResult<IPushSocket> PusherCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.PusherOpen(), url, isListener);
        public static INngResult<IPullSocket> PullerCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.PullerOpen(), url, isListener);

        public static INngResult<TSocket> DialOrListen<TSocket, TMsg>(this IAPIFactory<TMsg> factory, INngResult<TSocket> socketRes, string url, bool isListener)
            where TSocket : ISocket
        {
            if (socketRes is NngOk<TSocket> ok)
            {
                if (isListener)
                {
                    return factory.Listen<TSocket>(socketRes, url);
                }
                else
                {
                    return factory.Dial<TSocket>(socketRes, url);
                }
            }
            return socketRes;
        }
    }
}