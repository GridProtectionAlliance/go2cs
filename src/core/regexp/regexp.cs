// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package regexp implements regular expression search.
//
// The syntax of the regular expressions accepted is the same
// general syntax used by Perl, Python, and other languages.
// More precisely, it is the syntax accepted by RE2 and described at
// https://golang.org/s/re2syntax, except for \C.
// For an overview of the syntax, see the [regexp/syntax] package.
//
// The regexp implementation provided by this package is
// guaranteed to run in time linear in the size of the input.
// (This is a property not guaranteed by most open source
// implementations of regular expressions.) For more information
// about this property, see https://swtch.com/~rsc/regexp/regexp1.html
// or any book about automata theory.
//
// All characters are UTF-8-encoded code points.
// Following [utf8.DecodeRune], each byte of an invalid UTF-8 sequence
// is treated as if it encoded utf8.RuneError (U+FFFD).
//
// There are 16 methods of [Regexp] that match a regular expression and identify
// the matched text. Their names are matched by this regular expression:
//
//	Find(All)?(String)?(Submatch)?(Index)?
//
// If 'All' is present, the routine matches successive non-overlapping
// matches of the entire expression. Empty matches abutting a preceding
// match are ignored. The return value is a slice containing the successive
// return values of the corresponding non-'All' routine. These routines take
// an extra integer argument, n. If n >= 0, the function returns at most n
// matches/submatches; otherwise, it returns all of them.
//
// If 'String' is present, the argument is a string; otherwise it is a slice
// of bytes; return values are adjusted as appropriate.
//
// If 'Submatch' is present, the return value is a slice identifying the
// successive submatches of the expression. Submatches are matches of
// parenthesized subexpressions (also known as capturing groups) within the
// regular expression, numbered from left to right in order of opening
// parenthesis. Submatch 0 is the match of the entire expression, submatch 1 is
// the match of the first parenthesized subexpression, and so on.
//
// If 'Index' is present, matches and submatches are identified by byte index
// pairs within the input string: result[2*n:2*n+2] identifies the indexes of
// the nth submatch. The pair for n==0 identifies the match of the entire
// expression. If 'Index' is not present, the match is identified by the text
// of the match/submatch. If an index is negative or text is nil, it means that
// subexpression did not match any string in the input. For 'String' versions
// an empty string means either no match or an empty match.
//
// There is also a subset of the methods that can be applied to text read from
// an [io.RuneReader]: [Regexp.MatchReader], [Regexp.FindReaderIndex],
// [Regexp.FindReaderSubmatchIndex].
//
// This set may grow. Note that regular expression matches may need to
// examine text beyond the text returned by a match, so the methods that
// match text from an [io.RuneReader] may read arbitrarily far into the input
// before returning.
//
// (There are a few other methods that do not match this pattern.)
namespace go;

using bytes = bytes_package;
using io = io_package;
using syntax = regexp.syntax_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using regexp;
using unicode;

