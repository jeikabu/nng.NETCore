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
                using (var pub = Factory.PublisherOpen().Unwrap())
                {
                    pub.Listen(url).Unwrap();
                    using (var socket = Factory.SubscriberOpen().Unwrap())
                    using (var dialer = socket.DialerCreate(url).Unwrap())
                    {
                        Assert.NotNull(dialer);
                        Assert.Equal(0, dialer.Start());
                    }
                }
            });
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task Nonblock(string url)
        {
            return Fixture.TestIterate(() => DoNonblock(url));
        }

        async Task DoNonblock(string url)
        {
            using (var pub = Factory.PublisherOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var sub = Factory.SubscriberOpen().Unwrap())
            {
                var dialUrl = GetDialUrl(listener, url);
                var res = await RetryAgain(() => sub.Dial(dialUrl, Defines.NngFlag.NNG_FLAG_NONBLOCK));
                res.Unwrap();
            }
        }
    }
}