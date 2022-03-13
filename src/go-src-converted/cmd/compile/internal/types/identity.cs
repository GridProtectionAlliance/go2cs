// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:07 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\identity.go
namespace go.cmd.compile.@internal;

public static partial class types_package {

// Identical reports whether t1 and t2 are identical types, following
// the spec rules. Receiver parameter types are ignored.
public static bool Identical(ptr<Type> _addr_t1, ptr<Type> _addr_t2) {
    ref Type t1 = ref _addr_t1.val;
    ref Type t2 = ref _addr_t2.val;

    return identical(_addr_t1, _addr_t2, true, null);
}

// IdenticalIgnoreTags is like Identical, but it ignores struct tags
// for struct identity.
public static bool IdenticalIgnoreTags(ptr<Type> _addr_t1, ptr<Type> _addr_t2) {
    ref Type t1 = ref _addr_t1.val;
    ref Type t2 = ref _addr_t2.val;

    return identical(_addr_t1, _addr_t2, false, null);
}

private partial struct typePair {
    public ptr<Type> t1;
    public ptr<Type> t2;
}

private static bool identical(ptr<Type> _addr_t1, ptr<Type> _addr_t2, bool cmpTags, object assumedEqual) {
    ref Type t1 = ref _addr_t1.val;
    ref Type t2 = ref _addr_t2.val;

    if (t1 == t2) {
        return true;
    }
    if (t1 == null || t2 == null || t1.kind != t2.kind || t1.Broke() || t2.Broke()) {
        return false;
    }
    if (t1.sym != null || t2.sym != null) { 
        // Special case: we keep byte/uint8 and rune/int32
        // separate for error messages. Treat them as equal.

        if (t1.kind == TUINT8) 
            return (t1 == Types[TUINT8] || t1 == ByteType) && (t2 == Types[TUINT8] || t2 == ByteType);
        else if (t1.kind == TINT32) 
            return (t1 == Types[TINT32] || t1 == RuneType) && (t2 == Types[TINT32] || t2 == RuneType);
        else 
            return false;
            }
    if (assumedEqual == null) {
        assumedEqual = make();
    }    {
        var (_, ok) = assumedEqual[new typePair(t1,t2)];


        else if (ok) {
            return true;
        }
    }
    assumedEqual[new typePair(t1,t2)] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};


    if (t1.kind == TIDEAL) 
        // Historically, cmd/compile used a single "untyped
        // number" type, so all untyped number types were
        // identical. Match this behavior.
        // TODO(mdempsky): Revisit this.
        return true;
    else if (t1.kind == TINTER) 
        if (t1.AllMethods().Len() != t2.AllMethods().Len()) {
            return false;
        }
        {
            var i__prev1 = i;
            var f1__prev1 = f1;

            foreach (var (__i, __f1) in t1.AllMethods().Slice()) {
                i = __i;
                f1 = __f1;
                var f2 = t2.AllMethods().Index(i);
                if (f1.Sym != f2.Sym || !identical(_addr_f1.Type, _addr_f2.Type, cmpTags, assumedEqual)) {
                    return false;
                }
            }

            i = i__prev1;
            f1 = f1__prev1;
        }

        return true;
    else if (t1.kind == TSTRUCT) 
        if (t1.NumFields() != t2.NumFields()) {
            return false;
        }
        {
            var i__prev1 = i;
            var f1__prev1 = f1;

            foreach (var (__i, __f1) in t1.FieldSlice()) {
                i = __i;
                f1 = __f1;
                f2 = t2.Field(i);
                if (f1.Sym != f2.Sym || f1.Embedded != f2.Embedded || !identical(_addr_f1.Type, _addr_f2.Type, cmpTags, assumedEqual)) {
                    return false;
                }
                if (cmpTags && f1.Note != f2.Note) {
                    return false;
                }
            }

            i = i__prev1;
            f1 = f1__prev1;
        }

        return true;
    else if (t1.kind == TFUNC) 
        // Check parameters and result parameters for type equality.
        // We intentionally ignore receiver parameters for type
        // equality, because they're never relevant.
        foreach (var (_, f) in ParamsResults) { 
            // Loop over fields in structs, ignoring argument names.
            var fs1 = f(t1).FieldSlice();
            var fs2 = f(t2).FieldSlice();
            if (len(fs1) != len(fs2)) {
                return false;
            }
            {
                var i__prev2 = i;
                var f1__prev2 = f1;

                foreach (var (__i, __f1) in fs1) {
                    i = __i;
                    f1 = __f1;
                    f2 = fs2[i];
                    if (f1.IsDDD() != f2.IsDDD() || !identical(_addr_f1.Type, _addr_f2.Type, cmpTags, assumedEqual)) {
                        return false;
                    }
                }

                i = i__prev2;
                f1 = f1__prev2;
            }
        }        return true;
    else if (t1.kind == TARRAY) 
        if (t1.NumElem() != t2.NumElem()) {
            return false;
        }
    else if (t1.kind == TCHAN) 
        if (t1.ChanDir() != t2.ChanDir()) {
            return false;
        }
    else if (t1.kind == TMAP) 
        if (!identical(_addr_t1.Key(), _addr_t2.Key(), cmpTags, assumedEqual)) {
            return false;
        }
        return identical(_addr_t1.Elem(), _addr_t2.Elem(), cmpTags, assumedEqual);
}

} // end types_package
