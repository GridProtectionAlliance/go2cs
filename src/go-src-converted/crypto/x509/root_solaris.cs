// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 06 22:19:51 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\root_solaris.go


namespace go.crypto;

public static partial class x509_package {

    // Possible certificate files; stop after finding one.
private static @string certFiles = new slice<@string>(new @string[] { "/etc/certs/ca-certificates.crt", "/etc/ssl/certs/ca-certificates.crt", "/etc/ssl/cacert.pem" });

// Possible directories with certificate files; stop after successfully
// reading at least one file from a directory.
private static @string certDirectories = new slice<@string>(new @string[] { "/etc/certs/CA" });

} // end x509_package
