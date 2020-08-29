// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jpeg -- go2cs converted at 2020 August 29 10:10:11 UTC
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
        private static readonly long blockSize = 64L; // A DCT block is 8x8.

 // A DCT block is 8x8.

        private partial struct block // : array<int>
        {
        }

        private static readonly long w1 = 2841L; // 2048*sqrt(2)*cos(1*pi/16)
        private static readonly long w2 = 2676L; // 2048*sqrt(2)*cos(2*pi/16)
        private static readonly long w3 = 2408L; // 2048*sqrt(2)*cos(3*pi/16)
        private static readonly long w5 = 1609L; // 2048*sqrt(2)*cos(5*pi/16)
        private static readonly long w6 = 1108L; // 2048*sqrt(2)*cos(6*pi/16)
        private static readonly long w7 = 565L; // 2048*sqrt(2)*cos(7*pi/16)

        private static readonly var w1pw7 = w1 + w7;
        private static readonly var w1mw7 = w1 - w7;
        private static readonly var w2pw6 = w2 + w6;
        private static readonly var w2mw6 = w2 - w6;
        private static readonly var w3pw5 = w3 + w5;
        private static readonly var w3mw5 = w3 - w5;

        private static readonly long r2 = 181L; // 256/sqrt(2)

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
        private static void idct(ref block src)
        { 
            // Horizontal 1-D IDCT.
            for (long y = 0L; y < 8L; y++)
            {
                var y8 = y * 8L; 
                // If all the AC components are zero, then the IDCT is trivial.
                if (src[y8 + 1L] == 0L && src[y8 + 2L] == 0L && src[y8 + 3L] == 0L && src[y8 + 4L] == 0L && src[y8 + 5L] == 0L && src[y8 + 6L] == 0L && src[y8 + 7L] == 0L)
                {
                    var dc = src[y8 + 0L] << (int)(3L);
                    src[y8 + 0L] = dc;
                    src[y8 + 1L] = dc;
                    src[y8 + 2L] = dc;
                    src[y8 + 3L] = dc;
                    src[y8 + 4L] = dc;
                    src[y8 + 5L] = dc;
                    src[y8 + 6L] = dc;
                    src[y8 + 7L] = dc;
                    continue;
                } 

                // Prescale.
                var x0 = (src[y8 + 0L] << (int)(11L)) + 128L;
                var x1 = src[y8 + 4L] << (int)(11L);
                var x2 = src[y8 + 6L];
                var x3 = src[y8 + 2L];
                var x4 = src[y8 + 1L];
                var x5 = src[y8 + 7L];
                var x6 = src[y8 + 5L];
                var x7 = src[y8 + 3L]; 

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
                src[y8 + 0L] = (x7 + x1) >> (int)(8L);
                src[y8 + 1L] = (x3 + x2) >> (int)(8L);
                src[y8 + 2L] = (x0 + x4) >> (int)(8L);
                src[y8 + 3L] = (x8 + x6) >> (int)(8L);
                src[y8 + 4L] = (x8 - x6) >> (int)(8L);
                src[y8 + 5L] = (x0 - x4) >> (int)(8L);
                src[y8 + 6L] = (x3 - x2) >> (int)(8L);
                src[y8 + 7L] = (x7 - x1) >> (int)(8L);
            } 

            // Vertical 1-D IDCT.
 

            // Vertical 1-D IDCT.
            for (long x = 0L; x < 8L; x++)
            { 
                // Similar to the horizontal 1-D IDCT case, if all the AC components are zero, then the IDCT is trivial.
                // However, after performing the horizontal 1-D IDCT, there are typically non-zero AC components, so
                // we do not bother to check for the all-zero case.

                // Prescale.
                var y0 = (src[8L * 0L + x] << (int)(8L)) + 8192L;
                var y1 = src[8L * 4L + x] << (int)(8L);
                var y2 = src[8L * 6L + x];
                var y3 = src[8L * 2L + x];
                var y4 = src[8L * 1L + x];
                var y5 = src[8L * 7L + x];
                var y6 = src[8L * 5L + x];
                var y7 = src[8L * 3L + x]; 

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
                src[8L * 0L + x] = (y7 + y1) >> (int)(14L);
                src[8L * 1L + x] = (y3 + y2) >> (int)(14L);
                src[8L * 2L + x] = (y0 + y4) >> (int)(14L);
                src[8L * 3L + x] = (y8 + y6) >> (int)(14L);
                src[8L * 4L + x] = (y8 - y6) >> (int)(14L);
                src[8L * 5L + x] = (y0 - y4) >> (int)(14L);
                src[8L * 6L + x] = (y3 - y2) >> (int)(14L);
                src[8L * 7L + x] = (y7 - y1) >> (int)(14L);
            }

        }
    }
}}
