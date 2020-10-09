// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

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
// package terminal -- go2cs converted at 2020 October 09 05:55:48 UTC
// import "cmd/vendor/golang.org/x/crypto/ssh/terminal" ==> using terminal = go.cmd.vendor.golang.org.x.crypto.ssh.terminal_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ssh\terminal\util_plan9.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
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
        }

        // IsTerminal returns whether the given file descriptor is a terminal.
        public static bool IsTerminal(long fd)
        {
            return false;
        }

        // MakeRaw put the terminal connected to the given file descriptor into raw
        // mode and returns the previous state of the terminal so that it can be
        // restored.
        public static (ptr<State>, error) MakeRaw(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(fmt.Errorf("terminal: MakeRaw not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
        }

        // GetState returns the current state of a terminal which may be useful to
        // restore the terminal after a signal.
        public static (ptr<State>, error) GetState(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(fmt.Errorf("terminal: GetState not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
        }

        // Restore restores the terminal connected to the given file descriptor to a
        // previous state.
        public static error Restore(long fd, ptr<State> _addr_state)
        {
            ref State state = ref _addr_state.val;

            return error.As(fmt.Errorf("terminal: Restore not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!;
        }

        // GetSize returns the dimensions of the given terminal.
        public static (long, long, error) GetSize(long fd)
        {
            long width = default;
            long height = default;
            error err = default!;

            return (0L, 0L, error.As(fmt.Errorf("terminal: GetSize not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
        }

        // ReadPassword reads a line of input from a terminal without local echo.  This
        // is commonly used for inputting passwords and other sensitive data. The slice
        // returned does not include the \n.
        public static (slice<byte>, error) ReadPassword(long fd)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            return (null, error.As(fmt.Errorf("terminal: ReadPassword not implemented on %s/%s", runtime.GOOS, runtime.GOARCH))!);
        }
    }
}}}}}}}
