// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:31:44 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root.go
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static sync.Once once = default;        private static ref CertPool systemRoots = default;        private static error systemRootsErr = default;

        private static ref CertPool systemRootsPool()
        {
            once.Do(initSystemRoots);
            return systemRoots;
        }

        private static void initSystemRoots()
        {
            systemRoots, systemRootsErr = loadSystemRoots();
        }
    }
}}
