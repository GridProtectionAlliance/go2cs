// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pkginit -- go2cs converted at 2022 March 13 06:25:39 UTC
// import "cmd/compile/internal/pkginit" ==> using pkginit = go.cmd.compile.@internal.pkginit_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\pkginit\init.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using deadcode = cmd.compile.@internal.deadcode_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;


// Task makes and returns an initialization record for the package.
// See runtime/proc.go:initTask for its layout.
// The 3 tasks for initialization are:
//   1) Initialize all of the packages the current package depends on.
//   2) Initialize all the variables that have initializers.
//   3) Run any init functions.

public static partial class pkginit_package {

public static ptr<ir.Name> Task() {
    var nf = initOrder(typecheck.Target.Decls);

    slice<ptr<obj.LSym>> deps = default; // initTask records for packages the current package depends on
    slice<ptr<obj.LSym>> fns = default; // functions to call for package initialization

    // Find imported packages with init tasks.
    foreach (var (_, pkg) in typecheck.Target.Imports) {
        var n = typecheck.Resolve(ir.NewIdent(@base.Pos, pkg.Lookup(".inittask")));
        if (n.Op() == ir.ONONAME) {
            continue;
        }
        if (n.Op() != ir.ONAME || n._<ptr<ir.Name>>().Class != ir.PEXTERN) {
            @base.Fatalf("bad inittask: %v", n);
        }
        deps = append(deps, n._<ptr<ir.Name>>().Linksym());
    }    if (len(nf) > 0) {
        @base.Pos = nf[0].Pos(); // prolog/epilog gets line number of first init stmt
        var initializers = typecheck.Lookup("init");
        var fn = typecheck.DeclFunc(initializers, ir.NewFuncType(@base.Pos, null, null, null));
        foreach (var (_, dcl) in typecheck.InitTodoFunc.Dcl) {
            dcl.Curfn = fn;
        }        fn.Dcl = append(fn.Dcl, typecheck.InitTodoFunc.Dcl);
        typecheck.InitTodoFunc.Dcl = null;

        fn.Body = nf;
        typecheck.FinishFuncBody();

        typecheck.Func(fn);
        ir.CurFunc = fn;
        typecheck.Stmts(nf);
        ir.CurFunc = null;
        typecheck.Target.Decls = append(typecheck.Target.Decls, fn);
        fns = append(fns, fn.Linksym());
    }
    if (typecheck.InitTodoFunc.Dcl != null) { 
        // We only generate temps using InitTodoFunc if there
        // are package-scope initialization statements, so
        // something's weird if we get here.
        @base.Fatalf("InitTodoFunc still has declarations");
    }
    typecheck.InitTodoFunc = null; 

    // Record user init functions.
    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in typecheck.Target.Inits) {
            fn = __fn; 
            // Must happen after initOrder; see #43444.
            deadcode.Func(fn); 

            // Skip init functions with empty bodies.
            if (len(fn.Body) == 1) {
                {
                    var stmt = fn.Body[0];

                    if (stmt.Op() == ir.OBLOCK && len(stmt._<ptr<ir.BlockStmt>>().List) == 0) {
                        continue;
                    }
                }
            }
            fns = append(fns, fn.Nname.Linksym());
        }
        fn = fn__prev1;
    }

    if (len(deps) == 0 && len(fns) == 0 && types.LocalPkg.Name != "main" && types.LocalPkg.Name != "runtime") {
        return _addr_null!; // nothing to initialize
    }
    var sym = typecheck.Lookup(".inittask");
    var task = typecheck.NewName(sym);
    task.SetType(types.Types[types.TUINT8]); // fake type
    task.Class = ir.PEXTERN;
    sym.Def = task;
    var lsym = task.Linksym();
    nint ot = 0;
    ot = objw.Uintptr(lsym, ot, 0); // state: not initialized yet
    ot = objw.Uintptr(lsym, ot, uint64(len(deps)));
    ot = objw.Uintptr(lsym, ot, uint64(len(fns)));
    foreach (var (_, d) in deps) {
        ot = objw.SymPtr(lsym, ot, d, 0);
    }    foreach (var (_, f) in fns) {
        ot = objw.SymPtr(lsym, ot, f, 0);
    }    objw.Global(lsym, int32(ot), obj.NOPTR);
    return _addr_task!;
}

} // end pkginit_package
