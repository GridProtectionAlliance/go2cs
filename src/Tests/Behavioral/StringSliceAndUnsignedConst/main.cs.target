namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 takesUint32(uint32 v) {
    return v;
}

internal static void Main() {
    fmt.Println(takesUint32(2147483648U));
    @string env = "cpu.feature=on"u8;
    fmt.Println(env[..4] == "cpu.");
    @string field = default!;
    (field, env) = (env[..4], env[5..]);
    fmt.Println(field, env);
    (field, env) = (env, "");
    fmt.Printf("[%s][%s]\n"u8, field, env);
}

} // end main_package
