// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 06 23:38:49 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\composition.go
using utf8 = go.unicode.utf8_package;
using System;


namespace go.vendor.golang.org.x.text.unicode;

public static partial class norm_package {

private static readonly nint maxNonStarters = 30; 
// The maximum number of characters needed for a buffer is
// maxNonStarters + 1 for the starter + 1 for the GCJ
private static readonly var maxBufferSize = maxNonStarters + 2;
private static readonly nint maxNFCExpansion = 3; // NFC(0x1D160)
private static readonly nint maxNFKCExpansion = 18; // NFKC(0xFDFA)

private static readonly var maxByteBufferSize = utf8.UTFMax * maxBufferSize; // 128

// ssState is used for reporting the segment state after inserting a rune.
// It is returned by streamSafe.next.
private partial struct ssState { // : nint
}

 
// Indicates a rune was successfully added to the segment.
private static readonly ssState ssSuccess = iota; 
// Indicates a rune starts a new segment and should not be added.
private static readonly var ssStarter = 0; 
// Indicates a rune caused a segment overflow and a CGJ should be inserted.
private static readonly var ssOverflow = 1;


// streamSafe implements the policy of when a CGJ should be inserted.
private partial struct streamSafe { // : byte
}

// first inserts the first rune of a segment. It is a faster version of next if
// it is known p represents the first rune in a segment.
private static void first(this ptr<streamSafe> _addr_ss, Properties p) {
    ref streamSafe ss = ref _addr_ss.val;

    ss.val = streamSafe(p.nTrailingNonStarters());
}

// insert returns a ssState value to indicate whether a rune represented by p
// can be inserted.
private static ssState next(this ptr<streamSafe> _addr_ss, Properties p) => func((_, panic, _) => {
    ref streamSafe ss = ref _addr_ss.val;

    if (ss > maxNonStarters.val) {
        panic("streamSafe was not reset");
    }
    var n = p.nLeadingNonStarters();
    ss.val += streamSafe(n);

    if (ss > maxNonStarters.val) {
        ss.val = 0;
        return ssOverflow;
    }
    if (n == 0) {
        ss.val = streamSafe(p.nTrailingNonStarters());
        return ssStarter;
    }
    return ssSuccess;

});

// backwards is used for checking for overflow and segment starts
// when traversing a string backwards. Users do not need to call first
// for the first rune. The state of the streamSafe retains the count of
// the non-starters loaded.
private static ssState backwards(this ptr<streamSafe> _addr_ss, Properties p) => func((_, panic, _) => {
    ref streamSafe ss = ref _addr_ss.val;

    if (ss > maxNonStarters.val) {
        panic("streamSafe was not reset");
    }
    var c = ss + streamSafe(p.nTrailingNonStarters()).val;
    if (c > maxNonStarters) {
        return ssOverflow;
    }
    ss.val = c;
    if (p.nLeadingNonStarters() == 0) {
        return ssStarter;
    }
    return ssSuccess;

});

private static bool isMax(this streamSafe ss) {
    return ss == maxNonStarters;
}

// GraphemeJoiner is inserted after maxNonStarters non-starter runes.
public static readonly @string GraphemeJoiner = "\u034F";

// reorderBuffer is used to normalize a single segment.  Characters inserted with
// insert are decomposed and reordered based on CCC. The compose method can
// be used to recombine characters.  Note that the byte buffer does not hold
// the UTF-8 characters in order.  Only the rune array is maintained in sorted
// order. flush writes the resulting segment to a byte array.


// reorderBuffer is used to normalize a single segment.  Characters inserted with
// insert are decomposed and reordered based on CCC. The compose method can
// be used to recombine characters.  Note that the byte buffer does not hold
// the UTF-8 characters in order.  Only the rune array is maintained in sorted
// order. flush writes the resulting segment to a byte array.
private partial struct reorderBuffer {
    public array<Properties> rune; // Per character info.
    public array<byte> @byte; // UTF-8 buffer. Referenced by runeInfo.pos.
    public byte nbyte; // Number or bytes.
    public streamSafe ss; // For limiting length of non-starter sequence.
    public nint nrune; // Number of runeInfos.
    public formInfo f;
    public input src;
    public nint nsrc;
    public input tmpBytes;
    public slice<byte> @out;
    public Func<ptr<reorderBuffer>, bool> flushF;
}

private static void init(this ptr<reorderBuffer> _addr_rb, Form f, slice<byte> src) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.f = formTable[f].val;
    rb.src.setBytes(src);
    rb.nsrc = len(src);
    rb.ss = 0;
}

private static void initString(this ptr<reorderBuffer> _addr_rb, Form f, @string src) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.f = formTable[f].val;
    rb.src.setString(src);
    rb.nsrc = len(src);
    rb.ss = 0;
}

