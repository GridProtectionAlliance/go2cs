namespace go;

using fmt = fmt_package;

partial class main_package {

public static readonly GoUntyped Two129 = /* 1 << 129 */
    GoUntyped.Parse("680564733841876926926749214863536422912");

internal static void Main() {
    float64 x = 1e40D;
    fmt.Println(x > (float64)Two129);
    fmt.Println(x < (float64)Two129);
    float64 y = 1e30D;
    fmt.Println(y >= (float64)Two129);
}

} // end main_package
