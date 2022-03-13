// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 13 05:38:03 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Program Files\Go\src\regexp\syntax\prog.go
namespace go.regexp;

using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;


// Compiled program.
// May not belong in this package, but convenient for now.

// A Prog is a compiled regular expression program.

public static partial class syntax_package {

public partial struct Prog {
    public slice<Inst> Inst;
    public nint Start; // index of start instruction
    public nint NumCap; // number of InstCapture insts in re
}

// An InstOp is an instruction opcode.
public partial struct InstOp { // : byte
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

public static @string String(this InstOp i) {
    if (uint(i) >= uint(len(instOpNames))) {
        return "";
    }
    return instOpNames[i];
}

// An EmptyOp specifies a kind or mixture of zero-width assertions.
public partial struct EmptyOp { // : byte
}

public static readonly EmptyOp EmptyBeginLine = 1 << (int)(iota);
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
public static EmptyOp EmptyOpContext(int r1, int r2) {
    EmptyOp op = EmptyNoWordBoundary;
    byte boundary = default;

    if (IsWordChar(r1)) 
        boundary = 1;
    else if (r1 == '\n') 
        op |= EmptyBeginLine;
    else if (r1 < 0) 
        op |= EmptyBeginText | EmptyBeginLine;
    
    if (IsWordChar(r2)) 
        boundary ^= 1;
    else if (r2 == '\n') 
        op |= EmptyEndLine;
    else if (r2 < 0) 
        op |= EmptyEndText | EmptyEndLine;
        if (boundary != 0) { // IsWordChar(r1) != IsWordChar(r2)
        op ^= (EmptyWordBoundary | EmptyNoWordBoundary);
    }
    return op;
}

// IsWordChar reports whether r is consider a ``word character''
// during the evaluation of the \b and \B zero-width assertions.
// These assertions are ASCII-only: the word characters are [A-Za-z0-9_].
public static bool IsWordChar(int r) {
    return 'A' <= r && r <= 'Z' || 'a' <= r && r <= 'z' || '0' <= r && r <= '9' || r == '_';
}

// An Inst is a single instruction in a regular expression program.
public partial struct Inst {
    public InstOp Op;
    public uint Out; // all but InstMatch, InstFail
    public uint Arg; // InstAlt, InstAltMatch, InstCapture, InstEmptyWidth
    public slice<int> Rune;
}

private static @string String(this ptr<Prog> _addr_p) {
    ref Prog p = ref _addr_p.val;

    ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
    dumpProg(_addr_b, _addr_p);
    return b.String();
}

// skipNop follows any no-op or capturing instructions.
private static ptr<Inst> skipNop(this ptr<Prog> _addr_p, uint pc) {
    ref Prog p = ref _addr_p.val;

    var i = _addr_p.Inst[pc];
    while (i.Op == InstNop || i.Op == InstCapture) {
        i = _addr_p.Inst[i.Out];
    }
    return _addr_i!;
}

// op returns i.Op but merges all the Rune special cases into InstRune
private static InstOp op(this ptr<Inst> _addr_i) {
    ref Inst i = ref _addr_i.val;

    var op = i.Op;

    if (op == InstRune1 || op == InstRuneAny || op == InstRuneAnyNotNL) 
        op = InstRune;
        return op;
}

// Prefix returns a literal string that all matches for the
// regexp must start with. Complete is true if the prefix
// is the entire match.
private static (@string, bool) Prefix(this ptr<Prog> _addr_p) {
    @string prefix = default;
    bool complete = default;
    ref Prog p = ref _addr_p.val;

    var i = p.skipNop(uint32(p.Start)); 

    // Avoid allocation of buffer if prefix is empty.
    if (i.op() != InstRune || len(i.Rune) != 1) {
        return ("", i.Op == InstMatch);
    }
    strings.Builder buf = default;
    while (i.op() == InstRune && len(i.Rune) == 1 && Flags(i.Arg) & FoldCase == 0) {
        buf.WriteRune(i.Rune[0]);
        i = p.skipNop(i.Out);
    }
    return (buf.String(), i.Op == InstMatch);
}

// StartCond returns the leading empty-width conditions that must
// be true in any match. It returns ^EmptyOp(0) if no matches are possible.
private static EmptyOp StartCond(this ptr<Prog> _addr_p) {
    ref Prog p = ref _addr_p.val;

    EmptyOp flag = default;
    var pc = uint32(p.Start);
    var i = _addr_p.Inst[pc];
Loop:
    while (true) {

        if (i.Op == InstEmptyWidth) 
            flag |= EmptyOp(i.Arg);
        else if (i.Op == InstFail) 
            return ~EmptyOp(0);
        else if (i.Op == InstCapture || i.Op == InstNop)         else 
            _breakLoop = true;
            break;
                pc = i.Out;
        i = _addr_p.Inst[pc];
    }
    return flag;
}

private static readonly nint noMatch = -1;

// MatchRune reports whether the instruction matches (and consumes) r.
// It should only be called when i.Op == InstRune.


// MatchRune reports whether the instruction matches (and consumes) r.
// It should only be called when i.Op == InstRune.
private static bool MatchRune(this ptr<Inst> _addr_i, int r) {
    ref Inst i = ref _addr_i.val;

    return i.MatchRunePos(r) != noMatch;
}

// MatchRunePos checks whether the instruction matches (and consumes) r.
// If so, MatchRunePos returns the index of the matching rune pair
// (or, when len(i.Rune) == 1, rune singleton).
// If not, MatchRunePos returns -1.
// MatchRunePos should only be called when i.Op == InstRune.
private static nint MatchRunePos(this ptr<Inst> _addr_i, int r) {
    ref Inst i = ref _addr_i.val;

    var rune = i.Rune;

    switch (len(rune)) {
        case 0: 
            return noMatch;
            break;
        case 1: 
            // Special case: single-rune slice is from literal string, not char class.
            var r0 = rune[0];
            if (r == r0) {
                return 0;
            }
            if (Flags(i.Arg) & FoldCase != 0) {
                {
                    var r1 = unicode.SimpleFold(r0);

                    while (r1 != r0) {
                        if (r == r1) {
                            return 0;
                        r1 = unicode.SimpleFold(r1);
                        }
                    }

                }
            }
            return noMatch;
            break;
        case 2: 
            if (r >= rune[0] && r <= rune[1]) {
                return 0;
            }
            return noMatch;
            break;
        case 4: 
            // Linear search for a few pairs.
            // Should handle ASCII well.

        case 6: 
            // Linear search for a few pairs.
            // Should handle ASCII well.

        case 8: 
            // Linear search for a few pairs.
            // Should handle ASCII well.
            {
                nint j = 0;

                while (j < len(rune)) {
                    if (r < rune[j]) {
                        return noMatch;
                    j += 2;
                    }
                    if (r <= rune[j + 1]) {
                        return j / 2;
                    }
                }

            }
            return noMatch;
            break;
    } 

    // Otherwise binary search.
    nint lo = 0;
    var hi = len(rune) / 2;
    while (lo < hi) {
        var m = lo + (hi - lo) / 2;
        {
            var c = rune[2 * m];

            if (c <= r) {
                if (r <= rune[2 * m + 1]) {
                    return m;
                }
                lo = m + 1;
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
private static bool MatchEmptyWidth(this ptr<Inst> _addr_i, int before, int after) => func((_, panic, _) => {
    ref Inst i = ref _addr_i.val;


    if (EmptyOp(i.Arg) == EmptyBeginLine) 
        return before == '\n' || before == -1;
    else if (EmptyOp(i.Arg) == EmptyEndLine) 
        return after == '\n' || after == -1;
    else if (EmptyOp(i.Arg) == EmptyBeginText) 
        return before == -1;
    else if (EmptyOp(i.Arg) == EmptyEndText) 
        return after == -1;
    else if (EmptyOp(i.Arg) == EmptyWordBoundary) 
        return IsWordChar(before) != IsWordChar(after);
    else if (EmptyOp(i.Arg) == EmptyNoWordBoundary) 
        return IsWordChar(before) == IsWordChar(after);
        panic("unknown empty width arg");
});

private static @string String(this ptr<Inst> _addr_i) {
    ref Inst i = ref _addr_i.val;

    ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
    dumpInst(_addr_b, _addr_i);
    return b.String();
}

private static void bw(ptr<strings.Builder> _addr_b, params @string[] args) {
    args = args.Clone();
    ref strings.Builder b = ref _addr_b.val;

    foreach (var (_, s) in args) {
        b.WriteString(s);
    }
}

private static void dumpProg(ptr<strings.Builder> _addr_b, ptr<Prog> _addr_p) {
    ref strings.Builder b = ref _addr_b.val;
    ref Prog p = ref _addr_p.val;

    foreach (var (j) in p.Inst) {
        var i = _addr_p.Inst[j];
        var pc = strconv.Itoa(j);
        if (len(pc) < 3) {
            b.WriteString("   "[(int)len(pc)..]);
        }
        if (j == p.Start) {
            pc += "*";
        }
        bw(_addr_b, pc, "\t");
        dumpInst(_addr_b, _addr_i);
        bw(_addr_b, "\n");
    }
}

private static @string u32(uint i) {
    return strconv.FormatUint(uint64(i), 10);
}

private static void dumpInst(ptr<strings.Builder> _addr_b, ptr<Inst> _addr_i) {
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
        if (i.Rune == null) { 
            // shouldn't happen
            bw(_addr_b, "rune <nil>");
        }
        bw(_addr_b, "rune ", strconv.QuoteToASCII(string(i.Rune)));
        if (Flags(i.Arg) & FoldCase != 0) {
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

} // end syntax_package
