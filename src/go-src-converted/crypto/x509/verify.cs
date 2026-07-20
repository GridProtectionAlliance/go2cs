// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using pkix = go.crypto.x509.pkix_package;
using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using url = go.net.url_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using time = time_package;
using utf8 = go.unicode.utf8_package;
using asn1 = encoding.asn1_package;
using cryptobyte = vendor.golang.org.x.crypto.cryptobyte_package;
using encoding;
using go.crypto.x509;
using go.math;
using go.net;
using go.unicode;

partial class x509_package {

[GoType("num:nint")] partial struct InvalidReason;

public static readonly InvalidReason NotAuthorizedToSign = /* iota */ 0;
public static readonly InvalidReason Expired = 1;
public static readonly InvalidReason CANotAuthorizedForThisName = 2;
public static readonly InvalidReason TooManyIntermediates = 3;
public static readonly InvalidReason IncompatibleUsage = 4;
public static readonly InvalidReason NameMismatch = 5;
public static readonly InvalidReason NameConstraintsWithoutSANs = 6;
public static readonly InvalidReason UnconstrainedName = 7;
public static readonly InvalidReason TooManyConstraints = 8;
public static readonly InvalidReason CANotAuthorizedForExtKeyUsage = 9;

// CertificateInvalidError results when an odd error occurs. Users of this
// library probably want to handle all these errors uniformly.
[GoType] partial struct CertificateInvalidError {
    public ж<Certificate> Cert;
    public InvalidReason Reason;
    public @string Detail;
}

public static @string Error(this CertificateInvalidError e) {
    var exprᴛ1 = e.Reason;
    if (exprᴛ1 == NotAuthorizedToSign) {
        return "x509: certificate is not authorized to sign other certificates"u8;
    }
    if (exprᴛ1 == Expired) {
        return "x509: certificate has expired or is not yet valid: "u8 + e.Detail;
    }
    if (exprᴛ1 == CANotAuthorizedForThisName) {
        return "x509: a root or intermediate certificate is not authorized to sign for this name: "u8 + e.Detail;
    }
    if (exprᴛ1 == CANotAuthorizedForExtKeyUsage) {
        return "x509: a root or intermediate certificate is not authorized for an extended key usage: "u8 + e.Detail;
    }
    if (exprᴛ1 == TooManyIntermediates) {
        return "x509: too many intermediates for path length constraint"u8;
    }
    if (exprᴛ1 == IncompatibleUsage) {
        return "x509: certificate specifies an incompatible key usage"u8;
    }
    if (exprᴛ1 == NameMismatch) {
        return "x509: issuer name does not match subject from issuing certificate"u8;
    }
    if (exprᴛ1 == NameConstraintsWithoutSANs) {
        return "x509: issuer has name constraints but leaf doesn't have a SAN extension"u8;
    }
    if (exprᴛ1 == UnconstrainedName) {
        return "x509: issuer has name constraints but leaf contains unknown or unconstrained name: "u8 + e.Detail;
    }

    return "x509: unknown error"u8;
}

// HostnameError results when the set of authorized names doesn't match the
// requested name.
[GoType] partial struct HostnameError {
    public ж<Certificate> Certificate;
    public @string Host;
}

public static @string Error(this HostnameError h) {
    var c = h.Certificate;
    if (!c.hasSANExtension() && matchHostnames((~c).Subject.CommonName, h.Host)) {
        return "x509: certificate relies on legacy Common Name field, use SANs instead"u8;
    }
    @string valid = default!;
    {
        var ip = net.ParseIP(h.Host); if (ip != default!){
            // Trying to validate an IP
            if (builtin.len((~c).IPAddresses) == 0) {
                return "x509: cannot validate certificate for "u8 + h.Host + " because it doesn't contain any IP SANs"u8;
            }
            foreach (var (_, san) in (~c).IPAddresses) {
                if (builtin.len(valid) > 0) {
                    valid += ", "u8;
                }
                valid += san.String();
            }
        } else {
            valid = strings.Join((~c).DNSNames, ", "u8);
        }
    }
    if (builtin.len(valid) == 0) {
        return "x509: certificate is not valid for any names, but wanted to match "u8 + h.Host;
    }
    return "x509: certificate is valid for "u8 + valid + ", not "u8 + h.Host;
}

// UnknownAuthorityError results when the certificate issuer is unknown
[GoType] partial struct UnknownAuthorityError {
    public ж<Certificate> Cert;
    // hintErr contains an error that may be helpful in determining why an
    // authority wasn't found.
    internal error hintErr;
    // hintCert contains a possible authority certificate that was rejected
    // because of the error in hintErr.
    internal ж<Certificate> hintCert;
}

public static @string Error(this UnknownAuthorityError e) {
    @string s = "x509: certificate signed by unknown authority"u8;
    if (e.hintErr != default!) {
        @string certName = e.hintCert.Value.Subject.CommonName;
        if (builtin.len(certName) == 0) {
            if (builtin.len((~e.hintCert).Subject.Organization) > 0){
                certName = (~e.hintCert).Subject.Organization[0];
            } else {
                certName = "serial:"u8 + (~e.hintCert).SerialNumber.String();
            }
        }
        s += fmt.Sprintf(" (possibly because of %q while trying to verify candidate authority certificate %q)"u8, e.hintErr, certName);
    }
    return s;
}

// SystemRootsError results when we fail to load the system root certificates.
[GoType] partial struct SystemRootsError {
    public error Err;
}

public static @string Error(this SystemRootsError se) {
    @string msg = "x509: failed to load system roots and no roots provided"u8;
    if (se.Err != default!) {
        return msg + "; "u8 + se.Err.Error();
    }
    return msg;
}

public static error Unwrap(this SystemRootsError se) {
    return se.Err;
}

// errNotParsed is returned when a certificate without ASN.1 contents is
// verified. Platform-specific verification needs the ASN.1 contents.
internal static error errNotParsed = errors.New("x509: missing ASN.1 contents; use ParseCertificate"u8);

// VerifyOptions contains parameters for Certificate.Verify.
[GoType] partial struct VerifyOptions {
    // DNSName, if set, is checked against the leaf certificate with
    // Certificate.VerifyHostname or the platform verifier.
    public @string DNSName;
    // Intermediates is an optional pool of certificates that are not trust
    // anchors, but can be used to form a chain from the leaf certificate to a
    // root certificate.
    public ж<CertPool> Intermediates;
    // Roots is the set of trusted root certificates the leaf certificate needs
    // to chain up to. If nil, the system roots or the platform verifier are used.
    public ж<CertPool> Roots;
    // CurrentTime is used to check the validity of all certificates in the
    // chain. If zero, the current time is used.
    public time.Time CurrentTime;
    // KeyUsages specifies which Extended Key Usage values are acceptable. A
    // chain is accepted if it allows any of the listed values. An empty list
    // means ExtKeyUsageServerAuth. To accept any key usage, include ExtKeyUsageAny.
    public slice<ExtKeyUsage> KeyUsages;
    // MaxConstraintComparisions is the maximum number of comparisons to
    // perform when checking a given certificate's name constraints. If
    // zero, a sensible default is used. This limit prevents pathological
    // certificates from consuming excessive amounts of CPU time when
    // validating. It does not apply to the platform verifier.
    public nint MaxConstraintComparisions;
}

internal static readonly UntypedInt leafCertificate = iota;
internal static readonly UntypedInt intermediateCertificate = 1;
internal static readonly UntypedInt rootCertificate = 2;

// rfc2821Mailbox represents a “mailbox” (which is an email address to most
// people) by breaking it into the “local” (i.e. before the '@') and “domain”
// parts.
[GoType] partial struct rfc2821Mailbox {
    internal @string local, domain;
}

// parseRFC2821Mailbox parses an email address into local and domain parts,
// based on the ABNF for a “Mailbox” from RFC 2821. According to RFC 5280,
// Section 4.2.1.6 that's correct for an rfc822Name from a certificate: “The
// format of an rfc822Name is a "Mailbox" as defined in RFC 2821, Section 4.1.2”.
internal static (rfc2821Mailbox mailbox, bool ok) parseRFC2821Mailbox(@string @in) {
    rfc2821Mailbox mailbox = default!;
    bool ok = default!;

    if (builtin.len(@in) == 0) {
        return (mailbox, false);
    }
    var localPartBytes = new slice<byte>(0, builtin.len(@in) / 2);
    if (@in[0] == (rune)'"'){
        // Quoted-string = DQUOTE *qcontent DQUOTE
        // non-whitespace-control = %d1-8 / %d11 / %d12 / %d14-31 / %d127
        // qcontent = qtext / quoted-pair
        // qtext = non-whitespace-control /
        //         %d33 / %d35-91 / %d93-126
        // quoted-pair = ("\" text) / obs-qp
        // text = %d1-9 / %d11 / %d12 / %d14-127 / obs-text
        //
        // (Names beginning with “obs-” are the obsolete syntax from RFC 2822,
        // Section 4. Since it has been 16 years, we no longer accept that.)
        @in = @in[1..];
QuotedString:
        while (ᐧ) {
            if (builtin.len(@in) == 0) {
                return (mailbox, false);
            }
            var c = @in[0];
            @in = @in[1..];
            switch (ᐧ) {
            case {} when c is (rune)'"': {
                goto break_QuotedString;
                break;
            }
            case {} when c is (rune)'\\': {
                if (builtin.len(@in) == 0) {
                    // quoted-pair
                    return (mailbox, false);
                }
                if (@in[0] == 11 || @in[0] == 12 || (1 <= @in[0] && @in[0] <= 9) || (14 <= @in[0] && @in[0] <= 127)){
                    localPartBytes = append(localPartBytes, @in[0]);
                    @in = @in[1..];
                } else {
                    return (mailbox, false);
                }
                break;
            }
            case {} when c == 11 || c == 12 || c == 32 || c == 33 || c == 127 || (1 <= c && c <= 8) || (14 <= c && c <= 31) || (35 <= c && c <= 91) || (93 <= c && c <= 126): {
                localPartBytes = append(localPartBytes, // Space (char 32) is not allowed based on the
 // BNF, but RFC 3696 gives an example that
 // assumes that it is. Several “verified”
 // errata continue to argue about this point.
 // We choose to accept it.
 // qtext
 c);
                break;
            }
            default: {
                return (mailbox, false);
            }}

continue_QuotedString:;
        }
break_QuotedString:;
    } else {
        // Atom ("." Atom)*
NextChar:
        while (builtin.len(@in) > 0) {
            // atext from RFC 2822, Section 3.2.4
            var c = @in[0];
            var matchᴛ1 = false;
            if (c is (rune)'\\') { matchᴛ1 = true;
                @in = @in[1..];
                if (builtin.len(@in) == 0) {
                    // Examples given in RFC 3696 suggest that
                    // escaped characters can appear outside of a
                    // quoted string. Several “verified” errata
                    // continue to argue the point. We choose to
                    // accept it.
                    return (mailbox, false);
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && (((rune)'0' <= c && c <= (rune)'9') || ((rune)'a' <= c && c <= (rune)'z') || ((rune)'A' <= c && c <= (rune)'Z') || c == (rune)'!' || c == (rune)'#' || c == (rune)'$' || c == (rune)'%' || c == (rune)'&' || c == (rune)'\'' || c == (rune)'*' || c == (rune)'+' || c == (rune)'-' || c == (rune)'/' || c == (rune)'=' || c == (rune)'?' || c == (rune)'^' || c == (rune)'_' || c == (rune)'`' || c == (rune)'{' || c == (rune)'|' || c == (rune)'}' || c == (rune)'~' || c == (rune)'.')) {
                localPartBytes = append(localPartBytes, @in[0]);
                @in = @in[1..];
            }
            else if (!matchᴛ1) { /* default: */
                goto break_NextChar;
            }

continue_NextChar:;
        }
break_NextChar:;
        if (builtin.len(localPartBytes) == 0) {
            return (mailbox, false);
        }
        // From RFC 3696, Section 3:
        // “period (".") may also appear, but may not be used to start
        // or end the local part, nor may two or more consecutive
        // periods appear.”
        var twoDots = new byte[]{(rune)'.', (rune)'.'}.slice();
        if (localPartBytes[0] == (rune)'.' || localPartBytes[builtin.len(localPartBytes) - 1] == (rune)'.' || bytes.Contains(localPartBytes, twoDots)) {
            return (mailbox, false);
        }
    }
    if (builtin.len(@in) == 0 || @in[0] != (rune)'@') {
        return (mailbox, false);
    }
    @in = @in[1..];
    // The RFC species a format for domains, but that's known to be
    // violated in practice so we accept that anything after an '@' is the
    // domain part.
    {
        var (_, okΔ1) = domainToReverseLabels(@in); if (!okΔ1) {
            return (mailbox, false);
        }
    }
    mailbox.local = ((@string)localPartBytes);
    mailbox.domain = @in;
    return (mailbox, true);
}

// domainToReverseLabels converts a textual domain name like foo.example.com to
// the list of labels in reverse order, e.g. ["com", "example", "foo"].
internal static (slice<@string> reverseLabels, bool ok) domainToReverseLabels(@string domain) {
    slice<@string> reverseLabels = default!;
    bool ok = default!;

    while (builtin.len(domain) > 0) {
        {
            nint i = strings.LastIndexByte(domain, (rune)'.'); if (i == -1){
                reverseLabels = append(reverseLabels, domain);
                domain = ""u8;
            } else {
                reverseLabels = append(reverseLabels, domain[(int)(i + 1)..]);
                domain = domain[..(int)(i)];
                if (i == 0) {
                    // domain == ""
                    // domain is prefixed with an empty label, append an empty
                    // string to reverseLabels to indicate this.
                    reverseLabels = append(reverseLabels, ""u8);
                }
            }
        }
    }
    if (builtin.len(reverseLabels) > 0 && builtin.len(reverseLabels[0]) == 0) {
        // An empty label at the end indicates an absolute value.
        return (default!, false);
    }
    foreach (var (_, label) in reverseLabels) {
        if (builtin.len(label) == 0) {
            // Empty labels are otherwise invalid.
            return (default!, false);
        }
        foreach (var (_, c) in label) {
            if (c < 33 || c > 126) {
                // Invalid character.
                return (default!, false);
            }
        }
    }
    return (reverseLabels, true);
}

internal static (bool, error) matchEmailConstraint(rfc2821Mailbox mailbox, @string constraint) {
    // If the constraint contains an @, then it specifies an exact mailbox
    // name.
    if (strings.Contains(constraint, "@"u8)) {
        var (constraintMailbox, ok) = parseRFC2821Mailbox(constraint);
        if (!ok) {
            return (false, fmt.Errorf("x509: internal error: cannot parse constraint %q"u8, constraint));
        }
        return (mailbox.local == constraintMailbox.local && strings.EqualFold(mailbox.domain, constraintMailbox.domain), default!);
    }
    // Otherwise the constraint is like a DNS constraint of the domain part
    // of the mailbox.
    return matchDomainConstraint(mailbox.domain, constraint);
}

internal static (bool, error) matchURIConstraint(ж<url.URL> Ꮡuri, @string constraint) {
    ref var uri = ref Ꮡuri.Value;

    // From RFC 5280, Section 4.2.1.10:
    // “a uniformResourceIdentifier that does not include an authority
    // component with a host name specified as a fully qualified domain
    // name (e.g., if the URI either does not include an authority
    // component or includes an authority component in which the host name
    // is specified as an IP address), then the application MUST reject the
    // certificate.”
    @string host = uri.Host;
    if (builtin.len(host) == 0) {
        return (false, fmt.Errorf("URI with empty host (%q) cannot be matched against constraints"u8, uri.String()));
    }
    if (strings.Contains(host, ":"u8) && !strings.HasSuffix(host, "]"u8)) {
        error err = default!;
        (host, _, err) = net.SplitHostPort(uri.Host);
        if (err != default!) {
            return (false, err);
        }
    }
    if (strings.HasPrefix(host, "["u8) && strings.HasSuffix(host, "]"u8) || net.ParseIP(host) != default!) {
        return (false, fmt.Errorf("URI with IP (%q) cannot be matched against constraints"u8, uri.String()));
    }
    return matchDomainConstraint(host, constraint);
}

internal static (bool, error) matchIPConstraint(net.IP ip, ж<net.IPNet> Ꮡconstraint) {
    ref var constraint = ref Ꮡconstraint.Value;

    if (builtin.len(ip) != builtin.len(constraint.IP)) {
        return (false, default!);
    }
    foreach (var (i, _) in ip) {
        {
            var mask = constraint.Mask[i]; if ((byte)(ip[i] & mask) != (byte)(constraint.IP[i] & mask)) {
                return (false, default!);
            }
        }
    }
    return (true, default!);
}

internal static (bool, error) matchDomainConstraint(@string domain, @string constraint) {
    // The meaning of zero length constraints is not specified, but this
    // code follows NSS and accepts them as matching everything.
    if (builtin.len(constraint) == 0) {
        return (true, default!);
    }
    var (domainLabels, ok) = domainToReverseLabels(domain);
    if (!ok) {
        return (false, fmt.Errorf("x509: internal error: cannot parse domain %q"u8, domain));
    }
    // RFC 5280 says that a leading period in a domain name means that at
    // least one label must be prepended, but only for URI and email
    // constraints, not DNS constraints. The code also supports that
    // behaviour for DNS constraints.
    var mustHaveSubdomains = false;
    if (constraint[0] == (rune)'.') {
        mustHaveSubdomains = true;
        constraint = constraint[1..];
    }
    (var constraintLabels, ok) = domainToReverseLabels(constraint);
    if (!ok) {
        return (false, fmt.Errorf("x509: internal error: cannot parse domain %q"u8, constraint));
    }
    if (builtin.len(domainLabels) < builtin.len(constraintLabels) || (mustHaveSubdomains && builtin.len(domainLabels) == builtin.len(constraintLabels))) {
        return (false, default!);
    }
    foreach (var (i, constraintLabel) in constraintLabels) {
        if (!strings.EqualFold(constraintLabel, domainLabels[i])) {
            return (false, default!);
        }
    }
    return (true, default!);
}

// checkNameConstraints checks that c permits a child certificate to claim the
// given name, of type nameType. The argument parsedName contains the parsed
// form of name, suitable for passing to the match function. The total number
// of comparisons is tracked in the given count and should not exceed the given
// limit.
internal static error checkNameConstraints(this ж<Certificate> Ꮡc, ж<nint> Ꮡcount, nint maxConstraintComparisons, @string nameType, @string name, any parsedName, Func<any, any, (bool, error)> match, any permitted, any excluded) {
    ref var count = ref Ꮡcount.Value;

    var excludedValue = reflect.ValueOf(excluded);
    count += excludedValue.Len();
    if (count > maxConstraintComparisons) {
        return new CertificateInvalidError(Ꮡc, TooManyConstraints, "");
    }
    for (nint i = 0; i < excludedValue.Len(); i++) {
        var constraint = excludedValue.Index(i).Interface();
        var (matchΔ1, err) = match(parsedName, constraint);
        if (err != default!) {
            return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, err.Error());
        }
        if (matchΔ1) {
            return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, fmt.Sprintf("%s %q is excluded by constraint %q"u8, nameType, name, constraint));
        }
    }
    var permittedValue = reflect.ValueOf(permitted);
    count += permittedValue.Len();
    if (count > maxConstraintComparisons) {
        return new CertificateInvalidError(Ꮡc, TooManyConstraints, "");
    }
    var ok = true;
    for (nint i = 0; i < permittedValue.Len(); i++) {
        var constraint = permittedValue.Index(i).Interface();
        error err = default!;
        {
            (ok, err) = match(parsedName, constraint); if (err != default!) {
                return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, err.Error());
            }
        }
        if (ok) {
            break;
        }
    }
    if (!ok) {
        return new CertificateInvalidError(Ꮡc, CANotAuthorizedForThisName, fmt.Sprintf("%s %q is not permitted by any constraint"u8, nameType, name));
    }
    return default!;
}

