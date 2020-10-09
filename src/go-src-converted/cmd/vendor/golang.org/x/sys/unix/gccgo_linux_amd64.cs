// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build gccgo,linux,amd64

// package unix -- go2cs converted at 2020 October 09 05:56:16 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\gccgo_linux_amd64.go
using syscall = go.syscall_package;
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
        //extern gettimeofday
        private static int realGettimeofday(ptr<Timeval> _p0, ptr<byte> _p0)
;

        private static syscall.Errno gettimeofday(ptr<Timeval> _addr_tv)
        {
            syscall.Errno err = default;
            ref Timeval tv = ref _addr_tv.val;

            var r = realGettimeofday(_addr_tv, _addr_null);
            if (r < 0L)
            {>>MARKER:FUNCTION_realGettimeofday_BLOCK_PREFIX<<
                return syscall.GetErrno();
            }

            return 0L;

        }
    }
}}}}}}
