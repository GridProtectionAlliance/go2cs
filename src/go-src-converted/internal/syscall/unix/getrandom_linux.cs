// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 08 03:32:01 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\getrandom_linux.go
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

        // GetRandomFlag is a flag supported by the getrandom system call.
        public partial struct GetRandomFlag // : System.UIntPtr
        {
        }

 
        // GRND_NONBLOCK means return EAGAIN rather than blocking.
        public static readonly GetRandomFlag GRND_NONBLOCK = (GetRandomFlag)0x0001UL; 

        // GRND_RANDOM means use the /dev/random pool instead of /dev/urandom.
        public static readonly GetRandomFlag GRND_RANDOM = (GetRandomFlag)0x0002UL;


        // GetRandom calls the Linux getrandom system call.
        // See https://git.kernel.org/cgit/linux/kernel/git/torvalds/linux.git/commit/?id=c6e9d6f38894798696f23c8084ca7edbf16ee895
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

            var (r1, _, errno) = syscall.Syscall(getrandomTrap, uintptr(@unsafe.Pointer(_addr_p[0L])), uintptr(len(p)), uintptr(flags));
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
