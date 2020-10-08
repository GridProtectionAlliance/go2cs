// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package regexp -- go2cs converted at 2020 October 08 03:41:10 UTC
// import "regexp" ==> using regexp = go.regexp_package
// Original source: C:\Go\src\regexp\exec.go
using io = go.io_package;
using syntax = go.regexp.syntax_package;
using sync = go.sync_package;
using static go.builtin;

namespace go
{
    public static partial class regexp_package
    {
        // A queue is a 'sparse array' holding pending threads of execution.
        // See https://research.swtch.com/2008/03/using-uninitialized-memory-for-fun-and.html
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
        // See https://swtch.com/~rsc/regexp/regexp2.html
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
            public queue q0; // two queues for runq, nextq
            public queue q1; // two queues for runq, nextq
            public slice<ptr<thread>> pool; // pool of available threads
            public bool matched; // whether a match was found
            public slice<long> matchcap; // capture information for the match

            public inputs inputs;
        }

        private partial struct inputs
        {
            public inputBytes bytes;
            public inputString @string;
            public inputReader reader;
        }

        private static input newBytes(this ptr<inputs> _addr_i, slice<byte> b)
        {
            ref inputs i = ref _addr_i.val;

            i.bytes.str = b;
            return _addr_i.bytes;
        }

        private static input newString(this ptr<inputs> _addr_i, @string s)
        {
            ref inputs i = ref _addr_i.val;

            i.@string.str = s;
            return _addr_i.@string;
        }

        private static input newReader(this ptr<inputs> _addr_i, io.RuneReader r)
        {
            ref inputs i = ref _addr_i.val;

            i.reader.r = r;
            i.reader.atEOT = false;
            i.reader.pos = 0L;
            return _addr_i.reader;
        }

        private static void clear(this ptr<inputs> _addr_i)
        {
            ref inputs i = ref _addr_i.val;
 
            // We need to clear 1 of these.
            // Avoid the expense of clearing the others (pointer write barrier).
            if (i.bytes.str != null)
            {
                i.bytes.str = null;
            }
            else if (i.reader.r != null)
            {
                i.reader.r = null;
            }
            else
            {
                i.@string.str = "";
            }

        }

        private static (input, long) init(this ptr<inputs> _addr_i, io.RuneReader r, slice<byte> b, @string s)
        {
            input _p0 = default;
            long _p0 = default;
            ref inputs i = ref _addr_i.val;

            if (r != null)
            {
                return (i.newReader(r), 0L);
            }

            if (b != null)
            {
                return (i.newBytes(b), len(b));
            }

            return (i.newString(s), len(s));

        }

        private static void init(this ptr<machine> _addr_m, long ncap)
        {
            ref machine m = ref _addr_m.val;

            foreach (var (_, t) in m.pool)
            {
                t.cap = t.cap[..ncap];
            }
            m.matchcap = m.matchcap[..ncap];

        }

        // alloc allocates a new thread with the given instruction.
        // It uses the free pool if possible.
        private static ptr<thread> alloc(this ptr<machine> _addr_m, ptr<syntax.Inst> _addr_i)
        {
            ref machine m = ref _addr_m.val;
            ref syntax.Inst i = ref _addr_i.val;

            ptr<thread> t;
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
            return _addr_t!;

        }

        // A lazyFlag is a lazily-evaluated syntax.EmptyOp,
        // for checking zero-width flags like ^ $ \A \z \B \b.
        // It records the pair of relevant runes and does not
        // determine the implied flags until absolutely necessary
        // (most of the time, that means never).
        private partial struct lazyFlag // : ulong
        {
        }

        private static lazyFlag newLazyFlag(int r1, int r2)
        {
            return lazyFlag(uint64(r1) << (int)(32L) | uint64(uint32(r2)));
        }

        private static bool match(this lazyFlag f, syntax.EmptyOp op)
        {
            if (op == 0L)
            {
                return true;
            }

            var r1 = rune(f >> (int)(32L));
            if (op & syntax.EmptyBeginLine != 0L)
            {
                if (r1 != '\n' && r1 >= 0L)
                {
                    return false;
                }

                op &= syntax.EmptyBeginLine;

            }

            if (op & syntax.EmptyBeginText != 0L)
            {
                if (r1 >= 0L)
                {
                    return false;
                }

                op &= syntax.EmptyBeginText;

            }

            if (op == 0L)
            {
                return true;
            }

            var r2 = rune(f);
            if (op & syntax.EmptyEndLine != 0L)
            {
                if (r2 != '\n' && r2 >= 0L)
                {
                    return false;
                }

                op &= syntax.EmptyEndLine;

            }

            if (op & syntax.EmptyEndText != 0L)
            {
                if (r2 >= 0L)
                {
                    return false;
                }

                op &= syntax.EmptyEndText;

            }

            if (op == 0L)
            {
                return true;
            }

            if (syntax.IsWordChar(r1) != syntax.IsWordChar(r2))
            {
                op &= syntax.EmptyWordBoundary;
            }
            else
            {
                op &= syntax.EmptyNoWordBoundary;
            }

            return op == 0L;

        }

