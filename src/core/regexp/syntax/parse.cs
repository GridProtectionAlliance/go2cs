// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using sort = sort_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class syntax_package {

// An Error describes a failure to parse a regular expression
// and gives the offending expression.
[GoType] partial struct ΔError {
    public ErrorCode Code;
    public @string Expr;
}

[GoRecv] public static @string Error(this ref ΔError e) {
    return "error parsing regexp: "u8 + e.Code.String() + ": `"u8 + e.Expr + "`"u8;
}

[GoType("@string")] partial struct ErrorCode;

public static readonly @string ErrInternalError = "regexp/syntax: internal error"u8;
public static readonly @string ErrInvalidCharClass = "invalid character class"u8;
public static readonly @string ErrInvalidCharRange = "invalid character class range"u8;
public static readonly @string ErrInvalidEscape = "invalid escape sequence"u8;
public static readonly @string ErrInvalidNamedCapture = "invalid named capture"u8;
public static readonly @string ErrInvalidPerlOp = "invalid or unsupported Perl syntax"u8;
public static readonly @string ErrInvalidRepeatOp = "invalid nested repetition operator"u8;
public static readonly @string ErrInvalidRepeatSize = "invalid repeat count"u8;
public static readonly @string ErrInvalidUTF8 = "invalid UTF-8"u8;
public static readonly @string ErrMissingBracket = "missing closing ]"u8;
public static readonly @string ErrMissingParen = "missing closing )"u8;
public static readonly @string ErrMissingRepeatArgument = "missing argument to repetition operator"u8;
public static readonly @string ErrTrailingBackslash = "trailing backslash at end of expression"u8;
public static readonly @string ErrUnexpectedParen = "unexpected )"u8;
public static readonly @string ErrNestingDepth = "expression nests too deeply"u8;
public static readonly @string ErrLarge = "expression too large"u8;

public static @string String(this ErrorCode e) {
    return ((@string)e);
}

[GoType("num:uint16")] partial struct Flags;

public static readonly Flags FoldCase = /* 1 << iota */ 1;            // case-insensitive match
public static readonly Flags Literal = 2;             // treat pattern as literal string
public static readonly Flags ClassNL = 4;             // allow character classes like [^a-z] and [[:space:]] to match newline
public static readonly Flags DotNL = 8;               // allow . to match newline
public static readonly Flags OneLine = 16;             // treat ^ and $ as only matching at beginning and end of text
public static readonly Flags NonGreedy = 32;           // make repetition operators default to non-greedy
public static readonly Flags PerlX = 64;               // allow Perl extensions
public static readonly Flags UnicodeGroups = 128;       // allow \p{Han}, \P{Han} for Unicode group and negation
public static readonly Flags WasDollar = 256;           // regexp OpEndText was $, not \z
public static readonly Flags Simple = 512;              // regexp contains no counted repetition
public static readonly Flags MatchNL = /* ClassNL | DotNL */ 12;
public static readonly Flags Perl = /* ClassNL | OneLine | PerlX | UnicodeGroups */ 212; // as close to Perl as possible
public static readonly Flags POSIX = 0;                                       // POSIX syntax

// Pseudo-ops for parsing stack.
internal static readonly Op opLeftParen = /* opPseudo + iota */ 128;

internal static readonly Op opVerticalBar = 129;

// maxHeight is the maximum height of a regexp parse tree.
// It is somewhat arbitrarily chosen, but the idea is to be large enough
// that no one will actually hit in real use but at the same time small enough
// that recursion on the Regexp tree will not hit the 1GB Go stack limit.
// The maximum amount of stack for a single recursive frame is probably
// closer to 1kB, so this could potentially be raised, but it seems unlikely
// that people have regexps nested even this deeply.
// We ran a test on Google's C++ code base and turned up only
// a single use case with depth > 100; it had depth 128.
// Using depth 1000 should be plenty of margin.
// As an optimization, we don't even bother calculating heights
// until we've allocated at least maxHeight Regexp structures.
internal static readonly UntypedInt maxHeight = 1000;

// maxSize is the maximum size of a compiled regexp in Insts.
// It too is somewhat arbitrarily chosen, but the idea is to be large enough
// to allow significant regexps while at the same time small enough that
// the compiled form will not take up too much memory.
// 128 MB is enough for a 3.3 million Inst structures, which roughly
// corresponds to a 3.3 MB regexp.
internal static readonly UntypedInt maxSize = /* 128 << 20 / instSize */ 3355443;

internal static readonly UntypedInt instSize = /* 5 * 8 */ 40; // byte, 2 uint32, slice is 5 64-bit words

// maxRunes is the maximum number of runes allowed in a regexp tree
// counting the runes in all the nodes.
// Ignoring character classes p.numRunes is always less than the length of the regexp.
// Character classes can make it much larger: each \pL adds 1292 runes.
// 128 MB is enough for 32M runes, which is over 26k \pL instances.
// Note that repetitions do not make copies of the rune slices,
// so \pL{1000} is only one rune slice, not 1000.
// We could keep a cache of character classes we've seen,
// so that all the \pL we see use the same rune list,
// but that doesn't remove the problem entirely:
// consider something like [\pL01234][\pL01235][\pL01236]...[\pL^&*()].
// And because the Rune slice is exposed directly in the Regexp,
// there is not an opportunity to change the representation to allow
// partial sharing between different character classes.
// So the limit is the best we can do.
internal static readonly UntypedInt maxRunes = /* 128 << 20 / runeSize */ 33554432;

internal static readonly UntypedInt runeSize = 4; // rune is int32

[GoType] partial struct parser {
    internal Flags flags;     // parse mode flags
    internal slice<ж<Regexp>> stack; // stack of parsed expressions
    internal ж<Regexp> free;
    internal nint numCap; // number of capturing groups seen
    internal @string wholeRegexp;
    internal slice<rune> tmpClass;       // temporary char class work space
    internal nint numRegexp;              // number of regexps allocated
    internal nint numRunes;              // number of runes in char classes
    internal int64 repeats;             // product of all repetitions seen
    internal map<ж<Regexp>, nint> height; // regexp height, for height limit check
    internal map<ж<Regexp>, int64> size; // regexp compiled size, for size limit check
}

[GoRecv] internal static ж<Regexp> newRegexp(this ref parser p, Op op) {
    var re = p.free;
    if (re != nil){
        p.free = (~re).Sub0[0];
        re.val = new Regexp(nil);
    } else {
        re = @new<Regexp>();
        p.numRegexp++;
    }
    re.val.Op = op;
    return re;
}

[GoRecv] internal static void reuse(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (p.height != default!) {
        delete(p.height, Ꮡre);
    }
    re.Sub0[0] = p.free;
    p.free = re;
}

[GoRecv] internal static void checkLimits(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (p.numRunes > maxRunes) {
        throw panic(ErrLarge);
    }
    p.checkSize(Ꮡre);
    p.checkHeight(Ꮡre);
}

[GoRecv] internal static void checkSize(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (p.size == default!) {
        // We haven't started tracking size yet.
        // Do a relatively cheap check to see if we need to start.
        // Maintain the product of all the repeats we've seen
        // and don't track if the total number of regexp nodes
        // we've seen times the repeat product is in budget.
        if (p.repeats == 0) {
            p.repeats = 1;
        }
        if (re.Op == OpRepeat) {
            nint n = re.Max;
            if (n == -1) {
                n = re.Min;
            }
            if (n <= 0) {
                n = 1;
            }
            if (((int64)n) > maxSize / p.repeats){
                p.repeats = maxSize;
            } else {
                p.repeats *= ((int64)n);
            }
        }
        if (((int64)p.numRegexp) < maxSize / p.repeats) {
            return;
        }
        // We need to start tracking size.
        // Make the map and belatedly populate it
        // with info about everything we've constructed so far.
        p.size = new map<ж<Regexp>, int64>();
        foreach (var (_, reΔ1) in p.stack) {
            p.checkSize(ᏑreΔ1);
        }
    }
    if (p.calcSize(Ꮡre, true) > maxSize) {
        throw panic(ErrLarge);
    }
}

[GoRecv] internal static int64 calcSize(this ref parser p, ж<Regexp> Ꮡre, bool force) {
    ref var re = ref Ꮡre.val;

    if (!force) {
        {
            var (sizeΔ1, ok) = p.size[re]; if (ok) {
                return sizeΔ1;
            }
        }
    }
    int64 size = default!;
    var exprᴛ1 = re.Op;
    if (exprᴛ1 == OpLiteral) {
        size = ((int64)len(re.Rune));
    }
    else if (exprᴛ1 == OpCapture || exprᴛ1 == OpStar) {
        size = 2 + p.calcSize(re.Sub[0], // star can be 1+ or 2+; assume 2 pessimistically
 false);
    }
    else if (exprᴛ1 == OpPlus || exprᴛ1 == OpQuest) {
        size = 1 + p.calcSize(re.Sub[0], false);
    }
    else if (exprᴛ1 == OpConcat) {
        foreach (var (_, sub) in re.Sub) {
            size += p.calcSize(sub, false);
        }
    }
    else if (exprᴛ1 == OpAlternate) {
        foreach (var (_, sub) in re.Sub) {
            size += p.calcSize(sub, false);
        }
        if (len(re.Sub) > 1) {
            size += ((int64)len(re.Sub)) - 1;
        }
    }
    else if (exprᴛ1 == OpRepeat) {
        var sub = p.calcSize(re.Sub[0], false);
        if (re.Max == -1) {
            if (re.Min == 0){
                size = 2 + sub;
            } else {
                // x*
                size = 1 + ((int64)re.Min) * sub;
            }
            // xxx+
            break;
        }
        size = ((int64)re.Max) * sub + ((int64)(re.Max - re.Min));
    }

    // x{2,5} = xx(x(x(x)?)?)?
    size = max(1, size);
    p.size[re] = size;
    return size;
}

