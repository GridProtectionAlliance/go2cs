// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos

// package term -- go2cs converted at 2022 March 06 23:31:04 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term_unix.go
using unix = go.golang.org.x.sys.unix_package;

namespace go.cmd.vendor.golang.org.x;

public static partial class term_package {

private partial struct state {
    public unix.Termios termios;
}

private static bool isTerminal(nint fd) {
    var (_, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
    return err == null;
}

private static (ptr<State>, error) makeRaw(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref State oldState = ref heap(new State(state{termios:*termios}), out ptr<State> _addr_oldState); 

    // This attempts to replicate the behaviour documented for cfmakeraw in
    // the termios(3) manpage.
    termios.Iflag &= unix.IGNBRK | unix.BRKINT | unix.PARMRK | unix.ISTRIP | unix.INLCR | unix.IGNCR | unix.ICRNL | unix.IXON;
    termios.Oflag &= unix.OPOST;
    termios.Lflag &= unix.ECHO | unix.ECHONL | unix.ICANON | unix.ISIG | unix.IEXTEN;
    termios.Cflag &= unix.CSIZE | unix.PARENB;
    termios.Cflag |= unix.CS8;
    termios.Cc[unix.VMIN] = 1;
    termios.Cc[unix.VTIME] = 0;
    {
        var err = unix.IoctlSetTermios(fd, ioctlWriteTermios, termios);

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }


    return (_addr__addr_oldState!, error.As(null!)!);

}

private static (ptr<State>, error) getState(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (addr(new State(state{termios:*termios})), error.As(null!)!);

}

private static error restore(nint fd, ptr<State> _addr_state) {
    ref State state = ref _addr_state.val;

    return error.As(unix.IoctlSetTermios(fd, ioctlWriteTermios, _addr_state.termios))!;
}

private static (nint, nint, error) getSize(nint fd) {
    nint width = default;
    nint height = default;
    error err = default!;

    var (ws, err) = unix.IoctlGetWinsize(fd, unix.TIOCGWINSZ);
    if (err != null) {
        return (-1, -1, error.As(err)!);
    }
    return (int(ws.Col), int(ws.Row), error.As(null!)!);

}

// passwordReader is an io.Reader that reads from a specific file descriptor.
private partial struct passwordReader { // : nint
}

private static (nint, error) Read(this passwordReader r, slice<byte> buf) {
    nint _p0 = default;
    error _p0 = default!;

    return unix.Read(int(r), buf);
}

private static (slice<byte>, error) readPassword(nint fd) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var (termios, err) = unix.IoctlGetTermios(fd, ioctlReadTermios);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ref var newState = ref heap(termios.val, out ptr<var> _addr_newState);
    newState.Lflag &= unix.ECHO;
    newState.Lflag |= unix.ICANON | unix.ISIG;
    newState.Iflag |= unix.ICRNL;
    {
        var err = unix.IoctlSetTermios(fd, ioctlWriteTermios, _addr_newState);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }


    defer(unix.IoctlSetTermios(fd, ioctlWriteTermios, termios));

    return readPasswordLine(passwordReader(fd));

});

} // end term_package
