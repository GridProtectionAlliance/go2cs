// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 13 05:35:55 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\handshake_messages.go
namespace go.crypto;

using fmt = fmt_package;
using strings = strings_package;

using cryptobyte = golang.org.x.crypto.cryptobyte_package;


// The marshalingFunction type is an adapter to allow the use of ordinary
// functions as cryptobyte.MarshalingValue.

using System;
public static partial class tls_package {

public delegate  error marshalingFunction(ptr<cryptobyte.Builder>);

private static error Marshal(this marshalingFunction f, ptr<cryptobyte.Builder> _addr_b) {
    ref cryptobyte.Builder b = ref _addr_b.val;

    return error.As(f(b))!;
}

// addBytesWithLength appends a sequence of bytes to the cryptobyte.Builder. If
// the length of the sequence is not the value specified, it produces an error.
private static void addBytesWithLength(ptr<cryptobyte.Builder> _addr_b, slice<byte> v, nint n) {
    ref cryptobyte.Builder b = ref _addr_b.val;

    b.AddValue(marshalingFunction(b => {
        if (len(v) != n) {
            return fmt.Errorf("invalid value length: expected %d, got %d", n, len(v));
        }
        b.AddBytes(v);
        return null;
    }));
}

// addUint64 appends a big-endian, 64-bit value to the cryptobyte.Builder.
private static void addUint64(ptr<cryptobyte.Builder> _addr_b, ulong v) {
    ref cryptobyte.Builder b = ref _addr_b.val;

    b.AddUint32(uint32(v >> 32));
    b.AddUint32(uint32(v));
}

// readUint64 decodes a big-endian, 64-bit value into out and advances over it.
// It reports whether the read was successful.
private static bool readUint64(ptr<cryptobyte.String> _addr_s, ptr<ulong> _addr_@out) {
    ref cryptobyte.String s = ref _addr_s.val;
    ref ulong @out = ref _addr_@out.val;

    ref uint hi = ref heap(out ptr<uint> _addr_hi);    ref uint lo = ref heap(out ptr<uint> _addr_lo);

    if (!s.ReadUint32(_addr_hi) || !s.ReadUint32(_addr_lo)) {
        return false;
    }
    out.val = uint64(hi) << 32 | uint64(lo);
    return true;
}

// readUint8LengthPrefixed acts like s.ReadUint8LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
private static bool readUint8LengthPrefixed(ptr<cryptobyte.String> _addr_s, ptr<slice<byte>> _addr_@out) {
    ref cryptobyte.String s = ref _addr_s.val;
    ref slice<byte> @out = ref _addr_@out.val;

    return s.ReadUint8LengthPrefixed((cryptobyte.String.val)(out));
}

// readUint16LengthPrefixed acts like s.ReadUint16LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
private static bool readUint16LengthPrefixed(ptr<cryptobyte.String> _addr_s, ptr<slice<byte>> _addr_@out) {
    ref cryptobyte.String s = ref _addr_s.val;
    ref slice<byte> @out = ref _addr_@out.val;

    return s.ReadUint16LengthPrefixed((cryptobyte.String.val)(out));
}

// readUint24LengthPrefixed acts like s.ReadUint24LengthPrefixed, but targets a
// []byte instead of a cryptobyte.String.
private static bool readUint24LengthPrefixed(ptr<cryptobyte.String> _addr_s, ptr<slice<byte>> _addr_@out) {
    ref cryptobyte.String s = ref _addr_s.val;
    ref slice<byte> @out = ref _addr_@out.val;

    return s.ReadUint24LengthPrefixed((cryptobyte.String.val)(out));
}

private partial struct clientHelloMsg {
    public slice<byte> raw;
    public ushort vers;
    public slice<byte> random;
    public slice<byte> sessionId;
    public slice<ushort> cipherSuites;
    public slice<byte> compressionMethods;
    public @string serverName;
    public bool ocspStapling;
    public slice<CurveID> supportedCurves;
    public slice<byte> supportedPoints;
    public bool ticketSupported;
    public slice<byte> sessionTicket;
    public slice<SignatureScheme> supportedSignatureAlgorithms;
    public slice<SignatureScheme> supportedSignatureAlgorithmsCert;
    public bool secureRenegotiationSupported;
    public slice<byte> secureRenegotiation;
    public slice<@string> alpnProtocols;
    public bool scts;
    public slice<ushort> supportedVersions;
    public slice<byte> cookie;
    public slice<keyShare> keyShares;
    public bool earlyData;
    public slice<byte> pskModes;
    public slice<pskIdentity> pskIdentities;
    public slice<slice<byte>> pskBinders;
}

private static slice<byte> marshal(this ptr<clientHelloMsg> _addr_m) {
    ref clientHelloMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeClientHello);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint16(m.vers);
        addBytesWithLength(_addr_b, m.random, 32);
        b.AddUint8LengthPrefixed(b => {
            b.AddBytes(m.sessionId);
        });
        b.AddUint16LengthPrefixed(b => {
            foreach (var (_, suite) in m.cipherSuites) {
                b.AddUint16(suite);
            }
        });
        b.AddUint8LengthPrefixed(b => {
            b.AddBytes(m.compressionMethods);
        }); 

        // If extensions aren't present, omit them.
        bool extensionsPresent = default;
        var bWithoutExtensions = b.val;

