// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 13 05:34:37 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\parser.go
namespace go.crypto;

using bytes = bytes_package;
using dsa = crypto.dsa_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using elliptic = crypto.elliptic_package;
using rsa = crypto.rsa_package;
using pkix = crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using big = math.big_package;
using net = net_package;
using url = net.url_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using utf16 = unicode.utf16_package;
using utf8 = unicode.utf8_package;

using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using cryptobyte_asn1 = golang.org.x.crypto.cryptobyte.asn1_package;


// isPrintable reports whether the given b is in the ASN.1 PrintableString set.
// This is a simplified version of encoding/asn1.isPrintable.

using System;
public static partial class x509_package {

private static bool isPrintable(byte b) {
    return 'a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || '0' <= b && b <= '9' || '\'' <= b && b <= ')' || '+' <= b && b <= '/' || b == ' ' || b == ':' || b == '=' || b == '?' || b == '*' || b == '&';
}

// parseASN1String parses the ASN.1 string types T61String, PrintableString,
// UTF8String, BMPString, and IA5String. This is mostly copied from the
// respective encoding/asn1.parse... methods, rather than just increasing
// the API surface of that package.
private static (@string, error) parseASN1String(cryptobyte_asn1.Tag tag, slice<byte> value) {
    @string _p0 = default;
    error _p0 = default!;


    if (tag == cryptobyte_asn1.T61String) 
        return (string(value), error.As(null!)!);
    else if (tag == cryptobyte_asn1.PrintableString) 
        foreach (var (_, b) in value) {
            if (!isPrintable(b)) {
                return ("", error.As(errors.New("invalid PrintableString"))!);
            }
        }        return (string(value), error.As(null!)!);
    else if (tag == cryptobyte_asn1.UTF8String) 
        if (!utf8.Valid(value)) {
            return ("", error.As(errors.New("invalid UTF-8 string"))!);
        }
        return (string(value), error.As(null!)!);
    else if (tag == cryptobyte_asn1.Tag(asn1.TagBMPString)) 
        if (len(value) % 2 != 0) {
            return ("", error.As(errors.New("invalid BMPString"))!);
        }
        {
            var l = len(value);

            if (l >= 2 && value[l - 1] == 0 && value[l - 2] == 0) {
                value = value[..(int)l - 2];
            }

        }

        var s = make_slice<ushort>(0, len(value) / 2);
        while (len(value) > 0) {
            s = append(s, uint16(value[0]) << 8 + uint16(value[1]));
            value = value[(int)2..];
        }

        return (string(utf16.Decode(s)), error.As(null!)!);
    else if (tag == cryptobyte_asn1.IA5String) 
        s = string(value);
        if (isIA5String(s) != null) {
            return ("", error.As(errors.New("invalid IA5String"))!);
        }
        return (s, error.As(null!)!);
        return ("", error.As(fmt.Errorf("unsupported string type: %v", tag))!);
}

// parseName parses a DER encoded Name as defined in RFC 5280. We may
// want to export this function in the future for use in crypto/tls.
private static (ptr<pkix.RDNSequence>, error) parseName(cryptobyte.String raw) {
    ptr<pkix.RDNSequence> _p0 = default!;
    error _p0 = default!;

    if (!raw.ReadASN1(_addr_raw, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: invalid RDNSequence"))!);
    }
    ref pkix.RDNSequence rdnSeq = ref heap(out ptr<pkix.RDNSequence> _addr_rdnSeq);
    while (!raw.Empty()) {
        pkix.RelativeDistinguishedNameSET rdnSet = default;
        ref cryptobyte.String set = ref heap(out ptr<cryptobyte.String> _addr_set);
        if (!raw.ReadASN1(_addr_set, cryptobyte_asn1.SET)) {
            return (_addr_null!, error.As(errors.New("x509: invalid RDNSequence"))!);
        }
        while (!set.Empty()) {
            ref cryptobyte.String atav = ref heap(out ptr<cryptobyte.String> _addr_atav);
            if (!set.ReadASN1(_addr_atav, cryptobyte_asn1.SEQUENCE)) {
                return (_addr_null!, error.As(errors.New("x509: invalid RDNSequence: invalid attribute"))!);
            }
            pkix.AttributeTypeAndValue attr = default;
            if (!atav.ReadASN1ObjectIdentifier(_addr_attr.Type)) {
                return (_addr_null!, error.As(errors.New("x509: invalid RDNSequence: invalid attribute type"))!);
            }
            ref cryptobyte.String rawValue = ref heap(out ptr<cryptobyte.String> _addr_rawValue);
            ref cryptobyte_asn1.Tag valueTag = ref heap(out ptr<cryptobyte_asn1.Tag> _addr_valueTag);
            if (!atav.ReadAnyASN1(_addr_rawValue, _addr_valueTag)) {
                return (_addr_null!, error.As(errors.New("x509: invalid RDNSequence: invalid attribute value"))!);
            }
            error err = default!;
            attr.Value, err = parseASN1String(valueTag, rawValue);
            if (err != null) {
                return (_addr_null!, error.As(fmt.Errorf("x509: invalid RDNSequence: invalid attribute value: %s", err))!);
            }
            rdnSet = append(rdnSet, attr);
        }

        rdnSeq = append(rdnSeq, rdnSet);
    }

    return (_addr__addr_rdnSeq!, error.As(null!)!);
}

private static (pkix.AlgorithmIdentifier, error) parseAI(cryptobyte.String der) {
    pkix.AlgorithmIdentifier _p0 = default;
    error _p0 = default!;

    pkix.AlgorithmIdentifier ai = new pkix.AlgorithmIdentifier();
    if (!der.ReadASN1ObjectIdentifier(_addr_ai.Algorithm)) {
        return (ai, error.As(errors.New("x509: malformed OID"))!);
    }
    if (der.Empty()) {
        return (ai, error.As(null!)!);
    }
    ref cryptobyte.String @params = ref heap(out ptr<cryptobyte.String> _addr_@params);
    ref cryptobyte_asn1.Tag tag = ref heap(out ptr<cryptobyte_asn1.Tag> _addr_tag);
    if (!der.ReadAnyASN1Element(_addr_params, _addr_tag)) {
        return (ai, error.As(errors.New("x509: malformed parameters"))!);
    }
    ai.Parameters.Tag = int(tag);
    ai.Parameters.FullBytes = params;
    return (ai, error.As(null!)!);
}

private static (time.Time, time.Time, error) parseValidity(cryptobyte.String der) {
    time.Time _p0 = default;
    time.Time _p0 = default;
    error _p0 = default!;

    Func<(time.Time, error)> extract = () => {
        ref time.Time t = ref heap(out ptr<time.Time> _addr_t);

        if (der.PeekASN1Tag(cryptobyte_asn1.UTCTime)) 
            // TODO(rolandshoemaker): once #45411 is fixed, the following code
            // should be replaced with a call to der.ReadASN1UTCTime.
            ref cryptobyte.String utc = ref heap(out ptr<cryptobyte.String> _addr_utc);
            if (!der.ReadASN1(_addr_utc, cryptobyte_asn1.UTCTime)) {
                return (t, errors.New("x509: malformed UTCTime"));
            }
            var s = string(utc);

            @string formatStr = "0601021504Z0700";
            error err = default!;
            t, err = time.Parse(formatStr, s);
            if (err != null) {
                formatStr = "060102150405Z0700";
                t, err = time.Parse(formatStr, s);
            }
            if (err != null) {
                return (t, err);
            }
            {
                var serialized = t.Format(formatStr);

                if (serialized != s) {
                    return (t, errors.New("x509: malformed UTCTime"));
                }

            }

            if (t.Year() >= 2050) { 
                // UTCTime only encodes times prior to 2050. See https://tools.ietf.org/html/rfc5280#section-4.1.2.5.1
                t = t.AddDate(-100, 0, 0);
            }
        else if (der.PeekASN1Tag(cryptobyte_asn1.GeneralizedTime)) 
            if (!der.ReadASN1GeneralizedTime(_addr_t)) {
                return (t, errors.New("x509: malformed GeneralizedTime"));
            }
        else 
            return (t, errors.New("x509: unsupported time format"));
                return (t, null);
    };

    var (notBefore, err) = extract();
    if (err != null) {
        return (new time.Time(), new time.Time(), error.As(err)!);
    }
    var (notAfter, err) = extract();
    if (err != null) {
        return (new time.Time(), new time.Time(), error.As(err)!);
    }
    return (notBefore, notAfter, error.As(null!)!);
}

private static (pkix.Extension, error) parseExtension(cryptobyte.String der) {
    pkix.Extension _p0 = default;
    error _p0 = default!;

    pkix.Extension ext = default;
    if (!der.ReadASN1ObjectIdentifier(_addr_ext.Id)) {
        return (ext, error.As(errors.New("x509: malformed extention OID field"))!);
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(_addr_ext.Critical)) {
            return (ext, error.As(errors.New("x509: malformed extention critical field"))!);
        }
    }
    ref cryptobyte.String val = ref heap(out ptr<cryptobyte.String> _addr_val);
    if (!der.ReadASN1(_addr_val, cryptobyte_asn1.OCTET_STRING)) {
        return (ext, error.As(errors.New("x509: malformed extention value field"))!);
    }
    ext.Value = val;
    return (ext, error.As(null!)!);
}

private static (object, error) parsePublicKey(PublicKeyAlgorithm algo, ptr<publicKeyInfo> _addr_keyData) {
    object _p0 = default;
    error _p0 = default!;
    ref publicKeyInfo keyData = ref _addr_keyData.val;

    ref var der = ref heap(cryptobyte.String(keyData.PublicKey.RightAlign()), out ptr<var> _addr_der);

    if (algo == RSA) 
        // RSA public keys must have a NULL in the parameters.
        // See RFC 3279, Section 2.3.1.
        if (!bytes.Equal(keyData.Algorithm.Parameters.FullBytes, asn1.NullBytes)) {
            return (null, error.As(errors.New("x509: RSA key missing NULL parameters"))!);
        }
        ptr<pkcs1PublicKey> p = addr(new pkcs1PublicKey(N:new(big.Int)));
        if (!der.ReadASN1(_addr_der, cryptobyte_asn1.SEQUENCE)) {
            return (null, error.As(errors.New("x509: invalid RSA public key"))!);
        }
        if (!der.ReadASN1Integer(p.N)) {
            return (null, error.As(errors.New("x509: invalid RSA modulus"))!);
        }
        if (!der.ReadASN1Integer(_addr_p.E)) {
            return (null, error.As(errors.New("x509: invalid RSA public exponent"))!);
        }
        if (p.N.Sign() <= 0) {
            return (null, error.As(errors.New("x509: RSA modulus is not a positive number"))!);
        }
        if (p.E <= 0) {
            return (null, error.As(errors.New("x509: RSA public exponent is not a positive number"))!);
        }
        ptr<rsa.PublicKey> pub = addr(new rsa.PublicKey(E:p.E,N:p.N,));
        return (pub, error.As(null!)!);
    else if (algo == ECDSA) 
        ref var paramsDer = ref heap(cryptobyte.String(keyData.Algorithm.Parameters.FullBytes), out ptr<var> _addr_paramsDer);
        ptr<object> namedCurveOID = @new<asn1.ObjectIdentifier>();
        if (!paramsDer.ReadASN1ObjectIdentifier(namedCurveOID)) {
            return (null, error.As(errors.New("x509: invalid ECDSA parameters"))!);
        }
        var namedCurve = namedCurveFromOID(namedCurveOID.val);
        if (namedCurve == null) {
            return (null, error.As(errors.New("x509: unsupported elliptic curve"))!);
        }
        var (x, y) = elliptic.Unmarshal(namedCurve, der);
        if (x == null) {
            return (null, error.As(errors.New("x509: failed to unmarshal elliptic curve point"))!);
        }
        pub = addr(new ecdsa.PublicKey(Curve:namedCurve,X:x,Y:y,));
        return (pub, error.As(null!)!);
    else if (algo == Ed25519) 
        // RFC 8410, Section 3
        // > For all of the OIDs, the parameters MUST be absent.
        if (len(keyData.Algorithm.Parameters.FullBytes) != 0) {
            return (null, error.As(errors.New("x509: Ed25519 key encoded with illegal parameters"))!);
        }
        if (len(der) != ed25519.PublicKeySize) {
            return (null, error.As(errors.New("x509: wrong Ed25519 public key size"))!);
        }
        return (ed25519.PublicKey(der), error.As(null!)!);
    else if (algo == DSA) 
        ptr<object> y = @new<big.Int>();
        if (!der.ReadASN1Integer(y)) {
            return (null, error.As(errors.New("x509: invalid DSA public key"))!);
        }
        pub = addr(new dsa.PublicKey(Y:y,Parameters:dsa.Parameters{P:new(big.Int),Q:new(big.Int),G:new(big.Int),},));
        paramsDer = cryptobyte.String(keyData.Algorithm.Parameters.FullBytes);
        if (!paramsDer.ReadASN1(_addr_paramsDer, cryptobyte_asn1.SEQUENCE) || !paramsDer.ReadASN1Integer(pub.Parameters.P) || !paramsDer.ReadASN1Integer(pub.Parameters.Q) || !paramsDer.ReadASN1Integer(pub.Parameters.G)) {
            return (null, error.As(errors.New("x509: invalid DSA parameters"))!);
        }
        if (pub.Y.Sign() <= 0 || pub.Parameters.P.Sign() <= 0 || pub.Parameters.Q.Sign() <= 0 || pub.Parameters.G.Sign() <= 0) {
            return (null, error.As(errors.New("x509: zero or negative DSA parameter"))!);
        }
        return (pub, error.As(null!)!);
    else 
        return (null, error.As(null!)!);
    }

private static (KeyUsage, error) parseKeyUsageExtension(cryptobyte.String der) {
    KeyUsage _p0 = default;
    error _p0 = default!;

    ref asn1.BitString usageBits = ref heap(out ptr<asn1.BitString> _addr_usageBits);
    if (!der.ReadASN1BitString(_addr_usageBits)) {
        return (0, error.As(errors.New("x509: invalid key usage"))!);
    }
    nint usage = default;
    for (nint i = 0; i < 9; i++) {
        if (usageBits.At(i) != 0) {
            usage |= 1 << (int)(uint(i));
        }
    }
    return (KeyUsage(usage), error.As(null!)!);
}

private static (bool, nint, error) parseBasicConstraintsExtension(cryptobyte.String der) {
    bool _p0 = default;
    nint _p0 = default;
    error _p0 = default!;

    ref bool isCA = ref heap(out ptr<bool> _addr_isCA);
    if (!der.ReadASN1(_addr_der, cryptobyte_asn1.SEQUENCE)) {
        return (false, 0, error.As(errors.New("x509: invalid basic constraints a"))!);
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(_addr_isCA)) {
            return (false, 0, error.As(errors.New("x509: invalid basic constraints b"))!);
        }
    }
    ref nint maxPathLen = ref heap(-1, out ptr<nint> _addr_maxPathLen);
    if (!der.Empty() && der.PeekASN1Tag(cryptobyte_asn1.INTEGER)) {
        if (!der.ReadASN1Integer(_addr_maxPathLen)) {
            return (false, 0, error.As(errors.New("x509: invalid basic constraints c"))!);
        }
    }
    return (isCA, maxPathLen, error.As(null!)!);
}

