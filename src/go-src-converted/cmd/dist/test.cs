// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:59:57 UTC
// Original source: C:\Go\src\cmd\dist\test.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
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
            bool noRebuild = default;
            flag.BoolVar(ref t.listMode, "list", false, "list available tests");
            flag.BoolVar(ref t.rebuild, "rebuild", false, "rebuild everything first");
            flag.BoolVar(ref noRebuild, "no-rebuild", false, "overrides -rebuild (historical dreg)");
            flag.BoolVar(ref t.keepGoing, "k", false, "keep going even when error occurred");
            flag.BoolVar(ref t.race, "race", false, "run in race builder mode (different set of tests)");
            flag.BoolVar(ref t.compileOnly, "compile-only", false, "compile tests, but don't run them. This is for some builders. Not all dist tests respect this flag, but most do.");
            flag.StringVar(ref t.banner, "banner", "##### ", "banner prefix; blank means no section banners");
            flag.StringVar(ref t.runRxStr, "run", os.Getenv("GOTESTONLY"), "run only those tests matching the regular expression; empty means to run all. " + "Special exception: if the string begins with '!', the match is inverted.");
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
            public slice<ref work> worklist;
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
            public Func<ref distTest, error> fn;
        }

        private static void run(this ref tester t)
        {
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

            var (slurp, err) = exec.Command("go", "env", "CGO_ENABLED").Output();
            if (err != null)
            {
                log.Fatalf("Error running go env CGO_ENABLED: %v", err);
            }
            t.cgoEnabled, _ = strconv.ParseBool(strings.TrimSpace(string(slurp)));
            if (flag.NArg() > 0L && t.runRxStr != "")
            {
                log.Fatalf("the -run regular expression flag is mutually exclusive with test name arguments");
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
            if (!t.listMode)
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
                        log.Fatalf("failed to parse $GO_TEST_TIMEOUT_SCALE = %q as integer: %v", s, err);
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
                return;
            } 

            // We must unset GOROOT_FINAL before tests, because runtime/debug requires
            // correct access to source code, so if we have GOROOT_FINAL in effect,
            // at least runtime/debug test will fail.
            // If GOROOT_FINAL was set before, then now all the commands will appear stale.
            // Nothing we can do about that other than not checking them below.
            // (We call checkNotStale but only with "std" not "cmd".)
            os.Setenv("GOROOT_FINAL_OLD", os.Getenv("GOROOT_FINAL")); // for cmd/link test
            os.Unsetenv("GOROOT_FINAL");

            foreach (var (_, name) in t.runNames)
            {
                if (!t.isRegisteredTestName(name))
                {
                    log.Fatalf("unknown test %q", name);
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
                    var dt = dt; // dt used in background after this iteration
                    {
                        var err = dt.fn(ref dt);

                        if (err != null)
                        {
                            t.runPending(ref dt); // in case that hasn't been done yet
                            t.failed = true;
                            if (t.keepGoing)
                            {
                                log.Printf("Failed: %v", err);
                            }
                            else
                            {
                                log.Fatalf("Failed: %v", err);
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
                os.Exit(1L);
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

        private static bool shouldRunTest(this ref tester t, @string name)
        {
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

        // goTest returns the beginning of the go test command line.
        // Callers should use goTest and then pass flags overriding these
        // defaults as later arguments in the command line.
        private static slice<@string> goTest(this ref tester t)
        {
            return new slice<@string>(new @string[] { "go", "test", "-short", "-count=1", t.tags(), t.runFlag("") });
        }

        private static @string tags(this ref tester t)
        {
            if (t.iOS())
            {
                return "-tags=lldb";
            }
            return "-tags=";
        }

        private static @string timeout(this ref tester t, long sec)
        {
            return "-timeout=" + fmt.Sprint(time.Duration(sec) * time.Second * time.Duration(t.timeoutScale));
        }

        // ranGoTest and stdMatches are state closed over by the stdlib
        // testing func in registerStdTest below. The tests are run
        // sequentially, so there's no need for locks.
        //
        // ranGoBench and benchMatches are the same, but are only used
        // in -race mode.
        private static bool ranGoTest = default;        private static slice<@string> stdMatches = default;        private static bool ranGoBench = default;        private static slice<@string> benchMatches = default;

        private static void registerStdTest(this ref tester _t, @string pkg) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
            @string testName = "go_test:" + pkg;
            if (t.runRx == null || t.runRx.MatchString(testName) == t.runRxWant)
            {
                stdMatches = append(stdMatches, pkg);
            }
            long timeoutSec = 180L;
            if (pkg == "cmd/go")
            {
                timeoutSec *= 2L;
            }
            t.tests = append(t.tests, new distTest(name:testName,heading:"Testing packages.",fn:func(dt*distTest)error{ifranGoTest{returnnil}t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)ranGoTest=trueargs:=[]string{"test","-short",t.tags(),t.timeout(timeoutSec),"-gcflags=all="+gogcflags,}ift.race{args=append(args,"-race")}ift.compileOnly{args=append(args,"-run=^$")}args=append(args,stdMatches...)cmd:=exec.Command("go",args...)cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrreturncmd.Run()},));
        });

        private static void registerRaceBenchTest(this ref tester _t, @string pkg) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
            @string testName = "go_test_bench:" + pkg;
            if (t.runRx == null || t.runRx.MatchString(testName) == t.runRxWant)
            {
                benchMatches = append(benchMatches, pkg);
            }
            t.tests = append(t.tests, new distTest(name:testName,heading:"Running benchmarks briefly.",fn:func(dt*distTest)error{ifranGoBench{returnnil}t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)ranGoBench=trueargs:=[]string{"test","-short","-race","-run=^$","-benchtime=.1s","-cpu=4",}if!t.compileOnly{args=append(args,"-bench=.*")}args=append(args,benchMatches...)cmd:=exec.Command("go",args...)cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrreturncmd.Run()},));
        });

        // stdOutErrAreTerminals is defined in test_linux.go, to report
        // whether stdout & stderr are terminals.
        private static Func<bool> stdOutErrAreTerminals = default;

        private static void registerTests(this ref tester _t) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
            if (strings.HasSuffix(os.Getenv("GO_BUILDER_NAME"), "-vetall"))
            { 
                // Run vet over std and cmd and call it quits.
                foreach (var (k) in cgoEnabled)
                {
                    var osarch = k;
                    t.tests = append(t.tests, new distTest(name:"vet/"+osarch,heading:"cmd/vet/all",fn:func(dt*distTest)error{t.addCmd(dt,"src/cmd/vet/all","go","run","main.go","-p="+osarch)returnnil},));
                }
                return;
            } 

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
                const @string format = "{{if (or .TestGoFiles .XTestGoFiles)}}{{.ImportPath}}{{end}}";

                var cmd = exec.Command("go", "list", "-f", format);
                if (t.race)
                {
                    cmd.Args = append(cmd.Args, "-tags=race");
                }
                cmd.Args = append(cmd.Args, "std");
                if (!t.race)
                {
                    cmd.Args = append(cmd.Args, "cmd");
                }
                var (all, err) = cmd.Output();
                if (err != null)
                {
                    log.Fatalf("Error running go list std cmd: %v, %s", err, all);
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
            if (t.race)
            {
                return;
            } 

            // Runtime CPU tests.
            if (!t.compileOnly)
            {
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
            // Fails on Android with an exec format error.
            // Fails on plan9 with "cannot find GOROOT" (issue #21016).
            if (os.Getenv("GO_BUILDER_NAME") != "" && goos != "android" && !t.iOS() && goos != "plan9")
            {
                t.tests = append(t.tests, new distTest(name:"moved_goroot",heading:"moved GOROOT",fn:func(dt*distTest)error{t.runPending(dt)timelog("start",dt.name)defertimelog("end",dt.name)moved:=goroot+"-moved"iferr:=os.Rename(goroot,moved);err!=nil{ifgoos=="windows"{log.Printf("skipping test on Windows")returnnil}returnerr}cmd:=exec.Command(filepath.Join(moved,"bin","go"),"test","fmt")cmd.Stdout=os.Stdoutcmd.Stderr=os.Stderrfor_,e:=rangeos.Environ(){if!strings.HasPrefix(e,"GOROOT=")&&!strings.HasPrefix(e,"GOCACHE="){cmd.Env=append(cmd.Env,e)}}cmd.Env=append(cmd.Env,"GOCACHE=off")err:=cmd.Run()ifrerr:=os.Rename(moved,goroot);rerr!=nil{log.Fatalf("failed to restore GOROOT: %v",rerr)}returnerr},));
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
                    t.tests = append(t.tests, new distTest(name:"nolibgcc:"+pkg,heading:"Testing without libgcc.",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"-ldflags=-linkmode=internal -libgcc=none",pkg,t.runFlag(run))returnnil},));
                } 

                // Test internal linking of PIE binaries where it is supported.

                pkg = pkg__prev1;
            }

            if (goos == "linux" && goarch == "amd64" && !isAlpineLinux())
            { 
                // Issue 18243: We don't have a way to set the default
                // dynamic linker used in internal linking mode. So
                // this test is skipped on Alpine.
                t.tests = append(t.tests, new distTest(name:"pie_internal",heading:"internal linking of -buildmode=pie",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"reflect","-buildmode=pie","-ldflags=-linkmode=internal",t.timeout(60))returnnil},));
            } 

            // sync tests
            t.tests = append(t.tests, new distTest(name:"sync_cpu",heading:"sync -cpu=10",fn:func(dt*distTest)error{t.addCmd(dt,"src",t.goTest(),"sync",t.timeout(120),"-cpu=10",t.runFlag(""))returnnil},));

            if (t.raceDetectorSupported())
            {
                t.tests = append(t.tests, new distTest(name:"race",heading:"Testing race detector",fn:t.raceTest,));
            }
            if (t.cgoEnabled && !t.iOS())
            { 
                // Disabled on iOS. golang.org/issue/15919
                t.tests = append(t.tests, new distTest(name:"cgo_stdio",heading:"../misc/cgo/stdio",fn:func(dt*distTest)error{t.addCmd(dt,"misc/cgo/stdio","go","run",filepath.Join(os.Getenv("GOROOT"),"test/run.go"),"-",".")returnnil},));
                t.tests = append(t.tests, new distTest(name:"cgo_life",heading:"../misc/cgo/life",fn:func(dt*distTest)error{t.addCmd(dt,"misc/cgo/life","go","run",filepath.Join(os.Getenv("GOROOT"),"test/run.go"),"-",".")returnnil},));
                var fortran = os.Getenv("FC");
                if (fortran == "")
                {
                    fortran, _ = exec.LookPath("gfortran");
                }
                if (t.hasBash() && fortran != "")
                {
                    t.tests = append(t.tests, new distTest(name:"cgo_fortran",heading:"../misc/cgo/fortran",fn:func(dt*distTest)error{t.addCmd(dt,"misc/cgo/fortran","./test.bash",fortran)returnnil},));
                }
                if (t.hasSwig() && goos != "android")
                {
                    t.tests = append(t.tests, new distTest(name:"swig_stdio",heading:"../misc/swig/stdio",fn:func(dt*distTest)error{t.addCmd(dt,"misc/swig/stdio",t.goTest())returnnil},));
                    {
                        var (cxx, _) = exec.LookPath(compilerEnvLookup(defaultcxx, goos, goarch));

                        if (cxx != "")
                        {
                            t.tests = append(t.tests, new distTest(name:"swig_callback",heading:"../misc/swig/callback",fn:func(dt*distTest)error{t.addCmd(dt,"misc/swig/callback",t.goTest())returnnil},));
                        }

                    }
                }
            }
            if (t.cgoEnabled)
            {
                t.tests = append(t.tests, new distTest(name:"cgo_test",heading:"../misc/cgo/test",fn:t.cgoTest,));
            }
            if (t.hasBash() && t.cgoEnabled && goos != "android" && goos != "darwin")
            {
                t.registerTest("testgodefs", "../misc/cgo/testgodefs", "./test.bash");
            } 

            // Don't run these tests with $GO_GCFLAGS because most of them
            // assume that they can run "go install" with no -gcflags and not
            // recompile the entire standard library. If make.bash ran with
            // special -gcflags, that's not true.
            if (t.cgoEnabled && gogcflags == "")
            {
                if (t.cgoTestSOSupported())
                {
                    t.tests = append(t.tests, new distTest(name:"testso",heading:"../misc/cgo/testso",fn:func(dt*distTest)error{returnt.cgoTestSO(dt,"misc/cgo/testso")},));
                    t.tests = append(t.tests, new distTest(name:"testsovar",heading:"../misc/cgo/testsovar",fn:func(dt*distTest)error{returnt.cgoTestSO(dt,"misc/cgo/testsovar")},));
                }
                if (t.supportedBuildmode("c-archive"))
                {
                    t.registerHostTest("testcarchive", "../misc/cgo/testcarchive", "misc/cgo/testcarchive", "carchive_test.go");
                }
                if (t.supportedBuildmode("c-shared"))
                {
                    t.registerHostTest("testcshared", "../misc/cgo/testcshared", "misc/cgo/testcshared", "cshared_test.go");
                }
                if (t.supportedBuildmode("shared"))
                {
                    t.registerTest("testshared", "../misc/cgo/testshared", t.goTest(), t.timeout(600L));
                }
                if (t.supportedBuildmode("plugin"))
                {
                    t.registerTest("testplugin", "../misc/cgo/testplugin", "./test.bash");
                }
                if (gohostos == "linux" && goarch == "amd64")
                {
                    t.registerTest("testasan", "../misc/cgo/testasan", "go", "run", "main.go");
                }
                if (goos == "linux" && goarch == "amd64")
                {
                    t.registerHostTest("testsanitizers/msan", "../misc/cgo/testsanitizers", "misc/cgo/testsanitizers", ".");
                }
                if (t.hasBash() && goos != "android" && !t.iOS() && gohostos != "windows")
                {
                    t.registerHostTest("cgo_errors", "../misc/cgo/errors", "misc/cgo/errors", ".");
                }
                if (gohostos == "linux" && t.extLink())
                {
                    t.registerTest("testsigfwd", "../misc/cgo/testsigfwd", "go", "run", "main.go");
                }
            } 

            // Doc tests only run on builders.
            // They find problems approximately never.
            if (t.hasBash() && goos != "nacl" && goos != "android" && !t.iOS() && os.Getenv("GO_BUILDER_NAME") != "")
            {
                t.registerTest("doc_progs", "../doc/progs", "time", "go", "run", "run.go");
                t.registerTest("wiki", "../doc/articles/wiki", "./test.bash");
                t.registerTest("codewalk", "../doc/codewalk", "time", "./run");
            }
            if (goos != "android" && !t.iOS())
            {
                t.registerTest("bench_go1", "../test/bench/go1", t.goTest(), t.timeout(600L));
            }
            if (goos != "android" && !t.iOS())
            { 
                // Only start multiple test dir shards on builders,
                // where they get distributed to multiple machines.
                // See issue 20141.
                long nShards = 1L;
                if (os.Getenv("GO_BUILDER_NAME") != "")
                {
                    nShards = 10L;
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
            if (goos != "nacl" && goos != "android" && !t.iOS())
            {
                t.tests = append(t.tests, new distTest(name:"api",heading:"API check",fn:func(dt*distTest)error{ift.compileOnly{t.addCmd(dt,"src","go","build",filepath.Join(goroot,"src/cmd/api/run.go"))returnnil}t.addCmd(dt,"src","go","run",filepath.Join(goroot,"src/cmd/api/run.go"))returnnil},));
            }
        });

        // isRegisteredTestName reports whether a test named testName has already
        // been registered.
        private static bool isRegisteredTestName(this ref tester t, @string testName)
        {
            foreach (var (_, tt) in t.tests)
            {
                if (tt.name == testName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void registerTest1(this ref tester _t, bool seq, @string name, @string dirBanner, params object[] cmdline) => func(_t, (ref tester t, Defer defer, Panic panic, Recover _) =>
        {
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

        private static void registerTest(this ref tester t, @string name, @string dirBanner, params object[] cmdline)
        {
            t.registerTest1(false, name, dirBanner, cmdline);
        }

        private static void registerSeqTest(this ref tester t, @string name, @string dirBanner, params object[] cmdline)
        {
            t.registerTest1(true, name, dirBanner, cmdline);
        }

        private static ref exec.Cmd bgDirCmd(this ref tester t, @string dir, @string bin, params @string[] args)
        {
            var cmd = exec.Command(bin, args);
            if (filepath.IsAbs(dir))
            {
                cmd.Dir = dir;
            }
            else
            {
                cmd.Dir = filepath.Join(goroot, dir);
            }
            return cmd;
        }

        private static ref exec.Cmd dirCmd(this ref tester t, @string dir, params object[] cmdline)
        {
            var (bin, args) = flattenCmdline(cmdline);
            var cmd = t.bgDirCmd(dir, bin, args);
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            if (vflag > 1L)
            {
                errprintf("%s\n", strings.Join(cmd.Args, " "));
            }
            return cmd;
        }

        // flattenCmdline flattens a mixture of string and []string as single list
        // and then interprets it as a command line: first element is binary, then args.
        private static (@string, slice<@string>) flattenCmdline(slice<object> cmdline) => func((_, panic, __) =>
        {
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

        private static ref exec.Cmd addCmd(this ref tester t, ref distTest dt, @string dir, params object[] cmdline)
        {
            var (bin, args) = flattenCmdline(cmdline);
            work w = ref new work(dt:dt,cmd:t.bgDirCmd(dir,bin,args...),);
            t.worklist = append(t.worklist, w);
            return w.cmd;
        }

        private static bool iOS(this ref tester t)
        {
            return goos == "darwin" && (goarch == "arm" || goarch == "arm64");
        }

        private static void @out(this ref tester t, @string v)
        {
            if (t.banner == "")
            {
                return;
            }
            fmt.Println("\n" + t.banner + v);
        }

        private static bool extLink(this ref tester t)
        {
            var pair = gohostos + "-" + goarch;
            switch (pair)
            {
                case "android-arm": 

                case "darwin-arm": 

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
                case "darwin-386": 
                    // linkmode=external fails on OS X 10.6 and earlier == Darwin
                    // 10.8 and earlier.

                case "darwin-amd64": 
                    // linkmode=external fails on OS X 10.6 and earlier == Darwin
                    // 10.8 and earlier.
                    var (unameR, err) = exec.Command("uname", "-r").Output();
                    if (err != null)
                    {
                        log.Fatalf("uname -r: %v", err);
                    }
                    var (major, _) = strconv.Atoi(string(unameR[..bytes.IndexByte(unameR, '.')]));
                    return major > 10L;
                    break;
            }
            return false;
        }

        private static bool internalLink(this ref tester t)
        {
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
            if (goos == "darwin" && (goarch == "arm" || goarch == "arm64"))
            {
                return false;
            } 
            // Internally linking cgo is incomplete on some architectures.
            // https://golang.org/issue/10373
            // https://golang.org/issue/14449
            if (goarch == "arm64" || goarch == "mips64" || goarch == "mips64le" || goarch == "mips" || goarch == "mipsle")
            {
                return false;
            }
            if (isAlpineLinux())
            { 
                // Issue 18243.
                return false;
            }
            return true;
        }

        private static bool supportedBuildmode(this ref tester t, @string mode)
        {
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
                        case "darwin-386": 

                        case "darwin-amd64": 

                        case "darwin-arm": 

                        case "darwin-arm64": 

                        case "linux-amd64": 

                        case "linux-386": 

                        case "linux-ppc64le": 

                        case "linux-s390x": 

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

                        case "darwin-386": 

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
                    }
                    return false;
                    break;
                case "pie": 
                    switch (pair)
                    {
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
                    }
                    return false;
                    break;
                default: 
                    log.Fatalf("internal error: unknown buildmode %s", mode);
                    return false;
                    break;
            }
        }

        private static void registerHostTest(this ref tester _t, @string name, @string heading, @string dir, @string pkg) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
            t.tests = append(t.tests, new distTest(name:name,heading:heading,fn:func(dt*distTest)error{t.runPending(dt)timelog("start",name)defertimelog("end",name)returnt.runHostTest(dir,pkg)},));
        });

        private static error runHostTest(this ref tester _t, @string dir, @string pkg) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
            defer(os.Remove(filepath.Join(goroot, dir, "test.test")));
            var cmd = t.dirCmd(dir, t.goTest(), "-c", "-o", "test.test", pkg);
            cmd.Env = append(os.Environ(), "GOARCH=" + gohostarch, "GOOS=" + gohostos);
            {
                var err = cmd.Run();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(t.dirCmd(dir, "./test.test").Run());
        });

        private static error cgoTest(this ref tester t, ref distTest dt)
        {
            t.addCmd(dt, "misc/cgo/test", t.goTest(), "-ldflags", "-linkmode=auto");

            if (t.internalLink())
            {
                t.addCmd(dt, "misc/cgo/test", t.goTest(), "-tags=internal", "-ldflags", "-linkmode=internal");
            }
            var pair = gohostos + "-" + goarch;
            switch (pair)
            {
                case "darwin-386": 
                    // test linkmode=external, but __thread not supported, so skip testtls.

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
                    t.addCmd(dt, "misc/cgo/test", t.goTest(), "-ldflags", "-linkmode=external");
                    t.addCmd(dt, "misc/cgo/test", t.goTest(), "-ldflags", "-linkmode=external -s");
                    break;
                case "android-arm": 


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

                    t.addCmd(dt, "misc/cgo/test", t.goTest(), "-ldflags", "-linkmode=external");
                    t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-ldflags", "-linkmode=auto");
                    t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-ldflags", "-linkmode=external");

                    switch (pair)
                    {
                        case "netbsd-386": 

                        case "netbsd-amd64": 
                            break;
                        case "freebsd-arm": 
                            break;
                        default: 
                            var cmd = t.dirCmd("misc/cgo/test", compilerEnvLookup(defaultcc, goos, goarch), "-xc", "-o", "/dev/null", "-static", "-");
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
                                    }
                                }

                            }

                            if (t.supportedBuildmode("pie"))
                            {
                                t.addCmd(dt, "misc/cgo/test", t.goTest(), "-buildmode=pie");
                                t.addCmd(dt, "misc/cgo/testtls", t.goTest(), "-buildmode=pie");
                                t.addCmd(dt, "misc/cgo/nocgo", t.goTest(), "-buildmode=pie");
                            }
                            break;
                    }
                    break;
            }

            return error.As(null);
        }

        // run pending test commands, in parallel, emitting headers as appropriate.
        // When finished, emit header for nextTest, which is going to run after the
        // pending commands are done (and runPending returns).
        // A test should call runPending if it wants to make sure that it is not
        // running in parallel with earlier tests, or if it has some other reason
        // for needing the earlier tests to be done.
        private static void runPending(this ref tester t, ref distTest nextTest)
        {
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
                        }
                        timelog("end", w.dt.name);
                        w.end.Send(true);
                    }(w));
                }

                w = w__prev1;
            }

            long started = 0L;
            long ended = 0L;
            ref distTest last = default;
            while (ended < len(worklist))
            {
                while (started < len(worklist) && started - ended < maxbg)
                { 
                    //println("start", started)
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
                //println("wait", ended)
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
                log.Fatal("FAILED");
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

        private static bool cgoTestSOSupported(this ref tester t)
        {
            if (goos == "android" || t.iOS())
            { 
                // No exec facility on Android or iOS.
                return false;
            }
            if (goarch == "ppc64")
            { 
                // External linking not implemented on ppc64 (issue #8912).
                return false;
            }
            if (goarch == "mips64le" || goarch == "mips64")
            { 
                // External linking not implemented on mips64.
                return false;
            }
            return true;
        }

        private static error cgoTestSO(this ref tester _t, ref distTest _dt, @string testpath) => func(_t, _dt, (ref tester t, ref distTest dt, Defer defer, Panic _, Recover __) =>
        {
            t.runPending(dt);

            timelog("start", dt.name);
            defer(timelog("end", dt.name));

            var dir = filepath.Join(goroot, testpath); 

            // build shared object
            var (output, err) = exec.Command("go", "env", "CC").Output();
            if (err != null)
            {
                return error.As(fmt.Errorf("Error running go env CC: %v", err));
            }
            var cc = strings.TrimSuffix(string(output), "\n");
            if (cc == "")
            {
                return error.As(errors.New("CC environment variable (go env CC) cannot be empty"));
            }
            output, err = exec.Command("go", "env", "GOGCCFLAGS").Output();
            if (err != null)
            {
                return error.As(fmt.Errorf("Error running go env GOGCCFLAGS: %v", err));
            }
            var gogccflags = strings.Split(strings.TrimSuffix(string(output), "\n"), " ");

            @string ext = "so";
            var args = append(gogccflags, "-shared");
            switch (goos)
            {
                case "darwin": 
                    ext = "dylib";
                    args = append(args, "-undefined", "suppress", "-flat_namespace");
                    break;
                case "windows": 
                    ext = "dll";
                    args = append(args, "-DEXPORT_DLL");
                    break;
            }
            @string sofname = "libcgosotest." + ext;
            args = append(args, "-o", sofname, "cgoso_c.c");

            {
                var err__prev1 = err;

                var err = t.dirCmd(dir, cc, args).Run();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            defer(os.Remove(filepath.Join(dir, sofname)));

            {
                var err__prev1 = err;

                err = t.dirCmd(dir, "go", "build", "-o", "main.exe", "main.go").Run();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            defer(os.Remove(filepath.Join(dir, "main.exe")));

            var cmd = t.dirCmd(dir, "./main.exe");
            if (goos != "windows")
            {
                @string s = "LD_LIBRARY_PATH";
                if (goos == "darwin")
                {
                    s = "DYLD_LIBRARY_PATH";
                }
                cmd.Env = append(os.Environ(), s + "=."); 

                // On FreeBSD 64-bit architectures, the 32-bit linker looks for
                // different environment variables.
                if (goos == "freebsd" && gohostarch == "386")
                {
                    cmd.Env = append(cmd.Env, "LD_32_LIBRARY_PATH=.");
                }
            }
            return error.As(cmd.Run());
        });

        private static bool hasBash(this ref tester t)
        {
            switch (gohostos)
            {
                case "windows": 

                case "plan9": 
                    return false;
                    break;
            }
            return true;
        }

        private static bool hasSwig(this ref tester t)
        {
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

        private static bool raceDetectorSupported(this ref tester t)
        {
            switch (gohostos)
            {
                case "linux": 
                    // The race detector doesn't work on Alpine Linux:
                    // golang.org/issue/14481

                case "darwin": 
                    // The race detector doesn't work on Alpine Linux:
                    // golang.org/issue/14481

                case "freebsd": 
                    // The race detector doesn't work on Alpine Linux:
                    // golang.org/issue/14481

                case "windows": 
                    // The race detector doesn't work on Alpine Linux:
                    // golang.org/issue/14481
                    return t.cgoEnabled && goarch == "amd64" && gohostos == goos && !isAlpineLinux();
                    break;
            }
            return false;
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

        private static @string runFlag(this ref tester t, @string rx)
        {
            if (t.compileOnly)
            {
                return "-run=^$";
            }
            return "-run=" + rx;
        }

        private static error raceTest(this ref tester t, ref distTest dt)
        {
            t.addCmd(dt, "src", t.goTest(), "-race", "-i", "runtime/race", "flag", "os", "os/exec");
            t.addCmd(dt, "src", t.goTest(), "-race", t.runFlag("Output"), "runtime/race");
            t.addCmd(dt, "src", t.goTest(), "-race", t.runFlag("TestParse|TestEcho|TestStdinCloseRace|TestClosedPipeRace|TestTypeRace"), "flag", "os", "os/exec", "encoding/gob"); 
            // We don't want the following line, because it
            // slows down all.bash (by 10 seconds on my laptop).
            // The race builder should catch any error here, but doesn't.
            // TODO(iant): Figure out how to catch this.
            // t.addCmd(dt, "src", t.goTest(),  "-race", "-run=TestParallelTest", "cmd/go")
            if (t.cgoEnabled)
            {
                var cmd = t.addCmd(dt, "misc/cgo/test", t.goTest(), "-race");
                cmd.Env = append(os.Environ(), "GOTRACEBACK=2");
            }
            if (t.extLink())
            { 
                // Test with external linking; see issue 9133.
                t.addCmd(dt, "src", t.goTest(), "-race", "-ldflags=-linkmode=external", t.runFlag("TestParse|TestEcho|TestStdinCloseRace"), "flag", "os/exec");
            }
            return error.As(null);
        }

        private static var runtest = default;

        private static error testDirTest(this ref tester t, ref distTest dt, long shard, long shards)
        {
            runtest.Do(() =>
            {
                const @string exe = "runtest.exe"; // named exe for Windows, but harmless elsewhere
 // named exe for Windows, but harmless elsewhere
                var cmd = t.dirCmd("test", "go", "build", "-o", exe, "run.go");
                cmd.Env = append(os.Environ(), "GOOS=" + gohostos, "GOARCH=" + gohostarch);
                runtest.exe = filepath.Join(cmd.Dir, exe);
                {
                    var err = cmd.Run();

                    if (err != null)
                    {
                        runtest.err = err;
                        return;
                    }

                }
                xatexit(() =>
                {
                    os.Remove(runtest.exe);
                });
            });
            if (runtest.err != null)
            {
                return error.As(runtest.err);
            }
            if (t.compileOnly)
            {
                return error.As(null);
            }
            t.addCmd(dt, "test", runtest.exe, fmt.Sprintf("--shard=%d", shard), fmt.Sprintf("--shards=%d", shards));
            return error.As(null);
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
        private static bool packageHasBenchmarks(this ref tester _t, @string pkg) => func(_t, (ref tester t, Defer defer, Panic _, Recover __) =>
        {
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
    }
}
