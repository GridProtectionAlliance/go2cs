// package main -- go2cs converted at 2018 August 05 14:35:29 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructPointerPromotionWithInterface.go
using fmt = go.fmt_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial interface Abser
        {
            float64 Abs();
        }

        public partial struct MyError
        {
            public time.Time When;
            public @string What;
        }

        public partial struct MyCustomError : Abser
        {
            public @string Message;
            public Abser Abser;
            public ref MyError MyError => ref MyError_ptr;
        }
        private static void Main()
        {
            var e = MyError{time.Now(),"Hello"};
            var a = MyCustomError{"New One",nil,&e};

            a.Message = "New";
            a.What = "World";

            fmt.Println("MyError What =", e.What);
            fmt.Println("MyCustomError What =", a.What);
        }
    }
}
