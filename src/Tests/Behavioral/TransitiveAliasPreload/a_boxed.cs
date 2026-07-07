namespace go;

using CrossPkgBox = CrossPkgBox_package;

partial class main_package {

internal static any peekPtr(nint code) {
    var b = CrossPkgBox.New(code);
    ref var s = ref heap<CrossPkgLibꓸStatus>(out var Ꮡs);
    s = b.S;
    return Ꮡs;
}

} // end main_package
