using goutil;
using System;

public static partial class main_package
{
    private static void Main()
    {
        Slice<int> s = new Slice<int>();
        Console.WriteLine("{0} {1} {2}", s, len(s), cap(s));

        if (s == nil)
            Console.WriteLine("nil!");
    }
    
    public static int len<T>(Slice<T> slice) => slice.Length;

    public static int cap<T>(Slice<T> slice) => slice.Capacity;
}