// Code generated from gen/dec64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 23:00:32 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewritedec64.go
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValuedec64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAdd64) 
        return rewriteValuedec64_OpAdd64(_addr_v);
    else if (v.Op == OpAnd64) 
        return rewriteValuedec64_OpAnd64(_addr_v);
    else if (v.Op == OpArg) 
        return rewriteValuedec64_OpArg(_addr_v);
    else if (v.Op == OpBitLen64) 
        return rewriteValuedec64_OpBitLen64(_addr_v);
    else if (v.Op == OpBswap64) 
        return rewriteValuedec64_OpBswap64(_addr_v);
    else if (v.Op == OpCom64) 
        return rewriteValuedec64_OpCom64(_addr_v);
    else if (v.Op == OpConst64) 
        return rewriteValuedec64_OpConst64(_addr_v);
    else if (v.Op == OpCtz64) 
        return rewriteValuedec64_OpCtz64(_addr_v);
    else if (v.Op == OpCtz64NonZero) 
        v.Op = OpCtz64;
        return true;
    else if (v.Op == OpEq64) 
        return rewriteValuedec64_OpEq64(_addr_v);
    else if (v.Op == OpInt64Hi) 
        return rewriteValuedec64_OpInt64Hi(_addr_v);
    else if (v.Op == OpInt64Lo) 
        return rewriteValuedec64_OpInt64Lo(_addr_v);
    else if (v.Op == OpLeq64) 
        return rewriteValuedec64_OpLeq64(_addr_v);
    else if (v.Op == OpLeq64U) 
        return rewriteValuedec64_OpLeq64U(_addr_v);
    else if (v.Op == OpLess64) 
        return rewriteValuedec64_OpLess64(_addr_v);
    else if (v.Op == OpLess64U) 
        return rewriteValuedec64_OpLess64U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValuedec64_OpLoad(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValuedec64_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValuedec64_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh64x16) 
        return rewriteValuedec64_OpLsh64x16(_addr_v);
    else if (v.Op == OpLsh64x32) 
        return rewriteValuedec64_OpLsh64x32(_addr_v);
    else if (v.Op == OpLsh64x64) 
        return rewriteValuedec64_OpLsh64x64(_addr_v);
    else if (v.Op == OpLsh64x8) 
        return rewriteValuedec64_OpLsh64x8(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValuedec64_OpLsh8x64(_addr_v);
    else if (v.Op == OpMul64) 
        return rewriteValuedec64_OpMul64(_addr_v);
    else if (v.Op == OpNeg64) 
        return rewriteValuedec64_OpNeg64(_addr_v);
    else if (v.Op == OpNeq64) 
        return rewriteValuedec64_OpNeq64(_addr_v);
    else if (v.Op == OpOr32) 
        return rewriteValuedec64_OpOr32(_addr_v);
    else if (v.Op == OpOr64) 
        return rewriteValuedec64_OpOr64(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValuedec64_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValuedec64_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValuedec64_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValuedec64_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh64Ux16) 
        return rewriteValuedec64_OpRsh64Ux16(_addr_v);
    else if (v.Op == OpRsh64Ux32) 
        return rewriteValuedec64_OpRsh64Ux32(_addr_v);
    else if (v.Op == OpRsh64Ux64) 
        return rewriteValuedec64_OpRsh64Ux64(_addr_v);
    else if (v.Op == OpRsh64Ux8) 
        return rewriteValuedec64_OpRsh64Ux8(_addr_v);
    else if (v.Op == OpRsh64x16) 
        return rewriteValuedec64_OpRsh64x16(_addr_v);
    else if (v.Op == OpRsh64x32) 
        return rewriteValuedec64_OpRsh64x32(_addr_v);
    else if (v.Op == OpRsh64x64) 
        return rewriteValuedec64_OpRsh64x64(_addr_v);
    else if (v.Op == OpRsh64x8) 
        return rewriteValuedec64_OpRsh64x8(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValuedec64_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValuedec64_OpRsh8x64(_addr_v);
    else if (v.Op == OpSignExt16to64) 
        return rewriteValuedec64_OpSignExt16to64(_addr_v);
    else if (v.Op == OpSignExt32to64) 
        return rewriteValuedec64_OpSignExt32to64(_addr_v);
    else if (v.Op == OpSignExt8to64) 
        return rewriteValuedec64_OpSignExt8to64(_addr_v);
    else if (v.Op == OpStore) 
        return rewriteValuedec64_OpStore(_addr_v);
    else if (v.Op == OpSub64) 
        return rewriteValuedec64_OpSub64(_addr_v);
    else if (v.Op == OpTrunc64to16) 
        return rewriteValuedec64_OpTrunc64to16(_addr_v);
    else if (v.Op == OpTrunc64to32) 
        return rewriteValuedec64_OpTrunc64to32(_addr_v);
    else if (v.Op == OpTrunc64to8) 
        return rewriteValuedec64_OpTrunc64to8(_addr_v);
    else if (v.Op == OpXor64) 
        return rewriteValuedec64_OpXor64(_addr_v);
    else if (v.Op == OpZeroExt16to64) 
        return rewriteValuedec64_OpZeroExt16to64(_addr_v);
    else if (v.Op == OpZeroExt32to64) 
        return rewriteValuedec64_OpZeroExt32to64(_addr_v);
    else if (v.Op == OpZeroExt8to64) 
        return rewriteValuedec64_OpZeroExt8to64(_addr_v);
        return false;

}
private static bool rewriteValuedec64_OpAdd64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Add64 x y)
    // result: (Int64Make (Add32withcarry <typ.Int32> (Int64Hi x) (Int64Hi y) (Select1 <types.TypeFlags> (Add32carry (Int64Lo x) (Int64Lo y)))) (Select0 <typ.UInt32> (Add32carry (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpAdd32withcarry, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpAdd32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(y);
        v4.AddArg2(v5, v6);
        v3.AddArg(v4);
        v0.AddArg3(v1, v2, v3);
        var v7 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
        v7.AddArg(v4);
        v.AddArg2(v0, v7);
        return true;
    }

}
private static bool rewriteValuedec64_OpAnd64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (And64 x y)
    // result: (Int64Make (And32 <typ.UInt32> (Int64Hi x) (Int64Hi y)) (And32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(y);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpArg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Arg {n} [off])
    // cond: is64BitInt(v.Type) && !config.BigEndian && v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin")
    // result: (Int64Make (Arg <typ.Int32> {n} [off+4]) (Arg <typ.UInt32> {n} [off]))
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var n = auxToSym(v.Aux);
        if (!(is64BitInt(v.Type) && !config.BigEndian && v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin"))) {
            break;
        }
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpArg, typ.Int32);
        v0.AuxInt = int32ToAuxInt(off + 4);
        v0.Aux = symToAux(n);
        var v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(off);
        v1.Aux = symToAux(n);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Arg {n} [off])
    // cond: is64BitInt(v.Type) && !config.BigEndian && !v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin")
    // result: (Int64Make (Arg <typ.UInt32> {n} [off+4]) (Arg <typ.UInt32> {n} [off]))
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        n = auxToSym(v.Aux);
        if (!(is64BitInt(v.Type) && !config.BigEndian && !v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin"))) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(off + 4);
        v0.Aux = symToAux(n);
        v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(off);
        v1.Aux = symToAux(n);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Arg {n} [off])
    // cond: is64BitInt(v.Type) && config.BigEndian && v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin")
    // result: (Int64Make (Arg <typ.Int32> {n} [off]) (Arg <typ.UInt32> {n} [off+4]))
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        n = auxToSym(v.Aux);
        if (!(is64BitInt(v.Type) && config.BigEndian && v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin"))) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpArg, typ.Int32);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(n);
        v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(off + 4);
        v1.Aux = symToAux(n);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Arg {n} [off])
    // cond: is64BitInt(v.Type) && config.BigEndian && !v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin")
    // result: (Int64Make (Arg <typ.UInt32> {n} [off]) (Arg <typ.UInt32> {n} [off+4]))
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        n = auxToSym(v.Aux);
        if (!(is64BitInt(v.Type) && config.BigEndian && !v.Type.IsSigned() && !(b.Func.pass.name == "decompose builtin"))) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(n);
        v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(off + 4);
        v1.Aux = symToAux(n);
        v.AddArg2(v0, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpBitLen64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen64 x)
    // result: (Add32 <typ.Int> (BitLen32 <typ.Int> (Int64Hi x)) (BitLen32 <typ.Int> (Or32 <typ.UInt32> (Int64Lo x) (Zeromask (Int64Hi x)))))
    while (true) {
        var x = v_0;
        v.reset(OpAdd32);
        v.Type = typ.Int;
        var v0 = b.NewValue0(v.Pos, OpBitLen32, typ.Int);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        var v2 = b.NewValue0(v.Pos, OpBitLen32, typ.Int);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v5.AddArg(v1);
        v3.AddArg2(v4, v5);
        v2.AddArg(v3);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpBswap64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Bswap64 x)
    // result: (Int64Make (Bswap32 <typ.UInt32> (Int64Lo x)) (Bswap32 <typ.UInt32> (Int64Hi x)))
    while (true) {
        var x = v_0;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpBswap32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        var v2 = b.NewValue0(v.Pos, OpBswap32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v3.AddArg(x);
        v2.AddArg(v3);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpCom64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Com64 x)
    // result: (Int64Make (Com32 <typ.UInt32> (Int64Hi x)) (Com32 <typ.UInt32> (Int64Lo x)))
    while (true) {
        var x = v_0;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        var v2 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(x);
        v2.AddArg(v3);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpConst64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Const64 <t> [c])
    // cond: t.IsSigned()
    // result: (Int64Make (Const32 <typ.Int32> [int32(c>>32)]) (Const32 <typ.UInt32> [int32(c)]))
    while (true) {
        var t = v.Type;
        var c = auxIntToInt64(v.AuxInt);
        if (!(t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
        v0.AuxInt = int32ToAuxInt(int32(c >> 32));
        var v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Const64 <t> [c])
    // cond: !t.IsSigned()
    // result: (Int64Make (Const32 <typ.UInt32> [int32(c>>32)]) (Const32 <typ.UInt32> [int32(c)]))
    while (true) {
        t = v.Type;
        c = auxIntToInt64(v.AuxInt);
        if (!(!t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(c >> 32));
        v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(v0, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpCtz64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz64 x)
    // result: (Add32 <typ.UInt32> (Ctz32 <typ.UInt32> (Int64Lo x)) (And32 <typ.UInt32> (Com32 <typ.UInt32> (Zeromask (Int64Lo x))) (Ctz32 <typ.UInt32> (Int64Hi x))))
    while (true) {
        var x = v_0;
        v.reset(OpAdd32);
        v.Type = typ.UInt32;
        var v0 = b.NewValue0(v.Pos, OpCtz32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        var v2 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v4.AddArg(v1);
        v3.AddArg(v4);
        var v5 = b.NewValue0(v.Pos, OpCtz32, typ.UInt32);
        var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v6.AddArg(x);
        v5.AddArg(v6);
        v2.AddArg2(v3, v5);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpEq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq64 x y)
    // result: (AndB (Eq32 (Int64Hi x) (Int64Hi y)) (Eq32 (Int64Lo x) (Int64Lo y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpAndB);
        var v0 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(y);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpInt64Hi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Int64Hi (Int64Make hi _))
    // result: hi
    while (true) {
        if (v_0.Op != OpInt64Make) {
            break;
        }
        var hi = v_0.Args[0];
        v.copyOf(hi);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpInt64Lo(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Int64Lo (Int64Make _ lo))
    // result: lo
    while (true) {
        if (v_0.Op != OpInt64Make) {
            break;
        }
        var lo = v_0.Args[1];
        v.copyOf(lo);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpLeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq64 x y)
    // result: (OrB (Less32 (Int64Hi x) (Int64Hi y)) (AndB (Eq32 (Int64Hi x) (Int64Hi y)) (Leq32U (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpOrB);
        var v0 = b.NewValue0(v.Pos, OpLess32, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        v4.AddArg2(v1, v2);
        var v5 = b.NewValue0(v.Pos, OpLeq32U, typ.Bool);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(x);
        var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v7.AddArg(y);
        v5.AddArg2(v6, v7);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpLeq64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq64U x y)
    // result: (OrB (Less32U (Int64Hi x) (Int64Hi y)) (AndB (Eq32 (Int64Hi x) (Int64Hi y)) (Leq32U (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpOrB);
        var v0 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        v4.AddArg2(v1, v2);
        var v5 = b.NewValue0(v.Pos, OpLeq32U, typ.Bool);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(x);
        var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v7.AddArg(y);
        v5.AddArg2(v6, v7);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpLess64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less64 x y)
    // result: (OrB (Less32 (Int64Hi x) (Int64Hi y)) (AndB (Eq32 (Int64Hi x) (Int64Hi y)) (Less32U (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpOrB);
        var v0 = b.NewValue0(v.Pos, OpLess32, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        v4.AddArg2(v1, v2);
        var v5 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(x);
        var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v7.AddArg(y);
        v5.AddArg2(v6, v7);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpLess64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less64U x y)
    // result: (OrB (Less32U (Int64Hi x) (Int64Hi y)) (AndB (Eq32 (Int64Hi x) (Int64Hi y)) (Less32U (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpOrB);
        var v0 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
        v4.AddArg2(v1, v2);
        var v5 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(x);
        var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v7.AddArg(y);
        v5.AddArg2(v6, v7);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Load <t> ptr mem)
    // cond: is64BitInt(t) && !config.BigEndian && t.IsSigned()
    // result: (Int64Make (Load <typ.Int32> (OffPtr <typ.Int32Ptr> [4] ptr) mem) (Load <typ.UInt32> ptr mem))
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(is64BitInt(t) && !config.BigEndian && t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpLoad, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpOffPtr, typ.Int32Ptr);
        v1.AuxInt = int64ToAuxInt(4);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        var v2 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v2.AddArg2(ptr, mem);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitInt(t) && !config.BigEndian && !t.IsSigned()
    // result: (Int64Make (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem) (Load <typ.UInt32> ptr mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitInt(t) && !config.BigEndian && !t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
        v1.AuxInt = int64ToAuxInt(4);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        v2 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v2.AddArg2(ptr, mem);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitInt(t) && config.BigEndian && t.IsSigned()
    // result: (Int64Make (Load <typ.Int32> ptr mem) (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitInt(t) && config.BigEndian && t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.Int32);
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
        v2.AuxInt = int64ToAuxInt(4);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitInt(t) && config.BigEndian && !t.IsSigned()
    // result: (Int64Make (Load <typ.UInt32> ptr mem) (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitInt(t) && config.BigEndian && !t.IsSigned())) {
            break;
        }
        v.reset(OpInt64Make);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
        v2.AuxInt = int64ToAuxInt(4);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Lsh16x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Lsh16x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpLsh16x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Lsh16x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Lsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpLsh16x32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh16x64 x y)
    // result: (Lsh16x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpLsh16x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Lsh32x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Lsh32x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpLsh32x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Lsh32x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Lsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpLsh32x32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh32x64 x y)
    // result: (Lsh32x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpLsh32x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x16 x s)
    // result: (Int64Make (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Lsh32x16 <typ.UInt32> (Int64Hi x) s) (Rsh32Ux16 <typ.UInt32> (Int64Lo x) (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s))) (Lsh32x16 <typ.UInt32> (Int64Lo x) (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32])))) (Lsh32x16 <typ.UInt32> (Int64Lo x) s))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v3.AddArg(x);
        v2.AddArg2(v3, s);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        var v6 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        var v7 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
        v7.AuxInt = int16ToAuxInt(32);
        v6.AddArg2(v7, s);
        v4.AddArg2(v5, v6);
        v1.AddArg2(v2, v4);
        var v8 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
        var v9 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        v9.AddArg2(s, v7);
        v8.AddArg2(v5, v9);
        v0.AddArg2(v1, v8);
        var v10 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
        v10.AddArg2(v5, s);
        v.AddArg2(v0, v10);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x32 x s)
    // result: (Int64Make (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Lsh32x32 <typ.UInt32> (Int64Hi x) s) (Rsh32Ux32 <typ.UInt32> (Int64Lo x) (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s))) (Lsh32x32 <typ.UInt32> (Int64Lo x) (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32])))) (Lsh32x32 <typ.UInt32> (Int64Lo x) s))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v3.AddArg(x);
        v2.AddArg2(v3, s);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        var v6 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v7.AuxInt = int32ToAuxInt(32);
        v6.AddArg2(v7, s);
        v4.AddArg2(v5, v6);
        v1.AddArg2(v2, v4);
        var v8 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
        var v9 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        v9.AddArg2(s, v7);
        v8.AddArg2(v5, v9);
        v0.AddArg2(v1, v8);
        var v10 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
        v10.AddArg2(v5, s);
        v.AddArg2(v0, v10);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const64 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Lsh64x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Lsh64x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpLsh64x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Lsh64x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Lsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpLsh64x32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh64x64 x y)
    // result: (Lsh64x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpLsh64x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x8 x s)
    // result: (Int64Make (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Lsh32x8 <typ.UInt32> (Int64Hi x) s) (Rsh32Ux8 <typ.UInt32> (Int64Lo x) (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s))) (Lsh32x8 <typ.UInt32> (Int64Lo x) (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32])))) (Lsh32x8 <typ.UInt32> (Int64Lo x) s))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v3.AddArg(x);
        v2.AddArg2(v3, s);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        var v6 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        var v7 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
        v7.AuxInt = int8ToAuxInt(32);
        v6.AddArg2(v7, s);
        v4.AddArg2(v5, v6);
        v1.AddArg2(v2, v4);
        var v8 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
        var v9 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        v9.AddArg2(s, v7);
        v8.AddArg2(v5, v9);
        v0.AddArg2(v1, v8);
        var v10 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
        v10.AddArg2(v5, s);
        v.AddArg2(v0, v10);
        return true;
    }

}
private static bool rewriteValuedec64_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Lsh8x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Lsh8x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpLsh8x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Lsh8x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Lsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpLsh8x32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh8x64 x y)
    // result: (Lsh8x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpLsh8x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpMul64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mul64 x y)
    // result: (Int64Make (Add32 <typ.UInt32> (Mul32 <typ.UInt32> (Int64Lo x) (Int64Hi y)) (Add32 <typ.UInt32> (Mul32 <typ.UInt32> (Int64Hi x) (Int64Lo y)) (Select0 <typ.UInt32> (Mul32uhilo (Int64Lo x) (Int64Lo y))))) (Select1 <typ.UInt32> (Mul32uhilo (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v2.AddArg(x);
        var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v3.AddArg(y);
        v1.AddArg2(v2, v3);
        var v4 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v6.AddArg(x);
        var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v7.AddArg(y);
        v5.AddArg2(v6, v7);
        var v8 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
        var v9 = b.NewValue0(v.Pos, OpMul32uhilo, types.NewTuple(typ.UInt32, typ.UInt32));
        v9.AddArg2(v2, v7);
        v8.AddArg(v9);
        v4.AddArg2(v5, v8);
        v0.AddArg2(v1, v4);
        var v10 = b.NewValue0(v.Pos, OpSelect1, typ.UInt32);
        v10.AddArg(v9);
        v.AddArg2(v0, v10);
        return true;
    }

}
private static bool rewriteValuedec64_OpNeg64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neg64 <t> x)
    // result: (Sub64 (Const64 <t> [0]) x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpSub64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValuedec64_OpNeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq64 x y)
    // result: (OrB (Neq32 (Int64Hi x) (Int64Hi y)) (Neq32 (Int64Lo x) (Int64Lo y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpOrB);
        var v0 = b.NewValue0(v.Pos, OpNeq32, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpNeq32, typ.Bool);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(y);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpOr32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Or32 <typ.UInt32> (Zeromask (Const32 [c])) y)
    // cond: c == 0
    // result: y
    while (true) {
        if (v.Type != typ.UInt32) {
            break;
        }
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpZeromask) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                var c = auxIntToInt32(v_0_0.AuxInt);
                var y = v_1;
                if (!(c == 0)) {
                    continue;
                }

                v.copyOf(y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 <typ.UInt32> (Zeromask (Const32 [c])) y)
    // cond: c != 0
    // result: (Const32 <typ.UInt32> [-1])
    while (true) {
        if (v.Type != typ.UInt32) {
            break;
        }
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpZeromask) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (!(c != 0)) {
                    continue;
                }

                v.reset(OpConst32);
                v.Type = typ.UInt32;
                v.AuxInt = int32ToAuxInt(-1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuedec64_OpOr64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Or64 x y)
    // result: (Int64Make (Or32 <typ.UInt32> (Int64Hi x) (Int64Hi y)) (Or32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(y);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh16Ux64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh16Ux32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh16Ux32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh16Ux64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh16Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh16Ux32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux64 x y)
    // result: (Rsh16Ux32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh16Ux32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 x (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Signmask (SignExt16to32 x))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpSignmask);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh16x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh16x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh16x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh16x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh16x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16x64 x y)
    // result: (Rsh16x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh16x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh32Ux64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh32Ux32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh32Ux32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh32Ux64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh32Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh32Ux32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux64 x y)
    // result: (Rsh32Ux32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh32Ux32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x64 x (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Signmask x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpSignmask);
        v.AddArg(x);
        return true;

    } 
    // match: (Rsh32x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh32x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh32x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh32x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh32x32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32x64 x y)
    // result: (Rsh32x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh32x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux16 x s)
    // result: (Int64Make (Rsh32Ux16 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux16 <typ.UInt32> (Int64Lo x) s) (Lsh32x16 <typ.UInt32> (Int64Hi x) (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s))) (Rsh32Ux16 <typ.UInt32> (Int64Hi x) (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32])))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        var v8 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
        v8.AuxInt = int16ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        v10.AddArg2(s, v8);
        v9.AddArg2(v1, v10);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux32 x s)
    // result: (Int64Make (Rsh32Ux32 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux32 <typ.UInt32> (Int64Lo x) s) (Lsh32x32 <typ.UInt32> (Int64Hi x) (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s))) (Rsh32Ux32 <typ.UInt32> (Int64Hi x) (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32])))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        var v8 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v8.AuxInt = int32ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        v10.AddArg2(s, v8);
        v9.AddArg2(v1, v10);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const64 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Rsh64Ux64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh64Ux32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh64Ux32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh64Ux64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh64Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh64Ux32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux64 x y)
    // result: (Rsh64Ux32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux8 x s)
    // result: (Int64Make (Rsh32Ux8 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux8 <typ.UInt32> (Int64Lo x) s) (Lsh32x8 <typ.UInt32> (Int64Hi x) (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s))) (Rsh32Ux8 <typ.UInt32> (Int64Hi x) (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32])))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        var v8 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
        v8.AuxInt = int8ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        v10.AddArg2(s, v8);
        v9.AddArg2(v1, v10);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x16 x s)
    // result: (Int64Make (Rsh32x16 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux16 <typ.UInt32> (Int64Lo x) s) (Lsh32x16 <typ.UInt32> (Int64Hi x) (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s))) (And32 <typ.UInt32> (Rsh32x16 <typ.UInt32> (Int64Hi x) (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32]))) (Zeromask (ZeroExt16to32 (Rsh16Ux32 <typ.UInt16> s (Const32 <typ.UInt32> [5])))))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32x16, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        var v8 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
        v8.AuxInt = int16ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpRsh32x16, typ.UInt32);
        var v11 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
        v11.AddArg2(s, v8);
        v10.AddArg2(v1, v11);
        var v12 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v13 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        var v14 = b.NewValue0(v.Pos, OpRsh16Ux32, typ.UInt16);
        var v15 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v15.AuxInt = int32ToAuxInt(5);
        v14.AddArg2(s, v15);
        v13.AddArg(v14);
        v12.AddArg(v13);
        v9.AddArg2(v10, v12);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x32 x s)
    // result: (Int64Make (Rsh32x32 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux32 <typ.UInt32> (Int64Lo x) s) (Lsh32x32 <typ.UInt32> (Int64Hi x) (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s))) (And32 <typ.UInt32> (Rsh32x32 <typ.UInt32> (Int64Hi x) (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32]))) (Zeromask (Rsh32Ux32 <typ.UInt32> s (Const32 <typ.UInt32> [5]))))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32x32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        var v8 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v8.AuxInt = int32ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpRsh32x32, typ.UInt32);
        var v11 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
        v11.AddArg2(s, v8);
        v10.AddArg2(v1, v11);
        var v12 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v13 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
        var v14 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v14.AuxInt = int32ToAuxInt(5);
        v13.AddArg2(s, v14);
        v12.AddArg(v13);
        v9.AddArg2(v10, v12);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x64 x (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Int64Make (Signmask (Int64Hi x)) (Signmask (Int64Hi x)))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg2(v0, v0);
        return true;

    } 
    // match: (Rsh64x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh64x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh64x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh64x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh64x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64x64 x y)
    // result: (Rsh64x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh64x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x8 x s)
    // result: (Int64Make (Rsh32x8 <typ.UInt32> (Int64Hi x) s) (Or32 <typ.UInt32> (Or32 <typ.UInt32> (Rsh32Ux8 <typ.UInt32> (Int64Lo x) s) (Lsh32x8 <typ.UInt32> (Int64Hi x) (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s))) (And32 <typ.UInt32> (Rsh32x8 <typ.UInt32> (Int64Hi x) (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32]))) (Zeromask (ZeroExt8to32 (Rsh8Ux32 <typ.UInt8> s (Const32 <typ.UInt32> [5])))))))
    while (true) {
        var x = v_0;
        var s = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpRsh32x8, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, s);
        var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        v4.AddArg2(v5, s);
        var v6 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        var v8 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
        v8.AuxInt = int8ToAuxInt(32);
        v7.AddArg2(v8, s);
        v6.AddArg2(v1, v7);
        v3.AddArg2(v4, v6);
        var v9 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpRsh32x8, typ.UInt32);
        var v11 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
        v11.AddArg2(s, v8);
        v10.AddArg2(v1, v11);
        var v12 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v13 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        var v14 = b.NewValue0(v.Pos, OpRsh8Ux32, typ.UInt8);
        var v15 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v15.AuxInt = int32ToAuxInt(5);
        v14.AddArg2(s, v15);
        v13.AddArg(v14);
        v12.AddArg(v13);
        v9.AddArg2(v10, v12);
        v2.AddArg2(v3, v9);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 _ (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh8Ux64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh8Ux32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh8Ux32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh8Ux64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh8Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh8Ux32);
        var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux64 x y)
    // result: (Rsh8Ux32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh8Ux32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 x (Int64Make (Const32 [c]) _))
    // cond: c != 0
    // result: (Signmask (SignExt8to32 x))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpSignmask);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh8x64 [c] x (Int64Make (Const32 [0]) lo))
    // result: (Rsh8x32 [c] x lo)
    while (true) {
        c = auxIntToBool(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || auxIntToInt32(v_1_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpRsh8x32);
        v.AuxInt = boolToAuxInt(c);
        v.AddArg2(x, lo);
        return true;

    } 
    // match: (Rsh8x64 x (Int64Make hi lo))
    // cond: hi.Op != OpConst32
    // result: (Rsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
    while (true) {
        x = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        var hi = v_1.Args[0];
        if (!(hi.Op != OpConst32)) {
            break;
        }
        v.reset(OpRsh8x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        v1.AddArg(hi);
        v0.AddArg2(v1, lo);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8x64 x y)
    // result: (Rsh8x32 x (Or32 <typ.UInt32> (Zeromask (Int64Hi y)) (Int64Lo y)))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpRsh8x32);
        v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v1.AddArg(v2);
        var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v3.AddArg(y);
        v0.AddArg2(v1, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpSignExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt16to64 x)
    // result: (SignExt32to64 (SignExt16to32 x))
    while (true) {
        var x = v_0;
        v.reset(OpSignExt32to64);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpSignExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt32to64 x)
    // result: (Int64Make (Signmask x) x)
    while (true) {
        var x = v_0;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValuedec64_OpSignExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt8to64 x)
    // result: (SignExt32to64 (SignExt8to32 x))
    while (true) {
        var x = v_0;
        v.reset(OpSignExt32to64);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Store {t} dst (Int64Make hi lo) mem)
    // cond: t.Size() == 8 && !config.BigEndian
    // result: (Store {hi.Type} (OffPtr <hi.Type.PtrTo()> [4] dst) hi (Store {lo.Type} dst lo mem))
    while (true) {
        var t = auxToType(v.Aux);
        var dst = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        var lo = v_1.Args[1];
        var hi = v_1.Args[0];
        var mem = v_2;
        if (!(t.Size() == 8 && !config.BigEndian)) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(hi.Type);
        var v0 = b.NewValue0(v.Pos, OpOffPtr, hi.Type.PtrTo());
        v0.AuxInt = int64ToAuxInt(4);
        v0.AddArg(dst);
        var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(lo.Type);
        v1.AddArg3(dst, lo, mem);
        v.AddArg3(v0, hi, v1);
        return true;

    } 
    // match: (Store {t} dst (Int64Make hi lo) mem)
    // cond: t.Size() == 8 && config.BigEndian
    // result: (Store {lo.Type} (OffPtr <lo.Type.PtrTo()> [4] dst) lo (Store {hi.Type} dst hi mem))
    while (true) {
        t = auxToType(v.Aux);
        dst = v_0;
        if (v_1.Op != OpInt64Make) {
            break;
        }
        lo = v_1.Args[1];
        hi = v_1.Args[0];
        mem = v_2;
        if (!(t.Size() == 8 && config.BigEndian)) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(lo.Type);
        v0 = b.NewValue0(v.Pos, OpOffPtr, lo.Type.PtrTo());
        v0.AuxInt = int64ToAuxInt(4);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(hi.Type);
        v1.AddArg3(dst, hi, mem);
        v.AddArg3(v0, lo, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec64_OpSub64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Sub64 x y)
    // result: (Int64Make (Sub32withcarry <typ.Int32> (Int64Hi x) (Int64Hi y) (Select1 <types.TypeFlags> (Sub32carry (Int64Lo x) (Int64Lo y)))) (Select0 <typ.UInt32> (Sub32carry (Int64Lo x) (Int64Lo y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpSub32withcarry, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpSub32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(x);
        var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v6.AddArg(y);
        v4.AddArg2(v5, v6);
        v3.AddArg(v4);
        v0.AddArg3(v1, v2, v3);
        var v7 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
        v7.AddArg(v4);
        v.AddArg2(v0, v7);
        return true;
    }

}
private static bool rewriteValuedec64_OpTrunc64to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Trunc64to16 (Int64Make _ lo))
    // result: (Trunc32to16 lo)
    while (true) {
        if (v_0.Op != OpInt64Make) {
            break;
        }
        var lo = v_0.Args[1];
        v.reset(OpTrunc32to16);
        v.AddArg(lo);
        return true;

    } 
    // match: (Trunc64to16 x)
    // result: (Trunc32to16 (Int64Lo x))
    while (true) {
        var x = v_0;
        v.reset(OpTrunc32to16);
        var v0 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpTrunc64to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to32 (Int64Make _ lo))
    // result: lo
    while (true) {
        if (v_0.Op != OpInt64Make) {
            break;
        }
        var lo = v_0.Args[1];
        v.copyOf(lo);
        return true;

    } 
    // match: (Trunc64to32 x)
    // result: (Int64Lo x)
    while (true) {
        var x = v_0;
        v.reset(OpInt64Lo);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValuedec64_OpTrunc64to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Trunc64to8 (Int64Make _ lo))
    // result: (Trunc32to8 lo)
    while (true) {
        if (v_0.Op != OpInt64Make) {
            break;
        }
        var lo = v_0.Args[1];
        v.reset(OpTrunc32to8);
        v.AddArg(lo);
        return true;

    } 
    // match: (Trunc64to8 x)
    // result: (Trunc32to8 (Int64Lo x))
    while (true) {
        var x = v_0;
        v.reset(OpTrunc32to8);
        var v0 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpXor64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Xor64 x y)
    // result: (Int64Make (Xor32 <typ.UInt32> (Int64Hi x) (Int64Hi y)) (Xor32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpXor32, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpXor32, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
        v5.AddArg(y);
        v3.AddArg2(v4, v5);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValuedec64_OpZeroExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt16to64 x)
    // result: (ZeroExt32to64 (ZeroExt16to32 x))
    while (true) {
        var x = v_0;
        v.reset(OpZeroExt32to64);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValuedec64_OpZeroExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt32to64 x)
    // result: (Int64Make (Const32 <typ.UInt32> [0]) x)
    while (true) {
        var x = v_0;
        v.reset(OpInt64Make);
        var v0 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValuedec64_OpZeroExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt8to64 x)
    // result: (ZeroExt32to64 (ZeroExt8to32 x))
    while (true) {
        var x = v_0;
        v.reset(OpZeroExt32to64);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteBlockdec64(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return false;
}

} // end ssa_package
