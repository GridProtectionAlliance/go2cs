using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static go.BuiltInFunctions;

namespace go
{
    public static unsafe partial class main_package
    {
        private static void Main()
        {
            Main1();
            Main2();
            Main3();
            Main4();
            Main5();
            Main6();
            Main7();
            ref int value = ref Main8();

            Console.WriteLine("Ref int out1 = {0}", value);

            value = Main9().Value;

            Console.WriteLine("Ref int out2 = {0}", value);

            GoString a = "hello";
            GoString b = a;

            a += "more";

            Console.WriteLine(a);
            Console.WriteLine(b);

            Console.ReadLine();
        }

        public interface Abser
        {
            double Abs();
        }

        public partial struct ColorList
        {
            public long Total;
            public string Color;
            public Ptr<ColorList> Next;
            public Ptr<Ptr<ColorList>> NextNext;
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

        public partial struct MyCustomError : Abser
        {
            public string Message;

            public Abser Abser;

            public ref MyError MyError => ref MyError_val;
        }

        [PromotedStruct(typeof(MyError))]
        public partial struct MyCustomError
        {
            // Abser.Abs function promotion
            private delegate double AbsByVal(MyCustomError value);
            private delegate double AbsByRef(ref MyCustomError value);

            private static readonly AbsByVal s_AbsByVal;
            private static readonly AbsByRef s_AbsByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double Abs() => s_AbsByRef?.Invoke(ref this) ?? s_AbsByVal?.Invoke(this) ?? Abser?.Abs() ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);

            // MyError structure promotion
            private readonly Ref<MyError> m_MyErrorRef;

            private ref MyError MyError_val => ref m_MyErrorRef.Value;

            public ref DateTime When => ref m_MyErrorRef.Value.When;

            public ref string What => ref m_MyErrorRef.Value.What;            

            [DebuggerStepperBoundary]
            static MyCustomError()
            {
                Type targetType = typeof(MyCustomError);
                MethodInfo method;

                // Any existing defined extensions will override interface reference calls
                method = targetType.GetExtensionMethodSearchingPromotions("Abs");

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
                this.m_MyErrorRef = new Ref<MyError>(new MyError(nil));
            }

            public MyCustomError(string Message, Abser Abser, MyError MyError)
            {
                this.Message = Message;
                this.Abser = Abser;
                this.m_MyErrorRef = new Ref<MyError>(MyError);
            }
        }

        private struct buffer
        {

        }

        // flags placed in a separate struct for easy clearing.
        private partial struct fmtFlags
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

        private partial struct fmtFlags
        {
            public static bool operator ==(fmtFlags value, NilType nil) => value.Equals(default(fmtFlags));
            public static bool operator !=(fmtFlags value, NilType nil) => !(value == nil);
            public static bool operator ==(NilType nil, fmtFlags value) => value == nil;
            public static bool operator !=(NilType nil, fmtFlags value) => value != nil;
        }

        // A fmt is the raw formatter used by Printf etc.
        // It prints into a buffer that must be set up separately.
        private struct fmt
        {
            public Ptr<buffer> buf;

            public fmtFlags fmtFlags;

            public int wid;  // width
            public int prec; // precision

            // intbuf is large enough to store %b of an int64 with a sign and
            // avoids padding at the end of the struct on 32 bit architectures.
            public fixed byte intbuf[68];
        }

        private static void clearFlags(ref this fmt _this) => func(ref _this, (ref fmt f, Defer defer, Panic panic, Recover recover) =>
        {
            f.fmtFlags = new fmtFlags();
        });

        private static string Error(this MyError myError)
        {
            //ValueTuple<double> AnotherType; // = new ValueTuple<double>();

            fmt f = new fmt();

            val(ref f.buf.Deref);

            f.clearFlags();

            return $"at {myError.When}, {myError.What}";
        }

        private static void val(ref buffer buf)
        {

        }

        public partial struct MyFloat
        {
            private readonly double m_value;

            public MyFloat(double value) => m_value = value;

