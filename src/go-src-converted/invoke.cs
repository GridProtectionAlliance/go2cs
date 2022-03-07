// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gocommand is a helper for calling the go command.
// package gocommand -- go2cs converted at 2022 March 06 23:31:33 UTC
// import "golang.org/x/tools/internal/gocommand" ==> using gocommand = go.golang.org.x.tools.@internal.gocommand_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\gocommand\invoke.go
using bytes = go.bytes_package;
using context = go.context_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using @event = go.golang.org.x.tools.@internal.@event_package;
using System;
using System.Threading;


namespace go.golang.org.x.tools.@internal;

public static partial class gocommand_package {

    // An Runner will run go command invocations and serialize
    // them if it sees a concurrency error.
public partial struct Runner {
    public sync.Mutex loadMu;
    public nint serializeLoads;
}

// 1.13: go: updates to go.mod needed, but contents have changed
// 1.14: go: updating go.mod: existing contents have changed since last read
private static var modConcurrencyError = regexp.MustCompile("go:.*go.mod.*contents have changed");

// Run calls Runner.RunRaw, serializing requests if they fight over
// go.mod changes.
private static (ptr<bytes.Buffer>, error) Run(this ptr<Runner> _addr_runner, context.Context ctx, Invocation inv) {
    ptr<bytes.Buffer> _p0 = default!;
    error _p0 = default!;
    ref Runner runner = ref _addr_runner.val;

    var (stdout, _, friendly, _) = runner.RunRaw(ctx, inv);
    return (_addr_stdout!, error.As(friendly)!);
}

// RunRaw calls Invocation.runRaw, serializing requests if they fight over
// go.mod changes.
private static (ptr<bytes.Buffer>, ptr<bytes.Buffer>, error, error) RunRaw(this ptr<Runner> _addr_runner, context.Context ctx, Invocation inv) => func((defer, _, _) => {
    ptr<bytes.Buffer> _p0 = default!;
    ptr<bytes.Buffer> _p0 = default!;
    error _p0 = default!;
    error _p0 = default!;
    ref Runner runner = ref _addr_runner.val;
 
    // We want to run invocations concurrently as much as possible. However,
    // if go.mod updates are needed, only one can make them and the others will
    // fail. We need to retry in those cases, but we don't want to thrash so
    // badly we never recover. To avoid that, once we've seen one concurrency
    // error, start serializing everything until the backlog has cleared out.
    runner.loadMu.Lock();
    bool locked = default; // If true, we hold the mutex and have incremented.
    if (runner.serializeLoads == 0) {
        runner.loadMu.Unlock();
    }
    else
 {
        locked = true;
        runner.serializeLoads++;
    }
    defer(() => {
        if (locked) {
            runner.serializeLoads--;
            runner.loadMu.Unlock();
        }
    }());

    while (true) {
        var (stdout, stderr, friendlyErr, err) = inv.runRaw(ctx);
        if (friendlyErr == null || !modConcurrencyError.MatchString(friendlyErr.Error())) {
            return (_addr_stdout!, _addr_stderr!, error.As(friendlyErr)!, error.As(err)!);
        }
        @event.Error(ctx, "Load concurrency error, will retry serially", err);
        if (!locked) {
            runner.loadMu.Lock();
            runner.serializeLoads++;
            locked = true;
        }
    }

});

// An Invocation represents a call to the go command.
public partial struct Invocation {
    public @string Verb;
    public slice<@string> Args;
    public slice<@string> BuildFlags;
    public slice<@string> Env;
    public @string WorkingDir;
    public Action<@string, object[]> Logf;
}

// RunRaw is like RunPiped, but also returns the raw stderr and error for callers
// that want to do low-level error handling/recovery.
private static (ptr<bytes.Buffer>, ptr<bytes.Buffer>, error, error) runRaw(this ptr<Invocation> _addr_i, context.Context ctx) {
    ptr<bytes.Buffer> stdout = default!;
    ptr<bytes.Buffer> stderr = default!;
    error friendlyError = default!;
    error rawError = default!;
    ref Invocation i = ref _addr_i.val;

    stdout = addr(new bytes.Buffer());
    stderr = addr(new bytes.Buffer());
    rawError = i.RunPiped(ctx, stdout, stderr);
    if (rawError != null) {
        friendlyError = rawError; 
        // Check for 'go' executable not being found.
        {
            ptr<exec.Error> (ee, ok) = rawError._<ptr<exec.Error>>();

            if (ok && ee.Err == exec.ErrNotFound) {
                friendlyError = fmt.Errorf("go command required, not found: %v", ee);
            }

        }

        if (ctx.Err() != null) {
            friendlyError = ctx.Err();
        }
        friendlyError = fmt.Errorf("err: %v: stderr: %s", friendlyError, stderr);

    }
    return ;

}

// RunPiped is like Run, but relies on the given stdout/stderr
private static error RunPiped(this ptr<Invocation> _addr_i, context.Context ctx, io.Writer stdout, io.Writer stderr) => func((defer, _, _) => {
    ref Invocation i = ref _addr_i.val;

    var log = i.Logf;
    if (log == null) {
        log = (_p0, _p0) => {
        };

    }
    @string goArgs = new slice<@string>(new @string[] { i.Verb });
    switch (i.Verb) {
        case "mod": 
            // mod needs the sub-verb before build flags.
            goArgs = append(goArgs, i.Args[0]);
            goArgs = append(goArgs, i.BuildFlags);
            goArgs = append(goArgs, i.Args[(int)1..]);
            break;
        case "env": 
            // env doesn't take build flags.
            goArgs = append(goArgs, i.Args);
            break;
        default: 
            goArgs = append(goArgs, i.BuildFlags);
            goArgs = append(goArgs, i.Args);
            break;
    }
    var cmd = exec.Command("go", goArgs);
    cmd.Stdout = stdout;
    cmd.Stderr = stderr; 
    // On darwin the cwd gets resolved to the real path, which breaks anything that
    // expects the working directory to keep the original path, including the
    // go command when dealing with modules.
    // The Go stdlib has a special feature where if the cwd and the PWD are the
    // same node then it trusts the PWD, so by setting it in the env for the child
    // process we fix up all the paths returned by the go command.
    cmd.Env = append(os.Environ(), i.Env);
    if (i.WorkingDir != "") {
        cmd.Env = append(cmd.Env, "PWD=" + i.WorkingDir);
        cmd.Dir = i.WorkingDir;
    }
    defer(start => {
        log("%s for %v", time.Since(start), cmdDebugStr(_addr_cmd));
    }(time.Now()));

    return error.As(runCmdContext(ctx, _addr_cmd))!;

});

// runCmdContext is like exec.CommandContext except it sends os.Interrupt
// before os.Kill.
private static error runCmdContext(context.Context ctx, ptr<exec.Cmd> _addr_cmd) {
    ref exec.Cmd cmd = ref _addr_cmd.val;

    {
        var err__prev1 = err;

        var err = cmd.Start();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var resChan = make_channel<error>(1);
    go_(() => () => {
        resChan.Send(cmd.Wait());
    }());

    return error.As(err)!;
    cmd.Process.Signal(os.Interrupt);
    return error.As(err)!;
    cmd.Process.Kill();
    return error.As(resChan.Receive())!;

}

private static @string cmdDebugStr(ptr<exec.Cmd> _addr_cmd) {
    ref exec.Cmd cmd = ref _addr_cmd.val;

    var env = make_map<@string, @string>();
    foreach (var (_, kv) in cmd.Env) {
        var split = strings.Split(kv, "=");
        var k = split[0];
        var v = split[1];
        env[k] = v;

    }    return fmt.Sprintf("GOROOT=%v GOPATH=%v GO111MODULE=%v GOPROXY=%v PWD=%v go %v", env["GOROOT"], env["GOPATH"], env["GO111MODULE"], env["GOPROXY"], env["PWD"], cmd.Args);

}

} // end gocommand_package
