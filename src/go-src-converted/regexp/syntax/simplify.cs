// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 08:24:00 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\simplify.go

using static go.builtin;

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        // Simplify returns a regexp equivalent to re but without counted repetitions
        // and with various other simplifications, such as rewriting /(?:a+)+/ to /a+/.
        // The resulting regexp will execute correctly but its string representation
        // will not produce the same parse tree, because capturing parentheses
        // may have been duplicated or removed. For example, the simplified form
        // for /(x){1,2}/ is /(x)(x)?/ but both parentheses capture as $1.
        // The returned regexp may share structure with or be the original.
        private static ref Regexp Simplify(this ref Regexp re)
        {
            if (re == null)
            {
                return null;
            }

            if (re.Op == OpCapture || re.Op == OpConcat || re.Op == OpAlternate) 
                // Simplify children, building new Regexp if children change.
                var nre = re;
                {
                    var i__prev1 = i;
                    var sub__prev1 = sub;

                    foreach (var (__i, __sub) in re.Sub)
                    {
                        i = __i;
                        sub = __sub;
                        var nsub = sub.Simplify();
                        if (nre == re && nsub != sub)
                        { 
                            // Start a copy.
                            nre = @new<Regexp>();
                            nre.Value = re.Value;
                            nre.Rune = null;
                            nre.Sub = append(nre.Sub0[..0L], re.Sub[..i]);
                        }
                        if (nre != re)
                        {
                            nre.Sub = append(nre.Sub, nsub);
                        }
                    }
                    i = i__prev1;
                    sub = sub__prev1;
                }

                return nre;
            else if (re.Op == OpStar || re.Op == OpPlus || re.Op == OpQuest) 
                var sub = re.Sub[0L].Simplify();
                return simplify1(re.Op, re.Flags, sub, re);
            else if (re.Op == OpRepeat) 
                // Special special case: x{0} matches the empty string
                // and doesn't even need to consider x.
                if (re.Min == 0L && re.Max == 0L)
                {
                    return ref new Regexp(Op:OpEmptyMatch);
                }
                sub = re.Sub[0L].Simplify(); 

                // x{n,} means at least n matches of x.
                if (re.Max == -1L)
                { 
                    // Special case: x{0,} is x*.
                    if (re.Min == 0L)
                    {
                        return simplify1(OpStar, re.Flags, sub, null);
                    }
                    if (re.Min == 1L)
                    {
                        return simplify1(OpPlus, re.Flags, sub, null);
                    }
                    nre = ref new Regexp(Op:OpConcat);
                    nre.Sub = nre.Sub0[..0L];
                    {
                        var i__prev1 = i;

                        for (long i = 0L; i < re.Min - 1L; i++)
                        {
                            nre.Sub = append(nre.Sub, sub);
                        }

                        i = i__prev1;
                    }
                    nre.Sub = append(nre.Sub, simplify1(OpPlus, re.Flags, sub, null));
                    return nre;
                }
                if (re.Min == 1L && re.Max == 1L)
                {
                    return sub;
                }
                ref Regexp prefix = default;
                if (re.Min > 0L)
                {
                    prefix = ref new Regexp(Op:OpConcat);
                    prefix.Sub = prefix.Sub0[..0L];
                    {
                        var i__prev1 = i;

                        for (i = 0L; i < re.Min; i++)
                        {
                            prefix.Sub = append(prefix.Sub, sub);
                        }

                        i = i__prev1;
                    }
                }
                if (re.Max > re.Min)
                {
                    var suffix = simplify1(OpQuest, re.Flags, sub, null);
                    {
                        var i__prev1 = i;

                        for (i = re.Min + 1L; i < re.Max; i++)
                        {
                            Regexp nre2 = ref new Regexp(Op:OpConcat);
                            nre2.Sub = append(nre2.Sub0[..0L], sub, suffix);
                            suffix = simplify1(OpQuest, re.Flags, nre2, null);
                        }

                        i = i__prev1;
                    }
                    if (prefix == null)
                    {
                        return suffix;
                    }
                    prefix.Sub = append(prefix.Sub, suffix);
                }
                if (prefix != null)
                {
                    return prefix;
                }
                return ref new Regexp(Op:OpNoMatch);
                        return re;
        }

        // simplify1 implements Simplify for the unary OpStar,
        // OpPlus, and OpQuest operators. It returns the simple regexp
        // equivalent to
        //
        //    Regexp{Op: op, Flags: flags, Sub: {sub}}
        //
        // under the assumption that sub is already simple, and
        // without first allocating that structure. If the regexp
        // to be returned turns out to be equivalent to re, simplify1
        // returns re instead.
        //
        // simplify1 is factored out of Simplify because the implementation
        // for other operators generates these unary expressions.
        // Letting them call simplify1 makes sure the expressions they
        // generate are simple.
        private static ref Regexp simplify1(Op op, Flags flags, ref Regexp sub, ref Regexp re)
        { 
            // Special case: repeat the empty string as much as
            // you want, but it's still the empty string.
            if (sub.Op == OpEmptyMatch)
            {
                return sub;
            } 
            // The operators are idempotent if the flags match.
            if (op == sub.Op && flags & NonGreedy == sub.Flags & NonGreedy)
            {
                return sub;
            }
            if (re != null && re.Op == op && re.Flags & NonGreedy == flags & NonGreedy && sub == re.Sub[0L])
            {
                return re;
            }
            re = ref new Regexp(Op:op,Flags:flags);
            re.Sub = append(re.Sub0[..0L], sub);
            return re;
        }
    }
}}
