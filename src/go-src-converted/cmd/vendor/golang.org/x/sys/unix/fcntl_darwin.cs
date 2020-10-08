// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 04:46:18 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\fcntl_darwin.go
using @unsafe = go.@unsafe_package;
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
        // FcntlInt performs a fcntl syscall on fd with the provided command and argument.
        public static (long, error) FcntlInt(System.UIntPtr fd, long cmd, long arg)
        {
            long _p0 = default;
            error _p0 = default!;

            return fcntl(int(fd), cmd, arg);
        }

        // FcntlFlock performs a fcntl syscall for the F_GETLK, F_SETLK or F_SETLKW command.
        public static error FcntlFlock(System.UIntPtr fd, long cmd, ptr<Flock_t> _addr_lk)
        {
            ref Flock_t lk = ref _addr_lk.val;

            var (_, err) = fcntl(int(fd), cmd, int(uintptr(@unsafe.Pointer(lk))));
            return error.As(err)!;
        }
    }
}}}}}}
