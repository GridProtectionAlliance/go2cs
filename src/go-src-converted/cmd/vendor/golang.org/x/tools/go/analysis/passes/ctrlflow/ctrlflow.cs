// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ctrlflow is an analysis that provides a syntactic
// control-flow graph (CFG) for the body of a function.
// It records whether a function cannot return.
// By itself, it does not report any diagnostics.
// package ctrlflow -- go2cs converted at 2022 March 06 23:34:35 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/ctrlflow" ==> using ctrlflow = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.ctrlflow_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\ctrlflow\ctrlflow.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using log = go.log_package;
using reflect = go.reflect_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using cfg = go.golang.org.x.tools.go.cfg_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class ctrlflow_package {

public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"ctrlflow",Doc:"build a control-flow graph",Run:run,ResultType:reflect.TypeOf(new(CFGs)),FactTypes:[]analysis.Fact{new(noReturn)},Requires:[]*analysis.Analyzer{inspect.Analyzer},));

// noReturn is a fact indicating that a function does not return.
private partial struct noReturn {
}

private static void AFact(this ptr<noReturn> _addr__p0) {
    ref noReturn _p0 = ref _addr__p0.val;

}

private static @string String(this ptr<noReturn> _addr__p0) {
    ref noReturn _p0 = ref _addr__p0.val;

    return "noReturn";
}

// A CFGs holds the control-flow graphs
// for all the functions of the current package.
public partial struct CFGs {
    public map<ptr<ast.Ident>, types.Object> defs; // from Pass.TypesInfo.Defs
    public map<ptr<types.Func>, ptr<declInfo>> funcDecls;
    public map<ptr<ast.FuncLit>, ptr<litInfo>> funcLits;
    public ptr<analysis.Pass> pass; // transient; nil after construction
}

// CFGs has two maps: funcDecls for named functions and funcLits for
// unnamed ones. Unlike funcLits, the funcDecls map is not keyed by its
// syntax node, *ast.FuncDecl, because callMayReturn needs to do a
// look-up by *types.Func, and you can get from an *ast.FuncDecl to a
// *types.Func but not the other way.

private partial struct declInfo {
    public ptr<ast.FuncDecl> decl;
    public ptr<cfg.CFG> cfg; // iff decl.Body != nil
    public bool started; // to break cycles
    public bool noReturn;
}

private partial struct litInfo {
    public ptr<cfg.CFG> cfg;
    public bool noReturn;
}

// FuncDecl returns the control-flow graph for a named function.
// It returns nil if decl.Body==nil.
private static ptr<cfg.CFG> FuncDecl(this ptr<CFGs> _addr_c, ptr<ast.FuncDecl> _addr_decl) {
    ref CFGs c = ref _addr_c.val;
    ref ast.FuncDecl decl = ref _addr_decl.val;

    if (decl.Body == null) {
        return _addr_null!;
    }
    ptr<types.Func> fn = c.defs[decl.Name]._<ptr<types.Func>>();
    return _addr_c.funcDecls[fn].cfg!;

}

// FuncLit returns the control-flow graph for a literal function.
private static ptr<cfg.CFG> FuncLit(this ptr<CFGs> _addr_c, ptr<ast.FuncLit> _addr_lit) {
    ref CFGs c = ref _addr_c.val;
    ref ast.FuncLit lit = ref _addr_lit.val;

    return _addr_c.funcLits[lit].cfg!;
}

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>(); 

    // Because CFG construction consumes and produces noReturn
    // facts, CFGs for exported FuncDecls must be built before 'run'
    // returns; we cannot construct them lazily.
    // (We could build CFGs for FuncLits lazily,
    // but the benefit is marginal.)

    // Pass 1. Map types.Funcs to ast.FuncDecls in this package.
    var funcDecls = make_map<ptr<types.Func>, ptr<declInfo>>(); // functions and methods
    var funcLits = make_map<ptr<ast.FuncLit>, ptr<litInfo>>();

    slice<ptr<types.Func>> decls = default; // keys(funcDecls), in order
    slice<ptr<ast.FuncLit>> lits = default; // keys(funcLits), in order

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.FuncDecl)(nil), (*ast.FuncLit)(nil) });
    inspect.Preorder(nodeFilter, n => {
        switch (n.type()) {
            case ptr<ast.FuncDecl> n:
                {
                    ptr<types.Func> fn__prev1 = fn;

                    ptr<types.Func> (fn, ok) = pass.TypesInfo.Defs[n.Name]._<ptr<types.Func>>();

                    if (ok) {
                        funcDecls[fn] = addr(new declInfo(decl:n));
                        decls = append(decls, fn);
                    }

                    fn = fn__prev1;

                }

                break;
            case ptr<ast.FuncLit> n:
                funcLits[n] = @new<litInfo>();
                lits = append(lits, n);
                break;
        }

    });

    ptr<CFGs> c = addr(new CFGs(defs:pass.TypesInfo.Defs,funcDecls:funcDecls,funcLits:funcLits,pass:pass,)); 

    // Pass 2. Build CFGs.

    // Build CFGs for named functions.
    // Cycles in the static call graph are broken
    // arbitrarily but deterministically.
    // We create noReturn facts as discovered.
    {
        ptr<types.Func> fn__prev1 = fn;

        foreach (var (_, __fn) in decls) {
            fn = __fn;
            c.buildDecl(fn, funcDecls[fn]);
        }
        fn = fn__prev1;
    }

    foreach (var (_, lit) in lits) {
        var li = funcLits[lit];
        if (li.cfg == null) {
            li.cfg = cfg.New(lit.Body, c.callMayReturn);
            if (!hasReachableReturn(_addr_li.cfg)) {
                li.noReturn = true;
            }
        }
    }    c.pass = null;

    return (c, error.As(null!)!);

}

