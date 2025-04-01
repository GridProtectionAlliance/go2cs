namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:uint8")] partial struct Kind;

public static readonly Kind Invalid = /* iota */ 0;
public static readonly Kind Bool = 1;
public static readonly Kind Int = 2;
public static readonly Kind Int8 = 3;
public static readonly Kind Int16 = 4;
public static readonly Kind Int32 = 5;
public static readonly Kind Int64 = 6;
public static readonly Kind Uint = 7;
public static readonly Kind Uint8 = 8;
public static readonly Kind Uint16 = 9;
public static readonly Kind Uint32 = 10;
public static readonly Kind Uint64 = 11;
public static readonly Kind Uintptr = 12;
public static readonly Kind Float32 = 13;
public static readonly Kind Float64 = 14;
public static readonly Kind Complex64 = 15;
public static readonly Kind Complex128 = 16;
public static readonly Kind Array = 17;
public static readonly Kind Chan = 18;
public static readonly Kind Func = 19;
public static readonly Kind Interface = 20;
public static readonly Kind Map = 21;
public static readonly Kind Pointer = 22;
public static readonly Kind Slice = 23;
public static readonly Kind ΔString = 24;
public static readonly Kind Struct = 25;
public static readonly Kind UnsafePointer = 26;

public static @string String(this Kind k) {
    if (((nint)k) < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

internal static slice<@string> kindNames = new runtime.SparseArray<@string>{
    [Invalid] = "invalid"u8,
    [Bool] = "bool"u8,
    [Int] = "int"u8,
    [Int8] = "int8"u8,
    [Int16] = "int16"u8,
    [Int32] = "int32"u8,
    [Int64] = "int64"u8,
    [Uint] = "uint"u8,
    [Uint8] = "uint8"u8,
    [Uint16] = "uint16"u8,
    [Uint32] = "uint32"u8,
    [Uint64] = "uint64"u8,
    [Uintptr] = "uintptr"u8,
    [Float32] = "float32"u8,
    [Float64] = "float64"u8,
    [Complex64] = "complex64"u8,
    [Complex128] = "complex128"u8,
    [Array] = "array"u8,
    [Chan] = "chan"u8,
    [Func] = "func"u8,
    [Interface] = "interface"u8,
    [Map] = "map"u8,
    [Pointer] = "ptr"u8,
    [Slice] = "slice"u8,
    [ΔString] = "string"u8,
    [Struct] = "struct"u8,
    [UnsafePointer] = "unsafe.Pointer"u8
}.slice();

internal static void Main() {
    fmt.Printf("Slice Kind Value: %s [%d]\n"u8, Slice.String(), ((nint)Slice));
}

} // end main_package
