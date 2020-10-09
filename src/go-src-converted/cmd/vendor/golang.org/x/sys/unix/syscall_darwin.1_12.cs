// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,go1.12,!go1.13

// package unix -- go2cs converted at 2020 October 09 05:56:26 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin.1_12.go
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
        public static (long, error) Getdirentries(long fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep)
        {
            long n = default;
            error err = default!;
            ref System.UIntPtr basep = ref _addr_basep.val;
 
            // To implement this using libSystem we'd need syscall_syscallPtr for
            // fdopendir. However, syscallPtr was only added in Go 1.13, so we fall
            // back to raw syscalls for this func on Go 1.12.
            unsafe.Pointer p = default;
            if (len(buf) > 0L)
            {
                p = @unsafe.Pointer(_addr_buf[0L]);
            }
            else
            {
                p = @unsafe.Pointer(_addr__zero);
            }
            var (r0, _, e1) = Syscall6(SYS_GETDIRENTRIES64, uintptr(fd), uintptr(p), uintptr(len(buf)), uintptr(@unsafe.Pointer(basep)), 0L, 0L);
            n = int(r0);
            if (e1 != 0L)
            {
                return (n, error.As(errnoErr(e1))!);
            }
            return (n, error.As(null!)!);

        }
    }
}}}}}}