// di.cfg may be nil on return.
private static void buildDecl(this ptr<CFGs> _addr_c, ptr<types.Func> _addr_fn, ptr<declInfo> _addr_di) {
    ref CFGs c = ref _addr_c.val;
    ref types.Func fn = ref _addr_fn.val;
    ref declInfo di = ref _addr_di.val;
 
    // buildDecl may call itself recursively for the same function,
    // because cfg.New is passed the callMayReturn method, which
    // builds the CFG of the callee, leading to recursion.
    // The buildDecl call tree thus resembles the static call graph.
    // We mark each node when we start working on it to break cycles.

    if (!di.started) { // break cycle
        di.started = true;

        if (isIntrinsicNoReturn(_addr_fn)) {
            di.noReturn = true;
        }
        if (di.decl.Body != null) {
            di.cfg = cfg.New(di.decl.Body, c.callMayReturn);
            if (!hasReachableReturn(_addr_di.cfg)) {
                di.noReturn = true;
            }
        }
        if (di.noReturn) {
            c.pass.ExportObjectFact(fn, @new<noReturn>());
        }
        if (false) {
            log.Printf("CFG for %s:\n%s (noreturn=%t)\n", fn, di.cfg.Format(c.pass.Fset), di.noReturn);
        }
    }
}

// callMayReturn reports whether the called function may return.
// It is passed to the CFG builder.
private static bool callMayReturn(this ptr<CFGs> _addr_c, ptr<ast.CallExpr> _addr_call) {
    bool r = default;
    ref CFGs c = ref _addr_c.val;
    ref ast.CallExpr call = ref _addr_call.val;

    {
        ptr<ast.Ident> (id, ok) = call.Fun._<ptr<ast.Ident>>();

        if (ok && c.pass.TypesInfo.Uses[id] == panicBuiltin) {
            return false; // panic never returns
        }
    } 

    // Is this a static call?
    var fn = typeutil.StaticCallee(c.pass.TypesInfo, call);
    if (fn == null) {
        return true; // callee not statically known; be conservative
    }
    {
        var (di, ok) = c.funcDecls[fn];

        if (ok) {
            c.buildDecl(fn, di);
            return !di.noReturn;
        }
    } 

    // Not declared in this package.
    // Is there a fact from another package?
    return !c.pass.ImportObjectFact(fn, @new<noReturn>());

}

private static ptr<types.Builtin> panicBuiltin = types.Universe.Lookup("panic")._<ptr<types.Builtin>>();

private static bool hasReachableReturn(ptr<cfg.CFG> _addr_g) {
    ref cfg.CFG g = ref _addr_g.val;

    foreach (var (_, b) in g.Blocks) {
        if (b.Live && b.Return() != null) {
            return true;
        }
    }    return false;

}

// isIntrinsicNoReturn reports whether a function intrinsically never
// returns because it stops execution of the calling thread.
// It is the base case in the recursion.
private static bool isIntrinsicNoReturn(ptr<types.Func> _addr_fn) {
    ref types.Func fn = ref _addr_fn.val;
 
    // Add functions here as the need arises, but don't allocate memory.
    var path = fn.Pkg().Path();
    var name = fn.Name();
    return path == "syscall" && (name == "Exit" || name == "ExitProcess" || name == "ExitThread") || path == "runtime" && name == "Goexit";

}

} // end ctrlflow_package
