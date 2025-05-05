// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

// Note to implementers:
// In this package, re is always a *Regexp and r is always a rune.
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;

partial class syntax_package {

// A Regexp is a node in a regular expression syntax tree.
[GoType] partial struct Regexp {
    public Op Op; // operator
    public Flags Flags;
    public slice<ж<Regexp>> Sub; // subexpressions, if any
    public array<ж<Regexp>> Sub0 = new(1); // storage for short Sub
    public slice<rune> Rune; // matched runes, for OpLiteral, OpCharClass
    public array<rune> Rune0 = new(2); // storage for short Rune
    public nint Min;       // min, max for OpRepeat
    public nint Max;
    public nint Cap;       // capturing index, for OpCapture
    public @string Name;    // capturing name, for OpCapture
}

[GoType("num:uint8")] partial struct Op;

//go:generate stringer -type Op -trimprefix Op
// Operators are listed in precedence order, tightest binding to weakest.
// Character class operators are listed simplest to most complex
// (OpLiteral, OpCharClass, OpAnyCharNotNL, OpAnyChar).
public static readonly Op OpNoMatch = /* 1 + iota */ 1;             // matches no strings
public static readonly Op OpEmptyMatch = 2;          // matches empty string
public static readonly Op OpLiteral = 3;             // matches Runes sequence
public static readonly Op OpCharClass = 4;           // matches Runes interpreted as range pair list
public static readonly Op OpAnyCharNotNL = 5;        // matches any character except newline
public static readonly Op OpAnyChar = 6;             // matches any character
public static readonly Op OpBeginLine = 7;           // matches empty string at beginning of line
public static readonly Op OpEndLine = 8;             // matches empty string at end of line
public static readonly Op OpBeginText = 9;           // matches empty string at beginning of text
public static readonly Op OpEndText = 10;             // matches empty string at end of text
public static readonly Op OpWordBoundary = 11;        // matches word boundary `\b`
public static readonly Op OpNoWordBoundary = 12;      // matches word non-boundary `\B`
public static readonly Op OpCapture = 13;             // capturing subexpression with index Cap, optional name Name
public static readonly Op OpStar = 14;                // matches Sub[0] zero or more times
public static readonly Op OpPlus = 15;                // matches Sub[0] one or more times
public static readonly Op OpQuest = 16;               // matches Sub[0] zero or one times
public static readonly Op OpRepeat = 17;              // matches Sub[0] at least Min times, at most Max (Max == -1 is no limit)
public static readonly Op OpConcat = 18;              // matches concatenation of Subs
public static readonly Op OpAlternate = 19;           // matches alternation of Subs

internal static readonly Op opPseudo = 128; // where pseudo-ops start

// Equal reports whether x and y have identical structure.
[GoRecv] public static bool Equal(this ref Regexp x, ж<Regexp> Ꮡy) {
    ref var y = ref Ꮡy.val;

    if (x == nil || y == nil) {
        return x == Ꮡy;
    }
    if (x.Op != y.Op) {
        return false;
    }
    var exprᴛ1 = x.Op;
    if (exprᴛ1 == OpEndText) {
        if ((Flags)(x.Flags & WasDollar) != (Flags)(y.Flags & WasDollar)) {
            // The parse flags remember whether this is \z or \Z.
            return false;
        }
    }
    if (exprᴛ1 == OpLiteral || exprᴛ1 == OpCharClass) {
        return slices.Equal(x.Rune, y.Rune);
    }
    if (exprᴛ1 == OpAlternate || exprᴛ1 == OpConcat) {
        return slices.EqualFunc(x.Sub, y.Sub, (ж<Regexp>).Equal);
    }
    if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest) {
        if ((Flags)(x.Flags & NonGreedy) != (Flags)(y.Flags & NonGreedy) || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }
    if (exprᴛ1 == OpRepeat) {
        if ((Flags)(x.Flags & NonGreedy) != (Flags)(y.Flags & NonGreedy) || x.Min != y.Min || x.Max != y.Max || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }
    if (exprᴛ1 == OpCapture) {
        if (x.Cap != y.Cap || x.Name != y.Name || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    }

    return true;
}

[GoType("num:uint8")] partial struct printFlags;

internal static readonly printFlags flagI = /* 1 << iota */ 1;        // (?i:
internal static readonly printFlags flagM = 2;        // (?m:
internal static readonly printFlags flagS = 4;        // (?s:
internal static readonly printFlags flagOff = 8;      // )
internal static readonly printFlags flagPrec = 16;     // (?: )
internal static readonly UntypedInt negShift = 5;     // flagI<<negShift is (?-i:

// addSpan enables the flags f around start..last,
// by setting flags[start] = f and flags[last] = flagOff.
internal static void addSpan(ж<Regexp> Ꮡstart, ж<Regexp> Ꮡlast, printFlags f, ж<syntax.printFlags> Ꮡflags) {
    ref var start = ref Ꮡstart.val;
    ref var last = ref Ꮡlast.val;
    ref var flags = ref Ꮡflags.val;

    if (flags == default!) {
        flags = new syntax.printFlags();
    }
    (flags)[start] = f;
    (flags)[last] |= flagOff;
}

// maybe start==last

// calcFlags calculates the flags to print around each subexpression in re,
// storing that information in (*flags)[sub] for each affected subexpression.
// The first time an entry needs to be written to *flags, calcFlags allocates the map.
// calcFlags also calculates the flags that must be active or can't be active
// around re and returns those flags.
internal static (printFlags must, printFlags cant) calcFlags(ж<Regexp> Ꮡre, ж<syntax.printFlags> Ꮡflags) {
    printFlags mustΔ1 = default!;
    printFlags cantΔ1 = default!;

    ref var re = ref Ꮡre.val;
    ref var flags = ref Ꮡflags.val;
    var exprᴛ1 = re.Op;
    { /* default: */
        return (0, 0);
    }
    if (exprᴛ1 == OpLiteral) {
        foreach (var (_, r) in re.Rune) {
            // If literal is fold-sensitive, return (flagI, 0) or (0, flagI)
            // according to whether (?i) is active.
            // If literal is not fold-sensitive, return 0, 0.
            if (minFold <= r && r <= maxFold && unicode.SimpleFold(r) != r) {
                if ((Flags)(re.Flags & FoldCase) != 0){
                    return (flagI, 0);
                } else {
                    return (0, flagI);
                }
            }
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpCharClass) {
        for (nint i = 0; i < len(re.Rune); i += 2) {
            // If literal is fold-sensitive, return 0, flagI - (?i) has been compiled out.
            // If literal is not fold-sensitive, return 0, 0.
            var lo = max(minFold, re.Rune[i]);
            var hi = min(maxFold, re.Rune[i + 1]);
            for (var r = lo; r <= hi; r++) {
                for (var f = unicode.SimpleFold(r); f != r; f = unicode.SimpleFold(f)) {
                    if (!(lo <= f && f <= hi) && !inCharClass(f, re.Rune)) {
                        return (0, flagI);
                    }
                }
            }
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpAnyCharNotNL) {
        return (0, flagS);
    }
    if (exprᴛ1 == OpAnyChar) {
        return (flagS, 0);
    }
    if (exprᴛ1 == OpBeginLine || exprᴛ1 == OpEndLine) {
        return (flagM, 0);
    }
    if (exprᴛ1 == OpEndText) {
        if ((Flags)(re.Flags & WasDollar) != 0) {
            // (?-s).
            // (?s).
            // (?m)^ (?m)$
            // (?-m)$
            return (0, flagM);
        }
        return (0, 0);
    }
    if (exprᴛ1 == OpCapture || exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest || exprᴛ1 == OpRepeat) {
        return calcFlags(re.Sub[0], Ꮡflags);
    }
    if (exprᴛ1 == OpConcat || exprᴛ1 == OpAlternate) {
        // Gather the must and cant for each subexpression.
        // When we find a conflicting subexpression, insert the necessary
        // flags around the previously identified span and start over.
        printFlags mustΔ2 = default!;
        printFlags cantΔ2 = default!;
        printFlags allCant = default!;
        ref var start = ref heap<nint>(out var Ꮡstart);
        start = 0;
        ref var last = ref heap<nint>(out var Ꮡlast);
        last = 0;
        var did = false;
        foreach (var (i, sub) in re.Sub) {
            var (subMust, subCant) = calcFlags(sub, Ꮡflags);
            if ((printFlags)(mustΔ2 & subCant) != 0 || (printFlags)(subMust & cantΔ2) != 0) {
                if (mustΔ2 != 0) {
                    addSpan(re.Sub[start], re.Sub[last], mustΔ2, Ꮡflags);
                }
                mustΔ2 = 0;
                cantΔ2 = 0;
                start = i;
                did = true;
            }
            mustΔ2 |= (printFlags)(subMust);
            cantΔ2 |= (printFlags)(subCant);
            allCant |= (printFlags)(subCant);
            if (subMust != 0) {
                last = i;
            }
            if (mustΔ2 == 0 && start == i) {
                start++;
            }
        }
        if (!did) {
            // No conflicts: pass the accumulated must and cant upward.
            return (mustΔ2, cantΔ2);
        }
        if (mustΔ2 != 0) {
            // Conflicts found; need to finish final span.
            addSpan(re.Sub[start], re.Sub[last], mustΔ2, Ꮡflags);
        }
        return (0, allCant);
    }

}

// writeRegexp writes the Perl syntax for the regular expression re to b.
internal static void writeRegexp(ж<strings.Builder> Ꮡb, ж<Regexp> Ꮡre, printFlags f, syntax.printFlags flags) => func((defer, _) => {
    ref var b = ref Ꮡb.val;
    ref var re = ref Ꮡre.val;

    f |= (printFlags)(flags[re]);
    if ((printFlags)(f & flagPrec) != 0 && (printFlags)(f & ~((printFlags)(flagOff | flagPrec))) != 0 && (printFlags)(f & flagOff) != 0) {
        // flagPrec is redundant with other flags being added and terminated
        f &= ~(printFlags)(flagPrec);
    }
    if ((printFlags)(f & ~((printFlags)(flagOff | flagPrec))) != 0) {
        b.WriteString(@"(?"u8);
        if ((printFlags)(f & flagI) != 0) {
            b.WriteString(@"i"u8);
        }
        if ((printFlags)(f & flagM) != 0) {
            b.WriteString(@"m"u8);
        }
        if ((printFlags)(f & flagS) != 0) {
            b.WriteString(@"s"u8);
        }
        if ((printFlags)(f & (((printFlags)(flagM | flagS)) << (int)(negShift))) != 0) {
            b.WriteString(@"-"u8);
            if ((printFlags)(f & (flagM << (int)(negShift))) != 0) {
                b.WriteString(@"m"u8);
            }
            if ((printFlags)(f & (flagS << (int)(negShift))) != 0) {
                b.WriteString(@"s"u8);
            }
        }
        b.WriteString(@":"u8);
    }
    if ((printFlags)(f & flagOff) != 0) {
        deferǃ(b.WriteString, @")", defer);
    }
    if ((printFlags)(f & flagPrec) != 0) {
        b.WriteString(@"(?:"u8);
        deferǃ(b.WriteString, @")", defer);
    }
    var exprᴛ1 = re.Op;
    { /* default: */
        b.WriteString("<invalid op"u8 + strconv.Itoa(((nint)re.Op)) + ">"u8);
    }
    else if (exprᴛ1 == OpNoMatch) {
        b.WriteString(@"[^\x00-\x{10FFFF}]"u8);
    }
    else if (exprᴛ1 == OpEmptyMatch) {
        b.WriteString(@"(?:)"u8);
    }
    else if (exprᴛ1 == OpLiteral) {
        foreach (var (_, r) in re.Rune) {
            escape(Ꮡb, r, false);
        }
    }
    else if (exprᴛ1 == OpCharClass) {
        if (len(re.Rune) % 2 != 0) {
            b.WriteString(@"[invalid char class]"u8);
            break;
        }
        b.WriteRune((rune)'[');
        if (len(re.Rune) == 0){
            b.WriteString(@"^\x00-\x{10FFFF}"u8);
        } else 
        if (re.Rune[0] == 0 && re.Rune[len(re.Rune) - 1] == unicode.MaxRune && len(re.Rune) > 2){
            // Contains 0 and MaxRune. Probably a negated class.
            // Print the gaps.
            b.WriteRune((rune)'^');
            for (nint i = 1; i < len(re.Rune) - 1; i += 2) {
                var (lo, hi) = (re.Rune[i] + 1, re.Rune[i + 1] - 1);
                escape(Ꮡb, lo, lo == (rune)'-');
                if (lo != hi) {
                    if (hi != lo + 1) {
                        b.WriteRune((rune)'-');
                    }
                    escape(Ꮡb, hi, hi == (rune)'-');
                }
            }
        } else {
            for (nint i = 0; i < len(re.Rune); i += 2) {
                var (lo, hi) = (re.Rune[i], re.Rune[i + 1]);
                escape(Ꮡb, lo, lo == (rune)'-');
                if (lo != hi) {
                    if (hi != lo + 1) {
                        b.WriteRune((rune)'-');
                    }
                    escape(Ꮡb, hi, hi == (rune)'-');
                }
            }
        }
        b.WriteRune((rune)']');
    }
    else if (exprᴛ1 == OpAnyCharNotNL || exprᴛ1 == OpAnyChar) {
        b.WriteString(@"."u8);
    }
    else if (exprᴛ1 == OpBeginLine) {
        b.WriteString(@"^"u8);
    }
    else if (exprᴛ1 == OpEndLine) {
        b.WriteString(@"$"u8);
    }
    else if (exprᴛ1 == OpBeginText) {
        b.WriteString(@"\A"u8);
    }
    else if (exprᴛ1 == OpEndText) {
        if ((Flags)(re.Flags & WasDollar) != 0){
            b.WriteString(@"$"u8);
        } else {
            b.WriteString(@"\z"u8);
        }
    }
    else if (exprᴛ1 == OpWordBoundary) {
        b.WriteString(@"\b"u8);
    }
    else if (exprᴛ1 == OpNoWordBoundary) {
        b.WriteString(@"\B"u8);
    }
    else if (exprᴛ1 == OpCapture) {
        if (re.Name != ""u8){
            b.WriteString(@"(?P<"u8);
            b.WriteString(re.Name);
            b.WriteRune((rune)'>');
        } else {
            b.WriteRune((rune)'(');
        }
        if (re.Sub[0].Op != OpEmptyMatch) {
            writeRegexp(Ꮡb, re.Sub[0], flags[re.Sub[0]], flags);
        }
        b.WriteRune((rune)')');
    }
    else if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest || exprᴛ1 == OpRepeat) {
        var p = ((printFlags)0);
        var sub = re.Sub[0];
        if ((~sub).Op > OpCapture || (~sub).Op == OpLiteral && len((~sub).Rune) > 1) {
            p = flagPrec;
        }
        writeRegexp(Ꮡb, sub, p, flags);
        var exprᴛ2 = re.Op;
        if (exprᴛ2 == OpStar) {
            b.WriteRune((rune)'*');
        }
        else if (exprᴛ2 == OpPlus) {
            b.WriteRune((rune)'+');
        }
        else if (exprᴛ2 == OpQuest) {
            b.WriteRune((rune)'?');
        }
        else if (exprᴛ2 == OpRepeat) {
            b.WriteRune((rune)'{');
            b.WriteString(strconv.Itoa(re.Min));
            if (re.Max != re.Min) {
                b.WriteRune((rune)',');
                if (re.Max >= 0) {
                    b.WriteString(strconv.Itoa(re.Max));
                }
            }
            b.WriteRune((rune)'}');
        }

        if ((Flags)(re.Flags & NonGreedy) != 0) {
            b.WriteRune((rune)'?');
        }
    }
    else if (exprᴛ1 == OpConcat) {
        foreach (var (_, sub) in re.Sub) {
            var p = ((printFlags)0);
            if ((~sub).Op == OpAlternate) {
                p = flagPrec;
            }
            writeRegexp(Ꮡb, sub, p, flags);
        }
    }
    else if (exprᴛ1 == OpAlternate) {
        foreach (var (i, sub) in re.Sub) {
            if (i > 0) {
                b.WriteRune((rune)'|');
            }
            writeRegexp(Ꮡb, sub, 0, flags);
        }
    }

});

[GoRecv] public static @string String(this ref Regexp re) {
    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    syntax.printFlags flags = default!;
    var (must, cant) = calcFlags(re, Ꮡ(flags));
    must |= (printFlags)(((printFlags)(cant & ~flagI)) << (int)(negShift));
    if (must != 0) {
        must |= (printFlags)(flagOff);
    }
    writeRegexp(Ꮡb, re, must, flags);
    return b.String();
}

internal static readonly @string meta = @"\.+*?()|[]{}^$"u8;

internal static void escape(ж<strings.Builder> Ꮡb, rune r, bool force) {
    ref var b = ref Ꮡb.val;

    if (unicode.IsPrint(r)) {
        if (strings.ContainsRune(meta, r) || force) {
            b.WriteRune((rune)'\\');
        }
        b.WriteRune(r);
        return;
    }
    switch (r) {
    case (rune)'\a': {
        b.WriteString(@"\a"u8);
        break;
    }
    case (rune)'\f': {
        b.WriteString(@"\f"u8);
        break;
    }
    case (rune)'\n': {
        b.WriteString(@"\n"u8);
        break;
    }
    case (rune)'\r': {
        b.WriteString(@"\r"u8);
        break;
    }
    case (rune)'\t': {
        b.WriteString(@"\t"u8);
        break;
    }
    case (rune)'\v': {
        b.WriteString(@"\v"u8);
        break;
    }
    default: {
        if (r < 256) {
            b.WriteString(@"\x"u8);
            @string s = strconv.FormatInt(((int64)r), 16);
            if (len(s) == 1) {
                b.WriteRune((rune)'0');
            }
            b.WriteString(s);
            break;
        }
        b.WriteString(@"\x{"u8);
        b.WriteString(strconv.FormatInt(((int64)r), 16));
        b.WriteString(@"}"u8);
        break;
    }}

}

// MaxCap walks the regexp to find the maximum capture index.
[GoRecv] public static nint MaxCap(this ref Regexp re) {
    nint m = 0;
    if (re.Op == OpCapture) {
        m = re.Cap;
    }
    foreach (var (_, sub) in re.Sub) {
        {
            nint n = sub.MaxCap(); if (m < n) {
                m = n;
            }
        }
    }
    return m;
}

// CapNames walks the regexp to find the names of capturing groups.
[GoRecv] public static slice<@string> CapNames(this ref Regexp re) {
    var names = new slice<@string>(re.MaxCap() + 1);
    re.capNames(names);
    return names;
}

[GoRecv] internal static void capNames(this ref Regexp re, slice<@string> names) {
    if (re.Op == OpCapture) {
        names[re.Cap] = re.Name;
    }
    foreach (var (_, sub) in re.Sub) {
        sub.capNames(names);
    }
}

} // end syntax_package
