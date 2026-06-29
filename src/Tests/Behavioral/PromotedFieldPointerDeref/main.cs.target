namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct header {
    internal ж<node> next;
    internal nint tag;
}

[GoType] partial struct node {
    internal partial ref header header { get; }
    internal nint val;
}

[GoType] partial struct list {
    internal ж<node> head;
}

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    a.val.tag = 7;
    var b = Ꮡ(new node(val: 2));
    b.val.tag = 9;
    a.val.next = b;
    list l = default!;
    l.head = a;
    var x = l.head;
    while (x != nil) {
        fmt.Println((~x).val, (~x).tag);
        x = x.val.next;
    }
    var y = l.head;
    y.val.tag = 99;
    fmt.Println((~a).tag);
}

} // end main_package
