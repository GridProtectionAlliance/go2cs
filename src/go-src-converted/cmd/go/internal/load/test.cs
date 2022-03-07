// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2022 March 06 23:17:15 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\load\test.go
using bytes = go.bytes_package;
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using doc = go.go.doc_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using lazytemplate = go.@internal.lazytemplate_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using fsys = go.cmd.go.@internal.fsys_package;
using str = go.cmd.go.@internal.str_package;
using trace = go.cmd.go.@internal.trace_package;
using System;


namespace go.cmd.go.@internal;

public static partial class load_package {

public static @string TestMainDeps = new slice<@string>(new @string[] { "os", "reflect", "testing", "testing/internal/testdeps" });

public partial struct TestCover {
    public @string Mode;
    public bool Local;
    public slice<ptr<Package>> Pkgs;
    public slice<@string> Paths;
    public slice<coverInfo> Vars;
    public Func<ptr<Package>, @string, map<@string, ptr<CoverVar>>> DeclVars;
}

// TestPackagesFor is like TestPackagesAndErrors but it returns
// an error if the test packages or their dependencies have errors.
// Only test packages without errors are returned.
public static (ptr<Package>, ptr<Package>, ptr<Package>, error) TestPackagesFor(context.Context ctx, PackageOpts opts, ptr<Package> _addr_p, ptr<TestCover> _addr_cover) {
    ptr<Package> pmain = default!;
    ptr<Package> ptest = default!;
    ptr<Package> pxtest = default!;
    error err = default!;
    ref Package p = ref _addr_p.val;
    ref TestCover cover = ref _addr_cover.val;

    pmain, ptest, pxtest = TestPackagesAndErrors(ctx, opts, _addr_p, _addr_cover);
    foreach (var (_, p1) in new slice<ptr<Package>>(new ptr<Package>[] { ptest, pxtest, pmain })) {
        if (p1 == null) { 
            // pxtest may be nil
            continue;

        }
        if (p1.Error != null) {
            err = p1.Error;
            break;
        }
        if (len(p1.DepsErrors) > 0) {
            var perr = p1.DepsErrors[0];
            err = perr;
            break;
        }
    }    if (pmain.Error != null || len(pmain.DepsErrors) > 0) {
        pmain = null;
    }
    if (ptest.Error != null || len(ptest.DepsErrors) > 0) {
        ptest = null;
    }
    if (pxtest != null && (pxtest.Error != null || len(pxtest.DepsErrors) > 0)) {
        pxtest = null;
    }
    return (_addr_pmain!, _addr_ptest!, _addr_pxtest!, error.As(err)!);

}

// TestPackagesAndErrors returns three packages:
//    - pmain, the package main corresponding to the test binary (running tests in ptest and pxtest).
//    - ptest, the package p compiled with added "package p" test files.
//    - pxtest, the result of compiling any "package p_test" (external) test files.
//
// If the package has no "package p_test" test files, pxtest will be nil.
// If the non-test compilation of package p can be reused
// (for example, if there are no "package p" test files and
// package p need not be instrumented for coverage or any other reason),
// then the returned ptest == p.
//
// An error is returned if the testmain source cannot be completely generated
// (for example, due to a syntax error in a test file). No error will be
// returned for errors loading packages, but the Error or DepsError fields
// of the returned packages may be set.
//
// The caller is expected to have checked that len(p.TestGoFiles)+len(p.XTestGoFiles) > 0,
// or else there's no point in any of this.
public static (ptr<Package>, ptr<Package>, ptr<Package>) TestPackagesAndErrors(context.Context ctx, PackageOpts opts, ptr<Package> _addr_p, ptr<TestCover> _addr_cover) => func((defer, _, _) => {
    ptr<Package> pmain = default!;
    ptr<Package> ptest = default!;
    ptr<Package> pxtest = default!;
    ref Package p = ref _addr_p.val;
    ref TestCover cover = ref _addr_cover.val;

    var (ctx, span) = trace.StartSpan(ctx, "load.TestPackagesAndErrors");
    defer(span.Done());

    var pre = newPreload();
    defer(pre.flush());
    var allImports = append(new slice<@string>(new @string[] {  }), p.TestImports);
    allImports = append(allImports, p.XTestImports);
    pre.preloadImports(ctx, opts, allImports, p.Internal.Build);

    ptr<PackageError> ptestErr;    ptr<PackageError> pxtestErr;

    slice<ptr<Package>> imports = default;    slice<ptr<Package>> ximports = default;

    ref ImportStack stk = ref heap(out ptr<ImportStack> _addr_stk);
    map<@string, slice<@string>> testEmbed = default;    map<@string, slice<@string>> xtestEmbed = default;

    stk.Push(p.ImportPath + " (test)");
    var rawTestImports = str.StringList(p.TestImports);
    {
        var i__prev1 = i;
        var path__prev1 = path;

        foreach (var (__i, __path) in p.TestImports) {
            i = __i;
            path = __path;
            var p1 = loadImport(ctx, opts, pre, path, p.Dir, p, _addr_stk, p.Internal.Build.TestImportPos[path], ResolveImport);
            if (str.Contains(p1.Deps, p.ImportPath) || p1.ImportPath == p.ImportPath) { 
                // Same error that loadPackage returns (via reusePackage) in pkg.go.
                // Can't change that code, because that code is only for loading the
                // non-test copy of a package.
                ptestErr = addr(new PackageError(ImportStack:importCycleStack(p1,p.ImportPath),Err:errors.New("import cycle not allowed in test"),IsImportCycle:true,));

            }

            p.TestImports[i] = p1.ImportPath;
            imports = append(imports, p1);

        }
        i = i__prev1;
        path = path__prev1;
    }

    error err = default!;
    p.TestEmbedFiles, testEmbed, err = resolveEmbed(p.Dir, p.TestEmbedPatterns);
    if (err != null && ptestErr == null) {
        ptestErr = addr(new PackageError(ImportStack:stk.Copy(),Err:err,));
        ptr<EmbedError> embedErr = err._<ptr<EmbedError>>();
        ptestErr.setPos(p.Internal.Build.TestEmbedPatternPos[embedErr.Pattern]);
    }
    stk.Pop();

    stk.Push(p.ImportPath + "_test");
    var pxtestNeedsPtest = false;
    var rawXTestImports = str.StringList(p.XTestImports);
    {
        var i__prev1 = i;
        var path__prev1 = path;

        foreach (var (__i, __path) in p.XTestImports) {
            i = __i;
            path = __path;
            p1 = loadImport(ctx, opts, pre, path, p.Dir, p, _addr_stk, p.Internal.Build.XTestImportPos[path], ResolveImport);
            if (p1.ImportPath == p.ImportPath) {
                pxtestNeedsPtest = true;
            }
            else
 {
                ximports = append(ximports, p1);
            }

            p.XTestImports[i] = p1.ImportPath;

        }
        i = i__prev1;
        path = path__prev1;
    }

    p.XTestEmbedFiles, xtestEmbed, err = resolveEmbed(p.Dir, p.XTestEmbedPatterns);
    if (err != null && pxtestErr == null) {
        pxtestErr = addr(new PackageError(ImportStack:stk.Copy(),Err:err,));
        embedErr = err._<ptr<EmbedError>>();
        pxtestErr.setPos(p.Internal.Build.XTestEmbedPatternPos[embedErr.Pattern]);
    }
    stk.Pop(); 

    // Test package.
    if (len(p.TestGoFiles) > 0 || p.Name == "main" || cover != null && cover.Local) {
        ptest = @new<Package>();
        ptest.val = p;
        ptest.Error = ptestErr;
        ptest.ForTest = p.ImportPath;
        ptest.GoFiles = null;
        ptest.GoFiles = append(ptest.GoFiles, p.GoFiles);
        ptest.GoFiles = append(ptest.GoFiles, p.TestGoFiles);
        ptest.Target = ""; 
        // Note: The preparation of the vet config requires that common
        // indexes in ptest.Imports and ptest.Internal.RawImports
        // all line up (but RawImports can be shorter than the others).
        // That is, for 0 ≤ i < len(RawImports),
        // RawImports[i] is the import string in the program text, and
        // Imports[i] is the expanded import string (vendoring applied or relative path expanded away).
        // Any implicitly added imports appear in Imports and Internal.Imports
        // but not RawImports (because they were not in the source code).
        // We insert TestImports, imports, and rawTestImports at the start of
        // these lists to preserve the alignment.
        // Note that p.Internal.Imports may not be aligned with p.Imports/p.Internal.RawImports,
        // but we insert at the beginning there too just for consistency.
        ptest.Imports = str.StringList(p.TestImports, p.Imports);
        ptest.Internal.Imports = append(imports, p.Internal.Imports);
        ptest.Internal.RawImports = str.StringList(rawTestImports, p.Internal.RawImports);
        ptest.Internal.ForceLibrary = true;
        ptest.Internal.BuildInfo = "";
        ptest.Internal.Build = @new<build.Package>();
        ptest.Internal.Build.val = p.Internal.Build.val;
        map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<token.Position>>{};
        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in p.Internal.Build.ImportPos) {
                k = __k;
                v = __v;
                m[k] = append(m[k], v);
            }
    else

            k = k__prev1;
            v = v__prev1;
        }

        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in p.Internal.Build.TestImportPos) {
                k = __k;
                v = __v;
                m[k] = append(m[k], v);
            }

