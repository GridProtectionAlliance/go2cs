namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    var nums = new nint[]{2, 3, 4}.slice();
    nint sum = 0;
    nint i = default;
    nint num = default;
    nint total = default;
    foreach (var (iᴛ1, vᴛ1) in nums) {
        i = iᴛ1;
        num = vᴛ1;

        sum += num;
        total += i;
    }
    fmt.Println("sum:", sum, "total:", total);
    foreach (var (iΔ1, numΔ1) in nums) {
        if (numΔ1 == 3) {
            fmt.Println("index:", iΔ1);
        }
    }
    foreach (var (_, numΔ2) in nums) {
        fmt.Println("num:", numΔ2);
    }
    foreach (var (iΔ2, _) in nums) {
        fmt.Println("index:", iΔ2);
    }
    total = 0;
    foreach ((_, _) in nums) {
        total++;
    }
    fmt.Println("Total:", total);
    var kvs = new map<@string, @string>{["a"u8] = "apple"u8, ["b"u8] = "banana"u8};
    foreach (var (k1, v1) in kvs) {
        fmt.Printf("%s -> %s\n"u8, k1, v1);
    }
    foreach (var (k1, _) in kvs) {
        fmt.Println("key:", k1);
    }
    foreach (var (v1, _) in kvs) {
        fmt.Println("value:", v1);
    }
    @string k = default;
    @string v = default;
    foreach (var (kᴛ1, vᴛ2) in kvs) {
        k = kᴛ1;
        v = vᴛ2;

        fmt.Printf("%s2 -> %s\n"u8, k, v);
    }
    foreach (var (kᴛ2, _) in kvs) {
        k = kᴛ2;

        fmt.Println("key:", k);
    }
    foreach (var (_, vᴛ3) in kvs) {
        v = vᴛ3;

        fmt.Println("val:", v);
    }
    total = 0;
    foreach ((_, _) in kvs) {
        total++;
    }
    fmt.Println("Total:", total);
    foreach (var (iΔ3, c) in @string("go"u8)) {
        fmt.Println(iΔ3, c);
    }
    @string str = "test"u8;
    nint i1 = default;
    rune c1 = default;
    foreach (var (iᴛ2, rᴛ1) in str) {
        i1 = iᴛ2;
        c1 = rᴛ1;

        fmt.Println(i1, c1);
    }
    var arr = new array<nint>(5){[2] = 42, [4] = 100};
    foreach (var (iΔ4, vΔ1) in arr) {
        fmt.Println(iΔ4, vΔ1);
    }
    var slice = new slice<nint>(5){[2] = 42, [4] = 100};
    foreach (var (iΔ5, vΔ2) in slice) {
        fmt.Println(iΔ5, vΔ2);
    }
    nint v2 = default;
    foreach (var (iᴛ3, vᴛ4) in slice) {
        i1 = iᴛ3;
        v2 = vᴛ4;

        fmt.Println(i1, v2);
    }
}

} // end main_package
