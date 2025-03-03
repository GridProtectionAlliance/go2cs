namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Box<T>
    where T : new()
{
    public T Value;
}

public static object GetValue(nint index) {
    var values = new object[]{
        new Box<nint>(Value: 42),
        new Box<@string>(Value: "hello"u8),
        new Box<float64>(Value: 3.14F)
    }.slice();
    if (index >= 0 && index < len(values)) {
        return values[index];
    }
    return default!;
}

internal static void Main() {
    var value = GetValue(0);
    {
        var (intBox, ok) = value._<Box<nint>>(ᐧ); if (ok) {
            fmt.Printf("Found an int box with value: %d\n"u8, intBox.Value);
        }
    }
    {
        var (strBox, ok) = value._<Box<@string>>(ᐧ); if (ok){
            fmt.Printf("Found a string box with value: %s\n"u8, strBox.Value);
        } else {
            fmt.Println("Not a string box");
        }
    }
    switch (GetValue(1).type()) {
    case Box<nint> v:
        fmt.Printf("Int box: %d\n"u8, v.Value);
        break;
    case Box<@string> v:
        fmt.Printf("String box: %s\n"u8, v.Value);
        break;
    case Box<float64> v:
        fmt.Printf("Float box: %f\n"u8, v.Value);
        break;
    default: {
        var v = GetValue(1).type();
        fmt.Println("Unknown box type");
        break;
    }}
}

} // end main_package
