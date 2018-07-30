// package main -- go2cs converted at 2018 July 29 23:58:04 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\TypeConversion.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            public partial struct Person
            {
                public @string Name;
            }

            ref dynamic data;

            Person mine;

            var person = (Person.Deref)(data);
            // ignoring tags, the underlying types are identical
            person = ref mine;

            fmt.Println(mine == person.Deref);

            fmt.Println((slice<rune>)@string("白鵬翔"));
        }    }
}
