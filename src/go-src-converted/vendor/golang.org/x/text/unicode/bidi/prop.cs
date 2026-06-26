// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.text.unicode;

using utf8 = unicode.utf8_package;
using unicode;

partial class bidi_package {

// Properties provides access to BiDi properties of runes.
[GoType] partial struct Properties {
    internal uint8 entry;
    internal uint8 last;
}

internal static ж<bidiTrie> trie = newBidiTrie(0);

// TODO: using this for bidirule reduces the running time by about 5%. Consider
// if this is worth exposing or if we can find a way to speed up the Class
// method.
//
// // CompactClass is like Class, but maps all of the BiDi control classes
// // (LRO, RLO, LRE, RLE, PDF, LRI, RLI, FSI, PDI) to the class Control.
// func (p Properties) CompactClass() Class {
// 	return Class(p.entry & 0x0F)
// }

// Class returns the Bidi class for p.
public static ΔClass Class(this Properties p) {
    ΔClass c = ((ΔClass)((uint8)(p.entry & 15)));
    if (c == Control) {
        c = controlByteToClass[(uint8)(p.last & 15)];
    }
    return c;
}

// IsBracket reports whether the rune is a bracket.
public static bool IsBracket(this Properties p) {
    return (uint8)(p.entry & 240) != 0;
}

// IsOpeningBracket reports whether the rune is an opening bracket.
// IsBracket must return true.
public static bool IsOpeningBracket(this Properties p) {
    return (uint8)(p.entry & openMask) != 0;
}

// TODO: find a better API and expose.
internal static rune reverseBracket(this Properties p, rune r) {
    return (int32)(xorMasks[p.entry >> (int)(xorMaskShift)] ^ r);
}

// U+202D LeftToRightOverride,
// U+202E RightToLeftOverride,
// U+202A LeftToRightEmbedding,
// U+202B RightToLeftEmbedding,
// U+202C PopDirectionalFormat,
// U+2066 LeftToRightIsolate,
// U+2067 RightToLeftIsolate,
// U+2068 FirstStrongIsolate,
// U+2069 PopDirectionalIsolate,
internal static array<ΔClass> controlByteToClass = new array<ΔClass>(16){
    [13] = LRO,
    [14] = RLO,
    [10] = LRE,
    [11] = RLE,
    [12] = PDF,
    [6] = LRI,
    [7] = RLI,
    [8] = FSI,
    [9] = PDI
};

// LookupRune returns properties for r.
public static (Properties p, nint size) LookupRune(rune r) {
    Properties p = default!;
    nint size = default!;

    array<byte> buf = new(4);
    nint n = utf8.EncodeRune(buf[..], r);
    return Lookup(buf[..(int)(n)]);
}

// TODO: these lookup methods are based on the generated trie code. The returned
// sizes have slightly different semantics from the generated code, in that it
// always returns size==1 for an illegal UTF-8 byte (instead of the length
// of the maximum invalid subsequence). Most Transformers, like unicode/norm,
// leave invalid UTF-8 untouched, in which case it has performance benefits to
// do so (without changing the semantics). Bidi requires the semantics used here
// for the bidirule implementation to be compatible with the Go semantics.
//  They ultimately should perhaps be adopted by all trie implementations, for
// convenience sake.
// This unrolled code also boosts performance of the secure/bidirule package by
// about 30%.
// So, to remove this code:
//   - add option to trie generator to define return type.
//   - always return 1 byte size for ill-formed UTF-8 runes.

// Lookup returns properties for the first rune in s and the width in bytes of
// its encoding. The size will be 0 if s does not hold enough bytes to complete
// the encoding.
public static (Properties p, nint sz) Lookup(slice<byte> s) {
    Properties p = default!;
    nint sz = default!;

    var c0 = s[0];
    switch (ᐧ) {
    case {} when c0 is < 128: {
        return (new Properties( // is ASCII
entry: bidiValues[c0]), 1);
    }
    case {} when c0 is < 194: {
        return (new Properties(nil), 1);
    }
    case {} when c0 is < 224: {
        if (len(s) < 2) {
            // 2-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c1)), 2);
    }
    case {} when c0 is < 240: {
        if (len(s) < 3) {
            // 3-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        var o = ((uint32)i) << (int)(6) + ((uint32)c1);
        i = bidiIndex[o];
        var c2 = s[2];
        if (c2 < 128 || 192 <= c2) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c2), last: c2), 3);
    }
    case {} when c0 is < 248: {
        if (len(s) < 4) {
            // 4-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        var o = ((uint32)i) << (int)(6) + ((uint32)c1);
        i = bidiIndex[o];
        var c2 = s[2];
        if (c2 < 128 || 192 <= c2) {
            return (new Properties(nil), 1);
        }
        o = ((uint32)i) << (int)(6) + ((uint32)c2);
        i = bidiIndex[o];
        var c3 = s[3];
        if (c3 < 128 || 192 <= c3) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c3)), 4);
    }}

    // Illegal rune
    return (new Properties(nil), 1);
}

// LookupString returns properties for the first rune in s and the width in
// bytes of its encoding. The size will be 0 if s does not hold enough bytes to
// complete the encoding.
public static (Properties p, nint sz) LookupString(@string s) {
    Properties p = default!;
    nint sz = default!;

    var c0 = s[0];
    switch (ᐧ) {
    case {} when c0 is < 128: {
        return (new Properties( // is ASCII
entry: bidiValues[c0]), 1);
    }
    case {} when c0 is < 194: {
        return (new Properties(nil), 1);
    }
    case {} when c0 is < 224: {
        if (len(s) < 2) {
            // 2-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c1)), 2);
    }
    case {} when c0 is < 240: {
        if (len(s) < 3) {
            // 3-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        var o = ((uint32)i) << (int)(6) + ((uint32)c1);
        i = bidiIndex[o];
        var c2 = s[2];
        if (c2 < 128 || 192 <= c2) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c2), last: c2), 3);
    }
    case {} when c0 is < 248: {
        if (len(s) < 4) {
            // 4-byte UTF-8
            return (new Properties(nil), 0);
        }
        var i = bidiIndex[c0];
        var c1 = s[1];
        if (c1 < 128 || 192 <= c1) {
            return (new Properties(nil), 1);
        }
        var o = ((uint32)i) << (int)(6) + ((uint32)c1);
        i = bidiIndex[o];
        var c2 = s[2];
        if (c2 < 128 || 192 <= c2) {
            return (new Properties(nil), 1);
        }
        o = ((uint32)i) << (int)(6) + ((uint32)c2);
        i = bidiIndex[o];
        var c3 = s[3];
        if (c3 < 128 || 192 <= c3) {
            return (new Properties(nil), 1);
        }
        return (new Properties(entry: trie.lookupValue(((uint32)i), c3)), 4);
    }}

    // Illegal rune
    return (new Properties(nil), 1);
}

} // end bidi_package
