using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using go.golib;

namespace go.testing_runtime;

public sealed record RegisteredTest(
    string Name,
    Action<ж<testing_package.T>> Action,
    string Source,
    int Line);

public sealed class TestRegistry(string package, IReadOnlyList<string> fixtures)
{
    private readonly List<RegisteredTest> m_tests = [];

    public string Package { get; } = package;

    public IReadOnlyList<string> Fixtures { get; } = fixtures;

    public IReadOnlyList<RegisteredTest> Tests => m_tests;

    public Action<ж<testing_package.M>>? TestMain { get; private set; }

    public void Add(string name, Action<ж<testing_package.T>> action, string source, int line) =>
        m_tests.Add(new RegisteredTest(name, action, source, line));

    public void SetTestMain(Action<ж<testing_package.M>> testMain) => TestMain = testMain;
}

public sealed record TestEvent(
    string Package,
    string Test,
    string Action,
    double Elapsed = 0.0D,
    string? Output = null,
    string? Source = null,
    int? Line = null);

internal sealed class TestReporter(string package, bool json, bool verbose)
{
    private readonly object m_syncRoot = new();
    private readonly List<TestEvent> m_events = [];

    public IReadOnlyList<TestEvent> Events
    {
        get
        {
            lock (m_syncRoot)
                return m_events.ToArray();
        }
    }

    public void Report(TestEvent testEvent)
    {
        lock (m_syncRoot)
        {
            m_events.Add(testEvent);

            if (json)
            {
                Console.WriteLine(JsonSerializer.Serialize(testEvent, JsonOptions));
                return;
            }

            if (testEvent.Action == "run" && !verbose)
                return;

            string output = string.IsNullOrWhiteSpace(testEvent.Output) ? "" : $" — {testEvent.Output}";
            Console.WriteLine($"{testEvent.Action.ToUpperInvariant(),-20} {testEvent.Test}{output}");
        }
    }

    public void ReportPackage(string action, double elapsed = 0.0D, string? output = null) =>
        Report(new TestEvent(package, "", action, elapsed, output));

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

internal sealed class TestOptions
{
    public bool Json { get; private set; }
    public bool Verbose { get; private set; }
    public bool Short { get; private set; }
    public int Count { get; private set; } = 1;
    public int Parallel { get; private set; } = Environment.ProcessorCount;
    public int? ShuffleSeed { get; private set; }
    public TimeSpan Timeout { get; private set; } = TimeSpan.FromMinutes(10.0D);
    public string? ResultFile { get; private set; }
    public string? JUnitFile { get; private set; }
    private Regex[]? Filters { get; set; }

    public static TestOptions Parse(string[] args)
    {
        TestOptions options = new();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string key = arg;
            string? value = null;
            int equals = arg.IndexOf('=');

            if (equals >= 0)
            {
                key = arg[..equals];
                value = arg[(equals + 1)..];
            }

            switch (key)
            {
                case "--json":
                    options.Json = true;
                    break;
                case "-v":
                case "-test.v":
                    options.Verbose = value is null || bool.Parse(value);
                    break;
                case "-short":
                case "-test.short":
                    // Backs testing.Short() in the shim; go test's default (flag absent) is false.
                    options.Short = value is null || bool.Parse(value);
                    break;
                case "-run":
                case "-test.run":
                    value ??= NextValue(args, ref i, key);
                    options.Filters = value.Split('/').Select(part =>
                        new Regex(part, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1.0D))).ToArray();
                    break;
                case "-count":
                case "-test.count":
                    value ??= NextValue(args, ref i, key);
                    options.Count = Math.Max(1, int.Parse(value, CultureInfo.InvariantCulture));
                    break;
                case "-parallel":
                case "-test.parallel":
                    // Caps simultaneously RUNNING parallel tests, like go test's -parallel flag
                    // (whose default is GOMAXPROCS — the processor count matches that default).
                    value ??= NextValue(args, ref i, key);
                    options.Parallel = Math.Max(1, int.Parse(value, CultureInfo.InvariantCulture));
                    break;
                case "-shuffle":
                case "-test.shuffle":
                    value ??= NextValue(args, ref i, key);
                    options.ShuffleSeed = value == "on"
                        ? Random.Shared.Next()
                        : value == "off" ? null : int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "-timeout":
                case "-test.timeout":
                    value ??= NextValue(args, ref i, key);
                    options.Timeout = ParseDuration(value);
                    break;
                case "--result":
                    value ??= NextValue(args, ref i, key);
                    options.ResultFile = value;
                    break;
                case "--junit":
                    value ??= NextValue(args, ref i, key);
                    options.JUnitFile = value;
                    break;
                default:
                    throw new ArgumentException($"unsupported converted test option: {arg}");
            }
        }

