// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !ios
// +build !ios

// package x509 -- go2cs converted at 2022 March 06 22:19:50 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\root_darwin.go
using bytes = go.bytes_package;
using macOS = go.crypto.x509.@internal.macos_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;
using System;


namespace go.crypto;

public static partial class x509_package {

private static var debugDarwinRoots = strings.Contains(os.Getenv("GODEBUG"), "x509roots=1");

private static (slice<slice<ptr<Certificate>>>, error) systemVerify(this ptr<Certificate> _addr_c, ptr<VerifyOptions> _addr_opts) {
    slice<slice<ptr<Certificate>>> chains = default;
    error err = default!;
    ref Certificate c = ref _addr_c.val;
    ref VerifyOptions opts = ref _addr_opts.val;

    return (null, error.As(null!)!);
}

private static (ptr<CertPool>, error) loadSystemRoots() => func((defer, _, _) => {
    ptr<CertPool> _p0 = default!;
    error _p0 = default!;

    slice<ptr<Certificate>> trustedRoots = default;
    var untrustedRoots = make_map<@string, bool>(); 

    // macOS has three trust domains: one for CAs added by users to their
    // "login" keychain, one for CAs added by Admins to the "System" keychain,
    // and one for the CAs that ship with the OS.
    foreach (var (_, domain) in new slice<macOS.SecTrustSettingsDomain>(new macOS.SecTrustSettingsDomain[] { macOS.SecTrustSettingsDomainUser, macOS.SecTrustSettingsDomainAdmin, macOS.SecTrustSettingsDomainSystem })) {
        var (certs, err) = macOS.SecTrustSettingsCopyCertificates(domain);
        if (err == macOS.ErrNoTrustSettings) {
            continue;
        }
        else if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        defer(macOS.CFRelease(certs));

        for (nint i = 0; i < macOS.CFArrayGetCount(certs); i++) {
            var c = macOS.CFArrayGetValueAtIndex(certs, i);
            var (cert, err) = exportCertificate(c);
            if (err != null) {
                if (debugDarwinRoots) {
                    fmt.Fprintf(os.Stderr, "crypto/x509: domain %d, certificate #%d: %v\n", domain, i, err);
                }
                continue;
            }
            macOS.SecTrustSettingsResult result = default;
            if (domain == macOS.SecTrustSettingsDomainSystem) { 
                // Certs found in the system domain are always trusted. If the user
                // configures "Never Trust" on such a cert, it will also be found in the
                // admin or user domain, causing it to be added to untrustedRoots.
                result = macOS.SecTrustSettingsResultTrustRoot;

            }
            else
 {
                result, err = sslTrustSettingsResult(c);
                if (err != null) {
                    if (debugDarwinRoots) {
                        fmt.Fprintf(os.Stderr, "crypto/x509: trust settings for %v: %v\n", cert.Subject, err);
                    }
                    continue;
                }
                if (debugDarwinRoots) {
                    fmt.Fprintf(os.Stderr, "crypto/x509: trust settings for %v: %d\n", cert.Subject, result);
                }
            }


            // "Note the distinction between the results kSecTrustSettingsResultTrustRoot
            // and kSecTrustSettingsResultTrustAsRoot: The former can only be applied to
            // root (self-signed) certificates; the latter can only be applied to
            // non-root certificates."
            if (result == macOS.SecTrustSettingsResultTrustRoot) 
                if (isRootCertificate(_addr_cert)) {
                    trustedRoots = append(trustedRoots, cert);
                }
            else if (result == macOS.SecTrustSettingsResultTrustAsRoot) 
                if (!isRootCertificate(_addr_cert)) {
                    trustedRoots = append(trustedRoots, cert);
                }
            else if (result == macOS.SecTrustSettingsResultDeny) 
                // Add this certificate to untrustedRoots, which are subtracted
                // from trustedRoots, so that we don't have to evaluate policies
                // for every root in the system domain, but still apply user and
                // admin policies that override system roots.
                untrustedRoots[string(cert.Raw)] = true;
            else if (result == macOS.SecTrustSettingsResultUnspecified)             else 
                if (debugDarwinRoots) {
                    fmt.Fprintf(os.Stderr, "crypto/x509: unknown trust setting for %v: %d\n", cert.Subject, result);
                }
            
        }

    }    var pool = NewCertPool();
    {
        var cert__prev1 = cert;

        foreach (var (_, __cert) in trustedRoots) {
            cert = __cert;
            if (!untrustedRoots[string(cert.Raw)]) {
                pool.AddCert(cert);
            }
        }
        cert = cert__prev1;
    }

    return (_addr_pool!, error.As(null!)!);

});

// exportCertificate returns a *Certificate for a SecCertificateRef.
private static (ptr<Certificate>, error) exportCertificate(macOS.CFRef cert) => func((defer, _, _) => {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;

    var (data, err) = macOS.SecItemExport(cert);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(macOS.CFRelease(data));
    var der = macOS.CFDataToSlice(data);

    return _addr_ParseCertificate(der)!;

});

// isRootCertificate reports whether Subject and Issuer match.
private static bool isRootCertificate(ptr<Certificate> _addr_cert) {
    ref Certificate cert = ref _addr_cert.val;

    return bytes.Equal(cert.RawSubject, cert.RawIssuer);
}

// sslTrustSettingsResult obtains the final kSecTrustSettingsResult value for a
// certificate in the user or admin domain, combining usage constraints for the
// SSL SecTrustSettingsPolicy,
//
// It ignores SecTrustSettingsKeyUsage and kSecTrustSettingsAllowedError, and
// doesn't support kSecTrustSettingsDefaultRootCertSetting.
//
// https://developer.apple.com/documentation/security/1400261-sectrustsettingscopytrustsetting
private static (macOS.SecTrustSettingsResult, error) sslTrustSettingsResult(macOS.CFRef cert) => func((defer, _, _) => {
    macOS.SecTrustSettingsResult _p0 = default;
    error _p0 = default!;
 
    // In Apple's implementation user trust settings override admin trust settings
    // (which themselves override system trust settings). If SecTrustSettingsCopyTrustSettings
    // fails, or returns a NULL trust settings, when looking for the user trust
    // settings then fallback to checking the admin trust settings.
    //
    // See Security-59306.41.2/trust/headers/SecTrustSettings.h for a description of
    // the trust settings overrides, and SecLegacyAnchorSourceCopyUsageConstraints in
    // Security-59306.41.2/trust/trustd/SecCertificateSource.c for a concrete example
    // of how Apple applies the override in the case of NULL trust settings, or non
    // success errors.
    var (trustSettings, err) = macOS.SecTrustSettingsCopyTrustSettings(cert, macOS.SecTrustSettingsDomainUser);
    if (err != null || trustSettings == 0) {
        if (debugDarwinRoots && err != macOS.ErrNoTrustSettings) {
            fmt.Fprintf(os.Stderr, "crypto/x509: SecTrustSettingsCopyTrustSettings for SecTrustSettingsDomainUser failed: %s\n", err);
        }
        trustSettings, err = macOS.SecTrustSettingsCopyTrustSettings(cert, macOS.SecTrustSettingsDomainAdmin);

    }
    if (err != null || trustSettings == 0) { 
        // If there are neither user nor admin trust settings for a certificate returned
        // from SecTrustSettingsCopyCertificates Apple returns kSecTrustSettingsResultInvalid,
        // as this method is intended to return certificates _which have trust settings_.
        // The most likely case for this being triggered is that the existing trust settings
        // are invalid and cannot be properly parsed. In this case SecTrustSettingsCopyTrustSettings
        // returns errSecInvalidTrustSettings. The existing cgo implementation returns
        // kSecTrustSettingsResultUnspecified in this case, which mostly matches the Apple
        // implementation because we don't do anything with certificates marked with this
        // result.
        //
        // See SecPVCGetTrustSettingsResult in Security-59306.41.2/trust/trustd/SecPolicyServer.c
        if (debugDarwinRoots && err != macOS.ErrNoTrustSettings) {
            fmt.Fprintf(os.Stderr, "crypto/x509: SecTrustSettingsCopyTrustSettings for SecTrustSettingsDomainAdmin failed: %s\n", err);
        }
        return (macOS.SecTrustSettingsResultUnspecified, error.As(null!)!);

    }
    defer(macOS.CFRelease(trustSettings)); 

    // "An empty trust settings array means 'always trust this certificate' with an
    // overall trust setting for the certificate of kSecTrustSettingsResultTrustRoot."
    if (macOS.CFArrayGetCount(trustSettings) == 0) {
        return (macOS.SecTrustSettingsResultTrustRoot, error.As(null!)!);
    }
    Func<macOS.CFRef, bool> isSSLPolicy = policyRef => {
        var properties = macOS.SecPolicyCopyProperties(policyRef);
        defer(macOS.CFRelease(properties));
        {
            var (v, ok) = macOS.CFDictionaryGetValueIfPresent(properties, macOS.SecPolicyOid);

            if (ok) {
                return macOS.CFEqual(v, macOS.CFRef(macOS.SecPolicyAppleSSL));
            }

        }

        return false;

    };

    for (nint i = 0; i < macOS.CFArrayGetCount(trustSettings); i++) {
        var tSetting = macOS.CFArrayGetValueAtIndex(trustSettings, i); 

        // First, check if this trust setting is constrained to a non-SSL policy.
        {
            var (policyRef, ok) = macOS.CFDictionaryGetValueIfPresent(tSetting, macOS.SecTrustSettingsPolicy);

            if (ok) {
                if (!isSSLPolicy(policyRef)) {
                    continue;
                }
            } 

            // Then check if it is restricted to a hostname, so not a root.

        } 

        // Then check if it is restricted to a hostname, so not a root.
        {
            var (_, ok) = macOS.CFDictionaryGetValueIfPresent(tSetting, macOS.SecTrustSettingsPolicyString);

            if (ok) {
                continue;
            }

        }


        var (cfNum, ok) = macOS.CFDictionaryGetValueIfPresent(tSetting, macOS.SecTrustSettingsResultKey); 
        // "If this key is not present, a default value of kSecTrustSettingsResultTrustRoot is assumed."
        if (!ok) {
            return (macOS.SecTrustSettingsResultTrustRoot, error.As(null!)!);
        }
        var (result, err) = macOS.CFNumberGetValue(cfNum);
        if (err != null) {
            return (0, error.As(err)!);
        }
        {
            var r = macOS.SecTrustSettingsResult(result);


            if (r == macOS.SecTrustSettingsResultTrustRoot || r == macOS.SecTrustSettingsResultTrustAsRoot || r == macOS.SecTrustSettingsResultDeny) 
                return (r, error.As(null!)!);

        }

    } 

    // If trust settings are present, but none of them match the policy...
    // the docs don't tell us what to do.
    //
    // "Trust settings for a given use apply if any of the dictionaries in the
    // certificate’s trust settings array satisfies the specified use." suggests
    // that it's as if there were no trust settings at all, so we should maybe
    // fallback to the admin trust settings? TODO(golang.org/issue/38888).

    return (macOS.SecTrustSettingsResultUnspecified, error.As(null!)!);

});

} // end x509_package
