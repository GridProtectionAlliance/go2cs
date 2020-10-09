// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:58 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\getentropy_openbsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        // getentropy(2)'s syscall number, from /usr/src/sys/kern/syscalls.master
        private static readonly System.UIntPtr entropyTrap = (System.UIntPtr)7L;

        // GetEntropy calls the OpenBSD getentropy system call.


        // GetEntropy calls the OpenBSD getentropy system call.
        public static error GetEntropy(slice<byte> p)
        {
            var (_, _, errno) = syscall.Syscall(entropyTrap, uintptr(@unsafe.Pointer(_addr_p[0L])), uintptr(len(p)), 0L);
            if (errno != 0L)
            {
                return error.As(errno)!;
            }

            return error.As(null!)!;

        }
    }
}}}
