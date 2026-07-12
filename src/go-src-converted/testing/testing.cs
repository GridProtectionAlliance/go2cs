// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testing provides support for automated testing of Go packages.
// It is intended to be used in concert with the "go test" command, which automates
// execution of any function of the form
//
//	func TestXxx(*testing.T)
//
// where Xxx does not start with a lowercase letter. The function name
// serves to identify the test routine.
//
// Within these functions, use the Error, Fail or related methods to signal failure.
//
// To write a new test suite, create a file that
// contains the TestXxx functions as described here,
// and give that file a name ending in "_test.go".
// The file will be excluded from regular
// package builds but will be included when the "go test" command is run.
//
// The test file can be in the same package as the one being tested,
// or in a corresponding package with the suffix "_test".
//
// If the test file is in the same package, it may refer to unexported
// identifiers within the package, as in this example:
//
//	package abs
//
//	import "testing"
//
//	func TestAbs(t *testing.T) {
//	    got := Abs(-1)
//	    if got != 1 {
//	        t.Errorf("Abs(-1) = %d; want 1", got)
//	    }
//	}
//
// If the file is in a separate "_test" package, the package being tested
// must be imported explicitly and only its exported identifiers may be used.
// This is known as "black box" testing.
//
//	package abs_test
//
//	import (
//		"testing"
//
//		"path_to_pkg/abs"
//	)
//
//	func TestAbs(t *testing.T) {
//	    got := abs.Abs(-1)
//	    if got != 1 {
//	        t.Errorf("Abs(-1) = %d; want 1", got)
//	    }
//	}
//
// For more detail, run "go help test" and "go help testflag".
//
// # Benchmarks
//
// Functions of the form
//
//	func BenchmarkXxx(*testing.B)
//
// are considered benchmarks, and are executed by the "go test" command when
// its -bench flag is provided. Benchmarks are run sequentially.
//
// For a description of the testing flags, see
// https://golang.org/cmd/go/#hdr-Testing_flags.
//
// A sample benchmark function looks like this:
//
//	func BenchmarkRandInt(b *testing.B) {
//	    for range b.N {
//	        rand.Int()
//	    }
//	}
//
// The benchmark function must run the target code b.N times.
// It is called multiple times with b.N adjusted until the
// benchmark function lasts long enough to be timed reliably.
// The output
//
//	BenchmarkRandInt-8   	68453040	        17.8 ns/op
//
// means that the loop ran 68453040 times at a speed of 17.8 ns per loop.
//
// If a benchmark needs some expensive setup before running, the timer
// may be reset:
//
//	func BenchmarkBigLen(b *testing.B) {
//	    big := NewBig()
//	    b.ResetTimer()
//	    for range b.N {
//	        big.Len()
//	    }
//	}
//
// If a benchmark needs to test performance in a parallel setting, it may use
// the RunParallel helper function; such benchmarks are intended to be used with
// the go test -cpu flag:
//
//	func BenchmarkTemplateParallel(b *testing.B) {
//	    templ := template.Must(template.New("test").Parse("Hello, {{.}}!"))
//	    b.RunParallel(func(pb *testing.PB) {
//	        var buf bytes.Buffer
//	        for pb.Next() {
//	            buf.Reset()
//	            templ.Execute(&buf, "World")
//	        }
//	    })
//	}
//
// A detailed specification of the benchmark results format is given
// in https://golang.org/design/14313-benchmark-format.
//
// There are standard tools for working with benchmark results at
// https://golang.org/x/perf/cmd.
// In particular, https://golang.org/x/perf/cmd/benchstat performs
// statistically robust A/B comparisons.
//
// # Examples
//
// The package also runs and verifies example code. Example functions may
// include a concluding line comment that begins with "Output:" and is compared with
// the standard output of the function when the tests are run. (The comparison
// ignores leading and trailing space.) These are examples of an example:
//
//	func ExampleHello() {
//	    fmt.Println("hello")
//	    // Output: hello
//	}
//
//	func ExampleSalutations() {
//	    fmt.Println("hello, and")
//	    fmt.Println("goodbye")
//	    // Output:
//	    // hello, and
//	    // goodbye
//	}
//
// The comment prefix "Unordered output:" is like "Output:", but matches any
// line order:
//
//	func ExamplePerm() {
//	    for _, value := range Perm(5) {
//	        fmt.Println(value)
//	    }
//	    // Unordered output: 4
//	    // 2
//	    // 1
//	    // 3
//	    // 0
//	}
//
// Example functions without output comments are compiled but not executed.
//
// The naming convention to declare examples for the package, a function F, a type T and
// method M on type T are:
//
//	func Example() { ... }
//	func ExampleF() { ... }
//	func ExampleT() { ... }
//	func ExampleT_M() { ... }
//
// Multiple example functions for a package/type/function/method may be provided by
// appending a distinct suffix to the name. The suffix must start with a
// lower-case letter.
//
//	func Example_suffix() { ... }
//	func ExampleF_suffix() { ... }
//	func ExampleT_suffix() { ... }
//	func ExampleT_M_suffix() { ... }
//
// The entire test file is presented as the example when it contains a single
// example function, at least one other function, type, variable, or constant
// declaration, and no test or benchmark functions.
//
// # Fuzzing
//
// 'go test' and the testing package support fuzzing, a testing technique where
// a function is called with randomly generated inputs to find bugs not
// anticipated by unit tests.
//
// Functions of the form
//
//	func FuzzXxx(*testing.F)
//
// are considered fuzz tests.
//
// For example:
//
//	func FuzzHex(f *testing.F) {
//	  for _, seed := range [][]byte{{}, {0}, {9}, {0xa}, {0xf}, {1, 2, 3, 4}} {
//	    f.Add(seed)
//	  }
//	  f.Fuzz(func(t *testing.T, in []byte) {
//	    enc := hex.EncodeToString(in)
//	    out, err := hex.DecodeString(enc)
//	    if err != nil {
//	      t.Fatalf("%v: decode: %v", in, err)
//	    }
//	    if !bytes.Equal(in, out) {
//	      t.Fatalf("%v: not equal after round trip: %v", in, out)
//	    }
//	  })
//	}
//
// A fuzz test maintains a seed corpus, or a set of inputs which are run by
// default, and can seed input generation. Seed inputs may be registered by
// calling (*F).Add or by storing files in the directory testdata/fuzz/<Name>
// (where <Name> is the name of the fuzz test) within the package containing
// the fuzz test. Seed inputs are optional, but the fuzzing engine may find
// bugs more efficiently when provided with a set of small seed inputs with good
// code coverage. These seed inputs can also serve as regression tests for bugs
// identified through fuzzing.
//
// The function passed to (*F).Fuzz within the fuzz test is considered the fuzz
// target. A fuzz target must accept a *T parameter, followed by one or more
// parameters for random inputs. The types of arguments passed to (*F).Add must
// be identical to the types of these parameters. The fuzz target may signal
// that it's found a problem the same way tests do: by calling T.Fail (or any
// method that calls it like T.Error or T.Fatal) or by panicking.
//
// When fuzzing is enabled (by setting the -fuzz flag to a regular expression
// that matches a specific fuzz test), the fuzz target is called with arguments
// generated by repeatedly making random changes to the seed inputs. On
// supported platforms, 'go test' compiles the test executable with fuzzing
// coverage instrumentation. The fuzzing engine uses that instrumentation to
// find and cache inputs that expand coverage, increasing the likelihood of
// finding bugs. If the fuzz target fails for a given input, the fuzzing engine
// writes the inputs that caused the failure to a file in the directory
// testdata/fuzz/<Name> within the package directory. This file later serves as
// a seed input. If the file can't be written at that location (for example,
// because the directory is read-only), the fuzzing engine writes the file to
// the fuzz cache directory within the build cache instead.
//
// When fuzzing is disabled, the fuzz target is called with the seed inputs
// registered with F.Add and seed inputs from testdata/fuzz/<Name>. In this
// mode, the fuzz test acts much like a regular test, with subtests started
// with F.Fuzz instead of T.Run.
//
// See https://go.dev/doc/fuzz for documentation about fuzzing.
//
// # Skipping
//
// Tests or benchmarks may be skipped at run time with a call to
// the Skip method of *T or *B:
//
//	func TestTimeConsuming(t *testing.T) {
//	    if testing.Short() {
//	        t.Skip("skipping test in short mode.")
//	    }
//	    ...
//	}
//
// The Skip method of *T can be used in a fuzz target if the input is invalid,
// but should not be considered a failing input. For example:
//
//	func FuzzJSONMarshaling(f *testing.F) {
//	    f.Fuzz(func(t *testing.T, b []byte) {
//	        var v interface{}
//	        if err := json.Unmarshal(b, &v); err != nil {
//	            t.Skip()
//	        }
//	        if _, err := json.Marshal(v); err != nil {
//	            t.Errorf("Marshal: %v", err)
//	        }
//	    })
//	}
//
// # Subtests and Sub-benchmarks
//
// The Run methods of T and B allow defining subtests and sub-benchmarks,
// without having to define separate functions for each. This enables uses
// like table-driven benchmarks and creating hierarchical tests.
// It also provides a way to share common setup and tear-down code:
//
//	func TestFoo(t *testing.T) {
//	    // <setup code>
//	    t.Run("A=1", func(t *testing.T) { ... })
//	    t.Run("A=2", func(t *testing.T) { ... })
//	    t.Run("B=1", func(t *testing.T) { ... })
//	    // <tear-down code>
//	}
//
// Each subtest and sub-benchmark has a unique name: the combination of the name
// of the top-level test and the sequence of names passed to Run, separated by
// slashes, with an optional trailing sequence number for disambiguation.
//
// The argument to the -run, -bench, and -fuzz command-line flags is an unanchored regular
// expression that matches the test's name. For tests with multiple slash-separated
// elements, such as subtests, the argument is itself slash-separated, with
// expressions matching each name element in turn. Because it is unanchored, an
// empty expression matches any string.
// For example, using "matching" to mean "whose name contains":
//
//	go test -run ''        # Run all tests.
//	go test -run Foo       # Run top-level tests matching "Foo", such as "TestFooBar".
//	go test -run Foo/A=    # For top-level tests matching "Foo", run subtests matching "A=".
//	go test -run /A=1      # For all top-level tests, run subtests matching "A=1".
//	go test -fuzz FuzzFoo  # Fuzz the target matching "FuzzFoo"
//
// The -run argument can also be used to run a specific value in the seed
// corpus, for debugging. For example:
//
//	go test -run=FuzzFoo/9ddb952d9814
//
// The -fuzz and -run flags can both be set, in order to fuzz a target but
// skip the execution of all other tests.
//
// Subtests can also be used to control parallelism. A parent test will only
// complete once all of its subtests complete. In this example, all tests are
// run in parallel with each other, and only with each other, regardless of
// other top-level tests that may be defined:
//
//	func TestGroupedParallel(t *testing.T) {
//	    for _, tc := range tests {
//	        tc := tc // capture range variable
//	        t.Run(tc.Name, func(t *testing.T) {
//	            t.Parallel()
//	            ...
//	        })
//	    }
//	}
//
// Run does not return until parallel subtests have completed, providing a way
// to clean up after a group of parallel tests:
//
//	func TestTeardownParallel(t *testing.T) {
//	    // This Run will not return until the parallel tests finish.
//	    t.Run("group", func(t *testing.T) {
//	        t.Run("Test1", parallelTest1)
//	        t.Run("Test2", parallelTest2)
//	        t.Run("Test3", parallelTest3)
//	    })
//	    // <tear-down code>
//	}
//
// # Main
//
// It is sometimes necessary for a test or benchmark program to do extra setup or teardown
// before or after it executes. It is also sometimes necessary to control
// which code runs on the main thread. To support these and other cases,
// if a test file contains a function:
//
//	func TestMain(m *testing.M)
//
// then the generated test will call TestMain(m) instead of running the tests or benchmarks
// directly. TestMain runs in the main goroutine and can do whatever setup
// and teardown is necessary around a call to m.Run. m.Run will return an exit
// code that may be passed to os.Exit. If TestMain returns, the test wrapper
// will pass the result of m.Run to os.Exit itself.
//
// When TestMain is called, flag.Parse has not been run. If TestMain depends on
// command-line flags, including those of the testing package, it should call
// flag.Parse explicitly. Command line flags are always parsed by the time test
// or benchmark functions run.
//
// A simple implementation of TestMain is:
//
//	func TestMain(m *testing.M) {
//		// call flag.Parse() here if TestMain uses flags
//		m.Run()
//	}
//
// TestMain is a low-level primitive and should not be necessary for casual
// testing needs, where ordinary test functions suffice.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using goexperiment = @internal.goexperiment_package;
using race = @internal.race_package;
using Δio = io_package;
using rand = go.math.rand_package;
using os = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using trace = go.runtime.trace_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @internal;
using fs = go.io.fs_package;
using go.math;
using go.runtime;
using go.sync;
using go.unicode;
using ꓸꓸꓸany = Span<any>;

