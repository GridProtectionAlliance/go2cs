// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || netbsd || openbsd
// +build dragonfly freebsd netbsd openbsd

// package x509 -- go2cs converted at 2022 March 06 22:19:49 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\root_bsd.go


namespace go.crypto;

public static partial class x509_package {

    // Possible certificate files; stop after finding one.
private static @string certFiles = new slice<@string>(new @string[] { "/usr/local/etc/ssl/cert.pem", "/etc/ssl/cert.pem", "/usr/local/share/certs/ca-root-nss.crt", "/etc/openssl/certs/ca-certificates.crt" });

// Possible directories with certificate files; stop after successfully
// reading at least one file from a directory.
private static @string certDirectories = new slice<@string>(new @string[] { "/etc/ssl/certs", "/usr/local/share/certs", "/etc/openssl/certs" });

} // end x509_package
