// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 04:50:58 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\getrandom_freebsd.go
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        private static int randomUnsupported = default; // atomic

        // FreeBSD getrandom system call number.
        private static readonly System.UIntPtr randomTrap = (System.UIntPtr)563L;

        // GetRandomFlag is a flag supported by the getrandom system call.


        // GetRandomFlag is a flag supported by the getrandom system call.
        public partial struct GetRandomFlag // : System.UIntPtr
        {
        }

 
        // GRND_NONBLOCK means return EAGAIN rather than blocking.
        public static readonly GetRandomFlag GRND_NONBLOCK = (GetRandomFlag)0x0001UL; 

        // GRND_RANDOM is only set for portability purpose, no-op on FreeBSD.
        public static readonly GetRandomFlag GRND_RANDOM = (GetRandomFlag)0x0002UL;


        // GetRandom calls the FreeBSD getrandom system call.
        public static (long, error) GetRandom(slice<byte> p, GetRandomFlag flags)
        {
            long n = default;
            error err = default!;

            if (len(p) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            if (atomic.LoadInt32(_addr_randomUnsupported) != 0L)
            {
                return (0L, error.As(syscall.ENOSYS)!);
            }

            var (r1, _, errno) = syscall.Syscall(randomTrap, uintptr(@unsafe.Pointer(_addr_p[0L])), uintptr(len(p)), uintptr(flags));
            if (errno != 0L)
            {
                if (errno == syscall.ENOSYS)
                {
                    atomic.StoreInt32(_addr_randomUnsupported, 1L);
                }

                return (0L, error.As(errno)!);

            }

            return (int(r1), error.As(null!)!);

        }
    }
}}}
