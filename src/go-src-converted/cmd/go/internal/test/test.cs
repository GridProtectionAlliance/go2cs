// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package test -- go2cs converted at 2022 March 06 23:17:52 UTC
// import "cmd/go/internal/test" ==> using test = go.cmd.go.@internal.test_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\test\test.go
using bytes = go.bytes_package;
using context = go.context_package;
using sha256 = go.crypto.sha256_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using exec = go.@internal.execabs_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using trace = go.cmd.go.@internal.trace_package;
using work = go.cmd.go.@internal.work_package;
using test2json = go.cmd.@internal.test2json_package;
using System;
using System.Threading;


namespace go.cmd.go.@internal;

public static partial class test_package {

    // Break init loop.
private static void init() {
    CmdTest.Run = runTest;
}

private static readonly @string testUsage = "go test [build/test flags] [packages] [build/test flags & test binary flags]";



public static ptr<base.Command> CmdTest = addr(new base.Command(CustomFlags:true,UsageLine:testUsage,Short:"test packages",Long:`
'Go test' automates testing the packages named by the import paths.
It prints a summary of the test results in the format:

	ok   archive/tar   0.011s
	FAIL archive/zip   0.022s
	ok   compress/gzip 0.033s
	...

followed by detailed output for each failed package.

'Go test' recompiles each package along with any files with names matching
the file pattern "*_test.go".
These additional files can contain test functions, benchmark functions, and
example functions. See 'go help testfunc' for more.
Each listed package causes the execution of a separate test binary.
Files whose names begin with "_" (including "_test.go") or "." are ignored.

Test files that declare a package with the suffix "_test" will be compiled as a
separate package, and then linked and run with the main test binary.

The go tool will ignore a directory named "testdata", making it available
to hold ancillary data needed by the tests.

As part of building a test binary, go test runs go vet on the package
and its test source files to identify significant problems. If go vet
finds any problems, go test reports those and does not run the test
binary. Only a high-confidence subset of the default go vet checks are
used. That subset is: 'atomic', 'bool', 'buildtags', 'errorsas',
'ifaceassert', 'nilfunc', 'printf', and 'stringintconv'. You can see
the documentation for these and other vet tests via "go doc cmd/vet".
To disable the running of go vet, use the -vet=off flag.

All test output and summary lines are printed to the go command's
standard output, even if the test printed them to its own standard
error. (The go command's standard error is reserved for printing
errors building the tests.)

Go test runs in two different modes:

The first, called local directory mode, occurs when go test is
invoked with no package arguments (for example, 'go test' or 'go
test -v'). In this mode, go test compiles the package sources and
tests found in the current directory and then runs the resulting
test binary. In this mode, caching (discussed below) is disabled.
After the package test finishes, go test prints a summary line
showing the test status ('ok' or 'FAIL'), package name, and elapsed
time.

The second, called package list mode, occurs when go test is invoked
with explicit package arguments (for example 'go test math', 'go
test ./...', and even 'go test .'). In this mode, go test compiles
and tests each of the packages listed on the command line. If a
package test passes, go test prints only the final 'ok' summary
line. If a package test fails, go test prints the full test output.
If invoked with the -bench or -v flag, go test prints the full
output even for passing package tests, in order to display the
requested benchmark results or verbose logging. After the package
tests for all of the listed packages finish, and their output is
printed, go test prints a final 'FAIL' status if any package test
has failed.

In package list mode only, go test caches successful package test
results to avoid unnecessary repeated running of tests. When the
result of a test can be recovered from the cache, go test will
redisplay the previous output instead of running the test binary
again. When this happens, go test prints '(cached)' in place of the
elapsed time in the summary line.

The rule for a match in the cache is that the run involves the same
test binary and the flags on the command line come entirely from a
restricted set of 'cacheable' test flags, defined as -benchtime, -cpu,
-list, -parallel, -run, -short, and -v. If a run of go test has any test
or non-test flags outside this set, the result is not cached. To
disable test caching, use any test flag or argument other than the
cacheable flags. The idiomatic way to disable test caching explicitly
is to use -count=1. Tests that open files within the package's source
root (usually $GOPATH) or that consult environment variables only
match future runs in which the files and environment variables are unchanged.
A cached test result is treated as executing in no time at all,
so a successful package test result will be cached and reused
regardless of -timeout setting.

In addition to the build flags, the flags handled by 'go test' itself are:

	-args
	    Pass the remainder of the command line (everything after -args)
	    to the test binary, uninterpreted and unchanged.
	    Because this flag consumes the remainder of the command line,
	    the package list (if present) must appear before this flag.

	-c
	    Compile the test binary to pkg.test but do not run it
	    (where pkg is the last element of the package's import path).
	    The file name can be changed with the -o flag.

	-exec xprog
	    Run the test binary using xprog. The behavior is the same as
	    in 'go run'. See 'go help run' for details.

	-i
	    Install packages that are dependencies of the test.
	    Do not run the test.
	    The -i flag is deprecated. Compiled packages are cached automatically.

	-json
	    Convert test output to JSON suitable for automated processing.
	    See 'go doc test2json' for the encoding details.

	-o file
	    Compile the test binary to the named file.
	    The test still runs (unless -c or -i is specified).

The test binary also accepts flags that control execution of the test; these
flags are also accessible by 'go test'. See 'go help testflag' for details.

For more about build flags, see 'go help build'.
For more about specifying packages, see 'go help packages'.

See also: go build, go vet.
`,));

public static ptr<base.Command> HelpTestflag = addr(new base.Command(UsageLine:"testflag",Short:"testing flags",Long:`
The 'go test' command takes both flags that apply to 'go test' itself
and flags that apply to the resulting test binary.

Several of the flags control profiling and write an execution profile
suitable for "go tool pprof"; run "go tool pprof -h" for more
information. The --alloc_space, --alloc_objects, and --show_bytes
options of pprof control how the information is presented.

The following flags are recognized by the 'go test' command and
control the execution of any test:

	-bench regexp
	    Run only those benchmarks matching a regular expression.
	    By default, no benchmarks are run.
	    To run all benchmarks, use '-bench .' or '-bench=.'.
	    The regular expression is split by unbracketed slash (/)
	    characters into a sequence of regular expressions, and each
	    part of a benchmark's identifier must match the corresponding
	    element in the sequence, if any. Possible parents of matches
	    are run with b.N=1 to identify sub-benchmarks. For example,
	    given -bench=X/Y, top-level benchmarks matching X are run
	    with b.N=1 to find any sub-benchmarks matching Y, which are
	    then run in full.

	-benchtime t
	    Run enough iterations of each benchmark to take t, specified
	    as a time.Duration (for example, -benchtime 1h30s).
	    The default is 1 second (1s).
	    The special syntax Nx means to run the benchmark N times
	    (for example, -benchtime 100x).

	-count n
	    Run each test and benchmark n times (default 1).
	    If -cpu is set, run n times for each GOMAXPROCS value.
	    Examples are always run once.

	-cover
	    Enable coverage analysis.
	    Note that because coverage works by annotating the source
	    code before compilation, compilation and test failures with
	    coverage enabled may report line numbers that don't correspond
	    to the original sources.

	-covermode set,count,atomic
	    Set the mode for coverage analysis for the package[s]
	    being tested. The default is "set" unless -race is enabled,
	    in which case it is "atomic".
	    The values:
		set: bool: does this statement run?
		count: int: how many times does this statement run?
		atomic: int: count, but correct in multithreaded tests;
			significantly more expensive.
	    Sets -cover.

	-coverpkg pattern1,pattern2,pattern3
	    Apply coverage analysis in each test to packages matching the patterns.
	    The default is for each test to analyze only the package being tested.
	    See 'go help packages' for a description of package patterns.
	    Sets -cover.

	-cpu 1,2,4
	    Specify a list of GOMAXPROCS values for which the tests or
	    benchmarks should be executed. The default is the current value
	    of GOMAXPROCS.

	-failfast
	    Do not start new tests after the first test failure.

	-list regexp
	    List tests, benchmarks, or examples matching the regular expression.
	    No tests, benchmarks or examples will be run. This will only
	    list top-level tests. No subtest or subbenchmarks will be shown.

	-parallel n
	    Allow parallel execution of test functions that call t.Parallel.
	    The value of this flag is the maximum number of tests to run
	    simultaneously; by default, it is set to the value of GOMAXPROCS.
	    Note that -parallel only applies within a single test binary.
	    The 'go test' command may run tests for different packages
	    in parallel as well, according to the setting of the -p flag
	    (see 'go help build').

	-run regexp
	    Run only those tests and examples matching the regular expression.
	    For tests, the regular expression is split by unbracketed slash (/)
	    characters into a sequence of regular expressions, and each part
	    of a test's identifier must match the corresponding element in
	    the sequence, if any. Note that possible parents of matches are
	    run too, so that -run=X/Y matches and runs and reports the result
	    of all tests matching X, even those without sub-tests matching Y,
	    because it must run them to look for those sub-tests.

	-short
	    Tell long-running tests to shorten their run time.
	    It is off by default but set during all.bash so that installing
	    the Go tree can run a sanity check but not spend time running
	    exhaustive tests.

	-shuffle off,on,N
		Randomize the execution order of tests and benchmarks.
		It is off by default. If -shuffle is set to on, then it will seed
		the randomizer using the system clock. If -shuffle is set to an
		integer N, then N will be used as the seed value. In both cases,
		the seed will be reported for reproducibility.

	-timeout d
	    If a test binary runs longer than duration d, panic.
	    If d is 0, the timeout is disabled.
	    The default is 10 minutes (10m).

	-v
	    Verbose output: log all tests as they are run. Also print all
	    text from Log and Logf calls even if the test succeeds.

	-vet list
	    Configure the invocation of "go vet" during "go test"
	    to use the comma-separated list of vet checks.
	    If list is empty, "go test" runs "go vet" with a curated list of
	    checks believed to be always worth addressing.
	    If list is "off", "go test" does not run "go vet" at all.

The following flags are also recognized by 'go test' and can be used to
profile the tests during execution:

	-benchmem
	    Print memory allocation statistics for benchmarks.

	-blockprofile block.out
	    Write a goroutine blocking profile to the specified file
	    when all tests are complete.
	    Writes test binary as -c would.

	-blockprofilerate n
	    Control the detail provided in goroutine blocking profiles by
	    calling runtime.SetBlockProfileRate with n.
	    See 'go doc runtime.SetBlockProfileRate'.
	    The profiler aims to sample, on average, one blocking event every
	    n nanoseconds the program spends blocked. By default,
	    if -test.blockprofile is set without this flag, all blocking events
	    are recorded, equivalent to -test.blockprofilerate=1.

	-coverprofile cover.out
	    Write a coverage profile to the file after all tests have passed.
	    Sets -cover.

	-cpuprofile cpu.out
	    Write a CPU profile to the specified file before exiting.
	    Writes test binary as -c would.

	-memprofile mem.out
	    Write an allocation profile to the file after all tests have passed.
	    Writes test binary as -c would.

	-memprofilerate n
	    Enable more precise (and expensive) memory allocation profiles by
	    setting runtime.MemProfileRate. See 'go doc runtime.MemProfileRate'.
	    To profile all memory allocations, use -test.memprofilerate=1.

	-mutexprofile mutex.out
	    Write a mutex contention profile to the specified file
	    when all tests are complete.
	    Writes test binary as -c would.

	-mutexprofilefraction n
	    Sample 1 in n stack traces of goroutines holding a
	    contended mutex.

	-outputdir directory
	    Place output files from profiling in the specified directory,
	    by default the directory in which "go test" is running.

	-trace trace.out
	    Write an execution trace to the specified file before exiting.

Each of these flags is also recognized with an optional 'test.' prefix,
as in -test.v. When invoking the generated test binary (the result of
'go test -c') directly, however, the prefix is mandatory.

The 'go test' command rewrites or removes recognized flags,
as appropriate, both before and after the optional package list,
before invoking the test binary.

For instance, the command

	go test -v -myflag testdata -cpuprofile=prof.out -x

will compile the test binary and then run it as

	pkg.test -test.v -myflag testdata -test.cpuprofile=prof.out

(The -x flag is removed because it applies only to the go command's
execution, not to the test itself.)

The test flags that generate profiles (other than for coverage) also
leave the test binary in pkg.test for use when analyzing the profiles.

When 'go test' runs a test binary, it does so from within the
corresponding package's source code directory. Depending on the test,
it may be necessary to do the same when invoking a generated test
binary directly.

The command-line package list, if present, must appear before any
flag not known to the go test command. Continuing the example above,
the package list would have to appear before -myflag, but could appear
on either side of -v.

When 'go test' runs in package list mode, 'go test' caches successful
package test results to avoid unnecessary repeated running of tests. To
disable test caching, use any test flag or argument other than the
cacheable flags. The idiomatic way to disable test caching explicitly
is to use -count=1.

To keep an argument for a test binary from being interpreted as a
known flag or a package name, use -args (see 'go help test') which
passes the remainder of the command line through to the test binary
uninterpreted and unaltered.

For instance, the command

	go test -v -args -x -v

will compile the test binary and then run it as

	pkg.test -test.v -x -v

Similarly,

	go test -args math

will compile the test binary and then run it as

	pkg.test math

In the first example, the -x and the second -v are passed through to the
test binary unchanged and with no effect on the go command itself.
In the second example, the argument math is passed through to the test
binary, instead of being interpreted as the package list.
`,));

public static ptr<base.Command> HelpTestfunc = addr(new base.Command(UsageLine:"testfunc",Short:"testing functions",Long:`
The 'go test' command expects to find test, benchmark, and example functions
in the "*_test.go" files corresponding to the package under test.

A test function is one named TestXxx (where Xxx does not start with a
lower case letter) and should have the signature,

	func TestXxx(t *testing.T) { ... }

A benchmark function is one named BenchmarkXxx and should have the signature,

	func BenchmarkXxx(b *testing.B) { ... }

An example function is similar to a test function but, instead of using
*testing.T to report success or failure, prints output to os.Stdout.
If the last comment in the function starts with "Output:" then the output
is compared exactly against the comment (see examples below). If the last
comment begins with "Unordered output:" then the output is compared to the
comment, however the order of the lines is ignored. An example with no such
comment is compiled but not executed. An example with no text after
"Output:" is compiled, executed, and expected to produce no output.

Godoc displays the body of ExampleXxx to demonstrate the use
of the function, constant, or variable Xxx. An example of a method M with
receiver type T or *T is named ExampleT_M. There may be multiple examples
for a given function, constant, or variable, distinguished by a trailing _xxx,
where xxx is a suffix not beginning with an upper case letter.

Here is an example of an example:

	func ExamplePrintln() {
		Println("The output of\nthis example.")
		// Output: The output of
		// this example.
	}

Here is another example where the ordering of the output is ignored:

	func ExamplePerm() {
		for _, value := range Perm(4) {
			fmt.Println(value)
		}

		// Unordered output: 4
		// 2
		// 1
		// 3
		// 0
	}

The entire test file is presented as the example when it contains a single
example function, at least one other function, type, variable, or constant
declaration, and no test or benchmark functions.

See the documentation of the testing package for more information.
`,));

private static @string testBench = default;private static bool testC = default;private static bool testCover = default;private static @string testCoverMode = default;private static slice<@string> testCoverPaths = default;private static slice<ptr<load.Package>> testCoverPkgs = default;private static @string testCoverProfile = default;private static bool testJSON = default;private static @string testList = default;private static @string testO = default;private static outputdirFlag testOutputDir = default;private static shuffleFlag testShuffle = default;private static time.Duration testTimeout = default;private static bool testV = default;private static vetFlag testVet = new vetFlag(flags:defaultVetFlags);

private static slice<@string> testArgs = default;private static slice<@string> pkgArgs = default;private static slice<ptr<load.Package>> pkgs = default;private static bool testHelp = default;private static nint testKillTimeout = 100 * 365 * 24 * time.Hour;private static time.Time testCacheExpire = default;private static @string testBlockProfile = default;private static @string testCPUProfile = default;private static @string testMemProfile = default;private static @string testMutexProfile = default;private static @string testTrace = default; // profiling flag that limits test to one package

// testProfile returns the name of an arbitrary single-package profiling flag
// that is set, if any.
private static @string testProfile() {

    if (testBlockProfile != "") 
        return "-blockprofile";
    else if (testCPUProfile != "") 
        return "-cpuprofile";
    else if (testMemProfile != "") 
        return "-memprofile";
    else if (testMutexProfile != "") 
        return "-mutexprofile";
    else if (testTrace != "") 
        return "-trace";
    else 
        return "";
    
}

// testNeedBinary reports whether the test needs to keep the binary around.
private static bool testNeedBinary() {

    if (testBlockProfile != "") 
        return true;
    else if (testCPUProfile != "") 
        return true;
    else if (testMemProfile != "") 
        return true;
    else if (testMutexProfile != "") 
        return true;
    else if (testO != "") 
        return true;
    else 
        return false;
    
}

// testShowPass reports whether the output for a passing test should be shown.
private static bool testShowPass() {
    return testV || (testList != "") || testHelp;
}

private static @string defaultVetFlags = new slice<@string>(new @string[] { "-atomic", "-bool", "-buildtags", "-errorsas", "-ifaceassert", "-nilfunc", "-printf", "-stringintconv" });

private static void runTest(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) => func((defer, _, _) => {
    ref base.Command cmd = ref _addr_cmd.val;

    pkgArgs, testArgs = testFlags(args);

    if (cfg.DebugTrace != "") {
        Func<error> close = default;
        error err = default!;
        ctx, close, err = trace.Start(ctx, cfg.DebugTrace);
        if (err != null) {
            @base.Fatalf("failed to start trace: %v", err);
        }
        defer(() => {
            {
                error err__prev2 = err;

                err = close();

                if (err != null) {
                    @base.Fatalf("failed to stop trace: %v", err);
                }

                err = err__prev2;

            }

        }());

    }
    var (ctx, span) = trace.StartSpan(ctx, fmt.Sprint("Running ", cmd.Name(), " command"));
    defer(span.Done());

    work.FindExecCmd(); // initialize cached result

    work.BuildInit();
    work.VetFlags = testVet.flags;
    work.VetExplicit = testVet.@explicit;

    load.PackageOpts pkgOpts = new load.PackageOpts(ModResolveTests:true);
    pkgs = load.PackagesAndErrors(ctx, pkgOpts, pkgArgs);
    load.CheckPackageErrors(pkgs);
    if (len(pkgs) == 0) {
        @base.Fatalf("no packages to test");
    }
    if (testC && len(pkgs) != 1) {
        @base.Fatalf("cannot use -c flag with multiple packages");
    }
    if (testO != "" && len(pkgs) != 1) {
        @base.Fatalf("cannot use -o flag with multiple packages");
    }
    if (testProfile() != "" && len(pkgs) != 1) {
        @base.Fatalf("cannot use %s flag with multiple packages", testProfile());
    }
    initCoverProfile();
    defer(closeCoverProfile()); 

    // If a test timeout is finite, set our kill timeout
    // to that timeout plus one minute. This is a backup alarm in case
    // the test wedges with a goroutine spinning and its background
    // timer does not get a chance to fire.
    if (testTimeout > 0) {
        testKillTimeout = testTimeout + 1 * time.Minute;
    }
    if (cfg.BuildI && testO != "") {
        testC = true;
    }
    {
        var dir = cache.DefaultDir();

        if (dir != "off") {
            {
                var (data, _) = lockedfile.Read(filepath.Join(dir, "testexpire.txt"));

                if (len(data) > 0 && data[len(data) - 1] == '\n') {
                    {
                        error err__prev3 = err;

                        var (t, err) = strconv.ParseInt(string(data[..(int)len(data) - 1]), 10, 64);

                        if (err == null) {
                            testCacheExpire = time.Unix(0, t);
                        }

                        err = err__prev3;

                    }

                }

            }

        }
    }


    ref work.Builder b = ref heap(out ptr<work.Builder> _addr_b);
    b.Init();

    if (cfg.BuildI) {
        fmt.Fprint(os.Stderr, "go test: -i flag is deprecated\n");
        cfg.BuildV = testV;

        var deps = make_map<@string, bool>();
        foreach (var (_, dep) in load.TestMainDeps) {
            deps[dep] = true;
        }        {
            var p__prev1 = p;

            foreach (var (_, __p) in pkgs) {
                p = __p; 
                // Dependencies for each test.
                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in p.Imports) {
                        path = __path;
                        deps[path] = true;
                    }

                    path = path__prev2;
                }

                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in p.Resolve(p.TestImports)) {
                        path = __path;
                        deps[path] = true;
                    }

                    path = path__prev2;
                }

                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in p.Resolve(p.XTestImports)) {
                        path = __path;
                        deps[path] = true;
                    }

