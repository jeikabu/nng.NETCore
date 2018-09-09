using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    public abstract class Socket : ISocket
    {
        public nng_socket NngSocket { get; protected set; }

        public void SetOpt(string name, byte[] data)
        {
            var res = nng_setopt(NngSocket, name, data);
            if (res != 0)
            {
                throw new NngException(res);
            }
        }
    }
}