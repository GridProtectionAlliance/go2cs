// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using errors = errors_package;
using strings = strings_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class x509_package {

internal static (ж<CertPool>, error) loadSystemRoots() {
    return (Ꮡ(new CertPool(systemPool: true)), default!);
}

// Creates a new *syscall.CertContext representing the leaf certificate in an in-memory
// certificate store containing itself and all of the intermediate certificates specified
// in the opts.Intermediates CertPool.
//
// A pointer to the in-memory store is available in the returned CertContext's Store field.
// The store is automatically freed when the CertContext is freed using
// syscall.CertFreeCertificateContext.
internal static (ж<syscall.CertContext>, error) createStoreContext(ж<Certificate> Ꮡleaf, ж<VerifyOptions> Ꮡopts) => func((defer, _) => {
    ref var leaf = ref Ꮡleaf.val;
    ref var opts = ref Ꮡopts.val;

    ж<syscall.CertContext> storeCtx = default!;
    (leafCtx, err) = syscall.CertCreateCertificateContext((uint32)(syscall.X509_ASN_ENCODING | syscall.PKCS_7_ASN_ENCODING), Ꮡ(leaf.Raw, 0), ((uint32)len(leaf.Raw)));
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(syscall.CertFreeCertificateContext, leafCtx, defer);
    var (handle, err) = syscall.CertOpenStore(syscall.CERT_STORE_PROV_MEMORY, 0, 0, syscall.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 0);
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(syscall.CertCloseStore, handle, 0, defer);
    err = syscall.CertAddCertificateContextToStore(handle, leafCtx, syscall.CERT_STORE_ADD_ALWAYS, Ꮡ(storeCtx));
    if (err != default!) {
        return (default!, err);
    }
    if (opts.Intermediates != nil) {
        for (nint i = 0; i < opts.Intermediates.len(); i++) {
            (intermediate, _, errΔ1) = opts.Intermediates.cert(i);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            (ctx, err) = syscall.CertCreateCertificateContext((uint32)(syscall.X509_ASN_ENCODING | syscall.PKCS_7_ASN_ENCODING), Ꮡ((~intermediate).Raw, 0), ((uint32)len((~intermediate).Raw)));
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            err = syscall.CertAddCertificateContextToStore(handle, ctx, syscall.CERT_STORE_ADD_ALWAYS, nil);
            syscall.CertFreeCertificateContext(ctx);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    return (storeCtx, default!);
});

// extractSimpleChain extracts the final certificate chain from a CertSimpleChain.
internal static (slice<ж<Certificate>> chain, error err) extractSimpleChain(ж<ж<syscall.CertSimpleChain>> ᏑsimpleChain, nint count) {
    slice<ж<Certificate>> chain = default!;
    error err = default!;

    ref var simpleChain = ref ᏑsimpleChain.val;
    if (simpleChain == nil || count == 0) {
        return (default!, errors.New("x509: invalid simple chain"u8));
    }
    var simpleChains = @unsafe.Slice(ᏑsimpleChain, count);
    var lastChain = simpleChains[count - 1];
    var elements = @unsafe.Slice((~lastChain).Elements, (~lastChain).NumElements);
    for (nint i = 0; i < ((nint)(~lastChain).NumElements); i++) {
        // Copy the buf, since ParseCertificate does not create its own copy.
        var cert = elements[i].val.CertContext;
        var encodedCert = @unsafe.Slice((~cert).EncodedCert, (~cert).Length);
        var buf = bytes.Clone(encodedCert);
        (parsedCert, errΔ1) = ParseCertificate(buf);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        chain = append(chain, parsedCert);
    }
    return (chain, default!);
}

// checkChainTrustStatus checks the trust status of the certificate chain, translating
// any errors it finds into Go errors in the process.
internal static error checkChainTrustStatus(ж<Certificate> Ꮡc, ж<syscall.CertChainContext> ᏑchainCtx) {
    ref var c = ref Ꮡc.val;
    ref var chainCtx = ref ᏑchainCtx.val;

    if (chainCtx.TrustStatus.ErrorStatus != syscall.CERT_TRUST_NO_ERROR) {
        var status = chainCtx.TrustStatus.ErrorStatus;
        switch (status) {
        case syscall.CERT_TRUST_IS_NOT_TIME_VALID: {
            return new CertificateInvalidError(Ꮡc, Expired, "");
        }
        case syscall.CERT_TRUST_IS_NOT_VALID_FOR_USAGE: {
            return new CertificateInvalidError(Ꮡc, IncompatibleUsage, "");
        }
        default: {
            return new UnknownAuthorityError( // TODO(filippo): surface more error statuses.
Ꮡc, default!, nil);
        }}

    }
    return default!;
}

// checkChainSSLServerPolicy checks that the certificate chain in chainCtx is valid for
// use as a certificate chain for a SSL/TLS server.
internal static error checkChainSSLServerPolicy(ж<Certificate> Ꮡc, ж<syscall.CertChainContext> ᏑchainCtx, ж<VerifyOptions> Ꮡopts) {
    ref var c = ref Ꮡc.val;
    ref var chainCtx = ref ᏑchainCtx.val;
    ref var opts = ref Ꮡopts.val;

    (servernamep, err) = syscall.UTF16PtrFromString(strings.TrimSuffix(opts.DNSName, "."u8));
    if (err != default!) {
        return err;
    }
    var sslPara = Ꮡ(new syscall.SSLExtraCertChainPolicyPara(
        AuthType: syscall.AUTHTYPE_SERVER,
        ServerName: servernamep
    ));
    sslPara.val.Size = ((uint32)@unsafe.Sizeof(sslPara.val));
    var para = Ꮡ(new syscall.CertChainPolicyPara(
        ExtraPolicyPara: ((syscall.Pointer)new @unsafe.Pointer(sslPara))
    ));
    para.val.Size = ((uint32)@unsafe.Sizeof(para.val));
    ref var status = ref heap<syscall_package.CertChainPolicyStatus>(out var Ꮡstatus);
    status = new syscall.CertChainPolicyStatus(nil);
    err = syscall.CertVerifyCertificateChainPolicy(syscall.CERT_CHAIN_POLICY_SSL, ᏑchainCtx, para, Ꮡstatus);
    if (err != default!) {
        return err;
    }
    // TODO(mkrautz): use the lChainIndex and lElementIndex fields
    // of the CertChainPolicyStatus to provide proper context, instead
    // using c.
    if (status.Error != 0) {
        switch (status.Error) {
        case syscall.CERT_E_EXPIRED: {
            return new CertificateInvalidError(Ꮡc, Expired, "");
        }
        case syscall.CERT_E_CN_NO_MATCH: {
            return new HostnameError(Ꮡc, opts.DNSName);
        }
        case syscall.CERT_E_UNTRUSTEDROOT: {
            return new UnknownAuthorityError(Ꮡc, default!, nil);
        }
        default: {
            return new UnknownAuthorityError(Ꮡc, default!, nil);
        }}

    }
    return default!;
}

// windowsExtKeyUsageOIDs are the C NUL-terminated string representations of the
// OIDs for use with the Windows API.
internal static map<ExtKeyUsage, slice<byte>> windowsExtKeyUsageOIDs = new map<ExtKeyUsage, slice<byte>>(len(extKeyUsageOIDs));

[GoInit] internal static void init() {
    foreach (var (_, eku) in extKeyUsageOIDs) {
        windowsExtKeyUsageOIDs[eku.extKeyUsage] = slice<byte>(eku.oid.String() + "\x00"u8);
    }
}

internal static (slice<ж<Certificate>> chain, error err) verifyChain(ж<Certificate> Ꮡc, ж<syscall.CertChainContext> ᏑchainCtx, ж<VerifyOptions> Ꮡopts) {
    slice<ж<Certificate>> chain = default!;
    error err = default!;

    ref var c = ref Ꮡc.val;
    ref var chainCtx = ref ᏑchainCtx.val;
    ref var opts = ref Ꮡopts.val;
    err = checkChainTrustStatus(Ꮡc, ᏑchainCtx);
    if (err != default!) {
        return (default!, err);
    }
    if (opts != nil && len(opts.DNSName) > 0) {
        err = checkChainSSLServerPolicy(Ꮡc, ᏑchainCtx, Ꮡopts);
        if (err != default!) {
            return (default!, err);
        }
    }
    (chain, err) = extractSimpleChain(chainCtx.Chains, ((nint)chainCtx.ChainCount));
    if (err != default!) {
        return (default!, err);
    }
    if (len(chain) == 0) {
        return (default!, errors.New("x509: internal error: system verifier returned an empty chain"u8));
    }
    // Mitigate CVE-2020-0601, where the Windows system verifier might be
    // tricked into using custom curve parameters for a trusted root, by
    // double-checking all ECDSA signatures. If the system was tricked into
    // using spoofed parameters, the signature will be invalid for the correct
    // ones we parsed. (We don't support custom curves ourselves.)
    foreach (var (i, parent) in chain[1..]) {
        if ((~parent).PublicKeyAlgorithm != ECDSA) {
            continue;
        }
        {
            var errΔ1 = parent.CheckSignature((~chain[i]).SignatureAlgorithm,
                (~chain[i]).RawTBSCertificate, (~chain[i]).Signature); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
    }
    return (chain, default!);
}

// systemVerify is like Verify, except that it uses CryptoAPI calls
// to build certificate chains and verify them.
[GoRecv] public static (slice<slice<ж<Certificate>>> chains, error err) systemVerify(this ref Certificate c, ж<VerifyOptions> Ꮡopts) => func((defer, _) => {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;

    ref var opts = ref Ꮡopts.val;
    (storeCtx, err) = createStoreContext(c, Ꮡopts);
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(syscall.CertFreeCertificateContext, storeCtx, defer);
    var para = @new<syscall.CertChainPara>();
    para.val.Size = ((uint32)@unsafe.Sizeof(para.val));
    var keyUsages = opts.KeyUsages;
    if (len(keyUsages) == 0) {
        keyUsages = new ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
    }
    var oids = new slice<ж<byte>>(0, len(keyUsages));
    foreach (var (_, eku) in keyUsages) {
        if (eku == ExtKeyUsageAny) {
            oids = default!;
            break;
        }
        {
            var oid = windowsExtKeyUsageOIDs[eku];
            var ok = windowsExtKeyUsageOIDs[eku]; if (ok) {
                oids = append(oids, Ꮡ(oid, 0));
            }
        }
    }
    if (oids != default!){
        (~para).RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_OR;
        (~para).RequestedUsage.Usage.Length = ((uint32)len(oids));
        (~para).RequestedUsage.Usage.UsageIdentifiers = Ꮡ(oids, 0);
    } else {
        (~para).RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_AND;
        (~para).RequestedUsage.Usage.Length = 0;
        (~para).RequestedUsage.Usage.UsageIdentifiers = default!;
    }
    ж<syscall.Filetime> verifyTime = default!;
    if (opts != nil && !opts.CurrentTime.IsZero()) {
        ref var ft = ref heap<syscall_package.Filetime>(out var Ꮡft);
        ft = syscall.NsecToFiletime(opts.CurrentTime.UnixNano());
        verifyTime = Ꮡft;
    }
    // The default is to return only the highest quality chain,
    // setting this flag will add additional lower quality contexts.
    // These are returned in the LowerQualityChains field.
    static readonly UntypedInt CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS = /* 0x00000080 */ 128;
    // CertGetCertificateChain will traverse Windows's root stores in an attempt to build a verified certificate chain
    ж<syscall.CertChainContext> topCtx = default!;
    err = syscall.CertGetCertificateChain(((syscallꓸHandle)0), storeCtx, verifyTime, (~storeCtx).Store, para, CERT_CHAIN_RETURN_LOWER_QUALITY_CONTEXTS, 0, Ꮡ(topCtx));
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(syscall.CertFreeCertificateChain, topCtx, defer);
    (chain, topErr) = verifyChain(c, topCtx, Ꮡopts);
    if (topErr == default!) {
        chains = append(chains, chain);
    }
    {
        var lqCtxCount = topCtx.val.LowerQualityChainCount; if (lqCtxCount > 0) {
            var lqCtxs = @unsafe.Slice((~topCtx).LowerQualityChains, lqCtxCount);
            foreach (var (_, ctx) in lqCtxs) {
                (chainΔ1, errΔ1) = verifyChain(c, ctx, Ꮡopts);
                if (errΔ1 == default!) {
                    chains = append(chains, chainΔ1);
                }
            }
        }
    }
    if (len(chains) == 0) {
        // Return the error from the highest quality context.
        return (default!, topErr);
    }
    return (chains, default!);
});

} // end x509_package
