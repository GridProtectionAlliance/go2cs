// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testing provides support for automated testing of Go packages.
// It is intended to be used in concert with the ``go test'' command, which automates
// execution of any function of the form
//     func TestXxx(*testing.T)
// where Xxx does not start with a lowercase letter. The function name
// serves to identify the test routine.
//
// Within these functions, use the Error, Fail or related methods to signal failure.
//
// To write a new test suite, create a file whose name ends _test.go that
// contains the TestXxx functions as described here. Put the file in the same
// package as the one being tested. The file will be excluded from regular
// package builds but will be included when the ``go test'' command is run.
// For more detail, run ``go help test'' and ``go help testflag''.
//
// Tests and benchmarks may be skipped if not applicable with a call to
// the Skip method of *T and *B:
//     func TestTimeConsuming(t *testing.T) {
//         if testing.Short() {
//             t.Skip("skipping test in short mode.")
//         }
//         ...
//     }
//
// Benchmarks
//
// Functions of the form
//     func BenchmarkXxx(*testing.B)
// are considered benchmarks, and are executed by the "go test" command when
// its -bench flag is provided. Benchmarks are run sequentially.
//
// For a description of the testing flags, see
// https://golang.org/cmd/go/#hdr-Description_of_testing_flags.
//
// A sample benchmark function looks like this:
//     func BenchmarkHello(b *testing.B) {
//         for i := 0; i < b.N; i++ {
//             fmt.Sprintf("hello")
//         }
//     }
//
// The benchmark function must run the target code b.N times.
// During benchmark execution, b.N is adjusted until the benchmark function lasts
// long enough to be timed reliably. The output
//     BenchmarkHello    10000000    282 ns/op
// means that the loop ran 10000000 times at a speed of 282 ns per loop.
//
// If a benchmark needs some expensive setup before running, the timer
// may be reset:
//
//     func BenchmarkBigLen(b *testing.B) {
//         big := NewBig()
//         b.ResetTimer()
//         for i := 0; i < b.N; i++ {
//             big.Len()
//         }
//     }
//
// If a benchmark needs to test performance in a parallel setting, it may use
// the RunParallel helper function; such benchmarks are intended to be used with
// the go test -cpu flag:
//
//     func BenchmarkTemplateParallel(b *testing.B) {
//         templ := template.Must(template.New("test").Parse("Hello, {{.}}!"))
//         b.RunParallel(func(pb *testing.PB) {
//             var buf bytes.Buffer
//             for pb.Next() {
//                 buf.Reset()
//                 templ.Execute(&buf, "World")
//             }
//         })
//     }
//
// Examples
//
// The package also runs and verifies example code. Example functions may
// include a concluding line comment that begins with "Output:" and is compared with
// the standard output of the function when the tests are run. (The comparison
// ignores leading and trailing space.) These are examples of an example:
//
//     func ExampleHello() {
//         fmt.Println("hello")
//         // Output: hello
//     }
//
//     func ExampleSalutations() {
//         fmt.Println("hello, and")
//         fmt.Println("goodbye")
//         // Output:
//         // hello, and
//         // goodbye
//     }
//
// The comment prefix "Unordered output:" is like "Output:", but matches any
// line order:
//
//     func ExamplePerm() {
//         for _, value := range Perm(4) {
//             fmt.Println(value)
//         }
//         // Unordered output: 4
//         // 2
//         // 1
//         // 3
//         // 0
//     }
//
// Example functions without output comments are compiled but not executed.
//
// The naming convention to declare examples for the package, a function F, a type T and
// method M on type T are:
//
//     func Example() { ... }
//     func ExampleF() { ... }
//     func ExampleT() { ... }
//     func ExampleT_M() { ... }
//
// Multiple example functions for a package/type/function/method may be provided by
// appending a distinct suffix to the name. The suffix must start with a
// lower-case letter.
//
//     func Example_suffix() { ... }
//     func ExampleF_suffix() { ... }
//     func ExampleT_suffix() { ... }
//     func ExampleT_M_suffix() { ... }
//
// The entire test file is presented as the example when it contains a single
// example function, at least one other function, type, variable, or constant
// declaration, and no test or benchmark functions.
//
// Subtests and Sub-benchmarks
//
// The Run methods of T and B allow defining subtests and sub-benchmarks,
// without having to define separate functions for each. This enables uses
// like table-driven benchmarks and creating hierarchical tests.
// It also provides a way to share common setup and tear-down code:
//
//     func TestFoo(t *testing.T) {
//         // <setup code>
//         t.Run("A=1", func(t *testing.T) { ... })
//         t.Run("A=2", func(t *testing.T) { ... })
//         t.Run("B=1", func(t *testing.T) { ... })
//         // <tear-down code>
//     }
//
// Each subtest and sub-benchmark has a unique name: the combination of the name
// of the top-level test and the sequence of names passed to Run, separated by
// slashes, with an optional trailing sequence number for disambiguation.
//
// The argument to the -run and -bench command-line flags is an unanchored regular
// expression that matches the test's name. For tests with multiple slash-separated
// elements, such as subtests, the argument is itself slash-separated, with
// expressions matching each name element in turn. Because it is unanchored, an
// empty expression matches any string.
// For example, using "matching" to mean "whose name contains":
//
//     go test -run ''      # Run all tests.
//     go test -run Foo     # Run top-level tests matching "Foo", such as "TestFooBar".
//     go test -run Foo/A=  # For top-level tests matching "Foo", run subtests matching "A=".
//     go test -run /A=1    # For all top-level tests, run subtests matching "A=1".
//
// Subtests can also be used to control parallelism. A parent test will only
// complete once all of its subtests complete. In this example, all tests are
// run in parallel with each other, and only with each other, regardless of
// other top-level tests that may be defined:
//
//     func TestGroupedParallel(t *testing.T) {
//         for _, tc := range tests {
//             tc := tc // capture range variable
//             t.Run(tc.Name, func(t *testing.T) {
//                 t.Parallel()
//                 ...
//             })
//         }
//     }
//
// Run does not return until parallel subtests have completed, providing a way
// to clean up after a group of parallel tests:
//
//     func TestTeardownParallel(t *testing.T) {
//         // This Run will not return until the parallel tests finish.
//         t.Run("group", func(t *testing.T) {
//             t.Run("Test1", parallelTest1)
//             t.Run("Test2", parallelTest2)
//             t.Run("Test3", parallelTest3)
//         })
//         // <tear-down code>
//     }
//
// Main
//
// It is sometimes necessary for a test program to do extra setup or teardown
// before or after testing. It is also sometimes necessary for a test to control
// which code runs on the main thread. To support these and other cases,
// if a test file contains a function:
//
//    func TestMain(m *testing.M)
//
// then the generated test will call TestMain(m) instead of running the tests
// directly. TestMain runs in the main goroutine and can do whatever setup
// and teardown is necessary around a call to m.Run. It should then call
// os.Exit with the result of m.Run. When TestMain is called, flag.Parse has
// not been run. If TestMain depends on command-line flags, including those
// of the testing package, it should call flag.Parse explicitly.
//
// A simple implementation of TestMain is:
//
//    func TestMain(m *testing.M) {
//        // call flag.Parse() here if TestMain uses flags
//        os.Exit(m.Run())
//    }
//
// package testing -- go2cs converted at 2020 August 29 10:05:56 UTC
// import "testing" ==> using testing = go.testing_package
// Original source: C:\Go\src\testing\testing.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using race = go.@internal.race_package;
using io = go.io_package;
using os = go.os_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using trace = go.runtime.trace_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
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
 
        // The short flag requests that tests run more quickly, but its functionality
        // is provided by test writers themselves. The testing package is just its
        // home. The all.bash installation script sets it to make installation more
        // efficient, but by default the flag is off so a plain "go test" will do a
        // full test of the package.
        private static var @short = flag.Bool("test.short", false, "run smaller test suite to save time");        private static var failFast = flag.Bool("test.failfast", false, "do not start new tests after the first test failure");        private static var outputDir = flag.String("test.outputdir", "", "write profiles to `dir`");        private static var chatty = flag.Bool("test.v", false, "verbose: print additional output");        private static var count = flag.Uint("test.count", 1L, "run tests and benchmarks `n` times");        private static var coverProfile = flag.String("test.coverprofile", "", "write a coverage profile to `file`");        private static var matchList = flag.String("test.list", "", "list tests, examples, and benchmarks matching `regexp` then exit");        private static var match = flag.String("test.run", "", "run only tests and examples matching `regexp`");        private static var memProfile = flag.String("test.memprofile", "", "write a memory profile to `file`");        private static var memProfileRate = flag.Int("test.memprofilerate", 0L, "set memory profiling `rate` (see runtime.MemProfileRate)");        private static var cpuProfile = flag.String("test.cpuprofile", "", "write a cpu profile to `file`");        private static var blockProfile = flag.String("test.blockprofile", "", "write a goroutine blocking profile to `file`");        private static var blockProfileRate = flag.Int("test.blockprofilerate", 1L, "set blocking profile `rate` (see runtime.SetBlockProfileRate)");        private static var mutexProfile = flag.String("test.mutexprofile", "", "write a mutex contention profile to the named file after execution");        private static var mutexProfileFraction = flag.Int("test.mutexprofilefraction", 1L, "if >= 0, calls runtime.SetMutexProfileFraction()");        private static var traceFile = flag.String("test.trace", "", "write an execution trace to `file`");        private static var timeout = flag.Duration("test.timeout", 0L, "panic test binary after duration `d` (default 0, timeout disabled)");        private static var cpuListStr = flag.String("test.cpu", "", "comma-separated `list` of cpu counts to run each test with");        private static var parallel = flag.Int("test.parallel", runtime.GOMAXPROCS(0L), "run at most `n` tests in parallel");        private static var testlog = flag.String("test.testlogfile", "", "write test action log to `file` (for use only by cmd/go)");        private static bool haveExamples = default;        private static slice<long> cpuList = default;        private static ref os.File testlogFile = default;        private static uint numFailed = default;

        // common holds the elements common between T and B and
        // captures common methods such as Errorf.
        private partial struct common
        {
            public sync.RWMutex mu; // guards this group of fields
            public slice<byte> output; // Output generated by test or benchmark.
            public io.Writer w; // For flushToParent.
            public bool ran; // Test or benchmark (or one of its subtests) was executed.
            public bool failed; // Test or benchmark has failed.
            public bool skipped; // Test of benchmark has been skipped.
            public bool done; // Test is finished and all subtests have completed.
            public bool chatty; // A copy of the chatty flag.
            public bool finished; // Test function has completed.
            public int hasSub; // written atomically
            public long raceErrors; // number of races detected during test
            public @string runner; // function name of tRunner running the test

            public ptr<common> parent;
            public long level; // Nesting depth of test or benchmark.
            public @string name; // Name of test or benchmark.
            public time.Time start; // Time test or benchmark started
            public time.Duration duration;
            public channel<bool> barrier; // To signal parallel subtests they may start.
            public channel<bool> signal; // To signal a test is done.
            public slice<ref T> sub; // Queue of subtests to be run in parallel.
        }

        // Short reports whether the -test.short flag is set.
        public static bool Short()
        {
            return short.Value;
        }

        // CoverMode reports what the test coverage mode is set to. The
        // values are "set", "count", or "atomic". The return value will be
        // empty if test coverage is not enabled.
        public static @string CoverMode()
        {
            return cover.Mode;
        }

        // Verbose reports whether the -test.v flag is set.
        public static bool Verbose()
        {
            return chatty.Value;
        }

        // frameSkip searches, starting after skip frames, for the first caller frame
        // in a function not marked as a helper and returns the frames to skip
        // to reach that site. The search stops if it finds a tRunner function that
        // was the entry point into the test.
        // This function must be called with c.mu held.
        private static long frameSkip(this ref common _c, long skip) => func(_c, (ref common c, Defer _, Panic panic, Recover __) =>
        {
            if (c.helpers == null)
            {
                return skip;
            }
            array<System.UIntPtr> pc = new array<System.UIntPtr>(50L); 
            // Skip two extra frames to account for this function
            // and runtime.Callers itself.
            var n = runtime.Callers(skip + 2L, pc[..]);
            if (n == 0L)
            {
                panic("testing: zero callers found");
            }
            var frames = runtime.CallersFrames(pc[..n]);
            runtime.Frame frame = default;
            var more = true;
            for (long i = 0L; more; i++)
            {
                frame, more = frames.Next();
                if (frame.Function == c.runner)
                { 
                    // We've gone up all the way to the tRunner calling
                    // the test function (so the user must have
                    // called tb.Helper from inside that test function).
                    // Only skip up to the test function itself.
                    return skip + i - 1L;
                }
                {
                    var (_, ok) = c.helpers[frame.Function];

                    if (!ok)
                    { 
                        // Found a frame that wasn't inside a helper function.
                        return skip + i;
                    }

                }
            }

            return skip;
        });

        // decorate prefixes the string with the file and line of the call site
        // and inserts the final newline if needed and indentation tabs for formatting.
        // This function must be called with c.mu held.
        private static @string decorate(this ref common c, @string s)
        {
            var skip = c.frameSkip(3L); // decorate + log + public function.
            var (_, file, line, ok) = runtime.Caller(skip);
            if (ok)
            { 
                // Truncate file name at last file name separator.
                {
                    var index = strings.LastIndex(file, "/");

                    if (index >= 0L)
                    {
                        file = file[index + 1L..];
                    }                    index = strings.LastIndex(file, "\\");


                    else if (index >= 0L)
                    {
                        file = file[index + 1L..];
                    }

                }
            }
            else
            {
                file = "???";
                line = 1L;
            }
            ptr<object> buf = @new<bytes.Buffer>(); 
            // Every line is indented at least one tab.
            buf.WriteByte('\t');
            fmt.Fprintf(buf, "%s:%d: ", file, line);
            var lines = strings.Split(s, "\n");
            {
                var l = len(lines);

                if (l > 1L && lines[l - 1L] == "")
                {
                    lines = lines[..l - 1L];
                }

            }
            foreach (var (i, line) in lines)
            {
                if (i > 0L)
                { 
                    // Second and subsequent lines are indented an extra tab.
                    buf.WriteString("\n\t\t");
                }
                buf.WriteString(line);
            }
            buf.WriteByte('\n');
            return buf.String();
        }

        // flushToParent writes c.output to the parent after first writing the header
        // with the given format and arguments.
        private static void flushToParent(this ref common _c, @string format, params object[] args) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            var p = c.parent;
            p.mu.Lock();
            defer(p.mu.Unlock());

            fmt.Fprintf(p.w, format, args);

            c.mu.Lock();
            defer(c.mu.Unlock());
            io.Copy(p.w, bytes.NewReader(c.output));
            c.output = c.output[..0L];
        });

        private partial struct indenter
        {
            public ptr<common> c;
        }

        private static (long, error) Write(this indenter w, slice<byte> b)
        {
            n = len(b);
            while (len(b) > 0L)
            {
                var end = bytes.IndexByte(b, '\n');
                if (end == -1L)
                {
                    end = len(b);
                }
                else
                {
                    end++;
                } 
                // An indent of 4 spaces will neatly align the dashes with the status
                // indicator of the parent.
                const @string indent = "    ";

                w.c.output = append(w.c.output, indent);
                w.c.output = append(w.c.output, b[..end]);
                b = b[end..];
            }

            return;
        }

        // fmtDuration returns a string representing d in the form "87.00s".
        private static @string fmtDuration(time.Duration d)
        {
            return fmt.Sprintf("%.2fs", d.Seconds());
        }

        // TB is the interface common to T and B.
        public partial interface TB
        {
            bool Error(params object[] args);
            bool Errorf(@string format, params object[] args);
            bool Fail();
            bool FailNow();
            bool Failed();
            bool Fatal(params object[] args);
            bool Fatalf(@string format, params object[] args);
            bool Log(params object[] args);
            bool Logf(@string format, params object[] args);
            bool Name();
            bool Skip(params object[] args);
            bool SkipNow();
            bool Skipf(@string format, params object[] args);
            bool Skipped();
            bool Helper(); // A private method to prevent users implementing the
// interface and so future additions to it will not
// violate Go 1 compatibility.
            bool @private();
        }

        private static TB _ = TB.As((T.Value)(null));
        private static TB _ = TB.As((B.Value)(null));

        // T is a type passed to Test functions to manage test state and support formatted test logs.
        // Logs are accumulated during execution and dumped to standard output when done.
        //
        // A test ends when its Test function returns or calls any of the methods
        // FailNow, Fatal, Fatalf, SkipNow, Skip, or Skipf. Those methods, as well as
        // the Parallel method, must be called only from the goroutine running the
        // Test function.
        //
        // The other reporting methods, such as the variations of Log and Error,
        // may be called simultaneously from multiple goroutines.
        public partial struct T
        {
            public ref common common => ref common_val;
            public bool isParallel;
            public ptr<testContext> context; // For running tests and subtests.
        }

        private static void @private(this ref common c)
        {
        }

        // Name returns the name of the running test or benchmark.
        private static @string Name(this ref common c)
        {
            return c.name;
        }

        private static void setRan(this ref common _c) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            if (c.parent != null)
            {
                c.parent.setRan();
            }
            c.mu.Lock();
            defer(c.mu.Unlock());
            c.ran = true;
        });

        // Fail marks the function as having failed but continues execution.
        private static void Fail(this ref common _c) => func(_c, (ref common c, Defer defer, Panic panic, Recover _) =>
        {
            if (c.parent != null)
            {
                c.parent.Fail();
            }
            c.mu.Lock();
            defer(c.mu.Unlock()); 
            // c.done needs to be locked to synchronize checks to c.done in parent tests.
            if (c.done)
            {
                panic("Fail in goroutine after " + c.name + " has completed");
            }
            c.failed = true;
        });

        // Failed reports whether the function has failed.
        private static bool Failed(this ref common c)
        {
            c.mu.RLock();
            var failed = c.failed;
            c.mu.RUnlock();
            return failed || c.raceErrors + race.Errors() > 0L;
        }

        // FailNow marks the function as having failed and stops its execution
        // by calling runtime.Goexit (which then runs all deferred calls in the
        // current goroutine).
        // Execution will continue at the next test or benchmark.
        // FailNow must be called from the goroutine running the
        // test or benchmark function, not from other goroutines
        // created during the test. Calling FailNow does not stop
        // those other goroutines.
        private static void FailNow(this ref common c)
        {
            c.Fail(); 

            // Calling runtime.Goexit will exit the goroutine, which
            // will run the deferred functions in this goroutine,
            // which will eventually run the deferred lines in tRunner,
            // which will signal to the test loop that this test is done.
            //
            // A previous version of this code said:
            //
            //    c.duration = ...
            //    c.signal <- c.self
            //    runtime.Goexit()
            //
            // This previous version duplicated code (those lines are in
            // tRunner no matter what), but worse the goroutine teardown
            // implicit in runtime.Goexit was not guaranteed to complete
            // before the test exited. If a test deferred an important cleanup
            // function (like removing temporary files), there was no guarantee
            // it would run on a test failure. Because we send on c.signal during
            // a top-of-stack deferred function now, we know that the send
            // only happens after any other stacked defers have completed.
            c.finished = true;
            runtime.Goexit();
        }

        // log generates the output. It's always at the same stack depth.
        private static void log(this ref common _c, @string s) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            c.mu.Lock();
            defer(c.mu.Unlock());
            c.output = append(c.output, c.decorate(s));
        });

        // Log formats its arguments using default formatting, analogous to Println,
        // and records the text in the error log. For tests, the text will be printed only if
        // the test fails or the -test.v flag is set. For benchmarks, the text is always
        // printed to avoid having performance depend on the value of the -test.v flag.
        private static void Log(this ref common c, params object[] args)
        {
            c.log(fmt.Sprintln(args));

        }

        // Logf formats its arguments according to the format, analogous to Printf, and
        // records the text in the error log. A final newline is added if not provided. For
        // tests, the text will be printed only if the test fails or the -test.v flag is
        // set. For benchmarks, the text is always printed to avoid having performance
        // depend on the value of the -test.v flag.
        private static void Logf(this ref common c, @string format, params object[] args)
        {
            c.log(fmt.Sprintf(format, args));

        }

        // Error is equivalent to Log followed by Fail.
        private static void Error(this ref common c, params object[] args)
        {
            c.log(fmt.Sprintln(args));
            c.Fail();
        }

        // Errorf is equivalent to Logf followed by Fail.
        private static void Errorf(this ref common c, @string format, params object[] args)
        {
            c.log(fmt.Sprintf(format, args));
            c.Fail();
        }

        // Fatal is equivalent to Log followed by FailNow.
        private static void Fatal(this ref common c, params object[] args)
        {
            c.log(fmt.Sprintln(args));
            c.FailNow();
        }

        // Fatalf is equivalent to Logf followed by FailNow.
        private static void Fatalf(this ref common c, @string format, params object[] args)
        {
            c.log(fmt.Sprintf(format, args));
            c.FailNow();
        }

        // Skip is equivalent to Log followed by SkipNow.
        private static void Skip(this ref common c, params object[] args)
        {
            c.log(fmt.Sprintln(args));
            c.SkipNow();
        }

        // Skipf is equivalent to Logf followed by SkipNow.
        private static void Skipf(this ref common c, @string format, params object[] args)
        {
            c.log(fmt.Sprintf(format, args));
            c.SkipNow();
        }

        // SkipNow marks the test as having been skipped and stops its execution
        // by calling runtime.Goexit.
        // If a test fails (see Error, Errorf, Fail) and is then skipped,
        // it is still considered to have failed.
        // Execution will continue at the next test or benchmark. See also FailNow.
        // SkipNow must be called from the goroutine running the test, not from
        // other goroutines created during the test. Calling SkipNow does not stop
        // those other goroutines.
        private static void SkipNow(this ref common c)
        {
            c.skip();
            c.finished = true;
            runtime.Goexit();
        }

        private static void skip(this ref common _c) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            c.mu.Lock();
            defer(c.mu.Unlock());
            c.skipped = true;
        });

        // Skipped reports whether the test was skipped.
        private static bool Skipped(this ref common _c) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            c.mu.RLock();
            defer(c.mu.RUnlock());
            return c.skipped;
        });

        // Helper marks the calling function as a test helper function.
        // When printing file and line information, that function will be skipped.
        // Helper may be called simultaneously from multiple goroutines.
        // Helper has no effect if it is called directly from a TestXxx/BenchmarkXxx
        // function or a subtest/sub-benchmark function.
        private static void Helper(this ref common _c) => func(_c, (ref common c, Defer defer, Panic _, Recover __) =>
        {
            c.mu.Lock();
            defer(c.mu.Unlock());
            if (c.helpers == null)
            {
                c.helpers = make();
            }
            c.helpers[callerName(1L)] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        });

        // callerName gives the function name (qualified with a package path)
        // for the caller after skip frames (where 0 means the current function).
        private static @string callerName(long skip) => func((_, panic, __) =>
        { 
            // Make room for the skip PC.
            array<System.UIntPtr> pc = new array<System.UIntPtr>(2L);
            var n = runtime.Callers(skip + 2L, pc[..]); // skip + runtime.Callers + callerName
            if (n == 0L)
            {
                panic("testing: zero callers found");
            }
            var frames = runtime.CallersFrames(pc[..n]);
            var (frame, _) = frames.Next();
            return frame.Function;
        });

        // Parallel signals that this test is to be run in parallel with (and only with)
        // other parallel tests. When a test is run multiple times due to use of
        // -test.count or -test.cpu, multiple instances of a single test never run in
        // parallel with each other.
        private static void Parallel(this ref T _t) => func(_t, (ref T t, Defer _, Panic panic, Recover __) =>
        {
            if (t.isParallel)
            {
                panic("testing: t.Parallel called multiple times");
            }
            t.isParallel = true; 

            // We don't want to include the time we spend waiting for serial tests
            // in the test duration. Record the elapsed time thus far and reset the
            // timer afterwards.
            t.duration += time.Since(t.start); 

            // Add to the list of tests to be released by the parent.
            t.parent.sub = append(t.parent.sub, t);
            t.raceErrors += race.Errors();

            if (t.chatty)
            { 
                // Print directly to root's io.Writer so there is no delay.
                var root = t.parent;
                while (root.parent != null)
                {
                    root = root.parent;
                }

                root.mu.Lock();
                fmt.Fprintf(root.w, "=== PAUSE %s\n", t.name);
                root.mu.Unlock();
            }
            t.signal.Send(true); // Release calling test.
            t.parent.barrier.Receive(); // Wait for the parent test to complete.
            t.context.waitParallel();

            if (t.chatty)
            { 
                // Print directly to root's io.Writer so there is no delay.
                root = t.parent;
                while (root.parent != null)
                {
                    root = root.parent;
                }

                root.mu.Lock();
                fmt.Fprintf(root.w, "=== CONT  %s\n", t.name);
                root.mu.Unlock();
            }
            t.start = time.Now();
            t.raceErrors += -race.Errors();
        });

        // An internal type but exported because it is cross-package; part of the implementation
        // of the "go test" command.
        public partial struct InternalTest
        {
            public @string Name;
            public Action<ref T> F;
        }

        private static void tRunner(ref T _t, Action<ref T> fn) => func(_t, (ref T t, Defer defer, Panic panic, Recover _) =>
        {
            t.runner = callerName(0L); 

            // When this goroutine is done, either because fn(t)
            // returned normally or because a test failure triggered
            // a call to runtime.Goexit, record the duration and send
            // a signal saying that the test is done.
            defer(() =>
            {
                if (t.raceErrors + race.Errors() > 0L)
                {
                    t.Errorf("race detected during execution of test");
                }
                t.duration += time.Since(t.start); 
                // If the test panicked, print any test output before dying.
                var err = recover();
                if (!t.finished && err == null)
                {
                    err = fmt.Errorf("test executed panic(nil) or runtime.Goexit");
                }
                if (err != null)
                {
                    t.Fail();
                    t.report();
                    panic(err);
                }
                if (len(t.sub) > 0L)
                { 
                    // Run parallel subtests.
                    // Decrease the running count for this test.
                    t.context.release(); 
                    // Release the parallel subtests.
                    close(t.barrier); 
                    // Wait for subtests to complete.
                    foreach (var (_, sub) in t.sub)
                    {
                        sub.signal.Receive();
                    }
                    if (!t.isParallel)
                    { 
                        // Reacquire the count for sequential tests. See comment in Run.
                        t.context.waitParallel();
                    }
                }
                else if (t.isParallel)
                { 
                    // Only release the count for this test if it was run as a parallel
                    // test. See comment in Run method.
                    t.context.release();
                }
                t.report(); // Report after all subtests have finished.

                // Do not lock t.done to allow race detector to detect race in case
                // the user does not appropriately synchronizes a goroutine.
                t.done = true;
                if (t.parent != null && atomic.LoadInt32(ref t.hasSub) == 0L)
                {
                    t.setRan();
                }
                t.signal.Send(true);
            }());

            t.start = time.Now();
            t.raceErrors = -race.Errors();
            fn(t);

            if (t.failed)
            {
                atomic.AddUint32(ref numFailed, 1L);
            }
            t.finished = true;
        });

        // Run runs f as a subtest of t called name. It runs f in a separate goroutine
        // and blocks until f returns or calls t.Parallel to become a parallel test.
        // Run reports whether f succeeded (or at least did not fail before calling t.Parallel).
        //
        // Run may be called simultaneously from multiple goroutines, but all such calls
        // must return before the outer test function for t returns.
        private static bool Run(this ref T t, @string name, Action<ref T> f)
        {
            atomic.StoreInt32(ref t.hasSub, 1L);
            var (testName, ok, _) = t.context.match.fullName(ref t.common, name);
            if (!ok || shouldFailFast())
            {
                return true;
            }
            t = ref new T(common:common{barrier:make(chanbool),signal:make(chanbool),name:testName,parent:&t.common,level:t.level+1,chatty:t.chatty,},context:t.context,);
            t.w = new indenter(&t.common);

            if (t.chatty)
            { 
                // Print directly to root's io.Writer so there is no delay.
                var root = t.parent;
                while (root.parent != null)
                {
                    root = root.parent;
                }

                root.mu.Lock();
                fmt.Fprintf(root.w, "=== RUN   %s\n", t.name);
                root.mu.Unlock();
            } 
            // Instead of reducing the running count of this test before calling the
            // tRunner and increasing it afterwards, we rely on tRunner keeping the
            // count correct. This ensures that a sequence of sequential tests runs
            // without being preempted, even when their parent is a parallel test. This
            // may especially reduce surprises if *parallel == 1.
            go_(() => tRunner(t, f));
            t.signal.Receive();
            return !t.failed;
        }

        // testContext holds all fields that are common to all tests. This includes
        // synchronization primitives to run at most *parallel tests.
        private partial struct testContext
        {
            public ptr<matcher> match;
            public sync.Mutex mu; // Channel used to signal tests that are ready to be run in parallel.
            public channel<bool> startParallel; // running is the number of tests currently running in parallel.
// This does not include tests that are waiting for subtests to complete.
            public long running; // numWaiting is the number tests waiting to be run in parallel.
            public long numWaiting; // maxParallel is a copy of the parallel flag.
            public long maxParallel;
        }

        private static ref testContext newTestContext(long maxParallel, ref matcher m)
        {
            return ref new testContext(match:m,startParallel:make(chanbool),maxParallel:maxParallel,running:1,);
        }

        private static void waitParallel(this ref testContext c)
        {
            c.mu.Lock();
            if (c.running < c.maxParallel)
            {
                c.running++;
                c.mu.Unlock();
                return;
            }
            c.numWaiting++;
            c.mu.Unlock().Send(c.startParallel);
        }

        private static void release(this ref testContext c)
        {
            c.mu.Lock();
            if (c.numWaiting == 0L)
            {
                c.running--;
                c.mu.Unlock();
                return;
            }
            c.numWaiting--;
            c.mu.Unlock();
            c.startParallel.Send(true); // Pick a waiting test to be run.
        }

        // No one should be using func Main anymore.
        // See the doc comment on func Main and use MainStart instead.
        private static var errMain = errors.New("testing: unexpected use of func Main");

        public delegate  error) matchStringOnly(@string,  @string,  (bool);

        private static (bool, error) MatchString(this matchStringOnly f, @string pat, @string str)
        {
            return f(pat, str);
        }
        private static error StartCPUProfile(this matchStringOnly f, io.Writer w)
        {
            return error.As(errMain);
        }
        private static void StopCPUProfile(this matchStringOnly f)
        {
        }
        private static error WriteHeapProfile(this matchStringOnly f, io.Writer w)
        {
            return error.As(errMain);
        }
        private static error WriteProfileTo(this matchStringOnly f, @string _p0, io.Writer _p0, long _p0)
        {
            return error.As(errMain);
        }
        private static @string ImportPath(this matchStringOnly f)
        {
            return "";
        }
        private static void StartTestLog(this matchStringOnly f, io.Writer _p0)
        {
        }
        private static error StopTestLog(this matchStringOnly f)
        {
            return error.As(errMain);
        }

        // Main is an internal function, part of the implementation of the "go test" command.
        // It was exported because it is cross-package and predates "internal" packages.
        // It is no longer used by "go test" but preserved, as much as possible, for other
        // systems that simulate "go test" using Main, but Main sometimes cannot be updated as
        // new functionality is added to the testing package.
        // Systems simulating "go test" should be updated to use MainStart.
        public static (bool, error) Main(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalExample> examples)
        {
            os.Exit(MainStart(matchStringOnly(matchString), tests, benchmarks, examples).Run());
        }

        // M is a type passed to a TestMain function to run the actual tests.
        public partial struct M
        {
            public testDeps deps;
            public slice<InternalTest> tests;
            public slice<InternalBenchmark> benchmarks;
            public slice<InternalExample> examples;
            public ptr<time.Timer> timer;
            public sync.Once afterOnce;
            public long numRun;
        }

        // testDeps is an internal interface of functionality that is
        // passed into this package by a test's generated main package.
        // The canonical implementation of this interface is
        // testing/internal/testdeps's TestDeps.
        private partial interface testDeps
        {
            error ImportPath();
            error MatchString(@string pat, @string str);
            error StartCPUProfile(io.Writer _p0);
            error StopCPUProfile();
            error StartTestLog(io.Writer _p0);
            error StopTestLog();
            error WriteHeapProfile(io.Writer _p0);
            error WriteProfileTo(@string _p0, io.Writer _p0, long _p0);
        }

        // MainStart is meant for use by tests generated by 'go test'.
        // It is not meant to be called directly and is not subject to the Go 1 compatibility document.
        // It may change signature from release to release.
        public static ref M MainStart(testDeps deps, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalExample> examples)
        {
            return ref new M(deps:deps,tests:tests,benchmarks:benchmarks,examples:examples,);
        }

        // Run runs the tests. It returns an exit code to pass to os.Exit.
        private static long Run(this ref M _m) => func(_m, (ref M m, Defer defer, Panic _, Recover __) =>
        { 
            // Count the number of calls to m.Run.
            // We only ever expected 1, but we didn't enforce that,
            // and now there are tests in the wild that call m.Run multiple times.
            // Sigh. golang.org/issue/23129.
            m.numRun++; 

            // TestMain may have already called flag.Parse.
            if (!flag.Parsed())
            {
                flag.Parse();
            }
            if (parallel < 1L.Value)
            {
                fmt.Fprintln(os.Stderr, "testing: -parallel can only be given a positive integer");
                flag.Usage();
                return 2L;
            }
            if (len(matchList.Value) != 0L)
            {
                listTests(m.deps.MatchString, m.tests, m.benchmarks, m.examples);
                return 0L;
            }
            parseCpuList();

            m.before();
            defer(m.after());
            m.startAlarm();
            haveExamples = len(m.examples) > 0L;
            var (testRan, testOk) = runTests(m.deps.MatchString, m.tests);
            var (exampleRan, exampleOk) = runExamples(m.deps.MatchString, m.examples);
            m.stopAlarm();
            if (!testRan && !exampleRan && matchBenchmarks == "".Value)
            {
                fmt.Fprintln(os.Stderr, "testing: warning: no tests to run");
            }
            if (!testOk || !exampleOk || !runBenchmarks(m.deps.ImportPath(), m.deps.MatchString, m.benchmarks) || race.Errors() > 0L)
            {
                fmt.Println("FAIL");
                return 1L;
            }
            fmt.Println("PASS");
            return 0L;
        });

        private static void report(this ref T t)
        {
            if (t.parent == null)
            {
                return;
            }
            var dstr = fmtDuration(t.duration);
            @string format = "--- %s: %s (%s)\n";
            if (t.Failed())
            {
                t.flushToParent(format, "FAIL", t.name, dstr);
            }
            else if (t.chatty)
            {
                if (t.Skipped())
                {
                    t.flushToParent(format, "SKIP", t.name, dstr);
                }
                else
                {
                    t.flushToParent(format, "PASS", t.name, dstr);
                }
            }
        }

        private static (bool, error) listTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests, slice<InternalBenchmark> benchmarks, slice<InternalExample> examples)
        {
            {
                var (_, err) = matchString(matchList.Value, "non-empty");

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: invalid regexp in -test.list (%q): %s\n", matchList.Value, err);
                    os.Exit(1L);
                }

            }

            foreach (var (_, test) in tests)
            {
                {
                    var ok__prev1 = ok;

                    var (ok, _) = matchString(matchList.Value, test.Name);

                    if (ok)
                    {
                        fmt.Println(test.Name);
                    }

                    ok = ok__prev1;

                }
            }
            foreach (var (_, bench) in benchmarks)
            {
                {
                    var ok__prev1 = ok;

                    (ok, _) = matchString(matchList.Value, bench.Name);

                    if (ok)
                    {
                        fmt.Println(bench.Name);
                    }

                    ok = ok__prev1;

                }
            }
            foreach (var (_, example) in examples)
            {
                {
                    var ok__prev1 = ok;

                    (ok, _) = matchString(matchList.Value, example.Name);

                    if (ok)
                    {
                        fmt.Println(example.Name);
                    }

                    ok = ok__prev1;

                }
            }
        }

        // An internal function but exported because it is cross-package; part of the implementation
        // of the "go test" command.
        public static bool RunTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests)
        {
            var (ran, ok) = runTests(matchString, tests);
            if (!ran && !haveExamples)
            {
                fmt.Fprintln(os.Stderr, "testing: warning: no tests to run");
            }
            return ok;
        }

        private static (bool, bool) runTests(Func<@string, @string, (bool, error)> matchString, slice<InternalTest> tests)
        {
            ok = true;
            foreach (var (_, procs) in cpuList)
            {
                runtime.GOMAXPROCS(procs);
                for (var i = uint(0L); i < count.Value; i++)
                {
                    if (shouldFailFast())
                    {
                        break;
                    }
                    var ctx = newTestContext(parallel.Value, newMatcher(matchString, match.Value, "-test.run"));
                    T t = ref new T(common:common{signal:make(chanbool),barrier:make(chanbool),w:os.Stdout,chatty:*chatty,},context:ctx,);
                    tRunner(t, t =>
                    {
                        foreach (var (_, test) in tests)
                        {
                            t.Run(test.Name, test.F);
                        } 
                        // Run catching the signal rather than the tRunner as a separate
                        // goroutine to avoid adding a goroutine during the sequential
                        // phase as this pollutes the stacktrace output when aborting.
                        go_(() => () =>
                        {
                            t.signal.Receive();

                        }());
                    });
                    ok = ok && !t.Failed();
                    ran = ran || t.ran;
                }

            }
            return (ran, ok);
        }

        // before runs before all testing.
        private static void before(this ref M m)
        {
            if (memProfileRate > 0L.Value)
            {
                runtime.MemProfileRate = memProfileRate.Value;
            }
            if (cpuProfile != "".Value)
            {
                var (f, err) = os.Create(toOutputDir(cpuProfile.Value));
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    return;
                }
                {
                    var err__prev2 = err;

                    var err = m.deps.StartCPUProfile(f);

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testing: can't start cpu profile: %s\n", err);
                        f.Close();
                        return;
                    } 
                    // Could save f so after can call f.Close; not worth the effort.

                    err = err__prev2;

                } 
                // Could save f so after can call f.Close; not worth the effort.
            }
            if (traceFile != "".Value)
            {
                (f, err) = os.Create(toOutputDir(traceFile.Value));
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    return;
                }
                {
                    var err__prev2 = err;

                    err = trace.Start(f);

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testing: can't start tracing: %s\n", err);
                        f.Close();
                        return;
                    } 
                    // Could save f so after can call f.Close; not worth the effort.

                    err = err__prev2;

                } 
                // Could save f so after can call f.Close; not worth the effort.
            }
            if (blockProfile != "" && blockProfileRate >= 0L.Value.Value)
            {
                runtime.SetBlockProfileRate(blockProfileRate.Value);
            }
            if (mutexProfile != "" && mutexProfileFraction >= 0L.Value.Value)
            {
                runtime.SetMutexProfileFraction(mutexProfileFraction.Value);
            }
            if (coverProfile != "" && cover.Mode == "".Value)
            {
                fmt.Fprintf(os.Stderr, "testing: cannot use -test.coverprofile because test binary was not built with coverage enabled\n");
                os.Exit(2L);
            }
            if (testlog != "".Value)
            { 
                // Note: Not using toOutputDir.
                // This file is for use by cmd/go, not users.
                ref os.File f = default;
                err = default;
                if (m.numRun == 1L)
                {
                    f, err = os.Create(testlog.Value);
                }
                else
                {
                    f, err = os.OpenFile(testlog.Value, os.O_WRONLY, 0L);
                    if (err == null)
                    {
                        f.Seek(0L, io.SeekEnd);
                    }
                }
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    os.Exit(2L);
                }
                m.deps.StartTestLog(f);
                testlogFile = f;
            }
        }

        // after runs after all testing.
        private static void after(this ref M m)
        {
            m.afterOnce.Do(() =>
            {
                m.writeProfiles();
            });
        }

        private static void writeProfiles(this ref M m)
        {
            if (testlog != "".Value)
            {
                {
                    var err__prev2 = err;

                    var err = m.deps.StopTestLog();

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testing: can't write %s: %s\n", testlog.Value, err);
                        os.Exit(2L);
                    }

                    err = err__prev2;

                }
                {
                    var err__prev2 = err;

                    err = testlogFile.Close();

                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testing: can't write %s: %s\n", testlog.Value, err);
                        os.Exit(2L);
                    }

                    err = err__prev2;

                }
            }
            if (cpuProfile != "".Value)
            {
                m.deps.StopCPUProfile(); // flushes profile to disk
            }
            if (traceFile != "".Value)
            {
                trace.Stop(); // flushes trace to disk
            }
            if (memProfile != "".Value)
            {
                var (f, err) = os.Create(toOutputDir(memProfile.Value));
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    os.Exit(2L);
                }
                runtime.GC(); // materialize all statistics
                err = m.deps.WriteHeapProfile(f);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: can't write %s: %s\n", memProfile.Value, err);
                    os.Exit(2L);
                }
                f.Close();
            }
            if (blockProfile != "" && blockProfileRate >= 0L.Value.Value)
            {
                (f, err) = os.Create(toOutputDir(blockProfile.Value));
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    os.Exit(2L);
                }
                err = m.deps.WriteProfileTo("block", f, 0L);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: can't write %s: %s\n", blockProfile.Value, err);
                    os.Exit(2L);
                }
                f.Close();
            }
            if (mutexProfile != "" && mutexProfileFraction >= 0L.Value.Value)
            {
                (f, err) = os.Create(toOutputDir(mutexProfile.Value));
                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: %s\n", err);
                    os.Exit(2L);
                }
                err = m.deps.WriteProfileTo("mutex", f, 0L);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "testing: can't write %s: %s\n", blockProfile.Value, err);
                    os.Exit(2L);
                }
                f.Close();
            }
            if (cover.Mode != "")
            {
                coverReport();
            }
        }

        // toOutputDir returns the file name relocated, if required, to outputDir.
        // Simple implementation to avoid pulling in path/filepath.
        private static @string toOutputDir(@string path)
        {
            if (outputDir == "" || path == "".Value)
            {
                return path;
            }
            if (runtime.GOOS == "windows")
            { 
                // On Windows, it's clumsy, but we can be almost always correct
                // by just looking for a drive letter and a colon.
                // Absolute paths always have a drive letter (ignoring UNC).
                // Problem: if path == "C:A" and outputdir == "C:\Go" it's unclear
                // what to do, but even then path/filepath doesn't help.
                // TODO: Worth doing better? Probably not, because we're here only
                // under the management of go test.
                if (len(path) >= 2L)
                {
                    var letter = path[0L];
                    var colon = path[1L];
                    if (('a' <= letter && letter <= 'z' || 'A' <= letter && letter <= 'Z') && colon == ':')
                    { 
                        // If path starts with a drive letter we're stuck with it regardless.
                        return path;
                    }
                }
            }
            if (os.IsPathSeparator(path[0L]))
            {
                return path;
            }
            return fmt.Sprintf("%s%c%s", outputDir.Value, os.PathSeparator, path);
        }

        // startAlarm starts an alarm if requested.
        private static void startAlarm(this ref M _m) => func(_m, (ref M m, Defer _, Panic panic, Recover __) =>
        {
            if (timeout > 0L.Value)
            {
                m.timer = time.AfterFunc(timeout.Value, () =>
                {
                    m.after();
                    debug.SetTraceback("all");
                    panic(fmt.Sprintf("test timed out after %v", timeout.Value));
                });
            }
        });

        // stopAlarm turns off the alarm.
        private static void stopAlarm(this ref M m)
        {
            if (timeout > 0L.Value)
            {
                m.timer.Stop();
            }
        }

        private static void parseCpuList()
        {
            foreach (var (_, val) in strings.Split(cpuListStr.Value, ","))
            {
                val = strings.TrimSpace(val);
                if (val == "")
                {
                    continue;
                }
                var (cpu, err) = strconv.Atoi(val);
                if (err != null || cpu <= 0L)
                {
                    fmt.Fprintf(os.Stderr, "testing: invalid value %q for -test.cpu\n", val);
                    os.Exit(1L);
                }
                cpuList = append(cpuList, cpu);
            }
            if (cpuList == null)
            {
                cpuList = append(cpuList, runtime.GOMAXPROCS(-1L));
            }
        }

        private static bool shouldFailFast()
        {
            return failFast && atomic.LoadUint32(ref numFailed) > 0L.Value;
        }
    }
}
