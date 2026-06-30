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

internal static ж<Δmark> Ꮡgm = new(default(Δmark));
internal static ref Δmark gm => ref Ꮡgm.val;

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
    var pid = Ꮡgm.of(Δmark.Ꮡid);
    pid.val = 99;
    fmt.Println(gm.id);
    var h2 = new holder(mark: 5, extra: 6);
    fmt.Println(h2.mark, h2.extra);
}

} // end main_package