// isValid performs validity checks on c given that it is a candidate to append
// to the chain in currentChain.
internal static error isValid(this ж<Certificate> Ꮡc, nint certType, slice<ж<Certificate>> currentChain, ж<VerifyOptions> Ꮡopts) {
    ref var c = ref Ꮡc.Value;
    ref var opts = ref Ꮡopts.Value;

    if (builtin.len(c.UnhandledCriticalExtensions) > 0) {
        return new UnhandledCriticalExtension(nil);
    }
    if (builtin.len(currentChain) > 0) {
        var child = currentChain[builtin.len(currentChain) - 1];
        if (!bytes.Equal((~child).RawIssuer, c.RawSubject)) {
            return new CertificateInvalidError(Ꮡc, NameMismatch, "");
        }
    }
    var now = opts.CurrentTime;
    if (now.IsZero()) {
        now = time.Now();
    }
    if (now.Before(c.NotBefore)){
        return new CertificateInvalidError(
            Cert: Ꮡc,
            Reason: Expired,
            Detail: fmt.Sprintf("current time %s is before %s"u8, now.Format(time.RFC3339), c.NotBefore.Format(time.RFC3339))
        );
    } else 
    if (now.After(c.NotAfter)) {
        return new CertificateInvalidError(
            Cert: Ꮡc,
            Reason: Expired,
            Detail: fmt.Sprintf("current time %s is after %s"u8, now.Format(time.RFC3339), c.NotAfter.Format(time.RFC3339))
        );
    }
    nint maxConstraintComparisons = opts.MaxConstraintComparisions;
    if (maxConstraintComparisons == 0) {
        maxConstraintComparisons = 250000;
    }
    ref var comparisonCount = ref heap<nint>(out var ᏑcomparisonCount);
    comparisonCount = 0;
    if (certType == intermediateCertificate || certType == rootCertificate) {
        if (builtin.len(currentChain) == 0) {
            return errors.New("x509: internal error: empty chain when appending CA cert"u8);
        }
    }
    if ((certType == intermediateCertificate || certType == rootCertificate) && c.hasNameConstraints()) {
        var toCheck = new ж<Certificate>[]{}.slice();
        foreach (var (_, cΔ1) in currentChain) {
            if (cΔ1.hasSANExtension()) {
                toCheck = append(toCheck, cΔ1);
            }
        }
        foreach (var (_, sanCert) in toCheck) {
            var err = forEachSAN(sanCert.getSANExtension(), error (nint tag, slice<byte> data) => {
                var exprᴛ1 = tag;
                if (exprᴛ1 == nameTypeEmail) {
                    @string name = ((@string)data);
                    ref var mailbox = ref heap<rfc2821Mailbox>(out var Ꮡmailbox);
                    (mailbox, var ok) = parseRFC2821Mailbox(name);
                    if (!ok) {
                        return fmt.Errorf("x509: cannot parse rfc822Name %q"u8, mailbox);
                    }
                    {
                        var errΔ6 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, "email address"u8, name, mailbox,
                            (any parsedName, any constraint) => matchEmailConstraint(parsedName._<rfc2821Mailbox>(), constraint._<@string>()), Ꮡc.Value.PermittedEmailAddresses, Ꮡc.Value.ExcludedEmailAddresses); if (errΔ6 != default!) {
                            return errΔ6;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeDNS) {
                    @string name = ((@string)data);
                    {
                        var (_, ok) = domainToReverseLabels(name); if (!ok) {
                            return fmt.Errorf("x509: cannot parse dnsName %q"u8, name);
                        }
                    }
                    {
                        var errΔ7 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, "DNS name"u8, name, name,
                            (any parsedName, any constraint) => matchDomainConstraint(parsedName._<@string>(), constraint._<@string>()), Ꮡc.Value.PermittedDNSDomains, Ꮡc.Value.ExcludedDNSDomains); if (errΔ7 != default!) {
                            return errΔ7;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeURI) {
                    @string name = ((@string)data);
                    var (uri, errΔ8) = url.Parse(name);
                    if (errΔ8 != default!) {
                        return fmt.Errorf("x509: internal error: URI SAN %q failed to parse"u8, name);
                    }
                    {
                        var errΔ9 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, "URI"u8, name, uri,
                            (any parsedName, any constraint) => matchURIConstraint(parsedName._<ж<url.URL>>(), constraint._<@string>()), Ꮡc.Value.PermittedURIDomains, Ꮡc.Value.ExcludedURIDomains); if (errΔ9 != default!) {
                            return errΔ9;
                        }
                    }
                }
                else if (exprᴛ1 == nameTypeIP) {
                    var ip = ((net.IP)data);
                    {
                        nint l = builtin.len(ip); if (l != net.IPv4len && l != net.IPv6len) {
                            return fmt.Errorf("x509: internal error: IP SAN %x failed to parse"u8, data);
                        }
                    }
                    {
                        var errΔ10 = Ꮡc.checkNameConstraints(ᏑcomparisonCount, maxConstraintComparisons, "IP address"u8, ip.String(), ip,
                            (any parsedName, any constraint) => matchIPConstraint(parsedName._<net.IP>(), constraint._<ж<net.IPNet>>()), Ꮡc.Value.PermittedIPRanges, Ꮡc.Value.ExcludedIPRanges); if (errΔ10 != default!) {
                            return errΔ10;
                        }
                    }
                }
                else { /* default: */
                }

                // Unknown SAN types are ignored.
                return default!;
            });
            if (err != default!) {
                return err;
            }
        }
    }
    // KeyUsage status flags are ignored. From Engineering Security, Peter
    // Gutmann: A European government CA marked its signing certificates as
    // being valid for encryption only, but no-one noticed. Another
    // European CA marked its signature keys as not being valid for
    // signatures. A different CA marked its own trusted root certificate
    // as being invalid for certificate signing. Another national CA
    // distributed a certificate to be used to encrypt data for the
    // country’s tax authority that was marked as only being usable for
    // digital signatures but not for encryption. Yet another CA reversed
    // the order of the bit flags in the keyUsage due to confusion over
    // encoding endianness, essentially setting a random keyUsage in
    // certificates that it issued. Another CA created a self-invalidating
    // certificate by adding a certificate policy statement stipulating
    // that the certificate had to be used strictly as specified in the
    // keyUsage, and a keyUsage containing a flag indicating that the RSA
    // encryption key could only be used for Diffie-Hellman key agreement.
    if (certType == intermediateCertificate && (!c.BasicConstraintsValid || !c.IsCA)) {
        return new CertificateInvalidError(Ꮡc, NotAuthorizedToSign, "");
    }
    if (c.BasicConstraintsValid && c.MaxPathLen >= 0) {
        nint numIntermediates = builtin.len(currentChain) - 1;
        if (numIntermediates > c.MaxPathLen) {
            return new CertificateInvalidError(Ꮡc, TooManyIntermediates, "");
        }
    }
    if (!boringAllowCert(Ꮡc)) {
        // IncompatibleUsage is not quite right here,
        // but it's also the "no chains found" error
        // and is close enough.
        return new CertificateInvalidError(Ꮡc, IncompatibleUsage, "");
    }
    return default!;
}

// Verify attempts to verify c by building one or more chains from c to a
// certificate in opts.Roots, using certificates in opts.Intermediates if
// needed. If successful, it returns one or more chains where the first
// element of the chain is c and the last element is from opts.Roots.
//
// If opts.Roots is nil, the platform verifier might be used, and
// verification details might differ from what is described below. If system
// roots are unavailable the returned error will be of type SystemRootsError.
//
// Name constraints in the intermediates will be applied to all names claimed
// in the chain, not just opts.DNSName. Thus it is invalid for a leaf to claim
// example.com if an intermediate doesn't permit it, even if example.com is not
// the name being validated. Note that DirectoryName constraints are not
// supported.
//
// Name constraint validation follows the rules from RFC 5280, with the
// addition that DNS name constraints may use the leading period format
// defined for emails and URIs. When a constraint has a leading period
// it indicates that at least one additional label must be prepended to
// the constrained name to be considered valid.
//
// Extended Key Usage values are enforced nested down a chain, so an intermediate
// or root that enumerates EKUs prevents a leaf from asserting an EKU not in that
// list. (While this is not specified, it is common practice in order to limit
// the types of certificates a CA can issue.)
//
// Certificates that use SHA1WithRSA and ECDSAWithSHA1 signatures are not supported,
// and will not be used to build chains.
//
// Certificates other than c in the returned chains should not be modified.
//
// WARNING: this function doesn't do any revocation checking.
public static (slice<slice<ж<Certificate>>> chains, error err) Verify(this ж<Certificate> Ꮡc, VerifyOptions opts) {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;

    ref var c = ref Ꮡc.Value;
    // Platform-specific verification needs the ASN.1 contents so
    // this makes the behavior consistent across platforms.
    if (builtin.len(c.Raw) == 0) {
        return (default!, errNotParsed);
    }
    for (nint i = 0; i < opts.Intermediates.len(); i++) {
        var (cΔ1, _, errΔ1) = opts.Intermediates.cert(i);
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("crypto/x509: error fetching intermediate: %w"u8, errΔ1));
        }
        if (builtin.len((~cΔ1).Raw) == 0) {
            return (default!, errNotParsed);
        }
    }
    // Use platform verifiers, where available, if Roots is from SystemCertPool.
    if (runtime.GOOS == "windows"u8 || runtime.GOOS == "darwin"u8 || runtime.GOOS == "ios"u8) {
        // Don't use the system verifier if the system pool was replaced with a non-system pool,
        // i.e. if SetFallbackRoots was called with x509usefallbackroots=1.
        var systemPool = systemRootsPool();
        if (opts.Roots == nil && (systemPool == nil || (~systemPool).systemPool)) {
            return Ꮡc.systemVerify(Ꮡ(opts));
        }
        if (opts.Roots != nil && (~opts.Roots).systemPool) {
            var (platformChains, errΔ2) = Ꮡc.systemVerify(Ꮡ(opts));
            // If the platform verifier succeeded, or there are no additional
            // roots, return the platform verifier result. Otherwise, continue
            // with the Go verifier.
            if (errΔ2 == default! || opts.Roots.len() == 0) {
                return (platformChains, errΔ2);
            }
        }
    }
    if (opts.Roots == nil) {
        opts.Roots = systemRootsPool();
        if (opts.Roots == nil) {
            return (default!, new SystemRootsError(systemRootsErr));
        }
    }
    err = Ꮡc.isValid(leafCertificate, default!, Ꮡ(opts));
    if (err != default!) {
        return (chains, err);
    }
    if (builtin.len(opts.DNSName) > 0) {
        err = Ꮡc.VerifyHostname(opts.DNSName);
        if (err != default!) {
            return (chains, err);
        }
    }
    slice<slice<ж<Certificate>>> candidateChains = default!;
    if (opts.Roots.contains(Ꮡc)){
        candidateChains = new slice<ж<Certificate>>[]{new ж<Certificate>[]{Ꮡc}.slice()}.slice();
    } else {
        (candidateChains, err) = Ꮡc.buildChains(new ж<Certificate>[]{Ꮡc}.slice(), nil, Ꮡ(opts));
        if (err != default!) {
            return (default!, err);
        }
    }
    if (builtin.len(opts.KeyUsages) == 0) {
        opts.KeyUsages = new ExtKeyUsage[]{ExtKeyUsageServerAuth}.slice();
    }
    foreach (var (_, eku) in opts.KeyUsages) {
        if (eku == ExtKeyUsageAny) {
            // If any key usage is acceptable, no need to check the chain for
            // key usages.
            return (candidateChains, default!);
        }
    }
    chains = new slice<slice<ж<Certificate>>>(0, builtin.len(candidateChains));
    foreach (var (_, candidate) in candidateChains) {
        if (checkChainForKeyUsage(candidate, opts.KeyUsages)) {
            chains = append(chains, candidate);
        }
    }
    if (builtin.len(chains) == 0) {
        return (default!, new CertificateInvalidError(Ꮡc, IncompatibleUsage, ""));
    }
    return (chains, default!);
}

