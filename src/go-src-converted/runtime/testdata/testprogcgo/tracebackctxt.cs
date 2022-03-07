// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:19 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\tracebackctxt.go
// Test the context argument to SetCgoTraceback.
// Use fake context, traceback, and symbolizer functions.

/*
// Defined in tracebackctxt_c.c.
extern void C1(void);
extern void C2(void);
extern void tcContext(void*);
extern void tcContextSimple(void*);
extern void tcTraceback(void*);
extern void tcSymbolizer(void*);
extern int getContextCount(void);
extern void TracebackContextPreemptionCallGo(int);
*/
using C = go.C_package;// Test the context argument to SetCgoTraceback.
// Use fake context, traceback, and symbolizer functions.

/*
// Defined in tracebackctxt_c.c.
extern void C1(void);
extern void C2(void);
extern void tcContext(void*);
extern void tcContextSimple(void*);
extern void tcTraceback(void*);
extern void tcSymbolizer(void*);
extern int getContextCount(void);
extern void TracebackContextPreemptionCallGo(int);
*/


using fmt = go.fmt_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("TracebackContext", TracebackContext);
    register("TracebackContextPreemption", TracebackContextPreemption);
}

private static bool tracebackOK = default;

public static void TracebackContext() {
    runtime.SetCgoTraceback(0, @unsafe.Pointer(C.tcTraceback), @unsafe.Pointer(C.tcContext), @unsafe.Pointer(C.tcSymbolizer));
    C.C1();
    {
        var got = C.getContextCount();

        if (got != 0) {
            fmt.Printf("at end contextCount == %d, expected 0\n", got);
            tracebackOK = false;
        }
    }

    if (tracebackOK) {
        fmt.Println("OK");
    }
}

//export G1
public static void G1() {
    C.C2();
}

//export G2
public static void G2() {
    var pc = make_slice<System.UIntPtr>(32);
    var n = runtime.Callers(0, pc);
    var cf = runtime.CallersFrames(pc[..(int)n]);
    slice<runtime.Frame> frames = default;
    while (true) {
        var (frame, more) = cf.Next();
        frames = append(frames, frame);
        if (!more) {
            break;
        }
    }

    var ok = true;
    nint i = 0;
wantLoop:
    foreach (var (_, w) in want) {
        while (i < len(frames)) {
            if (w.function == frames[i].Function) {
                if (w.line != 0 && w.line != frames[i].Line) {
                    fmt.Printf("found function %s at wrong line %#x (expected %#x)\n", w.function, frames[i].Line, w.line);
                    ok = false;
            i++;
                }

                i++;
                _continuewantLoop = true;
                break;
            }

        }
        fmt.Printf("did not find function %s in\n", w.function);
        foreach (var (_, f) in frames) {
            fmt.Println(f);
        }        ok = false;
        break;

    }    tracebackOK = ok;
    {
        var got = C.getContextCount();

        if (got != 2) {
            fmt.Printf("at bottom contextCount == %d, expected 2\n", got);
            tracebackOK = false;
        }
    }

}

// Issue 47441.
public static void TracebackContextPreemption() => func((defer, _, _) => {
    runtime.SetCgoTraceback(0, @unsafe.Pointer(C.tcTraceback), @unsafe.Pointer(C.tcContextSimple), @unsafe.Pointer(C.tcSymbolizer));

    const nint funcs = 10;

    const float calls = 1e5F;

    sync.WaitGroup wg = default;
    for (nint i = 0; i < funcs; i++) {
        wg.Add(1);
        go_(() => i => {
            defer(wg.Done());
            for (nint j = 0; j < calls; j++) {
                C.TracebackContextPreemptionCallGo(C.@int(i * calls + j));
            }
        }(i));
    }
    wg.Wait();

    fmt.Println("OK");
});

//export TracebackContextPreemptionGoFunction
public static void TracebackContextPreemptionGoFunction(C.int i) { 
    // Do some busy work.
    fmt.Sprintf("%d\n", i);

}

} // end main_package
