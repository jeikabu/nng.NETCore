using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Native.Aio.UnsafeNativeMethods;
    using static nng.Native.Msg.UnsafeNativeMethods;

    public class AioTests
    {
        const string UrlIpcTest = "ipc://test";

        RepAsyncCtx CreateRepAsyncCtx(string url)
        {
            var repSocket = RepSocket.Create(UrlIpcTest) as RepSocket;
            Assert.NotNull(repSocket);
            var repAioCtx = repSocket.CreateAioCtx() as RepAsyncCtx;
            Assert.NotNull(repAioCtx);
            return repAioCtx;
        }

        ReqAsyncCtx CreateReqAsyncCtx(string url)
        {
            var reqSocket = ReqSocket.Create(UrlIpcTest) as ReqSocket;
            Assert.NotNull(reqSocket);
            var reqAioCtx = reqSocket.CreateAioCtx() as ReqAsyncCtx;
            Assert.NotNull(reqAioCtx);

            return reqAioCtx;
        }

        nng_msg CreateMsg()
        {
            Assert.Equal(0, nng_msg_alloc(out var msg, 32));
            return msg;
        }

        async Task WaitAssert(int timeoutMs, params Task[] tasks)
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
            var repAioCtx = CreateRepAsyncCtx(UrlIpcTest);
            var reqAioCtx = CreateReqAsyncCtx(UrlIpcTest);

            var asyncReq = reqAioCtx.Send(CreateMsg());
            var receivedReq = await repAioCtx.Receive();
            var asyncRep = repAioCtx.Reply(CreateMsg());
            var response = await asyncReq;
        }

        [Fact]
        public async Task ReqRepTasks()
        {
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                var repAioCtx = CreateRepAsyncCtx(UrlIpcTest);

                await barrier.SignalAndWait();

                var msg = await repAioCtx.Receive();
                Assert.True(await repAioCtx.Reply(CreateMsg()));
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                var reqAioCtx = CreateReqAsyncCtx(UrlIpcTest);
                var response = await reqAioCtx.Send(CreateMsg());
                //Assert.NotNull(response);
            });
            await WaitAssert(1000, rep, req);
        }
    }
}
