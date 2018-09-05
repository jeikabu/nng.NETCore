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
    }
}
