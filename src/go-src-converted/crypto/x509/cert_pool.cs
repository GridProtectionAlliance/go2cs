// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:28:38 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\cert_pool.go
using pem = go.encoding.pem_package;
using errors = go.errors_package;
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // CertPool is a set of certificates.
        public partial struct CertPool
        {
            public map<@string, slice<long>> bySubjectKeyId;
            public map<@string, slice<long>> byName;
            public slice<ref Certificate> certs;
        }

        // NewCertPool returns a new, empty CertPool.
        public static ref CertPool NewCertPool()
        {
            return ref new CertPool(bySubjectKeyId:make(map[string][]int),byName:make(map[string][]int),);
        }

        // SystemCertPool returns a copy of the system cert pool.
        //
        // Any mutations to the returned pool are not written to disk and do
        // not affect any other pool.
        public static (ref CertPool, error) SystemCertPool()
        {
            if (runtime.GOOS == "windows")
            { 
                // Issue 16736, 18609:
                return (null, errors.New("crypto/x509: system root pool is not available on Windows"));
            }
            return loadSystemRoots();
        }

        // findVerifiedParents attempts to find certificates in s which have signed the
        // given certificate. If any candidates were rejected then errCert will be set
        // to one of them, arbitrarily, and err will contain the reason that it was
        // rejected.
        private static (slice<long>, ref Certificate, error) findVerifiedParents(this ref CertPool s, ref Certificate cert)
        {
            if (s == null)
            {
                return;
            }
            slice<long> candidates = default;

            if (len(cert.AuthorityKeyId) > 0L)
            {
                candidates = s.bySubjectKeyId[string(cert.AuthorityKeyId)];
            }
            if (len(candidates) == 0L)
            {
                candidates = s.byName[string(cert.RawIssuer)];
            }
            foreach (var (_, c) in candidates)
            {
                err = cert.CheckSignatureFrom(s.certs[c]);

                if (err == null)
                {
                    parents = append(parents, c);
                }
                else
                {
                    errCert = s.certs[c];
                }
            }
            return;
        }

        private static bool contains(this ref CertPool s, ref Certificate cert)
        {
            if (s == null)
            {
                return false;
            }
            var candidates = s.byName[string(cert.RawSubject)];
            foreach (var (_, c) in candidates)
            {
                if (s.certs[c].Equal(cert))
                {
                    return true;
                }
            }
            return false;
        }

        // AddCert adds a certificate to a pool.
        private static void AddCert(this ref CertPool _s, ref Certificate _cert) => func(_s, _cert, (ref CertPool s, ref Certificate cert, Defer _, Panic panic, Recover __) =>
        {
            if (cert == null)
            {
                panic("adding nil Certificate to CertPool");
            } 

            // Check that the certificate isn't being added twice.
            if (s.contains(cert))
            {
                return;
            }
            var n = len(s.certs);
            s.certs = append(s.certs, cert);

            if (len(cert.SubjectKeyId) > 0L)
            {
                var keyId = string(cert.SubjectKeyId);
                s.bySubjectKeyId[keyId] = append(s.bySubjectKeyId[keyId], n);
            }
            var name = string(cert.RawSubject);
            s.byName[name] = append(s.byName[name], n);
        });

        // AppendCertsFromPEM attempts to parse a series of PEM encoded certificates.
        // It appends any certificates found to s and reports whether any certificates
        // were successfully parsed.
        //
        // On many Linux systems, /etc/ssl/cert.pem will contain the system wide set
        // of root CAs in a format suitable for this function.
        private static bool AppendCertsFromPEM(this ref CertPool s, slice<byte> pemCerts)
        {
            while (len(pemCerts) > 0L)
            {
                ref pem.Block block = default;
                block, pemCerts = pem.Decode(pemCerts);
                if (block == null)
                {
                    break;
                }
                if (block.Type != "CERTIFICATE" || len(block.Headers) != 0L)
                {
                    continue;
                }
                var (cert, err) = ParseCertificate(block.Bytes);
                if (err != null)
                {
                    continue;
                }
                s.AddCert(cert);
                ok = true;
            }


            return;
        }

        // Subjects returns a list of the DER-encoded subjects of
        // all of the certificates in the pool.
        private static slice<slice<byte>> Subjects(this ref CertPool s)
        {
            var res = make_slice<slice<byte>>(len(s.certs));
            foreach (var (i, c) in s.certs)
            {
                res[i] = c.RawSubject;
            }
            return res;
        }
    }
}}
