// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using crypto = crypto_package;
using aes = crypto.aes_package;
using cipher = crypto.cipher_package;
using ecdh = crypto.ecdh_package;
using rand = crypto.rand_package;
using binary = encoding.binary_package;
using errors = errors_package;
using bits = math.bits_package;
using chacha20poly1305 = golang.org.x.crypto.chacha20poly1305_package;
using hkdf = golang.org.x.crypto.hkdf_package;
using crypto;
using encoding;
using golang.org.x.crypto;
using math;

partial class hpke_package {

// testingOnlyGenerateKey is only used during testing, to provide
// a fixed test key to use when checking the RFC 9180 vectors.
internal static ecdh.PrivateKey, error) testingOnlyGenerateKey;

[GoType] partial struct hkdfKDF {
    internal crypto_package.Hash hash;
}

[GoRecv] internal static slice<byte> LabeledExtract(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> salt, @string label, slice<byte> inputKey) {
    var labeledIKM = new slice<byte>(0, 7 + len(suiteID) + len(label) + len(inputKey));
    labeledIKM = append(labeledIKM, slice<byte>("HPKE-v1").ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, suiteID.ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, label.ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, inputKey.ꓸꓸꓸ);
    return hkdf.Extract(kdf.hash.New, labeledIKM, salt);
}

[GoRecv] internal static slice<byte> LabeledExpand(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length) {
    var labeledInfo = new slice<byte>(0, 2 + 7 + len(suiteID) + len(label) + len(info));
    labeledInfo = binary.BigEndian.AppendUint16(labeledInfo, length);
    labeledInfo = append(labeledInfo, slice<byte>("HPKE-v1").ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, suiteID.ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, label.ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, info.ꓸꓸꓸ);
    var @out = new slice<byte>(length);
    var (n, err) = hkdf.Expand(kdf.hash.New, randomKey, labeledInfo).Read(@out);
    if (err != default! || n != ((nint)length)) {
        throw panic("hpke: LabeledExpand failed unexpectedly");
    }
    return @out;
}

// dhKEM implements the KEM specified in RFC 9180, Section 4.1.
[GoType] partial struct dhKEM {
    internal crypto.ecdh_package.ΔCurve dh;
    internal hkdfKDF kdf;
    internal slice<byte> suiteID;
    internal uint16 nSecret;
}

// RFC 9180 Section 7.1
public static ecdh.Curve; hash crypto.Hash; nSecret uint16} SupportedKEMs = new map<uint16, ecdh.Curve; hash crypto.Hash; nSecret uint16}>{
    [32] = new(ecdh.X25519(), crypto.SHA256, 32)
};

internal static (ж<dhKEM>, error) newDHKem(uint16 kemID) {
    var (suite, ok) = SupportedKEMs[kemID];
    if (!ok) {
        return (default!, errors.New("unsupported suite ID"u8));
    }
    return (Ꮡ(new dhKEM(
        dh: suite.curve,
        kdf: new hkdfKDF(suite.hash),
        suiteID: binary.BigEndian.AppendUint16(slice<byte>("KEM"), kemID),
        nSecret: suite.nSecret
    )), default!);
}

[GoRecv] internal static slice<byte> ExtractAndExpand(this ref dhKEM dh, slice<byte> dhKey, slice<byte> kemContext) {
    var eaePRK = dh.kdf.LabeledExtract(dh.suiteID[..], default!, "eae_prk"u8, dhKey);
    return dh.kdf.LabeledExpand(dh.suiteID[..], eaePRK, "shared_secret"u8, kemContext, dh.nSecret);
}

[GoRecv] internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) Encap(this ref dhKEM dh, ж<ecdhꓸPublicKey> ᏑpubRecipient) {
    slice<byte> sharedSecret = default!;
    slice<byte> encapPub = default!;
    error err = default!;

    ref var pubRecipient = ref ᏑpubRecipient.val;
    ж<ecdh.PrivateKey> privEph = default!;
    if (testingOnlyGenerateKey != default!){
        (privEph, err) = testingOnlyGenerateKey();
    } else {
        (privEph, err) = dh.dh.GenerateKey(rand.Reader);
    }
    if (err != default!) {
        return (default!, default!, err);
    }
    (dhVal, err) = privEph.ECDH(ᏑpubRecipient);
    if (err != default!) {
        return (default!, default!, err);
    }
    var encPubEph = privEph.PublicKey().Bytes();
    var encPubRecip = pubRecipient.Bytes();
    var kemContext = append(encPubEph, encPubRecip.ꓸꓸꓸ);
    return (dh.ExtractAndExpand(dhVal, kemContext), encPubEph, default!);
}

[GoType] partial struct Sender {
    internal crypto.cipher_package.AEAD aead;
    internal ж<dhKEM> kem;
    internal slice<byte> sharedSecret;
    internal slice<byte> suiteID;
    internal slice<byte> key;
    internal slice<byte> baseNonce;
    internal slice<byte> exporterSecret;
    internal uint128 seqNum;
}

internal static cipher.AEAD, error) aesGCMNew = (slice<byte> key) => {
    var (block, err) = aes.NewCipher(key);
    if (err != default!) {
        return (default!, err);
    }
    return cipher.NewGCM(block);
};

// RFC 9180, Section 7.3
public static cipher.AEAD, error)} SupportedAEADs = new map<uint16, cipher.AEAD, error)}>{
    [1] = new(keySize: 16, nonceSize: 12, aead: aesGCMNew),
    [2] = new(keySize: 32, nonceSize: 12, aead: aesGCMNew),
    [3] = new(keySize: chacha20poly1305.KeySize, nonceSize: chacha20poly1305.NonceSize, aead: chacha20poly1305.New)
};

