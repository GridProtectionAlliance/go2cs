// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pprof writes runtime profiling data in the format expected
// by the pprof visualization tool.
//
// # Profiling a Go program
//
// The first step to profiling a Go program is to enable profiling.
// Support for profiling benchmarks built with the standard testing
// package is built into go test. For example, the following command
// runs benchmarks in the current directory and writes the CPU and
// memory profiles to cpu.prof and mem.prof:
//
//	go test -cpuprofile cpu.prof -memprofile mem.prof -bench .
//
// To add equivalent profiling support to a standalone program, add
// code like the following to your main function:
//
//	var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to `file`")
//	var memprofile = flag.String("memprofile", "", "write memory profile to `file`")
//
//	func main() {
//	    flag.Parse()
//	    if *cpuprofile != "" {
//	        f, err := os.Create(*cpuprofile)
//	        if err != nil {
//	            log.Fatal("could not create CPU profile: ", err)
//	        }
//	        defer f.Close() // error handling omitted for example
//	        if err := pprof.StartCPUProfile(f); err != nil {
//	            log.Fatal("could not start CPU profile: ", err)
//	        }
//	        defer pprof.StopCPUProfile()
//	    }
//
//	    // ... rest of the program ...
//
//	    if *memprofile != "" {
//	        f, err := os.Create(*memprofile)
//	        if err != nil {
//	            log.Fatal("could not create memory profile: ", err)
//	        }
//	        defer f.Close() // error handling omitted for example
//	        runtime.GC() // get up-to-date statistics
//	        if err := pprof.WriteHeapProfile(f); err != nil {
//	            log.Fatal("could not write memory profile: ", err)
//	        }
//	    }
//	}
//
// There is also a standard HTTP interface to profiling data. Adding
// the following line will install handlers under the /debug/pprof/
// URL to download live profiles:
//
//	import _ "net/http/pprof"
//
// See the net/http/pprof package for more details.
//
// Profiles can then be visualized with the pprof tool:
//
//	go tool pprof cpu.prof
//
// There are many commands available from the pprof command line.
// Commonly used commands include "top", which prints a summary of the
// top program hot-spots, and "web", which opens an interactive graph
// of hot-spots and their call graphs. Use "help" for information on
// all pprof commands.
//
// For more information about pprof, see
// https://github.com/google/pprof/blob/main/doc/README.md.
namespace go.runtime;

using bufio = bufio_package;
using cmp = cmp_package;
using fmt = fmt_package;
using abi = @internal.abi_package;
using profilerecord = @internal.profilerecord_package;
using io = io_package;
using runtime = runtime_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using tabwriter = text.tabwriter_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using text;