                    path = path__prev2;
                }
            } 

            // translate C to runtime/cgo

            p = p__prev1;
        }

        if (deps["C"]) {
            delete(deps, "C");
            deps["runtime/cgo"] = true;
        }
        delete(deps, "unsafe");

        @string all = new slice<@string>(new @string[] {  });
        {
            var path__prev1 = path;

            foreach (var (__path) in deps) {
                path = __path;
                if (!build.IsLocalImport(path)) {
                    all = append(all, path);
                }
            }

            path = path__prev1;
        }

        sort.Strings(all);

        ptr<work.Action> a = addr(new work.Action(Mode:"go test -i"));
        var pkgs = load.PackagesAndErrors(ctx, pkgOpts, all);
        load.CheckPackageErrors(pkgs);
        {
            var p__prev1 = p;

            foreach (var (_, __p) in pkgs) {
                p = __p;
                if (cfg.BuildToolchainName == "gccgo" && p.Standard) { 
                    // gccgo's standard library packages
                    // can not be reinstalled.
                    continue;

                }

                a.Deps = append(a.Deps, b.CompileAction(work.ModeInstall, work.ModeInstall, p));

            }

            p = p__prev1;
        }

        b.Do(ctx, a);
        if (!testC || a.Failed) {
            return ;
        }
        b.Init();

    }
    slice<ptr<work.Action>> builds = default;    slice<ptr<work.Action>> runs = default;    slice<ptr<work.Action>> prints = default;



    if (testCoverPaths != null) {
        var match = make_slice<Func<ptr<load.Package>, bool>>(len(testCoverPaths));
        var matched = make_slice<bool>(len(testCoverPaths));
        {
            var i__prev1 = i;

            foreach (var (__i) in testCoverPaths) {
                i = __i;
                match[i] = load.MatchPackage(testCoverPaths[i], @base.Cwd());
            } 

            // Select for coverage all dependencies matching the testCoverPaths patterns.

            i = i__prev1;
        }

        {
            var p__prev1 = p;

            foreach (var (_, __p) in load.TestPackageList(ctx, pkgOpts, pkgs)) {
                p = __p;
                var haveMatch = false;
                {
                    var i__prev2 = i;

                    foreach (var (__i) in testCoverPaths) {
                        i = __i;
                        if (match[i](p)) {
                            matched[i] = true;
                            haveMatch = true;
                        }
                    } 

                    // A package which only has test files can't be imported
                    // as a dependency, nor can it be instrumented for coverage.

                    i = i__prev2;
                }

                if (len(p.GoFiles) + len(p.CgoFiles) == 0) {
                    continue;
                } 

                // Silently ignore attempts to run coverage on
                // sync/atomic when using atomic coverage mode.
                // Atomic coverage mode uses sync/atomic, so
                // we can't also do coverage on it.
                if (testCoverMode == "atomic" && p.Standard && p.ImportPath == "sync/atomic") {
                    continue;
                } 

                // If using the race detector, silently ignore
                // attempts to run coverage on the runtime
                // packages. It will cause the race detector
                // to be invoked before it has been initialized.
                if (cfg.BuildRace && p.Standard && (p.ImportPath == "runtime" || strings.HasPrefix(p.ImportPath, "runtime/internal"))) {
                    continue;
                }

                if (haveMatch) {
                    testCoverPkgs = append(testCoverPkgs, p);
                }

            } 

            // Warn about -coverpkg arguments that are not actually used.

            p = p__prev1;
        }

        {
            var i__prev1 = i;

            foreach (var (__i) in testCoverPaths) {
                i = __i;
                if (!matched[i]) {
                    fmt.Fprintf(os.Stderr, "warning: no packages being tested depend on matches for pattern %s\n", testCoverPaths[i]);
                }
            } 

            // Mark all the coverage packages for rebuilding with coverage.

            i = i__prev1;
        }

        {
            var p__prev1 = p;

            foreach (var (_, __p) in testCoverPkgs) {
                p = __p; 
                // There is nothing to cover in package unsafe; it comes from the compiler.
                if (p.ImportPath == "unsafe") {
                    continue;
                }

                p.Internal.CoverMode = testCoverMode;
                slice<@string> coverFiles = default;
                coverFiles = append(coverFiles, p.GoFiles);
                coverFiles = append(coverFiles, p.CgoFiles);
                coverFiles = append(coverFiles, p.TestGoFiles);
                p.Internal.CoverVars = declareCoverVars(_addr_p, coverFiles);
                if (testCover && testCoverMode == "atomic") {
                    ensureImport(_addr_p, "sync/atomic");
                }

            }

            p = p__prev1;
        }
    }
    {
        var p__prev1 = p;

        foreach (var (_, __p) in pkgs) {
            p = __p; 
            // sync/atomic import is inserted by the cover tool. See #18486
            if (testCover && testCoverMode == "atomic") {
                ensureImport(_addr_p, "sync/atomic");
            }

            var (buildTest, runTest, printTest, err) = builderTest(_addr_b, ctx, pkgOpts, _addr_p);
            if (err != null) {
                var str = err.Error();
                str = strings.TrimPrefix(str, "\n");
                if (p.ImportPath != "") {
                    @base.Errorf("# %s\n%s", p.ImportPath, str);
                }
                else
 {
                    @base.Errorf("%s", str);
                }

                fmt.Printf("FAIL\t%s [setup failed]\n", p.ImportPath);
                continue;

            }

            builds = append(builds, buildTest);
            runs = append(runs, runTest);
            prints = append(prints, printTest);

        }
        p = p__prev1;
    }

    ptr<work.Action> root = addr(new work.Action(Mode:"go test",Func:printExitStatus,Deps:prints)); 

    // Force the printing of results to happen in order,
    // one at a time.
    {
        var i__prev1 = i;
        ptr<work.Action> a__prev1 = a;

        foreach (var (__i, __a) in prints) {
            i = __i;
            a = __a;
            if (i > 0) {
                a.Deps = append(a.Deps, prints[i - 1]);
            }
        }
        i = i__prev1;
        a = a__prev1;
    }

    if (!testC && (testBench != "")) { 
        // The first run must wait for all builds.
        // Later runs must wait for the previous run's print.
        {
            var i__prev1 = i;

            foreach (var (__i, __run) in runs) {
                i = __i;
                run = __run;
                if (i == 0) {
                    run.Deps = append(run.Deps, builds);
                }
                else
 {
                    run.Deps = append(run.Deps, prints[i - 1]);
                }

            }

            i = i__prev1;
        }
    }
    b.Do(ctx, root);

});

