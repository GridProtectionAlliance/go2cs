// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 13 05:37:55 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Program Files\Go\src\regexp\syntax\compile.go
namespace go.regexp;

using unicode = unicode_package;

public static partial class syntax_package {

// A patchList is a list of instruction pointers that need to be filled in (patched).
// Because the pointers haven't been filled in yet, we can reuse their storage
// to hold the list. It's kind of sleazy, but works well in practice.
// See https://swtch.com/~rsc/regexp/regexp1.html for inspiration.
//
// These aren't really pointers: they're integers, so we can reinterpret them
// this way without using package unsafe. A value l.head denotes
// p.inst[l.head>>1].Out (l.head&1==0) or .Arg (l.head&1==1).
// head == 0 denotes the empty list, okay because we start every program
// with a fail instruction, so we'll never want to point at its output link.
private partial struct patchList {
    public uint head;
    public uint tail;
}

private static patchList makePatchList(uint n) {
    return new patchList(n,n);
}

private static void patch(this patchList l, ptr<Prog> _addr_p, uint val) {
    ref Prog p = ref _addr_p.val;

    var head = l.head;
    while (head != 0) {
        var i = _addr_p.Inst[head >> 1];
        if (head & 1 == 0) {
            head = i.Out;
            i.Out = val;
        }
        else
 {
            head = i.Arg;
            i.Arg = val;
        }
    }
}

private static patchList append(this patchList l1, ptr<Prog> _addr_p, patchList l2) {
    ref Prog p = ref _addr_p.val;

    if (l1.head == 0) {
        return l2;
    }
    if (l2.head == 0) {
        return l1;
    }
    var i = _addr_p.Inst[l1.tail >> 1];
    if (l1.tail & 1 == 0) {
        i.Out = l2.head;
    }
    else
 {
        i.Arg = l2.head;
    }
    return new patchList(l1.head,l2.tail);
}

// A frag represents a compiled program fragment.
private partial struct frag {
    public uint i; // index of first instruction
    public patchList @out; // where to record end instruction
    public bool nullable; // whether fragment can match empty string
}

private partial struct compiler {
    public ptr<Prog> p;
}

// Compile compiles the regexp into a program to be executed.
// The regexp should have been simplified already (returned from re.Simplify).
public static (ptr<Prog>, error) Compile(ptr<Regexp> _addr_re) {
    ptr<Prog> _p0 = default!;
    error _p0 = default!;
    ref Regexp re = ref _addr_re.val;

    compiler c = default;
    c.init();
    var f = c.compile(re);
    f.@out.patch(c.p, c.inst(InstMatch).i);
    c.p.Start = int(f.i);
    return (_addr_c.p!, error.As(null!)!);
}

private static void init(this ptr<compiler> _addr_c) {
    ref compiler c = ref _addr_c.val;

    c.p = @new<Prog>();
    c.p.NumCap = 2; // implicit ( and ) for whole match $0
    c.inst(InstFail);
}

private static int anyRuneNotNL = new slice<int>(new int[] { 0, '\n'-1, '\n'+1, unicode.MaxRune });
private static int anyRune = new slice<int>(new int[] { 0, unicode.MaxRune });

private static frag compile(this ptr<compiler> _addr_c, ptr<Regexp> _addr_re) => func((_, panic, _) => {
    ref compiler c = ref _addr_c.val;
    ref Regexp re = ref _addr_re.val;


    if (re.Op == OpNoMatch) 
        return c.fail();
    else if (re.Op == OpEmptyMatch) 
        return c.nop();
    else if (re.Op == OpLiteral) 
        if (len(re.Rune) == 0) {
            return c.nop();
        }
        frag f = default;
        foreach (var (j) in re.Rune) {
            var f1 = c.rune(re.Rune[(int)j..(int)j + 1], re.Flags);
            if (j == 0) {
                f = f1;
            }
            else
 {
                f = c.cat(f, f1);
            }
        }        return f;
    else if (re.Op == OpCharClass) 
        return c.rune(re.Rune, re.Flags);
    else if (re.Op == OpAnyCharNotNL) 
        return c.rune(anyRuneNotNL, 0);
    else if (re.Op == OpAnyChar) 
        return c.rune(anyRune, 0);
    else if (re.Op == OpBeginLine) 
        return c.empty(EmptyBeginLine);
    else if (re.Op == OpEndLine) 
        return c.empty(EmptyEndLine);
    else if (re.Op == OpBeginText) 
        return c.empty(EmptyBeginText);
    else if (re.Op == OpEndText) 
        return c.empty(EmptyEndText);
    else if (re.Op == OpWordBoundary) 
        return c.empty(EmptyWordBoundary);
    else if (re.Op == OpNoWordBoundary) 
        return c.empty(EmptyNoWordBoundary);
    else if (re.Op == OpCapture) 
        var bra = c.cap(uint32(re.Cap << 1));
        var sub = c.compile(re.Sub[0]);
        var ket = c.cap(uint32(re.Cap << 1 | 1));
        return c.cat(c.cat(bra, sub), ket);
    else if (re.Op == OpStar) 
        return c.star(c.compile(re.Sub[0]), re.Flags & NonGreedy != 0);
    else if (re.Op == OpPlus) 
        return c.plus(c.compile(re.Sub[0]), re.Flags & NonGreedy != 0);
    else if (re.Op == OpQuest) 
        return c.quest(c.compile(re.Sub[0]), re.Flags & NonGreedy != 0);
    else if (re.Op == OpConcat) 
        if (len(re.Sub) == 0) {
            return c.nop();
        }
        f = default;
        {
            var sub__prev1 = sub;

            foreach (var (__i, __sub) in re.Sub) {
                i = __i;
                sub = __sub;
                if (i == 0) {
                    f = c.compile(sub);
                }
                else
 {
                    f = c.cat(f, c.compile(sub));
                }
            }

            sub = sub__prev1;
        }

        return f;
    else if (re.Op == OpAlternate) 
        f = default;
        {
            var sub__prev1 = sub;

            foreach (var (_, __sub) in re.Sub) {
                sub = __sub;
                f = c.alt(f, c.compile(sub));
            }

            sub = sub__prev1;
        }

        return f;
        panic("regexp: unhandled case in compile");
});

private static frag inst(this ptr<compiler> _addr_c, InstOp op) {
    ref compiler c = ref _addr_c.val;
 
    // TODO: impose length limit
    frag f = new frag(i:uint32(len(c.p.Inst)),nullable:true);
    c.p.Inst = append(c.p.Inst, new Inst(Op:op));
    return f;
}

private static frag nop(this ptr<compiler> _addr_c) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstNop);
    f.@out = makePatchList(f.i << 1);
    return f;
}

