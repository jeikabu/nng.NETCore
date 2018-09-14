using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Defines;
    using static nng.Native.Protocols.UnsafeNativeMethods;
    using static nng.Native.Socket.UnsafeNativeMethods;

    public class BusSocket : Socket, IBusSocket
    {
        public static BusSocket Open()
        {
            if (nng_bus0_open(out var socket) != 0)
            {
                return null;
            }
            return new BusSocket { NngSocket = socket };
        }
        
        public static BusSocket Create(string url, bool isListener)
        {
            var socket = Open();
            if (socket != null)
            {
                var res = isListener ? nng_listen(socket.NngSocket, url, 0) : nng_dial(socket.NngSocket, url, 0);
                if (res != 0)
                {
                    socket.Dispose();
                    socket = null;
                }
            }

            return socket;
        }
    }
}