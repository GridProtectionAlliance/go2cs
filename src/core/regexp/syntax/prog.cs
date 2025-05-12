// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using unicode;
using ꓸꓸꓸ@string = Span<@string>;

partial class syntax_package {

// Compiled program.
// May not belong in this package, but convenient for now.

// A Prog is a compiled regular expression program.
[GoType] partial struct Prog {
    public slice<Inst> Inst;
    public nint Start; // index of start instruction
    public nint NumCap; // number of InstCapture insts in re
}

[GoType("num:uint8")] partial struct InstOp;

public static readonly InstOp InstAlt = /* iota */ 0;
public static readonly InstOp InstAltMatch = 1;
public static readonly InstOp InstCapture = 2;
public static readonly InstOp InstEmptyWidth = 3;
public static readonly InstOp InstMatch = 4;
public static readonly InstOp InstFail = 5;
public static readonly InstOp InstNop = 6;
public static readonly InstOp InstRune = 7;
public static readonly InstOp InstRune1 = 8;
public static readonly InstOp InstRuneAny = 9;
public static readonly InstOp InstRuneAnyNotNL = 10;

internal static slice<@string> instOpNames = new @string[]{
    "InstAlt",
    "InstAltMatch",
    "InstCapture",
    "InstEmptyWidth",
    "InstMatch",
    "InstFail",
    "InstNop",
    "InstRune",
    "InstRune1",
    "InstRuneAny",
    "InstRuneAnyNotNL"
}.slice();

public static @string String(this InstOp i) {
    if (((nuint)i) >= ((nuint)len(instOpNames))) {
        return ""u8;
    }
    return instOpNames[i];
}

[GoType("num:uint8")] partial struct EmptyOp;

public static readonly EmptyOp EmptyBeginLine = /* 1 << iota */ 1;
public static readonly EmptyOp EmptyEndLine = 2;
public static readonly EmptyOp EmptyBeginText = 4;
public static readonly EmptyOp EmptyEndText = 8;
public static readonly EmptyOp EmptyWordBoundary = 16;
public static readonly EmptyOp EmptyNoWordBoundary = 32;

// EmptyOpContext returns the zero-width assertions
// satisfied at the position between the runes r1 and r2.
// Passing r1 == -1 indicates that the position is
// at the beginning of the text.
// Passing r2 == -1 indicates that the position is
// at the end of the text.
public static EmptyOp EmptyOpContext(rune r1, rune r2) {
    EmptyOp op = EmptyNoWordBoundary;
    byte boundary = default!;
    switch (ᐧ) {
    case {} when IsWordChar(r1): {
        boundary = 1;
        break;
    }
    case {} when r1 is (rune)'\n': {
        op |= (EmptyOp)(EmptyBeginLine);
        break;
    }
    case {} when r1 is < 0: {
        op |= (EmptyOp)((EmptyOp)(EmptyBeginText | EmptyBeginLine));
        break;
    }}

    switch (ᐧ) {
    case {} when IsWordChar(r2): {
        boundary ^= (byte)(1);
        break;
    }
    case {} when r2 is (rune)'\n': {
        op |= (EmptyOp)(EmptyEndLine);
        break;
    }
    case {} when r2 is < 0: {
        op |= (EmptyOp)((EmptyOp)(EmptyEndText | EmptyEndLine));
        break;
    }}

    if (boundary != 0) {
        // IsWordChar(r1) != IsWordChar(r2)
        op ^= (EmptyOp)(((EmptyOp)(EmptyWordBoundary | EmptyNoWordBoundary)));
    }
    return op;
}

// IsWordChar reports whether r is considered a “word character”
// during the evaluation of the \b and \B zero-width assertions.
// These assertions are ASCII-only: the word characters are [A-Za-z0-9_].
public static bool IsWordChar(rune r) {
    // Test for lowercase letters first, as these occur more
    // frequently than uppercase letters in common cases.
    return (rune)'a' <= r && r <= (rune)'z' || (rune)'A' <= r && r <= (rune)'Z' || (rune)'0' <= r && r <= (rune)'9' || r == (rune)'_';
}

// An Inst is a single instruction in a regular expression program.
[GoType] partial struct Inst {
    public InstOp Op;
    public uint32 Out; // all but InstMatch, InstFail
    public uint32 Arg; // InstAlt, InstAltMatch, InstCapture, InstEmptyWidth
    public slice<rune> Rune;
}

[GoRecv] public static @string String(this ref Prog p) {
    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    dumpProg(Ꮡb, p);
    return b.String();
}

// skipNop follows any no-op or capturing instructions.
[GoRecv] internal static ж<Inst> skipNop(this ref Prog p, uint32 pc) {
    var i = Ꮡ(p.Inst[pc]);
    while ((~i).Op == InstNop || (~i).Op == InstCapture) {
        i = Ꮡ(p.Inst[(~i).Out]);
    }
    return i;
}

// op returns i.Op but merges all the Rune special cases into InstRune
[GoRecv] internal static InstOp op(this ref Inst i) {
    var op = i.Op;
    var exprᴛ1 = op;
    if (exprᴛ1 == InstRune1 || exprᴛ1 == InstRuneAny || exprᴛ1 == InstRuneAnyNotNL) {
        op = InstRune;
    }

    return op;
}

// Prefix returns a literal string that all matches for the
// regexp must start with. Complete is true if the prefix
// is the entire match.
[GoRecv] public static (@string prefix, bool complete) Prefix(this ref Prog p) {
    @string prefix = default!;
    bool complete = default!;

    var i = p.skipNop(((uint32)p.Start));
    // Avoid allocation of buffer if prefix is empty.
    if (i.op() != InstRune || len((~i).Rune) != 1) {
        return ("", (~i).Op == InstMatch);
    }
    // Have prefix; gather characters.
    strings.Builder buf = default!;
    while (i.op() == InstRune && len((~i).Rune) == 1 && (Flags)(((Flags)(~i).Arg) & FoldCase) == 0 && (~i).Rune[0] != utf8.RuneError) {
        buf.WriteRune((~i).Rune[0]);
        i = p.skipNop((~i).Out);
    }
    return (buf.String(), (~i).Op == InstMatch);
}

// StartCond returns the leading empty-width conditions that must
// be true in any match. It returns ^EmptyOp(0) if no matches are possible.
[GoRecv] public static EmptyOp StartCond(this ref Prog p) {
    EmptyOp flag = default!;
    var pc = ((uint32)p.Start);
    var i = Ꮡ(p.Inst[pc]);
Loop:
    while (ᐧ) {
        var exprᴛ1 = (~i).Op;
        if (exprᴛ1 == InstEmptyWidth) {
            flag |= (EmptyOp)(((EmptyOp)(~i).Arg));
        }
        else if (exprᴛ1 == InstFail) {
            return ~((EmptyOp)0);
        }
        if (exprᴛ1 == InstCapture || exprᴛ1 == InstNop) {
        }
        { /* default: */
            goto break_Loop;
        }

        // skip
        pc = i.val.Out;
        i = Ꮡ(p.Inst[pc]);
continue_Loop:;
    }
break_Loop:;
    return flag;
}

internal static readonly GoUntyped noMatch = /* -1 */
    GoUntyped.Parse("-1");

// MatchRune reports whether the instruction matches (and consumes) r.
// It should only be called when i.Op == [InstRune].
[GoRecv] public static bool MatchRune(this ref Inst i, rune r) {
    return i.MatchRunePos(r) != noMatch;
}

// MatchRunePos checks whether the instruction matches (and consumes) r.
// If so, MatchRunePos returns the index of the matching rune pair
// (or, when len(i.Rune) == 1, rune singleton).
// If not, MatchRunePos returns -1.
// MatchRunePos should only be called when i.Op == [InstRune].
[GoRecv] public static nint MatchRunePos(this ref Inst i, rune r) {
    var rune = i.Rune;
    switch (len(rune)) {
    case 0: {
        return noMatch;
    }
    case 1: {
        var r0 = rune[0];
        if (r == r0) {
            // Special case: single-rune slice is from literal string, not char class.
            return 0;
        }
        if ((Flags)(((Flags)i.Arg) & FoldCase) != 0) {
            for (var r1 = unicode.SimpleFold(r0); r1 != r0; r1 = unicode.SimpleFold(r1)) {
                if (r == r1) {
                    return 0;
                }
            }
        }
        return noMatch;
    }
    case 2: {
        if (r >= rune[0] && r <= rune[1]) {
            return 0;
        }
        return noMatch;
    }
    case 4 or 6 or 8: {
        for (nint j = 0; j < len(rune); j += 2) {
            // Linear search for a few pairs.
            // Should handle ASCII well.
            if (r < rune[j]) {
                return noMatch;
            }
            if (r <= rune[j + 1]) {
                return j / 2;
            }
        }
        return noMatch;
    }}

    // Otherwise binary search.
    nint lo = 0;
    nint hi = len(rune) / 2;
    while (lo < hi) {
        nint m = ((nint)(((nuint)(lo + hi)) >> (int)(1)));
        {
            var c = rune[2 * m]; if (c <= r){
                if (r <= rune[2 * m + 1]) {
                    return m;
                }
                lo = m + 1;
            } else {
                hi = m;
            }
        }
    }
    return noMatch;
}

// MatchEmptyWidth reports whether the instruction matches
// an empty string between the runes before and after.
// It should only be called when i.Op == [InstEmptyWidth].
[GoRecv] public static bool MatchEmptyWidth(this ref Inst i, rune before, rune after) {
    var exprᴛ1 = ((EmptyOp)i.Arg);
    if (exprᴛ1 == EmptyBeginLine) {
        return before == (rune)'\n' || before == -1;
    }
    if (exprᴛ1 == EmptyEndLine) {
        return after == (rune)'\n' || after == -1;
    }
    if (exprᴛ1 == EmptyBeginText) {
        return before == -1;
    }
    if (exprᴛ1 == EmptyEndText) {
        return after == -1;
    }
    if (exprᴛ1 == EmptyWordBoundary) {
        return IsWordChar(before) != IsWordChar(after);
    }
    if (exprᴛ1 == EmptyNoWordBoundary) {
        return IsWordChar(before) == IsWordChar(after);
    }

    throw panic("unknown empty width arg");
}

[GoRecv] public static @string String(this ref Inst i) {
    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    dumpInst(Ꮡb, i);
    return b.String();
}

internal static void bw(ж<strings.Builder> Ꮡb, params ꓸꓸꓸ@string argsʗp) {
    var args = argsʗp.slice();

    ref var b = ref Ꮡb.val;
    foreach (var (_, s) in args) {
        b.WriteString(s);
    }
}

internal static void dumpProg(ж<strings.Builder> Ꮡb, ж<Prog> Ꮡp) {
    ref var b = ref Ꮡb.val;
    ref var p = ref Ꮡp.val;

    foreach (var (j, _) in p.Inst) {
        var i = Ꮡ(p.Inst, j);
        @string pc = strconv.Itoa(j);
        if (len(pc) < 3) {
            b.WriteString("   "u8[(int)(len(pc))..]);
        }
        if (j == p.Start) {
            pc += "*"u8;
        }
        bw(Ꮡb, pc, "\t");
        dumpInst(Ꮡb, i);
        bw(Ꮡb, "\n"u8);
    }
}

internal static @string u32(uint32 i) {
    return strconv.FormatUint(((uint64)i), 10);
}

internal static void dumpInst(ж<strings.Builder> Ꮡb, ж<Inst> Ꮡi) {
    ref var b = ref Ꮡb.val;
    ref var i = ref Ꮡi.val;

    var exprᴛ1 = i.Op;
    if (exprᴛ1 == InstAlt) {
        bw(Ꮡb, "alt -> "u8, u32(i.Out), ", ", u32(i.Arg));
    }
    else if (exprᴛ1 == InstAltMatch) {
        bw(Ꮡb, "altmatch -> "u8, u32(i.Out), ", ", u32(i.Arg));
    }
    else if (exprᴛ1 == InstCapture) {
        bw(Ꮡb, "cap "u8, u32(i.Arg), " -> ", u32(i.Out));
    }
    else if (exprᴛ1 == InstEmptyWidth) {
        bw(Ꮡb, "empty "u8, u32(i.Arg), " -> ", u32(i.Out));
    }
    else if (exprᴛ1 == InstMatch) {
        bw(Ꮡb, "match"u8);
    }
    else if (exprᴛ1 == InstFail) {
        bw(Ꮡb, "fail"u8);
    }
    else if (exprᴛ1 == InstNop) {
        bw(Ꮡb, "nop -> "u8, u32(i.Out));
    }
    else if (exprᴛ1 == InstRune) {
        if (i.Rune == default!) {
            // shouldn't happen
            bw(Ꮡb, "rune <nil>"u8);
        }
        bw(Ꮡb, "rune "u8, strconv.QuoteToASCII(((@string)i.Rune)));
        if ((Flags)(((Flags)i.Arg) & FoldCase) != 0) {
            bw(Ꮡb, "/i"u8);
        }
        bw(Ꮡb, " -> "u8, u32(i.Out));
    }
    else if (exprᴛ1 == InstRune1) {
        bw(Ꮡb, "rune1 "u8, strconv.QuoteToASCII(((@string)i.Rune)), " -> ", u32(i.Out));
    }
    else if (exprᴛ1 == InstRuneAny) {
        bw(Ꮡb, "any -> "u8, u32(i.Out));
    }
    else if (exprᴛ1 == InstRuneAnyNotNL) {
        bw(Ꮡb, "anynotnl -> "u8, u32(i.Out));
    }

}

} // end syntax_package
