// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package packagesdriver fetches type sizes for go/packages and go/analysis.
// package packagesdriver -- go2cs converted at 2022 March 06 23:31:32 UTC
// import "golang.org/x/tools/go/internal/packagesdriver" ==> using packagesdriver = go.golang.org.x.tools.go.@internal.packagesdriver_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\packagesdriver\sizes.go
using bytes = go.bytes_package;
using context = go.context_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using exec = go.os.exec_package;
using strings = go.strings_package;

using gocommand = go.golang.org.x.tools.@internal.gocommand_package;

namespace go.golang.org.x.tools.go.@internal;

public static partial class packagesdriver_package {

private static var debug = false;

public static (types.Sizes, error) GetSizes(context.Context ctx, slice<@string> buildFlags, slice<@string> env, ptr<gocommand.Runner> _addr_gocmdRunner, @string dir) {
    types.Sizes _p0 = default;
    error _p0 = default!;
    ref gocommand.Runner gocmdRunner = ref _addr_gocmdRunner.val;
 
    // TODO(matloob): Clean this up. This code is mostly a copy of packages.findExternalDriver.
    const @string toolPrefix = "GOPACKAGESDRIVER=";

    @string tool = "";
    foreach (var (_, env) in env) {
        {
            var val = strings.TrimPrefix(env, toolPrefix);

            if (val != env) {
                tool = val;
            }

        }

    }    if (tool == "") {
        error err = default!;
        tool, err = exec.LookPath("gopackagesdriver");
        if (err != null) { 
            // We did not find the driver, so use "go list".
            tool = "off";

        }
    }
    if (tool == "off") {
        return GetSizesGolist(ctx, buildFlags, env, _addr_gocmdRunner, dir);
    }
    var (req, err) = json.Marshal(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{Commandstring`json:"command"`Env[]string`json:"env"`BuildFlags[]string`json:"build_flags"`}{Command:"sizes",Env:env,BuildFlags:buildFlags,});
    if (err != null) {
        return (null, error.As(fmt.Errorf("failed to encode message to driver tool: %v", err))!);
    }
    ptr<object> buf = @new<bytes.Buffer>();
    var cmd = exec.CommandContext(ctx, tool);
    cmd.Dir = dir;
    cmd.Env = env;
    cmd.Stdin = bytes.NewReader(req);
    cmd.Stdout = buf;
    cmd.Stderr = @new<bytes.Buffer>();
    {
        error err__prev1 = err;

        err = cmd.Run();

        if (err != null) {
            return (null, error.As(fmt.Errorf("%v: %v: %s", tool, err, cmd.Stderr))!);
        }
        err = err__prev1;

    }

    ref var response = ref heap(out ptr<var> _addr_response);
    {
        error err__prev1 = err;

        err = json.Unmarshal(buf.Bytes(), _addr_response);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    return (response.Sizes, error.As(null!)!);

}

public static (types.Sizes, error) GetSizesGolist(context.Context ctx, slice<@string> buildFlags, slice<@string> env, ptr<gocommand.Runner> _addr_gocmdRunner, @string dir) {
    types.Sizes _p0 = default;
    error _p0 = default!;
    ref gocommand.Runner gocmdRunner = ref _addr_gocmdRunner.val;

    gocommand.Invocation inv = new gocommand.Invocation(Verb:"list",Args:[]string{"-f","{{context.GOARCH}} {{context.Compiler}}","--","unsafe"},Env:env,BuildFlags:buildFlags,WorkingDir:dir,);
    var (stdout, stderr, friendlyErr, rawErr) = gocmdRunner.RunRaw(ctx, inv);
    @string goarch = default;    @string compiler = default;

    if (rawErr != null) {
        if (strings.Contains(rawErr.Error(), "cannot find main module")) { 
            // User's running outside of a module. All bets are off. Get GOARCH and guess compiler is gc.
            // TODO(matloob): Is this a problem in practice?
            inv = new gocommand.Invocation(Verb:"env",Args:[]string{"GOARCH"},Env:env,WorkingDir:dir,);
            var (envout, enverr) = gocmdRunner.Run(ctx, inv);
            if (enverr != null) {
                return (null, error.As(enverr)!);
            }

            goarch = strings.TrimSpace(envout.String());
            compiler = "gc";

        }
        else
 {
            return (null, error.As(friendlyErr)!);
        }
    }
    else
 {
        var fields = strings.Fields(stdout.String());
        if (len(fields) < 2) {
            return (null, error.As(fmt.Errorf("could not parse GOARCH and Go compiler in format \"<GOARCH> <compiler>\":\nstdout: <<%s>>\nstderr: <<%s>>", stdout.String(), stderr.String()))!);
        }
        goarch = fields[0];
        compiler = fields[1];

    }
    return (types.SizesFor(compiler, goarch), error.As(null!)!);

}

} // end packagesdriver_package
