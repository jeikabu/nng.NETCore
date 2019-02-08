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
            using (var pushSocket = Factory.PusherOpen().Unwrap())
            using (var pullSocket = Factory.PullerOpen().Unwrap())
            {
                pushSocket.Listen("inproc://stats").Unwrap();
                pullSocket.Dial("inproc://stats").Unwrap();
                using (var root = Factory.GetStatSnapshot().Unwrap())
                {
                    foreach (var child in root.Child())
                    {
                        var _ = String.Format("Child {0}[{1}]: {2}", child.Name, child.Type, child.Desc);
                    }
                }
            }
        }
    }
}