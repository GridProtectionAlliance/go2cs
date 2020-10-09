// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package hpack -- go2cs converted at 2020 October 09 06:06:52 UTC
// import "vendor/golang.org/x/net/http2/hpack" ==> using hpack = go.vendor.golang.org.x.net.http2.hpack_package
// Original source: C:\Go\src\vendor\golang.org\x\net\http2\hpack\huffman.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using io = go.io_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net {
namespace http2
{
    public static partial class hpack_package
    {
        private static sync.Pool bufPool = new sync.Pool(New:func()interface{}{returnnew(bytes.Buffer)},);

        // HuffmanDecode decodes the string in v and writes the expanded
        // result to w, returning the number of bytes written to w and the
        // Write call's return value. At most one Write call is made.
        public static (long, error) HuffmanDecode(io.Writer w, slice<byte> v) => func((defer, _, __) =>
        {
            long _p0 = default;
            error _p0 = default!;

            ptr<bytes.Buffer> buf = bufPool.Get()._<ptr<bytes.Buffer>>();
            buf.Reset();
            defer(bufPool.Put(buf));
            {
                var err = huffmanDecode(buf, 0L, v);

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            return w.Write(buf.Bytes());

        });

        // HuffmanDecodeToString decodes the string in v.
        public static (@string, error) HuffmanDecodeToString(slice<byte> v) => func((defer, _, __) =>
        {
            @string _p0 = default;
            error _p0 = default!;

            ptr<bytes.Buffer> buf = bufPool.Get()._<ptr<bytes.Buffer>>();
            buf.Reset();
            defer(bufPool.Put(buf));
            {
                var err = huffmanDecode(buf, 0L, v);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

            }

            return (buf.String(), error.As(null!)!);

        });

        // ErrInvalidHuffman is returned for errors found decoding
        // Huffman-encoded strings.
        public static var ErrInvalidHuffman = errors.New("hpack: invalid Huffman-encoded data");

        // huffmanDecode decodes v to buf.
        // If maxLen is greater than 0, attempts to write more to buf than
        // maxLen bytes will return ErrStringLength.
        private static error huffmanDecode(ptr<bytes.Buffer> _addr_buf, long maxLen, slice<byte> v)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;

            var rootHuffmanNode = getRootHuffmanNode();
            var n = rootHuffmanNode; 
            // cur is the bit buffer that has not been fed into n.
            // cbits is the number of low order bits in cur that are valid.
            // sbits is the number of bits of the symbol prefix being decoded.
            var cur = uint(0L);
            var cbits = uint8(0L);
            var sbits = uint8(0L);
            foreach (var (_, b) in v)
            {
                cur = cur << (int)(8L) | uint(b);
                cbits += 8L;
                sbits += 8L;
                while (cbits >= 8L)
                {
                    var idx = byte(cur >> (int)((cbits - 8L)));
                    n = n.children[idx];
                    if (n == null)
                    {
                        return error.As(ErrInvalidHuffman)!;
                    }

                    if (n.children == null)
                    {
                        if (maxLen != 0L && buf.Len() == maxLen)
                        {
                            return error.As(ErrStringLength)!;
                        }

                        buf.WriteByte(n.sym);
                        cbits -= n.codeLen;
                        n = rootHuffmanNode;
                        sbits = cbits;

                    }
                    else
                    {
                        cbits -= 8L;
                    }

                }


            }
            while (cbits > 0L)
            {
                n = n.children[byte(cur << (int)((8L - cbits)))];
                if (n == null)
                {
                    return error.As(ErrInvalidHuffman)!;
                }

                if (n.children != null || n.codeLen > cbits)
                {
                    break;
                }

                if (maxLen != 0L && buf.Len() == maxLen)
                {
                    return error.As(ErrStringLength)!;
                }

                buf.WriteByte(n.sym);
                cbits -= n.codeLen;
                n = rootHuffmanNode;
                sbits = cbits;

            }

            if (sbits > 7L)
            { 
                // Either there was an incomplete symbol, or overlong padding.
                // Both are decoding errors per RFC 7541 section 5.2.
                return error.As(ErrInvalidHuffman)!;

            }

            {
                var mask = uint(1L << (int)(cbits) - 1L);

                if (cur & mask != mask)
                { 
                    // Trailing bits must be a prefix of EOS per RFC 7541 section 5.2.
                    return error.As(ErrInvalidHuffman)!;

                }

            }


            return error.As(null!)!;

        }

