// package main -- go2cs converted at 2018 June 25 19:10:12 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\InterfaceImplementation.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial interface Animal
        {
            string Type();
            string Swim();
        }



        private static void Main()
        {
            f:=new(Frog)d:=new(Dog)zoo:=[...]Animal{f,d}for_,a:=rangezoo{fmt.Println(a.Type(),"can",a.Swim())}
        }

        public static string Type(this ref Frog f)
        {
            return"Frog"
        }

        public static string Swim(this ref Frog f)
        {
            return"Kick"
        }

        public static string Swim(this ref Dog d)
        {
            return"Paddle"
        }

        public static string Type(this ref Dog d)
        {
            return"Doggie"
        }
    }
}