partial class pprof_package {

// BUG(rsc): Profiles are only as good as the kernel support used to generate them.
// See https://golang.org/issue/13841 for details about known problems.

// A Profile is a collection of stack traces showing the call sequences
// that led to instances of a particular event, such as allocation.
// Packages can create and maintain their own profiles; the most common
// use is for tracking resources that must be explicitly closed, such as files
// or network connections.
//
// A Profile's methods can be called from multiple goroutines simultaneously.
//
// Each Profile has a unique name. A few profiles are predefined:
//
//	goroutine    - stack traces of all current goroutines
//	heap         - a sampling of memory allocations of live objects
//	allocs       - a sampling of all past memory allocations
//	threadcreate - stack traces that led to the creation of new OS threads
//	block        - stack traces that led to blocking on synchronization primitives
//	mutex        - stack traces of holders of contended mutexes
//
// These predefined profiles maintain themselves and panic on an explicit
// [Profile.Add] or [Profile.Remove] method call.
//
// The CPU profile is not available as a Profile. It has a special API,
// the [StartCPUProfile] and [StopCPUProfile] functions, because it streams
// output to a writer during profiling.
//
// # Heap profile
//
// The heap profile reports statistics as of the most recently completed
// garbage collection; it elides more recent allocation to avoid skewing
// the profile away from live data and toward garbage.
// If there has been no garbage collection at all, the heap profile reports
// all known allocations. This exception helps mainly in programs running
// without garbage collection enabled, usually for debugging purposes.
//
// The heap profile tracks both the allocation sites for all live objects in
// the application memory and for all objects allocated since the program start.
// Pprof's -inuse_space, -inuse_objects, -alloc_space, and -alloc_objects
// flags select which to display, defaulting to -inuse_space (live objects,
// scaled by size).
//
// # Allocs profile
//
// The allocs profile is the same as the heap profile but changes the default
// pprof display to -alloc_space, the total number of bytes allocated since
// the program began (including garbage-collected bytes).
//
// # Block profile
//
// The block profile tracks time spent blocked on synchronization primitives,
// such as [sync.Mutex], [sync.RWMutex], [sync.WaitGroup], [sync.Cond], and
// channel send/receive/select.
//
// Stack traces correspond to the location that blocked (for example,
// [sync.Mutex.Lock]).
//
// Sample values correspond to cumulative time spent blocked at that stack
// trace, subject to time-based sampling specified by
// [runtime.SetBlockProfileRate].
//
// # Mutex profile
//
// The mutex profile tracks contention on mutexes, such as [sync.Mutex],
// [sync.RWMutex], and runtime-internal locks.
//
// Stack traces correspond to the end of the critical section causing
// contention. For example, a lock held for a long time while other goroutines
// are waiting to acquire the lock will report contention when the lock is
// finally unlocked (that is, at [sync.Mutex.Unlock]).
//
// Sample values correspond to the approximate cumulative time other goroutines
// spent blocked waiting for the lock, subject to event-based sampling
// specified by [runtime.SetMutexProfileFraction]. For example, if a caller
// holds a lock for 1s while 5 other goroutines are waiting for the entire
// second to acquire the lock, its unlock call stack will report 5s of
// contention.
//
// Runtime-internal locks are always reported at the location
// "runtime._LostContendedRuntimeLock". More detailed stack traces for
// runtime-internal locks can be obtained by setting
// `GODEBUG=runtimecontentionstacks=1` (see package [runtime] docs for
// caveats).
[GoType] partial struct Profile {
    internal @string name;
    internal sync_package.Mutex mu;
    internal map<any, slice<uintptr>> m;
    internal Func<nint> count;
    internal Func<io.Writer, nint, error> write;
}

// profiles records all registered profiles.

[GoType("dyn")] partial struct profilesᴛ1 {
    internal sync_package.Mutex mu;
    internal map<@string, ж<Profile>> m;
}
internal static profilesᴛ1 profiles;

internal static ж<Profile> goroutineProfile = Ꮡ(new Profile(
    name: "goroutine"u8,
    count: countGoroutine,
    write: writeGoroutine
));

internal static ж<Profile> threadcreateProfile = Ꮡ(new Profile(
    name: "threadcreate"u8,
    count: countThreadCreate,
    write: writeThreadCreate
));

internal static ж<Profile> heapProfile = Ꮡ(new Profile(
    name: "heap"u8,
    count: countHeap,
    write: writeHeap
));

// identical to heap profile
internal static ж<Profile> allocsProfile = Ꮡ(new Profile(
    name: "allocs"u8,
    count: countHeap,
    write: writeAlloc
));

internal static ж<Profile> blockProfile = Ꮡ(new Profile(
    name: "block"u8,
    count: countBlock,
    write: writeBlock
));

internal static ж<Profile> mutexProfile = Ꮡ(new Profile(
    name: "mutex"u8,
    count: countMutex,
    write: writeMutex
));

internal static void lockProfiles() {
    profiles.mu.Lock();
    if (profiles.m == default!) {
        // Initial built-in profiles.
        profiles.m = new map<@string, ж<Profile>>{
            ["goroutine"u8] = goroutineProfile,
            ["threadcreate"u8] = threadcreateProfile,
            ["heap"u8] = heapProfile,
            ["allocs"u8] = allocsProfile,
            ["block"u8] = blockProfile,
            ["mutex"u8] = mutexProfile
        };
    }
}

internal static void unlockProfiles() {
    profiles.mu.Unlock();
}

// NewProfile creates a new profile with the given name.
// If a profile with that name already exists, NewProfile panics.
// The convention is to use a 'import/path.' prefix to create
// separate name spaces for each package.
// For compatibility with various tools that read pprof data,
// profile names should not contain spaces.
public static ж<Profile> NewProfile(@string name) => func((defer, _) => {
    lockProfiles();
    defer(unlockProfiles);
    if (name == ""u8) {
        throw panic("pprof: NewProfile with empty name");
    }
    if (profiles.m[name] != nil) {
        throw panic("pprof: NewProfile name already in use: "u8 + name);
    }
    var p = Ꮡ(new Profile(
        name: name,
        m: new map<any, slice<uintptr>>{}
    ));
    profiles.m[name] = p;
    return p;
});

// Lookup returns the profile with the given name, or nil if no such profile exists.
public static ж<Profile> Lookup(@string name) => func((defer, _) => {
    lockProfiles();
    defer(unlockProfiles);
    return profiles.m[name];
});

// Profiles returns a slice of all the known profiles, sorted by name.
public static slice<ж<Profile>> Profiles() => func((defer, _) => {
    lockProfiles();
    defer(unlockProfiles);
    var all = new slice<ж<Profile>>(0, len(profiles.m));
    foreach (var (_, p) in profiles.m) {
        all = append(all, p);
    }
    slices.SortFunc(all, (ж<Profile> a, ж<Profile> b) => strings.Compare((~a).name, (~b).name));
    return all;
});

// Name returns this profile's name, which can be passed to [Lookup] to reobtain the profile.
[GoRecv] public static @string Name(this ref Profile p) {
    return p.name;
}

// Count returns the number of execution stacks currently in the profile.
[GoRecv] public static nint Count(this ref Profile p) => func((defer, _) => {
    p.mu.Lock();
    defer(p.mu.Unlock);
    if (p.count != default!) {
        return p.count();
    }
    return len(p.m);
});

// Add adds the current execution stack to the profile, associated with value.
// Add stores value in an internal map, so value must be suitable for use as
// a map key and will not be garbage collected until the corresponding
// call to [Profile.Remove]. Add panics if the profile already contains a stack for value.
//
// The skip parameter has the same meaning as [runtime.Caller]'s skip
// and controls where the stack trace begins. Passing skip=0 begins the
// trace in the function calling Add. For example, given this
// execution stack:
//
//	Add
//	called from rpc.NewClient
//	called from mypkg.Run
//	called from main.main
//
// Passing skip=0 begins the stack trace at the call to Add inside rpc.NewClient.
// Passing skip=1 begins the stack trace at the call to NewClient inside mypkg.Run.
[GoRecv] public static void Add(this ref Profile p, any value, nint skip) => func((defer, _) => {
    if (p.name == ""u8) {
        throw panic("pprof: use of uninitialized Profile");
    }
    if (p.write != default!) {
        throw panic("pprof: Add called on built-in Profile "u8 + p.name);
    }
    var stk = new slice<uintptr>(32);
    nint n = runtime.Callers(skip + 1, stk[..]);
    stk = stk[..(int)(n)];
    if (len(stk) == 0) {
        // The value for skip is too large, and there's no stack trace to record.
        stk = new uintptr[]{abi.FuncPCABIInternal(lostProfileEvent)}.slice();
    }
    p.mu.Lock();
    defer(p.mu.Unlock);
    if (p.m[value] != default!) {
        throw panic("pprof: Profile.Add of duplicate value");
    }
    p.m[value] = stk;
});

// Remove removes the execution stack associated with value from the profile.
// It is a no-op if the value is not in the profile.
[GoRecv] public static void Remove(this ref Profile p, any value) => func((defer, _) => {
    p.mu.Lock();
    defer(p.mu.Unlock);
    delete(p.m, value);
});

// WriteTo writes a pprof-formatted snapshot of the profile to w.
// If a write to w returns an error, WriteTo returns that error.
// Otherwise, WriteTo returns nil.
//
// The debug parameter enables additional output.
// Passing debug=0 writes the gzip-compressed protocol buffer described
// in https://github.com/google/pprof/tree/main/proto#overview.
// Passing debug=1 writes the legacy text format with comments
// translating addresses to function names and line numbers, so that a
// programmer can read the profile without tools.
//
// The predefined profiles may assign meaning to other debug values;
// for example, when printing the "goroutine" profile, debug=2 means to
// print the goroutine stacks in the same form that a Go program uses
// when dying due to an unrecovered panic.
[GoRecv] public static error WriteTo(this ref Profile p, io.Writer w, nint debug) {
    if (p.name == ""u8) {
        throw panic("pprof: use of zero Profile");
    }
    if (p.write != default!) {
        return p.write(w, debug);
    }
    // Obtain consistent snapshot under lock; then process without lock.
    p.mu.Lock();
    var all = new slice<slice<uintptr>>(0, len(p.m));
    foreach (var (_, stk) in p.m) {
        all = append(all, stk);
    }
    p.mu.Unlock();
    // Map order is non-deterministic; make output deterministic.
    slices.SortFunc(all, slices.Compare);
    return printCountProfile(w, debug, p.name, ((stackProfile)all));
}

[GoType("[]uintptr")] partial struct stackProfile;

internal static nint Len(this stackProfile x) {
    return len(x);
}

internal static slice<uintptr> Stack(this stackProfile x, nint i) {
    return x[i];
}

internal static ж<labelMap> Label(this stackProfile x, nint i) {
    return default!;
}

// A countProfile is a set of stack traces to be printed as counts
// grouped by stack trace. There are multiple implementations:
// all that matters is that we can find out how many traces there are
// and obtain each trace in turn.
[GoType] partial interface countProfile {
    nint Len();
    slice<uintptr> Stack(nint i);
    ж<labelMap> Label(nint i);
}

// expandInlinedFrames copies the call stack from pcs into dst, expanding any
// PCs corresponding to inlined calls into the corresponding PCs for the inlined
// functions. Returns the number of frames copied to dst.
internal static nint expandInlinedFrames(slice<uintptr> dst, slice<uintptr> pcs) {
    var cf = runtime.CallersFrames(pcs);
    nint n = default!;
    while (n < len(dst)) {
        var (f, more) = cf.Next();
        // f.PC is a "call PC", but later consumers will expect
        // "return PCs"
        dst[n] = f.PC + 1;
        n++;
        if (!more) {
            break;
        }
    }
    return n;
}

// printCountCycleProfile outputs block profile records (for block or mutex profiles)
// as the pprof-proto format output. Translations from cycle count to time duration
// are done because The proto expects count and time (nanoseconds) instead of count
// and the number of cycles for block, contention profiles.
internal static error printCountCycleProfile(io.Writer w, @string countName, @string cycleName, slice<profilerecord.BlockProfileRecord> records) {
    // Output profile in protobuf form.
    var b = newProfileBuilder(w);
    b.pbValueType(tagProfile_PeriodType, countName, "count"u8);
    (~b).pb.int64Opt(tagProfile_Period, 1);
    b.pbValueType(tagProfile_SampleType, countName, "count"u8);
    b.pbValueType(tagProfile_SampleType, cycleName, "nanoseconds"u8);
    var cpuGHz = ((float64)pprof_cyclesPerSecond()) / 1e9F;
    var values = new int64[]{0, 0}.slice();
    slice<uint64> locs = default!;
    var expandedStack = pprof_makeProfStack();
    foreach (var (_, r) in records) {
        values[0] = r.Count;
        values[1] = ((int64)(((float64)r.Cycles) / cpuGHz));
        // For count profiles, all stack addresses are
        // return PCs, which is what appendLocsForStack expects.
        nint n = expandInlinedFrames(expandedStack, r.Stack);
        locs = b.appendLocsForStack(locs[..0], expandedStack[..(int)(n)]);
        b.pbSample(values, locs, default!);
    }
    b.build();
    return default!;
}

// printCountProfile prints a countProfile at the specified debug level.
// The profile will be in compressed proto format unless debug is nonzero.
internal static error printCountProfile(io.Writer w, nint debug, @string name, countProfile p) {
    // Build count of each stack.
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    var key = 
    var bufʗ1 = buf;
    (slice<uintptr> stk, ж<labelMap> lbls) => {
        bufʗ1.Reset();
        fmt.Fprintf(~Ꮡbufʗ1, "@"u8);
        foreach (var (_, pc) in stk) {
            fmt.Fprintf(~Ꮡbufʗ1, " %#x"u8, pc);
        }
        if (lbls != nil) {
            bufʗ1.WriteString("\n# labels: "u8);
            bufʗ1.WriteString(lbls.String());
        }
        return bufʗ1.String();
    };
    var count = new map<@string, nint>{};
    var index = new map<@string, nint>{};
    slice<@string> keys = default!;
    nint n = p.Len();
    for (nint i = 0; i < n; i++) {
        @string k = key(p.Stack(i), p.Label(i));
        if (count[k] == 0) {
            index[k] = i;
            keys = append(keys, k);
        }
        count[k]++;
    }
    sort.Sort(new keysByCount(keys, count));
    if (debug > 0) {
        // Print debug profile in legacy format
        var tw = tabwriter.NewWriter(w, 1, 8, 1, (rune)'\t', 0);
        fmt.Fprintf(~tw, "%s profile: total %d\n"u8, name, p.Len());
        foreach (var (_, k) in keys) {
            fmt.Fprintf(~tw, "%d %s\n"u8, count[k], k);
            printStackRecord(~tw, p.Stack(index[k]), false);
        }
        return tw.Flush();
    }
    // Output profile in protobuf form.
    var b = newProfileBuilder(w);
    b.pbValueType(tagProfile_PeriodType, name, "count"u8);
    (~b).pb.int64Opt(tagProfile_Period, 1);
    b.pbValueType(tagProfile_SampleType, name, "count"u8);
    var values = new int64[]{0}.slice();
    slice<uint64> locs = default!;
    foreach (var (_, k) in keys) {
        values[0] = ((int64)count[k]);
        // For count profiles, all stack addresses are
        // return PCs, which is what appendLocsForStack expects.
        locs = b.appendLocsForStack(locs[..0], p.Stack(index[k]));
        nint idx = index[k];
        Action labels = default!;
        if (p.Label(idx) != nil) {
            labels = 
            var bʗ1 = b;
            () => {
                foreach (var (kΔ1, v) in p.Label(idx).val) {
                    bʗ1.pbLabel(tagSample_Label, kΔ1, v, 0);
                }
            };
        }
        b.pbSample(values, locs, labels);
    }
    b.build();
    return default!;
}

// keysByCount sorts keys with higher counts first, breaking ties by key string order.
[GoType] partial struct keysByCount {
    internal slice<@string> keys;
    internal map<@string, nint> count;
}

[GoRecv] internal static nint Len(this ref keysByCount x) {
    return len(x.keys);
}

[GoRecv] internal static void Swap(this ref keysByCount x, nint i, nint j) {
    (x.keys[i], x.keys[j]) = (x.keys[j], x.keys[i]);
}

[GoRecv] internal static bool Less(this ref keysByCount x, nint i, nint j) {
    @string ki = x.keys[i];
    @string kj = x.keys[j];
    nint ci = x.count[ki];
    nint cj = x.count[kj];
    if (ci != cj) {
        return ci > cj;
    }
    return ki < kj;
}

// printStackRecord prints the function + source line information
// for a single stack trace.
internal static void printStackRecord(io.Writer w, slice<uintptr> stk, bool allFrames) {
    var show = allFrames;
    var frames = runtime.CallersFrames(stk);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        @string name = frame.Function;
        if (name == ""u8){
            show = true;
            fmt.Fprintf(w, "#\t%#x\n"u8, frame.PC);
        } else 
        if (name != "runtime.goexit"u8 && (show || !strings.HasPrefix(name, "runtime."u8))) {
            // Hide runtime.goexit and any runtime functions at the beginning.
            // This is useful mainly for allocation traces.
            show = true;
            fmt.Fprintf(w, "#\t%#x\t%s+%#x\t%s:%d\n"u8, frame.PC, name, frame.PC - frame.Entry, frame.File, frame.Line);
        }
        if (!more) {
            break;
        }
    }
    if (!show) {
        // We didn't print anything; do it again,
        // and this time include runtime functions.
        printStackRecord(w, stk, true);
        return;
    }
    fmt.Fprintf(w, "\n"u8);
}

