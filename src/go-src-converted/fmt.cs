// package fmt -- go2cs converted at 2020 October 08 04:57:34 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/fmt" ==> using fmt = go.golang.org.x.tools.go.ssa.interp.testdata.src.fmt_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\fmt\fmt.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ssa {
namespace interp {
namespace testdata {
namespace src
{
    public static partial class fmt_package
    {
        public static @string Sprint(params object[] args)
;

        public static void Print(params object[] args)
        {
            args = args.Clone();

            foreach (var (i, arg) in args)
            {
                if (i > 0L)
                {>>MARKER:FUNCTION_Sprint_BLOCK_PREFIX<<
                    print(" ");
                }

                print(Sprint(arg));

            }

        }

        public static void Println(params object[] args)
        {
            args = args.Clone();

            Print(args);
            println();
        }

        // formatting is too complex to fake

        public static @string Printf(params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            panic("Printf is not supported");
        });
        public static @string Sprintf(@string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            panic("Sprintf is not supported");
        });
    }
}}}}}}}}}
