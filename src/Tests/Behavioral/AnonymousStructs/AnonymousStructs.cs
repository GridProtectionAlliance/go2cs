namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}

[GoType("dyn")] partial struct processAnonymousStruct_data {
    public @string Name;
    public nint Age;
}

// Function that takes an anonymous struct as a parameter
internal static void processAnonymousStruct(processAnonymousStruct_data data) {
    fmt.Printf("Processing: %s, %d years old\n"u8, data.Name, data.Age);
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

[GoType("dyn")] partial struct main_data {
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
    // Will be false
    // Using the anonymous struct as a function parameter
    fmt.Println("\n=== Function Parameter Tests ===");
    // Pass anonymous struct literal directly
    processAnonymousStruct(new main_data(Name: "Charlie"u8, Age: 40));
    // Pass our existing anonymous struct
    processAnonymousStruct(anonPerson);
    // Try to pass our named struct
    // This works because the function parameter is a structural specification
    // not a nominal type requirement!
    processAnonymousStruct(new processAnonymousStruct_data(namedPerson.Name, namedPerson.Age));
}

} // end main_package
