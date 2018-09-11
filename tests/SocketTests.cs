using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;
    using static nng.Native.Defines;

    [Collection("nng")]
    public class SocketTests
    {
        IFactory<IMessage> factory;

        public SocketTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Fact]
        public async void GetSetOpt()
        {
            var url = UrlRandomIpc();
            var rep = factory.CreateReplier(url);
            var req = factory.CreateRequester(url);

            // bool
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_TCP_NODELAY, out bool isSet));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_TCP_NODELAY, !isSet));
            req.Socket.GetOpt(NNG_OPT_TCP_NODELAY, out bool isSetNow);
            Assert.NotEqual(isSet, isSetNow);

            // int
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECVBUF, out int recvBufNumMsgs));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECVBUF, recvBufNumMsgs + 1));
            req.Socket.GetOpt(NNG_OPT_RECVBUF, out int nextRecvBufNumMsgs);
            Assert.Equal(recvBufNumMsgs + 1, nextRecvBufNumMsgs);
            
            // nng_duration
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECONNMINT, out nng_duration minWaitConnMs));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECONNMINT, minWaitConnMs + 100));
            req.Socket.GetOpt(NNG_OPT_RECONNMINT, out nng_duration nextMinWaitConnMs);
            Assert.Equal(minWaitConnMs + 100, nextMinWaitConnMs);

            // size_t
            Assert.Equal(0, req.Socket.GetOpt(NNG_OPT_RECVMAXSZ, out UIntPtr recvMaxBytes));
            Assert.Equal(0, req.Socket.SetOpt(NNG_OPT_RECVMAXSZ, UIntPtr.Zero));
            req.Socket.GetOpt(NNG_OPT_RECVMAXSZ, out UIntPtr nextRecvMaxBytes);
            Assert.Equal(UIntPtr.Zero, nextRecvMaxBytes);

            // uint64_t
            
            // string

            // ptr
        }
    }
}