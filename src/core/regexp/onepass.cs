// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syntax = regexp.syntax_package;
using slices = slices_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using regexp;
using unicode;

partial class regexp_package {

// "One-pass" regexp execution.
// Some regexps can be analyzed to determine that they never need
// backtracking: they are guaranteed to run in one pass over the string
// without bothering to save all the usual NFA state.
// Detect those and execute them more quickly.

// A onePassProg is a compiled one-pass regular expression program.
// It is the same as syntax.Prog except for the use of onePassInst.
[GoType] partial struct onePassProg {
    public slice<onePassInst> Inst;
    public nint Start; // index of start instruction
    public nint NumCap; // number of InstCapture insts in re
}

// A onePassInst is a single instruction in a one-pass regular expression program.
// It is the same as syntax.Inst except for the new 'Next' field.
[GoType] partial struct onePassInst {
    public partial ref regexp.syntax_package.Inst Inst { get; }
    public slice<uint32> Next;
}

// onePassPrefix returns a literal string that all matches for the
// regexp must start with. Complete is true if the prefix
// is the entire match. Pc is the index of the last rune instruction
// in the string. The onePassPrefix skips over the mandatory
// EmptyBeginText.
internal static (@string prefix, bool complete, uint32 pc) onePassPrefix(ж<syntax.Prog> Ꮡp) {
    @string prefix = default!;
    bool complete = default!;
    uint32 pc = default!;

    ref var p = ref Ꮡp.val;
    var i = Ꮡ(p.Inst, p.Start);
    if ((~i).Op != syntax.InstEmptyWidth || (syntax.EmptyOp)((((syntax.EmptyOp)(~i).Arg)) & syntax.EmptyBeginText) == 0) {
        return ("", (~i).Op == syntax.InstMatch, ((uint32)p.Start));
    }
    pc = i.val.Out;
    i = Ꮡ(p.Inst, pc);
    while ((~i).Op == syntax.InstNop) {
        pc = i.val.Out;
        i = Ꮡ(p.Inst, pc);
    }
    // Avoid allocation of buffer if prefix is empty.
    if (iop(i) != syntax.InstRune || len((~i).Rune) != 1) {
        return ("", (~i).Op == syntax.InstMatch, ((uint32)p.Start));
    }
    // Have prefix; gather characters.
    strings.Builder buf = default!;
    while (iop(i) == syntax.InstRune && len((~i).Rune) == 1 && (syntax.Flags)(((syntax.Flags)(~i).Arg) & syntax.FoldCase) == 0 && (~i).Rune[0] != utf8.RuneError) {
        buf.WriteRune((~i).Rune[0]);
        (pc, i) = (i.val.Out, Ꮡ(p.Inst, (~i).Out));
    }
    if ((~i).Op == syntax.InstEmptyWidth && (syntax.EmptyOp)(((syntax.EmptyOp)(~i).Arg) & syntax.EmptyEndText) != 0 && p.Inst[(~i).Out].Op == syntax.InstMatch) {
        complete = true;
    }
    return (buf.String(), complete, pc);
}

// onePassNext selects the next actionable state of the prog, based on the input character.
// It should only be called when i.Op == InstAlt or InstAltMatch, and from the one-pass machine.
// One of the alternates may ultimately lead without input to end of line. If the instruction
// is InstAltMatch the path to the InstMatch is in i.Out, the normal node in i.Next.
internal static uint32 onePassNext(ж<onePassInst> Ꮡi, rune r) {
    ref var i = ref Ꮡi.val;

    nint next = i.MatchRunePos(r);
    if (next >= 0) {
        return i.Next[next];
    }
    if (i.Op == syntax.InstAltMatch) {
        return i.Out;
    }
    return 0;
}

internal static syntax.InstOp iop(ж<syntax.Inst> Ꮡi) {
    ref var i = ref Ꮡi.val;

    var op = i.Op;
    var exprᴛ1 = op;
    if (exprᴛ1 == syntax.InstRune1 || exprᴛ1 == syntax.InstRuneAny || exprᴛ1 == syntax.InstRuneAnyNotNL) {
        op = syntax.InstRune;
    }

    return op;
}

// Sparse Array implementation is used as a queueOnePass.
[GoType] partial struct queueOnePass {
    internal slice<uint32> sparse;
    internal slice<uint32> dense;
    internal uint32 size;
    internal uint32 nextIndex;
}

[GoRecv] internal static bool empty(this ref queueOnePass q) {
    return q.nextIndex >= q.size;
}

[GoRecv] internal static uint32 /*n*/ next(this ref queueOnePass q) {
    uint32 n = default!;

    n = q.dense[q.nextIndex];
    q.nextIndex++;
    return n;
}

[GoRecv] internal static void clear(this ref queueOnePass q) {
    q.size = 0;
    q.nextIndex = 0;
}

[GoRecv] internal static bool contains(this ref queueOnePass q, uint32 u) {
    if (u >= ((uint32)len(q.sparse))) {
        return false;
    }
    return q.sparse[u] < q.size && q.dense[q.sparse[u]] == u;
}

[GoRecv] internal static void insert(this ref queueOnePass q, uint32 u) {
    if (!q.contains(u)) {
        q.insertNew(u);
    }
}

[GoRecv] internal static void insertNew(this ref queueOnePass q, uint32 u) {
    if (u >= ((uint32)len(q.sparse))) {
        return;
    }
    q.sparse[u] = q.size;
    q.dense[q.size] = u;
    q.size++;
}

internal static ж<queueOnePass> /*q*/ newQueue(nint size) {
    ж<queueOnePass> q = default!;

    return Ꮡ(new queueOnePass(
        sparse: new slice<uint32>(size),
        dense: new slice<uint32>(size)
    ));
}

// mergeRuneSets merges two non-intersecting runesets, and returns the merged result,
// and a NextIp array. The idea is that if a rune matches the OnePassRunes at index
// i, NextIp[i/2] is the target. If the input sets intersect, an empty runeset and a
// NextIp array with the single element mergeFailed is returned.
// The code assumes that both inputs contain ordered and non-intersecting rune pairs.
internal const uint32 mergeFailed = /* uint32(0xffffffff) */ 4294967295;

internal static slice<rune> noRune = new rune[]{}.slice();
internal static slice<uint32> noNext = new uint32[]{mergeFailed}.slice();

internal static (slice<rune>, slice<uint32>) mergeRuneSets(ж<slice<rune>> ᏑleftRunes, ж<slice<rune>> ᏑrightRunes, uint32 leftPC, uint32 rightPC) => func((defer, _) => {
    ref var leftRunes = ref ᏑleftRunes.val;
    ref var rightRunes = ref ᏑrightRunes.val;

    nint leftLen = len(leftRunes);
    nint rightLen = len(rightRunes);
    if ((nint)(leftLen & 1) != 0 || (nint)(rightLen & 1) != 0) {
        throw panic("mergeRuneSets odd length []rune");
    }
    ref var lx = ref heap(new nint(), out var Ꮡlx);
    ref var rx = ref heap(new nint(), out var Ꮡrx);
    var merged = new slice<rune>(0);
    var next = new slice<uint32>(0);
    var ok = true;
    var mergedʗ1 = merged;
    var nextʗ1 = next;
    defer(() => {
        if (!ok) {
            mergedʗ1 = default!;
            nextʗ1 = default!;
        }
    });
    nint ix = -1;
    var extend = 
    var mergedʗ2 = merged;
    var nextʗ2 = next;
    (ж<nint> newLow, ж<slice<rune>> newArray, uint32 pc) => {
        if (ix > 0 && (ж<ж<slice<rune>>>)[newLow.val] <= mergedʗ2[ix]) {
            return false;
        }
        mergedʗ2 = append(mergedʗ2, (ж<ж<slice<rune>>>)[newLow.val], (ж<ж<slice<rune>>>)[newLow.val + 1]);
        newLow.val += 2;
        ix += 2;
        nextʗ2 = append(nextʗ2, pc);
        return true;
    };
    while (lx < leftLen || rx < rightLen) {
        switch (ᐧ) {
        case {} when rx is >= rightLen: {
            ok = extend(Ꮡlx, ᏑleftRunes, leftPC);
            break;
        }
        case {} when lx is >= leftLen: {
            ok = extend(Ꮡrx, ᏑrightRunes, rightPC);
            break;
        }
        case {} when (rightRunes)[rx] is < (leftRunes)[lx]: {
            ok = extend(Ꮡrx, ᏑrightRunes, rightPC);
            break;
        }
        default: {
            ok = extend(Ꮡlx, ᏑleftRunes, leftPC);
            break;
        }}

        if (!ok) {
            return (noRune, noNext);
        }
    }
    return (merged, next);
});

// cleanupOnePass drops working memory, and restores certain shortcut instructions.
internal static void cleanupOnePass(ж<onePassProg> Ꮡprog, ж<syntax.Prog> Ꮡoriginal) {
    ref var prog = ref Ꮡprog.val;
    ref var original = ref Ꮡoriginal.val;

    foreach (var (ix, instOriginal) in original.Inst) {
        var exprᴛ1 = instOriginal.Op;
        if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch || exprᴛ1 == syntax.InstRune) {
        }
        else if (exprᴛ1 == syntax.InstCapture || exprᴛ1 == syntax.InstEmptyWidth || exprᴛ1 == syntax.InstNop || exprᴛ1 == syntax.InstMatch || exprᴛ1 == syntax.InstFail) {
            prog.Inst[ix].Next = default!;
        }
        else if (exprᴛ1 == syntax.InstRune1 || exprᴛ1 == syntax.InstRuneAny || exprᴛ1 == syntax.InstRuneAnyNotNL) {
            prog.Inst[ix].Next = default!;
            prog.Inst[ix] = new onePassInst(Inst: instOriginal);
        }

    }
}

