namespace go;

using _ = unsafe_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static bool blankImportInfersUnsafe() {
    ref var @base = ref heap(new int64(), out var Ꮡbase);
    @base = 7;
    @unsafe.Pointer p = (uintptr)ptrOf(Ꮡbase);
    return p == nil;
}

} // end main_package
