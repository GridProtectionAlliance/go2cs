// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = sync.atomic_package;
using sync;

partial class sync_package {

// Once is an object that will perform exactly one action.
//
// A Once must not be copied after first use.
//
// In the terminology of [the Go memory model],
// the return from f “synchronizes before”
// the return from any call of once.Do(f).
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct Once {
    // done indicates whether the action has been performed.
    // It is first in the struct because it is used in the hot path.
    // The hot path is inlined at every call site.
    // Placing done first allows more compact instructions on some architectures (amd64/386),
    // and fewer instructions (to calculate offset) on other architectures.
    internal sync.atomic_package.Uint32 done;
    internal Mutex m;
}

// Do calls the function f if and only if Do is being called for the
// first time for this instance of [Once]. In other words, given
//
//	var once Once
//
// if once.Do(f) is called multiple times, only the first call will invoke f,
// even if f has a different value in each invocation. A new instance of
// Once is required for each function to execute.
//
// Do is intended for initialization that must be run exactly once. Since f
// is niladic, it may be necessary to use a function literal to capture the
// arguments to a function to be invoked by Do:
//
//	config.once.Do(func() { config.init(filename) })
//
// Because no call to Do returns until the one call to f returns, if f causes
// Do to be called, it will deadlock.
//
// If f panics, Do considers it to have returned; future calls of Do return
// without calling f.
[GoRecv] public static void Do(this ref Once o, Action f) {
    // Note: Here is an incorrect implementation of Do:
    //
    //	if o.done.CompareAndSwap(0, 1) {
    //		f()
    //	}
    //
    // Do guarantees that when it returns, f has finished.
    // This implementation would not implement that guarantee:
    // given two simultaneous calls, the winner of the cas would
    // call f, and the second would return immediately, without
    // waiting for the first's call to f to complete.
    // This is why the slow path falls back to a mutex, and why
    // the o.done.Store must be delayed until after f returns.
    if (o.done.Load() == 0) {
        // Outlined slow-path to allow inlining of the fast-path.
        o.doSlow(f);
    }
}

[GoRecv] internal static void doSlow(this ref Once o, Action f) => func((defer, _) => {
    o.m.Lock();
    defer(o.m.Unlock);
    if (o.done.Load() == 0) {
        deferǃ(o.done.Store, 1, defer);
        f();
    }
});

} // end sync_package
