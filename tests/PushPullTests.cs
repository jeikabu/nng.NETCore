using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;
    using static nng.Tests.Util;

    public class PushPullTests
    {
        TestFactory factory = new TestFactory();

        [Fact]
        public async Task PushPull()
        {
            var url = UrlRandomIpc();
            var barrier = new AsyncBarrier(2);
            var push = Task.Run(async () => {
                var pushSocket = factory.CreatePusher(url, true);
                await barrier.SignalAndWait();
                Assert.True(await pushSocket.Send(factory.CreateMsg()));
            });
            var pull = Task.Run(async () => {
                await barrier.SignalAndWait();
                var pullSocket = factory.CreatePuller(url, false);
                await pullSocket.Receive(CancellationToken.None);
            });
            
            await AssertWait(1000, pull, push);
        }

        [Fact]
        public async Task Broker()
        {
            //await PushPullBroker(1, 1, 1);
            await PushPullBroker(2, 3, 2);
        }

        async Task PushPullBroker(int numPushers, int numPullers, int numMessagesPerPusher, int msTimeout = 1000)
        {
            var inUrl = UrlRandomIpc();
            var outUrl = UrlRandomIpc();
            const int numBrokers = 1;
            // In pull/push (pipeline) pattern, each message is sent to one receiver in round-robin fashion
            int numTotalMessages = numPushers * numMessagesPerPusher;

            var cts = new CancellationTokenSource();
            var brokersReady = new AsyncBarrier(numBrokers + 1);
            var clientsReady = new AsyncBarrier(numPushers + numPullers + numBrokers);
            var counter = new AsyncCountdownEvent(numTotalMessages);

            var numForwarded = 0;
            var tasks = new List<Task>();
            for (var i = 0; i < numBrokers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pullSocket = factory.CreatePuller(inUrl, true))
                    using (var pushSocket = factory.CreatePusher(outUrl, true))
                    {
                        await brokersReady.SignalAndWait(); // Broker is ready
                        await clientsReady.SignalAndWait(); // Wait for clients
                        while (!cts.IsCancellationRequested)
                        {
                            var msg = await pullSocket.Receive(cts.Token);
                            await pushSocket.Send(msg);
                            ++numForwarded;
                        }
                    }
                });
                tasks.Add(task);
            }
            
            await brokersReady.SignalAndWait();

            for (var i = 0; i < numPushers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pushSocket = factory.CreatePusher(inUrl, false))
                    {
                        await clientsReady.SignalAndWait(); // This client ready, wait for rest
                        for (var m = 0; m < numMessagesPerPusher; ++m)
                        {
                            await pushSocket.Send(factory.CreateMsg());
                            await Task.Delay(15);
                        }
                    }
                    
                });
                tasks.Add(task);
            }
            
            for (var i = 0; i < numPullers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pullSocket = factory.CreatePuller(outUrl, false))
                    {
                        await clientsReady.SignalAndWait(); // This client ready, wait for rest
                        while (!cts.IsCancellationRequested)
                        {
                            var _ = await pullSocket.Receive(cts.Token);
                            counter.Signal();
                        }
                    }
                });
                tasks.Add(task);
            }
            
            await AssertWait(msTimeout, counter.WaitAsync());
            cts.Cancel();
            try 
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    // ok
                }
                else
                {
                    throw ex;
                }
            }
        }
    }
}