            k = k__prev1;
            v = v__prev1;
        }

        ptest.Internal.Build.ImportPos = m;
        if (testEmbed == null && len(p.Internal.Embed) > 0) {
            testEmbed = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<@string>>{};
        }
        {
            var k__prev1 = k;
            var v__prev1 = v;

            foreach (var (__k, __v) in p.Internal.Embed) {
                k = __k;
                v = __v;
                testEmbed[k] = v;
            }

            k = k__prev1;
            v = v__prev1;
        }

        ptest.Internal.Embed = testEmbed;
        ptest.EmbedFiles = str.StringList(p.EmbedFiles, p.TestEmbedFiles);
        ptest.Internal.OrigImportPath = p.Internal.OrigImportPath;
        ptest.collectDeps();

    } {
        ptest = p;
    }
    if (len(p.XTestGoFiles) > 0) {
        pxtest = addr(new Package(PackagePublic:PackagePublic{Name:p.Name+"_test",ImportPath:p.ImportPath+"_test",Root:p.Root,Dir:p.Dir,Goroot:p.Goroot,GoFiles:p.XTestGoFiles,Imports:p.XTestImports,ForTest:p.ImportPath,Module:p.Module,Error:pxtestErr,EmbedFiles:p.XTestEmbedFiles,},Internal:PackageInternal{LocalPrefix:p.Internal.LocalPrefix,Build:&build.Package{ImportPos:p.Internal.Build.XTestImportPos,},Imports:ximports,RawImports:rawXTestImports,Asmflags:p.Internal.Asmflags,Gcflags:p.Internal.Gcflags,Ldflags:p.Internal.Ldflags,Gccgoflags:p.Internal.Gccgoflags,Embed:xtestEmbed,OrigImportPath:p.Internal.OrigImportPath,},));
        if (pxtestNeedsPtest) {
            pxtest.Internal.Imports = append(pxtest.Internal.Imports, ptest);
        }
        pxtest.collectDeps();

    }
    pmain = addr(new Package(PackagePublic:PackagePublic{Name:"main",Dir:p.Dir,GoFiles:[]string{"_testmain.go"},ImportPath:p.ImportPath+".test",Root:p.Root,Imports:str.StringList(TestMainDeps),Module:p.Module,},Internal:PackageInternal{Build:&build.Package{Name:"main"},BuildInfo:p.Internal.BuildInfo,Asmflags:p.Internal.Asmflags,Gcflags:p.Internal.Gcflags,Ldflags:p.Internal.Ldflags,Gccgoflags:p.Internal.Gccgoflags,OrigImportPath:p.Internal.OrigImportPath,},)); 

    // The generated main also imports testing, regexp, and os.
    // Also the linker introduces implicit dependencies reported by LinkerDeps.
    stk.Push("testmain");
    var deps = TestMainDeps; // cap==len, so safe for append
    foreach (var (_, d) in LinkerDeps(p)) {
        deps = append(deps, d);
    }    foreach (var (_, dep) in deps) {
        if (dep == ptest.ImportPath) {
            pmain.Internal.Imports = append(pmain.Internal.Imports, ptest);
        }
        else
 {
            p1 = loadImport(ctx, opts, pre, dep, "", null, _addr_stk, null, 0);
            pmain.Internal.Imports = append(pmain.Internal.Imports, p1);
        }
    }    stk.Pop();

    if (cover != null && cover.Pkgs != null) { 
        // Add imports, but avoid duplicates.
        map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Package>, bool>{p:true,ptest:true};
        {
            var p1__prev1 = p1;

            foreach (var (_, __p1) in pmain.Internal.Imports) {
                p1 = __p1;
                seen[p1] = true;
            }

            p1 = p1__prev1;
        }

        {
            var p1__prev1 = p1;

            foreach (var (_, __p1) in cover.Pkgs) {
                p1 = __p1;
                if (seen[p1]) { 
                    // Don't add duplicate imports.
                    continue;

                }

                seen[p1] = true;
                pmain.Internal.Imports = append(pmain.Internal.Imports, p1);

            }

            p1 = p1__prev1;
        }
    }
    var allTestImports = make_slice<ptr<Package>>(0, len(pmain.Internal.Imports) + len(imports) + len(ximports));
    allTestImports = append(allTestImports, pmain.Internal.Imports);
    allTestImports = append(allTestImports, imports);
    allTestImports = append(allTestImports, ximports);
    setToolFlags(allTestImports); 

    // Do initial scan for metadata needed for writing _testmain.go
    // Use that metadata to update the list of imports for package main.
    // The list of imports is used by recompileForTest and by the loop
    // afterward that gathers t.Cover information.
    var (t, err) = loadTestFuncs(_addr_ptest);
    if (err != null && pmain.Error == null) {
        pmain.setLoadPackageDataError(err, p.ImportPath, _addr_stk, null);
    }
    t.Cover = cover;
    if (len(ptest.GoFiles) + len(ptest.CgoFiles) > 0) {
        pmain.Internal.Imports = append(pmain.Internal.Imports, ptest);
        pmain.Imports = append(pmain.Imports, ptest.ImportPath);
        t.ImportTest = true;
    }
    if (pxtest != null) {
        pmain.Internal.Imports = append(pmain.Internal.Imports, pxtest);
        pmain.Imports = append(pmain.Imports, pxtest.ImportPath);
        t.ImportXtest = true;
    }
    pmain.collectDeps(); 

    // Sort and dedup pmain.Imports.
    // Only matters for go list -test output.
    sort.Strings(pmain.Imports);
    nint w = 0;
    {
        var path__prev1 = path;

        foreach (var (_, __path) in pmain.Imports) {
            path = __path;
            if (w == 0 || path != pmain.Imports[w - 1]) {
                pmain.Imports[w] = path;
                w++;
            }
        }
        path = path__prev1;
    }

    pmain.Imports = pmain.Imports[..(int)w];
    pmain.Internal.RawImports = str.StringList(pmain.Imports); 

    // Replace pmain's transitive dependencies with test copies, as necessary.
    recompileForTest(_addr_pmain, _addr_p, _addr_ptest, _addr_pxtest); 

    // Should we apply coverage analysis locally,
    // only for this package and only for this test?
    // Yes, if -cover is on but -coverpkg has not specified
    // a list of packages for global coverage.
    if (cover != null && cover.Local) {
        ptest.Internal.CoverMode = cover.Mode;
        slice<@string> coverFiles = default;
        coverFiles = append(coverFiles, ptest.GoFiles);
        coverFiles = append(coverFiles, ptest.CgoFiles);
        ptest.Internal.CoverVars = cover.DeclVars(ptest, coverFiles);
    }
    foreach (var (_, cp) in pmain.Internal.Imports) {
        if (len(cp.Internal.CoverVars) > 0) {
            t.Cover.Vars = append(t.Cover.Vars, new coverInfo(cp,cp.Internal.CoverVars));
        }
    }    var (data, err) = formatTestmain(_addr_t);
    if (err != null && pmain.Error == null) {
        pmain.Error = addr(new PackageError(Err:err));
    }
    if (data != null) {
        pmain.Internal.TestmainGo = _addr_data;
    }
    return (_addr_pmain!, _addr_ptest!, _addr_pxtest!);

});

