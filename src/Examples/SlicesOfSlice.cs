// package main -- go2cs converted at 2018 August 06 15:38:07 UTC
// Original source: D:\Projects\go2cs\src\Examples\SlicesOfSlice.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            // Create a tic-tac-toe board.
            var board = [][]string{[]string{"_","_","_"},[]string{"_","_","_"},[]string{"_","_","_"},};

            // The players take turns.
            board[0][0] = "X";
            board[2][2] = "O";
            board[1][2] = "X";
            board[1][0] = "O";
            board[0][2] = "X";

            {
                fmt.Printf("%s\n", strings.Join(board[i], " "));
            }        }
    }
}
