// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class pkgbits_package {

[GoType("num:int32")] partial struct RelocKind;

[GoType("num:int32")] partial struct Index;

// A relocEnt (relocation entry) is an entry in an element's local
// reference table.
//
// TODO(mdempsky): Rename this too.
[GoType] partial struct RelocEnt {
    public RelocKind Kind;
    public Index Idx;
}

// Reserved indices within the meta relocation section.
public static readonly Index PublicRootIdx = 0;

public static readonly Index PrivateRootIdx = 1;

public static readonly RelocKind RelocString = /* iota */ 0;
public static readonly RelocKind RelocMeta = 1;
public static readonly RelocKind RelocPosBase = 2;
public static readonly RelocKind RelocPkg = 3;
public static readonly RelocKind RelocName = 4;
public static readonly RelocKind RelocType = 5;
public static readonly RelocKind RelocObj = 6;
public static readonly RelocKind RelocObjExt = 7;
public static readonly RelocKind RelocObjDict = 8;
public static readonly RelocKind RelocBody = 9;
internal static readonly UntypedInt numRelocs = iota;

} // end pkgbits_package
