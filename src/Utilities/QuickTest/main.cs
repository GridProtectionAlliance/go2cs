// This is just testing code for quick validations and experiments...

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable ShiftExpressionRightOperandNotEqualRealCount
// ReSharper disable UnusedVariable
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeThisQualifier
// ReSharper disable NotAccessedField.Local
// ReSharper disable RedundantAssignment
// ReSharper disable StructCanBeMadeReadOnly
// ReSharper disable PossibleNullReferenceException
#pragma warning disable 414
#pragma warning disable 219

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using go.runtime;
using @unsafe = go.unsafe_package;
//using go2cs.AST;

namespace go;

public static unsafe partial class main_package
{

    public record Node(string NodeType, int RefId);

    public record PositionNode(string NodeType, int RefId, string Filename, int Offset, int Line, int Column) : Node(NodeType, RefId);

    public static void JsonTest()
    {
        //const string RootPath = @"..\..\..\..\..";
        //using FileStream stream = File.OpenRead($@"{RootPath}\Tests\solitaire.json");
        //var file = JsonSerializer.Deserialize<FileNode>(stream);
    }

    private static ref int Test()
    {
        ref int c = ref Ꮡ(12).val;

        return ref c;
    }

    private static void Main()
    {
        @string str = testUnsafe();

        Console.WriteLine(str);

        var ptr = @unsafe.StringData(str);

        Console.WriteLine($"ptr = {ptr}, len = {str.Length}");

        Console.WriteLine(@unsafe.String(ptr, len(str)));

        JsonTest();

        ref int c = ref Test();

        Console.WriteLine(c);

        bool b = false;
        @string s = "hello"u8;
        //int i = 12;
        object eb = b;
        object ai = (int)12;

        //@string y1 = 0x266c;

        //char x = '\x266c';

        //Console.WriteLine(x);

        Main1();
        Main2();
        Main3();
        Main4();
        Main5();
        Main6();
        Main7();
        ref int value = ref Main8();

        Console.WriteLine("Ref int out1 = {0}", value);

        value = Main9().val;

        Console.WriteLine("Ref int out2 = {0}", value);
        Console.WriteLine();

        Main10();
        Main11();

        Console.ReadLine();
    }

    public partial interface Abser
    {
        double Abs();
    }

    // Right operand of shift operators should always be cast it int
    private static readonly dynamic intSize = 32 << (int)(~(uint)0 >> 63);

    private static readonly dynamic maxUint64 = (1 << 64 - 1);

    // A NumError records a failed conversion.
    public struct NumError
    {
        public @string Func; // the failing function (ParseBool, ParseInt, ParseUint, ParseFloat)
        public @string Num;  // the input
        public error Err;    // the reason the conversion failed (e.g. ErrRange, ErrSyntax, etc.)
    }

    private static ref NumError syntaxError(@string fn, @string str)
    {
        return ref new ж<NumError>(new NumError{Func = fn, Num = str, Err = null}).val;
    }

    public struct ColorList
    {
        public long Total;
        public string Color;
        public ж<ColorList> Next;
        public ж<ж<ColorList>> NextNext;
    }

    public partial struct MyError
    {
        public DateTime When;
        public string What;
    }

    public partial struct MyError
    {
        public MyError(NilType _)
        {
            When = default;
            What = "";
        }

        public MyError(DateTime When, string What)
        {
            this.When = When;
            this.What = What;
        }
    }

#pragma warning disable CS0282
    public partial struct MyCustomError : Abser
    {
        public string Message;

        public Abser Abser;

        public ref MyError MyError => ref MyError_val;
    }
#pragma warning restore CS0282

    public partial struct MyCustomError
    {
        // Abser.Abs function promotion
        private delegate double AbsByVal(MyCustomError value);
        private delegate double AbsByRef(ref MyCustomError value);

        private static readonly AbsByVal s_AbsByVal;
        private static readonly AbsByRef s_AbsByRef;

