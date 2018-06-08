// package main -- go2cs converted at 2018 June 08 14:02:11 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructWIthPointer.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial struct ColorList
        {
            public long Total;
            public string Color;
            public ColorList* Next;
            public ColorList** NextNext;
        }

        private static void Main() => func((defer, panic, recover) =>
        {
            red:=ColorList{2,"red",nil,nil}blue:=ColorList{2,"blue",nil,nil}red.Next=&bluefmt.Printf("Value of red = %v\n",red)fmt.Printf("Value of blue = %v\n",blue)
        });
    }
}
