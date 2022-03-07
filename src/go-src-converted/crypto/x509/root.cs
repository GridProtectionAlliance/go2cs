// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 06 22:19:49 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\root.go
// To update the embedded iOS root store, update the -version
// argument to the latest security_certificates version from
// https://opensource.apple.com/source/security_certificates/
// and run "go generate". See https://golang.org/issue/38843.
//go:generate go run root_ios_gen.go -version 55188.120.1.0.1

using sync = go.sync_package;

namespace go.crypto;

public static partial class x509_package {

private static sync.Once once = default;private static ptr<CertPool> systemRoots;private static error systemRootsErr = default!;

private static ptr<CertPool> systemRootsPool() {
    once.Do(initSystemRoots);
    return _addr_systemRoots!;
}

private static void initSystemRoots() {
    systemRoots, systemRootsErr = loadSystemRoots();
    if (systemRootsErr != null) {
        systemRoots = null;
    }
}

} // end x509_package
