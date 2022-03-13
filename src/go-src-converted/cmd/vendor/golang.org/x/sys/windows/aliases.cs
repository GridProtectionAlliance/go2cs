// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows
// +build go1.9

// package windows -- go2cs converted at 2022 March 13 06:41:28 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\aliases.go
namespace go.cmd.vendor.golang.org.x.sys;

using syscall = syscall_package;

public static partial class windows_package {

public partial struct Errno { // : syscall.Errno
}
public partial struct SysProcAttr { // : syscall.SysProcAttr
}

} // end windows_package
