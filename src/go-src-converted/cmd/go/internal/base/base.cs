// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base defines shared basic pieces of the go command,
// in particular logging and the Command structure.
// package @base -- go2cs converted at 2020 October 09 05:45:06 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\base.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using strings = go.strings_package;
using sync = go.sync_package;

using cfg = go.cmd.go.@internal.cfg_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // A Command is an implementation of a go command
        // like go build or go fix.
        public partial struct Command
        {
            public Action<ptr<Command>, slice<@string>> Run; // UsageLine is the one-line usage message.
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

        // LongName returns the command's long name: all the words in the usage line between "go" and a flag or argument,
        private static @string LongName(this ptr<Command> _addr_c)
        {
            ref Command c = ref _addr_c.val;

            var name = c.UsageLine;
            {
                var i = strings.Index(name, " [");

                if (i >= 0L)
                {
                    name = name[..i];
                }

            }

            if (name == "go")
            {
                return "";
            }

            return strings.TrimPrefix(name, "go ");

        }

        // Name returns the command's short name: the last word in the usage line before a flag or argument.
        private static @string Name(this ptr<Command> _addr_c)
        {
            ref Command c = ref _addr_c.val;

            var name = c.LongName();
            {
                var i = strings.LastIndex(name, " ");

                if (i >= 0L)
                {
                    name = name[i + 1L..];
                }

            }

            return name;

        }

        private static void Usage(this ptr<Command> _addr_c)
        {
            ref Command c = ref _addr_c.val;

            fmt.Fprintf(os.Stderr, "usage: %s\n", c.UsageLine);
            fmt.Fprintf(os.Stderr, "Run 'go help %s' for details.\n", c.LongName());
            SetExitStatus(2L);
            Exit();
        }

        // Runnable reports whether the command can be run; otherwise
        // it is a documentation pseudo-command such as importpath.
        private static bool Runnable(this ptr<Command> _addr_c)
        {
            ref Command c = ref _addr_c.val;

            return c.Run != null;
        }

        private static slice<Action> atExitFuncs = default;

        public static void AtExit(Action f)
        {
            atExitFuncs = append(atExitFuncs, f);
        }

        public static void Exit()
        {
            foreach (var (_, f) in atExitFuncs)
            {
                f();
            }
            os.Exit(exitStatus);

        }

        public static void Fatalf(@string format, params object[] args)
        {
            args = args.Clone();

            Errorf(format, args);
            Exit();
        }

        public static void Errorf(@string format, params object[] args)
        {
            args = args.Clone();

            log.Printf(format, args);
            SetExitStatus(1L);
        }

        public static void ExitIfErrors()
        {
            if (exitStatus != 0L)
            {
                Exit();
            }

        }

        private static long exitStatus = 0L;
        private static sync.Mutex exitMu = default;

        public static void SetExitStatus(long n)
        {
            exitMu.Lock();
            if (exitStatus < n)
            {
                exitStatus = n;
            }

            exitMu.Unlock();

        }

        public static long GetExitStatus()
        {
            return exitStatus;
        }

        // Run runs the command, with stdout and stderr
        // connected to the go command's own stdout and stderr.
        // If the command fails, Run reports the error using Errorf.
        public static void Run(params object[] cmdargs)
        {
            cmdargs = cmdargs.Clone();

            var cmdline = str.StringList(cmdargs);
            if (cfg.BuildN || cfg.BuildX)
            {
                fmt.Printf("%s\n", strings.Join(cmdline, " "));
                if (cfg.BuildN)
                {
                    return ;
                }

            }

            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            {
                var err = cmd.Run();

                if (err != null)
                {
                    Errorf("%v", err);
                }

            }

        }

        // RunStdin is like run but connects Stdin.
        public static void RunStdin(slice<@string> cmdline)
        {
            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            cmd.Stdin = os.Stdin;
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr;
            cmd.Env = cfg.OrigEnv;
            StartSigHandlers();
            {
                var err = cmd.Run();

                if (err != null)
                {
                    Errorf("%v", err);
                }

            }

        }

        // Usage is the usage-reporting function, filled in by package main
        // but here for reference by other packages.
        public static Action Usage = default;
    }
}}}}
