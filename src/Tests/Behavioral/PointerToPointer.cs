// package main -- go2cs converted at 2018 August 05 14:35:28 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\PointerToPointer.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            @int a;
            ref @int ptr;
            Ptr<Ptr<@int>> pptr;

            a = 3000;

            /* take the address of var */
            ptr = ref a;

            /* take the address of ptr using address of operator & */
            pptr = ref ptr;

            /* take the value using pptr */
            fmt.Printf("Value of a = %d\n", a);
            PrintValPtr(ptr);
            PrintValPtr2Ptr(pptr);
        }

        public static void PrintValPtr(ref @int ptr)
        {
            fmt.Printf("Value available at *ptr = %d\n", ptr.Deref);
        }

        public static void PrintValPtr2Ptr(Ptr<Ptr<@int>> pptr)
        {
            fmt.Printf("Value available at **pptr = %d\n", pptr.Deref.Deref);
        }
    }
}
