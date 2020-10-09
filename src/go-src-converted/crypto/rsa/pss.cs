// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rsa -- go2cs converted at 2020 October 09 04:53:51 UTC
// import "crypto/rsa" ==> using rsa = go.crypto.rsa_package
// Original source: C:\Go\src\crypto\rsa\pss.go
// This file implements the RSASSA-PSS signature scheme according to RFC 8017.

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
        // Per RFC 8017, Section 9.1
        //
        //     EM = MGF1 xor DB || H( 8*0x00 || mHash || salt ) || 0xbc
        //
        // where
        //
        //     DB = PS || 0x01 || salt
        //
        // and PS can be empty so
        //
        //     emLen = dbLen + hLen + 1 = psLen + sLen + hLen + 2
        //
        private static (slice<byte>, error) emsaPSSEncode(slice<byte> mHash, long emBits, slice<byte> salt, hash.Hash hash)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
 
            // See RFC 8017, Section 9.1.1.

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
                return (null, error.As(errors.New("crypto/rsa: input must be hashed with given hash"))!);
            }
            if (emLen < hLen + sLen + 2L)
            {
                return (null, error.As(errors.New("crypto/rsa: key size too small for PSS signature"))!);
            }
            var em = make_slice<byte>(emLen);
            var psLen = emLen - sLen - hLen - 2L;
            var db = em[..psLen + 1L + sLen];
            var h = em[psLen + 1L + sLen..emLen - 1L]; 

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

            db[psLen] = 0x01UL;
            copy(db[psLen + 1L..], salt); 

            // 9.  Let dbMask = MGF(H, emLen - hLen - 1).
            //
            // 10. Let maskedDB = DB \xor dbMask.

            mgf1XOR(db, hash, h); 

            // 11. Set the leftmost 8 * emLen - emBits bits of the leftmost octet in
            //     maskedDB to zero.

            db[0L] &= 0xffUL >> (int)((8L * emLen - emBits)); 

            // 12. Let EM = maskedDB || H || 0xbc.
            em[emLen - 1L] = 0xbcUL; 

            // 13. Output EM.
            return (em, error.As(null!)!);

        }

        private static error emsaPSSVerify(slice<byte> mHash, slice<byte> em, long emBits, long sLen, hash.Hash hash)
        { 
            // See RFC 8017, Section 9.1.2.

            var hLen = hash.Size();
            if (sLen == PSSSaltLengthEqualsHash)
            {
                sLen = hLen;
            }

            var emLen = (emBits + 7L) / 8L;
            if (emLen != len(em))
            {
                return error.As(errors.New("rsa: internal error: inconsistent length"))!;
            } 

            // 1.  If the length of M is greater than the input limitation for the
            //     hash function (2^61 - 1 octets for SHA-1), output "inconsistent"
            //     and stop.
            //
            // 2.  Let mHash = Hash(M), an octet string of length hLen.
            if (hLen != len(mHash))
            {
                return error.As(ErrVerification)!;
            } 

            // 3.  If emLen < hLen + sLen + 2, output "inconsistent" and stop.
            if (emLen < hLen + sLen + 2L)
            {
                return error.As(ErrVerification)!;
            } 

            // 4.  If the rightmost octet of EM does not have hexadecimal value
            //     0xbc, output "inconsistent" and stop.
            if (em[emLen - 1L] != 0xbcUL)
            {
                return error.As(ErrVerification)!;
            } 

            // 5.  Let maskedDB be the leftmost emLen - hLen - 1 octets of EM, and
            //     let H be the next hLen octets.
            var db = em[..emLen - hLen - 1L];
            var h = em[emLen - hLen - 1L..emLen - 1L]; 

            // 6.  If the leftmost 8 * emLen - emBits bits of the leftmost octet in
            //     maskedDB are not all equal to zero, output "inconsistent" and
            //     stop.
            byte bitMask = 0xffUL >> (int)((8L * emLen - emBits));
            if (em[0L] & ~bitMask != 0L)
            {
                return error.As(ErrVerification)!;
            } 

            // 7.  Let dbMask = MGF(H, emLen - hLen - 1).
            //
            // 8.  Let DB = maskedDB \xor dbMask.
            mgf1XOR(db, hash, h); 

            // 9.  Set the leftmost 8 * emLen - emBits bits of the leftmost octet in DB
            //     to zero.
            db[0L] &= bitMask; 

            // If we don't know the salt length, look for the 0x01 delimiter.
            if (sLen == PSSSaltLengthAuto)
            {
                var psLen = bytes.IndexByte(db, 0x01UL);
                if (psLen < 0L)
                {
                    return error.As(ErrVerification)!;
                }

                sLen = len(db) - psLen - 1L;

            } 

            // 10. If the emLen - hLen - sLen - 2 leftmost octets of DB are not zero
            //     or if the octet at position emLen - hLen - sLen - 1 (the leftmost
            //     position is "position 1") does not have hexadecimal value 0x01,
            //     output "inconsistent" and stop.
            psLen = emLen - hLen - sLen - 2L;
            foreach (var (_, e) in db[..psLen])
            {
                if (e != 0x00UL)
                {
                    return error.As(ErrVerification)!;
                }

            }
            if (db[psLen] != 0x01UL)
            {
                return error.As(ErrVerification)!;
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
            { // TODO: constant time?
                return error.As(ErrVerification)!;

            }

            return error.As(null!)!;

        }

        // signPSSWithSalt calculates the signature of hashed using PSS with specified salt.
        // Note that hashed must be the result of hashing the input message using the
        // given hash function. salt is a random sequence of bytes whose length will be
        // later used to verify the signature.
        private static (slice<byte>, error) signPSSWithSalt(io.Reader rand, ptr<PrivateKey> _addr_priv, crypto.Hash hash, slice<byte> hashed, slice<byte> salt)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref PrivateKey priv = ref _addr_priv.val;

            var emBits = priv.N.BitLen() - 1L;
            var (em, err) = emsaPSSEncode(hashed, emBits, salt, hash.New());
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            ptr<object> m = @new<big.Int>().SetBytes(em);
            var (c, err) = decryptAndCheck(rand, priv, m);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var s = make_slice<byte>(priv.Size());
            return (c.FillBytes(s), error.As(null!)!);

        }

 
        // PSSSaltLengthAuto causes the salt in a PSS signature to be as large
        // as possible when signing, and to be auto-detected when verifying.
        public static readonly long PSSSaltLengthAuto = (long)0L; 
        // PSSSaltLengthEqualsHash causes the salt length to equal the length
        // of the hash used in the signature.
        public static readonly long PSSSaltLengthEqualsHash = (long)-1L;


        // PSSOptions contains options for creating and verifying PSS signatures.
        public partial struct PSSOptions
        {
            public long SaltLength; // Hash is the hash function used to generate the message digest. If not
// zero, it overrides the hash function passed to SignPSS. It's required
// when using PrivateKey.Sign.
            public crypto.Hash Hash;
        }

        // HashFunc returns opts.Hash so that PSSOptions implements crypto.SignerOpts.
        private static crypto.Hash HashFunc(this ptr<PSSOptions> _addr_opts)
        {
            ref PSSOptions opts = ref _addr_opts.val;

            return opts.Hash;
        }

        private static long saltLength(this ptr<PSSOptions> _addr_opts)
        {
            ref PSSOptions opts = ref _addr_opts.val;

            if (opts == null)
            {
                return PSSSaltLengthAuto;
            }

            return opts.SaltLength;

        }

        // SignPSS calculates the signature of digest using PSS.
        //
        // digest must be the result of hashing the input message using the given hash
        // function. The opts argument may be nil, in which case sensible defaults are
        // used. If opts.Hash is set, it overrides hash.
        public static (slice<byte>, error) SignPSS(io.Reader rand, ptr<PrivateKey> _addr_priv, crypto.Hash hash, slice<byte> digest, ptr<PSSOptions> _addr_opts)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref PrivateKey priv = ref _addr_priv.val;
            ref PSSOptions opts = ref _addr_opts.val;

            if (opts != null && opts.Hash != 0L)
            {
                hash = opts.Hash;
            }

            var saltLength = opts.saltLength();

            if (saltLength == PSSSaltLengthAuto) 
                saltLength = priv.Size() - 2L - hash.Size();
            else if (saltLength == PSSSaltLengthEqualsHash) 
                saltLength = hash.Size();
                        var salt = make_slice<byte>(saltLength);
            {
                var (_, err) = io.ReadFull(rand, salt);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            return signPSSWithSalt(rand, _addr_priv, hash, digest, salt);

        }

        // VerifyPSS verifies a PSS signature.
        //
        // A valid signature is indicated by returning a nil error. digest must be the
        // result of hashing the input message using the given hash function. The opts
        // argument may be nil, in which case sensible defaults are used. opts.Hash is
        // ignored.
        public static error VerifyPSS(ptr<PublicKey> _addr_pub, crypto.Hash hash, slice<byte> digest, slice<byte> sig, ptr<PSSOptions> _addr_opts)
        {
            ref PublicKey pub = ref _addr_pub.val;
            ref PSSOptions opts = ref _addr_opts.val;

            if (len(sig) != pub.Size())
            {
                return error.As(ErrVerification)!;
            }

            ptr<object> s = @new<big.Int>().SetBytes(sig);
            var m = encrypt(@new<big.Int>(), pub, s);
            var emBits = pub.N.BitLen() - 1L;
            var emLen = (emBits + 7L) / 8L;
            if (m.BitLen() > emLen * 8L)
            {
                return error.As(ErrVerification)!;
            }

            var em = m.FillBytes(make_slice<byte>(emLen));
            return error.As(emsaPSSVerify(digest, em, emBits, opts.saltLength(), hash.New()))!;

        }
    }
}}