        b.AddUint16LengthPrefixed(b => {
            if (len(m.serverName) > 0) { 
                // RFC 6066, Section 3
                b.AddUint16(extensionServerName);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        b.AddUint8(0); // name_type = host_name
                        b.AddUint16LengthPrefixed(b => {
                            b.AddBytes((slice<byte>)m.serverName);
                        });
                    });
                });
            }
            if (m.ocspStapling) { 
                // RFC 4366, Section 3.6
                b.AddUint16(extensionStatusRequest);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8(1); // status_type = ocsp
                    b.AddUint16(0); // empty responder_id_list
                    b.AddUint16(0); // empty request_extensions
                });
            }
            if (len(m.supportedCurves) > 0) { 
                // RFC 4492, sections 5.1.1 and RFC 8446, Section 4.2.7
                b.AddUint16(extensionSupportedCurves);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, curve) in m.supportedCurves) {
                            b.AddUint16(uint16(curve));
                        }
                    });
                });
            }
            if (len(m.supportedPoints) > 0) { 
                // RFC 4492, Section 5.1.2
                b.AddUint16(extensionSupportedPoints);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        b.AddBytes(m.supportedPoints);
                    });
                });
            }
            if (m.ticketSupported) { 
                // RFC 5077, Section 3.2
                b.AddUint16(extensionSessionTicket);
                b.AddUint16LengthPrefixed(b => {
                    b.AddBytes(m.sessionTicket);
                });
            }
            if (len(m.supportedSignatureAlgorithms) > 0) { 
                // RFC 5246, Section 7.4.1.4.1
                b.AddUint16(extensionSignatureAlgorithms);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        {
                            var sigAlgo__prev1 = sigAlgo;

                            foreach (var (_, __sigAlgo) in m.supportedSignatureAlgorithms) {
                                sigAlgo = __sigAlgo;
                                b.AddUint16(uint16(sigAlgo));
                            }

                            sigAlgo = sigAlgo__prev1;
                        }
                    });
                });
            }
            if (len(m.supportedSignatureAlgorithmsCert) > 0) { 
                // RFC 8446, Section 4.2.3
                b.AddUint16(extensionSignatureAlgorithmsCert);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        {
                            var sigAlgo__prev1 = sigAlgo;

                            foreach (var (_, __sigAlgo) in m.supportedSignatureAlgorithmsCert) {
                                sigAlgo = __sigAlgo;
                                b.AddUint16(uint16(sigAlgo));
                            }

                            sigAlgo = sigAlgo__prev1;
                        }
                    });
                });
            }
            if (m.secureRenegotiationSupported) { 
                // RFC 5746, Section 3.2
                b.AddUint16(extensionRenegotiationInfo);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        b.AddBytes(m.secureRenegotiation);
                    });
                });
            }
            if (len(m.alpnProtocols) > 0) { 
                // RFC 7301, Section 3.1
                b.AddUint16(extensionALPN);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, proto) in m.alpnProtocols) {
                            b.AddUint8LengthPrefixed(b => {
                                b.AddBytes((slice<byte>)proto);
                            });
                        }
                    });
                });
            }
            if (m.scts) { 
                // RFC 6962, Section 3.3.1
                b.AddUint16(extensionSCT);
                b.AddUint16(0); // empty extension_data
            }
            if (len(m.supportedVersions) > 0) { 
                // RFC 8446, Section 4.2.1
                b.AddUint16(extensionSupportedVersions);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        foreach (var (_, vers) in m.supportedVersions) {
                            b.AddUint16(vers);
                        }
                    });
                });
            }
            if (len(m.cookie) > 0) { 
                // RFC 8446, Section 4.2.2
                b.AddUint16(extensionCookie);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        b.AddBytes(m.cookie);
                    });
                });
            }
            if (len(m.keyShares) > 0) { 
                // RFC 8446, Section 4.2.8
                b.AddUint16(extensionKeyShare);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, ks) in m.keyShares) {
                            b.AddUint16(uint16(ks.group));
                            b.AddUint16LengthPrefixed(b => {
                                b.AddBytes(ks.data);
                            });
                        }
                    });
                });
            }
            if (m.earlyData) { 
                // RFC 8446, Section 4.2.10
                b.AddUint16(extensionEarlyData);
                b.AddUint16(0); // empty extension_data
            }
            if (len(m.pskModes) > 0) { 
                // RFC 8446, Section 4.2.9
                b.AddUint16(extensionPSKModes);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        b.AddBytes(m.pskModes);
                    });
                });
            }
            if (len(m.pskIdentities) > 0) { // pre_shared_key must be the last extension
                // RFC 8446, Section 4.2.11
                b.AddUint16(extensionPreSharedKey);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, psk) in m.pskIdentities) {
                            b.AddUint16LengthPrefixed(b => {
                                b.AddBytes(psk.label);
                            });
                            b.AddUint32(psk.obfuscatedTicketAge);
                        }
                    });
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, binder) in m.pskBinders) {
                            b.AddUint8LengthPrefixed(b => {
                                b.AddBytes(binder);
                            });
                        }
                    });
                });
            }
            extensionsPresent = len(b.BytesOrPanic()) > 2;
        });

        if (!extensionsPresent) {
            b.val = bWithoutExtensions;
        }
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

// marshalWithoutBinders returns the ClientHello through the
// PreSharedKeyExtension.identities field, according to RFC 8446, Section
// 4.2.11.2. Note that m.pskBinders must be set to slices of the correct length.
private static slice<byte> marshalWithoutBinders(this ptr<clientHelloMsg> _addr_m) {
    ref clientHelloMsg m = ref _addr_m.val;

    nint bindersLen = 2; // uint16 length prefix
    foreach (var (_, binder) in m.pskBinders) {
        bindersLen += 1; // uint8 length prefix
        bindersLen += len(binder);
    }    var fullMessage = m.marshal();
    return fullMessage[..(int)len(fullMessage) - bindersLen];
}

// updateBinders updates the m.pskBinders field, if necessary updating the
// cached marshaled representation. The supplied binders must have the same
// length as the current m.pskBinders.
private static void updateBinders(this ptr<clientHelloMsg> _addr_m, slice<slice<byte>> pskBinders) => func((_, panic, _) => {
    ref clientHelloMsg m = ref _addr_m.val;

    if (len(pskBinders) != len(m.pskBinders)) {
        panic("tls: internal error: pskBinders length mismatch");
    }
    foreach (var (i) in m.pskBinders) {
        if (len(pskBinders[i]) != len(m.pskBinders[i])) {
            panic("tls: internal error: pskBinders length mismatch");
        }
    }    m.pskBinders = pskBinders;
    if (m.raw != null) {
        var lenWithoutBinders = len(m.marshalWithoutBinders()); 
        // TODO(filippo): replace with NewFixedBuilder once CL 148882 is imported.
        var b = cryptobyte.NewBuilder(m.raw[..(int)lenWithoutBinders]);
        b.AddUint16LengthPrefixed(b => {
            foreach (var (_, binder) in m.pskBinders) {
                b.AddUint8LengthPrefixed(b => {
                    b.AddBytes(binder);
                });
            }
        });
        if (len(b.BytesOrPanic()) != len(m.raw)) {
            panic("tls: internal error: failed to update binders");
        }
    }
});

