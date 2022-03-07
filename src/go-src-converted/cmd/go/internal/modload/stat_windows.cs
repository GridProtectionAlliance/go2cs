// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build windows
// +build windows

// package modload -- go2cs converted at 2022 March 06 23:18:31 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\stat_windows.go
using fs = go.io.fs_package;

namespace go.cmd.go.@internal;

public static partial class modload_package {

    // hasWritePerm reports whether the current user has permission to write to the
    // file with the given info.
private static bool hasWritePerm(@string _, fs.FileInfo fi) { 
    // Windows has a read-only attribute independent of ACLs, so use that to
    // determine whether the file is intended to be overwritten.
    //
    // Per https://golang.org/pkg/os/#Chmod:
    // “On Windows, only the 0200 bit (owner writable) of mode is used; it
    // controls whether the file's read-only attribute is set or cleared.”
    return fi.Mode() & 0200 != 0;

}

} // end modload_package
