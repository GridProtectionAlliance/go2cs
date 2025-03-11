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
    zoo = new Animal[]{~f, ~d}.array();
    Test t = default!;
    fmt.Printf("Iface cmp result = %v\n"u8, zoo[0] == ~f);
    fmt.Printf("Iface cmp result = %v\n"u8, AreEqual(zoo[0], zoo[0]));
    fmt.Printf("Iface cmp result = %v\n"u8, !AreEqual(zoo[0], t));
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

public static void ShowZoo(ж<array<Animal>> Ꮡzoo) {
    ref var zoo = ref Ꮡzoo.val;

    Animal a = default!;
    foreach (var (_, vᴛ1) in zoo) {
        a = vᴛ1;

        fmt.Println(a.Type(), "can", a.Swim());
    }
}

[GoRecv] internal static @string Type(this ref Frog f) {
    return "Frog"u8;
}

[GoRecv] internal static @string Swim(this ref Frog f) {
    return "Kick"u8;
}

[GoRecv] internal static @string Swim(this ref Dog d) {
    return "Paddle"u8;
}

[GoRecv] internal static @string Type(this ref Dog d) {
    return "Doggie"u8;
}

} // end main_package
