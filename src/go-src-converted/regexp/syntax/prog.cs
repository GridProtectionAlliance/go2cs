// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 08 03:41:05 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\prog.go
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        // Compiled program.
        // May not belong in this package, but convenient for now.

        // A Prog is a compiled regular expression program.
        public partial struct Prog
        {
            public slice<Inst> Inst;
            public long Start; // index of start instruction
            public long NumCap; // number of InstCapture insts in re
        }

        // An InstOp is an instruction opcode.
        public partial struct InstOp // : byte
        {
        }

        public static readonly InstOp InstAlt = (InstOp)iota;
        public static readonly var InstAltMatch = (var)0;
        public static readonly var InstCapture = (var)1;
        public static readonly var InstEmptyWidth = (var)2;
        public static readonly var InstMatch = (var)3;
        public static readonly var InstFail = (var)4;
        public static readonly var InstNop = (var)5;
        public static readonly var InstRune = (var)6;
        public static readonly var InstRune1 = (var)7;
        public static readonly var InstRuneAny = (var)8;
        public static readonly var InstRuneAnyNotNL = (var)9;


        private static @string instOpNames = new slice<@string>(new @string[] { "InstAlt", "InstAltMatch", "InstCapture", "InstEmptyWidth", "InstMatch", "InstFail", "InstNop", "InstRune", "InstRune1", "InstRuneAny", "InstRuneAnyNotNL" });

        public static @string String(this InstOp i)
        {
            if (uint(i) >= uint(len(instOpNames)))
            {
                return "";
            }

            return instOpNames[i];

        }

        // An EmptyOp specifies a kind or mixture of zero-width assertions.
        public partial struct EmptyOp // : byte
        {
        }

        public static readonly EmptyOp EmptyBeginLine = (EmptyOp)1L << (int)(iota);
        public static readonly var EmptyEndLine = (var)0;
        public static readonly var EmptyBeginText = (var)1;
        public static readonly var EmptyEndText = (var)2;
        public static readonly var EmptyWordBoundary = (var)3;
        public static readonly var EmptyNoWordBoundary = (var)4;


        // EmptyOpContext returns the zero-width assertions
        // satisfied at the position between the runes r1 and r2.
        // Passing r1 == -1 indicates that the position is
        // at the beginning of the text.
        // Passing r2 == -1 indicates that the position is
        // at the end of the text.
        public static EmptyOp EmptyOpContext(int r1, int r2)
        {
            EmptyOp op = EmptyNoWordBoundary;
            byte boundary = default;

            if (IsWordChar(r1)) 
                boundary = 1L;
            else if (r1 == '\n') 
                op |= EmptyBeginLine;
            else if (r1 < 0L) 
                op |= EmptyBeginText | EmptyBeginLine;
            
            if (IsWordChar(r2)) 
                boundary ^= 1L;
            else if (r2 == '\n') 
                op |= EmptyEndLine;
            else if (r2 < 0L) 
                op |= EmptyEndText | EmptyEndLine;
                        if (boundary != 0L)
            { // IsWordChar(r1) != IsWordChar(r2)
                op ^= (EmptyWordBoundary | EmptyNoWordBoundary);

            }

            return op;

        }

        // IsWordChar reports whether r is consider a ``word character''
        // during the evaluation of the \b and \B zero-width assertions.
        // These assertions are ASCII-only: the word characters are [A-Za-z0-9_].
        public static bool IsWordChar(int r)
        {
            return 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z' || '0' <= r && r <= '9' || r == '_';
        }

        // An Inst is a single instruction in a regular expression program.
        public partial struct Inst
        {
            public InstOp Op;
            public uint Out; // all but InstMatch, InstFail
            public uint Arg; // InstAlt, InstAltMatch, InstCapture, InstEmptyWidth
            public slice<int> Rune;
        }

        private static @string String(this ptr<Prog> _addr_p)
        {
            ref Prog p = ref _addr_p.val;

            ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
            dumpProg(_addr_b, _addr_p);
            return b.String();
        }

        // skipNop follows any no-op or capturing instructions.
        private static ptr<Inst> skipNop(this ptr<Prog> _addr_p, uint pc)
        {
            ref Prog p = ref _addr_p.val;

            var i = _addr_p.Inst[pc];
            while (i.Op == InstNop || i.Op == InstCapture)
            {
                i = _addr_p.Inst[i.Out];
            }

            return _addr_i!;

        }

        // op returns i.Op but merges all the Rune special cases into InstRune
        private static InstOp op(this ptr<Inst> _addr_i)
        {
            ref Inst i = ref _addr_i.val;

            var op = i.Op;

            if (op == InstRune1 || op == InstRuneAny || op == InstRuneAnyNotNL) 
                op = InstRune;
                        return op;

        }

        // Prefix returns a literal string that all matches for the
        // regexp must start with. Complete is true if the prefix
        // is the entire match.
        private static (@string, bool) Prefix(this ptr<Prog> _addr_p)
        {
            @string prefix = default;
            bool complete = default;
            ref Prog p = ref _addr_p.val;

            var i = p.skipNop(uint32(p.Start)); 

            // Avoid allocation of buffer if prefix is empty.
            if (i.op() != InstRune || len(i.Rune) != 1L)
            {
                return ("", i.Op == InstMatch);
            } 

            // Have prefix; gather characters.
            strings.Builder buf = default;
            while (i.op() == InstRune && len(i.Rune) == 1L && Flags(i.Arg) & FoldCase == 0L)
            {
                buf.WriteRune(i.Rune[0L]);
                i = p.skipNop(i.Out);
            }

            return (buf.String(), i.Op == InstMatch);

        }

        // StartCond returns the leading empty-width conditions that must
        // be true in any match. It returns ^EmptyOp(0) if no matches are possible.
        private static EmptyOp StartCond(this ptr<Prog> _addr_p)
        {
            ref Prog p = ref _addr_p.val;

            EmptyOp flag = default;
            var pc = uint32(p.Start);
            var i = _addr_p.Inst[pc];
Loop:
            while (true)
            {

                if (i.Op == InstEmptyWidth) 
                    flag |= EmptyOp(i.Arg);
                else if (i.Op == InstFail) 
                    return ~EmptyOp(0L);
                else if (i.Op == InstCapture || i.Op == InstNop)                 else 
                    _breakLoop = true;
                    break;
                                pc = i.Out;
                i = _addr_p.Inst[pc];

            }
            return flag;

        }

        private static readonly long noMatch = (long)-1L;

        // MatchRune reports whether the instruction matches (and consumes) r.
        // It should only be called when i.Op == InstRune.


        // MatchRune reports whether the instruction matches (and consumes) r.
        // It should only be called when i.Op == InstRune.
        private static bool MatchRune(this ptr<Inst> _addr_i, int r)
        {
            ref Inst i = ref _addr_i.val;

            return i.MatchRunePos(r) != noMatch;
        }

        // MatchRunePos checks whether the instruction matches (and consumes) r.
        // If so, MatchRunePos returns the index of the matching rune pair
        // (or, when len(i.Rune) == 1, rune singleton).
        // If not, MatchRunePos returns -1.
        // MatchRunePos should only be called when i.Op == InstRune.
        private static long MatchRunePos(this ptr<Inst> _addr_i, int r)
        {
            ref Inst i = ref _addr_i.val;

            var rune = i.Rune;

            switch (len(rune))
            {
                case 0L: 
                    return noMatch;
                    break;
                case 1L: 
                    // Special case: single-rune slice is from literal string, not char class.
                    var r0 = rune[0L];
                    if (r == r0)
                    {
                        return 0L;
                    }

                    if (Flags(i.Arg) & FoldCase != 0L)
                    {
                        {
                            var r1 = unicode.SimpleFold(r0);

                            while (r1 != r0)
                            {
                                if (r == r1)
                                {
                                    return 0L;
                                r1 = unicode.SimpleFold(r1);
                                }

                            }

                        }

                    }

                    return noMatch;
                    break;
                case 2L: 
                    if (r >= rune[0L] && r <= rune[1L])
                    {
                        return 0L;
                    }

                    return noMatch;
                    break;
                case 4L: 
                    // Linear search for a few pairs.
                    // Should handle ASCII well.

                case 6L: 
                    // Linear search for a few pairs.
                    // Should handle ASCII well.

                case 8L: 
                    // Linear search for a few pairs.
                    // Should handle ASCII well.
                    {
                        long j = 0L;

                        while (j < len(rune))
                        {
                            if (r < rune[j])
                            {
                                return noMatch;
                            j += 2L;
                            }

                            if (r <= rune[j + 1L])
                            {
                                return j / 2L;
                            }

                        }

                    }
                    return noMatch;
                    break;
            } 

            // Otherwise binary search.
            long lo = 0L;
            var hi = len(rune) / 2L;
            while (lo < hi)
            {
                var m = lo + (hi - lo) / 2L;
                {
                    var c = rune[2L * m];

                    if (c <= r)
                    {
                        if (r <= rune[2L * m + 1L])
                        {
                            return m;
                        }

                        lo = m + 1L;

                    }
                    else
                    {
                        hi = m;
                    }

                }

            }

            return noMatch;

        }

        // MatchEmptyWidth reports whether the instruction matches
        // an empty string between the runes before and after.
        // It should only be called when i.Op == InstEmptyWidth.
        private static bool MatchEmptyWidth(this ptr<Inst> _addr_i, int before, int after) => func((_, panic, __) =>
        {
            ref Inst i = ref _addr_i.val;


            if (EmptyOp(i.Arg) == EmptyBeginLine) 
                return before == '\n' || before == -1L;
            else if (EmptyOp(i.Arg) == EmptyEndLine) 
                return after == '\n' || after == -1L;
            else if (EmptyOp(i.Arg) == EmptyBeginText) 
                return before == -1L;
            else if (EmptyOp(i.Arg) == EmptyEndText) 
                return after == -1L;
            else if (EmptyOp(i.Arg) == EmptyWordBoundary) 
                return IsWordChar(before) != IsWordChar(after);
            else if (EmptyOp(i.Arg) == EmptyNoWordBoundary) 
                return IsWordChar(before) == IsWordChar(after);
                        panic("unknown empty width arg");

        });

        private static @string String(this ptr<Inst> _addr_i)
        {
            ref Inst i = ref _addr_i.val;

            ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
            dumpInst(_addr_b, _addr_i);
            return b.String();
        }

        private static void bw(ptr<strings.Builder> _addr_b, params @string[] args)
        {
            args = args.Clone();
            ref strings.Builder b = ref _addr_b.val;

            foreach (var (_, s) in args)
            {
                b.WriteString(s);
            }

        }

        private static void dumpProg(ptr<strings.Builder> _addr_b, ptr<Prog> _addr_p)
        {
            ref strings.Builder b = ref _addr_b.val;
            ref Prog p = ref _addr_p.val;

            foreach (var (j) in p.Inst)
            {
                var i = _addr_p.Inst[j];
                var pc = strconv.Itoa(j);
                if (len(pc) < 3L)
                {
                    b.WriteString("   "[len(pc)..]);
                }

                if (j == p.Start)
                {
                    pc += "*";
                }

                bw(_addr_b, pc, "\t");
                dumpInst(_addr_b, _addr_i);
                bw(_addr_b, "\n");

            }

        }

        private static @string u32(uint i)
        {
            return strconv.FormatUint(uint64(i), 10L);
        }

        private static void dumpInst(ptr<strings.Builder> _addr_b, ptr<Inst> _addr_i)
        {
            ref strings.Builder b = ref _addr_b.val;
            ref Inst i = ref _addr_i.val;


            if (i.Op == InstAlt) 
                bw(_addr_b, "alt -> ", u32(i.Out), ", ", u32(i.Arg));
            else if (i.Op == InstAltMatch) 
                bw(_addr_b, "altmatch -> ", u32(i.Out), ", ", u32(i.Arg));
            else if (i.Op == InstCapture) 
                bw(_addr_b, "cap ", u32(i.Arg), " -> ", u32(i.Out));
            else if (i.Op == InstEmptyWidth) 
                bw(_addr_b, "empty ", u32(i.Arg), " -> ", u32(i.Out));
            else if (i.Op == InstMatch) 
                bw(_addr_b, "match");
            else if (i.Op == InstFail) 
                bw(_addr_b, "fail");
            else if (i.Op == InstNop) 
                bw(_addr_b, "nop -> ", u32(i.Out));
            else if (i.Op == InstRune) 
                if (i.Rune == null)
                { 
                    // shouldn't happen
                    bw(_addr_b, "rune <nil>");

                }

                bw(_addr_b, "rune ", strconv.QuoteToASCII(string(i.Rune)));
                if (Flags(i.Arg) & FoldCase != 0L)
                {
                    bw(_addr_b, "/i");
                }

                bw(_addr_b, " -> ", u32(i.Out));
            else if (i.Op == InstRune1) 
                bw(_addr_b, "rune1 ", strconv.QuoteToASCII(string(i.Rune)), " -> ", u32(i.Out));
            else if (i.Op == InstRuneAny) 
                bw(_addr_b, "any -> ", u32(i.Out));
            else if (i.Op == InstRuneAnyNotNL) 
                bw(_addr_b, "anynotnl -> ", u32(i.Out));
            
        }
    }
}}