internal static slice<ж<Certificate>> appendToFreshChain(slice<ж<Certificate>> chain, ж<Certificate> Ꮡcert) {
    var n = new slice<ж<Certificate>>(builtin.len(chain) + 1);
    copy(n, chain);
    n[builtin.len(chain)] = Ꮡcert;
    return n;
}

[GoType("dyn")] partial interface alreadyInChain_pubKeyEqual {
    bool Equal(cryptoꓸPublicKey _);
}

// alreadyInChain checks whether a candidate certificate is present in a chain.
// Rather than doing a direct byte for byte equivalency check, we check if the
// subject, public key, and SAN, if present, are equal. This prevents loops that
// are created by mutual cross-signatures, or other cross-signature bridge
// oddities.
internal static bool alreadyInChain(ж<Certificate> Ꮡcandidate, slice<ж<Certificate>> chain) {
    ref var candidate = ref Ꮡcandidate.Value;

    ж<pkix.Extension> candidateSAN = default!;
    foreach (var (_, vᴛ1) in candidate.Extensions) {
        ref var ext = ref heap(new pkix.Extension(), out var Ꮡext);
        ext = vᴛ1;

        if (ext.Id.Equal(oidExtensionSubjectAltName)) {
            candidateSAN = Ꮡext;
            break;
        }
    }
    foreach (var (_, cert) in chain) {
        if (!bytes.Equal(candidate.RawSubject, (~cert).RawSubject)) {
            continue;
        }
        if (!candidate.PublicKey._<alreadyInChain_pubKeyEqual>().Equal((~cert).PublicKey)) {
            continue;
        }
        ж<pkix.Extension> certSAN = default!;
        foreach (var (_, vᴛ2) in (~cert).Extensions) {
            ref var ext = ref heap(new pkix.Extension(), out var Ꮡext);
            ext = vᴛ2;

            if (ext.Id.Equal(oidExtensionSubjectAltName)) {
                certSAN = Ꮡext;
                break;
            }
        }
        if (candidateSAN == nil && certSAN == nil){
            return true;
        } else 
        if (candidateSAN == nil || certSAN == nil) {
            return false;
        }
        if (bytes.Equal((~candidateSAN).Value, (~certSAN).Value)) {
            return true;
        }
    }
    return false;
}

