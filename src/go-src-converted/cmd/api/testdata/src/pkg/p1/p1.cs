// package p1 -- go2cs converted at 2020 August 29 08:48:37 UTC
// import "cmd/api/testdata/src/pkg/p1" ==> using p1 = go.cmd.api.testdata.src.pkg.p1_package
// Original source: C:\Go\src\cmd\api\testdata\src\pkg\p1\p1.go
using ptwo = go.p2_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace api {
namespace testdata {
namespace src {
namespace pkg
{
    public static partial class p1_package
    {
        public static readonly var ConstChase2 = constChase; // forward declaration to unexported ident
        private static readonly var constChase = AIsLowerA; // forward declaration to exported ident

        public static readonly long A = 1L;
        private static readonly long a = 11L;
        public static readonly long A64 = 1L;

        public static readonly var AIsLowerA = a; // previously declared

        public static readonly var ConversionConst = MyInt(5L);

        // Variables from function calls.
        public static var V = ptwo.F();        public static var VError = BarE();        public static var V1 = Bar1(1L, 2L, 3L);        public static var V2 = ptwo.G();

        // Variables with conversions:
        public static var StrConv = string("foo");        public static slice<byte> ByteConv = (slice<byte>)"foo";

        public static var ChecksumError = ptwo.NewError("gzip checksum error");

        public static readonly long B0 = 2L;

        public static readonly @string StrConst = "foo";

        public static readonly float FloatConst = 1.5F;



        private partial struct myInt // : long
        {
        }

        public partial struct MyInt // : long
        {
        }

        public partial struct Time
        {
        }

        public partial struct S
        {
            public ptr<long> Public;
            public ptr<long> @private;
            public Time PublicTime;
        }

        public partial struct URL
        {
        }

        public partial struct EmbedURLPtr
        {
            public ref URL URL => ref URL_ptr;
        }

        public partial struct S2
        {
            public ref S S => ref S_val;
            public bool Extra;
        }

        public static long X0 = default;

        public static long Y = default;        public static I X = default;

        public partial interface Namer
        {
            @string Name();
        }

        public partial interface I : Namer, ptwo.Twoer
        {
            long Set(@string name, long balance);
            long Get(@string _p0);
            long GetNamed(@string _p0);
            long @private();
        }

        public partial interface Public
        {
            void X();
            void Y();
        }

        public partial interface Private
        {
            void X();
            void y();
        }

        public partial interface Error : error
        {
            bool Temporary();
        }

        private static void privateTypeMethod(this myInt _p0)
        {
        }
        private static void CapitalMethodUnexportedType(this myInt _p0)
        {
        }

        private static void SMethod(this ref S2 s, sbyte x, short y, long z)
        {
        }

        private partial struct s
        {
        }

        private static void method(this s _p0)

        private static void Method(this s _p0)


        public static void StructValueMethod(this S _p0)

        public static void StructValueMethodNamedRecv(this S ignored)


        private static void unexported(this ref S2 s, sbyte x, short y, long z)
        {
        }

        public static void Bar(sbyte x, short y, long z)
        {
        }
        public static ulong Bar1(sbyte x, short y, long z)
        {
        }
        public static (byte, ulong) Bar2(sbyte x, short y, long z)
        {
        }
        public static Error BarE()
        {
        }

        private static void unexported(sbyte x, short y, long z)
        {
        }

        public static long TakesFunc(Func<long, long> f)
;

        public partial struct Codec
        {
            public Func<long, long, long> Func;
        }

        public partial struct SI
        {
            public long I;
        }

        public static SI SIVal = new SI();
        public static SI SIPtr = ref new SI();
        public static ref SI SIPtr2 = default;

        public partial struct T
        {
            public ref common common => ref common_val;
        }

        public partial struct B
        {
            public ref common common => ref common_val;
        }

        private partial struct common
        {
            public long i;
        }

        public partial struct TPtrUnexported
        {
            public ref common common => ref common_ptr;
        }

        public partial struct TPtrExported
        {
            public ref Embedded Embedded => ref Embedded_ptr;
        }

        public delegate  error) FuncType(long,  long,  @string,  (ref B);

        public partial struct Embedded
        {
        }

        public static (ref B, error) PlainFunc(long x, long y, @string s)
;

        private static void OnEmbedded(this ref Embedded _p0)
        {>>MARKER:FUNCTION_PlainFunc_BLOCK_PREFIX<<
        }

        private static void JustOnT(this ref T _p0)
        {>>MARKER:FUNCTION_TakesFunc_BLOCK_PREFIX<<
        }
        private static void JustOnB(this ref B _p0)
        {
        }
        private static void OnBothTandBPtr(this ref common _p0)
        {
        }
        private static void OnBothTandBVal(this common _p0)
        {
        }

        public partial struct EmbedSelector
        {
            public ref Time Time => ref Time_val;
        }

        private static readonly @string foo = "foo";
        private static readonly @string foo2 = "foo2";
        private static readonly var truth = foo == "foo" || foo2 == "foo2";

        private static void ellipsis(params @string _p0)
        {
        }

        public static Time Now()
        {
            Time now = default;
            return now;
        }

        private static S x = ref new S(Public:nil,private:nil,PublicTime:Now(),);

        private static long parenExpr = (1L + 5L);

        private static Action funcLit = () =>
        {
        };

        private static map<@string, long> m = default;

        private static channel<long> chanVar = default;

        private static long ifaceVar = 5L;

        private static long assertVar = ifaceVar._<long>();

        private static var indexVar = m["foo"];

        public static byte Byte = default;
        public static Func<byte, int> ByteFunc = default;

        public partial struct ByteStruct
        {
            public byte B;
            public int R;
        }
    }
}}}}}}
