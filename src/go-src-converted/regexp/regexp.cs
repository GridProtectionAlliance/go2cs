// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package regexp implements regular expression search.
//
// The syntax of the regular expressions accepted is the same
// general syntax used by Perl, Python, and other languages.
// More precisely, it is the syntax accepted by RE2 and described at
// https://golang.org/s/re2syntax, except for \C.
// For an overview of the syntax, run
//   go doc regexp/syntax
//
// The regexp implementation provided by this package is
// guaranteed to run in time linear in the size of the input.
// (This is a property not guaranteed by most open source
// implementations of regular expressions.) For more information
// about this property, see
//    https://swtch.com/~rsc/regexp/regexp1.html
// or any book about automata theory.
//
// All characters are UTF-8-encoded code points.
//
// There are 16 methods of Regexp that match a regular expression and identify
// the matched text. Their names are matched by this regular expression:
//
//    Find(All)?(String)?(Submatch)?(Index)?
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
// parenthesis. Submatch 0 is the match of the entire expression, submatch 1
// the match of the first parenthesized subexpression, and so on.
//
// If 'Index' is present, matches and submatches are identified by byte index
// pairs within the input string: result[2*n:2*n+1] identifies the indexes of
// the nth submatch. The pair for n==0 identifies the match of the entire
// expression. If 'Index' is not present, the match is identified by the text
// of the match/submatch. If an index is negative or text is nil, it means that
// subexpression did not match any string in the input. For 'String' versions
// an empty string means either no match or an empty match.
//
// There is also a subset of the methods that can be applied to text read
// from a RuneReader:
//
//    MatchReader, FindReaderIndex, FindReaderSubmatchIndex
//
// This set may grow. Note that regular expression matches may need to
// examine text beyond the text returned by a match, so the methods that
// match text from a RuneReader may read arbitrarily far into the input
// before returning.
//
// (There are a few other methods that do not match this pattern.)
//

// package regexp -- go2cs converted at 2022 March 13 05:38:13 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Program Files\Go\src\regexp\regexp.go
namespace go;

using bytes = bytes_package;
using io = io_package;
using syntax = regexp.syntax_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// Regexp is the representation of a compiled regular expression.
// A Regexp is safe for concurrent use by multiple goroutines,
// except for configuration methods, such as Longest.

