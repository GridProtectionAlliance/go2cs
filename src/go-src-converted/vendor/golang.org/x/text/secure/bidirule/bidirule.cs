// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bidirule implements the Bidi Rule defined by RFC 5893.
//
// This package is under development. The API may change without notice and
// without preserving backward compatibility.
// package bidirule -- go2cs converted at 2022 March 06 23:38:22 UTC
// import "vendor/golang.org/x/text/secure/bidirule" ==> using bidirule = go.vendor.golang.org.x.text.secure.bidirule_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\secure\bidirule\bidirule.go
using errors = go.errors_package;
using utf8 = go.unicode.utf8_package;

using transform = go.golang.org.x.text.transform_package;
using bidi = go.golang.org.x.text.unicode.bidi_package;

namespace go.vendor.golang.org.x.text.secure;

public static partial class bidirule_package {

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
public static var ErrInvalid = errors.New("bidirule: failed Bidi Rule");

private partial struct ruleState { // : byte
}

private static readonly ruleState ruleInitial = iota;
private static readonly var ruleLTR = 0;
private static readonly var ruleLTRFinal = 1;
private static readonly var ruleRTL = 2;
private static readonly var ruleRTLFinal = 3;
private static readonly var ruleInvalid = 4;


private partial struct ruleTransition {
    public ruleState next;
    public ushort mask;
}

private static array<array<ruleTransition>> transitions = new array<array<ruleTransition>>(InitKeyedValues<array<ruleTransition>>((ruleInitial, {{ruleLTRFinal,1<<bidi.L},{ruleRTLFinal,1<<bidi.R|1<<bidi.AL},}), (ruleRTL, {{ruleRTLFinal,1<<bidi.R|1<<bidi.AL|1<<bidi.EN|1<<bidi.AN},{ruleRTL,1<<bidi.ES|1<<bidi.CS|1<<bidi.ET|1<<bidi.ON|1<<bidi.BN|1<<bidi.NSM},}), (ruleRTLFinal, {{ruleRTLFinal,1<<bidi.R|1<<bidi.AL|1<<bidi.EN|1<<bidi.AN|1<<bidi.NSM},{ruleRTL,1<<bidi.ES|1<<bidi.CS|1<<bidi.ET|1<<bidi.ON|1<<bidi.BN},}), (ruleLTR, {{ruleLTRFinal,1<<bidi.L|1<<bidi.EN},{ruleLTR,1<<bidi.ES|1<<bidi.CS|1<<bidi.ET|1<<bidi.ON|1<<bidi.BN|1<<bidi.NSM},}), (ruleLTRFinal, {{ruleLTRFinal,1<<bidi.L|1<<bidi.EN|1<<bidi.NSM},{ruleLTR,1<<bidi.ES|1<<bidi.CS|1<<bidi.ET|1<<bidi.ON|1<<bidi.BN},}), (ruleInvalid, {{ruleInvalid,0},{ruleInvalid,0},})));

// [2.4] In an RTL label, if an EN is present, no AN may be present, and
// vice versa.
private static readonly var exclusiveRTL = uint16(1 << (int)(bidi.EN) | 1 << (int)(bidi.AN));

// From RFC 5893
// An RTL label is a label that contains at least one character of type
// R, AL, or AN.
//
// An LTR label is any label that is not an RTL label.

// Direction reports the direction of the given label as defined by RFC 5893.
// The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.


// From RFC 5893
// An RTL label is a label that contains at least one character of type
// R, AL, or AN.
//
// An LTR label is any label that is not an RTL label.

// Direction reports the direction of the given label as defined by RFC 5893.
// The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidi.Direction Direction(slice<byte> b) {
    {
        nint i = 0;

        while (i < len(b)) {
            var (e, sz) = bidi.Lookup(b[(int)i..]);
            if (sz == 0) {
                i++;
            }
            var c = e.Class();
            if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
                return bidi.RightToLeft;
            }
            i += sz;
        }
    }
    return bidi.LeftToRight;

}

// DirectionString reports the direction of the given label as defined by RFC
// 5893. The Bidi Rule does not have to be applied to labels of the category
// LeftToRight.
public static bidi.Direction DirectionString(@string s) {
    {
        nint i = 0;

        while (i < len(s)) {
            var (e, sz) = bidi.LookupString(s[(int)i..]);
            if (sz == 0) {
                i++;
                continue;
            }
            var c = e.Class();
            if (c == bidi.R || c == bidi.AL || c == bidi.AN) {
                return bidi.RightToLeft;
            }
            i += sz;
        }
    }
    return bidi.LeftToRight;

}

// Valid reports whether b conforms to the BiDi rule.
public static bool Valid(slice<byte> b) {
    Transformer t = default;
    {
        var (n, ok) = t.advance(b);

        if (!ok || n < len(b)) {
            return false;
        }
    }

    return t.isFinal();

}

// ValidString reports whether s conforms to the BiDi rule.
public static bool ValidString(@string s) {
    Transformer t = default;
    {
        var (n, ok) = t.advanceString(s);

        if (!ok || n < len(s)) {
            return false;
        }
    }

    return t.isFinal();

}

