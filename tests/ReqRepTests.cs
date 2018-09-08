using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;

    public class ReqRepTests
    {
        IFactory<NngMessage> factory;

        [Fact]
        public async Task ReqRepBasic()
        {
            var url = UrlRandomIpc();
            var repAioCtx = factory.CreateReplier(url);
            var reqAioCtx = factory.CreateRequester(url);

            var asyncReq = reqAioCtx.Send(factory.CreateMessage());
            var receivedReq = await repAioCtx.Receive();
            var asyncRep = repAioCtx.Reply(factory.CreateMessage());
            var response = await asyncReq;
        }

        [Fact]
        public async Task ReqRepTasks()
        {
            var url = UrlRandomIpc();
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                var repAioCtx = factory.CreateReplier(url);

                await barrier.SignalAndWait();

                var msg = await repAioCtx.Receive();
                Assert.True(await repAioCtx.Reply(factory.CreateMessage()));
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                var reqAioCtx = factory.CreateRequester(url);
                var response = await reqAioCtx.Send(factory.CreateMessage());
                //Assert.NotNull(response);
            });
            await AssertWait(1000, rep, req);
        }
    }
}
