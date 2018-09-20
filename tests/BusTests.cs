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
    public class BusTests
    {
        IAPIFactory<IMessage> factory;

        public BusTests(NngCollectionFixture collectionFixture)
        {
            this.factory = collectionFixture.Factory;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Basic(string url)
        {
            const int numIterations = 10;
            for (int i = 0; i < numIterations; ++i)
            {
                using (var bus0 = factory.BusCreate(url, true).Unwrap())
                {
                    using (var bus1 = factory.BusCreate(url, false).Unwrap())
                    {

                    }
                }

                // Manually create listener/dialer
                using (var bus0 = factory.BusOpen().Unwrap())
                using (var listener0 = factory.ListenerCreate(bus0, url))
                {
                    using (var bus1 = factory.BusOpen().Unwrap())
                    using (var dialer1 = factory.DialerCreate(bus1, url))
                    {

                    }
                }
            }
        }

        [Theory]
        [ClassData(typeof(TransportsNoWsClassData))]
        public async Task Advanced(string url)
        {
            int numIterations = 10;
            int numOk = 0;
            for (int i = 0; i < numIterations; ++i)
            {
                if (await DoAdvanced(url))
                {
                    ++numOk;
                }
            }
            Assert.InRange((float)numOk/numIterations, 0.69, 1.0);
        }

        async Task<bool> DoAdvanced(string url)
        {
            var readyToDial = new AsyncBarrier(3);
            var readyToSend = new AsyncBarrier(3);
            var messageReceipt = new AsyncCountdownEvent(2);
            var cts = new CancellationTokenSource();
            var bus0Task = Task.Run(async () => {
                using (var ctx = factory.BusCreate(url, true).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    //await WaitReady();
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    //await WaitReady();
                    await ctx.Send(factory.CreateMessage());
                }
            });
            var bus1Task = Task.Run(async () => {
                await readyToDial.SignalAndWait();
                using (var ctx = factory.BusCreate(url, false).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                }
            });
            var bus2Task = Task.Run(async () => {
                await readyToDial.SignalAndWait();
                using (var ctx = factory.BusCreate(url, false).Unwrap().CreateAsyncContext(factory).Unwrap())
                {
                    await readyToSend.SignalAndWait();
                    var _ = await ctx.Receive(cts.Token);
                    messageReceipt.Signal();
                }
            });
            cts.CancelAfter(DefaultTimeoutMs);
            return await Util.WhenAll(DefaultTimeoutMs, messageReceipt.WaitAsync(), bus0Task, bus1Task, bus2Task);
        }
    }
}
