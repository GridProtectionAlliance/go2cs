namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Pair<T, U>
    where T : new()
    where U : new()
{
    public T First;
    public U Second;
}

private static void Main() {
    var p = new Pair<@string, nint>(
        First: "answer"u8,
        Second: 42
    );
    fmt.Printf("Pair: %v, %v\n"u8, p.First, p.Second);
}

} // end main_package