private static bool setFlusher(this ptr<reorderBuffer> _addr_rb, slice<byte> @out, Func<ptr<reorderBuffer>, bool> f) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.@out = out;
    rb.flushF = f;
}

// reset discards all characters from the buffer.
private static void reset(this ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.nrune = 0;
    rb.nbyte = 0;
}

private static bool doFlush(this ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;

    if (rb.f.composing) {
        rb.compose();
    }
    var res = rb.flushF(rb);
    rb.reset();
    return res;

}

// appendFlush appends the normalized segment to rb.out.
private static bool appendFlush(ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;

    for (nint i = 0; i < rb.nrune; i++) {
        var start = rb.rune[i].pos;
        var end = start + rb.rune[i].size;
        rb.@out = append(rb.@out, rb.@byte[(int)start..(int)end]);
    }
    return true;
}

// flush appends the normalized segment to out and resets rb.
private static slice<byte> flush(this ptr<reorderBuffer> _addr_rb, slice<byte> @out) {
    ref reorderBuffer rb = ref _addr_rb.val;

    for (nint i = 0; i < rb.nrune; i++) {
        var start = rb.rune[i].pos;
        var end = start + rb.rune[i].size;
        out = append(out, rb.@byte[(int)start..(int)end]);
    }
    rb.reset();
    return out;
}

// flushCopy copies the normalized segment to buf and resets rb.
// It returns the number of bytes written to buf.
private static nint flushCopy(this ptr<reorderBuffer> _addr_rb, slice<byte> buf) {
    ref reorderBuffer rb = ref _addr_rb.val;

    nint p = 0;
    for (nint i = 0; i < rb.nrune; i++) {
        var runep = rb.rune[i];
        p += copy(buf[(int)p..], rb.@byte[(int)runep.pos..(int)runep.pos + runep.size]);
    }
    rb.reset();
    return p;
}

// insertOrdered inserts a rune in the buffer, ordered by Canonical Combining Class.
// It returns false if the buffer is not large enough to hold the rune.
// It is used internally by insert and insertString only.
private static void insertOrdered(this ptr<reorderBuffer> _addr_rb, Properties info) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var n = rb.nrune;
    var b = rb.rune[..];
    var cc = info.ccc;
    if (cc > 0) { 
        // Find insertion position + move elements to make room.
        while (n > 0) {
            if (b[n - 1].ccc <= cc) {
                break;
            n--;
            }

            b[n] = b[n - 1];

        }

    }
    rb.nrune += 1;
    var pos = uint8(rb.nbyte);
    rb.nbyte += utf8.UTFMax;
    info.pos = pos;
    b[n] = info;

}

// insertErr is an error code returned by insert. Using this type instead
// of error improves performance up to 20% for many of the benchmarks.
private partial struct insertErr { // : nint
}

private static readonly insertErr iSuccess = -iota;
private static readonly var iShortDst = 0;
private static readonly var iShortSrc = 1;


// insertFlush inserts the given rune in the buffer ordered by CCC.
// If a decomposition with multiple segments are encountered, they leading
// ones are flushed.
// It returns a non-zero error code if the rune was not inserted.
private static insertErr insertFlush(this ptr<reorderBuffer> _addr_rb, input src, nint i, Properties info) {
    ref reorderBuffer rb = ref _addr_rb.val;

    {
        var rune = src.hangul(i);

        if (rune != 0) {
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
private static void insertUnsafe(this ptr<reorderBuffer> _addr_rb, input src, nint i, Properties info) {
    ref reorderBuffer rb = ref _addr_rb.val;

    {
        var rune = src.hangul(i);

        if (rune != 0) {
            rb.decomposeHangul(rune);
        }
    }

    if (info.hasDecomposition()) { 
        // TODO: inline.
        rb.insertDecomposed(info.Decomposition());

    }
    else
 {
        rb.insertSingle(src, i, info);
    }
}

// insertDecomposed inserts an entry in to the reorderBuffer for each rune
// in dcomp. dcomp must be a sequence of decomposed UTF-8-encoded runes.
// It flushes the buffer on each new segment start.
private static insertErr insertDecomposed(this ptr<reorderBuffer> _addr_rb, slice<byte> dcomp) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.tmpBytes.setBytes(dcomp); 
    // As the streamSafe accounting already handles the counting for modifiers,
    // we don't have to call next. However, we do need to keep the accounting
    // intact when flushing the buffer.
    {
        nint i = 0;

        while (i < len(dcomp)) {
            var info = rb.f.info(rb.tmpBytes, i);
            if (info.BoundaryBefore() && rb.nrune > 0 && !rb.doFlush()) {
                return iShortDst;
            }
            i += copy(rb.@byte[(int)rb.nbyte..], dcomp[(int)i..(int)i + int(info.size)]);
            rb.insertOrdered(info);
        }
    }
    return iSuccess;

}

// insertSingle inserts an entry in the reorderBuffer for the rune at
// position i. info is the runeInfo for the rune at position i.
private static void insertSingle(this ptr<reorderBuffer> _addr_rb, input src, nint i, Properties info) {
    ref reorderBuffer rb = ref _addr_rb.val;

    src.copySlice(rb.@byte[(int)rb.nbyte..], i, i + int(info.size));
    rb.insertOrdered(info);
}

// insertCGJ inserts a Combining Grapheme Joiner (0x034f) into rb.
private static void insertCGJ(this ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;

    rb.insertSingle(new input(str:GraphemeJoiner), 0, new Properties(size:uint8(len(GraphemeJoiner))));
}

// appendRune inserts a rune at the end of the buffer. It is used for Hangul.
private static void appendRune(this ptr<reorderBuffer> _addr_rb, int r) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var bn = rb.nbyte;
    var sz = utf8.EncodeRune(rb.@byte[(int)bn..], rune(r));
    rb.nbyte += utf8.UTFMax;
    rb.rune[rb.nrune] = new Properties(pos:bn,size:uint8(sz));
    rb.nrune++;
}

// assignRune sets a rune at position pos. It is used for Hangul and recomposition.
private static void assignRune(this ptr<reorderBuffer> _addr_rb, nint pos, int r) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var bn = rb.rune[pos].pos;
    var sz = utf8.EncodeRune(rb.@byte[(int)bn..], rune(r));
    rb.rune[pos] = new Properties(pos:bn,size:uint8(sz));
}

