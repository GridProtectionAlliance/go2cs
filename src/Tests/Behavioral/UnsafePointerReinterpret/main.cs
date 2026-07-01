namespace go;

using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer indirectKey(@unsafe.Pointer k) {
    return ((ж<@unsafe.Pointer>)(uintptr)(k)).val;
}

internal static void store(ж<@unsafe.Pointer> Ꮡdst, @unsafe.Pointer val) {
    ref var dst = ref Ꮡdst.val;

    dst = val;
}

internal static void writeBarrier(@unsafe.Pointer ptr, @unsafe.Pointer val) {
    store((ж<@unsafe.Pointer>)(uintptr)(ptr), val);
}

internal static void Main() {
    ref var backing = ref heap(new uintptr(), out var Ꮡbacking);
    @unsafe.Pointer p = @unsafe.Pointer.FromRef(ref (Ꮡbacking).val);
    writeBarrier(p, p);
    _ = (uintptr)indirectKey(p);
    println("compiled");
}

} // end main_package