[GoRecv] internal static void checkHeight(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (p.numRegexp < maxHeight) {
        return;
    }
    if (p.height == default!) {
        p.height = new map<ж<Regexp>, nint>();
        foreach (var (_, reΔ1) in p.stack) {
            p.checkHeight(ᏑreΔ1);
        }
    }
    if (p.calcHeight(Ꮡre, true) > maxHeight) {
        throw panic(ErrNestingDepth);
    }
}

[GoRecv] internal static nint calcHeight(this ref parser p, ж<Regexp> Ꮡre, bool force) {
    ref var re = ref Ꮡre.val;

    if (!force) {
        {
            nint hΔ1 = p.height[re];
            var ok = p.height[re]; if (ok) {
                return hΔ1;
            }
        }
    }
    nint h = 1;
    foreach (var (_, sub) in re.Sub) {
        nint hsub = p.calcHeight(sub, false);
        if (h < 1 + hsub) {
            h = 1 + hsub;
        }
    }
    p.height[re] = h;
    return h;
}

// Parse stack manipulation.

// push pushes the regexp re onto the parse stack and returns the regexp.
[GoRecv] internal static ж<Regexp> push(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    p.numRunes += len(re.Rune);
    if (re.Op == OpCharClass && len(re.Rune) == 2 && re.Rune[0] == re.Rune[1]){
        // Single rune.
        if (p.maybeConcat(re.Rune[0], (Flags)(p.flags & ~FoldCase))) {
            return default!;
        }
        re.Op = OpLiteral;
        re.Rune = re.Rune[..1];
        re.Flags = (Flags)(p.flags & ~FoldCase);
    } else 
    if (re.Op == OpCharClass && len(re.Rune) == 4 && re.Rune[0] == re.Rune[1] && re.Rune[2] == re.Rune[3] && unicode.SimpleFold(re.Rune[0]) == re.Rune[2] && unicode.SimpleFold(re.Rune[2]) == re.Rune[0] || re.Op == OpCharClass && len(re.Rune) == 2 && re.Rune[0] + 1 == re.Rune[1] && unicode.SimpleFold(re.Rune[0]) == re.Rune[1] && unicode.SimpleFold(re.Rune[1]) == re.Rune[0]){
        // Case-insensitive rune like [Aa] or [Δδ].
        if (p.maybeConcat(re.Rune[0], (Flags)(p.flags | FoldCase))) {
            return default!;
        }
        // Rewrite as (case-insensitive) literal.
        re.Op = OpLiteral;
        re.Rune = re.Rune[..1];
        re.Flags = (Flags)(p.flags | FoldCase);
    } else {
        // Incremental concatenation.
        p.maybeConcat(-1, 0);
    }
    p.stack = append(p.stack, Ꮡre);
    p.checkLimits(Ꮡre);
    return Ꮡre;
}

// maybeConcat implements incremental concatenation
// of literal runes into string nodes. The parser calls this
// before each push, so only the top fragment of the stack
// might need processing. Since this is called before a push,
// the topmost literal is no longer subject to operators like *
// (Otherwise ab* would turn into (ab)*.)
// If r >= 0 and there's a node left over, maybeConcat uses it
// to push r with the given flags.
// maybeConcat reports whether r was pushed.
[GoRecv] internal static bool maybeConcat(this ref parser p, rune r, Flags flags) {
    nint n = len(p.stack);
    if (n < 2) {
        return false;
    }
    var re1 = p.stack[n - 1];
    var re2 = p.stack[n - 2];
    if ((~re1).Op != OpLiteral || (~re2).Op != OpLiteral || (Flags)((~re1).Flags & FoldCase) != (Flags)((~re2).Flags & FoldCase)) {
        return false;
    }
    // Push re1 into re2.
    re2.val.Rune = append((~re2).Rune, (~re1).Rune.ꓸꓸꓸ);
    // Reuse re1 if possible.
    if (r >= 0) {
        re1.val.Rune = (~re1).Rune0[..1];
        (~re1).Rune[0] = r;
        re1.val.Flags = flags;
        return true;
    }
    p.stack = p.stack[..(int)(n - 1)];
    p.reuse(re1);
    return false;
}

// did not push r

// literal pushes a literal regexp for the rune r on the stack.
[GoRecv] internal static void literal(this ref parser p, rune r) {
    var re = p.newRegexp(OpLiteral);
    re.val.Flags = p.flags;
    if ((Flags)(p.flags & FoldCase) != 0) {
        r = minFoldRune(r);
    }
    (~re).Rune0[0] = r;
    re.val.Rune = (~re).Rune0[..1];
    p.push(re);
}

// minFoldRune returns the minimum rune fold-equivalent to r.
internal static rune minFoldRune(rune r) {
    if (r < minFold || r > maxFold) {
        return r;
    }
    var m = r;
    var r0 = r;
    for (r = unicode.SimpleFold(r); r != r0; r = unicode.SimpleFold(r)) {
        m = min(m, r);
    }
    return m;
}

// op pushes a regexp with the given op onto the stack
// and returns that regexp.
[GoRecv] internal static ж<Regexp> op(this ref parser p, Op op) {
    var re = p.newRegexp(op);
    re.val.Flags = p.flags;
    return p.push(re);
}

// repeat replaces the top stack element with itself repeated according to op, min, max.
// before is the regexp suffix starting at the repetition operator.
// after is the regexp suffix following after the repetition operator.
// repeat returns an updated 'after' and an error, if any.
[GoRecv] internal static (@string, error) repeat(this ref parser p, Op op, nint min, nint max, @string before, @string after, @string lastRepeat) {
    var flags = p.flags;
    if ((Flags)(p.flags & PerlX) != 0) {
        if (len(after) > 0 && after[0] == (rune)'?') {
            after = after[1..];
            flags ^= (Flags)(NonGreedy);
        }
        if (lastRepeat != ""u8) {
            // In Perl it is not allowed to stack repetition operators:
            // a** is a syntax error, not a doubled star, and a++ means
            // something else entirely, which we don't support!
            return ("", new ΔError(ErrInvalidRepeatOp, lastRepeat[..(int)(len(lastRepeat) - len(after))]));
        }
    }
    nint n = len(p.stack);
    if (n == 0) {
        return ("", new ΔError(ErrMissingRepeatArgument, before[..(int)(len(before) - len(after))]));
    }
    var sub = p.stack[n - 1];
    if ((~sub).Op >= opPseudo) {
        return ("", new ΔError(ErrMissingRepeatArgument, before[..(int)(len(before) - len(after))]));
    }
    var re = p.newRegexp(op);
    re.val.Min = min;
    re.val.Max = max;
    re.val.Flags = flags;
    re.val.Sub = (~re).Sub0[..1];
    (~re).Sub[0] = sub;
    p.stack[n - 1] = re;
    p.checkLimits(re);
    if (op == OpRepeat && (min >= 2 || max >= 2) && !repeatIsValid(re, 1000)) {
        return ("", new ΔError(ErrInvalidRepeatSize, before[..(int)(len(before) - len(after))]));
    }
    return (after, default!);
}

// repeatIsValid reports whether the repetition re is valid.
// Valid means that the combination of the top-level repetition
// and any inner repetitions does not exceed n copies of the
// innermost thing.
// This function rewalks the regexp tree and is called for every repetition,
// so we have to worry about inducing quadratic behavior in the parser.
// We avoid this by only calling repeatIsValid when min or max >= 2.
// In that case the depth of any >= 2 nesting can only get to 9 without
// triggering a parse error, so each subtree can only be rewalked 9 times.
internal static bool repeatIsValid(ж<Regexp> Ꮡre, nint n) {
    ref var re = ref Ꮡre.val;

    if (re.Op == OpRepeat) {
        nint m = re.Max;
        if (m == 0) {
            return true;
        }
        if (m < 0) {
            m = re.Min;
        }
        if (m > n) {
            return false;
        }
        if (m > 0) {
            n /= m;
        }
    }
    foreach (var (_, sub) in re.Sub) {
        if (!repeatIsValid(sub, n)) {
            return false;
        }
    }
    return true;
}

// concat replaces the top of the stack (above the topmost '|' or '(') with its concatenation.
[GoRecv] internal static ж<Regexp> concat(this ref parser p) {
    p.maybeConcat(-1, 0);
    // Scan down to find pseudo-operator | or (.
    nint i = len(p.stack);
    while (i > 0 && p.stack[i - 1].Op < opPseudo) {
        i--;
    }
    var subs = p.stack[(int)(i)..];
    p.stack = p.stack[..(int)(i)];
    // Empty concatenation is special case.
    if (len(subs) == 0) {
        return p.push(p.newRegexp(OpEmptyMatch));
    }
    return p.push(p.collapse(subs, OpConcat));
}

// alternate replaces the top of the stack (above the topmost '(') with its alternation.
[GoRecv] internal static ж<Regexp> alternate(this ref parser p) {
    // Scan down to find pseudo-operator (.
    // There are no | above (.
    nint i = len(p.stack);
    while (i > 0 && p.stack[i - 1].Op < opPseudo) {
        i--;
    }
    var subs = p.stack[(int)(i)..];
    p.stack = p.stack[..(int)(i)];
    // Make sure top class is clean.
    // All the others already are (see swapVerticalBar).
    if (len(subs) > 0) {
        cleanAlt(subs[len(subs) - 1]);
    }
    // Empty alternate is special case
    // (shouldn't happen but easy to handle).
    if (len(subs) == 0) {
        return p.push(p.newRegexp(OpNoMatch));
    }
    return p.push(p.collapse(subs, OpAlternate));
}

