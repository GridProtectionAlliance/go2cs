// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 09 04:58:31 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\parse.go
using sort = go.sort_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        // An Error describes a failure to parse a regular expression
        // and gives the offending expression.
        public partial struct Error
        {
            public ErrorCode Code;
            public @string Expr;
        }

        private static @string Error(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return "error parsing regexp: " + e.Code.String() + ": `" + e.Expr + "`";
        }

        // An ErrorCode describes a failure to parse a regular expression.
        public partial struct ErrorCode // : @string
        {
        }

 
        // Unexpected error
        public static readonly ErrorCode ErrInternalError = (ErrorCode)"regexp/syntax: internal error"; 

        // Parse errors
        public static readonly ErrorCode ErrInvalidCharClass = (ErrorCode)"invalid character class";
        public static readonly ErrorCode ErrInvalidCharRange = (ErrorCode)"invalid character class range";
        public static readonly ErrorCode ErrInvalidEscape = (ErrorCode)"invalid escape sequence";
        public static readonly ErrorCode ErrInvalidNamedCapture = (ErrorCode)"invalid named capture";
        public static readonly ErrorCode ErrInvalidPerlOp = (ErrorCode)"invalid or unsupported Perl syntax";
        public static readonly ErrorCode ErrInvalidRepeatOp = (ErrorCode)"invalid nested repetition operator";
        public static readonly ErrorCode ErrInvalidRepeatSize = (ErrorCode)"invalid repeat count";
        public static readonly ErrorCode ErrInvalidUTF8 = (ErrorCode)"invalid UTF-8";
        public static readonly ErrorCode ErrMissingBracket = (ErrorCode)"missing closing ]";
        public static readonly ErrorCode ErrMissingParen = (ErrorCode)"missing closing )";
        public static readonly ErrorCode ErrMissingRepeatArgument = (ErrorCode)"missing argument to repetition operator";
        public static readonly ErrorCode ErrTrailingBackslash = (ErrorCode)"trailing backslash at end of expression";
        public static readonly ErrorCode ErrUnexpectedParen = (ErrorCode)"unexpected )";


        public static @string String(this ErrorCode e)
        {
            return string(e);
        }

        // Flags control the behavior of the parser and record information about regexp context.
        public partial struct Flags // : ushort
        {
        }

        public static readonly Flags FoldCase = (Flags)1L << (int)(iota); // case-insensitive match
        public static readonly var Literal = 0; // treat pattern as literal string
        public static readonly var ClassNL = 1; // allow character classes like [^a-z] and [[:space:]] to match newline
        public static readonly var DotNL = 2; // allow . to match newline
        public static readonly var OneLine = 3; // treat ^ and $ as only matching at beginning and end of text
        public static readonly var NonGreedy = 4; // make repetition operators default to non-greedy
        public static readonly var PerlX = 5; // allow Perl extensions
        public static readonly var UnicodeGroups = 6; // allow \p{Han}, \P{Han} for Unicode group and negation
        public static readonly var WasDollar = 7; // regexp OpEndText was $, not \z
        public static readonly MatchNL Simple = (MatchNL)ClassNL | DotNL;

        public static readonly var Perl = ClassNL | OneLine | PerlX | UnicodeGroups; // as close to Perl as possible
        public static readonly Flags POSIX = (Flags)0L; // POSIX syntax

        // Pseudo-ops for parsing stack.
        private static readonly var opLeftParen = opPseudo + iota;
        private static readonly var opVerticalBar = 0;


        private partial struct parser
        {
            public Flags flags; // parse mode flags
            public slice<ptr<Regexp>> stack; // stack of parsed expressions
            public ptr<Regexp> free;
            public long numCap; // number of capturing groups seen
            public @string wholeRegexp;
            public slice<int> tmpClass; // temporary char class work space
        }

        private static ptr<Regexp> newRegexp(this ptr<parser> _addr_p, Op op)
        {
            ref parser p = ref _addr_p.val;

            var re = p.free;
            if (re != null)
            {
                p.free = re.Sub0[0L];
                re.val = new Regexp();
            }
            else
            {
                re = @new<Regexp>();
            }

            re.Op = op;
            return _addr_re!;

        }

        private static void reuse(this ptr<parser> _addr_p, ptr<Regexp> _addr_re)
        {
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            re.Sub0[0L] = p.free;
            p.free = re;
        }

        // Parse stack manipulation.

        // push pushes the regexp re onto the parse stack and returns the regexp.
        private static ptr<Regexp> push(this ptr<parser> _addr_p, ptr<Regexp> _addr_re)
        {
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpCharClass && len(re.Rune) == 2L && re.Rune[0L] == re.Rune[1L])
            { 
                // Single rune.
                if (p.maybeConcat(re.Rune[0L], p.flags & ~FoldCase))
                {
                    return _addr_null!;
                }

                re.Op = OpLiteral;
                re.Rune = re.Rune[..1L];
                re.Flags = p.flags & ~FoldCase;

            }
            else if (re.Op == OpCharClass && len(re.Rune) == 4L && re.Rune[0L] == re.Rune[1L] && re.Rune[2L] == re.Rune[3L] && unicode.SimpleFold(re.Rune[0L]) == re.Rune[2L] && unicode.SimpleFold(re.Rune[2L]) == re.Rune[0L] || re.Op == OpCharClass && len(re.Rune) == 2L && re.Rune[0L] + 1L == re.Rune[1L] && unicode.SimpleFold(re.Rune[0L]) == re.Rune[1L] && unicode.SimpleFold(re.Rune[1L]) == re.Rune[0L])
            { 
                // Case-insensitive rune like [Aa] or [Δδ].
                if (p.maybeConcat(re.Rune[0L], p.flags | FoldCase))
                {
                    return _addr_null!;
                } 

                // Rewrite as (case-insensitive) literal.
                re.Op = OpLiteral;
                re.Rune = re.Rune[..1L];
                re.Flags = p.flags | FoldCase;

            }
            else
            { 
                // Incremental concatenation.
                p.maybeConcat(-1L, 0L);

            }

            p.stack = append(p.stack, re);
            return _addr_re!;

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
        private static bool maybeConcat(this ptr<parser> _addr_p, int r, Flags flags)
        {
            ref parser p = ref _addr_p.val;

            var n = len(p.stack);
            if (n < 2L)
            {
                return false;
            }

            var re1 = p.stack[n - 1L];
            var re2 = p.stack[n - 2L];
            if (re1.Op != OpLiteral || re2.Op != OpLiteral || re1.Flags & FoldCase != re2.Flags & FoldCase)
            {
                return false;
            } 

            // Push re1 into re2.
            re2.Rune = append(re2.Rune, re1.Rune); 

            // Reuse re1 if possible.
            if (r >= 0L)
            {
                re1.Rune = re1.Rune0[..1L];
                re1.Rune[0L] = r;
                re1.Flags = flags;
                return true;
            }

            p.stack = p.stack[..n - 1L];
            p.reuse(re1);
            return false; // did not push r
        }

        // literal pushes a literal regexp for the rune r on the stack.
        private static void literal(this ptr<parser> _addr_p, int r)
        {
            ref parser p = ref _addr_p.val;

            var re = p.newRegexp(OpLiteral);
            re.Flags = p.flags;
            if (p.flags & FoldCase != 0L)
            {
                r = minFoldRune(r);
            }

            re.Rune0[0L] = r;
            re.Rune = re.Rune0[..1L];
            p.push(re);

        }

        // minFoldRune returns the minimum rune fold-equivalent to r.
        private static int minFoldRune(int r)
        {
            if (r < minFold || r > maxFold)
            {
                return r;
            }

            var min = r;
            var r0 = r;
            r = unicode.SimpleFold(r);

            while (r != r0)
            {
                if (min > r)
                {
                    min = r;
                r = unicode.SimpleFold(r);
                }

            }

            return min;

        }

        // op pushes a regexp with the given op onto the stack
        // and returns that regexp.
        private static ptr<Regexp> op(this ptr<parser> _addr_p, Op op)
        {
            ref parser p = ref _addr_p.val;

            var re = p.newRegexp(op);
            re.Flags = p.flags;
            return _addr_p.push(re)!;
        }

        // repeat replaces the top stack element with itself repeated according to op, min, max.
        // before is the regexp suffix starting at the repetition operator.
        // after is the regexp suffix following after the repetition operator.
        // repeat returns an updated 'after' and an error, if any.
        private static (@string, error) repeat(this ptr<parser> _addr_p, Op op, long min, long max, @string before, @string after, @string lastRepeat)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref parser p = ref _addr_p.val;

            var flags = p.flags;
            if (p.flags & PerlX != 0L)
            {
                if (len(after) > 0L && after[0L] == '?')
                {
                    after = after[1L..];
                    flags ^= NonGreedy;
                }

                if (lastRepeat != "")
                { 
                    // In Perl it is not allowed to stack repetition operators:
                    // a** is a syntax error, not a doubled star, and a++ means
                    // something else entirely, which we don't support!
                    return ("", error.As(addr(new Error(ErrInvalidRepeatOp,lastRepeat[:len(lastRepeat)-len(after)]))!)!);

                }

            }

            var n = len(p.stack);
            if (n == 0L)
            {
                return ("", error.As(addr(new Error(ErrMissingRepeatArgument,before[:len(before)-len(after)]))!)!);
            }

            var sub = p.stack[n - 1L];
            if (sub.Op >= opPseudo)
            {
                return ("", error.As(addr(new Error(ErrMissingRepeatArgument,before[:len(before)-len(after)]))!)!);
            }

            var re = p.newRegexp(op);
            re.Min = min;
            re.Max = max;
            re.Flags = flags;
            re.Sub = re.Sub0[..1L];
            re.Sub[0L] = sub;
            p.stack[n - 1L] = re;

            if (op == OpRepeat && (min >= 2L || max >= 2L) && !repeatIsValid(_addr_re, 1000L))
            {
                return ("", error.As(addr(new Error(ErrInvalidRepeatSize,before[:len(before)-len(after)]))!)!);
            }

            return (after, error.As(null!)!);

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
        private static bool repeatIsValid(ptr<Regexp> _addr_re, long n)
        {
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpRepeat)
            {
                var m = re.Max;
                if (m == 0L)
                {
                    return true;
                }

                if (m < 0L)
                {
                    m = re.Min;
                }

                if (m > n)
                {
                    return false;
                }

                if (m > 0L)
                {
                    n /= m;
                }

            }

            foreach (var (_, sub) in re.Sub)
            {
                if (!repeatIsValid(_addr_sub, n))
                {
                    return false;
                }

            }
            return true;

        }

        // concat replaces the top of the stack (above the topmost '|' or '(') with its concatenation.
        private static ptr<Regexp> concat(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            p.maybeConcat(-1L, 0L); 

            // Scan down to find pseudo-operator | or (.
            var i = len(p.stack);
            while (i > 0L && p.stack[i - 1L].Op < opPseudo)
            {
                i--;
            }

            var subs = p.stack[i..];
            p.stack = p.stack[..i]; 

            // Empty concatenation is special case.
            if (len(subs) == 0L)
            {
                return _addr_p.push(p.newRegexp(OpEmptyMatch))!;
            }

            return _addr_p.push(p.collapse(subs, OpConcat))!;

        }

        // alternate replaces the top of the stack (above the topmost '(') with its alternation.
        private static ptr<Regexp> alternate(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;
 
            // Scan down to find pseudo-operator (.
            // There are no | above (.
            var i = len(p.stack);
            while (i > 0L && p.stack[i - 1L].Op < opPseudo)
            {
                i--;
            }

            var subs = p.stack[i..];
            p.stack = p.stack[..i]; 

            // Make sure top class is clean.
            // All the others already are (see swapVerticalBar).
            if (len(subs) > 0L)
            {
                cleanAlt(_addr_subs[len(subs) - 1L]);
            } 

            // Empty alternate is special case
            // (shouldn't happen but easy to handle).
            if (len(subs) == 0L)
            {
                return _addr_p.push(p.newRegexp(OpNoMatch))!;
            }

            return _addr_p.push(p.collapse(subs, OpAlternate))!;

        }

        // cleanAlt cleans re for eventual inclusion in an alternation.
        private static void cleanAlt(ptr<Regexp> _addr_re)
        {
            ref Regexp re = ref _addr_re.val;


            if (re.Op == OpCharClass) 
                re.Rune = cleanClass(_addr_re.Rune);
                if (len(re.Rune) == 2L && re.Rune[0L] == 0L && re.Rune[1L] == unicode.MaxRune)
                {
                    re.Rune = null;
                    re.Op = OpAnyChar;
                    return ;
                }

                if (len(re.Rune) == 4L && re.Rune[0L] == 0L && re.Rune[1L] == '\n' - 1L && re.Rune[2L] == '\n' + 1L && re.Rune[3L] == unicode.MaxRune)
                {
                    re.Rune = null;
                    re.Op = OpAnyCharNotNL;
                    return ;
                }

                if (cap(re.Rune) - len(re.Rune) > 100L)
                { 
                    // re.Rune will not grow any more.
                    // Make a copy or inline to reclaim storage.
                    re.Rune = append(re.Rune0[..0L], re.Rune);

                }

                    }

        // collapse returns the result of applying op to sub.
        // If sub contains op nodes, they all get hoisted up
        // so that there is never a concat of a concat or an
        // alternate of an alternate.
        private static ptr<Regexp> collapse(this ptr<parser> _addr_p, slice<ptr<Regexp>> subs, Op op)
        {
            ref parser p = ref _addr_p.val;

            if (len(subs) == 1L)
            {
                return _addr_subs[0L]!;
            }

            var re = p.newRegexp(op);
            re.Sub = re.Sub0[..0L];
            foreach (var (_, sub) in subs)
            {
                if (sub.Op == op)
                {
                    re.Sub = append(re.Sub, sub.Sub);
                    p.reuse(sub);
                }
                else
                {
                    re.Sub = append(re.Sub, sub);
                }

            }
            if (op == OpAlternate)
            {
                re.Sub = p.factor(re.Sub);
                if (len(re.Sub) == 1L)
                {
                    var old = re;
                    re = re.Sub[0L];
                    p.reuse(old);
                }

            }

            return _addr_re!;

        }

        // factor factors common prefixes from the alternation list sub.
        // It returns a replacement list that reuses the same storage and
        // frees (passes to p.reuse) any removed *Regexps.
        //
        // For example,
        //     ABC|ABD|AEF|BCX|BCY
        // simplifies by literal prefix extraction to
        //     A(B(C|D)|EF)|BC(X|Y)
        // which simplifies by character class introduction to
        //     A(B[CD]|EF)|BC[XY]
        //
        private static slice<ptr<Regexp>> factor(this ptr<parser> _addr_p, slice<ptr<Regexp>> sub)
        {
            ref parser p = ref _addr_p.val;

            if (len(sub) < 2L)
            {
                return sub;
            } 

            // Round 1: Factor out common literal prefixes.
            slice<int> str = default;
            Flags strflags = default;
            long start = 0L;
            var @out = sub[..0L];
            {
                long i__prev1 = i;

                for (long i = 0L; i <= len(sub); i++)
                { 
                    // Invariant: the Regexps that were in sub[0:start] have been
                    // used or marked for reuse, and the slice space has been reused
                    // for out (len(out) <= start).
                    //
                    // Invariant: sub[start:i] consists of regexps that all begin
                    // with str as modified by strflags.
                    slice<int> istr = default;
                    Flags iflags = default;
                    if (i < len(sub))
                    {
                        istr, iflags = p.leadingString(sub[i]);
                        if (iflags == strflags)
                        {
                            long same = 0L;
                            while (same < len(str) && same < len(istr) && str[same] == istr[same])
                            {
                                same++;
                            }

                            if (same > 0L)
                            { 
                                // Matches at least one rune in current range.
                                // Keep going around.
                                str = str[..same];
                                continue;

                            }

                        }

                    } 

                    // Found end of a run with common leading literal string:
                    // sub[start:i] all begin with str[0:len(str)], but sub[i]
                    // does not even begin with str[0].
                    //
                    // Factor out common string and append factored expression to out.
                    if (i == start)
                    { 
                        // Nothing to do - run of length 0.
                    }
                    else if (i == start + 1L)
                    { 
                        // Just one: don't bother factoring.
                        out = append(out, sub[start]);

                    }
                    else
                    { 
                        // Construct factored form: prefix(suffix1|suffix2|...)
                        var prefix = p.newRegexp(OpLiteral);
                        prefix.Flags = strflags;
                        prefix.Rune = append(prefix.Rune[..0L], str);

                        {
                            var j__prev2 = j;

                            for (var j = start; j < i; j++)
                            {
                                sub[j] = p.removeLeadingString(sub[j], len(str));
                            }


                            j = j__prev2;
                        }
                        var suffix = p.collapse(sub[start..i], OpAlternate); // recurse

                        var re = p.newRegexp(OpConcat);
                        re.Sub = append(re.Sub[..0L], prefix, suffix);
                        out = append(out, re);

                    } 

                    // Prepare for next iteration.
                    start = i;
                    str = istr;
                    strflags = iflags;

                }


                i = i__prev1;
            }
            sub = out; 

            // Round 2: Factor out common simple prefixes,
            // just the first piece of each concatenation.
            // This will be good enough a lot of the time.
            //
            // Complex subexpressions (e.g. involving quantifiers)
            // are not safe to factor because that collapses their
            // distinct paths through the automaton, which affects
            // correctness in some cases.
            start = 0L;
            out = sub[..0L];
            ptr<Regexp> first;
            {
                long i__prev1 = i;

                for (i = 0L; i <= len(sub); i++)
                { 
                    // Invariant: the Regexps that were in sub[0:start] have been
                    // used or marked for reuse, and the slice space has been reused
                    // for out (len(out) <= start).
                    //
                    // Invariant: sub[start:i] consists of regexps that all begin with ifirst.
                    ptr<Regexp> ifirst;
                    if (i < len(sub))
                    {
                        ifirst = p.leadingRegexp(sub[i]);
                        if (first != null && first.Equal(ifirst) && (isCharClass(first) || (first.Op == OpRepeat && first.Min == first.Max && isCharClass(_addr_first.Sub[0L]))))
                        {
                            continue;
                        }

                    } 

                    // Found end of a run with common leading regexp:
                    // sub[start:i] all begin with first but sub[i] does not.
                    //
                    // Factor out common regexp and append factored expression to out.
                    if (i == start)
                    { 
                        // Nothing to do - run of length 0.
                    }
                    else if (i == start + 1L)
                    { 
                        // Just one: don't bother factoring.
                        out = append(out, sub[start]);

                    }
                    else
                    { 
                        // Construct factored form: prefix(suffix1|suffix2|...)
                        prefix = first;
                        {
                            var j__prev2 = j;

                            for (j = start; j < i; j++)
                            {
                                var reuse = j != start; // prefix came from sub[start]
                                sub[j] = p.removeLeadingRegexp(sub[j], reuse);

                            }


                            j = j__prev2;
                        }
                        suffix = p.collapse(sub[start..i], OpAlternate); // recurse

                        re = p.newRegexp(OpConcat);
                        re.Sub = append(re.Sub[..0L], prefix, suffix);
                        out = append(out, re);

                    } 

                    // Prepare for next iteration.
                    start = i;
                    first = addr(ifirst);

                }


                i = i__prev1;
            }
            sub = out; 

            // Round 3: Collapse runs of single literals into character classes.
            start = 0L;
            out = sub[..0L];
            {
                long i__prev1 = i;

                for (i = 0L; i <= len(sub); i++)
                { 
                    // Invariant: the Regexps that were in sub[0:start] have been
                    // used or marked for reuse, and the slice space has been reused
                    // for out (len(out) <= start).
                    //
                    // Invariant: sub[start:i] consists of regexps that are either
                    // literal runes or character classes.
                    if (i < len(sub) && isCharClass(_addr_sub[i]))
                    {
                        continue;
                    } 

                    // sub[i] is not a char or char class;
                    // emit char class for sub[start:i]...
                    if (i == start)
                    { 
                        // Nothing to do - run of length 0.
                    }
                    else if (i == start + 1L)
                    {
                        out = append(out, sub[start]);
                    }
                    else
                    { 
                        // Make new char class.
                        // Start with most complex regexp in sub[start].
                        var max = start;
                        {
                            var j__prev2 = j;

                            for (j = start + 1L; j < i; j++)
                            {
                                if (sub[max].Op < sub[j].Op || sub[max].Op == sub[j].Op && len(sub[max].Rune) < len(sub[j].Rune))
                                {
                                    max = j;
                                }

                            }


                            j = j__prev2;
                        }
                        sub[start] = sub[max];
                        sub[max] = sub[start];

                        {
                            var j__prev2 = j;

                            for (j = start + 1L; j < i; j++)
                            {
                                mergeCharClass(_addr_sub[start], _addr_sub[j]);
                                p.reuse(sub[j]);
                            }


                            j = j__prev2;
                        }
                        cleanAlt(_addr_sub[start]);
                        out = append(out, sub[start]);

                    } 

                    // ... and then emit sub[i].
                    if (i < len(sub))
                    {
                        out = append(out, sub[i]);
                    }

                    start = i + 1L;

                }


                i = i__prev1;
            }
            sub = out; 

            // Round 4: Collapse runs of empty matches into a single empty match.
            start = 0L;
            out = sub[..0L];
            {
                long i__prev1 = i;

                foreach (var (__i) in sub)
                {
                    i = __i;
                    if (i + 1L < len(sub) && sub[i].Op == OpEmptyMatch && sub[i + 1L].Op == OpEmptyMatch)
                    {
                        continue;
                    }

                    out = append(out, sub[i]);

                }

                i = i__prev1;
            }

            sub = out;

            return sub;

        }

        // leadingString returns the leading literal string that re begins with.
        // The string refers to storage in re or its children.
        private static (slice<int>, Flags) leadingString(this ptr<parser> _addr_p, ptr<Regexp> _addr_re)
        {
            slice<int> _p0 = default;
            Flags _p0 = default;
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpConcat && len(re.Sub) > 0L)
            {
                re = re.Sub[0L];
            }

            if (re.Op != OpLiteral)
            {
                return (null, 0L);
            }

            return (re.Rune, re.Flags & FoldCase);

        }

        // removeLeadingString removes the first n leading runes
        // from the beginning of re. It returns the replacement for re.
        private static ptr<Regexp> removeLeadingString(this ptr<parser> _addr_p, ptr<Regexp> _addr_re, long n)
        {
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpConcat && len(re.Sub) > 0L)
            { 
                // Removing a leading string in a concatenation
                // might simplify the concatenation.
                var sub = re.Sub[0L];
                sub = p.removeLeadingString(sub, n);
                re.Sub[0L] = sub;
                if (sub.Op == OpEmptyMatch)
                {
                    p.reuse(sub);
                    switch (len(re.Sub))
                    {
                        case 0L: 
                            // Impossible but handle.

                        case 1L: 
                            // Impossible but handle.
                            re.Op = OpEmptyMatch;
                            re.Sub = null;
                            break;
                        case 2L: 
                            var old = re;
                            re = re.Sub[1L];
                            p.reuse(old);
                            break;
                        default: 
                            copy(re.Sub, re.Sub[1L..]);
                            re.Sub = re.Sub[..len(re.Sub) - 1L];
                            break;
                    }

                }

                return _addr_re!;

            }

            if (re.Op == OpLiteral)
            {
                re.Rune = re.Rune[..copy(re.Rune, re.Rune[n..])];
                if (len(re.Rune) == 0L)
                {
                    re.Op = OpEmptyMatch;
                }

            }

            return _addr_re!;

        }

        // leadingRegexp returns the leading regexp that re begins with.
        // The regexp refers to storage in re or its children.
        private static ptr<Regexp> leadingRegexp(this ptr<parser> _addr_p, ptr<Regexp> _addr_re)
        {
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpEmptyMatch)
            {
                return _addr_null!;
            }

            if (re.Op == OpConcat && len(re.Sub) > 0L)
            {
                var sub = re.Sub[0L];
                if (sub.Op == OpEmptyMatch)
                {
                    return _addr_null!;
                }

                return _addr_sub!;

            }

            return _addr_re!;

        }

        // removeLeadingRegexp removes the leading regexp in re.
        // It returns the replacement for re.
        // If reuse is true, it passes the removed regexp (if no longer needed) to p.reuse.
        private static ptr<Regexp> removeLeadingRegexp(this ptr<parser> _addr_p, ptr<Regexp> _addr_re, bool reuse)
        {
            ref parser p = ref _addr_p.val;
            ref Regexp re = ref _addr_re.val;

            if (re.Op == OpConcat && len(re.Sub) > 0L)
            {
                if (reuse)
                {
                    p.reuse(re.Sub[0L]);
                }

                re.Sub = re.Sub[..copy(re.Sub, re.Sub[1L..])];
                switch (len(re.Sub))
                {
                    case 0L: 
                        re.Op = OpEmptyMatch;
                        re.Sub = null;
                        break;
                    case 1L: 
                        var old = re;
                        re = re.Sub[0L];
                        p.reuse(old);
                        break;
                }
                return _addr_re!;

            }

            if (reuse)
            {
                p.reuse(re);
            }

            return _addr_p.newRegexp(OpEmptyMatch)!;

        }

        private static ptr<Regexp> literalRegexp(@string s, Flags flags)
        {
            ptr<Regexp> re = addr(new Regexp(Op:OpLiteral));
            re.Flags = flags;
            re.Rune = re.Rune0[..0L]; // use local storage for small strings
            foreach (var (_, c) in s)
            {
                if (len(re.Rune) >= cap(re.Rune))
                { 
                    // string is too long to fit in Rune0.  let Go handle it
                    re.Rune = (slice<int>)s;
                    break;

                }

                re.Rune = append(re.Rune, c);

            }
            return _addr_re!;

        }

        // Parsing.

        // Parse parses a regular expression string s, controlled by the specified
        // Flags, and returns a regular expression parse tree. The syntax is
        // described in the top-level comment.
        public static (ptr<Regexp>, error) Parse(@string s, Flags flags)
        {
            ptr<Regexp> _p0 = default!;
            error _p0 = default!;

            if (flags & Literal != 0L)
            { 
                // Trivial parser for literal string.
                {
                    var err__prev2 = err;

                    var err = checkUTF8(s);

                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev2;

                }

                return (_addr_literalRegexp(s, flags)!, error.As(null!)!);

            } 

            // Otherwise, must do real work.
            parser p = default;            err = default!;            int c = default;            Op op = default;            @string lastRepeat = default;
            p.flags = flags;
            p.wholeRegexp = s;
            var t = s;
            while (t != "")
            {
                @string repeat = "";
BigSwitch:
                switch (t[0L])
                {
                    case '(': 
                        if (p.flags & PerlX != 0L && len(t) >= 2L && t[1L] == '?')
                        { 
                            // Flag changes and non-capturing groups.
                            t, err = p.parsePerlFlags(t);

                            if (err != null)
                            {
                                return (_addr_null!, error.As(err)!);
                            }

                            break;

                        }

                        p.numCap++;
                        p.op(opLeftParen).Cap;

                        p.numCap;
                        t = t[1L..];
                        break;
                    case '|': 
                        err = p.parseVerticalBar();

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        t = t[1L..];
                        break;
                    case ')': 
                        err = p.parseRightParen();

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        t = t[1L..];
                        break;
                    case '^': 
                        if (p.flags & OneLine != 0L)
                        {
                            p.op(OpBeginText);
                        }
                        else
                        {
                            p.op(OpBeginLine);
                        }

                        t = t[1L..];
                        break;
                    case '$': 
                        if (p.flags & OneLine != 0L)
                        {
                            p.op(OpEndText).Flags;

                            WasDollar;

                        }
                        else
                        {
                            p.op(OpEndLine);
                        }

                        t = t[1L..];
                        break;
                    case '.': 
                        if (p.flags & DotNL != 0L)
                        {
                            p.op(OpAnyChar);
                        }
                        else
                        {
                            p.op(OpAnyCharNotNL);
                        }

                        t = t[1L..];
                        break;
                    case '[': 
                        t, err = p.parseClass(t);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        break;
                    case '*': 

                    case '+': 

                    case '?': 
                        var before = t;
                        switch (t[0L])
                        {
                            case '*': 
                                op = OpStar;
                                break;
                            case '+': 
                                op = OpPlus;
                                break;
                            case '?': 
                                op = OpQuest;
                                break;
                        }
                        var after = t[1L..];
                        after, err = p.repeat(op, 0L, 0L, before, after, lastRepeat);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        repeat = before;
                        t = after;
                        break;
                    case '{': 
                        op = OpRepeat;
                        before = t;
                        var (min, max, after, ok) = p.parseRepeat(t);
                        if (!ok)
                        { 
                            // If the repeat cannot be parsed, { is a literal.
                            p.literal('{');
                            t = t[1L..];
                            break;

                        }

                        if (min < 0L || min > 1000L || max > 1000L || max >= 0L && min > max)
                        { 
                            // Numbers were too big, or max is present and min > max.
                            return (_addr_null!, error.As(addr(new Error(ErrInvalidRepeatSize,before[:len(before)-len(after)]))!)!);

                        }

                        after, err = p.repeat(op, min, max, before, after, lastRepeat);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        repeat = before;
                        t = after;
                        break;
                    case '\\': 
                        if (p.flags & PerlX != 0L && len(t) >= 2L)
                        {
                            switch (t[1L])
                            {
                                case 'A': 
                                    p.op(OpBeginText);
                                    t = t[2L..];
                                    _breakBigSwitch = true;
                                    break;
                                    break;
                                case 'b': 
                                    p.op(OpWordBoundary);
                                    t = t[2L..];
                                    _breakBigSwitch = true;
                                    break;
                                    break;
                                case 'B': 
                                    p.op(OpNoWordBoundary);
                                    t = t[2L..];
                                    _breakBigSwitch = true;
                                    break;
                                    break;
                                case 'C': 
                                    // any byte; not supported
                                    return (_addr_null!, error.As(addr(new Error(ErrInvalidEscape,t[:2]))!)!);
                                    break;
                                case 'Q': 
                                    // \Q ... \E: the ... is always literals
                                    @string lit = default;
                                    {
                                        var i = strings.Index(t, "\\E");

                                        if (i < 0L)
                                        {
                                            lit = t[2L..];
                                            t = "";
                                        }
                                        else
                                        {
                                            lit = t[2L..i];
                                            t = t[i + 2L..];
                                        }

                                    }

                                    while (lit != "")
                                    {
                                        var (c, rest, err) = nextRune(lit);
                                        if (err != null)
                                        {
                                            return (_addr_null!, error.As(err)!);
                                        }

                                        p.literal(c);
                                        lit = rest;

                                    }

                                    _breakBigSwitch = true;
                                    break;
                                    break;
                                case 'z': 
                                    p.op(OpEndText);
                                    t = t[2L..];
                                    _breakBigSwitch = true;
                                    break;
                                    break;
                            }

                        }

                        var re = p.newRegexp(OpCharClass);
                        re.Flags = p.flags; 

                        // Look for Unicode character group like \p{Han}
                        if (len(t) >= 2L && (t[1L] == 'p' || t[1L] == 'P'))
                        {
                            var (r, rest, err) = p.parseUnicodeClass(t, re.Rune0[..0L]);
                            if (err != null)
                            {
                                return (_addr_null!, error.As(err)!);
                            }

                            if (r != null)
                            {
                                re.Rune = r;
                                t = rest;
                                p.push(re);
                                _breakBigSwitch = true;
                                break;
                            }

                        } 

                        // Perl character class escape.
                        {
                            var r__prev1 = r;

                            var (r, rest) = p.parsePerlClassEscape(t, re.Rune0[..0L]);

                            if (r != null)
                            {
                                re.Rune = r;
                                t = rest;
                                p.push(re);
                                _breakBigSwitch = true;
                                break;
                            }

                            r = r__prev1;

                        }

                        p.reuse(re); 

                        // Ordinary single-character escape.
                        c, t, err = p.parseEscape(t);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        p.literal(c);
                        break;
                    default: 
                        c, t, err = nextRune(t);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        p.literal(c);
                        break;
                }
                lastRepeat = repeat;

            }


            p.concat();
            if (p.swapVerticalBar())
            { 
                // pop vertical bar
                p.stack = p.stack[..len(p.stack) - 1L];

            }

            p.alternate();

            var n = len(p.stack);
            if (n != 1L)
            {
                return (_addr_null!, error.As(addr(new Error(ErrMissingParen,s))!)!);
            }

            return (_addr_p.stack[0L]!, error.As(null!)!);

        }

        // parseRepeat parses {min} (max=min) or {min,} (max=-1) or {min,max}.
        // If s is not of that form, it returns ok == false.
        // If s has the right form but the values are too big, it returns min == -1, ok == true.
        private static (long, long, @string, bool) parseRepeat(this ptr<parser> _addr_p, @string s)
        {
            long min = default;
            long max = default;
            @string rest = default;
            bool ok = default;
            ref parser p = ref _addr_p.val;

            if (s == "" || s[0L] != '{')
            {
                return ;
            }

            s = s[1L..];
            bool ok1 = default;
            min, s, ok1 = p.parseInt(s);

            if (!ok1)
            {
                return ;
            }

            if (s == "")
            {
                return ;
            }

            if (s[0L] != ',')
            {
                max = min;
            }
            else
            {
                s = s[1L..];
                if (s == "")
                {
                    return ;
                }

                if (s[0L] == '}')
                {
                    max = -1L;
                }                max, s, ok1 = p.parseInt(s);


                else if (!ok1)
                {
                    return ;
                }
                else if (max < 0L)
                { 
                    // parseInt found too big a number
                    min = -1L;

                }

            }

            if (s == "" || s[0L] != '}')
            {
                return ;
            }

            rest = s[1L..];
            ok = true;
            return ;

        }

        // parsePerlFlags parses a Perl flag setting or non-capturing group or both,
        // like (?i) or (?: or (?i:.  It removes the prefix from s and updates the parse state.
        // The caller must have ensured that s begins with "(?".
        private static (@string, error) parsePerlFlags(this ptr<parser> _addr_p, @string s)
        {
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            var t = s; 

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
            // Google source tree, (?P<expr>name) is the dominant form,
            // so that's the one we implement. One is enough.
            if (len(t) > 4L && t[2L] == 'P' && t[3L] == '<')
            { 
                // Pull out name.
                var end = strings.IndexRune(t, '>');
                if (end < 0L)
                {
                    err = checkUTF8(t);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    return ("", error.As(addr(new Error(ErrInvalidNamedCapture,s))!)!);

                }

                var capture = t[..end + 1L]; // "(?P<name>"
                var name = t[4L..end]; // "name"
                err = checkUTF8(name);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                if (!isValidCaptureName(name))
                {
                    return ("", error.As(addr(new Error(ErrInvalidNamedCapture,capture))!)!);
                } 

                // Like ordinary capture, but named.
                p.numCap++;
                var re = p.op(opLeftParen);
                re.Cap = p.numCap;
                re.Name = name;
                return (t[end + 1L..], error.As(null!)!);

            } 

            // Non-capturing group. Might also twiddle Perl flags.
            int c = default;
            t = t[2L..]; // skip (?
            var flags = p.flags;
            long sign = +1L;
            var sawFlag = false;
Loop:

            while (t != "")
            {
                c, t, err = nextRune(t);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                switch (c)
                {
                    case 'i': 
                        flags |= FoldCase;
                        sawFlag = true;
                        break;
                    case 'm': 
                        flags &= OneLine;
                        sawFlag = true;
                        break;
                    case 's': 
                        flags |= DotNL;
                        sawFlag = true;
                        break;
                    case 'U': 
                        flags |= NonGreedy;
                        sawFlag = true; 

                        // Switch to negation.
                        break;
                    case '-': 
                        if (sign < 0L)
                        {
                            _breakLoop = true;
                            break;
                        }

                        sign = -1L; 
                        // Invert flags so that | above turn into &^ and vice versa.
                        // We'll invert flags again before using it below.
                        flags = ~flags;
                        sawFlag = false; 

                        // End of flags, starting group or not.
                        break;
                    case ':': 

                    case ')': 
                        if (sign < 0L)
                        {
                            if (!sawFlag)
                            {
                                _breakLoop = true;
                                break;
                            }

                            flags = ~flags;

                        }

                        if (c == ':')
                        { 
                            // Open new group
                            p.op(opLeftParen);

                        }

                        p.flags = flags;
                        return (t, error.As(null!)!);
                        break;
                    default: 
                        _breakLoop = true; 

                        // Flags.
                        break;
                        break;
                }

            }

            return ("", error.As(addr(new Error(ErrInvalidPerlOp,s[:len(s)-len(t)]))!)!);

        }

        // isValidCaptureName reports whether name
        // is a valid capture name: [A-Za-z0-9_]+.
        // PCRE limits names to 32 bytes.
        // Python rejects names starting with digits.
        // We don't enforce either of those.
        private static bool isValidCaptureName(@string name)
        {
            if (name == "")
            {
                return false;
            }

            foreach (var (_, c) in name)
            {
                if (c != '_' && !isalnum(c))
                {
                    return false;
                }

            }
            return true;

        }

        // parseInt parses a decimal integer.
        private static (long, @string, bool) parseInt(this ptr<parser> _addr_p, @string s)
        {
            long n = default;
            @string rest = default;
            bool ok = default;
            ref parser p = ref _addr_p.val;

            if (s == "" || s[0L] < '0' || '9' < s[0L])
            {
                return ;
            } 
            // Disallow leading zeros.
            if (len(s) >= 2L && s[0L] == '0' && '0' <= s[1L] && s[1L] <= '9')
            {
                return ;
            }

            var t = s;
            while (s != "" && '0' <= s[0L] && s[0L] <= '9')
            {
                s = s[1L..];
            }

            rest = s;
            ok = true; 
            // Have digits, compute value.
            t = t[..len(t) - len(s)];
            for (long i = 0L; i < len(t); i++)
            { 
                // Avoid overflow.
                if (n >= 1e8F)
                {
                    n = -1L;
                    break;
                }

                n = n * 10L + int(t[i]) - '0';

            }

            return ;

        }

        // can this be represented as a character class?
        // single-rune literal string, char class, ., and .|\n.
        private static bool isCharClass(ptr<Regexp> _addr_re)
        {
            ref Regexp re = ref _addr_re.val;

            return re.Op == OpLiteral && len(re.Rune) == 1L || re.Op == OpCharClass || re.Op == OpAnyCharNotNL || re.Op == OpAnyChar;
        }

        // does re match r?
        private static bool matchRune(ptr<Regexp> _addr_re, int r)
        {
            ref Regexp re = ref _addr_re.val;


            if (re.Op == OpLiteral) 
                return len(re.Rune) == 1L && re.Rune[0L] == r;
            else if (re.Op == OpCharClass) 
                {
                    long i = 0L;

                    while (i < len(re.Rune))
                    {
                        if (re.Rune[i] <= r && r <= re.Rune[i + 1L])
                        {
                            return true;
                        i += 2L;
                        }

                    }

                }
                return false;
            else if (re.Op == OpAnyCharNotNL) 
                return r != '\n';
            else if (re.Op == OpAnyChar) 
                return true;
                        return false;

        }

        // parseVerticalBar handles a | in the input.
        private static error parseVerticalBar(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            p.concat(); 

            // The concatenation we just parsed is on top of the stack.
            // If it sits above an opVerticalBar, swap it below
            // (things below an opVerticalBar become an alternation).
            // Otherwise, push a new vertical bar.
            if (!p.swapVerticalBar())
            {
                p.op(opVerticalBar);
            }

            return error.As(null!)!;

        }

        // mergeCharClass makes dst = dst|src.
        // The caller must ensure that dst.Op >= src.Op,
        // to reduce the amount of copying.
        private static void mergeCharClass(ptr<Regexp> _addr_dst, ptr<Regexp> _addr_src)
        {
            ref Regexp dst = ref _addr_dst.val;
            ref Regexp src = ref _addr_src.val;


            if (dst.Op == OpAnyChar)             else if (dst.Op == OpAnyCharNotNL) 
                // src might add \n
                if (matchRune(_addr_src, '\n'))
                {
                    dst.Op = OpAnyChar;
                }

            else if (dst.Op == OpCharClass) 
                // src is simpler, so either literal or char class
                if (src.Op == OpLiteral)
                {
                    dst.Rune = appendLiteral(dst.Rune, src.Rune[0L], src.Flags);
                }
                else
                {
                    dst.Rune = appendClass(dst.Rune, src.Rune);
                }

            else if (dst.Op == OpLiteral) 
                // both literal
                if (src.Rune[0L] == dst.Rune[0L] && src.Flags == dst.Flags)
                {
                    break;
                }

                dst.Op = OpCharClass;
                dst.Rune = appendLiteral(dst.Rune[..0L], dst.Rune[0L], dst.Flags);
                dst.Rune = appendLiteral(dst.Rune, src.Rune[0L], src.Flags);
            
        }

        // If the top of the stack is an element followed by an opVerticalBar
        // swapVerticalBar swaps the two and returns true.
        // Otherwise it returns false.
        private static bool swapVerticalBar(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;
 
            // If above and below vertical bar are literal or char class,
            // can merge into a single char class.
            var n = len(p.stack);
            if (n >= 3L && p.stack[n - 2L].Op == opVerticalBar && isCharClass(_addr_p.stack[n - 1L]) && isCharClass(_addr_p.stack[n - 3L]))
            {
                var re1 = p.stack[n - 1L];
                var re3 = p.stack[n - 3L]; 
                // Make re3 the more complex of the two.
                if (re1.Op > re3.Op)
                {
                    re1 = re3;
                    re3 = re1;
                    p.stack[n - 3L] = re3;

                }

                mergeCharClass(_addr_re3, _addr_re1);
                p.reuse(re1);
                p.stack = p.stack[..n - 1L];
                return true;

            }

            if (n >= 2L)
            {
                re1 = p.stack[n - 1L];
                var re2 = p.stack[n - 2L];
                if (re2.Op == opVerticalBar)
                {
                    if (n >= 3L)
                    { 
                        // Now out of reach.
                        // Clean opportunistically.
                        cleanAlt(_addr_p.stack[n - 3L]);

                    }

                    p.stack[n - 2L] = re1;
                    p.stack[n - 1L] = re2;
                    return true;

                }

            }

            return false;

        }

        // parseRightParen handles a ) in the input.
        private static error parseRightParen(this ptr<parser> _addr_p)
        {
            ref parser p = ref _addr_p.val;

            p.concat();
            if (p.swapVerticalBar())
            { 
                // pop vertical bar
                p.stack = p.stack[..len(p.stack) - 1L];

            }

            p.alternate();

            var n = len(p.stack);
            if (n < 2L)
            {
                return error.As(addr(new Error(ErrUnexpectedParen,p.wholeRegexp))!)!;
            }

            var re1 = p.stack[n - 1L];
            var re2 = p.stack[n - 2L];
            p.stack = p.stack[..n - 2L];
            if (re2.Op != opLeftParen)
            {
                return error.As(addr(new Error(ErrUnexpectedParen,p.wholeRegexp))!)!;
            } 
            // Restore flags at time of paren.
            p.flags = re2.Flags;
            if (re2.Cap == 0L)
            { 
                // Just for grouping.
                p.push(re1);

            }
            else
            {
                re2.Op = OpCapture;
                re2.Sub = re2.Sub0[..1L];
                re2.Sub[0L] = re1;
                p.push(re2);
            }

            return error.As(null!)!;

        }

        // parseEscape parses an escape sequence at the beginning of s
        // and returns the rune.
        private static (int, @string, error) parseEscape(this ptr<parser> _addr_p, @string s)
        {
            int r = default;
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            var t = s[1L..];
            if (t == "")
            {
                return (0L, "", error.As(addr(new Error(ErrTrailingBackslash,""))!)!);
            }

            var (c, t, err) = nextRune(t);
            if (err != null)
            {
                return (0L, "", error.As(err)!);
            }

Switch:

            if (c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7') 
            {
                // Single non-zero digit is a backreference; not supported
                if (t == "" || t[0L] < '0' || t[0L] > '7')
                {
                    break;
                }

                fallthrough = true;
            }
            if (fallthrough || c == '0') 
            {
                // Consume up to three octal digits; already have one.
                r = c - '0';
                for (long i = 1L; i < 3L; i++)
                {
                    if (t == "" || t[0L] < '0' || t[0L] > '7')
                    {
                        break;
                    }

                    r = r * 8L + rune(t[0L]) - '0';
                    t = t[1L..];

                }

                return (r, t, error.As(null!)!); 

                // Hexadecimal escapes.
                goto __switch_break0;
            }
            if (c == 'x')
            {
                if (t == "")
                {
                    break;
                }

                c, t, err = nextRune(t);

                if (err != null)
                {
                    return (0L, "", error.As(err)!);
                }

                if (c == '{')
                { 
                    // Any number of digits in braces.
                    // Perl accepts any text at all; it ignores all text
                    // after the first non-hex digit. We require only hex digits,
                    // and at least one.
                    long nhex = 0L;
                    r = 0L;
                    while (true)
                    {
                        if (t == "")
                        {
                            _breakSwitch = true;
                            break;
                        }

                        c, t, err = nextRune(t);

                        if (err != null)
                        {
                            return (0L, "", error.As(err)!);
                        }

                        if (c == '}')
                        {
                            break;
                        }

                        var v = unhex(c);
                        if (v < 0L)
                        {
                            _breakSwitch = true;
                            break;
                        }

                        r = r * 16L + v;
                        if (r > unicode.MaxRune)
                        {
                            _breakSwitch = true;
                            break;
                        }

                        nhex++;

                    }

                    if (nhex == 0L)
                    {
                        _breakSwitch = true;
                        break;
                    }

                    return (r, t, error.As(null!)!);

                } 

                // Easy case: two hex digits.
                var x = unhex(c);
                c, t, err = nextRune(t);

                if (err != null)
                {
                    return (0L, "", error.As(err)!);
                }

                var y = unhex(c);
                if (x < 0L || y < 0L)
                {
                    break;
                }

                return (x * 16L + y, t, error.As(null!)!); 

                // C escapes. There is no case 'b', to avoid misparsing
                // the Perl word-boundary \b as the C backspace \b
                // when in POSIX mode. In Perl, /\b/ means word-boundary
                // but /[\b]/ means backspace. We don't support that.
                // If you want a backspace, embed a literal backspace
                // character or use \x08.
                goto __switch_break0;
            }
            if (c == 'a')
            {
                return ('\a', t, error.As(err)!);
                goto __switch_break0;
            }
            if (c == 'f')
            {
                return ('\f', t, error.As(err)!);
                goto __switch_break0;
            }
            if (c == 'n')
            {
                return ('\n', t, error.As(err)!);
                goto __switch_break0;
            }
            if (c == 'r')
            {
                return ('\r', t, error.As(err)!);
                goto __switch_break0;
            }
            if (c == 't')
            {
                return ('\t', t, error.As(err)!);
                goto __switch_break0;
            }
            if (c == 'v')
            {
                return ('\v', t, error.As(err)!);
                goto __switch_break0;
            }
            // default: 
                if (c < utf8.RuneSelf && !isalnum(c))
                { 
                    // Escaped non-word characters are always themselves.
                    // PCRE is not quite so rigorous: it accepts things like
                    // \q, but we don't. We once rejected \_, but too many
                    // programs and people insist on using it, so allow \_.
                    return (c, t, error.As(null!)!);

                } 

                // Octal escapes.

            __switch_break0:;
            return (0L, "", error.As(addr(new Error(ErrInvalidEscape,s[:len(s)-len(t)]))!)!);

        }

        // parseClassChar parses a character class character at the beginning of s
        // and returns it.
        private static (int, @string, error) parseClassChar(this ptr<parser> _addr_p, @string s, @string wholeClass)
        {
            int r = default;
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            if (s == "")
            {
                return (0L, "", error.As(addr(new Error(Code:ErrMissingBracket,Expr:wholeClass))!)!);
            } 

            // Allow regular escape sequences even though
            // many need not be escaped in this context.
            if (s[0L] == '\\')
            {
                return p.parseEscape(s);
            }

            return nextRune(s);

        }

        private partial struct charGroup
        {
            public long sign;
            public slice<int> @class;
        }

        // parsePerlClassEscape parses a leading Perl character class escape like \d
        // from the beginning of s. If one is present, it appends the characters to r
        // and returns the new slice r and the remainder of the string.
        private static (slice<int>, @string) parsePerlClassEscape(this ptr<parser> _addr_p, @string s, slice<int> r)
        {
            slice<int> @out = default;
            @string rest = default;
            ref parser p = ref _addr_p.val;

            if (p.flags & PerlX == 0L || len(s) < 2L || s[0L] != '\\')
            {
                return ;
            }

            var g = perlGroup[s[0L..2L]];
            if (g.sign == 0L)
            {
                return ;
            }

            return (p.appendGroup(r, g), s[2L..]);

        }

        // parseNamedClass parses a leading POSIX named character class like [:alnum:]
        // from the beginning of s. If one is present, it appends the characters to r
        // and returns the new slice r and the remainder of the string.
        private static (slice<int>, @string, error) parseNamedClass(this ptr<parser> _addr_p, @string s, slice<int> r)
        {
            slice<int> @out = default;
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            if (len(s) < 2L || s[0L] != '[' || s[1L] != ':')
            {
                return ;
            }

            var i = strings.Index(s[2L..], ":]");
            if (i < 0L)
            {
                return ;
            }

            i += 2L;
            var name = s[0L..i + 2L];
            var s = s[i + 2L..];
            var g = posixGroup[name];
            if (g.sign == 0L)
            {
                return (null, "", error.As(addr(new Error(ErrInvalidCharRange,name))!)!);
            }

            return (p.appendGroup(r, g), s, error.As(null!)!);

        }

        private static slice<int> appendGroup(this ptr<parser> _addr_p, slice<int> r, charGroup g)
        {
            ref parser p = ref _addr_p.val;

            if (p.flags & FoldCase == 0L)
            {
                if (g.sign < 0L)
                {
                    r = appendNegatedClass(r, g.@class);
                }
                else
                {
                    r = appendClass(r, g.@class);
                }

            }
            else
            {
                var tmp = p.tmpClass[..0L];
                tmp = appendFoldedClass(tmp, g.@class);
                p.tmpClass = tmp;
                tmp = cleanClass(_addr_p.tmpClass);
                if (g.sign < 0L)
                {
                    r = appendNegatedClass(r, tmp);
                }
                else
                {
                    r = appendClass(r, tmp);
                }

            }

            return r;

        }

        private static ptr<unicode.RangeTable> anyTable = addr(new unicode.RangeTable(R16:[]unicode.Range16{{Lo:0,Hi:1<<16-1,Stride:1}},R32:[]unicode.Range32{{Lo:1<<16,Hi:unicode.MaxRune,Stride:1}},));

        // unicodeTable returns the unicode.RangeTable identified by name
        // and the table of additional fold-equivalent code points.
        private static (ptr<unicode.RangeTable>, ptr<unicode.RangeTable>) unicodeTable(@string name)
        {
            ptr<unicode.RangeTable> _p0 = default!;
            ptr<unicode.RangeTable> _p0 = default!;
 
            // Special case: "Any" means any.
            if (name == "Any")
            {
                return (_addr_anyTable!, _addr_anyTable!);
            }

            {
                var t__prev1 = t;

                var t = unicode.Categories[name];

                if (t != null)
                {
                    return (_addr_t!, _addr_unicode.FoldCategory[name]!);
                }

                t = t__prev1;

            }

            {
                var t__prev1 = t;

                t = unicode.Scripts[name];

                if (t != null)
                {
                    return (_addr_t!, _addr_unicode.FoldScript[name]!);
                }

                t = t__prev1;

            }

            return (_addr_null!, _addr_null!);

        }

        // parseUnicodeClass parses a leading Unicode character class like \p{Han}
        // from the beginning of s. If one is present, it appends the characters to r
        // and returns the new slice r and the remainder of the string.
        private static (slice<int>, @string, error) parseUnicodeClass(this ptr<parser> _addr_p, @string s, slice<int> r)
        {
            slice<int> @out = default;
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            if (p.flags & UnicodeGroups == 0L || len(s) < 2L || s[0L] != '\\' || s[1L] != 'p' && s[1L] != 'P')
            {
                return ;
            } 

            // Committed to parse or return error.
            long sign = +1L;
            if (s[1L] == 'P')
            {
                sign = -1L;
            }

            var t = s[2L..];
            var (c, t, err) = nextRune(t);
            if (err != null)
            {
                return ;
            }

            @string seq = default;            @string name = default;

            if (c != '{')
            { 
                // Single-letter name.
                seq = s[..len(s) - len(t)];
                name = seq[2L..];

            }
            else
            { 
                // Name is in braces.
                var end = strings.IndexRune(s, '}');
                if (end < 0L)
                {
                    err = checkUTF8(s);

                    if (err != null)
                    {
                        return ;
                    }

                    return (null, "", error.As(addr(new Error(ErrInvalidCharRange,s))!)!);

                }

                seq = s[..end + 1L];
                t = s[end + 1L..];
                name = s[3L..end];
                err = checkUTF8(name);

                if (err != null)
                {
                    return ;
                }

            } 

            // Group can have leading negation too.  \p{^Han} == \P{Han}, \P{^Han} == \p{Han}.
            if (name != "" && name[0L] == '^')
            {
                sign = -sign;
                name = name[1L..];
            }

            var (tab, fold) = unicodeTable(name);
            if (tab == null)
            {
                return (null, "", error.As(addr(new Error(ErrInvalidCharRange,seq))!)!);
            }

            if (p.flags & FoldCase == 0L || fold == null)
            {
                if (sign > 0L)
                {
                    r = appendTable(r, _addr_tab);
                }
                else
                {
                    r = appendNegatedTable(r, _addr_tab);
                }

            }
            else
            { 
                // Merge and clean tab and fold in a temporary buffer.
                // This is necessary for the negative case and just tidy
                // for the positive case.
                var tmp = p.tmpClass[..0L];
                tmp = appendTable(tmp, _addr_tab);
                tmp = appendTable(tmp, _addr_fold);
                p.tmpClass = tmp;
                tmp = cleanClass(_addr_p.tmpClass);
                if (sign > 0L)
                {
                    r = appendClass(r, tmp);
                }
                else
                {
                    r = appendNegatedClass(r, tmp);
                }

            }

            return (r, t, error.As(null!)!);

        }

        // parseClass parses a character class at the beginning of s
        // and pushes it onto the parse stack.
        private static (@string, error) parseClass(this ptr<parser> _addr_p, @string s)
        {
            @string rest = default;
            error err = default!;
            ref parser p = ref _addr_p.val;

            var t = s[1L..]; // chop [
            var re = p.newRegexp(OpCharClass);
            re.Flags = p.flags;
            re.Rune = re.Rune0[..0L];

            long sign = +1L;
            if (t != "" && t[0L] == '^')
            {
                sign = -1L;
                t = t[1L..]; 

                // If character class does not match \n, add it here,
                // so that negation later will do the right thing.
                if (p.flags & ClassNL == 0L)
                {
                    re.Rune = append(re.Rune, '\n', '\n');
                }

            }

            var @class = re.Rune;
            var first = true; // ] and - are okay as first char in class
            while (t == "" || t[0L] != ']' || first)
            { 
                // POSIX: - is only okay unescaped as first or last in class.
                // Perl: - is okay anywhere.
                if (t != "" && t[0L] == '-' && p.flags & PerlX == 0L && !first && (len(t) == 1L || t[1L] != ']'))
                {
                    var (_, size) = utf8.DecodeRuneInString(t[1L..]);
                    return ("", error.As(addr(new Error(Code:ErrInvalidCharRange,Expr:t[:1+size]))!)!);
                }

                first = false; 

                // Look for POSIX [:alnum:] etc.
                if (len(t) > 2L && t[0L] == '[' && t[1L] == ':')
                {
                    var (nclass, nt, err) = p.parseNamedClass(t, class);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    if (nclass != null)
                    {
                        class = nclass;
                        t = nt;
                        continue;

                    }

                } 

                // Look for Unicode character group like \p{Han}.
                (nclass, nt, err) = p.parseUnicodeClass(t, class);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                if (nclass != null)
                {
                    class = nclass;
                    t = nt;
                    continue;

                } 

                // Look for Perl character class symbols (extension).
                {
                    var nclass__prev1 = nclass;

                    var (nclass, nt) = p.parsePerlClassEscape(t, class);

                    if (nclass != null)
                    {
                        class = nclass;
                        t = nt;
                        continue;

                    } 

                    // Single character or simple range.

                    nclass = nclass__prev1;

                } 

                // Single character or simple range.
                var rng = t;
                int lo = default;                int hi = default;

                lo, t, err = p.parseClassChar(t, s);

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                hi = lo; 
                // [a-] means (a|-) so check for final ].
                if (len(t) >= 2L && t[0L] == '-' && t[1L] != ']')
                {
                    t = t[1L..];
                    hi, t, err = p.parseClassChar(t, s);

                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    if (hi < lo)
                    {
                        rng = rng[..len(rng) - len(t)];
                        return ("", error.As(addr(new Error(Code:ErrInvalidCharRange,Expr:rng))!)!);
                    }

                }

                if (p.flags & FoldCase == 0L)
                {
                    class = appendRange(class, lo, hi);
                }
                else
                {
                    class = appendFoldedRange(class, lo, hi);
                }

            }

            t = t[1L..]; // chop ]

            // Use &re.Rune instead of &class to avoid allocation.
            re.Rune = class;
            class = cleanClass(_addr_re.Rune);
            if (sign < 0L)
            {
                class = negateClass(class);
            }

            re.Rune = class;
            p.push(re);
            return (t, error.As(null!)!);

        }

        // cleanClass sorts the ranges (pairs of elements of r),
        // merges them, and eliminates duplicates.
        private static slice<int> cleanClass(ptr<slice<int>> _addr_rp)
        {
            ref slice<int> rp = ref _addr_rp.val;

            // Sort by lo increasing, hi decreasing to break ties.
            sort.Sort(new ranges(rp));

            slice<int> r = rp;
            if (len(r) < 2L)
            {
                return r;
            } 

            // Merge abutting, overlapping.
            long w = 2L; // write index
            {
                long i = 2L;

                while (i < len(r))
                {
                    var lo = r[i];
                    var hi = r[i + 1L];
                    if (lo <= r[w - 1L] + 1L)
                    { 
                        // merge with previous range
                        if (hi > r[w - 1L])
                        {
                            r[w - 1L] = hi;
                    i += 2L;
                        }

                        continue;

                    } 
                    // new disjoint range
                    r[w] = lo;
                    r[w + 1L] = hi;
                    w += 2L;

                }

            }

            return r[..w];

        }

        // appendLiteral returns the result of appending the literal x to the class r.
        private static slice<int> appendLiteral(slice<int> r, int x, Flags flags)
        {
            if (flags & FoldCase != 0L)
            {
                return appendFoldedRange(r, x, x);
            }

            return appendRange(r, x, x);

        }

        // appendRange returns the result of appending the range lo-hi to the class r.
        private static slice<int> appendRange(slice<int> r, int lo, int hi)
        { 
            // Expand last range or next to last range if it overlaps or abuts.
            // Checking two ranges helps when appending case-folded
            // alphabets, so that one range can be expanding A-Z and the
            // other expanding a-z.
            var n = len(r);
            {
                long i = 2L;

                while (i <= 4L)
                { // twice, using i=2, i=4
                    if (n >= i)
                    {
                        var rlo = r[n - i];
                        var rhi = r[n - i + 1L];
                        if (lo <= rhi + 1L && rlo <= hi + 1L)
                        {
                            if (lo < rlo)
                            {
                                r[n - i] = lo;
                    i += 2L;
                            }

                            if (hi > rhi)
                            {
                                r[n - i + 1L] = hi;
                            }

                            return r;

                        }

                    }

                }

            }

            return append(r, lo, hi);

        }

 
        // minimum and maximum runes involved in folding.
        // checked during test.
        private static readonly ulong minFold = (ulong)0x0041UL;
        private static readonly ulong maxFold = (ulong)0x1e943UL;


        // appendFoldedRange returns the result of appending the range lo-hi
        // and its case folding-equivalent runes to the class r.
        private static slice<int> appendFoldedRange(slice<int> r, int lo, int hi)
        { 
            // Optimizations.
            if (lo <= minFold && hi >= maxFold)
            { 
                // Range is full: folding can't add more.
                return appendRange(r, lo, hi);

            }

            if (hi < minFold || lo > maxFold)
            { 
                // Range is outside folding possibilities.
                return appendRange(r, lo, hi);

            }

            if (lo < minFold)
            { 
                // [lo, minFold-1] needs no folding.
                r = appendRange(r, lo, minFold - 1L);
                lo = minFold;

            }

            if (hi > maxFold)
            { 
                // [maxFold+1, hi] needs no folding.
                r = appendRange(r, maxFold + 1L, hi);
                hi = maxFold;

            } 

            // Brute force. Depend on appendRange to coalesce ranges on the fly.
            for (var c = lo; c <= hi; c++)
            {
                r = appendRange(r, c, c);
                var f = unicode.SimpleFold(c);
                while (f != c)
                {
                    r = appendRange(r, f, f);
                    f = unicode.SimpleFold(f);
                }


            }

            return r;

        }

        // appendClass returns the result of appending the class x to the class r.
        // It assume x is clean.
        private static slice<int> appendClass(slice<int> r, slice<int> x)
        {
            {
                long i = 0L;

                while (i < len(x))
                {
                    r = appendRange(r, x[i], x[i + 1L]);
                    i += 2L;
                }

            }
            return r;

        }

        // appendFolded returns the result of appending the case folding of the class x to the class r.
        private static slice<int> appendFoldedClass(slice<int> r, slice<int> x)
        {
            {
                long i = 0L;

                while (i < len(x))
                {
                    r = appendFoldedRange(r, x[i], x[i + 1L]);
                    i += 2L;
                }

            }
            return r;

        }

        // appendNegatedClass returns the result of appending the negation of the class x to the class r.
        // It assumes x is clean.
        private static slice<int> appendNegatedClass(slice<int> r, slice<int> x)
        {
            char nextLo = '\u0000';
            {
                long i = 0L;

                while (i < len(x))
                {
                    var lo = x[i];
                    var hi = x[i + 1L];
                    if (nextLo <= lo - 1L)
                    {
                        r = appendRange(r, nextLo, lo - 1L);
                    i += 2L;
                    }

                    nextLo = hi + 1L;

                }

            }
            if (nextLo <= unicode.MaxRune)
            {
                r = appendRange(r, nextLo, unicode.MaxRune);
            }

            return r;

        }

        // appendTable returns the result of appending x to the class r.
        private static slice<int> appendTable(slice<int> r, ptr<unicode.RangeTable> _addr_x)
        {
            ref unicode.RangeTable x = ref _addr_x.val;

            {
                var xr__prev1 = xr;

                foreach (var (_, __xr) in x.R16)
                {
                    xr = __xr;
                    var lo = rune(xr.Lo);
                    var hi = rune(xr.Hi);
                    var stride = rune(xr.Stride);
                    if (stride == 1L)
                    {
                        r = appendRange(r, lo, hi);
                        continue;
                    }

                    {
                        var c__prev2 = c;

                        var c = lo;

                        while (c <= hi)
                        {
                            r = appendRange(r, c, c);
                            c += stride;
                        }


                        c = c__prev2;
                    }

                }

                xr = xr__prev1;
            }

            {
                var xr__prev1 = xr;

                foreach (var (_, __xr) in x.R32)
                {
                    xr = __xr;
                    lo = rune(xr.Lo);
                    hi = rune(xr.Hi);
                    stride = rune(xr.Stride);
                    if (stride == 1L)
                    {
                        r = appendRange(r, lo, hi);
                        continue;
                    }

                    {
                        var c__prev2 = c;

                        c = lo;

                        while (c <= hi)
                        {
                            r = appendRange(r, c, c);
                            c += stride;
                        }


                        c = c__prev2;
                    }

                }

                xr = xr__prev1;
            }

            return r;

        }

        // appendNegatedTable returns the result of appending the negation of x to the class r.
        private static slice<int> appendNegatedTable(slice<int> r, ptr<unicode.RangeTable> _addr_x)
        {
            ref unicode.RangeTable x = ref _addr_x.val;

            char nextLo = '\u0000'; // lo end of next class to add
            {
                var xr__prev1 = xr;

                foreach (var (_, __xr) in x.R16)
                {
                    xr = __xr;
                    var lo = rune(xr.Lo);
                    var hi = rune(xr.Hi);
                    var stride = rune(xr.Stride);
                    if (stride == 1L)
                    {
                        if (nextLo <= lo - 1L)
                        {
                            r = appendRange(r, nextLo, lo - 1L);
                        }

                        nextLo = hi + 1L;
                        continue;

                    }

                    {
                        var c__prev2 = c;

                        var c = lo;

                        while (c <= hi)
                        {
                            if (nextLo <= c - 1L)
                            {
                                r = appendRange(r, nextLo, c - 1L);
                            c += stride;
                            }

                            nextLo = c + 1L;

                        }


                        c = c__prev2;
                    }

                }

                xr = xr__prev1;
            }

            {
                var xr__prev1 = xr;

                foreach (var (_, __xr) in x.R32)
                {
                    xr = __xr;
                    lo = rune(xr.Lo);
                    hi = rune(xr.Hi);
                    stride = rune(xr.Stride);
                    if (stride == 1L)
                    {
                        if (nextLo <= lo - 1L)
                        {
                            r = appendRange(r, nextLo, lo - 1L);
                        }

                        nextLo = hi + 1L;
                        continue;

                    }

                    {
                        var c__prev2 = c;

                        c = lo;

                        while (c <= hi)
                        {
                            if (nextLo <= c - 1L)
                            {
                                r = appendRange(r, nextLo, c - 1L);
                            c += stride;
                            }

                            nextLo = c + 1L;

                        }


                        c = c__prev2;
                    }

                }

                xr = xr__prev1;
            }

            if (nextLo <= unicode.MaxRune)
            {
                r = appendRange(r, nextLo, unicode.MaxRune);
            }

            return r;

        }

        // negateClass overwrites r and returns r's negation.
        // It assumes the class r is already clean.
        private static slice<int> negateClass(slice<int> r)
        {
            char nextLo = '\u0000'; // lo end of next class to add
            long w = 0L; // write index
            {
                long i = 0L;

                while (i < len(r))
                {
                    var lo = r[i];
                    var hi = r[i + 1L];
                    if (nextLo <= lo - 1L)
                    {
                        r[w] = nextLo;
                        r[w + 1L] = lo - 1L;
                        w += 2L;
                    i += 2L;
                    }

                    nextLo = hi + 1L;

                }

            }
            r = r[..w];
            if (nextLo <= unicode.MaxRune)
            { 
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
        private partial struct ranges
        {
            public ptr<slice<int>> p;
        }

        private static bool Less(this ranges ra, long i, long j)
        {
            var p = ra.p.val;
            i *= 2L;
            j *= 2L;
            return p[i] < p[j] || p[i] == p[j] && p[i + 1L] > p[j + 1L];
        }

        private static long Len(this ranges ra)
        {
            return len(ra.p.val) / 2L;
        }

        private static void Swap(this ranges ra, long i, long j)
        {
            var p = ra.p.val;
            i *= 2L;
            j *= 2L;
            p[i] = p[j];
            p[i + 1L] = p[j + 1L];
            p[j] = p[i];
            p[j + 1L] = p[i + 1L];

        }

        private static error checkUTF8(@string s)
        {
            while (s != "")
            {
                var (rune, size) = utf8.DecodeRuneInString(s);
                if (rune == utf8.RuneError && size == 1L)
                {
                    return error.As(addr(new Error(Code:ErrInvalidUTF8,Expr:s))!)!;
                }

                s = s[size..];

            }

            return error.As(null!)!;

        }

        private static (int, @string, error) nextRune(@string s)
        {
            int c = default;
            @string t = default;
            error err = default!;

            var (c, size) = utf8.DecodeRuneInString(s);
            if (c == utf8.RuneError && size == 1L)
            {
                return (0L, "", error.As(addr(new Error(Code:ErrInvalidUTF8,Expr:s))!)!);
            }

            return (c, s[size..], error.As(null!)!);

        }

        private static bool isalnum(int c)
        {
            return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z';
        }

        private static int unhex(int c)
        {
            if ('0' <= c && c <= '9')
            {
                return c - '0';
            }

            if ('a' <= c && c <= 'f')
            {
                return c - 'a' + 10L;
            }

            if ('A' <= c && c <= 'F')
            {
                return c - 'A' + 10L;
            }

            return -1L;

        }
    }
}}
