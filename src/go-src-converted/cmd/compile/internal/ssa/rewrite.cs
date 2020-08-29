// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:55:04 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewrite.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static void applyRewrite(ref Func f, blockRewriter rb, valueRewriter rv)
        { 
            // repeat rewrites until we find no more rewrites
            while (true)
            {
                var change = false;
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in f.Blocks)
                    {
                        b = __b;
                        if (b.Control != null && b.Control.Op == OpCopy)
                        {
                            while (b.Control.Op == OpCopy)
                            {
                                b.SetControl(b.Control.Args[0L]);
                            }
                        }
                        if (rb(b))
                        {
                            change = true;
                        }
                        {
                            var v__prev3 = v;

                            foreach (var (_, __v) in b.Values)
                            {
                                v = __v;
                                change = phielimValue(v) || change; 

                                // Eliminate copy inputs.
                                // If any copy input becomes unused, mark it
                                // as invalid and discard its argument. Repeat
                                // recursively on the discarded argument.
                                // This phase helps remove phantom "dead copy" uses
                                // of a value so that a x.Uses==1 rule condition
                                // fires reliably.
                                {
                                    var i__prev4 = i;

                                    foreach (var (__i, __a) in v.Args)
                                    {
                                        i = __i;
                                        a = __a;
                                        if (a.Op != OpCopy)
                                        {
                                            continue;
                                        }
                                        v.SetArg(i, copySource(a));
                                        change = true;
                                        while (a.Uses == 0L)
                                        {
                                            var b = a.Args[0L];
                                            a.reset(OpInvalid);
                                            a = b;
                                        }
                                    }
                                    i = i__prev4;
                                }

                                if (rv(v))
                                {
                                    change = true;
                                }
                            }
                            v = v__prev3;
                        }

                    }
                    b = b__prev2;
                }

                if (!change)
                {
                    break;
                }
            } 
            // remove clobbered values
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    long j = 0L;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in b.Values)
                        {
                            i = __i;
                            v = __v;
                            if (v.Op == OpInvalid)
                            {
                                f.freeValue(v);
                                continue;
                            }
                            if (i != j)
                            {
                                b.Values[j] = v;
                            }
                            j++;
                        }
                        i = i__prev2;
                        v = v__prev2;
                    }

                    if (j != len(b.Values))
                    {
                        var tail = b.Values[j..];
                        {
                            long j__prev2 = j;

                            foreach (var (__j) in tail)
                            {
                                j = __j;
                                tail[j] = null;
                            }
                            j = j__prev2;
                        }

                        b.Values = b.Values[..j];
                    }
                }
                b = b__prev1;
            }

        }

        // Common functions called from rewriting rules

        private static bool is64BitFloat(ref types.Type t)
        {
            return t.Size() == 8L && t.IsFloat();
        }

        private static bool is32BitFloat(ref types.Type t)
        {
            return t.Size() == 4L && t.IsFloat();
        }

        private static bool is64BitInt(ref types.Type t)
        {
            return t.Size() == 8L && t.IsInteger();
        }

        private static bool is32BitInt(ref types.Type t)
        {
            return t.Size() == 4L && t.IsInteger();
        }

        private static bool is16BitInt(ref types.Type t)
        {
            return t.Size() == 2L && t.IsInteger();
        }

        private static bool is8BitInt(ref types.Type t)
        {
            return t.Size() == 1L && t.IsInteger();
        }

        private static bool isPtr(ref types.Type t)
        {
            return t.IsPtrShaped();
        }

        private static bool isSigned(ref types.Type t)
        {
            return t.IsSigned();
        }

        // mergeSym merges two symbolic offsets. There is no real merging of
        // offsets, we just pick the non-nil one.
        private static void mergeSym(object x, object y) => func((_, panic, __) =>
        {
            if (x == null)
            {
                return y;
            }
            if (y == null)
            {
                return x;
            }
            panic(fmt.Sprintf("mergeSym with two non-nil syms %s %s", x, y));
        });
        private static bool canMergeSym(object x, object y)
        {
            return x == null || y == null;
        }

        // canMergeLoad reports whether the load can be merged into target without
        // invalidating the schedule.
        // It also checks that the other non-load argument x is something we
        // are ok with clobbering (all our current load+op instructions clobber
        // their input register).
        private static bool canMergeLoad(ref Value target, ref Value load, ref Value x)
        {
            if (target.Block.ID != load.Block.ID)
            { 
                // If the load is in a different block do not merge it.
                return false;
            } 

            // We can't merge the load into the target if the load
            // has more than one use.
            if (load.Uses != 1L)
            {
                return false;
            } 

            // The register containing x is going to get clobbered.
            // Don't merge if we still need the value of x.
            // We don't have liveness information here, but we can
            // approximate x dying with:
            //  1) target is x's only use.
            //  2) target is not in a deeper loop than x.
            if (x.Uses != 1L)
            {
                return false;
            }
            var loopnest = x.Block.Func.loopnest();
            loopnest.calculateDepths();
            if (loopnest.depth(target.Block.ID) > loopnest.depth(x.Block.ID))
            {
                return false;
            }
            var mem = load.MemoryArg(); 

            // We need the load's memory arg to still be alive at target. That
            // can't be the case if one of target's args depends on a memory
            // state that is a successor of load's memory arg.
            //
            // For example, it would be invalid to merge load into target in
            // the following situation because newmem has killed oldmem
            // before target is reached:
            //     load = read ... oldmem
            //   newmem = write ... oldmem
            //     arg0 = read ... newmem
            //   target = add arg0 load
            //
            // If the argument comes from a different block then we can exclude
            // it immediately because it must dominate load (which is in the
            // same block as target).
            slice<ref Value> args = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in target.Args)
                {
                    a = __a;
                    if (a != load && a.Block.ID == target.Block.ID)
                    {
                        args = append(args, a);
                    }
                } 

                // memPreds contains memory states known to be predecessors of load's
                // memory state. It is lazily initialized.

                a = a__prev1;
            }

            map<ref Value, bool> memPreds = default;
