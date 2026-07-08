namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Node {
    internal nint val;
}

[GoRecv] public static nint Value(this ref Node n) {
    return n.val;
}

internal static slice<slice<ж<Node>>> group(this ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.Value;

    return new slice<ж<Node>>[]{new ж<Node>[]{Ꮡn}.slice()}.slice();
}

internal static void Main() {
    var n = Ꮡ(new Node(val: 42));
    var groups = n.group();
    fmt.Println(len(groups), len(groups[0]), groups[0][0].Value());
}

} // end main_package
