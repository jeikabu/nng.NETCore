using nng.Native;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Msg.UnsafeNativeMethods;

    public class NngMessageHeader : IMessagePart
    {
        public NngMessageHeader(nng_msg message)
        {
            this.message = message;
        }

        //public int Append(byte[] data) => nng_msg_header_append(NngMsg, data);
        public int Append(ReadOnlySpan<byte> data) => nng_msg_header_append(NngMsg, data);
        public int Append(UInt32 data) => nng_msg_header_append_u32(NngMsg, data);
        public int Chop(UIntPtr size) => nng_msg_header_chop(NngMsg, size);
        public int Chop(out uint data) => nng_msg_header_chop_u32(NngMsg, out data);
        public void Clear() => nng_msg_header_clear(NngMsg);
        public int Insert(byte[] data) => nng_msg_header_insert(NngMsg, data);
        public int Insert(UInt32 data) => nng_msg_header_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_header_len(NngMsg);
        public int Trim(UIntPtr size) => nng_msg_header_trim(NngMsg, size);
        public int Trim(out uint data) => nng_msg_header_trim_u32(NngMsg, out data);
        public Span<byte> AsSpan() => nng_msg_header_span(NngMsg);

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
            NngException.AssertZero(res);
            _header = new NngMessageHeader(message);
        }

        public Message(nng_msg message)
        {
            this.message = message;
            _header = new NngMessageHeader(message);
        }

        public nng_msg Take()
        {
            var msg = message;
            _header = null;
            message = default;
            return msg;
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

        //public int Append(byte[] data) => nng_msg_append(NngMsg, data);
        public int Append(ReadOnlySpan<byte> data) => nng_msg_append(NngMsg, data);
        public int Append(uint data) => nng_msg_append_u32(NngMsg, data);
        public int Chop(UIntPtr size) => nng_msg_chop(NngMsg, size);
        public int Chop(out uint data) => nng_msg_chop_u32(NngMsg, out data);
        public void Clear() => nng_msg_clear(NngMsg);
        public int Insert(byte[] data) => nng_msg_insert(NngMsg, data);
        public int Insert(uint data) => nng_msg_insert_u32(NngMsg, data);
        public int Length => (int)nng_msg_len(NngMsg);
        public int Trim(UIntPtr size) => nng_msg_trim(NngMsg, size);
        public int Trim(out uint data) => nng_msg_trim_u32(NngMsg, out data);
        public Span<byte> AsSpan() => nng_msg_body_span(NngMsg);

        nng_msg message;
        NngMessageHeader _header;
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

        public int GetOpt(string name, out bool data)
            => nng_pipe_getopt_bool(NngPipe, name, out data);

        public int GetOpt(string name, out int data)
            => nng_pipe_getopt_int(NngPipe, name, out data);

        public int GetOpt(string name, out nng_duration data)
            => nng_pipe_getopt_ms(NngPipe, name, out data);

        public int GetOpt(string name, out IntPtr data)
            => nng_pipe_getopt_ptr(NngPipe, name, out data);

        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_pipe_getopt_string(NngPipe, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }

        public int GetOpt(string name, out UIntPtr data)
            => nng_pipe_getopt_size(NngPipe, name, out data);

        public int GetOpt(string name, out ulong data)
            => nng_pipe_getopt_uint64(NngPipe, name, out data);
    }
}