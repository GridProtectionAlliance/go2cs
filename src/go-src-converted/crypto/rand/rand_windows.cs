// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows cryptographically secure pseudorandom number
// generator.

// package rand -- go2cs converted at 2022 March 06 22:17:21 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand_windows.go
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;

namespace go.crypto;

public static partial class rand_package {

private static void init() {
    Reader = addr(new rngReader());
}

private partial struct rngReader {
}

private static (nint, error) Read(this ptr<rngReader> _addr_r, slice<byte> b) {
    nint n = default;
    error err = default!;
    ref rngReader r = ref _addr_r.val;
 
    // RtlGenRandom only accepts 2**32-1 bytes at a time, so truncate.
    var inputLen = uint32(len(b));

    if (inputLen == 0) {
        return (0, error.As(null!)!);
    }
    err = windows.RtlGenRandom(b);
    if (err != null) {
        return (0, error.As(os.NewSyscallError("RtlGenRandom", err))!);
    }
    return (int(inputLen), error.As(null!)!);

}

} // end rand_package
