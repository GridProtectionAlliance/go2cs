// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

//go:generate go run wincallback.go
//go:generate go run mkduff.go
//go:generate go run mkfastlog2table.go
//go:generate go run mklockrank.go -o lockrank.go
internal static ticksType ticks;

[GoType] partial struct ticksType {
    // lock protects access to start* and val.
    internal mutex @lock;
    internal int64 startTicks;
    internal int64 startTime;
    internal @internal.runtime.atomic_package.Int64 val;
}

// init initializes ticks to maximize the chance that we have a good ticksPerSecond reference.
//
// Must not run concurrently with ticksPerSecond.
[GoRecv] internal static void init(this ref ticksType t) {
    @lock(Ꮡticks.of(ticksType.Ꮡlock));
    t.startTime = nanotime();
    t.startTicks = cputicks();
    unlock(Ꮡticks.of(ticksType.Ꮡlock));
}

// minTimeForTicksPerSecond is the minimum elapsed time we require to consider our ticksPerSecond
// measurement to be of decent enough quality for profiling.
//
// There's a linear relationship here between minimum time and error from the true value.
// The error from the true ticks-per-second in a linux/amd64 VM seems to be:
// -   1 ms -> ~0.02% error
// -   5 ms -> ~0.004% error
// -  10 ms -> ~0.002% error
// -  50 ms -> ~0.0003% error
// - 100 ms -> ~0.0001% error
//
// We're willing to take 0.004% error here, because ticksPerSecond is intended to be used for
// converting durations, not timestamps. Durations are usually going to be much larger, and so
// the tiny error doesn't matter. The error is definitely going to be a problem when trying to
// use this for timestamps, as it'll make those timestamps much less likely to line up.
internal static readonly UntypedInt minTimeForTicksPerSecond = /* 5_000_000*(1-osHasLowResClockInt) + 100_000_000*osHasLowResClockInt */ 100000000;

// ticksPerSecond returns a conversion rate between the cputicks clock and the nanotime clock.
//
// Note: Clocks are hard. Using this as an actual conversion rate for timestamps is ill-advised
// and should be avoided when possible. Use only for durations, where a tiny error term isn't going
// to make a meaningful difference in even a 1ms duration. If an accurate timestamp is needed,
// use nanotime instead. (The entire Windows platform is a broad exception to this rule, where nanotime
// produces timestamps on such a coarse granularity that the error from this conversion is actually
// preferable.)
//
// The strategy for computing the conversion rate is to write down nanotime and cputicks as
// early in process startup as possible. From then, we just need to wait until we get values
// from nanotime that we can use (some platforms have a really coarse system time granularity).
// We require some amount of time to pass to ensure that the conversion rate is fairly accurate
// in aggregate. But because we compute this rate lazily, there's a pretty good chance a decent
// amount of time has passed by the time we get here.
//
// Must be called from a normal goroutine context (running regular goroutine with a P).
//
// Called by runtime/pprof in addition to runtime code.
//
// TODO(mknyszek): This doesn't account for things like CPU frequency scaling. Consider
// a more sophisticated and general approach in the future.
internal static int64 ticksPerSecond() {
    // Get the conversion rate if we've already computed it.
    var r = ticks.val.Load();
    if (r != 0) {
        return r;
    }
    // Compute the conversion rate.
    while (ᐧ) {
        @lock(Ꮡticks.of(ticksType.Ꮡlock));
        r = ticks.val.Load();
        if (r != 0) {
            unlock(Ꮡticks.of(ticksType.Ꮡlock));
            return r;
        }
        // Grab the current time in both clocks.
        var nowTime = nanotime();
        var nowTicks = cputicks();
        // See if we can use these times.
        if (nowTicks > ticks.startTicks && nowTime - ticks.startTime > minTimeForTicksPerSecond) {
            // Perform the calculation with floats. We don't want to risk overflow.
            r = ((int64)(((float64)(nowTicks - ticks.startTicks)) * 1e9F / ((float64)(nowTime - ticks.startTime))));
            if (r == 0) {
                // Zero is both a sentinel value and it would be bad if callers used this as
                // a divisor. We tried out best, so just make it 1.
                r++;
            }
            ticks.val.Store(r);
            unlock(Ꮡticks.of(ticksType.Ꮡlock));
            break;
        }
        unlock(Ꮡticks.of(ticksType.Ꮡlock));
        // Sleep in one millisecond increments until we have a reliable time.
        timeSleep(1000000);
    }
    return r;
}

