// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !freebsd && !openbsd && !solaris && !windows && !(linux && amd64)
// +build !aix
// +build !darwin
// +build !freebsd
// +build !openbsd
// +build !solaris
// +build !windows
// +build !linux !amd64

// package runtime -- go2cs converted at 2022 March 06 22:12:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\timestub2.go


namespace go;

public static partial class runtime_package {

private static (long, int) walltime();

} // end runtime_package
