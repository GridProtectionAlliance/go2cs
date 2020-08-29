// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2020 August 29 08:31:25 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\handshake_messages.go
using bytes = go.bytes_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        private partial struct clientHelloMsg
        {
            public slice<byte> raw;
            public ushort vers;
            public slice<byte> random;
            public slice<byte> sessionId;
            public slice<ushort> cipherSuites;
            public slice<byte> compressionMethods;
            public bool nextProtoNeg;
            public @string serverName;
            public bool ocspStapling;
            public bool scts;
            public slice<CurveID> supportedCurves;
            public slice<byte> supportedPoints;
            public bool ticketSupported;
            public slice<byte> sessionTicket;
            public slice<SignatureScheme> supportedSignatureAlgorithms;
            public slice<byte> secureRenegotiation;
            public bool secureRenegotiationSupported;
            public slice<@string> alpnProtocols;
        }

        private static bool equal(this ref clientHelloMsg m, object i)
        {
            ref clientHelloMsg (m1, ok) = i._<ref clientHelloMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && m.vers == m1.vers && bytes.Equal(m.random, m1.random) && bytes.Equal(m.sessionId, m1.sessionId) && eqUint16s(m.cipherSuites, m1.cipherSuites) && bytes.Equal(m.compressionMethods, m1.compressionMethods) && m.nextProtoNeg == m1.nextProtoNeg && m.serverName == m1.serverName && m.ocspStapling == m1.ocspStapling && m.scts == m1.scts && eqCurveIDs(m.supportedCurves, m1.supportedCurves) && bytes.Equal(m.supportedPoints, m1.supportedPoints) && m.ticketSupported == m1.ticketSupported && bytes.Equal(m.sessionTicket, m1.sessionTicket) && eqSignatureAlgorithms(m.supportedSignatureAlgorithms, m1.supportedSignatureAlgorithms) && m.secureRenegotiationSupported == m1.secureRenegotiationSupported && bytes.Equal(m.secureRenegotiation, m1.secureRenegotiation) && eqStrings(m.alpnProtocols, m1.alpnProtocols);
        }

        private static slice<byte> marshal(this ref clientHelloMsg _m) => func(_m, (ref clientHelloMsg m, Defer _, Panic panic, Recover __) =>
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            long length = 2L + 32L + 1L + len(m.sessionId) + 2L + len(m.cipherSuites) * 2L + 1L + len(m.compressionMethods);
            long numExtensions = 0L;
            long extensionsLength = 0L;
            if (m.nextProtoNeg)
            {
                numExtensions++;
            }
            if (m.ocspStapling)
            {
                extensionsLength += 1L + 2L + 2L;
                numExtensions++;
            }
            if (len(m.serverName) > 0L)
            {
                extensionsLength += 5L + len(m.serverName);
                numExtensions++;
            }
            if (len(m.supportedCurves) > 0L)
            {
                extensionsLength += 2L + 2L * len(m.supportedCurves);
                numExtensions++;
            }
            if (len(m.supportedPoints) > 0L)
            {
                extensionsLength += 1L + len(m.supportedPoints);
                numExtensions++;
            }
            if (m.ticketSupported)
            {
                extensionsLength += len(m.sessionTicket);
                numExtensions++;
            }
            if (len(m.supportedSignatureAlgorithms) > 0L)
            {
                extensionsLength += 2L + 2L * len(m.supportedSignatureAlgorithms);
                numExtensions++;
            }
            if (m.secureRenegotiationSupported)
            {
                extensionsLength += 1L + len(m.secureRenegotiation);
                numExtensions++;
            }
            if (len(m.alpnProtocols) > 0L)
            {
                extensionsLength += 2L;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in m.alpnProtocols)
                    {
                        s = __s;
                        {
                            var l__prev2 = l;

                            var l = len(s);

                            if (l == 0L || l > 255L)
                            {
                                panic("invalid ALPN protocol");
                            }

                            l = l__prev2;

                        }
                        extensionsLength++;
                        extensionsLength += len(s);
                    }

                    s = s__prev1;
                }

                numExtensions++;
            }
            if (m.scts)
            {
                numExtensions++;
            }
            if (numExtensions > 0L)
            {
                extensionsLength += 4L * numExtensions;
                length += 2L + extensionsLength;
            }
            var x = make_slice<byte>(4L + length);
            x[0L] = typeClientHello;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            x[4L] = uint8(m.vers >> (int)(8L));
            x[5L] = uint8(m.vers);
            copy(x[6L..38L], m.random);
            x[38L] = uint8(len(m.sessionId));
            copy(x[39L..39L + len(m.sessionId)], m.sessionId);
            var y = x[39L + len(m.sessionId)..];
            y[0L] = uint8(len(m.cipherSuites) >> (int)(7L));
            y[1L] = uint8(len(m.cipherSuites) << (int)(1L));
            foreach (var (i, suite) in m.cipherSuites)
            {
                y[2L + i * 2L] = uint8(suite >> (int)(8L));
                y[3L + i * 2L] = uint8(suite);
            }
            var z = y[2L + len(m.cipherSuites) * 2L..];
            z[0L] = uint8(len(m.compressionMethods));
            copy(z[1L..], m.compressionMethods);

            z = z[1L + len(m.compressionMethods)..];
            if (numExtensions > 0L)
            {
                z[0L] = byte(extensionsLength >> (int)(8L));
                z[1L] = byte(extensionsLength);
                z = z[2L..];
            }
            if (m.nextProtoNeg)
            {
                z[0L] = byte(extensionNextProtoNeg >> (int)(8L));
                z[1L] = byte(extensionNextProtoNeg & 0xffUL); 
                // The length is always 0
                z = z[4L..];
            }
            if (len(m.serverName) > 0L)
            {
                z[0L] = byte(extensionServerName >> (int)(8L));
                z[1L] = byte(extensionServerName & 0xffUL);
                l = len(m.serverName) + 5L;
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                z = z[4L..]; 

                // RFC 3546, section 3.1
                //
                // struct {
                //     NameType name_type;
                //     select (name_type) {
                //         case host_name: HostName;
                //     } name;
                // } ServerName;
                //
                // enum {
                //     host_name(0), (255)
                // } NameType;
                //
                // opaque HostName<1..2^16-1>;
                //
                // struct {
                //     ServerName server_name_list<1..2^16-1>
                // } ServerNameList;

                z[0L] = byte((len(m.serverName) + 3L) >> (int)(8L));
                z[1L] = byte(len(m.serverName) + 3L);
                z[3L] = byte(len(m.serverName) >> (int)(8L));
                z[4L] = byte(len(m.serverName));
                copy(z[5L..], (slice<byte>)m.serverName);
                z = z[l..];
            }
            if (m.ocspStapling)
            { 
                // RFC 4366, section 3.6
                z[0L] = byte(extensionStatusRequest >> (int)(8L));
                z[1L] = byte(extensionStatusRequest);
                z[2L] = 0L;
                z[3L] = 5L;
                z[4L] = 1L; // OCSP type
                // Two zero valued uint16s for the two lengths.
                z = z[9L..];
            }
            if (len(m.supportedCurves) > 0L)
            { 
                // http://tools.ietf.org/html/rfc4492#section-5.5.1
                z[0L] = byte(extensionSupportedCurves >> (int)(8L));
                z[1L] = byte(extensionSupportedCurves);
                l = 2L + 2L * len(m.supportedCurves);
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                l -= 2L;
                z[4L] = byte(l >> (int)(8L));
                z[5L] = byte(l);
                z = z[6L..];
                foreach (var (_, curve) in m.supportedCurves)
                {
                    z[0L] = byte(curve >> (int)(8L));
                    z[1L] = byte(curve);
                    z = z[2L..];
                }
            }
            if (len(m.supportedPoints) > 0L)
            { 
                // http://tools.ietf.org/html/rfc4492#section-5.5.2
                z[0L] = byte(extensionSupportedPoints >> (int)(8L));
                z[1L] = byte(extensionSupportedPoints);
                l = 1L + len(m.supportedPoints);
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                l--;
                z[4L] = byte(l);
                z = z[5L..];
                foreach (var (_, pointFormat) in m.supportedPoints)
                {
                    z[0L] = pointFormat;
                    z = z[1L..];
                }
            }
            if (m.ticketSupported)
            { 
                // http://tools.ietf.org/html/rfc5077#section-3.2
                z[0L] = byte(extensionSessionTicket >> (int)(8L));
                z[1L] = byte(extensionSessionTicket);
                l = len(m.sessionTicket);
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                z = z[4L..];
                copy(z, m.sessionTicket);
                z = z[len(m.sessionTicket)..];
            }
            if (len(m.supportedSignatureAlgorithms) > 0L)
            { 
                // https://tools.ietf.org/html/rfc5246#section-7.4.1.4.1
                z[0L] = byte(extensionSignatureAlgorithms >> (int)(8L));
                z[1L] = byte(extensionSignatureAlgorithms);
                l = 2L + 2L * len(m.supportedSignatureAlgorithms);
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                z = z[4L..];

                l -= 2L;
                z[0L] = byte(l >> (int)(8L));
                z[1L] = byte(l);
                z = z[2L..];
                foreach (var (_, sigAlgo) in m.supportedSignatureAlgorithms)
                {
                    z[0L] = byte(sigAlgo >> (int)(8L));
                    z[1L] = byte(sigAlgo);
                    z = z[2L..];
                }
            }
            if (m.secureRenegotiationSupported)
            {
                z[0L] = byte(extensionRenegotiationInfo >> (int)(8L));
                z[1L] = byte(extensionRenegotiationInfo & 0xffUL);
                z[2L] = 0L;
                z[3L] = byte(len(m.secureRenegotiation) + 1L);
                z[4L] = byte(len(m.secureRenegotiation));
                z = z[5L..];
                copy(z, m.secureRenegotiation);
                z = z[len(m.secureRenegotiation)..];
            }
            if (len(m.alpnProtocols) > 0L)
            {
                z[0L] = byte(extensionALPN >> (int)(8L));
                z[1L] = byte(extensionALPN & 0xffUL);
                var lengths = z[2L..];
                z = z[6L..];

                long stringsLength = 0L;
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in m.alpnProtocols)
                    {
                        s = __s;
                        l = len(s);
                        z[0L] = byte(l);
                        copy(z[1L..], s);
                        z = z[1L + l..];
                        stringsLength += 1L + l;
                    }

                    s = s__prev1;
                }

                lengths[2L] = byte(stringsLength >> (int)(8L));
                lengths[3L] = byte(stringsLength);
                stringsLength += 2L;
                lengths[0L] = byte(stringsLength >> (int)(8L));
                lengths[1L] = byte(stringsLength);
            }
            if (m.scts)
            { 
                // https://tools.ietf.org/html/rfc6962#section-3.3.1
                z[0L] = byte(extensionSCT >> (int)(8L));
                z[1L] = byte(extensionSCT); 
                // zero uint16 for the zero-length extension_data
                z = z[4L..];
            }
            m.raw = x;

            return x;
        });

        private static bool unmarshal(this ref clientHelloMsg m, slice<byte> data)
        {
            if (len(data) < 42L)
            {
                return false;
            }
            m.raw = data;
            m.vers = uint16(data[4L]) << (int)(8L) | uint16(data[5L]);
            m.random = data[6L..38L];
            var sessionIdLen = int(data[38L]);
            if (sessionIdLen > 32L || len(data) < 39L + sessionIdLen)
            {
                return false;
            }
            m.sessionId = data[39L..39L + sessionIdLen];
            data = data[39L + sessionIdLen..];
            if (len(data) < 2L)
            {
                return false;
            } 
            // cipherSuiteLen is the number of bytes of cipher suite numbers. Since
            // they are uint16s, the number must be even.
            var cipherSuiteLen = int(data[0L]) << (int)(8L) | int(data[1L]);
            if (cipherSuiteLen % 2L == 1L || len(data) < 2L + cipherSuiteLen)
            {
                return false;
            }
            var numCipherSuites = cipherSuiteLen / 2L;
            m.cipherSuites = make_slice<ushort>(numCipherSuites);
            {
                long i__prev1 = i;

                for (long i = 0L; i < numCipherSuites; i++)
                {
                    m.cipherSuites[i] = uint16(data[2L + 2L * i]) << (int)(8L) | uint16(data[3L + 2L * i]);
                    if (m.cipherSuites[i] == scsvRenegotiation)
                    {
                        m.secureRenegotiationSupported = true;
                    }
                }


                i = i__prev1;
            }
            data = data[2L + cipherSuiteLen..];
            if (len(data) < 1L)
            {
                return false;
            }
            var compressionMethodsLen = int(data[0L]);
            if (len(data) < 1L + compressionMethodsLen)
            {
                return false;
            }
            m.compressionMethods = data[1L..1L + compressionMethodsLen];

            data = data[1L + compressionMethodsLen..];

            m.nextProtoNeg = false;
            m.serverName = "";
            m.ocspStapling = false;
            m.ticketSupported = false;
            m.sessionTicket = null;
            m.supportedSignatureAlgorithms = null;
            m.alpnProtocols = null;
            m.scts = false;

            if (len(data) == 0L)
            { 
                // ClientHello is optionally followed by extension data
                return true;
            }
            if (len(data) < 2L)
            {
                return false;
            }
            var extensionsLength = int(data[0L]) << (int)(8L) | int(data[1L]);
            data = data[2L..];
            if (extensionsLength != len(data))
            {
                return false;
            }
            while (len(data) != 0L)
            {
                if (len(data) < 4L)
                {
                    return false;
                }
                var extension = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
                var length = int(data[2L]) << (int)(8L) | int(data[3L]);
                data = data[4L..];
                if (len(data) < length)
                {
                    return false;
                }

                if (extension == extensionServerName) 
                    var d = data[..length];
                    if (len(d) < 2L)
                    {
                        return false;
                    }
                    var namesLen = int(d[0L]) << (int)(8L) | int(d[1L]);
                    d = d[2L..];
                    if (len(d) != namesLen)
                    {
                        return false;
                    }
                    while (len(d) > 0L)
                    {
                        if (len(d) < 3L)
                        {
                            return false;
                        }
                        var nameType = d[0L];
                        var nameLen = int(d[1L]) << (int)(8L) | int(d[2L]);
                        d = d[3L..];
                        if (len(d) < nameLen)
                        {
                            return false;
                        }
                        if (nameType == 0L)
                        {
                            m.serverName = string(d[..nameLen]); 
                            // An SNI value may not include a
                            // trailing dot. See
                            // https://tools.ietf.org/html/rfc6066#section-3.
                            if (strings.HasSuffix(m.serverName, "."))
                            {
                                return false;
                            }
                            break;
                        }
                        d = d[nameLen..];
                    }
                else if (extension == extensionNextProtoNeg) 
                    if (length > 0L)
                    {
                        return false;
                    }
                    m.nextProtoNeg = true;
                else if (extension == extensionStatusRequest) 
                    m.ocspStapling = length > 0L && data[0L] == statusTypeOCSP;
                else if (extension == extensionSupportedCurves) 
                    // http://tools.ietf.org/html/rfc4492#section-5.5.1
                    if (length < 2L)
                    {
                        return false;
                    }
                    var l = int(data[0L]) << (int)(8L) | int(data[1L]);
                    if (l % 2L == 1L || length != l + 2L)
                    {
                        return false;
                    }
                    var numCurves = l / 2L;
                    m.supportedCurves = make_slice<CurveID>(numCurves);
                    d = data[2L..];
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < numCurves; i++)
                        {
                            m.supportedCurves[i] = CurveID(d[0L]) << (int)(8L) | CurveID(d[1L]);
                            d = d[2L..];
                        }


                        i = i__prev2;
                    }
                else if (extension == extensionSupportedPoints) 
                    // http://tools.ietf.org/html/rfc4492#section-5.5.2
                    if (length < 1L)
                    {
                        return false;
                    }
                    l = int(data[0L]);
                    if (length != l + 1L)
                    {
                        return false;
                    }
                    m.supportedPoints = make_slice<byte>(l);
                    copy(m.supportedPoints, data[1L..]);
                else if (extension == extensionSessionTicket) 
                    // http://tools.ietf.org/html/rfc5077#section-3.2
                    m.ticketSupported = true;
                    m.sessionTicket = data[..length];
                else if (extension == extensionSignatureAlgorithms) 
                    // https://tools.ietf.org/html/rfc5246#section-7.4.1.4.1
                    if (length < 2L || length & 1L != 0L)
                    {
                        return false;
                    }
                    l = int(data[0L]) << (int)(8L) | int(data[1L]);
                    if (l != length - 2L)
                    {
                        return false;
                    }
                    var n = l / 2L;
                    d = data[2L..];
                    m.supportedSignatureAlgorithms = make_slice<SignatureScheme>(n);
                    {
                        long i__prev2 = i;

                        foreach (var (__i) in m.supportedSignatureAlgorithms)
                        {
                            i = __i;
                            m.supportedSignatureAlgorithms[i] = SignatureScheme(d[0L]) << (int)(8L) | SignatureScheme(d[1L]);
                            d = d[2L..];
                        }

                        i = i__prev2;
                    }
                else if (extension == extensionRenegotiationInfo) 
                    if (length == 0L)
                    {
                        return false;
                    }
                    d = data[..length];
                    l = int(d[0L]);
                    d = d[1L..];
                    if (l != len(d))
                    {
                        return false;
                    }
                    m.secureRenegotiation = d;
                    m.secureRenegotiationSupported = true;
                else if (extension == extensionALPN) 
                    if (length < 2L)
                    {
                        return false;
                    }
                    l = int(data[0L]) << (int)(8L) | int(data[1L]);
                    if (l != length - 2L)
                    {
                        return false;
                    }
                    d = data[2L..length];
                    while (len(d) != 0L)
                    {
                        var stringLen = int(d[0L]);
                        d = d[1L..];
                        if (stringLen == 0L || stringLen > len(d))
                        {
                            return false;
                        }
                        m.alpnProtocols = append(m.alpnProtocols, string(d[..stringLen]));
                        d = d[stringLen..];
                    }
                else if (extension == extensionSCT) 
                    m.scts = true;
                    if (length != 0L)
                    {
                        return false;
                    }
                                data = data[length..];
            }


            return true;
        }

        private partial struct serverHelloMsg
        {
            public slice<byte> raw;
            public ushort vers;
            public slice<byte> random;
            public slice<byte> sessionId;
            public ushort cipherSuite;
            public byte compressionMethod;
            public bool nextProtoNeg;
            public slice<@string> nextProtos;
            public bool ocspStapling;
            public slice<slice<byte>> scts;
            public bool ticketSupported;
            public slice<byte> secureRenegotiation;
            public bool secureRenegotiationSupported;
            public @string alpnProtocol;
        }

        private static bool equal(this ref serverHelloMsg m, object i)
        {
            ref serverHelloMsg (m1, ok) = i._<ref serverHelloMsg>();
            if (!ok)
            {
                return false;
            }
            if (len(m.scts) != len(m1.scts))
            {
                return false;
            }
            foreach (var (i, sct) in m.scts)
            {
                if (!bytes.Equal(sct, m1.scts[i]))
                {
                    return false;
                }
            }
            return bytes.Equal(m.raw, m1.raw) && m.vers == m1.vers && bytes.Equal(m.random, m1.random) && bytes.Equal(m.sessionId, m1.sessionId) && m.cipherSuite == m1.cipherSuite && m.compressionMethod == m1.compressionMethod && m.nextProtoNeg == m1.nextProtoNeg && eqStrings(m.nextProtos, m1.nextProtos) && m.ocspStapling == m1.ocspStapling && m.ticketSupported == m1.ticketSupported && m.secureRenegotiationSupported == m1.secureRenegotiationSupported && bytes.Equal(m.secureRenegotiation, m1.secureRenegotiation) && m.alpnProtocol == m1.alpnProtocol;
        }

        private static slice<byte> marshal(this ref serverHelloMsg _m) => func(_m, (ref serverHelloMsg m, Defer _, Panic panic, Recover __) =>
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            long length = 38L + len(m.sessionId);
            long numExtensions = 0L;
            long extensionsLength = 0L;

            long nextProtoLen = 0L;
            if (m.nextProtoNeg)
            {
                numExtensions++;
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in m.nextProtos)
                    {
                        v = __v;
                        nextProtoLen += len(v);
                    }

                    v = v__prev1;
                }

                nextProtoLen += len(m.nextProtos);
                extensionsLength += nextProtoLen;
            }
            if (m.ocspStapling)
            {
                numExtensions++;
            }
            if (m.ticketSupported)
            {
                numExtensions++;
            }
            if (m.secureRenegotiationSupported)
            {
                extensionsLength += 1L + len(m.secureRenegotiation);
                numExtensions++;
            }
            {
                var alpnLen__prev1 = alpnLen;

                var alpnLen = len(m.alpnProtocol);

                if (alpnLen > 0L)
                {
                    if (alpnLen >= 256L)
                    {
                        panic("invalid ALPN protocol");
                    }
                    extensionsLength += 2L + 1L + alpnLen;
                    numExtensions++;
                }

                alpnLen = alpnLen__prev1;

            }
            long sctLen = 0L;
            if (len(m.scts) > 0L)
            {
                {
                    var sct__prev1 = sct;

                    foreach (var (_, __sct) in m.scts)
                    {
                        sct = __sct;
                        sctLen += len(sct) + 2L;
                    }

                    sct = sct__prev1;
                }

                extensionsLength += 2L + sctLen;
                numExtensions++;
            }
            if (numExtensions > 0L)
            {
                extensionsLength += 4L * numExtensions;
                length += 2L + extensionsLength;
            }
            var x = make_slice<byte>(4L + length);
            x[0L] = typeServerHello;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            x[4L] = uint8(m.vers >> (int)(8L));
            x[5L] = uint8(m.vers);
            copy(x[6L..38L], m.random);
            x[38L] = uint8(len(m.sessionId));
            copy(x[39L..39L + len(m.sessionId)], m.sessionId);
            var z = x[39L + len(m.sessionId)..];
            z[0L] = uint8(m.cipherSuite >> (int)(8L));
            z[1L] = uint8(m.cipherSuite);
            z[2L] = m.compressionMethod;

            z = z[3L..];
            if (numExtensions > 0L)
            {
                z[0L] = byte(extensionsLength >> (int)(8L));
                z[1L] = byte(extensionsLength);
                z = z[2L..];
            }
            if (m.nextProtoNeg)
            {
                z[0L] = byte(extensionNextProtoNeg >> (int)(8L));
                z[1L] = byte(extensionNextProtoNeg & 0xffUL);
                z[2L] = byte(nextProtoLen >> (int)(8L));
                z[3L] = byte(nextProtoLen);
                z = z[4L..];

                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in m.nextProtos)
                    {
                        v = __v;
                        var l = len(v);
                        if (l > 255L)
                        {
                            l = 255L;
                        }
                        z[0L] = byte(l);
                        copy(z[1L..], (slice<byte>)v[0L..l]);
                        z = z[1L + l..];
                    }

                    v = v__prev1;
                }

            }
            if (m.ocspStapling)
            {
                z[0L] = byte(extensionStatusRequest >> (int)(8L));
                z[1L] = byte(extensionStatusRequest);
                z = z[4L..];
            }
            if (m.ticketSupported)
            {
                z[0L] = byte(extensionSessionTicket >> (int)(8L));
                z[1L] = byte(extensionSessionTicket);
                z = z[4L..];
            }
            if (m.secureRenegotiationSupported)
            {
                z[0L] = byte(extensionRenegotiationInfo >> (int)(8L));
                z[1L] = byte(extensionRenegotiationInfo & 0xffUL);
                z[2L] = 0L;
                z[3L] = byte(len(m.secureRenegotiation) + 1L);
                z[4L] = byte(len(m.secureRenegotiation));
                z = z[5L..];
                copy(z, m.secureRenegotiation);
                z = z[len(m.secureRenegotiation)..];
            }
            {
                var alpnLen__prev1 = alpnLen;

                alpnLen = len(m.alpnProtocol);

                if (alpnLen > 0L)
                {
                    z[0L] = byte(extensionALPN >> (int)(8L));
                    z[1L] = byte(extensionALPN & 0xffUL);
                    l = 2L + 1L + alpnLen;
                    z[2L] = byte(l >> (int)(8L));
                    z[3L] = byte(l);
                    l -= 2L;
                    z[4L] = byte(l >> (int)(8L));
                    z[5L] = byte(l);
                    l -= 1L;
                    z[6L] = byte(l);
                    copy(z[7L..], (slice<byte>)m.alpnProtocol);
                    z = z[7L + alpnLen..];
                }

                alpnLen = alpnLen__prev1;

            }
            if (sctLen > 0L)
            {
                z[0L] = byte(extensionSCT >> (int)(8L));
                z[1L] = byte(extensionSCT);
                l = sctLen + 2L;
                z[2L] = byte(l >> (int)(8L));
                z[3L] = byte(l);
                z[4L] = byte(sctLen >> (int)(8L));
                z[5L] = byte(sctLen);

                z = z[6L..];
                {
                    var sct__prev1 = sct;

                    foreach (var (_, __sct) in m.scts)
                    {
                        sct = __sct;
                        z[0L] = byte(len(sct) >> (int)(8L));
                        z[1L] = byte(len(sct));
                        copy(z[2L..], sct);
                        z = z[len(sct) + 2L..];
                    }

                    sct = sct__prev1;
                }

            }
            m.raw = x;

            return x;
        });

        private static bool unmarshal(this ref serverHelloMsg m, slice<byte> data)
        {
            if (len(data) < 42L)
            {
                return false;
            }
            m.raw = data;
            m.vers = uint16(data[4L]) << (int)(8L) | uint16(data[5L]);
            m.random = data[6L..38L];
            var sessionIdLen = int(data[38L]);
            if (sessionIdLen > 32L || len(data) < 39L + sessionIdLen)
            {
                return false;
            }
            m.sessionId = data[39L..39L + sessionIdLen];
            data = data[39L + sessionIdLen..];
            if (len(data) < 3L)
            {
                return false;
            }
            m.cipherSuite = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
            m.compressionMethod = data[2L];
            data = data[3L..];

            m.nextProtoNeg = false;
            m.nextProtos = null;
            m.ocspStapling = false;
            m.scts = null;
            m.ticketSupported = false;
            m.alpnProtocol = "";

            if (len(data) == 0L)
            { 
                // ServerHello is optionally followed by extension data
                return true;
            }
            if (len(data) < 2L)
            {
                return false;
            }
            var extensionsLength = int(data[0L]) << (int)(8L) | int(data[1L]);
            data = data[2L..];
            if (len(data) != extensionsLength)
            {
                return false;
            }
            while (len(data) != 0L)
            {
                if (len(data) < 4L)
                {
                    return false;
                }
                var extension = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
                var length = int(data[2L]) << (int)(8L) | int(data[3L]);
                data = data[4L..];
                if (len(data) < length)
                {
                    return false;
                }

                if (extension == extensionNextProtoNeg) 
                    m.nextProtoNeg = true;
                    var d = data[..length];
                    while (len(d) > 0L)
                    {
                        var l = int(d[0L]);
                        d = d[1L..];
                        if (l == 0L || l > len(d))
                        {
                            return false;
                        }
                        m.nextProtos = append(m.nextProtos, string(d[..l]));
                        d = d[l..];
                    }
                else if (extension == extensionStatusRequest) 
                    if (length > 0L)
                    {
                        return false;
                    }
                    m.ocspStapling = true;
                else if (extension == extensionSessionTicket) 
                    if (length > 0L)
                    {
                        return false;
                    }
                    m.ticketSupported = true;
                else if (extension == extensionRenegotiationInfo) 
                    if (length == 0L)
                    {
                        return false;
                    }
                    d = data[..length];
                    l = int(d[0L]);
                    d = d[1L..];
                    if (l != len(d))
                    {
                        return false;
                    }
                    m.secureRenegotiation = d;
                    m.secureRenegotiationSupported = true;
                else if (extension == extensionALPN) 
                    d = data[..length];
                    if (len(d) < 3L)
                    {
                        return false;
                    }
                    l = int(d[0L]) << (int)(8L) | int(d[1L]);
                    if (l != len(d) - 2L)
                    {
                        return false;
                    }
                    d = d[2L..];
                    l = int(d[0L]);
                    if (l != len(d) - 1L)
                    {
                        return false;
                    }
                    d = d[1L..];
                    if (len(d) == 0L)
                    { 
                        // ALPN protocols must not be empty.
                        return false;
                    }
                    m.alpnProtocol = string(d);
                else if (extension == extensionSCT) 
                    d = data[..length];

                    if (len(d) < 2L)
                    {
                        return false;
                    }
                    l = int(d[0L]) << (int)(8L) | int(d[1L]);
                    d = d[2L..];
                    if (len(d) != l || l == 0L)
                    {
                        return false;
                    }
                    m.scts = make_slice<slice<byte>>(0L, 3L);
                    while (len(d) != 0L)
                    {
                        if (len(d) < 2L)
                        {
                            return false;
                        }
                        var sctLen = int(d[0L]) << (int)(8L) | int(d[1L]);
                        d = d[2L..];
                        if (sctLen == 0L || len(d) < sctLen)
                        {
                            return false;
                        }
                        m.scts = append(m.scts, d[..sctLen]);
                        d = d[sctLen..];
                    }
                                data = data[length..];
            }


            return true;
        }

        private partial struct certificateMsg
        {
            public slice<byte> raw;
            public slice<slice<byte>> certificates;
        }

        private static bool equal(this ref certificateMsg m, object i)
        {
            ref certificateMsg (m1, ok) = i._<ref certificateMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && eqByteSlices(m.certificates, m1.certificates);
        }

        private static slice<byte> marshal(this ref certificateMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            long i = default;
            {
                var slice__prev1 = slice;

                foreach (var (_, __slice) in m.certificates)
                {
                    slice = __slice;
                    i += len(slice);
                }

                slice = slice__prev1;
            }

            long length = 3L + 3L * len(m.certificates) + i;
            x = make_slice<byte>(4L + length);
            x[0L] = typeCertificate;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);

            var certificateOctets = length - 3L;
            x[4L] = uint8(certificateOctets >> (int)(16L));
            x[5L] = uint8(certificateOctets >> (int)(8L));
            x[6L] = uint8(certificateOctets);

            var y = x[7L..];
            {
                var slice__prev1 = slice;

                foreach (var (_, __slice) in m.certificates)
                {
                    slice = __slice;
                    y[0L] = uint8(len(slice) >> (int)(16L));
                    y[1L] = uint8(len(slice) >> (int)(8L));
                    y[2L] = uint8(len(slice));
                    copy(y[3L..], slice);
                    y = y[3L + len(slice)..];
                }

                slice = slice__prev1;
            }

            m.raw = x;
            return;
        }

        private static bool unmarshal(this ref certificateMsg m, slice<byte> data)
        {
            if (len(data) < 7L)
            {
                return false;
            }
            m.raw = data;
            var certsLen = uint32(data[4L]) << (int)(16L) | uint32(data[5L]) << (int)(8L) | uint32(data[6L]);
            if (uint32(len(data)) != certsLen + 7L)
            {
                return false;
            }
            long numCerts = 0L;
            var d = data[7L..];
            while (certsLen > 0L)
            {
                if (len(d) < 4L)
                {
                    return false;
                }
                var certLen = uint32(d[0L]) << (int)(16L) | uint32(d[1L]) << (int)(8L) | uint32(d[2L]);
                if (uint32(len(d)) < 3L + certLen)
                {
                    return false;
                }
                d = d[3L + certLen..];
                certsLen -= 3L + certLen;
                numCerts++;
            }


            m.certificates = make_slice<slice<byte>>(numCerts);
            d = data[7L..];
            for (long i = 0L; i < numCerts; i++)
            {
                certLen = uint32(d[0L]) << (int)(16L) | uint32(d[1L]) << (int)(8L) | uint32(d[2L]);
                m.certificates[i] = d[3L..3L + certLen];
                d = d[3L + certLen..];
            }


            return true;
        }

        private partial struct serverKeyExchangeMsg
        {
            public slice<byte> raw;
            public slice<byte> key;
        }

        private static bool equal(this ref serverKeyExchangeMsg m, object i)
        {
            ref serverKeyExchangeMsg (m1, ok) = i._<ref serverKeyExchangeMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && bytes.Equal(m.key, m1.key);
        }

        private static slice<byte> marshal(this ref serverKeyExchangeMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            var length = len(m.key);
            var x = make_slice<byte>(length + 4L);
            x[0L] = typeServerKeyExchange;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            copy(x[4L..], m.key);

            m.raw = x;
            return x;
        }

        private static bool unmarshal(this ref serverKeyExchangeMsg m, slice<byte> data)
        {
            m.raw = data;
            if (len(data) < 4L)
            {
                return false;
            }
            m.key = data[4L..];
            return true;
        }

        private partial struct certificateStatusMsg
        {
            public slice<byte> raw;
            public byte statusType;
            public slice<byte> response;
        }

        private static bool equal(this ref certificateStatusMsg m, object i)
        {
            ref certificateStatusMsg (m1, ok) = i._<ref certificateStatusMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && m.statusType == m1.statusType && bytes.Equal(m.response, m1.response);
        }

        private static slice<byte> marshal(this ref certificateStatusMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            slice<byte> x = default;
            if (m.statusType == statusTypeOCSP)
            {
                x = make_slice<byte>(4L + 4L + len(m.response));
                x[0L] = typeCertificateStatus;
                var l = len(m.response) + 4L;
                x[1L] = byte(l >> (int)(16L));
                x[2L] = byte(l >> (int)(8L));
                x[3L] = byte(l);
                x[4L] = statusTypeOCSP;

                l -= 4L;
                x[5L] = byte(l >> (int)(16L));
                x[6L] = byte(l >> (int)(8L));
                x[7L] = byte(l);
                copy(x[8L..], m.response);
            }
            else
            {
                x = new slice<byte>(new byte[] { typeCertificateStatus, 0, 0, 1, m.statusType });
            }
            m.raw = x;
            return x;
        }

        private static bool unmarshal(this ref certificateStatusMsg m, slice<byte> data)
        {
            m.raw = data;
            if (len(data) < 5L)
            {
                return false;
            }
            m.statusType = data[4L];

            m.response = null;
            if (m.statusType == statusTypeOCSP)
            {
                if (len(data) < 8L)
                {
                    return false;
                }
                var respLen = uint32(data[5L]) << (int)(16L) | uint32(data[6L]) << (int)(8L) | uint32(data[7L]);
                if (uint32(len(data)) != 4L + 4L + respLen)
                {
                    return false;
                }
                m.response = data[8L..];
            }
            return true;
        }

        private partial struct serverHelloDoneMsg
        {
        }

        private static bool equal(this ref serverHelloDoneMsg m, object i)
        {
            ref serverHelloDoneMsg (_, ok) = i._<ref serverHelloDoneMsg>();
            return ok;
        }

        private static slice<byte> marshal(this ref serverHelloDoneMsg m)
        {
            var x = make_slice<byte>(4L);
            x[0L] = typeServerHelloDone;
            return x;
        }

        private static bool unmarshal(this ref serverHelloDoneMsg m, slice<byte> data)
        {
            return len(data) == 4L;
        }

        private partial struct clientKeyExchangeMsg
        {
            public slice<byte> raw;
            public slice<byte> ciphertext;
        }

        private static bool equal(this ref clientKeyExchangeMsg m, object i)
        {
            ref clientKeyExchangeMsg (m1, ok) = i._<ref clientKeyExchangeMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && bytes.Equal(m.ciphertext, m1.ciphertext);
        }

        private static slice<byte> marshal(this ref clientKeyExchangeMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            var length = len(m.ciphertext);
            var x = make_slice<byte>(length + 4L);
            x[0L] = typeClientKeyExchange;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            copy(x[4L..], m.ciphertext);

            m.raw = x;
            return x;
        }

        private static bool unmarshal(this ref clientKeyExchangeMsg m, slice<byte> data)
        {
            m.raw = data;
            if (len(data) < 4L)
            {
                return false;
            }
            var l = int(data[1L]) << (int)(16L) | int(data[2L]) << (int)(8L) | int(data[3L]);
            if (l != len(data) - 4L)
            {
                return false;
            }
            m.ciphertext = data[4L..];
            return true;
        }

        private partial struct finishedMsg
        {
            public slice<byte> raw;
            public slice<byte> verifyData;
        }

        private static bool equal(this ref finishedMsg m, object i)
        {
            ref finishedMsg (m1, ok) = i._<ref finishedMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && bytes.Equal(m.verifyData, m1.verifyData);
        }

        private static slice<byte> marshal(this ref finishedMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            x = make_slice<byte>(4L + len(m.verifyData));
            x[0L] = typeFinished;
            x[3L] = byte(len(m.verifyData));
            copy(x[4L..], m.verifyData);
            m.raw = x;
            return;
        }

        private static bool unmarshal(this ref finishedMsg m, slice<byte> data)
        {
            m.raw = data;
            if (len(data) < 4L)
            {
                return false;
            }
            m.verifyData = data[4L..];
            return true;
        }

        private partial struct nextProtoMsg
        {
            public slice<byte> raw;
            public @string proto;
        }

        private static bool equal(this ref nextProtoMsg m, object i)
        {
            ref nextProtoMsg (m1, ok) = i._<ref nextProtoMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && m.proto == m1.proto;
        }

        private static slice<byte> marshal(this ref nextProtoMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            }
            var l = len(m.proto);
            if (l > 255L)
            {
                l = 255L;
            }
            long padding = 32L - (l + 2L) % 32L;
            var length = l + padding + 2L;
            var x = make_slice<byte>(length + 4L);
            x[0L] = typeNextProtocol;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);

            var y = x[4L..];
            y[0L] = byte(l);
            copy(y[1L..], (slice<byte>)m.proto[0L..l]);
            y = y[1L + l..];
            y[0L] = byte(padding);

            m.raw = x;

            return x;
        }

        private static bool unmarshal(this ref nextProtoMsg m, slice<byte> data)
        {
            m.raw = data;

            if (len(data) < 5L)
            {
                return false;
            }
            data = data[4L..];
            var protoLen = int(data[0L]);
            data = data[1L..];
            if (len(data) < protoLen)
            {
                return false;
            }
            m.proto = string(data[0L..protoLen]);
            data = data[protoLen..];

            if (len(data) < 1L)
            {
                return false;
            }
            var paddingLen = int(data[0L]);
            data = data[1L..];
            if (len(data) != paddingLen)
            {
                return false;
            }
            return true;
        }

        private partial struct certificateRequestMsg
        {
            public slice<byte> raw; // hasSignatureAndHash indicates whether this message includes a list
// of signature and hash functions. This change was introduced with TLS
// 1.2.
            public bool hasSignatureAndHash;
            public slice<byte> certificateTypes;
            public slice<SignatureScheme> supportedSignatureAlgorithms;
            public slice<slice<byte>> certificateAuthorities;
        }

        private static bool equal(this ref certificateRequestMsg m, object i)
        {
            ref certificateRequestMsg (m1, ok) = i._<ref certificateRequestMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && bytes.Equal(m.certificateTypes, m1.certificateTypes) && eqByteSlices(m.certificateAuthorities, m1.certificateAuthorities) && eqSignatureAlgorithms(m.supportedSignatureAlgorithms, m1.supportedSignatureAlgorithms);
        }

        private static slice<byte> marshal(this ref certificateRequestMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            } 

            // See http://tools.ietf.org/html/rfc4346#section-7.4.4
            long length = 1L + len(m.certificateTypes) + 2L;
            long casLength = 0L;
            {
                var ca__prev1 = ca;

                foreach (var (_, __ca) in m.certificateAuthorities)
                {
                    ca = __ca;
                    casLength += 2L + len(ca);
                }

                ca = ca__prev1;
            }

            length += casLength;

            if (m.hasSignatureAndHash)
            {
                length += 2L + 2L * len(m.supportedSignatureAlgorithms);
            }
            x = make_slice<byte>(4L + length);
            x[0L] = typeCertificateRequest;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);

            x[4L] = uint8(len(m.certificateTypes));

            copy(x[5L..], m.certificateTypes);
            var y = x[5L + len(m.certificateTypes)..];

            if (m.hasSignatureAndHash)
            {
                var n = len(m.supportedSignatureAlgorithms) * 2L;
                y[0L] = uint8(n >> (int)(8L));
                y[1L] = uint8(n);
                y = y[2L..];
                foreach (var (_, sigAlgo) in m.supportedSignatureAlgorithms)
                {
                    y[0L] = uint8(sigAlgo >> (int)(8L));
                    y[1L] = uint8(sigAlgo);
                    y = y[2L..];
                }
            }
            y[0L] = uint8(casLength >> (int)(8L));
            y[1L] = uint8(casLength);
            y = y[2L..];
            {
                var ca__prev1 = ca;

                foreach (var (_, __ca) in m.certificateAuthorities)
                {
                    ca = __ca;
                    y[0L] = uint8(len(ca) >> (int)(8L));
                    y[1L] = uint8(len(ca));
                    y = y[2L..];
                    copy(y, ca);
                    y = y[len(ca)..];
                }

                ca = ca__prev1;
            }

            m.raw = x;
            return;
        }

        private static bool unmarshal(this ref certificateRequestMsg m, slice<byte> data)
        {
            m.raw = data;

            if (len(data) < 5L)
            {
                return false;
            }
            var length = uint32(data[1L]) << (int)(16L) | uint32(data[2L]) << (int)(8L) | uint32(data[3L]);
            if (uint32(len(data)) - 4L != length)
            {
                return false;
            }
            var numCertTypes = int(data[4L]);
            data = data[5L..];
            if (numCertTypes == 0L || len(data) <= numCertTypes)
            {
                return false;
            }
            m.certificateTypes = make_slice<byte>(numCertTypes);
            if (copy(m.certificateTypes, data) != numCertTypes)
            {
                return false;
            }
            data = data[numCertTypes..];

            if (m.hasSignatureAndHash)
            {
                if (len(data) < 2L)
                {
                    return false;
                }
                var sigAndHashLen = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
                data = data[2L..];
                if (sigAndHashLen & 1L != 0L)
                {
                    return false;
                }
                if (len(data) < int(sigAndHashLen))
                {
                    return false;
                }
                var numSigAlgos = sigAndHashLen / 2L;
                m.supportedSignatureAlgorithms = make_slice<SignatureScheme>(numSigAlgos);
                foreach (var (i) in m.supportedSignatureAlgorithms)
                {
                    m.supportedSignatureAlgorithms[i] = SignatureScheme(data[0L]) << (int)(8L) | SignatureScheme(data[1L]);
                    data = data[2L..];
                }
            }
            if (len(data) < 2L)
            {
                return false;
            }
            var casLength = uint16(data[0L]) << (int)(8L) | uint16(data[1L]);
            data = data[2L..];
            if (len(data) < int(casLength))
            {
                return false;
            }
            var cas = make_slice<byte>(casLength);
            copy(cas, data);
            data = data[casLength..];

            m.certificateAuthorities = null;
            while (len(cas) > 0L)
            {
                if (len(cas) < 2L)
                {
                    return false;
                }
                var caLen = uint16(cas[0L]) << (int)(8L) | uint16(cas[1L]);
                cas = cas[2L..];

                if (len(cas) < int(caLen))
                {
                    return false;
                }
                m.certificateAuthorities = append(m.certificateAuthorities, cas[..caLen]);
                cas = cas[caLen..];
            }


            return len(data) == 0L;
        }

        private partial struct certificateVerifyMsg
        {
            public slice<byte> raw;
            public bool hasSignatureAndHash;
            public SignatureScheme signatureAlgorithm;
            public slice<byte> signature;
        }

        private static bool equal(this ref certificateVerifyMsg m, object i)
        {
            ref certificateVerifyMsg (m1, ok) = i._<ref certificateVerifyMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && m.hasSignatureAndHash == m1.hasSignatureAndHash && m.signatureAlgorithm == m1.signatureAlgorithm && bytes.Equal(m.signature, m1.signature);
        }

        private static slice<byte> marshal(this ref certificateVerifyMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            } 

            // See http://tools.ietf.org/html/rfc4346#section-7.4.8
            var siglength = len(m.signature);
            long length = 2L + siglength;
            if (m.hasSignatureAndHash)
            {
                length += 2L;
            }
            x = make_slice<byte>(4L + length);
            x[0L] = typeCertificateVerify;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            var y = x[4L..];
            if (m.hasSignatureAndHash)
            {
                y[0L] = uint8(m.signatureAlgorithm >> (int)(8L));
                y[1L] = uint8(m.signatureAlgorithm);
                y = y[2L..];
            }
            y[0L] = uint8(siglength >> (int)(8L));
            y[1L] = uint8(siglength);
            copy(y[2L..], m.signature);

            m.raw = x;

            return;
        }

        private static bool unmarshal(this ref certificateVerifyMsg m, slice<byte> data)
        {
            m.raw = data;

            if (len(data) < 6L)
            {
                return false;
            }
            var length = uint32(data[1L]) << (int)(16L) | uint32(data[2L]) << (int)(8L) | uint32(data[3L]);
            if (uint32(len(data)) - 4L != length)
            {
                return false;
            }
            data = data[4L..];
            if (m.hasSignatureAndHash)
            {
                m.signatureAlgorithm = SignatureScheme(data[0L]) << (int)(8L) | SignatureScheme(data[1L]);
                data = data[2L..];
            }
            if (len(data) < 2L)
            {
                return false;
            }
            var siglength = int(data[0L]) << (int)(8L) + int(data[1L]);
            data = data[2L..];
            if (len(data) != siglength)
            {
                return false;
            }
            m.signature = data;

            return true;
        }

        private partial struct newSessionTicketMsg
        {
            public slice<byte> raw;
            public slice<byte> ticket;
        }

        private static bool equal(this ref newSessionTicketMsg m, object i)
        {
            ref newSessionTicketMsg (m1, ok) = i._<ref newSessionTicketMsg>();
            if (!ok)
            {
                return false;
            }
            return bytes.Equal(m.raw, m1.raw) && bytes.Equal(m.ticket, m1.ticket);
        }

        private static slice<byte> marshal(this ref newSessionTicketMsg m)
        {
            if (m.raw != null)
            {
                return m.raw;
            } 

            // See http://tools.ietf.org/html/rfc5077#section-3.3
            var ticketLen = len(m.ticket);
            long length = 2L + 4L + ticketLen;
            x = make_slice<byte>(4L + length);
            x[0L] = typeNewSessionTicket;
            x[1L] = uint8(length >> (int)(16L));
            x[2L] = uint8(length >> (int)(8L));
            x[3L] = uint8(length);
            x[8L] = uint8(ticketLen >> (int)(8L));
            x[9L] = uint8(ticketLen);
            copy(x[10L..], m.ticket);

            m.raw = x;

            return;
        }

        private static bool unmarshal(this ref newSessionTicketMsg m, slice<byte> data)
        {
            m.raw = data;

            if (len(data) < 10L)
            {
                return false;
            }
            var length = uint32(data[1L]) << (int)(16L) | uint32(data[2L]) << (int)(8L) | uint32(data[3L]);
            if (uint32(len(data)) - 4L != length)
            {
                return false;
            }
            var ticketLen = int(data[8L]) << (int)(8L) + int(data[9L]);
            if (len(data) - 10L != ticketLen)
            {
                return false;
            }
            m.ticket = data[10L..];

            return true;
        }

        private partial struct helloRequestMsg
        {
        }

        private static slice<byte> marshal(this ref helloRequestMsg _p0)
        {
            return new slice<byte>(new byte[] { typeHelloRequest, 0, 0, 0 });
        }

        private static bool unmarshal(this ref helloRequestMsg _p0, slice<byte> data)
        {
            return len(data) == 4L;
        }

        private static bool eqUint16s(slice<ushort> x, slice<ushort> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, v) in x)
            {
                if (y[i] != v)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool eqCurveIDs(slice<CurveID> x, slice<CurveID> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, v) in x)
            {
                if (y[i] != v)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool eqStrings(slice<@string> x, slice<@string> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, v) in x)
            {
                if (y[i] != v)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool eqByteSlices(slice<slice<byte>> x, slice<slice<byte>> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, v) in x)
            {
                if (!bytes.Equal(v, y[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool eqSignatureAlgorithms(slice<SignatureScheme> x, slice<SignatureScheme> y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            foreach (var (i, v) in x)
            {
                if (v != y[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}}
