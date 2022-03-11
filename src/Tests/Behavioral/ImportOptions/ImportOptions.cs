namespace go;

using a = a_package;
using b = b_package;
/* Outer upper import comment */ // Inner upper import comment
using fmt = fmt_package; // fmt comment
using _math_ = math_package; /* math comment */
using _file_ = path.file_package; // path/file comment
// Intra import comments
using static math.rand_package; // math/rand comment
using os = os_package; // os comment
using @implicit = text.tabwriter_package; // implicit comment
/* Inner lower import comment */ // Outer lower import comment

using time = time_package;
using sync = sync_package;

public static partial class main_package {

private static void Main() => func((defer, _, _) => {
    fmt.Println(Int());
    var w = @implicit.NewWriter(os.Stdout, 1, 1, 1, ' ', 0);
    defer(w.Flush());
});

} // end main_package
