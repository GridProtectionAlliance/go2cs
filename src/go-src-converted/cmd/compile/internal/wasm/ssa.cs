// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2022 March 13 05:58:55 UTC
// import "cmd/compile/internal/wasm" ==> using wasm = go.cmd.compile.@internal.wasm_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\wasm\ssa.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using logopt = cmd.compile.@internal.logopt_package;
using objw = cmd.compile.@internal.objw_package;
using ssa = cmd.compile.@internal.ssa_package;
using ssagen = cmd.compile.@internal.ssagen_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using wasm = cmd.@internal.obj.wasm_package;
using buildcfg = @internal.buildcfg_package;

public static partial class wasm_package {

public static void Init(ptr<ssagen.ArchInfo> _addr_arch) {
    ref ssagen.ArchInfo arch = ref _addr_arch.val;

    arch.LinkArch = _addr_wasm.Linkwasm;
    arch.REGSP = wasm.REG_SP;
    arch.MAXWIDTH = 1 << 50;

    arch.ZeroRange = zeroRange;
    arch.Ginsnop = ginsnop;
    arch.Ginsnopdefer = ginsnop;

    arch.SSAMarkMoves = ssaMarkMoves;
    arch.SSAGenValue = ssaGenValue;
    arch.SSAGenBlock = ssaGenBlock;
}

private static ptr<obj.Prog> zeroRange(ptr<objw.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_state) {
    ref objw.Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;
    ref uint state = ref _addr_state.val;

    if (cnt == 0) {
        return _addr_p!;
    }
    if (cnt % 8 != 0) {
        @base.Fatalf("zerorange count not a multiple of widthptr %d", cnt);
    }
    {
        var i = int64(0);

        while (i < cnt) {
            p = pp.Append(p, wasm.AGet, obj.TYPE_REG, wasm.REG_SP, 0, 0, 0, 0);
            p = pp.Append(p, wasm.AI64Const, obj.TYPE_CONST, 0, 0, 0, 0, 0);
            p = pp.Append(p, wasm.AI64Store, 0, 0, 0, obj.TYPE_CONST, 0, off + i);
            i += 8;
        }
    }

    return _addr_p!;
}

private static ptr<obj.Prog> ginsnop(ptr<objw.Progs> _addr_pp) {
    ref objw.Progs pp = ref _addr_pp.val;

    return _addr_pp.Prog(wasm.ANop)!;
}

private static void ssaMarkMoves(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;

}

private static void ssaGenBlock(ptr<ssagen.State> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next) => func((_, panic, _) => {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Block b = ref _addr_b.val;
    ref ssa.Block next = ref _addr_next.val;


    if (b.Kind == ssa.BlockPlain) 
        if (next != b.Succs[0].Block()) {
            s.Br(obj.AJMP, b.Succs[0].Block());
        }
    else if (b.Kind == ssa.BlockIf) 

        if (next == b.Succs[0].Block()) 
            // if false, jump to b.Succs[1]
            getValue32(_addr_s, _addr_b.Controls[0]);
            s.Prog(wasm.AI32Eqz);
            s.Prog(wasm.AIf);
            s.Br(obj.AJMP, b.Succs[1].Block());
            s.Prog(wasm.AEnd);
        else if (next == b.Succs[1].Block()) 
            // if true, jump to b.Succs[0]
            getValue32(_addr_s, _addr_b.Controls[0]);
            s.Prog(wasm.AIf);
            s.Br(obj.AJMP, b.Succs[0].Block());
            s.Prog(wasm.AEnd);
        else 
            // if true, jump to b.Succs[0], else jump to b.Succs[1]
            getValue32(_addr_s, _addr_b.Controls[0]);
            s.Prog(wasm.AIf);
            s.Br(obj.AJMP, b.Succs[0].Block());
            s.Prog(wasm.AEnd);
            s.Br(obj.AJMP, b.Succs[1].Block());
            else if (b.Kind == ssa.BlockRet) 
        s.Prog(obj.ARET);
    else if (b.Kind == ssa.BlockRetJmp) 
        var p = s.Prog(obj.ARET);
        p.To.Type = obj.TYPE_MEM;
        p.To.Name = obj.NAME_EXTERN;
        p.To.Sym = b.Aux._<ptr<obj.LSym>>();
    else if (b.Kind == ssa.BlockExit)     else if (b.Kind == ssa.BlockDefer) 
        p = s.Prog(wasm.AGet);
        p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:wasm.REG_RET0);
        s.Prog(wasm.AI64Eqz);
        s.Prog(wasm.AI32Eqz);
        s.Prog(wasm.AIf);
        s.Br(obj.AJMP, b.Succs[1].Block());
        s.Prog(wasm.AEnd);
        if (next != b.Succs[0].Block()) {
            s.Br(obj.AJMP, b.Succs[0].Block());
        }
    else 
        panic("unexpected block");
    // Entry point for the next block. Used by the JMP in goToBlock.
    s.Prog(wasm.ARESUMEPOINT);

    if (s.OnWasmStackSkipped != 0) {
        panic("wasm: bad stack");
    }
});

