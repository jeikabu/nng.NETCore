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
    public class ListenerTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public ListenerTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Fact]
        public async Task Basic()
        {
            var url = UrlInproc();
            using (var socket = Factory.PublisherOpen().Unwrap())
            using (var listener = Factory.ListenerCreate(socket, url))
            {
                Assert.NotNull(listener);
                Assert.Equal(0, listener.Start());
                await WaitReady();
                Assert.NotNull(Factory.SubscriberCreate(url));
            }
        }

        [Fact]
        public async Task GetSetOptions()
        {
            var url = UrlInproc();
            using (var socket = Factory.PublisherOpen().Unwrap())
            using (var listener = Factory.ListenerCreate(socket, url))
            {
                //AssertGetSetOpts(listener, NNG_OPT_RECVBUF, (int data) => data + 16);
                AssertGetSetOpts(listener, NNG_OPT_RECVMAXSZ, (UIntPtr data) => data + 16);
            }
        }
    }
}