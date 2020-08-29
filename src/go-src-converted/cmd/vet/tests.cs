// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:09:30 UTC
// Original source: C:\Go\src\cmd\vet\tests.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("tests", "check for common mistaken usages of tests/documentation examples", checkTestFunctions, funcDecl);
        }

        private static bool isExampleSuffix(@string s)
        {
            var (r, size) = utf8.DecodeRuneInString(s);
            return size > 0L && unicode.IsLower(r);
        }

        private static bool isTestSuffix(@string name)
        {
            if (len(name) == 0L)
            { 
                // "Test" is ok.
                return true;
            }
            var (r, _) = utf8.DecodeRuneInString(name);
            return !unicode.IsLower(r);
        }

        private static bool isTestParam(ast.Expr typ, @string wantType)
        {
            ref ast.StarExpr (ptr, ok) = typ._<ref ast.StarExpr>();
            if (!ok)
            { 
                // Not a pointer.
                return false;
            } 
            // No easy way of making sure it's a *testing.T or *testing.B:
            // ensure the name of the type matches.
            {
                ref ast.Ident (name, ok) = ptr.X._<ref ast.Ident>();

                if (ok)
                {
                    return name.Name == wantType;
                }

            }
            {
                ref ast.SelectorExpr (sel, ok) = ptr.X._<ref ast.SelectorExpr>();

                if (ok)
                {
                    return sel.Sel.Name == wantType;
                }

            }
            return false;
        }

        private static types.Object lookup(@string name, slice<ref types.Scope> scopes)
        {
            foreach (var (_, scope) in scopes)
            {
                {
                    var o = scope.Lookup(name);

                    if (o != null)
                    {
                        return o;
                    }

                }
            }
            return null;
        }

        private static slice<ref types.Scope> extendedScope(ref File f)
        {
            ref types.Scope scopes = new slice<ref types.Scope>(new ref types.Scope[] { f.pkg.typesPkg.Scope() });
            if (f.basePkg != null)
            {
                scopes = append(scopes, f.basePkg.typesPkg.Scope());
            }
            else
            { 
                // If basePkg is not specified (e.g. when checking a single file) try to
                // find it among imports.
                var pkgName = f.pkg.typesPkg.Name();
                if (strings.HasSuffix(pkgName, "_test"))
                {
                    var basePkgName = strings.TrimSuffix(pkgName, "_test");
                    foreach (var (_, p) in f.pkg.typesPkg.Imports())
                    {
                        if (p.Name() == basePkgName)
                        {
                            scopes = append(scopes, p.Scope());
                            break;
                        }
                    }
                }
            }
            return scopes;
        }

        private static void checkExample(ref ast.FuncDecl fn, ref File f, reporter report)
        {
            var fnName = fn.Name.Name;
            {
                var @params = fn.Type.Params;

                if (len(@params.List) != 0L)
                {
                    report("%s should be niladic", fnName);
                }

            }
            {
                var results = fn.Type.Results;

                if (results != null && len(results.List) != 0L)
                {
                    report("%s should return nothing", fnName);
                }

            }

            if (filesRun && !includesNonTest)
            { 
                // The coherence checks between a test and the package it tests
                // will report false positives if no non-test files have
                // been provided.
                return;
            }
            if (fnName == "Example")
            { 
                // Nothing more to do.
                return;
            }
            var exName = strings.TrimPrefix(fnName, "Example");            var elems = strings.SplitN(exName, "_", 3L);            var ident = elems[0L];            var obj = lookup(ident, extendedScope(f));
            if (ident != "" && obj == null)
            { 
                // Check ExampleFoo and ExampleBadFoo.
                report("%s refers to unknown identifier: %s", fnName, ident); 
                // Abort since obj is absent and no subsequent checks can be performed.
                return;
            }
            if (len(elems) < 2L)
            { 
                // Nothing more to do.
                return;
            }
            if (ident == "")
            { 
                // Check Example_suffix and Example_BadSuffix.
                {
                    var residual = strings.TrimPrefix(exName, "_");

                    if (!isExampleSuffix(residual))
                    {
                        report("%s has malformed example suffix: %s", fnName, residual);
                    }

                }
                return;
            }
            var mmbr = elems[1L];
            if (!isExampleSuffix(mmbr))
            { 
                // Check ExampleFoo_Method and ExampleFoo_BadMethod.
                {
                    var obj__prev2 = obj;

                    var (obj, _, _) = types.LookupFieldOrMethod(obj.Type(), true, obj.Pkg(), mmbr);

                    if (obj == null)
                    {
                        report("%s refers to unknown field or method: %s.%s", fnName, ident, mmbr);
                    }

                    obj = obj__prev2;

                }
            }
            if (len(elems) == 3L && !isExampleSuffix(elems[2L]))
            { 
                // Check ExampleFoo_Method_suffix and ExampleFoo_Method_Badsuffix.
                report("%s has malformed example suffix: %s", fnName, elems[2L]);
            }
        }

        private static void checkTest(ref ast.FuncDecl fn, @string prefix, reporter report)
        { 
            // Want functions with 0 results and 1 parameter.
            if (fn.Type.Results != null && len(fn.Type.Results.List) > 0L || fn.Type.Params == null || len(fn.Type.Params.List) != 1L || len(fn.Type.Params.List[0L].Names) > 1L)
            {
                return;
            } 

            // The param must look like a *testing.T or *testing.B.
            if (!isTestParam(fn.Type.Params.List[0L].Type, prefix[..1L]))
            {
                return;
            }
            if (!isTestSuffix(fn.Name.Name[len(prefix)..]))
            {
                report("%s has malformed name: first letter after '%s' must not be lowercase", fn.Name.Name, prefix);
            }
        }

        public delegate void reporter(@string, object[]);

        // checkTestFunctions walks Test, Benchmark and Example functions checking
        // malformed names, wrong signatures and examples documenting nonexistent
        // identifiers.
        private static void checkTestFunctions(ref File f, ast.Node node)
        {
            if (!strings.HasSuffix(f.name, "_test.go"))
            {
                return;
            }
            ref ast.FuncDecl (fn, ok) = node._<ref ast.FuncDecl>();
            if (!ok || fn.Recv != null)
            { 
                // Ignore non-functions or functions with receivers.
                return;
            }
            Action<@string, object[]> report = (format, args) =>
            {
                f.Badf(node.Pos(), format, args);

            }
;


            if (strings.HasPrefix(fn.Name.Name, "Example")) 
                checkExample(fn, f, report);
            else if (strings.HasPrefix(fn.Name.Name, "Test")) 
                checkTest(fn, "Test", report);
            else if (strings.HasPrefix(fn.Name.Name, "Benchmark")) 
                checkTest(fn, "Benchmark", report);
                    }
    }
}
