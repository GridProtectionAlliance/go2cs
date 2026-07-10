// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using crypto = crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using ecdh = go.crypto.ecdh_package;
using rand = go.crypto.rand_package;
using binary = encoding.binary_package;
using errors = errors_package;
using bits = math.bits_package;
using chacha20poly1305 = vendor.golang.org.x.crypto.chacha20poly1305_package;
using hkdf = vendor.golang.org.x.crypto.hkdf_package;
using encoding;
using go.crypto;
using hash = hash_package;
using io = io_package;
using math;
using vendor.golang.org.x.crypto;

partial class hpke_package {

// testingOnlyGenerateKey is only used during testing, to provide
// a fixed test key to use when checking the RFC 9180 vectors.
internal static Func<(ж<ecdh.PrivateKey>, error)> testingOnlyGenerateKey;

[GoType] public partial struct hkdfKDF {
    internal crypto.Hash hash;
}

public static slice<byte> LabeledExtract(this ж<hkdfKDF> Ꮡkdf, slice<byte> suiteID, slice<byte> salt, @string label, slice<byte> inputKey) {
    ref var kdf = ref Ꮡkdf.Value;

    var labeledIKM = new slice<byte>(0, 7 + len(suiteID) + len(label) + len(inputKey));
    labeledIKM = append(labeledIKM, slice<byte>((@string)"HPKE-v1").ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, suiteID.ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, label.ꓸꓸꓸ);
    labeledIKM = append(labeledIKM, inputKey.ꓸꓸꓸ);
    return hkdf.Extract(() => Ꮡkdf.Value.hash.New(), labeledIKM, salt);
}

public static slice<byte> LabeledExpand(this ж<hkdfKDF> Ꮡkdf, slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length) {
    ref var kdf = ref Ꮡkdf.Value;

    var labeledInfo = new slice<byte>(0, 2 + 7 + len(suiteID) + len(label) + len(info));
    labeledInfo = binary.BigEndian.AppendUint16(labeledInfo, length);
    labeledInfo = append(labeledInfo, slice<byte>((@string)"HPKE-v1").ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, suiteID.ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, label.ꓸꓸꓸ);
    labeledInfo = append(labeledInfo, info.ꓸꓸꓸ);
    var @out = new slice<byte>(length);
    var (n, err) = hkdf.Expand(() => Ꮡkdf.Value.hash.New(), randomKey, labeledInfo).Read(@out);
    if (err != default! || n != (nint)length) {
        throw panic("hpke: LabeledExpand failed unexpectedly");
    }
    return @out;
}

// dhKEM implements the KEM specified in RFC 9180, Section 4.1.
[GoType] partial struct dhKEM {
    internal ecdhꓸCurve dh;
    internal hkdfKDF kdf;
    internal slice<byte> suiteID;
    internal uint16 nSecret;
}

// RFC 9180 Section 7.1

[GoType("dyn")] partial struct SupportedKEMsᴛ1 {
    internal ecdhꓸCurve curve;
    internal crypto.Hash hash;
    internal uint16 nSecret;
}
public static map<uint16, SupportedKEMsᴛ1> SupportedKEMs = new map<uint16, SupportedKEMsᴛ1>{
    [0x0020] = new(ecdh.X25519(), crypto.SHA256, 32)
};

internal static (ж<dhKEM>, error) newDHKem(uint16 kemID) {
    var (suite, ok) = SupportedKEMs[kemID, ꟷ];
    if (!ok) {
        return (default!, errors.New("unsupported suite ID"u8));
    }
    return (Ꮡ(new dhKEM(
        dh: suite.curve,
        kdf: new hkdfKDF(suite.hash),
        suiteID: binary.BigEndian.AppendUint16(slice<byte>((@string)"KEM"), kemID),
        nSecret: suite.nSecret
    )), default!);
}

internal static slice<byte> ExtractAndExpand(this ж<dhKEM> Ꮡdh, slice<byte> dhKey, slice<byte> kemContext) {
    ref var dh = ref Ꮡdh.Value;

    var eaePRK = Ꮡdh.of(dhKEM.Ꮡkdf).LabeledExtract(dh.suiteID[..], default!, "eae_prk"u8, dhKey);
    return Ꮡdh.of(dhKEM.Ꮡkdf).LabeledExpand(dh.suiteID[..], eaePRK, "shared_secret"u8, kemContext, dh.nSecret);
}

internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) Encap(this ж<dhKEM> Ꮡdh, ж<ecdhꓸPublicKey> ᏑpubRecipient) {
    slice<byte> sharedSecret = default!;
    slice<byte> encapPub = default!;
    error err = default!;

    ref var dh = ref Ꮡdh.Value;
    ref var pubRecipient = ref ᏑpubRecipient.Value;
    ж<ecdh.PrivateKey> privEph = default!;
    if (testingOnlyGenerateKey != default!){
        (privEph, err) = testingOnlyGenerateKey();
    } else {
        (privEph, err) = dh.dh.GenerateKey(rand.Reader);
    }
    if (err != default!) {
        return (default!, default!, err);
    }
    (var dhVal, err) = privEph.ECDH(ᏑpubRecipient);
    if (err != default!) {
        return (default!, default!, err);
    }
    var encPubEph = privEph.PublicKey().Bytes();
    var encPubRecip = pubRecipient.Bytes();
    var kemContext = append(encPubEph, encPubRecip.ꓸꓸꓸ);
    return (Ꮡdh.ExtractAndExpand(dhVal, kemContext), encPubEph, default!);
}

