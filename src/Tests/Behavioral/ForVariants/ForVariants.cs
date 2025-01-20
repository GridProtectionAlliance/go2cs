namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

private static nint x = 1;
private static nint getNext() {
    x++;
    return x;
}

private static void Main() {
    nint i = 2;
    fmt.Print("Write ", i, " as ");
    switch (i) {
    case 1:
        fmt.Println("one");
        break;
    case 2:
        fmt.Println("two");
        break;
    case 3:
        {
            fmt.Println("three");
        }
        break;
    case 4:
        fmt.Println("four");
        break;
    default:
        fmt.Println("unknown");
        break;
    }

    switch (time.Now().Weekday()) {
    case time.Saturday or time.Sunday:
        fmt.Println("It's the weekend");
        break;
    case time.Monday:
        fmt.Println("Ugh, it's Monday");
        break;
    default:
        fmt.Println("It's a weekday");
        break;
    }

    var t = time.Now();
    switch (ᐧ) {
    case {} when t.Hour() is < 12:
        fmt.Println("It's before noon");
        break;
    default:
        fmt.Println("It's after noon");
        break;
    }

    fmt.Printf("i before = %d\n"u8, i);
    {
        nint iΔ1 = 1;
        var exprᴛ1 = getNext();
        var matchᴛ1 = false;
        if (exprᴛ1 is -1) { matchᴛ1 = true;
            fmt.Println("negative");
        }
        else if (exprᴛ1 is 0) { matchᴛ1 = true;
            fmt.Println("zero");
        }
        else if (exprᴛ1 is 1 or 2) { matchᴛ1 = true;
            fmt.Println("one or two");
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) { matchᴛ1 = true;
            fmt.Printf("three, but x=%d and i now = %d\n"u8, x, iΔ1);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    fmt.Printf("i after = %d\n"u8, i);
}

} // end main_package
