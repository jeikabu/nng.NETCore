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
        IAPIFactory<IMessage> factory;

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

            var clone = msg.Dup();
            Assert.True(Util.BytesEqual(msg.Raw, clone.Raw));
            Assert.True(Util.BytesEqual(msg.Header.Raw, clone.Header.Raw));
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
            Assert.True(msg.Raw.EndsWith(bytes1));
            msg.Header.Append(bytes1);
            msg.Header.Append(bytes0);
            Assert.True(msg.Header.Raw.EndsWith(bytes0));
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
            Assert.True(msg.Raw.StartsWith(bytes0));
            msg.Header.Append(bytes0);
            msg.Header.Insert(bytes1);
            Assert.True(msg.Header.Raw.StartsWith(bytes1));
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
            Assert.True(Util.BytesEqual(bodyBytes, msg.Raw));
            msg.Header.Append(uint1);
            msg.Header.Append(uint0);
            var headerBytes = Concat(int1, int0).ToArray();
            Assert.True(Util.BytesEqual(headerBytes, msg.Header.Raw));
        }

        void ChopTrimPart(IMessagePart part)
        {
            var bytes0 = Guid.NewGuid().ToByteArray();
            var int0 = (uint)Util.rng.Next(1000, 1000000);
            var int1 = (uint)Util.rng.Next(1000, 1000000);

            part.Append(bytes0);

            part.Insert(int0);
            part.Insert(int0);
            part.Trim((UIntPtr)4);
            part.Trim(out var trim);
            Assert.Equal(int0, trim);
            Assert.True(Util.BytesEqual(bytes0, part.Raw));
            
            part.Append(int1);
            part.Append(int1);
            part.Chop((UIntPtr)4);
            part.Chop(out var chop);
            Assert.Equal(int1, chop);
            Assert.True(Util.BytesEqual(bytes0, part.Raw));
        }

        [Fact]
        public void ChopTrim()
        {
            var msg = factory.CreateMessage();
            ChopTrimPart(msg);
            ChopTrimPart(msg.Header);
        }

        [Fact]
        public void Pipe()
        {
            var msg = factory.CreateMessage();
            Assert.Equal(-1, msg.Pipe.Id);
            Assert.False(msg.Pipe.GetOptionBool("option-name"));
            Assert.Equal(0, msg.Pipe.GetOptionInt("option-name"));
            Assert.Equal(0, msg.Pipe.GetOptionMs("option-name"));
            Assert.Equal(UIntPtr.Zero, msg.Pipe.GetOptionPtr("option-name"));
            Assert.Null(msg.Pipe.GetOptionString("option-name"));
            Assert.Equal(UIntPtr.Zero, msg.Pipe.GetOptionSize("option-name"));
            Assert.Equal(0UL, msg.Pipe.GetOptionUInt64("option-name"));
        }
    }
}