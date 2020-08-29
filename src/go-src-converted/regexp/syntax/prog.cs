// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 08:23:57 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\prog.go
using bytes = go.bytes_package;
using strconv = go.strconv_package;
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

        public static readonly InstOp InstAlt = iota;
        public static readonly var InstAltMatch = 0;
        public static readonly var InstCapture = 1;
        public static readonly var InstEmptyWidth = 2;
        public static readonly var InstMatch = 3;
        public static readonly var InstFail = 4;
        public static readonly var InstNop = 5;
        public static readonly var InstRune = 6;
        public static readonly var InstRune1 = 7;
        public static readonly var InstRuneAny = 8;
        public static readonly var InstRuneAnyNotNL = 9;

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

        public static readonly EmptyOp EmptyBeginLine = 1L << (int)(iota);
        public static readonly var EmptyEndLine = 0;
        public static readonly var EmptyBeginText = 1;
        public static readonly var EmptyEndText = 2;
        public static readonly var EmptyWordBoundary = 3;
        public static readonly var EmptyNoWordBoundary = 4;

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

        private static @string String(this ref Prog p)
        {
            bytes.Buffer b = default;
            dumpProg(ref b, p);
            return b.String();
        }

        // skipNop follows any no-op or capturing instructions
        // and returns the resulting pc.
        private static (ref Inst, uint) skipNop(this ref Prog p, uint pc)
        {
            var i = ref p.Inst[pc];
            while (i.Op == InstNop || i.Op == InstCapture)
            {
                pc = i.Out;
                i = ref p.Inst[pc];
            }

            return (i, pc);
        }

        // op returns i.Op but merges all the Rune special cases into InstRune
        private static InstOp op(this ref Inst i)
        {
            var op = i.Op;

            if (op == InstRune1 || op == InstRuneAny || op == InstRuneAnyNotNL) 
                op = InstRune;
                        return op;
        }

        // Prefix returns a literal string that all matches for the
        // regexp must start with. Complete is true if the prefix
        // is the entire match.
        private static (@string, bool) Prefix(this ref Prog p)
        {
            var (i, _) = p.skipNop(uint32(p.Start)); 

            // Avoid allocation of buffer if prefix is empty.
            if (i.op() != InstRune || len(i.Rune) != 1L)
            {
                return ("", i.Op == InstMatch);
            } 

            // Have prefix; gather characters.
            bytes.Buffer buf = default;
            while (i.op() == InstRune && len(i.Rune) == 1L && Flags(i.Arg) & FoldCase == 0L)
            {
                buf.WriteRune(i.Rune[0L]);
                i, _ = p.skipNop(i.Out);
            }

            return (buf.String(), i.Op == InstMatch);
        }

        // StartCond returns the leading empty-width conditions that must
        // be true in any match. It returns ^EmptyOp(0) if no matches are possible.
        private static EmptyOp StartCond(this ref Prog p)
        {
            EmptyOp flag = default;
            var pc = uint32(p.Start);
            var i = ref p.Inst[pc];
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
                i = ref p.Inst[pc];
            }
            return flag;
        }

        private static readonly long noMatch = -1L;

        // MatchRune reports whether the instruction matches (and consumes) r.
        // It should only be called when i.Op == InstRune.


        // MatchRune reports whether the instruction matches (and consumes) r.
        // It should only be called when i.Op == InstRune.
        private static bool MatchRune(this ref Inst i, int r)
        {
            return i.MatchRunePos(r) != noMatch;
        }

        // MatchRunePos checks whether the instruction matches (and consumes) r.
        // If so, MatchRunePos returns the index of the matching rune pair
        // (or, when len(i.Rune) == 1, rune singleton).
        // If not, MatchRunePos returns -1.
        // MatchRunePos should only be called when i.Op == InstRune.
        private static long MatchRunePos(this ref Inst i, int r)
        {
            var rune = i.Rune; 

            // Special case: single-rune slice is from literal string, not char class.
            if (len(rune) == 1L)
            {
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
            } 

            // Peek at the first few pairs.
            // Should handle ASCII well.
            {
                long j = 0L;

                while (j < len(rune) && j <= 8L)
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

                // Otherwise binary search.

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
        private static bool MatchEmptyWidth(this ref Inst _i, int before, int after) => func(_i, (ref Inst i, Defer _, Panic panic, Recover __) =>
        {

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

        private static @string String(this ref Inst i)
        {
            bytes.Buffer b = default;
            dumpInst(ref b, i);
            return b.String();
        }

        private static void bw(ref bytes.Buffer b, params @string[] args)
        {
            args = args.Clone();

            foreach (var (_, s) in args)
            {
                b.WriteString(s);
            }
        }

        private static void dumpProg(ref bytes.Buffer b, ref Prog p)
        {
            foreach (var (j) in p.Inst)
            {
                var i = ref p.Inst[j];
                var pc = strconv.Itoa(j);
                if (len(pc) < 3L)
                {
                    b.WriteString("   "[len(pc)..]);
                }
                if (j == p.Start)
                {
                    pc += "*";
                }
                bw(b, pc, "\t");
                dumpInst(b, i);
                bw(b, "\n");
            }
        }

        private static @string u32(uint i)
        {
            return strconv.FormatUint(uint64(i), 10L);
        }

        private static void dumpInst(ref bytes.Buffer b, ref Inst i)
        {

            if (i.Op == InstAlt) 
                bw(b, "alt -> ", u32(i.Out), ", ", u32(i.Arg));
            else if (i.Op == InstAltMatch) 
                bw(b, "altmatch -> ", u32(i.Out), ", ", u32(i.Arg));
            else if (i.Op == InstCapture) 
                bw(b, "cap ", u32(i.Arg), " -> ", u32(i.Out));
            else if (i.Op == InstEmptyWidth) 
                bw(b, "empty ", u32(i.Arg), " -> ", u32(i.Out));
            else if (i.Op == InstMatch) 
                bw(b, "match");
            else if (i.Op == InstFail) 
                bw(b, "fail");
            else if (i.Op == InstNop) 
                bw(b, "nop -> ", u32(i.Out));
            else if (i.Op == InstRune) 
                if (i.Rune == null)
                { 
                    // shouldn't happen
                    bw(b, "rune <nil>");
                }
                bw(b, "rune ", strconv.QuoteToASCII(string(i.Rune)));
                if (Flags(i.Arg) & FoldCase != 0L)
                {
                    bw(b, "/i");
                }
                bw(b, " -> ", u32(i.Out));
            else if (i.Op == InstRune1) 
                bw(b, "rune1 ", strconv.QuoteToASCII(string(i.Rune)), " -> ", u32(i.Out));
            else if (i.Op == InstRuneAny) 
                bw(b, "any -> ", u32(i.Out));
            else if (i.Op == InstRuneAnyNotNL) 
                bw(b, "anynotnl -> ", u32(i.Out));
                    }
    }
}}
