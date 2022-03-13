// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate ./mkalldocs.sh

// package main -- go2cs converted at 2022 March 13 06:29:22 UTC
// Original source: C:\Program Files\Go\src\cmd\go\main.go
namespace go;

using context = context_package;
using flag = flag_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using log = log_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using bug = cmd.go.@internal.bug_package;
using cfg = cmd.go.@internal.cfg_package;
using clean = cmd.go.@internal.clean_package;
using doc = cmd.go.@internal.doc_package;
using envcmd = cmd.go.@internal.envcmd_package;
using fix = cmd.go.@internal.fix_package;
using fmtcmd = cmd.go.@internal.fmtcmd_package;
using generate = cmd.go.@internal.generate_package;
using get = cmd.go.@internal.get_package;
using help = cmd.go.@internal.help_package;
using list = cmd.go.@internal.list_package;
using modcmd = cmd.go.@internal.modcmd_package;
using modfetch = cmd.go.@internal.modfetch_package;
using modget = cmd.go.@internal.modget_package;
using modload = cmd.go.@internal.modload_package;
using run = cmd.go.@internal.run_package;
using test = cmd.go.@internal.test_package;
using tool = cmd.go.@internal.tool_package;
using trace = cmd.go.@internal.trace_package;
using version = cmd.go.@internal.version_package;
using vet = cmd.go.@internal.vet_package;
using work = cmd.go.@internal.work_package;
using System;

