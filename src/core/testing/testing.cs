using System;
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
    /// Benchmark receiver surface — COMPILE-ONLY in the bootstrap shim. Benchmark declarations
    /// are disclosed-unsupported in the manifest (execution is deferred to Phase 4D) and never
    /// registered with the host, but their converted BODIES still compile into the test assembly,
    /// so the members they reference must exist. All members are safe non-throwing no-ops
    /// (req §6.2): N defaults to 0 so a b.N loop iterates zero times, and Run reports success
    /// without executing.
    /// </summary>
    public struct B
    {
        public nint N;
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
    // RecvGenerator supplies the ж<B> overload, as for every ordinary [GoRecv] signature.
    [GoRecv] public static bool Run(this ref B b, @string name, Action<ж<B>> benchmark) => true;

    /// <summary>
    /// Reports whether the -short flag was set — like Go's testing.Short() (default false).
    /// </summary>
    public static bool Short() => TestHost.ShortMode;

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
