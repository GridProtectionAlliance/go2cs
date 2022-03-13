// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 13 05:38:05 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Program Files\Go\src\regexp\syntax\simplify.go
namespace go.regexp;

public static partial class syntax_package {

// Simplify returns a regexp equivalent to re but without counted repetitions
// and with various other simplifications, such as rewriting /(?:a+)+/ to /a+/.
// The resulting regexp will execute correctly but its string representation
// will not produce the same parse tree, because capturing parentheses
// may have been duplicated or removed. For example, the simplified form
// for /(x){1,2}/ is /(x)(x)?/ but both parentheses capture as $1.
// The returned regexp may share structure with or be the original.
private static ptr<Regexp> Simplify(this ptr<Regexp> _addr_re) {
    ref Regexp re = ref _addr_re.val;

    if (re == null) {
        return _addr_null!;
    }

    if (re.Op == OpCapture || re.Op == OpConcat || re.Op == OpAlternate) 
        // Simplify children, building new Regexp if children change.
        var nre = re;
        {
            var i__prev1 = i;
            var sub__prev1 = sub;

            foreach (var (__i, __sub) in re.Sub) {
                i = __i;
                sub = __sub;
                var nsub = sub.Simplify();
                if (nre == re && nsub != sub) { 
                    // Start a copy.
                    nre = @new<Regexp>();
                    nre.val = re.val;
                    nre.Rune = null;
                    nre.Sub = append(nre.Sub0[..(int)0], re.Sub[..(int)i]);
                }
                if (nre != re) {
                    nre.Sub = append(nre.Sub, nsub);
                }
            }
            i = i__prev1;
            sub = sub__prev1;
        }

        return _addr_nre!;
    else if (re.Op == OpStar || re.Op == OpPlus || re.Op == OpQuest) 
        var sub = re.Sub[0].Simplify();
        return _addr_simplify1(re.Op, re.Flags, _addr_sub, _addr_re)!;
    else if (re.Op == OpRepeat) 
        // Special special case: x{0} matches the empty string
        // and doesn't even need to consider x.
        if (re.Min == 0 && re.Max == 0) {
            return addr(new Regexp(Op:OpEmptyMatch));
        }
        sub = re.Sub[0].Simplify(); 

        // x{n,} means at least n matches of x.
        if (re.Max == -1) { 
            // Special case: x{0,} is x*.
            if (re.Min == 0) {
                return _addr_simplify1(OpStar, re.Flags, _addr_sub, _addr_null)!;
            }
            if (re.Min == 1) {
                return _addr_simplify1(OpPlus, re.Flags, _addr_sub, _addr_null)!;
            }
            nre = addr(new Regexp(Op:OpConcat));
            nre.Sub = nre.Sub0[..(int)0];
            {
                var i__prev1 = i;

                for (nint i = 0; i < re.Min - 1; i++) {
                    nre.Sub = append(nre.Sub, sub);
                }

                i = i__prev1;
            }
            nre.Sub = append(nre.Sub, simplify1(OpPlus, re.Flags, _addr_sub, _addr_null));
            return _addr_nre!;
        }
        if (re.Min == 1 && re.Max == 1) {
            return _addr_sub!;
        }
        ptr<Regexp> prefix;
        if (re.Min > 0) {
            prefix = addr(new Regexp(Op:OpConcat));
            prefix.Sub = prefix.Sub0[..(int)0];
            {
                var i__prev1 = i;

                for (i = 0; i < re.Min; i++) {
                    prefix.Sub = append(prefix.Sub, sub);
                }

                i = i__prev1;
            }
        }
        if (re.Max > re.Min) {
            var suffix = simplify1(OpQuest, re.Flags, _addr_sub, _addr_null);
            {
                var i__prev1 = i;

                for (i = re.Min + 1; i < re.Max; i++) {
                    ptr<Regexp> nre2 = addr(new Regexp(Op:OpConcat));
                    nre2.Sub = append(nre2.Sub0[..(int)0], sub, suffix);
                    suffix = simplify1(OpQuest, re.Flags, nre2, _addr_null);
                }

                i = i__prev1;
            }
            if (prefix == null) {
                return _addr_suffix!;
            }
            prefix.Sub = append(prefix.Sub, suffix);
        }
        if (prefix != null) {
            return _addr_prefix!;
        }
        return addr(new Regexp(Op:OpNoMatch));
        return _addr_re!;
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
private static ptr<Regexp> simplify1(Op op, Flags flags, ptr<Regexp> _addr_sub, ptr<Regexp> _addr_re) {
    ref Regexp sub = ref _addr_sub.val;
    ref Regexp re = ref _addr_re.val;
 
    // Special case: repeat the empty string as much as
    // you want, but it's still the empty string.
    if (sub.Op == OpEmptyMatch) {
        return _addr_sub!;
    }
    if (op == sub.Op && flags & NonGreedy == sub.Flags & NonGreedy) {
        return _addr_sub!;
    }
    if (re != null && re.Op == op && re.Flags & NonGreedy == flags & NonGreedy && sub == re.Sub[0]) {
        return _addr_re!;
    }
    re = addr(new Regexp(Op:op,Flags:flags));
    re.Sub = append(re.Sub0[..(int)0], sub);
    return _addr_re!;
}

} // end syntax_package
