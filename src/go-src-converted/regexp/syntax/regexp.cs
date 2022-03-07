// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 06 22:23:39 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Program Files\Go\src\regexp\syntax\regexp.go
// Note to implementers:
// In this package, re is always a *Regexp and r is always a rune.

using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;

namespace go.regexp;

public static partial class syntax_package {

    // A Regexp is a node in a regular expression syntax tree.
public partial struct Regexp {
    public Op Op; // operator
    public Flags Flags;
    public slice<ptr<Regexp>> Sub; // subexpressions, if any
    public array<ptr<Regexp>> Sub0; // storage for short Sub
    public slice<int> Rune; // matched runes, for OpLiteral, OpCharClass
    public array<int> Rune0; // storage for short Rune
    public nint Min; // min, max for OpRepeat
    public nint Max; // min, max for OpRepeat
    public nint Cap; // capturing index, for OpCapture
    public @string Name; // capturing name, for OpCapture
}

//go:generate stringer -type Op -trimprefix Op

// An Op is a single regular expression operator.
public partial struct Op { // : byte
}

// Operators are listed in precedence order, tightest binding to weakest.
// Character class operators are listed simplest to most complex
// (OpLiteral, OpCharClass, OpAnyCharNotNL, OpAnyChar).

public static readonly Op OpNoMatch = 1 + iota; // matches no strings
public static readonly var OpEmptyMatch = 0; // matches empty string
public static readonly var OpLiteral = 1; // matches Runes sequence
public static readonly var OpCharClass = 2; // matches Runes interpreted as range pair list
public static readonly var OpAnyCharNotNL = 3; // matches any character except newline
public static readonly var OpAnyChar = 4; // matches any character
public static readonly var OpBeginLine = 5; // matches empty string at beginning of line
public static readonly var OpEndLine = 6; // matches empty string at end of line
public static readonly var OpBeginText = 7; // matches empty string at beginning of text
public static readonly var OpEndText = 8; // matches empty string at end of text
public static readonly var OpWordBoundary = 9; // matches word boundary `\b`
public static readonly var OpNoWordBoundary = 10; // matches word non-boundary `\B`
public static readonly var OpCapture = 11; // capturing subexpression with index Cap, optional name Name
public static readonly var OpStar = 12; // matches Sub[0] zero or more times
public static readonly var OpPlus = 13; // matches Sub[0] one or more times
public static readonly var OpQuest = 14; // matches Sub[0] zero or one times
public static readonly var OpRepeat = 15; // matches Sub[0] at least Min times, at most Max (Max == -1 is no limit)
public static readonly var OpConcat = 16; // matches concatenation of Subs
public static readonly var OpAlternate = 17; // matches alternation of Subs

private static readonly Op opPseudo = 128; // where pseudo-ops start

// Equal reports whether x and y have identical structure.
 // where pseudo-ops start

// Equal reports whether x and y have identical structure.
private static bool Equal(this ptr<Regexp> _addr_x, ptr<Regexp> _addr_y) {
    ref Regexp x = ref _addr_x.val;
    ref Regexp y = ref _addr_y.val;

    if (x == null || y == null) {
        return x == y;
    }
    if (x.Op != y.Op) {
        return false;
    }

    if (x.Op == OpEndText) 
        // The parse flags remember whether this is \z or \Z.
        if (x.Flags & WasDollar != y.Flags & WasDollar) {
            return false;
        }
    else if (x.Op == OpLiteral || x.Op == OpCharClass) 
        if (len(x.Rune) != len(y.Rune)) {
            return false;
        }
        {
            var i__prev1 = i;

            foreach (var (__i, __r) in x.Rune) {
                i = __i;
                r = __r;
                if (r != y.Rune[i]) {
                    return false;
                }
            }

            i = i__prev1;
        }
    else if (x.Op == OpAlternate || x.Op == OpConcat) 
        if (len(x.Sub) != len(y.Sub)) {
            return false;
        }
        {
            var i__prev1 = i;

            foreach (var (__i, __sub) in x.Sub) {
                i = __i;
                sub = __sub;
                if (!sub.Equal(y.Sub[i])) {
                    return false;
                }
            }

            i = i__prev1;
        }
    else if (x.Op == OpStar || x.Op == OpPlus || x.Op == OpQuest) 
        if (x.Flags & NonGreedy != y.Flags & NonGreedy || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    else if (x.Op == OpRepeat) 
        if (x.Flags & NonGreedy != y.Flags & NonGreedy || x.Min != y.Min || x.Max != y.Max || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
    else if (x.Op == OpCapture) 
        if (x.Cap != y.Cap || x.Name != y.Name || !x.Sub[0].Equal(y.Sub[0])) {
            return false;
        }
        return true;

}

// writeRegexp writes the Perl syntax for the regular expression re to b.
private static void writeRegexp(ptr<strings.Builder> _addr_b, ptr<Regexp> _addr_re) {
    ref strings.Builder b = ref _addr_b.val;
    ref Regexp re = ref _addr_re.val;


    if (re.Op == OpNoMatch) 
        b.WriteString("[^\\x00-\\x{10FFFF}]");
    else if (re.Op == OpEmptyMatch) 
        b.WriteString("(?:)");
    else if (re.Op == OpLiteral) 
        if (re.Flags & FoldCase != 0) {
            b.WriteString("(?i:");
        }
        foreach (var (_, r) in re.Rune) {
            escape(_addr_b, r, false);
        }        if (re.Flags & FoldCase != 0) {
            b.WriteString(")");
        }
    else if (re.Op == OpCharClass) 
        if (len(re.Rune) % 2 != 0) {
            b.WriteString("[invalid char class]");
            break;
        }
        b.WriteRune('[');
        if (len(re.Rune) == 0) {
            b.WriteString("^\\x00-\\x{10FFFF}");
        }
        else if (re.Rune[0] == 0 && re.Rune[len(re.Rune) - 1] == unicode.MaxRune && len(re.Rune) > 2) { 
            // Contains 0 and MaxRune. Probably a negated class.
            // Print the gaps.
            b.WriteRune('^');
            {
                nint i__prev1 = i;

                nint i = 1;

                while (i < len(re.Rune) - 1) {
                    var lo = re.Rune[i] + 1;
                    var hi = re.Rune[i + 1] - 1;
                    escape(_addr_b, lo, lo == '-');
                    if (lo != hi) {
                        b.WriteRune('-');
                        escape(_addr_b, hi, hi == '-');
                    i += 2;
                    }

                }
        else


                i = i__prev1;
            }

        } {
            {
                nint i__prev1 = i;

                i = 0;

                while (i < len(re.Rune)) {
                    lo = re.Rune[i];
                    hi = re.Rune[i + 1];
                    escape(_addr_b, lo, lo == '-');
                    if (lo != hi) {
                        b.WriteRune('-');
                        escape(_addr_b, hi, hi == '-');
                    i += 2;
                    }

                }


                i = i__prev1;
            }

        }
        b.WriteRune(']');
    else if (re.Op == OpAnyCharNotNL) 
        b.WriteString("(?-s:.)");
    else if (re.Op == OpAnyChar) 
        b.WriteString("(?s:.)");
    else if (re.Op == OpBeginLine) 
        b.WriteString("(?m:^)");
    else if (re.Op == OpEndLine) 
        b.WriteString("(?m:$)");
    else if (re.Op == OpBeginText) 
        b.WriteString("\\A");
    else if (re.Op == OpEndText) 
        if (re.Flags & WasDollar != 0) {
            b.WriteString("(?-m:$)");
        }
        else
 {
            b.WriteString("\\z");
        }
    else if (re.Op == OpWordBoundary) 
        b.WriteString("\\b");
    else if (re.Op == OpNoWordBoundary) 
        b.WriteString("\\B");
    else if (re.Op == OpCapture) 
        if (re.Name != "") {
            b.WriteString("(?P<");
            b.WriteString(re.Name);
            b.WriteRune('>');
        }
        else
 {
            b.WriteRune('(');
        }
        if (re.Sub[0].Op != OpEmptyMatch) {
            writeRegexp(_addr_b, _addr_re.Sub[0]);
        }
        b.WriteRune(')');
    else if (re.Op == OpStar || re.Op == OpPlus || re.Op == OpQuest || re.Op == OpRepeat) 
        {
            var sub__prev1 = sub;

            var sub = re.Sub[0];

            if (sub.Op > OpCapture || sub.Op == OpLiteral && len(sub.Rune) > 1) {
                b.WriteString("(?:");
                writeRegexp(_addr_b, _addr_sub);
                b.WriteString(")");
            }
            else
 {
                writeRegexp(_addr_b, _addr_sub);
            }

            sub = sub__prev1;

        }


        if (re.Op == OpStar) 
            b.WriteRune('*');
        else if (re.Op == OpPlus) 
            b.WriteRune('+');
        else if (re.Op == OpQuest) 
            b.WriteRune('?');
        else if (re.Op == OpRepeat) 
            b.WriteRune('{');
            b.WriteString(strconv.Itoa(re.Min));
            if (re.Max != re.Min) {
                b.WriteRune(',');
                if (re.Max >= 0) {
                    b.WriteString(strconv.Itoa(re.Max));
                }
            }
            b.WriteRune('}');
                if (re.Flags & NonGreedy != 0) {
            b.WriteRune('?');
        }
    else if (re.Op == OpConcat) 
        {
            var sub__prev1 = sub;

            foreach (var (_, __sub) in re.Sub) {
                sub = __sub;
                if (sub.Op == OpAlternate) {
                    b.WriteString("(?:");
                    writeRegexp(_addr_b, _addr_sub);
                    b.WriteString(")");
                }
                else
 {
                    writeRegexp(_addr_b, _addr_sub);
                }

            }

            sub = sub__prev1;
        }
    else if (re.Op == OpAlternate) 
        {
            nint i__prev1 = i;
            var sub__prev1 = sub;

            foreach (var (__i, __sub) in re.Sub) {
                i = __i;
                sub = __sub;
                if (i > 0) {
                    b.WriteRune('|');
                }
                writeRegexp(_addr_b, _addr_sub);
            }

            i = i__prev1;
            sub = sub__prev1;
        }
    else 
        b.WriteString("<invalid op" + strconv.Itoa(int(re.Op)) + ">");
    
}

private static @string String(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
    writeRegexp(_addr_b, _addr_re);
    return b.String();
}

private static readonly @string meta = "\\.+*?()|[]{}^$";



private static void escape(ptr<strings.Builder> _addr_b, int r, bool force) {
    ref strings.Builder b = ref _addr_b.val;

    if (unicode.IsPrint(r)) {
        if (strings.ContainsRune(meta, r) || force) {
            b.WriteRune('\\');
        }
        b.WriteRune(r);
        return ;

    }
    switch (r) {
        case '\a': 
            b.WriteString("\\a");
            break;
        case '\f': 
            b.WriteString("\\f");
            break;
        case '\n': 
            b.WriteString("\\n");
            break;
        case '\r': 
            b.WriteString("\\r");
            break;
        case '\t': 
            b.WriteString("\\t");
            break;
        case '\v': 
            b.WriteString("\\v");
            break;
        default: 
            if (r < 0x100) {
                b.WriteString("\\x");
                var s = strconv.FormatInt(int64(r), 16);
                if (len(s) == 1) {
                    b.WriteRune('0');
                }
                b.WriteString(s);
                break;
            }
            b.WriteString("\\x{");
            b.WriteString(strconv.FormatInt(int64(r), 16));
            b.WriteString("}");

            break;
    }

}

// MaxCap walks the regexp to find the maximum capture index.
private static nint MaxCap(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    nint m = 0;
    if (re.Op == OpCapture) {
        m = re.Cap;
    }
    foreach (var (_, sub) in re.Sub) {
        {
            var n = sub.MaxCap();

            if (m < n) {
                m = n;
            }

        }

    }    return m;

}

// CapNames walks the regexp to find the names of capturing groups.
private static slice<@string> CapNames(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    var names = make_slice<@string>(re.MaxCap() + 1);
    re.capNames(names);
    return names;
}

private static void capNames(this ptr<Regexp> _addr_re, slice<@string> names) {
    ref Regexp re = ref _addr_re.val;

    if (re.Op == OpCapture) {
        names[re.Cap] = re.Name;
    }
    foreach (var (_, sub) in re.Sub) {
        sub.capNames(names);
    }
}

} // end syntax_package