// importCycleStack returns an import stack from p to the package whose import
// path is target.
private static slice<@string> importCycleStack(ptr<Package> _addr_p, @string target) => func((_, panic, _) => {
    ref Package p = ref _addr_p.val;
 
    // importerOf maps each import path to its importer nearest to p.
    map importerOf = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{p.ImportPath:""}; 

    // q is a breadth-first queue of packages to search for target.
    // Every package added to q has a corresponding entry in pathTo.
    //
    // We search breadth-first for two reasons:
    //
    //     1. We want to report the shortest cycle.
    //
    //     2. If p contains multiple cycles, the first cycle we encounter might not
    //        contain target. To ensure termination, we have to break all cycles
    //        other than the first.
    ptr<Package> q = new slice<ptr<Package>>(new ptr<Package>[] { p });

    while (len(q) > 0) {
        var p = q[0];
        q = q[(int)1..];
        {
            var path = p.ImportPath;

            if (path == target) {
                slice<@string> stk = default;
                while (path != "") {
                    stk = append(stk, path);
                    path = importerOf[path];
                }

                return stk;
            }

        }

        foreach (var (_, dep) in p.Internal.Imports) {
            {
                var (_, ok) = importerOf[dep.ImportPath];

                if (!ok) {
                    importerOf[dep.ImportPath] = p.ImportPath;
                    q = append(q, dep);
                }

            }

        }
    }

    panic("lost path to cycle");

});

