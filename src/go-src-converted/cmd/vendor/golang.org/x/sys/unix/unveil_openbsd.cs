// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 04:48:04 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\unveil_openbsd.go
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
        // Unveil implements the unveil syscall.
        // For more information see unveil(2).
        // Note that the special case of blocking further
        // unveil calls is handled by UnveilBlock.
        public static error Unveil(@string path, @string flags)
        {
            var (pathPtr, err) = syscall.BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err)!;
            }
            var (flagsPtr, err) = syscall.BytePtrFromString(flags);
            if (err != null)
            {
                return error.As(err)!;
            }
            var (_, _, e) = syscall.Syscall(SYS_UNVEIL, uintptr(@unsafe.Pointer(pathPtr)), uintptr(@unsafe.Pointer(flagsPtr)), 0L);
            if (e != 0L)
            {
                return error.As(e)!;
            }
            return error.As(null!)!;

        }

        // UnveilBlock blocks future unveil calls.
        // For more information see unveil(2).
        public static error UnveilBlock()
        { 
            // Both pointers must be nil.
            unsafe.Pointer pathUnsafe = default;            unsafe.Pointer flagsUnsafe = default;

            var (_, _, e) = syscall.Syscall(SYS_UNVEIL, uintptr(pathUnsafe), uintptr(flagsUnsafe), 0L);
            if (e != 0L)
            {
                return error.As(e)!;
            }

            return error.As(null!)!;

        }
    }
}}}}}}
