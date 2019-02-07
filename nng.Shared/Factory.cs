namespace nng
{
    public static class FactoryExt
    {
        public static NngResult<T> ThenListen<T>(this NngResult<T> result, string url, Native.Defines.NngFlag flags = default)
            where T: ISocket
        {
            return result.Then(socket => socket.Listen(url, flags));
        }

        public static NngResult<T> ThenListenAs<T>(this NngResult<T> result, out IListener listener, string url, Native.Defines.NngFlag flags = default)
            where T: ISocket
        {
            listener = null;
            if (result.IsOk())
            {
                var socket = result.Ok();
                var res = socket.ListenWithListener(url, flags);
                if (res.IsOk())
                {
                    listener = res.Ok();
                }
                else
                {
                    return res.IntoErr<T>();
                }
            }
            return result;
        }

        public static NngResult<T> ThenDial<T>(this NngResult<T> result, string url, Native.Defines.NngFlag flags = default)
            where T: ISocket
        {
            return result.Then(socket => socket.Dial(url, flags));
        }
    }
}