// ensures that package p imports the named package
private static void ensureImport(ptr<load.Package> _addr_p, @string pkg) {
    ref load.Package p = ref _addr_p.val;

    foreach (var (_, d) in p.Internal.Imports) {
        if (d.Name == pkg) {
            return ;
        }
    }    var p1 = load.LoadImportWithFlags(pkg, p.Dir, p, addr(new load.ImportStack()), null, 0);
    if (p1.Error != null) {
        @base.Fatalf("load %s: %v", pkg, p1.Error);
    }
    p.Internal.Imports = append(p.Internal.Imports, p1);

}

private static @string windowsBadWords = new slice<@string>(new @string[] { "install", "patch", "setup", "update" });

private static (ptr<work.Action>, ptr<work.Action>, ptr<work.Action>, error) builderTest(ptr<work.Builder> _addr_b, context.Context ctx, load.PackageOpts pkgOpts, ptr<load.Package> _addr_p) {
    ptr<work.Action> buildAction = default!;
    ptr<work.Action> runAction = default!;
    ptr<work.Action> printAction = default!;
    error err = default!;
    ref work.Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;

    if (len(p.TestGoFiles) + len(p.XTestGoFiles) == 0) {
        var build = b.CompileAction(work.ModeBuild, work.ModeBuild, p);
        ptr<work.Action> run = addr(new work.Action(Mode:"test run",Package:p,Deps:[]*work.Action{build}));
        addTestVet(_addr_b, _addr_p, run, _addr_null);
        ptr<work.Action> print = addr(new work.Action(Mode:"test print",Func:builderNoTest,Package:p,Deps:[]*work.Action{run}));
        return (_addr_build!, _addr_run!, _addr_print!, error.As(null!)!);
    }
    ptr<load.TestCover> cover;
    if (testCover) {
        cover = addr(new load.TestCover(Mode:testCoverMode,Local:testCover&&testCoverPaths==nil,Pkgs:testCoverPkgs,Paths:testCoverPaths,DeclVars:declareCoverVars,));
    }
    var (pmain, ptest, pxtest, err) = load.TestPackagesFor(ctx, pkgOpts, p, cover);
    if (err != null) {
        return (_addr_null!, _addr_null!, _addr_null!, error.As(err)!);
    }
    @string elem = default;
    if (p.ImportPath == "command-line-arguments") {
        elem = p.Name;
    }
    else
 {
        elem = p.DefaultExecName();
    }
    var testBinary = elem + ".test";

    var testDir = b.NewObjdir();
    {
        var err__prev1 = err;

        var err = b.Mkdir(testDir);

        if (err != null) {
            return (_addr_null!, _addr_null!, _addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }


    pmain.Dir = testDir;
    pmain.Internal.OmitDebug = !testC && !testNeedBinary();

    if (!cfg.BuildN) { 
        // writeTestmain writes _testmain.go,
        // using the test description gathered in t.
        {
            var err__prev2 = err;

            err = os.WriteFile(testDir + "_testmain.go", pmain.Internal.TestmainGo.val, 0666);

            if (err != null) {
                return (_addr_null!, _addr_null!, _addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

    }
    b.CompileAction(work.ModeBuild, work.ModeBuild, pmain).Objdir = testDir;

    var a = b.LinkAction(work.ModeBuild, work.ModeBuild, pmain);
    a.Target = testDir + testBinary + cfg.ExeSuffix;
    if (cfg.Goos == "windows") { 
        // There are many reserved words on Windows that,
        // if used in the name of an executable, cause Windows
        // to try to ask for extra permissions.
        // The word list includes setup, install, update, and patch,
        // but it does not appear to be defined anywhere.
        // We have run into this trying to run the
        // go.codereview/patch tests.
        // For package names containing those words, use test.test.exe
        // instead of pkgname.test.exe.
        // Note that this file name is only used in the Go command's
        // temporary directory. If the -c or other flags are
        // given, the code below will still use pkgname.test.exe.
        // There are two user-visible effects of this change.
        // First, you can actually run 'go test' in directories that
        // have names that Windows thinks are installer-like,
        // without getting a dialog box asking for more permissions.
        // Second, in the Windows process listing during go test,
        // the test shows up as test.test.exe, not pkgname.test.exe.
        // That second one is a drawback, but it seems a small
        // price to pay for the test running at all.
        // If maintaining the list of bad words is too onerous,
        // we could just do this always on Windows.
        foreach (var (_, bad) in windowsBadWords) {
            if (strings.Contains(testBinary, bad)) {
                a.Target = testDir + "test.test" + cfg.ExeSuffix;
                break;
            }
        }
    }
    buildAction = a;
    ptr<work.Action> installAction;    ptr<work.Action> cleanAction;

    if (testC || testNeedBinary()) { 
        // -c or profiling flag: create action to copy binary to ./test.out.
        var target = filepath.Join(@base.Cwd(), testBinary + cfg.ExeSuffix);
        if (testO != "") {
            target = testO;
            if (!filepath.IsAbs(target)) {
                target = filepath.Join(@base.Cwd(), target);
            }
        }
        if (target == os.DevNull) {
            runAction = buildAction;
        }
        else
 {
            pmain.Target = target;
            installAction = addr(new work.Action(Mode:"test build",Func:work.BuildInstallFunc,Deps:[]*work.Action{buildAction},Package:pmain,Target:target,));
            runAction = installAction; // make sure runAction != nil even if not running test
        }
    }
    ptr<work.Action> vetRunAction;
    if (testC) {
        printAction = addr(new work.Action(Mode:"test print (nop)",Package:p,Deps:[]*work.Action{runAction})); // nop
        vetRunAction = printAction;

    }
    else
 { 
        // run test
        ptr<object> c = @new<runCache>();
        runAction = addr(new work.Action(Mode:"test run",Func:c.builderRunTest,Deps:[]*work.Action{buildAction},Package:p,IgnoreFail:true,TryCache:c.tryCache,Objdir:testDir,));
        vetRunAction = runAction;
        cleanAction = addr(new work.Action(Mode:"test clean",Func:builderCleanTest,Deps:[]*work.Action{runAction},Package:p,IgnoreFail:true,Objdir:testDir,));
        printAction = addr(new work.Action(Mode:"test print",Func:builderPrintTest,Deps:[]*work.Action{cleanAction},Package:p,IgnoreFail:true,));

    }
    if (len(ptest.GoFiles) + len(ptest.CgoFiles) > 0) {
        addTestVet(_addr_b, _addr_ptest, vetRunAction, installAction);
    }
    if (pxtest != null) {
        addTestVet(_addr_b, _addr_pxtest, vetRunAction, installAction);
    }
    if (installAction != null) {
        if (runAction != installAction) {
            installAction.Deps = append(installAction.Deps, runAction);
        }
        if (cleanAction != null) {
            cleanAction.Deps = append(cleanAction.Deps, installAction);
        }
    }
    return (_addr_buildAction!, _addr_runAction!, _addr_printAction!, error.As(null!)!);

}

private static void addTestVet(ptr<work.Builder> _addr_b, ptr<load.Package> _addr_p, ptr<work.Action> _addr_runAction, ptr<work.Action> _addr_installAction) {
    ref work.Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;
    ref work.Action runAction = ref _addr_runAction.val;
    ref work.Action installAction = ref _addr_installAction.val;

    if (testVet.off) {
        return ;
    }
    var vet = b.VetAction(work.ModeBuild, work.ModeBuild, p);
    runAction.Deps = append(runAction.Deps, vet); 
    // Install will clean the build directory.
    // Make sure vet runs first.
    // The install ordering in b.VetAction does not apply here
    // because we are using a custom installAction (created above).
    if (installAction != null) {
        installAction.Deps = append(installAction.Deps, vet);
    }
}

// isTestFile reports whether the source file is a set of tests and should therefore
// be excluded from coverage analysis.
private static bool isTestFile(@string file) { 
    // We don't cover tests, only the code they test.
    return strings.HasSuffix(file, "_test.go");

}

// declareCoverVars attaches the required cover variables names
// to the files, to be used when annotating the files.
private static map<@string, ptr<load.CoverVar>> declareCoverVars(ptr<load.Package> _addr_p, params @string[] files) {
    files = files.Clone();
    ref load.Package p = ref _addr_p.val;

    var coverVars = make_map<@string, ptr<load.CoverVar>>();
    nint coverIndex = 0; 
    // We create the cover counters as new top-level variables in the package.
    // We need to avoid collisions with user variables (GoCover_0 is unlikely but still)
    // and more importantly with dot imports of other covered packages,
    // so we append 12 hex digits from the SHA-256 of the import path.
    // The point is only to avoid accidents, not to defeat users determined to
    // break things.
    var sum = sha256.Sum256((slice<byte>)p.ImportPath);
    var h = fmt.Sprintf("%x", sum[..(int)6]);
    foreach (var (_, file) in files) {
        if (isTestFile(file)) {
            continue;
        }
        @string longFile = default;
        if (p.Internal.Local) {
            longFile = filepath.Join(p.Dir, file);
        }
        else
 {
            longFile = path.Join(p.ImportPath, file);
        }
        coverVars[file] = addr(new load.CoverVar(File:longFile,Var:fmt.Sprintf("GoCover_%d_%x",coverIndex,h),));
        coverIndex++;

    }    return coverVars;

}

private static slice<byte> noTestsToRun = (slice<byte>)"\ntesting: warning: no tests to run\n";

private partial struct runCache {
    public bool disableCache; // cache should be disabled for this run

    public ptr<bytes.Buffer> buf;
    public cache.ActionID id1;
    public cache.ActionID id2;
}

// stdoutMu and lockedStdout provide a locked standard output
// that guarantees never to interlace writes from multiple
// goroutines, so that we can have multiple JSON streams writing
// to a lockedStdout simultaneously and know that events will
// still be intelligible.
private static sync.Mutex stdoutMu = default;

private partial struct lockedStdout {
}

private static (nint, error) Write(this lockedStdout _p0, slice<byte> b) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;

    stdoutMu.Lock();
    defer(stdoutMu.Unlock());
    return os.Stdout.Write(b);
});

// builderRunTest is the action for running a test binary.
private static error builderRunTest(this ptr<runCache> _addr_c, ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) => func((defer, _, _) => {
    ref runCache c = ref _addr_c.val;
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    if (a.Failed) { 
        // We were unable to build the binary.
        a.Failed = false;
        a.TestOutput = @new<bytes.Buffer>();
        fmt.Fprintf(a.TestOutput, "FAIL\t%s [build failed]\n", a.Package.ImportPath);
        @base.SetExitStatus(1);
        return error.As(null!)!;

    }
    io.Writer stdout = os.Stdout;
    error err = default!;
    if (testJSON) {
        var json = test2json.NewConverter(new lockedStdout(), a.Package.ImportPath, test2json.Timestamp);
        defer(() => {
            json.Exited(err);
            json.Close();
        }());
        stdout = json;
    }
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    if (len(pkgArgs) == 0 || (testBench != "")) { 
        // Stream test output (no buffering) when no package has
        // been given on the command line (implicit current directory)
        // or when benchmarking.
        // No change to stdout.
    }
    else
 { 
        // If we're only running a single package under test or if parallelism is
        // set to 1, and if we're displaying all output (testShowPass), we can
        // hurry the output along, echoing it as soon as it comes in.
        // We still have to copy to &buf for caching the result. This special
        // case was introduced in Go 1.5 and is intentionally undocumented:
        // the exact details of output buffering are up to the go command and
        // subject to change. It would be nice to remove this special case
        // entirely, but it is surely very helpful to see progress being made
        // when tests are run on slow single-CPU ARM systems.
        //
        // If we're showing JSON output, then display output as soon as
        // possible even when multiple tests are being run: the JSON output
        // events are attributed to specific package tests, so interlacing them
        // is OK.
        if (testShowPass() && (len(pkgs) == 1 || cfg.BuildP == 1) || testJSON) { 
            // Write both to stdout and buf, for possible saving
            // to cache, and for looking for the "no tests to run" message.
            stdout = io.MultiWriter(stdout, _addr_buf);

        }
        else
 {
            _addr_stdout = _addr_buf;
            stdout = ref _addr_stdout.val;

        }
    }
    if (c.buf == null) { 
        // We did not find a cached result using the link step action ID,
        // so we ran the link step. Try again now with the link output
        // content ID. The attempt using the action ID makes sure that
        // if the link inputs don't change, we reuse the cached test
        // result without even rerunning the linker. The attempt using
        // the link output (test binary) content ID makes sure that if
        // we have different link inputs but the same final binary,
        // we still reuse the cached test result.
        // c.saveOutput will store the result under both IDs.
        c.tryCacheWithID(b, a, a.Deps[0].BuildContentID());

    }
    if (c.buf != null) {
        if (stdout != _addr_buf) {
            stdout.Write(c.buf.Bytes());
            c.buf.Reset();
        }
        a.TestOutput = c.buf;
        return error.As(null!)!;

    }
    var execCmd = work.FindExecCmd();
    @string testlogArg = new slice<@string>(new @string[] {  });
    if (!c.disableCache && len(execCmd) == 0) {
        testlogArg = new slice<@string>(new @string[] { "-test.testlogfile="+a.Objdir+"testlog.txt" });
    }
    @string panicArg = "-test.paniconexit0";
    var args = str.StringList(execCmd, a.Deps[0].BuiltTarget(), testlogArg, panicArg, testArgs);

    if (testCoverProfile != "") { 
        // Write coverage to temporary profile, for merging later.
        {
            var i__prev1 = i;

            foreach (var (__i, __arg) in args) {
                i = __i;
                arg = __arg;
                if (strings.HasPrefix(arg, "-test.coverprofile=")) {
                    args[i] = "-test.coverprofile=" + a.Objdir + "_cover_.out";
                }
            }

            i = i__prev1;
        }
    }
    if (cfg.BuildN || cfg.BuildX) {
        b.Showcmd("", "%s", strings.Join(args, " "));
        if (cfg.BuildN) {
            return error.As(null!)!;
        }
    }
    var cmd = exec.Command(args[0], args[(int)1..]);
    cmd.Dir = a.Package.Dir;
    cmd.Env = @base.AppendPWD(cfg.OrigEnv.slice(-1, len(cfg.OrigEnv), len(cfg.OrigEnv)), cmd.Dir);
    cmd.Stdout = stdout;
    cmd.Stderr = stdout; 

    // If there are any local SWIG dependencies, we want to load
    // the shared library from the build directory.
    if (a.Package.UsesSwig()) {
        var env = cmd.Env;
        var found = false;
        @string prefix = "LD_LIBRARY_PATH=";
        {
            var i__prev1 = i;

            foreach (var (__i, __v) in env) {
                i = __i;
                v = __v;
                if (strings.HasPrefix(v, prefix)) {
                    env[i] = v + ":.";
                    found = true;
                    break;
                }
            }

            i = i__prev1;
        }

        if (!found) {
            env = append(env, "LD_LIBRARY_PATH=.");
        }
        cmd.Env = env;

    }
    var t0 = time.Now();
    err = error.As(cmd.Start())!; 

    // This is a last-ditch deadline to detect and
    // stop wedged test binaries, to keep the builders
    // running.
    if (err == null) {
        var tick = time.NewTimer(testKillTimeout);
        @base.StartSigHandlers();
        var done = make_channel<error>();
        go_(() => () => {
            done.Send(cmd.Wait());
        }());
Outer:
        if (@base.SignalTrace != null) { 
            // Send a quit signal in the hope that the program will print
            // a stack trace and exit. Give it five seconds before resorting
            // to Kill.
            cmd.Process.Signal(@base.SignalTrace);
            fmt.Fprintf(cmd.Stdout, "*** Test killed with %v: ran too long (%v).\n", @base.SignalTrace, testKillTimeout);
            _breakOuter = true;
            break;
        }
        cmd.Process.Kill();
        err = error.As(done.Receive())!;
        fmt.Fprintf(cmd.Stdout, "*** Test killed: ran too long (%v).\n", testKillTimeout);
        tick.Stop();

    }
    var @out = buf.Bytes();
    _addr_a.TestOutput = _addr_buf;
    a.TestOutput = ref _addr_a.TestOutput.val;
    var t = fmt.Sprintf("%.3fs", time.Since(t0).Seconds());

    mergeCoverProfile(cmd.Stdout, a.Objdir + "_cover_.out");

    if (err == null) {
        @string norun = "";
        if (!testShowPass() && !testJSON) {
            buf.Reset();
        }
        if (bytes.HasPrefix(out, noTestsToRun[(int)1..]) || bytes.Contains(out, noTestsToRun)) {
            norun = " [no tests to run]";
        }
        fmt.Fprintf(cmd.Stdout, "ok  \t%s\t%s%s%s\n", a.Package.ImportPath, t, coveragePercentage(out), norun);
        c.saveOutput(a);

    }
    else
 {
        @base.SetExitStatus(1); 
        // If there was test output, assume we don't need to print the exit status.
        // Buf there's no test output, do print the exit status.
        if (len(out) == 0) {
            fmt.Fprintf(cmd.Stdout, "%s\n", err);
        }
        fmt.Fprintf(cmd.Stdout, "FAIL\t%s\t%s\n", a.Package.ImportPath, t);

    }
    if (cmd.Stdout != _addr_buf) {
        buf.Reset(); // cmd.Stdout was going to os.Stdout already
    }
    return error.As(null!)!;

});

// tryCache is called just before the link attempt,
// to see if the test result is cached and therefore the link is unneeded.
// It reports whether the result can be satisfied from cache.
private static bool tryCache(this ptr<runCache> _addr_c, ptr<work.Builder> _addr_b, ptr<work.Action> _addr_a) {
    ref runCache c = ref _addr_c.val;
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    return c.tryCacheWithID(b, a, a.Deps[0].BuildActionID());
}

private static bool tryCacheWithID(this ptr<runCache> _addr_c, ptr<work.Builder> _addr_b, ptr<work.Action> _addr_a, @string id) {
    ref runCache c = ref _addr_c.val;
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    if (len(pkgArgs) == 0) { 
        // Caching does not apply to "go test",
        // only to "go test foo" (including "go test .").
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: caching disabled in local directory mode\n");
        }
        c.disableCache = true;
        return false;

    }
    if (a.Package.Root == "") { 
        // Caching does not apply to tests outside of any module, GOPATH, or GOROOT.
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: caching disabled for package outside of module root, GOPATH, or GOROOT: %s\n", a.Package.ImportPath);
        }
        c.disableCache = true;
        return false;

    }
    slice<@string> cacheArgs = default;
    foreach (var (_, arg) in testArgs) {
        var i = strings.Index(arg, "=");
        if (i < 0 || !strings.HasPrefix(arg, "-test.")) {
            if (cache.DebugTest) {
                fmt.Fprintf(os.Stderr, "testcache: caching disabled for test argument: %s\n", arg);
            }
            c.disableCache = true;
            return false;
        }
        switch (arg[..(int)i]) {
            case "-test.benchtime": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.cpu": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.list": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.parallel": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.run": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.short": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.timeout": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.

            case "-test.v": 
                // These are cacheable.
                // Note that this list is documented above,
                // so if you add to this list, update the docs too.
                cacheArgs = append(cacheArgs, arg);
                break;
            default: 
                // nothing else is cacheable
                if (cache.DebugTest) {
                    fmt.Fprintf(os.Stderr, "testcache: caching disabled for test argument: %s\n", arg);
                }
                c.disableCache = true;
                return false;
                break;
        }

    }    if (cache.Default() == null) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: GOCACHE=off\n");
        }
        c.disableCache = true;
        return false;

    }
    var h = cache.NewHash("testResult");
    fmt.Fprintf(h, "test binary %s args %q execcmd %q", id, cacheArgs, work.ExecCmd);
    var testID = h.Sum();
    if (c.id1 == (new cache.ActionID())) {
        c.id1 = testID;
    }
    else
 {
        c.id2 = testID;
    }
    if (cache.DebugTest) {
        fmt.Fprintf(os.Stderr, "testcache: %s: test ID %x => %x\n", a.Package.ImportPath, id, testID);
    }
    var (data, entry, err) = cache.Default().GetBytes(testID);
    if (!bytes.HasPrefix(data, testlogMagic) || data[len(data) - 1] != '\n') {
        if (cache.DebugTest) {
            if (err != null) {
                fmt.Fprintf(os.Stderr, "testcache: %s: input list not found: %v\n", a.Package.ImportPath, err);
            }
            else
 {
                fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed\n", a.Package.ImportPath);
            }

        }
        return false;

    }
    var (testInputsID, err) = computeTestInputsID(_addr_a, data);
    if (err != null) {
        return false;
    }
    if (cache.DebugTest) {
        fmt.Fprintf(os.Stderr, "testcache: %s: test ID %x => input ID %x => %x\n", a.Package.ImportPath, testID, testInputsID, testAndInputKey(testID, testInputsID));
    }
    data, entry, err = cache.Default().GetBytes(testAndInputKey(testID, testInputsID));
    if (len(data) == 0 || data[len(data) - 1] != '\n') {
        if (cache.DebugTest) {
            if (err != null) {
                fmt.Fprintf(os.Stderr, "testcache: %s: test output not found: %v\n", a.Package.ImportPath, err);
            }
            else
 {
                fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
            }

        }
        return false;

    }
    if (entry.Time.Before(testCacheExpire)) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: %s: test output expired due to go clean -testcache\n", a.Package.ImportPath);
        }
        return false;

    }
    i = bytes.LastIndexByte(data[..(int)len(data) - 1], '\n') + 1;
    if (!bytes.HasPrefix(data[(int)i..], (slice<byte>)"ok  \t")) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
        }
        return false;

    }
    var j = bytes.IndexByte(data[(int)i + len("ok  \t")..], '\t');
    if (j < 0) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
        }
        return false;

    }
    j += i + len("ok  \t") + 1; 

    // Committed to printing.
    c.buf = @new<bytes.Buffer>();
    c.buf.Write(data[..(int)j]);
    c.buf.WriteString("(cached)");
    while (j < len(data) && ('0' <= data[j] && data[j] <= '9' || data[j] == '.' || data[j] == 's')) {
        j++;
    }
    c.buf.Write(data[(int)j..]);
    return true;

}

