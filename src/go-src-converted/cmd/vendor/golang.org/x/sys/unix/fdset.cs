// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2020 October 09 05:56:16 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\fdset.go

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
        // Set adds fd to the set fds.
        private static void Set(this ptr<FdSet> _addr_fds, long fd)
        {
            ref FdSet fds = ref _addr_fds.val;

            fds.Bits[fd / NFDBITS] |= (1L << (int)((uintptr(fd) % NFDBITS)));
        }

        // Clear removes fd from the set fds.
        private static void Clear(this ptr<FdSet> _addr_fds, long fd)
        {
            ref FdSet fds = ref _addr_fds.val;

            fds.Bits[fd / NFDBITS] &= (1L << (int)((uintptr(fd) % NFDBITS)));
        }

        // IsSet returns whether fd is in the set fds.
        private static bool IsSet(this ptr<FdSet> _addr_fds, long fd)
        {
            ref FdSet fds = ref _addr_fds.val;

            return fds.Bits[fd / NFDBITS] & (1L << (int)((uintptr(fd) % NFDBITS))) != 0L;
        }

        // Zero clears the set fds.
        private static void Zero(this ptr<FdSet> _addr_fds)
        {
            ref FdSet fds = ref _addr_fds.val;

            foreach (var (i) in fds.Bits)
            {
                fds.Bits[i] = 0L;
            }

        }
    }
}}}}}}
