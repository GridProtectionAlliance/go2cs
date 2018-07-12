// package main -- go2cs converted at 2018 July 12 03:35:11 UTC
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
            return p.age >= 18;
        }

        public partial struct Employee
        {
            public GoString position;
        }


        public static bool IsManager(this Employee e)
        {
            return e.position == "manager";
        }

        public partial struct Record
        {
            public ref Person Person => ref Person_val;
            public ref Employee Employee => ref Employee_val;
        }


        private static void Main()
        {
            fmt.Println(person);  // {Michał 29}
            fmt.Println(record); // {{Michał 29} {software engineer}}
            fmt.Println(record.name); // Michał
            fmt.Println(record.age); // 29
            fmt.Println(record.position); // software engineer
            fmt.Println(record.IsAdult()); // true
            fmt.Println(record.IsManager()); // false
        }
    }
}
