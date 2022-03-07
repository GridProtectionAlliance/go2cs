// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Plan 9 environment variables.

// package plan9 -- go2cs converted at 2022 March 06 23:26:23 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\env_plan9.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class plan9_package {

public static (@string, bool) Getenv(@string key) {
    @string value = default;
    bool found = default;

    return syscall.Getenv(key);
}

public static error Setenv(@string key, @string value) {
    return error.As(syscall.Setenv(key, value))!;
}

public static void Clearenv() {
    syscall.Clearenv();
}

public static slice<@string> Environ() {
    return syscall.Environ();
}

public static error Unsetenv(@string key) {
    return error.As(syscall.Unsetenv(key))!;
}

} // end plan9_package
