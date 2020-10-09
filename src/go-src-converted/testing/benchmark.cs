// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 October 09 05:47:38 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\benchmark.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using race = go.@internal.race_package;
using io = go.io_package;
using math = go.math_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using unicode = go.unicode_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class testing_package
    {
        private static void initBenchmarkFlags()
        {
            matchBenchmarks = flag.String("test.bench", "", "run only benchmarks matching `regexp`");
            benchmarkMemory = flag.Bool("test.benchmem", false, "print memory allocations for benchmarks");
            flag.Var(_addr_benchTime, "test.benchtime", "run each benchmark for duration `d`");
        }

        private static ptr<@string> matchBenchmarks;        private static ptr<bool> benchmarkMemory;        private static benchTimeFlag benchTime = new benchTimeFlag(d:1*time.Second);

        private partial struct benchTimeFlag
        {
            public time.Duration d;
            public long n;
        }

        private static @string String(this ptr<benchTimeFlag> _addr_f)
        {
            ref benchTimeFlag f = ref _addr_f.val;

            if (f.n > 0L)
            {
                return fmt.Sprintf("%dx", f.n);
            }

            return time.Duration(f.d).String();

        }

        private static error Set(this ptr<benchTimeFlag> _addr_f, @string s)
        {
            ref benchTimeFlag f = ref _addr_f.val;

            if (strings.HasSuffix(s, "x"))
            {
                var (n, err) = strconv.ParseInt(s[..len(s) - 1L], 10L, 0L);
                if (err != null || n <= 0L)
                {
                    return error.As(fmt.Errorf("invalid count"))!;
                }

                f.val = new benchTimeFlag(n:int(n));
                return error.As(null!)!;

            }

            var (d, err) = time.ParseDuration(s);
            if (err != null || d <= 0L)
            {
                return error.As(fmt.Errorf("invalid duration"))!;
            }

            f.val = new benchTimeFlag(d:d);
            return error.As(null!)!;

        }

        // Global lock to ensure only one benchmark runs at a time.
        private static sync.Mutex benchmarkLock = default;

        // Used for every benchmark for measuring memory.
        private static runtime.MemStats memStats = default;

        // InternalBenchmark is an internal type but exported because it is cross-package;
        // it is part of the implementation of the "go test" command.
        public partial struct InternalBenchmark
        {
            public @string Name;
            public Action<ptr<B>> F;
        }

        // B is a type passed to Benchmark functions to manage benchmark
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
        public partial struct B
        {
            public ref common common => ref common_val;
            public @string importPath; // import path of the package containing the benchmark
            public ptr<benchContext> context;
            public long N;
            public long previousN; // number of iterations in the previous run
            public time.Duration previousDuration; // total duration of the previous run
            public Action<ptr<B>> benchFunc;
            public benchTimeFlag benchTime;
            public long bytes;
            public bool missingBytes; // one of the subbenchmarks does not have bytes set.
            public bool timerOn;
            public bool showAllocResult;
            public BenchmarkResult result;
            public long parallelism; // RunParallel creates parallelism*GOMAXPROCS goroutines
// The initial states of memStats.Mallocs and memStats.TotalAlloc.
            public ulong startAllocs;
            public ulong startBytes; // The net total of this test after being run.
            public ulong netAllocs;
            public ulong netBytes; // Extra metrics collected by ReportMetric.
            public map<@string, double> extra;
        }

        // StartTimer starts timing a test. This function is called automatically
        // before a benchmark starts, but it can also be used to resume timing after
        // a call to StopTimer.
        private static void StartTimer(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            if (!b.timerOn)
            {
                runtime.ReadMemStats(_addr_memStats);
                b.startAllocs = memStats.Mallocs;
                b.startBytes = memStats.TotalAlloc;
                b.start = time.Now();
                b.timerOn = true;
            }

        }

        // StopTimer stops timing a test. This can be used to pause the timer
        // while performing complex initialization that you don't
        // want to measure.
        private static void StopTimer(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            if (b.timerOn)
            {
                b.duration += time.Since(b.start);
                runtime.ReadMemStats(_addr_memStats);
                b.netAllocs += memStats.Mallocs - b.startAllocs;
                b.netBytes += memStats.TotalAlloc - b.startBytes;
                b.timerOn = false;
            }

        }

        // ResetTimer zeroes the elapsed benchmark time and memory allocation counters
        // and deletes user-reported metrics.
        // It does not affect whether the timer is running.
        private static void ResetTimer(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            if (b.extra == null)
            { 
                // Allocate the extra map before reading memory stats.
                // Pre-size it to make more allocation unlikely.
                b.extra = make_map<@string, double>(16L);

            }
            else
            {
                foreach (var (k) in b.extra)
                {
                    delete(b.extra, k);
                }

            }

            if (b.timerOn)
            {
                runtime.ReadMemStats(_addr_memStats);
                b.startAllocs = memStats.Mallocs;
                b.startBytes = memStats.TotalAlloc;
                b.start = time.Now();
            }

            b.duration = 0L;
            b.netAllocs = 0L;
            b.netBytes = 0L;

        }

        // SetBytes records the number of bytes processed in a single operation.
        // If this is called, the benchmark will report ns/op and MB/s.
        private static void SetBytes(this ptr<B> _addr_b, long n)
        {
            ref B b = ref _addr_b.val;

            b.bytes = n;
        }

        // ReportAllocs enables malloc statistics for this benchmark.
        // It is equivalent to setting -test.benchmem, but it only affects the
        // benchmark function that calls ReportAllocs.
        private static void ReportAllocs(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            b.showAllocResult = true;
        }

        // runN runs a single benchmark for the specified number of iterations.
        private static void runN(this ptr<B> _addr_b, long n) => func((defer, _, __) =>
        {
            ref B b = ref _addr_b.val;

            benchmarkLock.Lock();
            defer(benchmarkLock.Unlock());
            defer(b.runCleanup(normalPanic)); 
            // Try to get a comparable environment for each run
            // by clearing garbage from previous runs.
            runtime.GC();
            b.raceErrors = -race.Errors();
            b.N = n;
            b.parallelism = 1L;
            b.ResetTimer();
            b.StartTimer();
            b.benchFunc(b);
            b.StopTimer();
            b.previousN = n;
            b.previousDuration = b.duration;
            b.raceErrors += race.Errors();
            if (b.raceErrors > 0L)
            {
                b.Errorf("race detected during execution of benchmark");
            }

        });

        private static long min(long x, long y)
        {
            if (x > y)
            {
                return y;
            }

            return x;

        }

        private static long max(long x, long y)
        {
            if (x < y)
            {
                return y;
            }

            return x;

        }

        // run1 runs the first iteration of benchFunc. It reports whether more
        // iterations of this benchmarks should be run.
        private static bool run1(this ptr<B> _addr_b) => func((defer, _, __) =>
        {
            ref B b = ref _addr_b.val;

            {
                var ctx = b.context;

                if (ctx != null)
                { 
                    // Extend maxLen, if needed.
                    {
                        var n = len(b.name) + ctx.extLen + 1L;

                        if (n > ctx.maxLen)
                        {
                            ctx.maxLen = n + 8L; // Add additional slack to avoid too many jumps in size.
                        }

                    }

                }

            }

            go_(() => () =>
            { 
                // Signal that we're done whether we return normally
                // or by FailNow's runtime.Goexit.
                defer(() =>
                {
                    b.signal.Send(true);
                }());

                b.runN(1L);

            }());
            b.signal.Receive();
            if (b.failed)
            {
                fmt.Fprintf(b.w, "--- FAIL: %s\n%s", b.name, b.output);
                return false;
            } 
            // Only print the output if we know we are not going to proceed.
            // Otherwise it is printed in processBench.
            if (atomic.LoadInt32(_addr_b.hasSub) != 0L || b.finished)
            {
                @string tag = "BENCH";
                if (b.skipped)
                {
                    tag = "SKIP";
                }

                if (b.chatty && (len(b.output) > 0L || b.finished))
                {
                    b.trimOutput();
                    fmt.Fprintf(b.w, "--- %s: %s\n%s", tag, b.name, b.output);
                }

                return false;

            }

            return true;

        });

        private static sync.Once labelsOnce = default;

        // run executes the benchmark in a separate goroutine, including all of its
        // subbenchmarks. b must not have subbenchmarks.
        private static void run(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            labelsOnce.Do(() =>
            {
                fmt.Fprintf(b.w, "goos: %s\n", runtime.GOOS);
                fmt.Fprintf(b.w, "goarch: %s\n", runtime.GOARCH);
                if (b.importPath != "")
                {
                    fmt.Fprintf(b.w, "pkg: %s\n", b.importPath);
                }

            });
            if (b.context != null)
            { 
                // Running go test --test.bench
                b.context.processBench(b); // Must call doBench.
            }
            else
            { 
                // Running func Benchmark.
                b.doBench();

            }

        }

        private static BenchmarkResult doBench(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;

            go_(() => b.launch());
            b.signal.Receive();
            return b.result;
        }

        // launch launches the benchmark function. It gradually increases the number
        // of benchmark iterations until the benchmark runs for the requested benchtime.
        // launch is run by the doBench function as a separate goroutine.
        // run1 must have been called on b.
        private static void launch(this ptr<B> _addr_b) => func((defer, _, __) =>
        {
            ref B b = ref _addr_b.val;
 
            // Signal that we're done whether we return normally
            // or by FailNow's runtime.Goexit.
            defer(() =>
            {
                b.signal.Send(true);
            }()); 

            // Run the benchmark for at least the specified amount of time.
            if (b.benchTime.n > 0L)
            {
                b.runN(b.benchTime.n);
            }
            else
            {
                var d = b.benchTime.d;
                {
                    var n = int64(1L);

                    while (!b.failed && b.duration < d && n < 1e9F)
                    {
                        var last = n; 
                        // Predict required iterations.
                        var goalns = d.Nanoseconds();
                        var prevIters = int64(b.N);
                        var prevns = b.duration.Nanoseconds();
                        if (prevns <= 0L)
                        { 
                            // Round up, to avoid div by zero.
                            prevns = 1L;

                        } 
                        // Order of operations matters.
                        // For very fast benchmarks, prevIters ~= prevns.
                        // If you divide first, you get 0 or 1,
                        // which can hide an order of magnitude in execution time.
                        // So multiply first, then divide.
                        n = goalns * prevIters / prevns; 
                        // Run more iterations than we think we'll need (1.2x).
                        n += n / 5L; 
                        // Don't grow too fast in case we had timing errors previously.
                        n = min(n, 100L * last); 
                        // Be sure to run at least one more than last time.
                        n = max(n, last + 1L); 
                        // Don't run more than 1e9 times. (This also keeps n in int range on 32 bit platforms.)
                        n = min(n, 1e9F);
                        b.runN(int(n));

                    }

                }

            }

            b.result = new BenchmarkResult(b.N,b.duration,b.bytes,b.netAllocs,b.netBytes,b.extra);

        });

        // ReportMetric adds "n unit" to the reported benchmark results.
        // If the metric is per-iteration, the caller should divide by b.N,
        // and by convention units should end in "/op".
        // ReportMetric overrides any previously reported value for the same unit.
        // ReportMetric panics if unit is the empty string or if unit contains
        // any whitespace.
        // If unit is a unit normally reported by the benchmark framework itself
        // (such as "allocs/op"), ReportMetric will override that metric.
        // Setting "ns/op" to 0 will suppress that built-in metric.
        private static void ReportMetric(this ptr<B> _addr_b, double n, @string unit) => func((_, panic, __) =>
        {
            ref B b = ref _addr_b.val;

            if (unit == "")
            {
                panic("metric unit must not be empty");
            }

            if (strings.IndexFunc(unit, unicode.IsSpace) >= 0L)
            {
                panic("metric unit must not contain whitespace");
            }

            b.extra[unit] = n;

        });

        // BenchmarkResult contains the results of a benchmark run.
        public partial struct BenchmarkResult
        {
            public long N; // The number of iterations.
            public time.Duration T; // The total time taken.
            public long Bytes; // Bytes processed in one iteration.
            public ulong MemAllocs; // The total number of memory allocations.
            public ulong MemBytes; // The total number of bytes allocated.

// Extra records additional metrics reported by ReportMetric.
            public map<@string, double> Extra;
        }

        // NsPerOp returns the "ns/op" metric.
        public static long NsPerOp(this BenchmarkResult r)
        {
            {
                var (v, ok) = r.Extra["ns/op"];

                if (ok)
                {
                    return int64(v);
                }

            }

            if (r.N <= 0L)
            {
                return 0L;
            }

            return r.T.Nanoseconds() / int64(r.N);

        }

        // mbPerSec returns the "MB/s" metric.
        public static double mbPerSec(this BenchmarkResult r)
        {
            {
                var (v, ok) = r.Extra["MB/s"];

                if (ok)
                {
                    return v;
                }

            }

            if (r.Bytes <= 0L || r.T <= 0L || r.N <= 0L)
            {
                return 0L;
            }

            return (float64(r.Bytes) * float64(r.N) / 1e6F) / r.T.Seconds();

        }

        // AllocsPerOp returns the "allocs/op" metric,
        // which is calculated as r.MemAllocs / r.N.
        public static long AllocsPerOp(this BenchmarkResult r)
        {
            {
                var (v, ok) = r.Extra["allocs/op"];

                if (ok)
                {
                    return int64(v);
                }

            }

            if (r.N <= 0L)
            {
                return 0L;
            }

            return int64(r.MemAllocs) / int64(r.N);

        }

        // AllocedBytesPerOp returns the "B/op" metric,
        // which is calculated as r.MemBytes / r.N.
        public static long AllocedBytesPerOp(this BenchmarkResult r)
        {
            {
                var (v, ok) = r.Extra["B/op"];

                if (ok)
                {
                    return int64(v);
                }

            }

            if (r.N <= 0L)
            {
                return 0L;
            }

            return int64(r.MemBytes) / int64(r.N);

        }

        // String returns a summary of the benchmark results.
        // It follows the benchmark result line format from
        // https://golang.org/design/14313-benchmark-format, not including the
        // benchmark name.
        // Extra metrics override built-in metrics of the same name.
        // String does not include allocs/op or B/op, since those are reported
        // by MemString.
        public static @string String(this BenchmarkResult r)
        {
            ptr<object> buf = @new<strings.Builder>();
            fmt.Fprintf(buf, "%8d", r.N); 

            // Get ns/op as a float.
            var (ns, ok) = r.Extra["ns/op"];
            if (!ok)
            {
                ns = float64(r.T.Nanoseconds()) / float64(r.N);
            }

            if (ns != 0L)
            {
                buf.WriteByte('\t');
                prettyPrint(buf, ns, "ns/op");
            }

            {
                var mbs = r.mbPerSec();

                if (mbs != 0L)
                {
                    fmt.Fprintf(buf, "\t%7.2f MB/s", mbs);
                } 

                // Print extra metrics that aren't represented in the standard
                // metrics.

            } 

            // Print extra metrics that aren't represented in the standard
            // metrics.
            slice<@string> extraKeys = default;
            {
                var k__prev1 = k;

                foreach (var (__k) in r.Extra)
                {
                    k = __k;
                    switch (k)
                    {
                        case "ns/op": 
                            // Built-in metrics reported elsewhere.

                        case "MB/s": 
                            // Built-in metrics reported elsewhere.

                        case "B/op": 
                            // Built-in metrics reported elsewhere.

                        case "allocs/op": 
                            // Built-in metrics reported elsewhere.
                            continue;
                            break;
                    }
                    extraKeys = append(extraKeys, k);

                }

                k = k__prev1;
            }

            sort.Strings(extraKeys);
            {
                var k__prev1 = k;

                foreach (var (_, __k) in extraKeys)
                {
                    k = __k;
                    buf.WriteByte('\t');
                    prettyPrint(buf, r.Extra[k], k);
                }

                k = k__prev1;
            }

            return buf.String();

        }

        private static void prettyPrint(io.Writer w, double x, @string unit)
        { 
            // Print all numbers with 10 places before the decimal point
            // and small numbers with three sig figs.
            @string format = default;
            {
                var y = math.Abs(x);


                if (y == 0L || y >= 99.95F) 
                    format = "%10.0f %s";
                else if (y >= 9.995F) 
                    format = "%12.1f %s";
                else if (y >= 0.9995F) 
                    format = "%13.2f %s";
                else if (y >= 0.09995F) 
                    format = "%14.3f %s";
                else if (y >= 0.009995F) 
                    format = "%15.4f %s";
                else if (y >= 0.0009995F) 
                    format = "%16.5f %s";
                else 
                    format = "%17.6f %s";

            }
            fmt.Fprintf(w, format, x, unit);

        }

        // MemString returns r.AllocedBytesPerOp and r.AllocsPerOp in the same format as 'go test'.
        public static @string MemString(this BenchmarkResult r)
        {
            return fmt.Sprintf("%8d B/op\t%8d allocs/op", r.AllocedBytesPerOp(), r.AllocsPerOp());
        }

        // benchmarkName returns full name of benchmark including procs suffix.
        private static @string benchmarkName(@string name, long n)
        {
            if (n != 1L)
            {
                return fmt.Sprintf("%s-%d", name, n);
            }

            return name;

        }

        private partial struct benchContext
        {
            public ptr<matcher> match;
            public long maxLen; // The largest recorded benchmark name.
            public long extLen; // Maximum extension length.
        }

        // RunBenchmarks is an internal function but exported because it is cross-package;
        // it is part of the implementation of the "go test" command.
        public static (bool, error) RunBenchmarks(Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks)
        {
            bool _p0 = default;
            error _p0 = default!;

            runBenchmarks("", matchString, benchmarks);
        }

        private static bool runBenchmarks(@string importPath, Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks)
        { 
            // If no flag was specified, don't run benchmarks.
            if (len(matchBenchmarks.val) == 0L)
            {
                return true;
            } 
            // Collect matching benchmarks and determine longest name.
            long maxprocs = 1L;
            foreach (var (_, procs) in cpuList)
            {
                if (procs > maxprocs)
                {
                    maxprocs = procs;
                }

            }
            ptr<benchContext> ctx = addr(new benchContext(match:newMatcher(matchString,*matchBenchmarks,"-test.bench"),extLen:len(benchmarkName("",maxprocs)),));
            slice<InternalBenchmark> bs = default;
            {
                var Benchmark__prev1 = Benchmark;

                foreach (var (_, __Benchmark) in benchmarks)
                {
                    Benchmark = __Benchmark;
                    {
                        var (_, matched, _) = ctx.match.fullName(null, Benchmark.Name);

                        if (matched)
                        {
                            bs = append(bs, Benchmark);
                            var benchName = benchmarkName(Benchmark.Name, maxprocs);
                            {
                                var l = len(benchName) + ctx.extLen + 1L;

                                if (l > ctx.maxLen)
                                {
                                    ctx.maxLen = l;
                                }

                            }

                        }

                    }

                }

                Benchmark = Benchmark__prev1;
            }

            ptr<B> main = addr(new B(common:common{name:"Main",w:os.Stdout,chatty:*chatty,bench:true,},importPath:importPath,benchFunc:func(b*B){for_,Benchmark:=rangebs{b.Run(Benchmark.Name,Benchmark.F)}},benchTime:benchTime,context:ctx,));
            main.runN(1L);
            return !main.failed;

        }

        // processBench runs bench b for the configured CPU counts and prints the results.
        private static void processBench(this ptr<benchContext> _addr_ctx, ptr<B> _addr_b)
        {
            ref benchContext ctx = ref _addr_ctx.val;
            ref B b = ref _addr_b.val;

            foreach (var (i, procs) in cpuList)
            {
                for (var j = uint(0L); j < count.val; j++)
                {
                    runtime.GOMAXPROCS(procs);
                    var benchName = benchmarkName(b.name, procs); 

                    // If it's chatty, we've already printed this information.
                    if (!b.chatty)
                    {
                        fmt.Fprintf(b.w, "%-*s\t", ctx.maxLen, benchName);
                    } 
                    // Recompute the running time for all but the first iteration.
                    if (i > 0L || j > 0L)
                    {
                        b = addr(new B(common:common{signal:make(chanbool),name:b.name,w:b.w,chatty:b.chatty,bench:true,},benchFunc:b.benchFunc,benchTime:b.benchTime,));
                        b.run1();
                    }

                    var r = b.doBench();
                    if (b.failed)
                    { 
                        // The output could be very long here, but probably isn't.
                        // We print it all, regardless, because we don't want to trim the reason
                        // the benchmark failed.
                        fmt.Fprintf(b.w, "--- FAIL: %s\n%s", benchName, b.output);
                        continue;

                    }

                    var results = r.String();
                    if (b.chatty)
                    {
                        fmt.Fprintf(b.w, "%-*s\t", ctx.maxLen, benchName);
                    }

                    if (benchmarkMemory || b.showAllocResult.val)
                    {
                        results += "\t" + r.MemString();
                    }

                    fmt.Fprintln(b.w, results); 
                    // Unlike with tests, we ignore the -chatty flag and always print output for
                    // benchmarks since the output generation time will skew the results.
                    if (len(b.output) > 0L)
                    {
                        b.trimOutput();
                        fmt.Fprintf(b.w, "--- BENCH: %s\n%s", benchName, b.output);
                    }

                    {
                        var p = runtime.GOMAXPROCS(-1L);

                        if (p != procs)
                        {
                            fmt.Fprintf(os.Stderr, "testing: %s left GOMAXPROCS set to %d\n", benchName, p);
                        }

                    }

                }


            }

        }

        // Run benchmarks f as a subbenchmark with the given name. It reports
        // whether there were any failures.
        //
        // A subbenchmark is like any other benchmark. A benchmark that calls Run at
        // least once will not be measured itself and will be called once with N=1.
        private static bool Run(this ptr<B> _addr_b, @string name, Action<ptr<B>> f) => func((defer, _, __) =>
        {
            ref B b = ref _addr_b.val;
 
            // Since b has subbenchmarks, we will no longer run it as a benchmark itself.
            // Release the lock and acquire it on exit to ensure locks stay paired.
            atomic.StoreInt32(_addr_b.hasSub, 1L);
            benchmarkLock.Unlock();
            defer(benchmarkLock.Lock());

            var benchName = b.name;
            var ok = true;
            var partial = false;
            if (b.context != null)
            {
                benchName, ok, partial = b.context.match.fullName(_addr_b.common, name);
            }

            if (!ok)
            {
                return true;
            }

            array<System.UIntPtr> pc = new array<System.UIntPtr>(maxStackLen);
            var n = runtime.Callers(2L, pc[..]);
            ptr<B> sub = addr(new B(common:common{signal:make(chanbool),name:benchName,parent:&b.common,level:b.level+1,creator:pc[:n],w:b.w,chatty:b.chatty,bench:true,},importPath:b.importPath,benchFunc:f,benchTime:b.benchTime,context:b.context,));
            if (partial)
            { 
                // Partial name match, like -bench=X/Y matching BenchmarkX.
                // Only process sub-benchmarks, if any.
                atomic.StoreInt32(_addr_sub.hasSub, 1L);

            }

            if (b.chatty)
            {
                labelsOnce.Do(() =>
                {
                    fmt.Printf("goos: %s\n", runtime.GOOS);
                    fmt.Printf("goarch: %s\n", runtime.GOARCH);
                    if (b.importPath != "")
                    {
                        fmt.Printf("pkg: %s\n", b.importPath);
                    }

                });

                fmt.Println(benchName);

            }

            if (sub.run1())
            {
                sub.run();
            }

            b.add(sub.result);
            return !sub.failed;

        });

        // add simulates running benchmarks in sequence in a single iteration. It is
        // used to give some meaningful results in case func Benchmark is used in
        // combination with Run.
        private static void add(this ptr<B> _addr_b, BenchmarkResult other)
        {
            ref B b = ref _addr_b.val;

            var r = _addr_b.result; 
            // The aggregated BenchmarkResults resemble running all subbenchmarks as
            // in sequence in a single benchmark.
            r.N = 1L;
            r.T += time.Duration(other.NsPerOp());
            if (other.Bytes == 0L)
            { 
                // Summing Bytes is meaningless in aggregate if not all subbenchmarks
                // set it.
                b.missingBytes = true;
                r.Bytes = 0L;

            }

            if (!b.missingBytes)
            {
                r.Bytes += other.Bytes;
            }

            r.MemAllocs += uint64(other.AllocsPerOp());
            r.MemBytes += uint64(other.AllocedBytesPerOp());

        }

        // trimOutput shortens the output from a benchmark, which can be very long.
        private static void trimOutput(this ptr<B> _addr_b)
        {
            ref B b = ref _addr_b.val;
 
            // The output is likely to appear multiple times because the benchmark
            // is run multiple times, but at least it will be seen. This is not a big deal
            // because benchmarks rarely print, but just in case, we trim it if it's too long.
            const long maxNewlines = (long)10L;

            for (long nlCount = 0L;
            long j = 0L; j < len(b.output); j++)
            {
                if (b.output[j] == '\n')
                {
                    nlCount++;
                    if (nlCount >= maxNewlines)
                    {
                        b.output = append(b.output[..j], "\n\t... [output truncated]\n");
                        break;
                    }

                }

            }


        }

        // A PB is used by RunParallel for running parallel benchmarks.
        public partial struct PB
        {
            public ptr<ulong> globalN; // shared between all worker goroutines iteration counter
            public ulong grain; // acquire that many iterations from globalN at once
            public ulong cache; // local cache of acquired iterations
            public ulong bN; // total number of iterations to execute (b.N)
        }

        // Next reports whether there are more iterations to execute.
        private static bool Next(this ptr<PB> _addr_pb)
        {
            ref PB pb = ref _addr_pb.val;

            if (pb.cache == 0L)
            {
                var n = atomic.AddUint64(pb.globalN, pb.grain);
                if (n <= pb.bN)
                {
                    pb.cache = pb.grain;
                }
                else if (n < pb.bN + pb.grain)
                {
                    pb.cache = pb.bN + pb.grain - n;
                }
                else
                {
                    return false;
                }

            }

            pb.cache--;
            return true;

        }

        // RunParallel runs a benchmark in parallel.
        // It creates multiple goroutines and distributes b.N iterations among them.
        // The number of goroutines defaults to GOMAXPROCS. To increase parallelism for
        // non-CPU-bound benchmarks, call SetParallelism before RunParallel.
        // RunParallel is usually used with the go test -cpu flag.
        //
        // The body function will be run in each goroutine. It should set up any
        // goroutine-local state and then iterate until pb.Next returns false.
        // It should not use the StartTimer, StopTimer, or ResetTimer functions,
        // because they have global effect. It should also not call Run.
        private static void RunParallel(this ptr<B> _addr_b, Action<ptr<PB>> body) => func((defer, _, __) =>
        {
            ref B b = ref _addr_b.val;

            if (b.N == 0L)
            {
                return ; // Nothing to do when probing.
            } 
            // Calculate grain size as number of iterations that take ~100µs.
            // 100µs is enough to amortize the overhead and provide sufficient
            // dynamic load balancing.
            var grain = uint64(0L);
            if (b.previousN > 0L && b.previousDuration > 0L)
            {
                grain = 1e5F * uint64(b.previousN) / uint64(b.previousDuration);
            }

            if (grain < 1L)
            {
                grain = 1L;
            } 
            // We expect the inner loop and function call to take at least 10ns,
            // so do not do more than 100µs/10ns=1e4 iterations.
            if (grain > 1e4F)
            {
                grain = 1e4F;
            }

            ref var n = ref heap(uint64(0L), out ptr<var> _addr_n);
            var numProcs = b.parallelism * runtime.GOMAXPROCS(0L);
            sync.WaitGroup wg = default;
            wg.Add(numProcs);
            for (long p = 0L; p < numProcs; p++)
            {
                go_(() => () =>
                {
                    defer(wg.Done());
                    ptr<PB> pb = addr(new PB(globalN:&n,grain:grain,bN:uint64(b.N),));
                    body(pb);
                }());

            }

            wg.Wait();
            if (n <= uint64(b.N) && !b.Failed())
            {
                b.Fatal("RunParallel: body exited without pb.Next() == false");
            }

        });

        // SetParallelism sets the number of goroutines used by RunParallel to p*GOMAXPROCS.
        // There is usually no need to call SetParallelism for CPU-bound benchmarks.
        // If p is less than 1, this call will have no effect.
        private static void SetParallelism(this ptr<B> _addr_b, long p)
        {
            ref B b = ref _addr_b.val;

            if (p >= 1L)
            {
                b.parallelism = p;
            }

        }

        // Benchmark benchmarks a single function. It is useful for creating
        // custom benchmarks that do not use the "go test" command.
        //
        // If f depends on testing flags, then Init must be used to register
        // those flags before calling Benchmark and before calling flag.Parse.
        //
        // If f calls Run, the result will be an estimate of running all its
        // subbenchmarks that don't call Run in sequence in a single benchmark.
        public static BenchmarkResult Benchmark(Action<ptr<B>> f)
        {
            ptr<B> b = addr(new B(common:common{signal:make(chanbool),w:discard{},},benchFunc:f,benchTime:benchTime,));
            if (b.run1())
            {
                b.run();
            }

            return b.result;

        }

        private partial struct discard
        {
        }

        private static (long, error) Write(this discard _p0, slice<byte> b)
        {
            long n = default;
            error err = default!;

            return (len(b), error.As(null!)!);
        }
    }
}
