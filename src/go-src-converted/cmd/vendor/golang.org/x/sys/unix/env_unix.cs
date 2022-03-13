// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris zos

// Unix environment variables.

// package unix -- go2cs converted at 2022 March 13 06:41:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\env_unix.go
namespace go.cmd.vendor.golang.org.x.sys;

using syscall = syscall_package;

public static partial class unix_package {

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

} // end unix_package