[GoType] partial struct Sender {
    internal cipher.AEAD aead;
    internal ж<dhKEM> kem;
    internal slice<byte> sharedSecret;
    internal slice<byte> suiteID;
    internal slice<byte> key;
    internal slice<byte> baseNonce;
    internal slice<byte> exporterSecret;
    internal uint128 seqNum;
}

internal static Func<slice<byte>, (cipher.AEAD, error)> aesGCMNew = (slice<byte> key) => {
    var (block, err) = aes.NewCipher(key);
    if (err != default!) {
        return (default!, err);
    }
    return cipher.NewGCM(block);
};

// RFC 9180, Section 7.3

[GoType("dyn")] partial struct SupportedAEADsᴛ1 {
    internal nint keySize;
    internal nint nonceSize;
    internal Func<slice<byte>, (cipher.AEAD, error)> aead;
}
public static map<uint16, SupportedAEADsᴛ1> SupportedAEADs = new map<uint16, SupportedAEADsᴛ1>{
    [0x0001] = new(keySize: 16, nonceSize: 12, aead: aesGCMNew),
    [0x0002] = new(keySize: 32, nonceSize: 12, aead: aesGCMNew),
    [0x0003] = new(keySize: chacha20poly1305.KeySize, nonceSize: chacha20poly1305.ΔNonceSize, aead: chacha20poly1305.New)
};

// RFC 9180, Section 7.2
public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs = new map<uint16, Func<ж<hkdfKDF>>>{
    [0x0001] = () => Ꮡ(new hkdfKDF(crypto.SHA256))
};

public static (slice<byte>, ж<Sender>, error) SetupSender(uint16 kemID, uint16 kdfID, uint16 aeadID, cryptoꓸPublicKey pub, slice<byte> info) {
    var suiteID = SuiteID(kemID, kdfID, aeadID);
    var (kem, err) = newDHKem(kemID);
    if (err != default!) {
        return (default!, default!, err);
    }
    var (pubRecipient, ok) = pub._<ж<ecdhꓸPublicKey>>(ᐧ);
    if (!ok) {
        return (default!, default!, errors.New("incorrect public key type"u8));
    }
    (var sharedSecret, var encapsulatedKey, err) = kem.Encap(pubRecipient);
    if (err != default!) {
        return (default!, default!, err);
    }
    (var kdfInit, ok) = SupportedKDFs[kdfID, ꟷ];
    if (!ok) {
        return (default!, default!, errors.New("unsupported KDF id"u8));
    }
    var kdf = kdfInit();
    (var aeadInfo, ok) = SupportedAEADs[aeadID, ꟷ];
    if (!ok) {
        return (default!, default!, errors.New("unsupported AEAD id"u8));
    }
    var pskIDHash = kdf.LabeledExtract(suiteID, default!, "psk_id_hash"u8, default!);
    var infoHash = kdf.LabeledExtract(suiteID, default!, "info_hash"u8, info);
    var ksContext = append(new byte[]{0}.slice(), pskIDHash.ꓸꓸꓸ);
    ksContext = append(ksContext, infoHash.ꓸꓸꓸ);
    var secret = kdf.LabeledExtract(suiteID, sharedSecret, "secret"u8, default!);
    var key = kdf.LabeledExpand(suiteID, secret, "key"u8, ksContext, (uint16)aeadInfo.keySize);
    /* Nk - key size for AEAD */
    var baseNonce = kdf.LabeledExpand(suiteID, secret, "base_nonce"u8, ksContext, (uint16)aeadInfo.nonceSize);
    /* Nn - nonce size for AEAD */
    var exporterSecret = kdf.LabeledExpand(suiteID, secret, "exp"u8, ksContext, (uint16)(~kdf).hash.Size());
    /* Nh - hash output size of the kdf*/
    (var aead, err) = aeadInfo.aead(key);
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
    suiteID = append(suiteID, slice<byte>((@string)"HPKE").ꓸꓸꓸ);
    suiteID = binary.BigEndian.AppendUint16(suiteID, kemID);
    suiteID = binary.BigEndian.AppendUint16(suiteID, kdfID);
    suiteID = binary.BigEndian.AppendUint16(suiteID, aeadID);
    return suiteID;
}

public static (ж<ecdhꓸPublicKey>, error) ParseHPKEPublicKey(uint16 kemID, slice<byte> bytes) {
    var (kemInfo, ok) = SupportedKEMs[kemID, ꟷ];
    if (!ok) {
        return (default!, errors.New("unsupported KEM id"u8));
    }
    return kemInfo.curve.NewPublicKey(bytes);
}

[GoType] partial struct uint128 {
    internal uint64 hi, lo;
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
