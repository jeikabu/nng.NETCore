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
    public class StatsTests
    {
        NngCollectionFixture Fixture;
        IAPIFactory<IMessage> Factory => Fixture.Factory;

        public StatsTests(NngCollectionFixture collectionFixture)
        {
            Fixture = collectionFixture;
        }

        [Fact]
        public void Basic()
        {
            // Snapshop without sockets shouldn't crash
            using (var root = Factory.GetStatSnapshot().Unwrap())
            {
                foreach (var _ in root.Child())
                {
                }
            }

            var url = UrlInproc();
            using (var pushSocket = Factory.PairOpen().ThenListen(url).Unwrap())
            using (var pullSocket = Factory.PairOpen().ThenDial(url).Unwrap())
            using (var root = Factory.GetStatSnapshot().Unwrap())
            {
                foreach (var child in root.Child())
                {
                    var _ = $"{child.Name} {child.Type} {child.Desc}";
                }
            }
        }
    }
}