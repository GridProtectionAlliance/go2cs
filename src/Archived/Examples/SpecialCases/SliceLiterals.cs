// package main -- go2cs converted at 2018 August 10 20:17:57 UTC
// Original source: D:\Projects\go2cs\src\Examples\SliceLiterals.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            var q = []int{2,3,5,7,11,13};
            fmt.Println(q);

            var r = []bool{true,false,true,true,false,true};
            fmt.Println(r);

            var s = []struct{iintbbool}{{2,true},{3,false},{5,true},{7,true},{11,false},{13,true},};
            fmt.Println(s);
        }
    }
}
