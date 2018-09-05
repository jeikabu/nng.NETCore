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
    using static nng.Native.Socket.UnsafeNativeMethods;
    using static nng.Tests.Util;

    class TestFactory : IMessageFactory<NngMessage>
    {
        public NngMessage CreateMsg()
        {
            Assert.Equal(0, nng_msg_alloc(out var msg, 32));
            return CreateMessage(msg);
        }

        public NngMessage CreateMessage(nng_msg msg)
        {
            return new NngMessage { message = msg };
        }

        public nng_msg Borrow(NngMessage msg)
        {
            return msg.message;
        }

        public void Destroy(ref NngMessage msg)
        {
            nng_msg_free(msg.message);
            msg = null;
        }

        public ISendAsyncContext<NngMessage> CreatePublisher(string url)
        {
            var context = PubSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreateSubscriber(string url)
        {
            var context = SubSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public ISendAsyncContext<NngMessage> CreatePusher(string url, bool isListener)
        {
            var context = PushSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            Assert.NotNull(context);
            return context;
        }

        public IReceiveAsyncContext<NngMessage> CreatePuller(string url, bool isListener)
        {
            var context = PullSocket<NngMessage>.CreateAsyncContext(this, url, isListener);
            Assert.NotNull(context);
            return context;
        }

        public IReqRepAsyncContext<NngMessage> CreateRequester(string url)
        {
            var context = ReqSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }

        public IRepReqAsyncContext<NngMessage> CreateReplier(string url)
        {
            var context = RepSocket<NngMessage>.CreateAsyncContext(this, url);
            Assert.NotNull(context);
            return context;
        }
    }

    public class PubSubTests
    {
        TestFactory factory = new TestFactory();

        [Fact]
        public async Task BasicPubSub()
        {
            var url = UrlRandomIpc();
            var pub = PubSocket<NngMessage>.Create(url);
            await Task.Delay(100);
            var sub = SubSocket<NngMessage>.Create(url);
            var topic = System.Text.Encoding.ASCII.GetBytes("topic");
            Assert.True(sub.Subscribe(topic));
            var ret = nng_msg_alloc(out var msg, 0);
            ret = nng_msg_append(msg, topic);
            ret = nng_sendmsg(pub.NngSocket, msg, 0);
            ret = nng_recvmsg(sub.NngSocket, out var recv, 0);
        }

        [Fact]
        public async Task PubSub()
        {
            var url = UrlRandomIpc();
            var barrier = new AsyncBarrier(2);
            var pub = Task.Run(async () => {
                var pubSocket = factory.CreatePublisher(url);
                await barrier.SignalAndWait();
                await Task.Delay(200);
                Assert.True(await pubSocket.Send(factory.CreateMsg()));
                //await Task.Delay(100);
            });
            var sub = Task.Run(async () => {
                await barrier.SignalAndWait();
                var subSocket = factory.CreateSubscriber(url);
                await subSocket.Receive(CancellationToken.None);
            });
            
            await AssertWait(100000, pub, sub);
        }

        [Fact]
        public async Task BrokerTest()
        {
            await PubSubBroker(1, 1, 1);
        }

        async Task<List<Task>> Broker(int numPushers, int numPullers, int numMessagesPerPusher, AsyncCountdownEvent counter, CancellationToken token)
        {
            var inUrl = UrlRandomIpc();
            var outUrl = UrlRandomIpc();
            const int numBrokers = 1;

            var brokersReady = new AsyncBarrier(numBrokers + 1);
            var clientsReady = new AsyncBarrier(numPushers + numPullers + numBrokers);

            var numForwarded = 0;
            var tasks = new List<Task>();
            for (var i = 0; i < numBrokers; ++i)
            {
                var task = Task.Run(async () => {
                    using (var pullSocket = factory.CreatePuller(inUrl, true))
                    using (var pushSocket = factory.CreatePublisher(outUrl))
                    {
                        await brokersReady.SignalAndWait(); // Broker is ready
                        await clientsReady.SignalAndWait(); // Wait for clients
                        while (!token.IsCancellationRequested)
                        {
                            var msg = await pullSocket.Receive(token);
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
                    using (var pullSocket = factory.CreateSubscriber(outUrl))
                    {
                        await clientsReady.SignalAndWait(); // This client ready, wait for rest
                        while (!token.IsCancellationRequested)
                        {
                            var _ = await pullSocket.Receive(token);
                            counter.Signal();
                        }
                    }
                });
                tasks.Add(task);
            }
            
            return tasks;
        }

        async Task PubSubBroker(int numPublishers, int numSubscribers, int numMessagesPerSender, int msTimeout = 1000)
        {
            // In pub/sub pattern, each message is sent to every receiver
            int numTotalMessages = numPublishers * numSubscribers * numMessagesPerSender;
            var counter = new AsyncCountdownEvent(numTotalMessages);
            var cts = new CancellationTokenSource();

            var tasks = await Broker(numPublishers, numSubscribers, numMessagesPerSender, counter, cts.Token);

            await AssertWait(msTimeout, counter.WaitAsync());
            await CancelAndWait(cts, tasks.ToArray());
        }
    }
}