partial class testing_package {

internal static bool initRan;

// Init registers testing flags. These flags are automatically registered by
// the "go test" command before running test functions, so Init is only needed
// when calling functions such as Benchmark without using "go test".
//
// Init is not safe to call concurrently. It has no effect if it was already called.
public static void Init() {
    if (initRan) {
        return;
    }
    initRan = true;
    // The short flag requests that tests run more quickly, but its functionality
    // is provided by test writers themselves. The testing package is just its
    // home. The all.bash installation script sets it to make installation more
    // efficient, but by default the flag is off so a plain "go test" will do a
    // full test of the package.
    @short = flag.Bool("test.short"u8, false, "run smaller test suite to save time"u8);
    // The failfast flag requests that test execution stop after the first test failure.
    failFast = flag.Bool("test.failfast"u8, false, "do not start new tests after the first test failure"u8);
    // The directory in which to create profile files and the like. When run from
    // "go test", the binary always runs in the source directory for the package;
    // this flag lets "go test" tell the binary to write the files in the directory where
    // the "go test" command is run.
    outputDir = flag.String("test.outputdir"u8, ""u8, "write profiles to `dir`"u8);
    // Report as tests are run; default is silent for success.
    flag.Var(new chattyFlagжValue(Ꮡchatty), "test.v"u8, "verbose: print additional output"u8);
    count = flag.Uint("test.count"u8, 1, "run tests and benchmarks `n` times"u8);
    coverProfile = flag.String("test.coverprofile"u8, ""u8, "write a coverage profile to `file`"u8);
    gocoverdir = flag.String("test.gocoverdir"u8, ""u8, "write coverage intermediate files to this directory"u8);
    matchList = flag.String("test.list"u8, ""u8, "list tests, examples, and benchmarks matching `regexp` then exit"u8);
    match = flag.String("test.run"u8, ""u8, "run only tests and examples matching `regexp`"u8);
    skip = flag.String("test.skip"u8, ""u8, "do not list or run tests matching `regexp`"u8);
    memProfile = flag.String("test.memprofile"u8, ""u8, "write an allocation profile to `file`"u8);
    memProfileRate = flag.Int("test.memprofilerate"u8, 0, "set memory allocation profiling `rate` (see runtime.MemProfileRate)"u8);
    cpuProfile = flag.String("test.cpuprofile"u8, ""u8, "write a cpu profile to `file`"u8);
    blockProfile = flag.String("test.blockprofile"u8, ""u8, "write a goroutine blocking profile to `file`"u8);
    blockProfileRate = flag.Int("test.blockprofilerate"u8, 1, "set blocking profile `rate` (see runtime.SetBlockProfileRate)"u8);
    mutexProfile = flag.String("test.mutexprofile"u8, ""u8, "write a mutex contention profile to the named file after execution"u8);
    mutexProfileFraction = flag.Int("test.mutexprofilefraction"u8, 1, "if >= 0, calls runtime.SetMutexProfileFraction()"u8);
    panicOnExit0 = flag.Bool("test.paniconexit0"u8, false, "panic on call to os.Exit(0)"u8);
    traceFile = flag.String("test.trace"u8, ""u8, "write an execution trace to `file`"u8);
    timeout = flag.Duration("test.timeout"u8, 0, "panic test binary after duration `d` (default 0, timeout disabled)"u8);
    cpuListStr = flag.String("test.cpu"u8, ""u8, "comma-separated `list` of cpu counts to run each test with"u8);
    parallel = flag.Int("test.parallel"u8, Δruntime.GOMAXPROCS(0), "run at most `n` tests in parallel"u8);
    testlog = flag.String("test.testlogfile"u8, ""u8, "write test action log to `file` (for use only by cmd/go)"u8);
    shuffle = flag.String("test.shuffle"u8, "off"u8, "randomize the execution order of tests and benchmarks"u8);
    fullPath = flag.Bool("test.fullpath"u8, false, "show full file names in error messages"u8);
    initBenchmarkFlags();
    initFuzzFlags();
}

internal static ж<bool> @short;
internal static ж<bool> failFast;
internal static ж<@string> outputDir;
internal static ж<chattyFlag> Ꮡchatty = new(default(chattyFlag));
internal static ref chattyFlag chatty => ref Ꮡchatty.Value;
internal static ж<nuint> count;
internal static ж<@string> coverProfile;
internal static ж<@string> gocoverdir;
internal static ж<@string> matchList;
internal static ж<@string> match;
internal static ж<@string> skip;
internal static ж<@string> memProfile;
internal static ж<nint> memProfileRate;
internal static ж<@string> cpuProfile;
internal static ж<@string> blockProfile;
internal static ж<nint> blockProfileRate;
internal static ж<@string> mutexProfile;
internal static ж<nint> mutexProfileFraction;
internal static ж<bool> panicOnExit0;
internal static ж<@string> traceFile;
internal static ж<time.Duration> timeout;
internal static ж<@string> cpuListStr;
internal static ж<nint> parallel;
internal static ж<@string> shuffle;
internal static ж<@string> testlog;
internal static ж<bool> fullPath;
internal static bool haveExamples; // are there examples?
internal static slice<nint> cpuList;
internal static ж<os.File> testlogFile;
internal static ж<atomic.Uint32> ᏑnumFailed = new(default(atomic.Uint32));
internal static ref atomic.Uint32 numFailed => ref ᏑnumFailed.Value;     // number of test failures
internal static ж<Δsync.Map> Ꮡrunning = new(default(Δsync.Map));
internal static ref Δsync.Map running => ref Ꮡrunning.Value; // map[string]time.Time of running, unpaused tests

[GoType] partial struct chattyFlag {
    internal bool on; // -v is set in some form
    internal bool json; // -v=test2json is set, to make output better for test2json
}

[GoRecv] internal static bool IsBoolFlag(this ref chattyFlag _) {
    return true;
}

[GoRecv] internal static error Set(this ref chattyFlag f, @string arg) {
    var exprᴛ1 = arg;
    if (exprᴛ1 == "true"u8 || exprᴛ1 == "test2json"u8) {
        f.on = true;
        f.json = arg == "test2json"u8;
    }
    else if (exprᴛ1 == "false"u8) {
        f.on = false;
        f.json = false;
    }
    else { /* default: */
        return fmt.Errorf("invalid flag -test.v=%s"u8, arg);
    }

    return default!;
}

[GoRecv] internal static @string String(this ref chattyFlag f) {
    if (f.json) {
        return "test2json"u8;
    }
    if (f.on) {
        return "true"u8;
    }
    return "false"u8;
}

[GoRecv] internal static any Get(this ref chattyFlag f) {
    if (f.json) {
        return (@string)"test2json";
    }
    return f.on;
}

internal const byte marker = /* byte(0x16) */ 22; // ^V for framing

[GoRecv] internal static @string prefix(this ref chattyFlag f) {
    if (f.json) {
        return ((@string)marker);
    }
    return ""u8;
}

[GoType] partial struct chattyPrinter {
    internal Δio.Writer w;
    internal Δsync.Mutex lastNameMu; // guards lastName
    internal @string lastName;    // last printed test name in chatty mode
    internal bool json;       // -v=json output mode
}

internal static ж<chattyPrinter> newChattyPrinter(Δio.Writer w) {
    return Ꮡ(new chattyPrinter(w: w, json: chatty.json));
}

// prefix is like chatty.prefix but using p.json instead of chatty.json.
// Using p.json allows tests to check the json behavior without modifying
// the global variable. For convenience, we allow p == nil and treat
// that as not in json mode (because it's not chatty at all).
internal static @string prefix(this ж<chattyPrinter> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    if (Ꮡp != nil && p.json) {
        return ((@string)marker);
    }
    return ""u8;
}

// Updatef prints a message about the status of the named test to w.
//
// The formatted message must include the test name itself.
internal static void Updatef(this ж<chattyPrinter> Ꮡp, @string testName, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        Ꮡp.of(chattyPrinter.ᏑlastNameMu).Lock();
        defer(Ꮡp.of(chattyPrinter.ᏑlastNameMu).Unlock);
        // Since the message already implies an association with a specific new test,
        // we don't need to check what the old test name was or log an extra NAME line
        // for it. (We're updating it anyway, and the current message already includes
        // the test name.)
        p.lastName = testName;
        fmt.Fprintf(p.w, Ꮡp.prefix() + format, args.ꓸꓸꓸ);
    });
}

// Printf prints a message, generated by the named test, that does not
// necessarily mention that tests's name itself.
internal static void Printf(this ж<chattyPrinter> Ꮡp, @string testName, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        Ꮡp.of(chattyPrinter.ᏑlastNameMu).Lock();
        defer(Ꮡp.of(chattyPrinter.ᏑlastNameMu).Unlock);
        if (p.lastName == ""u8){
            p.lastName = testName;
        } else 
        if (p.lastName != testName) {
            fmt.Fprintf(p.w, "%s=== NAME  %s\n"u8, Ꮡp.prefix(), testName);
            p.lastName = testName;
        }
        fmt.Fprintf(p.w, format, args.ꓸꓸꓸ);
    });
}

// The maximum number of stack frames to go through when skipping helper functions for
// the purpose of decorating log messages.
internal static readonly UntypedInt maxStackLen = 50;

// common holds the elements common between T and B and
// captures common methods such as Errorf.
[GoType] partial struct common {
    internal Δsync.RWMutex mu;         // guards this group of fields
    internal slice<byte> output;          // Output generated by test or benchmark.
    internal Δio.Writer w;            // For flushToParent.
    internal bool ran;                 // Test or benchmark (or one of its subtests) was executed.
    internal bool failed;                 // Test or benchmark has failed.
    internal bool skipped;                 // Test or benchmark has been skipped.
    internal bool done;                 // Test is finished and all subtests have completed.
    internal map<uintptr, EmptyStruct> helperPCs; // functions to be skipped when writing file/line info
    internal map<@string, EmptyStruct> helperNames; // helperPCs converted to function names
    internal slice<Action> cleanups;        // optional functions to be called at the end of the test
    internal @string cleanupName;              // Name of the cleanup function.
    internal slice<uintptr> cleanupPc;       // The stack trace at the point where Cleanup was called.
    internal bool finished;                 // Test function has completed.
    internal bool inFuzzFn;                 // Whether the fuzz target, if this is one, is running.
    internal ж<chattyPrinter> chatty; // A copy of chattyPrinter, if the chatty flag is set.
    internal bool bench;           // Whether the current test is a benchmark.
    internal atomic.Bool hasSub;    // whether there are sub-benchmarks.
    internal atomic.Bool cleanupStarted;    // Registered cleanup callbacks have started to execute
    internal @string runner;        // Function name of tRunner running the test.
    internal bool isParallel;           // Whether the test is parallel.
    internal ж<common> parent;
    internal nint level;              // Nesting depth of test or benchmark.
    internal slice<uintptr> creator;    // If level > 0, the stack trace at the point where the parent called t.Run.
    internal @string name;           // Name of test or benchmark.
    internal highPrecisionTime start; // Time test or benchmark started
    internal time.Duration duration;
    internal channel<bool> barrier; // To signal parallel subtests they may start. Nil when T.Parallel is not present (B) or not usable (when fuzzing).
    internal channel<bool> signal; // To signal a test is done.
    internal slice<ж<T>> sub; // Queue of subtests to be run in parallel.
    internal atomic.Int64 lastRaceErrors; // Max value of race.Errors seen during the test or its subtests.
    internal atomic.Bool raceErrorLogged;
    internal Δsync.Mutex tempDirMu;
    internal @string tempDir;
    internal error tempDirErr;
    internal int32 tempDirSeq;
}

