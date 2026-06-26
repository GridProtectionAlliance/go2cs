// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

// A Code is an enum value that can be encoded into bitstreams.
//
// Code types are preferable for enum types, because they allow
// Decoder to detect desyncs.
[GoType] partial interface ΔCode {
    // Marker returns the SyncMarker for the Code's dynamic type.
    SyncMarker Marker();
    // Value returns the Code's ordinal value.
    nint Value();
}

[GoType("num:nint")] partial struct CodeVal;

public static SyncMarker Marker(this CodeVal c) {
    return SyncVal;
}

public static nint Value(this CodeVal c) {
    return ((nint)c);
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static readonly CodeVal ValBool = /* iota */ 0;
public static readonly CodeVal ValString = 1;
public static readonly CodeVal ValInt64 = 2;
public static readonly CodeVal ValBigInt = 3;
public static readonly CodeVal ValBigRat = 4;
public static readonly CodeVal ValBigFloat = 5;

[GoType("num:nint")] partial struct CodeType;

public static SyncMarker Marker(this CodeType c) {
    return SyncType;
}

public static nint Value(this CodeType c) {
    return ((nint)c);
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static readonly CodeType TypeBasic = /* iota */ 0;
public static readonly CodeType TypeNamed = 1;
public static readonly CodeType TypePointer = 2;
public static readonly CodeType TypeSlice = 3;
public static readonly CodeType TypeArray = 4;
public static readonly CodeType TypeChan = 5;
public static readonly CodeType TypeMap = 6;
public static readonly CodeType TypeSignature = 7;
public static readonly CodeType TypeStruct = 8;
public static readonly CodeType TypeInterface = 9;
public static readonly CodeType TypeUnion = 10;
public static readonly CodeType TypeTypeParam = 11;

[GoType("num:nint")] partial struct CodeObj;

public static SyncMarker Marker(this CodeObj c) {
    return SyncCodeObj;
}

public static nint Value(this CodeObj c) {
    return ((nint)c);
}

// Note: These values are public and cannot be changed without
// updating the go/types importers.
public static readonly CodeObj ObjAlias = /* iota */ 0;
public static readonly CodeObj ObjConst = 1;
public static readonly CodeObj ObjType = 2;
public static readonly CodeObj ObjFunc = 3;
public static readonly CodeObj ObjVar = 4;
public static readonly CodeObj ObjStub = 5;

} // end pkgbits_package
