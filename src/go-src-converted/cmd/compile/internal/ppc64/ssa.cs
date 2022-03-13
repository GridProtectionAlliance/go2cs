// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2022 March 13 06:24:29 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ppc64\ssa.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using logopt = cmd.compile.@internal.logopt_package;
using ssa = cmd.compile.@internal.ssa_package;
using ssagen = cmd.compile.@internal.ssagen_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using ppc64 = cmd.@internal.obj.ppc64_package;
using buildcfg = @internal.buildcfg_package;
using math = math_package;
using strings = strings_package;


// markMoves marks any MOVXconst ops that need to avoid clobbering flags.

public static partial class ppc64_package {

private static void ssaMarkMoves(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
 
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
private static obj.As loadByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return ppc64.AFMOVS;
                break;
            case 8: 
                return ppc64.AFMOVD;
                break;
        }
    }
    else
 {
        switch (t.Size()) {
            case 1: 
                           if (t.IsSigned()) {
                               return ppc64.AMOVB;
                           }
                           else
                {
                               return ppc64.AMOVBZ;
                           }
                break;
            case 2: 
                           if (t.IsSigned()) {
                               return ppc64.AMOVH;
                           }
                           else
                {
                               return ppc64.AMOVHZ;
                           }
                break;
            case 4: 
                           if (t.IsSigned()) {
                               return ppc64.AMOVW;
                           }
                           else
                {
                               return ppc64.AMOVWZ;
                           }
                break;
            case 8: 
                return ppc64.AMOVD;
                break;
        }
    }
    panic("bad load type");
});

// storeByType returns the store instruction of the given type.
private static obj.As storeByType(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return ppc64.AFMOVS;
                break;
            case 8: 
                return ppc64.AFMOVD;
                break;
        }
    }
    else
 {
        switch (t.Size()) {
            case 1: 
                return ppc64.AMOVB;
                break;
            case 2: 
                return ppc64.AMOVH;
                break;
            case 4: 
                return ppc64.AMOVW;
                break;
            case 8: 
                return ppc64.AMOVD;
                break;
        }
    }
    panic("bad store type");
});

