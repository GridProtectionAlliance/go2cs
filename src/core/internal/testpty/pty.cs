// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package testpty is a simple pseudo-terminal package for Unix systems,
// implemented by calling C functions via cgo.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using os = os_package;

partial class testpty_package {

[GoType] partial struct PtyError {
    public @string FuncName;
    public @string ErrorString;
    public error Errno;
}

internal static ж<PtyError> ptyError(@string name, error err) {
    return Ꮡ(new PtyError(name, err.Error(), err));
}

[GoRecv] public static @string Error(this ref PtyError e) {
    return fmt.Sprintf("%s: %s"u8, e.FuncName, e.ErrorString);
}

[GoRecv] public static error Unwrap(this ref PtyError e) {
    return e.Errno;
}

public static error ErrNotSupported = errors.New("testpty.Open not implemented on this platform"u8);

// Open returns a control pty and the name of the linked process tty.
//
// If Open is not implemented on this platform, it returns ErrNotSupported.
public static (ж<os.File> pty, @string processTTY, error err) Open() {
    ж<os.File> pty = default!;
    @string processTTY = default!;
    error err = default!;

    return open();
}

} // end testpty_package
