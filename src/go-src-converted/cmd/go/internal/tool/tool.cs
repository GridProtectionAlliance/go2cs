// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tool implements the ``go tool'' command.
// package tool -- go2cs converted at 2020 October 08 04:33:48 UTC
// import "cmd/go/internal/tool" ==> using tool = go.cmd.go.@internal.tool_package
// Original source: C:\Go\src\cmd\go\internal\tool\tool.go
using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using sort = go.sort_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class tool_package
    {
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
        private static bool isGccgoTool(@string tool)
        {
            switch (tool)
            {
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

        private static void init()
        {
            CmdTool.Flag.BoolVar(_addr_toolN, "n", false, "");
        }

        private static void runTool(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (len(args) == 0L)
            {
                listTools();
                return ;
            }

            var toolName = args[0L]; 
            // The tool name must be lower-case letters, numbers or underscores.
            foreach (var (_, c) in toolName)
            {

                if ('a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_')                 else 
                    fmt.Fprintf(os.Stderr, "go tool: bad tool name %q\n", toolName);
                    @base.SetExitStatus(2L);
                    return ;
                
            }
            var toolPath = @base.Tool(toolName);
            if (toolPath == "")
            {
                return ;
            }

            if (toolN)
            {
                var cmd = toolPath;
                if (len(args) > 1L)
                {
                    cmd += " " + strings.Join(args[1L..], " ");
                }

                fmt.Printf("%s\n", cmd);
                return ;

            }

            args[0L] = toolPath; // in case the tool wants to re-exec itself, e.g. cmd/dist
            ptr<exec.Cmd> toolCmd = addr(new exec.Cmd(Path:toolPath,Args:args,Stdin:os.Stdin,Stdout:os.Stdout,Stderr:os.Stderr,));
            var err = toolCmd.Run();
            if (err != null)
            { 
                // Only print about the exit status if the command
                // didn't even run (not an ExitError) or it didn't exit cleanly
                // or we're printing command lines too (-x mode).
                // Assume if command exited cleanly (even with non-zero status)
                // it printed any messages it wanted to print.
                {
                    ptr<exec.ExitError> (e, ok) = err._<ptr<exec.ExitError>>();

                    if (!ok || !e.Exited() || cfg.BuildX)
                    {
                        fmt.Fprintf(os.Stderr, "go tool %s: %s\n", toolName, err);
                    }

                }

                @base.SetExitStatus(1L);
                return ;

            }

        }

        // listTools prints a list of the available tools in the tools directory.
        private static void listTools() => func((defer, _, __) =>
        {
            var (f, err) = os.Open(@base.ToolDir);
            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "go tool: no tool directory: %s\n", err);
                @base.SetExitStatus(2L);
                return ;
            }

            defer(f.Close());
            var (names, err) = f.Readdirnames(-1L);
            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "go tool: can't read directory: %s\n", err);
                @base.SetExitStatus(2L);
                return ;
            }

            sort.Strings(names);
            foreach (var (_, name) in names)
            { 
                // Unify presentation by going to lower case.
                name = strings.ToLower(name); 
                // If it's windows, don't show the .exe suffix.
                if (@base.ToolIsWindows && strings.HasSuffix(name, @base.ToolWindowsExtension))
                {
                    name = name[..len(name) - len(@base.ToolWindowsExtension)];
                } 
                // The tool directory used by gccgo will have other binaries
                // in addition to go tools. Only display go tools here.
                if (cfg.BuildToolchainName == "gccgo" && !isGccgoTool(name))
                {
                    continue;
                }

                fmt.Println(name);

            }

        });
    }
}}}}
