// package main -- go2cs converted at 2018 May 30 19:31:15 UTC
// Original source: D:\Projects\go2cs\src\Examples\Hello.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;
using System;

namespace go
{
    public static unsafe partial class main_package
    {
        public partial struct User
        {
            public long Id;
            public string Name;        }
        public partial struct MyFloat
        {
            // Redeclares Go float64 type - see "Hello_MyFloatStructOf(float64).cs"
        }
        public partial struct Account
        {
            public User User;
            public long MyFloat;        }

        private static void Main() => func((defer, panic, recover) =>
        {
            varaUser;a.Id=12a.Name="Me"fmt.Println("Hello, 世界 ",a)
        });
    }
}
