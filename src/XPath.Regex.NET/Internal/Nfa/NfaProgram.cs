using System.Collections.Immutable;

namespace XPath.Regex.NET.Internal.Nfa;

/// <summary>
/// An immutable compiled NFA program produced by the compiler.
/// </summary>
internal sealed class NfaProgram
{
    /// <summary>
    /// Maps group index (0 = top-level sentinel) to directly-nested child group numbers.
    /// <c>GroupChildren[0]</c> = list of top-level capturing groups.
    /// <c>GroupChildren[N]</c> = groups directly enclosed by group N.
    /// Size = CaptureCount + 1.
    /// </summary>
    public ImmutableArray<ImmutableArray<int>> GroupChildren { get; }

    /// <summary>Instruction array. Index 0 is the entry point.</summary>
    public ImmutableArray<NfaInstruction> Instructions { get; }

    /// <summary>Whether capture state can affect execution through a backreference.</summary>
    public bool HasBackReferences { get; }

    /// <summary>Total number of capturing groups (0 = only overall match slot).</summary>
    public int CaptureCount { get; }

    /// <summary>
    /// Whether the program can match the empty string at any position.
    /// Pre-computed during compilation for fast <c>FORX0003</c> detection.
    /// </summary>
    public bool CanMatchEmpty { get; }

    /// <summary>
    /// Minimum input length that can lead to a successful match.
    /// </summary>
    public int MinMatchLength { get; }

    internal string? RequiredPrefixHint { get; }

    internal NfaProgram(
        ImmutableArray<NfaInstruction> instructions,
        int captureCount,
        bool canMatchEmpty,
        int minMatchLength,
        string? requiredPrefixHint,
        ImmutableArray<ImmutableArray<int>> groupChildren)
    {
        Instructions = instructions;
        HasBackReferences = instructions.Any(static instruction => instruction is BackRefInstruction);
        CaptureCount = captureCount;
        CanMatchEmpty = canMatchEmpty;
        MinMatchLength = minMatchLength;
        RequiredPrefixHint = requiredPrefixHint;
        GroupChildren = groupChildren;
    }
}
