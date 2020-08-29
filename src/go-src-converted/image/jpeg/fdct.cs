// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2020 August 29 10:10:09 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Go\src\image\jpeg\fdct.go

using static go.builtin;

namespace go {
namespace image
{
    public static partial class jpeg_package
    {
        // This file implements a Forward Discrete Cosine Transformation.

        /*
        It is based on the code in jfdctint.c from the Independent JPEG Group,
        found at http://www.ijg.org/files/jpegsrc.v8c.tar.gz.

        The "LEGAL ISSUES" section of the README in that archive says:

        In plain English:

        1. We don't promise that this software works.  (But if you find any bugs,
           please let us know!)
        2. You can use this software for whatever you want.  You don't have to pay us.
        3. You may not pretend that you wrote this software.  If you use it in a
           program, you must acknowledge somewhere in your documentation that
           you've used the IJG code.

        In legalese:

        The authors make NO WARRANTY or representation, either express or implied,
        with respect to this software, its quality, accuracy, merchantability, or
        fitness for a particular purpose.  This software is provided "AS IS", and you,
        its user, assume the entire risk as to its quality and accuracy.

        This software is copyright (C) 1991-2011, Thomas G. Lane, Guido Vollbeding.
        All Rights Reserved except as specified below.

        Permission is hereby granted to use, copy, modify, and distribute this
        software (or portions thereof) for any purpose, without fee, subject to these
        conditions:
        (1) If any part of the source code for this software is distributed, then this
        README file must be included, with this copyright and no-warranty notice
        unaltered; and any additions, deletions, or changes to the original files
        must be clearly indicated in accompanying documentation.
        (2) If only executable code is distributed, then the accompanying
        documentation must state that "this software is based in part on the work of
        the Independent JPEG Group".
        (3) Permission for use of this software is granted only if the user accepts
        full responsibility for any undesirable consequences; the authors accept
        NO LIABILITY for damages of any kind.

        These conditions apply to any software derived from or based on the IJG code,
        not just to the unmodified library.  If you use our work, you ought to
        acknowledge us.

        Permission is NOT granted for the use of any IJG author's name or company name
        in advertising or publicity relating to this software or products derived from
        it.  This software may be referred to only as "the Independent JPEG Group's
        software".

        We specifically permit and encourage the use of this software as the basis of
        commercial products, provided that all warranty or liability claims are
        assumed by the product vendor.
        */

        // Trigonometric constants in 13-bit fixed point format.
        private static readonly long fix_0_298631336 = 2446L;
        private static readonly long fix_0_390180644 = 3196L;
        private static readonly long fix_0_541196100 = 4433L;
        private static readonly long fix_0_765366865 = 6270L;
        private static readonly long fix_0_899976223 = 7373L;
        private static readonly long fix_1_175875602 = 9633L;
        private static readonly long fix_1_501321110 = 12299L;
        private static readonly long fix_1_847759065 = 15137L;
        private static readonly long fix_1_961570560 = 16069L;
        private static readonly long fix_2_053119869 = 16819L;
        private static readonly long fix_2_562915447 = 20995L;
        private static readonly long fix_3_072711026 = 25172L;

        private static readonly long constBits = 13L;
        private static readonly long pass1Bits = 2L;
        private static readonly long centerJSample = 128L;

