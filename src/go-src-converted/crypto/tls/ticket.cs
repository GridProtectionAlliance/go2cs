// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tls -- go2cs converted at 2022 March 13 05:36:14 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Program Files\Go\src\crypto\tls\ticket.go
namespace go.crypto;

using bytes = bytes_package;
using aes = crypto.aes_package;
using cipher = crypto.cipher_package;
using hmac = crypto.hmac_package;
using sha256 = crypto.sha256_package;
using subtle = crypto.subtle_package;
using errors = errors_package;
using io = io_package;

using cryptobyte = golang.org.x.crypto.cryptobyte_package;


// sessionState contains the information that is serialized into a session
// ticket in order to later resume a connection.

using System;
public static partial class tls_package {

private partial struct sessionState {
    public ushort vers;
    public ushort cipherSuite;
    public ulong createdAt;
    public slice<byte> masterSecret; // opaque master_secret<1..2^16-1>;
// struct { opaque certificate<1..2^24-1> } Certificate;
    public slice<slice<byte>> certificates; // Certificate certificate_list<0..2^24-1>;

// usedOldKey is true if the ticket from which this session came from
// was encrypted with an older key and thus should be refreshed.
    public bool usedOldKey;
}

private static slice<byte> marshal(this ptr<sessionState> _addr_m) {
    ref sessionState m = ref _addr_m.val;

    ref cryptobyte.Builder b = ref heap(out ptr<cryptobyte.Builder> _addr_b);
    b.AddUint16(m.vers);
    b.AddUint16(m.cipherSuite);
    addUint64(_addr_b, m.createdAt);
    b.AddUint16LengthPrefixed(b => {
        b.AddBytes(m.masterSecret);
    });
    b.AddUint24LengthPrefixed(b => {
        foreach (var (_, cert) in m.certificates) {
            b.AddUint24LengthPrefixed(b => {
                b.AddBytes(cert);
            });
        }
    });
    return b.BytesOrPanic();
}

private static bool unmarshal(this ptr<sessionState> _addr_m, slice<byte> data) {
    ref sessionState m = ref _addr_m.val;

    m.val = new sessionState(usedOldKey:m.usedOldKey);
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);
    {
        var ok = s.ReadUint16(_addr_m.vers) && s.ReadUint16(_addr_m.cipherSuite) && readUint64(_addr_s, _addr_m.createdAt) && readUint16LengthPrefixed(_addr_s, _addr_m.masterSecret) && len(m.masterSecret) != 0;

        if (!ok) {
            return false;
        }
    }
    ref cryptobyte.String certList = ref heap(out ptr<cryptobyte.String> _addr_certList);
    if (!s.ReadUint24LengthPrefixed(_addr_certList)) {
        return false;
    }
    while (!certList.Empty()) {
        ref slice<byte> cert = ref heap(out ptr<slice<byte>> _addr_cert);
        if (!readUint24LengthPrefixed(_addr_certList, _addr_cert)) {
            return false;
        }
        m.certificates = append(m.certificates, cert);
    }
    return s.Empty();
}

// sessionStateTLS13 is the content of a TLS 1.3 session ticket. Its first
// version (revision = 0) doesn't carry any of the information needed for 0-RTT
// validation and the nonce is always empty.
private partial struct sessionStateTLS13 {
    public ushort cipherSuite;
    public ulong createdAt;
    public slice<byte> resumptionSecret; // opaque resumption_master_secret<1..2^8-1>;
    public Certificate certificate; // CertificateEntry certificate_list<0..2^24-1>;
}

private static slice<byte> marshal(this ptr<sessionStateTLS13> _addr_m) {
    ref sessionStateTLS13 m = ref _addr_m.val;

    ref cryptobyte.Builder b = ref heap(out ptr<cryptobyte.Builder> _addr_b);
    b.AddUint16(VersionTLS13);
    b.AddUint8(0); // revision
    b.AddUint16(m.cipherSuite);
    addUint64(_addr_b, m.createdAt);
    b.AddUint8LengthPrefixed(b => {
        b.AddBytes(m.resumptionSecret);
    });
    marshalCertificate(_addr_b, m.certificate);
    return b.BytesOrPanic();
}

