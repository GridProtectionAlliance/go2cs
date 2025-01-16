// ReSharper disable InconsistentNaming
// ReSharper disable PreferConcreteValueOverDefault
namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct MyError {
    public @string description;
}

public static @string Error(this MyError err) {
    return fmt.Sprintf("error: %s"u8, err.description);
}

private static error f() {
    return error.As(new MyError("foo"));
}

[GoType] partial interface Animal {
    public @string Speak();
}

[GoType] partial struct Dog {
}

public static @string Speak(this Dog d) {
    return "Woof!"u8;
}

[GoType] partial struct Cat {
}

[GoRecv] public static @string Speak(this ref Cat c) {
    return "Meow!"u8;
}

[GoType] partial struct Llama {
}

public static @string Speak(this Llama l) {
    return "?????"u8;
}

[GoType] partial struct JavaProgrammer {
}

public static @string Speak(this JavaProgrammer j) {
    return "Design patterns!"u8;
}

private static void Main() {
    error err = default!;
    err = new MyError("bar");
    fmt.Printf("%v %v\n"u8, f(), err);
    var animals = new Animal[]{~@new<Dog>(), ~@new<Cat>(), new Llama(), new JavaProgrammer()}.slice();
    foreach (var (_, animal) in animals) {
        fmt.Println(animal.Speak());
    }
}

} // end main_package
