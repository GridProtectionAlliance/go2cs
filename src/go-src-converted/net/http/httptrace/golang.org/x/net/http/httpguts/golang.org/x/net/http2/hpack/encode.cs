// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package hpack -- go2cs converted at 2022 March 06 22:22:05 UTC
// import "golang.org/x/net/http2/hpack" ==> using hpack = go.golang.org.x.net.http2.hpack_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\http2\hpack\encode.go
using io = go.io_package;

namespace go.golang.org.x.net.http2;

public static partial class hpack_package {

private static readonly var uint32Max = ~uint32(0);
private static readonly nint initialHeaderTableSize = 4096;


public partial struct Encoder {
    public dynamicTable dynTab; // minSize is the minimum table size set by
// SetMaxDynamicTableSize after the previous Header Table Size
// Update.
    public uint minSize; // maxSizeLimit is the maximum table size this encoder
// supports. This will protect the encoder from too large
// size.
    public uint maxSizeLimit; // tableSizeUpdate indicates whether "Header Table Size
// Update" is required.
    public bool tableSizeUpdate;
    public io.Writer w;
    public slice<byte> buf;
}

// NewEncoder returns a new Encoder which performs HPACK encoding. An
// encoded data is written to w.
public static ptr<Encoder> NewEncoder(io.Writer w) {
    ptr<Encoder> e = addr(new Encoder(minSize:uint32Max,maxSizeLimit:initialHeaderTableSize,tableSizeUpdate:false,w:w,));
    e.dynTab.table.init();
    e.dynTab.setMaxSize(initialHeaderTableSize);
    return _addr_e!;
}

// WriteField encodes f into a single Write to e's underlying Writer.
// This function may also produce bytes for "Header Table Size Update"
// if necessary. If produced, it is done before encoding f.
private static error WriteField(this ptr<Encoder> _addr_e, HeaderField f) {
    ref Encoder e = ref _addr_e.val;

    e.buf = e.buf[..(int)0];

    if (e.tableSizeUpdate) {
        e.tableSizeUpdate = false;
        if (e.minSize < e.dynTab.maxSize) {
            e.buf = appendTableSize(e.buf, e.minSize);
        }
        e.minSize = uint32Max;
        e.buf = appendTableSize(e.buf, e.dynTab.maxSize);

    }
    var (idx, nameValueMatch) = e.searchTable(f);
    if (nameValueMatch) {
        e.buf = appendIndexed(e.buf, idx);
    }
    else
 {
        var indexing = e.shouldIndex(f);
        if (indexing) {
            e.dynTab.add(f);
        }
        if (idx == 0) {
            e.buf = appendNewName(e.buf, f, indexing);
        }
        else
 {
            e.buf = appendIndexedName(e.buf, f, idx, indexing);
        }
    }
    var (n, err) = e.w.Write(e.buf);
    if (err == null && n != len(e.buf)) {
        err = io.ErrShortWrite;
    }
    return error.As(err)!;

}

// searchTable searches f in both stable and dynamic header tables.
// The static header table is searched first. Only when there is no
// exact match for both name and value, the dynamic header table is
// then searched. If there is no match, i is 0. If both name and value
// match, i is the matched index and nameValueMatch becomes true. If
// only name matches, i points to that index and nameValueMatch
// becomes false.
private static (ulong, bool) searchTable(this ptr<Encoder> _addr_e, HeaderField f) {
    ulong i = default;
    bool nameValueMatch = default;
    ref Encoder e = ref _addr_e.val;

    i, nameValueMatch = staticTable.search(f);
    if (nameValueMatch) {
        return (i, true);
    }
    var (j, nameValueMatch) = e.dynTab.table.search(f);
    if (nameValueMatch || (i == 0 && j != 0)) {
        return (j + uint64(staticTable.len()), nameValueMatch);
    }
    return (i, false);

}

// SetMaxDynamicTableSize changes the dynamic header table size to v.
// The actual size is bounded by the value passed to
// SetMaxDynamicTableSizeLimit.
private static void SetMaxDynamicTableSize(this ptr<Encoder> _addr_e, uint v) {
    ref Encoder e = ref _addr_e.val;

    if (v > e.maxSizeLimit) {
        v = e.maxSizeLimit;
    }
    if (v < e.minSize) {
        e.minSize = v;
    }
    e.tableSizeUpdate = true;
    e.dynTab.setMaxSize(v);

}

// SetMaxDynamicTableSizeLimit changes the maximum value that can be
// specified in SetMaxDynamicTableSize to v. By default, it is set to
// 4096, which is the same size of the default dynamic header table
// size described in HPACK specification. If the current maximum
// dynamic header table size is strictly greater than v, "Header Table
// Size Update" will be done in the next WriteField call and the
// maximum dynamic header table size is truncated to v.
private static void SetMaxDynamicTableSizeLimit(this ptr<Encoder> _addr_e, uint v) {
    ref Encoder e = ref _addr_e.val;

    e.maxSizeLimit = v;
    if (e.dynTab.maxSize > v) {
        e.tableSizeUpdate = true;
        e.dynTab.setMaxSize(v);
    }
}

// shouldIndex reports whether f should be indexed.
private static bool shouldIndex(this ptr<Encoder> _addr_e, HeaderField f) {
    ref Encoder e = ref _addr_e.val;

    return !f.Sensitive && f.Size() <= e.dynTab.maxSize;
}

// appendIndexed appends index i, as encoded in "Indexed Header Field"
// representation, to dst and returns the extended buffer.
private static slice<byte> appendIndexed(slice<byte> dst, ulong i) {
    var first = len(dst);
    dst = appendVarInt(dst, 7, i);
    dst[first] |= 0x80;
    return dst;
}

// appendNewName appends f, as encoded in one of "Literal Header field
// - New Name" representation variants, to dst and returns the
// extended buffer.
//
// If f.Sensitive is true, "Never Indexed" representation is used. If
// f.Sensitive is false and indexing is true, "Incremental Indexing"
// representation is used.
private static slice<byte> appendNewName(slice<byte> dst, HeaderField f, bool indexing) {
    dst = append(dst, encodeTypeByte(indexing, f.Sensitive));
    dst = appendHpackString(dst, f.Name);
    return appendHpackString(dst, f.Value);
}

// appendIndexedName appends f and index i referring indexed name
// entry, as encoded in one of "Literal Header field - Indexed Name"
// representation variants, to dst and returns the extended buffer.
//
// If f.Sensitive is true, "Never Indexed" representation is used. If
// f.Sensitive is false and indexing is true, "Incremental Indexing"
// representation is used.
private static slice<byte> appendIndexedName(slice<byte> dst, HeaderField f, ulong i, bool indexing) {
    var first = len(dst);
    byte n = default;
    if (indexing) {
        n = 6;
    }
    else
 {
        n = 4;
    }
    dst = appendVarInt(dst, n, i);
    dst[first] |= encodeTypeByte(indexing, f.Sensitive);
    return appendHpackString(dst, f.Value);

}

// appendTableSize appends v, as encoded in "Header Table Size Update"
// representation, to dst and returns the extended buffer.
private static slice<byte> appendTableSize(slice<byte> dst, uint v) {
    var first = len(dst);
    dst = appendVarInt(dst, 5, uint64(v));
    dst[first] |= 0x20;
    return dst;
}

// appendVarInt appends i, as encoded in variable integer form using n
// bit prefix, to dst and returns the extended buffer.
//
// See
// http://http2.github.io/http2-spec/compression.html#integer.representation
private static slice<byte> appendVarInt(slice<byte> dst, byte n, ulong i) {
    var k = uint64((1 << (int)(n)) - 1);
    if (i < k) {
        return append(dst, byte(i));
    }
    dst = append(dst, byte(k));
    i -= k;
    while (i >= 128) {
        dst = append(dst, byte(0x80 | (i & 0x7f)));
        i>>=7;
    }
    return append(dst, byte(i));

}

// appendHpackString appends s, as encoded in "String Literal"
// representation, to dst and returns the extended buffer.
//
// s will be encoded in Huffman codes only when it produces strictly
// shorter byte string.
private static slice<byte> appendHpackString(slice<byte> dst, @string s) {
    var huffmanLength = HuffmanEncodeLength(s);
    if (huffmanLength < uint64(len(s))) {
        var first = len(dst);
        dst = appendVarInt(dst, 7, huffmanLength);
        dst = AppendHuffmanString(dst, s);
        dst[first] |= 0x80;
    }
    else
 {
        dst = appendVarInt(dst, 7, uint64(len(s)));
        dst = append(dst, s);
    }
    return dst;

}

// encodeTypeByte returns type byte. If sensitive is true, type byte
// for "Never Indexed" representation is returned. If sensitive is
// false and indexing is true, type byte for "Incremental Indexing"
// representation is returned. Otherwise, type byte for "Without
// Indexing" is returned.
private static byte encodeTypeByte(bool indexing, bool sensitive) {
    if (sensitive) {
        return 0x10;
    }
    if (indexing) {
        return 0x40;
    }
    return 0;

}

} // end hpack_package
