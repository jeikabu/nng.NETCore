using nng.Native;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
            var serverReady = new AsyncBarrier(2);
            var clientReady = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var dialUrl = string.Empty;
            var push = Task.Run(async () =>
            {
                using (var socket = Factory.PusherOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await serverReady.SignalAndWait();
                    await clientReady.SignalAndWait();
                    // Make sure receiver is actually receiving before start sending
                    await WaitShort();
                    (await ctx.Send(Factory.CreateMessage())).Unwrap();
                }
            });
            var pull = Task.Run(async () =>
            {
                await serverReady.SignalAndWait();
                using (var socket = Factory.PullerOpen().ThenDial(dialUrl).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    await clientReady.SignalAndWait();
                    await ctx.Receive(cts.Token);
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
            using (var broker = new Broker(new PushPullBrokerImpl(Factory)))
            {
                var tasks = await broker.RunAsync(numPushers, numPullers, numMessagesPerPusher, counter, cts.Token);
                tasks.Add(counter.WaitAsync());
                await CancelAfterAssertwait(tasks, cts);
            }
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
            var socket = Factory.PullerOpen().Unwrap();
            socket.Listen(url).Unwrap();
            var ctx = socket.CreateAsyncContext(Factory).Unwrap();
            disposable.Add(socket);
            disposable.Add(ctx);
            return ctx;
        }
        public ISendAsyncContext<IMessage> CreateOutSocket(string url)
        {
            var socket = Factory.PusherOpen().Unwrap();
            socket.Listen(url).Unwrap();
            var ctx = socket.CreateAsyncContext(Factory).Unwrap();
            disposable.Add(socket);
            disposable.Add(ctx);
            return ctx;
        }
        public IReceiveAsyncContext<IMessage> CreateClient(string url)
        {
            var socket = Factory.PullerOpen().Unwrap();
            socket.Dial(url).Unwrap();
            var ctx = socket.CreateAsyncContext(Factory).Unwrap();
            disposable.Add(socket);
            disposable.Add(ctx);
            return ctx;
        }

        public IMessage CreateMessage()
        {
            return Factory.CreateMessage();
        }

        public void Dispose()
        {
            foreach (var obj in disposable)
            {
                obj.Dispose();
            }
        }

        ConcurrentBag<IDisposable> disposable = new ConcurrentBag<IDisposable>();
    }
}
