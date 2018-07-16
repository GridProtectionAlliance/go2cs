// package main -- go2cs converted at 2018 July 16 19:42:07 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial interface Animal
        {
            GoString Type();
            GoString Swim();
        }


        public partial struct Dog
        {
            public GoString Name;
            public GoString Breed;
        }


        public partial struct Frog
        {
            public GoString Name;
            public GoString Color;
        }


        private static void Main()
        {
            var f = @new(Frog);
            var d = @new(Dog);
            var zoo = [...]Animal{f,d};

            {
                fmt.Println(a.Type(), "can", a.Swim());
            }        }

        public static GoString Type(this ref Frog f)
        {
            return "Frog";
        }

        public static GoString Swim(this ref Frog f)
        {
            return "Kick";
        }

        public static GoString Swim(this ref Dog d)
        {
            return "Paddle";
        }

        public static GoString Type(this ref Dog d)
        {
            return "Doggie";
        }
    }
}
