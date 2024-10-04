// _Switch statements_ express conditionals across many// branches.
namespace go;

using fmt = fmt_package;
using time = time_package;

public static partial class main_package {

private static nint x = 1;

private static nint getNext() {
    x++;
    return x;
}

private static @string getStr(@string test) {
    return "string"u8 + test;
}

private static @string getStr2(object test1, @string test2) {
    return test1._<string>() + test2;
}

private static void Main() {

    fmt.Println(getStr("test"u8));
    fmt.Println(getStr2("hello, ", "world"u8));

    // Here's a basic `switch`.

    var i = 2;
    fmt.Print("Write ", i, " as ");
    switch (i) {
    case 1:
        fmt.Println("one");
        break;
    case 2:
        fmt.Println("two");
        break;
    case 3: {

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



    var xꞥ1 = 5;
    fmt.Println(xꞥ1);
 {


        var xꞥ2 = 6;
        fmt.Println(xꞥ2);
    }


    fmt.Println(xꞥ1);



    // You can use commas to separate multiple expressions
    // in the same `case` statement. We use the optional
    // `default` case in this example as well.
    switch (time.Now().Weekday()) {
    case time.Saturday:
    case time.Sunday:
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


    // Before noon
    // After noon
    // "i" before should be saved
    fmt.Printf("i before = %d\n"u8, i);

    // Here is a switch with simple statement and a redeclared identifier plus a fallthrough
    {


        var iꞥ1 = 1;
    }


    // "i" after should be restored
    fmt.Printf("i after = %d\n"u8, i);
}

} // end main_package
