// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\SlicesOfSlice.go

using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            board:=[][]string{[]string{"_","_","_"},[]string{"_","_","_"},[]string{"_","_","_"},}board[0][0]="X"board[2][2]="O"board[1][2]="X"board[1][0]="O"board[0][2]="X"fori:=0;i<len(board);i++{fmt.Printf("%s\n",strings.Join(board[i]," "))}
        }
    }
}
