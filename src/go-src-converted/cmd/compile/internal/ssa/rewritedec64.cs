// Code generated from gen/dec64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 August 29 09:08:32 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewritedec64.go
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

        private static bool rewriteValuedec64(ref Value v)
        {

            if (v.Op == OpAdd64) 
                return rewriteValuedec64_OpAdd64_0(v);
            else if (v.Op == OpAnd64) 
                return rewriteValuedec64_OpAnd64_0(v);
            else if (v.Op == OpArg) 
                return rewriteValuedec64_OpArg_0(v);
            else if (v.Op == OpBitLen64) 
                return rewriteValuedec64_OpBitLen64_0(v);
            else if (v.Op == OpBswap64) 
                return rewriteValuedec64_OpBswap64_0(v);
            else if (v.Op == OpCom64) 
                return rewriteValuedec64_OpCom64_0(v);
            else if (v.Op == OpConst64) 
                return rewriteValuedec64_OpConst64_0(v);
            else if (v.Op == OpCtz64) 
                return rewriteValuedec64_OpCtz64_0(v);
            else if (v.Op == OpEq64) 
                return rewriteValuedec64_OpEq64_0(v);
            else if (v.Op == OpGeq64) 
                return rewriteValuedec64_OpGeq64_0(v);
            else if (v.Op == OpGeq64U) 
                return rewriteValuedec64_OpGeq64U_0(v);
            else if (v.Op == OpGreater64) 
                return rewriteValuedec64_OpGreater64_0(v);
            else if (v.Op == OpGreater64U) 
                return rewriteValuedec64_OpGreater64U_0(v);
            else if (v.Op == OpInt64Hi) 
                return rewriteValuedec64_OpInt64Hi_0(v);
            else if (v.Op == OpInt64Lo) 
                return rewriteValuedec64_OpInt64Lo_0(v);
            else if (v.Op == OpLeq64) 
                return rewriteValuedec64_OpLeq64_0(v);
            else if (v.Op == OpLeq64U) 
                return rewriteValuedec64_OpLeq64U_0(v);
            else if (v.Op == OpLess64) 
                return rewriteValuedec64_OpLess64_0(v);
            else if (v.Op == OpLess64U) 
                return rewriteValuedec64_OpLess64U_0(v);
            else if (v.Op == OpLoad) 
                return rewriteValuedec64_OpLoad_0(v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValuedec64_OpLsh16x64_0(v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValuedec64_OpLsh32x64_0(v);
            else if (v.Op == OpLsh64x16) 
                return rewriteValuedec64_OpLsh64x16_0(v);
            else if (v.Op == OpLsh64x32) 
                return rewriteValuedec64_OpLsh64x32_0(v);
            else if (v.Op == OpLsh64x64) 
                return rewriteValuedec64_OpLsh64x64_0(v);
            else if (v.Op == OpLsh64x8) 
                return rewriteValuedec64_OpLsh64x8_0(v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValuedec64_OpLsh8x64_0(v);
            else if (v.Op == OpMul64) 
                return rewriteValuedec64_OpMul64_0(v);
            else if (v.Op == OpNeg64) 
                return rewriteValuedec64_OpNeg64_0(v);
            else if (v.Op == OpNeq64) 
                return rewriteValuedec64_OpNeq64_0(v);
            else if (v.Op == OpOr64) 
                return rewriteValuedec64_OpOr64_0(v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValuedec64_OpRsh16Ux64_0(v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValuedec64_OpRsh16x64_0(v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValuedec64_OpRsh32Ux64_0(v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValuedec64_OpRsh32x64_0(v);
            else if (v.Op == OpRsh64Ux16) 
                return rewriteValuedec64_OpRsh64Ux16_0(v);
            else if (v.Op == OpRsh64Ux32) 
                return rewriteValuedec64_OpRsh64Ux32_0(v);
            else if (v.Op == OpRsh64Ux64) 
                return rewriteValuedec64_OpRsh64Ux64_0(v);
            else if (v.Op == OpRsh64Ux8) 
                return rewriteValuedec64_OpRsh64Ux8_0(v);
            else if (v.Op == OpRsh64x16) 
                return rewriteValuedec64_OpRsh64x16_0(v);
            else if (v.Op == OpRsh64x32) 
                return rewriteValuedec64_OpRsh64x32_0(v);
            else if (v.Op == OpRsh64x64) 
                return rewriteValuedec64_OpRsh64x64_0(v);
            else if (v.Op == OpRsh64x8) 
                return rewriteValuedec64_OpRsh64x8_0(v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValuedec64_OpRsh8Ux64_0(v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValuedec64_OpRsh8x64_0(v);
            else if (v.Op == OpSignExt16to64) 
                return rewriteValuedec64_OpSignExt16to64_0(v);
            else if (v.Op == OpSignExt32to64) 
                return rewriteValuedec64_OpSignExt32to64_0(v);
            else if (v.Op == OpSignExt8to64) 
                return rewriteValuedec64_OpSignExt8to64_0(v);
            else if (v.Op == OpStore) 
                return rewriteValuedec64_OpStore_0(v);
            else if (v.Op == OpSub64) 
                return rewriteValuedec64_OpSub64_0(v);
            else if (v.Op == OpTrunc64to16) 
                return rewriteValuedec64_OpTrunc64to16_0(v);
            else if (v.Op == OpTrunc64to32) 
                return rewriteValuedec64_OpTrunc64to32_0(v);
            else if (v.Op == OpTrunc64to8) 
                return rewriteValuedec64_OpTrunc64to8_0(v);
            else if (v.Op == OpXor64) 
                return rewriteValuedec64_OpXor64_0(v);
            else if (v.Op == OpZeroExt16to64) 
                return rewriteValuedec64_OpZeroExt16to64_0(v);
            else if (v.Op == OpZeroExt32to64) 
                return rewriteValuedec64_OpZeroExt32to64_0(v);
            else if (v.Op == OpZeroExt8to64) 
                return rewriteValuedec64_OpZeroExt8to64_0(v);
                        return false;
        }
        private static bool rewriteValuedec64_OpAdd64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Add64 x y)
            // cond:
            // result: (Int64Make         (Add32withcarry <typ.Int32>             (Int64Hi x)             (Int64Hi y)             (Select1 <types.TypeFlags> (Add32carry (Int64Lo x) (Int64Lo y))))         (Select0 <typ.UInt32> (Add32carry (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpAdd32withcarry, typ.Int32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
                var v4 = b.NewValue0(v.Pos, OpAdd32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                v0.AddArg(v3);
                v.AddArg(v0);
                var v7 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
                var v8 = b.NewValue0(v.Pos, OpAdd32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(x);
                v8.AddArg(v9);
                var v10 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v10.AddArg(y);
                v8.AddArg(v10);
                v7.AddArg(v8);
                v.AddArg(v7);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpAnd64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (And64 x y)
            // cond:
            // result: (Int64Make         (And32 <typ.UInt32> (Int64Hi x) (Int64Hi y))         (And32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(y);
                v3.AddArg(v5);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpArg_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && !config.BigEndian && v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.Int32> {n} [off+4])     (Arg <typ.UInt32> {n} [off]))
            while (true)
            {
                var off = v.AuxInt;
                var n = v.Aux;
                if (!(is64BitInt(v.Type) && !config.BigEndian && v.Type.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpArg, typ.Int32);
                v0.AuxInt = off + 4L;
                v0.Aux = n;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v1.AuxInt = off;
                v1.Aux = n;
                v.AddArg(v1);
                return true;
            } 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && !config.BigEndian && !v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.UInt32> {n} [off+4])     (Arg <typ.UInt32> {n} [off]))
 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && !config.BigEndian && !v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.UInt32> {n} [off+4])     (Arg <typ.UInt32> {n} [off]))
            while (true)
            {
                off = v.AuxInt;
                n = v.Aux;
                if (!(is64BitInt(v.Type) && !config.BigEndian && !v.Type.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v0.AuxInt = off + 4L;
                v0.Aux = n;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v1.AuxInt = off;
                v1.Aux = n;
                v.AddArg(v1);
                return true;
            } 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && config.BigEndian && v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.Int32> {n} [off])     (Arg <typ.UInt32> {n} [off+4]))
 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && config.BigEndian && v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.Int32> {n} [off])     (Arg <typ.UInt32> {n} [off+4]))
            while (true)
            {
                off = v.AuxInt;
                n = v.Aux;
                if (!(is64BitInt(v.Type) && config.BigEndian && v.Type.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpArg, typ.Int32);
                v0.AuxInt = off;
                v0.Aux = n;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v1.AuxInt = off + 4L;
                v1.Aux = n;
                v.AddArg(v1);
                return true;
            } 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && config.BigEndian && !v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.UInt32> {n} [off])     (Arg <typ.UInt32> {n} [off+4]))
 
            // match: (Arg {n} [off])
            // cond: is64BitInt(v.Type) && config.BigEndian && !v.Type.IsSigned()
            // result: (Int64Make     (Arg <typ.UInt32> {n} [off])     (Arg <typ.UInt32> {n} [off+4]))
            while (true)
            {
                off = v.AuxInt;
                n = v.Aux;
                if (!(is64BitInt(v.Type) && config.BigEndian && !v.Type.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v0.AuxInt = off;
                v0.Aux = n;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpArg, typ.UInt32);
                v1.AuxInt = off + 4L;
                v1.Aux = n;
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpBitLen64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (BitLen64 x)
            // cond:
            // result: (Add32 <typ.Int>         (BitLen32 <typ.Int> (Int64Hi x))         (BitLen32 <typ.Int>             (Or32 <typ.UInt32>                 (Int64Lo x)                 (Zeromask (Int64Hi x)))))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpAdd32);
                v.Type = typ.Int;
                var v0 = b.NewValue0(v.Pos, OpBitLen32, typ.Int);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpBitLen32, typ.Int);
                var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(x);
                v5.AddArg(v6);
                v3.AddArg(v5);
                v2.AddArg(v3);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpBswap64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Bswap64 x)
            // cond:
            // result: (Int64Make         (Bswap32 <typ.UInt32> (Int64Lo x))         (Bswap32 <typ.UInt32> (Int64Hi x)))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpBswap32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpBswap32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v3.AddArg(x);
                v2.AddArg(v3);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpCom64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Com64 x)
            // cond:
            // result: (Int64Make         (Com32 <typ.UInt32> (Int64Hi x))         (Com32 <typ.UInt32> (Int64Lo x)))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v3.AddArg(x);
                v2.AddArg(v3);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpConst64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Const64 <t> [c])
            // cond: t.IsSigned()
            // result: (Int64Make (Const32 <typ.Int32> [c>>32]) (Const32 <typ.UInt32> [int64(int32(c))]))
            while (true)
            {
                var t = v.Type;
                var c = v.AuxInt;
                if (!(t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
                v0.AuxInt = c >> (int)(32L);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v1.AuxInt = int64(int32(c));
                v.AddArg(v1);
                return true;
            } 
            // match: (Const64 <t> [c])
            // cond: !t.IsSigned()
            // result: (Int64Make (Const32 <typ.UInt32> [c>>32]) (Const32 <typ.UInt32> [int64(int32(c))]))
 
            // match: (Const64 <t> [c])
            // cond: !t.IsSigned()
            // result: (Int64Make (Const32 <typ.UInt32> [c>>32]) (Const32 <typ.UInt32> [int64(int32(c))]))
            while (true)
            {
                t = v.Type;
                c = v.AuxInt;
                if (!(!t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v0.AuxInt = c >> (int)(32L);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v1.AuxInt = int64(int32(c));
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpCtz64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Ctz64 x)
            // cond:
            // result: (Add32 <typ.UInt32>         (Ctz32 <typ.UInt32> (Int64Lo x))         (And32 <typ.UInt32>             (Com32 <typ.UInt32> (Zeromask (Int64Lo x)))             (Ctz32 <typ.UInt32> (Int64Hi x))))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpAdd32);
                v.Type = typ.UInt32;
                var v0 = b.NewValue0(v.Pos, OpCtz32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpCom32, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                v3.AddArg(v4);
                v2.AddArg(v3);
                var v6 = b.NewValue0(v.Pos, OpCtz32, typ.UInt32);
                var v7 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v7.AddArg(x);
                v6.AddArg(v7);
                v2.AddArg(v6);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpEq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq64 x y)
            // cond:
            // result: (AndB         (Eq32 (Int64Hi x) (Int64Hi y))         (Eq32 (Int64Lo x) (Int64Lo y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpAndB);
                var v0 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(y);
                v3.AddArg(v5);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpGeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq64 x y)
            // cond:
            // result: (OrB         (Greater32 (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Geq32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpGreater32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpGeq32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpGeq64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq64U x y)
            // cond:
            // result: (OrB         (Greater32U (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Geq32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpGreater32U, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpGeq32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpGreater64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater64 x y)
            // cond:
            // result: (OrB         (Greater32 (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Greater32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpGreater32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpGreater32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpGreater64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater64U x y)
            // cond:
            // result: (OrB         (Greater32U (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Greater32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpGreater32U, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpGreater32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpInt64Hi_0(ref Value v)
        { 
            // match: (Int64Hi (Int64Make hi _))
            // cond:
            // result: hi
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                v.reset(OpCopy);
                v.Type = hi.Type;
                v.AddArg(hi);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpInt64Lo_0(ref Value v)
        { 
            // match: (Int64Lo (Int64Make _ lo))
            // cond:
            // result: lo
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var lo = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = lo.Type;
                v.AddArg(lo);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq64 x y)
            // cond:
            // result: (OrB         (Less32 (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Leq32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpLess32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpLeq32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpLeq64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq64U x y)
            // cond:
            // result: (OrB         (Less32U (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Leq32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpLeq32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpLess64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less64 x y)
            // cond:
            // result: (OrB         (Less32 (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Less32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpLess32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpLess64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less64U x y)
            // cond:
            // result: (OrB         (Less32U (Int64Hi x) (Int64Hi y))         (AndB             (Eq32 (Int64Hi x) (Int64Hi y))             (Less32U (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpAndB, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpEq32, typ.Bool);
                var v5 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                var v7 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
                var v8 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v8.AddArg(x);
                v7.AddArg(v8);
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(y);
                v7.AddArg(v9);
                v3.AddArg(v7);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpLoad_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && !config.BigEndian && t.IsSigned()
            // result: (Int64Make         (Load <typ.Int32> (OffPtr <typ.Int32Ptr> [4] ptr) mem)         (Load <typ.UInt32> ptr mem))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                if (!(is64BitInt(t) && !config.BigEndian && t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpLoad, typ.Int32);
                var v1 = b.NewValue0(v.Pos, OpOffPtr, typ.Int32Ptr);
                v1.AuxInt = 4L;
                v1.AddArg(ptr);
                v0.AddArg(v1);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v2.AddArg(ptr);
                v2.AddArg(mem);
                v.AddArg(v2);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && !config.BigEndian && !t.IsSigned()
            // result: (Int64Make         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem)         (Load <typ.UInt32> ptr mem))
 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && !config.BigEndian && !t.IsSigned()
            // result: (Int64Make         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem)         (Load <typ.UInt32> ptr mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is64BitInt(t) && !config.BigEndian && !t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v1 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
                v1.AuxInt = 4L;
                v1.AddArg(ptr);
                v0.AddArg(v1);
                v0.AddArg(mem);
                v.AddArg(v0);
                v2 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v2.AddArg(ptr);
                v2.AddArg(mem);
                v.AddArg(v2);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && config.BigEndian && t.IsSigned()
            // result: (Int64Make         (Load <typ.Int32> ptr mem)         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && config.BigEndian && t.IsSigned()
            // result: (Int64Make         (Load <typ.Int32> ptr mem)         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is64BitInt(t) && config.BigEndian && t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.Int32);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
                v2.AuxInt = 4L;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && config.BigEndian && !t.IsSigned()
            // result: (Int64Make         (Load <typ.UInt32> ptr mem)         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
 
            // match: (Load <t> ptr mem)
            // cond: is64BitInt(t) && config.BigEndian && !t.IsSigned()
            // result: (Int64Make         (Load <typ.UInt32> ptr mem)         (Load <typ.UInt32> (OffPtr <typ.UInt32Ptr> [4] ptr) mem))
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is64BitInt(t) && config.BigEndian && !t.IsSigned()))
                {
                    break;
                }
                v.reset(OpInt64Make);
                v0 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v0.AddArg(ptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpLoad, typ.UInt32);
                v2 = b.NewValue0(v.Pos, OpOffPtr, typ.UInt32Ptr);
                v2.AuxInt = 4L;
                v2.AddArg(ptr);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh16x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Lsh16x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh16x32 x lo)
 
            // match: (Lsh16x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh16x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpLsh16x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Lsh16x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Lsh16x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpLsh16x32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh32x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Lsh32x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh32x32 x lo)
 
            // match: (Lsh32x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh32x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpLsh32x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Lsh32x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Lsh32x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpLsh32x32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh64x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x16 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Lsh32x16 <typ.UInt32> hi s)                 (Rsh32Ux16 <typ.UInt32>                     lo                     (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s)))             (Lsh32x16 <typ.UInt32>                 lo                 (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32]))))         (Lsh32x16 <typ.UInt32> lo s))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
                v2.AddArg(hi);
                v2.AddArg(s);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
                v3.AddArg(lo);
                var v4 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                var v5 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v5.AuxInt = 32L;
                v4.AddArg(v5);
                v4.AddArg(s);
                v3.AddArg(v4);
                v1.AddArg(v3);
                v0.AddArg(v1);
                var v6 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
                v6.AddArg(lo);
                var v7 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                v7.AddArg(s);
                var v8 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v8.AuxInt = 32L;
                v7.AddArg(v8);
                v6.AddArg(v7);
                v0.AddArg(v6);
                v.AddArg(v0);
                var v9 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
                v9.AddArg(lo);
                v9.AddArg(s);
                v.AddArg(v9);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh64x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x32 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Lsh32x32 <typ.UInt32> hi s)                 (Rsh32Ux32 <typ.UInt32>                     lo                     (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s)))             (Lsh32x32 <typ.UInt32>                 lo                 (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32]))))         (Lsh32x32 <typ.UInt32> lo s))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
                v2.AddArg(hi);
                v2.AddArg(s);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v3.AddArg(lo);
                var v4 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                var v5 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v5.AuxInt = 32L;
                v4.AddArg(v5);
                v4.AddArg(s);
                v3.AddArg(v4);
                v1.AddArg(v3);
                v0.AddArg(v1);
                var v6 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
                v6.AddArg(lo);
                var v7 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                v7.AddArg(s);
                var v8 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v8.AuxInt = 32L;
                v7.AddArg(v8);
                v6.AddArg(v7);
                v0.AddArg(v6);
                v.AddArg(v0);
                var v9 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
                v9.AddArg(lo);
                v9.AddArg(s);
                v.AddArg(v9);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh64x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const64 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst64);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Lsh64x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh64x32 x lo)
 
            // match: (Lsh64x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh64x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpLsh64x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Lsh64x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Lsh64x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpLsh64x32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh64x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x8 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Lsh32x8 <typ.UInt32> hi s)                 (Rsh32Ux8 <typ.UInt32>                     lo                     (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s)))             (Lsh32x8 <typ.UInt32>                 lo                 (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32]))))         (Lsh32x8 <typ.UInt32> lo s))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
                v2.AddArg(hi);
                v2.AddArg(s);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
                v3.AddArg(lo);
                var v4 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                var v5 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v5.AuxInt = 32L;
                v4.AddArg(v5);
                v4.AddArg(s);
                v3.AddArg(v4);
                v1.AddArg(v3);
                v0.AddArg(v1);
                var v6 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
                v6.AddArg(lo);
                var v7 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                v7.AddArg(s);
                var v8 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v8.AuxInt = 32L;
                v7.AddArg(v8);
                v6.AddArg(v7);
                v0.AddArg(v6);
                v.AddArg(v0);
                var v9 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
                v9.AddArg(lo);
                v9.AddArg(s);
                v.AddArg(v9);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpLsh8x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Lsh8x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh8x32 x lo)
 
            // match: (Lsh8x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Lsh8x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpLsh8x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Lsh8x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Lsh8x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Lsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpLsh8x32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpMul64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mul64 x y)
            // cond:
            // result: (Int64Make         (Add32 <typ.UInt32>             (Mul32 <typ.UInt32> (Int64Lo x) (Int64Hi y))             (Add32 <typ.UInt32>                 (Mul32 <typ.UInt32> (Int64Hi x) (Int64Lo y))                 (Select0 <typ.UInt32> (Mul32uhilo (Int64Lo x) (Int64Lo y)))))         (Select1 <typ.UInt32> (Mul32uhilo (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
                var v5 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                var v6 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v6.AddArg(x);
                v5.AddArg(v6);
                var v7 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v7.AddArg(y);
                v5.AddArg(v7);
                v4.AddArg(v5);
                var v8 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
                var v9 = b.NewValue0(v.Pos, OpMul32uhilo, types.NewTuple(typ.UInt32, typ.UInt32));
                var v10 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v10.AddArg(x);
                v9.AddArg(v10);
                var v11 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v11.AddArg(y);
                v9.AddArg(v11);
                v8.AddArg(v9);
                v4.AddArg(v8);
                v0.AddArg(v4);
                v.AddArg(v0);
                var v12 = b.NewValue0(v.Pos, OpSelect1, typ.UInt32);
                var v13 = b.NewValue0(v.Pos, OpMul32uhilo, types.NewTuple(typ.UInt32, typ.UInt32));
                var v14 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v14.AddArg(x);
                v13.AddArg(v14);
                var v15 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v15.AddArg(y);
                v13.AddArg(v15);
                v12.AddArg(v13);
                v.AddArg(v12);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpNeg64_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neg64 <t> x)
            // cond:
            // result: (Sub64 (Const64 <t> [0]) x)
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(OpSub64);
                var v0 = b.NewValue0(v.Pos, OpConst64, t);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpNeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq64 x y)
            // cond:
            // result: (OrB         (Neq32 (Int64Hi x) (Int64Hi y))         (Neq32 (Int64Lo x) (Int64Lo y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpOrB);
                var v0 = b.NewValue0(v.Pos, OpNeq32, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpNeq32, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(y);
                v3.AddArg(v5);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpOr64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Or64 x y)
            // cond:
            // result: (Int64Make         (Or32 <typ.UInt32> (Int64Hi x) (Int64Hi y))         (Or32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(y);
                v3.AddArg(v5);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpRsh16Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Rsh16Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh16Ux32 x lo)
 
            // match: (Rsh16Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh16Ux32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh16Ux32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh16Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh16Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh16Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh16Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh16Ux32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh16x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x64 x (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Signmask (SignExt16to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpSignmask);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh16x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh16x32 x lo)
 
            // match: (Rsh16x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh16x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh16x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh16x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh16x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh16x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh16x32);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh32Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Rsh32Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh32Ux32 x lo)
 
            // match: (Rsh32Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh32Ux32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh32Ux32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh32Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh32Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh32Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh32Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh32Ux32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh32x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x64 x (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Signmask x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpSignmask);
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh32x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh32x32 x lo)
 
            // match: (Rsh32x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh32x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh32x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh32x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh32x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh32x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh32x32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux16 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32Ux16 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux16 <typ.UInt32> lo s)                 (Lsh32x16 <typ.UInt32>                     hi                     (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s)))             (Rsh32Ux16 <typ.UInt32>                 hi                 (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32])))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                var v6 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
                v7.AddArg(hi);
                var v8 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                v8.AddArg(s);
                var v9 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v9.AuxInt = 32L;
                v8.AddArg(v9);
                v7.AddArg(v8);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux32 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32Ux32 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux32 <typ.UInt32> lo s)                 (Lsh32x32 <typ.UInt32>                     hi                     (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s)))             (Rsh32Ux32 <typ.UInt32>                 hi                 (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32])))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                var v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v7.AddArg(hi);
                var v8 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                v8.AddArg(s);
                var v9 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v9.AuxInt = 32L;
                v8.AddArg(v9);
                v7.AddArg(v8);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const64 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst64);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Rsh64Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh64Ux32 x lo)
 
            // match: (Rsh64Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh64Ux32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh64Ux32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh64Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh64Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh64Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh64Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh64Ux32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux8 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32Ux8 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux8 <typ.UInt32> lo s)                 (Lsh32x8 <typ.UInt32>                     hi                     (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s)))             (Rsh32Ux8 <typ.UInt32>                 hi                 (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32])))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                var v6 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
                v7.AddArg(hi);
                var v8 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                v8.AddArg(s);
                var v9 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v9.AuxInt = 32L;
                v8.AddArg(v9);
                v7.AddArg(v8);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x16 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32x16 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux16 <typ.UInt32> lo s)                 (Lsh32x16 <typ.UInt32>                     hi                     (Sub16 <typ.UInt16> (Const16 <typ.UInt16> [32]) s)))             (And32 <typ.UInt32>                 (Rsh32x16 <typ.UInt32>                     hi                     (Sub16 <typ.UInt16> s (Const16 <typ.UInt16> [32])))                 (Zeromask                     (ZeroExt16to32                         (Rsh16Ux32 <typ.UInt16> s (Const32 <typ.UInt32> [5])))))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32x16, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux16, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x16, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                var v6 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v8 = b.NewValue0(v.Pos, OpRsh32x16, typ.UInt32);
                v8.AddArg(hi);
                var v9 = b.NewValue0(v.Pos, OpSub16, typ.UInt16);
                v9.AddArg(s);
                var v10 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                v10.AuxInt = 32L;
                v9.AddArg(v10);
                v8.AddArg(v9);
                v7.AddArg(v8);
                var v11 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                var v12 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                var v13 = b.NewValue0(v.Pos, OpRsh16Ux32, typ.UInt16);
                v13.AddArg(s);
                var v14 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v14.AuxInt = 5L;
                v13.AddArg(v14);
                v12.AddArg(v13);
                v11.AddArg(v12);
                v7.AddArg(v11);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x32 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32x32 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux32 <typ.UInt32> lo s)                 (Lsh32x32 <typ.UInt32>                     hi                     (Sub32 <typ.UInt32> (Const32 <typ.UInt32> [32]) s)))             (And32 <typ.UInt32>                 (Rsh32x32 <typ.UInt32>                     hi                     (Sub32 <typ.UInt32> s (Const32 <typ.UInt32> [32])))                 (Zeromask                     (Rsh32Ux32 <typ.UInt32> s (Const32 <typ.UInt32> [5]))))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32x32, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x32, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                var v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v8 = b.NewValue0(v.Pos, OpRsh32x32, typ.UInt32);
                v8.AddArg(hi);
                var v9 = b.NewValue0(v.Pos, OpSub32, typ.UInt32);
                v9.AddArg(s);
                var v10 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v10.AuxInt = 32L;
                v9.AddArg(v10);
                v8.AddArg(v9);
                v7.AddArg(v8);
                var v11 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                var v12 = b.NewValue0(v.Pos, OpRsh32Ux32, typ.UInt32);
                v12.AddArg(s);
                var v13 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v13.AuxInt = 5L;
                v12.AddArg(v13);
                v11.AddArg(v12);
                v7.AddArg(v11);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x64 x (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Int64Make (Signmask (Int64Hi x)) (Signmask (Int64Hi x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
                var v3 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v3.AddArg(x);
                v2.AddArg(v3);
                v.AddArg(v2);
                return true;
            } 
            // match: (Rsh64x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh64x32 x lo)
 
            // match: (Rsh64x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh64x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh64x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh64x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh64x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh64x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh64x32);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh64x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x8 (Int64Make hi lo) s)
            // cond:
            // result: (Int64Make         (Rsh32x8 <typ.UInt32> hi s)         (Or32 <typ.UInt32>             (Or32 <typ.UInt32>                 (Rsh32Ux8 <typ.UInt32> lo s)                 (Lsh32x8 <typ.UInt32>                     hi                     (Sub8 <typ.UInt8> (Const8 <typ.UInt8> [32]) s)))             (And32 <typ.UInt32>                 (Rsh32x8 <typ.UInt32>                     hi                     (Sub8 <typ.UInt8> s (Const8 <typ.UInt8> [32])))                 (Zeromask                     (ZeroExt8to32                         (Rsh8Ux32 <typ.UInt8> s (Const32 <typ.UInt32> [5])))))))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var hi = v_0.Args[0L];
                var lo = v_0.Args[1L];
                var s = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpRsh32x8, typ.UInt32);
                v0.AddArg(hi);
                v0.AddArg(s);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpRsh32Ux8, typ.UInt32);
                v3.AddArg(lo);
                v3.AddArg(s);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpLsh32x8, typ.UInt32);
                v4.AddArg(hi);
                var v5 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                var v6 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v6.AuxInt = 32L;
                v5.AddArg(v6);
                v5.AddArg(s);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v1.AddArg(v2);
                var v7 = b.NewValue0(v.Pos, OpAnd32, typ.UInt32);
                var v8 = b.NewValue0(v.Pos, OpRsh32x8, typ.UInt32);
                v8.AddArg(hi);
                var v9 = b.NewValue0(v.Pos, OpSub8, typ.UInt8);
                v9.AddArg(s);
                var v10 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                v10.AuxInt = 32L;
                v9.AddArg(v10);
                v8.AddArg(v9);
                v7.AddArg(v8);
                var v11 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                var v12 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                var v13 = b.NewValue0(v.Pos, OpRsh8Ux32, typ.UInt8);
                v13.AddArg(s);
                var v14 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v14.AuxInt = 5L;
                v13.AddArg(v14);
                v12.AddArg(v13);
                v11.AddArg(v12);
                v7.AddArg(v11);
                v1.AddArg(v7);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh8Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux64 _ (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Rsh8Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh8Ux32 x lo)
 
            // match: (Rsh8Ux64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh8Ux32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh8Ux32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh8Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh8Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh8Ux64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh8Ux32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh8Ux32);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpRsh8x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x64 x (Int64Make (Const32 [c]) _))
            // cond: c != 0
            // result: (Signmask (SignExt8to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                var c = v_1_0.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpSignmask);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh8x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh8x32 x lo)
 
            // match: (Rsh8x64 x (Int64Make (Const32 [0]) lo))
            // cond:
            // result: (Rsh8x32 x lo)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                v_1_0 = v_1.Args[0L];
                if (v_1_0.Op != OpConst32)
                {
                    break;
                }
                if (v_1_0.AuxInt != 0L)
                {
                    break;
                }
                var lo = v_1.Args[1L];
                v.reset(OpRsh8x32);
                v.AddArg(x);
                v.AddArg(lo);
                return true;
            } 
            // match: (Rsh8x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
 
            // match: (Rsh8x64 x (Int64Make hi lo))
            // cond: hi.Op != OpConst32
            // result: (Rsh8x32 x (Or32 <typ.UInt32> (Zeromask hi) lo))
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                if (!(hi.Op != OpConst32))
                {
                    break;
                }
                v.reset(OpRsh8x32);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, OpOr32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeromask, typ.UInt32);
                v1.AddArg(hi);
                v0.AddArg(v1);
                v0.AddArg(lo);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpSignExt16to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (SignExt16to64 x)
            // cond:
            // result: (SignExt32to64 (SignExt16to32 x))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpSignExt32to64);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpSignExt32to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (SignExt32to64 x)
            // cond:
            // result: (Int64Make (Signmask x) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpSignExt8to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (SignExt8to64 x)
            // cond:
            // result: (SignExt32to64 (SignExt8to32 x))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpSignExt32to64);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpStore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (Store {t} dst (Int64Make hi lo) mem)
            // cond: t.(*types.Type).Size() == 8 && !config.BigEndian
            // result: (Store {hi.Type}         (OffPtr <hi.Type.PtrTo()> [4] dst)         hi         (Store {lo.Type} dst lo mem))
            while (true)
            {
                var t = v.Aux;
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                var hi = v_1.Args[0L];
                var lo = v_1.Args[1L];
                var mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L && !config.BigEndian))
                {
                    break;
                }
                v.reset(OpStore);
                v.Aux = hi.Type;
                var v0 = b.NewValue0(v.Pos, OpOffPtr, hi.Type.PtrTo());
                v0.AuxInt = 4L;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(hi);
                var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = lo.Type;
                v1.AddArg(dst);
                v1.AddArg(lo);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Store {t} dst (Int64Make hi lo) mem)
            // cond: t.(*types.Type).Size() == 8 && config.BigEndian
            // result: (Store {lo.Type}         (OffPtr <lo.Type.PtrTo()> [4] dst)         lo         (Store {hi.Type} dst hi mem))
 
            // match: (Store {t} dst (Int64Make hi lo) mem)
            // cond: t.(*types.Type).Size() == 8 && config.BigEndian
            // result: (Store {lo.Type}         (OffPtr <lo.Type.PtrTo()> [4] dst)         lo         (Store {hi.Type} dst hi mem))
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_1.Args[1L];
                hi = v_1.Args[0L];
                lo = v_1.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L && config.BigEndian))
                {
                    break;
                }
                v.reset(OpStore);
                v.Aux = lo.Type;
                v0 = b.NewValue0(v.Pos, OpOffPtr, lo.Type.PtrTo());
                v0.AuxInt = 4L;
                v0.AddArg(dst);
                v.AddArg(v0);
                v.AddArg(lo);
                v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
                v1.Aux = hi.Type;
                v1.AddArg(dst);
                v1.AddArg(hi);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpSub64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Sub64 x y)
            // cond:
            // result: (Int64Make         (Sub32withcarry <typ.Int32>             (Int64Hi x)             (Int64Hi y)             (Select1 <types.TypeFlags> (Sub32carry (Int64Lo x) (Int64Lo y))))         (Select0 <typ.UInt32> (Sub32carry (Int64Lo x) (Int64Lo y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpSub32withcarry, typ.Int32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
                var v4 = b.NewValue0(v.Pos, OpSub32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v6.AddArg(y);
                v4.AddArg(v6);
                v3.AddArg(v4);
                v0.AddArg(v3);
                v.AddArg(v0);
                var v7 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
                var v8 = b.NewValue0(v.Pos, OpSub32carry, types.NewTuple(typ.UInt32, types.TypeFlags));
                var v9 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v9.AddArg(x);
                v8.AddArg(v9);
                var v10 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v10.AddArg(y);
                v8.AddArg(v10);
                v7.AddArg(v8);
                v.AddArg(v7);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpTrunc64to16_0(ref Value v)
        { 
            // match: (Trunc64to16 (Int64Make _ lo))
            // cond:
            // result: (Trunc32to16 lo)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var lo = v_0.Args[1L];
                v.reset(OpTrunc32to16);
                v.AddArg(lo);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpTrunc64to32_0(ref Value v)
        { 
            // match: (Trunc64to32 (Int64Make _ lo))
            // cond:
            // result: lo
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var lo = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = lo.Type;
                v.AddArg(lo);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpTrunc64to8_0(ref Value v)
        { 
            // match: (Trunc64to8 (Int64Make _ lo))
            // cond:
            // result: (Trunc32to8 lo)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpInt64Make)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var lo = v_0.Args[1L];
                v.reset(OpTrunc32to8);
                v.AddArg(lo);
                return true;
            }

            return false;
        }
        private static bool rewriteValuedec64_OpXor64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Xor64 x y)
            // cond:
            // result: (Int64Make         (Xor32 <typ.UInt32> (Int64Hi x) (Int64Hi y))         (Xor32 <typ.UInt32> (Int64Lo x) (Int64Lo y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpXor32, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpInt64Hi, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpXor32, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v4.AddArg(x);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpInt64Lo, typ.UInt32);
                v5.AddArg(y);
                v3.AddArg(v5);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpZeroExt16to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (ZeroExt16to64 x)
            // cond:
            // result: (ZeroExt32to64 (ZeroExt16to32 x))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpZeroExt32to64);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpZeroExt32to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (ZeroExt32to64 x)
            // cond:
            // result: (Int64Make (Const32 <typ.UInt32> [0]) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpInt64Make);
                var v0 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValuedec64_OpZeroExt8to64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (ZeroExt8to64 x)
            // cond:
            // result: (ZeroExt32to64 (ZeroExt8to32 x))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpZeroExt32to64);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteBlockdec64(ref Block b)
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
