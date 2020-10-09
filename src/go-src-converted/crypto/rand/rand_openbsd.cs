// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rand -- go2cs converted at 2020 October 09 04:53:05 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_openbsd.go
using unix = go.@internal.syscall.unix_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        private static void init()
        {
            altGetRandom = getRandomOpenBSD;
        }

        private static bool getRandomOpenBSD(slice<byte> p)
        {
            bool ok = default;
 
            // getentropy(2) returns a maximum of 256 bytes per call
            {
                long i = 0L;

                while (i < len(p))
                {
                    var end = i + 256L;
                    if (len(p) < end)
                    {
                        end = len(p);
                    i += 256L;
                    }

                    var err = unix.GetEntropy(p[i..end]);
                    if (err != null)
                    {
                        return false;
                    }

                }

            }
            return true;

        }
    }
}}
