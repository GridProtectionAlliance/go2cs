// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 08:23:47 UTC
// import "regexp/syntax" ==> using syntax = go.regexp.syntax_package
// Original source: C:\Go\src\regexp\syntax\compile.go
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace regexp
{
    public static partial class syntax_package
    {
        // A patchList is a list of instruction pointers that need to be filled in (patched).
        // Because the pointers haven't been filled in yet, we can reuse their storage
        // to hold the list. It's kind of sleazy, but works well in practice.
        // See http://swtch.com/~rsc/regexp/regexp1.html for inspiration.
        //
        // These aren't really pointers: they're integers, so we can reinterpret them
        // this way without using package unsafe. A value l denotes
        // p.inst[l>>1].Out (l&1==0) or .Arg (l&1==1).
        // l == 0 denotes the empty list, okay because we start every program
        // with a fail instruction, so we'll never want to point at its output link.
        private partial struct patchList // : uint
        {
        }

        private static patchList next(this patchList l, ref Prog p)
        {
            var i = ref p.Inst[l >> (int)(1L)];
            if (l & 1L == 0L)
            {
                return patchList(i.Out);
            }
            return patchList(i.Arg);
        }

        private static void patch(this patchList l, ref Prog p, uint val)
        {
            while (l != 0L)
            {
                var i = ref p.Inst[l >> (int)(1L)];
                if (l & 1L == 0L)
                {
                    l = patchList(i.Out);
                    i.Out = val;
                }
                else
                {
                    l = patchList(i.Arg);
                    i.Arg = val;
                }
            }

        }

        private static patchList append(this patchList l1, ref Prog p, patchList l2)
        {
            if (l1 == 0L)
            {
                return l2;
            }
            if (l2 == 0L)
            {
                return l1;
            }
            var last = l1;
            while (true)
            {
                var next = last.next(p);
                if (next == 0L)
                {
                    break;
                }
                last = next;
            }


            var i = ref p.Inst[last >> (int)(1L)];
            if (last & 1L == 0L)
            {
                i.Out = uint32(l2);
            }
            else
            {
                i.Arg = uint32(l2);
            }
            return l1;
        }

        // A frag represents a compiled program fragment.
        private partial struct frag
        {
            public uint i; // index of first instruction
            public patchList @out; // where to record end instruction
        }

        private partial struct compiler
        {
            public ptr<Prog> p;
        }

        // Compile compiles the regexp into a program to be executed.
        // The regexp should have been simplified already (returned from re.Simplify).
        public static (ref Prog, error) Compile(ref Regexp re)
        {
            compiler c = default;
            c.init();
            var f = c.compile(re);
            f.@out.patch(c.p, c.inst(InstMatch).i);
            c.p.Start = int(f.i);
            return (c.p, null);
        }

        private static void init(this ref compiler c)
        {
            c.p = @new<Prog>();
            c.p.NumCap = 2L; // implicit ( and ) for whole match $0
            c.inst(InstFail);
        }

        private static int anyRuneNotNL = new slice<int>(new int[] { 0, '\n'-1, '\n'+1, unicode.MaxRune });
        private static int anyRune = new slice<int>(new int[] { 0, unicode.MaxRune });

        private static frag compile(this ref compiler _c, ref Regexp _re) => func(_c, _re, (ref compiler c, ref Regexp re, Defer _, Panic panic, Recover __) =>
        {

            if (re.Op == OpNoMatch) 
                return c.fail();
            else if (re.Op == OpEmptyMatch) 
                return c.nop();
            else if (re.Op == OpLiteral) 
                if (len(re.Rune) == 0L)
                {
                    return c.nop();
                }
                frag f = default;
                foreach (var (j) in re.Rune)
                {
                    var f1 = c.rune(re.Rune[j..j + 1L], re.Flags);
                    if (j == 0L)
                    {
                        f = f1;
                    }
                    else
                    {
                        f = c.cat(f, f1);
                    }
                }
                return f;
            else if (re.Op == OpCharClass) 
                return c.rune(re.Rune, re.Flags);
            else if (re.Op == OpAnyCharNotNL) 
                return c.rune(anyRuneNotNL, 0L);
            else if (re.Op == OpAnyChar) 
                return c.rune(anyRune, 0L);
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
                var bra = c.cap(uint32(re.Cap << (int)(1L)));
                var sub = c.compile(re.Sub[0L]);
                var ket = c.cap(uint32(re.Cap << (int)(1L) | 1L));
                return c.cat(c.cat(bra, sub), ket);
            else if (re.Op == OpStar) 
                return c.star(c.compile(re.Sub[0L]), re.Flags & NonGreedy != 0L);
            else if (re.Op == OpPlus) 
                return c.plus(c.compile(re.Sub[0L]), re.Flags & NonGreedy != 0L);
            else if (re.Op == OpQuest) 
                return c.quest(c.compile(re.Sub[0L]), re.Flags & NonGreedy != 0L);
            else if (re.Op == OpConcat) 
                if (len(re.Sub) == 0L)
                {
                    return c.nop();
                }
                f = default;
                {
                    var sub__prev1 = sub;

                    foreach (var (__i, __sub) in re.Sub)
                    {
                        i = __i;
                        sub = __sub;
                        if (i == 0L)
                        {
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

                    foreach (var (_, __sub) in re.Sub)
                    {
                        sub = __sub;
                        f = c.alt(f, c.compile(sub));
                    }

                    sub = sub__prev1;
                }

                return f;
                        panic("regexp: unhandled case in compile");
        });

        private static frag inst(this ref compiler c, InstOp op)
        { 
            // TODO: impose length limit
            frag f = new frag(i:uint32(len(c.p.Inst)));
            c.p.Inst = append(c.p.Inst, new Inst(Op:op));
            return f;
        }

        private static frag nop(this ref compiler c)
        {
            var f = c.inst(InstNop);
            f.@out = patchList(f.i << (int)(1L));
            return f;
        }

        private static frag fail(this ref compiler c)
        {
            return new frag();
        }

        private static frag cap(this ref compiler c, uint arg)
        {
            var f = c.inst(InstCapture);
            f.@out = patchList(f.i << (int)(1L));
            c.p.Inst[f.i].Arg = arg;

            if (c.p.NumCap < int(arg) + 1L)
            {
                c.p.NumCap = int(arg) + 1L;
            }
            return f;
        }

        private static frag cat(this ref compiler c, frag f1, frag f2)
        { 
            // concat of failure is failure
            if (f1.i == 0L || f2.i == 0L)
            {
                return new frag();
            } 

            // TODO: elide nop
            f1.@out.patch(c.p, f2.i);
            return new frag(f1.i,f2.out);
        }

        private static frag alt(this ref compiler c, frag f1, frag f2)
        { 
            // alt of failure is other
            if (f1.i == 0L)
            {
                return f2;
            }
            if (f2.i == 0L)
            {
                return f1;
            }
            var f = c.inst(InstAlt);
            var i = ref c.p.Inst[f.i];
            i.Out = f1.i;
            i.Arg = f2.i;
            f.@out = f1.@out.append(c.p, f2.@out);
            return f;
        }

        private static frag quest(this ref compiler c, frag f1, bool nongreedy)
        {
            var f = c.inst(InstAlt);
            var i = ref c.p.Inst[f.i];
            if (nongreedy)
            {
                i.Arg = f1.i;
                f.@out = patchList(f.i << (int)(1L));
            }
            else
            {
                i.Out = f1.i;
                f.@out = patchList(f.i << (int)(1L) | 1L);
            }
            f.@out = f.@out.append(c.p, f1.@out);
            return f;
        }

        private static frag star(this ref compiler c, frag f1, bool nongreedy)
        {
            var f = c.inst(InstAlt);
            var i = ref c.p.Inst[f.i];
            if (nongreedy)
            {
                i.Arg = f1.i;
                f.@out = patchList(f.i << (int)(1L));
            }
            else
            {
                i.Out = f1.i;
                f.@out = patchList(f.i << (int)(1L) | 1L);
            }
            f1.@out.patch(c.p, f.i);
            return f;
        }

        private static frag plus(this ref compiler c, frag f1, bool nongreedy)
        {
            return new frag(f1.i,c.star(f1,nongreedy).out);
        }

        private static frag empty(this ref compiler c, EmptyOp op)
        {
            var f = c.inst(InstEmptyWidth);
            c.p.Inst[f.i].Arg = uint32(op);
            f.@out = patchList(f.i << (int)(1L));
            return f;
        }

        private static frag rune(this ref compiler c, slice<int> r, Flags flags)
        {
            var f = c.inst(InstRune);
            var i = ref c.p.Inst[f.i];
            i.Rune = r;
            flags &= FoldCase; // only relevant flag is FoldCase
            if (len(r) != 1L || unicode.SimpleFold(r[0L]) == r[0L])
            { 
                // and sometimes not even that
                flags &= FoldCase;
            }
            i.Arg = uint32(flags);
            f.@out = patchList(f.i << (int)(1L)); 

            // Special cases for exec machine.

            if (flags & FoldCase == 0L && (len(r) == 1L || len(r) == 2L && r[0L] == r[1L])) 
                i.Op = InstRune1;
            else if (len(r) == 2L && r[0L] == 0L && r[1L] == unicode.MaxRune) 
                i.Op = InstRuneAny;
            else if (len(r) == 4L && r[0L] == 0L && r[1L] == '\n' - 1L && r[2L] == '\n' + 1L && r[3L] == unicode.MaxRune) 
                i.Op = InstRuneAnyNotNL;
                        return f;
        }
    }
}}
