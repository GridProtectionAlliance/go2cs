// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 08 04:36:59 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\tool.go
using fmt = go.fmt_package;
using build = go.go.build_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;

using cfg = go.cmd.go.@internal.cfg_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // Configuration for finding tool binaries.
        public static var ToolGOOS = runtime.GOOS;        public static var ToolGOARCH = runtime.GOARCH;        public static var ToolIsWindows = ToolGOOS == "windows";        public static var ToolDir = build.ToolDir;

        public static readonly @string ToolWindowsExtension = (@string)".exe";

        // Tool returns the path to the named tool (for example, "vet").
        // If the tool cannot be found, Tool exits the process.


        // Tool returns the path to the named tool (for example, "vet").
        // If the tool cannot be found, Tool exits the process.
        public static @string Tool(@string toolName)
        {
            var toolPath = filepath.Join(ToolDir, toolName);
            if (ToolIsWindows)
            {
                toolPath += ToolWindowsExtension;
            }

            if (len(cfg.BuildToolexec) > 0L)
            {
                return toolPath;
            } 
            // Give a nice message if there is no tool with that name.
            {
                var (_, err) = os.Stat(toolPath);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go tool: no such tool %q\n", toolName);
                    SetExitStatus(2L);
                    Exit();
                }

            }

            return toolPath;

        }
    }
}}}}
