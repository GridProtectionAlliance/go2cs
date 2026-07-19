namespace go;

using @unsafe = unsafe_package;

partial class main_package {

[GoType("[3]nint")] partial struct Row;

internal static Row castDerefReturn(@unsafe.Pointer p) {
    return (~(ж<Row>)(uintptr)(p)).Clone();
}

internal static array<uintptr> castDerefReturnDirect(@unsafe.Pointer p) {
    return (~(ж<array<uintptr>>)(uintptr)(p)).Clone();
}

internal static void Main() {
    ref var r = ref heap(new Row(), out var Ꮡr);
    ref var u = ref heap(new array<uintptr>(2), out var Ꮡu);
    _ = castDerefReturn(new @unsafe.Pointer(Ꮡr));
    _ = castDerefReturnDirect(new @unsafe.Pointer(Ꮡu));
}

} // end main_package
