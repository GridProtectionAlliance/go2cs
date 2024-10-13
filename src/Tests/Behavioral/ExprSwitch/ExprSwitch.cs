// _Switch statements_ express conditionals across many// branches.
namespace go;

using fmt = fmt_package;
using time = time_package;

public static partial class main_package {

private static nint x = 1;

private static int32 getNext() {
    x++;
    return int32(x);
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

private static void Main() {
    fmt.Println(getStr("test"u8));
    fmt.Println(getStr2("hello, ", "world"u8));
    fmt.Println(getStr3("hello, %s"u8, "world"));
    // Here's a basic `switch`.
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
        nint xɅ1 = 6;
        fmt.Println(xɅ1);
    }

    fmt.Println(x);
    // You can use commas to separate multiple expressions
    // in the same `case` statement. We use the optional
    // `default` case in this example as well.
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

    // Case Mon comment
    // `switch` without an expression is an alternate way
    // to express if/else logic. Here we also show how the
    // `case` expressions can be non-constants.
    var t = time.Now();
    {
        switch (ᐧ) {
        case ᐧ when t.Hour() is < 12:
            fmt.Println("It's before noon");
            break;
        default:
            fmt.Println("It's after noon");
            break;
        }
    }

    // Before noon
    // After noon
    // Here is a more complex switch
    nint hour = 1;
    nint hour1 = time.Now().Hour();
    {
        nint hourɅ1 = time.Now().Hour();
        switch (ᐧ) {
        case ᐧ when hourɅ1 is 1 or < 12 or 2:
            fmt.Println("Good morning!");
            break;
        case ᐧ when (hourɅ1 == 1) || (hourɅ1 < 12) || (hourɅ1 == 2 || hour1 == 4):
            fmt.Println("Good morning (opt 2)!");
            break;
        case ᐧ when hourɅ1 is < 17:
            fmt.Println("Good afternoon!");
            break;
        case ᐧ when hourɅ1 is 0:
            fmt.Println("Midnight!");
            break;
        case ᐧ when hourɅ1 == 0 && hour1 == 1:
            fmt.Println("Midnight (opt 2)!");
            break;
        default:
            fmt.Println("Good evening!");
            break;
        }
    }

    // missing expression means "true"
    fmt.Println(hour);
    var c = '\r';
    switch (c) {
    case ' ' or '\t' or '\n' or '\f' or '\r':
        fmt.Println("whitespace");
        break;
    }

    // "i" before should be saved
    fmt.Printf("i before = %d\n"u8, i);
    // Here is a switch with simple statement and a redeclared identifier plus a fallthrough
    {
        nint iɅ1 = 1;
        var exprꞥ1 = getNext();
        var matchꞥ1 = false;
        if (exprꞥ1 is -1) { matchꞥ1 = true;
            fmt.Println("negative");
        }
        if (exprꞥ1 is 0) { matchꞥ1 = true;
            fmt.Println("zero");
        }
        if (exprꞥ1 is 1 or 2) { matchꞥ1 = true;
            fmt.Println("one or two");
            fallthrough = true;
        }
        if (fallthrough || exprꞥ1 is 3) { matchꞥ1 = true;
            fmt.Printf("three, but x=%d and i now = %d\n"u8, x, iɅ1);
            fallthrough = true;
        }
        if (fallthrough || !matchꞥ1) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    // "i" after should be restored
    fmt.Printf("i after = %d\n"u8, i);
    {
        var next = getNext();
        var matchꞥ2 = false;
        if (next is <= -1) { matchꞥ2 = true;
            fmt.Println("negative");
            {
                var exprꞥ3 = getNext();
                var matchꞥ3 = false;
                if (exprꞥ3 is 1 or 2) { matchꞥ3 = true;
                    fmt.Println("sub0 one or two");
                }
                if (exprꞥ3 is 3) { matchꞥ3 = true;
                    fmt.Println("sub0 three");
                    fallthrough = true;
                }
                if (fallthrough || !matchꞥ3) { /* default: */
                    fmt.Println("sub0 default");
                }
            }

        }
        if (next is 0) { matchꞥ2 = true;
            fmt.Println("zero");
            {
                var nextɅ1 = getNext();
                var matchꞥ4 = false;
                if (nextɅ1 is 1 or <= 2) { matchꞥ4 = true;
                    fmt.Println("sub1 one or two");
                }
                if (nextɅ1 is 3) { matchꞥ4 = true;
                    fmt.Println("sub1 three");
                    fallthrough = true;
                }
                if (fallthrough || !matchꞥ4) { /* default: */
                    fmt.Println("sub1 default");
                }
            }

        }
        if (next is 1 or 2) { matchꞥ2 = true;
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
        if (fallthrough || next >= 3 && next < 100) { matchꞥ2 = true;
            fmt.Printf("three or greater < 100: %d\n"u8, x);
            fallthrough = true;
        }
        if (fallthrough || !matchꞥ2) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

}

} // end main_package
