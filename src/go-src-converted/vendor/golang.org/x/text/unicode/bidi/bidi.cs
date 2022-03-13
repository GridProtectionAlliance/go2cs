// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run gen.go gen_trieval.go gen_ranges.go

// Package bidi contains functionality for bidirectional text support.
//
// See https://www.unicode.org/reports/tr9.
//
// NOTE: UNDER CONSTRUCTION. This API may change in backwards incompatible ways
// and without notice.

// package bidi -- go2cs converted at 2022 March 13 06:46:38 UTC
// import "vendor/golang.org/x/text/unicode/bidi" ==> using bidi = go.vendor.golang.org.x.text.unicode.bidi_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\text\unicode\bidi\bidi.go
namespace go.vendor.golang.org.x.text.unicode;
// import "golang.org/x/text/unicode/bidi"

// TODO
// - Transformer for reordering?
// - Transformer (validator, really) for Bidi Rule.


using bytes = bytes_package;


// This API tries to avoid dealing with embedding levels for now. Under the hood
// these will be computed, but the question is to which extent the user should
// know they exist. We should at some point allow the user to specify an
// embedding hierarchy, though.

// A Direction indicates the overall flow of text.

using System;
public static partial class bidi_package {

public partial struct Direction { // : nint
}

 
// LeftToRight indicates the text contains no right-to-left characters and
// that either there are some left-to-right characters or the option
// DefaultDirection(LeftToRight) was passed.
public static readonly Direction LeftToRight = iota; 

// RightToLeft indicates the text contains no left-to-right characters and
// that either there are some right-to-left characters or the option
// DefaultDirection(RightToLeft) was passed.
public static readonly var RightToLeft = 0; 

// Mixed indicates text contains both left-to-right and right-to-left
// characters.
public static readonly var Mixed = 1; 

// Neutral means that text contains no left-to-right and right-to-left
// characters and that no default direction has been set.
public static readonly var Neutral = 2;

private partial struct options {
    public Direction defaultDirection;
}

// An Option is an option for Bidi processing.
public delegate void Option(ptr<options>);

// ICU allows the user to define embedding levels. This may be used, for example,
// to use hierarchical structure of markup languages to define embeddings.
// The following option may be a way to expose this functionality in this API.
// // LevelFunc sets a function that associates nesting levels with the given text.
// // The levels function will be called with monotonically increasing values for p.
// func LevelFunc(levels func(p int) int) Option {
//     panic("unimplemented")
// }

// DefaultDirection sets the default direction for a Paragraph. The direction is
// overridden if the text contains directional characters.
public static Option DefaultDirection(Direction d) {
    return opts => {
        opts.defaultDirection = d;
    };
}

// A Paragraph holds a single Paragraph for Bidi processing.
public partial struct Paragraph {
    public slice<byte> p;
    public Ordering o;
    public slice<Option> opts;
    public slice<Class> types;
    public slice<bracketType> pairTypes;
    public slice<int> pairValues;
    public slice<int> runes;
    public options options;
}

// Initialize the p.pairTypes, p.pairValues and p.types from the input previously
// set by p.SetBytes() or p.SetString(). Also limit the input up to (and including) a paragraph
// separator (bidi class B).
//
// The function p.Order() needs these values to be set, so this preparation could be postponed.
// But since the SetBytes and SetStrings functions return the length of the input up to the paragraph
// separator, the whole input needs to be processed anyway and should not be done twice.
//
// The function has the same return values as SetBytes() / SetString()
private static (nint, error) prepareInput(this ptr<Paragraph> _addr_p) {
    nint n = default;
    error err = default!;
    ref Paragraph p = ref _addr_p.val;

    p.runes = bytes.Runes(p.p);
    nint bytecount = 0; 
    // clear slices from previous SetString or SetBytes
    p.pairTypes = null;
    p.pairValues = null;
    p.types = null;

    foreach (var (_, r) in p.runes) {
        var (props, i) = LookupRune(r);
        bytecount += i;
        var cls = props.Class();
        if (cls == B) {
            return (bytecount, error.As(null!)!);
        }
        p.types = append(p.types, cls);
        if (props.IsOpeningBracket()) {
            p.pairTypes = append(p.pairTypes, bpOpen);
            p.pairValues = append(p.pairValues, r);
        }
        else if (props.IsBracket()) { 
            // this must be a closing bracket,
            // since IsOpeningBracket is not true
            p.pairTypes = append(p.pairTypes, bpClose);
            p.pairValues = append(p.pairValues, r);
        }
        else
 {
            p.pairTypes = append(p.pairTypes, bpNone);
            p.pairValues = append(p.pairValues, 0);
        }
    }    return (bytecount, error.As(null!)!);
}

// SetBytes configures p for the given paragraph text. It replaces text
// previously set by SetBytes or SetString. If b contains a paragraph separator
// it will only process the first paragraph and report the number of bytes
// consumed from b including this separator. Error may be non-nil if options are
// given.
private static (nint, error) SetBytes(this ptr<Paragraph> _addr_p, slice<byte> b, params Option[] opts) {
    nint n = default;
    error err = default!;
    opts = opts.Clone();
    ref Paragraph p = ref _addr_p.val;

    p.p = b;
    p.opts = opts;
    return p.prepareInput();
}

// SetString configures s for the given paragraph text. It replaces text
// previously set by SetBytes or SetString. If s contains a paragraph separator
// it will only process the first paragraph and report the number of bytes
// consumed from s including this separator. Error may be non-nil if options are
// given.
private static (nint, error) SetString(this ptr<Paragraph> _addr_p, @string s, params Option[] opts) {
    nint n = default;
    error err = default!;
    opts = opts.Clone();
    ref Paragraph p = ref _addr_p.val;

    p.p = (slice<byte>)s;
    p.opts = opts;
    return p.prepareInput();
}

// IsLeftToRight reports whether the principle direction of rendering for this
// paragraphs is left-to-right. If this returns false, the principle direction
// of rendering is right-to-left.
private static bool IsLeftToRight(this ptr<Paragraph> _addr_p) {
    ref Paragraph p = ref _addr_p.val;

    return p.Direction() == LeftToRight;
}

// Direction returns the direction of the text of this paragraph.
//
// The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
private static Direction Direction(this ptr<Paragraph> _addr_p) {
    ref Paragraph p = ref _addr_p.val;

    return p.o.Direction();
}

// TODO: what happens if the position is > len(input)? This should return an error.

// RunAt reports the Run at the given position of the input text.
//
// This method can be used for computing line breaks on paragraphs.
private static Run RunAt(this ptr<Paragraph> _addr_p, nint pos) {
    ref Paragraph p = ref _addr_p.val;

    nint c = 0;
    nint runNumber = 0;
    foreach (var (i, r) in p.o.runes) {
        c += len(r);
        if (pos < c) {
            runNumber = i;
        }
    }    return p.o.Run(runNumber);
}

private static Ordering calculateOrdering(slice<level> levels, slice<int> runes) {
    Direction curDir = default;

    var prevDir = Neutral;
    nint prevI = 0;

    Ordering o = new Ordering(); 
    // lvl = 0,2,4,...: left to right
    // lvl = 1,3,5,...: right to left
    foreach (var (i, lvl) in levels) {
        if (lvl % 2 == 0) {
            curDir = LeftToRight;
        }
        else
 {
            curDir = RightToLeft;
        }
        if (curDir != prevDir) {
            if (i > 0) {
                o.runes = append(o.runes, runes[(int)prevI..(int)i]);
                o.directions = append(o.directions, prevDir);
                o.startpos = append(o.startpos, prevI);
            }
            prevI = i;
            prevDir = curDir;
        }
    }    o.runes = append(o.runes, runes[(int)prevI..]);
    o.directions = append(o.directions, prevDir);
    o.startpos = append(o.startpos, prevI);
    return o;
}

// Order computes the visual ordering of all the runs in a Paragraph.
private static (Ordering, error) Order(this ptr<Paragraph> _addr_p) {
    Ordering _p0 = default;
    error _p0 = default!;
    ref Paragraph p = ref _addr_p.val;

    if (len(p.types) == 0) {
        return (new Ordering(), error.As(null!)!);
    }
    foreach (var (_, fn) in p.opts) {
        fn(_addr_p.options);
    }    var lvl = level(-1);
    if (p.options.defaultDirection == RightToLeft) {
        lvl = 1;
    }
    var (para, err) = newParagraph(p.types, p.pairTypes, p.pairValues, lvl);
    if (err != null) {
        return (new Ordering(), error.As(err)!);
    }
    var levels = para.getLevels(new slice<nint>(new nint[] { len(p.types) }));

    p.o = calculateOrdering(levels, p.runes);
    return (p.o, error.As(null!)!);
}

// Line computes the visual ordering of runs for a single line starting and
// ending at the given positions in the original text.
private static (Ordering, error) Line(this ptr<Paragraph> _addr_p, nint start, nint end) {
    Ordering _p0 = default;
    error _p0 = default!;
    ref Paragraph p = ref _addr_p.val;

    var lineTypes = p.types[(int)start..(int)end];
    var (para, err) = newParagraph(lineTypes, p.pairTypes[(int)start..(int)end], p.pairValues[(int)start..(int)end], -1);
    if (err != null) {
        return (new Ordering(), error.As(err)!);
    }
    var levels = para.getLevels(new slice<nint>(new nint[] { len(lineTypes) }));
    var o = calculateOrdering(levels, p.runes[(int)start..(int)end]);
    return (o, error.As(null!)!);
}

// An Ordering holds the computed visual order of runs of a Paragraph. Calling
// SetBytes or SetString on the originating Paragraph invalidates an Ordering.
// The methods of an Ordering should only be called by one goroutine at a time.
public partial struct Ordering {
    public slice<slice<int>> runes;
    public slice<Direction> directions;
    public slice<nint> startpos;
}

// Direction reports the directionality of the runs.
//
// The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
private static Direction Direction(this ptr<Ordering> _addr_o) {
    ref Ordering o = ref _addr_o.val;

    return o.directions[0];
}

// NumRuns returns the number of runs.
private static nint NumRuns(this ptr<Ordering> _addr_o) {
    ref Ordering o = ref _addr_o.val;

    return len(o.runes);
}

// Run returns the ith run within the ordering.
private static Run Run(this ptr<Ordering> _addr_o, nint i) {
    ref Ordering o = ref _addr_o.val;

    Run r = new Run(runes:o.runes[i],direction:o.directions[i],startpos:o.startpos[i],);
    return r;
}

// TODO: perhaps with options.
// // Reorder creates a reader that reads the runes in visual order per character.
// // Modifiers remain after the runes they modify.
// func (l *Runs) Reorder() io.Reader {
//     panic("unimplemented")
// }

// A Run is a continuous sequence of characters of a single direction.
public partial struct Run {
    public slice<int> runes;
    public Direction direction;
    public nint startpos;
}

// String returns the text of the run in its original order.
private static @string String(this ptr<Run> _addr_r) {
    ref Run r = ref _addr_r.val;

    return string(r.runes);
}

// Bytes returns the text of the run in its original order.
private static slice<byte> Bytes(this ptr<Run> _addr_r) {
    ref Run r = ref _addr_r.val;

    return (slice<byte>)r.String();
}

// TODO: methods for
// - Display order
// - headers and footers
// - bracket replacement.

// Direction reports the direction of the run.
private static Direction Direction(this ptr<Run> _addr_r) {
    ref Run r = ref _addr_r.val;

    return r.direction;
}

// Pos returns the position of the Run within the text passed to SetBytes or SetString of the
// originating Paragraph value.
private static (nint, nint) Pos(this ptr<Run> _addr_r) {
    nint start = default;
    nint end = default;
    ref Run r = ref _addr_r.val;

    return (r.startpos, r.startpos + len(r.runes) - 1);
}

// AppendReverse reverses the order of characters of in, appends them to out,
// and returns the result. Modifiers will still follow the runes they modify.
// Brackets are replaced with their counterparts.
public static slice<byte> AppendReverse(slice<byte> @out, slice<byte> @in) {
    var ret = make_slice<byte>(len(in) + len(out));
    copy(ret, out);
    var inRunes = bytes.Runes(in);

    {
        var i__prev1 = i;

        foreach (var (__i, __r) in inRunes) {
            i = __i;
            r = __r;
            var (prop, _) = LookupRune(r);
            if (prop.IsBracket()) {
                inRunes[i] = prop.reverseBracket(r);
            }
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        nint i = 0;
        var j = len(inRunes) - 1;

        while (i < j) {
            (inRunes[i], inRunes[j]) = (inRunes[j], inRunes[i]);            (i, j) = (i + 1, j - 1);
        }

        i = i__prev1;
    }
    copy(ret[(int)len(out)..], string(inRunes));

    return ret;
}

// ReverseString reverses the order of characters in s and returns a new string.
// Modifiers will still follow the runes they modify. Brackets are replaced with
// their counterparts.
public static @string ReverseString(@string s) {
    slice<int> input = (slice<int>)s;
    var li = len(input);
    var ret = make_slice<int>(li);
    foreach (var (i, r) in input) {
        var (prop, _) = LookupRune(r);
        if (prop.IsBracket()) {
            ret[li - i - 1] = prop.reverseBracket(r);
        }
        else
 {
            ret[li - i - 1] = r;
        }
    }    return string(ret);
}

} // end bidi_package
