namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    var nums = new nint[] {2, 3, 4}.slice();
    nint sum = 0;
    nint i = default;
    nint num = default;
    nint total = default;
    for (i = 0; i < len(nums); i++) {
        num = nums[i];

        sum += num;
        total += i;
    }
    fmt.Println("sum:", sum, "total:", total);
    for (var iΔ1 = 0; iΔ1 < len(nums); iΔ1++) {
        var numΔ1 = nums[iΔ1];

        if (numΔ1 == 3) {
            fmt.Println("index:", iΔ1);
        }
    }
    foreach (var numΔ2 in nums) {
        fmt.Println("num:", numΔ2);
    }
    for (var iΔ2 = 0; iΔ2 < len(nums); iΔ2++) {
        fmt.Println("index:", iΔ2);
    }
    total = 0;
    foreach (var _ in nums) {
        total++;
    }
    fmt.Println("Total:", total);
    var kvs = new map<@string, @string>{ ["a"u8] = "apple"u8, ["b"u8] = "banana"u8 };
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
    foreach (var (kᴛ1, vᴛ1) in kvs) {
        k = kᴛ1;
        v = vᴛ1;

        fmt.Printf("%s2 -> %s\n"u8, k, v);
    }
    foreach (var (kᴛ2, _) in kvs) {
        k = kᴛ2;

        fmt.Println("key:", k);
    }
    foreach (var (_, vᴛ2) in kvs) {
        v = vᴛ2;

        fmt.Println("val:", v);
    }
    total = 0;
    foreach (var (_, _) in kvs) {
        total++;
    }
    fmt.Println("Total:", total);
    foreach (var (iΔ3, c) in @string("go"u8)) {
        fmt.Println(iΔ3, c);
    }
    @string str = "test"u8;
    nint i1 = default;
    rune c1 = default;
    foreach (var (iᴛ1, rᴛ1) in str) {
        i1 = iᴛ1;
        c1 = str[iᴛ1];

        fmt.Println(i1, c1);
    }
    var arr = new nint[5];
    arr[2] = 42;
    arr[4] = 100;
    for (var iΔ4 = 0; iΔ4 < len(arr); iΔ4++) {
        var vΔ1 = arr[iΔ4];

        fmt.Println(iΔ4, vΔ1);
    }
    var slice = new slice<nint>(5){ [2] = 42, [4] = 100 };
    for (var iΔ5 = 0; iΔ5 < len(slice); iΔ5++) {
        var vΔ2 = slice[iΔ5];

        fmt.Println(iΔ5, vΔ2);
    }
}

} // end main_package
