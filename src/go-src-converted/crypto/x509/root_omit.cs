// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,arm64 darwin,amd64,ios
// +build x509omitbundledroots

// This file provides the loadSystemRoots func when the
// "x509omitbundledroots" build tag has disabled bundling a copy,
// which currently on happens on darwin/arm64 (root_darwin_arm64.go).
// This then saves 256 KiB of binary size and another 560 KiB of
// runtime memory size retaining the parsed roots forever. Constrained
// environments can construct minimal x509 root CertPools on the fly
// in the crypto/tls.Config.VerifyPeerCertificate hook.

// package x509 -- go2cs converted at 2020 October 09 04:54:54 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_omit.go
using errors = go.errors_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static (ptr<CertPool>, error) loadSystemRoots()
        {
            ptr<CertPool> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(errors.New("x509: system root bundling disabled"))!);
        }

        private static (slice<slice<ptr<Certificate>>>, error) systemVerify(this ptr<Certificate> _addr_c, ptr<VerifyOptions> _addr_opts)
        {
            slice<slice<ptr<Certificate>>> chains = default;
            error err = default!;
            ref Certificate c = ref _addr_c.val;
            ref VerifyOptions opts = ref _addr_opts.val;

            return (null, error.As(null!)!);
        }

        // loadSystemRootsWithCgo is not available on iOS.
        private static Func<(ptr<CertPool>, error)> loadSystemRootsWithCgo = default;
    }
}}