        // fdct performs a forward DCT on an 8x8 block of coefficients, including a
        // level shift.
        private static void fdct(ref block b)
        { 
            // Pass 1: process rows.
            for (long y = 0L; y < 8L; y++)
            {
                var x0 = b[y * 8L + 0L];
                var x1 = b[y * 8L + 1L];
                var x2 = b[y * 8L + 2L];
                var x3 = b[y * 8L + 3L];
                var x4 = b[y * 8L + 4L];
                var x5 = b[y * 8L + 5L];
                var x6 = b[y * 8L + 6L];
                var x7 = b[y * 8L + 7L];

                var tmp0 = x0 + x7;
                var tmp1 = x1 + x6;
                var tmp2 = x2 + x5;
                var tmp3 = x3 + x4;

                var tmp10 = tmp0 + tmp3;
                var tmp12 = tmp0 - tmp3;
                var tmp11 = tmp1 + tmp2;
                var tmp13 = tmp1 - tmp2;

                tmp0 = x0 - x7;
                tmp1 = x1 - x6;
                tmp2 = x2 - x5;
                tmp3 = x3 - x4;

                b[y * 8L + 0L] = (tmp10 + tmp11 - 8L * centerJSample) << (int)(pass1Bits);
                b[y * 8L + 4L] = (tmp10 - tmp11) << (int)(pass1Bits);
                var z1 = (tmp12 + tmp13) * fix_0_541196100;
                z1 += 1L << (int)((constBits - pass1Bits - 1L));
                b[y * 8L + 2L] = (z1 + tmp12 * fix_0_765366865) >> (int)((constBits - pass1Bits));
                b[y * 8L + 6L] = (z1 - tmp13 * fix_1_847759065) >> (int)((constBits - pass1Bits));

                tmp10 = tmp0 + tmp3;
                tmp11 = tmp1 + tmp2;
                tmp12 = tmp0 + tmp2;
                tmp13 = tmp1 + tmp3;
                z1 = (tmp12 + tmp13) * fix_1_175875602;
                z1 += 1L << (int)((constBits - pass1Bits - 1L));
                tmp0 = tmp0 * fix_1_501321110;
                tmp1 = tmp1 * fix_3_072711026;
                tmp2 = tmp2 * fix_2_053119869;
                tmp3 = tmp3 * fix_0_298631336;
                tmp10 = tmp10 * -fix_0_899976223;
                tmp11 = tmp11 * -fix_2_562915447;
                tmp12 = tmp12 * -fix_0_390180644;
                tmp13 = tmp13 * -fix_1_961570560;

                tmp12 += z1;
                tmp13 += z1;
                b[y * 8L + 1L] = (tmp0 + tmp10 + tmp12) >> (int)((constBits - pass1Bits));
                b[y * 8L + 3L] = (tmp1 + tmp11 + tmp13) >> (int)((constBits - pass1Bits));
                b[y * 8L + 5L] = (tmp2 + tmp11 + tmp12) >> (int)((constBits - pass1Bits));
                b[y * 8L + 7L] = (tmp3 + tmp10 + tmp13) >> (int)((constBits - pass1Bits));
            } 
            // Pass 2: process columns.
            // We remove pass1Bits scaling, but leave results scaled up by an overall factor of 8.
 
            // Pass 2: process columns.
            // We remove pass1Bits scaling, but leave results scaled up by an overall factor of 8.
            for (long x = 0L; x < 8L; x++)
            {
                tmp0 = b[0L * 8L + x] + b[7L * 8L + x];
                tmp1 = b[1L * 8L + x] + b[6L * 8L + x];
                tmp2 = b[2L * 8L + x] + b[5L * 8L + x];
                tmp3 = b[3L * 8L + x] + b[4L * 8L + x];

                tmp10 = tmp0 + tmp3 + 1L << (int)((pass1Bits - 1L));
                tmp12 = tmp0 - tmp3;
                tmp11 = tmp1 + tmp2;
                tmp13 = tmp1 - tmp2;

                tmp0 = b[0L * 8L + x] - b[7L * 8L + x];
                tmp1 = b[1L * 8L + x] - b[6L * 8L + x];
                tmp2 = b[2L * 8L + x] - b[5L * 8L + x];
                tmp3 = b[3L * 8L + x] - b[4L * 8L + x];

                b[0L * 8L + x] = (tmp10 + tmp11) >> (int)(pass1Bits);
                b[4L * 8L + x] = (tmp10 - tmp11) >> (int)(pass1Bits);

                z1 = (tmp12 + tmp13) * fix_0_541196100;
                z1 += 1L << (int)((constBits + pass1Bits - 1L));
                b[2L * 8L + x] = (z1 + tmp12 * fix_0_765366865) >> (int)((constBits + pass1Bits));
                b[6L * 8L + x] = (z1 - tmp13 * fix_1_847759065) >> (int)((constBits + pass1Bits));

                tmp10 = tmp0 + tmp3;
                tmp11 = tmp1 + tmp2;
                tmp12 = tmp0 + tmp2;
                tmp13 = tmp1 + tmp3;
                z1 = (tmp12 + tmp13) * fix_1_175875602;
                z1 += 1L << (int)((constBits + pass1Bits - 1L));
                tmp0 = tmp0 * fix_1_501321110;
                tmp1 = tmp1 * fix_3_072711026;
                tmp2 = tmp2 * fix_2_053119869;
                tmp3 = tmp3 * fix_0_298631336;
                tmp10 = tmp10 * -fix_0_899976223;
                tmp11 = tmp11 * -fix_2_562915447;
                tmp12 = tmp12 * -fix_0_390180644;
                tmp13 = tmp13 * -fix_1_961570560;

                tmp12 += z1;
                tmp13 += z1;
                b[1L * 8L + x] = (tmp0 + tmp10 + tmp12) >> (int)((constBits + pass1Bits));
                b[3L * 8L + x] = (tmp1 + tmp11 + tmp13) >> (int)((constBits + pass1Bits));
                b[5L * 8L + x] = (tmp2 + tmp11 + tmp12) >> (int)((constBits + pass1Bits));
                b[7L * 8L + x] = (tmp3 + tmp10 + tmp13) >> (int)((constBits + pass1Bits));
            }

        }
    }
}}
