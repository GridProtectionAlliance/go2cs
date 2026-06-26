// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Windows cryptographically secure pseudorandom number
// generator.
namespace go.crypto;

using windows = @internal.syscall.windows_package;
using @internal.syscall;

partial class rand_package {

[GoInit] internal static void init() {
    ᏑReader = new rngReader(nil); Reader = ref ᏑReader.val;
}

[GoType] partial struct rngReader {
}

[GoRecv] internal static (nint, error) Read(this ref rngReader r, slice<byte> b) {
    {
        var err = windows.ProcessPrng(b); if (err != default!) {
            return (0, err);
        }
    }
    return (len(b), default!);
}

} // end rand_package