// recompileForTest copies and replaces certain packages in pmain's dependency
// graph. This is necessary for two reasons. First, if ptest is different than
// preal, packages that import the package under test should get ptest instead
// of preal. This is particularly important if pxtest depends on functionality
// exposed in test sources in ptest. Second, if there is a main package
// (other than pmain) anywhere, we need to set p.Internal.ForceLibrary and
// clear p.Internal.BuildInfo in the test copy to prevent link conflicts.
// This may happen if both -coverpkg and the command line patterns include
// multiple main packages.
private static void recompileForTest(ptr<Package> _addr_pmain, ptr<Package> _addr_preal, ptr<Package> _addr_ptest, ptr<Package> _addr_pxtest) => func((_, panic, _) => {
    ref Package pmain = ref _addr_pmain.val;
    ref Package preal = ref _addr_preal.val;
    ref Package ptest = ref _addr_ptest.val;
    ref Package pxtest = ref _addr_pxtest.val;
 
    // The "test copy" of preal is ptest.
    // For each package that depends on preal, make a "test copy"
    // that depends on ptest. And so on, up the dependency tree.
    map testCopy = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Package>, ptr<Package>>{preal:ptest};
    foreach (var (_, p) in PackageList(new slice<ptr<Package>>(new ptr<Package>[] { pmain }))) {
        if (p == preal) {
            continue;
        }
        var didSplit = p == pmain || p == pxtest;
        Action split = () => {
            if (didSplit) {
                return ;
            }
            didSplit = true;
            if (testCopy[p] != null) {
                panic("recompileForTest loop");
            }
            ptr<Package> p1 = @new<Package>();
            testCopy[p] = p1;
            p1.val = p.val;
            p1.ForTest = preal.ImportPath;
            p1.Internal.Imports = make_slice<ptr<Package>>(len(p.Internal.Imports));
            copy(p1.Internal.Imports, p.Internal.Imports);
            p1.Imports = make_slice<@string>(len(p.Imports));
            copy(p1.Imports, p.Imports);
            p = p1;
            p.Target = "";
            p.Internal.BuildInfo = "";
            p.Internal.ForceLibrary = true;
        }; 

        // Update p.Internal.Imports to use test copies.
        foreach (var (i, imp) in p.Internal.Imports) {
            {
                ptr<Package> p1__prev1 = p1;

                p1 = testCopy[imp];

                if (p1 != null && p1 != imp) {
                    split();
                    p.Internal.Imports[i] = p1;
                }

                p1 = p1__prev1;

            }

        }        if (p.Name == "main" && p != pmain && p != ptest) {
            split();
        }
    }
});

