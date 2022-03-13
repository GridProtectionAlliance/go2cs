// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package regexp -- go2cs converted at 2022 March 13 05:38:09 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Program Files\Go\src\regexp\onepass.go
namespace go;

using syntax = regexp.syntax_package;
using sort = sort_package;
using strings = strings_package;
using unicode = unicode_package;


// "One-pass" regexp execution.
// Some regexps can be analyzed to determine that they never need
// backtracking: they are guaranteed to run in one pass over the string
// without bothering to save all the usual NFA state.
// Detect those and execute them more quickly.

// A onePassProg is a compiled one-pass regular expression program.
// It is the same as syntax.Prog except for the use of onePassInst.

using System;
public static partial class regexp_package {

private partial struct onePassProg {
    public slice<onePassInst> Inst;
    public nint Start; // index of start instruction
    public nint NumCap; // number of InstCapture insts in re
}

// A onePassInst is a single instruction in a one-pass regular expression program.
// It is the same as syntax.Inst except for the new 'Next' field.
private partial struct onePassInst {
    public ref syntax.Inst Inst => ref Inst_val;
    public slice<uint> Next;
}

// OnePassPrefix returns a literal string that all matches for the
// regexp must start with. Complete is true if the prefix
// is the entire match. Pc is the index of the last rune instruction
// in the string. The OnePassPrefix skips over the mandatory
// EmptyBeginText
private static (@string, bool, uint) onePassPrefix(ptr<syntax.Prog> _addr_p) {
    @string prefix = default;
    bool complete = default;
    uint pc = default;
    ref syntax.Prog p = ref _addr_p.val;

    var i = _addr_p.Inst[p.Start];
    if (i.Op != syntax.InstEmptyWidth || (syntax.EmptyOp(i.Arg)) & syntax.EmptyBeginText == 0) {
        return ("", i.Op == syntax.InstMatch, uint32(p.Start));
    }
    pc = i.Out;
    i = _addr_p.Inst[pc];
    while (i.Op == syntax.InstNop) {
        pc = i.Out;
        i = _addr_p.Inst[pc];
    } 
    // Avoid allocation of buffer if prefix is empty.
    if (iop(_addr_i) != syntax.InstRune || len(i.Rune) != 1) {
        return ("", i.Op == syntax.InstMatch, uint32(p.Start));
    }
    strings.Builder buf = default;
    while (iop(_addr_i) == syntax.InstRune && len(i.Rune) == 1 && syntax.Flags(i.Arg) & syntax.FoldCase == 0) {
        buf.WriteRune(i.Rune[0]);
        (pc, i) = (i.Out, _addr_p.Inst[i.Out]);
    }
    if (i.Op == syntax.InstEmptyWidth && syntax.EmptyOp(i.Arg) & syntax.EmptyEndText != 0 && p.Inst[i.Out].Op == syntax.InstMatch) {
        complete = true;
    }
    return (buf.String(), complete, pc);
}

// OnePassNext selects the next actionable state of the prog, based on the input character.
// It should only be called when i.Op == InstAlt or InstAltMatch, and from the one-pass machine.
// One of the alternates may ultimately lead without input to end of line. If the instruction
// is InstAltMatch the path to the InstMatch is in i.Out, the normal node in i.Next.
private static uint onePassNext(ptr<onePassInst> _addr_i, int r) {
    ref onePassInst i = ref _addr_i.val;

    var next = i.MatchRunePos(r);
    if (next >= 0) {
        return i.Next[next];
    }
    if (i.Op == syntax.InstAltMatch) {
        return i.Out;
    }
    return 0;
}

private static syntax.InstOp iop(ptr<syntax.Inst> _addr_i) {
    ref syntax.Inst i = ref _addr_i.val;

    var op = i.Op;

    if (op == syntax.InstRune1 || op == syntax.InstRuneAny || op == syntax.InstRuneAnyNotNL) 
        op = syntax.InstRune;
        return op;
}

// Sparse Array implementation is used as a queueOnePass.
private partial struct queueOnePass {
    public slice<uint> sparse;
    public slice<uint> dense;
    public uint size;
    public uint nextIndex;
}

private static bool empty(this ptr<queueOnePass> _addr_q) {
    ref queueOnePass q = ref _addr_q.val;

    return q.nextIndex >= q.size;
}

private static uint next(this ptr<queueOnePass> _addr_q) {
    uint n = default;
    ref queueOnePass q = ref _addr_q.val;

    n = q.dense[q.nextIndex];
    q.nextIndex++;
    return ;
}

private static void clear(this ptr<queueOnePass> _addr_q) {
    ref queueOnePass q = ref _addr_q.val;

    q.size = 0;
    q.nextIndex = 0;
}

private static bool contains(this ptr<queueOnePass> _addr_q, uint u) {
    ref queueOnePass q = ref _addr_q.val;

    if (u >= uint32(len(q.sparse))) {
        return false;
    }
    return q.sparse[u] < q.size && q.dense[q.sparse[u]] == u;
}

private static void insert(this ptr<queueOnePass> _addr_q, uint u) {
    ref queueOnePass q = ref _addr_q.val;

    if (!q.contains(u)) {
        q.insertNew(u);
    }
}

private static void insertNew(this ptr<queueOnePass> _addr_q, uint u) {
    ref queueOnePass q = ref _addr_q.val;

    if (u >= uint32(len(q.sparse))) {
        return ;
    }
    q.sparse[u] = q.size;
    q.dense[q.size] = u;
    q.size++;
}

private static ptr<queueOnePass> newQueue(nint size) {
    ptr<queueOnePass> q = default!;

    return addr(new queueOnePass(sparse:make([]uint32,size),dense:make([]uint32,size),));
}

// mergeRuneSets merges two non-intersecting runesets, and returns the merged result,
// and a NextIp array. The idea is that if a rune matches the OnePassRunes at index
// i, NextIp[i/2] is the target. If the input sets intersect, an empty runeset and a
// NextIp array with the single element mergeFailed is returned.
// The code assumes that both inputs contain ordered and non-intersecting rune pairs.
private static readonly var mergeFailed = uint32(0xffffffff);



private static int noRune = new slice<int>(new int[] {  });private static uint noNext = new slice<uint>(new uint[] { mergeFailed });

private static (slice<int>, slice<uint>) mergeRuneSets(ptr<slice<int>> _addr_leftRunes, ptr<slice<int>> _addr_rightRunes, uint leftPC, uint rightPC) => func((defer, panic, _) => {
    slice<int> _p0 = default;
    slice<uint> _p0 = default;
    ref slice<int> leftRunes = ref _addr_leftRunes.val;
    ref slice<int> rightRunes = ref _addr_rightRunes.val;

    var leftLen = len(leftRunes);
    var rightLen = len(rightRunes);
    if (leftLen & 0x1 != 0 || rightLen & 0x1 != 0) {
        panic("mergeRuneSets odd length []rune");
    }
    ref nint lx = ref heap(out ptr<nint> _addr_lx);    ref nint rx = ref heap(out ptr<nint> _addr_rx);
    var merged = make_slice<int>(0);
    var next = make_slice<uint>(0);
    var ok = true;
    defer(() => {
        if (!ok) {
            merged = null;
            next = null;
        }
    }());

    nint ix = -1;
    Func<ptr<nint>, ptr<slice<int>>, uint, bool> extend = (newLow, newArray, pc) => {
        if (ix > 0 && (newArray.val)[newLow.val] <= merged[ix]) {
            return false;
        }
        merged = append(merged, (newArray.val)[newLow.val], (newArray.val)[newLow + 1.val]);
        newLow.val += 2;
        ix += 2;
        next = append(next, pc);
        return true;
    };

    while (lx < leftLen || rx < rightLen) {

        if (rx >= rightLen) 
            ok = extend(_addr_lx, leftRunes, leftPC);
        else if (lx >= leftLen) 
            ok = extend(_addr_rx, rightRunes, rightPC);
        else if ((rightRunes)[rx] < (leftRunes)[lx]) 
            ok = extend(_addr_rx, rightRunes, rightPC);
        else 
            ok = extend(_addr_lx, leftRunes, leftPC);
                if (!ok) {
            return (noRune, noNext);
        }
    }
    return (merged, next);
});

// cleanupOnePass drops working memory, and restores certain shortcut instructions.
private static void cleanupOnePass(ptr<onePassProg> _addr_prog, ptr<syntax.Prog> _addr_original) {
    ref onePassProg prog = ref _addr_prog.val;
    ref syntax.Prog original = ref _addr_original.val;

    foreach (var (ix, instOriginal) in original.Inst) {

        if (instOriginal.Op == syntax.InstAlt || instOriginal.Op == syntax.InstAltMatch || instOriginal.Op == syntax.InstRune)         else if (instOriginal.Op == syntax.InstCapture || instOriginal.Op == syntax.InstEmptyWidth || instOriginal.Op == syntax.InstNop || instOriginal.Op == syntax.InstMatch || instOriginal.Op == syntax.InstFail) 
            prog.Inst[ix].Next = null;
        else if (instOriginal.Op == syntax.InstRune1 || instOriginal.Op == syntax.InstRuneAny || instOriginal.Op == syntax.InstRuneAnyNotNL) 
            prog.Inst[ix].Next = null;
            prog.Inst[ix] = new onePassInst(Inst:instOriginal);
            }
}

// onePassCopy creates a copy of the original Prog, as we'll be modifying it
private static ptr<onePassProg> onePassCopy(ptr<syntax.Prog> _addr_prog) {
    ref syntax.Prog prog = ref _addr_prog.val;

    ptr<onePassProg> p = addr(new onePassProg(Start:prog.Start,NumCap:prog.NumCap,Inst:make([]onePassInst,len(prog.Inst)),));
    foreach (var (i, inst) in prog.Inst) {
        p.Inst[i] = new onePassInst(Inst:inst);
    }    foreach (var (pc) in p.Inst) {

        if (p.Inst[pc].Op == syntax.InstAlt || p.Inst[pc].Op == syntax.InstAltMatch) 
            // A:Bx + B:Ay
            var p_A_Other = _addr_p.Inst[pc].Out;
            var p_A_Alt = _addr_p.Inst[pc].Arg; 
            // make sure a target is another Alt
            var instAlt = p.Inst[p_A_Alt.val];
            if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch)) {
                (p_A_Alt, p_A_Other) = (p_A_Other, p_A_Alt);                instAlt = p.Inst[p_A_Alt.val];
                if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch)) {
                    continue;
                }
            }
            var instOther = p.Inst[p_A_Other.val]; 
            // Analyzing both legs pointing to Alts is for another day
            if (instOther.Op == syntax.InstAlt || instOther.Op == syntax.InstAltMatch) { 
                // too complicated
                continue;
            } 
            // simple empty transition loop
            // A:BC + B:DA => A:BC + B:DC
            var p_B_Alt = _addr_p.Inst[p_A_Alt.val].Out;
            var p_B_Other = _addr_p.Inst[p_A_Alt.val].Arg;
            var patch = false;
            if (instAlt.Out == uint32(pc)) {
                patch = true;
            }
            else if (instAlt.Arg == uint32(pc)) {
                patch = true;
                (p_B_Alt, p_B_Other) = (p_B_Other, p_B_Alt);
            }
            if (patch) {
                p_B_Alt.val = p_A_Other.val;
            } 

            // empty transition to common target
            // A:BC + B:DC => A:DC + B:DC
            if (p_A_Other == p_B_Alt.val) {
                p_A_Alt.val = p_B_Other.val;
            }
        else 
            continue;
            }    return _addr_p!;
}