// maxChainSignatureChecks is the maximum number of CheckSignatureFrom calls
// that an invocation of buildChains will (transitively) make. Most chains are
// less than 15 certificates long, so this leaves space for multiple chains and
// for failed checks due to different intermediates having the same Subject.
internal static readonly UntypedInt maxChainSignatureChecks = 100;

internal static (slice<slice<ж<Certificate>>> chains, error err) buildChains(this ж<Certificate> Ꮡc, slice<ж<Certificate>> currentChain, ж<nint> ᏑsigChecks, ж<VerifyOptions> Ꮡopts) {
    slice<slice<ж<Certificate>>> chains = default!;
    error err = default!;

    ref var opts = ref Ꮡopts.Value;
    ref var hintErr = ref heap<error>(out var ᏑhintErr);
    ref var hintCert = ref heap<ж<Certificate>>(out var ᏑhintCert);
    var currentChainʗ1 = currentChain;
    var considerCandidate = (nint certType, potentialParent candidate) => {
        if ((~candidate.cert).PublicKey == default! || alreadyInChain(candidate.cert, currentChainʗ1)) {
            return;
        }
        if (ᏑsigChecks == nil) {
            ᏑsigChecks = @new<nint>();
        }
        ᏑsigChecks.Value++;
        if (ᏑsigChecks.Value > maxChainSignatureChecks) {
            err = errors.New("x509: signature check attempts limit reached while verifying certificate chain"u8);
            return;
        }
        {
            var errΔ1 = Ꮡc.Value.CheckSignatureFrom(candidate.cert); if (errΔ1 != default!) {
                if (ᏑhintErr.ValueSlot == default!) {
                    ᏑhintErr.ValueSlot = errΔ1;
                    ᏑhintCert.ValueSlot = candidate.cert;
                }
                return;
            }
        }
        err = candidate.cert.isValid(certType, currentChainʗ1, Ꮡopts);
        if (err != default!) {
            if (ᏑhintErr.ValueSlot == default!) {
                ᏑhintErr.ValueSlot = err;
                ᏑhintCert.ValueSlot = candidate.cert;
            }
            return;
        }
        if (candidate.constraint != default!) {
            {
                var errΔ2 = candidate.constraint(currentChainʗ1); if (errΔ2 != default!) {
                    if (ᏑhintErr.ValueSlot == default!) {
                        ᏑhintErr.ValueSlot = errΔ2;
                        ᏑhintCert.ValueSlot = candidate.cert;
                    }
                    return;
                }
            }
        }
        var exprᴛ1 = certType;
        if (exprᴛ1 == rootCertificate) {
            chains = append(chains, appendToFreshChain(currentChainʗ1, candidate.cert));
        }
        else if (exprᴛ1 == intermediateCertificate) {
            slice<slice<ж<Certificate>>> childChains = default!;
            (childChains, err) = candidate.cert.buildChains(appendToFreshChain(currentChainʗ1, candidate.cert), ᏑsigChecks, Ꮡopts);
            chains = append(chains, childChains.ꓸꓸꓸ);
        }

    };
    foreach (var (_, root) in opts.Roots.findPotentialParents(Ꮡc)) {
        considerCandidate(rootCertificate, root);
    }
    foreach (var (_, intermediate) in opts.Intermediates.findPotentialParents(Ꮡc)) {
        considerCandidate(intermediateCertificate, intermediate);
    }
    if (builtin.len(chains) > 0) {
        err = default!;
    }
    if (builtin.len(chains) == 0 && err == default!) {
        err = new UnknownAuthorityError(Ꮡc, hintErr, hintCert);
    }
    return (chains, err);
}

