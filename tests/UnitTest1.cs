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
            var barrier = new Barrier(2);
            var rep = Task.Run(async () => {
                var repAioCtx = CreateRepAsyncCtx(UrlIpcTest);

                barrier.SignalAndWait();

                var msg = await repAioCtx.Receive();
                Assert.True(await repAioCtx.Reply(CreateMsg()));
            }).ContinueWith(task => {
                Assert.False(task.IsFaulted);
            });
            var req = Task.Run(async () => {
                barrier.SignalAndWait();
                var reqAioCtx = CreateReqAsyncCtx(UrlIpcTest);
                var response = await reqAioCtx.Send(CreateMsg());
                //Assert.NotNull(response);
            }).ContinueWith(task => {
                Assert.False(task.IsFaulted);
            });
            var timeout = Task.Delay(1000);
            Assert.NotEqual(timeout, await Task.WhenAny(timeout, Task.WhenAll(rep, req)));
        }
    }
}
