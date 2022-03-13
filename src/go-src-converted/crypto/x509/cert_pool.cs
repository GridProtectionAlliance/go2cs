// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 13 05:34:31 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\cert_pool.go
namespace go.crypto;

using bytes = bytes_package;
using sha256 = crypto.sha256_package;
using pem = encoding.pem_package;
using errors = errors_package;
using runtime = runtime_package;
using sync = sync_package;
using System;

public static partial class x509_package {

private partial struct sum224 { // : array<byte>
}

// CertPool is a set of certificates.
public partial struct CertPool {
    public map<@string, slice<nint>> byName; // cert.RawSubject => index into lazyCerts

// lazyCerts contains funcs that return a certificate,
// lazily parsing/decompressing it as needed.
    public slice<lazyCert> lazyCerts; // haveSum maps from sum224(cert.Raw) to true. It's used only
// for AddCert duplicate detection, to avoid CertPool.contains
// calls in the AddCert path (because the contains method can
// call getCert and otherwise negate savings from lazy getCert
// funcs).
    public map<sum224, bool> haveSum;
}

// lazyCert is minimal metadata about a Cert and a func to retrieve it
// in its normal expanded *Certificate form.
private partial struct lazyCert {
    public slice<byte> rawSubject; // getCert returns the certificate.
//
// It is not meant to do network operations or anything else
// where a failure is likely; the func is meant to lazily
// parse/decompress data that is already known to be good. The
// error in the signature primarily is meant for use in the
// case where a cert file existed on local disk when the program
// started up is deleted later before it's read.
    public Func<(ptr<Certificate>, error)> getCert;
}

// NewCertPool returns a new, empty CertPool.
public static ptr<CertPool> NewCertPool() {
    return addr(new CertPool(byName:make(map[string][]int),haveSum:make(map[sum224]bool),));
}

// len returns the number of certs in the set.
// A nil set is a valid empty set.
private static nint len(this ptr<CertPool> _addr_s) {
    ref CertPool s = ref _addr_s.val;

    if (s == null) {
        return 0;
    }
    return len(s.lazyCerts);
}

// cert returns cert index n in s.
private static (ptr<Certificate>, error) cert(this ptr<CertPool> _addr_s, nint n) {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;
    ref CertPool s = ref _addr_s.val;

    return _addr_s.lazyCerts[n].getCert()!;
}

private static ptr<CertPool> copy(this ptr<CertPool> _addr_s) {
    ref CertPool s = ref _addr_s.val;

    ptr<CertPool> p = addr(new CertPool(byName:make(map[string][]int,len(s.byName)),lazyCerts:make([]lazyCert,len(s.lazyCerts)),haveSum:make(map[sum224]bool,len(s.haveSum)),));
    {
        var k__prev1 = k;

        foreach (var (__k, __v) in s.byName) {
            k = __k;
            v = __v;
            var indexes = make_slice<nint>(len(v));
            copy(indexes, v);
            p.byName[k] = indexes;
        }
        k = k__prev1;
    }

    {
        var k__prev1 = k;

        foreach (var (__k) in s.haveSum) {
            k = __k;
            p.haveSum[k] = true;
        }
        k = k__prev1;
    }

    copy(p.lazyCerts, s.lazyCerts);
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
public static (ptr<CertPool>, error) SystemCertPool() {
    ptr<CertPool> _p0 = default!;
    error _p0 = default!;

    if (runtime.GOOS == "windows") { 
        // Issue 16736, 18609:
        return (_addr_null!, error.As(errors.New("crypto/x509: system root pool is not available on Windows"))!);
    }
    {
        var sysRoots = systemRootsPool();

        if (sysRoots != null) {
            return (_addr_sysRoots.copy()!, error.As(null!)!);
        }
    }

    return _addr_loadSystemRoots()!;
}

// findPotentialParents returns the indexes of certificates in s which might
// have signed cert.
private static slice<ptr<Certificate>> findPotentialParents(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert) {
    ref CertPool s = ref _addr_s.val;
    ref Certificate cert = ref _addr_cert.val;

    if (s == null) {
        return null;
    }
    slice<ptr<Certificate>> matchingKeyID = default;    slice<ptr<Certificate>> oneKeyID = default;    slice<ptr<Certificate>> mismatchKeyID = default;

    foreach (var (_, c) in s.byName[string(cert.RawIssuer)]) {
        var (candidate, err) = s.cert(c);
        if (err != null) {
            continue;
        }
        var kidMatch = bytes.Equal(candidate.SubjectKeyId, cert.AuthorityKeyId);

        if (kidMatch) 
            matchingKeyID = append(matchingKeyID, candidate);
        else if ((len(candidate.SubjectKeyId) == 0 && len(cert.AuthorityKeyId) > 0) || (len(candidate.SubjectKeyId) > 0 && len(cert.AuthorityKeyId) == 0)) 
            oneKeyID = append(oneKeyID, candidate);
        else 
            mismatchKeyID = append(mismatchKeyID, candidate);
            }    var found = len(matchingKeyID) + len(oneKeyID) + len(mismatchKeyID);
    if (found == 0) {
        return null;
    }
    var candidates = make_slice<ptr<Certificate>>(0, found);
    candidates = append(candidates, matchingKeyID);
    candidates = append(candidates, oneKeyID);
    candidates = append(candidates, mismatchKeyID);
    return candidates;
}

private static bool contains(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert) {
    ref CertPool s = ref _addr_s.val;
    ref Certificate cert = ref _addr_cert.val;

    if (s == null) {
        return false;
    }
    return s.haveSum[sha256.Sum224(cert.Raw)];
}

// AddCert adds a certificate to a pool.
private static void AddCert(this ptr<CertPool> _addr_s, ptr<Certificate> _addr_cert) => func((_, panic, _) => {
    ref CertPool s = ref _addr_s.val;
    ref Certificate cert = ref _addr_cert.val;

    if (cert == null) {
        panic("adding nil Certificate to CertPool");
    }
    s.addCertFunc(sha256.Sum224(cert.Raw), string(cert.RawSubject), () => (cert, null));
});

// addCertFunc adds metadata about a certificate to a pool, along with
// a func to fetch that certificate later when needed.
//
// The rawSubject is Certificate.RawSubject and must be non-empty.
// The getCert func may be called 0 or more times.
private static (ptr<Certificate>, error) addCertFunc(this ptr<CertPool> _addr_s, sum224 rawSum224, @string rawSubject, Func<(ptr<Certificate>, error)> getCert) => func((_, panic, _) => {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;
    ref CertPool s = ref _addr_s.val;

    if (getCert == null) {
        panic("getCert can't be nil");
    }
    if (s.haveSum[rawSum224]) {
        return ;
    }
    s.haveSum[rawSum224] = true;
    s.lazyCerts = append(s.lazyCerts, new lazyCert(rawSubject:[]byte(rawSubject),getCert:getCert,));
    s.byName[rawSubject] = append(s.byName[rawSubject], len(s.lazyCerts) - 1);
});

// AppendCertsFromPEM attempts to parse a series of PEM encoded certificates.
// It appends any certificates found to s and reports whether any certificates
// were successfully parsed.
//
// On many Linux systems, /etc/ssl/cert.pem will contain the system wide set
// of root CAs in a format suitable for this function.
private static bool AppendCertsFromPEM(this ptr<CertPool> _addr_s, slice<byte> pemCerts) {
    bool ok = default;
    ref CertPool s = ref _addr_s.val;

    while (len(pemCerts) > 0) {
        ptr<pem.Block> block;
        block, pemCerts = pem.Decode(pemCerts);
        if (block == null) {
            break;
        }
        if (block.Type != "CERTIFICATE" || len(block.Headers) != 0) {
            continue;
        }
        var certBytes = block.Bytes;
        var (cert, err) = ParseCertificate(certBytes);
        if (err != null) {
            continue;
        }
        var lazyCert = default;
        s.addCertFunc(sha256.Sum224(cert.Raw), string(cert.RawSubject), () => {
            lazyCert.Do(() => { 
                // This can't fail, as the same bytes already parsed above.
                lazyCert.v, _ = ParseCertificate(certBytes);
                certBytes = null;
            });
            return (lazyCert.v, null);
        });
        ok = true;
    }

    return ok;
}

// Subjects returns a list of the DER-encoded subjects of
// all of the certificates in the pool.
private static slice<slice<byte>> Subjects(this ptr<CertPool> _addr_s) {
    ref CertPool s = ref _addr_s.val;

    var res = make_slice<slice<byte>>(s.len());
    foreach (var (i, lc) in s.lazyCerts) {
        res[i] = lc.rawSubject;
    }    return res;
}

} // end x509_package
