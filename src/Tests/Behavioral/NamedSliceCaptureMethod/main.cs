namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]nint")] partial struct stack;

internal static void growTo(ж<stack> Ꮡs, nint v) {
    ref var s = ref Ꮡs.Value;

    s = append(s, v);
}

internal static void shrink(ж<stack> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    s = (s)[..(int)(len(s) - 1)];
}

internal static void push(this ж<stack> Ꮡs, nint v) {
    ref var s = ref Ꮡs.Value;

    growTo(Ꮡs, v);
}

internal static nint pop(this ж<stack> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    nint v = (s)[len(s) - 1];
    shrink(Ꮡs);
    return v;
}

internal static void Main() {
    ref var st = ref heap<stack>(out var Ꮡst);
    Ꮡst.push(10);
    Ꮡst.push(20);
    Ꮡst.push(30);
    fmt.Println(len(st));
    fmt.Println(Ꮡst.pop());
    fmt.Println(Ꮡst.pop());
    fmt.Println(len(st));
}

} // end main_package
