// Code generated from gen/dec.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 August 29 09:08:03 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewritedec.go
using math = go.math_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static var _ = math.MinInt8; // in case not otherwise used
        private static var _ = obj.ANOP; // in case not otherwise used
        private static var _ = objabi.GOROOT; // in case not otherwise used
        private static var _ = types.TypeMem; // in case not otherwise used

        private static bool rewriteValuedec(ref Value v)
        {

            if (v.Op == OpComplexImag) 
                return rewriteValuedec_OpComplexImag_0(v);
            else if (v.Op == OpComplexReal) 
                return rewriteValuedec_OpComplexReal_0(v);
            else if (v.Op == OpIData) 
                return rewriteValuedec_OpIData_0(v);
            else if (v.Op == OpITab) 
                return rewriteValuedec_OpITab_0(v);
            else if (v.Op == OpLoad) 
                return rewriteValuedec_OpLoad_0(v);
            else if (v.Op == OpSliceCap) 
                return rewriteValuedec_OpSliceCap_0(v);
            else if (v.Op == OpSliceLen) 
                return rewriteValuedec_OpSliceLen_0(v);
            else if (v.Op == OpSlicePtr) 
                return rewriteValuedec_OpSlicePtr_0(v);
            else if (v.Op == OpStore) 
                return rewriteValuedec_OpStore_0(v);
            else if (v.Op == OpStringLen) 
                return rewriteValuedec_OpStringLen_0(v);
            else if (v.Op == OpStringPtr) 
                return rewriteValuedec_OpStringPtr_0(v);
                        return false;
        }
        private static bool rewriteValuedec_OpComplexImag_0(ref Value v)
        { 
            // match: (ComplexImag (ComplexMake _ imag))
            // cond:
            // result: imag
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpComplexMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var imag = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = imag.Type;
                v.AddArg(imag);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpComplexReal_0(ref Value v)
        { 
            // match: (ComplexReal (ComplexMake real _))
            // cond:
            // result: real
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpComplexMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var real = v_0.Args[0L];
                v.reset(OpCopy);
                v.Type = real.Type;
                v.AddArg(real);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpIData_0(ref Value v)
        { 
            // match: (IData (IMake _ data))
            // cond:
            // result: data
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpIMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var data = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = data.Type;
                v.AddArg(data);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpITab_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (ITab (IMake itab _))
            // cond:
            // result: itab
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpIMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var itab = v_0.Args[0L];
                v.reset(OpCopy);
                v.Type = itab.Type;
                v.AddArg(itab);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpLoad_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Load <t> ptr mem)
            // cond: t.IsComplex() && t.Size() == 8
            // result: (ComplexMake     (Load <typ.Float32> ptr mem)     (Load <typ.Float32>       (OffPtr <typ.Float32Ptr> [4] ptr)       mem)     )
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                if (!(t.IsComplex() && t.Size() == 8L))
                {
                    break;
                }
                v.reset(OpComplexMake);
                var v0 = b.NewValue0(v.Pos, OpLoad, typ.Float32);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpLoad, typ.Float32);
                var v2 = b.NewValue0(v.Pos, OpOffPtr, typ.Float32Ptr);
                v2.AuxInt = 4L;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: t.IsComplex() && t.Size() == 16
            // result: (ComplexMake     (Load <typ.Float64> ptr mem)     (Load <typ.Float64>       (OffPtr <typ.Float64Ptr> [8] ptr)       mem)     )
 
            // match: (Load <t> ptr mem)
            // cond: t.IsComplex() && t.Size() == 16
            // result: (ComplexMake     (Load <typ.Float64> ptr mem)     (Load <typ.Float64>       (OffPtr <typ.Float64Ptr> [8] ptr)       mem)     )
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t.IsComplex() && t.Size() == 16L))
                {
                    break;
                }
                v.reset(OpComplexMake);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.Float64);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.Float64);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.Float64Ptr);
                v2.AuxInt = 8L;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: t.IsString()
            // result: (StringMake     (Load <typ.BytePtr> ptr mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [config.PtrSize] ptr)       mem))
 
            // match: (Load <t> ptr mem)
            // cond: t.IsString()
            // result: (StringMake     (Load <typ.BytePtr> ptr mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [config.PtrSize] ptr)       mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t.IsString()))
                {
                    break;
                }
                v.reset(OpStringMake);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.BytePtr);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.Int);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v2.AuxInt = config.PtrSize;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: t.IsSlice()
            // result: (SliceMake     (Load <t.ElemType().PtrTo()> ptr mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [config.PtrSize] ptr)       mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [2*config.PtrSize] ptr)       mem))
 
            // match: (Load <t> ptr mem)
            // cond: t.IsSlice()
            // result: (SliceMake     (Load <t.ElemType().PtrTo()> ptr mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [config.PtrSize] ptr)       mem)     (Load <typ.Int>       (OffPtr <typ.IntPtr> [2*config.PtrSize] ptr)       mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t.IsSlice()))
                {
                    break;
                }
                v.reset(OpSliceMake);
                v0 = b.NewValue0(v.Pos, OpLoad, t.ElemType().PtrTo());
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.Int);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v2.AuxInt = config.PtrSize;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpLoad, typ.Int);
                var v4 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v4.AuxInt = 2L * config.PtrSize;
                v4.AddArg(ptr);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v.AddArg(v3);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: t.IsInterface()
            // result: (IMake     (Load <typ.BytePtr> ptr mem)     (Load <typ.BytePtr>       (OffPtr <typ.BytePtrPtr> [config.PtrSize] ptr)       mem))
 
            // match: (Load <t> ptr mem)
            // cond: t.IsInterface()
            // result: (IMake     (Load <typ.BytePtr> ptr mem)     (Load <typ.BytePtr>       (OffPtr <typ.BytePtrPtr> [config.PtrSize] ptr)       mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t.IsInterface()))
                {
                    break;
                }
                v.reset(OpIMake);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.BytePtr);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.BytePtr);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.BytePtrPtr);
                v2.AuxInt = config.PtrSize;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpSliceCap_0(ref Value v)
        { 
            // match: (SliceCap (SliceMake _ _ cap))
            // cond:
            // result: cap
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpSliceMake)
                {
                    break;
                }
                _ = v_0.Args[2L];
                var cap = v_0.Args[2L];
                v.reset(OpCopy);
                v.Type = cap.Type;
                v.AddArg(cap);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpSliceLen_0(ref Value v)
        { 
            // match: (SliceLen (SliceMake _ len _))
            // cond:
            // result: len
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpSliceMake)
                {
                    break;
                }
                _ = v_0.Args[2L];
                var len = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = len.Type;
                v.AddArg(len);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpSlicePtr_0(ref Value v)
        { 
            // match: (SlicePtr (SliceMake ptr _ _))
            // cond:
            // result: ptr
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpSliceMake)
                {
                    break;
                }
                _ = v_0.Args[2L];
                var ptr = v_0.Args[0L];
                v.reset(OpCopy);
                v.Type = ptr.Type;
                v.AddArg(ptr);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpStore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Store {t} dst (ComplexMake real imag) mem)
            // cond: t.(*types.Type).Size() == 8
            // result: (Store {typ.Float32}     (OffPtr <typ.Float32Ptr> [4] dst)     imag     (Store {typ.Float32} dst real mem))
            while (true)
            {
                var t = v.Aux;
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpComplexMake)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var real = v_1.Args[0L];
                var imag = v_1.Args[1L];
                var mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L))
                {
                    break;
                }
                v.reset(OpStore);
                v.Aux = typ.Float32;
                var v0 = b.NewValue0(v.Pos, OpOffPtr, typ.Float32Ptr);
                v0.AuxInt = 4L;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(imag);
                var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = typ.Float32;
                v1.AddArg(dst);
                v1.AddArg(real);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Store {t} dst (ComplexMake real imag) mem)
            // cond: t.(*types.Type).Size() == 16
            // result: (Store {typ.Float64}     (OffPtr <typ.Float64Ptr> [8] dst)     imag     (Store {typ.Float64} dst real mem))
 
            // match: (Store {t} dst (ComplexMake real imag) mem)
            // cond: t.(*types.Type).Size() == 16
            // result: (Store {typ.Float64}     (OffPtr <typ.Float64Ptr> [8] dst)     imag     (Store {typ.Float64} dst real mem))
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpComplexMake)
                {
                    break;
                }
                _ = v_1.Args[1L];
                real = v_1.Args[0L];
                imag = v_1.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 16L))
                {
                    break;
                }
                v.reset(OpStore);
                v.Aux = typ.Float64;
                v0 = b.NewValue0(v.Pos, OpOffPtr, typ.Float64Ptr);
                v0.AuxInt = 8L;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(imag);
                v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = typ.Float64;
                v1.AddArg(dst);
                v1.AddArg(real);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Store dst (StringMake ptr len) mem)
            // cond:
            // result: (Store {typ.Int}     (OffPtr <typ.IntPtr> [config.PtrSize] dst)     len     (Store {typ.BytePtr} dst ptr mem))
 
            // match: (Store dst (StringMake ptr len) mem)
            // cond:
            // result: (Store {typ.Int}     (OffPtr <typ.IntPtr> [config.PtrSize] dst)     len     (Store {typ.BytePtr} dst ptr mem))
            while (true)
            {
                _ = v.Args[2L];
                dst = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpStringMake)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var ptr = v_1.Args[0L];
                var len = v_1.Args[1L];
                mem = v.Args[2L];
                v.reset(OpStore);
                v.Aux = typ.Int;
                v0 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v0.AuxInt = config.PtrSize;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(len);
                v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = typ.BytePtr;
                v1.AddArg(dst);
                v1.AddArg(ptr);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Store dst (SliceMake ptr len cap) mem)
            // cond:
            // result: (Store {typ.Int}     (OffPtr <typ.IntPtr> [2*config.PtrSize] dst)     cap     (Store {typ.Int}       (OffPtr <typ.IntPtr> [config.PtrSize] dst)       len       (Store {typ.BytePtr} dst ptr mem)))
 
            // match: (Store dst (SliceMake ptr len cap) mem)
            // cond:
            // result: (Store {typ.Int}     (OffPtr <typ.IntPtr> [2*config.PtrSize] dst)     cap     (Store {typ.Int}       (OffPtr <typ.IntPtr> [config.PtrSize] dst)       len       (Store {typ.BytePtr} dst ptr mem)))
            while (true)
            {
                _ = v.Args[2L];
                dst = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpSliceMake)
                {
                    break;
                }
                _ = v_1.Args[2L];
                ptr = v_1.Args[0L];
                len = v_1.Args[1L];
                var cap = v_1.Args[2L];
                mem = v.Args[2L];
                v.reset(OpStore);
                v.Aux = typ.Int;
                v0 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v0.AuxInt = 2L * config.PtrSize;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(cap);
                v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = typ.Int;
                var v2 = b.NewValue0(v.Pos, OpOffPtr, typ.IntPtr);
                v2.AuxInt = config.PtrSize;
                v2.AddArg(dst);
                v1.AddArg(v2);
                v1.AddArg(len);
                var v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v3.Aux = typ.BytePtr;
                v3.AddArg(dst);
                v3.AddArg(ptr);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Store dst (IMake itab data) mem)
            // cond:
            // result: (Store {typ.BytePtr}     (OffPtr <typ.BytePtrPtr> [config.PtrSize] dst)     data     (Store {typ.Uintptr} dst itab mem))
 
            // match: (Store dst (IMake itab data) mem)
            // cond:
            // result: (Store {typ.BytePtr}     (OffPtr <typ.BytePtrPtr> [config.PtrSize] dst)     data     (Store {typ.Uintptr} dst itab mem))
            while (true)
            {
                _ = v.Args[2L];
                dst = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpIMake)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var itab = v_1.Args[0L];
                var data = v_1.Args[1L];
                mem = v.Args[2L];
                v.reset(OpStore);
                v.Aux = typ.BytePtr;
                v0 = b.NewValue0(v.Pos, OpOffPtr, typ.BytePtrPtr);
                v0.AuxInt = config.PtrSize;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(data);
                v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = typ.Uintptr;
                v1.AddArg(dst);
                v1.AddArg(itab);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpStringLen_0(ref Value v)
        { 
            // match: (StringLen (StringMake _ len))
            // cond:
            // result: len
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpStringMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var len = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = len.Type;
                v.AddArg(len);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec_OpStringPtr_0(ref Value v)
        { 
            // match: (StringPtr (StringMake ptr _))
            // cond:
            // result: ptr
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpStringMake)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var ptr = v_0.Args[0L];
                v.reset(OpCopy);
                v.Type = ptr.Type;
                v.AddArg(ptr);
                return true;
            }

            return false;
        }
        private static bool rewriteBlockdec(ref Block b)
        {
            var config = b.Func.Config;
            _ = config;
            var fe = b.Func.fe;
            _ = fe;
            var typ = ref config.Types;
            _ = typ;
            switch (b.Kind)
            {
            }
            return false;
        }
    }
}}}}
