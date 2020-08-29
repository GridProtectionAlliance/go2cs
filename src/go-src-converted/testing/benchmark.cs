// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2020 August 29 10:05:48 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\benchmark.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using race = go.@internal.race_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class testing_package
    {
        private static var matchBenchmarks = flag.String("test.bench", "", "run only benchmarks matching `regexp`");
        private static var benchTime = flag.Duration("test.benchtime", 1L * time.Second, "run each benchmark for duration `d`");
        private static var benchmarkMemory = flag.Bool("test.benchmem", false, "print memory allocations for benchmarks");

        // Global lock to ensure only one benchmark runs at a time.
        private static sync.Mutex benchmarkLock = default;

        // Used for every benchmark for measuring memory.
        private static runtime.MemStats memStats = default;

        // An internal type but exported because it is cross-package; part of the implementation
        // of the "go test" command.
        public partial struct InternalBenchmark
        {
            public @string Name;
            public Action<ref B> F;
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
        // and dumped to standard error when done. Unlike in tests, benchmark logs
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
            public Action<ref B> benchFunc;
            public time.Duration benchTime;
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
            public ulong netBytes;
        }

        // StartTimer starts timing a test. This function is called automatically
        // before a benchmark starts, but it can also used to resume timing after
        // a call to StopTimer.
        private static void StartTimer(this ref B b)
        {
            if (!b.timerOn)
            {
                runtime.ReadMemStats(ref memStats);
                b.startAllocs = memStats.Mallocs;
                b.startBytes = memStats.TotalAlloc;
                b.start = time.Now();
                b.timerOn = true;
            }
        }

        // StopTimer stops timing a test. This can be used to pause the timer
        // while performing complex initialization that you don't
        // want to measure.
        private static void StopTimer(this ref B b)
        {
            if (b.timerOn)
            {
                b.duration += time.Since(b.start);
                runtime.ReadMemStats(ref memStats);
                b.netAllocs += memStats.Mallocs - b.startAllocs;
                b.netBytes += memStats.TotalAlloc - b.startBytes;
                b.timerOn = false;
            }
        }

        // ResetTimer zeros the elapsed benchmark time and memory allocation counters.
        // It does not affect whether the timer is running.
        private static void ResetTimer(this ref B b)
        {
            if (b.timerOn)
            {
                runtime.ReadMemStats(ref memStats);
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
        private static void SetBytes(this ref B b, long n)
        {
            b.bytes = n;

        }

        // ReportAllocs enables malloc statistics for this benchmark.
        // It is equivalent to setting -test.benchmem, but it only affects the
        // benchmark function that calls ReportAllocs.
        private static void ReportAllocs(this ref B b)
        {
            b.showAllocResult = true;
        }

        private static long nsPerOp(this ref B b)
        {
            if (b.N <= 0L)
            {
                return 0L;
            }
            return b.duration.Nanoseconds() / int64(b.N);
        }

        // runN runs a single benchmark for the specified number of iterations.
        private static void runN(this ref B _b, long n) => func(_b, (ref B b, Defer defer, Panic _, Recover __) =>
        {
            benchmarkLock.Lock();
            defer(benchmarkLock.Unlock()); 
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

        // roundDown10 rounds a number down to the nearest power of 10.
        private static long roundDown10(long n)
        {
            long tens = 0L; 
            // tens = floor(log_10(n))
            while (n >= 10L)
            {
                n = n / 10L;
                tens++;
            } 
            // result = 10^tens
 
            // result = 10^tens
            long result = 1L;
            for (long i = 0L; i < tens; i++)
            {
                result *= 10L;
            }

            return result;
        }

        // roundUp rounds x up to a number of the form [1eX, 2eX, 3eX, 5eX].
        private static long roundUp(long n)
        {
            var @base = roundDown10(n);

            if (n <= base) 
                return base;
            else if (n <= (2L * base)) 
                return 2L * base;
            else if (n <= (3L * base)) 
                return 3L * base;
            else if (n <= (5L * base)) 
                return 5L * base;
            else 
                return 10L * base;
                    }

        // run1 runs the first iteration of benchFunc. It returns whether more
        // iterations of this benchmarks should be run.
        private static bool run1(this ref B _b) => func(_b, (ref B b, Defer defer, Panic _, Recover __) =>
        {
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
            if (atomic.LoadInt32(ref b.hasSub) != 0L || b.finished)
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
        private static void run(this ref B b)
        {
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

        private static BenchmarkResult doBench(this ref B b)
        {
            go_(() => b.launch());
            b.signal.Receive();
            return b.result;
        }

        // launch launches the benchmark function. It gradually increases the number
        // of benchmark iterations until the benchmark runs for the requested benchtime.
        // launch is run by the doBench function as a separate goroutine.
        // run1 must have been called on b.
        private static void launch(this ref B _b) => func(_b, (ref B b, Defer defer, Panic _, Recover __) =>
        { 
            // Signal that we're done whether we return normally
            // or by FailNow's runtime.Goexit.
            defer(() =>
            {
                b.signal.Send(true);
            }()); 

            // Run the benchmark for at least the specified amount of time.
            var d = b.benchTime;
            {
                long n = 1L;

                while (!b.failed && b.duration < d && n < 1e9F)
                {
                    var last = n; 
                    // Predict required iterations.
                    n = int(d.Nanoseconds());
                    {
                        var nsop = b.nsPerOp();

                        if (nsop != 0L)
                        {
                            n /= int(nsop);
                        } 
                        // Run more iterations than we think we'll need (1.2x).
                        // Don't grow too fast in case we had timing errors previously.
                        // Be sure to run at least one more than last time.

                    } 
                    // Run more iterations than we think we'll need (1.2x).
                    // Don't grow too fast in case we had timing errors previously.
                    // Be sure to run at least one more than last time.
                    n = max(min(n + n / 5L, 100L * last), last + 1L); 
                    // Round up to something easy to read.
                    n = roundUp(n);
                    b.runN(n);
                }

            }
            b.result = new BenchmarkResult(b.N,b.duration,b.bytes,b.netAllocs,b.netBytes);
        });

        // The results of a benchmark run.
        public partial struct BenchmarkResult
        {
            public long N; // The number of iterations.
            public time.Duration T; // The total time taken.
            public long Bytes; // Bytes processed in one iteration.
            public ulong MemAllocs; // The total number of memory allocations.
            public ulong MemBytes; // The total number of bytes allocated.
        }

        public static long NsPerOp(this BenchmarkResult r)
        {
            if (r.N <= 0L)
            {
                return 0L;
            }
            return r.T.Nanoseconds() / int64(r.N);
        }

        public static double mbPerSec(this BenchmarkResult r)
        {
            if (r.Bytes <= 0L || r.T <= 0L || r.N <= 0L)
            {
                return 0L;
            }
            return (float64(r.Bytes) * float64(r.N) / 1e6F) / r.T.Seconds();
        }

        // AllocsPerOp returns r.MemAllocs / r.N.
        public static long AllocsPerOp(this BenchmarkResult r)
        {
            if (r.N <= 0L)
            {
                return 0L;
            }
            return int64(r.MemAllocs) / int64(r.N);
        }

        // AllocedBytesPerOp returns r.MemBytes / r.N.
        public static long AllocedBytesPerOp(this BenchmarkResult r)
        {
            if (r.N <= 0L)
            {
                return 0L;
            }
            return int64(r.MemBytes) / int64(r.N);
        }

        public static @string String(this BenchmarkResult r)
        {
            var mbs = r.mbPerSec();
            @string mb = "";
            if (mbs != 0L)
            {
                mb = fmt.Sprintf("\t%7.2f MB/s", mbs);
            }
            var nsop = r.NsPerOp();
            var ns = fmt.Sprintf("%10d ns/op", nsop);
            if (r.N > 0L && nsop < 100L)
            { 
                // The format specifiers here make sure that
                // the ones digits line up for all three possible formats.
                if (nsop < 10L)
                {
                    ns = fmt.Sprintf("%13.2f ns/op", float64(r.T.Nanoseconds()) / float64(r.N));
                }
                else
                {
                    ns = fmt.Sprintf("%12.1f ns/op", float64(r.T.Nanoseconds()) / float64(r.N));
                }
            }
            return fmt.Sprintf("%8d\t%s%s", r.N, ns, mb);
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

        // An internal function but exported because it is cross-package; part of the implementation
        // of the "go test" command.
        public static (bool, error) RunBenchmarks(Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks)
        {
            runBenchmarks("", matchString, benchmarks);
        }

        private static bool runBenchmarks(@string importPath, Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks)
        { 
            // If no flag was specified, don't run benchmarks.
            if (len(matchBenchmarks.Value) == 0L)
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
            benchContext ctx = ref new benchContext(match:newMatcher(matchString,*matchBenchmarks,"-test.bench"),extLen:len(benchmarkName("",maxprocs)),);
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

            B main = ref new B(common:common{name:"Main",w:os.Stdout,chatty:*chatty,},importPath:importPath,benchFunc:func(b*B){for_,Benchmark:=rangebs{b.Run(Benchmark.Name,Benchmark.F)}},benchTime:*benchTime,context:ctx,);
            main.runN(1L);
            return !main.failed;
        }

        // processBench runs bench b for the configured CPU counts and prints the results.
        private static void processBench(this ref benchContext ctx, ref B b)
        {
            foreach (var (i, procs) in cpuList)
            {
                for (var j = uint(0L); j < count.Value; j++)
                {
                    runtime.GOMAXPROCS(procs);
                    var benchName = benchmarkName(b.name, procs);
                    fmt.Fprintf(b.w, "%-*s\t", ctx.maxLen, benchName); 
                    // Recompute the running time for all but the first iteration.
                    if (i > 0L || j > 0L)
                    {
                        b = ref new B(common:common{signal:make(chanbool),name:b.name,w:b.w,chatty:b.chatty,},benchFunc:b.benchFunc,benchTime:b.benchTime,);
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
                    if (benchmarkMemory || b.showAllocResult.Value)
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
        private static bool Run(this ref B _b, @string name, Action<ref B> f) => func(_b, (ref B b, Defer defer, Panic _, Recover __) =>
        { 
            // Since b has subbenchmarks, we will no longer run it as a benchmark itself.
            // Release the lock and acquire it on exit to ensure locks stay paired.
            atomic.StoreInt32(ref b.hasSub, 1L);
            benchmarkLock.Unlock();
            defer(benchmarkLock.Lock());

            var benchName = b.name;
            var ok = true;
            var partial = false;
            if (b.context != null)
            {
                benchName, ok, partial = b.context.match.fullName(ref b.common, name);
            }
            if (!ok)
            {
                return true;
            }
            B sub = ref new B(common:common{signal:make(chanbool),name:benchName,parent:&b.common,level:b.level+1,w:b.w,chatty:b.chatty,},importPath:b.importPath,benchFunc:f,benchTime:b.benchTime,context:b.context,);
            if (partial)
            { 
                // Partial name match, like -bench=X/Y matching BenchmarkX.
                // Only process sub-benchmarks, if any.
                atomic.StoreInt32(ref sub.hasSub, 1L);
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
        private static void add(this ref B b, BenchmarkResult other)
        {
            var r = ref b.result; 
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
        private static void trimOutput(this ref B b)
        { 
            // The output is likely to appear multiple times because the benchmark
            // is run multiple times, but at least it will be seen. This is not a big deal
            // because benchmarks rarely print, but just in case, we trim it if it's too long.
            const long maxNewlines = 10L;

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
        private static bool Next(this ref PB pb)
        {
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
        private static void RunParallel(this ref B _b, Action<ref PB> body) => func(_b, (ref B b, Defer defer, Panic _, Recover __) =>
        {
            if (b.N == 0L)
            {
                return; // Nothing to do when probing.
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
            var n = uint64(0L);
            var numProcs = b.parallelism * runtime.GOMAXPROCS(0L);
            sync.WaitGroup wg = default;
            wg.Add(numProcs);
            for (long p = 0L; p < numProcs; p++)
            {
                go_(() => () =>
                {
                    defer(wg.Done());
                    PB pb = ref new PB(globalN:&n,grain:grain,bN:uint64(b.N),);
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
        private static void SetParallelism(this ref B b, long p)
        {
            if (p >= 1L)
            {
                b.parallelism = p;
            }
        }

        // Benchmark benchmarks a single function. Useful for creating
        // custom benchmarks that do not use the "go test" command.
        //
        // If f calls Run, the result will be an estimate of running all its
        // subbenchmarks that don't call Run in sequence in a single benchmark.
        public static BenchmarkResult Benchmark(Action<ref B> f)
        {
            B b = ref new B(common:common{signal:make(chanbool),w:discard{},},benchFunc:f,benchTime:*benchTime,);
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
            return (len(b), null);
        }
    }
}
