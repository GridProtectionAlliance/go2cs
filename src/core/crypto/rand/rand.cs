// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rand implements a cryptographically secure
// random number generator.
namespace go.crypto;

using io = io_package;

partial class rand_package {

// Reader is a global, shared instance of a cryptographically
// secure random number generator.
//
//   - On Linux, FreeBSD, Dragonfly, and Solaris, Reader uses getrandom(2)
//     if available, and /dev/urandom otherwise.
//   - On macOS and iOS, Reader uses arc4random_buf(3).
//   - On OpenBSD and NetBSD, Reader uses getentropy(2).
//   - On other Unix-like systems, Reader reads from /dev/urandom.
//   - On Windows, Reader uses the ProcessPrng API.
//   - On js/wasm, Reader uses the Web Crypto API.
//   - On wasip1/wasm, Reader uses random_get from wasi_snapshot_preview1.
public static io.Reader Reader;

// Read is a helper function that calls Reader.Read using io.ReadFull.
// On return, n == len(b) if and only if err == nil.
public static (nint n, error err) Read(slice<byte> b) {
    nint n = default!;
    error err = default!;

    return io.ReadFull(Reader, b);
}

// batched returns a function that calls f to populate a []byte by chunking it
// into subslices of, at most, readMax bytes.
internal static Func<slice<byte>, error> batched(Func<slice<byte>, error> f, nint readMax) {
    return (slice<byte> @out) => {
        while (len(@out) > 0) {
            nint read = len(@out);
            if (read > readMax) {
                read = readMax;
            }
            {
                var err = f(@out[..(int)(read)]); if (err != default!) {
                    return err;
                }
            }
            @out = @out[(int)(read)..];
        }
        return default!;
    };
}

} // end rand_package
