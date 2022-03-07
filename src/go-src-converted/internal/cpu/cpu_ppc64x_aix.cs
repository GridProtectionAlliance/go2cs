// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ppc64 || ppc64le
// +build ppc64 ppc64le

// package cpu -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu_ppc64x_aix.go


namespace go.@internal;

public static partial class cpu_package {

 
// getsystemcfg constants
private static readonly nint _SC_IMPL = 2;
private static readonly nuint _IMPL_POWER9 = 0x20000;


private static void osinit() {
    var impl = getsystemcfg(_SC_IMPL);
    PPC64.IsPOWER9 = isSet(impl, _IMPL_POWER9);
}

// getsystemcfg is defined in runtime/os2_aix.go
private static nuint getsystemcfg(nuint label);

} // end cpu_package
