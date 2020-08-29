// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package regexp -- go2cs converted at 2020 August 29 08:24:03 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\exec.go
using io = go.io_package;
using syntax = go.regexp.syntax_package;
using static go.builtin;

namespace go
{
    public static partial class regexp_package
    {
        // A queue is a 'sparse array' holding pending threads of execution.
        // See http://research.swtch.com/2008/03/using-uninitialized-memory-for-fun-and.html
        private partial struct queue
        {
            public slice<uint> sparse;
            public slice<entry> dense;
        }

        // An entry is an entry on a queue.
        // It holds both the instruction pc and the actual thread.
        // Some queue entries are just place holders so that the machine
        // knows it has considered that pc. Such entries have t == nil.
        private partial struct entry
        {
            public uint pc;
            public ptr<thread> t;
        }

        // A thread is the state of a single path through the machine:
        // an instruction and a corresponding capture array.
        // See http://swtch.com/~rsc/regexp/regexp2.html
        private partial struct thread
        {
            public ptr<syntax.Inst> inst;
            public slice<long> cap;
        }

        // A machine holds all the state during an NFA simulation for p.
        private partial struct machine
        {
            public ptr<Regexp> re; // corresponding Regexp
            public ptr<syntax.Prog> p; // compiled program
            public ptr<onePassProg> op; // compiled onepass program, or notOnePass
            public long maxBitStateLen; // max length of string to search with bitstate
            public ptr<bitState> b; // state for backtracker, allocated lazily
            public queue q0; // two queues for runq, nextq
            public queue q1; // two queues for runq, nextq
            public slice<ref thread> pool; // pool of available threads
            public bool matched; // whether a match was found
            public slice<long> matchcap; // capture information for the match

// cached inputs, to avoid allocation
            public inputBytes inputBytes;
            public inputString inputString;
            public inputReader inputReader;
        }

        private static input newInputBytes(this ref machine m, slice<byte> b)
        {
            m.inputBytes.str = b;
            return ref m.inputBytes;
        }

        private static input newInputString(this ref machine m, @string s)
        {
            m.inputString.str = s;
            return ref m.inputString;
        }

        private static input newInputReader(this ref machine m, io.RuneReader r)
        {
            m.inputReader.r = r;
            m.inputReader.atEOT = false;
            m.inputReader.pos = 0L;
            return ref m.inputReader;
        }

        // progMachine returns a new machine running the prog p.
        private static ref machine progMachine(ref syntax.Prog p, ref onePassProg op)
        {
            machine m = ref new machine(p:p,op:op);
            var n = len(m.p.Inst);
            m.q0 = new queue(make([]uint32,n),make([]entry,0,n));
            m.q1 = new queue(make([]uint32,n),make([]entry,0,n));
            var ncap = p.NumCap;
            if (ncap < 2L)
            {
                ncap = 2L;
            }
            if (op == notOnePass)
            {
                m.maxBitStateLen = maxBitStateLen(p);
            }
            m.matchcap = make_slice<long>(ncap);
            return m;
        }

        private static void init(this ref machine m, long ncap)
        {
            foreach (var (_, t) in m.pool)
            {
                t.cap = t.cap[..ncap];
            }
            m.matchcap = m.matchcap[..ncap];
        }

        // alloc allocates a new thread with the given instruction.
        // It uses the free pool if possible.
        private static ref thread alloc(this ref machine m, ref syntax.Inst i)
        {
            ref thread t = default;
            {
                var n = len(m.pool);

                if (n > 0L)
                {
                    t = m.pool[n - 1L];
                    m.pool = m.pool[..n - 1L];
                }
                else
                {
                    t = @new<thread>();
                    t.cap = make_slice<long>(len(m.matchcap), cap(m.matchcap));
                }

            }
            t.inst = i;
            return t;
        }