private static void ssaGenValue(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) => func((_, panic, _) => {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpWasmLoweredStaticCall || v.Op == ssa.OpWasmLoweredClosureCall || v.Op == ssa.OpWasmLoweredInterCall) 
        s.PrepareCall(v);
        {
            ptr<ssa.AuxCall> call__prev1 = call;

            ptr<ssa.AuxCall> (call, ok) = v.Aux._<ptr<ssa.AuxCall>>();

            if (ok && call.Fn == ir.Syms.Deferreturn) { 
                // add a resume point before call to deferreturn so it can be called again via jmpdefer
                s.Prog(wasm.ARESUMEPOINT);
            }

            call = call__prev1;

        }
        if (v.Op == ssa.OpWasmLoweredClosureCall) {
            getValue64(_addr_s, _addr_v.Args[1]);
            setReg(_addr_s, wasm.REG_CTXT);
        }
        {
            ptr<ssa.AuxCall> call__prev1 = call;

            (call, ok) = v.Aux._<ptr<ssa.AuxCall>>();

            if (ok && call.Fn != null) {
                var sym = call.Fn;
                var p = s.Prog(obj.ACALL);
                p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:sym);
                p.Pos = v.Pos;
            }
            else
 {
                getValue64(_addr_s, _addr_v.Args[0]);
                p = s.Prog(obj.ACALL);
                p.To = new obj.Addr(Type:obj.TYPE_NONE);
                p.Pos = v.Pos;
            }

            call = call__prev1;

        }
    else if (v.Op == ssa.OpWasmLoweredMove) 
        getValue32(_addr_s, _addr_v.Args[0]);
        getValue32(_addr_s, _addr_v.Args[1]);
        i32Const(_addr_s, int32(v.AuxInt));
        p = s.Prog(wasm.ACall);
        p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.WasmMove);
    else if (v.Op == ssa.OpWasmLoweredZero) 
        getValue32(_addr_s, _addr_v.Args[0]);
        i32Const(_addr_s, int32(v.AuxInt));
        p = s.Prog(wasm.ACall);
        p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.WasmZero);
    else if (v.Op == ssa.OpWasmLoweredNilCheck) 
        getValue64(_addr_s, _addr_v.Args[0]);
        s.Prog(wasm.AI64Eqz);
        s.Prog(wasm.AIf);
        p = s.Prog(wasm.ACALLNORESUME);
        p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.SigPanic);
        s.Prog(wasm.AEnd);
        if (logopt.Enabled()) {
            logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
        }
        if (@base.Debug.Nil != 0 && v.Pos.Line() > 1) { // v.Pos.Line()==1 in generated wrappers
            @base.WarnfAt(v.Pos, "generated nil check");
        }
    else if (v.Op == ssa.OpWasmLoweredWB) 
        getValue64(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        p = s.Prog(wasm.ACALLNORESUME); // TODO(neelance): If possible, turn this into a simple wasm.ACall).
        p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:v.Aux.(*obj.LSym));
    else if (v.Op == ssa.OpWasmI64Store8 || v.Op == ssa.OpWasmI64Store16 || v.Op == ssa.OpWasmI64Store32 || v.Op == ssa.OpWasmI64Store || v.Op == ssa.OpWasmF32Store || v.Op == ssa.OpWasmF64Store) 
        getValue32(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        p = s.Prog(v.Op.Asm());
        p.To = new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt);
    else if (v.Op == ssa.OpStoreReg) 
        getReg(_addr_s, wasm.REG_SP);
        getValue64(_addr_s, _addr_v.Args[0]);
        p = s.Prog(storeOp(_addr_v.Type));
        ssagen.AddrAuto(_addr_p.To, v);
    else if (v.Op == ssa.OpClobber || v.Op == ssa.OpClobberReg)     else 
        if (v.Type.IsMemory()) {
            return ;
        }
        if (v.OnWasmStack) {
            s.OnWasmStackSkipped++; 
            // If a Value is marked OnWasmStack, we don't generate the value and store it to a register now.
            // Instead, we delay the generation to when the value is used and then directly generate it on the WebAssembly stack.
            return ;
        }
        ssaGenValueOnStack(_addr_s, _addr_v, true);
        if (s.OnWasmStackSkipped != 0) {
            panic("wasm: bad stack");
        }
        setReg(_addr_s, v.Reg());
    });

