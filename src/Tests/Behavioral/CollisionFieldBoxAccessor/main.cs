namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Δmark {
    internal nint id;
}

[GoType] partial struct tagger {
}

internal static void mark(this tagger _) {
}

[GoType] partial struct holder {
    internal nint mark;
    internal nint extra;
}

internal static ж<holder> Ꮡh = new(default(holder));
internal static ref holder h => ref Ꮡh.val;

internal static void Main() {
    var p = Ꮡh.of(holder.Ꮡmark);
    p.val = 42;
    var q = Ꮡh.of(holder.Ꮡextra);
    q.val = 7;
    fmt.Println(h.mark, h.extra);
    Δmark m = default!;
    m.id = 3;
    tagger t = default!;
    t.mark();
    fmt.Println(m.id);
}

} // end main_package
