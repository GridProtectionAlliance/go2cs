using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        { 
            // A type `switch` compares types instead of values.  You
            // can use this to discover the type of an interface
            // value.  In this example, the variable `t` will have the
            // type corresponding to its clause.
            Action<object> whatAmI = i =>
            {
                switch (i.type())
                {
                    case bool t:
                        fmt.Println("I'm a bool");
                        break;
                    case long t:
                        fmt.Printf("I'm an int, specifically type %T\n", t);
                        break;
                    case ulong t:
                        fmt.Printf("I'm an int, specifically type %T\n", t);
                        break;
                    default:
                    {
                        var t = i.type();
                        fmt.Printf("Don't know type %T\n", t);
                        break;
                    }
                }
            };
            whatAmI(true);
            whatAmI(1L);
            whatAmI(int64(2L));
            whatAmI(uint64(2L));
            whatAmI("hey");
        }
    }
}