// Short reports whether the -test.short flag is set.
public static bool Short() {
    if (@short == nil) {
        throw panic("testing: Short called before Init");
    }
    // Catch code that calls this from TestMain without first calling flag.Parse.
    if (!flag.Parsed()) {
        throw panic("testing: Short called before Parse");
    }
    return @short.Value;
}

// testBinary is set by cmd/go to "1" if this is a binary built by "go test".
// The value is set to "1" by a -X option to cmd/link. We assume that
// because this is possible, the compiler will not optimize testBinary
// into a constant on the basis that it is an unexported package-scope
// variable that is never changed. If the compiler ever starts implementing
// such an optimization, we will need some technique to mark this variable
// as "changed by a cmd/link -X option".
internal static @string testBinary = "0"u8;

// Testing reports whether the current code is being run in a test.
// This will report true in programs created by "go test",
// false in programs created by "go build".
public static bool Testing() {
    return testBinary == "1"u8;
}

// CoverMode reports what the test coverage mode is set to. The
// values are "set", "count", or "atomic". The return value will be
// empty if test coverage is not enabled.
public static @string CoverMode() {
    if (goexperiment.CoverageRedesign) {
        return cover2.mode;
    }
    return cover.Mode;
}

// Verbose reports whether the -test.v flag is set.
public static bool Verbose() {
    // Same as in Short.
    if (!flag.Parsed()) {
        throw panic("testing: Verbose called before Parse");
    }
    return chatty.on;
}

[GoRecv] internal static void checkFuzzFn(this ref common c, @string name) {
    if (c.inFuzzFn) {
        throw panic(fmt.Sprintf("testing: f.%s was called inside the fuzz target, use t.%s instead"u8, name, name));
    }
}

// frameSkip searches, starting after skip frames, for the first caller frame
// in a function not marked as a helper and returns that frame.
// The search stops if it finds a tRunner function that
// was the entry point into the test and the test is not a subtest.
// This function must be called with c.mu held.
internal static Δruntime.Frame frameSkip(this ж<common> Ꮡc, nint skip) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    // If the search continues into the parent test, we'll have to hold
    // its mu temporarily. If we then return, we need to unlock it.
    var shouldUnlock = false;
    defer(() => {
        if (shouldUnlock) {
            Ꮡc.of(common.Ꮡmu).Unlock();
        }
    });
    array<uintptr> pc = new(50); /* maxStackLen */
    // Skip two extra frames to account for this function
    // and runtime.Callers itself.
    nint n = Δruntime.Callers(skip + 2, pc[..]);
    if (n == 0) {
        throw panic("testing: zero callers found");
    }
    var frames = Δruntime.CallersFrames(pc[..(int)(n)]);
    Δruntime.Frame firstFrame = default!;
    Δruntime.Frame prevFrame = default!;
    Δruntime.Frame frame = default!;
    for (var more = true; more; prevFrame = frame) {
        (frame, more) = frames.Next();
        if (frame.Function == "runtime.gopanic"u8) {
            continue;
        }
        if (frame.Function == c.cleanupName) {
            frames = Δruntime.CallersFrames(c.cleanupPc);
            continue;
        }
        if (firstFrame.PC == 0) {
            firstFrame = frame;
        }
        if (frame.Function == c.runner) {
            // We've gone up all the way to the tRunner calling
            // the test function (so the user must have
            // called tb.Helper from inside that test function).
            // If this is a top-level test, only skip up to the test function itself.
            // If we're in a subtest, continue searching in the parent test,
            // starting from the point of the call to Run which created this subtest.
            if (c.level > 1) {
                frames = Δruntime.CallersFrames(c.creator);
                var parent = c.parent;
                // We're no longer looking at the current c after this point,
                // so we should unlock its mu, unless it's the original receiver,
                // in which case our caller doesn't expect us to do that.
                if (shouldUnlock) {
                    Ꮡc.of(common.Ꮡmu).Unlock();
                }
                Ꮡc = parent; c = ref Ꮡc.Value;
                // Remember to unlock c.mu when we no longer need it, either
                // because we went up another nesting level, or because we
                // returned.
                shouldUnlock = true;
                Ꮡc.of(common.Ꮡmu).Lock();
                continue;
            }
            return prevFrame;
        }
        // If more helper PCs have been added since we last did the conversion
        if (c.helperNames == default!) {
            c.helperNames = new map<@string, EmptyStruct>();
            foreach (var (pcΔ1, _) in c.helperPCs) {
                c.helperNames[pcToName(pcΔ1)] = new EmptyStruct();
            }
        }
        {
            var (_, ok) = c.helperNames[frame.Function, ꟷ]; if (!ok) {
                // Found a frame that wasn't inside a helper function.
                return frame;
            }
        }
    }
    return firstFrame;
});

// decorate prefixes the string with the file and line of the call site
// and inserts the final newline if needed and indentation spaces for formatting.
// This function must be called with c.mu held.
internal static @string decorate(this ж<common> Ꮡc, @string s, nint skip) {
    var frame = Ꮡc.frameSkip(skip);
    @string @file = frame.File;
    nint line = frame.Line;
    if (@file != ""u8){
        if (fullPath.Value){
        } else 
        {
            nint index = strings.LastIndexAny(@file, // If relative path, truncate file name at last file name separator.
 @"/\"u8); if (index >= 0) {
                @file = @file[(int)(index + 1)..];
            }
        }
    } else {
        @file = "???"u8;
    }
    if (line == 0) {
        line = 1;
    }
    var buf = @new<strings.Builder>();
    // Every line is indented at least 4 spaces.
    buf.WriteString("    "u8);
    fmt.Fprintf(new strings_BuilderжWriter(buf), "%s:%d: "u8, @file, line);
    var lines = strings.Split(s, "\n"u8);
    {
        nint l = len(lines); if (l > 1 && lines[l - 1] == "") {
            lines = lines[..(int)(l - 1)];
        }
    }
    foreach (var (i, lineΔ1) in lines) {
        if (i > 0) {
            // Second and subsequent lines are indented an additional 4 spaces.
            buf.WriteString("\n        "u8);
        }
        buf.WriteString(lineΔ1);
    }
    buf.WriteByte((rune)'\n');
    return buf.String();
}

// flushToParent writes c.output to the parent after first writing the header
// with the given format and arguments.
internal static void flushToParent(this ж<common> Ꮡc, @string testName, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();
    func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

        var p = c.parent;
        p.of(common.Ꮡmu).Lock();
        var pʗ1 = p;
        defer(pʗ1.of(common.Ꮡmu).Unlock);
        Ꮡc.of(common.Ꮡmu).Lock();
        defer(Ꮡc.of(common.Ꮡmu).Unlock);
        if (len(c.output) > 0) {
            // Add the current c.output to the print,
            // and then arrange for the print to replace c.output.
            // (This displays the logged output after the --- FAIL line.)
            format += "%s"u8;
            args = append(args.slice(-1, len(args), len(args)), (any)(c.output));
            c.output = c.output[..0];
        }
        if (c.chatty != nil && (AreEqual((~p).w, (~c.chatty).w) || (~c.chatty).json)){
            // We're flushing to the actual output, so track that this output is
            // associated with a specific test (and, specifically, that the next output
            // is *not* associated with that test).
            //
            // Moreover, if c.output is non-empty it is important that this write be
            // atomic with respect to the output of other tests, so that we don't end up
            // with confusing '=== NAME' lines in the middle of our '--- PASS' block.
            // Neither humans nor cmd/test2json can parse those easily.
            // (See https://go.dev/issue/40771.)
            //
            // If test2json is used, we never flush to parent tests,
            // so that the json stream shows subtests as they finish.
            // (See https://go.dev/issue/29811.)
            c.chatty.Updatef(testName, format, args.ꓸꓸꓸ);
        } else {
            // We're flushing to the output buffer of the parent test, which will
            // itself follow a test-name header when it is finally flushed to stdout.
            fmt.Fprintf((~p).w, c.chatty.prefix() + format, args.ꓸꓸꓸ);
        }
    });
}

[GoType] partial struct indenter {
    internal ж<common> c;
}

internal static (nint n, error err) Write(this indenter w, slice<byte> b) {
    nint n = default!;
    error err = default!;

    n = len(b);
    while (len(b) > 0) {
        nint end = bytes.IndexByte(b, (rune)'\n');
        if (end == -1){
            end = len(b);
        } else {
            end++;
        }
        // An indent of 4 spaces will neatly align the dashes with the status
        // indicator of the parent.
        var line = b[..(int)(end)];
        if (line[0] == marker) {
            w.c.Value.output = append((~w.c).output, marker);
            line = line[1..];
        }
        @string indent = "    "u8;
        w.c.Value.output = append((~w.c).output, indent.ꓸꓸꓸ);
        w.c.Value.output = append((~w.c).output, line.ꓸꓸꓸ);
        b = b[(int)(end)..];
    }
    return (n, err);
}

// fmtDuration returns a string representing d in the form "87.00s".
internal static @string fmtDuration(time.Duration d) {
    return fmt.Sprintf("%.2fs"u8, d.Seconds());
}

// TB is the interface common to T, B, and F.
[GoType] partial interface TB {
    void Cleanup(Action _);
    void Error(params ꓸꓸꓸany argsʗp);
    void Errorf(@string format, params ꓸꓸꓸany argsʗp);
    void Fail();
    void FailNow();
    bool Failed();
    void Fatal(params ꓸꓸꓸany argsʗp);
    void Fatalf(@string format, params ꓸꓸꓸany argsʗp);
    void Helper();
    void Log(params ꓸꓸꓸany argsʗp);
    void Logf(@string format, params ꓸꓸꓸany argsʗp);
    @string Name();
    void Setenv(@string key, @string value);
    void Skip(params ꓸꓸꓸany argsʗp);
    void SkipNow();
    void Skipf(@string format, params ꓸꓸꓸany argsʗp);
    bool Skipped();
    @string TempDir();
    // A private method to prevent users implementing the
    // interface and so future additions to it will not
    // violate Go 1 compatibility.
    void @private();
}

internal static TB _ᴛ2ʗ = new TжTB((ж<T>)(default!));

internal static TB _ᴛ3ʗ = new BжTB((ж<B>)(default!));

// T is a type passed to Test functions to manage test state and support formatted test logs.
//
// A test ends when its Test function returns or calls any of the methods
// FailNow, Fatal, Fatalf, SkipNow, Skip, or Skipf. Those methods, as well as
// the Parallel method, must be called only from the goroutine running the
// Test function.
//
// The other reporting methods, such as the variations of Log and Error,
// may be called simultaneously from multiple goroutines.
[GoType] partial struct T {
    internal partial ref common common { get; }
    internal bool isEnvSet;
    internal ж<testContext> context; // For running tests and subtests.
}

[GoRecv] internal static void @private(this ref common c) {
}

// Name returns the name of the running (sub-) test or benchmark.
//
// The name will include the name of the test along with the names of
// any nested sub-tests. If two sibling sub-tests have the same name,
// Name will append a suffix to guarantee the returned name is unique.
[GoRecv] internal static @string Name(this ref common c) {
    return c.name;
}

internal static void setRan(this ж<common> Ꮡc) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    if (c.parent != nil) {
        c.parent.setRan();
    }
    Ꮡc.of(common.Ꮡmu).Lock();
    defer(Ꮡc.of(common.Ꮡmu).Unlock);
    c.ran = true;
});