        return options;
    }

    public void ResolveOutputPaths(string baseDirectory)
    {
        if (!string.IsNullOrWhiteSpace(ResultFile) && !Path.IsPathRooted(ResultFile))
            ResultFile = Path.GetFullPath(Path.Combine(baseDirectory, ResultFile));
        if (!string.IsNullOrWhiteSpace(JUnitFile) && !Path.IsPathRooted(JUnitFile))
            JUnitFile = Path.GetFullPath(Path.Combine(baseDirectory, JUnitFile));
    }

    public bool ShouldRun(string fullName)
    {
        if (Filters is null)
            return true;

        string[] nameParts = fullName.Split('/');
        int count = Math.Min(nameParts.Length, Filters.Length);
        for (int i = 0; i < count; i++)
        {
            if (!Filters[i].IsMatch(nameParts[i]))
                return false;
        }
        return true;
    }

    private static string NextValue(string[] args, ref int index, string option)
    {
        if (++index >= args.Length)
            throw new ArgumentException($"missing value for {option}");
        return args[index];
    }

    private static TimeSpan ParseDuration(string value)
    {
        Match match = Regex.Match(value, @"^(?<number>\d+(?:\.\d+)?)(?<unit>ms|s|m|h)$", RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new ArgumentException($"invalid Go-style duration: {value}");

        double number = double.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
        return match.Groups["unit"].Value switch
        {
            "ms" => TimeSpan.FromMilliseconds(number),
            "s" => TimeSpan.FromSeconds(number),
            "m" => TimeSpan.FromMinutes(number),
            "h" => TimeSpan.FromHours(number),
            _ => throw new ArgumentException($"invalid duration unit: {value}")
        };
    }
}

public static class TestHost
{
    /// <summary>
    /// Gets whether the current run was started with -short — the value testing.Short() reports.
    /// One host run executes per test process, so process-wide state matches Go's model.
    /// </summary>
    public static bool ShortMode { get; private set; }

    /// <summary>
    /// Gets whether the current run was started with -v — the value testing.Verbose() reports.
    /// </summary>
    public static bool VerboseMode { get; private set; }

