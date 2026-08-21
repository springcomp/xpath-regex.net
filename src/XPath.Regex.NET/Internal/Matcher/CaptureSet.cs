using System.Buffers;

namespace XPath.Regex.NET.Internal.Matcher;

internal static class CaptureSet
{
    public static int[] Rent(int slotCount, List<int[]> rentals)
    {
        int[] array = ArrayPool<int>.Shared.Rent(slotCount);
        Array.Fill(array, -1);
        rentals.Add(array);
        return array;
    }

    public static int[] Clone(int[] source, List<int[]> rentals)
    {
        int[] clone = ArrayPool<int>.Shared.Rent(source.Length);
        Array.Copy(source, clone, source.Length);
        rentals.Add(clone);
        return clone;
    }

    public static void ReturnAll(List<int[]> rentals)
    {
        foreach (int[] arr in rentals)
            ArrayPool<int>.Shared.Return(arr);

        rentals.Clear();
    }
}
