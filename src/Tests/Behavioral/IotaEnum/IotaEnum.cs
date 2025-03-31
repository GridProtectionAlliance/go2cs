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
public static readonly Kind String = 24;
public static readonly Kind Struct = 25;
public static readonly Kind UnsafePointer = 26;

internal static void Main() {
    fmt.Printf("Slice Kind Value: %v\n"u8, Slice);
}

} // end main_package
