// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:32:02 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\verify.go
using bytes = go.bytes_package;
using asn1 = go.encoding.asn1_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using net = go.net_package;
using url = go.net.url_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        public partial struct InvalidReason // : long
        {
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
        // NameConstraintsWithoutSANs results when a leaf certificate doesn't
        // contain a Subject Alternative Name extension, but a CA certificate
        // contains name constraints.
        public static readonly var NameConstraintsWithoutSANs = 5; 
        // UnconstrainedName results when a CA certificate contains permitted
        // name constraints, but leaf certificate contains a name of an
        // unsupported or unconstrained type.
        public static readonly var UnconstrainedName = 6; 
        // TooManyConstraints results when the number of comparision operations
        // needed to check a certificate exceeds the limit set by
        // VerifyOptions.MaxConstraintComparisions. This limit exists to
        // prevent pathological certificates can consuming excessive amounts of
        // CPU time to verify.
        public static readonly var TooManyConstraints = 7; 
        // CANotAuthorizedForExtKeyUsage results when an intermediate or root
        // certificate does not permit an extended key usage that is claimed by
        // the leaf certificate.
        public static readonly var CANotAuthorizedForExtKeyUsage = 8;

        // CertificateInvalidError results when an odd error occurs. Users of this
        // library probably want to handle all these errors uniformly.
        public partial struct CertificateInvalidError
        {
            public ptr<Certificate> Cert;
            public InvalidReason Reason;
            public @string Detail;
        }

        public static @string Error(this CertificateInvalidError e)
        {

            if (e.Reason == NotAuthorizedToSign) 
                return "x509: certificate is not authorized to sign other certificates";
            else if (e.Reason == Expired) 
                return "x509: certificate has expired or is not yet valid";
            else if (e.Reason == CANotAuthorizedForThisName) 
                return "x509: a root or intermediate certificate is not authorized to sign for this name: " + e.Detail;
            else if (e.Reason == CANotAuthorizedForExtKeyUsage) 
                return "x509: a root or intermediate certificate is not authorized for an extended key usage: " + e.Detail;
            else if (e.Reason == TooManyIntermediates) 
                return "x509: too many intermediates for path length constraint";
            else if (e.Reason == IncompatibleUsage) 
                return "x509: certificate specifies an incompatible key usage: " + e.Detail;
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
        public partial struct HostnameError
        {
            public ptr<Certificate> Certificate;
            public @string Host;
        }

        public static @string Error(this HostnameError h)
        {
            var c = h.Certificate;

            @string valid = default;
            {
                var ip = net.ParseIP(h.Host);

                if (ip != null)
                { 
                    // Trying to validate an IP
                    if (len(c.IPAddresses) == 0L)
                    {
                        return "x509: cannot validate certificate for " + h.Host + " because it doesn't contain any IP SANs";
                    }
                    foreach (var (_, san) in c.IPAddresses)
                    {
                        if (len(valid) > 0L)
                        {
                            valid += ", ";
                        }
                        valid += san.String();
                    }
                else
                }                {
                    if (c.hasSANExtension())
                    {
                        valid = strings.Join(c.DNSNames, ", ");
                    }
                    else
                    {
                        valid = c.Subject.CommonName;
                    }
                }

            }

            if (len(valid) == 0L)
            {
                return "x509: certificate is not valid for any names, but wanted to match " + h.Host;
            }
            return "x509: certificate is valid for " + valid + ", not " + h.Host;
        }

        // UnknownAuthorityError results when the certificate issuer is unknown
        public partial struct UnknownAuthorityError
        {
            public ptr<Certificate> Cert; // hintErr contains an error that may be helpful in determining why an
// authority wasn't found.
            public error hintErr; // hintCert contains a possible authority certificate that was rejected
// because of the error in hintErr.
            public ptr<Certificate> hintCert;
        }

        public static @string Error(this UnknownAuthorityError e)
        {
            @string s = "x509: certificate signed by unknown authority";
            if (e.hintErr != null)
            {
                var certName = e.hintCert.Subject.CommonName;
                if (len(certName) == 0L)
                {
                    if (len(e.hintCert.Subject.Organization) > 0L)
                    {
                        certName = e.hintCert.Subject.Organization[0L];
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
        public partial struct SystemRootsError
        {
            public error Err;
        }

        public static @string Error(this SystemRootsError se)
        {
            @string msg = "x509: failed to load system roots and no roots provided";
            if (se.Err != null)
            {
                return msg + "; " + se.Err.Error();
            }
            return msg;
        }

        // errNotParsed is returned when a certificate without ASN.1 contents is
        // verified. Platform-specific verification needs the ASN.1 contents.
        private static var errNotParsed = errors.New("x509: missing ASN.1 contents; use ParseCertificate");

        // VerifyOptions contains parameters for Certificate.Verify. It's a structure
        // because other PKIX verification APIs have ended up needing many options.
        public partial struct VerifyOptions
        {
            public @string DNSName;
            public ptr<CertPool> Intermediates;
            public ptr<CertPool> Roots; // if nil, the system roots are used
            public time.Time CurrentTime; // if zero, the current time is used
// KeyUsage specifies which Extended Key Usage values are acceptable. A leaf
// certificate is accepted if it contains any of the listed values. An empty
// list means ExtKeyUsageServerAuth. To accept any key usage, include
// ExtKeyUsageAny.
//
// Certificate chains are required to nest extended key usage values,
// irrespective of this value. This matches the Windows CryptoAPI behavior,
// but not the spec.
            public slice<ExtKeyUsage> KeyUsages; // MaxConstraintComparisions is the maximum number of comparisons to
// perform when checking a given certificate's name constraints. If
// zero, a sensible default is used. This limit prevents pathalogical
// certificates from consuming excessive amounts of CPU time when
// validating.
            public long MaxConstraintComparisions;
        }

        private static readonly var leafCertificate = iota;
        private static readonly var intermediateCertificate = 0;
        private static readonly var rootCertificate = 1;

        // rfc2821Mailbox represents a “mailbox” (which is an email address to most
        // people) by breaking it into the “local” (i.e. before the '@') and “domain”
        // parts.
        private partial struct rfc2821Mailbox
        {
            public @string local;
            public @string domain;
        }

        // parseRFC2821Mailbox parses an email address into local and domain parts,
        // based on the ABNF for a “Mailbox” from RFC 2821. According to
        // https://tools.ietf.org/html/rfc5280#section-4.2.1.6 that's correct for an
        // rfc822Name from a certificate: “The format of an rfc822Name is a "Mailbox"
        // as defined in https://tools.ietf.org/html/rfc2821#section-4.1.2”.
        private static (rfc2821Mailbox, bool) parseRFC2821Mailbox(@string @in)
        {
            if (len(in) == 0L)
            {
                return (mailbox, false);
            }
            var localPartBytes = make_slice<byte>(0L, len(in) / 2L);

            if (in[0L] == '"')
            { 
                // Quoted-string = DQUOTE *qcontent DQUOTE
                // non-whitespace-control = %d1-8 / %d11 / %d12 / %d14-31 / %d127
                // qcontent = qtext / quoted-pair
                // qtext = non-whitespace-control /
                //         %d33 / %d35-91 / %d93-126
                // quoted-pair = ("\" text) / obs-qp
                // text = %d1-9 / %d11 / %d12 / %d14-127 / obs-text
                //
                // (Names beginning with “obs-” are the obsolete syntax from
                // https://tools.ietf.org/html/rfc2822#section-4. Since it has
                // been 16 years, we no longer accept that.)
                in = in[1L..];
QuotedString:
                while (true)
                {
                    if (len(in) == 0L)
                    {
                        return (mailbox, false);
                    }
                    var c = in[0L];
                    in = in[1L..];


                    if (c == '"') 
                        _breakQuotedString = true;

                        break;
                    else if (c == '\\') 
                        // quoted-pair
                        if (len(in) == 0L)
                        {
                            return (mailbox, false);
                        }
                        if (in[0L] == 11L || in[0L] == 12L || (1L <= in[0L] && in[0L] <= 9L) || (14L <= in[0L] && in[0L] <= 127L))
                        {
                            localPartBytes = append(localPartBytes, in[0L]);
                            in = in[1L..];
                        }
                        else
                        {
                            return (mailbox, false);
                        }
                    else if (c == 11L || c == 12L || c == 32L || c == 33L || c == 127L || (1L <= c && c <= 8L) || (14L <= c && c <= 31L) || (35L <= c && c <= 91L) || (93L <= c && c <= 126L)) 
                        // qtext
                        localPartBytes = append(localPartBytes, c);
                    else 
                        return (mailbox, false);
                                    }
            else
            }            { 
                // Atom ("." Atom)*
NextChar:

                while (len(in) > 0L)
                { 
                    // atext from https://tools.ietf.org/html/rfc2822#section-3.2.4
                    c = in[0L];


                    if (c == '\\') 
                    {
                        // Examples given in RFC 3696 suggest that
                        // escaped characters can appear outside of a
                        // quoted string. Several “verified” errata
                        // continue to argue the point. We choose to
                        // accept it.
                        in = in[1L..];
                        if (len(in) == 0L)
                        {
                            return (mailbox, false);
                        }
                        fallthrough = true;

                    }
                    if (fallthrough || ('0' <= c && c <= '9') || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' || c == '*' || c == '+' || c == '-' || c == '/' || c == '=' || c == '?' || c == '^' || c == '_' || c == '`' || c == '{' || c == '|' || c == '}' || c == '~' || c == '.')
                    {
                        localPartBytes = append(localPartBytes, in[0L]);
                        in = in[1L..];
                        goto __switch_break0;
                    }
                    // default: 
                        _breakNextChar = true;
                        break;

                    __switch_break0:;
                }

                if (len(localPartBytes) == 0L)
                {
                    return (mailbox, false);
                } 

                // https://tools.ietf.org/html/rfc3696#section-3
                // “period (".") may also appear, but may not be used to start
                // or end the local part, nor may two or more consecutive
                // periods appear.”
                byte twoDots = new slice<byte>(new byte[] { '.', '.' });
                if (localPartBytes[0L] == '.' || localPartBytes[len(localPartBytes) - 1L] == '.' || bytes.Contains(localPartBytes, twoDots))
                {
                    return (mailbox, false);
                }
            }
            if (len(in) == 0L || in[0L] != '@')
            {
                return (mailbox, false);
            }
            in = in[1L..]; 

            // The RFC species a format for domains, but that's known to be
            // violated in practice so we accept that anything after an '@' is the
            // domain part.
            {
                var (_, ok) = domainToReverseLabels(in);

                if (!ok)
                {
                    return (mailbox, false);
                }

            }

            mailbox.local = string(localPartBytes);
            mailbox.domain = in;
            return (mailbox, true);
        }

        // domainToReverseLabels converts a textual domain name like foo.example.com to
        // the list of labels in reverse order, e.g. ["com", "example", "foo"].
        private static (slice<@string>, bool) domainToReverseLabels(@string domain)
        {
            while (len(domain) > 0L)
            {
                {
                    var i = strings.LastIndexByte(domain, '.');

                    if (i == -1L)
                    {
                        reverseLabels = append(reverseLabels, domain);
                        domain = "";
                    }
                    else
                    {
                        reverseLabels = append(reverseLabels, domain[i + 1L..len(domain)]);
                        domain = domain[..i];
                    }

                }
            }


            if (len(reverseLabels) > 0L && len(reverseLabels[0L]) == 0L)
            { 
                // An empty label at the end indicates an absolute value.
                return (null, false);
            }
            foreach (var (_, label) in reverseLabels)
            {
                if (len(label) == 0L)
                { 
                    // Empty labels are otherwise invalid.
                    return (null, false);
                }
                foreach (var (_, c) in label)
                {
                    if (c < 33L || c > 126L)
                    { 
                        // Invalid character.
                        return (null, false);
                    }
                }
            }
            return (reverseLabels, true);
        }

        private static (bool, error) matchEmailConstraint(rfc2821Mailbox mailbox, @string constraint)
        { 
            // If the constraint contains an @, then it specifies an exact mailbox
            // name.
            if (strings.Contains(constraint, "@"))
            {
                var (constraintMailbox, ok) = parseRFC2821Mailbox(constraint);
                if (!ok)
                {
                    return (false, fmt.Errorf("x509: internal error: cannot parse constraint %q", constraint));
                }
                return (mailbox.local == constraintMailbox.local && strings.EqualFold(mailbox.domain, constraintMailbox.domain), null);
            } 

            // Otherwise the constraint is like a DNS constraint of the domain part
            // of the mailbox.
            return matchDomainConstraint(mailbox.domain, constraint);
        }

        private static (bool, error) matchURIConstraint(ref url.URL uri, @string constraint)
        { 
            // https://tools.ietf.org/html/rfc5280#section-4.2.1.10
            // “a uniformResourceIdentifier that does not include an authority
            // component with a host name specified as a fully qualified domain
            // name (e.g., if the URI either does not include an authority
            // component or includes an authority component in which the host name
            // is specified as an IP address), then the application MUST reject the
            // certificate.”

            var host = uri.Host;
            if (len(host) == 0L)
            {
                return (false, fmt.Errorf("URI with empty host (%q) cannot be matched against constraints", uri.String()));
            }
            if (strings.Contains(host, ":") && !strings.HasSuffix(host, "]"))
            {
                error err = default;
                host, _, err = net.SplitHostPort(uri.Host);
                if (err != null)
                {
                    return (false, err);
                }
            }
            if (strings.HasPrefix(host, "[") && strings.HasSuffix(host, "]") || net.ParseIP(host) != null)
            {
                return (false, fmt.Errorf("URI with IP (%q) cannot be matched against constraints", uri.String()));
            }
            return matchDomainConstraint(host, constraint);
        }

        private static (bool, error) matchIPConstraint(net.IP ip, ref net.IPNet constraint)
        {
            if (len(ip) != len(constraint.IP))
            {
                return (false, null);
            }
            foreach (var (i) in ip)
            {
                {
                    var mask = constraint.Mask[i];

                    if (ip[i] & mask != constraint.IP[i] & mask)
                    {
                        return (false, null);
                    }

                }
            }
            return (true, null);
        }

        private static (bool, error) matchDomainConstraint(@string domain, @string constraint)
        { 
            // The meaning of zero length constraints is not specified, but this
            // code follows NSS and accepts them as matching everything.
            if (len(constraint) == 0L)
            {
                return (true, null);
            }
            var (domainLabels, ok) = domainToReverseLabels(domain);
            if (!ok)
            {
                return (false, fmt.Errorf("x509: internal error: cannot parse domain %q", domain));
            } 

            // RFC 5280 says that a leading period in a domain name means that at
            // least one label must be prepended, but only for URI and email
            // constraints, not DNS constraints. The code also supports that
            // behaviour for DNS constraints.
            var mustHaveSubdomains = false;
            if (constraint[0L] == '.')
            {
                mustHaveSubdomains = true;
                constraint = constraint[1L..];
            }
            var (constraintLabels, ok) = domainToReverseLabels(constraint);
            if (!ok)
            {
                return (false, fmt.Errorf("x509: internal error: cannot parse domain %q", constraint));
            }
            if (len(domainLabels) < len(constraintLabels) || (mustHaveSubdomains && len(domainLabels) == len(constraintLabels)))
            {
                return (false, null);
            }
            foreach (var (i, constraintLabel) in constraintLabels)
            {
                if (!strings.EqualFold(constraintLabel, domainLabels[i]))
                {
                    return (false, null);
                }
            }
            return (true, null);
        }

        // checkNameConstraints checks that c permits a child certificate to claim the
        // given name, of type nameType. The argument parsedName contains the parsed
        // form of name, suitable for passing to the match function. The total number
        // of comparisons is tracked in the given count and should not exceed the given
        // limit.
        private static error checkNameConstraints(this ref Certificate c, ref long count, long maxConstraintComparisons, @string nameType, @string name, object parsedName, Func<object, object, (bool, error)> match, object permitted, object excluded)
        {
            var excludedValue = reflect.ValueOf(excluded);

            count.Value += excludedValue.Len();
            if (count > maxConstraintComparisons.Value)
            {
                return error.As(new CertificateInvalidError(c,TooManyConstraints,""));
            }
            {
                long i__prev1 = i;

                for (long i = 0L; i < excludedValue.Len(); i++)
                {
                    var constraint = excludedValue.Index(i).Interface();
                    var (match, err) = match(parsedName, constraint);
                    if (err != null)
                    {
                        return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,err.Error()));
                    }
                    if (match)
                    {
                        return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,fmt.Sprintf("%s %q is excluded by constraint %q",nameType,name,constraint)));
                    }
                }


                i = i__prev1;
            }

            var permittedValue = reflect.ValueOf(permitted);

            count.Value += permittedValue.Len();
            if (count > maxConstraintComparisons.Value)
            {
                return error.As(new CertificateInvalidError(c,TooManyConstraints,""));
            }
            var ok = true;
            {
                long i__prev1 = i;

                for (i = 0L; i < permittedValue.Len(); i++)
                {
                    constraint = permittedValue.Index(i).Interface();

                    error err = default;
                    ok, err = match(parsedName, constraint);

                    if (err != null)
                    {
                        return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,err.Error()));
                    }
                    if (ok)
                    {
                        break;
                    }
                }


                i = i__prev1;
            }

            if (!ok)
            {
                return error.As(new CertificateInvalidError(c,CANotAuthorizedForThisName,fmt.Sprintf("%s %q is not permitted by any constraint",nameType,name)));
            }
            return error.As(null);
        }

        private static readonly var checkingAgainstIssuerCert = iota;
        private static readonly var checkingAgainstLeafCert = 0;

        // ekuPermittedBy returns true iff the given extended key usage is permitted by
        // the given EKU from a certificate. Normally, this would be a simple
        // comparison plus a special case for the “any” EKU. But, in order to support
        // existing certificates, some exceptions are made.
        private static bool ekuPermittedBy(ExtKeyUsage eku, ExtKeyUsage certEKU, long context)
        {
            if (certEKU == ExtKeyUsageAny || eku == certEKU)
            {
                return true;
            } 

            // Some exceptions are made to support existing certificates. Firstly,
            // the ServerAuth and SGC EKUs are treated as a group.
            Func<ExtKeyUsage, ExtKeyUsage> mapServerAuthEKUs = eku =>
            {
                if (eku == ExtKeyUsageNetscapeServerGatedCrypto || eku == ExtKeyUsageMicrosoftServerGatedCrypto)
                {
                    return ExtKeyUsageServerAuth;
                }
                return eku;
            }
;

            eku = mapServerAuthEKUs(eku);
            certEKU = mapServerAuthEKUs(certEKU);

            if (eku == certEKU)
            {
                return true;
            } 

            // If checking a requested EKU against the list in a leaf certificate there
            // are fewer exceptions.
            if (context == checkingAgainstLeafCert)
            {
                return false;
            } 

            // ServerAuth in a CA permits ClientAuth in the leaf.
            return (eku == ExtKeyUsageClientAuth && certEKU == ExtKeyUsageServerAuth) || eku == ExtKeyUsageOCSPSigning || (eku == ExtKeyUsageMicrosoftCommercialCodeSigning || eku == ExtKeyUsageMicrosoftKernelCodeSigning) && certEKU == ExtKeyUsageCodeSigning;
        }

        // isValid performs validity checks on c given that it is a candidate to append
        // to the chain in currentChain.
        private static error isValid(this ref Certificate c, long certType, slice<ref Certificate> currentChain, ref VerifyOptions opts)
        {
            if (len(c.UnhandledCriticalExtensions) > 0L)
            {
                return error.As(new UnhandledCriticalExtension());
            }
            if (len(currentChain) > 0L)
            {
                var child = currentChain[len(currentChain) - 1L];
                if (!bytes.Equal(child.RawIssuer, c.RawSubject))
                {
                    return error.As(new CertificateInvalidError(c,NameMismatch,""));
                }
            }
            var now = opts.CurrentTime;
            if (now.IsZero())
            {
                now = time.Now();
            }
            if (now.Before(c.NotBefore) || now.After(c.NotAfter))
            {
                return error.As(new CertificateInvalidError(c,Expired,""));
            }
            var maxConstraintComparisons = opts.MaxConstraintComparisions;
            if (maxConstraintComparisons == 0L)
            {
                maxConstraintComparisons = 250000L;
            }
            long comparisonCount = 0L;

            ref Certificate leaf = default;
            if (certType == intermediateCertificate || certType == rootCertificate)
            {
                if (len(currentChain) == 0L)
                {
                    return error.As(errors.New("x509: internal error: empty chain when appending CA cert"));
                }
                leaf = currentChain[0L];
            }
            if ((certType == intermediateCertificate || certType == rootCertificate) && c.hasNameConstraints())
            {
                var (sanExtension, ok) = leaf.getSANExtension();
                if (!ok)
                { 
                    // This is the deprecated, legacy case of depending on
                    // the CN as a hostname. Chains modern enough to be
                    // using name constraints should not be depending on
                    // CNs.
                    return error.As(new CertificateInvalidError(c,NameConstraintsWithoutSANs,""));
                }
                err = forEachSAN(sanExtension, (tag, data) =>
                {

                    if (tag == nameTypeEmail) 
                        var name = string(data);
                        var (mailbox, ok) = parseRFC2821Mailbox(name);
                        if (!ok)
                        {
                            return error.As(fmt.Errorf("x509: cannot parse rfc822Name %q", mailbox));
                        }
                        {
                            var err__prev2 = err;

                            var err = c.checkNameConstraints(ref comparisonCount, maxConstraintComparisons, "email address", name, mailbox, (parsedName, constraint) =>
                            {
                                return error.As(matchEmailConstraint(parsedName._<rfc2821Mailbox>(), constraint._<@string>()));
                            }, c.PermittedEmailAddresses, c.ExcludedEmailAddresses);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                    else if (tag == nameTypeDNS) 
                        name = string(data);
                        {
                            var (_, ok) = domainToReverseLabels(name);

                            if (!ok)
                            {
                                return error.As(fmt.Errorf("x509: cannot parse dnsName %q", name));
                            }

                        }

                        {
                            var err__prev2 = err;

                            err = c.checkNameConstraints(ref comparisonCount, maxConstraintComparisons, "DNS name", name, name, (parsedName, constraint) =>
                            {
                                return error.As(matchDomainConstraint(parsedName._<@string>(), constraint._<@string>()));
                            }, c.PermittedDNSDomains, c.ExcludedDNSDomains);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                    else if (tag == nameTypeURI) 
                        name = string(data);
                        var (uri, err) = url.Parse(name);
                        if (err != null)
                        {
                            return error.As(fmt.Errorf("x509: internal error: URI SAN %q failed to parse", name));
                        }
                        {
                            var err__prev2 = err;

                            err = c.checkNameConstraints(ref comparisonCount, maxConstraintComparisons, "URI", name, uri, (parsedName, constraint) =>
                            {
                                return error.As(matchURIConstraint(parsedName._<ref url.URL>(), constraint._<@string>()));
                            }, c.PermittedURIDomains, c.ExcludedURIDomains);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                    else if (tag == nameTypeIP) 
                        var ip = net.IP(data);
                        {
                            var l = len(ip);

                            if (l != net.IPv4len && l != net.IPv6len)
                            {
                                return error.As(fmt.Errorf("x509: internal error: IP SAN %x failed to parse", data));
                            }

                        }

                        {
                            var err__prev2 = err;

                            err = c.checkNameConstraints(ref comparisonCount, maxConstraintComparisons, "IP address", ip.String(), ip, (parsedName, constraint) =>
                            {
                                return error.As(matchIPConstraint(parsedName._<net.IP>(), constraint._<ref net.IPNet>()));
                            }, c.PermittedIPRanges, c.ExcludedIPRanges);

                            if (err != null)
                            {
                                return error.As(err);
                            }

                            err = err__prev2;

                        }
                    else                                         return error.As(null);
                });

                if (err != null)
                {
                    return error.As(err);
                }
            }
            var checkEKUs = certType == intermediateCertificate; 

            // If no extended key usages are specified, then all are acceptable.
            if (checkEKUs && (len(c.ExtKeyUsage) == 0L && len(c.UnknownExtKeyUsage) == 0L))
            {
                checkEKUs = false;
            } 

            // If the “any” key usage is permitted, then no more checks are needed.
            if (checkEKUs)
            {
                {
                    var caEKU__prev1 = caEKU;

                    foreach (var (_, __caEKU) in c.ExtKeyUsage)
                    {
                        caEKU = __caEKU;
                        comparisonCount++;
                        if (caEKU == ExtKeyUsageAny)
                        {
                            checkEKUs = false;
                            break;
                        }
                    }

                    caEKU = caEKU__prev1;
                }

            }
            if (checkEKUs)
            {
NextEKU:

                {
                    var eku__prev1 = eku;

                    foreach (var (_, __eku) in leaf.ExtKeyUsage)
                    {
                        eku = __eku;
                        if (comparisonCount > maxConstraintComparisons)
                        {
                            return error.As(new CertificateInvalidError(c,TooManyConstraints,""));
                        }
                        {
                            var caEKU__prev2 = caEKU;

                            foreach (var (_, __caEKU) in c.ExtKeyUsage)
                            {
                                caEKU = __caEKU;
                                comparisonCount++;
                                if (ekuPermittedBy(eku, caEKU, checkingAgainstIssuerCert))
                                {
                                    _continueNextEKU = true;
                                    break;
                                }
                            }

                            caEKU = caEKU__prev2;
                        }

                        var (oid, _) = oidFromExtKeyUsage(eku);
                        return error.As(new CertificateInvalidError(c,CANotAuthorizedForExtKeyUsage,fmt.Sprintf("EKU not permitted: %#v",oid)));
                    }

                    eku = eku__prev1;
                }
NextUnknownEKU:
                {
                    var eku__prev1 = eku;

                    foreach (var (_, __eku) in leaf.UnknownExtKeyUsage)
                    {
                        eku = __eku;
                        if (comparisonCount > maxConstraintComparisons)
                        {
                            return error.As(new CertificateInvalidError(c,TooManyConstraints,""));
                        }
                        {
                            var caEKU__prev2 = caEKU;

                            foreach (var (_, __caEKU) in c.UnknownExtKeyUsage)
                            {
                                caEKU = __caEKU;
                                comparisonCount++;
                                if (caEKU.Equal(eku))
                                {
                                    _continueNextUnknownEKU = true;
                                    break;
                                }
                            }

                            caEKU = caEKU__prev2;
                        }

                        return error.As(new CertificateInvalidError(c,CANotAuthorizedForExtKeyUsage,fmt.Sprintf("EKU not permitted: %#v",eku)));
                    }

                    eku = eku__prev1;
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
            if (certType == intermediateCertificate && (!c.BasicConstraintsValid || !c.IsCA))
            {
                return error.As(new CertificateInvalidError(c,NotAuthorizedToSign,""));
            }
            if (c.BasicConstraintsValid && c.MaxPathLen >= 0L)
            {
                var numIntermediates = len(currentChain) - 1L;
                if (numIntermediates > c.MaxPathLen)
                {
                    return error.As(new CertificateInvalidError(c,TooManyIntermediates,""));
                }
            }
            return error.As(null);
        }

        // formatOID formats an ASN.1 OBJECT IDENTIFER in the common, dotted style.
        private static @string formatOID(asn1.ObjectIdentifier oid)
        {
            @string ret = "";
            foreach (var (i, v) in oid)
            {
                if (i > 0L)
                {
                    ret += ".";
                }
                ret += strconv.Itoa(v);
            }
            return ret;
        }

        // Verify attempts to verify c by building one or more chains from c to a
        // certificate in opts.Roots, using certificates in opts.Intermediates if
        // needed. If successful, it returns one or more chains where the first
        // element of the chain is c and the last element is from opts.Roots.
        //
        // If opts.Roots is nil and system roots are unavailable the returned error
        // will be of type SystemRootsError.
        //
        // Name constraints in the intermediates will be applied to all names claimed
        // in the chain, not just opts.DNSName. Thus it is invalid for a leaf to claim
        // example.com if an intermediate doesn't permit it, even if example.com is not
        // the name being validated. Note that DirectoryName constraints are not
        // supported.
        //
        // Extended Key Usage values are enforced down a chain, so an intermediate or
        // root that enumerates EKUs prevents a leaf from asserting an EKU not in that
        // list.
        //
        // WARNING: this function doesn't do any revocation checking.
        private static (slice<slice<ref Certificate>>, error) Verify(this ref Certificate c, VerifyOptions opts)
        { 
            // Platform-specific verification needs the ASN.1 contents so
            // this makes the behavior consistent across platforms.
            if (len(c.Raw) == 0L)
            {
                return (null, errNotParsed);
            }
            if (opts.Intermediates != null)
            {
                foreach (var (_, intermediate) in opts.Intermediates.certs)
                {
                    if (len(intermediate.Raw) == 0L)
                    {
                        return (null, errNotParsed);
                    }
                }
            } 

            // Use Windows's own verification and chain building.
            if (opts.Roots == null && runtime.GOOS == "windows")
            {
                return c.systemVerify(ref opts);
            }
            if (opts.Roots == null)
            {
                opts.Roots = systemRootsPool();
                if (opts.Roots == null)
                {
                    return (null, new SystemRootsError(systemRootsErr));
                }
            }
            err = c.isValid(leafCertificate, null, ref opts);
            if (err != null)
            {
                return;
            }
            if (len(opts.DNSName) > 0L)
            {
                err = c.VerifyHostname(opts.DNSName);
                if (err != null)
                {
                    return;
                }
            }
            var requestedKeyUsages = make_slice<ExtKeyUsage>(len(opts.KeyUsages));
            copy(requestedKeyUsages, opts.KeyUsages);
            if (len(requestedKeyUsages) == 0L)
            {
                requestedKeyUsages = append(requestedKeyUsages, ExtKeyUsageServerAuth);
            } 

            // If no key usages are specified, then any are acceptable.
            var checkEKU = len(c.ExtKeyUsage) > 0L;

            {
                var eku__prev1 = eku;

                foreach (var (_, __eku) in requestedKeyUsages)
                {
                    eku = __eku;
                    if (eku == ExtKeyUsageAny)
                    {
                        checkEKU = false;
                        break;
                    }
                }

                eku = eku__prev1;
            }

            if (checkEKU)
            {
                var foundMatch = false;
NextUsage:

                {
                    var eku__prev1 = eku;

                    foreach (var (_, __eku) in requestedKeyUsages)
                    {
                        eku = __eku;
                        {
                            var leafEKU__prev2 = leafEKU;

                            foreach (var (_, __leafEKU) in c.ExtKeyUsage)
                            {
                                leafEKU = __leafEKU;
                                if (ekuPermittedBy(eku, leafEKU, checkingAgainstLeafCert))
                                {
                                    foundMatch = true;
                                    _breakNextUsage = true;
                                    break;
                                }
                            }

                            leafEKU = leafEKU__prev2;
                        }

                    }

                    eku = eku__prev1;
                }
                if (!foundMatch)
                {
                    @string msg = "leaf contains the following, recognized EKUs: ";

                    {
                        var leafEKU__prev1 = leafEKU;

                        foreach (var (__i, __leafEKU) in c.ExtKeyUsage)
                        {
                            i = __i;
                            leafEKU = __leafEKU;
                            var (oid, ok) = oidFromExtKeyUsage(leafEKU);
                            if (!ok)
                            {
                                continue;
                            }
                            if (i > 0L)
                            {
                                msg += ", ";
                            }
                            msg += formatOID(oid);
                        }

                        leafEKU = leafEKU__prev1;
                    }

                    return (null, new CertificateInvalidError(c,IncompatibleUsage,msg));
                }
            }
            slice<slice<ref Certificate>> candidateChains = default;
            if (opts.Roots.contains(c))
            {
                candidateChains = append(candidateChains, new slice<ref Certificate>(new ref Certificate[] { c }));
            }
            else
            {
                candidateChains, err = c.buildChains(make_map<long, slice<slice<ref Certificate>>>(), new slice<ref Certificate>(new ref Certificate[] { c }), ref opts);

                if (err != null)
                {
                    return (null, err);
                }
            }
            return (candidateChains, null);
        }

        private static slice<ref Certificate> appendToFreshChain(slice<ref Certificate> chain, ref Certificate cert)
        {
            var n = make_slice<ref Certificate>(len(chain) + 1L);
            copy(n, chain);
            n[len(chain)] = cert;
            return n;
        }

        private static (slice<slice<ref Certificate>>, error) buildChains(this ref Certificate c, map<long, slice<slice<ref Certificate>>> cache, slice<ref Certificate> currentChain, ref VerifyOptions opts)
        {
            var (possibleRoots, failedRoot, rootErr) = opts.Roots.findVerifiedParents(c);
nextRoot:

            foreach (var (_, rootNum) in possibleRoots)
            {
                var root = opts.Roots.certs[rootNum];

                {
                    var cert__prev2 = cert;

                    foreach (var (_, __cert) in currentChain)
                    {
                        cert = __cert;
                        if (cert.Equal(root))
                        {
                            _continuenextRoot = true;
                            break;
                        }
                    }

                    cert = cert__prev2;
                }

                err = root.isValid(rootCertificate, currentChain, opts);
                if (err != null)
                {
                    continue;
                }
                chains = append(chains, appendToFreshChain(currentChain, root));
            }
            var (possibleIntermediates, failedIntermediate, intermediateErr) = opts.Intermediates.findVerifiedParents(c);
nextIntermediate:

            foreach (var (_, intermediateNum) in possibleIntermediates)
            {
                var intermediate = opts.Intermediates.certs[intermediateNum];
                {
                    var cert__prev2 = cert;

                    foreach (var (_, __cert) in currentChain)
                    {
                        cert = __cert;
                        if (cert.Equal(intermediate))
                        {
                            _continuenextIntermediate = true;
                            break;
                        }
                    }

                    cert = cert__prev2;
                }

                err = intermediate.isValid(intermediateCertificate, currentChain, opts);
                if (err != null)
                {
                    continue;
                }
                slice<slice<ref Certificate>> childChains = default;
                var (childChains, ok) = cache[intermediateNum];
                if (!ok)
                {
                    childChains, err = intermediate.buildChains(cache, appendToFreshChain(currentChain, intermediate), opts);
                    cache[intermediateNum] = childChains;
                }
                chains = append(chains, childChains);
            }
            if (len(chains) > 0L)
            {
                err = null;
            }
            if (len(chains) == 0L && err == null)
            {
                var hintErr = rootErr;
                var hintCert = failedRoot;
                if (hintErr == null)
                {
                    hintErr = intermediateErr;
                    hintCert = failedIntermediate;
                }
                err = new UnknownAuthorityError(c,hintErr,hintCert);
            }
            return;
        }

        private static bool matchHostnames(@string pattern, @string host)
        {
            host = strings.TrimSuffix(host, ".");
            pattern = strings.TrimSuffix(pattern, ".");

            if (len(pattern) == 0L || len(host) == 0L)
            {
                return false;
            }
            var patternParts = strings.Split(pattern, ".");
            var hostParts = strings.Split(host, ".");

            if (len(patternParts) != len(hostParts))
            {
                return false;
            }
            foreach (var (i, patternPart) in patternParts)
            {
                if (i == 0L && patternPart == "*")
                {
                    continue;
                }
                if (patternPart != hostParts[i])
                {
                    return false;
                }
            }
            return true;
        }

        // toLowerCaseASCII returns a lower-case version of in. See RFC 6125 6.4.1. We use
        // an explicitly ASCII function to avoid any sharp corners resulting from
        // performing Unicode operations on DNS labels.
        private static @string toLowerCaseASCII(@string @in)
        { 
            // If the string is already lower-case then there's nothing to do.
            var isAlreadyLowerCase = true;
            {
                var c__prev1 = c;

                foreach (var (_, __c) in in)
                {
                    c = __c;
                    if (c == utf8.RuneError)
                    { 
                        // If we get a UTF-8 error then there might be
                        // upper-case ASCII bytes in the invalid sequence.
                        isAlreadyLowerCase = false;
                        break;
                    }
                    if ('A' <= c && c <= 'Z')
                    {
                        isAlreadyLowerCase = false;
                        break;
                    }
                }

                c = c__prev1;
            }

            if (isAlreadyLowerCase)
            {
                return in;
            }
            slice<byte> @out = (slice<byte>)in;
            {
                var c__prev1 = c;

                foreach (var (__i, __c) in out)
                {
                    i = __i;
                    c = __c;
                    if ('A' <= c && c <= 'Z')
                    {
                        out[i] += 'a' - 'A';
                    }
                }

                c = c__prev1;
            }

            return string(out);
        }

        // VerifyHostname returns nil if c is a valid certificate for the named host.
        // Otherwise it returns an error describing the mismatch.
        private static error VerifyHostname(this ref Certificate c, @string h)
        { 
            // IP addresses may be written in [ ].
            var candidateIP = h;
            if (len(h) >= 3L && h[0L] == '[' && h[len(h) - 1L] == ']')
            {
                candidateIP = h[1L..len(h) - 1L];
            }
            {
                var ip = net.ParseIP(candidateIP);

                if (ip != null)
                { 
                    // We only match IP addresses against IP SANs.
                    // https://tools.ietf.org/html/rfc6125#appendix-B.2
                    foreach (var (_, candidate) in c.IPAddresses)
                    {
                        if (ip.Equal(candidate))
                        {
                            return error.As(null);
                        }
                    }
                    return error.As(new HostnameError(c,candidateIP));
                }

            }

            var lowered = toLowerCaseASCII(h);

            if (c.hasSANExtension())
            {
                foreach (var (_, match) in c.DNSNames)
                {
                    if (matchHostnames(toLowerCaseASCII(match), lowered))
                    {
                        return error.As(null);
                    }
                } 
                // If Subject Alt Name is given, we ignore the common name.
            }
            else if (matchHostnames(toLowerCaseASCII(c.Subject.CommonName), lowered))
            {
                return error.As(null);
            }
            return error.As(new HostnameError(c,h));
        }
    }
}}
