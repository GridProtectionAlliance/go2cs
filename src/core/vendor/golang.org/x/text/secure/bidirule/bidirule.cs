// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bidirule implements the Bidi Rule defined by RFC 5893.
//
// This package is under development. The API may change without notice and
// without preserving backward compatibility.
namespace go.vendor.golang.org.x.text.secure;

using errors = errors_package;
using utf8 = unicode.utf8_package;
using transform = golang.org.x.text.transform_package;
using bidi = golang.org.x.text.unicode.bidi_package;
using golang.org.x.text;
using golang.org.x.text.unicode;
using unicode;

partial class bidirule_package {

// This file contains an implementation of RFC 5893: Right-to-Left Scripts for
// Internationalized Domain Names for Applications (IDNA)
//
// A label is an individual component of a domain name.  Labels are usually
// shown separated by dots; for example, the domain name "www.example.com" is
// composed of three labels: "www", "example", and "com".
//
// An RTL label is a label that contains at least one character of class R, AL,
// or AN. An LTR label is any label that is not an RTL label.
//
// A "Bidi domain name" is a domain name that contains at least one RTL label.
//
//  The following guarantees can be made based on the above:
//
//  o  In a domain name consisting of only labels that satisfy the rule,
//     the requirements of Section 3 are satisfied.  Note that even LTR
//     labels and pure ASCII labels have to be tested.
//
//  o  In a domain name consisting of only LDH labels (as defined in the
//     Definitions document [RFC5890]) and labels that satisfy the rule,
//     the requirements of Section 3 are satisfied as long as a label
//     that starts with an ASCII digit does not come after a
//     right-to-left label.
//
//  No guarantee is given for other combinations.

// ErrInvalid indicates a label is invalid according to the Bidi Rule.
public static error ErrInvalid = errors.New("bidirule: failed Bidi Rule"u8);

[GoType("num:uint8")] partial struct ruleState;

internal static readonly ruleState ruleInitial = /* iota */ 0;
internal static readonly ruleState ruleLTR = 1;
internal static readonly ruleState ruleLTRFinal = 2;
internal static readonly ruleState ruleRTL = 3;
internal static readonly ruleState ruleRTLFinal = 4;
internal static readonly ruleState ruleInvalid = 5;

[GoType] partial struct ruleTransition {
    internal ruleState next;
    internal uint16 mask;
}

// [2.1] The first character must be a character with Bidi property L, R, or
// AL. If it has the R or AL property, it is an RTL label; if it has the L
// property, it is an LTR label.
// [2.3] In an RTL label, the end of the label must be a character with
// Bidi property R, AL, EN, or AN, followed by zero or more characters
// with Bidi property NSM.
// [2.2] In an RTL label, only characters with the Bidi properties R,
// AL, AN, EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.3]
// [2.3] In an RTL label, the end of the label must be a character with
// Bidi property R, AL, EN, or AN, followed by zero or more characters
// with Bidi property NSM.
// [2.2] In an RTL label, only characters with the Bidi properties R,
// AL, AN, EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.3] and NSM.
// [2.6] In an LTR label, the end of the label must be a character with
// Bidi property L or EN, followed by zero or more characters with Bidi
// property NSM.
// [2.5] In an LTR label, only characters with the Bidi properties L,
// EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.6].
// [2.6] In an LTR label, the end of the label must be a character with
// Bidi property L or EN, followed by zero or more characters with Bidi
// property NSM.
// [2.5] In an LTR label, only characters with the Bidi properties L,
// EN, ES, CS, ET, ON, BN, or NSM are allowed.
// We exclude the entries from [2.6].
internal static array<array<ruleTransition>> transitions = new runtime.SparseArray<array<ruleTransition>>{
    [ruleInitial] = new(
        new(ruleLTRFinal, 1 << (int)(bidi.L)),
        new(ruleRTLFinal, (uint16)(1 << (int)(bidi.R) | 1 << (int)(bidi.AL)))
    ),
    [ruleRTL] = new(
        new(ruleRTLFinal, (uint16)((UntypedInt)((UntypedInt)(1 << (int)(bidi.R) | 1 << (int)(bidi.AL)) | 1 << (int)(bidi.EN)) | 1 << (int)(bidi.AN))),
        new(ruleRTL, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(bidi.ES) | 1 << (int)(bidi.CS)) | 1 << (int)(bidi.ET)) | 1 << (int)(bidi.ON)) | 1 << (int)(bidi.BN)) | 1 << (int)(bidi.NSM)))
    ),
    [ruleRTLFinal] = new(
        new(ruleRTLFinal, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(bidi.R) | 1 << (int)(bidi.AL)) | 1 << (int)(bidi.EN)) | 1 << (int)(bidi.AN)) | 1 << (int)(bidi.NSM))),
        new(ruleRTL, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(bidi.ES) | 1 << (int)(bidi.CS)) | 1 << (int)(bidi.ET)) | 1 << (int)(bidi.ON)) | 1 << (int)(bidi.BN)))
    ),
    [ruleLTR] = new(
        new(ruleLTRFinal, (uint16)(1 << (int)(bidi.L) | 1 << (int)(bidi.EN))),
        new(ruleLTR, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(bidi.ES) | 1 << (int)(bidi.CS)) | 1 << (int)(bidi.ET)) | 1 << (int)(bidi.ON)) | 1 << (int)(bidi.BN)) | 1 << (int)(bidi.NSM)))
    ),
    [ruleLTRFinal] = new(
        new(ruleLTRFinal, (uint16)((UntypedInt)(1 << (int)(bidi.L) | 1 << (int)(bidi.EN)) | 1 << (int)(bidi.NSM))),
        new(ruleLTR, (uint16)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(bidi.ES) | 1 << (int)(bidi.CS)) | 1 << (int)(bidi.ET)) | 1 << (int)(bidi.ON)) | 1 << (int)(bidi.BN)))
    ),
    [ruleInvalid] = new(
        new(ruleInvalid, 0),
        new(ruleInvalid, 0)
    )
}.array();

