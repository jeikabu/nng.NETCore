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

    public class NngCollectionFixture
    {
        public NngCollectionFixture()
        {
            var alc = new ALC();
            var assem = alc.LoadFromAssemblyName(new System.Reflection.AssemblyName("nng.NETCore"));
            var type = assem.GetType("nng.Tests.TestFactory");
            Factory = (IFactory<IMessage>)Activator.CreateInstance(type);
        }

        public IFactory<IMessage> Factory { get; private set; }
    }

    [CollectionDefinition("nng")]
    public class NngCollection : ICollectionFixture<NngCollectionFixture>
    {
    }
}