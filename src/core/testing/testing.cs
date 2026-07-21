// testing.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using go.testing_runtime;
using any = System.Object;
using ꓸꓸꓸany = System.Span<System.Object>;

namespace go;

/// <summary>
/// Bootstrap implementation of the Go testing package used only by converted test projects.
/// </summary>
[GoPackage("testing")]
public static partial class testing_package
{
    public struct T
    {
        internal TestExecution? Execution;

        internal readonly TestExecution RequiredExecution =>
            Execution ?? throw new InvalidOperationException("testing.T is not attached to a running test");
    }

    public struct M
    {
        internal TestRunner? Runner;
    }

    /// <summary>
    /// The testing.TB interface — the common surface of T and B that test-support packages
    /// (internal/testenv is the driving consumer) accept as parameters. Declared with Go 1.23's
    /// full public member set so the compiled shape never drifts as more helpers convert.
    /// T does not yet implement TB (no suite validated so far passes a T into a TB parameter —
    /// sort uses only testenv's t-free helpers); wiring the implementation lands with the first
    /// suite that needs it.
    /// </summary>
    public interface TB
    {
        void Cleanup(Action cleanup);
        void Error(params ꓸꓸꓸany args);
        void Errorf(@string format, params ꓸꓸꓸany args);
        void Fail();
        void FailNow();
        bool Failed();
        void Fatal(params ꓸꓸꓸany args);
        void Fatalf(@string format, params ꓸꓸꓸany args);
        void Helper();
        void Log(params ꓸꓸꓸany args);
        void Logf(@string format, params ꓸꓸꓸany args);
        @string Name();
        void Setenv(@string key, @string value);
        void Skip(params ꓸꓸꓸany args);
        void SkipNow();
        void Skipf(@string format, params ꓸꓸꓸany args);
        bool Skipped();
        @string TempDir();
    }

    /// <summary>
    /// Benchmark receiver surface. Top-level BenchmarkXxx DECLARATIONS remain disclosed-unsupported
    /// in the manifest (execution is deferred to Phase 4D) and are never registered with the host,
    /// but their converted BODIES still compile into the test assembly, so the members they
    /// reference must exist. Those receiver members (Run and the timer/allocation/failure reporters)
    /// stay safe non-throwing no-ops — a disclosed declaration is never invoked, so there is no run
    /// to time or fail. N is the exception: it is set by <see cref="Benchmark"/>, which DOES drive a
    /// closure in-process (a converted Test can legitimately call testing.Benchmark itself —
    /// unicode's TestCalibrate does), so a b.N loop inside such a closure iterates the measured
    /// count rather than zero times.
    /// </summary>
    public struct B
    {
        public nint N;
    }

    /// <summary>
    /// The result of one <see cref="Benchmark"/> run — the subset of Go's testing.BenchmarkResult a
    /// converted Test function needs when it drives an in-process benchmark (unicode's TestCalibrate
    /// reads NsPerOp()). N is the final iteration count Benchmark settled on. Go's companion field is
    /// T time.Duration; the shim stays time-package-free (see the file's Sprint remark on why no
    /// second stdlib tree is dragged in), so the elapsed wall-clock time is held here as Nanoseconds,
    /// the int64 nanosecond form of that Duration.
    /// </summary>
    public struct BenchmarkResult
    {
        public nint N;
        public long Nanoseconds;

        /// <summary>
        /// Average nanoseconds per iteration — like Go's BenchmarkResult.NsPerOp(). Zero iterations
        /// yields 0 (Go returns 0 rather than dividing when N is non-positive).
        /// </summary>
        public readonly long NsPerOp()
        {
            if (N <= 0)
                return 0L;
            return Nanoseconds / (long)N;
        }
    }

