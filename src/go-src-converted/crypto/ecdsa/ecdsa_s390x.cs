// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ecdsa -- go2cs converted at 2022 March 06 22:19:17 UTC
// import "crypto/ecdsa" ==> using ecdsa = go.crypto.ecdsa_package
// Original source: C:\Program Files\Go\src\crypto\ecdsa\ecdsa_s390x.go
using cipher = go.crypto.cipher_package;
using elliptic = go.crypto.elliptic_package;
using cpu = go.@internal.cpu_package;
using big = go.math.big_package;

namespace go.crypto;

public static partial class ecdsa_package {

    // kdsa invokes the "compute digital signature authentication"
    // instruction with the given function code and 4096 byte
    // parameter block.
    //
    // The return value corresponds to the condition code set by the
    // instruction. Interrupted invocations are handled by the
    // function.
    //go:noescape
private static ulong kdsa(ulong fc, ptr<array<byte>> @params);

// testingDisableKDSA forces the generic fallback path. It must only be set in tests.
private static bool testingDisableKDSA = default;

// canUseKDSA checks if KDSA instruction is available, and if it is, it checks
// the name of the curve to see if it matches the curves supported(P-256, P-384, P-521).
// Then, based on the curve name, a function code and a block size will be assigned.
// If KDSA instruction is not available or if the curve is not supported, canUseKDSA
// will set ok to false.
private static (ulong, nint, bool) canUseKDSA(elliptic.Curve c) {
    ulong functionCode = default;
    nint blockSize = default;
    bool ok = default;

    if (testingDisableKDSA) {>>MARKER:FUNCTION_kdsa_BLOCK_PREFIX<<
        return (0, 0, false);
    }
    if (!cpu.S390X.HasECDSA) {
        return (0, 0, false);
    }
    switch (c.Params().Name) {
        case "P-256": 
            return (1, 32, true);
            break;
        case "P-384": 
            return (2, 48, true);
            break;
        case "P-521": 
            return (3, 80, true);
            break;
    }
    return (0, 0, false); // A mismatch
}

private static void hashToBytes(slice<byte> dst, slice<byte> hash, elliptic.Curve c) {
    var l = len(dst);
    {
        var n = c.Params().N.BitLen();

        if (n == l * 8) { 
            // allocation free path for curves with a length that is a whole number of bytes
            if (len(hash) >= l) { 
                // truncate hash
                copy(dst, hash[..(int)l]);
                return ;

            } 
            // pad hash with leading zeros
            var p = l - len(hash);
            for (nint i = 0; i < p; i++) {
                dst[i] = 0;
            }

            copy(dst[(int)p..], hash);
            return ;

        }
    } 
    // TODO(mundaym): avoid hashToInt call here
    hashToInt(hash, c).FillBytes(dst);

}

private static (ptr<big.Int>, ptr<big.Int>, error) sign(ptr<PrivateKey> _addr_priv, ptr<cipher.StreamReader> _addr_csprng, elliptic.Curve c, slice<byte> hash) => func((_, panic, _) => {
    ptr<big.Int> r = default!;
    ptr<big.Int> s = default!;
    error err = default!;
    ref PrivateKey priv = ref _addr_priv.val;
    ref cipher.StreamReader csprng = ref _addr_csprng.val;

    {
        var (functionCode, blockSize, ok) = canUseKDSA(c);

        if (ok) {
            while (true) {
                ptr<big.Int> k;
                k, err = randFieldElement(c, csprng);
                if (err != null) {
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
                ref array<byte> @params = ref heap(new array<byte>(4096), out ptr<array<byte>> _addr_@params); 

                // Copy content into the parameter block. In the sign case,
                // we copy hashed message, private key and random number into
                // the parameter block.
                hashToBytes(params[(int)2 * blockSize..(int)3 * blockSize], hash, c);
                priv.D.FillBytes(params[(int)3 * blockSize..(int)4 * blockSize]);
                k.FillBytes(params[(int)4 * blockSize..(int)5 * blockSize]); 
                // Convert verify function code into a sign function code by adding 8.
                // We also need to set the 'deterministic' bit in the function code, by
                // adding 128, in order to stop the instruction using its own random number
                // generator in addition to the random number we supply.
                switch (kdsa(functionCode + 136, _addr_params)) {
                    case 0: // success
                        r = @new<big.Int>();
                        r.SetBytes(params[..(int)blockSize]);
                        s = @new<big.Int>();
                        s.SetBytes(params[(int)blockSize..(int)2 * blockSize]);
                        return ;
                        break;
                    case 1: // error
                        return (_addr_null!, _addr_null!, error.As(errZeroParam)!);
                        break;
                    case 2: // retry
                        continue;
                        break;
                }
                panic("unreachable");

            }


        }
    }

    return _addr_signGeneric(priv, csprng, c, hash)!;

});

private static bool verify(ptr<PublicKey> _addr_pub, elliptic.Curve c, slice<byte> hash, ptr<big.Int> _addr_r, ptr<big.Int> _addr_s) {
    ref PublicKey pub = ref _addr_pub.val;
    ref big.Int r = ref _addr_r.val;
    ref big.Int s = ref _addr_s.val;

    {
        var (functionCode, blockSize, ok) = canUseKDSA(c);

        if (ok) { 
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
            ref array<byte> @params = ref heap(new array<byte>(4096), out ptr<array<byte>> _addr_@params); 

            // Copy content into the parameter block. In the verify case,
            // we copy signature (r), signature(s), hashed message, public key x component,
            // and public key y component into the parameter block.
            r.FillBytes(params[(int)0 * blockSize..(int)1 * blockSize]);
            s.FillBytes(params[(int)1 * blockSize..(int)2 * blockSize]);
            hashToBytes(params[(int)2 * blockSize..(int)3 * blockSize], hash, c);
            pub.X.FillBytes(params[(int)3 * blockSize..(int)4 * blockSize]);
            pub.Y.FillBytes(params[(int)4 * blockSize..(int)5 * blockSize]);
            return kdsa(functionCode, _addr_params) == 0;

        }
    }

    return verifyGeneric(pub, c, hash, r, s);

}

} // end ecdsa_package