internal static slice<@string> envs;

internal static slice<@string> argslice;

//go:linkname syscall_runtime_envs syscall.runtime_envs
internal static slice<@string> syscall_runtime_envs() {
    return append(new @string[]{}.slice(), envs.ꓸꓸꓸ);
}

//go:linkname syscall_Getpagesize syscall.Getpagesize
internal static nint syscall_Getpagesize() {
    return ((nint)physPageSize);
}

//go:linkname os_runtime_args os.runtime_args
internal static slice<@string> os_runtime_args() {
    return append(new @string[]{}.slice(), argslice.ꓸꓸꓸ);
}

//go:linkname syscall_Exit syscall.Exit
//go:nosplit
internal static void syscall_Exit(nint code) {
    exit(((int32)code));
}

internal static @string godebugDefault;

internal static atomic.Pointer<Action, string)> godebugUpdate;

internal static atomic.Pointer<@string> godebugEnv;                      // set by parsedebugvars

internal static atomic.Pointer<Func<@string, Action>> godebugNewIncNonDefault;

//go:linkname godebug_setUpdate internal/godebug.setUpdate
internal static void godebug_setUpdate(Action<@string, @string> update) {
    var Δp = @new<Action<@string, @string>>();
    Δp.val = update;
    godebugUpdate.Store(Δp);
    godebugNotify(false);
}

//go:linkname godebug_setNewIncNonDefault internal/godebug.setNewIncNonDefault
internal static void godebug_setNewIncNonDefault(Func<@string, Action> newIncNonDefault) {
    var Δp = @new<Func<@string, Action>>();
    Δp.val = newIncNonDefault;
    godebugNewIncNonDefault.Store(Δp);
}

// A godebugInc provides access to internal/godebug's IncNonDefault function
// for a given GODEBUG setting.
// Calls before internal/godebug registers itself are dropped on the floor.
[GoType] partial struct godebugInc {
    internal @string name;
    internal @internal.runtime.atomic_package.Pointer inc;
}

[GoRecv] internal static void IncNonDefault(this ref godebugInc g) {
    var inc = g.inc.Load();
    if (inc == nil) {
        var newInc = godebugNewIncNonDefault.Load();
        if (newInc == nil) {
            return;
        }
        inc = @new<Action>();
        inc.val = (ж<ж<Func<@string, Action>>>)(g.name);
        if (raceenabled) {
            racereleasemerge(new @unsafe.Pointer(Ꮡ(g.inc)));
        }
        if (!g.inc.CompareAndSwap(nil, inc)) {
            inc = g.inc.Load();
        }
    }
    if (raceenabled) {
        raceacquire(new @unsafe.Pointer(Ꮡ(g.inc)));
    }
    (ж<ж<Action>>)();
}

internal static void godebugNotify(bool envChanged) {
    var update = godebugUpdate.Load();
    @string env = default!;
    {
        var Δp = godebugEnv.Load(); if (Δp != nil) {
            env = Δp.val;
        }
    }
    if (envChanged) {
        reparsedebugvars(env);
    }
    if (update != nil) {
        (ж<ж<Action<@string, @string>>>)(godebugDefault, env);
    }
}

//go:linkname syscall_runtimeSetenv syscall.runtimeSetenv
internal static void syscall_runtimeSetenv(@string key, @string value) {
    setenv_c(key, value);
    if (key == "GODEBUG"u8) {
        var Δp = @new<@string>();
        Δp.val = value;
        godebugEnv.Store(Δp);
        godebugNotify(true);
    }
}

