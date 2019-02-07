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
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public BusTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public async Task Basic(string url)
        {
            Fixture.TestIterate(() =>
            {
                using (var bus0 = Factory.BusOpen().Unwrap())
                using (var bus1 = Factory.BusOpen().Unwrap())
                {
                    var listener = bus0.ListenWithListener(url).Unwrap();
                    bus1.Dial(GetDialUrl(listener, url)).Unwrap();
                }

                // Manually create listener/dialer
                using (var bus0 = Factory.BusOpen().Unwrap())
                using (var listener0 = bus0.ListenerCreate(url).Unwrap())
                {
                    using (var bus1 = Factory.BusOpen().Unwrap())
                    using (var dialer1 = bus1.DialerCreate(GetDialUrl(listener0, url)).Unwrap())
                    {

                    }
                }
            });
        }

        [Theory]
        [ClassData(typeof(TransportsNoWsClassData))]
        public async Task Advanced(string url)
        {
            await Fixture.TestIterate(() => DoAdvanced(url));
        }

        async Task DoAdvanced(string url)
        {
            var readyToDial = new AsyncBarrier(3);
            var readyToSend = new AsyncBarrier(3);
            var messageReceipt = new AsyncCountdownEvent(2);
            var cts = new CancellationTokenSource();
            var dialUrl = string.Empty;
            var bus0Task = Task.Run(async () =>
            {
                using (var socket = Factory.BusOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await readyToDial.SignalAndWait();
                    await readyToSend.SignalAndWait();
                    await ctx.Send(Factory.CreateMessage());
                    await WaitShort();
                }
            });
            var bus1Task = Task.Run(async () =>
            {
                await readyToDial.SignalAndWait();
                using (var socket = Factory.BusOpen().ThenDial(dialUrl).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    // Make sure receive has started before signalling ready
                    var recvFuture = ctx.Receive(cts.Token);
                    await WaitShort();
                    await readyToSend.SignalAndWait();
                    var _ = await recvFuture;
                    messageReceipt.Signal();
                }
            });
            var bus2Task = Task.Run(async () =>
            {
                await readyToDial.SignalAndWait();
                using (var socket = Factory.BusOpen().ThenDial(dialUrl).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    // Make sure receive has started before signalling ready
                    var recvFuture = ctx.Receive(cts.Token);
                    await WaitShort();
                    await readyToSend.SignalAndWait();
                    var _ = await recvFuture;
                    messageReceipt.Signal();
                }
            });
            await CancelAfterAssertwait(cts, bus0Task, bus1Task, bus2Task);
            Assert.Equal(0, messageReceipt.Count);
        }
    }
}
