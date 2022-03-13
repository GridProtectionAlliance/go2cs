// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:51 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\validate.go
namespace go.cmd.compile.@internal;

using constant = go.constant_package;

using @base = cmd.compile.@internal.@base_package;
using syntax = cmd.compile.@internal.syntax_package;
using types = cmd.compile.@internal.types_package;
using types2 = cmd.compile.@internal.types2_package;


// match reports whether types t1 and t2 are consistent
// representations for a given expression's type.

public static partial class noder_package {

private static bool match(this ptr<irgen> _addr_g, ptr<types.Type> _addr_t1, types2.Type t2, bool hasOK) {
    ref irgen g = ref _addr_g.val;
    ref types.Type t1 = ref _addr_t1.val;

    ptr<types2.Tuple> (tuple, ok) = t2._<ptr<types2.Tuple>>();
    if (!ok) { 
        // Not a tuple; can use simple type identity comparison.
        return types.Identical(t1, g.typ(t2));
    }
    if (hasOK) { 
        // For has-ok values, types2 represents the expression's type as a
        // 2-element tuple, whereas ir just uses the first type and infers
        // that the second type is boolean. Must match either, since we
        // sometimes delay the transformation to the ir form.
        if (tuple.Len() == 2 && types.Identical(t1, g.typ(tuple.At(0).Type()))) {
            return true;
        }
        return types.Identical(t1, g.typ(t2));
    }
    if (t1 == null || tuple == null) {
        return t1 == null && tuple == null;
    }
    if (!t1.IsFuncArgStruct()) {
        return false;
    }
    if (t1.NumFields() != tuple.Len()) {
        return false;
    }
    foreach (var (i, result) in t1.FieldSlice()) {
        if (!types.Identical(result.Type, g.typ(tuple.At(i).Type()))) {
            return false;
        }
    }    return true;
}

private static void validate(this ptr<irgen> _addr_g, syntax.Node n) {
    ref irgen g = ref _addr_g.val;

    switch (n.type()) {
        case ptr<syntax.CallExpr> n:
            var tv = g.info.Types[n.Fun];
            if (tv.IsBuiltin()) {
                switch (n.Fun.type()) {
                    case ptr<syntax.Name> builtin:
                        g.validateBuiltin(builtin.Value, n);
                        break;
                    case ptr<syntax.SelectorExpr> builtin:
                        g.validateBuiltin(builtin.Sel.Value, n);
                        break;
                    default:
                    {
                        var builtin = n.Fun.type();
                        g.unhandled("builtin", n);
                        break;
                    }
                }
            }
            break;
    }
}

private static void validateBuiltin(this ptr<irgen> _addr_g, @string name, ptr<syntax.CallExpr> _addr_call) {
    ref irgen g = ref _addr_g.val;
    ref syntax.CallExpr call = ref _addr_call.val;

    switch (name) {
        case "Alignof": 
            // Check that types2+gcSizes calculates sizes the same
            // as cmd/compile does.


        case "Offsetof": 
            // Check that types2+gcSizes calculates sizes the same
            // as cmd/compile does.


        case "Sizeof": 
            // Check that types2+gcSizes calculates sizes the same
            // as cmd/compile does.

            var (got, ok) = constant.Int64Val(g.info.Types[call].Value);
            if (!ok) {
                @base.FatalfAt(g.pos(call), "expected int64 constant value");
            }
            var want = g.unsafeExpr(name, call.ArgList[0]);
            if (got != want) {
                @base.FatalfAt(g.pos(call), "got %v from types2, but want %v", got, want);
            }
            break;
    }
}

// unsafeExpr evaluates the given unsafe builtin function on arg.
private static long unsafeExpr(this ptr<irgen> _addr_g, @string name, syntax.Expr arg) {
    ref irgen g = ref _addr_g.val;

    switch (name) {
        case "Alignof": 
            return g.typ(g.info.Types[arg].Type).Alignment();
            break;
        case "Sizeof": 
            return g.typ(g.info.Types[arg].Type).Size();
            break;
    } 

    // Offsetof

    ptr<syntax.SelectorExpr> sel = arg._<ptr<syntax.SelectorExpr>>();
    var selection = g.info.Selections[sel];

    var typ = g.typ(g.info.Types[sel.X].Type);
    typ = deref(typ);

    long offset = default;
    foreach (var (_, i) in selection.Index()) { 
        // Ensure field offsets have been calculated.
        types.CalcSize(typ);

        var f = typ.Field(i);
        offset += f.Offset;
        typ = f.Type;
    }    return offset;
}

} // end noder_package