using System;
public static partial class regexp_package {

public partial struct Regexp {
    public @string expr; // as passed to Compile
    public ptr<syntax.Prog> prog; // compiled program
    public ptr<onePassProg> onepass; // onepass program or nil
    public nint numSubexp;
    public nint maxBitStateLen;
    public slice<@string> subexpNames;
    public @string prefix; // required prefix in unanchored matches
    public slice<byte> prefixBytes; // prefix, as a []byte
    public int prefixRune; // first rune in prefix
    public uint prefixEnd; // pc for last rune in prefix
    public nint mpool; // pool for machines
    public nint matchcap; // size of recorded match lengths
    public bool prefixComplete; // prefix is the entire regexp
    public syntax.EmptyOp cond; // empty-width conditions required at start of match
    public nint minInputLen; // minimum length of the input in bytes

// This field can be modified by the Longest method,
// but it is otherwise read-only.
    public bool longest; // whether regexp prefers leftmost-longest match
}

// String returns the source text used to compile the regular expression.
private static @string String(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    return re.expr;
}

// Copy returns a new Regexp object copied from re.
// Calling Longest on one copy does not affect another.
//
// Deprecated: In earlier releases, when using a Regexp in multiple goroutines,
// giving each goroutine its own copy helped to avoid lock contention.
// As of Go 1.12, using Copy is no longer necessary to avoid lock contention.
// Copy may still be appropriate if the reason for its use is to make
// two copies with different Longest settings.
private static ptr<Regexp> Copy(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    ref var re2 = ref heap(re.val, out ptr<var> _addr_re2);
    return _addr__addr_re2!;
}

// Compile parses a regular expression and returns, if successful,
// a Regexp object that can be used to match against text.
//
// When matching against text, the regexp returns a match that
// begins as early as possible in the input (leftmost), and among those
// it chooses the one that a backtracking search would have found first.
// This so-called leftmost-first matching is the same semantics
// that Perl, Python, and other implementations use, although this
// package implements it without the expense of backtracking.
// For POSIX leftmost-longest matching, see CompilePOSIX.
public static (ptr<Regexp>, error) Compile(@string expr) {
    ptr<Regexp> _p0 = default!;
    error _p0 = default!;

    return _addr_compile(expr, syntax.Perl, false)!;
}

// CompilePOSIX is like Compile but restricts the regular expression
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
public static (ptr<Regexp>, error) CompilePOSIX(@string expr) {
    ptr<Regexp> _p0 = default!;
    error _p0 = default!;

    return _addr_compile(expr, syntax.POSIX, true)!;
}

// Longest makes future searches prefer the leftmost-longest match.
// That is, when matching against text, the regexp returns a match that
// begins as early as possible in the input (leftmost), and among those
// it chooses a match that is as long as possible.
// This method modifies the Regexp and may not be called concurrently
// with any other methods.
private static void Longest(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    re.longest = true;
}

private static (ptr<Regexp>, error) compile(@string expr, syntax.Flags mode, bool longest) {
    ptr<Regexp> _p0 = default!;
    error _p0 = default!;

    var (re, err) = syntax.Parse(expr, mode);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var maxCap = re.MaxCap();
    var capNames = re.CapNames();

    re = re.Simplify();
    var (prog, err) = syntax.Compile(re);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var matchcap = prog.NumCap;
    if (matchcap < 2) {
        matchcap = 2;
    }
    ptr<Regexp> regexp = addr(new Regexp(expr:expr,prog:prog,onepass:compileOnePass(prog),numSubexp:maxCap,subexpNames:capNames,cond:prog.StartCond(),longest:longest,matchcap:matchcap,minInputLen:minInputLen(re),));
    if (regexp.onepass == null) {
        regexp.prefix, regexp.prefixComplete = prog.Prefix();
        regexp.maxBitStateLen = maxBitStateLen(prog);
    }
    else
 {
        regexp.prefix, regexp.prefixComplete, regexp.prefixEnd = onePassPrefix(prog);
    }
    if (regexp.prefix != "") { 
        // TODO(rsc): Remove this allocation by adding
        // IndexString to package bytes.
        regexp.prefixBytes = (slice<byte>)regexp.prefix;
        regexp.prefixRune, _ = utf8.DecodeRuneInString(regexp.prefix);
    }
    var n = len(prog.Inst);
    nint i = 0;
    while (matchSize[i] != 0 && matchSize[i] < n) {
        i++;
    }
    regexp.mpool = i;

    return (_addr_regexp!, error.As(null!)!);
}

// Pools of *machine for use during (*Regexp).doExecute,
// split up by the size of the execution queues.
// matchPool[i] machines have queue size matchSize[i].
// On a 64-bit system each queue entry is 16 bytes,
// so matchPool[0] has 16*2*128 = 4kB queues, etc.
// The final matchPool is a catch-all for very large queues.
private static array<nint> matchSize = new array<nint>(new nint[] { 128, 512, 2048, 16384, 0 });private static array<sync.Pool> matchPool = new array<sync.Pool>(len(matchSize));

// get returns a machine to use for matching re.
// It uses the re's machine cache if possible, to avoid
// unnecessary allocation.
private static ptr<machine> get(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    ptr<machine> (m, ok) = matchPool[re.mpool].Get()._<ptr<machine>>();
    if (!ok) {
        m = @new<machine>();
    }
    m.re = re;
    m.p = re.prog;
    if (cap(m.matchcap) < re.matchcap) {
        m.matchcap = make_slice<nint>(re.matchcap);
        foreach (var (_, t) in m.pool) {
            t.cap = make_slice<nint>(re.matchcap);
        }
    }
    var n = matchSize[re.mpool];
    if (n == 0) { // large pool
        n = len(re.prog.Inst);
    }
    if (len(m.q0.sparse) < n) {
        m.q0 = new queue(make([]uint32,n),make([]entry,0,n));
        m.q1 = new queue(make([]uint32,n),make([]entry,0,n));
    }
    return _addr_m!;
}

// put returns a machine to the correct machine pool.
private static void put(this ptr<Regexp> _addr_re, ptr<machine> _addr_m) {
    ref Regexp re = ref _addr_re.val;
    ref machine m = ref _addr_m.val;

    m.re = null;
    m.p = null;
    m.inputs.clear();
    matchPool[re.mpool].Put(m);
}

// minInputLen walks the regexp to find the minimum length of any matchable input
private static nint minInputLen(ptr<syntax.Regexp> _addr_re) {
    ref syntax.Regexp re = ref _addr_re.val;


    if (re.Op == syntax.OpAnyChar || re.Op == syntax.OpAnyCharNotNL || re.Op == syntax.OpCharClass) 
        return 1;
    else if (re.Op == syntax.OpLiteral) 
        nint l = 0;
        foreach (var (_, r) in re.Rune) {
            l += utf8.RuneLen(r);
        }        return l;
    else if (re.Op == syntax.OpCapture || re.Op == syntax.OpPlus) 
        return minInputLen(_addr_re.Sub[0]);
    else if (re.Op == syntax.OpRepeat) 
        return re.Min * minInputLen(_addr_re.Sub[0]);
    else if (re.Op == syntax.OpConcat) 
        l = 0;
        {
            var sub__prev1 = sub;

            foreach (var (_, __sub) in re.Sub) {
                sub = __sub;
                l += minInputLen(_addr_sub);
            }

            sub = sub__prev1;
        }

        return l;
    else if (re.Op == syntax.OpAlternate) 
        l = minInputLen(_addr_re.Sub[0]);
        nint lnext = default;
        {
            var sub__prev1 = sub;

            foreach (var (_, __sub) in re.Sub[(int)1..]) {
                sub = __sub;
                lnext = minInputLen(_addr_sub);
                if (lnext < l) {
                    l = lnext;
                }
            }

            sub = sub__prev1;
        }

        return l;
    else 
        return 0;
    }

// MustCompile is like Compile but panics if the expression cannot be parsed.
// It simplifies safe initialization of global variables holding compiled regular
// expressions.
public static ptr<Regexp> MustCompile(@string str) => func((_, panic, _) => {
    var (regexp, err) = Compile(str);
    if (err != null) {
        panic("regexp: Compile(" + quote(str) + "): " + err.Error());
    }
    return _addr_regexp!;
});

// MustCompilePOSIX is like CompilePOSIX but panics if the expression cannot be parsed.
// It simplifies safe initialization of global variables holding compiled regular
// expressions.
public static ptr<Regexp> MustCompilePOSIX(@string str) => func((_, panic, _) => {
    var (regexp, err) = CompilePOSIX(str);
    if (err != null) {
        panic("regexp: CompilePOSIX(" + quote(str) + "): " + err.Error());
    }
    return _addr_regexp!;
});

private static @string quote(@string s) {
    if (strconv.CanBackquote(s)) {
        return "`" + s + "`";
    }
    return strconv.Quote(s);
}

// NumSubexp returns the number of parenthesized subexpressions in this Regexp.
private static nint NumSubexp(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    return re.numSubexp;
}

// SubexpNames returns the names of the parenthesized subexpressions
// in this Regexp. The name for the first sub-expression is names[1],
// so that if m is a match slice, the name for m[i] is SubexpNames()[i].
// Since the Regexp as a whole cannot be named, names[0] is always
// the empty string. The slice should not be modified.
private static slice<@string> SubexpNames(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    return re.subexpNames;
}

// SubexpIndex returns the index of the first subexpression with the given name,
// or -1 if there is no subexpression with that name.
//
// Note that multiple subexpressions can be written using the same name, as in
// (?P<bob>a+)(?P<bob>b+), which declares two subexpressions named "bob".
// In this case, SubexpIndex returns the index of the leftmost such subexpression
// in the regular expression.
private static nint SubexpIndex(this ptr<Regexp> _addr_re, @string name) {
    ref Regexp re = ref _addr_re.val;

    if (name != "") {
        foreach (var (i, s) in re.subexpNames) {
            if (name == s) {
                return i;
            }
        }
    }
    return -1;
}

private static readonly int endOfText = -1;

// input abstracts different representations of the input text. It provides
// one-character lookahead.


// input abstracts different representations of the input text. It provides
// one-character lookahead.
private partial interface input {
    lazyFlag step(nint pos); // advance one rune
    lazyFlag canCheckPrefix(); // can we look ahead without losing info?
    lazyFlag hasPrefix(ptr<Regexp> re);
    lazyFlag index(ptr<Regexp> re, nint pos);
    lazyFlag context(nint pos);
}

// inputString scans a string.
private partial struct inputString {
    public @string str;
}

private static (int, nint) step(this ptr<inputString> _addr_i, nint pos) {
    int _p0 = default;
    nint _p0 = default;
    ref inputString i = ref _addr_i.val;

    if (pos < len(i.str)) {
        var c = i.str[pos];
        if (c < utf8.RuneSelf) {
            return (rune(c), 1);
        }
        return utf8.DecodeRuneInString(i.str[(int)pos..]);
    }
    return (endOfText, 0);
}

private static bool canCheckPrefix(this ptr<inputString> _addr_i) {
    ref inputString i = ref _addr_i.val;

    return true;
}

private static bool hasPrefix(this ptr<inputString> _addr_i, ptr<Regexp> _addr_re) {
    ref inputString i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return strings.HasPrefix(i.str, re.prefix);
}

private static nint index(this ptr<inputString> _addr_i, ptr<Regexp> _addr_re, nint pos) {
    ref inputString i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return strings.Index(i.str[(int)pos..], re.prefix);
}

private static lazyFlag context(this ptr<inputString> _addr_i, nint pos) {
    ref inputString i = ref _addr_i.val;

    var r1 = endOfText;
    var r2 = endOfText; 
    // 0 < pos && pos <= len(i.str)
    if (uint(pos - 1) < uint(len(i.str))) {
        r1 = rune(i.str[pos - 1]);
        if (r1 >= utf8.RuneSelf) {
            r1, _ = utf8.DecodeLastRuneInString(i.str[..(int)pos]);
        }
    }
    if (uint(pos) < uint(len(i.str))) {
        r2 = rune(i.str[pos]);
        if (r2 >= utf8.RuneSelf) {
            r2, _ = utf8.DecodeRuneInString(i.str[(int)pos..]);
        }
    }
    return newLazyFlag(r1, r2);
}

// inputBytes scans a byte slice.
private partial struct inputBytes {
    public slice<byte> str;
}

private static (int, nint) step(this ptr<inputBytes> _addr_i, nint pos) {
    int _p0 = default;
    nint _p0 = default;
    ref inputBytes i = ref _addr_i.val;

    if (pos < len(i.str)) {
        var c = i.str[pos];
        if (c < utf8.RuneSelf) {
            return (rune(c), 1);
        }
        return utf8.DecodeRune(i.str[(int)pos..]);
    }
    return (endOfText, 0);
}

private static bool canCheckPrefix(this ptr<inputBytes> _addr_i) {
    ref inputBytes i = ref _addr_i.val;

    return true;
}

private static bool hasPrefix(this ptr<inputBytes> _addr_i, ptr<Regexp> _addr_re) {
    ref inputBytes i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return bytes.HasPrefix(i.str, re.prefixBytes);
}

private static nint index(this ptr<inputBytes> _addr_i, ptr<Regexp> _addr_re, nint pos) {
    ref inputBytes i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return bytes.Index(i.str[(int)pos..], re.prefixBytes);
}

private static lazyFlag context(this ptr<inputBytes> _addr_i, nint pos) {
    ref inputBytes i = ref _addr_i.val;

    var r1 = endOfText;
    var r2 = endOfText; 
    // 0 < pos && pos <= len(i.str)
    if (uint(pos - 1) < uint(len(i.str))) {
        r1 = rune(i.str[pos - 1]);
        if (r1 >= utf8.RuneSelf) {
            r1, _ = utf8.DecodeLastRune(i.str[..(int)pos]);
        }
    }
    if (uint(pos) < uint(len(i.str))) {
        r2 = rune(i.str[pos]);
        if (r2 >= utf8.RuneSelf) {
            r2, _ = utf8.DecodeRune(i.str[(int)pos..]);
        }
    }
    return newLazyFlag(r1, r2);
}

// inputReader scans a RuneReader.
private partial struct inputReader {
    public io.RuneReader r;
    public bool atEOT;
    public nint pos;
}

private static (int, nint) step(this ptr<inputReader> _addr_i, nint pos) {
    int _p0 = default;
    nint _p0 = default;
    ref inputReader i = ref _addr_i.val;

    if (!i.atEOT && pos != i.pos) {
        return (endOfText, 0);
    }
    var (r, w, err) = i.r.ReadRune();
    if (err != null) {
        i.atEOT = true;
        return (endOfText, 0);
    }
    i.pos += w;
    return (r, w);
}

private static bool canCheckPrefix(this ptr<inputReader> _addr_i) {
    ref inputReader i = ref _addr_i.val;

    return false;
}

private static bool hasPrefix(this ptr<inputReader> _addr_i, ptr<Regexp> _addr_re) {
    ref inputReader i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return false;
}

private static nint index(this ptr<inputReader> _addr_i, ptr<Regexp> _addr_re, nint pos) {
    ref inputReader i = ref _addr_i.val;
    ref Regexp re = ref _addr_re.val;

    return -1;
}

private static lazyFlag context(this ptr<inputReader> _addr_i, nint pos) {
    ref inputReader i = ref _addr_i.val;

    return 0; // not used
}

// LiteralPrefix returns a literal string that must begin any match
// of the regular expression re. It returns the boolean true if the
// literal string comprises the entire regular expression.
private static (@string, bool) LiteralPrefix(this ptr<Regexp> _addr_re) {
    @string prefix = default;
    bool complete = default;
    ref Regexp re = ref _addr_re.val;

    return (re.prefix, re.prefixComplete);
}

// MatchReader reports whether the text returned by the RuneReader
// contains any match of the regular expression re.
private static bool MatchReader(this ptr<Regexp> _addr_re, io.RuneReader r) {
    ref Regexp re = ref _addr_re.val;

    return re.doMatch(r, null, "");
}

// MatchString reports whether the string s
// contains any match of the regular expression re.
private static bool MatchString(this ptr<Regexp> _addr_re, @string s) {
    ref Regexp re = ref _addr_re.val;

    return re.doMatch(null, null, s);
}

// Match reports whether the byte slice b
// contains any match of the regular expression re.
private static bool Match(this ptr<Regexp> _addr_re, slice<byte> b) {
    ref Regexp re = ref _addr_re.val;

    return re.doMatch(null, b, "");
}

// MatchReader reports whether the text returned by the RuneReader
// contains any match of the regular expression pattern.
// More complicated queries need to use Compile and the full Regexp interface.
public static (bool, error) MatchReader(@string pattern, io.RuneReader r) {
    bool matched = default;
    error err = default!;

    var (re, err) = Compile(pattern);
    if (err != null) {
        return (false, error.As(err)!);
    }
    return (re.MatchReader(r), error.As(null!)!);
}

// MatchString reports whether the string s
// contains any match of the regular expression pattern.
// More complicated queries need to use Compile and the full Regexp interface.
public static (bool, error) MatchString(@string pattern, @string s) {
    bool matched = default;
    error err = default!;

    var (re, err) = Compile(pattern);
    if (err != null) {
        return (false, error.As(err)!);
    }
    return (re.MatchString(s), error.As(null!)!);
}

// Match reports whether the byte slice b
// contains any match of the regular expression pattern.
// More complicated queries need to use Compile and the full Regexp interface.
public static (bool, error) Match(@string pattern, slice<byte> b) {
    bool matched = default;
    error err = default!;

    var (re, err) = Compile(pattern);
    if (err != null) {
        return (false, error.As(err)!);
    }
    return (re.Match(b), error.As(null!)!);
}

// ReplaceAllString returns a copy of src, replacing matches of the Regexp
// with the replacement string repl. Inside repl, $ signs are interpreted as
// in Expand, so for instance $1 represents the text of the first submatch.
private static @string ReplaceAllString(this ptr<Regexp> _addr_re, @string src, @string repl) {
    ref Regexp re = ref _addr_re.val;

    nint n = 2;
    if (strings.Contains(repl, "$")) {
        n = 2 * (re.numSubexp + 1);
    }
    var b = re.replaceAll(null, src, n, (dst, match) => re.expand(dst, repl, null, src, match));
    return string(b);
}

// ReplaceAllLiteralString returns a copy of src, replacing matches of the Regexp
// with the replacement string repl. The replacement repl is substituted directly,
// without using Expand.
private static @string ReplaceAllLiteralString(this ptr<Regexp> _addr_re, @string src, @string repl) {
    ref Regexp re = ref _addr_re.val;

    return string(re.replaceAll(null, src, 2, (dst, match) => append(dst, repl)));
}

// ReplaceAllStringFunc returns a copy of src in which all matches of the
// Regexp have been replaced by the return value of function repl applied
// to the matched substring. The replacement returned by repl is substituted
// directly, without using Expand.
private static @string ReplaceAllStringFunc(this ptr<Regexp> _addr_re, @string src, Func<@string, @string> repl) {
    ref Regexp re = ref _addr_re.val;

    var b = re.replaceAll(null, src, 2, (dst, match) => append(dst, repl(src[(int)match[0]..(int)match[1]])));
    return string(b);
}

private static slice<byte> replaceAll(this ptr<Regexp> _addr_re, slice<byte> bsrc, @string src, nint nmatch, Func<slice<byte>, slice<nint>, slice<byte>> repl) {
    ref Regexp re = ref _addr_re.val;

    nint lastMatchEnd = 0; // end position of the most recent match
    nint searchPos = 0; // position where we next look for a match
    slice<byte> buf = default;
    nint endPos = default;
    if (bsrc != null) {
        endPos = len(bsrc);
    }
    else
 {
        endPos = len(src);
    }
    if (nmatch > re.prog.NumCap) {
        nmatch = re.prog.NumCap;
    }
    array<nint> dstCap = new array<nint>(2);
    while (searchPos <= endPos) {
        var a = re.doExecute(null, bsrc, src, searchPos, nmatch, dstCap[..(int)0]);
        if (len(a) == 0) {
            break; // no more matches
        }
        if (bsrc != null) {
            buf = append(buf, bsrc[(int)lastMatchEnd..(int)a[0]]);
        }
        else
 {
            buf = append(buf, src[(int)lastMatchEnd..(int)a[0]]);
        }
        if (a[1] > lastMatchEnd || a[0] == 0) {
            buf = repl(buf, a);
        }
        lastMatchEnd = a[1]; 

        // Advance past this match; always advance at least one character.
        nint width = default;
        if (bsrc != null) {
            _, width = utf8.DecodeRune(bsrc[(int)searchPos..]);
        }
        else
 {
            _, width = utf8.DecodeRuneInString(src[(int)searchPos..]);
        }
        if (searchPos + width > a[1]) {
            searchPos += width;
        }
        else if (searchPos + 1 > a[1]) { 
            // This clause is only needed at the end of the input
            // string. In that case, DecodeRuneInString returns width=0.
            searchPos++;
        }
        else
 {
            searchPos = a[1];
        }
    } 

    // Copy the unmatched characters after the last match.
    if (bsrc != null) {
        buf = append(buf, bsrc[(int)lastMatchEnd..]);
    }
    else
 {
        buf = append(buf, src[(int)lastMatchEnd..]);
    }
    return buf;
}

// ReplaceAll returns a copy of src, replacing matches of the Regexp
// with the replacement text repl. Inside repl, $ signs are interpreted as
// in Expand, so for instance $1 represents the text of the first submatch.
private static slice<byte> ReplaceAll(this ptr<Regexp> _addr_re, slice<byte> src, slice<byte> repl) {
    ref Regexp re = ref _addr_re.val;

    nint n = 2;
    if (bytes.IndexByte(repl, '$') >= 0) {
        n = 2 * (re.numSubexp + 1);
    }
    @string srepl = "";
    var b = re.replaceAll(src, "", n, (dst, match) => {
        if (len(srepl) != len(repl)) {
            srepl = string(repl);
        }
        return re.expand(dst, srepl, src, "", match);
    });
    return b;
}

// ReplaceAllLiteral returns a copy of src, replacing matches of the Regexp
// with the replacement bytes repl. The replacement repl is substituted directly,
// without using Expand.
private static slice<byte> ReplaceAllLiteral(this ptr<Regexp> _addr_re, slice<byte> src, slice<byte> repl) {
    ref Regexp re = ref _addr_re.val;

    return re.replaceAll(src, "", 2, (dst, match) => append(dst, repl));
}

// ReplaceAllFunc returns a copy of src in which all matches of the
// Regexp have been replaced by the return value of function repl applied
// to the matched byte slice. The replacement returned by repl is substituted
// directly, without using Expand.
private static slice<byte> ReplaceAllFunc(this ptr<Regexp> _addr_re, slice<byte> src, Func<slice<byte>, slice<byte>> repl) {
    ref Regexp re = ref _addr_re.val;

    return re.replaceAll(src, "", 2, (dst, match) => append(dst, repl(src[(int)match[0]..(int)match[1]])));
}

// Bitmap used by func special to check whether a character needs to be escaped.
private static array<byte> specialBytes = new array<byte>(16);

// special reports whether byte b needs to be escaped by QuoteMeta.
private static bool special(byte b) {
    return b < utf8.RuneSelf && specialBytes[b % 16] & (1 << (int)((b / 16))) != 0;
}

private static void init() {
    foreach (var (_, b) in (slice<byte>)"\\.+*?()|[]{}^$") {
        specialBytes[b % 16] |= 1 << (int)((b / 16));
    }
}

// QuoteMeta returns a string that escapes all regular expression metacharacters
// inside the argument text; the returned string is a regular expression matching
// the literal text.
public static @string QuoteMeta(@string s) { 
    // A byte loop is correct because all metacharacters are ASCII.
    nint i = default;
    for (i = 0; i < len(s); i++) {
        if (special(s[i])) {
            break;
        }
    } 
    // No meta characters found, so return original string.
    if (i >= len(s)) {
        return s;
    }
    var b = make_slice<byte>(2 * len(s) - i);
    copy(b, s[..(int)i]);
    var j = i;
    while (i < len(s)) {
        if (special(s[i])) {
            b[j] = '\\';
            j++;
        i++;
        }
        b[j] = s[i];
        j++;
    }
    return string(b[..(int)j]);
}

// The number of capture values in the program may correspond
// to fewer capturing expressions than are in the regexp.
// For example, "(a){0}" turns into an empty program, so the
// maximum capture in the program is 0 but we need to return
// an expression for \1.  Pad appends -1s to the slice a as needed.
private static slice<nint> pad(this ptr<Regexp> _addr_re, slice<nint> a) {
    ref Regexp re = ref _addr_re.val;

    if (a == null) { 
        // No match.
        return null;
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
private static void allMatches(this ptr<Regexp> _addr_re, @string s, slice<byte> b, nint n, Action<slice<nint>> deliver) {
    ref Regexp re = ref _addr_re.val;

    nint end = default;
    if (b == null) {
        end = len(s);
    }
    else
 {
        end = len(b);
    }
    {
        nint pos = 0;
        nint i = 0;
        nint prevMatchEnd = -1;

        while (i < n && pos <= end) {
            var matches = re.doExecute(null, b, s, pos, re.prog.NumCap, null);
            if (len(matches) == 0) {
                break;
            }
            var accept = true;
            if (matches[1] == pos) { 
                // We've found an empty match.
                if (matches[0] == prevMatchEnd) { 
                    // We don't allow an empty match right
                    // after a previous match, so ignore it.
                    accept = false;
                }
                nint width = default; 
                // TODO: use step()
                if (b == null) {
                    _, width = utf8.DecodeRuneInString(s[(int)pos..(int)end]);
                }
                else
 {
                    _, width = utf8.DecodeRune(b[(int)pos..(int)end]);
                }
                if (width > 0) {
                    pos += width;
                }
                else
 {
                    pos = end + 1;
                }
            }
            else
 {
                pos = matches[1];
            }
            prevMatchEnd = matches[1];

            if (accept) {
                deliver(re.pad(matches));
                i++;
            }
        }
    }
}

// Find returns a slice holding the text of the leftmost match in b of the regular expression.
// A return value of nil indicates no match.
private static slice<byte> Find(this ptr<Regexp> _addr_re, slice<byte> b) {
    ref Regexp re = ref _addr_re.val;

    array<nint> dstCap = new array<nint>(2);
    var a = re.doExecute(null, b, "", 0, 2, dstCap[..(int)0]);
    if (a == null) {
        return null;
    }
    return b.slice(a[0], a[1], a[1]);
}

// FindIndex returns a two-element slice of integers defining the location of
// the leftmost match in b of the regular expression. The match itself is at
// b[loc[0]:loc[1]].
// A return value of nil indicates no match.
private static slice<nint> FindIndex(this ptr<Regexp> _addr_re, slice<byte> b) {
    slice<nint> loc = default;
    ref Regexp re = ref _addr_re.val;

    var a = re.doExecute(null, b, "", 0, 2, null);
    if (a == null) {
        return null;
    }
    return a[(int)0..(int)2];
}

// FindString returns a string holding the text of the leftmost match in s of the regular
// expression. If there is no match, the return value is an empty string,
// but it will also be empty if the regular expression successfully matches
// an empty string. Use FindStringIndex or FindStringSubmatch if it is
// necessary to distinguish these cases.
private static @string FindString(this ptr<Regexp> _addr_re, @string s) {
    ref Regexp re = ref _addr_re.val;

    array<nint> dstCap = new array<nint>(2);
    var a = re.doExecute(null, null, s, 0, 2, dstCap[..(int)0]);
    if (a == null) {
        return "";
    }
    return s[(int)a[0]..(int)a[1]];
}

// FindStringIndex returns a two-element slice of integers defining the
// location of the leftmost match in s of the regular expression. The match
// itself is at s[loc[0]:loc[1]].
// A return value of nil indicates no match.
private static slice<nint> FindStringIndex(this ptr<Regexp> _addr_re, @string s) {
    slice<nint> loc = default;
    ref Regexp re = ref _addr_re.val;

    var a = re.doExecute(null, null, s, 0, 2, null);
    if (a == null) {
        return null;
    }
    return a[(int)0..(int)2];
}

// FindReaderIndex returns a two-element slice of integers defining the
// location of the leftmost match of the regular expression in text read from
// the RuneReader. The match text was found in the input stream at
// byte offset loc[0] through loc[1]-1.
// A return value of nil indicates no match.
private static slice<nint> FindReaderIndex(this ptr<Regexp> _addr_re, io.RuneReader r) {
    slice<nint> loc = default;
    ref Regexp re = ref _addr_re.val;

    var a = re.doExecute(r, null, "", 0, 2, null);
    if (a == null) {
        return null;
    }
    return a[(int)0..(int)2];
}

// FindSubmatch returns a slice of slices holding the text of the leftmost
// match of the regular expression in b and the matches, if any, of its
// subexpressions, as defined by the 'Submatch' descriptions in the package
// comment.
// A return value of nil indicates no match.
private static slice<slice<byte>> FindSubmatch(this ptr<Regexp> _addr_re, slice<byte> b) {
    ref Regexp re = ref _addr_re.val;

    array<nint> dstCap = new array<nint>(4);
    var a = re.doExecute(null, b, "", 0, re.prog.NumCap, dstCap[..(int)0]);
    if (a == null) {
        return null;
    }
    var ret = make_slice<slice<byte>>(1 + re.numSubexp);
    foreach (var (i) in ret) {
        if (2 * i < len(a) && a[2 * i] >= 0) {
            ret[i] = b.slice(a[2 * i], a[2 * i + 1], a[2 * i + 1]);
        }
    }    return ret;
}

// Expand appends template to dst and returns the result; during the
// append, Expand replaces variables in the template with corresponding
// matches drawn from src. The match slice should have been returned by
// FindSubmatchIndex.
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
private static slice<byte> Expand(this ptr<Regexp> _addr_re, slice<byte> dst, slice<byte> template, slice<byte> src, slice<nint> match) {
    ref Regexp re = ref _addr_re.val;

    return re.expand(dst, string(template), src, "", match);
}

// ExpandString is like Expand but the template and source are strings.
// It appends to and returns a byte slice in order to give the calling
// code control over allocation.
private static slice<byte> ExpandString(this ptr<Regexp> _addr_re, slice<byte> dst, @string template, @string src, slice<nint> match) {
    ref Regexp re = ref _addr_re.val;

    return re.expand(dst, template, null, src, match);
}

private static slice<byte> expand(this ptr<Regexp> _addr_re, slice<byte> dst, @string template, slice<byte> bsrc, @string src, slice<nint> match) {
    ref Regexp re = ref _addr_re.val;

    while (len(template) > 0) {
        var i = strings.Index(template, "$");
        if (i < 0) {
            break;
        }
        dst = append(dst, template[..(int)i]);
        template = template[(int)i..];
        if (len(template) > 1 && template[1] == '$') { 
            // Treat $$ as $.
            dst = append(dst, '$');
            template = template[(int)2..];
            continue;
        }
        var (name, num, rest, ok) = extract(template);
        if (!ok) { 
            // Malformed; treat $ as raw text.
            dst = append(dst, '$');
            template = template[(int)1..];
            continue;
        }
        template = rest;
        if (num >= 0) {
            if (2 * num + 1 < len(match) && match[2 * num] >= 0) {
                if (bsrc != null) {
                    dst = append(dst, bsrc[(int)match[2 * num]..(int)match[2 * num + 1]]);
                }
                else
 {
                    dst = append(dst, src[(int)match[2 * num]..(int)match[2 * num + 1]]);
                }
            }
        }
        else
 {
            {
                var i__prev2 = i;

                foreach (var (__i, __namei) in re.subexpNames) {
                    i = __i;
                    namei = __namei;
                    if (name == namei && 2 * i + 1 < len(match) && match[2 * i] >= 0) {
                        if (bsrc != null) {
                            dst = append(dst, bsrc[(int)match[2 * i]..(int)match[2 * i + 1]]);
                        }
                        else
 {
                            dst = append(dst, src[(int)match[2 * i]..(int)match[2 * i + 1]]);
                        }
                        break;
                    }
                }

                i = i__prev2;
            }
        }
    }
    dst = append(dst, template);
    return dst;
}

// extract returns the name from a leading "$name" or "${name}" in str.
// If it is a number, extract returns num set to that number; otherwise num = -1.
private static (@string, nint, @string, bool) extract(@string str) {
    @string name = default;
    nint num = default;
    @string rest = default;
    bool ok = default;

    if (len(str) < 2 || str[0] != '$') {
        return ;
    }
    var brace = false;
    if (str[1] == '{') {
        brace = true;
        str = str[(int)2..];
    }
    else
 {
        str = str[(int)1..];
    }
    nint i = 0;
    while (i < len(str)) {
        var (rune, size) = utf8.DecodeRuneInString(str[(int)i..]);
        if (!unicode.IsLetter(rune) && !unicode.IsDigit(rune) && rune != '_') {
            break;
        }
        i += size;
    }
    if (i == 0) { 
        // empty name is not okay
        return ;
    }
    name = str[..(int)i];
    if (brace) {
        if (i >= len(str) || str[i] != '}') { 
            // missing closing brace
            return ;
        }
        i++;
    }
    num = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < len(name); i++) {
            if (name[i] < '0' || '9' < name[i] || num >= 1e8F) {
                num = -1;
                break;
            }
            num = num * 10 + int(name[i]) - '0';
        }

        i = i__prev1;
    } 
    // Disallow leading zeros.
    if (name[0] == '0' && len(name) > 1) {
        num = -1;
    }
    rest = str[(int)i..];
    ok = true;
    return ;
}

// FindSubmatchIndex returns a slice holding the index pairs identifying the
// leftmost match of the regular expression in b and the matches, if any, of
// its subexpressions, as defined by the 'Submatch' and 'Index' descriptions
// in the package comment.
// A return value of nil indicates no match.
private static slice<nint> FindSubmatchIndex(this ptr<Regexp> _addr_re, slice<byte> b) {
    ref Regexp re = ref _addr_re.val;

    return re.pad(re.doExecute(null, b, "", 0, re.prog.NumCap, null));
}

// FindStringSubmatch returns a slice of strings holding the text of the
// leftmost match of the regular expression in s and the matches, if any, of
// its subexpressions, as defined by the 'Submatch' description in the
// package comment.
// A return value of nil indicates no match.
private static slice<@string> FindStringSubmatch(this ptr<Regexp> _addr_re, @string s) {
    ref Regexp re = ref _addr_re.val;

    array<nint> dstCap = new array<nint>(4);
    var a = re.doExecute(null, null, s, 0, re.prog.NumCap, dstCap[..(int)0]);
    if (a == null) {
        return null;
    }
    var ret = make_slice<@string>(1 + re.numSubexp);
    foreach (var (i) in ret) {
        if (2 * i < len(a) && a[2 * i] >= 0) {
            ret[i] = s[(int)a[2 * i]..(int)a[2 * i + 1]];
        }
    }    return ret;
}

// FindStringSubmatchIndex returns a slice holding the index pairs
// identifying the leftmost match of the regular expression in s and the
// matches, if any, of its subexpressions, as defined by the 'Submatch' and
// 'Index' descriptions in the package comment.
// A return value of nil indicates no match.
private static slice<nint> FindStringSubmatchIndex(this ptr<Regexp> _addr_re, @string s) {
    ref Regexp re = ref _addr_re.val;

    return re.pad(re.doExecute(null, null, s, 0, re.prog.NumCap, null));
}

// FindReaderSubmatchIndex returns a slice holding the index pairs
// identifying the leftmost match of the regular expression of text read by
// the RuneReader, and the matches, if any, of its subexpressions, as defined
// by the 'Submatch' and 'Index' descriptions in the package comment. A
// return value of nil indicates no match.
private static slice<nint> FindReaderSubmatchIndex(this ptr<Regexp> _addr_re, io.RuneReader r) {
    ref Regexp re = ref _addr_re.val;

    return re.pad(re.doExecute(r, null, "", 0, re.prog.NumCap, null));
}

private static readonly nint startSize = 10; // The size at which to start a slice in the 'All' routines.

// FindAll is the 'All' version of Find; it returns a slice of all successive
// matches of the expression, as defined by the 'All' description in the
// package comment.
// A return value of nil indicates no match.
 // The size at which to start a slice in the 'All' routines.

// FindAll is the 'All' version of Find; it returns a slice of all successive
// matches of the expression, as defined by the 'All' description in the
// package comment.
// A return value of nil indicates no match.
private static slice<slice<byte>> FindAll(this ptr<Regexp> _addr_re, slice<byte> b, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<byte>> result = default;
    re.allMatches("", b, n, match => {
        if (result == null) {
            result = make_slice<slice<byte>>(0, startSize);
        }
        result = append(result, b.slice(match[0], match[1], match[1]));
    });
    return result;
}

// FindAllIndex is the 'All' version of FindIndex; it returns a slice of all
// successive matches of the expression, as defined by the 'All' description
// in the package comment.
// A return value of nil indicates no match.
private static slice<slice<nint>> FindAllIndex(this ptr<Regexp> _addr_re, slice<byte> b, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<nint>> result = default;
    re.allMatches("", b, n, match => {
        if (result == null) {
            result = make_slice<slice<nint>>(0, startSize);
        }
        result = append(result, match[(int)0..(int)2]);
    });
    return result;
}

// FindAllString is the 'All' version of FindString; it returns a slice of all
// successive matches of the expression, as defined by the 'All' description
// in the package comment.
// A return value of nil indicates no match.
private static slice<@string> FindAllString(this ptr<Regexp> _addr_re, @string s, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(s) + 1;
    }
    slice<@string> result = default;
    re.allMatches(s, null, n, match => {
        if (result == null) {
            result = make_slice<@string>(0, startSize);
        }
        result = append(result, s[(int)match[0]..(int)match[1]]);
    });
    return result;
}