        // match runs the machine over the input starting at pos.
        // It reports whether a match was found.
        // If so, m.matchcap holds the submatch information.
        private static bool match(this ptr<machine> _addr_m, input i, long pos)
        {
            ref machine m = ref _addr_m.val;

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
            var runq = _addr_m.q0;
            var nextq = _addr_m.q1;
            var r = endOfText;
            var r1 = endOfText;
            long width = 0L;
            long width1 = 0L;
            r, width = i.step(pos);
            if (r != endOfText)
            {
                r1, width1 = i.step(pos + width);
            }

            ref lazyFlag flag = ref heap(out ptr<lazyFlag> _addr_flag);
            if (pos == 0L)
            {
                flag = newLazyFlag(-1L, r);
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

                    m.add(runq, uint32(m.p.Start), pos, m.matchcap, _addr_flag, null);

                }

                flag = newLazyFlag(r, r1);
                m.step(runq, nextq, pos, pos + width, r, _addr_flag);
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
        private static void clear(this ptr<machine> _addr_m, ptr<queue> _addr_q)
        {
            ref machine m = ref _addr_m.val;
            ref queue q = ref _addr_q.val;

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
        private static void step(this ptr<machine> _addr_m, ptr<queue> _addr_runq, ptr<queue> _addr_nextq, long pos, long nextPos, int c, ptr<lazyFlag> _addr_nextCond) => func((_, panic, __) =>
        {
            ref machine m = ref _addr_m.val;
            ref queue runq = ref _addr_runq.val;
            ref queue nextq = ref _addr_nextq.val;
            ref lazyFlag nextCond = ref _addr_nextCond.val;

            var longest = m.re.longest;
            for (long j = 0L; j < len(runq.dense); j++)
            {
                var d = _addr_runq.dense[j];
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
        private static ptr<thread> add(this ptr<machine> _addr_m, ptr<queue> _addr_q, uint pc, long pos, slice<long> cap, ptr<lazyFlag> _addr_cond, ptr<thread> _addr_t) => func((_, panic, __) =>
        {
            ref machine m = ref _addr_m.val;
            ref queue q = ref _addr_q.val;
            ref lazyFlag cond = ref _addr_cond.val;
            ref thread t = ref _addr_t.val;

Again:
            if (pc == 0L)
            {
                return _addr_t!;
            }

            {
                var j__prev1 = j;

                var j = q.sparse[pc];

                if (j < uint32(len(q.dense)) && q.dense[j].pc == pc)
                {
                    return _addr_t!;
                }

                j = j__prev1;

            }


            j = len(q.dense);
            q.dense = q.dense[..j + 1L];
            var d = _addr_q.dense[j];
            d.t = null;
            d.pc = pc;
            q.sparse[pc] = uint32(j);

            var i = _addr_m.p.Inst[pc];

            if (i.Op == syntax.InstFail)             else if (i.Op == syntax.InstAlt || i.Op == syntax.InstAltMatch) 
                t = m.add(q, i.Out, pos, cap, cond, t);
                pc = i.Arg;
                goto Again;
            else if (i.Op == syntax.InstEmptyWidth) 
                if (cond.match(syntax.EmptyOp(i.Arg)))
                {
                    pc = i.Out;
                    goto Again;
                }

            else if (i.Op == syntax.InstNop) 
                pc = i.Out;
                goto Again;
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
                    pc = i.Out;
                    goto Again;
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

                if (len(cap) > 0L && _addr_t.cap[0L] != _addr_cap[0L])
                {
                    copy(t.cap, cap);
                }

                d.t = t;
                t = null;
            else 
                panic("unhandled");
                        return _addr_t!;

        });

        private partial struct onePassMachine
        {
            public inputs inputs;
            public slice<long> matchcap;
        }

        private static sync.Pool onePassPool = default;

        private static ptr<onePassMachine> newOnePassMachine()
        {
            ptr<onePassMachine> (m, ok) = onePassPool.Get()._<ptr<onePassMachine>>();
            if (!ok)
            {
                m = @new<onePassMachine>();
            }

            return _addr_m!;

        }

        private static void freeOnePassMachine(ptr<onePassMachine> _addr_m)
        {
            ref onePassMachine m = ref _addr_m.val;

            m.inputs.clear();
            onePassPool.Put(m);
        }

        // doOnePass implements r.doExecute using the one-pass execution engine.
        private static slice<long> doOnePass(this ptr<Regexp> _addr_re, io.RuneReader ir, slice<byte> ib, @string @is, long pos, long ncap, slice<long> dstCap) => func((_, panic, __) =>
        {
            ref Regexp re = ref _addr_re.val;

            var startCond = re.cond;
            if (startCond == ~syntax.EmptyOp(0L))
            { // impossible
                return null;

            }

            var m = newOnePassMachine();
            if (cap(m.matchcap) < ncap)
            {
                m.matchcap = make_slice<long>(ncap);
            }
            else
            {
                m.matchcap = m.matchcap[..ncap];
            }

            var matched = false;
            {
                var i__prev1 = i;

                foreach (var (__i) in m.matchcap)
                {
                    i = __i;
                    m.matchcap[i] = -1L;
                }

                i = i__prev1;
            }

            var (i, _) = m.inputs.init(ir, ib, is);

            var r = endOfText;
            var r1 = endOfText;
            long width = 0L;
            long width1 = 0L;
            r, width = i.step(pos);
            if (r != endOfText)
            {
                r1, width1 = i.step(pos + width);
            }

            lazyFlag flag = default;
            if (pos == 0L)
            {
                flag = newLazyFlag(-1L, r);
            }
            else
            {
                flag = i.context(pos);
            }

            var pc = re.onepass.Start;
            ref var inst = ref heap(re.onepass.Inst[pc], out ptr<var> _addr_inst); 
            // If there is a simple literal prefix, skip over it.
            if (pos == 0L && flag.match(syntax.EmptyOp(inst.Arg)) && len(re.prefix) > 0L && i.canCheckPrefix())
            { 
                // Match requires literal prefix; fast search for it.
                if (!i.hasPrefix(re))
                {
                    goto Return;
                }

                pos += len(re.prefix);
                r, width = i.step(pos);
                r1, width1 = i.step(pos + width);
                flag = i.context(pos);
                pc = int(re.prefixEnd);

            }

            while (true)
            {
                inst = re.onepass.Inst[pc];
                pc = int(inst.Out);

                if (inst.Op == syntax.InstMatch) 
                    matched = true;
                    if (len(m.matchcap) > 0L)
                    {
                        m.matchcap[0L] = 0L;
                        m.matchcap[1L] = pos;
                    }

                    goto Return;
                else if (inst.Op == syntax.InstRune) 
                    if (!inst.MatchRune(r))
                    {
                        goto Return;
                    }

                else if (inst.Op == syntax.InstRune1) 
                    if (r != inst.Rune[0L])
                    {
                        goto Return;
                    }

                else if (inst.Op == syntax.InstRuneAny)                 else if (inst.Op == syntax.InstRuneAnyNotNL) 
                    if (r == '\n')
                    {
                        goto Return;
                    } 
                    // peek at the input rune to see which branch of the Alt to take
                else if (inst.Op == syntax.InstAlt || inst.Op == syntax.InstAltMatch) 
                    pc = int(onePassNext(_addr_inst, r));
                    continue;
                else if (inst.Op == syntax.InstFail) 
                    goto Return;
                else if (inst.Op == syntax.InstNop) 
                    continue;
                else if (inst.Op == syntax.InstEmptyWidth) 
                    if (!flag.match(syntax.EmptyOp(inst.Arg)))
                    {
                        goto Return;
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

                flag = newLazyFlag(r, r1);
                pos += width;
                r = r1;
                width = width1;
                if (r != endOfText)
                {
                    r1, width1 = i.step(pos + width);
                }

            }


Return:

            if (!matched)
            {
                freeOnePassMachine(_addr_m);
                return null;
            }

            dstCap = append(dstCap, m.matchcap);
            freeOnePassMachine(_addr_m);
            return dstCap;

        });

        // doMatch reports whether either r, b or s match the regexp.
        private static bool doMatch(this ptr<Regexp> _addr_re, io.RuneReader r, slice<byte> b, @string s)
        {
            ref Regexp re = ref _addr_re.val;

            return re.doExecute(r, b, s, 0L, 0L, null) != null;
        }

        // doExecute finds the leftmost match in the input, appends the position
        // of its subexpressions to dstCap and returns dstCap.
        //
        // nil is returned if no matches are found and non-nil if matches are found.
        private static slice<long> doExecute(this ptr<Regexp> _addr_re, io.RuneReader r, slice<byte> b, @string s, long pos, long ncap, slice<long> dstCap)
        {
            ref Regexp re = ref _addr_re.val;

            if (dstCap == null)
            { 
                // Make sure 'return dstCap' is non-nil.
                dstCap = arrayNoInts.slice(-1, 0L, 0L);

            }

            if (r == null && len(b) + len(s) < re.minInputLen)
            {
                return null;
            }

            if (re.onepass != null)
            {
                return re.doOnePass(r, b, s, pos, ncap, dstCap);
            }

            if (r == null && len(b) + len(s) < re.maxBitStateLen)
            {
                return re.backtrack(b, s, pos, ncap, dstCap);
            }

            var m = re.get();
            var (i, _) = m.inputs.init(r, b, s);

            m.init(ncap);
            if (!m.match(i, pos))
            {
                re.put(m);
                return null;
            }

            dstCap = append(dstCap, m.matchcap);
            re.put(m);
            return dstCap;

        }

        // arrayNoInts is returned by doExecute match if nil dstCap is passed
        // to it with ncap=0.
        private static array<long> arrayNoInts = new array<long>(0L);
    }
}
