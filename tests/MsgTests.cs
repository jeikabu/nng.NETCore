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
    public class MsgTests
    {
        IFactory<IMessage> factory;

        public MsgTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Fact]
        public void AppendAndClear()
        {
            var bytes0 = Guid.NewGuid().ToByteArray();
            var bytes1 = Guid.NewGuid().ToByteArray();

            var msg = factory.CreateMessage();
            Assert.Equal(0, msg.Length);
            Assert.Equal(0, msg.Header.Length);
            Assert.Equal(0, msg.Append(bytes0));
            Assert.Equal(bytes0.Length, msg.Length);
            Assert.Equal(0, msg.Header.Append(bytes0));
            Assert.Equal(bytes0.Length, msg.Header.Length);

            // Clearing header works and body remains
            msg.Header.Clear();
            Assert.Equal(0, msg.Header.Length);
            Assert.Equal(bytes0.Length, msg.Length);

            msg.Clear();
            Assert.Equal(0, msg.Length);

            // Append appends
            msg.Append(bytes0);
            msg.Append(bytes1);
            Assert.True(msg.BodyRaw.EndsWith(bytes1));
            msg.Header.Append(bytes1);
            msg.Header.Append(bytes0);
            Assert.True(msg.HeaderRaw.EndsWith(bytes0));
        }

        [Fact]
        public void Insert()
        {
            var bytes0 = Guid.NewGuid().ToByteArray();
            var bytes1 = Guid.NewGuid().ToByteArray();

            var msg = factory.CreateMessage();
            Assert.Equal(0, msg.Insert(bytes0));
            Assert.Equal(bytes0.Length, msg.Length);
            Assert.Equal(0, msg.Header.Insert(bytes0));
            Assert.Equal(bytes0.Length, msg.Header.Length);

            // Clearing body works and header remains
            msg.Clear();
            Assert.Equal(0, msg.Length);
            Assert.Equal(bytes0.Length, msg.Header.Length);

            msg.Header.Clear();
            Assert.Equal(0, msg.Header.Length);

            // Insert prepends
            msg.Append(bytes1);
            msg.Insert(bytes0);
            Assert.True(msg.BodyRaw.StartsWith(bytes0));
            msg.Header.Append(bytes0);
            msg.Header.Insert(bytes1);
            Assert.True(msg.HeaderRaw.StartsWith(bytes1));
        }
    }
}