private static error forEachSAN(cryptobyte.String der, Func<nint, slice<byte>, error> callback) {
    if (!der.ReadASN1(_addr_der, cryptobyte_asn1.SEQUENCE)) {
        return error.As(errors.New("x509: invalid subject alternative names"))!;
    }
    while (!der.Empty()) {
        ref cryptobyte.String san = ref heap(out ptr<cryptobyte.String> _addr_san);
        ref cryptobyte_asn1.Tag tag = ref heap(out ptr<cryptobyte_asn1.Tag> _addr_tag);
        if (!der.ReadAnyASN1(_addr_san, _addr_tag)) {
            return error.As(errors.New("x509: invalid subject alternative name"))!;
        }
        {
            var err = callback(int(tag ^ 0x80), san);

            if (err != null) {
                return error.As(err)!;
            }

        }
    }

    return error.As(null!)!;
}

private static (slice<@string>, slice<@string>, slice<net.IP>, slice<ptr<url.URL>>, error) parseSANExtension(cryptobyte.String der) {
    slice<@string> dnsNames = default;
    slice<@string> emailAddresses = default;
    slice<net.IP> ipAddresses = default;
    slice<ptr<url.URL>> uris = default;
    error err = default!;

    err = forEachSAN(der, (tag, data) => {

        if (tag == nameTypeEmail) 
            var email = string(data);
            {
                var err__prev1 = err;

                var err = isIA5String(email);

                if (err != null) {
                    return errors.New("x509: SAN rfc822Name is malformed");
                }

                err = err__prev1;

            }
            emailAddresses = append(emailAddresses, email);
        else if (tag == nameTypeDNS) 
            var name = string(data);
            {
                var err__prev1 = err;

                err = isIA5String(name);

                if (err != null) {
                    return errors.New("x509: SAN dNSName is malformed");
                }

                err = err__prev1;

            }
            dnsNames = append(dnsNames, string(name));
        else if (tag == nameTypeURI) 
            var uriStr = string(data);
            {
                var err__prev1 = err;

                err = isIA5String(uriStr);

                if (err != null) {
                    return errors.New("x509: SAN uniformResourceIdentifier is malformed");
                }

                err = err__prev1;

            }
            var (uri, err) = url.Parse(uriStr);
            if (err != null) {
                return fmt.Errorf("x509: cannot parse URI %q: %s", uriStr, err);
            }
            if (len(uri.Host) > 0) {
                {
                    var (_, ok) = domainToReverseLabels(uri.Host);

                    if (!ok) {
                        return fmt.Errorf("x509: cannot parse URI %q: invalid domain", uriStr);
                    }

                }
            }
            uris = append(uris, uri);
        else if (tag == nameTypeIP) 

            if (len(data) == net.IPv4len || len(data) == net.IPv6len) 
                ipAddresses = append(ipAddresses, data);
            else 
                return errors.New("x509: cannot parse IP address of length " + strconv.Itoa(len(data)));
                            return null;
    });

    return ;
}

