// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package x509 -- go2cs converted at 2020 October 08 03:36:58 UTC
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

        private static (slice<slice<ptr<Certificate>>>, error) systemVerify(this ptr<Certificate> _addr_c, ptr<VerifyOptions> _addr_opts)
        {
            slice<slice<ptr<Certificate>>> chains = default;
            error err = default!;
            ref Certificate c = ref _addr_c.val;
            ref VerifyOptions opts = ref _addr_opts.val;

            return (null, error.As(null!)!);
        }

        private static (ptr<CertPool>, error) loadSystemRoots()
        {
            ptr<CertPool> _p0 = default!;
            error _p0 = default!;

            var roots = NewCertPool();
            error bestErr = default!;
            foreach (var (_, file) in certFiles)
            {
                var (data, err) = ioutil.ReadFile(file);
                if (err == null)
                {
                    roots.AppendCertsFromPEM(data);
                    return (_addr_roots!, error.As(null!)!);
                }

                if (bestErr == null || (os.IsNotExist(bestErr) && !os.IsNotExist(err)))
                {
                    bestErr = error.As(err)!;
                }

            }
            if (bestErr == null)
            {
                return (_addr_roots!, error.As(null!)!);
            }

            return (_addr_null!, error.As(bestErr)!);

        }
    }
}}