partial class regexp_package {

// Regexp is the representation of a compiled regular expression.
// A Regexp is safe for concurrent use by multiple goroutines,
// except for configuration methods, such as [Regexp.Longest].
[GoType] partial struct Regexp {
    internal @string expr;      // as passed to Compile
    internal ж<regexp.syntax_package.Prog> prog; // compiled program
    internal ж<onePassProg> onepass; // onepass program or nil
    internal nint numSubexp;
    internal nint maxBitStateLen;
    internal slice<@string> subexpNames;
    internal @string prefix;        // required prefix in unanchored matches
    internal slice<byte> prefixBytes;    // prefix, as a []byte
    internal rune prefixRune;           // first rune in prefix
    internal uint32 prefixEnd;         // pc for last rune in prefix
    internal nint mpool;           // pool for machines
    internal nint matchcap;           // size of recorded match lengths
    internal bool prefixComplete;           // prefix is the entire regexp
    internal regexp.syntax_package.EmptyOp cond; // empty-width conditions required at start of match
    internal nint minInputLen;           // minimum length of the input in bytes
    // This field can be modified by the Longest method,
    // but it is otherwise read-only.
    internal bool longest; // whether regexp prefers leftmost-longest match
}

// String returns the source text used to compile the regular expression.
[GoRecv] public static @string String(this ref Regexp re) {
    return re.expr;
}

// Copy returns a new [Regexp] object copied from re.
// Calling [Regexp.Longest] on one copy does not affect another.
//
// Deprecated: In earlier releases, when using a [Regexp] in multiple goroutines,
// giving each goroutine its own copy helped to avoid lock contention.
// As of Go 1.12, using Copy is no longer necessary to avoid lock contention.
// Copy may still be appropriate if the reason for its use is to make
// two copies with different [Regexp.Longest] settings.
[GoRecv] public static ж<Regexp> Copy(this ref Regexp re) {
    ref var re2 = ref heap<Regexp>(out var Ꮡre2);
    re2 = re;
    return Ꮡre2;
}

// Compile parses a regular expression and returns, if successful,
// a [Regexp] object that can be used to match against text.
//
// When matching against text, the regexp returns a match that
// begins as early as possible in the input (leftmost), and among those
// it chooses the one that a backtracking search would have found first.
// This so-called leftmost-first matching is the same semantics
// that Perl, Python, and other implementations use, although this
// package implements it without the expense of backtracking.
// For POSIX leftmost-longest matching, see [CompilePOSIX].
public static (ж<Regexp>, error) Compile(@string expr) {
    return compile(expr, syntax.Perl, false);
}

// CompilePOSIX is like [Compile] but restricts the regular expression
// to POSIX ERE (egrep) syntax and changes the match semantics to
// leftmost-longest.
//
// That is, when matching against text, the regexp returns a match that
// begins as early as possible in the input (leftmost), and among those
// it chooses a match that is as long as possible.
// This so-called leftmost-longest matching is the same semantics
// that early regular expression implementations used and that POSIX
// specifies.
//
// However, there can be multiple leftmost-longest matches, with different
// submatch choices, and here this package diverges from POSIX.
// Among the possible leftmost-longest matches, this package chooses
// the one that a backtracking search would have found first, while POSIX
// specifies that the match be chosen to maximize the length of the first
// subexpression, then the second, and so on from left to right.
// The POSIX rule is computationally prohibitive and not even well-defined.
// See https://swtch.com/~rsc/regexp/regexp2.html#posix for details.
public static (ж<Regexp>, error) CompilePOSIX(@string expr) {
    return compile(expr, syntax.POSIX, true);
}

// Longest makes future searches prefer the leftmost-longest match.
// That is, when matching against text, the regexp returns a match that
// begins as early as possible in the input (leftmost), and among those
// it chooses a match that is as long as possible.
// This method modifies the [Regexp] and may not be called concurrently
// with any other methods.
[GoRecv] public static void Longest(this ref Regexp re) {
    re.longest = true;
}

internal static (ж<Regexp>, error) compile(@string expr, syntax.Flags mode, bool longest) {
    (re, err) = syntax.Parse(expr, mode);
    if (err != default!) {
        return (default!, err);
    }
    ref var maxCap = ref heap<nint>(out var ᏑmaxCap);
    maxCap = re.MaxCap();
    var capNames = re.CapNames();
    re = re.Simplify();
    (prog, err) = syntax.Compile(re);
    if (err != default!) {
        return (default!, err);
    }
    ref var matchcap = ref heap<nint>(out var Ꮡmatchcap);
    matchcap = prog.val.NumCap;
    if (matchcap < 2) {
        matchcap = 2;
    }
    var regexp = Ꮡ(new Regexp(
        expr: expr,
        prog: prog,
        onepass: compileOnePass(prog),
        numSubexp: maxCap,
        subexpNames: capNames,
        cond: prog.StartCond(),
        longest: longest,
        matchcap: matchcap,
        minInputLen: minInputLen(re)
    ));
    if ((~regexp).onepass == nil){
        (regexp.val.prefix, regexp.val.prefixComplete) = prog.Prefix();
        regexp.val.maxBitStateLen = maxBitStateLen(prog);
    } else {
        (regexp.val.prefix, regexp.val.prefixComplete, regexp.val.prefixEnd) = onePassPrefix(prog);
    }
    if ((~regexp).prefix != ""u8) {
        // TODO(rsc): Remove this allocation by adding
        // IndexString to package bytes.
        regexp.val.prefixBytes = slice<byte>((~regexp).prefix);
        (regexp.val.prefixRune, _) = utf8.DecodeRuneInString((~regexp).prefix);
    }
    nint n = len((~prog).Inst);
    nint i = 0;
    while (matchSize[i] != 0 && matchSize[i] < n) {
        i++;
    }
    regexp.val.mpool = i;
    return (regexp, default!);
}

// Pools of *machine for use during (*Regexp).doExecute,
// split up by the size of the execution queues.
// matchPool[i] machines have queue size matchSize[i].
// On a 64-bit system each queue entry is 16 bytes,
// so matchPool[0] has 16*2*128 = 4kB queues, etc.
// The final matchPool is a catch-all for very large queues.
internal static array<nint> matchSize = new nint[]{128, 512, 2048, 16384, 0}.array();

internal static array<sync.Pool> matchPool;

// get returns a machine to use for matching re.
// It uses the re's machine cache if possible, to avoid
// unnecessary allocation.
[GoRecv] internal static ж<machine> get(this ref Regexp re) {
    var (m, ok) = matchPool[re.mpool].Get()._<machine.val>(ᐧ);
    if (!ok) {
        m = @new<machine>();
    }
    m.val.re = re;
    m.val.p = re.prog;
    if (cap((~m).matchcap) < re.matchcap) {
        m.val.matchcap = new slice<nint>(re.matchcap);
        foreach (var (_, t) in (~m).pool) {
            t.val.cap = new slice<nint>(re.matchcap);
        }
    }
    // Allocate queues if needed.
    // Or reallocate, for "large" match pool.
    nint n = matchSize[re.mpool];
    if (n == 0) {
        // large pool
        n = len(re.prog.Inst);
    }
    if (len((~m).q0.sparse) < n) {
        m.val.q0 = new queue(new slice<uint32>(n), new slice<entry>(0, n));
        m.val.q1 = new queue(new slice<uint32>(n), new slice<entry>(0, n));
    }
    return m;
}

// put returns a machine to the correct machine pool.
[GoRecv] public static void put(this ref Regexp re, ж<machine> Ꮡm) {
    ref var m = ref Ꮡm.val;

    m.re = default!;
    m.p = default!;
    m.inputs.clear();
    matchPool[re.mpool].Put(m);
}

// minInputLen walks the regexp to find the minimum length of any matchable input.
internal static nint minInputLen(ж<syntax.Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    var exprᴛ1 = re.Op;
    { /* default: */
        return 0;
    }
    if (exprᴛ1 == syntax.OpAnyChar || exprᴛ1 == syntax.OpAnyCharNotNL || exprᴛ1 == syntax.OpCharClass) {
        return 1;
    }
    if (exprᴛ1 == syntax.OpLiteral) {
        nint l = 0;
        foreach (var (_, r) in re.Rune) {
            if (r == utf8.RuneError){
                l++;
            } else {
                l += utf8.RuneLen(r);
            }
        }
        return l;
    }
    if (exprᴛ1 == syntax.OpCapture || exprᴛ1 == syntax.OpPlus) {
        return minInputLen(re.Sub[0]);
    }
    if (exprᴛ1 == syntax.OpRepeat) {
        return re.Min * minInputLen(re.Sub[0]);
    }
    if (exprᴛ1 == syntax.OpConcat) {
        nint l = 0;
        foreach (var (_, sub) in re.Sub) {
            l += minInputLen(sub);
        }
        return l;
    }
    if (exprᴛ1 == syntax.OpAlternate) {
        nint l = minInputLen(re.Sub[0]);
        nint lnext = default!;
        foreach (var (_, sub) in re.Sub[1..]) {
            lnext = minInputLen(sub);
            if (lnext < l) {
                l = lnext;
            }
        }
        return l;
    }

}

// MustCompile is like [Compile] but panics if the expression cannot be parsed.
// It simplifies safe initialization of global variables holding compiled regular
// expressions.
public static ж<Regexp> MustCompile(@string str) {
    (regexp, err) = Compile(str);
    if (err != default!) {
        throw panic(@"regexp: Compile("u8 + quote(str) + @"): "u8 + err.Error());
    }
    return regexp;
}

// MustCompilePOSIX is like [CompilePOSIX] but panics if the expression cannot be parsed.
// It simplifies safe initialization of global variables holding compiled regular
// expressions.
public static ж<Regexp> MustCompilePOSIX(@string str) {
    (regexp, err) = CompilePOSIX(str);
    if (err != default!) {
        throw panic(@"regexp: CompilePOSIX("u8 + quote(str) + @"): "u8 + err.Error());
    }
    return regexp;
}

internal static @string quote(@string s) {
    if (strconv.CanBackquote(s)) {
        return "`"u8 + s + "`"u8;
    }
    return strconv.Quote(s);
}

// NumSubexp returns the number of parenthesized subexpressions in this [Regexp].
[GoRecv] public static nint NumSubexp(this ref Regexp re) {
    return re.numSubexp;
}

// SubexpNames returns the names of the parenthesized subexpressions
// in this [Regexp]. The name for the first sub-expression is names[1],
// so that if m is a match slice, the name for m[i] is SubexpNames()[i].
// Since the Regexp as a whole cannot be named, names[0] is always
// the empty string. The slice should not be modified.
[GoRecv] public static slice<@string> SubexpNames(this ref Regexp re) {
    return re.subexpNames;
}

// SubexpIndex returns the index of the first subexpression with the given name,
// or -1 if there is no subexpression with that name.
//
// Note that multiple subexpressions can be written using the same name, as in
// (?P<bob>a+)(?P<bob>b+), which declares two subexpressions named "bob".
// In this case, SubexpIndex returns the index of the leftmost such subexpression
// in the regular expression.
[GoRecv] public static nint SubexpIndex(this ref Regexp re, @string name) {
    if (name != ""u8) {
        foreach (var (i, s) in re.subexpNames) {
            if (name == s) {
                return i;
            }
        }
    }
    return -1;
}

internal const rune endOfText = -1;

// input abstracts different representations of the input text. It provides
// one-character lookahead.
[GoType] partial interface input {
    (rune r, nint width) step(nint pos); // advance one rune
    bool canCheckPrefix();             // can we look ahead without losing info?
    bool hasPrefix(ж<Regexp> re);
    nint index(ж<Regexp> re, nint pos);
    lazyFlag context(nint pos);
}

// inputString scans a string.
[GoType] partial struct inputString {
    internal @string str;
}

[GoRecv] internal static (rune, nint) step(this ref inputString i, nint pos) {
    if (pos < len(i.str)) {
        var c = i.str[pos];
        if (c < utf8.RuneSelf) {
            return (((rune)c), 1);
        }
        return utf8.DecodeRuneInString(i.str[(int)(pos)..]);
    }
    return (endOfText, 0);
}

[GoRecv] internal static bool canCheckPrefix(this ref inputString i) {
    return true;
}

[GoRecv] internal static bool hasPrefix(this ref inputString i, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    return strings.HasPrefix(i.str, re.prefix);
}

[GoRecv] internal static nint index(this ref inputString i, ж<Regexp> Ꮡre, nint pos) {
    ref var re = ref Ꮡre.val;

    return strings.Index(i.str[(int)(pos)..], re.prefix);
}

[GoRecv] internal static lazyFlag context(this ref inputString i, nint pos) {
    var (r1, r2) = (endOfText, endOfText);
    // 0 < pos && pos <= len(i.str)
    if (((nuint)(pos - 1)) < ((nuint)len(i.str))) {
        r1 = ((rune)i.str[pos - 1]);
        if (r1 >= utf8.RuneSelf) {
            (r1, _) = utf8.DecodeLastRuneInString(i.str[..(int)(pos)]);
        }
    }
    // 0 <= pos && pos < len(i.str)
    if (((nuint)pos) < ((nuint)len(i.str))) {
        r2 = ((rune)i.str[pos]);
        if (r2 >= utf8.RuneSelf) {
            (r2, _) = utf8.DecodeRuneInString(i.str[(int)(pos)..]);
        }
    }
    return newLazyFlag(r1, r2);
}

// inputBytes scans a byte slice.
[GoType] partial struct inputBytes {
    internal slice<byte> str;
}

[GoRecv] internal static (rune, nint) step(this ref inputBytes i, nint pos) {
    if (pos < len(i.str)) {
        var c = i.str[pos];
        if (c < utf8.RuneSelf) {
            return (((rune)c), 1);
        }
        return utf8.DecodeRune(i.str[(int)(pos)..]);
    }
    return (endOfText, 0);
}

[GoRecv] internal static bool canCheckPrefix(this ref inputBytes i) {
    return true;
}

[GoRecv] internal static bool hasPrefix(this ref inputBytes i, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    return bytes.HasPrefix(i.str, re.prefixBytes);
}

[GoRecv] internal static nint index(this ref inputBytes i, ж<Regexp> Ꮡre, nint pos) {
    ref var re = ref Ꮡre.val;

    return bytes.Index(i.str[(int)(pos)..], re.prefixBytes);
}

[GoRecv] internal static lazyFlag context(this ref inputBytes i, nint pos) {
    var (r1, r2) = (endOfText, endOfText);
    // 0 < pos && pos <= len(i.str)
    if (((nuint)(pos - 1)) < ((nuint)len(i.str))) {
        r1 = ((rune)i.str[pos - 1]);
        if (r1 >= utf8.RuneSelf) {
            (r1, _) = utf8.DecodeLastRune(i.str[..(int)(pos)]);
        }
    }
    // 0 <= pos && pos < len(i.str)
    if (((nuint)pos) < ((nuint)len(i.str))) {
        r2 = ((rune)i.str[pos]);
        if (r2 >= utf8.RuneSelf) {
            (r2, _) = utf8.DecodeRune(i.str[(int)(pos)..]);
        }
    }
    return newLazyFlag(r1, r2);
}

// inputReader scans a RuneReader.
[GoType] partial struct inputReader {
    internal io_package.RuneReader r;
    internal bool atEOT;
    internal nint pos;
}

[GoRecv] internal static (rune, nint) step(this ref inputReader i, nint pos) {
    if (!i.atEOT && pos != i.pos) {
        return (endOfText, 0);
    }
    var (r, w, err) = i.r.ReadRune();
    if (err != default!) {
        i.atEOT = true;
        return (endOfText, 0);
    }
    i.pos += w;
    return (r, w);
}

[GoRecv] internal static bool canCheckPrefix(this ref inputReader i) {
    return false;
}

[GoRecv] internal static bool hasPrefix(this ref inputReader i, ж<Regexp> Ꮡre) {
    ref var re = ref Ꮡre.val;

    return false;
}

[GoRecv] internal static nint index(this ref inputReader i, ж<Regexp> Ꮡre, nint pos) {
    ref var re = ref Ꮡre.val;

    return -1;
}

[GoRecv] internal static lazyFlag context(this ref inputReader i, nint pos) {
    return 0;
}

// not used

// LiteralPrefix returns a literal string that must begin any match
// of the regular expression re. It returns the boolean true if the
// literal string comprises the entire regular expression.
[GoRecv] public static (@string prefix, bool complete) LiteralPrefix(this ref Regexp re) {
    @string prefix = default!;
    bool complete = default!;

    return (re.prefix, re.prefixComplete);
}

// MatchReader reports whether the text returned by the [io.RuneReader]
// contains any match of the regular expression re.
[GoRecv] public static bool MatchReader(this ref Regexp re, io.RuneReader r) {
    return re.doMatch(r, default!, ""u8);
}

// MatchString reports whether the string s
// contains any match of the regular expression re.
[GoRecv] public static bool MatchString(this ref Regexp re, @string s) {
    return re.doMatch(default!, default!, s);
}

// Match reports whether the byte slice b
// contains any match of the regular expression re.
[GoRecv] public static bool Match(this ref Regexp re, slice<byte> b) {
    return re.doMatch(default!, b, ""u8);
}

// MatchReader reports whether the text returned by the [io.RuneReader]
// contains any match of the regular expression pattern.
// More complicated queries need to use [Compile] and the full [Regexp] interface.
public static (bool matched, error err) MatchReader(@string pattern, io.RuneReader r) {
    bool matched = default!;
    error err = default!;

    (re, err) = Compile(pattern);
    if (err != default!) {
        return (false, err);
    }
    return (re.MatchReader(r), default!);
}

// MatchString reports whether the string s
// contains any match of the regular expression pattern.
// More complicated queries need to use [Compile] and the full [Regexp] interface.
public static (bool matched, error err) MatchString(@string pattern, @string s) {
    bool matched = default!;
    error err = default!;

    (re, err) = Compile(pattern);
    if (err != default!) {
        return (false, err);
    }
    return (re.MatchString(s), default!);
}

// Match reports whether the byte slice b
// contains any match of the regular expression pattern.
// More complicated queries need to use [Compile] and the full [Regexp] interface.
public static (bool matched, error err) Match(@string pattern, slice<byte> b) {
    bool matched = default!;
    error err = default!;

    (re, err) = Compile(pattern);
    if (err != default!) {
        return (false, err);
    }
    return (re.Match(b), default!);
}

// ReplaceAllString returns a copy of src, replacing matches of the [Regexp]
// with the replacement string repl.
// Inside repl, $ signs are interpreted as in [Regexp.Expand].
[GoRecv] public static @string ReplaceAllString(this ref Regexp re, @string src, @string repl) {
    nint n = 2;
    if (strings.Contains(repl, "$"u8)) {
        n = 2 * (re.numSubexp + 1);
    }
    var b = re.replaceAll(default!, src, n, (slice<byte> dst, slice<nint> match) => re.expand(dst, repl, default!, src, match));
    return ((@string)b);
}

// ReplaceAllLiteralString returns a copy of src, replacing matches of the [Regexp]
// with the replacement string repl. The replacement repl is substituted directly,
// without using [Regexp.Expand].
[GoRecv] public static @string ReplaceAllLiteralString(this ref Regexp re, @string src, @string repl) {
    return ((@string)re.replaceAll(default!, src, 2, (slice<byte> dst, slice<nint> match) => append(dst, repl.ꓸꓸꓸ)));
}

// ReplaceAllStringFunc returns a copy of src in which all matches of the
// [Regexp] have been replaced by the return value of function repl applied
// to the matched substring. The replacement returned by repl is substituted
// directly, without using [Regexp.Expand].
[GoRecv] public static @string ReplaceAllStringFunc(this ref Regexp re, @string src, Func<@string, @string> repl) {
    var b = re.replaceAll(default!, src, 2, (slice<byte> dst, slice<nint> match) => append(dst, repl(src[(int)(match[0])..(int)(match[1])]).ꓸꓸꓸ));
    return ((@string)b);
}

[GoRecv] internal static slice<byte> replaceAll(this ref Regexp re, slice<byte> bsrc, @string src, nint nmatch, Func<slice<byte>, slice<nint>, slice<byte>> repl) {
    nint lastMatchEnd = 0;
    // end position of the most recent match
    nint searchPos = 0;
    // position where we next look for a match
    slice<byte> buf = default!;
    nint endPos = default!;
    if (bsrc != default!){
        endPos = len(bsrc);
    } else {
        endPos = len(src);
    }
    if (nmatch > re.prog.NumCap) {
        nmatch = re.prog.NumCap;
    }
    array<nint> dstCap = new(2);
    while (searchPos <= endPos) {
        var a = re.doExecute(default!, bsrc, src, searchPos, nmatch, dstCap[..0]);
        if (len(a) == 0) {
            break;
        }
        // no more matches
        // Copy the unmatched characters before this match.
        if (bsrc != default!){
            buf = append(buf, bsrc[(int)(lastMatchEnd)..(int)(a[0])].ꓸꓸꓸ);
        } else {
            buf = append(buf, src[(int)(lastMatchEnd)..(int)(a[0])].ꓸꓸꓸ);
        }
        // Now insert a copy of the replacement string, but not for a
        // match of the empty string immediately after another match.
        // (Otherwise, we get double replacement for patterns that
        // match both empty and nonempty strings.)
        if (a[1] > lastMatchEnd || a[0] == 0) {
            buf = repl(buf, a);
        }
        lastMatchEnd = a[1];
        // Advance past this match; always advance at least one character.
        nint width = default!;
        if (bsrc != default!){
            (_, width) = utf8.DecodeRune(bsrc[(int)(searchPos)..]);
        } else {
            (_, width) = utf8.DecodeRuneInString(src[(int)(searchPos)..]);
        }
        if (searchPos + width > a[1]){
            searchPos += width;
        } else 
        if (searchPos + 1 > a[1]){
            // This clause is only needed at the end of the input
            // string. In that case, DecodeRuneInString returns width=0.
            searchPos++;
        } else {
            searchPos = a[1];
        }
    }
    // Copy the unmatched characters after the last match.
    if (bsrc != default!){
        buf = append(buf, bsrc[(int)(lastMatchEnd)..].ꓸꓸꓸ);
    } else {
        buf = append(buf, src[(int)(lastMatchEnd)..].ꓸꓸꓸ);
    }
    return buf;
}

// ReplaceAll returns a copy of src, replacing matches of the [Regexp]
// with the replacement text repl.
// Inside repl, $ signs are interpreted as in [Regexp.Expand].
[GoRecv] public static slice<byte> ReplaceAll(this ref Regexp re, slice<byte> src, slice<byte> repl) {
    nint n = 2;
    if (bytes.IndexByte(repl, (rune)'$') >= 0) {
        n = 2 * (re.numSubexp + 1);
    }
    @string srepl = ""u8;
    var b = re.replaceAll(src, ""u8, n, 
    var replʗ1 = repl;
    var srcʗ1 = src;
    (slice<byte> dst, slice<nint> match) => {
        if (len(srepl) != len(replʗ1)) {
            srepl = ((@string)replʗ1);
        }
        return re.expand(dst, srepl, srcʗ1, ""u8, match);
    });
    return b;
}

// ReplaceAllLiteral returns a copy of src, replacing matches of the [Regexp]
// with the replacement bytes repl. The replacement repl is substituted directly,
// without using [Regexp.Expand].
[GoRecv] public static slice<byte> ReplaceAllLiteral(this ref Regexp re, slice<byte> src, slice<byte> repl) {
    return re.replaceAll(src, ""u8, 2, 
    var replʗ1 = repl;
    (slice<byte> dst, slice<nint> match) => append(dst, replʗ1.ꓸꓸꓸ));
}

// ReplaceAllFunc returns a copy of src in which all matches of the
// [Regexp] have been replaced by the return value of function repl applied
// to the matched byte slice. The replacement returned by repl is substituted
// directly, without using [Regexp.Expand].
[GoRecv] public static slice<byte> ReplaceAllFunc(this ref Regexp re, slice<byte> src, Func<slice<byte>, slice<byte>> repl) {
    return re.replaceAll(src, ""u8, 2, 
    var srcʗ1 = src;
    (slice<byte> dst, slice<nint> match) => append(dst, repl(srcʗ1[(int)(match[0])..(int)(match[1])]).ꓸꓸꓸ));
}

// Bitmap used by func special to check whether a character needs to be escaped.
internal static array<byte> specialBytes;

// special reports whether byte b needs to be escaped by QuoteMeta.
internal static bool special(byte b) {
    return b < utf8.RuneSelf && (byte)(specialBytes[b % 16] & (1 << (int)((b / 16)))) != 0;
}

[GoInit] internal static void init() {
    foreach (var (_, b) in slice<byte>(@"\.+*?()|[]{}^$")) {
        specialBytes[b % 16] |= (byte)(1 << (int)((b / 16)));
    }
}

// QuoteMeta returns a string that escapes all regular expression metacharacters
// inside the argument text; the returned string is a regular expression matching
// the literal text.
public static @string QuoteMeta(@string s) {
    // A byte loop is correct because all metacharacters are ASCII.
    nint i = default!;
    for (i = 0; i < len(s); i++) {
        if (special(s[i])) {
            break;
        }
    }
    // No meta characters found, so return original string.
    if (i >= len(s)) {
        return s;
    }
    var b = new slice<byte>(2 * len(s) - i);
    copy(b, s[..(int)(i)]);
    nint j = i;
    for (; i < len(s); i++) {
        if (special(s[i])) {
            b[j] = (rune)'\\';
            j++;
        }
        b[j] = s[i];
        j++;
    }
    return ((@string)(b[..(int)(j)]));
}

// The number of capture values in the program may correspond
// to fewer capturing expressions than are in the regexp.
// For example, "(a){0}" turns into an empty program, so the
// maximum capture in the program is 0 but we need to return
// an expression for \1.  Pad appends -1s to the slice a as needed.
[GoRecv] internal static slice<nint> pad(this ref Regexp re, slice<nint> a) {
    if (a == default!) {
        // No match.
        return default!;
    }
    nint n = (1 + re.numSubexp) * 2;
    while (len(a) < n) {
        a = append(a, -1);
    }
    return a;
}

// allMatches calls deliver at most n times
// with the location of successive matches in the input text.
// The input text is b if non-nil, otherwise s.
[GoRecv] internal static void allMatches(this ref Regexp re, @string s, slice<byte> b, nint n, Action<slice<nint>> deliver) {
    nint end = default!;
    if (b == default!){
        end = len(s);
    } else {
        end = len(b);
    }
    for (nint pos = 0;nint i = 0;nint prevMatchEnd = -1; i < n && pos <= end; ) {
        var matches = re.doExecute(default!, b, s, pos, re.prog.NumCap, default!);
        if (len(matches) == 0) {
            break;
        }
        var accept = true;
        if (matches[1] == pos){
            // We've found an empty match.
            if (matches[0] == prevMatchEnd) {
                // We don't allow an empty match right
                // after a previous match, so ignore it.
                accept = false;
            }
            nint width = default!;
            if (b == default!){
                var @is = new inputString(str: s);
                (_, width) = @is.step(pos);
            } else {
                var ib = new inputBytes(str: b);
                (_, width) = ib.step(pos);
            }
            if (width > 0){
                pos += width;
            } else {
                pos = end + 1;
            }
        } else {
            pos = matches[1];
        }
        prevMatchEnd = matches[1];
        if (accept) {
            deliver(re.pad(matches));
            i++;
        }
    }
}

// Find returns a slice holding the text of the leftmost match in b of the regular expression.
// A return value of nil indicates no match.
[GoRecv] public static slice<byte> Find(this ref Regexp re, slice<byte> b) {
    array<nint> dstCap = new(2);
    var a = re.doExecute(default!, b, ""u8, 0, 2, dstCap[..0]);
    if (a == default!) {
        return default!;
    }
    return b.slice(a[0], a[1], a[1]);
}

// FindIndex returns a two-element slice of integers defining the location of
// the leftmost match in b of the regular expression. The match itself is at
// b[loc[0]:loc[1]].
// A return value of nil indicates no match.
[GoRecv] public static slice<nint> /*loc*/ FindIndex(this ref Regexp re, slice<byte> b) {
    slice<nint> loc = default!;

    var a = re.doExecute(default!, b, ""u8, 0, 2, default!);
    if (a == default!) {
        return default!;
    }
    return a[0..2];
}

// FindString returns a string holding the text of the leftmost match in s of the regular
// expression. If there is no match, the return value is an empty string,
// but it will also be empty if the regular expression successfully matches
// an empty string. Use [Regexp.FindStringIndex] or [Regexp.FindStringSubmatch] if it is
// necessary to distinguish these cases.
[GoRecv] public static @string FindString(this ref Regexp re, @string s) {
    array<nint> dstCap = new(2);
    var a = re.doExecute(default!, default!, s, 0, 2, dstCap[..0]);
    if (a == default!) {
        return ""u8;
    }
    return s[(int)(a[0])..(int)(a[1])];
}

// FindStringIndex returns a two-element slice of integers defining the
// location of the leftmost match in s of the regular expression. The match
// itself is at s[loc[0]:loc[1]].
// A return value of nil indicates no match.
[GoRecv] public static slice<nint> /*loc*/ FindStringIndex(this ref Regexp re, @string s) {
    slice<nint> loc = default!;

    var a = re.doExecute(default!, default!, s, 0, 2, default!);
    if (a == default!) {
        return default!;
    }
    return a[0..2];
}

// FindReaderIndex returns a two-element slice of integers defining the
// location of the leftmost match of the regular expression in text read from
// the [io.RuneReader]. The match text was found in the input stream at
// byte offset loc[0] through loc[1]-1.
// A return value of nil indicates no match.
[GoRecv] public static slice<nint> /*loc*/ FindReaderIndex(this ref Regexp re, io.RuneReader r) {
    slice<nint> loc = default!;

    var a = re.doExecute(r, default!, ""u8, 0, 2, default!);
    if (a == default!) {
        return default!;
    }
    return a[0..2];
}

// FindSubmatch returns a slice of slices holding the text of the leftmost
// match of the regular expression in b and the matches, if any, of its
// subexpressions, as defined by the 'Submatch' descriptions in the package
// comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<byte>> FindSubmatch(this ref Regexp re, slice<byte> b) {
    array<nint> dstCap = new(4);
    var a = re.doExecute(default!, b, ""u8, 0, re.prog.NumCap, dstCap[..0]);
    if (a == default!) {
        return default!;
    }
    var ret = new slice<slice<byte>>(1 + re.numSubexp);
    foreach (var (i, _) in ret) {
        if (2 * i < len(a) && a[2 * i] >= 0) {
            ret[i] = b.slice(a[2 * i], a[2 * i + 1], a[2 * i + 1]);
        }
    }
    return ret;
}

