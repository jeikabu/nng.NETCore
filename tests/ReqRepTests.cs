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
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public ReqRepTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task ReqRepBasic(string url)
        {
            using (var repAioCtx = Factory.ReplierCreate(url).Unwrap().CreateAsyncContext(Factory).Unwrap())
            using (var reqAioCtx = Factory.RequesterCreate(url).Unwrap().CreateAsyncContext(Factory).Unwrap())
            {
                var receiveTask = repAioCtx.Receive();
                var asyncReq = reqAioCtx.Send(Factory.CreateMessage());
                var _receivedReq = await receiveTask;
                Assert.True(await repAioCtx.Reply(Factory.CreateMessage()));
                var _response = await asyncReq;
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task ReqRepTasks(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoReqRep(url);
            }
        }

        Task DoReqRep(string url)
        {
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () => {
                using (var repAioCtx = Factory.ReplierCreate(url).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await barrier.SignalAndWait();

                    var msg = await repAioCtx.Receive();
                    Assert.True(await repAioCtx.Reply(Factory.CreateMessage()));
                    await WaitShort();
                }
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                using (var reqAioCtx = Factory.RequesterCreate(url).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    var response = await reqAioCtx.Send(Factory.CreateMessage());
                    //Assert.NotNull(response);
                }
            });
            return Task.WhenAll(rep, req);
        }
    }
}
