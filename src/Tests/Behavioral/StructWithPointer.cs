// package main -- go2cs converted at 2018 August 09 01:21:20 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructWithPointer.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct ColorList
        {
            public @int Total;
            public @string Color;
            public Ptr<ColorList> Next;
            public Ptr<Ptr<ColorList>> NextNext;
        }

        private static void Main()
        {
            var red = ColorList{2,"red",nil,nil};
            var blue = ColorList{2,"blue",nil,nil};

            red.Next = ref blue;

            fmt.Printf("Value of red = %v\n", red);
            fmt.Printf("Value of blue = %v\n", blue);
        }
    }
}
