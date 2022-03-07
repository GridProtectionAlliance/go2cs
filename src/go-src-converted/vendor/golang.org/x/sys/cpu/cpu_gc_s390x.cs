// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc
// +build gc

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_gc_s390x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // haveAsmFunctions reports whether the other functions in this file can
    // be safely called.
private static bool haveAsmFunctions() {
    return true;
}

// The following feature detection functions are defined in cpu_s390x.s.
// They are likely to be expensive to call so the results should be cached.
private static facilityList stfle();
private static queryResult kmQuery();
private static queryResult kmcQuery();
private static queryResult kmctrQuery();
private static queryResult kmaQuery();
private static queryResult kimdQuery();
private static queryResult klmdQuery();

} // end cpu_package