private static frag fail(this ptr<compiler> _addr_c) {
    ref compiler c = ref _addr_c.val;

    return new frag();
}

private static frag cap(this ptr<compiler> _addr_c, uint arg) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstCapture);
    f.@out = makePatchList(f.i << 1);
    c.p.Inst[f.i].Arg = arg;

    if (c.p.NumCap < int(arg) + 1) {
        c.p.NumCap = int(arg) + 1;
    }
    return f;
}

private static frag cat(this ptr<compiler> _addr_c, frag f1, frag f2) {
    ref compiler c = ref _addr_c.val;
 
    // concat of failure is failure
    if (f1.i == 0 || f2.i == 0) {
        return new frag();
    }
    f1.@out.patch(c.p, f2.i);
    return new frag(f1.i,f2.out,f1.nullable&&f2.nullable);
}

private static frag alt(this ptr<compiler> _addr_c, frag f1, frag f2) {
    ref compiler c = ref _addr_c.val;
 
    // alt of failure is other
    if (f1.i == 0) {
        return f2;
    }
    if (f2.i == 0) {
        return f1;
    }
    var f = c.inst(InstAlt);
    var i = _addr_c.p.Inst[f.i];
    i.Out = f1.i;
    i.Arg = f2.i;
    f.@out = f1.@out.append(c.p, f2.@out);
    f.nullable = f1.nullable || f2.nullable;
    return f;
}

private static frag quest(this ptr<compiler> _addr_c, frag f1, bool nongreedy) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstAlt);
    var i = _addr_c.p.Inst[f.i];
    if (nongreedy) {
        i.Arg = f1.i;
        f.@out = makePatchList(f.i << 1);
    }
    else
 {
        i.Out = f1.i;
        f.@out = makePatchList(f.i << 1 | 1);
    }
    f.@out = f.@out.append(c.p, f1.@out);
    return f;
}

// loop returns the fragment for the main loop of a plus or star.
// For plus, it can be used after changing the entry to f1.i.
// For star, it can be used directly when f1 can't match an empty string.
// (When f1 can match an empty string, f1* must be implemented as (f1+)?
// to get the priority match order correct.)
private static frag loop(this ptr<compiler> _addr_c, frag f1, bool nongreedy) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstAlt);
    var i = _addr_c.p.Inst[f.i];
    if (nongreedy) {
        i.Arg = f1.i;
        f.@out = makePatchList(f.i << 1);
    }
    else
 {
        i.Out = f1.i;
        f.@out = makePatchList(f.i << 1 | 1);
    }
    f1.@out.patch(c.p, f.i);
    return f;
}

private static frag star(this ptr<compiler> _addr_c, frag f1, bool nongreedy) {
    ref compiler c = ref _addr_c.val;

    if (f1.nullable) { 
        // Use (f1+)? to get priority match order correct.
        // See golang.org/issue/46123.
        return c.quest(c.plus(f1, nongreedy), nongreedy);
    }
    return c.loop(f1, nongreedy);
}

private static frag plus(this ptr<compiler> _addr_c, frag f1, bool nongreedy) {
    ref compiler c = ref _addr_c.val;

    return new frag(f1.i,c.loop(f1,nongreedy).out,f1.nullable);
}

private static frag empty(this ptr<compiler> _addr_c, EmptyOp op) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstEmptyWidth);
    c.p.Inst[f.i].Arg = uint32(op);
    f.@out = makePatchList(f.i << 1);
    return f;
}

private static frag rune(this ptr<compiler> _addr_c, slice<int> r, Flags flags) {
    ref compiler c = ref _addr_c.val;

    var f = c.inst(InstRune);
    f.nullable = false;
    var i = _addr_c.p.Inst[f.i];
    i.Rune = r;
    flags &= FoldCase; // only relevant flag is FoldCase
    if (len(r) != 1 || unicode.SimpleFold(r[0]) == r[0]) { 
        // and sometimes not even that
        flags &= FoldCase;
    }
    i.Arg = uint32(flags);
    f.@out = makePatchList(f.i << 1); 

    // Special cases for exec machine.

    if (flags & FoldCase == 0 && (len(r) == 1 || len(r) == 2 && r[0] == r[1])) 
        i.Op = InstRune1;
    else if (len(r) == 2 && r[0] == 0 && r[1] == unicode.MaxRune) 
        i.Op = InstRuneAny;
    else if (len(r) == 4 && r[0] == 0 && r[1] == '\n' - 1 && r[2] == '\n' + 1 && r[3] == unicode.MaxRune) 
        i.Op = InstRuneAnyNotNL;
        return f;
}

} // end syntax_package
