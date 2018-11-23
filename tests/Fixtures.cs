using nng.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace nng.Tests
{
    using static nng.Tests.Util;

    public class NngCollectionFixture
    {
        public NngCollectionFixture()
        {
            var managedAssemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var alc = new NngLoadContext(managedAssemblyPath);
            Factory = NngLoadContext.Init(alc);
        }

        public IAPIFactory<IMessage> Factory { get; private set; }

        public int Iterations => 10; 
    }

    [CollectionDefinition("nng")]
    public class NngCollection : ICollectionFixture<NngCollectionFixture>
    {
    }
}