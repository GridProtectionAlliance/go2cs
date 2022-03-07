// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha256 -- go2cs converted at 2022 March 06 22:19:28 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Program Files\Go\src\crypto\sha256\sha256block_s390x.go
using cpu = go.@internal.cpu_package;

namespace go.crypto;

public static partial class sha256_package {

private static var useAsm = cpu.S390X.HasSHA256;

} // end sha256_package
