// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 October 08 03:33:36 UTC
// import "golang.org/x/net/lif" ==> using lif = go.golang.org.x.net.lif_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\lif\syscall.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        //go:cgo_import_dynamic libc_ioctl ioctl "libc.so"

        //go:linkname procIoctl libc_ioctl
        private static System.UIntPtr procIoctl = default;

        private static (System.UIntPtr, System.UIntPtr, syscall.Errno) sysvicall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        private static error ioctl(System.UIntPtr s, System.UIntPtr ioc, unsafe.Pointer arg)
        {
            var (_, _, errno) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procIoctl)), 3L, s, ioc, uintptr(arg), 0L, 0L, 0L);
            if (errno != 0L)
            {>>MARKER:FUNCTION_sysvicall6_BLOCK_PREFIX<<
                return error.As(error(errno))!;
            }

            return error.As(null!)!;

        }
    }
}}}}
