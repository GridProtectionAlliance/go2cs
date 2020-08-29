// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package x509 -- go2cs converted at 2020 August 29 08:31:55 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_plan9.go
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // Possible certificate files; stop after finding one.
        private static @string certFiles = new slice<@string>(new @string[] { "/sys/lib/tls/ca.pem" });

        private static (slice<slice<ref Certificate>>, error) systemVerify(this ref Certificate c, ref VerifyOptions opts)
        {
            return (null, null);
        }

        private static (ref CertPool, error) loadSystemRoots()
        {
            var roots = NewCertPool();
            error bestErr = default;
            foreach (var (_, file) in certFiles)
            {
                var (data, err) = ioutil.ReadFile(file);
                if (err == null)
                {
                    roots.AppendCertsFromPEM(data);
                    return (roots, null);
                }
                if (bestErr == null || (os.IsNotExist(bestErr) && !os.IsNotExist(err)))
                {
                    bestErr = error.As(err);
                }
            }
            return (null, bestErr);
        }
    }
}}