// New returns a Transformer that verifies that input adheres to the Bidi Rule.
public static ptr<Transformer> New() {
    return addr(new Transformer());
}

// Transformer implements transform.Transform.
public partial struct Transformer {
    public ruleState state;
    public bool hasRTL;
    public ushort seen;
}

// A rule can only be violated for "Bidi Domain names", meaning if one of the
// following categories has been observed.
private static bool isRTL(this ptr<Transformer> _addr_t) {
    ref Transformer t = ref _addr_t.val;

    const nint isRTL = 1 << (int)(bidi.R) | 1 << (int)(bidi.AL) | 1 << (int)(bidi.AN);

    return t.seen & isRTL != 0;
}

// Reset implements transform.Transformer.
private static void Reset(this ptr<Transformer> _addr_t) {
    ref Transformer t = ref _addr_t.val;

    t.val = new Transformer();
}

// Transform implements transform.Transformer. This Transformer has state and
// needs to be reset between uses.
private static (nint, nint, error) Transform(this ptr<Transformer> _addr_t, slice<byte> dst, slice<byte> src, bool atEOF) {
    nint nDst = default;
    nint nSrc = default;
    error err = default!;
    ref Transformer t = ref _addr_t.val;

    if (len(dst) < len(src)) {
        src = src[..(int)len(dst)];
        atEOF = false;
        err = transform.ErrShortDst;
    }
    var (n, err1) = t.Span(src, atEOF);
    copy(dst, src[..(int)n]);
    if (err == null || err1 != null && err1 != transform.ErrShortSrc) {
        err = err1;
    }
    return (n, n, error.As(err)!);

}

// Span returns the first n bytes of src that conform to the Bidi rule.
private static (nint, error) Span(this ptr<Transformer> _addr_t, slice<byte> src, bool atEOF) {
    nint n = default;
    error err = default!;
    ref Transformer t = ref _addr_t.val;

    if (t.state == ruleInvalid && t.isRTL()) {
        return (0, error.As(ErrInvalid)!);
    }
    var (n, ok) = t.advance(src);

    if (!ok) 
        err = ErrInvalid;
    else if (n < len(src)) 
        if (!atEOF) {
            err = transform.ErrShortSrc;
            break;
        }
        err = ErrInvalid;
    else if (!t.isFinal()) 
        err = ErrInvalid;
        return (n, error.As(err)!);

}

// Precomputing the ASCII values decreases running time for the ASCII fast path
// by about 30%.
private static array<bidi.Properties> asciiTable = new array<bidi.Properties>(128);

private static void init() {
    foreach (var (i) in asciiTable) {
        var (p, _) = bidi.LookupRune(rune(i));
        asciiTable[i] = p;
    }
}

private static (nint, bool) advance(this ptr<Transformer> _addr_t, slice<byte> s) {
    nint n = default;
    bool ok = default;
    ref Transformer t = ref _addr_t.val;

    bidi.Properties e = default;
    nint sz = default;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf) {
            (e, sz) = (asciiTable[s[n]], 1);
        }
        else
 {
            e, sz = bidi.Lookup(s[(int)n..]);
            if (sz <= 1) {
                if (sz == 1) { 
                    // We always consider invalid UTF-8 to be invalid, even if
                    // the string has not yet been determined to be RTL.
                    // TODO: is this correct?
                    return (n, false);

                }

                return (n, true); // incomplete UTF-8 encoding
            }

        }
        var c = uint16(1 << (int)(e.Class()));
        t.seen |= c;
        if (t.seen & exclusiveRTL == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state];


            if (tr[0].mask & c != 0) 
                t.state = tr[0].next;
            else if (tr[1].mask & c != 0) 
                t.state = tr[1].next;
            else 
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }

        }
        n += sz;

    }
    return (n, true);

}

private static (nint, bool) advanceString(this ptr<Transformer> _addr_t, @string s) {
    nint n = default;
    bool ok = default;
    ref Transformer t = ref _addr_t.val;

    bidi.Properties e = default;
    nint sz = default;
    while (n < len(s)) {
        if (s[n] < utf8.RuneSelf) {
            (e, sz) = (asciiTable[s[n]], 1);
        }
        else
 {
            e, sz = bidi.LookupString(s[(int)n..]);
            if (sz <= 1) {
                if (sz == 1) {
                    return (n, false); // invalid UTF-8
                }

                return (n, true); // incomplete UTF-8 encoding
            }

        }
        var c = uint16(1 << (int)(e.Class()));
        t.seen |= c;
        if (t.seen & exclusiveRTL == exclusiveRTL) {
            t.state = ruleInvalid;
            return (n, false);
        }
        {
            var tr = transitions[t.state];


            if (tr[0].mask & c != 0) 
                t.state = tr[0].next;
            else if (tr[1].mask & c != 0) 
                t.state = tr[1].next;
            else 
                t.state = ruleInvalid;
                if (t.isRTL()) {
                    return (n, false);
                }

        }
        n += sz;

    }
    return (n, true);

}

} // end bidirule_package
