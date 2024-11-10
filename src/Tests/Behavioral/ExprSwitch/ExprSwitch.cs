namespace go;

using fmt = fmt_package;
using time = time_package;

public static partial class main_package {

private static nint x = 1;
private static int32 getNext() {
    x++;
    return (int32)(x);
}

private static @string getStr(@string test) {
    return "string"u8 + test;
}

private static @string getStr2(object test1, @string test2) {
    return test1._<@string>() + test2;
}

private static @string getStr3(@string format, params object[] a) {
    return fmt.Sprintf(format, a);
}

public static nint Foo(nint n) {
    fmt.Println(n);
    return n;
}

private static void Main() {
    fmt.Println(getStr("test"u8));
    fmt.Println(getStr2("hello, ", "world"u8));
    fmt.Println(getStr3("hello, %s"u8, "world"));
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
    case 4 or 5 or 6:
        fmt.Println("four, five or siz");
        break;
    default:
        fmt.Println("unknown");
        break;
    }

    nint x = 5;
    fmt.Println(x);
    {
        nint xΔ1 = 6;
        fmt.Println(xΔ1);
    }
    fmt.Println(x);
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
    case ᐧ when t.Hour() is < 12:
        fmt.Println("It's before noon");
        break;
    default:
        fmt.Println("It's after noon");
        break;
    }

    nint hour = 1;
    nint hour1 = time.Now().Hour();
    {
        nint hourΔ1 = time.Now().Hour();
        switch (ᐧ) {
        case ᐧ when hourΔ1 is 1 or < 12 or 2:
            fmt.Println("Good morning!");
            break;
        case ᐧ when (hourΔ1 == 1) || (hourΔ1 < 12) || (hourΔ1 == 2 || hour1 == 4):
            fmt.Println("Good morning (opt 2)!");
            break;
        case ᐧ when hourΔ1 is < 17:
            fmt.Println("Good afternoon!");
            break;
        case ᐧ when hourΔ1 is 0:
            fmt.Println("Midnight!");
            break;
        case ᐧ when hourΔ1 == 0 && hour1 == 1:
            fmt.Println("Midnight (opt 2)!");
            break;
        default:
            fmt.Println("Good evening!");
            break;
        }
    }

    fmt.Println(hour);
    var c = '\r';
    switch (c) {
    case ' ' or '\t' or '\n' or '\f' or '\r':
        fmt.Println("whitespace");
        break;
    }

    fmt.Printf("i before = %d\n"u8, i);
    {
        nint iΔ1 = 1;
        var exprɅ1 = getNext();
        var matchɅ1 = false;
        if (exprɅ1 is -1) { matchɅ1 = true;
            fmt.Println("negative");
        }
        else if (exprɅ1 is 0) { matchɅ1 = true;
            fmt.Println("zero");
        }
        else if (exprɅ1 is 1 or 2) { matchɅ1 = true;
            fmt.Println("one or two");
            fallthrough = true;
        }
        if (fallthrough || !matchɅ1 && exprɅ1 is 3) { matchɅ1 = true;
            fmt.Printf("three, but x=%d and i now = %d\n"u8, x, iΔ1);
            fallthrough = true;
        }
        if (fallthrough || !matchɅ1) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    fmt.Printf("i after = %d\n"u8, i);
    {
        var next = getNext();
        var matchɅ2 = false;
        if (next is <= -1) { matchɅ2 = true;
            fmt.Println("negative");
            var exprɅ3 = getNext();
            var matchɅ3 = false;
            if (exprɅ3 is 1 or 2) { matchɅ3 = true;
                fmt.Println("sub0 one or two");
            }
            else if (exprɅ3 is 3) { matchɅ3 = true;
                fmt.Println("sub0 three");
                fallthrough = true;
            }
            if (fallthrough || !matchɅ3) { /* default: */
                fmt.Println("sub0 default");
            }

        }
        else if (next is 0) { matchɅ2 = true;
            fmt.Println("zero");
            {
                var nextΔ1 = getNext();
                var matchɅ4 = false;
                if (nextΔ1 is 1 or <= 2) { matchɅ4 = true;
                    fmt.Println("sub1 one or two");
                }
                else if (nextΔ1 is 3) { matchɅ4 = true;
                    fmt.Println("sub1 three");
                    fallthrough = true;
                }
                if (fallthrough || !matchɅ4) { /* default: */
                    fmt.Println("sub1 default");
                }
            }

        }
        else if (next is 1 or 2) { matchɅ2 = true;
            fmt.Println("one or two");
            switch (next) {
            case 1 or 2:
                fmt.Println("sub2 one or two");
                break;
            case 3:
                fmt.Println("sub2 three");
                break;
            default:
                fmt.Println("sub2 default");
                break;
            }

            fallthrough = true;
        }
        if (fallthrough || !matchɅ2 && (next >= 3 && next < 100)) { matchɅ2 = true;
            fmt.Printf("three or greater < 100: %d\n"u8, x);
            fallthrough = true;
        }
        if (fallthrough || !matchɅ2) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    var exprɅ5 = Foo(2);
    var matchɅ5 = false;
    if (exprɅ5 == Foo(1) || exprɅ5 == Foo(2) || exprɅ5 == Foo(3)) { matchɅ5 = true;
        fmt.Println("First case");
        fallthrough = true;
    }
    if (fallthrough || !matchɅ5 && exprɅ5 == Foo(4)) {
        fmt.Println("Second case");
    }
    else { /* default: */
        fmt.Println("Default case");
    }

}

} // end main_package
