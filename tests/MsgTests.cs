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

        IMessage MsgRandom()
        {
            var bytes0 = Guid.NewGuid().ToByteArray();
            var bytes1 = Guid.NewGuid().ToByteArray();
            var msg = factory.CreateMessage();
            msg.Append(bytes0);
            msg.Header.Append(bytes1);
            return msg;
        }

        [Fact]
        public void Creation()
        {
            var msg = MsgRandom();

            var clone = msg.Clone();
            Assert.True(Util.BytesEqual(msg.BodyRaw, clone.BodyRaw));
            Assert.True(Util.BytesEqual(msg.HeaderRaw, clone.HeaderRaw));
        }

        [Fact]
        public void HeaderBody()
        {
            var msg = MsgRandom();

            // Message.BodyRaw == Message.Header.BodyRaw and Message.HeaderRaw == Message.Header.HeaderRaw
            Assert.True(Util.BytesEqual(msg.BodyRaw, msg.Header.BodyRaw));
            Assert.True(Util.BytesEqual(msg.HeaderRaw, msg.Header.HeaderRaw));
        }

        [Fact]
        public void AppendBytesAndClear()
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
        public void InsertBytesAndClear()
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

        IEnumerable<byte> Concat(params int[] values)
        {
            IEnumerable<byte> res = null;
            foreach (var val in values)
            {
                var bytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(val));
                if (res == null)
                {
                    res = bytes;
                }
                else
                {
                    res = res.Concat(bytes);
                }
            }
            return res;
        }

        [Fact]
        public void AppendInsertInts()
        {
            var int0 = Util.rng.Next(1000, 1000000);
            var int1 = Util.rng.Next(1000, 1000000);
            var uint0 = (uint)int0;
            var uint1 = (uint)int1;
            
            var msg = factory.CreateMessage();
            msg.Append(uint0);
            msg.Append(uint1);
            var bodyBytes = Concat(int0, int1).ToArray();
            Assert.True(Util.BytesEqual(bodyBytes, msg.BodyRaw));
            msg.Header.Append(uint1);
            msg.Header.Append(uint0);
            var headerBytes = Concat(int1, int0).ToArray();
            Assert.True(Util.BytesEqual(headerBytes, msg.HeaderRaw));
        }
    }
}