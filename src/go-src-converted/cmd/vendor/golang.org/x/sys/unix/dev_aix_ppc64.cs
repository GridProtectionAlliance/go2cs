// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix && ppc64
// +build aix,ppc64

// Functions to access/create device major and minor numbers matching the
// encoding used AIX.

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_aix_ppc64.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Major returns the major component of a Linux device number.
public static uint Major(ulong dev) {
    return uint32((dev & 0x3fffffff00000000) >> 32);
}

// Minor returns the minor component of a Linux device number.
public static uint Minor(ulong dev) {
    return uint32((dev & 0x00000000ffffffff) >> 0);
}

// Mkdev returns a Linux device number generated from the given major and minor
// components.
public static ulong Mkdev(uint major, uint minor) {
    ulong DEVNO64 = default;
    DEVNO64 = 0x8000000000000000;
    return ((uint64(major) << 32) | (uint64(minor) & 0x00000000FFFFFFFF) | DEVNO64);
}

} // end unix_package
