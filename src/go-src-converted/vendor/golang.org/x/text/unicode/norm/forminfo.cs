// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package norm -- go2cs converted at 2022 March 13 06:47:04 UTC
// import "vendor/golang.org/x/text/unicode/norm" ==> using norm = go.vendor.golang.org.x.text.unicode.norm_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\norm\forminfo.go
namespace go.vendor.golang.org.x.text.unicode;

using binary = encoding.binary_package;

public static partial class norm_package {

// This file contains Form-specific logic and wrappers for data in tables.go.

// Rune info is stored in a separate trie per composing form. A composing form
// and its corresponding decomposing form share the same trie.  Each trie maps
// a rune to a uint16. The values take two forms.  For v >= 0x8000:
//   bits
//   15:    1 (inverse of NFD_QC bit of qcInfo)
//   13..7: qcInfo (see below). isYesD is always true (no decompostion).
//    6..0: ccc (compressed CCC value).
// For v < 0x8000, the respective rune has a decomposition and v is an index
// into a byte array of UTF-8 decomposition sequences and additional info and
// has the form:
//    <header> <decomp_byte>* [<tccc> [<lccc>]]
// The header contains the number of bytes in the decomposition (excluding this
// length byte). The two most significant bits of this length byte correspond
// to bit 5 and 4 of qcInfo (see below).  The byte sequence itself starts at v+1.
// The byte sequence is followed by a trailing and leading CCC if the values
// for these are not zero.  The value of v determines which ccc are appended
// to the sequences.  For v < firstCCC, there are none, for v >= firstCCC,
// the sequence is followed by a trailing ccc, and for v >= firstLeadingCC
// there is an additional leading ccc. The value of tccc itself is the
// trailing CCC shifted left 2 bits. The two least-significant bits of tccc
// are the number of trailing non-starters.

private static readonly nuint qcInfoMask = 0x3F; // to clear all but the relevant bits in a qcInfo
private static readonly nuint headerLenMask = 0x3F; // extract the length value from the header byte
private static readonly nuint headerFlagsMask = 0xC0; // extract the qcInfo bits from the header byte

// Properties provides access to normalization properties of a rune.
public partial struct Properties {
    public byte pos; // start position in reorderBuffer; used in composition.go
    public byte size; // length of UTF-8 encoding of this rune
    public byte ccc; // leading canonical combining class (ccc if not decomposition)
    public byte tccc; // trailing canonical combining class (ccc if not decomposition)
    public byte nLead; // number of leading non-starters.
    public qcInfo flags; // quick check flags
    public ushort index;
}

// functions dispatchable per form
public delegate  Properties lookupFunc(input,  nint);

// formInfo holds Form-specific functions and tables.
private partial struct formInfo {
    public Form form;
    public bool composing; // form type
    public bool compatibility; // form type
    public lookupFunc info;
    public iterFunc nextMain;
}

private static ptr<formInfo> formTable = new slice<ptr<formInfo>>(new ptr<formInfo>[] { {form:NFC,composing:true,compatibility:false,info:lookupInfoNFC,nextMain:nextComposed,}, {form:NFD,composing:false,compatibility:false,info:lookupInfoNFC,nextMain:nextDecomposed,}, {form:NFKC,composing:true,compatibility:true,info:lookupInfoNFKC,nextMain:nextComposed,}, {form:NFKD,composing:false,compatibility:true,info:lookupInfoNFKC,nextMain:nextDecomposed,} });

// We do not distinguish between boundaries for NFC, NFD, etc. to avoid
// unexpected behavior for the user.  For example, in NFD, there is a boundary
// after 'a'.  However, 'a' might combine with modifiers, so from the application's
// perspective it is not a good boundary. We will therefore always use the
// boundaries for the combining variants.

// BoundaryBefore returns true if this rune starts a new segment and
// cannot combine with any rune on the left.
public static bool BoundaryBefore(this Properties p) {
    if (p.ccc == 0 && !p.combinesBackward()) {
        return true;
    }
    return false;
}

// BoundaryAfter returns true if runes cannot combine with or otherwise
// interact with this or previous runes.
public static bool BoundaryAfter(this Properties p) { 
    // TODO: loosen these conditions.
    return p.isInert();
}

// We pack quick check data in 4 bits:
//   5:    Combines forward  (0 == false, 1 == true)
//   4..3: NFC_QC Yes(00), No (10), or Maybe (11)
//   2:    NFD_QC Yes (0) or No (1). No also means there is a decomposition.
//   1..0: Number of trailing non-starters.
//
// When all 4 bits are zero, the character is inert, meaning it is never
// influenced by normalization.
private partial struct qcInfo { // : byte
}

public static bool isYesC(this Properties p) {
    return p.flags & 0x10 == 0;
}
public static bool isYesD(this Properties p) {
    return p.flags & 0x4 == 0;
}

public static bool combinesForward(this Properties p) {
    return p.flags & 0x20 != 0;
}
public static bool combinesBackward(this Properties p) {
    return p.flags & 0x8 != 0;
} // == isMaybe
public static bool hasDecomposition(this Properties p) {
    return p.flags & 0x4 != 0;
} // == isNoD

public static bool isInert(this Properties p) {
    return p.flags & qcInfoMask == 0 && p.ccc == 0;
}

public static bool multiSegment(this Properties p) {
    return p.index >= firstMulti && p.index < endMulti;
}

public static byte nLeadingNonStarters(this Properties p) {
    return p.nLead;
}

public static byte nTrailingNonStarters(this Properties p) {
    return uint8(p.flags & 0x03);
}

// Decomposition returns the decomposition for the underlying rune
// or nil if there is none.
public static slice<byte> Decomposition(this Properties p) { 
    // TODO: create the decomposition for Hangul?
    if (p.index == 0) {
        return null;
    }
    var i = p.index;
    var n = decomps[i] & headerLenMask;
    i++;
    return decomps[(int)i..(int)i + uint16(n)];
}

// Size returns the length of UTF-8 encoding of the rune.
public static nint Size(this Properties p) {
    return int(p.size);
}

// CCC returns the canonical combining class of the underlying rune.
public static byte CCC(this Properties p) {
    if (p.index >= firstCCCZeroExcept) {
        return 0;
    }
    return ccc[p.ccc];
}

// LeadCCC returns the CCC of the first rune in the decomposition.
// If there is no decomposition, LeadCCC equals CCC.
public static byte LeadCCC(this Properties p) {
    return ccc[p.ccc];
}

// TrailCCC returns the CCC of the last rune in the decomposition.
// If there is no decomposition, TrailCCC equals CCC.
public static byte TrailCCC(this Properties p) {
    return ccc[p.tccc];
}

private static void buildRecompMap() {
    recompMap = make_map<uint, int>(len(recompMapPacked) / 8);
    array<byte> buf = new array<byte>(8);
    {
        nint i = 0;

        while (i < len(recompMapPacked)) {
            copy(buf[..], recompMapPacked[(int)i..(int)i + 8]);
            var key = binary.BigEndian.Uint32(buf[..(int)4]);
            var val = binary.BigEndian.Uint32(buf[(int)4..]);
            recompMap[key] = rune(val);
            i += 8;
        }
    }
}

// Recomposition
// We use 32-bit keys instead of 64-bit for the two codepoint keys.
// This clips off the bits of three entries, but we know this will not
// result in a collision. In the unlikely event that changes to
// UnicodeData.txt introduce collisions, the compiler will catch it.
// Note that the recomposition map for NFC and NFKC are identical.

// combine returns the combined rune or 0 if it doesn't exist.
//
// The caller is responsible for calling
// recompMapOnce.Do(buildRecompMap) sometime before this is called.
private static int combine(int a, int b) => func((_, panic, _) => {
    var key = uint32(uint16(a)) << 16 + uint32(uint16(b));
    if (recompMap == null) {
        panic("caller error"); // see func comment
    }
    return recompMap[key];
});

private static Properties lookupInfoNFC(input b, nint i) {
    var (v, sz) = b.charinfoNFC(i);
    return compInfo(v, sz);
}

private static Properties lookupInfoNFKC(input b, nint i) {
    var (v, sz) = b.charinfoNFKC(i);
    return compInfo(v, sz);
}

// Properties returns properties for the first rune in s.
public static Properties Properties(this Form f, slice<byte> s) {
    if (f == NFC || f == NFD) {
        return compInfo(nfcData.lookup(s));
    }
    return compInfo(nfkcData.lookup(s));
}

// PropertiesString returns properties for the first rune in s.
public static Properties PropertiesString(this Form f, @string s) {
    if (f == NFC || f == NFD) {
        return compInfo(nfcData.lookupString(s));
    }
    return compInfo(nfkcData.lookupString(s));
}

// compInfo converts the information contained in v and sz
// to a Properties.  See the comment at the top of the file
// for more information on the format.
private static Properties compInfo(ushort v, nint sz) {
    if (v == 0) {
        return new Properties(size:uint8(sz));
    }
    else if (v >= 0x8000) {
        Properties p = new Properties(size:uint8(sz),ccc:uint8(v),tccc:uint8(v),flags:qcInfo(v>>8),);
        if (p.ccc > 0 || p.combinesBackward()) {
            p.nLead = uint8(p.flags & 0x3);
        }
        return p;
    }
    var h = decomps[v];
    var f = (qcInfo(h & headerFlagsMask) >> 2) | 0x4;
    p = new Properties(size:uint8(sz),flags:f,index:v);
    if (v >= firstCCC) {
        v += uint16(h & headerLenMask) + 1;
        var c = decomps[v];
        p.tccc = c >> 2;
        p.flags |= qcInfo(c & 0x3);
        if (v >= firstLeadingCCC) {
            p.nLead = c & 0x3;
            if (v >= firstStarterWithNLead) { 
                // We were tricked. Remove the decomposition.
                p.flags &= 0x03;
                p.index = 0;
                return p;
            }
            p.ccc = decomps[v + 1];
        }
    }
    return p;
}

} // end norm_package
