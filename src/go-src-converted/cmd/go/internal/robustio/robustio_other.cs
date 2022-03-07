// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !windows && !darwin
// +build !windows,!darwin

// package robustio -- go2cs converted at 2022 March 06 23:18:39 UTC
// import "cmd/go/internal/robustio" ==> using robustio = go.cmd.go.@internal.robustio_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\robustio\robustio_other.go
using os = go.os_package;

namespace go.cmd.go.@internal;

public static partial class robustio_package {

private static error rename(@string oldpath, @string newpath) {
    return error.As(os.Rename(oldpath, newpath))!;
}

private static (slice<byte>, error) readFile(@string filename) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return os.ReadFile(filename);
}

private static error removeAll(@string path) {
    return error.As(os.RemoveAll(path))!;
}

private static bool isEphemeralError(error err) {
    return false;
}

} // end robustio_package
