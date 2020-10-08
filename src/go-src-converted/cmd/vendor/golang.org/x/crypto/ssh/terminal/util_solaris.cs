// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package terminal -- go2cs converted at 2020 October 08 04:45:44 UTC
// import "cmd/vendor/golang.org/x/crypto/ssh/terminal" ==> using terminal = go.cmd.vendor.golang.org.x.crypto.ssh.terminal_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ssh\terminal\util_solaris.go
// import "golang.org/x/crypto/ssh/terminal"

using unix = go.golang.org.x.sys.unix_package;
using io = go.io_package;
using syscall = go.syscall_package;
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
            var (_, err) = unix.IoctlGetTermio(fd, unix.TCGETA);
            return err == null;
        }

        // ReadPassword reads a line of input from a terminal without local echo.  This
        // is commonly used for inputting passwords and other sensitive data. The slice
        // returned does not include the \n.
        public static (slice<byte>, error) ReadPassword(long fd) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
 
            // see also: http://src.illumos.org/source/xref/illumos-gate/usr/src/lib/libast/common/uwin/getpass.c
            var (val, err) = unix.IoctlGetTermios(fd, unix.TCGETS);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            ref var oldState = ref heap(val.val, out ptr<var> _addr_oldState);

            ref var newState = ref heap(oldState, out ptr<var> _addr_newState);
            newState.Lflag &= syscall.ECHO;
            newState.Lflag |= syscall.ICANON | syscall.ISIG;
            newState.Iflag |= syscall.ICRNL;
            err = unix.IoctlSetTermios(fd, unix.TCSETS, _addr_newState);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(unix.IoctlSetTermios(fd, unix.TCSETS, _addr_oldState));

            array<byte> buf = new array<byte>(16L);
            slice<byte> ret = default;
            while (true)
            {
                var (n, err) = syscall.Read(fd, buf[..]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                if (n == 0L)
                {
                    if (len(ret) == 0L)
                    {
                        return (null, error.As(io.EOF)!);
                    }

                    break;

                }

                if (buf[n - 1L] == '\n')
                {
                    n--;
                }

                ret = append(ret, buf[..n]);
                if (n < len(buf))
                {
                    break;
                }

            }


            return (ret, error.As(null!)!);

        });

        // MakeRaw puts the terminal connected to the given file descriptor into raw
        // mode and returns the previous state of the terminal so that it can be
        // restored.
        // see http://cr.illumos.org/~webrev/andy_js/1060/
        public static (ptr<State>, error) MakeRaw(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            var (termios, err) = unix.IoctlGetTermios(fd, unix.TCGETS);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ref State oldState = ref heap(new State(termios:*termios), out ptr<State> _addr_oldState);

            termios.Iflag &= unix.IGNBRK | unix.BRKINT | unix.PARMRK | unix.ISTRIP | unix.INLCR | unix.IGNCR | unix.ICRNL | unix.IXON;
            termios.Oflag &= unix.OPOST;
            termios.Lflag &= unix.ECHO | unix.ECHONL | unix.ICANON | unix.ISIG | unix.IEXTEN;
            termios.Cflag &= unix.CSIZE | unix.PARENB;
            termios.Cflag |= unix.CS8;
            termios.Cc[unix.VMIN] = 1L;
            termios.Cc[unix.VTIME] = 0L;

            {
                var err = unix.IoctlSetTermios(fd, unix.TCSETS, termios);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


            return (_addr__addr_oldState!, error.As(null!)!);

        }

        // Restore restores the terminal connected to the given file descriptor to a
        // previous state.
        public static error Restore(long fd, ptr<State> _addr_oldState)
        {
            ref State oldState = ref _addr_oldState.val;

            return error.As(unix.IoctlSetTermios(fd, unix.TCSETS, _addr_oldState.termios))!;
        }

        // GetState returns the current state of a terminal which may be useful to
        // restore the terminal after a signal.
        public static (ptr<State>, error) GetState(long fd)
        {
            ptr<State> _p0 = default!;
            error _p0 = default!;

            var (termios, err) = unix.IoctlGetTermios(fd, unix.TCGETS);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new State(termios:*termios)), error.As(null!)!);

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
                return (0L, 0L, error.As(err)!);
            }

            return (int(ws.Col), int(ws.Row), error.As(null!)!);

        }
    }
}}}}}}}