// Interface to system profiles.

// WriteHeapProfile is shorthand for [Lookup]("heap").WriteTo(w, 0).
// It is preserved for backwards compatibility.
public static error WriteHeapProfile(io.Writer w) {
    return writeHeap(w, 0);
}

// countHeap returns the number of records in the heap profile.
internal static nint countHeap() {
    var (n, _) = runtime.MemProfile(default!, true);
    return n;
}

// writeHeap writes the current runtime heap profile to w.
internal static error writeHeap(io.Writer w, nint debug) {
    return writeHeapInternal(w, debug, ""u8);
}

// writeAlloc writes the current runtime heap profile to w
// with the total allocation space as the default sample type.
internal static error writeAlloc(io.Writer w, nint debug) {
    return writeHeapInternal(w, debug, "alloc_space"u8);
}

internal static error writeHeapInternal(io.Writer w, nint debug, @string defaultSampleType) {
    ж<runtime.MemStats> memStats = default!;
    if (debug != 0) {
        // Read mem stats first, so that our other allocations
        // do not appear in the statistics.
        memStats = @new<runtime.MemStats>();
        runtime.ReadMemStats(memStats);
    }
    // Find out how many records there are (the call
    // pprof_memProfileInternal(nil, true) below),
    // allocate that many records, and get the data.
    // There's a race—more records might be added between
    // the two calls—so allocate a few extra records for safety
    // and also try again if we're very unlucky.
    // The loop should only execute one iteration in the common case.
    slice<profilerecord.MemProfileRecord> p = default!;
    var (n, ok) = pprof_memProfileInternal(default!, true);
    while (ᐧ) {
        // Allocate room for a slightly bigger profile,
        // in case a few more entries have been added
        // since the call to MemProfile.
        p = new slice<profilerecord.MemProfileRecord>(n + 50);
        (n, ok) = pprof_memProfileInternal(p, true);
        if (ok) {
            p = p[0..(int)(n)];
            break;
        }
    }
    // Profile grew; try again.
    if (debug == 0) {
        return writeHeapProto(w, p, ((int64)runtime.MemProfileRate), defaultSampleType);
    }
    slices.SortFunc(p, (profilerecord.MemProfileRecord a, profilerecord.MemProfileRecord b) => cmp.Compare(a.InUseBytes(), bΔ1.InUseBytes()));
    var b = bufio.NewWriter(w);
    var tw = tabwriter.NewWriter(~b, 1, 8, 1, (rune)'\t', 0);
    w = ~tw;
    runtime.MemProfileRecord total = default!;
    foreach (var (i, _) in p) {
        var r = Ꮡ(p, i);
        total.AllocBytes += r.val.AllocBytes;
        total.AllocObjects += r.val.AllocObjects;
        total.FreeBytes += r.val.FreeBytes;
        total.FreeObjects += r.val.FreeObjects;
    }
    // Technically the rate is MemProfileRate not 2*MemProfileRate,
    // but early versions of the C++ heap profiler reported 2*MemProfileRate,
    // so that's what pprof has come to expect.
    nint rate = 2 * runtime.MemProfileRate;
    // pprof reads a profile with alloc == inuse as being a "2-column" profile
    // (objects and bytes, not distinguishing alloc from inuse),
    // but then such a profile can't be merged using pprof *.prof with
    // other 4-column profiles where alloc != inuse.
    // The easiest way to avoid this bug is to adjust allocBytes so it's never == inuseBytes.
    // pprof doesn't use these header values anymore except for checking equality.
    var inUseBytes = total.InUseBytes();
    var allocBytes = total.AllocBytes;
    if (inUseBytes == allocBytes) {
        allocBytes++;
    }
    fmt.Fprintf(w, "heap profile: %d: %d [%d: %d] @ heap/%d\n"u8,
        total.InUseObjects(), inUseBytes,
        total.AllocObjects, allocBytes,
        rate);
    foreach (var (i, _) in p) {
        var r = Ꮡ(p, i);
        fmt.Fprintf(w, "%d: %d [%d: %d] @"u8,
            r.InUseObjects(), r.InUseBytes(),
            (~r).AllocObjects, (~r).AllocBytes);
        foreach (var (_, pc) in (~r).Stack) {
            fmt.Fprintf(w, " %#x"u8, pc);
        }
        fmt.Fprintf(w, "\n"u8);
        printStackRecord(w, (~r).Stack, false);
    }
    // Print memstats information too.
    // Pprof will ignore, but useful for people
    var s = memStats;
    fmt.Fprintf(w, "\n# runtime.MemStats\n"u8);
    fmt.Fprintf(w, "# Alloc = %d\n"u8, (~s).Alloc);
    fmt.Fprintf(w, "# TotalAlloc = %d\n"u8, (~s).TotalAlloc);
    fmt.Fprintf(w, "# Sys = %d\n"u8, (~s).Sys);
    fmt.Fprintf(w, "# Lookups = %d\n"u8, (~s).Lookups);
    fmt.Fprintf(w, "# Mallocs = %d\n"u8, (~s).Mallocs);
    fmt.Fprintf(w, "# Frees = %d\n"u8, (~s).Frees);
    fmt.Fprintf(w, "# HeapAlloc = %d\n"u8, (~s).HeapAlloc);
    fmt.Fprintf(w, "# HeapSys = %d\n"u8, (~s).HeapSys);
    fmt.Fprintf(w, "# HeapIdle = %d\n"u8, (~s).HeapIdle);
    fmt.Fprintf(w, "# HeapInuse = %d\n"u8, (~s).HeapInuse);
    fmt.Fprintf(w, "# HeapReleased = %d\n"u8, (~s).HeapReleased);
    fmt.Fprintf(w, "# HeapObjects = %d\n"u8, (~s).HeapObjects);
    fmt.Fprintf(w, "# Stack = %d / %d\n"u8, (~s).StackInuse, (~s).StackSys);
    fmt.Fprintf(w, "# MSpan = %d / %d\n"u8, (~s).MSpanInuse, (~s).MSpanSys);
    fmt.Fprintf(w, "# MCache = %d / %d\n"u8, (~s).MCacheInuse, (~s).MCacheSys);
    fmt.Fprintf(w, "# BuckHashSys = %d\n"u8, (~s).BuckHashSys);
    fmt.Fprintf(w, "# GCSys = %d\n"u8, (~s).GCSys);
    fmt.Fprintf(w, "# OtherSys = %d\n"u8, (~s).OtherSys);
    fmt.Fprintf(w, "# NextGC = %d\n"u8, (~s).NextGC);
    fmt.Fprintf(w, "# LastGC = %d\n"u8, (~s).LastGC);
    fmt.Fprintf(w, "# PauseNs = %d\n"u8, (~s).PauseNs);
    fmt.Fprintf(w, "# PauseEnd = %d\n"u8, (~s).PauseEnd);
    fmt.Fprintf(w, "# NumGC = %d\n"u8, (~s).NumGC);
    fmt.Fprintf(w, "# NumForcedGC = %d\n"u8, (~s).NumForcedGC);
    fmt.Fprintf(w, "# GCCPUFraction = %v\n"u8, (~s).GCCPUFraction);
    fmt.Fprintf(w, "# DebugGC = %v\n"u8, (~s).DebugGC);
    // Also flush out MaxRSS on supported platforms.
    addMaxRSS(w);
    tw.Flush();
    return b.Flush();
}

