// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha256 -- go2cs converted at 2020 August 29 08:31:02 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Go\src\crypto\sha256\sha256block_s390x.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha256_package
    {
        // featureCheck reports whether the CPU supports the
        // SHA256 compute intermediate message digest (KIMD)
        // function code.
        private static bool featureCheck()
;

        private static var useAsm = featureCheck();
    }
}}
