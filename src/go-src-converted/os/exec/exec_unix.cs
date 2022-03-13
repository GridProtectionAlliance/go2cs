// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !plan9 && !windows
// +build !plan9,!windows

// package exec -- go2cs converted at 2022 March 13 05:28:32 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Program Files\Go\src\os\exec\exec_unix.go
namespace go.os;

using fs = io.fs_package;
using syscall = syscall_package;
using System;

public static partial class exec_package {

private static void init() {
    skipStdinCopyError = err => { 
        // Ignore EPIPE errors copying to stdin if the program
        // completed successfully otherwise.
        // See Issue 9173.
        ptr<fs.PathError> (pe, ok) = err._<ptr<fs.PathError>>();
        return ok && pe.Op == "write" && pe.Path == "|1" && pe.Err == syscall.EPIPE;
    };
}

} // end exec_package
