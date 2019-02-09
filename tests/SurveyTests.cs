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
                using (var bus0 = Factory.SurveyorOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var bus1 = Factory.RespondentOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
                {
                }

                // Manually create listener/dialer
                using (var bus0 = Factory.SurveyorOpen().Unwrap())
                using (var listener0 = bus0.ListenerCreate(url).Unwrap())
                {
                    using (var bus1 = Factory.RespondentOpen().Unwrap())
                    using (var dialer1 = bus1.DialerCreate(GetDialUrl(listener0, url)).Unwrap())
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
                using (var socket = Factory.SurveyorOpen().ThenListen(url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
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
                using (var socket = Factory.RespondentOpen().ThenListen(url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
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
            const int NumSurveyors = 1;
            const int NumResponders = 2;
            var readyToDial = new AsyncBarrier(NumSurveyors + NumResponders);
            var readyToSend = new AsyncBarrier(NumSurveyors + NumResponders);
            var numSurveyorReceive = new AsyncCountdownEvent(NumResponders);
            var numResponderReceive = new AsyncCountdownEvent(NumResponders);
            var cts = new CancellationTokenSource();
            var tasks = new List<Task>();
            var dialUrl = string.Empty;
            var task = Task.Run(async () =>
            {
                using (var socket = Factory.SurveyorOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    // Send survey and receive response
                    await WaitShort();
                    await ctx.Send(Factory.CreateMessage());
                    await WaitShort();
                    while (!cts.IsCancellationRequested)
                    {
                        var response = await ctx.Receive(cts.Token);
                        if (numSurveyorReceive.Signal() == 0)
                            break;
                    }
                }
            });
            tasks.Add(task);

            for (int i = 0; i < NumResponders; ++i)
            {
                task = Task.Run(async () =>
                {
                    await readyToDial.SignalAndWait();
                    using (var socket = Factory.RespondentOpen().ThenDial(dialUrl).Unwrap())
                    using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                    {
                        await readyToSend.SignalAndWait();
                        // Receive survey and send response
                        var survey = await ctx.Receive(cts.Token);
                        numResponderReceive.Signal();
                        (await ctx.Send(survey.Unwrap())).Unwrap();
                        await numSurveyorReceive.WaitAsync();
                    }
                });
                tasks.Add(task);
            }
            await Util.AssertWait(tasks);
            Assert.Equal(0, numSurveyorReceive.Count);
            Assert.Equal(0, numResponderReceive.Count);
        }

        // FIXME: Temporarily disabled.
        // Either NNG_OPT_RECVTIMEO or NNG_OPT_SURVEYOR_SURVEYTIME is being ignored so Surveyor receive times out
        // [Theory]
        // [ClassData(typeof(TransportsClassData))]
        public Task Contexts(string url)
        {
            return Fixture.TestIterate(() => DoContexts(url));
        }

        async Task DoContexts(string url)
        {
            const int NumSurveyors = 1;
            const int NumResponders = 2;
            var readyToDial = new AsyncBarrier(NumSurveyors + NumResponders);
            var readyToSend = new AsyncBarrier(NumSurveyors + NumResponders);
            var ready = readyToSend.WaitAsync();
            var numSurveyorReceive = new AsyncCountdownEvent(NumSurveyors);
            var numResponderReceive = new AsyncCountdownEvent(NumSurveyors);

            using (var surveySocket = Factory.SurveyorOpen().ThenListen(url).Unwrap())
            using (var respondSocket = Factory.RespondentOpen().ThenDial(url).Unwrap())
            {
                var duration = new nng_duration { TimeMs = DefaultTimeoutMs };
                // Send() is not cancelable so need it to timeout
                surveySocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});
                surveySocket.SetOpt(nng.Native.Defines.NNG_OPT_RECVTIMEO, nng_duration.Infinite);
                surveySocket.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, nng_duration.Infinite);
                respondSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});

                var cts = new CancellationTokenSource();
                var tasks = new List<Task>();
                for (var i = 0; i < NumSurveyors; ++i)
                {
                    var id = i;
                    var task = Task.Run(async () =>
                    {
                        using (var ctx = surveySocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            (ctx as ICtx).Ctx.SetOpt(Native.Defines.NNG_OPT_RECVTIMEO, nng_duration.Infinite);
                            (ctx as ICtx).Ctx.SetOpt(Native.Defines.NNG_OPT_SURVEYOR_SURVEYTIME, nng_duration.Infinite);

                            await readyToDial.SignalAndWait();
                            await readyToSend.SignalAndWait();

                            // Send survey and receive responses
                            var survey = Factory.CreateMessage();
                            var val = (uint)rng.Next();
                            survey.Append(val);
                            //Assert.Equal(0, survey.Header.Append((uint)(0x8000000 | i))); // Protocol header contains "survey ID"
                            (await ctx.Send(survey)).Unwrap();
                            while (!cts.IsCancellationRequested)
                            {
                                try
                                {
                                    var response = (await ctx.Receive(cts.Token)).Unwrap();
                                    response.Trim(out uint respVal);
                                    Assert.Equal(val, respVal);
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
                    var id = i;
                    var task = Task.Run(async () =>
                    {
                        await readyToDial.SignalAndWait();
                        using (var ctx = respondSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            // Receive survey and send response
                            try
                            {
                                // Receive is async, give it a chance to start before signaling we are ready.
                                // This to avoid race where surveyor sends before it actually starts receiving
                                var recvFuture = ctx.Receive(cts.Token);
                                await WaitShort();
                                await readyToSend.SignalAndWait();
                                var survey = (await recvFuture).Unwrap();
                                await Task.Delay(10); // Make sure surveyor has a chance to start receiving
                                (await ctx.Send(survey)).Unwrap();
                                numResponderReceive.Signal();
                                await numSurveyorReceive.WaitAsync();
                                cts.Cancel(); // Cancel any responders still receiving
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
                await Task.WhenAny(ready, Task.WhenAll(tasks));
                await Util.CancelAfterAssertwait(tasks, cts);
                Assert.Equal(0, numSurveyorReceive.Count);
                Assert.Equal(0, numResponderReceive.Count);
            }
        }
    }
}
