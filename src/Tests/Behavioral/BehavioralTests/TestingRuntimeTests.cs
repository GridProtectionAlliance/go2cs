using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using go;
using go.testing_runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class TestingRuntimeTests
{
    [TestMethod]
    public void PassingCleanupAndParallelSubtestsComplete()
    {
        ConcurrentQueue<string> events = new();
        TestRegistry registry = new("runtime/pass", []);
        registry.Add("TestPass", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => events.Enqueue("cleanup"));
            test.Run("first", child =>
            {
                ref testing_package.T subtest = ref child.Value;
                subtest.Parallel();
                events.Enqueue("first");
            });
            test.Run("second", child =>
            {
                ref testing_package.T subtest = ref child.Value;
                subtest.Parallel();
                events.Enqueue("second");
            });
            events.Enqueue("parent");
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));
        CollectionAssert.Contains(events.ToArray(), "first");
        CollectionAssert.Contains(events.ToArray(), "second");
        Assert.AreEqual("cleanup", events.Last());
    }

    [TestMethod]
    public void ErrorFatalPanicAndSkipProduceExpectedExitCodes()
    {
        Assert.AreEqual(1, RunSingle("TestError", (ref testing_package.T test) => test.Error("nonfatal")));
        Assert.AreEqual(1, RunSingle("TestFatal", (ref testing_package.T test) => test.Fatal("fatal")));
        Assert.AreEqual(1, RunSingle("TestPanic", (ref testing_package.T _) => throw new PanicException("boom")));
        Assert.AreEqual(1, RunSingle("TestRuntimePanic", (ref testing_package.T unused) =>
        {
            int zero = 0;
            _ = 1 / zero;
        }));
        Assert.AreEqual(0, RunSingle("TestSkip", (ref testing_package.T test) => test.Skip("not applicable")));
    }

    [TestMethod]
    public void TestMainControlsRegistryExecution()
    {
        bool ran = false;
        TestRegistry registry = new("runtime/main", []);
        registry.Add("TestThroughMain", _ => ran = true, "runtime_test.go", 1);
        registry.SetTestMain(pointer =>
        {
            ref testing_package.M testMain = ref pointer.Value;
            testMain.Run();
        });

        Assert.AreEqual(0, TestHost.Run(registry, []));
        Assert.IsTrue(ran);
    }

    [TestMethod]
    public void FatalStillRunsCleanupInLifoOrder()
    {
        ConcurrentQueue<string> cleanupOrder = new();
        TestRegistry registry = new("runtime/cleanup", []);
        registry.Add("TestCleanup", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => cleanupOrder.Enqueue("first"));
            test.Cleanup(() => cleanupOrder.Enqueue("second"));
            test.Fatal("stop now");
        }, "runtime_test.go", 1);

        Assert.AreEqual(1, TestHost.Run(registry, []));
        CollectionAssert.AreEqual(new[] { "second", "first" }, cleanupOrder.ToArray());
    }

    [TestMethod]
    public void SetenvTempDirAndFixturesAreIsolated()
    {
        string key = $"GO2CS_TEST_{Guid.NewGuid():N}";
        string fixtureRelativePath = Path.Combine("testdata", "runtime-fixture.txt");
        string fixtureSourcePath = Path.Combine(AppContext.BaseDirectory, fixtureRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fixtureSourcePath)!);
        File.WriteAllText(fixtureSourcePath, "original");

        string? tempDirectory = null;
        bool observedEnvironment = false;
        bool observedFixture = false;

        try
        {
            TestRegistry registry = new("runtime/isolation", [fixtureRelativePath]);
            registry.Add("TestIsolation", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                test.Setenv(key, "value");
                observedEnvironment = Environment.GetEnvironmentVariable(key) == "value";
                tempDirectory = test.TempDir().ToString();
                observedFixture = File.ReadAllText(fixtureRelativePath) == "original";
                File.WriteAllText(fixtureRelativePath, "working-copy");
            }, "runtime_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, []));
            Assert.IsTrue(observedEnvironment);
            Assert.IsTrue(observedFixture);
            Assert.IsNull(Environment.GetEnvironmentVariable(key));
            Assert.IsNotNull(tempDirectory);
            Assert.IsFalse(Directory.Exists(tempDirectory));
            Assert.AreEqual("original", File.ReadAllText(fixtureSourcePath));
        }
        finally
        {
            File.Delete(fixtureSourcePath);
        }
    }

    [TestMethod]
    public void JsonAndJUnitResultFilesAreWritten()
    {
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-results-{Guid.NewGuid():N}.json");
        string junitPath = Path.ChangeExtension(resultPath, ".xml");

        try
        {
            TestRegistry registry = new("runtime/results", []);
            registry.Add("TestPass", _ => { }, "runtime_test.go", 1);

            Assert.AreEqual(0, TestHost.Run(registry, ["--result", resultPath, "--junit", junitPath]));
            StringAssert.Contains(File.ReadAllText(resultPath), "\"action\":\"pass\"");
            StringAssert.Contains(File.ReadAllText(junitPath), "<testsuite");
            StringAssert.Contains(File.ReadAllText(junitPath), "TestPass");
        }
        finally
        {
            File.Delete(resultPath);
            File.Delete(junitPath);
        }
    }

    [TestMethod]
    public void HierarchicalFilterSelectsNamedSubtest()
    {
        ConcurrentQueue<string> ran = new();
        TestRegistry registry = new("runtime/filter", []);
        registry.Add("TestParent", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Run("wanted", _ => ran.Enqueue("wanted"));
            test.Run("other", _ => ran.Enqueue("other"));
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-run", "TestParent/wanted"]));
        CollectionAssert.AreEqual(new[] { "wanted" }, ran.ToArray());
    }

    [TestMethod]
    public void CrossGoroutineFatalRecordsInfrastructureFailureWithoutKillingProcess()
    {
        // F8 guard: golib goroutines run on the bare thread pool with no non-panic exception
        // handler, and golib's AppDomain backstop responds to an unhandled exception by printing
        // the report to stderr and exiting 2 (like Go) — killing the whole run with no result
        // files — so an ownership violation must be ROUTED to an infrastructure failure on the
        // owning execution, never thrown into the foreign thread.
        string resultPath = Path.Combine(Path.GetTempPath(), $"go2cs-ownership-{Guid.NewGuid():N}.json");

        try
        {
            TestRegistry registry = new("runtime/ownership", []);
            registry.Add("TestCrossGoroutineFatal", pointer =>
            {
                Thread goroutine = new(() => pointer.Fatal("cross-goroutine fatal")) { IsBackground = true };
                goroutine.Start();
                goroutine.Join();
            }, "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--result", resultPath]));
            StringAssert.Contains(File.ReadAllText(resultPath), "\"action\":\"infrastructure-error\"");
        }
        finally
        {
            File.Delete(resultPath);
        }
    }

    [TestMethod]
    public void FailNowInsideCleanupRecordsFailure()
    {
        // F9 guard: a FailNow issued from within a cleanup must mark the test failed (the abort
        // exception itself is contained by the cleanup loop, and later cleanups still run).
        ConcurrentQueue<string> cleanupOrder = new();
        TestRegistry registry = new("runtime/cleanupfailnow", []);
        registry.Add("TestCleanupFailNow", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Cleanup(() => cleanupOrder.Enqueue("outer"));
            test.Cleanup(() => testing_package.FailNow(ref pointer.Value));
        }, "runtime_test.go", 1);

        Assert.AreEqual(1, TestHost.Run(registry, []));
        CollectionAssert.AreEqual(new[] { "outer" }, cleanupOrder.ToArray());
    }

    [TestMethod]
    public void CleanupRegisteredAfterCompletionIsRejected()
    {
        // F9 guard: a cleanup registered after the test completed can never run — it must be
        // rejected loudly (Go panics on late testing.T use) instead of silently dropped.
        ж<testing_package.T>? captured = null;
        TestRegistry registry = new("runtime/latecleanup", []);
        registry.Add("TestCapture", pointer => captured = pointer, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, []));
        Assert.IsNotNull(captured);
        Assert.ThrowsException<InvalidOperationException>(() => testing_package.Cleanup(ref captured.Value, () => { }));
    }

    [TestMethod]
    public void ParallelTestsAreReleasedPerCountIteration()
    {
        // F10 guard: Go releases top-level parallel tests at the end of EACH -count iteration
        // (serial, parallel, serial, parallel) — not batched after every iteration completes.
        ConcurrentQueue<string> order = new();
        TestRegistry registry = new("runtime/countiterations", []);
        registry.Add("TestAParallel", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Parallel();
            order.Enqueue("parallel");
        }, "runtime_test.go", 1);
        registry.Add("TestSerial", _ => order.Enqueue("serial"), "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-count", "2", "-timeout", "30s"]));
        CollectionAssert.AreEqual(new[] { "serial", "parallel", "serial", "parallel" }, order.ToArray());
    }

    [TestMethod]
    public void ParallelCapLimitsConcurrentParallelTests()
    {
        // F10 guard: -parallel caps simultaneously RUNNING parallel tests (Go semantics; the
        // default is the processor count, matching go test's GOMAXPROCS default).
        int current = 0;
        int observedMax = 0;

        TestRegistry registry = new("runtime/parallelcap", []);

        for (int i = 1; i <= 4; i++)
        {
            registry.Add($"TestParallel{i}", pointer =>
            {
                ref testing_package.T test = ref pointer.Value;
                test.Parallel();
                int now = Interlocked.Increment(ref current);
                int seenMax;
                do
                {
                    seenMax = Volatile.Read(ref observedMax);
                }
                while (now > seenMax && Interlocked.CompareExchange(ref observedMax, now, seenMax) != seenMax);
                Thread.Sleep(50);
                Interlocked.Decrement(ref current);
            }, "runtime_test.go", 1);
        }

        Assert.AreEqual(0, TestHost.Run(registry, ["-parallel", "1", "-timeout", "30s"]));
        Assert.AreEqual(1, observedMax);
    }

    [TestMethod]
    public void ParallelParentReleasesItsSlotForParallelChildren()
    {
        // F10 guard: a parallel parent must give its -parallel slot back before waiting on its
        // parallel children (Go's tRunner ordering) or a cap of 1 would deadlock this shape.
        bool childRan = false;
        TestRegistry registry = new("runtime/parallelparent", []);
        registry.Add("TestParent", pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            test.Parallel();
            test.Run("child", childPointer =>
            {
                ref testing_package.T child = ref childPointer.Value;
                child.Parallel();
                childRan = true;
            });
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-parallel", "1", "-timeout", "30s"]));
        Assert.IsTrue(childRan);
    }

    [TestMethod]
    public void ShortAndVerboseFlagsReachTheTestingShim()
    {
        // F4 support guard: testing.Short()/testing.Verbose() report the host's -short/-v flags
        // (go test defaults: both false when absent).
        bool shortSeen = true;
        bool verboseSeen = true;

        TestRegistry registry = new("runtime/flags", []);
        registry.Add("TestFlags", _ =>
        {
            shortSeen = testing_package.Short();
            verboseSeen = testing_package.Verbose();
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(registry, ["-short", "-v"]));
        Assert.IsTrue(shortSeen);
        Assert.IsTrue(verboseSeen);

        TestRegistry defaults = new("runtime/flags", []);
        defaults.Add("TestDefaults", _ =>
        {
            shortSeen = testing_package.Short();
            verboseSeen = testing_package.Verbose();
        }, "runtime_test.go", 1);

        Assert.AreEqual(0, TestHost.Run(defaults, []));
        Assert.IsFalse(shortSeen);
        Assert.IsFalse(verboseSeen);
    }

    [TestMethod]
    public void JUnitOutputSurvivesXmlInvalidCharacters()
    {
        // Real Go test logs legitimately contain XML-invalid characters (unicode/utf8's own
        // tests log U+FFFE/U+FFFF data); an unsanitized XDocument.Save threw AFTER the suite
        // completed, downgrading the whole run to an infrastructure error with no JUnit file.
        string junitPath = Path.Combine(Path.GetTempPath(), $"go2cs-junit-{Guid.NewGuid():N}.xml");

        try
        {
            TestRegistry registry = new("runtime/xmlchars", []);
            registry.Add("TestWeirdOutput", pointer => pointer.Error("bad ￾￿ chars"), "runtime_test.go", 1);

            Assert.AreEqual(1, TestHost.Run(registry, ["--junit", junitPath]));
            string junit = File.ReadAllText(junitPath);
            StringAssert.Contains(junit, "TestWeirdOutput");
            StringAssert.Contains(junit, "\\ufffe");
        }
        finally
        {
            File.Delete(junitPath);
        }
    }

    [TestMethod]
    public void FieldPointerEqualityPairsAcrossOfCalls()
    {
        // Guards the golib pointer-identity fix the Phase-4 test runtime depends on: the typed
        // ж.of(...) overload wraps its accessor in a per-call closure, and comparing the wrappers
        // made every distinct `&x.field` box unequal — `&x.f == &x.f` was FALSE (violating Go
        // pointer identity) and the address-keyed runtime semaphores in the hand-owned
        // sync/internal-poll implementations never paired Semrelease with Semacquire (the
        // ConvertedTestHarness os.ReadFile close hang). Equality now compares the field's
        // identity token: same box + same accessor = equal pointer, across call sites.
        ж<SemaHolder> box = new(new SemaHolder());
        ж<uint> first = box.of<uint>(SemaField);
        ж<uint> second = box.of<uint>(SemaField);

        Assert.IsTrue(first.Equals(second));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());

        ж<SemaHolder> otherBox = new(new SemaHolder());
        Assert.IsFalse(first.Equals(otherBox.of<uint>(SemaField)));
    }

    private struct SemaHolder
    {
        public uint Sema;
    }

    private static ref uint SemaField(ref SemaHolder instance) => ref instance.Sema;

    private static int RunSingle(string name, ActionRef action)
    {
        TestRegistry registry = new("runtime/failure", []);
        registry.Add(name, pointer =>
        {
            ref testing_package.T test = ref pointer.Value;
            action(ref test);
        }, "runtime_test.go", 1);
        return TestHost.Run(registry, []);
    }

    private delegate void ActionRef(ref testing_package.T test);
}