// FindAllStringIndex is the 'All' version of FindStringIndex; it returns a
// slice of all successive matches of the expression, as defined by the 'All'
// description in the package comment.
// A return value of nil indicates no match.
private static slice<slice<nint>> FindAllStringIndex(this ptr<Regexp> _addr_re, @string s, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<nint>> result = default;
    re.allMatches(s, null, n, match => {
        if (result == null) {
            result = make_slice<slice<nint>>(0, startSize);
        }
        result = append(result, match[(int)0..(int)2]);
    });
    return result;
}

// FindAllSubmatch is the 'All' version of FindSubmatch; it returns a slice
// of all successive matches of the expression, as defined by the 'All'
// description in the package comment.
// A return value of nil indicates no match.
private static slice<slice<slice<byte>>> FindAllSubmatch(this ptr<Regexp> _addr_re, slice<byte> b, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<slice<byte>>> result = default;
    re.allMatches("", b, n, match => {
        if (result == null) {
            result = make_slice<slice<slice<byte>>>(0, startSize);
        }
        var slice = make_slice<slice<byte>>(len(match) / 2);
        foreach (var (j) in slice) {
            if (match[2 * j] >= 0) {
                slice[j] = b.slice(match[2 * j], match[2 * j + 1], match[2 * j + 1]);
            }
        }        result = append(result, slice);
    });
    return result;
}

