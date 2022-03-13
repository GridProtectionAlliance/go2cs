// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build faketime && !windows
// +build faketime,!windows

// Faketime isn't currently supported on Windows. This would require
// modifying syscall.Write to call syscall.faketimeWrite,
// translating the Stdout and Stderr handles into FDs 1 and 2.
// (See CL 192739 PS 3.)

// package runtime -- go2cs converted at 2022 March 13 05:27:19 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\time_fake.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

// faketime is the simulated time in nanoseconds since 1970 for the
// playground.
private static long faketime = (nint)1257894000000000000L;

private static var faketimeState = default;

//go:nosplit
private static long nanotime() {
    return faketime;
}

//go:linkname time_now time.now
private static (long, int, long) time_now() {
    long sec = default;
    int nsec = default;
    long mono = default;

    return (faketime / 1e9F, int32(faketime % 1e9F), faketime);
}

private static int write(System.UIntPtr fd, unsafe.Pointer p, int n) {
    if (!(fd == 1 || fd == 2)) { 
        // Do an ordinary write.
        return write1(fd, p, n);
    }
    lock(_addr_faketimeState.@lock); 

    // If the current fd doesn't match the fd of the previous write,
    // ensure that the timestamp is strictly greater. That way, we can
    // recover the original order even if we read the fds separately.
    var t = faketimeState.lastfaketime;
    if (fd != faketimeState.lastfd) {
        t++;
        faketimeState.lastfd = fd;
    }
    if (faketime > t) {
        t = faketime;
    }
    faketimeState.lastfaketime = t; 

    // Playback header: 0 0 P B <8-byte time> <4-byte data length> (big endian)
    array<byte> buf = new array<byte>(4 + 8 + 4);
    buf[2] = 'P';
    buf[3] = 'B';
    var tu = uint64(t);
    buf[4] = byte(tu >> (int)((7 * 8)));
    buf[5] = byte(tu >> (int)((6 * 8)));
    buf[6] = byte(tu >> (int)((5 * 8)));
    buf[7] = byte(tu >> (int)((4 * 8)));
    buf[8] = byte(tu >> (int)((3 * 8)));
    buf[9] = byte(tu >> (int)((2 * 8)));
    buf[10] = byte(tu >> (int)((1 * 8)));
    buf[11] = byte(tu >> (int)((0 * 8)));
    var nu = uint32(n);
    buf[12] = byte(nu >> (int)((3 * 8)));
    buf[13] = byte(nu >> (int)((2 * 8)));
    buf[14] = byte(nu >> (int)((1 * 8)));
    buf[15] = byte(nu >> (int)((0 * 8)));
    write1(fd, @unsafe.Pointer(_addr_buf[0]), int32(len(buf))); 

    // Write actual data.
    var res = write1(fd, p, n);

    unlock(_addr_faketimeState.@lock);
    return res;
}

} // end runtime_package
