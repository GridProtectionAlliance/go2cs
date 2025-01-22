namespace go;

using fmt = fmt_package;

partial class main_package {

private static void Main() {
    var whatAmI = (object i) => {
        switch (i.type()) {
        case bool t:
            fmt.Println("I'm a bool");
            break;
        case nint t:
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        case int32 t:
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        case int64 t:
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        case uint64 t:
            fmt.Printf("I'm an int, specifically type %T\n"u8, t);
            break;
        default: {
            var t = i.type();
            fmt.Printf("Don't know type %T\n"u8, t);
            break;
        }}
    };
    whatAmI(true);
    whatAmI(1);
    whatAmI(((int64)2));
    whatAmI(((uint64)2));
    whatAmI("hey");
}

} // end main_package
