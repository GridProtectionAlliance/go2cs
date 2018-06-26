// package main -- go2cs converted at 2018 June 25 19:10:12 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\StructPromotion.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {

        public static bool IsAdult(this Person p)
        {
            returnp.age>=18
        }


        public static bool IsManager(this Employee e)
        {
            returne.position=="manager"
        }


        private static void Main()
        {
            person:=Person{name:"Michał",age:29}fmt.Println(person)record:=Record{}record.name="Michał"record.age=29record.position="software engineer"fmt.Println(record)fmt.Println(record.name)fmt.Println(record.age)fmt.Println(record.position)fmt.Println(record.IsAdult())fmt.Println(record.IsManager())
        }
    }
}
