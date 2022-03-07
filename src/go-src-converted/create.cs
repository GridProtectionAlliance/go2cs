// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:12 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\create.go
// This file implements the CREATE phase of SSA construction.
// See builder.go for explanation.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using sync = go.sync_package;

using typeutil = go.golang.org.x.tools.go.types.typeutil_package;

namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

    // NewProgram returns a new SSA Program.
    //
    // mode controls diagnostics and checking during SSA construction.
    //
public static ptr<Program> NewProgram(ptr<token.FileSet> _addr_fset, BuilderMode mode) {
    ref token.FileSet fset = ref _addr_fset.val;

    ptr<Program> prog = addr(new Program(Fset:fset,imported:make(map[string]*Package),packages:make(map[*types.Package]*Package),thunks:make(map[selectionKey]*Function),bounds:make(map[*types.Func]*Function),mode:mode,));

    var h = typeutil.MakeHasher(); // protected by methodsMu, in effect
    prog.methodSets.SetHasher(h);
    prog.canon.SetHasher(h);

    return _addr_prog!;

}

// memberFromObject populates package pkg with a member for the
// typechecker object obj.
//
// For objects from Go source code, syntax is the associated syntax
// tree (for funcs and vars only); it will be used during the build
// phase.
//
private static void memberFromObject(ptr<Package> _addr_pkg, types.Object obj, ast.Node syntax) => func((_, panic, _) => {
    ref Package pkg = ref _addr_pkg.val;

    var name = obj.Name();
    switch (obj.type()) {
        case ptr<types.Builtin> obj:
            if (pkg.Pkg != types.Unsafe) {
                panic("unexpected builtin object: " + obj.String());
            }
            break;
        case ptr<types.TypeName> obj:
            pkg.Members[name] = addr(new Type(object:obj,pkg:pkg,));
            break;
        case ptr<types.Const> obj:
            ptr<NamedConst> c = addr(new NamedConst(object:obj,Value:NewConst(obj.Val(),obj.Type()),pkg:pkg,));
            pkg.values[obj] = c.Value;
            pkg.Members[name] = c;
            break;
        case ptr<types.Var> obj:
            ptr<Global> g = addr(new Global(Pkg:pkg,name:name,object:obj,typ:types.NewPointer(obj.Type()),pos:obj.Pos(),));
            pkg.values[obj] = g;
            pkg.Members[name] = g;
            break;
        case ptr<types.Func> obj:
            ptr<types.Signature> sig = obj.Type()._<ptr<types.Signature>>();
            if (sig.Recv() == null && name == "init") {
                pkg.ninit++;
                name = fmt.Sprintf("init#%d", pkg.ninit);
            }
            ptr<Function> fn = addr(new Function(name:name,object:obj,Signature:sig,syntax:syntax,pos:obj.Pos(),Pkg:pkg,Prog:pkg.Prog,));
            if (syntax == null) {
                fn.Synthetic = "loaded from gc object file";
            }
            pkg.values[obj] = fn;
            if (sig.Recv() == null) {
                pkg.Members[name] = fn; // package-level function
            }

            break;
        default:
        {
            var obj = obj.type();
            panic("unexpected Object type: " + obj.String());
            break;
        }
    }

});

// membersFromDecl populates package pkg with members for each
// typechecker object (var, func, const or type) associated with the
// specified decl.
//
private static void membersFromDecl(ptr<Package> _addr_pkg, ast.Decl decl) {
    ref Package pkg = ref _addr_pkg.val;

    switch (decl.type()) {
        case ptr<ast.GenDecl> decl:

            if (decl.Tok == token.CONST) 
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in decl.Specs) {
                        spec = __spec;
                        {
                            ptr<ast.ValueSpec> id__prev2 = id;

                            foreach (var (_, __id) in spec._<ptr<ast.ValueSpec>>().Names) {
                                id = __id;
                                if (!isBlankIdent(id)) {
                                    memberFromObject(_addr_pkg, pkg.info.Defs[id], null);
                                }
                            }

                            id = id__prev2;
                        }
                    }

                    spec = spec__prev1;
                }
            else if (decl.Tok == token.VAR) 
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in decl.Specs) {
                        spec = __spec;
                        {
                            ptr<ast.ValueSpec> id__prev2 = id;

                            foreach (var (_, __id) in spec._<ptr<ast.ValueSpec>>().Names) {
                                id = __id;
                                if (!isBlankIdent(id)) {
                                    memberFromObject(_addr_pkg, pkg.info.Defs[id], spec);
                                }
                            }

                            id = id__prev2;
                        }
                    }

                    spec = spec__prev1;
                }
            else if (decl.Tok == token.TYPE) 
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in decl.Specs) {
                        spec = __spec;
                        ptr<ast.TypeSpec> id = spec._<ptr<ast.TypeSpec>>().Name;
                        if (!isBlankIdent(id)) {
                            memberFromObject(_addr_pkg, pkg.info.Defs[id], null);
                        }
                    }

                    spec = spec__prev1;
                }
                        break;
        case ptr<ast.FuncDecl> decl:
            id = decl.Name;
            if (!isBlankIdent(id)) {
                memberFromObject(_addr_pkg, pkg.info.Defs[id], decl);
            }
            break;
    }

}

