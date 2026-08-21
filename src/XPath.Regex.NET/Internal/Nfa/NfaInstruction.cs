using System.Collections.Immutable;

namespace XPath.Regex.NET.Internal.Nfa;

internal abstract record NfaInstruction;

/// <summary>Match and consume a single code-point in the given range set.</summary>
internal sealed record CharInstruction(ImmutableArray<(int Lo, int Hi)> Ranges) : NfaInstruction;

/// <summary>
/// Epsilon split: non-deterministically try <see cref="First"/> then <see cref="Second"/>.
/// For greedy quantifiers, <see cref="First"/> points to the repeat body, <see cref="Second"/>
/// to the skip-out. For reluctant quantifiers, order is reversed.
/// </summary>
internal sealed record SplitInstruction(int First, int Second) : NfaInstruction;

/// <summary>Unconditional jump to <see cref="Target"/>.</summary>
internal sealed record JmpInstruction(int Target) : NfaInstruction;

/// <summary>
/// Write the current input position to capture slot <see cref="Slot"/>.
/// Slots: 2N-2 = start of group N, 2N-1 = end of group N.
/// Slot 0 and 1 are the overall match start/end.
/// </summary>
internal sealed record SaveInstruction(int Slot) : NfaInstruction;

/// <summary>Assert start of input (or start of line when multiline is in effect).</summary>
internal sealed record AnchorStartInstruction : NfaInstruction;

/// <summary>Assert end of input (or end of line when multiline is in effect).</summary>
internal sealed record AnchorEndInstruction : NfaInstruction;

/// <summary>
/// Back-reference assertion: re-match the string captured by group <see cref="GroupNumber"/>
/// at the current input position.
/// </summary>
internal sealed record BackRefInstruction(int GroupNumber, bool IgnoreCase) : NfaInstruction;

/// <summary>Successful match terminal.</summary>
internal sealed record MatchInstruction : NfaInstruction;
