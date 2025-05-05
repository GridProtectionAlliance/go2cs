// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package exithook provides limited support for on-exit cleanup.
//
// CAREFUL! The expectation is that Add should only be called
// from a safe context (e.g. not an error/panic path or signal
// handler, preemption enabled, allocation allowed, write barriers
// allowed, etc), and that the exit function F will be invoked under
// similar circumstances. That is the say, we are expecting that F
// uses normal / high-level Go code as opposed to one of the more
// restricted dialects used for the trickier parts of the runtime.
namespace go.@internal.runtime;

using atomic = @internal.runtime.atomic_package;
using _ = unsafe_package; // for linkname

partial class exithook_package {

// A Hook is a function to be run at program termination
// (when someone invokes os.Exit, or when main.main returns).
// Hooks are run in reverse order of registration:
// the first hook added is the last one run.
[GoType] partial struct Hook {
    public Action F; // func to run
    public bool RunOnFailure;   // whether to run on non-zero exit code
}

internal static atomic.Int32 locked;
internal static atomic.Uint64 runGoid;
internal static slice<Hook> hooks;
internal static bool running;
public static Action Gosched;
public static Func<uint64> Goid;
public static Action<@string> Throw;

// Add adds a new exit hook.
public static void Add(Hook h) {
    while (!locked.CompareAndSwap(0, 1)) {
        Gosched();
    }
    hooks = append(hooks, h);
    locked.Store(0);
}

// Run runs the exit hooks.
//
// If an exit hook panics, Run will throw with the panic on the stack.
// If an exit hook invokes exit in the same goroutine, the goroutine will throw.
// If an exit hook invokes exit in another goroutine, that exit will block.
public static void Run(nint code) => func((defer, recover) => {
    while (!locked.CompareAndSwap(0, 1)) {
        if (Goid() == runGoid.Load()) {
            Throw("exit hook invoked exit"u8);
        }
        Gosched();
    }
    var lockedʗ1 = locked;
    deferǃ(lockedʗ1.Store, 0, defer);
    runGoid.Store(Goid());
    var runGoidʗ1 = runGoid;
    deferǃ(runGoidʗ1.Store, 0, defer);
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                Throw("exit hook invoked panic"u8);
            }
        }
    });
    while (len(hooks) > 0) {
        var h = hooks[len(hooks) - 1];
        hooks = hooks[..(int)(len(hooks) - 1)];
        if (code != 0 && !h.RunOnFailure) {
            continue;
        }
        h.F();
    }
});

[GoType("@string")] partial struct exitError;

internal static @string Error(this exitError e) {
    return ((@string)e);
}

} // end exithook_package
