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

    [Collection("nng")]
    public class PushPullTests
    {
        IAPIFactory<IMessage> factory;

        public PushPullTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(TransportsNoWsClassData))]
        public async Task PushPull(string url)
        {
            var barrier = new AsyncBarrier(2);
            var push = Task.Run(async () => {
                var pushSocket = factory.CreatePusher(url, true).Unwrap();
                await barrier.SignalAndWait();
                Assert.True(await pushSocket.Send(factory.CreateMessage()));
            });
            var pull = Task.Run(async () => {
                await barrier.SignalAndWait();
                var pullSocket = factory.CreatePuller(url, false).Unwrap();
                await pullSocket.Receive(CancellationToken.None);
            });
            
            await AssertWait(1000, pull, push);
        }

        [Fact]
        public async Task Broker()
        {
            await PushPullBrokerAsync(2, 3, 2);
        }

        async Task PushPullBrokerAsync(int numPushers, int numPullers, int numMessagesPerPusher, int msTimeout = 1000)
        {
            // In pull/push (pipeline) pattern, each message is sent to one receiver in round-robin fashion
            int numTotalMessages = numPushers * numMessagesPerPusher;
            var counter = new AsyncCountdownEvent(numTotalMessages);
            var cts = new CancellationTokenSource();
            
            var broker = new Broker(new PushPullBrokerImpl(factory));
            var tasks = await broker.RunAsync(numPushers, numPullers, numMessagesPerPusher, counter, cts.Token);

            await AssertWait(msTimeout, counter.WaitAsync());
            await CancelAndWait(cts, tasks.ToArray());
        }
    }

    class PushPullBrokerImpl : IBrokerImpl<IMessage>
    {
        public IAPIFactory<IMessage>  Factory { get; private set; }

        public PushPullBrokerImpl(IAPIFactory<IMessage>  factory)
        {
            Factory = factory;
        }

        public IReceiveAsyncContext<IMessage> CreateInSocket(string url)
        {
            return Factory.CreatePuller(url, true).Unwrap();
        }
        public ISendAsyncContext<IMessage> CreateOutSocket(string url)
        {
            return Factory.CreatePusher(url, true).Unwrap();
        }
        public IReceiveAsyncContext<IMessage> CreateClient(string url)
        {
            return Factory.CreatePuller(url, false).Unwrap();
        }

        public IMessage CreateMessage()
        {
            return Factory.CreateMessage();
        }
    }
}
