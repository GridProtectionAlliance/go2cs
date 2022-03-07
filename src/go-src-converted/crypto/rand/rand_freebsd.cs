// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rand -- go2cs converted at 2022 March 06 22:17:18 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand_freebsd.go


namespace go.crypto;

public static partial class rand_package {

    // maxGetRandomRead is the maximum number of bytes to ask for in one call to the
    // getrandom() syscall. In FreeBSD at most 256 bytes will be returned per call.
private static readonly nint maxGetRandomRead = 1 << 8;


} // end rand_package
