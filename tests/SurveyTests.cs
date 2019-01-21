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
            Fixture.TestIterate(() =>
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
            });
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task SurveyorFail(string url)
        {
            await Fixture.TestIterate(() => DoSurveyorFail(url));
        }

        async Task DoSurveyorFail(string url)
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                using (var ctx = Factory.SurveyorCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    // Receive with no survey fails
                    await Util.AssertThrowsNng(() => ctx.Receive(cts.Token), Defines.NngErrno.ESTATE);
                    // Survey with no responses times out
                    var asyncctx = (ctx as ICtx).Ctx;
                    // NB: when using nng_ctx must call ctx_setopt instead of (socket) setopt
                    asyncctx.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, new nng_duration { TimeMs = 10 });
                    //ctx.Socket.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, new nng_duration { TimeMs = 10 });
                    await ctx.Send(Factory.CreateMessage());
                    await Util.AssertThrowsNng(() => ctx.Receive(cts.Token), Defines.NngErrno.ETIMEDOUT);
                }
            });
            await CancelAfterAssertwait(cts, task);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task RespondentFail(string url)
        {
            await Fixture.TestIterate(() => DoRespondentFail(url));
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
            await Util.AssertWait(task);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Advanced(string url)
        {
            await Fixture.TestIterate(() => DoAdvanced(url));
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
                    await ctx.Send(survey.Unwrap());
                }
            });
            await CancelAfterAssertwait(cts, surveyorTask, respondentTask);
            Assert.Equal(0, messageReceipt.Count);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task Contexts(string url)
        {
            return Fixture.TestIterate(() => DoContexts(url));
        }

        async Task DoContexts(string url)
        {
            const int NumSurveyors = 1;
            const int NumResponders = 3;
            var readyToDial = new AsyncBarrier(NumSurveyors + NumResponders);
            var readyToSend = new AsyncBarrier(NumSurveyors + NumResponders);
            var numSurveyorReceive = new AsyncCountdownEvent(NumSurveyors);
            var numResponderReceive = new AsyncCountdownEvent(NumSurveyors);

            using (var surveySocket = Factory.SurveyorCreate(url, true).Unwrap())
            using (var respondSocket = Factory.RespondentCreate(url, false).Unwrap())
            {
                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();
                for (var i = 0; i < NumSurveyors; ++i)
                {
                    var task = Task.Run(async () =>
                    {
                        using (var ctx = surveySocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            (ctx as ICtx).Ctx.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, new nng_duration { TimeMs = DefaultTimeoutMs });

                            await readyToDial.SignalAndWait();
                            await readyToSend.SignalAndWait();

                            // Send survey and receive responses
                            var survey = Factory.CreateMessage();
                            //Assert.Equal(0, survey.Header.Append((uint)(0x8000000 | i))); // Protocol header contains "survey ID"
                            await ctx.Send(survey);
                            while (!cts.IsCancellationRequested)
                            {
                                try
                                {
                                    var response = await ctx.Receive(cts.Token);
                                    if (numSurveyorReceive.Signal() == 0)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine(ex.ToString());
                                    throw ex;
                                }
                            }
                        }
                    });
                    tasks.Add(task);
                }

                for (var i = 0; i < NumResponders; ++i)
                {
                    var task = Task.Run(async () =>
                    {
                        await readyToDial.SignalAndWait();
                        using (var ctx = respondSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            await readyToSend.SignalAndWait();
                            try
                            {
                                // Receive survey and send response
                                var survey = await ctx.Receive(cts.Token);
                                (await ctx.Send(survey.Unwrap())).Unwrap();
                                numResponderReceive.Signal();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                throw ex;
                            }
                        }
                    });
                    tasks.Add(task);
                }

                await Util.CancelAfterAssertwait(tasks, cts);
                Assert.Equal(0, numSurveyorReceive.Count);
                Assert.Equal(0, numResponderReceive.Count);
            }
        }
    }
}