        // match runs the machine over the input starting at pos.
        // It reports whether a match was found.
        // If so, m.matchcap holds the submatch information.
        private static bool match(this ref machine m, input i, long pos)
        {
            var startCond = m.re.cond;
            if (startCond == ~syntax.EmptyOp(0L))
            { // impossible
                return false;
            }
            m.matched = false;
            foreach (var (i) in m.matchcap)
            {
                m.matchcap[i] = -1L;
            }
            var runq = ref m.q0;
            var nextq = ref m.q1;
            var r = endOfText;
            var r1 = endOfText;
            long width = 0L;
            long width1 = 0L;
            r, width = i.step(pos);
            if (r != endOfText)
            {
                r1, width1 = i.step(pos + width);
            }
            syntax.EmptyOp flag = default;
            if (pos == 0L)
            {
                flag = syntax.EmptyOpContext(-1L, r);
            }
            else
            {
                flag = i.context(pos);
            }
            while (true)
            {
                if (len(runq.dense) == 0L)
                {
                    if (startCond & syntax.EmptyBeginText != 0L && pos != 0L)
                    { 
                        // Anchored match, past beginning of text.
                        break;
                    }
                    if (m.matched)
                    { 
                        // Have match; finished exploring alternatives.
                        break;
                    }
                    if (len(m.re.prefix) > 0L && r1 != m.re.prefixRune && i.canCheckPrefix())
                    { 
                        // Match requires literal prefix; fast search for it.
                        var advance = i.index(m.re, pos);
                        if (advance < 0L)
                        {
                            break;
                        }
                        pos += advance;
                        r, width = i.step(pos);
                        r1, width1 = i.step(pos + width);
                    }
                }
                if (!m.matched)
                {
                    if (len(m.matchcap) > 0L)
                    {
                        m.matchcap[0L] = pos;
                    }
                    m.add(runq, uint32(m.p.Start), pos, m.matchcap, flag, null);
                }
                flag = syntax.EmptyOpContext(r, r1);
                m.step(runq, nextq, pos, pos + width, r, flag);
                if (width == 0L)
                {
                    break;
                }
                if (len(m.matchcap) == 0L && m.matched)
                { 
                    // Found a match and not paying attention
                    // to where it is, so any match will do.
                    break;
                }
                pos += width;
                r = r1;
                width = width1;
                if (r != endOfText)
                {
                    r1, width1 = i.step(pos + width);
                }
                runq = nextq;
                nextq = runq;
            }

            m.clear(nextq);
            return m.matched;
        }

        // clear frees all threads on the thread queue.
        private static void clear(this ref machine m, ref queue q)
        {
            foreach (var (_, d) in q.dense)
            {
                if (d.t != null)
                {
                    m.pool = append(m.pool, d.t);
                }
            }
            q.dense = q.dense[..0L];
        }

        // step executes one step of the machine, running each of the threads
        // on runq and appending new threads to nextq.
        // The step processes the rune c (which may be endOfText),
        // which starts at position pos and ends at nextPos.
        // nextCond gives the setting for the empty-width flags after c.
        private static void step(this ref machine _m, ref queue _runq, ref queue _nextq, long pos, long nextPos, int c, syntax.EmptyOp nextCond) => func(_m, _runq, _nextq, (ref machine m, ref queue runq, ref queue nextq, Defer _, Panic panic, Recover __) =>
        {
            var longest = m.re.longest;
            for (long j = 0L; j < len(runq.dense); j++)
            {
                var d = ref runq.dense[j];
                var t = d.t;
                if (t == null)
                {
                    continue;
                }
                if (longest && m.matched && len(t.cap) > 0L && m.matchcap[0L] < t.cap[0L])
                {
                    m.pool = append(m.pool, t);
                    continue;
                }
                var i = t.inst;
                var add = false;

                if (i.Op == syntax.InstMatch) 
                    if (len(t.cap) > 0L && (!longest || !m.matched || m.matchcap[1L] < pos))
                    {
                        t.cap[1L] = pos;
                        copy(m.matchcap, t.cap);
                    }
                    if (!longest)
                    { 
                        // First-match mode: cut off all lower-priority threads.
                        {
                            var d__prev2 = d;

                            foreach (var (_, __d) in runq.dense[j + 1L..])
                            {
                                d = __d;
                                if (d.t != null)
                                {
                                    m.pool = append(m.pool, d.t);
                                }
                            }

                            d = d__prev2;
                        }

                        runq.dense = runq.dense[..0L];
                    }
                    m.matched = true;
                else if (i.Op == syntax.InstRune) 
                    add = i.MatchRune(c);
                else if (i.Op == syntax.InstRune1) 
                    add = c == i.Rune[0L];
                else if (i.Op == syntax.InstRuneAny) 
                    add = true;
                else if (i.Op == syntax.InstRuneAnyNotNL) 
                    add = c != '\n';
                else 
                    panic("bad inst");
                                if (add)
                {
                    t = m.add(nextq, i.Out, nextPos, t.cap, nextCond, t);
                }
                if (t != null)
                {
                    m.pool = append(m.pool, t);
                }
            }

            runq.dense = runq.dense[..0L];
        });