// isTestFunc tells whether fn has the type of a testing function. arg
// specifies the parameter type we look for: B, M or T.
private static bool isTestFunc(ptr<ast.FuncDecl> _addr_fn, @string arg) {
    ref ast.FuncDecl fn = ref _addr_fn.val;

    if (fn.Type.Results != null && len(fn.Type.Results.List) > 0 || fn.Type.Params.List == null || len(fn.Type.Params.List) != 1 || len(fn.Type.Params.List[0].Names) > 1) {
        return false;
    }
    ptr<ast.StarExpr> (ptr, ok) = fn.Type.Params.List[0].Type._<ptr<ast.StarExpr>>();
    if (!ok) {
        return false;
    }
    {
        ptr<ast.Ident> (name, ok) = ptr.X._<ptr<ast.Ident>>();

        if (ok && name.Name == arg) {
            return true;
        }
    }

    {
        ptr<ast.SelectorExpr> (sel, ok) = ptr.X._<ptr<ast.SelectorExpr>>();

        if (ok && sel.Sel.Name == arg) {
            return true;
        }
    }

    return false;

}

// isTest tells whether name looks like a test (or benchmark, according to prefix).
// It is a Test (say) if there is a character after Test that is not a lower-case letter.
// We don't want TesticularCancer.
private static bool isTest(@string name, @string prefix) {
    if (!strings.HasPrefix(name, prefix)) {
        return false;
    }
    if (len(name) == len(prefix)) { // "Test" is ok
        return true;

    }
    var (rune, _) = utf8.DecodeRuneInString(name[(int)len(prefix)..]);
    return !unicode.IsLower(rune);

}