private static void ssaGenValueOnStack(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v, bool extend) => func((_, panic, _) => {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpWasmLoweredGetClosurePtr) 
        getReg(_addr_s, wasm.REG_CTXT);
    else if (v.Op == ssa.OpWasmLoweredGetCallerPC) 
        var p = s.Prog(wasm.AI64Load); 
        // Caller PC is stored 8 bytes below first parameter.
        p.From = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_PARAM,Offset:-8,);
    else if (v.Op == ssa.OpWasmLoweredGetCallerSP) 
        p = s.Prog(wasm.AGet); 
        // Caller SP is the address of the first parameter.
        p.From = new obj.Addr(Type:obj.TYPE_ADDR,Name:obj.NAME_PARAM,Reg:wasm.REG_SP,Offset:0,);
    else if (v.Op == ssa.OpWasmLoweredAddr) 
        if (v.Aux == null) { // address of off(SP), no symbol
            getValue64(_addr_s, _addr_v.Args[0]);
            i64Const(_addr_s, v.AuxInt);
            s.Prog(wasm.AI64Add);
            break;
        }
        p = s.Prog(wasm.AGet);
        p.From.Type = obj.TYPE_ADDR;
        switch (v.Aux.type()) {
            case ptr<obj.LSym> _:
                ssagen.AddAux(_addr_p.From, v);
                break;
            case ptr<ir.Name> _:
                p.From.Reg = v.Args[0].Reg();
                ssagen.AddAux(_addr_p.From, v);
                break;
            default:
            {
                panic("wasm: bad LoweredAddr");
                break;
            }

        }
    else if (v.Op == ssa.OpWasmLoweredConvert) 
        getValue64(_addr_s, _addr_v.Args[0]);
    else if (v.Op == ssa.OpWasmSelect) 
        getValue64(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        getValue32(_addr_s, _addr_v.Args[2]);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpWasmI64AddConst) 
        getValue64(_addr_s, _addr_v.Args[0]);
        i64Const(_addr_s, v.AuxInt);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpWasmI64Const) 
        i64Const(_addr_s, v.AuxInt);
    else if (v.Op == ssa.OpWasmF32Const) 
        f32Const(_addr_s, v.AuxFloat());
    else if (v.Op == ssa.OpWasmF64Const) 
        f64Const(_addr_s, v.AuxFloat());
    else if (v.Op == ssa.OpWasmI64Load8U || v.Op == ssa.OpWasmI64Load8S || v.Op == ssa.OpWasmI64Load16U || v.Op == ssa.OpWasmI64Load16S || v.Op == ssa.OpWasmI64Load32U || v.Op == ssa.OpWasmI64Load32S || v.Op == ssa.OpWasmI64Load || v.Op == ssa.OpWasmF32Load || v.Op == ssa.OpWasmF64Load) 
        getValue32(_addr_s, _addr_v.Args[0]);
        p = s.Prog(v.Op.Asm());
        p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt);
    else if (v.Op == ssa.OpWasmI64Eqz) 
        getValue64(_addr_s, _addr_v.Args[0]);
        s.Prog(v.Op.Asm());
        if (extend) {
            s.Prog(wasm.AI64ExtendI32U);
        }
    else if (v.Op == ssa.OpWasmI64Eq || v.Op == ssa.OpWasmI64Ne || v.Op == ssa.OpWasmI64LtS || v.Op == ssa.OpWasmI64LtU || v.Op == ssa.OpWasmI64GtS || v.Op == ssa.OpWasmI64GtU || v.Op == ssa.OpWasmI64LeS || v.Op == ssa.OpWasmI64LeU || v.Op == ssa.OpWasmI64GeS || v.Op == ssa.OpWasmI64GeU || v.Op == ssa.OpWasmF32Eq || v.Op == ssa.OpWasmF32Ne || v.Op == ssa.OpWasmF32Lt || v.Op == ssa.OpWasmF32Gt || v.Op == ssa.OpWasmF32Le || v.Op == ssa.OpWasmF32Ge || v.Op == ssa.OpWasmF64Eq || v.Op == ssa.OpWasmF64Ne || v.Op == ssa.OpWasmF64Lt || v.Op == ssa.OpWasmF64Gt || v.Op == ssa.OpWasmF64Le || v.Op == ssa.OpWasmF64Ge) 
        getValue64(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        s.Prog(v.Op.Asm());
        if (extend) {
            s.Prog(wasm.AI64ExtendI32U);
        }
    else if (v.Op == ssa.OpWasmI64Add || v.Op == ssa.OpWasmI64Sub || v.Op == ssa.OpWasmI64Mul || v.Op == ssa.OpWasmI64DivU || v.Op == ssa.OpWasmI64RemS || v.Op == ssa.OpWasmI64RemU || v.Op == ssa.OpWasmI64And || v.Op == ssa.OpWasmI64Or || v.Op == ssa.OpWasmI64Xor || v.Op == ssa.OpWasmI64Shl || v.Op == ssa.OpWasmI64ShrS || v.Op == ssa.OpWasmI64ShrU || v.Op == ssa.OpWasmI64Rotl || v.Op == ssa.OpWasmF32Add || v.Op == ssa.OpWasmF32Sub || v.Op == ssa.OpWasmF32Mul || v.Op == ssa.OpWasmF32Div || v.Op == ssa.OpWasmF32Copysign || v.Op == ssa.OpWasmF64Add || v.Op == ssa.OpWasmF64Sub || v.Op == ssa.OpWasmF64Mul || v.Op == ssa.OpWasmF64Div || v.Op == ssa.OpWasmF64Copysign) 
        getValue64(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpWasmI32Rotl) 
        getValue32(_addr_s, _addr_v.Args[0]);
        getValue32(_addr_s, _addr_v.Args[1]);
        s.Prog(wasm.AI32Rotl);
        s.Prog(wasm.AI64ExtendI32U);
    else if (v.Op == ssa.OpWasmI64DivS) 
        getValue64(_addr_s, _addr_v.Args[0]);
        getValue64(_addr_s, _addr_v.Args[1]);
        if (v.Type.Size() == 8) { 
            // Division of int64 needs helper function wasmDiv to handle the MinInt64 / -1 case.
            p = s.Prog(wasm.ACall);
            p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.WasmDiv);
            break;
        }
        s.Prog(wasm.AI64DivS);
    else if (v.Op == ssa.OpWasmI64TruncSatF32S || v.Op == ssa.OpWasmI64TruncSatF64S) 
        getValue64(_addr_s, _addr_v.Args[0]);
        if (buildcfg.GOWASM.SatConv) {
            s.Prog(v.Op.Asm());
        }
        else
 {
            if (v.Op == ssa.OpWasmI64TruncSatF32S) {
                s.Prog(wasm.AF64PromoteF32);
            }
            p = s.Prog(wasm.ACall);
            p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.WasmTruncS);
        }
    else if (v.Op == ssa.OpWasmI64TruncSatF32U || v.Op == ssa.OpWasmI64TruncSatF64U) 
        getValue64(_addr_s, _addr_v.Args[0]);
        if (buildcfg.GOWASM.SatConv) {
            s.Prog(v.Op.Asm());
        }
        else
 {
            if (v.Op == ssa.OpWasmI64TruncSatF32U) {
                s.Prog(wasm.AF64PromoteF32);
            }
            p = s.Prog(wasm.ACall);
            p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:ir.Syms.WasmTruncU);
        }
    else if (v.Op == ssa.OpWasmF32DemoteF64) 
        getValue64(_addr_s, _addr_v.Args[0]);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpWasmF64PromoteF32) 
        getValue64(_addr_s, _addr_v.Args[0]);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpWasmF32ConvertI64S || v.Op == ssa.OpWasmF32ConvertI64U || v.Op == ssa.OpWasmF64ConvertI64S || v.Op == ssa.OpWasmF64ConvertI64U || v.Op == ssa.OpWasmI64Extend8S || v.Op == ssa.OpWasmI64Extend16S || v.Op == ssa.OpWasmI64Extend32S || v.Op == ssa.OpWasmF32Neg || v.Op == ssa.OpWasmF32Sqrt || v.Op == ssa.OpWasmF32Trunc || v.Op == ssa.OpWasmF32Ceil || v.Op == ssa.OpWasmF32Floor || v.Op == ssa.OpWasmF32Nearest || v.Op == ssa.OpWasmF32Abs || v.Op == ssa.OpWasmF64Neg || v.Op == ssa.OpWasmF64Sqrt || v.Op == ssa.OpWasmF64Trunc || v.Op == ssa.OpWasmF64Ceil || v.Op == ssa.OpWasmF64Floor || v.Op == ssa.OpWasmF64Nearest || v.Op == ssa.OpWasmF64Abs || v.Op == ssa.OpWasmI64Ctz || v.Op == ssa.OpWasmI64Clz || v.Op == ssa.OpWasmI64Popcnt) 
        getValue64(_addr_s, _addr_v.Args[0]);
        s.Prog(v.Op.Asm());
    else if (v.Op == ssa.OpLoadReg) 
        p = s.Prog(loadOp(_addr_v.Type));
        ssagen.AddrAuto(_addr_p.From, v.Args[0]);
    else if (v.Op == ssa.OpCopy) 
        getValue64(_addr_s, _addr_v.Args[0]);
    else 
        v.Fatalf("unexpected op: %s", v.Op);
    });

