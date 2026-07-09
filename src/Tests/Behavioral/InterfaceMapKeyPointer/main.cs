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

internal static void Main() {
    var item = Ꮡ(new Item(pos: 7));
    var seen = new map<Node, @string>{};
    seen[new ItemжNode(item)] = "kept"u8;
    var (value, ok) = seen[new ItemжNode(item), ꟷ];
    fmt.Println(value, ok, item.Pos());
}

} // end main_package
