// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using md5 = crypto.md5_package;
using binary = encoding.binary_package;
using constant = go.constant_package;
using io = io_package;
using big = math.big_package;
using runtime = runtime_package;
using strings = strings_package;
using crypto;
using encoding;
using go;
using math;

partial class pkgbits_package {

// currentVersion is the current version number.
//
//   - v0: initial prototype
//
//   - v1: adds the flags uint32 word
//
// TODO(mdempsky): For the next version bump:
//   - remove the legacy "has init" bool from the public root
//   - remove obj's "derived func instance" bool
internal const uint32 currentVersion = 1;

// A PkgEncoder provides methods for encoding a package's Unified IR
// export data.
[GoType] partial struct PkgEncoder {
    // elems holds the bitstream for previously encoded elements.
    internal array<slice<@string>> elems = new(numRelocs);
    // stringsIdx maps previously encoded strings to their index within
    // the RelocString section, to allow deduplication. That is,
    // elems[RelocString][stringsIdx[s]] == s (if present).
    internal map<@string, Index> stringsIdx;
    // syncFrames is the number of frames to write at each sync
    // marker. A negative value means sync markers are omitted.
    internal nint syncFrames;
}

// SyncMarkers reports whether pw uses sync markers.
[GoRecv] public static bool SyncMarkers(this ref PkgEncoder pw) {
    return pw.syncFrames >= 0;
}

// NewPkgEncoder returns an initialized PkgEncoder.
//
// syncFrames is the number of caller frames that should be serialized
// at Sync points. Serializing additional frames results in larger
// export data files, but can help diagnosing desync errors in
// higher-level Unified IR reader/writer code. If syncFrames is
// negative, then sync markers are omitted entirely.
public static PkgEncoder NewPkgEncoder(nint syncFrames) {
    return new PkgEncoder(
        stringsIdx: new map<@string, Index>(),
        syncFrames: syncFrames
    );
}

// DumpTo writes the package's encoded data to out0 and returns the
// package fingerprint.
[GoRecv] public static array<byte> /*fingerprint*/ DumpTo(this ref PkgEncoder pw, io.Writer out0) {
    array<byte> fingerprint = default!;

    var h = md5.New();
    var @out = io.MultiWriter(out0, h);
    var writeUint32 = 
    var outʗ1 = @out;
    (uint32 x) => {
        assert(binary.Write(outʗ1, binary.LittleEndian, x) == default!);
    };
    writeUint32(currentVersion);
    uint32 flags = default!;
    if (pw.SyncMarkers()) {
        flags |= (uint32)(flagSyncMarkers);
    }
    writeUint32(flags);
    // Write elemEndsEnds.
    uint32 sum = default!;
    foreach (var (Δ_, elems) in Ꮡ(pw.elems).val) {
        sum += ((uint32)len(elems));
        writeUint32(sum);
    }
    // Write elemEnds.
    sum = 0;
    foreach (var (Δ_, elems) in Ꮡ(pw.elems).val) {
        foreach (var (Δ_, elem) in elems) {
            sum += ((uint32)len(elem));
            writeUint32(sum);
        }
    }
    // Write elemData.
    foreach (var (Δ_, elems) in Ꮡ(pw.elems).val) {
        foreach (var (Δ_, elem) in elems) {
            var (Δ_, errΔ1) = io.WriteString(@out, elem);
            assert(errΔ1 == default!);
        }
    }
    // Write fingerprint.
    copy(fingerprint[..], h.Sum(default!));
    var (Δ_, err) = out0.Write(fingerprint[..]);
    assert(err == default!);
    return fingerprint;
}

// StringIdx adds a string value to the strings section, if not
// already present, and returns its index.
[GoRecv] public static Index StringIdx(this ref PkgEncoder pw, @string s) {
    {
        var (idxΔ1, ok) = pw.stringsIdx[s]; if (ok) {
            assert(pw.elems[RelocString][idxΔ1] == s);
            return idxΔ1;
        }
    }
    var idx = ((Index)len(pw.elems[RelocString]));
    pw.elems[RelocString] = append(pw.elems[RelocString], s);
    pw.stringsIdx[s] = idx;
    return idx;
}

// NewEncoder returns an Encoder for a new element within the given
// section, and encodes the given SyncMarker as the start of the
// element bitstream.
[GoRecv] public static Encoder NewEncoder(this ref PkgEncoder pw, RelocKind k, SyncMarker marker) {
    var e = pw.NewEncoderRaw(k);
    e.Sync(marker);
    return e;
}

// NewEncoderRaw returns an Encoder for a new element within the given
// section.
//
// Most callers should use NewEncoder instead.
[GoRecv] public static Encoder NewEncoderRaw(this ref PkgEncoder pw, RelocKind k) {
    var idx = ((Index)len(pw.elems[k]));
    pw.elems[k] = append(pw.elems[k], ""u8);
    // placeholder
    return new Encoder(
        p: pw,
        k: k,
        Idx: idx
    );
}

// An Encoder provides methods for encoding an individual element's
// bitstream data.
[GoType] partial struct Encoder {
    internal ж<PkgEncoder> p;
    public slice<RelocEnt> Relocs;
    public map<RelocEnt, uint32> RelocMap;
    public bytes_package.Buffer Data; // accumulated element bitstream data
    internal bool encodingRelocHeader;
    internal RelocKind k;
    public Index Idx; // index within relocation section
}

// Flush finalizes the element's bitstream and returns its Index.
[GoRecv] public static Index Flush(this ref Encoder w) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    // Backup the data so we write the relocations at the front.
    ref var tmp = ref heap(new bytes_package.Buffer(), out var Ꮡtmp);
    io.Copy(~Ꮡtmp, w.Data);
    // TODO(mdempsky): Consider writing these out separately so they're
    // easier to strip, along with function bodies, so that we can prune
    // down to just the data that's relevant to go/types.
    if (w.encodingRelocHeader) {
        throw panic("encodingRelocHeader already true; recursive flush?");
    }
    w.encodingRelocHeader = true;
    w.Sync(SyncRelocs);
    w.Len(len(w.Relocs));
    foreach (var (Δ_, rEnt) in w.Relocs) {
        w.Sync(SyncReloc);
        w.Len(((nint)rEnt.Kind));
        w.Len(((nint)rEnt.Idx));
    }
    io.Copy(~Ꮡsb, w.Data);
    io.Copy(~Ꮡsb, ~Ꮡtmp);
    w.p.elems[w.k][w.Idx] = sb.String();
    return w.Idx;
}

