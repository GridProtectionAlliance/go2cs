// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base defines shared basic pieces of the go command,
// in particular logging and the Command structure.

// package @base -- go2cs converted at 2022 March 13 06:29:23 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\base.go
namespace go.cmd.go.@internal;

using context = context_package;
using flag = flag_package;
using fmt = fmt_package;
using exec = @internal.execabs_package;
using log = log_package;
using os = os_package;
using strings = strings_package;
using sync = sync_package;

using cfg = cmd.go.@internal.cfg_package;
using str = cmd.go.@internal.str_package;


// A Command is an implementation of a go command
// like go build or go fix.

using System;
public static partial class @base_package {

public partial struct Command {
    public Action<context.Context, ptr<Command>, slice<@string>> Run; // UsageLine is the one-line usage message.
// The words between "go" and the first flag or argument in the line are taken to be the command name.
    public @string UsageLine; // Short is the short description shown in the 'go help' output.
    public @string Short; // Long is the long message shown in the 'go help <this-command>' output.
    public @string Long; // Flag is a set of flags specific to this command.
    public flag.FlagSet Flag; // CustomFlags indicates that the command will do its own
// flag parsing.
    public bool CustomFlags; // Commands lists the available commands and help topics.
// The order here is the order in which they are printed by 'go help'.
// Note that subcommands are in general best avoided.
    public slice<ptr<Command>> Commands;
}

public static ptr<Command> Go = addr(new Command(UsageLine:"go",Long:`Go is a tool for managing Go source code.`,));

// hasFlag reports whether a command or any of its subcommands contain the given
// flag.
private static bool hasFlag(ptr<Command> _addr_c, @string name) {
    ref Command c = ref _addr_c.val;

    {
        var f = c.Flag.Lookup(name);

        if (f != null) {
            return true;
        }
    }
    foreach (var (_, sub) in c.Commands) {
        if (hasFlag(_addr_sub, name)) {
            return true;
        }
    }    return false;
}

// LongName returns the command's long name: all the words in the usage line between "go" and a flag or argument,
private static @string LongName(this ptr<Command> _addr_c) {
    ref Command c = ref _addr_c.val;

    var name = c.UsageLine;
    {
        var i = strings.Index(name, " [");

        if (i >= 0) {
            name = name[..(int)i];
        }
    }
    if (name == "go") {
        return "";
    }
    return strings.TrimPrefix(name, "go ");
}

// Name returns the command's short name: the last word in the usage line before a flag or argument.
private static @string Name(this ptr<Command> _addr_c) {
    ref Command c = ref _addr_c.val;

    var name = c.LongName();
    {
        var i = strings.LastIndex(name, " ");

        if (i >= 0) {
            name = name[(int)i + 1..];
        }
    }
    return name;
}

private static void Usage(this ptr<Command> _addr_c) {
    ref Command c = ref _addr_c.val;

    fmt.Fprintf(os.Stderr, "usage: %s\n", c.UsageLine);
    fmt.Fprintf(os.Stderr, "Run 'go help %s' for details.\n", c.LongName());
    SetExitStatus(2);
    Exit();
}

// Runnable reports whether the command can be run; otherwise
// it is a documentation pseudo-command such as importpath.
private static bool Runnable(this ptr<Command> _addr_c) {
    ref Command c = ref _addr_c.val;

    return c.Run != null;
}

private static slice<Action> atExitFuncs = default;

public static void AtExit(Action f) {
    atExitFuncs = append(atExitFuncs, f);
}

public static void Exit() {
    foreach (var (_, f) in atExitFuncs) {
        f();
    }    os.Exit(exitStatus);
}

public static void Fatalf(@string format, params object[] args) {
    args = args.Clone();

    Errorf(format, args);
    Exit();
}

public static void Errorf(@string format, params object[] args) {
    args = args.Clone();

    log.Printf(format, args);
    SetExitStatus(1);
}

public static void ExitIfErrors() {
    if (exitStatus != 0) {
        Exit();
    }
}

private static nint exitStatus = 0;
private static sync.Mutex exitMu = default;

public static void SetExitStatus(nint n) {
    exitMu.Lock();
    if (exitStatus < n) {
        exitStatus = n;
    }
    exitMu.Unlock();
}

public static nint GetExitStatus() {
    return exitStatus;
}

// Run runs the command, with stdout and stderr
// connected to the go command's own stdout and stderr.
// If the command fails, Run reports the error using Errorf.
public static void Run(params object[] cmdargs) {
    cmdargs = cmdargs.Clone();

    var cmdline = str.StringList(cmdargs);
    if (cfg.BuildN || cfg.BuildX) {
        fmt.Printf("%s\n", strings.Join(cmdline, " "));
        if (cfg.BuildN) {
            return ;
        }
    }
    var cmd = exec.Command(cmdline[0], cmdline[(int)1..]);
    cmd.Stdout = os.Stdout;
    cmd.Stderr = os.Stderr;
    {
        var err = cmd.Run();

        if (err != null) {
            Errorf("%v", err);
        }
    }
}

// RunStdin is like run but connects Stdin.
public static void RunStdin(slice<@string> cmdline) {
    var cmd = exec.Command(cmdline[0], cmdline[(int)1..]);
    cmd.Stdin = os.Stdin;
    cmd.Stdout = os.Stdout;
    cmd.Stderr = os.Stderr;
    cmd.Env = cfg.OrigEnv;
    StartSigHandlers();
    {
        var err = cmd.Run();

        if (err != null) {
            Errorf("%v", err);
        }
    }
}

// Usage is the usage-reporting function, filled in by package main
// but here for reference by other packages.
public static Action Usage = default;

} // end @base_package
