// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:44:43 UTC
// Original source: C:\Go\src\cmd\dist\test.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void cmdtest()
        {
            gogcflags = os.Getenv("GO_GCFLAGS");

            tester t = default;
            ref bool noRebuild = ref heap(out ptr<bool> _addr_noRebuild);
            flag.BoolVar(_addr_t.listMode, "list", false, "list available tests");
            flag.BoolVar(_addr_t.rebuild, "rebuild", false, "rebuild everything first");
            flag.BoolVar(_addr_noRebuild, "no-rebuild", false, "overrides -rebuild (historical dreg)");
            flag.BoolVar(_addr_t.keepGoing, "k", false, "keep going even when error occurred");
            flag.BoolVar(_addr_t.race, "race", false, "run in race builder mode (different set of tests)");
            flag.BoolVar(_addr_t.compileOnly, "compile-only", false, "compile tests, but don't run them. This is for some builders. Not all dist tests respect this flag, but most do.");
            flag.StringVar(_addr_t.banner, "banner", "##### ", "banner prefix; blank means no section banners");
            flag.StringVar(_addr_t.runRxStr, "run", os.Getenv("GOTESTONLY"), "run only those tests matching the regular expression; empty means to run all. " + "Special exception: if the string begins with '!', the match is inverted.");
            xflagparse(-1L); // any number of args
            if (noRebuild)
            {
                t.rebuild = false;
            }
            t.run();

        }

        // tester executes cmdtest.
        private partial struct tester
        {
            public bool race;
            public bool listMode;
            public bool rebuild;
            public bool failed;
            public bool keepGoing;
            public bool compileOnly; // just try to compile all tests, but no need to run
            public @string runRxStr;
            public ptr<regexp.Regexp> runRx;
            public bool runRxWant; // want runRx to match (true) or not match (false)
            public slice<@string> runNames; // tests to run, exclusive with runRx; empty means all
            public @string banner; // prefix, or "" for none
            public @string lastHeading; // last dir heading printed

            public bool cgoEnabled;
            public bool partial;
            public bool haveTime; // the 'time' binary is available

            public slice<distTest> tests;
            public long timeoutScale;
            public slice<ptr<work>> worklist;
        }

        private partial struct work
        {
            public ptr<distTest> dt;
            public ptr<exec.Cmd> cmd;
            public channel<bool> start;
            public slice<byte> @out;
            public error err;
            public channel<bool> end;
        }

        // A distTest is a test run by dist test.
        // Each test has a unique name and belongs to a group (heading)
        private partial struct distTest
        {
            public @string name; // unique test name; may be filtered with -run flag
            public @string heading; // group section; this header is printed before the test is run.
            public Func<ptr<distTest>, error> fn;
        }

        private static void run(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            timelog("start", "dist test");

            @string exeSuffix = default;
            if (goos == "windows")
            {
                exeSuffix = ".exe";
            }

            {
                var (_, err) = os.Stat(filepath.Join(gobin, "go" + exeSuffix));

                if (err == null)
                {
                    os.Setenv("PATH", fmt.Sprintf("%s%c%s", gobin, os.PathListSeparator, os.Getenv("PATH")));
                }

            }


            var cmd = exec.Command("go", "env", "CGO_ENABLED");
            cmd.Stderr = @new<bytes.Buffer>();
            var (slurp, err) = cmd.Output();
            if (err != null)
            {
                fatalf("Error running go env CGO_ENABLED: %v\n%s", err, cmd.Stderr);
            }

            t.cgoEnabled, _ = strconv.ParseBool(strings.TrimSpace(string(slurp)));
            if (flag.NArg() > 0L && t.runRxStr != "")
            {
                fatalf("the -run regular expression flag is mutually exclusive with test name arguments");
            }

            t.runNames = flag.Args();

            if (t.hasBash())
            {
                {
                    (_, err) = exec.LookPath("time");

                    if (err == null)
                    {
                        t.haveTime = true;
                    }

                }

            }

            if (t.rebuild)
            {
                t.@out("Building packages and commands."); 
                // Force rebuild the whole toolchain.
                goInstall("go", append(new slice<@string>(new @string[] { "-a", "-i" }), toolchain));

            } 

            // Complete rebuild bootstrap, even with -no-rebuild.
            // If everything is up-to-date, this is a no-op.
            // If everything is not up-to-date, the first checkNotStale
            // during the test process will kill the tests, so we might
            // as well install the world.
            // Now that for example "go install cmd/compile" does not
            // also install runtime (you need "go install -i cmd/compile"
            // for that), it's easy for previous workflows like
            // "rebuild the compiler and then run run.bash"
            // to break if we don't automatically refresh things here.
            // Rebuilding is a shortened bootstrap.
            // See cmdbootstrap for a description of the overall process.
            //
            // But don't do this if we're running in the Go build system,
            // where cmd/dist is invoked many times. This just slows that
            // down (Issue 24300).
            if (!t.listMode && os.Getenv("GO_BUILDER_NAME") == "")
            {
                goInstall("go", append(new slice<@string>(new @string[] { "-i" }), toolchain));
                goInstall("go", append(new slice<@string>(new @string[] { "-i" }), toolchain));
                goInstall("go", "std", "cmd");
                checkNotStale("go", "std", "cmd");
            }

            t.timeoutScale = 1L;
            switch (goarch)
            {
                case "arm": 
                    t.timeoutScale = 2L;
                    break;
                case "mips": 

                case "mipsle": 

                case "mips64": 

                case "mips64le": 
                    t.timeoutScale = 4L;
                    break;
            }
            {
                var s = os.Getenv("GO_TEST_TIMEOUT_SCALE");

                if (s != "")
                {
                    t.timeoutScale, err = strconv.Atoi(s);
                    if (err != null)
                    {
                        fatalf("failed to parse $GO_TEST_TIMEOUT_SCALE = %q as integer: %v", s, err);
                    }

                }

            }


            if (t.runRxStr != "")
            {
                if (t.runRxStr[0L] == '!')
                {
                    t.runRxWant = false;
                    t.runRxStr = t.runRxStr[1L..];
                }
                else
                {
                    t.runRxWant = true;
                }

                t.runRx = regexp.MustCompile(t.runRxStr);

            }

            t.registerTests();
            if (t.listMode)
            {
                foreach (var (_, tt) in t.tests)
                {
                    fmt.Println(tt.name);
                }
                return ;

            }

            foreach (var (_, name) in t.runNames)
            {
                if (!t.isRegisteredTestName(name))
                {
                    fatalf("unknown test %q", name);
                }

            } 

            // On a few builders, make GOROOT unwritable to catch tests writing to it.
            if (strings.HasPrefix(os.Getenv("GO_BUILDER_NAME"), "linux-"))
            {
                if (os.Getuid() == 0L)
                { 
                    // Don't bother making GOROOT unwritable:
                    // we're running as root, so permissions would have no effect.
                }
                else
                {
                    xatexit(t.makeGOROOTUnwritable());
                }

            }

            {
                var dt__prev1 = dt;

                foreach (var (_, __dt) in t.tests)
                {
                    dt = __dt;
                    if (!t.shouldRunTest(dt.name))
                    {
                        t.partial = true;
                        continue;
                    }

                    ref var dt = ref heap(dt, out ptr<var> _addr_dt); // dt used in background after this iteration
                    {
                        var err = dt.fn(_addr_dt);

                        if (err != null)
                        {
                            t.runPending(_addr_dt); // in case that hasn't been done yet
                            t.failed = true;
                            if (t.keepGoing)
                            {
                                log.Printf("Failed: %v", err);
                            }
                            else
                            {
                                fatalf("Failed: %v", err);
                            }

                        }

                    }

                }

                dt = dt__prev1;
            }

            t.runPending(null);
            timelog("end", "dist test");

            if (t.failed)
            {
                fmt.Println("\nFAILED");
                xexit(1L);
            }
            else if (incomplete[goos + "/" + goarch])
            {
                fmt.Println("\nFAILED (incomplete port)");
                xexit(1L);
            }
            else if (t.partial)
            {
                fmt.Println("\nALL TESTS PASSED (some were excluded)");
            }
            else
            {
                fmt.Println("\nALL TESTS PASSED");
            }

        }

        private static bool shouldRunTest(this ptr<tester> _addr_t, @string name)
        {
            ref tester t = ref _addr_t.val;

            if (t.runRx != null)
            {
                return t.runRx.MatchString(name) == t.runRxWant;
            }

            if (len(t.runNames) == 0L)
            {
                return true;
            }

            foreach (var (_, runName) in t.runNames)
            {
                if (runName == name)
                {
                    return true;
                }

            }
            return false;

        }

        // short returns a -short flag to pass to 'go test'.
        // It returns "-short", unless the environment variable
        // GO_TEST_SHORT is set to a non-empty, false-ish string.
        //
        // This environment variable is meant to be an internal
        // detail between the Go build system and cmd/dist
        // and is not intended for use by users.
        private static @string @short()
        {
            {
                var v = os.Getenv("GO_TEST_SHORT");

                if (v != "")
                {
                    var (short, err) = strconv.ParseBool(v);
                    if (err != null)
                    {
                        fatalf("invalid GO_TEST_SHORT %q: %v", v, err);
                    }

                    if (!short)
                    {
                        return "-short=false";
                    }

                }

            }

            return "-short";

        }

        // goTest returns the beginning of the go test command line.
        // Callers should use goTest and then pass flags overriding these
        // defaults as later arguments in the command line.
        private static slice<@string> goTest(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            return new slice<@string>(new @string[] { "go", "test", short(), "-count=1", t.tags(), t.runFlag("") });
        }

        private static @string tags(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            if (t.iOS())
            {
                return "-tags=lldb";
            }

            return "-tags=";

        }

        // timeoutDuration converts the provided number of seconds into a
        // time.Duration, scaled by the t.timeoutScale factor.
        private static time.Duration timeoutDuration(this ptr<tester> _addr_t, long sec)
        {
            ref tester t = ref _addr_t.val;

            return time.Duration(sec) * time.Second * time.Duration(t.timeoutScale);
        }

        // timeout returns the "-timeout=" string argument to "go test" given
        // the number of seconds of timeout. It scales it by the
        // t.timeoutScale factor.
        private static @string timeout(this ptr<tester> _addr_t, long sec)
        {
            ref tester t = ref _addr_t.val;

            return "-timeout=" + t.timeoutDuration(sec).String();
        }

        // ranGoTest and stdMatches are state closed over by the stdlib
        // testing func in registerStdTest below. The tests are run
        // sequentially, so there's no need for locks.
        //
        // ranGoBench and benchMatches are the same, but are only used
        // in -race mode.
        private static bool ranGoTest = default;        private static slice<@string> stdMatches = default;        private static bool ranGoBench = default;        private static slice<@string> benchMatches = default;

        private static void registerStdTest(this ptr<tester> _addr_t, @string pkg) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            @string testName = "go_test:" + pkg;
            if (t.runRx == null || t.runRx.MatchString(testName) == t.runRxWant)
            {
                stdMatches = append(stdMatches, pkg);
            }

            t.tests = append(t.tests, new distTest(name:testName,heading:"Testing packages.",fn:func(dt*distTest)error{ifranGoTest{returnnil}t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)ranGoTest=truetimeoutSec:=180for_,pkg:=rangestdMatches{ifpkg=="cmd/go"{timeoutSec*=3break}}ift.shouldUsePrecompiledStdTest(){returnt.runPrecompiledStdTest(t.timeoutDuration(timeoutSec))}args:=[]string{"test",short(),t.tags(),t.timeout(timeoutSec),"-gcflags=all="+gogcflags,}ift.race{args=append(args,"-race")}ift.compileOnly{args=append(args,"-run=^$")}args=append(args,stdMatches...)cmd:=exec.Command("go",args...)cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrreturncmd.Run()},));

        });

        private static void registerRaceBenchTest(this ptr<tester> _addr_t, @string pkg) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            @string testName = "go_test_bench:" + pkg;
            if (t.runRx == null || t.runRx.MatchString(testName) == t.runRxWant)
            {
                benchMatches = append(benchMatches, pkg);
            }

            t.tests = append(t.tests, new distTest(name:testName,heading:"Running benchmarks briefly.",fn:func(dt*distTest)error{ifranGoBench{returnnil}t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)ranGoBench=trueargs:=[]string{"test",short(),"-race",t.timeout(1200),"-run=^$","-benchtime=.1s","-cpu=4",}if!t.compileOnly{args=append(args,"-bench=.*")}args=append(args,benchMatches...)cmd:=exec.Command("go",args...)cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrreturncmd.Run()},));

        });

        // stdOutErrAreTerminals is defined in test_linux.go, to report
        // whether stdout & stderr are terminals.
        private static Func<bool> stdOutErrAreTerminals = default;

        private static void registerTests(this ptr<tester> _addr_t) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;
 
            // Fast path to avoid the ~1 second of `go list std cmd` when
            // the caller lists specific tests to run. (as the continuous
            // build coordinator does).
            if (len(t.runNames) > 0L)
            {
                foreach (var (_, name) in t.runNames)
                {
                    if (strings.HasPrefix(name, "go_test:"))
                    {
                        t.registerStdTest(strings.TrimPrefix(name, "go_test:"));
                    }

                    if (strings.HasPrefix(name, "go_test_bench:"))
                    {
                        t.registerRaceBenchTest(strings.TrimPrefix(name, "go_test_bench:"));
                    }

                }
            else
            }            { 
                // Use a format string to only list packages and commands that have tests.
                const @string format = (@string)"{{if (or .TestGoFiles .XTestGoFiles)}}{{.ImportPath}}{{end}}";

                var cmd = exec.Command("go", "list", "-f", format);
                if (t.race)
                {
                    cmd.Args = append(cmd.Args, "-tags=race");
                }

                cmd.Args = append(cmd.Args, "std");
                if (t.shouldTestCmd())
                {
                    cmd.Args = append(cmd.Args, "cmd");
                }

                cmd.Stderr = @new<bytes.Buffer>();
                var (all, err) = cmd.Output();
                if (err != null)
                {
                    fatalf("Error running go list std cmd: %v:\n%s", err, cmd.Stderr);
                }

                var pkgs = strings.Fields(string(all));
                {
                    var pkg__prev1 = pkg;

                    foreach (var (_, __pkg) in pkgs)
                    {
                        pkg = __pkg;
                        t.registerStdTest(pkg);
                    }

                    pkg = pkg__prev1;
                }

                if (t.race)
                {
                    {
                        var pkg__prev1 = pkg;

                        foreach (var (_, __pkg) in pkgs)
                        {
                            pkg = __pkg;
                            if (t.packageHasBenchmarks(pkg))
                            {
                                t.registerRaceBenchTest(pkg);
                            }

                        }

                        pkg = pkg__prev1;
                    }
                }

            } 

            // Test the os/user package in the pure-Go mode too.
            if (!t.compileOnly)
            {
                t.tests = append(t.tests, new distTest(name:"osusergo",heading:"os/user with tag osusergo",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),t.timeout(300),"-tags=osusergo","os/user")returnnil},));
            }

            if (t.iOS() && !t.compileOnly)
            {
                t.tests = append(t.tests, new distTest(name:"x509omitbundledroots",heading:"crypto/x509 without bundled roots",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),t.timeout(300),"-tags=x509omitbundledroots","-run=OmitBundledRoots","crypto/x509")returnnil},));
            } 

            // Test the ios build tag on darwin/amd64 for the iOS simulator.
            if (goos == "darwin" && !t.iOS())
            {
                t.tests = append(t.tests, new distTest(name:"amd64ios",heading:"ios tag on darwin/amd64",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),t.timeout(300),"-tags=ios","-run=SystemRoots","crypto/x509")returnnil},));
            }

            if (t.race)
            {
                return ;
            } 

            // Runtime CPU tests.
            if (!t.compileOnly && goos != "js")
            { // js can't handle -cpu != 1
                @string testName = "runtime:cpu124";
                t.tests = append(t.tests, new distTest(name:testName,heading:"GOMAXPROCS=2 runtime -cpu=1,2,4 -quick",fn:func(dt*distTest)error{cmd:=t.addCmd(dt,"src",t.goTest(),t.timeout(300),"runtime","-cpu=1,2,4","-quick")cmd.Env=append(os.Environ(),"GOMAXPROCS=2")returnnil},));

            } 

            // This test needs its stdout/stderr to be terminals, so we don't run it from cmd/go's tests.
            // See issue 18153.
            if (goos == "linux")
            {
                t.tests = append(t.tests, new distTest(name:"cmd_go_test_terminal",heading:"cmd/go terminal test",fn:func(dt*distTest)error{t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)if!stdOutErrAreTerminals(){fmt.Println("skipping terminal test; stdout/stderr not terminals")returnnil}cmd:=exec.Command("go","test")cmd.Dir=filepath.Join(os.Getenv("GOROOT"),"src/cmd/go/testdata/testterminal18153")cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrreturncmd.Run()},));
            } 

            // On the builders only, test that a moved GOROOT still works.
            // Fails on iOS because CC_FOR_TARGET refers to clangwrap.sh
            // in the unmoved GOROOT.
            // Fails on Android and js/wasm with an exec format error.
            // Fails on plan9 with "cannot find GOROOT" (issue #21016).
            if (os.Getenv("GO_BUILDER_NAME") != "" && goos != "android" && !t.iOS() && goos != "plan9" && goos != "js")
            {
                t.tests = append(t.tests, new distTest(name:"moved_goroot",heading:"moved GOROOT",fn:func(dt*distTest)error{t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)moved:=goroot+"-moved"iferr:=os.Rename(goroot,moved);err!=nil{ifgoos=="windows"{log.Printf("skipping test on Windows")returnnil}returnerr}cmd:=exec.Command(filepath.Join(moved,"bin","go"),"test","fmt")cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrfor_,e:=rangeos.Environ(){if!strings.HasPrefix(e,"GOROOT=")&&!strings.HasPrefix(e,"GOCACHE="){cmd.Env=append(cmd.Env,e)}}err:=cmd.Run()ifrerr:=os.Rename(moved,goroot);rerr!=nil{fatalf("failed to restore GOROOT: %v",rerr)}returnerr},));
            } 

            // Test that internal linking of standard packages does not
            // require libgcc. This ensures that we can install a Go
            // release on a system that does not have a C compiler
            // installed and still build Go programs (that don't use cgo).
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in cgoPackages)
                {
                    pkg = __pkg;
                    if (!t.internalLink())
                    {
                        break;
                    } 

                    // ARM libgcc may be Thumb, which internal linking does not support.
                    if (goarch == "arm")
                    {
                        break;
                    }

                    var pkg = pkg;
                    @string run = default;
                    if (pkg == "net")
                    {
                        run = "TestTCPStress";
                    }

                    t.tests = append(t.tests, new distTest(name:"nolibgcc:"+pkg,heading:"Testing without libgcc.",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"-ldflags=-linkmode=internal -libgcc=none","-run=^Test[^CS]",pkg,t.runFlag(run))returnnil},));

                } 

                // Test internal linking of PIE binaries where it is supported.

                pkg = pkg__prev1;
            }

            if (t.internalLinkPIE())
            {
                t.tests = append(t.tests, new distTest(name:"pie_internal",heading:"internal linking of -buildmode=pie",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"reflect","-buildmode=pie","-ldflags=-linkmode=internal",t.timeout(60))returnnil},)); 
                // Also test a cgo package.
                if (t.cgoEnabled && t.internalLink())
                {
                    t.tests = append(t.tests, new distTest(name:"pie_internal_cgo",heading:"internal linking of -buildmode=pie",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"os/user","-buildmode=pie","-ldflags=-linkmode=internal",t.timeout(60))returnnil},));
                }

            } 

            // sync tests
            if (goos != "js")
            { // js doesn't support -cpu=10
                t.tests = append(t.tests, new distTest(name:"sync_cpu",heading:"sync -cpu=10",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"sync",t.timeout(120),"-cpu=10",t.runFlag(""))returnnil},));

            }

            if (t.raceDetectorSupported())
            {
                t.tests = append(t.tests, new distTest(name:"race",heading:"Testing race detector",fn:t.raceTest,));
            }

            if (t.cgoEnabled && !t.iOS())
            { 
                // Disabled on iOS. golang.org/issue/15919
                t.registerHostTest("cgo_stdio", "../misc/cgo/stdio", "misc/cgo/stdio", ".");
                t.registerHostTest("cgo_life", "../misc/cgo/life", "misc/cgo/life", ".");
                var fortran = os.Getenv("FC");
                if (fortran == "")
                {
                    fortran, _ = exec.LookPath("gfortran");
                }

                if (t.hasBash() && goos != "android" && fortran != "")
                {
                    t.tests = append(t.tests, new distTest(name:"cgo_fortran",heading:"../misc/cgo/fortran",fn:func(dt*distTest)error{t.addCmd(dt,"misc/cgo/fortran","./test.bash",fortran)returnnil},));
                }

                if (t.hasSwig() && goos != "android")
                {
                    t.tests = append(t.tests, new distTest(name:"swig_stdio",heading:"../misc/swig/stdio",fn:func(dt*distTest)error{t.addCmd(dt,"misc/swig/stdio",t.goTest())returnnil},));
                    if (t.hasCxx())
                    {
                        t.tests = append(t.tests, new distTest(name:"swig_callback",heading:"../misc/swig/callback",fn:func(dt*distTest)error{t.addCmd(dt,"misc/swig/callback",t.goTest())returnnil},));
                    }

                }

            }

            if (t.cgoEnabled)
            {
                t.tests = append(t.tests, new distTest(name:"cgo_test",heading:"../misc/cgo/test",fn:t.cgoTest,));
            } 

            // Don't run these tests with $GO_GCFLAGS because most of them
            // assume that they can run "go install" with no -gcflags and not
            // recompile the entire standard library. If make.bash ran with
            // special -gcflags, that's not true.
            if (t.cgoEnabled && gogcflags == "")
            {
                t.registerHostTest("testgodefs", "../misc/cgo/testgodefs", "misc/cgo/testgodefs", ".");

                t.registerTest("testso", "../misc/cgo/testso", t.goTest(), t.timeout(600L), ".");
                t.registerTest("testsovar", "../misc/cgo/testsovar", t.goTest(), t.timeout(600L), ".");
                if (t.supportedBuildmode("c-archive"))
                {
                    t.registerHostTest("testcarchive", "../misc/cgo/testcarchive", "misc/cgo/testcarchive", ".");
                }

                if (t.supportedBuildmode("c-shared"))
                {
                    t.registerHostTest("testcshared", "../misc/cgo/testcshared", "misc/cgo/testcshared", ".");
                }

                if (t.supportedBuildmode("shared"))
                {
                    t.registerTest("testshared", "../misc/cgo/testshared", t.goTest(), t.timeout(600L), ".");
                }

                if (t.supportedBuildmode("plugin"))
                {
                    t.registerTest("testplugin", "../misc/cgo/testplugin", t.goTest(), t.timeout(600L), ".");
                }

                if (gohostos == "linux" && goarch == "amd64")
                {
                    t.registerTest("testasan", "../misc/cgo/testasan", "go", "run", ".");
                }

                if (mSanSupported(goos, goarch))
                {
                    t.registerHostTest("testsanitizers/msan", "../misc/cgo/testsanitizers", "misc/cgo/testsanitizers", ".");
                }

                if (t.hasBash() && goos != "android" && !t.iOS() && gohostos != "windows")
                {
                    t.registerHostTest("cgo_errors", "../misc/cgo/errors", "misc/cgo/errors", ".");
                }

                if (gohostos == "linux" && t.extLink())
                {
                    t.registerTest("testsigfwd", "../misc/cgo/testsigfwd", "go", "run", ".");
                }

            } 

            // Doc tests only run on builders.
            // They find problems approximately never.
            if (goos != "js" && goos != "android" && !t.iOS() && os.Getenv("GO_BUILDER_NAME") != "")
            {
                t.registerTest("doc_progs", "../doc/progs", "go", "run", "run.go");
                t.registerTest("wiki", "../doc/articles/wiki", t.goTest(), ".");
                t.registerTest("codewalk", "../doc/codewalk", t.goTest(), "codewalk_test.go");
            }

            if (goos != "android" && !t.iOS())
            { 
                // There are no tests in this directory, only benchmarks.
                // Check that the test binary builds but don't bother running it.
                // (It has init-time work to set up for the benchmarks that is not worth doing unnecessarily.)
                t.registerTest("bench_go1", "../test/bench/go1", t.goTest(), "-c", "-o=" + os.DevNull);

            }

            if (goos != "android" && !t.iOS())
            { 
                // Only start multiple test dir shards on builders,
                // where they get distributed to multiple machines.
                // See issues 20141 and 31834.
                long nShards = 1L;
                if (os.Getenv("GO_BUILDER_NAME") != "")
                {
                    nShards = 10L;
                }

                {
                    var err__prev2 = err;

                    var (n, err) = strconv.Atoi(os.Getenv("GO_TEST_SHARDS"));

                    if (err == null)
                    {
                        nShards = n;
                    }

                    err = err__prev2;

                }

                {
                    long shard__prev1 = shard;

                    for (long shard = 0L; shard < nShards; shard++)
                    {
                        shard = shard;
                        t.tests = append(t.tests, new distTest(name:fmt.Sprintf("test:%d_%d",shard,nShards),heading:"../test",fn:func(dt*distTest)error{returnt.testDirTest(dt,shard,nShards)},));
                    }


                    shard = shard__prev1;
                }

            } 
            // Only run the API check on fast development platforms. Android, iOS, and JS
            // are always cross-compiled, and the filesystems on our only plan9 builders
            // are too slow to complete in a reasonable timeframe. Every platform checks
            // the API on every GOOS/GOARCH/CGO_ENABLED combination anyway, so we really
            // only need to run this check once anywhere to get adequate coverage.
            if (goos != "android" && !t.iOS() && goos != "js" && goos != "plan9")
            {
                t.tests = append(t.tests, new distTest(name:"api",heading:"API check",fn:func(dt*distTest)error{ift.compileOnly{t.addCmd(dt,"src","go","build","-o",os.DevNull,filepath.Join(goroot,"src/cmd/api/run.go"))returnnil}t.addCmd(dt,"src","go","run",filepath.Join(goroot,"src/cmd/api/run.go"))returnnil},));
            } 

            // Ensure that the toolchain can bootstrap itself.
            // This test adds another ~45s to all.bash if run sequentially, so run it only on the builders.
            if (os.Getenv("GO_BUILDER_NAME") != "" && goos != "android" && !t.iOS())
            {
                t.registerHostTest("reboot", "../misc/reboot", "misc/reboot", ".");
            }

        });

        // isRegisteredTestName reports whether a test named testName has already
        // been registered.
        private static bool isRegisteredTestName(this ptr<tester> _addr_t, @string testName)
        {
            ref tester t = ref _addr_t.val;

            foreach (var (_, tt) in t.tests)
            {
                if (tt.name == testName)
                {
                    return true;
                }

            }
            return false;

        }

        private static void registerTest1(this ptr<tester> _addr_t, bool seq, @string name, @string dirBanner, params object[] cmdline) => func((defer, panic, _) =>
        {
            cmdline = cmdline.Clone();
            ref tester t = ref _addr_t.val;

            var (bin, args) = flattenCmdline(cmdline);
            if (bin == "time" && !t.haveTime)
            {
                bin = args[0L];
                args = args[1L..];

            }

            if (t.isRegisteredTestName(name))
            {
                panic("duplicate registered test name " + name);
            }

            t.tests = append(t.tests, new distTest(name:name,heading:dirBanner,fn:func(dt*distTest)error{ifseq{t.runPending(dt)timelog("start",name)defertimelog("end",name)returnt.dirCmd(filepath.Join(goroot,"src",dirBanner),bin,args).Run()}t.addCmd(dt,filepath.Join(goroot,"src",dirBanner),bin,args)returnnil},));

        });

        private static void registerTest(this ptr<tester> _addr_t, @string name, @string dirBanner, params object[] cmdline)
        {
            cmdline = cmdline.Clone();
            ref tester t = ref _addr_t.val;

            t.registerTest1(false, name, dirBanner, cmdline);
        }

        private static void registerSeqTest(this ptr<tester> _addr_t, @string name, @string dirBanner, params object[] cmdline)
        {
            cmdline = cmdline.Clone();
            ref tester t = ref _addr_t.val;

            t.registerTest1(true, name, dirBanner, cmdline);
        }

        private static ptr<exec.Cmd> bgDirCmd(this ptr<tester> _addr_t, @string dir, @string bin, params @string[] args)
        {
            args = args.Clone();
            ref tester t = ref _addr_t.val;

            var cmd = exec.Command(bin, args);
            if (filepath.IsAbs(dir))
            {
                cmd.Dir = dir;
            }
            else
            {
                cmd.Dir = filepath.Join(goroot, dir);
            }

            return _addr_cmd!;

        }

        private static ptr<exec.Cmd> dirCmd(this ptr<tester> _addr_t, @string dir, params object[] cmdline)
        {
            cmdline = cmdline.Clone();
            ref tester t = ref _addr_t.val;

            var (bin, args) = flattenCmdline(cmdline);
            var cmd = t.bgDirCmd(dir, bin, args);
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            if (vflag > 1L)
            {
                errprintf("%s\n", strings.Join(cmd.Args, " "));
            }

            return _addr_cmd!;

        }

        // flattenCmdline flattens a mixture of string and []string as single list
        // and then interprets it as a command line: first element is binary, then args.
        private static (@string, slice<@string>) flattenCmdline(slice<object> cmdline) => func((_, panic, __) =>
        {
            @string bin = default;
            slice<@string> args = default;

            slice<@string> list = default;
            {
                var x__prev1 = x;

                foreach (var (_, __x) in cmdline)
                {
                    x = __x;
                    switch (x.type())
                    {
                        case @string x:
                            list = append(list, x);
                            break;
                        case slice<@string> x:
                            list = append(list, x);
                            break;
                        default:
                        {
                            var x = x.type();
                            panic("invalid addCmd argument type: " + reflect.TypeOf(x).String());
                            break;
                        }
                    }

                } 

                // The go command is too picky about duplicated flags.
                // Drop all but the last of the allowed duplicated flags.

                x = x__prev1;
            }

            var drop = make_slice<bool>(len(list));
            map have = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{};
            {
                long i__prev1 = i;

                for (long i = 1L; i < len(list); i++)
                {
                    var j = strings.Index(list[i], "=");
                    if (j < 0L)
                    {
                        continue;
                    }

                    var flag = list[i][..j];
                    switch (flag)
                    {
                        case "-run": 

                        case "-tags": 
                            if (have[flag] != 0L)
                            {
                                drop[have[flag]] = true;
                            }

                            have[flag] = i;
                            break;
                    }

                }


                i = i__prev1;
            }
            var @out = list[..0L];
            {
                long i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in list)
                {
                    i = __i;
                    x = __x;
                    if (!drop[i])
                    {
                        out = append(out, x);
                    }

                }

                i = i__prev1;
                x = x__prev1;
            }

            list = out;

            return (list[0L], list[1L..]);

        });

        private static ptr<exec.Cmd> addCmd(this ptr<tester> _addr_t, ptr<distTest> _addr_dt, @string dir, params object[] cmdline)
        {
            cmdline = cmdline.Clone();
            ref tester t = ref _addr_t.val;
            ref distTest dt = ref _addr_dt.val;

            var (bin, args) = flattenCmdline(cmdline);
            ptr<work> w = addr(new work(dt:dt,cmd:t.bgDirCmd(dir,bin,args...),));
            t.worklist = append(t.worklist, w);
            return _addr_w.cmd!;
        }

        private static bool iOS(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            return goos == "darwin" && goarch == "arm64";
        }

        private static void @out(this ptr<tester> _addr_t, @string v)
        {
            ref tester t = ref _addr_t.val;

            if (t.banner == "")
            {
                return ;
            }

            fmt.Println("\n" + t.banner + v);

        }

        private static bool extLink(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            var pair = gohostos + "-" + goarch;
            switch (pair)
            {
                case "aix-ppc64": 

                case "android-arm": 

                case "android-arm64": 

                case "darwin-amd64": 

                case "darwin-arm64": 

                case "dragonfly-amd64": 

                case "freebsd-386": 

                case "freebsd-amd64": 

                case "freebsd-arm": 

                case "linux-386": 

                case "linux-amd64": 

                case "linux-arm": 

                case "linux-arm64": 

                case "linux-ppc64le": 

                case "linux-mips64": 

                case "linux-mips64le": 

                case "linux-mips": 

                case "linux-mipsle": 

                case "linux-s390x": 

                case "netbsd-386": 

                case "netbsd-amd64": 

                case "openbsd-386": 

                case "openbsd-amd64": 

                case "windows-386": 

                case "windows-amd64": 
                    return true;
                    break;
            }
            return false;

        }

        private static bool internalLink(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            if (gohostos == "dragonfly")
            { 
                // linkmode=internal fails on dragonfly since errno is a TLS relocation.
                return false;

            }

            if (gohostarch == "ppc64le")
            { 
                // linkmode=internal fails on ppc64le because cmd/link doesn't
                // handle the TOC correctly (issue 15409).
                return false;

            }

            if (goos == "android")
            {
                return false;
            }

            if (t.iOS())
            {
                return false;
            } 
            // Internally linking cgo is incomplete on some architectures.
            // https://golang.org/issue/10373
            // https://golang.org/issue/14449
            if (goarch == "mips64" || goarch == "mips64le" || goarch == "mips" || goarch == "mipsle")
            {
                return false;
            }

            if (goos == "aix")
            { 
                // linkmode=internal isn't supported.
                return false;

            }

            return true;

        }

        private static bool internalLinkPIE(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            switch (goos + "-" + goarch)
            {
                case "linux-amd64": 

                case "linux-arm64": 

                case "android-arm64": 
                    return true;
                    break;
                case "windows-amd64": 

                case "windows-386": 

                case "windows-arm": 
                    return true;
                    break;
            }
            return false;

        }

        private static bool supportedBuildmode(this ptr<tester> _addr_t, @string mode)
        {
            ref tester t = ref _addr_t.val;

            var pair = goos + "-" + goarch;
            switch (mode)
            {
                case "c-archive": 
                    if (!t.extLink())
                    {
                        return false;
                    }

                    switch (pair)
                    {
                        case "aix-ppc64": 

                        case "darwin-amd64": 

                        case "darwin-arm64": 

                        case "linux-amd64": 

                        case "linux-386": 

                        case "linux-ppc64le": 

                        case "linux-s390x": 

                        case "freebsd-amd64": 

                        case "windows-amd64": 

                        case "windows-386": 
                            return true;
                            break;
                    }
                    return false;
                    break;
                case "c-shared": 
                    switch (pair)
                    {
                        case "linux-386": 

                        case "linux-amd64": 

                        case "linux-arm": 

                        case "linux-arm64": 

                        case "linux-ppc64le": 

                        case "linux-s390x": 

                        case "darwin-amd64": 

                        case "freebsd-amd64": 

                        case "android-arm": 

                        case "android-arm64": 

                        case "android-386": 

                        case "windows-amd64": 

                        case "windows-386": 
                            return true;
                            break;
                    }
                    return false;
                    break;
                case "shared": 
                    switch (pair)
                    {
                        case "linux-386": 

                        case "linux-amd64": 

                        case "linux-arm": 

                        case "linux-arm64": 

                        case "linux-ppc64le": 

                        case "linux-s390x": 
                            return true;
                            break;
                    }
                    return false;
                    break;
                case "plugin": 
                    // linux-arm64 is missing because it causes the external linker
                    // to crash, see https://golang.org/issue/17138
                    switch (pair)
                    {
                        case "linux-386": 

                        case "linux-amd64": 

                        case "linux-arm": 

                        case "linux-s390x": 

                        case "linux-ppc64le": 
                            return true;
                            break;
                        case "darwin-amd64": 
                            return true;
                            break;
                        case "freebsd-amd64": 
                            return true;
                            break;
                    }
                    return false;
                    break;
                case "pie": 
                    switch (pair)
                    {
                        case "aix/ppc64": 

                        case "linux-386": 

                        case "linux-amd64": 

                        case "linux-arm": 

                        case "linux-arm64": 

                        case "linux-ppc64le": 

                        case "linux-s390x": 

                        case "android-amd64": 

                        case "android-arm": 

                        case "android-arm64": 

                        case "android-386": 
                            return true;
                            break;
                        case "darwin-amd64": 
                            return true;
                            break;
                        case "windows-amd64": 

                        case "windows-386": 

                        case "windows-arm": 
                            return true;
                            break;
                    }
                    return false;
                    break;
                default: 
                    fatalf("internal error: unknown buildmode %s", mode);
                    return false;
                    break;
            }

        }

        private static void registerHostTest(this ptr<tester> _addr_t, @string name, @string heading, @string dir, @string pkg) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            t.tests = append(t.tests, new distTest(name:name,heading:heading,fn:func(dt*distTest)error{t.runPending(dt)timelog("start",name)defertimelog("end",name)returnt.runHostTest(dir,pkg)},));
        });

        private static error runHostTest(this ptr<tester> _addr_t, @string dir, @string pkg) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            var (out, err) = exec.Command("go", "env", "GOEXE", "GOTMPDIR").Output();
            if (err != null)
            {
                return error.As(err)!;
            }

            var parts = strings.Split(string(out), "\n");
            if (len(parts) < 2L)
            {
                return error.As(fmt.Errorf("'go env GOEXE GOTMPDIR' output contains <2 lines"))!;
            }

            var GOEXE = strings.TrimSpace(parts[0L]);
            var GOTMPDIR = strings.TrimSpace(parts[1L]);

            var (f, err) = ioutil.TempFile(GOTMPDIR, "test.test-*" + GOEXE);
            if (err != null)
            {
                return error.As(err)!;
            }

            f.Close();
            defer(os.Remove(f.Name()));

            var cmd = t.dirCmd(dir, t.goTest(), "-c", "-o", f.Name(), pkg);
            cmd.Env = append(os.Environ(), "GOARCH=" + gohostarch, "GOOS=" + gohostos);
            {
                var err = cmd.Run();

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            return error.As(t.dirCmd(dir, f.Name(), "-test.short").Run())!;

        });

        private static error cgoTest(this ptr<tester> _addr_t, ptr<distTest> _addr_dt)
        {
            ref tester t = ref _addr_t.val;
            ref distTest dt = ref _addr_dt.val;

            var cmd = t.addCmd(dt, "misc/cgo/test", t.goTest());
            cmd.Env = append(os.Environ(), "GOFLAGS=-ldflags=-linkmode=auto");

            if (t.internalLink())
            {
                cmd = t.addCmd(dt, "misc/cgo/test", t.goTest(), "-tags=internal");
                cmd.Env = append(os.Environ(), "GOFLAGS=-ldflags=-linkmode=internal");
            }

            var pair = gohostos + "-" + goarch;
            switch (pair)
            {
                case "darwin-amd64": 
                    // test linkmode=external, but __thread not supported, so skip testtls.

                case "openbsd-386": 
                    // test linkmode=external, but __thread not supported, so skip testtls.

                case "openbsd-amd64": 
                    // test linkmode=external, but __thread not supported, so skip testtls.

                case "windows-386": 
                    // test linkmode=external, but __thread not supported, so skip testtls.

                case "windows-amd64": 
                    // test linkmode=external, but __thread not supported, so skip testtls.
                    if (!t.extLink())
                    {
                        break;
                    }

                    cmd = t.addCmd(dt, "misc/cgo/test", t.goTest());
                    cmd.Env = append(os.Environ(), "GOFLAGS=-ldflags=-linkmode=external");

                    cmd = t.addCmd(dt, "misc/cgo/test", t.goTest(), "-ldflags", "-linkmode=external -s");
                    break;
                case "aix-ppc64": 


                case "android-arm": 


                case "android-arm64": 


                case "dragonfly-amd64": 


                case "freebsd-386": 


                case "freebsd-amd64": 


                case "freebsd-arm": 


                case "linux-386": 


                case "linux-amd64": 


                case "linux-arm": 


                case "linux-ppc64le": 


                case "linux-s390x": 


                case "netbsd-386": 


                case "netbsd-amd64": 


                case "linux-arm64": 

                    cmd = t.addCmd(dt, "misc/cgo/test", t.goTest());
                    cmd.Env = append(os.Environ(), "GOFLAGS=-ldflags=-linkmode=external"); 
                    // A -g argument in CGO_CFLAGS should not affect how the test runs.
                    cmd.Env = append(cmd.Env, "CGO_CFLAGS=-g0");

                    t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-ldflags", "-linkmode=auto");
                    t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-ldflags", "-linkmode=external");

                    switch (pair)
                    {
                        case "aix-ppc64": 

                        case "netbsd-386": 

                        case "netbsd-amd64": 
                            break;
                        case "freebsd-arm": 
                            break;
                        default: 
                            cmd = t.dirCmd("misc/cgo/test", compilerEnvLookup(defaultcc, goos, goarch), "-xc", "-o", "/dev/null", "-static", "-");
                            cmd.Stdin = strings.NewReader("int main() {}");
                            {
                                var err = cmd.Run();

                                if (err != null)
                                {
                                    fmt.Println("No support for static linking found (lacks libc.a?), skip cgo static linking test.");
                                }
                                else
                                {
                                    if (goos != "android")
                                    {
                                        t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-ldflags", "-linkmode=external -extldflags \"-static -pthread\"");
                                    }

                                    t.addCmd(dt, "misc/cgo/nocgo", t.goTest());
                                    t.addCmd(dt, "misc/cgo/nocgo", t.goTest(), "-ldflags", "-linkmode=external");
                                    if (goos != "android")
                                    {
                                        t.addCmd(dt, "misc/cgo/nocgo", t.goTest(), "-ldflags", "-linkmode=external -extldflags \"-static -pthread\"");
                                        t.addCmd(dt, "misc/cgo/test", t.goTest(), "-tags=static", "-ldflags", "-linkmode=external -extldflags \"-static -pthread\""); 
                                        // -static in CGO_LDFLAGS triggers a different code path
                                        // than -static in -extldflags, so test both.
                                        // See issue #16651.
                                        cmd = t.addCmd(dt, "misc/cgo/test", t.goTest(), "-tags=static");
                                        cmd.Env = append(os.Environ(), "CGO_LDFLAGS=-static -pthread");

                                    }

                                }

                            }


                            if (t.supportedBuildmode("pie"))
                            {
                                t.addCmd(dt, "misc/cgo/test", t.goTest(), "-buildmode=pie");
                                if (t.internalLink() && t.internalLinkPIE())
                                {
                                    t.addCmd(dt, "misc/cgo/test", t.goTest(), "-buildmode=pie", "-ldflags=-linkmode=internal");
                                }

                                t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-buildmode=pie");
                                t.addCmd(dt, "misc/cgo/nocgo", t.goTest(), "-buildmode=pie");

                            }

                            break;
                    }
                    break;
            }

            return error.As(null!)!;

        }

        // run pending test commands, in parallel, emitting headers as appropriate.
        // When finished, emit header for nextTest, which is going to run after the
        // pending commands are done (and runPending returns).
        // A test should call runPending if it wants to make sure that it is not
        // running in parallel with earlier tests, or if it has some other reason
        // for needing the earlier tests to be done.
        private static void runPending(this ptr<tester> _addr_t, ptr<distTest> _addr_nextTest)
        {
            ref tester t = ref _addr_t.val;
            ref distTest nextTest = ref _addr_nextTest.val;

            checkNotStale("go", "std");
            var worklist = t.worklist;
            t.worklist = null;
            {
                var w__prev1 = w;

                foreach (var (_, __w) in worklist)
                {
                    w = __w;
                    w.start = make_channel<bool>();
                    w.end = make_channel<bool>();
                    go_(() => w =>
                    {
                        if (!w.start.Receive())
                        {
                            timelog("skip", w.dt.name);
                            w.@out = (slice<byte>)fmt.Sprintf("skipped due to earlier error\n");
                        }
                        else
                        {
                            timelog("start", w.dt.name);
                            w.@out, w.err = w.cmd.CombinedOutput();
                            if (w.err != null)
                            {
                                if (isUnsupportedVMASize(_addr_w))
                                {
                                    timelog("skip", w.dt.name);
                                    w.@out = (slice<byte>)fmt.Sprintf("skipped due to unsupported VMA\n");
                                    w.err = null;
                                }

                            }

                        }

                        timelog("end", w.dt.name);
                        w.end.Send(true);

                    }(w));

                }

                w = w__prev1;
            }

            long started = 0L;
            long ended = 0L;
            ptr<distTest> last;
            while (ended < len(worklist))
            {
                while (started < len(worklist) && started - ended < maxbg)
                {
                    var w = worklist[started];
                    started++;
                    w.start.Send(!t.failed || t.keepGoing);
                }

                w = worklist[ended];
                var dt = w.dt;
                if (dt.heading != "" && t.lastHeading != dt.heading)
                {
                    t.lastHeading = dt.heading;
                    t.@out(dt.heading);
                }

                if (dt != last)
                { 
                    // Assumes all the entries for a single dt are in one worklist.
                    last = w.dt;
                    if (vflag > 0L)
                    {
                        fmt.Printf("# go tool dist test -run=^%s$\n", dt.name);
                    }

                }

                if (vflag > 1L)
                {
                    errprintf("%s\n", strings.Join(w.cmd.Args, " "));
                }

                ended++;
                w.end.Receive();
                os.Stdout.Write(w.@out);
                if (w.err != null)
                {
                    log.Printf("Failed: %v", w.err);
                    t.failed = true;
                }

                checkNotStale("go", "std");

            }

            if (t.failed && !t.keepGoing)
            {
                fatalf("FAILED");
            }

            {
                var dt__prev1 = dt;

                dt = nextTest;

                if (dt != null)
                {
                    if (dt.heading != "" && t.lastHeading != dt.heading)
                    {
                        t.lastHeading = dt.heading;
                        t.@out(dt.heading);
                    }

                    if (vflag > 0L)
                    {
                        fmt.Printf("# go tool dist test -run=^%s$\n", dt.name);
                    }

                }

                dt = dt__prev1;

            }

        }

        private static bool hasBash(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            switch (gohostos)
            {
                case "windows": 

                case "plan9": 
                    return false;
                    break;
            }
            return true;

        }

        private static bool hasCxx(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            var (cxx, _) = exec.LookPath(compilerEnvLookup(defaultcxx, goos, goarch));
            return cxx != "";
        }

        private static bool hasSwig(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            var (swig, err) = exec.LookPath("swig");
            if (err != null)
            {
                return false;
            } 

            // Check that swig was installed with Go support by checking
            // that a go directory exists inside the swiglib directory.
            // See https://golang.org/issue/23469.
            var (output, err) = exec.Command(swig, "-go", "-swiglib").Output();
            if (err != null)
            {
                return false;
            }

            var swigDir = strings.TrimSpace(string(output));

            _, err = os.Stat(filepath.Join(swigDir, "go"));
            if (err != null)
            {
                return false;
            } 

            // Check that swig has a new enough version.
            // See https://golang.org/issue/22858.
            var (out, err) = exec.Command(swig, "-version").CombinedOutput();
            if (err != null)
            {
                return false;
            }

            var re = regexp.MustCompile("[vV]ersion +([\\d]+)([.][\\d]+)?([.][\\d]+)?");
            var matches = re.FindSubmatch(out);
            if (matches == null)
            { 
                // Can't find version number; hope for the best.
                return true;

            }

            var (major, err) = strconv.Atoi(string(matches[1L]));
            if (err != null)
            { 
                // Can't find version number; hope for the best.
                return true;

            }

            if (major < 3L)
            {
                return false;
            }

            if (major > 3L)
            { 
                // 4.0 or later
                return true;

            } 

            // We have SWIG version 3.x.
            if (len(matches[2L]) > 0L)
            {
                var (minor, err) = strconv.Atoi(string(matches[2L][1L..]));
                if (err != null)
                {
                    return true;
                }

                if (minor > 0L)
                { 
                    // 3.1 or later
                    return true;

                }

            } 

            // We have SWIG version 3.0.x.
            if (len(matches[3L]) > 0L)
            {
                var (patch, err) = strconv.Atoi(string(matches[3L][1L..]));
                if (err != null)
                {
                    return true;
                }

                if (patch < 6L)
                { 
                    // Before 3.0.6.
                    return false;

                }

            }

            return true;

        }

        private static bool raceDetectorSupported(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            if (gohostos != goos)
            {
                return false;
            }

            if (!t.cgoEnabled)
            {
                return false;
            }

            if (!raceDetectorSupported(goos, goarch))
            {
                return false;
            } 
            // The race detector doesn't work on Alpine Linux:
            // golang.org/issue/14481
            if (isAlpineLinux())
            {
                return false;
            } 
            // NetBSD support is unfinished.
            // golang.org/issue/26403
            if (goos == "netbsd")
            {
                return false;
            }

            return true;

        }

        private static bool isAlpineLinux()
        {
            if (runtime.GOOS != "linux")
            {
                return false;
            }

            var (fi, err) = os.Lstat("/etc/alpine-release");
            return err == null && fi.Mode().IsRegular();

        }

        private static @string runFlag(this ptr<tester> _addr_t, @string rx)
        {
            ref tester t = ref _addr_t.val;

            if (t.compileOnly)
            {
                return "-run=^$";
            }

            return "-run=" + rx;

        }

        private static error raceTest(this ptr<tester> _addr_t, ptr<distTest> _addr_dt)
        {
            ref tester t = ref _addr_t.val;
            ref distTest dt = ref _addr_dt.val;

            t.addCmd(dt, "src", t.goTest(), "-race", t.runFlag("Output"), "runtime/race");
            t.addCmd(dt, "src", t.goTest(), "-race", t.runFlag("TestParse|TestEcho|TestStdinCloseRace|TestClosedPipeRace|TestTypeRace|TestFdRace|TestFdReadRace|TestFileCloseRace"), "flag", "net", "os", "os/exec", "encoding/gob"); 
            // We don't want the following line, because it
            // slows down all.bash (by 10 seconds on my laptop).
            // The race builder should catch any error here, but doesn't.
            // TODO(iant): Figure out how to catch this.
            // t.addCmd(dt, "src", t.goTest(),  "-race", "-run=TestParallelTest", "cmd/go")
            if (t.cgoEnabled)
            { 
                // Building misc/cgo/test takes a long time.
                // There are already cgo-enabled packages being tested with the race detector.
                // We shouldn't need to redo all of misc/cgo/test too.
                // The race buildler will take care of this.
                // cmd := t.addCmd(dt, "misc/cgo/test", t.goTest(), "-race")
                // cmd.Env = append(os.Environ(), "GOTRACEBACK=2")
            }

            if (t.extLink())
            { 
                // Test with external linking; see issue 9133.
                t.addCmd(dt, "src", t.goTest(), "-race", "-ldflags=-linkmode=external", t.runFlag("TestParse|TestEcho|TestStdinCloseRace"), "flag", "os/exec");

            }

            return error.As(null!)!;

        }

        private static var runtest = default;

        private static error testDirTest(this ptr<tester> _addr_t, ptr<distTest> _addr_dt, long shard, long shards)
        {
            ref tester t = ref _addr_t.val;
            ref distTest dt = ref _addr_dt.val;

            runtest.Do(() =>
            {
                var (f, err) = ioutil.TempFile("", "runtest-*.exe"); // named exe for Windows, but harmless elsewhere
                if (err != null)
                {
                    runtest.err = err;
                    return ;
                }

                f.Close();

                runtest.exe = f.Name();
                xatexit(() =>
                {
                    os.Remove(runtest.exe);
                });

                var cmd = t.dirCmd("test", "go", "build", "-o", runtest.exe, "run.go");
                cmd.Env = append(os.Environ(), "GOOS=" + gohostos, "GOARCH=" + gohostarch);
                runtest.err = cmd.Run();

            });
            if (runtest.err != null)
            {
                return error.As(runtest.err)!;
            }

            if (t.compileOnly)
            {
                return error.As(null!)!;
            }

            t.addCmd(dt, "test", runtest.exe, fmt.Sprintf("--shard=%d", shard), fmt.Sprintf("--shards=%d", shards));
            return error.As(null!)!;

        }

        // cgoPackages is the standard packages that use cgo.
        private static @string cgoPackages = new slice<@string>(new @string[] { "crypto/x509", "net", "os/user" });

        private static slice<byte> funcBenchmark = (slice<byte>)"\nfunc Benchmark";

        // packageHasBenchmarks reports whether pkg has benchmarks.
        // On any error, it conservatively returns true.
        //
        // This exists just to eliminate work on the builders, since compiling
        // a test in race mode just to discover it has no benchmarks costs a
        // second or two per package, and this function returns false for
        // about 100 packages.
        private static bool packageHasBenchmarks(this ptr<tester> _addr_t, @string pkg) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            var pkgDir = filepath.Join(goroot, "src", pkg);
            var (d, err) = os.Open(pkgDir);
            if (err != null)
            {
                return true; // conservatively
            }

            defer(d.Close());
            var (names, err) = d.Readdirnames(-1L);
            if (err != null)
            {
                return true; // conservatively
            }

            foreach (var (_, name) in names)
            {
                if (!strings.HasSuffix(name, "_test.go"))
                {
                    continue;
                }

                var (slurp, err) = ioutil.ReadFile(filepath.Join(pkgDir, name));
                if (err != null)
                {
                    return true; // conservatively
                }

                if (bytes.Contains(slurp, funcBenchmark))
                {
                    return true;
                }

            }
            return false;

        });

        // makeGOROOTUnwritable makes all $GOROOT files & directories non-writable to
        // check that no tests accidentally write to $GOROOT.
        private static Action makeGOROOTUnwritable(this ptr<tester> _addr_t) => func((_, panic, __) =>
        {
            Action undo = default;
            ref tester t = ref _addr_t.val;

            var dir = os.Getenv("GOROOT");
            if (dir == "")
            {
                panic("GOROOT not set");
            }

            private partial struct pathMode
            {
                public @string path;
                public os.FileMode mode;
            }
            slice<pathMode> dirs = default; // in lexical order

            undo = () =>
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in dirs)
                    {
                        i = __i;
                        os.Chmod(dirs[i].path, dirs[i].mode); // best effort
                    }

                    i = i__prev1;
                }
            }
