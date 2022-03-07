// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Functions to access/create device major and minor numbers matching the
// encoding used in Darwin's sys/types.h header.

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_darwin.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Major returns the major component of a Darwin device number.
public static uint Major(ulong dev) {
    return uint32((dev >> 24) & 0xff);
}

// Minor returns the minor component of a Darwin device number.
public static uint Minor(ulong dev) {
    return uint32(dev & 0xffffff);
}

// Mkdev returns a Darwin device number generated from the given major and minor
// components.
public static ulong Mkdev(uint major, uint minor) {
    return (uint64(major) << 24) | uint64(minor);
}

} // end unix_package