// FindAllSubmatchIndex is the 'All' version of FindSubmatchIndex; it returns
// a slice of all successive matches of the expression, as defined by the
// 'All' description in the package comment.
// A return value of nil indicates no match.
private static slice<slice<nint>> FindAllSubmatchIndex(this ptr<Regexp> _addr_re, slice<byte> b, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(b) + 1;
    }
    slice<slice<nint>> result = default;
    re.allMatches("", b, n, match => {
        if (result == null) {
            result = make_slice<slice<nint>>(0, startSize);
        }
        result = append(result, match);
    });
    return result;
}

// FindAllStringSubmatch is the 'All' version of FindStringSubmatch; it
// returns a slice of all successive matches of the expression, as defined by
// the 'All' description in the package comment.
// A return value of nil indicates no match.
private static slice<slice<@string>> FindAllStringSubmatch(this ptr<Regexp> _addr_re, @string s, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<@string>> result = default;
    re.allMatches(s, null, n, match => {
        if (result == null) {
            result = make_slice<slice<@string>>(0, startSize);
        }
        var slice = make_slice<@string>(len(match) / 2);
        foreach (var (j) in slice) {
            if (match[2 * j] >= 0) {
                slice[j] = s[(int)match[2 * j]..(int)match[2 * j + 1]];
            }
        }        result = append(result, slice);
    });
    return result;
}

