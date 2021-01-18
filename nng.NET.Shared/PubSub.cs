namespace nng
{
    using static nng.Native.Defines;

    public static class Subscriber
    {
        /// <summary>
        /// Subscribe to the specified topic.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Subscribe(this ISubSocket socket, byte[] topic)
        {
            return socket.Socket.SetOpt(NNG_OPT_SUB_SUBSCRIBE, topic);
        }

        /// <summary>
        /// Unsubscribe from the specified topic.
        /// </summary>
        /// <returns>The unsubscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Unsubscribe(this ISubSocket socket, byte[] topic)
        {
            return socket.Socket.SetOpt(NNG_OPT_SUB_UNSUBSCRIBE, topic);
        }

        /// <summary>
        /// Subscribe to the specified topic.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Subscribe<T>(this ISubAsyncContext<T> socket, byte[] topic)
        {
            return socket.Ctx.SetOpt(NNG_OPT_SUB_SUBSCRIBE, topic);
        }

        /// <summary>
        /// Unsubscribe from the specified topic.
        /// </summary>
        /// <returns>The unsubscribe.</returns>
        /// <param name="socket">Socket.</param>
        /// <param name="topic">Topic.</param>
        public static int Unsubscribe<T>(this ISubAsyncContext<T> socket, byte[] topic)
        {
            return socket.Ctx.SetOpt(NNG_OPT_SUB_UNSUBSCRIBE, topic);
        }
    }
}