// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix && ppc
// +build aix,ppc

// Functions to access/create device major and minor numbers matching the
// encoding used by AIX.

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_aix_ppc.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Major returns the major component of a Linux device number.
public static uint Major(ulong dev) {
    return uint32((dev >> 16) & 0xffff);
}

// Minor returns the minor component of a Linux device number.
public static uint Minor(ulong dev) {
    return uint32(dev & 0xffff);
}

// Mkdev returns a Linux device number generated from the given major and minor
// components.
public static ulong Mkdev(uint major, uint minor) {
    return uint64(((major) << 16) | (minor));
}

} // end unix_package
