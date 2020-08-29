// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base defines shared basic pieces of the go command,
// in particular logging and the Command structure.
// package @base -- go2cs converted at 2020 August 29 10:00:28 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\base.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using scanner = go.go.scanner_package;
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
            public Action<ref Command, slice<@string>> Run; // UsageLine is the one-line usage message.
// The first word in the line is taken to be the command name.
            public @string UsageLine; // Short is the short description shown in the 'go help' output.
            public @string Short; // Long is the long message shown in the 'go help <this-command>' output.
            public @string Long; // Flag is a set of flags specific to this command.
            public flag.FlagSet Flag; // CustomFlags indicates that the command will do its own
// flag parsing.
            public bool CustomFlags;
        }

        // Commands lists the available commands and help topics.
        // The order here is the order in which they are printed by 'go help'.
        public static slice<ref Command> Commands = default;

        // Name returns the command's name: the first word in the usage line.
        private static @string Name(this ref Command c)
        {
            var name = c.UsageLine;
            var i = strings.Index(name, " ");
            if (i >= 0L)
            {
                name = name[..i];
            }
            return name;
        }

        private static void Usage(this ref Command c)
        {
            fmt.Fprintf(os.Stderr, "usage: %s\n", c.UsageLine);
            fmt.Fprintf(os.Stderr, "Run 'go help %s' for details.\n", c.Name());
            os.Exit(2L);
        }

        // Runnable reports whether the command can be run; otherwise
        // it is a documentation pseudo-command such as importpath.
        private static bool Runnable(this ref Command c)
        {
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
                    return;
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

        // ExpandScanner expands a scanner.List error into all the errors in the list.
        // The default Error method only shows the first error
        // and does not shorten paths.
        public static error ExpandScanner(error err)
        { 
            // Look for parser errors.
            {
                scanner.ErrorList (err, ok) = err._<scanner.ErrorList>();

                if (ok)
                { 
                    // Prepare error with \n before each message.
                    // When printed in something like context: %v
                    // this will put the leading file positions each on
                    // its own line. It will also show all the errors
                    // instead of just the first, as err.Error does.
                    bytes.Buffer buf = default;
                    foreach (var (_, e) in err)
                    {
                        e.Pos.Filename = ShortPath(e.Pos.Filename);
                        buf.WriteString("\n");
                        buf.WriteString(e.Error());
                    }
                    return error.As(errors.New(buf.String()));
                }

            }
            return error.As(err);
        }
    }
}}}}
