using System;

public static partial class main_package
{
    private static void Main()
    {
        int[] s = { 2, 3, 5, 7, 11, 13 };
        Console.WriteLine(a);

        bool[] r = { true, false, true, true, false, true };
        Console.WriteLine(r);

        ArraySegment<(int i, bool b)> s = new ArraySegment<(int, bool)>(new[]
        {
            (2, true),
            (3, false),
            (5, true),
            (7, true),
            (11, false),
            (13, true)
        });
        
        Console.WriteLine(s);
    }
}