    /// <summary>
    /// Runs benchmark closure <paramref name="f"/> in-process and reports the measurement — a
    /// minimal stand-in for Go's testing.Benchmark. Like Go, it grows N geometrically (starting at
    /// 1) until the run spends a target time budget or hits an iteration ceiling, times the final
    /// run, and returns a <see cref="BenchmarkResult"/>. This is the ONLY path that executes a
    /// benchmark body: it exists for converted Test functions that call testing.Benchmark directly
    /// (unicode's TestCalibrate). Top-level BenchmarkXxx declarations are still never registered or
    /// run (they are disclosed in the manifest), so adding this changes no existing test's behavior.
    /// </summary>
    /// <remarks>
    /// Go's default benchmark time is 1s; the shim uses a smaller budget so an in-process benchmark
    /// completes quickly. The N-growth mirrors Go's predictNextN shape (scale by target/observed,
    /// grow by at most 100x and at least +20%, clamp to the ceiling). Only N and the run's wall time
    /// are measured — the B timer/allocation reporters are the no-op receiver members above, exactly
    /// as for a disclosed benchmark, so a closure that calls them still behaves consistently.
    /// </remarks>
    public static BenchmarkResult Benchmark(Action<ж<B>> f)
    {
        ArgumentNullException.ThrowIfNull(f);

        const long targetNanoseconds = 100L * 1000L * 1000L;   // 100ms budget per benchmark
        const nint maxIterations = 1_000_000_000;              // Go's benchmark N ceiling (1e9)

        nint n = 1;
        long elapsedNanoseconds = 0L;

        while (true)
        {
            ref B b = ref builtin.heap<B>(out ж<B> box);
            b.N = n;

            Stopwatch timer = Stopwatch.StartNew();
            f(box);
            timer.Stop();
            elapsedNanoseconds = (long)timer.Elapsed.TotalNanoseconds;

            if (elapsedNanoseconds >= targetNanoseconds || n >= maxIterations)
                break;

            // Predict the next N: scale by how far under budget the last run was, bounded to a
            // 100x jump above and a +20% floor below, and never past the ceiling (Go's predictNextN).
            nint next;
            if (elapsedNanoseconds <= 0L)
                next = n * 100;
            else
            {
                next = (nint)((double)n * targetNanoseconds / elapsedNanoseconds);
                next = Math.Min(next, n * 100);
                next = Math.Max(next, n + n / 5);
            }

            n = Math.Min(Math.Max(next, n + 1), maxIterations);
        }

        return new BenchmarkResult { N = n, Nanoseconds = elapsedNanoseconds };
    }

    /// <summary>
    /// Parallel-benchmark body handle — COMPILE-ONLY, for the same reason as B above. A Go
    /// parallel benchmark is written as b.RunParallel(func(pb *testing.PB) { for pb.Next() {…} }),
    /// so both the type and its Next method must exist for the converted body to compile. Next
    /// reports false so the loop it drives would iterate zero times; RunParallel never invokes
    /// the body at all, since benchmarks are not executed by the bootstrap host.
    /// </summary>
    public struct PB
    {
    }

    /// <summary>
    /// Fuzz-target receiver surface — COMPILE-ONLY, for the same reason as B above. Fuzz
    /// declarations are disclosed-unsupported in the manifest (execution is deferred to Phase 4D)
    /// and never registered with the host, but their converted BODIES still compile into the test
    /// assembly, so the members they reference must exist. There is no fuzzing engine, so Fuzz
    /// never invokes the target and Add never records a seed: with no run to perform there is
    /// nothing to time, seed or fail, and every member is a safe non-throwing no-op. The member
    /// set is Go 1.23's full public surface for *testing.F — the TB members it inherits from the
    /// embedded common, plus its own Add and Fuzz — so the compiled shape never drifts as more
    /// fuzz targets convert (the same rule TB above is declared under).
    /// </summary>
    public struct F
    {
    }

    public static void Error(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.Fail();
    }