    public static int Run(TestRegistry registry, string[] args)
    {
        TestOptions options;

        try
        {
            options = TestOptions.Parse(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        ShortMode = options.Short;
        VerboseMode = options.Verbose;

        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUICulture = CultureInfo.CurrentUICulture;
        string previousDirectory = Environment.CurrentDirectory;
        string? previousTimezone = Environment.GetEnvironmentVariable("TZ");
        string workingDirectory = Path.Combine(Path.GetTempPath(), "go2cs-tests", SanitizePath(registry.Package), Guid.NewGuid().ToString("N"));
        options.ResolveOutputPaths(previousDirectory);

        try
        {
            Directory.CreateDirectory(workingDirectory);
            CopyFixtures(registry.Fixtures, workingDirectory);
            Environment.CurrentDirectory = workingDirectory;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            Environment.SetEnvironmentVariable("TZ", "UTC");

            TestReporter reporter = new(registry.Package, options.Json, options.Verbose);
            TestRunner runner = new(registry, options, reporter, workingDirectory);
            Task<nint> run = Task.Run(() => RunTests(registry, runner));

            if (!run.Wait(options.Timeout))
            {
                reporter.ReportPackage("timeout", options.Timeout.TotalSeconds, $"package timeout after {options.Timeout}");
                WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
                WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
                return 1;
            }

            int exitCode = checked((int)run.Result);
            WriteResults(options.ResultFile, registry.Package, options, reporter.Events);
            WriteJUnit(options.JUnitFile, registry.Package, reporter.Events);
            return exitCode;
        }
        catch (Exception ex)
        {
            TestEvent infrastructureError = new(registry.Package, "", "infrastructure-error", Output: ex.ToString());
            if (options.Json)
                Console.WriteLine(JsonSerializer.Serialize(infrastructureError, TestReporter.JsonOptions));
            else
                Console.Error.WriteLine(ex);
            return 2;
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
            Environment.SetEnvironmentVariable("TZ", previousTimezone);

            try
            {
                Directory.Delete(workingDirectory, true);
            }
            catch
            {
                // Per-test cleanup failures are reported; final process cleanup is best effort.
            }
        }
    }

    private static nint RunTests(TestRegistry registry, TestRunner runner)
    {
        if (registry.TestMain is null)
            return runner.RunAll();

        testing_package.M m = new() { Runner = runner };
        registry.TestMain(new ж<testing_package.M>(m));
        return runner.HasRun ? runner.ExitCode : 0;
    }

    private static void CopyFixtures(IReadOnlyList<string> fixtures, string workingDirectory)
    {
        foreach (string relativePath in fixtures)
        {
            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            string source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, normalized));
            string target = Path.GetFullPath(Path.Combine(workingDirectory, normalized));

            if (!source.StartsWith(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase) ||
                !target.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"fixture escapes package root: {relativePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, true);
        }
    }

    private static void WriteResults(string? path, string package, TestOptions options, IReadOnlyList<TestEvent> events)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            object result = new
            {
                schemaVersion = 1,
                package,
                environment = new
                {
                    dotnetRuntime = RuntimeInformation.FrameworkDescription,
                    culture = CultureInfo.CurrentCulture.Name,
                    timezone = Environment.GetEnvironmentVariable("TZ"),
                    shuffleSeed = options.ShuffleSeed
                },
                events
            };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path, JsonSerializer.Serialize(result, TestReporter.JsonOptions));
        }
    }

    private static void WriteJUnit(string? path, string package, IReadOnlyList<TestEvent> events)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        TestEvent[] terminal = events
            .Where(testEvent => testEvent.Test.Length > 0 && testEvent.Action is "pass" or "fail" or "skip" or "timeout" or "infrastructure-error")
            .ToArray();
        int failures = terminal.Count(testEvent => testEvent.Action is "fail" or "timeout");
        int errors = terminal.Count(testEvent => testEvent.Action == "infrastructure-error");
        int skipped = terminal.Count(testEvent => testEvent.Action == "skip");

        XElement suite = new("testsuite",
            new XAttribute("name", package),
            new XAttribute("tests", terminal.Length),
            new XAttribute("failures", failures),
            new XAttribute("errors", errors),
            new XAttribute("skipped", skipped),
            new XAttribute("time", terminal.Sum(testEvent => testEvent.Elapsed).ToString("0.######", CultureInfo.InvariantCulture)));

        foreach (TestEvent testEvent in terminal)
        {
            XElement testCase = new("testcase",
                new XAttribute("classname", package),
                new XAttribute("name", testEvent.Test),
                new XAttribute("time", testEvent.Elapsed.ToString("0.######", CultureInfo.InvariantCulture)));

            if (testEvent.Action == "skip")
                testCase.Add(new XElement("skipped", new XAttribute("message", testEvent.Output ?? "skipped")));
            else if (testEvent.Action is "fail" or "timeout")
                testCase.Add(new XElement("failure", new XAttribute("message", testEvent.Action), testEvent.Output ?? ""));
            else if (testEvent.Action == "infrastructure-error")
                testCase.Add(new XElement("error", new XAttribute("message", "infrastructure-error"), testEvent.Output ?? ""));

            suite.Add(testCase);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        new XDocument(new XElement("testsuites", suite)).Save(path);
    }

    private static string SanitizePath(string value) =>
        string.Concat(value.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) || ch is '/' or '\\' ? '_' : ch));
}

