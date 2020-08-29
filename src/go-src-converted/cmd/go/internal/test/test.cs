// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package test -- go2cs converted at 2020 August 29 10:01:46 UTC
// import "cmd/go/internal/test" ==> using test = go.cmd.go.@internal.test_package
// Original source: C:\Go\src\cmd\go\internal\test\test.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using doc = go.go.doc_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using template = go.text.template_package;
using time = go.time_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using work = go.cmd.go.@internal.work_package;
using test2json = go.cmd.@internal.test2json_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class test_package
    {
        // Break init loop.
        private static void init()
        {
            CmdTest.Run = runTest;
        }

        private static readonly @string testUsage = "test [build/test flags] [packages] [build/test flags & test binary flags]";



        public static base.Command CmdTest = ref new base.Command(CustomFlags:true,UsageLine:testUsage,Short:"test packages",Long:`
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
finds any problems, go test reports those and does not run the test binary.
Only a high-confidence subset of the default go vet checks are used.
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
requested benchmark results or verbose logging.

In package list mode only, go test caches successful package test
results to avoid unnecessary repeated running of tests. When the
result of a test can be recovered from the cache, go test will
redisplay the previous output instead of running the test binary
again. When this happens, go test prints '(cached)' in place of the
elapsed time in the summary line.

The rule for a match in the cache is that the run involves the same
test binary and the flags on the command line come entirely from a
restricted set of 'cacheable' test flags, defined as -cpu, -list,
-parallel, -run, -short, and -v. If a run of go test has any test
or non-test flags outside this set, the result is not cached. To
disable test caching, use any test flag or argument other than the
cacheable flags. The idiomatic way to disable test caching explicitly
is to use -count=1. Tests that open files within the package's source
root (usually $GOPATH) or that consult environment variables only
match future runs in which the files and environment variables are unchanged.
A cached test result is treated as executing in no time at all,
so a successful package test result will be cached and reused
regardless of -timeout setting.

`+strings.TrimSpace(testFlag1)+` See 'go help testflag' for details.

For more about build flags, see 'go help build'.
For more about specifying packages, see 'go help packages'.

See also: go build, go vet.
`,);

        private static readonly @string testFlag1 = @"
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

	-json
	    Convert test output to JSON suitable for automated processing.
	    See 'go doc test2json' for the encoding details.

	-o file
	    Compile the test binary to the named file.
	    The test still runs (unless -c or -i is specified).

The test binary also accepts flags that control execution of the test; these
flags are also accessible by 'go test'.
";

        // Usage prints the usage message for 'go test -h' and exits.


        // Usage prints the usage message for 'go test -h' and exits.
        public static void Usage()
        {
            os.Stderr.WriteString(testUsage + "\n\n" + strings.TrimSpace(testFlag1) + "\n\n\t" + strings.TrimSpace(testFlag2) + "\n");
            os.Exit(2L);
        }

        public static base.Command HelpTestflag = ref new base.Command(UsageLine:"testflag",Short:"testing flags",Long:`
The 'go test' command takes both flags that apply to 'go test' itself
and flags that apply to the resulting test binary.

Several of the flags control profiling and write an execution profile
suitable for "go tool pprof"; run "go tool pprof -h" for more
information. The --alloc_space, --alloc_objects, and --show_bytes
options of pprof control how the information is presented.

The following flags are recognized by the 'go test' command and
control the execution of any test:

	`+strings.TrimSpace(testFlag2)+`
`,);

        private static readonly @string testFlag2 = "\n\t-bench regexp\n\t    Run only those benchmarks matching a regular expression.\n\t  " +
    "  By default, no benchmarks are run.\n\t    To run all benchmarks, use \'-bench .\' " +
    "or \'-bench=.\'.\n\t    The regular expression is split by unbracketed slash (/)\n\t  " +
    "  characters into a sequence of regular expressions, and each\n\t    part of a ben" +
    "chmark\'s identifier must match the corresponding\n\t    element in the sequence, i" +
    "f any. Possible parents of matches\n\t    are run with b.N=1 to identify sub-bench" +
    "marks. For example,\n\t    given -bench=X/Y, top-level benchmarks matching X are r" +
    "un\n\t    with b.N=1 to find any sub-benchmarks matching Y, which are\n\t    then ru" +
    "n in full.\n\n\t-benchtime t\n\t    Run enough iterations of each benchmark to take t" +
    ", specified\n\t    as a time.Duration (for example, -benchtime 1h30s).\n\t    The de" +
    "fault is 1 second (1s).\n\n\t-count n\n\t    Run each test and benchmark n times (def" +
    "ault 1).\n\t    If -cpu is set, run n times for each GOMAXPROCS value.\n\t    Exampl" +
    "es are always run once.\n\n\t-cover\n\t    Enable coverage analysis.\n\t    Note that b" +
    "ecause coverage works by annotating the source\n\t    code before compilation, com" +
    "pilation and test failures with\n\t    coverage enabled may report line numbers th" +
    "at don\'t correspond\n\t    to the original sources.\n\n\t-covermode set,count,atomic\n" +
    "\t    Set the mode for coverage analysis for the package[s]\n\t    being tested. Th" +
    "e default is \"set\" unless -race is enabled,\n\t    in which case it is \"atomic\".\n\t" +
    "    The values:\n\t\tset: bool: does this statement run?\n\t\tcount: int: how many tim" +
    "es does this statement run?\n\t\tatomic: int: count, but correct in multithreaded t" +
    "ests;\n\t\t\tsignificantly more expensive.\n\t    Sets -cover.\n\n\t-coverpkg pattern1,pa" +
    "ttern2,pattern3\n\t    Apply coverage analysis in each test to packages matching t" +
    "he patterns.\n\t    The default is for each test to analyze only the package being" +
    " tested.\n\t    See \'go help packages\' for a description of package patterns.\n\t   " +
    " Sets -cover.\n\n\t-cpu 1,2,4\n\t    Specify a list of GOMAXPROCS values for which th" +
    "e tests or\n\t    benchmarks should be executed. The default is the current value\n" +
    "\t    of GOMAXPROCS.\n\n\t-failfast\n\t    Do not start new tests after the first test" +
    " failure.\n\n\t-list regexp\n\t    List tests, benchmarks, or examples matching the r" +
    "egular expression.\n\t    No tests, benchmarks or examples will be run. This will " +
    "only\n\t    list top-level tests. No subtest or subbenchmarks will be shown.\n\n\t-pa" +
    "rallel n\n\t    Allow parallel execution of test functions that call t.Parallel.\n\t" +
    "    The value of this flag is the maximum number of tests to run\n\t    simultaneo" +
    "usly; by default, it is set to the value of GOMAXPROCS.\n\t    Note that -parallel" +
    " only applies within a single test binary.\n\t    The \'go test\' command may run te" +
    "sts for different packages\n\t    in parallel as well, according to the setting of" +
    " the -p flag\n\t    (see \'go help build\').\n\n\t-run regexp\n\t    Run only those tests" +
    " and examples matching the regular expression.\n\t    For tests, the regular expre" +
    "ssion is split by unbracketed slash (/)\n\t    characters into a sequence of regul" +
    "ar expressions, and each part\n\t    of a test\'s identifier must match the corresp" +
    "onding element in\n\t    the sequence, if any. Note that possible parents of match" +
    "es are\n\t    run too, so that -run=X/Y matches and runs and reports the result\n\t " +
    "   of all tests matching X, even those without sub-tests matching Y,\n\t    becaus" +
    "e it must run them to look for those sub-tests.\n\n\t-short\n\t    Tell long-running " +
    "tests to shorten their run time.\n\t    It is off by default but set during all.ba" +
    "sh so that installing\n\t    the Go tree can run a sanity check but not spend time" +
    " running\n\t    exhaustive tests.\n\n\t-timeout d\n\t    If a test binary runs longer t" +
    "han duration d, panic.\n\t    If d is 0, the timeout is disabled.\n\t    The default" +
    " is 10 minutes (10m).\n\n\t-v\n\t    Verbose output: log all tests as they are run. A" +
    "lso print all\n\t    text from Log and Logf calls even if the test succeeds.\n\n\t-ve" +
    "t list\n\t    Configure the invocation of \"go vet\" during \"go test\"\n\t    to use th" +
    "e comma-separated list of vet checks.\n\t    If list is empty, \"go test\" runs \"go " +
    "vet\" with a curated list of\n\t    checks believed to be always worth addressing.\n" +
    "\t    If list is \"off\", \"go test\" does not run \"go vet\" at all.\n\nThe following fl" +
    "ags are also recognized by \'go test\' and can be used to\nprofile the tests during" +
    " execution:\n\n\t-benchmem\n\t    Print memory allocation statistics for benchmarks.\n" +
    "\n\t-blockprofile block.out\n\t    Write a goroutine blocking profile to the specifi" +
    "ed file\n\t    when all tests are complete.\n\t    Writes test binary as -c would.\n\n" +
    "\t-blockprofilerate n\n\t    Control the detail provided in goroutine blocking prof" +
    "iles by\n\t    calling runtime.SetBlockProfileRate with n.\n\t    See \'go doc runtim" +
    "e.SetBlockProfileRate\'.\n\t    The profiler aims to sample, on average, one blocki" +
    "ng event every\n\t    n nanoseconds the program spends blocked. By default,\n\t    i" +
    "f -test.blockprofile is set without this flag, all blocking events\n\t    are reco" +
    "rded, equivalent to -test.blockprofilerate=1.\n\n\t-coverprofile cover.out\n\t    Wri" +
    "te a coverage profile to the file after all tests have passed.\n\t    Sets -cover." +
    "\n\n\t-cpuprofile cpu.out\n\t    Write a CPU profile to the specified file before exi" +
    "ting.\n\t    Writes test binary as -c would.\n\n\t-memprofile mem.out\n\t    Write a me" +
    "mory profile to the file after all tests have passed.\n\t    Writes test binary as" +
    " -c would.\n\n\t-memprofilerate n\n\t    Enable more precise (and expensive) memory p" +
    "rofiles by setting\n\t    runtime.MemProfileRate. See \'go doc runtime.MemProfileRa" +
    "te\'.\n\t    To profile all memory allocations, use -test.memprofilerate=1\n\t    and" +
    " pass --alloc_space flag to the pprof tool.\n\n\t-mutexprofile mutex.out\n\t    Write" +
    " a mutex contention profile to the specified file\n\t    when all tests are comple" +
    "te.\n\t    Writes test binary as -c would.\n\n\t-mutexprofilefraction n\n\t    Sample 1" +
    " in n stack traces of goroutines holding a\n\t    contended mutex.\n\n\t-outputdir di" +
    "rectory\n\t    Place output files from profiling in the specified directory,\n\t    " +
    "by default the directory in which \"go test\" is running.\n\n\t-trace trace.out\n\t    " +
    "Write an execution trace to the specified file before exiting.\n\nEach of these fl" +
    "ags is also recognized with an optional \'test.\' prefix,\nas in -test.v. When invo" +
    "king the generated test binary (the result of\n\'go test -c\') directly, however, t" +
    "he prefix is mandatory.\n\nThe \'go test\' command rewrites or removes recognized fl" +
    "ags,\nas appropriate, both before and after the optional package list,\nbefore inv" +
    "oking the test binary.\n\nFor instance, the command\n\n\tgo test -v -myflag testdata " +
    "-cpuprofile=prof.out -x\n\nwill compile the test binary and then run it as\n\n\tpkg.t" +
    "est -test.v -myflag testdata -test.cpuprofile=prof.out\n\n(The -x flag is removed " +
    "because it applies only to the go command\'s\nexecution, not to the test itself.)\n" +
    "\nThe test flags that generate profiles (other than for coverage) also\nleave the " +
    "test binary in pkg.test for use when analyzing the profiles.\n\nWhen \'go test\' run" +
    "s a test binary, it does so from within the\ncorresponding package\'s source code " +
    "directory. Depending on the test,\nit may be necessary to do the same when invoki" +
    "ng a generated test\nbinary directly.\n\nThe command-line package list, if present," +
    " must appear before any\nflag not known to the go test command. Continuing the ex" +
    "ample above,\nthe package list would have to appear before -myflag, but could app" +
    "ear\non either side of -v.\n\nTo keep an argument for a test binary from being inte" +
    "rpreted as a\nknown flag or a package name, use -args (see \'go help test\') which\n" +
    "passes the remainder of the command line through to the test binary\nuninterprete" +
    "d and unaltered.\n\nFor instance, the command\n\n\tgo test -v -args -x -v\n\nwill compi" +
    "le the test binary and then run it as\n\n\tpkg.test -test.v -x -v\n\nSimilarly,\n\n\tgo " +
    "test -args math\n\nwill compile the test binary and then run it as\n\n\tpkg.test math" +
    "\n\nIn the first example, the -x and the second -v are passed through to the\ntest " +
    "binary unchanged and with no effect on the go command itself.\nIn the second exam" +
    "ple, the argument math is passed through to the test\nbinary, instead of being in" +
    "terpreted as the package list.\n";



        public static base.Command HelpTestfunc = ref new base.Command(UsageLine:"testfunc",Short:"testing functions",Long:`
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
`,);

        private static bool testC = default;        private static bool testCover = default;        private static @string testCoverMode = default;        private static slice<@string> testCoverPaths = default;        private static slice<ref load.Package> testCoverPkgs = default;        private static @string testCoverProfile = default;        private static @string testOutputDir = default;        private static @string testO = default;        private static @string testProfile = default;        private static bool testNeedBinary = default;        private static bool testJSON = default;        private static bool testV = default;        private static @string testTimeout = default;        private static slice<@string> testArgs = default;        private static bool testBench = default;        private static bool testList = default;        private static bool testShowPass = default;        private static @string testVetList = default;        private static slice<@string> pkgArgs = default;        private static slice<ref load.Package> pkgs = default;        private static long testKillTimeout = 10L * time.Minute;        private static time.Time testCacheExpire = default;

        private static @string testMainDeps = new slice<@string>(new @string[] { "os", "testing", "testing/internal/testdeps" });

        // testVetFlags is the list of flags to pass to vet when invoked automatically during go test.
        private static @string testVetFlags = new slice<@string>(new @string[] { "-atomic", "-bool", "-buildtags", "-nilfunc", "-printf" });

        private static void runTest(ref base.Command _cmd, slice<@string> args) => func(_cmd, (ref base.Command cmd, Defer defer, Panic _, Recover __) =>
        {
            pkgArgs, testArgs = testFlags(args);

            work.FindExecCmd(); // initialize cached result

            work.BuildInit();
            work.VetFlags = testVetFlags;

            pkgs = load.PackagesForBuild(pkgArgs);
            if (len(pkgs) == 0L)
            {
                @base.Fatalf("no packages to test");
            }
            if (testC && len(pkgs) != 1L)
            {
                @base.Fatalf("cannot use -c flag with multiple packages");
            }
            if (testO != "" && len(pkgs) != 1L)
            {
                @base.Fatalf("cannot use -o flag with multiple packages");
            }
            if (testProfile != "" && len(pkgs) != 1L)
            {
                @base.Fatalf("cannot use %s flag with multiple packages", testProfile);
            }
            initCoverProfile();
            defer(closeCoverProfile()); 

            // If a test timeout was given and is parseable, set our kill timeout
            // to that timeout plus one minute. This is a backup alarm in case
            // the test wedges with a goroutine spinning and its background
            // timer does not get a chance to fire.
            {
                var (dt, err) = time.ParseDuration(testTimeout);

                if (err == null && dt > 0L)
                {
                    testKillTimeout = dt + 1L * time.Minute;
                }
                else if (err == null && dt == 0L)
                { 
                    // An explicit zero disables the test timeout.
                    // Let it have one century (almost) before we kill it.
                    testKillTimeout = 100L * 365L * 24L * time.Hour;
                } 

                // show passing test output (after buffering) with -v flag.
                // must buffer because tests are running in parallel, and
                // otherwise the output will get mixed.

            } 

            // show passing test output (after buffering) with -v flag.
            // must buffer because tests are running in parallel, and
            // otherwise the output will get mixed.
            testShowPass = testV || testList; 

            // For 'go test -i -o x.test', we want to build x.test. Imply -c to make the logic easier.
            if (cfg.BuildI && testO != "")
            {
                testC = true;
            } 

            // Read testcache expiration time, if present.
            // (We implement go clean -testcache by writing an expiration date
            // instead of searching out and deleting test result cache entries.)
            {
                var dir = cache.DefaultDir();

                if (dir != "off")
                {
                    {
                        var (data, _) = ioutil.ReadFile(filepath.Join(dir, "testexpire.txt"));

                        if (len(data) > 0L && data[len(data) - 1L] == '\n')
                        {
                            {
                                var (t, err) = strconv.ParseInt(string(data[..len(data) - 1L]), 10L, 64L);

                                if (err == null)
                                {
                                    testCacheExpire = time.Unix(0L, t);
                                }

                            }
                        }

                    }
                }

            }

            work.Builder b = default;
            b.Init();

            if (cfg.BuildI)
            {
                cfg.BuildV = testV;

                var deps = make_map<@string, bool>();
                foreach (var (_, dep) in testMainDeps)
                {
                    deps[dep] = true;
                }
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in pkgs)
                    {
                        p = __p; 
                        // Dependencies for each test.
                        {
                            var path__prev2 = path;

                            foreach (var (_, __path) in p.Imports)
                            {
                                path = __path;
                                deps[path] = true;
                            }

                            path = path__prev2;
                        }

                        {
                            var path__prev2 = path;

                            foreach (var (_, __path) in p.Vendored(p.TestImports))
                            {
                                path = __path;
                                deps[path] = true;
                            }

                            path = path__prev2;
                        }

                        {
                            var path__prev2 = path;

                            foreach (var (_, __path) in p.Vendored(p.XTestImports))
                            {
                                path = __path;
                                deps[path] = true;
                            }

                            path = path__prev2;
                        }

                    } 

                    // translate C to runtime/cgo

                    p = p__prev1;
                }

                if (deps["C"])
                {
                    delete(deps, "C");
                    deps["runtime/cgo"] = true;
                } 
                // Ignore pseudo-packages.
                delete(deps, "unsafe");

                @string all = new slice<@string>(new @string[] {  });
                {
                    var path__prev1 = path;

                    foreach (var (__path) in deps)
                    {
                        path = __path;
                        if (!build.IsLocalImport(path))
                        {
                            all = append(all, path);
                        }
                    }

                    path = path__prev1;
                }

                sort.Strings(all);

                work.Action a = ref new work.Action(Mode:"go test -i");
                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in load.PackagesForBuild(all))
                    {
                        p = __p;
                        a.Deps = append(a.Deps, b.CompileAction(work.ModeInstall, work.ModeInstall, p));
                    }

                    p = p__prev1;
                }

                b.Do(a);
                if (!testC || a.Failed)
                {
                    return;
                }
                b.Init();
            }
            slice<ref work.Action> builds = default;            slice<ref work.Action> runs = default;            slice<ref work.Action> prints = default;



            if (testCoverPaths != null)
            {
                var match = make_slice<Func<ref load.Package, bool>>(len(testCoverPaths));
                var matched = make_slice<bool>(len(testCoverPaths));
                {
                    var i__prev1 = i;

                    foreach (var (__i) in testCoverPaths)
                    {
                        i = __i;
                        match[i] = load.MatchPackage(testCoverPaths[i], @base.Cwd);
                    } 

                    // Select for coverage all dependencies matching the testCoverPaths patterns.

                    i = i__prev1;
                }

                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in load.PackageList(pkgs))
                    {
                        p = __p;
                        var haveMatch = false;
                        {
                            var i__prev2 = i;

                            foreach (var (__i) in testCoverPaths)
                            {
                                i = __i;
                                if (match[i](p))
                                {
                                    matched[i] = true;
                                    haveMatch = true;
                                }
                            } 

                            // Silently ignore attempts to run coverage on
                            // sync/atomic when using atomic coverage mode.
                            // Atomic coverage mode uses sync/atomic, so
                            // we can't also do coverage on it.

                            i = i__prev2;
                        }

                        if (testCoverMode == "atomic" && p.Standard && p.ImportPath == "sync/atomic")
                        {
                            continue;
                        } 

                        // If using the race detector, silently ignore
                        // attempts to run coverage on the runtime
                        // packages. It will cause the race detector
                        // to be invoked before it has been initialized.
                        if (cfg.BuildRace && p.Standard && (p.ImportPath == "runtime" || strings.HasPrefix(p.ImportPath, "runtime/internal")))
                        {
                            continue;
                        }
                        if (haveMatch)
                        {
                            testCoverPkgs = append(testCoverPkgs, p);
                        }
                    } 

                    // Warn about -coverpkg arguments that are not actually used.

                    p = p__prev1;
                }

                {
                    var i__prev1 = i;

                    foreach (var (__i) in testCoverPaths)
                    {
                        i = __i;
                        if (!matched[i])
                        {
                            fmt.Fprintf(os.Stderr, "warning: no packages being tested depend on matches for pattern %s\n", testCoverPaths[i]);
                        }
                    } 

                    // Mark all the coverage packages for rebuilding with coverage.

                    i = i__prev1;
                }

                {
                    var p__prev1 = p;

                    foreach (var (_, __p) in testCoverPkgs)
                    {
                        p = __p; 
                        // There is nothing to cover in package unsafe; it comes from the compiler.
                        if (p.ImportPath == "unsafe")
                        {
                            continue;
                        }
                        p.Internal.CoverMode = testCoverMode;
                        slice<@string> coverFiles = default;
                        coverFiles = append(coverFiles, p.GoFiles);
                        coverFiles = append(coverFiles, p.CgoFiles);
                        coverFiles = append(coverFiles, p.TestGoFiles);
                        p.Internal.CoverVars = declareCoverVars(p.ImportPath, coverFiles);
                        if (testCover && testCoverMode == "atomic")
                        {
                            ensureImport(p, "sync/atomic");
                        }
                    }

                    p = p__prev1;
                }

            } 

            // Prepare build + run + print actions for all packages being tested.
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs)
                {
                    p = __p; 
                    // sync/atomic import is inserted by the cover tool. See #18486
                    if (testCover && testCoverMode == "atomic")
                    {
                        ensureImport(p, "sync/atomic");
                    }
                    var (buildTest, runTest, printTest, err) = builderTest(ref b, p);
                    if (err != null)
                    {
                        var str = err.Error();
                        if (strings.HasPrefix(str, "\n"))
                        {
                            str = str[1L..];
                        }
                        var failed = fmt.Sprintf("FAIL\t%s [setup failed]\n", p.ImportPath);

                        if (p.ImportPath != "")
                        {
                            @base.Errorf("# %s\n%s\n%s", p.ImportPath, str, failed);
                        }
                        else
                        {
                            @base.Errorf("%s\n%s", str, failed);
                        }
                        continue;
                    }
                    builds = append(builds, buildTest);
                    runs = append(runs, runTest);
                    prints = append(prints, printTest);
                } 

                // Ultimately the goal is to print the output.

                p = p__prev1;
            }

            work.Action root = ref new work.Action(Mode:"go test",Deps:prints); 

            // Force the printing of results to happen in order,
            // one at a time.
            {
                var i__prev1 = i;
                work.Action a__prev1 = a;

                foreach (var (__i, __a) in prints)
                {
                    i = __i;
                    a = __a;
                    if (i > 0L)
                    {
                        a.Deps = append(a.Deps, prints[i - 1L]);
                    }
                } 

                // Force benchmarks to run in serial.

                i = i__prev1;
                a = a__prev1;
            }

            if (!testC && testBench)
            { 
                // The first run must wait for all builds.
                // Later runs must wait for the previous run's print.
                {
                    var i__prev1 = i;

                    foreach (var (__i, __run) in runs)
                    {
                        i = __i;
                        run = __run;
                        if (i == 0L)
                        {
                            run.Deps = append(run.Deps, builds);
                        }
                        else
                        {
                            run.Deps = append(run.Deps, prints[i - 1L]);
                        }
                    }

                    i = i__prev1;
                }

            }
            b.Do(root);
        });

        // ensures that package p imports the named package
        private static void ensureImport(ref load.Package p, @string pkg)
        {
            foreach (var (_, d) in p.Internal.Imports)
            {
                if (d.Name == pkg)
                {
                    return;
                }
            }
            var p1 = load.LoadPackage(pkg, ref new load.ImportStack());
            if (p1.Error != null)
            {
                @base.Fatalf("load %s: %v", pkg, p1.Error);
            }
            p.Internal.Imports = append(p.Internal.Imports, p1);
        }

        private static @string windowsBadWords = new slice<@string>(new @string[] { "install", "patch", "setup", "update" });

        private static (ref work.Action, ref work.Action, ref work.Action, error) builderTest(ref work.Builder b, ref load.Package p)
        {
            if (len(p.TestGoFiles) + len(p.XTestGoFiles) == 0L)
            {
                var build = b.CompileAction(work.ModeBuild, work.ModeBuild, p);
                work.Action run = ref new work.Action(Mode:"test run",Package:p,Deps:[]*work.Action{build});
                addTestVet(b, p, run, null);
                work.Action print = ref new work.Action(Mode:"test print",Func:builderNoTest,Package:p,Deps:[]*work.Action{run});
                return (build, run, print, null);
            } 

            // Build Package structs describing:
            //    ptest - package + test files
            //    pxtest - package of external test files
            //    pmain - pkg.test binary
            ref load.Package ptest = default;            ref load.Package pxtest = default;            ref load.Package pmain = default;



            var localCover = testCover && testCoverPaths == null;

            ptest, pxtest, err = load.TestPackagesFor(p, localCover || p.Name == "main");
            if (err != null)
            {
                return (null, null, null, err);
            } 

            // Use last element of import path, not package name.
            // They differ when package name is "main".
            // But if the import path is "command-line-arguments",
            // like it is during 'go run', use the package name.
            @string elem = default;
            if (p.ImportPath == "command-line-arguments")
            {
                elem = p.Name;
            }
            else
            {
                _, elem = path.Split(p.ImportPath);
            }
            var testBinary = elem + ".test"; 

            // Should we apply coverage analysis locally,
            // only for this package and only for this test?
            // Yes, if -cover is on but -coverpkg has not specified
            // a list of packages for global coverage.
            if (localCover)
            {
                ptest.Internal.CoverMode = testCoverMode;
                slice<@string> coverFiles = default;
                coverFiles = append(coverFiles, ptest.GoFiles);
                coverFiles = append(coverFiles, ptest.CgoFiles);
                ptest.Internal.CoverVars = declareCoverVars(ptest.ImportPath, coverFiles);
            }
            var testDir = b.NewObjdir();
            {
                var err__prev1 = err;

                var err = b.Mkdir(testDir);

                if (err != null)
                {
                    return (null, null, null, err);
                } 

                // Action for building pkg.test.

                err = err__prev1;

            } 

            // Action for building pkg.test.
            pmain = ref new load.Package(PackagePublic:load.PackagePublic{Name:"main",Dir:testDir,GoFiles:[]string{"_testmain.go"},ImportPath:p.ImportPath+" (testmain)",Root:p.Root,},Internal:load.PackageInternal{Build:&build.Package{Name:"main"},OmitDebug:!testC&&!testNeedBinary,Asmflags:p.Internal.Asmflags,Gcflags:p.Internal.Gcflags,Ldflags:p.Internal.Ldflags,Gccgoflags:p.Internal.Gccgoflags,},); 

            // The generated main also imports testing, regexp, and os.
            // Also the linker introduces implicit dependencies reported by LinkerDeps.
            load.ImportStack stk = default;
            stk.Push("testmain");
            var deps = testMainDeps; // cap==len, so safe for append
            foreach (var (_, d) in load.LinkerDeps(p))
            {
                deps = append(deps, d);
            }
            foreach (var (_, dep) in deps)
            {
                if (dep == ptest.ImportPath)
                {
                    pmain.Internal.Imports = append(pmain.Internal.Imports, ptest);
                }
                else
                {
                    var p1 = load.LoadImport(dep, "", null, ref stk, null, 0L);
                    if (p1.Error != null)
                    {
                        return (null, null, null, p1.Error);
                    }
                    pmain.Internal.Imports = append(pmain.Internal.Imports, p1);
                }
            }
            if (testCoverPkgs != null)
            { 
                // Add imports, but avoid duplicates.
                map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref load.Package, bool>{p:true,ptest:true};
                {
                    var p1__prev1 = p1;

                    foreach (var (_, __p1) in pmain.Internal.Imports)
                    {
                        p1 = __p1;
                        seen[p1] = true;
                    }

                    p1 = p1__prev1;
                }

                {
                    var p1__prev1 = p1;

                    foreach (var (_, __p1) in testCoverPkgs)
                    {
                        p1 = __p1;
                        if (!seen[p1])
                        {
                            seen[p1] = true;
                            pmain.Internal.Imports = append(pmain.Internal.Imports, p1);
                        }
                    }

                    p1 = p1__prev1;
                }

            } 

            // Do initial scan for metadata needed for writing _testmain.go
            // Use that metadata to update the list of imports for package main.
            // The list of imports is used by recompileForTest and by the loop
            // afterward that gathers t.Cover information.
            var (t, err) = loadTestFuncs(ptest);
            if (err != null)
            {
                return (null, null, null, err);
            }
            if (len(ptest.GoFiles) + len(ptest.CgoFiles) > 0L)
            {
                pmain.Internal.Imports = append(pmain.Internal.Imports, ptest);
                t.ImportTest = true;
            }
            if (pxtest != null)
            {
                pmain.Internal.Imports = append(pmain.Internal.Imports, pxtest);
                t.ImportXtest = true;
            }
            if (ptest != p)
            { 
                // We have made modifications to the package p being tested
                // and are rebuilding p (as ptest).
                // Arrange to rebuild all packages q such that
                // the test depends on q and q depends on p.
                // This makes sure that q sees the modifications to p.
                // Strictly speaking, the rebuild is only necessary if the
                // modifications to p change its export metadata, but
                // determining that is a bit tricky, so we rebuild always.
                recompileForTest(pmain, p, ptest, pxtest);
            }
            foreach (var (_, cp) in pmain.Internal.Imports)
            {
                if (len(cp.Internal.CoverVars) > 0L)
                {
                    t.Cover = append(t.Cover, new coverInfo(cp,cp.Internal.CoverVars));
                }
            }
            if (!cfg.BuildN)
            { 
                // writeTestmain writes _testmain.go,
                // using the test description gathered in t.
                {
                    var err__prev2 = err;

                    err = writeTestmain(testDir + "_testmain.go", t);

                    if (err != null)
                    {
                        return (null, null, null, err);
                    }

                    err = err__prev2;

                }
            } 

            // Set compile objdir to testDir we've already created,
            // so that the default file path stripping applies to _testmain.go.
            b.CompileAction(work.ModeBuild, work.ModeBuild, pmain).Objdir = testDir;

            var a = b.LinkAction(work.ModeBuild, work.ModeBuild, pmain);
            a.Target = testDir + testBinary + cfg.ExeSuffix;
            if (cfg.Goos == "windows")
            { 
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
                foreach (var (_, bad) in windowsBadWords)
                {
                    if (strings.Contains(testBinary, bad))
                    {
                        a.Target = testDir + "test.test" + cfg.ExeSuffix;
                        break;
                    }
                }
            }
            buildAction = a;
            ref work.Action installAction = default;            ref work.Action cleanAction = default;

            if (testC || testNeedBinary)
            { 
                // -c or profiling flag: create action to copy binary to ./test.out.
                var target = filepath.Join(@base.Cwd, testBinary + cfg.ExeSuffix);
                if (testO != "")
                {
                    target = testO;
                    if (!filepath.IsAbs(target))
                    {
                        target = filepath.Join(@base.Cwd, target);
                    }
                }
                pmain.Target = target;
                installAction = ref new work.Action(Mode:"test build",Func:work.BuildInstallFunc,Deps:[]*work.Action{buildAction},Package:pmain,Target:target,);
                runAction = installAction; // make sure runAction != nil even if not running test
            }
            if (testC)
            {
                printAction = ref new work.Action(Mode:"test print (nop)",Package:p,Deps:[]*work.Action{runAction}); // nop
            }
            else
            { 
                // run test
                ptr<object> c = @new<runCache>();
                runAction = ref new work.Action(Mode:"test run",Func:c.builderRunTest,Deps:[]*work.Action{buildAction},Package:p,IgnoreFail:true,TryCache:c.tryCache,Objdir:testDir,);
                if (len(ptest.GoFiles) + len(ptest.CgoFiles) > 0L)
                {
                    addTestVet(b, ptest, runAction, installAction);
                }
                if (pxtest != null)
                {
                    addTestVet(b, pxtest, runAction, installAction);
                }
                cleanAction = ref new work.Action(Mode:"test clean",Func:builderCleanTest,Deps:[]*work.Action{runAction},Package:p,IgnoreFail:true,Objdir:testDir,);
                printAction = ref new work.Action(Mode:"test print",Func:builderPrintTest,Deps:[]*work.Action{cleanAction},Package:p,IgnoreFail:true,);
            }
            if (installAction != null)
            {
                if (runAction != installAction)
                {
                    installAction.Deps = append(installAction.Deps, runAction);
                }
                if (cleanAction != null)
                {
                    cleanAction.Deps = append(cleanAction.Deps, installAction);
                }
            }
            return (buildAction, runAction, printAction, null);
        }

        private static void addTestVet(ref work.Builder b, ref load.Package p, ref work.Action runAction, ref work.Action installAction)
        {
            if (testVetList == "off")
            {
                return;
            }
            var vet = b.VetAction(work.ModeBuild, work.ModeBuild, p);
            runAction.Deps = append(runAction.Deps, vet); 
            // Install will clean the build directory.
            // Make sure vet runs first.
            // The install ordering in b.VetAction does not apply here
            // because we are using a custom installAction (created above).
            if (installAction != null)
            {
                installAction.Deps = append(installAction.Deps, vet);
            }
        }

        private static void recompileForTest(ref load.Package _pmain, ref load.Package _preal, ref load.Package _ptest, ref load.Package _pxtest) => func(_pmain, _preal, _ptest, _pxtest, (ref load.Package pmain, ref load.Package preal, ref load.Package ptest, ref load.Package pxtest, Defer _, Panic panic, Recover __) =>
        { 
            // The "test copy" of preal is ptest.
            // For each package that depends on preal, make a "test copy"
            // that depends on ptest. And so on, up the dependency tree.
            map testCopy = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref load.Package, ref load.Package>{preal:ptest};
            foreach (var (_, p) in load.PackageList(new slice<ref load.Package>(new ref load.Package[] { pmain })))
            {
                if (p == preal)
                {
                    continue;
                } 
                // Copy on write.
                var didSplit = p == pmain || p == pxtest;
                Action split = () =>
                {
                    if (didSplit)
                    {
                        return;
                    }
                    didSplit = true;
                    if (testCopy[p] != null)
                    {
                        panic("recompileForTest loop");
                    }
                    ptr<load.Package> p1 = @new<load.Package>();
                    testCopy[p] = p1;
                    p1.Value = p.Value;
                    p1.Internal.Imports = make_slice<ref load.Package>(len(p.Internal.Imports));
                    copy(p1.Internal.Imports, p.Internal.Imports);
                    p = p1;
                    p.Target = "";
                } 

                // Update p.Internal.Imports to use test copies.
; 

                // Update p.Internal.Imports to use test copies.
                foreach (var (i, imp) in p.Internal.Imports)
                {
                    {
                        ptr<load.Package> p1__prev1 = p1;

                        p1 = testCopy[imp];

                        if (p1 != null && p1 != imp)
                        {
                            split();
                            p.Internal.Imports[i] = p1;
                        }

                        p1 = p1__prev1;

                    }
                }
            }
        });

        // isTestFile reports whether the source file is a set of tests and should therefore
        // be excluded from coverage analysis.
        private static bool isTestFile(@string file)
        { 
            // We don't cover tests, only the code they test.
            return strings.HasSuffix(file, "_test.go");
        }

        // declareCoverVars attaches the required cover variables names
        // to the files, to be used when annotating the files.
        private static map<@string, ref load.CoverVar> declareCoverVars(@string importPath, params @string[] files)
        {
            files = files.Clone();

            var coverVars = make_map<@string, ref load.CoverVar>();
            long coverIndex = 0L; 
            // We create the cover counters as new top-level variables in the package.
            // We need to avoid collisions with user variables (GoCover_0 is unlikely but still)
            // and more importantly with dot imports of other covered packages,
            // so we append 12 hex digits from the SHA-256 of the import path.
            // The point is only to avoid accidents, not to defeat users determined to
            // break things.
            var sum = sha256.Sum256((slice<byte>)importPath);
            var h = fmt.Sprintf("%x", sum[..6L]);
            foreach (var (_, file) in files)
            {
                if (isTestFile(file))
                {
                    continue;
                }
                coverVars[file] = ref new load.CoverVar(File:filepath.Join(importPath,file),Var:fmt.Sprintf("GoCover_%d_%x",coverIndex,h),);
                coverIndex++;
            }
            return coverVars;
        }

        private static slice<byte> noTestsToRun = (slice<byte>)"\ntesting: warning: no tests to run\n";

        private partial struct runCache
        {
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

        private partial struct lockedStdout
        {
        }

        private static (long, error) Write(this lockedStdout _p0, slice<byte> b) => func((defer, _, __) =>
        {
            stdoutMu.Lock();
            defer(stdoutMu.Unlock());
            return os.Stdout.Write(b);
        });

        // builderRunTest is the action for running a test binary.
        private static error builderRunTest(this ref runCache _c, ref work.Builder _b, ref work.Action _a) => func(_c, _b, _a, (ref runCache c, ref work.Builder b, ref work.Action a, Defer defer, Panic _, Recover __) =>
        {
            if (a.Failed)
            { 
                // We were unable to build the binary.
                a.Failed = false;
                a.TestOutput = @new<bytes.Buffer>();
                fmt.Fprintf(a.TestOutput, "FAIL\t%s [build failed]\n", a.Package.ImportPath);
                @base.SetExitStatus(1L);
                return error.As(null);
            }
            io.Writer stdout = os.Stdout;
            if (testJSON)
            {
                var json = test2json.NewConverter(new lockedStdout(), a.Package.ImportPath, test2json.Timestamp);
                defer(json.Close());
                stdout = json;
            }
            bytes.Buffer buf = default;
            if (len(pkgArgs) == 0L || testBench)
            { 
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
                if (testShowPass && (len(pkgs) == 1L || cfg.BuildP == 1L) || testJSON)
                { 
                    // Write both to stdout and buf, for possible saving
                    // to cache, and for looking for the "no tests to run" message.
                    stdout = io.MultiWriter(stdout, ref buf);
                }
                else
                {
                    stdout = ref buf;
                }
            }
            if (c.buf == null)
            { 
                // We did not find a cached result using the link step action ID,
                // so we ran the link step. Try again now with the link output
                // content ID. The attempt using the action ID makes sure that
                // if the link inputs don't change, we reuse the cached test
                // result without even rerunning the linker. The attempt using
                // the link output (test binary) content ID makes sure that if
                // we have different link inputs but the same final binary,
                // we still reuse the cached test result.
                // c.saveOutput will store the result under both IDs.
                c.tryCacheWithID(b, a, a.Deps[0L].BuildContentID());
            }
            if (c.buf != null)
            {
                if (stdout != ref buf)
                {
                    stdout.Write(c.buf.Bytes());
                    c.buf.Reset();
                }
                a.TestOutput = c.buf;
                return error.As(null);
            }
            var execCmd = work.FindExecCmd();
            @string testlogArg = new slice<@string>(new @string[] {  });
            if (!c.disableCache && len(execCmd) == 0L)
            {
                testlogArg = new slice<@string>(new @string[] { "-test.testlogfile="+a.Objdir+"testlog.txt" });
            }
            var args = str.StringList(execCmd, a.Deps[0L].BuiltTarget(), testlogArg, testArgs);

            if (testCoverProfile != "")
            { 
                // Write coverage to temporary profile, for merging later.
                {
                    var i__prev1 = i;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        if (strings.HasPrefix(arg, "-test.coverprofile="))
                        {
                            args[i] = "-test.coverprofile=" + a.Objdir + "_cover_.out";
                        }
                    }

                    i = i__prev1;
                }

            }
            if (cfg.BuildN || cfg.BuildX)
            {
                b.Showcmd("", "%s", strings.Join(args, " "));
                if (cfg.BuildN)
                {
                    return error.As(null);
                }
            }
            var cmd = exec.Command(args[0L], args[1L..]);
            cmd.Dir = a.Package.Dir;
            cmd.Env = @base.EnvForDir(cmd.Dir, cfg.OrigEnv);
            cmd.Stdout = stdout;
            cmd.Stderr = stdout; 

            // If there are any local SWIG dependencies, we want to load
            // the shared library from the build directory.
            if (a.Package.UsesSwig())
            {
                var env = cmd.Env;
                var found = false;
                @string prefix = "LD_LIBRARY_PATH=";
                {
                    var i__prev1 = i;

                    foreach (var (__i, __v) in env)
                    {
                        i = __i;
                        v = __v;
                        if (strings.HasPrefix(v, prefix))
                        {
                            env[i] = v + ":.";
                            found = true;
                            break;
                        }
                    }

                    i = i__prev1;
                }

                if (!found)
                {
                    env = append(env, "LD_LIBRARY_PATH=.");
                }
                cmd.Env = env;
            }
            var t0 = time.Now();
            var err = cmd.Start(); 

            // This is a last-ditch deadline to detect and
            // stop wedged test binaries, to keep the builders
            // running.
            if (err == null)
            {
                var tick = time.NewTimer(testKillTimeout);
                @base.StartSigHandlers();
                var done = make_channel<error>();
                go_(() => () =>
                {
                    done.Send(cmd.Wait());
                }());
Outer:
                if (@base.SignalTrace != null)
                { 
                    // Send a quit signal in the hope that the program will print
                    // a stack trace and exit. Give it five seconds before resorting
                    // to Kill.
                    cmd.Process.Signal(@base.SignalTrace);
                    fmt.Fprintf(cmd.Stdout, "*** Test killed with %v: ran too long (%v).\n", @base.SignalTrace, testKillTimeout);
                    _breakOuter = true;
                    break;
                }
                cmd.Process.Kill();
                err = done.Receive();
                fmt.Fprintf(cmd.Stdout, "*** Test killed: ran too long (%v).\n", testKillTimeout);
                tick.Stop();
            }
            var @out = buf.Bytes();
            a.TestOutput = ref buf;
            var t = fmt.Sprintf("%.3fs", time.Since(t0).Seconds());

            mergeCoverProfile(cmd.Stdout, a.Objdir + "_cover_.out");

            if (err == null)
            {
                @string norun = "";
                if (!testShowPass && !testJSON)
                {
                    buf.Reset();
                }
                if (bytes.HasPrefix(out, noTestsToRun[1L..]) || bytes.Contains(out, noTestsToRun))
                {
                    norun = " [no tests to run]";
                }
                fmt.Fprintf(cmd.Stdout, "ok  \t%s\t%s%s%s\n", a.Package.ImportPath, t, coveragePercentage(out), norun);
                c.saveOutput(a);
            }
            else
            {
                @base.SetExitStatus(1L); 
                // If there was test output, assume we don't need to print the exit status.
                // Buf there's no test output, do print the exit status.
                if (len(out) == 0L)
                {
                    fmt.Fprintf(cmd.Stdout, "%s\n", err);
                }
                fmt.Fprintf(cmd.Stdout, "FAIL\t%s\t%s\n", a.Package.ImportPath, t);
            }
            if (cmd.Stdout != ref buf)
            {
                buf.Reset(); // cmd.Stdout was going to os.Stdout already
            }
            return error.As(null);
        });

        // tryCache is called just before the link attempt,
        // to see if the test result is cached and therefore the link is unneeded.
        // It reports whether the result can be satisfied from cache.
        private static bool tryCache(this ref runCache c, ref work.Builder b, ref work.Action a)
        {
            return c.tryCacheWithID(b, a, a.Deps[0L].BuildActionID());
        }

        private static bool tryCacheWithID(this ref runCache c, ref work.Builder b, ref work.Action a, @string id)
        {
            if (len(pkgArgs) == 0L)
            { 
                // Caching does not apply to "go test",
                // only to "go test foo" (including "go test .").
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: caching disabled in local directory mode\n");
                }
                c.disableCache = true;
                return false;
            }
            slice<@string> cacheArgs = default;
            foreach (var (_, arg) in testArgs)
            {
                var i = strings.Index(arg, "=");
                if (i < 0L || !strings.HasPrefix(arg, "-test."))
                {
                    if (cache.DebugTest)
                    {
                        fmt.Fprintf(os.Stderr, "testcache: caching disabled for test argument: %s\n", arg);
                    }
                    c.disableCache = true;
                    return false;
                }
                switch (arg[..i])
                {
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

                    case "-test.v": 
                        // These are cacheable.
                        // Note that this list is documented above,
                        // so if you add to this list, update the docs too.
                        cacheArgs = append(cacheArgs, arg);
                        break;
                    case "-test.timeout": 
                        break;
                    default: 
                        // nothing else is cacheable
                        if (cache.DebugTest)
                        {
                            fmt.Fprintf(os.Stderr, "testcache: caching disabled for test argument: %s\n", arg);
                        }
                        c.disableCache = true;
                        return false;
                        break;
                }
            }
            if (cache.Default() == null)
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: GOCACHE=off\n");
                }
                c.disableCache = true;
                return false;
            } 

            // The test cache result fetch is a two-level lookup.
            //
            // First, we use the content hash of the test binary
            // and its command-line arguments to find the
            // list of environment variables and files consulted
            // the last time the test was run with those arguments.
            // (To avoid unnecessary links, we store this entry
            // under two hashes: id1 uses the linker inputs as a
            // proxy for the test binary, and id2 uses the actual
            // test binary. If the linker inputs are unchanged,
            // this way we avoid the link step, even though we
            // do not cache link outputs.)
            //
            // Second, we compute a hash of the values of the
            // environment variables and the content of the files
            // listed in the log from the previous run.
            // Then we look up test output using a combination of
            // the hash from the first part (testID) and the hash of the
            // test inputs (testInputsID).
            //
            // In order to store a new test result, we must redo the
            // testInputsID computation using the log from the run
            // we want to cache, and then we store that new log and
            // the new outputs.
            var h = cache.NewHash("testResult");
            fmt.Fprintf(h, "test binary %s args %q execcmd %q", id, cacheArgs, work.ExecCmd);
            var testID = h.Sum();
            if (c.id1 == (new cache.ActionID()))
            {
                c.id1 = testID;
            }
            else
            {
                c.id2 = testID;
            }
            if (cache.DebugTest)
            {
                fmt.Fprintf(os.Stderr, "testcache: %s: test ID %x => %x\n", a.Package.ImportPath, id, testID);
            } 

            // Load list of referenced environment variables and files
            // from last run of testID, and compute hash of that content.
            var (data, entry, err) = cache.Default().GetBytes(testID);
            if (!bytes.HasPrefix(data, testlogMagic) || data[len(data) - 1L] != '\n')
            {
                if (cache.DebugTest)
                {
                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: input list not found: %v\n", a.Package.ImportPath, err);
                    }
                    else
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed\n", a.Package.ImportPath);
                    }
                }
                return false;
            }
            var (testInputsID, err) = computeTestInputsID(a, data);
            if (err != null)
            {
                return false;
            }
            if (cache.DebugTest)
            {
                fmt.Fprintf(os.Stderr, "testcache: %s: test ID %x => input ID %x => %x\n", a.Package.ImportPath, testID, testInputsID, testAndInputKey(testID, testInputsID));
            } 

            // Parse cached result in preparation for changing run time to "(cached)".
            // If we can't parse the cached result, don't use it.
            data, entry, err = cache.Default().GetBytes(testAndInputKey(testID, testInputsID));
            if (len(data) == 0L || data[len(data) - 1L] != '\n')
            {
                if (cache.DebugTest)
                {
                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: test output not found: %v\n", a.Package.ImportPath, err);
                    }
                    else
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
                    }
                }
                return false;
            }
            if (entry.Time.Before(testCacheExpire))
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: %s: test output expired due to go clean -testcache\n", a.Package.ImportPath);
                }
                return false;
            }
            i = bytes.LastIndexByte(data[..len(data) - 1L], '\n') + 1L;
            if (!bytes.HasPrefix(data[i..], (slice<byte>)"ok  \t"))
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
                }
                return false;
            }
            var j = bytes.IndexByte(data[i + len("ok  \t")..], '\t');
            if (j < 0L)
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: %s: test output malformed\n", a.Package.ImportPath);
                }
                return false;
            }
            j += i + len("ok  \t") + 1L; 

            // Committed to printing.
            c.buf = @new<bytes.Buffer>();
            c.buf.Write(data[..j]);
            c.buf.WriteString("(cached)");
            while (j < len(data) && ('0' <= data[j] && data[j] <= '9' || data[j] == '.' || data[j] == 's'))
            {
                j++;
            }

            c.buf.Write(data[j..]);
            return true;
        }

        private static var errBadTestInputs = errors.New("error parsing test inputs");
        private static slice<byte> testlogMagic = (slice<byte>)"# test log\n"; // known to testing/internal/testdeps/deps.go

        // computeTestInputsID computes the "test inputs ID"
        // (see comment in tryCacheWithID above) for the
        // test log.
        private static (cache.ActionID, error) computeTestInputsID(ref work.Action a, slice<byte> testlog)
        {
            testlog = bytes.TrimPrefix(testlog, testlogMagic);
            var h = cache.NewHash("testInputs");
            var pwd = a.Package.Dir;
            foreach (var (_, line) in bytes.Split(testlog, (slice<byte>)"\n"))
            {
                if (len(line) == 0L)
                {
                    continue;
                }
                var s = string(line);
                var i = strings.Index(s, " ");
                if (i < 0L)
                {
                    if (cache.DebugTest)
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed (%q)\n", a.Package.ImportPath, line);
                    }
                    return (new cache.ActionID(), errBadTestInputs);
                }
                var op = s[..i];
                var name = s[i + 1L..];
                switch (op)
                {
                    case "getenv": 
                        fmt.Fprintf(h, "env %s %x\n", name, hashGetenv(name));
                        break;
                    case "chdir": 
                        pwd = name; // always absolute
                        fmt.Fprintf(h, "cbdir %s %x\n", name, hashStat(name));
                        break;
                    case "stat": 
                        if (!filepath.IsAbs(name))
                        {
                            name = filepath.Join(pwd, name);
                        }
                        if (!inDir(name, a.Package.Root))
                        { 
                            // Do not recheck files outside the GOPATH or GOROOT root.
                            break;
                        }
                        fmt.Fprintf(h, "stat %s %x\n", name, hashStat(name));
                        break;
                    case "open": 
                        if (!filepath.IsAbs(name))
                        {
                            name = filepath.Join(pwd, name);
                        }
                        if (!inDir(name, a.Package.Root))
                        { 
                            // Do not recheck files outside the GOPATH or GOROOT root.
                            break;
                        }
                        var (fh, err) = hashOpen(name);
                        if (err != null)
                        {
                            if (cache.DebugTest)
                            {
                                fmt.Fprintf(os.Stderr, "testcache: %s: input file %s: %s\n", a.Package.ImportPath, name, err);
                            }
                            return (new cache.ActionID(), err);
                        }
                        fmt.Fprintf(h, "open %s %x\n", name, fh);
                        break;
                    default: 
                        if (cache.DebugTest)
                        {
                            fmt.Fprintf(os.Stderr, "testcache: %s: input list malformed (%q)\n", a.Package.ImportPath, line);
                        }
                        return (new cache.ActionID(), errBadTestInputs);
                        break;
                }
            }
            var sum = h.Sum();
            return (sum, null);
        }

        private static bool inDir(@string path, @string dir)
        {
            if (str.HasFilePathPrefix(path, dir))
            {
                return true;
            }
            var (xpath, err1) = filepath.EvalSymlinks(path);
            var (xdir, err2) = filepath.EvalSymlinks(dir);
            if (err1 == null && err2 == null && str.HasFilePathPrefix(xpath, xdir))
            {
                return true;
            }
            return false;
        }

        private static cache.ActionID hashGetenv(@string name)
        {
            var h = cache.NewHash("getenv");
            var (v, ok) = os.LookupEnv(name);
            if (!ok)
            {
                h.Write(new slice<byte>(new byte[] { 0 }));
            }
            else
            {
                h.Write(new slice<byte>(new byte[] { 1 }));
                h.Write((slice<byte>)v);
            }
            return h.Sum();
        }

        private static readonly long modTimeCutoff = 2L * time.Second;



        private static var errFileTooNew = errors.New("file used as input is too new");

        private static (cache.ActionID, error) hashOpen(@string name)
        {
            var h = cache.NewHash("open");
            var (info, err) = os.Stat(name);
            if (err != null)
            {
                fmt.Fprintf(h, "err %v\n", err);
                return (h.Sum(), null);
            }
            hashWriteStat(h, info);
            if (info.IsDir())
            {
                var (names, err) = ioutil.ReadDir(name);
                if (err != null)
                {
                    fmt.Fprintf(h, "err %v\n", err);
                }
                foreach (var (_, f) in names)
                {
                    fmt.Fprintf(h, "file %s ", f.Name());
                    hashWriteStat(h, f);
                }
            }
            else if (info.Mode().IsRegular())
            { 
                // Because files might be very large, do not attempt
                // to hash the entirety of their content. Instead assume
                // the mtime and size recorded in hashWriteStat above
                // are good enough.
                //
                // To avoid problems for very recent files where a new
                // write might not change the mtime due to file system
                // mtime precision, reject caching if a file was read that
                // is less than modTimeCutoff old.
                if (time.Since(info.ModTime()) < modTimeCutoff)
                {
                    return (new cache.ActionID(), errFileTooNew);
                }
            }
            return (h.Sum(), null);
        }

        private static cache.ActionID hashStat(@string name)
        {
            var h = cache.NewHash("stat");
            {
                var info__prev1 = info;

                var (info, err) = os.Stat(name);

                if (err != null)
                {
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

                if (err != null)
                {
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

        private static void hashWriteStat(io.Writer h, os.FileInfo info)
        {
            fmt.Fprintf(h, "stat %d %x %v %v\n", info.Size(), uint64(info.Mode()), info.ModTime(), info.IsDir());
        }

        // testAndInputKey returns the actual cache key for the pair (testID, testInputsID).
        private static cache.ActionID testAndInputKey(cache.ActionID testID, cache.ActionID testInputsID)
        {
            return cache.Subkey(testID, fmt.Sprintf("inputs:%x", testInputsID));
        }

        private static void saveOutput(this ref runCache c, ref work.Action a)
        {
            if (c.id1 == (new cache.ActionID()) && c.id2 == (new cache.ActionID()))
            {
                return;
            } 

            // See comment about two-level lookup in tryCacheWithID above.
            var (testlog, err) = ioutil.ReadFile(a.Objdir + "testlog.txt");
            if (err != null || !bytes.HasPrefix(testlog, testlogMagic) || testlog[len(testlog) - 1L] != '\n')
            {
                if (cache.DebugTest)
                {
                    if (err != null)
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: reading testlog: %v\n", a.Package.ImportPath, err);
                    }
                    else
                    {
                        fmt.Fprintf(os.Stderr, "testcache: %s: reading testlog: malformed\n", a.Package.ImportPath);
                    }
                }
                return;
            }
            var (testInputsID, err) = computeTestInputsID(a, testlog);
            if (err != null)
            {
                return;
            }
            if (c.id1 != (new cache.ActionID()))
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: %s: save test ID %x => input ID %x => %x\n", a.Package.ImportPath, c.id1, testInputsID, testAndInputKey(c.id1, testInputsID));
                }
                cache.Default().PutNoVerify(c.id1, bytes.NewReader(testlog));
                cache.Default().PutNoVerify(testAndInputKey(c.id1, testInputsID), bytes.NewReader(a.TestOutput.Bytes()));
            }
            if (c.id2 != (new cache.ActionID()))
            {
                if (cache.DebugTest)
                {
                    fmt.Fprintf(os.Stderr, "testcache: %s: save test ID %x => input ID %x => %x\n", a.Package.ImportPath, c.id2, testInputsID, testAndInputKey(c.id2, testInputsID));
                }
                cache.Default().PutNoVerify(c.id2, bytes.NewReader(testlog));
                cache.Default().PutNoVerify(testAndInputKey(c.id2, testInputsID), bytes.NewReader(a.TestOutput.Bytes()));
            }
        }

        // coveragePercentage returns the coverage results (if enabled) for the
        // test. It uncovers the data by scanning the output from the test run.
        private static @string coveragePercentage(slice<byte> @out)
        {
            if (!testCover)
            {
                return "";
            } 
            // The string looks like
            //    test coverage for encoding/binary: 79.9% of statements
            // Extract the piece from the percentage to the end of the line.
            var re = regexp.MustCompile("coverage: (.*)\\n");
            var matches = re.FindSubmatch(out);
            if (matches == null)
            { 
                // Probably running "go test -cover" not "go test -cover fmt".
                // The coverage output will appear in the output directly.
                return "";
            }
            return fmt.Sprintf("\tcoverage: %s", matches[1L]);
        }

        // builderCleanTest is the action for cleaning up after a test.
        private static error builderCleanTest(ref work.Builder b, ref work.Action a)
        {
            if (cfg.BuildWork)
            {
                return error.As(null);
            }
            if (cfg.BuildX)
            {
                b.Showcmd("", "rm -r %s", a.Objdir);
            }
            os.RemoveAll(a.Objdir);
            return error.As(null);
        }

        // builderPrintTest is the action for printing a test result.
        private static error builderPrintTest(ref work.Builder b, ref work.Action a)
        {
            var clean = a.Deps[0L];
            var run = clean.Deps[0L];
            if (run.TestOutput != null)
            {
                os.Stdout.Write(run.TestOutput.Bytes());
                run.TestOutput = null;
            }
            return error.As(null);
        }

        // builderNoTest is the action for testing a package with no test files.
        private static error builderNoTest(ref work.Builder _b, ref work.Action _a) => func(_b, _a, (ref work.Builder b, ref work.Action a, Defer defer, Panic _, Recover __) =>
        {
            io.Writer stdout = os.Stdout;
            if (testJSON)
            {
                var json = test2json.NewConverter(new lockedStdout(), a.Package.ImportPath, test2json.Timestamp);
                defer(json.Close());
                stdout = json;
            }
            fmt.Fprintf(stdout, "?   \t%s\t[no test files]\n", a.Package.ImportPath);
            return error.As(null);
        });

        // isTestFunc tells whether fn has the type of a testing function. arg
        // specifies the parameter type we look for: B, M or T.
        private static bool isTestFunc(ref ast.FuncDecl fn, @string arg)
        {
            if (fn.Type.Results != null && len(fn.Type.Results.List) > 0L || fn.Type.Params.List == null || len(fn.Type.Params.List) != 1L || len(fn.Type.Params.List[0L].Names) > 1L)
            {
                return false;
            }
            ref ast.StarExpr (ptr, ok) = fn.Type.Params.List[0L].Type._<ref ast.StarExpr>();
            if (!ok)
            {
                return false;
            } 
            // We can't easily check that the type is *testing.M
            // because we don't know how testing has been imported,
            // but at least check that it's *M or *something.M.
            // Same applies for B and T.
            {
                ref ast.Ident (name, ok) = ptr.X._<ref ast.Ident>();

                if (ok && name.Name == arg)
                {
                    return true;
                }

            }
            {
                ref ast.SelectorExpr (sel, ok) = ptr.X._<ref ast.SelectorExpr>();

                if (ok && sel.Sel.Name == arg)
                {
                    return true;
                }

            }
            return false;
        }

        // isTest tells whether name looks like a test (or benchmark, according to prefix).
        // It is a Test (say) if there is a character after Test that is not a lower-case letter.
        // We don't want TesticularCancer.
        private static bool isTest(@string name, @string prefix)
        {
            if (!strings.HasPrefix(name, prefix))
            {
                return false;
            }
            if (len(name) == len(prefix))
            { // "Test" is ok
                return true;
            }
            var (rune, _) = utf8.DecodeRuneInString(name[len(prefix)..]);
            return !unicode.IsLower(rune);
        }

        private partial struct coverInfo
        {
            public ptr<load.Package> Package;
            public map<@string, ref load.CoverVar> Vars;
        }

        // loadTestFuncs returns the testFuncs describing the tests that will be run.
        private static (ref testFuncs, error) loadTestFuncs(ref load.Package ptest)
        {
            testFuncs t = ref new testFuncs(Package:ptest,);
            {
                var file__prev1 = file;

                foreach (var (_, __file) in ptest.TestGoFiles)
                {
                    file = __file;
                    {
                        var err__prev1 = err;

                        var err = t.load(filepath.Join(ptest.Dir, file), "_test", ref t.ImportTest, ref t.NeedTest);

                        if (err != null)
                        {
                            return (null, err);
                        }

                        err = err__prev1;

                    }
                }

                file = file__prev1;
            }

            {
                var file__prev1 = file;

                foreach (var (_, __file) in ptest.XTestGoFiles)
                {
                    file = __file;
                    {
                        var err__prev1 = err;

                        err = t.load(filepath.Join(ptest.Dir, file), "_xtest", ref t.ImportXtest, ref t.NeedXtest);

                        if (err != null)
                        {
                            return (null, err);
                        }

                        err = err__prev1;

                    }
                }

                file = file__prev1;
            }

            return (t, null);
        }

        // writeTestmain writes the _testmain.go file for t to the file named out.
        private static error writeTestmain(@string @out, ref testFuncs _t) => func(_t, (ref testFuncs t, Defer defer, Panic _, Recover __) =>
        {
            var (f, err) = os.Create(out);
            if (err != null)
            {
                return error.As(err);
            }
            defer(f.Close());

            {
                var err = testmainTmpl.Execute(f, t);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            return error.As(null);
        });

        private partial struct testFuncs
        {
            public slice<testFunc> Tests;
            public slice<testFunc> Benchmarks;
            public slice<testFunc> Examples;
            public ptr<testFunc> TestMain;
            public ptr<load.Package> Package;
            public bool ImportTest;
            public bool NeedTest;
            public bool ImportXtest;
            public bool NeedXtest;
            public slice<coverInfo> Cover;
        }

        private static @string CoverMode(this ref testFuncs t)
        {
            return testCoverMode;
        }

        private static bool CoverEnabled(this ref testFuncs t)
        {
            return testCover;
        }

        // ImportPath returns the import path of the package being tested, if it is within GOPATH.
        // This is printed by the testing package when running benchmarks.
        private static @string ImportPath(this ref testFuncs t)
        {
            var pkg = t.Package.ImportPath;
            if (strings.HasPrefix(pkg, "_/"))
            {
                return "";
            }
            if (pkg == "command-line-arguments")
            {
                return "";
            }
            return pkg;
        }

        // Covered returns a string describing which packages are being tested for coverage.
        // If the covered package is the same as the tested package, it returns the empty string.
        // Otherwise it is a comma-separated human-readable list of packages beginning with
        // " in", ready for use in the coverage message.
        private static @string Covered(this ref testFuncs t)
        {
            if (testCoverPaths == null)
            {
                return "";
            }
            return " in " + strings.Join(testCoverPaths, ", ");
        }

        // Tested returns the name of the package being tested.
        private static @string Tested(this ref testFuncs t)
        {
            return t.Package.Name;
        }

        private partial struct testFunc
        {
            public @string Package; // imported package name (_test or _xtest)
            public @string Name; // function name
            public @string Output; // output, for examples
            public bool Unordered; // output is allowed to be unordered.
        }

        private static var testFileSet = token.NewFileSet();

        private static error load(this ref testFuncs t, @string filename, @string pkg, ref bool doImport, ref bool seen)
        {
            var (f, err) = parser.ParseFile(testFileSet, filename, null, parser.ParseComments);
            if (err != null)
            {
                return error.As(@base.ExpandScanner(err));
            }
            foreach (var (_, d) in f.Decls)
            {
                ref ast.FuncDecl (n, ok) = d._<ref ast.FuncDecl>();
                if (!ok)
                {
                    continue;
                }
                if (n.Recv != null)
                {
                    continue;
                }
                var name = n.Name.String();

                if (name == "TestMain") 
                    if (isTestFunc(n, "T"))
                    {
                        t.Tests = append(t.Tests, new testFunc(pkg,name,"",false));
                        doImport.Value = true;
                        seen.Value = true;
                        continue;
                    }
                    var err = checkTestFunc(n, "M");
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    if (t.TestMain != null)
                    {
                        return error.As(errors.New("multiple definitions of TestMain"));
                    }
                    t.TestMain = ref new testFunc(pkg,name,"",false);
                    doImport.Value = true;
                    seen.Value = true;
                else if (isTest(name, "Test")) 
                    err = checkTestFunc(n, "T");
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    t.Tests = append(t.Tests, new testFunc(pkg,name,"",false));
                    doImport.Value = true;
                    seen.Value = true;
                else if (isTest(name, "Benchmark")) 
                    err = checkTestFunc(n, "B");
                    if (err != null)
                    {
                        return error.As(err);
                    }
                    t.Benchmarks = append(t.Benchmarks, new testFunc(pkg,name,"",false));
                    doImport.Value = true;
                    seen.Value = true;
                            }
            var ex = doc.Examples(f);
            sort.Slice(ex, (i, j) => error.As(ex[i].Order < ex[j].Order));
            foreach (var (_, e) in ex)
            {
                doImport.Value = true; // import test file whether executed or not
                if (e.Output == "" && !e.EmptyOutput)
                { 
                    // Don't run examples with no output.
                    continue;
                }
                t.Examples = append(t.Examples, new testFunc(pkg,"Example"+e.Name,e.Output,e.Unordered));
                seen.Value = true;
            }
            return error.As(null);
        }

        private static error checkTestFunc(ref ast.FuncDecl fn, @string arg)
        {
            if (!isTestFunc(fn, arg))
            {
                var name = fn.Name.String();
                var pos = testFileSet.Position(fn.Pos());
                return error.As(fmt.Errorf("%s: wrong signature for %s, must be: func %s(%s *testing.%s)", pos, name, name, strings.ToLower(arg), arg));
            }
            return error.As(null);
        }

        private static var testmainTmpl = template.Must(template.New("main").Parse("\npackage main\n\nimport (\n{{if not .TestMain}}\n\t\"os\"\n{{end}}\n\t\"testing\"\n\t\"testing/i" +
    "nternal/testdeps\"\n\n{{if .ImportTest}}\n\t{{if .NeedTest}}_test{{else}}_{{end}} {{." +
    "Package.ImportPath | printf \"%q\"}}\n{{end}}\n{{if .ImportXtest}}\n\t{{if .NeedXtest}" +
    "}_xtest{{else}}_{{end}} {{.Package.ImportPath | printf \"%s_test\" | printf \"%q\"}}" +
    "\n{{end}}\n{{range $i, $p := .Cover}}\n\t_cover{{$i}} {{$p.Package.ImportPath | prin" +
    "tf \"%q\"}}\n{{end}}\n)\n\nvar tests = []testing.InternalTest{\n{{range .Tests}}\n\t{\"{{." +
    "Name}}\", {{.Package}}.{{.Name}}},\n{{end}}\n}\n\nvar benchmarks = []testing.Internal" +
    "Benchmark{\n{{range .Benchmarks}}\n\t{\"{{.Name}}\", {{.Package}}.{{.Name}}},\n{{end}}" +
    "\n}\n\nvar examples = []testing.InternalExample{\n{{range .Examples}}\n\t{\"{{.Name}}\"," +
    " {{.Package}}.{{.Name}}, {{.Output | printf \"%q\"}}, {{.Unordered}}},\n{{end}}\n}\n\n" +
    "func init() {\n\ttestdeps.ImportPath = {{.ImportPath | printf \"%q\"}}\n}\n\n{{if .Cove" +
    "rEnabled}}\n\n// Only updated by init functions, so no need for atomicity.\nvar (\n\t" +
    "coverCounters = make(map[string][]uint32)\n\tcoverBlocks = make(map[string][]testi" +
    "ng.CoverBlock)\n)\n\nfunc init() {\n\t{{range $i, $p := .Cover}}\n\t{{range $file, $cov" +
    "er := $p.Vars}}\n\tcoverRegisterFile({{printf \"%q\" $cover.File}}, _cover{{$i}}.{{$" +
    "cover.Var}}.Count[:], _cover{{$i}}.{{$cover.Var}}.Pos[:], _cover{{$i}}.{{$cover." +
    "Var}}.NumStmt[:])\n\t{{end}}\n\t{{end}}\n}\n\nfunc coverRegisterFile(fileName string, c" +
    "ounter []uint32, pos []uint32, numStmts []uint16) {\n\tif 3*len(counter) != len(po" +
    "s) || len(counter) != len(numStmts) {\n\t\tpanic(\"coverage: mismatched sizes\")\n\t}\n\t" +
    "if coverCounters[fileName] != nil {\n\t\t// Already registered.\n\t\treturn\n\t}\n\tcoverC" +
    "ounters[fileName] = counter\n\tblock := make([]testing.CoverBlock, len(counter))\n\t" +
    "for i := range counter {\n\t\tblock[i] = testing.CoverBlock{\n\t\t\tLine0: pos[3*i+0],\n" +
    "\t\t\tCol0: uint16(pos[3*i+2]),\n\t\t\tLine1: pos[3*i+1],\n\t\t\tCol1: uint16(pos[3*i+2]>>1" +
    "6),\n\t\t\tStmts: numStmts[i],\n\t\t}\n\t}\n\tcoverBlocks[fileName] = block\n}\n{{end}}\n\nfunc" +
    " main() {\n{{if .CoverEnabled}}\n\ttesting.RegisterCover(testing.Cover{\n\t\tMode: {{p" +
    "rintf \"%q\" .CoverMode}},\n\t\tCounters: coverCounters,\n\t\tBlocks: coverBlocks,\n\t\tCov" +
    "eredPackages: {{printf \"%q\" .Covered}},\n\t})\n{{end}}\n\tm := testing.MainStart(test" +
    "deps.TestDeps{}, tests, benchmarks, examples)\n{{with .TestMain}}\n\t{{.Package}}.{" +
    "{.Name}}(m)\n{{else}}\n\tos.Exit(m.Run())\n{{end}}\n}\n\n"));
    }
}}}}
