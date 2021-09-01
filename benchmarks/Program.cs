using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace benchmarks
{
    class Program
    {
        static void Main(string[] args) 
        {
            System.Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.JoinSummary)
                .AddExporter(BenchmarkDotNet.Exporters.Json.JsonExporter.Full);
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}