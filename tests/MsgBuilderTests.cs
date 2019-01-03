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
            var bytes = randomBytes();
            uint anInt = 0x01234567;
            var builder = new MsgBuilder();
            builder.Body.Add(bytes).Add(anInt);
            var builderMsg = builder.Build().ToMessage(factory);
            var msg = factory.CreateMessage();
            msg.Append(bytes);
            msg.Append(anInt);
            Assert.True(Util.Equals(msg, builderMsg));
        }
    }
}