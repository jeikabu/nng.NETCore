using nng.Native;
using System;
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
                await pullSocket.Receive();
            });
            
            await AssertWait(1000, pull, push);
        }

        [Fact]
        public async Task Broker()
        {
            var inUrl = UrlRandomIpc();
            var outUrl = UrlRandomIpc();
            
            var brokerReady = new AsyncBarrier(3);
            var clientsReady = new AsyncBarrier(3);
            var broker = Task.Run(async () => {
                var pullSocket = PullSocket.CreateAsyncContext(inUrl, true) as PullAsyncCtx;
                var pushSocket = PushSocket.CreateAsyncContext(outUrl, true) as PushAsyncCtx;
                await brokerReady.SignalAndWait(); // Broker is ready
                await clientsReady.SignalAndWait(); // Wait for clients
                var msg = await pullSocket.Receive();
                await pushSocket.Send(msg);
            });
            var sender = Task.Run(async () => {
                await brokerReady.SignalAndWait(); // Wait for broker
                var pushSocket = PushSocket.CreateAsyncContext(inUrl, false) as PushAsyncCtx;
                await clientsReady.SignalAndWait(); // This client ready, wait for rest
                await pushSocket.Send(CreateMsg());
            });
            var receiver = Task.Run(async () => {
                await brokerReady.SignalAndWait(); // Wait for broker
                var pullSocket = PullSocket.CreateAsyncContext(outUrl, false) as PullAsyncCtx;
                await clientsReady.SignalAndWait(); // This client ready, wait for rest
                var msg = await pullSocket.Receive();
                Console.WriteLine(msg);
            });
            await AssertWait(4000, broker, sender, receiver);
        }
    }
}