// Fail marks the function as having failed but continues execution.
internal static void Fail(this ж<common> Ꮡc) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    if (c.parent != nil) {
        c.parent.Fail();
    }
    Ꮡc.of(common.Ꮡmu).Lock();
    defer(Ꮡc.of(common.Ꮡmu).Unlock);
    // c.done needs to be locked to synchronize checks to c.done in parent tests.
    if (c.done) {
        throw panic("Fail in goroutine after " + c.name + " has completed");
    }
    c.failed = true;
});

// Failed reports whether the function has failed.
internal static bool Failed(this ж<common> Ꮡc) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(common.Ꮡmu).RLock();
    defer(Ꮡc.of(common.Ꮡmu).RUnlock);
    if (!c.done && (int64)race.Errors() > Ꮡc.of(common.ᏑlastRaceErrors).Load()) {
        Ꮡc.of(common.Ꮡmu).RUnlock();
        Ꮡc.checkRaces();
        Ꮡc.of(common.Ꮡmu).RLock();
    }
    return c.failed;
});

// FailNow marks the function as having failed and stops its execution
// by calling runtime.Goexit (which then runs all deferred calls in the
// current goroutine).
// Execution will continue at the next test or benchmark.
// FailNow must be called from the goroutine running the
// test or benchmark function, not from other goroutines
// created during the test. Calling FailNow does not stop
// those other goroutines.
internal static void FailNow(this ж<common> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    c.checkFuzzFn("FailNow"u8);
    Ꮡc.Fail();
    // Calling runtime.Goexit will exit the goroutine, which
    // will run the deferred functions in this goroutine,
    // which will eventually run the deferred lines in tRunner,
    // which will signal to the test loop that this test is done.
    //
    // A previous version of this code said:
    //
    //	c.duration = ...
    //	c.signal <- c.self
    //	runtime.Goexit()
    //
    // This previous version duplicated code (those lines are in
    // tRunner no matter what), but worse the goroutine teardown
    // implicit in runtime.Goexit was not guaranteed to complete
    // before the test exited. If a test deferred an important cleanup
    // function (like removing temporary files), there was no guarantee
    // it would run on a test failure. Because we send on c.signal during
    // a top-of-stack deferred function now, we know that the send
    // only happens after any other stacked defers have completed.
    Ꮡc.of(common.Ꮡmu).Lock();
    c.finished = true;
    Ꮡc.of(common.Ꮡmu).Unlock();
    Δruntime.Goexit();
}

// log generates the output. It's always at the same stack depth.
internal static void log(this ж<common> Ꮡc, @string s) {
    Ꮡc.logDepth(s, 3);
}

// logDepth + log + public function

// logDepth generates the output at an arbitrary stack depth.
internal static void logDepth(this ж<common> Ꮡc, @string s, nint depth) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(common.Ꮡmu).Lock();
    defer(Ꮡc.of(common.Ꮡmu).Unlock);
    if (c.done){
        // This test has already finished. Try and log this message
        // with our parent. If we don't have a parent, panic.
        for (var parent = c.parent; parent != nil; parent = parent.Value.parent) {
            parent.of(common.Ꮡmu).Lock();
            var parentʗ1 = parent;
            defer(parentʗ1.of(common.Ꮡmu).Unlock);
            if (!(~parent).done) {
                parent.Value.output = append((~parent).output, parent.decorate(s, depth + 1).ꓸꓸꓸ);
                return;
            }
        }
        throw panic("Log in goroutine after " + c.name + " has completed: " + s);
    } else {
        if (c.chatty != nil) {
            if (c.bench){
                // Benchmarks don't print === CONT, so we should skip the test
                // printer and just print straight to stdout.
                fmt.Print(Ꮡc.decorate(s, depth + 1));
            } else {
                c.chatty.Printf(c.name, "%s"u8, Ꮡc.decorate(s, depth + 1));
            }
            return;
        }
        c.output = append(c.output, Ꮡc.decorate(s, depth + 1).ꓸꓸꓸ);
    }
});

// Log formats its arguments using default formatting, analogous to Println,
// and records the text in the error log. For tests, the text will be printed only if
// the test fails or the -test.v flag is set. For benchmarks, the text is always
// printed to avoid having performance depend on the value of the -test.v flag.
internal static void Log(this ж<common> Ꮡc, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Log"u8);
    Ꮡc.log(fmt.Sprintln(args.ꓸꓸꓸ));
}

// Logf formats its arguments according to the format, analogous to Printf, and
// records the text in the error log. A final newline is added if not provided. For
// tests, the text will be printed only if the test fails or the -test.v flag is
// set. For benchmarks, the text is always printed to avoid having performance
// depend on the value of the -test.v flag.
internal static void Logf(this ж<common> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Logf"u8);
    Ꮡc.log(fmt.Sprintf(format, args.ꓸꓸꓸ));
}

// Error is equivalent to Log followed by Fail.
internal static void Error(this ж<common> Ꮡc, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Error"u8);
    Ꮡc.log(fmt.Sprintln(args.ꓸꓸꓸ));
    Ꮡc.Fail();
}

// Errorf is equivalent to Logf followed by Fail.
internal static void Errorf(this ж<common> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Errorf"u8);
    Ꮡc.log(fmt.Sprintf(format, args.ꓸꓸꓸ));
    Ꮡc.Fail();
}

// Fatal is equivalent to Log followed by FailNow.
internal static void Fatal(this ж<common> Ꮡc, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Fatal"u8);
    Ꮡc.log(fmt.Sprintln(args.ꓸꓸꓸ));
    Ꮡc.FailNow();
}

// Fatalf is equivalent to Logf followed by FailNow.
internal static void Fatalf(this ж<common> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Fatalf"u8);
    Ꮡc.log(fmt.Sprintf(format, args.ꓸꓸꓸ));
    Ꮡc.FailNow();
}

// Skip is equivalent to Log followed by SkipNow.
internal static void Skip(this ж<common> Ꮡc, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Skip"u8);
    Ꮡc.log(fmt.Sprintln(args.ꓸꓸꓸ));
    Ꮡc.SkipNow();
}

// Skipf is equivalent to Logf followed by SkipNow.
internal static void Skipf(this ж<common> Ꮡc, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    ref var c = ref Ꮡc.Value;
    c.checkFuzzFn("Skipf"u8);
    Ꮡc.log(fmt.Sprintf(format, args.ꓸꓸꓸ));
    Ꮡc.SkipNow();
}

// SkipNow marks the test as having been skipped and stops its execution
// by calling [runtime.Goexit].
// If a test fails (see Error, Errorf, Fail) and is then skipped,
// it is still considered to have failed.
// Execution will continue at the next test or benchmark. See also FailNow.
// SkipNow must be called from the goroutine running the test, not from
// other goroutines created during the test. Calling SkipNow does not stop
// those other goroutines.
internal static void SkipNow(this ж<common> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    c.checkFuzzFn("SkipNow"u8);
    Ꮡc.of(common.Ꮡmu).Lock();
    c.skipped = true;
    c.finished = true;
    Ꮡc.of(common.Ꮡmu).Unlock();
    Δruntime.Goexit();
}

// Skipped reports whether the test was skipped.
internal static bool Skipped(this ж<common> Ꮡc) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(common.Ꮡmu).RLock();
    defer(Ꮡc.of(common.Ꮡmu).RUnlock);
    return c.skipped;
});

// Helper marks the calling function as a test helper function.
// When printing file and line information, that function will be skipped.
// Helper may be called simultaneously from multiple goroutines.
internal static void Helper(this ж<common> Ꮡc) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(common.Ꮡmu).Lock();
    defer(Ꮡc.of(common.Ꮡmu).Unlock);
    if (c.helperPCs == default!) {
        c.helperPCs = new map<uintptr, EmptyStruct>();
    }
    // repeating code from callerName here to save walking a stack frame
    array<uintptr> pc = new(1);
    nint n = Δruntime.Callers(2, pc[..]);
    // skip runtime.Callers + Helper
    if (n == 0) {
        throw panic("testing: zero callers found");
    }
    {
        var (_, found) = c.helperPCs[pc[0], ꟷ]; if (!found) {
            c.helperPCs[pc[0]] = new EmptyStruct();
            c.helperNames = default!;
        }
    }
});

// map will be recreated next time it is needed

// Cleanup registers a function to be called when the test (or subtest) and all its
// subtests complete. Cleanup functions will be called in last added,
// first called order.
internal static void Cleanup(this ж<common> Ꮡc, Action f) => func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

    c.checkFuzzFn("Cleanup"u8);
    array<uintptr> pc = new(50); /* maxStackLen */
    // Skip two extra frames to account for this function and runtime.Callers itself.
    nint n = Δruntime.Callers(2, pc[..]);
    var cleanupPc = pc[..(int)(n)];
    var cleanupPcʗ1 = cleanupPc;
    var fn = () => func((defer, recover) => {
        defer(() => {
            Ꮡc.of(common.Ꮡmu).Lock();
            defer(Ꮡc.of(common.Ꮡmu).Unlock);
            Ꮡc.Value.cleanupName = ""u8;
            Ꮡc.Value.cleanupPc = default!;
        });
        @string name = callerName(0);
        Ꮡc.of(common.Ꮡmu).Lock();
        Ꮡc.Value.cleanupName = name;
        Ꮡc.Value.cleanupPc = cleanupPcʗ1;
        Ꮡc.of(common.Ꮡmu).Unlock();
        f();
    });
    Ꮡc.of(common.Ꮡmu).Lock();
    defer(Ꮡc.of(common.Ꮡmu).Unlock);
    c.cleanups = append(c.cleanups, fn);
});

// TempDir returns a temporary directory for the test to use.
// The directory is automatically removed when the test and
// all its subtests complete.
// Each subsequent call to t.TempDir returns a unique directory;
// if the directory creation fails, TempDir terminates the test by calling Fatal.
internal static @string TempDir(this ж<common> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    c.checkFuzzFn("TempDir"u8);
    // Use a single parent directory for all the temporary directories
    // created by a test, each numbered sequentially.
    Ꮡc.of(common.ᏑtempDirMu).Lock();
    bool nonExistent = default!;
    if (c.tempDir == ""u8){
        // Usually the case with js/wasm
        nonExistent = true;
    } else {
        var (_, err) = os.Stat(c.tempDir);
        nonExistent = os.IsNotExist(err);
        if (err != default! && !nonExistent) {
            Ꮡc.Fatalf("TempDir: %v"u8, err);
        }
    }
    if (nonExistent) {
        Ꮡc.Helper();
        // Drop unusual characters (such as path separators or
        // characters interacting with globs) from the directory name to
        // avoid surprising os.MkdirTemp behavior.
        var mapper = (rune r) => {
            if (r < utf8.RuneSelf){
                @string allowed = "!#$%&()+,-.=@^_{}~ "u8;
                if ((rune)'0' <= r && r <= (rune)'9' || (rune)'a' <= r && r <= (rune)'z' || (rune)'A' <= r && r <= (rune)'Z') {
                    return r;
                }
                if (strings.ContainsRune(allowed, r)) {
                    return r;
                }
            } else 
            if (Δunicode.IsLetter(r) || Δunicode.IsNumber(r)) {
                return r;
            }
            return -1;
        };
        @string pattern = strings.Map(mapper, c.Name());
        (c.tempDir, c.tempDirErr) = os.MkdirTemp(""u8, pattern);
        if (c.tempDirErr == default!) {
            Ꮡc.Cleanup(() => {
                {
                    var err = removeAll(Ꮡc.Value.tempDir); if (err != default!) {
                        Ꮡc.Errorf("TempDir RemoveAll cleanup: %v"u8, err);
                    }
                }
            });
        }
    }
    if (c.tempDirErr == default!) {
        c.tempDirSeq++;
    }
    var seq = c.tempDirSeq;
    Ꮡc.of(common.ᏑtempDirMu).Unlock();
    if (c.tempDirErr != default!) {
        Ꮡc.Fatalf("TempDir: %v"u8, c.tempDirErr);
    }
    @string dir = fmt.Sprintf("%s%c%03d"u8, c.tempDir, os.PathSeparator, seq);
    {
        var err = os.Mkdir(dir, 511); if (err != default!) {
            Ꮡc.Fatalf("TempDir: %v"u8, err);
        }
    }
    return dir;
}

