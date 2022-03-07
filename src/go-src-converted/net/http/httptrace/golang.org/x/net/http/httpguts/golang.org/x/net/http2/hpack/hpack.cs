// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hpack implements HPACK, a compression format for
// efficiently representing HTTP header fields in the context of HTTP/2.
//
// See http://tools.ietf.org/html/draft-ietf-httpbis-header-compression-09
// package hpack -- go2cs converted at 2022 March 06 22:22:45 UTC
// import "golang.org/x/net/http2/hpack" ==> using hpack = go.golang.org.x.net.http2.hpack_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\http2\hpack\hpack.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using System;


namespace go.golang.org.x.net.http2;

public static partial class hpack_package {

    // A DecodingError is something the spec defines as a decoding error.
public partial struct DecodingError {
    public error Err;
}

public static @string Error(this DecodingError de) {
    return fmt.Sprintf("decoding error: %v", de.Err);
}

// An InvalidIndexError is returned when an encoder references a table
// entry before the static table or after the end of the dynamic table.
public partial struct InvalidIndexError { // : nint
}

public static @string Error(this InvalidIndexError e) {
    return fmt.Sprintf("invalid indexed representation index %d", int(e));
}

// A HeaderField is a name-value pair. Both the name and value are
// treated as opaque sequences of octets.
public partial struct HeaderField {
    public @string Name; // Sensitive means that this header field should never be
// indexed.
    public @string Value; // Sensitive means that this header field should never be
// indexed.
    public bool Sensitive;
}

// IsPseudo reports whether the header field is an http2 pseudo header.
// That is, it reports whether it starts with a colon.
// It is not otherwise guaranteed to be a valid pseudo header field,
// though.
public static bool IsPseudo(this HeaderField hf) {
    return len(hf.Name) != 0 && hf.Name[0] == ':';
}

public static @string String(this HeaderField hf) {
    @string suffix = default;
    if (hf.Sensitive) {
        suffix = " (sensitive)";
    }
    return fmt.Sprintf("header field %q = %q%s", hf.Name, hf.Value, suffix);

}

// Size returns the size of an entry per RFC 7541 section 4.1.
public static uint Size(this HeaderField hf) { 
    // http://http2.github.io/http2-spec/compression.html#rfc.section.4.1
    // "The size of the dynamic table is the sum of the size of
    // its entries. The size of an entry is the sum of its name's
    // length in octets (as defined in Section 5.2), its value's
    // length in octets (see Section 5.2), plus 32.  The size of
    // an entry is calculated using the length of the name and
    // value without any Huffman encoding applied."

    // This can overflow if somebody makes a large HeaderField
    // Name and/or Value by hand, but we don't care, because that
    // won't happen on the wire because the encoding doesn't allow
    // it.
    return uint32(len(hf.Name) + len(hf.Value) + 32);

}

// A Decoder is the decoding context for incremental processing of
// header blocks.
public partial struct Decoder {
    public dynamicTable dynTab;
    public Action<HeaderField> emit;
    public bool emitEnabled; // whether calls to emit are enabled
    public nint maxStrLen; // 0 means unlimited

// buf is the unparsed buffer. It's only written to
// saveBuf if it was truncated in the middle of a header
// block. Because it's usually not owned, we can only
// process it under Write.
    public slice<byte> buf; // not owned; only valid during Write

// saveBuf is previous data passed to Write which we weren't able
// to fully parse before. Unlike buf, we own this data.
    public bytes.Buffer saveBuf;
    public bool firstField; // processing the first field of the header block
}

// NewDecoder returns a new decoder with the provided maximum dynamic
// table size. The emitFunc will be called for each valid field
// parsed, in the same goroutine as calls to Write, before Write returns.
public static ptr<Decoder> NewDecoder(uint maxDynamicTableSize, Action<HeaderField> emitFunc) {
    ptr<Decoder> d = addr(new Decoder(emit:emitFunc,emitEnabled:true,firstField:true,));
    d.dynTab.table.init();
    d.dynTab.allowedMaxSize = maxDynamicTableSize;
    d.dynTab.setMaxSize(maxDynamicTableSize);
    return _addr_d!;
}

// ErrStringLength is returned by Decoder.Write when the max string length
// (as configured by Decoder.SetMaxStringLength) would be violated.
public static var ErrStringLength = errors.New("hpack: string too long");

// SetMaxStringLength sets the maximum size of a HeaderField name or
// value string. If a string exceeds this length (even after any
// decompression), Write will return ErrStringLength.
// A value of 0 means unlimited and is the default from NewDecoder.
private static void SetMaxStringLength(this ptr<Decoder> _addr_d, nint n) {
    ref Decoder d = ref _addr_d.val;

    d.maxStrLen = n;
}

// SetEmitFunc changes the callback used when new header fields
// are decoded.
// It must be non-nil. It does not affect EmitEnabled.
private static void SetEmitFunc(this ptr<Decoder> _addr_d, Action<HeaderField> emitFunc) {
    ref Decoder d = ref _addr_d.val;

    d.emit = emitFunc;
}

// SetEmitEnabled controls whether the emitFunc provided to NewDecoder
// should be called. The default is true.
//
// This facility exists to let servers enforce MAX_HEADER_LIST_SIZE
// while still decoding and keeping in-sync with decoder state, but
// without doing unnecessary decompression or generating unnecessary
// garbage for header fields past the limit.
private static void SetEmitEnabled(this ptr<Decoder> _addr_d, bool v) {
    ref Decoder d = ref _addr_d.val;

    d.emitEnabled = v;
}

// EmitEnabled reports whether calls to the emitFunc provided to NewDecoder
// are currently enabled. The default is true.
private static bool EmitEnabled(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    return d.emitEnabled;
}

// TODO: add method *Decoder.Reset(maxSize, emitFunc) to let callers re-use Decoders and their
// underlying buffers for garbage reasons.

private static void SetMaxDynamicTableSize(this ptr<Decoder> _addr_d, uint v) {
    ref Decoder d = ref _addr_d.val;

    d.dynTab.setMaxSize(v);
}

// SetAllowedMaxDynamicTableSize sets the upper bound that the encoded
// stream (via dynamic table size updates) may set the maximum size
// to.
private static void SetAllowedMaxDynamicTableSize(this ptr<Decoder> _addr_d, uint v) {
    ref Decoder d = ref _addr_d.val;

    d.dynTab.allowedMaxSize = v;
}

private partial struct dynamicTable {
    public headerFieldTable table;
    public uint size; // in bytes
    public uint maxSize; // current maxSize
    public uint allowedMaxSize; // maxSize may go up to this, inclusive
}

private static void setMaxSize(this ptr<dynamicTable> _addr_dt, uint v) {
    ref dynamicTable dt = ref _addr_dt.val;

    dt.maxSize = v;
    dt.evict();
}

private static void add(this ptr<dynamicTable> _addr_dt, HeaderField f) {
    ref dynamicTable dt = ref _addr_dt.val;

    dt.table.addEntry(f);
    dt.size += f.Size();
    dt.evict();
}

// If we're too big, evict old stuff.
private static void evict(this ptr<dynamicTable> _addr_dt) {
    ref dynamicTable dt = ref _addr_dt.val;

    nint n = default;
    while (dt.size > dt.maxSize && n < dt.table.len()) {
        dt.size -= dt.table.ents[n].Size();
        n++;
    }
    dt.table.evictOldest(n);
}

private static nint maxTableIndex(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;
 
    // This should never overflow. RFC 7540 Section 6.5.2 limits the size of
    // the dynamic table to 2^32 bytes, where each entry will occupy more than
    // one byte. Further, the staticTable has a fixed, small length.
    return d.dynTab.table.len() + staticTable.len();

}

private static (HeaderField, bool) at(this ptr<Decoder> _addr_d, ulong i) {
    HeaderField hf = default;
    bool ok = default;
    ref Decoder d = ref _addr_d.val;
 
    // See Section 2.3.3.
    if (i == 0) {
        return ;
    }
    if (i <= uint64(staticTable.len())) {
        return (staticTable.ents[i - 1], true);
    }
    if (i > uint64(d.maxTableIndex())) {
        return ;
    }
    var dt = d.dynTab.table;
    return (dt.ents[dt.len() - (int(i) - staticTable.len())], true);

}

// Decode decodes an entire block.
//
// TODO: remove this method and make it incremental later? This is
// easier for debugging now.
private static (slice<HeaderField>, error) DecodeFull(this ptr<Decoder> _addr_d, slice<byte> p) => func((defer, _, _) => {
    slice<HeaderField> _p0 = default;
    error _p0 = default!;
    ref Decoder d = ref _addr_d.val;

    slice<HeaderField> hf = default;
    var saveFunc = d.emit;
    defer(() => {
        d.emit = saveFunc;
    }());
    d.emit = f => {
        hf = append(hf, f);
    };
    {
        var (_, err) = d.Write(p);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    {
        var err = d.Close();

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    return (hf, error.As(null!)!);

});

// Close declares that the decoding is complete and resets the Decoder
// to be reused again for a new header block. If there is any remaining
// data in the decoder's buffer, Close returns an error.
private static error Close(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    if (d.saveBuf.Len() > 0) {
        d.saveBuf.Reset();
        return error.As(new DecodingError(errors.New("truncated headers")))!;
    }
    d.firstField = true;
    return error.As(null!)!;

}

private static (nint, error) Write(this ptr<Decoder> _addr_d, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Decoder d = ref _addr_d.val;

    if (len(p) == 0) { 
        // Prevent state machine CPU attacks (making us redo
        // work up to the point of finding out we don't have
        // enough data)
        return ;

    }
    if (d.saveBuf.Len() == 0) {
        d.buf = p;
    }
    else
 {
        d.saveBuf.Write(p);
        d.buf = d.saveBuf.Bytes();
        d.saveBuf.Reset();
    }
    while (len(d.buf) > 0) {
        err = d.parseHeaderFieldRepr();
        if (err == errNeedMore) { 
            // Extra paranoia, making sure saveBuf won't
            // get too large. All the varint and string
            // reading code earlier should already catch
            // overlong things and return ErrStringLength,
            // but keep this as a last resort.
            const nint varIntOverhead = 8; // conservative
 // conservative
            if (d.maxStrLen != 0 && int64(len(d.buf)) > 2 * (int64(d.maxStrLen) + varIntOverhead)) {
                return (0, error.As(ErrStringLength)!);
            }

            d.saveBuf.Write(d.buf);
            return (len(p), error.As(null!)!);

        }
        d.firstField = false;
        if (err != null) {
            break;
        }
    }
    return (len(p), error.As(err)!);

}

// errNeedMore is an internal sentinel error value that means the
// buffer is truncated and we need to read more data before we can
// continue parsing.
private static var errNeedMore = errors.New("need more data");

private partial struct indexType { // : nint
}

private static readonly indexType indexedTrue = iota;
private static readonly var indexedFalse = 0;
private static readonly var indexedNever = 1;


private static bool indexed(this indexType v) {
    return v == indexedTrue;
}
private static bool sensitive(this indexType v) {
    return v == indexedNever;
}

// returns errNeedMore if there isn't enough data available.
// any other error is fatal.
// consumes d.buf iff it returns nil.
// precondition: must be called with len(d.buf) > 0
private static error parseHeaderFieldRepr(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    var b = d.buf[0];

    if (b & 128 != 0) 
        // Indexed representation.
        // High bit set?
        // http://http2.github.io/http2-spec/compression.html#rfc.section.6.1
        return error.As(d.parseFieldIndexed())!;
    else if (b & 192 == 64) 
        // 6.2.1 Literal Header Field with Incremental Indexing
        // 0b10xxxxxx: top two bits are 10
        // http://http2.github.io/http2-spec/compression.html#rfc.section.6.2.1
        return error.As(d.parseFieldLiteral(6, indexedTrue))!;
    else if (b & 240 == 0) 
        // 6.2.2 Literal Header Field without Indexing
        // 0b0000xxxx: top four bits are 0000
        // http://http2.github.io/http2-spec/compression.html#rfc.section.6.2.2
        return error.As(d.parseFieldLiteral(4, indexedFalse))!;
    else if (b & 240 == 16) 
        // 6.2.3 Literal Header Field never Indexed
        // 0b0001xxxx: top four bits are 0001
        // http://http2.github.io/http2-spec/compression.html#rfc.section.6.2.3
        return error.As(d.parseFieldLiteral(4, indexedNever))!;
    else if (b & 224 == 32) 
        // 6.3 Dynamic Table Size Update
        // Top three bits are '001'.
        // http://http2.github.io/http2-spec/compression.html#rfc.section.6.3
        return error.As(d.parseDynamicTableSizeUpdate())!;
        return error.As(new DecodingError(errors.New("invalid encoding")))!;

}

// (same invariants and behavior as parseHeaderFieldRepr)
private static error parseFieldIndexed(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;

    var buf = d.buf;
    var (idx, buf, err) = readVarInt(7, buf);
    if (err != null) {
        return error.As(err)!;
    }
    var (hf, ok) = d.at(idx);
    if (!ok) {
        return error.As(new DecodingError(InvalidIndexError(idx)))!;
    }
    d.buf = buf;
    return error.As(d.callEmit(new HeaderField(Name:hf.Name,Value:hf.Value)))!;

}

// (same invariants and behavior as parseHeaderFieldRepr)
private static error parseFieldLiteral(this ptr<Decoder> _addr_d, byte n, indexType it) {
    ref Decoder d = ref _addr_d.val;

    var buf = d.buf;
    var (nameIdx, buf, err) = readVarInt(n, buf);
    if (err != null) {
        return error.As(err)!;
    }
    HeaderField hf = default;
    var wantStr = d.emitEnabled || it.indexed();
    if (nameIdx > 0) {
        var (ihf, ok) = d.at(nameIdx);
        if (!ok) {
            return error.As(new DecodingError(InvalidIndexError(nameIdx)))!;
        }
        hf.Name = ihf.Name;

    }
    else
 {
        hf.Name, buf, err = d.readString(buf, wantStr);
        if (err != null) {
            return error.As(err)!;
        }
    }
    hf.Value, buf, err = d.readString(buf, wantStr);
    if (err != null) {
        return error.As(err)!;
    }
    d.buf = buf;
    if (it.indexed()) {
        d.dynTab.add(hf);
    }
    hf.Sensitive = it.sensitive();
    return error.As(d.callEmit(hf))!;

}

private static error callEmit(this ptr<Decoder> _addr_d, HeaderField hf) {
    ref Decoder d = ref _addr_d.val;

    if (d.maxStrLen != 0) {
        if (len(hf.Name) > d.maxStrLen || len(hf.Value) > d.maxStrLen) {
            return error.As(ErrStringLength)!;
        }
    }
    if (d.emitEnabled) {
        d.emit(hf);
    }
    return error.As(null!)!;

}

// (same invariants and behavior as parseHeaderFieldRepr)
private static error parseDynamicTableSizeUpdate(this ptr<Decoder> _addr_d) {
    ref Decoder d = ref _addr_d.val;
 
    // RFC 7541, sec 4.2: This dynamic table size update MUST occur at the
    // beginning of the first header block following the change to the dynamic table size.
    if (!d.firstField && d.dynTab.size > 0) {
        return error.As(new DecodingError(errors.New("dynamic table size update MUST occur at the beginning of a header block")))!;
    }
    var buf = d.buf;
    var (size, buf, err) = readVarInt(5, buf);
    if (err != null) {
        return error.As(err)!;
    }
    if (size > uint64(d.dynTab.allowedMaxSize)) {
        return error.As(new DecodingError(errors.New("dynamic table size update too large")))!;
    }
    d.dynTab.setMaxSize(uint32(size));
    d.buf = buf;
    return error.As(null!)!;

}

private static DecodingError errVarintOverflow = new DecodingError(errors.New("varint integer overflow"));

// readVarInt reads an unsigned variable length integer off the
// beginning of p. n is the parameter as described in
// http://http2.github.io/http2-spec/compression.html#rfc.section.5.1.
//
// n must always be between 1 and 8.
//
// The returned remain buffer is either a smaller suffix of p, or err != nil.
// The error is errNeedMore if p doesn't contain a complete integer.
private static (ulong, slice<byte>, error) readVarInt(byte n, slice<byte> p) => func((_, panic, _) => {
    ulong i = default;
    slice<byte> remain = default;
    error err = default!;

    if (n < 1 || n > 8) {
        panic("bad n");
    }
    if (len(p) == 0) {
        return (0, p, error.As(errNeedMore)!);
    }
    i = uint64(p[0]);
    if (n < 8) {
        i &= (1 << (int)(uint64(n))) - 1;
    }
    if (i < (1 << (int)(uint64(n))) - 1) {
        return (i, p[(int)1..], error.As(null!)!);
    }
    var origP = p;
    p = p[(int)1..];
    ulong m = default;
    while (len(p) > 0) {
        var b = p[0];
        p = p[(int)1..];
        i += uint64(b & 127) << (int)(m);
        if (b & 128 == 0) {
            return (i, p, error.As(null!)!);
        }
        m += 7;
        if (m >= 63) { // TODO: proper overflow check. making this up.
            return (0, origP, error.As(errVarintOverflow)!);

        }
    }
    return (0, origP, error.As(errNeedMore)!);

});

// readString decodes an hpack string from p.
//
// wantStr is whether s will be used. If false, decompression and
// []byte->string garbage are skipped if s will be ignored
// anyway. This does mean that huffman decoding errors for non-indexed
// strings past the MAX_HEADER_LIST_SIZE are ignored, but the server
// is returning an error anyway, and because they're not indexed, the error
// won't affect the decoding state.
private static (@string, slice<byte>, error) readString(this ptr<Decoder> _addr_d, slice<byte> p, bool wantStr) => func((defer, _, _) => {
    @string s = default;
    slice<byte> remain = default;
    error err = default!;
    ref Decoder d = ref _addr_d.val;

    if (len(p) == 0) {
        return ("", p, error.As(errNeedMore)!);
    }
    var isHuff = p[0] & 128 != 0;
    var (strLen, p, err) = readVarInt(7, p);
    if (err != null) {
        return ("", p, error.As(err)!);
    }
    if (d.maxStrLen != 0 && strLen > uint64(d.maxStrLen)) {
        return ("", null, error.As(ErrStringLength)!);
    }
    if (uint64(len(p)) < strLen) {
        return ("", p, error.As(errNeedMore)!);
    }
    if (!isHuff) {
        if (wantStr) {
            s = string(p[..(int)strLen]);
        }
        return (s, p[(int)strLen..], error.As(null!)!);

    }
    if (wantStr) {
        ptr<bytes.Buffer> buf = bufPool.Get()._<ptr<bytes.Buffer>>();
        buf.Reset(); // don't trust others
        defer(bufPool.Put(buf));
        {
            var err = huffmanDecode(buf, d.maxStrLen, p[..(int)strLen]);

            if (err != null) {
                buf.Reset();
                return ("", null, error.As(err)!);
            }

        }

        s = buf.String();
        buf.Reset(); // be nice to GC
    }
    return (s, p[(int)strLen..], error.As(null!)!);

});

} // end hpack_package
