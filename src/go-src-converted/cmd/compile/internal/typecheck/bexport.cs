// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:47:44 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\bexport.go
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // ----------------------------------------------------------------------------
    // Export format

    // Tags. Must be < 0.
 
// Objects
private static readonly var packageTag = -(iota + 1);
private static readonly var constTag = 0;
private static readonly var typeTag = 1;
private static readonly var varTag = 2;
private static readonly var funcTag = 3;
private static readonly var endTag = 4; 

// Types
private static readonly var namedTag = 5;
private static readonly var arrayTag = 6;
private static readonly var sliceTag = 7;
private static readonly var dddTag = 8;
private static readonly var structTag = 9;
private static readonly var pointerTag = 10;
private static readonly var signatureTag = 11;
private static readonly var interfaceTag = 12;
private static readonly var mapTag = 13;
private static readonly var chanTag = 14; 

// Values
private static readonly var falseTag = 15;
private static readonly var trueTag = 16;
private static readonly var int64Tag = 17;
private static readonly var floatTag = 18;
private static readonly var fractionTag = 19; // not used by gc
private static readonly var complexTag = 20;
private static readonly var stringTag = 21;
private static readonly var nilTag = 22;
private static readonly var unknownTag = 23; // not used by gc (only appears in packages with errors)

// Type aliases
private static readonly var aliasTag = 24;


private static slice<ptr<types.Type>> predecl = default; // initialized lazily

private static slice<ptr<types.Type>> predeclared() {
    if (predecl == null) { 
        // initialize lazily to be sure that all
        // elements have been initialized before
        predecl = new slice<ptr<types.Type>>(new ptr<types.Type>[] { types.Types[types.TBOOL], types.Types[types.TINT], types.Types[types.TINT8], types.Types[types.TINT16], types.Types[types.TINT32], types.Types[types.TINT64], types.Types[types.TUINT], types.Types[types.TUINT8], types.Types[types.TUINT16], types.Types[types.TUINT32], types.Types[types.TUINT64], types.Types[types.TUINTPTR], types.Types[types.TFLOAT32], types.Types[types.TFLOAT64], types.Types[types.TCOMPLEX64], types.Types[types.TCOMPLEX128], types.Types[types.TSTRING], types.ByteType, types.RuneType, types.ErrorType, types.UntypedBool, types.UntypedInt, types.UntypedRune, types.UntypedFloat, types.UntypedComplex, types.UntypedString, types.Types[types.TNIL], types.Types[types.TUNSAFEPTR], types.Types[types.Txxx], types.Types[types.TANY] });

    }
    return predecl;

}

} // end typecheck_package
