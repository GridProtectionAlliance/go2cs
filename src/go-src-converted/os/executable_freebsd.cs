// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:25 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\executable_freebsd.go


namespace go;

public static partial class os_package {

    // From FreeBSD's <sys/sysctl.h>
private static readonly nint _CTL_KERN = 1;
private static readonly nint _KERN_PROC = 14;
private static readonly nint _KERN_PROC_PATHNAME = 12;


} // end os_package
