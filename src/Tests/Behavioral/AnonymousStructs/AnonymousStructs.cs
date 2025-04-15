namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}

[GoType("dyn")] partial struct main_anonPerson {
    public @string Name;
    public nint Age;
}

[GoType("dyn")] partial struct main_type {
    public @string Name;
    public nint Age;
}

[GoType("dyn")] partial struct main_typeᴛ1 {
    public @string Name;
    public nint Age;
}

internal static void Main() {
    // Create values of both named and anonymous struct types
    var namedPerson = new Person(Name: "Alice"u8, Age: 30);
    var anonPerson = new main_anonPerson(Name: "Bob"u8, Age: 25);
    // Test with anonymous struct type
    any someInterface = anonPerson;
    // Type assertion with the same anonymous struct type
    var (_, ok) = someInterface._<main_type>(ᐧ);
    fmt.Println("Anonymous struct type assertion:", ok);
    // Should be true
    // Now test with named struct
    someInterface = namedPerson;
    (_, ok) = someInterface._<main_typeᴛ1>(ᐧ);
    fmt.Println("Named struct with identical fields:", ok);
}

// Will be false as your results showed

} // end main_package