// runeSlice exists to permit sorting the case-folded rune sets.
private partial struct runeSlice { // : slice<int>
}

private static nint Len(this runeSlice p) {
    return len(p);
}
private static bool Less(this runeSlice p, nint i, nint j) {
    return p[i] < p[j];
}
private static void Swap(this runeSlice p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

private static int anyRuneNotNL = new slice<int>(new int[] { 0, '\n'-1, '\n'+1, unicode.MaxRune });
private static int anyRune = new slice<int>(new int[] { 0, unicode.MaxRune });

// makeOnePass creates a onepass Prog, if possible. It is possible if at any alt,
// the match engine can always tell which branch to take. The routine may modify
// p if it is turned into a onepass Prog. If it isn't possible for this to be a
// onepass Prog, the Prog nil is returned. makeOnePass is recursive
// to the size of the Prog.
private static ptr<onePassProg> makeOnePass(ptr<onePassProg> _addr_p) {
    ref onePassProg p = ref _addr_p.val;
 
    // If the machine is very long, it's not worth the time to check if we can use one pass.
    if (len(p.Inst) >= 1000) {
        return _addr_null!;
    }
    var instQueue = newQueue(len(p.Inst));    var visitQueue = newQueue(len(p.Inst));    Func<uint, slice<bool>, bool> check = default;    var onePassRunes = make_slice<slice<int>>(len(p.Inst)); 

    // check that paths from Alt instructions are unambiguous, and rebuild the new
    // program as a onepass program
    check = (pc, m) => {
        ok = true;
        var inst = _addr_p.Inst[pc];
        if (visitQueue.contains(pc)) {
            return ;
        }
        visitQueue.insert(pc);

        if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
            ok = check(inst.Out, m) && check(inst.Arg, m); 
            // check no-input paths to InstMatch
            var matchOut = m[inst.Out];
            var matchArg = m[inst.Arg];
            if (matchOut && matchArg) {
                ok = false;
                break;
            } 
            // Match on empty goes in inst.Out
            if (matchArg) {
                (inst.Out, inst.Arg) = (inst.Arg, inst.Out);                (matchOut, matchArg) = (matchArg, matchOut);
            }
            if (matchOut) {
                m[pc] = true;
                inst.Op = syntax.InstAltMatch;
            } 

            // build a dispatch operator from the two legs of the alt.
            onePassRunes[pc], inst.Next = mergeRuneSets(_addr_onePassRunes[inst.Out], _addr_onePassRunes[inst.Arg], inst.Out, inst.Arg);
            if (len(inst.Next) > 0 && inst.Next[0] == mergeFailed) {
                ok = false;
                break;
            }
        else if (inst.Op == syntax.InstCapture || inst.Op == syntax.InstNop) 
            ok = check(inst.Out, m);
            m[pc] = m[inst.Out]; 
            // pass matching runes back through these no-ops.
            onePassRunes[pc] = append(new slice<int>(new int[] {  }), onePassRunes[inst.Out]);
            inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2 + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in inst.Next) {
                    i = __i;
                    inst.Next[i] = inst.Out;
                }

                i = i__prev1;
            }
        else if (inst.Op == syntax.InstEmptyWidth) 
            ok = check(inst.Out, m);
            m[pc] = m[inst.Out];
            onePassRunes[pc] = append(new slice<int>(new int[] {  }), onePassRunes[inst.Out]);
            inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2 + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in inst.Next) {
                    i = __i;
                    inst.Next[i] = inst.Out;
                }

                i = i__prev1;
            }
        else if (inst.Op == syntax.InstMatch || inst.Op == syntax.InstFail) 
            m[pc] = inst.Op == syntax.InstMatch;
        else if (inst.Op == syntax.InstRune) 
            m[pc] = false;
            if (len(inst.Next) > 0) {
                break;
            }
            instQueue.insert(inst.Out);
            if (len(inst.Rune) == 0) {
                onePassRunes[pc] = new slice<int>(new int[] {  });
                inst.Next = new slice<uint>(new uint[] { inst.Out });
                break;
            }
            var runes = make_slice<int>(0);
            if (len(inst.Rune) == 1 && syntax.Flags(inst.Arg) & syntax.FoldCase != 0) {
                var r0 = inst.Rune[0];
                runes = append(runes, r0, r0);
                {
                    var r1__prev1 = r1;

                    var r1 = unicode.SimpleFold(r0);

                    while (r1 != r0) {
                        runes = append(runes, r1, r1);
                        r1 = unicode.SimpleFold(r1);
                    }
            else


                    r1 = r1__prev1;
                }
                sort.Sort(runeSlice(runes));
            } {
                runes = append(runes, inst.Rune);
            }
            onePassRunes[pc] = runes;
            inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2 + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in inst.Next) {
                    i = __i;
                    inst.Next[i] = inst.Out;
                }

                i = i__prev1;
            }

            inst.Op = syntax.InstRune;
        else if (inst.Op == syntax.InstRune1) 
            m[pc] = false;
            if (len(inst.Next) > 0) {
                break;
            }
            instQueue.insert(inst.Out);
            runes = new slice<int>(new int[] {  }); 
            // expand case-folded runes
            if (syntax.Flags(inst.Arg) & syntax.FoldCase != 0) {
                r0 = inst.Rune[0];
                runes = append(runes, r0, r0);
                {
                    var r1__prev1 = r1;

                    r1 = unicode.SimpleFold(r0);

                    while (r1 != r0) {
                        runes = append(runes, r1, r1);
                        r1 = unicode.SimpleFold(r1);
                    }
            else


                    r1 = r1__prev1;
                }
                sort.Sort(runeSlice(runes));
            } {
                runes = append(runes, inst.Rune[0], inst.Rune[0]);
            }
            onePassRunes[pc] = runes;
            inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2 + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in inst.Next) {
                    i = __i;
                    inst.Next[i] = inst.Out;
                }

                i = i__prev1;
            }

            inst.Op = syntax.InstRune;
        else if (inst.Op == syntax.InstRuneAny) 
            m[pc] = false;
            if (len(inst.Next) > 0) {
                break;
            }
            instQueue.insert(inst.Out);
            onePassRunes[pc] = append(new slice<int>(new int[] {  }), anyRune);
            inst.Next = new slice<uint>(new uint[] { inst.Out });
        else if (inst.Op == syntax.InstRuneAnyNotNL) 
            m[pc] = false;
            if (len(inst.Next) > 0) {
                break;
            }
            instQueue.insert(inst.Out);
            onePassRunes[pc] = append(new slice<int>(new int[] {  }), anyRuneNotNL);
            inst.Next = make_slice<uint>(len(onePassRunes[pc]) / 2 + 1);
            {
                var i__prev1 = i;

                foreach (var (__i) in inst.Next) {
                    i = __i;
                    inst.Next[i] = inst.Out;
                }

                i = i__prev1;
            }
                return ;
    };

    instQueue.clear();
    instQueue.insert(uint32(p.Start));
    var m = make_slice<bool>(len(p.Inst));
    while (!instQueue.empty()) {
        visitQueue.clear();
        var pc = instQueue.next();
        if (!check(pc, m)) {
            p = null;
            break;
        }
    }
    if (p != null) {
        {
            var i__prev1 = i;

            foreach (var (__i) in p.Inst) {
                i = __i;
                p.Inst[i].Rune = onePassRunes[i];
            }

            i = i__prev1;
        }
    }
    return _addr_p!;
}

