// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rand implements a cryptographically secure
// random number generator.

// package rand -- go2cs converted at 2022 March 13 05:30:40 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand.go
namespace go.crypto;

using io = io_package;

public static partial class rand_package {

// Reader is a global, shared instance of a cryptographically
// secure random number generator.
//
// On Linux and FreeBSD, Reader uses getrandom(2) if available, /dev/urandom otherwise.
// On OpenBSD, Reader uses getentropy(2).
// On other Unix-like systems, Reader reads from /dev/urandom.
// On Windows systems, Reader uses the RtlGenRandom API.
// On Wasm, Reader uses the Web Crypto API.
public static io.Reader Reader = default;

// Read is a helper function that calls Reader.Read using io.ReadFull.
// On return, n == len(b) if and only if err == nil.
public static (nint, error) Read(slice<byte> b) {
    nint n = default;
    error err = default!;

    return io.ReadFull(Reader, b);
}

} // end rand_package
