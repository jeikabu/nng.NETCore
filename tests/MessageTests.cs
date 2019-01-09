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
            Assert.True(Util.Equals(msg, clone));
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
            Assert.True(msg.AsSpan().EndsWith(bytes1));
            msg.Header.Append(bytes1);
            msg.Header.Append(bytes0);
            Assert.True(msg.Header.AsSpan().EndsWith(bytes0));
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
            Assert.True(msg.AsSpan().StartsWith(bytes0));
            msg.Header.Append(bytes0);
            msg.Header.Insert(bytes1);
            Assert.True(msg.Header.AsSpan().StartsWith(bytes1));
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
            Assert.True(Util.BytesEqual(bodyBytes, msg.AsSpan()));
            msg.Header.Append(uint1);
            msg.Header.Append(uint0);
            var headerBytes = Concat(int1, int0).ToArray();
            Assert.True(Util.BytesEqual(headerBytes, msg.Header.AsSpan()));
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
            Assert.True(Util.BytesEqual(bytes0, part.AsSpan()));

            part.Append(int1);
            part.Append(int1);
            part.Chop((UIntPtr)4);
            part.Chop(out var chop);
            Assert.Equal(int1, chop);
            Assert.True(Util.BytesEqual(bytes0, part.AsSpan()));
        }

        [Fact]
        public void ChopTrim()
        {
            var msg = factory.CreateMessage();
            ChopTrimPart(msg);
            ChopTrimPart(msg.Header);
        }

        [Fact]
        public void Spans()
        {
            var randomBytes = Guid.NewGuid().ToByteArray();
            var msg = factory.CreateMessage();
            unsafe
            {
                var randomStack = stackalloc byte[randomBytes.Length];
                var randomSpan = new Span<byte>(randomStack, randomBytes.Length);
                randomBytes.AsSpan().CopyTo(randomSpan);
                msg.Append(randomSpan);
            }

            Assert.True(Util.BytesEqual(randomBytes, msg.AsSpan()));
        }

        [Fact]
        public void Pipe()
        {
            var msg = factory.CreateMessage();
            Assert.Equal(-1, msg.Pipe.Id);

            int result;
            result = msg.Pipe.GetOpt("option-name", out bool boolData);
            Assert.NotEqual(0, result);
            Assert.False(boolData);

            result = msg.Pipe.GetOpt("option-name", out int intData);
            Assert.NotEqual(0, result);
            Assert.Equal(0, intData);

            result = msg.Pipe.GetOpt("option-name", out nng_duration msData);
            Assert.NotEqual(0, result);
            Assert.Equal(default(nng_duration), msData);

            result = msg.Pipe.GetOpt("option-name", out IntPtr ptr);
            Assert.NotEqual(0, result);
            Assert.Equal(IntPtr.Zero, ptr);

            result = msg.Pipe.GetOpt("option-name", out string strData);
            Assert.NotEqual(0, result);
            Assert.Null(strData);

            result = msg.Pipe.GetOpt("option-name", out UIntPtr sizeData);
            Assert.NotEqual(0, result);
            Assert.Equal(UIntPtr.Zero, sizeData);

            result = msg.Pipe.GetOpt("option-name", out ulong ulongData);
            Assert.NotEqual(0, result);
            Assert.Equal(0UL, ulongData);
        }
    }
}