// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:13:56 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\irgen.go
using fmt = go.fmt_package;
using os = go.os_package;

using @base = go.cmd.compile.@internal.@base_package;
using dwarfgen = go.cmd.compile.@internal.dwarfgen_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using types2 = go.cmd.compile.@internal.types2_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class noder_package {

    // check2 type checks a Go package using types2, and then generates IR
    // using the results.
private static void check2(slice<ptr<noder>> noders) {
    if (@base.SyntaxErrors() != 0) {
        @base.ErrorExit();
    }
    posMap m = default;
    var files = make_slice<ptr<syntax.File>>(len(noders));
    foreach (var (i, p) in noders) {
        m.join(_addr_p.posMap);
        files[i] = p.file;
    }    types2.Config conf = new types2.Config(GoVersion:base.Flag.Lang,IgnoreLabels:true,CompilerErrorMessages:true,Error:func(errerror){terr:=err.(types2.Error)base.ErrorfAt(m.makeXPos(terr.Pos),"%s",terr.Msg)},Importer:&gcimports{packages:make(map[string]*types2.Package),},Sizes:&gcSizes{},);
    ref types2.Info info = ref heap(new types2.Info(Types:make(map[syntax.Expr]types2.TypeAndValue),Defs:make(map[*syntax.Name]types2.Object),Uses:make(map[*syntax.Name]types2.Object),Selections:make(map[*syntax.SelectorExpr]*types2.Selection),Implicits:make(map[syntax.Node]types2.Object),Scopes:make(map[syntax.Node]*types2.Scope),Inferred:make(map[syntax.Expr]types2.Inferred),), out ptr<types2.Info> _addr_info);
    var (pkg, err) = conf.Check(@base.Ctxt.Pkgpath, files, _addr_info);
    files = null;
    @base.ExitIfErrors();
    if (err != null) {
        @base.FatalfAt(src.NoXPos, "conf.Check error: %v", err);
    }
    if (@base.Flag.G < 2) {
        os.Exit(0);
    }
    irgen g = new irgen(target:typecheck.Target,self:pkg,info:&info,posMap:m,objs:make(map[types2.Object]*ir.Name),typs:make(map[types2.Type]*types.Type),);
    g.generate(noders);

    if (@base.Flag.G < 3) {
        os.Exit(0);
    }
}

private partial struct irgen {
    public ptr<ir.Package> target;
    public ptr<types2.Package> self;
    public ptr<types2.Info> info;
    public ref posMap posMap => ref posMap_val;
    public map<types2.Object, ptr<ir.Name>> objs;
    public map<types2.Type, ptr<types.Type>> typs;
    public dwarfgen.ScopeMarker marker; // Fully-instantiated generic types whose methods should be instantiated
    public slice<ptr<types.Type>> instTypeList;
}

private static void generate(this ptr<irgen> _addr_g, slice<ptr<noder>> noders) {
    ref irgen g = ref _addr_g.val;

    types.LocalPkg.Name = g.self.Name();
    typecheck.TypecheckAllowed = true; 

    // Prevent size calculations until we set the underlying type
    // for all package-block defined types.
    types.DeferCheckSize(); 

    // At this point, types2 has already handled name resolution and
    // type checking. We just need to map from its object and type
    // representations to those currently used by the rest of the
    // compiler. This happens mostly in 3 passes.

    // 1. Process all import declarations. We use the compiler's own
    // importer for this, rather than types2's gcimporter-derived one,
    // to handle extensions and inline function bodies correctly.
    //
    // Also, we need to do this in a separate pass, because mappings are
    // instantiated on demand. If we interleaved processing import
    // declarations with other declarations, it's likely we'd end up
    // wanting to map an object/type from another source file, but not
    // yet have the import data it relies on.
    var declLists = make_slice<slice<syntax.Decl>>(len(noders));
Outer:
    {
        var i__prev1 = i;
        var p__prev1 = p;

        foreach (var (__i, __p) in noders) {
            i = __i;
            p = __p;
            g.pragmaFlags(p.file.Pragma, ir.GoBuildPragma);
            {
                var j__prev2 = j;
                var decl__prev2 = decl;

                foreach (var (__j, __decl) in p.file.DeclList) {
                    j = __j;
                    decl = __decl;
                    switch (decl.type()) {
                        case ptr<syntax.ImportDecl> decl:
                            g.importDecl(p, decl);
                            break;
                        default:
                        {
                            var decl = decl.type();
                            declLists[i] = p.file.DeclList[(int)j..];
                            _continueOuter = true; // no more ImportDecls
                            break;
                            break;
                        }
                    }

                }

                j = j__prev2;
                decl = decl__prev2;
            }
        }
        i = i__prev1;
        p = p__prev1;
    }
    types.LocalPkg.Height = myheight; 

    // 2. Process all package-block type declarations. As with imports,
    // we need to make sure all types are properly instantiated before
    // trying to map any expressions that utilize them. In particular,
    // we need to make sure type pragmas are already known (see comment
    // in irgen.typeDecl).
    //
    // We could perhaps instead defer processing of package-block
    // variable initializers and function bodies, like noder does, but
    // special-casing just package-block type declarations minimizes the
    // differences between processing package-block and function-scoped
    // declarations.
    {
        var declList__prev1 = declList;

        foreach (var (_, __declList) in declLists) {
            declList = __declList;
            {
                var decl__prev2 = decl;

                foreach (var (_, __decl) in declList) {
                    decl = __decl;
                    switch (decl.type()) {
                        case ptr<syntax.TypeDecl> decl:
                            g.typeDecl((ir.Nodes.val)(_addr_g.target.Decls), decl);
                            break;
                    }

                }

                decl = decl__prev2;
            }
        }
        declList = declList__prev1;
    }

    types.ResumeCheckSize(); 

    // 3. Process all remaining declarations.
    {
        var declList__prev1 = declList;

        foreach (var (_, __declList) in declLists) {
            declList = __declList;
            g.target.Decls = append(g.target.Decls, g.decls(declList));
        }
        declList = declList__prev1;
    }

    if (@base.Flag.W > 1) {
        foreach (var (_, n) in g.target.Decls) {
            var s = fmt.Sprintf("\nafter noder2 %v", n);
            ir.Dump(s, n);
        }
    }
    typecheck.DeclareUniverse();

    {
        var p__prev1 = p;

        foreach (var (_, __p) in noders) {
            p = __p; 
            // Process linkname and cgo pragmas.
            p.processPragmas(); 

            // Double check for any type-checking inconsistencies. This can be
            // removed once we're confident in IR generation results.
            syntax.Walk(p.file, n => {
                g.validate(n);
                return false;
            });

        }
        p = p__prev1;
    }

    g.stencil(); 

    // For now, remove all generic functions from g.target.Decl, since they
    // have been used for stenciling, but don't compile. TODO: We will
    // eventually export any exportable generic functions.
    nint j = 0;
    {
        var i__prev1 = i;
        var decl__prev1 = decl;

        foreach (var (__i, __decl) in g.target.Decls) {
            i = __i;
            decl = __decl;
            if (decl.Op() != ir.ODCLFUNC || !decl.Type().HasTParam()) {
                g.target.Decls[j] = g.target.Decls[i];
                j++;
            }
        }
        i = i__prev1;
        decl = decl__prev1;
    }

    g.target.Decls = g.target.Decls[..(int)j];

}

private static void unhandled(this ptr<irgen> _addr_g, @string what, poser p) => func((_, panic, _) => {
    ref irgen g = ref _addr_g.val;

    @base.FatalfAt(g.pos(p), "unhandled %s: %T", what, p);
    panic("unreachable");
});

} // end noder_package
