// package main -- go2cs converted at 2018 August 09 01:21:20 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructWithDelegate.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public Action work;
            public @string name;
            public int32 age;
        }

        private static void Main()
        {
            var person = Person{work:nil,name:"Michał",age:29};
            fmt.Println(person);  // {<nil> Michał 29}
        }
    }
}
