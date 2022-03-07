// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix
// +build aix

// package cpu -- go2cs converted at 2022 March 06 23:38:19 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_aix.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

 
// getsystemcfg constants
private static readonly nint _SC_IMPL = 2;
private static readonly nuint _IMPL_POWER8 = 0x10000;
private static readonly nuint _IMPL_POWER9 = 0x20000;


private static void archInit() {
    var impl = getsystemcfg(_SC_IMPL);
    if (impl & _IMPL_POWER8 != 0) {
        PPC64.IsPOWER8 = true;
    }
    if (impl & _IMPL_POWER9 != 0) {
        PPC64.IsPOWER8 = true;
        PPC64.IsPOWER9 = true;
    }
    Initialized = true;

}

private static ulong getsystemcfg(nint label) {
    ulong n = default;

    var (r0, _) = callgetsystemcfg(label);
    n = uint64(r0);
    return ;
}

} // end cpu_package