private static (slice<ExtKeyUsage>, slice<asn1.ObjectIdentifier>, error) parseExtKeyUsageExtension(cryptobyte.String der) {
    slice<ExtKeyUsage> _p0 = default;
    slice<asn1.ObjectIdentifier> _p0 = default;
    error _p0 = default!;

    slice<ExtKeyUsage> extKeyUsages = default;
    slice<asn1.ObjectIdentifier> unknownUsages = default;
    if (!der.ReadASN1(_addr_der, cryptobyte_asn1.SEQUENCE)) {
        return (null, null, error.As(errors.New("x509: invalid extended key usages"))!);
    }
    while (!der.Empty()) {
        ref asn1.ObjectIdentifier eku = ref heap(out ptr<asn1.ObjectIdentifier> _addr_eku);
        if (!der.ReadASN1ObjectIdentifier(_addr_eku)) {
            return (null, null, error.As(errors.New("x509: invalid extended key usages"))!);
        }
        {
            var (extKeyUsage, ok) = extKeyUsageFromOID(eku);

            if (ok) {
                extKeyUsages = append(extKeyUsages, extKeyUsage);
            }
            else
 {
                unknownUsages = append(unknownUsages, eku);
            }

        }
    }
    return (extKeyUsages, unknownUsages, error.As(null!)!);
}

