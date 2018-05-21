// package main -- go2cs converted at 2018 May 21 03:52:57 UTC
// Original source: C:\Projects\go2cs\src\Examples\SlicesOfSlice.go

using fmt = go.fmt_package;
using strings = go.strings_package;

using static goutil.BuiltInFunctions;
using goutil;
using System;

namespace go
{
    private static partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            board:=[][]string{[]string{"_", "_", "_"},[]string{"_", "_", "_"},[]string{"_", "_", "_"},}board[0][0]="X"board[2][2]="O"board[1][2]="X"board[1][0]="O"board[0][2]="X"fori:=0;i<len(board);i++{fmt.Printf("%s\n", strings.Join(board[i], " "))}
        });
    }
}
