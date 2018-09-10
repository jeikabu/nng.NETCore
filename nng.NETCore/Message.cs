using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;
    
    public class Message : IMessage
    {
        public nng_msg NngMsg { get { return message; } }

        public Message()
        {
            int res = nng_msg_alloc(out message, 0);
            if (res != 0)
            {
                throw new NngException(res);
            }
        }

        public Message(nng_msg message)
        {
            this.message = message;
        }

        public void Append(byte[] data)
        {
            var res = nng_msg_append(message, data);
            if (res != 0)
            {
                throw new NngException(res);
            }
        }

        public void Append(UInt32 data)
        {
            var res = nng_msg_append_u32(message, data);
            if (res != 0)
            {
                throw new NngException(res);
            }
        }

        public void Clear()
        {
            nng_msg_clear(message);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                nng_msg_free(message);
            }
            disposed = true;
        }
        bool disposed = false;
        #endregion

        nng_msg message;
    }
}