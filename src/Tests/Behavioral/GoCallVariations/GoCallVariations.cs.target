namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    goǃ(ᴛ1 => fmt.Println(ᴛ1), "First");
    goǃ(ᴛ1 => fmt.Println(ᴛ1), "Second");
    goǃ(ᴛ1 => fmt.Println(ᴛ1), "Third");
    var f1 = fmt.Println;
    var f1ʗ1 = f1;
    goǃ(ᴛ1 => f1ʗ1(ᴛ1), "Fourth");
    goǃ(GetPrintLn(), "Fifth");
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "Function result:", add(3, 4));
    printSquare(5);
    nint count = 1;
    goǃ(() => {
        fmt.Println("Go count (closure):", count);
    });
    count = 10;
    fmt.Println("Count before Go:", count);
    time.Sleep(200);
    fmt.Println("Main function");
}

public static Action<@string> GetPrintLn() {
    return (@string src) => {
        fmt.Println(src);
    };
}

internal static nint add(nint x, nint y) {
    nint result = x + y;
    fmt.Println("Calculate:", result);
    return result;
}

internal static void printSquare(nint n) {
    goǃ((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), "Go thread square:", n * n);
    n++;
    fmt.Println("Immediate n:", n);
}

} // end main_package
