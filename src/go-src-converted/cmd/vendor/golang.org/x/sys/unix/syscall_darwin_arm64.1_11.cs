// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,arm64,!go1.12

// package unix -- go2cs converted at 2020 October 09 05:56:30 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin_arm64.1_11.go

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
        public static (long, error) Getdirentries(long fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep)
        {
            long n = default;
            error err = default!;
            ref System.UIntPtr basep = ref _addr_basep.val;

            return (0L, error.As(ENOSYS)!);
        }
    }
}}}}}}
