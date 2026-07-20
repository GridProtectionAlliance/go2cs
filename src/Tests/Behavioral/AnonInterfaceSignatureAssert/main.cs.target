namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct simpleErr {
    internal @string msg;
}

internal static @string Error(this simpleErr e) {
    return e.msg;
}

[GoType] partial struct multiWrap {
    internal slice<error> errs;
}

internal static slice<error> Unwrap(this multiWrap m) {
    return m.errs;
}

[GoType] partial struct singleWrap {
    internal error err;
}

internal static error Unwrap(this singleWrap s) {
    return s.err;
}

[GoType("dyn")] partial interface classify_type {
    error Unwrap();
}

[GoType("dyn")] partial interface classify_typeᴛ1 {
    slice<error> Unwrap();
}

internal static (bool single, bool multi) classify(any v) {
    bool single = default!;
    bool multi = default!;

    (_, single) = v._<classify_type>(ᐧ);
    (_, multi) = v._<classify_typeᴛ1>(ᐧ);
    return (single, multi);
}

[GoType("dyn")] partial interface main_type {
    slice<error> Unwrap();
}

[GoType("dyn")] partial interface main_typeᴛ1 {
    error Unwrap();
}

internal static void Main() {
    var e1 = new simpleErr("boom");
    var m = new multiWrap(errs: new error[]{e1}.slice());
    var s = new singleWrap(err: e1);
    var (ms, mm) = classify(m);
    fmt.Printf("multiWrap:  single=%v multi=%v\n"u8, ms, mm);
    var (ss, sm) = classify(s);
    fmt.Printf("singleWrap: single=%v multi=%v\n"u8, ss, sm);
    var (es, em) = classify(e1);
    fmt.Printf("simpleErr:  single=%v multi=%v\n"u8, es, em);
    {
        var (u, ok) = ((any)m)._<main_type>(ᐧ); if (ok) {
            fmt.Println("multi unwrap count:", len(u.Unwrap()));
        }
    }
    {
        var (u, ok) = ((any)s)._<main_typeᴛ1>(ᐧ); if (ok) {
            fmt.Println("single unwrap:", u.Unwrap());
        }
    }
}

} // end main_package
