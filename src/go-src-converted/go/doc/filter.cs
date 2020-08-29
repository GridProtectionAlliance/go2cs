// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package doc -- go2cs converted at 2020 August 29 08:47:06 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Go\src\go\doc\filter.go
using ast = go.go.ast_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class doc_package
    {
        public delegate  bool Filter(@string);

        private static bool matchFields(ref ast.FieldList fields, Filter f)
        {
            if (fields != null)
            {
                foreach (var (_, field) in fields.List)
                {
                    foreach (var (_, name) in field.Names)
                    {
                        if (f(name.Name))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool matchDecl(ref ast.GenDecl d, Filter f)
        {
            foreach (var (_, d) in d.Specs)
            {
                switch (d.type())
                {
                    case ref ast.ValueSpec v:
                        foreach (var (_, name) in v.Names)
                        {
                            if (f(name.Name))
                            {
                                return true;
                            }
                        }
                        break;
                    case ref ast.TypeSpec v:
                        if (f(v.Name.Name))
                        {
                            return true;
                        }
                        switch (v.Type.type())
                        {
                            case ref ast.StructType t:
                                if (matchFields(t.Fields, f))
                                {
                                    return true;
                                }
                                break;
                            case ref ast.InterfaceType t:
                                if (matchFields(t.Methods, f))
                                {
                                    return true;
                                }
                                break;
                        }
                        break;
                }
            }
            return false;
        }

        private static slice<ref Value> filterValues(slice<ref Value> a, Filter f)
        {
            long w = 0L;
            foreach (var (_, vd) in a)
            {
                if (matchDecl(vd.Decl, f))
                {
                    a[w] = vd;
                    w++;
                }
            }
            return a[0L..w];
        }

        private static slice<ref Func> filterFuncs(slice<ref Func> a, Filter f)
        {
            long w = 0L;
            foreach (var (_, fd) in a)
            {
                if (f(fd.Name))
                {
                    a[w] = fd;
                    w++;
                }
            }
            return a[0L..w];
        }

        private static slice<ref Type> filterTypes(slice<ref Type> a, Filter f)
        {
            long w = 0L;
            foreach (var (_, td) in a)
            {
                long n = 0L; // number of matches
                if (matchDecl(td.Decl, f))
                {
                    n = 1L;
                }
                else
                { 
                    // type name doesn't match, but we may have matching consts, vars, factories or methods
                    td.Consts = filterValues(td.Consts, f);
                    td.Vars = filterValues(td.Vars, f);
                    td.Funcs = filterFuncs(td.Funcs, f);
                    td.Methods = filterFuncs(td.Methods, f);
                    n += len(td.Consts) + len(td.Vars) + len(td.Funcs) + len(td.Methods);
                }
                if (n > 0L)
                {
                    a[w] = td;
                    w++;
                }
            }
            return a[0L..w];
        }

        // Filter eliminates documentation for names that don't pass through the filter f.
        // TODO(gri): Recognize "Type.Method" as a name.
        //
        private static void Filter(this ref Package p, Filter f)
        {
            p.Consts = filterValues(p.Consts, f);
            p.Vars = filterValues(p.Vars, f);
            p.Types = filterTypes(p.Types, f);
            p.Funcs = filterFuncs(p.Funcs, f);
            p.Doc = ""; // don't show top-level package doc
        }
    }
}}
