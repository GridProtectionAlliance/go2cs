// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 October 08 03:36:45 UTC
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
            public slice<ptr<Certificate>> certs;
        }

        // NewCertPool returns a new, empty CertPool.
        public static ptr<CertPool> NewCertPool()
        {
            return addr(new CertPool(bySubjectKeyId:make(map[string][]int),byName:make(map[string][]int),));
        }

        private static ptr<CertPool> copy(this ptr<CertPool> _addr_s)
        {
            ref CertPool s = ref _addr_s.val;

            ptr<CertPool> p = addr(new CertPool(bySubjectKeyId:make(map[string][]int,len(s.bySubjectKeyId)),byName:make(map[string][]int,len(s.byName)),certs:make([]*Certificate,len(s.certs)),));
            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in s.bySubjectKeyId)
                {
                    k = __k;
                    v = __v;
                    var indexes = make_slice<long>(len(v));
                    copy(indexes, v);
                    p.bySubjectKeyId[k] = indexes;
                }

                k = k__prev1;
                v = v__prev1;
            }

            {
                var k__prev1 = k;
                var v__prev1 = v;

                foreach (var (__k, __v) in s.byName)
                {
                    k = __k;
                    v = __v;
                    indexes = make_slice<long>(len(v));
                    copy(indexes, v);
                    p.byName[k] = indexes;
                }

                k = k__prev1;
                v = v__prev1;
            }

            copy(p.certs, s.certs);
            return _addr_p!;

        }

        // SystemCertPool returns a copy of the system cert pool.
        //
        // On Unix systems other than macOS the environment variables SSL_CERT_FILE and
        // SSL_CERT_DIR can be used to override the system default locations for the SSL
        // certificate file and SSL certificate files directory, respectively. The
        // latter can be a colon-separated list.
        //
        // Any mutations to the returned pool are not written to disk and do not affect
        // any other pool returned by SystemCertPool.
        //
        // New changes in the system cert pool might not be reflected in subsequent calls.
        public static (ptr<CertPool>, error) SystemCertPool()
        {
            ptr<CertPool> _p0 = default!;
            error _p0 = default!;

            if (runtime.GOOS == "windows")
            { 
                // Issue 16736, 18609:
                return (_addr_null!, error.As(errors.New("crypto/x509: system root pool is not available on Windows"))!);

            }

            {
                var sysRoots = systemRootsPool();

                if (sysRoots != null)
                {
                    return (_addr_sysRoots.copy()!, error.As(null!)!);
                }

            }


            return _addr_loadSystemRoots()!;

        }

        // findPotentialParents returns the indexes of certificates in s which might
        // have signed cert. The caller must not modify the returned slice.
        private static slice<long> findPotentialParents(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert)
        {
            ref CertPool s = ref _addr_s.val;
            ref Certificate cert = ref _addr_cert.val;

            if (s == null)
            {
                return null;
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

            return candidates;

        }

        private static bool contains(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert)
        {
            ref CertPool s = ref _addr_s.val;
            ref Certificate cert = ref _addr_cert.val;

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
        private static void AddCert(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert) => func((_, panic, __) =>
        {
            ref CertPool s = ref _addr_s.val;
            ref Certificate cert = ref _addr_cert.val;

            if (cert == null)
            {
                panic("adding nil Certificate to CertPool");
            } 

            // Check that the certificate isn't being added twice.
            if (s.contains(cert))
            {
                return ;
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
        private static bool AppendCertsFromPEM(this ptr<CertPool> _addr_s, slice<byte> pemCerts)
        {
            bool ok = default;
            ref CertPool s = ref _addr_s.val;

            while (len(pemCerts) > 0L)
            {
                ptr<pem.Block> block;
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


            return ;

        }

        // Subjects returns a list of the DER-encoded subjects of
        // all of the certificates in the pool.
        private static slice<slice<byte>> Subjects(this ptr<CertPool> _addr_s)
        {
            ref CertPool s = ref _addr_s.val;

            var res = make_slice<slice<byte>>(len(s.certs));
            foreach (var (i, c) in s.certs)
            {
                res[i] = c.RawSubject;
            }
            return res;

        }
    }
}}