// removeAll is like os.RemoveAll, but retries Windows "Access is denied."
// errors up to an arbitrary timeout.
//
// Those errors have been known to occur spuriously on at least the
// windows-amd64-2012 builder (https://go.dev/issue/50051), and can only occur
// legitimately if the test leaves behind a temp file that either is still open
// or the test otherwise lacks permission to delete. In the case of legitimate
// failures, a failing test may take a bit longer to fail, but once the test is
// fixed the extra latency will go away.
internal static error removeAll(@string path) {
    time.Duration arbitraryTimeout = /* 2 * time.Second */ 2000000000;
    time.Time start = default!;
    time.Duration nextSleep = 1 * time.Millisecond;
    while (ᐧ) {
        var err = os.RemoveAll(path);
        if (!isWindowsRetryable(err)) {
            return err;
        }
        if (start.IsZero()){
            start = time.Now();
        } else 
        {
            var d = time.Since(start) + nextSleep; if (d >= arbitraryTimeout) {
                return err;
            }
        }
        time.Sleep(nextSleep);
        nextSleep += ((time.Duration)rand.Int63n((int64)nextSleep));
    }
}

// Setenv calls os.Setenv(key, value) and uses Cleanup to
// restore the environment variable to its original value
// after the test.
//
// Because Setenv affects the whole process, it cannot be used
// in parallel tests or tests with parallel ancestors.
internal static void Setenv(this ж<common> Ꮡc, @string key, @string value) {
    ref var c = ref Ꮡc.Value;

    c.checkFuzzFn("Setenv"u8);
    var (prevValue, ok) = os.LookupEnv(key);
    {
        var err = os.Setenv(key, value); if (err != default!) {
            Ꮡc.Fatalf("cannot set environment variable: %v"u8, err);
        }
    }
    if (ok){
        Ꮡc.Cleanup(() => {
            os.Setenv(key, prevValue);
        });
    } else {
        Ꮡc.Cleanup(() => {
            os.Unsetenv(key);
        });
    }
}

[GoType("num:nint")] partial struct panicHandling;

internal static readonly panicHandling normalPanic = /* iota */ 0;
internal static readonly panicHandling recoverAndReturnPanic = 1;

// runCleanup is called at the end of the test.
// If ph is recoverAndReturnPanic, it will catch panics, and return the
// recovered value if any.
internal static any /*panicVal*/ runCleanup(this ж<common> Ꮡc, panicHandling ph) {
    any panicVal = default!;
    func((defer, recover) => {
    ref var c = ref Ꮡc.Value;

        Ꮡc.of(common.ᏑcleanupStarted).Store(true);
        deferǃ(Ꮡc.of(common.ᏑcleanupStarted).Store, (bool)false, defer);
        if (ph == recoverAndReturnPanic) {
            defer(() => {
                panicVal = recover();
            });
        }
        // Make sure that if a cleanup function panics,
        // we still run the remaining cleanup functions.
        defer(() => {
            Ꮡc.of(common.Ꮡmu).Lock();
            var recur = len(Ꮡc.Value.cleanups) > 0;
            Ꮡc.of(common.Ꮡmu).Unlock();
            if (recur) {
                Ꮡc.runCleanup(normalPanic);
            }
        });
        while (ᐧ) {
            Action cleanup = default!;
            Ꮡc.of(common.Ꮡmu).Lock();
            if (len(c.cleanups) > 0) {
                nint last = len(c.cleanups) - 1;
                cleanup = c.cleanups[last];
                c.cleanups = c.cleanups[..(int)(last)];
            }
            Ꮡc.of(common.Ꮡmu).Unlock();
            if (cleanup == default!) {
                panicVal = default!; return;
            }
            cleanup();
        }
    });
    return panicVal;
}

// resetRaces updates c.parent's count of data race errors (or the global count,
// if c has no parent), and updates c.lastRaceErrors to match.
//
// Any races that occurred prior to this call to resetRaces will
// not be attributed to c.
internal static void resetRaces(this ж<common> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    if (c.parent == nil){
        Ꮡc.of(common.ᏑlastRaceErrors).Store((int64)race.Errors());
    } else {
        Ꮡc.of(common.ᏑlastRaceErrors).Store(c.parent.checkRaces());
    }
}

// checkRaces checks whether the global count of data race errors has increased
// since c's count was last reset.
//
// If so, it marks c as having failed due to those races (logging an error for
// the first such race), and updates the race counts for the parents of c so
// that if they are currently suspended (such as in a call to T.Run) they will
// not log separate errors for the race(s).
//
// Note that multiple tests may be marked as failed due to the same race if they
// are executing in parallel.
internal static int64 /*raceErrors*/ checkRaces(this ж<common> Ꮡc) {
    int64 raceErrors = default!;

    ref var c = ref Ꮡc.Value;
    raceErrors = (int64)race.Errors();
    while (ᐧ) {
        var last = Ꮡc.of(common.ᏑlastRaceErrors).Load();
        if (raceErrors <= last) {
            // All races have already been reported.
            return raceErrors;
        }
        if (Ꮡc.of(common.ᏑlastRaceErrors).CompareAndSwap(last, raceErrors)) {
            break;
        }
    }
    if (Ꮡc.of(common.ᏑraceErrorLogged).CompareAndSwap(false, true)) {
        // This is the first race we've encountered for this test.
        // Mark the test as failed, and log the reason why only once.
        // (Note that the race detector itself will still write a goroutine
        // dump for any further races it detects.)
        Ꮡc.Errorf("race detected during execution of test"u8);
    }
    // Update the parent(s) of this test so that they don't re-report the race.
    var parent = c.parent;
    while (parent != nil) {
        while (ᐧ) {
            var last = parent.of(common.ᏑlastRaceErrors).Load();
            if (raceErrors <= last) {
                // This race was already reported by another (likely parallel) subtest.
                return raceErrors;
            }
            if (parent.of(common.ᏑlastRaceErrors).CompareAndSwap(last, raceErrors)) {
                break;
            }
        }
        parent = parent.Value.parent;
    }
    return raceErrors;
}

// callerName gives the function name (qualified with a package path)
// for the caller after skip frames (where 0 means the current function).
internal static @string callerName(nint skip) {
    array<uintptr> pc = new(1);
    nint n = Δruntime.Callers(skip + 2, pc[..]);
    // skip + runtime.Callers + callerName
    if (n == 0) {
        throw panic("testing: zero callers found");
    }
    return pcToName(pc[0]);
}

internal static @string pcToName(uintptr pc) {
    var pcs = new uintptr[]{pc}.slice();
    var frames = Δruntime.CallersFrames(pcs);
    var (frame, _) = frames.Next();
    return frame.Function;
}

// Parallel signals that this test is to be run in parallel with (and only with)
// other parallel tests. When a test is run multiple times due to use of
// -test.count or -test.cpu, multiple instances of a single test never run in
// parallel with each other.
public static void Parallel(this ж<T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.isParallel) {
        throw panic("testing: t.Parallel called multiple times");
    }
    if (t.isEnvSet) {
        throw panic("testing: t.Parallel called after t.Setenv; cannot set environment variables in parallel tests");
    }
    t.isParallel = true;
    if ((~t.parent).barrier == default!) {
        // T.Parallel has no effect when fuzzing.
        // Multiple processes may run in parallel, but only one input can run at a
        // time per process so we can attribute crashes to specific inputs.
        return;
    }
    // We don't want to include the time we spend waiting for serial tests
    // in the test duration. Record the elapsed time thus far and reset the
    // timer afterwards.
    t.duration += highPrecisionTimeSince(t.start);
    // Add to the list of tests to be released by the parent.
    t.parent.Value.sub = append((~t.parent).sub, Ꮡt);
    // Report any races during execution of this test up to this point.
    //
    // We will assume that any races that occur between here and the point where
    // we unblock are not caused by this subtest. That assumption usually holds,
    // although it can be wrong if the test spawns a goroutine that races in the
    // background while the rest of the test is blocked on the call to Parallel.
    // If that happens, we will misattribute the background race to some other
    // test, or to no test at all — but that false-negative is so unlikely that it
    // is not worth adding race-report noise for the common case where the test is
    // completely suspended during the call to Parallel.
    Ꮡt.of(T.Ꮡcommon).checkRaces();
    if (t.chatty != nil) {
        t.chatty.Updatef(t.name, "=== PAUSE %s\n"u8, t.name);
    }
    Ꮡrunning.Delete(t.name);
    t.signal.ᐸꟷ(true);
    // Release calling test.
    ᐸꟷ((~t.parent).barrier);
    // Wait for the parent test to complete.
    t.context.waitParallel();
    if (t.chatty != nil) {
        t.chatty.Updatef(t.name, "=== CONT  %s\n"u8, t.name);
    }
    Ꮡrunning.Store(t.name, highPrecisionTimeNow());
    t.start = highPrecisionTimeNow();
    // Reset the local race counter to ignore any races that happened while this
    // goroutine was blocked, such as in the parent test or in other parallel
    // subtests.
    //
    // (Note that we don't call parent.checkRaces here:
    // if other parallel subtests have already introduced races, we want to
    // let them report those races instead of attributing them to the parent.)
    Ꮡt.of(T.ᏑlastRaceErrors).Store((int64)race.Errors());
}

// Setenv calls os.Setenv(key, value) and uses Cleanup to
// restore the environment variable to its original value
// after the test.
//
// Because Setenv affects the whole process, it cannot be used
// in parallel tests or tests with parallel ancestors.
public static void Setenv(this ж<T> Ꮡt, @string key, @string value) {
    ref var t = ref Ꮡt.Value;

    // Non-parallel subtests that have parallel ancestors may still
    // run in parallel with other tests: they are only non-parallel
    // with respect to the other subtests of the same parent.
    // Since SetEnv affects the whole process, we need to disallow it
    // if the current test or any parent is parallel.
    var isParallel = false;
    for (var c = Ꮡt.of(T.Ꮡcommon); c != nil; c = c.Value.parent) {
        if ((~c).isParallel) {
            isParallel = true;
            break;
        }
    }
    if (isParallel) {
        throw panic("testing: t.Setenv called after t.Parallel; cannot set environment variables in parallel tests");
    }
    t.isEnvSet = true;
    Ꮡt.of(T.Ꮡcommon).Setenv(key, value);
}

// InternalTest is an internal type but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
[GoType] partial struct InternalTest {
    public @string Name;
    public Action<ж<T>> F;
}

internal static error errNilPanicOrGoexit = errors.New("test executed panic(nil) or runtime.Goexit"u8);

