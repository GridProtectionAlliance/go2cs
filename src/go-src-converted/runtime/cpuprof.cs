// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// CPU profiling.
//
// The signal handler for the profiling clock tick adds a new stack trace
// to a log of recent traces. The log is read by a user goroutine that
// turns it into formatted profile data. If the reader does not keep up
// with the log, those writes will be recorded as a count of lost records.
// The actual profile buffer is in profbuf.go.

// package runtime -- go2cs converted at 2022 March 06 22:08:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cpuprof.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nint maxCPUProfStack = 64;



private partial struct cpuProfile {
    public mutex @lock;
    public bool on; // profiling is on
    public ptr<profBuf> log; // profile events written here

// extra holds extra stacks accumulated in addNonGo
// corresponding to profiling signals arriving on
// non-Go-created threads. Those stacks are written
// to log the next time a normal Go thread gets the
// signal handler.
// Assuming the stacks are 2 words each (we don't get
// a full traceback from those threads), plus one word
// size for framing, 100 Hz profiling would generate
// 300 words per second.
// Hopefully a normal Go thread will get the profiling
// signal at least once every few seconds.
    public array<System.UIntPtr> extra;
    public nint numExtra;
    public ulong lostExtra; // count of frames lost because extra is full
    public ulong lostAtomic; // count of frames lost because of being in atomic64 on mips/arm; updated racily
}

private static cpuProfile cpuprof = default;

// SetCPUProfileRate sets the CPU profiling rate to hz samples per second.
// If hz <= 0, SetCPUProfileRate turns off profiling.
// If the profiler is on, the rate cannot be changed without first turning it off.
//
// Most clients should use the runtime/pprof package or
// the testing package's -test.cpuprofile flag instead of calling
// SetCPUProfileRate directly.
public static void SetCPUProfileRate(nint hz) { 
    // Clamp hz to something reasonable.
    if (hz < 0) {
        hz = 0;
    }
    if (hz > 1000000) {
        hz = 1000000;
    }
    lock(_addr_cpuprof.@lock);
    if (hz > 0) {
        if (cpuprof.on || cpuprof.log != null) {
            print("runtime: cannot set cpu profile rate until previous profile has finished.\n");
            unlock(_addr_cpuprof.@lock);
            return ;
        }
        cpuprof.on = true;
        cpuprof.log = newProfBuf(1, 1 << 17, 1 << 14);
        array<ulong> hdr = new array<ulong>(new ulong[] { uint64(hz) });
        cpuprof.log.write(null, nanotime(), hdr[..], null);
        setcpuprofilerate(int32(hz));

    }
    else if (cpuprof.on) {
        setcpuprofilerate(0);
        cpuprof.on = false;
        cpuprof.addExtra();
        cpuprof.log.close();
    }
    unlock(_addr_cpuprof.@lock);

}

// add adds the stack trace to the profile.
// It is called from signal handlers and other limited environments
// and cannot allocate memory or acquire locks that might be
// held at the time of the signal, nor can it use substantial amounts
// of stack.
//go:nowritebarrierrec
private static void add(this ptr<cpuProfile> _addr_p, ptr<g> _addr_gp, slice<System.UIntPtr> stk) {
    ref cpuProfile p = ref _addr_p.val;
    ref g gp = ref _addr_gp.val;
 
    // Simple cas-lock to coordinate with setcpuprofilerate.
    while (!atomic.Cas(_addr_prof.signalLock, 0, 1)) {
        osyield();
    }

    if (prof.hz != 0) { // implies cpuprof.log != nil
        if (p.numExtra > 0 || p.lostExtra > 0 || p.lostAtomic > 0) {
            p.addExtra();
        }
        array<ulong> hdr = new array<ulong>(new ulong[] { 1 }); 
        // Note: write "knows" that the argument is &gp.labels,
        // because otherwise its write barrier behavior may not
        // be correct. See the long comment there before
        // changing the argument here.
        //
        // Note: it can happen on Windows, where we are calling
        // p.add with a gp that is not the current g, that gp is nil,
        // meaning we interrupted a system thread with no g.
        // Avoid faulting in that case.
        ptr<unsafe.Pointer> tagPtr;
        if (gp != null) {
            tagPtr = _addr_gp.labels;
        }
        cpuprof.log.write(tagPtr, nanotime(), hdr[..], stk);

    }
    atomic.Store(_addr_prof.signalLock, 0);

}

