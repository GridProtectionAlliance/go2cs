namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal any inner;
}

[GoType] partial struct pair {
    internal @string label;
    internal any value;
}

internal static void describe(@string prefix, any v) {
    switch (v.type()) {
    case @string s: {
        fmt.Println(prefix, "string:", s);
        break;
    }
    default: {
        var s = v;
        fmt.Println(prefix, "other:", s);
        break;
    }}
}

internal static void Main() {
    var n = Ꮡ(new node(inner: (@string)"hi"));
    var p = new pair("tag", (@string)"val");
    var m = new map<@string, any>{["k"u8] = (@string)"mv"};
    var mk = new map<any, nint>{[(@string)"ky"] = 7};
    var s = new any[]{(@string)"a", (@string)"b"}.slice();
    var arr = new any[]{(@string)"x", (@string)"y"}.array();
    var el = new node[]{new(inner: (@string)"e1")}.slice();
    var ep = new pair[]{new("e2"u8, (@string)"e3")}.slice();
    var pp = new ж<node>[]{Ꮡ(new node(inner: (@string)"p1"))}.slice();
    var pq = new ж<pair>[]{Ꮡ(new pair("q1"u8, (@string)"q2"))}.slice();
    var sp = new array<any>(3){[1] = (@string)"sp"};
    describe("keyed"u8, (~n).inner);
    fmt.Println("label:", p.label);
    describe("positional"u8, p.value);
    describe("mapval"u8, m["k"u8]);
    foreach (var (k, v) in mk) {
        describe("mapkey"u8, k);
        fmt.Println("mapkeyval:", v);
    }
    describe("slice0"u8, s[0]);
    describe("slice1"u8, s[1]);
    describe("array0"u8, arr[0]);
    describe("array1"u8, arr[1]);
    describe("elided"u8, el[0].inner);
    fmt.Println("elidedpos label:", ep[0].label);
    describe("elidedpos"u8, ep[0].value);
    describe("ptrelided"u8, (~pp[0]).inner);
    fmt.Println("ptrelidedpos label:", (~pq[0]).label);
    describe("ptrelidedpos"u8, (~pq[0]).value);
    describe("sparse0"u8, sp[0]);
    describe("sparse1"u8, sp[1]);
}

} // end main_package
