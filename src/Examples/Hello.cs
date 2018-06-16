// package main -- go2cs converted at 2018 June 16 17:46:02 UTC
// Original source: C:\Projects\go2cs\src\Examples\Hello.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public partial struct User
        {
            public long Id;
            public string Name;
        }
        public partial struct MyFloat // double
        {
        }
        public partial struct Account
        {
            public User User;
            public long MyFloat;
        }

        private static void Main() => func((defer, panic, recover) =>
        {
            varaUser;a.Id=12a.Name="Me"fmt.Println("Hello, 世界 ",a)
        });
    }
}
