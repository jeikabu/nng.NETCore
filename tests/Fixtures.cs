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
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
            // Assume we're in:
            // <root>/tests/bin/Debug/netcoreapp2.1/
            // And we need to get to:
            // <root>/nng.NETCore/bin/Debug/netstandard2.0/
            var root = dirInfo.Parent.Parent.Parent.Parent.FullName;
            #if DEBUG
            var configuration = "Debug";
            #else
            var configuration = "Release";
            #endif
            var managedAssemblyPath = Path.Combine(root, "nng.NETCore", "bin", configuration, "netstandard2.0");
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