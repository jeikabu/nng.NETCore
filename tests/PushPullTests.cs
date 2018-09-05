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
        nng_msg CreateMsg()
        {
            Assert.Equal(0, nng_msg_alloc(out var msg, 32));
            return msg;
        }

        [Fact]
        public async Task PushPull()
        {
            var url = UrlRandomIpc();
            var barrier = new AsyncBarrier(2);
            var push = Task.Run(async () => {
                var pushSocket = PushSocket.CreateAsyncContext(url, true) as PushAsyncCtx;
                await barrier.SignalAndWait();
                Assert.True(await pushSocket.Send(CreateMsg()));
            });
            var pull = Task.Run(async () => {
                await barrier.SignalAndWait();
                var pullSocket = PullSocket.CreateAsyncContext(url, false) as PullAsyncCtx;
                await pullSocket.Receive(CancellationToken.None);
            });
            
            await AssertWait(1000, pull, push);
        }

        [Fact]
        public async Task Broker()
        {
            await PushPullBroker(1, 1, 1);
        }

        async Task PushPullBroker(int numPushers, int numPullers, int numMessagesPerPusher)
        {
            var inUrl = UrlRandomIpc();
            var outUrl = UrlRandomIpc();
            const int numBrokers = 1;
            int numTotalMessages = numPushers * numPullers * numMessagesPerPusher;

            var cts = new CancellationTokenSource();
            var brokersReady = new AsyncBarrier(numBrokers + 1);
            var clientsReady = new AsyncBarrier(numPushers + numPullers + numBrokers);
            var counter = new AsyncCountdownEvent(numTotalMessages);

            var tasks = new List<Task>();
            for (var i = 0; i < numBrokers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pullSocket = PullSocket.CreateAsyncContext(inUrl, true) as PullAsyncCtx)
                    using (var pushSocket = PushSocket.CreateAsyncContext(outUrl, true) as PushAsyncCtx)
                    {
                        await brokersReady.SignalAndWait(); // Broker is ready
                        await clientsReady.SignalAndWait(); // Wait for clients
                        while (!cts.IsCancellationRequested)
                        {
                            var msg = await pullSocket.Receive(cts.Token);
                            await pushSocket.Send(msg);
                        }
                    }
                });
                tasks.Add(task);
            }
            
            await brokersReady.SignalAndWait();

            for (var i = 0; i < numPushers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pushSocket = PushSocket.CreateAsyncContext(inUrl, false) as PushAsyncCtx)
                    {
                        await clientsReady.SignalAndWait(); // This client ready, wait for rest
                        for (var m = 0; m < numMessagesPerPusher; ++m)
                        {
                            await pushSocket.Send(CreateMsg());
                        }
                    }
                    
                });
                tasks.Add(task);
            }
            
            for (var i = 0; i < numPullers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pullSocket = PullSocket.CreateAsyncContext(outUrl, false) as PullAsyncCtx)
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
            
            await AssertWait(4000, counter.WaitAsync());
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
