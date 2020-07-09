using goutil;
using System;

public static partial class main_package
{
    private static void Main()
    {
        Slice<int> s = new Slice<int>(new[] { 2, 3, 5, 7, 11, 13 });
        printSlice(s);

        // Slice the slice to give it zero length.
        s = s.Slice(high: 0);
        printSlice(s);

        // Extend its length.
        s = s.Slice(high: 4);
        printSlice(s);

        // Drop its first two values.
        s = s.Slice(2);
        printSlice(s);
    }

    private static void printSlice(Slice<int> s)
    {
        Console.WriteLine("len={0} cap={1} [{2}]", len(s), cap(s), string.Join(" ", s));
    }

    public static int len<T>(Slice<T> slice) => slice.Length;

    public static int cap<T>(Slice<T> slice) => slice.Capacity;
}