internal static bool validHostnamePattern(@string host) {
    return validHostname(host, true);
}

internal static bool validHostnameInput(@string host) {
    return validHostname(host, false);
}

// validHostname reports whether host is a valid hostname that can be matched or
// matched against according to RFC 6125 2.2, with some leniency to accommodate
// legacy values.
internal static bool validHostname(@string host, bool isPattern) {
    if (!isPattern) {
        host = strings.TrimSuffix(host, "."u8);
    }
    if (builtin.len(host) == 0) {
        return false;
    }
    if (host == "*"u8) {
        // Bare wildcards are not allowed, they are not valid DNS names,
        // nor are they allowed per RFC 6125.
        return false;
    }
    foreach (var (i, part) in strings.Split(host, "."u8)) {
        if (part == ""u8) {
            // Empty label.
            return false;
        }
        if (isPattern && i == 0 && part == "*"u8) {
            // Only allow full left-most wildcards, as those are the only ones
            // we match, and matching literal '*' characters is probably never
            // the expected behavior.
            continue;
        }
        foreach (var (j, c) in part) {
            if ((rune)'a' <= c && c <= (rune)'z') {
                continue;
            }
            if ((rune)'0' <= c && c <= (rune)'9') {
                continue;
            }
            if ((rune)'A' <= c && c <= (rune)'Z') {
                continue;
            }
            if (c == (rune)'-' && j != 0) {
                continue;
            }
            if (c == (rune)'_') {
                // Not a valid character in hostnames, but commonly
                // found in deployments outside the WebPKI.
                continue;
            }
            return false;
        }
    }
    return true;
}

