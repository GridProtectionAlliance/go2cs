namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Animal {
    @string Type();
    @string Swim();
}

[GoType] partial struct Dog {
    public @string Name;
    public @string Breed;
}

[GoType] partial struct Frog {
    public @string Name;
    public @string Color;
}

private static void Main() {
    var f = @new<Frog>();
    var d = @new<Dog>();
    ref var zoo = ref heap<array<Animal>>(out var Ꮡzoo);
    zoo = new Animal[]{~f, ~d}.array();
    Animal a = default!;
    fmt.Printf("%T\n"u8, a);
    foreach (var (_, aΔ1) in zoo) {
        fmt.Println(a.Type(), "can", a.Swim());
    }
    fmt.Printf("%T\n"u8, a);
    ShowZoo(Ꮡzoo);
    fmt.Printf("%T\n"u8, a);
    var vowels = new bool[]{vowels['a'] = true, vowels['e'] = true, vowels['i'] = true, vowels['o'] = true, vowels['u'] = true, vowels['y'] = true}.array();
    fmt.Println(vowels);
}

public static void ShowZoo(ptr<array<Animal>> Ꮡzoo) {
    ref var zoo = ref Ꮡzoo.val;

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
