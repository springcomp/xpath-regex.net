using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using XPath.Regex.NET;

#if REGEX_INSTRUMENTATION
using XPath.Regex.NET.Internal;
#endif

namespace XPath.Regex.NET.Benchmarks;

internal static class Program
{
#if REGEX_INSTRUMENTATION
  private const string MetricsHeader = "scenario,pattern,inputLength,result,candidateStarts,runExecutions,threadDispatches,characterTests,codePointReads,epsilonExpansionSteps,threadsAdded,maxThreadListSize,backReferenceTests,characterTestsPerInputSquared";
#endif

    private static int Main(string[] args)
    {
        if (args.Any(static arg => string.Equals(arg, "--metrics", StringComparison.Ordinal)))
            return RunMetrics();

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        return 0;
    }

    private static int RunMetrics()
    {
#if REGEX_INSTRUMENTATION
    Console.WriteLine(MetricsHeader);

    foreach (MetricScenario scenario in MetricScenario.All)
    {
      foreach (int inputLength in new[] { 256, 512, 1024, 2048 })
      {
        XPathRegex regex = XPathRegex.Compile(scenario.Pattern);
        string input = scenario.CreateInput(inputLength);

        VmInstrumentation.Start();
        bool result;
        VmMetrics metrics;
        try
        {
          result = regex.IsMatch(input);
        }
        finally
        {
          metrics = VmInstrumentation.Stop();
        }

        double characterTestsPerInputSquared = (double)metrics.CharacterTests / (inputLength * (double)inputLength);
        Console.WriteLine(string.Join(",", new object[]
        {
          scenario.Name,
          Csv(scenario.Pattern),
          inputLength,
          result,
          metrics.CandidateStarts,
          metrics.RunExecutions,
          metrics.ThreadDispatches,
          metrics.CharacterTests,
          metrics.CodePointReads,
          metrics.EpsilonExpansionSteps,
          metrics.ThreadsAdded,
          metrics.MaxThreadListSize,
          metrics.BackReferenceTests,
          characterTestsPerInputSquared.ToString("G17", System.Globalization.CultureInfo.InvariantCulture),
        }));
      }
    }

    return 0;
#else
        Console.Error.WriteLine("The --metrics mode requires the Instrumented configuration.");
        return 2;
#endif
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

#if REGEX_INSTRUMENTATION
  private sealed record MetricScenario(string Name, string Pattern, Func<int, string> CreateInput)
  {
    public static IReadOnlyList<MetricScenario> All { get; } =
    [
      new("UnanchoredNoMatch", "a+z", static length => new string('a', length)),
      new("AnchoredNoMatch", "^a+z$", static length => new string('a', length)),
      new("QuickFail", "a+z", static length => new string('b', length)),
      new("MatchAtEnd", "a+z", static length => new string('a', length - 1) + 'z'),
      new("PrefixLateNoMatch", "(foobar)\\d+\\1", static length => new string('x', length)),
      new("PrefixLateMatch", "(foobar)\\d+\\1", static length => new string('x', length - 15) + "foobar123foobar"),
      new("NoPrefixControl", ".*abc", static length => new string('x', length)),
    ];
  }
#endif
}

[MemoryDiagnoser]
public class PrefixBenchmarks
{
    private XPathRegex _late = null!;
    private XPathRegex _control = null!;
    private string _lateInput = null!;
    private string _controlInput = null!;

    [Params(256, 512, 1024, 2048, 8192)]
    public int InputLength { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _late = XPathRegex.Compile("(foobar)\\d+\\1");
        _control = XPathRegex.Compile(".*abc");
        _lateInput = new string('x', InputLength);
        _controlInput = new string('x', InputLength);
    }

    [Benchmark] public bool PrefixLateNoMatch() => _late.IsMatch(_lateInput);
    [Benchmark] public bool NoPrefixControl() => _control.IsMatch(_controlInput);
}

[MemoryDiagnoser]
public class MatchingBenchmarks
{
    private XPathRegex _unanchored = null!;
    private XPathRegex _anchored = null!;
    private string _allA = null!;
    private string _allB = null!;
    private string _matchAtEnd = null!;

    [Params(256, 512, 1024, 2048)]
    public int InputLength { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _unanchored = XPathRegex.Compile("a+z");
        _anchored = XPathRegex.Compile("^a+z$");
        _allA = new string('a', InputLength);
        _allB = new string('b', InputLength);
        _matchAtEnd = new string('a', InputLength - 1) + 'z';
    }

    [Benchmark]
    public bool UnanchoredNoMatch() => _unanchored.IsMatch(_allA);

    [Benchmark]
    public bool AnchoredNoMatch() => _anchored.IsMatch(_allA);

    [Benchmark]
    public bool QuickFail() => _unanchored.IsMatch(_allB);

    [Benchmark]
    public bool MatchAtEnd() => _unanchored.IsMatch(_matchAtEnd);
}
