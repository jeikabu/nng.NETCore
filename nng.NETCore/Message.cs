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
        public int Chop(UIntPtr size) => nng_msg_header_chop(NngMsg, size);
        public int Chop(out uint data) => nng_msg_header_chop_u32(NngMsg, out data);
        public void Clear() => nng_msg_header_clear(NngMsg);
        public int Insert(byte[] data) => nng_msg_header_insert(NngMsg, data);
        public int Insert(UInt32 data) => nng_msg_header_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_header_len(NngMsg);
        public int Trim(UIntPtr size) => nng_msg_header_trim(NngMsg, size);
        public int Trim(out uint data) => nng_msg_header_trim_u32(NngMsg, out data);
        public ReadOnlySpan<byte> Raw => nng_msg_header_span(NngMsg);

        nng_msg NngMsg => message;
        readonly nng_msg message;
    }

    /// <summary>
    /// Message to send/receive with nng
    /// </summary>
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

        public IPipe Pipe => _pipe ?? (_pipe = new Pipe(nng_msg_get_pipe(NngMsg)));

        public int Append(byte[] data) => nng_msg_append(NngMsg, data);
        public int Append(uint data) => nng_msg_append_u32(NngMsg, data);
        public int Chop(UIntPtr size) => nng_msg_chop(NngMsg, size);
        public int Chop(out uint data) => nng_msg_chop_u32(NngMsg, out data);
        public void Clear() => nng_msg_clear(NngMsg);
        public int Insert(byte[] data) => nng_msg_insert(NngMsg, data);
        public int Insert(uint data) => nng_msg_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_len(NngMsg);
        public int Trim(UIntPtr size) => nng_msg_trim(NngMsg, size);
        public int Trim(out uint data) => nng_msg_trim_u32(NngMsg, out data);
        public ReadOnlySpan<byte> Raw => nng_msg_body_span(NngMsg);

        readonly nng_msg message;
        readonly NngMessageHeader _header;
        Pipe _pipe;

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

    public class Pipe : IPipe
    {
        public Pipe(nng_pipe pipe)
        {
            NngPipe = pipe;
        }

        public nng_pipe NngPipe { get; }
        public int Id => nng_pipe_id(NngPipe);

        public bool GetOptionBool(string option)
        {
            nng_pipe_getopt_bool(NngPipe, option, out int val);
            return val != 0;
        }

        public int GetOptionInt(string option)
        {
            nng_pipe_getopt_int(NngPipe, option, out int val);
            return val;
        }

        public int GetOptionMs(string option)
        {
            nng_pipe_getopt_ms(NngPipe, option, out int val);
            return val;
        }

        public UIntPtr GetOptionPtr(string option)
        {
            nng_pipe_getopt_ptr(NngPipe, option, out UIntPtr val);
            return val;
        }

        public string GetOptionString(string option)
        {
            nng_pipe_getopt_string(NngPipe, option, out string val);
            return val;
        }

        public UIntPtr GetOptionSize(string option)
        {
            nng_pipe_getopt_size(NngPipe, option, out UIntPtr val);
            return val;
        }

        public ulong GetOptionUInt64(string option)
        {
            nng_pipe_getopt_uint64(NngPipe, option, out ulong val);
            return val;
        }
    }
}