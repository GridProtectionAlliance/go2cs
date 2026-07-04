namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct item {
    internal nint n;
}

[GoType("num:uintptr")] partial struct handle;

internal static readonly handle invalid = /* ^handle(0) */ unchecked((handle)18446744073709551615);

internal static readonly UntypedInt appErr = /* 1 << 29 */ 536870912;

internal static readonly handle big = /* appErr + 5 */ 536870917;

internal static nint idx(this handle h) {
    return (nint)(uintptr)(h - (handle)appErr);
}

internal static (ж<item>, error) find(slice<item> xs, nint want) => func<(ж<item>, error)>((defer, recover) => {
    deferǃ(ᴛ1 => fmt.Println(ᴛ1), "find done", defer);
    foreach (var (i, _) in xs) {
        if (xs[i].n == want) {
            return (Ꮡ(xs, i), default!);
        }
    }
    return (default!, fmt.Errorf("not found"u8));
});

internal static error closeIt(ж<nint> Ꮡp, nint tag) {
    ref var p = ref Ꮡp.DerefOrNil();

    fmt.Println("closeIt", Ꮡp == nil, tag);
    return default!;
}

internal static byte first(array<byte> value) {
    value = value.Clone();

    var p = Ꮡ(value).at<byte>(0);
    return p.Value;
}

internal static void Main() => func((defer, recover) => {
    deferǃ(closeIt, (ж<nint>)(nil), (nint)(3), defer);
    ref var h = ref heap<res>(out var Ꮡh);
    h = new res(id: 4);
    var hʗ1 = h;
    defer(() => hʗ1.close());
    var xs = new item[]{new(1), new(2)}.slice();
    var (p, err) = find(xs, 2);
    fmt.Println(p != nil, err == default!);
    (_, err) = find(xs, 9);
    fmt.Println(err);
    fmt.Println(first(new byte[]{7, 8, 9, 10}.array()));
    fmt.Println(big.idx());
    fmt.Println(invalid == big, invalid == invalid);
});

[GoType] partial struct res {
    internal nint id;
}

internal static error close(this res h) {
    fmt.Println("closed", h.id);
    return default!;
}

} // end main_package