// Expand appends template to dst and returns the result; during the
// append, Expand replaces variables in the template with corresponding
// matches drawn from src. The match slice should have been returned by
// [Regexp.FindSubmatchIndex].
//
// In the template, a variable is denoted by a substring of the form
// $name or ${name}, where name is a non-empty sequence of letters,
// digits, and underscores. A purely numeric name like $1 refers to
// the submatch with the corresponding index; other names refer to
// capturing parentheses named with the (?P<name>...) syntax. A
// reference to an out of range or unmatched index or a name that is not
// present in the regular expression is replaced with an empty slice.
//
// In the $name form, name is taken to be as long as possible: $1x is
// equivalent to ${1x}, not ${1}x, and, $10 is equivalent to ${10}, not ${1}0.
//
// To insert a literal $ in the output, use $$ in the template.
[GoRecv] public static slice<byte> Expand(this ref Regexp re, slice<byte> dst, slice<byte> template, slice<byte> src, slice<nint> match) {
    return re.expand(dst, ((@string)template), src, ""u8, match);
}

// ExpandString is like [Regexp.Expand] but the template and source are strings.
// It appends to and returns a byte slice in order to give the calling
// code control over allocation.
[GoRecv] public static slice<byte> ExpandString(this ref Regexp re, slice<byte> dst, @string template, @string src, slice<nint> match) {
    return re.expand(dst, template, default!, src, match);
}

