// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:00 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\alg.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;

public static partial class types_package {

// AlgKind describes the kind of algorithms used for comparing and
// hashing a Type.
public partial struct AlgKind { // : nint
}

//go:generate stringer -type AlgKind -trimprefix A alg.go

 
// These values are known by runtime.
public static readonly AlgKind ANOEQ = iota;
public static readonly var AMEM0 = 0;
public static readonly var AMEM8 = 1;
public static readonly var AMEM16 = 2;
public static readonly var AMEM32 = 3;
public static readonly var AMEM64 = 4;
public static readonly var AMEM128 = 5;
public static readonly var ASTRING = 6;
public static readonly var AINTER = 7;
public static readonly var ANILINTER = 8;
public static readonly var AFLOAT32 = 9;
public static readonly var AFLOAT64 = 10;
public static readonly var ACPLX64 = 11;
public static readonly var ACPLX128 = 12; 

// Type can be compared/hashed as regular memory.
public static readonly AlgKind AMEM = 100; 

// Type needs special comparison/hashing functions.
public static readonly AlgKind ASPECIAL = -1;

// AlgType returns the AlgKind used for comparing and hashing Type t.
// If it returns ANOEQ, it also returns the component type of t that
// makes it incomparable.
public static (AlgKind, ptr<Type>) AlgType(ptr<Type> _addr_t) {
    AlgKind _p0 = default;
    ptr<Type> _p0 = default!;
    ref Type t = ref _addr_t.val;

    if (t.Broke()) {
        return (AMEM, _addr_null!);
    }
    if (t.Noalg()) {
        return (ANOEQ, _addr_t!);
    }

    if (t.Kind() == TANY || t.Kind() == TFORW) 
        // will be defined later.
        return (ANOEQ, _addr_t!);
    else if (t.Kind() == TINT8 || t.Kind() == TUINT8 || t.Kind() == TINT16 || t.Kind() == TUINT16 || t.Kind() == TINT32 || t.Kind() == TUINT32 || t.Kind() == TINT64 || t.Kind() == TUINT64 || t.Kind() == TINT || t.Kind() == TUINT || t.Kind() == TUINTPTR || t.Kind() == TBOOL || t.Kind() == TPTR || t.Kind() == TCHAN || t.Kind() == TUNSAFEPTR) 
        return (AMEM, _addr_null!);
    else if (t.Kind() == TFUNC || t.Kind() == TMAP) 
        return (ANOEQ, _addr_t!);
    else if (t.Kind() == TFLOAT32) 
        return (AFLOAT32, _addr_null!);
    else if (t.Kind() == TFLOAT64) 
        return (AFLOAT64, _addr_null!);
    else if (t.Kind() == TCOMPLEX64) 
        return (ACPLX64, _addr_null!);
    else if (t.Kind() == TCOMPLEX128) 
        return (ACPLX128, _addr_null!);
    else if (t.Kind() == TSTRING) 
        return (ASTRING, _addr_null!);
    else if (t.Kind() == TINTER) 
        if (t.IsEmptyInterface()) {
            return (ANILINTER, _addr_null!);
        }
        return (AINTER, _addr_null!);
    else if (t.Kind() == TSLICE) 
        return (ANOEQ, _addr_t!);
    else if (t.Kind() == TARRAY) 
        var (a, bad) = AlgType(_addr_t.Elem());

        if (a == AMEM) 
            return (AMEM, _addr_null!);
        else if (a == ANOEQ) 
            return (ANOEQ, _addr_bad!);
                switch (t.NumElem()) {
            case 0: 
                // We checked above that the element type is comparable.
                return (AMEM, _addr_null!);
                break;
            case 1: 
                // Single-element array is same as its lone element.
                return (a, _addr_null!);
                break;
        }

        return (ASPECIAL, _addr_null!);
    else if (t.Kind() == TSTRUCT) 
        var fields = t.FieldSlice(); 

        // One-field struct is same as that one field alone.
        if (len(fields) == 1 && !fields[0].Sym.IsBlank()) {
            return AlgType(_addr_fields[0].Type);
        }
        var ret = AMEM;
        foreach (var (i, f) in fields) { 
            // All fields must be comparable.
            (a, bad) = AlgType(_addr_f.Type);
            if (a == ANOEQ) {
                return (ANOEQ, _addr_bad!);
            } 

            // Blank fields, padded fields, fields with non-memory
            // equality need special compare.
            if (a != AMEM || f.Sym.IsBlank() || IsPaddedField(_addr_t, i)) {
                ret = ASPECIAL;
            }
        }        return (ret, _addr_null!);
        @base.Fatalf("AlgType: unexpected type %v", t);
    return (0, _addr_null!);
}

// TypeHasNoAlg reports whether t does not have any associated hash/eq
// algorithms because t, or some component of t, is marked Noalg.
public static bool TypeHasNoAlg(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var (a, bad) = AlgType(_addr_t);
    return a == ANOEQ && bad.Noalg();
}

// IsComparable reports whether t is a comparable type.
public static bool IsComparable(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var (a, _) = AlgType(_addr_t);
    return a != ANOEQ;
}

// IncomparableField returns an incomparable Field of struct Type t, if any.
public static ptr<Field> IncomparableField(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    foreach (var (_, f) in t.FieldSlice()) {
        if (!IsComparable(_addr_f.Type)) {
            return _addr_f!;
        }
    }    return _addr_null!;
}

// IsPaddedField reports whether the i'th field of struct type t is followed
// by padding.
public static bool IsPaddedField(ptr<Type> _addr_t, nint i) {
    ref Type t = ref _addr_t.val;

    if (!t.IsStruct()) {
        @base.Fatalf("IsPaddedField called non-struct %v", t);
    }
    var end = t.Width;
    if (i + 1 < t.NumFields()) {
        end = t.Field(i + 1).Offset;
    }
    return t.Field(i).End() != end;
}

} // end types_package
