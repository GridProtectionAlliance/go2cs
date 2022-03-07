// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_zos.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static void archInit() {
    doinit();
    Initialized = true;
}

} // end cpu_package
