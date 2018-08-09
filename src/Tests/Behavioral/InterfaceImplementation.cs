// package main -- go2cs converted at 2018 August 09 01:21:19 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial interface Animal
        {
            @string Type();
            @string Swim();
        }

        public partial struct Dog
        {
            public @string Name;
            public @string Breed;
        }

        public partial struct Frog
        {
            public @string Name;
            public @string Color;
        }

        private static void Main()
        {
            var f = @new(Frog);
            var d = @new(Dog);
            var zoo = [...]Animal{f,d};

            {
                {
                    fmt.Println(a.Type(), "can", a.Swim());
                }

            }
        }

        private static @string Type(this ref Frog f)
        {
            return "Frog";
        }

        private static @string Swim(this ref Frog f)
        {
            return "Kick";
        }

        private static @string Swim(this ref Dog d)
        {
            return "Paddle";
        }

        private static @string Type(this ref Dog d)
        {
            return "Doggie";
        }
    }
}
