namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct OSArch {
    internal @string os;
    internal @string arch;
}

[GoType] partial struct info {
    internal bool supported;
}

internal static map<OSArch, info> table = new map<OSArch, info>{
    [new("linux"u8, "amd64"u8)] = new(supported: true),
    [new("windows"u8, "386"u8)] = new(supported: false)
};

internal static void Main() {
    fmt.Println(table[new OSArch("linux", "amd64")].supported);
    fmt.Println(table[new OSArch("windows", "386")].supported);
    fmt.Println(len(table));
}

} // end main_package