internal static bool matchExactly(@string hostA, @string hostB) {
    if (hostA == ""u8 || hostA == "."u8 || hostB == ""u8 || hostB == "."u8) {
        return false;
    }
    return toLowerCaseASCII(hostA) == toLowerCaseASCII(hostB);
}

internal static bool matchHostnames(@string pattern, @string host) {
    pattern = toLowerCaseASCII(pattern);
    host = toLowerCaseASCII(strings.TrimSuffix(host, "."u8));
    if (builtin.len(pattern) == 0 || builtin.len(host) == 0) {
        return false;
    }
    var patternParts = strings.Split(pattern, "."u8);
    var hostParts = strings.Split(host, "."u8);
    if (builtin.len(patternParts) != builtin.len(hostParts)) {
        return false;
    }
    foreach (var (i, patternPart) in patternParts) {
        if (i == 0 && patternPart == "*"u8) {
            continue;
        }
        if (patternPart != hostParts[i]) {
            return false;
        }
    }
    return true;
}

// toLowerCaseASCII returns a lower-case version of in. See RFC 6125 6.4.1. We use
// an explicitly ASCII function to avoid any sharp corners resulting from
// performing Unicode operations on DNS labels.
internal static @string toLowerCaseASCII(@string @in) {
    // If the string is already lower-case then there's nothing to do.
    var isAlreadyLowerCase = true;
    foreach (var (_, c) in @in) {
        if (c == utf8.RuneError) {
            // If we get a UTF-8 error then there might be
            // upper-case ASCII bytes in the invalid sequence.
            isAlreadyLowerCase = false;
            break;
        }
        if ((rune)'A' <= c && c <= (rune)'Z') {
            isAlreadyLowerCase = false;
            break;
        }
    }
    if (isAlreadyLowerCase) {
        return @in;
    }
    var @out = slice<byte>(@in);
    foreach (var (i, c) in @out) {
        if ((rune)'A' <= c && c <= (rune)'Z') {
            @out[i] += (byte)((rune)'a' - (rune)'A');
        }
    }
    return ((@string)@out);
}