// onePassCopy creates a copy of the original Prog, as we'll be modifying it.
internal static ж<onePassProg> onePassCopy(ж<syntax.Prog> Ꮡprog) {
    ref var prog = ref Ꮡprog.val;

    var p = Ꮡ(new onePassProg(
        Start: prog.Start,
        NumCap: prog.NumCap,
        Inst: new slice<onePassInst>(len(prog.Inst))
    ));
    foreach (var (i, inst) in prog.Inst) {
        (~p).Inst[i] = new onePassInst(Inst: inst);
    }
    // rewrites one or more common Prog constructs that enable some otherwise
    // non-onepass Progs to be onepass. A:BD (for example) means an InstAlt at
    // ip A, that points to ips B & C.
    // A:BC + B:DA => A:BC + B:CD
    // A:BC + B:DC => A:DC + B:DC
    foreach (var (pc, _) in (~p).Inst) {
        var exprᴛ1 = (~p).Inst[pc].Op;
        { /* default: */
            continue;
        }
        else if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch) {
            var p_A_Other = Ꮡ(~p).Inst[pc].of(onePassInst.ᏑOut);
            var p_A_Alt = Ꮡ(~p).Inst[pc].of(onePassInst.ᏑArg);
            var instAlt = (~p).Inst[p_A_Alt.val];
            if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch)) {
                // A:Bx + B:Ay
                // make sure a target is another Alt
                (p_A_Alt, p_A_Other) = (p_A_Other, p_A_Alt);
                instAlt = (~p).Inst[p_A_Alt.val];
                if (!(instAlt.Op == syntax.InstAlt || instAlt.Op == syntax.InstAltMatch)) {
                    continue;
                }
            }
            var instOther = (~p).Inst[p_A_Other.val];
            if (instOther.Op == syntax.InstAlt || instOther.Op == syntax.InstAltMatch) {
                // Analyzing both legs pointing to Alts is for another day
                // too complicated
                continue;
            }
            var p_B_Alt = Ꮡ(~p).Inst[p_A_Alt.val].of(onePassInst.ᏑOut);
            var p_B_Other = Ꮡ(~p).Inst[p_A_Alt.val].of(onePassInst.ᏑArg);
            var patch = false;
            if (instAlt.Out == ((uint32)pc)){
                // simple empty transition loop
                // A:BC + B:DA => A:BC + B:DC
                patch = true;
            } else 
            if (instAlt.Arg == ((uint32)pc)) {
                patch = true;
                (p_B_Alt, p_B_Other) = (p_B_Other, p_B_Alt);
            }
            if (patch) {
                p_B_Alt.val = p_A_Other.val;
            }
            if (p_A_Other.val == p_B_Alt.val) {
                // empty transition to common target
                // A:BC + B:DC => A:DC + B:DC
                p_A_Alt.val = p_B_Other.val;
            }
        }

    }
    return p;
}