[GoRecv] internal static slice<byte> expand(this ref Regexp re, slice<byte> dst, @string template, slice<byte> bsrc, @string src, slice<nint> match) {
    while (len(template) > 0) {
        var (before, after, ok) = strings.Cut(template, "$"u8);
        if (!ok) {
            break;
        }
        dst = append(dst, before.ꓸꓸꓸ);
        template = after;
        if (template != ""u8 && template[0] == (rune)'$') {
            // Treat $$ as $.
            dst = append(dst, (rune)'$');
            template = template[1..];
            continue;
        }
        var (name, num, rest, ok) = extract(template);
        if (!ok) {
            // Malformed; treat $ as raw text.
            dst = append(dst, (rune)'$');
            continue;
        }
        template = rest;
        if (num >= 0){
            if (2 * num + 1 < len(match) && match[2 * num] >= 0) {
                if (bsrc != default!){
                    dst = append(dst, bsrc[(int)(match[2 * num])..(int)(match[2 * num + 1])].ꓸꓸꓸ);
                } else {
                    dst = append(dst, src[(int)(match[2 * num])..(int)(match[2 * num + 1])].ꓸꓸꓸ);
                }
            }
        } else {
            foreach (var (i, namei) in re.subexpNames) {
                if (name == namei && 2 * i + 1 < len(match) && match[2 * i] >= 0) {
                    if (bsrc != default!){
                        dst = append(dst, bsrc[(int)(match[2 * i])..(int)(match[2 * i + 1])].ꓸꓸꓸ);
                    } else {
                        dst = append(dst, src[(int)(match[2 * i])..(int)(match[2 * i + 1])].ꓸꓸꓸ);
                    }
                    break;
                }
            }
        }
    }
    dst = append(dst, template.ꓸꓸꓸ);
    return dst;
}

