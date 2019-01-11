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
        [ClassData(typeof(TransportsNoWsClassData))]
        public async Task Advanced(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoAdvanced(url);
            }
        }

        async Task DoAdvanced(string url)
        {
            var readyToDial = new AsyncBarrier(3);
            var readyToSend = new AsyncBarrier(3);
            var messageReceipt = new AsyncCountdownEvent(2);
            var cts = new CancellationTokenSource();
            var bus0Task = Task.Run(async () =>
            {
                using (var ctx = Factory.BusCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    await WaitShort();
                    await ctx.Send(Factory.CreateMessage());
                    await WaitShort();
                }
            });
            var bus1Task = Task.Run(async () =>
            {
                await readyToDial.SignalAndWait();
                using (var ctx = Factory.BusCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                }
            });
            var bus2Task = Task.Run(async () =>
            {
                await readyToDial.SignalAndWait();
                using (var ctx = Factory.BusCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                }
            });
            cts.CancelAfter(DefaultTimeoutMs);
            await Task.WhenAll(bus0Task, bus1Task, bus2Task);
            Assert.Equal(0, messageReceipt.Count);
        }
    }
}
