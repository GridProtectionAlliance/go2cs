// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using sysinfo = @internal.sysinfo_package;
using io = io_package;
using math = math_package;
using os = os_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using time = time_package;
using unicode = unicode_package;
using @internal;
using sync;

partial class testing_package {

internal static void initBenchmarkFlags() {
    matchBenchmarks = flag.String("test.bench"u8, ""u8, "run only benchmarks matching `regexp`"u8);
    benchmarkMemory = flag.Bool("test.benchmem"u8, false, "print memory allocations for benchmarks"u8);
    flag.Var(benchTime, "test.benchtime"u8, "run each benchmark for duration `d` or N times if `d` is of the form Nx"u8);
}

internal static ж<@string> matchBenchmarks;
internal static ж<bool> benchmarkMemory;
internal static durationOrCountFlag benchTime = new durationOrCountFlag(d: 1 * time.ΔSecond);   // changed during test of testing package

[GoType] partial struct durationOrCountFlag {
    internal time_package.Duration d;
    internal nint n;
    internal bool allowZero;
}

[GoRecv] internal static @string String(this ref durationOrCountFlag f) {
    if (f.n > 0) {
        return fmt.Sprintf("%dx"u8, f.n);
    }
    return f.d.String();
}

[GoRecv] internal static error Set(this ref durationOrCountFlag f, @string s) {
    if (strings.HasSuffix(s, "x"u8)) {
        var (n, errΔ1) = strconv.ParseInt(s[..(int)(len(s) - 1)], 10, 0);
        if (errΔ1 != default! || n < 0 || (!f.allowZero && n == 0)) {
            return fmt.Errorf("invalid count"u8);
        }
        f = new durationOrCountFlag(n: ((nint)n));
        return default!;
    }
    var (d, err) = time.ParseDuration(s);
    if (err != default! || d < 0 || (!f.allowZero && d == 0)) {
        return fmt.Errorf("invalid duration"u8);
    }
    f = new durationOrCountFlag(d: d);
    return default!;
}

// Global lock to ensure only one benchmark runs at a time.
internal static sync.Mutex benchmarkLock;

// Used for every benchmark for measuring memory.
internal static runtime.MemStats memStats;

// InternalBenchmark is an internal type but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
[GoType] partial struct InternalBenchmark {
    public @string Name;
    public Action<ж<B>> F;
}

// B is a type passed to [Benchmark] functions to manage benchmark
// timing and to specify the number of iterations to run.
//
// A benchmark ends when its Benchmark function returns or calls any of the methods
// FailNow, Fatal, Fatalf, SkipNow, Skip, or Skipf. Those methods must be called
// only from the goroutine running the Benchmark function.
// The other reporting methods, such as the variations of Log and Error,
// may be called simultaneously from multiple goroutines.
//
// Like in tests, benchmark logs are accumulated during execution
// and dumped to standard output when done. Unlike in tests, benchmark logs
// are always printed, so as not to hide output whose existence may be
// affecting benchmark results.
[GoType] partial struct B {
    internal partial ref common common { get; }
    internal @string importPath; // import path of the package containing the benchmark
    internal ж<benchContext> context;
    public nint N;
    internal nint previousN;          // number of iterations in the previous run
    internal time_package.Duration previousDuration; // total duration of the previous run
    internal Action<ж<B>> benchFunc;
    internal durationOrCountFlag benchTime;
    internal int64 bytes;
    internal bool missingBytes; // one of the subbenchmarks does not have bytes set.
    internal bool timerOn;
    internal bool showAllocResult;
    internal BenchmarkResult result;
    internal nint parallelism; // RunParallel creates parallelism*GOMAXPROCS goroutines
    // The initial states of memStats.Mallocs and memStats.TotalAlloc.
    internal uint64 startAllocs;
    internal uint64 startBytes;
    // The net total of this test after being run.
    internal uint64 netAllocs;
    internal uint64 netBytes;
    // Extra metrics collected by ReportMetric.
    internal map<@string, float64> extra;
}

// StartTimer starts timing a test. This function is called automatically
// before a benchmark starts, but it can also be used to resume timing after
// a call to [B.StopTimer].
[GoRecv] public static void StartTimer(this ref B b) {
    if (!b.timerOn) {
        runtime.ReadMemStats(Ꮡ(memStats));
        b.startAllocs = memStats.Mallocs;
        b.startBytes = memStats.TotalAlloc;
        b.start = highPrecisionTimeNow();
        b.timerOn = true;
    }
}

// StopTimer stops timing a test. This can be used to pause the timer
// while performing complex initialization that you don't
// want to measure.
[GoRecv] public static void StopTimer(this ref B b) {
    if (b.timerOn) {
        b.duration += highPrecisionTimeSince(b.start);
        runtime.ReadMemStats(Ꮡ(memStats));
        b.netAllocs += memStats.Mallocs - b.startAllocs;
        b.netBytes += memStats.TotalAlloc - b.startBytes;
        b.timerOn = false;
    }
}

// ResetTimer zeroes the elapsed benchmark time and memory allocation counters
// and deletes user-reported metrics.
// It does not affect whether the timer is running.
[GoRecv] public static void ResetTimer(this ref B b) {
    if (b.extra == default!){
        // Allocate the extra map before reading memory stats.
        // Pre-size it to make more allocation unlikely.
        b.extra = new map<@string, float64>(16);
    } else {
        clear(b.extra);
    }
    if (b.timerOn) {
        runtime.ReadMemStats(Ꮡ(memStats));
        b.startAllocs = memStats.Mallocs;
        b.startBytes = memStats.TotalAlloc;
        b.start = highPrecisionTimeNow();
    }
    b.duration = 0;
    b.netAllocs = 0;
    b.netBytes = 0;
}

// SetBytes records the number of bytes processed in a single operation.
// If this is called, the benchmark will report ns/op and MB/s.
[GoRecv] public static void SetBytes(this ref B b, int64 n) {
    b.bytes = n;
}

// ReportAllocs enables malloc statistics for this benchmark.
// It is equivalent to setting -test.benchmem, but it only affects the
// benchmark function that calls ReportAllocs.
[GoRecv] public static void ReportAllocs(this ref B b) {
    b.showAllocResult = true;
}

// runN runs a single benchmark for the specified number of iterations.
[GoRecv] internal static void runN(this ref B b, nint n) => func((defer, _) => {
    benchmarkLock.Lock();
    var benchmarkLockʗ1 = benchmarkLock;
    defer(benchmarkLockʗ1.Unlock);
    defer(() => {
        b.runCleanup(normalPanic);
        b.checkRaces();
    });
    // Try to get a comparable environment for each run
    // by clearing garbage from previous runs.
    runtime.GC();
    b.resetRaces();
    b.N = n;
    b.parallelism = 1;
    b.ResetTimer();
    b.StartTimer();
    b.benchFunc(b);
    b.StopTimer();
    b.previousN = n;
    b.previousDuration = b.duration;
});

// run1 runs the first iteration of benchFunc. It reports whether more
// iterations of this benchmarks should be run.
[GoRecv] internal static bool run1(this ref B b) => func((defer, _) => {
    {
        var ctx = b.context; if (ctx != nil) {
            // Extend maxLen, if needed.
            {
                nint n = len(b.name) + (~ctx).extLen + 1; if (n > (~ctx).maxLen) {
                    ctx.val.maxLen = n + 8;
                }
            }
        }
    }
    // Add additional slack to avoid too many jumps in size.
    goǃ(() => {
        // Signal that we're done whether we return normally
        // or by FailNow's runtime.Goexit.
        defer(() => {
            b.signal.ᐸꟷ(true);
        });
        b.runN(1);
    });
    ᐸꟷ(b.signal);
    if (b.failed) {
        fmt.Fprintf(b.w, "%s--- FAIL: %s\n%s"u8, b.chatty.prefix(), b.name, b.output);
        return false;
    }
    // Only print the output if we know we are not going to proceed.
    // Otherwise it is printed in processBench.
    b.mu.RLock();
    var finished = b.finished;
    b.mu.RUnlock();
    if (b.hasSub.Load() || finished) {
        @string tag = "BENCH"u8;
        if (b.skipped) {
            tag = "SKIP"u8;
        }
        if (b.chatty != nil && (len(b.output) > 0 || finished)) {
            b.trimOutput();
            fmt.Fprintf(b.w, "%s--- %s: %s\n%s"u8, b.chatty.prefix(), tag, b.name, b.output);
        }
        return false;
    }
    return true;
});

internal static sync.Once labelsOnce;

// run executes the benchmark in a separate goroutine, including all of its
// subbenchmarks. b must not have subbenchmarks.
[GoRecv] internal static void run(this ref B b) {
    labelsOnce.Do(() => {
        fmt.Fprintf(b.w, "goos: %s\n"u8, runtime.GOOS);
        fmt.Fprintf(b.w, "goarch: %s\n"u8, runtime.GOARCH);
        if (b.importPath != ""u8) {
            fmt.Fprintf(b.w, "pkg: %s\n"u8, b.importPath);
        }
        {
            @string cpu = sysinfo.CPUName(); if (cpu != ""u8) {
                fmt.Fprintf(b.w, "cpu: %s\n"u8, cpu);
            }
        }
    });
    if (b.context != nil){
        // Running go test --test.bench
        b.context.processBench(b);
    } else {
        // Must call doBench.
        // Running func Benchmark.
        b.doBench();
    }
}

[GoRecv] internal static BenchmarkResult doBench(this ref B b) {
    goǃ(b.launch);
    ᐸꟷ(b.signal);
    return b.result;
}

// launch launches the benchmark function. It gradually increases the number
// of benchmark iterations until the benchmark runs for the requested benchtime.
// launch is run by the doBench function as a separate goroutine.
// run1 must have been called on b.
[GoRecv] internal static void launch(this ref B b) => func((defer, _) => {
    // Signal that we're done whether we return normally
    // or by FailNow's runtime.Goexit.
    defer(() => {
        b.signal.ᐸꟷ(true);
    });
    // Run the benchmark for at least the specified amount of time.
    if (b.benchTime.n > 0){
        // We already ran a single iteration in run1.
        // If -benchtime=1x was requested, use that result.
        // See https://golang.org/issue/32051.
        if (b.benchTime.n > 1) {
            b.runN(b.benchTime.n);
        }
    } else {
        var d = b.benchTime.d;
        for (var n = ((int64)1); !b.failed && b.duration < d && n < 1e9F; ) {
            var last = n;
            // Predict required iterations.
            var goalns = d.Nanoseconds();
            var prevIters = ((int64)b.N);
            var prevns = b.duration.Nanoseconds();
            if (prevns <= 0) {
                // Round up, to avoid div by zero.
                prevns = 1;
            }
            // Order of operations matters.
            // For very fast benchmarks, prevIters ~= prevns.
            // If you divide first, you get 0 or 1,
            // which can hide an order of magnitude in execution time.
            // So multiply first, then divide.
            n = goalns * prevIters / prevns;
            // Run more iterations than we think we'll need (1.2x).
            n += n / 5;
            // Don't grow too fast in case we had timing errors previously.
            n = min(n, 100 * last);
            // Be sure to run at least one more than last time.
            n = max(n, last + 1);
            // Don't run more than 1e9 times. (This also keeps n in int range on 32 bit platforms.)
            n = min(n, 1e9F);
            b.runN(((nint)n));
        }
    }
    b.result = new BenchmarkResult(b.N, b.duration, b.bytes, b.netAllocs, b.netBytes, b.extra);
});

// Elapsed returns the measured elapsed time of the benchmark.
// The duration reported by Elapsed matches the one measured by
// [B.StartTimer], [B.StopTimer], and [B.ResetTimer].
[GoRecv] public static time.Duration Elapsed(this ref B b) {
    var d = b.duration;
    if (b.timerOn) {
        d += highPrecisionTimeSince(b.start);
    }
    return d;
}

// ReportMetric adds "n unit" to the reported benchmark results.
// If the metric is per-iteration, the caller should divide by b.N,
// and by convention units should end in "/op".
// ReportMetric overrides any previously reported value for the same unit.
// ReportMetric panics if unit is the empty string or if unit contains
// any whitespace.
// If unit is a unit normally reported by the benchmark framework itself
// (such as "allocs/op"), ReportMetric will override that metric.
// Setting "ns/op" to 0 will suppress that built-in metric.
[GoRecv] public static void ReportMetric(this ref B b, float64 n, @string unit) {
    if (unit == ""u8) {
        throw panic("metric unit must not be empty");
    }
    if (strings.IndexFunc(unit, unicode.IsSpace) >= 0) {
        throw panic("metric unit must not contain whitespace");
    }
    b.extra[unit] = n;
}

// BenchmarkResult contains the results of a benchmark run.
[GoType] partial struct BenchmarkResult {
    public nint N;          // The number of iterations.
    public time_package.Duration T; // The total time taken.
    public int64 Bytes;         // Bytes processed in one iteration.
    public uint64 MemAllocs;        // The total number of memory allocations.
    public uint64 MemBytes;        // The total number of bytes allocated.
    // Extra records additional metrics reported by ReportMetric.
    public map<@string, float64> Extra;
}

// NsPerOp returns the "ns/op" metric.
public static int64 NsPerOp(this BenchmarkResult r) {
    {
        var (v, ok) = r.Extra["ns/op"u8]; if (ok) {
            return ((int64)v);
        }
    }
    if (r.N <= 0) {
        return 0;
    }
    return r.T.Nanoseconds() / ((int64)r.N);
}

// mbPerSec returns the "MB/s" metric.
internal static float64 mbPerSec(this BenchmarkResult r) {
    {
        var (v, ok) = r.Extra["MB/s"u8]; if (ok) {
            return v;
        }
    }
    if (r.Bytes <= 0 || r.T <= 0 || r.N <= 0) {
        return 0;
    }
    return (((float64)r.Bytes) * ((float64)r.N) / 1e6F) / r.T.Seconds();
}

// AllocsPerOp returns the "allocs/op" metric,
// which is calculated as r.MemAllocs / r.N.
public static int64 AllocsPerOp(this BenchmarkResult r) {
    {
        var (v, ok) = r.Extra["allocs/op"u8]; if (ok) {
            return ((int64)v);
        }
    }
    if (r.N <= 0) {
        return 0;
    }
    return ((int64)r.MemAllocs) / ((int64)r.N);
}

// AllocedBytesPerOp returns the "B/op" metric,
// which is calculated as r.MemBytes / r.N.
public static int64 AllocedBytesPerOp(this BenchmarkResult r) {
    {
        var (v, ok) = r.Extra["B/op"u8]; if (ok) {
            return ((int64)v);
        }
    }
    if (r.N <= 0) {
        return 0;
    }
    return ((int64)r.MemBytes) / ((int64)r.N);
}

// String returns a summary of the benchmark results.
// It follows the benchmark result line format from
// https://golang.org/design/14313-benchmark-format, not including the
// benchmark name.
// Extra metrics override built-in metrics of the same name.
// String does not include allocs/op or B/op, since those are reported
// by [BenchmarkResult.MemString].
public static @string String(this BenchmarkResult r) {
    var buf = @new<strings.Builder>();
    fmt.Fprintf(~buf, "%8d"u8, r.N);
    // Get ns/op as a float.
    var (ns, ok) = r.Extra["ns/op"u8];
    if (!ok) {
        ns = ((float64)r.T.Nanoseconds()) / ((float64)r.N);
    }
    if (ns != 0) {
        buf.WriteByte((rune)'\t');
        prettyPrint(~buf, ns, "ns/op"u8);
    }
    {
        var mbs = r.mbPerSec(); if (mbs != 0) {
            fmt.Fprintf(~buf, "\t%7.2f MB/s"u8, mbs);
        }
    }
    // Print extra metrics that aren't represented in the standard
    // metrics.
    slice<@string> extraKeys = default!;
    foreach (var (k, _) in r.Extra) {
        var exprᴛ1 = k;
        if (exprᴛ1 == "ns/op"u8 || exprᴛ1 == "MB/s"u8 || exprᴛ1 == "B/op"u8 || exprᴛ1 == "allocs/op"u8) {
            continue;
        }

        // Built-in metrics reported elsewhere.
        extraKeys = append(extraKeys, k);
    }
    slices.Sort(extraKeys);
    foreach (var (_, k) in extraKeys) {
        buf.WriteByte((rune)'\t');
        prettyPrint(~buf, r.Extra[k], k);
    }
    return buf.String();
}

internal static void prettyPrint(io.Writer w, float64 x, @string unit) {
    // Print all numbers with 10 places before the decimal point
    // and small numbers with four sig figs. Field widths are
    // chosen to fit the whole part in 10 places while aligning
    // the decimal point of all fractional formats.
    @string format = default!;
    {
        var y = math.Abs(x);
        switch (ᐧ) {
        case {} when y == 0 || y >= 999.95F: {
            format = "%10.0f %s"u8;
            break;
        }
        case {} when y is >= 99.995F: {
            format = "%12.1f %s"u8;
            break;
        }
        case {} when y is >= 9.9995F: {
            format = "%13.2f %s"u8;
            break;
        }
        case {} when y is >= 0.99995F: {
            format = "%14.3f %s"u8;
            break;
        }
        case {} when y is >= 0.099995F: {
            format = "%15.4f %s"u8;
            break;
        }
        case {} when y is >= 0.0099995F: {
            format = "%16.5f %s"u8;
            break;
        }
        case {} when y is >= 0.00099995F: {
            format = "%17.6f %s"u8;
            break;
        }
        default: {
            format = "%18.7f %s"u8;
            break;
        }}
    }

    fmt.Fprintf(w, format, x, unit);
}

// MemString returns r.AllocedBytesPerOp and r.AllocsPerOp in the same format as 'go test'.
public static @string MemString(this BenchmarkResult r) {
    return fmt.Sprintf("%8d B/op\t%8d allocs/op"u8,
        r.AllocedBytesPerOp(), r.AllocsPerOp());
}

// benchmarkName returns full name of benchmark including procs suffix.
internal static @string benchmarkName(@string name, nint n) {
    if (n != 1) {
        return fmt.Sprintf("%s-%d"u8, name, n);
    }
    return name;
}

[GoType] partial struct benchContext {
    internal ж<matcher> match;
    internal nint maxLen; // The largest recorded benchmark name.
    internal nint extLen; // Maximum extension length.
}

// RunBenchmarks is an internal function but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
public static void RunBenchmarks(Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks) {
    runBenchmarks(""u8, matchString, benchmarks);
}

internal static bool runBenchmarks(@string importPath, Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks) {
    // If no flag was specified, don't run benchmarks.
    if (len(matchBenchmarks.val) == 0) {
        return true;
    }
    // Collect matching benchmarks and determine longest name.
    nint maxprocs = 1;
    foreach (var (_, procs) in cpuList) {
        if (procs > maxprocs) {
            maxprocs = procs;
        }
    }
    var ctx = Ꮡ(new benchContext(
        match: newMatcher(matchString, matchBenchmarks.val, "-test.bench"u8, skip.val),
        extLen: len(benchmarkName(""u8, maxprocs))
    ));
    slice<InternalBenchmark> bs = default!;
    foreach (var (_, Benchmark) in benchmarks) {
        {
            var (_, matched, _) = (~ctx).match.fullName(nil, Benchmark.Name); if (matched) {
                bs = append(bs, Benchmark);
                @string benchName = benchmarkName(Benchmark.Name, maxprocs);
                {
                    nint l = len(benchName) + (~ctx).extLen + 1; if (l > (~ctx).maxLen) {
                        ctx.val.maxLen = l;
                    }
                }
            }
        }
    }
    var main = Ꮡ(new B(
        common: new common(
            name: "Main"u8,
            w: os.Stdout,
            bench: true
        ),
        importPath: importPath,
        benchFunc: 
        var bsʗ1 = bs;
        (ж<B> b) => {
            ref var Benchmark = ref heap(new InternalBenchmark(), out var ᏑBenchmark);

            foreach (var (_, Benchmark) in bsʗ1) {
                b.Run(Benchmark.Name, Benchmark.F);
            }
        },
        benchTime: benchTime,
        context: ctx
    ));
    if (Verbose()) {
        main.chatty = newChattyPrinter(main.w);
    }
    main.runN(1);
    return !main.failed;
}

// processBench runs bench b for the configured CPU counts and prints the results.
[GoRecv] internal static void processBench(this ref benchContext ctx, ж<B> Ꮡb) {
    ref var b = ref Ꮡb.val;

    foreach (var (i, procs) in cpuList) {
        for (nuint j = ((nuint)0); j < count.val; j++) {
            runtime.GOMAXPROCS(procs);
            @string benchName = benchmarkName(b.name, procs);
            // If it's chatty, we've already printed this information.
            if (b.chatty == nil) {
                fmt.Fprintf(b.w, "%-*s\t"u8, ctx.maxLen, benchName);
            }
            // Recompute the running time for all but the first iteration.
            if (i > 0 || j > 0) {
                Ꮡb = Ꮡ(new B(
                    common: new common(
                        signal: new channel<bool>(1),
                        name: b.name,
                        w: b.w,
                        chatty: b.chatty,
                        bench: true
                    ),
                    benchFunc: b.benchFunc,
                    benchTime: b.benchTime
                )); b = ref Ꮡb.val;
                b.run1();
            }
            var r = b.doBench();
            if (b.failed) {
                // The output could be very long here, but probably isn't.
                // We print it all, regardless, because we don't want to trim the reason
                // the benchmark failed.
                fmt.Fprintf(b.w, "%s--- FAIL: %s\n%s"u8, b.chatty.prefix(), benchName, b.output);
                continue;
            }
            @string results = r.String();
            if (b.chatty != nil) {
                fmt.Fprintf(b.w, "%-*s\t"u8, ctx.maxLen, benchName);
            }
            if (benchmarkMemory.val || b.showAllocResult) {
                results += "\t"u8 + r.MemString();
            }
            fmt.Fprintln(b.w, results);
            // Unlike with tests, we ignore the -chatty flag and always print output for
            // benchmarks since the output generation time will skew the results.
            if (len(b.output) > 0) {
                b.trimOutput();
                fmt.Fprintf(b.w, "%s--- BENCH: %s\n%s"u8, b.chatty.prefix(), benchName, b.output);
            }
            {
                nint p = runtime.GOMAXPROCS(-1); if (p != procs) {
                    fmt.Fprintf(~os.Stderr, "testing: %s left GOMAXPROCS set to %d\n"u8, benchName, p);
                }
            }
            if (b.chatty != nil && b.chatty.json) {
                b.chatty.Updatef(""u8, "=== NAME  %s\n"u8, "");
            }
        }
    }
}

// If hideStdoutForTesting is true, Run does not print the benchName.
// This avoids a spurious print during 'go test' on package testing itself,
// which invokes b.Run in its own tests (see sub_test.go).
internal static bool hideStdoutForTesting = false;

// Run benchmarks f as a subbenchmark with the given name. It reports
// whether there were any failures.
//
// A subbenchmark is like any other benchmark. A benchmark that calls Run at
// least once will not be measured itself and will be called once with N=1.
[GoRecv] public static bool Run(this ref B b, @string name, Action<ж<B>> f) => func((defer, _) => {
    // Since b has subbenchmarks, we will no longer run it as a benchmark itself.
    // Release the lock and acquire it on exit to ensure locks stay paired.
    b.hasSub.Store(true);
    benchmarkLock.Unlock();
    var benchmarkLockʗ1 = benchmarkLock;
    defer(benchmarkLockʗ1.Lock);
    @string benchName = b.name;
    var ok = true;
    var partial = false;
    if (b.context != nil) {
        (benchName, ok, partial) = b.context.match.fullName(Ꮡ(b.common), name);
    }
    if (!ok) {
        return true;
    }
    array<uintptr> pc = new(50); /* maxStackLen */
    nint n = runtime.Callers(2, pc[..]);
    var sub = Ꮡ(new B(
        common: new common(
            signal: new channel<bool>(1),
            name: benchName,
            parent: Ꮡ(b.common),
            level: b.level + 1,
            creator: pc[..(int)(n)],
            w: b.w,
            chatty: b.chatty,
            bench: true
        ),
        importPath: b.importPath,
        benchFunc: f,
        benchTime: b.benchTime,
        context: b.context
    ));
    if (partial) {
        // Partial name match, like -bench=X/Y matching BenchmarkX.
        // Only process sub-benchmarks, if any.
        sub.hasSub.Store(true);
    }
    if (b.chatty != nil) {
        labelsOnce.Do(
        () => {
            fmt.Printf("goos: %s\n"u8, runtime.GOOS);
            fmt.Printf("goarch: %s\n"u8, runtime.GOARCH);
            if (b.importPath != ""u8) {
                fmt.Printf("pkg: %s\n"u8, b.importPath);
            }
            {
                @string cpu = sysinfo.CPUName(); if (cpu != ""u8) {
                    fmt.Printf("cpu: %s\n"u8, cpu);
                }
            }
        });
        if (!hideStdoutForTesting) {
            if (b.chatty.json) {
                b.chatty.Updatef(benchName, "=== RUN   %s\n"u8, benchName);
            }
            fmt.Println(benchName);
        }
    }
    if (sub.run1()) {
        sub.run();
    }
    b.add((~sub).result);
    return !sub.failed;
});

// add simulates running benchmarks in sequence in a single iteration. It is
// used to give some meaningful results in case func Benchmark is used in
// combination with Run.
[GoRecv] internal static void add(this ref B b, BenchmarkResult other) {
    var r = Ꮡ(b.result);
    // The aggregated BenchmarkResults resemble running all subbenchmarks as
    // in sequence in a single benchmark.
    r.val.N = 1;
    r.val.T += ((time.Duration)other.NsPerOp());
    if (other.Bytes == 0) {
        // Summing Bytes is meaningless in aggregate if not all subbenchmarks
        // set it.
        b.missingBytes = true;
        r.val.Bytes = 0;
    }
    if (!b.missingBytes) {
        r.val.Bytes += other.Bytes;
    }
    r.val.MemAllocs += ((uint64)other.AllocsPerOp());
    r.val.MemBytes += ((uint64)other.AllocedBytesPerOp());
}

// trimOutput shortens the output from a benchmark, which can be very long.
[GoRecv] internal static void trimOutput(this ref B b) {
    // The output is likely to appear multiple times because the benchmark
    // is run multiple times, but at least it will be seen. This is not a big deal
    // because benchmarks rarely print, but just in case, we trim it if it's too long.
    static readonly UntypedInt maxNewlines = 10;
    for (nint nlCount = 0;nint j = 0; j < len(b.output); j++) {
        if (b.output[j] == (rune)'\n') {
            nlCount++;
            if (nlCount >= maxNewlines) {
                b.output = append(b.output[..(int)(j)], "\n\t... [output truncated]\n"u8.ꓸꓸꓸ);
                break;
            }
        }
    }
}

// A PB is used by RunParallel for running parallel benchmarks.
[GoType] partial struct PB {
    internal ж<sync.atomic_package.Uint64> globalN; // shared between all worker goroutines iteration counter
    internal uint64 grain;         // acquire that many iterations from globalN at once
    internal uint64 cache;         // local cache of acquired iterations
    internal uint64 bN;         // total number of iterations to execute (b.N)
}

// Next reports whether there are more iterations to execute.
[GoRecv] public static bool Next(this ref PB pb) {
    if (pb.cache == 0) {
        var n = pb.globalN.Add(pb.grain);
        if (n <= pb.bN){
            pb.cache = pb.grain;
        } else 
        if (n < pb.bN + pb.grain){
            pb.cache = pb.bN + pb.grain - n;
        } else {
            return false;
        }
    }
    pb.cache--;
    return true;
}

// RunParallel runs a benchmark in parallel.
// It creates multiple goroutines and distributes b.N iterations among them.
// The number of goroutines defaults to GOMAXPROCS. To increase parallelism for
// non-CPU-bound benchmarks, call [B.SetParallelism] before RunParallel.
// RunParallel is usually used with the go test -cpu flag.
//
// The body function will be run in each goroutine. It should set up any
// goroutine-local state and then iterate until pb.Next returns false.
// It should not use the [B.StartTimer], [B.StopTimer], or [B.ResetTimer] functions,
// because they have global effect. It should also not call [B.Run].
//
// RunParallel reports ns/op values as wall time for the benchmark as a whole,
// not the sum of wall time or CPU time over each parallel goroutine.
[GoRecv] public static void RunParallel(this ref B b, Action<ж<PB>> body) => func((defer, _) => {
    if (b.N == 0) {
        return;
    }
    // Nothing to do when probing.
    // Calculate grain size as number of iterations that take ~100µs.
    // 100µs is enough to amortize the overhead and provide sufficient
    // dynamic load balancing.
    ref var grain = ref heap<uint64>(out var Ꮡgrain);
    grain = ((uint64)0);
    if (b.previousN > 0 && b.previousDuration > 0) {
        grain = 1e5F * ((uint64)b.previousN) / ((uint64)b.previousDuration);
    }
    if (grain < 1) {
        grain = 1;
    }
    // We expect the inner loop and function call to take at least 10ns,
    // so do not do more than 100µs/10ns=1e4 iterations.
    if (grain > 1e4F) {
        grain = 1e4F;
    }
    ref var n = ref heap(new sync.atomic_package.Uint64(), out var Ꮡn);
    nint numProcs = b.parallelism * runtime.GOMAXPROCS(0);
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    wg.Add(numProcs);
    for (nint p = 0; p < numProcs; p++) {
        var grainʗ1 = grain;
        var nʗ1 = n;
        var wgʗ1 = wg;
        goǃ(() => {
            var wgʗ2 = wg;
            defer(wgʗ2.Done);
            var pb = Ꮡ(new PB(
                globalN: Ꮡn,
                grain: grain,
                bN: ((uint64)b.N)
            ));
            body(pb);
        });
    }
    wg.Wait();
    if (n.Load() <= ((uint64)b.N) && !b.Failed()) {
        b.Fatal("RunParallel: body exited without pb.Next() == false");
    }
});

// SetParallelism sets the number of goroutines used by [B.RunParallel] to p*GOMAXPROCS.
// There is usually no need to call SetParallelism for CPU-bound benchmarks.
// If p is less than 1, this call will have no effect.
[GoRecv] public static void SetParallelism(this ref B b, nint p) {
    if (p >= 1) {
        b.parallelism = p;
    }
}

// Benchmark benchmarks a single function. It is useful for creating
// custom benchmarks that do not use the "go test" command.
//
// If f depends on testing flags, then [Init] must be used to register
// those flags before calling Benchmark and before calling [flag.Parse].
//
// If f calls Run, the result will be an estimate of running all its
// subbenchmarks that don't call Run in sequence in a single benchmark.
public static BenchmarkResult Benchmark(Action<ж<B>> f) {
    var b = Ꮡ(new B(
        common: new common(
            signal: new channel<bool>(1),
            w: new discard(nil)
        ),
        benchFunc: f,
        benchTime: benchTime
    ));
    if (b.run1()) {
        b.run();
    }
    return (~b).result;
}

[GoType] partial struct discard {
}

internal static (nint n, error err) Write(this discard _, slice<byte> b) {
    nint n = default!;
    error err = default!;

    return (len(b), default!);
}

} // end testing_package