private static var errBadTestInputs = errors.New("error parsing test inputs");
private static slice<byte> testlogMagic = (slice<byte>)"# test log\n"; // known to testing/internal/testdeps/deps.go

// computeTestInputsID computes the "test inputs ID"
// (see comment in tryCacheWithID above) for the
// test log.
private static (cache.ActionID, error) computeTestInputsID(ptr<work.Action> _addr_a, slice<byte> testlog) {
    cache.ActionID _p0 = default;
    error _p0 = default!;
    ref work.Action a = ref _addr_a.val;

    testlog = bytes.TrimPrefix(testlog, testlogMagic);
    var h = cache.NewHash("testInputs");
    var pwd = a.Package.Dir;
    foreach (var (_, line) in bytes.Split(testlog, (slice<byte>)"\n")) {
        if (len(line) == 0) {
            continue;
        }
        var s = string(line);
        var i = strings.Index(s, " ");
        if (i < 0) {
            if (cache.DebugTest) {
                fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed (%q)\n", a.Package.ImportPath, line);
            }
            return (new cache.ActionID(), error.As(errBadTestInputs)!);
        }
        var op = s[..(int)i];
        var name = s[(int)i + 1..];
        switch (op) {
            case "getenv": 
                fmt.Fprintf(h, "env %s %x\n", name, hashGetenv(name));
                break;
            case "chdir": 
                pwd = name; // always absolute
                fmt.Fprintf(h, "chdir %s %x\n", name, hashStat(name));

                break;
            case "stat": 
                if (!filepath.IsAbs(name)) {
                    name = filepath.Join(pwd, name);
                }
                if (a.Package.Root == "" || search.InDir(name, a.Package.Root) == "") { 
                    // Do not recheck files outside the module, GOPATH, or GOROOT root.
                    break;

                }

                fmt.Fprintf(h, "stat %s %x\n", name, hashStat(name));

                break;
            case "open": 
                if (!filepath.IsAbs(name)) {
                    name = filepath.Join(pwd, name);
                }
                if (a.Package.Root == "" || search.InDir(name, a.Package.Root) == "") { 
                    // Do not recheck files outside the module, GOPATH, or GOROOT root.
                    break;

                }

                var (fh, err) = hashOpen(name);
                if (err != null) {
                    if (cache.DebugTest) {
                        fmt.Fprintf(os.Stderr, "testcache: %s: input file %s: %s\n", a.Package.ImportPath, name, err);
                    }
                    return (new cache.ActionID(), error.As(err)!);
                }

                fmt.Fprintf(h, "open %s %x\n", name, fh);

                break;
            default: 
                if (cache.DebugTest) {
                    fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed (%q)\n", a.Package.ImportPath, line);
                }
                return (new cache.ActionID(), error.As(errBadTestInputs)!);
                break;
        }

    }    var sum = h.Sum();
    return (sum, error.As(null!)!);

}