//go:linkname syscall_runtimeUnsetenv syscall.runtimeUnsetenv
internal static void syscall_runtimeUnsetenv(@string key) {
    unsetenv_c(key);
    if (key == "GODEBUG"u8) {
        godebugEnv.Store(nil);
        godebugNotify(true);
    }
}

// writeErrStr writes a string to descriptor 2.
// If SetCrashOutput(f) was called, it also writes to f.
//
//go:nosplit
internal static void writeErrStr(@string s) {
    writeErrData(@unsafe.StringData(s), ((int32)len(s)));
}

// writeErrData is the common parts of writeErr{,Str}.
//
//go:nosplit
internal static void writeErrData(ж<byte> Ꮡdata, int32 n) {
    ref var data = ref Ꮡdata.val;

    write(2, new @unsafe.Pointer(Ꮡdata), n);
    // If crashing, print a copy to the SetCrashOutput fd.
    var gp = getg();
    if (gp != nil && (~(~gp).m).dying > 0 || gp == nil && panicking.Load() > 0) {
        {
            var fd = crashFD.Load(); if (fd != ^((uintptr)0)) {
                write(fd, new @unsafe.Pointer(Ꮡdata), n);
            }
        }
    }
}

// crashFD is an optional file descriptor to use for fatal panics, as
// set by debug.SetCrashOutput (see #42888). If it is a valid fd (not
// all ones), writeErr and related functions write to it in addition
// to standard error.
//
// Initialized to -1 in schedinit.
internal static atomic.Uintptr crashFD;

//go:linkname setCrashFD
internal static uintptr setCrashFD(uintptr fd) {
    // Don't change the crash FD if a crash is already in progress.
    //
    // Unlike the case below, this is not required for correctness, but it
    // is generally nicer to have all of the crash output go to the same
    // place rather than getting split across two different FDs.
    if (panicking.Load() > 0) {
        return ^((uintptr)0);
    }
    var old = crashFD.Swap(fd);
    // If we are panicking, don't return the old FD to runtime/debug for
    // closing. writeErrData may have already read the old FD from crashFD
    // before the swap and closing it would cause the write to be lost [1].
    // The old FD will never be closed, but we are about to crash anyway.
    //
    // On the writeErrData thread, panicking.Add(1) happens-before
    // crashFD.Load() [2].
    //
    // On this thread, swapping old FD for new in crashFD happens-before
    // panicking.Load() > 0.
    //
    // Therefore, if panicking.Load() == 0 here (old FD will be closed), it
    // is impossible for the writeErrData thread to observe
    // crashFD.Load() == old FD.
    //
    // [1] Or, if really unlucky, another concurrent open could reuse the
    // FD, sending the write into an unrelated file.
    //
    // [2] If gp != nil, it occurs when incrementing gp.m.dying in
    // startpanic_m. If gp == nil, we read panicking.Load() > 0, so an Add
    // must have happened-before.
    if (panicking.Load() > 0) {
        return ^((uintptr)0);
    }
    return old;
}

// auxv is populated on relevant platforms but defined here for all platforms
// so x/sys/cpu can assume the getAuxv symbol exists without keeping its list
// of auxv-using GOOS build tags in sync.
//
// It contains an even number of elements, (tag, value) pairs.
internal static slice<uintptr> auxv;

// golang.org/x/sys/cpu uses getAuxv via linkname.
// Do not remove or change the type signature.
// (See go.dev/issue/57336.)
//
// getAuxv should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cilium/ebpf
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname getAuxv
internal static slice<uintptr> getAuxv() {
    return auxv;
}

// zeroVal is used by reflect via linkname.
//
// zeroVal should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ugorji/go/codec
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname zeroVal
internal static array<byte> zeroVal;

} // end runtime_package
