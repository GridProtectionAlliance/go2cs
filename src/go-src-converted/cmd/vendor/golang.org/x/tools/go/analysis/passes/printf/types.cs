// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package printf -- go2cs converted at 2022 March 13 06:42:01 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/printf" ==> using printf = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.printf_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\printf\types.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using analysisutil = golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;

public static partial class printf_package {

private static ptr<types.Interface> errorType = types.Universe.Lookup("error").Type().Underlying()._<ptr<types.Interface>>();

// matchArgType reports an error if printf verb t is not appropriate
// for operand arg.
//
// typ is used only for recursive calls; external callers must supply nil.
//
// (Recursion arises from the compound types {map,chan,slice} which
// may be printed with %d etc. if that is appropriate for their element
// types.)
private static bool matchArgType(ptr<analysis.Pass> _addr_pass, printfArgType t, types.Type typ, ast.Expr arg) {
    ref analysis.Pass pass = ref _addr_pass.val;

    return matchArgTypeInternal(_addr_pass, t, typ, arg, make_map<types.Type, bool>());
}

// matchArgTypeInternal is the internal version of matchArgType. It carries a map
// remembering what types are in progress so we don't recur when faced with recursive
// types or mutually recursive types.
private static bool matchArgTypeInternal(ptr<analysis.Pass> _addr_pass, printfArgType t, types.Type typ, ast.Expr arg, map<types.Type, bool> inProgress) => func((_, panic, _) => {
    ref analysis.Pass pass = ref _addr_pass.val;
 
    // %v, %T accept any argument type.
    if (t == anyType) {
        return true;
    }
    if (typ == null) { 
        // external call
        typ = pass.TypesInfo.Types[arg].Type;
        if (typ == null) {
            return true; // probably a type check problem
        }
    }
    if (t == argError) {
        return types.ConvertibleTo(typ, errorType);
    }
    if (isFormatter(typ)) {
        return true;
    }
    if (t & argString != 0 && isConvertibleToString(_addr_pass, typ)) {
        return true;
    }
    typ = typ.Underlying();
    if (inProgress[typ]) { 
        // We're already looking at this type. The call that started it will take care of it.
        return true;
    }
    inProgress[typ] = true;

    switch (typ.type()) {
        case ptr<types.Signature> typ:
            return t == argPointer;
            break;
        case ptr<types.Map> typ:
            return t == argPointer || (matchArgTypeInternal(_addr_pass, t, typ.Key(), arg, inProgress) && matchArgTypeInternal(_addr_pass, t, typ.Elem(), arg, inProgress));
            break;
        case ptr<types.Chan> typ:
            return t & argPointer != 0;
            break;
        case ptr<types.Array> typ:
            if (types.Identical(typ.Elem().Underlying(), types.Typ[types.Byte]) && t & argString != 0) {
                return true; // %s matches []byte
            } 
            // Recur: []int matches %d.
            return matchArgTypeInternal(_addr_pass, t, typ.Elem(), arg, inProgress);
            break;
        case ptr<types.Slice> typ:
            if (types.Identical(typ.Elem().Underlying(), types.Typ[types.Byte]) && t & argString != 0) {
                return true; // %s matches []byte
            }
            if (t == argPointer) {
                return true; // %p prints a slice's 0th element
            } 
            // Recur: []int matches %d. But watch out for
            //    type T []T
            // If the element is a pointer type (type T[]*T), it's handled fine by the Pointer case below.
            return matchArgTypeInternal(_addr_pass, t, typ.Elem(), arg, inProgress);
            break;
        case ptr<types.Pointer> typ:
            if (typ.Elem().String() == "invalid type") {
                if (false) {
                    pass.Reportf(arg.Pos(), "printf argument %v is pointer to invalid or unknown type", analysisutil.Format(pass.Fset, arg));
                }
                return true; // special case
            } 
            // If it's actually a pointer with %p, it prints as one.
            if (t == argPointer) {
                return true;
            }
            var under = typ.Elem().Underlying();
            switch (under.type()) {
                case ptr<types.Struct> _:
                    break;
                case ptr<types.Array> _:
                    break;
                case ptr<types.Slice> _:
                    break;
                case ptr<types.Map> _:
                    break;
                default:
                {
                    return t & argPointer != 0;
                    break;
                } 
                // If it's a top-level pointer to a struct, array, slice, or
                // map, that's equivalent in our analysis to whether we can
                // print the type being pointed to. Pointers in nested levels
                // are not supported to minimize fmt running into loops.
            } 
            // If it's a top-level pointer to a struct, array, slice, or
            // map, that's equivalent in our analysis to whether we can
            // print the type being pointed to. Pointers in nested levels
            // are not supported to minimize fmt running into loops.
            if (len(inProgress) > 1) {
                return false;
            }
            return matchArgTypeInternal(_addr_pass, t, under, arg, inProgress);
            break;
        case ptr<types.Struct> typ:
            return matchStructArgType(_addr_pass, t, _addr_typ, arg, inProgress);
            break;
        case ptr<types.Interface> typ:
            return true;
            break;
        case ptr<types.Basic> typ:

            if (typ.Kind() == types.UntypedBool || typ.Kind() == types.Bool) 
                return t & argBool != 0;
            else if (typ.Kind() == types.UntypedInt || typ.Kind() == types.Int || typ.Kind() == types.Int8 || typ.Kind() == types.Int16 || typ.Kind() == types.Int32 || typ.Kind() == types.Int64 || typ.Kind() == types.Uint || typ.Kind() == types.Uint8 || typ.Kind() == types.Uint16 || typ.Kind() == types.Uint32 || typ.Kind() == types.Uint64 || typ.Kind() == types.Uintptr) 
                return t & argInt != 0;
            else if (typ.Kind() == types.UntypedFloat || typ.Kind() == types.Float32 || typ.Kind() == types.Float64) 
                return t & argFloat != 0;
            else if (typ.Kind() == types.UntypedComplex || typ.Kind() == types.Complex64 || typ.Kind() == types.Complex128) 
                return t & argComplex != 0;
            else if (typ.Kind() == types.UntypedString || typ.Kind() == types.String) 
                return t & argString != 0;
            else if (typ.Kind() == types.UnsafePointer) 
                return t & (argPointer | argInt) != 0;
            else if (typ.Kind() == types.UntypedRune) 
                return t & (argInt | argRune) != 0;
            else if (typ.Kind() == types.UntypedNil) 
                return false;
            else if (typ.Kind() == types.Invalid) 
                if (false) {
                    pass.Reportf(arg.Pos(), "printf argument %v has invalid or unknown type", analysisutil.Format(pass.Fset, arg));
                }
                return true; // Probably a type check problem.
                        panic("unreachable");
            break;

    }

    return false;
});

private static bool isConvertibleToString(ptr<analysis.Pass> _addr_pass, types.Type typ) {
    ref analysis.Pass pass = ref _addr_pass.val;

    {
        ptr<types.Basic> (bt, ok) = typ._<ptr<types.Basic>>();

        if (ok && bt.Kind() == types.UntypedNil) { 
            // We explicitly don't want untyped nil, which is
            // convertible to both of the interfaces below, as it
            // would just panic anyway.
            return false;
        }
    }
    if (types.ConvertibleTo(typ, errorType)) {
        return true; // via .Error()
    }
    {
        var (obj, _, _) = types.LookupFieldOrMethod(typ, false, null, "String");

        if (obj != null) {
            {
                ptr<types.Func> (fn, ok) = obj._<ptr<types.Func>>();

                if (ok) {
                    ptr<types.Signature> sig = fn.Type()._<ptr<types.Signature>>();
                    if (sig.Params().Len() == 0 && sig.Results().Len() == 1 && sig.Results().At(0).Type() == types.Typ[types.String]) {
                        return true;
                    }
                }

            }
        }
    }

    return false;
}

// hasBasicType reports whether x's type is a types.Basic with the given kind.
private static bool hasBasicType(ptr<analysis.Pass> _addr_pass, ast.Expr x, types.BasicKind kind) {
    ref analysis.Pass pass = ref _addr_pass.val;

    var t = pass.TypesInfo.Types[x].Type;
    if (t != null) {
        t = t.Underlying();
    }
    ptr<types.Basic> (b, ok) = t._<ptr<types.Basic>>();
    return ok && b.Kind() == kind;
}

// matchStructArgType reports whether all the elements of the struct match the expected
// type. For instance, with "%d" all the elements must be printable with the "%d" format.
private static bool matchStructArgType(ptr<analysis.Pass> _addr_pass, printfArgType t, ptr<types.Struct> _addr_typ, ast.Expr arg, map<types.Type, bool> inProgress) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref types.Struct typ = ref _addr_typ.val;

    for (nint i = 0; i < typ.NumFields(); i++) {
        var typf = typ.Field(i);
        if (!matchArgTypeInternal(_addr_pass, t, typf.Type(), arg, inProgress)) {
            return false;
        }
        if (t & argString != 0 && !typf.Exported() && isConvertibleToString(_addr_pass, typf.Type())) { 
            // Issue #17798: unexported Stringer or error cannot be properly formatted.
            return false;
        }
    }
    return true;
}

} // end printf_package