public sealed class TestRunner
{
    private readonly TestRegistry m_registry;
    private readonly TestOptions m_options;
    private readonly TestReporter m_reporter;
    private readonly SemaphoreSlim m_parallelLimiter;
    private int m_failures;
    private int m_infrastructureFailures;

    internal TestRunner(TestRegistry registry, TestOptions options, TestReporter reporter, string workingDirectory)
    {
        m_registry = registry;
        m_options = options;
        m_reporter = reporter;
        m_parallelLimiter = new SemaphoreSlim(options.Parallel);
        WorkingDirectory = workingDirectory;
    }

    public bool HasRun { get; private set; }

    public nint ExitCode => m_failures == 0 && m_infrastructureFailures == 0 ? 0 : 1;

    internal string Package => m_registry.Package;

    internal string WorkingDirectory { get; }

    public nint RunAll()
    {
        HasRun = true;
        Stopwatch packageTimer = Stopwatch.StartNew();
        m_reporter.ReportPackage("run", output: m_options.ShuffleSeed is int reportedSeed ? $"shuffle seed: {reportedSeed}" : null);

        for (int count = 0; count < m_options.Count; count++)
        {
            List<RegisteredTest> tests = m_registry.Tests
                .Where(test => m_options.ShouldRun(test.Name))
                .OrderBy(test => test.Name, StringComparer.Ordinal)
                .ToList();

            if (m_options.ShuffleSeed is int seed)
                Shuffle(tests, unchecked(seed + count));

            // Go releases top-level parallel tests at the end of EACH -count iteration, so
            // iterations interleave serial and parallel phases rather than batching every
            // iteration's parallel tests to the very end of the run.
            List<TestExecution> parallel = [];

            foreach (RegisteredTest test in tests)
            {
                TestExecution execution = Start(test.Name, test.Action, null, test.Source, test.Line);
                WaitForSerialBoundary(execution, parallel);
            }

            foreach (TestExecution execution in parallel)
                execution.ReleaseParallel();
            foreach (TestExecution execution in parallel)
                execution.Wait();
        }

        packageTimer.Stop();
        m_reporter.ReportPackage(ExitCode == 0 ? "pass" : "fail", packageTimer.Elapsed.TotalSeconds);
        return ExitCode;
    }

    internal bool RunChild(TestExecution parent, string requestedName, Action<ж<testing_package.T>> action)
    {
        string name = parent.NextSubtestName(requestedName);
        if (!m_options.ShouldRun(name))
            return true;
        TestExecution child = Start(name, action, parent, parent.Source, parent.Line);
        WaitForSerialBoundary(child, parent.ParallelChildren);
        return !child.Failed;
    }

    private TestExecution Start(string name, Action<ж<testing_package.T>> action, TestExecution? parent, string source, int line)
    {
        TestExecution execution = new(this, name, parent, source, line);
        execution.Start(action);
        return execution;
    }

    private static void WaitForSerialBoundary(TestExecution execution, List<TestExecution> parallel)
    {
        Task completed = Task.WhenAny(execution.Completion, execution.ParallelReached).GetAwaiter().GetResult();
        if (completed == execution.ParallelReached && !execution.Completion.IsCompleted)
            parallel.Add(execution);
        else
            execution.Wait();
    }

    internal void Completed(TestExecution execution)
    {
        if (execution.InfrastructureFailed)
            Interlocked.Increment(ref m_infrastructureFailures);
        else if (execution.Failed)
            Interlocked.Increment(ref m_failures);
    }

