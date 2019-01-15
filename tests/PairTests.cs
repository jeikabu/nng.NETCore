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
        public async Task Pair(string url)
        {
            for (int i = 0; i < Fixture.Iterations; ++i)
            {
                await DoPair(url);
            }
        }

        Task DoPair(string url)
        {
            var barrier = new AsyncBarrier(2);
            var cts = new CancellationTokenSource();
            var push = Task.Run(async () =>
            {
                using (var socket = Factory.PairCreate(url, true).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await barrier.SignalAndWait();
                    Assert.True(await socket.Send(Factory.CreateMessage()));
                    await WaitShort();
                }
            });
            var pull = Task.Run(async () =>
            {
                await barrier.SignalAndWait();
                using (var socket = Factory.PairCreate(url, false).Unwrap().CreateAsyncContext(Factory).Unwrap())
                {
                    await socket.Receive(cts.Token);
                }
            });
            cts.CancelAfter(DefaultTimeoutMs);
            return Task.WhenAll(pull, push);
        }

    }
}
