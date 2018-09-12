using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Basic.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;
    
    public class NngMessageHeader : IMessagePart
    {
        public NngMessageHeader(nng_msg message)
        {
            this.message = message;
        }

        public int Append(byte[] data) => nng_msg_header_append(NngMsg, data);
        public int Append(UInt32 data) => nng_msg_header_append_u32(NngMsg, data);
        public void Clear() => nng_msg_header_clear(NngMsg);
        public int Insert(byte[] data) => nng_msg_header_insert(NngMsg, data);
        public int Insert(UInt32 data) => nng_msg_header_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_header_len(NngMsg);
        public ReadOnlySpan<byte> Raw => nng_msg_header_span(NngMsg);

        nng_msg NngMsg => message;
        readonly nng_msg message;
    }

    public class Message : IMessage
    {
        public Message(uint messageBytes = 0)
        {
            int res = nng_msg_alloc(out message, messageBytes);
            if (res != 0)
            {
                throw new NngException(res);
            }
            _header = new NngMessageHeader(message);
        }

        public Message(nng_msg message)
        {
            this.message = message;
            _header = new NngMessageHeader(message);
        }

        public nng_msg NngMsg => message;
        public IMessagePart Header => _header;
        public IMessage Dup()
        {
            var res = nng_msg_dup(out var msg, NngMsg);
            if (res != 0)
            {
                throw new NngException(res);
            }
            return new Message(msg);
        }

        public int Append(byte[] data) => nng_msg_append(NngMsg, data);
        public int Append(uint data) => nng_msg_append_u32(NngMsg, data);
        public void Clear() => nng_msg_clear(NngMsg);
        
        public int Insert(byte[] data) => nng_msg_insert(NngMsg, data);
        public int Insert(uint data) => nng_msg_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_len(NngMsg);
        public ReadOnlySpan<byte> Raw => nng_msg_body_span(NngMsg);

        readonly nng_msg message;
        readonly NngMessageHeader _header;

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        
    }
}