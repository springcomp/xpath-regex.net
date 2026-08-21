using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Compiler;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.UnitTests.Compiler;

public sealed class CompilerCursorTests
{
    [Fact]
    public void EmptyCursorStartsAtZero()
    {
        var cursor = new CompilerCursor();

        Assert.Equal(0, cursor.Current);
        Assert.Empty(cursor.Instructions);
    }

    [Fact]
    public void EmitReturnsSequentialAddresses()
    {
        var cursor = new CompilerCursor();

        int first = cursor.Emit(new CharInstruction(
            ImmutableArray.Create<(int Lo, int Hi)>(((int)'a', (int)'a'))));
        int second = cursor.Emit(new MatchInstruction());

        Assert.Equal(0, first);
        Assert.Equal(1, second);
        Assert.Equal(2, cursor.Current);
        Assert.Equal(new MatchInstruction(), cursor.Instructions[1]);
    }

    [Fact]
    public void PatchUpdatesSplitAndJumpSlots()
    {
        var cursor = new CompilerCursor();
        int split = cursor.Emit(new SplitInstruction(-1, 7));
        int jump = cursor.Emit(new JmpInstruction(-1));

        cursor.Patch(PatchList.ForSplitFirst(split), 3);
        cursor.Patch(PatchList.ForJmp(jump), 9);

        Assert.Equal(new SplitInstruction(3, 7), cursor.Instructions[split]);
        Assert.Equal(new JmpInstruction(9), cursor.Instructions[jump]);
    }

    [Fact]
    public void BuildProgramPreservesInstructionsAndMetadata()
    {
        var cursor = new CompilerCursor();
        cursor.Emit(new MatchInstruction());

        NfaProgram program = cursor.BuildProgram(
            captureCount: 2,
            canMatchEmpty: true,
            minMatchLength: 4,
            requiredPrefixHint: "abc",
            groupChildren: ImmutableArray.Create(
                ImmutableArray.Create(1),
                ImmutableArray.Create(2),
                ImmutableArray<int>.Empty));

        Assert.Single(program.Instructions);
        Assert.Equal(2, program.CaptureCount);
        Assert.True(program.CanMatchEmpty);
        Assert.Equal(4, program.MinMatchLength);
        Assert.Equal("abc", program.RequiredPrefixHint);
        Assert.Equal([1], program.GroupChildren[0]);
        Assert.Equal([2], program.GroupChildren[1]);
    }
}
