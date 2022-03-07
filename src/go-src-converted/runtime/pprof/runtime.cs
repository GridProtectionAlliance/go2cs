// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 06 22:15:10 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\runtime.go
using context = go.context_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.runtime;

public static partial class pprof_package {

    // runtime_expandFinalInlineFrame is defined in runtime/symtab.go.
private static slice<System.UIntPtr> runtime_expandFinalInlineFrame(slice<System.UIntPtr> stk);

// runtime_setProfLabel is defined in runtime/proflabel.go.
private static void runtime_setProfLabel(unsafe.Pointer labels);

// runtime_getProfLabel is defined in runtime/proflabel.go.
private static unsafe.Pointer runtime_getProfLabel();

// SetGoroutineLabels sets the current goroutine's labels to match ctx.
// A new goroutine inherits the labels of the goroutine that created it.
// This is a lower-level API than Do, which should be used instead when possible.
public static void SetGoroutineLabels(context.Context ctx) {
    ptr<labelMap> (ctxLabels, _) = ctx.Value(new labelContextKey())._<ptr<labelMap>>();
    runtime_setProfLabel(@unsafe.Pointer(ctxLabels));
}

// Do calls f with a copy of the parent context with the
// given labels added to the parent's label map.
// Goroutines spawned while executing f will inherit the augmented label-set.
// Each key/value pair in labels is inserted into the label map in the
// order provided, overriding any previous value for the same key.
// The augmented label map will be set for the duration of the call to f
// and restored once f returns.
public static void Do(context.Context ctx, LabelSet labels, Action<context.Context> f) => func((defer, _, _) => {
    defer(SetGoroutineLabels(ctx));
    ctx = WithLabels(ctx, labels);
    SetGoroutineLabels(ctx);
    f(ctx);
});

} // end pprof_package
