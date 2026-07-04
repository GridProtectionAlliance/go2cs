namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Animal {
    @string Type();
    @string Swim();
}

[GoType] partial interface Test {
}

[GoType] partial struct Dog {
    public @string Name;
    public @string Breed;
}

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

internal static void Main() {
    var f = @new<Frog>();
    var d = @new<Dog>();
    ref var zoo = ref heap<array<Animal>>(out var Ꮡzoo);
    zoo = new Animal[]{new FrogжAnimal(f), new DogжAnimal(d)}.array();
    Test t = default!;
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], f));
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], zoo[0]));
    fmt.Printf("Iface cmp result = %v\n"u8, !AreEqual(zoo[0], t));
    checkErr(1);
    checkErr(0);
    useAndRelease();
    Animal a = default!;
    fmt.Printf("%T\n"u8, a);
    foreach (var (_, aΔ1) in zoo) {
        fmt.Println(aΔ1.Type(), "can", aΔ1.Swim());
    }
    fmt.Printf("%T\n"u8, a);
    ShowZoo(Ꮡzoo);
    fmt.Printf("%T\n"u8, a);
    var vowels = new array<bool>(128){[(rune)'a'] = true, [(rune)'e'] = true, [(rune)'i'] = true, [(rune)'o'] = true, [(rune)'u'] = true, [(rune)'y'] = true};
    fmt.Println(vowels);
}

[GoType("num:uintptr")] partial struct errno;

internal static @string Error(this errno e) {
    return "errno"u8;
}

internal static readonly errno errAgain = 11;

internal static error mayFail(nint n) {
    if (n > 0) {
        return errAgain;
    }
    return default!;
}

internal static void checkErr(nint n) {
    var err = mayFail(n);
    if (AreEqual(err, errAgain)) {
        fmt.Println("got again");
    }
    if (!AreEqual(err, errAgain)) {
        fmt.Println("not again");
    }
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, errAgain)) {
        fmt.Println("switch: again");
    }
    else if (AreEqual(exprᴛ1, default!)) {
        fmt.Println("switch: nil");
    }
    else { /* default: */
        fmt.Println("switch: other");
    }

}

internal static error release(errno e) {
    fmt.Println("released", (uintptr)e);
    return errAgain;
}

internal static void useAndRelease() => func((defer, recover) => {
    deferǃ(release, errAgain, defer);
    fmt.Println("using");
});

public static void ShowZoo(ж<array<Animal>> Ꮡzoo) {
    ref var zoo = ref Ꮡzoo.Value;

    Animal a = default!;
    foreach (var (_, vᴛ1) in zoo) {
        a = vᴛ1;

        fmt.Println(a.Type(), "can", a.Swim());
    }
}

[GoRecv] public static @string Type(this ref Frog f) {
    return "Frog"u8;
}

[GoRecv] public static @string Swim(this ref Frog f) {
    return "Kick"u8;
}

[GoRecv] public static @string Swim(this ref Dog d) {
    return "Paddle"u8;
}

[GoRecv] public static @string Type(this ref Dog d) {
    return "Doggie"u8;
}

} // end main_package