// cleanAlt cleans re for eventual inclusion in an alternation.
internal static void cleanAlt(ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    var exprᴛ1 = re.Op;
    if (exprᴛ1 == OpCharClass) {
        re.Rune = cleanClass(Ꮡ(re.Rune));
        if (len(re.Rune) == 2 && re.Rune[0] == 0 && re.Rune[1] == unicode.MaxRune) {
            re.Rune = default!;
            re.Op = OpAnyChar;
            return;
        }
        if (len(re.Rune) == 4 && re.Rune[0] == 0 && re.Rune[1] == (rune)'\n' - 1 && re.Rune[2] == (rune)'\n' + 1 && re.Rune[3] == unicode.MaxRune) {
            re.Rune = default!;
            re.Op = OpAnyCharNotNL;
            return;
        }
        if (cap(re.Rune) - len(re.Rune) > 100) {
            // re.Rune will not grow any more.
            // Make a copy or inline to reclaim storage.
            re.Rune = append(re.Rune0[..0], re.Rune.ꓸꓸꓸ);
        }
    }

}

// collapse returns the result of applying op to sub.
// If sub contains op nodes, they all get hoisted up
// so that there is never a concat of a concat or an
// alternate of an alternate.
[GoRecv] internal static ж<Regexp> collapse(this ref parser p, slice<ж<Regexp>> subs, Op op) {
    if (len(subs) == 1) {
        return Ꮡsubs[0];
    }
    var re = p.newRegexp(op);
    re.val.Sub = (~re).Sub0[..0];
    foreach (var (_, sub) in subs) {
        if ((~sub).Op == op){
            re.val.Sub = append((~re).Sub, (~sub).Sub.ꓸꓸꓸ);
            p.reuse(sub);
        } else {
            re.val.Sub = append((~re).Sub, sub);
        }
    }
    if (op == OpAlternate) {
        re.val.Sub = p.factor((~re).Sub);
        if (len((~re).Sub) == 1) {
            var old = re;
            re = (~re).Sub[0];
            p.reuse(old);
        }
    }
    return re;
}

// factor factors common prefixes from the alternation list sub.
// It returns a replacement list that reuses the same storage and
// frees (passes to p.reuse) any removed *Regexps.
//
// For example,
//
//	ABC|ABD|AEF|BCX|BCY
//
// simplifies by literal prefix extraction to
//
//	A(B(C|D)|EF)|BC(X|Y)
//
// which simplifies by character class introduction to
//
//	A(B[CD]|EF)|BC[XY]
[GoRecv] internal static slice<ж<Regexp>> factor(this ref parser p, slice<ж<Regexp>> sub) {
    if (len(sub) < 2) {
        return sub;
    }
    // Round 1: Factor out common literal prefixes.
    slice<rune> str = default!;
    Flags strflags = default!;
    ref var start = ref heap<nint>(out var Ꮡstart);
    start = 0;
    var @out = sub[..0];
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i <= len(sub); i++) {
        // Invariant: the Regexps that were in sub[0:start] have been
        // used or marked for reuse, and the slice space has been reused
        // for out (len(out) <= start).
        //
        // Invariant: sub[start:i] consists of regexps that all begin
        // with str as modified by strflags.
        slice<rune> istr = default!;
        Flags iflags = default!;
        if (i < len(sub)) {
            (istr, iflags) = p.leadingString(sub[i]);
            if (iflags == strflags) {
                nint same = 0;
                while (same < len(str) && same < len(istr) && str[same] == istr[same]) {
                    same++;
                }
                if (same > 0) {
                    // Matches at least one rune in current range.
                    // Keep going around.
                    str = str[..(int)(same)];
                    continue;
                }
            }
        }
        // Found end of a run with common leading literal string:
        // sub[start:i] all begin with str[0:len(str)], but sub[i]
        // does not even begin with str[0].
        //
        // Factor out common string and append factored expression to out.
        if (i == start){
        } else 
        if (i == start + 1){
            // Nothing to do - run of length 0.
            // Just one: don't bother factoring.
            @out = append(@out, sub[start]);
        } else {
            // Construct factored form: prefix(suffix1|suffix2|...)
            var prefix = p.newRegexp(OpLiteral);
            prefix.val.Flags = strflags;
            prefix.val.Rune = append((~prefix).Rune[..0], str.ꓸꓸꓸ);
            ref var j = ref heap<nint>(out var Ꮡj);
            for (j = start; j < i; j++) {
                sub[j] = p.removeLeadingString(sub[j], len(str));
                p.checkLimits(sub[j]);
            }
            var suffix = p.collapse(sub[(int)(start)..(int)(i)], OpAlternate);
            // recurse
            var re = p.newRegexp(OpConcat);
            re.val.Sub = append((~re).Sub[..0], prefix, suffix);
            @out = append(@out, re);
        }
        // Prepare for next iteration.
        start = i;
        str = istr;
        strflags = iflags;
    }
    sub = @out;
    // Round 2: Factor out common simple prefixes,
    // just the first piece of each concatenation.
    // This will be good enough a lot of the time.
    //
    // Complex subexpressions (e.g. involving quantifiers)
    // are not safe to factor because that collapses their
    // distinct paths through the automaton, which affects
    // correctness in some cases.
    start = 0;
    @out = sub[..0];
    ж<Regexp> first = default!;
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i <= len(sub); i++) {
        // Invariant: the Regexps that were in sub[0:start] have been
        // used or marked for reuse, and the slice space has been reused
        // for out (len(out) <= start).
        //
        // Invariant: sub[start:i] consists of regexps that all begin with ifirst.
        ж<Regexp> ifirst = default!;
        if (i < len(sub)) {
            ifirst = p.leadingRegexp(sub[i]);
            if (first != nil && first.Equal(ifirst) && (isCharClass(first) || ((~first).Op == OpRepeat && (~first).Min == (~first).Max && isCharClass((~first).Sub[0])))) {
                // first must be a character class OR a fixed repeat of a character class.
                continue;
            }
        }
        // Found end of a run with common leading regexp:
        // sub[start:i] all begin with first but sub[i] does not.
        //
        // Factor out common regexp and append factored expression to out.
        if (i == start){
        } else 
        if (i == start + 1){
            // Nothing to do - run of length 0.
            // Just one: don't bother factoring.
            @out = append(@out, sub[start]);
        } else {
            // Construct factored form: prefix(suffix1|suffix2|...)
            var prefix = first;
            ref var j = ref heap<nint>(out var Ꮡj);
            for (j = start; j < i; j++) {
                var reuse = j != start;
                // prefix came from sub[start]
                sub[j] = p.removeLeadingRegexp(sub[j], reuse);
                p.checkLimits(sub[j]);
            }
            var suffix = p.collapse(sub[(int)(start)..(int)(i)], OpAlternate);
            // recurse
            var re = p.newRegexp(OpConcat);
            re.val.Sub = append((~re).Sub[..0], prefix, suffix);
            @out = append(@out, re);
        }
        // Prepare for next iteration.
        start = i;
        first = ifirst;
    }
    sub = @out;
    // Round 3: Collapse runs of single literals into character classes.
    start = 0;
    @out = sub[..0];
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i <= len(sub); i++) {
        // Invariant: the Regexps that were in sub[0:start] have been
        // used or marked for reuse, and the slice space has been reused
        // for out (len(out) <= start).
        //
        // Invariant: sub[start:i] consists of regexps that are either
        // literal runes or character classes.
        if (i < len(sub) && isCharClass(sub[i])) {
            continue;
        }
        // sub[i] is not a char or char class;
        // emit char class for sub[start:i]...
        if (i == start){
        } else 
        if (i == start + 1){
            // Nothing to do - run of length 0.
            @out = append(@out, sub[start]);
        } else {
            // Make new char class.
            // Start with most complex regexp in sub[start].
            nint max = start;
            for (nint j = start + 1; j < i; j++) {
                if (sub[max].Op < sub[j].Op || sub[max].Op == sub[j].Op && len(sub[max].Rune) < len(sub[j].Rune)) {
                    max = j;
                }
            }
            (sub[start], sub[max]) = (sub[max], sub[start]);
            ref var j = ref heap<nint>(out var Ꮡj);
            for (j = start + 1; j < i; j++) {
                mergeCharClass(sub[start], sub[j]);
                p.reuse(sub[j]);
            }
            cleanAlt(sub[start]);
            @out = append(@out, sub[start]);
        }
        // ... and then emit sub[i].
        if (i < len(sub)) {
            @out = append(@out, sub[i]);
        }
        start = i + 1;
    }
    sub = @out;
    // Round 4: Collapse runs of empty matches into a single empty match.
    start = 0;
    @out = sub[..0];
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in sub) {
        if (i + 1 < len(sub) && sub[i].Op == OpEmptyMatch && sub[i + 1].Op == OpEmptyMatch) {
            continue;
        }
        @out = append(@out, sub[i]);
    }
    sub = @out;
    return sub;
}

// leadingString returns the leading literal string that re begins with.
// The string refers to storage in re or its children.
[GoRecv] internal static (slice<rune>, Flags) leadingString(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (re.Op == OpConcat && len(re.Sub) > 0) {
        re = re.Sub[0];
    }
    if (re.Op != OpLiteral) {
        return (default!, 0);
    }
    return (re.Rune, (Flags)(re.Flags & FoldCase));
}

