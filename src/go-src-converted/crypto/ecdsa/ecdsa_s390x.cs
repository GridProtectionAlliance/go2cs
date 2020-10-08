// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ecdsa -- go2cs converted at 2020 October 08 03:36:34 UTC
// import "crypto/ecdsa" ==> using ecdsa = go.crypto.ecdsa_package
// Original source: C:\Go\src\crypto\ecdsa\ecdsa_s390x.go
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using cpu = go.@internal.cpu_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class ecdsa_package
    {
        // kdsa invokes the "compute digital signature authentication"
        // instruction with the given function code and 4096 byte
        // parameter block.
        //
        // The return value corresponds to the condition code set by the
        // instruction. Interrupted invocations are handled by the
        // function.
        //go:noescape
        private static ulong kdsa(ulong fc, ptr<array<byte>> @params)
;

        // canUseKDSA checks if KDSA instruction is available, and if it is, it checks
        // the name of the curve to see if it matches the curves supported(P-256, P-384, P-521).
        // Then, based on the curve name, a function code and a block size will be assigned.
        // If KDSA instruction is not available or if the curve is not supported, canUseKDSA
        // will set ok to false.
        private static (ulong, long, bool) canUseKDSA(elliptic.Curve c)
        {
            ulong functionCode = default;
            long blockSize = default;
            bool ok = default;

            if (!cpu.S390X.HasECDSA)
            {>>MARKER:FUNCTION_kdsa_BLOCK_PREFIX<<
                return (0L, 0L, false);
            }

            switch (c.Params().Name)
            {
                case "P-256": 
                    return (1L, 32L, true);
                    break;
                case "P-384": 
                    return (2L, 48L, true);
                    break;
                case "P-521": 
                    return (3L, 80L, true);
                    break;
            }
            return (0L, 0L, false); // A mismatch
        }

        // zeroExtendAndCopy pads src with leading zeros until it has the size given.
        // It then copies the padded src into the dst. Bytes beyond size in dst are
        // not modified.
        private static void zeroExtendAndCopy(slice<byte> dst, slice<byte> src, long size) => func((_, panic, __) =>
        {
            var nz = size - len(src);
            if (nz < 0L)
            {
                panic("src is too long");
            } 
            // the compiler should replace this loop with a memclr call
            var z = dst[..nz];
            foreach (var (i) in z)
            {
                z[i] = 0L;
            }
            copy(dst[nz..size], src[..size - nz]);
            return ;

        });

        private static (ptr<big.Int>, ptr<big.Int>, error) sign(ptr<PrivateKey> _addr_priv, ptr<cipher.StreamReader> _addr_csprng, elliptic.Curve c, slice<byte> hash) => func((_, panic, __) =>
        {
            ptr<big.Int> r = default!;
            ptr<big.Int> s = default!;
            error err = default!;
            ref PrivateKey priv = ref _addr_priv.val;
            ref cipher.StreamReader csprng = ref _addr_csprng.val;

            {
                var (functionCode, blockSize, ok) = canUseKDSA(c);

                if (ok)
                {
                    var e = hashToInt(hash, c);
                    while (true)
                    {
                        ptr<big.Int> k;
                        k, err = randFieldElement(c, csprng);
                        if (err != null)
                        {
                            return (_addr_null!, _addr_null!, error.As(err)!);
                        } 

                        // The parameter block looks like the following for sign.
                        //     +---------------------+
                        //     |   Signature(R)      |
                        //    +---------------------+
                        //    |   Signature(S)      |
                        //    +---------------------+
                        //    |   Hashed Message    |
                        //    +---------------------+
                        //    |   Private Key       |
                        //    +---------------------+
                        //    |   Random Number     |
                        //    +---------------------+
                        //    |                     |
                        //    |        ...          |
                        //    |                     |
                        //    +---------------------+
                        // The common components(signatureR, signatureS, hashedMessage, privateKey and
                        // random number) each takes block size of bytes. The block size is different for
                        // different curves and is set by canUseKDSA function.
                        ref array<byte> @params = ref heap(new array<byte>(4096L), out ptr<array<byte>> _addr_@params);

                        long startingOffset = 2L * blockSize; // Set the starting location for copying
                        // Copy content into the parameter block. In the sign case,
                        // we copy hashed message, private key and random number into
                        // the parameter block. Since those are consecutive components in the parameter
                        // block, we use a for loop here.
                        foreach (var (i, v) in new slice<ptr<big.Int>>(new ptr<big.Int>[] { e, priv.D, k }))
                        {
                            var startPosition = startingOffset + i * blockSize;
                            var endPosition = startPosition + blockSize;
                            zeroExtendAndCopy(params[startPosition..endPosition], v.Bytes(), blockSize);
                        } 

                        // Convert verify function code into a sign function code by adding 8.
                        // We also need to set the 'deterministic' bit in the function code, by
                        // adding 128, in order to stop the instruction using its own random number
                        // generator in addition to the random number we supply.
                        switch (kdsa(functionCode + 136L, _addr_params))
                        {
                            case 0L: // success
                                r = @new<big.Int>();
                                r.SetBytes(params[..blockSize]);
                                s = @new<big.Int>();
                                s.SetBytes(params[blockSize..2L * blockSize]);
                                return ;
                                break;
                            case 1L: // error
                                return (_addr_null!, _addr_null!, error.As(errZeroParam)!);
                                break;
                            case 2L: // retry
                                continue;
                                break;
                        }
                        panic("unreachable");

                    }


                }

            }

            return _addr_signGeneric(priv, csprng, c, hash)!;

        });

        private static bool verify(ptr<PublicKey> _addr_pub, elliptic.Curve c, slice<byte> hash, ptr<big.Int> _addr_r, ptr<big.Int> _addr_s)
        {
            ref PublicKey pub = ref _addr_pub.val;
            ref big.Int r = ref _addr_r.val;
            ref big.Int s = ref _addr_s.val;

            {
                var (functionCode, blockSize, ok) = canUseKDSA(c);

                if (ok)
                {
                    var e = hashToInt(hash, c); 
                    // The parameter block looks like the following for verify:
                    //     +---------------------+
                    //     |   Signature(R)      |
                    //    +---------------------+
                    //    |   Signature(S)      |
                    //    +---------------------+
                    //    |   Hashed Message    |
                    //    +---------------------+
                    //    |   Public Key X      |
                    //    +---------------------+
                    //    |   Public Key Y      |
                    //    +---------------------+
                    //    |                     |
                    //    |        ...          |
                    //    |                     |
                    //    +---------------------+
                    // The common components(signatureR, signatureS, hashed message, public key X,
                    // and public key Y) each takes block size of bytes. The block size is different for
                    // different curves and is set by canUseKDSA function.
                    ref array<byte> @params = ref heap(new array<byte>(4096L), out ptr<array<byte>> _addr_@params); 

                    // Copy content into the parameter block. In the verify case,
                    // we copy signature (r), signature(s), hashed message, public key x component,
                    // and public key y component into the parameter block.
                    // Since those are consecutive components in the parameter block, we use a for loop here.
                    foreach (var (i, v) in new slice<ptr<big.Int>>(new ptr<big.Int>[] { r, s, e, pub.X, pub.Y }))
                    {
                        var startPosition = i * blockSize;
                        var endPosition = startPosition + blockSize;
                        zeroExtendAndCopy(params[startPosition..endPosition], v.Bytes(), blockSize);
                    }
                    return kdsa(functionCode, _addr_params) == 0L;

                }

            }

            return verifyGeneric(pub, c, hash, r, s);

        }
    }
}}