private partial struct coverInfo {
    public ptr<Package> Package;
    public map<@string, ptr<CoverVar>> Vars;
}

// loadTestFuncs returns the testFuncs describing the tests that will be run.
// The returned testFuncs is always non-nil, even if an error occurred while
// processing test files.
private static (ptr<testFuncs>, error) loadTestFuncs(ptr<Package> _addr_ptest) {
    ptr<testFuncs> _p0 = default!;
    error _p0 = default!;
    ref Package ptest = ref _addr_ptest.val;

    ptr<testFuncs> t = addr(new testFuncs(Package:ptest,));
    error err = default!;
    {
        var file__prev1 = file;

        foreach (var (_, __file) in ptest.TestGoFiles) {
            file = __file;
            {
                var lerr__prev1 = lerr;

                var lerr = t.load(filepath.Join(ptest.Dir, file), "_test", _addr_t.ImportTest, _addr_t.NeedTest);

                if (lerr != null && err == null) {
                    err = error.As(lerr)!;
                }

                lerr = lerr__prev1;

            }

        }
        file = file__prev1;
    }

    {
        var file__prev1 = file;

        foreach (var (_, __file) in ptest.XTestGoFiles) {
            file = __file;
            {
                var lerr__prev1 = lerr;

                lerr = t.load(filepath.Join(ptest.Dir, file), "_xtest", _addr_t.ImportXtest, _addr_t.NeedXtest);

                if (lerr != null && err == null) {
                    err = error.As(lerr)!;
                }

                lerr = lerr__prev1;

            }

        }
        file = file__prev1;
    }

    return (_addr_t!, error.As(err)!);

}

// formatTestmain returns the content of the _testmain.go file for t.
private static (slice<byte>, error) formatTestmain(ptr<testFuncs> _addr_t) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref testFuncs t = ref _addr_t.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    {
        var err = testmainTmpl.Execute(_addr_buf, t);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return (buf.Bytes(), error.As(null!)!);

}

private partial struct testFuncs {
    public slice<testFunc> Tests;
    public slice<testFunc> Benchmarks;
    public slice<testFunc> Examples;
    public ptr<testFunc> TestMain;
    public ptr<Package> Package;
    public bool ImportTest;
    public bool NeedTest;
    public bool ImportXtest;
    public bool NeedXtest;
    public ptr<TestCover> Cover;
}

// ImportPath returns the import path of the package being tested, if it is within GOPATH.
// This is printed by the testing package when running benchmarks.
private static @string ImportPath(this ptr<testFuncs> _addr_t) {
    ref testFuncs t = ref _addr_t.val;

    var pkg = t.Package.ImportPath;
    if (strings.HasPrefix(pkg, "_/")) {
        return "";
    }
    if (pkg == "command-line-arguments") {
        return "";
    }
    return pkg;

}

