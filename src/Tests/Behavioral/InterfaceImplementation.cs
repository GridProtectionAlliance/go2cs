// package main -- go2cs converted at 2018 July 05 21:01:33 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public interface Animal
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
            {
                fmt.Println(a.Type(),"can",a.Swim());
            }        }

        public static GoString Type(this ref Frog f)
        {
            {
                return ;
            }        }

        public static GoString Swim(this ref Frog f)
        {
            {
                return ;
            }        }

        public static GoString Swim(this ref Dog d)
        {
            {
                return ;
            }        }

        public static GoString Type(this ref Dog d)
        {
            {
                return ;
            }        }
    }
}
