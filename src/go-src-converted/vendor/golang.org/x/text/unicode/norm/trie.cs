// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2020 October 08 05:03:32 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\norm\trie.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class norm_package
    {
        private partial struct valueRange
        {
            public ushort value; // header: value:stride
            public byte lo; // header: lo:n
            public byte hi; // header: lo:n
        }

        private partial struct sparseBlocks
        {
            public slice<valueRange> values;
            public slice<ushort> offset;
        }

        private static sparseBlocks nfcSparse = new sparseBlocks(values:nfcSparseValues[:],offset:nfcSparseOffset[:],);

        private static sparseBlocks nfkcSparse = new sparseBlocks(values:nfkcSparseValues[:],offset:nfkcSparseOffset[:],);

        private static var nfcData = newNfcTrie(0L);        private static var nfkcData = newNfkcTrie(0L);

        // lookupValue determines the type of block n and looks up the value for b.
        // For n < t.cutoff, the block is a simple lookup table. Otherwise, the block
        // is a list of ranges with an accompanying value. Given a matching range r,
        // the value for b is by r.value + (b - r.lo) * stride.
        private static ushort lookup(this ptr<sparseBlocks> _addr_t, uint n, byte b)
        {
            ref sparseBlocks t = ref _addr_t.val;

            var offset = t.offset[n];
            var header = t.values[offset];
            var lo = offset + 1L;
            var hi = lo + uint16(header.lo);
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                var r = t.values[m];
                if (r.lo <= b && b <= r.hi)
                {
                    return r.value + uint16(b - r.lo) * header.value;
                }

                if (b < r.lo)
                {
                    hi = m;
                }
                else
                {
                    lo = m + 1L;
                }

            }

            return 0L;

        }
    }
}}}}}}
