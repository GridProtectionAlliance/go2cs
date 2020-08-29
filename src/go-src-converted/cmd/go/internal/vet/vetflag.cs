// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package vet -- go2cs converted at 2020 August 29 10:01:38 UTC
// import "cmd/go/internal/vet" ==> using vet = go.cmd.go.@internal.vet_package
// Original source: C:\Go\src\cmd\go\internal\vet\vetflag.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cmdflag = go.cmd.go.@internal.cmdflag_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class vet_package
    {
        private static readonly @string cmd = "vet";

        // vetFlagDefn is the set of flags we process.


        // vetFlagDefn is the set of flags we process.
        private static ref cmdflag.Defn vetFlagDefn = new slice<ref cmdflag.Defn>(new ref cmdflag.Defn[] { {Name:"all",BoolVar:new(bool)}, {Name:"asmdecl",BoolVar:new(bool)}, {Name:"assign",BoolVar:new(bool)}, {Name:"atomic",BoolVar:new(bool)}, {Name:"bool",BoolVar:new(bool)}, {Name:"buildtags",BoolVar:new(bool)}, {Name:"cgocall",BoolVar:new(bool)}, {Name:"composites",BoolVar:new(bool)}, {Name:"copylocks",BoolVar:new(bool)}, {Name:"httpresponse",BoolVar:new(bool)}, {Name:"lostcancel",BoolVar:new(bool)}, {Name:"methods",BoolVar:new(bool)}, {Name:"nilfunc",BoolVar:new(bool)}, {Name:"printf",BoolVar:new(bool)}, {Name:"printfuncs"}, {Name:"rangeloops",BoolVar:new(bool)}, {Name:"shadow",BoolVar:new(bool)}, {Name:"shadowstrict",BoolVar:new(bool)}, {Name:"shift",BoolVar:new(bool)}, {Name:"source",BoolVar:new(bool)}, {Name:"structtags",BoolVar:new(bool)}, {Name:"tests",BoolVar:new(bool)}, {Name:"unreachable",BoolVar:new(bool)}, {Name:"unsafeptr",BoolVar:new(bool)}, {Name:"unusedfuncs"}, {Name:"unusedresult",BoolVar:new(bool)}, {Name:"unusedstringmethods"} });

        private static @string vetTool = default;

        // add build flags to vetFlagDefn.
        private static void init()
        {
            base.Command cmd = default;
            work.AddBuildFlags(ref cmd);
            cmd.Flag.StringVar(ref vetTool, "vettool", "", "path to vet tool binary"); // for cmd/vet tests; undocumented for now
            cmd.Flag.VisitAll(f =>
            {
                vetFlagDefn = append(vetFlagDefn, ref new cmdflag.Defn(Name:f.Name,Value:f.Value,));
            });
        }

        // vetFlags processes the command line, splitting it at the first non-flag
        // into the list of flags and list of packages.
        private static (slice<@string>, slice<@string>) vetFlags(slice<@string> args)
        {
            for (long i = 0L; i < len(args); i++)
            {
                if (!strings.HasPrefix(args[i], "-"))
                {
                    return (args[..i], args[i..]);
                }
                var (f, value, extraWord) = cmdflag.Parse(cmd, vetFlagDefn, args, i);
                if (f == null)
                {
                    fmt.Fprintf(os.Stderr, "vet: flag %q not defined\n", args[i]);
                    fmt.Fprintf(os.Stderr, "Run \"go help vet\" for more information\n");
                    os.Exit(2L);
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
                    switch (f.Name)
                    { 
                    // Flags known to the build but not to vet, so must be dropped.
                        case "x": 

                        case "n": 

                        case "vettool": 
                            if (extraWord)
                            {
                                args = append(args[..i], args[i + 2L..]);
                                extraWord = false;
                            }
                            else
                            {
                                args = append(args[..i], args[i + 1L..]);
                            }
                            i--;
                            break;
                    }
                }
                if (extraWord)
                {
                    i++;
                }
            }

            return (args, null);
        }
    }
}}}}
