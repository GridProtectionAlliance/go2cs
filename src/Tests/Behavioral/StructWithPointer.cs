// package main -- go2cs converted at 2018 July 02 12:54:34 UTC
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
            red:=ColorList{2,"red",nil,nil}blue:=ColorList{2,"blue",nil,nil}red.Next=&bluefmt.Printf("Value of red = %v\n",red)fmt.Printf("Value of blue = %v\n",blue)
        }
    }
}
