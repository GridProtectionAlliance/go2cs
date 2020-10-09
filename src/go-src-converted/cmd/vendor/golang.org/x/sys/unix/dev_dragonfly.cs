// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Functions to access/create device major and minor numbers matching the
// encoding used in Dragonfly's sys/types.h header.
//
// The information below is extracted and adapted from sys/types.h:
//
// Minor gives a cookie instead of an index since in order to avoid changing the
// meanings of bits 0-15 or wasting time and space shifting bits 16-31 for
// devices that don't use them.

// package unix -- go2cs converted at 2020 October 09 05:56:12 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_dragonfly.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // Major returns the major component of a DragonFlyBSD device number.
        public static uint Major(ulong dev)
        {
            return uint32((dev >> (int)(8L)) & 0xffUL);
        }

        // Minor returns the minor component of a DragonFlyBSD device number.
        public static uint Minor(ulong dev)
        {
            return uint32(dev & 0xffff00ffUL);
        }

        // Mkdev returns a DragonFlyBSD device number generated from the given major and
        // minor components.
        public static ulong Mkdev(uint major, uint minor)
        {
            return (uint64(major) << (int)(8L)) | uint64(minor);
        }
    }
}}}}}}
