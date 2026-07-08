namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface TB {
    @string Name();
    void @private();
}

[GoType] partial struct harness {
    internal @string name;
    internal nint deadline;
}

internal static @string Name(this harness h) {
    return h.name;
}

internal static void @private(this harness h) {
}

internal static (nint, bool) Deadline(this harness h) {
    return (h.deadline, true);
}

[GoType("dyn")] partial interface commandContext_type :
    TB
{
    (nint, bool) Deadline();
}

internal static void commandContext(TB t) {
    {
        var (td, ok) = t._<commandContext_type>(ᐧ); if (ok){
            var (d, _) = td.Deadline();
            fmt.Println("Name:", td.Name(), "deadline:", d);
        } else {
            fmt.Println("no deadline");
        }
    }
}

internal static void Main() {
    commandContext(new harness(name: "cmd"u8, deadline: 100));
}

} // end main_package
