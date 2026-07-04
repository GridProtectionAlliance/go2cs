namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}


[GoType("dyn")] partial struct settingsᴛ1 {
    public bool Verbose;
    public nint Retries;
}
internal static ж<settingsᴛ1> Ꮡsettings = new(new settingsᴛ1(Verbose: true, Retries: 3));
internal static ref settingsᴛ1 settings => ref Ꮡsettings.Value;

[GoType("dyn")] partial struct processAnonymousStruct_data {
    public @string Name;
    public nint Age;
}

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

[GoType("dyn")] partial struct main_typeᴛ2 {
    internal @string name;
    internal uint32 size;
}

internal static void Main() {
    var namedPerson = new Person(Name: "Alice"u8, Age: 30);
    var anonPerson = new main_anonPerson(Name: "Bob"u8, Age: 25);
    any someInterface = anonPerson;
    var (_, ok) = someInterface._<main_type>(ᐧ);
    fmt.Println("Anonymous struct type assertion:", ok);
    someInterface = namedPerson;
    (_, ok) = someInterface._<main_typeᴛ1>(ᐧ);
    fmt.Println("Named struct with identical fields:", ok);
    fmt.Println("\n=== Function Parameter Tests ===");
    processAnonymousStruct(new main_data(Name: "Charlie"u8, Age: 40));
    processAnonymousStruct(anonPerson);
    processAnonymousStruct(new processAnonymousStruct_data(namedPerson.Name, namedPerson.Age));
    fmt.Println("\n=== Package-Global Anonymous Struct ===");
    fmt.Printf("settings: Verbose=%t Retries=%d\n"u8, settings.Verbose, settings.Retries);
    var pRetries = Ꮡsettings.of(settingsᴛ1.ᏑRetries);
    pRetries.Value = 5;
    fmt.Printf("after &settings.Retries=5: *p=%d global=%d\n"u8, pRetries.Value, settings.Retries);
    fmt.Println("\n=== In-Function var Slice of Anonymous Struct ===");
    slice<main_typeᴛ2> sects = new main_typeᴛ2[]{
        new("text"u8, 100),
        new("data"u8, 200),
        new("syms"u8, 300)
    }.slice();
    var total = (uint32)0;
    foreach (var (_, sect) in sects) {
        total += sect.size;
    }
    fmt.Printf("sections=%d total=%d first=%s\n"u8, len(sects), total, sects[0].name);
}

} // end main_package