// FindAllStringSubmatchIndex is the 'All' version of
// FindStringSubmatchIndex; it returns a slice of all successive matches of
// the expression, as defined by the 'All' description in the package
// comment.
// A return value of nil indicates no match.
private static slice<slice<nint>> FindAllStringSubmatchIndex(this ptr<Regexp> _addr_re, @string s, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n < 0) {
        n = len(s) + 1;
    }
    slice<slice<nint>> result = default;
    re.allMatches(s, null, n, match => {
        if (result == null) {
            result = make_slice<slice<nint>>(0, startSize);
        }
        result = append(result, match);
    });
    return result;
}

// Split slices s into substrings separated by the expression and returns a slice of
// the substrings between those expression matches.
//
// The slice returned by this method consists of all the substrings of s
// not contained in the slice returned by FindAllString. When called on an expression
// that contains no metacharacters, it is equivalent to strings.SplitN.
//
// Example:
//   s := regexp.MustCompile("a*").Split("abaabaccadaaae", 5)
//   // s: ["", "b", "b", "c", "cadaaae"]
//
// The count determines the number of substrings to return:
//   n > 0: at most n substrings; the last substring will be the unsplit remainder.
//   n == 0: the result is nil (zero substrings)
//   n < 0: all substrings
private static slice<@string> Split(this ptr<Regexp> _addr_re, @string s, nint n) {
    ref Regexp re = ref _addr_re.val;

    if (n == 0) {
        return null;
    }
    if (len(re.expr) > 0 && len(s) == 0) {
        return new slice<@string>(new @string[] { "" });
    }
    var matches = re.FindAllStringIndex(s, n);
    var strings = make_slice<@string>(0, len(matches));

    nint beg = 0;
    nint end = 0;
    foreach (var (_, match) in matches) {
        if (n > 0 && len(strings) >= n - 1) {
            break;
        }
        end = match[0];
        if (match[1] != 0) {
            strings = append(strings, s[(int)beg..(int)end]);
        }
        beg = match[1];
    }    if (end != len(s)) {
        strings = append(strings, s[(int)beg..]);
    }
    return strings;
}

} // end regexp_package
