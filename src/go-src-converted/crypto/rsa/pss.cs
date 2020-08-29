// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rsa -- go2cs converted at 2020 August 29 08:30:56 UTC
// import "crypto/rsa" ==> using rsa = go.crypto.rsa_package
// Original source: C:\Go\src\crypto\rsa\pss.go
// This file implements the PSS signature scheme [1].
//
// [1] https://www.emc.com/collateral/white-papers/h11300-pkcs-1v2-2-rsa-cryptography-standard-wp.pdf

using bytes = go.bytes_package;
using crypto = go.crypto_package;
using errors = go.errors_package;
using hash = go.hash_package;
using io = go.io_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rsa_package
    {
        private static (slice<byte>, error) emsaPSSEncode(slice<byte> mHash, long emBits, slice<byte> salt, hash.Hash hash)
        { 
            // See [1], section 9.1.1
            var hLen = hash.Size();
            var sLen = len(salt);
            var emLen = (emBits + 7L) / 8L; 

            // 1.  If the length of M is greater than the input limitation for the
            //     hash function (2^61 - 1 octets for SHA-1), output "message too
            //     long" and stop.
            //
            // 2.  Let mHash = Hash(M), an octet string of length hLen.

            if (len(mHash) != hLen)
            {
                return (null, errors.New("crypto/rsa: input must be hashed message"));
            }
            if (emLen < hLen + sLen + 2L)
            {
                return (null, errors.New("crypto/rsa: encoding error"));
            }
            var em = make_slice<byte>(emLen);
            var db = em[..emLen - sLen - hLen - 2L + 1L + sLen];
            var h = em[emLen - sLen - hLen - 2L + 1L + sLen..emLen - 1L]; 

            // 4.  Generate a random octet string salt of length sLen; if sLen = 0,
            //     then salt is the empty string.
            //
            // 5.  Let
            //       M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt;
            //
            //     M' is an octet string of length 8 + hLen + sLen with eight
            //     initial zero octets.
            //
            // 6.  Let H = Hash(M'), an octet string of length hLen.

            array<byte> prefix = new array<byte>(8L);

            hash.Write(prefix[..]);
            hash.Write(mHash);
            hash.Write(salt);

            h = hash.Sum(h[..0L]);
            hash.Reset(); 

            // 7.  Generate an octet string PS consisting of emLen - sLen - hLen - 2
            //     zero octets. The length of PS may be 0.
            //
            // 8.  Let DB = PS || 0x01 || salt; DB is an octet string of length
            //     emLen - hLen - 1.

            db[emLen - sLen - hLen - 2L] = 0x01UL;
            copy(db[emLen - sLen - hLen - 1L..], salt); 

            // 9.  Let dbMask = MGF(H, emLen - hLen - 1).
            //
            // 10. Let maskedDB = DB \xor dbMask.

            mgf1XOR(db, hash, h); 

            // 11. Set the leftmost 8 * emLen - emBits bits of the leftmost octet in
            //     maskedDB to zero.

            db[0L] &= (0xFFUL >> (int)(uint(8L * emLen - emBits))); 

            // 12. Let EM = maskedDB || H || 0xbc.
            em[emLen - 1L] = 0xBCUL; 

            // 13. Output EM.
            return (em, null);
        }

        private static error emsaPSSVerify(slice<byte> mHash, slice<byte> em, long emBits, long sLen, hash.Hash hash)
        { 
            // 1.  If the length of M is greater than the input limitation for the
            //     hash function (2^61 - 1 octets for SHA-1), output "inconsistent"
            //     and stop.
            //
            // 2.  Let mHash = Hash(M), an octet string of length hLen.
            var hLen = hash.Size();
            if (hLen != len(mHash))
            {
                return error.As(ErrVerification);
            } 

            // 3.  If emLen < hLen + sLen + 2, output "inconsistent" and stop.
            var emLen = (emBits + 7L) / 8L;
            if (emLen < hLen + sLen + 2L)
            {
                return error.As(ErrVerification);
            } 

            // 4.  If the rightmost octet of EM does not have hexadecimal value
            //     0xbc, output "inconsistent" and stop.
            if (em[len(em) - 1L] != 0xBCUL)
            {
                return error.As(ErrVerification);
            } 

            // 5.  Let maskedDB be the leftmost emLen - hLen - 1 octets of EM, and
            //     let H be the next hLen octets.
            var db = em[..emLen - hLen - 1L];
            var h = em[emLen - hLen - 1L..len(em) - 1L]; 

            // 6.  If the leftmost 8 * emLen - emBits bits of the leftmost octet in
            //     maskedDB are not all equal to zero, output "inconsistent" and
            //     stop.
            if (em[0L] & (0xFFUL << (int)(uint(8L - (8L * emLen - emBits)))) != 0L)
            {
                return error.As(ErrVerification);
            } 

            // 7.  Let dbMask = MGF(H, emLen - hLen - 1).
            //
            // 8.  Let DB = maskedDB \xor dbMask.
            mgf1XOR(db, hash, h); 

            // 9.  Set the leftmost 8 * emLen - emBits bits of the leftmost octet in DB
            //     to zero.
            db[0L] &= (0xFFUL >> (int)(uint(8L * emLen - emBits)));

            if (sLen == PSSSaltLengthAuto)
            {
FindSaltLength:
                for (sLen = emLen - (hLen + 2L); sLen >= 0L; sLen--)
                {
                    switch (db[emLen - hLen - sLen - 2L])
                    {
                        case 1L: 
                            _breakFindSaltLength = true;
                            break;
                            break;
                        case 0L: 
                            continue;
                            break;
                        default: 
                            return error.As(ErrVerification);
                            break;
                    }
                }
            else
                if (sLen < 0L)
                {
                    return error.As(ErrVerification);
                }
            }            { 
                // 10. If the emLen - hLen - sLen - 2 leftmost octets of DB are not zero
                //     or if the octet at position emLen - hLen - sLen - 1 (the leftmost
                //     position is "position 1") does not have hexadecimal value 0x01,
                //     output "inconsistent" and stop.
                foreach (var (_, e) in db[..emLen - hLen - sLen - 2L])
                {
                    if (e != 0x00UL)
                    {
                        return error.As(ErrVerification);
                    }
                }
                if (db[emLen - hLen - sLen - 2L] != 0x01UL)
                {
                    return error.As(ErrVerification);
                }
            } 

            // 11.  Let salt be the last sLen octets of DB.
            var salt = db[len(db) - sLen..]; 

            // 12.  Let
            //          M' = (0x)00 00 00 00 00 00 00 00 || mHash || salt ;
            //     M' is an octet string of length 8 + hLen + sLen with eight
            //     initial zero octets.
            //
            // 13. Let H' = Hash(M'), an octet string of length hLen.
            array<byte> prefix = new array<byte>(8L);
            hash.Write(prefix[..]);
            hash.Write(mHash);
            hash.Write(salt);

            var h0 = hash.Sum(null); 

            // 14. If H = H', output "consistent." Otherwise, output "inconsistent."
            if (!bytes.Equal(h0, h))
            {
                return error.As(ErrVerification);
            }
            return error.As(null);
        }

        // signPSSWithSalt calculates the signature of hashed using PSS [1] with specified salt.
        // Note that hashed must be the result of hashing the input message using the
        // given hash function. salt is a random sequence of bytes whose length will be
        // later used to verify the signature.
        private static (slice<byte>, error) signPSSWithSalt(io.Reader rand, ref PrivateKey priv, crypto.Hash hash, slice<byte> hashed, slice<byte> salt)
        {
            var nBits = priv.N.BitLen();
            var (em, err) = emsaPSSEncode(hashed, nBits - 1L, salt, hash.New());
            if (err != null)
            {
                return;
            }
            ptr<object> m = @new<big.Int>().SetBytes(em);
            var (c, err) = decryptAndCheck(rand, priv, m);
            if (err != null)
            {
                return;
            }
            s = make_slice<byte>((nBits + 7L) / 8L);
            copyWithLeftPad(s, c.Bytes());
            return;
        }

 
        // PSSSaltLengthAuto causes the salt in a PSS signature to be as large
        // as possible when signing, and to be auto-detected when verifying.
        public static readonly long PSSSaltLengthAuto = 0L; 
        // PSSSaltLengthEqualsHash causes the salt length to equal the length
        // of the hash used in the signature.
        public static readonly long PSSSaltLengthEqualsHash = -1L;

        // PSSOptions contains options for creating and verifying PSS signatures.
        public partial struct PSSOptions
        {
            public long SaltLength; // Hash, if not zero, overrides the hash function passed to SignPSS.
// This is the only way to specify the hash function when using the
// crypto.Signer interface.
            public crypto.Hash Hash;
        }

        // HashFunc returns pssOpts.Hash so that PSSOptions implements
        // crypto.SignerOpts.
        private static crypto.Hash HashFunc(this ref PSSOptions pssOpts)
        {
            return pssOpts.Hash;
        }

        private static long saltLength(this ref PSSOptions opts)
        {
            if (opts == null)
            {
                return PSSSaltLengthAuto;
            }
            return opts.SaltLength;
        }

        // SignPSS calculates the signature of hashed using RSASSA-PSS [1].
        // Note that hashed must be the result of hashing the input message using the
        // given hash function. The opts argument may be nil, in which case sensible
        // defaults are used.
        public static (slice<byte>, error) SignPSS(io.Reader rand, ref PrivateKey priv, crypto.Hash hash, slice<byte> hashed, ref PSSOptions opts)
        {
            var saltLength = opts.saltLength();

            if (saltLength == PSSSaltLengthAuto) 
                saltLength = (priv.N.BitLen() + 7L) / 8L - 2L - hash.Size();
            else if (saltLength == PSSSaltLengthEqualsHash) 
                saltLength = hash.Size();
                        if (opts != null && opts.Hash != 0L)
            {
                hash = opts.Hash;
            }
            var salt = make_slice<byte>(saltLength);
            {
                var (_, err) = io.ReadFull(rand, salt);

                if (err != null)
                {
                    return (null, err);
                }

            }
            return signPSSWithSalt(rand, priv, hash, hashed, salt);
        }

        // VerifyPSS verifies a PSS signature.
        // hashed is the result of hashing the input message using the given hash
        // function and sig is the signature. A valid signature is indicated by
        // returning a nil error. The opts argument may be nil, in which case sensible
        // defaults are used.
        public static error VerifyPSS(ref PublicKey pub, crypto.Hash hash, slice<byte> hashed, slice<byte> sig, ref PSSOptions opts)
        {
            return error.As(verifyPSS(pub, hash, hashed, sig, opts.saltLength()));
        }

        // verifyPSS verifies a PSS signature with the given salt length.
        private static error verifyPSS(ref PublicKey pub, crypto.Hash hash, slice<byte> hashed, slice<byte> sig, long saltLen)
        {
            var nBits = pub.N.BitLen();
            if (len(sig) != (nBits + 7L) / 8L)
            {
                return error.As(ErrVerification);
            }
            ptr<object> s = @new<big.Int>().SetBytes(sig);
            var m = encrypt(@new<big.Int>(), pub, s);
            var emBits = nBits - 1L;
            var emLen = (emBits + 7L) / 8L;
            if (emLen < len(m.Bytes()))
            {
                return error.As(ErrVerification);
            }
            var em = make_slice<byte>(emLen);
            copyWithLeftPad(em, m.Bytes());
            if (saltLen == PSSSaltLengthEqualsHash)
            {
                saltLen = hash.Size();
            }
            return error.As(emsaPSSVerify(hashed, em, emBits, saltLen, hash.New()));
        }
    }
}}
