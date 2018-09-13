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
        public async Task Test()
        {
            var url = UrlRandomIpc();
            var socket = factory.PublisherOpen();
            var listener = factory.ListenerCreate(socket, url);

            listener.Start();
        }
    }
}