search:

            {
                long i__prev1 = i;

                for (long i = 0L; len(args) > 0L; i++)
                {
                    const long limit = 100L;

                    if (i >= limit)
                    { 
                        // Give up if we have done a lot of iterations.
                        return false;
                    }
                    var v = args[len(args) - 1L];
                    args = args[..len(args) - 1L];
                    if (target.Block.ID != v.Block.ID)
                    { 
                        // Since target and load are in the same block
                        // we can stop searching when we leave the block.
                        _continuesearch = true;
                        break;
                    }
                    if (v.Op == OpPhi)
                    { 
                        // A Phi implies we have reached the top of the block.
                        // The memory phi, if it exists, is always
                        // the first logical store in the block.
                        _continuesearch = true;
                        break;
                    }
                    if (v.Type.IsTuple() && v.Type.FieldType(1L).IsMemory())
                    { 
                        // We could handle this situation however it is likely
                        // to be very rare.
                        return false;
                    }
                    if (v.Type.IsMemory())
                    {
                        if (memPreds == null)
                        { 
                            // Initialise a map containing memory states
                            // known to be predecessors of load's memory
                            // state.
                            memPreds = make_map<ref Value, bool>();
                            var m = mem;
                            const long limit = 50L;

                            {
                                long i__prev2 = i;

                                for (i = 0L; i < limit; i++)
                                {
                                    if (m.Op == OpPhi)
                                    { 
                                        // The memory phi, if it exists, is always
                                        // the first logical store in the block.
                                        break;
                                    }
                                    if (m.Block.ID != target.Block.ID)
                                    {
                                        break;
                                    }
                                    if (!m.Type.IsMemory())
                                    {
                                        break;
                                    }
                                    memPreds[m] = true;
                                    if (len(m.Args) == 0L)
                                    {
                                        break;
                                    }
                                    m = m.MemoryArg();
                                }


                                i = i__prev2;
                            }
                        } 

                        // We can merge if v is a predecessor of mem.
                        //
                        // For example, we can merge load into target in the
                        // following scenario:
                        //      x = read ... v
                        //    mem = write ... v
                        //   load = read ... mem
                        // target = add x load
                        if (memPreds[v])
                        {
                            _continuesearch = true;
                            break;
                        }
                        return false;
                    }
                    if (len(v.Args) > 0L && v.Args[len(v.Args) - 1L] == mem)
                    { 
                        // If v takes mem as an input then we know mem
                        // is valid at this point.
                        _continuesearch = true;
                        break;
                    }
                    {
                        var a__prev2 = a;

                        foreach (var (_, __a) in v.Args)
                        {
                            a = __a;
                            if (target.Block.ID == a.Block.ID)
                            {
                                args = append(args, a);
                            }
                        }

                        a = a__prev2;
                    }

                }


                i = i__prev1;
            }
            return true;
        }

        // isSameSym returns whether sym is the same as the given named symbol
        private static bool isSameSym(object sym, @string name)
        {
            fmt.Stringer (s, ok) = sym._<fmt.Stringer>();
            return ok && s.String() == name;
        }

        // nlz returns the number of leading zeros.
        private static long nlz(long x)
        { 
            // log2(0) == 1, so nlz(0) == 64
            return 63L - log2(x);
        }

        // ntz returns the number of trailing zeros.
        private static long ntz(long x)
        {
            return 64L - nlz(~x & (x - 1L));
        }

        private static bool oneBit(long x)
        {
            return nlz(x) + ntz(x) == 63L;
        }

        // nlo returns the number of leading ones.
        private static long nlo(long x)
        {
            return nlz(~x);
        }

        // nto returns the number of trailing ones.
        private static long nto(long x)
        {
            return ntz(~x);
        }

        // log2 returns logarithm in base 2 of uint64(n), with log2(0) = -1.
        // Rounds down.
        private static long log2(long n)
        {
            l = -1L;
            var x = uint64(n);
            while (x >= 0x8000UL)
            {
                l += 16L;
                x >>= 16L;
            }

            if (x >= 0x80UL)
            {
                x >>= 8L;
                l += 8L;
            }
            if (x >= 0x8UL)
            {
                x >>= 4L;
                l += 4L;
            }
            if (x >= 0x2UL)
            {
                x >>= 2L;
                l += 2L;
            }
            if (x >= 0x1UL)
            {
                l++;
            }
            return;
        }

        // isPowerOfTwo reports whether n is a power of 2.
        private static bool isPowerOfTwo(long n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }

        // is32Bit reports whether n can be represented as a signed 32 bit integer.
        private static bool is32Bit(long n)
        {
            return n == int64(int32(n));
        }

        // is16Bit reports whether n can be represented as a signed 16 bit integer.
        private static bool is16Bit(long n)
        {
            return n == int64(int16(n));
        }

        // isU12Bit reports whether n can be represented as an unsigned 12 bit integer.
        private static bool isU12Bit(long n)
        {
            return 0L <= n && n < (1L << (int)(12L));
        }

        // isU16Bit reports whether n can be represented as an unsigned 16 bit integer.
        private static bool isU16Bit(long n)
        {
            return n == int64(uint16(n));
        }

        // isU32Bit reports whether n can be represented as an unsigned 32 bit integer.
        private static bool isU32Bit(long n)
        {
            return n == int64(uint32(n));
        }

        // is20Bit reports whether n can be represented as a signed 20 bit integer.
        private static bool is20Bit(long n)
        {
            return -(1L << (int)(19L)) <= n && n < (1L << (int)(19L));
        }

        // b2i translates a boolean value to 0 or 1 for assigning to auxInt.
        private static long b2i(bool b)
        {
            if (b)
            {
                return 1L;
            }
            return 0L;
        }

        // i2f is used in rules for converting from an AuxInt to a float.
        private static double i2f(long i)
        {
            return math.Float64frombits(uint64(i));
        }

        // i2f32 is used in rules for converting from an AuxInt to a float32.
        private static float i2f32(long i)
        {
            return float32(math.Float64frombits(uint64(i)));
        }

        // f2i is used in the rules for storing a float in AuxInt.
        private static long f2i(double f)
        {
            return int64(math.Float64bits(f));
        }

        // uaddOvf returns true if unsigned a+b would overflow.
        private static bool uaddOvf(long a, long b)
        {
            return uint64(a) + uint64(b) < uint64(a);
        }

        // de-virtualize an InterCall
        // 'sym' is the symbol for the itab
        private static ref obj.LSym devirt(ref Value v, object sym, long offset)
        {
            var f = v.Block.Func;
            ref obj.LSym (n, ok) = sym._<ref obj.LSym>();
            if (!ok)
            {
                return null;
            }
            var lsym = f.fe.DerefItab(n, offset);
            if (f.pass.debug > 0L)
            {
                if (lsym != null)
                {
                    f.Warnl(v.Pos, "de-virtualizing call");
                }
                else
                {
                    f.Warnl(v.Pos, "couldn't de-virtualize call");
                }
            }
            return lsym;
        }

        // isSamePtr reports whether p1 and p2 point to the same address.
        private static bool isSamePtr(ref Value p1, ref Value p2)
        {
            if (p1 == p2)
            {
                return true;
            }
            if (p1.Op != p2.Op)
            {
                return false;
            }

            if (p1.Op == OpOffPtr) 
                return p1.AuxInt == p2.AuxInt && isSamePtr(p1.Args[0L], p2.Args[0L]);
            else if (p1.Op == OpAddr) 
                // OpAddr's 0th arg is either OpSP or OpSB, which means that it is uniquely identified by its Op.
                // Checking for value equality only works after [z]cse has run.
                return p1.Aux == p2.Aux && p1.Args[0L].Op == p2.Args[0L].Op;
            else if (p1.Op == OpAddPtr) 
                return p1.Args[1L] == p2.Args[1L] && isSamePtr(p1.Args[0L], p2.Args[0L]);
                        return false;
        }

        // moveSize returns the number of bytes an aligned MOV instruction moves
        private static long moveSize(long align, ref Config c)
        {

            if (align % 8L == 0L && c.PtrSize == 8L) 
                return 8L;
            else if (align % 4L == 0L) 
                return 4L;
            else if (align % 2L == 0L) 
                return 2L;
                        return 1L;
        }

        // mergePoint finds a block among a's blocks which dominates b and is itself
        // dominated by all of a's blocks. Returns nil if it can't find one.
        // Might return nil even if one does exist.
        private static ref Block mergePoint(ref Block b, params ptr<Value>[] a)
        {
            a = a.Clone();
 
            // Walk backward from b looking for one of the a's blocks.

            // Max distance
            long d = 100L;

            while (d > 0L)
            {
                {
                    var x__prev2 = x;

                    foreach (var (_, __x) in a)
                    {
                        x = __x;
                        if (b == x.Block)
                        {
                            goto found;
                        }
                    }

                    x = x__prev2;
                }

                if (len(b.Preds) > 1L)
                { 
                    // Don't know which way to go back. Abort.
                    return null;
                }
                b = b.Preds[0L].b;
                d--;
            }

            return null; // too far away
found: 

            // Keep going, counting the other a's that we find. They must all dominate r.
            var r = b; 

            // Keep going, counting the other a's that we find. They must all dominate r.
            long na = 0L;
            while (d > 0L)
            {
                {
                    var x__prev2 = x;

                    foreach (var (_, __x) in a)
                    {
                        x = __x;
                        if (b == x.Block)
                        {
                            na++;
                        }
                    }

                    x = x__prev2;
                }

                if (na == len(a))
                { 
                    // Found all of a in a backwards walk. We can return r.
                    return r;
                }
                if (len(b.Preds) > 1L)
                {
                    return null;
                }
                b = b.Preds[0L].b;
                d--;

            }

            return null; // too far away
        }

        // clobber invalidates v.  Returns true.
        // clobber is used by rewrite rules to:
        //   A) make sure v is really dead and never used again.
        //   B) decrement use counts of v's args.
        private static bool clobber(ref Value v)
        {
            v.reset(OpInvalid); 
            // Note: leave v.Block intact.  The Block field is used after clobber.
            return true;
        }

        // noteRule is an easy way to track if a rule is matched when writing
        // new ones.  Make the rule of interest also conditional on
        //     noteRule("note to self: rule of interest matched")
        // and that message will print when the rule matches.
        private static bool noteRule(@string s)
        {
            fmt.Println(s);
            return true;
        }

        // warnRule generates a compiler debug output with string s when
        // cond is true and the rule is fired.
        private static bool warnRule(bool cond, ref Value v, @string s)
        {
            if (cond)
            {
                v.Block.Func.Warnl(v.Pos, s);
            }
            return true;
        }

        // logRule logs the use of the rule s. This will only be enabled if
        // rewrite rules were generated with the -log option, see gen/rulegen.go.
        private static void logRule(@string s) => func((_, panic, __) =>
        {
            if (ruleFile == null)
            { 
                // Open a log file to write log to. We open in append
                // mode because all.bash runs the compiler lots of times,
                // and we want the concatenation of all of those logs.
                // This means, of course, that users need to rm the old log
                // to get fresh data.
                // TODO: all.bash runs compilers in parallel. Need to synchronize logging somehow?
                var (w, err) = os.OpenFile(filepath.Join(os.Getenv("GOROOT"), "src", "rulelog"), os.O_CREATE | os.O_WRONLY | os.O_APPEND, 0666L);
                if (err != null)
                {
                    panic(err);
                }
                ruleFile = w;
            }
            var (_, err) = fmt.Fprintf(ruleFile, "rewrite %s\n", s);
            if (err != null)
            {
                panic(err);
            }
        });

        private static io.Writer ruleFile = default;

        private static long min(long x, long y)
        {
            if (x < y)
            {
                return x;
            }
            return y;
        }

        private static bool isConstZero(ref Value v)
        {

            if (v.Op == OpConstNil) 
                return true;
            else if (v.Op == OpConst64 || v.Op == OpConst32 || v.Op == OpConst16 || v.Op == OpConst8 || v.Op == OpConstBool || v.Op == OpConst32F || v.Op == OpConst64F) 
                return v.AuxInt == 0L;
                        return false;
        }

        // reciprocalExact64 reports whether 1/c is exactly representable.
        private static bool reciprocalExact64(double c)
        {
            var b = math.Float64bits(c);
            var man = b & (1L << (int)(52L) - 1L);
            if (man != 0L)
            {
                return false; // not a power of 2, denormal, or NaN
            }
            var exp = b >> (int)(52L) & (1L << (int)(11L) - 1L); 
            // exponent bias is 0x3ff.  So taking the reciprocal of a number
            // changes the exponent to 0x7fe-exp.
            switch (exp)
            {
                case 0L: 
                    return false; // ±0
                    break;
                case 0x7ffUL: 
                    return false; // ±inf
                    break;
                case 0x7feUL: 
                    return false; // exponent is not representable
                    break;
                default: 
                    return true;
                    break;
            }
        }

        // reciprocalExact32 reports whether 1/c is exactly representable.
        private static bool reciprocalExact32(float c)
        {
            var b = math.Float32bits(c);
            var man = b & (1L << (int)(23L) - 1L);
            if (man != 0L)
            {
                return false; // not a power of 2, denormal, or NaN
            }
            var exp = b >> (int)(23L) & (1L << (int)(8L) - 1L); 
            // exponent bias is 0x7f.  So taking the reciprocal of a number
            // changes the exponent to 0xfe-exp.
            switch (exp)
            {
                case 0L: 
                    return false; // ±0
                    break;
                case 0xffUL: 
                    return false; // ±inf
                    break;
                case 0xfeUL: 
                    return false; // exponent is not representable
                    break;
                default: 
                    return true;
                    break;
            }
        }

        // check if an immediate can be directly encoded into an ARM's instruction
        private static bool isARMImmRot(uint v)
        {
            for (long i = 0L; i < 16L; i++)
            {
                if (v & ~0xffUL == 0L)
                {
                    return true;
                }
                v = v << (int)(2L) | v >> (int)(30L);
            }


            return false;
        }

        // overlap reports whether the ranges given by the given offset and
        // size pairs overlap.
        private static bool overlap(long offset1, long size1, long offset2, long size2)
        {
            if (offset1 >= offset2 && offset2 + size2 > offset1)
            {
                return true;
            }
            if (offset2 >= offset1 && offset1 + size1 > offset2)
            {
                return true;
            }
            return false;
        }

        // check if value zeroes out upper 32-bit of 64-bit register.
        // depth limits recursion depth. In AMD64.rules 3 is used as limit,
        // because it catches same amount of cases as 4.
        private static bool zeroUpper32Bits(ref Value x, long depth)
        {

            if (x.Op == OpAMD64MOVLconst || x.Op == OpAMD64MOVLload || x.Op == OpAMD64MOVLQZX || x.Op == OpAMD64MOVLloadidx1 || x.Op == OpAMD64MOVWload || x.Op == OpAMD64MOVWloadidx1 || x.Op == OpAMD64MOVBload || x.Op == OpAMD64MOVBloadidx1 || x.Op == OpAMD64MOVLloadidx4 || x.Op == OpAMD64ADDLmem || x.Op == OpAMD64SUBLmem || x.Op == OpAMD64ANDLmem || x.Op == OpAMD64ORLmem || x.Op == OpAMD64XORLmem || x.Op == OpAMD64CVTTSD2SL || x.Op == OpAMD64ADDL || x.Op == OpAMD64ADDLconst || x.Op == OpAMD64SUBL || x.Op == OpAMD64SUBLconst || x.Op == OpAMD64ANDL || x.Op == OpAMD64ANDLconst || x.Op == OpAMD64ORL || x.Op == OpAMD64ORLconst || x.Op == OpAMD64XORL || x.Op == OpAMD64XORLconst || x.Op == OpAMD64NEGL || x.Op == OpAMD64NOTL) 
                return true;
            else if (x.Op == OpArg) 
                return x.Type.Width == 4L;
            else if (x.Op == OpSelect0 || x.Op == OpSelect1) 
                // Disabled for now. See issue 23305.
                // TODO: we could look into the arg of the Select to decide.
                return false;
            else if (x.Op == OpPhi) 
                // Phis can use each-other as an arguments, instead of tracking visited values,
                // just limit recursion depth.
                if (depth <= 0L)
                {
                    return false;
                }
                foreach (var (i) in x.Args)
                {
                    if (!zeroUpper32Bits(x.Args[i], depth - 1L))
                    {
                        return false;
                    }
                }
                return true;
                        return false;
        }

        // inlineablememmovesize reports whether the given arch performs OpMove of the given size
        // faster than memmove and in a safe way when src and dst overlap.
        // This is used as a check for replacing memmove with OpMove.
        private static bool isInlinableMemmoveSize(long sz, ref Config c)
        {
            switch (c.arch)
            {
                case "amd64": 

                case "amd64p32": 
                    return sz <= 16L;
                    break;
                case "386": 

                case "ppc64": 

                case "s390x": 

                case "ppc64le": 
                    return sz <= 8L;
                    break;
                case "arm": 

                case "mips": 

                case "mips64": 

                case "mipsle": 

                case "mips64le": 
                    return sz <= 4L;
                    break;
            }
            return false;
        }
    }
}}}}
