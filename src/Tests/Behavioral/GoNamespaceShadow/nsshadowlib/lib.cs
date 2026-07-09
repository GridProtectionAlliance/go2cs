namespace go.go;

using math = math_package;
using rand = global::go.math.rand_package;
using global::go.math;

partial class nsshadow_package {

public static nint Add(nint x, nint y) {
    return x + y;
}

public static nint Max8() {
    return math.MaxInt8;
}

public static nint Pad() {
    return rand.Intn(1);
}

} // end nsshadow_package
