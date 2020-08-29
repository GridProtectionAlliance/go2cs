// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate ./mkalldocs.sh

// package main -- go2cs converted at 2020 August 29 10:00:27 UTC
// Original source: C:\Go\src\cmd\go\main.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using bug = go.cmd.go.@internal.bug_package;
using cfg = go.cmd.go.@internal.cfg_package;
using clean = go.cmd.go.@internal.clean_package;
using doc = go.cmd.go.@internal.doc_package;
using envcmd = go.cmd.go.@internal.envcmd_package;
using fix = go.cmd.go.@internal.fix_package;
using fmtcmd = go.cmd.go.@internal.fmtcmd_package;
using generate = go.cmd.go.@internal.generate_package;
using get = go.cmd.go.@internal.get_package;
using help = go.cmd.go.@internal.help_package;
using list = go.cmd.go.@internal.list_package;
using run = go.cmd.go.@internal.run_package;
using test = go.cmd.go.@internal.test_package;
using tool = go.cmd.go.@internal.tool_package;
using version = go.cmd.go.@internal.version_package;
using vet = go.cmd.go.@internal.vet_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            @base.Commands = new slice<ref base.Command>(new ref base.Command[] { work.CmdBuild, clean.CmdClean, doc.CmdDoc, envcmd.CmdEnv, bug.CmdBug, fix.CmdFix, fmtcmd.CmdFmt, generate.CmdGenerate, get.CmdGet, work.CmdInstall, list.CmdList, run.CmdRun, test.CmdTest, tool.CmdTool, version.CmdVersion, vet.CmdVet, help.HelpC, help.HelpBuildmode, help.HelpCache, help.HelpFileType, help.HelpGopath, help.HelpEnvironment, help.HelpImportPath, help.HelpPackages, test.HelpTestflag, test.HelpTestfunc });
        }

        private static void Main()
        {
            _ = go11tag;
            flag.Usage = @base.Usage;
            flag.Parse();
            log.SetFlags(0L);

            var args = flag.Args();
            if (len(args) < 1L)
            {
                @base.Usage();
            }
            cfg.CmdName = args[0L]; // for error messages
            if (args[0L] == "help")
            {
                help.Help(args[1L..]);
                return;
            } 

            // Diagnose common mistake: GOPATH==GOROOT.
            // This setting is equivalent to not setting GOPATH at all,
            // which is not what most people want when they do it.
            {
                var gopath = cfg.BuildContext.GOPATH;

                if (filepath.Clean(gopath) == filepath.Clean(runtime.GOROOT()))
                {
                    fmt.Fprintf(os.Stderr, "warning: GOPATH set to GOROOT (%s) has no effect\n", gopath);
                }
                else
                {
                    foreach (var (_, p) in filepath.SplitList(gopath))
                    { 
                        // Some GOPATHs have empty directory elements - ignore them.
                        // See issue 21928 for details.
                        if (p == "")
                        {
                            continue;
                        } 
                        // Note: using HasPrefix instead of Contains because a ~ can appear
                        // in the middle of directory elements, such as /tmp/git-1.8.2~rc3
                        // or C:\PROGRA~1. Only ~ as a path prefix has meaning to the shell.
                        if (strings.HasPrefix(p, "~"))
                        {
                            fmt.Fprintf(os.Stderr, "go: GOPATH entry cannot start with shell metacharacter '~': %q\n", p);
                            os.Exit(2L);
                        }
                        if (!filepath.IsAbs(p))
                        {
                            fmt.Fprintf(os.Stderr, "go: GOPATH entry is relative; must be absolute path: %q.\nFor more details see: 'go help gopath'\n", p);
                            os.Exit(2L);
                        }
                    }
                }

            }

            {
                var (fi, err) = os.Stat(cfg.GOROOT);

                if (err != null || !fi.IsDir())
                {
                    fmt.Fprintf(os.Stderr, "go: cannot find GOROOT directory: %v\n", cfg.GOROOT);
                    os.Exit(2L);
                } 

                // Set environment (GOOS, GOARCH, etc) explicitly.
                // In theory all the commands we invoke should have
                // the same default computation of these as we do,
                // but in practice there might be skew
                // This makes sure we all agree.

            } 

            // Set environment (GOOS, GOARCH, etc) explicitly.
            // In theory all the commands we invoke should have
            // the same default computation of these as we do,
            // but in practice there might be skew
            // This makes sure we all agree.
            cfg.OrigEnv = os.Environ();
            cfg.CmdEnv = envcmd.MkEnv();
            foreach (var (_, env) in cfg.CmdEnv)
            {
                if (os.Getenv(env.Name) != env.Value)
                {
                    os.Setenv(env.Name, env.Value);
                }
            }
            foreach (var (_, cmd) in @base.Commands)
            {
                if (cmd.Name() == args[0L] && cmd.Runnable())
                {
                    cmd.Flag.Usage = () =>
                    {
                        cmd.Usage();

                    }
;
                    if (cmd.CustomFlags)
                    {
                        args = args[1L..];
                    }
                    else
                    {
                        cmd.Flag.Parse(args[1L..]);
                        args = cmd.Flag.Args();
                    }
                    cmd.Run(cmd, args);
                    @base.Exit();
                    return;
                }
            }
            fmt.Fprintf(os.Stderr, "go: unknown subcommand %q\nRun 'go help' for usage.\n", args[0L]);
            @base.SetExitStatus(2L);
            @base.Exit();
        }

        private static void init()
        {
            @base.Usage = mainUsage;
        }

        private static void mainUsage()
        { 
            // special case "go test -h"
            if (len(os.Args) > 1L && os.Args[1L] == "test")
            {
                test.Usage();
            }
            help.PrintUsage(os.Stderr);
            os.Exit(2L);
        }
    }
}
