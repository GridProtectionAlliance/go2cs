namespace go;

using fmt = fmt_package;
using ꓸꓸꓸnint = System.Span<nint>;

public static partial class main_package {

private static nint sum(params ꓸꓸꓸnint numsʗp) {
    var nums = numsʗp.slice();

    nint total = 0;
    foreach (var (_, n) in nums) {
        total += n;
    }
    return total;
}

private static void Main() {
    var values = new nint[]{1, 2, 3}.slice();
    fmt.Println(sum(values.ꓸꓸꓸ));
    fmt.Println(sum(1, 2, 3));
}

} // end main_package
