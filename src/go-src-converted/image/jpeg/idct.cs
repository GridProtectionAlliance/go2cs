// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2020 October 08 04:59:25 UTC
// import "image/jpeg" ==> using jpeg = go.image.jpeg_package
// Original source: C:\Go\src\image\jpeg\idct.go

using static go.builtin;

namespace go {
namespace image
{
    public static partial class jpeg_package
    {
        // This is a Go translation of idct.c from
        //
        // http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_IEC_13818-4_2004_Conformance_Testing/Video/verifier/mpeg2decode_960109.tar.gz
        //
        // which carries the following notice:

        /* Copyright (C) 1996, MPEG Software Simulation Group. All Rights Reserved. */

        /*
         * Disclaimer of Warranty
         *
         * These software programs are available to the user without any license fee or
         * royalty on an "as is" basis.  The MPEG Software Simulation Group disclaims
         * any and all warranties, whether express, implied, or statuary, including any
         * implied warranties or merchantability or of fitness for a particular
         * purpose.  In no event shall the copyright-holder be liable for any
         * incidental, punitive, or consequential damages of any kind whatsoever
         * arising from the use of these programs.
         *
         * This disclaimer of warranty extends to the user of these programs and user's
         * customers, employees, agents, transferees, successors, and assigns.
         *
         * The MPEG Software Simulation Group does not represent or warrant that the
         * programs furnished hereunder are free of infringement of any third-party
         * patents.
         *
         * Commercial implementations of MPEG-1 and MPEG-2 video, including shareware,
         * are subject to royalty fees to patent holders.  Many of these patents are
         * general enough such that they are unavoidable regardless of implementation
         * design.
         *
         */
        private static readonly long blockSize = (long)64L; // A DCT block is 8x8.

 // A DCT block is 8x8.

        private partial struct block // : array<int>
        {
        }

        private static readonly long w1 = (long)2841L; // 2048*sqrt(2)*cos(1*pi/16)
        private static readonly long w2 = (long)2676L; // 2048*sqrt(2)*cos(2*pi/16)
        private static readonly long w3 = (long)2408L; // 2048*sqrt(2)*cos(3*pi/16)
        private static readonly long w5 = (long)1609L; // 2048*sqrt(2)*cos(5*pi/16)
        private static readonly long w6 = (long)1108L; // 2048*sqrt(2)*cos(6*pi/16)
        private static readonly long w7 = (long)565L; // 2048*sqrt(2)*cos(7*pi/16)

        private static readonly var w1pw7 = (var)w1 + w7;
        private static readonly var w1mw7 = (var)w1 - w7;
        private static readonly var w2pw6 = (var)w2 + w6;
        private static readonly var w2mw6 = (var)w2 - w6;
        private static readonly var w3pw5 = (var)w3 + w5;
        private static readonly var w3mw5 = (var)w3 - w5;

        private static readonly long r2 = (long)181L; // 256/sqrt(2)

