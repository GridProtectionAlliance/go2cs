// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2022 March 06 22:15:04 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Program Files\Go\src\compress\flate\huffman_code.go
using math = go.math_package;
using bits = go.math.bits_package;
using sort = go.sort_package;

namespace go.compress;

public static partial class flate_package {

    // hcode is a huffman code with a bit code and bit length.
private partial struct hcode {
    public ushort code;
    public ushort len;
}

private partial struct huffmanEncoder {
    public slice<hcode> codes;
    public slice<literalNode> freqcache;
    public array<int> bitCount;
    public byLiteral lns; // stored to avoid repeated allocation in generate
    public byFreq lfs; // stored to avoid repeated allocation in generate
}

private partial struct literalNode {
    public ushort literal;
    public int freq;
}

// A levelInfo describes the state of the constructed tree for a given depth.
private partial struct levelInfo {
    public int level; // The frequency of the last node at this level
    public int lastFreq; // The frequency of the next character to add to this level
    public int nextCharFreq; // The frequency of the next pair (from level below) to add to this level.
// Only valid if the "needed" value of the next lower level is 0.
    public int nextPairFreq; // The number of chains remaining to generate for this level before moving
// up to the next level
    public int needed;
}

// set sets the code and length of an hcode.
private static void set(this ptr<hcode> _addr_h, ushort code, ushort length) {
    ref hcode h = ref _addr_h.val;

    h.len = length;
    h.code = code;
}

private static literalNode maxNode() {
    return new literalNode(math.MaxUint16,math.MaxInt32);
}

private static ptr<huffmanEncoder> newHuffmanEncoder(nint size) {
    return addr(new huffmanEncoder(codes:make([]hcode,size)));
}

// Generates a HuffmanCode corresponding to the fixed literal table
private static ptr<huffmanEncoder> generateFixedLiteralEncoding() {
    var h = newHuffmanEncoder(maxNumLit);
    var codes = h.codes;
    ushort ch = default;
    for (ch = 0; ch < maxNumLit; ch++) {
        ushort bits = default;
        ushort size = default;

        if (ch < 144) 
            // size 8, 000110000  .. 10111111
            bits = ch + 48;
            size = 8;
            break;
        else if (ch < 256) 
            // size 9, 110010000 .. 111111111
            bits = ch + 400 - 144;
            size = 9;
            break;
        else if (ch < 280) 
            // size 7, 0000000 .. 0010111
            bits = ch - 256;
            size = 7;
            break;
        else 
            // size 8, 11000000 .. 11000111
            bits = ch + 192 - 280;
            size = 8;
                codes[ch] = new hcode(code:reverseBits(bits,byte(size)),len:size);

    }
    return _addr_h!;

}

private static ptr<huffmanEncoder> generateFixedOffsetEncoding() {
    var h = newHuffmanEncoder(30);
    var codes = h.codes;
    foreach (var (ch) in codes) {
        codes[ch] = new hcode(code:reverseBits(uint16(ch),5),len:5);
    }    return _addr_h!;
}

private static ptr<huffmanEncoder> fixedLiteralEncodinggenerateFixedLiteralEncoding();
private static ptr<huffmanEncoder> fixedOffsetEncodinggenerateFixedOffsetEncoding();

private static nint bitLength(this ptr<huffmanEncoder> _addr_h, slice<int> freq) {
    ref huffmanEncoder h = ref _addr_h.val;

    nint total = default;
    foreach (var (i, f) in freq) {
        if (f != 0) {
            total += int(f) * int(h.codes[i].len);
        }
    }    return total;

}

private static readonly nint maxBitsLimit = 16;

// Return the number of literals assigned to each bit size in the Huffman encoding
//
// This method is only called when list.length >= 3
// The cases of 0, 1, and 2 literals are handled by special case code.
//
// list  An array of the literals with non-zero frequencies
//             and their associated frequencies. The array is in order of increasing
//             frequency, and has as its last element a special element with frequency
//             MaxInt32
// maxBits     The maximum number of bits that should be used to encode any literal.
//             Must be less than 16.
// return      An integer array in which array[i] indicates the number of literals
//             that should be encoded in i bits.


// Return the number of literals assigned to each bit size in the Huffman encoding
//
// This method is only called when list.length >= 3
// The cases of 0, 1, and 2 literals are handled by special case code.
//
// list  An array of the literals with non-zero frequencies
//             and their associated frequencies. The array is in order of increasing
//             frequency, and has as its last element a special element with frequency
//             MaxInt32
// maxBits     The maximum number of bits that should be used to encode any literal.
//             Must be less than 16.
// return      An integer array in which array[i] indicates the number of literals
//             that should be encoded in i bits.
private static slice<int> bitCounts(this ptr<huffmanEncoder> _addr_h, slice<literalNode> list, int maxBits) => func((_, panic, _) => {
    ref huffmanEncoder h = ref _addr_h.val;

    if (maxBits >= maxBitsLimit) {
        panic("flate: maxBits too large");
    }
    var n = int32(len(list));
    list = list[(int)0..(int)n + 1];
    list[n] = maxNode(); 

    // The tree can't have greater depth than n - 1, no matter what. This
    // saves a little bit of work in some small cases
    if (maxBits > n - 1) {
        maxBits = n - 1;
    }
    array<levelInfo> levels = new array<levelInfo>(maxBitsLimit); 
    // leafCounts[i] counts the number of literals at the left
    // of ancestors of the rightmost node at level i.
    // leafCounts[i][j] is the number of literals at the left
    // of the level j ancestor.
    array<array<int>> leafCounts = new array<array<int>>(maxBitsLimit);

    {
        var level__prev1 = level;

        for (var level = int32(1); level <= maxBits; level++) { 
            // For every level, the first two items are the first two characters.
            // We initialize the levels as if we had already figured this out.
            levels[level] = new levelInfo(level:level,lastFreq:list[1].freq,nextCharFreq:list[2].freq,nextPairFreq:list[0].freq+list[1].freq,);
            leafCounts[level][level] = 2;
            if (level == 1) {
                levels[level].nextPairFreq = math.MaxInt32;
            }

        }

        level = level__prev1;
    } 

    // We need a total of 2*n - 2 items at top level and have already generated 2.
    levels[maxBits].needed = 2 * n - 4;

    level = maxBits;
    while (true) {
        var l = _addr_levels[level];
        if (l.nextPairFreq == math.MaxInt32 && l.nextCharFreq == math.MaxInt32) { 
            // We've run out of both leafs and pairs.
            // End all calculations for this level.
            // To make sure we never come back to this level or any lower level,
            // set nextPairFreq impossibly large.
            l.needed = 0;
            levels[level + 1].nextPairFreq = math.MaxInt32;
            level++;
            continue;

        }
        var prevFreq = l.lastFreq;
        if (l.nextCharFreq < l.nextPairFreq) { 
            // The next item on this row is a leaf node.
            n = leafCounts[level][level] + 1;
            l.lastFreq = l.nextCharFreq; 
            // Lower leafCounts are the same of the previous node.
            leafCounts[level][level] = n;
            l.nextCharFreq = list[n].freq;

        }
        else
 { 
            // The next item on this row is a pair from the previous row.
            // nextPairFreq isn't valid until we generate two
            // more values in the level below
            l.lastFreq = l.nextPairFreq; 
            // Take leaf counts from the lower level, except counts[level] remains the same.
            copy(leafCounts[level][..(int)level], leafCounts[level - 1][..(int)level]);
            levels[l.level - 1].needed = 2;

        }
        l.needed--;

        if (l.needed == 0) { 
            // We've done everything we need to do for this level.
            // Continue calculating one level up. Fill in nextPairFreq
            // of that level with the sum of the two nodes we've just calculated on
            // this level.
            if (l.level == maxBits) { 
                // All done!
                break;

            }

            levels[l.level + 1].nextPairFreq = prevFreq + l.lastFreq;
            level++;

        }
        else
 { 
            // If we stole from below, move down temporarily to replenish it.
            while (levels[level - 1].needed > 0) {
                level--;
            }


        }
    } 

    // Somethings is wrong if at the end, the top level is null or hasn't used
    // all of the leaves.
    if (leafCounts[maxBits][maxBits] != n) {
        panic("leafCounts[maxBits][maxBits] != n");
    }
    var bitCount = h.bitCount[..(int)maxBits + 1];
    nint bits = 1;
    var counts = _addr_leafCounts[maxBits];
    {
        var level__prev1 = level;

        for (level = maxBits; level > 0; level--) { 
            // chain.leafCount gives the number of literals requiring at least "bits"
            // bits to encode.
            bitCount[bits] = counts[level] - counts[level - 1];
            bits++;

        }

        level = level__prev1;
    }
    return bitCount;

});

// Look at the leaves and assign them a bit count and an encoding as specified
// in RFC 1951 3.2.2
private static void assignEncodingAndSize(this ptr<huffmanEncoder> _addr_h, slice<int> bitCount, slice<literalNode> list) {
    ref huffmanEncoder h = ref _addr_h.val;

    var code = uint16(0);
    foreach (var (n, bits) in bitCount) {
        code<<=1;
        if (n == 0 || bits == 0) {
            continue;
        }
        var chunk = list[(int)len(list) - int(bits)..];

        h.lns.sort(chunk);
        foreach (var (_, node) in chunk) {
            h.codes[node.literal] = new hcode(code:reverseBits(code,uint8(n)),len:uint16(n));
            code++;
        }        list = list[(int)0..(int)len(list) - int(bits)];

    }
}

// Update this Huffman Code object to be the minimum code for the specified frequency count.
//
// freq  An array of frequencies, in which frequency[i] gives the frequency of literal i.
// maxBits  The maximum number of bits to use for any literal.
private static void generate(this ptr<huffmanEncoder> _addr_h, slice<int> freq, int maxBits) {
    ref huffmanEncoder h = ref _addr_h.val;

    if (h.freqcache == null) { 
        // Allocate a reusable buffer with the longest possible frequency table.
        // Possible lengths are codegenCodeCount, offsetCodeCount and maxNumLit.
        // The largest of these is maxNumLit, so we allocate for that case.
        h.freqcache = make_slice<literalNode>(maxNumLit + 1);

    }
    var list = h.freqcache[..(int)len(freq) + 1]; 
    // Number of non-zero literals
    nint count = 0; 
    // Set list to be the set of all non-zero literals and their frequencies
    {
        var i__prev1 = i;

        foreach (var (__i, __f) in freq) {
            i = __i;
            f = __f;
            if (f != 0) {
                list[count] = new literalNode(uint16(i),f);
                count++;
            }
            else
 {
                list[count] = new literalNode();
                h.codes[i].len = 0;
            }

        }
        i = i__prev1;
    }

    list[len(freq)] = new literalNode();

    list = list[..(int)count];
    if (count <= 2) { 
        // Handle the small cases here, because they are awkward for the general case code. With
        // two or fewer literals, everything has bit length 1.
        {
            var i__prev1 = i;

            foreach (var (__i, __node) in list) {
                i = __i;
                node = __node; 
                // "list" is in order of increasing literal value.
                h.codes[node.literal].set(uint16(i), 1);

            }

            i = i__prev1;
        }

        return ;

    }
    h.lfs.sort(list); 

    // Get the number of literals for each bit count
    var bitCount = h.bitCounts(list, maxBits); 
    // And do the assignment
    h.assignEncodingAndSize(bitCount, list);

}

private partial struct byLiteral { // : slice<literalNode>
}

private static void sort(this ptr<byLiteral> _addr_s, slice<literalNode> a) {
    ref byLiteral s = ref _addr_s.val;

    s.val = byLiteral(a);
    sort.Sort(s);
}

private static nint Len(this byLiteral s) {
    return len(s);
}

private static bool Less(this byLiteral s, nint i, nint j) {
    return s[i].literal < s[j].literal;
}

private static void Swap(this byLiteral s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

private partial struct byFreq { // : slice<literalNode>
}

private static void sort(this ptr<byFreq> _addr_s, slice<literalNode> a) {
    ref byFreq s = ref _addr_s.val;

    s.val = byFreq(a);
    sort.Sort(s);
}

private static nint Len(this byFreq s) {
    return len(s);
}

private static bool Less(this byFreq s, nint i, nint j) {
    if (s[i].freq == s[j].freq) {
        return s[i].literal < s[j].literal;
    }
    return s[i].freq < s[j].freq;

}

private static void Swap(this byFreq s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

private static ushort reverseBits(ushort number, byte bitLength) {
    return bits.Reverse16(number << (int)((16 - bitLength)));
}

} // end flate_package
