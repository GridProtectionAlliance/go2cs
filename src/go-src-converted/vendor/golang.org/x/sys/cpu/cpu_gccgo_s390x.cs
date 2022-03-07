// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gccgo
// +build gccgo

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_gccgo_s390x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // haveAsmFunctions reports whether the other functions in this file can
    // be safely called.
private static bool haveAsmFunctions() {
    return false;
}

// TODO(mundaym): the following feature detection functions are currently
// stubs. See https://golang.org/cl/162887 for how to fix this.
// They are likely to be expensive to call so the results should be cached.
private static facilityList stfle() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult kmQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult kmcQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult kmctrQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult kmaQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult kimdQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});
private static queryResult klmdQuery() => func((_, panic, _) => {
    panic("not implemented for gccgo");
});

} // end cpu_package
