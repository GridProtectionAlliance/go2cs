namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Node {
    nint Pos();
}

[GoType] partial struct Item {
    internal nint pos;
}

[GoRecv] public static nint Pos(this ref Item i) {
    return i.pos;
}

internal static (@string, bool) lookup(map<Node, @string> seen, ж<Item> Ꮡit) {
    ref var it = ref Ꮡit.Value;

    var (value, ok) = seen[new ItemжNode(Ꮡit), ꟷ];
    return (value, ok);
}

internal static void record(map<Node, @string> seen, ж<Item> Ꮡit, @string label) {
    ref var it = ref Ꮡit.Value;

    seen[new ItemжNode(Ꮡit)] = label;
}

internal static @string plainRead(map<Node, @string> seen, ж<Item> Ꮡit) {
    ref var it = ref Ꮡit.Value;

    return seen[new ItemжNode(Ꮡit)];
}

internal static void Main() {
    var item = Ꮡ(new Item(pos: 7));
    var seen = new map<Node, @string>{};
    seen[new ItemжNode(item)] = "kept"u8;
    var (value, ok) = seen[new ItemжNode(item), ꟷ];
    fmt.Println(value, ok, item.Pos());
    (value, ok) = lookup(seen, item);
    fmt.Println(value, ok);
    var other = Ꮡ(new Item(pos: 9));
    record(seen, other, "added"u8);
    fmt.Println(plainRead(seen, other), len(seen));
}

} // end main_package
