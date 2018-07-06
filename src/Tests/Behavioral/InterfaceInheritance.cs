// package main -- go2cs converted at 2018 July 06 21:24:28 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\InterfaceInheritance.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;
using System.Collections.Generic;

namespace go
{
    public static partial class main_package
    {
        public partial struct T1
        {
            public GoString name;
        }


        public static void M(this T1 t)
        {
            {
            }        }

        public static void N(this T1 t)
        {
            {
            }        }

        public static GoString String(this T1 t)
        {
            {
                return ;
            }        }

        public static GoString Error(this T1 t)
        {
            {
                return ;
            }        }

        public partial struct T2
        {
            public GoString name;
        }


        public static void M(this T2 t)
        {
            {
            }        }

        public static void N(this T2 t)
        {
            {
            }        }

        public static GoString String(this T2 t)
        {
            {
                return ;
            }        }

        public static GoString Error(this T2 t)
        {
            {
                return ;
            }        }

        public interface I
        {
            void M();
        }


        public interface V : I, fmt.Stringer, error
        {
            void N();
        }


        private static void Main()
        {
            fmt.Println(m);
            v1.N();
            v2.M();
            v1.String();
            v2.Error();
            fmt.Println(n);
        }
    }
}
