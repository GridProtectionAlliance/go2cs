namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var whatAmI = (any i) => {
        switch (i.type()) {
        case null: {
            fmt.Println("I'm nil");
            break;
        }
        case bool t: {
            fmt.Println("I'm a bool");
            break;
        }
        case nint t: {
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        }
        case int32 t: {
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        }
        case int64 t: {
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        }
        case uint64 t: {
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        }
        default: {
            var t = i.type();
            fmt.Printf("Don't know type %T\n"u8, t);
            break;
        }}
    };
    whatAmI(true);
    whatAmI(1);
    whatAmI((int64)2);
    whatAmI((uint64)2);
    whatAmI("hey");
    whatAmI(default!);
    var classify = (any i) => {
        switch (i.type()) {
        case nint : {
            fmt.Println("int");
            break;
        }
        case int32 : {
            fmt.Println("int32");
            break;
        }
        case nuint : {
            fmt.Println("uint");
            break;
        }
        case uint32 : {
            fmt.Println("uint32");
            break;
        }
        default: {
            fmt.Println("other");
            break;
        }}

    };
    nint a = 1;
    int32 b = 2;
    nuint c = 3;
    uint32 d = 4;
    classify(a);
    classify(b);
    classify(c);
    classify(d);
}

} // end main_package
