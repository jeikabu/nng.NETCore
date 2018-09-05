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

    public class ReqRepTests
    {
        RepAsyncCtx CreateRepAsyncCtx(string url)
        {
            var repAioCtx = RepSocket.CreateAsyncContext(url) as RepAsyncCtx;
            Assert.NotNull(repAioCtx);
            return repAioCtx;
        }

        ReqAsyncCtx CreateReqAsyncCtx(string url)
        {
            var reqAioCtx = ReqSocket.CreateAsyncContext(url) as ReqAsyncCtx;
            Assert.NotNull(reqAioCtx);
            return reqAioCtx;
        }

        nng_msg CreateMsg()
        {
            Assert.Equal(0, nng_msg_alloc(out var msg, 32));
            return msg;
        }

        async Task AssertWait(int timeoutMs, params Task[] tasks)
        {
            var timeout = Task.Delay(timeoutMs);
            Assert.NotEqual(timeout, await Task.WhenAny(timeout, Task.WhenAll(tasks)));
        }

        [Fact]
        public void Test1()
        {
            Assert.Equal(0, nng_aio_alloc(out var aio, null, IntPtr.Zero));
            nng_aio_free(aio);
        }

        [Fact]
        public async Task ReqRepBasic()
        {
            var url = UrlRandomIpc();
            var repAioCtx = CreateRepAsyncCtx(url);
            var reqAioCtx = CreateReqAsyncCtx(url);

            var asyncReq = reqAioCtx.Send(CreateMsg());
            var receivedReq = await repAioCtx.Receive();
            var asyncRep = repAioCtx.Reply(CreateMsg());
            var response = await asyncReq;
        }

        [Fact]
        public async Task ReqRepTasks()
        {
            var url = UrlRandomIpc();
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                var repAioCtx = CreateRepAsyncCtx(url);

                await barrier.SignalAndWait();

                var msg = await repAioCtx.Receive();
                Assert.True(await repAioCtx.Reply(CreateMsg()));
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                var reqAioCtx = CreateReqAsyncCtx(url);
                var response = await reqAioCtx.Send(CreateMsg());
                //Assert.NotNull(response);
            });
            await AssertWait(1000, rep, req);
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
            
            await AssertWait(10000, pull, push);
        }

        [Fact]
        public async Task PushPullBroker()
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
                var msg = await pullSocket.Receive(CancellationToken.None);
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
                var msg = await pullSocket.Receive(CancellationToken.None);
                Console.WriteLine(msg);
            });
            await AssertWait(4000, broker, sender, receiver);
        }
    }
}