// compileOnePass returns a new *syntax.Prog suitable for onePass execution if the original Prog
// can be recharacterized as a one-pass regexp program, or syntax.nil if the
// Prog cannot be converted. For a one pass prog, the fundamental condition that must
// be true is: at any InstAlt, there must be no ambiguity about what branch to  take.
private static ptr<onePassProg> compileOnePass(ptr<syntax.Prog> _addr_prog) {
    ptr<onePassProg> p = default!;
    ref syntax.Prog prog = ref _addr_prog.val;

    if (prog.Start == 0) {
        return _addr_null!;
    }
    if (prog.Inst[prog.Start].Op != syntax.InstEmptyWidth || syntax.EmptyOp(prog.Inst[prog.Start].Arg) & syntax.EmptyBeginText != syntax.EmptyBeginText) {
        return _addr_null!;
    }
    foreach (var (_, inst) in prog.Inst) {
        var opOut = prog.Inst[inst.Out].Op;

        if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
            if (opOut == syntax.InstMatch || prog.Inst[inst.Arg].Op == syntax.InstMatch) {
                return _addr_null!;
            }
        else if (inst.Op == syntax.InstEmptyWidth) 
            if (opOut == syntax.InstMatch) {
                if (syntax.EmptyOp(inst.Arg) & syntax.EmptyEndText == syntax.EmptyEndText) {
                    continue;
                }
                return _addr_null!;
            }
        else 
            if (opOut == syntax.InstMatch) {
                return _addr_null!;
            }
            }    p = onePassCopy(_addr_prog); 

    // checkAmbiguity on InstAlts, build onepass Prog if possible
    p = makeOnePass(_addr_p);

    if (p != null) {
        cleanupOnePass(_addr_p, _addr_prog);
    }
    return _addr_p!;
}

} // end regexp_package
