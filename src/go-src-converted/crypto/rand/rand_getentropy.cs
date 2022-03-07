// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (darwin && !ios) || openbsd
// +build darwin,!ios openbsd

// package rand -- go2cs converted at 2022 March 06 22:17:18 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand_getentropy.go
using unix = go.@internal.syscall.unix_package;

namespace go.crypto;

public static partial class rand_package {

private static void init() {
    altGetRandom = getEntropy;
}

private static bool getEntropy(slice<byte> p) {
    bool ok = default;
 
    // getentropy(2) returns a maximum of 256 bytes per call
    {
        nint i = 0;

        while (i < len(p)) {
            var end = i + 256;
            if (len(p) < end) {
                end = len(p);
            i += 256;
            }

            var err = unix.GetEntropy(p[(int)i..(int)end]);
            if (err != null) {
                return false;
            }

        }
    }
    return true;

}

} // end rand_package
