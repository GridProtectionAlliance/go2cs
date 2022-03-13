// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package term provides support functions for dealing with terminals, as
// commonly found on UNIX systems.
//
// Putting a terminal into raw mode is the most common requirement:
//
//     oldState, err := term.MakeRaw(int(os.Stdin.Fd()))
//     if err != nil {
//             panic(err)
//     }
//     defer term.Restore(int(os.Stdin.Fd()), oldState)

// package term -- go2cs converted at 2022 March 13 06:41:29 UTC
// import "cmd/vendor/golang.org/x/term" ==> using term = go.cmd.vendor.golang.org.x.term_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\term\term.go
namespace go.cmd.vendor.golang.org.x;

public static partial class term_package {

// State contains the state of a terminal.
public partial struct State {
    public ref state state => ref state_val;
}

// IsTerminal returns whether the given file descriptor is a terminal.
public static bool IsTerminal(nint fd) {
    return isTerminal(fd);
}

// MakeRaw puts the terminal connected to the given file descriptor into raw
// mode and returns the previous state of the terminal so that it can be
// restored.
public static (ptr<State>, error) MakeRaw(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    return _addr_makeRaw(fd)!;
}

// GetState returns the current state of a terminal which may be useful to
// restore the terminal after a signal.
public static (ptr<State>, error) GetState(nint fd) {
    ptr<State> _p0 = default!;
    error _p0 = default!;

    return _addr_getState(fd)!;
}

// Restore restores the terminal connected to the given file descriptor to a
// previous state.
public static error Restore(nint fd, ptr<State> _addr_oldState) {
    ref State oldState = ref _addr_oldState.val;

    return error.As(restore(fd, oldState))!;
}

// GetSize returns the visible dimensions of the given terminal.
//
// These dimensions don't include any scrollback buffer height.
public static (nint, nint, error) GetSize(nint fd) {
    nint width = default;
    nint height = default;
    error err = default!;

    return getSize(fd);
}

// ReadPassword reads a line of input from a terminal without local echo.  This
// is commonly used for inputting passwords and other sensitive data. The slice
// returned does not include the \n.
public static (slice<byte>, error) ReadPassword(nint fd) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return readPassword(fd);
}

} // end term_package
