using System;
using System.Collections.Generic;
using System.Linq;

namespace nng
{
    /// <summary>
    /// Part of a message like the header or body.
    /// </summary>
    /// <typeparam name="byte"></typeparam>
    public class MsgPart : List<byte>
    {
        public MsgPart() : base() { }
        public MsgPart(IEnumerable<byte> collection)
            : base(collection)
        { }
        public void Add(uint data)
        {
            data = (uint)System.Net.IPAddress.HostToNetworkOrder((int)data);
            var bytes = BitConverter.GetBytes(data);
            AddRange(bytes);
        }
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
            Body = new MsgPart();
        }

        public MsgBuilder(IMessage message)
        {
            var headerSpan = message.Header.AsSpan();
            if (headerSpan.Length > 0)
            {
                Header = new MsgPart(headerSpan.ToArray());
            }
            var bodySpan = message.AsSpan();
            if (bodySpan.Length > 0)
            {
                Body = new MsgPart(bodySpan.ToArray());
            }
        }

        public MsgBuilder(MsgPart header, MsgPart body)
        {
            Header = header;
            Body = body;
        }

        public MsgBuilder AddRange(IEnumerable<byte> data)
        {
            Body.AddRange(data);
            return this;
        }

        public MsgBuilder Add(uint data)
        {
            Body.Add(data);
            return this;
        }

        public Msg Build()
        {
            var header = Header?.ToArray();
            var body = Body?.ToArray();
            return new Msg(header, body);
        }

        /// <summary>
        /// Reset builder by clearing buffers
        /// </summary>
        public void Reset()
        {
            Header?.Clear();
            Body?.Clear();
        }
    }

    /// <summary>
    /// Message represented as buffers to construct native nng_msg
    /// </summary>
    public class Msg
    {
        public Msg(Memory<byte> header = default, Memory<byte> body = default)
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
            var headerSpan = message.Header.AsSpan();
            if (headerSpan.Length > 0)
            {
                header = headerSpan.ToArray();
            }
            var bodySpan = message.AsSpan();
            if (bodySpan.Length > 0)
            {
                body = bodySpan.ToArray();
            }
        }

        public Span<byte> Header => header.Span;
        public Span<byte> Body => body.Span;

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

        readonly Memory<byte> header;
        readonly Memory<byte> body;
    }
}