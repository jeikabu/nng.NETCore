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
            var repAioCtx = factory.ReplierCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap();
            var reqAioCtx = factory.RequesterCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap();

            var asyncReq = reqAioCtx.Send(factory.CreateMessage());
            var receivedReq = await repAioCtx.Receive();
            var asyncRep = repAioCtx.Reply(factory.CreateMessage());
            var response = await asyncReq;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task ReqRepTasks(string url)
        {
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                var repAioCtx = factory.ReplierCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap();

                await barrier.SignalAndWait();

                var msg = await repAioCtx.Receive();
                Assert.True(await repAioCtx.Reply(factory.CreateMessage()));
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                var reqAioCtx = factory.RequesterCreate(url).Unwrap().CreateAsyncContext(factory).Unwrap();
                var response = await reqAioCtx.Send(factory.CreateMessage());
                //Assert.NotNull(response);
            });
            await AssertWait(1000, rep, req);
        }
    }
}