        [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Abs() => s_AbsByRef?.Invoke(ref this) ?? s_AbsByVal?.Invoke(this) ?? Abser?.Abs() ?? throw new Exception(); //RuntimeErrorPanic.NilPointerDereference();

        // MyError structure promotion
        private readonly ж<MyError> m_MyErrorRef;

        private ref MyError MyError_val => ref m_MyErrorRef.val;

        public ref DateTime When => ref m_MyErrorRef.val.When;

        public ref string What => ref m_MyErrorRef.val.What;            

        [DebuggerStepperBoundary]
        static MyCustomError()
        {
            Type targetType = typeof(MyCustomError);
            MethodInfo? method;

            // Any existing defined extensions will override interface reference calls
            method = targetType.GetExtensionMethod("Abs");

            if (method != null)
            {
                s_AbsByRef = method.CreateStaticDelegate(typeof(AbsByRef)) as AbsByRef;
                
                if (s_AbsByRef == null)
                    s_AbsByVal = method.CreateStaticDelegate(typeof(AbsByVal)) as AbsByVal;
            }
        }

        // Constructors
        public MyCustomError(NilType _)
        {
            this.Message = "";
            this.Abser = null;
            this.m_MyErrorRef = new ж<MyError>(new MyError(nil));
        }

        public MyCustomError(string Message, Abser Abser, MyError MyError)
        {
            this.Message = Message;
            this.Abser = Abser;
            this.m_MyErrorRef = new ж<MyError>(MyError);
        }
    }

    private struct buffer
    {

    }

#pragma warning disable CS0649
    // flags placed in a separate struct for easy clearing.
    private struct fmtFlags
    {
        public bool widPresent;
        public bool precPresent;
        public bool minus;
        public bool plus;
        public bool sharp;
        public bool space;
        public bool zero;

        // For the formats %+v %#v, we set the plusV/sharpV flags
        // and clear the plus/sharp flags since %+v and %#v are in effect
        // different, flagless formats set at the top level.
        public bool plusV;
        public bool sharpV;
    }

    //private partial struct fmtFlags
    //{
    //    public static bool operator ==(fmtFlags value, NilType nil) => value.Equals(default(fmtFlags));
    //    public static bool operator !=(fmtFlags value, NilType nil) => !(value == nil);
    //    public static bool operator ==(NilType nil, fmtFlags value) => value == nil;
    //    public static bool operator !=(NilType nil, fmtFlags value) => value != nil;
    //}

    // A fmt is the raw formatter used by Printf etc.
    // It prints into a buffer that must be set up separately.
    private struct fmt
    {
        public ж<buffer> buf;

        public fmtFlags fmtFlags;

        public int wid;  // width
        public int prec; // precision

        // intbuf is large enough to store %b of an int64 with a sign and
        // avoids padding at the end of the struct on 32 bit architectures.
        public fixed byte intbuf[68];
    }
#pragma warning restore CS0649

    private static void clearFlags(ref this fmt _this) => func(ref _this, (ref fmt f, Defer defer, Recover recover) =>
    {
        f.fmtFlags = new fmtFlags();
    });

    private static string Error(this MyError myError)
    {
        //ValueTuple<double> AnotherType; // = new ValueTuple<double>();

        fmt f = new fmt();

        val(ref f.buf.val);

        f.clearFlags();

        return $"at {myError.When}, {myError.What}";
    }

    private static void val(ref buffer buf)
    {

    }

    public struct MyFloat
    {
        private readonly double m_value;

        public MyFloat(double value) => m_value = value;

        public static implicit operator MyFloat(double value) => new MyFloat(value);
        public static implicit operator double(MyFloat value) => value.m_value;
    }

    private static void Main1()
    {
        complex128 vi1 = new complex128(0.0D, 0.0D);

        // 12 * 1.5 + 1i * iota / 3 + 2i
        //for (int iota = 0; iota < 2; iota++)
        //{
        //    complex128 result = 12 * 1.5 + i(1) * iota / 3 + i(2);
        //    Console.WriteLine(result);
        //}

        //Complex t1 = new Complex(12 * 1.5, 1 * 1 / 3 + 2);
        Abser ab;
        MyFloat f = (MyFloat)(-Math.Sqrt(2));
        Vertex v = new Vertex { X = 3, Y = 4 };

        ab = Abser_cast(f);
        ab = (Abser<MyFloat>)f;
        ab = Abser_cast(v);

        var err = ab.TypeAssert<Vertex>();

        Console.WriteLine("Type asserted ab = " + err.GetType().Name);

        //GoString test1 = ab.TypeAssert<GoString>();
        //bool ok = ab.TryTypeAssert(out @string test);

        Console.WriteLine(ab.Abs());
    }

    public static double Abs(this MyFloat f)
    {
        if (f < 0)
            return -f;

        return f;
    }

    public struct Vertex
    {
        public double X;
        public double Y;
    }