// countThreadCreate returns the size of the current ThreadCreateProfile.
internal static nint countThreadCreate() {
    var (n, _) = runtime.ThreadCreateProfile(default!);
    return n;
}

// writeThreadCreate writes the current runtime ThreadCreateProfile to w.
internal static error writeThreadCreate(io.Writer w, nint debug) {
    // Until https://golang.org/issues/6104 is addressed, wrap
    // ThreadCreateProfile because there's no point in tracking labels when we
    // don't get any stack-traces.
    return writeRuntimeProfile(w, debug, "threadcreate"u8, (slice<profilerecord.StackRecord> p, slice<@unsafe.Pointer> _) => pprof_threadCreateInternal(p));
}

// countGoroutine returns the number of goroutines.
internal static nint countGoroutine() {
    return runtime.NumGoroutine();
}

// writeGoroutine writes the current runtime GoroutineProfile to w.
internal static error writeGoroutine(io.Writer w, nint debug) {
    if (debug >= 2) {
        return writeGoroutineStacks(w);
    }
    return writeRuntimeProfile(w, debug, "goroutine"u8, pprof_goroutineProfileWithLabels);
}

internal static error writeGoroutineStacks(io.Writer w) {
    // We don't know how big the buffer needs to be to collect
    // all the goroutines. Start with 1 MB and try a few times, doubling each time.
    // Give up and use a truncated trace if 64 MB is not enough.
    var buf = new slice<byte>(1 << (int)(20));
    for (nint i = 0; ᐧ ; i++) {
        nint n = runtime.Stack(buf, true);
        if (n < len(buf)) {
            buf = buf[..(int)(n)];
            break;
        }
        if (len(buf) >= 64 << (int)(20)) {
            // Filled 64 MB - stop there.
            break;
        }
        buf = new slice<byte>(2 * len(buf));
    }
    var (_, err) = w.Write(buf);
    return err;
}

