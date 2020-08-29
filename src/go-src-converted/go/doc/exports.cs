// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements export filtering of an AST.

// package doc -- go2cs converted at 2020 August 29 08:47:05 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\exports.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        // filterIdentList removes unexported names from list in place
        // and returns the resulting list.
        //
        private static slice<ref ast.Ident> filterIdentList(slice<ref ast.Ident> list)
        {
            long j = 0L;
            foreach (var (_, x) in list)
            {
                if (ast.IsExported(x.Name))
                {
                    list[j] = x;
                    j++;
                }
            }            return list[0L..j];
        }

        // hasExportedName reports whether list contains any exported names.
        //
        private static bool hasExportedName(slice<ref ast.Ident> list)
        {
            foreach (var (_, x) in list)
            {
                if (x.IsExported())
                {
                    return true;
                }
            }
            return false;
        }

        // removeErrorField removes anonymous fields named "error" from an interface.
        // This is called when "error" has been determined to be a local name,
        // not the predeclared type.
        //
        private static void removeErrorField(ref ast.InterfaceType ityp)
        {
            var list = ityp.Methods.List; // we know that ityp.Methods != nil
            long j = 0L;
            foreach (var (_, field) in list)
            {
                var keepField = true;
                {
                    var n = len(field.Names);

                    if (n == 0L)
                    { 
                        // anonymous field
                        {
                            var (fname, _) = baseTypeName(field.Type);

                            if (fname == "error")
                            {
                                keepField = false;
                            }

                        }
                    }

                }
                if (keepField)
                {
                    list[j] = field;
                    j++;
                }
            }
            if (j < len(list))
            {
                ityp.Incomplete = true;
            }
            ityp.Methods.List = list[0L..j];
        }

        // filterFieldList removes unexported fields (field names) from the field list
        // in place and reports whether fields were removed. Anonymous fields are
        // recorded with the parent type. filterType is called with the types of
        // all remaining fields.
        //
        private static bool filterFieldList(this ref reader r, ref namedType parent, ref ast.FieldList fields, ref ast.InterfaceType ityp)
        {
            if (fields == null)
            {
                return;
            }
            var list = fields.List;
            long j = 0L;
            foreach (var (_, field) in list)
            {
                var keepField = false;
                {
                    var n = len(field.Names);

                    if (n == 0L)
                    { 
                        // anonymous field
                        var fname = r.recordAnonymousField(parent, field.Type);
                        if (ast.IsExported(fname))
                        {
                            keepField = true;
                        }
                        else if (ityp != null && fname == "error")
                        { 
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
                        if (len(field.Names) < n)
                        {
                            removedFields = true;
                        }
                        if (len(field.Names) > 0L)
                        {
                            keepField = true;
                        }
                    }

                }
                if (keepField)
                {
                    r.filterType(null, field.Type);
                    list[j] = field;
                    j++;
                }
            }
            if (j < len(list))
            {
                removedFields = true;
            }
            fields.List = list[0L..j];
            return;
        }

        // filterParamList applies filterType to each parameter type in fields.
        //
        private static void filterParamList(this ref reader r, ref ast.FieldList fields)
        {
            if (fields != null)
            {
                foreach (var (_, f) in fields.List)
                {
                    r.filterType(null, f.Type);
                }
            }
        }

        // filterType strips any unexported struct fields or method types from typ
        // in place. If fields (or methods) have been removed, the corresponding
        // struct or interface type has the Incomplete field set to true.
        //
        private static void filterType(this ref reader r, ref namedType parent, ast.Expr typ)
        {
            switch (typ.type())
            {
                case ref ast.Ident t:
                    break;
                case ref ast.ParenExpr t:
                    r.filterType(null, t.X);
                    break;
                case ref ast.ArrayType t:
                    r.filterType(null, t.Elt);
                    break;
                case ref ast.StructType t:
                    if (r.filterFieldList(parent, t.Fields, null))
                    {
                        t.Incomplete = true;
                    }
                    break;
                case ref ast.FuncType t:
                    r.filterParamList(t.Params);
                    r.filterParamList(t.Results);
                    break;
                case ref ast.InterfaceType t:
                    if (r.filterFieldList(parent, t.Methods, t))
                    {
                        t.Incomplete = true;
                    }
                    break;
                case ref ast.MapType t:
                    r.filterType(null, t.Key);
                    r.filterType(null, t.Value);
                    break;
                case ref ast.ChanType t:
                    r.filterType(null, t.Value);
                    break;
            }
        }

        private static bool filterSpec(this ref reader r, ast.Spec spec)
        {
            switch (spec.type())
            {
                case ref ast.ImportSpec s:
                    return true;
                    break;
                case ref ast.ValueSpec s:
                    s.Names = filterIdentList(s.Names);
                    if (len(s.Names) > 0L)
                    {
                        r.filterType(null, s.Type);
                        return true;
                    }
                    break;
                case ref ast.TypeSpec s:
                    {
                        var name = s.Name.Name;

                        if (ast.IsExported(name))
                        {
                            r.filterType(r.lookupType(s.Name.Name), s.Type);
                            return true;
                        }
                        else if (name == "error")
                        { 
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
        private static ast.Expr copyConstType(ast.Expr typ, token.Pos pos)
        {
            switch (typ.type())
            {
                case ref ast.Ident typ:
                    return ref new ast.Ident(Name:typ.Name,NamePos:pos);
                    break;
                case ref ast.SelectorExpr typ:
                    {
                        ref ast.Ident (id, ok) = typ.X._<ref ast.Ident>();

                        if (ok)
                        { 
                            // presumably a qualified identifier
                            return ref new ast.SelectorExpr(Sel:ast.NewIdent(typ.Sel.Name),X:&ast.Ident{Name:id.Name,NamePos:pos},);
                        }

                    }
                    break;
            }
            return null; // shouldn't happen, but be conservative and don't panic
        }

        private static slice<ast.Spec> filterSpecList(this ref reader r, slice<ast.Spec> list, token.Token tok)
        {
            if (tok == token.CONST)
            { 
                // Propagate any type information that would get lost otherwise
                // when unexported constants are filtered.
                ast.Expr prevType = default;
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in list)
                    {
                        spec = __spec;
                        ref ast.ValueSpec spec = spec._<ref ast.ValueSpec>();
                        if (spec.Type == null && len(spec.Values) == 0L && prevType != null)
                        { 
                            // provide current spec with an explicit type
                            spec.Type = copyConstType(prevType, spec.Pos());
                        }
                        if (hasExportedName(spec.Names))
                        { 
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
            long j = 0L;
            foreach (var (_, s) in list)
            {
                if (r.filterSpec(s))
                {
                    list[j] = s;
                    j++;
                }
            }
            return list[0L..j];
        }

        private static bool filterDecl(this ref reader r, ast.Decl decl)
        {
            switch (decl.type())
            {
                case ref ast.GenDecl d:
                    d.Specs = r.filterSpecList(d.Specs, d.Tok);
                    return len(d.Specs) > 0L;
                    break;
                case ref ast.FuncDecl d:
                    return ast.IsExported(d.Name.Name);
                    break;
            }
            return false;
        }

        // fileExports removes unexported declarations from src in place.
        //
        private static void fileExports(this ref reader r, ref ast.File src)
        {
            long j = 0L;
            foreach (var (_, d) in src.Decls)
            {
                if (r.filterDecl(d))
                {
                    src.Decls[j] = d;
                    j++;
                }
            }
            src.Decls = src.Decls[0L..j];
        }
    }
}}