[GoRecv] internal static void checkErr(this ref Encoder w, error err) {
    if (err != default!) {
        errorf("unexpected encoding error: %v"u8, err);
    }
}

[GoRecv] internal static void rawUvarint(this ref Encoder w, uint64 x) {
    array<byte> buf = new(10); /* binary.MaxVarintLen64 */
    nint n = binary.PutUvarint(buf[..], x);
    var (Δ_, err) = w.Data.Write(buf[..(int)(n)]);
    w.checkErr(err);
}

[GoRecv] internal static void rawVarint(this ref Encoder w, int64 x) {
    // Zig-zag encode.
    var ux = ((uint64)x) << (int)(1);
    if (x < 0) {
        ux = ~ux;
    }
    w.rawUvarint(ux);
}

[GoRecv] internal static nint rawReloc(this ref Encoder w, RelocKind r, Index idx) {
    var e = new RelocEnt(r, idx);
    if (w.RelocMap != default!){
        {
            var (iΔ1, ok) = w.RelocMap[e]; if (ok) {
                return ((nint)iΔ1);
            }
        }
    } else {
        w.RelocMap = new map<RelocEnt, uint32>();
    }
    nint i = len(w.Relocs);
    w.RelocMap[e] = ((uint32)i);
    w.Relocs = append(w.Relocs, e);
    return i;
}

[GoRecv] public static void Sync(this ref Encoder w, SyncMarker m) {
    if (!w.p.SyncMarkers()) {
        return;
    }
    // Writing out stack frame string references requires working
    // relocations, but writing out the relocations themselves involves
    // sync markers. To prevent infinite recursion, we simply trim the
    // stack frame for sync markers within the relocation header.
    slice<@string> frames = default!;
    if (!w.encodingRelocHeader && w.p.syncFrames > 0) {
        var pcs = new slice<uintptr>(w.p.syncFrames);
        nint n = runtime.Callers(2, pcs);
        frames = fmtFrames(pcs[..(int)(n)].ꓸꓸꓸ);
    }
    // TODO(mdempsky): Save space by writing out stack frames as a
    // linked list so we can share common stack frames.
    w.rawUvarint(((uint64)m));
    w.rawUvarint(((uint64)len(frames)));
    foreach (var (Δ_, frame) in frames) {
        w.rawUvarint(((uint64)w.rawReloc(RelocString, w.p.StringIdx(frame))));
    }
}

// Bool encodes and writes a bool value into the element bitstream,
// and then returns the bool value.
//
// For simple, 2-alternative encodings, the idiomatic way to call Bool
// is something like:
//
//	if w.Bool(x != 0) {
//		// alternative #1
//	} else {
//		// alternative #2
//	}
//
// For multi-alternative encodings, use Code instead.
[GoRecv] public static bool Bool(this ref Encoder w, bool b) {
    w.Sync(SyncBool);
    byte x = default!;
    if (b) {
        x = 1;
    }
    var err = w.Data.WriteByte(x);
    w.checkErr(err);
    return b;
}

