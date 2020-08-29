// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pprof writes runtime profiling data in the format expected
// by the pprof visualization tool.
//
// Profiling a Go program
//
// The first step to profiling a Go program is to enable profiling.
// Support for profiling benchmarks built with the standard testing
// package is built into go test. For example, the following command
// runs benchmarks in the current directory and writes the CPU and
// memory profiles to cpu.prof and mem.prof:
//
//     go test -cpuprofile cpu.prof -memprofile mem.prof -bench .
//
// To add equivalent profiling support to a standalone program, add
// code like the following to your main function:
//
//    var cpuprofile = flag.String("cpuprofile", "", "write cpu profile to `file`")
//    var memprofile = flag.String("memprofile", "", "write memory profile to `file`")
//
//    func main() {
//        flag.Parse()
//        if *cpuprofile != "" {
//            f, err := os.Create(*cpuprofile)
//            if err != nil {
//                log.Fatal("could not create CPU profile: ", err)
//            }
//            if err := pprof.StartCPUProfile(f); err != nil {
//                log.Fatal("could not start CPU profile: ", err)
//            }
//            defer pprof.StopCPUProfile()
//        }
//
//        // ... rest of the program ...
//
//        if *memprofile != "" {
//            f, err := os.Create(*memprofile)
//            if err != nil {
//                log.Fatal("could not create memory profile: ", err)
//            }
//            runtime.GC() // get up-to-date statistics
//            if err := pprof.WriteHeapProfile(f); err != nil {
//                log.Fatal("could not write memory profile: ", err)
//            }
//            f.Close()
//        }
//    }
//
// There is also a standard HTTP interface to profiling data. Adding
// the following line will install handlers under the /debug/pprof/
// URL to download live profiles:
//
//    import _ "net/http/pprof"
//
// See the net/http/pprof package for more details.
//
// Profiles can then be visualized with the pprof tool:
//
//    go tool pprof cpu.prof
//
// There are many commands available from the pprof command line.
// Commonly used commands include "top", which prints a summary of the
// top program hot-spots, and "web", which opens an interactive graph
// of hot-spots and their call graphs. Use "help" for information on
// all pprof commands.
//
// For more information about pprof, see
// https://github.com/google/pprof/blob/master/doc/pprof.md.
// package pprof -- go2cs converted at 2020 August 29 08:22:50 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\pprof.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using tabwriter = go.text.tabwriter_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
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
        //    goroutine    - stack traces of all current goroutines
        //    heap         - a sampling of all heap allocations
        //    threadcreate - stack traces that led to the creation of new OS threads
        //    block        - stack traces that led to blocking on synchronization primitives
        //    mutex        - stack traces of holders of contended mutexes
        //
        // These predefined profiles maintain themselves and panic on an explicit
        // Add or Remove method call.
        //
        // The heap profile reports statistics as of the most recently completed
        // garbage collection; it elides more recent allocation to avoid skewing
        // the profile away from live data and toward garbage.
        // If there has been no garbage collection at all, the heap profile reports
        // all known allocations. This exception helps mainly in programs running
        // without garbage collection enabled, usually for debugging purposes.
        //
        // The CPU profile is not available as a Profile. It has a special API,
        // the StartCPUProfile and StopCPUProfile functions, because it streams
        // output to a writer during profiling.
        //
        public partial struct Profile
        {
            public @string name;
            public sync.Mutex mu;
            public Func<long> count;
            public Func<io.Writer, long, error> write;
        }

        // profiles records all registered profiles.
        private static var profiles = default;

        private static Profile goroutineProfile = ref new Profile(name:"goroutine",count:countGoroutine,write:writeGoroutine,);

        private static Profile threadcreateProfile = ref new Profile(name:"threadcreate",count:countThreadCreate,write:writeThreadCreate,);

        private static Profile heapProfile = ref new Profile(name:"heap",count:countHeap,write:writeHeap,);

        private static Profile blockProfile = ref new Profile(name:"block",count:countBlock,write:writeBlock,);

        private static Profile mutexProfile = ref new Profile(name:"mutex",count:countMutex,write:writeMutex,);

        private static void lockProfiles()
        {
            profiles.mu.Lock();
            if (profiles.m == null)
            { 
                // Initial built-in profiles.
                profiles.m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref Profile>{"goroutine":goroutineProfile,"threadcreate":threadcreateProfile,"heap":heapProfile,"block":blockProfile,"mutex":mutexProfile,};
            }
        }

        private static void unlockProfiles()
        {
            profiles.mu.Unlock();
        }

        // NewProfile creates a new profile with the given name.
        // If a profile with that name already exists, NewProfile panics.
        // The convention is to use a 'import/path.' prefix to create
        // separate name spaces for each package.
        // For compatibility with various tools that read pprof data,
        // profile names should not contain spaces.
        public static ref Profile NewProfile(@string name) => func((defer, panic, _) =>
        {
            lockProfiles();
            defer(unlockProfiles());
            if (name == "")
            {
                panic("pprof: NewProfile with empty name");
            }
            if (profiles.m[name] != null)
            {
                panic("pprof: NewProfile name already in use: " + name);
            }
            Profile p = ref new Profile(name:name,m:map[interface{}][]uintptr{},);
            profiles.m[name] = p;
            return p;
        });

        // Lookup returns the profile with the given name, or nil if no such profile exists.
        public static ref Profile Lookup(@string name) => func((defer, _, __) =>
        {
            lockProfiles();
            defer(unlockProfiles());
            return profiles.m[name];
        });

        // Profiles returns a slice of all the known profiles, sorted by name.
        public static slice<ref Profile> Profiles() => func((defer, _, __) =>
        {
            lockProfiles();
            defer(unlockProfiles());

            var all = make_slice<ref Profile>(0L, len(profiles.m));
            foreach (var (_, p) in profiles.m)
            {
                all = append(all, p);
            }
            sort.Slice(all, (i, j) => all[i].name < all[j].name);
            return all;
        });

        // Name returns this profile's name, which can be passed to Lookup to reobtain the profile.
        private static @string Name(this ref Profile p)
        {
            return p.name;
        }

        // Count returns the number of execution stacks currently in the profile.
        private static long Count(this ref Profile _p) => func(_p, (ref Profile p, Defer defer, Panic _, Recover __) =>
        {
            p.mu.Lock();
            defer(p.mu.Unlock());
            if (p.count != null)
            {
                return p.count();
            }
            return len(p.m);
        });

        // Add adds the current execution stack to the profile, associated with value.
        // Add stores value in an internal map, so value must be suitable for use as
        // a map key and will not be garbage collected until the corresponding
        // call to Remove. Add panics if the profile already contains a stack for value.
        //
        // The skip parameter has the same meaning as runtime.Caller's skip
        // and controls where the stack trace begins. Passing skip=0 begins the
        // trace in the function calling Add. For example, given this
        // execution stack:
        //
        //    Add
        //    called from rpc.NewClient
        //    called from mypkg.Run
        //    called from main.main
        //
        // Passing skip=0 begins the stack trace at the call to Add inside rpc.NewClient.
        // Passing skip=1 begins the stack trace at the call to NewClient inside mypkg.Run.
        //
        private static void Add(this ref Profile _p, object value, long skip) => func(_p, (ref Profile p, Defer defer, Panic panic, Recover _) =>
        {
            if (p.name == "")
            {
                panic("pprof: use of uninitialized Profile");
            }
            if (p.write != null)
            {
                panic("pprof: Add called on built-in Profile " + p.name);
            }
            var stk = make_slice<System.UIntPtr>(32L);
            var n = runtime.Callers(skip + 1L, stk[..]);
            stk = stk[..n];
            if (len(stk) == 0L)
            { 
                // The value for skip is too large, and there's no stack trace to record.
                stk = new slice<System.UIntPtr>(new System.UIntPtr[] { funcPC(lostProfileEvent) });
            }
            p.mu.Lock();
            defer(p.mu.Unlock());
            if (p.m[value] != null)
            {
                panic("pprof: Profile.Add of duplicate value");
            }
            p.m[value] = stk;
        });

        // Remove removes the execution stack associated with value from the profile.
        // It is a no-op if the value is not in the profile.
        private static void Remove(this ref Profile _p, object value) => func(_p, (ref Profile p, Defer defer, Panic _, Recover __) =>
        {
            p.mu.Lock();
            defer(p.mu.Unlock());
            delete(p.m, value);
        });

        // WriteTo writes a pprof-formatted snapshot of the profile to w.
        // If a write to w returns an error, WriteTo returns that error.
        // Otherwise, WriteTo returns nil.
        //
        // The debug parameter enables additional output.
        // Passing debug=0 prints only the hexadecimal addresses that pprof needs.
        // Passing debug=1 adds comments translating addresses to function names
        // and line numbers, so that a programmer can read the profile without tools.
        //
        // The predefined profiles may assign meaning to other debug values;
        // for example, when printing the "goroutine" profile, debug=2 means to
        // print the goroutine stacks in the same form that a Go program uses
        // when dying due to an unrecovered panic.
        private static error WriteTo(this ref Profile _p, io.Writer w, long debug) => func(_p, (ref Profile p, Defer _, Panic panic, Recover __) =>
        {
            if (p.name == "")
            {
                panic("pprof: use of zero Profile");
            }
            if (p.write != null)
            {
                return error.As(p.write(w, debug));
            } 

            // Obtain consistent snapshot under lock; then process without lock.
            p.mu.Lock();
            var all = make_slice<slice<System.UIntPtr>>(0L, len(p.m));
            foreach (var (_, stk) in p.m)
            {
                all = append(all, stk);
            }
            p.mu.Unlock(); 

            // Map order is non-deterministic; make output deterministic.
            sort.Slice(all, (i, j) =>
            {
                var t = all[i];
                var u = all[j];
                for (long k = 0L; k < len(t) && k < len(u); k++)
                {
                    if (t[k] != u[k])
                    {
                        return error.As(t[k] < u[k]);
                    }
                }

                return error.As(len(t) < len(u));
            });

            return error.As(printCountProfile(w, debug, p.name, stackProfile(all)));
        });

        private partial struct stackProfile // : slice<slice<System.UIntPtr>>
        {
        }

        private static long Len(this stackProfile x)
        {
            return len(x);
        }
        private static slice<System.UIntPtr> Stack(this stackProfile x, long i)
        {
            return x[i];
        }

        // A countProfile is a set of stack traces to be printed as counts
        // grouped by stack trace. There are multiple implementations:
        // all that matters is that we can find out how many traces there are
        // and obtain each trace in turn.
        private partial interface countProfile
        {
            slice<System.UIntPtr> Len();
            slice<System.UIntPtr> Stack(long i);
        }

        // printCountCycleProfile outputs block profile records (for block or mutex profiles)
        // as the pprof-proto format output. Translations from cycle count to time duration
        // are done because The proto expects count and time (nanoseconds) instead of count
        // and the number of cycles for block, contention profiles.
        // Possible 'scaler' functions are scaleBlockProfile and scaleMutexProfile.
        private static error printCountCycleProfile(io.Writer w, @string countName, @string cycleName, Func<long, double, (long, double)> scaler, slice<runtime.BlockProfileRecord> records)
        { 
            // Output profile in protobuf form.
            var b = newProfileBuilder(w);
            b.pbValueType(tagProfile_PeriodType, countName, "count");
            b.pb.int64Opt(tagProfile_Period, 1L);
            b.pbValueType(tagProfile_SampleType, countName, "count");
            b.pbValueType(tagProfile_SampleType, cycleName, "nanoseconds");

            var cpuGHz = float64(runtime_cyclesPerSecond()) / 1e9F;

            long values = new slice<long>(new long[] { 0, 0 });
            slice<ulong> locs = default;
            foreach (var (_, r) in records)
            {
                var (count, nanosec) = scaler(r.Count, float64(r.Cycles) / cpuGHz);
                values[0L] = count;
                values[1L] = int64(nanosec);
                locs = locs[..0L];
                foreach (var (_, addr) in r.Stack())
                { 
                    // For count profiles, all stack addresses are
                    // return PCs, which is what locForPC expects.
                    var l = b.locForPC(addr);
                    if (l == 0L)
                    { // runtime.goexit
                        continue;
                    }
                    locs = append(locs, l);
                }
                b.pbSample(values, locs, null);
            }
            b.build();
            return error.As(null);
        }

        // printCountProfile prints a countProfile at the specified debug level.
        // The profile will be in compressed proto format unless debug is nonzero.
        private static error printCountProfile(io.Writer w, long debug, @string name, countProfile p)
        { 
            // Build count of each stack.
            bytes.Buffer buf = default;
            Func<slice<System.UIntPtr>, @string> key = stk =>
            {
                buf.Reset();
                fmt.Fprintf(ref buf, "@");
                foreach (var (_, pc) in stk)
                {
                    fmt.Fprintf(ref buf, " %#x", pc);
                }
                return error.As(buf.String());
            }
;
            map count = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};
            map index = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};
            slice<@string> keys = default;
            var n = p.Len();
            for (long i = 0L; i < n; i++)
            {
                var k = key(p.Stack(i));
                if (count[k] == 0L)
                {
                    index[k] = i;
                    keys = append(keys, k);
                }
                count[k]++;
            }


            sort.Sort(ref new keysByCount(keys,count));

            if (debug > 0L)
            { 
                // Print debug profile in legacy format
                var tw = tabwriter.NewWriter(w, 1L, 8L, 1L, '\t', 0L);
                fmt.Fprintf(tw, "%s profile: total %d\n", name, p.Len());
                {
                    var k__prev1 = k;

                    foreach (var (_, __k) in keys)
                    {
                        k = __k;
                        fmt.Fprintf(tw, "%d %s\n", count[k], k);
                        printStackRecord(tw, p.Stack(index[k]), false);
                    }

                    k = k__prev1;
                }

                return error.As(tw.Flush());
            } 

            // Output profile in protobuf form.
            var b = newProfileBuilder(w);
            b.pbValueType(tagProfile_PeriodType, name, "count");
            b.pb.int64Opt(tagProfile_Period, 1L);
            b.pbValueType(tagProfile_SampleType, name, "count");

            long values = new slice<long>(new long[] { 0 });
            slice<ulong> locs = default;
            {
                var k__prev1 = k;

                foreach (var (_, __k) in keys)
                {
                    k = __k;
                    values[0L] = int64(count[k]);
                    locs = locs[..0L];
                    foreach (var (_, addr) in p.Stack(index[k]))
                    { 
                        // For count profiles, all stack addresses are
                        // return PCs, which is what locForPC expects.
                        var l = b.locForPC(addr);
                        if (l == 0L)
                        { // runtime.goexit
                            continue;
                        }
                        locs = append(locs, l);
                    }
                    b.pbSample(values, locs, null);
                }

                k = k__prev1;
            }

            b.build();
            return error.As(null);
        }

        // keysByCount sorts keys with higher counts first, breaking ties by key string order.
        private partial struct keysByCount
        {
            public slice<@string> keys;
            public map<@string, long> count;
        }

        private static long Len(this ref keysByCount x)
        {
            return len(x.keys);
        }
        private static void Swap(this ref keysByCount x, long i, long j)
        {
            x.keys[i] = x.keys[j];
            x.keys[j] = x.keys[i];

        }
        private static bool Less(this ref keysByCount x, long i, long j)
        {
            var ki = x.keys[i];
            var kj = x.keys[j];
            var ci = x.count[ki];
            var cj = x.count[kj];
            if (ci != cj)
            {
                return ci > cj;
            }
            return ki < kj;
        }

        // printStackRecord prints the function + source line information
        // for a single stack trace.
        private static void printStackRecord(io.Writer w, slice<System.UIntPtr> stk, bool allFrames)
        {
            var show = allFrames;
            var frames = runtime.CallersFrames(stk);
            while (true)
            {
                var (frame, more) = frames.Next();
                var name = frame.Function;
                if (name == "")
                {
                    show = true;
                    fmt.Fprintf(w, "#\t%#x\n", frame.PC);
                }
                else if (name != "runtime.goexit" && (show || !strings.HasPrefix(name, "runtime.")))
                { 
                    // Hide runtime.goexit and any runtime functions at the beginning.
                    // This is useful mainly for allocation traces.
                    show = true;
                    fmt.Fprintf(w, "#\t%#x\t%s+%#x\t%s:%d\n", frame.PC, name, frame.PC - frame.Entry, frame.File, frame.Line);
                }
                if (!more)
                {
                    break;
                }
            }

            if (!show)
            { 
                // We didn't print anything; do it again,
                // and this time include runtime functions.
                printStackRecord(w, stk, true);
                return;
            }
            fmt.Fprintf(w, "\n");
        }

        // Interface to system profiles.

        // WriteHeapProfile is shorthand for Lookup("heap").WriteTo(w, 0).
        // It is preserved for backwards compatibility.
        public static error WriteHeapProfile(io.Writer w)
        {
            return error.As(writeHeap(w, 0L));
        }

        // countHeap returns the number of records in the heap profile.
        private static long countHeap()
        {
            var (n, _) = runtime.MemProfile(null, true);
            return n;
        }

        // writeHeap writes the current runtime heap profile to w.
        private static error writeHeap(io.Writer w, long debug)
        {
            ref runtime.MemStats memStats = default;
            if (debug != 0L)
            { 
                // Read mem stats first, so that our other allocations
                // do not appear in the statistics.
                memStats = @new<runtime.MemStats>();
                runtime.ReadMemStats(memStats);
            } 

            // Find out how many records there are (MemProfile(nil, true)),
            // allocate that many records, and get the data.
            // There's a race—more records might be added between
            // the two calls—so allocate a few extra records for safety
            // and also try again if we're very unlucky.
            // The loop should only execute one iteration in the common case.
            slice<runtime.MemProfileRecord> p = default;
            var (n, ok) = runtime.MemProfile(null, true);
            while (true)
            { 
                // Allocate room for a slightly bigger profile,
                // in case a few more entries have been added
                // since the call to MemProfile.
                p = make_slice<runtime.MemProfileRecord>(n + 50L);
                n, ok = runtime.MemProfile(p, true);
                if (ok)
                {
                    p = p[0L..n];
                    break;
                } 
                // Profile grew; try again.
            }


            if (debug == 0L)
            {
                return error.As(writeHeapProto(w, p, int64(runtime.MemProfileRate)));
            }
            sort.Slice(p, (i, j) => error.As(p[i].InUseBytes() > p[j].InUseBytes()));

            var b = bufio.NewWriter(w);
            var tw = tabwriter.NewWriter(b, 1L, 8L, 1L, '\t', 0L);
            w = tw;

            runtime.MemProfileRecord total = default;
            {
                var i__prev1 = i;

                foreach (var (__i) in p)
                {
                    i = __i;
                    var r = ref p[i];
                    total.AllocBytes += r.AllocBytes;
                    total.AllocObjects += r.AllocObjects;
                    total.FreeBytes += r.FreeBytes;
                    total.FreeObjects += r.FreeObjects;
                } 

                // Technically the rate is MemProfileRate not 2*MemProfileRate,
                // but early versions of the C++ heap profiler reported 2*MemProfileRate,
                // so that's what pprof has come to expect.

                i = i__prev1;
            }

            fmt.Fprintf(w, "heap profile: %d: %d [%d: %d] @ heap/%d\n", total.InUseObjects(), total.InUseBytes(), total.AllocObjects, total.AllocBytes, 2L * runtime.MemProfileRate);

            {
                var i__prev1 = i;

                foreach (var (__i) in p)
                {
                    i = __i;
                    r = ref p[i];
                    fmt.Fprintf(w, "%d: %d [%d: %d] @", r.InUseObjects(), r.InUseBytes(), r.AllocObjects, r.AllocBytes);
                    foreach (var (_, pc) in r.Stack())
                    {
                        fmt.Fprintf(w, " %#x", pc);
                    }
                    fmt.Fprintf(w, "\n");
                    printStackRecord(w, r.Stack(), false);
                } 

                // Print memstats information too.
                // Pprof will ignore, but useful for people

                i = i__prev1;
            }

            var s = memStats;
            fmt.Fprintf(w, "\n# runtime.MemStats\n");
            fmt.Fprintf(w, "# Alloc = %d\n", s.Alloc);
            fmt.Fprintf(w, "# TotalAlloc = %d\n", s.TotalAlloc);
            fmt.Fprintf(w, "# Sys = %d\n", s.Sys);
            fmt.Fprintf(w, "# Lookups = %d\n", s.Lookups);
            fmt.Fprintf(w, "# Mallocs = %d\n", s.Mallocs);
            fmt.Fprintf(w, "# Frees = %d\n", s.Frees);

            fmt.Fprintf(w, "# HeapAlloc = %d\n", s.HeapAlloc);
            fmt.Fprintf(w, "# HeapSys = %d\n", s.HeapSys);
            fmt.Fprintf(w, "# HeapIdle = %d\n", s.HeapIdle);
            fmt.Fprintf(w, "# HeapInuse = %d\n", s.HeapInuse);
            fmt.Fprintf(w, "# HeapReleased = %d\n", s.HeapReleased);
            fmt.Fprintf(w, "# HeapObjects = %d\n", s.HeapObjects);

            fmt.Fprintf(w, "# Stack = %d / %d\n", s.StackInuse, s.StackSys);
            fmt.Fprintf(w, "# MSpan = %d / %d\n", s.MSpanInuse, s.MSpanSys);
            fmt.Fprintf(w, "# MCache = %d / %d\n", s.MCacheInuse, s.MCacheSys);
            fmt.Fprintf(w, "# BuckHashSys = %d\n", s.BuckHashSys);
            fmt.Fprintf(w, "# GCSys = %d\n", s.GCSys);
            fmt.Fprintf(w, "# OtherSys = %d\n", s.OtherSys);

            fmt.Fprintf(w, "# NextGC = %d\n", s.NextGC);
            fmt.Fprintf(w, "# LastGC = %d\n", s.LastGC);
            fmt.Fprintf(w, "# PauseNs = %d\n", s.PauseNs);
            fmt.Fprintf(w, "# PauseEnd = %d\n", s.PauseEnd);
            fmt.Fprintf(w, "# NumGC = %d\n", s.NumGC);
            fmt.Fprintf(w, "# NumForcedGC = %d\n", s.NumForcedGC);
            fmt.Fprintf(w, "# GCCPUFraction = %v\n", s.GCCPUFraction);
            fmt.Fprintf(w, "# DebugGC = %v\n", s.DebugGC);

            tw.Flush();
            return error.As(b.Flush());
        }

        // countThreadCreate returns the size of the current ThreadCreateProfile.
        private static long countThreadCreate()
        {
            var (n, _) = runtime.ThreadCreateProfile(null);
            return n;
        }

        // writeThreadCreate writes the current runtime ThreadCreateProfile to w.
        private static error writeThreadCreate(io.Writer w, long debug)
        {
            return error.As(writeRuntimeProfile(w, debug, "threadcreate", runtime.ThreadCreateProfile));
        }

        // countGoroutine returns the number of goroutines.
        private static long countGoroutine()
        {
            return runtime.NumGoroutine();
        }

        // writeGoroutine writes the current runtime GoroutineProfile to w.
        private static error writeGoroutine(io.Writer w, long debug)
        {
            if (debug >= 2L)
            {
                return error.As(writeGoroutineStacks(w));
            }
            return error.As(writeRuntimeProfile(w, debug, "goroutine", runtime.GoroutineProfile));
        }

        private static error writeGoroutineStacks(io.Writer w)
        { 
            // We don't know how big the buffer needs to be to collect
            // all the goroutines. Start with 1 MB and try a few times, doubling each time.
            // Give up and use a truncated trace if 64 MB is not enough.
            var buf = make_slice<byte>(1L << (int)(20L));
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                var n = runtime.Stack(buf, true);
                if (n < len(buf))
                {
                    buf = buf[..n];
                    break;
                }
                if (len(buf) >= 64L << (int)(20L))
                { 
                    // Filled 64 MB - stop there.
                    break;
                }
                buf = make_slice<byte>(2L * len(buf));
            }

            var (_, err) = w.Write(buf);
            return error.As(err);
        }

        private static error writeRuntimeProfile(io.Writer w, long debug, @string name, Func<slice<runtime.StackRecord>, (long, bool)> fetch)
        { 
            // Find out how many records there are (fetch(nil)),
            // allocate that many records, and get the data.
            // There's a race—more records might be added between
            // the two calls—so allocate a few extra records for safety
            // and also try again if we're very unlucky.
            // The loop should only execute one iteration in the common case.
            slice<runtime.StackRecord> p = default;
            var (n, ok) = fetch(null);
            while (true)
            { 
                // Allocate room for a slightly bigger profile,
                // in case a few more entries have been added
                // since the call to ThreadProfile.
                p = make_slice<runtime.StackRecord>(n + 10L);
                n, ok = fetch(p);
                if (ok)
                {
                    p = p[0L..n];
                    break;
                } 
                // Profile grew; try again.
            }


            return error.As(printCountProfile(w, debug, name, runtimeProfile(p)));
        }

        private partial struct runtimeProfile // : slice<runtime.StackRecord>
        {
        }

        private static long Len(this runtimeProfile p)
        {
            return len(p);
        }
        private static slice<System.UIntPtr> Stack(this runtimeProfile p, long i)
        {
            return p[i].Stack();
        }

        private static var cpu = default;

        // StartCPUProfile enables CPU profiling for the current process.
        // While profiling, the profile will be buffered and written to w.
        // StartCPUProfile returns an error if profiling is already enabled.
        //
        // On Unix-like systems, StartCPUProfile does not work by default for
        // Go code built with -buildmode=c-archive or -buildmode=c-shared.
        // StartCPUProfile relies on the SIGPROF signal, but that signal will
        // be delivered to the main program's SIGPROF signal handler (if any)
        // not to the one used by Go. To make it work, call os/signal.Notify
        // for syscall.SIGPROF, but note that doing so may break any profiling
        // being done by the main program.
        public static error StartCPUProfile(io.Writer w) => func((defer, _, __) =>
        { 
            // The runtime routines allow a variable profiling rate,
            // but in practice operating systems cannot trigger signals
            // at more than about 500 Hz, and our processing of the
            // signal is not cheap (mostly getting the stack trace).
            // 100 Hz is a reasonable choice: it is frequent enough to
            // produce useful data, rare enough not to bog down the
            // system, and a nice round number to make it easy to
            // convert sample counts to seconds. Instead of requiring
            // each client to specify the frequency, we hard code it.
            const long hz = 100L;



            cpu.Lock();
            defer(cpu.Unlock());
            if (cpu.done == null)
            {
                cpu.done = make_channel<bool>();
            } 
            // Double-check.
            if (cpu.profiling)
            {
                return error.As(fmt.Errorf("cpu profiling already in use"));
            }
            cpu.profiling = true;
            runtime.SetCPUProfileRate(hz);
            go_(() => profileWriter(w));
            return error.As(null);
        });

        // readProfile, provided by the runtime, returns the next chunk of
        // binary CPU profiling stack trace data, blocking until data is available.
        // If profiling is turned off and all the profile data accumulated while it was
        // on has been returned, readProfile returns eof=true.
        // The caller must save the returned data and tags before calling readProfile again.
        private static (slice<ulong>, slice<unsafe.Pointer>, bool) readProfile()
