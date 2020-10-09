// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux,!appengine netbsd openbsd

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
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ssh\terminal\util.go
// import "golang.org/x/crypto/ssh/terminal"

using unix = go.golang.org.x.sys.unix_package;
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
        // State contains the state of a terminal.
        public partial struct State
        {
            public unix.Termios termios;
        }

        // IsTerminal returns whether the given file descriptor is a terminal.
        public static bool IsTerminal(long fd)
        {
            var (_, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
            return err == null;
        }

        // MakeRaw put the terminal connected to the given file descriptor into raw
        // mode and returns the previous state of the terminal so that it can be
        // restored.
        public static (ptr<State>, error) MakeRaw(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ref State oldState = ref heap(new State(termios:*termios), out ptr<State> _addr_oldState); 

            // This attempts to replicate the behaviour documented for cfmakeraw in
            // the termios(3) manpage.
            termios.Iflag &= unix.IGNBRK | unix.BRKINT | unix.PARMRK | unix.ISTRIP | unix.INLCR | unix.IGNCR | unix.ICRNL | unix.IXON;
            termios.Oflag &= unix.OPOST;
            termios.Lflag &= unix.ECHO | unix.ECHONL | unix.ICANON | unix.ISIG | unix.IEXTEN;
            termios.Cflag &= unix.CSIZE | unix.PARENB;
            termios.Cflag |= unix.CS8;
            termios.Cc[unix.VMIN] = 1L;
            termios.Cc[unix.VTIME] = 0L;
            {
                var err = unix.IoctlSetTermios(fd, ioctlWriteTermios, termios);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


            return (_addr__addr_oldState!, error.As(null!)!);

        }

        // GetState returns the current state of a terminal which may be useful to
        // restore the terminal after a signal.
        public static (ptr<State>, error) GetState(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new State(termios:*termios)), error.As(null!)!);

        }

        // Restore restores the terminal connected to the given file descriptor to a
        // previous state.
        public static error Restore(long fd, ptr<State> _addr_state)
        {
            ref State state = ref _addr_state.val;

            return error.As(unix.IoctlSetTermios(fd, ioctlWriteTermios, _addr_state.termios))!;
        }

        // GetSize returns the dimensions of the given terminal.
        public static (long, long, error) GetSize(long fd)
        {
            long width = default;
            long height = default;
            error err = default!;

            var (ws, err) = unix.IoctlGetWinsize(fd, unix.TIOCGWINSZ);
            if (err != null)
            {
                return (-1L, -1L, error.As(err)!);
            }

            return (int(ws.Col), int(ws.Row), error.As(null!)!);

        }

        // passwordReader is an io.Reader that reads from a specific file descriptor.
        private partial struct passwordReader // : long
        {
        }

        private static (long, error) Read(this passwordReader r, slice<byte> buf)
        {
            long _p0 = default;
            error _p0 = default!;

            return unix.Read(int(r), buf);
        }

        // ReadPassword reads a line of input from a terminal without local echo.  This
        // is commonly used for inputting passwords and other sensitive data. The slice
        // returned does not include the \n.
        public static (slice<byte>, error) ReadPassword(long fd) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            ref var newState = ref heap(termios.val, out ptr<var> _addr_newState);
            newState.Lflag &= unix.ECHO;
            newState.Lflag |= unix.ICANON | unix.ISIG;
            newState.Iflag |= unix.ICRNL;
            {
                var err = unix.IoctlSetTermios(fd, ioctlWriteTermios, _addr_newState);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            defer(unix.IoctlSetTermios(fd, ioctlWriteTermios, termios));

            return readPasswordLine(passwordReader(fd));

        });
    }
}}}}}}}
