// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package test -- go2cs converted at 2020 August 29 10:01:49 UTC
// import "cmd/go/internal/test" ==> using test = go.cmd.go.@internal.test_package
// Original source: C:\Go\src\cmd\go\internal\test\testflag.go
using flag = go.flag_package;
using os = go.os_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using cmdflag = go.cmd.go.@internal.cmdflag_package;
using str = go.cmd.go.@internal.str_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class test_package
    {
        private static readonly @string cmd = "test";

        // The flag handling part of go test is large and distracting.
        // We can't use the flag package because some of the flags from
        // our command line are for us, and some are for 6.out, and
        // some are for both.

        // testFlagDefn is the set of flags we process.


        // The flag handling part of go test is large and distracting.
        // We can't use the flag package because some of the flags from
        // our command line are for us, and some are for 6.out, and
        // some are for both.

        // testFlagDefn is the set of flags we process.
        private static ref cmdflag.Defn testFlagDefn = new slice<ref cmdflag.Defn>(new ref cmdflag.Defn[] { {Name:"c",BoolVar:&testC}, {Name:"i",BoolVar:&cfg.BuildI}, {Name:"o"}, {Name:"cover",BoolVar:&testCover}, {Name:"covermode"}, {Name:"coverpkg"}, {Name:"exec"}, {Name:"json",BoolVar:&testJSON}, {Name:"vet"}, {Name:"bench",PassToTest:true}, {Name:"benchmem",BoolVar:new(bool),PassToTest:true}, {Name:"benchtime",PassToTest:true}, {Name:"blockprofile",PassToTest:true}, {Name:"blockprofilerate",PassToTest:true}, {Name:"count",PassToTest:true}, {Name:"coverprofile",PassToTest:true}, {Name:"cpu",PassToTest:true}, {Name:"cpuprofile",PassToTest:true}, {Name:"failfast",BoolVar:new(bool),PassToTest:true}, {Name:"list",PassToTest:true}, {Name:"memprofile",PassToTest:true}, {Name:"memprofilerate",PassToTest:true}, {Name:"mutexprofile",PassToTest:true}, {Name:"mutexprofilefraction",PassToTest:true}, {Name:"outputdir",PassToTest:true}, {Name:"parallel",PassToTest:true}, {Name:"run",PassToTest:true}, {Name:"short",BoolVar:new(bool),PassToTest:true}, {Name:"timeout",PassToTest:true}, {Name:"trace",PassToTest:true}, {Name:"v",BoolVar:&testV,PassToTest:true} });

        // add build flags to testFlagDefn
        private static void init()
        {
            base.Command cmd = default;
            work.AddBuildFlags(ref cmd);
            cmd.Flag.VisitAll(f =>
            {
                if (f.Name == "v")
                { 
                    // test overrides the build -v flag
                    return;
                }
                testFlagDefn = append(testFlagDefn, ref new cmdflag.Defn(Name:f.Name,Value:f.Value,));
            });
        }

        // testFlags processes the command line, grabbing -x and -c, rewriting known flags
        // to have "test" before them, and reading the command line for the 6.out.
        // Unfortunately for us, we need to do our own flag processing because go test
        // grabs some flags but otherwise its command line is just a holding place for
        // pkg.test's arguments.
        // We allow known flags both before and after the package name list,
        // to allow both
        //    go test fmt -custom-flag-for-fmt-test
        //    go test -x math
        private static (slice<@string>, slice<@string>) testFlags(slice<@string> args)
        {
            var inPkg = false;
            slice<@string> explicitArgs = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(args); i++)
                {
                    if (!strings.HasPrefix(args[i], "-"))
                    {
                        if (!inPkg && packageNames == null)
                        { 
                            // First package name we've seen.
                            inPkg = true;
                        }
                        if (inPkg)
                        {
                            packageNames = append(packageNames, args[i]);
                            continue;
                        }
                    }
                    if (inPkg)
                    { 
                        // Found an argument beginning with "-"; end of package list.
                        inPkg = false;
                    }
                    var (f, value, extraWord) = cmdflag.Parse(cmd, testFlagDefn, args, i);
                    if (f == null)
                    { 
                        // This is a flag we do not know; we must assume
                        // that any args we see after this might be flag
                        // arguments, not package names.
                        inPkg = false;
                        if (packageNames == null)
                        { 
                            // make non-nil: we have seen the empty package list
                            packageNames = new slice<@string>(new @string[] {  });
                        }
                        if (args[i] == "-args" || args[i] == "--args")
                        { 
                            // -args or --args signals that everything that follows
                            // should be passed to the test.
                            explicitArgs = args[i + 1L..];
                            break;
                        }
                        passToTest = append(passToTest, args[i]);
                        continue;
                    }
                    if (f.Value != null)
                    {
                        {
                            var err = f.Value.Set(value);

                            if (err != null)
                            {
                                @base.Fatalf("invalid flag argument for -%s: %v", f.Name, err);
                            }

                        }
                    }
                    else
                    { 
                        // Test-only flags.
                        // Arguably should be handled by f.Value, but aren't.
                        switch (f.Name)
                        { 
                        // bool flags.
                            case "c": 

                            case "i": 

                            case "v": 

                            case "cover": 

                            case "json": 
                                cmdflag.SetBool(cmd, f.BoolVar, value);
                                if (f.Name == "json" && testJSON)
                                {
                                    passToTest = append(passToTest, "-test.v=true");
                                }
                                break;
                            case "o": 
                                testO = value;
                                testNeedBinary = true;
                                break;
                            case "exec": 
                                var (xcmd, err) = str.SplitQuotedFields(value);
                                if (err != null)
                                {
                                    @base.Fatalf("invalid flag argument for -%s: %v", f.Name, err);
                                }
                                work.ExecCmd = xcmd;
                                break;
                            case "bench": 
                                // record that we saw the flag; don't care about the value
                                testBench = true;
                                break;
                            case "list": 
                                testList = true;
                                break;
                            case "timeout": 
                                testTimeout = value;
                                break;
                            case "blockprofile": 

                            case "cpuprofile": 

                            case "memprofile": 

                            case "mutexprofile": 
                                testProfile = "-" + f.Name;
                                testNeedBinary = true;
                                break;
                            case "trace": 
                                testProfile = "-trace";
                                break;
                            case "coverpkg": 
                                testCover = true;
                                if (value == "")
                                {
                                    testCoverPaths = null;
                                }
                                else
                                {
                                    testCoverPaths = strings.Split(value, ",");
                                }
                                break;
                            case "coverprofile": 
                                testCover = true;
                                testCoverProfile = value;
                                break;
                            case "covermode": 
                                switch (value)
                                {
                                    case "set": 

                                    case "count": 

                                    case "atomic": 
                                        testCoverMode = value;
                                        break;
                                    default: 
                                        @base.Fatalf("invalid flag argument for -covermode: %q", value);
                                        break;
                                }
                                testCover = true;
                                break;
                            case "outputdir": 
                                testOutputDir = value;
                                break;
                            case "vet": 
                                testVetList = value;
                                break;
                        }
                    }
                    if (extraWord)
                    {
                        i++;
                    }
                    if (f.PassToTest)
                    {
                        passToTest = append(passToTest, "-test." + f.Name + "=" + value);
                    }
                }


                i = i__prev1;
            }

            if (testCoverMode == "")
            {
                testCoverMode = "set";
                if (cfg.BuildRace)
                { 
                    // Default coverage mode is atomic when -race is set.
                    testCoverMode = "atomic";
                }
            }
            if (testVetList != "" && testVetList != "off")
            {
                if (strings.Contains(testVetList, "="))
                {
                    @base.Fatalf("-vet argument cannot contain equal signs");
                }
                if (strings.Contains(testVetList, " "))
                {
                    @base.Fatalf("-vet argument is comma-separated list, cannot contain spaces");
                }
                var list = strings.Split(testVetList, ",");
                {
                    long i__prev1 = i;

                    foreach (var (__i, __arg) in list)
                    {
                        i = __i;
                        arg = __arg;
                        list[i] = "-" + arg;
                    }

                    i = i__prev1;
                }

                testVetFlags = list;
            }
            if (cfg.BuildRace && testCoverMode != "atomic")
            {
                @base.Fatalf("-covermode must be \"atomic\", not %q, when -race is enabled", testCoverMode);
            } 

            // Tell the test what directory we're running in, so it can write the profiles there.
            if (testProfile != "" && testOutputDir == "")
            {
                var (dir, err) = os.Getwd();
                if (err != null)
                {
                    @base.Fatalf("error from os.Getwd: %s", err);
                }
                passToTest = append(passToTest, "-test.outputdir", dir);
            }
            passToTest = append(passToTest, explicitArgs);
            return;
        }
    }
}}}}
