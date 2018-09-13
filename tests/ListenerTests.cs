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
        IAPIFactory<IMessage> factory;

        public ListenerTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Fact]
        public async Task Basic()
        {
            var url = UrlRandomIpc();
            var socket = factory.PublisherOpen();
            using (var listener = factory.ListenerCreate(socket, url))
            {
                Assert.NotNull(listener);
                Assert.Equal(0, listener.Start());
                await WaitReady();
                Assert.NotNull(factory.SubscriberCreate(url));
            }
        }

        [Fact]
        public async Task GetSetOptions()
        {
            var url = UrlRandomIpc();
            var socket = factory.PublisherOpen();
            using (var listener = factory.ListenerCreate(socket, url))
            {
                //AssertGetSetOpts(listener, NNG_OPT_RECVBUF, (int data) => data + 16);
                AssertGetSetOpts(listener, NNG_OPT_RECVMAXSZ, (UIntPtr data) => data + 16);
            }
        }
    }
}