// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = unicode.utf8_package;
using unicode;

partial class norm_package {

internal static readonly UntypedInt maxNonStarters = 30;
internal static readonly UntypedInt maxBufferSize = /* maxNonStarters + 2 */ 32;
internal static readonly UntypedInt maxNFCExpansion = 3; // NFC(0x1D160)
internal static readonly UntypedInt maxNFKCExpansion = 18; // NFKC(0xFDFA)
internal static readonly UntypedInt maxByteBufferSize = /* utf8.UTFMax * maxBufferSize */ 128; // 128

[GoType("num:nint")] partial struct ssState;

internal static readonly ssState ssSuccess = /* iota */ 0;
internal static readonly ssState ssStarter = 1;
internal static readonly ssState ssOverflow = 2;

[GoType("num:uint8")] partial struct streamSafe;

// first inserts the first rune of a segment. It is a faster version of next if
// it is known p represents the first rune in a segment.
[GoRecv] internal static void first(this ref streamSafe ss, ΔProperties p) {
    ss = ((streamSafe)p.nTrailingNonStarters());
}

// insert returns a ssState value to indicate whether a rune represented by p
// can be inserted.
[GoRecv] internal static ssState next(this ref streamSafe ss, ΔProperties p) {
    if (ss > maxNonStarters) {
        throw panic("streamSafe was not reset");
    }
    var n = p.nLeadingNonStarters();
    {
        var ss += ((streamSafe)n); if (ss > maxNonStarters) {
            ss = 0;
            return ssOverflow;
        }
    }
    // The Stream-Safe Text Processing prescribes that the counting can stop
    // as soon as a starter is encountered. However, there are some starters,
    // like Jamo V and T, that can combine with other runes, leaving their
    // successive non-starters appended to the previous, possibly causing an
    // overflow. We will therefore consider any rune with a non-zero nLead to
    // be a non-starter. Note that it always hold that if nLead > 0 then
    // nLead == nTrail.
    if (n == 0) {
        ss = ((streamSafe)p.nTrailingNonStarters());
        return ssStarter;
    }
    return ssSuccess;
}

// backwards is used for checking for overflow and segment starts
// when traversing a string backwards. Users do not need to call first
// for the first rune. The state of the streamSafe retains the count of
// the non-starters loaded.
[GoRecv] internal static ssState backwards(this ref streamSafe ss, ΔProperties p) {
    if (ss > maxNonStarters) {
        throw panic("streamSafe was not reset");
    }
    var c = ss + ((streamSafe)p.nTrailingNonStarters());
    if (c > maxNonStarters) {
        return ssOverflow;
    }
    ss = c;
    if (p.nLeadingNonStarters() == 0) {
        return ssStarter;
    }
    return ssSuccess;
}

internal static bool isMax(this streamSafe ss) {
    return ss == maxNonStarters;
}

// GraphemeJoiner is inserted after maxNonStarters non-starter runes.
public static readonly @string GraphemeJoiner = "\u034F"u8;

// reorderBuffer is used to normalize a single segment.  Characters inserted with
// insert are decomposed and reordered based on CCC. The compose method can
// be used to recombine characters.  Note that the byte buffer does not hold
// the UTF-8 characters in order.  Only the rune array is maintained in sorted
// order. flush writes the resulting segment to a byte array.
[GoType] partial struct reorderBuffer {
    internal array<ΔProperties> rune = new(maxBufferSize); // Per character info.
    internal array<byte> @byte = new(maxByteBufferSize); // UTF-8 buffer. Referenced by runeInfo.pos.
    internal uint8 nbyte;                     // Number or bytes.
    internal streamSafe ss;                // For limiting length of non-starter sequence.
    internal nint nrune;                      // Number of runeInfos.
    internal formInfo f;
    internal input src;
    internal nint nsrc;
    internal input tmpBytes;
    internal slice<byte> @out;
    internal Func<ж<reorderBuffer>, bool> flushF;
}

[GoRecv] internal static void init(this ref reorderBuffer rb, Form f, slice<byte> src) {
    rb.f = formTable[f].val;
    rb.src.setBytes(src);
    rb.nsrc = len(src);
    rb.ss = 0;
}

[GoRecv] internal static void initString(this ref reorderBuffer rb, Form f, @string src) {
    rb.f = formTable[f].val;
    rb.src.setString(src);
    rb.nsrc = len(src);
    rb.ss = 0;
}

[GoRecv] internal static void setFlusher(this ref reorderBuffer rb, slice<byte> @out, Func<ж<reorderBuffer>, bool> f) {
    rb.@out = @out;
    rb.flushF = f;
}

// reset discards all characters from the buffer.
[GoRecv] internal static void reset(this ref reorderBuffer rb) {
    rb.nrune = 0;
    rb.nbyte = 0;
}

[GoRecv] internal static bool doFlush(this ref reorderBuffer rb) {
    if (rb.f.composing) {
        rb.compose();
    }
    var res = rb.flushF(rb);
    rb.reset();
    return res;
}

// appendFlush appends the normalized segment to rb.out.
internal static bool appendFlush(ж<reorderBuffer> Ꮡrb) {
    ref var rb = ref Ꮡrb.val;

    for (nint i = 0; i < rb.nrune; i++) {
        var start = rb.rune[i].pos;
        var end = start + rb.rune[i].size;
        rb.@out = append(rb.@out, rb.@byte[(int)(start)..(int)(end)].ꓸꓸꓸ);
    }
    return true;
}

// flush appends the normalized segment to out and resets rb.
[GoRecv] internal static slice<byte> flush(this ref reorderBuffer rb, slice<byte> @out) {
    for (nint i = 0; i < rb.nrune; i++) {
        var start = rb.rune[i].pos;
        var end = start + rb.rune[i].size;
        @out = append(@out, rb.@byte[(int)(start)..(int)(end)].ꓸꓸꓸ);
    }
    rb.reset();
    return @out;
}

// flushCopy copies the normalized segment to buf and resets rb.
// It returns the number of bytes written to buf.
[GoRecv] internal static nint flushCopy(this ref reorderBuffer rb, slice<byte> buf) {
    nint p = 0;
    for (nint i = 0; i < rb.nrune; i++) {
        var runep = rb.rune[i];
        p += copy(buf[(int)(p)..], rb.@byte[(int)(runep.pos)..(int)(runep.pos + runep.size)]);
    }
    rb.reset();
    return p;
}

// insertOrdered inserts a rune in the buffer, ordered by Canonical Combining Class.
// It returns false if the buffer is not large enough to hold the rune.
// It is used internally by insert and insertString only.
[GoRecv] internal static void insertOrdered(this ref reorderBuffer rb, ΔProperties info) {
    nint n = rb.nrune;
    var b = rb.rune[..];
    var cc = info.ccc;
    if (cc > 0) {
        // Find insertion position + move elements to make room.
        for (; n > 0; n--) {
            if (b[n - 1].ccc <= cc) {
                break;
            }
            b[n] = b[n - 1];
        }
    }
    rb.nrune += 1;
    var pos = ((uint8)rb.nbyte);
    rb.nbyte += utf8.UTFMax;
    info.pos = pos;
    b[n] = info;
}

[GoType("num:nint")] partial struct insertErr;

internal static readonly insertErr iSuccess = /* -iota */ 0;
internal static readonly insertErr iShortDst = -1;
internal static readonly insertErr iShortSrc = -2;

// insertFlush inserts the given rune in the buffer ordered by CCC.
// If a decomposition with multiple segments are encountered, they leading
// ones are flushed.
// It returns a non-zero error code if the rune was not inserted.
[GoRecv] internal static insertErr insertFlush(this ref reorderBuffer rb, input src, nint i, ΔProperties info) {
    {
        var rune = src.hangul(i); if (rune != 0) {
            rb.decomposeHangul(rune);
            return iSuccess;
        }
    }
    if (info.hasDecomposition()) {
        return rb.insertDecomposed(info.Decomposition());
    }
    rb.insertSingle(src, i, info);
    return iSuccess;
}

// insertUnsafe inserts the given rune in the buffer ordered by CCC.
// It is assumed there is sufficient space to hold the runes. It is the
// responsibility of the caller to ensure this. This can be done by checking
// the state returned by the streamSafe type.
[GoRecv] internal static void insertUnsafe(this ref reorderBuffer rb, input src, nint i, ΔProperties info) {
    {
        var rune = src.hangul(i); if (rune != 0) {
            rb.decomposeHangul(rune);
        }
    }
    if (info.hasDecomposition()){
        // TODO: inline.
        rb.insertDecomposed(info.Decomposition());
    } else {
        rb.insertSingle(src, i, info);
    }
}

// insertDecomposed inserts an entry in to the reorderBuffer for each rune
// in dcomp. dcomp must be a sequence of decomposed UTF-8-encoded runes.
// It flushes the buffer on each new segment start.
[GoRecv] internal static insertErr insertDecomposed(this ref reorderBuffer rb, slice<byte> dcomp) {
    rb.tmpBytes.setBytes(dcomp);
    // As the streamSafe accounting already handles the counting for modifiers,
    // we don't have to call next. However, we do need to keep the accounting
    // intact when flushing the buffer.
    for (nint i = 0; i < len(dcomp); ) {
        var info = rb.f.info(rb.tmpBytes, i);
        if (info.BoundaryBefore() && rb.nrune > 0 && !rb.doFlush()) {
            return iShortDst;
        }
        i += copy(rb.@byte[(int)(rb.nbyte)..], dcomp[(int)(i)..(int)(i + ((nint)info.size))]);
        rb.insertOrdered(info);
    }
    return iSuccess;
}

// insertSingle inserts an entry in the reorderBuffer for the rune at
// position i. info is the runeInfo for the rune at position i.
[GoRecv] internal static void insertSingle(this ref reorderBuffer rb, input src, nint i, ΔProperties info) {
    src.copySlice(rb.@byte[(int)(rb.nbyte)..], i, i + ((nint)info.size));
    rb.insertOrdered(info);
}

// insertCGJ inserts a Combining Grapheme Joiner (0x034f) into rb.
[GoRecv] internal static void insertCGJ(this ref reorderBuffer rb) {
    rb.insertSingle(new input(str: GraphemeJoiner), 0, new ΔProperties(size: ((uint8)len(GraphemeJoiner))));
}

// appendRune inserts a rune at the end of the buffer. It is used for Hangul.
[GoRecv] internal static void appendRune(this ref reorderBuffer rb, rune r) {
    var bn = rb.nbyte;
    nint sz = utf8.EncodeRune(rb.@byte[(int)(bn)..], ((rune)r));
    rb.nbyte += utf8.UTFMax;
    rb.rune[rb.nrune] = new ΔProperties(pos: bn, size: ((uint8)sz));
    rb.nrune++;
}

// assignRune sets a rune at position pos. It is used for Hangul and recomposition.
[GoRecv] internal static void assignRune(this ref reorderBuffer rb, nint pos, rune r) {
    var bn = rb.rune[pos].pos;
    nint sz = utf8.EncodeRune(rb.@byte[(int)(bn)..], ((rune)r));
    rb.rune[pos] = new ΔProperties(pos: bn, size: ((uint8)sz));
}

// runeAt returns the rune at position n. It is used for Hangul and recomposition.
[GoRecv] internal static rune runeAt(this ref reorderBuffer rb, nint n) {
    var inf = rb.rune[n];
    var (r, _) = utf8.DecodeRune(rb.@byte[(int)(inf.pos)..(int)(inf.pos + inf.size)]);
    return r;
}

// bytesAt returns the UTF-8 encoding of the rune at position n.
// It is used for Hangul and recomposition.
[GoRecv] internal static slice<byte> bytesAt(this ref reorderBuffer rb, nint n) {
    var inf = rb.rune[n];
    return rb.@byte[(int)(inf.pos)..(int)(((nint)inf.pos) + ((nint)inf.size))];
}

// For Hangul we combine algorithmically, instead of using tables.
internal static readonly UntypedInt hangulBase = /* 0xAC00 */ 44032; // UTF-8(hangulBase) -> EA B0 80

internal static readonly UntypedInt hangulBase0 = /* 0xEA */ 234;

internal static readonly UntypedInt hangulBase1 = /* 0xB0 */ 176;

internal static readonly UntypedInt hangulBase2 = /* 0x80 */ 128;

internal static readonly UntypedInt hangulEnd = /* hangulBase + jamoLVTCount */ 55204; // UTF-8(0xD7A4) -> ED 9E A4

internal static readonly UntypedInt hangulEnd0 = /* 0xED */ 237;

internal static readonly UntypedInt hangulEnd1 = /* 0x9E */ 158;

internal static readonly UntypedInt hangulEnd2 = /* 0xA4 */ 164;

internal static readonly UntypedInt jamoLBase = /* 0x1100 */ 4352; // UTF-8(jamoLBase) -> E1 84 00

internal static readonly UntypedInt jamoLBase0 = /* 0xE1 */ 225;

internal static readonly UntypedInt jamoLBase1 = /* 0x84 */ 132;

internal static readonly UntypedInt jamoLEnd = /* 0x1113 */ 4371;

internal static readonly UntypedInt jamoVBase = /* 0x1161 */ 4449;

internal static readonly UntypedInt jamoVEnd = /* 0x1176 */ 4470;

internal static readonly UntypedInt jamoTBase = /* 0x11A7 */ 4519;

internal static readonly UntypedInt jamoTEnd = /* 0x11C3 */ 4547;

internal static readonly UntypedInt jamoTCount = 28;

internal static readonly UntypedInt jamoVCount = 21;

internal static readonly UntypedInt jamoVTCount = /* 21 * 28 */ 588;

internal static readonly UntypedInt jamoLVTCount = /* 19 * 21 * 28 */ 11172;

internal static readonly UntypedInt hangulUTF8Size = 3;

internal static bool isHangul(slice<byte> b) {
    if (len(b) < hangulUTF8Size) {
        return false;
    }
    var b0 = b[0];
    if (b0 < hangulBase0) {
        return false;
    }
    var b1 = b[1];
    switch (ᐧ) {
    case {} when b0 is hangulBase0: {
        return b1 >= hangulBase1;
    }
    case {} when b0 is < hangulEnd0: {
        return true;
    }
    case {} when b0 is > hangulEnd0: {
        return false;
    }
    case {} when b1 is < hangulEnd1: {
        return true;
    }}

    return b1 == hangulEnd1 && b[2] < hangulEnd2;
}

internal static bool isHangulString(@string b) {
    if (len(b) < hangulUTF8Size) {
        return false;
    }
    var b0 = b[0];
    if (b0 < hangulBase0) {
        return false;
    }
    var b1 = b[1];
    switch (ᐧ) {
    case {} when b0 is hangulBase0: {
        return b1 >= hangulBase1;
    }
    case {} when b0 is < hangulEnd0: {
        return true;
    }
    case {} when b0 is > hangulEnd0: {
        return false;
    }
    case {} when b1 is < hangulEnd1: {
        return true;
    }}

    return b1 == hangulEnd1 && b[2] < hangulEnd2;
}

// Caller must ensure len(b) >= 2.
internal static bool isJamoVT(slice<byte> b) {
    // True if (rune & 0xff00) == jamoLBase
    return b[0] == jamoLBase0 && ((byte)(b[1] & 252)) == jamoLBase1;
}

internal static bool isHangulWithoutJamoT(slice<byte> b) {
    var (c, _) = utf8.DecodeRune(b);
    c -= hangulBase;
    return c < jamoLVTCount && c % jamoTCount == 0;
}

// decomposeHangul writes the decomposed Hangul to buf and returns the number
// of bytes written.  len(buf) should be at least 9.
internal static nint decomposeHangul(slice<byte> buf, rune r) {
    static readonly UntypedInt JamoUTF8Len = 3;
    r -= hangulBase;
    var x = r % jamoTCount;
    r /= jamoTCount;
    utf8.EncodeRune(buf, jamoLBase + r / jamoVCount);
    utf8.EncodeRune(buf[(int)(JamoUTF8Len)..], jamoVBase + r % jamoVCount);
    if (x != 0) {
        utf8.EncodeRune(buf[(int)(2 * JamoUTF8Len)..], jamoTBase + x);
        return 3 * JamoUTF8Len;
    }
    return 2 * JamoUTF8Len;
}

// decomposeHangul algorithmically decomposes a Hangul rune into
// its Jamo components.
// See https://unicode.org/reports/tr15/#Hangul for details on decomposing Hangul.
[GoRecv] internal static void decomposeHangul(this ref reorderBuffer rb, rune r) {
    r -= hangulBase;
    var x = r % jamoTCount;
    r /= jamoTCount;
    rb.appendRune(jamoLBase + r / jamoVCount);
    rb.appendRune(jamoVBase + r % jamoVCount);
    if (x != 0) {
        rb.appendRune(jamoTBase + x);
    }
}

// combineHangul algorithmically combines Jamo character components into Hangul.
// See https://unicode.org/reports/tr15/#Hangul for details on combining Hangul.
[GoRecv] internal static void combineHangul(this ref reorderBuffer rb, nint s, nint i, nint k) {
    var b = rb.rune[..];
    nint bn = rb.nrune;
    for (; i < bn; i++) {
        var cccB = b[k - 1].ccc;
        var cccC = b[i].ccc;
        if (cccB == 0) {
            s = k - 1;
        }
        if (s != k - 1 && cccB >= cccC){
            // b[i] is blocked by greater-equal cccX below it
            b[k] = b[i];
            k++;
        } else {
            var l = rb.runeAt(s);
            // also used to compare to hangulBase
            var v = rb.runeAt(i);
            // also used to compare to jamoT
            switch (ᐧ) {
            case {} when jamoLBase <= l && l < jamoLEnd && jamoVBase <= v && v < jamoVEnd: {
                rb.assignRune(s, // 11xx plus 116x to LV
 hangulBase + (l - jamoLBase) * jamoVTCount + (v - jamoVBase) * jamoTCount);
                break;
            }
            case {} when hangulBase <= l && l < hangulEnd && jamoTBase < v && v < jamoTEnd && ((l - hangulBase) % jamoTCount) == 0: {
                rb.assignRune(s, // ACxx plus 11Ax to LVT
 l + v - jamoTBase);
                break;
            }
            default: {
                b[k] = b[i];
                k++;
                break;
            }}

        }
    }
    rb.nrune = k;
}

// compose recombines the runes in the buffer.
// It should only be used to recompose a single segment, as it will not
// handle alternations between Hangul and non-Hangul characters correctly.
[GoRecv] internal static void compose(this ref reorderBuffer rb) {
    // Lazily load the map used by the combine func below, but do
    // it outside of the loop.
    recompMapOnce.Do(buildRecompMap);
    // UAX #15, section X5 , including Corrigendum #5
    // "In any character sequence beginning with starter S, a character C is
    //  blocked from S if and only if there is some character B between S
    //  and C, and either B is a starter or it has the same or higher
    //  combining class as C."
    nint bn = rb.nrune;
    if (bn == 0) {
        return;
    }
    nint k = 1;
    var b = rb.rune[..];
    for (nint s = 0;nint i = 1; i < bn; i++) {
        if (isJamoVT(rb.bytesAt(i))) {
            // Redo from start in Hangul mode. Necessary to support
            // U+320E..U+321E in NFKC mode.
            rb.combineHangul(s, i, k);
            return;
        }
        var ii = b[i];
        // We can only use combineForward as a filter if we later
        // get the info for the combined character. This is more
        // expensive than using the filter. Using combinesBackward()
        // is safe.
        if (ii.combinesBackward()) {
            var cccB = b[k - 1].ccc;
            var cccC = ii.ccc;
            var blocked = false;
            // b[i] blocked by starter or greater or equal CCC?
            if (cccB == 0){
                s = k - 1;
            } else {
                blocked = s != k - 1 && cccB >= cccC;
            }
            if (!blocked) {
                var combined = combine(rb.runeAt(s), rb.runeAt(i));
                if (combined != 0) {
                    rb.assignRune(s, combined);
                    continue;
                }
            }
        }
        b[k] = b[i];
        k++;
    }
    rb.nrune = k;
}

} // end norm_package
