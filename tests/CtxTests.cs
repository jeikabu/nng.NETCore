using nng.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;
    using static nng.Native.Defines;

    [Collection("nng")]
    public class CtxTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public CtxTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Fact]
        public void GetSetOpt()
        {
            var url = UrlIpc();
            using (var socket = Factory.ReplierOpen().ThenListen(url).Unwrap())
            using (var rep = socket.CreateAsyncContext(Factory).Unwrap())
            {
                // TODO: remove this after deprecating ICtx
                var _remove = (rep as ICtx).Ctx;
                // Get a value, set a new value, get back the new value
                Assert.Equal(0, rep.Ctx.GetOpt(NNG_OPT_RECVTIMEO, out nng_duration recvTimeout));
                var newResvTimeout = new nng_duration(recvTimeout);
                ++newResvTimeout.TimeMs;
                Assert.Equal(0, rep.Ctx.SetOpt(NNG_OPT_RECVTIMEO, newResvTimeout));
                rep.Ctx.GetOpt(NNG_OPT_RECVTIMEO, out nng_duration nextRecvTimeout);
                Assert.Equal(newResvTimeout.TimeMs, nextRecvTimeout.TimeMs);
            }
        }

        [Fact]
        public async Task MustUseCtxGetSetOpt()
        {
            var url = UrlIpc();

            // Setting receive timeout on nng_ctx works
            using (var socket = Factory.ReplierOpen().ThenListen(url).Unwrap())
            using (var rep = socket.CreateAsyncContext(Factory).Unwrap())
            {
                Assert.Equal(0, rep.Ctx.SetOpt(NNG_OPT_RECVTIMEO, nng_duration.Zero));
                Assert.True((await rep.Receive()).Err() == NngErrno.ECLOSED);
            }

            // Setting receive timeout on socket doesn't timeout read from nng_ctx
            var task = Task.Run(async () => {
                using (var socket = Factory.ReplierOpen().ThenListen(url).Unwrap())
                using (var rep = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    Assert.Equal(0, rep.Socket.SetOpt(NNG_OPT_RECVTIMEO, nng_duration.Zero));
                    (await rep.Receive()).Unwrap();
                }
            });
            var timeout = Task.Delay(Util.ShortTestMs);
            var first = await Task.WhenAny(Task.WhenAll(task), timeout);
            Assert.Equal(timeout, first);
        }
    }
}