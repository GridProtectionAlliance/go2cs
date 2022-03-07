// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.5

// package plan9 -- go2cs converted at 2022 March 06 23:26:24 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\pwd_go15_plan9.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class plan9_package {

private static void fixwd() {
    syscall.Fixwd();
}

public static (@string, error) Getwd() {
    @string wd = default;
    error err = default!;

    return syscall.Getwd();
}

public static error Chdir(@string path) {
    return error.As(syscall.Chdir(path))!;
}

} // end plan9_package
