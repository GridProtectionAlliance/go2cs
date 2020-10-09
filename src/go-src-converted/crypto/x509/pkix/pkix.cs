// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pkix contains shared, low level structures used for ASN.1 parsing
// and serialization of X.509 certificates, CRL and OCSP.
// package pkix -- go2cs converted at 2020 October 09 04:54:52 UTC
// import "crypto/x509/pkix" ==> using pkix = go.crypto.x509.pkix_package
// Original source: C:\Go\src\crypto\x509\pkix\pkix.go
using asn1 = go.encoding.asn1_package;
using hex = go.encoding.hex_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using time = go.time_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace crypto {
namespace x509
{
    public static partial class pkix_package
    {
        // AlgorithmIdentifier represents the ASN.1 structure of the same name. See RFC
        // 5280, section 4.1.1.2.
        public partial struct AlgorithmIdentifier
        {
            public asn1.ObjectIdentifier Algorithm;
            [Description("asn1:\"optional\"")]
            public asn1.RawValue Parameters;
        }

        public partial struct RDNSequence // : slice<RelativeDistinguishedNameSET>
        {
        }

        private static map attributeTypeNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"2.5.4.6":"C","2.5.4.10":"O","2.5.4.11":"OU","2.5.4.3":"CN","2.5.4.5":"SERIALNUMBER","2.5.4.7":"L","2.5.4.8":"ST","2.5.4.9":"STREET","2.5.4.17":"POSTALCODE",};

        // String returns a string representation of the sequence r,
        // roughly following the RFC 2253 Distinguished Names syntax.
        public static @string String(this RDNSequence r)
        {
            @string s = "";
            for (long i = 0L; i < len(r); i++)
            {
                var rdn = r[len(r) - 1L - i];
                if (i > 0L)
                {
                    s += ",";
                }

                foreach (var (j, tv) in rdn)
                {
                    if (j > 0L)
                    {
                        s += "+";
                    }

                    var oidString = tv.Type.String();
                    var (typeName, ok) = attributeTypeNames[oidString];
                    if (!ok)
                    {
                        var (derBytes, err) = asn1.Marshal(tv.Value);
                        if (err == null)
                        {
                            s += oidString + "=#" + hex.EncodeToString(derBytes);
                            continue; // No value escaping necessary.
                        }

                        typeName = oidString;

                    }

                    var valueString = fmt.Sprint(tv.Value);
                    var escaped = make_slice<int>(0L, len(valueString));

                    foreach (var (k, c) in valueString)
                    {
                        var escape = false;

                        switch (c)
                        {
                            case ',': 

                            case '+': 

                            case '"': 

                            case '\\': 

                            case '<': 

                            case '>': 

                            case ';': 
                                escape = true;
                                break;
                            case ' ': 
                                escape = k == 0L || k == len(valueString) - 1L;
                                break;
                            case '#': 
                                escape = k == 0L;
                                break;
                        }

                        if (escape)
                        {
                            escaped = append(escaped, '\\', c);
                        }
                        else
                        {
                            escaped = append(escaped, c);
                        }

                    }
                    s += typeName + "=" + string(escaped);

                }

            }


            return s;

        }

        public partial struct RelativeDistinguishedNameSET // : slice<AttributeTypeAndValue>
        {
        }

        // AttributeTypeAndValue mirrors the ASN.1 structure of the same name in
        // RFC 5280, Section 4.1.2.4.
        public partial struct AttributeTypeAndValue
        {
            public asn1.ObjectIdentifier Type;
        }

        // AttributeTypeAndValueSET represents a set of ASN.1 sequences of
        // AttributeTypeAndValue sequences from RFC 2986 (PKCS #10).
        public partial struct AttributeTypeAndValueSET
        {
            public asn1.ObjectIdentifier Type;
            [Description("asn1:\"set\"")]
            public slice<slice<AttributeTypeAndValue>> Value;
        }

        // Extension represents the ASN.1 structure of the same name. See RFC
        // 5280, section 4.2.
        public partial struct Extension
        {
            public asn1.ObjectIdentifier Id;
            [Description("asn1:\"optional\"")]
            public bool Critical;
            public slice<byte> Value;
        }

        // Name represents an X.509 distinguished name. This only includes the common
        // elements of a DN. Note that Name is only an approximation of the X.509
        // structure. If an accurate representation is needed, asn1.Unmarshal the raw
        // subject or issuer as an RDNSequence.
        public partial struct Name
        {
            public slice<@string> Country;
            public slice<@string> Organization;
            public slice<@string> OrganizationalUnit;
            public slice<@string> Locality;
            public slice<@string> Province;
            public slice<@string> StreetAddress;
            public slice<@string> PostalCode;
            public @string SerialNumber; // Names contains all parsed attributes. When parsing distinguished names,
// this can be used to extract non-standard attributes that are not parsed
// by this package. When marshaling to RDNSequences, the Names field is
// ignored, see ExtraNames.
            public @string CommonName; // Names contains all parsed attributes. When parsing distinguished names,
// this can be used to extract non-standard attributes that are not parsed
// by this package. When marshaling to RDNSequences, the Names field is
// ignored, see ExtraNames.
            public slice<AttributeTypeAndValue> Names; // ExtraNames contains attributes to be copied, raw, into any marshaled
// distinguished names. Values override any attributes with the same OID.
// The ExtraNames field is not populated when parsing, see Names.
            public slice<AttributeTypeAndValue> ExtraNames;
        }

        // FillFromRDNSequence populates n from the provided RDNSequence.
        // Multi-entry RDNs are flattened, all entries are added to the
        // relevant n fields, and the grouping is not preserved.
        private static void FillFromRDNSequence(this ptr<Name> _addr_n, ptr<RDNSequence> _addr_rdns)
        {
            ref Name n = ref _addr_n.val;
            ref RDNSequence rdns = ref _addr_rdns.val;

            foreach (var (_, rdn) in rdns)
            {
                if (len(rdn) == 0L)
                {
                    continue;
                }

                foreach (var (_, atv) in rdn)
                {
                    n.Names = append(n.Names, atv);
                    @string (value, ok) = atv.Value._<@string>();
                    if (!ok)
                    {
                        continue;
                    }

                    var t = atv.Type;
                    if (len(t) == 4L && t[0L] == 2L && t[1L] == 5L && t[2L] == 4L)
                    {
                        switch (t[3L])
                        {
                            case 3L: 
                                n.CommonName = value;
                                break;
                            case 5L: 
                                n.SerialNumber = value;
                                break;
                            case 6L: 
                                n.Country = append(n.Country, value);
                                break;
                            case 7L: 
                                n.Locality = append(n.Locality, value);
                                break;
                            case 8L: 
                                n.Province = append(n.Province, value);
                                break;
                            case 9L: 
                                n.StreetAddress = append(n.StreetAddress, value);
                                break;
                            case 10L: 
                                n.Organization = append(n.Organization, value);
                                break;
                            case 11L: 
                                n.OrganizationalUnit = append(n.OrganizationalUnit, value);
                                break;
                            case 17L: 
                                n.PostalCode = append(n.PostalCode, value);
                                break;
                        }

                    }

                }

            }

        }

        private static long oidCountry = new slice<long>(new long[] { 2, 5, 4, 6 });        private static long oidOrganization = new slice<long>(new long[] { 2, 5, 4, 10 });        private static long oidOrganizationalUnit = new slice<long>(new long[] { 2, 5, 4, 11 });        private static long oidCommonName = new slice<long>(new long[] { 2, 5, 4, 3 });        private static long oidSerialNumber = new slice<long>(new long[] { 2, 5, 4, 5 });        private static long oidLocality = new slice<long>(new long[] { 2, 5, 4, 7 });        private static long oidProvince = new slice<long>(new long[] { 2, 5, 4, 8 });        private static long oidStreetAddress = new slice<long>(new long[] { 2, 5, 4, 9 });        private static long oidPostalCode = new slice<long>(new long[] { 2, 5, 4, 17 });

        // appendRDNs appends a relativeDistinguishedNameSET to the given RDNSequence
        // and returns the new value. The relativeDistinguishedNameSET contains an
        // attributeTypeAndValue for each of the given values. See RFC 5280, A.1, and
        // search for AttributeTypeAndValue.
        public static RDNSequence appendRDNs(this Name n, RDNSequence @in, slice<@string> values, asn1.ObjectIdentifier oid)
        {
            if (len(values) == 0L || oidInAttributeTypeAndValue(oid, n.ExtraNames))
            {
                return in;
            }

            var s = make_slice<AttributeTypeAndValue>(len(values));
            foreach (var (i, value) in values)
            {
                s[i].Type = oid;
                s[i].Value = value;
            }
            return append(in, s);

        }

        // ToRDNSequence converts n into a single RDNSequence. The following
        // attributes are encoded as multi-value RDNs:
        //
        //  - Country
        //  - Organization
        //  - OrganizationalUnit
        //  - Locality
        //  - Province
        //  - StreetAddress
        //  - PostalCode
        //
        // Each ExtraNames entry is encoded as an individual RDN.
        public static RDNSequence ToRDNSequence(this Name n)
        {
            RDNSequence ret = default;

            ret = n.appendRDNs(ret, n.Country, oidCountry);
            ret = n.appendRDNs(ret, n.Province, oidProvince);
            ret = n.appendRDNs(ret, n.Locality, oidLocality);
            ret = n.appendRDNs(ret, n.StreetAddress, oidStreetAddress);
            ret = n.appendRDNs(ret, n.PostalCode, oidPostalCode);
            ret = n.appendRDNs(ret, n.Organization, oidOrganization);
            ret = n.appendRDNs(ret, n.OrganizationalUnit, oidOrganizationalUnit);
            if (len(n.CommonName) > 0L)
            {
                ret = n.appendRDNs(ret, new slice<@string>(new @string[] { n.CommonName }), oidCommonName);
            }

            if (len(n.SerialNumber) > 0L)
            {
                ret = n.appendRDNs(ret, new slice<@string>(new @string[] { n.SerialNumber }), oidSerialNumber);
            }

            foreach (var (_, atv) in n.ExtraNames)
            {
                ret = append(ret, new slice<AttributeTypeAndValue>(new AttributeTypeAndValue[] { atv }));
            }
            return ret;

        }

        // String returns the string form of n, roughly following
        // the RFC 2253 Distinguished Names syntax.
        public static @string String(this Name n)
        {
            RDNSequence rdns = default; 
            // If there are no ExtraNames, surface the parsed value (all entries in
            // Names) instead.
            if (n.ExtraNames == null)
            {
                foreach (var (_, atv) in n.Names)
                {
                    var t = atv.Type;
                    if (len(t) == 4L && t[0L] == 2L && t[1L] == 5L && t[2L] == 4L)
                    {
                        switch (t[3L])
                        {
                            case 3L: 
                                // These attributes were already parsed into named fields.

                            case 5L: 
                                // These attributes were already parsed into named fields.

                            case 6L: 
                                // These attributes were already parsed into named fields.

                            case 7L: 
                                // These attributes were already parsed into named fields.

                            case 8L: 
                                // These attributes were already parsed into named fields.

                            case 9L: 
                                // These attributes were already parsed into named fields.

                            case 10L: 
                                // These attributes were already parsed into named fields.

                            case 11L: 
                                // These attributes were already parsed into named fields.

                            case 17L: 
                                // These attributes were already parsed into named fields.
                                continue;
                                break;
                        }

                    } 
                    // Place non-standard parsed values at the beginning of the sequence
                    // so they will be at the end of the string. See Issue 39924.
                    rdns = append(rdns, new slice<AttributeTypeAndValue>(new AttributeTypeAndValue[] { atv }));

                }

            }

            rdns = append(rdns, n.ToRDNSequence());
            return rdns.String();

        }

        // oidInAttributeTypeAndValue reports whether a type with the given OID exists
        // in atv.
        private static bool oidInAttributeTypeAndValue(asn1.ObjectIdentifier oid, slice<AttributeTypeAndValue> atv)
        {
            foreach (var (_, a) in atv)
            {
                if (a.Type.Equal(oid))
                {
                    return true;
                }

            }
            return false;

        }

        // CertificateList represents the ASN.1 structure of the same name. See RFC
        // 5280, section 5.1. Use Certificate.CheckCRLSignature to verify the
        // signature.
        public partial struct CertificateList
        {
            public TBSCertificateList TBSCertList;
            public AlgorithmIdentifier SignatureAlgorithm;
            public asn1.BitString SignatureValue;
        }

        // HasExpired reports whether certList should have been updated by now.
        private static bool HasExpired(this ptr<CertificateList> _addr_certList, time.Time now)
        {
            ref CertificateList certList = ref _addr_certList.val;

            return !now.Before(certList.TBSCertList.NextUpdate);
        }

        // TBSCertificateList represents the ASN.1 structure of the same name. See RFC
        // 5280, section 5.1.
        public partial struct TBSCertificateList
        {
            public asn1.RawContent Raw;
            [Description("asn1:\"optional,default:0\"")]
            public long Version;
            public AlgorithmIdentifier Signature;
            public RDNSequence Issuer;
            public time.Time ThisUpdate;
            [Description("asn1:\"optional\"")]
            public time.Time NextUpdate;
            [Description("asn1:\"optional\"")]
            public slice<RevokedCertificate> RevokedCertificates;
            [Description("asn1:\"tag:0,optional,explicit\"")]
            public slice<Extension> Extensions;
        }

        // RevokedCertificate represents the ASN.1 structure of the same name. See RFC
        // 5280, section 5.1.
        public partial struct RevokedCertificate
        {
            public ptr<big.Int> SerialNumber;
            public time.Time RevocationTime;
            [Description("asn1:\"optional\"")]
            public slice<Extension> Extensions;
        }
    }
}}}
