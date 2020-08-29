// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2020 August 29 09:25:09 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Go\src\cmd\compile\internal\ppc64\ssa.go
using gc = go.cmd.compile.@internal.gc_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using ppc64 = go.cmd.@internal.obj.ppc64_package;
using math = go.math_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ppc64_package
    {
        // iselOp encodes mapping of comparison operations onto ISEL operands
        private partial struct iselOp
        {
            public long cond;
            public long valueIfCond; // if cond is true, the value to return (0 or 1)
        }

        // Input registers to ISEL used for comparison. Index 0 is zero, 1 is (will be) 1
        private static array<short> iselRegs = new array<short>(new short[] { ppc64.REG_R0, ppc64.REGTMP });

        private static map iselOps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, iselOp>{ssa.OpPPC64Equal:iselOp{cond:ppc64.C_COND_EQ,valueIfCond:1},ssa.OpPPC64NotEqual:iselOp{cond:ppc64.C_COND_EQ,valueIfCond:0},ssa.OpPPC64LessThan:iselOp{cond:ppc64.C_COND_LT,valueIfCond:1},ssa.OpPPC64GreaterEqual:iselOp{cond:ppc64.C_COND_LT,valueIfCond:0},ssa.OpPPC64GreaterThan:iselOp{cond:ppc64.C_COND_GT,valueIfCond:1},ssa.OpPPC64LessEqual:iselOp{cond:ppc64.C_COND_GT,valueIfCond:0},ssa.OpPPC64FLessThan:iselOp{cond:ppc64.C_COND_LT,valueIfCond:1},ssa.OpPPC64FGreaterThan:iselOp{cond:ppc64.C_COND_GT,valueIfCond:1},ssa.OpPPC64FLessEqual:iselOp{cond:ppc64.C_COND_LT,valueIfCond:1},ssa.OpPPC64FGreaterEqual:iselOp{cond:ppc64.C_COND_GT,valueIfCond:1},};

        // markMoves marks any MOVXconst ops that need to avoid clobbering flags.
        private static void ssaMarkMoves(ref gc.SSAGenState s, ref ssa.Block b)
        { 
            //    flive := b.FlagsLiveAtEnd
            //    if b.Control != nil && b.Control.Type.IsFlags() {
            //        flive = true
            //    }
            //    for i := len(b.Values) - 1; i >= 0; i-- {
            //        v := b.Values[i]
            //        if flive && (v.Op == v.Op == ssa.OpPPC64MOVDconst) {
            //            // The "mark" is any non-nil Aux value.
            //            v.Aux = v
            //        }
            //        if v.Type.IsFlags() {
            //            flive = false
            //        }
            //        for _, a := range v.Args {
            //            if a.Type.IsFlags() {
            //                flive = true
            //            }
            //        }
            //    }
        }

        // loadByType returns the load instruction of the given type.
        private static obj.As loadByType(ref types.Type _t) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return ppc64.AFMOVS;
                        break;
                    case 8L: 
                        return ppc64.AFMOVD;
                        break;
                }
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        if (t.IsSigned())
                        {
                            return ppc64.AMOVB;
                        }
                        else
                        {
                            return ppc64.AMOVBZ;
                        }
                        break;
                    case 2L: 
                        if (t.IsSigned())
                        {
                            return ppc64.AMOVH;
                        }
                        else
                        {
                            return ppc64.AMOVHZ;
                        }
                        break;
                    case 4L: 
                        if (t.IsSigned())
                        {
                            return ppc64.AMOVW;
                        }
                        else
                        {
                            return ppc64.AMOVWZ;
                        }
                        break;
                    case 8L: 
                        return ppc64.AMOVD;
                        break;
                }
            }
            panic("bad load type");
        });

        // storeByType returns the store instruction of the given type.
        private static obj.As storeByType(ref types.Type _t) => func(_t, (ref types.Type t, Defer _, Panic panic, Recover __) =>
        {
            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return ppc64.AFMOVS;
                        break;
                    case 8L: 
                        return ppc64.AFMOVD;
                        break;
                }
            }
            else
            {
                switch (t.Size())
                {
                    case 1L: 
                        return ppc64.AMOVB;
                        break;
                    case 2L: 
                        return ppc64.AMOVH;
                        break;
                    case 4L: 
                        return ppc64.AMOVW;
                        break;
                    case 8L: 
                        return ppc64.AMOVD;
                        break;
                }
            }
            panic("bad store type");
        });

        private static void ssaGenISEL(ref gc.SSAGenState s, ref ssa.Value v, long cr, short r1, short r2)
        {
            var r = v.Reg();
            var p = s.Prog(ppc64.AISEL);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = r;
            p.Reg = r1;
            p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:r2));
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = cr;
        }

        private static void ssaGenValue(ref gc.SSAGenState s, ref ssa.Value v)
        {

            if (v.Op == ssa.OpCopy || v.Op == ssa.OpPPC64MOVDconvert) 
                var t = v.Type;
                if (t.IsMemory())
                {
                    return;
                }
                var x = v.Args[0L].Reg();
                var y = v.Reg();
                if (x != y)
                {
                    var rt = obj.TYPE_REG;
                    var op = ppc64.AMOVD;

                    if (t.IsFloat())
                    {
                        op = ppc64.AFMOVD;
                    }
                    var p = s.Prog(op);
                    p.From.Type = rt;
                    p.From.Reg = x;
                    p.To.Type = rt;
                    p.To.Reg = y;
                }
            else if (v.Op == ssa.OpPPC64LoweredAtomicAnd8 || v.Op == ssa.OpPPC64LoweredAtomicOr8) 
                // SYNC
                // LBAR        (Rarg0), Rtmp
                // AND/OR    Rarg1, Rtmp
                // STBCCC    Rtmp, (Rarg0)
                // BNE        -3(PC)
                // ISYNC
                var r0 = v.Args[0L].Reg();
                var r1 = v.Args[1L].Reg();
                var psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE;
                p = s.Prog(ppc64.ALBAR);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP;
                var p1 = s.Prog(v.Op.Asm());
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = ppc64.REGTMP;
                var p2 = s.Prog(ppc64.ASTBCCC);
                p2.From.Type = obj.TYPE_REG;
                p2.From.Reg = ppc64.REGTMP;
                p2.To.Type = obj.TYPE_MEM;
                p2.To.Reg = r0;
                p2.RegTo2 = ppc64.REGTMP;
                var p3 = s.Prog(ppc64.ABNE);
                p3.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p3, p);
                var pisync = s.Prog(ppc64.AISYNC);
                pisync.To.Type = obj.TYPE_NONE;
            else if (v.Op == ssa.OpPPC64LoweredAtomicAdd32 || v.Op == ssa.OpPPC64LoweredAtomicAdd64) 
                // SYNC
                // LDAR/LWAR    (Rarg0), Rout
                // ADD        Rarg1, Rout
                // STDCCC/STWCCC Rout, (Rarg0)
                // BNE         -3(PC)
                // ISYNC
                // MOVW        Rout,Rout (if Add32)
                var ld = ppc64.ALDAR;
                var st = ppc64.ASTDCCC;
                if (v.Op == ssa.OpPPC64LoweredAtomicAdd32)
                {
                    ld = ppc64.ALWAR;
                    st = ppc64.ASTWCCC;
                }
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                var @out = v.Reg0(); 
                // SYNC
                psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE; 
                // LDAR or LWAR
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out; 
                // ADD reg1,out
                p1 = s.Prog(ppc64.AADD);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Reg = out;
                p1.To.Type = obj.TYPE_REG; 
                // STDCCC or STWCCC
                p3 = s.Prog(st);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = out;
                p3.To.Type = obj.TYPE_MEM;
                p3.To.Reg = r0; 
                // BNE retry
                var p4 = s.Prog(ppc64.ABNE);
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p); 
                // ISYNC
                pisync = s.Prog(ppc64.AISYNC);
                pisync.To.Type = obj.TYPE_NONE; 

                // Ensure a 32 bit result
                if (v.Op == ssa.OpPPC64LoweredAtomicAdd32)
                {
                    var p5 = s.Prog(ppc64.AMOVWZ);
                    p5.To.Type = obj.TYPE_REG;
                    p5.To.Reg = out;
                    p5.From.Type = obj.TYPE_REG;
                    p5.From.Reg = out;
                }
            else if (v.Op == ssa.OpPPC64LoweredAtomicExchange32 || v.Op == ssa.OpPPC64LoweredAtomicExchange64) 
                // SYNC
                // LDAR/LWAR    (Rarg0), Rout
                // STDCCC/STWCCC Rout, (Rarg0)
                // BNE         -2(PC)
                // ISYNC
                ld = ppc64.ALDAR;
                st = ppc64.ASTDCCC;
                if (v.Op == ssa.OpPPC64LoweredAtomicExchange32)
                {
                    ld = ppc64.ALWAR;
                    st = ppc64.ASTWCCC;
                }
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                @out = v.Reg0(); 
                // SYNC
                psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE; 
                // LDAR or LWAR
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out; 
                // STDCCC or STWCCC
                p1 = s.Prog(st);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Type = obj.TYPE_MEM;
                p1.To.Reg = r0; 
                // BNE retry
                p2 = s.Prog(ppc64.ABNE);
                p2.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p2, p); 
                // ISYNC
                pisync = s.Prog(ppc64.AISYNC);
                pisync.To.Type = obj.TYPE_NONE;
            else if (v.Op == ssa.OpPPC64LoweredAtomicLoad32 || v.Op == ssa.OpPPC64LoweredAtomicLoad64 || v.Op == ssa.OpPPC64LoweredAtomicLoadPtr) 
                // SYNC
                // MOVD/MOVW (Rarg0), Rout
                // CMP Rout,Rout
                // BNE 1(PC)
                // ISYNC
                ld = ppc64.AMOVD;
                var cmp = ppc64.ACMP;
                if (v.Op == ssa.OpPPC64LoweredAtomicLoad32)
                {
                    ld = ppc64.AMOVW;
                    cmp = ppc64.ACMPW;
                }
                var arg0 = v.Args[0L].Reg();
                @out = v.Reg0(); 
                // SYNC
                psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE; 
                // Load
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = arg0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = out; 
                // CMP
                p1 = s.Prog(cmp);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = out;
                p1.To.Type = obj.TYPE_REG;
                p1.To.Reg = out; 
                // BNE
                p2 = s.Prog(ppc64.ABNE);
                p2.To.Type = obj.TYPE_BRANCH; 
                // ISYNC
                pisync = s.Prog(ppc64.AISYNC);
                pisync.To.Type = obj.TYPE_NONE;
                gc.Patch(p2, pisync);
            else if (v.Op == ssa.OpPPC64LoweredAtomicStore32 || v.Op == ssa.OpPPC64LoweredAtomicStore64) 
                // SYNC
                // MOVD/MOVW arg1,(arg0)
                st = ppc64.AMOVD;
                if (v.Op == ssa.OpPPC64LoweredAtomicStore32)
                {
                    st = ppc64.AMOVW;
                }
                arg0 = v.Args[0L].Reg();
                var arg1 = v.Args[1L].Reg(); 
                // SYNC
                psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE; 
                // Store
                p = s.Prog(st);
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = arg0;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = arg1;
            else if (v.Op == ssa.OpPPC64LoweredAtomicCas64 || v.Op == ssa.OpPPC64LoweredAtomicCas32) 
                // SYNC
                // loop:
                // LDAR        (Rarg0), Rtmp
                // CMP         Rarg1, Rtmp
                // BNE         fail
                // STDCCC      Rarg2, (Rarg0)
                // BNE         loop
                // ISYNC
                // MOVD        $1, Rout
                // BR          end
                // fail:
                // MOVD        $0, Rout
                // end:
                ld = ppc64.ALDAR;
                st = ppc64.ASTDCCC;
                cmp = ppc64.ACMP;
                if (v.Op == ssa.OpPPC64LoweredAtomicCas32)
                {
                    ld = ppc64.ALWAR;
                    st = ppc64.ASTWCCC;
                    cmp = ppc64.ACMPW;
                }
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();
                var r2 = v.Args[2L].Reg();
                @out = v.Reg0(); 
                // SYNC
                psync = s.Prog(ppc64.ASYNC);
                psync.To.Type = obj.TYPE_NONE; 
                // LDAR or LWAR
                p = s.Prog(ld);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP; 
                // CMP reg1,reg2
                p1 = s.Prog(cmp);
                p1.From.Type = obj.TYPE_REG;
                p1.From.Reg = r1;
                p1.To.Reg = ppc64.REGTMP;
                p1.To.Type = obj.TYPE_REG; 
                // BNE cas_fail
                p2 = s.Prog(ppc64.ABNE);
                p2.To.Type = obj.TYPE_BRANCH; 
                // STDCCC or STWCCC
                p3 = s.Prog(st);
                p3.From.Type = obj.TYPE_REG;
                p3.From.Reg = r2;
                p3.To.Type = obj.TYPE_MEM;
                p3.To.Reg = r0; 
                // BNE retry
                p4 = s.Prog(ppc64.ABNE);
                p4.To.Type = obj.TYPE_BRANCH;
                gc.Patch(p4, p); 
                // ISYNC
                pisync = s.Prog(ppc64.AISYNC);
                pisync.To.Type = obj.TYPE_NONE; 
                // return true
                p5 = s.Prog(ppc64.AMOVD);
                p5.From.Type = obj.TYPE_CONST;
                p5.From.Offset = 1L;
                p5.To.Type = obj.TYPE_REG;
                p5.To.Reg = out; 
                // BR done
                var p6 = s.Prog(obj.AJMP);
                p6.To.Type = obj.TYPE_BRANCH; 
                // return false
                var p7 = s.Prog(ppc64.AMOVD);
                p7.From.Type = obj.TYPE_CONST;
                p7.From.Offset = 0L;
                p7.To.Type = obj.TYPE_REG;
                p7.To.Reg = out;
                gc.Patch(p2, p7); 
                // done (label)
                var p8 = s.Prog(obj.ANOP);
                gc.Patch(p6, p8);
            else if (v.Op == ssa.OpPPC64LoweredGetClosurePtr) 
                // Closure pointer is R11 (already)
                gc.CheckLoweredGetClosurePtr(v);
            else if (v.Op == ssa.OpPPC64LoweredGetCallerSP) 
                // caller's SP is FixedFrameSize below the address of the first arg
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Offset = -gc.Ctxt.FixedFrameSize();
                p.From.Name = obj.NAME_PARAM;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64LoweredRound32F || v.Op == ssa.OpPPC64LoweredRound64F)             else if (v.Op == ssa.OpLoadReg) 
                var loadOp = loadByType(v.Type);
                p = s.Prog(loadOp);
                gc.AddrAuto(ref p.From, v.Args[0L]);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpStoreReg) 
                var storeOp = storeByType(v.Type);
                p = s.Prog(storeOp);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddrAuto(ref p.To, v);
            else if (v.Op == ssa.OpPPC64DIVD) 
                // For now,
                //
                // cmp arg1, -1
                // be  ahead
                // v = arg0 / arg1
                // b over
                // ahead: v = - arg0
                // over: nop
                var r = v.Reg();
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();

                p = s.Prog(ppc64.ACMP);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = -1L;

                var pbahead = s.Prog(ppc64.ABEQ);
                pbahead.To.Type = obj.TYPE_BRANCH;

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;

                var pbover = s.Prog(obj.AJMP);
                pbover.To.Type = obj.TYPE_BRANCH;

                p = s.Prog(ppc64.ANEG);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r0;
                gc.Patch(pbahead, p);

                p = s.Prog(obj.ANOP);
                gc.Patch(pbover, p);
            else if (v.Op == ssa.OpPPC64DIVW) 
                // word-width version of above
                r = v.Reg();
                r0 = v.Args[0L].Reg();
                r1 = v.Args[1L].Reg();

                p = s.Prog(ppc64.ACMPW);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = -1L;

                pbahead = s.Prog(ppc64.ABEQ);
                pbahead.To.Type = obj.TYPE_BRANCH;

                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.Reg = r0;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;

                pbover = s.Prog(obj.AJMP);
                pbover.To.Type = obj.TYPE_BRANCH;

                p = s.Prog(ppc64.ANEG);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r0;
                gc.Patch(pbahead, p);

                p = s.Prog(obj.ANOP);
                gc.Patch(pbover, p);
            else if (v.Op == ssa.OpPPC64ADD || v.Op == ssa.OpPPC64FADD || v.Op == ssa.OpPPC64FADDS || v.Op == ssa.OpPPC64SUB || v.Op == ssa.OpPPC64FSUB || v.Op == ssa.OpPPC64FSUBS || v.Op == ssa.OpPPC64MULLD || v.Op == ssa.OpPPC64MULLW || v.Op == ssa.OpPPC64DIVDU || v.Op == ssa.OpPPC64DIVWU || v.Op == ssa.OpPPC64SRAD || v.Op == ssa.OpPPC64SRAW || v.Op == ssa.OpPPC64SRD || v.Op == ssa.OpPPC64SRW || v.Op == ssa.OpPPC64SLD || v.Op == ssa.OpPPC64SLW || v.Op == ssa.OpPPC64ROTL || v.Op == ssa.OpPPC64ROTLW || v.Op == ssa.OpPPC64MULHD || v.Op == ssa.OpPPC64MULHW || v.Op == ssa.OpPPC64MULHDU || v.Op == ssa.OpPPC64MULHWU || v.Op == ssa.OpPPC64FMUL || v.Op == ssa.OpPPC64FMULS || v.Op == ssa.OpPPC64FDIV || v.Op == ssa.OpPPC64FDIVS || v.Op == ssa.OpPPC64FCPSGN || v.Op == ssa.OpPPC64AND || v.Op == ssa.OpPPC64OR || v.Op == ssa.OpPPC64ANDN || v.Op == ssa.OpPPC64ORN || v.Op == ssa.OpPPC64NOR || v.Op == ssa.OpPPC64XOR || v.Op == ssa.OpPPC64EQV) 
                r = v.Reg();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r2;
                p.Reg = r1;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpPPC64ROTLconst || v.Op == ssa.OpPPC64ROTLWconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64FMADD || v.Op == ssa.OpPPC64FMADDS || v.Op == ssa.OpPPC64FMSUB || v.Op == ssa.OpPPC64FMSUBS) 
                r = v.Reg();
                r1 = v.Args[0L].Reg();
                r2 = v.Args[1L].Reg();
                var r3 = v.Args[2L].Reg(); 
                // r = r1*r2 ± r3
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = r1;
                p.Reg = r3;
                p.SetFrom3(new obj.Addr(Type:obj.TYPE_REG,Reg:r2));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpPPC64MaskIfNotCarry) 
                r = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = ppc64.REGZERO;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
            else if (v.Op == ssa.OpPPC64ADDconstForCarry) 
                r1 = v.Args[0L].Reg();
                p = s.Prog(v.Op.Asm());
                p.Reg = r1;
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP; // Ignored; this is for the carry effect.
            else if (v.Op == ssa.OpPPC64NEG || v.Op == ssa.OpPPC64FNEG || v.Op == ssa.OpPPC64FSQRT || v.Op == ssa.OpPPC64FSQRTS || v.Op == ssa.OpPPC64FFLOOR || v.Op == ssa.OpPPC64FTRUNC || v.Op == ssa.OpPPC64FCEIL || v.Op == ssa.OpPPC64FCTIDZ || v.Op == ssa.OpPPC64FCTIWZ || v.Op == ssa.OpPPC64FCFID || v.Op == ssa.OpPPC64FCFIDS || v.Op == ssa.OpPPC64FRSP || v.Op == ssa.OpPPC64CNTLZD || v.Op == ssa.OpPPC64CNTLZW || v.Op == ssa.OpPPC64POPCNTD || v.Op == ssa.OpPPC64POPCNTW || v.Op == ssa.OpPPC64POPCNTB || v.Op == ssa.OpPPC64MFVSRD || v.Op == ssa.OpPPC64MTVSRD || v.Op == ssa.OpPPC64FABS || v.Op == ssa.OpPPC64FNABS) 
                r = v.Reg();
                p = s.Prog(v.Op.Asm());
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = r;
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
            else if (v.Op == ssa.OpPPC64ADDconst || v.Op == ssa.OpPPC64ANDconst || v.Op == ssa.OpPPC64ORconst || v.Op == ssa.OpPPC64XORconst || v.Op == ssa.OpPPC64SRADconst || v.Op == ssa.OpPPC64SRAWconst || v.Op == ssa.OpPPC64SRDconst || v.Op == ssa.OpPPC64SRWconst || v.Op == ssa.OpPPC64SLDconst || v.Op == ssa.OpPPC64SLWconst) 
                p = s.Prog(v.Op.Asm());
                p.Reg = v.Args[0L].Reg();
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64ANDCCconst) 
                p = s.Prog(v.Op.Asm());
                p.Reg = v.Args[0L].Reg();

                if (v.Aux != null)
                {
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = gc.AuxOffset(v);
                }
                else
                {
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = v.AuxInt;
                }
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP; // discard result
            else if (v.Op == ssa.OpPPC64MOVDaddr) 
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();

                @string wantreg = default; 
                // Suspect comment, copied from ARM code
                // MOVD $sym+off(base), R
                // the assembler expands it as the following:
                // - base is SP: add constant offset to SP
                //               when constant is large, tmp register (R11) may be used
                // - base is SB: load external address from constant pool (use relocation)
                switch (v.Aux.type())
                {
                    case ref obj.LSym _:
                        wantreg = "SB";
                        gc.AddAux(ref p.From, v);
                        break;
                    case ref gc.Node _:
                        wantreg = "SP";
                        gc.AddAux(ref p.From, v);
                        break;
                    case 
                        wantreg = "SP";
                        p.From.Offset = v.AuxInt;
                        break;
                    default:
                    {
                        v.Fatalf("aux is of unknown type %T", v.Aux);
                        break;
                    }
                }
                {
                    var reg = v.Args[0L].RegName();

                    if (reg != wantreg)
                    {
                        v.Fatalf("bad reg %s for symbol type %T, want %s", reg, v.Aux, wantreg);
                    }

                }
            else if (v.Op == ssa.OpPPC64MOVDconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = v.AuxInt;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64FMOVDconst || v.Op == ssa.OpPPC64FMOVSconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_FCONST;
                p.From.Val = math.Float64frombits(uint64(v.AuxInt));
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64FCMPU || v.Op == ssa.OpPPC64CMP || v.Op == ssa.OpPPC64CMPW || v.Op == ssa.OpPPC64CMPU || v.Op == ssa.OpPPC64CMPWU) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Args[1L].Reg();
            else if (v.Op == ssa.OpPPC64CMPconst || v.Op == ssa.OpPPC64CMPUconst || v.Op == ssa.OpPPC64CMPWconst || v.Op == ssa.OpPPC64CMPWUconst) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_CONST;
                p.To.Offset = v.AuxInt;
            else if (v.Op == ssa.OpPPC64MOVBreg || v.Op == ssa.OpPPC64MOVBZreg || v.Op == ssa.OpPPC64MOVHreg || v.Op == ssa.OpPPC64MOVHZreg || v.Op == ssa.OpPPC64MOVWreg || v.Op == ssa.OpPPC64MOVWZreg) 
                // Shift in register to required size
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Reg = v.Reg();
                p.To.Type = obj.TYPE_REG;
            else if (v.Op == ssa.OpPPC64MOVDload || v.Op == ssa.OpPPC64MOVWload || v.Op == ssa.OpPPC64MOVHload || v.Op == ssa.OpPPC64MOVWZload || v.Op == ssa.OpPPC64MOVBZload || v.Op == ssa.OpPPC64MOVHZload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64FMOVDload || v.Op == ssa.OpPPC64FMOVSload) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
            else if (v.Op == ssa.OpPPC64MOVDstorezero || v.Op == ssa.OpPPC64MOVWstorezero || v.Op == ssa.OpPPC64MOVHstorezero || v.Op == ssa.OpPPC64MOVBstorezero) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = ppc64.REGZERO;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpPPC64MOVDstore || v.Op == ssa.OpPPC64MOVWstore || v.Op == ssa.OpPPC64MOVHstore || v.Op == ssa.OpPPC64MOVBstore) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpPPC64FMOVDstore || v.Op == ssa.OpPPC64FMOVSstore) 
                p = s.Prog(v.Op.Asm());
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[1L].Reg();
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.To, v);
            else if (v.Op == ssa.OpPPC64Equal || v.Op == ssa.OpPPC64NotEqual || v.Op == ssa.OpPPC64LessThan || v.Op == ssa.OpPPC64FLessThan || v.Op == ssa.OpPPC64LessEqual || v.Op == ssa.OpPPC64GreaterThan || v.Op == ssa.OpPPC64FGreaterThan || v.Op == ssa.OpPPC64GreaterEqual) 

                // On Power7 or later, can use isel instruction:
                // for a < b, a > b, a = b:
                //   rtmp := 1
                //   isel rt,rtmp,r0,cond // rt is target in ppc asm

                // for  a >= b, a <= b, a != b:
                //   rtmp := 1
                //   isel rt,0,rtmp,!cond // rt is target in ppc asm

                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 1L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = iselRegs[1L];
                var iop = iselOps[v.Op];
                ssaGenISEL(s, v, iop.cond, iselRegs[iop.valueIfCond], iselRegs[1L - iop.valueIfCond]);
            else if (v.Op == ssa.OpPPC64FLessEqual || v.Op == ssa.OpPPC64FGreaterEqual) 

                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 1L;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = iselRegs[1L];
                iop = iselOps[v.Op];
                ssaGenISEL(s, v, iop.cond, iselRegs[iop.valueIfCond], iselRegs[1L - iop.valueIfCond]);
                ssaGenISEL(s, v, ppc64.C_COND_EQ, iselRegs[1L], v.Reg());
            else if (v.Op == ssa.OpPPC64LoweredZero) 

                // unaligned data doesn't hurt performance
                // for these instructions on power8 or later

                // for sizes >= 64 generate a loop as follows:

                // set up loop counter in CTR, used by BC
                //     MOVD len/32,REG_TMP
                //     MOVD REG_TMP,CTR
                //     loop:
                //     MOVD R0,(R3)
                //     MOVD R0,8(R3)
                //     MOVD R0,16(R3)
                //     MOVD R0,24(R3)
                //     ADD  $32,R3
                //     BC   16, 0, loop
                //
                // any remainder is done as described below

                // for sizes < 64 bytes, first clear as many doublewords as possible,
                // then handle the remainder
                //    MOVD R0,(R3)
                //    MOVD R0,8(R3)
                // .... etc.
                //
                // the remainder bytes are cleared using one or more
                // of the following instructions with the appropriate
                // offsets depending which instructions are needed
                //
                //    MOVW R0,n1(R3)    4 bytes
                //    MOVH R0,n2(R3)    2 bytes
                //    MOVB R0,n3(R3)    1 byte
                //
                // 7 bytes: MOVW, MOVH, MOVB
                // 6 bytes: MOVW, MOVH
                // 5 bytes: MOVW, MOVB
                // 3 bytes: MOVH, MOVB

                // each loop iteration does 32 bytes
                var ctr = v.AuxInt / 32L; 

                // remainder bytes
                var rem = v.AuxInt % 32L; 

                // only generate a loop if there is more
                // than 1 iteration.
                if (ctr > 1L)
                { 
                    // Set up CTR loop counter
                    p = s.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = ctr;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = ppc64.REGTMP;

                    p = s.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = ppc64.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = ppc64.REG_CTR; 

                    // generate 4 MOVDs
                    // when this is a loop then the top must be saved
                    ref obj.Prog top = default;
                    {
                        var offset__prev1 = offset;

                        var offset = int64(0L);

                        while (offset < 32L)
                        { 
                            // This is the top of loop
                            p = s.Prog(ppc64.AMOVD);
                            p.From.Type = obj.TYPE_REG;
                            p.From.Reg = ppc64.REG_R0;
                            p.To.Type = obj.TYPE_MEM;
                            p.To.Reg = v.Args[0L].Reg();
                            p.To.Offset = offset; 
                            // Save the top of loop
                            if (top == null)
                            {
                                top = p;
                            offset += 8L;
                            }
                        } 

                        // Increment address for the
                        // 4 doublewords just zeroed.


                        offset = offset__prev1;
                    } 

                    // Increment address for the
                    // 4 doublewords just zeroed.
                    p = s.Prog(ppc64.AADD);
                    p.Reg = v.Args[0L].Reg();
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 32L;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = v.Args[0L].Reg(); 

                    // Branch back to top of loop
                    // based on CTR
                    // BC with BO_BCTR generates bdnz
                    p = s.Prog(ppc64.ABC);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = ppc64.BO_BCTR;
                    p.Reg = ppc64.REG_R0;
                    p.To.Type = obj.TYPE_BRANCH;
                    gc.Patch(p, top);
                } 

                // when ctr == 1 the loop was not generated but
                // there are at least 32 bytes to clear, so add
                // that to the remainder to generate the code
                // to clear those doublewords
                if (ctr == 1L)
                {
                    rem += 32L;
                } 

                // clear the remainder starting at offset zero
                offset = int64(0L); 

                // first clear as many doublewords as possible
                // then clear remaining sizes as available
                while (rem > 0L)
                {
                    op = ppc64.AMOVB;
                    var size = int64(1L);

                    if (rem >= 8L) 
                        op = ppc64.AMOVD;
                        size = 8L;
                    else if (rem >= 4L) 
                        op = ppc64.AMOVW;
                        size = 4L;
                    else if (rem >= 2L) 
                        op = ppc64.AMOVH;
                        size = 2L;
                                        p = s.Prog(op);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = ppc64.REG_R0;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Reg = v.Args[0L].Reg();
                    p.To.Offset = offset;
                    rem -= size;
                    offset += size;
                }

            else if (v.Op == ssa.OpPPC64LoweredMove) 

                // This will be used when moving more
                // than 8 bytes.  Moves start with as
                // as many 8 byte moves as possible, then
                // 4, 2, or 1 byte(s) as remaining.  This will
                // work and be efficient for power8 or later.
                // If there are 64 or more bytes, then a
                // loop is generated to move 32 bytes and
                // update the src and dst addresses on each
                // iteration. When < 64 bytes, the appropriate
                // number of moves are generated based on the
                // size.
                // When moving >= 64 bytes a loop is used
                //    MOVD len/32,REG_TMP
                //    MOVD REG_TMP,CTR
                // top:
                //    MOVD (R4),R7
                //    MOVD 8(R4),R8
                //    MOVD 16(R4),R9
                //    MOVD 24(R4),R10
                //    ADD  R4,$32
                //    MOVD R7,(R3)
                //    MOVD R8,8(R3)
                //    MOVD R9,16(R3)
                //    MOVD R10,24(R3)
                //    ADD  R3,$32
                //    BC 16,0,top
                // Bytes not moved by this loop are moved
                // with a combination of the following instructions,
                // starting with the largest sizes and generating as
                // many as needed, using the appropriate offset value.
                //    MOVD  n(R4),R7
                //    MOVD  R7,n(R3)
                //    MOVW  n1(R4),R7
                //    MOVW  R7,n1(R3)
                //    MOVH  n2(R4),R7
                //    MOVH  R7,n2(R3)
                //    MOVB  n3(R4),R7
                //    MOVB  R7,n3(R3)

                // Each loop iteration moves 32 bytes
                ctr = v.AuxInt / 32L; 

                // Remainder after the loop
                rem = v.AuxInt % 32L;

                var dst_reg = v.Args[0L].Reg();
                var src_reg = v.Args[1L].Reg(); 

                // The set of registers used here, must match the clobbered reg list
                // in PPC64Ops.go.
                short useregs = new slice<short>(new short[] { ppc64.REG_R7, ppc64.REG_R8, ppc64.REG_R9, ppc64.REG_R10 });
                offset = int64(0L); 

                // top of the loop
                top = default; 
                // Only generate looping code when loop counter is > 1 for >= 64 bytes
                if (ctr > 1L)
                { 
                    // Set up the CTR
                    p = s.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = ctr;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = ppc64.REGTMP;

                    p = s.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = ppc64.REGTMP;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = ppc64.REG_CTR; 

                    // Generate all the MOVDs for loads
                    // based off the same register, increasing
                    // the offset by 8 for each instruction
                    {
                        var rg__prev1 = rg;

                        foreach (var (_, __rg) in useregs)
                        {
                            rg = __rg;
                            p = s.Prog(ppc64.AMOVD);
                            p.From.Type = obj.TYPE_MEM;
                            p.From.Reg = src_reg;
                            p.From.Offset = offset;
                            p.To.Type = obj.TYPE_REG;
                            p.To.Reg = rg;
                            if (top == null)
                            {
                                top = p;
                            }
                            offset += 8L;
                        } 
                        // increment the src_reg for next iteration

                        rg = rg__prev1;
                    }

                    p = s.Prog(ppc64.AADD);
                    p.Reg = src_reg;
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 32L;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = src_reg; 

                    // generate the MOVDs for stores, based
                    // off the same register, using the same
                    // offsets as in the loads.
                    offset = int64(0L);
                    {
                        var rg__prev1 = rg;

                        foreach (var (_, __rg) in useregs)
                        {
                            rg = __rg;
                            p = s.Prog(ppc64.AMOVD);
                            p.From.Type = obj.TYPE_REG;
                            p.From.Reg = rg;
                            p.To.Type = obj.TYPE_MEM;
                            p.To.Reg = dst_reg;
                            p.To.Offset = offset;
                            offset += 8L;
                        } 
                        // increment the dst_reg for next iteration

                        rg = rg__prev1;
                    }

                    p = s.Prog(ppc64.AADD);
                    p.Reg = dst_reg;
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = 32L;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = dst_reg; 

                    // BC with BO_BCTR generates bdnz to branch on nonzero CTR
                    // to loop top.
                    p = s.Prog(ppc64.ABC);
                    p.From.Type = obj.TYPE_CONST;
                    p.From.Offset = ppc64.BO_BCTR;
                    p.Reg = ppc64.REG_R0;
                    p.To.Type = obj.TYPE_BRANCH;
                    gc.Patch(p, top); 

                    // src_reg and dst_reg were incremented in the loop, so
                    // later instructions start with offset 0.
                    offset = int64(0L);
                } 

                // No loop was generated for one iteration, so
                // add 32 bytes to the remainder to move those bytes.
                if (ctr == 1L)
                {
                    rem += 32L;
                } 

                // Generate all the remaining load and store pairs, starting with
                // as many 8 byte moves as possible, then 4, 2, 1.
                while (rem > 0L)
                {
                    op = ppc64.AMOVB;
                    size = int64(1L);

                    if (rem >= 8L) 
                        op = ppc64.AMOVD;
                        size = 8L;
                    else if (rem >= 4L) 
                        op = ppc64.AMOVW;
                        size = 4L;
                    else if (rem >= 2L) 
                        op = ppc64.AMOVH;
                        size = 2L;
                    // Load
                    p = s.Prog(op);
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = ppc64.REG_R7;
                    p.From.Type = obj.TYPE_MEM;
                    p.From.Reg = src_reg;
                    p.From.Offset = offset; 

                    // Store
                    p = s.Prog(op);
                    p.From.Type = obj.TYPE_REG;
                    p.From.Reg = ppc64.REG_R7;
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Reg = dst_reg;
                    p.To.Offset = offset;
                    rem -= size;
                    offset += size;
                }

            else if (v.Op == ssa.OpPPC64CALLstatic) 
                s.Call(v);
            else if (v.Op == ssa.OpPPC64CALLclosure || v.Op == ssa.OpPPC64CALLinter) 
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = v.Args[0L].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_CTR;

                if (v.Args[0L].Reg() != ppc64.REG_R12)
                {
                    v.Fatalf("Function address for %v should be in R12 %d but is in %d", v.LongString(), ppc64.REG_R12, p.From.Reg);
                }
                var pp = s.Call(v);
                pp.To.Reg = ppc64.REG_CTR;

                if (gc.Ctxt.Flag_shared)
                { 
                    // When compiling Go into PIC, the function we just
                    // called via pointer might have been implemented in
                    // a separate module and so overwritten the TOC
                    // pointer in R2; reload it.
                    var q = s.Prog(ppc64.AMOVD);
                    q.From.Type = obj.TYPE_MEM;
                    q.From.Offset = 24L;
                    q.From.Reg = ppc64.REGSP;
                    q.To.Type = obj.TYPE_REG;
                    q.To.Reg = ppc64.REG_R2;
                }
            else if (v.Op == ssa.OpPPC64LoweredNilCheck) 
                // Issue a load which will fault if arg is nil.
                p = s.Prog(ppc64.AMOVBZ);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = v.Args[0L].Reg();
                gc.AddAux(ref p.From, v);
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP;
                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");
                }
            else if (v.Op == ssa.OpPPC64InvertFlags) 
                v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpPPC64FlagEQ || v.Op == ssa.OpPPC64FlagLT || v.Op == ssa.OpPPC64FlagGT) 
                v.Fatalf("Flag* ops should never make it to codegen %v", v.LongString());
            else if (v.Op == ssa.OpClobber)             else 
                v.Fatalf("genValue not implemented: %s", v.LongString());
                    }



        private static void ssaGenBlock(ref gc.SSAGenState s, ref ssa.Block b, ref ssa.Block next)
        {

            if (b.Kind == ssa.BlockDefer) 
                // defer returns in R3:
                // 0 if we should continue executing
                // 1 if we should jump to deferreturn call
                var p = s.Prog(ppc64.ACMP);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = ppc64.REG_R3;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_R0;

                p = s.Prog(ppc64.ABNE);
                p.To.Type = obj.TYPE_BRANCH;
                s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                if (b.Succs[0L].Block() != next)
                {
                    p = s.Prog(obj.AJMP);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                }
            else if (b.Kind == ssa.BlockPlain) 
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
            else if (b.Kind == ssa.BlockPPC64EQ || b.Kind == ssa.BlockPPC64NE || b.Kind == ssa.BlockPPC64LT || b.Kind == ssa.BlockPPC64GE || b.Kind == ssa.BlockPPC64LE || b.Kind == ssa.BlockPPC64GT || b.Kind == ssa.BlockPPC64FLT || b.Kind == ssa.BlockPPC64FGE || b.Kind == ssa.BlockPPC64FLE || b.Kind == ssa.BlockPPC64FGT) 
                var jmp = blockJump[b.Kind];
                p = default;

                if (next == b.Succs[0L].Block()) 
                    p = s.Prog(jmp.invasm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[1].Block()));
                    if (jmp.invasmun)
                    { 
                        // TODO: The second branch is probably predict-not-taken since it is for FP unordered
                        var q = s.Prog(ppc64.ABVS);
                        q.To.Type = obj.TYPE_BRANCH;
                        s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[1].Block()));
                    }
                else if (next == b.Succs[1L].Block()) 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                    if (jmp.asmeq)
                    {
                        q = s.Prog(ppc64.ABEQ);
                        q.To.Type = obj.TYPE_BRANCH;
                        s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[0].Block()));
                    }
                else 
                    p = s.Prog(jmp.asm);
                    p.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:p,B:b.Succs[0].Block()));
                    if (jmp.asmeq)
                    {
                        q = s.Prog(ppc64.ABEQ);
                        q.To.Type = obj.TYPE_BRANCH;
                        s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[0].Block()));
                    }
                    q = s.Prog(obj.AJMP);
                    q.To.Type = obj.TYPE_BRANCH;
                    s.Branches = append(s.Branches, new gc.Branch(P:q,B:b.Succs[1].Block()));
                            else 
                b.Fatalf("branch not implemented: %s. Control: %s", b.LongString(), b.Control.LongString());
                    }
    }
}}}}