;

            var gocache = os.Getenv("GOCACHE");
            if (gocache == "")
            {
                panic("GOCACHE not set");
            }

            var (gocacheSubdir, _) = filepath.Rel(dir, gocache);

            filepath.Walk(dir, (path, info, err) =>
            {
                {
                    var suffix = strings.TrimPrefix(path, dir + string(filepath.Separator));

                    if (suffix != "")
                    {
                        if (suffix == gocacheSubdir)
                        { 
                            // Leave GOCACHE writable: we may need to write test binaries into it.
                            return filepath.SkipDir;

                        }

                        if (suffix == ".git")
                        { 
                            // Leave Git metadata in whatever state it was in. It may contain a lot
                            // of files, and it is highly unlikely that a test will try to modify
                            // anything within that directory.
                            return filepath.SkipDir;

                        }

                    }

                }

                if (err == null)
                {
                    var mode = info.Mode();
                    if (mode & 0222L != 0L && (mode.IsDir() || mode.IsRegular()))
                    {
                        dirs = append(dirs, new pathMode(path,mode));
                    }

                }

                return null;

            }); 

            // Run over list backward to chmod children before parents.
            {
                var i__prev1 = i;

                for (var i = len(dirs) - 1L; i >= 0L; i--)
                {
                    var err = os.Chmod(dirs[i].path, dirs[i].mode & ~0222L);
                    if (err != null)
                    {
                        dirs = dirs[i..]; // Only undo what we did so far.
                        undo();
                        fatalf("failed to make GOROOT read-only: %v", err);

                    }

                }


                i = i__prev1;
            }

            return undo;

        });

        // shouldUsePrecompiledStdTest reports whether "dist test" should use
        // a pre-compiled go test binary on disk rather than running "go test"
        // and compiling it again. This is used by our slow qemu-based builder
        // that do full processor emulation where we cross-compile the
        // make.bash step as well as pre-compile each std test binary.
        //
        // This only reports true if dist is run with an single go_test:foo
        // argument (as the build coordinator does with our slow qemu-based
        // builders), we're in a builder environment ("GO_BUILDER_NAME" is set),
        // and the pre-built test binary exists.
        private static bool shouldUsePrecompiledStdTest(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            var bin = t.prebuiltGoPackageTestBinary();
            if (bin == "")
            {
                return false;
            }

            var (_, err) = os.Stat(bin);
            return err == null;

        }

        private static bool shouldTestCmd(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            if (goos == "js" && goarch == "wasm")
            { 
                // Issues 25911, 35220
                return false;

            }

            return true;

        }

        // prebuiltGoPackageTestBinary returns the path where we'd expect
        // the pre-built go test binary to be on disk when dist test is run with
        // a single argument.
        // It returns an empty string if a pre-built binary should not be used.
        private static @string prebuiltGoPackageTestBinary(this ptr<tester> _addr_t)
        {
            ref tester t = ref _addr_t.val;

            if (len(stdMatches) != 1L || t.race || t.compileOnly || os.Getenv("GO_BUILDER_NAME") == "")
            {
                return "";
            }

            var pkg = stdMatches[0L];
            return filepath.Join(os.Getenv("GOROOT"), "src", pkg, path.Base(pkg) + ".test");

        }

        // runPrecompiledStdTest runs the pre-compiled standard library package test binary.
        // See shouldUsePrecompiledStdTest above; it must return true for this to be called.
        private static error runPrecompiledStdTest(this ptr<tester> _addr_t, time.Duration timeout) => func((defer, _, __) =>
        {
            ref tester t = ref _addr_t.val;

            var bin = t.prebuiltGoPackageTestBinary();
            fmt.Fprintf(os.Stderr, "# %s: using pre-built %s...\n", stdMatches[0L], bin);
            var cmd = exec.Command(bin, "-test.short", "-test.timeout=" + timeout.String());
            cmd.Dir = filepath.Dir(bin);
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            {
                var err = cmd.Start();

                if (err != null)
                {
                    return error.As(err)!;
                } 
                // And start a timer to kill the process if it doesn't kill
                // itself in the prescribed timeout.

            } 
            // And start a timer to kill the process if it doesn't kill
            // itself in the prescribed timeout.
            const float backupKillFactor = (float)1.05F; // add 5%
 // add 5%
            var timer = time.AfterFunc(time.Duration(float64(timeout) * backupKillFactor), () =>
            {
                fmt.Fprintf(os.Stderr, "# %s: timeout running %s; killing...\n", stdMatches[0L], bin);
                cmd.Process.Kill();
            });
            defer(timer.Stop());
            return error.As(cmd.Wait())!;

        });

        // raceDetectorSupported is a copy of the function
        // cmd/internal/sys.RaceDetectorSupported, which can't be used here
        // because cmd/dist has to be buildable by Go 1.4.
        // The race detector only supports 48-bit VMA on arm64. But we don't have
        // a good solution to check VMA size(See https://golang.org/issue/29948)
        // raceDetectorSupported will always return true for arm64. But race
        // detector tests may abort on non 48-bit VMA configuration, the tests
        // will be marked as "skipped" in this case.
        private static bool raceDetectorSupported(@string goos, @string goarch)
        {
            switch (goos)
            {
                case "linux": 
                    return goarch == "amd64" || goarch == "ppc64le" || goarch == "arm64";
                    break;
                case "darwin": 

                case "freebsd": 

                case "netbsd": 

                case "windows": 
                    return goarch == "amd64";
                    break;
                default: 
                    return false;
                    break;
            }

        }

        // mSanSupported is a copy of the function cmd/internal/sys.MSanSupported,
        // which can't be used here because cmd/dist has to be buildable by Go 1.4.
        private static bool mSanSupported(@string goos, @string goarch)
        {
            switch (goos)
            {
                case "linux": 
                    return goarch == "amd64" || goarch == "arm64";
                    break;
                default: 
                    return false;
                    break;
            }

        }

        // isUnsupportedVMASize reports whether the failure is caused by an unsupported
        // VMA for the race detector (for example, running the race detector on an
        // arm64 machine configured with 39-bit VMA)
        private static bool isUnsupportedVMASize(ptr<work> _addr_w)
        {
            ref work w = ref _addr_w.val;

            slice<byte> unsupportedVMA = (slice<byte>)"unsupported VMA range";
            return w.dt.name == "race" && bytes.Contains(w.@out, unsupportedVMA);
        }
    }
}
