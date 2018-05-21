using goutil;
using System;

public static partial class main_package
{
    private static void Main()
    {
        Slice<Slice<string>> board = new Slice<Slice<string>>(new[]
        {
            new Slice<string>(new[] {"_", "_", "_"}),
            new Slice<string>(new[] {"_", "_", "_"}),
            new Slice<string>(new[] {"_", "_", "_"}),
        });

        // The players take turns.
        board[0][0] = "X";
        board[2][2] = "O";
        board[1][2] = "X";
        board[1][0] = "O";
        board[0][2] = "X";

        for (var i = 0; i < len(board); i++)
        {
            Console.WriteLine("{0}", string.Join(" ", board[i]));
        }
    }

    public static int len<T>(Slice<T> slice) => slice.Length;
}