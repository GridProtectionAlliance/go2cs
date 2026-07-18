namespace go;

using reflect = reflect_package;

partial class main_package {

[GoType] partial struct point {
    internal nint x, y;
    internal slice<@string> tags;
}

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static void Main() {
    println(reflect.DeepEqual(new @string[]{"a", "bc"}.slice(), new @string[]{"a", "bc"}.slice()));
    println(reflect.DeepEqual(new @string[]{"a"}.slice(), new @string[]{"b"}.slice()));
    println(reflect.DeepEqual(new nint[]{1, 2, 3}.slice(), new nint[]{1, 2, 3}.slice()));
    println(reflect.DeepEqual(new nint[]{1, 2, 3}.slice(), new nint[]{1, 2, 4}.slice()));
    println(reflect.DeepEqual(new slice<byte>[]{slice<byte>("ab"u8), default!}.slice(), new slice<byte>[]{slice<byte>("ab"u8), default!}.slice()));
    println(reflect.DeepEqual(new slice<byte>[]{slice<byte>("ab"u8)}.slice(), new slice<byte>[]{slice<byte>("ac"u8)}.slice()));
    println(reflect.DeepEqual(slice<byte>(default!), new byte[]{}.slice()));
    println(reflect.DeepEqual(new byte[]{}.slice(), new byte[]{}.slice()));
    slice<nint> nilInts = default!;
    println(reflect.DeepEqual(nilInts, nilInts));
    println(reflect.DeepEqual(nilInts, new nint[]{}.slice()));
    var zero = 0.0D;
    var nan = new float64[]{zero / zero}.slice();
    println(reflect.DeepEqual(nan, nan));
    println(reflect.DeepEqual(nan, new float64[]{nan[0]}.slice()));
    var p1 = new point(1, 2, new @string[]{"n"}.slice());
    var p2 = new point(1, 2, new @string[]{"n"}.slice());
    var p3 = new point(1, 3, new @string[]{"n"}.slice());
    println(reflect.DeepEqual(p1, p2));
    println(reflect.DeepEqual(p1, p3));
    var m1 = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    var m2 = new map<@string, nint>{["b"u8] = 2, ["a"u8] = 1};
    println(reflect.DeepEqual(m1, m2));
    println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1}));
    println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1, ["c"u8] = 2}));
    println(reflect.DeepEqual(m1, new map<@string, nint>{["a"u8] = 1, ["b"u8] = 3}));
    map<@string, nint> nilMap = default!;
    println(reflect.DeepEqual(nilMap, nilMap));
    println(reflect.DeepEqual(nilMap, new map<@string, nint>{}));
    println(reflect.DeepEqual(m1, m1));
    var q1 = Ꮡ(new point(1, 2, default!));
    var q2 = Ꮡ(new point(1, 2, default!));
    var q3 = q1;
    println(reflect.DeepEqual(q1, q2));
    println(reflect.DeepEqual(q1, q3));
    q2.Value.y = 9;
    println(reflect.DeepEqual(q1, q2));
    ж<point> np1 = default!;
    ж<point> np2 = default!;
    println(reflect.DeepEqual(np1, np2));
    println(reflect.DeepEqual(np1, q1));
    var a = Ꮡ(new node(val: 1));
    a.Value.next = a;
    var b = Ꮡ(new node(val: 1));
    b.Value.next = b;
    println(reflect.DeepEqual(a, b));
    var c = Ꮡ(new node(val: 2));
    c.Value.next = c;
    println(reflect.DeepEqual(a, c));
    println(reflect.DeepEqual(new nint[]{1}.slice(), new @string[]{"1"}.slice()));
    println(reflect.DeepEqual(default!, default!));
}

} // end main_package
