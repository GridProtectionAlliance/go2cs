// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2020 October 08 04:04:20 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\filter.go
using token = go.go.token_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // ----------------------------------------------------------------------------
        // Export filtering

        // exportFilter is a special filter function to extract exported nodes.
        private static bool exportFilter(@string name)
        {
            return IsExported(name);
        }

        // FileExports trims the AST for a Go source file in place such that
        // only exported nodes remain: all top-level identifiers which are not exported
        // and their associated information (such as type, initial value, or function
        // body) are removed. Non-exported fields and methods of exported types are
        // stripped. The File.Comments list is not changed.
        //
        // FileExports reports whether there are exported declarations.
        //
        public static bool FileExports(ptr<File> _addr_src)
        {
            ref File src = ref _addr_src.val;

            return filterFile(_addr_src, exportFilter, true);
        }

        // PackageExports trims the AST for a Go package in place such that
        // only exported nodes remain. The pkg.Files list is not changed, so that
        // file names and top-level package comments don't get lost.
        //
        // PackageExports reports whether there are exported declarations;
        // it returns false otherwise.
        //
        public static bool PackageExports(ptr<Package> _addr_pkg)
        {
            ref Package pkg = ref _addr_pkg.val;

            return filterPackage(_addr_pkg, exportFilter, true);
        }

        // ----------------------------------------------------------------------------
        // General filtering

        public delegate  bool Filter(@string);

        private static slice<ptr<Ident>> filterIdentList(slice<ptr<Ident>> list, Filter f)
        {
            long j = 0L;
            foreach (var (_, x) in list)
            {
                if (f(x.Name))
                {
                    list[j] = x;
                    j++;
                }

            }
            return list[0L..j];

        }

        // fieldName assumes that x is the type of an anonymous field and
        // returns the corresponding field name. If x is not an acceptable
        // anonymous field, the result is nil.
        //
        private static ptr<Ident> fieldName(Expr x)
        {
            switch (x.type())
            {
                case ptr<Ident> t:
                    return _addr_t!;
                    break;
                case ptr<SelectorExpr> t:
                    {
                        ptr<Ident> (_, ok) = t.X._<ptr<Ident>>();

                        if (ok)
                        {
                            return _addr_t.Sel!;
                        }

                    }

                    break;
                case ptr<StarExpr> t:
                    return _addr_fieldName(t.X)!;
                    break;
            }
            return _addr_null!;

        }

        private static bool filterFieldList(ptr<FieldList> _addr_fields, Filter filter, bool export)
        {
            bool removedFields = default;
            ref FieldList fields = ref _addr_fields.val;

            if (fields == null)
            {
                return false;
            }

            var list = fields.List;
            long j = 0L;
            foreach (var (_, f) in list)
            {
                var keepField = false;
                if (len(f.Names) == 0L)
                { 
                    // anonymous field
                    var name = fieldName(f.Type);
                    keepField = name != null && filter(name.Name);

                }
                else
                {
                    var n = len(f.Names);
                    f.Names = filterIdentList(f.Names, filter);
                    if (len(f.Names) < n)
                    {
                        removedFields = true;
                    }

                    keepField = len(f.Names) > 0L;

                }

                if (keepField)
                {
                    if (export)
                    {
                        filterType(f.Type, filter, export);
                    }

                    list[j] = f;
                    j++;

                }

            }
            if (j < len(list))
            {
                removedFields = true;
            }

            fields.List = list[0L..j];
            return ;

        }

        private static void filterCompositeLit(ptr<CompositeLit> _addr_lit, Filter filter, bool export)
        {
            ref CompositeLit lit = ref _addr_lit.val;

            var n = len(lit.Elts);
            lit.Elts = filterExprList(lit.Elts, filter, export);
            if (len(lit.Elts) < n)
            {
                lit.Incomplete = true;
            }

        }

        private static slice<Expr> filterExprList(slice<Expr> list, Filter filter, bool export)
        {
            long j = 0L;
            foreach (var (_, exp) in list)
            {
                switch (exp.type())
                {
                    case ptr<CompositeLit> x:
                        filterCompositeLit(_addr_x, filter, export);
                        break;
                    case ptr<KeyValueExpr> x:
                        {
                            var x__prev1 = x;

                            ptr<Ident> (x, ok) = x.Key._<ptr<Ident>>();

                            if (ok && !filter(x.Name))
                            {
                                continue;
                            }

                            x = x__prev1;

                        }

                        {
                            var x__prev1 = x;

                            (x, ok) = x.Value._<ptr<CompositeLit>>();

                            if (ok)
                            {
                                filterCompositeLit(_addr_x, filter, export);
                            }

                            x = x__prev1;

                        }

                        break;
                }
                list[j] = exp;
                j++;

            }
            return list[0L..j];

        }

        private static bool filterParamList(ptr<FieldList> _addr_fields, Filter filter, bool export)
        {
            ref FieldList fields = ref _addr_fields.val;

            if (fields == null)
            {
                return false;
            }

            bool b = default;
            foreach (var (_, f) in fields.List)
            {
                if (filterType(f.Type, filter, export))
                {
                    b = true;
                }

            }
            return b;

        }

        private static bool filterType(Expr typ, Filter f, bool export)
        {
            switch (typ.type())
            {
                case ptr<Ident> t:
                    return f(t.Name);
                    break;
                case ptr<ParenExpr> t:
                    return filterType(t.X, f, export);
                    break;
                case ptr<ArrayType> t:
                    return filterType(t.Elt, f, export);
                    break;
                case ptr<StructType> t:
                    if (filterFieldList(_addr_t.Fields, f, export))
                    {
                        t.Incomplete = true;
                    }

                    return len(t.Fields.List) > 0L;
                    break;
                case ptr<FuncType> t:
                    var b1 = filterParamList(_addr_t.Params, f, export);
                    var b2 = filterParamList(_addr_t.Results, f, export);
                    return b1 || b2;
                    break;
                case ptr<InterfaceType> t:
                    if (filterFieldList(_addr_t.Methods, f, export))
                    {
                        t.Incomplete = true;
                    }

                    return len(t.Methods.List) > 0L;
                    break;
                case ptr<MapType> t:
                    b1 = filterType(t.Key, f, export);
                    b2 = filterType(t.Value, f, export);
                    return b1 || b2;
                    break;
                case ptr<ChanType> t:
                    return filterType(t.Value, f, export);
                    break;
            }
            return false;

        }

        private static bool filterSpec(Spec spec, Filter f, bool export)
        {
            switch (spec.type())
            {
                case ptr<ValueSpec> s:
                    s.Names = filterIdentList(s.Names, f);
                    s.Values = filterExprList(s.Values, f, export);
                    if (len(s.Names) > 0L)
                    {
                        if (export)
                        {
                            filterType(s.Type, f, export);
                        }

                        return true;

                    }

                    break;
                case ptr<TypeSpec> s:
                    if (f(s.Name.Name))
                    {
                        if (export)
                        {
                            filterType(s.Type, f, export);
                        }

                        return true;

                    }

                    if (!export)
                    { 
                        // For general filtering (not just exports),
                        // filter type even if name is not filtered
                        // out.
                        // If the type contains filtered elements,
                        // keep the declaration.
                        return filterType(s.Type, f, export);

                    }

                    break;
            }
            return false;

        }

        private static slice<Spec> filterSpecList(slice<Spec> list, Filter f, bool export)
        {
            long j = 0L;
            foreach (var (_, s) in list)
            {
                if (filterSpec(s, f, export))
                {
                    list[j] = s;
                    j++;
                }

            }
            return list[0L..j];

        }

        // FilterDecl trims the AST for a Go declaration in place by removing
        // all names (including struct field and interface method names, but
        // not from parameter lists) that don't pass through the filter f.
        //
        // FilterDecl reports whether there are any declared names left after
        // filtering.
        //
        public static bool FilterDecl(Decl decl, Filter f)
        {
            return filterDecl(decl, f, false);
        }

        private static bool filterDecl(Decl decl, Filter f, bool export)
        {
            switch (decl.type())
            {
                case ptr<GenDecl> d:
                    d.Specs = filterSpecList(d.Specs, f, export);
                    return len(d.Specs) > 0L;
                    break;
                case ptr<FuncDecl> d:
                    return f(d.Name.Name);
                    break;
            }
            return false;

        }

        // FilterFile trims the AST for a Go file in place by removing all
        // names from top-level declarations (including struct field and
        // interface method names, but not from parameter lists) that don't
        // pass through the filter f. If the declaration is empty afterwards,
        // the declaration is removed from the AST. Import declarations are
        // always removed. The File.Comments list is not changed.
        //
        // FilterFile reports whether there are any top-level declarations
        // left after filtering.
        //
        public static bool FilterFile(ptr<File> _addr_src, Filter f)
        {
            ref File src = ref _addr_src.val;

            return filterFile(_addr_src, f, false);
        }

        private static bool filterFile(ptr<File> _addr_src, Filter f, bool export)
        {
            ref File src = ref _addr_src.val;

            long j = 0L;
            foreach (var (_, d) in src.Decls)
            {
                if (filterDecl(d, f, export))
                {
                    src.Decls[j] = d;
                    j++;
                }

            }
            src.Decls = src.Decls[0L..j];
            return j > 0L;

        }

        // FilterPackage trims the AST for a Go package in place by removing
        // all names from top-level declarations (including struct field and
        // interface method names, but not from parameter lists) that don't
        // pass through the filter f. If the declaration is empty afterwards,
        // the declaration is removed from the AST. The pkg.Files list is not
        // changed, so that file names and top-level package comments don't get
        // lost.
        //
        // FilterPackage reports whether there are any top-level declarations
        // left after filtering.
        //
        public static bool FilterPackage(ptr<Package> _addr_pkg, Filter f)
        {
            ref Package pkg = ref _addr_pkg.val;

            return filterPackage(_addr_pkg, f, false);
        }

        private static bool filterPackage(ptr<Package> _addr_pkg, Filter f, bool export)
        {
            ref Package pkg = ref _addr_pkg.val;

            var hasDecls = false;
            foreach (var (_, src) in pkg.Files)
            {
                if (filterFile(_addr_src, f, export))
                {
                    hasDecls = true;
                }

            }
            return hasDecls;

        }

        // ----------------------------------------------------------------------------
        // Merging of package files

        // The MergeMode flags control the behavior of MergePackageFiles.
        public partial struct MergeMode // : ulong
        {
        }

 
        // If set, duplicate function declarations are excluded.
        public static readonly MergeMode FilterFuncDuplicates = (MergeMode)1L << (int)(iota); 
        // If set, comments that are not associated with a specific
        // AST node (as Doc or Comment) are excluded.
        public static readonly var FilterUnassociatedComments = (var)0; 
        // If set, duplicate import declarations are excluded.
        public static readonly var FilterImportDuplicates = (var)1;


        // nameOf returns the function (foo) or method name (foo.bar) for
        // the given function declaration. If the AST is incorrect for the
        // receiver, it assumes a function instead.
        //
        private static @string nameOf(ptr<FuncDecl> _addr_f)
        {
            ref FuncDecl f = ref _addr_f.val;

            {
                var r = f.Recv;

                if (r != null && len(r.List) == 1L)
                { 
                    // looks like a correct receiver declaration
                    var t = r.List[0L].Type; 
                    // dereference pointer receiver types
                    {
                        ptr<StarExpr> p__prev2 = p;

                        ptr<StarExpr> (p, _) = t._<ptr<StarExpr>>();

                        if (p != null)
                        {
                            t = p.X;
                        } 
                        // the receiver type must be a type name

                        p = p__prev2;

                    } 
                    // the receiver type must be a type name
                    {
                        ptr<StarExpr> p__prev2 = p;

                        (p, _) = t._<ptr<Ident>>();

                        if (p != null)
                        {
                            return p.Name + "." + f.Name.Name;
                        } 
                        // otherwise assume a function instead

                        p = p__prev2;

                    } 
                    // otherwise assume a function instead
                }

            }

            return f.Name.Name;

        }

        // separator is an empty //-style comment that is interspersed between
        // different comment groups when they are concatenated into a single group
        //
        private static ptr<Comment> separator = addr(new Comment(token.NoPos,"//"));

        // MergePackageFiles creates a file AST by merging the ASTs of the
        // files belonging to a package. The mode flags control merging behavior.
        //
        public static ptr<File> MergePackageFiles(ptr<Package> _addr_pkg, MergeMode mode)
        {
            ref Package pkg = ref _addr_pkg.val;
 
            // Count the number of package docs, comments and declarations across
            // all package files. Also, compute sorted list of filenames, so that
            // subsequent iterations can always iterate in the same order.
            long ndocs = 0L;
            long ncomments = 0L;
            long ndecls = 0L;
            var filenames = make_slice<@string>(len(pkg.Files));
            long i = 0L;
            {
                var filename__prev1 = filename;
                var f__prev1 = f;

                foreach (var (__filename, __f) in pkg.Files)
                {
                    filename = __filename;
                    f = __f;
                    filenames[i] = filename;
                    i++;
                    if (f.Doc != null)
                    {
                        ndocs += len(f.Doc.List) + 1L; // +1 for separator
                    }

                    ncomments += len(f.Comments);
                    ndecls += len(f.Decls);

                }

                filename = filename__prev1;
                f = f__prev1;
            }

            sort.Strings(filenames); 

            // Collect package comments from all package files into a single
            // CommentGroup - the collected package documentation. In general
            // there should be only one file with a package comment; but it's
            // better to collect extra comments than drop them on the floor.
            ptr<CommentGroup> doc;
            token.Pos pos = default;
            if (ndocs > 0L)
            {
                var list = make_slice<ptr<Comment>>(ndocs - 1L); // -1: no separator before first group
                i = 0L;
                {
                    var filename__prev1 = filename;

                    foreach (var (_, __filename) in filenames)
                    {
                        filename = __filename;
                        var f = pkg.Files[filename];
                        if (f.Doc != null)
                        {
                            if (i > 0L)
                            { 
                                // not the first group - add separator
                                list[i] = separator;
                                i++;

                            }

                            foreach (var (_, c) in f.Doc.List)
                            {
                                list[i] = c;
                                i++;
                            }
                            if (f.Package > pos)
                            { 
                                // Keep the maximum package clause position as
                                // position for the package clause of the merged
                                // files.
                                pos = f.Package;

                            }

                        }

                    }

                    filename = filename__prev1;
                }

                doc = addr(new CommentGroup(list));

            } 

            // Collect declarations from all package files.
            slice<Decl> decls = default;
            if (ndecls > 0L)
            {
                decls = make_slice<Decl>(ndecls);
                var funcs = make_map<@string, long>(); // map of func name -> decls index
                i = 0L; // current index
                long n = 0L; // number of filtered entries
                {
                    var filename__prev1 = filename;

                    foreach (var (_, __filename) in filenames)
                    {
                        filename = __filename;
                        f = pkg.Files[filename];
                        {
                            var d__prev2 = d;

                            foreach (var (_, __d) in f.Decls)
                            {
                                d = __d;
                                if (mode & FilterFuncDuplicates != 0L)
                                { 
                                    // A language entity may be declared multiple
                                    // times in different package files; only at
                                    // build time declarations must be unique.
                                    // For now, exclude multiple declarations of
                                    // functions - keep the one with documentation.
                                    //
                                    // TODO(gri): Expand this filtering to other
                                    //            entities (const, type, vars) if
                                    //            multiple declarations are common.
                                    {
                                        var f__prev3 = f;

                                        ptr<FuncDecl> (f, isFun) = d._<ptr<FuncDecl>>();

                                        if (isFun)
                                        {
                                            var name = nameOf(_addr_f);
                                            {
                                                var (j, exists) = funcs[name];

                                                if (exists)
                                                { 
                                                    // function declared already
                                                    if (decls[j] != null && decls[j]._<ptr<FuncDecl>>().Doc == null)
                                                    { 
                                                        // existing declaration has no documentation;
                                                        // ignore the existing declaration
                                                        decls[j] = null;

                                                    }
                                                    else
                                                    { 
                                                        // ignore the new declaration
                                                        d = null;

                                                    }

                                                    n++; // filtered an entry
                                                }
                                                else
                                                {
                                                    funcs[name] = i;
                                                }

                                            }

                                        }

                                        f = f__prev3;

                                    }

                                }

                                decls[i] = d;
                                i++;

                            }

                            d = d__prev2;
                        }
                    } 

                    // Eliminate nil entries from the decls list if entries were
                    // filtered. We do this using a 2nd pass in order to not disturb
                    // the original declaration order in the source (otherwise, this
                    // would also invalidate the monotonically increasing position
                    // info within a single file).

                    filename = filename__prev1;
                }

                if (n > 0L)
                {
                    i = 0L;
                    {
                        var d__prev1 = d;

                        foreach (var (_, __d) in decls)
                        {
                            d = __d;
                            if (d != null)
                            {
                                decls[i] = d;
                                i++;
                            }

                        }

                        d = d__prev1;
                    }

                    decls = decls[0L..i];

                }

            } 

            // Collect import specs from all package files.
            slice<ptr<ImportSpec>> imports = default;
            if (mode & FilterImportDuplicates != 0L)
            {
                var seen = make_map<@string, bool>();
                {
                    var filename__prev1 = filename;

                    foreach (var (_, __filename) in filenames)
                    {
                        filename = __filename;
                        f = pkg.Files[filename];
                        foreach (var (_, imp) in f.Imports)
                        {
                            {
                                var path = imp.Path.Value;

                                if (!seen[path])
                                { 
                                    // TODO: consider handling cases where:
                                    // - 2 imports exist with the same import path but
                                    //   have different local names (one should probably
                                    //   keep both of them)
                                    // - 2 imports exist but only one has a comment
                                    // - 2 imports exist and they both have (possibly
                                    //   different) comments
                                    imports = append(imports, imp);
                                    seen[path] = true;

                                }

                            }

                        }
            else
                    }

                    filename = filename__prev1;
                }
            }            { 
                // Iterate over filenames for deterministic order.
                {
                    var filename__prev1 = filename;

                    foreach (var (_, __filename) in filenames)
                    {
                        filename = __filename;
                        f = pkg.Files[filename];
                        imports = append(imports, f.Imports);
                    }

                    filename = filename__prev1;
                }
            } 

            // Collect comments from all package files.
            slice<ptr<CommentGroup>> comments = default;
            if (mode & FilterUnassociatedComments == 0L)
            {
                comments = make_slice<ptr<CommentGroup>>(ncomments);
                i = 0L;
                {
                    var filename__prev1 = filename;

                    foreach (var (_, __filename) in filenames)
                    {
                        filename = __filename;
                        f = pkg.Files[filename];
                        i += copy(comments[i..], f.Comments);
                    }

                    filename = filename__prev1;
                }
            } 

            // TODO(gri) need to compute unresolved identifiers!
            return addr(new File(doc,pos,NewIdent(pkg.Name),decls,pkg.Scope,imports,nil,comments));

        }
    }
}}
