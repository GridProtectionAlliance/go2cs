namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct setting {
    internal @string name;
    internal nint count;
}

[GoRecv] internal static nint bump(this ref setting s) {
    s.count++;
    return s.count;
}

[GoType] partial struct Setting {
    internal @string tag;
    internal partial ref ж<setting> setting { get; }
}

internal static ж<setting> lookup(@string name) {
    return Ꮡ(new setting(name: name));
}

public static ж<Setting> New(@string tag) {
    return Ꮡ(new Setting(tag: tag));
}

internal static void Main() {
    var s = New("debug"u8);
    fmt.Println((~s).setting == nil);
    var sʗ1 = s;
    ((Action)(() => func((defer, recover) => {
        defer(() => {
            fmt.Println("recovered:", recover());
        });
        fmt.Println((~sʗ1).name);
    })))();
    ж<setting> p = default!;
    s.Value.setting = p;
    fmt.Println((~s).setting == nil);
    s.Value.setting = lookup("gcpacertrace"u8);
    fmt.Println((~s).setting == nil);
    fmt.Println((~s).name, s.bump(), s.bump());
    var direct = lookup("other"u8);
    s.Value.setting = direct;
    s.Value.count = 10;
    nint before = direct.Value.count;
    nint after = s.bump();
    fmt.Println(before, after, (~direct).count);
}

} // end main_package
