using nng.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;

    [Collection("nng")]
    public class ReqRepTests
    {
        IAPIFactory<IMessage> factory;

        public ReqRepTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task ReqRepBasic(string url)
        {
            using (var repAioCtx = factory.ReplierCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap())
            using (var reqAioCtx = factory.RequesterCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap())
            {
                var receiveTask = repAioCtx.Receive();
                var asyncReq = reqAioCtx.Send(factory.CreateMessage());
                var _receivedReq = await receiveTask;
                Assert.True(await repAioCtx.Reply(factory.CreateMessage()));
                var _response = await asyncReq;
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task ReqRepTasks(string url)
        {
            const int numIterations = 10;
            int numOk = 0;
            for (int i = 0; i < numIterations; ++i)
            {
                if (await DoReqRep(url))
                {
                    ++numOk;
                }
            }
            Assert.InRange((float)numOk/numIterations, 0.7, 1.0);
        }

        async Task<bool> DoReqRep(string url)
        {
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                using (var repAioCtx = factory.ReplierCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    await barrier.SignalAndWait();

                    var msg = await repAioCtx.Receive();
                    Assert.True(await repAioCtx.Reply(factory.CreateMessage()));
                }
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                using (var reqAioCtx = factory.RequesterCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    var response = await reqAioCtx.Send(factory.CreateMessage());
                    //Assert.NotNull(response);
                }
            });
            return await Util.WhenAll(DefaultTimeoutMs, rep, req);
        }
    }
}
