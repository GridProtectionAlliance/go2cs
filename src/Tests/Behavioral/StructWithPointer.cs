// package main -- go2cs converted at 2018 July 12 03:35:11 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructWithPointer.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct ColorList
        {
            public long Total;
            public GoString Color;
            public Ptr<ColorList> Next;
            public Ptr<Ptr<ColorList>> NextNext;
        }


        private static void Main()
        {
            fmt.Printf("Value of red = %v\n", red);
            fmt.Printf("Value of blue = %v\n", blue);
        }
    }
}