// removeLeadingString removes the first n leading runes
// from the beginning of re. It returns the replacement for re.
[GoRecv] internal static ж<Regexp> removeLeadingString(this ref parser p, ж<Regexp> Ꮡre, nint n) {
    ref var re = ref Ꮡre.val;

    if (re.Op == OpConcat && len(re.Sub) > 0) {
        // Removing a leading string in a concatenation
        // might simplify the concatenation.
        var sub = re.Sub[0];
        sub = p.removeLeadingString(sub, n);
        re.Sub[0] = sub;
        if ((~sub).Op == OpEmptyMatch) {
            p.reuse(sub);
            switch (len(re.Sub)) {
            case 0 or 1: {
                re.Op = OpEmptyMatch;
                re.Sub = default!;
                break;
            }
            case 2: {
                var old = re;
                re = re.Sub[1];
                p.reuse(old);
                break;
            }
            default: {
                copy(re.Sub, // Impossible but handle.
 re.Sub[1..]);
                re.Sub = re.Sub[..(int)(len(re.Sub) - 1)];
                break;
            }}

        }
        return Ꮡre;
    }
    if (re.Op == OpLiteral) {
        re.Rune = re.Rune[..(int)(copy(re.Rune, re.Rune[(int)(n)..]))];
        if (len(re.Rune) == 0) {
            re.Op = OpEmptyMatch;
        }
    }
    return Ꮡre;
}

// leadingRegexp returns the leading regexp that re begins with.
// The regexp refers to storage in re or its children.
[GoRecv] internal static ж<Regexp> leadingRegexp(this ref parser p, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    if (re.Op == OpEmptyMatch) {
        return default!;
    }
    if (re.Op == OpConcat && len(re.Sub) > 0) {
        var sub = re.Sub[0];
        if ((~sub).Op == OpEmptyMatch) {
            return default!;
        }
        return sub;
    }
    return Ꮡre;
}

// removeLeadingRegexp removes the leading regexp in re.
// It returns the replacement for re.
// If reuse is true, it passes the removed regexp (if no longer needed) to p.reuse.
[GoRecv] internal static ж<Regexp> removeLeadingRegexp(this ref parser p, ж<Regexp> Ꮡre, bool reuse) {
    ref var re = ref Ꮡre.val;

    if (re.Op == OpConcat && len(re.Sub) > 0) {
        if (reuse) {
            p.reuse(re.Sub[0]);
        }
        re.Sub = re.Sub[..(int)(copy(re.Sub, re.Sub[1..]))];
        switch (len(re.Sub)) {
        case 0: {
            re.Op = OpEmptyMatch;
            re.Sub = default!;
            break;
        }
        case 1: {
            var old = re;
            re = re.Sub[0];
            p.reuse(old);
            break;
        }}

        return Ꮡre;
    }
    if (reuse) {
        p.reuse(Ꮡre);
    }
    return p.newRegexp(OpEmptyMatch);
}

internal static ж<Regexp> literalRegexp(@string s, Flags flags) {
    var re = Ꮡ(new Regexp(Op: OpLiteral));
    re.val.Flags = flags;
    re.val.Rune = (~re).Rune0[..0];
    // use local storage for small strings
    foreach (var (_, c) in s) {
        if (len((~re).Rune) >= cap((~re).Rune)) {
            // string is too long to fit in Rune0.  let Go handle it
            re.val.Rune = slice<rune>(s);
            break;
        }
        re.val.Rune = append((~re).Rune, c);
    }
    return re;
}

// Parsing.

// Parse parses a regular expression string s, controlled by the specified
// Flags, and returns a regular expression parse tree. The syntax is
// described in the top-level comment.
public static (ж<Regexp>, error) Parse(@string s, Flags flags) {
    return parse(s, flags);
}

internal static (ж<Regexp> _, error err) parse(@string s, Flags flags) => func((defer, _) => {
    error err = default!;

    defer(() => {
        {
            var r = recover();
            var exprᴛ1 = r;
            { /* default: */
                throw panic(r);
            }
            else if (exprᴛ1 == default!) {
            }
            else if (exprᴛ1 == ErrLarge) {
                Ꮡerr = new ΔError( // ok
 // too big
Code: ErrLarge, Expr: s); err = ref Ꮡerr.val;
            }
            else if (exprᴛ1 == ErrNestingDepth) {
                Ꮡerr = new ΔError(Code: ErrNestingDepth, Expr: s); err = ref Ꮡerr.val;
            }
        }

    });
    if ((Flags)(flags & Literal) != 0) {
        // Trivial parser for literal string.
        {
            var errΔ1 = checkUTF8(s); if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
        }
        return (literalRegexp(s, flags), default!);
    }
    // Otherwise, must do real work.
    parser p = default!;
    
    rune c = default!;
    
    Op op = default!;
    
    @string lastRepeat = default!;
    p.flags = flags;
    p.wholeRegexp = s;
    @string t = s;
    while (t != ""u8) {
        @string repeat = ""u8;
BigSwitch:
        switch (t[0]) {
        default: {
            {
                (c, t, err) = nextRune(t); if (err != default!) {
                    return (default!, err);
                }
            }
            p.literal(c);
            break;
        }
        case (rune)'(': {
            if ((Flags)(p.flags & PerlX) != 0 && len(t) >= 2 && t[1] == (rune)'?') {
                // Flag changes and non-capturing groups.
                {
                    (t, err) = p.parsePerlFlags(t); if (err != default!) {
                        return (default!, err);
                    }
                }
                break;
            }
            p.numCap++;
            p.op(opLeftParen).val.Cap = p.numCap;
            t = t[1..];
            break;
        }
        case (rune)'|': {
            p.parseVerticalBar();
            t = t[1..];
            break;
        }
        case (rune)')': {
            {
                err = p.parseRightParen(); if (err != default!) {
                    return (default!, err);
                }
            }
            t = t[1..];
            break;
        }
        case (rune)'^': {
            if ((Flags)(p.flags & OneLine) != 0){
                p.op(OpBeginText);
            } else {
                p.op(OpBeginLine);
            }
            t = t[1..];
            break;
        }
        case (rune)'$': {
            if ((Flags)(p.flags & OneLine) != 0){
                p.op(OpEndText).val.Flags |= WasDollar;
            } else {
                p.op(OpEndLine);
            }
            t = t[1..];
            break;
        }
        case (rune)'.': {
            if ((Flags)(p.flags & DotNL) != 0){
                p.op(OpAnyChar);
            } else {
                p.op(OpAnyCharNotNL);
            }
            t = t[1..];
            break;
        }
        case (rune)'[': {
            {
                (t, err) = p.parseClass(t); if (err != default!) {
                    return (default!, err);
                }
            }
            break;
        }
        case (rune)'*' or (rune)'+' or (rune)'?': {
            @string before = t;
            switch (t[0]) {
            case (rune)'*': {
                op = OpStar;
                break;
            }
            case (rune)'+': {
                op = OpPlus;
                break;
            }
            case (rune)'?': {
                op = OpQuest;
                break;
            }}

            @string afterΔ1 = t[1..];
            {
                (afterΔ1, err) = p.repeat(op, 0, 0, before, after, lastRepeat); if (err != default!) {
                    return (default!, err);
                }
            }
            repeat = before;
            t = afterΔ1;
            break;
        }
        case (rune)'{': {
            op = OpRepeat;
            @string before = t;
            var (min, max, after, ok) = p.parseRepeat(t);
            if (!ok) {
                // If the repeat cannot be parsed, { is a literal.
                p.literal((rune)'{');
                t = t[1..];
                break;
            }
            if (min < 0 || min > 1000 || max > 1000 || max >= 0 && min > max) {
                // Numbers were too big, or max is present and min > max.
                return (default!, new ΔError(ErrInvalidRepeatSize, before[..(int)(len(before) - len(after))]));
            }
            {
                (after, err) = p.repeat(op, min, max, before, after, lastRepeat); if (err != default!) {
                    return (default!, err);
                }
            }
            repeat = before;
            t = after;
            break;
        }
        case (rune)'\\': {
            if ((Flags)(p.flags & PerlX) != 0 && len(t) >= 2) {
                switch (t[1]) {
                case (rune)'A': {
                    p.op(OpBeginText);
                    t = t[2..];
                    goto break_BigSwitch;
                    break;
                }
                case (rune)'b': {
                    p.op(OpWordBoundary);
                    t = t[2..];
                    goto break_BigSwitch;
                    break;
                }
                case (rune)'B': {
                    p.op(OpNoWordBoundary);
                    t = t[2..];
                    goto break_BigSwitch;
                    break;
                }
                case (rune)'C': {
                    return (default!, new ΔError( // any byte; not supported
ErrInvalidEscape, t[..2]));
                }
                case (rune)'Q': {
                    // \Q ... \E: the ... is always literals
                    @string lit = default!;
                    (lit, t, _) = strings.Cut(t[2..], @"\E"u8);
                    while (lit != ""u8) {
                        var (cΔ4, rest, errΔ6) = nextRune(lit);
                        if (errΔ6 != default!) {
                            return (default!, errΔ6);
                        }
                        p.literal(cΔ4);
                        lit = rest;
                    }
                    goto break_BigSwitch;
                    break;
                }
                case (rune)'z': {
                    p.op(OpEndText);
                    t = t[2..];
                    goto break_BigSwitch;
                    break;
                }}

            }
            var re = p.newRegexp(OpCharClass);
            re.val.Flags = p.flags;
            if (len(t) >= 2 && (t[1] == (rune)'p' || t[1] == (rune)'P')) {
                // Look for Unicode character group like \p{Han}
                var (r, rest, errΔ7) = p.parseUnicodeClass(t, (~re).Rune0[..0]);
                if (errΔ7 != default!) {
                    return (default!, errΔ7);
                }
                if (r != default!) {
                    re.val.Rune = r;
                    t = rest;
                    p.push(re);
                    goto break_BigSwitch;
                }
            }
            {
                var (r, rest) = p.parsePerlClassEscape(t, // Perl character class escape.
 (~re).Rune0[..0]); if (r != default!) {
                    re.val.Rune = r;
                    t = rest;
                    p.push(re);
                    goto break_BigSwitch;
                }
            }
            p.reuse(re);
            {
                (c, t, err) = p.parseEscape(t); if (err != default!) {
                    // Ordinary single-character escape.
                    return (default!, err);
                }
            }
            p.literal(c);
            break;
        }}

        lastRepeat = repeat;
    }
    p.concat();
    if (p.swapVerticalBar()) {
        // pop vertical bar
        p.stack = p.stack[..(int)(len(p.stack) - 1)];
    }
    p.alternate();
    nint n = len(p.stack);
    if (n != 1) {
        return (default!, new ΔError(ErrMissingParen, s));
    }
    return (p.stack[0], default!);
});