private static cache.ActionID hashGetenv(@string name) {
    var h = cache.NewHash("getenv");
    var (v, ok) = os.LookupEnv(name);
    if (!ok) {
        h.Write(new slice<byte>(new byte[] { 0 }));
    }
    else
 {
        h.Write(new slice<byte>(new byte[] { 1 }));
        h.Write((slice<byte>)v);
    }
    return h.Sum();

}

private static readonly nint modTimeCutoff = 2 * time.Second;



private static var errFileTooNew = errors.New("file used as input is too new");

private static (cache.ActionID, error) hashOpen(@string name) {
    cache.ActionID _p0 = default;
    error _p0 = default!;

    var h = cache.NewHash("open");
    var (info, err) = os.Stat(name);
    if (err != null) {
        fmt.Fprintf(h, "err %v\n", err);
        return (h.Sum(), error.As(null!)!);
    }
    hashWriteStat(h, info);
    if (info.IsDir()) {
        var (files, err) = os.ReadDir(name);
        if (err != null) {
            fmt.Fprintf(h, "err %v\n", err);
        }
        foreach (var (_, f) in files) {
            fmt.Fprintf(h, "file %s ", f.Name());
            var (finfo, err) = f.Info();
            if (err != null) {
                fmt.Fprintf(h, "err %v\n", err);
            }
            else
 {
                hashWriteStat(h, finfo);
            }

        }
    }
    else if (info.Mode().IsRegular()) { 
        // Because files might be very large, do not attempt
        // to hash the entirety of their content. Instead assume
        // the mtime and size recorded in hashWriteStat above
        // are good enough.
        //
        // To avoid problems for very recent files where a new
        // write might not change the mtime due to file system
        // mtime precision, reject caching if a file was read that
        // is less than modTimeCutoff old.
        if (time.Since(info.ModTime()) < modTimeCutoff) {
            return (new cache.ActionID(), error.As(errFileTooNew)!);
        }
    }
    return (h.Sum(), error.As(null!)!);

}

