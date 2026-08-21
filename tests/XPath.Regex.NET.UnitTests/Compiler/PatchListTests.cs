using XPath.Regex.NET.Internal.Compiler;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.UnitTests.Compiler;

public sealed class PatchListTests
{
    [Fact]
    public void FactoryMethodsCreateExpectedSlots()
    {
        Assert.Equal((4, PatchSlot.SplitFirst), PatchList.ForSplitFirst(4).Items.Single());
        Assert.Equal((5, PatchSlot.SplitSecond), PatchList.ForSplitSecond(5).Items.Single());
        Assert.Equal((6, PatchSlot.Jmp), PatchList.ForJmp(6).Items.Single());
    }

    [Fact]
    public void ConcatAppendsItemsAndLeavesInputsUnchanged()
    {
        PatchList first = PatchList.ForSplitFirst(1);
        PatchList second = PatchList.ForJmp(2);

        PatchList combined = first.Concat(second);

        Assert.Equal([(1, PatchSlot.SplitFirst), (2, PatchSlot.Jmp)], combined.Items);
        Assert.Single(first.Items);
        Assert.Single(second.Items);
    }

    [Fact]
    public void CombinedListBackPatchesEveryAddress()
    {
        var cursor = new CompilerCursor();
        int split = cursor.Emit(new SplitInstruction(-1, -1));
        int jump = cursor.Emit(new JmpInstruction(-1));
        PatchList list = PatchList.ForSplitFirst(split).Concat(PatchList.ForJmp(jump));

        cursor.Patch(list, 8);

        Assert.Equal(new SplitInstruction(8, -1), cursor.Instructions[split]);
        Assert.Equal(new JmpInstruction(8), cursor.Instructions[jump]);
    }
}
