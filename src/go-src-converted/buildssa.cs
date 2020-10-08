// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildssa defines an Analyzer that constructs the SSA
// representation of an error-free package and returns the set of all
// functions within it. It does not report any diagnostics itself but
// may be used as an input to other analyzers.
//
// THIS INTERFACE IS EXPERIMENTAL AND MAY BE SUBJECT TO INCOMPATIBLE CHANGE.
// package buildssa -- go2cs converted at 2020 October 08 04:56:34 UTC
// import "golang.org/x/tools/go/analysis/passes/buildssa" ==> using buildssa = go.golang.org.x.tools.go.analysis.passes.buildssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\buildssa\buildssa.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using reflect = go.reflect_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using ssa = go.golang.org.x.tools.go.ssa_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class buildssa_package
    {
        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"buildssa",Doc:"build SSA-form IR for later passes",Run:run,ResultType:reflect.TypeOf(new(SSA)),));

        // SSA provides SSA-form intermediate representation for all the
        // non-blank source functions in the current package.
        public partial struct SSA
        {
            public ptr<ssa.Package> Pkg;
            public slice<ptr<ssa.Function>> SrcFuncs;
        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass) => func((_, panic, __) =>
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;
 
            // Plundered from ssautil.BuildPackage.

            // We must create a new Program for each Package because the
            // analysis API provides no place to hang a Program shared by
            // all Packages. Consequently, SSA Packages and Functions do not
            // have a canonical representation across an analysis session of
            // multiple packages. This is unlikely to be a problem in
            // practice because the analysis API essentially forces all
            // packages to be analysed independently, so any given call to
            // Analysis.Run on a package will see only SSA objects belonging
            // to a single Program.

            // Some Analyzers may need GlobalDebug, in which case we'll have
            // to set it globally, but let's wait till we need it.
            var mode = ssa.BuilderMode(0L);

            var prog = ssa.NewProgram(pass.Fset, mode); 

            // Create SSA packages for all imports.
            // Order is not significant.
            var created = make_map<ptr<types.Package>, bool>();
            Action<slice<ptr<types.Package>>> createAll = default;
            createAll = pkgs =>
            {
                foreach (var (_, p) in pkgs)
                {
                    if (!created[p])
                    {
                        created[p] = true;
                        prog.CreatePackage(p, null, null, true);
                        createAll(p.Imports());
                    }

                }

            }
;
            createAll(pass.Pkg.Imports()); 

            // Create and build the primary package.
            var ssapkg = prog.CreatePackage(pass.Pkg, pass.Files, pass.TypesInfo, false);
            ssapkg.Build(); 

            // Compute list of source functions, including literals,
            // in source order.
            slice<ptr<ssa.Function>> funcs = default;
            {
                var f__prev1 = f;

                foreach (var (_, __f) in pass.Files)
                {
                    f = __f;
                    foreach (var (_, decl) in f.Decls)
                    {
                        {
                            ptr<ast.FuncDecl> (fdecl, ok) = decl._<ptr<ast.FuncDecl>>();

                            if (ok)
                            {
                                // SSA will not build a Function
                                // for a FuncDecl named blank.
                                // That's arguably too strict but
                                // relaxing it would break uniqueness of
                                // names of package members.
                                if (fdecl.Name.Name == "_")
                                {
                                    continue;
                                } 

                                // (init functions have distinct Func
                                // objects named "init" and distinct
                                // ssa.Functions named "init#1", ...)
                                ptr<types.Func> fn = pass.TypesInfo.Defs[fdecl.Name]._<ptr<types.Func>>();
                                if (fn == null)
                                {
                                    panic(fn);
                                }

                                var f = ssapkg.Prog.FuncValue(fn);
                                if (f == null)
                                {
                                    panic(fn);
                                }

                                Action<ptr<ssa.Function>> addAnons = default;
                                addAnons = f =>
                                {
                                    funcs = append(funcs, f);
                                    foreach (var (_, anon) in f.AnonFuncs)
                                    {
                                        addAnons(anon);
                                    }

                                }
;
                                addAnons(f);

                            }

                        }

                    }

                }

                f = f__prev1;
            }

            return (addr(new SSA(Pkg:ssapkg,SrcFuncs:funcs)), error.As(null!)!);

        });
    }
}}}}}}}