// RFC 9180, Section 7.2
public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs = new map<uint16, Func<ж<hkdfKDF>>>{
    [1] = () => Ꮡ(new hkdfKDF(crypto.SHA256))
};

public static (slice<byte>, ж<Sender>, error) SetupSender(uint16 kemID, uint16 kdfID, uint16 aeadID, crypto.PublicKey pub, slice<byte> info) {
    var suiteID = SuiteID(kemID, kdfID, aeadID);
    (kem, err) = newDHKem(kemID);
    if (err != default!) {
        return (default!, default!, err);
    }
    var (pubRecipient, ok) = pub._<ж<ecdhꓸPublicKey>>(ᐧ);
    if (!ok) {
        return (default!, default!, errors.New("incorrect public key type"u8));
    }
    (sharedSecret, encapsulatedKey, err) = kem.Encap(pubRecipient);
    if (err != default!) {
        return (default!, default!, err);
    }
    var kdfInit = SupportedKDFs[kdfID];
    ok = SupportedKDFs[kdfID];
    if (!ok) {
        return (default!, default!, errors.New("unsupported KDF id"u8));
    }
    var kdf = kdfInit();
    var aeadInfo = SupportedAEADs[aeadID];
    ok = SupportedAEADs[aeadID];
    if (!ok) {
        return (default!, default!, errors.New("unsupported AEAD id"u8));
    }
    var pskIDHash = kdf.LabeledExtract(suiteID, default!, "psk_id_hash"u8, default!);
    var infoHash = kdf.LabeledExtract(suiteID, default!, "info_hash"u8, info);
    var ksContext = append(new byte[]{0}.slice(), pskIDHash.ꓸꓸꓸ);
    ksContext = append(ksContext, infoHash.ꓸꓸꓸ);
    var secret = kdf.LabeledExtract(suiteID, sharedSecret, "secret"u8, default!);
    var key = kdf.LabeledExpand(suiteID, secret, "key"u8, ksContext, ((uint16)aeadInfo.keySize));
    /* Nk - key size for AEAD */
    var baseNonce = kdf.LabeledExpand(suiteID, secret, "base_nonce"u8, ksContext, ((uint16)aeadInfo.nonceSize));
    /* Nn - nonce size for AEAD */
    var exporterSecret = kdf.LabeledExpand(suiteID, secret, "exp"u8, ksContext, ((uint16)(~kdf).hash.Size()));
    /* Nh - hash output size of the kdf*/
    (aead, err) = aeadInfo.aead(key);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (encapsulatedKey, Ꮡ(new Sender(
        kem: kem,
        aead: aead,
        sharedSecret: sharedSecret,
        suiteID: suiteID,
        key: key,
        baseNonce: baseNonce,
        exporterSecret: exporterSecret
    )), default!);
}

[GoRecv] internal static slice<byte> nextNonce(this ref Sender s) {
    var nonce = s.seqNum.bytes()[(int)(16 - s.aead.NonceSize())..];
    foreach (var (i, _) in s.baseNonce) {
        nonce[i] ^= (byte)(s.baseNonce[i]);
    }
    // Message limit is, according to the RFC, 2^95+1, which
    // is somewhat confusing, but we do as we're told.
    if (s.seqNum.bitLen() >= (s.aead.NonceSize() * 8) - 1) {
        throw panic("message limit reached");
    }
    s.seqNum = s.seqNum.addOne();
    return nonce;
}

[GoRecv] public static (slice<byte>, error) Seal(this ref Sender s, slice<byte> aad, slice<byte> plaintext) {
    var ciphertext = s.aead.Seal(default!, s.nextNonce(), plaintext, aad);
    return (ciphertext, default!);
}

public static slice<byte> SuiteID(uint16 kemID, uint16 kdfID, uint16 aeadID) {
    var suiteID = new slice<byte>(0, 4 + 2 + 2 + 2);
    suiteID = append(suiteID, slice<byte>("HPKE").ꓸꓸꓸ);
    suiteID = binary.BigEndian.AppendUint16(suiteID, kemID);
    suiteID = binary.BigEndian.AppendUint16(suiteID, kdfID);
    suiteID = binary.BigEndian.AppendUint16(suiteID, aeadID);
    return suiteID;
}

public static (ж<ecdhꓸPublicKey>, error) ParseHPKEPublicKey(uint16 kemID, slice<byte> bytes) {
    var (kemInfo, ok) = SupportedKEMs[kemID];
    if (!ok) {
        return (default!, errors.New("unsupported KEM id"u8));
    }
    return kemInfo.curve.NewPublicKey(bytes);
}

[GoType] partial struct uint128 {
    internal uint64 hi;
    internal uint64 lo;
}

internal static uint128 addOne(this uint128 u) {
    var (lo, carry) = bits.Add64(u.lo, 1, 0);
    return new uint128(u.hi + carry, lo);
}

internal static nint bitLen(this uint128 u) {
    return bits.Len64(u.hi) + bits.Len64(u.lo);
}

internal static slice<byte> bytes(this uint128 u) {
    var b = new slice<byte>(16);
    binary.BigEndian.PutUint64(b[0..], u.hi);
    binary.BigEndian.PutUint64(b[8..], u.lo);
    return b;
}

} // end hpke_package
