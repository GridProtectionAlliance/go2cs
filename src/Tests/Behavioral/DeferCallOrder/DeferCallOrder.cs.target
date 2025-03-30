namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, _) => {
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "First", defer);
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "Second", defer);
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "Third", defer);
    var f1 = fmt.Println;
    var f1ʗ1 = f1;
    deferǃ(ᴛ1 => f1ʗ1(ᴛ1), "Fourth", defer);
    deferǃ(GetPrintLn(), "Fifth", defer);
    fmt.Println("Main function");
});

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

} // end main_package
