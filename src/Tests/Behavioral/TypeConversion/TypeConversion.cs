using go;

namespace go;

using fmt = fmt_package;

partial class main_package {

private static void Main() {
    [GoType] partial struct Person {
        public @string Name;
        public ж<struct{Street string; City string}> Address;
    }

    ж<struct{Name string "json:\"name\""; Address *struct{Street string "json:\"street\""; City string "json:\"city\""} "json:\"address\""}> data = default!;
    ref var mine = ref heap(new Person(), out var Ꮡmine);
    ж<Person> person = (Person.val)(data);
    person = Ꮡmine;
    fmt.Println(mine == person.val);
    fmt.Println(slice<rune>(((@string)"白鵬翔"u8)));
}

} // end main_package