private static bool isCmp(ptr<ssa.Value> _addr_v) {
    ref ssa.Value v = ref _addr_v.val;


    if (v.Op == ssa.OpWasmI64Eqz || v.Op == ssa.OpWasmI64Eq || v.Op == ssa.OpWasmI64Ne || v.Op == ssa.OpWasmI64LtS || v.Op == ssa.OpWasmI64LtU || v.Op == ssa.OpWasmI64GtS || v.Op == ssa.OpWasmI64GtU || v.Op == ssa.OpWasmI64LeS || v.Op == ssa.OpWasmI64LeU || v.Op == ssa.OpWasmI64GeS || v.Op == ssa.OpWasmI64GeU || v.Op == ssa.OpWasmF32Eq || v.Op == ssa.OpWasmF32Ne || v.Op == ssa.OpWasmF32Lt || v.Op == ssa.OpWasmF32Gt || v.Op == ssa.OpWasmF32Le || v.Op == ssa.OpWasmF32Ge || v.Op == ssa.OpWasmF64Eq || v.Op == ssa.OpWasmF64Ne || v.Op == ssa.OpWasmF64Lt || v.Op == ssa.OpWasmF64Gt || v.Op == ssa.OpWasmF64Le || v.Op == ssa.OpWasmF64Ge) 
        return true;
    else 
        return false;
    }