private static bool unmarshal(this ptr<clientHelloMsg> _addr_m, slice<byte> data) {
    ref clientHelloMsg m = ref _addr_m.val;

    m.val = new clientHelloMsg(raw:data);
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    if (!s.Skip(4) || !s.ReadUint16(_addr_m.vers) || !s.ReadBytes(_addr_m.random, 32) || !readUint8LengthPrefixed(_addr_s, _addr_m.sessionId)) {
        return false;
    }
    ref cryptobyte.String cipherSuites = ref heap(out ptr<cryptobyte.String> _addr_cipherSuites);
    if (!s.ReadUint16LengthPrefixed(_addr_cipherSuites)) {
        return false;
    }
    m.cipherSuites = new slice<ushort>(new ushort[] {  });
    m.secureRenegotiationSupported = false;
    while (!cipherSuites.Empty()) {
        ref ushort suite = ref heap(out ptr<ushort> _addr_suite);
        if (!cipherSuites.ReadUint16(_addr_suite)) {
            return false;
        }
        if (suite == scsvRenegotiation) {
            m.secureRenegotiationSupported = true;
        }
        m.cipherSuites = append(m.cipherSuites, suite);
    }

    if (!readUint8LengthPrefixed(_addr_s, _addr_m.compressionMethods)) {
        return false;
    }
    if (s.Empty()) { 
        // ClientHello is optionally followed by extension data
        return true;
    }
    ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
    if (!s.ReadUint16LengthPrefixed(_addr_extensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
        ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
        if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
            return false;
        }

        if (extension == extensionServerName) 
            // RFC 6066, Section 3
            ref cryptobyte.String nameList = ref heap(out ptr<cryptobyte.String> _addr_nameList);
            if (!extData.ReadUint16LengthPrefixed(_addr_nameList) || nameList.Empty()) {
                return false;
            }
            while (!nameList.Empty()) {
                ref byte nameType = ref heap(out ptr<byte> _addr_nameType);
                ref cryptobyte.String serverName = ref heap(out ptr<cryptobyte.String> _addr_serverName);
                if (!nameList.ReadUint8(_addr_nameType) || !nameList.ReadUint16LengthPrefixed(_addr_serverName) || serverName.Empty()) {
                    return false;
                }
                if (nameType != 0) {
                    continue;
                }
                if (len(m.serverName) != 0) { 
                    // Multiple names of the same name_type are prohibited.
                    return false;
                }
                m.serverName = string(serverName); 
                // An SNI value may not include a trailing dot.
                if (strings.HasSuffix(m.serverName, ".")) {
                    return false;
                }
            }
        else if (extension == extensionStatusRequest) 
            // RFC 4366, Section 3.6
            ref byte statusType = ref heap(out ptr<byte> _addr_statusType);
            ref cryptobyte.String ignored = ref heap(out ptr<cryptobyte.String> _addr_ignored);
            if (!extData.ReadUint8(_addr_statusType) || !extData.ReadUint16LengthPrefixed(_addr_ignored) || !extData.ReadUint16LengthPrefixed(_addr_ignored)) {
                return false;
            }
            m.ocspStapling = statusType == statusTypeOCSP;
        else if (extension == extensionSupportedCurves) 
            // RFC 4492, sections 5.1.1 and RFC 8446, Section 4.2.7
            ref cryptobyte.String curves = ref heap(out ptr<cryptobyte.String> _addr_curves);
            if (!extData.ReadUint16LengthPrefixed(_addr_curves) || curves.Empty()) {
                return false;
            }
            while (!curves.Empty()) {
                ref ushort curve = ref heap(out ptr<ushort> _addr_curve);
                if (!curves.ReadUint16(_addr_curve)) {
                    return false;
                }
                m.supportedCurves = append(m.supportedCurves, CurveID(curve));
            }
        else if (extension == extensionSupportedPoints) 
            // RFC 4492, Section 5.1.2
            if (!readUint8LengthPrefixed(_addr_extData, _addr_m.supportedPoints) || len(m.supportedPoints) == 0) {
                return false;
            }
        else if (extension == extensionSessionTicket) 
            // RFC 5077, Section 3.2
            m.ticketSupported = true;
            extData.ReadBytes(_addr_m.sessionTicket, len(extData));
        else if (extension == extensionSignatureAlgorithms) 
            // RFC 5246, Section 7.4.1.4.1
            ref cryptobyte.String sigAndAlgs = ref heap(out ptr<cryptobyte.String> _addr_sigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(_addr_sigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref ushort sigAndAlg = ref heap(out ptr<ushort> _addr_sigAndAlg);
                if (!sigAndAlgs.ReadUint16(_addr_sigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithms = append(m.supportedSignatureAlgorithms, SignatureScheme(sigAndAlg));
            }
        else if (extension == extensionSignatureAlgorithmsCert) 
            // RFC 8446, Section 4.2.3
            sigAndAlgs = default;
            if (!extData.ReadUint16LengthPrefixed(_addr_sigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                sigAndAlg = default;
                if (!sigAndAlgs.ReadUint16(_addr_sigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithmsCert = append(m.supportedSignatureAlgorithmsCert, SignatureScheme(sigAndAlg));
            }
        else if (extension == extensionRenegotiationInfo) 
            // RFC 5746, Section 3.2
            if (!readUint8LengthPrefixed(_addr_extData, _addr_m.secureRenegotiation)) {
                return false;
            }
            m.secureRenegotiationSupported = true;
        else if (extension == extensionALPN) 
            // RFC 7301, Section 3.1
            ref cryptobyte.String protoList = ref heap(out ptr<cryptobyte.String> _addr_protoList);
            if (!extData.ReadUint16LengthPrefixed(_addr_protoList) || protoList.Empty()) {
                return false;
            }
            while (!protoList.Empty()) {
                ref cryptobyte.String proto = ref heap(out ptr<cryptobyte.String> _addr_proto);
                if (!protoList.ReadUint8LengthPrefixed(_addr_proto) || proto.Empty()) {
                    return false;
                }
                m.alpnProtocols = append(m.alpnProtocols, string(proto));
            }
        else if (extension == extensionSCT) 
            // RFC 6962, Section 3.3.1
            m.scts = true;
        else if (extension == extensionSupportedVersions) 
            // RFC 8446, Section 4.2.1
            ref cryptobyte.String versList = ref heap(out ptr<cryptobyte.String> _addr_versList);
            if (!extData.ReadUint8LengthPrefixed(_addr_versList) || versList.Empty()) {
                return false;
            }
            while (!versList.Empty()) {
                ref ushort vers = ref heap(out ptr<ushort> _addr_vers);
                if (!versList.ReadUint16(_addr_vers)) {
                    return false;
                }
                m.supportedVersions = append(m.supportedVersions, vers);
            }
        else if (extension == extensionCookie) 
            // RFC 8446, Section 4.2.2
            if (!readUint16LengthPrefixed(_addr_extData, _addr_m.cookie) || len(m.cookie) == 0) {
                return false;
            }
        else if (extension == extensionKeyShare) 
            // RFC 8446, Section 4.2.8
            ref cryptobyte.String clientShares = ref heap(out ptr<cryptobyte.String> _addr_clientShares);
            if (!extData.ReadUint16LengthPrefixed(_addr_clientShares)) {
                return false;
            }
            while (!clientShares.Empty()) {
                keyShare ks = default;
                if (!clientShares.ReadUint16((uint16.val)(_addr_ks.group)) || !readUint16LengthPrefixed(_addr_clientShares, _addr_ks.data) || len(ks.data) == 0) {
                    return false;
                }
                m.keyShares = append(m.keyShares, ks);
            }
        else if (extension == extensionEarlyData) 
            // RFC 8446, Section 4.2.10
            m.earlyData = true;
        else if (extension == extensionPSKModes) 
            // RFC 8446, Section 4.2.9
            if (!readUint8LengthPrefixed(_addr_extData, _addr_m.pskModes)) {
                return false;
            }
        else if (extension == extensionPreSharedKey) 
            // RFC 8446, Section 4.2.11
            if (!extensions.Empty()) {
                return false; // pre_shared_key must be the last extension
            }
            ref cryptobyte.String identities = ref heap(out ptr<cryptobyte.String> _addr_identities);
            if (!extData.ReadUint16LengthPrefixed(_addr_identities) || identities.Empty()) {
                return false;
            }
            while (!identities.Empty()) {
                pskIdentity psk = default;
                if (!readUint16LengthPrefixed(_addr_identities, _addr_psk.label) || !identities.ReadUint32(_addr_psk.obfuscatedTicketAge) || len(psk.label) == 0) {
                    return false;
                }
                m.pskIdentities = append(m.pskIdentities, psk);
            }

            ref cryptobyte.String binders = ref heap(out ptr<cryptobyte.String> _addr_binders);
            if (!extData.ReadUint16LengthPrefixed(_addr_binders) || binders.Empty()) {
                return false;
            }
            while (!binders.Empty()) {
                ref slice<byte> binder = ref heap(out ptr<slice<byte>> _addr_binder);
                if (!readUint8LengthPrefixed(_addr_binders, _addr_binder) || len(binder) == 0) {
                    return false;
                }
                m.pskBinders = append(m.pskBinders, binder);
            }
        else 
            // Ignore unknown extensions.
            continue;
                if (!extData.Empty()) {
            return false;
        }
    }

    return true;
}

private partial struct serverHelloMsg {
    public slice<byte> raw;
    public ushort vers;
    public slice<byte> random;
    public slice<byte> sessionId;
    public ushort cipherSuite;
    public byte compressionMethod;
    public bool ocspStapling;
    public bool ticketSupported;
    public bool secureRenegotiationSupported;
    public slice<byte> secureRenegotiation;
    public @string alpnProtocol;
    public slice<slice<byte>> scts;
    public ushort supportedVersion;
    public keyShare serverShare;
    public bool selectedIdentityPresent;
    public ushort selectedIdentity;
    public slice<byte> supportedPoints; // HelloRetryRequest extensions
    public slice<byte> cookie;
    public CurveID selectedGroup;
}

private static slice<byte> marshal(this ptr<serverHelloMsg> _addr_m) {
    ref serverHelloMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeServerHello);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint16(m.vers);
        addBytesWithLength(_addr_b, m.random, 32);
        b.AddUint8LengthPrefixed(b => {
            b.AddBytes(m.sessionId);
        });
        b.AddUint16(m.cipherSuite);
        b.AddUint8(m.compressionMethod); 

        // If extensions aren't present, omit them.
        bool extensionsPresent = default;
        var bWithoutExtensions = b.val;

        b.AddUint16LengthPrefixed(b => {
            if (m.ocspStapling) {
                b.AddUint16(extensionStatusRequest);
                b.AddUint16(0); // empty extension_data
            }
            if (m.ticketSupported) {
                b.AddUint16(extensionSessionTicket);
                b.AddUint16(0); // empty extension_data
            }
            if (m.secureRenegotiationSupported) {
                b.AddUint16(extensionRenegotiationInfo);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        b.AddBytes(m.secureRenegotiation);
                    });
                });
            }
            if (len(m.alpnProtocol) > 0) {
                b.AddUint16(extensionALPN);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        b.AddUint8LengthPrefixed(b => {
                            b.AddBytes((slice<byte>)m.alpnProtocol);
                        });
                    });
                });
            }
            if (len(m.scts) > 0) {
                b.AddUint16(extensionSCT);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, sct) in m.scts) {
                            b.AddUint16LengthPrefixed(b => {
                                b.AddBytes(sct);
                            });
                        }
                    });
                });
            }
            if (m.supportedVersion != 0) {
                b.AddUint16(extensionSupportedVersions);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16(m.supportedVersion);
                });
            }
            if (m.serverShare.group != 0) {
                b.AddUint16(extensionKeyShare);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16(uint16(m.serverShare.group));
                    b.AddUint16LengthPrefixed(b => {
                        b.AddBytes(m.serverShare.data);
                    });
                });
            }
            if (m.selectedIdentityPresent) {
                b.AddUint16(extensionPreSharedKey);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16(m.selectedIdentity);
                });
            }
            if (len(m.cookie) > 0) {
                b.AddUint16(extensionCookie);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        b.AddBytes(m.cookie);
                    });
                });
            }
            if (m.selectedGroup != 0) {
                b.AddUint16(extensionKeyShare);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16(uint16(m.selectedGroup));
                });
            }
            if (len(m.supportedPoints) > 0) {
                b.AddUint16(extensionSupportedPoints);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint8LengthPrefixed(b => {
                        b.AddBytes(m.supportedPoints);
                    });
                });
            }
            extensionsPresent = len(b.BytesOrPanic()) > 2;
        });

        if (!extensionsPresent) {
            b.val = bWithoutExtensions;
        }
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<serverHelloMsg> _addr_m, slice<byte> data) {
    ref serverHelloMsg m = ref _addr_m.val;

    m.val = new serverHelloMsg(raw:data);
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    if (!s.Skip(4) || !s.ReadUint16(_addr_m.vers) || !s.ReadBytes(_addr_m.random, 32) || !readUint8LengthPrefixed(_addr_s, _addr_m.sessionId) || !s.ReadUint16(_addr_m.cipherSuite) || !s.ReadUint8(_addr_m.compressionMethod)) {
        return false;
    }
    if (s.Empty()) { 
        // ServerHello is optionally followed by extension data
        return true;
    }
    ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
    if (!s.ReadUint16LengthPrefixed(_addr_extensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
        ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
        if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
            return false;
        }

        if (extension == extensionStatusRequest) 
            m.ocspStapling = true;
        else if (extension == extensionSessionTicket) 
            m.ticketSupported = true;
        else if (extension == extensionRenegotiationInfo) 
            if (!readUint8LengthPrefixed(_addr_extData, _addr_m.secureRenegotiation)) {
                return false;
            }
            m.secureRenegotiationSupported = true;
        else if (extension == extensionALPN) 
            ref cryptobyte.String protoList = ref heap(out ptr<cryptobyte.String> _addr_protoList);
            if (!extData.ReadUint16LengthPrefixed(_addr_protoList) || protoList.Empty()) {
                return false;
            }
            ref cryptobyte.String proto = ref heap(out ptr<cryptobyte.String> _addr_proto);
            if (!protoList.ReadUint8LengthPrefixed(_addr_proto) || proto.Empty() || !protoList.Empty()) {
                return false;
            }
            m.alpnProtocol = string(proto);
        else if (extension == extensionSCT) 
            ref cryptobyte.String sctList = ref heap(out ptr<cryptobyte.String> _addr_sctList);
            if (!extData.ReadUint16LengthPrefixed(_addr_sctList) || sctList.Empty()) {
                return false;
            }
            while (!sctList.Empty()) {
                ref slice<byte> sct = ref heap(out ptr<slice<byte>> _addr_sct);
                if (!readUint16LengthPrefixed(_addr_sctList, _addr_sct) || len(sct) == 0) {
                    return false;
                }
                m.scts = append(m.scts, sct);
            }
        else if (extension == extensionSupportedVersions) 
            if (!extData.ReadUint16(_addr_m.supportedVersion)) {
                return false;
            }
        else if (extension == extensionCookie) 
            if (!readUint16LengthPrefixed(_addr_extData, _addr_m.cookie) || len(m.cookie) == 0) {
                return false;
            }
        else if (extension == extensionKeyShare) 
            // This extension has different formats in SH and HRR, accept either
            // and let the handshake logic decide. See RFC 8446, Section 4.2.8.
            if (len(extData) == 2) {
                if (!extData.ReadUint16((uint16.val)(_addr_m.selectedGroup))) {
                    return false;
                }
            }
            else
 {
                if (!extData.ReadUint16((uint16.val)(_addr_m.serverShare.group)) || !readUint16LengthPrefixed(_addr_extData, _addr_m.serverShare.data)) {
                    return false;
                }
            }
        else if (extension == extensionPreSharedKey) 
            m.selectedIdentityPresent = true;
            if (!extData.ReadUint16(_addr_m.selectedIdentity)) {
                return false;
            }
        else if (extension == extensionSupportedPoints) 
            // RFC 4492, Section 5.1.2
            if (!readUint8LengthPrefixed(_addr_extData, _addr_m.supportedPoints) || len(m.supportedPoints) == 0) {
                return false;
            }
        else 
            // Ignore unknown extensions.
            continue;
                if (!extData.Empty()) {
            return false;
        }
    }

    return true;
}

