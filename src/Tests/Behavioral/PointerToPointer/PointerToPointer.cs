using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct Buffer
        {
            public slice<byte> buf;
            public long off;
            public sbyte lastRead;
        }

        private static readonly sbyte opRead = (sbyte)-1L;
        private static readonly sbyte opInvalid = (sbyte)0L;

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

            /* take the value using pptr */
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

        private static (long, error) Read(this ptr<Buffer> _addr_b, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref Buffer b = ref _addr_b.val;

            b.lastRead = opInvalid;
            b.off += n;
            if (n > 0L)
            {
                b.lastRead = opRead;
            }
            (addr(new Buffer(buf:p))).Read(p);

            return (n, error.As(null!)!);
        }

        public static ptr<Buffer> NewBuffer(slice<byte> buf)
        {
            ptr<Buffer> b1 = default!;

            return addr(new Buffer(buf:buf));
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
            return _addr_ptr!;
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
