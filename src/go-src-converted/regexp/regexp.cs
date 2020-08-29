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
//    http://swtch.com/~rsc/regexp/regexp1.html
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
// an extra integer argument, n; if n >= 0, the function returns at most n
// matches/submatches.
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
// expression. If 'Index' is not present, the match is identified by the
// text of the match/submatch. If an index is negative, it means that
// subexpression did not match any string in the input.
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
// package regexp -- go2cs converted at 2020 August 29 08:24:11 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\regexp.go
using bytes = go.bytes_package;
using io = go.io_package;
using syntax = go.regexp.syntax_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class regexp_package
    {
        // Regexp is the representation of a compiled regular expression.
        // A Regexp is safe for concurrent use by multiple goroutines,
        // except for configuration methods, such as Longest.
        public partial struct Regexp
        {
            public ref regexpRO regexpRO => ref regexpRO_val; // cache of machines for running regexp
            public sync.Mutex mu;
            public slice<ref machine> machine;
        }

        private partial struct regexpRO
        {
            public @string expr; // as passed to Compile
            public ptr<syntax.Prog> prog; // compiled program
            public ptr<onePassProg> onepass; // onepass program or nil
            public @string prefix; // required prefix in unanchored matches
            public slice<byte> prefixBytes; // prefix, as a []byte
            public bool prefixComplete; // prefix is the entire regexp
            public int prefixRune; // first rune in prefix
            public uint prefixEnd; // pc for last rune in prefix
            public syntax.EmptyOp cond; // empty-width conditions required at start of match
            public long numSubexp;
            public slice<@string> subexpNames;
            public bool longest;
        }

        // String returns the source text used to compile the regular expression.
        private static @string String(this ref Regexp re)
        {
            return re.expr;
        }

        // Copy returns a new Regexp object copied from re.
        //
        // When using a Regexp in multiple goroutines, giving each goroutine
        // its own copy helps to avoid lock contention.
        private static ref Regexp Copy(this ref Regexp re)
        { 
            // It is not safe to copy Regexp by value
            // since it contains a sync.Mutex.
            return ref new Regexp(regexpRO:re.regexpRO,);
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
        public static (ref Regexp, error) Compile(@string expr)
        {
            return compile(expr, syntax.Perl, false);
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
        // See http://swtch.com/~rsc/regexp/regexp2.html#posix for details.
        public static (ref Regexp, error) CompilePOSIX(@string expr)
        {
            return compile(expr, syntax.POSIX, true);
        }

        // Longest makes future searches prefer the leftmost-longest match.
        // That is, when matching against text, the regexp returns a match that
        // begins as early as possible in the input (leftmost), and among those
        // it chooses a match that is as long as possible.
        // This method modifies the Regexp and may not be called concurrently
        // with any other methods.
        private static void Longest(this ref Regexp re)
        {
            re.longest = true;
        }

        private static (ref Regexp, error) compile(@string expr, syntax.Flags mode, bool longest)
        {
            var (re, err) = syntax.Parse(expr, mode);
            if (err != null)
            {
                return (null, err);
            }
            var maxCap = re.MaxCap();
            var capNames = re.CapNames();

            re = re.Simplify();
            var (prog, err) = syntax.Compile(re);
            if (err != null)
            {
                return (null, err);
            }
            Regexp regexp = ref new Regexp(regexpRO:regexpRO{expr:expr,prog:prog,onepass:compileOnePass(prog),numSubexp:maxCap,subexpNames:capNames,cond:prog.StartCond(),longest:longest,},);
            if (regexp.onepass == notOnePass)
            {
                regexp.prefix, regexp.prefixComplete = prog.Prefix();
            }
            else
            {
                regexp.prefix, regexp.prefixComplete, regexp.prefixEnd = onePassPrefix(prog);
            }
            if (regexp.prefix != "")
            { 
                // TODO(rsc): Remove this allocation by adding
                // IndexString to package bytes.
                regexp.prefixBytes = (slice<byte>)regexp.prefix;
                regexp.prefixRune, _ = utf8.DecodeRuneInString(regexp.prefix);
            }
            return (regexp, null);
        }

        // get returns a machine to use for matching re.
        // It uses the re's machine cache if possible, to avoid
        // unnecessary allocation.
        private static ref machine get(this ref Regexp re)
        {
            re.mu.Lock();
            {
                var n = len(re.machine);

                if (n > 0L)
                {
                    var z = re.machine[n - 1L];
                    re.machine = re.machine[..n - 1L];
                    re.mu.Unlock();
                    return z;
                }

            }
            re.mu.Unlock();
            z = progMachine(re.prog, re.onepass);
            z.re = re;
            return z;
        }

        // put returns a machine to the re's machine cache.
        // There is no attempt to limit the size of the cache, so it will
        // grow to the maximum number of simultaneous matches
        // run using re.  (The cache empties when re gets garbage collected.)
        private static void put(this ref Regexp re, ref machine z)
        {
            re.mu.Lock();
            re.machine = append(re.machine, z);
            re.mu.Unlock();
        }

        // MustCompile is like Compile but panics if the expression cannot be parsed.
        // It simplifies safe initialization of global variables holding compiled regular
        // expressions.
        public static ref Regexp MustCompile(@string str) => func((_, panic, __) =>
        {
            var (regexp, error) = Compile(str);
            if (error != null)
            {
                panic("regexp: Compile(" + quote(str) + "): " + error.Error());
            }
            return regexp;
        });

        // MustCompilePOSIX is like CompilePOSIX but panics if the expression cannot be parsed.
        // It simplifies safe initialization of global variables holding compiled regular
        // expressions.
        public static ref Regexp MustCompilePOSIX(@string str) => func((_, panic, __) =>
        {
            var (regexp, error) = CompilePOSIX(str);
            if (error != null)
            {
                panic("regexp: CompilePOSIX(" + quote(str) + "): " + error.Error());
            }
            return regexp;
        });

        private static @string quote(@string s)
        {
            if (strconv.CanBackquote(s))
            {
                return "`" + s + "`";
            }
            return strconv.Quote(s);
        }

        // NumSubexp returns the number of parenthesized subexpressions in this Regexp.
        private static long NumSubexp(this ref Regexp re)
        {
            return re.numSubexp;
        }

        // SubexpNames returns the names of the parenthesized subexpressions
        // in this Regexp. The name for the first sub-expression is names[1],
        // so that if m is a match slice, the name for m[i] is SubexpNames()[i].
        // Since the Regexp as a whole cannot be named, names[0] is always
        // the empty string. The slice should not be modified.
        private static slice<@string> SubexpNames(this ref Regexp re)
        {
            return re.subexpNames;
        }

        private static readonly int endOfText = -1L;

        // input abstracts different representations of the input text. It provides
        // one-character lookahead.


        // input abstracts different representations of the input text. It provides
        // one-character lookahead.
        private partial interface input
        {
            syntax.EmptyOp step(long pos); // advance one rune
            syntax.EmptyOp canCheckPrefix(); // can we look ahead without losing info?
            syntax.EmptyOp hasPrefix(ref Regexp re);
            syntax.EmptyOp index(ref Regexp re, long pos);
            syntax.EmptyOp context(long pos);
        }

        // inputString scans a string.
        private partial struct inputString
        {
            public @string str;
        }

        private static (int, long) step(this ref inputString i, long pos)
        {
            if (pos < len(i.str))
            {
                var c = i.str[pos];
                if (c < utf8.RuneSelf)
                {
                    return (rune(c), 1L);
                }
                return utf8.DecodeRuneInString(i.str[pos..]);
            }
            return (endOfText, 0L);
        }

        private static bool canCheckPrefix(this ref inputString i)
        {
            return true;
        }

        private static bool hasPrefix(this ref inputString i, ref Regexp re)
        {
            return strings.HasPrefix(i.str, re.prefix);
        }

        private static long index(this ref inputString i, ref Regexp re, long pos)
        {
            return strings.Index(i.str[pos..], re.prefix);
        }

        private static syntax.EmptyOp context(this ref inputString i, long pos)
        {
            var r1 = endOfText;
            var r2 = endOfText; 
            // 0 < pos && pos <= len(i.str)
            if (uint(pos - 1L) < uint(len(i.str)))
            {
                r1 = rune(i.str[pos - 1L]);
                if (r1 >= utf8.RuneSelf)
                {
                    r1, _ = utf8.DecodeLastRuneInString(i.str[..pos]);
                }
            } 
            // 0 <= pos && pos < len(i.str)
            if (uint(pos) < uint(len(i.str)))
            {
                r2 = rune(i.str[pos]);
                if (r2 >= utf8.RuneSelf)
                {
                    r2, _ = utf8.DecodeRuneInString(i.str[pos..]);
                }
            }
            return syntax.EmptyOpContext(r1, r2);
        }

        // inputBytes scans a byte slice.
        private partial struct inputBytes
        {
            public slice<byte> str;
        }

        private static (int, long) step(this ref inputBytes i, long pos)
        {
            if (pos < len(i.str))
            {
                var c = i.str[pos];
                if (c < utf8.RuneSelf)
                {
                    return (rune(c), 1L);
                }
                return utf8.DecodeRune(i.str[pos..]);
            }
            return (endOfText, 0L);
        }

        private static bool canCheckPrefix(this ref inputBytes i)
        {
            return true;
        }

        private static bool hasPrefix(this ref inputBytes i, ref Regexp re)
        {
            return bytes.HasPrefix(i.str, re.prefixBytes);
        }

        private static long index(this ref inputBytes i, ref Regexp re, long pos)
        {
            return bytes.Index(i.str[pos..], re.prefixBytes);
        }

        private static syntax.EmptyOp context(this ref inputBytes i, long pos)
        {
            var r1 = endOfText;
            var r2 = endOfText; 
            // 0 < pos && pos <= len(i.str)
            if (uint(pos - 1L) < uint(len(i.str)))
            {
                r1 = rune(i.str[pos - 1L]);
                if (r1 >= utf8.RuneSelf)
                {
                    r1, _ = utf8.DecodeLastRune(i.str[..pos]);
                }
            } 
            // 0 <= pos && pos < len(i.str)
            if (uint(pos) < uint(len(i.str)))
            {
                r2 = rune(i.str[pos]);
                if (r2 >= utf8.RuneSelf)
                {
                    r2, _ = utf8.DecodeRune(i.str[pos..]);
                }
            }
            return syntax.EmptyOpContext(r1, r2);
        }

        // inputReader scans a RuneReader.
        private partial struct inputReader
        {
            public io.RuneReader r;
            public bool atEOT;
            public long pos;
        }

        private static (int, long) step(this ref inputReader i, long pos)
        {
            if (!i.atEOT && pos != i.pos)
            {
                return (endOfText, 0L);

            }
            var (r, w, err) = i.r.ReadRune();
            if (err != null)
            {
                i.atEOT = true;
                return (endOfText, 0L);
            }
            i.pos += w;
            return (r, w);
        }

        private static bool canCheckPrefix(this ref inputReader i)
        {
            return false;
        }

        private static bool hasPrefix(this ref inputReader i, ref Regexp re)
        {
            return false;
        }

        private static long index(this ref inputReader i, ref Regexp re, long pos)
        {
            return -1L;
        }

        private static syntax.EmptyOp context(this ref inputReader i, long pos)
        {
            return 0L;
        }

        // LiteralPrefix returns a literal string that must begin any match
        // of the regular expression re. It returns the boolean true if the
        // literal string comprises the entire regular expression.
        private static (@string, bool) LiteralPrefix(this ref Regexp re)
        {
            return (re.prefix, re.prefixComplete);
        }

        // MatchReader reports whether the Regexp matches the text read by the
        // RuneReader.
        private static bool MatchReader(this ref Regexp re, io.RuneReader r)
        {
            return re.doMatch(r, null, "");
        }

        // MatchString reports whether the Regexp matches the string s.
        private static bool MatchString(this ref Regexp re, @string s)
        {
            return re.doMatch(null, null, s);
        }

        // Match reports whether the Regexp matches the byte slice b.
        private static bool Match(this ref Regexp re, slice<byte> b)
        {
            return re.doMatch(null, b, "");
        }

        // MatchReader checks whether a textual regular expression matches the text
        // read by the RuneReader. More complicated queries need to use Compile and
        // the full Regexp interface.
        public static (bool, error) MatchReader(@string pattern, io.RuneReader r)
        {
            var (re, err) = Compile(pattern);
            if (err != null)
            {
                return (false, err);
            }
            return (re.MatchReader(r), null);
        }

        // MatchString checks whether a textual regular expression
        // matches a string. More complicated queries need
        // to use Compile and the full Regexp interface.
        public static (bool, error) MatchString(@string pattern, @string s)
        {
            var (re, err) = Compile(pattern);
            if (err != null)
            {
                return (false, err);
            }
            return (re.MatchString(s), null);
        }

        // Match checks whether a textual regular expression
        // matches a byte slice. More complicated queries need
        // to use Compile and the full Regexp interface.
        public static (bool, error) Match(@string pattern, slice<byte> b)
        {
            var (re, err) = Compile(pattern);
            if (err != null)
            {
                return (false, err);
            }
            return (re.Match(b), null);
        }

        // ReplaceAllString returns a copy of src, replacing matches of the Regexp
        // with the replacement string repl. Inside repl, $ signs are interpreted as
        // in Expand, so for instance $1 represents the text of the first submatch.
        private static @string ReplaceAllString(this ref Regexp re, @string src, @string repl)
        {
            long n = 2L;
            if (strings.Contains(repl, "$"))
            {
                n = 2L * (re.numSubexp + 1L);
            }
            var b = re.replaceAll(null, src, n, (dst, match) =>
            {
                return re.expand(dst, repl, null, src, match);
            });
            return string(b);
        }

        // ReplaceAllLiteralString returns a copy of src, replacing matches of the Regexp
        // with the replacement string repl. The replacement repl is substituted directly,
        // without using Expand.
        private static @string ReplaceAllLiteralString(this ref Regexp re, @string src, @string repl)
        {
            return string(re.replaceAll(null, src, 2L, (dst, match) =>
            {
                return append(dst, repl);
            }));
        }

        // ReplaceAllStringFunc returns a copy of src in which all matches of the
        // Regexp have been replaced by the return value of function repl applied
        // to the matched substring. The replacement returned by repl is substituted
        // directly, without using Expand.
        private static @string ReplaceAllStringFunc(this ref Regexp re, @string src, Func<@string, @string> repl)
        {
            var b = re.replaceAll(null, src, 2L, (dst, match) =>
            {
                return append(dst, repl(src[match[0L]..match[1L]]));
            });
            return string(b);
        }

        private static slice<byte> replaceAll(this ref Regexp re, slice<byte> bsrc, @string src, long nmatch, Func<slice<byte>, slice<long>, slice<byte>> repl)
        {
            long lastMatchEnd = 0L; // end position of the most recent match
            long searchPos = 0L; // position where we next look for a match
            slice<byte> buf = default;
            long endPos = default;
            if (bsrc != null)
            {
                endPos = len(bsrc);
            }
            else
            {
                endPos = len(src);
            }
            if (nmatch > re.prog.NumCap)
            {
                nmatch = re.prog.NumCap;
            }
            array<long> dstCap = new array<long>(2L);
            while (searchPos <= endPos)
            {
                var a = re.doExecute(null, bsrc, src, searchPos, nmatch, dstCap[..0L]);
                if (len(a) == 0L)
                {
                    break; // no more matches
                } 

                // Copy the unmatched characters before this match.
                if (bsrc != null)
                {
                    buf = append(buf, bsrc[lastMatchEnd..a[0L]]);
                }
                else
                {
                    buf = append(buf, src[lastMatchEnd..a[0L]]);
                } 

                // Now insert a copy of the replacement string, but not for a
                // match of the empty string immediately after another match.
                // (Otherwise, we get double replacement for patterns that
                // match both empty and nonempty strings.)
                if (a[1L] > lastMatchEnd || a[0L] == 0L)
                {
                    buf = repl(buf, a);
                }
                lastMatchEnd = a[1L]; 

                // Advance past this match; always advance at least one character.
                long width = default;
                if (bsrc != null)
                {
                    _, width = utf8.DecodeRune(bsrc[searchPos..]);
                }
                else
                {
                    _, width = utf8.DecodeRuneInString(src[searchPos..]);
                }
                if (searchPos + width > a[1L])
                {
                    searchPos += width;
                }
                else if (searchPos + 1L > a[1L])
                { 
                    // This clause is only needed at the end of the input
                    // string. In that case, DecodeRuneInString returns width=0.
                    searchPos++;
                }
                else
                {
                    searchPos = a[1L];
                }
            } 

            // Copy the unmatched characters after the last match.
 

            // Copy the unmatched characters after the last match.
            if (bsrc != null)
            {
                buf = append(buf, bsrc[lastMatchEnd..]);
            }
            else
            {
                buf = append(buf, src[lastMatchEnd..]);
            }
            return buf;
        }

        // ReplaceAll returns a copy of src, replacing matches of the Regexp
        // with the replacement text repl. Inside repl, $ signs are interpreted as
        // in Expand, so for instance $1 represents the text of the first submatch.
        private static slice<byte> ReplaceAll(this ref Regexp re, slice<byte> src, slice<byte> repl)
        {
            long n = 2L;
            if (bytes.IndexByte(repl, '$') >= 0L)
            {
                n = 2L * (re.numSubexp + 1L);
            }
            @string srepl = "";
            var b = re.replaceAll(src, "", n, (dst, match) =>
            {
                if (len(srepl) != len(repl))
                {
                    srepl = string(repl);
                }
                return re.expand(dst, srepl, src, "", match);
            });
            return b;
        }

        // ReplaceAllLiteral returns a copy of src, replacing matches of the Regexp
        // with the replacement bytes repl. The replacement repl is substituted directly,
        // without using Expand.
        private static slice<byte> ReplaceAllLiteral(this ref Regexp re, slice<byte> src, slice<byte> repl)
        {
            return re.replaceAll(src, "", 2L, (dst, match) =>
            {
                return append(dst, repl);
            });
        }

        // ReplaceAllFunc returns a copy of src in which all matches of the
        // Regexp have been replaced by the return value of function repl applied
        // to the matched byte slice. The replacement returned by repl is substituted
        // directly, without using Expand.
        private static slice<byte> ReplaceAllFunc(this ref Regexp re, slice<byte> src, Func<slice<byte>, slice<byte>> repl)
        {
            return re.replaceAll(src, "", 2L, (dst, match) =>
            {
                return append(dst, repl(src[match[0L]..match[1L]]));
            });
        }

        // Bitmap used by func special to check whether a character needs to be escaped.
        private static array<byte> specialBytes = new array<byte>(16L);

        // special reports whether byte b needs to be escaped by QuoteMeta.
        private static bool special(byte b)
        {
            return b < utf8.RuneSelf && specialBytes[b % 16L] & (1L << (int)((b / 16L))) != 0L;
        }

        private static void init()
        {
            foreach (var (_, b) in (slice<byte>)"\\.+*?()|[]{}^$")
            {
                specialBytes[b % 16L] |= 1L << (int)((b / 16L));
            }
        }

        // QuoteMeta returns a string that quotes all regular expression metacharacters
        // inside the argument text; the returned string is a regular expression matching
        // the literal text. For example, QuoteMeta(`[foo]`) returns `\[foo\]`.
        public static @string QuoteMeta(@string s)
        { 
            // A byte loop is correct because all metacharacters are ASCII.
            long i = default;
            for (i = 0L; i < len(s); i++)
            {
                if (special(s[i]))
                {
                    break;
                }
            } 
            // No meta characters found, so return original string.
 
            // No meta characters found, so return original string.
            if (i >= len(s))
            {
                return s;
            }
            var b = make_slice<byte>(2L * len(s) - i);
            copy(b, s[..i]);
            var j = i;
            while (i < len(s))
            {
                if (special(s[i]))
                {
                    b[j] = '\\';
                    j++;
                i++;
                }
                b[j] = s[i];
                j++;
            }

            return string(b[..j]);
        }

        // The number of capture values in the program may correspond
        // to fewer capturing expressions than are in the regexp.
        // For example, "(a){0}" turns into an empty program, so the
        // maximum capture in the program is 0 but we need to return
        // an expression for \1.  Pad appends -1s to the slice a as needed.
        private static slice<long> pad(this ref Regexp re, slice<long> a)
        {
            if (a == null)
            { 
                // No match.
                return null;
            }
            long n = (1L + re.numSubexp) * 2L;
            while (len(a) < n)
            {
                a = append(a, -1L);
            }

            return a;
        }

        // Find matches in slice b if b is non-nil, otherwise find matches in string s.
        private static void allMatches(this ref Regexp re, @string s, slice<byte> b, long n, Action<slice<long>> deliver)
        {
            long end = default;
            if (b == null)
            {
                end = len(s);
            }
            else
            {
                end = len(b);
            }
            {
                long pos = 0L;
                long i = 0L;
                long prevMatchEnd = -1L;

                while (i < n && pos <= end)
                {
                    var matches = re.doExecute(null, b, s, pos, re.prog.NumCap, null);
                    if (len(matches) == 0L)
                    {
                        break;
                    }
                    var accept = true;
                    if (matches[1L] == pos)
                    { 
                        // We've found an empty match.
                        if (matches[0L] == prevMatchEnd)
                        { 
                            // We don't allow an empty match right
                            // after a previous match, so ignore it.
                            accept = false;
                        }
                        long width = default; 
                        // TODO: use step()
                        if (b == null)
                        {
                            _, width = utf8.DecodeRuneInString(s[pos..end]);
                        }
                        else
                        {
                            _, width = utf8.DecodeRune(b[pos..end]);
                        }
                        if (width > 0L)
                        {
                            pos += width;
                        }
                        else
                        {
                            pos = end + 1L;
                        }
                    }
                    else
                    {
                        pos = matches[1L];
                    }
                    prevMatchEnd = matches[1L];

                    if (accept)
                    {
                        deliver(re.pad(matches));
                        i++;
                    }
                }

            }
        }

        // Find returns a slice holding the text of the leftmost match in b of the regular expression.
        // A return value of nil indicates no match.
        private static slice<byte> Find(this ref Regexp re, slice<byte> b)
        {
            array<long> dstCap = new array<long>(2L);
            var a = re.doExecute(null, b, "", 0L, 2L, dstCap[..0L]);
            if (a == null)
            {
                return null;
            }
            return b[a[0L]..a[1L]];
        }

        // FindIndex returns a two-element slice of integers defining the location of
        // the leftmost match in b of the regular expression. The match itself is at
        // b[loc[0]:loc[1]].
        // A return value of nil indicates no match.
        private static slice<long> FindIndex(this ref Regexp re, slice<byte> b)
        {
            var a = re.doExecute(null, b, "", 0L, 2L, null);
            if (a == null)
            {
                return null;
            }
            return a[0L..2L];
        }

        // FindString returns a string holding the text of the leftmost match in s of the regular
        // expression. If there is no match, the return value is an empty string,
        // but it will also be empty if the regular expression successfully matches
        // an empty string. Use FindStringIndex or FindStringSubmatch if it is
        // necessary to distinguish these cases.
        private static @string FindString(this ref Regexp re, @string s)
        {
            array<long> dstCap = new array<long>(2L);
            var a = re.doExecute(null, null, s, 0L, 2L, dstCap[..0L]);
            if (a == null)
            {
                return "";
            }
            return s[a[0L]..a[1L]];
        }

        // FindStringIndex returns a two-element slice of integers defining the
        // location of the leftmost match in s of the regular expression. The match
        // itself is at s[loc[0]:loc[1]].
        // A return value of nil indicates no match.
        private static slice<long> FindStringIndex(this ref Regexp re, @string s)
        {
            var a = re.doExecute(null, null, s, 0L, 2L, null);
            if (a == null)
            {
                return null;
            }
            return a[0L..2L];
        }

        // FindReaderIndex returns a two-element slice of integers defining the
        // location of the leftmost match of the regular expression in text read from
        // the RuneReader. The match text was found in the input stream at
        // byte offset loc[0] through loc[1]-1.
        // A return value of nil indicates no match.
        private static slice<long> FindReaderIndex(this ref Regexp re, io.RuneReader r)
        {
            var a = re.doExecute(r, null, "", 0L, 2L, null);
            if (a == null)
            {
                return null;
            }
            return a[0L..2L];
        }

        // FindSubmatch returns a slice of slices holding the text of the leftmost
        // match of the regular expression in b and the matches, if any, of its
        // subexpressions, as defined by the 'Submatch' descriptions in the package
        // comment.
        // A return value of nil indicates no match.
        private static slice<slice<byte>> FindSubmatch(this ref Regexp re, slice<byte> b)
        {
            array<long> dstCap = new array<long>(4L);
            var a = re.doExecute(null, b, "", 0L, re.prog.NumCap, dstCap[..0L]);
            if (a == null)
            {
                return null;
            }
            var ret = make_slice<slice<byte>>(1L + re.numSubexp);
            foreach (var (i) in ret)
            {
                if (2L * i < len(a) && a[2L * i] >= 0L)
                {
                    ret[i] = b[a[2L * i]..a[2L * i + 1L]];
                }
            }
            return ret;
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
        private static slice<byte> Expand(this ref Regexp re, slice<byte> dst, slice<byte> template, slice<byte> src, slice<long> match)
        {
            return re.expand(dst, string(template), src, "", match);
        }

        // ExpandString is like Expand but the template and source are strings.
        // It appends to and returns a byte slice in order to give the calling
        // code control over allocation.
        private static slice<byte> ExpandString(this ref Regexp re, slice<byte> dst, @string template, @string src, slice<long> match)
        {
            return re.expand(dst, template, null, src, match);
        }

        private static slice<byte> expand(this ref Regexp re, slice<byte> dst, @string template, slice<byte> bsrc, @string src, slice<long> match)
        {
            while (len(template) > 0L)
            {
                var i = strings.Index(template, "$");
                if (i < 0L)
                {
                    break;
                }
                dst = append(dst, template[..i]);
                template = template[i..];
                if (len(template) > 1L && template[1L] == '$')
                { 
                    // Treat $$ as $.
                    dst = append(dst, '$');
                    template = template[2L..];
                    continue;
                }
                var (name, num, rest, ok) = extract(template);
                if (!ok)
                { 
                    // Malformed; treat $ as raw text.
                    dst = append(dst, '$');
                    template = template[1L..];
                    continue;
                }
                template = rest;
                if (num >= 0L)
                {
                    if (2L * num + 1L < len(match) && match[2L * num] >= 0L)
                    {
                        if (bsrc != null)
                        {
                            dst = append(dst, bsrc[match[2L * num]..match[2L * num + 1L]]);
                        }
                        else
                        {
                            dst = append(dst, src[match[2L * num]..match[2L * num + 1L]]);
                        }
                    }
                }
                else
                {
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __namei) in re.subexpNames)
                        {
                            i = __i;
                            namei = __namei;
                            if (name == namei && 2L * i + 1L < len(match) && match[2L * i] >= 0L)
                            {
                                if (bsrc != null)
                                {
                                    dst = append(dst, bsrc[match[2L * i]..match[2L * i + 1L]]);
                                }
                                else
                                {
                                    dst = append(dst, src[match[2L * i]..match[2L * i + 1L]]);
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
        private static (@string, long, @string, bool) extract(@string str)
        {
            if (len(str) < 2L || str[0L] != '$')
            {
                return;
            }
            var brace = false;
            if (str[1L] == '{')
            {
                brace = true;
                str = str[2L..];
            }
            else
            {
                str = str[1L..];
            }
            long i = 0L;
            while (i < len(str))
            {
                var (rune, size) = utf8.DecodeRuneInString(str[i..]);
                if (!unicode.IsLetter(rune) && !unicode.IsDigit(rune) && rune != '_')
                {
                    break;
                }
                i += size;
            }

            if (i == 0L)
            { 
                // empty name is not okay
                return;
            }
            name = str[..i];
            if (brace)
            {
                if (i >= len(str) || str[i] != '}')
                { 
                    // missing closing brace
                    return;
                }
                i++;
            } 

            // Parse number.
            num = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(name); i++)
                {
                    if (name[i] < '0' || '9' < name[i] || num >= 1e8F)
                    {
                        num = -1L;
                        break;
                    }
                    num = num * 10L + int(name[i]) - '0';
                } 
                // Disallow leading zeros.


                i = i__prev1;
            } 
            // Disallow leading zeros.
            if (name[0L] == '0' && len(name) > 1L)
            {
                num = -1L;
            }
            rest = str[i..];
            ok = true;
            return;
        }

        // FindSubmatchIndex returns a slice holding the index pairs identifying the
        // leftmost match of the regular expression in b and the matches, if any, of
        // its subexpressions, as defined by the 'Submatch' and 'Index' descriptions
        // in the package comment.
        // A return value of nil indicates no match.
        private static slice<long> FindSubmatchIndex(this ref Regexp re, slice<byte> b)
        {
            return re.pad(re.doExecute(null, b, "", 0L, re.prog.NumCap, null));
        }

        // FindStringSubmatch returns a slice of strings holding the text of the
        // leftmost match of the regular expression in s and the matches, if any, of
        // its subexpressions, as defined by the 'Submatch' description in the
        // package comment.
        // A return value of nil indicates no match.
        private static slice<@string> FindStringSubmatch(this ref Regexp re, @string s)
        {
            array<long> dstCap = new array<long>(4L);
            var a = re.doExecute(null, null, s, 0L, re.prog.NumCap, dstCap[..0L]);
            if (a == null)
            {
                return null;
            }
            var ret = make_slice<@string>(1L + re.numSubexp);
            foreach (var (i) in ret)
            {
                if (2L * i < len(a) && a[2L * i] >= 0L)
                {
                    ret[i] = s[a[2L * i]..a[2L * i + 1L]];
                }
            }
            return ret;
        }

        // FindStringSubmatchIndex returns a slice holding the index pairs
        // identifying the leftmost match of the regular expression in s and the
        // matches, if any, of its subexpressions, as defined by the 'Submatch' and
        // 'Index' descriptions in the package comment.
        // A return value of nil indicates no match.
        private static slice<long> FindStringSubmatchIndex(this ref Regexp re, @string s)
        {
            return re.pad(re.doExecute(null, null, s, 0L, re.prog.NumCap, null));
        }

        // FindReaderSubmatchIndex returns a slice holding the index pairs
        // identifying the leftmost match of the regular expression of text read by
        // the RuneReader, and the matches, if any, of its subexpressions, as defined
        // by the 'Submatch' and 'Index' descriptions in the package comment. A
        // return value of nil indicates no match.
        private static slice<long> FindReaderSubmatchIndex(this ref Regexp re, io.RuneReader r)
        {
            return re.pad(re.doExecute(r, null, "", 0L, re.prog.NumCap, null));
        }

        private static readonly long startSize = 10L; // The size at which to start a slice in the 'All' routines.

        // FindAll is the 'All' version of Find; it returns a slice of all successive
        // matches of the expression, as defined by the 'All' description in the
        // package comment.
        // A return value of nil indicates no match.
 // The size at which to start a slice in the 'All' routines.

        // FindAll is the 'All' version of Find; it returns a slice of all successive
        // matches of the expression, as defined by the 'All' description in the
        // package comment.
        // A return value of nil indicates no match.
        private static slice<slice<byte>> FindAll(this ref Regexp re, slice<byte> b, long n)
        {
            if (n < 0L)
            {
                n = len(b) + 1L;
            }
            var result = make_slice<slice<byte>>(0L, startSize);
            re.allMatches("", b, n, match =>
            {
                result = append(result, b[match[0L]..match[1L]]);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllIndex is the 'All' version of FindIndex; it returns a slice of all
        // successive matches of the expression, as defined by the 'All' description
        // in the package comment.
        // A return value of nil indicates no match.
        private static slice<slice<long>> FindAllIndex(this ref Regexp re, slice<byte> b, long n)
        {
            if (n < 0L)
            {
                n = len(b) + 1L;
            }
            var result = make_slice<slice<long>>(0L, startSize);
            re.allMatches("", b, n, match =>
            {
                result = append(result, match[0L..2L]);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllString is the 'All' version of FindString; it returns a slice of all
        // successive matches of the expression, as defined by the 'All' description
        // in the package comment.
        // A return value of nil indicates no match.
        private static slice<@string> FindAllString(this ref Regexp re, @string s, long n)
        {
            if (n < 0L)
            {
                n = len(s) + 1L;
            }
            var result = make_slice<@string>(0L, startSize);
            re.allMatches(s, null, n, match =>
            {
                result = append(result, s[match[0L]..match[1L]]);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllStringIndex is the 'All' version of FindStringIndex; it returns a
        // slice of all successive matches of the expression, as defined by the 'All'
        // description in the package comment.
        // A return value of nil indicates no match.
        private static slice<slice<long>> FindAllStringIndex(this ref Regexp re, @string s, long n)
        {
            if (n < 0L)
            {
                n = len(s) + 1L;
            }
            var result = make_slice<slice<long>>(0L, startSize);
            re.allMatches(s, null, n, match =>
            {
                result = append(result, match[0L..2L]);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllSubmatch is the 'All' version of FindSubmatch; it returns a slice
        // of all successive matches of the expression, as defined by the 'All'
        // description in the package comment.
        // A return value of nil indicates no match.
        private static slice<slice<slice<byte>>> FindAllSubmatch(this ref Regexp re, slice<byte> b, long n)
        {
            if (n < 0L)
            {
                n = len(b) + 1L;
            }
            var result = make_slice<slice<slice<byte>>>(0L, startSize);
            re.allMatches("", b, n, match =>
            {
                var slice = make_slice<slice<byte>>(len(match) / 2L);
                foreach (var (j) in slice)
                {
                    if (match[2L * j] >= 0L)
                    {
                        slice[j] = b[match[2L * j]..match[2L * j + 1L]];
                    }
                }
                result = append(result, slice);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllSubmatchIndex is the 'All' version of FindSubmatchIndex; it returns
        // a slice of all successive matches of the expression, as defined by the
        // 'All' description in the package comment.
        // A return value of nil indicates no match.
        private static slice<slice<long>> FindAllSubmatchIndex(this ref Regexp re, slice<byte> b, long n)
        {
            if (n < 0L)
            {
                n = len(b) + 1L;
            }
            var result = make_slice<slice<long>>(0L, startSize);
            re.allMatches("", b, n, match =>
            {
                result = append(result, match);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllStringSubmatch is the 'All' version of FindStringSubmatch; it
        // returns a slice of all successive matches of the expression, as defined by
        // the 'All' description in the package comment.
        // A return value of nil indicates no match.
        private static slice<slice<@string>> FindAllStringSubmatch(this ref Regexp re, @string s, long n)
        {
            if (n < 0L)
            {
                n = len(s) + 1L;
            }
            var result = make_slice<slice<@string>>(0L, startSize);
            re.allMatches(s, null, n, match =>
            {
                var slice = make_slice<@string>(len(match) / 2L);
                foreach (var (j) in slice)
                {
                    if (match[2L * j] >= 0L)
                    {
                        slice[j] = s[match[2L * j]..match[2L * j + 1L]];
                    }
                }
                result = append(result, slice);
            });
            if (len(result) == 0L)
            {
                return null;
            }
            return result;
        }

        // FindAllStringSubmatchIndex is the 'All' version of
        // FindStringSubmatchIndex; it returns a slice of all successive matches of
        // the expression, as defined by the 'All' description in the package
        // comment.
        // A return value of nil indicates no match.
        private static slice<slice<long>> FindAllStringSubmatchIndex(this ref Regexp re, @string s, long n)
        {
            if (n < 0L)
            {
                n = len(s) + 1L;
            }
            var result = make_slice<slice<long>>(0L, startSize);
            re.allMatches(s, null, n, match =>
            {
                result = append(result, match);
            });
            if (len(result) == 0L)
            {
                return null;
            }
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
        private static slice<@string> Split(this ref Regexp re, @string s, long n)
        {
            if (n == 0L)
            {
                return null;
            }
            if (len(re.expr) > 0L && len(s) == 0L)
            {
                return new slice<@string>(new @string[] { "" });
            }
            var matches = re.FindAllStringIndex(s, n);
            var strings = make_slice<@string>(0L, len(matches));

            long beg = 0L;
            long end = 0L;
            foreach (var (_, match) in matches)
            {
                if (n > 0L && len(strings) >= n - 1L)
                {
                    break;
                }
                end = match[0L];
                if (match[1L] != 0L)
                {
                    strings = append(strings, s[beg..end]);
                }
                beg = match[1L];
            }
            if (end != len(s))
            {
                strings = append(strings, s[beg..]);
            }
            return strings;
        }
    }
}