// Covered returns a string describing which packages are being tested for coverage.
// If the covered package is the same as the tested package, it returns the empty string.
// Otherwise it is a comma-separated human-readable list of packages beginning with
// " in", ready for use in the coverage message.
private static @string Covered(this ptr<testFuncs> _addr_t) {
    ref testFuncs t = ref _addr_t.val;

    if (t.Cover == null || t.Cover.Paths == null) {
        return "";
    }
    return " in " + strings.Join(t.Cover.Paths, ", ");

}

// Tested returns the name of the package being tested.
private static @string Tested(this ptr<testFuncs> _addr_t) {
    ref testFuncs t = ref _addr_t.val;

    return t.Package.Name;
}

private partial struct testFunc {
    public @string Package; // imported package name (_test or _xtest)
    public @string Name; // function name
    public @string Output; // output, for examples
    public bool Unordered; // output is allowed to be unordered.
}

private static var testFileSet = token.NewFileSet();

private static error load(this ptr<testFuncs> _addr_t, @string filename, @string pkg, ptr<bool> _addr_doImport, ptr<bool> _addr_seen) => func((defer, _, _) => {
    ref testFuncs t = ref _addr_t.val;
    ref bool doImport = ref _addr_doImport.val;
    ref bool seen = ref _addr_seen.val;
 
    // Pass in the overlaid source if we have an overlay for this file.
    var (src, err) = fsys.Open(filename);
    if (err != null) {
        return error.As(err)!;
    }
    defer(src.Close());
    var (f, err) = parser.ParseFile(testFileSet, filename, src, parser.ParseComments);
    if (err != null) {
        return error.As(err)!;
    }
    foreach (var (_, d) in f.Decls) {
        ptr<ast.FuncDecl> (n, ok) = d._<ptr<ast.FuncDecl>>();
        if (!ok) {
            continue;
        }
        if (n.Recv != null) {
            continue;
        }
        var name = n.Name.String();

        if (name == "TestMain") 
            if (isTestFunc(n, "T")) {
                t.Tests = append(t.Tests, new testFunc(pkg,name,"",false));
                (doImport, seen) = (true, true);                continue;
            }
            var err = checkTestFunc(n, "M");
            if (err != null) {
                return error.As(err)!;
            }
            if (t.TestMain != null) {
                return error.As(errors.New("multiple definitions of TestMain"))!;
            }
            t.TestMain = addr(new testFunc(pkg,name,"",false));
            (doImport, seen) = (true, true);        else if (isTest(name, "Test")) 
            err = checkTestFunc(n, "T");
            if (err != null) {
                return error.As(err)!;
            }
            t.Tests = append(t.Tests, new testFunc(pkg,name,"",false));
            (doImport, seen) = (true, true);        else if (isTest(name, "Benchmark")) 
            err = checkTestFunc(n, "B");
            if (err != null) {
                return error.As(err)!;
            }
            t.Benchmarks = append(t.Benchmarks, new testFunc(pkg,name,"",false));
            (doImport, seen) = (true, true);        
    }    var ex = doc.Examples(f);
    sort.Slice(ex, (i, j) => error.As(ex[i].Order < ex[j].Order)!);
    foreach (var (_, e) in ex) {
        doImport = true; // import test file whether executed or not
        if (e.Output == "" && !e.EmptyOutput) { 
            // Don't run examples with no output.
            continue;

        }
        t.Examples = append(t.Examples, new testFunc(pkg,"Example"+e.Name,e.Output,e.Unordered));
        seen = true;

    }    return error.As(null!)!;

});

private static error checkTestFunc(ptr<ast.FuncDecl> _addr_fn, @string arg) {
    ref ast.FuncDecl fn = ref _addr_fn.val;

    if (!isTestFunc(_addr_fn, arg)) {
        var name = fn.Name.String();
        var pos = testFileSet.Position(fn.Pos());
        return error.As(fmt.Errorf("%s: wrong signature for %s, must be: func %s(%s *testing.%s)", pos, name, name, strings.ToLower(arg), arg))!;
    }
    return error.As(null!)!;

}

