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
            using (var listener = socket.ListenerCreate(url).Unwrap())
            {
                Assert.NotNull(listener);
                Assert.Equal(0, listener.Start());
                await WaitReady();
                var subSocket = Factory.SubscriberOpen().Unwrap();
                Assert.NotNull(subSocket.DialerCreate(url));
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task Nonblock(string url)
        {
            return Fixture.TestIterate(() => DoNonblock(url));
        }

        async Task DoNonblock(string url)
        {
            using (var pub = Factory.PublisherOpen().Unwrap())
            {
                var listener = await Util.RetryAgain(() => pub.ListenWithListener(url, Defines.NngFlag.NNG_FLAG_NONBLOCK));
                using (var sub = Factory.SubscriberOpen().ThenDial(GetDialUrl(listener.Unwrap(), url)).Unwrap())
                {

                }
            }
        }

        [Fact]
        public async Task GetSetOptions()
        {
            var url = UrlInproc();
            using (var socket = Factory.PublisherOpen().Unwrap())
            using (var listener = socket.ListenerCreate(url).Unwrap())
            {
                //AssertGetSetOpts(listener, NNG_OPT_RECVBUF, (int data) => data + 16);
                AssertGetSetOpts(listener, NNG_OPT_RECVMAXSZ, (UIntPtr data) => data + 16);
            }
        }
    }
}