private partial struct encryptedExtensionsMsg {
    public slice<byte> raw;
    public @string alpnProtocol;
}

private static slice<byte> marshal(this ptr<encryptedExtensionsMsg> _addr_m) {
    ref encryptedExtensionsMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeEncryptedExtensions);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint16LengthPrefixed(b => {
            if (len(m.alpnProtocol) > 0) {
                b.AddUint16(extensionALPN);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        b.AddUint8LengthPrefixed(b => {
                            b.AddBytes((slice<byte>)m.alpnProtocol);
                        });
                    });
                });
            }
        });
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<encryptedExtensionsMsg> _addr_m, slice<byte> data) {
    ref encryptedExtensionsMsg m = ref _addr_m.val;

    m.val = new encryptedExtensionsMsg(raw:data);
    var s = cryptobyte.String(data);

    ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
    if (!s.Skip(4) || !s.ReadUint16LengthPrefixed(_addr_extensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
        ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
        if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
            return false;
        }

        if (extension == extensionALPN) 
            ref cryptobyte.String protoList = ref heap(out ptr<cryptobyte.String> _addr_protoList);
            if (!extData.ReadUint16LengthPrefixed(_addr_protoList) || protoList.Empty()) {
                return false;
            }
            ref cryptobyte.String proto = ref heap(out ptr<cryptobyte.String> _addr_proto);
            if (!protoList.ReadUint8LengthPrefixed(_addr_proto) || proto.Empty() || !protoList.Empty()) {
                return false;
            }
            m.alpnProtocol = string(proto);
        else 
            // Ignore unknown extensions.
            continue;
                if (!extData.Empty()) {
            return false;
        }
    }

    return true;
}

