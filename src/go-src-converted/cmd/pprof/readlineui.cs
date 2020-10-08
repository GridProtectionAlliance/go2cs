// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains a driver.UI implementation
// that provides the readline functionality if possible.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris windows
// +build !appengine
// +build !android

// package main -- go2cs converted at 2020 October 08 04:42:12 UTC
// Original source: C:\Go\src\cmd\pprof\readlineui.go
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;

using driver = go.github.com.google.pprof.driver_package;
using terminal = go.golang.org.x.crypto.ssh.terminal_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            newUI = newReadlineUI;
        }

        // readlineUI implements driver.UI interface using the
        // golang.org/x/crypto/ssh/terminal package.
        // The upstream pprof command implements the same functionality
        // using the github.com/chzyer/readline package.
        private partial struct readlineUI
        {
            public ptr<terminal.Terminal> term;
        }

        private static driver.UI newReadlineUI()
        { 
            // disable readline UI in dumb terminal. (golang.org/issue/26254)
            {
                var v = strings.ToLower(os.Getenv("TERM"));

                if (v == "" || v == "dumb")
                {
                    return null;
                } 
                // test if we can use terminal.ReadLine
                // that assumes operation in the raw mode.

            } 
            // test if we can use terminal.ReadLine
            // that assumes operation in the raw mode.
            var (oldState, err) = terminal.MakeRaw(0L);
            if (err != null)
            {
                return null;
            }

            terminal.Restore(0L, oldState);

            struct{io.Readerio.Writer} rw = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{io.Readerio.Writer}{os.Stdin,os.Stderr};
            return addr(new readlineUI(term:terminal.NewTerminal(rw,"")));

        }

        // Read returns a line of text (a command) read from the user.
        // prompt is printed before reading the command.
        private static (@string, error) ReadLine(this ptr<readlineUI> _addr_r, @string prompt) => func((defer, _, __) =>
        {
            @string _p0 = default;
            error _p0 = default!;
            ref readlineUI r = ref _addr_r.val;

            r.term.SetPrompt(prompt); 

            // skip error checking because we tested it
            // when creating this readlineUI initially.
            var (oldState, _) = terminal.MakeRaw(0L);
            defer(terminal.Restore(0L, oldState));

            var (s, err) = r.term.ReadLine();
            return (s, error.As(err)!);

        });

        // Print shows a message to the user.
        // It formats the text as fmt.Print would and adds a final \n if not already present.
        // For line-based UI, Print writes to standard error.
        // (Standard output is reserved for report data.)
        private static void Print(this ptr<readlineUI> _addr_r, params object[] args)
        {
            args = args.Clone();
            ref readlineUI r = ref _addr_r.val;

            r.print(false, args);
        }

        // PrintErr shows an error message to the user.
        // It formats the text as fmt.Print would and adds a final \n if not already present.
        // For line-based UI, PrintErr writes to standard error.
        private static void PrintErr(this ptr<readlineUI> _addr_r, params object[] args)
        {
            args = args.Clone();
            ref readlineUI r = ref _addr_r.val;

            r.print(true, args);
        }

        private static void print(this ptr<readlineUI> _addr_r, bool withColor, params object[] args)
        {
            args = args.Clone();
            ref readlineUI r = ref _addr_r.val;

            var text = fmt.Sprint(args);
            if (!strings.HasSuffix(text, "\n"))
            {
                text += "\n";
            }

            if (withColor)
            {
                text = colorize(text);
            }

            fmt.Fprint(r.term, text);

        }

        // colorize prints the msg in red using ANSI color escapes.
        private static @string colorize(@string msg)
        {
            const long red = (long)31L;

            var colorEscape = fmt.Sprintf("[0;%dm", red);
            @string colorResetEscape = "[0m";
            return colorEscape + msg + colorResetEscape;
        }

        // IsTerminal reports whether the UI is known to be tied to an
        // interactive terminal (as opposed to being redirected to a file).
        private static bool IsTerminal(this ptr<readlineUI> _addr_r)
        {
            ref readlineUI r = ref _addr_r.val;

            const long stdout = (long)1L;

            return terminal.IsTerminal(stdout);
        }

        // WantBrowser indicates whether browser should be opened with the -http option.
        private static bool WantBrowser(this ptr<readlineUI> _addr_r)
        {
            ref readlineUI r = ref _addr_r.val;

            return r.IsTerminal();
        }

        // SetAutoComplete instructs the UI to call complete(cmd) to obtain
        // the auto-completion of cmd, if the UI supports auto-completion at all.
        private static @string SetAutoComplete(this ptr<readlineUI> _addr_r, Func<@string, @string> complete)
        {
            ref readlineUI r = ref _addr_r.val;
 
            // TODO: Implement auto-completion support.
        }
    }
}
