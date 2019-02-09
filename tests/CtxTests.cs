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
                var ctx = (rep as ICtx).Ctx;
                // Get a value, set a new value, get back the new value
                Assert.Equal(0, ctx.GetOpt(NNG_OPT_RECVTIMEO, out nng_duration recvTimeout));
                var newResvTimeout = new nng_duration(recvTimeout);
                ++newResvTimeout.TimeMs;
                Assert.Equal(0, ctx.SetOpt(NNG_OPT_RECVTIMEO, newResvTimeout));
                ctx.GetOpt(NNG_OPT_RECVTIMEO, out nng_duration nextRecvTimeout);
                Assert.Equal(newResvTimeout.TimeMs, nextRecvTimeout.TimeMs);
            }
        }
    }
}