private static void getValue32(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;

    if (v.OnWasmStack) {
        s.OnWasmStackSkipped--;
        ssaGenValueOnStack(_addr_s, _addr_v, false);
        if (!isCmp(_addr_v)) {
            s.Prog(wasm.AI32WrapI64);
        }
        return ;
    }
    var reg = v.Reg();
    getReg(_addr_s, reg);
    if (reg != wasm.REG_SP) {
        s.Prog(wasm.AI32WrapI64);
    }
}

private static void getValue64(ptr<ssagen.State> _addr_s, ptr<ssa.Value> _addr_v) {
    ref ssagen.State s = ref _addr_s.val;
    ref ssa.Value v = ref _addr_v.val;

    if (v.OnWasmStack) {
        s.OnWasmStackSkipped--;
        ssaGenValueOnStack(_addr_s, _addr_v, true);
        return ;
    }
    var reg = v.Reg();
    getReg(_addr_s, reg);
    if (reg == wasm.REG_SP) {
        s.Prog(wasm.AI64ExtendI32U);
    }
}

private static void i32Const(ptr<ssagen.State> _addr_s, int val) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.AI32Const);
    p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(val));
}

private static void i64Const(ptr<ssagen.State> _addr_s, long val) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.AI64Const);
    p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:val);
}