        // add adds an entry to q for pc, unless the q already has such an entry.
        // It also recursively adds an entry for all instructions reachable from pc by following
        // empty-width conditions satisfied by cond.  pos gives the current position
        // in the input.
        private static ref thread add(this ref machine _m, ref queue _q, uint pc, long pos, slice<long> cap, syntax.EmptyOp cond, ref thread _t) => func(_m, _q, _t, (ref machine m, ref queue q, ref thread t, Defer _, Panic panic, Recover __) =>
        {
            if (pc == 0L)
            {
                return t;
            }
            {
                var j__prev1 = j;

                var j = q.sparse[pc];

                if (j < uint32(len(q.dense)) && q.dense[j].pc == pc)
                {
                    return t;
                }

                j = j__prev1;

            }

            j = len(q.dense);
            q.dense = q.dense[..j + 1L];
            var d = ref q.dense[j];
            d.t = null;
            d.pc = pc;
            q.sparse[pc] = uint32(j);

            var i = ref m.p.Inst[pc];

            if (i.Op == syntax.InstFail)             else if (i.Op == syntax.InstAlt || i.Op == syntax.InstAltMatch) 
                t = m.add(q, i.Out, pos, cap, cond, t);
                t = m.add(q, i.Arg, pos, cap, cond, t);
            else if (i.Op == syntax.InstEmptyWidth) 
                if (syntax.EmptyOp(i.Arg) & ~cond == 0L)
                {
                    t = m.add(q, i.Out, pos, cap, cond, t);
                }
            else if (i.Op == syntax.InstNop) 
                t = m.add(q, i.Out, pos, cap, cond, t);
            else if (i.Op == syntax.InstCapture) 
                if (int(i.Arg) < len(cap))
                {
                    var opos = cap[i.Arg];
                    cap[i.Arg] = pos;
                    m.add(q, i.Out, pos, cap, cond, null);
                    cap[i.Arg] = opos;
                }
                else
                {
                    t = m.add(q, i.Out, pos, cap, cond, t);
                }
            else if (i.Op == syntax.InstMatch || i.Op == syntax.InstRune || i.Op == syntax.InstRune1 || i.Op == syntax.InstRuneAny || i.Op == syntax.InstRuneAnyNotNL) 
                if (t == null)
                {
                    t = m.alloc(i);
                }
                else
                {
                    t.inst = i;
                }
                if (len(cap) > 0L && ref t.cap[0L] != ref cap[0L])
                {
                    copy(t.cap, cap);
                }
                d.t = t;
                t = null;
            else 
                panic("unhandled");
                        return t;
        });

