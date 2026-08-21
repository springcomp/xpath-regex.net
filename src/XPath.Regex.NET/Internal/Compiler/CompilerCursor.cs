using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.Internal.Compiler;

internal sealed class CompilerCursor
{
    private readonly List<NfaInstruction> _instructions = new();
    private readonly int _maxInstructions;

    public CompilerCursor(int maxInstructions = RegexCompileOptions.HardCeilingMaxProgramInstructions)
    {
        // The parameter default equals the secure default (== hard ceiling), so callers that
        // construct a cursor without an explicit budget never bypass RegexCompiler's enforcement.
        _maxInstructions = maxInstructions;
    }

    public int Current => _instructions.Count;

    public int Emit(NfaInstruction instruction)
    {
        // Checked before adding so the budget is enforced as soon as it would be exceeded,
        // bounding pathological nested repeats such as (a{1000}){1000}.
        if (_instructions.Count >= _maxInstructions)
            throw new RegexCompilationLimitExceededException(
                RegexCompilationLimit.MaxProgramInstructions,
                $"Compiled pattern exceeds the configured program instruction limit of {_maxInstructions.ToString(CultureInfo.InvariantCulture)}.");

        int address = _instructions.Count;
        _instructions.Add(instruction);
        return address;
    }

    public void Patch(PatchList list, int target)
    {
        foreach ((int address, PatchSlot slot) in list.Items)
        {
            NfaInstruction instruction = _instructions[address];
            switch (instruction)
            {
                case SplitInstruction split when slot == PatchSlot.SplitFirst:
                    _instructions[address] = new SplitInstruction(target, split.Second);
                    break;
                case SplitInstruction split when slot == PatchSlot.SplitSecond:
                    _instructions[address] = new SplitInstruction(split.First, target);
                    break;
                case JmpInstruction when slot == PatchSlot.Jmp:
                    _instructions[address] = new JmpInstruction(target);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot patch instruction type {instruction.GetType().Name} with slot {slot} at {address}.");
            }
        }
    }

    public IReadOnlyList<NfaInstruction> Instructions => _instructions;

    public NfaProgram BuildProgram(
        int captureCount,
        bool canMatchEmpty,
        int minMatchLength,
        string? requiredPrefixHint,
        ImmutableArray<ImmutableArray<int>> groupChildren)
    {
        return new NfaProgram(
            _instructions.ToImmutableArray(),
            captureCount,
            canMatchEmpty,
            minMatchLength,
            requiredPrefixHint,
            groupChildren);
    }
}
