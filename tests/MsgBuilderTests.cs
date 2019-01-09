using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Native.Defines;
    using static nng.Tests.Util;


    [Collection("nng")]
    public class MsgBuilderTests
    {
        IAPIFactory<IMessage> factory;

        public MsgBuilderTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        byte[] randomBytes()
        {
            return Guid.NewGuid().ToByteArray();
        }

        [Fact]
        public void Creation()
        {
            var randomHeader = randomBytes();
            var randomBody = randomBytes();
            const uint anInt = 0x01234567;

            var builder = new MsgBuilder().AddRange(randomBody).Add(anInt);
            builder.Header.AddRange(randomHeader);
            var msgBuffers = builder.Build();
            var msg = factory.CreateMessage();
            msg.Header.Append(randomHeader);
            msg.Append(randomBody);
            msg.Append(anInt);

            // Builder creates same message as native nng functions
            Assert.True(Util.Equals(msg, msgBuffers.ToMessage(factory)));
            // Create another identical message
            Assert.True(Util.Equals(msg, msgBuffers.ToMessage(factory)));
            // Rebuilding buffers create identical messages
            msgBuffers = builder.Build();
            Assert.True(Util.Equals(msg, msgBuffers.ToMessage(factory)));
        }

        [Fact]
        public void WrapRawMsg()
        {
            var randomHeader = randomBytes();
            var randomBody = randomBytes();
            var msg = factory.CreateMessage();
            msg.Header.Append(randomHeader);
            msg.Append(randomBody);

            var builder = new MsgBuilder(msg);
            Assert.True(Util.Equals(msg, builder.Build().ToMessage(factory)));
            var msgBuffers = new Msg(msg);
            Assert.True(Util.Equals(msg, msgBuffers.ToMessage(factory)));
        }
    }
}