// parseRepeat parses {min} (max=min) or {min,} (max=-1) or {min,max}.
// If s is not of that form, it returns ok == false.
// If s has the right form but the values are too big, it returns min == -1, ok == true.
[GoRecv] internal static (nint min, nint max, @string rest, bool ok) parseRepeat(this ref parser p, @string s) {
    nint min = default!;
    nint max = default!;
    @string rest = default!;
    bool ok = default!;

    if (s == ""u8 || s[0] != (rune)'{') {
        return (min, max, rest, ok);
    }
    s = s[1..];
    bool ok1 = default!;
    {
        (min, s, ok1) = p.parseInt(s); if (!ok1) {
            return (min, max, rest, ok);
        }
    }
    if (s == ""u8) {
        return (min, max, rest, ok);
    }
    if (s[0] != (rune)','){
        max = min;
    } else {
        s = s[1..];
        if (s == ""u8) {
            return (min, max, rest, ok);
        }
        if (s[0] == (rune)'}'){
            max = -1;
        } else 
        {
            (max, s, ok1) = p.parseInt(s); if (!ok1){
                return (min, max, rest, ok);
            } else 
            if (max < 0) {
                // parseInt found too big a number
                min = -1;
            }
        }
    }
    if (s == ""u8 || s[0] != (rune)'}') {
        return (min, max, rest, ok);
    }
    rest = s[1..];
    ok = true;
    return (min, max, rest, ok);
}

// parsePerlFlags parses a Perl flag setting or non-capturing group or both,
// like (?i) or (?: or (?i:.  It removes the prefix from s and updates the parse state.
// The caller must have ensured that s begins with "(?".
[GoRecv] internal static (@string rest, error err) parsePerlFlags(this ref parser p, @string s) {
    @string rest = default!;
    error err = default!;

    @string t = s;
    // Check for named captures, first introduced in Python's regexp library.
    // As usual, there are three slightly different syntaxes:
    //
    //   (?P<name>expr)   the original, introduced by Python
    //   (?<name>expr)    the .NET alteration, adopted by Perl 5.10
    //   (?'name'expr)    another .NET alteration, adopted by Perl 5.10
    //
    // Perl 5.10 gave in and implemented the Python version too,
    // but they claim that the last two are the preferred forms.
    // PCRE and languages based on it (specifically, PHP and Ruby)
    // support all three as well. EcmaScript 4 uses only the Python form.
    //
    // In both the open source world (via Code Search) and the
    // Google source tree, (?P<expr>name) and (?<expr>name) are the
    // dominant forms of named captures and both are supported.
    var startsWithP = len(t) > 4 && t[2] == (rune)'P' && t[3] == (rune)'<';
    var startsWithName = len(t) > 3 && t[2] == (rune)'<';
    if (startsWithP || startsWithName) {
        // position of expr start
        nint exprStartPos = 4;
        if (startsWithName) {
            exprStartPos = 3;
        }
        // Pull out name.
        nint end = strings.IndexRune(t, (rune)'>');
        if (end < 0) {
            {
                err = checkUTF8(t); if (err != default!) {
                    return ("", err);
                }
            }
            return ("", new ΔError(ErrInvalidNamedCapture, s));
        }
        @string capture = t[..(int)(end + 1)];
        // "(?P<name>" or "(?<name>"
        @string name = t[(int)(exprStartPos)..(int)(end)];
        // "name"
        {
            err = checkUTF8(name); if (err != default!) {
                return ("", err);
            }
        }
        if (!isValidCaptureName(name)) {
            return ("", new ΔError(ErrInvalidNamedCapture, capture));
        }
        // Like ordinary capture, but named.
        p.numCap++;
        var re = p.op(opLeftParen);
        re.val.Cap = p.numCap;
        re.val.Name = name;
        return (t[(int)(end + 1)..], default!);
    }
    // Non-capturing group. Might also twiddle Perl flags.
    rune c = default!;
    t = t[2..];
    // skip (?
    var flags = p.flags;
    nint sign = +1;
    var sawFlag = false;
Loop:
    while (t != ""u8) {
        {
            (c, t, err) = nextRune(t); if (err != default!) {
                return ("", err);
            }
        }
        switch (c) {
        default: {
            goto break_Loop;
            break;
        }
        case (rune)'i': {
            flags |= (Flags)(FoldCase);
            sawFlag = true;
            break;
        }
        case (rune)'m': {
            flags &= ~(Flags)(OneLine);
            sawFlag = true;
            break;
        }
        case (rune)'s': {
            flags |= (Flags)(DotNL);
            sawFlag = true;
            break;
        }
        case (rune)'U': {
            flags |= (Flags)(NonGreedy);
            sawFlag = true;
            break;
        }
        case (rune)'-': {
            if (sign < 0) {
                // Flags.
                // Switch to negation.
                goto break_Loop;
            }
            sign = -1;
            flags = ~flags;
            sawFlag = false;
            break;
        }
        case (rune)':' or (rune)')': {
            if (sign < 0) {
                // Invert flags so that | above turn into &^ and vice versa.
                // We'll invert flags again before using it below.
                // End of flags, starting group or not.
                if (!sawFlag) {
                    goto break_Loop;
                }
                flags = ~flags;
            }
            if (c == (rune)':') {
                // Open new group
                p.op(opLeftParen);
            }
            p.flags = flags;
            return (t, default!);
        }}

continue_Loop:;
    }
break_Loop:;
    return ("", new ΔError(ErrInvalidPerlOp, s[..(int)(len(s) - len(t))]));
}

// isValidCaptureName reports whether name
// is a valid capture name: [A-Za-z0-9_]+.
// PCRE limits names to 32 bytes.
// Python rejects names starting with digits.
// We don't enforce either of those.
internal static bool isValidCaptureName(@string name) {
    if (name == ""u8) {
        return false;
    }
    foreach (var (_, c) in name) {
        if (c != (rune)'_' && !isalnum(c)) {
            return false;
        }
    }
    return true;
}

// parseInt parses a decimal integer.
[GoRecv] internal static (nint n, @string rest, bool ok) parseInt(this ref parser p, @string s) {
    nint n = default!;
    @string rest = default!;
    bool ok = default!;

    if (s == ""u8 || s[0] < (rune)'0' || (rune)'9' < s[0]) {
        return (n, rest, ok);
    }
    // Disallow leading zeros.
    if (len(s) >= 2 && s[0] == (rune)'0' && (rune)'0' <= s[1] && s[1] <= (rune)'9') {
        return (n, rest, ok);
    }
    @string t = s;
    while (s != ""u8 && (rune)'0' <= s[0] && s[0] <= (rune)'9') {
        s = s[1..];
    }
    rest = s;
    ok = true;
    // Have digits, compute value.
    t = t[..(int)(len(t) - len(s))];
    for (nint i = 0; i < len(t); i++) {
        // Avoid overflow.
        if (n >= 1e8F) {
            n = -1;
            break;
        }
        n = n * 10 + ((nint)t[i]) - (rune)'0';
    }
    return (n, rest, ok);
}

// can this be represented as a character class?
// single-rune literal string, char class, ., and .|\n.
internal static bool isCharClass(ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    return re.Op == OpLiteral && len(re.Rune) == 1 || re.Op == OpCharClass || re.Op == OpAnyCharNotNL || re.Op == OpAnyChar;
}

// does re match r?
internal static bool matchRune(ж<Regexp> Ꮡre, rune r) {
    ref var re = ref Ꮡre.val;

    var exprᴛ1 = re.Op;
    if (exprᴛ1 == OpLiteral) {
        return len(re.Rune) == 1 && re.Rune[0] == r;
    }
    if (exprᴛ1 == OpCharClass) {
        for (nint i = 0; i < len(re.Rune); i += 2) {
            if (re.Rune[i] <= r && r <= re.Rune[i + 1]) {
                return true;
            }
        }
        return false;
    }
    if (exprᴛ1 == OpAnyCharNotNL) {
        return r != (rune)'\n';
    }
    if (exprᴛ1 == OpAnyChar) {
        return true;
    }

    return false;
}

// parseVerticalBar handles a | in the input.
[GoRecv] internal static void parseVerticalBar(this ref parser p) {
    p.concat();
    // The concatenation we just parsed is on top of the stack.
    // If it sits above an opVerticalBar, swap it below
    // (things below an opVerticalBar become an alternation).
    // Otherwise, push a new vertical bar.
    if (!p.swapVerticalBar()) {
        p.op(opVerticalBar);
    }
}

