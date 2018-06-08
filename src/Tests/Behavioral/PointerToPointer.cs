// package main -- go2cs converted at 2018 June 08 13:56:38 UTC
// Original source: C:\Projects\go2cs\src\Tests\Behavioral\PointerToPointer.go

using fmt = go.fmt_package;

using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main() => func((defer, panic, recover) =>
        {
            varaintvarptr*intvarpptr**inta=3000ptr=&apptr=&ptrfmt.Printf("Value of a = %d\n",a)PrintValPtr(ptr)PrintValPtr2Ptr(pptr)
        });

        public static void PrintValPtr(ref long _ptr) => func(ref _ptr, (ref long ptr, Defer defer, Panic panic, Recover recover) =>
        {
            fmt.Printf("Value available at *ptr = %d\n",*ptr)
        });

        public static void PrintValPtr2Ptr(long** pptr) => func((defer, panic, recover) =>
        {
            fmt.Printf("Value available at **pptr = %d\n",**pptr)
        });
    }
}