internal static error writeRuntimeProfile(io.Writer w, nint debug, @string name, profilerecord.StackRecord, <>unsafe.Pointer) (int, bool) fetch) {
    // Find out how many records there are (fetch(nil)),
    // allocate that many records, and get the data.
    // There's a race—more records might be added between
    // the two calls—so allocate a few extra records for safety
    // and also try again if we're very unlucky.
    // The loop should only execute one iteration in the common case.
    slice<profilerecord.StackRecord> p = default!;
    slice<@unsafe.Pointer> labels = default!;
    var (n, ok) = fetch(default!, default!);
    while (ᐧ) {
        // Allocate room for a slightly bigger profile,
        // in case a few more entries have been added
        // since the call to ThreadProfile.
        p = new slice<profilerecord.StackRecord>(n + 10);
        labels = new slice<@unsafe.Pointer>(n + 10);
        (n, ok) = fetch(p, labels);
        if (ok) {
            p = p[0..(int)(n)];
            break;
        }
    }
    // Profile grew; try again.
    return printCountProfile(w, debug, name, new runtimeProfile(p, labels));
}

[GoType] partial struct runtimeProfile {
    internal profilerecord.StackRecord stk;
    internal slice<@unsafe.Pointer> labels;
}

[GoRecv] internal static nint Len(this ref runtimeProfile p) {
    return len(p.stk);
}