        // onepass runs the machine over the input starting at pos.
        // It reports whether a match was found.
        // If so, m.matchcap holds the submatch information.
        // ncap is the number of captures.
        private static bool onepass(this ref machine _m, input i, long pos, long ncap) => func(_m, (ref machine m, Defer _, Panic panic, Recover __) =>
        {
            var startCond = m.re.cond;
            if (startCond == ~syntax.EmptyOp(0L))
            { // impossible
                return false;
            }
            m.matched = false;
            m.matchcap = m.matchcap[..ncap];
            foreach (var (i) in m.matchcap)
            {
                m.matchcap[i] = -1L;
            }
            var r = endOfText;
            var r1 = endOfText;
            long width = 0L;
            long width1 = 0L;
            r, width = i.step(pos);
            if (r != endOfText)
            {
                r1, width1 = i.step(pos + width);
            }
            syntax.EmptyOp flag = default;
            if (pos == 0L)
            {
                flag = syntax.EmptyOpContext(-1L, r);
            }
            else
            {
                flag = i.context(pos);
            }
            var pc = m.op.Start;
            var inst = m.op.Inst[pc]; 
            // If there is a simple literal prefix, skip over it.
            if (pos == 0L && syntax.EmptyOp(inst.Arg) & ~flag == 0L && len(m.re.prefix) > 0L && i.canCheckPrefix())
            { 
                // Match requires literal prefix; fast search for it.
                if (!i.hasPrefix(m.re))
                {
                    return m.matched;
                }
                pos += len(m.re.prefix);
                r, width = i.step(pos);
                r1, width1 = i.step(pos + width);
                flag = i.context(pos);
                pc = int(m.re.prefixEnd);
            }
            while (true)
            {
                inst = m.op.Inst[pc];
                pc = int(inst.Out);

                if (inst.Op == syntax.InstMatch) 
                    m.matched = true;
                    if (len(m.matchcap) > 0L)
                    {
                        m.matchcap[0L] = 0L;
                        m.matchcap[1L] = pos;
                    }
                    return m.matched;
                else if (inst.Op == syntax.InstRune) 
                    if (!inst.MatchRune(r))
                    {
                        return m.matched;
                    }
                else if (inst.Op == syntax.InstRune1) 
                    if (r != inst.Rune[0L])
                    {
                        return m.matched;
                    }
                else if (inst.Op == syntax.InstRuneAny)                 else if (inst.Op == syntax.InstRuneAnyNotNL) 
                    if (r == '\n')
                    {
                        return m.matched;
                    } 
                    // peek at the input rune to see which branch of the Alt to take
                else if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
                    pc = int(onePassNext(ref inst, r));
                    continue;
                else if (inst.Op == syntax.InstFail) 
                    return m.matched;
                else if (inst.Op == syntax.InstNop) 
                    continue;
                else if (inst.Op == syntax.InstEmptyWidth) 
                    if (syntax.EmptyOp(inst.Arg) & ~flag != 0L)
                    {
                        return m.matched;
                    }
                    continue;
                else if (inst.Op == syntax.InstCapture) 
                    if (int(inst.Arg) < len(m.matchcap))
                    {
                        m.matchcap[inst.Arg] = pos;
                    }
                    continue;
                else 
                    panic("bad inst");
                                if (width == 0L)
                {
                    break;
                }
                flag = syntax.EmptyOpContext(r, r1);
                pos += width;
                r = r1;
                width = width1;
                if (r != endOfText)
                {
                    r1, width1 = i.step(pos + width);
                }
            }

            return m.matched;
        });

        // doMatch reports whether either r, b or s match the regexp.
        private static bool doMatch(this ref Regexp re, io.RuneReader r, slice<byte> b, @string s)
        {
            return re.doExecute(r, b, s, 0L, 0L, null) != null;
        }

        // doExecute finds the leftmost match in the input, appends the position
        // of its subexpressions to dstCap and returns dstCap.
        //
        // nil is returned if no matches are found and non-nil if matches are found.
        private static slice<long> doExecute(this ref Regexp re, io.RuneReader r, slice<byte> b, @string s, long pos, long ncap, slice<long> dstCap)
        {
            var m = re.get();
            input i = default;
            long size = default;
            if (r != null)
            {
                i = m.newInputReader(r);
            }
            else if (b != null)
            {
                i = m.newInputBytes(b);
                size = len(b);
            }
            else
            {
                i = m.newInputString(s);
                size = len(s);
            }
            if (m.op != notOnePass)
            {
                if (!m.onepass(i, pos, ncap))
                {
                    re.put(m);
                    return null;
                }
            }
            else if (size < m.maxBitStateLen && r == null)
            {
                if (m.b == null)
                {
                    m.b = newBitState(m.p);
                }
                if (!m.backtrack(i, pos, size, ncap))
                {
                    re.put(m);
                    return null;
                }
            }
            else
            {
                m.init(ncap);
                if (!m.match(i, pos))
                {
                    re.put(m);
                    return null;
                }
            }
            dstCap = append(dstCap, m.matchcap);
            if (dstCap == null)
            { 
                // Keep the promise of returning non-nil value on match.
                dstCap = arrayNoInts[..0L];
            }
            re.put(m);
            return dstCap;
        }

        // arrayNoInts is returned by doExecute match if nil dstCap is passed
        // to it with ncap=0.
        private static array<long> arrayNoInts = new array<long>(0L);
    }
}
