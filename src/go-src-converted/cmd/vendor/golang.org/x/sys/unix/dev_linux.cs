// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Functions to access/create device major and minor numbers matching the
// encoding used by the Linux kernel and glibc.
//
// The information below is extracted and adapted from bits/sysmacros.h in the
// glibc sources:
//
// dev_t in glibc is 64-bit, with 32-bit major and minor numbers. glibc's
// default encoding is MMMM Mmmm mmmM MMmm, where M is a hex digit of the major
// number and m is a hex digit of the minor number. This is backward compatible
// with legacy systems where dev_t is 16 bits wide, encoded as MMmm. It is also
// backward compatible with the Linux kernel, which for some architectures uses
// 32-bit dev_t, encoded as mmmM MMmm.

// package unix -- go2cs converted at 2020 October 09 05:56:12 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_linux.go

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
            var major = uint32((dev & 0x00000000000fff00UL) >> (int)(8L));
            major |= uint32((dev & 0xfffff00000000000UL) >> (int)(32L));
            return major;
        }

        // Minor returns the minor component of a Linux device number.
        public static uint Minor(ulong dev)
        {
            var minor = uint32((dev & 0x00000000000000ffUL) >> (int)(0L));
            minor |= uint32((dev & 0x00000ffffff00000UL) >> (int)(12L));
            return minor;
        }

        // Mkdev returns a Linux device number generated from the given major and minor
        // components.
        public static ulong Mkdev(uint major, uint minor)
        {
            var dev = (uint64(major) & 0x00000fffUL) << (int)(8L);
            dev |= (uint64(major) & 0xfffff000UL) << (int)(32L);
            dev |= (uint64(minor) & 0x000000ffUL) << (int)(0L);
            dev |= (uint64(minor) & 0xffffff00UL) << (int)(12L);
            return dev;
        }
    }
}}}}}}
