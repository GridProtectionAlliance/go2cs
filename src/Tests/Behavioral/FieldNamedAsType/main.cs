namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Node {
    public slice<Node> Nodes;
    public ж<Node> ΔNode;
    internal nint value;
}

internal static void Main() {
    var root = new Node(value: 1);
    ref var a = ref heap<Node>(out var Ꮡa);
    a = new Node(value: 2);
    ref var b = ref heap<Node>(out var Ꮡb);
    b = new Node(value: 3);
    root.ΔNode = Ꮡa;
    root.Nodes = new Node[]{a, b}.slice();
    a.ΔNode = Ꮡb;
    fmt.Println(root.value);
    fmt.Println((~root.ΔNode).value);
    fmt.Println((~(~root.ΔNode).ΔNode).value);
    fmt.Println(len(root.Nodes));
    fmt.Println(root.Nodes[1].value);
}

} // end main_package
