// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// Functions to access/create device major and minor numbers matching the
// encoding used by z/OS.
//
// The information below is extracted and adapted from <sys/stat.h> macros.

// package unix -- go2cs converted at 2022 March 06 23:26:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_zos.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Major returns the major component of a z/OS device number.
public static uint Major(ulong dev) {
    return uint32((dev >> 16) & 0x0000FFFF);
}

// Minor returns the minor component of a z/OS device number.
public static uint Minor(ulong dev) {
    return uint32(dev & 0x0000FFFF);
}

// Mkdev returns a z/OS device number generated from the given major and minor
// components.
public static ulong Mkdev(uint major, uint minor) {
    return (uint64(major) << 16) | uint64(minor);
}

} // end unix_package