private static cache.ActionID hashStat(@string name) {
    var h = cache.NewHash("stat");
    {
        var info__prev1 = info;

        var (info, err) = os.Stat(name);

        if (err != null) {
            fmt.Fprintf(h, "err %v\n", err);
        }
        else
 {
            hashWriteStat(h, info);
        }
        info = info__prev1;

    }

    {
        var info__prev1 = info;

        (info, err) = os.Lstat(name);

        if (err != null) {
            fmt.Fprintf(h, "err %v\n", err);
        }
        else
 {
            hashWriteStat(h, info);
        }
        info = info__prev1;

    }

    return h.Sum();

}

private static void hashWriteStat(io.Writer h, fs.FileInfo info) {
    fmt.Fprintf(h, "stat %d %x %v %v\n", info.Size(), uint64(info.Mode()), info.ModTime(), info.IsDir());
}

// testAndInputKey returns the actual cache key for the pair (testID, testInputsID).
private static cache.ActionID testAndInputKey(cache.ActionID testID, cache.ActionID testInputsID) {
    return cache.Subkey(testID, fmt.Sprintf("inputs:%x", testInputsID));
}

private static void saveOutput(this ptr<runCache> _addr_c, ptr<work.Action> _addr_a) {
    ref runCache c = ref _addr_c.val;
    ref work.Action a = ref _addr_a.val;

    if (c.id1 == (new cache.ActionID()) && c.id2 == (new cache.ActionID())) {
        return ;
    }
    var (testlog, err) = os.ReadFile(a.Objdir + "testlog.txt");
    if (err != null || !bytes.HasPrefix(testlog, testlogMagic) || testlog[len(testlog) - 1] != '\n') {
        if (cache.DebugTest) {
            if (err != null) {
                fmt.Fprintf(os.Stderr, "testcache: %s: reading testlog: %v\n", a.Package.ImportPath, err);
            }
            else
 {
                fmt.Fprintf(os.Stderr, "testcache: %s: reading testlog: malformed\n", a.Package.ImportPath);
            }

        }
        return ;

    }
    var (testInputsID, err) = computeTestInputsID(_addr_a, testlog);
    if (err != null) {
        return ;
    }
    if (c.id1 != (new cache.ActionID())) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: %s: save test ID %x => input ID %x => %x\n", a.Package.ImportPath, c.id1, testInputsID, testAndInputKey(c.id1, testInputsID));
        }
        cache.Default().PutNoVerify(c.id1, bytes.NewReader(testlog));
        cache.Default().PutNoVerify(testAndInputKey(c.id1, testInputsID), bytes.NewReader(a.TestOutput.Bytes()));

    }
    if (c.id2 != (new cache.ActionID())) {
        if (cache.DebugTest) {
            fmt.Fprintf(os.Stderr, "testcache: %s: save test ID %x => input ID %x => %x\n", a.Package.ImportPath, c.id2, testInputsID, testAndInputKey(c.id2, testInputsID));
        }
        cache.Default().PutNoVerify(c.id2, bytes.NewReader(testlog));
        cache.Default().PutNoVerify(testAndInputKey(c.id2, testInputsID), bytes.NewReader(a.TestOutput.Bytes()));

    }
}

