// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package regexp -- go2cs converted at 2020 August 29 08:24:06 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\onepass.go
using bytes = go.bytes_package;
using syntax = go.regexp.syntax_package;
using sort = go.sort_package;
using unicode = go.unicode_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class regexp_package
    {
        // "One-pass" regexp execution.
        // Some regexps can be analyzed to determine that they never need
        // backtracking: they are guaranteed to run in one pass over the string
        // without bothering to save all the usual NFA state.
        // Detect those and execute them more quickly.

        // A onePassProg is a compiled one-pass regular expression program.
        // It is the same as syntax.Prog except for the use of onePassInst.
        private partial struct onePassProg
        {
            public slice<onePassInst> Inst;
            public long Start; // index of start instruction
            public long NumCap; // number of InstCapture insts in re
        }

        // A onePassInst is a single instruction in a one-pass regular expression program.
        // It is the same as syntax.Inst except for the new 'Next' field.
        private partial struct onePassInst
        {
            public ref syntax.Inst Inst => ref Inst_val;
            public slice<uint> Next;
        }

        // OnePassPrefix returns a literal string that all matches for the
        // regexp must start with. Complete is true if the prefix
        // is the entire match. Pc is the index of the last rune instruction
        // in the string. The OnePassPrefix skips over the mandatory
        // EmptyBeginText
        private static (@string, bool, uint) onePassPrefix(ref syntax.Prog p)
        {
            var i = ref p.Inst[p.Start];
            if (i.Op != syntax.InstEmptyWidth || (syntax.EmptyOp(i.Arg)) & syntax.EmptyBeginText == 0L)
            {
                return ("", i.Op == syntax.InstMatch, uint32(p.Start));
            }
            pc = i.Out;
            i = ref p.Inst[pc];
            while (i.Op == syntax.InstNop)
            {
                pc = i.Out;
                i = ref p.Inst[pc];
            } 
            // Avoid allocation of buffer if prefix is empty.
 
            // Avoid allocation of buffer if prefix is empty.
            if (iop(i) != syntax.InstRune || len(i.Rune) != 1L)
            {
                return ("", i.Op == syntax.InstMatch, uint32(p.Start));
            } 

            // Have prefix; gather characters.
            bytes.Buffer buf = default;
            while (iop(i) == syntax.InstRune && len(i.Rune) == 1L && syntax.Flags(i.Arg) & syntax.FoldCase == 0L)
            {
                buf.WriteRune(i.Rune[0L]);
                pc = i.Out;
                i = ref p.Inst[i.Out];
            }

            if (i.Op == syntax.InstEmptyWidth && syntax.EmptyOp(i.Arg) & syntax.EmptyEndText != 0L && p.Inst[i.Out].Op == syntax.InstMatch)
            {
                complete = true;
            }
            return (buf.String(), complete, pc);
        }

        // OnePassNext selects the next actionable state of the prog, based on the input character.
        // It should only be called when i.Op == InstAlt or InstAltMatch, and from the one-pass machine.
        // One of the alternates may ultimately lead without input to end of line. If the instruction
        // is InstAltMatch the path to the InstMatch is in i.Out, the normal node in i.Next.
        private static uint onePassNext(ref onePassInst i, int r)
        {
            var next = i.MatchRunePos(r);
            if (next >= 0L)
            {
                return i.Next[next];
            }
            if (i.Op == syntax.InstAltMatch)
            {
                return i.Out;
            }
            return 0L;
        }

        private static syntax.InstOp iop(ref syntax.Inst i)
        {
            var op = i.Op;

            if (op == syntax.InstRune1 || op == syntax.InstRuneAny || op == syntax.InstRuneAnyNotNL) 
                op = syntax.InstRune;
                        return op;
        }

        // Sparse Array implementation is used as a queueOnePass.
        private partial struct queueOnePass
        {
            public slice<uint> sparse;
            public slice<uint> dense;
            public uint size;
            public uint nextIndex;
        }

        private static bool empty(this ref queueOnePass q)
        {
            return q.nextIndex >= q.size;
        }

        private static uint next(this ref queueOnePass q)
        {
            n = q.dense[q.nextIndex];
            q.nextIndex++;
            return;
        }

        private static void clear(this ref queueOnePass q)
        {
            q.size = 0L;
            q.nextIndex = 0L;
        }

        private static bool contains(this ref queueOnePass q, uint u)
        {
            if (u >= uint32(len(q.sparse)))
            {
                return false;
            }
            return q.sparse[u] < q.size && q.dense[q.sparse[u]] == u;
        }

        private static void insert(this ref queueOnePass q, uint u)
        {
            if (!q.contains(u))
            {
                q.insertNew(u);
            }
        }

        private static void insertNew(this ref queueOnePass q, uint u)
        {
            if (u >= uint32(len(q.sparse)))
            {
                return;
            }
            q.sparse[u] = q.size;
            q.dense[q.size] = u;
            q.size++;
        }

        private static ref queueOnePass newQueue(long size)
        {
            return ref new queueOnePass(sparse:make([]uint32,size),dense:make([]uint32,size),);
        }

        // mergeRuneSets merges two non-intersecting runesets, and returns the merged result,
        // and a NextIp array. The idea is that if a rune matches the OnePassRunes at index
        // i, NextIp[i/2] is the target. If the input sets intersect, an empty runeset and a
        // NextIp array with the single element mergeFailed is returned.
        // The code assumes that both inputs contain ordered and non-intersecting rune pairs.
        private static readonly var mergeFailed = uint32(0xffffffffUL);



        private static int noRune = new slice<int>(new int[] {  });        private static uint noNext = new slice<uint>(new uint[] { mergeFailed });

        private static (slice<int>, slice<uint>) mergeRuneSets(ref slice<int> _leftRunes, ref slice<int> _rightRunes, uint leftPC, uint rightPC) => func(_leftRunes, _rightRunes, (ref slice<int> leftRunes, ref slice<int> rightRunes, Defer defer, Panic panic, Recover _) =>
        {
            var leftLen = len(leftRunes.Value);
            var rightLen = len(rightRunes.Value);
            if (leftLen & 0x1UL != 0L || rightLen & 0x1UL != 0L)
            {
                panic("mergeRuneSets odd length []rune");
            }
            long lx = default;            long rx = default;
            var merged = make_slice<int>(0L);
            var next = make_slice<uint>(0L);
            var ok = true;
            defer(() =>
            {
                if (!ok)
                {
                    merged = null;
                    next = null;
                }
            }());

            long ix = -1L;
            Func<ref long, ref slice<int>, uint, bool> extend = (newLow, newArray, pc) =>
            {
                if (ix > 0L && (newArray.Value)[newLow.Value] <= merged[ix])
                {
                    return false;
                }
                merged = append(merged, (newArray.Value)[newLow.Value], (newArray.Value)[newLow + 1L.Value]);
                newLow.Value += 2L;
                ix += 2L;
                next = append(next, pc);
                return true;
            }
;

            while (lx < leftLen || rx < rightLen)
            {

                if (rx >= rightLen) 
                    ok = extend(ref lx, leftRunes, leftPC);
                else if (lx >= leftLen) 
                    ok = extend(ref rx, rightRunes, rightPC);
                else if ((rightRunes.Value)[rx] < (leftRunes.Value)[lx]) 
                    ok = extend(ref rx, rightRunes, rightPC);
                else 
                    ok = extend(ref lx, leftRunes, leftPC);
                                if (!ok)
                {
                    return (noRune, noNext);
                }
            }

            return (merged, next);
        });

        // cleanupOnePass drops working memory, and restores certain shortcut instructions.
        private static void cleanupOnePass(ref onePassProg prog, ref syntax.Prog original)
        {
            foreach (var (ix, instOriginal) in original.Inst)
            {

                if (instOriginal.Op == syntax.InstAlt || instOriginal.Op == syntax.InstAltMatch || instOriginal.Op == syntax.InstRune)                 else if (instOriginal.Op == syntax.InstCapture || instOriginal.Op == syntax.InstEmptyWidth || instOriginal.Op == syntax.InstNop || instOriginal.Op == syntax.InstMatch || instOriginal.Op == syntax.InstFail) 
                    prog.Inst[ix].Next = null;
                else if (instOriginal.Op == syntax.InstRune1 || instOriginal.Op == syntax.InstRuneAny || instOriginal.Op == syntax.InstRuneAnyNotNL) 
                    prog.Inst[ix].Next = null;
                    prog.Inst[ix] = new onePassInst(Inst:instOriginal);
                            }
        }

        // onePassCopy creates a copy of the original Prog, as we'll be modifying it
        private static ref onePassProg onePassCopy(ref syntax.Prog prog)
        {
            onePassProg p = ref new onePassProg(Start:prog.Start,NumCap:prog.NumCap,Inst:make([]onePassInst,len(prog.Inst)),);
            foreach (var (i, inst) in prog.Inst)
            {
                p.Inst[i] = new onePassInst(Inst:inst);
            } 

            // rewrites one or more common Prog constructs that enable some otherwise
            // non-onepass Progs to be onepass. A:BD (for example) means an InstAlt at
            // ip A, that points to ips B & C.
            // A:BC + B:DA => A:BC + B:CD
            // A:BC + B:DC => A:DC + B:DC
            foreach (var (pc) in p.Inst)
            {

                if (p.Inst[pc].Op == syntax.InstAlt || p.Inst[pc].Op == syntax.InstAltMatch) 
                    // A:Bx + B:Ay
                    var p_A_Other = ref p.Inst[pc].Out;
                    var p_A_Alt = ref p.Inst[pc].Arg; 
                    // make sure a target is another Alt
                    var instAlt = p.Inst[p_A_Alt.Value];
                    if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch))
                    {
                        p_A_Alt = p_A_Other;
                        p_A_Other = p_A_Alt;
                        instAlt = p.Inst[p_A_Alt.Value];
                        if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch))
                        {
                            continue;
                        }
                    }
                    var instOther = p.Inst[p_A_Other.Value]; 
                    // Analyzing both legs pointing to Alts is for another day
                    if (instOther.Op == syntax.InstAlt || instOther.Op == syntax.InstAltMatch)
                    { 
                        // too complicated
                        continue;
                    } 
                    // simple empty transition loop
                    // A:BC + B:DA => A:BC + B:DC
                    var p_B_Alt = ref p.Inst[p_A_Alt.Value].Out;
                    var p_B_Other = ref p.Inst[p_A_Alt.Value].Arg;
                    var patch = false;
                    if (instAlt.Out == uint32(pc))
                    {
                        patch = true;
                    }
                    else if (instAlt.Arg == uint32(pc))
                    {
                        patch = true;
                        p_B_Alt = p_B_Other;
                        p_B_Other = p_B_Alt;
                    }
                    if (patch)
                    {
                        p_B_Alt.Value = p_A_Other.Value;
                    } 

                    // empty transition to common target
                    // A:BC + B:DC => A:DC + B:DC
                    if (p_A_Other == p_B_Alt.Value.Value)
                    {
                        p_A_Alt.Value = p_B_Other.Value;
                    }
                else 
                    continue;
                            }
            return p;
        }

        // runeSlice exists to permit sorting the case-folded rune sets.
        private partial struct runeSlice // : slice<int>
        {
        }

        private static long Len(this runeSlice p)
        {
            return len(p);
        }
        private static bool Less(this runeSlice p, long i, long j)
        {
            return p[i] < p[j];
        }
        private static void Swap(this runeSlice p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];

        }

        private static int anyRuneNotNL = new slice<int>(new int[] { 0, '\n'-1, '\n'+1, unicode.MaxRune });
        private static int anyRune = new slice<int>(new int[] { 0, unicode.MaxRune });

        // makeOnePass creates a onepass Prog, if possible. It is possible if at any alt,
        // the match engine can always tell which branch to take. The routine may modify
        // p if it is turned into a onepass Prog. If it isn't possible for this to be a
        // onepass Prog, the Prog notOnePass is returned. makeOnePass is recursive
        // to the size of the Prog.
        private static ref onePassProg makeOnePass(ref onePassProg p)
        { 
            // If the machine is very long, it's not worth the time to check if we can use one pass.
            if (len(p.Inst) >= 1000L)
            {
                return notOnePass;
            }
            var instQueue = newQueue(len(p.Inst));            var visitQueue = newQueue(len(p.Inst));            Func<uint, slice<bool>, bool> check = default;            var onePassRunes = make_slice<slice<int>>(len(p.Inst)); 

            // check that paths from Alt instructions are unambiguous, and rebuild the new
            // program as a onepass program
            check = (pc, m) =>
            {
                ok = true;
                var inst = ref p.Inst[pc];
                if (visitQueue.contains(pc))
                {
                    return;
                }
                visitQueue.insert(pc);

                if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
                    ok = check(inst.Out, m) && check(inst.Arg, m); 
                    // check no-input paths to InstMatch
                    var matchOut = m[inst.Out];
                    var matchArg = m[inst.Arg];
                    if (matchOut && matchArg)
                    {
                        ok = false;
                        break;
                    } 
                    // Match on empty goes in inst.Out
                    if (matchArg)
                    {
                        inst.Out = inst.Arg;
                        inst.Arg = inst.Out;
                        matchOut = matchArg;
                        matchArg = matchOut;
                    }
                    if (matchOut)
                    {
                        m[pc] = true;
                        inst.Op = syntax.InstAltMatch;
                    } 

                    // build a dispatch operator from the two legs of the alt.
                    onePassRunes[pc], inst.Next = mergeRuneSets(ref onePassRunes[inst.Out], ref onePassRunes[inst.Arg], inst.Out, inst.Arg);
                    if (len(inst.Next) > 0L && inst.Next[0L] == mergeFailed)
                    {
                        ok = false;
                        break;
                    }
                else if (inst.Op == syntax.InstCapture || inst.Op == syntax.InstNop) 
                    ok = check(inst.Out, m);
                    m[pc] = m[inst.Out]; 
                    // pass matching runes back through these no-ops.
                    onePassRunes[pc] = append(new slice<int>(new int[] {  }), onePassRunes[inst.Out]);
                    inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2L + 1L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in inst.Next)
                        {
                            i = __i;
                            inst.Next[i] = inst.Out;
                        }

                        i = i__prev1;
                    }
                else if (inst.Op == syntax.InstEmptyWidth) 
                    ok = check(inst.Out, m);
                    m[pc] = m[inst.Out];
                    onePassRunes[pc] = append(new slice<int>(new int[] {  }), onePassRunes[inst.Out]);
                    inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2L + 1L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in inst.Next)
                        {
                            i = __i;
                            inst.Next[i] = inst.Out;
                        }

                        i = i__prev1;
                    }
                else if (inst.Op == syntax.InstMatch || inst.Op == syntax.InstFail) 
                    m[pc] = inst.Op == syntax.InstMatch;
                else if (inst.Op == syntax.InstRune) 
                    m[pc] = false;
                    if (len(inst.Next) > 0L)
                    {
                        break;
                    }
                    instQueue.insert(inst.Out);
                    if (len(inst.Rune) == 0L)
                    {
                        onePassRunes[pc] = new slice<int>(new int[] {  });
                        inst.Next = new slice<uint>(new uint[] { inst.Out });
                        break;
                    }
                    var runes = make_slice<int>(0L);
                    if (len(inst.Rune) == 1L && syntax.Flags(inst.Arg) & syntax.FoldCase != 0L)
                    {
                        var r0 = inst.Rune[0L];
                        runes = append(runes, r0, r0);
                        {
                            var r1__prev1 = r1;

                            var r1 = unicode.SimpleFold(r0);

                            while (r1 != r0)
                            {
                                runes = append(runes, r1, r1);
                                r1 = unicode.SimpleFold(r1);
                            }
                    else


                            r1 = r1__prev1;
                        }
                        sort.Sort(runeSlice(runes));
                    }                    {
                        runes = append(runes, inst.Rune);
                    }
                    onePassRunes[pc] = runes;
                    inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2L + 1L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in inst.Next)
                        {
                            i = __i;
                            inst.Next[i] = inst.Out;
                        }

                        i = i__prev1;
                    }

                    inst.Op = syntax.InstRune;
                else if (inst.Op == syntax.InstRune1) 
                    m[pc] = false;
                    if (len(inst.Next) > 0L)
                    {
                        break;
                    }
                    instQueue.insert(inst.Out);
                    runes = new slice<int>(new int[] {  }); 
                    // expand case-folded runes
                    if (syntax.Flags(inst.Arg) & syntax.FoldCase != 0L)
                    {
                        r0 = inst.Rune[0L];
                        runes = append(runes, r0, r0);
                        {
                            var r1__prev1 = r1;

                            r1 = unicode.SimpleFold(r0);

                            while (r1 != r0)
                            {
                                runes = append(runes, r1, r1);
                                r1 = unicode.SimpleFold(r1);
                            }
                    else


                            r1 = r1__prev1;
                        }
                        sort.Sort(runeSlice(runes));
                    }                    {
                        runes = append(runes, inst.Rune[0L], inst.Rune[0L]);
                    }
                    onePassRunes[pc] = runes;
                    inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2L + 1L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in inst.Next)
                        {
                            i = __i;
                            inst.Next[i] = inst.Out;
                        }

                        i = i__prev1;
                    }

                    inst.Op = syntax.InstRune;
                else if (inst.Op == syntax.InstRuneAny) 
                    m[pc] = false;
                    if (len(inst.Next) > 0L)
                    {
                        break;
                    }
                    instQueue.insert(inst.Out);
                    onePassRunes[pc] = append(new slice<int>(new int[] {  }), anyRune);
                    inst.Next = new slice<uint>(new uint[] { inst.Out });
                else if (inst.Op == syntax.InstRuneAnyNotNL) 
                    m[pc] = false;
                    if (len(inst.Next) > 0L)
                    {
                        break;
                    }
                    instQueue.insert(inst.Out);
                    onePassRunes[pc] = append(new slice<int>(new int[] {  }), anyRuneNotNL);
                    inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2L + 1L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in inst.Next)
                        {
                            i = __i;
                            inst.Next[i] = inst.Out;
                        }

                        i = i__prev1;
                    }
                                return;
            }
