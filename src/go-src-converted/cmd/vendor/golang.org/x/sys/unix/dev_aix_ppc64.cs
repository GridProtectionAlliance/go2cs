// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix
// +build ppc64

// Functions to access/create device major and minor numbers matching the
// encoding used AIX.

// package unix -- go2cs converted at 2020 October 09 05:56:12 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_aix_ppc64.go

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
        // Major returns the major component of a Linux device number.
        public static uint Major(ulong dev)
        {
            return uint32((dev & 0x3fffffff00000000UL) >> (int)(32L));
        }

        // Minor returns the minor component of a Linux device number.
        public static uint Minor(ulong dev)
        {
            return uint32((dev & 0x00000000ffffffffUL) >> (int)(0L));
        }

        // Mkdev returns a Linux device number generated from the given major and minor
        // components.
        public static ulong Mkdev(uint major, uint minor)
        {
            ulong DEVNO64 = default;
            DEVNO64 = 0x8000000000000000UL;
            return ((uint64(major) << (int)(32L)) | (uint64(minor) & 0x00000000FFFFFFFFUL) | DEVNO64);
        }
    }
}}}}}}
