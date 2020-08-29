// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2020 August 29 09:59:21 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Go\src\cmd\compile\internal\amd64\ssa.go
using fmt = go.fmt_package;
using math = go.math_package;

using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class amd64_package
    {
        // markMoves marks any MOVXconst ops that need to avoid clobbering flags.
        private static void ssaMarkMoves(ref gc.SSAGenState s, ref ssa.Block b)
        {
            var flive = b.FlagsLiveAtEnd;
            if (b.Control != null && b.Control.Type.IsFlags())
            {
                flive = true;
            }
            for (var i = len(b.Values) - 1L; i >= 0L; i--)
            {
                var v = b.Values[i];
                if (flive && (v.Op == ssa.OpAMD64MOVLconst || v.Op == ssa.OpAMD64MOVQconst))
                { 
                    // The "mark" is any non-nil Aux value.
                    v.Aux = v;
                }
                if (v.Type.IsFlags())
                {
                    flive = false;
                }
                foreach (var (_, a) in v.Args)
                {
                    if (a.Type.IsFlags())
                    {
                        flive = true;
                    }
                }
            }
        }

        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ref types.Type t)
        { 
            // Avoid partial register write
            if (!t.IsFloat() && t.Size() <= 2L)
            {
                if (t.Size() == 1L)
                {
                    return x86.AMOVBLZX;
                }
                else
                {
                    return x86.AMOVWLZX;
                }
            } 
            // Otherwise, there's no difference between load and store opcodes.
            return storeByType(t);
        }

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ref types.Type _t) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            var width = t.Size();
            if (t.IsFloat())
            {
                switch (width)
                {
                    case 4L: 
                        return x86.AMOVSS;
                        break;
                    case 8L: 
                        return x86.AMOVSD;
                        break;
                }
            }
            else
            {
                switch (width)
                {
                    case 1L: 
                        return x86.AMOVB;
                        break;
                    case 2L: 
                        return x86.AMOVW;
                        break;
                    case 4L: 
                        return x86.AMOVL;
                        break;
                    case 8L: 
                        return x86.AMOVQ;
                        break;
                }
            }
            panic("bad store type");
        });

        // moveByType returns the reg->reg move instruction of the given type.
        private static obj.As moveByType(ref types.Type _t) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            if (t.IsFloat())
            { 
                // Moving the whole sse2 register is faster
                // than moving just the correct low portion of it.
                // There is no xmm->xmm move with 1 byte opcode,
                // so use movups, which has 2 byte opcode.
                return x86.AMOVUPS;
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        // Avoids partial register write
                        return x86.AMOVL;
                        break;
                    case 2L: 
                        return x86.AMOVL;
                        break;
                    case 4L: 
                        return x86.AMOVL;
                        break;
                    case 8L: 
                        return x86.AMOVQ;
                        break;
                    case 16L: 
                        return x86.AMOVUPS; // int128s are in SSE registers
                        break;
                    default: 
                        panic(fmt.Sprintf("bad int register width %d:%s", t.Size(), t));
                        break;
                }
            }
        });

        // opregreg emits instructions for
        //     dest := dest(To) op src(From)
        // and also returns the created obj.Prog so it
        // may be further adjusted (offset, scale, etc).
        private static ref obj.Prog opregreg(ref gc.SSAGenState s, obj.As op, short dest, short src)
        {
            var p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = dest;
            p.From.Reg = src;
            return p;
        }

        // DUFFZERO consists of repeated blocks of 4 MOVUPSs + LEAQ,
        // See runtime/mkduff.go.
        private static long duffStart(long size)
        {
            var (x, _) = duff(size);
            return x;
        }
        private static long duffAdj(long size)
        {
            var (_, x) = duff(size);
            return x;
        }

        // duff returns the offset (from duffzero, in bytes) and pointer adjust (in bytes)
        // required to use the duffzero mechanism for a block of the given size.
        private static (long, long) duff(long size) => func((_, panic, __) =>
        {
            if (size < 32L || size > 1024L || size % dzClearStep != 0L)
            {
                panic("bad duffzero size");
            }
            var steps = size / dzClearStep;
            var blocks = steps / dzBlockLen;
            steps %= dzBlockLen;
            var off = dzBlockSize * (dzBlocks - blocks);
            long adj = default;
            if (steps != 0L)
            {
                off -= dzLeaqSize;
                off -= dzMovSize * steps;
                adj -= dzClearStep * (dzBlockLen - steps);
            }
            return (off, adj);
        });

        private static void ssaGenValue(ref gc.SSAGenState s, ref ssa.Value v)
        {

            if (v.Op == ssa.OpAMD64ADDQ || v.Op == ssa.OpAMD64ADDL) 
                var r = v.Reg();
                var r1 = v.Args[0L].Reg();
                var r2 = v.Args[1L].Reg();

                if (r == r1) 
                    var p = s.Prog(v.Op.Asm());
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = r2;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                else if (r == r2) 
                    p = s.Prog(v.Op.Asm());
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = r1;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                else 
                    obj.As asm = default;
                    if (v.Op == ssa.OpAMD64ADDQ)
                    {
                        asm = x86.ALEAQ;
                    }
                    else
                    {
                        asm = x86.ALEAL;
                    }
                    p = s.Prog(asm);
                    p.From.Type = obj.TYPE_MEM;
                    p.From.Reg = r1;
                    p.From.Scale = 1L;
                    p.From.Index = r2;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                // 2-address opcode arithmetic
            else if (v.Op == ssa.OpAMD64SUBQ || v.Op == ssa.OpAMD64SUBL || v.Op == ssa.OpAMD64MULQ || v.Op == ssa.OpAMD64MULL || v.Op == ssa.OpAMD64ANDQ || v.Op == ssa.OpAMD64ANDL || v.Op == ssa.OpAMD64ORQ || v.Op == ssa.OpAMD64ORL || v.Op == ssa.OpAMD64XORQ || v.Op == ssa.OpAMD64XORL || v.Op == ssa.OpAMD64SHLQ || v.Op == ssa.OpAMD64SHLL || v.Op == ssa.OpAMD64SHRQ || v.Op == ssa.OpAMD64SHRL || v.Op == ssa.OpAMD64SHRW || v.Op == ssa.OpAMD64SHRB || v.Op == ssa.OpAMD64SARQ || v.Op == ssa.OpAMD64SARL || v.Op == ssa.OpAMD64SARW || v.Op == ssa.OpAMD64SARB || v.Op == ssa.OpAMD64ROLQ || v.Op == ssa.OpAMD64ROLL || v.Op == ssa.OpAMD64ROLW || v.Op == ssa.OpAMD64ROLB || v.Op == ssa.OpAMD64RORQ || v.Op == ssa.OpAMD64RORL || v.Op == ssa.OpAMD64RORW || v.Op == ssa.OpAMD64RORB || v.Op == ssa.OpAMD64ADDSS || v.Op == ssa.OpAMD64ADDSD || v.Op == ssa.OpAMD64SUBSS || v.Op == ssa.OpAMD64SUBSD || v.Op == ssa.OpAMD64MULSS || v.Op == ssa.OpAMD64MULSD || v.Op == ssa.OpAMD64DIVSS || v.Op == ssa.OpAMD64DIVSD || v.Op == ssa.OpAMD64PXOR) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                opregreg(s, v.Op.Asm(), r, v.Args[1L].Reg());
            else if (v.Op == ssa.OpAMD64DIVQU || v.Op == ssa.OpAMD64DIVLU || v.Op == ssa.OpAMD64DIVWU) 
                // Arg[0] (the dividend) is in AX.
                // Arg[1] (the divisor) can be in any other register.
                // Result[0] (the quotient) is in AX.
                // Result[1] (the remainder) is in DX.
                r = v.Args[1L].Reg(); 

                // Zero extend dividend.
                var c = s.Prog(x86.AXORL);
                c.From.Type = obj.TYPE_REG;
                c.From.Reg = x86.REG_DX;
                c.To.Type = obj.TYPE_REG;
                c.To.Reg = x86.REG_DX; 

                // Issue divide.
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
            else if (v.Op == ssa.OpAMD64DIVQ || v.Op == ssa.OpAMD64DIVL || v.Op == ssa.OpAMD64DIVW) 
                // Arg[0] (the dividend) is in AX.
                // Arg[1] (the divisor) can be in any other register.
                // Result[0] (the quotient) is in AX.
                // Result[1] (the remainder) is in DX.
                r = v.Args[1L].Reg(); 

                // CPU faults upon signed overflow, which occurs when the most
                // negative int is divided by -1. Handle divide by -1 as a special case.
                c = default;

                if (v.Op == ssa.OpAMD64DIVQ) 
                    c = s.Prog(x86.ACMPQ);
                else if (v.Op == ssa.OpAMD64DIVL) 
                    c = s.Prog(x86.ACMPL);
                else if (v.Op == ssa.OpAMD64DIVW) 
                    c = s.Prog(x86.ACMPW);
                                c.From.Type = obj.TYPE_REG;
                c.From.Reg = r;
                c.To.Type = obj.TYPE_CONST;
                c.To.Offset = -1L;
                var j1 = s.Prog(x86.AJEQ);
                j1.To.Type = obj.TYPE_BRANCH; 

                // Sign extend dividend.

                if (v.Op == ssa.OpAMD64DIVQ) 
                    s.Prog(x86.ACQO);
                else if (v.Op == ssa.OpAMD64DIVL) 
                    s.Prog(x86.ACDQ);
                else if (v.Op == ssa.OpAMD64DIVW) 
                    s.Prog(x86.ACWD);
                // Issue divide.
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r; 

                // Skip over -1 fixup code.
                var j2 = s.Prog(obj.AJMP);
                j2.To.Type = obj.TYPE_BRANCH; 

                // Issue -1 fixup code.
                // n / -1 = -n
                var n1 = s.Prog(x86.ANEGQ);
                n1.To.Type = obj.TYPE_REG;
                n1.To.Reg = x86.REG_AX; 

                // n % -1 == 0
                var n2 = s.Prog(x86.AXORL);
                n2.From.Type = obj.TYPE_REG;
                n2.From.Reg = x86.REG_DX;
                n2.To.Type = obj.TYPE_REG;
                n2.To.Reg = x86.REG_DX; 

                // TODO(khr): issue only the -1 fixup code we need.
                // For instance, if only the quotient is used, no point in zeroing the remainder.

                j1.To.Val = n1;
                j2.To.Val = s.Pc();
            else if (v.Op == ssa.OpAMD64HMULQ || v.Op == ssa.OpAMD64HMULL || v.Op == ssa.OpAMD64HMULQU || v.Op == ssa.OpAMD64HMULLU) 
                // the frontend rewrites constant division by 8/16/32 bit integers into
                // HMUL by a constant
                // SSA rewrites generate the 64 bit versions

                // Arg[0] is already in AX as it's the only register we allow
                // and DX is the only output we care about (the high bits)
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg(); 

                // IMULB puts the high portion in AH instead of DL,
                // so move it to DL for consistency
                if (v.Type.Size() == 1L)
                {
                    var m = s.Prog(x86.AMOVB);
                    m.From.Type = obj.TYPE_REG;
                    m.From.Reg = x86.REG_AH;
                    m.To.Type = obj.TYPE_REG;
                    m.To.Reg = x86.REG_DX;
                }
            else if (v.Op == ssa.OpAMD64MULQU2) 
                // Arg[0] is already in AX as it's the only register we allow
                // results hi in DX, lo in AX
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
            else if (v.Op == ssa.OpAMD64DIVQU2) 
                // Arg[0], Arg[1] are already in Dx, AX, as they're the only registers we allow
                // results q in AX, r in DX
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
            else if (v.Op == ssa.OpAMD64AVGQU) 
                // compute (x+y)/2 unsigned.
                // Do a 64-bit add, the overflow goes into the carry.
                // Shift right once and pull the carry back into the 63rd bit.
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(x86.AADDQ);
                p.From.Type = obj.TYPE_REG;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                p.From.Reg = v.Args[1L].Reg();
                p = s.Prog(x86.ARCRQ);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 1L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64ADDQconst || v.Op == ssa.OpAMD64ADDLconst) 
                r = v.Reg();
                var a = v.Args[0L].Reg();
                if (r == a)
                {
                    if (v.AuxInt == 1L)
                    {
                        asm = default; 
                        // Software optimization manual recommends add $1,reg.
                        // But inc/dec is 1 byte smaller. ICC always uses inc
                        // Clang/GCC choose depending on flags, but prefer add.
                        // Experiments show that inc/dec is both a little faster
                        // and make a binary a little smaller.
                        if (v.Op == ssa.OpAMD64ADDQconst)
                        {
                            asm = x86.AINCQ;
                        }
                        else
                        {
                            asm = x86.AINCL;
                        }
                        p = s.Prog(asm);
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = r;
                        return;
                    }
                    if (v.AuxInt == -1L)
                    {
                        asm = default;
                        if (v.Op == ssa.OpAMD64ADDQconst)
                        {
                            asm = x86.ADECQ;
                        }
                        else
                        {
                            asm = x86.ADECL;
                        }
                        p = s.Prog(asm);
                        p.To.Type = obj.TYPE_REG;
                        p.To.Reg = r;
                        return;
                    }
                    p = s.Prog(v.Op.Asm());
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = v.AuxInt;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                    return;
                }
                asm = default;
                if (v.Op == ssa.OpAMD64ADDQconst)
                {
                    asm = x86.ALEAQ;
                }
                else
                {
                    asm = x86.ALEAL;
                }
                p = s.Prog(asm);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = a;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64CMOVQEQ || v.Op == ssa.OpAMD64CMOVLEQ) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64MULQconst || v.Op == ssa.OpAMD64MULLconst) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r; 
                // TODO: Teach doasm to compile the three-address multiply imul $c, r1, r2
                // then we don't need to use resultInArg0 for these ops.
                //p.From3 = new(obj.Addr)
                //p.From3.Type = obj.TYPE_REG
                //p.From3.Reg = v.Args[0].Reg()
            else if (v.Op == ssa.OpAMD64SUBQconst || v.Op == ssa.OpAMD64SUBLconst || v.Op == ssa.OpAMD64ANDQconst || v.Op == ssa.OpAMD64ANDLconst || v.Op == ssa.OpAMD64ORQconst || v.Op == ssa.OpAMD64ORLconst || v.Op == ssa.OpAMD64XORQconst || v.Op == ssa.OpAMD64XORLconst || v.Op == ssa.OpAMD64SHLQconst || v.Op == ssa.OpAMD64SHLLconst || v.Op == ssa.OpAMD64SHRQconst || v.Op == ssa.OpAMD64SHRLconst || v.Op == ssa.OpAMD64SHRWconst || v.Op == ssa.OpAMD64SHRBconst || v.Op == ssa.OpAMD64SARQconst || v.Op == ssa.OpAMD64SARLconst || v.Op == ssa.OpAMD64SARWconst || v.Op == ssa.OpAMD64SARBconst || v.Op == ssa.OpAMD64ROLQconst || v.Op == ssa.OpAMD64ROLLconst || v.Op == ssa.OpAMD64ROLWconst || v.Op == ssa.OpAMD64ROLBconst) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64SBBQcarrymask || v.Op == ssa.OpAMD64SBBLcarrymask) 
                r = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64LEAQ1 || v.Op == ssa.OpAMD64LEAQ2 || v.Op == ssa.OpAMD64LEAQ4 || v.Op == ssa.OpAMD64LEAQ8) 
                r = v.Args[0L].Reg();
                var i = v.Args[1L].Reg();
                p = s.Prog(x86.ALEAQ);

                if (v.Op == ssa.OpAMD64LEAQ1) 
                    p.From.Scale = 1L;
                    if (i == x86.REG_SP)
                    {
                        r = i;
                        i = r;
                    }
                else if (v.Op == ssa.OpAMD64LEAQ2) 
                    p.From.Scale = 2L;
                else if (v.Op == ssa.OpAMD64LEAQ4) 
                    p.From.Scale = 4L;
                else if (v.Op == ssa.OpAMD64LEAQ8) 
                    p.From.Scale = 8L;
                                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r;
                p.From.Index = i;
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64LEAQ || v.Op == ssa.OpAMD64LEAL) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64CMPQ || v.Op == ssa.OpAMD64CMPL || v.Op == ssa.OpAMD64CMPW || v.Op == ssa.OpAMD64CMPB || v.Op == ssa.OpAMD64TESTQ || v.Op == ssa.OpAMD64TESTL || v.Op == ssa.OpAMD64TESTW || v.Op == ssa.OpAMD64TESTB || v.Op == ssa.OpAMD64BTL || v.Op == ssa.OpAMD64BTQ) 
                opregreg(s, v.Op.Asm(), v.Args[1L].Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpAMD64UCOMISS || v.Op == ssa.OpAMD64UCOMISD) 
                // Go assembler has swapped operands for UCOMISx relative to CMP,
                // must account for that right here.
                opregreg(s, v.Op.Asm(), v.Args[0L].Reg(), v.Args[1L].Reg());
            else if (v.Op == ssa.OpAMD64CMPQconst || v.Op == ssa.OpAMD64CMPLconst || v.Op == ssa.OpAMD64CMPWconst || v.Op == ssa.OpAMD64CMPBconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = v.AuxInt;
            else if (v.Op == ssa.OpAMD64TESTQconst || v.Op == ssa.OpAMD64TESTLconst || v.Op == ssa.OpAMD64TESTWconst || v.Op == ssa.OpAMD64TESTBconst || v.Op == ssa.OpAMD64BTLconst || v.Op == ssa.OpAMD64BTQconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Args[0L].Reg();
            else if (v.Op == ssa.OpAMD64MOVLconst || v.Op == ssa.OpAMD64MOVQconst) 
                var x = v.Reg(); 

                // If flags aren't live (indicated by v.Aux == nil),
                // then we can rewrite MOV $0, AX into XOR AX, AX.
                if (v.AuxInt == 0L && v.Aux == null)
                {
                    p = s.Prog(x86.AXORL);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = x;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = x;
                    break;
                }
                asm = v.Op.Asm(); 
                // Use MOVL to move a small constant into a register
                // when the constant is positive and fits into 32 bits.
                if (0L <= v.AuxInt && v.AuxInt <= (1L << (int)(32L) - 1L))
                { 
                    // The upper 32bit are zeroed automatically when using MOVL.
                    asm = x86.AMOVL;
                }
                p = s.Prog(asm);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = x;
            else if (v.Op == ssa.OpAMD64MOVSSconst || v.Op == ssa.OpAMD64MOVSDconst) 
                x = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = x;
            else if (v.Op == ssa.OpAMD64MOVQload || v.Op == ssa.OpAMD64MOVSSload || v.Op == ssa.OpAMD64MOVSDload || v.Op == ssa.OpAMD64MOVLload || v.Op == ssa.OpAMD64MOVWload || v.Op == ssa.OpAMD64MOVBload || v.Op == ssa.OpAMD64MOVBQSXload || v.Op == ssa.OpAMD64MOVWQSXload || v.Op == ssa.OpAMD64MOVLQSXload || v.Op == ssa.OpAMD64MOVOload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVQloadidx8 || v.Op == ssa.OpAMD64MOVSDloadidx8 || v.Op == ssa.OpAMD64MOVLloadidx8) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.From.Scale = 8L;
                p.From.Index = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVLloadidx4 || v.Op == ssa.OpAMD64MOVSSloadidx4) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.From.Scale = 4L;
                p.From.Index = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVWloadidx2) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.From.Scale = 2L;
                p.From.Index = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVBloadidx1 || v.Op == ssa.OpAMD64MOVWloadidx1 || v.Op == ssa.OpAMD64MOVLloadidx1 || v.Op == ssa.OpAMD64MOVQloadidx1 || v.Op == ssa.OpAMD64MOVSSloadidx1 || v.Op == ssa.OpAMD64MOVSDloadidx1) 
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();
                if (i == x86.REG_SP)
                {
                    r = i;
                    i = r;
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r;
                p.From.Scale = 1L;
                p.From.Index = i;
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVQstore || v.Op == ssa.OpAMD64MOVSSstore || v.Op == ssa.OpAMD64MOVSDstore || v.Op == ssa.OpAMD64MOVLstore || v.Op == ssa.OpAMD64MOVWstore || v.Op == ssa.OpAMD64MOVBstore || v.Op == ssa.OpAMD64MOVOstore) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64MOVQstoreidx8 || v.Op == ssa.OpAMD64MOVSDstoreidx8 || v.Op == ssa.OpAMD64MOVLstoreidx8) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Scale = 8L;
                p.To.Index = v.Args[1L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64MOVSSstoreidx4 || v.Op == ssa.OpAMD64MOVLstoreidx4) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Scale = 4L;
                p.To.Index = v.Args[1L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64MOVWstoreidx2) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                p.To.Scale = 2L;
                p.To.Index = v.Args[1L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64MOVBstoreidx1 || v.Op == ssa.OpAMD64MOVWstoreidx1 || v.Op == ssa.OpAMD64MOVLstoreidx1 || v.Op == ssa.OpAMD64MOVQstoreidx1 || v.Op == ssa.OpAMD64MOVSSstoreidx1 || v.Op == ssa.OpAMD64MOVSDstoreidx1) 
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();
                if (i == x86.REG_SP)
                {
                    r = i;
                    i = r;
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = r;
                p.To.Scale = 1L;
                p.To.Index = i;
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64ADDQconstmem || v.Op == ssa.OpAMD64ADDLconstmem) 
                var sc = v.AuxValAndOff();
                var off = sc.Off();
                var val = sc.Val();
                if (val == 1L)
                {
                    asm = default;
                    if (v.Op == ssa.OpAMD64ADDQconstmem)
                    {
                        asm = x86.AINCQ;
                    }
                    else
                    {
                        asm = x86.AINCL;
                    }
                    p = s.Prog(asm);
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Reg = v.Args[0L].Reg();
                    gc.AddAux2(ref p.To, v, off);
                }
                else
                {
                    p = s.Prog(v.Op.Asm());
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = val;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Reg = v.Args[0L].Reg();
                    gc.AddAux2(ref p.To, v, off);
                }
            else if (v.Op == ssa.OpAMD64MOVQstoreconst || v.Op == ssa.OpAMD64MOVLstoreconst || v.Op == ssa.OpAMD64MOVWstoreconst || v.Op == ssa.OpAMD64MOVBstoreconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                sc = v.AuxValAndOff();
                p.From.Offset = sc.Val();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux2(ref p.To, v, sc.Off());
            else if (v.Op == ssa.OpAMD64MOVQstoreconstidx1 || v.Op == ssa.OpAMD64MOVQstoreconstidx8 || v.Op == ssa.OpAMD64MOVLstoreconstidx1 || v.Op == ssa.OpAMD64MOVLstoreconstidx4 || v.Op == ssa.OpAMD64MOVWstoreconstidx1 || v.Op == ssa.OpAMD64MOVWstoreconstidx2 || v.Op == ssa.OpAMD64MOVBstoreconstidx1) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                sc = v.AuxValAndOff();
                p.From.Offset = sc.Val();
                r = v.Args[0L].Reg();
                i = v.Args[1L].Reg();

                if (v.Op == ssa.OpAMD64MOVBstoreconstidx1 || v.Op == ssa.OpAMD64MOVWstoreconstidx1 || v.Op == ssa.OpAMD64MOVLstoreconstidx1 || v.Op == ssa.OpAMD64MOVQstoreconstidx1) 
                    p.To.Scale = 1L;
                    if (i == x86.REG_SP)
                    {
                        r = i;
                        i = r;
                    }
                else if (v.Op == ssa.OpAMD64MOVWstoreconstidx2) 
                    p.To.Scale = 2L;
                else if (v.Op == ssa.OpAMD64MOVLstoreconstidx4) 
                    p.To.Scale = 4L;
                else if (v.Op == ssa.OpAMD64MOVQstoreconstidx8) 
                    p.To.Scale = 8L;
                                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = r;
                p.To.Index = i;
                gc.AddAux2(ref p.To, v, sc.Off());
            else if (v.Op == ssa.OpAMD64MOVLQSX || v.Op == ssa.OpAMD64MOVWQSX || v.Op == ssa.OpAMD64MOVBQSX || v.Op == ssa.OpAMD64MOVLQZX || v.Op == ssa.OpAMD64MOVWQZX || v.Op == ssa.OpAMD64MOVBQZX || v.Op == ssa.OpAMD64CVTTSS2SL || v.Op == ssa.OpAMD64CVTTSD2SL || v.Op == ssa.OpAMD64CVTTSS2SQ || v.Op == ssa.OpAMD64CVTTSD2SQ || v.Op == ssa.OpAMD64CVTSS2SD || v.Op == ssa.OpAMD64CVTSD2SS) 
                opregreg(s, v.Op.Asm(), v.Reg(), v.Args[0L].Reg());
            else if (v.Op == ssa.OpAMD64CVTSL2SD || v.Op == ssa.OpAMD64CVTSQ2SD || v.Op == ssa.OpAMD64CVTSQ2SS || v.Op == ssa.OpAMD64CVTSL2SS) 
                r = v.Reg(); 
                // Break false dependency on destination register.
                opregreg(s, x86.AXORPS, r, r);
                opregreg(s, v.Op.Asm(), r, v.Args[0L].Reg());
            else if (v.Op == ssa.OpAMD64MOVQi2f || v.Op == ssa.OpAMD64MOVQf2i) 
                p = s.Prog(x86.AMOVQ);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64MOVLi2f || v.Op == ssa.OpAMD64MOVLf2i) 
                p = s.Prog(x86.AMOVL);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64ADDQmem || v.Op == ssa.OpAMD64ADDLmem || v.Op == ssa.OpAMD64SUBQmem || v.Op == ssa.OpAMD64SUBLmem || v.Op == ssa.OpAMD64ANDQmem || v.Op == ssa.OpAMD64ANDLmem || v.Op == ssa.OpAMD64ORQmem || v.Op == ssa.OpAMD64ORLmem || v.Op == ssa.OpAMD64XORQmem || v.Op == ssa.OpAMD64XORLmem || v.Op == ssa.OpAMD64ADDSDmem || v.Op == ssa.OpAMD64ADDSSmem || v.Op == ssa.OpAMD64SUBSDmem || v.Op == ssa.OpAMD64SUBSSmem || v.Op == ssa.OpAMD64MULSDmem || v.Op == ssa.OpAMD64MULSSmem) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[1L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                if (v.Reg() != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
            else if (v.Op == ssa.OpAMD64DUFFZERO) 
                off = duffStart(v.AuxInt);
                var adj = duffAdj(v.AuxInt);
                p = default;
                if (adj != 0L)
                {
                    p = s.Prog(x86.ALEAQ);
                    p.From.Type = obj.TYPE_MEM;
                    p.From.Offset = adj;
                    p.From.Reg = x86.REG_DI;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = x86.REG_DI;
                }
                p = s.Prog(obj.ADUFFZERO);
                p.To.Type = obj.TYPE_ADDR;
                p.To.Sym = gc.Duffzero;
                p.To.Offset = off;
            else if (v.Op == ssa.OpAMD64MOVOconst) 
                if (v.AuxInt != 0L)
                {
                    v.Fatalf("MOVOconst can only do constant=0");
                }
                r = v.Reg();
                opregreg(s, x86.AXORPS, r, r);
            else if (v.Op == ssa.OpAMD64DUFFCOPY) 
                p = s.Prog(obj.ADUFFCOPY);
                p.To.Type = obj.TYPE_ADDR;
                p.To.Sym = gc.Duffcopy;
                p.To.Offset = v.AuxInt;
            else if (v.Op == ssa.OpAMD64MOVQconvert || v.Op == ssa.OpAMD64MOVLconvert) 
                if (v.Args[0L].Reg() != v.Reg())
                {
                    v.Fatalf("MOVXconvert should be a no-op");
                }
            else if (v.Op == ssa.OpCopy) // TODO: use MOVQreg for reg->reg copies instead of OpCopy?
                if (v.Type.IsMemory())
                {
                    return;
                }
                x = v.Args[0L].Reg();
                var y = v.Reg();
                if (x != y)
                {
                    opregreg(s, moveByType(v.Type), y, x);
                }
            else if (v.Op == ssa.OpLoadReg) 
                if (v.Type.IsFlags())
                {
                    v.Fatalf("load flags not implemented: %v", v.LongString());
                    return;
                }
                p = s.Prog(loadByType(v.Type));
                gc.AddrAuto(ref p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpStoreReg) 
                if (v.Type.IsFlags())
                {
                    v.Fatalf("store flags not implemented: %v", v.LongString());
                    return;
                }
                p = s.Prog(storeByType(v.Type));
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddrAuto(ref p.To, v);
            else if (v.Op == ssa.OpAMD64LoweredGetClosurePtr) 
                // Closure pointer is DX.
                gc.CheckLoweredGetClosurePtr(v);
            else if (v.Op == ssa.OpAMD64LoweredGetG) 
                r = v.Reg(); 
                // See the comments in cmd/internal/obj/x86/obj6.go
                // near CanUse1InsnTLS for a detailed explanation of these instructions.
                if (x86.CanUse1InsnTLS(gc.Ctxt))
                { 
                    // MOVQ (TLS), r
                    p = s.Prog(x86.AMOVQ);
                    p.From.Type = obj.TYPE_MEM;
                    p.From.Reg = x86.REG_TLS;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                }
                else
                { 
                    // MOVQ TLS, r
                    // MOVQ (r)(TLS*1), r
                    p = s.Prog(x86.AMOVQ);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = x86.REG_TLS;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = r;
                    var q = s.Prog(x86.AMOVQ);
                    q.From.Type = obj.TYPE_MEM;
                    q.From.Reg = r;
                    q.From.Index = x86.REG_TLS;
                    q.From.Scale = 1L;
                    q.To.Type = obj.TYPE_REG;
                    q.To.Reg = r;
                }
            else if (v.Op == ssa.OpAMD64CALLstatic || v.Op == ssa.OpAMD64CALLclosure || v.Op == ssa.OpAMD64CALLinter) 
                s.Call(v);
            else if (v.Op == ssa.OpAMD64LoweredGetCallerPC) 
                p = s.Prog(x86.AMOVQ);
                p.From.Type = obj.TYPE_MEM;
                p.From.Offset = -8L; // PC is stored 8 bytes below first parameter.
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64LoweredGetCallerSP) 
                // caller's SP is the address of the first arg
                var mov = x86.AMOVQ;
                if (gc.Widthptr == 4L)
                {
                    mov = x86.AMOVL;
                }
                p = s.Prog(mov);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize(); // 0 on amd64, just to be consistent with other architectures
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64LoweredWB) 
                p = s.Prog(obj.ACALL);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = v.Aux._<ref obj.LSym>();
            else if (v.Op == ssa.OpAMD64NEGQ || v.Op == ssa.OpAMD64NEGL || v.Op == ssa.OpAMD64BSWAPQ || v.Op == ssa.OpAMD64BSWAPL || v.Op == ssa.OpAMD64NOTQ || v.Op == ssa.OpAMD64NOTL) 
                r = v.Reg();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpAMD64BSFQ || v.Op == ssa.OpAMD64BSFL || v.Op == ssa.OpAMD64BSRQ || v.Op == ssa.OpAMD64BSRL) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
            else if (v.Op == ssa.OpAMD64SQRTSD) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64ROUNDSD) 
                p = s.Prog(v.Op.Asm());
                val = v.AuxInt; 
                // 0 means math.RoundToEven, 1 Floor, 2 Ceil, 3 Trunc
                if (val != 0L && val != 1L && val != 2L && val != 3L)
                {
                    v.Fatalf("Invalid rounding mode");
                }
                p.From.Offset = val;
                p.From.Type = obj.TYPE_CONST;
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:v.Args[0].Reg()));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64POPCNTQ || v.Op == ssa.OpAMD64POPCNTL) 
                if (v.Args[0L].Reg() != v.Reg())
                { 
                    // POPCNT on Intel has a false dependency on the destination register.
                    // Zero the destination to break the dependency.
                    p = s.Prog(x86.AMOVQ);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 0L;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = v.Reg();
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64SETEQ || v.Op == ssa.OpAMD64SETNE || v.Op == ssa.OpAMD64SETL || v.Op == ssa.OpAMD64SETLE || v.Op == ssa.OpAMD64SETG || v.Op == ssa.OpAMD64SETGE || v.Op == ssa.OpAMD64SETGF || v.Op == ssa.OpAMD64SETGEF || v.Op == ssa.OpAMD64SETB || v.Op == ssa.OpAMD64SETBE || v.Op == ssa.OpAMD64SETORD || v.Op == ssa.OpAMD64SETNAN || v.Op == ssa.OpAMD64SETA || v.Op == ssa.OpAMD64SETAE) 
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpAMD64SETEQmem || v.Op == ssa.OpAMD64SETNEmem || v.Op == ssa.OpAMD64SETLmem || v.Op == ssa.OpAMD64SETLEmem || v.Op == ssa.OpAMD64SETGmem || v.Op == ssa.OpAMD64SETGEmem || v.Op == ssa.OpAMD64SETBmem || v.Op == ssa.OpAMD64SETBEmem || v.Op == ssa.OpAMD64SETAmem || v.Op == ssa.OpAMD64SETAEmem) 
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64SETNEF) 
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                q = s.Prog(x86.ASETPS);
                q.To.Type = obj.TYPE_REG;
                q.To.Reg = x86.REG_AX; 
                // ORL avoids partial register write and is smaller than ORQ, used by old compiler
                opregreg(s, x86.AORL, v.Reg(), x86.REG_AX);
            else if (v.Op == ssa.OpAMD64SETEQF) 
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                q = s.Prog(x86.ASETPC);
                q.To.Type = obj.TYPE_REG;
                q.To.Reg = x86.REG_AX; 
                // ANDL avoids partial register write and is smaller than ANDQ, used by old compiler
                opregreg(s, x86.AANDL, v.Reg(), x86.REG_AX);
            else if (v.Op == ssa.OpAMD64InvertFlags) 
                v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpAMD64FlagEQ || v.Op == ssa.OpAMD64FlagLT_ULT || v.Op == ssa.OpAMD64FlagLT_UGT || v.Op == ssa.OpAMD64FlagGT_ULT || v.Op == ssa.OpAMD64FlagGT_UGT) 
                v.Fatalf("Flag* ops should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpAMD64AddTupleFirst32 || v.Op == ssa.OpAMD64AddTupleFirst64) 
                v.Fatalf("AddTupleFirst* should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpAMD64REPSTOSQ) 
                s.Prog(x86.AREP);
                s.Prog(x86.ASTOSQ);
            else if (v.Op == ssa.OpAMD64REPMOVSQ) 
                s.Prog(x86.AREP);
                s.Prog(x86.AMOVSQ);
            else if (v.Op == ssa.OpAMD64LoweredNilCheck) 
                // Issue a load which will fault if the input is nil.
                // TODO: We currently use the 2-byte instruction TESTB AX, (reg).
                // Should we use the 3-byte TESTB $0, (reg) instead? It is larger
                // but it doesn't have false dependency on AX.
                // Or maybe allocate an output register and use MOVL (reg),reg2 ?
                // That trades clobbering flags for clobbering a register.
                p = s.Prog(x86.ATESTB);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = x86.REG_AX;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");
                }
            else if (v.Op == ssa.OpAMD64MOVLatomicload || v.Op == ssa.OpAMD64MOVQatomicload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
            else if (v.Op == ssa.OpAMD64XCHGL || v.Op == ssa.OpAMD64XCHGQ) 
                r = v.Reg0();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output[0] not in same register %s", v.LongString());
                }
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[1L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64XADDLlock || v.Op == ssa.OpAMD64XADDQlock) 
                r = v.Reg0();
                if (r != v.Args[0L].Reg())
                {
                    v.Fatalf("input[0] and output[0] not in same register %s", v.LongString());
                }
                s.Prog(x86.ALOCK);
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[1L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpAMD64CMPXCHGLlock || v.Op == ssa.OpAMD64CMPXCHGQlock) 
                if (v.Args[1L].Reg() != x86.REG_AX)
                {
                    v.Fatalf("input[1] not in AX %s", v.LongString());
                }
                s.Prog(x86.ALOCK);
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[2L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
                p = s.Prog(x86.ASETEQ);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg0();
            else if (v.Op == ssa.OpAMD64ANDBlock || v.Op == ssa.OpAMD64ORBlock) 
                s.Prog(x86.ALOCK);
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpClobber) 
                p = s.Prog(x86.AMOVL);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 0xdeaddeadUL;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = x86.REG_SP;
                gc.AddAux(ref p.To, v);
                p = s.Prog(x86.AMOVL);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 0xdeaddeadUL;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = x86.REG_SP;
                gc.AddAux(ref p.To, v);
                p.To.Offset += 4L;
            else 
                v.Fatalf("genValue not implemented: %s", v.LongString());
                    }



        private static array<array<gc.FloatingEQNEJump>> eqfJumps = new array<array<gc.FloatingEQNEJump>>(new array<gc.FloatingEQNEJump>[] { {{Jump:x86.AJNE,Index:1},{Jump:x86.AJPS,Index:1}}, {{Jump:x86.AJNE,Index:1},{Jump:x86.AJPC,Index:0}} });
        private static array<array<gc.FloatingEQNEJump>> nefJumps = new array<array<gc.FloatingEQNEJump>>(new array<gc.FloatingEQNEJump>[] { {{Jump:x86.AJNE,Index:0},{Jump:x86.AJPC,Index:1}}, {{Jump:x86.AJNE,Index:0},{Jump:x86.AJPS,Index:0}} });

        private static void ssaGenBlock(ref gc.SSAGenState s, ref ssa.Block b, ref ssa.Block next)
        {

            if (b.Kind == ssa.BlockPlain) 
                if (b.Succs[0L].Block() != next)
                {
                    var p = s.Prog(obj.AJMP);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }
            else if (b.Kind == ssa.BlockDefer) 
                // defer returns in rax:
                // 0 if we should continue executing
                // 1 if we should jump to deferreturn call
                p = s.Prog(x86.ATESTL);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = x86.REG_AX;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = x86.REG_AX;
                p = s.Prog(x86.AJNE);
                p.To.Type = obj.TYPE_BRANCH;
                s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                if (b.Succs[0L].Block() != next)
                {
                    p = s.Prog(obj.AJMP);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }
            else if (b.Kind == ssa.BlockExit) 
                s.Prog(obj.AUNDEF); // tell plive.go that we never reach here
            else if (b.Kind == ssa.BlockRet) 
                s.Prog(obj.ARET);
            else if (b.Kind == ssa.BlockRetJmp) 
                p = s.Prog(obj.AJMP);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = b.Aux._<ref obj.LSym>();
            else if (b.Kind == ssa.BlockAMD64EQF) 
                s.FPJump(b, next, ref eqfJumps);
            else if (b.Kind == ssa.BlockAMD64NEF) 
                s.FPJump(b, next, ref nefJumps);
            else if (b.Kind == ssa.BlockAMD64EQ || b.Kind == ssa.BlockAMD64NE || b.Kind == ssa.BlockAMD64LT || b.Kind == ssa.BlockAMD64GE || b.Kind == ssa.BlockAMD64LE || b.Kind == ssa.BlockAMD64GT || b.Kind == ssa.BlockAMD64ULT || b.Kind == ssa.BlockAMD64UGT || b.Kind == ssa.BlockAMD64ULE || b.Kind == ssa.BlockAMD64UGE) 
                var jmp = blockJump[b.Kind];
                p = default;

                if (next == b.Succs[0L].Block()) 
                    p = s.Prog(jmp.invasm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                else if (next == b.Succs[1L].Block()) 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                else 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                    var q = s.Prog(obj.AJMP);
                    q.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[1].Block()));
                            else 
                b.Fatalf("branch not implemented: %s. Control: %s", b.LongString(), b.Control.LongString());
                    }
    }
}}}}
