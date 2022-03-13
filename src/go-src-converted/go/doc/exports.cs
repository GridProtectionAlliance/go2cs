// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements export filtering of an AST.

// package doc -- go2cs converted at 2022 March 13 05:52:35 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Program Files\Go\src\go\doc\exports.go
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;


// filterIdentList removes unexported names from list in place
// and returns the resulting list.
//

public static partial class doc_package {

private static slice<ptr<ast.Ident>> filterIdentList(slice<ptr<ast.Ident>> list) {
    nint j = 0;
    foreach (var (_, x) in list) {
        if (token.IsExported(x.Name)) {
            list[j] = x;
            j++;
        }
    }    return list[(int)0..(int)j];
}

private static var underscore = ast.NewIdent("_");

private static void filterCompositeLit(ptr<ast.CompositeLit> _addr_lit, Filter filter, bool export) {
    ref ast.CompositeLit lit = ref _addr_lit.val;

    var n = len(lit.Elts);
    lit.Elts = filterExprList(lit.Elts, filter, export);
    if (len(lit.Elts) < n) {
        lit.Incomplete = true;
    }
}

private static slice<ast.Expr> filterExprList(slice<ast.Expr> list, Filter filter, bool export) {
    nint j = 0;
    foreach (var (_, exp) in list) {
        switch (exp.type()) {
            case ptr<ast.CompositeLit> x:
                filterCompositeLit(_addr_x, filter, export);
                break;
            case ptr<ast.KeyValueExpr> x:
                {
                    var x__prev1 = x;

                    ptr<ast.Ident> (x, ok) = x.Key._<ptr<ast.Ident>>();

                    if (ok && !filter(x.Name)) {
                        continue;
                    }

                    x = x__prev1;

                }
                {
                    var x__prev1 = x;

                    (x, ok) = x.Value._<ptr<ast.CompositeLit>>();

                    if (ok) {
                        filterCompositeLit(_addr_x, filter, export);
                    }

                    x = x__prev1;

                }
                break;
        }
        list[j] = exp;
        j++;
    }    return list[(int)0..(int)j];
}

// updateIdentList replaces all unexported identifiers with underscore
// and reports whether at least one exported name exists.
private static bool updateIdentList(slice<ptr<ast.Ident>> list) {
    bool hasExported = default;

    foreach (var (i, x) in list) {
        if (token.IsExported(x.Name)) {
            hasExported = true;
        }
        else
 {
            list[i] = underscore;
        }
    }    return hasExported;
}

// hasExportedName reports whether list contains any exported names.
//
private static bool hasExportedName(slice<ptr<ast.Ident>> list) {
    foreach (var (_, x) in list) {
        if (x.IsExported()) {
            return true;
        }
    }    return false;
}

// removeErrorField removes anonymous fields named "error" from an interface.
// This is called when "error" has been determined to be a local name,
// not the predeclared type.
//
private static void removeErrorField(ptr<ast.InterfaceType> _addr_ityp) {
    ref ast.InterfaceType ityp = ref _addr_ityp.val;

    var list = ityp.Methods.List; // we know that ityp.Methods != nil
    nint j = 0;
    foreach (var (_, field) in list) {
        var keepField = true;
        {
            var n = len(field.Names);

            if (n == 0) { 
                // anonymous field
                {
                    var (fname, _) = baseTypeName(field.Type);

                    if (fname == "error") {
                        keepField = false;
                    }

                }
            }

        }
        if (keepField) {
            list[j] = field;
            j++;
        }
    }    if (j < len(list)) {
        ityp.Incomplete = true;
    }
    ityp.Methods.List = list[(int)0..(int)j];
}

// filterFieldList removes unexported fields (field names) from the field list
// in place and reports whether fields were removed. Anonymous fields are
// recorded with the parent type. filterType is called with the types of
// all remaining fields.
//
private static bool filterFieldList(this ptr<reader> _addr_r, ptr<namedType> _addr_parent, ptr<ast.FieldList> _addr_fields, ptr<ast.InterfaceType> _addr_ityp) {
    bool removedFields = default;
    ref reader r = ref _addr_r.val;
    ref namedType parent = ref _addr_parent.val;
    ref ast.FieldList fields = ref _addr_fields.val;
    ref ast.InterfaceType ityp = ref _addr_ityp.val;

    if (fields == null) {
        return ;
    }
    var list = fields.List;
    nint j = 0;
    foreach (var (_, field) in list) {
        var keepField = false;
        {
            var n = len(field.Names);

            if (n == 0) { 
                // anonymous field
                var fname = r.recordAnonymousField(parent, field.Type);
                if (token.IsExported(fname)) {
                    keepField = true;
                }
                else if (ityp != null && fname == "error") { 
                    // possibly the predeclared error interface; keep
                    // it for now but remember this interface so that
                    // it can be fixed if error is also defined locally
                    keepField = true;
                    r.remember(ityp);
                }
            }
            else
 {
                field.Names = filterIdentList(field.Names);
                if (len(field.Names) < n) {
                    removedFields = true;
                }
                if (len(field.Names) > 0) {
                    keepField = true;
                }
            }

        }
        if (keepField) {
            r.filterType(null, field.Type);
            list[j] = field;
            j++;
        }
    }    if (j < len(list)) {
        removedFields = true;
    }
    fields.List = list[(int)0..(int)j];
    return ;
}

// filterParamList applies filterType to each parameter type in fields.
//
private static void filterParamList(this ptr<reader> _addr_r, ptr<ast.FieldList> _addr_fields) {
    ref reader r = ref _addr_r.val;
    ref ast.FieldList fields = ref _addr_fields.val;

    if (fields != null) {
        foreach (var (_, f) in fields.List) {
            r.filterType(null, f.Type);
        }
    }
}

// filterType strips any unexported struct fields or method types from typ
// in place. If fields (or methods) have been removed, the corresponding
// struct or interface type has the Incomplete field set to true.
//
private static void filterType(this ptr<reader> _addr_r, ptr<namedType> _addr_parent, ast.Expr typ) {
    ref reader r = ref _addr_r.val;
    ref namedType parent = ref _addr_parent.val;

    switch (typ.type()) {
        case ptr<ast.Ident> t:
            break;
        case ptr<ast.ParenExpr> t:
            r.filterType(null, t.X);
            break;
        case ptr<ast.ArrayType> t:
            r.filterType(null, t.Elt);
            break;
        case ptr<ast.StructType> t:
            if (r.filterFieldList(parent, t.Fields, null)) {
                t.Incomplete = true;
            }
            break;
        case ptr<ast.FuncType> t:
            r.filterParamList(t.Params);
            r.filterParamList(t.Results);
            break;
        case ptr<ast.InterfaceType> t:
            if (r.filterFieldList(parent, t.Methods, t)) {
                t.Incomplete = true;
            }
            break;
        case ptr<ast.MapType> t:
            r.filterType(null, t.Key);
            r.filterType(null, t.Value);
            break;
        case ptr<ast.ChanType> t:
            r.filterType(null, t.Value);
            break;
    }
}

private static bool filterSpec(this ptr<reader> _addr_r, ast.Spec spec) {
    ref reader r = ref _addr_r.val;

    switch (spec.type()) {
        case ptr<ast.ImportSpec> s:
            return true;
            break;
        case ptr<ast.ValueSpec> s:
            s.Values = filterExprList(s.Values, token.IsExported, true);
            if (len(s.Values) > 0 || s.Type == null && len(s.Values) == 0) { 
                // If there are values declared on RHS, just replace the unexported
                // identifiers on the LHS with underscore, so that it matches
                // the sequence of expression on the RHS.
                //
                // Similarly, if there are no type and values, then this expression
                // must be following an iota expression, where order matters.
                if (updateIdentList(s.Names)) {
                    r.filterType(null, s.Type);
                    return true;
                }
            }
            else
 {
                s.Names = filterIdentList(s.Names);
                if (len(s.Names) > 0) {
                    r.filterType(null, s.Type);
                    return true;
                }
            }
            break;
        case ptr<ast.TypeSpec> s:
            {
                var name = s.Name.Name;

                if (token.IsExported(name)) {
                    r.filterType(r.lookupType(s.Name.Name), s.Type);
                    return true;
                }
                else if (name == "error") { 
                    // special case: remember that error is declared locally
                    r.errorDecl = true;
                }

            }
            break;
    }
    return false;
}

// copyConstType returns a copy of typ with position pos.
// typ must be a valid constant type.
// In practice, only (possibly qualified) identifiers are possible.
//
private static ast.Expr copyConstType(ast.Expr typ, token.Pos pos) {
    switch (typ.type()) {
        case ptr<ast.Ident> typ:
            return addr(new ast.Ident(Name:typ.Name,NamePos:pos));
            break;
        case ptr<ast.SelectorExpr> typ:
            {
                ptr<ast.Ident> (id, ok) = typ.X._<ptr<ast.Ident>>();

                if (ok) { 
                    // presumably a qualified identifier
                    return addr(new ast.SelectorExpr(Sel:ast.NewIdent(typ.Sel.Name),X:&ast.Ident{Name:id.Name,NamePos:pos},));
                }

            }
            break;
    }
    return null; // shouldn't happen, but be conservative and don't panic
}

private static slice<ast.Spec> filterSpecList(this ptr<reader> _addr_r, slice<ast.Spec> list, token.Token tok) {
    ref reader r = ref _addr_r.val;

    if (tok == token.CONST) { 
        // Propagate any type information that would get lost otherwise
        // when unexported constants are filtered.
        ast.Expr prevType = default;
        {
            var spec__prev1 = spec;

            foreach (var (_, __spec) in list) {
                spec = __spec;
                ptr<ast.ValueSpec> spec = spec._<ptr<ast.ValueSpec>>();
                if (spec.Type == null && len(spec.Values) == 0 && prevType != null) { 
                    // provide current spec with an explicit type
                    spec.Type = copyConstType(prevType, spec.Pos());
                }
                if (hasExportedName(spec.Names)) { 
                    // exported names are preserved so there's no need to propagate the type
                    prevType = null;
                }
                else
 {
                    prevType = spec.Type;
                }
            }

            spec = spec__prev1;
        }
    }
    nint j = 0;
    foreach (var (_, s) in list) {
        if (r.filterSpec(s)) {
            list[j] = s;
            j++;
        }
    }    return list[(int)0..(int)j];
}

private static bool filterDecl(this ptr<reader> _addr_r, ast.Decl decl) {
    ref reader r = ref _addr_r.val;

    switch (decl.type()) {
        case ptr<ast.GenDecl> d:
            d.Specs = r.filterSpecList(d.Specs, d.Tok);
            return len(d.Specs) > 0;
            break;
        case ptr<ast.FuncDecl> d:
            return token.IsExported(d.Name.Name);
            break;
    }
    return false;
}

// fileExports removes unexported declarations from src in place.
//
private static void fileExports(this ptr<reader> _addr_r, ptr<ast.File> _addr_src) {
    ref reader r = ref _addr_r.val;
    ref ast.File src = ref _addr_src.val;

    nint j = 0;
    foreach (var (_, d) in src.Decls) {
        if (r.filterDecl(d)) {
            src.Decls[j] = d;
            j++;
        }
    }    src.Decls = src.Decls[(int)0..(int)j];
}

} // end doc_package
