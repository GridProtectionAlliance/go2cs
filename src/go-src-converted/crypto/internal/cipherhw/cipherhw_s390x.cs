// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build s390x,!gccgo,!appengine

// package cipherhw -- go2cs converted at 2020 August 29 08:28:45 UTC
// import "crypto/internal/cipherhw" ==> using cipherhw = go.crypto.@internal.cipherhw_package
// Original source: C:\Go\src\crypto\internal\cipherhw\cipherhw_s390x.go

using static go.builtin;

namespace go {
namespace crypto {
namespace @internal
{
    public static partial class cipherhw_package
    {
        // hasHWSupport reports whether the AES-128, AES-192 and AES-256 cipher message
        // (KM) function codes are supported. Note that this function is expensive.
        // defined in asm_s390x.s
        private static bool hasHWSupport()
;

        private static var hwSupport = hasHWSupport();

        public static bool AESGCMSupport()
        {
            return hwSupport;
        }
    }
}}}
