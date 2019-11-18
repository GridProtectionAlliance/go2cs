// package main -- go2cs converted at 2019 November 18 01:05:23 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue\ArrayPassByValue.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct Vertex
        {
            public @int X;
            public @int Y;
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

            //var primes = [6]int{2,3,5,7,11,13};
            //fmt.Println(primes);

            //var i = 42;
            //var j = 2701;

            //var p = ref i; // point to i
            //fmt.Println(p); // read i through the pointer
            //p = 21; // set i through the pointer
            //fmt.Println(i); // see the new value of i

            //p = ref j; // point to j
            //p = p / 37; // divide j through the pointer
            //fmt.Println(j); // see the new value of 

            //var v = Vertex(1,2);
            //var p2 = ref v;
            //p2.X = 1e9;
            //fmt.Println(v);

            //var p3 = ref Vertex{1,3}; // has type *Vertex
            //fmt.Println(p3);

            //var q = []int{2,3,5,7,11,13};
            //fmt.Println(q);

            //var r = []bool{true,false,true,true,false,true};
            //fmt.Println(r);

            //var s = []struct{iintbbool}{{2,true},{3,false},{5,true},{7,true},{11,false},{13,true},};
            //fmt.Println(s);
        }

        // Arrays are passed by value (a full copy)
        private static void test(array<@string> a)
        { 
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
            a[0] = "Goodbye";
            fmt.Println(a[0], a[1]);
        }
    }
}
