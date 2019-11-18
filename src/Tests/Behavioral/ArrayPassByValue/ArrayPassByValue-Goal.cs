// package main -- go2cs converted at 2019 November 11 05:54:01 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue.go

using System;
using static go.builtin;

using fmt = go.fmt_package;

namespace go
{
    public static partial class main_package
    {
        public struct Vertex
        {
            public @int X;
            public @int Y;

            public Vertex(@int X = default, @int Y = default)
            {
                this.X = X;
                this.Y = Y;
            }

            public override string ToString() => $"{{{X} {Y}}}";
        }

        internal struct _Main_s_type
        {
            public @int i;
            public @bool b;

            public _Main_s_type(@int i = default, @bool b = default)
            {
                this.i = i;
                this.b = b;
            }

            public override string ToString() => $"{{{i} {b}}}";
        }
        private static void Main()
        {
            array<@string> a = new array<@string>(2);

            a[0] = "Hello";
            a[1] = "World";

            test(a);
            fmt.Println(a[0], a[1]);
            fmt.Println();

            a[0] = "Hello";
            test2(ref a);
            fmt.Println(a[0], a[1]);
            fmt.Println();

            a[0] = "Hello";
            test3(a.slice());
            fmt.Println(a[0], a[1]);
            fmt.Println();

            array<@int> primes = new @int[] {2,3,5,7,11,13};
            fmt.Println(primes);

            @int i = 42, j = 2701;

            ref @int p = ref i; // point to i
            fmt.Println(p);     // read i through the pointer
            p = 21;             // set i through the pointer
            fmt.Println(i);     // see the new value of i

            p = ref j;          // point to j
            p = p / 37;         // divide j through the pointer
            fmt.Println(j);     // see the new value of

            Vertex v = new Vertex(1, 2);
            ref Vertex p2 = ref v;
            p2.X = (int)1e9;
            fmt.Println(v);

            Vertex _p3_target = new Vertex(1, 3);
            ref Vertex p3 = ref _p3_target; // has type *Vertex
            fmt.Println(p3);

            slice<@int> q = new @int[] {2, 3, 5, 7, 11, 13};
            fmt.Println(q);

            slice<@bool> r = new @bool[] {true, false, true, true, false, true};
            fmt.Println(r);

            slice<_Main_s_type> s = new[]
            {
                new _Main_s_type(2, true),
                new _Main_s_type(3, false),
                new _Main_s_type(5, true),
                new _Main_s_type(7, true),
                new _Main_s_type(11, false),
                new _Main_s_type(13, true),
            };
            fmt.Println(s);

            Console.ReadLine();
        }

        // Arrays are passed by value (a full copy)
        private static void test(array<@string> a)
        {
            a = a.Clone();

            // Update to array will be local
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }

        private static void test2(ref array<@string> a)
        {
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }

        private static void test3(slice<@string> a)
        {
            fmt.Println(a[0], a[1]);
            a[0] = "Goodbye2";
            fmt.Println(a[0], a[1]);
        }
    }
}
