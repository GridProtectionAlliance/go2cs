namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct wbBuf {
    internal uintptr next;
    internal array<uintptr> buf = new(4);
}

[GoType] partial struct pstate {
    internal nint id;
    internal wbBuf wbBuf;
}

[GoType] partial struct cacheT {
    internal array<array<nint>> entries = new(2, () => new(3));
}

[GoType] partial struct store {
    internal nint id;
    internal slice<uintptr> data;
}

[GoRecv] internal static slice<uintptr> stk(this ref store s) {
    return s.data;
}

internal static void bump(ж<uintptr> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p = p + 7;
}

internal static void bumpInt(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p = p + 3;
}

internal static void Main() {
    var pp = Ꮡ(new pstate(wbBuf: new wbBuf(buf: new uintptr[]{10, 20, 30, 40}.array())));
    bump(pp.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0));
    fmt.Println((~pp).wbBuf.buf[0], (~pp).wbBuf.buf[1]);
    var got = (uintptr)0;
    var ppʗ1 = pp;
    ((Action)(() => {
        bump(ppʗ1.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 1));
        got = (~ppʗ1).wbBuf.buf[1];
    }))();
    fmt.Println(got);
    var cache = Ꮡ(new cacheT(entries: new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()));
    bumpInt(cache.at(cacheT.Ꮡentries, 1).at<nint>(2));
    fmt.Println((~cache).entries[1][2], (~cache).entries[0][0]);
    var st = Ꮡ(new store(id: 5, data: new uintptr[]{11, 22, 33}.slice()));
    bump(Ꮡ(st.stk(), 0));
    fmt.Println((~st).data[0], st.stk()[0]);
    bump(Ꮡ(st.stk(), 2));
    fmt.Println((~st).data[2]);
}

} // end main_package