// mergeCharClass makes dst = dst|src.
// The caller must ensure that dst.Op >= src.Op,
// to reduce the amount of copying.
internal static void mergeCharClass(ж<Regexp> Ꮡdst, ж<Regexp> Ꮡsrc) {
    ref var dst = ref Ꮡdst.val;
    ref var src = ref Ꮡsrc.val;

    var exprᴛ1 = dst.Op;
    if (exprᴛ1 == OpAnyChar) {
    }
    else if (exprᴛ1 == OpAnyCharNotNL) {
        if (matchRune(Ꮡsrc, // src doesn't add anything.
 // src might add \n
 (rune)'\n')) {
            dst.Op = OpAnyChar;
        }
    }
    else if (exprᴛ1 == OpCharClass) {
        if (src.Op == OpLiteral){
            // src is simpler, so either literal or char class
            dst.Rune = appendLiteral(dst.Rune, src.Rune[0], src.Flags);
        } else {
            dst.Rune = appendClass(dst.Rune, src.Rune);
        }
    }
    else if (exprᴛ1 == OpLiteral) {
        if (src.Rune[0] == dst.Rune[0] && src.Flags == dst.Flags) {
            // both literal
            break;
        }
        dst.Op = OpCharClass;
        dst.Rune = appendLiteral(dst.Rune[..0], dst.Rune[0], dst.Flags);
        dst.Rune = appendLiteral(dst.Rune, src.Rune[0], src.Flags);
    }

}

// If the top of the stack is an element followed by an opVerticalBar
// swapVerticalBar swaps the two and returns true.
// Otherwise it returns false.
[GoRecv] internal static bool swapVerticalBar(this ref parser p) {
    // If above and below vertical bar are literal or char class,
    // can merge into a single char class.
    ref var n = ref heap<nint>(out var Ꮡn);
    n = len(p.stack);
    if (n >= 3 && p.stack[n - 2].Op == opVerticalBar && isCharClass(p.stack[n - 1]) && isCharClass(p.stack[n - 3])) {
        var re1 = p.stack[n - 1];
        var re3 = p.stack[n - 3];
        // Make re3 the more complex of the two.
        if ((~re1).Op > (~re3).Op) {
            (re1, re3) = (re3, re1);
            p.stack[n - 3] = re3;
        }
        mergeCharClass(re3, re1);
        p.reuse(re1);
        p.stack = p.stack[..(int)(n - 1)];
        return true;
    }
    if (n >= 2) {
        var re1 = p.stack[n - 1];
        var re2 = p.stack[n - 2];
        if ((~re2).Op == opVerticalBar) {
            if (n >= 3) {
                // Now out of reach.
                // Clean opportunistically.
                cleanAlt(p.stack[n - 3]);
            }
            p.stack[n - 2] = re1;
            p.stack[n - 1] = re2;
            return true;
        }
    }
    return false;
}

// parseRightParen handles a ) in the input.
[GoRecv] internal static error parseRightParen(this ref parser p) {
    p.concat();
    if (p.swapVerticalBar()) {
        // pop vertical bar
        p.stack = p.stack[..(int)(len(p.stack) - 1)];
    }
    p.alternate();
    nint n = len(p.stack);
    if (n < 2) {
        return new ΔError(ErrUnexpectedParen, p.wholeRegexp);
    }
    var re1 = p.stack[n - 1];
    var re2 = p.stack[n - 2];
    p.stack = p.stack[..(int)(n - 2)];
    if ((~re2).Op != opLeftParen) {
        return new ΔError(ErrUnexpectedParen, p.wholeRegexp);
    }
    // Restore flags at time of paren.
    p.flags = re2.val.Flags;
    if ((~re2).Cap == 0){
        // Just for grouping.
        p.push(re1);
    } else {
        re2.val.Op = OpCapture;
        re2.val.Sub = (~re2).Sub0[..1];
        (~re2).Sub[0] = re1;
        p.push(re2);
    }
    return default!;
}

// parseEscape parses an escape sequence at the beginning of s
// and returns the rune.
[GoRecv] internal static (rune r, @string rest, error err) parseEscape(this ref parser p, @string s) {
    rune r = default!;
    @string rest = default!;
    error err = default!;

    @string t = s[1..];
    if (t == ""u8) {
        return (0, "", new ΔError(ErrTrailingBackslash, ""));
    }
    var (c, t, err) = nextRune(t);
    if (err != default!) {
        return (0, "", err);
    }
Switch:
    var exprᴛ1 = c;
    var matchᴛ1 = false;
    { /* default: */
        if (c < utf8.RuneSelf && !isalnum(c)) {
            // Escaped non-word characters are always themselves.
            // PCRE is not quite so rigorous: it accepts things like
            // \q, but we don't. We once rejected \_, but too many
            // programs and people insist on using it, so allow \_.
            return (c, t, default!);
        }
    }
    if (exprᴛ1 is (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7') { matchᴛ1 = true;
        if (t == ""u8 || t[0] < (rune)'0' || t[0] > (rune)'7') {
            // Octal escapes.
            // Single non-zero digit is a backreference; not supported
            break;
        }
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is (rune)'0')) { matchᴛ1 = true;
        r = c - (rune)'0';
        for (nint i = 1; i < 3; i++) {
            // Consume up to three octal digits; already have one.
            if (t == ""u8 || t[0] < (rune)'0' || t[0] > (rune)'7') {
                break;
            }
            r = r * 8 + ((rune)t[0]) - (rune)'0';
            t = t[1..];
        }
        return (r, t, default!);
    }
    if (exprᴛ1 is (rune)'x') { matchᴛ1 = true;
        if (t == ""u8) {
            // Hexadecimal escapes.
            break;
        }
        {
            (c, t, err) = nextRune(t); if (err != default!) {
                return (0, "", err);
            }
        }
        if (c == (rune)'{') {
            // Any number of digits in braces.
            // Perl accepts any text at all; it ignores all text
            // after the first non-hex digit. We require only hex digits,
            // and at least one.
            nint nhex = 0;
            r = 0;
            while (ᐧ) {
                if (t == ""u8) {
                    goto break_Switch;
                }
                {
                    (c, t, err) = nextRune(t); if (err != default!) {
                        return (0, "", err);
                    }
                }
                if (c == (rune)'}') {
                    break;
                }
                var v = unhex(c);
                if (v < 0) {
                    goto break_Switch;
                }
                r = r * 16 + v;
                if (r > unicode.MaxRune) {
                    goto break_Switch;
                }
                nhex++;
            }
            if (nhex == 0) {
                goto break_Switch;
            }
            return (r, t, default!);
        }
        var x = unhex(c);
        {
            (c, t, err) = nextRune(t); if (err != default!) {
                // Easy case: two hex digits.
                return (0, "", err);
            }
        }
        var y = unhex(c);
        if (x < 0 || y < 0) {
            break;
        }
        return (x * 16 + y, t, default!);
    }
    if (exprᴛ1 is (rune)'a') { matchᴛ1 = true;
        return ((rune)'\a', t, err);
    }
    if (exprᴛ1 is (rune)'f') { matchᴛ1 = true;
        return ((rune)'\f', t, err);
    }
    if (exprᴛ1 is (rune)'n') { matchᴛ1 = true;
        return ((rune)'\n', t, err);
    }
    if (exprᴛ1 is (rune)'r') { matchᴛ1 = true;
        return ((rune)'\r', t, err);
    }
    if (exprᴛ1 is (rune)'t') {
        return ((rune)'\t', t, err);
    }
    if (exprᴛ1 is (rune)'v') { matchᴛ1 = true;
        return ((rune)'\v', t, err);
    }

    // C escapes. There is no case 'b', to avoid misparsing
    // the Perl word-boundary \b as the C backspace \b
    // when in POSIX mode. In Perl, /\b/ means word-boundary
    // but /[\b]/ means backspace. We don't support that.
    // If you want a backspace, embed a literal backspace
    // character or use \x08.
    return (0, "", new ΔError(ErrInvalidEscape, s[..(int)(len(s) - len(t))]));
}

// parseClassChar parses a character class character at the beginning of s
// and returns it.
[GoRecv] internal static (rune r, @string rest, error err) parseClassChar(this ref parser p, @string s, @string wholeClass) {
    rune r = default!;
    @string rest = default!;
    error err = default!;

    if (s == ""u8) {
        return (0, "", new ΔError(Code: ErrMissingBracket, Expr: wholeClass));
    }
    // Allow regular escape sequences even though
    // many need not be escaped in this context.
    if (s[0] == (rune)'\\') {
        return p.parseEscape(s);
    }
    return nextRune(s);
}

[GoType] partial struct charGroup {
    internal nint sign;
    internal slice<rune> @class;
}

//go:generate perl make_perl_groups.pl perl_groups.go

// parsePerlClassEscape parses a leading Perl character class escape like \d
// from the beginning of s. If one is present, it appends the characters to r
// and returns the new slice r and the remainder of the string.
[GoRecv] internal static (slice<rune> @out, @string rest) parsePerlClassEscape(this ref parser p, @string s, slice<rune> r) {
    slice<rune> @out = default!;
    @string rest = default!;

    if ((Flags)(p.flags & PerlX) == 0 || len(s) < 2 || s[0] != (rune)'\\') {
        return (@out, rest);
    }
    var g = perlGroup[s[0..2]];
    if (g.sign == 0) {
        return (@out, rest);
    }
    return (p.appendGroup(r, g), s[2..]);
}

