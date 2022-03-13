// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 13 06:48:20 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\trie.go
namespace go.vendor.golang.org.x.text.unicode;

public static partial class norm_package {

private partial struct valueRange {
    public ushort value; // header: value:stride
    public byte lo; // header: lo:n
    public byte hi; // header: lo:n
}

private partial struct sparseBlocks {
    public slice<valueRange> values;
    public slice<ushort> offset;
}

private static sparseBlocks nfcSparse = new sparseBlocks(values:nfcSparseValues[:],offset:nfcSparseOffset[:],);

private static sparseBlocks nfkcSparse = new sparseBlocks(values:nfkcSparseValues[:],offset:nfkcSparseOffset[:],);

private static var nfcData = newNfcTrie(0);private static var nfkcData = newNfkcTrie(0);

// lookupValue determines the type of block n and looks up the value for b.
// For n < t.cutoff, the block is a simple lookup table. Otherwise, the block
// is a list of ranges with an accompanying value. Given a matching range r,
// the value for b is by r.value + (b - r.lo) * stride.
private static ushort lookup(this ptr<sparseBlocks> _addr_t, uint n, byte b) {
    ref sparseBlocks t = ref _addr_t.val;

    var offset = t.offset[n];
    var header = t.values[offset];
    var lo = offset + 1;
    var hi = lo + uint16(header.lo);
    while (lo < hi) {
        var m = lo + (hi - lo) / 2;
        var r = t.values[m];
        if (r.lo <= b && b <= r.hi) {
            return r.value + uint16(b - r.lo) * header.value;
        }
        if (b < r.lo) {
            hi = m;
        }
        else
 {
            lo = m + 1;
        }
    }
    return 0;
}

} // end norm_package
