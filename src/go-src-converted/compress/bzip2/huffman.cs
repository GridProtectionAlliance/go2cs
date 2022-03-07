// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2022 March 06 23:35:24 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Program Files\Go\src\compress\bzip2\huffman.go
using sort = go.sort_package;
using System;


namespace go.compress;

public static partial class bzip2_package {

    // A huffmanTree is a binary tree which is navigated, bit-by-bit to reach a
    // symbol.
private partial struct huffmanTree {
    public slice<huffmanNode> nodes;
    public nint nextNode;
}

// A huffmanNode is a node in the tree. left and right contain indexes into the
// nodes slice of the tree. If left or right is invalidNodeValue then the child
// is a left node and its value is in leftValue/rightValue.
//
// The symbols are uint16s because bzip2 encodes not only MTF indexes in the
// tree, but also two magic values for run-length encoding and an EOF symbol.
// Thus there are more than 256 possible symbols.
private partial struct huffmanNode {
    public ushort left;
    public ushort right;
    public ushort leftValue;
    public ushort rightValue;
}

// invalidNodeValue is an invalid index which marks a leaf node in the tree.
private static readonly nuint invalidNodeValue = 0xffff;

// Decode reads bits from the given bitReader and navigates the tree until a
// symbol is found.


// Decode reads bits from the given bitReader and navigates the tree until a
// symbol is found.
private static ushort Decode(this ptr<huffmanTree> _addr_t, ptr<bitReader> _addr_br) {
    ushort v = default;
    ref huffmanTree t = ref _addr_t.val;
    ref bitReader br = ref _addr_br.val;

    var nodeIndex = uint16(0); // node 0 is the root of the tree.

    while (true) {
        var node = _addr_t.nodes[nodeIndex];

        ushort bit = default;
        if (br.bits > 0) { 
            // Get next bit - fast path.
            br.bits--;
            bit = uint16(br.n >> (int)((br.bits & 63))) & 1;

        }
        else
 { 
            // Get next bit - slow path.
            // Use ReadBits to retrieve a single bit
            // from the underling io.ByteReader.
            bit = uint16(br.ReadBits(1));

        }
        var l = node.left;
        var r = node.right;

        if (bit == 1) {
            nodeIndex = l;
        }
        else
 {
            nodeIndex = r;
        }
        if (nodeIndex == invalidNodeValue) { 
            // We found a leaf. Use the value of bit to decide
            // whether is a left or a right value.
            l = node.leftValue;
            r = node.rightValue;
            if (bit == 1) {
                v = l;
            }
            else
 {
                v = r;
            }

            return ;

        }
    }

}

// newHuffmanTree builds a Huffman tree from a slice containing the code
// lengths of each symbol. The maximum code length is 32 bits.
private static (huffmanTree, error) newHuffmanTree(slice<byte> lengths) => func((_, panic, _) => {
    huffmanTree _p0 = default;
    error _p0 = default!;
 
    // There are many possible trees that assign the same code length to
    // each symbol (consider reflecting a tree down the middle, for
    // example). Since the code length assignments determine the
    // efficiency of the tree, each of these trees is equally good. In
    // order to minimize the amount of information needed to build a tree
    // bzip2 uses a canonical tree so that it can be reconstructed given
    // only the code length assignments.

    if (len(lengths) < 2) {
        panic("newHuffmanTree: too few symbols");
    }
    ref huffmanTree t = ref heap(out ptr<huffmanTree> _addr_t); 

    // First we sort the code length assignments by ascending code length,
    // using the symbol value to break ties.
    var pairs = make_slice<huffmanSymbolLengthPair>(len(lengths));
    {
        var i__prev1 = i;
        var length__prev1 = length;

        foreach (var (__i, __length) in lengths) {
            i = __i;
            length = __length;
            pairs[i].value = uint16(i);
            pairs[i].length = length;
        }
        i = i__prev1;
        length = length__prev1;
    }

    sort.Slice(pairs, (i, j) => {
        if (pairs[i].length < pairs[j].length) {
            return true;
        }
        if (pairs[i].length > pairs[j].length) {
            return false;
        }
        if (pairs[i].value < pairs[j].value) {
            return true;
        }
        return false;

    }); 

    // Now we assign codes to the symbols, starting with the longest code.
    // We keep the codes packed into a uint32, at the most-significant end.
    // So branches are taken from the MSB downwards. This makes it easy to
    // sort them later.
    var code = uint32(0);
    var length = uint8(32);

    var codes = make_slice<huffmanCode>(len(lengths));
    {
        var i__prev1 = i;

        for (var i = len(pairs) - 1; i >= 0; i--) {
            if (length > pairs[i].length) {
                length = pairs[i].length;
            }
            codes[i].code = code;
            codes[i].codeLen = length;
            codes[i].value = pairs[i].value; 
            // We need to 'increment' the code, which means treating |code|
            // like a |length| bit number.
            code += 1 << (int)((32 - length));

        }

        i = i__prev1;
    } 

    // Now we can sort by the code so that the left half of each branch are
    // grouped together, recursively.
    sort.Slice(codes, (i, j) => {
        return codes[i].code < codes[j].code;
    });

    t.nodes = make_slice<huffmanNode>(len(codes));
    var (_, err) = buildHuffmanNode(_addr_t, codes, 0);
    return (t, error.As(err)!);

});

// huffmanSymbolLengthPair contains a symbol and its code length.
private partial struct huffmanSymbolLengthPair {
    public ushort value;
    public byte length;
}

// huffmanCode contains a symbol, its code and code length.
private partial struct huffmanCode {
    public uint code;
    public byte codeLen;
    public ushort value;
}

// buildHuffmanNode takes a slice of sorted huffmanCodes and builds a node in
// the Huffman tree at the given level. It returns the index of the newly
// constructed node.
private static (ushort, error) buildHuffmanNode(ptr<huffmanTree> _addr_t, slice<huffmanCode> codes, uint level) {
    ushort nodeIndex = default;
    error err = default!;
    ref huffmanTree t = ref _addr_t.val;

    var test = uint32(1) << (int)((31 - level)); 

    // We have to search the list of codes to find the divide between the left and right sides.
    var firstRightIndex = len(codes);
    foreach (var (i, code) in codes) {
        if (code.code & test != 0) {
            firstRightIndex = i;
            break;
        }
    }    var left = codes[..(int)firstRightIndex];
    var right = codes[(int)firstRightIndex..];

    if (len(left) == 0 || len(right) == 0) { 
        // There is a superfluous level in the Huffman tree indicating
        // a bug in the encoder. However, this bug has been observed in
        // the wild so we handle it.

        // If this function was called recursively then we know that
        // len(codes) >= 2 because, otherwise, we would have hit the
        // "leaf node" case, below, and not recursed.
        //
        // However, for the initial call it's possible that len(codes)
        // is zero or one. Both cases are invalid because a zero length
        // tree cannot encode anything and a length-1 tree can only
        // encode EOF and so is superfluous. We reject both.
        if (len(codes) < 2) {
            return (0, error.As(StructuralError("empty Huffman tree"))!);
        }
        if (level == 31) { 
            // Since len(codes) >= 2 the only way that the values
            // can match at all 32 bits is if they are equal, which
            // is invalid. This ensures that we never enter
            // infinite recursion.
            return (0, error.As(StructuralError("equal symbols in Huffman tree"))!);

        }
        if (len(left) == 0) {
            return buildHuffmanNode(_addr_t, right, level + 1);
        }
        return buildHuffmanNode(_addr_t, left, level + 1);

    }
    nodeIndex = uint16(t.nextNode);
    var node = _addr_t.nodes[t.nextNode];
    t.nextNode++;

    if (len(left) == 1) { 
        // leaf node
        node.left = invalidNodeValue;
        node.leftValue = left[0].value;

    }
    else
 {
        node.left, err = buildHuffmanNode(_addr_t, left, level + 1);
    }
    if (err != null) {
        return ;
    }
    if (len(right) == 1) { 
        // leaf node
        node.right = invalidNodeValue;
        node.rightValue = right[0].value;

    }
    else
 {
        node.right, err = buildHuffmanNode(_addr_t, right, level + 1);
    }
    return ;

}

} // end bzip2_package
