// package fmt -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/interp/testdata/src/fmt" ==> using fmt = go.golang.org.x.tools.go.ssa.interp.testdata.src.fmt_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\src\fmt\fmt.go


namespace go.golang.org.x.tools.go.ssa.interp.testdata.src;

public static partial class fmt_package {

public static @string Sprint(params object[] args);

public static void Print(params object[] args) {
    args = args.Clone();

    foreach (var (i, arg) in args) {
        if (i > 0) {>>MARKER:FUNCTION_Sprint_BLOCK_PREFIX<<
            print(" ");
        }
        print(Sprint(arg));

    }
}

public static void Println(params object[] args) {
    args = args.Clone();

    Print(args);
    println();
}

// formatting is too complex to fake

public static @string Printf(params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    panic("Printf is not supported");
});
public static @string Sprintf(@string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    panic("Sprintf is not supported");
});

} // end fmt_package
