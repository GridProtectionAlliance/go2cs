namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void Main() {
    var format = @string (@string f, params ꓸꓸꓸany aʗp) => {
        var a = aʗp.slice();
        return fmt.Sprintf(f, a.ꓸꓸꓸ);
    };
    fmt.Println(format("%s=%d"u8, "x", 1));
    fmt.Println(format("%s=%d"u8, "y", 2));
    var sum = (params ꓸꓸꓸnint numsʗp) => {
        var nums = numsʗp.slice();
        nint total = 0;
        foreach (var (_, n) in nums) {
            total += n;
        }
        return total;
    };
    fmt.Println(sum(1, 2, 3, 4));
    var sumʗ1 = sum;
    var forward = (params ꓸꓸꓸnint aʗp) => {
        var a = aʗp.slice();
        return sumʗ1(a.ꓸꓸꓸ);
    };
    fmt.Println(forward(10, 20, 30));
}

} // end main_package
