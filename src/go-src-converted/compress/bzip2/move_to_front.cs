// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2020 October 08 04:58:42 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Go\src\compress\bzip2\move_to_front.go

using static go.builtin;

namespace go {
namespace compress
{
    public static partial class bzip2_package
    {
        // moveToFrontDecoder implements a move-to-front list. Such a list is an
        // efficient way to transform a string with repeating elements into one with
        // many small valued numbers, which is suitable for entropy encoding. It works
        // by starting with an initial list of symbols and references symbols by their
        // index into that list. When a symbol is referenced, it's moved to the front
        // of the list. Thus, a repeated symbol ends up being encoded with many zeros,
        // as the symbol will be at the front of the list after the first access.
        private partial struct moveToFrontDecoder // : slice<byte>
        {
        }

        // newMTFDecoder creates a move-to-front decoder with an explicit initial list
        // of symbols.
        private static moveToFrontDecoder newMTFDecoder(slice<byte> symbols) => func((_, panic, __) =>
        {
            if (len(symbols) > 256L)
            {
                panic("too many symbols");
            }

            return moveToFrontDecoder(symbols);

        });

        // newMTFDecoderWithRange creates a move-to-front decoder with an initial
        // symbol list of 0...n-1.
        private static moveToFrontDecoder newMTFDecoderWithRange(long n) => func((_, panic, __) =>
        {
            if (n > 256L)
            {
                panic("newMTFDecoderWithRange: cannot have > 256 symbols");
            }

            var m = make_slice<byte>(n);
            for (long i = 0L; i < n; i++)
            {
                m[i] = byte(i);
            }

            return moveToFrontDecoder(m);

        });

        private static byte Decode(this moveToFrontDecoder m, long n)
        {
            byte b = default;
 
            // Implement move-to-front with a simple copy. This approach
            // beats more sophisticated approaches in benchmarking, probably
            // because it has high locality of reference inside of a
            // single cache line (most move-to-front operations have n < 64).
            b = m[n];
            copy(m[1L..], m[..n]);
            m[0L] = b;
            return ;

        }

        // First returns the symbol at the front of the list.
        private static byte First(this moveToFrontDecoder m)
        {
            return m[0L];
        }
    }
}}
