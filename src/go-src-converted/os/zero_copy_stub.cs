// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !linux
namespace go;

using Δio = io_package;

partial class os_package {

[GoRecv] internal static (int64 written, bool handled, error err) writeTo(this ref File f, Δio.Writer w) {
    int64 written = default!;
    bool handled = default!;
    error err = default!;

    return (0, false, default!);
}

[GoRecv] internal static (int64 n, bool handled, error err) readFrom(this ref File f, Δio.Reader r) {
    int64 n = default!;
    bool handled = default!;
    error err = default!;

    return (0, false, default!);
}

} // end os_package
