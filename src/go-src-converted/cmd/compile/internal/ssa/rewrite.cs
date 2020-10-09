// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:25:49 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewrite.go
using logopt = go.cmd.compile.@internal.logopt_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using s390x = go.cmd.@internal.obj.s390x_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using bits = go.math.bits_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static void applyRewrite(ptr<Func> _addr_f, blockRewriter rb, valueRewriter rv)
        {
            ref Func f = ref _addr_f.val;
 
            // repeat rewrites until we find no more rewrites
            var pendingLines = f.cachedLineStarts; // Holds statement boundaries that need to be moved to a new value/block
            pendingLines.clear();
            var debug = f.pass.debug;
            if (debug > 1L)
            {
                fmt.Printf("%s: rewriting for %s\n", f.pass.name, f.Name);
            }
            while (true)
            {
                var change = false;
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in f.Blocks)
                    {
                        b = __b;
                        ptr<Block> b0;
                        if (debug > 1L)
                        {
                            b0 = @new<Block>();
                            b0.val = b.val;
                            b0.Succs = append(new slice<Edge>(new Edge[] {  }), b.Succs); // make a new copy, not aliasing
                        }
                        {
                            var i__prev3 = i;

                            foreach (var (__i, __c) in b.ControlValues())
                            {
                                i = __i;
                                c = __c;
                                while (c.Op == OpCopy)
                                {
                                    c = c.Args[0L];
                                    b.ReplaceControl(i, c);
                                }

                            }
                            i = i__prev3;
                        }

                        if (rb(b))
                        {
                            change = true;
                            if (debug > 1L)
                            {
                                fmt.Printf("rewriting %s  ->  %s\n", b0.LongString(), b.LongString());
                            }
                        }
                        {
                            var j__prev3 = j;
                            var v__prev3 = v;

                            foreach (var (__j, __v) in b.Values)
                            {
                                j = __j;
                                v = __v;
                                ptr<Value> v0;
                                if (debug > 1L)
                                {
                                    v0 = @new<Value>();
                                    v0.val = v.val;
                                    v0.Args = append(new slice<ptr<Value>>(new ptr<Value>[] {  }), v.Args); // make a new copy, not aliasing
                                }
                                var vchange = phielimValue(v);
                                if (vchange && debug > 1L)
                                {
                                    fmt.Printf("rewriting %s  ->  %s\n", v0.LongString(), v.LongString());
                                }
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
                                        var aa = copySource(a);
                                        v.SetArg(i, aa); 
                                        // If a, a copy, has a line boundary indicator, attempt to find a new value
                                        // to hold it.  The first candidate is the value that will replace a (aa),
                                        // if it shares the same block and line and is eligible.
                                        // The second option is v, which has a as an input.  Because aa is earlier in
                                        // the data flow, it is the better choice.
                                        if (a.Pos.IsStmt() == src.PosIsStmt)
                                        {
                                            if (aa.Block == a.Block && aa.Pos.Line() == a.Pos.Line() && aa.Pos.IsStmt() != src.PosNotStmt)
                                            {
                                                aa.Pos = aa.Pos.WithIsStmt();
                                            }
                                            else if (v.Block == a.Block && v.Pos.Line() == a.Pos.Line() && v.Pos.IsStmt() != src.PosNotStmt)
                                            {
                                                v.Pos = v.Pos.WithIsStmt();
                                            }
                                            else
                                            { 
                                                // Record the lost line and look for a new home after all rewrites are complete.
                                                // TODO: it's possible (in FOR loops, in particular) for statement boundaries for the same
                                                // line to appear in more than one block, but only one block is stored, so if both end
                                                // up here, then one will be lost.
                                                pendingLines.set(a.Pos, int32(a.Block.ID));

                                            }
                                            a.Pos = a.Pos.WithNotStmt();

                                        }
                                        vchange = true;
                                        while (a.Uses == 0L)
                                        {
                                            var b = a.Args[0L];
                                            a.reset(OpInvalid);
                                            a = b;
                                        }

                                    }
                                    i = i__prev4;
                                }

                                if (vchange && debug > 1L)
                                {
                                    fmt.Printf("rewriting %s  ->  %s\n", v0.LongString(), v.LongString());
                                }
                                if (rv(v))
                                {
                                    vchange = true; 
                                    // If value changed to a poor choice for a statement boundary, move the boundary
                                    if (v.Pos.IsStmt() == src.PosIsStmt)
                                    {
                                        {
                                            var k = nextGoodStatementIndex(v, j, b);

                                            if (k != j)
                                            {
                                                v.Pos = v.Pos.WithNotStmt();
                                                b.Values[k].Pos = b.Values[k].Pos.WithIsStmt();
                                            }
                                        }

                                    }
                                }
                                change = change || vchange;
                                if (vchange && debug > 1L)
                                {
                                    fmt.Printf("rewriting %s  ->  %s\n", v0.LongString(), v.LongString());
                                }
                            }
                            j = j__prev3;
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
                            var vl = v.Pos;
                            if (v.Op == OpInvalid)
                            {
                                if (v.Pos.IsStmt() == src.PosIsStmt)
                                {
                                    pendingLines.set(vl, int32(b.ID));
                                }
                                f.freeValue(v);
                                continue;

                            }
                            if (v.Pos.IsStmt() != src.PosNotStmt && pendingLines.get(vl) == int32(b.ID))
                            {
                                pendingLines.remove(vl);
                                v.Pos = v.Pos.WithIsStmt();
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

                    if (pendingLines.get(b.Pos) == int32(b.ID))
                    {
                        b.Pos = b.Pos.WithIsStmt();
                        pendingLines.remove(b.Pos);
                    }
                    b.truncateValues(j);

                }
                b = b__prev1;
            }
        }

        // Common functions called from rewriting rules

        private static bool is64BitFloat(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 8L && t.IsFloat();
        }

        private static bool is32BitFloat(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 4L && t.IsFloat();
        }

        private static bool is64BitInt(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 8L && t.IsInteger();
        }

        private static bool is32BitInt(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 4L && t.IsInteger();
        }

        private static bool is16BitInt(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 2L && t.IsInteger();
        }

        private static bool is8BitInt(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.Size() == 1L && t.IsInteger();
        }

        private static bool isPtr(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t.IsPtrShaped();
        }

        private static bool isSigned(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

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

        private static Sym mergeSymTyped(Sym x, Sym y) => func((_, panic, __) =>
        {
            if (x == null)
            {
                return y;
            }

            if (y == null)
            {
                return x;
            }

            panic(fmt.Sprintf("mergeSym with two non-nil syms %v %v", x, y));

        });

        // canMergeLoadClobber reports whether the load can be merged into target without
        // invalidating the schedule.
        // It also checks that the other non-load argument x is something we
        // are ok with clobbering.
        private static bool canMergeLoadClobber(ptr<Value> _addr_target, ptr<Value> _addr_load, ptr<Value> _addr_x)
        {
            ref Value target = ref _addr_target.val;
            ref Value load = ref _addr_load.val;
            ref Value x = ref _addr_x.val;
 
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

            return canMergeLoad(_addr_target, _addr_load);

        }

        // canMergeLoad reports whether the load can be merged into target without
        // invalidating the schedule.
        private static bool canMergeLoad(ptr<Value> _addr_target, ptr<Value> _addr_load)
        {
            ref Value target = ref _addr_target.val;
            ref Value load = ref _addr_load.val;

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
            slice<ptr<Value>> args = default;
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

            map<ptr<Value>, bool> memPreds = default;
            {
                long i__prev1 = i;

                for (long i = 0L; len(args) > 0L; i++)
                {
                    const long limit = (long)100L;

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
                        continue;

                    }

                    if (v.Op == OpPhi)
                    { 
                        // A Phi implies we have reached the top of the block.
                        // The memory phi, if it exists, is always
                        // the first logical store in the block.
                        continue;

                    }

                    if (v.Type.IsTuple() && v.Type.FieldType(1L).IsMemory())
                    { 
                        // We could handle this situation however it is likely
                        // to be very rare.
                        return false;

                    }

                    if (v.Op.SymEffect() & SymAddr != 0L)
                    { 
                        // This case prevents an operation that calculates the
                        // address of a local variable from being forced to schedule
                        // before its corresponding VarDef.
                        // See issue 28445.
                        //   v1 = LOAD ...
                        //   v2 = VARDEF
                        //   v3 = LEAQ
                        //   v4 = CMPQ v1 v3
                        // We don't want to combine the CMPQ with the load, because
                        // that would force the CMPQ to schedule before the VARDEF, which
                        // in turn requires the LEAQ to schedule before the VARDEF.
                        return false;

                    }

                    if (v.Type.IsMemory())
                    {
                        if (memPreds == null)
                        { 
                            // Initialise a map containing memory states
                            // known to be predecessors of load's memory
                            // state.
                            memPreds = make_map<ptr<Value>, bool>();
                            var m = mem;
                            const long limit = (long)50L;

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
                            continue;
                        }

                        return false;

                    }

                    if (len(v.Args) > 0L && v.Args[len(v.Args) - 1L] == mem)
                    { 
                        // If v takes mem as an input then we know mem
                        // is valid at this point.
                        continue;

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

        // symNamed reports whether sym's name is name.
        private static bool symNamed(Sym sym, @string name)
        {
            return sym.String() == name;
        }

        // isSameSym reports whether sym is the same as the given named symbol
        private static bool isSameSym(object sym, @string name)
        {
            fmt.Stringer (s, ok) = sym._<fmt.Stringer>();
            return ok && s.String() == name;
        }

        // nlz returns the number of leading zeros.
        private static long nlz64(long x)
        {
            return bits.LeadingZeros64(uint64(x));
        }
        private static long nlz32(int x)
        {
            return bits.LeadingZeros32(uint32(x));
        }
        private static long nlz16(short x)
        {
            return bits.LeadingZeros16(uint16(x));
        }
        private static long nlz8(sbyte x)
        {
            return bits.LeadingZeros8(uint8(x));
        }

        // ntzX returns the number of trailing zeros.
        private static long ntz64(long x)
        {
            return bits.TrailingZeros64(uint64(x));
        }
        private static long ntz32(int x)
        {
            return bits.TrailingZeros32(uint32(x));
        }
        private static long ntz16(short x)
        {
            return bits.TrailingZeros16(uint16(x));
        }
        private static long ntz8(sbyte x)
        {
            return bits.TrailingZeros8(uint8(x));
        }

        private static bool oneBit(long x)
        {
            return x & (x - 1L) == 0L && x != 0L;
        }
        private static bool oneBit8(sbyte x)
        {
            return x & (x - 1L) == 0L && x != 0L;
        }
        private static bool oneBit16(short x)
        {
            return x & (x - 1L) == 0L && x != 0L;
        }
        private static bool oneBit32(int x)
        {
            return x & (x - 1L) == 0L && x != 0L;
        }
        private static bool oneBit64(long x)
        {
            return x & (x - 1L) == 0L && x != 0L;
        }

        // nto returns the number of trailing ones.
        private static long nto(long x)
        {
            return int64(ntz64(~x));
        }

        // log2 returns logarithm in base 2 of uint64(n), with log2(0) = -1.
        // Rounds down.
        private static long log2(long n)
        {
            return int64(bits.Len64(uint64(n))) - 1L;
        }

        // logX returns logarithm of n base 2.
        // n must be a positive power of 2 (isPowerOfTwoX returns true).
        private static long log8(sbyte n)
        {
            return int64(bits.Len8(uint8(n))) - 1L;
        }
        private static long log16(short n)
        {
            return int64(bits.Len16(uint16(n))) - 1L;
        }
        private static long log32(int n)
        {
            return int64(bits.Len32(uint32(n))) - 1L;
        }
        private static long log64(long n)
        {
            return int64(bits.Len64(uint64(n))) - 1L;
        }

        // log2uint32 returns logarithm in base 2 of uint32(n), with log2(0) = -1.
        // Rounds down.
        private static long log2uint32(long n)
        {
            return int64(bits.Len32(uint32(n))) - 1L;
        }

        // isPowerOfTwo reports whether n is a power of 2.
        private static bool isPowerOfTwo(long n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }
        private static bool isPowerOfTwo8(sbyte n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }
        private static bool isPowerOfTwo16(short n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }
        private static bool isPowerOfTwo32(int n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }
        private static bool isPowerOfTwo64(long n)
        {
            return n > 0L && n & (n - 1L) == 0L;
        }

        // isUint64PowerOfTwo reports whether uint64(n) is a power of 2.
        private static bool isUint64PowerOfTwo(long @in)
        {
            var n = uint64(in);
            return n > 0L && n & (n - 1L) == 0L;
        }

        // isUint32PowerOfTwo reports whether uint32(n) is a power of 2.
        private static bool isUint32PowerOfTwo(long @in)
        {
            var n = uint64(uint32(in));
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

        // is8Bit reports whether n can be represented as a signed 8 bit integer.
        private static bool is8Bit(long n)
        {
            return n == int64(int8(n));
        }

        // isU8Bit reports whether n can be represented as an unsigned 8 bit integer.
        private static bool isU8Bit(long n)
        {
            return n == int64(uint8(n));
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

        // b2i32 translates a boolean value to 0 or 1.
        private static int b2i32(bool b)
        {
            if (b)
            {
                return 1L;
            }

            return 0L;

        }

        // shiftIsBounded reports whether (left/right) shift Value v is known to be bounded.
        // A shift is bounded if it is shifting by less than the width of the shifted value.
        private static bool shiftIsBounded(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            return v.AuxInt != 0L;
        }

        // truncate64Fto32F converts a float64 value to a float32 preserving the bit pattern
        // of the mantissa. It will panic if the truncation results in lost information.
        private static float truncate64Fto32F(double f) => func((_, panic, __) =>
        {
            if (!isExactFloat32(f))
            {
                panic("truncate64Fto32F: truncation is not exact");
            }

            if (!math.IsNaN(f))
            {
                return float32(f);
            } 
            // NaN bit patterns aren't necessarily preserved across conversion
            // instructions so we need to do the conversion manually.
            var b = math.Float64bits(f);
            var m = b & ((1L << (int)(52L)) - 1L); // mantissa (a.k.a. significand)
            //          | sign                  | exponent   | mantissa       |
            var r = uint32(((b >> (int)(32L)) & (1L << (int)(31L))) | 0x7f800000UL | (m >> (int)((52L - 23L))));
            return math.Float32frombits(r);

        });

        // extend32Fto64F converts a float32 value to a float64 value preserving the bit
        // pattern of the mantissa.
        private static double extend32Fto64F(float f)
        {
            if (!math.IsNaN(float64(f)))
            {
                return float64(f);
            } 
            // NaN bit patterns aren't necessarily preserved across conversion
            // instructions so we need to do the conversion manually.
            var b = uint64(math.Float32bits(f)); 
            //   | sign                  | exponent      | mantissa                    |
            var r = ((b << (int)(32L)) & (1L << (int)(63L))) | (0x7ffUL << (int)(52L)) | ((b & 0x7fffffUL) << (int)((52L - 23L)));
            return math.Float64frombits(r);

        }

        // DivisionNeedsFixUp reports whether the division needs fix-up code.
        public static bool DivisionNeedsFixUp(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            return v.AuxInt == 0L;
        }

        // auxFrom64F encodes a float64 value so it can be stored in an AuxInt.
        private static long auxFrom64F(double f) => func((_, panic, __) =>
        {
            if (f != f)
            {
                panic("can't encode a NaN in AuxInt field");
            }

            return int64(math.Float64bits(f));

        });

        // auxFrom32F encodes a float32 value so it can be stored in an AuxInt.
        private static long auxFrom32F(float f) => func((_, panic, __) =>
        {
            if (f != f)
            {
                panic("can't encode a NaN in AuxInt field");
            }

            return int64(math.Float64bits(extend32Fto64F(f)));

        });

        // auxTo32F decodes a float32 from the AuxInt value provided.
        private static float auxTo32F(long i)
        {
            return truncate64Fto32F(math.Float64frombits(uint64(i)));
        }

        // auxTo64F decodes a float64 from the AuxInt value provided.
        private static double auxTo64F(long i)
        {
            return math.Float64frombits(uint64(i));
        }

        private static bool auxIntToBool(long i)
        {
            if (i == 0L)
            {
                return false;
            }

            return true;

        }
        private static sbyte auxIntToInt8(long i)
        {
            return int8(i);
        }
        private static short auxIntToInt16(long i)
        {
            return int16(i);
        }
        private static int auxIntToInt32(long i)
        {
            return int32(i);
        }
        private static long auxIntToInt64(long i)
        {
            return i;
        }
        private static byte auxIntToUint8(long i)
        {
            return uint8(i);
        }
        private static float auxIntToFloat32(long i)
        {
            return float32(math.Float64frombits(uint64(i)));
        }
        private static double auxIntToFloat64(long i)
        {
            return math.Float64frombits(uint64(i));
        }
        private static ValAndOff auxIntToValAndOff(long i)
        {
            return ValAndOff(i);
        }
        private static int128 auxIntToInt128(long x) => func((_, panic, __) =>
        {
            if (x != 0L)
            {
                panic("nonzero int128 not allowed");
            }

            return 0L;

        });
        private static flagConstant auxIntToFlagConstant(long x)
        {
            return flagConstant(x);
        }

        private static long boolToAuxInt(bool b)
        {
            if (b)
            {
                return 1L;
            }

            return 0L;

        }
        private static long int8ToAuxInt(sbyte i)
        {
            return int64(i);
        }
        private static long int16ToAuxInt(short i)
        {
            return int64(i);
        }
        private static long int32ToAuxInt(int i)
        {
            return int64(i);
        }
        private static long int64ToAuxInt(long i)
        {
            return int64(i);
        }
        private static long uint8ToAuxInt(byte i)
        {
            return int64(int8(i));
        }
        private static long float32ToAuxInt(float f)
        {
            return int64(math.Float64bits(float64(f)));
        }
        private static long float64ToAuxInt(double f)
        {
            return int64(math.Float64bits(f));
        }
        private static long valAndOffToAuxInt(ValAndOff v)
        {
            return int64(v);
        }
        private static long int128ToAuxInt(int128 x) => func((_, panic, __) =>
        {
            if (x != 0L)
            {
                panic("nonzero int128 not allowed");
            }

            return 0L;

        });
        private static long flagConstantToAuxInt(flagConstant x)
        {
            return int64(x);
        }

        private static @string auxToString(object i)
        {
            return i._<@string>();
        }
        private static Sym auxToSym(object i)
        { 
            // TODO: kind of a hack - allows nil interface through
            Sym (s, _) = i._<Sym>();
            return s;

        }
        private static ptr<types.Type> auxToType(object i)
        {
            return i._<ptr<types.Type>>();
        }
        private static s390x.CCMask auxToS390xCCMask(object i)
        {
            return i._<s390x.CCMask>();
        }
        private static s390x.RotateParams auxToS390xRotateParams(object i)
        {
            return i._<s390x.RotateParams>();
        }

        private static void stringToAux(@string s)
        {
            return s;
        }
        private static void symToAux(Sym s)
        {
            return s;
        }
        private static void typeToAux(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            return t;
        }
        private static void s390xCCMaskToAux(s390x.CCMask c)
        {
            return c;
        }
        private static void s390xRotateParamsToAux(s390x.RotateParams r)
        {
            return r;
        }
        private static void cCopToAux(Op o)
        {
            return o;
        }

        // uaddOvf reports whether unsigned a+b would overflow.
        private static bool uaddOvf(long a, long b)
        {
            return uint64(a) + uint64(b) < uint64(a);
        }

        // de-virtualize an InterCall
        // 'sym' is the symbol for the itab
        private static ptr<obj.LSym> devirt(ptr<Value> _addr_v, Sym sym, long offset)
        {
            ref Value v = ref _addr_v.val;

            var f = v.Block.Func;
            ptr<obj.LSym> (n, ok) = sym._<ptr<obj.LSym>>();
            if (!ok)
            {
                return _addr_null!;
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

            return _addr_lsym!;

        }

        // isSamePtr reports whether p1 and p2 point to the same address.
        private static bool isSamePtr(ptr<Value> _addr_p1, ptr<Value> _addr_p2)
        {
            ref Value p1 = ref _addr_p1.val;
            ref Value p2 = ref _addr_p2.val;

            if (p1 == p2)
            {
                return true;
            }

            if (p1.Op != p2.Op)
            {
                return false;
            }


            if (p1.Op == OpOffPtr) 
                return p1.AuxInt == p2.AuxInt && isSamePtr(_addr_p1.Args[0L], _addr_p2.Args[0L]);
            else if (p1.Op == OpAddr || p1.Op == OpLocalAddr) 
                // OpAddr's 0th arg is either OpSP or OpSB, which means that it is uniquely identified by its Op.
                // Checking for value equality only works after [z]cse has run.
                return p1.Aux == p2.Aux && p1.Args[0L].Op == p2.Args[0L].Op;
            else if (p1.Op == OpAddPtr) 
                return p1.Args[1L] == p2.Args[1L] && isSamePtr(_addr_p1.Args[0L], _addr_p2.Args[0L]);
                        return false;

        }

        private static bool isStackPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            while (v.Op == OpOffPtr || v.Op == OpAddPtr)
            {
                v = v.Args[0L];
            }

            return v.Op == OpSP || v.Op == OpLocalAddr;

        }

        // disjoint reports whether the memory region specified by [p1:p1+n1)
        // does not overlap with [p2:p2+n2).
        // A return value of false does not imply the regions overlap.
        private static bool disjoint(ptr<Value> _addr_p1, long n1, ptr<Value> _addr_p2, long n2)
        {
            ref Value p1 = ref _addr_p1.val;
            ref Value p2 = ref _addr_p2.val;

            if (n1 == 0L || n2 == 0L)
            {
                return true;
            }

            if (p1 == p2)
            {
                return false;
            }

            Func<ptr<Value>, (ptr<Value>, long)> baseAndOffset = ptr =>
            {
                base = ptr;
                offset = 0L;
                while (@base.Op == OpOffPtr)
                {
                    offset += @base.AuxInt;
                    base = @base.Args[0L];
                }

                return (base, offset);

            }
;
            var (p1, off1) = baseAndOffset(p1);
            var (p2, off2) = baseAndOffset(p2);
            if (isSamePtr(_addr_p1, _addr_p2))
            {
                return !overlap(off1, n1, off2, n2);
            } 
            // p1 and p2 are not the same, so if they are both OpAddrs then
            // they point to different variables.
            // If one pointer is on the stack and the other is an argument
            // then they can't overlap.

            if (p1.Op == OpAddr || p1.Op == OpLocalAddr) 
                if (p2.Op == OpAddr || p2.Op == OpLocalAddr || p2.Op == OpSP)
                {
                    return true;
                }

                return p2.Op == OpArg && p1.Args[0L].Op == OpSP;
            else if (p1.Op == OpArg) 
                if (p2.Op == OpSP || p2.Op == OpLocalAddr)
                {
                    return true;
                }

            else if (p1.Op == OpSP) 
                return p2.Op == OpAddr || p2.Op == OpLocalAddr || p2.Op == OpArg || p2.Op == OpSP;
                        return false;

        }

        // moveSize returns the number of bytes an aligned MOV instruction moves
        private static long moveSize(long align, ptr<Config> _addr_c)
        {
            ref Config c = ref _addr_c.val;


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
        private static ptr<Block> mergePoint(ptr<Block> _addr_b, params ptr<ptr<Value>>[] _addr_a)
        {
            a = a.Clone();
            ref Block b = ref _addr_b.val;
            ref Value a = ref _addr_a.val;
 
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
                    return _addr_null!;

                }

                b = b.Preds[0L].b;
                d--;

            }

            return _addr_null!; // too far away
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
                    return _addr_r!;

                }

                if (len(b.Preds) > 1L)
                {
                    return _addr_null!;
                }

                b = b.Preds[0L].b;
                d--;


            }

            return _addr_null!; // too far away
        }

        // clobber invalidates values. Returns true.
        // clobber is used by rewrite rules to:
        //   A) make sure the values are really dead and never used again.
        //   B) decrement use counts of the values' args.
        private static bool clobber(params ptr<ptr<Value>>[] _addr_vv)
        {
            vv = vv.Clone();
            ref Value vv = ref _addr_vv.val;

            foreach (var (_, v) in vv)
            {
                v.reset(OpInvalid); 
                // Note: leave v.Block intact.  The Block field is used after clobber.
            }
            return true;

        }

        // clobberIfDead resets v when use count is 1. Returns true.
        // clobberIfDead is used by rewrite rules to decrement
        // use counts of v's args when v is dead and never used.
        private static bool clobberIfDead(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            if (v.Uses == 1L)
            {
                v.reset(OpInvalid);
            } 
            // Note: leave v.Block intact.  The Block field is used after clobberIfDead.
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

        // countRule increments Func.ruleMatches[key].
        // If Func.ruleMatches is non-nil at the end
        // of compilation, it will be printed to stdout.
        // This is intended to make it easier to find which functions
        // which contain lots of rules matches when developing new rules.
        private static bool countRule(ptr<Value> _addr_v, @string key)
        {
            ref Value v = ref _addr_v.val;

            var f = v.Block.Func;
            if (f.ruleMatches == null)
            {
                f.ruleMatches = make_map<@string, long>();
            }

            f.ruleMatches[key]++;
            return true;

        }

        // warnRule generates compiler debug output with string s when
        // v is not in autogenerated code, cond is true and the rule has fired.
        private static bool warnRule(bool cond, ptr<Value> _addr_v, @string s)
        {
            ref Value v = ref _addr_v.val;

            {
                var pos = v.Pos;

                if (pos.Line() > 1L && cond)
                {
                    v.Block.Func.Warnl(pos, s);
                }

            }

            return true;

        }

        // for a pseudo-op like (LessThan x), extract x
        private static ptr<Value> flagArg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            if (len(v.Args) != 1L || !v.Args[0L].Type.IsFlags())
            {
                return _addr_null!;
            }

            return _addr_v.Args[0L]!;

        }

        // arm64Negate finds the complement to an ARM64 condition code,
        // for example Equal -> NotEqual or LessThan -> GreaterEqual
        //
        // TODO: add floating-point conditions
        private static Op arm64Negate(Op op) => func((_, panic, __) =>
        {

            if (op == OpARM64LessThan) 
                return OpARM64GreaterEqual;
            else if (op == OpARM64LessThanU) 
                return OpARM64GreaterEqualU;
            else if (op == OpARM64GreaterThan) 
                return OpARM64LessEqual;
            else if (op == OpARM64GreaterThanU) 
                return OpARM64LessEqualU;
            else if (op == OpARM64LessEqual) 
                return OpARM64GreaterThan;
            else if (op == OpARM64LessEqualU) 
                return OpARM64GreaterThanU;
            else if (op == OpARM64GreaterEqual) 
                return OpARM64LessThan;
            else if (op == OpARM64GreaterEqualU) 
                return OpARM64LessThanU;
            else if (op == OpARM64Equal) 
                return OpARM64NotEqual;
            else if (op == OpARM64NotEqual) 
                return OpARM64Equal;
            else if (op == OpARM64LessThanF) 
                return OpARM64GreaterEqualF;
            else if (op == OpARM64GreaterThanF) 
                return OpARM64LessEqualF;
            else if (op == OpARM64LessEqualF) 
                return OpARM64GreaterThanF;
            else if (op == OpARM64GreaterEqualF) 
                return OpARM64LessThanF;
            else 
                panic("unreachable");
            
        });

        // arm64Invert evaluates (InvertFlags op), which
        // is the same as altering the condition codes such
        // that the same result would be produced if the arguments
        // to the flag-generating instruction were reversed, e.g.
        // (InvertFlags (CMP x y)) -> (CMP y x)
        //
        // TODO: add floating-point conditions
        private static Op arm64Invert(Op op) => func((_, panic, __) =>
        {

            if (op == OpARM64LessThan) 
                return OpARM64GreaterThan;
            else if (op == OpARM64LessThanU) 
                return OpARM64GreaterThanU;
            else if (op == OpARM64GreaterThan) 
                return OpARM64LessThan;
            else if (op == OpARM64GreaterThanU) 
                return OpARM64LessThanU;
            else if (op == OpARM64LessEqual) 
                return OpARM64GreaterEqual;
            else if (op == OpARM64LessEqualU) 
                return OpARM64GreaterEqualU;
            else if (op == OpARM64GreaterEqual) 
                return OpARM64LessEqual;
            else if (op == OpARM64GreaterEqualU) 
                return OpARM64LessEqualU;
            else if (op == OpARM64Equal || op == OpARM64NotEqual) 
                return op;
            else if (op == OpARM64LessThanF) 
                return OpARM64GreaterThanF;
            else if (op == OpARM64GreaterThanF) 
                return OpARM64LessThanF;
            else if (op == OpARM64LessEqualF) 
                return OpARM64GreaterEqualF;
            else if (op == OpARM64GreaterEqualF) 
                return OpARM64LessEqualF;
            else 
                panic("unreachable");
            
        });

        // evaluate an ARM64 op against a flags value
        // that is potentially constant; return 1 for true,
        // -1 for false, and 0 for not constant.
        private static long ccARM64Eval(object cc, ptr<Value> _addr_flags)
        {
            ref Value flags = ref _addr_flags.val;

            Op op = cc._<Op>();
            var fop = flags.Op;
            if (fop == OpARM64InvertFlags)
            {
                return -ccARM64Eval(op, _addr_flags.Args[0L]);
            }

            if (fop != OpARM64FlagConstant)
            {
                return 0L;
            }

            var fc = flagConstant(flags.AuxInt);
            Func<bool, long> b2i = b =>
            {
                if (b)
                {
                    return 1L;
                }

                return -1L;

            }
;

            if (op == OpARM64Equal) 
                return b2i(fc.eq());
            else if (op == OpARM64NotEqual) 
                return b2i(fc.ne());
            else if (op == OpARM64LessThan) 
                return b2i(fc.lt());
            else if (op == OpARM64LessThanU) 
                return b2i(fc.ult());
            else if (op == OpARM64GreaterThan) 
                return b2i(fc.gt());
            else if (op == OpARM64GreaterThanU) 
                return b2i(fc.ugt());
            else if (op == OpARM64LessEqual) 
                return b2i(fc.le());
            else if (op == OpARM64LessEqualU) 
                return b2i(fc.ule());
            else if (op == OpARM64GreaterEqual) 
                return b2i(fc.ge());
            else if (op == OpARM64GreaterEqualU) 
                return b2i(fc.uge());
                        return 0L;

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

            var (_, err) = fmt.Fprintln(ruleFile, s);
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

        private static bool isConstZero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


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

        private static bool areAdjacentOffsets(long off1, long off2, long size)
        {
            return off1 + size == off2 || off1 == off2 + size;
        }

        // check if value zeroes out upper 32-bit of 64-bit register.
        // depth limits recursion depth. In AMD64.rules 3 is used as limit,
        // because it catches same amount of cases as 4.
        private static bool zeroUpper32Bits(ptr<Value> _addr_x, long depth)
        {
            ref Value x = ref _addr_x.val;


            if (x.Op == OpAMD64MOVLconst || x.Op == OpAMD64MOVLload || x.Op == OpAMD64MOVLQZX || x.Op == OpAMD64MOVLloadidx1 || x.Op == OpAMD64MOVWload || x.Op == OpAMD64MOVWloadidx1 || x.Op == OpAMD64MOVBload || x.Op == OpAMD64MOVBloadidx1 || x.Op == OpAMD64MOVLloadidx4 || x.Op == OpAMD64ADDLload || x.Op == OpAMD64SUBLload || x.Op == OpAMD64ANDLload || x.Op == OpAMD64ORLload || x.Op == OpAMD64XORLload || x.Op == OpAMD64CVTTSD2SL || x.Op == OpAMD64ADDL || x.Op == OpAMD64ADDLconst || x.Op == OpAMD64SUBL || x.Op == OpAMD64SUBLconst || x.Op == OpAMD64ANDL || x.Op == OpAMD64ANDLconst || x.Op == OpAMD64ORL || x.Op == OpAMD64ORLconst || x.Op == OpAMD64XORL || x.Op == OpAMD64XORLconst || x.Op == OpAMD64NEGL || x.Op == OpAMD64NOTL || x.Op == OpAMD64SHRL || x.Op == OpAMD64SHRLconst || x.Op == OpAMD64SARL || x.Op == OpAMD64SARLconst || x.Op == OpAMD64SHLL || x.Op == OpAMD64SHLLconst) 
                return true;
            else if (x.Op == OpArg) 
                return x.Type.Width == 4L;
            else if (x.Op == OpPhi || x.Op == OpSelect0 || x.Op == OpSelect1) 
                // Phis can use each-other as an arguments, instead of tracking visited values,
                // just limit recursion depth.
                if (depth <= 0L)
                {
                    return false;
                }

                foreach (var (i) in x.Args)
                {
                    if (!zeroUpper32Bits(_addr_x.Args[i], depth - 1L))
                    {
                        return false;
                    }

                }
                return true;
                        return false;

        }

        // zeroUpper48Bits is similar to zeroUpper32Bits, but for upper 48 bits
        private static bool zeroUpper48Bits(ptr<Value> _addr_x, long depth)
        {
            ref Value x = ref _addr_x.val;


            if (x.Op == OpAMD64MOVWQZX || x.Op == OpAMD64MOVWload || x.Op == OpAMD64MOVWloadidx1 || x.Op == OpAMD64MOVWloadidx2) 
                return true;
            else if (x.Op == OpArg) 
                return x.Type.Width == 2L;
            else if (x.Op == OpPhi || x.Op == OpSelect0 || x.Op == OpSelect1) 
                // Phis can use each-other as an arguments, instead of tracking visited values,
                // just limit recursion depth.
                if (depth <= 0L)
                {
                    return false;
                }

                foreach (var (i) in x.Args)
                {
                    if (!zeroUpper48Bits(_addr_x.Args[i], depth - 1L))
                    {
                        return false;
                    }

                }
                return true;
                        return false;

        }

        // zeroUpper56Bits is similar to zeroUpper32Bits, but for upper 56 bits
        private static bool zeroUpper56Bits(ptr<Value> _addr_x, long depth)
        {
            ref Value x = ref _addr_x.val;


            if (x.Op == OpAMD64MOVBQZX || x.Op == OpAMD64MOVBload || x.Op == OpAMD64MOVBloadidx1) 
                return true;
            else if (x.Op == OpArg) 
                return x.Type.Width == 1L;
            else if (x.Op == OpPhi || x.Op == OpSelect0 || x.Op == OpSelect1) 
                // Phis can use each-other as an arguments, instead of tracking visited values,
                // just limit recursion depth.
                if (depth <= 0L)
                {
                    return false;
                }

                foreach (var (i) in x.Args)
                {
                    if (!zeroUpper56Bits(_addr_x.Args[i], depth - 1L))
                    {
                        return false;
                    }

                }
                return true;
                        return false;

        }

        // isInlinableMemmove reports whether the given arch performs a Move of the given size
        // faster than memmove. It will only return true if replacing the memmove with a Move is
        // safe, either because Move is small or because the arguments are disjoint.
        // This is used as a check for replacing memmove with Move ops.
        private static bool isInlinableMemmove(ptr<Value> _addr_dst, ptr<Value> _addr_src, long sz, ptr<Config> _addr_c)
        {
            ref Value dst = ref _addr_dst.val;
            ref Value src = ref _addr_src.val;
            ref Config c = ref _addr_c.val;
 
            // It is always safe to convert memmove into Move when its arguments are disjoint.
            // Move ops may or may not be faster for large sizes depending on how the platform
            // lowers them, so we only perform this optimization on platforms that we know to
            // have fast Move ops.
            switch (c.arch)
            {
                case "amd64": 
                    return sz <= 16L || (sz < 1024L && disjoint(_addr_dst, sz, _addr_src, sz));
                    break;
                case "386": 

                case "arm64": 
                    return sz <= 8L;
                    break;
                case "s390x": 

                case "ppc64": 

                case "ppc64le": 
                    return sz <= 8L || disjoint(_addr_dst, sz, _addr_src, sz);
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

        // logLargeCopy logs the occurrence of a large copy.
        // The best place to do this is in the rewrite rules where the size of the move is easy to find.
        // "Large" is arbitrarily chosen to be 128 bytes; this may change.
        private static bool logLargeCopy(ptr<Value> _addr_v, long s)
        {
            ref Value v = ref _addr_v.val;

            if (s < 128L)
            {
                return true;
            }

            if (logopt.Enabled())
            {
                logopt.LogOpt(v.Pos, "copy", "lower", v.Block.Func.Name, fmt.Sprintf("%d bytes", s));
            }

            return true;

        }

        // hasSmallRotate reports whether the architecture has rotate instructions
        // for sizes < 32-bit.  This is used to decide whether to promote some rotations.
        private static bool hasSmallRotate(ptr<Config> _addr_c)
        {
            ref Config c = ref _addr_c.val;

            switch (c.arch)
            {
                case "amd64": 

                case "386": 
                    return true;
                    break;
                default: 
                    return false;
                    break;
            }

        }

        // encodes the lsb and width for arm(64) bitfield ops into the expected auxInt format.
        private static long armBFAuxInt(long lsb, long width) => func((_, panic, __) =>
        {
            if (lsb < 0L || lsb > 63L)
            {
                panic("ARM(64) bit field lsb constant out of range");
            }

            if (width < 1L || width > 64L)
            {
                panic("ARM(64) bit field width constant out of range");
            }

            return width | lsb << (int)(8L);

        });

        // returns the lsb part of the auxInt field of arm64 bitfield ops.
        private static long getARM64BFlsb(long bfc)
        {
            return int64(uint64(bfc) >> (int)(8L));
        }

        // returns the width part of the auxInt field of arm64 bitfield ops.
        private static long getARM64BFwidth(long bfc)
        {
            return bfc & 0xffUL;
        }

        // checks if mask >> rshift applied at lsb is a valid arm64 bitfield op mask.
        private static bool isARM64BFMask(long lsb, long mask, long rshift)
        {
            var shiftedMask = int64(uint64(mask) >> (int)(uint64(rshift)));
            return shiftedMask != 0L && isPowerOfTwo(shiftedMask + 1L) && nto(shiftedMask) + lsb < 64L;
        }

        // returns the bitfield width of mask >> rshift for arm64 bitfield ops
        private static long arm64BFWidth(long mask, long rshift) => func((_, panic, __) =>
        {
            var shiftedMask = int64(uint64(mask) >> (int)(uint64(rshift)));
            if (shiftedMask == 0L)
            {
                panic("ARM64 BF mask is zero");
            }

            return nto(shiftedMask);

        });

        // sizeof returns the size of t in bytes.
        // It will panic if t is not a *types.Type.
        private static long @sizeof(object t)
        {
            return t._<ptr<types.Type>>().Size();
        }

        // registerizable reports whether t is a primitive type that fits in
        // a register. It assumes float64 values will always fit into registers
        // even if that isn't strictly true.
        private static bool registerizable(ptr<Block> _addr_b, ptr<types.Type> _addr_typ)
        {
            ref Block b = ref _addr_b.val;
            ref types.Type typ = ref _addr_typ.val;

            if (typ.IsPtrShaped() || typ.IsFloat())
            {
                return true;
            }

            if (typ.IsInteger())
            {
                return typ.Size() <= b.Func.Config.RegSize;
            }

            return false;

        }

        // needRaceCleanup reports whether this call to racefuncenter/exit isn't needed.
        private static bool needRaceCleanup(Sym sym, ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var f = v.Block.Func;
            if (!f.Config.Race)
            {
                return false;
            }

            if (!symNamed(sym, "runtime.racefuncenter") && !symNamed(sym, "runtime.racefuncexit"))
            {
                return false;
            }

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {

                    if (v.Op == OpStaticCall) 
                        // Check for racefuncenter will encounter racefuncexit and vice versa.
                        // Allow calls to panic*
                        fmt.Stringer s = v.Aux._<fmt.Stringer>().String();
                        switch (s)
                        {
                            case "runtime.racefuncenter": 

                            case "runtime.racefuncexit": 

                            case "runtime.panicdivide": 

                            case "runtime.panicwrap": 

                            case "runtime.panicshift": 
                                continue;
                                break;
                        } 
                        // If we encountered any call, we need to keep racefunc*,
                        // for accurate stacktraces.
                        return false;
                    else if (v.Op == OpPanicBounds || v.Op == OpPanicExtend)                     else if (v.Op == OpClosureCall || v.Op == OpInterCall) 
                        // We must keep the race functions if there are any other call types.
                        return false;
                    
                }

            }
            return true;

        }

        // symIsRO reports whether sym is a read-only global.
        private static bool symIsRO(object sym)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>();
            return lsym.Type == objabi.SRODATA && len(lsym.R) == 0L;
        }

        // symIsROZero reports whether sym is a read-only global whose data contains all zeros.
        private static bool symIsROZero(Sym sym)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>();
            if (lsym.Type != objabi.SRODATA || len(lsym.R) != 0L)
            {
                return false;
            }

            foreach (var (_, b) in lsym.P)
            {
                if (b != 0L)
                {
                    return false;
                }

            }
            return true;

        }

        // read8 reads one byte from the read-only global sym at offset off.
        private static byte read8(object sym, long off)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>();
            if (off >= int64(len(lsym.P)) || off < 0L)
            { 
                // Invalid index into the global sym.
                // This can happen in dead code, so we don't want to panic.
                // Just return any value, it will eventually get ignored.
                // See issue 29215.
                return 0L;

            }

            return lsym.P[off];

        }

        // read16 reads two bytes from the read-only global sym at offset off.
        private static ushort read16(object sym, long off, binary.ByteOrder byteorder)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>(); 
            // lsym.P is written lazily.
            // Bytes requested after the end of lsym.P are 0.
            slice<byte> src = default;
            if (0L <= off && off < int64(len(lsym.P)))
            {
                src = lsym.P[off..];
            }

            var buf = make_slice<byte>(2L);
            copy(buf, src);
            return byteorder.Uint16(buf);

        }

        // read32 reads four bytes from the read-only global sym at offset off.
        private static uint read32(object sym, long off, binary.ByteOrder byteorder)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>();
            slice<byte> src = default;
            if (0L <= off && off < int64(len(lsym.P)))
            {
                src = lsym.P[off..];
            }

            var buf = make_slice<byte>(4L);
            copy(buf, src);
            return byteorder.Uint32(buf);

        }

        // read64 reads eight bytes from the read-only global sym at offset off.
        private static ulong read64(object sym, long off, binary.ByteOrder byteorder)
        {
            ptr<obj.LSym> lsym = sym._<ptr<obj.LSym>>();
            slice<byte> src = default;
            if (0L <= off && off < int64(len(lsym.P)))
            {
                src = lsym.P[off..];
            }

            var buf = make_slice<byte>(8L);
            copy(buf, src);
            return byteorder.Uint64(buf);

        }

        // sequentialAddresses reports true if it can prove that x + n == y
        private static bool sequentialAddresses(ptr<Value> _addr_x, ptr<Value> _addr_y, long n)
        {
            ref Value x = ref _addr_x.val;
            ref Value y = ref _addr_y.val;

            if (x.Op == Op386ADDL && y.Op == Op386LEAL1 && y.AuxInt == n && y.Aux == null && (x.Args[0L] == y.Args[0L] && x.Args[1L] == y.Args[1L] || x.Args[0L] == y.Args[1L] && x.Args[1L] == y.Args[0L]))
            {
                return true;
            }

            if (x.Op == Op386LEAL1 && y.Op == Op386LEAL1 && y.AuxInt == x.AuxInt + n && x.Aux == y.Aux && (x.Args[0L] == y.Args[0L] && x.Args[1L] == y.Args[1L] || x.Args[0L] == y.Args[1L] && x.Args[1L] == y.Args[0L]))
            {
                return true;
            }

            if (x.Op == OpAMD64ADDQ && y.Op == OpAMD64LEAQ1 && y.AuxInt == n && y.Aux == null && (x.Args[0L] == y.Args[0L] && x.Args[1L] == y.Args[1L] || x.Args[0L] == y.Args[1L] && x.Args[1L] == y.Args[0L]))
            {
                return true;
            }

            if (x.Op == OpAMD64LEAQ1 && y.Op == OpAMD64LEAQ1 && y.AuxInt == x.AuxInt + n && x.Aux == y.Aux && (x.Args[0L] == y.Args[0L] && x.Args[1L] == y.Args[1L] || x.Args[0L] == y.Args[1L] && x.Args[1L] == y.Args[0L]))
            {
                return true;
            }

            return false;

        }

        // flagConstant represents the result of a compile-time comparison.
        // The sense of these flags does not necessarily represent the hardware's notion
        // of a flags register - these are just a compile-time construct.
        // We happen to match the semantics to those of arm/arm64.
        // Note that these semantics differ from x86: the carry flag has the opposite
        // sense on a subtraction!
        //   On amd64, C=1 represents a borrow, e.g. SBB on amd64 does x - y - C.
        //   On arm64, C=0 represents a borrow, e.g. SBC on arm64 does x - y - ^C.
        //    (because it does x + ^y + C).
        // See https://en.wikipedia.org/wiki/Carry_flag#Vs._borrow_flag
        private partial struct flagConstant // : byte
        {
        }

        // N reports whether the result of an operation is negative (high bit set).
        private static bool N(this flagConstant fc)
        {
            return fc & 1L != 0L;
        }

        // Z reports whether the result of an operation is 0.
        private static bool Z(this flagConstant fc)
        {
            return fc & 2L != 0L;
        }

        // C reports whether an unsigned add overflowed (carry), or an
        // unsigned subtract did not underflow (borrow).
        private static bool C(this flagConstant fc)
        {
            return fc & 4L != 0L;
        }

        // V reports whether a signed operation overflowed or underflowed.
        private static bool V(this flagConstant fc)
        {
            return fc & 8L != 0L;
        }

        private static bool eq(this flagConstant fc)
        {
            return fc.Z();
        }
        private static bool ne(this flagConstant fc)
        {
            return !fc.Z();
        }
        private static bool lt(this flagConstant fc)
        {
            return fc.N() != fc.V();
        }
        private static bool le(this flagConstant fc)
        {
            return fc.Z() || fc.lt();
        }
        private static bool gt(this flagConstant fc)
        {
            return !fc.Z() && fc.ge();
        }
        private static bool ge(this flagConstant fc)
        {
            return fc.N() == fc.V();
        }
        private static bool ult(this flagConstant fc)
        {
            return !fc.C();
        }
        private static bool ule(this flagConstant fc)
        {
            return fc.Z() || fc.ult();
        }
        private static bool ugt(this flagConstant fc)
        {
            return !fc.Z() && fc.uge();
        }
        private static bool uge(this flagConstant fc)
        {
            return fc.C();
        }

        private static bool ltNoov(this flagConstant fc)
        {
            return fc.lt() && !fc.V();
        }
        private static bool leNoov(this flagConstant fc)
        {
            return fc.le() && !fc.V();
        }
        private static bool gtNoov(this flagConstant fc)
        {
            return fc.gt() && !fc.V();
        }
        private static bool geNoov(this flagConstant fc)
        {
            return fc.ge() && !fc.V();
        }

        private static @string String(this flagConstant fc)
        {
            return fmt.Sprintf("N=%v,Z=%v,C=%v,V=%v", fc.N(), fc.Z(), fc.C(), fc.V());
        }

        private partial struct flagConstantBuilder
        {
            public bool N;
            public bool Z;
            public bool C;
            public bool V;
        }

        private static flagConstant encode(this flagConstantBuilder fcs)
        {
            flagConstant fc = default;
            if (fcs.N)
            {
                fc |= 1L;
            }

            if (fcs.Z)
            {
                fc |= 2L;
            }

            if (fcs.C)
            {
                fc |= 4L;
            }

            if (fcs.V)
            {
                fc |= 8L;
            }

            return fc;

        }

        // Note: addFlags(x,y) != subFlags(x,-y) in some situations:
        //  - the results of the C flag are different
        //  - the results of the V flag when y==minint are different

        // addFlags64 returns the flags that would be set from computing x+y.
        private static flagConstant addFlags64(long x, long y)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x + y == 0L;
            fcb.N = x + y < 0L;
            fcb.C = uint64(x + y) < uint64(x);
            fcb.V = x >= 0L && y >= 0L && x + y < 0L || x < 0L && y < 0L && x + y >= 0L;
            return fcb.encode();
        }

        // subFlags64 returns the flags that would be set from computing x-y.
        private static flagConstant subFlags64(long x, long y)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x - y == 0L;
            fcb.N = x - y < 0L;
            fcb.C = uint64(y) <= uint64(x); // This code follows the arm carry flag model.
            fcb.V = x >= 0L && y < 0L && x - y < 0L || x < 0L && y >= 0L && x - y >= 0L;
            return fcb.encode();

        }

        // addFlags32 returns the flags that would be set from computing x+y.
        private static flagConstant addFlags32(int x, int y)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x + y == 0L;
            fcb.N = x + y < 0L;
            fcb.C = uint32(x + y) < uint32(x);
            fcb.V = x >= 0L && y >= 0L && x + y < 0L || x < 0L && y < 0L && x + y >= 0L;
            return fcb.encode();
        }

        // subFlags32 returns the flags that would be set from computing x-y.
        private static flagConstant subFlags32(int x, int y)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x - y == 0L;
            fcb.N = x - y < 0L;
            fcb.C = uint32(y) <= uint32(x); // This code follows the arm carry flag model.
            fcb.V = x >= 0L && y < 0L && x - y < 0L || x < 0L && y >= 0L && x - y >= 0L;
            return fcb.encode();

        }

        // logicFlags64 returns flags set to the sign/zeroness of x.
        // C and V are set to false.
        private static flagConstant logicFlags64(long x)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x == 0L;
            fcb.N = x < 0L;
            return fcb.encode();
        }

        // logicFlags32 returns flags set to the sign/zeroness of x.
        // C and V are set to false.
        private static flagConstant logicFlags32(int x)
        {
            flagConstantBuilder fcb = default;
            fcb.Z = x == 0L;
            fcb.N = x < 0L;
            return fcb.encode();
        }
    }
}}}}
