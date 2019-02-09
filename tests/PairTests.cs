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
    public class PairTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public PairTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task Pair(string url)
        {
            return Fixture.TestIterate(() => DoPair(url));
        }

        Task DoPair(string url)
        {
            var barrier = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var dialUrl = string.Empty;
            var push = Task.Run(async () =>
            {
                using (var socket = Factory.PairOpen().ThenListenAs(out var listener, url).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    dialUrl = GetDialUrl(listener, url);
                    await barrier.SignalAndWait();
                    (await ctx.Send(Factory.CreateMessage())).Unwrap();
                    await WaitShort();
                }
            });
            var pull = Task.Run(async () =>
            {
                await barrier.SignalAndWait();
                using (var socket = Factory.PairOpen().ThenDial(dialUrl).Unwrap())
                using (var ctx = socket.CreateAsyncContext(Factory).Unwrap())
                {
                    await ctx.Receive(cts.Token);
                }
            });
            return CancelAfterAssertwait(cts, pull, push);
        }

        [Theory]
        [ClassData(typeof(TransportsClassData))]
        public Task PairShared(string url)
        {
            return Fixture.TestIterate(() => DoPairShared(url));
        }

        async Task DoPairShared(string url)
        {
            int numListeners = 2;
            int numDialers = 2;

            var listenerReady = new AsyncBarrier(numListeners + 1);
            var dialerReady = new AsyncBarrier(numListeners + numDialers);
            var cts = new CancellationTokenSource();

            using (var listenSocket = Factory.PairOpen().ThenListenAs(out var listener, url).Unwrap())
            using (var dialerSocket = Factory.PairOpen().ThenDial(GetDialUrl(listener, url)).Unwrap())
            {
                // Make sure sends can timeout since they aren't canceleable
                listenSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});
                dialerSocket.SetOpt(nng.Native.Defines.NNG_OPT_SENDTIMEO, new nng_duration{TimeMs = 50});
                var tasks = new List<Task>();

                // On listening socket create send/receive AIO
                {
                    var task = Task.Run(async () =>
                    {
                        using(var ctx = listenSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            await listenerReady.SignalAndWait();
                            await dialerReady.SignalAndWait();
                            while (!cts.IsCancellationRequested)
                            {
                                var msg = await ctx.Receive(cts.Token);
                            }
                        }
                    });
                    tasks.Add(task);
                    task = Task.Run(async () =>
                    {
                        using (var ctx = listenSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            await listenerReady.SignalAndWait();
                            await dialerReady.SignalAndWait();
                            await WaitShort(); // Give receiver a chance to start receiving
                            while (!cts.IsCancellationRequested)
                            {
                                var _ = await ctx.Send(Factory.CreateMessage());
                                await WaitShort();
                            }
                        }
                    });
                    tasks.Add(task);
                }

                await listenerReady.SignalAndWait();

                // On dialing socket create send/receive AIO
                {
                    var task = Task.Run(async () =>
                    {
                        using (var ctx = dialerSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            await dialerReady.SignalAndWait();
                            while (!cts.IsCancellationRequested)
                            {
                                var msg = await ctx.Receive(cts.Token);
                            }
                        }
                    });
                    tasks.Add(task);
                    task = Task.Run(async () =>
                    {
                        using (var ctx = dialerSocket.CreateAsyncContext(Factory).Unwrap())
                        {
                            await dialerReady.SignalAndWait();
                            await WaitShort(); // Give receiver a chance to start receiving
                            while (!cts.IsCancellationRequested)
                            {
                                var _ = await ctx.Send(Factory.CreateMessage());
                                await WaitShort();
                            }
                        }
                    });
                    tasks.Add(task);
                }

                await Util.CancelAfterAssertwait(tasks, cts);
            }
        }
    }
}
