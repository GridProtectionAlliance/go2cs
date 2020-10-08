// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file enables an external tool to intercept package requests.
// If the tool is present then its results are used in preference to
// the go list command.

// package packages -- go2cs converted at 2020 October 08 04:54:43 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\external.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using os = go.os_package;
using exec = go.os.exec_package;
using strings = go.strings_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        // The Driver Protocol
        //
        // The driver, given the inputs to a call to Load, returns metadata about the packages specified.
        // This allows for different build systems to support go/packages by telling go/packages how the
        // packages' source is organized.
        // The driver is a binary, either specified by the GOPACKAGESDRIVER environment variable or in
        // the path as gopackagesdriver. It's given the inputs to load in its argv. See the package
        // documentation in doc.go for the full description of the patterns that need to be supported.
        // A driver receives as a JSON-serialized driverRequest struct in standard input and will
        // produce a JSON-serialized driverResponse (see definition in packages.go) in its standard output.

        // driverRequest is used to provide the portion of Load's Config that is needed by a driver.
        private partial struct driverRequest
        {
            [Description("json:\"mode\"")]
            public LoadMode Mode; // Env specifies the environment the underlying build system should be run in.
            [Description("json:\"env\"")]
            public slice<@string> Env; // BuildFlags are flags that should be passed to the underlying build system.
            [Description("json:\"build_flags\"")]
            public slice<@string> BuildFlags; // Tests specifies whether the patterns should also return test packages.
            [Description("json:\"tests\"")]
            public bool Tests; // Overlay maps file paths (relative to the driver's working directory) to the byte contents
// of overlay files.
            [Description("json:\"overlay\"")]
            public map<@string, slice<byte>> Overlay;
        }

        // findExternalDriver returns the file path of a tool that supplies
        // the build system package structure, or "" if not found."
        // If GOPACKAGESDRIVER is set in the environment findExternalTool returns its
        // value, otherwise it searches for a binary named gopackagesdriver on the PATH.
        private static driver findExternalDriver(ptr<Config> _addr_cfg)
        {
            ref Config cfg = ref _addr_cfg.val;

            const @string toolPrefix = (@string)"GOPACKAGESDRIVER=";

            @string tool = "";
            foreach (var (_, env) in cfg.Env)
            {
                {
                    var val = strings.TrimPrefix(env, toolPrefix);

                    if (val != env)
                    {
                        tool = val;
                    }

                }

            }
            if (tool != "" && tool == "off")
            {
                return null;
            }

            if (tool == "")
            {
                error err = default!;
                tool, err = exec.LookPath("gopackagesdriver");
                if (err != null)
                {
                    return null;
                }

            }

            return (cfg, words) =>
            {
                var (req, err) = json.Marshal(new driverRequest(Mode:cfg.Mode,Env:cfg.Env,BuildFlags:cfg.BuildFlags,Tests:cfg.Tests,Overlay:cfg.Overlay,));
                if (err != null)
                {
                    return (null, fmt.Errorf("failed to encode message to driver tool: %v", err));
                }

                ptr<object> buf = @new<bytes.Buffer>();
                ptr<object> stderr = @new<bytes.Buffer>();
                var cmd = exec.CommandContext(cfg.Context, tool, words);
                cmd.Dir = cfg.Dir;
                cmd.Env = cfg.Env;
                cmd.Stdin = bytes.NewReader(req);
                cmd.Stdout = buf;
                cmd.Stderr = stderr;

                {
                    error err__prev1 = err;

                    err = cmd.Run();

                    if (err != null)
                    {
                        return (null, fmt.Errorf("%v: %v: %s", tool, err, cmd.Stderr));
                    }

                    err = err__prev1;

                }

                if (len(stderr.Bytes()) != 0L && os.Getenv("GOPACKAGESPRINTDRIVERERRORS") != "")
                {
                    fmt.Fprintf(os.Stderr, "%s stderr: <<%s>>\n", cmdDebugStr(cmd, words), stderr);
                }

                ref driverResponse response = ref heap(out ptr<driverResponse> _addr_response);
                {
                    error err__prev1 = err;

                    err = json.Unmarshal(buf.Bytes(), _addr_response);

                    if (err != null)
                    {
                        return (null, err);
                    }

                    err = err__prev1;

                }

                return (_addr_response, null);

            };

        }
    }
}}}}}
