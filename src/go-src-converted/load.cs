// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssautil -- go2cs converted at 2022 March 06 23:33:46 UTC
// import "golang.org/x/tools/go/ssa/ssautil" ==> using ssautil = go.golang.org.x.tools.go.ssa.ssautil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\ssautil\load.go
// This file defines utility functions for constructing programs in SSA form.

using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

using loader = go.golang.org.x.tools.go.loader_package;
using packages = go.golang.org.x.tools.go.packages_package;
using ssa = go.golang.org.x.tools.go.ssa_package;
using System;


namespace go.golang.org.x.tools.go.ssa;

public static partial class ssautil_package {

    // Packages creates an SSA program for a set of packages.
    //
    // The packages must have been loaded from source syntax using the
    // golang.org/x/tools/go/packages.Load function in LoadSyntax or
    // LoadAllSyntax mode.
    //
    // Packages creates an SSA package for each well-typed package in the
    // initial list, plus all their dependencies. The resulting list of
    // packages corresponds to the list of initial packages, and may contain
    // a nil if SSA code could not be constructed for the corresponding initial
    // package due to type errors.
    //
    // Code for bodies of functions is not built until Build is called on
    // the resulting Program. SSA code is constructed only for the initial
    // packages with well-typed syntax trees.
    //
    // The mode parameter controls diagnostics and checking during SSA construction.
    //
public static (ptr<ssa.Program>, slice<ptr<ssa.Package>>) Packages(slice<ptr<packages.Package>> initial, ssa.BuilderMode mode) {
    ptr<ssa.Program> _p0 = default!;
    slice<ptr<ssa.Package>> _p0 = default;

    return _addr_doPackages(initial, mode, false)!;
}

// AllPackages creates an SSA program for a set of packages plus all
// their dependencies.
//
// The packages must have been loaded from source syntax using the
// golang.org/x/tools/go/packages.Load function in LoadAllSyntax mode.
//
// AllPackages creates an SSA package for each well-typed package in the
// initial list, plus all their dependencies. The resulting list of
// packages corresponds to the list of initial packages, and may contain
// a nil if SSA code could not be constructed for the corresponding
// initial package due to type errors.
//
// Code for bodies of functions is not built until Build is called on
// the resulting Program. SSA code is constructed for all packages with
// well-typed syntax trees.
//
// The mode parameter controls diagnostics and checking during SSA construction.
//
public static (ptr<ssa.Program>, slice<ptr<ssa.Package>>) AllPackages(slice<ptr<packages.Package>> initial, ssa.BuilderMode mode) {
    ptr<ssa.Program> _p0 = default!;
    slice<ptr<ssa.Package>> _p0 = default;

    return _addr_doPackages(initial, mode, true)!;
}

private static (ptr<ssa.Program>, slice<ptr<ssa.Package>>) doPackages(slice<ptr<packages.Package>> initial, ssa.BuilderMode mode, bool deps) {
    ptr<ssa.Program> _p0 = default!;
    slice<ptr<ssa.Package>> _p0 = default;

    ptr<token.FileSet> fset;
    if (len(initial) > 0) {
        fset = initial[0].Fset;
    }
    var prog = ssa.NewProgram(fset, mode);

    var isInitial = make_map<ptr<packages.Package>, bool>(len(initial));
    {
        var p__prev1 = p;

        foreach (var (_, __p) in initial) {
            p = __p;
            isInitial[p] = true;
        }
        p = p__prev1;
    }

    var ssamap = make_map<ptr<packages.Package>, ptr<ssa.Package>>();
    packages.Visit(initial, null, p => {
        if (p.Types != null && !p.IllTyped) {
            slice<ptr<ast.File>> files = default;
            if (deps || isInitial[p]) {
                files = p.Syntax;
            }
            ssamap[p] = prog.CreatePackage(p.Types, files, p.TypesInfo, true);
        }
    });

    slice<ptr<ssa.Package>> ssapkgs = default;
    {
        var p__prev1 = p;

        foreach (var (_, __p) in initial) {
            p = __p;
            ssapkgs = append(ssapkgs, ssamap[p]); // may be nil
        }
        p = p__prev1;
    }

    return (_addr_prog!, ssapkgs);

}

// CreateProgram returns a new program in SSA form, given a program
// loaded from source.  An SSA package is created for each transitively
// error-free package of lprog.
//
// Code for bodies of functions is not built until Build is called
// on the result.
//
// The mode parameter controls diagnostics and checking during SSA construction.
//
// Deprecated: Use golang.org/x/tools/go/packages and the Packages
// function instead; see ssa.ExampleLoadPackages.
//
public static ptr<ssa.Program> CreateProgram(ptr<loader.Program> _addr_lprog, ssa.BuilderMode mode) {
    ref loader.Program lprog = ref _addr_lprog.val;

    var prog = ssa.NewProgram(lprog.Fset, mode);

    foreach (var (_, info) in lprog.AllPackages) {
        if (info.TransitivelyErrorFree) {
            prog.CreatePackage(info.Pkg, info.Files, _addr_info.Info, info.Importable);
        }
    }    return _addr_prog!;

}

// BuildPackage builds an SSA program with IR for a single package.
//
// It populates pkg by type-checking the specified file ASTs.  All
// dependencies are loaded using the importer specified by tc, which
// typically loads compiler export data; SSA code cannot be built for
// those packages.  BuildPackage then constructs an ssa.Program with all
// dependency packages created, and builds and returns the SSA package
// corresponding to pkg.
//
// The caller must have set pkg.Path() to the import path.
//
// The operation fails if there were any type-checking or import errors.
//
// See ../ssa/example_test.go for an example.
//
public static (ptr<ssa.Package>, ptr<types.Info>, error) BuildPackage(ptr<types.Config> _addr_tc, ptr<token.FileSet> _addr_fset, ptr<types.Package> _addr_pkg, slice<ptr<ast.File>> files, ssa.BuilderMode mode) => func((_, panic, _) => {
    ptr<ssa.Package> _p0 = default!;
    ptr<types.Info> _p0 = default!;
    error _p0 = default!;
    ref types.Config tc = ref _addr_tc.val;
    ref token.FileSet fset = ref _addr_fset.val;
    ref types.Package pkg = ref _addr_pkg.val;

    if (fset == null) {
        panic("no token.FileSet");
    }
    if (pkg.Path() == "") {
        panic("package has no import path");
    }
    ptr<types.Info> info = addr(new types.Info(Types:make(map[ast.Expr]types.TypeAndValue),Defs:make(map[*ast.Ident]types.Object),Uses:make(map[*ast.Ident]types.Object),Implicits:make(map[ast.Node]types.Object),Scopes:make(map[ast.Node]*types.Scope),Selections:make(map[*ast.SelectorExpr]*types.Selection),));
    {
        var err = types.NewChecker(tc, fset, pkg, info).Files(files);

        if (err != null) {
            return (_addr_null!, _addr_null!, error.As(err)!);
        }
    }


    var prog = ssa.NewProgram(fset, mode); 

    // Create SSA packages for all imports.
    // Order is not significant.
    var created = make_map<ptr<types.Package>, bool>();
    Action<slice<ptr<types.Package>>> createAll = default;
    createAll = pkgs => {
        foreach (var (_, p) in pkgs) {
            if (!created[p]) {
                created[p] = true;
                prog.CreatePackage(p, null, null, true);
                createAll(p.Imports());
            }
        }
    };
    createAll(pkg.Imports()); 

    // Create and build the primary package.
    var ssapkg = prog.CreatePackage(pkg, files, info, false);
    ssapkg.Build();
    return (_addr_ssapkg!, _addr_info!, error.As(null!)!);

});

} // end ssautil_package
