// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Functions to access/create device major and minor numbers matching the
// encoding used in NetBSD's sys/types.h header.

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_netbsd.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Major returns the major component of a NetBSD device number.
public static uint Major(ulong dev) {
    return uint32((dev & 0x000fff00) >> 8);
}

// Minor returns the minor component of a NetBSD device number.
public static uint Minor(ulong dev) {
    var minor = uint32((dev & 0x000000ff) >> 0);
    minor |= uint32((dev & 0xfff00000) >> 12);
    return minor;
}

// Mkdev returns a NetBSD device number generated from the given major and minor
// components.
public static ulong Mkdev(uint major, uint minor) {
    var dev = (uint64(major) << 8) & 0x000fff00;
    dev |= (uint64(minor) << 12) & 0xfff00000;
    dev |= (uint64(minor) << 0) & 0x000000ff;
    return dev;
}

} // end unix_package
