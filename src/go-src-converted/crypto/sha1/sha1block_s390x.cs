// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sha1 -- go2cs converted at 2020 August 29 08:31:01 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1block_s390x.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        // featureCheck reports whether the CPU supports the
        // SHA-1 compute intermediate message digest (KIMD)
        // function code.
        private static bool featureCheck()
;

        private static var useAsm = featureCheck();
    }
}}