[GoRecv] internal static slice<uintptr> Stack(this ref runtimeProfile p, nint i) {
    return p.stk[i].Stack;
}

[GoRecv] internal static ж<labelMap> Label(this ref runtimeProfile p, nint i) {
    return (ж<labelMap>)(uintptr)(p.labels[i]);
}


[GoType("dyn")] partial struct cpuᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal bool profiling;
    internal channel<bool> done;
}
internal static cpuᴛ1 cpu;

// StartCPUProfile enables CPU profiling for the current process.
// While profiling, the profile will be buffered and written to w.
// StartCPUProfile returns an error if profiling is already enabled.
//
// On Unix-like systems, StartCPUProfile does not work by default for
// Go code built with -buildmode=c-archive or -buildmode=c-shared.
// StartCPUProfile relies on the SIGPROF signal, but that signal will
// be delivered to the main program's SIGPROF signal handler (if any)
// not to the one used by Go. To make it work, call [os/signal.Notify]
// for [syscall.SIGPROF], but note that doing so may break any profiling
// being done by the main program.
public static error StartCPUProfile(io.Writer w) => func((defer, _) => {
    // The runtime routines allow a variable profiling rate,
    // but in practice operating systems cannot trigger signals
    // at more than about 500 Hz, and our processing of the
    // signal is not cheap (mostly getting the stack trace).
    // 100 Hz is a reasonable choice: it is frequent enough to
    // produce useful data, rare enough not to bog down the
    // system, and a nice round number to make it easy to
    // convert sample counts to seconds. Instead of requiring
    // each client to specify the frequency, we hard code it.
    static readonly UntypedInt hz = 100;
    cpu.Lock();
    var cpuʗ1 = cpu;
    defer(cpuʗ1.Unlock);
    if (cpu.done == default!) {
        cpu.done = new channel<bool>(1);
    }
    // Double-check.
    if (cpu.profiling) {
        return fmt.Errorf("cpu profiling already in use"u8);
    }
    cpu.profiling = true;
    runtime.SetCPUProfileRate(hz);
    goǃ(profileWriter, w);
    return default!;
});

