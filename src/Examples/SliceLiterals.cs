// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\SliceLiterals.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            q:=[]int{2,3,5,7,11,13}fmt.Println(q)r:=[]bool{true,false,true,true,false,true}fmt.Println(r)s:=[]struct{iintbbool}{{2,true},{3,false},{5,true},{7,true},{11,false},{13,true},}fmt.Println(s)
        }
    }
}