private partial struct endOfEarlyDataMsg {
}

private static slice<byte> marshal(this ptr<endOfEarlyDataMsg> _addr_m) {
    ref endOfEarlyDataMsg m = ref _addr_m.val;

    var x = make_slice<byte>(4);
    x[0] = typeEndOfEarlyData;
    return x;
}

private static bool unmarshal(this ptr<endOfEarlyDataMsg> _addr_m, slice<byte> data) {
    ref endOfEarlyDataMsg m = ref _addr_m.val;

    return len(data) == 4;
}

private partial struct keyUpdateMsg {
    public slice<byte> raw;
    public bool updateRequested;
}

private static slice<byte> marshal(this ptr<keyUpdateMsg> _addr_m) {
    ref keyUpdateMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeKeyUpdate);
    b.AddUint24LengthPrefixed(b => {
        if (m.updateRequested) {
            b.AddUint8(1);
        }
        else
 {
            b.AddUint8(0);
        }
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<keyUpdateMsg> _addr_m, slice<byte> data) {
    ref keyUpdateMsg m = ref _addr_m.val;

    m.raw = data;
    var s = cryptobyte.String(data);

    ref byte updateRequested = ref heap(out ptr<byte> _addr_updateRequested);
    if (!s.Skip(4) || !s.ReadUint8(_addr_updateRequested) || !s.Empty()) {
        return false;
    }
    switch (updateRequested) {
        case 0: 
            m.updateRequested = false;
            break;
        case 1: 
            m.updateRequested = true;
            break;
        default: 
            return false;
            break;
    }
    return true;
}

private partial struct newSessionTicketMsgTLS13 {
    public slice<byte> raw;
    public uint lifetime;
    public uint ageAdd;
    public slice<byte> nonce;
    public slice<byte> label;
    public uint maxEarlyData;
}

private static slice<byte> marshal(this ptr<newSessionTicketMsgTLS13> _addr_m) {
    ref newSessionTicketMsgTLS13 m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeNewSessionTicket);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint32(m.lifetime);
        b.AddUint32(m.ageAdd);
        b.AddUint8LengthPrefixed(b => {
            b.AddBytes(m.nonce);
        });
        b.AddUint16LengthPrefixed(b => {
            b.AddBytes(m.label);
        });

        b.AddUint16LengthPrefixed(b => {
            if (m.maxEarlyData > 0) {
                b.AddUint16(extensionEarlyData);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint32(m.maxEarlyData);
                });
            }
        });
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<newSessionTicketMsgTLS13> _addr_m, slice<byte> data) {
    ref newSessionTicketMsgTLS13 m = ref _addr_m.val;

    m.val = new newSessionTicketMsgTLS13(raw:data);
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
    if (!s.Skip(4) || !s.ReadUint32(_addr_m.lifetime) || !s.ReadUint32(_addr_m.ageAdd) || !readUint8LengthPrefixed(_addr_s, _addr_m.nonce) || !readUint16LengthPrefixed(_addr_s, _addr_m.label) || !s.ReadUint16LengthPrefixed(_addr_extensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
        ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
        if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
            return false;
        }

        if (extension == extensionEarlyData) 
            if (!extData.ReadUint32(_addr_m.maxEarlyData)) {
                return false;
            }
        else 
            // Ignore unknown extensions.
            continue;
                if (!extData.Empty()) {
            return false;
        }
    }

    return true;
}

private partial struct certificateRequestMsgTLS13 {
    public slice<byte> raw;
    public bool ocspStapling;
    public bool scts;
    public slice<SignatureScheme> supportedSignatureAlgorithms;
    public slice<SignatureScheme> supportedSignatureAlgorithmsCert;
    public slice<slice<byte>> certificateAuthorities;
}