// runeAt returns the rune at position n. It is used for Hangul and recomposition.
private static int runeAt(this ptr<reorderBuffer> _addr_rb, nint n) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var inf = rb.rune[n];
    var (r, _) = utf8.DecodeRune(rb.@byte[(int)inf.pos..(int)inf.pos + inf.size]);
    return r;
}

// bytesAt returns the UTF-8 encoding of the rune at position n.
// It is used for Hangul and recomposition.
private static slice<byte> bytesAt(this ptr<reorderBuffer> _addr_rb, nint n) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var inf = rb.rune[n];
    return rb.@byte[(int)inf.pos..(int)int(inf.pos) + int(inf.size)];
}

// For Hangul we combine algorithmically, instead of using tables.
private static readonly nuint hangulBase = 0xAC00; // UTF-8(hangulBase) -> EA B0 80
private static readonly nuint hangulBase0 = 0xEA;
private static readonly nuint hangulBase1 = 0xB0;
private static readonly nuint hangulBase2 = 0x80;

private static readonly var hangulEnd = hangulBase + jamoLVTCount; // UTF-8(0xD7A4) -> ED 9E A4
private static readonly nuint hangulEnd0 = 0xED;
private static readonly nuint hangulEnd1 = 0x9E;
private static readonly nuint hangulEnd2 = 0xA4;

private static readonly nuint jamoLBase = 0x1100; // UTF-8(jamoLBase) -> E1 84 00
private static readonly nuint jamoLBase0 = 0xE1;
private static readonly nuint jamoLBase1 = 0x84;
private static readonly nuint jamoLEnd = 0x1113;
private static readonly nuint jamoVBase = 0x1161;
private static readonly nuint jamoVEnd = 0x1176;
private static readonly nuint jamoTBase = 0x11A7;
private static readonly nuint jamoTEnd = 0x11C3;

private static readonly nint jamoTCount = 28;
private static readonly nint jamoVCount = 21;
private static readonly nint jamoVTCount = 21 * 28;
private static readonly nint jamoLVTCount = 19 * 21 * 28;


private static readonly nint hangulUTF8Size = 3;



private static bool isHangul(slice<byte> b) {
    if (len(b) < hangulUTF8Size) {
        return false;
    }
    var b0 = b[0];
    if (b0 < hangulBase0) {
        return false;
    }
    var b1 = b[1];

    if (b0 == hangulBase0) 
        return b1 >= hangulBase1;
    else if (b0 < hangulEnd0) 
        return true;
    else if (b0 > hangulEnd0) 
        return false;
    else if (b1 < hangulEnd1) 
        return true;
        return b1 == hangulEnd1 && b[2] < hangulEnd2;

}

private static bool isHangulString(@string b) {
    if (len(b) < hangulUTF8Size) {
        return false;
    }
    var b0 = b[0];
    if (b0 < hangulBase0) {
        return false;
    }
    var b1 = b[1];

    if (b0 == hangulBase0) 
        return b1 >= hangulBase1;
    else if (b0 < hangulEnd0) 
        return true;
    else if (b0 > hangulEnd0) 
        return false;
    else if (b1 < hangulEnd1) 
        return true;
        return b1 == hangulEnd1 && b[2] < hangulEnd2;

}

