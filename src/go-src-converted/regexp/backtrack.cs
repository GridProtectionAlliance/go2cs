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

// package regexp -- go2cs converted at 2020 August 29 08:23:45 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\backtrack.go
using syntax = go.regexp.syntax_package;
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

        private static readonly long visitedBits = 32L;
        private static readonly long maxBacktrackProg = 500L; // len(prog.Inst) <= max
        private static readonly long maxBacktrackVector = 256L * 1024L; // bit vector size <= max (bits)

        // bitState holds state for the backtracker.
        private partial struct bitState
        {
            public ptr<syntax.Prog> prog;
            public long end;
            public slice<long> cap;
            public slice<job> jobs;
            public slice<uint> visited;
        }

        private static ref bitState notBacktrack = null;

        // maxBitStateLen returns the maximum length of a string to search with
        // the backtracker using prog.
        private static long maxBitStateLen(ref syntax.Prog prog)
        {
            if (!shouldBacktrack(prog))
            {
                return 0L;
            }
            return maxBacktrackVector / len(prog.Inst);
        }

        // newBitState returns a new bitState for the given prog,
        // or notBacktrack if the size of the prog exceeds the maximum size that
        // the backtracker will be run for.
        private static ref bitState newBitState(ref syntax.Prog prog)
        {
            if (!shouldBacktrack(prog))
            {
                return notBacktrack;
            }
            return ref new bitState(prog:prog,);
        }

        // shouldBacktrack reports whether the program is too
        // long for the backtracker to run.
        private static bool shouldBacktrack(ref syntax.Prog prog)
        {
            return len(prog.Inst) <= maxBacktrackProg;
        }

        // reset resets the state of the backtracker.
        // end is the end position in the input.
        // ncap is the number of captures.
        private static void reset(this ref bitState b, long end, long ncap)
        {
            b.end = end;

            if (cap(b.jobs) == 0L)
            {
                b.jobs = make_slice<job>(0L, 256L);
            }
            else
            {
                b.jobs = b.jobs[..0L];
            }
            var visitedSize = (len(b.prog.Inst) * (end + 1L) + visitedBits - 1L) / visitedBits;
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

        }

        // shouldVisit reports whether the combination of (pc, pos) has not
        // been visited yet.
        private static bool shouldVisit(this ref bitState b, uint pc, long pos)
        {
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
        private static void push(this ref bitState b, uint pc, long pos, bool arg)
        { 
            // Only check shouldVisit when arg is false.
            // When arg is true, we are continuing a previous visit.
            if (b.prog.Inst[pc].Op != syntax.InstFail && (arg || b.shouldVisit(pc, pos)))
            {
                b.jobs = append(b.jobs, new job(pc:pc,arg:arg,pos:pos));
            }
        }

        // tryBacktrack runs a backtracking search starting at pos.
        private static bool tryBacktrack(this ref machine _m, ref bitState _b, input i, uint pc, long pos) => func(_m, _b, (ref machine m, ref bitState b, Defer _, Panic panic, Recover __) =>
        {
            var longest = m.re.longest;
            m.matched = false;

            b.push(pc, pos, false);
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

                var inst = b.prog.Inst[pc];

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
                        b.push(pc, pos, true);
                        pc = inst.Out;
                        goto CheckAndLoop;
                    }
                else if (inst.Op == syntax.InstAltMatch) 
                    // One opcode consumes runes; the other leads to match.

                    if (b.prog.Inst[inst.Out].Op == syntax.InstRune || b.prog.Inst[inst.Out].Op == syntax.InstRune1 || b.prog.Inst[inst.Out].Op == syntax.InstRuneAny || b.prog.Inst[inst.Out].Op == syntax.InstRuneAnyNotNL) 
                        // inst.Arg is the match.
                        b.push(inst.Arg, pos, false);
                        pc = inst.Arg;
                        pos = b.end;
                        goto CheckAndLoop;
                    // inst.Out is the match - non-greedy
                    b.push(inst.Out, b.end, false);
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
                        if (0L <= inst.Arg && inst.Arg < uint32(len(b.cap)))
                        { 
                            // Capture pos to register, but save old value.
                            b.push(pc, b.cap[inst.Arg], true); // come back when we're done.
                            b.cap[inst.Arg] = pos;
                        }
                        pc = inst.Out;
                        goto CheckAndLoop;
                    }
                else if (inst.Op == syntax.InstEmptyWidth) 
                    if (syntax.EmptyOp(inst.Arg) & ~i.context(pos) != 0L)
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
                        m.matched = true;
                        return m.matched;
                    } 

                    // Record best match so far.
                    // Only need to check end point, because this entire
                    // call is only considering one start position.
                    if (len(b.cap) > 1L)
                    {
                        b.cap[1L] = pos;
                    }
                    if (!m.matched || (longest && pos > 0L && pos > m.matchcap[1L]))
                    {
                        copy(m.matchcap, b.cap);
                    }
                    m.matched = true; 

                    // If going for first match, we're done.
                    if (!longest)
                    {
                        return m.matched;
                    } 

                    // If we used the entire text, no longer match is possible.
                    if (pos == b.end)
                    {
                        return m.matched;
                    } 

                    // Otherwise, continue on in hope of a longer match.
                    continue;
                else 
                    panic("bad inst");
                            }


            return m.matched;
        });

        // backtrack runs a backtracking search of prog on the input starting at pos.
        private static bool backtrack(this ref machine _m, input i, long pos, long end, long ncap) => func(_m, (ref machine m, Defer _, Panic panic, Recover __) =>
        {
            if (!i.canCheckPrefix())
            {
                panic("backtrack called for a RuneReader");
            }
            var startCond = m.re.cond;
            if (startCond == ~syntax.EmptyOp(0L))
            { // impossible
                return false;
            }
            if (startCond & syntax.EmptyBeginText != 0L && pos != 0L)
            { 
                // Anchored match, past beginning of text.
                return false;
            }
            var b = m.b;
            b.reset(end, ncap);

            m.matchcap = m.matchcap[..ncap];
            foreach (var (i) in m.matchcap)
            {
                m.matchcap[i] = -1L;
            } 

            // Anchored search must start at the beginning of the input
            if (startCond & syntax.EmptyBeginText != 0L)
            {
                if (len(b.cap) > 0L)
                {
                    b.cap[0L] = pos;
                }
                return m.tryBacktrack(b, i, uint32(m.p.Start), pos);
            } 

            // Unanchored search, starting from each possible text position.
            // Notice that we have to try the empty string at the end of
            // the text, so the loop condition is pos <= end, not pos < end.
            // This looks like it's quadratic in the size of the text,
            // but we are not clearing visited between calls to TrySearch,
            // so no work is duplicated and it ends up still being linear.
            long width = -1L;
            while (pos <= end && width != 0L)
            {
                if (len(m.re.prefix) > 0L)
                { 
                    // Match requires literal prefix; fast search for it.
                    var advance = i.index(m.re, pos);
                    if (advance < 0L)
                    {
                        return false;
                pos += width;
                    }
                    pos += advance;
                }
                if (len(b.cap) > 0L)
                {
                    b.cap[0L] = pos;
                }
                if (m.tryBacktrack(b, i, uint32(m.p.Start), pos))
                { 
                    // Match must be leftmost; done.
                    return true;
                }
                _, width = i.step(pos);
            }

            return false;
        });
    }
}