        // idct performs a 2-D Inverse Discrete Cosine Transformation.
        //
        // The input coefficients should already have been multiplied by the
        // appropriate quantization table. We use fixed-point computation, with the
        // number of bits for the fractional component varying over the intermediate
        // stages.
        //
        // For more on the actual algorithm, see Z. Wang, "Fast algorithms for the
        // discrete W transform and for the discrete Fourier transform", IEEE Trans. on
        // ASSP, Vol. ASSP- 32, pp. 803-816, Aug. 1984.
        private static void idct(ptr<block> _addr_src)
        {
            ref block src = ref _addr_src.val;
 
            // Horizontal 1-D IDCT.
            for (long y = 0L; y < 8L; y++)
            {
                var y8 = y * 8L;
                var s = src.slice(y8, y8 + 8L, y8 + 8L); // Small cap improves performance, see https://golang.org/issue/27857
                // If all the AC components are zero, then the IDCT is trivial.
                if (s[1L] == 0L && s[2L] == 0L && s[3L] == 0L && s[4L] == 0L && s[5L] == 0L && s[6L] == 0L && s[7L] == 0L)
                {
                    var dc = s[0L] << (int)(3L);
                    s[0L] = dc;
                    s[1L] = dc;
                    s[2L] = dc;
                    s[3L] = dc;
                    s[4L] = dc;
                    s[5L] = dc;
                    s[6L] = dc;
                    s[7L] = dc;
                    continue;
                } 

                // Prescale.
                var x0 = (s[0L] << (int)(11L)) + 128L;
                var x1 = s[4L] << (int)(11L);
                var x2 = s[6L];
                var x3 = s[2L];
                var x4 = s[1L];
                var x5 = s[7L];
                var x6 = s[5L];
                var x7 = s[3L]; 

                // Stage 1.
                var x8 = w7 * (x4 + x5);
                x4 = x8 + w1mw7 * x4;
                x5 = x8 - w1pw7 * x5;
                x8 = w3 * (x6 + x7);
                x6 = x8 - w3mw5 * x6;
                x7 = x8 - w3pw5 * x7; 

                // Stage 2.
                x8 = x0 + x1;
                x0 -= x1;
                x1 = w6 * (x3 + x2);
                x2 = x1 - w2pw6 * x2;
                x3 = x1 + w2mw6 * x3;
                x1 = x4 + x6;
                x4 -= x6;
                x6 = x5 + x7;
                x5 -= x7; 

                // Stage 3.
                x7 = x8 + x3;
                x8 -= x3;
                x3 = x0 + x2;
                x0 -= x2;
                x2 = (r2 * (x4 + x5) + 128L) >> (int)(8L);
                x4 = (r2 * (x4 - x5) + 128L) >> (int)(8L); 

                // Stage 4.
                s[0L] = (x7 + x1) >> (int)(8L);
                s[1L] = (x3 + x2) >> (int)(8L);
                s[2L] = (x0 + x4) >> (int)(8L);
                s[3L] = (x8 + x6) >> (int)(8L);
                s[4L] = (x8 - x6) >> (int)(8L);
                s[5L] = (x0 - x4) >> (int)(8L);
                s[6L] = (x3 - x2) >> (int)(8L);
                s[7L] = (x7 - x1) >> (int)(8L);

            } 

            // Vertical 1-D IDCT.
 

            // Vertical 1-D IDCT.
            for (long x = 0L; x < 8L; x++)
            { 
                // Similar to the horizontal 1-D IDCT case, if all the AC components are zero, then the IDCT is trivial.
                // However, after performing the horizontal 1-D IDCT, there are typically non-zero AC components, so
                // we do not bother to check for the all-zero case.
                s = src.slice(x, x + 57L, x + 57L); // Small cap improves performance, see https://golang.org/issue/27857

                // Prescale.
                var y0 = (s[8L * 0L] << (int)(8L)) + 8192L;
                var y1 = s[8L * 4L] << (int)(8L);
                var y2 = s[8L * 6L];
                var y3 = s[8L * 2L];
                var y4 = s[8L * 1L];
                var y5 = s[8L * 7L];
                var y6 = s[8L * 5L];
                var y7 = s[8L * 3L]; 

                // Stage 1.
                y8 = w7 * (y4 + y5) + 4L;
                y4 = (y8 + w1mw7 * y4) >> (int)(3L);
                y5 = (y8 - w1pw7 * y5) >> (int)(3L);
                y8 = w3 * (y6 + y7) + 4L;
                y6 = (y8 - w3mw5 * y6) >> (int)(3L);
                y7 = (y8 - w3pw5 * y7) >> (int)(3L); 

                // Stage 2.
                y8 = y0 + y1;
                y0 -= y1;
                y1 = w6 * (y3 + y2) + 4L;
                y2 = (y1 - w2pw6 * y2) >> (int)(3L);
                y3 = (y1 + w2mw6 * y3) >> (int)(3L);
                y1 = y4 + y6;
                y4 -= y6;
                y6 = y5 + y7;
                y5 -= y7; 

                // Stage 3.
                y7 = y8 + y3;
                y8 -= y3;
                y3 = y0 + y2;
                y0 -= y2;
                y2 = (r2 * (y4 + y5) + 128L) >> (int)(8L);
                y4 = (r2 * (y4 - y5) + 128L) >> (int)(8L); 

                // Stage 4.
                s[8L * 0L] = (y7 + y1) >> (int)(14L);
                s[8L * 1L] = (y3 + y2) >> (int)(14L);
                s[8L * 2L] = (y0 + y4) >> (int)(14L);
                s[8L * 3L] = (y8 + y6) >> (int)(14L);
                s[8L * 4L] = (y8 - y6) >> (int)(14L);
                s[8L * 5L] = (y0 - y4) >> (int)(14L);
                s[8L * 6L] = (y3 - y2) >> (int)(14L);
                s[8L * 7L] = (y7 - y1) >> (int)(14L);

            }


        }
    }
}}