// extract returns the name from a leading "name" or "{name}" in str.
// (The $ has already been removed by the caller.)
// If it is a number, extract returns num set to that number; otherwise num = -1.
internal static (@string name, nint num, @string rest, bool ok) extract(@string str) {
    @string name = default!;
    nint num = default!;
    @string rest = default!;
    bool ok = default!;

    if (str == ""u8) {
        return (name, num, rest, ok);
    }
    var brace = false;
    if (str[0] == (rune)'{') {
        brace = true;
        str = str[1..];
    }
    nint i = 0;
    while (i < len(str)) {
        var (rune, size) = utf8.DecodeRuneInString(str[(int)(i)..]);
        if (!unicode.IsLetter(rune) && !unicode.IsDigit(rune) && rune != (rune)'_') {
            break;
        }
        i += size;
    }
    if (i == 0) {
        // empty name is not okay
        return (name, num, rest, ok);
    }
    name = str[..(int)(i)];
    if (brace) {
        if (i >= len(str) || str[i] != (rune)'}') {
            // missing closing brace
            return (name, num, rest, ok);
        }
        i++;
    }
    // Parse number.
    num = 0;
    for (nint iΔ1 = 0; iΔ1 < len(name); iΔ1++) {
        if (name[iΔ1] < (rune)'0' || (rune)'9' < name[iΔ1] || num >= 1e8F) {
            num = -1;
            break;
        }
        num = num * 10 + ((nint)name[iΔ1]) - (rune)'0';
    }
    // Disallow leading zeros.
    if (name[0] == (rune)'0' && len(name) > 1) {
        num = -1;
    }
    rest = str[(int)(i)..];
    ok = true;
    return (name, num, rest, ok);
}

