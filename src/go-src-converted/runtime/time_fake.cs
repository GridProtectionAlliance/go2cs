// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build faketime
// +build !windows

// Faketime isn't currently supported on Windows. This would require:
//
// 1. Shadowing time_now, which is implemented in assembly on Windows.
//    Since that's exported directly to the time package from runtime
//    assembly, this would involve moving it from sys_windows_*.s into
//    its own assembly files build-tagged with !faketime and using the
//    implementation of time_now from timestub.go in faketime mode.
//
// 2. Modifying syscall.Write to call syscall.faketimeWrite,
//    translating the Stdout and Stderr handles into FDs 1 and 2.
//    (See CL 192739 PS 3.)

// package runtime -- go2cs converted at 2020 October 08 03:24:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\time_fake.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // faketime is the simulated time in nanoseconds since 1970 for the
        // playground.
        private static long faketime = 1257894000000000000L;

        private static var faketimeState = default;

        //go:nosplit
        private static long nanotime()
        {
            return faketime;
        }

        private static (long, int) walltime()
        {
            long sec = default;
            int nsec = default;

            return (faketime / 1000000000L, int32(faketime % 1000000000L));
        }

        private static int write(System.UIntPtr fd, unsafe.Pointer p, int n)
        {
            if (!(fd == 1L || fd == 2L))
            { 
                // Do an ordinary write.
                return write1(fd, p, n);

            } 

            // Write with the playback header.

            // First, lock to avoid interleaving writes.
            lock(_addr_faketimeState.@lock); 

            // If the current fd doesn't match the fd of the previous write,
            // ensure that the timestamp is strictly greater. That way, we can
            // recover the original order even if we read the fds separately.
            var t = faketimeState.lastfaketime;
            if (fd != faketimeState.lastfd)
            {
                t++;
                faketimeState.lastfd = fd;
            }

            if (faketime > t)
            {
                t = faketime;
            }

            faketimeState.lastfaketime = t; 

            // Playback header: 0 0 P B <8-byte time> <4-byte data length> (big endian)
            array<byte> buf = new array<byte>(4L + 8L + 4L);
            buf[2L] = 'P';
            buf[3L] = 'B';
            var tu = uint64(t);
            buf[4L] = byte(tu >> (int)((7L * 8L)));
            buf[5L] = byte(tu >> (int)((6L * 8L)));
            buf[6L] = byte(tu >> (int)((5L * 8L)));
            buf[7L] = byte(tu >> (int)((4L * 8L)));
            buf[8L] = byte(tu >> (int)((3L * 8L)));
            buf[9L] = byte(tu >> (int)((2L * 8L)));
            buf[10L] = byte(tu >> (int)((1L * 8L)));
            buf[11L] = byte(tu >> (int)((0L * 8L)));
            var nu = uint32(n);
            buf[12L] = byte(nu >> (int)((3L * 8L)));
            buf[13L] = byte(nu >> (int)((2L * 8L)));
            buf[14L] = byte(nu >> (int)((1L * 8L)));
            buf[15L] = byte(nu >> (int)((0L * 8L)));
            write1(fd, @unsafe.Pointer(_addr_buf[0L]), int32(len(buf))); 

            // Write actual data.
            var res = write1(fd, p, n);

            unlock(_addr_faketimeState.@lock);
            return res;

        }
    }
}
