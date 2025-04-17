namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("dyn")] partial struct DataProcessor_data {
    public nint ID;
    public @string Name;
    public bool Valid;
}

[GoType] partial interface DataProcessor {
    void Process(DataProcessor_data data);
}

[GoType] partial struct Processor {
}

[GoType("dyn")] partial struct Process_data {
    public nint ID;
    public @string Name;
    public bool Valid;
}

public static void Process(this Processor p, Process_data data) {
    fmt.Printf("Processing ID: %d, Name: %s, Valid: %t\n"u8, data.ID, data.Name, data.Valid);
}

[GoType("dyn")] partial struct main_data {
    public nint ID;
    public @string Name;
    public bool Valid;
}

internal static void Main() {
    DataProcessor p = default!;
    p = new Processor(nil);
    var data = new main_data(ID: 1, Name: "Alice"u8, Valid: true);
    p.Process(data);
}

} // end main_package
