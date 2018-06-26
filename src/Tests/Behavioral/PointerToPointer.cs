// package main -- go2cs converted at 2018 June 25 19:10:12 UTC
// Original source: D:\Projects\go2cs\src\Tests\Behavioral\PointerToPointer.go

using fmt = go.fmt_package;
using static go.BuiltInFunctions;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            varaintvarptr*intvarpptr**inta=3000ptr=&apptr=&ptrfmt.Printf("Value of a = %d\n",a)PrintValPtr(ptr)PrintValPtr2Ptr(pptr)
        }

        public static void PrintValPtr(ref long ptr)
        {
            fmt.Printf("Value available at *ptr = %d\n",*ptr)
        }

        public static void PrintValPtr2Ptr(Ptr<Ptr<long>> pptr)
        {
            fmt.Printf("Value available at **pptr = %d\n",**pptr)
        }
    }
}