    /// <summary>
    /// Records a host-level infrastructure failure that cannot be attached to a live execution —
    /// e.g. testing.T misuse observed after its test already completed, or an unexpected exception
    /// escaping an execution thread. Counted toward the exit code and disclosed as an event so the
    /// failure can never silently pass.
    /// </summary>
    internal void RecordInfrastructureFailure(string name, string output)
    {
        Interlocked.Increment(ref m_infrastructureFailures);
        m_reporter.Report(new TestEvent(Package, name, "infrastructure-error", Output: output));
    }

    // A parallel test holds one slot while it RUNS (acquired after its serial-phase gate opens,
    // released before it waits on its own parallel children — Go's tRunner does the same, so a
    // parallel parent never starves its children under a small -parallel cap).
    internal void AcquireParallelSlot() => m_parallelLimiter.Wait();

    internal void ReleaseParallelSlot() => m_parallelLimiter.Release();

    internal void Report(TestEvent testEvent) => m_reporter.Report(testEvent);

    private static void Shuffle<T>(IList<T> values, int seed)
    {
        Random random = new(seed);
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}

public sealed class TestExecution
{
    private readonly TestRunner m_runner;
    private readonly TestExecution? m_parent;
    private readonly object m_syncRoot = new();
    private readonly Stack<Action> m_cleanups = new();
    private readonly List<string> m_logs = [];
    private readonly Dictionary<string, int> m_subtestNames = new(StringComparer.Ordinal);
    private readonly ManualResetEventSlim m_parallelGate = new(false);
    private int m_ownerThread;
    private int m_tempDirSequence;
    private bool m_parallel;
    private bool m_holdsParallelSlot;
    private bool m_finished;
    private bool m_failed;
    private bool m_skipped;

    internal TestExecution(TestRunner runner, string name, TestExecution? parent, string source, int line)
    {
        m_runner = runner;
        m_parent = parent;
        Name = name;
        Source = source;
        Line = line;
    }

    public string Name { get; }

    public string Source { get; }

    public int Line { get; }

    public bool Failed { get { lock (m_syncRoot) return m_failed; } }

    public bool Skipped { get { lock (m_syncRoot) return m_skipped; } }

    public bool InfrastructureFailed { get; private set; }

    internal Task Completion { get; private set; } = Task.CompletedTask;

    private TaskCompletionSource ParallelSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task ParallelReached => ParallelSource.Task;

    internal List<TestExecution> ParallelChildren { get; } = [];

    internal void Start(Action<ж<testing_package.T>> action)
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Completion = completion.Task;

        // Each execution gets a DEDICATED thread rather than Task.Run: a parallel test parks its
        // thread on m_parallelGate (and then on the -parallel limiter) until the serial phase
        // completes, and dozens-to-hundreds of parked thread-pool threads would starve the pool
        // (injection is ~1 thread/s) — stalling both the suite and any converted goroutines the
        // tests spawn, which golib queues on that same pool. Dedicated threads keep test parking
        // and goroutine scheduling independent at stdlib-suite scale.
        Thread thread = new(() =>
        {
            try
            {
                Execute(action);
            }
            catch (Exception ex)
            {
                // Execute contains its own handling; anything escaping it is a host defect. Contain
                // it here — an unhandled exception on a background thread would hit golib's
                // AppDomain backstop, which prints the report to stderr and exits 2 (like Go):
                // the whole run dies mid-flight with NO result files and every unrelated test
                // killed, instead of one attributed infrastructure failure.
                m_runner.RecordInfrastructureFailure(Name, $"test host failure: {ex}");
            }
            finally
            {
                completion.TrySetResult();
            }
        })
        {
            IsBackground = true,
            Name = $"go2cs test: {Name}"
        };

        thread.Start();
    }

    internal void Wait() => Completion.GetAwaiter().GetResult();

    internal void ReleaseParallel() => m_parallelGate.Set();

    public void Fail()
    {
        lock (m_syncRoot)
        {
            if (m_finished)
                throw new InvalidOperationException($"Fail called after {Name} completed");
            m_failed = true;
        }
        m_parent?.FailFromChild();
    }

