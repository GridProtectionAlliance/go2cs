namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    ref var i = ref heap(new nint(), out var Ꮡi);
    i = 0;
    while (i < 10) 
    {
        f(Ꮡi);
        i++;
    }

    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();
    for (i = 0; i < 10; i++) 
    {
        f(Ꮡi);
        for (nint j = 0; j < 3; j++) 
        {
            fmt.Println(i + j);
        }

        fmt.Println();
    }

    fmt.Println("i =", i);
    fmt.Println();
@out:
    ref var iɅ1 = ref heap(new nint(), out var ᏑiɅ1);
    for (iɅ1 = 0; iɅ1 < 5; iɅ1++) 
    {
        f(ᏑiɅ1);
        ref var iɅ2 = ref heap(new nint(), out var ᏑiɅ2);
        for (iɅ2 = 12; iɅ2 < 15; iɅ2++) 
        {
            f(ᏑiɅ2);
            goto break_out;
        }

        if (iɅ1 > 13) 
        {
            goto continue_out;
        }

        fmt.Println();

continue_out:;
    }
break_out:;
    fmt.Println();
    fmt.Println("i =", i);
    fmt.Println();
    while (true) 
    {
        i++;
        f(Ꮡi);
        if (i > 12) 
        {
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
