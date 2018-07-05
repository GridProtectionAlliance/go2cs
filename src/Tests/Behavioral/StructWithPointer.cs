// package main -- go2cs converted at 2018 July 05 21:01:34 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\StructWithPointer.go

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
            fmt.Printf("Value of red = %v\n",red);
            fmt.Printf("Value of blue = %v\n",blue);
        }
    }
}
