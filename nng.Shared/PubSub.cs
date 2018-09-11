namespace nng
{
    using static nng.Native.Defines;

    public interface ISubscriber : IHasSocket
    {
    }

    public static class Subscriber
    {
        public static int Subscribe(this ISubscriber self, byte[] topic)
        {
            return self.Socket.SetOpt(NNG_OPT_SUB_SUBSCRIBE, topic);
        }

        public static int Unsubscribe(this ISubscriber self, byte[] topic)
        {
            return self.Socket.SetOpt(NNG_OPT_SUB_UNSUBSCRIBE, topic);
        }
    }
}