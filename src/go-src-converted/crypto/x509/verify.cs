// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2022 March 06 22:19:56 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Program Files\Go\src\crypto\x509\verify.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using net = go.net_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.crypto;

public static partial class x509_package {

public partial struct InvalidReason { // : nint
}

 
// NotAuthorizedToSign results when a certificate is signed by another
// which isn't marked as a CA certificate.
public static readonly InvalidReason NotAuthorizedToSign = iota; 
// Expired results when a certificate has expired, based on the time
// given in the VerifyOptions.
public static readonly var Expired = 0; 
// CANotAuthorizedForThisName results when an intermediate or root
// certificate has a name constraint which doesn't permit a DNS or
// other name (including IP address) in the leaf certificate.
public static readonly var CANotAuthorizedForThisName = 1; 
// TooManyIntermediates results when a path length constraint is
// violated.
public static readonly var TooManyIntermediates = 2; 
// IncompatibleUsage results when the certificate's key usage indicates
// that it may only be used for a different purpose.
public static readonly var IncompatibleUsage = 3; 
// NameMismatch results when the subject name of a parent certificate
// does not match the issuer name in the child.
public static readonly var NameMismatch = 4; 
// NameConstraintsWithoutSANs is a legacy error and is no longer returned.
public static readonly var NameConstraintsWithoutSANs = 5; 
// UnconstrainedName results when a CA certificate contains permitted
// name constraints, but leaf certificate contains a name of an
// unsupported or unconstrained type.
public static readonly var UnconstrainedName = 6; 
// TooManyConstraints results when the number of comparison operations
// needed to check a certificate exceeds the limit set by
// VerifyOptions.MaxConstraintComparisions. This limit exists to
// prevent pathological certificates can consuming excessive amounts of
// CPU time to verify.
public static readonly var TooManyConstraints = 7; 
// CANotAuthorizedForExtKeyUsage results when an intermediate or root
// certificate does not permit a requested extended key usage.
public static readonly var CANotAuthorizedForExtKeyUsage = 8;


// CertificateInvalidError results when an odd error occurs. Users of this
// library probably want to handle all these errors uniformly.
public partial struct CertificateInvalidError {
    public ptr<Certificate> Cert;
    public InvalidReason Reason;
    public @string Detail;
}

public static @string Error(this CertificateInvalidError e) {

    if (e.Reason == NotAuthorizedToSign) 
        return "x509: certificate is not authorized to sign other certificates";
    else if (e.Reason == Expired) 
        return "x509: certificate has expired or is not yet valid: " + e.Detail;
    else if (e.Reason == CANotAuthorizedForThisName) 
        return "x509: a root or intermediate certificate is not authorized to sign for this name: " + e.Detail;
    else if (e.Reason == CANotAuthorizedForExtKeyUsage) 
        return "x509: a root or intermediate certificate is not authorized for an extended key usage: " + e.Detail;
    else if (e.Reason == TooManyIntermediates) 
        return "x509: too many intermediates for path length constraint";
    else if (e.Reason == IncompatibleUsage) 
        return "x509: certificate specifies an incompatible key usage";
    else if (e.Reason == NameMismatch) 
        return "x509: issuer name does not match subject from issuing certificate";
    else if (e.Reason == NameConstraintsWithoutSANs) 
        return "x509: issuer has name constraints but leaf doesn't have a SAN extension";
    else if (e.Reason == UnconstrainedName) 
        return "x509: issuer has name constraints but leaf contains unknown or unconstrained name: " + e.Detail;
        return "x509: unknown error";

}

// HostnameError results when the set of authorized names doesn't match the
// requested name.
public partial struct HostnameError {
    public ptr<Certificate> Certificate;
    public @string Host;
}

public static @string Error(this HostnameError h) {
    var c = h.Certificate;

    if (!c.hasSANExtension() && matchHostnames(c.Subject.CommonName, h.Host)) {
        return "x509: certificate relies on legacy Common Name field, use SANs instead";
    }
    @string valid = default;
    {
        var ip = net.ParseIP(h.Host);

        if (ip != null) { 
            // Trying to validate an IP
            if (len(c.IPAddresses) == 0) {
                return "x509: cannot validate certificate for " + h.Host + " because it doesn't contain any IP SANs";
            }

            foreach (var (_, san) in c.IPAddresses) {
                if (len(valid) > 0) {
                    valid += ", ";
                }
                valid += san.String();
            }
        else
        } {
            valid = strings.Join(c.DNSNames, ", ");
        }
    }


    if (len(valid) == 0) {
        return "x509: certificate is not valid for any names, but wanted to match " + h.Host;
    }
    return "x509: certificate is valid for " + valid + ", not " + h.Host;

}

// UnknownAuthorityError results when the certificate issuer is unknown
public partial struct UnknownAuthorityError {
    public ptr<Certificate> Cert; // hintErr contains an error that may be helpful in determining why an
// authority wasn't found.
    public error hintErr; // hintCert contains a possible authority certificate that was rejected
// because of the error in hintErr.
    public ptr<Certificate> hintCert;
}

public static @string Error(this UnknownAuthorityError e) {
    @string s = "x509: certificate signed by unknown authority";
    if (e.hintErr != null) {
        var certName = e.hintCert.Subject.CommonName;
        if (len(certName) == 0) {
            if (len(e.hintCert.Subject.Organization) > 0) {
                certName = e.hintCert.Subject.Organization[0];
            }
            else
 {
                certName = "serial:" + e.hintCert.SerialNumber.String();
            }

        }
        s += fmt.Sprintf(" (possibly because of %q while trying to verify candidate authority certificate %q)", e.hintErr, certName);

    }
    return s;

}

// SystemRootsError results when we fail to load the system root certificates.
public partial struct SystemRootsError {
    public error Err;
}

public static @string Error(this SystemRootsError se) {
    @string msg = "x509: failed to load system roots and no roots provided";
    if (se.Err != null) {
        return msg + "; " + se.Err.Error();
    }
    return msg;

}

public static error Unwrap(this SystemRootsError se) {
    return error.As(se.Err)!;
}

// errNotParsed is returned when a certificate without ASN.1 contents is
// verified. Platform-specific verification needs the ASN.1 contents.
private static var errNotParsed = errors.New("x509: missing ASN.1 contents; use ParseCertificate");

// VerifyOptions contains parameters for Certificate.Verify.
public partial struct VerifyOptions {
    public @string DNSName; // Intermediates is an optional pool of certificates that are not trust
// anchors, but can be used to form a chain from the leaf certificate to a
// root certificate.
    public ptr<CertPool> Intermediates; // Roots is the set of trusted root certificates the leaf certificate needs
// to chain up to. If nil, the system roots or the platform verifier are used.
    public ptr<CertPool> Roots; // CurrentTime is used to check the validity of all certificates in the
// chain. If zero, the current time is used.
    public time.Time CurrentTime; // KeyUsages specifies which Extended Key Usage values are acceptable. A
// chain is accepted if it allows any of the listed values. An empty list
// means ExtKeyUsageServerAuth. To accept any key usage, include ExtKeyUsageAny.
    public slice<ExtKeyUsage> KeyUsages; // MaxConstraintComparisions is the maximum number of comparisons to
// perform when checking a given certificate's name constraints. If
// zero, a sensible default is used. This limit prevents pathological
// certificates from consuming excessive amounts of CPU time when
// validating. It does not apply to the platform verifier.
    public nint MaxConstraintComparisions;
}

private static readonly var leafCertificate = iota;
private static readonly var intermediateCertificate = 0;
private static readonly var rootCertificate = 1;


// rfc2821Mailbox represents a “mailbox” (which is an email address to most
// people) by breaking it into the “local” (i.e. before the '@') and “domain”
// parts.
private partial struct rfc2821Mailbox {
    public @string local;
    public @string domain;
}

// parseRFC2821Mailbox parses an email address into local and domain parts,
// based on the ABNF for a “Mailbox” from RFC 2821. According to RFC 5280,
// Section 4.2.1.6 that's correct for an rfc822Name from a certificate: “The
// format of an rfc822Name is a "Mailbox" as defined in RFC 2821, Section 4.1.2”.
private static (rfc2821Mailbox, bool) parseRFC2821Mailbox(@string @in) {
    rfc2821Mailbox mailbox = default;
    bool ok = default;

    if (len(in) == 0) {
        return (mailbox, false);
    }
    var localPartBytes = make_slice<byte>(0, len(in) / 2);

    if (in[0] == '"') { 
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
        in = in[(int)1..];
QuotedString:
        while (true) {
            if (len(in) == 0) {
                return (mailbox, false);
            }
            var c = in[0];
            in = in[(int)1..];


            if (c == '"') 
                _breakQuotedString = true;

                break;
            else if (c == '\\') 
                // quoted-pair
                if (len(in) == 0) {
                    return (mailbox, false);
                }
                if (in[0] == 11 || in[0] == 12 || (1 <= in[0] && in[0] <= 9) || (14 <= in[0] && in[0] <= 127)) {
                    localPartBytes = append(localPartBytes, in[0]);
                    in = in[(int)1..];
                }
                else
 {
                    return (mailbox, false);
                }

            else if (c == 11 || c == 12 || c == 32 || c == 33 || c == 127 || (1 <= c && c <= 8) || (14 <= c && c <= 31) || (35 <= c && c <= 91) || (93 <= c && c <= 126)) 
                // qtext
                localPartBytes = append(localPartBytes, c);
            else 
                return (mailbox, false);
            
        }
    else
    } { 
        // Atom ("." Atom)*
NextChar:

        while (len(in) > 0) { 
            // atext from RFC 2822, Section 3.2.4
            c = in[0];


            if (c == '\\') 
            {
                // Examples given in RFC 3696 suggest that
                // escaped characters can appear outside of a
                // quoted string. Several “verified” errata
                // continue to argue the point. We choose to
                // accept it.
                in = in[(int)1..];
                if (len(in) == 0) {
                    return (mailbox, false);
                }
                fallthrough = true;

            }
            if (fallthrough || ('0' <= c && c <= '9') || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' || c == '*' || c == '+' || c == '-' || c == '/' || c == '=' || c == '?' || c == '^' || c == '_' || c == '`' || c == '{' || c == '|' || c == '}' || c == '~' || c == '.')
            {
                localPartBytes = append(localPartBytes, in[0]);
                in = in[(int)1..];
                goto __switch_break0;
            }
            // default: 
                _breakNextChar = true;
                break;

            __switch_break0:;

        }
        if (len(localPartBytes) == 0) {
            return (mailbox, false);
        }
        byte twoDots = new slice<byte>(new byte[] { '.', '.' });
        if (localPartBytes[0] == '.' || localPartBytes[len(localPartBytes) - 1] == '.' || bytes.Contains(localPartBytes, twoDots)) {
            return (mailbox, false);
        }
    }
    if (len(in) == 0 || in[0] != '@') {
        return (mailbox, false);
    }
    in = in[(int)1..]; 

    // The RFC species a format for domains, but that's known to be
    // violated in practice so we accept that anything after an '@' is the
    // domain part.
    {
        var (_, ok) = domainToReverseLabels(in);

        if (!ok) {
            return (mailbox, false);
        }
    }


    mailbox.local = string(localPartBytes);
    mailbox.domain = in;
    return (mailbox, true);

}

// domainToReverseLabels converts a textual domain name like foo.example.com to
// the list of labels in reverse order, e.g. ["com", "example", "foo"].
private static (slice<@string>, bool) domainToReverseLabels(@string domain) {
    slice<@string> reverseLabels = default;
    bool ok = default;

    while (len(domain) > 0) {
        {
            var i = strings.LastIndexByte(domain, '.');

            if (i == -1) {
                reverseLabels = append(reverseLabels, domain);
                domain = "";
            }
            else
 {
                reverseLabels = append(reverseLabels, domain[(int)i + 1..]);
                domain = domain[..(int)i];
            }

        }

    }

    if (len(reverseLabels) > 0 && len(reverseLabels[0]) == 0) { 
        // An empty label at the end indicates an absolute value.
        return (null, false);

    }
    foreach (var (_, label) in reverseLabels) {
        if (len(label) == 0) { 
            // Empty labels are otherwise invalid.
            return (null, false);

        }
        foreach (var (_, c) in label) {
            if (c < 33 || c > 126) { 
                // Invalid character.
                return (null, false);

            }

        }
    }    return (reverseLabels, true);

}

private static (bool, error) matchEmailConstraint(rfc2821Mailbox mailbox, @string constraint) {
    bool _p0 = default;
    error _p0 = default!;
 
    // If the constraint contains an @, then it specifies an exact mailbox
    // name.
    if (strings.Contains(constraint, "@")) {
        var (constraintMailbox, ok) = parseRFC2821Mailbox(constraint);
        if (!ok) {
            return (false, error.As(fmt.Errorf("x509: internal error: cannot parse constraint %q", constraint))!);
        }
        return (mailbox.local == constraintMailbox.local && strings.EqualFold(mailbox.domain, constraintMailbox.domain), error.As(null!)!);

    }
    return matchDomainConstraint(mailbox.domain, constraint);

}

private static (bool, error) matchURIConstraint(ptr<url.URL> _addr_uri, @string constraint) {
    bool _p0 = default;
    error _p0 = default!;
    ref url.URL uri = ref _addr_uri.val;
 
    // From RFC 5280, Section 4.2.1.10:
    // “a uniformResourceIdentifier that does not include an authority
    // component with a host name specified as a fully qualified domain
    // name (e.g., if the URI either does not include an authority
    // component or includes an authority component in which the host name
    // is specified as an IP address), then the application MUST reject the
    // certificate.”

    var host = uri.Host;
    if (len(host) == 0) {
        return (false, error.As(fmt.Errorf("URI with empty host (%q) cannot be matched against constraints", uri.String()))!);
    }
    if (strings.Contains(host, ":") && !strings.HasSuffix(host, "]")) {
        error err = default!;
        host, _, err = net.SplitHostPort(uri.Host);
        if (err != null) {
            return (false, error.As(err)!);
        }
    }
    if (strings.HasPrefix(host, "[") && strings.HasSuffix(host, "]") || net.ParseIP(host) != null) {
        return (false, error.As(fmt.Errorf("URI with IP (%q) cannot be matched against constraints", uri.String()))!);
    }
    return matchDomainConstraint(host, constraint);

}

private static (bool, error) matchIPConstraint(net.IP ip, ptr<net.IPNet> _addr_constraint) {
    bool _p0 = default;
    error _p0 = default!;
    ref net.IPNet constraint = ref _addr_constraint.val;

    if (len(ip) != len(constraint.IP)) {
        return (false, error.As(null!)!);
    }
    foreach (var (i) in ip) {
        {
            var mask = constraint.Mask[i];

            if (ip[i] & mask != constraint.IP[i] & mask) {
                return (false, error.As(null!)!);
            }

        }

    }    return (true, error.As(null!)!);

}

private static (bool, error) matchDomainConstraint(@string domain, @string constraint) {
    bool _p0 = default;
    error _p0 = default!;
 
    // The meaning of zero length constraints is not specified, but this
    // code follows NSS and accepts them as matching everything.
    if (len(constraint) == 0) {
        return (true, error.As(null!)!);
    }
    var (domainLabels, ok) = domainToReverseLabels(domain);
    if (!ok) {
        return (false, error.As(fmt.Errorf("x509: internal error: cannot parse domain %q", domain))!);
    }
    var mustHaveSubdomains = false;
    if (constraint[0] == '.') {
        mustHaveSubdomains = true;
        constraint = constraint[(int)1..];
    }
    var (constraintLabels, ok) = domainToReverseLabels(constraint);
    if (!ok) {
        return (false, error.As(fmt.Errorf("x509: internal error: cannot parse domain %q", constraint))!);
    }
    if (len(domainLabels) < len(constraintLabels) || (mustHaveSubdomains && len(domainLabels) == len(constraintLabels))) {
        return (false, error.As(null!)!);
    }
    foreach (var (i, constraintLabel) in constraintLabels) {
        if (!strings.EqualFold(constraintLabel, domainLabels[i])) {
            return (false, error.As(null!)!);
        }
    }    return (true, error.As(null!)!);

}

// checkNameConstraints checks that c permits a child certificate to claim the
// given name, of type nameType. The argument parsedName contains the parsed
// form of name, suitable for passing to the match function. The total number
// of comparisons is tracked in the given count and should not exceed the given
// limit.
private static error checkNameConstraints(this ptr<Certificate> _addr_c, ptr<nint> _addr_count, nint maxConstraintComparisons, @string nameType, @string name, object parsedName, Func<object, object, (bool, error)> match, object permitted, object excluded) {
    ref Certificate c = ref _addr_c.val;
    ref nint count = ref _addr_count.val;

    var excludedValue = reflect.ValueOf(excluded);

    count += excludedValue.Len();
    if (count > maxConstraintComparisons.val) {
        return error.As(new CertificateInvalidError(c,TooManyConstraints,""))!;
    }
    {
        nint i__prev1 = i;

        for (nint i = 0; i < excludedValue.Len(); i++) {
            var constraint = excludedValue.Index(i).Interface();
            var (match, err) = match(parsedName, constraint);
            if (err != null) {
                return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,err.Error()))!;
            }
            if (match) {
                return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,fmt.Sprintf("%s %q is excluded by constraint %q",nameType,name,constraint)))!;
            }
        }

        i = i__prev1;
    }

    var permittedValue = reflect.ValueOf(permitted);

    count += permittedValue.Len();
    if (count > maxConstraintComparisons.val) {
        return error.As(new CertificateInvalidError(c,TooManyConstraints,""))!;
    }
    var ok = true;
    {
        nint i__prev1 = i;

        for (i = 0; i < permittedValue.Len(); i++) {
            constraint = permittedValue.Index(i).Interface();

            error err = default!;
            ok, err = match(parsedName, constraint);

            if (err != null) {
                return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,err.Error()))!;
            }

            if (ok) {
                break;
            }

        }

        i = i__prev1;
    }

    if (!ok) {
        return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,fmt.Sprintf("%s %q is not permitted by any constraint",nameType,name)))!;
    }
    return error.As(null!)!;

}

// isValid performs validity checks on c given that it is a candidate to append
// to the chain in currentChain.
private static error isValid(this ptr<Certificate> _addr_c, nint certType, slice<ptr<Certificate>> currentChain, ptr<VerifyOptions> _addr_opts) {
    ref Certificate c = ref _addr_c.val;
    ref VerifyOptions opts = ref _addr_opts.val;

    if (len(c.UnhandledCriticalExtensions) > 0) {
        return error.As(new UnhandledCriticalExtension())!;
    }
    if (len(currentChain) > 0) {
        var child = currentChain[len(currentChain) - 1];
        if (!bytes.Equal(child.RawIssuer, c.RawSubject)) {
            return error.As(new CertificateInvalidError(c,NameMismatch,""))!;
        }
    }
    var now = opts.CurrentTime;
    if (now.IsZero()) {
        now = time.Now();
    }
    if (now.Before(c.NotBefore)) {
        return error.As(new CertificateInvalidError(Cert:c,Reason:Expired,Detail:fmt.Sprintf("current time %s is before %s",now.Format(time.RFC3339),c.NotBefore.Format(time.RFC3339)),))!;
    }
    else if (now.After(c.NotAfter)) {
        return error.As(new CertificateInvalidError(Cert:c,Reason:Expired,Detail:fmt.Sprintf("current time %s is after %s",now.Format(time.RFC3339),c.NotAfter.Format(time.RFC3339)),))!;
    }
    var maxConstraintComparisons = opts.MaxConstraintComparisions;
    if (maxConstraintComparisons == 0) {
        maxConstraintComparisons = 250000;
    }
    ref nint comparisonCount = ref heap(0, out ptr<nint> _addr_comparisonCount);

    ptr<Certificate> leaf;
    if (certType == intermediateCertificate || certType == rootCertificate) {
        if (len(currentChain) == 0) {
            return error.As(errors.New("x509: internal error: empty chain when appending CA cert"))!;
        }
        leaf = currentChain[0];

    }
    if ((certType == intermediateCertificate || certType == rootCertificate) && c.hasNameConstraints() && leaf.hasSANExtension()) {
        err = forEachSAN(leaf.getSANExtension(), (tag, data) => {

            if (tag == nameTypeEmail) 
                var name = string(data);
                var (mailbox, ok) = parseRFC2821Mailbox(name);
                if (!ok) {
                    return error.As(fmt.Errorf("x509: cannot parse rfc822Name %q", mailbox))!;
                }
                {
                    var err__prev2 = err;

                    var err = c.checkNameConstraints(_addr_comparisonCount, maxConstraintComparisons, "email address", name, mailbox, (parsedName, constraint) => {
                        return error.As(matchEmailConstraint(parsedName._<rfc2821Mailbox>(), constraint._<@string>()))!;
                    }, c.PermittedEmailAddresses, c.ExcludedEmailAddresses);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }


            else if (tag == nameTypeDNS) 
                name = string(data);
                {
                    var (_, ok) = domainToReverseLabels(name);

                    if (!ok) {
                        return error.As(fmt.Errorf("x509: cannot parse dnsName %q", name))!;
                    }

                }


                {
                    var err__prev2 = err;

                    err = c.checkNameConstraints(_addr_comparisonCount, maxConstraintComparisons, "DNS name", name, name, (parsedName, constraint) => {
                        return error.As(matchDomainConstraint(parsedName._<@string>(), constraint._<@string>()))!;
                    }, c.PermittedDNSDomains, c.ExcludedDNSDomains);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }


            else if (tag == nameTypeURI) 
                name = string(data);
                var (uri, err) = url.Parse(name);
                if (err != null) {
                    return error.As(fmt.Errorf("x509: internal error: URI SAN %q failed to parse", name))!;
                }
                {
                    var err__prev2 = err;

                    err = c.checkNameConstraints(_addr_comparisonCount, maxConstraintComparisons, "URI", name, uri, (parsedName, constraint) => {
                        return error.As(matchURIConstraint(parsedName._<ptr<url.URL>>(), constraint._<@string>()))!;
                    }, c.PermittedURIDomains, c.ExcludedURIDomains);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }


            else if (tag == nameTypeIP) 
                var ip = net.IP(data);
                {
                    var l = len(ip);

                    if (l != net.IPv4len && l != net.IPv6len) {
                        return error.As(fmt.Errorf("x509: internal error: IP SAN %x failed to parse", data))!;
                    }

                }


                {
                    var err__prev2 = err;

                    err = c.checkNameConstraints(_addr_comparisonCount, maxConstraintComparisons, "IP address", ip.String(), ip, (parsedName, constraint) => {
                        return error.As(matchIPConstraint(parsedName._<net.IP>(), constraint._<ptr<net.IPNet>>()))!;
                    }, c.PermittedIPRanges, c.ExcludedIPRanges);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }


            else                         return error.As(null!)!;

        });

        if (err != null) {
            return error.As(err)!;
        }
    }
    if (certType == intermediateCertificate && (!c.BasicConstraintsValid || !c.IsCA)) {
        return error.As(new CertificateInvalidError(c,NotAuthorizedToSign,""))!;
    }
    if (c.BasicConstraintsValid && c.MaxPathLen >= 0) {
        var numIntermediates = len(currentChain) - 1;
        if (numIntermediates > c.MaxPathLen) {
            return error.As(new CertificateInvalidError(c,TooManyIntermediates,""))!;
        }
    }
    return error.As(null!)!;

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
// WARNING: this function doesn't do any revocation checking.
private static (slice<slice<ptr<Certificate>>>, error) Verify(this ptr<Certificate> _addr_c, VerifyOptions opts) {
    slice<slice<ptr<Certificate>>> chains = default;
    error err = default!;
    ref Certificate c = ref _addr_c.val;
 
    // Platform-specific verification needs the ASN.1 contents so
    // this makes the behavior consistent across platforms.
    if (len(c.Raw) == 0) {
        return (null, error.As(errNotParsed)!);
    }
    for (nint i = 0; i < opts.Intermediates.len(); i++) {
        var (c, err) = opts.Intermediates.cert(i);
        if (err != null) {
            return (null, error.As(fmt.Errorf("crypto/x509: error fetching intermediate: %w", err))!);
        }
        if (len(c.Raw) == 0) {
            return (null, error.As(errNotParsed)!);
        }
    } 

    // Use Windows's own verification and chain building.
    if (opts.Roots == null && runtime.GOOS == "windows") {
        return c.systemVerify(_addr_opts);
    }
    if (opts.Roots == null) {
        opts.Roots = systemRootsPool();
        if (opts.Roots == null) {
            return (null, error.As(new SystemRootsError(systemRootsErr))!);
        }
    }
    err = c.isValid(leafCertificate, null, _addr_opts);
    if (err != null) {
        return ;
    }
    if (len(opts.DNSName) > 0) {
        err = c.VerifyHostname(opts.DNSName);
        if (err != null) {
            return ;
        }
    }
    slice<slice<ptr<Certificate>>> candidateChains = default;
    if (opts.Roots.contains(c)) {
        candidateChains = append(candidateChains, new slice<ptr<Certificate>>(new ptr<Certificate>[] { c }));
    }
    else
 {
        candidateChains, err = c.buildChains(null, new slice<ptr<Certificate>>(new ptr<Certificate>[] { c }), null, _addr_opts);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    var keyUsages = opts.KeyUsages;
    if (len(keyUsages) == 0) {
        keyUsages = new slice<ExtKeyUsage>(new ExtKeyUsage[] { ExtKeyUsageServerAuth });
    }
    foreach (var (_, usage) in keyUsages) {
        if (usage == ExtKeyUsageAny) {
            return (candidateChains, error.As(null!)!);
        }
    }    foreach (var (_, candidate) in candidateChains) {
        if (checkChainForKeyUsage(candidate, keyUsages)) {
            chains = append(chains, candidate);
        }
    }    if (len(chains) == 0) {
        return (null, error.As(new CertificateInvalidError(c,IncompatibleUsage,""))!);
    }
    return (chains, error.As(null!)!);

}

private static slice<ptr<Certificate>> appendToFreshChain(slice<ptr<Certificate>> chain, ptr<Certificate> _addr_cert) {
    ref Certificate cert = ref _addr_cert.val;

    var n = make_slice<ptr<Certificate>>(len(chain) + 1);
    copy(n, chain);
    n[len(chain)] = cert;
    return n;
}

// maxChainSignatureChecks is the maximum number of CheckSignatureFrom calls
// that an invocation of buildChains will (transitively) make. Most chains are
// less than 15 certificates long, so this leaves space for multiple chains and
// for failed checks due to different intermediates having the same Subject.
private static readonly nint maxChainSignatureChecks = 100;



private static (slice<slice<ptr<Certificate>>>, error) buildChains(this ptr<Certificate> _addr_c, map<ptr<Certificate>, slice<slice<ptr<Certificate>>>> cache, slice<ptr<Certificate>> currentChain, ptr<nint> _addr_sigChecks, ptr<VerifyOptions> _addr_opts) {
    slice<slice<ptr<Certificate>>> chains = default;
    error err = default!;
    ref Certificate c = ref _addr_c.val;
    ref nint sigChecks = ref _addr_sigChecks.val;
    ref VerifyOptions opts = ref _addr_opts.val;

    error hintErr = default!;    ptr<Certificate> hintCert;

    Action<nint, ptr<Certificate>> considerCandidate = (certType, candidate) => {
        foreach (var (_, cert) in currentChain) {
            if (cert.Equal(candidate)) {
                return ;
            }
        }        if (sigChecks == null) {
            sigChecks = @new<int>();
        }
        sigChecks++;
        if (sigChecks > maxChainSignatureChecks.val) {
            err = errors.New("x509: signature check attempts limit reached while verifying certificate chain");
            return ;
        }
        {
            var err = c.CheckSignatureFrom(candidate);

            if (err != null) {
                if (hintErr == null) {
                    hintErr = error.As(err)!;
                    hintCert = candidate;
                }
                return ;
            }

        }


        err = candidate.isValid(certType, currentChain, opts);
        if (err != null) {
            return ;
        }

        if (certType == rootCertificate) 
            chains = append(chains, appendToFreshChain(currentChain, _addr_candidate));
        else if (certType == intermediateCertificate) 
            if (cache == null) {
                cache = make_map<ptr<Certificate>, slice<slice<ptr<Certificate>>>>();
            }
            var (childChains, ok) = cache[candidate];
            if (!ok) {
                childChains, err = candidate.buildChains(cache, appendToFreshChain(currentChain, _addr_candidate), sigChecks, opts);
                cache[candidate] = childChains;
            }
            chains = append(chains, childChains);
        
    };

    foreach (var (_, root) in opts.Roots.findPotentialParents(c)) {
        considerCandidate(rootCertificate, root);
    }    foreach (var (_, intermediate) in opts.Intermediates.findPotentialParents(c)) {
        considerCandidate(intermediateCertificate, intermediate);
    }    if (len(chains) > 0) {
        err = null;
    }
    if (len(chains) == 0 && err == null) {
        err = new UnknownAuthorityError(c,hintErr,hintCert);
    }
    return ;

}

private static bool validHostnamePattern(@string host) {
    return validHostname(host, true);
}
private static bool validHostnameInput(@string host) {
    return validHostname(host, false);
}

// validHostname reports whether host is a valid hostname that can be matched or
// matched against according to RFC 6125 2.2, with some leniency to accommodate
// legacy values.
private static bool validHostname(@string host, bool isPattern) {
    if (!isPattern) {
        host = strings.TrimSuffix(host, ".");
    }
    if (len(host) == 0) {
        return false;
    }
    foreach (var (i, part) in strings.Split(host, ".")) {
        if (part == "") { 
            // Empty label.
            return false;

        }
        if (isPattern && i == 0 && part == "*") { 
            // Only allow full left-most wildcards, as those are the only ones
            // we match, and matching literal '*' characters is probably never
            // the expected behavior.
            continue;

        }
        foreach (var (j, c) in part) {
            if ('a' <= c && c <= 'z') {
                continue;
            }
            if ('0' <= c && c <= '9') {
                continue;
            }
            if ('A' <= c && c <= 'Z') {
                continue;
            }
            if (c == '-' && j != 0) {
                continue;
            }
            if (c == '_') { 
                // Not a valid character in hostnames, but commonly
                // found in deployments outside the WebPKI.
                continue;

            }

            return false;

        }
    }    return true;

}

private static bool matchExactly(@string hostA, @string hostB) {
    if (hostA == "" || hostA == "." || hostB == "" || hostB == ".") {
        return false;
    }
    return toLowerCaseASCII(hostA) == toLowerCaseASCII(hostB);

}

private static bool matchHostnames(@string pattern, @string host) {
    pattern = toLowerCaseASCII(pattern);
    host = toLowerCaseASCII(strings.TrimSuffix(host, "."));

    if (len(pattern) == 0 || len(host) == 0) {
        return false;
    }
    var patternParts = strings.Split(pattern, ".");
    var hostParts = strings.Split(host, ".");

    if (len(patternParts) != len(hostParts)) {
        return false;
    }
    foreach (var (i, patternPart) in patternParts) {
        if (i == 0 && patternPart == "*") {
            continue;
        }
        if (patternPart != hostParts[i]) {
            return false;
        }
    }    return true;

}

// toLowerCaseASCII returns a lower-case version of in. See RFC 6125 6.4.1. We use
// an explicitly ASCII function to avoid any sharp corners resulting from
// performing Unicode operations on DNS labels.
private static @string toLowerCaseASCII(@string @in) { 
    // If the string is already lower-case then there's nothing to do.
    var isAlreadyLowerCase = true;
    {
        var c__prev1 = c;

        foreach (var (_, __c) in in) {
            c = __c;
            if (c == utf8.RuneError) { 
                // If we get a UTF-8 error then there might be
                // upper-case ASCII bytes in the invalid sequence.
                isAlreadyLowerCase = false;
                break;

            }

            if ('A' <= c && c <= 'Z') {
                isAlreadyLowerCase = false;
                break;
            }

        }
        c = c__prev1;
    }

    if (isAlreadyLowerCase) {
        return in;
    }
    slice<byte> @out = (slice<byte>)in;
    {
        var c__prev1 = c;

        foreach (var (__i, __c) in out) {
            i = __i;
            c = __c;
            if ('A' <= c && c <= 'Z') {
                out[i] += 'a' - 'A';
            }
        }
        c = c__prev1;
    }

    return string(out);

}

// VerifyHostname returns nil if c is a valid certificate for the named host.
// Otherwise it returns an error describing the mismatch.
//
// IP addresses can be optionally enclosed in square brackets and are checked
// against the IPAddresses field. Other names are checked case insensitively
// against the DNSNames field. If the names are valid hostnames, the certificate
// fields can have a wildcard as the left-most label.
//
// Note that the legacy Common Name field is ignored.
private static error VerifyHostname(this ptr<Certificate> _addr_c, @string h) {
    ref Certificate c = ref _addr_c.val;
 
    // IP addresses may be written in [ ].
    var candidateIP = h;
    if (len(h) >= 3 && h[0] == '[' && h[len(h) - 1] == ']') {
        candidateIP = h[(int)1..(int)len(h) - 1];
    }
    {
        var ip = net.ParseIP(candidateIP);

        if (ip != null) { 
            // We only match IP addresses against IP SANs.
            // See RFC 6125, Appendix B.2.
            foreach (var (_, candidate) in c.IPAddresses) {
                if (ip.Equal(candidate)) {
                    return error.As(null!)!;
                }
            }
            return error.As(new HostnameError(c,candidateIP))!;

        }
    }


    var candidateName = toLowerCaseASCII(h); // Save allocations inside the loop.
    var validCandidateName = validHostnameInput(candidateName);

    foreach (var (_, match) in c.DNSNames) { 
        // Ideally, we'd only match valid hostnames according to RFC 6125 like
        // browsers (more or less) do, but in practice Go is used in a wider
        // array of contexts and can't even assume DNS resolution. Instead,
        // always allow perfect matches, and only apply wildcard and trailing
        // dot processing to valid hostnames.
        if (validCandidateName && validHostnamePattern(match)) {
            if (matchHostnames(match, candidateName)) {
                return error.As(null!)!;
            }
        }
        else
 {
            if (matchExactly(match, candidateName)) {
                return error.As(null!)!;
            }
        }
    }    return error.As(new HostnameError(c,h))!;

}

private static bool checkChainForKeyUsage(slice<ptr<Certificate>> chain, slice<ExtKeyUsage> keyUsages) {
    var usages = make_slice<ExtKeyUsage>(len(keyUsages));
    copy(usages, keyUsages);

    if (len(chain) == 0) {
        return false;
    }
    var usagesRemaining = len(usages); 

    // We walk down the list and cross out any usages that aren't supported
    // by each certificate. If we cross out all the usages, then the chain
    // is unacceptable.

NextCert:

    {
        var i__prev1 = i;

        for (var i = len(chain) - 1; i >= 0; i--) {
            var cert = chain[i];
            if (len(cert.ExtKeyUsage) == 0 && len(cert.UnknownExtKeyUsage) == 0) { 
                // The certificate doesn't have any extended key usage specified.
                continue;

            }

            {
                var usage__prev2 = usage;

                foreach (var (_, __usage) in cert.ExtKeyUsage) {
                    usage = __usage;
                    if (usage == ExtKeyUsageAny) { 
                        // The certificate is explicitly good for any usage.
                        _continueNextCert = true;
                        break;
                    }

                }

                usage = usage__prev2;
            }

            const ExtKeyUsage invalidUsage = -1;



NextRequestedUsage:
            {
                var i__prev2 = i;

                foreach (var (__i, __requestedUsage) in usages) {
                    i = __i;
                    requestedUsage = __requestedUsage;
                    if (requestedUsage == invalidUsage) {
                        continue;
                    }
                    {
                        var usage__prev3 = usage;

                        foreach (var (_, __usage) in cert.ExtKeyUsage) {
                            usage = __usage;
                            if (requestedUsage == usage) {
                                _continueNextRequestedUsage = true;
                                break;
                            }
                            else if (requestedUsage == ExtKeyUsageServerAuth && (usage == ExtKeyUsageNetscapeServerGatedCrypto || usage == ExtKeyUsageMicrosoftServerGatedCrypto)) { 
                                // In order to support COMODO
                                // certificate chains, we have to
                                // accept Netscape or Microsoft SGC
                                // usages as equal to ServerAuth.
                                _continueNextRequestedUsage = true;
                                break;
                            }

                        }

                        usage = usage__prev3;
                    }

                    usages[i] = invalidUsage;
                    usagesRemaining--;
                    if (usagesRemaining == 0) {
                        return false;
                    }

                }

                i = i__prev2;
            }
        }

        i = i__prev1;
    }
    return true;

}

} // end x509_package
