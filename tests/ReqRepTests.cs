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
                (await repAioCtx.Reply(Factory.CreateMessage())).Unwrap();
                var _response = await asyncReq;
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task ReqRepTasks(string url)
        {
            return Fixture.TestIterate(() => DoReqRep(url));
        }

        Task DoReqRep(string url)
        {
            var barrier = new AsyncBarrier(2);
            var rep = Task.Run(async () =>
            {
                using (var socket = Factory.ReplierCreate(url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    await barrier.SignalAndWait();

                    var msg = await ctx.Receive();
                    (await ctx.Reply(Factory.CreateMessage())).Unwrap();
                    await WaitShort();
                }
            });
            var req = Task.Run(async () =>
            {
                await barrier.SignalAndWait();
                using (var socket = Factory.RequesterCreate(url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    var _response = await ctx.Send(Factory.CreateMessage());
                }
            });
            return Util.AssertWait(req, rep);
        }
    }
}
