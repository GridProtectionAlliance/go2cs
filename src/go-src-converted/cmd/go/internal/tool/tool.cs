// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tool implements the ``go tool'' command.

// package tool -- go2cs converted at 2022 March 13 06:29:52 UTC
// import "cmd/go/internal/tool" ==> using tool = go.cmd.go.@internal.tool_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\tool\tool.go
namespace go.cmd.go.@internal;

using context = context_package;
using fmt = fmt_package;
using exec = @internal.execabs_package;
using os = os_package;
using signal = os.signal_package;
using sort = sort_package;
using strings = strings_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using System;
using System.Threading;

public static partial class tool_package {

public static ptr<base.Command> CmdTool = addr(new base.Command(Run:runTool,UsageLine:"go tool [-n] command [args...]",Short:"run specified go tool",Long:`
Tool runs the go tool command identified by the arguments.
With no arguments it prints the list of known tools.

The -n flag causes tool to print the command that would be
executed but not execute it.

For more about each tool command, see 'go doc cmd/<command>'.
`,));

private static bool toolN = default;

// Return whether tool can be expected in the gccgo tool directory.
// Other binaries could be in the same directory so don't
// show those with the 'go tool' command.
private static bool isGccgoTool(@string tool) {
    switch (tool) {
        case "cgo": 

        case "fix": 

        case "cover": 

        case "godoc": 

        case "vet": 
            return true;
            break;
    }
    return false;
}

private static void init() {
    CmdTool.Flag.BoolVar(_addr_toolN, "n", false, "");
}

private static void runTool(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) == 0) {
        listTools();
        return ;
    }
    var toolName = args[0]; 
    // The tool name must be lower-case letters, numbers or underscores.
    {
        var c__prev1 = c;

        foreach (var (_, __c) in toolName) {
            c = __c;

            if ('a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_')             else 
                fmt.Fprintf(os.Stderr, "go tool: bad tool name %q\n", toolName);
                @base.SetExitStatus(2);
                return ;
                    }
        c = c__prev1;
    }

    var toolPath = @base.Tool(toolName);
    if (toolPath == "") {
        return ;
    }
    if (toolN) {
        var cmd = toolPath;
        if (len(args) > 1) {
            cmd += " " + strings.Join(args[(int)1..], " ");
        }
        fmt.Printf("%s\n", cmd);
        return ;
    }
    args[0] = toolPath; // in case the tool wants to re-exec itself, e.g. cmd/dist
    ptr<exec.Cmd> toolCmd = addr(new exec.Cmd(Path:toolPath,Args:args,Stdin:os.Stdin,Stdout:os.Stdout,Stderr:os.Stderr,));
    var err = toolCmd.Start();
    if (err == null) {
        var c = make_channel<os.Signal>(100);
        signal.Notify(c);
        go_(() => () => {
            foreach (var (sig) in c) {
                toolCmd.Process.Signal(sig);
            }
        }());
        err = toolCmd.Wait();
        signal.Stop(c);
        close(c);
    }
    if (err != null) { 
        // Only print about the exit status if the command
        // didn't even run (not an ExitError) or it didn't exit cleanly
        // or we're printing command lines too (-x mode).
        // Assume if command exited cleanly (even with non-zero status)
        // it printed any messages it wanted to print.
        {
            ptr<exec.ExitError> (e, ok) = err._<ptr<exec.ExitError>>();

            if (!ok || !e.Exited() || cfg.BuildX) {
                fmt.Fprintf(os.Stderr, "go tool %s: %s\n", toolName, err);
            }

        }
        @base.SetExitStatus(1);
        return ;
    }
}

// listTools prints a list of the available tools in the tools directory.
private static void listTools() => func((defer, _, _) => {
    var (f, err) = os.Open(@base.ToolDir);
    if (err != null) {
        fmt.Fprintf(os.Stderr, "go tool: no tool directory: %s\n", err);
        @base.SetExitStatus(2);
        return ;
    }
    defer(f.Close());
    var (names, err) = f.Readdirnames(-1);
    if (err != null) {
        fmt.Fprintf(os.Stderr, "go tool: can't read directory: %s\n", err);
        @base.SetExitStatus(2);
        return ;
    }
    sort.Strings(names);
    foreach (var (_, name) in names) { 
        // Unify presentation by going to lower case.
        name = strings.ToLower(name); 
        // If it's windows, don't show the .exe suffix.
        if (@base.ToolIsWindows && strings.HasSuffix(name, @base.ToolWindowsExtension)) {
            name = name[..(int)len(name) - len(@base.ToolWindowsExtension)];
        }
        if (cfg.BuildToolchainName == "gccgo" && !isGccgoTool(name)) {
            continue;
        }
        fmt.Println(name);
    }
});

} // end tool_package
