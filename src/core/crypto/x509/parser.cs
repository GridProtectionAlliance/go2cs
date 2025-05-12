// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using dsa = crypto.dsa_package;
using ecdh = crypto.ecdh_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using elliptic = crypto.elliptic_package;
using rsa = crypto.rsa_package;
using pkix = crypto.x509.pkix_package;
using asn1 = encoding.asn1_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
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
using @internal;
using crypto.x509;
using encoding;
using golang.org.x.crypto;
using math;
using net;
using unicode;

partial class x509_package {

// isPrintable reports whether the given b is in the ASN.1 PrintableString set.
// This is a simplified version of encoding/asn1.isPrintable.
internal static bool isPrintable(byte b) {
    return (rune)'a' <= b && b <= (rune)'z' || (rune)'A' <= b && b <= (rune)'Z' || (rune)'0' <= b && b <= (rune)'9' || (rune)'\'' <= b && b <= (rune)')' || (rune)'+' <= b && b <= (rune)'/' || b == (rune)' ' || b == (rune)':' || b == (rune)'=' || b == (rune)'?' || b == (rune)'*' || b == (rune)'&';
}

// This is technically not allowed in a PrintableString.
// However, x509 certificates with wildcard strings don't
// always use the correct string type so we permit it.
// This is not technically allowed either. However, not
// only is it relatively common, but there are also a
// handful of CA certificates that contain it. At least
// one of which will not expire until 2027.

// parseASN1String parses the ASN.1 string types T61String, PrintableString,
// UTF8String, BMPString, IA5String, and NumericString. This is mostly copied
// from the respective encoding/asn1.parse... methods, rather than just
// increasing the API surface of that package.
internal static (@string, error) parseASN1String(asn1.Tag tag, slice<byte> value) {
    var exprᴛ1 = tag;
    if (exprᴛ1 == cryptobyte_asn1.T61String) {
        return (((@string)value), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.PrintableString) {
        foreach (var (_, b) in value) {
            if (!isPrintable(b)) {
                return ("", errors.New("invalid PrintableString"u8));
            }
        }
        return (((@string)value), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.UTF8String) {
        if (!utf8.Valid(value)) {
            return ("", errors.New("invalid UTF-8 string"u8));
        }
        return (((@string)value), default!);
    }
    if (exprᴛ1 == ((asn1.Tag)asn1.TagBMPString)) {
        if (len(value) % 2 != 0) {
            return ("", errors.New("invalid BMPString"u8));
        }
        {
            nint l = len(value); if (l >= 2 && value[l - 1] == 0 && value[l - 2] == 0) {
                // Strip terminator if present.
                value = value[..(int)(l - 2)];
            }
        }
        var s = new slice<uint16>(0, len(value) / 2);
        while (len(value) > 0) {
            s = append(s, ((uint16)value[0]) << (int)(8) + ((uint16)value[1]));
            value = value[2..];
        }
        return (((@string)utf16.Decode(s)), default!);
    }
    if (exprᴛ1 == cryptobyte_asn1.IA5String) {
        @string s = ((@string)value);
        if (isIA5String(s) != default!) {
            return ("", errors.New("invalid IA5String"u8));
        }
        return (s, default!);
    }
    if (exprᴛ1 == ((asn1.Tag)asn1.TagNumericString)) {
        foreach (var (_, b) in value) {
            if (!((rune)'0' <= b && b <= (rune)'9' || b == (rune)' ')) {
                return ("", errors.New("invalid NumericString"u8));
            }
        }
        return (((@string)value), default!);
    }

    return ("", fmt.Errorf("unsupported string type: %v"u8, tag));
}

// parseName parses a DER encoded Name as defined in RFC 5280. We may
// want to export this function in the future for use in crypto/tls.
internal static (ж<pkix.RDNSequence>, error) parseName(cryptobyte.String raw) {
    if (!raw.ReadASN1(Ꮡ(raw), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: invalid RDNSequence"u8));
    }
    pkix.RDNSequence rdnSeq = default!;
    while (!raw.Empty()) {
        pkix.RelativeDistinguishedNameSET rdnSet = default!;
        cryptobyte.String set = default!;
        if (!raw.ReadASN1(Ꮡ(set), cryptobyte_asn1.SET)) {
            return (default!, errors.New("x509: invalid RDNSequence"u8));
        }
        while (!set.Empty()) {
            cryptobyte.String atav = default!;
            if (!set.ReadASN1(Ꮡ(atav), cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New("x509: invalid RDNSequence: invalid attribute"u8));
            }
            ref var attr = ref heap(new crypto.x509.pkix_package.AttributeTypeAndValue(), out var Ꮡattr);
            if (!atav.ReadASN1ObjectIdentifier(Ꮡattr.of(pkix.AttributeTypeAndValue.ᏑType))) {
                return (default!, errors.New("x509: invalid RDNSequence: invalid attribute type"u8));
            }
            cryptobyte.String rawValue = default!;
            ref var valueTag = ref heap(new vendor.golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var ᏑvalueTag);
            if (!atav.ReadAnyASN1(Ꮡ(rawValue), ᏑvalueTag)) {
                return (default!, errors.New("x509: invalid RDNSequence: invalid attribute value"u8));
            }
            error err = default!;
            (attr.Value, err) = parseASN1String(valueTag, rawValue);
            if (err != default!) {
                return (default!, fmt.Errorf("x509: invalid RDNSequence: invalid attribute value: %s"u8, err));
            }
            rdnSet = append(rdnSet, attr);
        }
        rdnSeq = append(rdnSeq, rdnSet);
    }
    return (Ꮡ(rdnSeq), default!);
}

internal static (pkix.AlgorithmIdentifier, error) parseAI(cryptobyte.String der) {
    ref var ai = ref heap<crypto.x509.pkix_package.AlgorithmIdentifier>(out var Ꮡai);
    ai = new pkix.AlgorithmIdentifier(nil);
    if (!der.ReadASN1ObjectIdentifier(Ꮡai.of(pkix.AlgorithmIdentifier.ᏑAlgorithm))) {
        return (ai, errors.New("x509: malformed OID"u8));
    }
    if (der.Empty()) {
        return (ai, default!);
    }
    cryptobyte.String @params = default!;
    ref var tag = ref heap(new vendor.golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var Ꮡtag);
    if (!der.ReadAnyASN1Element(Ꮡ(@params), Ꮡtag)) {
        return (ai, errors.New("x509: malformed parameters"u8));
    }
    ai.Parameters.Tag = ((nint)tag);
    ai.Parameters.FullBytes = @params;
    return (ai, default!);
}

internal static (time.Time, error) parseTime(ж<cryptobyte.String> Ꮡder) {
    ref var der = ref Ꮡder.val;

    ref var t = ref heap(new time_package.Time(), out var Ꮡt);
    switch (ᐧ) {
    case {} when der.PeekASN1Tag(cryptobyte_asn1.UTCTime): {
        if (!der.ReadASN1UTCTime(Ꮡt)) {
            return (t, errors.New("x509: malformed UTCTime"u8));
        }
        break;
    }
    case {} when der.PeekASN1Tag(cryptobyte_asn1.GeneralizedTime): {
        if (!der.ReadASN1GeneralizedTime(Ꮡt)) {
            return (t, errors.New("x509: malformed GeneralizedTime"u8));
        }
        break;
    }
    default: {
        return (t, errors.New("x509: unsupported time format"u8));
    }}

    return (t, default!);
}

internal static (time.Time, time.Time, error) parseValidity(cryptobyte.String der) {
    var (notBefore, err) = parseTime(Ꮡ(der));
    if (err != default!) {
        return (new time.Time(nil), new time.Time(nil), err);
    }
    var (notAfter, err) = parseTime(Ꮡ(der));
    if (err != default!) {
        return (new time.Time(nil), new time.Time(nil), err);
    }
    return (notBefore, notAfter, default!);
}

internal static (pkix.Extension, error) parseExtension(cryptobyte.String der) {
    ref var ext = ref heap(new crypto.x509.pkix_package.Extension(), out var Ꮡext);
    if (!der.ReadASN1ObjectIdentifier(Ꮡext.of(pkix.Extension.ᏑId))) {
        return (ext, errors.New("x509: malformed extension OID field"u8));
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(Ꮡext.of(pkix.Extension.ᏑCritical))) {
            return (ext, errors.New("x509: malformed extension critical field"u8));
        }
    }
    cryptobyte.String val = default!;
    if (!der.ReadASN1(Ꮡ(val), cryptobyte_asn1.OCTET_STRING)) {
        return (ext, errors.New("x509: malformed extension value field"u8));
    }
    ext.Value = val;
    return (ext, default!);
}

internal static (any, error) parsePublicKey(ж<publicKeyInfo> ᏑkeyData) {
    ref var keyData = ref ᏑkeyData.val;

    var oid = keyData.Algorithm.Algorithm;
    var @params = keyData.Algorithm.Parameters;
    var der = ((cryptobyte.String)keyData.PublicKey.RightAlign());
    switch (ᐧ) {
    case {} when oid.Equal(oidPublicKeyRSA): {
        if (!bytes.Equal(@params.FullBytes, // RSA public keys must have a NULL in the parameters.
 // See RFC 3279, Section 2.3.1.
 asn1.NullBytes)) {
            return (default!, errors.New("x509: RSA key missing NULL parameters"u8));
        }
        var p = Ꮡ(new pkcs1PublicKey(N: @new<bigꓸInt>()));
        if (!der.ReadASN1(Ꮡ(der), cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New("x509: invalid RSA public key"u8));
        }
        if (!der.ReadASN1Integer((~p).N)) {
            return (default!, errors.New("x509: invalid RSA modulus"u8));
        }
        if (!der.ReadASN1Integer(Ꮡ((~p).E))) {
            return (default!, errors.New("x509: invalid RSA public exponent"u8));
        }
        if ((~p).N.Sign() <= 0) {
            return (default!, errors.New("x509: RSA modulus is not a positive number"u8));
        }
        if ((~p).E <= 0) {
            return (default!, errors.New("x509: RSA public exponent is not a positive number"u8));
        }
        var pub = Ꮡ(new rsa.PublicKey(
            E: (~p).E,
            N: (~p).N
        ));
        return (pub, default!);
    }
    case {} when oid.Equal(oidPublicKeyECDSA): {
        var paramsDer = ((cryptobyte.String)@params.FullBytes);
        var namedCurveOID = @new<asn1.ObjectIdentifier>();
        if (!paramsDer.ReadASN1ObjectIdentifier(namedCurveOID)) {
            return (default!, errors.New("x509: invalid ECDSA parameters"u8));
        }
        var namedCurve = namedCurveFromOID(namedCurveOID.val);
        if (namedCurve == default!) {
            return (default!, errors.New("x509: unsupported elliptic curve"u8));
        }
        (x, y) = elliptic.Unmarshal(namedCurve, der);
        if (x == nil) {
            return (default!, errors.New("x509: failed to unmarshal elliptic curve point"u8));
        }
        var pub = Ꮡ(new ecdsa.PublicKey(
            Curve: namedCurve,
            X: x,
            Y: y
        ));
        return (pub, default!);
    }
    case {} when oid.Equal(oidPublicKeyEd25519): {
        if (len(@params.FullBytes) != 0) {
            // RFC 8410, Section 3
            // > For all of the OIDs, the parameters MUST be absent.
            return (default!, errors.New("x509: Ed25519 key encoded with illegal parameters"u8));
        }
        if (len(der) != ed25519.PublicKeySize) {
            return (default!, errors.New("x509: wrong Ed25519 public key size"u8));
        }
        return (((ed25519.PublicKey)der), default!);
    }
    case {} when oid.Equal(oidPublicKeyX25519): {
        if (len(@params.FullBytes) != 0) {
            // RFC 8410, Section 3
            // > For all of the OIDs, the parameters MUST be absent.
            return (default!, errors.New("x509: X25519 key encoded with illegal parameters"u8));
        }
        return ecdh.X25519().NewPublicKey(der);
    }
    case {} when oid.Equal(oidPublicKeyDSA): {
        var y = @new<bigꓸInt>();
        if (!der.ReadASN1Integer(y)) {
            return (default!, errors.New("x509: invalid DSA public key"u8));
        }
        var pub = Ꮡ(new dsa.PublicKey(
            Y: y,
            Parameters: new dsa.Parameters(
                P: @new<bigꓸInt>(),
                Q: @new<bigꓸInt>(),
                G: @new<bigꓸInt>()
            )
        ));
        var paramsDer = ((cryptobyte.String)@params.FullBytes);
        if (!paramsDer.ReadASN1(Ꮡ(paramsDer), cryptobyte_asn1.SEQUENCE) || !paramsDer.ReadASN1Integer((~pub).Parameters.P) || !paramsDer.ReadASN1Integer((~pub).Parameters.Q) || !paramsDer.ReadASN1Integer((~pub).Parameters.G)) {
            return (default!, errors.New("x509: invalid DSA parameters"u8));
        }
        if ((~pub).Y.Sign() <= 0 || (~pub).Parameters.P.Sign() <= 0 || (~pub).Parameters.Q.Sign() <= 0 || (~pub).Parameters.G.Sign() <= 0) {
            return (default!, errors.New("x509: zero or negative DSA parameter"u8));
        }
        return (pub, default!);
    }
    default: {
        return (default!, errors.New("x509: unknown public key algorithm"u8));
    }}

}

internal static (KeyUsage, error) parseKeyUsageExtension(cryptobyte.String der) {
    ref var usageBits = ref heap(new encoding.asn1_package.BitString(), out var ᏑusageBits);
    if (!der.ReadASN1BitString(ᏑusageBits)) {
        return (0, errors.New("x509: invalid key usage"u8));
    }
    nint usage = default!;
    for (nint i = 0; i < 9; i++) {
        if (usageBits.At(i) != 0) {
            usage |= (nint)(1 << (int)(((nuint)i)));
        }
    }
    return (((KeyUsage)usage), default!);
}

internal static (bool, nint, error) parseBasicConstraintsExtension(cryptobyte.String der) {
    ref var isCA = ref heap(new bool(), out var ᏑisCA);
    if (!der.ReadASN1(Ꮡ(der), cryptobyte_asn1.SEQUENCE)) {
        return (false, 0, errors.New("x509: invalid basic constraints"u8));
    }
    if (der.PeekASN1Tag(cryptobyte_asn1.BOOLEAN)) {
        if (!der.ReadASN1Boolean(ᏑisCA)) {
            return (false, 0, errors.New("x509: invalid basic constraints"u8));
        }
    }
    ref var maxPathLen = ref heap<nint>(out var ᏑmaxPathLen);
    maxPathLen = -1;
    if (der.PeekASN1Tag(cryptobyte_asn1.INTEGER)) {
        if (!der.ReadASN1Integer(ᏑmaxPathLen)) {
            return (false, 0, errors.New("x509: invalid basic constraints"u8));
        }
    }
    // TODO: map out.MaxPathLen to 0 if it has the -1 default value? (Issue 19285)
    return (isCA, maxPathLen, default!);
}

internal static error forEachSAN(cryptobyte.String der, Func<nint, slice<byte>, error> callback) {
    if (!der.ReadASN1(Ꮡ(der), cryptobyte_asn1.SEQUENCE)) {
        return errors.New("x509: invalid subject alternative names"u8);
    }
    while (!der.Empty()) {
        cryptobyte.String san = default!;
        ref var tag = ref heap(new vendor.golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var Ꮡtag);
        if (!der.ReadAnyASN1(Ꮡ(san), Ꮡtag)) {
            return errors.New("x509: invalid subject alternative name"u8);
        }
        {
            var err = callback(((nint)((asn1.Tag)(tag ^ 128))), san); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

internal static (slice<@string> dnsNames, slice<@string> emailAddresses, slice<net.IP> ipAddresses, slice<url.URL> uris, error err) parseSANExtension(cryptobyte.String der) {
    slice<@string> dnsNames = default!;
    slice<@string> emailAddresses = default!;
    slice<net.IP> ipAddresses = default!;
    slice<url.URL> uris = default!;
    error err = default!;

    err = forEachSAN(der, 
    var dnsNamesʗ1 = dnsNames;
    var emailAddressesʗ1 = emailAddresses;
    var ipAddressesʗ1 = ipAddresses;
    var urisʗ1 = uris;
    (nint tag, slice<byte> data) => {
        var exprᴛ1 = tag;
        if (exprᴛ1 == nameTypeEmail) {
            @string email = ((@string)data);
            {
                var errΔ5 = isIA5String(email); if (errΔ5 != default!) {
                    return errors.New("x509: SAN rfc822Name is malformed"u8);
                }
            }
            emailAddressesʗ1 = append(emailAddressesʗ1, email);
        }
        else if (exprᴛ1 == nameTypeDNS) {
            @string name = ((@string)data);
            {
                var errΔ6 = isIA5String(name); if (errΔ6 != default!) {
                    return errors.New("x509: SAN dNSName is malformed"u8);
                }
            }
            dnsNamesʗ1 = append(dnsNamesʗ1, ((@string)name));
        }
        else if (exprᴛ1 == nameTypeURI) {
            @string uriStr = ((@string)data);
            {
                var errΔ7 = isIA5String(uriStr); if (errΔ7 != default!) {
                    return errors.New("x509: SAN uniformResourceIdentifier is malformed"u8);
                }
            }
            (uri, errΔ8) = url.Parse(uriStr);
            if (errΔ8 != default!) {
                return fmt.Errorf("x509: cannot parse URI %q: %s"u8, uriStr, errΔ8);
            }
            if (len((~uri).Host) > 0) {
                {
                    var (_, ok) = domainToReverseLabels((~uri).Host); if (!ok) {
                        return fmt.Errorf("x509: cannot parse URI %q: invalid domain"u8, uriStr);
                    }
                }
            }
            urisʗ1 = append(urisʗ1, uri);
        }
        else if (exprᴛ1 == nameTypeIP) {
            switch (len(data)) {
            case net.IPv4len or net.IPv6len: {
                ipAddressesʗ1 = append(ipAddressesʗ1, data);
                break;
            }
            default: {
                return errors.New("x509: cannot parse IP address of length "u8 + strconv.Itoa(len(data)));
            }}

        }

        return default!;
    });
    return (dnsNames, emailAddresses, ipAddresses, uris, err);
}

internal static (slice<byte>, error) parseAuthorityKeyIdentifier(pkix.Extension e) {
    // RFC 5280, Section 4.2.1.1
    if (e.Critical) {
        // Conforming CAs MUST mark this extension as non-critical
        return (default!, errors.New("x509: authority key identifier incorrectly marked critical"u8));
    }
    var val = ((cryptobyte.String)e.Value);
    cryptobyte.String akid = default!;
    if (!val.ReadASN1(Ꮡ(akid), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: invalid authority key identifier"u8));
    }
    if (akid.PeekASN1Tag(((asn1.Tag)0).ContextSpecific())) {
        if (!akid.ReadASN1(Ꮡ(akid), ((asn1.Tag)0).ContextSpecific())) {
            return (default!, errors.New("x509: invalid authority key identifier"u8));
        }
        return (akid, default!);
    }
    return (default!, default!);
}

internal static (slice<ExtKeyUsage>, slice<asn1.ObjectIdentifier>, error) parseExtKeyUsageExtension(cryptobyte.String der) {
    slice<ExtKeyUsage> extKeyUsages = default!;
    slice<asn1.ObjectIdentifier> unknownUsages = default!;
    if (!der.ReadASN1(Ꮡ(der), cryptobyte_asn1.SEQUENCE)) {
        return (default!, default!, errors.New("x509: invalid extended key usages"u8));
    }
    while (!der.Empty()) {
        asn1.ObjectIdentifier eku = default!;
        if (!der.ReadASN1ObjectIdentifier(Ꮡ(eku))) {
            return (default!, default!, errors.New("x509: invalid extended key usages"u8));
        }
        {
            var (extKeyUsage, ok) = extKeyUsageFromOID(eku); if (ok){
                extKeyUsages = append(extKeyUsages, extKeyUsage);
            } else {
                unknownUsages = append(unknownUsages, eku);
            }
        }
    }
    return (extKeyUsages, unknownUsages, default!);
}

internal static (slice<OID>, error) parseCertificatePoliciesExtension(cryptobyte.String der) {
    slice<OID> oids = default!;
    if (!der.ReadASN1(Ꮡ(der), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: invalid certificate policies"u8));
    }
    while (!der.Empty()) {
        cryptobyte.String cp = default!;
        cryptobyte.String OIDBytes = default!;
        if (!der.ReadASN1(Ꮡ(cp), cryptobyte_asn1.SEQUENCE) || !cp.ReadASN1(Ꮡ(OIDBytes), cryptobyte_asn1.OBJECT_IDENTIFIER)) {
            return (default!, errors.New("x509: invalid certificate policies"u8));
        }
        var (oid, ok) = newOIDFromDER(OIDBytes);
        if (!ok) {
            return (default!, errors.New("x509: invalid certificate policies"u8));
        }
        oids = append(oids, oid);
    }
    return (oids, default!);
}

// isValidIPMask reports whether mask consists of zero or more 1 bits, followed by zero bits.
internal static bool isValidIPMask(slice<byte> mask) {
    var seenZero = false;
    foreach (var (_, b) in mask) {
        if (seenZero) {
            if (b != 0) {
                return false;
            }
            continue;
        }
        switch (b) {
        case 0 or 128 or 192 or 224 or 240 or 248 or 252 or 254: {
            seenZero = true;
            break;
        }
        case 255: {
            break;
        }
        default: {
            return false;
        }}

    }
    return true;
}

internal static (bool unhandled, error err) parseNameConstraintsExtension(ж<Certificate> Ꮡout, pkix.Extension e) {
    bool unhandled = default!;
    error err = default!;

    ref var @out = ref Ꮡout.val;
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
    var outer = ((cryptobyte.String)e.Value);
    cryptobyte.String toplevel = default!;
    cryptobyte.String permitted = default!;
    cryptobyte.String excluded = default!;
    ref var havePermitted = ref heap(new bool(), out var ᏑhavePermitted);
    ref var haveExcluded = ref heap(new bool(), out var ᏑhaveExcluded);
    if (!outer.ReadASN1(Ꮡ(toplevel), cryptobyte_asn1.SEQUENCE) || !outer.Empty() || !toplevel.ReadOptionalASN1(Ꮡ(permitted), ᏑhavePermitted, ((asn1.Tag)0).ContextSpecific().Constructed()) || !toplevel.ReadOptionalASN1(Ꮡ(excluded), ᏑhaveExcluded, ((asn1.Tag)1).ContextSpecific().Constructed()) || !toplevel.Empty()) {
        return (false, errors.New("x509: invalid NameConstraints extension"u8));
    }
    if (!havePermitted && !haveExcluded || len(permitted) == 0 && len(excluded) == 0) {
        // From RFC 5280, Section 4.2.1.10:
        //   “either the permittedSubtrees field
        //   or the excludedSubtrees MUST be
        //   present”
        return (false, errors.New("x509: empty name constraints extension"u8));
    }
    var getValues = 
    var dnsNamesʗ1 = dnsNames;
    var emailsʗ1 = emails;
    var ipsʗ1 = ips;
    var uriDomainsʗ1 = uriDomains;
    (cryptobyte.String subtrees) => {
        while (!subtrees.Empty()) {
            cryptobyte.String seq = default!;
            cryptobyte.String value = default!;
            ref var tag = ref heap(new vendor.golang.org.x.crypto.cryptobyte.asn1_package.Tag(), out var Ꮡtag);
            if (!subtrees.ReadASN1(Ꮡ(seq), cryptobyte_asn1.SEQUENCE) || !seq.ReadAnyASN1(Ꮡ(value), Ꮡtag)) {
                return (default!, default!, default!, default!, fmt.Errorf("x509: invalid NameConstraints extension"u8));
            }
            asn1.Tag dnsTag = ((asn1.Tag)2).ContextSpecific();
            asn1.Tag emailTag = ((asn1.Tag)1).ContextSpecific();
            asn1.Tag ipTag = ((asn1.Tag)7).ContextSpecific();
            asn1.Tag uriTag = ((asn1.Tag)6).ContextSpecific();
            var exprᴛ1 = tag;
            if (exprᴛ1 == dnsTag) {
                @string domain = ((@string)value);
                {
                    var errΔ5 = isIA5String(domain); if (errΔ5 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ5.Error()));
                    }
                }
                @string trimmedDomain = domain;
                if (len(trimmedDomain) > 0 && trimmedDomain[0] == (rune)'.') {
                    // constraints can have a leading
                    // period to exclude the domain
                    // itself, but that's not valid in a
                    // normal domain name.
                    trimmedDomain = trimmedDomain[1..];
                }
                {
                    var (_, ok) = domainToReverseLabels(trimmedDomain); if (!ok) {
                        return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse dnsName constraint %q"u8, domain));
                    }
                }
                dnsNamesʗ1 = append(dnsNamesʗ1, domain);
            }
            else if (exprᴛ1 == ipTag) {
                nint l = len(value);
                slice<byte> ip = default!;
                slice<byte> mask = default!;
                switch (l) {
                case 8: {
                    ip = value[..4];
                    mask = value[4..];
                    break;
                }
                case 32: {
                    ip = value[..16];
                    mask = value[16..];
                    break;
                }
                default: {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: IP constraint contained value of length %d"u8, l));
                }}

                if (!isValidIPMask(mask)) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: IP constraint contained invalid mask %x"u8, mask));
                }
                ipsʗ1 = append(ipsʗ1, Ꮡ(new net.IPNet(IP: ((net.IP)ip), Mask: ((net.IPMask)mask))));
            }
            else if (exprᴛ1 == emailTag) {
                @string constraint = ((@string)value);
                {
                    var errΔ6 = isIA5String(constraint); if (errΔ6 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ6.Error()));
                    }
                }
                if (strings.Contains(constraint, // If the constraint contains an @ then
 // it specifies an exact mailbox name.
 "@"u8)){
                    {
                        var (_, ok) = parseRFC2821Mailbox(constraint); if (!ok) {
                            return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse rfc822Name constraint %q"u8, constraint));
                        }
                    }
                } else {
                    // Otherwise it's a domain name.
                    @string domain = constraint;
                    if (len(domain) > 0 && domain[0] == (rune)'.') {
                        domain = domain[1..];
                    }
                    {
                        var (_, ok) = domainToReverseLabels(domain); if (!ok) {
                            return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse rfc822Name constraint %q"u8, constraint));
                        }
                    }
                }
                emailsʗ1 = append(emailsʗ1, constraint);
            }
            else if (exprᴛ1 == uriTag) {
                @string domain = ((@string)value);
                {
                    var errΔ7 = isIA5String(domain); if (errΔ7 != default!) {
                        return (default!, default!, default!, default!, errors.New("x509: invalid constraint value: "u8 + errΔ7.Error()));
                    }
                }
                if (net.ParseIP(domain) != default!) {
                    return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse URI constraint %q: cannot be IP address"u8, domain));
                }
                @string trimmedDomain = domain;
                if (len(trimmedDomain) > 0 && trimmedDomain[0] == (rune)'.') {
                    // constraints can have a leading
                    // period to exclude the domain itself,
                    // but that's not valid in a normal
                    // domain name.
                    trimmedDomain = trimmedDomain[1..];
                }
                {
                    var (_, ok) = domainToReverseLabels(trimmedDomain); if (!ok) {
                        return (default!, default!, default!, default!, fmt.Errorf("x509: failed to parse URI constraint %q"u8, domain));
                    }
                }
                uriDomainsʗ1 = append(uriDomainsʗ1, domain);
            }
            else { /* default: */
                unhandled = true;
            }

        }
        return (dnsNamesʗ1, ipsʗ1, emailsʗ1, uriDomainsʗ1, default!);
    };
    {
        var (@out.PermittedDNSDomains, @out.PermittedIPRanges, @out.PermittedEmailAddresses, @out.PermittedURIDomains, err) = getValues(permitted); if (err != default!) {
            return (false, err);
        }
    }
    {
        var (@out.ExcludedDNSDomains, @out.ExcludedIPRanges, @out.ExcludedEmailAddresses, @out.ExcludedURIDomains, err) = getValues(excluded); if (err != default!) {
            return (false, err);
        }
    }
    @out.PermittedDNSDomainsCritical = e.Critical;
    return (unhandled, default!);
}

internal static error processExtensions(ж<Certificate> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    error err = default!;
    foreach (var (_, e) in @out.Extensions) {
        var unhandled = false;
        if (len(e.Id) == 4 && e.Id[0] == 2 && e.Id[1] == 5 && e.Id[2] == 29){
            switch (e.Id[3]) {
            case 15: {
                (@out.KeyUsage, err) = parseKeyUsageExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 19: {
                (@out.IsCA, @out.MaxPathLen, err) = parseBasicConstraintsExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                @out.BasicConstraintsValid = true;
                @out.MaxPathLenZero = @out.MaxPathLen == 0;
                break;
            }
            case 17: {
                (@out.DNSNames, @out.EmailAddresses, @out.IPAddresses, @out.URIs, err) = parseSANExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                if (len(@out.DNSNames) == 0 && len(@out.EmailAddresses) == 0 && len(@out.IPAddresses) == 0 && len(@out.URIs) == 0) {
                    // If we didn't parse anything then we do the critical check, below.
                    unhandled = true;
                }
                break;
            }
            case 30: {
                (unhandled, err) = parseNameConstraintsExtension(Ꮡout, e);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 31: {
                var val = ((cryptobyte.String)e.Value);
                if (!val.ReadASN1(Ꮡ(val), // RFC 5280, 4.2.1.13
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
 cryptobyte_asn1.SEQUENCE)) {
                    return errors.New("x509: invalid CRL distribution points"u8);
                }
                while (!val.Empty()) {
                    cryptobyte.String dpDER = default!;
                    if (!val.ReadASN1(Ꮡ(dpDER), cryptobyte_asn1.SEQUENCE)) {
                        return errors.New("x509: invalid CRL distribution point"u8);
                    }
                    cryptobyte.String dpNameDER = default!;
                    ref var dpNamePresent = ref heap(new bool(), out var ᏑdpNamePresent);
                    if (!dpDER.ReadOptionalASN1(Ꮡ(dpNameDER), ᏑdpNamePresent, ((asn1.Tag)0).Constructed().ContextSpecific())) {
                        return errors.New("x509: invalid CRL distribution point"u8);
                    }
                    if (!dpNamePresent) {
                        continue;
                    }
                    if (!dpNameDER.ReadASN1(Ꮡ(dpNameDER), ((asn1.Tag)0).Constructed().ContextSpecific())) {
                        return errors.New("x509: invalid CRL distribution point"u8);
                    }
                    while (!dpNameDER.Empty()) {
                        if (!dpNameDER.PeekASN1Tag(((asn1.Tag)6).ContextSpecific())) {
                            break;
                        }
                        cryptobyte.String uri = default!;
                        if (!dpNameDER.ReadASN1(Ꮡ(uri), ((asn1.Tag)6).ContextSpecific())) {
                            return errors.New("x509: invalid CRL distribution point"u8);
                        }
                        @out.CRLDistributionPoints = append(@out.CRLDistributionPoints, ((@string)uri));
                    }
                }
                break;
            }
            case 35: {
                (@out.AuthorityKeyId, err) = parseAuthorityKeyIdentifier(e);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 37: {
                (@out.ExtKeyUsage, @out.UnknownExtKeyUsage, err) = parseExtKeyUsageExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                break;
            }
            case 14: {
                if (e.Critical) {
                    // RFC 5280, 4.2.1.2
                    // Conforming CAs MUST mark this extension as non-critical
                    return errors.New("x509: subject key identifier incorrectly marked critical"u8);
                }
                var val = ((cryptobyte.String)e.Value);
                cryptobyte.String skid = default!;
                if (!val.ReadASN1(Ꮡ(skid), cryptobyte_asn1.OCTET_STRING)) {
                    return errors.New("x509: invalid subject key identifier"u8);
                }
                @out.SubjectKeyId = skid;
                break;
            }
            case 32: {
                (@out.Policies, err) = parseCertificatePoliciesExtension(e.Value);
                if (err != default!) {
                    return err;
                }
                @out.PolicyIdentifiers = new slice<asn1.ObjectIdentifier>(0, len(@out.Policies));
                foreach (var (_, oid) in @out.Policies) {
                    {
                        var (oidΔ1, ok) = oid.toASN1OID(); if (ok) {
                            @out.PolicyIdentifiers = append(@out.PolicyIdentifiers, oidΔ1);
                        }
                    }
                }
                break;
            }
            default: {
                unhandled = true;
                break;
            }}

        } else 
        if (e.Id.Equal(oidExtensionAuthorityInfoAccess)){
            // Unknown extensions are recorded if critical.
            // RFC 5280 4.2.2.1: Authority Information Access
            if (e.Critical) {
                // Conforming CAs MUST mark this extension as non-critical
                return errors.New("x509: authority info access incorrectly marked critical"u8);
            }
            var val = ((cryptobyte.String)e.Value);
            if (!val.ReadASN1(Ꮡ(val), cryptobyte_asn1.SEQUENCE)) {
                return errors.New("x509: invalid authority info access"u8);
            }
            while (!val.Empty()) {
                cryptobyte.String aiaDER = default!;
                if (!val.ReadASN1(Ꮡ(aiaDER), cryptobyte_asn1.SEQUENCE)) {
                    return errors.New("x509: invalid authority info access"u8);
                }
                asn1.ObjectIdentifier method = default!;
                if (!aiaDER.ReadASN1ObjectIdentifier(Ꮡ(method))) {
                    return errors.New("x509: invalid authority info access"u8);
                }
                if (!aiaDER.PeekASN1Tag(((asn1.Tag)6).ContextSpecific())) {
                    continue;
                }
                if (!aiaDER.ReadASN1(Ꮡ(aiaDER), ((asn1.Tag)6).ContextSpecific())) {
                    return errors.New("x509: invalid authority info access"u8);
                }
                switch (ᐧ) {
                case {} when method.Equal(oidAuthorityInfoAccessOcsp): {
                    @out.OCSPServer = append(@out.OCSPServer, ((@string)aiaDER));
                    break;
                }
                case {} when method.Equal(oidAuthorityInfoAccessIssuers): {
                    @out.IssuingCertificateURL = append(@out.IssuingCertificateURL, ((@string)aiaDER));
                    break;
                }}

            }
        } else {
            // Unknown extensions are recorded if critical.
            unhandled = true;
        }
        if (e.Critical && unhandled) {
            @out.UnhandledCriticalExtensions = append(@out.UnhandledCriticalExtensions, e.Id);
        }
    }
    return default!;
}

internal static ж<godebug.Setting> x509negativeserial = godebug.New("x509negativeserial"u8);

internal static (ж<Certificate>, error) parseCertificate(slice<byte> der) {
    var cert = Ꮡ(new Certificate(nil));
    var input = ((cryptobyte.String)der);
    // we read the SEQUENCE including length and tag bytes so that
    // we can populate Certificate.Raw, before unwrapping the
    // SEQUENCE so it can be operated on
    if (!input.ReadASN1Element(Ꮡ(input), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed certificate"u8));
    }
    cert.val.Raw = input;
    if (!input.ReadASN1(Ꮡ(input), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed certificate"u8));
    }
    cryptobyte.String tbs = default!;
    // do the same trick again as above to extract the raw
    // bytes for Certificate.RawTBSCertificate
    if (!input.ReadASN1Element(Ꮡ(tbs), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed tbs certificate"u8));
    }
    cert.val.RawTBSCertificate = tbs;
    if (!tbs.ReadASN1(Ꮡ(tbs), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed tbs certificate"u8));
    }
    if (!tbs.ReadOptionalASN1Integer(Ꮡ((~cert).Version), ((asn1.Tag)0).Constructed().ContextSpecific(), 0)) {
        return (default!, errors.New("x509: malformed version"u8));
    }
    if ((~cert).Version < 0) {
        return (default!, errors.New("x509: malformed version"u8));
    }
    // for backwards compat reasons Version is one-indexed,
    // rather than zero-indexed as defined in 5280
    (~cert).Version++;
    if ((~cert).Version > 3) {
        return (default!, errors.New("x509: invalid version"u8));
    }
    var serial = @new<bigꓸInt>();
    if (!tbs.ReadASN1Integer(serial)) {
        return (default!, errors.New("x509: malformed serial number"u8));
    }
    if (serial.Sign() == -1) {
        if (x509negativeserial.Value() != "1"u8){
            return (default!, errors.New("x509: negative serial number"u8));
        } else {
            x509negativeserial.IncNonDefault();
        }
    }
    cert.val.SerialNumber = serial;
    cryptobyte.String sigAISeq = default!;
    if (!tbs.ReadASN1(Ꮡ(sigAISeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed signature algorithm identifier"u8));
    }
    // Before parsing the inner algorithm identifier, extract
    // the outer algorithm identifier and make sure that they
    // match.
    cryptobyte.String outerSigAISeq = default!;
    if (!input.ReadASN1(Ꮡ(outerSigAISeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed algorithm identifier"u8));
    }
    if (!bytes.Equal(outerSigAISeq, sigAISeq)) {
        return (default!, errors.New("x509: inner and outer signature algorithm identifiers don't match"u8));
    }
    var (sigAI, err) = parseAI(sigAISeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.val.SignatureAlgorithm = getSignatureAlgorithmFromAI(sigAI);
    cryptobyte.String issuerSeq = default!;
    if (!tbs.ReadASN1Element(Ꮡ(issuerSeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed issuer"u8));
    }
    cert.val.RawIssuer = issuerSeq;
    (issuerRDNs, err) = parseName(issuerSeq);
    if (err != default!) {
        return (default!, err);
    }
    (~cert).Issuer.FillFromRDNSequence(issuerRDNs);
    cryptobyte.String validity = default!;
    if (!tbs.ReadASN1(Ꮡ(validity), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed validity"u8));
    }
    (cert.val.NotBefore, cert.val.NotAfter, err) = parseValidity(validity);
    if (err != default!) {
        return (default!, err);
    }
    cryptobyte.String subjectSeq = default!;
    if (!tbs.ReadASN1Element(Ꮡ(subjectSeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed issuer"u8));
    }
    cert.val.RawSubject = subjectSeq;
    (subjectRDNs, err) = parseName(subjectSeq);
    if (err != default!) {
        return (default!, err);
    }
    (~cert).Subject.FillFromRDNSequence(subjectRDNs);
    cryptobyte.String spki = default!;
    if (!tbs.ReadASN1Element(Ꮡ(spki), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed spki"u8));
    }
    cert.val.RawSubjectPublicKeyInfo = spki;
    if (!spki.ReadASN1(Ꮡ(spki), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed spki"u8));
    }
    cryptobyte.String pkAISeq = default!;
    if (!spki.ReadASN1(Ꮡ(pkAISeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed public key algorithm identifier"u8));
    }
    (pkAI, err) = parseAI(pkAISeq);
    if (err != default!) {
        return (default!, err);
    }
    cert.val.PublicKeyAlgorithm = getPublicKeyAlgorithmFromOID(pkAI.Algorithm);
    ref var spk = ref heap(new encoding.asn1_package.BitString(), out var Ꮡspk);
    if (!spki.ReadASN1BitString(Ꮡspk)) {
        return (default!, errors.New("x509: malformed subjectPublicKey"u8));
    }
    if ((~cert).PublicKeyAlgorithm != UnknownPublicKeyAlgorithm) {
        (cert.val.PublicKey, err) = parsePublicKey(Ꮡ(new publicKeyInfo(
            Algorithm: pkAI,
            PublicKey: spk
        )));
        if (err != default!) {
            return (default!, err);
        }
    }
    if ((~cert).Version > 1) {
        if (!tbs.SkipOptionalASN1(((asn1.Tag)1).ContextSpecific())) {
            return (default!, errors.New("x509: malformed issuerUniqueID"u8));
        }
        if (!tbs.SkipOptionalASN1(((asn1.Tag)2).ContextSpecific())) {
            return (default!, errors.New("x509: malformed subjectUniqueID"u8));
        }
        if ((~cert).Version == 3) {
            cryptobyte.String extensions = default!;
            ref var present = ref heap(new bool(), out var Ꮡpresent);
            if (!tbs.ReadOptionalASN1(Ꮡ(extensions), Ꮡpresent, ((asn1.Tag)3).Constructed().ContextSpecific())) {
                return (default!, errors.New("x509: malformed extensions"u8));
            }
            if (present) {
                var seenExts = new map<@string, bool>();
                if (!extensions.ReadASN1(Ꮡ(extensions), cryptobyte_asn1.SEQUENCE)) {
                    return (default!, errors.New("x509: malformed extensions"u8));
                }
                while (!extensions.Empty()) {
                    cryptobyte.String extension = default!;
                    if (!extensions.ReadASN1(Ꮡ(extension), cryptobyte_asn1.SEQUENCE)) {
                        return (default!, errors.New("x509: malformed extension"u8));
                    }
                    var (ext, err) = parseExtension(extension);
                    if (err != default!) {
                        return (default!, err);
                    }
                    @string oidStr = ext.Id.String();
                    if (seenExts[oidStr]) {
                        return (default!, fmt.Errorf("x509: certificate contains duplicate extension with OID %q"u8, oidStr));
                    }
                    seenExts[oidStr] = true;
                    cert.val.Extensions = append((~cert).Extensions, ext);
                }
                err = processExtensions(cert);
                if (err != default!) {
                    return (default!, err);
                }
            }
        }
    }
    ref var signature = ref heap(new encoding.asn1_package.BitString(), out var Ꮡsignature);
    if (!input.ReadASN1BitString(Ꮡsignature)) {
        return (default!, errors.New("x509: malformed signature"u8));
    }
    cert.val.Signature = signature.RightAlign();
    return (cert, default!);
}

// ParseCertificate parses a single certificate from the given ASN.1 DER data.
//
// Before Go 1.23, ParseCertificate accepted certificates with negative serial
// numbers. This behavior can be restored by including "x509negativeserial=1" in
// the GODEBUG environment variable.
public static (ж<Certificate>, error) ParseCertificate(slice<byte> der) {
    (cert, err) = parseCertificate(der);
    if (err != default!) {
        return (default!, err);
    }
    if (len(der) != len((~cert).Raw)) {
        return (default!, errors.New("x509: trailing data"u8));
    }
    return (cert, err);
}

// ParseCertificates parses one or more certificates from the given ASN.1 DER
// data. The certificates must be concatenated with no intermediate padding.
public static (slice<ж<Certificate>>, error) ParseCertificates(slice<byte> der) {
    slice<ж<Certificate>> certs = default!;
    while (len(der) > 0) {
        (cert, err) = parseCertificate(der);
        if (err != default!) {
            return (default!, err);
        }
        certs = append(certs, cert);
        der = der[(int)(len((~cert).Raw))..];
    }
    return (certs, default!);
}

// The X.509 standards confusingly 1-indexed the version names, but 0-indexed
// the actual encoded version, so the version for X.509v2 is 1.
internal static readonly UntypedInt x509v2Version = 1;

// ParseRevocationList parses a X509 v2 [Certificate] Revocation List from the given
// ASN.1 DER data.
public static (ж<RevocationList>, error) ParseRevocationList(slice<byte> der) {
    var rl = Ꮡ(new RevocationList(nil));
    var input = ((cryptobyte.String)der);
    // we read the SEQUENCE including length and tag bytes so that
    // we can populate RevocationList.Raw, before unwrapping the
    // SEQUENCE so it can be operated on
    if (!input.ReadASN1Element(Ꮡ(input), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed crl"u8));
    }
    rl.val.Raw = input;
    if (!input.ReadASN1(Ꮡ(input), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed crl"u8));
    }
    cryptobyte.String tbs = default!;
    // do the same trick again as above to extract the raw
    // bytes for Certificate.RawTBSCertificate
    if (!input.ReadASN1Element(Ꮡ(tbs), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed tbs crl"u8));
    }
    rl.val.RawTBSRevocationList = tbs;
    if (!tbs.ReadASN1(Ꮡ(tbs), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed tbs crl"u8));
    }
    ref var version = ref heap(new nint(), out var Ꮡversion);
    if (!tbs.PeekASN1Tag(cryptobyte_asn1.INTEGER)) {
        return (default!, errors.New("x509: unsupported crl version"u8));
    }
    if (!tbs.ReadASN1Integer(Ꮡversion)) {
        return (default!, errors.New("x509: malformed crl"u8));
    }
    if (version != x509v2Version) {
        return (default!, fmt.Errorf("x509: unsupported crl version: %d"u8, version));
    }
    cryptobyte.String sigAISeq = default!;
    if (!tbs.ReadASN1(Ꮡ(sigAISeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed signature algorithm identifier"u8));
    }
    // Before parsing the inner algorithm identifier, extract
    // the outer algorithm identifier and make sure that they
    // match.
    cryptobyte.String outerSigAISeq = default!;
    if (!input.ReadASN1(Ꮡ(outerSigAISeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed algorithm identifier"u8));
    }
    if (!bytes.Equal(outerSigAISeq, sigAISeq)) {
        return (default!, errors.New("x509: inner and outer signature algorithm identifiers don't match"u8));
    }
    var (sigAI, err) = parseAI(sigAISeq);
    if (err != default!) {
        return (default!, err);
    }
    rl.val.SignatureAlgorithm = getSignatureAlgorithmFromAI(sigAI);
    ref var signature = ref heap(new encoding.asn1_package.BitString(), out var Ꮡsignature);
    if (!input.ReadASN1BitString(Ꮡsignature)) {
        return (default!, errors.New("x509: malformed signature"u8));
    }
    rl.val.Signature = signature.RightAlign();
    cryptobyte.String issuerSeq = default!;
    if (!tbs.ReadASN1Element(Ꮡ(issuerSeq), cryptobyte_asn1.SEQUENCE)) {
        return (default!, errors.New("x509: malformed issuer"u8));
    }
    rl.val.RawIssuer = issuerSeq;
    (issuerRDNs, err) = parseName(issuerSeq);
    if (err != default!) {
        return (default!, err);
    }
    (~rl).Issuer.FillFromRDNSequence(issuerRDNs);
    (rl.val.ThisUpdate, err) = parseTime(Ꮡ(tbs));
    if (err != default!) {
        return (default!, err);
    }
    if (tbs.PeekASN1Tag(cryptobyte_asn1.GeneralizedTime) || tbs.PeekASN1Tag(cryptobyte_asn1.UTCTime)) {
        (rl.val.NextUpdate, err) = parseTime(Ꮡ(tbs));
        if (err != default!) {
            return (default!, err);
        }
    }
    if (tbs.PeekASN1Tag(cryptobyte_asn1.SEQUENCE)) {
        cryptobyte.String revokedSeq = default!;
        if (!tbs.ReadASN1(Ꮡ(revokedSeq), cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New("x509: malformed crl"u8));
        }
        while (!revokedSeq.Empty()) {
            ref var rce = ref heap<RevocationListEntry>(out var Ꮡrce);
            rce = new RevocationListEntry(nil);
            cryptobyte.String certSeq = default!;
            if (!revokedSeq.ReadASN1Element(Ꮡ(certSeq), cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New("x509: malformed crl"u8));
            }
            rce.Raw = certSeq;
            if (!certSeq.ReadASN1(Ꮡ(certSeq), cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New("x509: malformed crl"u8));
            }
            rce.SerialNumber = @new<bigꓸInt>();
            if (!certSeq.ReadASN1Integer(rce.SerialNumber)) {
                return (default!, errors.New("x509: malformed serial number"u8));
            }
            (rce.RevocationTime, err) = parseTime(Ꮡ(certSeq));
            if (err != default!) {
                return (default!, err);
            }
            cryptobyte.String extensionsΔ1 = default!;
            ref var presentΔ1 = ref heap(new bool(), out var ᏑpresentΔ1);
            if (!certSeq.ReadOptionalASN1(Ꮡ(extensionsΔ1), ᏑpresentΔ1, cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New("x509: malformed extensions"u8));
            }
            if (presentΔ1) {
                while (!extensionsΔ1.Empty()) {
                    cryptobyte.String extensionΔ1 = default!;
                    if (!extensionsΔ1.ReadASN1(Ꮡ(extensionΔ1), cryptobyte_asn1.SEQUENCE)) {
                        return (default!, errors.New("x509: malformed extension"u8));
                    }
                    var (ext, err) = parseExtension(extensionΔ1);
                    if (err != default!) {
                        return (default!, err);
                    }
                    if (ext.Id.Equal(oidExtensionReasonCode)) {
                        var val = ((cryptobyte.String)ext.Value);
                        if (!val.ReadASN1Enum(Ꮡrce.of(RevocationListEntry.ᏑReasonCode))) {
                            return (default!, fmt.Errorf("x509: malformed reasonCode extension"u8));
                        }
                    }
                    rce.Extensions = append(rce.Extensions, ext);
                }
            }
            rl.val.RevokedCertificateEntries = append((~rl).RevokedCertificateEntries, rce);
            var rcDeprecated = new pkix.RevokedCertificate(
                SerialNumber: rce.SerialNumber,
                RevocationTime: rce.RevocationTime,
                Extensions: rce.Extensions
            );
            rl.val.RevokedCertificates = append((~rl).RevokedCertificates, rcDeprecated);
        }
    }
    cryptobyte.String extensions = default!;
    ref var present = ref heap(new bool(), out var Ꮡpresent);
    if (!tbs.ReadOptionalASN1(Ꮡ(extensions), Ꮡpresent, ((asn1.Tag)0).Constructed().ContextSpecific())) {
        return (default!, errors.New("x509: malformed extensions"u8));
    }
    if (present) {
        if (!extensions.ReadASN1(Ꮡ(extensions), cryptobyte_asn1.SEQUENCE)) {
            return (default!, errors.New("x509: malformed extensions"u8));
        }
        while (!extensions.Empty()) {
            cryptobyte.String extension = default!;
            if (!extensions.ReadASN1(Ꮡ(extension), cryptobyte_asn1.SEQUENCE)) {
                return (default!, errors.New("x509: malformed extension"u8));
            }
            var (ext, err) = parseExtension(extension);
            if (err != default!) {
                return (default!, err);
            }
            if (ext.Id.Equal(oidExtensionAuthorityKeyId)){
                (rl.val.AuthorityKeyId, err) = parseAuthorityKeyIdentifier(ext);
                if (err != default!) {
                    return (default!, err);
                }
            } else 
            if (ext.Id.Equal(oidExtensionCRLNumber)) {
                var value = ((cryptobyte.String)ext.Value);
                rl.val.Number = @new<bigꓸInt>();
                if (!value.ReadASN1Integer((~rl).Number)) {
                    return (default!, errors.New("x509: malformed crl number"u8));
                }
            }
            rl.val.Extensions = append((~rl).Extensions, ext);
        }
    }
    return (rl, default!);
}

} // end x509_package
