// package main -- go2cs converted at 2018 August 09 01:21:19 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\InterfaceInheritance.go
using fmt = go.fmt_package;
using static go.builtin;
using System.Collections.Generic;

namespace go
{
    public static partial class main_package
    {
        public partial struct T1
        {
            public @string name;
        }

        public static void M(this T1 t)
        {
        }
        public static void N(this T1 t)
        {
        }
        public static @string String(this T1 t)
        {
            return "";
        }
        public static @string Error(this T1 t)
        {
            return "";
        }

        public partial struct T2
        {
            public @string name;
        }

        public static void M(this T2 t)
        {
        }
        public static void N(this T2 t)
        {
        }
        public static @string String(this T2 t)
        {
            return "";
        }
        public static @string Error(this T2 t)
        {
            return "";
        }

        public partial interface I
        {
            void M();
        }

        public partial interface V : I, fmt.Stringer, error
        {
            void N();
        }

        private static void Main()
        {
            var m = make(typeof(Dictionary<I, @int>));
            I i1 = T1{"foo"};
            I i2 = T2{"bar"};
            m[i1] = 1;
            m[i2] = 2;
            fmt.Println(m);

            var n = make(typeof(Dictionary<V, @int>));
            V v1 = T1{"foo"};
            V v2 = T2{"bar"};
            v1.N();
            v2.M();
            v1.String();
            v2.Error();
            n[v1] = 3;
            n[v2] = 4;
            fmt.Println(n);
        }
    }
}
