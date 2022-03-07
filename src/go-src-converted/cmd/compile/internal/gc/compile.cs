// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2022 March 06 23:11:25 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\compile.go
using race = go.@internal.race_package;
using rand = go.math.rand_package;
using sort = go.sort_package;
using sync = go.sync_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using liveness = go.cmd.compile.@internal.liveness_package;
using objw = go.cmd.compile.@internal.objw_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using walk = go.cmd.compile.@internal.walk_package;
using obj = go.cmd.@internal.obj_package;
using System;
using System.Threading;


namespace go.cmd.compile.@internal;

public static partial class gc_package {

    // "Portable" code generation.
private static slice<ptr<ir.Func>> compilequeue = default;

private static void enqueueFunc(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (ir.CurFunc != null) {
        @base.FatalfAt(fn.Pos(), "enqueueFunc %v inside %v", fn, ir.CurFunc);
    }
    if (ir.FuncName(fn) == "_") { 
        // Skip compiling blank functions.
        // Frontend already reported any spec-mandated errors (#29870).
        return ;

    }
    {
        var clo = fn.OClosure;

        if (clo != null && !ir.IsTrivialClosure(clo)) {
            return ; // we'll get this as part of its enclosing function
        }
    }


    if (len(fn.Body) == 0) { 
        // Initialize ABI wrappers if necessary.
        ssagen.InitLSym(fn, false);
        types.CalcSize(fn.Type());
        var a = ssagen.AbiForBodylessFuncStackMap(fn);
        var abiInfo = a.ABIAnalyzeFuncType(fn.Type().FuncType()); // abiInfo has spill/home locations for wrapper
        liveness.WriteFuncMap(fn, abiInfo);
        if (fn.ABI == obj.ABI0) {
            var x = ssagen.EmitArgInfo(fn, abiInfo);
            objw.Global(x, int32(len(x.P)), obj.RODATA | obj.LOCAL);
        }
        return ;

    }
    var errorsBefore = @base.Errors();

    ptr<ir.Func> todo = new slice<ptr<ir.Func>>(new ptr<ir.Func>[] { fn });
    while (len(todo) > 0) {
        var next = todo[len(todo) - 1];
        todo = todo[..(int)len(todo) - 1];

        prepareFunc(_addr_next);
        todo = append(todo, next.Closures);
    }

    if (@base.Errors() > errorsBefore) {
        return ;
    }
    compilequeue = append(compilequeue, fn);

}

// prepareFunc handles any remaining frontend compilation tasks that
// aren't yet safe to perform concurrently.
private static void prepareFunc(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;
 
    // Set up the function's LSym early to avoid data races with the assemblers.
    // Do this before walk, as walk needs the LSym to set attributes/relocations
    // (e.g. in MarkTypeUsedInInterface).
    ssagen.InitLSym(fn, true); 

    // Calculate parameter offsets.
    types.CalcSize(fn.Type());

    typecheck.DeclContext = ir.PAUTO;
    ir.CurFunc = fn;
    walk.Walk(fn);
    ir.CurFunc = null; // enforce no further uses of CurFunc
    typecheck.DeclContext = ir.PEXTERN;

}

// compileFunctions compiles all functions in compilequeue.
// It fans out nBackendWorkers to do the work
// and waits for them to complete.
private static void compileFunctions() {
    if (len(compilequeue) == 0) {
        return ;
    }
    if (race.Enabled) { 
        // Randomize compilation order to try to shake out races.
        var tmp = make_slice<ptr<ir.Func>>(len(compilequeue));
        var perm = rand.Perm(len(compilequeue));
        {
            var i__prev1 = i;

            foreach (var (__i, __v) in perm) {
                i = __i;
                v = __v;
                tmp[v] = compilequeue[i];
            }
    else

            i = i__prev1;
        }

        copy(compilequeue, tmp);

    } { 
        // Compile the longest functions first,
        // since they're most likely to be the slowest.
        // This helps avoid stragglers.
        sort.Slice(compilequeue, (i, j) => {
            return len(compilequeue[i].Body) > len(compilequeue[j].Body);
        });

    }
    Action<Action<nint>> queue = work => {
        work(0);
    };

    {
        var nWorkers = @base.Flag.LowerC;

        if (nWorkers > 1) { 
            // For concurrent builds, we create a goroutine per task, but
            // require them to hold a unique worker ID while performing work
            // to limit parallelism.
            var workerIDs = make_channel<nint>(nWorkers);
            {
                var i__prev1 = i;

                for (nint i = 0; i < nWorkers; i++) {
                    workerIDs.Send(i);
                }


                i = i__prev1;
            }

            queue = work => {
                go_(() => () => {
                    var worker = workerIDs.Receive();
                    work(worker);
                    workerIDs.Send(worker);
                }());
            }
;

        }
    }


    sync.WaitGroup wg = default;
    Action<slice<ptr<ir.Func>>> compile = default;
    compile = fns => {
        wg.Add(len(fns));
        {
            var fn__prev1 = fn;

            foreach (var (_, __fn) in fns) {
                fn = __fn;
                var fn = fn;
                queue(worker => {
                    ssagen.Compile(fn, worker);
                    compile(fn.Closures);
                    wg.Done();
                });
            }

            fn = fn__prev1;
        }
    };

    types.CalcSizeDisabled = true; // not safe to calculate sizes concurrently
    @base.Ctxt.InParallel = true;

    compile(compilequeue);
    compilequeue = null;
    wg.Wait();

    @base.Ctxt.InParallel = false;
    types.CalcSizeDisabled = false;

}

} // end gc_package