private static slice<byte> marshal(this ptr<certificateRequestMsgTLS13> _addr_m) {
    ref certificateRequestMsgTLS13 m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeCertificateRequest);
    b.AddUint24LengthPrefixed(b => { 
        // certificate_request_context (SHALL be zero length unless used for
        // post-handshake authentication)
        b.AddUint8(0);

        b.AddUint16LengthPrefixed(b => {
            if (m.ocspStapling) {
                b.AddUint16(extensionStatusRequest);
                b.AddUint16(0); // empty extension_data
            }
            if (m.scts) { 
                // RFC 8446, Section 4.4.2.1 makes no mention of
                // signed_certificate_timestamp in CertificateRequest, but
                // "Extensions in the Certificate message from the client MUST
                // correspond to extensions in the CertificateRequest message
                // from the server." and it appears in the table in Section 4.2.
                b.AddUint16(extensionSCT);
                b.AddUint16(0); // empty extension_data
            }
            if (len(m.supportedSignatureAlgorithms) > 0) {
                b.AddUint16(extensionSignatureAlgorithms);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        {
                            var sigAlgo__prev1 = sigAlgo;

                            foreach (var (_, __sigAlgo) in m.supportedSignatureAlgorithms) {
                                sigAlgo = __sigAlgo;
                                b.AddUint16(uint16(sigAlgo));
                            }

                            sigAlgo = sigAlgo__prev1;
                        }
                    });
                });
            }
            if (len(m.supportedSignatureAlgorithmsCert) > 0) {
                b.AddUint16(extensionSignatureAlgorithmsCert);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        {
                            var sigAlgo__prev1 = sigAlgo;

                            foreach (var (_, __sigAlgo) in m.supportedSignatureAlgorithmsCert) {
                                sigAlgo = __sigAlgo;
                                b.AddUint16(uint16(sigAlgo));
                            }

                            sigAlgo = sigAlgo__prev1;
                        }
                    });
                });
            }
            if (len(m.certificateAuthorities) > 0) {
                b.AddUint16(extensionCertificateAuthorities);
                b.AddUint16LengthPrefixed(b => {
                    b.AddUint16LengthPrefixed(b => {
                        foreach (var (_, ca) in m.certificateAuthorities) {
                            b.AddUint16LengthPrefixed(b => {
                                b.AddBytes(ca);
                            });
                        }
                    });
                });
            }
        });
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<certificateRequestMsgTLS13> _addr_m, slice<byte> data) {
    ref certificateRequestMsgTLS13 m = ref _addr_m.val;

    m.val = new certificateRequestMsgTLS13(raw:data);
    var s = cryptobyte.String(data);

    ref cryptobyte.String context = ref heap(out ptr<cryptobyte.String> _addr_context);    ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);

    if (!s.Skip(4) || !s.ReadUint8LengthPrefixed(_addr_context) || !context.Empty() || !s.ReadUint16LengthPrefixed(_addr_extensions) || !s.Empty()) {
        return false;
    }
    while (!extensions.Empty()) {
        ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
        ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
        if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
            return false;
        }

        if (extension == extensionStatusRequest) 
            m.ocspStapling = true;
        else if (extension == extensionSCT) 
            m.scts = true;
        else if (extension == extensionSignatureAlgorithms) 
            ref cryptobyte.String sigAndAlgs = ref heap(out ptr<cryptobyte.String> _addr_sigAndAlgs);
            if (!extData.ReadUint16LengthPrefixed(_addr_sigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                ref ushort sigAndAlg = ref heap(out ptr<ushort> _addr_sigAndAlg);
                if (!sigAndAlgs.ReadUint16(_addr_sigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithms = append(m.supportedSignatureAlgorithms, SignatureScheme(sigAndAlg));
            }
        else if (extension == extensionSignatureAlgorithmsCert) 
            sigAndAlgs = default;
            if (!extData.ReadUint16LengthPrefixed(_addr_sigAndAlgs) || sigAndAlgs.Empty()) {
                return false;
            }
            while (!sigAndAlgs.Empty()) {
                sigAndAlg = default;
                if (!sigAndAlgs.ReadUint16(_addr_sigAndAlg)) {
                    return false;
                }
                m.supportedSignatureAlgorithmsCert = append(m.supportedSignatureAlgorithmsCert, SignatureScheme(sigAndAlg));
            }
        else if (extension == extensionCertificateAuthorities) 
            ref cryptobyte.String auths = ref heap(out ptr<cryptobyte.String> _addr_auths);
            if (!extData.ReadUint16LengthPrefixed(_addr_auths) || auths.Empty()) {
                return false;
            }
            while (!auths.Empty()) {
                ref slice<byte> ca = ref heap(out ptr<slice<byte>> _addr_ca);
                if (!readUint16LengthPrefixed(_addr_auths, _addr_ca) || len(ca) == 0) {
                    return false;
                }
                m.certificateAuthorities = append(m.certificateAuthorities, ca);
            }
        else 
            // Ignore unknown extensions.
            continue;
                if (!extData.Empty()) {
            return false;
        }
    }

    return true;
}

private partial struct certificateMsg {
    public slice<byte> raw;
    public slice<slice<byte>> certificates;
}

private static slice<byte> marshal(this ptr<certificateMsg> _addr_m) {
    slice<byte> x = default;
    ref certificateMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    nint i = default;
    {
        var slice__prev1 = slice;

        foreach (var (_, __slice) in m.certificates) {
            slice = __slice;
            i += len(slice);
        }
        slice = slice__prev1;
    }

    nint length = 3 + 3 * len(m.certificates) + i;
    x = make_slice<byte>(4 + length);
    x[0] = typeCertificate;
    x[1] = uint8(length >> 16);
    x[2] = uint8(length >> 8);
    x[3] = uint8(length);

    var certificateOctets = length - 3;
    x[4] = uint8(certificateOctets >> 16);
    x[5] = uint8(certificateOctets >> 8);
    x[6] = uint8(certificateOctets);

    var y = x[(int)7..];
    {
        var slice__prev1 = slice;

        foreach (var (_, __slice) in m.certificates) {
            slice = __slice;
            y[0] = uint8(len(slice) >> 16);
            y[1] = uint8(len(slice) >> 8);
            y[2] = uint8(len(slice));
            copy(y[(int)3..], slice);
            y = y[(int)3 + len(slice)..];
        }
        slice = slice__prev1;
    }

    m.raw = x;
    return ;
}

private static bool unmarshal(this ptr<certificateMsg> _addr_m, slice<byte> data) {
    ref certificateMsg m = ref _addr_m.val;

    if (len(data) < 7) {
        return false;
    }
    m.raw = data;
    var certsLen = uint32(data[4]) << 16 | uint32(data[5]) << 8 | uint32(data[6]);
    if (uint32(len(data)) != certsLen + 7) {
        return false;
    }
    nint numCerts = 0;
    var d = data[(int)7..];
    while (certsLen > 0) {
        if (len(d) < 4) {
            return false;
        }
        var certLen = uint32(d[0]) << 16 | uint32(d[1]) << 8 | uint32(d[2]);
        if (uint32(len(d)) < 3 + certLen) {
            return false;
        }
        d = d[(int)3 + certLen..];
        certsLen -= 3 + certLen;
        numCerts++;
    }

    m.certificates = make_slice<slice<byte>>(numCerts);
    d = data[(int)7..];
    for (nint i = 0; i < numCerts; i++) {
        certLen = uint32(d[0]) << 16 | uint32(d[1]) << 8 | uint32(d[2]);
        m.certificates[i] = d[(int)3..(int)3 + certLen];
        d = d[(int)3 + certLen..];
    }

    return true;
}

private partial struct certificateMsgTLS13 {
    public slice<byte> raw;
    public Certificate certificate;
    public bool ocspStapling;
    public bool scts;
}

private static slice<byte> marshal(this ptr<certificateMsgTLS13> _addr_m) {
    ref certificateMsgTLS13 m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeCertificate);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint8(0); // certificate_request_context

        var certificate = m.certificate;
        if (!m.ocspStapling) {
            certificate.OCSPStaple = null;
        }
        if (!m.scts) {
            certificate.SignedCertificateTimestamps = null;
        }
        marshalCertificate(_addr_b, certificate);
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static void marshalCertificate(ptr<cryptobyte.Builder> _addr_b, Certificate certificate) {
    ref cryptobyte.Builder b = ref _addr_b.val;

    b.AddUint24LengthPrefixed(b => {
        foreach (var (i, cert) in certificate.Certificate) {
            b.AddUint24LengthPrefixed(b => {
                b.AddBytes(cert);
            });
            b.AddUint16LengthPrefixed(b => {
                if (i > 0) { 
                    // This library only supports OCSP and SCT for leaf certificates.
                    return ;
                }
                if (certificate.OCSPStaple != null) {
                    b.AddUint16(extensionStatusRequest);
                    b.AddUint16LengthPrefixed(b => {
                        b.AddUint8(statusTypeOCSP);
                        b.AddUint24LengthPrefixed(b => {
                            b.AddBytes(certificate.OCSPStaple);
                        });
                    });
                }
                if (certificate.SignedCertificateTimestamps != null) {
                    b.AddUint16(extensionSCT);
                    b.AddUint16LengthPrefixed(b => {
                        b.AddUint16LengthPrefixed(b => {
                            foreach (var (_, sct) in certificate.SignedCertificateTimestamps) {
                                b.AddUint16LengthPrefixed(b => {
                                    b.AddBytes(sct);
                                });
                            }
                        });
                    });
                }
            });
        }
    });
}

private static bool unmarshal(this ptr<certificateMsgTLS13> _addr_m, slice<byte> data) {
    ref certificateMsgTLS13 m = ref _addr_m.val;

    m.val = new certificateMsgTLS13(raw:data);
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    ref cryptobyte.String context = ref heap(out ptr<cryptobyte.String> _addr_context);
    if (!s.Skip(4) || !s.ReadUint8LengthPrefixed(_addr_context) || !context.Empty() || !unmarshalCertificate(_addr_s, _addr_m.certificate) || !s.Empty()) {
        return false;
    }
    m.scts = m.certificate.SignedCertificateTimestamps != null;
    m.ocspStapling = m.certificate.OCSPStaple != null;

    return true;
}

private static bool unmarshalCertificate(ptr<cryptobyte.String> _addr_s, ptr<Certificate> _addr_certificate) {
    ref cryptobyte.String s = ref _addr_s.val;
    ref Certificate certificate = ref _addr_certificate.val;

    ref cryptobyte.String certList = ref heap(out ptr<cryptobyte.String> _addr_certList);
    if (!s.ReadUint24LengthPrefixed(_addr_certList)) {
        return false;
    }
    while (!certList.Empty()) {
        ref slice<byte> cert = ref heap(out ptr<slice<byte>> _addr_cert);
        ref cryptobyte.String extensions = ref heap(out ptr<cryptobyte.String> _addr_extensions);
        if (!readUint24LengthPrefixed(_addr_certList, _addr_cert) || !certList.ReadUint16LengthPrefixed(_addr_extensions)) {
            return false;
        }
        certificate.Certificate = append(certificate.Certificate, cert);
        while (!extensions.Empty()) {
            ref ushort extension = ref heap(out ptr<ushort> _addr_extension);
            ref cryptobyte.String extData = ref heap(out ptr<cryptobyte.String> _addr_extData);
            if (!extensions.ReadUint16(_addr_extension) || !extensions.ReadUint16LengthPrefixed(_addr_extData)) {
                return false;
            }
            if (len(certificate.Certificate) > 1) { 
                // This library only supports OCSP and SCT for leaf certificates.
                continue;
            }

            if (extension == extensionStatusRequest) 
                ref byte statusType = ref heap(out ptr<byte> _addr_statusType);
                if (!extData.ReadUint8(_addr_statusType) || statusType != statusTypeOCSP || !readUint24LengthPrefixed(_addr_extData, _addr_certificate.OCSPStaple) || len(certificate.OCSPStaple) == 0) {
                    return false;
                }
            else if (extension == extensionSCT) 
                ref cryptobyte.String sctList = ref heap(out ptr<cryptobyte.String> _addr_sctList);
                if (!extData.ReadUint16LengthPrefixed(_addr_sctList) || sctList.Empty()) {
                    return false;
                }
                while (!sctList.Empty()) {
                    ref slice<byte> sct = ref heap(out ptr<slice<byte>> _addr_sct);
                    if (!readUint16LengthPrefixed(_addr_sctList, _addr_sct) || len(sct) == 0) {
                        return false;
                    }
                    certificate.SignedCertificateTimestamps = append(certificate.SignedCertificateTimestamps, sct);
                }
            else 
                // Ignore unknown extensions.
                continue;
                        if (!extData.Empty()) {
                return false;
            }
        }
    }
    return true;
}

private partial struct serverKeyExchangeMsg {
    public slice<byte> raw;
    public slice<byte> key;
}

private static slice<byte> marshal(this ptr<serverKeyExchangeMsg> _addr_m) {
    ref serverKeyExchangeMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    var length = len(m.key);
    var x = make_slice<byte>(length + 4);
    x[0] = typeServerKeyExchange;
    x[1] = uint8(length >> 16);
    x[2] = uint8(length >> 8);
    x[3] = uint8(length);
    copy(x[(int)4..], m.key);

    m.raw = x;
    return x;
}

private static bool unmarshal(this ptr<serverKeyExchangeMsg> _addr_m, slice<byte> data) {
    ref serverKeyExchangeMsg m = ref _addr_m.val;

    m.raw = data;
    if (len(data) < 4) {
        return false;
    }
    m.key = data[(int)4..];
    return true;
}

private partial struct certificateStatusMsg {
    public slice<byte> raw;
    public slice<byte> response;
}

private static slice<byte> marshal(this ptr<certificateStatusMsg> _addr_m) {
    ref certificateStatusMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeCertificateStatus);
    b.AddUint24LengthPrefixed(b => {
        b.AddUint8(statusTypeOCSP);
        b.AddUint24LengthPrefixed(b => {
            b.AddBytes(m.response);
        });
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<certificateStatusMsg> _addr_m, slice<byte> data) {
    ref certificateStatusMsg m = ref _addr_m.val;

    m.raw = data;
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    ref byte statusType = ref heap(out ptr<byte> _addr_statusType);
    if (!s.Skip(4) || !s.ReadUint8(_addr_statusType) || statusType != statusTypeOCSP || !readUint24LengthPrefixed(_addr_s, _addr_m.response) || len(m.response) == 0 || !s.Empty()) {
        return false;
    }
    return true;
}

private partial struct serverHelloDoneMsg {
}

private static slice<byte> marshal(this ptr<serverHelloDoneMsg> _addr_m) {
    ref serverHelloDoneMsg m = ref _addr_m.val;

    var x = make_slice<byte>(4);
    x[0] = typeServerHelloDone;
    return x;
}

private static bool unmarshal(this ptr<serverHelloDoneMsg> _addr_m, slice<byte> data) {
    ref serverHelloDoneMsg m = ref _addr_m.val;

    return len(data) == 4;
}

private partial struct clientKeyExchangeMsg {
    public slice<byte> raw;
    public slice<byte> ciphertext;
}

private static slice<byte> marshal(this ptr<clientKeyExchangeMsg> _addr_m) {
    ref clientKeyExchangeMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    var length = len(m.ciphertext);
    var x = make_slice<byte>(length + 4);
    x[0] = typeClientKeyExchange;
    x[1] = uint8(length >> 16);
    x[2] = uint8(length >> 8);
    x[3] = uint8(length);
    copy(x[(int)4..], m.ciphertext);

    m.raw = x;
    return x;
}

private static bool unmarshal(this ptr<clientKeyExchangeMsg> _addr_m, slice<byte> data) {
    ref clientKeyExchangeMsg m = ref _addr_m.val;

    m.raw = data;
    if (len(data) < 4) {
        return false;
    }
    var l = int(data[1]) << 16 | int(data[2]) << 8 | int(data[3]);
    if (l != len(data) - 4) {
        return false;
    }
    m.ciphertext = data[(int)4..];
    return true;
}

private partial struct finishedMsg {
    public slice<byte> raw;
    public slice<byte> verifyData;
}

private static slice<byte> marshal(this ptr<finishedMsg> _addr_m) {
    ref finishedMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeFinished);
    b.AddUint24LengthPrefixed(b => {
        b.AddBytes(m.verifyData);
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<finishedMsg> _addr_m, slice<byte> data) {
    ref finishedMsg m = ref _addr_m.val;

    m.raw = data;
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);
    return s.Skip(1) && readUint24LengthPrefixed(_addr_s, _addr_m.verifyData) && s.Empty();
}

private partial struct certificateRequestMsg {
    public slice<byte> raw; // hasSignatureAlgorithm indicates whether this message includes a list of
// supported signature algorithms. This change was introduced with TLS 1.2.
    public bool hasSignatureAlgorithm;
    public slice<byte> certificateTypes;
    public slice<SignatureScheme> supportedSignatureAlgorithms;
    public slice<slice<byte>> certificateAuthorities;
}

private static slice<byte> marshal(this ptr<certificateRequestMsg> _addr_m) {
    slice<byte> x = default;
    ref certificateRequestMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    nint length = 1 + len(m.certificateTypes) + 2;
    nint casLength = 0;
    {
        var ca__prev1 = ca;

        foreach (var (_, __ca) in m.certificateAuthorities) {
            ca = __ca;
            casLength += 2 + len(ca);
        }
        ca = ca__prev1;
    }

    length += casLength;

    if (m.hasSignatureAlgorithm) {
        length += 2 + 2 * len(m.supportedSignatureAlgorithms);
    }
    x = make_slice<byte>(4 + length);
    x[0] = typeCertificateRequest;
    x[1] = uint8(length >> 16);
    x[2] = uint8(length >> 8);
    x[3] = uint8(length);

    x[4] = uint8(len(m.certificateTypes));

    copy(x[(int)5..], m.certificateTypes);
    var y = x[(int)5 + len(m.certificateTypes)..];

    if (m.hasSignatureAlgorithm) {
        var n = len(m.supportedSignatureAlgorithms) * 2;
        y[0] = uint8(n >> 8);
        y[1] = uint8(n);
        y = y[(int)2..];
        foreach (var (_, sigAlgo) in m.supportedSignatureAlgorithms) {
            y[0] = uint8(sigAlgo >> 8);
            y[1] = uint8(sigAlgo);
            y = y[(int)2..];
        }
    }
    y[0] = uint8(casLength >> 8);
    y[1] = uint8(casLength);
    y = y[(int)2..];
    {
        var ca__prev1 = ca;

        foreach (var (_, __ca) in m.certificateAuthorities) {
            ca = __ca;
            y[0] = uint8(len(ca) >> 8);
            y[1] = uint8(len(ca));
            y = y[(int)2..];
            copy(y, ca);
            y = y[(int)len(ca)..];
        }
        ca = ca__prev1;
    }

    m.raw = x;
    return ;
}

private static bool unmarshal(this ptr<certificateRequestMsg> _addr_m, slice<byte> data) {
    ref certificateRequestMsg m = ref _addr_m.val;

    m.raw = data;

    if (len(data) < 5) {
        return false;
    }
    var length = uint32(data[1]) << 16 | uint32(data[2]) << 8 | uint32(data[3]);
    if (uint32(len(data)) - 4 != length) {
        return false;
    }
    var numCertTypes = int(data[4]);
    data = data[(int)5..];
    if (numCertTypes == 0 || len(data) <= numCertTypes) {
        return false;
    }
    m.certificateTypes = make_slice<byte>(numCertTypes);
    if (copy(m.certificateTypes, data) != numCertTypes) {
        return false;
    }
    data = data[(int)numCertTypes..];

    if (m.hasSignatureAlgorithm) {
        if (len(data) < 2) {
            return false;
        }
        var sigAndHashLen = uint16(data[0]) << 8 | uint16(data[1]);
        data = data[(int)2..];
        if (sigAndHashLen & 1 != 0) {
            return false;
        }
        if (len(data) < int(sigAndHashLen)) {
            return false;
        }
        var numSigAlgos = sigAndHashLen / 2;
        m.supportedSignatureAlgorithms = make_slice<SignatureScheme>(numSigAlgos);
        foreach (var (i) in m.supportedSignatureAlgorithms) {
            m.supportedSignatureAlgorithms[i] = SignatureScheme(data[0]) << 8 | SignatureScheme(data[1]);
            data = data[(int)2..];
        }
    }
    if (len(data) < 2) {
        return false;
    }
    var casLength = uint16(data[0]) << 8 | uint16(data[1]);
    data = data[(int)2..];
    if (len(data) < int(casLength)) {
        return false;
    }
    var cas = make_slice<byte>(casLength);
    copy(cas, data);
    data = data[(int)casLength..];

    m.certificateAuthorities = null;
    while (len(cas) > 0) {
        if (len(cas) < 2) {
            return false;
        }
        var caLen = uint16(cas[0]) << 8 | uint16(cas[1]);
        cas = cas[(int)2..];

        if (len(cas) < int(caLen)) {
            return false;
        }
        m.certificateAuthorities = append(m.certificateAuthorities, cas[..(int)caLen]);
        cas = cas[(int)caLen..];
    }

    return len(data) == 0;
}

private partial struct certificateVerifyMsg {
    public slice<byte> raw;
    public bool hasSignatureAlgorithm; // format change introduced in TLS 1.2
    public SignatureScheme signatureAlgorithm;
    public slice<byte> signature;
}

private static slice<byte> marshal(this ptr<certificateVerifyMsg> _addr_m) {
    slice<byte> x = default;
    ref certificateVerifyMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    cryptobyte.Builder b = default;
    b.AddUint8(typeCertificateVerify);
    b.AddUint24LengthPrefixed(b => {
        if (m.hasSignatureAlgorithm) {
            b.AddUint16(uint16(m.signatureAlgorithm));
        }
        b.AddUint16LengthPrefixed(b => {
            b.AddBytes(m.signature);
        });
    });

    m.raw = b.BytesOrPanic();
    return m.raw;
}

private static bool unmarshal(this ptr<certificateVerifyMsg> _addr_m, slice<byte> data) {
    ref certificateVerifyMsg m = ref _addr_m.val;

    m.raw = data;
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);

    if (!s.Skip(4)) { // message type and uint24 length field
        return false;
    }
    if (m.hasSignatureAlgorithm) {
        if (!s.ReadUint16((uint16.val)(_addr_m.signatureAlgorithm))) {
            return false;
        }
    }
    return readUint16LengthPrefixed(_addr_s, _addr_m.signature) && s.Empty();
}

private partial struct newSessionTicketMsg {
    public slice<byte> raw;
    public slice<byte> ticket;
}

private static slice<byte> marshal(this ptr<newSessionTicketMsg> _addr_m) {
    slice<byte> x = default;
    ref newSessionTicketMsg m = ref _addr_m.val;

    if (m.raw != null) {
        return m.raw;
    }
    var ticketLen = len(m.ticket);
    nint length = 2 + 4 + ticketLen;
    x = make_slice<byte>(4 + length);
    x[0] = typeNewSessionTicket;
    x[1] = uint8(length >> 16);
    x[2] = uint8(length >> 8);
    x[3] = uint8(length);
    x[8] = uint8(ticketLen >> 8);
    x[9] = uint8(ticketLen);
    copy(x[(int)10..], m.ticket);

    m.raw = x;

    return ;
}

private static bool unmarshal(this ptr<newSessionTicketMsg> _addr_m, slice<byte> data) {
    ref newSessionTicketMsg m = ref _addr_m.val;

    m.raw = data;

    if (len(data) < 10) {
        return false;
    }
    var length = uint32(data[1]) << 16 | uint32(data[2]) << 8 | uint32(data[3]);
    if (uint32(len(data)) - 4 != length) {
        return false;
    }
    var ticketLen = int(data[8]) << 8 + int(data[9]);
    if (len(data) - 10 != ticketLen) {
        return false;
    }
    m.ticket = data[(int)10..];

    return true;
}

private partial struct helloRequestMsg {
}

private static slice<byte> marshal(this ptr<helloRequestMsg> _addr__p0) {
    ref helloRequestMsg _p0 = ref _addr__p0.val;

    return new slice<byte>(new byte[] { typeHelloRequest, 0, 0, 0 });
}

private static bool unmarshal(this ptr<helloRequestMsg> _addr__p0, slice<byte> data) {
    ref helloRequestMsg _p0 = ref _addr__p0.val;

    return len(data) == 4;
}

} // end tls_package