internal static slice<rune> anyRuneNotNL = new rune[]{0, (rune)'\n' - 1, (rune)'\n' + 1, unicode.MaxRune}.slice();

internal static slice<rune> anyRune = new rune[]{0, unicode.MaxRune}.slice();

// makeOnePass creates a onepass Prog, if possible. It is possible if at any alt,
// the match engine can always tell which branch to take. The routine may modify
// p if it is turned into a onepass Prog. If it isn't possible for this to be a
// onepass Prog, the Prog nil is returned. makeOnePass is recursive
// to the size of the Prog.
internal static ж<onePassProg> makeOnePass(ж<onePassProg> Ꮡp) {
    ref var p = ref Ꮡp.val;

    // If the machine is very long, it's not worth the time to check if we can use one pass.
    if (len(p.Inst) >= 1000) {
        return default!;
    }
    ж<queueOnePass> instQueue = newQueue(len(p.Inst));
    ж<queueOnePass> visitQueue = newQueue(len(p.Inst));
    Func<uint32, slice<bool>, bool> check = default!;
    slice<slice<rune>> onePassRunes = new slice<slice<rune>>(len(p.Inst));
    // check that paths from Alt instructions are unambiguous, and rebuild the new
    // program as a onepass program
    check = 
    var anyRuneʗ1 = anyRune;
    var anyRuneNotNLʗ1 = anyRuneNotNL;
    var checkʗ1 = check;
    var instQueueʗ1 = instQueue;
    var onePassRunesʗ1 = onePassRunes;
    var visitQueueʗ1 = visitQueue;
    (uint32 pc, slice<bool> m) => {
        ok = true;
        var inst = Ꮡ(p.Inst, pc);
        if (visitQueueʗ1.contains(pc)) {
            return;
        }
        visitQueueʗ1.insert(pc);
        var exprᴛ1 = inst.Op;
        if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch) {
            ok = checkʗ1(inst.Out, mΔ1) && checkʗ1(inst.Arg, mΔ1);
            var matchOut = mΔ1[inst.Out];
            var matchArg = mΔ1[inst.Arg];
            if (matchOut && matchArg) {
                // check no-input paths to InstMatch
                ok = false;
                break;
            }
            if (matchArg) {
                // Match on empty goes in inst.Out
                (inst.Out, inst.Arg) = (inst.Arg, inst.Out);
                (matchOut, matchArg) = (matchArg, matchOut);
            }
            if (matchOut) {
                [pc] = true;
                inst.Op = syntax.InstAltMatch;
            }
            (onePassRunesʗ1[pc], inst.val.Next) = mergeRuneSets(
                Ꮡ(onePassRunesʗ1, inst.Out), // build a dispatch operator from the two legs of the alt.
 Ꮡ(onePassRunesʗ1, inst.Arg), inst.Out, inst.Arg);
            if (len((~inst).Next) > 0 && (~inst).Next[0] == mergeFailed) {
                ok = false;
                break;
            }
        }
        else if (exprᴛ1 == syntax.InstCapture || exprᴛ1 == syntax.InstNop) {
            ok = checkʗ1(inst.Out, mΔ1);
            [pc] = mΔ1[inst.Out];
            onePassRunesʗ1[pc] = append(new rune[]{}.slice(), // pass matching runes back through these no-ops.
 onePassRunesʗ1[inst.Out].ꓸꓸꓸ);
            inst.val.Next = new slice<uint32>(len(onePassRunesʗ1[pc]) / 2 + 1);
            foreach (var (i, _) in (~inst).Next) {
                (~inst).Next[i] = inst.Out;
            }
        }
        else if (exprᴛ1 == syntax.InstEmptyWidth) {
            ok = checkʗ1(inst.Out, mΔ1);
            [pc] = mΔ1[inst.Out];
            onePassRunesʗ1[pc] = append(new rune[]{}.slice(), onePassRunesʗ1[inst.Out].ꓸꓸꓸ);
            inst.val.Next = new slice<uint32>(len(onePassRunesʗ1[pc]) / 2 + 1);
            foreach (var (i, _) in (~inst).Next) {
                (~inst).Next[i] = inst.Out;
            }
        }
        else if (exprᴛ1 == syntax.InstMatch || exprᴛ1 == syntax.InstFail) {
            [pc] = inst.Op == syntax.InstMatch;
        }
        else if (exprᴛ1 == syntax.InstRune) {
            [pc] = false;
            if (len((~inst).Next) > 0) {
                break;
            }
            instQueueʗ1.insert(inst.Out);
            if (len(inst.Rune) == 0) {
                onePassRunesʗ1[pc] = new rune[]{}.slice();
                inst.val.Next = new uint32[]{inst.Out}.slice();
                break;
            }
            var runes = new slice<rune>(0);
            if (len(inst.Rune) == 1 && (syntax.Flags)(((syntax.Flags)inst.Arg) & syntax.FoldCase) != 0){
                var r0 = inst.Rune[0];
                runes = append(runes, r0, r0);
                for (var r1 = unicode.SimpleFold(r0); r1 != r0; r1 = unicode.SimpleFold(r1)) {
                    runes = append(runes, r1, r1);
                }
                slices.Sort(runes);
            } else {
                runes = append(runes, inst.Rune.ꓸꓸꓸ);
            }
            onePassRunesʗ1[pc] = runes;
            inst.val.Next = new slice<uint32>(len(onePassRunesʗ1[pc]) / 2 + 1);
            foreach (var (i, _) in (~inst).Next) {
                (~inst).Next[i] = inst.Out;
            }
            inst.Op = syntax.InstRune;
        }
        else if (exprᴛ1 == syntax.InstRune1) {
            [pc] = false;
            if (len((~inst).Next) > 0) {
                break;
            }
            instQueueʗ1.insert(inst.Out);
            var runes = new rune[]{}.slice();
            if ((syntax.Flags)(((syntax.Flags)inst.Arg) & syntax.FoldCase) != 0){
                // expand case-folded runes
                var r0 = inst.Rune[0];
                runes = append(runes, r0, r0);
                for (var r1 = unicode.SimpleFold(r0); r1 != r0; r1 = unicode.SimpleFold(r1)) {
                    runes = append(runes, r1, r1);
                }
                slices.Sort(runes);
            } else {
                runes = append(runes, inst.Rune[0], inst.Rune[0]);
            }
            onePassRunesʗ1[pc] = runes;
            inst.val.Next = new slice<uint32>(len(onePassRunesʗ1[pc]) / 2 + 1);
            foreach (var (i, _) in (~inst).Next) {
                (~inst).Next[i] = inst.Out;
            }
            inst.Op = syntax.InstRune;
        }
        else if (exprᴛ1 == syntax.InstRuneAny) {
            [pc] = false;
            if (len((~inst).Next) > 0) {
                break;
            }
            instQueueʗ1.insert(inst.Out);
            onePassRunesʗ1[pc] = append(new rune[]{}.slice(), anyRuneʗ1.ꓸꓸꓸ);
            inst.val.Next = new uint32[]{inst.Out}.slice();
        }
        else if (exprᴛ1 == syntax.InstRuneAnyNotNL) {
            [pc] = false;
            if (len((~inst).Next) > 0) {
                break;
            }
            instQueueʗ1.insert(inst.Out);
            onePassRunesʗ1[pc] = append(new rune[]{}.slice(), anyRuneNotNLʗ1.ꓸꓸꓸ);
            inst.val.Next = new slice<uint32>(len(onePassRunesʗ1[pc]) / 2 + 1);
            foreach (var (i, _) in (~inst).Next) {
                (~inst).Next[i] = inst.Out;
            }
        }

        return;
    };
    instQueue.clear();
    instQueue.insert(((uint32)p.Start));
    var m = new slice<bool>(len(p.Inst));
    while (!instQueue.empty()) {
        visitQueue.clear();
        var pc = instQueue.next();
        if (!check(pc, m)) {
            p = default!;
            break;
        }
    }
    if (p != nil) {
        foreach (var (i, _) in p.Inst) {
            p.Inst[i].Rune = onePassRunes[i];
        }
    }
    return Ꮡp;
}

