using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            ref long a = ref heap(out ptr<long> _addr_a);
            ptr<long> ptr;
            ptr<ptr<long>> pptr;
            ptr<ptr<ptr<long>>> ppptr;

            a = 3000L; 

            /* take the address of var */
            ptr = _addr_a; 

            /* take the address of ptr using address of operator & */
            pptr = addr(ptr);
            ppptr = addr(pptr);

            fmt.Printf("Value of a = %d\n", a);
            PrintValPtr(ptr);
            fmt.Printf("Main-function return value available at *ptr = %d\n", EscapePrintValPtr(ptr).val);
            fmt.Printf("Main-function updated value available at *ptr = %d\n", ptr.val);
            PrintValPtr2Ptr(pptr);
            PrintValPtr2Ptr2Ptr(ppptr);

            a = 1900L;

            fmt.Printf("Value of a = %d\n", a);
            PrintValPtr(ptr);
            fmt.Printf("Main-function return value available at *ptr = %d\n", EscapePrintValPtr(ptr).val);
            fmt.Printf("Main-function updated value available at *ptr = %d\n", ptr.val);
            PrintValPtr2Ptr(pptr);
            PrintValPtr2Ptr2Ptr(ppptr);
        }

        public static void PrintValPtr(ptr<long> _addr_ptr)
        {
            ref long ptr = ref _addr_ptr.val;

            fmt.Printf("Value available at *ptr = %d\n", ptr);
            ptr++;
        }

        public static ptr<long> EscapePrintValPtr(ptr<long> _addr_ptr)
        {
            ref long ptr = ref _addr_ptr.val;

            fmt.Printf("Value available at *ptr = %d\n", ptr);
            ref long i = ref heap(99L, out ptr<long> _addr_i);
            _addr_ptr = _addr_i;
            ptr = ref _addr_ptr.val;
            fmt.Printf("Intra-function updated value available at *ptr = %d\n", ptr);
            PrintValPtr(_addr_ptr);

            return _addr_ptr;
        }

        public static void PrintValPtr2Ptr(ptr<ptr<long>> _addr_pptr)
        {
            ref ptr<long> pptr = ref _addr_pptr.val;

            fmt.Printf("Value available at **pptr = %d\n", pptr.val);
        }

        public static void PrintValPtr2Ptr2Ptr(ptr<ptr<ptr<long>>> _addr_ppptr)
        {
            ref ptr<ptr<long>> ppptr = ref _addr_ppptr.val;

            fmt.Printf("Value available at ***pptr = %d\n", ppptr.val.val);
        }
    }
}
