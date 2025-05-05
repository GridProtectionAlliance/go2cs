// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// backtrack is a regular expression search with submatch
// tracking for small regular expressions and texts. It allocates
// a bit vector with (length of input) * (length of prog) bits,
// to make sure it never explores the same (character position, instruction)
// state multiple times. This limits the search to run in time linear in
// the length of the test.
//
// backtrack is a fast replacement for the NFA code on small
// regexps when onepass cannot be used.
namespace go;

using syntax = regexp.syntax_package;
using sync = sync_package;
using regexp;

partial class regexp_package {

// A job is an entry on the backtracker's job stack. It holds
// the instruction pc and the position in the input.
[GoType] partial struct job {
    internal uint32 pc;
    internal bool arg;
    internal nint pos;
}

internal static readonly UntypedInt visitedBits = 32;
internal static readonly UntypedInt maxBacktrackProg = 500; // len(prog.Inst) <= max
internal static readonly UntypedInt maxBacktrackVector = /* 256 * 1024 */ 262144; // bit vector size <= max (bits)

// bitState holds state for the backtracker.
[GoType] partial struct bitState {
    internal nint end;
    internal slice<nint> cap;
    internal slice<nint> matchcap;
    internal slice<job> jobs;
    internal slice<uint32> visited;
    internal inputs inputs;
}

internal static sync.Pool bitStatePool;

internal static ж<bitState> newBitState() {
    var (b, ok) = bitStatePool.Get()._<bitState.val>(ᐧ);
    if (!ok) {
        b = @new<bitState>();
    }
    return b;
}

internal static void freeBitState(ж<bitState> Ꮡb) {
    ref var b = ref Ꮡb.val;

    b.inputs.clear();
    bitStatePool.Put(b);
}

// maxBitStateLen returns the maximum length of a string to search with
// the backtracker using prog.
internal static nint maxBitStateLen(ж<syntax.Prog> Ꮡprog) {
    ref var prog = ref Ꮡprog.val;

    if (!shouldBacktrack(Ꮡprog)) {
        return 0;
    }
    return maxBacktrackVector / len(prog.Inst);
}

// shouldBacktrack reports whether the program is too
// long for the backtracker to run.
internal static bool shouldBacktrack(ж<syntax.Prog> Ꮡprog) {
    ref var prog = ref Ꮡprog.val;

    return len(prog.Inst) <= maxBacktrackProg;
}

// reset resets the state of the backtracker.
// end is the end position in the input.
// ncap is the number of captures.
[GoRecv] internal static void reset(this ref bitState b, ж<syntax.Prog> Ꮡprog, nint end, nint ncap) {
    ref var prog = ref Ꮡprog.val;

    b.end = end;
    if (cap(b.jobs) == 0){
        b.jobs = new slice<job>(0, 256);
    } else {
        b.jobs = b.jobs[..0];
    }
    nint visitedSize = (len(prog.Inst) * (end + 1) + visitedBits - 1) / visitedBits;
    if (cap(b.visited) < visitedSize){
        b.visited = new slice<uint32>(visitedSize, maxBacktrackVector / visitedBits);
    } else {
        b.visited = b.visited[..(int)(visitedSize)];
        clear(b.visited);
    }
    // set to 0
    if (cap(b.cap) < ncap){
        b.cap = new slice<nint>(ncap);
    } else {
        b.cap = b.cap[..(int)(ncap)];
    }
    foreach (var (i, _) in b.cap) {
        b.cap[i] = -1;
    }
    if (cap(b.matchcap) < ncap){
        b.matchcap = new slice<nint>(ncap);
    } else {
        b.matchcap = b.matchcap[..(int)(ncap)];
    }
    foreach (var (i, _) in b.matchcap) {
        b.matchcap[i] = -1;
    }
}

// shouldVisit reports whether the combination of (pc, pos) has not
// been visited yet.
[GoRecv] internal static bool shouldVisit(this ref bitState b, uint32 pc, nint pos) {
    nuint n = ((nuint)(((nint)pc) * (b.end + 1) + pos));
    if ((uint32)(b.visited[n / visitedBits] & (1 << (int)(((nuint)(n & (visitedBits - 1)))))) != 0) {
        return false;
    }
    b.visited[n / visitedBits] |= (uint32)(1 << (int)(((nuint)(n & (visitedBits - 1)))));
    return true;
}

// push pushes (pc, pos, arg) onto the job stack if it should be
// visited.
[GoRecv] internal static void push(this ref bitState b, ж<Regexp> Ꮡre, uint32 pc, nint pos, bool arg) {
    ref var re = ref Ꮡre.val;

    // Only check shouldVisit when arg is false.
    // When arg is true, we are continuing a previous visit.
    if (re.prog.Inst[pc].Op != syntax.InstFail && (arg || b.shouldVisit(pc, pos))) {
        b.jobs = append(b.jobs, new job(pc: pc, arg: arg, pos: pos));
    }
}

// tryBacktrack runs a backtracking search starting at pos.
[GoRecv] public static bool tryBacktrack(this ref Regexp re, ж<bitState> Ꮡb, input i, uint32 pc, nint pos) {
    ref var b = ref Ꮡb.val;

    var longest = re.longest;
    b.push(re, pc, pos, false);
    while (len(b.jobs) > 0) {
        nint l = len(b.jobs) - 1;
        // Pop job off the stack.
        var pcΔ1 = b.jobs[l].pc;
        nint posΔ1 = b.jobs[l].pos;
        var arg = b.jobs[l].arg;
        b.jobs = b.jobs[..(int)(l)];
        // Optimization: rather than push and pop,
        // code that is going to Push and continue
        // the loop simply updates ip, p, and arg
        // and jumps to CheckAndLoop. We have to
        // do the ShouldVisit check that Push
        // would have, but we avoid the stack
        // manipulation.
        goto Skip;
CheckAndLoop:
        if (!b.shouldVisit(pcΔ1, posΔ1)) {
            continue;
        }
Skip:
        var inst = Ꮡ(re.prog.Inst[pcΔ1]);
        var exprᴛ1 = (~inst).Op;
        { /* default: */
            throw panic("bad inst");
        }
        else if (exprᴛ1 == syntax.InstFail) {
            throw panic("unexpected InstFail");
        }
        else if (exprᴛ1 == syntax.InstAlt) {
            if (arg){
                // Cannot just
                //   b.push(inst.Out, pos, false)
                //   b.push(inst.Arg, pos, false)
                // If during the processing of inst.Out, we encounter
                // inst.Arg via another path, we want to process it then.
                // Pushing it here will inhibit that. Instead, re-push
                // inst with arg==true as a reminder to push inst.Arg out
                // later.
                // Finished inst.Out; try inst.Arg.
                arg = false;
                pcΔ1 = inst.val.Arg;
                goto CheckAndLoop;
            } else {
                b.push(re, pcΔ1, posΔ1, true);
                pcΔ1 = inst.val.Out;
                goto CheckAndLoop;
            }
        }
        else if (exprᴛ1 == syntax.InstAltMatch) {
            var exprᴛ2 = re.prog.Inst[(~inst).Out].Op;
            if (exprᴛ2 == syntax.InstRune || exprᴛ2 == syntax.InstRune1 || exprᴛ2 == syntax.InstRuneAny || exprᴛ2 == syntax.InstRuneAnyNotNL) {
                b.push(re, // One opcode consumes runes; the other leads to match.
 // inst.Arg is the match.
 (~inst).Arg, posΔ1, false);
                pcΔ1 = inst.val.Arg;
                posΔ1 = b.end;
                goto CheckAndLoop;
            }

            b.push(re, // inst.Out is the match - non-greedy
 (~inst).Out, b.end, false);
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstRune) {
            var (r, width) = i.step(posΔ1);
            if (!inst.MatchRune(r)) {
                continue;
            }
            posΔ1 += width;
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstRune1) {
            var (r, width) = i.step(posΔ1);
            if (r != (~inst).Rune[0]) {
                continue;
            }
            posΔ1 += width;
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstRuneAnyNotNL) {
            var (r, width) = i.step(posΔ1);
            if (r == (rune)'\n' || r == endOfText) {
                continue;
            }
            posΔ1 += width;
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstRuneAny) {
            var (r, width) = i.step(posΔ1);
            if (r == endOfText) {
                continue;
            }
            posΔ1 += width;
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstCapture) {
            if (arg){
                // Finished inst.Out; restore the old value.
                b.cap[(~inst).Arg] = posΔ1;
                continue;
            } else {
                if ((~inst).Arg < ((uint32)len(b.cap))) {
                    // Capture pos to register, but save old value.
                    b.push(re, pcΔ1, b.cap[(~inst).Arg], true);
                    // come back when we're done.
                    b.cap[(~inst).Arg] = posΔ1;
                }
                pcΔ1 = inst.val.Out;
                goto CheckAndLoop;
            }
        }
        else if (exprᴛ1 == syntax.InstEmptyWidth) {
            var flag = i.context(posΔ1);
            if (!flag.match(((syntax.EmptyOp)(~inst).Arg))) {
                continue;
            }
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstNop) {
            pcΔ1 = inst.val.Out;
            goto CheckAndLoop;
        }
        else if (exprᴛ1 == syntax.InstMatch) {
            if (len(b.cap) == 0) {
                // We found a match. If the caller doesn't care
                // where the match is, no point going further.
                return true;
            }
            if (len(b.cap) > 1) {
                // Record best match so far.
                // Only need to check end point, because this entire
                // call is only considering one start position.
                b.cap[1] = posΔ1;
            }
            {
                nint old = b.matchcap[1]; if (old == -1 || (longest && posΔ1 > 0 && posΔ1 > old)) {
                    copy(b.matchcap, b.cap);
                }
            }
            if (!longest) {
                // If going for first match, we're done.
                return true;
            }
            if (posΔ1 == b.end) {
                // If we used the entire text, no longer match is possible.
                return true;
            }
            continue;
        }

    }
    // Otherwise, continue on in hope of a longer match.
    return longest && len(b.matchcap) > 1 && b.matchcap[1] >= 0;
}

// backtrack runs a backtracking search of prog on the input starting at pos.
[GoRecv] internal static slice<nint> backtrack(this ref Regexp re, slice<byte> ib, @string @is, nint pos, nint ncap, slice<nint> dstCap) {
    var startCond = re.cond;
    if (startCond == ^((syntax.EmptyOp)0)) {
        // impossible
        return default!;
    }
    if ((syntax.EmptyOp)(startCond & syntax.EmptyBeginText) != 0 && pos != 0) {
        // Anchored match, past beginning of text.
        return default!;
    }
    var b = newBitState();
    var (i, end) = (~b).inputs.init(default!, ib, @is);
    b.reset(re.prog, end, ncap);
    // Anchored search must start at the beginning of the input
    if ((syntax.EmptyOp)(startCond & syntax.EmptyBeginText) != 0){
        if (len((~b).cap) > 0) {
            (~b).cap[0] = pos;
        }
        if (!re.tryBacktrack(b, i, ((uint32)re.prog.Start), pos)) {
            freeBitState(b);
            return default!;
        }
    } else {
        // Unanchored search, starting from each possible text position.
        // Notice that we have to try the empty string at the end of
        // the text, so the loop condition is pos <= end, not pos < end.
        // This looks like it's quadratic in the size of the text,
        // but we are not clearing visited between calls to TrySearch,
        // so no work is duplicated and it ends up still being linear.
        nint width = -1;
        for (; pos <= end && width != 0; pos += width) {
            if (len(re.prefix) > 0) {
                // Match requires literal prefix; fast search for it.
                nint advance = i.index(re, pos);
                if (advance < 0) {
                    freeBitState(b);
                    return default!;
                }
                pos += advance;
            }
            if (len((~b).cap) > 0) {
                (~b).cap[0] = pos;
            }
            if (re.tryBacktrack(b, i, ((uint32)re.prog.Start), pos)) {
                // Match must be leftmost; done.
                goto Match;
            }
            (_, width) = i.step(pos);
        }
        freeBitState(b);
        return default!;
    }
Match:
    dstCap = append(dstCap, (~b).matchcap.ꓸꓸꓸ);
    freeBitState(b);
    return dstCap;
}

} // end regexp_package