// readProfile, provided by the runtime, returns the next chunk of
// binary CPU profiling stack trace data, blocking until data is available.
// If profiling is turned off and all the profile data accumulated while it was
// on has been returned, readProfile returns eof=true.
// The caller must save the returned data and tags before calling readProfile again.
internal static partial (slice<uint64> data, slice<@unsafe.Pointer> tags, bool eof) readProfile();

internal static void profileWriter(io.Writer w) {
    var b = newProfileBuilder(w);
    error err = default!;
    while (ᐧ) {
        time.Sleep(100 * time.Millisecond);
        var (data, tags, eof) = readProfile();
        {
            var e = b.addCPUData(data, tags); if (e != default! && err == default!) {
                err = e;
            }
        }
        if (eof) {
            break;
        }
    }
    if (err != default!) {
        // The runtime should never produce an invalid or truncated profile.
        // It drops records that can't fit into its log buffers.
        throw panic("runtime/pprof: converting profile: "u8 + err.Error());
    }
    b.build();
    cpu.done.ᐸꟷ(true);
}

// StopCPUProfile stops the current CPU profile, if any.
// StopCPUProfile only returns after all the writes for the
// profile have completed.
public static void StopCPUProfile() => func((defer, _) => {
    cpu.Lock();
    var cpuʗ1 = cpu;
    defer(cpuʗ1.Unlock);
    if (!cpu.profiling) {
        return;
    }
    cpu.profiling = false;
    runtime.SetCPUProfileRate(0);
    ᐸꟷ(cpu.done);
});

