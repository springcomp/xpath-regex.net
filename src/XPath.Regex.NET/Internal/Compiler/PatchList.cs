using System.Collections.Immutable;

namespace XPath.Regex.NET.Internal.Compiler;

internal enum PatchSlot
{
    SplitFirst,
    SplitSecond,
    Jmp
}

internal readonly struct PatchList
{
    public static PatchList Empty { get; } = new PatchList(ImmutableArray<(int Address, PatchSlot Slot)>.Empty);

    private readonly ImmutableArray<(int Address, PatchSlot Slot)> _items;

    private PatchList(ImmutableArray<(int Address, PatchSlot Slot)> items)
    {
        _items = items;
    }

    public ImmutableArray<(int Address, PatchSlot Slot)> Items => _items;

    public static PatchList ForSplitFirst(int address) => new PatchList(ImmutableArray.Create((address, PatchSlot.SplitFirst)));

    public static PatchList ForSplitSecond(int address) => new PatchList(ImmutableArray.Create((address, PatchSlot.SplitSecond)));

    public static PatchList ForJmp(int address) => new PatchList(ImmutableArray.Create((address, PatchSlot.Jmp)));

    public PatchList Concat(PatchList other)
    {
        if (_items.IsDefaultOrEmpty)
            return other;

        if (other._items.IsDefaultOrEmpty)
            return this;

        return new PatchList(_items.AddRange(other._items));
    }
}