// [2.4] In an RTL label, if an EN is present, no AN may be present, and
// vice versa.
internal const uint16 exclusiveRTL = /* uint16(1<<bidi.EN | 1<<bidi.AN) */ 36;

// From RFC 5893
// An RTL label is a label that contains at least one character of type
// R, AL, or AN.
//
// An LTR label is any label that is not an RTL label.

// Direction reports the direction of the given label as defined by RFC 5893.
// The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidi.Direction Direction(slice<byte> b) {
    for (nint i = 0; i < len(b); ) {
        var (e, sz) = bidi.Lookup(b[(int)(i)..]);
        if (sz == 0) {
            i++;
        }
        bidi.Class c = e.Class();
        if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
            return bidi.RightToLeft;
        }
        i += sz;
    }
    return bidi.LeftToRight;
}

// DirectionString reports the direction of the given label as defined by RFC
// 5893. The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidi.Direction DirectionString(@string s) {
    for (nint i = 0; i < len(s); ) {
        var (e, sz) = bidi.LookupString(s[(int)(i)..]);
        if (sz == 0) {
            i++;
            continue;
        }
        bidi.Class c = e.Class();
        if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
            return bidi.RightToLeft;
        }
        i += sz;
    }
    return bidi.LeftToRight;
}

// Valid reports whether b conforms to the BiDi rule.
public static bool Valid(slice<byte> b) {
    Transformer t = default!;
    {
        var (n, ok) = t.advance(b); if (!ok || n < len(b)) {
            return false;
        }
    }
    return t.isFinal();
}

// ValidString reports whether s conforms to the BiDi rule.
public static bool ValidString(@string s) {
    Transformer t = default!;
    {
        var (n, ok) = t.advanceString(s); if (!ok || n < len(s)) {
            return false;
        }
    }
    return t.isFinal();
}

// New returns a Transformer that verifies that input adheres to the Bidi Rule.
public static ж<Transformer> New() {
    return Ꮡ(new Transformer(nil));
}

// Transformer implements transform.Transform.
[GoType] partial struct Transformer {
    internal ruleState state;
    internal bool hasRTL;
    internal uint16 seen;
}

// A rule can only be violated for "Bidi Domain names", meaning if one of the
// following categories has been observed.
[GoRecv] internal static bool isRTL(this ref Transformer t) {
    static readonly UntypedInt isRTL = /* 1<<bidi.R | 1<<bidi.AL | 1<<bidi.AN */ 8226;
    return (uint16)(t.seen & isRTL) != 0;
}

// Reset implements transform.Transformer.
[GoRecv] public static void Reset(this ref Transformer t) {
    t = new Transformer(nil);
}

