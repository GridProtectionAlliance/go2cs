// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements export filtering of an AST.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;

partial class doc_package {

// filterIdentList removes unexported names from list in place
// and returns the resulting list.
internal static slice<ast.Ident> filterIdentList(slice<ast.Ident> list) {
    nint j = 0;
    foreach (var (_, x) in list) {
        if (token.IsExported((~x).Name)) {
            list[j] = x;
            j++;
        }
    }
    return list[0..(int)(j)];
}

internal static ж<ast.Ident> underscore = ast.NewIdent("_"u8);

internal static void filterCompositeLit(ж<ast.CompositeLit> Ꮡlit, ΔFilter filter, bool export) {
    ref var lit = ref Ꮡlit.val;

    nint n = len(lit.Elts);
    lit.Elts = filterExprList(lit.Elts, filter, export);
    if (len(lit.Elts) < n) {
        lit.Incomplete = true;
    }
}

internal static slice<ast.Expr> filterExprList(slice<ast.Expr> list, ΔFilter filter, bool export) {
    nint j = 0;
    foreach (var (_, exp) in list) {
        switch (exp.type()) {
        case ж<ast.CompositeLit> x: {
            filterCompositeLit(x, filter, export);
            break;
        }
        case ж<ast.KeyValueExpr> x: {
            {
                var (x, ok) = (~x).Key._<ж<ast.Ident>>(ᐧ); if (ok && !filter((~x).Name)) {
                    continue;
                }
            }
            {
                var (x, ok) = (~x).Value._<ж<ast.CompositeLit>>(ᐧ); if (ok) {
                    filterCompositeLit(x, filter, export);
                }
            }
            break;
        }}
        list[j] = exp;
        j++;
    }
    return list[0..(int)(j)];
}

// updateIdentList replaces all unexported identifiers with underscore
// and reports whether at least one exported name exists.
internal static bool /*hasExported*/ updateIdentList(slice<ast.Ident> list) {
    bool hasExported = default!;

    foreach (var (i, x) in list) {
        if (token.IsExported((~x).Name)){
            hasExported = true;
        } else {
            list[i] = underscore;
        }
    }
    return hasExported;
}

// hasExportedName reports whether list contains any exported names.
internal static bool hasExportedName(slice<ast.Ident> list) {
    foreach (var (_, x) in list) {
        if (x.IsExported()) {
            return true;
        }
    }
    return false;
}

// removeAnonymousField removes anonymous fields named name from an interface.
internal static void removeAnonymousField(@string name, ж<ast.InterfaceType> Ꮡityp) {
    ref var ityp = ref Ꮡityp.val;

    var list = ityp.Methods.List;
    // we know that ityp.Methods != nil
    nint j = 0;
    foreach (var (_, field) in list) {
        var keepField = true;
        {
            nint n = len((~field).Names); if (n == 0) {
                // anonymous field
                {
                    var (fname, _) = baseTypeName((~field).Type); if (fname == name) {
                        keepField = false;
                    }
                }
            }
        }
        if (keepField) {
            list[j] = field;
            j++;
        }
    }
    if (j < len(list)) {
        ityp.Incomplete = true;
    }
    ityp.Methods.List = list[0..(int)(j)];
}

// filterFieldList removes unexported fields (field names) from the field list
// in place and reports whether fields were removed. Anonymous fields are
// recorded with the parent type. filterType is called with the types of
// all remaining fields.
[GoRecv] internal static bool /*removedFields*/ filterFieldList(this ref reader r, ж<namedType> Ꮡparent, ж<ast.FieldList> Ꮡfields, ж<ast.InterfaceType> Ꮡityp) {
    bool removedFields = default!;

    ref var parent = ref Ꮡparent.val;
    ref var fields = ref Ꮡfields.val;
    ref var ityp = ref Ꮡityp.val;
    if (fields == nil) {
        return removedFields;
    }
    var list = fields.List;
    nint j = 0;
    foreach (var (_, field) in list) {
        var keepField = false;
        {
            nint n = len((~field).Names); if (n == 0){
                // anonymous field or embedded type or union element
                @string fname = r.recordAnonymousField(Ꮡparent, (~field).Type);
                if (fname != ""u8){
                    if (token.IsExported(fname)){
                        keepField = true;
                    } else 
                    if (ityp != nil && predeclaredTypes[fname]) {
                        // possibly an embedded predeclared type; keep it for now but
                        // remember this interface so that it can be fixed if name is also
                        // defined locally
                        keepField = true;
                        r.remember(fname, Ꮡityp);
                    }
                } else {
                    // If we're operating on an interface, assume that this is an embedded
                    // type or union element.
                    //
                    // TODO(rfindley): consider traversing into approximation/unions
                    // elements to see if they are entirely unexported.
                    keepField = ityp != nil;
                }
            } else {
                field.val.Names = filterIdentList((~field).Names);
                if (len((~field).Names) < n) {
                    removedFields = true;
                }
                if (len((~field).Names) > 0) {
                    keepField = true;
                }
            }
        }
        if (keepField) {
            r.filterType(nil, (~field).Type);
            list[j] = field;
            j++;
        }
    }
    if (j < len(list)) {
        removedFields = true;
    }
    fields.List = list[0..(int)(j)];
    return removedFields;
}

// filterParamList applies filterType to each parameter type in fields.
[GoRecv] internal static void filterParamList(this ref reader r, ж<ast.FieldList> Ꮡfields) {
    ref var fields = ref Ꮡfields.val;

    if (fields != nil) {
        foreach (var (_, f) in fields.List) {
            r.filterType(nil, (~f).Type);
        }
    }
}

// filterType strips any unexported struct fields or method types from typ
// in place. If fields (or methods) have been removed, the corresponding
// struct or interface type has the Incomplete field set to true.
[GoRecv] internal static void filterType(this ref reader r, ж<namedType> Ꮡparent, ast.Expr typ) {
    ref var parent = ref Ꮡparent.val;

    switch (typ.type()) {
    case ж<ast.Ident> t: {
        break;
    }
    case ж<ast.ParenExpr> t: {
        r.filterType(nil, // nothing to do
 (~t).X);
        break;
    }
    case ж<ast.StarExpr> t: {
        r.filterType(nil, // possibly an embedded type literal
 (~t).X);
        break;
    }
    case ж<ast.UnaryExpr> t: {
        if ((~t).Op == token.TILDE) {
            // approximation element
            r.filterType(nil, (~t).X);
        }
        break;
    }
    case ж<ast.BinaryExpr> t: {
        if ((~t).Op == token.OR) {
            // union
            r.filterType(nil, (~t).X);
            r.filterType(nil, (~t).Y);
        }
        break;
    }
    case ж<ast.ArrayType> t: {
        r.filterType(nil, (~t).Elt);
        break;
    }
    case ж<ast.StructType> t: {
        if (r.filterFieldList(Ꮡparent, (~t).Fields, nil)) {
            var t.val.Incomplete = true;
        }
        break;
    }
    case ж<ast.FuncType> t: {
        r.filterParamList((~t).TypeParams);
        r.filterParamList((~t).Params);
        r.filterParamList((~t).Results);
        break;
    }
    case ж<ast.InterfaceType> t: {
        if (r.filterFieldList(Ꮡparent, (~t).Methods, t)) {
            var t.val.Incomplete = true;
        }
        break;
    }
    case ж<ast.MapType> t: {
        r.filterType(nil, (~t).Key);
        r.filterType(nil, (~t).Value);
        break;
    }
    case ж<ast.ChanType> t: {
        r.filterType(nil, (~t).Value);
        break;
    }}
}

[GoRecv] internal static bool filterSpec(this ref reader r, ast.Spec spec) {
    switch (spec.type()) {
    case ж<ast.ImportSpec> s: {
        return true;
    }
    case ж<ast.ValueSpec> s: {
        var s.val.Values = filterExprList((~s).Values, // always keep imports so we can collect them
 token.IsExported, true);
        if (len((~s).Values) > 0 || (~s).Type == default! && len((~s).Values) == 0){
            // If there are values declared on RHS, just replace the unexported
            // identifiers on the LHS with underscore, so that it matches
            // the sequence of expression on the RHS.
            //
            // Similarly, if there are no type and values, then this expression
            // must be following an iota expression, where order matters.
            if (updateIdentList((~s).Names)) {
                r.filterType(nil, (~s).Type);
                return true;
            }
        } else {
            var s.val.Names = filterIdentList((~s).Names);
            if (len((~s).Names) > 0) {
                r.filterType(nil, (~s).Type);
                return true;
            }
        }
        break;
    }
    case ж<ast.TypeSpec> s: {
        {
            @string name = (~s).Name.val.Name; if (token.IsExported(name)){
                // Don't filter type parameters here, by analogy with function parameters
                // which are not filtered for top-level function declarations.
                r.filterType(r.lookupType((~(~s).Name).Name), (~s).Type);
                return true;
            } else 
            if (IsPredeclared(name)) {
                if (r.shadowedPredecl == default!) {
                    r.shadowedPredecl = new map<@string, bool>();
                }
                r.shadowedPredecl[name] = true;
            }
        }
        break;
    }}
    return false;
}

// copyConstType returns a copy of typ with position pos.
// typ must be a valid constant type.
// In practice, only (possibly qualified) identifiers are possible.
internal static ast.Expr copyConstType(ast.Expr typ, tokenꓸPos pos) {
    switch (typ.type()) {
    case ж<ast.Ident> typ: {
        return new ast.Ident(Name: (~typ).Name, NamePos: pos);
    }
    case ж<ast.SelectorExpr> typ: {
        {
            var (id, ok) = (~typ).X._<ж<ast.Ident>>(ᐧ); if (ok) {
                // presumably a qualified identifier
                return new ast.SelectorExpr(
                    Sel: ast.NewIdent((~(~typ).Sel).Name),
                    X: Ꮡ(new ast.Ident(Name: (~id).Name, NamePos: pos))
                );
            }
        }
        break;
    }}
    return default!;
}

// shouldn't happen, but be conservative and don't panic
[GoRecv] internal static slice<ast.Spec> filterSpecList(this ref reader r, slice<ast.Spec> list, token.Token tok) {
    if (tok == token.CONST) {
        // Propagate any type information that would get lost otherwise
        // when unexported constants are filtered.
        ast.Expr prevType = default!;
        foreach (var (_, spec) in list) {
            var specΔ1 = spec._<ж<ast.ValueSpec>>();
            if ((~specΔ1).Type == default! && len((~specΔ1).Values) == 0 && prevType != default!) {
                // provide current spec with an explicit type
                specΔ1.val.Type = copyConstType(prevType, specΔ1.Pos());
            }
            if (hasExportedName((~specΔ1).Names)){
                // exported names are preserved so there's no need to propagate the type
                prevType = default!;
            } else {
                prevType = specΔ1.val.Type;
            }
        }
    }
    nint j = 0;
    foreach (var (_, s) in list) {
        if (r.filterSpec(s)) {
            list[j] = s;
            j++;
        }
    }
    return list[0..(int)(j)];
}

[GoRecv] internal static bool filterDecl(this ref reader r, ast.Decl decl) {
    switch (decl.type()) {
    case ж<ast.GenDecl> d: {
        var d.val.Specs = r.filterSpecList((~d).Specs, (~d).Tok);
        return len((~d).Specs) > 0;
    }
    case ж<ast.FuncDecl> d: {
        return token.IsExported((~(~d).Name).Name);
    }}
    // ok to filter these methods early because any
    // conflicting method will be filtered here, too -
    // thus, removing these methods early will not lead
    // to the false removal of possible conflicts
    return false;
}

// fileExports removes unexported declarations from src in place.
[GoRecv] internal static void fileExports(this ref reader r, ж<ast.File> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    nint j = 0;
    foreach (var (_, d) in src.Decls) {
        if (r.filterDecl(d)) {
            src.Decls[j] = d;
            j++;
        }
    }
    src.Decls = src.Decls[0..(int)(j)];
}

} // end doc_package
