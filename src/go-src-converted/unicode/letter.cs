// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unicode provides data and functions to test some properties of
// Unicode code points.
// package unicode -- go2cs converted at 2020 October 09 04:49:32 UTC
// import "unicode" ==> using unicode = go.unicode_package
// Original source: C:\Go\src\unicode\letter.go

using static go.builtin;

namespace go
{
    public static partial class unicode_package
    {
        public static readonly char MaxRune = (char)'\U0010FFFF'; // Maximum valid Unicode code point.
        public static readonly char ReplacementChar = (char)'\uFFFD'; // Represents invalid code points.
        public static readonly char MaxASCII = (char)'\u007F'; // maximum ASCII value.
        public static readonly char MaxLatin1 = (char)'\u00FF'; // maximum Latin-1 value.

        // RangeTable defines a set of Unicode code points by listing the ranges of
        // code points within the set. The ranges are listed in two slices
        // to save space: a slice of 16-bit ranges and a slice of 32-bit ranges.
        // The two slices must be in sorted order and non-overlapping.
        // Also, R32 should contain only values >= 0x10000 (1<<16).
        public partial struct RangeTable
        {
            public slice<Range16> R16;
            public slice<Range32> R32;
            public long LatinOffset; // number of entries in R16 with Hi <= MaxLatin1
        }

        // Range16 represents of a range of 16-bit Unicode code points. The range runs from Lo to Hi
        // inclusive and has the specified stride.
        public partial struct Range16
        {
            public ushort Lo;
            public ushort Hi;
            public ushort Stride;
        }

        // Range32 represents of a range of Unicode code points and is used when one or
        // more of the values will not fit in 16 bits. The range runs from Lo to Hi
        // inclusive and has the specified stride. Lo and Hi must always be >= 1<<16.
        public partial struct Range32
        {
            public uint Lo;
            public uint Hi;
            public uint Stride;
        }

        // CaseRange represents a range of Unicode code points for simple (one
        // code point to one code point) case conversion.
        // The range runs from Lo to Hi inclusive, with a fixed stride of 1. Deltas
        // are the number to add to the code point to reach the code point for a
        // different case for that character. They may be negative. If zero, it
        // means the character is in the corresponding case. There is a special
        // case representing sequences of alternating corresponding Upper and Lower
        // pairs. It appears with a fixed Delta of
        //    {UpperLower, UpperLower, UpperLower}
        // The constant UpperLower has an otherwise impossible delta value.
        public partial struct CaseRange
        {
            public uint Lo;
            public uint Hi;
            public d Delta;
        }

        // SpecialCase represents language-specific case mappings such as Turkish.
        // Methods of SpecialCase customize (by overriding) the standard mappings.
        public partial struct SpecialCase // : slice<CaseRange>
        {
        }

        // BUG(r): There is no mechanism for full case folding, that is, for
        // characters that involve multiple runes in the input or output.

        // Indices into the Delta arrays inside CaseRanges for case mapping.
        public static readonly var UpperCase = iota;
        public static readonly var LowerCase = 0;
        public static readonly var TitleCase = 1;
        public static readonly var MaxCase = 2;


        private partial struct d // : array<int>
        {
        } // to make the CaseRanges text shorter

        // If the Delta field of a CaseRange is UpperLower, it means
        // this CaseRange represents a sequence of the form (say)
        // Upper Lower Upper Lower.
        public static readonly var UpperLower = MaxRune + 1L; // (Cannot be a valid delta.)

        // linearMax is the maximum size table for linear search for non-Latin1 rune.
        // Derived by running 'go test -calibrate'.
        private static readonly long linearMax = (long)18L;

        // is16 reports whether r is in the sorted slice of 16-bit ranges.


        // is16 reports whether r is in the sorted slice of 16-bit ranges.
        private static bool is16(slice<Range16> ranges, ushort r)
        {
            if (len(ranges) <= linearMax || r <= MaxLatin1)
            {
                foreach (var (i) in ranges)
                {
                    var range_ = _addr_ranges[i];
                    if (r < range_.Lo)
                    {
                        return false;
                    }

                    if (r <= range_.Hi)
                    {
                        return range_.Stride == 1L || (r - range_.Lo) % range_.Stride == 0L;
                    }

                }
                return false;

            } 

            // binary search over ranges
            long lo = 0L;
            var hi = len(ranges);
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                range_ = _addr_ranges[m];
                if (range_.Lo <= r && r <= range_.Hi)
                {
                    return range_.Stride == 1L || (r - range_.Lo) % range_.Stride == 0L;
                }

                if (r < range_.Lo)
                {
                    hi = m;
                }
                else
                {
                    lo = m + 1L;
                }

            }

