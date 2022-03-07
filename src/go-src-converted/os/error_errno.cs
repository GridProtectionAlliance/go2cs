// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !plan9
// +build !plan9

// package os -- go2cs converted at 2022 March 06 22:13:24 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\error_errno.go
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

private partial struct syscallErrorType { // : syscall.Errno
}

} // end os_package