private static (slice<asn1.ObjectIdentifier>, error) parseCertificatePoliciesExtension(cryptobyte.String der) {
    slice<asn1.ObjectIdentifier> _p0 = default;
    error _p0 = default!;

    slice<asn1.ObjectIdentifier> oids = default;
    if (!der.ReadASN1(_addr_der, cryptobyte_asn1.SEQUENCE)) {
        return (null, error.As(errors.New("x509: invalid certificate policies"))!);
    }
    while (!der.Empty()) {
        ref cryptobyte.String cp = ref heap(out ptr<cryptobyte.String> _addr_cp);
        if (!der.ReadASN1(_addr_cp, cryptobyte_asn1.SEQUENCE)) {
            return (null, error.As(errors.New("x509: invalid certificate policies"))!);
        }
        ref asn1.ObjectIdentifier oid = ref heap(out ptr<asn1.ObjectIdentifier> _addr_oid);
        if (!cp.ReadASN1ObjectIdentifier(_addr_oid)) {
            return (null, error.As(errors.New("x509: invalid certificate policies"))!);
        }
        oids = append(oids, oid);
    }

    return (oids, error.As(null!)!);
}

// isValidIPMask reports whether mask consists of zero or more 1 bits, followed by zero bits.
private static bool isValidIPMask(slice<byte> mask) {
    var seenZero = false;

    foreach (var (_, b) in mask) {
        if (seenZero) {
            if (b != 0) {
                return false;
            }
            continue;
        }
        switch (b) {
            case 0x00: 

            case 0x80: 

            case 0xc0: 

            case 0xe0: 

            case 0xf0: 

            case 0xf8: 

            case 0xfc: 

            case 0xfe: 
                seenZero = true;
                break;
            case 0xff: 

                break;
            default: 
                return false;
                break;
        }
    }    return true;
}

