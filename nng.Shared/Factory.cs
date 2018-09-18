namespace nng
{
    public static class FactoryExt
    {
        /// <summary>
        /// Create bus protocol node
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <param name="isListener">If set to <c>true</c> is listener.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IBusSocket> BusCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.BusOpen(), url, isListener);
        /// <summary>
        /// Create request node for request/reply protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IReqSocket> RequesterCreate<T>(this IAPIFactory<T> factory, string url) => factory.Dial(factory.RequesterOpen(), url);
        /// <summary>
        /// Create reply node for request/reply protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IRepSocket> ReplierCreate<T>(this IAPIFactory<T> factory, string url) => factory.Listen(factory.ReplierOpen(), url);
        /// <summary>
        /// Create publish node for publish/subscribe protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IPubSocket> PublisherCreate<T>(this IAPIFactory<T> factory, string url) => factory.Listen(factory.PublisherOpen(), url);
        /// <summary>
        /// Create subscribe node for publish/subscribe protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<ISubSocket> SubscriberCreate<T>(this IAPIFactory<T> factory, string url) => factory.Dial(factory.SubscriberOpen(), url);
        /// <summary>
        /// Create push node for push/pull protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <param name="isListener">If set to <c>true</c> is listener.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IPushSocket> PusherCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.PusherOpen(), url, isListener);
        /// <summary>
        /// Create pull node for push/pull protocol
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="factory">Factory.</param>
        /// <param name="url">URL.</param>
        /// <param name="isListener">If set to <c>true</c> is listener.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static INngResult<IPullSocket> PullerCreate<T>(this IAPIFactory<T> factory, string url, bool isListener) => factory.DialOrListen(factory.PullerOpen(), url, isListener);

        /// <summary>
        /// Creates a dialer or listener associated with a socket and starts it.
        /// </summary>
        /// <returns>The or listen.</returns>
        /// <param name="factory">Factory used to dial/listen.</param>
        /// <param name="socketRes">Socket the dialer/listener is associated with</param>
        /// <param name="url">URL used by dialer/listener</param>
        /// <param name="isListener">If set to <c>true</c> is listener, else is dialer</param>
        /// <typeparam name="TSocket">The 1st type parameter.</typeparam>
        /// <typeparam name="TMsg">The 2nd type parameter.</typeparam>
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