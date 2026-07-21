namespace go.MethodExprDotImport;

partial class mep_package {

[GoType] partial struct Reader {
    public @string Name;
}

[GoRecv] public static (@string, error) Read(this ref Reader r, byte delim) {
    return (r.Name, default!);
}

[GoRecv] public static (@string, error) Peek(this ref Reader r, byte delim) {
    return ("peek:" + r.Name, default!);
}

} // end mep_package
