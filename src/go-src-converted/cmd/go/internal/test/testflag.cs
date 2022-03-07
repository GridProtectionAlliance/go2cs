// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package test -- go2cs converted at 2022 March 06 23:17:55 UTC
// import "cmd/go/internal/test" ==> using test = go.cmd.go.@internal.test_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\test\testflag.go
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using cmdflag = go.cmd.go.@internal.cmdflag_package;
using work = go.cmd.go.@internal.work_package;
using System;


namespace go.cmd.go.@internal;

public static partial class test_package {

    //go:generate go run ./genflags.go

    // The flag handling part of go test is large and distracting.
    // We can't use (*flag.FlagSet).Parse because some of the flags from
    // our command line are for us, and some are for the test binary, and
    // some are for both.
private static void init() {
    work.AddBuildFlags(CmdTest, work.OmitVFlag);

    var cf = CmdTest.Flag;
    cf.BoolVar(_addr_testC, "c", false, "");
    cf.BoolVar(_addr_cfg.BuildI, "i", false, "");
    cf.StringVar(_addr_testO, "o", "", "");

    cf.BoolVar(_addr_testCover, "cover", false, "");
    cf.Var(new coverFlag((*coverModeFlag)(&testCoverMode)), "covermode", "");
    cf.Var(new coverFlag(commaListFlag{&testCoverPaths}), "coverpkg", "");

    cf.Var((@base.StringsFlag.val)(_addr_work.ExecCmd), "exec", "");
    cf.BoolVar(_addr_testJSON, "json", false, "");
    cf.Var(_addr_testVet, "vet", ""); 

    // Register flags to be forwarded to the test binary. We retain variables for
    // some of them so that cmd/go knows what to do with the test output, or knows
    // to build the test in a way that supports the use of the flag.

    cf.StringVar(_addr_testBench, "bench", "", "");
    cf.Bool("benchmem", false, "");
    cf.String("benchtime", "", "");
    cf.StringVar(_addr_testBlockProfile, "blockprofile", "", "");
    cf.String("blockprofilerate", "", "");
    cf.Int("count", 0, "");
    cf.Var(new coverFlag(stringFlag{&testCoverProfile}), "coverprofile", "");
    cf.String("cpu", "", "");
    cf.StringVar(_addr_testCPUProfile, "cpuprofile", "", "");
    cf.Bool("failfast", false, "");
    cf.StringVar(_addr_testList, "list", "", "");
    cf.StringVar(_addr_testMemProfile, "memprofile", "", "");
    cf.String("memprofilerate", "", "");
    cf.StringVar(_addr_testMutexProfile, "mutexprofile", "", "");
    cf.String("mutexprofilefraction", "", "");
    cf.Var(_addr_testOutputDir, "outputdir", "");
    cf.Int("parallel", 0, "");
    cf.String("run", "", "");
    cf.Bool("short", false, "");
    cf.DurationVar(_addr_testTimeout, "timeout", 10 * time.Minute, "");
    cf.StringVar(_addr_testTrace, "trace", "", "");
    cf.BoolVar(_addr_testV, "v", false, "");
    cf.Var(_addr_testShuffle, "shuffle", "");

    foreach (var (name) in passFlagToTest) {
        cf.Var(cf.Lookup(name).Value, "test." + name, "");
    }
}

// A coverFlag is a flag.Value that also implies -cover.
private partial struct coverFlag {
    public flag.Value v;
}

private static @string String(this coverFlag f) {
    return f.v.String();
}

private static error Set(this coverFlag f, @string value) {
    {
        var err = f.v.Set(value);

        if (err != null) {
            return error.As(err)!;
        }
    }

    testCover = true;
    return error.As(null!)!;

}

private partial struct coverModeFlag { // : @string
}

private static @string String(this ptr<coverModeFlag> _addr_f) {
    ref coverModeFlag f = ref _addr_f.val;

    return string(f.val);
}
private static error Set(this ptr<coverModeFlag> _addr_f, @string value) {
    ref coverModeFlag f = ref _addr_f.val;

    switch (value) {
        case "": 

        case "set": 

        case "count": 

        case "atomic": 
            f.val = coverModeFlag(value);
            return error.As(null!)!;
            break;
        default: 
            return error.As(errors.New("valid modes are \"set\", \"count\", or \"atomic\""))!;
            break;
    }

}

// A commaListFlag is a flag.Value representing a comma-separated list.
private partial struct commaListFlag {
    public ptr<slice<@string>> vals;
}

private static @string String(this commaListFlag f) {
    return strings.Join(f.vals.val, ",");
}

private static error Set(this commaListFlag f, @string value) {
    if (value == "") {
        f.vals.val = null;
    }
    else
 {
        f.vals.val = strings.Split(value, ",");
    }
    return error.As(null!)!;

}

// A stringFlag is a flag.Value representing a single string.
private partial struct stringFlag {
    public ptr<@string> val;
}

private static @string String(this stringFlag f) {
    return f.val;
}
private static error Set(this stringFlag f, @string value) {
    f.val = value;
    return error.As(null!)!;
}

// outputdirFlag implements the -outputdir flag.
// It interprets an empty value as the working directory of the 'go' command.
private partial struct outputdirFlag {
    public @string abs;
}

private static @string String(this ptr<outputdirFlag> _addr_f) {
    ref outputdirFlag f = ref _addr_f.val;

    return f.abs;
}
private static error Set(this ptr<outputdirFlag> _addr_f, @string value) {
    error err = default!;
    ref outputdirFlag f = ref _addr_f.val;

    if (value == "") {
        f.abs = "";
    }
    else
 {
        f.abs, err = filepath.Abs(value);
    }
    return error.As(err)!;

}
private static @string getAbs(this ptr<outputdirFlag> _addr_f) {
    ref outputdirFlag f = ref _addr_f.val;

    if (f.abs == "") {
        return @base.Cwd();
    }
    return f.abs;

}

// vetFlag implements the special parsing logic for the -vet flag:
// a comma-separated list, with a distinguished value "off" and
// a boolean tracking whether it was set explicitly.
private partial struct vetFlag {
    public bool @explicit;
    public bool off;
    public slice<@string> flags; // passed to vet when invoked automatically during 'go test'
}

private static @string String(this ptr<vetFlag> _addr_f) {
    ref vetFlag f = ref _addr_f.val;

    if (f.off) {
        return "off";
    }
    strings.Builder buf = default;
    foreach (var (i, f) in f.flags) {
        if (i > 0) {
            buf.WriteByte(',');
        }
        buf.WriteString(f);

    }    return buf.String();

}

private static error Set(this ptr<vetFlag> _addr_f, @string value) {
    ref vetFlag f = ref _addr_f.val;

    if (value == "") {
        f.val = new vetFlag(flags:defaultVetFlags);
        return error.As(null!)!;
    }
    if (value == "off") {
        f.val = new vetFlag(explicit:true,off:true,);
        return error.As(null!)!;
    }
    if (strings.Contains(value, "=")) {
        return error.As(fmt.Errorf("-vet argument cannot contain equal signs"))!;
    }
    if (strings.Contains(value, " ")) {
        return error.As(fmt.Errorf("-vet argument is comma-separated list, cannot contain spaces"))!;
    }
    f.val = new vetFlag(explicit:true);
    foreach (var (_, arg) in strings.Split(value, ",")) {
        if (arg == "") {
            return error.As(fmt.Errorf("-vet argument contains empty list element"))!;
        }
        f.flags = append(f.flags, "-" + arg);

    }    return error.As(null!)!;

}

private partial struct shuffleFlag {
    public bool on;
    public ptr<long> seed;
}

private static @string String(this ptr<shuffleFlag> _addr_f) {
    ref shuffleFlag f = ref _addr_f.val;

    if (!f.on) {
        return "off";
    }
    if (f.seed == null) {
        return "on";
    }
    return fmt.Sprintf("%d", f.seed.val);

}

private static error Set(this ptr<shuffleFlag> _addr_f, @string value) {
    ref shuffleFlag f = ref _addr_f.val;

    if (value == "off") {
        f.val = new shuffleFlag(on:false);
        return error.As(null!)!;
    }
    if (value == "on") {
        f.val = new shuffleFlag(on:true);
        return error.As(null!)!;
    }
    var (seed, err) = strconv.ParseInt(value, 10, 64);
    if (err != null) {
        return error.As(fmt.Errorf("-shuffle argument must be \"on\", \"off\", or an int64: %v", err))!;
    }
    f.val = new shuffleFlag(on:true,seed:&seed);
    return error.As(null!)!;

}

// testFlags processes the command line, grabbing -x and -c, rewriting known flags
// to have "test" before them, and reading the command line for the test binary.
// Unfortunately for us, we need to do our own flag processing because go test
// grabs some flags but otherwise its command line is just a holding place for
// pkg.test's arguments.
// We allow known flags both before and after the package name list,
// to allow both
//    go test fmt -custom-flag-for-fmt-test
//    go test -x math
private static (slice<@string>, slice<@string>) testFlags(slice<@string> args) {
    slice<@string> packageNames = default;
    slice<@string> passToTest = default;

    @base.SetFromGOFLAGS(_addr_CmdTest.Flag);
    map addFromGOFLAGS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    CmdTest.Flag.Visit(f => {
        {
            var short__prev1 = short;

            var @short = strings.TrimPrefix(f.Name, "test.");

            if (passFlagToTest[short]) {
                addFromGOFLAGS[f.Name] = true;
            }

            short = short__prev1;

        }

    }); 

    // firstUnknownFlag helps us report an error when flags not known to 'go
    // test' are used along with -i or -c.
    @string firstUnknownFlag = "";

    var explicitArgs = make_slice<@string>(0, len(args));
    var inPkgList = false;
    var afterFlagWithoutValue = false;
    while (len(args) > 0) {
        var (f, remainingArgs, err) = cmdflag.ParseOne(_addr_CmdTest.Flag, args);

        var wasAfterFlagWithoutValue = afterFlagWithoutValue;
        afterFlagWithoutValue = false; // provisionally

        if (errors.Is(err, flag.ErrHelp)) {
            exitWithUsage();
        }
        if (errors.Is(err, cmdflag.ErrFlagTerminator)) { 
            // 'go list' allows package arguments to be named either before or after
            // the terminator, but 'go test' has historically allowed them only
            // before. Preserve that behavior and treat all remaining arguments —
            // including the terminator itself! — as arguments to the test.
            explicitArgs = append(explicitArgs, args);
            break;

        }
        {
            ref cmdflag.NonFlagError nf = ref heap((new cmdflag.NonFlagError()), out ptr<cmdflag.NonFlagError> _addr_nf);

            if (errors.As(err, _addr_nf)) {
                if (!inPkgList && packageNames != null) { 
                    // We already saw the package list previously, and this argument is not
                    // a flag, so it — and everything after it — must be either a value for
                    // a preceding flag or a literal argument to the test binary.
                    if (wasAfterFlagWithoutValue) { 
                        // This argument could syntactically be a flag value, so
                        // optimistically assume that it is and keep looking for go command
                        // flags after it.
                        //
                        // (If we're wrong, we'll at least be consistent with historical
                        // behavior; see https://golang.org/issue/40763.)
                        explicitArgs = append(explicitArgs, nf.RawArg);
                        args = remainingArgs;
                        continue;

                    }
                    else
 { 
                        // This argument syntactically cannot be a flag value, so it must be a
                        // positional argument, and so must everything after it.
                        explicitArgs = append(explicitArgs, args);
                        break;

                    }

                }

                inPkgList = true;
                packageNames = append(packageNames, nf.RawArg);
                args = remainingArgs; // Consume the package name.
                continue;

            }

        }


        if (inPkgList) { 
            // This argument is syntactically a flag, so if we were in the package
            // list we're not anymore.
            inPkgList = false;

        }
        {
            ref cmdflag.FlagNotDefinedError nd = ref heap((new cmdflag.FlagNotDefinedError()), out ptr<cmdflag.FlagNotDefinedError> _addr_nd);

            if (errors.As(err, _addr_nd)) { 
                // This is a flag we do not know. We must assume that any args we see
                // after this might be flag arguments, not package names, so make
                // packageNames non-nil to indicate that the package list is complete.
                //
                // (Actually, we only strictly need to assume that if the flag is not of
                // the form -x=value, but making this more precise would be a breaking
                // change in the command line API.)
                if (packageNames == null) {
                    packageNames = new slice<@string>(new @string[] {  });
                }

                if (nd.RawArg == "-args" || nd.RawArg == "--args") { 
                    // -args or --args signals that everything that follows
                    // should be passed to the test.
                    explicitArgs = append(explicitArgs, remainingArgs);
                    break;

                }

                if (firstUnknownFlag == "") {
                    firstUnknownFlag = nd.RawArg;
                }

                explicitArgs = append(explicitArgs, nd.RawArg);
                args = remainingArgs;
                if (!nd.HasValue) {
                    afterFlagWithoutValue = true;
                }

                continue;

            }

        }


        if (err != null) {
            fmt.Fprintln(os.Stderr, err);
            exitWithUsage();
        }
        {
            var short__prev1 = short;

            @short = strings.TrimPrefix(f.Name, "test.");

            if (passFlagToTest[short]) {
                explicitArgs = append(explicitArgs, fmt.Sprintf("-test.%s=%v", short, f.Value)); 

                // This flag has been overridden explicitly, so don't forward its implicit
                // value from GOFLAGS.
                delete(addFromGOFLAGS, short);
                delete(addFromGOFLAGS, "test." + short);

            }

            short = short__prev1;

        }


        args = remainingArgs;

    }
    if (firstUnknownFlag != "" && (testC || cfg.BuildI)) {
        @string buildFlag = "-c";
        if (!testC) {
            buildFlag = "-i";
        }
        fmt.Fprintf(os.Stderr, "go test: unknown flag %s cannot be used with %s\n", firstUnknownFlag, buildFlag);
        exitWithUsage();

    }
    slice<@string> injectedFlags = default;
    if (testJSON) { 
        // If converting to JSON, we need the full output in order to pipe it to
        // test2json.
        injectedFlags = append(injectedFlags, "-test.v=true");
        delete(addFromGOFLAGS, "v");
        delete(addFromGOFLAGS, "test.v");

    }
    bool timeoutSet = default;    bool outputDirSet = default;

    CmdTest.Flag.Visit(f => {
        @short = strings.TrimPrefix(f.Name, "test.");
        if (addFromGOFLAGS[f.Name]) {
            injectedFlags = append(injectedFlags, fmt.Sprintf("-test.%s=%v", short, f.Value));
        }
        switch (short) {
            case "timeout": 
                timeoutSet = true;
                break;
            case "outputdir": 
                outputDirSet = true;
                break;
        }

    }); 

    // 'go test' has a default timeout, but the test binary itself does not.
    // If the timeout wasn't set (and forwarded) explicitly, add the default
    // timeout to the command line.
    if (testTimeout > 0 && !timeoutSet) {
        injectedFlags = append(injectedFlags, fmt.Sprintf("-test.timeout=%v", testTimeout));
    }
    if (testProfile() != "" && !outputDirSet) {
        injectedFlags = append(injectedFlags, "-test.outputdir=" + testOutputDir.getAbs());
    }
helpLoop: 

    // Ensure that -race and -covermode are compatible.
    foreach (var (_, arg) in explicitArgs) {
        switch (arg) {
            case "--": 
                _breakhelpLoop = true;
                break;
                break;
            case "-h": 

            case "-help": 

            case "--help": 
                testHelp = true;
                _breakhelpLoop = true;
                break;
                break;
        }

    }    if (testCoverMode == "") {
        testCoverMode = "set";
        if (cfg.BuildRace) { 
            // Default coverage mode is atomic when -race is set.
            testCoverMode = "atomic";

        }
    }
    if (cfg.BuildRace && testCoverMode != "atomic") {
        @base.Fatalf("-covermode must be \"atomic\", not %q, when -race is enabled", testCoverMode);
    }
    return (packageNames, append(injectedFlags, explicitArgs));

}

private static void exitWithUsage() {
    fmt.Fprintf(os.Stderr, "usage: %s\n", CmdTest.UsageLine);
    fmt.Fprintf(os.Stderr, "Run 'go help %s' and 'go help %s' for details.\n", CmdTest.LongName(), HelpTestflag.LongName());

    @base.SetExitStatus(2);
    @base.Exit();
}

} // end test_package
