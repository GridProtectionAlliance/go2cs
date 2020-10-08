// Code generated from gen/decArgs.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 October 08 04:19:36 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewritedecArgs.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static bool rewriteValuedecArgs(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Op == OpArg) 
                return rewriteValuedecArgs_OpArg(_addr_v);
                        return false;

        }
        private static bool rewriteValuedecArgs_OpArg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var b = v.Block;
            var config = b.Func.Config;
            var fe = b.Func.fe;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Arg {n} [off])
            // cond: v.Type.IsString()
            // result: (StringMake (Arg <typ.BytePtr> {n} [off]) (Arg <typ.Int> {n} [off+int32(config.PtrSize)]))
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                var n = auxToSym(v.Aux);
                if (!(v.Type.IsString()))
                {
                    break;
                }

                v.reset(OpStringMake);
                var v0 = b.NewValue0(v.Pos, OpArg, typ.BytePtr);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                var v1 = b.NewValue0(v.Pos, OpArg, typ.Int);
                v1.AuxInt = int32ToAuxInt(off + int32(config.PtrSize));
                v1.Aux = symToAux(n);
                v.AddArg2(v0, v1);
                return true;

            } 
            // match: (Arg {n} [off])
            // cond: v.Type.IsSlice()
            // result: (SliceMake (Arg <v.Type.Elem().PtrTo()> {n} [off]) (Arg <typ.Int> {n} [off+int32(config.PtrSize)]) (Arg <typ.Int> {n} [off+2*int32(config.PtrSize)]))
 
            // match: (Arg {n} [off])
            // cond: v.Type.IsSlice()
            // result: (SliceMake (Arg <v.Type.Elem().PtrTo()> {n} [off]) (Arg <typ.Int> {n} [off+int32(config.PtrSize)]) (Arg <typ.Int> {n} [off+2*int32(config.PtrSize)]))
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(v.Type.IsSlice()))
                {
                    break;
                }

                v.reset(OpSliceMake);
                v0 = b.NewValue0(v.Pos, OpArg, v.Type.Elem().PtrTo());
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, typ.Int);
                v1.AuxInt = int32ToAuxInt(off + int32(config.PtrSize));
                v1.Aux = symToAux(n);
                var v2 = b.NewValue0(v.Pos, OpArg, typ.Int);
                v2.AuxInt = int32ToAuxInt(off + 2L * int32(config.PtrSize));
                v2.Aux = symToAux(n);
                v.AddArg3(v0, v1, v2);
                return true;

            } 
            // match: (Arg {n} [off])
            // cond: v.Type.IsInterface()
            // result: (IMake (Arg <typ.Uintptr> {n} [off]) (Arg <typ.BytePtr> {n} [off+int32(config.PtrSize)]))
 
            // match: (Arg {n} [off])
            // cond: v.Type.IsInterface()
            // result: (IMake (Arg <typ.Uintptr> {n} [off]) (Arg <typ.BytePtr> {n} [off+int32(config.PtrSize)]))
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(v.Type.IsInterface()))
                {
                    break;
                }

                v.reset(OpIMake);
                v0 = b.NewValue0(v.Pos, OpArg, typ.Uintptr);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, typ.BytePtr);
                v1.AuxInt = int32ToAuxInt(off + int32(config.PtrSize));
                v1.Aux = symToAux(n);
                v.AddArg2(v0, v1);
                return true;

            } 
            // match: (Arg {n} [off])
            // cond: v.Type.IsComplex() && v.Type.Size() == 16
            // result: (ComplexMake (Arg <typ.Float64> {n} [off]) (Arg <typ.Float64> {n} [off+8]))
 
            // match: (Arg {n} [off])
            // cond: v.Type.IsComplex() && v.Type.Size() == 16
            // result: (ComplexMake (Arg <typ.Float64> {n} [off]) (Arg <typ.Float64> {n} [off+8]))
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(v.Type.IsComplex() && v.Type.Size() == 16L))
                {
                    break;
                }

                v.reset(OpComplexMake);
                v0 = b.NewValue0(v.Pos, OpArg, typ.Float64);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, typ.Float64);
                v1.AuxInt = int32ToAuxInt(off + 8L);
                v1.Aux = symToAux(n);
                v.AddArg2(v0, v1);
                return true;

            } 
            // match: (Arg {n} [off])
            // cond: v.Type.IsComplex() && v.Type.Size() == 8
            // result: (ComplexMake (Arg <typ.Float32> {n} [off]) (Arg <typ.Float32> {n} [off+4]))
 
            // match: (Arg {n} [off])
            // cond: v.Type.IsComplex() && v.Type.Size() == 8
            // result: (ComplexMake (Arg <typ.Float32> {n} [off]) (Arg <typ.Float32> {n} [off+4]))
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(v.Type.IsComplex() && v.Type.Size() == 8L))
                {
                    break;
                }

                v.reset(OpComplexMake);
                v0 = b.NewValue0(v.Pos, OpArg, typ.Float32);
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, typ.Float32);
                v1.AuxInt = int32ToAuxInt(off + 4L);
                v1.Aux = symToAux(n);
                v.AddArg2(v0, v1);
                return true;

            } 
            // match: (Arg <t>)
            // cond: t.IsStruct() && t.NumFields() == 0 && fe.CanSSA(t)
            // result: (StructMake0)
 
            // match: (Arg <t>)
            // cond: t.IsStruct() && t.NumFields() == 0 && fe.CanSSA(t)
            // result: (StructMake0)
            while (true)
            {
                var t = v.Type;
                if (!(t.IsStruct() && t.NumFields() == 0L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpStructMake0);
                return true;

            } 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 1 && fe.CanSSA(t)
            // result: (StructMake1 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]))
 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 1 && fe.CanSSA(t)
            // result: (StructMake1 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]))
            while (true)
            {
                t = v.Type;
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(t.IsStruct() && t.NumFields() == 1L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpStructMake1);
                v0 = b.NewValue0(v.Pos, OpArg, t.FieldType(0L));
                v0.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(0L)));
                v0.Aux = symToAux(n);
                v.AddArg(v0);
                return true;

            } 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 2 && fe.CanSSA(t)
            // result: (StructMake2 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]))
 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 2 && fe.CanSSA(t)
            // result: (StructMake2 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]))
            while (true)
            {
                t = v.Type;
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(t.IsStruct() && t.NumFields() == 2L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpStructMake2);
                v0 = b.NewValue0(v.Pos, OpArg, t.FieldType(0L));
                v0.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(0L)));
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, t.FieldType(1L));
                v1.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(1L)));
                v1.Aux = symToAux(n);
                v.AddArg2(v0, v1);
                return true;

            } 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 3 && fe.CanSSA(t)
            // result: (StructMake3 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]) (Arg <t.FieldType(2)> {n} [off+int32(t.FieldOff(2))]))
 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 3 && fe.CanSSA(t)
            // result: (StructMake3 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]) (Arg <t.FieldType(2)> {n} [off+int32(t.FieldOff(2))]))
            while (true)
            {
                t = v.Type;
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(t.IsStruct() && t.NumFields() == 3L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpStructMake3);
                v0 = b.NewValue0(v.Pos, OpArg, t.FieldType(0L));
                v0.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(0L)));
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, t.FieldType(1L));
                v1.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(1L)));
                v1.Aux = symToAux(n);
                v2 = b.NewValue0(v.Pos, OpArg, t.FieldType(2L));
                v2.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(2L)));
                v2.Aux = symToAux(n);
                v.AddArg3(v0, v1, v2);
                return true;

            } 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 4 && fe.CanSSA(t)
            // result: (StructMake4 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]) (Arg <t.FieldType(2)> {n} [off+int32(t.FieldOff(2))]) (Arg <t.FieldType(3)> {n} [off+int32(t.FieldOff(3))]))
 
            // match: (Arg <t> {n} [off])
            // cond: t.IsStruct() && t.NumFields() == 4 && fe.CanSSA(t)
            // result: (StructMake4 (Arg <t.FieldType(0)> {n} [off+int32(t.FieldOff(0))]) (Arg <t.FieldType(1)> {n} [off+int32(t.FieldOff(1))]) (Arg <t.FieldType(2)> {n} [off+int32(t.FieldOff(2))]) (Arg <t.FieldType(3)> {n} [off+int32(t.FieldOff(3))]))
            while (true)
            {
                t = v.Type;
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(t.IsStruct() && t.NumFields() == 4L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpStructMake4);
                v0 = b.NewValue0(v.Pos, OpArg, t.FieldType(0L));
                v0.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(0L)));
                v0.Aux = symToAux(n);
                v1 = b.NewValue0(v.Pos, OpArg, t.FieldType(1L));
                v1.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(1L)));
                v1.Aux = symToAux(n);
                v2 = b.NewValue0(v.Pos, OpArg, t.FieldType(2L));
                v2.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(2L)));
                v2.Aux = symToAux(n);
                var v3 = b.NewValue0(v.Pos, OpArg, t.FieldType(3L));
                v3.AuxInt = int32ToAuxInt(off + int32(t.FieldOff(3L)));
                v3.Aux = symToAux(n);
                v.AddArg4(v0, v1, v2, v3);
                return true;

            } 
            // match: (Arg <t>)
            // cond: t.IsArray() && t.NumElem() == 0
            // result: (ArrayMake0)
 
            // match: (Arg <t>)
            // cond: t.IsArray() && t.NumElem() == 0
            // result: (ArrayMake0)
            while (true)
            {
                t = v.Type;
                if (!(t.IsArray() && t.NumElem() == 0L))
                {
                    break;
                }

                v.reset(OpArrayMake0);
                return true;

            } 
            // match: (Arg <t> {n} [off])
            // cond: t.IsArray() && t.NumElem() == 1 && fe.CanSSA(t)
            // result: (ArrayMake1 (Arg <t.Elem()> {n} [off]))
 
            // match: (Arg <t> {n} [off])
            // cond: t.IsArray() && t.NumElem() == 1 && fe.CanSSA(t)
            // result: (ArrayMake1 (Arg <t.Elem()> {n} [off]))
            while (true)
            {
                t = v.Type;
                off = auxIntToInt32(v.AuxInt);
                n = auxToSym(v.Aux);
                if (!(t.IsArray() && t.NumElem() == 1L && fe.CanSSA(t)))
                {
                    break;
                }

                v.reset(OpArrayMake1);
                v0 = b.NewValue0(v.Pos, OpArg, t.Elem());
                v0.AuxInt = int32ToAuxInt(off);
                v0.Aux = symToAux(n);
                v.AddArg(v0);
                return true;

            }

            return false;

        }
        private static bool rewriteBlockdecArgs(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            switch (b.Kind)
            {
            }
            return false;

        }
    }
}}}}
