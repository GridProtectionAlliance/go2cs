// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build linux || freebsd || dragonfly || solaris
// +build linux freebsd dragonfly solaris

// package rand -- go2cs converted at 2022 March 13 05:30:40 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand_batched.go
namespace go.crypto;

using unix = @internal.syscall.unix_package;


// maxGetRandomRead is platform dependent.

using System;
public static partial class rand_package {

private static void init() {
    altGetRandom = batched(getRandomBatch, maxGetRandomRead);
}

// batched returns a function that calls f to populate a []byte by chunking it
// into subslices of, at most, readMax bytes.
private static Func<slice<byte>, bool> batched(Func<slice<byte>, bool> f, nint readMax) {
    return buf => {
        while (len(buf) > readMax) {
            if (!f(buf[..(int)readMax])) {
                return false;
            }
            buf = buf[(int)readMax..];
        }
        return len(buf) == 0 || f(buf);
    };
}

// If the kernel is too old to support the getrandom syscall(),
// unix.GetRandom will immediately return ENOSYS and we will then fall back to
// reading from /dev/urandom in rand_unix.go. unix.GetRandom caches the ENOSYS
// result so we only suffer the syscall overhead once in this case.
// If the kernel supports the getrandom() syscall, unix.GetRandom will block
// until the kernel has sufficient randomness (as we don't use GRND_NONBLOCK).
// In this case, unix.GetRandom will not return an error.
private static bool getRandomBatch(slice<byte> p) {
    bool ok = default;

    var (n, err) = unix.GetRandom(p, 0);
    return n == len(p) && err == null;
}

} // end rand_package
