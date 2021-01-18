using nng.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace nng
{
    using static nng.Native.Defines;
    using static nng.Native.Msg.UnsafeNativeMethods;

    public class NngMsgHeader : INngMsgPart
    {
        public NngMsgHeader(nng_msg message)
        {
            this.message = message;
        }

        public int Append(IntPtr data, int size)
        {
            unsafe
            {
                return nng_msg_header_append(NativeNngStruct, (byte*)data, (UIntPtr)size);
            }
        }
        public int Append(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    return nng_msg_header_append(message, ptr, (UIntPtr)data.Length);
                }
            }
        }
        public int Append(UInt32 data) => nng_msg_header_append_u32(NativeNngStruct, data);
        public int Chop(UIntPtr size) => nng_msg_header_chop(NativeNngStruct, size);
        public int Chop(out uint data) => nng_msg_header_chop_u32(NativeNngStruct, out data);
        public void Clear() => nng_msg_header_clear(NativeNngStruct);
        public int Insert(byte[] data) => nng_msg_header_insert(NativeNngStruct, data);
        public int Insert(UInt32 data) => nng_msg_header_insert_u32(NativeNngStruct, data);
        public int Length => (int)nng_msg_header_len(NativeNngStruct);
        public int Trim(UIntPtr size) => nng_msg_header_trim(NativeNngStruct, size);
        public int Trim(out uint data) => nng_msg_header_trim_u32(NativeNngStruct, out data);
        public Span<byte> AsSpan()
        {
            unsafe
            {
                return new Span<byte>(nng_msg_header(NativeNngStruct), (int)nng_msg_header_len(message));
            }
        }
        public IntPtr AsPtr()
        {
            unsafe
            {
                return (IntPtr)nng_msg_header(NativeNngStruct);
            }
        }

        nng_msg NativeNngStruct => message;
        readonly nng_msg message;
    }

    /// <summary>
    /// Message to send/receive with nng
    /// </summary>
    public class NngMsg : INngMsg
    {
        public NngMsg(uint messageBytes = 0)
        {
            int res = nng_msg_alloc(out message, messageBytes);
            NngException.AssertZero(res);
            _header = new NngMsgHeader(message);
        }

        public NngMsg(nng_msg message)
        {
            this.message = message;
            _header = new NngMsgHeader(message);
        }

        public nng_msg Take()
        {
            var msg = message;
            release();
            return msg;
        }

        public nng_msg NativeNngStruct => message;
        public INngMsgPart Header => _header;
        public NngResult<INngMsg> Dup()
        {
            var res = nng_msg_dup(out var msg, NativeNngStruct);
            return NngResult<INngMsg>.OkThen(res, () => new NngMsg(msg));
        }

        public INngPipe Pipe => _pipe ?? (_pipe = new NngPipe(nng_msg_get_pipe(NativeNngStruct)));

        public int Append(IntPtr data, int size)
        {
            unsafe
            {
                return nng_msg_append(NativeNngStruct, (byte*)data, (UIntPtr)size);
            }
        }
        public int Append(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    return nng_msg_append(NativeNngStruct, ptr, (UIntPtr)data.Length);
                }
            }
        }
        public int Append(uint data) => nng_msg_append_u32(NativeNngStruct, data);
        public int Chop(UIntPtr size) => nng_msg_chop(NativeNngStruct, size);
        public int Chop(out uint data) => nng_msg_chop_u32(NativeNngStruct, out data);
        public void Clear() => nng_msg_clear(NativeNngStruct);
        public int Insert(byte[] data) => nng_msg_insert(NativeNngStruct, data);
        public int Insert(uint data) => nng_msg_insert_u32(NativeNngStruct, data);
        public int Length => (int)nng_msg_len(NativeNngStruct);
        public int Trim(UIntPtr size) => nng_msg_trim(NativeNngStruct, size);
        public int Trim(out uint data) => nng_msg_trim_u32(NativeNngStruct, out data);
        public Span<byte> AsSpan()
        {
            unsafe
            {
                return new Span<byte>(nng_msg_body(message), (int)nng_msg_len(message));
            }
        }
        public IntPtr AsPtr()
        {
            unsafe
            {
                return (IntPtr)nng_msg_body(NativeNngStruct);
            }
        }

        private void release()
        {
            _header = null;
            message = default;
        }

        nng_msg message;
        NngMsgHeader _header;
        NngPipe _pipe;

        #region IDisposable
        ~NngMsg() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            nng_msg_free(message);
            release();
            disposed = true;
        }
        bool disposed = false;
        #endregion


    }

    public class NngPipe : INngPipe
    {
        public NngPipe(nng_pipe pipe)
        {
            NativeNngStruct = pipe;
        }

        public nng_pipe NativeNngStruct { get; }
        public int Id => nng_pipe_id(NativeNngStruct);
        public nng_socket Socket => nng_pipe_socket(NativeNngStruct);
        public INngDialer Dialer => nng.NngDialer.Create(nng_pipe_dialer(NativeNngStruct));
        public INngListener Listener => nng.NngListener.Create(nng_pipe_listener(NativeNngStruct));

        public int GetOpt(string name, out bool data)
            => nng_pipe_getopt_bool(NativeNngStruct, name, out data);

        public int GetOpt(string name, out int data)
            => nng_pipe_getopt_int(NativeNngStruct, name, out data);

        public int GetOpt(string name, out nng_duration data)
            => nng_pipe_getopt_ms(NativeNngStruct, name, out data);

        public int GetOpt(string name, out IntPtr data)
            => nng_pipe_getopt_ptr(NativeNngStruct, name, out data);

        public int GetOpt(string name, out string data)
        {
            IntPtr ptr;
            var res = nng_pipe_getopt_string(NativeNngStruct, name, out ptr);
            data = NngString.Create(ptr).ToManaged();
            return res;
        }

        public int GetOpt(string name, out UIntPtr data)
            => nng_pipe_getopt_size(NativeNngStruct, name, out data);

        public int GetOpt(string name, out ulong data)
            => nng_pipe_getopt_uint64(NativeNngStruct, name, out data);

        public NngResult<Unit> Close() => Unit.OkIfZero( nng_pipe_close(NativeNngStruct) );
    }
}
