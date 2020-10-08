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

// package regexp -- go2cs converted at 2020 October 08 03:40:57 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\backtrack.go
using syntax = go.regexp.syntax_package;
using sync = go.sync_package;
using static go.builtin;

namespace go
{
    public static partial class regexp_package
    {
        // A job is an entry on the backtracker's job stack. It holds
        // the instruction pc and the position in the input.
        private partial struct job
        {
            public uint pc;
            public bool arg;
            public long pos;
        }

        private static readonly long visitedBits = (long)32L;
        private static readonly long maxBacktrackProg = (long)500L; // len(prog.Inst) <= max
        private static readonly long maxBacktrackVector = (long)256L * 1024L; // bit vector size <= max (bits)

        // bitState holds state for the backtracker.
        private partial struct bitState
        {
            public long end;
            public slice<long> cap;
            public slice<long> matchcap;
            public slice<job> jobs;
            public slice<uint> visited;
            public inputs inputs;
        }

        private static sync.Pool bitStatePool = default;

        private static ptr<bitState> newBitState()
        {
            ptr<bitState> (b, ok) = bitStatePool.Get()._<ptr<bitState>>();
            if (!ok)
            {
                b = @new<bitState>();
            }

            return _addr_b!;

        }

        private static void freeBitState(ptr<bitState> _addr_b)
        {
            ref bitState b = ref _addr_b.val;

            b.inputs.clear();
            bitStatePool.Put(b);
        }

        // maxBitStateLen returns the maximum length of a string to search with
        // the backtracker using prog.
        private static long maxBitStateLen(ptr<syntax.Prog> _addr_prog)
        {
            ref syntax.Prog prog = ref _addr_prog.val;

            if (!shouldBacktrack(_addr_prog))
            {
                return 0L;
            }

            return maxBacktrackVector / len(prog.Inst);

        }

        // shouldBacktrack reports whether the program is too
        // long for the backtracker to run.
        private static bool shouldBacktrack(ptr<syntax.Prog> _addr_prog)
        {
            ref syntax.Prog prog = ref _addr_prog.val;

            return len(prog.Inst) <= maxBacktrackProg;
        }

        // reset resets the state of the backtracker.
        // end is the end position in the input.
        // ncap is the number of captures.
        private static void reset(this ptr<bitState> _addr_b, ptr<syntax.Prog> _addr_prog, long end, long ncap)
        {
            ref bitState b = ref _addr_b.val;
            ref syntax.Prog prog = ref _addr_prog.val;

            b.end = end;

            if (cap(b.jobs) == 0L)
            {
                b.jobs = make_slice<job>(0L, 256L);
            }
            else
            {
                b.jobs = b.jobs[..0L];
            }

            var visitedSize = (len(prog.Inst) * (end + 1L) + visitedBits - 1L) / visitedBits;
            if (cap(b.visited) < visitedSize)
            {
                b.visited = make_slice<uint>(visitedSize, maxBacktrackVector / visitedBits);
            }
            else
            {
                b.visited = b.visited[..visitedSize];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in b.visited)
                    {
                        i = __i;
                        b.visited[i] = 0L;
                    }

                    i = i__prev1;
                }
            }

            if (cap(b.cap) < ncap)
            {
                b.cap = make_slice<long>(ncap);
            }
            else
            {
                b.cap = b.cap[..ncap];
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in b.cap)
                {
                    i = __i;
                    b.cap[i] = -1L;
                }

