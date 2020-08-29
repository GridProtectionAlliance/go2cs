// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rand -- go2cs converted at 2020 August 29 08:30:49 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_linux.go
using unix = go.@internal.syscall.unix_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        private static void init()
        {
            altGetRandom = batched(getRandomLinux, maxGetRandomRead);
        }

        // maxGetRandomRead is the maximum number of bytes to ask for in one call to the
        // getrandom() syscall. In linux at most 2^25-1 bytes will be returned per call.
        // From the manpage
        //
        //    *  When reading from the urandom source, a maximum of 33554431 bytes
        //       is returned by a single call to getrandom() on systems where int
        //       has a size of 32 bits.
        private static readonly long maxGetRandomRead = (1L << (int)(25L)) - 1L;

        // batched returns a function that calls f to populate a []byte by chunking it
        // into subslices of, at most, readMax bytes.


        // batched returns a function that calls f to populate a []byte by chunking it
        // into subslices of, at most, readMax bytes.
        private static Func<slice<byte>, bool> batched(Func<slice<byte>, bool> f, long readMax)
        {
            return buf =>
            {
                while (len(buf) > readMax)
                {
                    if (!f(buf[..readMax]))
                    {
                        return false;
                    }
                    buf = buf[readMax..];
                }

                return len(buf) == 0L || f(buf);
            }
;
        }

        // If the kernel is too old (before 3.17) to support the getrandom syscall(),
        // unix.GetRandom will immediately return ENOSYS and we will then fall back to
        // reading from /dev/urandom in rand_unix.go. unix.GetRandom caches the ENOSYS
        // result so we only suffer the syscall overhead once in this case.
        // If the kernel supports the getrandom() syscall, unix.GetRandom will block
        // until the kernel has sufficient randomness (as we don't use GRND_NONBLOCK).
        // In this case, unix.GetRandom will not return an error.
        private static bool getRandomLinux(slice<byte> p)
        {
            var (n, err) = unix.GetRandom(p, 0L);
            return n == len(p) && err == null;
        }
    }
}}
