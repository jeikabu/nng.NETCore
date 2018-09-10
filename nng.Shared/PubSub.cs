namespace nng
{
    using static nng.Native.Defines;

    public static class Subscriber
    {
        public static bool Subscribe(this ISubscribeSocket self, byte[] topic)
        {
            self.SetOpt(NNG_OPT_SUB_SUBSCRIBE, topic);
            return true;
        }

        public static bool Unsubscribe(this ISubscribeSocket self, byte[] topic)
        {
            self.SetOpt(NNG_OPT_SUB_UNSUBSCRIBE, topic);
            return true;
        }
    }
}