private static void ssaGenValue(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpCopy) 
        var t = v.Type;
        if (t.IsMemory()) {
            return ;
        }
        var x = v.Args[0].Reg();
        var y = v.Reg();
        if (x != y) {
            var rt = obj.TYPE_REG;
            var op = ppc64.AMOVD;

            if (t.IsFloat()) {
                op = ppc64.AFMOVD;
            }
            var p = s.Prog(op);
            p.From.Type = rt;
            p.From.Reg = x;
            p.To.Type = rt;
            p.To.Reg = y;
        }
    else if (v.Op == ssa.OpPPC64LoweredMuluhilo) 
        // MULHDU    Rarg1, Rarg0, Reg0
        // MULLD    Rarg1, Rarg0, Reg1
        var r0 = v.Args[0].Reg();
        var r1 = v.Args[1].Reg();
        p = s.Prog(ppc64.AMULHDU);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r1;
        p.Reg = r0;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg0();
        var p1 = s.Prog(ppc64.AMULLD);
        p1.From.Type = obj.TYPE_REG;
        p1.From.Reg = r1;
        p1.Reg = r0;
        p1.To.Type = obj.TYPE_REG;
        p1.To.Reg = v.Reg1();
    else if (v.Op == ssa.OpPPC64LoweredAdd64Carry) 
        // ADDC        Rarg2, -1, Rtmp
        // ADDE        Rarg1, Rarg0, Reg0
        // ADDZE    Rzero, Reg1
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        var r2 = v.Args[2].Reg();
        p = s.Prog(ppc64.AADDC);
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = -1;
        p.Reg = r2;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REGTMP;
        p1 = s.Prog(ppc64.AADDE);
        p1.From.Type = obj.TYPE_REG;
        p1.From.Reg = r1;
        p1.Reg = r0;
        p1.To.Type = obj.TYPE_REG;
        p1.To.Reg = v.Reg0();
        var p2 = s.Prog(ppc64.AADDZE);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = ppc64.REGZERO;
        p2.To.Type = obj.TYPE_REG;
        p2.To.Reg = v.Reg1();
    else if (v.Op == ssa.OpPPC64LoweredAtomicAnd8 || v.Op == ssa.OpPPC64LoweredAtomicAnd32 || v.Op == ssa.OpPPC64LoweredAtomicOr8 || v.Op == ssa.OpPPC64LoweredAtomicOr32) 
        // LWSYNC
        // LBAR/LWAR    (Rarg0), Rtmp
        // AND/OR    Rarg1, Rtmp
        // STBCCC/STWCCC Rtmp, (Rarg0)
        // BNE        -3(PC)
        var ld = ppc64.ALBAR;
        var st = ppc64.ASTBCCC;
        if (v.Op == ssa.OpPPC64LoweredAtomicAnd32 || v.Op == ssa.OpPPC64LoweredAtomicOr32) {
            ld = ppc64.ALWAR;
            st = ppc64.ASTWCCC;
        }
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg(); 
        // LWSYNC - Assuming shared data not write-through-required nor
        // caching-inhibited. See Appendix B.2.2.2 in the ISA 2.07b.
        var plwsync = s.Prog(ppc64.ALWSYNC);
        plwsync.To.Type = obj.TYPE_NONE; 
        // LBAR or LWAR
        p = s.Prog(ld);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = r0;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REGTMP; 
        // AND/OR reg1,out
        p1 = s.Prog(v.Op.Asm());
        p1.From.Type = obj.TYPE_REG;
        p1.From.Reg = r1;
        p1.To.Type = obj.TYPE_REG;
        p1.To.Reg = ppc64.REGTMP; 
        // STBCCC or STWCCC
        p2 = s.Prog(st);
        p2.From.Type = obj.TYPE_REG;
        p2.From.Reg = ppc64.REGTMP;
        p2.To.Type = obj.TYPE_MEM;
        p2.To.Reg = r0;
        p2.RegTo2 = ppc64.REGTMP; 
        // BNE retry
        var p3 = s.Prog(ppc64.ABNE);
        p3.To.Type = obj.TYPE_BRANCH;
        p3.To.SetTarget(p);
    else if (v.Op == ssa.OpPPC64LoweredAtomicAdd32 || v.Op == ssa.OpPPC64LoweredAtomicAdd64) 
        // LWSYNC
        // LDAR/LWAR    (Rarg0), Rout
        // ADD        Rarg1, Rout
        // STDCCC/STWCCC Rout, (Rarg0)
        // BNE         -3(PC)
        // MOVW        Rout,Rout (if Add32)
        ld = ppc64.ALDAR;
        st = ppc64.ASTDCCC;
        if (v.Op == ssa.OpPPC64LoweredAtomicAdd32) {
            ld = ppc64.ALWAR;
            st = ppc64.ASTWCCC;
        }
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        var @out = v.Reg0(); 
        // LWSYNC - Assuming shared data not write-through-required nor
        // caching-inhibited. See Appendix B.2.2.2 in the ISA 2.07b.
        plwsync = s.Prog(ppc64.ALWSYNC);
        plwsync.To.Type = obj.TYPE_NONE; 
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
        p4.To.SetTarget(p); 

        // Ensure a 32 bit result
        if (v.Op == ssa.OpPPC64LoweredAtomicAdd32) {
            var p5 = s.Prog(ppc64.AMOVWZ);
            p5.To.Type = obj.TYPE_REG;
            p5.To.Reg = out;
            p5.From.Type = obj.TYPE_REG;
            p5.From.Reg = out;
        }
    else if (v.Op == ssa.OpPPC64LoweredAtomicExchange32 || v.Op == ssa.OpPPC64LoweredAtomicExchange64) 
        // LWSYNC
        // LDAR/LWAR    (Rarg0), Rout
        // STDCCC/STWCCC Rout, (Rarg0)
        // BNE         -2(PC)
        // ISYNC
        ld = ppc64.ALDAR;
        st = ppc64.ASTDCCC;
        if (v.Op == ssa.OpPPC64LoweredAtomicExchange32) {
            ld = ppc64.ALWAR;
            st = ppc64.ASTWCCC;
        }
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        @out = v.Reg0(); 
        // LWSYNC - Assuming shared data not write-through-required nor
        // caching-inhibited. See Appendix B.2.2.2 in the ISA 2.07b.
        plwsync = s.Prog(ppc64.ALWSYNC);
        plwsync.To.Type = obj.TYPE_NONE; 
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
        p2.To.SetTarget(p); 
        // ISYNC
        var pisync = s.Prog(ppc64.AISYNC);
        pisync.To.Type = obj.TYPE_NONE;
    else if (v.Op == ssa.OpPPC64LoweredAtomicLoad8 || v.Op == ssa.OpPPC64LoweredAtomicLoad32 || v.Op == ssa.OpPPC64LoweredAtomicLoad64 || v.Op == ssa.OpPPC64LoweredAtomicLoadPtr) 
        // SYNC
        // MOVB/MOVD/MOVW (Rarg0), Rout
        // CMP Rout,Rout
        // BNE 1(PC)
        // ISYNC
        ld = ppc64.AMOVD;
        var cmp = ppc64.ACMP;

        if (v.Op == ssa.OpPPC64LoweredAtomicLoad8) 
            ld = ppc64.AMOVBZ;
        else if (v.Op == ssa.OpPPC64LoweredAtomicLoad32) 
            ld = ppc64.AMOVWZ;
            cmp = ppc64.ACMPW;
                var arg0 = v.Args[0].Reg();
        @out = v.Reg0(); 
        // SYNC when AuxInt == 1; otherwise, load-acquire
        if (v.AuxInt == 1) {
            var psync = s.Prog(ppc64.ASYNC);
            psync.To.Type = obj.TYPE_NONE;
        }
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
        p2.To.SetTarget(pisync);
    else if (v.Op == ssa.OpPPC64LoweredAtomicStore8 || v.Op == ssa.OpPPC64LoweredAtomicStore32 || v.Op == ssa.OpPPC64LoweredAtomicStore64) 
        // SYNC or LWSYNC
        // MOVB/MOVW/MOVD arg1,(arg0)
        st = ppc64.AMOVD;

        if (v.Op == ssa.OpPPC64LoweredAtomicStore8) 
            st = ppc64.AMOVB;
        else if (v.Op == ssa.OpPPC64LoweredAtomicStore32) 
            st = ppc64.AMOVW;
                arg0 = v.Args[0].Reg();
        var arg1 = v.Args[1].Reg(); 
        // If AuxInt == 0, LWSYNC (Store-Release), else SYNC
        // SYNC
        var syncOp = ppc64.ASYNC;
        if (v.AuxInt == 0) {
            syncOp = ppc64.ALWSYNC;
        }
        psync = s.Prog(syncOp);
        psync.To.Type = obj.TYPE_NONE; 
        // Store
        p = s.Prog(st);
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = arg0;
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = arg1;
    else if (v.Op == ssa.OpPPC64LoweredAtomicCas64 || v.Op == ssa.OpPPC64LoweredAtomicCas32) 
        // LWSYNC
        // loop:
        // LDAR        (Rarg0), MutexHint, Rtmp
        // CMP         Rarg1, Rtmp
        // BNE         fail
        // STDCCC      Rarg2, (Rarg0)
        // BNE         loop
        // LWSYNC      // Only for sequential consistency; not required in CasRel.
        // MOVD        $1, Rout
        // BR          end
        // fail:
        // MOVD        $0, Rout
        // end:
        ld = ppc64.ALDAR;
        st = ppc64.ASTDCCC;
        cmp = ppc64.ACMP;
        if (v.Op == ssa.OpPPC64LoweredAtomicCas32) {
            ld = ppc64.ALWAR;
            st = ppc64.ASTWCCC;
            cmp = ppc64.ACMPW;
        }
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();
        r2 = v.Args[2].Reg();
        @out = v.Reg0(); 
        // LWSYNC - Assuming shared data not write-through-required nor
        // caching-inhibited. See Appendix B.2.2.2 in the ISA 2.07b.
        var plwsync1 = s.Prog(ppc64.ALWSYNC);
        plwsync1.To.Type = obj.TYPE_NONE; 
        // LDAR or LWAR
        p = s.Prog(ld);
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = r0;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REGTMP; 
        // If it is a Compare-and-Swap-Release operation, set the EH field with
        // the release hint.
        if (v.AuxInt == 0) {
            p.SetFrom3Const(0);
        }
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
        p4.To.SetTarget(p); 
        // LWSYNC - Assuming shared data not write-through-required nor
        // caching-inhibited. See Appendix B.2.1.1 in the ISA 2.07b.
        // If the operation is a CAS-Release, then synchronization is not necessary.
        if (v.AuxInt != 0) {
            var plwsync2 = s.Prog(ppc64.ALWSYNC);
            plwsync2.To.Type = obj.TYPE_NONE;
        }
        p5 = s.Prog(ppc64.AMOVD);
        p5.From.Type = obj.TYPE_CONST;
        p5.From.Offset = 1;
        p5.To.Type = obj.TYPE_REG;
        p5.To.Reg = out; 
        // BR done
        var p6 = s.Prog(obj.AJMP);
        p6.To.Type = obj.TYPE_BRANCH; 
        // return false
        var p7 = s.Prog(ppc64.AMOVD);
        p7.From.Type = obj.TYPE_CONST;
        p7.From.Offset = 0;
        p7.To.Type = obj.TYPE_REG;
        p7.To.Reg = out;
        p2.To.SetTarget(p7); 
        // done (label)
        var p8 = s.Prog(obj.ANOP);
        p6.To.SetTarget(p8);
    else if (v.Op == ssa.OpPPC64LoweredGetClosurePtr) 
        // Closure pointer is R11 (already)
        ssagen.CheckLoweredGetClosurePtr(v);
    else if (v.Op == ssa.OpPPC64LoweredGetCallerSP) 
        // caller's SP is FixedFrameSize below the address of the first arg
        p = s.Prog(ppc64.AMOVD);
        p.From.Type = obj.TYPE_ADDR;
        p.From.Offset = -@base.Ctxt.FixedFrameSize();
        p.From.Name = obj.NAME_PARAM;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64LoweredGetCallerPC) 
        p = s.Prog(obj.AGETCALLERPC);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64LoweredRound32F || v.Op == ssa.OpPPC64LoweredRound64F)     else if (v.Op == ssa.OpLoadReg) 
        var loadOp = loadByType(_addr_v.Type);
        p = s.Prog(loadOp);
        ssagen.AddrAuto(_addr_p.From, v.Args[0]);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpStoreReg) 
        var storeOp = storeByType(_addr_v.Type);
        p = s.Prog(storeOp);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddrAuto(_addr_p.To, v);
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
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();

        p = s.Prog(ppc64.ACMP);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r1;
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = -1;

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
        pbahead.To.SetTarget(p);

        p = s.Prog(obj.ANOP);
        pbover.To.SetTarget(p);
    else if (v.Op == ssa.OpPPC64DIVW) 
        // word-width version of above
        r = v.Reg();
        r0 = v.Args[0].Reg();
        r1 = v.Args[1].Reg();

        p = s.Prog(ppc64.ACMPW);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r1;
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = -1;

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
        pbahead.To.SetTarget(p);

        p = s.Prog(obj.ANOP);
        pbover.To.SetTarget(p);
    else if (v.Op == ssa.OpPPC64CLRLSLWI) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        var shifts = v.AuxInt;
        p = s.Prog(v.Op.Asm()); 
        // clrlslwi ra,rs,mb,sh will become rlwinm ra,rs,sh,mb-sh,31-sh as described in ISA
        p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:ssa.GetPPC64Shiftmb(shifts));
        p.SetFrom3Const(ssa.GetPPC64Shiftsh(shifts));
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpPPC64CLRLSLDI) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        shifts = v.AuxInt;
        p = s.Prog(v.Op.Asm()); 
        // clrlsldi ra,rs,mb,sh will become rldic ra,rs,sh,mb-sh
        p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:ssa.GetPPC64Shiftmb(shifts));
        p.SetFrom3Const(ssa.GetPPC64Shiftsh(shifts));
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r; 

        // Mask has been set as sh
    else if (v.Op == ssa.OpPPC64RLDICL) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        shifts = v.AuxInt;
        p = s.Prog(v.Op.Asm());
        p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:ssa.GetPPC64Shiftsh(shifts));
        p.SetFrom3Const(ssa.GetPPC64Shiftmb(shifts));
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpPPC64ADD || v.Op == ssa.OpPPC64FADD || v.Op == ssa.OpPPC64FADDS || v.Op == ssa.OpPPC64SUB || v.Op == ssa.OpPPC64FSUB || v.Op == ssa.OpPPC64FSUBS || v.Op == ssa.OpPPC64MULLD || v.Op == ssa.OpPPC64MULLW || v.Op == ssa.OpPPC64DIVDU || v.Op == ssa.OpPPC64DIVWU || v.Op == ssa.OpPPC64SRAD || v.Op == ssa.OpPPC64SRAW || v.Op == ssa.OpPPC64SRD || v.Op == ssa.OpPPC64SRW || v.Op == ssa.OpPPC64SLD || v.Op == ssa.OpPPC64SLW || v.Op == ssa.OpPPC64ROTL || v.Op == ssa.OpPPC64ROTLW || v.Op == ssa.OpPPC64MULHD || v.Op == ssa.OpPPC64MULHW || v.Op == ssa.OpPPC64MULHDU || v.Op == ssa.OpPPC64MULHWU || v.Op == ssa.OpPPC64FMUL || v.Op == ssa.OpPPC64FMULS || v.Op == ssa.OpPPC64FDIV || v.Op == ssa.OpPPC64FDIVS || v.Op == ssa.OpPPC64FCPSGN || v.Op == ssa.OpPPC64AND || v.Op == ssa.OpPPC64OR || v.Op == ssa.OpPPC64ANDN || v.Op == ssa.OpPPC64ORN || v.Op == ssa.OpPPC64NOR || v.Op == ssa.OpPPC64XOR || v.Op == ssa.OpPPC64EQV || v.Op == ssa.OpPPC64MODUD || v.Op == ssa.OpPPC64MODSD || v.Op == ssa.OpPPC64MODUW || v.Op == ssa.OpPPC64MODSW) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r2;
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpPPC64ANDCC || v.Op == ssa.OpPPC64ORCC || v.Op == ssa.OpPPC64XORCC) 
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r2;
        p.Reg = r1;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REGTMP; // result is not needed
    else if (v.Op == ssa.OpPPC64ROTLconst || v.Op == ssa.OpPPC64ROTLWconst) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg(); 

        // Auxint holds encoded rotate + mask
    else if (v.Op == ssa.OpPPC64RLWINM || v.Op == ssa.OpPPC64RLWMI) 
        var (rot, mb, me, _) = ssa.DecodePPC64RotateMask(v.AuxInt);
        p = s.Prog(v.Op.Asm());
        p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:v.Reg());
        p.Reg = v.Args[0].Reg();
        p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(rot));
        p.SetRestArgs(new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:mb}, {Type:obj.TYPE_CONST,Offset:me} })); 

        // Auxint holds mask
    else if (v.Op == ssa.OpPPC64RLWNM) 
        var (_, mb, me, _) = ssa.DecodePPC64RotateMask(v.AuxInt);
        p = s.Prog(v.Op.Asm());
        p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:v.Reg());
        p.Reg = v.Args[0].Reg();
        p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:v.Args[1].Reg());
        p.SetRestArgs(new slice<obj.Addr>(new obj.Addr[] { {Type:obj.TYPE_CONST,Offset:mb}, {Type:obj.TYPE_CONST,Offset:me} }));
    else if (v.Op == ssa.OpPPC64MADDLD) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
        var r3 = v.Args[2].Reg(); 
        // r = r1*r2 ± r3
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r1;
        p.Reg = r2;
        p.SetFrom3Reg(r3);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpPPC64FMADD || v.Op == ssa.OpPPC64FMADDS || v.Op == ssa.OpPPC64FMSUB || v.Op == ssa.OpPPC64FMSUBS) 
        r = v.Reg();
        r1 = v.Args[0].Reg();
        r2 = v.Args[1].Reg();
        r3 = v.Args[2].Reg(); 
        // r = r1*r2 ± r3
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = r1;
        p.Reg = r3;
        p.SetFrom3Reg(r2);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
    else if (v.Op == ssa.OpPPC64NEG || v.Op == ssa.OpPPC64FNEG || v.Op == ssa.OpPPC64FSQRT || v.Op == ssa.OpPPC64FSQRTS || v.Op == ssa.OpPPC64FFLOOR || v.Op == ssa.OpPPC64FTRUNC || v.Op == ssa.OpPPC64FCEIL || v.Op == ssa.OpPPC64FCTIDZ || v.Op == ssa.OpPPC64FCTIWZ || v.Op == ssa.OpPPC64FCFID || v.Op == ssa.OpPPC64FCFIDS || v.Op == ssa.OpPPC64FRSP || v.Op == ssa.OpPPC64CNTLZD || v.Op == ssa.OpPPC64CNTLZW || v.Op == ssa.OpPPC64POPCNTD || v.Op == ssa.OpPPC64POPCNTW || v.Op == ssa.OpPPC64POPCNTB || v.Op == ssa.OpPPC64MFVSRD || v.Op == ssa.OpPPC64MTVSRD || v.Op == ssa.OpPPC64FABS || v.Op == ssa.OpPPC64FNABS || v.Op == ssa.OpPPC64FROUND || v.Op == ssa.OpPPC64CNTTZW || v.Op == ssa.OpPPC64CNTTZD) 
        r = v.Reg();
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = r;
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
    else if (v.Op == ssa.OpPPC64ADDconst || v.Op == ssa.OpPPC64ANDconst || v.Op == ssa.OpPPC64ORconst || v.Op == ssa.OpPPC64XORconst || v.Op == ssa.OpPPC64SRADconst || v.Op == ssa.OpPPC64SRAWconst || v.Op == ssa.OpPPC64SRDconst || v.Op == ssa.OpPPC64SRWconst || v.Op == ssa.OpPPC64SLDconst || v.Op == ssa.OpPPC64SLWconst || v.Op == ssa.OpPPC64EXTSWSLconst || v.Op == ssa.OpPPC64MULLWconst || v.Op == ssa.OpPPC64MULLDconst) 
        p = s.Prog(v.Op.Asm());
        p.Reg = v.Args[0].Reg();
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64SUBFCconst) 
        p = s.Prog(v.Op.Asm());
        p.SetFrom3Const(v.AuxInt);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64ANDCCconst) 
        p = s.Prog(v.Op.Asm());
        p.Reg = v.Args[0].Reg();
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REGTMP; // discard result
    else if (v.Op == ssa.OpPPC64MOVDaddr) 
        switch (v.Aux.type()) {
            case 
                if (v.AuxInt != 0 || v.Args[0].Reg() != v.Reg()) {
                    p = s.Prog(ppc64.AMOVD);
                    p.From.Type = obj.TYPE_ADDR;
                    p.From.Reg = v.Args[0].Reg();
                    p.From.Offset = v.AuxInt;
                    p.To.Type = obj.TYPE_REG;
                    p.To.Reg = v.Reg();
                }
                break;
            case ptr<obj.LSym> _:
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                ssagen.AddAux(_addr_p.From, v);
                break;
            case ir.Node _:
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_ADDR;
                p.From.Reg = v.Args[0].Reg();
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = v.Reg();
                ssagen.AddAux(_addr_p.From, v);
                break;
            default:
            {
                v.Fatalf("aux in MOVDaddr is of unknown type %T", v.Aux);
                break;
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
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Args[1].Reg();
    else if (v.Op == ssa.OpPPC64CMPconst || v.Op == ssa.OpPPC64CMPUconst || v.Op == ssa.OpPPC64CMPWconst || v.Op == ssa.OpPPC64CMPWUconst) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_CONST;
        p.To.Offset = v.AuxInt;
    else if (v.Op == ssa.OpPPC64MOVBreg || v.Op == ssa.OpPPC64MOVBZreg || v.Op == ssa.OpPPC64MOVHreg || v.Op == ssa.OpPPC64MOVHZreg || v.Op == ssa.OpPPC64MOVWreg || v.Op == ssa.OpPPC64MOVWZreg) 
        // Shift in register to required size
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Reg = v.Reg();
        p.To.Type = obj.TYPE_REG;
    else if (v.Op == ssa.OpPPC64MOVDload || v.Op == ssa.OpPPC64MOVWload) 

        // MOVDload and MOVWload are DS form instructions that are restricted to
        // offsets that are a multiple of 4. If the offset is not a multple of 4,
        // then the address of the symbol to be loaded is computed (base + offset)
        // and used as the new base register and the offset field in the instruction
        // can be set to zero.

        // This same problem can happen with gostrings since the final offset is not
        // known yet, but could be unaligned after the relocation is resolved.
        // So gostrings are handled the same way.

        // This allows the MOVDload and MOVWload to be generated in more cases and
        // eliminates some offset and alignment checking in the rules file.

        ref obj.Addr fromAddr = ref heap(new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[0].Reg()), out ptr<obj.Addr> _addr_fromAddr);
        ssagen.AddAux(_addr_fromAddr, v);

        var genAddr = false;


        if (fromAddr.Name == obj.NAME_EXTERN || fromAddr.Name == obj.NAME_STATIC) 
            // Special case for a rule combines the bytes of gostring.
            // The v alignment might seem OK, but we don't want to load it
            // using an offset because relocation comes later.
            genAddr = strings.HasPrefix(fromAddr.Sym.Name, "go.string") || v.Type.Alignment() % 4 != 0 || fromAddr.Offset % 4 != 0;
        else 
            genAddr = fromAddr.Offset % 4 != 0;
                if (genAddr) { 
            // Load full address into the temp register.
            p = s.Prog(ppc64.AMOVD);
            p.From.Type = obj.TYPE_ADDR;
            p.From.Reg = v.Args[0].Reg();
            ssagen.AddAux(_addr_p.From, v); 
            // Load target using temp as base register
            // and offset zero. Setting NAME_NONE
            // prevents any extra offsets from being
            // added.
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP;
            fromAddr.Reg = ppc64.REGTMP; 
            // Clear the offset field and other
            // information that might be used
            // by the assembler to add to the
            // final offset value.
            fromAddr.Offset = 0;
            fromAddr.Name = obj.NAME_NONE;
            fromAddr.Sym = null;
        }
        p = s.Prog(v.Op.Asm());
        p.From = fromAddr;
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
        break;
    else if (v.Op == ssa.OpPPC64MOVHload || v.Op == ssa.OpPPC64MOVWZload || v.Op == ssa.OpPPC64MOVBZload || v.Op == ssa.OpPPC64MOVHZload || v.Op == ssa.OpPPC64FMOVDload || v.Op == ssa.OpPPC64FMOVSload) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.From, v);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64MOVDBRload || v.Op == ssa.OpPPC64MOVWBRload || v.Op == ssa.OpPPC64MOVHBRload) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64MOVDBRstore || v.Op == ssa.OpPPC64MOVWBRstore || v.Op == ssa.OpPPC64MOVHBRstore) 
        p = s.Prog(v.Op.Asm());
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
    else if (v.Op == ssa.OpPPC64MOVDloadidx || v.Op == ssa.OpPPC64MOVWloadidx || v.Op == ssa.OpPPC64MOVHloadidx || v.Op == ssa.OpPPC64MOVWZloadidx || v.Op == ssa.OpPPC64MOVBZloadidx || v.Op == ssa.OpPPC64MOVHZloadidx || v.Op == ssa.OpPPC64FMOVDloadidx || v.Op == ssa.OpPPC64FMOVSloadidx || v.Op == ssa.OpPPC64MOVDBRloadidx || v.Op == ssa.OpPPC64MOVWBRloadidx || v.Op == ssa.OpPPC64MOVHBRloadidx) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_MEM;
        p.From.Reg = v.Args[0].Reg();
        p.From.Index = v.Args[1].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg();
    else if (v.Op == ssa.OpPPC64MOVWstorezero || v.Op == ssa.OpPPC64MOVHstorezero || v.Op == ssa.OpPPC64MOVBstorezero) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = ppc64.REGZERO;
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
    else if (v.Op == ssa.OpPPC64MOVDstore || v.Op == ssa.OpPPC64MOVDstorezero) 

        // MOVDstore and MOVDstorezero become DS form instructions that are restricted
        // to offset values that are a multple of 4. If the offset field is not a
        // multiple of 4, then the full address of the store target is computed (base +
        // offset) and used as the new base register and the offset in the instruction
        // is set to 0.

        // This allows the MOVDstore and MOVDstorezero to be generated in more cases,
        // and prevents checking of the offset value and alignment in the rules.

        ref obj.Addr toAddr = ref heap(new obj.Addr(Type:obj.TYPE_MEM,Reg:v.Args[0].Reg()), out ptr<obj.Addr> _addr_toAddr);
        ssagen.AddAux(_addr_toAddr, v);

        if (toAddr.Offset % 4 != 0) {
            p = s.Prog(ppc64.AMOVD);
            p.From.Type = obj.TYPE_ADDR;
            p.From.Reg = v.Args[0].Reg();
            ssagen.AddAux(_addr_p.From, v);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP;
            toAddr.Reg = ppc64.REGTMP; 
            // Clear the offset field and other
            // information that might be used
            // by the assembler to add to the
            // final offset value.
            toAddr.Offset = 0;
            toAddr.Name = obj.NAME_NONE;
            toAddr.Sym = null;
        }
        p = s.Prog(v.Op.Asm());
        p.To = toAddr;
        p.From.Type = obj.TYPE_REG;
        if (v.Op == ssa.OpPPC64MOVDstorezero) {
            p.From.Reg = ppc64.REGZERO;
        }
        else
 {
            p.From.Reg = v.Args[1].Reg();
        }
    else if (v.Op == ssa.OpPPC64MOVWstore || v.Op == ssa.OpPPC64MOVHstore || v.Op == ssa.OpPPC64MOVBstore || v.Op == ssa.OpPPC64FMOVDstore || v.Op == ssa.OpPPC64FMOVSstore) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
        ssagen.AddAux(_addr_p.To, v);
    else if (v.Op == ssa.OpPPC64MOVDstoreidx || v.Op == ssa.OpPPC64MOVWstoreidx || v.Op == ssa.OpPPC64MOVHstoreidx || v.Op == ssa.OpPPC64MOVBstoreidx || v.Op == ssa.OpPPC64FMOVDstoreidx || v.Op == ssa.OpPPC64FMOVSstoreidx || v.Op == ssa.OpPPC64MOVDBRstoreidx || v.Op == ssa.OpPPC64MOVWBRstoreidx || v.Op == ssa.OpPPC64MOVHBRstoreidx) 
        p = s.Prog(v.Op.Asm());
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[2].Reg();
        p.To.Index = v.Args[1].Reg();
        p.To.Type = obj.TYPE_MEM;
        p.To.Reg = v.Args[0].Reg();
    else if (v.Op == ssa.OpPPC64ISEL || v.Op == ssa.OpPPC64ISELB) 
        // ISEL, ISELB
        // AuxInt value indicates condition: 0=LT 1=GT 2=EQ 4=GE 5=LE 6=NE
        // ISEL only accepts 0, 1, 2 condition values but the others can be
        // achieved by swapping operand order.
        // arg0 ? arg1 : arg2 with conditions LT, GT, EQ
        // arg0 ? arg2 : arg1 for conditions GE, LE, NE
        // ISELB is used when a boolean result is needed, returning 0 or 1
        p = s.Prog(ppc64.AISEL);
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = v.Reg(); 
        // For ISELB, boolean result 0 or 1. Use R0 for 0 operand to avoid load.
        r = new obj.Addr(Type:obj.TYPE_REG,Reg:ppc64.REG_R0);
        if (v.Op == ssa.OpPPC64ISEL) {
            r.Reg = v.Args[1].Reg();
        }
        if (v.AuxInt > 3) {
            p.Reg = r.Reg;
            p.SetFrom3Reg(v.Args[0].Reg());
        }
        else
 {
            p.Reg = v.Args[0].Reg();
            p.SetFrom3(r);
        }
        p.From.Type = obj.TYPE_CONST;
        p.From.Offset = v.AuxInt & 3;
    else if (v.Op == ssa.OpPPC64LoweredQuadZero || v.Op == ssa.OpPPC64LoweredQuadZeroShort) 
        // The LoweredQuad code generation
        // generates STXV instructions on
        // power9. The Short variation is used
        // if no loop is generated.

        // sizes >= 64 generate a loop as follows:

        // Set up loop counter in CTR, used by BC
        // XXLXOR clears VS32
        //       XXLXOR VS32,VS32,VS32
        //       MOVD len/64,REG_TMP
        //       MOVD REG_TMP,CTR
        //       loop:
        //       STXV VS32,0(R20)
        //       STXV VS32,16(R20)
        //       STXV VS32,32(R20)
        //       STXV VS32,48(R20)
        //       ADD  $64,R20
        //       BC   16, 0, loop

        // Bytes per iteration
        var ctr = v.AuxInt / 64; 

        // Remainder bytes
        var rem = v.AuxInt % 64; 

        // Only generate a loop if there is more
        // than 1 iteration.
        if (ctr > 1) { 
            // Set up VS32 (V0) to hold 0s
            p = s.Prog(ppc64.AXXLXOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;
            p.Reg = ppc64.REG_VS32; 

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

            // Don't generate padding for
            // loops with few iterations.
            if (ctr > 3) {
                p = s.Prog(obj.APCALIGN);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 16;
            } 

            // generate 4 STXVs to zero 64 bytes
            ptr<obj.Prog> top;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg(); 

            //  Save the top of loop
            if (top == null) {
                top = p;
            }
            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = 16;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = 32;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = 48; 

            // Increment address for the
            // 64 bytes just zeroed.
            p = s.Prog(ppc64.AADD);
            p.Reg = v.Args[0].Reg();
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = 64;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = v.Args[0].Reg(); 

            // Branch back to top of loop
            // based on CTR
            // BC with BO_BCTR generates bdnz
            p = s.Prog(ppc64.ABC);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = ppc64.BO_BCTR;
            p.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_BRANCH;
            p.To.SetTarget(top);
        }
        if (ctr == 1) {
            rem += 64;
        }
        var offset = int64(0);

        if (rem >= 16 && ctr <= 1) { 
            // If the XXLXOR hasn't already been
            // generated, do it here to initialize
            // VS32 (V0) to 0.
            p = s.Prog(ppc64.AXXLXOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;
            p.Reg = ppc64.REG_VS32;
        }
        while (rem >= 32) {
            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = offset;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = offset + 16;
            offset += 32;
            rem -= 32;
        } 
        // Generate 16 bytes
        if (rem >= 16) {
            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = offset;
            offset += 16;
            rem -= 16;
        }
        while (rem > 0) {
            op = ppc64.AMOVB;
            var size = int64(1);

            if (rem >= 8) 
                (op, size) = (ppc64.AMOVD, 8);            else if (rem >= 4) 
                (op, size) = (ppc64.AMOVW, 4);            else if (rem >= 2) 
                (op, size) = (ppc64.AMOVH, 2);                        p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = offset;
            rem -= size;
            offset += size;
        }
    else if (v.Op == ssa.OpPPC64LoweredZero || v.Op == ssa.OpPPC64LoweredZeroShort) 

        // Unaligned data doesn't hurt performance
        // for these instructions on power8.

        // For sizes >= 64 generate a loop as follows:

        // Set up loop counter in CTR, used by BC
        //       XXLXOR VS32,VS32,VS32
        //     MOVD len/32,REG_TMP
        //     MOVD REG_TMP,CTR
        //       MOVD $16,REG_TMP
        //     loop:
        //     STXVD2X VS32,(R0)(R20)
        //     STXVD2X VS32,(R31)(R20)
        //     ADD  $32,R20
        //     BC   16, 0, loop
        //
        // any remainder is done as described below

        // for sizes < 64 bytes, first clear as many doublewords as possible,
        // then handle the remainder
        //    MOVD R0,(R20)
        //    MOVD R0,8(R20)
        // .... etc.
        //
        // the remainder bytes are cleared using one or more
        // of the following instructions with the appropriate
        // offsets depending which instructions are needed
        //
        //    MOVW R0,n1(R20)    4 bytes
        //    MOVH R0,n2(R20)    2 bytes
        //    MOVB R0,n3(R20)    1 byte
        //
        // 7 bytes: MOVW, MOVH, MOVB
        // 6 bytes: MOVW, MOVH
        // 5 bytes: MOVW, MOVB
        // 3 bytes: MOVH, MOVB

        // each loop iteration does 32 bytes
        ctr = v.AuxInt / 32; 

        // remainder bytes
        rem = v.AuxInt % 32; 

        // only generate a loop if there is more
        // than 1 iteration.
        if (ctr > 1) { 
            // Set up VS32 (V0) to hold 0s
            p = s.Prog(ppc64.AXXLXOR);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;
            p.Reg = ppc64.REG_VS32; 

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

            // Set up R31 to hold index value 16
            p = s.Prog(ppc64.AMOVD);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = 16;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP; 

            // Don't add padding for alignment
            // with few loop iterations.
            if (ctr > 3) {
                p = s.Prog(obj.APCALIGN);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 16;
            } 

            // generate 2 STXVD2Xs to store 16 bytes
            // when this is a loop then the top must be saved
            top = ; 
            // This is the top of loop

            p = s.Prog(ppc64.ASTXVD2X);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Index = ppc64.REGZERO; 
            // Save the top of loop
            if (top == null) {
                top = p;
            }
            p = s.Prog(ppc64.ASTXVD2X);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Index = ppc64.REGTMP; 

            // Increment address for the
            // 4 doublewords just zeroed.
            p = s.Prog(ppc64.AADD);
            p.Reg = v.Args[0].Reg();
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = 32;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = v.Args[0].Reg(); 

            // Branch back to top of loop
            // based on CTR
            // BC with BO_BCTR generates bdnz
            p = s.Prog(ppc64.ABC);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = ppc64.BO_BCTR;
            p.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_BRANCH;
            p.To.SetTarget(top);
        }
        if (ctr == 1) {
            rem += 32;
        }
        offset = int64(0); 

        // first clear as many doublewords as possible
        // then clear remaining sizes as available
        while (rem > 0) {
            op = ppc64.AMOVB;
            size = int64(1);

            if (rem >= 8) 
                (op, size) = (ppc64.AMOVD, 8);            else if (rem >= 4) 
                (op, size) = (ppc64.AMOVW, 4);            else if (rem >= 2) 
                (op, size) = (ppc64.AMOVH, 2);                        p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = v.Args[0].Reg();
            p.To.Offset = offset;
            rem -= size;
            offset += size;
        }
    else if (v.Op == ssa.OpPPC64LoweredMove || v.Op == ssa.OpPPC64LoweredMoveShort) 

        var bytesPerLoop = int64(32); 
        // This will be used when moving more
        // than 8 bytes.  Moves start with
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
        //    MOVD $16,REG_TMP
        // top:
        //    LXVD2X (R0)(R21),VS32
        //    LXVD2X (R31)(R21),VS33
        //    ADD $32,R21
        //    STXVD2X VS32,(R0)(R20)
        //    STXVD2X VS33,(R31)(R20)
        //    ADD $32,R20
        //    BC 16,0,top
        // Bytes not moved by this loop are moved
        // with a combination of the following instructions,
        // starting with the largest sizes and generating as
        // many as needed, using the appropriate offset value.
        //    MOVD  n(R21),R31
        //    MOVD  R31,n(R20)
        //    MOVW  n1(R21),R31
        //    MOVW  R31,n1(R20)
        //    MOVH  n2(R21),R31
        //    MOVH  R31,n2(R20)
        //    MOVB  n3(R21),R31
        //    MOVB  R31,n3(R20)

        // Each loop iteration moves 32 bytes
        ctr = v.AuxInt / bytesPerLoop; 

        // Remainder after the loop
        rem = v.AuxInt % bytesPerLoop;

        var dstReg = v.Args[0].Reg();
        var srcReg = v.Args[1].Reg(); 

        // The set of registers used here, must match the clobbered reg list
        // in PPC64Ops.go.
        offset = int64(0); 

        // top of the loop
        top = ; 
        // Only generate looping code when loop counter is > 1 for >= 64 bytes
        if (ctr > 1) { 
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

            // Use REGTMP as index reg
            p = s.Prog(ppc64.AMOVD);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = 16;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP; 

            // Don't adding padding for
            // alignment with small iteration
            // counts.
            if (ctr > 3) {
                p = s.Prog(obj.APCALIGN);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 16;
            } 

            // Generate 16 byte loads and stores.
            // Use temp register for index (16)
            // on the second one.
            p = s.Prog(ppc64.ALXVD2X);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Index = ppc64.REGZERO;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;
            if (top == null) {
                top = p;
            }
            p = s.Prog(ppc64.ALXVD2X);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Index = ppc64.REGTMP;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS33; 

            // increment the src reg for next iteration
            p = s.Prog(ppc64.AADD);
            p.Reg = srcReg;
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = bytesPerLoop;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = srcReg; 

            // generate 16 byte stores
            p = s.Prog(ppc64.ASTXVD2X);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Index = ppc64.REGZERO;

            p = s.Prog(ppc64.ASTXVD2X);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS33;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Index = ppc64.REGTMP; 

            // increment the dst reg for next iteration
            p = s.Prog(ppc64.AADD);
            p.Reg = dstReg;
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = bytesPerLoop;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = dstReg; 

            // BC with BO_BCTR generates bdnz to branch on nonzero CTR
            // to loop top.
            p = s.Prog(ppc64.ABC);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = ppc64.BO_BCTR;
            p.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_BRANCH;
            p.To.SetTarget(top); 

            // srcReg and dstReg were incremented in the loop, so
            // later instructions start with offset 0.
            offset = int64(0);
        }
        if (ctr == 1) {
            rem += bytesPerLoop;
        }
        if (rem >= 16) { 
            // Generate 16 byte loads and stores.
            // Use temp register for index (value 16)
            // on the second one.
            p = s.Prog(ppc64.ALXVD2X);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Index = ppc64.REGZERO;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;

            p = s.Prog(ppc64.ASTXVD2X);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Index = ppc64.REGZERO;

            offset = 16;
            rem -= 16;

            if (rem >= 16) { 
                // Use REGTMP as index reg
                p = s.Prog(ppc64.AMOVD);
                p.From.Type = obj.TYPE_CONST;
                p.From.Offset = 16;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REGTMP;

                p = s.Prog(ppc64.ALXVD2X);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = srcReg;
                p.From.Index = ppc64.REGTMP;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_VS32;

                p = s.Prog(ppc64.ASTXVD2X);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = ppc64.REG_VS32;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = dstReg;
                p.To.Index = ppc64.REGTMP;

                offset = 32;
                rem -= 16;
            }
        }
        while (rem > 0) {
            op = ppc64.AMOVB;
            size = int64(1);

            if (rem >= 8) 
                (op, size) = (ppc64.AMOVD, 8);            else if (rem >= 4) 
                (op, size) = (ppc64.AMOVWZ, 4);            else if (rem >= 2) 
                (op, size) = (ppc64.AMOVH, 2);            // Load
            p = s.Prog(op);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP;
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset; 

            // Store
            p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REGTMP;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset;
            rem -= size;
            offset += size;
        }
    else if (v.Op == ssa.OpPPC64LoweredQuadMove || v.Op == ssa.OpPPC64LoweredQuadMoveShort) 
        bytesPerLoop = int64(64); 
        // This is used when moving more
        // than 8 bytes on power9.  Moves start with
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
        //      MOVD len/32,REG_TMP
        //      MOVD REG_TMP,CTR
        // top:
        //      LXV 0(R21),VS32
        //      LXV 16(R21),VS33
        //      ADD $32,R21
        //      STXV VS32,0(R20)
        //      STXV VS33,16(R20)
        //      ADD $32,R20
        //      BC 16,0,top
        // Bytes not moved by this loop are moved
        // with a combination of the following instructions,
        // starting with the largest sizes and generating as
        // many as needed, using the appropriate offset value.
        //      MOVD  n(R21),R31
        //      MOVD  R31,n(R20)
        //      MOVW  n1(R21),R31
        //      MOVW  R31,n1(R20)
        //      MOVH  n2(R21),R31
        //      MOVH  R31,n2(R20)
        //      MOVB  n3(R21),R31
        //      MOVB  R31,n3(R20)

        // Each loop iteration moves 32 bytes
        ctr = v.AuxInt / bytesPerLoop; 

        // Remainder after the loop
        rem = v.AuxInt % bytesPerLoop;

        dstReg = v.Args[0].Reg();
        srcReg = v.Args[1].Reg();

        offset = int64(0); 

        // top of the loop
        top = ; 

        // Only generate looping code when loop counter is > 1 for >= 64 bytes
        if (ctr > 1) { 
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

            p = s.Prog(obj.APCALIGN);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = 16; 

            // Generate 16 byte loads and stores.
            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;
            if (top == null) {
                top = p;
            }
            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset + 16;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS33; 

            // generate 16 byte stores
            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS33;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset + 16; 

            // Generate 16 byte loads and stores.
            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset + 32;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;

            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset + 48;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS33; 

            // generate 16 byte stores
            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset + 32;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS33;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset + 48; 

            // increment the src reg for next iteration
            p = s.Prog(ppc64.AADD);
            p.Reg = srcReg;
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = bytesPerLoop;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = srcReg; 

            // increment the dst reg for next iteration
            p = s.Prog(ppc64.AADD);
            p.Reg = dstReg;
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = bytesPerLoop;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = dstReg; 

            // BC with BO_BCTR generates bdnz to branch on nonzero CTR
            // to loop top.
            p = s.Prog(ppc64.ABC);
            p.From.Type = obj.TYPE_CONST;
            p.From.Offset = ppc64.BO_BCTR;
            p.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_BRANCH;
            p.To.SetTarget(top); 

            // srcReg and dstReg were incremented in the loop, so
            // later instructions start with offset 0.
            offset = int64(0);
        }
        if (ctr == 1) {
            rem += bytesPerLoop;
        }
        if (rem >= 32) {
            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;

            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = 16;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS33;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS33;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = 16;

            offset = 32;
            rem -= 32;
        }
        if (rem >= 16) { 
            // Generate 16 byte loads and stores.
            p = s.Prog(ppc64.ALXV);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset;
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_VS32;

            p = s.Prog(ppc64.ASTXV);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_VS32;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset;

            offset += 16;
            rem -= 16;

            if (rem >= 16) {
                p = s.Prog(ppc64.ALXV);
                p.From.Type = obj.TYPE_MEM;
                p.From.Reg = srcReg;
                p.From.Offset = offset;
                p.To.Type = obj.TYPE_REG;
                p.To.Reg = ppc64.REG_VS32;

                p = s.Prog(ppc64.ASTXV);
                p.From.Type = obj.TYPE_REG;
                p.From.Reg = ppc64.REG_VS32;
                p.To.Type = obj.TYPE_MEM;
                p.To.Reg = dstReg;
                p.To.Offset = offset;

                offset += 16;
                rem -= 16;
            }
        }
        while (rem > 0) {
            op = ppc64.AMOVB;
            size = int64(1);

            if (rem >= 8) 
                (op, size) = (ppc64.AMOVD, 8);            else if (rem >= 4) 
                (op, size) = (ppc64.AMOVWZ, 4);            else if (rem >= 2) 
                (op, size) = (ppc64.AMOVH, 2);            // Load
            p = s.Prog(op);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP;
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = srcReg;
            p.From.Offset = offset; 

            // Store
            p = s.Prog(op);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REGTMP;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = dstReg;
            p.To.Offset = offset;
            rem -= size;
            offset += size;
        }
    else if (v.Op == ssa.OpPPC64CALLstatic) 
        s.Call(v);
    else if (v.Op == ssa.OpPPC64CALLclosure || v.Op == ssa.OpPPC64CALLinter) 
        p = s.Prog(ppc64.AMOVD);
        p.From.Type = obj.TYPE_REG;
        p.From.Reg = v.Args[0].Reg();
        p.To.Type = obj.TYPE_REG;
        p.To.Reg = ppc64.REG_LR;

        if (v.Args[0].Reg() != ppc64.REG_R12) {
            v.Fatalf("Function address for %v should be in R12 %d but is in %d", v.LongString(), ppc64.REG_R12, p.From.Reg);
        }
        var pp = s.Call(v);
        pp.To.Reg = ppc64.REG_LR; 

        // Insert a hint this is not a subroutine return.
        pp.SetFrom3Const(1);

        if (@base.Ctxt.Flag_shared) { 
            // When compiling Go into PIC, the function we just
            // called via pointer might have been implemented in
            // a separate module and so overwritten the TOC
            // pointer in R2; reload it.
            var q = s.Prog(ppc64.AMOVD);
            q.From.Type = obj.TYPE_MEM;
            q.From.Offset = 24;
            q.From.Reg = ppc64.REGSP;
            q.To.Type = obj.TYPE_REG;
            q.To.Reg = ppc64.REG_R2;
        }
    else if (v.Op == ssa.OpPPC64LoweredWB) 
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = v.Aux._<ptr<obj.LSym>>();
    else if (v.Op == ssa.OpPPC64LoweredPanicBoundsA || v.Op == ssa.OpPPC64LoweredPanicBoundsB || v.Op == ssa.OpPPC64LoweredPanicBoundsC) 
        p = s.Prog(obj.ACALL);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = ssagen.BoundsCheckFunc[v.AuxInt];
        s.UseArgs(16); // space used in callee args area by assembly stubs
    else if (v.Op == ssa.OpPPC64LoweredNilCheck) 
        if (buildcfg.GOOS == "aix") { 
            // CMP Rarg0, R0
            // BNE 2(PC)
            // STW R0, 0(R0)
            // NOP (so the BNE has somewhere to land)

            // CMP Rarg0, R0
            p = s.Prog(ppc64.ACMP);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = v.Args[0].Reg();
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REG_R0; 

            // BNE 2(PC)
            p2 = s.Prog(ppc64.ABNE);
            p2.To.Type = obj.TYPE_BRANCH; 

            // STW R0, 0(R0)
            // Write at 0 is forbidden and will trigger a SIGSEGV
            p = s.Prog(ppc64.AMOVW);
            p.From.Type = obj.TYPE_REG;
            p.From.Reg = ppc64.REG_R0;
            p.To.Type = obj.TYPE_MEM;
            p.To.Reg = ppc64.REG_R0; 

            // NOP (so the BNE has somewhere to land)
            var nop = s.Prog(obj.ANOP);
            p2.To.SetTarget(nop);
        }
        else
 { 
            // Issue a load which will fault if arg is nil.
            p = s.Prog(ppc64.AMOVBZ);
            p.From.Type = obj.TYPE_MEM;
            p.From.Reg = v.Args[0].Reg();
            ssagen.AddAux(_addr_p.From, v);
            p.To.Type = obj.TYPE_REG;
            p.To.Reg = ppc64.REGTMP;
        }
        if (logopt.Enabled()) {
            logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
        }
        if (@base.Debug.Nil != 0 && v.Pos.Line() > 1) { // v.Pos.Line()==1 in generated wrappers
            @base.WarnfAt(v.Pos, "generated nil check");
        }
    else if (v.Op == ssa.OpPPC64Equal || v.Op == ssa.OpPPC64NotEqual || v.Op == ssa.OpPPC64LessThan || v.Op == ssa.OpPPC64FLessThan || v.Op == ssa.OpPPC64LessEqual || v.Op == ssa.OpPPC64GreaterThan || v.Op == ssa.OpPPC64FGreaterThan || v.Op == ssa.OpPPC64GreaterEqual || v.Op == ssa.OpPPC64FLessEqual || v.Op == ssa.OpPPC64FGreaterEqual) 
        v.Fatalf("Pseudo-op should not make it to codegen: %s ###\n", v.LongString());
    else if (v.Op == ssa.OpPPC64InvertFlags) 
        v.Fatalf("InvertFlags should never make it to codegen %v", v.LongString());
    else if (v.Op == ssa.OpPPC64FlagEQ || v.Op == ssa.OpPPC64FlagLT || v.Op == ssa.OpPPC64FlagGT) 
        v.Fatalf("Flag* ops should never make it to codegen %v", v.LongString());
    else if (v.Op == ssa.OpClobber || v.Op == ssa.OpClobberReg)     else 
        v.Fatalf("genValue not implemented: %s", v.LongString());
    }



private static void ssaGenBlock(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref ssa.Block next = ref _addr_next.val;


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
        s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[1].Block()));
        if (b.Succs[0].Block() != next) {
            p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockPlain) 
        if (b.Succs[0].Block() != next) {
            p = s.Prog(obj.AJMP);
            p.To.Type = obj.TYPE_BRANCH;
            s.Branches = append(s.Branches, new ssagen.Branch(P:p,B:b.Succs[0].Block()));
        }
    else if (b.Kind == ssa.BlockExit)     else if (b.Kind == ssa.BlockRet) 
        s.Prog(obj.ARET);
    else if (b.Kind == ssa.BlockRetJmp) 
        p = s.Prog(obj.AJMP);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = b.Aux._<ptr<obj.LSym>>();
    else if (b.Kind == ssa.BlockPPC64EQ || b.Kind == ssa.BlockPPC64NE || b.Kind == ssa.BlockPPC64LT || b.Kind == ssa.BlockPPC64GE || b.Kind == ssa.BlockPPC64LE || b.Kind == ssa.BlockPPC64GT || b.Kind == ssa.BlockPPC64FLT || b.Kind == ssa.BlockPPC64FGE || b.Kind == ssa.BlockPPC64FLE || b.Kind == ssa.BlockPPC64FGT) 
        var jmp = blockJump[b.Kind];

        if (next == b.Succs[0].Block()) 
            s.Br(jmp.invasm, b.Succs[1].Block());
            if (jmp.invasmun) { 
                // TODO: The second branch is probably predict-not-taken since it is for FP unordered
                s.Br(ppc64.ABVS, b.Succs[1].Block());
            }
        else if (next == b.Succs[1].Block()) 
            s.Br(jmp.asm, b.Succs[0].Block());
            if (jmp.asmeq) {
                s.Br(ppc64.ABEQ, b.Succs[0].Block());
            }
        else 
            if (b.Likely != ssa.BranchUnlikely) {
                s.Br(jmp.asm, b.Succs[0].Block());
                if (jmp.asmeq) {
                    s.Br(ppc64.ABEQ, b.Succs[0].Block());
                }
                s.Br(obj.AJMP, b.Succs[1].Block());
            }
            else
 {
                s.Br(jmp.invasm, b.Succs[1].Block());
                if (jmp.invasmun) { 
                    // TODO: The second branch is probably predict-not-taken since it is for FP unordered
                    s.Br(ppc64.ABVS, b.Succs[1].Block());
                }
                s.Br(obj.AJMP, b.Succs[0].Block());
            }
            else 
        b.Fatalf("branch not implemented: %s", b.LongString());
    }

} // end ppc64_package
