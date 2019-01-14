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

    [Collection("nng")]
    public class SurveyTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public SurveyTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Basic(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                using (var bus0 = Factory.SurveyorCreate(url, true).Unwrap())
                using (var bus1 = Factory.RespondentCreate(url, false).Unwrap())
                {
                }

                // Manually create listener/dialer
                using (var bus0 = Factory.SurveyorOpen().Unwrap())
                using (var listener0 = Factory.ListenerCreate(bus0, url))
                {
                    using (var bus1 = Factory.RespondentOpen().Unwrap())
                    using (var dialer1 = Factory.DialerCreate(bus1, url))
                    {

                    }
                }
            }
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task SurveyorFail(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoSurveyorFail(url);
            }
        }

        async Task DoSurveyorFail(string url)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(DefaultTimeoutMs);
            var task = Task.Run(async () =>
            {
                using (var ctx = Factory.SurveyorCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    // Receive with no survey fails
                    await Util.AssertThrowsNng(() => ctx.Receive(cts.Token), Defines.NngErrno.ESTATE);
                    // Survey with no responses times out
                    var asyncctx = (ICtx)ctx;
                    asyncctx.SetCtxOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, new nng_duration { TimeMs = 10 });
                    // NB: when using nng_ctx must call ctx_setopt instead of (socket) setopt
                    //ctx.Socket.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, new nng_duration { TimeMs = 10 });
                    await ctx.Send(Factory.CreateMessage());
                    await Util.AssertThrowsNng(() => ctx.Receive(cts.Token), Defines.NngErrno.ETIMEDOUT);
                }
            });
            await Util.AssertWait(DefaultTimeoutMs, task);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task RespondentFail(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoRespondentFail(url);
            }
        }

        async Task DoRespondentFail(string url)
        {
            var task = Task.Run(async () =>
            {
                using (var ctx = Factory.RespondentCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    // Response with no survey fails
                    await Util.AssertThrowsNng(() => ctx.Send(Factory.CreateMessage()), Defines.NngErrno.ESTATE);
                }
            });
            await Util.AssertWait(DefaultTimeoutMs, task);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Advanced(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoAdvanced(url);
            }
        }

        async Task DoAdvanced(string url)
        {
            var readyToDial = new AsyncBarrier(2);
            var readyToSend = new AsyncBarrier(2);
            var messageReceipt = new AsyncCountdownEvent(2);
            var cts = new CancellationTokenSource();
            var surveyorTask = Task.Run(async () =>
            {
                using (var ctx = Factory.SurveyorCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    // Send survey and receive response
                    await WaitShort();
                    await ctx.Send(Factory.CreateMessage());
                    await WaitShort();
                    var response = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                }
            });
            var respondentTask = Task.Run(async () =>
            {
                await readyToDial.SignalAndWait();
                using (var ctx = Factory.RespondentCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await readyToSend.SignalAndWait();
                    // Receive survey and send response
                    var survey = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                    await ctx.Send(survey);
                }
            });
            cts.CancelAfter(DefaultTimeoutMs);
            await Util.AssertWait(DefaultTimeoutMs, surveyorTask, respondentTask);
            Assert.Equal(0, messageReceipt.Count);
        }
    }
}