            return false;

        }

        // is32 reports whether r is in the sorted slice of 32-bit ranges.
        private static bool is32(slice<Range32> ranges, uint r)
        {
            if (len(ranges) <= linearMax)
            {
                foreach (var (i) in ranges)
                {
                    var range_ = _addr_ranges[i];
                    if (r < range_.Lo)
                    {
                        return false;
                    }

                    if (r <= range_.Hi)
                    {
                        return range_.Stride == 1L || (r - range_.Lo) % range_.Stride == 0L;
                    }

                }
                return false;

            } 

            // binary search over ranges
            long lo = 0L;
            var hi = len(ranges);
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                range_ = ranges[m];
                if (range_.Lo <= r && r <= range_.Hi)
                {
                    return range_.Stride == 1L || (r - range_.Lo) % range_.Stride == 0L;
                }

                if (r < range_.Lo)
                {
                    hi = m;
                }
                else
                {
                    lo = m + 1L;
                }

            }

            return false;

        }

        // Is reports whether the rune is in the specified table of ranges.
        public static bool Is(ptr<RangeTable> _addr_rangeTab, int r)
        {
            ref RangeTable rangeTab = ref _addr_rangeTab.val;

            var r16 = rangeTab.R16;
            if (len(r16) > 0L && r <= rune(r16[len(r16) - 1L].Hi))
            {
                return is16(r16, uint16(r));
            }

            var r32 = rangeTab.R32;
            if (len(r32) > 0L && r >= rune(r32[0L].Lo))
            {
                return is32(r32, uint32(r));
            }

            return false;

        }

        private static bool isExcludingLatin(ptr<RangeTable> _addr_rangeTab, int r)
        {
            ref RangeTable rangeTab = ref _addr_rangeTab.val;

            var r16 = rangeTab.R16;
            {
                var off = rangeTab.LatinOffset;

                if (len(r16) > off && r <= rune(r16[len(r16) - 1L].Hi))
                {
                    return is16(r16[off..], uint16(r));
                }

            }

            var r32 = rangeTab.R32;
            if (len(r32) > 0L && r >= rune(r32[0L].Lo))
            {
                return is32(r32, uint32(r));
            }

            return false;

        }

        // IsUpper reports whether the rune is an upper case letter.
        public static bool IsUpper(int r)
        { 
            // See comment in IsGraphic.
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pLmask == pLu;
            }

            return isExcludingLatin(_addr_Upper, r);

        }

        // IsLower reports whether the rune is a lower case letter.
        public static bool IsLower(int r)
        { 
            // See comment in IsGraphic.
            if (uint32(r) <= MaxLatin1)
            {
                return properties[uint8(r)] & pLmask == pLl;
            }

            return isExcludingLatin(_addr_Lower, r);

        }

        // IsTitle reports whether the rune is a title case letter.
        public static bool IsTitle(int r)
        {
            if (r <= MaxLatin1)
            {
                return false;
            }

            return isExcludingLatin(_addr_Title, r);

        }

        // to maps the rune using the specified case mapping.
        // It additionally reports whether caseRange contained a mapping for r.
        private static (int, bool) to(long _case, int r, slice<CaseRange> caseRange)
        {
            int mappedRune = default;
            bool foundMapping = default;

            if (_case < 0L || MaxCase <= _case)
            {
                return (ReplacementChar, false); // as reasonable an error as any
            } 
            // binary search over ranges
            long lo = 0L;
            var hi = len(caseRange);
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                var cr = caseRange[m];
                if (rune(cr.Lo) <= r && r <= rune(cr.Hi))
                {
                    var delta = cr.Delta[_case];
                    if (delta > MaxRune)
                    { 
                        // In an Upper-Lower sequence, which always starts with
                        // an UpperCase letter, the real deltas always look like:
                        //    {0, 1, 0}    UpperCase (Lower is next)
                        //    {-1, 0, -1}  LowerCase (Upper, Title are previous)
                        // The characters at even offsets from the beginning of the
                        // sequence are upper case; the ones at odd offsets are lower.
                        // The correct mapping can be done by clearing or setting the low
                        // bit in the sequence offset.
                        // The constants UpperCase and TitleCase are even while LowerCase
                        // is odd so we take the low bit from _case.
                        return (rune(cr.Lo) + ((r - rune(cr.Lo)) & ~1L | rune(_case & 1L)), true);

                    }

                    return (r + delta, true);

                }

                if (r < rune(cr.Lo))
                {
                    hi = m;
                }
                else
                {
                    lo = m + 1L;
                }

            }

            return (r, false);

        }

        // To maps the rune to the specified case: UpperCase, LowerCase, or TitleCase.
        public static int To(long _case, int r)
        {
            r, _ = to(_case, r, CaseRanges);
            return r;
        }

        // ToUpper maps the rune to upper case.
        public static int ToUpper(int r)
        {
            if (r <= MaxASCII)
            {
                if ('a' <= r && r <= 'z')
                {
                    r -= 'a' - 'A';
                }

                return r;

            }

            return To(UpperCase, r);

        }

        // ToLower maps the rune to lower case.
        public static int ToLower(int r)
        {
            if (r <= MaxASCII)
            {
                if ('A' <= r && r <= 'Z')
                {
                    r += 'a' - 'A';
                }

                return r;

            }

            return To(LowerCase, r);

        }

        // ToTitle maps the rune to title case.
        public static int ToTitle(int r)
        {
            if (r <= MaxASCII)
            {
                if ('a' <= r && r <= 'z')
                { // title case is upper case for ASCII
                    r -= 'a' - 'A';

                }

                return r;

            }

            return To(TitleCase, r);

        }

        // ToUpper maps the rune to upper case giving priority to the special mapping.
        public static int ToUpper(this SpecialCase special, int r)
        {
            var (r1, hadMapping) = to(UpperCase, r, (slice<CaseRange>)special);
            if (r1 == r && !hadMapping)
            {
                r1 = ToUpper(r);
            }

            return r1;

        }

        // ToTitle maps the rune to title case giving priority to the special mapping.
        public static int ToTitle(this SpecialCase special, int r)
        {
            var (r1, hadMapping) = to(TitleCase, r, (slice<CaseRange>)special);
            if (r1 == r && !hadMapping)
            {
                r1 = ToTitle(r);
            }

            return r1;

        }

        // ToLower maps the rune to lower case giving priority to the special mapping.
        public static int ToLower(this SpecialCase special, int r)
        {
            var (r1, hadMapping) = to(LowerCase, r, (slice<CaseRange>)special);
            if (r1 == r && !hadMapping)
            {
                r1 = ToLower(r);
            }

            return r1;

        }

        // caseOrbit is defined in tables.go as []foldPair. Right now all the
        // entries fit in uint16, so use uint16. If that changes, compilation
        // will fail (the constants in the composite literal will not fit in uint16)
        // and the types here can change to uint32.
        private partial struct foldPair
        {
            public ushort From;
            public ushort To;
        }

        // SimpleFold iterates over Unicode code points equivalent under
        // the Unicode-defined simple case folding. Among the code points
        // equivalent to rune (including rune itself), SimpleFold returns the
        // smallest rune > r if one exists, or else the smallest rune >= 0.
        // If r is not a valid Unicode code point, SimpleFold(r) returns r.
        //
        // For example:
        //    SimpleFold('A') = 'a'
        //    SimpleFold('a') = 'A'
        //
        //    SimpleFold('K') = 'k'
        //    SimpleFold('k') = '\u212A' (Kelvin symbol, K)
        //    SimpleFold('\u212A') = 'K'
        //
        //    SimpleFold('1') = '1'
        //
        //    SimpleFold(-2) = -2
        //
        public static int SimpleFold(int r)
        {
            if (r < 0L || r > MaxRune)
            {
                return r;
            }

            if (int(r) < len(asciiFold))
            {
                return rune(asciiFold[r]);
            } 

            // Consult caseOrbit table for special cases.
            long lo = 0L;
            var hi = len(caseOrbit);
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                if (rune(caseOrbit[m].From) < r)
                {
                    lo = m + 1L;
                }
                else
                {
                    hi = m;
                }

            }

            if (lo < len(caseOrbit) && rune(caseOrbit[lo].From) == r)
            {
                return rune(caseOrbit[lo].To);
            } 

            // No folding specified. This is a one- or two-element
            // equivalence class containing rune and ToLower(rune)
            // and ToUpper(rune) if they are different from rune.
            {
                var l = ToLower(r);

                if (l != r)
                {
                    return l;
                }

            }

            return ToUpper(r);

        }
    }
}