        // incomparable is a zero-width, non-comparable type. Adding it to a struct
        // makes that struct also non-comparable, and generally doesn't add
        // any size (as long as it's first).
        private partial struct incomparable // : array<Action>
        {
        }

        private partial struct node
        {
            public incomparable _; // children is non-nil for internal nodes
            public ptr<array<ptr<node>>> children; // The following are only valid if children is nil:
            public byte codeLen; // number of bits that led to the output of sym
            public byte sym; // output symbol
        }

        private static ptr<node> newInternalNode()
        {
            return addr(new node(children:new([256]*node)));
        }

        private static sync.Once buildRootOnce = default;        private static ptr<node> lazyRootHuffmanNode;

        private static ptr<node> getRootHuffmanNode()
        {
            buildRootOnce.Do(buildRootHuffmanNode);
            return _addr_lazyRootHuffmanNode!;
        }

        private static void buildRootHuffmanNode() => func((_, panic, __) =>
        {
            if (len(huffmanCodes) != 256L)
            {
                panic("unexpected size");
            }

            lazyRootHuffmanNode = newInternalNode();
            foreach (var (i, code) in huffmanCodes)
            {
                addDecoderNode(byte(i), code, huffmanCodeLen[i]);
            }

        });

        private static void addDecoderNode(byte sym, uint code, byte codeLen)
        {
            var cur = lazyRootHuffmanNode;
            while (codeLen > 8L)
            {
                codeLen -= 8L;
                var i = uint8(code >> (int)(codeLen));
                if (cur.children[i] == null)
                {
                    cur.children[i] = newInternalNode();
                }

                cur = cur.children[i];

            }

            long shift = 8L - codeLen;
            var start = int(uint8(code << (int)(shift)));
            var end = int(1L << (int)(shift));
            {
                var i__prev1 = i;

                for (i = start; i < start + end; i++)
                {
                    cur.children[i] = addr(new node(sym:sym,codeLen:codeLen));
                }


                i = i__prev1;
            }

        }

        // AppendHuffmanString appends s, as encoded in Huffman codes, to dst
        // and returns the extended buffer.
        public static slice<byte> AppendHuffmanString(slice<byte> dst, @string s)
        {
            var rembits = uint8(8L);

            for (long i = 0L; i < len(s); i++)
            {
                if (rembits == 8L)
                {
                    dst = append(dst, 0L);
                }

                dst, rembits = appendByteToHuffmanCode(dst, rembits, s[i]);

            }


            if (rembits < 8L)
            { 
                // special EOS symbol
                var code = uint32(0x3fffffffUL);
                var nbits = uint8(30L);

                var t = uint8(code >> (int)((nbits - rembits)));
                dst[len(dst) - 1L] |= t;

            }

            return dst;

        }

        // HuffmanEncodeLength returns the number of bytes required to encode
        // s in Huffman codes. The result is round up to byte boundary.
        public static ulong HuffmanEncodeLength(@string s)
        {
            var n = uint64(0L);
            for (long i = 0L; i < len(s); i++)
            {
                n += uint64(huffmanCodeLen[s[i]]);
            }

            return (n + 7L) / 8L;

        }

        // appendByteToHuffmanCode appends Huffman code for c to dst and
        // returns the extended buffer and the remaining bits in the last
        // element. The appending is not byte aligned and the remaining bits
        // in the last element of dst is given in rembits.
        private static (slice<byte>, byte) appendByteToHuffmanCode(slice<byte> dst, byte rembits, byte c)
        {
            slice<byte> _p0 = default;
            byte _p0 = default;

            var code = huffmanCodes[c];
            var nbits = huffmanCodeLen[c];

            while (true)
            {
                if (rembits > nbits)
                {
                    var t = uint8(code << (int)((rembits - nbits)));
                    dst[len(dst) - 1L] |= t;
                    rembits -= nbits;
                    break;
                }

                t = uint8(code >> (int)((nbits - rembits)));
                dst[len(dst) - 1L] |= t;

                nbits -= rembits;
                rembits = 8L;

                if (nbits == 0L)
                {
                    break;
                }

                dst = append(dst, 0L);

            }


            return (dst, rembits);

        }
    }
}}}}}}
