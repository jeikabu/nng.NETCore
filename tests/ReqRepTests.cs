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
        IAPIFactory<INngMsg> Factory => Fixture.Factory;

        public ReqRepTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task ReqRepBasic(string url)
        {
            return Fixture.TestIterate(() => DoReqRepBasic(url));
        }

        async Task DoReqRepBasic(string url)
        {
            using (var repSocket = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var reqSocket = Factory.RequesterOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                Util.SetTestOptions(reqSocket);
                Util.SetTestOptions(repSocket);
                reqSocket.SendMsg(Factory.CreateMessage()).Unwrap();
                var msg = repSocket.RecvMsg().Unwrap();
                repSocket.SendMsg(msg).Unwrap();
                var _ = reqSocket.RecvMsg().Unwrap();
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task ReqRepAsyncBasic(string url)
        {
            return Fixture.TestIterate(() => DoReqRepAsyncBasic(url));
        }

        async Task DoReqRepAsyncBasic(string url)
        {
            using (var repSocket = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var repAioCtx = repSocket.CreateAsyncContext(Factory).Unwrap())
            using (var reqSocket = Factory.RequesterOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            using (var reqAioCtx = reqSocket.CreateAsyncContext(Factory).Unwrap())
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
            var dialUrl = string.Empty;
            var rep = Task.Run(async () =>
            {
                using (var socket = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await barrier.SignalAndWait();

                    var msg = await ctx.Receive();
                    (await ctx.Reply(Factory.CreateMessage())).Unwrap();
                    await WaitShort();
                }
            });
            var req = Task.Run(async () =>
            {
                await barrier.SignalAndWait();
                using (var socket = Factory.RequesterOpen().ThenDial(dialUrl).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    var _response = await ctx.Send(Factory.CreateMessage());
                }
            });
            return Util.AssertWait(req, rep);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public void SendRecvAlloc(string url)
        {
            Fixture.TestIterate(() => DoSendRecvAlloc(url));
        }

        void DoSendRecvAlloc(string url)
        {
            using (var rep = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var req = Factory.RequesterOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                var msg = Factory.CreateAlloc(128);
                rng.NextBytes(msg.AsSpan());
                req.SendZeroCopy(msg).Unwrap();
                var request = rep.RecvZeroCopy().Unwrap();
                rep.SendZeroCopy(request).Unwrap();
                var reply = req.RecvZeroCopy().Unwrap();
                Util.Equals(msg, reply);
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task SendRecvNonBlock(string url)
        {
            return Fixture.TestIterate(() => DoSendRecvNonBlock(url));
        }

        async Task DoSendRecvNonBlock(string url)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Util.ShortTestMs);
            using (var rep = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var req = Factory.RequesterOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                var msg = Factory.CreateMessage();
                var flags = Defines.NngFlag.NNG_FLAG_NONBLOCK;
                var res = await RetryAgain(cts, () => req.SendMsg(msg, flags));
                res.Unwrap();
                var request = (await RetryAgain(cts, () => rep.RecvMsg(flags))).Unwrap();
                res = await RetryAgain(cts, () => rep.SendMsg(request, flags));

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    var reply = await RetryAgain(cts, () => req.RecvMsg(flags));
                    reply.Unwrap();
                }
                else
                {
                    // NOTE: Using NB req/rep from same thread seems to not work
                    Assert.Equal(Defines.NngErrno.EAGAIN, res.Err());
                }
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task SendRecvNonBlockTasks(string url)
        {
            return Fixture.TestIterate(() => DoSendRecvNonBlockTasks(url));
        }

        async Task DoSendRecvNonBlockTasks(string url)
        {
            var dialUrl = String.Empty;
            var barrier = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var flags = Defines.NngFlag.NNG_FLAG_NONBLOCK;

            var rep = Task.Run(async () => {
                using (var rep = Factory.ReplierOpen().ThenListenAs(out var listener, url).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await barrier.SignalAndWait();
                    var msg = Factory.CreateMessage();
                    var request = (await RetryAgain(cts, () => rep.RecvMsg(flags))).Unwrap();
                    var res = await RetryAgain(cts, () => rep.SendMsg(request, flags));
                    res.Unwrap();
                }
            });
            var req = Task.Run(async () => {
                await barrier.SignalAndWait();
                using (var req = Factory.RequesterOpen().ThenDial(dialUrl).Unwrap())
                {
                    var msg = Factory.CreateMessage();
                    var res = await RetryAgain(cts, () => req.SendMsg(msg, flags));
                    res.Unwrap();
                    var reply = await RetryAgain(cts, () => req.RecvMsg(flags));
                    var _ = reply.Unwrap();
                }
            });
            await Util.CancelAfterAssertwait(cts, req, rep);
        }
    }
}
