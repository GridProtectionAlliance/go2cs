// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !freebsd && !openbsd && !plan9 && !solaris
// +build !aix,!darwin,!freebsd,!openbsd,!plan9,!solaris

// package runtime -- go2cs converted at 2022 March 06 22:11:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs3.go


namespace go;

public static partial class runtime_package {

private static long nanotime1();

} // end runtime_package
