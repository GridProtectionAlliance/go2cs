// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux,gccgo,386

// package unix -- go2cs converted at 2020 October 08 04:47:32 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux_gccgo_386.go
using syscall = go.syscall_package;
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
        private static (long, syscall.Errno) seek(long fd, long offset, long whence)
        {
            long _p0 = default;
            syscall.Errno _p0 = default;

            ref long newoffset = ref heap(out ptr<long> _addr_newoffset);
            var offsetLow = uint32(offset & 0xffffffffUL);
            var offsetHigh = uint32((offset >> (int)(32L)) & 0xffffffffUL);
            var (_, _, err) = Syscall6(SYS__LLSEEK, uintptr(fd), uintptr(offsetHigh), uintptr(offsetLow), uintptr(@unsafe.Pointer(_addr_newoffset)), uintptr(whence), 0L);
            return (newoffset, err);
        }

        private static (long, syscall.Errno) socketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            long _p0 = default;
            syscall.Errno _p0 = default;

            var (fd, _, err) = Syscall(SYS_SOCKETCALL, uintptr(call), uintptr(@unsafe.Pointer(_addr_a0)), 0L);
            return (int(fd), err);
        }

        private static (long, syscall.Errno) rawsocketcall(long call, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            long _p0 = default;
            syscall.Errno _p0 = default;

            var (fd, _, err) = RawSyscall(SYS_SOCKETCALL, uintptr(call), uintptr(@unsafe.Pointer(_addr_a0)), 0L);
            return (int(fd), err);
        }
    }
}}}}}}
