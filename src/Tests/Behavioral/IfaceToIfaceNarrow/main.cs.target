namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Reader {
    @string Read();
}

[GoType] partial interface Writer {
    void Write(@string s);
}

[GoType] partial interface ReadWriteCloser :
    Reader,
    Writer
{
    @string Close();
}

[GoType] partial interface Conn {
    @string Read();
    void Write(@string s);
    @string Close();
}

[GoType] partial struct conn {
    internal @string data;
}

[GoRecv] internal static @string Read(this ref conn c) {
    return c.data;
}

[GoRecv] internal static void Write(this ref conn c, @string s) {
    c.data = s;
}

[GoRecv] internal static @string Close(this ref conn c) {
    return "closed:"u8 + c.data;
}

internal static @string readFrom(Reader r) {
    return r.Read();
}

internal static void writeTo(Writer w, @string s) {
    w.Write(s);
}

internal static Writer asWriter(Conn c) {
    return new ConnᴠWriter(c);
}

internal static void Main() {
    Conn c = new connжConn(Ꮡ(new conn(data: "init"u8)));
    ReadWriteCloser rwc = new ConnᴠReadWriteCloser(c);
    writeTo(new ConnᴠWriter(c), "hello"u8);
    fmt.Println(readFrom(new ConnᴠReader(c)));
    var w = asWriter(c);
    w.Write("via-writer"u8);
    fmt.Println(c.Read());
    fmt.Println(rwc.Close());
}

} // end main_package
