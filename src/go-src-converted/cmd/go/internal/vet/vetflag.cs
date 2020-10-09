// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package vet -- go2cs converted at 2020 October 09 05:46:26 UTC
// import "cmd/go/internal/vet" ==> using vet = go.cmd.go.@internal.vet_package
// Original source: C:\Go\src\cmd\go\internal\vet\vetflag.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
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
        // go vet flag processing
        //
        // We query the flags of the tool specified by -vettool and accept any
        // of those flags plus any flag valid for 'go build'. The tool must
        // support -flags, which prints a description of its flags in JSON to
        // stdout.

        // vetTool specifies the vet command to run.
        // Any tool that supports the (still unpublished) vet
        // command-line protocol may be supplied; see
        // golang.org/x/tools/go/analysis/unitchecker for one
        // implementation. It is also used by tests.
        //
        // The default behavior (vetTool=="") runs 'go tool vet'.
        //
        private static @string vetTool = default; // -vettool

        private static void init()
        {
            work.AddBuildFlags(CmdVet, work.DefaultBuildFlags);
            CmdVet.Flag.StringVar(_addr_vetTool, "vettool", "", "");
        }

        private static void parseVettoolFlag(slice<@string> args)
        { 
            // Extract -vettool by ad hoc flag processing:
            // its value is needed even before we can declare
            // the flags available during main flag processing.
            foreach (var (i, arg) in args)
            {
                if (arg == "-vettool" || arg == "--vettool")
                {
                    if (i + 1L >= len(args))
                    {
                        log.Fatalf("%s requires a filename", arg);
                    }

                    vetTool = args[i + 1L];
                    return ;

                }
                else if (strings.HasPrefix(arg, "-vettool=") || strings.HasPrefix(arg, "--vettool="))
                {
                    vetTool = arg[strings.IndexByte(arg, '=') + 1L..];
                    return ;
                }

            }

        }

        // vetFlags processes the command line, splitting it at the first non-flag
        // into the list of flags and list of packages.
        private static (slice<@string>, slice<@string>) vetFlags(slice<@string> args)
        {
            slice<@string> passToVet = default;
            slice<@string> packageNames = default;

            parseVettoolFlag(args); 

            // Query the vet command for its flags.
            @string tool = default;
            if (vetTool == "")
            {
                tool = @base.Tool("vet");
            }
            else
            {
                error err = default!;
                tool, err = filepath.Abs(vetTool);
                if (err != null)
                {
                    log.Fatal(err);
                }

            }

            ptr<object> @out = @new<bytes.Buffer>();
            var vetcmd = exec.Command(tool, "-flags");
            vetcmd.Stdout = out;
            {
                error err__prev1 = err;

                err = vetcmd.Run();

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go vet: can't execute %s -flags: %v\n", tool, err);
                    @base.SetExitStatus(2L);
                    @base.Exit();
                }

                err = err__prev1;

            }

            ref slice<object> analysisFlags = ref heap(out ptr<slice<object>> _addr_analysisFlags);
            {
                error err__prev1 = err;

                err = json.Unmarshal(@out.Bytes(), _addr_analysisFlags);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go vet: can't unmarshal JSON from %s -flags: %v", tool, err);
                    @base.SetExitStatus(2L);
                    @base.Exit();
                } 

                // Add vet's flags to CmdVet.Flag.
                //
                // Some flags, in particular -tags and -v, are known to vet but
                // also defined as build flags. This works fine, so we omit duplicates here.
                // However some, like -x, are known to the build but not to vet.

                err = err__prev1;

            } 

            // Add vet's flags to CmdVet.Flag.
            //
            // Some flags, in particular -tags and -v, are known to vet but
            // also defined as build flags. This works fine, so we omit duplicates here.
            // However some, like -x, are known to the build but not to vet.
            var isVetFlag = make_map<@string, bool>(len(analysisFlags));
            var cf = CmdVet.Flag;
            {
                var f__prev1 = f;

                foreach (var (_, __f) in analysisFlags)
                {
                    f = __f;
                    isVetFlag[f.Name] = true;
                    if (cf.Lookup(f.Name) == null)
                    {
                        if (f.Bool)
                        {
                            cf.Bool(f.Name, false, "");
                        }
                        else
                        {
                            cf.String(f.Name, "", "");
                        }

                    }

                } 

                // Record the set of vet tool flags set by GOFLAGS. We want to pass them to
                // the vet tool, but only if they aren't overridden by an explicit argument.

                f = f__prev1;
            }

            @base.SetFromGOFLAGS(_addr_CmdVet.Flag);
            map addFromGOFLAGS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            CmdVet.Flag.Visit(f =>
            {
                if (isVetFlag[f.Name])
                {
                    addFromGOFLAGS[f.Name] = true;
                }

            });

            var explicitFlags = make_slice<@string>(0L, len(args));
            while (len(args) > 0L)
            {
                var (f, remainingArgs, err) = cmdflag.ParseOne(_addr_CmdVet.Flag, args);

                if (errors.Is(err, flag.ErrHelp))
                {
                    exitWithUsage();
                }

                if (errors.Is(err, cmdflag.ErrFlagTerminator))
                { 
                    // All remaining args must be package names, but the flag terminator is
                    // not included.
                    packageNames = remainingArgs;
                    break;

                }

                {
                    ref cmdflag.NonFlagError nf = ref heap((new cmdflag.NonFlagError()), out ptr<cmdflag.NonFlagError> _addr_nf);

                    if (errors.As(err, _addr_nf))
                    { 
                        // Everything from here on out — including the argument we just consumed —
                        // must be a package name.
                        packageNames = args;
                        break;

                    }

                }


                if (err != null)
                {
                    fmt.Fprintln(os.Stderr, err);
                    exitWithUsage();
                }

                if (isVetFlag[f.Name])
                { 
                    // Forward the raw arguments rather than cleaned equivalents, just in
                    // case the vet tool parses them idiosyncratically.
                    explicitFlags = append(explicitFlags, args[..len(args) - len(remainingArgs)]); 

                    // This flag has been overridden explicitly, so don't forward its implicit
                    // value from GOFLAGS.
                    delete(addFromGOFLAGS, f.Name);

                }

                args = remainingArgs;

            } 

            // Prepend arguments from GOFLAGS before other arguments.
 

            // Prepend arguments from GOFLAGS before other arguments.
            CmdVet.Flag.Visit(f =>
            {
                if (addFromGOFLAGS[f.Name])
                {
                    passToVet = append(passToVet, fmt.Sprintf("-%s=%s", f.Name, f.Value));
                }

            });
            passToVet = append(passToVet, explicitFlags);
            return (passToVet, packageNames);

        }

        private static void exitWithUsage()
        {
            fmt.Fprintf(os.Stderr, "usage: %s\n", CmdVet.UsageLine);
            fmt.Fprintf(os.Stderr, "Run 'go help %s' for details.\n", CmdVet.LongName()); 

            // This part is additional to what (*Command).Usage does:
            @string cmd = "go tool vet";
            if (vetTool != "")
            {
                cmd = vetTool;
            }

            fmt.Fprintf(os.Stderr, "Run '%s -help' for the vet tool's flags.\n", cmd);

            @base.SetExitStatus(2L);
            @base.Exit();

        }
    }
}}}}