// parseNamedClass parses a leading POSIX named character class like [:alnum:]
// from the beginning of s. If one is present, it appends the characters to r
// and returns the new slice r and the remainder of the string.
[GoRecv] internal static (slice<rune> @out, @string rest, error err) parseNamedClass(this ref parser p, @string s, slice<rune> r) {
    slice<rune> @out = default!;
    @string rest = default!;
    error err = default!;

    if (len(s) < 2 || s[0] != (rune)'[' || s[1] != (rune)':') {
        return (@out, rest, err);
    }
    nint i = strings.Index(s[2..], ":]"u8);
    if (i < 0) {
        return (@out, rest, err);
    }
    i += 2;
    @string name = s[0..(int)(i + 2)];
    s = s[(int)(i + 2)..];
    var g = posixGroup[name];
    if (g.sign == 0) {
        return (default!, "", new ΔError(ErrInvalidCharRange, name));
    }
    return (p.appendGroup(r, g), s, default!);
}

[GoRecv] internal static slice<rune> appendGroup(this ref parser p, slice<rune> r, charGroup g) {
    if ((Flags)(p.flags & FoldCase) == 0){
        if (g.sign < 0){
            r = appendNegatedClass(r, g.@class);
        } else {
            r = appendClass(r, g.@class);
        }
    } else {
        var tmp = p.tmpClass[..0];
        tmp = appendFoldedClass(tmp, g.@class);
        p.tmpClass = tmp;
        tmp = cleanClass(Ꮡ(p.tmpClass));
        if (g.sign < 0){
            r = appendNegatedClass(r, tmp);
        } else {
            r = appendClass(r, tmp);
        }
    }
    return r;
}

internal static ж<unicode.RangeTable> anyTable = Ꮡ(new unicode.RangeTable(
    R16: new unicode.Range16[]{new(Lo: 0, Hi: 1 << (int)(16) - 1, Stride: 1)}.slice(),
    R32: new unicode.Range32[]{new(Lo: 1 << (int)(16), Hi: unicode.MaxRune, Stride: 1)}.slice()
));

// unicodeTable returns the unicode.RangeTable identified by name
// and the table of additional fold-equivalent code points.
internal static (ж<unicode.RangeTable>, ж<unicode.RangeTable>) unicodeTable(@string name) {
    // Special case: "Any" means any.
    if (name == "Any"u8) {
        return (anyTable, anyTable);
    }
    {
        var t = unicode.Categories[name]; if (t != nil) {
            return (t, unicode.FoldCategory[name]);
        }
    }
    {
        var t = unicode.Scripts[name]; if (t != nil) {
            return (t, unicode.FoldScript[name]);
        }
    }
    return (default!, default!);
}

// parseUnicodeClass parses a leading Unicode character class like \p{Han}
// from the beginning of s. If one is present, it appends the characters to r
// and returns the new slice r and the remainder of the string.
[GoRecv] internal static (slice<rune> @out, @string rest, error err) parseUnicodeClass(this ref parser p, @string s, slice<rune> r) {
    slice<rune> @out = default!;
    @string rest = default!;
    error err = default!;

    if ((Flags)(p.flags & UnicodeGroups) == 0 || len(s) < 2 || s[0] != (rune)'\\' || s[1] != (rune)'p' && s[1] != (rune)'P') {
        return (@out, rest, err);
    }
    // Committed to parse or return error.
    nint sign = +1;
    if (s[1] == (rune)'P') {
        sign = -1;
    }
    @string t = s[2..];
    var (c, t, err) = nextRune(t);
    if (err != default!) {
        return (@out, rest, err);
    }
    ref var seq = ref heap(new @string(), out var Ꮡseq);
    @string name = default!;
    if (c != (rune)'{'){
        // Single-letter name.
        seq = s[..(int)(len(s) - len(t))];
        name = seq[2..];
    } else {
        // Name is in braces.
        nint end = strings.IndexRune(s, (rune)'}');
        if (end < 0) {
            {
                err = checkUTF8(s); if (err != default!) {
                    return (@out, rest, err);
                }
            }
            return (default!, "", new ΔError(ErrInvalidCharRange, s));
        }
        (seq, t) = (s[..(int)(end + 1)], s[(int)(end + 1)..]);
        name = s[3..(int)(end)];
        {
            err = checkUTF8(name); if (err != default!) {
                return (@out, rest, err);
            }
        }
    }
    // Group can have leading negation too.  \p{^Han} == \P{Han}, \P{^Han} == \p{Han}.
    if (name != ""u8 && name[0] == (rune)'^') {
        sign = -sign;
        name = name[1..];
    }
    (tab, fold) = unicodeTable(name);
    if (tab == nil) {
        return (default!, "", new ΔError(ErrInvalidCharRange, seq));
    }
    if ((Flags)(p.flags & FoldCase) == 0 || fold == nil){
        if (sign > 0){
            r = appendTable(r, tab);
        } else {
            r = appendNegatedTable(r, tab);
        }
    } else {
        // Merge and clean tab and fold in a temporary buffer.
        // This is necessary for the negative case and just tidy
        // for the positive case.
        var tmp = p.tmpClass[..0];
        tmp = appendTable(tmp, tab);
        tmp = appendTable(tmp, fold);
        p.tmpClass = tmp;
        tmp = cleanClass(Ꮡ(p.tmpClass));
        if (sign > 0){
            r = appendClass(r, tmp);
        } else {
            r = appendNegatedClass(r, tmp);
        }
    }
    return (r, t, default!);
}

// parseClass parses a character class at the beginning of s
// and pushes it onto the parse stack.
[GoRecv] internal static (@string rest, error err) parseClass(this ref parser p, @string s) {
    @string rest = default!;
    error err = default!;

    @string t = s[1..];
    // chop [
    var re = p.newRegexp(OpCharClass);
    re.val.Flags = p.flags;
    re.val.Rune = (~re).Rune0[..0];
    nint sign = +1;
    if (t != ""u8 && t[0] == (rune)'^') {
        sign = -1;
        t = t[1..];
        // If character class does not match \n, add it here,
        // so that negation later will do the right thing.
        if ((Flags)(p.flags & ClassNL) == 0) {
            re.val.Rune = append((~re).Rune, (rune)'\n', (rune)'\n');
        }
    }
    var @class = re.val.Rune;
    var first = true;
    // ] and - are okay as first char in class
    while (t == ""u8 || t[0] != (rune)']' || first) {
        // POSIX: - is only okay unescaped as first or last in class.
        // Perl: - is okay anywhere.
        if (t != ""u8 && t[0] == (rune)'-' && (Flags)(p.flags & PerlX) == 0 && !first && (len(t) == 1 || t[1] != (rune)']')) {
            var (_, size) = utf8.DecodeRuneInString(t[1..]);
            return ("", new ΔError(Code: ErrInvalidCharRange, Expr: t[..(int)(1 + size)]));
        }
        first = false;
        // Look for POSIX [:alnum:] etc.
        if (len(t) > 2 && t[0] == (rune)'[' && t[1] == (rune)':') {
            var (nclass, nt, errΔ1) = p.parseNamedClass(t, @class);
            if (errΔ1 != default!) {
                return ("", errΔ1);
            }
            if (nclass != default!) {
                (@class, t) = (nclass, nt);
                continue;
            }
        }
        // Look for Unicode character group like \p{Han}.
        var (nclass, nt, errΔ2) = p.parseUnicodeClass(t, @class);
        if (errΔ2 != default!) {
            return ("", errΔ2);
        }
        if (nclass != default!) {
            (@class, t) = (nclass, nt);
            continue;
        }
        // Look for Perl character class symbols (extension).
        {
            var (nclassΔ1, ntΔ1) = p.parsePerlClassEscape(t, @class); if (nclassΔ1 != default!) {
                (@class, t) = (nclassΔ1, ntΔ1);
                continue;
            }
        }
        // Single character or simple range.
        @string rng = t;
        rune lo = default!;
        rune hi = default!;
        {
            (lo, t, errΔ2) = p.parseClassChar(t, s); if (errΔ2 != default!) {
                return ("", errΔ2);
            }
        }
        hi = lo;
        // [a-] means (a|-) so check for final ].
        if (len(t) >= 2 && t[0] == (rune)'-' && t[1] != (rune)']') {
            t = t[1..];
            {
                (hi, t, errΔ2) = p.parseClassChar(t, s); if (errΔ2 != default!) {
                    return ("", errΔ2);
                }
            }
            if (hi < lo) {
                rng = rng[..(int)(len(rng) - len(t))];
                return ("", new ΔError(Code: ErrInvalidCharRange, Expr: rng));
            }
        }
        if ((Flags)(p.flags & FoldCase) == 0){
            @class = appendRange(@class, lo, hi);
        } else {
            @class = appendFoldedRange(@class, lo, hi);
        }
    }
    t = t[1..];
    // chop ]
    // Use &re.Rune instead of &class to avoid allocation.
    re.val.Rune = @class;
    @class = cleanClass(Ꮡ((~re).Rune));
    if (sign < 0) {
        @class = negateClass(@class);
    }
    re.val.Rune = @class;
    p.push(re);
    return (t, default!);
}

// cleanClass sorts the ranges (pairs of elements of r),
// merges them, and eliminates duplicates.
internal static slice<rune> cleanClass(ж<slice<rune>> Ꮡrp) {
    ref var rp = ref Ꮡrp.val;

    // Sort by lo increasing, hi decreasing to break ties.
    sort.Sort(new ranges(Ꮡrp));
    var r = rp;
    if (len(r) < 2) {
        return r;
    }
    // Merge abutting, overlapping.
    nint w = 2;
    // write index
    for (nint i = 2; i < len(r); i += 2) {
        var (lo, hi) = (r[i], r[i + 1]);
        if (lo <= r[w - 1] + 1) {
            // merge with previous range
            if (hi > r[w - 1]) {
                r[w - 1] = hi;
            }
            continue;
        }
        // new disjoint range
        r[w] = lo;
        r[w + 1] = hi;
        w += 2;
    }
    return r[..(int)(w)];
}

