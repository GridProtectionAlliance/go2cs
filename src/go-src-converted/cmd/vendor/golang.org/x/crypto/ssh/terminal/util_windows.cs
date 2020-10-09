// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// Package terminal provides support functions for dealing with terminals, as
// commonly found on UNIX systems.
//
// Putting a terminal into raw mode is the most common requirement:
//
//     oldState, err := terminal.MakeRaw(0)
//     if err != nil {
//             panic(err)
//     }
//     defer terminal.Restore(0, oldState)
// package terminal -- go2cs converted at 2020 October 09 05:55:49 UTC
// import "cmd/vendor/golang.org/x/crypto/ssh/terminal" ==> using terminal = go.cmd.vendor.golang.org.x.crypto.ssh.terminal_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ssh\terminal\util_windows.go
using os = go.os_package;

using windows = go.golang.org.x.sys.windows_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace ssh
{
    public static partial class terminal_package
    {
        public partial struct State
        {
            public uint mode;
        }

        // IsTerminal returns whether the given file descriptor is a terminal.
        public static bool IsTerminal(long fd)
        {
            ref uint st = ref heap(out ptr<uint> _addr_st);
            var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);
            return err == null;
        }

        // MakeRaw put the terminal connected to the given file descriptor into raw
        // mode and returns the previous state of the terminal so that it can be
        // restored.
        public static (ptr<State>, error) MakeRaw(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            ref uint st = ref heap(out ptr<uint> _addr_st);
            {
                var err__prev1 = err;

                var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }

            var raw = st & ~(windows.ENABLE_ECHO_INPUT | windows.ENABLE_PROCESSED_INPUT | windows.ENABLE_LINE_INPUT | windows.ENABLE_PROCESSED_OUTPUT);
            {
                var err__prev1 = err;

                err = windows.SetConsoleMode(windows.Handle(fd), raw);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }

            return (addr(new State(st)), error.As(null!)!);

        }

        // GetState returns the current state of a terminal which may be useful to
        // restore the terminal after a signal.
        public static (ptr<State>, error) GetState(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            ref uint st = ref heap(out ptr<uint> _addr_st);
            {
                var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            return (addr(new State(st)), error.As(null!)!);

        }

        // Restore restores the terminal connected to the given file descriptor to a
        // previous state.
        public static error Restore(long fd, ptr<State> _addr_state)
        {
            ref State state = ref _addr_state.val;

            return error.As(windows.SetConsoleMode(windows.Handle(fd), state.mode))!;
        }

        // GetSize returns the visible dimensions of the given terminal.
        //
        // These dimensions don't include any scrollback buffer height.
        public static (long, long, error) GetSize(long fd)
        {
            long width = default;
            long height = default;
            error err = default!;

            ref windows.ConsoleScreenBufferInfo info = ref heap(out ptr<windows.ConsoleScreenBufferInfo> _addr_info);
            {
                var err = windows.GetConsoleScreenBufferInfo(windows.Handle(fd), _addr_info);

                if (err != null)
                {
                    return (0L, 0L, error.As(err)!);
                }

            }

            return (int(info.Window.Right - info.Window.Left + 1L), int(info.Window.Bottom - info.Window.Top + 1L), error.As(null!)!);

        }

        // ReadPassword reads a line of input from a terminal without local echo.  This
        // is commonly used for inputting passwords and other sensitive data. The slice
        // returned does not include the \n.
        public static (slice<byte>, error) ReadPassword(long fd) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            ref uint st = ref heap(out ptr<uint> _addr_st);
            {
                var err__prev1 = err;

                var err = windows.GetConsoleMode(windows.Handle(fd), _addr_st);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            var old = st;

            st &= (windows.ENABLE_ECHO_INPUT | windows.ENABLE_LINE_INPUT);
            st |= (windows.ENABLE_PROCESSED_OUTPUT | windows.ENABLE_PROCESSED_INPUT);
            {
                var err__prev1 = err;

                err = windows.SetConsoleMode(windows.Handle(fd), st);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }


            defer(windows.SetConsoleMode(windows.Handle(fd), old));

            ref windows.Handle h = ref heap(out ptr<windows.Handle> _addr_h);
            var (p, _) = windows.GetCurrentProcess();
            {
                var err__prev1 = err;

                err = windows.DuplicateHandle(p, windows.Handle(fd), p, _addr_h, 0L, false, windows.DUPLICATE_SAME_ACCESS);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }


            var f = os.NewFile(uintptr(h), "stdin");
            defer(f.Close());
            return readPasswordLine(f);

        });
    }
}}}}}}}