// CreatePackage constructs and returns an SSA Package from the
// specified type-checked, error-free file ASTs, and populates its
// Members mapping.
//
// importable determines whether this package should be returned by a
// subsequent call to ImportedPackage(pkg.Path()).
//
// The real work of building SSA form for each function is not done
// until a subsequent call to Package.Build().
//
private static ptr<Package> CreatePackage(this ptr<Program> _addr_prog, ptr<types.Package> _addr_pkg, slice<ptr<ast.File>> files, ptr<types.Info> _addr_info, bool importable) {
    ref Program prog = ref _addr_prog.val;
    ref types.Package pkg = ref _addr_pkg.val;
    ref types.Info info = ref _addr_info.val;

    ptr<Package> p = addr(new Package(Prog:prog,Members:make(map[string]Member),values:make(map[types.Object]Value),Pkg:pkg,info:info,files:files,)); 

    // Add init() function.
    p.init = addr(new Function(name:"init",Signature:new(types.Signature),Synthetic:"package initializer",Pkg:p,Prog:prog,));
    p.Members[p.init.name] = p.init; 

    // CREATE phase.
    // Allocate all package members: vars, funcs, consts and types.
    if (len(files) > 0) { 
        // Go source package.
        foreach (var (_, file) in files) {
            foreach (var (_, decl) in file.Decls) {
                membersFromDecl(p, decl);
            }
    else
        }
    } { 
        // GC-compiled binary package (or "unsafe")
        // No code.
        // No position information.
        var scope = p.Pkg.Scope();
        foreach (var (_, name) in scope.Names()) {
            var obj = scope.Lookup(name);
            memberFromObject(p, obj, null);
            {
                var obj__prev2 = obj;

                ptr<types.TypeName> (obj, ok) = obj._<ptr<types.TypeName>>();

                if (ok) {
                    {
                        ptr<types.Named> (named, ok) = obj.Type()._<ptr<types.Named>>();

                        if (ok) {
                            for (nint i = 0;
                            var n = named.NumMethods(); i < n; i++) {
                                memberFromObject(p, named.Method(i), null);
                            }


                        }

                    }

                }

                obj = obj__prev2;

            }

        }
    }
    if (prog.mode & BareInits == 0) { 
        // Add initializer guard variable.
        ptr<Global> initguard = addr(new Global(Pkg:p,name:"init$guard",typ:types.NewPointer(tBool),));
        p.Members[initguard.Name()] = initguard;

    }
    if (prog.mode & GlobalDebug != 0) {
        p.SetDebugMode(true);
    }
    if (prog.mode & PrintPackages != 0) {
        printMu.Lock();
        p.WriteTo(os.Stdout);
        printMu.Unlock();
    }
    if (importable) {
        prog.imported[p.Pkg.Path()] = p;
    }
    prog.packages[p.Pkg] = p;

    return _addr_p!;

}

// printMu serializes printing of Packages/Functions to stdout.
private static sync.Mutex printMu = default;

// AllPackages returns a new slice containing all packages in the
// program prog in unspecified order.
//
private static slice<ptr<Package>> AllPackages(this ptr<Program> _addr_prog) {
    ref Program prog = ref _addr_prog.val;

    var pkgs = make_slice<ptr<Package>>(0, len(prog.packages));
    foreach (var (_, pkg) in prog.packages) {
        pkgs = append(pkgs, pkg);
    }    return pkgs;
}

// ImportedPackage returns the importable Package whose PkgPath
// is path, or nil if no such Package has been created.
//
// A parameter to CreatePackage determines whether a package should be
// considered importable. For example, no import declaration can resolve
// to the ad-hoc main package created by 'go build foo.go'.
//
// TODO(adonovan): rethink this function and the "importable" concept;
// most packages are importable. This function assumes that all
// types.Package.Path values are unique within the ssa.Program, which is
// false---yet this function remains very convenient.
// Clients should use (*Program).Package instead where possible.
// SSA doesn't really need a string-keyed map of packages.
//
private static ptr<Package> ImportedPackage(this ptr<Program> _addr_prog, @string path) {
    ref Program prog = ref _addr_prog.val;

    return _addr_prog.imported[path]!;
}

} // end ssa_package
