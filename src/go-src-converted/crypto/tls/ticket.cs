// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using aes = crypto.aes_package;
using cipher = crypto.cipher_package;
using hmac = crypto.hmac_package;
using sha256 = crypto.sha256_package;
using subtle = crypto.subtle_package;
using x509 = crypto.x509_package;
using errors = errors_package;
using io = io_package;
using cryptobyte = golang.org.x.crypto.cryptobyte_package;
using golang.org.x.crypto;

partial class tls_package {

// A SessionState is a resumable session.
[GoType] partial struct SessionState {
// Encoded as a SessionState (in the language of RFC 8446, Section 3).
//
//   enum { server(1), client(2) } SessionStateType;
//
//   opaque Certificate<1..2^24-1>;
//
//   Certificate CertificateChain<0..2^24-1>;
//
//   opaque Extra<0..2^24-1>;
//
//   struct {
//       uint16 version;
//       SessionStateType type;
//       uint16 cipher_suite;
//       uint64 created_at;
//       opaque secret<1..2^8-1>;
//       Extra extra<0..2^24-1>;
//       uint8 ext_master_secret = { 0, 1 };
//       uint8 early_data = { 0, 1 };
//       CertificateEntry certificate_list<0..2^24-1>;
//       CertificateChain verified_chains<0..2^24-1>; /* excluding leaf */
//       select (SessionState.early_data) {
//           case 0: Empty;
//           case 1: opaque alpn<1..2^8-1>;
//       };
//       select (SessionState.type) {
//           case server: Empty;
//           case client: struct {
//               select (SessionState.version) {
//                   case VersionTLS10..VersionTLS12: Empty;
//                   case VersionTLS13: struct {
//                       uint64 use_by;
//                       uint32 age_add;
//                   };
//               };
//           };
//       };
//   } SessionState;
//

    // Extra is ignored by crypto/tls, but is encoded by [SessionState.Bytes]
    // and parsed by [ParseSessionState].
    //
    // This allows [Config.UnwrapSession]/[Config.WrapSession] and
    // [ClientSessionCache] implementations to store and retrieve additional
    // data alongside this session.
    //
    // To allow different layers in a protocol stack to share this field,
    // applications must only append to it, not replace it, and must use entries
    // that can be recognized even if out of order (for example, by starting
    // with an id and version prefix).
    public slice<slice<byte>> Extra;
    // EarlyData indicates whether the ticket can be used for 0-RTT in a QUIC
    // connection. The application may set this to false if it is true to
    // decline to offer 0-RTT even if supported.
    public bool EarlyData;
    internal uint16 version;
    internal bool isClient;
    internal uint16 cipherSuite;
    // createdAt is the generation time of the secret on the sever (which for
    // TLS 1.0–1.2 might be earlier than the current session) and the time at
    // which the ticket was received on the client.
    internal uint64 createdAt; // seconds since UNIX epoch
    internal slice<byte> secret; // master secret for TLS 1.2, or the PSK for TLS 1.3
    internal bool extMasterSecret;
    internal x509.Certificate peerCertificates;
    internal slice<ж<activeCert>> activeCertHandles;
    internal slice<byte> ocspResponse;
    internal slice<slice<byte>> scts;
    internal x509.Certificate verifiedChains;
    internal @string alpnProtocol; // only set if EarlyData is true
    // Client-side TLS 1.3-only fields.
    internal uint64 useBy; // seconds since UNIX epoch
    internal uint32 ageAdd;
    internal slice<byte> ticket;
}

// Bytes encodes the session, including any private fields, so that it can be
// parsed by [ParseSessionState]. The encoding contains secret values critical
// to the security of future and possibly past sessions.
//
// The specific encoding should be considered opaque and may change incompatibly
// between Go versions.
[GoRecv] public static (slice<byte>, error) Bytes(this ref SessionState s) {
    ref var b = ref heap(new vendor.golang.org.x.crypto.cryptobyte_package.Builder(), out var Ꮡb);
    b.AddUint16(s.version);
    if (s.isClient){
        b.AddUint8(2);
    } else {
        // client
        b.AddUint8(1);
    }
    // server
    b.AddUint16(s.cipherSuite);
    addUint64(Ꮡb, s.createdAt);
    b.AddUint8LengthPrefixed((ж<cryptobyte.Builder> b) => {
        bΔ1.AddBytes(s.secret);
    });
    b.AddUint24LengthPrefixed(
    (ж<cryptobyte.Builder> b) => {
        foreach (var (_, extra) in s.Extra) {
            bΔ2.AddUint24LengthPrefixed(
            var extraʗ5 = extra;
            (ж<cryptobyte.Builder> b) => {
                bΔ3.AddBytes(extraʗ5);
            });
        }
    });
    if (s.extMasterSecret){
        b.AddUint8(1);
    } else {
        b.AddUint8(0);
    }
    if (s.EarlyData){
        b.AddUint8(1);
    } else {
        b.AddUint8(0);
    }
    marshalCertificate(Ꮡb, new Certificate(
        Certificate: certificatesToBytesSlice(s.peerCertificates),
        OCSPStaple: s.ocspResponse,
        SignedCertificateTimestamps: s.scts
    ));
    b.AddUint24LengthPrefixed(
    (ж<cryptobyte.Builder> b) => {
        foreach (var (_, chain) in s.verifiedChains) {
            bΔ4.AddUint24LengthPrefixed(
            var chainʗ5 = chain;
            (ж<cryptobyte.Builder> b) => {
                if (len(chainʗ5) == 0) {
                    bΔ5.SetError(errors.New("tls: internal error: empty verified chain"u8));
                    return;
                }
                foreach (var (_, cert) in chainʗ5[1..]) {
                    bΔ5.AddUint24LengthPrefixed(
                    var certʗ14 = cert;
                    (ж<cryptobyte.Builder> b) => {
                        bΔ6.AddBytes((~certʗ14).Raw);
                    });
                }
            });
        }
    });
    if (s.EarlyData) {
        b.AddUint8LengthPrefixed(
        (ж<cryptobyte.Builder> b) => {
            bΔ7.AddBytes(slice<byte>(s.alpnProtocol));
        });
    }
    if (s.isClient) {
        if (s.version >= VersionTLS13) {
            addUint64(Ꮡb, s.useBy);
            b.AddUint32(s.ageAdd);
        }
    }
    return b.Bytes();
}

internal static slice<slice<byte>> certificatesToBytesSlice(slice<x509.Certificate> certs) {
    var s = new slice<slice<byte>>(0, len(certs));
    foreach (var (_, c) in certs) {
        s = append(s, (~c).Raw);
    }
    return s;
}

// ParseSessionState parses a [SessionState] encoded by [SessionState.Bytes].
public static (ж<SessionState>, error) ParseSessionState(slice<byte> data) {
    var ss = Ꮡ(new SessionState(nil));
    var s = ((cryptobyte.String)data);
    ref var typ = ref heap(new uint8(), out var Ꮡtyp);
    ref var extMasterSecret = ref heap(new uint8(), out var ᏑextMasterSecret);
    ref var earlyData = ref heap(new uint8(), out var ᏑearlyData);
    ref var certΔ1 = ref heap(new Certificate(), out var ᏑcertΔ1);
    cryptobyte.String extra = default!;
    if (!s.ReadUint16(Ꮡ((~ss).version)) || !s.ReadUint8(Ꮡtyp) || (typ != 1 && typ != 2) || !s.ReadUint16(Ꮡ((~ss).cipherSuite)) || !readUint64(Ꮡ(s), Ꮡ((~ss).createdAt)) || !readUint8LengthPrefixed(Ꮡ(s), Ꮡ((~ss).secret)) || !s.ReadUint24LengthPrefixed(Ꮡ(extra)) || !s.ReadUint8(ᏑextMasterSecret) || !s.ReadUint8(ᏑearlyData) || len((~ss).secret) == 0 || !unmarshalCertificate(Ꮡ(s), ᏑcertΔ1)) {
        return (default!, errors.New("tls: invalid session encoding"u8));
    }
    while (!extra.Empty()) {
        slice<byte> e = default!;
        if (!readUint24LengthPrefixed(Ꮡ(extra), Ꮡ(e))) {
            return (default!, errors.New("tls: invalid session encoding"u8));
        }
        ss.val.Extra = append((~ss).Extra, e);
    }
    switch (extMasterSecret) {
    case 0: {
        ss.val.extMasterSecret = false;
        break;
    }
    case 1: {
        ss.val.extMasterSecret = true;
        break;
    }
    default: {
        return (default!, errors.New("tls: invalid session encoding"u8));
    }}

    switch (earlyData) {
    case 0: {
        ss.val.EarlyData = false;
        break;
    }
    case 1: {
        ss.val.EarlyData = true;
        break;
    }
    default: {
        return (default!, errors.New("tls: invalid session encoding"u8));
    }}

    foreach (var (_, certΔ2) in certΔ1.Certificate) {
        (c, err) = globalCertCache.newCert(certΔ2);
        if (err != default!) {
            return (default!, err);
        }
        ss.val.activeCertHandles = append((~ss).activeCertHandles, c);
        ss.val.peerCertificates = append((~ss).peerCertificates, (~c).certΔ2);
    }
    ss.val.ocspResponse = certΔ1.OCSPStaple;
    ss.val.scts = certΔ1.SignedCertificateTimestamps;
    cryptobyte.String chainList = default!;
    if (!s.ReadUint24LengthPrefixed(Ꮡ(chainList))) {
        return (default!, errors.New("tls: invalid session encoding"u8));
    }
    while (!chainList.Empty()) {
        cryptobyte.String certList = default!;
        if (!chainList.ReadUint24LengthPrefixed(Ꮡ(certList))) {
            return (default!, errors.New("tls: invalid session encoding"u8));
        }
        slice<x509.Certificate> chain = default!;
        if (len((~ss).peerCertificates) == 0) {
            return (default!, errors.New("tls: invalid session encoding"u8));
        }
        chain = append(chain, (~ss).peerCertificates[0]);
        while (!certList.Empty()) {
            slice<byte> certΔ3 = default!;
            if (!readUint24LengthPrefixed(Ꮡ(certList), Ꮡ(certΔ3))) {
                return (default!, errors.New("tls: invalid session encoding"u8));
            }
            (c, err) = globalCertCache.newCert(certΔ3);
            if (err != default!) {
                return (default!, err);
            }
            ss.val.activeCertHandles = append((~ss).activeCertHandles, c);
            chain = append(chain, (~c).cert);
        }
        ss.val.verifiedChains = append((~ss).verifiedChains, chain);
    }
    if ((~ss).EarlyData) {
        slice<byte> alpn = default!;
        if (!readUint8LengthPrefixed(Ꮡ(s), Ꮡ(alpn))) {
            return (default!, errors.New("tls: invalid session encoding"u8));
        }
        ss.val.alpnProtocol = ((@string)alpn);
    }
    {
        var isClient = typ == 2; if (!isClient) {
            if (!s.Empty()) {
                return (default!, errors.New("tls: invalid session encoding"u8));
            }
            return (ss, default!);
        }
    }
    ss.val.isClient = true;
    if (len((~ss).peerCertificates) == 0) {
        return (default!, errors.New("tls: no server certificates in client session"u8));
    }
    if ((~ss).version < VersionTLS13) {
        if (!s.Empty()) {
            return (default!, errors.New("tls: invalid session encoding"u8));
        }
        return (ss, default!);
    }
    if (!s.ReadUint64(Ꮡ((~ss).useBy)) || !s.ReadUint32(Ꮡ((~ss).ageAdd)) || !s.Empty()) {
        return (default!, errors.New("tls: invalid session encoding"u8));
    }
    return (ss, default!);
}

// sessionState returns a partially filled-out [SessionState] with information
// from the current connection.
[GoRecv] internal static ж<SessionState> sessionState(this ref Conn c) {
    return Ꮡ(new SessionState(
        version: c.vers,
        cipherSuite: c.cipherSuite,
        createdAt: ((uint64)c.config.time().Unix()),
        alpnProtocol: c.clientProtocol,
        peerCertificates: c.peerCertificates,
        activeCertHandles: c.activeCertHandles,
        ocspResponse: c.ocspResponse,
        scts: c.scts,
        isClient: c.isClient,
        extMasterSecret: c.extMasterSecret,
        verifiedChains: c.verifiedChains
    ));
}

// EncryptTicket encrypts a ticket with the [Config]'s configured (or default)
// session ticket keys. It can be used as a [Config.WrapSession] implementation.
[GoRecv] public static (slice<byte>, error) EncryptTicket(this ref Config c, ΔConnectionState cs, ж<SessionState> Ꮡss) {
    ref var ss = ref Ꮡss.val;

    var ticketKeys = c.ticketKeys(nil);
    (stateBytes, err) = ss.Bytes();
    if (err != default!) {
        return (default!, err);
    }
    return c.encryptTicket(stateBytes, ticketKeys);
}

[GoRecv] internal static (slice<byte>, error) encryptTicket(this ref Config c, slice<byte> state, slice<ticketKey> ticketKeys) {
    if (len(ticketKeys) == 0) {
        return (default!, errors.New("tls: internal error: session ticket keys unavailable"u8));
    }
    var encrypted = new slice<byte>(aes.ΔBlockSize + len(state) + sha256.ΔSize);
    var iv = encrypted[..(int)(aes.ΔBlockSize)];
    var ciphertext = encrypted[(int)(aes.ΔBlockSize)..(int)(len(encrypted) - sha256.ΔSize)];
    var authenticated = encrypted[..(int)(len(encrypted) - sha256.ΔSize)];
    var macBytes = encrypted[(int)(len(encrypted) - sha256.ΔSize)..];
    {
        var (_, errΔ1) = io.ReadFull(c.rand(), iv); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    var key = ticketKeys[0];
    (block, err) = aes.NewCipher(key.aesKey[..]);
    if (err != default!) {
        return (default!, errors.New("tls: failed to create cipher while encrypting ticket: "u8 + err.Error()));
    }
    cipher.NewCTR(block, iv).XORKeyStream(ciphertext, state);
    var mac = hmac.New(sha256.New, key.hmacKey[..]);
    mac.Write(authenticated);
    mac.Sum(macBytes[..0]);
    return (encrypted, default!);
}

// DecryptTicket decrypts a ticket encrypted by [Config.EncryptTicket]. It can
// be used as a [Config.UnwrapSession] implementation.
//
// If the ticket can't be decrypted or parsed, DecryptTicket returns (nil, nil).
[GoRecv] public static (ж<SessionState>, error) DecryptTicket(this ref Config c, slice<byte> identity, ΔConnectionState cs) {
    var ticketKeys = c.ticketKeys(nil);
    var stateBytes = c.decryptTicket(identity, ticketKeys);
    if (stateBytes == default!) {
        return (default!, default!);
    }
    (s, err) = ParseSessionState(stateBytes);
    if (err != default!) {
        return (default!, default!);
    }
    // drop unparsable tickets on the floor
    return (s, default!);
}

[GoRecv] internal static slice<byte> decryptTicket(this ref Config c, slice<byte> encrypted, slice<ticketKey> ticketKeys) {
    if (len(encrypted) < aes.ΔBlockSize + sha256.ΔSize) {
        return default!;
    }
    var iv = encrypted[..(int)(aes.ΔBlockSize)];
    var ciphertext = encrypted[(int)(aes.ΔBlockSize)..(int)(len(encrypted) - sha256.ΔSize)];
    var authenticated = encrypted[..(int)(len(encrypted) - sha256.ΔSize)];
    var macBytes = encrypted[(int)(len(encrypted) - sha256.ΔSize)..];
    foreach (var (_, key) in ticketKeys) {
        var mac = hmac.New(sha256.New, key.hmacKey[..]);
        mac.Write(authenticated);
        var expected = mac.Sum(default!);
        if (subtle.ConstantTimeCompare(macBytes, expected) != 1) {
            continue;
        }
        (block, err) = aes.NewCipher(key.aesKey[..]);
        if (err != default!) {
            return default!;
        }
        var plaintext = new slice<byte>(len(ciphertext));
        cipher.NewCTR(block, iv).XORKeyStream(plaintext, ciphertext);
        return plaintext;
    }
    return default!;
}

// ClientSessionState contains the state needed by a client to
// resume a previous TLS session.
[GoType] partial struct ClientSessionState {
    internal ж<SessionState> session;
}

// ResumptionState returns the session ticket sent by the server (also known as
// the session's identity) and the state necessary to resume this session.
//
// It can be called by [ClientSessionCache.Put] to serialize (with
// [SessionState.Bytes]) and store the session.
[GoRecv] public static (slice<byte> ticket, ж<SessionState> state, error err) ResumptionState(this ref ClientSessionState cs) {
    slice<byte> ticket = default!;
    ж<SessionState> state = default!;
    error err = default!;

    if (cs == nil || cs.session == nil) {
        return (default!, default!, default!);
    }
    return (cs.session.ticket, cs.session, default!);
}

// NewResumptionState returns a state value that can be returned by
// [ClientSessionCache.Get] to resume a previous session.
//
// state needs to be returned by [ParseSessionState], and the ticket and session
// state must have been returned by [ClientSessionState.ResumptionState].
public static (ж<ClientSessionState>, error) NewResumptionState(slice<byte> ticket, ж<SessionState> Ꮡstate) {
    ref var state = ref Ꮡstate.val;

    state.ticket = ticket;
    return (Ꮡ(new ClientSessionState(
        session: state
    )), default!);
}

} // end tls_package
