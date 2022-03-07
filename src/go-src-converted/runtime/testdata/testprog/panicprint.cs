// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:05 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\panicprint.go


namespace go;

public static partial class main_package {

public partial struct MyBool { // : bool
}
public partial struct MyComplex128 { // : System.Numerics.Complex128
}
public partial struct MyComplex64 { // : complex64
}
public partial struct MyFloat32 { // : float
}
public partial struct MyFloat64 { // : double
}
public partial struct MyInt { // : nint
}
public partial struct MyInt8 { // : sbyte
}
public partial struct MyInt16 { // : short
}
public partial struct MyInt32 { // : int
}
public partial struct MyInt64 { // : long
}
public partial struct MyString { // : @string
}
public partial struct MyUint { // : nuint
}
public partial struct MyUint8 { // : byte
}
public partial struct MyUint16 { // : ushort
}
public partial struct MyUint32 { // : uint
}
public partial struct MyUint64 { // : ulong
}
public partial struct MyUintptr { // : System.UIntPtr
}

private static void panicCustomComplex64() => func((_, panic, _) => {
    panic(MyComplex64(0.11F + 3i));
});

private static void panicCustomComplex128() => func((_, panic, _) => {
    panic(MyComplex128(32.1F + 10i));
});

private static void panicCustomString() => func((_, panic, _) => {
    panic(MyString("Panic"));
});

private static void panicCustomBool() => func((_, panic, _) => {
    panic(MyBool(true));
});

private static void panicCustomInt() => func((_, panic, _) => {
    panic(MyInt(93));
});

private static void panicCustomInt8() => func((_, panic, _) => {
    panic(MyInt8(93));
});

private static void panicCustomInt16() => func((_, panic, _) => {
    panic(MyInt16(93));
});

private static void panicCustomInt32() => func((_, panic, _) => {
    panic(MyInt32(93));
});

private static void panicCustomInt64() => func((_, panic, _) => {
    panic(MyInt64(93));
});

private static void panicCustomUint() => func((_, panic, _) => {
    panic(MyUint(93));
});

private static void panicCustomUint8() => func((_, panic, _) => {
    panic(MyUint8(93));
});

private static void panicCustomUint16() => func((_, panic, _) => {
    panic(MyUint16(93));
});

private static void panicCustomUint32() => func((_, panic, _) => {
    panic(MyUint32(93));
});

private static void panicCustomUint64() => func((_, panic, _) => {
    panic(MyUint64(93));
});

private static void panicCustomUintptr() => func((_, panic, _) => {
    panic(MyUintptr(93));
});

private static void panicCustomFloat64() => func((_, panic, _) => {
    panic(MyFloat64(-93.70F));
});

private static void panicCustomFloat32() => func((_, panic, _) => {
    panic(MyFloat32(-93.70F));
});

private static void init() {
    register("panicCustomComplex64", panicCustomComplex64);
    register("panicCustomComplex128", panicCustomComplex128);
    register("panicCustomBool", panicCustomBool);
    register("panicCustomFloat32", panicCustomFloat32);
    register("panicCustomFloat64", panicCustomFloat64);
    register("panicCustomInt", panicCustomInt);
    register("panicCustomInt8", panicCustomInt8);
    register("panicCustomInt16", panicCustomInt16);
    register("panicCustomInt32", panicCustomInt32);
    register("panicCustomInt64", panicCustomInt64);
    register("panicCustomString", panicCustomString);
    register("panicCustomUint", panicCustomUint);
    register("panicCustomUint8", panicCustomUint8);
    register("panicCustomUint16", panicCustomUint16);
    register("panicCustomUint32", panicCustomUint32);
    register("panicCustomUint64", panicCustomUint64);
    register("panicCustomUintptr", panicCustomUintptr);
}

} // end main_package
