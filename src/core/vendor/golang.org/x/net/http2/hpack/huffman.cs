// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net.http2;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using sync = sync_package;

partial class hpack_package {

internal static sync.Pool bufPool = new sync.Pool(
    New: () => @new<bytes.Buffer>()
);

// HuffmanDecode decodes the string in v and writes the expanded
// result to w, returning the number of bytes written to w and the
// Write call's return value. At most one Write call is made.
public static (nint, error) HuffmanDecode(io.Writer w, slice<byte> v) => func((defer, _) => {
    var buf = bufPool.Get()._<ж<bytes.Buffer>>();
    buf.Reset();
    var bufPoolʗ1 = bufPool;
    deferǃ(bufPoolʗ1.Put, buf, defer);
    {
        var err = huffmanDecode(buf, 0, v); if (err != default!) {
            return (0, err);
        }
    }
    return w.Write(buf.Bytes());
});

// HuffmanDecodeToString decodes the string in v.
public static (@string, error) HuffmanDecodeToString(slice<byte> v) => func((defer, _) => {
    var buf = bufPool.Get()._<ж<bytes.Buffer>>();
    buf.Reset();
    var bufPoolʗ1 = bufPool;
    deferǃ(bufPoolʗ1.Put, buf, defer);
    {
        var err = huffmanDecode(buf, 0, v); if (err != default!) {
            return ("", err);
        }
    }
    return (buf.String(), default!);
});

// ErrInvalidHuffman is returned for errors found decoding
// Huffman-encoded strings.
public static error ErrInvalidHuffman = errors.New("hpack: invalid Huffman-encoded data"u8);

// huffmanDecode decodes v to buf.
// If maxLen is greater than 0, attempts to write more to buf than
// maxLen bytes will return ErrStringLength.
internal static error huffmanDecode(ж<bytes.Buffer> Ꮡbuf, nint maxLen, slice<byte> v) {
    ref var buf = ref Ꮡbuf.val;

    var rootHuffmanNode = getRootHuffmanNode();
    var n = rootHuffmanNode;
    // cur is the bit buffer that has not been fed into n.
    // cbits is the number of low order bits in cur that are valid.
    // sbits is the number of bits of the symbol prefix being decoded.
    nuint cur = ((nuint)0);
    var cbits = ((uint8)0);
    var sbits = ((uint8)0);
    foreach (var (_, b) in v) {
        cur = (nuint)(cur << (int)(8) | ((nuint)b));
        cbits += 8;
        sbits += 8;
        while (cbits >= 8) {
            var idx = ((byte)(cur >> (int)((cbits - 8))));
            n = (~n).children[idx];
            if (n == nil) {
                return ErrInvalidHuffman;
            }
            if ((~n).children == nil){
                if (maxLen != 0 && buf.Len() == maxLen) {
                    return ErrStringLength;
                }
                buf.WriteByte((~n).sym);
                cbits -= n.val.codeLen;
                n = rootHuffmanNode;
                sbits = cbits;
            } else {
                cbits -= 8;
            }
        }
    }
    while (cbits > 0) {
        n = (~n).children[((byte)(cur << (int)((8 - cbits))))];
        if (n == nil) {
            return ErrInvalidHuffman;
        }
        if ((~n).children != nil || (~n).codeLen > cbits) {
            break;
        }
        if (maxLen != 0 && buf.Len() == maxLen) {
            return ErrStringLength;
        }
        buf.WriteByte((~n).sym);
        cbits -= n.val.codeLen;
        n = rootHuffmanNode;
        sbits = cbits;
    }
    if (sbits > 7) {
        // Either there was an incomplete symbol, or overlong padding.
        // Both are decoding errors per RFC 7541 section 5.2.
        return ErrInvalidHuffman;
    }
    {
        nuint mask = ((nuint)(1 << (int)(cbits) - 1)); if ((nuint)(cur & mask) != mask) {
            // Trailing bits must be a prefix of EOS per RFC 7541 section 5.2.
            return ErrInvalidHuffman;
        }
    }
    return default!;
}
// [...]func()

[GoType] partial struct node {
    internal incomparable _;
    // children is non-nil for internal nodes
    internal ж<array<ж<node>>> children;
    // The following are only valid if children is nil:
    internal uint8 codeLen; // number of bits that led to the output of sym
    internal byte sym;  // output symbol
}

internal static ж<node> newInternalNode() {
    return Ꮡ(new node(children: @new<array<ж<node>>>()));
}

internal static sync.Once buildRootOnce;
internal static ж<node> lazyRootHuffmanNode;

internal static ж<node> getRootHuffmanNode() {
    buildRootOnce.Do(buildRootHuffmanNode);
    return lazyRootHuffmanNode;
}

internal static void buildRootHuffmanNode() {
    if (len(huffmanCodes) != 256) {
        throw panic("unexpected size");
    }
    lazyRootHuffmanNode = newInternalNode();
    // allocate a leaf node for each of the 256 symbols
    var leaves = @new<array<node>>();
    foreach (var (sym, code) in huffmanCodes) {
        var codeLen = huffmanCodeLen[sym];
        var cur = lazyRootHuffmanNode;
        while (codeLen > 8) {
            codeLen -= 8;
            var i = ((uint8)(code >> (int)(codeLen)));
            if ((~cur).children[i] == nil) {
                (~cur).children[i] = newInternalNode();
            }
            cur = (~cur).children[i];
        }
        var shift = 8 - codeLen;
        nint start = ((nint)((uint8)(code << (int)(shift))));
        nint end = ((nint)(1 << (int)(shift)));
        leaves[sym].sym = ((byte)sym);
        leaves[sym].codeLen = codeLen;
        for (nint i = start; i < start + end; i++) {
            (~cur).children[i] = Ꮡ(leaves[sym]);
        }
    }
}

// AppendHuffmanString appends s, as encoded in Huffman codes, to dst
// and returns the extended buffer.
public static slice<byte> AppendHuffmanString(slice<byte> dst, @string s) {
    // This relies on the maximum huffman code length being 30 (See tables.go huffmanCodeLen array)
    // So if a uint64 buffer has less than 32 valid bits can always accommodate another huffmanCode.
    uint64 x = default!;             // buffer
    
    nuint n = default!;           // number valid of bits present in x
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        n += ((nuint)huffmanCodeLen[c]);
        x <<= (uint8)(huffmanCodeLen[c] % 64);
        x |= (uint64)(((uint64)huffmanCodes[c]));
        if (n >= 32) {
            n %= 32;
            // Normally would be -= 32 but %= 32 informs compiler 0 <= n <= 31 for upcoming shift
            var yΔ1 = ((uint32)(x >> (int)(n)));
            // Compiler doesn't combine memory writes if y isn't uint32
            dst = append(dst, ((byte)(yΔ1 >> (int)(24))), ((byte)(yΔ1 >> (int)(16))), ((byte)(yΔ1 >> (int)(8))), ((byte)yΔ1));
        }
    }
    // Add padding bits if necessary
    {
        nuint over = n % 8; if (over > 0) {
            static readonly UntypedInt eosCode = /* 0x3fffffff */ 1073741823;
            static readonly UntypedInt eosNBits = 30;
            static readonly UntypedInt eosPadByte = /* eosCode >> (eosNBits - 8) */ 255;
            nuint pad = 8 - over;
            x = (uint64)((x << (int)(pad)) | (eosPadByte >> (int)(over)));
            n += pad;
        }
    }
    // 8 now divides into n exactly
    // n in (0, 8, 16, 24, 32)
    switch (n / 8) {
    case 0: {
        return dst;
    }
    case 1: {
        return append(dst, ((byte)x));
    }
    case 2: {
        var yΔ3 = ((uint16)x);
        return append(dst, ((byte)(yΔ3 >> (int)(8))), ((byte)yΔ3));
    }
    case 3: {
        var yΔ4 = ((uint16)(x >> (int)(8)));
        return append(dst, ((byte)(yΔ4 >> (int)(8))), ((byte)yΔ4), ((byte)x));
    }}

    //	case 4:
    var y = ((uint32)x);
    return append(dst, ((byte)(y >> (int)(24))), ((byte)(y >> (int)(16))), ((byte)(y >> (int)(8))), ((byte)y));
}

// HuffmanEncodeLength returns the number of bytes required to encode
// s in Huffman codes. The result is round up to byte boundary.
public static uint64 HuffmanEncodeLength(@string s) {
    var n = ((uint64)0);
    for (nint i = 0; i < len(s); i++) {
        n += ((uint64)huffmanCodeLen[s[i]]);
    }
    return (n + 7) / 8;
}

} // end hpack_package
