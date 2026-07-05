namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("map[@string, slice<@string>]")] partial struct Values;

public static @string Get(this Values v, @string key) {
    {
        var vs = v[key]; if (len(vs) > 0) {
            return vs[0];
        }
    }
    return ""u8;
}

public static void Add(this Values v, @string key, @string value) {
    v[key] = append(v[key], value);
}

internal static void Main() {
    var v = new Values(new map<@string, slice<@string>>{});
    v.Add("color"u8, "red"u8);
    v.Add("color"u8, "blue"u8);
    fmt.Println(v.Get("color"u8));
    fmt.Println(len(v["color"u8]));
    fmt.Println(len(v));
}

} // end main_package