// FindSubmatchIndex returns a slice holding the index pairs identifying the
// leftmost match of the regular expression in b and the matches, if any, of
// its subexpressions, as defined by the 'Submatch' and 'Index' descriptions
// in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<nint> FindSubmatchIndex(this ref Regexp re, slice<byte> b) {
    return re.pad(re.doExecute(default!, b, ""u8, 0, re.prog.NumCap, default!));
}

// FindStringSubmatch returns a slice of strings holding the text of the
// leftmost match of the regular expression in s and the matches, if any, of
// its subexpressions, as defined by the 'Submatch' description in the
// package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<@string> FindStringSubmatch(this ref Regexp re, @string s) {
    array<nint> dstCap = new(4);
    var a = re.doExecute(default!, default!, s, 0, re.prog.NumCap, dstCap[..0]);
    if (a == default!) {
        return default!;
    }
    var ret = new slice<@string>(1 + re.numSubexp);
    foreach (var (i, _) in ret) {
        if (2 * i < len(a) && a[2 * i] >= 0) {
            ret[i] = s[(int)(a[2 * i])..(int)(a[2 * i + 1])];
        }
    }
    return ret;
}

// FindStringSubmatchIndex returns a slice holding the index pairs
// identifying the leftmost match of the regular expression in s and the
// matches, if any, of its subexpressions, as defined by the 'Submatch' and
// 'Index' descriptions in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<nint> FindStringSubmatchIndex(this ref Regexp re, @string s) {
    return re.pad(re.doExecute(default!, default!, s, 0, re.prog.NumCap, default!));
}

