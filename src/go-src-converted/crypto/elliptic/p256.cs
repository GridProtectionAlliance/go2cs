// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64

// package elliptic -- go2cs converted at 2020 August 29 08:30:37 UTC
// import "crypto/elliptic" ==> using elliptic = go.crypto.elliptic_package
// Original source: C:\Go\src\crypto\elliptic\p256.go
// This file contains a constant-time, 32-bit implementation of P256.

using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class elliptic_package
    {
        private partial struct p256Curve
        {
            public ref CurveParams CurveParams => ref CurveParams_ptr;
        }

        private static ref CurveParams p256Params = default;        private static ref big.Int p256RInverse = default;

        private static void initP256()
        { 
            // See FIPS 186-3, section D.2.3
            p256Params = ref new CurveParams(Name:"P-256");
            p256Params.P, _ = @new<big.Int>().SetString("115792089210356248762697446949407573530086143415290314195533631308867097853951", 10L);
            p256Params.N, _ = @new<big.Int>().SetString("115792089210356248762697446949407573529996955224135760342422259061068512044369", 10L);
            p256Params.B, _ = @new<big.Int>().SetString("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b", 16L);
            p256Params.Gx, _ = @new<big.Int>().SetString("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296", 16L);
            p256Params.Gy, _ = @new<big.Int>().SetString("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5", 16L);
            p256Params.BitSize = 256L;

            p256RInverse, _ = @new<big.Int>().SetString("7fffffff00000001fffffffe8000000100000000ffffffff0000000180000000", 16L); 

            // Arch-specific initialization, i.e. let a platform dynamically pick a P256 implementation
            initP256Arch();
        }

        private static ref CurveParams Params(this p256Curve curve)
        {
            return curve.CurveParams;
        }

        // p256GetScalar endian-swaps the big-endian scalar value from in and writes it
        // to out. If the scalar is equal or greater than the order of the group, it's
        // reduced modulo that order.
        private static void p256GetScalar(ref array<byte> @out, slice<byte> @in)
        {
            ptr<big.Int> n = @new<big.Int>().SetBytes(in);
            slice<byte> scalarBytes = default;

            if (n.Cmp(p256Params.N) >= 0L)
            {
                n.Mod(n, p256Params.N);
                scalarBytes = n.Bytes();
            }
            else
            {
                scalarBytes = in;
            }
            foreach (var (i, v) in scalarBytes)
            {
                out[len(scalarBytes) - (1L + i)] = v;
            }
        }

        private static (ref big.Int, ref big.Int) ScalarBaseMult(this p256Curve _p0, slice<byte> scalar)
        {
            array<byte> scalarReversed = new array<byte>(32L);
            p256GetScalar(ref scalarReversed, scalar);

            array<uint> x1 = new array<uint>(p256Limbs);            array<uint> y1 = new array<uint>(p256Limbs);            array<uint> z1 = new array<uint>(p256Limbs);

            p256ScalarBaseMult(ref x1, ref y1, ref z1, ref scalarReversed);
            return p256ToAffine(ref x1, ref y1, ref z1);
        }

        private static (ref big.Int, ref big.Int) ScalarMult(this p256Curve _p0, ref big.Int bigX, ref big.Int bigY, slice<byte> scalar)
        {
            array<byte> scalarReversed = new array<byte>(32L);
            p256GetScalar(ref scalarReversed, scalar);

            array<uint> px = new array<uint>(p256Limbs);            array<uint> py = new array<uint>(p256Limbs);            array<uint> x1 = new array<uint>(p256Limbs);            array<uint> y1 = new array<uint>(p256Limbs);            array<uint> z1 = new array<uint>(p256Limbs);

            p256FromBig(ref px, bigX);
            p256FromBig(ref py, bigY);
            p256ScalarMult(ref x1, ref y1, ref z1, ref px, ref py, ref scalarReversed);
            return p256ToAffine(ref x1, ref y1, ref z1);
        }

        // Field elements are represented as nine, unsigned 32-bit words.
        //
        // The value of an field element is:
        //   x[0] + (x[1] * 2**29) + (x[2] * 2**57) + ... + (x[8] * 2**228)
        //
        // That is, each limb is alternately 29 or 28-bits wide in little-endian
        // order.
        //
        // This means that a field element hits 2**257, rather than 2**256 as we would
        // like. A 28, 29, ... pattern would cause us to hit 2**256, but that causes
        // problems when multiplying as terms end up one bit short of a limb which
        // would require much bit-shifting to correct.
        //
        // Finally, the values stored in a field element are in Montgomery form. So the
        // value |y| is stored as (y*R) mod p, where p is the P-256 prime and R is
        // 2**257.

        private static readonly long p256Limbs = 9L;
        private static readonly ulong bottom29Bits = 0x1fffffffUL;

 
        // p256One is the number 1 as a field element.
        private static array<uint> p256One = new array<uint>(new uint[] { 2, 0, 0, 0xffff800, 0x1fffffff, 0xfffffff, 0x1fbfffff, 0x1ffffff, 0 });        private static array<uint> p256Zero = new array<uint>(new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });        private static array<uint> p256P = new array<uint>(new uint[] { 0x1fffffff, 0xfffffff, 0x1fffffff, 0x3ff, 0, 0, 0x200000, 0xf000000, 0xfffffff });        private static array<uint> p2562P = new array<uint>(new uint[] { 0x1ffffffe, 0xfffffff, 0x1fffffff, 0x7ff, 0, 0, 0x400000, 0xe000000, 0x1fffffff });

        // p256Precomputed contains precomputed values to aid the calculation of scalar
        // multiples of the base point, G. It's actually two, equal length, tables
        // concatenated.
        //
        // The first table contains (x,y) field element pairs for 16 multiples of the
        // base point, G.
        //
        //   Index  |  Index (binary) | Value
        //       0  |           0000  | 0G (all zeros, omitted)
        //       1  |           0001  | G
        //       2  |           0010  | 2**64G
        //       3  |           0011  | 2**64G + G
        //       4  |           0100  | 2**128G
        //       5  |           0101  | 2**128G + G
        //       6  |           0110  | 2**128G + 2**64G
        //       7  |           0111  | 2**128G + 2**64G + G
        //       8  |           1000  | 2**192G
        //       9  |           1001  | 2**192G + G
        //      10  |           1010  | 2**192G + 2**64G
        //      11  |           1011  | 2**192G + 2**64G + G
        //      12  |           1100  | 2**192G + 2**128G
        //      13  |           1101  | 2**192G + 2**128G + G
        //      14  |           1110  | 2**192G + 2**128G + 2**64G
        //      15  |           1111  | 2**192G + 2**128G + 2**64G + G
        //
        // The second table follows the same style, but the terms are 2**32G,
        // 2**96G, 2**160G, 2**224G.
        //
        // This is ~2KB of data.
        private static array<uint> p256Precomputed = new array<uint>(new uint[] { 0x11522878, 0xe730d41, 0xdb60179, 0x4afe2ff, 0x12883add, 0xcaddd88, 0x119e7edc, 0xd4a6eab, 0x3120bee, 0x1d2aac15, 0xf25357c, 0x19e45cdd, 0x5c721d0, 0x1992c5a5, 0xa237487, 0x154ba21, 0x14b10bb, 0xae3fe3, 0xd41a576, 0x922fc51, 0x234994f, 0x60b60d3, 0x164586ae, 0xce95f18, 0x1fe49073, 0x3fa36cc, 0x5ebcd2c, 0xb402f2f, 0x15c70bf, 0x1561925c, 0x5a26704, 0xda91e90, 0xcdc1c7f, 0x1ea12446, 0xe1ade1e, 0xec91f22, 0x26f7778, 0x566847e, 0xa0bec9e, 0x234f453, 0x1a31f21a, 0xd85e75c, 0x56c7109, 0xa267a00, 0xb57c050, 0x98fb57, 0xaa837cc, 0x60c0792, 0xcfa5e19, 0x61bab9e, 0x589e39b, 0xa324c5, 0x7d6dee7, 0x2976e4b, 0x1fc4124a, 0xa8c244b, 0x1ce86762, 0xcd61c7e, 0x1831c8e0, 0x75774e1, 0x1d96a5a9, 0x843a649, 0xc3ab0fa, 0x6e2e7d5, 0x7673a2a, 0x178b65e8, 0x4003e9b, 0x1a1f11c2, 0x7816ea, 0xf643e11, 0x58c43df, 0xf423fc2, 0x19633ffa, 0x891f2b2, 0x123c231c, 0x46add8c, 0x54700dd, 0x59e2b17, 0x172db40f, 0x83e277d, 0xb0dd609, 0xfd1da12, 0x35c6e52, 0x19ede20c, 0xd19e0c0, 0x97d0f40, 0xb015b19, 0x449e3f5, 0xe10c9e, 0x33ab581, 0x56a67ab, 0x577734d, 0x1dddc062, 0xc57b10d, 0x149b39d, 0x26a9e7b, 0xc35df9f, 0x48764cd, 0x76dbcca, 0xca4b366, 0xe9303ab, 0x1a7480e7, 0x57e9e81, 0x1e13eb50, 0xf466cf3, 0x6f16b20, 0x4ba3173, 0xc168c33, 0x15cb5439, 0x6a38e11, 0x73658bd, 0xb29564f, 0x3f6dc5b, 0x53b97e, 0x1322c4c0, 0x65dd7ff, 0x3a1e4f6, 0x14e614aa, 0x9246317, 0x1bc83aca, 0xad97eed, 0xd38ce4a, 0xf82b006, 0x341f077, 0xa6add89, 0x4894acd, 0x9f162d5, 0xf8410ef, 0x1b266a56, 0xd7f223, 0x3e0cb92, 0xe39b672, 0x6a2901a, 0x69a8556, 0x7e7c0, 0x9b7d8d3, 0x309a80, 0x1ad05f7f, 0xc2fb5dd, 0xcbfd41d, 0x9ceb638, 0x1051825c, 0xda0cf5b, 0x812e881, 0x6f35669, 0x6a56f2c, 0x1df8d184, 0x345820, 0x1477d477, 0x1645db1, 0xbe80c51, 0xc22be3e, 0xe35e65a, 0x1aeb7aa0, 0xc375315, 0xf67bc99, 0x7fdd7b9, 0x191fc1be, 0x61235d, 0x2c184e9, 0x1c5a839, 0x47a1e26, 0xb7cb456, 0x93e225d, 0x14f3c6ed, 0xccc1ac9, 0x17fe37f3, 0x4988989, 0x1a90c502, 0x2f32042, 0xa17769b, 0xafd8c7c, 0x8191c6e, 0x1dcdb237, 0x16200c0, 0x107b32a1, 0x66c08db, 0x10d06a02, 0x3fc93, 0x5620023, 0x16722b27, 0x68b5c59, 0x270fcfc, 0xfad0ecc, 0xe5de1c2, 0xeab466b, 0x2fc513c, 0x407f75c, 0xbaab133, 0x9705fe9, 0xb88b8e7, 0x734c993, 0x1e1ff8f, 0x19156970, 0xabd0f00, 0x10469ea7, 0x3293ac0, 0xcdc98aa, 0x1d843fd, 0xe14bfe8, 0x15be825f, 0x8b5212, 0xeb3fb67, 0x81cbd29, 0xbc62f16, 0x2b6fcc7, 0xf5a4e29, 0x13560b66, 0xc0b6ac2, 0x51ae690, 0xd41e271, 0xf3e9bd4, 0x1d70aab, 0x1029f72, 0x73e1c35, 0xee70fbc, 0xad81baf, 0x9ecc49a, 0x86c741e, 0xfe6be30, 0x176752e7, 0x23d416, 0x1f83de85, 0x27de188, 0x66f70b8, 0x181cd51f, 0x96b6e4c, 0x188f2335, 0xa5df759, 0x17a77eb6, 0xfeb0e73, 0x154ae914, 0x2f3ec51, 0x3826b59, 0xb91f17d, 0x1c72949, 0x1362bf0a, 0xe23fddf, 0xa5614b0, 0xf7d8f, 0x79061, 0x823d9d2, 0x8213f39, 0x1128ae0b, 0xd095d05, 0xb85c0c2, 0x1ecb2ef, 0x24ddc84, 0xe35e901, 0x18411a4a, 0xf5ddc3d, 0x3786689, 0x52260e8, 0x5ae3564, 0x542b10d, 0x8d93a45, 0x19952aa4, 0x996cc41, 0x1051a729, 0x4be3499, 0x52b23aa, 0x109f307e, 0x6f5b6bb, 0x1f84e1e7, 0x77a0cfa, 0x10c4df3f, 0x25a02ea, 0xb048035, 0xe31de66, 0xc6ecaa3, 0x28ea335, 0x2886024, 0x1372f020, 0xf55d35, 0x15e4684c, 0xf2a9e17, 0x1a4a7529, 0xcb7beb1, 0xb2a78a1, 0x1ab21f1f, 0x6361ccf, 0x6c9179d, 0xb135627, 0x1267b974, 0x4408bad, 0x1cbff658, 0xe3d6511, 0xc7d76f, 0x1cc7a69, 0xe7ee31b, 0x54fab4f, 0x2b914f, 0x1ad27a30, 0xcd3579e, 0xc50124c, 0x50daa90, 0xb13f72, 0xb06aa75, 0x70f5cc6, 0x1649e5aa, 0x84a5312, 0x329043c, 0x41c4011, 0x13d32411, 0xb04a838, 0xd760d2d, 0x1713b532, 0xbaa0c03, 0x84022ab, 0x6bcf5c1, 0x2f45379, 0x18ae070, 0x18c9e11e, 0x20bca9a, 0x66f496b, 0x3eef294, 0x67500d2, 0xd7f613c, 0x2dbbeb, 0xb741038, 0xe04133f, 0x1582968d, 0xbe985f7, 0x1acbc1a, 0x1a6a939f, 0x33e50f6, 0xd665ed4, 0xb4b7bd6, 0x1e5a3799, 0x6b33847, 0x17fa56ff, 0x65ef930, 0x21dc4a, 0x2b37659, 0x450fe17, 0xb357b65, 0xdf5efac, 0x15397bef, 0x9d35a7f, 0x112ac15f, 0x624e62e, 0xa90ae2f, 0x107eecd2, 0x1f69bbe, 0x77d6bce, 0x5741394, 0x13c684fc, 0x950c910, 0x725522b, 0xdc78583, 0x40eeabb, 0x1fde328a, 0xbd61d96, 0xd28c387, 0x9e77d89, 0x12550c40, 0x759cb7d, 0x367ef34, 0xae2a960, 0x91b8bdc, 0x93462a9, 0xf469ef, 0xb2e9aef, 0xd2ca771, 0x54e1f42, 0x7aaa49, 0x6316abb, 0x2413c8e, 0x5425bf9, 0x1bed3e3a, 0xf272274, 0x1f5e7326, 0x6416517, 0xea27072, 0x9cedea7, 0x6e7633, 0x7c91952, 0xd806dce, 0x8e2a7e1, 0xe421e1a, 0x418c9e1, 0x1dbc890, 0x1b395c36, 0xa1dc175, 0x1dc4ef73, 0x8956f34, 0xe4b5cf2, 0x1b0d3a18, 0x3194a36, 0x6c2641f, 0xe44124c, 0xa2f4eaa, 0xa8c25ba, 0xf927ed7, 0x627b614, 0x7371cca, 0xba16694, 0x417bc03, 0x7c0a7e3, 0x9c35c19, 0x1168a205, 0x8b6b00d, 0x10e3edc9, 0x9c19bf2, 0x5882229, 0x1b2b4162, 0xa5cef1a, 0x1543622b, 0x9bd433e, 0x364e04d, 0x7480792, 0x5c9b5b3, 0xe85ff25, 0x408ef57, 0x1814cfa4, 0x121b41b, 0xd248a0f, 0x3b05222, 0x39bb16a, 0xc75966d, 0xa038113, 0xa4a1769, 0x11fbc6c, 0x917e50e, 0xeec3da8, 0x169d6eac, 0x10c1699, 0xa416153, 0xf724912, 0x15cd60b7, 0x4acbad9, 0x5efc5fa, 0xf150ed7, 0x122b51, 0x1104b40a, 0xcb7f442, 0xfbb28ff, 0x6ac53ca, 0x196142cc, 0x7bf0fa9, 0x957651, 0x4e0f215, 0xed439f8, 0x3f46bd5, 0x5ace82f, 0x110916b6, 0x6db078, 0xffd7d57, 0xf2ecaac, 0xca86dec, 0x15d6b2da, 0x965ecc9, 0x1c92b4c2, 0x1f3811, 0x1cb080f5, 0x2d8b804, 0x19d1c12d, 0xf20bd46, 0x1951fa7, 0xa3656c3, 0x523a425, 0xfcd0692, 0xd44ddc8, 0x131f0f5b, 0xaf80e4a, 0xcd9fc74, 0x99bb618, 0x2db944c, 0xa673090, 0x1c210e1, 0x178c8d23, 0x1474383, 0x10b8743d, 0x985a55b, 0x2e74779, 0x576138, 0x9587927, 0x133130fa, 0xbe05516, 0x9f4d619, 0xbb62570, 0x99ec591, 0xd9468fe, 0x1d07782d, 0xfc72e0b, 0x701b298, 0x1863863b, 0x85954b8, 0x121a0c36, 0x9e7fedf, 0xf64b429, 0x9b9d71e, 0x14e2f5d8, 0xf858d3a, 0x942eea8, 0xda5b765, 0x6edafff, 0xa9d18cc, 0xc65e4ba, 0x1c747e86, 0xe4ea915, 0x1981d7a1, 0x8395659, 0x52ed4e2, 0x87d43b7, 0x37ab11b, 0x19d292ce, 0xf8d4692, 0x18c3053f, 0x8863e13, 0x4c146c0, 0x6bdf55a, 0x4e4457d, 0x16152289, 0xac78ec2, 0x1a59c5a2, 0x2028b97, 0x71c2d01, 0x295851f, 0x404747b, 0x878558d, 0x7d29aa4, 0x13d8341f, 0x8daefd7, 0x139c972d, 0x6b7ea75, 0xd4a9dde, 0xff163d8, 0x81d55d7, 0xa5bef68, 0xb7b30d8, 0xbe73d6f, 0xaa88141, 0xd976c81, 0x7e7a9cc, 0x18beb771, 0xd773cbd, 0x13f51951, 0x9d0c177, 0x1c49a78 });

        // Field element operations:

        // nonZeroToAllOnes returns:
        //   0xffffffff for 0 < x <= 2**31
        //   0 for x == 0 or x > 2**31.
        private static uint nonZeroToAllOnes(uint x)
        {
            return ((x - 1L) >> (int)(31L)) - 1L;
        }

        // p256ReduceCarry adds a multiple of p in order to cancel |carry|,
        // which is a term at 2**257.
        //
        // On entry: carry < 2**3, inout[0,2,...] < 2**29, inout[1,3,...] < 2**28.
        // On exit: inout[0,2,..] < 2**30, inout[1,3,...] < 2**29.
        private static void p256ReduceCarry(ref array<uint> inout, uint carry)
        {
            var carry_mask = nonZeroToAllOnes(carry);

            inout[0L] += carry << (int)(1L);
            inout[3L] += 0x10000000UL & carry_mask; 
            // carry < 2**3 thus (carry << 11) < 2**14 and we added 2**28 in the
            // previous line therefore this doesn't underflow.
            inout[3L] -= carry << (int)(11L);
            inout[4L] += (0x20000000UL - 1L) & carry_mask;
            inout[5L] += (0x10000000UL - 1L) & carry_mask;
            inout[6L] += (0x20000000UL - 1L) & carry_mask;
            inout[6L] -= carry << (int)(22L); 
            // This may underflow if carry is non-zero but, if so, we'll fix it in the
            // next line.
            inout[7L] -= 1L & carry_mask;
            inout[7L] += carry << (int)(25L);
        }

        // p256Sum sets out = in+in2.
        //
        // On entry, in[i]+in2[i] must not overflow a 32-bit word.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29
        private static void p256Sum(ref array<uint> @out, ref array<uint> @in, ref array<uint> in2)
        {
            var carry = uint32(0L);
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                out[i] = in[i] + in2[i];
                out[i] += carry;
                carry = out[i] >> (int)(29L);
                out[i] &= bottom29Bits;

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                out[i] = in[i] + in2[i];
                out[i] += carry;
                carry = out[i] >> (int)(28L);
                out[i] &= bottom28Bits;
            }


            p256ReduceCarry(out, carry);
        }

        private static readonly long two30m2 = 1L << (int)(30L) - 1L << (int)(2L);
        private static readonly long two30p13m2 = 1L << (int)(30L) + 1L << (int)(13L) - 1L << (int)(2L);
        private static readonly long two31m2 = 1L << (int)(31L) - 1L << (int)(2L);
        private static readonly long two31p24m2 = 1L << (int)(31L) + 1L << (int)(24L) - 1L << (int)(2L);
        private static readonly long two30m27m2 = 1L << (int)(30L) - 1L << (int)(27L) - 1L << (int)(2L);

        // p256Zero31 is 0 mod p.
        private static array<uint> p256Zero31 = new array<uint>(new uint[] { two31m3, two30m2, two31m2, two30p13m2, two31m2, two30m2, two31p24m2, two30m27m2, two31m2 });

        // p256Diff sets out = in-in2.
        //
        // On entry: in[0,2,...] < 2**30, in[1,3,...] < 2**29 and
        //           in2[0,2,...] < 2**30, in2[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Diff(ref array<uint> @out, ref array<uint> @in, ref array<uint> in2)
        {
            uint carry = default;

            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                out[i] = in[i] - in2[i];
                out[i] += p256Zero31[i];
                out[i] += carry;
                carry = out[i] >> (int)(29L);
                out[i] &= bottom29Bits;

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                out[i] = in[i] - in2[i];
                out[i] += p256Zero31[i];
                out[i] += carry;
                carry = out[i] >> (int)(28L);
                out[i] &= bottom28Bits;
            }


            p256ReduceCarry(out, carry);
        }

        // p256ReduceDegree sets out = tmp/R mod p where tmp contains 64-bit words with
        // the same 29,28,... bit positions as an field element.
        //
        // The values in field elements are in Montgomery form: x*R mod p where R =
        // 2**257. Since we just multiplied two Montgomery values together, the result
        // is x*y*R*R mod p. We wish to divide by R in order for the result also to be
        // in Montgomery form.
        //
        // On entry: tmp[i] < 2**64
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29
        private static void p256ReduceDegree(ref array<uint> @out, array<ulong> tmp)
        {
            tmp = tmp.Clone();
 
            // The following table may be helpful when reading this code:
            //
            // Limb number:   0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10...
            // Width (bits):  29| 28| 29| 28| 29| 28| 29| 28| 29| 28| 29
            // Start bit:     0 | 29| 57| 86|114|143|171|200|228|257|285
            //   (odd phase): 0 | 28| 57| 85|114|142|171|199|228|256|285
            array<uint> tmp2 = new array<uint>(18L);
            uint carry = default;            uint x = default;            uint xMask = default; 

            // tmp contains 64-bit words with the same 29,28,29-bit positions as an
            // field element. So the top of an element of tmp might overlap with
            // another element two positions down. The following loop eliminates
            // this overlap.
 

            // tmp contains 64-bit words with the same 29,28,29-bit positions as an
            // field element. So the top of an element of tmp might overlap with
            // another element two positions down. The following loop eliminates
            // this overlap.
            tmp2[0L] = uint32(tmp[0L]) & bottom29Bits;

            tmp2[1L] = uint32(tmp[0L]) >> (int)(29L);
            tmp2[1L] |= (uint32(tmp[0L] >> (int)(32L)) << (int)(3L)) & bottom28Bits;
            tmp2[1L] += uint32(tmp[1L]) & bottom28Bits;
            carry = tmp2[1L] >> (int)(28L);
            tmp2[1L] &= bottom28Bits;

            {
                long i__prev1 = i;

                for (long i = 2L; i < 17L; i++)
                {
                    tmp2[i] = (uint32(tmp[i - 2L] >> (int)(32L))) >> (int)(25L);
                    tmp2[i] += (uint32(tmp[i - 1L])) >> (int)(28L);
                    tmp2[i] += (uint32(tmp[i - 1L] >> (int)(32L)) << (int)(4L)) & bottom29Bits;
                    tmp2[i] += uint32(tmp[i]) & bottom29Bits;
                    tmp2[i] += carry;
                    carry = tmp2[i] >> (int)(29L);
                    tmp2[i] &= bottom29Bits;

                    i++;
                    if (i == 17L)
                    {
                        break;
                    }
                    tmp2[i] = uint32(tmp[i - 2L] >> (int)(32L)) >> (int)(25L);
                    tmp2[i] += uint32(tmp[i - 1L]) >> (int)(29L);
                    tmp2[i] += ((uint32(tmp[i - 1L] >> (int)(32L))) << (int)(3L)) & bottom28Bits;
                    tmp2[i] += uint32(tmp[i]) & bottom28Bits;
                    tmp2[i] += carry;
                    carry = tmp2[i] >> (int)(28L);
                    tmp2[i] &= bottom28Bits;
                }


                i = i__prev1;
            }

            tmp2[17L] = uint32(tmp[15L] >> (int)(32L)) >> (int)(25L);
            tmp2[17L] += uint32(tmp[16L]) >> (int)(29L);
            tmp2[17L] += uint32(tmp[16L] >> (int)(32L)) << (int)(3L);
            tmp2[17L] += carry; 

            // Montgomery elimination of terms:
            //
            // Since R is 2**257, we can divide by R with a bitwise shift if we can
            // ensure that the right-most 257 bits are all zero. We can make that true
            // by adding multiplies of p without affecting the value.
            //
            // So we eliminate limbs from right to left. Since the bottom 29 bits of p
            // are all ones, then by adding tmp2[0]*p to tmp2 we'll make tmp2[0] == 0.
            // We can do that for 8 further limbs and then right shift to eliminate the
            // extra factor of R.
            {
                long i__prev1 = i;

                i = 0L;

                while (i < 8L)
                {
                    tmp2[i + 1L] += tmp2[i] >> (int)(29L);
                    x = tmp2[i] & bottom29Bits;
                    xMask = nonZeroToAllOnes(x);
                    tmp2[i] = 0L; 

                    // The bounds calculations for this loop are tricky. Each iteration of
                    // the loop eliminates two words by adding values to words to their
                    // right.
                    //
                    // The following table contains the amounts added to each word (as an
                    // offset from the value of i at the top of the loop). The amounts are
                    // accounted for from the first and second half of the loop separately
                    // and are written as, for example, 28 to mean a value <2**28.
                    //
                    // Word:                   3   4   5   6   7   8   9   10
                    // Added in top half:     28  11      29  21  29  28
                    //                                        28  29
                    //                                            29
                    // Added in bottom half:      29  10      28  21  28   28
                    //                                            29
                    //
                    // The value that is currently offset 7 will be offset 5 for the next
                    // iteration and then offset 3 for the iteration after that. Therefore
                    // the total value added will be the values added at 7, 5 and 3.
                    //
                    // The following table accumulates these values. The sums at the bottom
                    // are written as, for example, 29+28, to mean a value < 2**29+2**28.
                    //
                    // Word:                   3   4   5   6   7   8   9  10  11  12  13
                    //                        28  11  10  29  21  29  28  28  28  28  28
                    //                            29  28  11  28  29  28  29  28  29  28
                    //                                    29  28  21  21  29  21  29  21
                    //                                        10  29  28  21  28  21  28
                    //                                        28  29  28  29  28  29  28
                    //                                            11  10  29  10  29  10
                    //                                            29  28  11  28  11
                    //                                                    29      29
                    //                        --------------------------------------------
                    //                                                30+ 31+ 30+ 31+ 30+
                    //                                                28+ 29+ 28+ 29+ 21+
                    //                                                21+ 28+ 21+ 28+ 10
                    //                                                10  21+ 10  21+
                    //                                                    11      11
                    //
                    // So the greatest amount is added to tmp2[10] and tmp2[12]. If
                    // tmp2[10/12] has an initial value of <2**29, then the maximum value
                    // will be < 2**31 + 2**30 + 2**28 + 2**21 + 2**11, which is < 2**32,
                    // as required.
                    tmp2[i + 3L] += (x << (int)(10L)) & bottom28Bits;
                    tmp2[i + 4L] += (x >> (int)(18L));

                    tmp2[i + 6L] += (x << (int)(21L)) & bottom29Bits;
                    tmp2[i + 7L] += x >> (int)(8L); 

                    // At position 200, which is the starting bit position for word 7, we
                    // have a factor of 0xf000000 = 2**28 - 2**24.
                    tmp2[i + 7L] += 0x10000000UL & xMask;
                    tmp2[i + 8L] += (x - 1L) & xMask;
                    tmp2[i + 7L] -= (x << (int)(24L)) & bottom28Bits;
                    tmp2[i + 8L] -= x >> (int)(4L);

                    tmp2[i + 8L] += 0x20000000UL & xMask;
                    tmp2[i + 8L] -= x;
                    tmp2[i + 8L] += (x << (int)(28L)) & bottom29Bits;
                    tmp2[i + 9L] += ((x >> (int)(1L)) - 1L) & xMask;

                    if (i + 1L == p256Limbs)
                    {
                        break;
                    i += 2L;
                    }
                    tmp2[i + 2L] += tmp2[i + 1L] >> (int)(28L);
                    x = tmp2[i + 1L] & bottom28Bits;
                    xMask = nonZeroToAllOnes(x);
                    tmp2[i + 1L] = 0L;

                    tmp2[i + 4L] += (x << (int)(11L)) & bottom29Bits;
                    tmp2[i + 5L] += (x >> (int)(18L));

                    tmp2[i + 7L] += (x << (int)(21L)) & bottom28Bits;
                    tmp2[i + 8L] += x >> (int)(7L); 

                    // At position 199, which is the starting bit of the 8th word when
                    // dealing with a context starting on an odd word, we have a factor of
                    // 0x1e000000 = 2**29 - 2**25. Since we have not updated i, the 8th
                    // word from i+1 is i+8.
                    tmp2[i + 8L] += 0x20000000UL & xMask;
                    tmp2[i + 9L] += (x - 1L) & xMask;
                    tmp2[i + 8L] -= (x << (int)(25L)) & bottom29Bits;
                    tmp2[i + 9L] -= x >> (int)(4L);

                    tmp2[i + 9L] += 0x10000000UL & xMask;
                    tmp2[i + 9L] -= x;
                    tmp2[i + 10L] += (x - 1L) & xMask;
                } 

                // We merge the right shift with a carry chain. The words above 2**257 have
                // widths of 28,29,... which we need to correct when copying them down.


                i = i__prev1;
            } 

            // We merge the right shift with a carry chain. The words above 2**257 have
            // widths of 28,29,... which we need to correct when copying them down.
            carry = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                { 
                    // The maximum value of tmp2[i + 9] occurs on the first iteration and
                    // is < 2**30+2**29+2**28. Adding 2**29 (from tmp2[i + 10]) is
                    // therefore safe.
                    out[i] = tmp2[i + 9L];
                    out[i] += carry;
                    out[i] += (tmp2[i + 10L] << (int)(28L)) & bottom29Bits;
                    carry = out[i] >> (int)(29L);
                    out[i] &= bottom29Bits;

                    i++;
                    out[i] = tmp2[i + 9L] >> (int)(1L);
                    out[i] += carry;
                    carry = out[i] >> (int)(28L);
                    out[i] &= bottom28Bits;
                }


                i = i__prev1;
            }

            out[8L] = tmp2[17L];
            out[8L] += carry;
            carry = out[8L] >> (int)(29L);
            out[8L] &= bottom29Bits;

            p256ReduceCarry(out, carry);
        }

        // p256Square sets out=in*in.
        //
        // On entry: in[0,2,...] < 2**30, in[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Square(ref array<uint> @out, ref array<uint> @in)
        {
            array<ulong> tmp = new array<ulong>(17L);

            tmp[0L] = uint64(in[0L]) * uint64(in[0L]);
            tmp[1L] = uint64(in[0L]) * (uint64(in[1L]) << (int)(1L));
            tmp[2L] = uint64(in[0L]) * (uint64(in[2L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[1L]) << (int)(1L));
            tmp[3L] = uint64(in[0L]) * (uint64(in[3L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[2L]) << (int)(1L));
            tmp[4L] = uint64(in[0L]) * (uint64(in[4L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[3L]) << (int)(2L)) + uint64(in[2L]) * uint64(in[2L]);
            tmp[5L] = uint64(in[0L]) * (uint64(in[5L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[4L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in[3L]) << (int)(1L));
            tmp[6L] = uint64(in[0L]) * (uint64(in[6L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[5L]) << (int)(2L)) + uint64(in[2L]) * (uint64(in[4L]) << (int)(1L)) + uint64(in[3L]) * (uint64(in[3L]) << (int)(1L));
            tmp[7L] = uint64(in[0L]) * (uint64(in[7L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[6L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in[5L]) << (int)(1L)) + uint64(in[3L]) * (uint64(in[4L]) << (int)(1L)); 
            // tmp[8] has the greatest value of 2**61 + 2**60 + 2**61 + 2**60 + 2**60,
            // which is < 2**64 as required.
            tmp[8L] = uint64(in[0L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[1L]) * (uint64(in[7L]) << (int)(2L)) + uint64(in[2L]) * (uint64(in[6L]) << (int)(1L)) + uint64(in[3L]) * (uint64(in[5L]) << (int)(2L)) + uint64(in[4L]) * uint64(in[4L]);
            tmp[9L] = uint64(in[1L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in[7L]) << (int)(1L)) + uint64(in[3L]) * (uint64(in[6L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in[5L]) << (int)(1L));
            tmp[10L] = uint64(in[2L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[3L]) * (uint64(in[7L]) << (int)(2L)) + uint64(in[4L]) * (uint64(in[6L]) << (int)(1L)) + uint64(in[5L]) * (uint64(in[5L]) << (int)(1L));
            tmp[11L] = uint64(in[3L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in[7L]) << (int)(1L)) + uint64(in[5L]) * (uint64(in[6L]) << (int)(1L));
            tmp[12L] = uint64(in[4L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[5L]) * (uint64(in[7L]) << (int)(2L)) + uint64(in[6L]) * uint64(in[6L]);
            tmp[13L] = uint64(in[5L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[6L]) * (uint64(in[7L]) << (int)(1L));
            tmp[14L] = uint64(in[6L]) * (uint64(in[8L]) << (int)(1L)) + uint64(in[7L]) * (uint64(in[7L]) << (int)(1L));
            tmp[15L] = uint64(in[7L]) * (uint64(in[8L]) << (int)(1L));
            tmp[16L] = uint64(in[8L]) * uint64(in[8L]);

            p256ReduceDegree(out, tmp);
        }

        // p256Mul sets out=in*in2.
        //
        // On entry: in[0,2,...] < 2**30, in[1,3,...] < 2**29 and
        //           in2[0,2,...] < 2**30, in2[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Mul(ref array<uint> @out, ref array<uint> @in, ref array<uint> in2)
        {
            array<ulong> tmp = new array<ulong>(17L);

            tmp[0L] = uint64(in[0L]) * uint64(in2[0L]);
            tmp[1L] = uint64(in[0L]) * (uint64(in2[1L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[2L] = uint64(in[0L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[1L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[3L] = uint64(in[0L]) * (uint64(in2[3L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[2L]) * (uint64(in2[1L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[4L] = uint64(in[0L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[3L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[1L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[5L] = uint64(in[0L]) * (uint64(in2[5L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[2L]) * (uint64(in2[3L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[4L]) * (uint64(in2[1L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[6L] = uint64(in[0L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[5L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[3L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[1L]) << (int)(1L)) + uint64(in[6L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[7L] = uint64(in[0L]) * (uint64(in2[7L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[2L]) * (uint64(in2[5L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[4L]) * (uint64(in2[3L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[6L]) * (uint64(in2[1L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[0L]) << (int)(0L)); 
            // tmp[8] has the greatest value but doesn't overflow. See logic in
            // p256Square.
            tmp[8L] = uint64(in[0L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[1L]) * (uint64(in2[7L]) << (int)(1L)) + uint64(in[2L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[5L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[3L]) << (int)(1L)) + uint64(in[6L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[1L]) << (int)(1L)) + uint64(in[8L]) * (uint64(in2[0L]) << (int)(0L));
            tmp[9L] = uint64(in[1L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[2L]) * (uint64(in2[7L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[4L]) * (uint64(in2[5L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[6L]) * (uint64(in2[3L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[2L]) << (int)(0L)) + uint64(in[8L]) * (uint64(in2[1L]) << (int)(0L));
            tmp[10L] = uint64(in[2L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[3L]) * (uint64(in2[7L]) << (int)(1L)) + uint64(in[4L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[5L]) << (int)(1L)) + uint64(in[6L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[3L]) << (int)(1L)) + uint64(in[8L]) * (uint64(in2[2L]) << (int)(0L));
            tmp[11L] = uint64(in[3L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[4L]) * (uint64(in2[7L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[6L]) * (uint64(in2[5L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[4L]) << (int)(0L)) + uint64(in[8L]) * (uint64(in2[3L]) << (int)(0L));
            tmp[12L] = uint64(in[4L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[5L]) * (uint64(in2[7L]) << (int)(1L)) + uint64(in[6L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[5L]) << (int)(1L)) + uint64(in[8L]) * (uint64(in2[4L]) << (int)(0L));
            tmp[13L] = uint64(in[5L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[6L]) * (uint64(in2[7L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[6L]) << (int)(0L)) + uint64(in[8L]) * (uint64(in2[5L]) << (int)(0L));
            tmp[14L] = uint64(in[6L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[7L]) * (uint64(in2[7L]) << (int)(1L)) + uint64(in[8L]) * (uint64(in2[6L]) << (int)(0L));
            tmp[15L] = uint64(in[7L]) * (uint64(in2[8L]) << (int)(0L)) + uint64(in[8L]) * (uint64(in2[7L]) << (int)(0L));
            tmp[16L] = uint64(in[8L]) * (uint64(in2[8L]) << (int)(0L));

            p256ReduceDegree(out, tmp);
        }

        private static void p256Assign(ref array<uint> @out, ref array<uint> @in)
        {
            out.Value = in.Value;
        }

        // p256Invert calculates |out| = |in|^{-1}
        //
        // Based on Fermat's Little Theorem:
        //   a^p = a (mod p)
        //   a^{p-1} = 1 (mod p)
        //   a^{p-2} = a^{-1} (mod p)
        private static void p256Invert(ref array<uint> @out, ref array<uint> @in)
        {
            array<uint> ftmp = new array<uint>(p256Limbs);            array<uint> ftmp2 = new array<uint>(p256Limbs); 

            // each e_I will hold |in|^{2^I - 1}
 

            // each e_I will hold |in|^{2^I - 1}
            array<uint> e2 = new array<uint>(p256Limbs);            array<uint> e4 = new array<uint>(p256Limbs);            array<uint> e8 = new array<uint>(p256Limbs);            array<uint> e16 = new array<uint>(p256Limbs);            array<uint> e32 = new array<uint>(p256Limbs);            array<uint> e64 = new array<uint>(p256Limbs);



            p256Square(ref ftmp, in); // 2^1
            p256Mul(ref ftmp, in, ref ftmp); // 2^2 - 2^0
            p256Assign(ref e2, ref ftmp);
            p256Square(ref ftmp, ref ftmp); // 2^3 - 2^1
            p256Square(ref ftmp, ref ftmp); // 2^4 - 2^2
            p256Mul(ref ftmp, ref ftmp, ref e2); // 2^4 - 2^0
            p256Assign(ref e4, ref ftmp);
            p256Square(ref ftmp, ref ftmp); // 2^5 - 2^1
            p256Square(ref ftmp, ref ftmp); // 2^6 - 2^2
            p256Square(ref ftmp, ref ftmp); // 2^7 - 2^3
            p256Square(ref ftmp, ref ftmp); // 2^8 - 2^4
            p256Mul(ref ftmp, ref ftmp, ref e4); // 2^8 - 2^0
            p256Assign(ref e8, ref ftmp);
            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    p256Square(ref ftmp, ref ftmp);
                } // 2^16 - 2^8


                i = i__prev1;
            } // 2^16 - 2^8
            p256Mul(ref ftmp, ref ftmp, ref e8); // 2^16 - 2^0
            p256Assign(ref e16, ref ftmp);
            {
                long i__prev1 = i;

                for (i = 0L; i < 16L; i++)
                {
                    p256Square(ref ftmp, ref ftmp);
                } // 2^32 - 2^16


                i = i__prev1;
            } // 2^32 - 2^16
            p256Mul(ref ftmp, ref ftmp, ref e16); // 2^32 - 2^0
            p256Assign(ref e32, ref ftmp);
            {
                long i__prev1 = i;

                for (i = 0L; i < 32L; i++)
                {
                    p256Square(ref ftmp, ref ftmp);
                } // 2^64 - 2^32


                i = i__prev1;
            } // 2^64 - 2^32
            p256Assign(ref e64, ref ftmp);
            p256Mul(ref ftmp, ref ftmp, in); // 2^64 - 2^32 + 2^0
            {
                long i__prev1 = i;

                for (i = 0L; i < 192L; i++)
                {
                    p256Square(ref ftmp, ref ftmp);
                } // 2^256 - 2^224 + 2^192


                i = i__prev1;
            } // 2^256 - 2^224 + 2^192

            p256Mul(ref ftmp2, ref e64, ref e32); // 2^64 - 2^0
            {
                long i__prev1 = i;

                for (i = 0L; i < 16L; i++)
                {
                    p256Square(ref ftmp2, ref ftmp2);
                } // 2^80 - 2^16


                i = i__prev1;
            } // 2^80 - 2^16
            p256Mul(ref ftmp2, ref ftmp2, ref e16); // 2^80 - 2^0
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    p256Square(ref ftmp2, ref ftmp2);
                } // 2^88 - 2^8


                i = i__prev1;
            } // 2^88 - 2^8
            p256Mul(ref ftmp2, ref ftmp2, ref e8); // 2^88 - 2^0
            {
                long i__prev1 = i;

                for (i = 0L; i < 4L; i++)
                {
                    p256Square(ref ftmp2, ref ftmp2);
                } // 2^92 - 2^4


                i = i__prev1;
            } // 2^92 - 2^4
            p256Mul(ref ftmp2, ref ftmp2, ref e4); // 2^92 - 2^0
            p256Square(ref ftmp2, ref ftmp2); // 2^93 - 2^1
            p256Square(ref ftmp2, ref ftmp2); // 2^94 - 2^2
            p256Mul(ref ftmp2, ref ftmp2, ref e2); // 2^94 - 2^0
            p256Square(ref ftmp2, ref ftmp2); // 2^95 - 2^1
            p256Square(ref ftmp2, ref ftmp2); // 2^96 - 2^2
            p256Mul(ref ftmp2, ref ftmp2, in); // 2^96 - 3

            p256Mul(out, ref ftmp2, ref ftmp); // 2^256 - 2^224 + 2^192 + 2^96 - 3
        }

        // p256Scalar3 sets out=3*out.
        //
        // On entry: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Scalar3(ref array<uint> @out)
        {
            uint carry = default;

            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                out[i] *= 3L;
                out[i] += carry;
                carry = out[i] >> (int)(29L);
                out[i] &= bottom29Bits;

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                out[i] *= 3L;
                out[i] += carry;
                carry = out[i] >> (int)(28L);
                out[i] &= bottom28Bits;
            }


            p256ReduceCarry(out, carry);
        }

        // p256Scalar4 sets out=4*out.
        //
        // On entry: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Scalar4(ref array<uint> @out)
        {
            uint carry = default;            uint nextCarry = default;



            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                nextCarry = out[i] >> (int)(27L);
                out[i] <<= 2L;
                out[i] &= bottom29Bits;
                out[i] += carry;
                carry = nextCarry + (out[i] >> (int)(29L));
                out[i] &= bottom29Bits;

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                nextCarry = out[i] >> (int)(26L);
                out[i] <<= 2L;
                out[i] &= bottom28Bits;
                out[i] += carry;
                carry = nextCarry + (out[i] >> (int)(28L));
                out[i] &= bottom28Bits;
            }


            p256ReduceCarry(out, carry);
        }

        // p256Scalar8 sets out=8*out.
        //
        // On entry: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        // On exit: out[0,2,...] < 2**30, out[1,3,...] < 2**29.
        private static void p256Scalar8(ref array<uint> @out)
        {
            uint carry = default;            uint nextCarry = default;



            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                nextCarry = out[i] >> (int)(26L);
                out[i] <<= 3L;
                out[i] &= bottom29Bits;
                out[i] += carry;
                carry = nextCarry + (out[i] >> (int)(29L));
                out[i] &= bottom29Bits;

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                nextCarry = out[i] >> (int)(25L);
                out[i] <<= 3L;
                out[i] &= bottom28Bits;
                out[i] += carry;
                carry = nextCarry + (out[i] >> (int)(28L));
                out[i] &= bottom28Bits;
            }


            p256ReduceCarry(out, carry);
        }

        // Group operations:
        //
        // Elements of the elliptic curve group are represented in Jacobian
        // coordinates: (x, y, z). An affine point (x', y') is x'=x/z**2, y'=y/z**3 in
        // Jacobian form.

        // p256PointDouble sets {xOut,yOut,zOut} = 2*{x,y,z}.
        //
        // See http://www.hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-0.html#doubling-dbl-2009-l
        private static void p256PointDouble(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<uint> x, ref array<uint> y, ref array<uint> z)
        {
            array<uint> delta = new array<uint>(p256Limbs);            array<uint> gamma = new array<uint>(p256Limbs);            array<uint> alpha = new array<uint>(p256Limbs);            array<uint> beta = new array<uint>(p256Limbs);            array<uint> tmp = new array<uint>(p256Limbs);            array<uint> tmp2 = new array<uint>(p256Limbs);



            p256Square(ref delta, z);
            p256Square(ref gamma, y);
            p256Mul(ref beta, x, ref gamma);

            p256Sum(ref tmp, x, ref delta);
            p256Diff(ref tmp2, x, ref delta);
            p256Mul(ref alpha, ref tmp, ref tmp2);
            p256Scalar3(ref alpha);

            p256Sum(ref tmp, y, z);
            p256Square(ref tmp, ref tmp);
            p256Diff(ref tmp, ref tmp, ref gamma);
            p256Diff(zOut, ref tmp, ref delta);

            p256Scalar4(ref beta);
            p256Square(xOut, ref alpha);
            p256Diff(xOut, xOut, ref beta);
            p256Diff(xOut, xOut, ref beta);

            p256Diff(ref tmp, ref beta, xOut);
            p256Mul(ref tmp, ref alpha, ref tmp);
            p256Square(ref tmp2, ref gamma);
            p256Scalar8(ref tmp2);
            p256Diff(yOut, ref tmp, ref tmp2);
        }

        // p256PointAddMixed sets {xOut,yOut,zOut} = {x1,y1,z1} + {x2,y2,1}.
        // (i.e. the second point is affine.)
        //
        // See http://www.hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-0.html#addition-add-2007-bl
        //
        // Note that this function does not handle P+P, infinity+P nor P+infinity
        // correctly.
        private static void p256PointAddMixed(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<uint> x1, ref array<uint> y1, ref array<uint> z1, ref array<uint> x2, ref array<uint> y2)
        {
            array<uint> z1z1 = new array<uint>(p256Limbs);            array<uint> z1z1z1 = new array<uint>(p256Limbs);            array<uint> s2 = new array<uint>(p256Limbs);            array<uint> u2 = new array<uint>(p256Limbs);            array<uint> h = new array<uint>(p256Limbs);            array<uint> i = new array<uint>(p256Limbs);            array<uint> j = new array<uint>(p256Limbs);            array<uint> r = new array<uint>(p256Limbs);            array<uint> rr = new array<uint>(p256Limbs);            array<uint> v = new array<uint>(p256Limbs);            array<uint> tmp = new array<uint>(p256Limbs);



            p256Square(ref z1z1, z1);
            p256Sum(ref tmp, z1, z1);

            p256Mul(ref u2, x2, ref z1z1);
            p256Mul(ref z1z1z1, z1, ref z1z1);
            p256Mul(ref s2, y2, ref z1z1z1);
            p256Diff(ref h, ref u2, x1);
            p256Sum(ref i, ref h, ref h);
            p256Square(ref i, ref i);
            p256Mul(ref j, ref h, ref i);
            p256Diff(ref r, ref s2, y1);
            p256Sum(ref r, ref r, ref r);
            p256Mul(ref v, x1, ref i);

            p256Mul(zOut, ref tmp, ref h);
            p256Square(ref rr, ref r);
            p256Diff(xOut, ref rr, ref j);
            p256Diff(xOut, xOut, ref v);
            p256Diff(xOut, xOut, ref v);

            p256Diff(ref tmp, ref v, xOut);
            p256Mul(yOut, ref tmp, ref r);
            p256Mul(ref tmp, y1, ref j);
            p256Diff(yOut, yOut, ref tmp);
            p256Diff(yOut, yOut, ref tmp);
        }

        // p256PointAdd sets {xOut,yOut,zOut} = {x1,y1,z1} + {x2,y2,z2}.
        //
        // See http://www.hyperelliptic.org/EFD/g1p/auto-shortw-jacobian-0.html#addition-add-2007-bl
        //
        // Note that this function does not handle P+P, infinity+P nor P+infinity
        // correctly.
        private static void p256PointAdd(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<uint> x1, ref array<uint> y1, ref array<uint> z1, ref array<uint> x2, ref array<uint> y2, ref array<uint> z2)
        {
            array<uint> z1z1 = new array<uint>(p256Limbs);            array<uint> z1z1z1 = new array<uint>(p256Limbs);            array<uint> z2z2 = new array<uint>(p256Limbs);            array<uint> z2z2z2 = new array<uint>(p256Limbs);            array<uint> s1 = new array<uint>(p256Limbs);            array<uint> s2 = new array<uint>(p256Limbs);            array<uint> u1 = new array<uint>(p256Limbs);            array<uint> u2 = new array<uint>(p256Limbs);            array<uint> h = new array<uint>(p256Limbs);            array<uint> i = new array<uint>(p256Limbs);            array<uint> j = new array<uint>(p256Limbs);            array<uint> r = new array<uint>(p256Limbs);            array<uint> rr = new array<uint>(p256Limbs);            array<uint> v = new array<uint>(p256Limbs);            array<uint> tmp = new array<uint>(p256Limbs);



            p256Square(ref z1z1, z1);
            p256Square(ref z2z2, z2);
            p256Mul(ref u1, x1, ref z2z2);

            p256Sum(ref tmp, z1, z2);
            p256Square(ref tmp, ref tmp);
            p256Diff(ref tmp, ref tmp, ref z1z1);
            p256Diff(ref tmp, ref tmp, ref z2z2);

            p256Mul(ref z2z2z2, z2, ref z2z2);
            p256Mul(ref s1, y1, ref z2z2z2);

            p256Mul(ref u2, x2, ref z1z1);
            p256Mul(ref z1z1z1, z1, ref z1z1);
            p256Mul(ref s2, y2, ref z1z1z1);
            p256Diff(ref h, ref u2, ref u1);
            p256Sum(ref i, ref h, ref h);
            p256Square(ref i, ref i);
            p256Mul(ref j, ref h, ref i);
            p256Diff(ref r, ref s2, ref s1);
            p256Sum(ref r, ref r, ref r);
            p256Mul(ref v, ref u1, ref i);

            p256Mul(zOut, ref tmp, ref h);
            p256Square(ref rr, ref r);
            p256Diff(xOut, ref rr, ref j);
            p256Diff(xOut, xOut, ref v);
            p256Diff(xOut, xOut, ref v);

            p256Diff(ref tmp, ref v, xOut);
            p256Mul(yOut, ref tmp, ref r);
            p256Mul(ref tmp, ref s1, ref j);
            p256Diff(yOut, yOut, ref tmp);
            p256Diff(yOut, yOut, ref tmp);
        }

        // p256CopyConditional sets out=in if mask = 0xffffffff in constant time.
        //
        // On entry: mask is either 0 or 0xffffffff.
        private static void p256CopyConditional(ref array<uint> @out, ref array<uint> @in, uint mask)
        {
            for (long i = 0L; i < p256Limbs; i++)
            {
                var tmp = mask & (in[i] ^ out[i]);
                out[i] ^= tmp;
            }

        }

        // p256SelectAffinePoint sets {out_x,out_y} to the index'th entry of table.
        // On entry: index < 16, table[0] must be zero.
        private static void p256SelectAffinePoint(ref array<uint> xOut, ref array<uint> yOut, slice<uint> table, uint index)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in xOut)
                {
                    i = __i;
                    xOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in yOut)
                {
                    i = __i;
                    yOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (var i = uint32(1L); i < 16L; i++)
                {
                    var mask = i ^ index;
                    mask |= mask >> (int)(2L);
                    mask |= mask >> (int)(1L);
                    mask &= 1L;
                    mask--;
                    {
                        var j__prev2 = j;

                        foreach (var (__j) in xOut)
                        {
                            j = __j;
                            xOut[j] |= table[0L] & mask;
                            table = table[1L..];
                        }

                        j = j__prev2;
                    }

                    {
                        var j__prev2 = j;

                        foreach (var (__j) in yOut)
                        {
                            j = __j;
                            yOut[j] |= table[0L] & mask;
                            table = table[1L..];
                        }

                        j = j__prev2;
                    }

                }


                i = i__prev1;
            }
        }

        // p256SelectJacobianPoint sets {out_x,out_y,out_z} to the index'th entry of
        // table.
        // On entry: index < 16, table[0] must be zero.
        private static void p256SelectJacobianPoint(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<array<array<uint>>> table, uint index)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in xOut)
                {
                    i = __i;
                    xOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in yOut)
                {
                    i = __i;
                    yOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in zOut)
                {
                    i = __i;
                    zOut[i] = 0L;
                } 

                // The implicit value at index 0 is all zero. We don't need to perform that
                // iteration of the loop because we already set out_* to zero.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (var i = uint32(1L); i < 16L; i++)
                {
                    var mask = i ^ index;
                    mask |= mask >> (int)(2L);
                    mask |= mask >> (int)(1L);
                    mask &= 1L;
                    mask--;
                    {
                        var j__prev2 = j;

                        foreach (var (__j) in xOut)
                        {
                            j = __j;
                            xOut[j] |= table[i][0L][j] & mask;
                        }

                        j = j__prev2;
                    }

                    {
                        var j__prev2 = j;

                        foreach (var (__j) in yOut)
                        {
                            j = __j;
                            yOut[j] |= table[i][1L][j] & mask;
                        }

                        j = j__prev2;
                    }

                    {
                        var j__prev2 = j;

                        foreach (var (__j) in zOut)
                        {
                            j = __j;
                            zOut[j] |= table[i][2L][j] & mask;
                        }

                        j = j__prev2;
                    }

                }


                i = i__prev1;
            }
        }

        // p256GetBit returns the bit'th bit of scalar.
        private static uint p256GetBit(ref array<byte> scalar, ulong bit)
        {
            return uint32(((scalar[bit >> (int)(3L)]) >> (int)((bit & 7L))) & 1L);
        }

        // p256ScalarBaseMult sets {xOut,yOut,zOut} = scalar*G where scalar is a
        // little-endian number. Note that the value of scalar must be less than the
        // order of the group.
        private static void p256ScalarBaseMult(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<byte> scalar)
        {
            var nIsInfinityMask = ~uint32(0L);
            uint pIsNoninfiniteMask = default;            uint mask = default;            uint tableOffset = default;

            array<uint> px = new array<uint>(p256Limbs);            array<uint> py = new array<uint>(p256Limbs);            array<uint> tx = new array<uint>(p256Limbs);            array<uint> ty = new array<uint>(p256Limbs);            array<uint> tz = new array<uint>(p256Limbs);



            {
                var i__prev1 = i;

                foreach (var (__i) in xOut)
                {
                    i = __i;
                    xOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in yOut)
                {
                    i = __i;
                    yOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in zOut)
                {
                    i = __i;
                    zOut[i] = 0L;
                } 

                // The loop adds bits at positions 0, 64, 128 and 192, followed by
                // positions 32,96,160 and 224 and does this 32 times.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (var i = uint(0L); i < 32L; i++)
                {
                    if (i != 0L)
                    {
                        p256PointDouble(xOut, yOut, zOut, xOut, yOut, zOut);
                    }
                    tableOffset = 0L;
                    {
                        var j = uint(0L);

                        while (j <= 32L)
                        {
                            var bit0 = p256GetBit(scalar, 31L - i + j);
                            var bit1 = p256GetBit(scalar, 95L - i + j);
                            var bit2 = p256GetBit(scalar, 159L - i + j);
                            var bit3 = p256GetBit(scalar, 223L - i + j);
                            var index = bit0 | (bit1 << (int)(1L)) | (bit2 << (int)(2L)) | (bit3 << (int)(3L));

                            p256SelectAffinePoint(ref px, ref py, p256Precomputed[tableOffset..], index);
                            tableOffset += 30L * p256Limbs; 

                            // Since scalar is less than the order of the group, we know that
                            // {xOut,yOut,zOut} != {px,py,1}, unless both are zero, which we handle
                            // below.
                            p256PointAddMixed(ref tx, ref ty, ref tz, xOut, yOut, zOut, ref px, ref py); 
                            // The result of pointAddMixed is incorrect if {xOut,yOut,zOut} is zero
                            // (a.k.a.  the point at infinity). We handle that situation by
                            // copying the point from the table.
                            p256CopyConditional(xOut, ref px, nIsInfinityMask);
                            p256CopyConditional(yOut, ref py, nIsInfinityMask);
                            p256CopyConditional(zOut, ref p256One, nIsInfinityMask); 

                            // Equally, the result is also wrong if the point from the table is
                            // zero, which happens when the index is zero. We handle that by
                            // only copying from {tx,ty,tz} to {xOut,yOut,zOut} if index != 0.
                            pIsNoninfiniteMask = nonZeroToAllOnes(index);
                            mask = pIsNoninfiniteMask & ~nIsInfinityMask;
                            p256CopyConditional(xOut, ref tx, mask);
                            p256CopyConditional(yOut, ref ty, mask);
                            p256CopyConditional(zOut, ref tz, mask); 
                            // If p was not zero, then n is now non-zero.
                            nIsInfinityMask &= pIsNoninfiniteMask;
                            j += 32L;
                        }

                    }
                }


                i = i__prev1;
            }
        }

        // p256PointToAffine converts a Jacobian point to an affine point. If the input
        // is the point at infinity then it returns (0, 0) in constant time.
        private static void p256PointToAffine(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> x, ref array<uint> y, ref array<uint> z)
        {
            array<uint> zInv = new array<uint>(p256Limbs);            array<uint> zInvSq = new array<uint>(p256Limbs);



            p256Invert(ref zInv, z);
            p256Square(ref zInvSq, ref zInv);
            p256Mul(xOut, x, ref zInvSq);
            p256Mul(ref zInv, ref zInv, ref zInvSq);
            p256Mul(yOut, y, ref zInv);
        }

        // p256ToAffine returns a pair of *big.Int containing the affine representation
        // of {x,y,z}.
        private static (ref big.Int, ref big.Int) p256ToAffine(ref array<uint> x, ref array<uint> y, ref array<uint> z)
        {
            array<uint> xx = new array<uint>(p256Limbs);            array<uint> yy = new array<uint>(p256Limbs);

            p256PointToAffine(ref xx, ref yy, x, y, z);
            return (p256ToBig(ref xx), p256ToBig(ref yy));
        }

        // p256ScalarMult sets {xOut,yOut,zOut} = scalar*{x,y}.
        private static void p256ScalarMult(ref array<uint> xOut, ref array<uint> yOut, ref array<uint> zOut, ref array<uint> x, ref array<uint> y, ref array<byte> scalar)
        {
            array<uint> px = new array<uint>(p256Limbs);            array<uint> py = new array<uint>(p256Limbs);            array<uint> pz = new array<uint>(p256Limbs);            array<uint> tx = new array<uint>(p256Limbs);            array<uint> ty = new array<uint>(p256Limbs);            array<uint> tz = new array<uint>(p256Limbs);

            array<array<array<uint>>> precomp = new array<array<array<uint>>>(16L);
            uint nIsInfinityMask = default;            uint index = default;            uint pIsNoninfiniteMask = default;            uint mask = default; 

            // We precompute 0,1,2,... times {x,y}.
 

            // We precompute 0,1,2,... times {x,y}.
            precomp[1L][0L] = x.Value;
            precomp[1L][1L] = y.Value;
            precomp[1L][2L] = p256One;

            {
                long i__prev1 = i;

                long i = 2L;

                while (i < 16L)
                {
                    p256PointDouble(ref precomp[i][0L], ref precomp[i][1L], ref precomp[i][2L], ref precomp[i / 2L][0L], ref precomp[i / 2L][1L], ref precomp[i / 2L][2L]);
                    p256PointAddMixed(ref precomp[i + 1L][0L], ref precomp[i + 1L][1L], ref precomp[i + 1L][2L], ref precomp[i][0L], ref precomp[i][1L], ref precomp[i][2L], x, y);
                    i += 2L;
                }


                i = i__prev1;
            }

            {
                long i__prev1 = i;

                foreach (var (__i) in xOut)
                {
                    i = __i;
                    xOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                long i__prev1 = i;

                foreach (var (__i) in yOut)
                {
                    i = __i;
                    yOut[i] = 0L;
                }

                i = i__prev1;
            }

            {
                long i__prev1 = i;

                foreach (var (__i) in zOut)
                {
                    i = __i;
                    zOut[i] = 0L;
                }

                i = i__prev1;
            }

            nIsInfinityMask = ~uint32(0L); 

            // We add in a window of four bits each iteration and do this 64 times.
            {
                long i__prev1 = i;

                for (i = 0L; i < 64L; i++)
                {
                    if (i != 0L)
                    {
                        p256PointDouble(xOut, yOut, zOut, xOut, yOut, zOut);
                        p256PointDouble(xOut, yOut, zOut, xOut, yOut, zOut);
                        p256PointDouble(xOut, yOut, zOut, xOut, yOut, zOut);
                        p256PointDouble(xOut, yOut, zOut, xOut, yOut, zOut);
                    }
                    index = uint32(scalar[31L - i / 2L]);
                    if ((i & 1L) == 1L)
                    {
                        index &= 15L;
                    }
                    else
                    {
                        index >>= 4L;
                    } 

                    // See the comments in scalarBaseMult about handling infinities.
                    p256SelectJacobianPoint(ref px, ref py, ref pz, ref precomp, index);
                    p256PointAdd(ref tx, ref ty, ref tz, xOut, yOut, zOut, ref px, ref py, ref pz);
                    p256CopyConditional(xOut, ref px, nIsInfinityMask);
                    p256CopyConditional(yOut, ref py, nIsInfinityMask);
                    p256CopyConditional(zOut, ref pz, nIsInfinityMask);

                    pIsNoninfiniteMask = nonZeroToAllOnes(index);
                    mask = pIsNoninfiniteMask & ~nIsInfinityMask;
                    p256CopyConditional(xOut, ref tx, mask);
                    p256CopyConditional(yOut, ref ty, mask);
                    p256CopyConditional(zOut, ref tz, mask);
                    nIsInfinityMask &= pIsNoninfiniteMask;
                }


                i = i__prev1;
            }
        }

        // p256FromBig sets out = R*in.
        private static void p256FromBig(ref array<uint> @out, ref big.Int @in)
        {
            ptr<big.Int> tmp = @new<big.Int>().Lsh(in, 257L);
            tmp.Mod(tmp, p256Params.P);

            for (long i = 0L; i < p256Limbs; i++)
            {
                {
                    var bits__prev1 = bits;

                    var bits = tmp.Bits();

                    if (len(bits) > 0L)
                    {
                        out[i] = uint32(bits[0L]) & bottom29Bits;
                    }
                    else
                    {
                        out[i] = 0L;
                    }

                    bits = bits__prev1;

                }
                tmp.Rsh(tmp, 29L);

                i++;
                if (i == p256Limbs)
                {
                    break;
                }
                {
                    var bits__prev1 = bits;

                    bits = tmp.Bits();

                    if (len(bits) > 0L)
                    {
                        out[i] = uint32(bits[0L]) & bottom28Bits;
                    }
                    else
                    {
                        out[i] = 0L;
                    }

                    bits = bits__prev1;

                }
                tmp.Rsh(tmp, 28L);
            }

        }

        // p256ToBig returns a *big.Int containing the value of in.
        private static ref big.Int p256ToBig(ref array<uint> @in)
        {
            ptr<big.Int> result = @new<big.Int>();
            ptr<big.Int> tmp = @new<big.Int>();

            result.SetInt64(int64(in[p256Limbs - 1L]));
            for (var i = p256Limbs - 2L; i >= 0L; i--)
            {
                if ((i & 1L) == 0L)
                {
                    result.Lsh(result, 29L);
                }
                else
                {
                    result.Lsh(result, 28L);
                }
                tmp.SetInt64(int64(in[i]));
                result.Add(result, tmp);
            }


            result.Mul(result, p256RInverse);
            result.Mod(result, p256Params.P);
            return result;
        }
    }
}}