// countBlock returns the number of records in the blocking profile.
internal static nint countBlock() {
    var (n, _) = runtime.BlockProfile(default!);
    return n;
}

// countMutex returns the number of records in the mutex profile.
internal static nint countMutex() {
    var (n, _) = runtime.MutexProfile(default!);
    return n;
}

// writeBlock writes the current blocking profile to w.
internal static error writeBlock(io.Writer w, nint debug) {
    return writeProfileInternal(w, debug, "contention"u8, pprof_blockProfileInternal);
}

// writeMutex writes the current mutex profile to w.
internal static error writeMutex(io.Writer w, nint debug) {
    return writeProfileInternal(w, debug, "mutex"u8, pprof_mutexProfileInternal);
}

// writeProfileInternal writes the current blocking or mutex profile depending on the passed parameters.
internal static error writeProfileInternal(io.Writer w, nint debug, @string name, profilerecord.BlockProfileRecord) (int, bool) runtimeProfile) {
    slice<profilerecord.BlockProfileRecord> p = default!;
    var (n, ok) = runtimeProfile(default!);
    while (ᐧ) {
        p = new slice<profilerecord.BlockProfileRecord>(n + 50);
        (n, ok) = runtimeProfile(p);
        if (ok) {
            p = p[..(int)(n)];
            break;
        }
    }
    slices.SortFunc(p, (profilerecord.BlockProfileRecord a, profilerecord.BlockProfileRecord b) => cmp.Compare(bΔ1.Cycles, a.Cycles));
    if (debug <= 0) {
        return printCountCycleProfile(w, "contentions"u8, "delay"u8, p);
    }
    var b = bufio.NewWriter(w);
    var tw = tabwriter.NewWriter(w, 1, 8, 1, (rune)'\t', 0);
    w = ~tw;
    fmt.Fprintf(w, "--- %v:\n"u8, name);
    fmt.Fprintf(w, "cycles/second=%v\n"u8, pprof_cyclesPerSecond());
    if (name == "mutex"u8) {
        fmt.Fprintf(w, "sampling period=%d\n"u8, runtime.SetMutexProfileFraction(-1));
    }
    var expandedStack = pprof_makeProfStack();
    foreach (var (i, _) in p) {
        var r = Ꮡ(p, i);
        fmt.Fprintf(w, "%v %v @"u8, (~r).Cycles, (~r).Count);
        nint nΔ1 = expandInlinedFrames(expandedStack, (~r).Stack);
        var stack = expandedStack[..(int)(nΔ1)];
        foreach (var (_, pc) in stack) {
            fmt.Fprintf(w, " %#x"u8, pc);
        }
        fmt.Fprint(w, "\n");
        if (debug > 0) {
            printStackRecord(w, stack, true);
        }
    }
    if (tw != nil) {
        tw.Flush();
    }
    return b.Flush();
}

//go:linkname pprof_goroutineProfileWithLabels runtime.pprof_goroutineProfileWithLabels
internal static partial (nint n, bool ok) pprof_goroutineProfileWithLabels(slice<profilerecord.StackRecord> p, slice<@unsafe.Pointer> labels);

//go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond
internal static partial int64 pprof_cyclesPerSecond();

//go:linkname pprof_memProfileInternal runtime.pprof_memProfileInternal
internal static partial (nint n, bool ok) pprof_memProfileInternal(slice<profilerecord.MemProfileRecord> p, bool inuseZero);

//go:linkname pprof_blockProfileInternal runtime.pprof_blockProfileInternal
internal static partial (nint n, bool ok) pprof_blockProfileInternal(slice<profilerecord.BlockProfileRecord> p);

//go:linkname pprof_mutexProfileInternal runtime.pprof_mutexProfileInternal
internal static partial (nint n, bool ok) pprof_mutexProfileInternal(slice<profilerecord.BlockProfileRecord> p);

//go:linkname pprof_threadCreateInternal runtime.pprof_threadCreateInternal
internal static partial (nint n, bool ok) pprof_threadCreateInternal(slice<profilerecord.StackRecord> p);

//go:linkname pprof_fpunwindExpand runtime.pprof_fpunwindExpand
internal static partial nint pprof_fpunwindExpand(slice<uintptr> dst, slice<uintptr> src);

//go:linkname pprof_makeProfStack runtime.pprof_makeProfStack
internal static partial slice<uintptr> pprof_makeProfStack();

} // end pprof_package
