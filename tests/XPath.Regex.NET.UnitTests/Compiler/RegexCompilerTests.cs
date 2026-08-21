using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Compiler;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.UnitTests.Compiler;

public sealed class RegexCompilerTests
{
    [Fact]
    public void NestedAndSequentialCapturesHaveCorrectCountAndTree()
    {
        AstNode root = new ConcatNode([
            new CaptureNode(1, new CaptureNode(2, new LiteralNode('a'))),
        new CaptureNode(3, new LiteralNode('b')),
    ]);

        NfaProgram program = Compile(root, captureCount: 3);

        Assert.Equal(3, program.CaptureCount);
        Assert.Equal([1, 3], program.GroupChildren[0]);
        Assert.Equal([2], program.GroupChildren[1]);
        Assert.Empty(program.GroupChildren[2]);
        Assert.Empty(program.GroupChildren[3]);
    }

    [Fact]
    public void BackReferenceSetsProgramFlag()
    {
        AstNode root = new ConcatNode([
            new CaptureNode(1, new LiteralNode('a')),
        new BackReferenceNode(1),
    ]);

        NfaProgram program = Compile(root, captureCount: 1);

        Assert.True(program.HasBackReferences);
        Assert.Contains(program.Instructions, instruction => instruction is BackRefInstruction { GroupNumber: 1 });
    }

    [Fact]
    public void MetadataPropagatesEmptyAndMinimumLength()
    {
        NfaProgram empty = Compile(new RepeatNode(new LiteralNode('a'), 0, null, Greedy: true));
        NfaProgram nonEmpty = Compile(new ConcatNode([new AnchorStartNode(), new LiteralNode('a')]));

        Assert.True(empty.CanMatchEmpty);
        Assert.Equal(0, empty.MinMatchLength);
        Assert.False(nonEmpty.CanMatchEmpty);
        Assert.Equal(1, nonEmpty.MinMatchLength);
    }

    [Fact]
    public void AlternationUsesLeftToRightSplitAndJumps()
    {
        NfaProgram program = Compile(new AlternationNode([new LiteralNode('a'), new LiteralNode('b')]));

        Assert.Equal(new SplitInstruction(3, 5), program.Instructions[2]);
        AssertChar(program.Instructions[3], 'a');
        Assert.Equal(new JmpInstruction(8), program.Instructions[4]);
        Assert.Equal(new SplitInstruction(-1, 6), program.Instructions[5]);
        AssertChar(program.Instructions[6], 'b');
    }

    [Theory]
    [InlineData(true, 3, 5)]
    [InlineData(false, 5, 3)]
    public void RepeatSplitOrderingMatchesGreedyMode(bool greedy, int first, int second)
    {
        NfaProgram program = Compile(new RepeatNode(new LiteralNode('a'), 0, null, greedy));

        Assert.Equal(new SplitInstruction(first, second), program.Instructions[2]);
        Assert.Equal(new JmpInstruction(2), program.Instructions[4]);
    }

    private static NfaProgram Compile(AstNode root, int captureCount = 0)
    {
        return RegexCompiler.Compile(root, captureCount, RegexDialect.Xsd, RegexFlags.None);
    }

    private static void AssertChar(NfaInstruction instruction, int codePoint)
    {
        CharInstruction character = Assert.IsType<CharInstruction>(instruction);
        Assert.Equal([(codePoint, codePoint)], character.Ranges);
    }
}
