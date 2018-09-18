namespace nng
{
    using static nng.Native.Defines;

    /// <summary>
    /// Represents subscribe half of publish/subscribe protocol
    /// </summary>
    public interface ISubscriber : IHasSocket
    {
    }

    public static class Subscriber
    {
        /// <summary>
        /// Subscribe to the specified topic.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Subscribe(this ISubscriber socket, byte[] topic)
        {
            return socket.Socket.SetOpt(NNG_OPT_SUB_SUBSCRIBE, topic);
        }

        /// <summary>
        /// Unsubscribe from the specified topic.
        /// </summary>
        /// <returns>The unsubscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Unsubscribe(this ISubscriber socket, byte[] topic)
        {
            return socket.Socket.SetOpt(NNG_OPT_SUB_UNSUBSCRIBE, topic);
        }
    }
}