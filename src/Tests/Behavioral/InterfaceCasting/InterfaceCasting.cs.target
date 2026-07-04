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

[GoType] partial struct reversed {
    public Animal Animal;
}

public static Animal Reversed(Animal a) {
    return new reversedᴵAnimal(Ꮡ(new reversed(a)));
}

internal static (Animal, @string) pick(nint kind) {
    Animal a = default!;
    @string name = default!;
    switch (kind) {
    case 0: {
        ref var d = ref heap(new Dog(), out var Ꮡd);
        a = new DogᴵAnimal(Ꮡd);
        name = "dog"u8;
        break;
    }
    case 1: {
        ref var c = ref heap(new Cat(), out var Ꮡc);
        a = new CatᴵAnimal(Ꮡc);
        name = "cat"u8;
        break;
    }
    default: {
        ref var l = ref heap(new Llama(), out var Ꮡl);
        a = new LlamaᴵAnimal(Ꮡl);
        name = "llama"u8;
        break;
    }}

    return (a, name);
}

public static Animal Self(this ж<Cat> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    return new CatᴵAnimal(Ꮡc);
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
    var r = Reversed(new Dog(nil));
    fmt.Println("promoted via pointer adapter:", r.Speak());
    for (nint k = 0; k < 3; k++) {
        var (a, name) = pick(k);
        fmt.Println("picked:", name, a.Speak());
    }
    rdr rd = new strRdr(nil);
    fmt.Println(rd.read());
    var rc = open("data"u8);
    fmt.Println(rc.read(), rc.close());
    var readers = new rdr[]{new strRdr(nil)}.slice();
    readers[0] = new fileRdr(name: "x"u8);
    fmt.Println(readers[0].read());
    slice<Animal> pack = default!;
    pack = append(pack, (Animal)(new CatᴵAnimal(Ꮡ(new Cat(nil)))));
    pack = append(pack, (Animal)(new Dog(nil)));
    fmt.Println(len(pack), pack[0].Speak(), pack[1].Speak());
    var cp = Ꮡ(new Cat(nil));
    fmt.Println(cp.Self().Speak());
}

[GoType] partial interface rdr {
    @string read();
}

[GoType] partial interface clsr {
    @string close();
}

[GoType] partial interface rdCloser :
    rdr,
    clsr
{
}

[GoType] partial struct strRdr {
}

internal static @string read(this strRdr _) {
    return "strRdr"u8;
}

[GoType] partial struct fileRdr {
    internal @string name;
}

internal static @string read(this fileRdr f) {
    return "read:"u8 + f.name;
}

internal static @string close(this fileRdr f) {
    return "close:"u8 + f.name;
}

internal static rdCloser open(@string name) {
    return new fileRdr(name);
}

} // end main_package