;

        private static void profileWriter(io.Writer w) => func((_, panic, __) =>
        {
            var b = newProfileBuilder(w);
            error err = default;
            while (true)
            {>>MARKER:FUNCTION_readProfile_BLOCK_PREFIX<<
                time.Sleep(100L * time.Millisecond);
                var (data, tags, eof) = readProfile();
                {
                    var e = b.addCPUData(data, tags);

                    if (e != null && err == null)
                    {
                        err = error.As(e);
                    }

                }
                if (eof)
                {
                    break;
                }
            }

            if (err != null)
            { 
                // The runtime should never produce an invalid or truncated profile.
                // It drops records that can't fit into its log buffers.
                panic("runtime/pprof: converting profile: " + err.Error());
            }
            b.build();
            cpu.done.Send(true);
        });

        // StopCPUProfile stops the current CPU profile, if any.
        // StopCPUProfile only returns after all the writes for the
        // profile have completed.
        public static void StopCPUProfile() => func((defer, _, __) =>
        {
            cpu.Lock();
            defer(cpu.Unlock());

            if (!cpu.profiling)
            {
                return;
            }
            cpu.profiling = false;
            runtime.SetCPUProfileRate(0L).Send(cpu.done);
        });

        // countBlock returns the number of records in the blocking profile.
        private static long countBlock()
        {
            var (n, _) = runtime.BlockProfile(null);
            return n;
        }

        // countMutex returns the number of records in the mutex profile.
        private static long countMutex()
        {
            var (n, _) = runtime.MutexProfile(null);
            return n;
        }

        // writeBlock writes the current blocking profile to w.
        private static error writeBlock(io.Writer w, long debug)
        {
            slice<runtime.BlockProfileRecord> p = default;
            var (n, ok) = runtime.BlockProfile(null);
            while (true)
            {
                p = make_slice<runtime.BlockProfileRecord>(n + 50L);
                n, ok = runtime.BlockProfile(p);
                if (ok)
                {
                    p = p[..n];
                    break;
                }
            }


            sort.Slice(p, (i, j) => error.As(p[i].Cycles > p[j].Cycles));

            if (debug <= 0L)
            {
                return error.As(printCountCycleProfile(w, "contentions", "delay", scaleBlockProfile, p));
            }
            var b = bufio.NewWriter(w);
            var tw = tabwriter.NewWriter(w, 1L, 8L, 1L, '\t', 0L);
            w = tw;

            fmt.Fprintf(w, "--- contention:\n");
            fmt.Fprintf(w, "cycles/second=%v\n", runtime_cyclesPerSecond());
            foreach (var (i) in p)
            {
                var r = ref p[i];
                fmt.Fprintf(w, "%v %v @", r.Cycles, r.Count);
                foreach (var (_, pc) in r.Stack())
                {
                    fmt.Fprintf(w, " %#x", pc);
                }
                fmt.Fprint(w, "\n");
                if (debug > 0L)
                {
                    printStackRecord(w, r.Stack(), true);
                }
            }
            if (tw != null)
            {
                tw.Flush();
            }
            return error.As(b.Flush());
        }

        private static (long, double) scaleBlockProfile(long cnt, double ns)
        { 
            // Do nothing.
            // The current way of block profile sampling makes it
            // hard to compute the unsampled number. The legacy block
            // profile parse doesn't attempt to scale or unsample.
            return (cnt, ns);
        }

        // writeMutex writes the current mutex profile to w.
        private static error writeMutex(io.Writer w, long debug)
        { 
            // TODO(pjw): too much common code with writeBlock. FIX!
            slice<runtime.BlockProfileRecord> p = default;
            var (n, ok) = runtime.MutexProfile(null);
            while (true)
            {
                p = make_slice<runtime.BlockProfileRecord>(n + 50L);
                n, ok = runtime.MutexProfile(p);
                if (ok)
                {
                    p = p[..n];
                    break;
                }
            }


            sort.Slice(p, (i, j) => error.As(p[i].Cycles > p[j].Cycles));

            if (debug <= 0L)
            {
                return error.As(printCountCycleProfile(w, "contentions", "delay", scaleMutexProfile, p));
            }
            var b = bufio.NewWriter(w);
            var tw = tabwriter.NewWriter(w, 1L, 8L, 1L, '\t', 0L);
            w = tw;

            fmt.Fprintf(w, "--- mutex:\n");
            fmt.Fprintf(w, "cycles/second=%v\n", runtime_cyclesPerSecond());
            fmt.Fprintf(w, "sampling period=%d\n", runtime.SetMutexProfileFraction(-1L));
            foreach (var (i) in p)
            {
                var r = ref p[i];
                fmt.Fprintf(w, "%v %v @", r.Cycles, r.Count);
                foreach (var (_, pc) in r.Stack())
                {
                    fmt.Fprintf(w, " %#x", pc);
                }
                fmt.Fprint(w, "\n");
                if (debug > 0L)
                {
                    printStackRecord(w, r.Stack(), true);
                }
            }
            if (tw != null)
            {
                tw.Flush();
            }
            return error.As(b.Flush());
        }

        private static (long, double) scaleMutexProfile(long cnt, double ns)
        {
            var period = runtime.SetMutexProfileFraction(-1L);
            return (cnt * int64(period), ns * float64(period));
        }

        private static long runtime_cyclesPerSecond()
;
    }
}}
