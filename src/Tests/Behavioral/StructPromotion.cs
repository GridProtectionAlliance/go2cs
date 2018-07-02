// package main -- go2cs converted at 2018 July 02 12:54:34 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructPromotion.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct Person
        {
            public GoString name;
            public int age;
        }

        public static bool IsAdult(this Person p)
        {
            returnp.age>=18
        }

        public partial struct Employee
        {
            public GoString position;
        }

        public static bool IsManager(this Employee e)
        {
            returne.position=="manager"
        }

        public partial struct Record
        {
            public ref Person Person => ref Person_val;
            public ref Employee Employee => ref Employee_val;
        }

        private static void Main()
        {
            person:=Person{name:"Michał",age:29}fmt.Println(person)record:=Record{}record.name="Michał"record.age=29record.position="software engineer"fmt.Println(record)fmt.Println(record.name)fmt.Println(record.age)fmt.Println(record.position)fmt.Println(record.IsAdult())fmt.Println(record.IsManager())
        }
    }
}