public static partial class main_package {

private static void init() {
    @base.Go.Commands = new slice<ptr<base.Command>>(new ptr<base.Command>[] { bug.CmdBug, work.CmdBuild, clean.CmdClean, doc.CmdDoc, envcmd.CmdEnv, fix.CmdFix, fmtcmd.CmdFmt, generate.CmdGenerate, modget.CmdGet, work.CmdInstall, list.CmdList, modcmd.CmdMod, run.CmdRun, test.CmdTest, tool.CmdTool, version.CmdVersion, vet.CmdVet, help.HelpBuildConstraint, help.HelpBuildmode, help.HelpC, help.HelpCache, help.HelpEnvironment, help.HelpFileType, modload.HelpGoMod, help.HelpGopath, get.HelpGopathGet, modfetch.HelpGoproxy, help.HelpImportPath, modload.HelpModules, modget.HelpModuleGet, modfetch.HelpModuleAuth, help.HelpPackages, modfetch.HelpPrivate, test.HelpTestflag, test.HelpTestfunc, modget.HelpVCS });
}

private static void Main() {
    _ = go11tag;
    flag.Usage = @base.Usage;
    flag.Parse();
    log.SetFlags(0);

    var args = flag.Args();
    if (len(args) < 1) {
        @base.Usage();
    }
    if (args[0] == "get" || args[0] == "help") {
        if (!modload.WillBeEnabled()) { 
            // Replace module-aware get with GOPATH get if appropriate.
            modget.CmdGet.val = get.CmdGet.val;
        }
    }
    cfg.CmdName = args[0]; // for error messages
    if (args[0] == "help") {
        help.Help(os.Stdout, args[(int)1..]);
        return ;
    }
    {
        var gopath = cfg.BuildContext.GOPATH;

        if (filepath.Clean(gopath) == filepath.Clean(runtime.GOROOT())) {
            fmt.Fprintf(os.Stderr, "warning: GOPATH set to GOROOT (%s) has no effect\n", gopath);
        }
        else
 {
            foreach (var (_, p) in filepath.SplitList(gopath)) { 
                // Some GOPATHs have empty directory elements - ignore them.
                // See issue 21928 for details.
                if (p == "") {
                    continue;
                } 
                // Note: using HasPrefix instead of Contains because a ~ can appear
                // in the middle of directory elements, such as /tmp/git-1.8.2~rc3
                // or C:\PROGRA~1. Only ~ as a path prefix has meaning to the shell.
                if (strings.HasPrefix(p, "~")) {
                    fmt.Fprintf(os.Stderr, "go: GOPATH entry cannot start with shell metacharacter '~': %q\n", p);
                    os.Exit(2);
                }
                if (!filepath.IsAbs(p)) {
                    if (cfg.Getenv("GOPATH") == "") { 
                        // We inferred $GOPATH from $HOME and did a bad job at it.
                        // Instead of dying, uninfer it.
                        cfg.BuildContext.GOPATH = "";
                    }
                    else
 {
                        fmt.Fprintf(os.Stderr, "go: GOPATH entry is relative; must be absolute path: %q.\nFor more details see: 'go help gopath'\n", p);
                        os.Exit(2);
                    }
                }
            }
        }
    }

    {
        var (fi, err) = os.Stat(cfg.GOROOT);

        if (err != null || !fi.IsDir()) {
            fmt.Fprintf(os.Stderr, "go: cannot find GOROOT directory: %v\n", cfg.GOROOT);
            os.Exit(2);
        }
    }

BigCmdLoop:
    {
        var bigCmd = @base.Go;

        while () {
            foreach (var (_, cmd) in bigCmd.Commands) {
                if (cmd.Name() != args[0]) {
                    continue;
                }
                if (len(cmd.Commands) > 0) {
                    bigCmd = cmd;
                    args = args[(int)1..];
                    if (len(args) == 0) {
                        help.PrintUsage(os.Stderr, bigCmd);
                        @base.SetExitStatus(2);
                        @base.Exit();
                    }
                    if (args[0] == "help") { 
                        // Accept 'go mod help' and 'go mod help foo' for 'go help mod' and 'go help mod foo'.
                        help.Help(os.Stdout, append(strings.Split(cfg.CmdName, " "), args[(int)1..]));
                        return ;
                    }
                    cfg.CmdName += " " + args[0];
                    _continueBigCmdLoop = true;
                    break;
                }
                if (!cmd.Runnable()) {
                    continue;
                }
                invoke(_addr_cmd, args);
                @base.Exit();
                return ;
            }
            @string helpArg = "";
            {
                var i = strings.LastIndex(cfg.CmdName, " ");

                if (i >= 0) {
                    helpArg = " " + cfg.CmdName[..(int)i];
                }

            }
            fmt.Fprintf(os.Stderr, "go %s: unknown command\nRun 'go help%s' for usage.\n", cfg.CmdName, helpArg);
            @base.SetExitStatus(2);
            @base.Exit();
        }
    }
}

private static void invoke(ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;
 
    // 'go env' handles checking the build config
    if (cmd != envcmd.CmdEnv) {
        buildcfg.Check();
    }
    cfg.OrigEnv = os.Environ();
    cfg.CmdEnv = envcmd.MkEnv();
    foreach (var (_, env) in cfg.CmdEnv) {
        if (os.Getenv(env.Name) != env.Value) {
            os.Setenv(env.Name, env.Value);
        }
    }    cmd.Flag.Usage = () => {
        cmd.Usage();
    };
    if (cmd.CustomFlags) {
        args = args[(int)1..];
    }
    else
 {
        @base.SetFromGOFLAGS(_addr_cmd.Flag);
        cmd.Flag.Parse(args[(int)1..]);
        args = cmd.Flag.Args();
    }
    var ctx = maybeStartTrace(context.Background());
    var (ctx, span) = trace.StartSpan(ctx, fmt.Sprint("Running ", cmd.Name(), " command"));
    cmd.Run(ctx, cmd, args);
    span.Done();
}

private static void init() {
    @base.Usage = mainUsage;
}

private static void mainUsage() {
    help.PrintUsage(os.Stderr, @base.Go);
    os.Exit(2);
}

private static context.Context maybeStartTrace(context.Context pctx) {
    if (cfg.DebugTrace == "") {
        return pctx;
    }
    var (ctx, close, err) = trace.Start(pctx, cfg.DebugTrace);
    if (err != null) {
        @base.Fatalf("failed to start trace: %v", err);
    }
    @base.AtExit(() => {
        {
            var err = close();

            if (err != null) {
                @base.Fatalf("failed to stop trace: %v", err);
            }

        }
    });

    return ctx;
}

} // end main_package
