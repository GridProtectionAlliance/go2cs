namespace go;

using @unsafe = unsafe_package;

partial class main_package {

public static @unsafe.Pointer NoEscape(@unsafe.Pointer p) {
    var x = ((uintptr)p);
    return ((@unsafe.Pointer)((uintptr)(x ^ 0)));
}

internal static bool alwaysFalse;

internal static any escapeSink;

public static T Escape<T>(T x)
    where T : new()
{
    if (alwaysFalse) {
        escapeSink = x;
    }
    return x;
}

} // end main_package
