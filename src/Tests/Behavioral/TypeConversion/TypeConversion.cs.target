namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct main_Person_Address {
    public @string Street;
    public @string City;
}

[GoType] partial struct main_Person {
    public @string Name;
    public ж<main_Person_Address> Address;
}

[GoType] partial struct main_data_Address {
    [GoTag(@"json:""street""")]
    public @string Street;
    [GoTag(@"json:""city""")]
    public @string City;
}

[GoType] partial struct main_data {
    [GoTag(@"json:""name""")]
    public @string Name;
    [GoTag(@"json:""address""")]
    public ж<main_data_Address> Address;
}

internal static void Main() {
    ж<main_data> data = default!;
    ref var mine = ref heap(new main_Person(), out var Ꮡmine);
    ж<main_Person> person = ((ж<main_Person>)(data?.val ?? default!));
    person = Ꮡmine;
    fmt.Println(mine == person.val);
    fmt.Println(slice<rune>(((@string)"白鵬翔"u8)));
}

} // end main_package
