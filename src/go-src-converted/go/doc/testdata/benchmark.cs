// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testing -- go2cs converted at 2022 March 13 05:52:39 UTC
// import "go/doc.testing" ==> using testing = go.go.doc.testing_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\benchmark.go
namespace go.go;

using flag = flag_package;
using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using time = time_package;
using System;
using System.Threading;

public static partial class testing_package {

private static var matchBenchmarks = flag.String("test.bench", "", "regular expression to select benchmarks to run");
private static var benchTime = flag.Duration("test.benchtime", 1 * time.Second, "approximate run time for each benchmark");

// An internal type but exported because it is cross-package; part of the implementation
// of go test.
public partial struct InternalBenchmark {
    public @string Name;
    public Action<ptr<B>> F;
}

// B is a type passed to Benchmark functions to manage benchmark
// timing and to specify the number of iterations to run.
public partial struct B {
    public ref common common => ref common_val;
    public nint N;
    public InternalBenchmark benchmark;
    public long bytes;
    public bool timerOn;
    public BenchmarkResult result;
}

// StartTimer starts timing a test. This function is called automatically
// before a benchmark starts, but it can also used to resume timing after
// a call to StopTimer.
private static void StartTimer(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;

    if (!b.timerOn) {
        b.start = time.Now();
        b.timerOn = true;
    }
}

// StopTimer stops timing a test. This can be used to pause the timer
// while performing complex initialization that you don't
// want to measure.
private static void StopTimer(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;

    if (b.timerOn) {
        b.duration += time.Now().Sub(b.start);
        b.timerOn = false;
    }
}

// ResetTimer sets the elapsed benchmark time to zero.
// It does not affect whether the timer is running.
private static void ResetTimer(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;

    if (b.timerOn) {
        b.start = time.Now();
    }
    b.duration = 0;
}

// SetBytes records the number of bytes processed in a single operation.
// If this is called, the benchmark will report ns/op and MB/s.
private static void SetBytes(this ptr<B> _addr_b, long n) {
    ref B b = ref _addr_b.val;

    b.bytes = n;
}

private static long nsPerOp(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;

    if (b.N <= 0) {
        return 0;
    }
    return b.duration.Nanoseconds() / int64(b.N);
}

// runN runs a single benchmark for the specified number of iterations.
private static void runN(this ptr<B> _addr_b, nint n) {
    ref B b = ref _addr_b.val;
 
    // Try to get a comparable environment for each run
    // by clearing garbage from previous runs.
    runtime.GC();
    b.N = n;
    b.ResetTimer();
    b.StartTimer();
    b.benchmark.F(b);
    b.StopTimer();
}

private static nint min(nint x, nint y) {
    if (x > y) {
        return y;
    }
    return x;
}

private static nint max(nint x, nint y) {
    if (x < y) {
        return y;
    }
    return x;
}

// roundDown10 rounds a number down to the nearest power of 10.
private static nint roundDown10(nint n) {
    nint tens = 0; 
    // tens = floor(log_10(n))
    while (n > 10) {
        n = n / 10;
        tens++;
    } 
    // result = 10^tens
    nint result = 1;
    for (nint i = 0; i < tens; i++) {
        result *= 10;
    }
    return result;
}

// roundUp rounds x up to a number of the form [1eX, 2eX, 5eX].
private static nint roundUp(nint n) {
    var @base = roundDown10(n);
    if (n < (2 * base)) {
        return 2 * base;
    }
    if (n < (5 * base)) {
        return 5 * base;
    }
    return 10 * base;
}

// run times the benchmark function in a separate goroutine.
private static BenchmarkResult run(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;

    go_(() => b.launch());
    b.signal.Receive();
    return b.result;
}

// launch launches the benchmark function. It gradually increases the number
// of benchmark iterations until the benchmark runs for a second in order
// to get a reasonable measurement. It prints timing information in this form
//        testing.BenchmarkHello    100000        19 ns/op
// launch is run by the fun function as a separate goroutine.
private static void launch(this ptr<B> _addr_b) => func((defer, _, _) => {
    ref B b = ref _addr_b.val;
 
    // Run the benchmark for a single iteration in case it's expensive.
    nint n = 1; 

    // Signal that we're done whether we return normally
    // or by FailNow's runtime.Goexit.
    defer(() => {
        b.signal.Send(b);
    }());

    b.runN(n); 
    // Run the benchmark for at least the specified amount of time.
    var d = benchTime.val;
    while (!b.failed && b.duration < d && n < 1e9F) {
        var last = n; 
        // Predict iterations/sec.
        if (b.nsPerOp() == 0) {
            n = 1e9F;
        }
        else
 {
            n = int(d.Nanoseconds() / b.nsPerOp());
        }
        n = max(min(n + n / 2, 100 * last), last + 1); 
        // Round up to something easy to read.
        n = roundUp(n);
        b.runN(n);
    }
    b.result = new BenchmarkResult(b.N,b.duration,b.bytes);
});

// The results of a benchmark run.
public partial struct BenchmarkResult {
    public nint N; // The number of iterations.
    public time.Duration T; // The total time taken.
    public long Bytes; // Bytes processed in one iteration.
}

public static long NsPerOp(this BenchmarkResult r) {
    if (r.N <= 0) {
        return 0;
    }
    return r.T.Nanoseconds() / int64(r.N);
}

public static double mbPerSec(this BenchmarkResult r) {
    if (r.Bytes <= 0 || r.T <= 0 || r.N <= 0) {
        return 0;
    }
    return (float64(r.Bytes) * float64(r.N) / 1e6F) / r.T.Seconds();
}

public static @string String(this BenchmarkResult r) {
    var mbs = r.mbPerSec();
    @string mb = "";
    if (mbs != 0) {
        mb = fmt.Sprintf("\t%7.2f MB/s", mbs);
    }
    var nsop = r.NsPerOp();
    var ns = fmt.Sprintf("%10d ns/op", nsop);
    if (r.N > 0 && nsop < 100) { 
        // The format specifiers here make sure that
        // the ones digits line up for all three possible formats.
        if (nsop < 10) {
            ns = fmt.Sprintf("%13.2f ns/op", float64(r.T.Nanoseconds()) / float64(r.N));
        }
        else
 {
            ns = fmt.Sprintf("%12.1f ns/op", float64(r.T.Nanoseconds()) / float64(r.N));
        }
    }
    return fmt.Sprintf("%8d\t%s%s", r.N, ns, mb);
}

// An internal function but exported because it is cross-package; part of the implementation
// of go test.
public static (bool, error) RunBenchmarks(Func<@string, @string, (bool, error)> matchString, slice<InternalBenchmark> benchmarks) {
    bool _p0 = default;
    error _p0 = default!;
 
    // If no flag was specified, don't run benchmarks.
    if (len(matchBenchmarks.val) == 0) {
        return ;
    }
    foreach (var (_, Benchmark) in benchmarks) {
        var (matched, err) = matchString(matchBenchmarks.val, Benchmark.Name);
        if (err != null) {
            fmt.Fprintf(os.Stderr, "testing: invalid regexp for -test.bench: %s\n", err);
            os.Exit(1);
        }
        if (!matched) {
            continue;
        }
        foreach (var (_, procs) in cpuList) {
            runtime.GOMAXPROCS(procs);
            ptr<B> b = addr(new B(common:common{signal:make(chaninterface{}),},benchmark:Benchmark,));
            var benchName = Benchmark.Name;
            if (procs != 1) {
                benchName = fmt.Sprintf("%s-%d", Benchmark.Name, procs);
            }
            fmt.Printf("%s\t", benchName);
            var r = b.run();
            if (b.failed) { 
                // The output could be very long here, but probably isn't.
                // We print it all, regardless, because we don't want to trim the reason
                // the benchmark failed.
                fmt.Printf("--- FAIL: %s\n%s", benchName, b.output);
                continue;
            }
            fmt.Printf("%v\n", r); 
            // Unlike with tests, we ignore the -chatty flag and always print output for
            // benchmarks since the output generation time will skew the results.
            if (len(b.output) > 0) {
                b.trimOutput();
                fmt.Printf("--- BENCH: %s\n%s", benchName, b.output);
            }
            {
                var p = runtime.GOMAXPROCS(-1);

                if (p != procs) {
                    fmt.Fprintf(os.Stderr, "testing: %s left GOMAXPROCS set to %d\n", benchName, p);
                }

            }
        }
    }
}

// trimOutput shortens the output from a benchmark, which can be very long.
private static void trimOutput(this ptr<B> _addr_b) {
    ref B b = ref _addr_b.val;
 
    // The output is likely to appear multiple times because the benchmark
    // is run multiple times, but at least it will be seen. This is not a big deal
    // because benchmarks rarely print, but just in case, we trim it if it's too long.
    const nint maxNewlines = 10;

    for (nint nlCount = 0;
    nint j = 0; j < len(b.output); j++) {
        if (b.output[j] == '\n') {
            nlCount++;
            if (nlCount >= maxNewlines) {
                b.output = append(b.output[..(int)j], "\n\t... [output truncated]\n");
                break;
            }
        }
    }
}

// Benchmark benchmarks a single function. Useful for creating
// custom benchmarks that do not use go test.
public static BenchmarkResult Benchmark(Action<ptr<B>> f) {
    ptr<B> b = addr(new B(common:common{signal:make(chaninterface{}),},benchmark:InternalBenchmark{"",f},));
    return b.run();
}

} // end testing_package
