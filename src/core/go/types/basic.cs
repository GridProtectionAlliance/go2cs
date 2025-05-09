// Code generated by "go test -run=Generate -write=all"; DO NOT EDIT.
// Source: ../../cmd/compile/internal/types2/basic.go
// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

partial class types_package {

[GoType("num:nint")] partial struct BasicKind;

public static readonly BasicKind Invalid = /* iota */ 0;  // type is invalid
public static readonly BasicKind Bool = 1;
public static readonly BasicKind Int = 2;
public static readonly BasicKind Int8 = 3;
public static readonly BasicKind Int16 = 4;
public static readonly BasicKind Int32 = 5;
public static readonly BasicKind Int64 = 6;
public static readonly BasicKind Uint = 7;
public static readonly BasicKind Uint8 = 8;
public static readonly BasicKind Uint16 = 9;
public static readonly BasicKind Uint32 = 10;
public static readonly BasicKind Uint64 = 11;
public static readonly BasicKind Uintptr = 12;
public static readonly BasicKind Float32 = 13;
public static readonly BasicKind Float64 = 14;
public static readonly BasicKind Complex64 = 15;
public static readonly BasicKind Complex128 = 16;
public static readonly BasicKind ΔString = 17;
public static readonly BasicKind UnsafePointer = 18;
public static readonly BasicKind UntypedBool = 19;
public static readonly BasicKind ΔUntypedInt = 20;
public static readonly BasicKind UntypedRune = 21;
public static readonly BasicKind ΔUntypedFloat = 22;
public static readonly BasicKind ΔUntypedComplex = 23;
public static readonly BasicKind UntypedString = 24;
public static readonly BasicKind UntypedNil = 25;
public static readonly BasicKind Byte = /* Uint8 */ 8;
public static readonly BasicKind Rune = /* Int32 */ 5;

[GoType("num:nint")] partial struct BasicInfo;

// Properties of basic types.
public static readonly BasicInfo IsBoolean = /* 1 << iota */ 1;

public static readonly BasicInfo IsInteger = 2;

public static readonly BasicInfo IsUnsigned = 4;

public static readonly BasicInfo IsFloat = 8;

public static readonly BasicInfo IsComplex = 16;

public static readonly BasicInfo IsString = 32;

public static readonly BasicInfo IsUntyped = 64;

public static readonly BasicInfo IsOrdered = /* IsInteger | IsFloat | IsString */ 42;

public static readonly BasicInfo IsNumeric = /* IsInteger | IsFloat | IsComplex */ 26;

public static readonly BasicInfo IsConstType = /* IsBoolean | IsNumeric | IsString */ 59;

// A Basic represents a basic type.
[GoType] partial struct Basic {
    internal BasicKind kind;
    internal BasicInfo info;
    internal @string name;
}

// Kind returns the kind of basic type b.
[GoRecv] public static BasicKind Kind(this ref Basic b) {
    return b.kind;
}

// Info returns information about properties of basic type b.
[GoRecv] public static BasicInfo Info(this ref Basic b) {
    return b.info;
}

// Name returns the name of basic type b.
[GoRecv] public static @string Name(this ref Basic b) {
    return b.name;
}

[GoRecv("capture")] public static ΔType Underlying(this ref Basic b) {
    return ~b;
}

[GoRecv] public static @string String(this ref Basic b) {
    return TypeString(~b, default!);
}

} // end types_package
