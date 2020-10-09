// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 October 09 04:54:52 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root.go
//go:generate go run root_darwin_ios_gen.go -version 55161.80.1

using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static sync.Once once = default;        private static ptr<CertPool> systemRoots;        private static error systemRootsErr = default!;

        private static ptr<CertPool> systemRootsPool()
        {
            once.Do(initSystemRoots);
            return _addr_systemRoots!;
        }

        private static void initSystemRoots()
        {
            systemRoots, systemRootsErr = loadSystemRoots();
            if (systemRootsErr != null)
            {
                systemRoots = null;
            }

        }
    }
}}
