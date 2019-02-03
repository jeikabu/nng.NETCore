using nng.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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

        int Iterations { get; } = 10;

        public void TestIterate(Action testFunction)
        {
            for (int i = 0; i < Iterations; ++i)
            {
                testFunction();
            }
        }

        public async Task TestIterate(Func<Task> testFunction)
        {
            for (int i = 0; i < Iterations; ++i)
            {
                await testFunction();
            }
        }
    }

    [CollectionDefinition("nng")]
    public class NngCollection : ICollectionFixture<NngCollectionFixture>
    {
    }

    // using Xunit.Abstractions;
    // [Collection("nng")]
    // public class PubSubTests
    // {
    //     public PubSubTests(ITestOutputHelper outputHelper)
    //     {
    //         Converter.Register(outputHelper);
    //     }
    //https://stackoverflow.com/questions/7138935/xunit-net-does-not-capture-console-output/47529356#47529356
    internal class Converter : TextWriter
    {
        public static void Register(ITestOutputHelper outputHelper)
        {
            var converter = new Converter(outputHelper ?? throw new ArgumentNullException(nameof(outputHelper)));
            System.Console.SetOut(converter);
        }

        ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }
    }
}