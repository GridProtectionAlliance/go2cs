// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wasm -- go2cs converted at 2020 October 08 04:09:39 UTC
// import "cmd/compile/internal/wasm" ==> using wasm = go.cmd.compile.@internal.wasm_package
// Original source: C:\Go\src\cmd\compile\internal\wasm\ssa.go
using gc = go.cmd.compile.@internal.gc_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using wasm = go.cmd.@internal.obj.wasm_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class wasm_package
    {
        public static void Init(ptr<gc.Arch> _addr_arch)
        {
            ref gc.Arch arch = ref _addr_arch.val;

            arch.LinkArch = _addr_wasm.Linkwasm;
            arch.REGSP = wasm.REG_SP;
            arch.MAXWIDTH = 1L << (int)(50L);

            arch.ZeroRange = zeroRange;
            arch.Ginsnop = ginsnop;
            arch.Ginsnopdefer = ginsnop;

            arch.SSAMarkMoves = ssaMarkMoves;
            arch.SSAGenValue = ssaGenValue;
            arch.SSAGenBlock = ssaGenBlock;
        }

        private static ptr<obj.Prog> zeroRange(ptr<gc.Progs> _addr_pp, ptr<obj.Prog> _addr_p, long off, long cnt, ptr<uint> _addr_state)
        {
            ref gc.Progs pp = ref _addr_pp.val;
            ref obj.Prog p = ref _addr_p.val;
            ref uint state = ref _addr_state.val;

            if (cnt == 0L)
            {
                return _addr_p!;
            }

            if (cnt % 8L != 0L)
            {
                gc.Fatalf("zerorange count not a multiple of widthptr %d", cnt);
            }

            {
                var i = int64(0L);

                while (i < cnt)
                {
                    p = pp.Appendpp(p, wasm.AGet, obj.TYPE_REG, wasm.REG_SP, 0L, 0L, 0L, 0L);
                    p = pp.Appendpp(p, wasm.AI64Const, obj.TYPE_CONST, 0L, 0L, 0L, 0L, 0L);
                    p = pp.Appendpp(p, wasm.AI64Store, 0L, 0L, 0L, obj.TYPE_CONST, 0L, off + i);
                    i += 8L;
                }

            }

            return _addr_p!;

        }

        private static ptr<obj.Prog> ginsnop(ptr<gc.Progs> _addr_pp)
        {
            ref gc.Progs pp = ref _addr_pp.val;

            return _addr_pp.Prog(wasm.ANop)!;
        }

        private static void ssaMarkMoves(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Block> _addr_b)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;

        }

        private static void ssaGenBlock(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Block> _addr_b, ptr<ssa.Block> _addr_next) => func((_, panic, __) =>
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Block b = ref _addr_b.val;
            ref ssa.Block next = ref _addr_next.val;


            if (b.Kind == ssa.BlockPlain) 
                if (next != b.Succs[0L].Block())
                {
                    s.Br(obj.AJMP, b.Succs[0L].Block());
                }

            else if (b.Kind == ssa.BlockIf) 

                if (next == b.Succs[0L].Block()) 
                    // if false, jump to b.Succs[1]
                    getValue32(_addr_s, _addr_b.Controls[0L]);
                    s.Prog(wasm.AI32Eqz);
                    s.Prog(wasm.AIf);
                    s.Br(obj.AJMP, b.Succs[1L].Block());
                    s.Prog(wasm.AEnd);
                else if (next == b.Succs[1L].Block()) 
                    // if true, jump to b.Succs[0]
                    getValue32(_addr_s, _addr_b.Controls[0L]);
                    s.Prog(wasm.AIf);
                    s.Br(obj.AJMP, b.Succs[0L].Block());
                    s.Prog(wasm.AEnd);
                else 
                    // if true, jump to b.Succs[0], else jump to b.Succs[1]
                    getValue32(_addr_s, _addr_b.Controls[0L]);
                    s.Prog(wasm.AIf);
                    s.Br(obj.AJMP, b.Succs[0L].Block());
                    s.Prog(wasm.AEnd);
                    s.Br(obj.AJMP, b.Succs[1L].Block());
                            else if (b.Kind == ssa.BlockRet) 
                s.Prog(obj.ARET);
            else if (b.Kind == ssa.BlockRetJmp) 
                var p = s.Prog(obj.ARET);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = b.Aux._<ptr<obj.LSym>>();
            else if (b.Kind == ssa.BlockExit)             else if (b.Kind == ssa.BlockDefer) 
                p = s.Prog(wasm.AGet);
                p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:wasm.REG_RET0);
                s.Prog(wasm.AI64Eqz);
                s.Prog(wasm.AI32Eqz);
                s.Prog(wasm.AIf);
                s.Br(obj.AJMP, b.Succs[1L].Block());
                s.Prog(wasm.AEnd);
                if (next != b.Succs[0L].Block())
                {
                    s.Br(obj.AJMP, b.Succs[0L].Block());
                }

            else 
                panic("unexpected block");
            // Entry point for the next block. Used by the JMP in goToBlock.
            s.Prog(wasm.ARESUMEPOINT);

            if (s.OnWasmStackSkipped != 0L)
            {
                panic("wasm: bad stack");
            }

        });

        private static void ssaGenValue(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v) => func((_, panic, __) =>
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpWasmLoweredStaticCall || v.Op == ssa.OpWasmLoweredClosureCall || v.Op == ssa.OpWasmLoweredInterCall) 
                s.PrepareCall(v);
                if (v.Aux == gc.Deferreturn)
                { 
                    // add a resume point before call to deferreturn so it can be called again via jmpdefer
                    s.Prog(wasm.ARESUMEPOINT);

                }

                if (v.Op == ssa.OpWasmLoweredClosureCall)
                {
                    getValue64(_addr_s, _addr_v.Args[1L]);
                    setReg(_addr_s, wasm.REG_CTXT);
                }

                {
                    ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();

                    if (ok)
                    {
                        var p = s.Prog(obj.ACALL);
                        p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:sym);
                        p.Pos = v.Pos;
                    }
                    else
                    {
                        getValue64(_addr_s, _addr_v.Args[0L]);
                        p = s.Prog(obj.ACALL);
                        p.To = new obj.Addr(Type:obj.TYPE_NONE);
                        p.Pos = v.Pos;
                    }

                }


            else if (v.Op == ssa.OpWasmLoweredMove) 
                getValue32(_addr_s, _addr_v.Args[0L]);
                getValue32(_addr_s, _addr_v.Args[1L]);
                i32Const(_addr_s, int32(v.AuxInt));
                p = s.Prog(wasm.ACall);
                p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.WasmMove);
            else if (v.Op == ssa.OpWasmLoweredZero) 
                getValue32(_addr_s, _addr_v.Args[0L]);
                i32Const(_addr_s, int32(v.AuxInt));
                p = s.Prog(wasm.ACall);
                p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.WasmZero);
            else if (v.Op == ssa.OpWasmLoweredNilCheck) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                s.Prog(wasm.AI64Eqz);
                s.Prog(wasm.AIf);
                p = s.Prog(wasm.ACALLNORESUME);
                p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.SigPanic);
                s.Prog(wasm.AEnd);
                if (logopt.Enabled())
                {
                    logopt.LogOpt(v.Pos, "nilcheck", "genssa", v.Block.Func.Name);
                }

                if (gc.Debug_checknil != 0L && v.Pos.Line() > 1L)
                { // v.Pos.Line()==1 in generated wrappers
                    gc.Warnl(v.Pos, "generated nil check");

                }

            else if (v.Op == ssa.OpWasmLoweredWB) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                p = s.Prog(wasm.ACALLNORESUME); // TODO(neelance): If possible, turn this into a simple wasm.ACall).
                p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:v.Aux.(*obj.LSym));
            else if (v.Op == ssa.OpWasmI64Store8 || v.Op == ssa.OpWasmI64Store16 || v.Op == ssa.OpWasmI64Store32 || v.Op == ssa.OpWasmI64Store || v.Op == ssa.OpWasmF32Store || v.Op == ssa.OpWasmF64Store) 
                getValue32(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                p = s.Prog(v.Op.Asm());
                p.To = new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt);
            else if (v.Op == ssa.OpStoreReg) 
                getReg(_addr_s, wasm.REG_SP);
                getValue64(_addr_s, _addr_v.Args[0L]);
                p = s.Prog(storeOp(_addr_v.Type));
                gc.AddrAuto(_addr_p.To, v);
            else 
                if (v.Type.IsMemory())
                {
                    return ;
                }

                if (v.OnWasmStack)
                {
                    s.OnWasmStackSkipped++; 
                    // If a Value is marked OnWasmStack, we don't generate the value and store it to a register now.
                    // Instead, we delay the generation to when the value is used and then directly generate it on the WebAssembly stack.
                    return ;

                }

                ssaGenValueOnStack(_addr_s, _addr_v, true);
                if (s.OnWasmStackSkipped != 0L)
                {
                    panic("wasm: bad stack");
                }

                setReg(_addr_s, v.Reg());
            
        });

        private static void ssaGenValueOnStack(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v, bool extend) => func((_, panic, __) =>
        {
            ref gc.SSAGenState s = ref _addr_s.val;
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
                p = s.Prog(wasm.AGet);
                p.From.Type = obj.TYPE_ADDR;
                switch (v.Aux.type())
                {
                    case ptr<obj.LSym> _:
                        gc.AddAux(_addr_p.From, v);
                        break;
                    case ptr<gc.Node> _:
                        p.From.Reg = v.Args[0L].Reg();
                        gc.AddAux(_addr_p.From, v);
                        break;
                    default:
                    {
                        panic("wasm: bad LoweredAddr");
                        break;
                    }

                }
            else if (v.Op == ssa.OpWasmLoweredConvert) 
                getValue64(_addr_s, _addr_v.Args[0L]);
            else if (v.Op == ssa.OpWasmSelect) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                getValue32(_addr_s, _addr_v.Args[2L]);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpWasmI64AddConst) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                i64Const(_addr_s, v.AuxInt);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpWasmI64Const) 
                i64Const(_addr_s, v.AuxInt);
            else if (v.Op == ssa.OpWasmF32Const) 
                f32Const(_addr_s, v.AuxFloat());
            else if (v.Op == ssa.OpWasmF64Const) 
                f64Const(_addr_s, v.AuxFloat());
            else if (v.Op == ssa.OpWasmI64Load8U || v.Op == ssa.OpWasmI64Load8S || v.Op == ssa.OpWasmI64Load16U || v.Op == ssa.OpWasmI64Load16S || v.Op == ssa.OpWasmI64Load32U || v.Op == ssa.OpWasmI64Load32S || v.Op == ssa.OpWasmI64Load || v.Op == ssa.OpWasmF32Load || v.Op == ssa.OpWasmF64Load) 
                getValue32(_addr_s, _addr_v.Args[0L]);
                p = s.Prog(v.Op.Asm());
                p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:v.AuxInt);
            else if (v.Op == ssa.OpWasmI64Eqz) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                s.Prog(v.Op.Asm());
                if (extend)
                {
                    s.Prog(wasm.AI64ExtendI32U);
                }

            else if (v.Op == ssa.OpWasmI64Eq || v.Op == ssa.OpWasmI64Ne || v.Op == ssa.OpWasmI64LtS || v.Op == ssa.OpWasmI64LtU || v.Op == ssa.OpWasmI64GtS || v.Op == ssa.OpWasmI64GtU || v.Op == ssa.OpWasmI64LeS || v.Op == ssa.OpWasmI64LeU || v.Op == ssa.OpWasmI64GeS || v.Op == ssa.OpWasmI64GeU || v.Op == ssa.OpWasmF32Eq || v.Op == ssa.OpWasmF32Ne || v.Op == ssa.OpWasmF32Lt || v.Op == ssa.OpWasmF32Gt || v.Op == ssa.OpWasmF32Le || v.Op == ssa.OpWasmF32Ge || v.Op == ssa.OpWasmF64Eq || v.Op == ssa.OpWasmF64Ne || v.Op == ssa.OpWasmF64Lt || v.Op == ssa.OpWasmF64Gt || v.Op == ssa.OpWasmF64Le || v.Op == ssa.OpWasmF64Ge) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                s.Prog(v.Op.Asm());
                if (extend)
                {
                    s.Prog(wasm.AI64ExtendI32U);
                }

            else if (v.Op == ssa.OpWasmI64Add || v.Op == ssa.OpWasmI64Sub || v.Op == ssa.OpWasmI64Mul || v.Op == ssa.OpWasmI64DivU || v.Op == ssa.OpWasmI64RemS || v.Op == ssa.OpWasmI64RemU || v.Op == ssa.OpWasmI64And || v.Op == ssa.OpWasmI64Or || v.Op == ssa.OpWasmI64Xor || v.Op == ssa.OpWasmI64Shl || v.Op == ssa.OpWasmI64ShrS || v.Op == ssa.OpWasmI64ShrU || v.Op == ssa.OpWasmI64Rotl || v.Op == ssa.OpWasmF32Add || v.Op == ssa.OpWasmF32Sub || v.Op == ssa.OpWasmF32Mul || v.Op == ssa.OpWasmF32Div || v.Op == ssa.OpWasmF32Copysign || v.Op == ssa.OpWasmF64Add || v.Op == ssa.OpWasmF64Sub || v.Op == ssa.OpWasmF64Mul || v.Op == ssa.OpWasmF64Div || v.Op == ssa.OpWasmF64Copysign) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpWasmI32Rotl) 
                getValue32(_addr_s, _addr_v.Args[0L]);
                getValue32(_addr_s, _addr_v.Args[1L]);
                s.Prog(wasm.AI32Rotl);
                s.Prog(wasm.AI64ExtendI32U);
            else if (v.Op == ssa.OpWasmI64DivS) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                getValue64(_addr_s, _addr_v.Args[1L]);
                if (v.Type.Size() == 8L)
                { 
                    // Division of int64 needs helper function wasmDiv to handle the MinInt64 / -1 case.
                    p = s.Prog(wasm.ACall);
                    p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.WasmDiv);
                    break;

                }

                s.Prog(wasm.AI64DivS);
            else if (v.Op == ssa.OpWasmI64TruncSatF32S || v.Op == ssa.OpWasmI64TruncSatF64S) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                if (objabi.GOWASM.SatConv)
                {
                    s.Prog(v.Op.Asm());
                }
                else
                {
                    if (v.Op == ssa.OpWasmI64TruncSatF32S)
                    {
                        s.Prog(wasm.AF64PromoteF32);
                    }

                    p = s.Prog(wasm.ACall);
                    p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.WasmTruncS);

                }

            else if (v.Op == ssa.OpWasmI64TruncSatF32U || v.Op == ssa.OpWasmI64TruncSatF64U) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                if (objabi.GOWASM.SatConv)
                {
                    s.Prog(v.Op.Asm());
                }
                else
                {
                    if (v.Op == ssa.OpWasmI64TruncSatF32U)
                    {
                        s.Prog(wasm.AF64PromoteF32);
                    }

                    p = s.Prog(wasm.ACall);
                    p.To = new obj.Addr(Type:obj.TYPE_MEM,Name:obj.NAME_EXTERN,Sym:gc.WasmTruncU);

                }

            else if (v.Op == ssa.OpWasmF32DemoteF64) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpWasmF64PromoteF32) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpWasmF32ConvertI64S || v.Op == ssa.OpWasmF32ConvertI64U || v.Op == ssa.OpWasmF64ConvertI64S || v.Op == ssa.OpWasmF64ConvertI64U || v.Op == ssa.OpWasmI64Extend8S || v.Op == ssa.OpWasmI64Extend16S || v.Op == ssa.OpWasmI64Extend32S || v.Op == ssa.OpWasmF32Neg || v.Op == ssa.OpWasmF32Sqrt || v.Op == ssa.OpWasmF32Trunc || v.Op == ssa.OpWasmF32Ceil || v.Op == ssa.OpWasmF32Floor || v.Op == ssa.OpWasmF32Nearest || v.Op == ssa.OpWasmF32Abs || v.Op == ssa.OpWasmF64Neg || v.Op == ssa.OpWasmF64Sqrt || v.Op == ssa.OpWasmF64Trunc || v.Op == ssa.OpWasmF64Ceil || v.Op == ssa.OpWasmF64Floor || v.Op == ssa.OpWasmF64Nearest || v.Op == ssa.OpWasmF64Abs || v.Op == ssa.OpWasmI64Ctz || v.Op == ssa.OpWasmI64Clz || v.Op == ssa.OpWasmI64Popcnt) 
                getValue64(_addr_s, _addr_v.Args[0L]);
                s.Prog(v.Op.Asm());
            else if (v.Op == ssa.OpLoadReg) 
                p = s.Prog(loadOp(_addr_v.Type));
                gc.AddrAuto(_addr_p.From, v.Args[0L]);
            else if (v.Op == ssa.OpCopy) 
                getValue64(_addr_s, _addr_v.Args[0L]);
            else 
                v.Fatalf("unexpected op: %s", v.Op);
            
        });

        private static bool isCmp(ptr<ssa.Value> _addr_v)
        {
            ref ssa.Value v = ref _addr_v.val;


            if (v.Op == ssa.OpWasmI64Eqz || v.Op == ssa.OpWasmI64Eq || v.Op == ssa.OpWasmI64Ne || v.Op == ssa.OpWasmI64LtS || v.Op == ssa.OpWasmI64LtU || v.Op == ssa.OpWasmI64GtS || v.Op == ssa.OpWasmI64GtU || v.Op == ssa.OpWasmI64LeS || v.Op == ssa.OpWasmI64LeU || v.Op == ssa.OpWasmI64GeS || v.Op == ssa.OpWasmI64GeU || v.Op == ssa.OpWasmF32Eq || v.Op == ssa.OpWasmF32Ne || v.Op == ssa.OpWasmF32Lt || v.Op == ssa.OpWasmF32Gt || v.Op == ssa.OpWasmF32Le || v.Op == ssa.OpWasmF32Ge || v.Op == ssa.OpWasmF64Eq || v.Op == ssa.OpWasmF64Ne || v.Op == ssa.OpWasmF64Lt || v.Op == ssa.OpWasmF64Gt || v.Op == ssa.OpWasmF64Le || v.Op == ssa.OpWasmF64Ge) 
                return true;
            else 
                return false;
            
        }

        private static void getValue32(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;

            if (v.OnWasmStack)
            {
                s.OnWasmStackSkipped--;
                ssaGenValueOnStack(_addr_s, _addr_v, false);
                if (!isCmp(_addr_v))
                {
                    s.Prog(wasm.AI32WrapI64);
                }

                return ;

            }

            var reg = v.Reg();
            getReg(_addr_s, reg);
            if (reg != wasm.REG_SP)
            {
                s.Prog(wasm.AI32WrapI64);
            }

        }

        private static void getValue64(ptr<gc.SSAGenState> _addr_s, ptr<ssa.Value> _addr_v)
        {
            ref gc.SSAGenState s = ref _addr_s.val;
            ref ssa.Value v = ref _addr_v.val;

            if (v.OnWasmStack)
            {
                s.OnWasmStackSkipped--;
                ssaGenValueOnStack(_addr_s, _addr_v, true);
                return ;
            }

            var reg = v.Reg();
            getReg(_addr_s, reg);
            if (reg == wasm.REG_SP)
            {
                s.Prog(wasm.AI64ExtendI32U);
            }

        }

        private static void i32Const(ptr<gc.SSAGenState> _addr_s, int val)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.AI32Const);
            p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:int64(val));
        }

        private static void i64Const(ptr<gc.SSAGenState> _addr_s, long val)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.AI64Const);
            p.From = new obj.Addr(Type:obj.TYPE_CONST,Offset:val);
        }

        private static void f32Const(ptr<gc.SSAGenState> _addr_s, double val)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.AF32Const);
            p.From = new obj.Addr(Type:obj.TYPE_FCONST,Val:val);
        }

        private static void f64Const(ptr<gc.SSAGenState> _addr_s, double val)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.AF64Const);
            p.From = new obj.Addr(Type:obj.TYPE_FCONST,Val:val);
        }

        private static void getReg(ptr<gc.SSAGenState> _addr_s, short reg)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.AGet);
            p.From = new obj.Addr(Type:obj.TYPE_REG,Reg:reg);
        }

        private static void setReg(ptr<gc.SSAGenState> _addr_s, short reg)
        {
            ref gc.SSAGenState s = ref _addr_s.val;

            var p = s.Prog(wasm.ASet);
            p.To = new obj.Addr(Type:obj.TYPE_REG,Reg:reg);
        }

        private static obj.As loadOp(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return wasm.AF32Load;
                        break;
                    case 8L: 
                        return wasm.AF64Load;
                        break;
                    default: 
                        panic("bad load type");
                        break;
                }

            }

            switch (t.Size())
            {
                case 1L: 
                    if (t.IsSigned())
                    {
                        return wasm.AI64Load8S;
                    }

                    return wasm.AI64Load8U;
                    break;
                case 2L: 
                    if (t.IsSigned())
                    {
                        return wasm.AI64Load16S;
                    }

                    return wasm.AI64Load16U;
                    break;
                case 4L: 
                    if (t.IsSigned())
                    {
                        return wasm.AI64Load32S;
                    }

                    return wasm.AI64Load32U;
                    break;
                case 8L: 
                    return wasm.AI64Load;
                    break;
                default: 
                    panic("bad load type");
                    break;
            }

        });

        private static obj.As storeOp(ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            if (t.IsFloat())
            {
                switch (t.Size())
                {
                    case 4L: 
                        return wasm.AF32Store;
                        break;
                    case 8L: 
                        return wasm.AF64Store;
                        break;
                    default: 
                        panic("bad store type");
                        break;
                }

            }

            switch (t.Size())
            {
                case 1L: 
                    return wasm.AI64Store8;
                    break;
                case 2L: 
                    return wasm.AI64Store16;
                    break;
                case 4L: 
                    return wasm.AI64Store32;
                    break;
                case 8L: 
                    return wasm.AI64Store;
                    break;
                default: 
                    panic("bad store type");
                    break;
            }

        });
    }
}}}}
