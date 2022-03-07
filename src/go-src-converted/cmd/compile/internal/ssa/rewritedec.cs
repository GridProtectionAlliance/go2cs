// Code generated from gen/dec.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 23:00:14 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewritedec.go
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValuedec(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpComplexImag) 
        return rewriteValuedec_OpComplexImag(_addr_v);
    else if (v.Op == OpComplexReal) 
        return rewriteValuedec_OpComplexReal(_addr_v);
    else if (v.Op == OpIData) 
        return rewriteValuedec_OpIData(_addr_v);
    else if (v.Op == OpITab) 
        return rewriteValuedec_OpITab(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValuedec_OpLoad(_addr_v);
    else if (v.Op == OpSliceCap) 
        return rewriteValuedec_OpSliceCap(_addr_v);
    else if (v.Op == OpSliceLen) 
        return rewriteValuedec_OpSliceLen(_addr_v);
    else if (v.Op == OpSlicePtr) 
        return rewriteValuedec_OpSlicePtr(_addr_v);
    else if (v.Op == OpSlicePtrUnchecked) 
        return rewriteValuedec_OpSlicePtrUnchecked(_addr_v);
    else if (v.Op == OpStore) 
        return rewriteValuedec_OpStore(_addr_v);
    else if (v.Op == OpStringLen) 
        return rewriteValuedec_OpStringLen(_addr_v);
    else if (v.Op == OpStringPtr) 
        return rewriteValuedec_OpStringPtr(_addr_v);
        return false;

}
private static bool rewriteValuedec_OpComplexImag(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ComplexImag (ComplexMake _ imag ))
    // result: imag
    while (true) {
        if (v_0.Op != OpComplexMake) {
            break;
        }
        var imag = v_0.Args[1];
        v.copyOf(imag);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpComplexReal(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ComplexReal (ComplexMake real _ ))
    // result: real
    while (true) {
        if (v_0.Op != OpComplexMake) {
            break;
        }
        var real = v_0.Args[0];
        v.copyOf(real);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpIData(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (IData (IMake _ data))
    // result: data
    while (true) {
        if (v_0.Op != OpIMake) {
            break;
        }
        var data = v_0.Args[1];
        v.copyOf(data);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpITab(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ITab (IMake itab _))
    // result: itab
    while (true) {
        if (v_0.Op != OpIMake) {
            break;
        }
        var itab = v_0.Args[0];
        v.copyOf(itab);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Load <t> ptr mem)
    // cond: t.IsComplex() && t.Size() == 8
    // result: (ComplexMake (Load <typ.Float32> ptr mem) (Load <typ.Float32> (OffPtr <typ.Float32Ptr> [4] ptr) mem) )
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(t.IsComplex() && t.Size() == 8)) {
            break;
        }
        v.reset(OpComplexMake);
        var v0 = b.NewValue0(v.Pos, OpLoad, typ.Float32);
        v0.AddArg2(ptr, mem);
        var v1 = b.NewValue0(v.Pos, OpLoad, typ.Float32);
        var v2 = b.NewValue0(v.Pos, OpOffPtr, typ.Float32Ptr);
        v2.AuxInt = int64ToAuxInt(4);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsComplex() && t.Size() == 16
    // result: (ComplexMake (Load <typ.Float64> ptr mem) (Load <typ.Float64> (OffPtr <typ.Float64Ptr> [8] ptr) mem) )
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsComplex() && t.Size() == 16)) {
            break;
        }
        v.reset(OpComplexMake);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.Float64);
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.Float64);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.Float64Ptr);
        v2.AuxInt = int64ToAuxInt(8);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsString()
    // result: (StringMake (Load <typ.BytePtr> ptr mem) (Load <typ.Int> (OffPtr <typ.IntPtr> [config.PtrSize] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsString())) {
            break;
        }
        v.reset(OpStringMake);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.BytePtr);
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.Int);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v2.AuxInt = int64ToAuxInt(config.PtrSize);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsSlice()
    // result: (SliceMake (Load <t.Elem().PtrTo()> ptr mem) (Load <typ.Int> (OffPtr <typ.IntPtr> [config.PtrSize] ptr) mem) (Load <typ.Int> (OffPtr <typ.IntPtr> [2*config.PtrSize] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsSlice())) {
            break;
        }
        v.reset(OpSliceMake);
        v0 = b.NewValue0(v.Pos, OpLoad, t.Elem().PtrTo());
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.Int);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v2.AuxInt = int64ToAuxInt(config.PtrSize);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        var v3 = b.NewValue0(v.Pos, OpLoad, typ.Int);
        var v4 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v4.AuxInt = int64ToAuxInt(2 * config.PtrSize);
        v4.AddArg(ptr);
        v3.AddArg2(v4, mem);
        v.AddArg3(v0, v1, v3);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsInterface()
    // result: (IMake (Load <typ.Uintptr> ptr mem) (Load <typ.BytePtr> (OffPtr <typ.BytePtrPtr> [config.PtrSize] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsInterface())) {
            break;
        }
        v.reset(OpIMake);
        v0 = b.NewValue0(v.Pos, OpLoad, typ.Uintptr);
        v0.AddArg2(ptr, mem);
        v1 = b.NewValue0(v.Pos, OpLoad, typ.BytePtr);
        v2 = b.NewValue0(v.Pos, OpOffPtr, typ.BytePtrPtr);
        v2.AuxInt = int64ToAuxInt(config.PtrSize);
        v2.AddArg(ptr);
        v1.AddArg2(v2, mem);
        v.AddArg2(v0, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpSliceCap(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SliceCap (SliceMake _ _ cap))
    // result: cap
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        var cap = v_0.Args[2];
        v.copyOf(cap);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpSliceLen(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SliceLen (SliceMake _ len _))
    // result: len
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        var len = v_0.Args[1];
        v.copyOf(len);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpSlicePtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SlicePtr (SliceMake ptr _ _ ))
    // result: ptr
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        var ptr = v_0.Args[0];
        v.copyOf(ptr);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpSlicePtrUnchecked(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SlicePtrUnchecked (SliceMake ptr _ _ ))
    // result: ptr
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        var ptr = v_0.Args[0];
        v.copyOf(ptr);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Store {t} dst (ComplexMake real imag) mem)
    // cond: t.Size() == 8
    // result: (Store {typ.Float32} (OffPtr <typ.Float32Ptr> [4] dst) imag (Store {typ.Float32} dst real mem))
    while (true) {
        var t = auxToType(v.Aux);
        var dst = v_0;
        if (v_1.Op != OpComplexMake) {
            break;
        }
        var imag = v_1.Args[1];
        var real = v_1.Args[0];
        var mem = v_2;
        if (!(t.Size() == 8)) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(typ.Float32);
        var v0 = b.NewValue0(v.Pos, OpOffPtr, typ.Float32Ptr);
        v0.AuxInt = int64ToAuxInt(4);
        v0.AddArg(dst);
        var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(typ.Float32);
        v1.AddArg3(dst, real, mem);
        v.AddArg3(v0, imag, v1);
        return true;

    } 
    // match: (Store {t} dst (ComplexMake real imag) mem)
    // cond: t.Size() == 16
    // result: (Store {typ.Float64} (OffPtr <typ.Float64Ptr> [8] dst) imag (Store {typ.Float64} dst real mem))
    while (true) {
        t = auxToType(v.Aux);
        dst = v_0;
        if (v_1.Op != OpComplexMake) {
            break;
        }
        imag = v_1.Args[1];
        real = v_1.Args[0];
        mem = v_2;
        if (!(t.Size() == 16)) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(typ.Float64);
        v0 = b.NewValue0(v.Pos, OpOffPtr, typ.Float64Ptr);
        v0.AuxInt = int64ToAuxInt(8);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(typ.Float64);
        v1.AddArg3(dst, real, mem);
        v.AddArg3(v0, imag, v1);
        return true;

    } 
    // match: (Store dst (StringMake ptr len) mem)
    // result: (Store {typ.Int} (OffPtr <typ.IntPtr> [config.PtrSize] dst) len (Store {typ.BytePtr} dst ptr mem))
    while (true) {
        dst = v_0;
        if (v_1.Op != OpStringMake) {
            break;
        }
        var len = v_1.Args[1];
        var ptr = v_1.Args[0];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(typ.Int);
        v0 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v0.AuxInt = int64ToAuxInt(config.PtrSize);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(typ.BytePtr);
        v1.AddArg3(dst, ptr, mem);
        v.AddArg3(v0, len, v1);
        return true;

    } 
    // match: (Store {t} dst (SliceMake ptr len cap) mem)
    // result: (Store {typ.Int} (OffPtr <typ.IntPtr> [2*config.PtrSize] dst) cap (Store {typ.Int} (OffPtr <typ.IntPtr> [config.PtrSize] dst) len (Store {t.Elem().PtrTo()} dst ptr mem)))
    while (true) {
        t = auxToType(v.Aux);
        dst = v_0;
        if (v_1.Op != OpSliceMake) {
            break;
        }
        var cap = v_1.Args[2];
        ptr = v_1.Args[0];
        len = v_1.Args[1];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(typ.Int);
        v0 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v0.AuxInt = int64ToAuxInt(2 * config.PtrSize);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(typ.Int);
        var v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
        v2.AuxInt = int64ToAuxInt(config.PtrSize);
        v2.AddArg(dst);
        var v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t.Elem().PtrTo());
        v3.AddArg3(dst, ptr, mem);
        v1.AddArg3(v2, len, v3);
        v.AddArg3(v0, cap, v1);
        return true;

    } 
    // match: (Store dst (IMake itab data) mem)
    // result: (Store {typ.BytePtr} (OffPtr <typ.BytePtrPtr> [config.PtrSize] dst) data (Store {typ.Uintptr} dst itab mem))
    while (true) {
        dst = v_0;
        if (v_1.Op != OpIMake) {
            break;
        }
        var data = v_1.Args[1];
        var itab = v_1.Args[0];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(typ.BytePtr);
        v0 = b.NewValue0(v.Pos, OpOffPtr, typ.BytePtrPtr);
        v0.AuxInt = int64ToAuxInt(config.PtrSize);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(typ.Uintptr);
        v1.AddArg3(dst, itab, mem);
        v.AddArg3(v0, data, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpStringLen(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (StringLen (StringMake _ len))
    // result: len
    while (true) {
        if (v_0.Op != OpStringMake) {
            break;
        }
        var len = v_0.Args[1];
        v.copyOf(len);
        return true;

    }
    return false;

}
private static bool rewriteValuedec_OpStringPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (StringPtr (StringMake ptr _))
    // result: ptr
    while (true) {
        if (v_0.Op != OpStringMake) {
            break;
        }
        var ptr = v_0.Args[0];
        v.copyOf(ptr);
        return true;

    }
    return false;

}
private static bool rewriteBlockdec(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return false;
}

} // end ssa_package
