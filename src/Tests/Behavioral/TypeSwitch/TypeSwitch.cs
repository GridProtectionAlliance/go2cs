namespace go;

using System;

public static partial class main_package {

private static void Main() { 
    // A type `switch` compares types instead of values.  You
    // can use this to discover the type of an interface
    // value.  In this example, the variable `t` will have the
    // type corresponding to its clause.
    Action<object> whatAmI = i => {
        switch (i.type()) {
            case bool t:
                fmt.Println("I'm a bool"u8);
                break;
            case nint t:
                fmt.Printf("I'm an int, specifically type %T\n"u8, t);
                break;
            case int32 t: /* Matches int literals */
                fmt.Printf("I'm an int, specifically type %T\n"u8, t);
                break;
            case int64 t:
                fmt.Printf("I'm an int, specifically type %T\n"u8, t);
                break;
            case uint64 t:
                fmt.Printf("I'm an int, specifically type %T\n"u8, t);
                break;
            default:
            {
                var t = i.type();
                fmt.Printf("Don't know type %T\n"u8, t);
                break;
            }
        }
    };
    whatAmI(true);
    whatAmI(1);
    whatAmI(int64(2));
    whatAmI(uint64(2));
    whatAmI("hey"u8);
}

} // end main_package
