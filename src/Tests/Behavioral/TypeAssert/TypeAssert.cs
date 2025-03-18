namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    // Safe assertions with 'ok'
    safeAssertions();
    // Unsafe assertion with recover
    assertionsWithPanic();
    fmt.Println("Program completed after panic recovery");
}

internal static void safeAssertions() {
    any i = "hello";
    // Type assertion with ok check
    {
        var (s, ok) = i._<@string>(ᐧ); if (ok){
            fmt.Println("Value is a string:", s);
        } else {
            fmt.Println("Value is not a string");
        }
    }
    // This will not panic, just set ok to false
    {
        var (n, ok) = i._<nint>(ᐧ); if (ok){
            fmt.Println("Value is an int:", n);
        } else {
            fmt.Println("Value is not an int");
        }
    }
}

internal static void assertionsWithPanic() => func((defer, recover) => {
    // This function will be called when defer executes, after the panic
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                fmt.Println("Recovered from panic:", r);
            }
        }
    });
    any i = "hello";
    // Safe type assertion
    @string s = i._<@string>();
    fmt.Println("String value:", s);
    // This will cause a panic, but we'll recover from it
    nint n = i._<nint>();
    // This line will panic
    // Code below the panic will not execute
    fmt.Println("Integer value:", n);
});

} // end main_package
