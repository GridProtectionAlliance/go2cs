namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct MyError {
    internal @string description;
}

public static @string Error(this MyError err) {
    return fmt.Sprintf("error: %s"u8, err.description);
}

internal static error f() {
    return new MyError("foo");
}

[GoType] partial interface Animal {
    @string Speak();
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

[GoType] partial struct Counter {
    internal nint n;
}

internal static void addTo(ж<nint> Ꮡp, nint delta) {
    ref var p = ref Ꮡp.Value;

    p += delta;
}

public static @string Inc(this ж<Counter> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    addTo(Ꮡc.of(Counter.Ꮡn), 1);
    return "inc"u8;
}

[GoRecv] public static nint Total(this ref Counter c) {
    return c.n;
}

[GoType] partial interface Incrementer {
    @string Inc();
    nint Total();
}

internal static void Main() {
    error err = default!;
    err = new MyError("bar");
    fmt.Printf("%v %v\n"u8, f(), err);
    var animals = new Animal[]{new DogᴵAnimal(@new<Dog>()), new CatᴵAnimal(@new<Cat>()), new Llama(nil), new JavaProgrammer(nil)}.slice();
    foreach (var (_, animal) in animals) {
        fmt.Println(animal.Speak());
    }
    var c = Ꮡ(new Counter(nil));
    Incrementer inc = new CounterᴵIncrementer(c);
    inc.Inc();
    inc.Inc();
    fmt.Println("via pointer:", c.Total());
    c.Value.n = 10;
    fmt.Println("via interface:", inc.Total());
    var (back, ok) = inc._<ж<Counter>>(ᐧ);
    back.Inc();
    fmt.Println("assert-back:", ok, c.Total(), back == c);
}

} // end main_package
