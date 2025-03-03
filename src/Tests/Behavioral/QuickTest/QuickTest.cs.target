namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Option<T>
    where T : new()
{
    public T value;
    public bool valid;
}

internal static void Main() {
    var optionPtr = Ꮡ(new Option<@string>(
        value: "hello"u8,
        valid: true
    ));
    fmt.Printf("Option value: %s, valid: %t\n"u8, (~optionPtr).value, (~optionPtr).valid);
}

} // end main_package