    public void FailNow()
    {
        if (!TryEnsureOwner(nameof(FailNow)))
            return;
        Fail();
        throw new TestAbortException();
    }

    public void SkipNow()
    {
        if (!TryEnsureOwner(nameof(SkipNow)))
            return;
        lock (m_syncRoot)
            m_skipped = true;
        throw new TestAbortException();
    }

    public void Log(string text)
    {
        lock (m_syncRoot)
        {
            if (m_finished)
                throw new InvalidOperationException($"Log called after {Name} completed");
            m_logs.Add(text.TrimEnd('\r', '\n'));
        }
    }

    public void Helper()
    {
        // Helper-frame elision is staged; declaration source identity is still reported.
    }

    public void Cleanup(Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(cleanup);
        lock (m_syncRoot)
        {
            // A cleanup registered after the test completed can never run (the cleanup phase is
            // already over) — reject it instead of silently dropping it. Go panics here too.
            if (m_finished)
                throw new InvalidOperationException($"Cleanup called after {Name} completed");
            m_cleanups.Push(cleanup);
        }
    }

    public bool Run(string name, Action<ж<testing_package.T>> action)
    {
        if (!TryEnsureOwner(nameof(Run)))
            return false;
        return m_runner.RunChild(this, name, action);
    }

    public void Parallel()
    {
        if (!TryEnsureOwner(nameof(Parallel)))
            return;
        lock (m_syncRoot)
        {
            if (m_parallel)
                throw new InvalidOperationException($"testing: {Name} called Parallel more than once");
            m_parallel = true;
        }
        ParallelSource.TrySetResult();
        m_parallelGate.Wait();

        // Released from the serial-phase gate; now compete for a -parallel slot so at most
        // Options.Parallel parallel tests RUN simultaneously (Go's -parallel semantics).
        m_runner.AcquireParallelSlot();
        m_holdsParallelSlot = true;
    }

    public @string TempDir()
    {
        string path = Path.Combine(m_runner.WorkingDirectory, ".tmp", SanitizeName(Name), Interlocked.Increment(ref m_tempDirSequence).ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(path);
        Cleanup(() => Directory.Delete(path, true));
        return path;
    }

    public void Setenv(string key, string value)
    {
        if (!TryEnsureOwner(nameof(Setenv)))
            return;
        if (m_parallel)
            throw new InvalidOperationException("testing: t.Setenv called after t.Parallel");

        string? previous = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
        Cleanup(() => Environment.SetEnvironmentVariable(key, previous));
    }

    internal string NextSubtestName(string requested)
    {
        string baseName = SanitizeName(requested);
        lock (m_syncRoot)
        {
            int sequence = m_subtestNames.TryGetValue(baseName, out int current) ? current + 1 : 0;
            m_subtestNames[baseName] = sequence;
            string unique = sequence == 0 ? baseName : $"{baseName}#{sequence:00}";
            return $"{Name}/{unique}";
        }
    }

    private void Execute(Action<ж<testing_package.T>> action)
    {
        Stopwatch timer = Stopwatch.StartNew();
        m_ownerThread = Environment.CurrentManagedThreadId;
        m_runner.Report(new TestEvent(m_runner.Package, Name, "run", Source: Source, Line: Line));

        testing_package.T t = new() { Execution = this };
        try
        {
            action(new ж<testing_package.T>(t));
        }
        catch (TestAbortException)
        {
        }
        catch (PanicException ex)
        {
            Log($"panic: {ex.Message}\n{ex.StackTrace}");
            Fail();
        }
        catch (Exception ex) when (RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic))
        {
            Log($"panic: {panic!.Message}\n{panic.StackTrace}");
            Fail();
        }
        catch (Exception ex)
        {
            InfrastructureFailed = true;
            Log(ex.ToString());
            FailFromInfrastructure();
        }
        finally
        {
            // Give the -parallel slot back BEFORE waiting on parallel children (Go's ordering):
            // a parallel parent that held its slot while waiting would deadlock its own children
            // under a small -parallel cap.
            if (m_holdsParallelSlot)
            {
                m_holdsParallelSlot = false;
                m_runner.ReleaseParallelSlot();
            }

            foreach (TestExecution child in ParallelChildren)
                child.ReleaseParallel();
            foreach (TestExecution child in ParallelChildren)
                child.Wait();

            RunCleanups();
            timer.Stop();

            lock (m_syncRoot)
                m_finished = true;

            string terminal = InfrastructureFailed ? "infrastructure-error" : Failed ? "fail" : Skipped ? "skip" : "pass";
            string? output;
            lock (m_syncRoot)
                output = m_logs.Count == 0 ? null : string.Join(Environment.NewLine, m_logs);

            m_runner.Report(new TestEvent(m_runner.Package, Name, terminal, timer.Elapsed.TotalSeconds, output, Source, Line));
            m_runner.Completed(this);
        }
    }