internal static void tRunner(ж<T> Ꮡt, Action<ж<T>> fn) => func((defer, recover) => {
    ref var t = ref Ꮡt.Value;

    t.runner = callerName(0);
    // When this goroutine is done, either because fn(t)
    // returned normally or because a test failure triggered
    // a call to runtime.Goexit, record the duration and send
    // a signal saying that the test is done.
    defer(() => {
        Ꮡt.of(T.Ꮡcommon).checkRaces();
        // TODO(#61034): This is the wrong place for this check.
        if (Ꮡt.of(T.Ꮡcommon).Failed()) {
            ᏑnumFailed.Add(1);
        }
        // Check if the test panicked or Goexited inappropriately.
        //
        // If this happens in a normal test, print output but continue panicking.
        // tRunner is called in its own goroutine, so this terminates the process.
        //
        // If this happens while fuzzing, recover from the panic and treat it like a
        // normal failure. It's important that the process keeps running in order to
        // find short inputs that cause panics.
        ref var err = ref heap<any>(out var Ꮡerr);
        Ꮡerr.ValueSlot = recover();
        var signal = true;
        Ꮡt.of(T.Ꮡmu).RLock();
        var finished = Ꮡt.Value.finished;
        Ꮡt.of(T.Ꮡmu).RUnlock();
        if (!finished && Ꮡerr.ValueSlot == default!) {
            Ꮡerr.ValueSlot = errNilPanicOrGoexit;
            for (var p = Ꮡt.Value.parent; p != nil; p = p.Value.parent) {
                p.of(common.Ꮡmu).RLock();
                finished = p.Value.finished;
                p.of(common.Ꮡmu).RUnlock();
                if (finished) {
                    if (!Ꮡt.Value.isParallel) {
                        Ꮡt.of(T.Ꮡcommon).Errorf("%v: subtest may have called FailNow on a parent test"u8, Ꮡerr.ValueSlot);
                        Ꮡerr.ValueSlot = default!;
                    }
                    signal = false;
                    break;
                }
            }
        }
        if (Ꮡerr.ValueSlot != default! && (~Ꮡt.Value.context).isFuzzing) {
            @string prefix = "panic: "u8;
            if (AreEqual(Ꮡerr.ValueSlot, errNilPanicOrGoexit)) {
                prefix = ""u8;
            }
            Ꮡt.of(T.Ꮡcommon).Errorf("%s%s\n%s\n"u8, prefix, Ꮡerr.ValueSlot, ((@string)debug.Stack()));
            Ꮡt.of(T.Ꮡmu).Lock();
            Ꮡt.Value.finished = true;
            Ꮡt.of(T.Ꮡmu).Unlock();
            Ꮡerr.ValueSlot = default!;
        }
        // Use a deferred call to ensure that we report that the test is
        // complete even if a cleanup function calls t.FailNow. See issue 41355.
        var didPanic = false;
        defer(() => {
            // Only report that the test is complete if it doesn't panic,
            // as otherwise the test binary can exit before the panic is
            // reported to the user. See issue 41479.
            if (didPanic) {
                return;
            }
            if (Ꮡerr.ValueSlot != default!) {
                throw panic(Ꮡerr.ValueSlot);
            }
            Ꮡrunning.Delete(Ꮡt.Value.name);
            Ꮡt.Value.signal.ᐸꟷ(signal);
        });
        var doPanic = (any errΔ1) => {
            Ꮡt.of(T.Ꮡcommon).Fail();
            {
                var r = Ꮡt.of(T.Ꮡcommon).runCleanup(recoverAndReturnPanic); if (r != default!) {
                    Ꮡt.of(T.Ꮡcommon).Logf("cleanup panicked with %v"u8, r);
                }
            }
            // Flush the output log up to the root before dying.
            for (var root = Ꮡt.of(T.Ꮡcommon); (~root).parent != nil; root = root.Value.parent) {
                root.of(common.Ꮡmu).Lock();
                root.Value.duration += highPrecisionTimeSince((~root).start);
                var d = root.Value.duration;
                root.of(common.Ꮡmu).Unlock();
                root.flushToParent((~root).name, "--- FAIL: %s (%s)\n"u8, (~root).name, fmtDuration(d));
                {
                    var r = (~root).parent.runCleanup(recoverAndReturnPanic); if (r != default!) {
                        fmt.Fprintf((~(~root).parent).w, "cleanup panicked with %v"u8, r);
                    }
                }
            }
            didPanic = true;
            throw panic(errΔ1);
        };
        if (Ꮡerr.ValueSlot != default!) {
            doPanic(Ꮡerr.ValueSlot);
        }
        Ꮡt.Value.duration += highPrecisionTimeSince(Ꮡt.Value.start);
        if (len(Ꮡt.Value.sub) > 0){
            // Run parallel subtests.
            // Decrease the running count for this test and mark it as no longer running.
            Ꮡt.Value.context.release();
            Ꮡrunning.Delete(Ꮡt.Value.name);
            // Release the parallel subtests.
            close(Ꮡt.Value.barrier);
            // Wait for subtests to complete.
            foreach (var (_, sub) in Ꮡt.Value.sub) {
                ᐸꟷ((~sub).signal);
            }
            // Run any cleanup callbacks, marking the test as running
            // in case the cleanup hangs.
            ref var cleanupStart = ref heap<highPrecisionTime>(out var ᏑcleanupStart);
            cleanupStart = highPrecisionTimeNow();
            Ꮡrunning.Store(Ꮡt.Value.name, cleanupStart);
            var errΔ2 = Ꮡt.of(T.Ꮡcommon).runCleanup(recoverAndReturnPanic);
            Ꮡt.Value.duration += highPrecisionTimeSince(cleanupStart);
            if (errΔ2 != default!) {
                doPanic(errΔ2);
            }
            Ꮡt.of(T.Ꮡcommon).checkRaces();
            if (!Ꮡt.Value.isParallel) {
                // Reacquire the count for sequential tests. See comment in Run.
                Ꮡt.Value.context.waitParallel();
            }
        } else 
        if (Ꮡt.Value.isParallel) {
            // Only release the count for this test if it was run as a parallel
            // test. See comment in Run method.
            Ꮡt.Value.context.release();
        }
        Ꮡt.report();
        // Report after all subtests have finished.
        // Do not lock t.done to allow race detector to detect race in case
        // the user does not appropriately synchronize a goroutine.
        Ꮡt.Value.done = true;
        if (Ꮡt.Value.parent != nil && !Ꮡt.of(T.ᏑhasSub).Load()) {
            Ꮡt.of(T.Ꮡcommon).setRan();
        }
    });
    defer(() => {
        if (len(Ꮡt.Value.sub) == 0) {
            Ꮡt.of(T.Ꮡcommon).runCleanup(normalPanic);
        }
    });
    t.start = highPrecisionTimeNow();
    Ꮡt.of(T.Ꮡcommon).resetRaces();
    fn(Ꮡt);
    // code beyond here will not be executed when FailNow is invoked
    Ꮡt.of(T.Ꮡmu).Lock();
    t.finished = true;
    Ꮡt.of(T.Ꮡmu).Unlock();
});

// Run runs f as a subtest of t called name. It runs f in a separate goroutine
// and blocks until f returns or calls t.Parallel to become a parallel test.
// Run reports whether f succeeded (or at least did not fail before calling t.Parallel).
//
// Run may be called simultaneously from multiple goroutines, but all such calls
// must return before the outer test function for t returns.
public static bool Run(this ж<T> Ꮡt, @string name, Action<ж<T>> f) {
    ref var t = ref Ꮡt.Value;

    if (Ꮡt.of(T.ᏑcleanupStarted).Load()) {
        throw panic("testing: t.Run called during t.Cleanup");
    }
    Ꮡt.of(T.ᏑhasSub).Store(true);
    var (testName, ok, _) = (~t.context).match.fullName(Ꮡt.of(T.Ꮡcommon), name);
    if (!ok || shouldFailFast()) {
        return true;
    }
    // Record the stack trace at the point of this call so that if the subtest
    // function - which runs in a separate stack - is marked as a helper, we can
    // continue walking the stack into the parent test.
    array<uintptr> pc = new(50); /* maxStackLen */
    nint n = Δruntime.Callers(2, pc[..]);
    Ꮡt = Ꮡ(new T(
        common: new common(
            barrier: new channel<bool>(1),
            signal: new channel<bool>(1),
            name: testName,
            parent: Ꮡt.of(T.Ꮡcommon),
            level: t.level + 1,
            creator: pc[..(int)(n)],
            chatty: t.chatty
        ),
        context: t.context
    )); t = ref Ꮡt.Value;
    t.w = new indenter(Ꮡt.of(T.Ꮡcommon));
    if (t.chatty != nil) {
        t.chatty.Updatef(t.name, "=== RUN   %s\n"u8, t.name);
    }
    Ꮡrunning.Store(t.name, highPrecisionTimeNow());
    // Instead of reducing the running count of this test before calling the
    // tRunner and increasing it afterwards, we rely on tRunner keeping the
    // count correct. This ensures that a sequence of sequential tests runs
    // without being preempted, even when their parent is a parallel test. This
    // may especially reduce surprises if *parallel == 1.
    goǃ(tRunner, Ꮡt, f);
    // The parent goroutine will block until the subtest either finishes or calls
    // Parallel, but in general we don't know whether the parent goroutine is the
    // top-level test function or some other goroutine it has spawned.
    // To avoid confusing false-negatives, we leave the parent in the running map
    // even though in the typical case it is blocked.
    if (!ᐸꟷ(t.signal)) {
        // At this point, it is likely that FailNow was called on one of the
        // parent tests by one of the subtests. Continue aborting up the chain.
        Δruntime.Goexit();
    }
    if (t.chatty != nil && (~t.chatty).json) {
        t.chatty.Updatef((~t.parent).name, "=== NAME  %s\n"u8, (~t.parent).name);
    }
    return !t.failed;
}

// Deadline reports the time at which the test binary will have
// exceeded the timeout specified by the -timeout flag.
//
// The ok result is false if the -timeout flag indicates “no timeout” (0).
[GoRecv] public static (time.Time deadline, bool ok) Deadline(this ref T t) {
    time.Time deadline = default!;
    bool ok = default!;

    deadline = t.context.Value.deadline;
    return (deadline, !deadline.IsZero());
}

// testContext holds all fields that are common to all tests. This includes
// synchronization primitives to run at most *parallel tests.
[GoType] partial struct testContext {
    internal ж<matcher> match;
    internal time.Time deadline;
    // isFuzzing is true in the context used when generating random inputs
    // for fuzz targets. isFuzzing is false when running normal tests and
    // when running fuzz tests as unit tests (without -fuzz or when -fuzz
    // does not match).
    internal bool isFuzzing;
    internal Δsync.Mutex mu;
    // Channel used to signal tests that are ready to be run in parallel.
    internal channel<bool> startParallel;
    // running is the number of tests currently running in parallel.
    // This does not include tests that are waiting for subtests to complete.
    internal nint running;
    // numWaiting is the number tests waiting to be run in parallel.
    internal nint numWaiting;
    // maxParallel is a copy of the parallel flag.
    internal nint maxParallel;
}

internal static ж<testContext> newTestContext(nint maxParallel, ж<matcher> Ꮡm) {
    return Ꮡ(new testContext(
        match: Ꮡm,
        startParallel: new channel<bool>(1),
        maxParallel: maxParallel,
        running: 1
    ));
}

// Set the count to 1 for the main (sequential) test.
internal static void waitParallel(this ж<testContext> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(testContext.Ꮡmu).Lock();
    if (c.running < c.maxParallel) {
        c.running++;
        Ꮡc.of(testContext.Ꮡmu).Unlock();
        return;
    }
    c.numWaiting++;
    Ꮡc.of(testContext.Ꮡmu).Unlock();
    ᐸꟷ(c.startParallel);
}

internal static void release(this ж<testContext> Ꮡc) {
    ref var c = ref Ꮡc.Value;

    Ꮡc.of(testContext.Ꮡmu).Lock();
    if (c.numWaiting == 0) {
        c.running--;
        Ꮡc.of(testContext.Ꮡmu).Unlock();
        return;
    }
    c.numWaiting--;
    Ꮡc.of(testContext.Ꮡmu).Unlock();
    c.startParallel.ᐸꟷ(true);
}

// Pick a waiting test to be run.

// No one should be using func Main anymore.
// See the doc comment on func Main and use MainStart instead.
internal static error errMain = errors.New("testing: unexpected use of func Main"u8);

internal delegate (bool, error) matchStringOnly(@string pat, @string str);

internal static (bool, error) MatchString(this matchStringOnly f, @string pat, @string str) {
    return f(pat, str);
}

internal static error StartCPUProfile(this matchStringOnly f, Δio.Writer w) {
    return errMain;
}

internal static void StopCPUProfile(this matchStringOnly f) {
}

internal static error WriteProfileTo(this matchStringOnly f, @string _Δp1, Δio.Writer _Δp2, nint _Δp3) {
    return errMain;
}