;

            instQueue.clear();
            instQueue.insert(uint32(p.Start));
            var m = make_slice<bool>(len(p.Inst));
            while (!instQueue.empty())
            {
                visitQueue.clear();
                var pc = instQueue.next();
                if (!check(pc, m))
                {
                    p = notOnePass;
                    break;
                }
            }

            if (p != notOnePass)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in p.Inst)
                    {
                        i = __i;
                        p.Inst[i].Rune = onePassRunes[i];
                    }

                    i = i__prev1;
                }

            }
            return p;
        }

        private static ref onePassProg notOnePass = null;

        // compileOnePass returns a new *syntax.Prog suitable for onePass execution if the original Prog
        // can be recharacterized as a one-pass regexp program, or syntax.notOnePass if the
        // Prog cannot be converted. For a one pass prog, the fundamental condition that must
        // be true is: at any InstAlt, there must be no ambiguity about what branch to  take.
        private static ref onePassProg compileOnePass(ref syntax.Prog prog)
        {
            if (prog.Start == 0L)
            {
                return notOnePass;
            } 
            // onepass regexp is anchored
            if (prog.Inst[prog.Start].Op != syntax.InstEmptyWidth || syntax.EmptyOp(prog.Inst[prog.Start].Arg) & syntax.EmptyBeginText != syntax.EmptyBeginText)
            {
                return notOnePass;
            } 
            // every instruction leading to InstMatch must be EmptyEndText
            foreach (var (_, inst) in prog.Inst)
            {
                var opOut = prog.Inst[inst.Out].Op;

                if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
                    if (opOut == syntax.InstMatch || prog.Inst[inst.Arg].Op == syntax.InstMatch)
                    {
                        return notOnePass;
                    }
                else if (inst.Op == syntax.InstEmptyWidth) 
                    if (opOut == syntax.InstMatch)
                    {
                        if (syntax.EmptyOp(inst.Arg) & syntax.EmptyEndText == syntax.EmptyEndText)
                        {
                            continue;
                        }
                        return notOnePass;
                    }
                else 
                    if (opOut == syntax.InstMatch)
                    {
                        return notOnePass;
                    }
                            } 
            // Creates a slightly optimized copy of the original Prog
            // that cleans up some Prog idioms that block valid onepass programs
            p = onePassCopy(prog); 

            // checkAmbiguity on InstAlts, build onepass Prog if possible
            p = makeOnePass(p);

            if (p != notOnePass)
            {
                cleanupOnePass(p, prog);
            }
            return p;
        }
    }
}
