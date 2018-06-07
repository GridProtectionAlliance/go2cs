// package main -- go2cs converted at 2018 June 07 01:54:03 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructPromotion.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial struct Person
        {
            public string name;
            public int age;
        }
        returnp.age>=18

        public partial struct Employee
        {
            public string position;
        }
        returne.position=="manager"

        public partial struct Record
        {
            public Employee Person;
        }

        private static bool Main() => func((defer, panic, recover) =>
        {
            person:=Person{name:"Michał",age:29}fmt.Println(person)record:=Record{}record.name="Michał"record.age=29record.position="software engineer"fmt.Println(record)fmt.Println(record.name)fmt.Println(record.age)fmt.Println(record.position)fmt.Println(record.IsAdult())fmt.Println(record.IsManager())
        });
    }
}