// coveragePercentage returns the coverage results (if enabled) for the
// test. It uncovers the data by scanning the output from the test run.
private static @string coveragePercentage(slice<byte> @out) {
    if (!testCover) {
        return "";
    }
    var re = regexp.MustCompile("coverage: (.*)\\n");
    var matches = re.FindSubmatch(out);
    if (matches == null) { 
        // Probably running "go test -cover" not "go test -cover fmt".
        // The coverage output will appear in the output directly.
        return "";

    }
    return fmt.Sprintf("\tcoverage: %s", matches[1]);

}

// builderCleanTest is the action for cleaning up after a test.
private static error builderCleanTest(ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) {
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    if (cfg.BuildWork) {
        return error.As(null!)!;
    }
    if (cfg.BuildX) {
        b.Showcmd("", "rm -r %s", a.Objdir);
    }
    os.RemoveAll(a.Objdir);
    return error.As(null!)!;

}

// builderPrintTest is the action for printing a test result.
private static error builderPrintTest(ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) {
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    var clean = a.Deps[0];
    var run = clean.Deps[0];
    if (run.TestOutput != null) {
        os.Stdout.Write(run.TestOutput.Bytes());
        run.TestOutput = null;
    }
    return error.As(null!)!;

}

// builderNoTest is the action for testing a package with no test files.
private static error builderNoTest(ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) => func((defer, _, _) => {
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    io.Writer stdout = os.Stdout;
    if (testJSON) {
        var json = test2json.NewConverter(new lockedStdout(), a.Package.ImportPath, test2json.Timestamp);
        defer(json.Close());
        stdout = json;
    }
    fmt.Fprintf(stdout, "?   \t%s\t[no test files]\n", a.Package.ImportPath);
    return error.As(null!)!;

});

// printExitStatus is the action for printing the exit status
private static error printExitStatus(ptr<work.Builder> _addr_b, context.Context ctx, ptr<work.Action> _addr_a) {
    ref work.Builder b = ref _addr_b.val;
    ref work.Action a = ref _addr_a.val;

    if (!testJSON && len(pkgArgs) != 0) {
        if (@base.GetExitStatus() != 0) {
            fmt.Println("FAIL");
            return error.As(null!)!;
        }
    }
    return error.As(null!)!;

}

} // end test_package