private static bool unmarshal(this ptr<sessionStateTLS13> _addr_m, slice<byte> data) {
    ref sessionStateTLS13 m = ref _addr_m.val;

    m.val = new sessionStateTLS13();
    ref var s = ref heap(cryptobyte.String(data), out ptr<var> _addr_s);
    ref ushort version = ref heap(out ptr<ushort> _addr_version);
    ref byte revision = ref heap(out ptr<byte> _addr_revision);
    return s.ReadUint16(_addr_version) && version == VersionTLS13 && s.ReadUint8(_addr_revision) && revision == 0 && s.ReadUint16(_addr_m.cipherSuite) && readUint64(_addr_s, _addr_m.createdAt) && readUint8LengthPrefixed(_addr_s, _addr_m.resumptionSecret) && len(m.resumptionSecret) != 0 && unmarshalCertificate(_addr_s, _addr_m.certificate) && s.Empty();
}

private static (slice<byte>, error) encryptTicket(this ptr<Conn> _addr_c, slice<byte> state) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Conn c = ref _addr_c.val;

    if (len(c.ticketKeys) == 0) {
        return (null, error.As(errors.New("tls: internal error: session ticket keys unavailable"))!);
    }
    var encrypted = make_slice<byte>(ticketKeyNameLen + aes.BlockSize + len(state) + sha256.Size);
    var keyName = encrypted[..(int)ticketKeyNameLen];
    var iv = encrypted[(int)ticketKeyNameLen..(int)ticketKeyNameLen + aes.BlockSize];
    var macBytes = encrypted[(int)len(encrypted) - sha256.Size..];

    {
        var (_, err) = io.ReadFull(c.config.rand(), iv);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    var key = c.ticketKeys[0];
    copy(keyName, key.keyName[..]);
    var (block, err) = aes.NewCipher(key.aesKey[..]);
    if (err != null) {
        return (null, error.As(errors.New("tls: failed to create cipher while encrypting ticket: " + err.Error()))!);
    }
    cipher.NewCTR(block, iv).XORKeyStream(encrypted[(int)ticketKeyNameLen + aes.BlockSize..], state);

    var mac = hmac.New(sha256.New, key.hmacKey[..]);
    mac.Write(encrypted[..(int)len(encrypted) - sha256.Size]);
    mac.Sum(macBytes[..(int)0]);

    return (encrypted, error.As(null!)!);
}

private static (slice<byte>, bool) decryptTicket(this ptr<Conn> _addr_c, slice<byte> encrypted) {
    slice<byte> plaintext = default;
    bool usedOldKey = default;
    ref Conn c = ref _addr_c.val;

    if (len(encrypted) < ticketKeyNameLen + aes.BlockSize + sha256.Size) {
        return (null, false);
    }
    var keyName = encrypted[..(int)ticketKeyNameLen];
    var iv = encrypted[(int)ticketKeyNameLen..(int)ticketKeyNameLen + aes.BlockSize];
    var macBytes = encrypted[(int)len(encrypted) - sha256.Size..];
    var ciphertext = encrypted[(int)ticketKeyNameLen + aes.BlockSize..(int)len(encrypted) - sha256.Size];

    nint keyIndex = -1;
    foreach (var (i, candidateKey) in c.ticketKeys) {
        if (bytes.Equal(keyName, candidateKey.keyName[..])) {
            keyIndex = i;
            break;
        }
    }    if (keyIndex == -1) {
        return (null, false);
    }
    var key = _addr_c.ticketKeys[keyIndex];

    var mac = hmac.New(sha256.New, key.hmacKey[..]);
    mac.Write(encrypted[..(int)len(encrypted) - sha256.Size]);
    var expected = mac.Sum(null);

    if (subtle.ConstantTimeCompare(macBytes, expected) != 1) {
        return (null, false);
    }
    var (block, err) = aes.NewCipher(key.aesKey[..]);
    if (err != null) {
        return (null, false);
    }
    plaintext = make_slice<byte>(len(ciphertext));
    cipher.NewCTR(block, iv).XORKeyStream(plaintext, ciphertext);

    return (plaintext, keyIndex > 0);
}

} // end tls_package