                i = i__prev1;
            }

            if (cap(b.matchcap) < ncap)
            {
                b.matchcap = make_slice<long>(ncap);
            }
            else
            {
                b.matchcap = b.matchcap[..ncap];
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in b.matchcap)
                {
                    i = __i;
                    b.matchcap[i] = -1L;
                }

                i = i__prev1;
            }
        }

        // shouldVisit reports whether the combination of (pc, pos) has not
        // been visited yet.
        private static bool shouldVisit(this ptr<bitState> _addr_b, uint pc, long pos)
        {
            ref bitState b = ref _addr_b.val;

            var n = uint(int(pc) * (b.end + 1L) + pos);
            if (b.visited[n / visitedBits] & (1L << (int)((n & (visitedBits - 1L)))) != 0L)
            {
                return false;
            }

            b.visited[n / visitedBits] |= 1L << (int)((n & (visitedBits - 1L)));
            return true;

        }

        // push pushes (pc, pos, arg) onto the job stack if it should be
        // visited.
        private static void push(this ptr<bitState> _addr_b, ptr<Regexp> _addr_re, uint pc, long pos, bool arg)
        {
            ref bitState b = ref _addr_b.val;
            ref Regexp re = ref _addr_re.val;
 
            // Only check shouldVisit when arg is false.
            // When arg is true, we are continuing a previous visit.
            if (re.prog.Inst[pc].Op != syntax.InstFail && (arg || b.shouldVisit(pc, pos)))
            {
                b.jobs = append(b.jobs, new job(pc:pc,arg:arg,pos:pos));
            }

        }

        // tryBacktrack runs a backtracking search starting at pos.
        private static bool tryBacktrack(this ptr<Regexp> _addr_re, ptr<bitState> _addr_b, input i, uint pc, long pos) => func((_, panic, __) =>
        {
            ref Regexp re = ref _addr_re.val;
            ref bitState b = ref _addr_b.val;

            var longest = re.longest;

            b.push(re, pc, pos, false);
            while (len(b.jobs) > 0L)
            {
                var l = len(b.jobs) - 1L; 
                // Pop job off the stack.
                var pc = b.jobs[l].pc;
                var pos = b.jobs[l].pos;
                var arg = b.jobs[l].arg;
                b.jobs = b.jobs[..l]; 

                // Optimization: rather than push and pop,
                // code that is going to Push and continue
                // the loop simply updates ip, p, and arg
                // and jumps to CheckAndLoop. We have to
                // do the ShouldVisit check that Push
                // would have, but we avoid the stack
                // manipulation.
                goto Skip;
CheckAndLoop:
                if (!b.shouldVisit(pc, pos))
                {
                    continue;
                }

Skip:

                var inst = re.prog.Inst[pc];

                if (inst.Op == syntax.InstFail) 
                    panic("unexpected InstFail");
                else if (inst.Op == syntax.InstAlt) 
                    // Cannot just
                    //   b.push(inst.Out, pos, false)
                    //   b.push(inst.Arg, pos, false)
                    // If during the processing of inst.Out, we encounter
                    // inst.Arg via another path, we want to process it then.
                    // Pushing it here will inhibit that. Instead, re-push
                    // inst with arg==true as a reminder to push inst.Arg out
                    // later.
                    if (arg)
                    { 
                        // Finished inst.Out; try inst.Arg.
                        arg = false;
                        pc = inst.Arg;
                        goto CheckAndLoop;

                    }
                    else
                    {
                        b.push(re, pc, pos, true);
                        pc = inst.Out;
                        goto CheckAndLoop;
                    }

                else if (inst.Op == syntax.InstAltMatch) 
                    // One opcode consumes runes; the other leads to match.

                    if (re.prog.Inst[inst.Out].Op == syntax.InstRune || re.prog.Inst[inst.Out].Op == syntax.InstRune1 || re.prog.Inst[inst.Out].Op == syntax.InstRuneAny || re.prog.Inst[inst.Out].Op == syntax.InstRuneAnyNotNL) 
                        // inst.Arg is the match.
                        b.push(re, inst.Arg, pos, false);
                        pc = inst.Arg;
                        pos = b.end;
                        goto CheckAndLoop;
                    // inst.Out is the match - non-greedy
                    b.push(re, inst.Out, b.end, false);
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstRune) 
                    var (r, width) = i.step(pos);
                    if (!inst.MatchRune(r))
                    {
                        continue;
                    }

                    pos += width;
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstRune1) 
                    (r, width) = i.step(pos);
                    if (r != inst.Rune[0L])
                    {
                        continue;
                    }

                    pos += width;
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstRuneAnyNotNL) 
                    (r, width) = i.step(pos);
                    if (r == '\n' || r == endOfText)
                    {
                        continue;
                    }

                    pos += width;
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstRuneAny) 
                    (r, width) = i.step(pos);
                    if (r == endOfText)
                    {
                        continue;
                    }

                    pos += width;
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstCapture) 
                    if (arg)
                    { 
                        // Finished inst.Out; restore the old value.
                        b.cap[inst.Arg] = pos;
                        continue;

                    }
                    else
                    {
                        if (inst.Arg < uint32(len(b.cap)))
                        { 
                            // Capture pos to register, but save old value.
                            b.push(re, pc, b.cap[inst.Arg], true); // come back when we're done.
                            b.cap[inst.Arg] = pos;

                        }

                        pc = inst.Out;
                        goto CheckAndLoop;

                    }

                else if (inst.Op == syntax.InstEmptyWidth) 
                    var flag = i.context(pos);
                    if (!flag.match(syntax.EmptyOp(inst.Arg)))
                    {
                        continue;
                    }

                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstNop) 
                    pc = inst.Out;
                    goto CheckAndLoop;
                else if (inst.Op == syntax.InstMatch) 
                    // We found a match. If the caller doesn't care
                    // where the match is, no point going further.
                    if (len(b.cap) == 0L)
                    {
                        return true;
                    } 

                    // Record best match so far.
                    // Only need to check end point, because this entire
                    // call is only considering one start position.
                    if (len(b.cap) > 1L)
                    {
                        b.cap[1L] = pos;
                    }

                    {
                        var old = b.matchcap[1L];

                        if (old == -1L || (longest && pos > 0L && pos > old))
                        {
                            copy(b.matchcap, b.cap);
                        } 

                        // If going for first match, we're done.

                    } 

                    // If going for first match, we're done.
                    if (!longest)
                    {
                        return true;
                    } 

                    // If we used the entire text, no longer match is possible.
                    if (pos == b.end)
                    {
                        return true;
                    } 

                    // Otherwise, continue on in hope of a longer match.
                    continue;
                else 
                    panic("bad inst");
                
            }


            return longest && len(b.matchcap) > 1L && b.matchcap[1L] >= 0L;

        });

        // backtrack runs a backtracking search of prog on the input starting at pos.
        private static slice<long> backtrack(this ptr<Regexp> _addr_re, slice<byte> ib, @string @is, long pos, long ncap, slice<long> dstCap)
        {
            ref Regexp re = ref _addr_re.val;

            var startCond = re.cond;
            if (startCond == ~syntax.EmptyOp(0L))
            { // impossible
                return null;

            }

            if (startCond & syntax.EmptyBeginText != 0L && pos != 0L)
            { 
                // Anchored match, past beginning of text.
                return null;

            }

            var b = newBitState();
            var (i, end) = b.inputs.init(null, ib, is);
            b.reset(re.prog, end, ncap); 

            // Anchored search must start at the beginning of the input
            if (startCond & syntax.EmptyBeginText != 0L)
            {
                if (len(b.cap) > 0L)
                {
                    b.cap[0L] = pos;
                }

                if (!re.tryBacktrack(b, i, uint32(re.prog.Start), pos))
                {
                    freeBitState(_addr_b);
                    return null;
                }

            }
            else
            {
                // Unanchored search, starting from each possible text position.
                // Notice that we have to try the empty string at the end of
                // the text, so the loop condition is pos <= end, not pos < end.
                // This looks like it's quadratic in the size of the text,
                // but we are not clearing visited between calls to TrySearch,
                // so no work is duplicated and it ends up still being linear.
                long width = -1L;
                while (pos <= end && width != 0L)
                {
                    if (len(re.prefix) > 0L)
                    { 
                        // Match requires literal prefix; fast search for it.
                        var advance = i.index(re, pos);
                        if (advance < 0L)
                        {
                            freeBitState(_addr_b);
                            return null;
                    pos += width;
                        }

                        pos += advance;

                    }

                    if (len(b.cap) > 0L)
                    {
                        b.cap[0L] = pos;
                    }

                    if (re.tryBacktrack(b, i, uint32(re.prog.Start), pos))
                    { 
                        // Match must be leftmost; done.
                        goto Match;

                    }

                    _, width = i.step(pos);

                }

                freeBitState(_addr_b);
                return null;

            }

Match:
            dstCap = append(dstCap, b.matchcap);
            freeBitState(_addr_b);
            return dstCap;

        }
    }
}
