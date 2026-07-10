namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 0;
    while (i < 10) {
        f(Ꮡi);
        i++;
    }
    fmt.Println();
    fmt.Println("i =", i);
    for (i = 0; i < 10; i++) {
        f(Ꮡi);
        for (nint j = 0; j < 3; j++) {
            fmt.Println(i + j);
        }
        fmt.Println();
    }
    fmt.Println("i =", i);
    fmt.Println();
@out:
    for (nint iΔ1ᴛ1 = 0; iΔ1ᴛ1 < 5; iΔ1ᴛ1++) {
        ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1);
        iΔ1 = iΔ1ᴛ1;
        f(ᏑiΔ1);
        for (nint iΔ2ᴛ1 = 12; iΔ2ᴛ1 < 15; iΔ2ᴛ1++) {
            ref var iΔ2 = ref heap<nint>(out var ᏑiΔ2);
            iΔ2 = iΔ2ᴛ1;
            f(ᏑiΔ2);
            goto break_out;
            iΔ2ᴛ1 = iΔ2;
        }
        if (iΔ1 > 13) {
            goto continue_out;
        }
        fmt.Println();
continue_out:;
        iΔ1ᴛ1 = iΔ1;
    }
break_out:;
    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();
    var nums = new nint[]{1, 2, 3, 4}.slice();
scan:
    foreach (var (_, n) in nums) {
        foreach (var (_, m) in nums) {
            if (n == m) {
                goto continue_scan;
            }
            if (n + m > 5) {
                goto break_scan;
            }
            fmt.Println("pair", n, m);
        }
continue_scan:;
    }
break_scan:;
    fmt.Println();
    nint x = 99;
    fmt.Println("i before thread and", i, "x before thread", x);
    goǃ((ᴛ1, ᴛ2, ᴛ3, ᴛ4) => fmt.Println(ᴛ1, ᴛ2, ᴛ3, ᴛ4), "i from thread and", i, "x from thread", x);
    while (ᐧ) {
        i++;
        x++;
        f(Ꮡi);
        if (i > 12) {
            break;
        }
    }
    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println("x = ", x);
    time.Sleep(1);
    fmt.Println("i after thread and", i, "x after thread", x);
}

internal static void f(ж<nint> Ꮡy) {
    ref var y = ref Ꮡy.Value;

    fmt.Print(y);
}

} // end main_package
