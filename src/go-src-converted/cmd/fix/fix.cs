// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:15:46 UTC
// Original source: C:\Program Files\Go\src\cmd\fix\fix.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using path = go.path_package;
using strconv = go.strconv_package;
using System;


namespace go;

public static partial class main_package {

private partial struct fix {
    public @string name;
    public @string date; // date that fix was introduced, in YYYY-MM-DD format
    public Func<ptr<ast.File>, bool> f;
    public @string desc;
    public bool disabled; // whether this fix should be disabled by default
}

// main runs sort.Sort(byName(fixes)) before printing list of fixes.
private partial struct byName { // : slice<fix>
}

private static nint Len(this byName f) {
    return len(f);
}
private static void Swap(this byName f, nint i, nint j) {
    (f[i], f[j]) = (f[j], f[i]);
}
private static bool Less(this byName f, nint i, nint j) {
    return f[i].name < f[j].name;
}

// main runs sort.Sort(byDate(fixes)) before applying fixes.
private partial struct byDate { // : slice<fix>
}

private static nint Len(this byDate f) {
    return len(f);
}
private static void Swap(this byDate f, nint i, nint j) {
    (f[i], f[j]) = (f[j], f[i]);
}
private static bool Less(this byDate f, nint i, nint j) {
    return f[i].date < f[j].date;
}

private static slice<fix> fixes = default;

private static void register(fix f) {
    fixes = append(fixes, f);
}

// walk traverses the AST x, calling visit(y) for each node y in the tree but
// also with a pointer to each ast.Expr, ast.Stmt, and *ast.BlockStmt,
// in a bottom-up traversal.
private static void walk(object x, Action<object> visit) {
    walkBeforeAfter(x, nop, visit);
}

private static void nop(object _p0) {
}

// walkBeforeAfter is like walk but calls before(x) before traversing
// x's children and after(x) afterward.
private static void walkBeforeAfter(object x, Action<object> before, Action<object> after) => func((_, panic, _) => {
    before(x);

    switch (x.type()) {
        case 
            break;
        case ptr<ast.Decl> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ast.Expr> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ast.Spec> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ast.Stmt> n:
            walkBeforeAfter(n.val, before, after); 

            // pointers to struct pointers
            break;
        case ptr<ptr<ast.BlockStmt>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ptr<ast.CallExpr>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ptr<ast.FieldList>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ptr<ast.FuncType>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ptr<ast.Ident>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<ptr<ast.BasicLit>> n:
            walkBeforeAfter(n.val, before, after); 

            // pointers to slices
            break;
        case ptr<slice<ast.Decl>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<slice<ast.Expr>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<slice<ptr<ast.File>>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<slice<ptr<ast.Ident>>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<slice<ast.Spec>> n:
            walkBeforeAfter(n.val, before, after);
            break;
        case ptr<slice<ast.Stmt>> n:
            walkBeforeAfter(n.val, before, after); 

            // These are ordered and grouped to match ../../go/ast/ast.go
            break;
        case ptr<ast.Field> n:
            walkBeforeAfter(_addr_n.Names, before, after);
            walkBeforeAfter(_addr_n.Type, before, after);
            walkBeforeAfter(_addr_n.Tag, before, after);
            break;
        case ptr<ast.FieldList> n:
            foreach (var (_, field) in n.List) {
                walkBeforeAfter(field, before, after);
            }
            break;
        case ptr<ast.BadExpr> n:
            break;
        case ptr<ast.Ident> n:
            break;
        case ptr<ast.Ellipsis> n:
            walkBeforeAfter(_addr_n.Elt, before, after);
            break;
        case ptr<ast.BasicLit> n:
            break;
        case ptr<ast.FuncLit> n:
            walkBeforeAfter(_addr_n.Type, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.CompositeLit> n:
            walkBeforeAfter(_addr_n.Type, before, after);
            walkBeforeAfter(_addr_n.Elts, before, after);
            break;
        case ptr<ast.ParenExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.SelectorExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.IndexExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            walkBeforeAfter(_addr_n.Index, before, after);
            break;
        case ptr<ast.SliceExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            if (n.Low != null) {
                walkBeforeAfter(_addr_n.Low, before, after);
            }
            if (n.High != null) {
                walkBeforeAfter(_addr_n.High, before, after);
            }
            break;
        case ptr<ast.TypeAssertExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            walkBeforeAfter(_addr_n.Type, before, after);
            break;
        case ptr<ast.CallExpr> n:
            walkBeforeAfter(_addr_n.Fun, before, after);
            walkBeforeAfter(_addr_n.Args, before, after);
            break;
        case ptr<ast.StarExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.UnaryExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.BinaryExpr> n:
            walkBeforeAfter(_addr_n.X, before, after);
            walkBeforeAfter(_addr_n.Y, before, after);
            break;
        case ptr<ast.KeyValueExpr> n:
            walkBeforeAfter(_addr_n.Key, before, after);
            walkBeforeAfter(_addr_n.Value, before, after);
            break;
        case ptr<ast.ArrayType> n:
            walkBeforeAfter(_addr_n.Len, before, after);
            walkBeforeAfter(_addr_n.Elt, before, after);
            break;
        case ptr<ast.StructType> n:
            walkBeforeAfter(_addr_n.Fields, before, after);
            break;
        case ptr<ast.FuncType> n:
            walkBeforeAfter(_addr_n.Params, before, after);
            if (n.Results != null) {
                walkBeforeAfter(_addr_n.Results, before, after);
            }
            break;
        case ptr<ast.InterfaceType> n:
            walkBeforeAfter(_addr_n.Methods, before, after);
            break;
        case ptr<ast.MapType> n:
            walkBeforeAfter(_addr_n.Key, before, after);
            walkBeforeAfter(_addr_n.Value, before, after);
            break;
        case ptr<ast.ChanType> n:
            walkBeforeAfter(_addr_n.Value, before, after);
            break;
        case ptr<ast.BadStmt> n:
            break;
        case ptr<ast.DeclStmt> n:
            walkBeforeAfter(_addr_n.Decl, before, after);
            break;
        case ptr<ast.EmptyStmt> n:
            break;
        case ptr<ast.LabeledStmt> n:
            walkBeforeAfter(_addr_n.Stmt, before, after);
            break;
        case ptr<ast.ExprStmt> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.SendStmt> n:
            walkBeforeAfter(_addr_n.Chan, before, after);
            walkBeforeAfter(_addr_n.Value, before, after);
            break;
        case ptr<ast.IncDecStmt> n:
            walkBeforeAfter(_addr_n.X, before, after);
            break;
        case ptr<ast.AssignStmt> n:
            walkBeforeAfter(_addr_n.Lhs, before, after);
            walkBeforeAfter(_addr_n.Rhs, before, after);
            break;
        case ptr<ast.GoStmt> n:
            walkBeforeAfter(_addr_n.Call, before, after);
            break;
        case ptr<ast.DeferStmt> n:
            walkBeforeAfter(_addr_n.Call, before, after);
            break;
        case ptr<ast.ReturnStmt> n:
            walkBeforeAfter(_addr_n.Results, before, after);
            break;
        case ptr<ast.BranchStmt> n:
            break;
        case ptr<ast.BlockStmt> n:
            walkBeforeAfter(_addr_n.List, before, after);
            break;
        case ptr<ast.IfStmt> n:
            walkBeforeAfter(_addr_n.Init, before, after);
            walkBeforeAfter(_addr_n.Cond, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            walkBeforeAfter(_addr_n.Else, before, after);
            break;
        case ptr<ast.CaseClause> n:
            walkBeforeAfter(_addr_n.List, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.SwitchStmt> n:
            walkBeforeAfter(_addr_n.Init, before, after);
            walkBeforeAfter(_addr_n.Tag, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.TypeSwitchStmt> n:
            walkBeforeAfter(_addr_n.Init, before, after);
            walkBeforeAfter(_addr_n.Assign, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.CommClause> n:
            walkBeforeAfter(_addr_n.Comm, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.SelectStmt> n:
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.ForStmt> n:
            walkBeforeAfter(_addr_n.Init, before, after);
            walkBeforeAfter(_addr_n.Cond, before, after);
            walkBeforeAfter(_addr_n.Post, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.RangeStmt> n:
            walkBeforeAfter(_addr_n.Key, before, after);
            walkBeforeAfter(_addr_n.Value, before, after);
            walkBeforeAfter(_addr_n.X, before, after);
            walkBeforeAfter(_addr_n.Body, before, after);
            break;
        case ptr<ast.ImportSpec> n:
            break;
        case ptr<ast.ValueSpec> n:
            walkBeforeAfter(_addr_n.Type, before, after);
            walkBeforeAfter(_addr_n.Values, before, after);
            walkBeforeAfter(_addr_n.Names, before, after);
            break;
        case ptr<ast.TypeSpec> n:
            walkBeforeAfter(_addr_n.Type, before, after);
            break;
        case ptr<ast.BadDecl> n:
            break;
        case ptr<ast.GenDecl> n:
            walkBeforeAfter(_addr_n.Specs, before, after);
            break;
        case ptr<ast.FuncDecl> n:
            if (n.Recv != null) {
                walkBeforeAfter(_addr_n.Recv, before, after);
            }
            walkBeforeAfter(_addr_n.Type, before, after);
            if (n.Body != null) {
                walkBeforeAfter(_addr_n.Body, before, after);
            }
            break;
        case ptr<ast.File> n:
            walkBeforeAfter(_addr_n.Decls, before, after);
            break;
        case ptr<ast.Package> n:
            walkBeforeAfter(_addr_n.Files, before, after);
            break;
        case slice<ptr<ast.File>> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        case slice<ast.Decl> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        case slice<ast.Expr> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        case slice<ptr<ast.Ident>> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        case slice<ast.Stmt> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        case slice<ast.Spec> n:
            {
                var i__prev1 = i;

                foreach (var (__i) in n) {
                    i = __i;
                    walkBeforeAfter(_addr_n[i], before, after);
                }

                i = i__prev1;
            }
            break;
        default:
        {
            var n = x.type();
            panic(fmt.Errorf("unexpected type %T in walkBeforeAfter", x));
            break;
        }
    }
    after(x);

});

// imports reports whether f imports path.
private static bool imports(ptr<ast.File> _addr_f, @string path) {
    ref ast.File f = ref _addr_f.val;

    return importSpec(_addr_f, path) != null;
}

// importSpec returns the import spec if f imports path,
// or nil otherwise.
private static ptr<ast.ImportSpec> importSpec(ptr<ast.File> _addr_f, @string path) {
    ref ast.File f = ref _addr_f.val;

    foreach (var (_, s) in f.Imports) {
        if (importPath(_addr_s) == path) {
            return _addr_s!;
        }
    }    return _addr_null!;

}

// importPath returns the unquoted import path of s,
// or "" if the path is not properly quoted.
private static @string importPath(ptr<ast.ImportSpec> _addr_s) {
    ref ast.ImportSpec s = ref _addr_s.val;

    var (t, err) = strconv.Unquote(s.Path.Value);
    if (err == null) {
        return t;
    }
    return "";

}

// declImports reports whether gen contains an import of path.
private static bool declImports(ptr<ast.GenDecl> _addr_gen, @string path) {
    ref ast.GenDecl gen = ref _addr_gen.val;

    if (gen.Tok != token.IMPORT) {
        return false;
    }
    foreach (var (_, spec) in gen.Specs) {
        ptr<ast.ImportSpec> impspec = spec._<ptr<ast.ImportSpec>>();
        if (importPath(impspec) == path) {
            return true;
        }
    }    return false;

}

// isTopName reports whether n is a top-level unresolved identifier with the given name.
private static bool isTopName(ast.Expr n, @string name) {
    ptr<ast.Ident> (id, ok) = n._<ptr<ast.Ident>>();
    return ok && id.Name == name && id.Obj == null;
}

// renameTop renames all references to the top-level name old.
// It reports whether it makes any changes.
private static bool renameTop(ptr<ast.File> _addr_f, @string old, @string @new) {
    ref ast.File f = ref _addr_f.val;

    bool @fixed = default; 

    // Rename any conflicting imports
    // (assuming package name is last element of path).
    {
        var s__prev1 = s;

        foreach (var (_, __s) in f.Imports) {
            s = __s;
            if (s.Name != null) {
                if (s.Name.Name == old) {
                    s.Name.Name = new;
                    fixed = true;
                }
            }
            else
 {
                var (_, thisName) = path.Split(importPath(_addr_s));
                if (thisName == old) {
                    s.Name = ast.NewIdent(new);
                    fixed = true;
                }
            }

        }
        s = s__prev1;
    }

    {
        var d__prev1 = d;

        foreach (var (_, __d) in f.Decls) {
            d = __d;
            switch (d.type()) {
                case ptr<ast.FuncDecl> d:
                    if (d.Recv == null && d.Name.Name == old) {
                        d.Name.Name = new;
                        d.Name.Obj.Name = new;
                        fixed = true;
                    }
                    break;
                case ptr<ast.GenDecl> d:
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in d.Specs) {
                            s = __s;
                            switch (s.type()) {
                                case ptr<ast.TypeSpec> s:
                                    if (s.Name.Name == old) {
                                        s.Name.Name = new;
                                        s.Name.Obj.Name = new;
                                        fixed = true;
                                    }
                                    break;
                                case ptr<ast.ValueSpec> s:
                                    foreach (var (_, n) in s.Names) {
                                        if (n.Name == old) {
                                            n.Name = new;
                                            n.Obj.Name = new;
                                            fixed = true;
                                        }
                                    }
                                    break;
                            }

                        }

                        s = s__prev2;
                    }
                    break;
            }

        }
        d = d__prev1;
    }

    walk(f, n => {
        ptr<ast.Ident> (id, ok) = n._<ptr<ast.Ident>>();
        if (ok && isTopName(id, old)) {
            id.Name = new;
            fixed = true;
        }
        if (ok && id.Obj != null && id.Name == old && id.Obj.Name == new) {
            id.Name = id.Obj.Name;
            fixed = true;
        }
    });

    return fixed;

}

// matchLen returns the length of the longest prefix shared by x and y.
private static nint matchLen(@string x, @string y) {
    nint i = 0;
    while (i < len(x) && i < len(y) && x[i] == y[i]) {
        i++;
    }
    return i;
}

// addImport adds the import path to the file f, if absent.
private static bool addImport(ptr<ast.File> _addr_f, @string ipath) {
    bool added = default;
    ref ast.File f = ref _addr_f.val;

    if (imports(_addr_f, ipath)) {
        return false;
    }
    var (_, name) = path.Split(ipath); 

    // Rename any conflicting top-level references from name to name_.
    renameTop(_addr_f, name, name + "_");

    ptr<ast.ImportSpec> newImport = addr(new ast.ImportSpec(Path:&ast.BasicLit{Kind:token.STRING,Value:strconv.Quote(ipath),},)); 

    // Find an import decl to add to.
    nint bestMatch = -1;    nint lastImport = -1;    ptr<ast.GenDecl> impDecl;    nint impIndex = -1;
    foreach (var (i, decl) in f.Decls) {
        ptr<ast.GenDecl> (gen, ok) = decl._<ptr<ast.GenDecl>>();
        if (ok && gen.Tok == token.IMPORT) {
            lastImport = i; 
            // Do not add to import "C", to avoid disrupting the
            // association with its doc comment, breaking cgo.
            if (declImports(gen, "C")) {
                continue;
            } 

            // Compute longest shared prefix with imports in this block.
            foreach (var (j, spec) in gen.Specs) {
                ptr<ast.ImportSpec> impspec = spec._<ptr<ast.ImportSpec>>();
                var n = matchLen(importPath(impspec), ipath);
                if (n > bestMatch) {
                    bestMatch = n;
                    impDecl = gen;
                    impIndex = j;
                }
            }

        }
    }    if (impDecl == null) {
        impDecl = addr(new ast.GenDecl(Tok:token.IMPORT,));
        f.Decls = append(f.Decls, null);
        copy(f.Decls[(int)lastImport + 2..], f.Decls[(int)lastImport + 1..]);
        f.Decls[lastImport + 1] = impDecl;
    }
    if (len(impDecl.Specs) > 0 && !impDecl.Lparen.IsValid()) {
        impDecl.Lparen = impDecl.Pos();
    }
    var insertAt = impIndex + 1;
    if (insertAt == 0) {
        insertAt = len(impDecl.Specs);
    }
    impDecl.Specs = append(impDecl.Specs, null);
    copy(impDecl.Specs[(int)insertAt + 1..], impDecl.Specs[(int)insertAt..]);
    impDecl.Specs[insertAt] = newImport;
    if (insertAt > 0) { 
        // Assign same position as the previous import,
        // so that the sorter sees it as being in the same block.
        var prev = impDecl.Specs[insertAt - 1];
        newImport.Path.ValuePos = prev.Pos();
        newImport.EndPos = prev.Pos();

    }
    f.Imports = append(f.Imports, newImport);
    return true;

}

// deleteImport deletes the import path from the file f, if present.
private static bool deleteImport(ptr<ast.File> _addr_f, @string path) {
    bool deleted = default;
    ref ast.File f = ref _addr_f.val;

    var oldImport = importSpec(_addr_f, path); 

    // Find the import node that imports path, if any.
    {
        var i__prev1 = i;

        foreach (var (__i, __decl) in f.Decls) {
            i = __i;
            decl = __decl;
            ptr<ast.GenDecl> (gen, ok) = decl._<ptr<ast.GenDecl>>();
            if (!ok || gen.Tok != token.IMPORT) {
                continue;
            }
            foreach (var (j, spec) in gen.Specs) {
                ptr<ast.ImportSpec> impspec = spec._<ptr<ast.ImportSpec>>();
                if (oldImport != impspec) {
                    continue;
                } 

                // We found an import spec that imports path.
                // Delete it.
                deleted = true;
                copy(gen.Specs[(int)j..], gen.Specs[(int)j + 1..]);
                gen.Specs = gen.Specs[..(int)len(gen.Specs) - 1]; 

                // If this was the last import spec in this decl,
                // delete the decl, too.
                if (len(gen.Specs) == 0) {
                    copy(f.Decls[(int)i..], f.Decls[(int)i + 1..]);
                    f.Decls = f.Decls[..(int)len(f.Decls) - 1];
                }
                else if (len(gen.Specs) == 1) {
                    gen.Lparen = token.NoPos; // drop parens
                }

                if (j > 0) { 
                    // We deleted an entry but now there will be
                    // a blank line-sized hole where the import was.
                    // Close the hole by making the previous
                    // import appear to "end" where this one did.
                    gen.Specs[j - 1]._<ptr<ast.ImportSpec>>().EndPos = impspec.End();

                }

                break;

            }

        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __imp) in f.Imports) {
            i = __i;
            imp = __imp;
            if (imp == oldImport) {
                copy(f.Imports[(int)i..], f.Imports[(int)i + 1..]);
                f.Imports = f.Imports[..(int)len(f.Imports) - 1];
                break;
            }
        }
        i = i__prev1;
    }

    return ;

}

// rewriteImport rewrites any import of path oldPath to path newPath.
private static bool rewriteImport(ptr<ast.File> _addr_f, @string oldPath, @string newPath) {
    bool rewrote = default;
    ref ast.File f = ref _addr_f.val;

    foreach (var (_, imp) in f.Imports) {
        if (importPath(_addr_imp) == oldPath) {
            rewrote = true; 
            // record old End, because the default is to compute
            // it using the length of imp.Path.Value.
            imp.EndPos = imp.End();
            imp.Path.Value = strconv.Quote(newPath);

        }
    }    return ;

}

} // end main_package
