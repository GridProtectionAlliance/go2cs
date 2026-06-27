namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() => func((defer, recover) => {
    fmt.Println(fmt.Errorf("plain message"u8));
    fmt.Println(fmt.Errorf("got %v"u8, 42));
    fmt.Println(fmt.Errorf("name %s = %d"u8, "x", 7));
    fmt.Println(fmt.Errorf("%v and %v"u8, true, "y"));
    func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(fmt.Errorf("recovered: %v"u8, r));
                }
            }
        });
        throw panic("kaboom");
    });
    var @base = fmt.Errorf("base failure"u8);
    var wrapped = fmt.Errorf("while doing X: %w"u8, @base);
    fmt.Println(wrapped);
    fmt.Println(wrapped.Error());
});

} // end main_package
