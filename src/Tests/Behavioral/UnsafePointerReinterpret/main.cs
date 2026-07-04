namespace go;

using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer indirectKey(@unsafe.Pointer k) {
    return ((ж<@unsafe.Pointer>)(uintptr)(k)).Value;
}

internal static void store(ж<@unsafe.Pointer> Ꮡdst, @unsafe.Pointer val) {
    ref var dst = ref Ꮡdst.Value;

    dst = val;
}

internal static void writeBarrier(@unsafe.Pointer ptr, @unsafe.Pointer val) {
    store((ж<@unsafe.Pointer>)(uintptr)(ptr), val);
}

internal static (Action, bool) funcAt(@unsafe.Pointer p) {
    return (((ж<Action>)(uintptr)(p)).ValueSlot, true);
}

internal static uint64 zeroPair(@unsafe.Pointer x) {
    ((ж<array<uint64>>)(uintptr)(x)).Value[0] = 0;
    ((ж<array<uint64>>)(uintptr)(x)).Value[1] = 0;
    return ((ж<array<uint64>>)(uintptr)(x)).Value[0];
}

[GoType("num:uintptr")] partial struct linkaddr;

internal static uintptr throughPointer(linkaddr v) {
    return (uintptr)((@unsafe.Pointer)(uintptr)v);
}

internal static void Main() {
    ref var backing = ref heap(new uintptr(), out var Ꮡbacking);
    @unsafe.Pointer p = @unsafe.Pointer.FromRef(ref (Ꮡbacking).Value);
    writeBarrier(p, p);
    _ = (uintptr)indirectKey(p);
    ref var fslot = ref heap<Action>(out var Ꮡfslot);
    var (_, ok) = funcAt(new @unsafe.Pointer(Ꮡfslot));
    _ = ok;
    ref var pair = ref heap(new array<uint64>(2), out var Ꮡpair);
    _ = zeroPair(new @unsafe.Pointer(Ꮡpair));
    linkaddr @base = 0x4000;
    var next = ((linkaddr)((uintptr)@base + 32));
    println(throughPointer(@base) == 0x4000, throughPointer(next) - throughPointer(@base));
    println("compiled");
}

} // end main_package