// inCharClass reports whether r is in the class.
// It assumes the class has been cleaned by cleanClass.
internal static bool inCharClass(rune r, slice<rune> @class) {
    var (_, ok) = sort.Find(len(@class) / 2, 
    var classʗ1 = @class;
    (nint i) => {
        var (lo, hi) = (classʗ1[2 * i], classʗ1[2 * i + 1]);
        if (r > hi) {
            return +1;
        }
        if (r < lo) {
            return -1;
        }
        return 0;
    });
    return ok;
}

// appendLiteral returns the result of appending the literal x to the class r.
internal static slice<rune> appendLiteral(slice<rune> r, rune x, Flags flags) {
    if ((Flags)(flags & FoldCase) != 0) {
        return appendFoldedRange(r, x, x);
    }
    return appendRange(r, x, x);
}

// appendRange returns the result of appending the range lo-hi to the class r.
internal static slice<rune> appendRange(slice<rune> r, rune lo, rune hi) {
    // Expand last range or next to last range if it overlaps or abuts.
    // Checking two ranges helps when appending case-folded
    // alphabets, so that one range can be expanding A-Z and the
    // other expanding a-z.
    nint n = len(r);
    for (nint i = 2; i <= 4; i += 2) {
        // twice, using i=2, i=4
        if (n >= i) {
            var (rlo, rhi) = (r[n - i], r[n - i + 1]);
            if (lo <= rhi + 1 && rlo <= hi + 1) {
                if (lo < rlo) {
                    r[n - i] = lo;
                }
                if (hi > rhi) {
                    r[n - i + 1] = hi;
                }
                return r;
            }
        }
    }
    return append(r, lo, hi);
}

internal static readonly UntypedInt minFold = /* 0x0041 */ 65;
internal static readonly UntypedInt maxFold = /* 0x1e943 */ 125251;

// appendFoldedRange returns the result of appending the range lo-hi
// and its case folding-equivalent runes to the class r.
internal static slice<rune> appendFoldedRange(slice<rune> r, rune lo, rune hi) {
    // Optimizations.
    if (lo <= minFold && hi >= maxFold) {
        // Range is full: folding can't add more.
        return appendRange(r, lo, hi);
    }
    if (hi < minFold || lo > maxFold) {
        // Range is outside folding possibilities.
        return appendRange(r, lo, hi);
    }
    if (lo < minFold) {
        // [lo, minFold-1] needs no folding.
        r = appendRange(r, lo, minFold - 1);
        lo = minFold;
    }
    if (hi > maxFold) {
        // [maxFold+1, hi] needs no folding.
        r = appendRange(r, maxFold + 1, hi);
        hi = maxFold;
    }
    // Brute force. Depend on appendRange to coalesce ranges on the fly.
    for (var c = lo; c <= hi; c++) {
        r = appendRange(r, c, c);
        var f = unicode.SimpleFold(c);
        while (f != c) {
            r = appendRange(r, f, f);
            f = unicode.SimpleFold(f);
        }
    }
    return r;
}

// appendClass returns the result of appending the class x to the class r.
// It assume x is clean.
internal static slice<rune> appendClass(slice<rune> r, slice<rune> x) {
    for (nint i = 0; i < len(x); i += 2) {
        r = appendRange(r, x[i], x[i + 1]);
    }
    return r;
}

// appendFoldedClass returns the result of appending the case folding of the class x to the class r.
internal static slice<rune> appendFoldedClass(slice<rune> r, slice<rune> x) {
    for (nint i = 0; i < len(x); i += 2) {
        r = appendFoldedRange(r, x[i], x[i + 1]);
    }
    return r;
}

// appendNegatedClass returns the result of appending the negation of the class x to the class r.
// It assumes x is clean.
internal static slice<rune> appendNegatedClass(slice<rune> r, slice<rune> x) {
    var nextLo = (rune)'\u0000';
    for (nint i = 0; i < len(x); i += 2) {
        var (lo, hi) = (x[i], x[i + 1]);
        if (nextLo <= lo - 1) {
            r = appendRange(r, nextLo, lo - 1);
        }
        nextLo = hi + 1;
    }
    if (nextLo <= unicode.MaxRune) {
        r = appendRange(r, nextLo, unicode.MaxRune);
    }
    return r;
}

// appendTable returns the result of appending x to the class r.
internal static slice<rune> appendTable(slice<rune> r, ж<unicode.RangeTable> Ꮡx) {
    ref var x = ref Ꮡx.val;

    foreach (var (_, xr) in x.R16) {
        var (lo, hi, stride) = (((rune)xr.Lo), ((rune)xr.Hi), ((rune)xr.Stride));
        if (stride == 1) {
            r = appendRange(r, lo, hi);
            continue;
        }
        for (var c = lo; c <= hi; c += stride) {
            r = appendRange(r, c, c);
        }
    }
    foreach (var (_, xr) in x.R32) {
        var (lo, hi, stride) = (((rune)xr.Lo), ((rune)xr.Hi), ((rune)xr.Stride));
        if (stride == 1) {
            r = appendRange(r, lo, hi);
            continue;
        }
        for (var c = lo; c <= hi; c += stride) {
            r = appendRange(r, c, c);
        }
    }
    return r;
}

// appendNegatedTable returns the result of appending the negation of x to the class r.
internal static slice<rune> appendNegatedTable(slice<rune> r, ж<unicode.RangeTable> Ꮡx) {
    ref var x = ref Ꮡx.val;

    var nextLo = (rune)'\u0000';
    // lo end of next class to add
    foreach (var (_, xr) in x.R16) {
        var (lo, hi, stride) = (((rune)xr.Lo), ((rune)xr.Hi), ((rune)xr.Stride));
        if (stride == 1) {
            if (nextLo <= lo - 1) {
                r = appendRange(r, nextLo, lo - 1);
            }
            nextLo = hi + 1;
            continue;
        }
        for (var c = lo; c <= hi; c += stride) {
            if (nextLo <= c - 1) {
                r = appendRange(r, nextLo, c - 1);
            }
            nextLo = c + 1;
        }
    }
    foreach (var (_, xr) in x.R32) {
        var (lo, hi, stride) = (((rune)xr.Lo), ((rune)xr.Hi), ((rune)xr.Stride));
        if (stride == 1) {
            if (nextLo <= lo - 1) {
                r = appendRange(r, nextLo, lo - 1);
            }
            nextLo = hi + 1;
            continue;
        }
        for (var c = lo; c <= hi; c += stride) {
            if (nextLo <= c - 1) {
                r = appendRange(r, nextLo, c - 1);
            }
            nextLo = c + 1;
        }
    }
    if (nextLo <= unicode.MaxRune) {
        r = appendRange(r, nextLo, unicode.MaxRune);
    }
    return r;
}

// negateClass overwrites r and returns r's negation.
// It assumes the class r is already clean.
internal static slice<rune> negateClass(slice<rune> r) {
    var nextLo = (rune)'\u0000';
    // lo end of next class to add
    nint w = 0;
    // write index
    for (nint i = 0; i < len(r); i += 2) {
        var (lo, hi) = (r[i], r[i + 1]);
        if (nextLo <= lo - 1) {
            r[w] = nextLo;
            r[w + 1] = lo - 1;
            w += 2;
        }
        nextLo = hi + 1;
    }
    r = r[..(int)(w)];
    if (nextLo <= unicode.MaxRune) {
        // It's possible for the negation to have one more
        // range - this one - than the original class, so use append.
        r = append(r, nextLo, unicode.MaxRune);
    }
    return r;
}

// ranges implements sort.Interface on a []rune.
// The choice of receiver type definition is strange
// but avoids an allocation since we already have
// a *[]rune.
[GoType] partial struct ranges {
    internal ж<slice<rune>> p;
}

internal static bool Less(this ranges ra, nint i, nint j) {
    var p = ra.p.val;
    i *= 2;
    j *= 2;
    return p[i] < p[j] || p[i] == p[j] && p[i + 1] > p[j + 1];
}

internal static nint Len(this ranges ra) {
    return len(ra.p.val) / 2;
}

internal static void Swap(this ranges ra, nint i, nint j) {
    var p = ra.p.val;
    i *= 2;
    j *= 2;
    (p[i], p[i + 1], p[j], p[j + 1]) = (p[j], p[j + 1], p[i], p[i + 1]);
}

internal static error checkUTF8(@string s) {
    while (s != ""u8) {
        var (rune, size) = utf8.DecodeRuneInString(s);
        if (rune == utf8.RuneError && size == 1) {
            return new ΔError(Code: ErrInvalidUTF8, Expr: s);
        }
        s = s[(int)(size)..];
    }
    return default!;
}

internal static (rune c, @string t, error err) nextRune(@string s) {
    rune c = default!;
    @string t = default!;
    error err = default!;

    var (c, size) = utf8.DecodeRuneInString(s);
    if (c == utf8.RuneError && size == 1) {
        return (0, "", new ΔError(Code: ErrInvalidUTF8, Expr: s));
    }
    return (c, s[(int)(size)..], default!);
}

internal static bool isalnum(rune c) {
    return (rune)'0' <= c && c <= (rune)'9' || (rune)'A' <= c && c <= (rune)'Z' || (rune)'a' <= c && c <= (rune)'z';
}

internal static rune unhex(rune c) {
    if ((rune)'0' <= c && c <= (rune)'9') {
        return c - (rune)'0';
    }
    if ((rune)'a' <= c && c <= (rune)'f') {
        return c - (rune)'a' + 10;
    }
    if ((rune)'A' <= c && c <= (rune)'F') {
        return c - (rune)'A' + 10;
    }
    return -1;
}

} // end syntax_package
