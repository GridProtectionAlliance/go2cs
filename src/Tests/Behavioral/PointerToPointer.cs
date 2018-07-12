// package main -- go2cs converted at 2018 July 12 03:35:10 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\PointerToPointer.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            fmt.Printf("Value of a = %d\n", a);
            PrintValPtr(ptr);
            PrintValPtr2Ptr(pptr);
        }

        public static void PrintValPtr(ref long ptr)
        {
            fmt.Printf("Value available at *ptr = %d\n", ptr.Deref);
        }

        public static void PrintValPtr2Ptr(Ptr<Ptr<long>> pptr)
        {
            fmt.Printf("Value available at **pptr = %d\n", pptr.Deref.Deref);
        }
    }
}
