// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package elliptic implements several standard elliptic curves over prime
// fields.
// package elliptic -- go2cs converted at 2020 August 29 08:29:39 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\elliptic.go
// This package operates, internally, on Jacobian coordinates. For a given
// (x, y) position on the curve, the Jacobian coordinates are (x1, y1, z1)
// where x = x1/z1² and y = y1/z1³. The greatest speedups come when the whole
// calculation can be performed within the transform (as in ScalarMult and
// ScalarBaseMult). But even for Add and Double, it's faster to apply and
// reverse the transform than to operate in affine coordinates.

using io = go.io_package;
using big = go.math.big_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        // A Curve represents a short-form Weierstrass curve with a=-3.
        // See http://www.hyperelliptic.org/EFD/g1p/auto-shortw.html
        public partial interface Curve
        {
            (ref big.Int, ref big.Int) Params(); // IsOnCurve reports whether the given (x,y) lies on the curve.
            (ref big.Int, ref big.Int) IsOnCurve(ref big.Int x, ref big.Int y); // Add returns the sum of (x1,y1) and (x2,y2)
            (ref big.Int, ref big.Int) Add(ref big.Int x1, ref big.Int y1, ref big.Int x2, ref big.Int y2); // Double returns 2*(x,y)
            (ref big.Int, ref big.Int) Double(ref big.Int x1, ref big.Int y1); // ScalarMult returns k*(Bx,By) where k is a number in big-endian form.
            (ref big.Int, ref big.Int) ScalarMult(ref big.Int x1, ref big.Int y1, slice<byte> k); // ScalarBaseMult returns k*G, where G is the base point of the group
// and k is an integer in big-endian form.
            (ref big.Int, ref big.Int) ScalarBaseMult(slice<byte> k);
        }

        // CurveParams contains the parameters of an elliptic curve and also provides
        // a generic, non-constant time implementation of Curve.
        public partial struct CurveParams
        {
            public ptr<big.Int> P; // the order of the underlying field
            public ptr<big.Int> N; // the order of the base point
            public ptr<big.Int> B; // the constant of the curve equation
            public ptr<big.Int> Gx; // (x,y) of the base point
            public ptr<big.Int> Gy; // (x,y) of the base point
            public long BitSize; // the size of the underlying field
            public @string Name; // the canonical name of the curve
        }

        private static ref CurveParams Params(this ref CurveParams curve)
        {
            return curve;
        }

        private static bool IsOnCurve(this ref CurveParams curve, ref big.Int x, ref big.Int y)
        { 
            // y² = x³ - 3x + b
            ptr<big.Int> y2 = @new<big.Int>().Mul(y, y);
            y2.Mod(y2, curve.P);

            ptr<big.Int> x3 = @new<big.Int>().Mul(x, x);
            x3.Mul(x3, x);

            ptr<big.Int> threeX = @new<big.Int>().Lsh(x, 1L);
            threeX.Add(threeX, x);

            x3.Sub(x3, threeX);
            x3.Add(x3, curve.B);
            x3.Mod(x3, curve.P);

            return x3.Cmp(y2) == 0L;
        }

        // zForAffine returns a Jacobian Z value for the affine point (x, y). If x and
        // y are zero, it assumes that they represent the point at infinity because (0,
        // 0) is not on the any of the curves handled here.
        private static ref big.Int zForAffine(ref big.Int x, ref big.Int y)
        {
            ptr<big.Int> z = @new<big.Int>();
            if (x.Sign() != 0L || y.Sign() != 0L)
            {
                z.SetInt64(1L);
            }
            return z;
        }

        // affineFromJacobian reverses the Jacobian transform. See the comment at the
        // top of the file. If the point is ∞ it returns 0, 0.
        private static (ref big.Int, ref big.Int) affineFromJacobian(this ref CurveParams curve, ref big.Int x, ref big.Int y, ref big.Int z)
        {
            if (z.Sign() == 0L)
            {
                return (@new<big.Int>(), @new<big.Int>());
            }
            ptr<big.Int> zinv = @new<big.Int>().ModInverse(z, curve.P);
            ptr<big.Int> zinvsq = @new<big.Int>().Mul(zinv, zinv);

            xOut = @new<big.Int>().Mul(x, zinvsq);
            xOut.Mod(xOut, curve.P);
            zinvsq.Mul(zinvsq, zinv);
            yOut = @new<big.Int>().Mul(y, zinvsq);
            yOut.Mod(yOut, curve.P);
            return;
        }

        private static (ref big.Int, ref big.Int) Add(this ref CurveParams curve, ref big.Int x1, ref big.Int y1, ref big.Int x2, ref big.Int y2)
        {
            var z1 = zForAffine(x1, y1);
            var z2 = zForAffine(x2, y2);
            return curve.affineFromJacobian(curve.addJacobian(x1, y1, z1, x2, y2, z2));
        }

        // addJacobian takes two points in Jacobian coordinates, (x1, y1, z1) and
        // (x2, y2, z2) and returns their sum, also in Jacobian form.
        private static (ref big.Int, ref big.Int, ref big.Int) addJacobian(this ref CurveParams curve, ref big.Int x1, ref big.Int y1, ref big.Int z1, ref big.Int x2, ref big.Int y2, ref big.Int z2)
        { 
            // See http://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#addition-add-2007-bl
            ptr<big.Int> x3 = @new<big.Int>();
            ptr<big.Int> y3 = @new<big.Int>();
            ptr<big.Int> z3 = @new<big.Int>();
            if (z1.Sign() == 0L)
            {
                x3.Set(x2);
                y3.Set(y2);
                z3.Set(z2);
                return (x3, y3, z3);
            }
            if (z2.Sign() == 0L)
            {
                x3.Set(x1);
                y3.Set(y1);
                z3.Set(z1);
                return (x3, y3, z3);
            }
            ptr<big.Int> z1z1 = @new<big.Int>().Mul(z1, z1);
            z1z1.Mod(z1z1, curve.P);
            ptr<big.Int> z2z2 = @new<big.Int>().Mul(z2, z2);
            z2z2.Mod(z2z2, curve.P);

            ptr<big.Int> u1 = @new<big.Int>().Mul(x1, z2z2);
            u1.Mod(u1, curve.P);
            ptr<big.Int> u2 = @new<big.Int>().Mul(x2, z1z1);
            u2.Mod(u2, curve.P);
            ptr<big.Int> h = @new<big.Int>().Sub(u2, u1);
            var xEqual = h.Sign() == 0L;
            if (h.Sign() == -1L)
            {
                h.Add(h, curve.P);
            }
            ptr<big.Int> i = @new<big.Int>().Lsh(h, 1L);
            i.Mul(i, i);
            ptr<big.Int> j = @new<big.Int>().Mul(h, i);

            ptr<big.Int> s1 = @new<big.Int>().Mul(y1, z2);
            s1.Mul(s1, z2z2);
            s1.Mod(s1, curve.P);
            ptr<big.Int> s2 = @new<big.Int>().Mul(y2, z1);
            s2.Mul(s2, z1z1);
            s2.Mod(s2, curve.P);
            ptr<big.Int> r = @new<big.Int>().Sub(s2, s1);
            if (r.Sign() == -1L)
            {
                r.Add(r, curve.P);
            }
            var yEqual = r.Sign() == 0L;
            if (xEqual && yEqual)
            {
                return curve.doubleJacobian(x1, y1, z1);
            }
            r.Lsh(r, 1L);
            ptr<big.Int> v = @new<big.Int>().Mul(u1, i);

            x3.Set(r);
            x3.Mul(x3, x3);
            x3.Sub(x3, j);
            x3.Sub(x3, v);
            x3.Sub(x3, v);
            x3.Mod(x3, curve.P);

            y3.Set(r);
            v.Sub(v, x3);
            y3.Mul(y3, v);
            s1.Mul(s1, j);
            s1.Lsh(s1, 1L);
            y3.Sub(y3, s1);
            y3.Mod(y3, curve.P);

            z3.Add(z1, z2);
            z3.Mul(z3, z3);
            z3.Sub(z3, z1z1);
            z3.Sub(z3, z2z2);
            z3.Mul(z3, h);
            z3.Mod(z3, curve.P);

            return (x3, y3, z3);
        }

        private static (ref big.Int, ref big.Int) Double(this ref CurveParams curve, ref big.Int x1, ref big.Int y1)
        {
            var z1 = zForAffine(x1, y1);
            return curve.affineFromJacobian(curve.doubleJacobian(x1, y1, z1));
        }

        // doubleJacobian takes a point in Jacobian coordinates, (x, y, z), and
        // returns its double, also in Jacobian form.
        private static (ref big.Int, ref big.Int, ref big.Int) doubleJacobian(this ref CurveParams curve, ref big.Int x, ref big.Int y, ref big.Int z)
        { 
            // See http://hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-3.html#doubling-dbl-2001-b
            ptr<big.Int> delta = @new<big.Int>().Mul(z, z);
            delta.Mod(delta, curve.P);
            ptr<big.Int> gamma = @new<big.Int>().Mul(y, y);
            gamma.Mod(gamma, curve.P);
            ptr<big.Int> alpha = @new<big.Int>().Sub(x, delta);
            if (alpha.Sign() == -1L)
            {
                alpha.Add(alpha, curve.P);
            }
            ptr<big.Int> alpha2 = @new<big.Int>().Add(x, delta);
            alpha.Mul(alpha, alpha2);
            alpha2.Set(alpha);
            alpha.Lsh(alpha, 1L);
            alpha.Add(alpha, alpha2);

            var beta = alpha2.Mul(x, gamma);

            ptr<big.Int> x3 = @new<big.Int>().Mul(alpha, alpha);
            ptr<big.Int> beta8 = @new<big.Int>().Lsh(beta, 3L);
            x3.Sub(x3, beta8);
            while (x3.Sign() == -1L)
            {
                x3.Add(x3, curve.P);
            }

            x3.Mod(x3, curve.P);

            ptr<big.Int> z3 = @new<big.Int>().Add(y, z);
            z3.Mul(z3, z3);
            z3.Sub(z3, gamma);
            if (z3.Sign() == -1L)
            {
                z3.Add(z3, curve.P);
            }
            z3.Sub(z3, delta);
            if (z3.Sign() == -1L)
            {
                z3.Add(z3, curve.P);
            }
            z3.Mod(z3, curve.P);

            beta.Lsh(beta, 2L);
            beta.Sub(beta, x3);
            if (beta.Sign() == -1L)
            {
                beta.Add(beta, curve.P);
            }
            var y3 = alpha.Mul(alpha, beta);

            gamma.Mul(gamma, gamma);
            gamma.Lsh(gamma, 3L);
            gamma.Mod(gamma, curve.P);

            y3.Sub(y3, gamma);
            if (y3.Sign() == -1L)
            {
                y3.Add(y3, curve.P);
            }
            y3.Mod(y3, curve.P);

            return (x3, y3, z3);
        }

        private static (ref big.Int, ref big.Int) ScalarMult(this ref CurveParams curve, ref big.Int Bx, ref big.Int By, slice<byte> k)
        {
            ptr<big.Int> Bz = @new<big.Int>().SetInt64(1L);
            ptr<big.Int> x = @new<big.Int>();
            ptr<big.Int> y = @new<big.Int>();
            ptr<big.Int> z = @new<big.Int>();

            foreach (var (_, byte) in k)
            {
                for (long bitNum = 0L; bitNum < 8L; bitNum++)
                {
                    x, y, z = curve.doubleJacobian(x, y, z);
                    if (byte & 0x80UL == 0x80UL)
                    {
                        x, y, z = curve.addJacobian(Bx, By, Bz, x, y, z);
                    }
                    byte <<= 1L;
                }

            }
            return curve.affineFromJacobian(x, y, z);
        }

        private static (ref big.Int, ref big.Int) ScalarBaseMult(this ref CurveParams curve, slice<byte> k)
        {
            return curve.ScalarMult(curve.Gx, curve.Gy, k);
        }

        private static byte mask = new slice<byte>(new byte[] { 0xff, 0x1, 0x3, 0x7, 0xf, 0x1f, 0x3f, 0x7f });

        // GenerateKey returns a public/private key pair. The private key is
        // generated using the given reader, which must return random data.
        public static (slice<byte>, ref big.Int, ref big.Int, error) GenerateKey(Curve curve, io.Reader rand)
        {
            var N = curve.Params().N;
            var bitSize = N.BitLen();
            var byteLen = (bitSize + 7L) >> (int)(3L);
            priv = make_slice<byte>(byteLen);

            while (x == null)
            {
                _, err = io.ReadFull(rand, priv);
                if (err != null)
                {
                    return;
                } 
                // We have to mask off any excess bits in the case that the size of the
                // underlying field is not a whole number of bytes.
                priv[0L] &= mask[bitSize % 8L]; 
                // This is because, in tests, rand will return all zeros and we don't
                // want to get the point at infinity and loop forever.
                priv[1L] ^= 0x42UL; 

                // If the scalar is out of range, sample another random number.
                if (@new<big.Int>().SetBytes(priv).Cmp(N) >= 0L)
                {
                    continue;
                }
                x, y = curve.ScalarBaseMult(priv);
            }

            return;
        }

        // Marshal converts a point into the uncompressed form specified in section 4.3.6 of ANSI X9.62.
        public static slice<byte> Marshal(Curve curve, ref big.Int x, ref big.Int y)
        {
            var byteLen = (curve.Params().BitSize + 7L) >> (int)(3L);

            var ret = make_slice<byte>(1L + 2L * byteLen);
            ret[0L] = 4L; // uncompressed point

            var xBytes = x.Bytes();
            copy(ret[1L + byteLen - len(xBytes)..], xBytes);
            var yBytes = y.Bytes();
            copy(ret[1L + 2L * byteLen - len(yBytes)..], yBytes);
            return ret;
        }

        // Unmarshal converts a point, serialized by Marshal, into an x, y pair.
        // It is an error if the point is not in uncompressed form or is not on the curve.
        // On error, x = nil.
        public static (ref big.Int, ref big.Int) Unmarshal(Curve curve, slice<byte> data)
        {
            var byteLen = (curve.Params().BitSize + 7L) >> (int)(3L);
            if (len(data) != 1L + 2L * byteLen)
            {
                return;
            }
            if (data[0L] != 4L)
            { // uncompressed form
                return;
            }
            var p = curve.Params().P;
            x = @new<big.Int>().SetBytes(data[1L..1L + byteLen]);
            y = @new<big.Int>().SetBytes(data[1L + byteLen..]);
            if (x.Cmp(p) >= 0L || y.Cmp(p) >= 0L)
            {
                return (null, null);
            }
            if (!curve.IsOnCurve(x, y))
            {
                return (null, null);
            }
            return;
        }

        private static sync.Once initonce = default;
        private static ref CurveParams p384 = default;
        private static ref CurveParams p521 = default;

        private static void initAll()
        {
            initP224();
            initP256();
            initP384();
            initP521();
        }

        private static void initP384()
        { 
            // See FIPS 186-3, section D.2.4
            p384 = ref new CurveParams(Name:"P-384");
            p384.P, _ = @new<big.Int>().SetString("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319", 10L);
            p384.N, _ = @new<big.Int>().SetString("39402006196394479212279040100143613805079739270465446667946905279627659399113263569398956308152294913554433653942643", 10L);
            p384.B, _ = @new<big.Int>().SetString("b3312fa7e23ee7e4988e056be3f82d19181d9c6efe8141120314088f5013875ac656398d8a2ed19d2a85c8edd3ec2aef", 16L);
            p384.Gx, _ = @new<big.Int>().SetString("aa87ca22be8b05378eb1c71ef320ad746e1d3b628ba79b9859f741e082542a385502f25dbf55296c3a545e3872760ab7", 16L);
            p384.Gy, _ = @new<big.Int>().SetString("3617de4a96262c6f5d9e98bf9292dc29f8f41dbd289a147ce9da3113b5f0b8c00a60b1ce1d7e819d7a431d7c90ea0e5f", 16L);
            p384.BitSize = 384L;
        }

        private static void initP521()
        { 
            // See FIPS 186-3, section D.2.5
            p521 = ref new CurveParams(Name:"P-521");
            p521.P, _ = @new<big.Int>().SetString("6864797660130609714981900799081393217269435300143305409394463459185543183397656052122559640661454554977296311391480858037121987999716643812574028291115057151", 10L);
            p521.N, _ = @new<big.Int>().SetString("6864797660130609714981900799081393217269435300143305409394463459185543183397655394245057746333217197532963996371363321113864768612440380340372808892707005449", 10L);
            p521.B, _ = @new<big.Int>().SetString("051953eb9618e1c9a1f929a21a0b68540eea2da725b99b315f3b8b489918ef109e156193951ec7e937b1652c0bd3bb1bf073573df883d2c34f1ef451fd46b503f00", 16L);
            p521.Gx, _ = @new<big.Int>().SetString("c6858e06b70404e9cd9e3ecb662395b4429c648139053fb521f828af606b4d3dbaa14b5e77efe75928fe1dc127a2ffa8de3348b3c1856a429bf97e7e31c2e5bd66", 16L);
            p521.Gy, _ = @new<big.Int>().SetString("11839296a789a3bc0045c8a5fb42c7d1bd998f54449579b446817afbd17273e662c97ee72995ef42640c550b9013fad0761353c7086a272c24088be94769fd16650", 16L);
            p521.BitSize = 521L;
        }

        // P256 returns a Curve which implements P-256 (see FIPS 186-3, section D.2.3)
        //
        // The cryptographic operations are implemented using constant-time algorithms.
        public static Curve P256()
        {
            initonce.Do(initAll);
            return p256;
        }

        // P384 returns a Curve which implements P-384 (see FIPS 186-3, section D.2.4)
        //
        // The cryptographic operations do not use constant-time algorithms.
        public static Curve P384()
        {
            initonce.Do(initAll);
            return p384;
        }

        // P521 returns a Curve which implements P-521 (see FIPS 186-3, section D.2.5)
        //
        // The cryptographic operations do not use constant-time algorithms.
        public static Curve P521()
        {
            initonce.Do(initAll);
            return p521;
        }
    }
}}
