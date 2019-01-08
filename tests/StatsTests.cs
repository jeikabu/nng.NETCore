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
            // FIXME: Can be removed in nng 1.1.2 or 1.2.0
            // https://github.com/nanomsg/nng/issues/841
            using (var pushSocket = Factory.PusherCreate("inproc://stats", true).Unwrap())
            using (var pullSocket = Factory.PullerCreate("inproc://stats", false).Unwrap())
            {
                //var root = Factory.GetStatSnapshot().Unwrap();
                using (var root = Factory.GetStatSnapshot().Unwrap())
                {
                    var next = root.Child();
                    while (!next.NngStat.IsNull)
                    {
                        Console.WriteLine("Child: " + next.Name);
                        next = next.Next();
                    }
                }
            }
        }

        void init_stats()
        {

        }
    }
}