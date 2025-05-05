// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class pprof_package {

// runtime_FrameStartLine is defined in runtime/symtab.go.
//
//go:noescape
internal static partial nint runtime_FrameStartLine(ж<runtime.Frame> f);

// runtime_FrameSymbolName is defined in runtime/symtab.go.
//
//go:noescape
internal static partial @string runtime_FrameSymbolName(ж<runtime.Frame> f);

// runtime_expandFinalInlineFrame is defined in runtime/symtab.go.
internal static partial slice<uintptr> runtime_expandFinalInlineFrame(slice<uintptr> stk);

// runtime_setProfLabel is defined in runtime/proflabel.go.
internal static partial void runtime_setProfLabel(@unsafe.Pointer labels);

// runtime_getProfLabel is defined in runtime/proflabel.go.
internal static partial @unsafe.Pointer runtime_getProfLabel();

// SetGoroutineLabels sets the current goroutine's labels to match ctx.
// A new goroutine inherits the labels of the goroutine that created it.
// This is a lower-level API than [Do], which should be used instead when possible.
public static void SetGoroutineLabels(context.Context ctx) {
    var (ctxLabels, _) = ctx.Value(new labelContextKey(nil))._<labelMap.val>(ᐧ);
    runtime_setProfLabel(new @unsafe.Pointer(ctxLabels));
}

// Do calls f with a copy of the parent context with the
// given labels added to the parent's label map.
// Goroutines spawned while executing f will inherit the augmented label-set.
// Each key/value pair in labels is inserted into the label map in the
// order provided, overriding any previous value for the same key.
// The augmented label map will be set for the duration of the call to f
// and restored once f returns.
public static void Do(context.Context ctx, LabelSet labels, Action<context.Context> f) => func((defer, _) => {
    deferǃ(SetGoroutineLabels, ctx, defer);
    ctx = WithLabels(ctx, labels);
    SetGoroutineLabels(ctx);
    f(ctx);
});

} // end pprof_package