internal static @string ImportPath(this matchStringOnly f) {
    return ""u8;
}

internal static void StartTestLog(this matchStringOnly f, Δio.Writer _) {
}

internal static error StopTestLog(this matchStringOnly f) {
    return errMain;
}

internal static void SetPanicOnExit0(this matchStringOnly f, bool _) {
}

internal static error CoordinateFuzzing(this matchStringOnly f, time.Duration _Δp1, int64 _Δp2, time.Duration _Δp3, int64 _Δp4, nint _Δp5, slice<corpusEntry> _Δp6, slice<reflectꓸType> _Δp7, @string _Δp8, @string _Δp9) {
    return errMain;
}

internal static error RunFuzzWorker(this matchStringOnly f, Func<corpusEntry, error> _) {
    return errMain;
}

internal static (slice<corpusEntry>, error) ReadCorpus(this matchStringOnly f, @string _Δp1, slice<reflectꓸType> _Δp2) {
    return (default!, errMain);
}

internal static error CheckCorpus(this matchStringOnly f, slice<any> _Δp1, slice<reflectꓸType> _Δp2) {
    return default!;
}

internal static void ResetCoverage(this matchStringOnly f) {
}

internal static void SnapshotCoverage(this matchStringOnly f) {
}

internal static (@string mode, Func<@string, @string, (@string, error)> tearDown, Func<float64> snapcov) InitRuntimeCoverage(this matchStringOnly f) {
    @string mode = default!;
    Func<@string, @string, (@string, error)> tearDown = default!;
    Func<float64> snapcov = default!;

    return (mode, tearDown, snapcov);
}

// Main is an internal function, part of the implementation of the "go test" command.
// It was exported because it is cross-package and predates "internal" packages.
// It is no longer used by "go test" but preserved, as much as possible, for other
// systems that simulate "go test" using Main, but Main sometimes cannot be updated as
// new functionality is added to the testing package.
// Systems simulating "go test" should be updated to use MainStart.
public static void ΔMain(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalExample> examples) {
    os.Exit(MainStart(new matchStringOnlyᴠtestDeps(new matchStringOnly(matchString)), tests, benchmarks, default!, examples).Run());
}

// M is a type passed to a TestMain function to run the actual tests.
[GoType] partial struct M {
    internal testDeps deps;
    internal slice<InternalTest> tests;
    internal slice<InternalBenchmark> benchmarks;
    internal slice<InternalFuzzTarget> fuzzTargets;
    internal slice<InternalExample> examples;
    internal ж<time.Timer> timer;
    internal Δsync.Once afterOnce;
    internal nint numRun;
    // value to pass to os.Exit, the outer test func main
    // harness calls os.Exit with this code. See #34129.
    internal nint exitCode;
}

// testDeps is an internal interface of functionality that is
// passed into this package by a test's generated main package.
// The canonical implementation of this interface is
// testing/internal/testdeps's TestDeps.
[GoType] public partial interface testDeps {
    @string ImportPath();
    (bool, error) MatchString(@string pat, @string str);
    void SetPanicOnExit0(bool _);
    error StartCPUProfile(Δio.Writer _);
    void StopCPUProfile();
    void StartTestLog(Δio.Writer _);
    error StopTestLog();
    error WriteProfileTo(@string _Δp0, Δio.Writer _Δp1, nint _Δp2);
    error CoordinateFuzzing(time.Duration _Δp0, int64 _Δp1, time.Duration _Δp2, int64 _Δp3, nint _Δp4, slice<corpusEntry> _Δp5, slice<reflectꓸType> _Δp6, @string _Δp7, @string _Δp8);
    error RunFuzzWorker(Func<corpusEntry, error> _);
    (slice<corpusEntry>, error) ReadCorpus(@string _Δp0, slice<reflectꓸType> _Δp1);
    error CheckCorpus(slice<any> _Δp0, slice<reflectꓸType> _Δp1);
    void ResetCoverage();
    void SnapshotCoverage();
    (@string mode, Func<@string, @string, (@string, error)> tearDown, Func<float64> snapcov) InitRuntimeCoverage();
}

// MainStart is meant for use by tests generated by 'go test'.
// It is not meant to be called directly and is not subject to the Go 1 compatibility document.
// It may change signature from release to release.
public static ж<M> MainStart(testDeps deps, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalFuzzTarget> fuzzTargets, slice<InternalExample> examples) {
    var (ᴛ1, ᴛ2, ᴛ3) = deps.InitRuntimeCoverage();
    registerCover2(ᴛ1, ᴛ2, ᴛ3);
    Init();
    return Ꮡ(new M(
        deps: deps,
        tests: tests,
        benchmarks: benchmarks,
        fuzzTargets: fuzzTargets,
        examples: examples
    ));
}

internal static bool testingTesting;

internal static ж<os.File> realStderr;

// Run runs the tests. It returns an exit code to pass to os.Exit.
public static nint /*code*/ Run(this ж<M> Ꮡm) {
    nint code = default!;
    func((defer, recover) => {
    ref var m = ref Ꮡm.Value;

        defer(() => {
            code = Ꮡm.Value.exitCode;
        });
        // Count the number of calls to m.Run.
        // We only ever expected 1, but we didn't enforce that,
        // and now there are tests in the wild that call m.Run multiple times.
        // Sigh. go.dev/issue/23129.
        m.numRun++;
        // TestMain may have already called flag.Parse.
        if (!flag.Parsed()) {
            flag.Parse();
        }
        if (chatty.json) {
            // With -v=json, stdout and stderr are pointing to the same pipe,
            // which is leading into test2json. In general, operating systems
            // do a good job of ensuring that writes to the same pipe through
            // different file descriptors are delivered whole, so that writing
            // AAA to stdout and BBB to stderr simultaneously produces
            // AAABBB or BBBAAA on the pipe, not something like AABBBA.
            // However, the exception to this is when the pipe fills: in that
            // case, Go's use of non-blocking I/O means that writing AAA
            // or BBB might be split across multiple system calls, making it
            // entirely possible to get output like AABBBA. The same problem
            // happens inside the operating system kernel if we switch to
            // blocking I/O on the pipe. This interleaved output can do things
            // like print unrelated messages in the middle of a TestFoo line,
            // which confuses test2json. Setting os.Stderr = os.Stdout will make
            // them share a single pfd, which will hold a lock for each program
            // write, preventing any interleaving.
            //
            // It might be nice to set Stderr = Stdout always, or perhaps if
            // we can tell they are the same file, but for now -v=json is
            // a very clear signal. Making the two files the same may cause
            // surprises if programs close os.Stdout but expect to be able
            // to continue to write to os.Stderr, but it's hard to see why a
            // test would think it could take over global state that way.
            //
            // This fix only helps programs where the output is coming directly
            // from Go code. It does not help programs in which a subprocess is
            // writing to stderr or stdout at the same time that a Go test is writing output.
            // It also does not help when the output is coming from the runtime,
            // such as when using the print/println functions, since that code writes
            // directly to fd 2 without any locking.
            // We keep realStderr around to prevent fd 2 from being closed.
            //
            // See go.dev/issue/33419.
            realStderr = os.Stderr;
            os.Stderr = os.Stdout;
        }
        if (parallel.Value < 1) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), "testing: -parallel can only be given a positive integer");
            flag.Usage();
            m.exitCode = 2;
            return;
        }
        if (matchFuzz.Value != ""u8 && fuzzCacheDir.Value == ""u8) {
            fmt.Fprintln(new os.FileжWriter(os.Stderr), "testing: -test.fuzzcachedir must be set if -test.fuzz is set");
            flag.Usage();
            m.exitCode = 2;
            return;
        }
        if (matchList.Value != ""u8) {
            listTests(m.deps.MatchString, m.tests, m.benchmarks, m.fuzzTargets, m.examples);
            m.exitCode = 0;
            return;
        }
        if (shuffle.Value != "off"u8) {
            int64 n = default!;
            error err = default!;
            if (shuffle.Value == "on"u8){
                n = time.Now().UnixNano();
            } else {
                (n, err) = strconv.ParseInt(shuffle.Value, 10, 64);
                if (err != default!) {
                    fmt.Fprintln(new os.FileжWriter(os.Stderr), @"testing: -shuffle should be ""off"", ""on"", or a valid integer:", err);
                    m.exitCode = 2;
                    return;
                }
            }
            fmt.Println("-test.shuffle", n);
            var rng = rand.New(rand.NewSource(n));
            rng.Shuffle(len(m.tests), (nint i, nint j) => {
                (Ꮡm.Value.tests[i], Ꮡm.Value.tests[j]) = (Ꮡm.Value.tests[j], Ꮡm.Value.tests[i]);
            });
            rng.Shuffle(len(m.benchmarks), (nint i, nint j) => {
                (Ꮡm.Value.benchmarks[i], Ꮡm.Value.benchmarks[j]) = (Ꮡm.Value.benchmarks[j], Ꮡm.Value.benchmarks[i]);
            });
        }
        parseCpuList();
        m.before();
        defer(Ꮡm.after);
        // Run tests, examples, and benchmarks unless this is a fuzz worker process.
        // Workers start after this is done by their parent process, and they should
        // not repeat this work.
        if (!isFuzzWorker.Value) {
            var deadline = Ꮡm.startAlarm();
            haveExamples = len(m.examples) > 0;
            var (testRan, testOk) = runTests(m.deps.MatchString, m.tests, deadline);
            var (fuzzTargetsRan, fuzzTargetsOk) = runFuzzTests(m.deps, m.fuzzTargets, deadline);
            var (exampleRan, exampleOk) = runExamples(m.deps.MatchString, m.examples);
            m.stopAlarm();
            if (!testRan && !exampleRan && !fuzzTargetsRan && matchBenchmarks.Value == ""u8 && matchFuzz.Value == ""u8) {
                fmt.Fprintln(new os.FileжWriter(os.Stderr), "testing: warning: no tests to run");
                if (testingTesting && match.Value != "^$"u8) {
                    // If this happens during testing of package testing it could be that
                    // package testing's own logic for when to run a test is broken,
                    // in which case every test will run nothing and succeed,
                    // with no obvious way to detect this problem (since no tests are running).
                    // So make 'no tests to run' a hard failure when testing package testing itself.
                    fmt.Print(chatty.prefix(), "FAIL: package testing must run tests\n");
                    testOk = false;
                }
            }
            var anyFailed = !testOk || !exampleOk || !fuzzTargetsOk || !runBenchmarks(m.deps.ImportPath(), m.deps.MatchString, m.benchmarks);
            if (!anyFailed && race.Errors() > 0) {
                fmt.Print(chatty.prefix(), "testing: race detected outside of test execution\n");
                anyFailed = true;
            }
            if (anyFailed) {
                fmt.Print(chatty.prefix(), "FAIL\n");
                m.exitCode = 1;
                return;
            }
        }
        var fuzzingOk = runFuzzing(m.deps, m.fuzzTargets);
        if (!fuzzingOk) {
            fmt.Print(chatty.prefix(), "FAIL\n");
            if (isFuzzWorker.Value){
                m.exitCode = fuzzWorkerExitCode;
            } else {
                m.exitCode = 1;
            }
            return;
        }
        m.exitCode = 0;
        if (!isFuzzWorker.Value) {
            fmt.Print(chatty.prefix(), "PASS\n");
        }
    });
    return code;
}

internal static void report(this ж<T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.parent == nil) {
        return;
    }
    @string dstr = fmtDuration(t.duration);
    @string format = "--- %s: %s (%s)\n"u8;
    if (Ꮡt.of(T.Ꮡcommon).Failed()){
        Ꮡt.of(T.Ꮡcommon).flushToParent(t.name, format, "FAIL", t.name, dstr);
    } else 
    if (t.chatty != nil) {
        if (Ꮡt.of(T.Ꮡcommon).Skipped()){
            Ꮡt.of(T.Ꮡcommon).flushToParent(t.name, format, "SKIP", t.name, dstr);
        } else {
            Ꮡt.of(T.Ꮡcommon).flushToParent(t.name, format, "PASS", t.name, dstr);
        }
    }
}

