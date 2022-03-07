// Code generated from gen/AMD64splitload.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 22:55:36 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewriteAMD64splitload.go


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValueAMD64splitload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAMD64CMPBconstload) 
        return rewriteValueAMD64splitload_OpAMD64CMPBconstload(_addr_v);
    else if (v.Op == OpAMD64CMPBconstloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPBconstloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPBload) 
        return rewriteValueAMD64splitload_OpAMD64CMPBload(_addr_v);
    else if (v.Op == OpAMD64CMPBloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPBloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPLconstload) 
        return rewriteValueAMD64splitload_OpAMD64CMPLconstload(_addr_v);
    else if (v.Op == OpAMD64CMPLconstloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPLconstloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPLconstloadidx4) 
        return rewriteValueAMD64splitload_OpAMD64CMPLconstloadidx4(_addr_v);
    else if (v.Op == OpAMD64CMPLload) 
        return rewriteValueAMD64splitload_OpAMD64CMPLload(_addr_v);
    else if (v.Op == OpAMD64CMPLloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPLloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPLloadidx4) 
        return rewriteValueAMD64splitload_OpAMD64CMPLloadidx4(_addr_v);
    else if (v.Op == OpAMD64CMPQconstload) 
        return rewriteValueAMD64splitload_OpAMD64CMPQconstload(_addr_v);
    else if (v.Op == OpAMD64CMPQconstloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPQconstloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPQconstloadidx8) 
        return rewriteValueAMD64splitload_OpAMD64CMPQconstloadidx8(_addr_v);
    else if (v.Op == OpAMD64CMPQload) 
        return rewriteValueAMD64splitload_OpAMD64CMPQload(_addr_v);
    else if (v.Op == OpAMD64CMPQloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPQloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPQloadidx8) 
        return rewriteValueAMD64splitload_OpAMD64CMPQloadidx8(_addr_v);
    else if (v.Op == OpAMD64CMPWconstload) 
        return rewriteValueAMD64splitload_OpAMD64CMPWconstload(_addr_v);
    else if (v.Op == OpAMD64CMPWconstloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPWconstloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPWconstloadidx2) 
        return rewriteValueAMD64splitload_OpAMD64CMPWconstloadidx2(_addr_v);
    else if (v.Op == OpAMD64CMPWload) 
        return rewriteValueAMD64splitload_OpAMD64CMPWload(_addr_v);
    else if (v.Op == OpAMD64CMPWloadidx1) 
        return rewriteValueAMD64splitload_OpAMD64CMPWloadidx1(_addr_v);
    else if (v.Op == OpAMD64CMPWloadidx2) 
        return rewriteValueAMD64splitload_OpAMD64CMPWloadidx2(_addr_v);
        return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPBconstload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPBconstload {sym} [vo] ptr mem)
    // cond: vo.Val() == 0
    // result: (TESTB x:(MOVBload {sym} [vo.Off()] ptr mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var mem = v_1;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTB);
        var x = b.NewValue0(v.Pos, OpAMD64MOVBload, typ.UInt8);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg2(ptr, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPBconstload {sym} [vo] ptr mem)
    // cond: vo.Val() != 0
    // result: (CMPBconst (MOVBload {sym} [vo.Off()] ptr mem) [vo.Val8()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPBconst);
        v.AuxInt = int8ToAuxInt(vo.Val8());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVBload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPBconstloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPBconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTB x:(MOVBloadidx1 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTB);
        var x = b.NewValue0(v.Pos, OpAMD64MOVBloadidx1, typ.UInt8);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPBconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPBconst (MOVBloadidx1 {sym} [vo.Off()] ptr idx mem) [vo.Val8()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPBconst);
        v.AuxInt = int8ToAuxInt(vo.Val8());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVBloadidx1, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPBload {sym} [off] ptr x mem)
    // result: (CMPB (MOVBload {sym} [off] ptr mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var x = v_1;
        var mem = v_2;
        v.reset(OpAMD64CMPB);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVBload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPBloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPBloadidx1 {sym} [off] ptr idx x mem)
    // result: (CMPB (MOVBloadidx1 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPB);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVBloadidx1, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLconstload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLconstload {sym} [vo] ptr mem)
    // cond: vo.Val() == 0
    // result: (TESTL x:(MOVLload {sym} [vo.Off()] ptr mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var mem = v_1;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTL);
        var x = b.NewValue0(v.Pos, OpAMD64MOVLload, typ.UInt32);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg2(ptr, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPLconstload {sym} [vo] ptr mem)
    // cond: vo.Val() != 0
    // result: (CMPLconst (MOVLload {sym} [vo.Off()] ptr mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPLconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLconstloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTL x:(MOVLloadidx1 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTL);
        var x = b.NewValue0(v.Pos, OpAMD64MOVLloadidx1, typ.UInt32);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPLconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPLconst (MOVLloadidx1 {sym} [vo.Off()] ptr idx mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPLconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLloadidx1, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLconstloadidx4(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLconstloadidx4 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTL x:(MOVLloadidx4 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTL);
        var x = b.NewValue0(v.Pos, OpAMD64MOVLloadidx4, typ.UInt32);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPLconstloadidx4 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPLconst (MOVLloadidx4 {sym} [vo.Off()] ptr idx mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPLconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLloadidx4, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLload {sym} [off] ptr x mem)
    // result: (CMPL (MOVLload {sym} [off] ptr mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var x = v_1;
        var mem = v_2;
        v.reset(OpAMD64CMPL);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLloadidx1 {sym} [off] ptr idx x mem)
    // result: (CMPL (MOVLloadidx1 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPL);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLloadidx1, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPLloadidx4(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPLloadidx4 {sym} [off] ptr idx x mem)
    // result: (CMPL (MOVLloadidx4 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPL);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVLloadidx4, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQconstload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQconstload {sym} [vo] ptr mem)
    // cond: vo.Val() == 0
    // result: (TESTQ x:(MOVQload {sym} [vo.Off()] ptr mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var mem = v_1;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTQ);
        var x = b.NewValue0(v.Pos, OpAMD64MOVQload, typ.UInt64);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg2(ptr, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPQconstload {sym} [vo] ptr mem)
    // cond: vo.Val() != 0
    // result: (CMPQconst (MOVQload {sym} [vo.Off()] ptr mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPQconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQload, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQconstloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTQ x:(MOVQloadidx1 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTQ);
        var x = b.NewValue0(v.Pos, OpAMD64MOVQloadidx1, typ.UInt64);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPQconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPQconst (MOVQloadidx1 {sym} [vo.Off()] ptr idx mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPQconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQloadidx1, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQconstloadidx8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQconstloadidx8 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTQ x:(MOVQloadidx8 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTQ);
        var x = b.NewValue0(v.Pos, OpAMD64MOVQloadidx8, typ.UInt64);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPQconstloadidx8 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPQconst (MOVQloadidx8 {sym} [vo.Off()] ptr idx mem) [vo.Val()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPQconst);
        v.AuxInt = int32ToAuxInt(vo.Val());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQloadidx8, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQload {sym} [off] ptr x mem)
    // result: (CMPQ (MOVQload {sym} [off] ptr mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var x = v_1;
        var mem = v_2;
        v.reset(OpAMD64CMPQ);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQload, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQloadidx1 {sym} [off] ptr idx x mem)
    // result: (CMPQ (MOVQloadidx1 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPQ);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQloadidx1, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPQloadidx8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPQloadidx8 {sym} [off] ptr idx x mem)
    // result: (CMPQ (MOVQloadidx8 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPQ);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVQloadidx8, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWconstload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWconstload {sym} [vo] ptr mem)
    // cond: vo.Val() == 0
    // result: (TESTW x:(MOVWload {sym} [vo.Off()] ptr mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var mem = v_1;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTW);
        var x = b.NewValue0(v.Pos, OpAMD64MOVWload, typ.UInt16);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg2(ptr, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPWconstload {sym} [vo] ptr mem)
    // cond: vo.Val() != 0
    // result: (CMPWconst (MOVWload {sym} [vo.Off()] ptr mem) [vo.Val16()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPWconst);
        v.AuxInt = int16ToAuxInt(vo.Val16());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWconstloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTW x:(MOVWloadidx1 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTW);
        var x = b.NewValue0(v.Pos, OpAMD64MOVWloadidx1, typ.UInt16);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPWconstloadidx1 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPWconst (MOVWloadidx1 {sym} [vo.Off()] ptr idx mem) [vo.Val16()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPWconst);
        v.AuxInt = int16ToAuxInt(vo.Val16());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWloadidx1, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWconstloadidx2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWconstloadidx2 {sym} [vo] ptr idx mem)
    // cond: vo.Val() == 0
    // result: (TESTW x:(MOVWloadidx2 {sym} [vo.Off()] ptr idx mem) x)
    while (true) {
        var vo = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var mem = v_2;
        if (!(vo.Val() == 0)) {
            break;
        }
        v.reset(OpAMD64TESTW);
        var x = b.NewValue0(v.Pos, OpAMD64MOVWloadidx2, typ.UInt16);
        x.AuxInt = int32ToAuxInt(vo.Off());
        x.Aux = symToAux(sym);
        x.AddArg3(ptr, idx, mem);
        v.AddArg2(x, x);
        return true;

    } 
    // match: (CMPWconstloadidx2 {sym} [vo] ptr idx mem)
    // cond: vo.Val() != 0
    // result: (CMPWconst (MOVWloadidx2 {sym} [vo.Off()] ptr idx mem) [vo.Val16()])
    while (true) {
        vo = auxIntToValAndOff(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        idx = v_1;
        mem = v_2;
        if (!(vo.Val() != 0)) {
            break;
        }
        v.reset(OpAMD64CMPWconst);
        v.AuxInt = int16ToAuxInt(vo.Val16());
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWloadidx2, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(vo.Off());
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWload {sym} [off] ptr x mem)
    // result: (CMPW (MOVWload {sym} [off] ptr mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var x = v_1;
        var mem = v_2;
        v.reset(OpAMD64CMPW);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWloadidx1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWloadidx1 {sym} [off] ptr idx x mem)
    // result: (CMPW (MOVWloadidx1 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPW);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWloadidx1, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteValueAMD64splitload_OpAMD64CMPWloadidx2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (CMPWloadidx2 {sym} [off] ptr idx x mem)
    // result: (CMPW (MOVWloadidx2 {sym} [off] ptr idx mem) x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        var idx = v_1;
        var x = v_2;
        var mem = v_3;
        v.reset(OpAMD64CMPW);
        var v0 = b.NewValue0(v.Pos, OpAMD64MOVWloadidx2, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, idx, mem);
        v.AddArg2(v0, x);
        return true;
    }

}
private static bool rewriteBlockAMD64splitload(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return false;
}

} // end ssa_package
