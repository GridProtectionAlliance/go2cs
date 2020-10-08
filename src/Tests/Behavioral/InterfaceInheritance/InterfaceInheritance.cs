using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct T1
        {
            public @string name;
        }

        public static void M(this T1 t)
        {
        }
        public static void N(this T1 t)
        {
        }
        public static @string String(this T1 t)
        {
            return "";
        }
        public static @string Error(this T1 t)
        {
            return "";
        }

        public partial struct T2
        {
            public @string name;
        }

        public static void M(this T2 t)
        {
        }
        public static void N(this T2 t)
        {
        }
        public static @string String(this T2 t)
        {
            return "";
        }
        public static @string Error(this T2 t)
        {
            return "";
        }

        public partial interface I
        {
            void M();
        }

        public partial interface V : I, fmt.Stringer, error
        {
            void N();
        }

        private static void Main()
        {
            var m = make_map<I, long>();
            I i1 = I.As(new T1("foo"))!;
            I i2 = I.As(new T2("bar"))!;
            m[i1] = 1L;
            m[i2] = 2L;
            fmt.Println(m);

            var n = make_map<V, long>();
            V v1 = V.As(new T1("foo"))!;
            V v2 = V.As(new T2("bar"))!;
            v1.N();
            v2.M();
            fmt.Print(v1.String());
            fmt.Print(v2.Error());
            n[v1] = 3L;
            n[v2] = 4L;
            fmt.Println(n);
        }
    }
}
