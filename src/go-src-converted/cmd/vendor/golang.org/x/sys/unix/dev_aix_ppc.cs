// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix
// +build ppc

// Functions to access/create device major and minor numbers matching the
// encoding used by AIX.

// package unix -- go2cs converted at 2020 October 09 05:56:12 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\dev_aix_ppc.go

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
            return uint32((dev >> (int)(16L)) & 0xffffUL);
        }

        // Minor returns the minor component of a Linux device number.
        public static uint Minor(ulong dev)
        {
            return uint32(dev & 0xffffUL);
        }

        // Mkdev returns a Linux device number generated from the given major and minor
        // components.
        public static ulong Mkdev(uint major, uint minor)
        {
            return uint64(((major) << (int)(16L)) | (minor));
        }
    }
}}}}}}
