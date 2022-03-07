// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package amd64 -- go2cs converted at 2022 March 06 23:14:47 UTC
// import "cmd/compile/internal/amd64" ==> using amd64 = go.cmd.compile.@internal.amd64_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\amd64\ggen.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using x86 = go.cmd.@internal.obj.x86_package;
using buildcfg = go.@internal.buildcfg_package;

namespace go.cmd.compile.@internal;

public static partial class amd64_package {

    // no floating point in note handlers on Plan 9
private static var isPlan9 = buildcfg.GOOS == "plan9";

// DUFFZERO consists of repeated blocks of 4 MOVUPSs + LEAQ,
// See runtime/mkduff.go.
private static readonly nint dzBlocks = 16; // number of MOV/ADD blocks
private static readonly nint dzBlockLen = 4; // number of clears per block
private static readonly nint dzBlockSize = 23; // size of instructions in a single block
private static readonly nint dzMovSize = 5; // size of single MOV instruction w/ offset
private static readonly nint dzLeaqSize = 4; // size of single LEAQ instruction
private static readonly nint dzClearStep = 16; // number of bytes cleared by each MOV instruction

private static readonly var dzClearLen = dzClearStep * dzBlockLen; // bytes cleared by one block
private static readonly var dzSize = dzBlocks * dzBlockSize;


// dzOff returns the offset for a jump into DUFFZERO.
// b is the number of bytes to zero.
private static long dzOff(long b) {
    var off = int64(dzSize);
    off -= b / dzClearLen * dzBlockSize;
    var tailLen = b % dzClearLen;
    if (tailLen >= dzClearStep) {
        off -= dzLeaqSize + dzMovSize * (tailLen / dzClearStep);
    }
    return off;

}

// duffzeroDI returns the pre-adjustment to DI for a call to DUFFZERO.
// b is the number of bytes to zero.
private static long dzDI(long b) {
    var tailLen = b % dzClearLen;
    if (tailLen < dzClearStep) {
        return 0;
    }
    var tailSteps = tailLen / dzClearStep;
    return -dzClearStep * (dzBlockLen - tailSteps);

}

private static ptr<obj.Prog> zerorange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_state) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint state = ref _addr_state.val;

    const nint r13 = 1 << (int)(iota); // if R13 is already zeroed.
    const var x15 = 0; // if X15 is already zeroed. Note: in new ABI, X15 is always zero.

    if (cnt == 0) {
        return _addr_p!;
    }
    if (cnt % int64(types.RegSize) != 0) { 
        // should only happen with nacl
        if (cnt % int64(types.PtrSize) != 0) {
            @base.Fatalf("zerorange count not a multiple of widthptr %d", cnt);
        }
        if (state & r13 == 0.val) {
            p = pp.Append(p, x86.AMOVQ, obj.TYPE_CONST, 0, 0, obj.TYPE_REG, x86.REG_R13, 0);
            state |= r13;
        }
        p = pp.Append(p, x86.AMOVL, obj.TYPE_REG, x86.REG_R13, 0, obj.TYPE_MEM, x86.REG_SP, off);
        off += int64(types.PtrSize);
        cnt -= int64(types.PtrSize);

    }
    if (cnt == 8) {
        if (state & r13 == 0.val) {
            p = pp.Append(p, x86.AMOVQ, obj.TYPE_CONST, 0, 0, obj.TYPE_REG, x86.REG_R13, 0);
            state |= r13;
        }
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_R13, 0, obj.TYPE_MEM, x86.REG_SP, off);

    }
    else if (!isPlan9 && cnt <= int64(8 * types.RegSize)) {
        if (!buildcfg.Experiment.RegabiG && state & x15 == 0.val) {
            p = pp.Append(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X15, 0, obj.TYPE_REG, x86.REG_X15, 0);
            state |= x15;
        }
        for (var i = int64(0); i < cnt / 16; i++) {
            p = pp.Append(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X15, 0, obj.TYPE_MEM, x86.REG_SP, off + i * 16);
        }

        if (cnt % 16 != 0) {
            p = pp.Append(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X15, 0, obj.TYPE_MEM, x86.REG_SP, off + cnt - int64(16));
        }
    }
    else if (!isPlan9 && (cnt <= int64(128 * types.RegSize))) {
        if (!buildcfg.Experiment.RegabiG && state & x15 == 0.val) {
            p = pp.Append(p, x86.AXORPS, obj.TYPE_REG, x86.REG_X15, 0, obj.TYPE_REG, x86.REG_X15, 0);
            state |= x15;
        }
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_DI, 0, obj.TYPE_REG, x86.REG_R12, 0); 
        // Emit duffzero call
        p = pp.Append(p, leaptr, obj.TYPE_MEM, x86.REG_SP, off + dzDI(cnt), obj.TYPE_REG, x86.REG_DI, 0);
        p = pp.Append(p, obj.ADUFFZERO, obj.TYPE_NONE, 0, 0, obj.TYPE_ADDR, 0, dzOff(cnt));
        p.To.Sym = ir.Syms.Duffzero;
        if (cnt % 16 != 0) {
            p = pp.Append(p, x86.AMOVUPS, obj.TYPE_REG, x86.REG_X15, 0, obj.TYPE_MEM, x86.REG_DI, -int64(8));
        }
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_R12, 0, obj.TYPE_REG, x86.REG_DI, 0);


    }
    else
 { 
        // When the register ABI is in effect, at this point in the
        // prolog we may have live values in all of RAX,RDI,RCX. Save
        // them off to registers before the REPSTOSQ below, then
        // restore. Note that R12 and R13 are always available as
        // scratch regs; here we also use R15 (this is safe to do
        // since there won't be any globals accessed in the prolog).
        // See rewriteToUseGot() in obj6.go for more on r15 use.

        // Save rax/rdi/rcx
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_DI, 0, obj.TYPE_REG, x86.REG_R12, 0);
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_AX, 0, obj.TYPE_REG, x86.REG_R13, 0);
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_CX, 0, obj.TYPE_REG, x86.REG_R15, 0); 

        // Set up the REPSTOSQ and kick it off.
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_CONST, 0, 0, obj.TYPE_REG, x86.REG_AX, 0);
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_CONST, 0, cnt / int64(types.RegSize), obj.TYPE_REG, x86.REG_CX, 0);
        p = pp.Append(p, leaptr, obj.TYPE_MEM, x86.REG_SP, off, obj.TYPE_REG, x86.REG_DI, 0);
        p = pp.Append(p, x86.AREP, obj.TYPE_NONE, 0, 0, obj.TYPE_NONE, 0, 0);
        p = pp.Append(p, x86.ASTOSQ, obj.TYPE_NONE, 0, 0, obj.TYPE_NONE, 0, 0); 

        // Restore rax/rdi/rcx
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_R12, 0, obj.TYPE_REG, x86.REG_DI, 0);
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_R13, 0, obj.TYPE_REG, x86.REG_AX, 0);
        p = pp.Append(p, x86.AMOVQ, obj.TYPE_REG, x86.REG_R15, 0, obj.TYPE_REG, x86.REG_CX, 0); 

        // Record the fact that r13 is no longer zero.
        state &= ~uint32(r13);

    }
    return _addr_p!;

}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;
 
    // This is a hardware nop (1-byte 0x90) instruction,
    // even though we describe it as an explicit XCHGL here.
    // Particularly, this does not zero the high 32 bits
    // like typical *L opcodes.
    // (gas assembles "xchg %eax,%eax" to 0x87 0xc0, which
    // does zero the high 32 bits.)
    var p = pp.Prog(x86.AXCHGL);
    p.From.Type = obj.TYPE_REG;
    p.From.Reg = x86.REG_AX;
    p.To.Type = obj.TYPE_REG;
    p.To.Reg = x86.REG_AX;
    return _addr_p!;

}

} // end amd64_package
