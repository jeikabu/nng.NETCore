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
    public class DialerTests
    {
        IAPIFactory<IMessage> factory;

        public DialerTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Fact]
        public async Task Basic()
        {
            for (int i = 0; i < 10; ++i)
            {
                var url = UrlIpc();
                using (var pub = factory.PublisherCreate(url).Unwrap())
                {
                    var socket = factory.SubscriberOpen().Unwrap();
                    using (var dialer = factory.DialerCreate(socket, url))
                    {
                        Assert.NotNull(dialer);
                        Assert.Equal(0, dialer.Start());
                    }
                }
            }
        }
    }
}