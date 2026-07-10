namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("map[@string, slice<@string>]")] partial struct Header;

[GoType] partial struct Request {
    public Header Header;
    public map<@string, nint> Count;
    public slice<@string> Body;
}

internal static void Main() {
    var req = Ꮡ(new Request(Header: new Header(new map<@string, slice<@string>>{}), Count: new map<@string, nint>{}, Body: new @string[]{"a", "b"}.slice()));
    req.Value.Header["Accept"u8] = new @string[]{"text/html", "text/plain"}.slice();
    req.Value.Header["X-Tag"u8] = new @string[]{"a"}.slice();
    req.Value.Count["hits"u8] = (~req).Count["hits"u8] + 1;
    req.Value.Count["hits"u8] += 2;
    req.Value.Count["hits"u8]++;
    req.Value.Body[1] = "z"u8;
    fmt.Println((~req).Header["Accept"u8]);
    fmt.Println((~req).Header["X-Tag"u8]);
    fmt.Println((~req).Count["hits"u8]);
    fmt.Println((~req).Body[0], (~req).Body[1]);
}

} // end main_package