    private void RunCleanups()
    {
        while (true)
        {
            Action cleanup;
            lock (m_syncRoot)
            {
                if (m_cleanups.Count == 0)
                    return;
                cleanup = m_cleanups.Pop();
            }

            try
            {
                cleanup();
            }
            catch (TestAbortException)
            {
            }
            catch (Exception ex) when (RuntimeErrorPanic.TryAsPanic(ex, out PanicException? panic))
            {
                lock (m_syncRoot)
                    m_logs.Add($"cleanup panic: {panic!.Message}\n{panic.StackTrace}");
                FailFromInfrastructure();
            }
            catch (Exception ex)
            {
                InfrastructureFailed = true;
                lock (m_syncRoot)
                    m_logs.Add($"cleanup failed: {ex}");
                FailFromInfrastructure();
            }
        }
    }

    private void FailFromChild()
    {
        lock (m_syncRoot)
            m_failed = true;
        m_parent?.FailFromChild();
    }

    private void FailFromInfrastructure()
    {
        lock (m_syncRoot)
            m_failed = true;
        m_parent?.FailFromChild();
    }

    /// <summary>
    /// Verifies the caller is the test's own goroutine for operations Go restricts to it
    /// (FailNow/SkipNow/Run/Parallel/Setenv). A violation is recorded as an infrastructure
    /// failure on THIS execution and the operation becomes a no-op — it must never throw.
    /// </summary>
    /// <remarks>
    /// Throwing here would surface inside foreign converted code: golib's GoFunc exception filter
    /// only captures panic-convertible exceptions (its comment: "Non-panic exceptions fail the
    /// filter and propagate unchanged"), converted goroutines are queued on the bare thread pool
    /// (builtin.goǃ), and golib's AppDomain.UnhandledException backstop prints the report to
    /// stderr and exits 2 (like Go) — terminating the whole run mid-flight with NO result files
    /// and every unrelated test killed. Routing to an infrastructure failure keeps the misuse
    /// disclosed (req §6.1) without throwing into code that cannot handle it. (Fail/Error/Log
    /// intentionally have no owner check — Go permits them from any goroutine.)
    /// </remarks>
    private bool TryEnsureOwner(string operation)
    {
        if (Environment.CurrentManagedThreadId == m_ownerThread)
            return true;

        string message = $"testing: {operation} called from a goroutine other than the test goroutine for {Name}";
        bool completed;

        lock (m_syncRoot)
        {
            completed = m_finished;

            if (!completed)
            {
                InfrastructureFailed = true;
                m_logs.Add(message);
            }
        }

        if (completed)
        {
            // The owning execution already reported its terminal event and was counted — record
            // the late misuse at the runner level so it still fails the run and emits an event.
            m_runner.RecordInfrastructureFailure(Name, message);
        }
        else
        {
            FailFromInfrastructure();
        }

        return false;
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "#00";
        return string.Concat(value.Select(ch => char.IsWhiteSpace(ch) ? '_' : char.IsControl(ch) ? '\uFFFD' : ch));
    }
}

internal sealed class TestAbortException : Exception;
