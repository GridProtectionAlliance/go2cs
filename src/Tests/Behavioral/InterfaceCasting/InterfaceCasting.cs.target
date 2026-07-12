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
    addTo(Ꮡc.of(Counter.Ꮡn), 1);
    return "inc"u8;
}

[GoRecv] public static nint Total(this ref Counter c) {
    return c.n;
}

internal static (ж<Counter>, error) makeCounter() {
    return (Ꮡ(new Counter(n: 5)), default!);
}

[GoType] partial interface Incrementer {
    @string Inc();
    nint Total();
}

[GoType] partial struct reversed {
    public Animal Animal;
}

public static Animal Reversed(Animal a) {
    return new reversedжAnimal(Ꮡ(new reversed(a)));
}

internal static (Animal, @string) pick(nint kind) {
    Animal a = default!;
    @string name = default!;
    switch (kind) {
    case 0: {
        ref var d = ref heap(new Dog(), out var Ꮡd);
        a = new DogжAnimal(Ꮡd);
        name = "dog"u8;
        break;
    }
    case 1: {
        ref var c = ref heap(new Cat(), out var Ꮡc);
        a = new CatжAnimal(Ꮡc);
        name = "cat"u8;
        break;
    }
    default: {
        ref var l = ref heap(new Llama(), out var Ꮡl);
        a = new LlamaжAnimal(Ꮡl);
        name = "llama"u8;
        break;
    }}

    return (a, name);
}

public static Animal Self(this ж<Cat> Ꮡc) {
    return new CatжAnimal(Ꮡc);
}

internal static void Main() {
    error err = default!;
    err = new MyError("bar");
    fmt.Printf("%v %v\n"u8, f(), err);
    var animals = new Animal[]{new DogжAnimal(@new<Dog>()), new CatжAnimal(@new<Cat>()), new Llama(nil), new JavaProgrammer(nil)}.slice();
    foreach (var (_, animal) in animals) {
        fmt.Println(animal.Speak());
    }
    var c = Ꮡ(new Counter(nil));
    Incrementer inc = new CounterжIncrementer(c);
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
    pack = append(pack, (Animal)(new CatжAnimal(Ꮡ(new Cat(nil)))));
    pack = append(pack, (Animal)(new Dog(nil)));
    fmt.Println(len(pack), pack[0].Speak(), pack[1].Speak());
    var cp = Ꮡ(new Cat(nil));
    fmt.Println(cp.Self().Speak());
    ref var swapped = ref heap<Animal>(out var Ꮡswapped);

    swapped = new Dog(nil);
    replaceAnimal(Ꮡswapped);
    fmt.Println("replaced:", swapped.Speak());
    Incrementer inc2 = default!;
    var (ᴛ1, ᴛ2) = makeCounter();
    (inc2, err) = (new CounterжIncrementer(ᴛ1), ᴛ2);
    inc2.Inc();
    fmt.Println("deconstructed into iface:", inc2.Total(), err == default!);
    var makeAnimal = Animal (bool feline) => {
        if (feline) {
            return new CatжAnimal(Ꮡ(new Cat(nil)));
        }
        return new Dog(nil);
    };
    fmt.Println("made:", makeAnimal(true).Speak(), makeAnimal(false).Speak());
    fmt.Println("plumbed:", runPlumbing());
    speakShutter ss = new wrapSinkжspeakShutter(Ꮡ(new wrapSink(Animal: new Dog(nil))));
    fmt.Println(ss.Speak(), ss.Shut());
    labeler lb = new badgeжlabeler(Ꮡ(new badge(text: "id"u8, num: 9)));
    fmt.Println("keyword-method:", lb.@string(), lb.@int());
    var av = describe(true);
    var (@as, aok) = av._<@string>(ᐧ);
    fmt.Println("any-string:", av, describe(false), @as, aok);
}

internal static any describe(bool b) {
    if (b) {
        return (@string)"text-value";
    }
    return 99;
}

[GoType] partial interface labeler {
    @string @string();
    nint @int();
}

[GoType] partial struct badge {
    internal @string text;
    internal nint num;
}

[GoRecv] internal static @string @string(this ref badge b) {
    return b.text;
}

[GoRecv] internal static nint @int(this ref badge b) {
    return b.num;
}

internal static void replaceAnimal(ж<Animal> Ꮡa) {
    ref var a = ref Ꮡa.Value;

    a = new CatжAnimal(Ꮡ(new Cat(nil)));
}

[GoType] partial interface speakShutter {
    @string Speak();
    @string Shut();
}

[GoType] partial struct wrapSink {
    public Animal Animal;
}

internal static @string Shut(this wrapSink w) {
    return "shut"u8;
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

[GoType] partial interface sink {
    @string drain();
}

[GoType] partial struct basin {
    internal @string tag;
}

internal static @string drain(this basin b) {
    return "b:"u8 + b.tag;
}

[GoType] partial struct plumbing {
    internal sink s;
    internal @string name;
}

[GoType("[]sink")] partial struct sinks;

internal static @string runPlumbing() {
    var p = new plumbing(name: "n1"u8);
    var batch = new sinks(new sink[]{new basin(tag: "x"u8), new basin(tag: "y"u8)}.slice());
    slice<sink> all = default!;
    all = append(all, batch.ꓸꓸꓸ);
    return p.name + ":"u8 + all[0].drain() + ","u8 + all[1].drain();
}

} // end main_package
