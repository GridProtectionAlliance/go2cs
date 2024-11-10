namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 0;
    while (i < 10) {
        f(Ꮡi);
        i++;
    }
    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();
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
    ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1);
    for (iΔ1 = 0; iΔ1 < 5; iΔ1++) {
        f(ᏑiΔ1);
        ref var iΔ2 = ref heap<nint>(out var ᏑiΔ2);
        for (iΔ2 = 12; iΔ2 < 15; iΔ2++) {
            f(ᏑiΔ2);
            goto break_out;
        }
        if (iΔ1 > 13) {
            goto continue_out;
        }
        fmt.Println();
continue_out:;
    }
break_out:;
    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();
    while (ᐧ) {
        i++;
        f(Ꮡi);
        if (i > 12) {
            break;
        }
    }
    fmt.Println();
    fmt.Println("i =", i);
}

private static void f(ptr<nint> Ꮡy) {
    ref var y = ref Ꮡy.val;

    fmt.Print(y);
}

} // end main_package
