namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface badge {
    @string label();
}

[GoType] partial struct gold {
}

internal static @string label(this gold _) {
    return "gold"u8;
}


[GoType("dyn")] partial struct reservedᴛ1 {
    internal badge badge;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();

[GoType("dyn")] partial struct main_type {
    internal badge badge;
}

internal static void Main() {
    badge b = new reservedᴛ1жbadge(reserved);
    fmt.Println(b != default!);
    reserved.Value.badge = new gold(nil);
    fmt.Println(b.label());
    var local = @new<main_type>();
    local.Value.badge = new gold(nil);
    badge b2 = new main_typeжbadge(local);
    fmt.Println("local:", b2.label());
}

} // end main_package