// compileOnePass returns a new *syntax.Prog suitable for onePass execution if the original Prog
// can be recharacterized as a one-pass regexp program, or syntax.nil if the
// Prog cannot be converted. For a one pass prog, the fundamental condition that must
// be true is: at any InstAlt, there must be no ambiguity about what branch to  take.
internal static ж<onePassProg> /*p*/ compileOnePass(ж<syntax.Prog> Ꮡprog) {
    ж<onePassProg> p = default!;

    ref var prog = ref Ꮡprog.val;
    if (prog.Start == 0) {
        return default!;
    }
    // onepass regexp is anchored
    if (prog.Inst[prog.Start].Op != syntax.InstEmptyWidth || (syntax.EmptyOp)(((syntax.EmptyOp)prog.Inst[prog.Start].Arg) & syntax.EmptyBeginText) != syntax.EmptyBeginText) {
        return default!;
    }
    // every instruction leading to InstMatch must be EmptyEndText
    foreach (var (_, inst) in prog.Inst) {
        var opOut = prog.Inst[inst.Out].Op;
        var exprᴛ1 = inst.Op;
        { /* default: */
            if (opOut == syntax.InstMatch) {
                return default!;
            }
        }
        if (exprᴛ1 == syntax.InstAlt || exprᴛ1 == syntax.InstAltMatch) {
            if (opOut == syntax.InstMatch || prog.Inst[inst.Arg].Op == syntax.InstMatch) {
                return default!;
            }
        }
        if (exprᴛ1 == syntax.InstEmptyWidth) {
            if (opOut == syntax.InstMatch) {
                if ((syntax.EmptyOp)(((syntax.EmptyOp)inst.Arg) & syntax.EmptyEndText) == syntax.EmptyEndText) {
                    continue;
                }
                return default!;
            }
        }

    }
    // Creates a slightly optimized copy of the original Prog
    // that cleans up some Prog idioms that block valid onepass programs
    p = onePassCopy(Ꮡprog);
    // checkAmbiguity on InstAlts, build onepass Prog if possible
    p = makeOnePass(p);
    if (p != nil) {
        cleanupOnePass(p, Ꮡprog);
    }
    return p;
}

} // end regexp_package
