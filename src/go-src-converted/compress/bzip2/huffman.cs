// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2020 August 29 10:10:44 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Go\src\compress\bzip2\huffman.go
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace compress
{
    public static partial class bzip2_package
    {
        // A huffmanTree is a binary tree which is navigated, bit-by-bit to reach a
        // symbol.
        private partial struct huffmanTree
        {
            public slice<huffmanNode> nodes;
            public long nextNode;
        }

        // A huffmanNode is a node in the tree. left and right contain indexes into the
        // nodes slice of the tree. If left or right is invalidNodeValue then the child
        // is a left node and its value is in leftValue/rightValue.
        //
        // The symbols are uint16s because bzip2 encodes not only MTF indexes in the
        // tree, but also two magic values for run-length encoding and an EOF symbol.
        // Thus there are more than 256 possible symbols.
        private partial struct huffmanNode
        {
            public ushort left;
            public ushort right;
            public ushort leftValue;
            public ushort rightValue;
        }

        // invalidNodeValue is an invalid index which marks a leaf node in the tree.
        private static readonly ulong invalidNodeValue = 0xffffUL;

        // Decode reads bits from the given bitReader and navigates the tree until a
        // symbol is found.


        // Decode reads bits from the given bitReader and navigates the tree until a
        // symbol is found.
        private static ushort Decode(this ref huffmanTree t, ref bitReader br)
        {
            var nodeIndex = uint16(0L); // node 0 is the root of the tree.

            while (true)
            {
                var node = ref t.nodes[nodeIndex];

                ushort bit = default;
                if (br.bits > 0L)
                { 
                    // Get next bit - fast path.
                    br.bits--;
                    bit = 0L - (uint16(br.n >> (int)(br.bits)) & 1L);
                }
                else
                { 
                    // Get next bit - slow path.
                    // Use ReadBits to retrieve a single bit
                    // from the underling io.ByteReader.
                    bit = 0L - uint16(br.ReadBits(1L));
                } 
                // now
                // bit = 0xffff if the next bit was 1
                // bit = 0x0000 if the next bit was 0

                // 1 means left, 0 means right.
                //
                // if bit == 0xffff {
                //     nodeIndex = node.left
                // } else {
                //     nodeIndex = node.right
                // }
                nodeIndex = (bit & node.left) | (~bit & node.right);

                if (nodeIndex == invalidNodeValue)
                { 
                    // We found a leaf. Use the value of bit to decide
                    // whether is a left or a right value.
                    return (bit & node.leftValue) | (~bit & node.rightValue);
                }
            }

        }

        // newHuffmanTree builds a Huffman tree from a slice containing the code
        // lengths of each symbol. The maximum code length is 32 bits.
        private static (huffmanTree, error) newHuffmanTree(slice<byte> lengths) => func((_, panic, __) =>
        { 
            // There are many possible trees that assign the same code length to
            // each symbol (consider reflecting a tree down the middle, for
            // example). Since the code length assignments determine the
            // efficiency of the tree, each of these trees is equally good. In
            // order to minimize the amount of information needed to build a tree
            // bzip2 uses a canonical tree so that it can be reconstructed given
            // only the code length assignments.

            if (len(lengths) < 2L)
            {
                panic("newHuffmanTree: too few symbols");
            }
            huffmanTree t = default; 

            // First we sort the code length assignments by ascending code length,
            // using the symbol value to break ties.
            var pairs = huffmanSymbolLengthPairs(make_slice<huffmanSymbolLengthPair>(len(lengths)));
            {
                var i__prev1 = i;
                var length__prev1 = length;

                foreach (var (__i, __length) in lengths)
                {
                    i = __i;
                    length = __length;
                    pairs[i].value = uint16(i);
                    pairs[i].length = length;
                }

                i = i__prev1;
                length = length__prev1;
            }

            sort.Sort(pairs); 

            // Now we assign codes to the symbols, starting with the longest code.
            // We keep the codes packed into a uint32, at the most-significant end.
            // So branches are taken from the MSB downwards. This makes it easy to
            // sort them later.
            var code = uint32(0L);
            var length = uint8(32L);

            var codes = huffmanCodes(make_slice<huffmanCode>(len(lengths)));
            {
                var i__prev1 = i;

                for (var i = len(pairs) - 1L; i >= 0L; i--)
                {
                    if (length > pairs[i].length)
                    {
                        length = pairs[i].length;
                    }
                    codes[i].code = code;
                    codes[i].codeLen = length;
                    codes[i].value = pairs[i].value; 
                    // We need to 'increment' the code, which means treating |code|
                    // like a |length| bit number.
                    code += 1L << (int)((32L - length));
                } 

                // Now we can sort by the code so that the left half of each branch are
                // grouped together, recursively.


                i = i__prev1;
            } 

            // Now we can sort by the code so that the left half of each branch are
            // grouped together, recursively.
            sort.Sort(codes);

            t.nodes = make_slice<huffmanNode>(len(codes));
            var (_, err) = buildHuffmanNode(ref t, codes, 0L);
            return (t, err);
        });

        // huffmanSymbolLengthPair contains a symbol and its code length.
        private partial struct huffmanSymbolLengthPair
        {
            public ushort value;
            public byte length;
        }

        // huffmanSymbolLengthPair is used to provide an interface for sorting.
        private partial struct huffmanSymbolLengthPairs // : slice<huffmanSymbolLengthPair>
        {
        }

        private static long Len(this huffmanSymbolLengthPairs h)
        {
            return len(h);
        }

        private static bool Less(this huffmanSymbolLengthPairs h, long i, long j)
        {
            if (h[i].length < h[j].length)
            {
                return true;
            }
            if (h[i].length > h[j].length)
            {
                return false;
            }
            if (h[i].value < h[j].value)
            {
                return true;
            }
            return false;
        }

        private static void Swap(this huffmanSymbolLengthPairs h, long i, long j)
        {
            h[i] = h[j];
            h[j] = h[i];
        }

        // huffmanCode contains a symbol, its code and code length.
        private partial struct huffmanCode
        {
            public uint code;
            public byte codeLen;
            public ushort value;
        }

        // huffmanCodes is used to provide an interface for sorting.
        private partial struct huffmanCodes // : slice<huffmanCode>
        {
        }

        private static long Len(this huffmanCodes n)
        {
            return len(n);
        }

        private static bool Less(this huffmanCodes n, long i, long j)
        {
            return n[i].code < n[j].code;
        }

        private static void Swap(this huffmanCodes n, long i, long j)
        {
            n[i] = n[j];
            n[j] = n[i];
        }

        // buildHuffmanNode takes a slice of sorted huffmanCodes and builds a node in
        // the Huffman tree at the given level. It returns the index of the newly
        // constructed node.
        private static (ushort, error) buildHuffmanNode(ref huffmanTree t, slice<huffmanCode> codes, uint level)
        {
            var test = uint32(1L) << (int)((31L - level)); 

            // We have to search the list of codes to find the divide between the left and right sides.
            var firstRightIndex = len(codes);
            foreach (var (i, code) in codes)
            {
                if (code.code & test != 0L)
                {
                    firstRightIndex = i;
                    break;
                }
            }
            var left = codes[..firstRightIndex];
            var right = codes[firstRightIndex..];

            if (len(left) == 0L || len(right) == 0L)
            { 
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
                if (len(codes) < 2L)
                {
                    return (0L, StructuralError("empty Huffman tree"));
                } 

                // In this case the recursion doesn't always reduce the length
                // of codes so we need to ensure termination via another
                // mechanism.
                if (level == 31L)
                { 
                    // Since len(codes) >= 2 the only way that the values
                    // can match at all 32 bits is if they are equal, which
                    // is invalid. This ensures that we never enter
                    // infinite recursion.
                    return (0L, StructuralError("equal symbols in Huffman tree"));
                }
                if (len(left) == 0L)
                {
                    return buildHuffmanNode(t, right, level + 1L);
                }
                return buildHuffmanNode(t, left, level + 1L);
            }
            nodeIndex = uint16(t.nextNode);
            var node = ref t.nodes[t.nextNode];
            t.nextNode++;

            if (len(left) == 1L)
            { 
                // leaf node
                node.left = invalidNodeValue;
                node.leftValue = left[0L].value;
            }
            else
            {
                node.left, err = buildHuffmanNode(t, left, level + 1L);
            }
            if (err != null)
            {
                return;
            }
            if (len(right) == 1L)
            { 
                // leaf node
                node.right = invalidNodeValue;
                node.rightValue = right[0L].value;
            }
            else
            {
                node.right, err = buildHuffmanNode(t, right, level + 1L);
            }
            return;
        }
    }
}}
