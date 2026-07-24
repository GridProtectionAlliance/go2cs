namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ref var p1 = ref heap<ж<nint>>(out var Ꮡp1);
    p1 = ((ж<nint>)nil);
    ref var p2 = ref heap<ж<nint>>(out var Ꮡp2);
    p2 = ((ж<nint>)nil);
    var pp1 = Ꮡp1;
    var pp2 = Ꮡp2;
    fmt.Println("pp1==pp2", pp1 == pp2);
    fmt.Println("pp1==nil", pp1 == nil);
    fmt.Println("*pp1==nil", pp1.ValueSlot == nil);
    fmt.Println("*pp1==*pp2", pp1.ValueSlot == pp2.ValueSlot);
    var alias = pp1;
    fmt.Println("alias==pp1", alias == pp1);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 42;
    alias.ValueSlot = Ꮡn;
    fmt.Println("p1 set", pp1.ValueSlot != nil, (pp1.ValueSlot).Value);
}

} // end main_package
