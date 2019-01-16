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
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public DialerTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Fact]
        public void Basic()
        {
            Fixture.TestIterate(() =>
            {
                var url = UrlIpc();
                using (var pub = Factory.PublisherCreate(url).Unwrap())
                {
                    var socket = Factory.SubscriberOpen().Unwrap();
                    using (var dialer = Factory.DialerCreate(socket, url))
                    {
                        Assert.NotNull(dialer);
                        Assert.Equal(0, dialer.Start());
                    }
                }
            });
        }
    }
}