            public static implicit operator MyFloat(double value) => new MyFloat(value);
            public static implicit operator double(MyFloat value) => value.m_value;
        }

        private static void Main1()
        {
            Complex vi1 = new Complex(0, 0);

            // 12 * 1.5 + 1i * iota / 3 + 2i
            for (int iota = 0; iota < 2; iota++)
            {
                Complex result = 12 * 1.5 + i(1) * iota / 3 + i(2);
                Console.WriteLine(result);
            }

            //Complex t1 = new Complex(12 * 1.5, 1 * 1 / 3 + 2);
            Abser ab;
            MyFloat f = (MyFloat)(-Math.Sqrt(2));
            Vertex v = new Vertex { X = 3, Y = 4 };

            ab = Abser_cast(f);
            ab = (Abser<MyFloat>)f;
            ab = Abser_cast(v);

            //GoString test1 = ab.TypeAssert<GoString>();
            bool ok = ab.TryTypeAssert(out GoString test);

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
            Slice<int> s = new Slice<int>(new[] { 2, 3, 5, 7, 11, 13 });
            printSlice(s);

            // Slice the slice to give it zero length.
            s = s.Slice(high: 0);
            printSlice(s);

            // Extend its length.
            s = s.Slice(high: 4);
            printSlice(s);

            // Drop its first two values.
            s = s.Slice(2);
            printSlice(s);
        }

        private static void printSlice(Slice<int> s)
        {
            Console.WriteLine("len={0} cap={1} {2}", len(s), cap(s), s);
        }

        private static void Main3()
        {
            Slice<int> s = new Slice<int>();
            Console.WriteLine("{0} {1} {2}", s, len(s), cap(s));

            if (s == nil)
                Console.WriteLine("nil!");
        }

        private static void Main4()
        {
            Slice<Slice<string>> board = new Slice<Slice<string>>(new[]
            {
            new Slice<string>(new[] {"_", "_", "_"}),
            new Slice<string>(new[] {"_", "_", "_"}),
            new Slice<string>(new[] {"_", "_", "_"}),
        });

            // The players take turns.
            board[0][0] = "X";
            board[2][2] = "O";
            board[1][2] = "X";
            board[1][0] = "O";
            board[0][2] = "X";

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

            Slice<int> s = new Slice<int>();
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

        private static void f() => func((defer, panic, recover) =>
        {
            defer(handleError);

            Console.WriteLine("Calling g.");
            g(0);
            Console.WriteLine("Returned normally from g.");
        });

        private static void g(int i) => func((defer, panic, recover) =>
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

        private static void handleError() => func((defer, panic, recover) =>
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
            Ref<int> i = new Ref<int>(42), j = new Ref<int>(2701);

            ref int p = ref i.Value;
            Console.WriteLine(p);
            p = 21;
            Console.WriteLine(i);

            p = ref j.Value;
            p = p / 37;
            Console.WriteLine(j);

            Ref<Vertex> v = new Ref<Vertex>(new Vertex { X = 1, Y = 2 });
            ref Vertex pv = ref v.Value;

            pv.X = 12;

            pv.X = 99;

            PrintVertex(ref v.Value);

            return ref i.Value;
        }

        private static Ref<int> Main9()
        {
            Ref<int> i = new Ref<int>(42), j = new Ref<int>(2701);

            Ptr<int> p = new Ptr<int>(i);
            Console.WriteLine(p);
            p.Deref = 21;
            Console.WriteLine(i);

            p.Value = j;
            p.Deref = p.Deref / 37;
            Console.WriteLine(j);

            Ref<Vertex> v = new Ref<Vertex>(new Vertex { X = 1, Y = 2 });
            Ptr<Vertex> pv = new Ptr<Vertex>(v);

            pv.Deref.X = 12;

            pv.Deref.X = 99;

            PrintVertex(ref v.Value);

            return i;
        }

        public static void PrintVertex(ref Vertex v)
        {
            Console.WriteLine("Value of vertex X = {0}\n", v.X);
        }
    }
}