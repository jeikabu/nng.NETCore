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
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public PushPullTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsNoWsClassData))]
        public Task PushPull(string url)
        {
            return Fixture.TestIterate(() => DoPushPull(url));
        }

        Task DoPushPull(string url)
        {
            var barrier = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var push = Task.Run(async () =>
            {
                using (var pushSocket = Factory.PusherCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await barrier.SignalAndWait();
                    (await pushSocket.Send(Factory.CreateMessage())).Unwrap();
                    await WaitShort();
                }
            });
            var pull = Task.Run(async () =>
            {
                await barrier.SignalAndWait();
                using (var pullSocket = Factory.PullerCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await pullSocket.Receive(cts.Token);
                }
            });
            return CancelAfterAssertwait(cts, pull, push);
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

            var broker = new Broker(new PushPullBrokerImpl(Factory));
            var tasks = await broker.RunAsync(numPushers, numPullers, numMessagesPerPusher, counter, cts.Token);

            await AssertWait(new[] { counter.WaitAsync() }, msTimeout);
            await CancelAndWait(tasks, cts, msTimeout);
        }
    }

    class PushPullBrokerImpl : IBrokerImpl<IMessage>
    {
        public IAPIFactory<IMessage> Factory { get; private set; }

        public PushPullBrokerImpl(IAPIFactory<IMessage> factory)
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
