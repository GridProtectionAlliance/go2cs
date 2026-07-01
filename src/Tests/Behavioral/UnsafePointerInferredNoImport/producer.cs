namespace go;

using @unsafe = unsafe_package;

partial class main_package {

internal static @unsafe.Pointer ptrOf(ж<int64> Ꮡx) {
    ref var x = ref Ꮡx.val;

    return new @unsafe.Pointer(Ꮡx);
}

internal static bool isNil(@unsafe.Pointer p) {
    return p == nil;
}

internal static slice<@unsafe.Pointer> makePtrs(ж<int64> Ꮡx) {
    ref var x = ref Ꮡx.val;

    return new @unsafe.Pointer[]{new @unsafe.Pointer(Ꮡx)}.slice();
}

} // end main_package
