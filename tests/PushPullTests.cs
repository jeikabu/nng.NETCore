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
            const int numIterations = 10;
            int numOk = 0;
            for (int i = 0; i < numIterations; ++i)
            {
                if (await DoPushPull(url))
                {
                    ++numOk;
                }
            }
            Assert.InRange((float)numOk/numIterations, 0.7, 1.0);
        }

        async Task<bool> DoPushPull(string url)
        {
            var barrier = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var push = Task.Run(async () => {
                using(var pushSocket = factory.PusherCreate(url, true).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    await barrier.SignalAndWait();
                    Assert.True(await pushSocket.Send(factory.CreateMessage()));
                }
            });
            var pull = Task.Run(async () => {
                await barrier.SignalAndWait();
                using (var pullSocket = factory.PullerCreate(url, false).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    await pullSocket.Receive(cts.Token);
                }
            });
            cts.CancelAfter(DefaultTimeoutMs);
            return await Util.WhenAll(DefaultTimeoutMs, pull, push);
        }

        [Fact]
        public async Task Broker()
        {
            await PushPullBrokerAsync(2, 3, 2);
        }

        async Task PushPullBrokerAsync(int numPushers, int numPullers, int numMessagesPerPusher, int msTimeout = DefaultTimeoutMs)
        {
            // In pull/push (pipeline) pattern, each message is sent to one receiver in round-robin fashion
            int numTotalMessages = numPushers * numMessagesPerPusher;
            var counter = new AsyncCountdownEvent(numTotalMessages);
            var cts = new CancellationTokenSource();
            
            var broker = new Broker(new PushPullBrokerImpl(factory));
            var tasks = await broker.RunAsync(numPushers, numPullers, numMessagesPerPusher, counter, cts.Token);

            await AssertWait(msTimeout, counter.WaitAsync());
            await CancelAndWait(cts, msTimeout, tasks.ToArray());
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
            return Factory.PullerCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap();
        }
        public ISendAsyncContext<IMessage> CreateOutSocket(string url)
        {
            return Factory.PusherCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap();
        }
        public IReceiveAsyncContext<IMessage> CreateClient(string url)
        {
            return Factory.PullerCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap();
        }

        public IMessage CreateMessage()
        {
            return Factory.CreateMessage();
        }
    }
}