    public static void Errorf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.Fail();
    }

    [GoRecv] public static void Fail(this ref T t) => t.RequiredExecution.Fail();

    [GoRecv] public static void FailNow(this ref T t) => t.RequiredExecution.FailNow();

    [GoRecv] public static bool Failed(this ref T t) => t.RequiredExecution.Failed;

    public static void Fatal(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.FailNow();
    }

    public static void Fatalf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.FailNow();
    }

    public static void Log(this ref T t, params ꓸꓸꓸany args) =>
        t.RequiredExecution.Log(Sprint(args));

    public static void Logf(this ref T t, @string format, params ꓸꓸꓸany args) =>
        t.RequiredExecution.Log(Sprintf(format, args));

    [GoRecv] public static void Helper(this ref T t) => t.RequiredExecution.Helper();

    [GoRecv] public static @string Name(this ref T t) => t.RequiredExecution.Name;

    [GoRecv] public static void Cleanup(this ref T t, Action cleanup) =>
        t.RequiredExecution.Cleanup(cleanup);

    [GoRecv] public static bool Run(this ref T t, @string name, Action<ж<T>> test) =>
        t.RequiredExecution.Run(name.ToString(), test);

    public static void Skip(this ref T t, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprint(args));
        execution.SkipNow();
    }

    public static void Skipf(this ref T t, @string format, params ꓸꓸꓸany args)
    {
        TestExecution execution = t.RequiredExecution;
        execution.Log(Sprintf(format, args));
        execution.SkipNow();
    }

    [GoRecv] public static void SkipNow(this ref T t) => t.RequiredExecution.SkipNow();

    [GoRecv] public static bool Skipped(this ref T t) => t.RequiredExecution.Skipped;

    [GoRecv] public static @string TempDir(this ref T t) => t.RequiredExecution.TempDir();

    [GoRecv] public static void Setenv(this ref T t, @string key, @string value) =>
        t.RequiredExecution.Setenv(key.ToString(), value.ToString());

    [GoRecv] public static void Parallel(this ref T t) => t.RequiredExecution.Parallel();

    // RecvGenerator intentionally handles ordinary receiver signatures. C# params
    // collections use Span<T>, which is ref-like and needs explicit pointer receiver
    // overloads for converted closures that retain *testing.T rather than a ref local.
    public static void Error(this ж<T> t, params ꓸꓸꓸany args) => Error(ref t.Value, args);

    public static void Errorf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Errorf(ref t.Value, format, args);

    public static void Fatal(this ж<T> t, params ꓸꓸꓸany args) => Fatal(ref t.Value, args);

    public static void Fatalf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Fatalf(ref t.Value, format, args);

    public static void Log(this ж<T> t, params ꓸꓸꓸany args) => Log(ref t.Value, args);

    public static void Logf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Logf(ref t.Value, format, args);

    public static void Skip(this ж<T> t, params ꓸꓸꓸany args) => Skip(ref t.Value, args);

    public static void Skipf(this ж<T> t, @string format, params ꓸꓸꓸany args) => Skipf(ref t.Value, format, args);

    [GoRecv] public static nint Run(this ref M m)
    {
        TestRunner runner = m.Runner ?? throw new InvalidOperationException("testing.M is not attached to a test registry");
        return runner.RunAll();
    }

    // Compile-only B surface (see struct B above) — never executed, never throwing. The
    // RecvGenerator supplies the ж<B> overloads, as for every ordinary [GoRecv] signature.
    [GoRecv] public static bool Run(this ref B b, @string name, Action<ж<B>> benchmark) => true;

    [GoRecv] public static void ReportAllocs(this ref B b) { }

    [GoRecv] public static void ResetTimer(this ref B b) { }

    [GoRecv] public static void SetBytes(this ref B b, long n) { }

    [GoRecv] public static void StartTimer(this ref B b) { }

    [GoRecv] public static void StopTimer(this ref B b) { }

    // Parallel benchmarks (see struct PB above): RunParallel does not invoke the body — nothing
    // runs, so nothing is scheduled across goroutines — and PB.Next reports "no more work" so the
    // body's for pb.Next() loop would terminate immediately if it ever were invoked.
    [GoRecv] public static void RunParallel(this ref B b, Action<ж<PB>> body) { }

    [GoRecv] public static bool Next(this ref PB pb) => false;

    // Params-taking B members need the same explicit ж<B> overloads as T's above (params
    // collections are ref-like Spans the RecvGenerator does not synthesize overloads for).
    // Failure reporting is a no-op: benchmark bodies never execute, so there is no run to fail.
    public static void Errorf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Fatal(this ref B b, params ꓸꓸꓸany args) { }

    public static void Fatalf(this ref B b, @string format, params ꓸꓸꓸany args) { }

    public static void Skip(this ref B b, params ꓸꓸꓸany args) { }

    public static void Errorf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Errorf(ref b.Value, format, args);

    public static void Fatal(this ж<B> b, params ꓸꓸꓸany args) => Fatal(ref b.Value, args);

    public static void Fatalf(this ж<B> b, @string format, params ꓸꓸꓸany args) => Fatalf(ref b.Value, format, args);

    public static void Skip(this ж<B> b, params ꓸꓸꓸany args) => Skip(ref b.Value, args);

    // Compile-only F surface (see struct F above) — never executed, never throwing. Fuzz takes a
    // System.Delegate because a Go fuzz target's signature is arbitrary (*testing.T followed by
    // the fuzzed argument types, e.g. func(*testing.T, uint, uint, …)); the converted body is an
    // explicitly-typed lambda, so C# infers its natural Action<…> and converts it to Delegate.
    // The target is never invoked and the seed corpus Add collects is discarded — there is no
    // fuzzing engine to consume either.
    [GoRecv] public static void Fuzz(this ref F f, Delegate target) { }

    [GoRecv] public static void Fail(this ref F f) { }

    [GoRecv] public static void FailNow(this ref F f) { }

    [GoRecv] public static bool Failed(this ref F f) => false;

    [GoRecv] public static void Helper(this ref F f) { }

    [GoRecv] public static @string Name(this ref F f) => ""u8;

    [GoRecv] public static void SkipNow(this ref F f) { }

    [GoRecv] public static bool Skipped(this ref F f) => false;

    [GoRecv] public static void Cleanup(this ref F f, Action cleanup) { }

    [GoRecv] public static void Setenv(this ref F f, @string key, @string value) { }

    [GoRecv] public static @string TempDir(this ref F f) => ""u8;

    // Params-taking F members need the same explicit ж<F> overloads as T's and B's above (params
    // collections are ref-like Spans the RecvGenerator does not synthesize overloads for).
    public static void Add(this ref F f, params ꓸꓸꓸany args) { }

    public static void Error(this ref F f, params ꓸꓸꓸany args) { }

    public static void Errorf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Fatal(this ref F f, params ꓸꓸꓸany args) { }

    public static void Fatalf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Log(this ref F f, params ꓸꓸꓸany args) { }

    public static void Logf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Skip(this ref F f, params ꓸꓸꓸany args) { }

    public static void Skipf(this ref F f, @string format, params ꓸꓸꓸany args) { }

    public static void Add(this ж<F> f, params ꓸꓸꓸany args) => Add(ref f.Value, args);

    public static void Error(this ж<F> f, params ꓸꓸꓸany args) => Error(ref f.Value, args);

    public static void Errorf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Errorf(ref f.Value, format, args);

    public static void Fatal(this ж<F> f, params ꓸꓸꓸany args) => Fatal(ref f.Value, args);

    public static void Fatalf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Fatalf(ref f.Value, format, args);

    public static void Log(this ж<F> f, params ꓸꓸꓸany args) => Log(ref f.Value, args);

    public static void Logf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Logf(ref f.Value, format, args);

    public static void Skip(this ж<F> f, params ꓸꓸꓸany args) => Skip(ref f.Value, args);

    public static void Skipf(this ж<F> f, @string format, params ꓸꓸꓸany args) => Skipf(ref f.Value, format, args);

    /// <summary>
    /// Reports the average allocation cost per run of f — like Go's testing.AllocsPerRun, with
    /// one deliberate semantic difference: Go counts MALLOCS (runtime.MemStats.Mallocs delta);
    /// the CLR exposes no malloc counter, so the shim measures allocated BYTES on the calling
    /// thread. Zero maps exactly — 0 bytes ⟺ 0 allocations — so the dominant assert-zero stdlib
    /// tests are faithful; a nonzero result is the average allocated bytes per run (floored at 1
    /// so amortized sub-byte-per-run allocation can never masquerade as the exact-zero case) — a
    /// byte-derived approximation, NOT a malloc count, so a test asserting a specific nonzero
    /// count diverges as a loud failure in the differential oracle instead of silently passing.
    /// </summary>
    /// <remarks>
    /// GC.GetAllocatedBytesForCurrentThread is precise and inherently scoped to this thread,
    /// which stands in for Go's GOMAXPROCS(1) pinning: other threads' allocations never pollute
    /// the measurement. Like Go's, f is assumed single-threaded — allocations made by goroutines
    /// f spawns run on other threads (converted goroutines share the thread pool) and are not
    /// observed. See docs/ConversionStrategies-Reference.md "Manually-Converted Declarations".
    /// </remarks>
    public static double AllocsPerRun(nint runs, Action f)
    {
        // Warmup run outside the measurement window (Go does the same): first-call lazy
        // initialization — and JIT compilation here — must not count against f.
        f();

        long start = GC.GetAllocatedBytesForCurrentThread();

        for (nint i = 0; i < runs; i++)
            f();

        long allocated = GC.GetAllocatedBytesForCurrentThread() - start;

        // Integer division like Go's (its comment: "do the division as integers"); runs == 0
        // divides by zero — a runtime-error panic exactly where Go's own division panics.
        double average = Math.Max(1L, allocated / runs);
        return allocated == 0L ? 0.0D : average;
    }

    /// <summary>
    /// Reports what the test coverage mode is set to — like Go's testing.CoverMode(). Coverage
    /// instrumentation does not exist in the shim, so the mode is always "" — exactly Go's value
    /// when the binary is built without -cover, sending callers down the coverage-off path.
    /// </summary>
    public static @string CoverMode() => "";

    /// <summary>
    /// Reports whether the -short flag was set — like Go's testing.Short() (default false).
    /// </summary>
    public static bool Short() => TestHost.ShortMode;

    /// <summary>
    /// Reports whether the program is a test binary — like Go's testing.Testing(). The shim is
    /// referenced ONLY by converted test projects (the go2cs test host is the program), so every
    /// reachable caller is a test binary.
    /// </summary>
    public static bool Testing() => true;

    /// <summary>
    /// Reports whether the -v flag was set — like Go's testing.Verbose() (default false).
    /// </summary>
    public static bool Verbose() => TestHost.VerboseMode;

    // Formatting is intentionally NOT the fmt package's: the shim stays fmt-free so converted test
    // projects can resolve fmt (like every other stdlib dependency) from the overlaid
    // go-src-converted tree without this baseline-built runtime dragging in a second tree's
    // "namespace go" classes. See TestFormat's remarks for the full mixed-tree rationale.
    private static string Sprint(ReadOnlySpan<any> args) => TestFormat.Sprint(args);

    private static string Sprintf(@string format, ReadOnlySpan<any> args) => TestFormat.Sprintf(format, args);
}
