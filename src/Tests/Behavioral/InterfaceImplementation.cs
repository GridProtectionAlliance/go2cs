// package main -- go2cs converted at 2018 June 07 01:54:02 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial interface Animal
        {
            public string Type();
            public string Swim();
        }

        public partial struct Dog
        {
            public string Name;
            public string Breed;
        }

        public partial struct Frog
        {
            public string Name;
            public string Color;
        }

        private static string Main() => func((defer, panic, recover) =>
        {
            f:=new(Frog)d:=new(Dog)zoo:=[...]Animal{f,d}for_,a:=rangezoo{fmt.Println(a.Type(),"can",a.Swim())}
        });
        return"Frog"
        return"Kick"
        return"Paddle"
        return"Doggie"
    }
}