// Int64 encodes and writes an int64 value into the element bitstream.
[GoRecv] public static void Int64(this ref Encoder w, int64 x) {
    w.Sync(SyncInt64);
    w.rawVarint(x);
}

// Uint64 encodes and writes a uint64 value into the element bitstream.
[GoRecv] public static void Uint64(this ref Encoder w, uint64 x) {
    w.Sync(SyncUint64);
    w.rawUvarint(x);
}

// Len encodes and writes a non-negative int value into the element bitstream.
[GoRecv] public static void Len(this ref Encoder w, nint x) {
    assert(x >= 0);
    w.Uint64(((uint64)x));
}

// Int encodes and writes an int value into the element bitstream.
[GoRecv] public static void Int(this ref Encoder w, nint x) {
    w.Int64(((int64)x));
}

// Len encodes and writes a uint value into the element bitstream.
[GoRecv] public static void Uint(this ref Encoder w, nuint x) {
    w.Uint64(((uint64)x));
}

// Reloc encodes and writes a relocation for the given (section,
// index) pair into the element bitstream.
//
// Note: Only the index is formally written into the element
// bitstream, so bitstream decoders must know from context which
// section an encoded relocation refers to.
[GoRecv] public static void Reloc(this ref Encoder w, RelocKind r, Index idx) {
    w.Sync(SyncUseReloc);
    w.Len(w.rawReloc(r, idx));
}

// Code encodes and writes a Code value into the element bitstream.
[GoRecv] public static void Code(this ref Encoder w, ΔCode c) {
    w.Sync(c.Marker());
    w.Len(c.Value());
}

// String encodes and writes a string value into the element
// bitstream.
//
// Internally, strings are deduplicated by adding them to the strings
// section (if not already present), and then writing a relocation
// into the element bitstream.
[GoRecv] public static void String(this ref Encoder w, @string s) {
    w.StringRef(w.p.StringIdx(s));
}

// StringRef writes a reference to the given index, which must be a
// previously encoded string value.
[GoRecv] public static void StringRef(this ref Encoder w, Index idx) {
    w.Sync(SyncString);
    w.Reloc(RelocString, idx);
}

// Strings encodes and writes a variable-length slice of strings into
// the element bitstream.
[GoRecv] public static void Strings(this ref Encoder w, slice<@string> ss) {
    w.Len(len(ss));
    foreach (var (Δ_, s) in ss) {
        w.String(s);
    }
}

// Value encodes and writes a constant.Value into the element
// bitstream.
[GoRecv] public static void Value(this ref Encoder w, constant.Value val) {
    w.Sync(SyncValue);
    if (w.Bool(val.Kind() == constant.Complex)){
        w.scalar(constant.Real(val));
        w.scalar(constant.Imag(val));
    } else {
        w.scalar(val);
    }
}

[GoRecv] internal static void scalar(this ref Encoder w, constant.Value val) {
    switch (constant.Val(val).type()) {
    default: {
        var v = constant.Val(val).type();
        errorf("unhandled %v (%v)"u8, val, val.Kind());
        break;
    }
    case bool v: {
        w.Code(ValBool);
        w.Bool(v);
        break;
    }
    case @string v: {
        w.Code(ValString);
        w.String(v);
        break;
    }
    case int64 v: {
        w.Code(ValInt64);
        w.Int64(v);
        break;
    }
    case ж<bigꓸInt> v: {
        w.Code(ValBigInt);
        w.bigInt(v);
        break;
    }
    case ж<bigꓸRat> v: {
        w.Code(ValBigRat);
        w.bigInt(v.Num());
        w.bigInt(v.Denom());
        break;
    }
    case ж<big.Float> v: {
        w.Code(ValBigFloat);
        w.bigFloat(v);
        break;
    }}
}

[GoRecv] public static void bigInt(this ref Encoder w, ж<bigꓸInt> Ꮡv) {
    ref var v = ref Ꮡv.val;

    var b = v.Bytes();
    w.String(((@string)b));
    // TODO: More efficient encoding.
    w.Bool(v.Sign() < 0);
}

[GoRecv] public static void bigFloat(this ref Encoder w, ж<big.Float> Ꮡv) {
    ref var v = ref Ꮡv.val;

    var b = v.Append(default!, (rune)'p', -1);
    w.String(((@string)b));
}

// TODO: More efficient encoding.

} // end pkgbits_package