// VerifyHostname returns nil if c is a valid certificate for the named host.
// Otherwise it returns an error describing the mismatch.
//
// IP addresses can be optionally enclosed in square brackets and are checked
// against the IPAddresses field. Other names are checked case insensitively
// against the DNSNames field. If the names are valid hostnames, the certificate
// fields can have a wildcard as the complete left-most label (e.g. *.example.com).
//
// Note that the legacy Common Name field is ignored.
public static error VerifyHostname(this ж<Certificate> Ꮡc, @string h) {
    ref var c = ref Ꮡc.Value;

    // IP addresses may be written in [ ].
    @string candidateIP = h;
    if (builtin.len(h) >= 3 && h[0] == (rune)'[' && h[builtin.len(h) - 1] == (rune)']') {
        candidateIP = h[1..(int)(builtin.len(h) - 1)];
    }
    {
        var ip = net.ParseIP(candidateIP); if (ip != default!) {
            // We only match IP addresses against IP SANs.
            // See RFC 6125, Appendix B.2.
            foreach (var (_, candidate) in c.IPAddresses) {
                if (ip.Equal(candidate)) {
                    return default!;
                }
            }
            return new HostnameError(Ꮡc, candidateIP);
        }
    }
    @string candidateName = toLowerCaseASCII(h);
    // Save allocations inside the loop.
    var validCandidateName = validHostnameInput(candidateName);
    foreach (var (_, match) in c.DNSNames) {
        // Ideally, we'd only match valid hostnames according to RFC 6125 like
        // browsers (more or less) do, but in practice Go is used in a wider
        // array of contexts and can't even assume DNS resolution. Instead,
        // always allow perfect matches, and only apply wildcard and trailing
        // dot processing to valid hostnames.
        if (validCandidateName && validHostnamePattern(match)){
            if (matchHostnames(match, candidateName)) {
                return default!;
            }
        } else {
            if (matchExactly(match, candidateName)) {
                return default!;
            }
        }
    }
    return new HostnameError(Ꮡc, h);
}