// Transform implements transform.Transformer. This Transformer has state and
// needs to be reset between uses.
[GoRecv] public static (nint nDst, nint nSrc, error err) Transform(this ref Transformer t, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default!;
    nint nSrc = default!;
    error err = default!;

    if (len(dst) < len(src)) {
        src = src[..(int)(len(dst))];
        atEOF = false;
        err = transform.ErrShortDst;
    }
    var (n, err1) = t.Span(src, atEOF);
    copy(dst, src[..(int)(n)]);
    if (err == default! || err1 != default! && !AreEqual(err1, transform.ErrShortSrc)) {
        err = err1;
    }
    return (n, n, err);
}

// Span returns the first n bytes of src that conform to the Bidi rule.
[GoRecv] public static (nint n, error err) Span(this ref Transformer t, slice<byte> src, bool atEOF) {
    nint n = default!;
    error err = default!;

    if (t.state == ruleInvalid && t.isRTL()) {
        return (0, ErrInvalid);
    }
    var (n, ok) = t.advance(src);
    switch (ᐧ) {
    case {} when !ok: {
        err = ErrInvalid;
        break;
    }
    case {} when n < len(src): {
        if (!atEOF) {
            err = transform.ErrShortSrc;
            break;
        }
        err = ErrInvalid;
        break;
    }
    case {} when !t.isFinal(): {
        err = ErrInvalid;
        break;
    }}

    return (n, err);
}

// Precomputing the ASCII values decreases running time for the ASCII fast path
// by about 30%.
internal static array<bidi.Properties> asciiTable;

[GoInit] internal static void init() {
    foreach (var (i, _) in asciiTable) {
        var (p, _) = bidi.LookupRune(((rune)i));
        asciiTable[i] = p;
    }
}

[GoRecv] internal static (nint n, bool ok) advance(this ref Transformer t, slice<byte> s) {
    nint n = default!;
    bool ok = default!;

    bidi.Properties e = default!;
    nint sz = default!;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf){
            (e, sz) = (asciiTable[s[n]], 1);
        } else {
            (e, sz) = bidi.Lookup(s[(int)(n)..]);
            if (sz <= 1) {
                if (sz == 1) {
                    // We always consider invalid UTF-8 to be invalid, even if
                    // the string has not yet been determined to be RTL.
                    // TODO: is this correct?
                    return (n, false);
                }
                return (n, true);
            }
        }
        // incomplete UTF-8 encoding
        // TODO: using CompactClass would result in noticeable speedup.
        // See unicode/bidi/prop.go:Properties.CompactClass.
        var c = ((uint16)(1 << (int)(e.Class())));
        t.seen |= (uint16)(c);
        if ((uint16)(t.seen & exclusiveRTL) == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state];
            switch (ᐧ) {
            case {} when (uint16)(tr[0].mask & c) != 0: {
                t.state = tr[0].next;
                break;
            }
            case {} when (uint16)(tr[1].mask & c) != 0: {
                t.state = tr[1].next;
                break;
            }
            default: {
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }
                break;
            }}
        }

        n += sz;
    }
    return (n, true);
}

[GoRecv] internal static (nint n, bool ok) advanceString(this ref Transformer t, @string s) {
    nint n = default!;
    bool ok = default!;

    bidi.Properties e = default!;
    nint sz = default!;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf){
            (e, sz) = (asciiTable[s[n]], 1);
        } else {
            (e, sz) = bidi.LookupString(s[(int)(n)..]);
            if (sz <= 1) {
                if (sz == 1) {
                    return (n, false);
                }
                // invalid UTF-8
                return (n, true);
            }
        }
        // incomplete UTF-8 encoding
        // TODO: using CompactClass results in noticeable speedup.
        // See unicode/bidi/prop.go:Properties.CompactClass.
        var c = ((uint16)(1 << (int)(e.Class())));
        t.seen |= (uint16)(c);
        if ((uint16)(t.seen & exclusiveRTL) == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state];
            switch (ᐧ) {
            case {} when (uint16)(tr[0].mask & c) != 0: {
                t.state = tr[0].next;
                break;
            }
            case {} when (uint16)(tr[1].mask & c) != 0: {
                t.state = tr[1].next;
                break;
            }
            default: {
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }
                break;
            }}
        }

        n += sz;
    }
    return (n, true);
}

} // end bidirule_package
