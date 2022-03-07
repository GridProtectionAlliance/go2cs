// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2022 March 06 22:19:26 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Program Files\Go\src\crypto\sha1\sha1block_s390x.go
using cpu = go.@internal.cpu_package;

namespace go.crypto;

public static partial class sha1_package {

private static var useAsm = cpu.S390X.HasSHA1;

} // end sha1_package
