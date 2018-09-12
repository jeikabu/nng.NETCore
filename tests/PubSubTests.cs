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
    public class PubSubTests
    {
        IFactory<IMessage> factory;

        public PubSubTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        // [Fact]
        // public async Task BasicPubSub()
        // {
        //     var url = UrlRandomIpc();
        //     var pub = PubSocket<NngMessage>.Create(url);
        //     await Task.Delay(100);
        //     var sub = SubSocket<NngMessage>.Create(url);
        //     var topic = System.Text.Encoding.ASCII.GetBytes("topic");
        //     Assert.True(sub.Subscribe(topic));
        //     var ret = nng_msg_alloc(out var msg, 0);
        //     ret = nng_msg_append(msg, topic);
        //     ret = nng_sendmsg(pub.NngSocket, msg, 0);
        //     ret = nng_recvmsg(sub.NngSocket, out var recv, 0);
        // }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task BasicPubSub(string url)
        {
            var pub = factory.CreatePublisher(url);
            await WaitReady();
            var sub = factory.CreateSubscriber(url);
            var topic = TopicRandom();
            Assert.Equal(0, sub.Subscribe(topic));
            var msg = factory.CreateMessage();
            msg.Append(topic);
            var sendTask = pub.Send(msg);
            var resvTask = sub.Receive(CancellationToken.None);
            await AssertWait(1000, sendTask, resvTask);
        }

        // [Theory]
        // [ClassData(typeof(IpcTransportClassData))]
        // public async Task PubSub(string url)
        // {
        //     var topic = TopicRandom();
        //     for (int i = 0; i < 10; ++i)
        //     {
        //     await Task.Delay(50);
        //     var serverReady = new AsyncBarrier(2);
        //     var clientReady = new AsyncBarrier(2);
        //     var pubTask = Task.Run(async () => {
        //         using (var pubSocket = factory.CreatePublisher(url))
        //         {
        //             await serverReady.SignalAndWait();
        //             await clientReady.SignalAndWait();
        //             await WaitReady();
        //             Assert.True(await pubSocket.Send(factory.CreateTopicMessage(topic)));
        //             await Task.Delay(10);
        //         }
        //     });
        //     var subTask = Task.Run(async () => {
        //         await serverReady.SignalAndWait();
        //         using (var sub = factory.CreateSubscriber(url))
        //         {
        //             sub.Subscribe(topic);
        //             await clientReady.SignalAndWait();
        //             await sub.Receive(CancellationToken.None);
        //             await Task.Delay(10);
        //         }
        //     });
        //     await AssertWait(100000, subTask, subTask);
        //     }
        // }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task BrokerTest(string url)
        {
            await PubSubBrokerAsync(1, 1, 1);
        }

        async Task PubSubBrokerAsync(int numPublishers, int numSubscribers, int numMessagesPerSender, int msTimeout = 1000)
        {
            // In pub/sub pattern, each message is sent to every receiver
            int numTotalMessages = numPublishers * numSubscribers * numMessagesPerSender;
            var counter = new AsyncCountdownEvent(numTotalMessages);
            var cts = new CancellationTokenSource();

            var broker = new Broker(new PubSubBrokerImpl(factory));
            var tasks = await broker.RunAsync(numPublishers, numSubscribers, numMessagesPerSender, counter, cts.Token);

            await AssertWait(msTimeout, counter.WaitAsync());
            await CancelAndWait(cts, tasks.ToArray());
        }
    }

    
    class PubSubBrokerImpl : IBrokerImpl<IMessage>
    {
        public IFactory<IMessage> Factory { get; private set; }

        public PubSubBrokerImpl(IFactory<IMessage> factory)
        {
            Factory = factory;
            topic = TopicRandom();
        }

        public IReceiveAsyncContext<IMessage> CreateInSocket(string url)
        {
            return Factory.CreatePuller(url, true);
        }
        public ISendAsyncContext<IMessage> CreateOutSocket(string url)
        {
            return Factory.CreatePublisher(url);
        }
        public IReceiveAsyncContext<IMessage> CreateClient(string url)
        {
            var sub = Factory.CreateSubscriber(url);
            sub.Subscribe(topic);
            return sub;
        }

        public IMessage CreateMessage()
        {
            return Factory.CreateTopicMessage(topic);
        }

        byte[] topic;
    }
}
