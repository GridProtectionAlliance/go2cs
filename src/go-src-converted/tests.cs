// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tests defines an Analyzer that checks for common mistaken
// usages of tests and examples.
// package tests -- go2cs converted at 2022 March 06 23:34:16 UTC
// import "golang.org/x/tools/go/analysis/passes/tests" ==> using tests = go.golang.org.x.tools.go.analysis.passes.tests_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\tests\tests.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using analysis = go.golang.org.x.tools.go.analysis_package;

namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class tests_package {

public static readonly @string Doc = @"check for common mistaken usages of tests and examples

The tests checker walks Test, Benchmark and Example functions checking
malformed names, wrong signatures and examples documenting non-existent
identifiers.

Please see the documentation for package testing in golang.org/pkg/testing
for the conventions that are enforced for Tests, Benchmarks, and Examples.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"tests",Doc:Doc,Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    foreach (var (_, f) in pass.Files) {
        if (!strings.HasSuffix(pass.Fset.File(f.Pos()).Name(), "_test.go")) {
            continue;
        }
        foreach (var (_, decl) in f.Decls) {
            ptr<ast.FuncDecl> (fn, ok) = decl._<ptr<ast.FuncDecl>>();
            if (!ok || fn.Recv != null) { 
                // Ignore non-functions or functions with receivers.
                continue;

            }


            if (strings.HasPrefix(fn.Name.Name, "Example")) 
                checkExample(_addr_pass, fn);
            else if (strings.HasPrefix(fn.Name.Name, "Test")) 
                checkTest(_addr_pass, fn, "Test");
            else if (strings.HasPrefix(fn.Name.Name, "Benchmark")) 
                checkTest(_addr_pass, fn, "Benchmark");
            
        }
    }    return (null, error.As(null!)!);

}

private static bool isExampleSuffix(@string s) {
    var (r, size) = utf8.DecodeRuneInString(s);
    return size > 0 && unicode.IsLower(r);
}

private static bool isTestSuffix(@string name) {
    if (len(name) == 0) { 
        // "Test" is ok.
        return true;

    }
    var (r, _) = utf8.DecodeRuneInString(name);
    return !unicode.IsLower(r);

}

private static bool isTestParam(ast.Expr typ, @string wantType) {
    ptr<ast.StarExpr> (ptr, ok) = typ._<ptr<ast.StarExpr>>();
    if (!ok) { 
        // Not a pointer.
        return false;

    }
    {
        ptr<ast.Ident> (name, ok) = ptr.X._<ptr<ast.Ident>>();

        if (ok) {
            return name.Name == wantType;
        }
    }

    {
        ptr<ast.SelectorExpr> (sel, ok) = ptr.X._<ptr<ast.SelectorExpr>>();

        if (ok) {
            return sel.Sel.Name == wantType;
        }
    }

    return false;

}

private static slice<types.Object> lookup(ptr<types.Package> _addr_pkg, @string name) {
    ref types.Package pkg = ref _addr_pkg.val;

    {
        var o = pkg.Scope().Lookup(name);

        if (o != null) {
            return new slice<types.Object>(new types.Object[] { o });
        }
    }


    slice<types.Object> ret = default; 
    // Search through the imports to see if any of them define name.
    // It's hard to tell in general which package is being tested, so
    // for the purposes of the analysis, allow the object to appear
    // in any of the imports. This guarantees there are no false positives
    // because the example needs to use the object so it must be defined
    // in the package or one if its imports. On the other hand, false
    // negatives are possible, but should be rare.
    foreach (var (_, imp) in pkg.Imports()) {
        {
            var obj = imp.Scope().Lookup(name);

            if (obj != null) {
                ret = append(ret, obj);
            }

        }

    }    return ret;

}

private static void checkExample(ptr<analysis.Pass> _addr_pass, ptr<ast.FuncDecl> _addr_fn) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.FuncDecl fn = ref _addr_fn.val;

    var fnName = fn.Name.Name;
    {
        var @params = fn.Type.Params;

        if (len(@params.List) != 0) {
            pass.Reportf(fn.Pos(), "%s should be niladic", fnName);
        }
    }

    {
        var results = fn.Type.Results;

        if (results != null && len(results.List) != 0) {
            pass.Reportf(fn.Pos(), "%s should return nothing", fnName);
        }
    }


    if (fnName == "Example") { 
        // Nothing more to do.
        return ;

    }
    var exName = strings.TrimPrefix(fnName, "Example");    var elems = strings.SplitN(exName, "_", 3);    var ident = elems[0];    var objs = lookup(_addr_pass.Pkg, ident);
    if (ident != "" && len(objs) == 0) { 
        // Check ExampleFoo and ExampleBadFoo.
        pass.Reportf(fn.Pos(), "%s refers to unknown identifier: %s", fnName, ident); 
        // Abort since obj is absent and no subsequent checks can be performed.
        return ;

    }
    if (len(elems) < 2) { 
        // Nothing more to do.
        return ;

    }
    if (ident == "") { 
        // Check Example_suffix and Example_BadSuffix.
        {
            var residual = strings.TrimPrefix(exName, "_");

            if (!isExampleSuffix(residual)) {
                pass.Reportf(fn.Pos(), "%s has malformed example suffix: %s", fnName, residual);
            }

        }

        return ;

    }
    var mmbr = elems[1];
    if (!isExampleSuffix(mmbr)) { 
        // Check ExampleFoo_Method and ExampleFoo_BadMethod.
        var found = false; 
        // Check if Foo.Method exists in this package or its imports.
        {
            var obj__prev1 = obj;

            foreach (var (_, __obj) in objs) {
                obj = __obj;
                {
                    var obj__prev2 = obj;

                    var (obj, _, _) = types.LookupFieldOrMethod(obj.Type(), true, obj.Pkg(), mmbr);

                    if (obj != null) {
                        found = true;
                        break;
                    }

                    obj = obj__prev2;

                }

            }

            obj = obj__prev1;
        }

        if (!found) {
            pass.Reportf(fn.Pos(), "%s refers to unknown field or method: %s.%s", fnName, ident, mmbr);
        }
    }
    if (len(elems) == 3 && !isExampleSuffix(elems[2])) { 
        // Check ExampleFoo_Method_suffix and ExampleFoo_Method_Badsuffix.
        pass.Reportf(fn.Pos(), "%s has malformed example suffix: %s", fnName, elems[2]);

    }
}

private static void checkTest(ptr<analysis.Pass> _addr_pass, ptr<ast.FuncDecl> _addr_fn, @string prefix) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.FuncDecl fn = ref _addr_fn.val;
 
    // Want functions with 0 results and 1 parameter.
    if (fn.Type.Results != null && len(fn.Type.Results.List) > 0 || fn.Type.Params == null || len(fn.Type.Params.List) != 1 || len(fn.Type.Params.List[0].Names) > 1) {
        return ;
    }
    if (!isTestParam(fn.Type.Params.List[0].Type, prefix[..(int)1])) {
        return ;
    }
    if (!isTestSuffix(fn.Name.Name[(int)len(prefix)..])) {
        pass.Reportf(fn.Pos(), "%s has malformed name: first letter after '%s' must not be lowercase", fn.Name.Name, prefix);
    }
}

} // end tests_package
