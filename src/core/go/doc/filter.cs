// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = go.ast_package;

partial class doc_package {

public delegate bool Filter(@string _);

internal static bool matchFields(ж<ast.FieldList> Ꮡfields, ΔFilter f) {
    ref var fields = ref Ꮡfields.val;

    if (fields != nil) {
        foreach (var (_, field) in fields.List) {
            foreach (var (_, name) in (~field).Names) {
                if (f((~name).Name)) {
                    return true;
                }
            }
        }
    }
    return false;
}

internal static bool matchDecl(ж<ast.GenDecl> Ꮡd, ΔFilter f) {
    ref var d = ref Ꮡd.val;

    foreach (var (_, dΔ1) in d.Specs) {
        switch (dΔ1.type()) {
        case ж<ast.ValueSpec> v: {
            foreach (var (_, name) in (~v).Names) {
                if (f((~name).Name)) {
                    return true;
                }
            }
            break;
        }
        case ж<ast.TypeSpec> v: {
            if (f((~(~v).Name).Name)) {
                return true;
            }
            switch ((~v).Type.type()) {
            case ж<ast.StructType> t: {
                if (matchFields((~t).Fields, // We don't match ordinary parameters in filterFuncs, so by analogy don't
 // match type parameters here.
 f)) {
                    return true;
                }
                break;
            }
            case ж<ast.InterfaceType> t: {
                if (matchFields((~t).Methods, f)) {
                    return true;
                }
                break;
            }}
            break;
        }}
    }
    return false;
}

internal static slice<ж<Value>> filterValues(slice<ж<Value>> a, ΔFilter f) {
    nint w = 0;
    foreach (var (_, vd) in a) {
        if (matchDecl((~vd).Decl, f)) {
            a[w] = vd;
            w++;
        }
    }
    return a[0..(int)(w)];
}

internal static slice<ж<Func>> filterFuncs(slice<ж<Func>> a, ΔFilter f) {
    nint w = 0;
    foreach (var (_, fd) in a) {
        if (f((~fd).Name)) {
            a[w] = fd;
            w++;
        }
    }
    return a[0..(int)(w)];
}

internal static slice<ж<Type>> filterTypes(slice<ж<Type>> a, ΔFilter f) {
    nint w = 0;
    foreach (var (_, td) in a) {
        nint n = 0;
        // number of matches
        if (matchDecl((~td).Decl, f)){
            n = 1;
        } else {
            // type name doesn't match, but we may have matching consts, vars, factories or methods
            td.val.Consts = filterValues((~td).Consts, f);
            td.val.Vars = filterValues((~td).Vars, f);
            td.val.Funcs = filterFuncs((~td).Funcs, f);
            td.val.Methods = filterFuncs((~td).Methods, f);
            n += len((~td).Consts) + len((~td).Vars) + len((~td).Funcs) + len((~td).Methods);
        }
        if (n > 0) {
            a[w] = td;
            w++;
        }
    }
    return a[0..(int)(w)];
}

// Filter eliminates documentation for names that don't pass through the filter f.
// TODO(gri): Recognize "Type.Method" as a name.
[GoRecv] public static void Filter(this ref Package p, ΔFilter f) {
    p.Consts = filterValues(p.Consts, f);
    p.Vars = filterValues(p.Vars, f);
    p.Types = filterTypes(p.Types, f);
    p.Funcs = filterFuncs(p.Funcs, f);
    p.Doc = ""u8;
}

// don't show top-level package doc

} // end doc_package