private static var testmainTmpl = lazytemplate.New("main", "\n// Code generated by \'go test\'. DO NOT EDIT.\n\npackage main\n\nimport (\n\t\"os\"\n{{if " +
    ".TestMain}}\n\t\"reflect\"\n{{end}}\n\t\"testing\"\n\t\"testing/internal/testdeps\"\n\n{{if .Im" +
    "portTest}}\n\t{{if .NeedTest}}_test{{else}}_{{end}} {{.Package.ImportPath | printf" +
    " \"%q\"}}\n{{end}}\n{{if .ImportXtest}}\n\t{{if .NeedXtest}}_xtest{{else}}_{{end}} {{." +
    "Package.ImportPath | printf \"%s_test\" | printf \"%q\"}}\n{{end}}\n{{if .Cover}}\n{{ra" +
    "nge $i, $p := .Cover.Vars}}\n\t_cover{{$i}} {{$p.Package.ImportPath | printf \"%q\"}" +
    "}\n{{end}}\n{{end}}\n)\n\nvar tests = []testing.InternalTest{\n{{range .Tests}}\n\t{\"{{." +
    "Name}}\", {{.Package}}.{{.Name}}},\n{{end}}\n}\n\nvar benchmarks = []testing.Internal" +
    "Benchmark{\n{{range .Benchmarks}}\n\t{\"{{.Name}}\", {{.Package}}.{{.Name}}},\n{{end}}" +
    "\n}\n\nvar examples = []testing.InternalExample{\n{{range .Examples}}\n\t{\"{{.Name}}\"," +
    " {{.Package}}.{{.Name}}, {{.Output | printf \"%q\"}}, {{.Unordered}}},\n{{end}}\n}\n\n" +
    "func init() {\n\ttestdeps.ImportPath = {{.ImportPath | printf \"%q\"}}\n}\n\n{{if .Cove" +
    "r}}\n\n// Only updated by init functions, so no need for atomicity.\nvar (\n\tcoverCo" +
    "unters = make(map[string][]uint32)\n\tcoverBlocks = make(map[string][]testing.Cove" +
    "rBlock)\n)\n\nfunc init() {\n\t{{range $i, $p := .Cover.Vars}}\n\t{{range $file, $cover" +
    " := $p.Vars}}\n\tcoverRegisterFile({{printf \"%q\" $cover.File}}, _cover{{$i}}.{{$co" +
    "ver.Var}}.Count[:], _cover{{$i}}.{{$cover.Var}}.Pos[:], _cover{{$i}}.{{$cover.Va" +
    "r}}.NumStmt[:])\n\t{{end}}\n\t{{end}}\n}\n\nfunc coverRegisterFile(fileName string, cou" +
    "nter []uint32, pos []uint32, numStmts []uint16) {\n\tif 3*len(counter) != len(pos)" +
    " || len(counter) != len(numStmts) {\n\t\tpanic(\"coverage: mismatched sizes\")\n\t}\n\tif" +
    " coverCounters[fileName] != nil {\n\t\t// Already registered.\n\t\treturn\n\t}\n\tcoverCou" +
    "nters[fileName] = counter\n\tblock := make([]testing.CoverBlock, len(counter))\n\tfo" +
    "r i := range counter {\n\t\tblock[i] = testing.CoverBlock{\n\t\t\tLine0: pos[3*i+0],\n\t\t" +
    "\tCol0: uint16(pos[3*i+2]),\n\t\t\tLine1: pos[3*i+1],\n\t\t\tCol1: uint16(pos[3*i+2]>>16)" +
    ",\n\t\t\tStmts: numStmts[i],\n\t\t}\n\t}\n\tcoverBlocks[fileName] = block\n}\n{{end}}\n\nfunc m" +
    "ain() {\n{{if .Cover}}\n\ttesting.RegisterCover(testing.Cover{\n\t\tMode: {{printf \"%q" +
    "\" .Cover.Mode}},\n\t\tCounters: coverCounters,\n\t\tBlocks: coverBlocks,\n\t\tCoveredPack" +
    "ages: {{printf \"%q\" .Covered}},\n\t})\n{{end}}\n\tm := testing.MainStart(testdeps.Tes" +
    "tDeps{}, tests, benchmarks, examples)\n{{with .TestMain}}\n\t{{.Package}}.{{.Name}}" +
    "(m)\n\tos.Exit(int(reflect.ValueOf(m).Elem().FieldByName(\"exitCode\").Int()))\n{{els" +
    "e}}\n\tos.Exit(m.Run())\n{{end}}\n}\n\n");

} // end load_package