// FindReaderSubmatchIndex returns a slice holding the index pairs
// identifying the leftmost match of the regular expression of text read by
// the [io.RuneReader], and the matches, if any, of its subexpressions, as defined
// by the 'Submatch' and 'Index' descriptions in the package comment. A
// return value of nil indicates no match.
[GoRecv] public static slice<nint> FindReaderSubmatchIndex(this ref Regexp re, io.RuneReader r) {
    return re.pad(re.doExecute(r, default!, ""u8, 0, re.prog.NumCap, default!));
}

internal static readonly UntypedInt startSize = 10; // The size at which to start a slice in the 'All' routines.

// FindAll is the 'All' version of [Regexp.Find]; it returns a slice of all successive
// matches of the expression, as defined by the 'All' description in the
// package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<byte>> FindAll(this ref Regexp re, slice<byte> b, nint n) {
    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<byte>> result = default!;
    re.allMatches(""u8, b, n, 
    var bʗ1 = b;
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<byte>>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, bʗ1.slice(match[0], match[1], match[1]));
    });
    return result;
}

// FindAllIndex is the 'All' version of [Regexp.FindIndex]; it returns a slice of all
// successive matches of the expression, as defined by the 'All' description
// in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<nint>> FindAllIndex(this ref Regexp re, slice<byte> b, nint n) {
    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<nint>> result = default!;
    re.allMatches(""u8, b, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<nint>>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, match[0..2]);
    });
    return result;
}