private static void f32Const(ptr<ssagen.State> _addr_s, double val) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.AF32Const);
    p.From = new obj.Addr(Type:obj.TYPE_FCONST,Val:val);
}

private static void f64Const(ptr<ssagen.State> _addr_s, double val) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.AF64Const);
    p.From = new obj.Addr(Type:obj.TYPE_FCONST,Val:val);
}

private static void getReg(ptr<ssagen.State> _addr_s, short reg) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.AGet);
    p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:reg);
}

private static void setReg(ptr<ssagen.State> _addr_s, short reg) {
    ref ssagen.State s = ref _addr_s.val;

    var p = s.Prog(wasm.ASet);
    p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:reg);
}

private static obj.As loadOp(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return wasm.AF32Load;
                break;
            case 8: 
                return wasm.AF64Load;
                break;
            default: 
                panic("bad load type");
                break;
        }
    }
    switch (t.Size()) {
        case 1: 
            if (t.IsSigned()) {
                return wasm.AI64Load8S;
            }
            return wasm.AI64Load8U;
            break;
        case 2: 
            if (t.IsSigned()) {
                return wasm.AI64Load16S;
            }
            return wasm.AI64Load16U;
            break;
        case 4: 
            if (t.IsSigned()) {
                return wasm.AI64Load32S;
            }
            return wasm.AI64Load32U;
            break;
        case 8: 
            return wasm.AI64Load;
            break;
        default: 
            panic("bad load type");
            break;
    }
});

private static obj.As storeOp(ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsFloat()) {
        switch (t.Size()) {
            case 4: 
                return wasm.AF32Store;
                break;
            case 8: 
                return wasm.AF64Store;
                break;
            default: 
                panic("bad store type");
                break;
        }
    }
    switch (t.Size()) {
        case 1: 
            return wasm.AI64Store8;
            break;
        case 2: 
            return wasm.AI64Store16;
            break;
        case 4: 
            return wasm.AI64Store32;
            break;
        case 8: 
            return wasm.AI64Store;
            break;
        default: 
            panic("bad store type");
            break;
    }
});

} // end wasm_package
