using System;
using System.Collections.Generic;
using System.Linq;

namespace nng
{
    public class MsgPart
    {
        public MsgPart Add(IEnumerable<byte> data)
        {
            buffer.AddRange(data);
            return this;
        }

        public MsgPart Add(uint data)
        {
            data = (uint)System.Net.IPAddress.HostToNetworkOrder((int)data);
            var bytes = BitConverter.GetBytes(data);
            buffer.AddRange(bytes);
            return this;
        }

        public byte[] ToArray()
        {
            if (buffer.Count > 0)
            {
                return buffer.ToArray();
            }
            return null;
        }

        readonly List<byte> buffer = new List<byte>();
    }

    /// <summary>
    /// Work with nng messages with minimal P/Invoke calls
    /// </summary>
    public class MsgBuilder
    {
        public MsgPart Header { get; }
        public MsgPart Body { get; }

        public MsgBuilder()
        {
            Header = new MsgPart();
            Body  = new MsgPart();
        }
        public MsgBuilder(MsgPart header, MsgPart body)
        {
            Header = header;
            Body  = body;
        }

        public Msg Build()
        {
            var header = Header?.ToArray();
            var body = Body?.ToArray();
            return new Msg(header, body);
        }
    }

    public class Msg
    {
        public Msg(ReadOnlyMemory<byte> header = default, ReadOnlyMemory<byte> body = default)
        {
            this.header = header;
            this.body = body;
        }

        /// <summary>
        /// Copy contents of nng message from unmanaged to managed memory.
        /// </summary>
        /// <param name="message"></param>
        public Msg(IMessage message)
        {
            var headerSpan = message.Header.Raw;
            if (headerSpan.Length > 0)
            {
                header = headerSpan.ToArray();
            }
            var bodySpan = message.Raw;
            if (bodySpan.Length > 0)
            {
                body = bodySpan.ToArray();
            }
        }

        public IMessage ToMessage<TMsg>(IMessageFactory<TMsg> factory) where TMsg : IMessage
        {
            var message = factory.CreateMessage();
            if (header.Length > 0)
            {
                message.Header.Append(header.Span);
            }
            if (body.Length > 0)
            {
                message.Append(body.Span);
            }
            return message;
        }

        readonly ReadOnlyMemory<byte> header;
        readonly ReadOnlyMemory<byte> body;
    }
}