// FindAllString is the 'All' version of [Regexp.FindString]; it returns a slice of all
// successive matches of the expression, as defined by the 'All' description
// in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<@string> FindAllString(this ref Regexp re, @string s, nint n) {
    if (n < 0) {
        n = len(s) + 1;
    }
    slice<@string> result = default!;
    re.allMatches(s, default!, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<@string>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, s[(int)(match[0])..(int)(match[1])]);
    });
    return result;
}

// FindAllStringIndex is the 'All' version of [Regexp.FindStringIndex]; it returns a
// slice of all successive matches of the expression, as defined by the 'All'
// description in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<nint>> FindAllStringIndex(this ref Regexp re, @string s, nint n) {
    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<nint>> result = default!;
    re.allMatches(s, default!, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<nint>>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, match[0..2]);
    });
    return result;
}

// FindAllSubmatch is the 'All' version of [Regexp.FindSubmatch]; it returns a slice
// of all successive matches of the expression, as defined by the 'All'
// description in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<slice<byte>>> FindAllSubmatch(this ref Regexp re, slice<byte> b, nint n) {
    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<slice<byte>>> result = default!;
    re.allMatches(""u8, b, n, 
    var bʗ1 = b;
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<slice<byte>>>(0, startSize);
        }
        var Δslice = new slice<slice<byte>>(len(match) / 2);
        foreach (var (j, _) in Δslice) {
            if (match[2 * j] >= 0) {
                Δslice[j] = bʗ1.slice(match[2 * j], match[2 * j + 1], match[2 * j + 1]);
            }
        }
        resultʗ1 = append(resultʗ1, Δslice);
    });
    return result;
}

// FindAllSubmatchIndex is the 'All' version of [Regexp.FindSubmatchIndex]; it returns
// a slice of all successive matches of the expression, as defined by the
// 'All' description in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<nint>> FindAllSubmatchIndex(this ref Regexp re, slice<byte> b, nint n) {
    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<nint>> result = default!;
    re.allMatches(""u8, b, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<nint>>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, match);
    });
    return result;
}

// FindAllStringSubmatch is the 'All' version of [Regexp.FindStringSubmatch]; it
// returns a slice of all successive matches of the expression, as defined by
// the 'All' description in the package comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<@string>> FindAllStringSubmatch(this ref Regexp re, @string s, nint n) {
    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<@string>> result = default!;
    re.allMatches(s, default!, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<@string>>(0, startSize);
        }
        var Δslice = new slice<@string>(len(match) / 2);
        foreach (var (j, _) in Δslice) {
            if (match[2 * j] >= 0) {
                Δslice[j] = s[(int)(match[2 * j])..(int)(match[2 * j + 1])];
            }
        }
        resultʗ1 = append(resultʗ1, Δslice);
    });
    return result;
}

// FindAllStringSubmatchIndex is the 'All' version of
// [Regexp.FindStringSubmatchIndex]; it returns a slice of all successive matches of
// the expression, as defined by the 'All' description in the package
// comment.
// A return value of nil indicates no match.
[GoRecv] public static slice<slice<nint>> FindAllStringSubmatchIndex(this ref Regexp re, @string s, nint n) {
    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<nint>> result = default!;
    re.allMatches(s, default!, n, 
    var resultʗ1 = result;
    (slice<nint> match) => {
        if (resultʗ1 == default!) {
            resultʗ1 = new slice<slice<nint>>(0, startSize);
        }
        resultʗ1 = append(resultʗ1, match);
    });
    return result;
}

// Split slices s into substrings separated by the expression and returns a slice of
// the substrings between those expression matches.
//
// The slice returned by this method consists of all the substrings of s
// not contained in the slice returned by [Regexp.FindAllString]. When called on an expression
// that contains no metacharacters, it is equivalent to [strings.SplitN].
//
// Example:
//
//	s := regexp.MustCompile("a*").Split("abaabaccadaaae", 5)
//	// s: ["", "b", "b", "c", "cadaaae"]
//
// The count determines the number of substrings to return:
//   - n > 0: at most n substrings; the last substring will be the unsplit remainder;
//   - n == 0: the result is nil (zero substrings);
//   - n < 0: all substrings.
[GoRecv] public static slice<@string> Split(this ref Regexp re, @string s, nint n) {
    if (n == 0) {
        return default!;
    }
    if (len(re.expr) > 0 && len(s) == 0) {
        return new @string[]{""}.slice();
    }
    var matches = re.FindAllStringIndex(s, n);
    var strings = new slice<@string>(0, len(matches));
    nint beg = 0;
    nint end = 0;
    foreach (var (_, match) in matches) {
        if (n > 0 && len(strings) >= n - 1) {
            break;
        }
        end = match[0];
        if (match[1] != 0) {
            strings = append(strings, s[(int)(beg)..(int)(end)]);
        }
        beg = match[1];
    }
    if (end != len(s)) {
        strings = append(strings, s[(int)(beg)..]);
    }
    return strings;
}

// MarshalText implements [encoding.TextMarshaler]. The output
// matches that of calling the [Regexp.String] method.
//
// Note that the output is lossy in some cases: This method does not indicate
// POSIX regular expressions (i.e. those compiled by calling [CompilePOSIX]), or
// those for which the [Regexp.Longest] method has been called.
[GoRecv] public static (slice<byte>, error) MarshalText(this ref Regexp re) {
    return (slice<byte>(re.String()), default!);
}

// UnmarshalText implements [encoding.TextUnmarshaler] by calling
// [Compile] on the encoded value.
[GoRecv] public static error UnmarshalText(this ref Regexp re, slice<byte> text) {
    (newRE, err) = Compile(((@string)text));
    if (err != default!) {
        return err;
    }
    re = newRE.val;
    return default!;
}

} // end regexp_package
