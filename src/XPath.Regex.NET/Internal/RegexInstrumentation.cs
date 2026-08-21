#if REGEX_INSTRUMENTATION

namespace XPath.Regex.NET.Internal;

internal sealed class VmMetrics
{
  public long CandidateStarts { get; set; }
  public long RunExecutions { get; set; }
  public long ThreadDispatches { get; set; }
  public long CharacterTests { get; set; }
  public long CodePointReads { get; set; }
  public long EpsilonExpansionSteps { get; set; }
  public long ThreadsAdded { get; set; }
  public long MaxThreadListSize { get; set; }
  public long BackReferenceTests { get; set; }

  public VmMetrics Snapshot() => new()
  {
    CandidateStarts = CandidateStarts,
    RunExecutions = RunExecutions,
    ThreadDispatches = ThreadDispatches,
    CharacterTests = CharacterTests,
    CodePointReads = CodePointReads,
    EpsilonExpansionSteps = EpsilonExpansionSteps,
    ThreadsAdded = ThreadsAdded,
    MaxThreadListSize = MaxThreadListSize,
    BackReferenceTests = BackReferenceTests,
  };
}

internal static class VmInstrumentation
{
  [ThreadStatic]
  private static VmMetrics? _current;

  public static VmMetrics? Current => _current;

  public static void Start()
  {
    if (_current is not null)
      throw new InvalidOperationException("A VM instrumentation session is already active.");

    _current = new VmMetrics();
  }

  public static VmMetrics Stop()
  {
    VmMetrics? current = _current;
    _current = null;
    return current?.Snapshot() ?? throw new InvalidOperationException("No VM instrumentation session is active.");
  }
}

#endif
