// package main -- go2cs converted at 2018 June 26 17:29:46 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\InterfaceInheritance.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;
using System.Collections.Generic;

namespace go
{
    public static partial class main_package
    {

        public static void M(this T1 t)
        {

        }

        public static void N(this T1 t)
        {

        }

        public static string String(this T1 t)
        {
            return""
        }

        public static string Error(this T1 t)
        {
            return""
        }


        public static void M(this T2 t)
        {

        }

        public static void N(this T2 t)
        {

        }

        public static string String(this T2 t)
        {
            return""
        }

        public static string Error(this T2 t)
        {
            return""
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
            m:=make(map[I]int)vari1I=T1{"foo"}vari2I=T2{"bar"}m[i1]=1m[i2]=2fmt.Println(m)n:=make(map[V]int)varv1V=T1{"foo"}varv2V=T2{"bar"}v1.N()v2.M()v1.String()v2.Error()n[v1]=3n[v2]=4fmt.Println(n)
        }
    }
}
