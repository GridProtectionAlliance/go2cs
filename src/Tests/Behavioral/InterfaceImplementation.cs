// package main -- go2cs converted at 2018 June 19 13:39:31 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
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

        private static void Main() => func((defer, panic, recover) =>
        {
            f:=new(Frog)d:=new(Dog)zoo:=[...]Animal{f,d}for_,a:=rangezoo{fmt.Println(a.Type(),"can",a.Swim())}
        });

        public static string Type(this ref Frog _f) => func(ref _f, (ref Frog f, Defer defer, Panic panic, Recover recover) =>
        {
            return"Frog"
        });

        public static string Swim(this ref Frog _f) => func(ref _f, (ref Frog f, Defer defer, Panic panic, Recover recover) =>
        {
            return"Kick"
        });

        public static string Swim(this ref Dog _d) => func(ref _d, (ref Dog d, Defer defer, Panic panic, Recover recover) =>
        {
            return"Paddle"
        });

        public static string Type(this ref Dog _d) => func(ref _d, (ref Dog d, Defer defer, Panic panic, Recover recover) =>
        {
            return"Doggie"
        });
    }
}