// addNonGo adds the non-Go stack trace to the profile.
// It is called from a non-Go thread, so we cannot use much stack at all,
// nor do anything that needs a g or an m.
// In particular, we can't call cpuprof.log.write.
// Instead, we copy the stack into cpuprof.extra,
// which will be drained the next time a Go thread
// gets the signal handling event.
//go:nosplit
//go:nowritebarrierrec
private static void addNonGo(this ptr<cpuProfile> _addr_p, slice<System.UIntPtr> stk) {
    ref cpuProfile p = ref _addr_p.val;
 
    // Simple cas-lock to coordinate with SetCPUProfileRate.
    // (Other calls to add or addNonGo should be blocked out
    // by the fact that only one SIGPROF can be handled by the
    // process at a time. If not, this lock will serialize those too.)
    while (!atomic.Cas(_addr_prof.signalLock, 0, 1)) {
        osyield();
    }

    if (cpuprof.numExtra + 1 + len(stk) < len(cpuprof.extra)) {
        var i = cpuprof.numExtra;
        cpuprof.extra[i] = uintptr(1 + len(stk));
        copy(cpuprof.extra[(int)i + 1..], stk);
        cpuprof.numExtra += 1 + len(stk);
    }
    else
 {
        cpuprof.lostExtra++;
    }
    atomic.Store(_addr_prof.signalLock, 0);

}

// addExtra adds the "extra" profiling events,
// queued by addNonGo, to the profile log.
// addExtra is called either from a signal handler on a Go thread
// or from an ordinary goroutine; either way it can use stack
// and has a g. The world may be stopped, though.
private static void addExtra(this ptr<cpuProfile> _addr_p) {
    ref cpuProfile p = ref _addr_p.val;
 
    // Copy accumulated non-Go profile events.
    array<ulong> hdr = new array<ulong>(new ulong[] { 1 });
    {
        nint i = 0;

        while (i < p.numExtra) {
            p.log.write(null, 0, hdr[..], p.extra[(int)i + 1..(int)i + int(p.extra[i])]);
            i += int(p.extra[i]);
        }
    }
    p.numExtra = 0; 

    // Report any lost events.
    if (p.lostExtra > 0) {
        hdr = new array<ulong>(new ulong[] { p.lostExtra });
        array<System.UIntPtr> lostStk = new array<System.UIntPtr>(new System.UIntPtr[] { funcPC(_LostExternalCode)+sys.PCQuantum, funcPC(_ExternalCode)+sys.PCQuantum });
        p.log.write(null, 0, hdr[..], lostStk[..]);
        p.lostExtra = 0;
    }
    if (p.lostAtomic > 0) {
        hdr = new array<ulong>(new ulong[] { p.lostAtomic });
        lostStk = new array<System.UIntPtr>(new System.UIntPtr[] { funcPC(_LostSIGPROFDuringAtomic64)+sys.PCQuantum, funcPC(_System)+sys.PCQuantum });
        p.log.write(null, 0, hdr[..], lostStk[..]);
        p.lostAtomic = 0;
    }
}

// CPUProfile panics.
// It formerly provided raw access to chunks of
// a pprof-format profile generated by the runtime.
// The details of generating that format have changed,
// so this functionality has been removed.
//
// Deprecated: Use the runtime/pprof package,
// or the handlers in the net/http/pprof package,
// or the testing package's -test.cpuprofile flag instead.
public static slice<byte> CPUProfile() => func((_, panic, _) => {
    panic("CPUProfile no longer available");
});

//go:linkname runtime_pprof_runtime_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond
private static long runtime_pprof_runtime_cyclesPerSecond() {
    return tickspersecond();
}

// readProfile, provided to runtime/pprof, returns the next chunk of
// binary CPU profiling stack trace data, blocking until data is available.
// If profiling is turned off and all the profile data accumulated while it was
// on has been returned, readProfile returns eof=true.
// The caller must save the returned data and tags before calling readProfile again.
//
//go:linkname runtime_pprof_readProfile runtime/pprof.readProfile
private static (slice<ulong>, slice<unsafe.Pointer>, bool) runtime_pprof_readProfile() {
    slice<ulong> _p0 = default;
    slice<unsafe.Pointer> _p0 = default;
    bool _p0 = default;

    lock(_addr_cpuprof.@lock);
    var log = cpuprof.log;
    unlock(_addr_cpuprof.@lock);
    var (data, tags, eof) = log.read(profBufBlocking);
    if (len(data) == 0 && eof) {
        lock(_addr_cpuprof.@lock);
        cpuprof.log = null;
        unlock(_addr_cpuprof.@lock);
    }
    return (data, tags, eof);

}

} // end runtime_package
