// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 22:47:34 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\base.go
using os = go.os_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class @base_package {

private static slice<Action> atExitFuncs = default;

public static void AtExit(Action f) {
    atExitFuncs = append(atExitFuncs, f);
}

public static void Exit(nint code) {
    for (var i = len(atExitFuncs) - 1; i >= 0; i--) {
        var f = atExitFuncs[i];
        atExitFuncs = atExitFuncs[..(int)i];
        f();
    }
    os.Exit(code);
}

// To enable tracing support (-t flag), set EnableTrace to true.
public static readonly var EnableTrace = false;



public static bool Compiling(slice<@string> pkgs) {
    if (Ctxt.Pkgpath != "") {
        foreach (var (_, p) in pkgs) {
            if (Ctxt.Pkgpath == p) {
                return true;
            }
        }
    }
    return false;

}

// The racewalk pass is currently handled in three parts.
//
// First, for flag_race, it inserts calls to racefuncenter and
// racefuncexit at the start and end (respectively) of each
// function. This is handled below.
//
// Second, during buildssa, it inserts appropriate instrumentation
// calls immediately before each memory load or store. This is handled
// by the (*state).instrument method in ssa.go, so here we just set
// the Func.InstrumentBody flag as needed. For background on why this
// is done during SSA construction rather than a separate SSA pass,
// see issue #19054.
//
// Third we remove calls to racefuncenter and racefuncexit, for leaf
// functions without instrumented operations. This is done as part of
// ssa opt pass via special rule.

// TODO(dvyukov): do not instrument initialization as writes:
// a := make([]int, 10)

// Do not instrument the following packages at all,
// at best instrumentation would cause infinite recursion.
public static @string NoInstrumentPkgs = new slice<@string>(new @string[] { "runtime/internal/atomic", "runtime/internal/sys", "runtime/internal/math", "runtime", "runtime/race", "runtime/msan", "internal/cpu" });

// Don't insert racefuncenter/racefuncexit into the following packages.
// Memory accesses in the packages are either uninteresting or will cause false positives.
public static @string NoRacePkgs = new slice<@string>(new @string[] { "sync", "sync/atomic" });

} // end @base_package