private static (bool, error) parseNameConstraintsExtension(ptr<Certificate> _addr_@out, pkix.Extension e) {
    bool unhandled = default;
    error err = default!;
    ref Certificate @out = ref _addr_@out.val;
 
    // RFC 5280, 4.2.1.10

    // NameConstraints ::= SEQUENCE {
    //      permittedSubtrees       [0]     GeneralSubtrees OPTIONAL,
    //      excludedSubtrees        [1]     GeneralSubtrees OPTIONAL }
    //
    // GeneralSubtrees ::= SEQUENCE SIZE (1..MAX) OF GeneralSubtree
    //
    // GeneralSubtree ::= SEQUENCE {
    //      base                    GeneralName,
    //      minimum         [0]     BaseDistance DEFAULT 0,
    //      maximum         [1]     BaseDistance OPTIONAL }
    //
    // BaseDistance ::= INTEGER (0..MAX)

    var outer = cryptobyte.String(e.Value);
    ref cryptobyte.String toplevel = ref heap(out ptr<cryptobyte.String> _addr_toplevel);    ref cryptobyte.String permitted = ref heap(out ptr<cryptobyte.String> _addr_permitted);    ref cryptobyte.String excluded = ref heap(out ptr<cryptobyte.String> _addr_excluded);

    ref bool havePermitted = ref heap(out ptr<bool> _addr_havePermitted);    ref bool haveExcluded = ref heap(out ptr<bool> _addr_haveExcluded);

    if (!outer.ReadASN1(_addr_toplevel, cryptobyte_asn1.SEQUENCE) || !outer.Empty() || !toplevel.ReadOptionalASN1(_addr_permitted, _addr_havePermitted, cryptobyte_asn1.Tag(0).ContextSpecific().Constructed()) || !toplevel.ReadOptionalASN1(_addr_excluded, _addr_haveExcluded, cryptobyte_asn1.Tag(1).ContextSpecific().Constructed()) || !toplevel.Empty()) {
        return (false, error.As(errors.New("x509: invalid NameConstraints extension"))!);
    }
    if (!havePermitted && !haveExcluded || len(permitted) == 0 && len(excluded) == 0) { 
        // From RFC 5280, Section 4.2.1.10:
        //   “either the permittedSubtrees field
        //   or the excludedSubtrees MUST be
        //   present”
        return (false, error.As(errors.New("x509: empty name constraints extension"))!);
    }
    Func<cryptobyte.String, (slice<@string>, slice<ptr<net.IPNet>>, slice<@string>, slice<@string>, error)> getValues = subtrees => {
        while (!subtrees.Empty()) {
            ref cryptobyte.String seq = ref heap(out ptr<cryptobyte.String> _addr_seq);            ref cryptobyte.String value = ref heap(out ptr<cryptobyte.String> _addr_value);

            ref cryptobyte_asn1.Tag tag = ref heap(out ptr<cryptobyte_asn1.Tag> _addr_tag);
            if (!subtrees.ReadASN1(_addr_seq, cryptobyte_asn1.SEQUENCE) || !seq.ReadAnyASN1(_addr_value, _addr_tag)) {
                return (null, error.As(null!)!, null, null, fmt.Errorf("x509: invalid NameConstraints extension"));
            }
            var dnsTag = cryptobyte_asn1.Tag(2).ContextSpecific();            var emailTag = cryptobyte_asn1.Tag(1).ContextSpecific();            var ipTag = cryptobyte_asn1.Tag(7).ContextSpecific();            var uriTag = cryptobyte_asn1.Tag(6).ContextSpecific();


            if (tag == dnsTag) 
                var domain = string(value);
                {
                    var err__prev1 = err;

                    var err = isIA5String(domain);

                    if (err != null) {
                        return (null, error.As(null!)!, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                    }

                    err = err__prev1;

                }

                var trimmedDomain = domain;
                if (len(trimmedDomain) > 0 && trimmedDomain[0] == '.') { 
                    // constraints can have a leading
                    // period to exclude the domain
                    // itself, but that's not valid in a
                    // normal domain name.
                    trimmedDomain = trimmedDomain[(int)1..];
                }
                {
                    var (_, ok) = domainToReverseLabels(trimmedDomain);

                    if (!ok) {
                        return (null, error.As(null!)!, null, null, fmt.Errorf("x509: failed to parse dnsName constraint %q", domain));
                    }

                }
                dnsNames = append(dnsNames, domain);
            else if (tag == ipTag) 
                var l = len(value);
                slice<byte> ip = default;                slice<byte> mask = default;



                switch (l) {
                    case 8: 
                        ip = value[..(int)4];
                        mask = value[(int)4..];
                        break;
                    case 32: 
                        ip = value[..(int)16];
                        mask = value[(int)16..];
                        break;
                    default: 
                        return (null, error.As(null!)!, null, null, fmt.Errorf("x509: IP constraint contained value of length %d", l));
                        break;
                }

                if (!isValidIPMask(mask)) {
                    return (null, error.As(null!)!, null, null, fmt.Errorf("x509: IP constraint contained invalid mask %x", mask));
                }
                ips = append(ips, addr(new net.IPNet(IP:net.IP(ip),Mask:net.IPMask(mask))));
            else if (tag == emailTag) 
                var constraint = string(value);
                {
                    var err__prev1 = err;

                    err = isIA5String(constraint);

                    if (err != null) {
                        return (null, error.As(null!)!, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                    } 

                    // If the constraint contains an @ then
                    // it specifies an exact mailbox name.

                    err = err__prev1;

                } 

                // If the constraint contains an @ then
                // it specifies an exact mailbox name.
                if (strings.Contains(constraint, "@")) {
                    {
                        (_, ok) = parseRFC2821Mailbox(constraint);

                        if (!ok) {
                            return (null, error.As(null!)!, null, null, fmt.Errorf("x509: failed to parse rfc822Name constraint %q", constraint));
                        }

                    }
                }
                else
 { 
                    // Otherwise it's a domain name.
                    domain = constraint;
                    if (len(domain) > 0 && domain[0] == '.') {
                        domain = domain[(int)1..];
                    }
                    {
                        (_, ok) = domainToReverseLabels(domain);

                        if (!ok) {
                            return (null, error.As(null!)!, null, null, fmt.Errorf("x509: failed to parse rfc822Name constraint %q", constraint));
                        }

                    }
                }
                emails = append(emails, constraint);
            else if (tag == uriTag) 
                domain = string(value);
                {
                    var err__prev1 = err;

                    err = isIA5String(domain);

                    if (err != null) {
                        return (null, error.As(null!)!, null, null, errors.New("x509: invalid constraint value: " + err.Error()));
                    }

                    err = err__prev1;

                }

                if (net.ParseIP(domain) != null) {
                    return (null, error.As(null!)!, null, null, fmt.Errorf("x509: failed to parse URI constraint %q: cannot be IP address", domain));
                }
                trimmedDomain = domain;
                if (len(trimmedDomain) > 0 && trimmedDomain[0] == '.') { 
                    // constraints can have a leading
                    // period to exclude the domain itself,
                    // but that's not valid in a normal
                    // domain name.
                    trimmedDomain = trimmedDomain[(int)1..];
                }
                {
                    (_, ok) = domainToReverseLabels(trimmedDomain);

                    if (!ok) {
                        return (null, error.As(null!)!, null, null, fmt.Errorf("x509: failed to parse URI constraint %q", domain));
                    }

                }
                uriDomains = append(uriDomains, domain);
            else 
                unhandled = true;
                    }

        return (dnsNames, error.As(ips)!, emails, uriDomains, null);
    };

    @out.PermittedDNSDomains, @out.PermittedIPRanges, @out.PermittedEmailAddresses, @out.PermittedURIDomains, err = getValues(permitted);

    if (err != null) {
        return (false, error.As(err)!);
    }
    @out.ExcludedDNSDomains, @out.ExcludedIPRanges, @out.ExcludedEmailAddresses, @out.ExcludedURIDomains, err = getValues(excluded);

    if (err != null) {
        return (false, error.As(err)!);
    }
    @out.PermittedDNSDomainsCritical = e.Critical;

    return (unhandled, error.As(null!)!);
}

private static error processExtensions(ptr<Certificate> _addr_@out) {
    ref Certificate @out = ref _addr_@out.val;

    error err = default!;
    foreach (var (_, e) in @out.Extensions) {
        var unhandled = false;

        if (len(e.Id) == 4 && e.Id[0] == 2 && e.Id[1] == 5 && e.Id[2] == 29) {
            switch (e.Id[3]) {
                case 15: 
                    @out.KeyUsage, err = parseKeyUsageExtension(e.Value);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    break;
                case 19: 
                    @out.IsCA, @out.MaxPathLen, err = parseBasicConstraintsExtension(e.Value);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    @out.BasicConstraintsValid = true;
                    @out.MaxPathLenZero = @out.MaxPathLen == 0;
                    break;
                case 17: 
                    @out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err = parseSANExtension(e.Value);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    if (len(@out.DNSNames) == 0 && len(@out.EmailAddresses) == 0 && len(@out.IPAddresses) == 0 && len(@out.URIs) == 0) { 
                        // If we didn't parse anything then we do the critical check, below.
                        unhandled = true;
                    }
                    break;
                case 30: 
                    unhandled, err = parseNameConstraintsExtension(_addr_out, e);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    break;
                case 31: 
                    // RFC 5280, 4.2.1.13

                    // CRLDistributionPoints ::= SEQUENCE SIZE (1..MAX) OF DistributionPoint
                    //
                    // DistributionPoint ::= SEQUENCE {
                    //     distributionPoint       [0]     DistributionPointName OPTIONAL,
                    //     reasons                 [1]     ReasonFlags OPTIONAL,
                    //     cRLIssuer               [2]     GeneralNames OPTIONAL }
                    //
                    // DistributionPointName ::= CHOICE {
                    //     fullName                [0]     GeneralNames,
                    //     nameRelativeToCRLIssuer [1]     RelativeDistinguishedName }
                    ref var val = ref heap(cryptobyte.String(e.Value), out ptr<var> _addr_val);
                    if (!val.ReadASN1(_addr_val, cryptobyte_asn1.SEQUENCE)) {
                        return error.As(errors.New("x509: invalid CRL distribution points"))!;
                    }
                    while (!val.Empty()) {
                        ref cryptobyte.String dpDER = ref heap(out ptr<cryptobyte.String> _addr_dpDER);
                        if (!val.ReadASN1(_addr_dpDER, cryptobyte_asn1.SEQUENCE)) {
                            return error.As(errors.New("x509: invalid CRL distribution point"))!;
                        }
                        ref cryptobyte.String dpNameDER = ref heap(out ptr<cryptobyte.String> _addr_dpNameDER);
                        ref bool dpNamePresent = ref heap(out ptr<bool> _addr_dpNamePresent);
                        if (!dpDER.ReadOptionalASN1(_addr_dpNameDER, _addr_dpNamePresent, cryptobyte_asn1.Tag(0).Constructed().ContextSpecific())) {
                            return error.As(errors.New("x509: invalid CRL distribution point"))!;
                        }
                        if (!dpNamePresent) {
                            continue;
                        }
                        if (!dpNameDER.ReadASN1(_addr_dpNameDER, cryptobyte_asn1.Tag(0).Constructed().ContextSpecific())) {
                            return error.As(errors.New("x509: invalid CRL distribution point"))!;
                        }
                        while (!dpNameDER.Empty()) {
                            if (!dpNameDER.PeekASN1Tag(cryptobyte_asn1.Tag(6).ContextSpecific())) {
                                break;
                            }
                            ref cryptobyte.String uri = ref heap(out ptr<cryptobyte.String> _addr_uri);
                            if (!dpNameDER.ReadASN1(_addr_uri, cryptobyte_asn1.Tag(6).ContextSpecific())) {
                                return error.As(errors.New("x509: invalid CRL distribution point"))!;
                            }
                            @out.CRLDistributionPoints = append(@out.CRLDistributionPoints, string(uri));
                        }
                    }

                    break;
                case 35: 
                    // RFC 5280, 4.2.1.1
                    val = cryptobyte.String(e.Value);
                    ref cryptobyte.String akid = ref heap(out ptr<cryptobyte.String> _addr_akid);
                    if (!val.ReadASN1(_addr_akid, cryptobyte_asn1.SEQUENCE)) {
                        return error.As(errors.New("x509: invalid authority key identifier"))!;
                    }
                    if (akid.PeekASN1Tag(cryptobyte_asn1.Tag(0).ContextSpecific())) {
                        if (!akid.ReadASN1(_addr_akid, cryptobyte_asn1.Tag(0).ContextSpecific())) {
                            return error.As(errors.New("x509: invalid authority key identifier"))!;
                        }
                        @out.AuthorityKeyId = akid;
                    }
                    break;
                case 37: 
                    @out.ExtKeyUsage, @out.UnknownExtKeyUsage, err = parseExtKeyUsageExtension(e.Value);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    break;
                case 14: 
                    // RFC 5280, 4.2.1.2
                    val = cryptobyte.String(e.Value);
                    ref cryptobyte.String skid = ref heap(out ptr<cryptobyte.String> _addr_skid);
                    if (!val.ReadASN1(_addr_skid, cryptobyte_asn1.OCTET_STRING)) {
                        return error.As(errors.New("x509: invalid subject key identifier"))!;
                    }
                    @out.SubjectKeyId = skid;
                    break;
                case 32: 
                    @out.PolicyIdentifiers, err = parseCertificatePoliciesExtension(e.Value);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    break;
                default: 
                    // Unknown extensions are recorded if critical.
                    unhandled = true;
                    break;
            }
        }
        else if (e.Id.Equal(oidExtensionAuthorityInfoAccess)) { 
            // RFC 5280 4.2.2.1: Authority Information Access
            val = cryptobyte.String(e.Value);
            if (!val.ReadASN1(_addr_val, cryptobyte_asn1.SEQUENCE)) {
                return error.As(errors.New("x509: invalid authority info access"))!;
            }
            while (!val.Empty()) {
                ref cryptobyte.String aiaDER = ref heap(out ptr<cryptobyte.String> _addr_aiaDER);
                if (!val.ReadASN1(_addr_aiaDER, cryptobyte_asn1.SEQUENCE)) {
                    return error.As(errors.New("x509: invalid authority info access"))!;
                }
                ref asn1.ObjectIdentifier method = ref heap(out ptr<asn1.ObjectIdentifier> _addr_method);
                if (!aiaDER.ReadASN1ObjectIdentifier(_addr_method)) {
                    return error.As(errors.New("x509: invalid authority info access"))!;
                }
                if (!aiaDER.PeekASN1Tag(cryptobyte_asn1.Tag(6).ContextSpecific())) {
                    continue;
                }
                if (!aiaDER.ReadASN1(_addr_aiaDER, cryptobyte_asn1.Tag(6).ContextSpecific())) {
                    return error.As(errors.New("x509: invalid authority info access"))!;
                }

                if (method.Equal(oidAuthorityInfoAccessOcsp)) 
                    @out.OCSPServer = append(@out.OCSPServer, string(aiaDER));
                else if (method.Equal(oidAuthorityInfoAccessIssuers)) 
                    @out.IssuingCertificateURL = append(@out.IssuingCertificateURL, string(aiaDER));
                            }
        else
        } { 
            // Unknown extensions are recorded if critical.
            unhandled = true;
        }
        if (e.Critical && unhandled) {
            @out.UnhandledCriticalExtensions = append(@out.UnhandledCriticalExtensions, e.Id);
        }
    }    return error.As(null!)!;
}

private static (ptr<Certificate>, error) parseCertificate(slice<byte> der) {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;

    ptr<Certificate> cert = addr(new Certificate());

    ref var input = ref heap(cryptobyte.String(der), out ptr<var> _addr_input); 
    // we read the SEQUENCE including length and tag bytes so that
    // we can populate Certificate.Raw, before unwrapping the
    // SEQUENCE so it can be operated on
    if (!input.ReadASN1Element(_addr_input, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed certificate"))!);
    }
    cert.Raw = input;
    if (!input.ReadASN1(_addr_input, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed certificate"))!);
    }
    ref cryptobyte.String tbs = ref heap(out ptr<cryptobyte.String> _addr_tbs); 
    // do the same trick again as above to extract the raw
    // bytes for Certificate.RawTBSCertificate
    if (!input.ReadASN1Element(_addr_tbs, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed tbs certificate"))!);
    }
    cert.RawTBSCertificate = tbs;
    if (!tbs.ReadASN1(_addr_tbs, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed tbs certificate"))!);
    }
    if (!tbs.ReadOptionalASN1Integer(_addr_cert.Version, cryptobyte_asn1.Tag(0).Constructed().ContextSpecific(), 0)) {
        return (_addr_null!, error.As(errors.New("x509: malformed version"))!);
    }
    if (cert.Version < 0) {
        return (_addr_null!, error.As(errors.New("x509: malformed version"))!);
    }
    cert.Version++;
    if (cert.Version > 3) {
        return (_addr_null!, error.As(errors.New("x509: invalid version"))!);
    }
    ptr<object> serial = @new<big.Int>();
    if (!tbs.ReadASN1Integer(serial)) {
        return (_addr_null!, error.As(errors.New("x509: malformed serial number"))!);
    }
    cert.SerialNumber = serial;

    ref cryptobyte.String sigAISeq = ref heap(out ptr<cryptobyte.String> _addr_sigAISeq);
    if (!tbs.ReadASN1(_addr_sigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed signature algorithm identifier"))!);
    }
    ref cryptobyte.String outerSigAISeq = ref heap(out ptr<cryptobyte.String> _addr_outerSigAISeq);
    if (!input.ReadASN1(_addr_outerSigAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed algorithm identifier"))!);
    }
    if (!bytes.Equal(outerSigAISeq, sigAISeq)) {
        return (_addr_null!, error.As(errors.New("x509: inner and outer signature algorithm identifiers don't match"))!);
    }
    var (sigAI, err) = parseAI(sigAISeq);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    cert.SignatureAlgorithm = getSignatureAlgorithmFromAI(sigAI);

    ref cryptobyte.String issuerSeq = ref heap(out ptr<cryptobyte.String> _addr_issuerSeq);
    if (!tbs.ReadASN1Element(_addr_issuerSeq, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed issuer"))!);
    }
    cert.RawIssuer = issuerSeq;
    var (issuerRDNs, err) = parseName(issuerSeq);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    cert.Issuer.FillFromRDNSequence(issuerRDNs);

    ref cryptobyte.String validity = ref heap(out ptr<cryptobyte.String> _addr_validity);
    if (!tbs.ReadASN1(_addr_validity, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed validity"))!);
    }
    cert.NotBefore, cert.NotAfter, err = parseValidity(validity);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    ref cryptobyte.String subjectSeq = ref heap(out ptr<cryptobyte.String> _addr_subjectSeq);
    if (!tbs.ReadASN1Element(_addr_subjectSeq, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed issuer"))!);
    }
    cert.RawSubject = subjectSeq;
    var (subjectRDNs, err) = parseName(subjectSeq);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    cert.Subject.FillFromRDNSequence(subjectRDNs);

    ref cryptobyte.String spki = ref heap(out ptr<cryptobyte.String> _addr_spki);
    if (!tbs.ReadASN1Element(_addr_spki, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed spki"))!);
    }
    cert.RawSubjectPublicKeyInfo = spki;
    if (!spki.ReadASN1(_addr_spki, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed spki"))!);
    }
    ref cryptobyte.String pkAISeq = ref heap(out ptr<cryptobyte.String> _addr_pkAISeq);
    if (!spki.ReadASN1(_addr_pkAISeq, cryptobyte_asn1.SEQUENCE)) {
        return (_addr_null!, error.As(errors.New("x509: malformed public key algorithm identifier"))!);
    }
    var (pkAI, err) = parseAI(pkAISeq);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    cert.PublicKeyAlgorithm = getPublicKeyAlgorithmFromOID(pkAI.Algorithm);
    ref asn1.BitString spk = ref heap(out ptr<asn1.BitString> _addr_spk);
    if (!spki.ReadASN1BitString(_addr_spk)) {
        return (_addr_null!, error.As(errors.New("x509: malformed subjectPublicKey"))!);
    }
    cert.PublicKey, err = parsePublicKey(cert.PublicKeyAlgorithm, addr(new publicKeyInfo(Algorithm:pkAI,PublicKey:spk,)));
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (cert.Version > 1) {
        if (!tbs.SkipOptionalASN1(cryptobyte_asn1.Tag(1).Constructed().ContextSpecific())) {
            return (_addr_null!, error.As(errors.New("x509: malformed issuerUniqueID"))!);
        }
        if (!tbs.SkipOptionalASN1(cryptobyte_asn1.Tag(2).Constructed().ContextSpecific())) {
            return (_addr_null!, error.As(errors.New("x509: malformed subjectUniqueID"))!);
        }
        if (cert.Version == 3) {
            ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
            ref bool present = ref heap(out ptr<bool> _addr_present);
            if (!tbs.ReadOptionalASN1(_addr_extensions, _addr_present, cryptobyte_asn1.Tag(3).Constructed().ContextSpecific())) {
                return (_addr_null!, error.As(errors.New("x509: malformed extensions"))!);
            }
            if (present) {
                if (!extensions.ReadASN1(_addr_extensions, cryptobyte_asn1.SEQUENCE)) {
                    return (_addr_null!, error.As(errors.New("x509: malformed extensions"))!);
                }
                while (!extensions.Empty()) {
                    ref cryptobyte.String extension = ref heap(out ptr<cryptobyte.String> _addr_extension);
                    if (!extensions.ReadASN1(_addr_extension, cryptobyte_asn1.SEQUENCE)) {
                        return (_addr_null!, error.As(errors.New("x509: malformed extension"))!);
                    }
                    var (ext, err) = parseExtension(extension);
                    if (err != null) {
                        return (_addr_null!, error.As(err)!);
                    }
                    cert.Extensions = append(cert.Extensions, ext);
                }

                err = processExtensions(cert);
                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }
            }
        }
    }
    ref asn1.BitString signature = ref heap(out ptr<asn1.BitString> _addr_signature);
    if (!input.ReadASN1BitString(_addr_signature)) {
        return (_addr_null!, error.As(errors.New("x509: malformed signature"))!);
    }
    cert.Signature = signature.RightAlign();

    return (_addr_cert!, error.As(null!)!);
}

// ParseCertificate parses a single certificate from the given ASN.1 DER data.
public static (ptr<Certificate>, error) ParseCertificate(slice<byte> der) {
    ptr<Certificate> _p0 = default!;
    error _p0 = default!;

    var (cert, err) = parseCertificate(der);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (len(der) != len(cert.Raw)) {
        return (_addr_null!, error.As(errors.New("x509: trailing data"))!);
    }
    return (_addr_cert!, error.As(err)!);
}

// ParseCertificates parses one or more certificates from the given ASN.1 DER
// data. The certificates must be concatenated with no intermediate padding.
public static (slice<ptr<Certificate>>, error) ParseCertificates(slice<byte> der) {
    slice<ptr<Certificate>> _p0 = default;
    error _p0 = default!;

    slice<ptr<Certificate>> certs = default;
    while (len(der) > 0) {
        var (cert, err) = parseCertificate(der);
        if (err != null) {
            return (null, error.As(err)!);
        }
        certs = append(certs, cert);
        der = der[(int)len(cert.Raw)..];
    }
    return (certs, error.As(null!)!);
}

} // end x509_package