    public static double Abs(this Vertex v)
    {
        return Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    private static void Main2()
    {
        slice<int> s = new slice<int>(new[] { 2, 3, 5, 7, 11, 13 });
        printSlice(s);

        // Slice the slice to give it zero length.
        s = s.slice(high: 0);
        printSlice(s);

        // Extend its length.
        s = s.slice(high: 4);
        printSlice(s);

        // Drop its first two values.
        s = s.slice(2);
        printSlice(s);
    }

    private static void printSlice(slice<int> s)
    {
        Console.WriteLine("len={0} cap={1} {2}", len(s), cap(s), s);
    }

    private static void Main3()
    {
        slice<int> s = new slice<int>();
        Console.WriteLine("{0} {1} {2}", s, len(s), cap(s));

        if (s == nil)
            Console.WriteLine("nil!");
    }

    private static void Main4()
    {
        // slice of slice board creation with array slice extension:
        var board = new[]
        {
            new @string[] {"_"u8, "_"u8, "_"u8}.slice(),
            new @string[] {"_"u8, "_"u8, "_"u8}.slice(),
            new @string[] {"_"u8, "_"u8, "_"u8}.slice()
        }.slice();

        // slice of slice board creation with make_slice function:
        //var board = make_slice(new[]
        //{
        //    make_slice(new @string[] {"_"u8, "_"u8, "_"u8}), 
        //    make_slice(new @string[] {"_"u8, "_"u8, "_"u8}),
        //    make_slice(new @string[] {"_"u8, "_"u8, "_"u8})
        //});

        // slice of slice board creation standard / verbose:
        //var board = new slice<slice<@string>>(new slice<@string>[]
        //{
        //    new slice<@string>(new @string[] {"_"u8, "_"u8, "_"u8}),
        //    new slice<@string>(new @string[] {"_"u8, "_"u8, "_"u8}),
        //    new slice<@string>(new @string[] {"_"u8, "_"u8, "_"u8})
        //});

        // The players take turns.
        board[0][0] = "X"u8;
        board[2][2] = "O"u8;
        board[1][2] = "X"u8;
        board[1][0] = "O"u8;
        board[0][2] = "X"u8;

        for (var i = 0; i < len(board); i++)
        {
            Console.WriteLine("{0}", string.Join(" ", board[i]));
        }
    }

    private static void Main5()
    {
        //Slice<int> s = make(Slice<int>.Nil, 0, 5);
        //printSlice(s);

        //for (int j = 0; j < 33; j++)
        //{
        //    s = append(s, j);
        //    printSlice(s);
        //}

        slice<int> s = new slice<int>();
        printSlice(s);

        // append works on nil slices.
        s = append(s, 0);
        printSlice(s);

        // The slice grows as needed.
        s = append(s, 1);
        printSlice(s);

        // We can add more than one element at a time.
        s = append(s, 2, 3, 4);
        printSlice(s);
    }

    private static void Main6()
    {
        f();
        Console.WriteLine("Returned normally from f.");

        //f2();
        //Console.WriteLine("Returned normally from f.");
    }

    private static void f() => func((defer, recover) =>
    {
        defer(handleError);

        Console.WriteLine("Calling g.");
        g(0);
        Console.WriteLine("Returned normally from g.");
    });

    private static void g(int i) => func((defer, recover) =>
    {
        if (i > 3)
        {
            Console.WriteLine("Panicking!");
            panic($"{i}");
        }

        defer(() => Console.WriteLine($"Defer in g {i}"));
        Console.WriteLine($"Printing in g {i}");
        g(i + 1);
    });

    private static void handleError() => func((defer, recover) =>
    {
        var r = recover();

        if (r != nil)
            Console.WriteLine($"Recovered in f {r}");
    });

    //private static void f2() => func((defer, panic, recover) =>
    //{
    //    defer(() =>
    //    {
    //        {
    //            var r = recover();

    //            if (r != nil)
    //                Console.WriteLine($"Recovered in f2 {r}");
    //        }
    //    });

    //    Console.WriteLine("Calling g.");
    //    (string name, int age) = g2(0);
    //    Console.WriteLine($"Returned normally from g2: {name} - {age}.");
    //});

    //private static (string message, int err) g2(int i) => func((defer, panic, recover) =>
    //{
    //    string message = "Test";
    //    int err = 12;

    //    if (i > 3)
    //    {
    //        Console.WriteLine("Panicking!");
    //        panic($"{i}");
    //    }

    //    defer(() => Console.WriteLine($"Defer in g2 {i}"));
    //    Console.WriteLine($"Printing in g2 {i}");
    //    g2(i + 1);

    //    return (message, err);
    //});

    private static void Main7()
    {
        int i = 42, j = 2701;

        int* p = &i;
        Console.WriteLine(*p);
        *p = 21;
        Console.WriteLine(i);

        p = &j;
        *p = *p / 37;
        Console.WriteLine(j);

        Vertex v = new Vertex {X = 1, Y = 2};
        Vertex* pv = &v;

        pv->X = 12;

        (*pv).X = 99;

        PrintVertex(ref v);
    }

    private static ref int Main8()
    {
        ж<int> i = new ж<int>(42), j = new ж<int>(2701);

        ref int p = ref i.val;
        Console.WriteLine(p);
        p = 21;
        Console.WriteLine(i);

        p = ref j.val;
        p = p / 37;
        Console.WriteLine(j);

        ж<Vertex> v = new ж<Vertex>(new Vertex { X = 1, Y = 2 });
        ref Vertex pv = ref v.val;

        pv.X = 12;

        pv.X = 99;

        PrintVertex(ref v.val);

        return ref i.val;
    }

    private static ж<int> Main9()
    {
        ж<int> i = new ж<int>(42);

        //ptr<int> j = new ptr<int>(2701);

        //ptr<int> p = new ptr<int>(~i);
        //Console.WriteLine(p);
        //p.Value = 21;
        //Console.WriteLine(i);

        //p.Value = j;
        //p.Value = p.Value / 37;
        //Console.WriteLine(j);

        //ptr<Vertex> v = new ptr<Vertex>(new Vertex { X = 1, Y = 2 });
        //ptr<Vertex> pv = new ptr<Vertex>(~v);

        //pv.Value.X = 12;

        //pv.Value.X = 99;

        //PrintVertex(ref v.Value);

        return i;
    }

    public static void PrintVertex(ref Vertex v)
    {
        Console.WriteLine("Value of vertex X = {0}\n", v.X);
    }

    private static void Main10()
    {
        array<@string> a = new array<@string>(2);

        a[0] = "Hello";
        a[1] = "World";

        test(a);
        Console.WriteLine("{0} {1}", a[0], a[1]);
        Console.WriteLine();

        a[0] = "Hello";
        test2(ref a);
        Console.WriteLine("{0} {1}", a[0], a[1]);
        Console.WriteLine();

        a[0] = "Hello";
        test3(a.slice());
        Console.WriteLine("{0} {1}", a[0], a[1]);
        Console.WriteLine();

        var primes = new array<int>(new[] { 2, 3, 5, 7, 11, 13 });
        Console.WriteLine(primes);
    }

    // Arrays are passed by value (a full copy)
    private static void test(array<@string> a)
    {
        a = a.Clone();

        Console.WriteLine("{0} {1}", a[0], a[1]);
        a[0] = "Goodbye";
        Console.WriteLine("{0} {1}", a[0], a[1]);
    }

    private static void test2(ref array<@string> a)
    {
        Console.WriteLine("{0} {1}", a[0], a[1]);
        a[0] = "Goodbye";
        Console.WriteLine("{0} {1}", a[0], a[1]);
    }

    private static void test3(slice<@string> a)
    {
        Console.WriteLine("{0} {1}", a[0], a[1]);
        a[0] = "Goodbye";
        Console.WriteLine("{0} {1}", a[0], a[1]);
    }

    private static void Main11()
    {
        @string a;

        a = "Hello World";

        test(a);
        Console.WriteLine(a);
        Console.WriteLine();

        a = "Hello World";
        test2(ref a);
        Console.WriteLine(a);
    }

    private static void test(@string a)
    {
        Console.WriteLine(a);
        a = "Goodbye World";
        Console.WriteLine(a);
    }

    private static void test2(ref @string a)
    {
        Console.WriteLine(a);
        a = "Goodbye World";
        Console.WriteLine(a);
    }

    public static @string testUnsafe()
    {
        var b = (slice<byte>)[];

        for (rune ch = 32; ch < 80; ch++)
            b = append(b, ch);
        
        // At this point, b has at least one byte '\n'.
        return @unsafe.String(Ꮡ(b, 0), len(b));
    }
}
