using goutil;
using System;

public static partial class main_package
{
    private static void Main()
    {
        Slice<int> s = new Slice<int>();
        printSlice(s);
        
        // append works on nil slices.
        s = append(s, 0);
        printSlice(s);

        // The slice grows as needed.
        s = append(s, 1);
        printSlice(s);

        // We can add more than one element at a time.
        s = append(s, 2, 3, 4);
        printSlice(s);
    }

    private static void printSlice(Slice<int> s)
    {
        Console.WriteLine("len={0} cap={1} [{2}]", len(s), cap(s), string.Join(" ", s));
    }

    public static int len<T>(Slice<T> slice) => slice.Length;

    public static int cap<T>(Slice<T> slice) => slice.Capacity;
}