internal static void listTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalFuzzTarget> fuzzTargets, slice<InternalExample> examples) {
    {
        var (_, err) = matchString(matchList.Value, "non-empty"u8); if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: invalid regexp in -test.list (%q): %s\n"u8, matchList.Value, err);
            os.Exit(1);
        }
    }
    foreach (var (_, test) in tests) {
        {
            var (ok, _) = matchString(matchList.Value, test.Name); if (ok) {
                fmt.Println(test.Name);
            }
        }
    }
    foreach (var (_, bench) in benchmarks) {
        {
            var (ok, _) = matchString(matchList.Value, bench.Name); if (ok) {
                fmt.Println(bench.Name);
            }
        }
    }
    foreach (var (_, fuzzTarget) in fuzzTargets) {
        {
            var (ok, _) = matchString(matchList.Value, fuzzTarget.Name); if (ok) {
                fmt.Println(fuzzTarget.Name);
            }
        }
    }
    foreach (var (_, example) in examples) {
        {
            var (ok, _) = matchString(matchList.Value, example.Name); if (ok) {
                fmt.Println(example.Name);
            }
        }
    }
}

// RunTests is an internal function but exported because it is cross-package;
// it is part of the implementation of the "go test" command.
public static bool /*ok*/ RunTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests) {
    bool ok = default!;

    time.Time deadline = default!;
    if (timeout.Value > 0) {
        deadline = time.Now().Add(timeout.Value);
    }
    (var ran, ok) = runTests(matchString, tests, deadline);
    if (!ran && !haveExamples) {
        fmt.Fprintln(new os.FileжWriter(os.Stderr), "testing: warning: no tests to run");
    }
    return ok;
}

internal static (bool ran, bool ok) runTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests, time.Time deadline) {
    bool ran = default!;
    bool ok = default!;

    ok = true;
    foreach (var (_, procs) in cpuList) {
        Δruntime.GOMAXPROCS(procs);
        for (nuint i = (nuint)0; i < count.Value; i++) {
            if (shouldFailFast()) {
                break;
            }
            if (i > 0 && !ran) {
                // There were no tests to run on the first
                // iteration. This won't change, so no reason
                // to keep trying.
                break;
            }
            var ctx = newTestContext(parallel.Value, newMatcher(matchString, match.Value, "-test.run"u8, skip.Value));
            ctx.Value.deadline = deadline;
            var t = Ꮡ(new T(
                common: new common(
                    signal: new channel<bool>(1),
                    barrier: new channel<bool>(1),
                    w: new os.FileжWriter(os.Stdout)
                ),
                context: ctx
            ));
            if (Verbose()) {
                t.Value.chatty = newChattyPrinter((~t).w);
            }
            var testsʗ1 = tests;
            tRunner(t, (ж<T> tΔ1) => {
                foreach (var (_, vᴛ1) in testsʗ1) {
                    ref var test = ref heap(new InternalTest(), out var Ꮡtest);
                    test = vᴛ1;

                    tΔ1.Run(test.Name, test.F);
                }
            });
            switch (ᐧ) {
            case ᐧ when (~t).signal.ꟷᐳ(out _): {
                break;
            }
            default: {
                throw panic("internal error: tRunner exited without sending on t.signal");
                break;
            }}
            ok = ok && !t.of(T.Ꮡcommon).Failed();
            ran = ran || (~t).ran;
        }
    }
    return (ran, ok);
}

// before runs before all testing.
[GoRecv] internal static void before(this ref M m) {
    if (memProfileRate.Value > 0) {
        Δruntime.MemProfileRate = memProfileRate.Value;
    }
    if (cpuProfile.Value != ""u8) {
        var (f, err) = os.Create(toOutputDir(cpuProfile.Value));
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            return;
        }
        {
            var errΔ1 = m.deps.StartCPUProfile(new os.FileжWriter(f)); if (errΔ1 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't start cpu profile: %s\n"u8, errΔ1);
                f.Close();
                return;
            }
        }
    }
    // Could save f so after can call f.Close; not worth the effort.
    if (traceFile.Value != ""u8) {
        var (f, err) = os.Create(toOutputDir(traceFile.Value));
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            return;
        }
        {
            var errΔ1 = trace.Start(new os.FileжWriter(f)); if (errΔ1 != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't start tracing: %s\n"u8, errΔ1);
                f.Close();
                return;
            }
        }
    }
    // Could save f so after can call f.Close; not worth the effort.
    if (blockProfile.Value != ""u8 && blockProfileRate.Value >= 0) {
        Δruntime.SetBlockProfileRate(blockProfileRate.Value);
    }
    if (mutexProfile.Value != ""u8 && mutexProfileFraction.Value >= 0) {
        Δruntime.SetMutexProfileFraction(mutexProfileFraction.Value);
    }
    if (coverProfile.Value != ""u8 && CoverMode() == ""u8) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: cannot use -test.coverprofile because test binary was not built with coverage enabled\n"u8);
        os.Exit(2);
    }
    if (gocoverdir.Value != ""u8 && CoverMode() == ""u8) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: cannot use -test.gocoverdir because test binary was not built with coverage enabled\n"u8);
        os.Exit(2);
    }
    if (testlog.Value != ""u8) {
        // Note: Not using toOutputDir.
        // This file is for use by cmd/go, not users.
        ж<os.File> f = default!;
        error err = default!;
        if (m.numRun == 1){
            (f, err) = os.Create(testlog.Value);
        } else {
            (f, err) = os.OpenFile(testlog.Value, os.O_WRONLY, 0);
            if (err == default!) {
                f.Seek(0, Δio.SeekEnd);
            }
        }
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            os.Exit(2);
        }
        m.deps.StartTestLog(new os.FileжWriter(f));
        testlogFile = f;
    }
    if (panicOnExit0.Value) {
        m.deps.SetPanicOnExit0(true);
    }
}

// after runs after all testing.
internal static void after(this ж<M> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    Ꮡm.of(M.ᏑafterOnce).Do(() => {
        Ꮡm.Value.writeProfiles();
    });
    // Restore PanicOnExit0 after every run, because we set it to true before
    // every run. Otherwise, if m.Run is called multiple times the behavior of
    // os.Exit(0) will not be restored after the second run.
    if (panicOnExit0.Value) {
        m.deps.SetPanicOnExit0(false);
    }
}

[GoRecv] internal static void writeProfiles(this ref M m) {
    if (testlog.Value != ""u8) {
        {
            var err = m.deps.StopTestLog(); if (err != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't write %s: %s\n"u8, testlog.Value, err);
                os.Exit(2);
            }
        }
        {
            var err = testlogFile.Close(); if (err != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't write %s: %s\n"u8, testlog.Value, err);
                os.Exit(2);
            }
        }
    }
    if (cpuProfile.Value != ""u8) {
        m.deps.StopCPUProfile();
    }
    // flushes profile to disk
    if (traceFile.Value != ""u8) {
        trace.Stop();
    }
    // flushes trace to disk
    if (memProfile.Value != ""u8) {
        var (f, err) = os.Create(toOutputDir(memProfile.Value));
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            os.Exit(2);
        }
        Δruntime.GC();
        // materialize all statistics
        {
            err = m.deps.WriteProfileTo("allocs"u8, new os.FileжWriter(f), 0); if (err != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't write %s: %s\n"u8, memProfile.Value, err);
                os.Exit(2);
            }
        }
        f.Close();
    }
    if (blockProfile.Value != ""u8 && blockProfileRate.Value >= 0) {
        var (f, err) = os.Create(toOutputDir(blockProfile.Value));
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            os.Exit(2);
        }
        {
            err = m.deps.WriteProfileTo("block"u8, new os.FileжWriter(f), 0); if (err != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't write %s: %s\n"u8, blockProfile.Value, err);
                os.Exit(2);
            }
        }
        f.Close();
    }
    if (mutexProfile.Value != ""u8 && mutexProfileFraction.Value >= 0) {
        var (f, err) = os.Create(toOutputDir(mutexProfile.Value));
        if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: %s\n"u8, err);
            os.Exit(2);
        }
        {
            err = m.deps.WriteProfileTo("mutex"u8, new os.FileжWriter(f), 0); if (err != default!) {
                fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: can't write %s: %s\n"u8, mutexProfile.Value, err);
                os.Exit(2);
            }
        }
        f.Close();
    }
    if (CoverMode() != ""u8) {
        coverReport();
    }
}

// toOutputDir returns the file name relocated, if required, to outputDir.
// Simple implementation to avoid pulling in path/filepath.
internal static @string toOutputDir(@string path) {
    if (outputDir.Value == ""u8 || path == ""u8) {
        return path;
    }
    // On Windows, it's clumsy, but we can be almost always correct
    // by just looking for a drive letter and a colon.
    // Absolute paths always have a drive letter (ignoring UNC).
    // Problem: if path == "C:A" and outputdir == "C:\Go" it's unclear
    // what to do, but even then path/filepath doesn't help.
    // TODO: Worth doing better? Probably not, because we're here only
    // under the management of go test.
    if (Δruntime.GOOS == "windows"u8 && len(path) >= 2) {
        var (letter, colon) = (path[0], path[1]);
        if (((rune)'a' <= letter && letter <= (rune)'z' || (rune)'A' <= letter && letter <= (rune)'Z') && colon == (rune)':') {
            // If path starts with a drive letter we're stuck with it regardless.
            return path;
        }
    }
    if (os.IsPathSeparator(path[0])) {
        return path;
    }
    return fmt.Sprintf("%s%c%s"u8, outputDir.Value, os.PathSeparator, path);
}

// startAlarm starts an alarm if requested.
internal static time.Time startAlarm(this ж<M> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    if (timeout.Value <= 0) {
        return new time.Time(nil);
    }
    var deadline = time.Now().Add(timeout.Value);
    m.timer = time.AfterFunc(timeout.Value, () => {
        Ꮡm.after();
        debug.SetTraceback("all"u8);
        @string extra = ""u8;
        {
            var list = runningList(); if (len(list) > 0) {
                ref var b = ref heap(new strings.Builder(), out var Ꮡb);
                Ꮡb.WriteString("\nrunning tests:"u8);
                foreach (var (_, name) in list) {
                    Ꮡb.WriteString("\n\t"u8);
                    Ꮡb.WriteString(name);
                }
                extra = b.String();
            }
        }
        throw panic(fmt.Sprintf("test timed out after %v%s"u8, timeout.Value, extra));
    });
    return deadline;
}

// runningList returns the list of running tests.
internal static slice<@string> runningList() {
    ref var list = ref heap<slice<@string>>(out var Ꮡlist);
    Ꮡrunning.Range((any k, any v) => {
        Ꮡlist.ValueSlot = append(Ꮡlist.ValueSlot, fmt.Sprintf("%s (%v)"u8, k._<@string>(), highPrecisionTimeSince(v._<highPrecisionTime>()).Round(time.ΔSecond)));
        return true;
    });
    slices.Sort<slice<@string>, @string>(list);
    return list;
}

// stopAlarm turns off the alarm.
[GoRecv] internal static void stopAlarm(this ref M m) {
    if (timeout.Value > 0) {
        m.timer.Stop();
    }
}

internal static void parseCpuList() {
    foreach (var (_, vᴛ1) in strings.Split(cpuListStr.Value, ","u8)) {
        var val = vᴛ1;

        val = strings.TrimSpace(val);
        if (val == ""u8) {
            continue;
        }
        var (cpu, err) = strconv.Atoi(val);
        if (err != default! || cpu <= 0) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "testing: invalid value %q for -test.cpu\n"u8, val);
            os.Exit(1);
        }
        cpuList = append(cpuList, cpu);
    }
    if (cpuList == default!) {
        cpuList = append(cpuList, Δruntime.GOMAXPROCS(-1));
    }
}

internal static bool shouldFailFast() {
    return failFast.Value && ᏑnumFailed.Load() > 0;
}

} // end testing_package
