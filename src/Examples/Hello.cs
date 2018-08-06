// package main -- go2cs converted at 2018 August 06 15:38:06 UTC
// Original source: D:\Projects\go2cs\src\Examples\Hello.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct User
        {
            public @int Id;
            public @string Name;
        }

        public partial struct MyFloat // : float64
        {
        }

        public partial struct Account
        {
            public User User;
            public @int MyFloat;
        }
        private static void Main()
        {
            User a;            a.Id = 12;
            a.Name = "Me";
            fmt.Println("Hello, 世界 ", a);
        }
    }
}