// Caller must ensure len(b) >= 2.
private static bool isJamoVT(slice<byte> b) { 
    // True if (rune & 0xff00) == jamoLBase
    return b[0] == jamoLBase0 && (b[1] & 0xFC) == jamoLBase1;

}

private static bool isHangulWithoutJamoT(slice<byte> b) {
    var (c, _) = utf8.DecodeRune(b);
    c -= hangulBase;
    return c < jamoLVTCount && c % jamoTCount == 0;
}

// decomposeHangul writes the decomposed Hangul to buf and returns the number
// of bytes written.  len(buf) should be at least 9.
private static nint decomposeHangul(slice<byte> buf, int r) {
    const nint JamoUTF8Len = 3;

    r -= hangulBase;
    var x = r % jamoTCount;
    r /= jamoTCount;
    utf8.EncodeRune(buf, jamoLBase + r / jamoVCount);
    utf8.EncodeRune(buf[(int)JamoUTF8Len..], jamoVBase + r % jamoVCount);
    if (x != 0) {
        utf8.EncodeRune(buf[(int)2 * JamoUTF8Len..], jamoTBase + x);
        return 3 * JamoUTF8Len;
    }
    return 2 * JamoUTF8Len;

}

// decomposeHangul algorithmically decomposes a Hangul rune into
// its Jamo components.
// See https://unicode.org/reports/tr15/#Hangul for details on decomposing Hangul.
private static void decomposeHangul(this ptr<reorderBuffer> _addr_rb, int r) {
    ref reorderBuffer rb = ref _addr_rb.val;

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
private static void combineHangul(this ptr<reorderBuffer> _addr_rb, nint s, nint i, nint k) {
    ref reorderBuffer rb = ref _addr_rb.val;

    var b = rb.rune[..];
    var bn = rb.nrune;
    while (i < bn) {
        var cccB = b[k - 1].ccc;
        var cccC = b[i].ccc;
        if (cccB == 0) {
            s = k - 1;
        i++;
        }
        if (s != k - 1 && cccB >= cccC) { 
            // b[i] is blocked by greater-equal cccX below it
            b[k] = b[i];
            k++;

        }
        else
 {
            var l = rb.runeAt(s); // also used to compare to hangulBase
            var v = rb.runeAt(i); // also used to compare to jamoT

            if (jamoLBase <= l && l < jamoLEnd && jamoVBase <= v && v < jamoVEnd) 
                // 11xx plus 116x to LV
                rb.assignRune(s, hangulBase + (l - jamoLBase) * jamoVTCount + (v - jamoVBase) * jamoTCount);
            else if (hangulBase <= l && l < hangulEnd && jamoTBase < v && v < jamoTEnd && ((l - hangulBase) % jamoTCount) == 0) 
                // ACxx plus 11Ax to LVT
                rb.assignRune(s, l + v - jamoTBase);
            else 
                b[k] = b[i];
                k++;
            
        }
    }
    rb.nrune = k;

}

// compose recombines the runes in the buffer.
// It should only be used to recompose a single segment, as it will not
// handle alternations between Hangul and non-Hangul characters correctly.
private static void compose(this ptr<reorderBuffer> _addr_rb) {
    ref reorderBuffer rb = ref _addr_rb.val;
 
    // Lazily load the map used by the combine func below, but do
    // it outside of the loop.
    recompMapOnce.Do(buildRecompMap); 

    // UAX #15, section X5 , including Corrigendum #5
    // "In any character sequence beginning with starter S, a character C is
    //  blocked from S if and only if there is some character B between S
    //  and C, and either B is a starter or it has the same or higher
    //  combining class as C."
    var bn = rb.nrune;
    if (bn == 0) {
        return ;
    }
    nint k = 1;
    var b = rb.rune[..];
    for (nint s = 0;
    nint i = 1; i < bn; i++) {
        if (isJamoVT(rb.bytesAt(i))) { 
            // Redo from start in Hangul mode. Necessary to support
            // U+320E..U+321E in NFKC mode.
            rb.combineHangul(s, i, k);
            return ;

        }
        var ii = b[i]; 
        // We can only use combineForward as a filter if we later
        // get the info for the combined character. This is more
        // expensive than using the filter. Using combinesBackward()
        // is safe.
        if (ii.combinesBackward()) {
            var cccB = b[k - 1].ccc;
            var cccC = ii.ccc;
            var blocked = false; // b[i] blocked by starter or greater or equal CCC?
            if (cccB == 0) {
                s = k - 1;
            }
            else
 {
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
