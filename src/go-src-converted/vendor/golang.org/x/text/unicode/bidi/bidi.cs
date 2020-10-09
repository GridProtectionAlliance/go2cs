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
// package bidi -- go2cs converted at 2020 October 09 06:07:57 UTC
// import "vendor/golang.org/x/text/unicode/bidi" ==> using bidi = go.vendor.golang.org.x.text.unicode.bidi_package
// Original source: C:\Go\src\vendor\golang.org\x\text\unicode\bidi\bidi.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace text {
namespace unicode
{
    public static partial class bidi_package
    { // import "golang.org/x/text/unicode/bidi"

        // TODO:
        // The following functionality would not be hard to implement, but hinges on
        // the definition of a Segmenter interface. For now this is up to the user.
        // - Iterate over paragraphs
        // - Segmenter to iterate over runs directly from a given text.
        // Also:
        // - Transformer for reordering?
        // - Transformer (validator, really) for Bidi Rule.

        // This API tries to avoid dealing with embedding levels for now. Under the hood
        // these will be computed, but the question is to which extent the user should
        // know they exist. We should at some point allow the user to specify an
        // embedding hierarchy, though.

        // A Direction indicates the overall flow of text.
        public partial struct Direction // : long
        {
        }

 
        // LeftToRight indicates the text contains no right-to-left characters and
        // that either there are some left-to-right characters or the option
        // DefaultDirection(LeftToRight) was passed.
        public static readonly Direction LeftToRight = (Direction)iota; 

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


        private partial struct options
        {
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
        public static Option DefaultDirection(Direction d) => func((_, panic, __) =>
        {
            panic("unimplemented");
        });

        // A Paragraph holds a single Paragraph for Bidi processing.
        public partial struct Paragraph
        {
        }

        // SetBytes configures p for the given paragraph text. It replaces text
        // previously set by SetBytes or SetString. If b contains a paragraph separator
        // it will only process the first paragraph and report the number of bytes
        // consumed from b including this separator. Error may be non-nil if options are
        // given.
        private static (long, error) SetBytes(this ptr<Paragraph> _addr_p, slice<byte> b, params Option[] opts) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            opts = opts.Clone();
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // SetString configures p for the given paragraph text. It replaces text
        // previously set by SetBytes or SetString. If b contains a paragraph separator
        // it will only process the first paragraph and report the number of bytes
        // consumed from b including this separator. Error may be non-nil if options are
        // given.
        private static (long, error) SetString(this ptr<Paragraph> _addr_p, @string s, params Option[] opts) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            opts = opts.Clone();
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // IsLeftToRight reports whether the principle direction of rendering for this
        // paragraphs is left-to-right. If this returns false, the principle direction
        // of rendering is right-to-left.
        private static bool IsLeftToRight(this ptr<Paragraph> _addr_p) => func((_, panic, __) =>
        {
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // Direction returns the direction of the text of this paragraph.
        //
        // The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
        private static Direction Direction(this ptr<Paragraph> _addr_p) => func((_, panic, __) =>
        {
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // RunAt reports the Run at the given position of the input text.
        //
        // This method can be used for computing line breaks on paragraphs.
        private static Run RunAt(this ptr<Paragraph> _addr_p, long pos) => func((_, panic, __) =>
        {
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // Order computes the visual ordering of all the runs in a Paragraph.
        private static (Ordering, error) Order(this ptr<Paragraph> _addr_p) => func((_, panic, __) =>
        {
            Ordering _p0 = default;
            error _p0 = default!;
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // Line computes the visual ordering of runs for a single line starting and
        // ending at the given positions in the original text.
        private static (Ordering, error) Line(this ptr<Paragraph> _addr_p, long start, long end) => func((_, panic, __) =>
        {
            Ordering _p0 = default;
            error _p0 = default!;
            ref Paragraph p = ref _addr_p.val;

            panic("unimplemented");
        });

        // An Ordering holds the computed visual order of runs of a Paragraph. Calling
        // SetBytes or SetString on the originating Paragraph invalidates an Ordering.
        // The methods of an Ordering should only be called by one goroutine at a time.
        public partial struct Ordering
        {
        }

        // Direction reports the directionality of the runs.
        //
        // The direction may be LeftToRight, RightToLeft, Mixed, or Neutral.
        private static Direction Direction(this ptr<Ordering> _addr_o) => func((_, panic, __) =>
        {
            ref Ordering o = ref _addr_o.val;

            panic("unimplemented");
        });

        // NumRuns returns the number of runs.
        private static long NumRuns(this ptr<Ordering> _addr_o) => func((_, panic, __) =>
        {
            ref Ordering o = ref _addr_o.val;

            panic("unimplemented");
        });

        // Run returns the ith run within the ordering.
        private static Run Run(this ptr<Ordering> _addr_o, long i) => func((_, panic, __) =>
        {
            ref Ordering o = ref _addr_o.val;

            panic("unimplemented");
        });

        // TODO: perhaps with options.
        // // Reorder creates a reader that reads the runes in visual order per character.
        // // Modifiers remain after the runes they modify.
        // func (l *Runs) Reorder() io.Reader {
        //     panic("unimplemented")
        // }

        // A Run is a continuous sequence of characters of a single direction.
        public partial struct Run
        {
        }

        // String returns the text of the run in its original order.
        private static @string String(this ptr<Run> _addr_r) => func((_, panic, __) =>
        {
            ref Run r = ref _addr_r.val;

            panic("unimplemented");
        });

        // Bytes returns the text of the run in its original order.
        private static slice<byte> Bytes(this ptr<Run> _addr_r) => func((_, panic, __) =>
        {
            ref Run r = ref _addr_r.val;

            panic("unimplemented");
        });

        // TODO: methods for
        // - Display order
        // - headers and footers
        // - bracket replacement.

        // Direction reports the direction of the run.
        private static Direction Direction(this ptr<Run> _addr_r) => func((_, panic, __) =>
        {
            ref Run r = ref _addr_r.val;

            panic("unimplemented");
        });

        // Position of the Run within the text passed to SetBytes or SetString of the
        // originating Paragraph value.
        private static (long, long) Pos(this ptr<Run> _addr_r) => func((_, panic, __) =>
        {
            long start = default;
            long end = default;
            ref Run r = ref _addr_r.val;

            panic("unimplemented");
        });

        // AppendReverse reverses the order of characters of in, appends them to out,
        // and returns the result. Modifiers will still follow the runes they modify.
        // Brackets are replaced with their counterparts.
        public static slice<byte> AppendReverse(slice<byte> @out, slice<byte> @in) => func((_, panic, __) =>
        {
            panic("unimplemented");
        });

        // ReverseString reverses the order of characters in s and returns a new string.
        // Modifiers will still follow the runes they modify. Brackets are replaced with
        // their counterparts.
        public static @string ReverseString(@string s) => func((_, panic, __) =>
        {
            panic("unimplemented");
        });
    }
}}}}}}
