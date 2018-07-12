// package main -- go2cs converted at 2018 July 12 03:35:10 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\StructPointerPromotionWithInterface.go

using fmt = go.fmt_package;
using time = go.time_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        public interface Abser
        {
            double Abs();
        }

        public partial struct MyError
        {
            public time.Time When;
            public GoString What;
        }

        public partial struct MyCustomError : Abser
        {
            public GoString Message;
            public Abser Abser;
            public ref MyError MyError => ref MyError_ptr;
        }


        private static void Main()
        {
            fmt.Println("MyError What =", e.What);
            fmt.Println("MyCustomError What =", a.What);
        }
    }
}
