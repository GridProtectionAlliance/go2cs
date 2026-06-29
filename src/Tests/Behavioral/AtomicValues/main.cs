namespace go;

using fmt = fmt_package;
using atomic = sync.atomic_package;
using sync;

partial class main_package {

[GoType] partial struct holder {
    internal sync.atomic_package.Int64 count;
}

internal static ж<holder> ᏑgHolder = new(default(holder));
internal static ref holder gHolder => ref ᏑgHolder.val;

internal static void Main() {
    ref var n = ref heap(new sync.atomic_package.Int32(), out var Ꮡn);
    Ꮡn.Store(10);
    fmt.Println("add:", Ꮡn.Add(5));
    fmt.Println("load:", Ꮡn.Load());
    fmt.Println("swap:", Ꮡn.Swap(100));
    fmt.Println("cas ok:", Ꮡn.CompareAndSwap(100, 7));
    fmt.Println("cas no:", Ꮡn.CompareAndSwap(100, 8));
    fmt.Println("final:", Ꮡn.Load());
    ref var p = ref heap(new sync.atomic_package.Pointer<nint>(), out var Ꮡp);
    fmt.Println("ptr nil:", Ꮡp.Load() == nil);
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    Ꮡp.Store(Ꮡa);
    fmt.Println("ptr load:", Ꮡp.Load().val);
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    fmt.Println("ptr cas no:", Ꮡp.CompareAndSwap(Ꮡb, Ꮡb));
    fmt.Println("ptr cas ok:", Ꮡp.CompareAndSwap(Ꮡa, Ꮡb));
    fmt.Println("ptr final:", Ꮡp.Load().val);
    var old = Ꮡp.Swap(Ꮡa);
    fmt.Println("ptr swap:", old.val, Ꮡp.Load().val);
    ᏑgHolder.of(holder.Ꮡcount).Store(42);
    ᏑgHolder.of(holder.Ꮡcount).Add(8);
    fmt.Println("global field:", ᏑgHolder.of(holder.Ꮡcount).Load());
}

} // end main_package
