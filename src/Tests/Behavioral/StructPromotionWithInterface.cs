// package main -- go2cs converted at 2018 July 12 19:15:05 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\StructPromotionWithInterface.go

using fmt = go.fmt_package;
using time = go.time_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        // Above type comment
        // Top inner type comment
        public interface Abser
        {
            double Abs(); // To the right comments
        }

        // Middle type comment
        public partial struct MyError
        {
            public time.Time When;
            public GoString What;
        }

        // Weirdly placed comment

        public partial struct MyCustomError : Abser, error
        {
            public GoString Message; // My custom error message
            public Abser Abser;
            public ref MyError MyError => ref MyError_val;
            public error error;
        }

        // Bottom inner type comment

        // Below type comment

        // The following takes precedence over instance call to Abs()
        public static double Abs(this ref MyCustomError myErr)
        {
            return 0.0;
        }

        private static void Main()
        {
            a.Abs();
            fmt.Println("MyCustomError method =", a.Abs());
        }
    }
}