internal static bool checkChainForKeyUsage(slice<ж<Certificate>> chain, slice<ExtKeyUsage> keyUsages) {
    var usages = new slice<ExtKeyUsage>(builtin.len(keyUsages));
    copy(usages, keyUsages);
    if (builtin.len(chain) == 0) {
        return false;
    }
    nint usagesRemaining = builtin.len(usages);
    // We walk down the list and cross out any usages that aren't supported
    // by each certificate. If we cross out all the usages, then the chain
    // is unacceptable.
NextCert:
    for (nint i = builtin.len(chain) - 1; i >= 0; i--) {
        var cert = chain[i];
        if (builtin.len((~cert).ExtKeyUsage) == 0 && builtin.len((~cert).UnknownExtKeyUsage) == 0) {
            // The certificate doesn't have any extended key usage specified.
            continue;
        }
        foreach (var (_, usage) in (~cert).ExtKeyUsage) {
            if (usage == ExtKeyUsageAny) {
                // The certificate is explicitly good for any usage.
                goto continue_NextCert;
            }
        }
        ExtKeyUsage invalidUsage = -1;
NextRequestedUsage:
        foreach (var (iΔ1, requestedUsage) in usages) {
            if (requestedUsage == invalidUsage) {
                continue;
            }
            foreach (var (_, usage) in (~cert).ExtKeyUsage) {
                if (requestedUsage == usage) {
                    goto continue_NextRequestedUsage;
                }
            }
            usages[iΔ1] = invalidUsage;
            usagesRemaining--;
            if (usagesRemaining == 0) {
                return false;
            }
continue_NextRequestedUsage:;
        }
break_NextRequestedUsage:;
continue_NextCert:;
    }
break_NextCert:;
    return true;
}

} // end x509_package
