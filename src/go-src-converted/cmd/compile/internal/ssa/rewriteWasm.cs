// Code generated from gen/Wasm.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 13 06:21:55 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewriteWasm.go
namespace go.cmd.compile.@internal;

using buildcfg = @internal.buildcfg_package;
using math = math_package;
using types = cmd.compile.@internal.types_package;

public static partial class ssa_package {

private static bool rewriteValueWasm(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAbs) 
        v.Op = OpWasmF64Abs;
        return true;
    else if (v.Op == OpAdd16) 
        v.Op = OpWasmI64Add;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = OpWasmI64Add;
        return true;
    else if (v.Op == OpAdd32F) 
        v.Op = OpWasmF32Add;
        return true;
    else if (v.Op == OpAdd64) 
        v.Op = OpWasmI64Add;
        return true;
    else if (v.Op == OpAdd64F) 
        v.Op = OpWasmF64Add;
        return true;
    else if (v.Op == OpAdd8) 
        v.Op = OpWasmI64Add;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = OpWasmI64Add;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValueWasm_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = OpWasmI64And;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = OpWasmI64And;
        return true;
    else if (v.Op == OpAnd64) 
        v.Op = OpWasmI64And;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = OpWasmI64And;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = OpWasmI64And;
        return true;
    else if (v.Op == OpBitLen64) 
        return rewriteValueWasm_OpBitLen64(_addr_v);
    else if (v.Op == OpCeil) 
        v.Op = OpWasmF64Ceil;
        return true;
    else if (v.Op == OpClosureCall) 
        v.Op = OpWasmLoweredClosureCall;
        return true;
    else if (v.Op == OpCom16) 
        return rewriteValueWasm_OpCom16(_addr_v);
    else if (v.Op == OpCom32) 
        return rewriteValueWasm_OpCom32(_addr_v);
    else if (v.Op == OpCom64) 
        return rewriteValueWasm_OpCom64(_addr_v);
    else if (v.Op == OpCom8) 
        return rewriteValueWasm_OpCom8(_addr_v);
    else if (v.Op == OpCondSelect) 
        v.Op = OpWasmSelect;
        return true;
    else if (v.Op == OpConst16) 
        return rewriteValueWasm_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        return rewriteValueWasm_OpConst32(_addr_v);
    else if (v.Op == OpConst32F) 
        v.Op = OpWasmF32Const;
        return true;
    else if (v.Op == OpConst64) 
        v.Op = OpWasmI64Const;
        return true;
    else if (v.Op == OpConst64F) 
        v.Op = OpWasmF64Const;
        return true;
    else if (v.Op == OpConst8) 
        return rewriteValueWasm_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValueWasm_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValueWasm_OpConstNil(_addr_v);
    else if (v.Op == OpConvert) 
        v.Op = OpWasmLoweredConvert;
        return true;
    else if (v.Op == OpCopysign) 
        v.Op = OpWasmF64Copysign;
        return true;
    else if (v.Op == OpCtz16) 
        return rewriteValueWasm_OpCtz16(_addr_v);
    else if (v.Op == OpCtz16NonZero) 
        v.Op = OpWasmI64Ctz;
        return true;
    else if (v.Op == OpCtz32) 
        return rewriteValueWasm_OpCtz32(_addr_v);
    else if (v.Op == OpCtz32NonZero) 
        v.Op = OpWasmI64Ctz;
        return true;
    else if (v.Op == OpCtz64) 
        v.Op = OpWasmI64Ctz;
        return true;
    else if (v.Op == OpCtz64NonZero) 
        v.Op = OpWasmI64Ctz;
        return true;
    else if (v.Op == OpCtz8) 
        return rewriteValueWasm_OpCtz8(_addr_v);
    else if (v.Op == OpCtz8NonZero) 
        v.Op = OpWasmI64Ctz;
        return true;
    else if (v.Op == OpCvt32Fto32) 
        v.Op = OpWasmI64TruncSatF32S;
        return true;
    else if (v.Op == OpCvt32Fto32U) 
        v.Op = OpWasmI64TruncSatF32U;
        return true;
    else if (v.Op == OpCvt32Fto64) 
        v.Op = OpWasmI64TruncSatF32S;
        return true;
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = OpWasmF64PromoteF32;
        return true;
    else if (v.Op == OpCvt32Fto64U) 
        v.Op = OpWasmI64TruncSatF32U;
        return true;
    else if (v.Op == OpCvt32Uto32F) 
        return rewriteValueWasm_OpCvt32Uto32F(_addr_v);
    else if (v.Op == OpCvt32Uto64F) 
        return rewriteValueWasm_OpCvt32Uto64F(_addr_v);
    else if (v.Op == OpCvt32to32F) 
        return rewriteValueWasm_OpCvt32to32F(_addr_v);
    else if (v.Op == OpCvt32to64F) 
        return rewriteValueWasm_OpCvt32to64F(_addr_v);
    else if (v.Op == OpCvt64Fto32) 
        v.Op = OpWasmI64TruncSatF64S;
        return true;
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = OpWasmF32DemoteF64;
        return true;
    else if (v.Op == OpCvt64Fto32U) 
        v.Op = OpWasmI64TruncSatF64U;
        return true;
    else if (v.Op == OpCvt64Fto64) 
        v.Op = OpWasmI64TruncSatF64S;
        return true;
    else if (v.Op == OpCvt64Fto64U) 
        v.Op = OpWasmI64TruncSatF64U;
        return true;
    else if (v.Op == OpCvt64Uto32F) 
        v.Op = OpWasmF32ConvertI64U;
        return true;
    else if (v.Op == OpCvt64Uto64F) 
        v.Op = OpWasmF64ConvertI64U;
        return true;
    else if (v.Op == OpCvt64to32F) 
        v.Op = OpWasmF32ConvertI64S;
        return true;
    else if (v.Op == OpCvt64to64F) 
        v.Op = OpWasmF64ConvertI64S;
        return true;
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        return rewriteValueWasm_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValueWasm_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValueWasm_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        v.Op = OpWasmF32Div;
        return true;
    else if (v.Op == OpDiv32u) 
        return rewriteValueWasm_OpDiv32u(_addr_v);
    else if (v.Op == OpDiv64) 
        return rewriteValueWasm_OpDiv64(_addr_v);
    else if (v.Op == OpDiv64F) 
        v.Op = OpWasmF64Div;
        return true;
    else if (v.Op == OpDiv64u) 
        v.Op = OpWasmI64DivU;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValueWasm_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValueWasm_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValueWasm_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValueWasm_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        v.Op = OpWasmF32Eq;
        return true;
    else if (v.Op == OpEq64) 
        v.Op = OpWasmI64Eq;
        return true;
    else if (v.Op == OpEq64F) 
        v.Op = OpWasmF64Eq;
        return true;
    else if (v.Op == OpEq8) 
        return rewriteValueWasm_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        v.Op = OpWasmI64Eq;
        return true;
    else if (v.Op == OpEqPtr) 
        v.Op = OpWasmI64Eq;
        return true;
    else if (v.Op == OpFloor) 
        v.Op = OpWasmF64Floor;
        return true;
    else if (v.Op == OpGetCallerPC) 
        v.Op = OpWasmLoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = OpWasmLoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = OpWasmLoweredGetClosurePtr;
        return true;
    else if (v.Op == OpInterCall) 
        v.Op = OpWasmLoweredInterCall;
        return true;
    else if (v.Op == OpIsInBounds) 
        v.Op = OpWasmI64LtU;
        return true;
    else if (v.Op == OpIsNonNil) 
        return rewriteValueWasm_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        v.Op = OpWasmI64LeU;
        return true;
    else if (v.Op == OpLeq16) 
        return rewriteValueWasm_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValueWasm_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValueWasm_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        v.Op = OpWasmF32Le;
        return true;
    else if (v.Op == OpLeq32U) 
        return rewriteValueWasm_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64) 
        v.Op = OpWasmI64LeS;
        return true;
    else if (v.Op == OpLeq64F) 
        v.Op = OpWasmF64Le;
        return true;
    else if (v.Op == OpLeq64U) 
        v.Op = OpWasmI64LeU;
        return true;
    else if (v.Op == OpLeq8) 
        return rewriteValueWasm_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValueWasm_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValueWasm_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValueWasm_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValueWasm_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        v.Op = OpWasmF32Lt;
        return true;
    else if (v.Op == OpLess32U) 
        return rewriteValueWasm_OpLess32U(_addr_v);
    else if (v.Op == OpLess64) 
        v.Op = OpWasmI64LtS;
        return true;
    else if (v.Op == OpLess64F) 
        v.Op = OpWasmF64Lt;
        return true;
    else if (v.Op == OpLess64U) 
        v.Op = OpWasmI64LtU;
        return true;
    else if (v.Op == OpLess8) 
        return rewriteValueWasm_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValueWasm_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValueWasm_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValueWasm_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValueWasm_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValueWasm_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        v.Op = OpLsh64x64;
        return true;
    else if (v.Op == OpLsh16x8) 
        return rewriteValueWasm_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValueWasm_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValueWasm_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        v.Op = OpLsh64x64;
        return true;
    else if (v.Op == OpLsh32x8) 
        return rewriteValueWasm_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh64x16) 
        return rewriteValueWasm_OpLsh64x16(_addr_v);
    else if (v.Op == OpLsh64x32) 
        return rewriteValueWasm_OpLsh64x32(_addr_v);
    else if (v.Op == OpLsh64x64) 
        return rewriteValueWasm_OpLsh64x64(_addr_v);
    else if (v.Op == OpLsh64x8) 
        return rewriteValueWasm_OpLsh64x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValueWasm_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValueWasm_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        v.Op = OpLsh64x64;
        return true;
    else if (v.Op == OpLsh8x8) 
        return rewriteValueWasm_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValueWasm_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValueWasm_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValueWasm_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValueWasm_OpMod32u(_addr_v);
    else if (v.Op == OpMod64) 
        return rewriteValueWasm_OpMod64(_addr_v);
    else if (v.Op == OpMod64u) 
        v.Op = OpWasmI64RemU;
        return true;
    else if (v.Op == OpMod8) 
        return rewriteValueWasm_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValueWasm_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValueWasm_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = OpWasmI64Mul;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = OpWasmI64Mul;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = OpWasmF32Mul;
        return true;
    else if (v.Op == OpMul64) 
        v.Op = OpWasmI64Mul;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = OpWasmF64Mul;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = OpWasmI64Mul;
        return true;
    else if (v.Op == OpNeg16) 
        return rewriteValueWasm_OpNeg16(_addr_v);
    else if (v.Op == OpNeg32) 
        return rewriteValueWasm_OpNeg32(_addr_v);
    else if (v.Op == OpNeg32F) 
        v.Op = OpWasmF32Neg;
        return true;
    else if (v.Op == OpNeg64) 
        return rewriteValueWasm_OpNeg64(_addr_v);
    else if (v.Op == OpNeg64F) 
        v.Op = OpWasmF64Neg;
        return true;
    else if (v.Op == OpNeg8) 
        return rewriteValueWasm_OpNeg8(_addr_v);
    else if (v.Op == OpNeq16) 
        return rewriteValueWasm_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValueWasm_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        v.Op = OpWasmF32Ne;
        return true;
    else if (v.Op == OpNeq64) 
        v.Op = OpWasmI64Ne;
        return true;
    else if (v.Op == OpNeq64F) 
        v.Op = OpWasmF64Ne;
        return true;
    else if (v.Op == OpNeq8) 
        return rewriteValueWasm_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        v.Op = OpWasmI64Ne;
        return true;
    else if (v.Op == OpNeqPtr) 
        v.Op = OpWasmI64Ne;
        return true;
    else if (v.Op == OpNilCheck) 
        v.Op = OpWasmLoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        v.Op = OpWasmI64Eqz;
        return true;
    else if (v.Op == OpOffPtr) 
        v.Op = OpWasmI64AddConst;
        return true;
    else if (v.Op == OpOr16) 
        v.Op = OpWasmI64Or;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = OpWasmI64Or;
        return true;
    else if (v.Op == OpOr64) 
        v.Op = OpWasmI64Or;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = OpWasmI64Or;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = OpWasmI64Or;
        return true;
    else if (v.Op == OpPopCount16) 
        return rewriteValueWasm_OpPopCount16(_addr_v);
    else if (v.Op == OpPopCount32) 
        return rewriteValueWasm_OpPopCount32(_addr_v);
    else if (v.Op == OpPopCount64) 
        v.Op = OpWasmI64Popcnt;
        return true;
    else if (v.Op == OpPopCount8) 
        return rewriteValueWasm_OpPopCount8(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValueWasm_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        v.Op = OpWasmI32Rotl;
        return true;
    else if (v.Op == OpRotateLeft64) 
        v.Op = OpWasmI64Rotl;
        return true;
    else if (v.Op == OpRotateLeft8) 
        return rewriteValueWasm_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound32F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRoundToEven) 
        v.Op = OpWasmF64Nearest;
        return true;
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValueWasm_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValueWasm_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValueWasm_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValueWasm_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValueWasm_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValueWasm_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValueWasm_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValueWasm_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValueWasm_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValueWasm_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValueWasm_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValueWasm_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValueWasm_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValueWasm_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValueWasm_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValueWasm_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh64Ux16) 
        return rewriteValueWasm_OpRsh64Ux16(_addr_v);
    else if (v.Op == OpRsh64Ux32) 
        return rewriteValueWasm_OpRsh64Ux32(_addr_v);
    else if (v.Op == OpRsh64Ux64) 
        return rewriteValueWasm_OpRsh64Ux64(_addr_v);
    else if (v.Op == OpRsh64Ux8) 
        return rewriteValueWasm_OpRsh64Ux8(_addr_v);
    else if (v.Op == OpRsh64x16) 
        return rewriteValueWasm_OpRsh64x16(_addr_v);
    else if (v.Op == OpRsh64x32) 
        return rewriteValueWasm_OpRsh64x32(_addr_v);
    else if (v.Op == OpRsh64x64) 
        return rewriteValueWasm_OpRsh64x64(_addr_v);
    else if (v.Op == OpRsh64x8) 
        return rewriteValueWasm_OpRsh64x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValueWasm_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValueWasm_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValueWasm_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValueWasm_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValueWasm_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValueWasm_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValueWasm_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValueWasm_OpRsh8x8(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        return rewriteValueWasm_OpSignExt16to32(_addr_v);
    else if (v.Op == OpSignExt16to64) 
        return rewriteValueWasm_OpSignExt16to64(_addr_v);
    else if (v.Op == OpSignExt32to64) 
        return rewriteValueWasm_OpSignExt32to64(_addr_v);
    else if (v.Op == OpSignExt8to16) 
        return rewriteValueWasm_OpSignExt8to16(_addr_v);
    else if (v.Op == OpSignExt8to32) 
        return rewriteValueWasm_OpSignExt8to32(_addr_v);
    else if (v.Op == OpSignExt8to64) 
        return rewriteValueWasm_OpSignExt8to64(_addr_v);
    else if (v.Op == OpSlicemask) 
        return rewriteValueWasm_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = OpWasmF64Sqrt;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = OpWasmF32Sqrt;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = OpWasmLoweredStaticCall;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValueWasm_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = OpWasmI64Sub;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = OpWasmI64Sub;
        return true;
    else if (v.Op == OpSub32F) 
        v.Op = OpWasmF32Sub;
        return true;
    else if (v.Op == OpSub64) 
        v.Op = OpWasmI64Sub;
        return true;
    else if (v.Op == OpSub64F) 
        v.Op = OpWasmF64Sub;
        return true;
    else if (v.Op == OpSub8) 
        v.Op = OpWasmI64Sub;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = OpWasmI64Sub;
        return true;
    else if (v.Op == OpTrunc) 
        v.Op = OpWasmF64Trunc;
        return true;
    else if (v.Op == OpTrunc16to8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpTrunc32to16) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpTrunc32to8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpTrunc64to16) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpTrunc64to32) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpTrunc64to8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpWB) 
        v.Op = OpWasmLoweredWB;
        return true;
    else if (v.Op == OpWasmF64Add) 
        return rewriteValueWasm_OpWasmF64Add(_addr_v);
    else if (v.Op == OpWasmF64Mul) 
        return rewriteValueWasm_OpWasmF64Mul(_addr_v);
    else if (v.Op == OpWasmI64Add) 
        return rewriteValueWasm_OpWasmI64Add(_addr_v);
    else if (v.Op == OpWasmI64AddConst) 
        return rewriteValueWasm_OpWasmI64AddConst(_addr_v);
    else if (v.Op == OpWasmI64And) 
        return rewriteValueWasm_OpWasmI64And(_addr_v);
    else if (v.Op == OpWasmI64Eq) 
        return rewriteValueWasm_OpWasmI64Eq(_addr_v);
    else if (v.Op == OpWasmI64Eqz) 
        return rewriteValueWasm_OpWasmI64Eqz(_addr_v);
    else if (v.Op == OpWasmI64LeU) 
        return rewriteValueWasm_OpWasmI64LeU(_addr_v);
    else if (v.Op == OpWasmI64Load) 
        return rewriteValueWasm_OpWasmI64Load(_addr_v);
    else if (v.Op == OpWasmI64Load16S) 
        return rewriteValueWasm_OpWasmI64Load16S(_addr_v);
    else if (v.Op == OpWasmI64Load16U) 
        return rewriteValueWasm_OpWasmI64Load16U(_addr_v);
    else if (v.Op == OpWasmI64Load32S) 
        return rewriteValueWasm_OpWasmI64Load32S(_addr_v);
    else if (v.Op == OpWasmI64Load32U) 
        return rewriteValueWasm_OpWasmI64Load32U(_addr_v);
    else if (v.Op == OpWasmI64Load8S) 
        return rewriteValueWasm_OpWasmI64Load8S(_addr_v);
    else if (v.Op == OpWasmI64Load8U) 
        return rewriteValueWasm_OpWasmI64Load8U(_addr_v);
    else if (v.Op == OpWasmI64LtU) 
        return rewriteValueWasm_OpWasmI64LtU(_addr_v);
    else if (v.Op == OpWasmI64Mul) 
        return rewriteValueWasm_OpWasmI64Mul(_addr_v);
    else if (v.Op == OpWasmI64Ne) 
        return rewriteValueWasm_OpWasmI64Ne(_addr_v);
    else if (v.Op == OpWasmI64Or) 
        return rewriteValueWasm_OpWasmI64Or(_addr_v);
    else if (v.Op == OpWasmI64Shl) 
        return rewriteValueWasm_OpWasmI64Shl(_addr_v);
    else if (v.Op == OpWasmI64ShrS) 
        return rewriteValueWasm_OpWasmI64ShrS(_addr_v);
    else if (v.Op == OpWasmI64ShrU) 
        return rewriteValueWasm_OpWasmI64ShrU(_addr_v);
    else if (v.Op == OpWasmI64Store) 
        return rewriteValueWasm_OpWasmI64Store(_addr_v);
    else if (v.Op == OpWasmI64Store16) 
        return rewriteValueWasm_OpWasmI64Store16(_addr_v);
    else if (v.Op == OpWasmI64Store32) 
        return rewriteValueWasm_OpWasmI64Store32(_addr_v);
    else if (v.Op == OpWasmI64Store8) 
        return rewriteValueWasm_OpWasmI64Store8(_addr_v);
    else if (v.Op == OpWasmI64Xor) 
        return rewriteValueWasm_OpWasmI64Xor(_addr_v);
    else if (v.Op == OpXor16) 
        v.Op = OpWasmI64Xor;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = OpWasmI64Xor;
        return true;
    else if (v.Op == OpXor64) 
        v.Op = OpWasmI64Xor;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = OpWasmI64Xor;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValueWasm_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        return rewriteValueWasm_OpZeroExt16to32(_addr_v);
    else if (v.Op == OpZeroExt16to64) 
        return rewriteValueWasm_OpZeroExt16to64(_addr_v);
    else if (v.Op == OpZeroExt32to64) 
        return rewriteValueWasm_OpZeroExt32to64(_addr_v);
    else if (v.Op == OpZeroExt8to16) 
        return rewriteValueWasm_OpZeroExt8to16(_addr_v);
    else if (v.Op == OpZeroExt8to32) 
        return rewriteValueWasm_OpZeroExt8to32(_addr_v);
    else if (v.Op == OpZeroExt8to64) 
        return rewriteValueWasm_OpZeroExt8to64(_addr_v);
        return false;
}
private static bool rewriteValueWasm_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (LoweredAddr {sym} [0] base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpWasmLoweredAddr);
        v.AuxInt = int32ToAuxInt(0);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValueWasm_OpBitLen64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen64 x)
    // result: (I64Sub (I64Const [64]) (I64Clz x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Sub);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Clz, typ.Int64);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpCom16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Com16 x)
    // result: (I64Xor x (I64Const [-1]))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Xor);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(-1);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCom32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Com32 x)
    // result: (I64Xor x (I64Const [-1]))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Xor);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(-1);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCom64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Com64 x)
    // result: (I64Xor x (I64Const [-1]))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Xor);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(-1);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCom8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Com8 x)
    // result: (I64Xor x (I64Const [-1]))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Xor);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(-1);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [c])
    // result: (I64Const [int64(c)])
    while (true) {
        var c = auxIntToInt16(v.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;
    }
}
private static bool rewriteValueWasm_OpConst32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32 [c])
    // result: (I64Const [int64(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;
    }
}
private static bool rewriteValueWasm_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [c])
    // result: (I64Const [int64(c)])
    while (true) {
        var c = auxIntToInt8(v.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;
    }
}
private static bool rewriteValueWasm_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [c])
    // result: (I64Const [b2i(c)])
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(b2i(c));
        return true;
    }
}
private static bool rewriteValueWasm_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (I64Const [0])
    while (true) {
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCtz16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz16 x)
    // result: (I64Ctz (I64Or x (I64Const [0x10000])))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Ctz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Or, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0x10000);
        v0.AddArg2(x, v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz32 x)
    // result: (I64Ctz (I64Or x (I64Const [0x100000000])))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Ctz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Or, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0x100000000);
        v0.AddArg2(x, v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCtz8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz8 x)
    // result: (I64Ctz (I64Or x (I64Const [0x100])))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Ctz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Or, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0x100);
        v0.AddArg2(x, v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCvt32Uto32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32Uto32F x)
    // result: (F32ConvertI64U (ZeroExt32to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmF32ConvertI64U);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCvt32Uto64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32Uto64F x)
    // result: (F64ConvertI64U (ZeroExt32to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmF64ConvertI64U);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCvt32to32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32to32F x)
    // result: (F32ConvertI64S (SignExt32to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmF32ConvertI64S);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpCvt32to64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32to64F x)
    // result: (F64ConvertI64S (SignExt32to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmF64ConvertI64S);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 [false] x y)
    // result: (I64DivS (SignExt16to64 x) (SignExt16to64 y))
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivS);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u x y)
    // result: (I64DivU (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32 [false] x y)
    // result: (I64DivS (SignExt32to64 x) (SignExt32to64 y))
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivS);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpDiv32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32u x y)
    // result: (I64DivU (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpDiv64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Div64 [false] x y)
    // result: (I64DivS x y)
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivS);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (I64DivS (SignExt8to64 x) (SignExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivS);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (I64DivU (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64DivU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x y)
    // result: (I64Eq (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Eq);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq32 x y)
    // result: (I64Eq (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Eq);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x y)
    // result: (I64Eq (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Eq);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsNonNil p)
    // result: (I64Eqz (I64Eqz p))
    while (true) {
        var p = v_0;
        v.reset(OpWasmI64Eqz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Eqz, typ.Bool);
        v0.AddArg(p);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16 x y)
    // result: (I64LeS (SignExt16to64 x) (SignExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeS);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16U x y)
    // result: (I64LeU (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32 x y)
    // result: (I64LeS (SignExt32to64 x) (SignExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeS);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32U x y)
    // result: (I64LeU (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8 x y)
    // result: (I64LeS (SignExt8to64 x) (SignExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeS);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8U x y)
    // result: (I64LeU (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LeU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16 x y)
    // result: (I64LtS (SignExt16to64 x) (SignExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtS);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16U x y)
    // result: (I64LtU (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less32 x y)
    // result: (I64LtS (SignExt32to64 x) (SignExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtS);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less32U x y)
    // result: (I64LtU (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8 x y)
    // result: (I64LtS (SignExt8to64 x) (SignExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtS);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8U x y)
    // result: (I64LtU (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64LtU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Load <t> ptr mem)
    // cond: is32BitFloat(t)
    // result: (F32Load ptr mem)
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(OpWasmF32Load);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitFloat(t)
    // result: (F64Load ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(OpWasmF64Load);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 8
    // result: (I64Load ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 8)) {
            break;
        }
        v.reset(OpWasmI64Load);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 4 && !t.IsSigned()
    // result: (I64Load32U ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 4 && !t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load32U);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 4 && t.IsSigned()
    // result: (I64Load32S ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 4 && t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load32S);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 2 && !t.IsSigned()
    // result: (I64Load16U ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 2 && !t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load16U);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 2 && t.IsSigned()
    // result: (I64Load16S ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 2 && t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load16S);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 1 && !t.IsSigned()
    // result: (I64Load8U ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 1 && !t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load8U);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.Size() == 1 && t.IsSigned()
    // result: (I64Load8S ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.Size() == 1 && t.IsSigned())) {
            break;
        }
        v.reset(OpWasmI64Load8S);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (LoweredAddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpWasmLoweredAddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x16 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x32 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x8 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x16 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x32 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x8 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x16 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x32 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (I64Shl x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpWasmI64Shl);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x64 x (I64Const [c]))
    // cond: uint64(c) < 64
    // result: (I64Shl x (I64Const [c]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpWasmI64Shl);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(c);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Lsh64x64 x (I64Const [c]))
    // cond: uint64(c) >= 64
    // result: (I64Const [0])
    while (true) {
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Lsh64x64 x y)
    // result: (Select (I64Shl x y) (I64Const [0]) (I64LtU y (I64Const [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpWasmSelect);
        v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpWasmI64LtU, typ.Bool);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x8 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x16 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x32 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x8 [c] x y)
    // result: (Lsh64x64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpLsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16 [false] x y)
    // result: (I64RemS (SignExt16to64 x) (SignExt16to64 y))
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemS);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16u x y)
    // result: (I64RemU (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32 [false] x y)
    // result: (I64RemS (SignExt32to64 x) (SignExt32to64 y))
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemS);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32u x y)
    // result: (I64RemU (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpMod64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Mod64 [false] x y)
    // result: (I64RemS x y)
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemS);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (I64RemS (SignExt8to64 x) (SignExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemS);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (I64RemU (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64RemU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpMove(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Move [0] _ _ mem)
    // result: mem
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var mem = v_2;
        v.copyOf(mem);
        return true;
    } 
    // match: (Move [1] dst src mem)
    // result: (I64Store8 dst (I64Load8U src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var dst = v_0;
        var src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store8);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Load8U, typ.UInt8);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [2] dst src mem)
    // result: (I64Store16 dst (I64Load16U src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store16);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load16U, typ.UInt16);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [4] dst src mem)
    // result: (I64Store32 dst (I64Load32U src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store32);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load32U, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [8] dst src mem)
    // result: (I64Store dst (I64Load src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [16] dst src mem)
    // result: (I64Store [8] dst (I64Load [8] src mem) (I64Store dst (I64Load src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(8);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [3] dst src mem)
    // result: (I64Store8 [2] dst (I64Load8U [2] src mem) (I64Store16 dst (I64Load16U src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store8);
        v.AuxInt = int64ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load8U, typ.UInt8);
        v0.AuxInt = int64ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store16, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpWasmI64Load16U, typ.UInt16);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [5] dst src mem)
    // result: (I64Store8 [4] dst (I64Load8U [4] src mem) (I64Store32 dst (I64Load32U src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store8);
        v.AuxInt = int64ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load8U, typ.UInt8);
        v0.AuxInt = int64ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpWasmI64Load32U, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [6] dst src mem)
    // result: (I64Store16 [4] dst (I64Load16U [4] src mem) (I64Store32 dst (I64Load32U src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store16);
        v.AuxInt = int64ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load16U, typ.UInt16);
        v0.AuxInt = int64ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpWasmI64Load32U, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [7] dst src mem)
    // result: (I64Store32 [3] dst (I64Load32U [3] src mem) (I64Store32 dst (I64Load32U src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpWasmI64Store32);
        v.AuxInt = int64ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load32U, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(3);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpWasmI64Load32U, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && s < 16
    // result: (I64Store [s-8] dst (I64Load [s-8] src mem) (I64Store dst (I64Load src mem) mem))
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && s < 16)) {
            break;
        }
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(s - 8);
        v0 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(s - 8);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 16 && s%16 != 0 && s%16 <= 8
    // result: (Move [s-s%16] (OffPtr <dst.Type> dst [s%16]) (OffPtr <src.Type> src [s%16]) (I64Store dst (I64Load src mem) mem))
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 16 && s % 16 != 0 && s % 16 <= 8)) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(s - s % 16);
        v0 = b.NewValue0(v.Pos, OpOffPtr, dst.Type);
        v0.AuxInt = int64ToAuxInt(s % 16);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpOffPtr, src.Type);
        v1.AuxInt = int64ToAuxInt(s % 16);
        v1.AddArg(src);
        v2 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v3.AddArg2(src, mem);
        v2.AddArg3(dst, v3, mem);
        v.AddArg3(v0, v1, v2);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 16 && s%16 != 0 && s%16 > 8
    // result: (Move [s-s%16] (OffPtr <dst.Type> dst [s%16]) (OffPtr <src.Type> src [s%16]) (I64Store [8] dst (I64Load [8] src mem) (I64Store dst (I64Load src mem) mem)))
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 16 && s % 16 != 0 && s % 16 > 8)) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(s - s % 16);
        v0 = b.NewValue0(v.Pos, OpOffPtr, dst.Type);
        v0.AuxInt = int64ToAuxInt(s % 16);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpOffPtr, src.Type);
        v1.AuxInt = int64ToAuxInt(s % 16);
        v1.AddArg(src);
        v2 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v2.AuxInt = int64ToAuxInt(8);
        v3 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(src, mem);
        var v4 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        var v5 = b.NewValue0(v.Pos, OpWasmI64Load, typ.UInt64);
        v5.AddArg2(src, mem);
        v4.AddArg3(dst, v5, mem);
        v2.AddArg3(dst, v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s%8 == 0 && logLargeCopy(v, s)
    // result: (LoweredMove [s/8] dst src mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s % 8 == 0 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpWasmLoweredMove);
        v.AuxInt = int64ToAuxInt(s / 8);
        v.AddArg3(dst, src, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpNeg16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg16 x)
    // result: (I64Sub (I64Const [0]) x)
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Sub);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeg32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg32 x)
    // result: (I64Sub (I64Const [0]) x)
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Sub);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeg64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg64 x)
    // result: (I64Sub (I64Const [0]) x)
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Sub);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeg8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg8 x)
    // result: (I64Sub (I64Const [0]) x)
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Sub);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x y)
    // result: (I64Ne (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Ne);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq32 x y)
    // result: (I64Ne (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Ne);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x y)
    // result: (I64Ne (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpWasmI64Ne);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpPopCount16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount16 x)
    // result: (I64Popcnt (ZeroExt16to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Popcnt);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpPopCount32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount32 x)
    // result: (I64Popcnt (ZeroExt32to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Popcnt);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpPopCount8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount8 x)
    // result: (I64Popcnt (ZeroExt8to64 x))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64Popcnt);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft16 <t> x (I64Const [c]))
    // result: (Or16 (Lsh16x64 <t> x (I64Const [c&15])) (Rsh16Ux64 <t> x (I64Const [-c&15])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr16);
        var v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(c & 15);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v3.AuxInt = int64ToAuxInt(-c & 15);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft8 <t> x (I64Const [c]))
    // result: (Or8 (Lsh8x64 <t> x (I64Const [c&7])) (Rsh8Ux64 <t> x (I64Const [-c&7])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr8);
        var v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(c & 7);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v3.AuxInt = int64ToAuxInt(-c & 7);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux16 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux32 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt16to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt16to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux8 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt16to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x16 [c] x y)
    // result: (Rsh64x64 [c] (SignExt16to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x32 [c] x y)
    // result: (Rsh64x64 [c] (SignExt16to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 [c] x y)
    // result: (Rsh64x64 [c] (SignExt16to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x8 [c] x y)
    // result: (Rsh64x64 [c] (SignExt16to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux16 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt32to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux32 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux64 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt32to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux8 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt32to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x16 [c] x y)
    // result: (Rsh64x64 [c] (SignExt32to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x32 [c] x y)
    // result: (Rsh64x64 [c] (SignExt32to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x64 [c] x y)
    // result: (Rsh64x64 [c] (SignExt32to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x8 [c] x y)
    // result: (Rsh64x64 [c] (SignExt32to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux16 [c] x y)
    // result: (Rsh64Ux64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux32 [c] x y)
    // result: (Rsh64Ux64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (I64ShrU x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpWasmI64ShrU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux64 x (I64Const [c]))
    // cond: uint64(c) < 64
    // result: (I64ShrU x (I64Const [c]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpWasmI64ShrU);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(c);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64Ux64 x (I64Const [c]))
    // cond: uint64(c) >= 64
    // result: (I64Const [0])
    while (true) {
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Rsh64Ux64 x y)
    // result: (Select (I64ShrU x y) (I64Const [0]) (I64LtU y (I64Const [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpWasmSelect);
        v0 = b.NewValue0(v.Pos, OpWasmI64ShrU, typ.Int64);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpWasmI64LtU, typ.Bool);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux8 [c] x y)
    // result: (Rsh64Ux64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x16 [c] x y)
    // result: (Rsh64x64 [c] x (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x32 [c] x y)
    // result: (Rsh64x64 [c] x (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (I64ShrS x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpWasmI64ShrS);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x64 x (I64Const [c]))
    // cond: uint64(c) < 64
    // result: (I64ShrS x (I64Const [c]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(c);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64x64 x (I64Const [c]))
    // cond: uint64(c) >= 64
    // result: (I64ShrS x (I64Const [63]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpWasmI64ShrS);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(63);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64x64 x y)
    // result: (I64ShrS x (Select <typ.Int64> y (I64Const [63]) (I64LtU y (I64Const [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpWasmI64ShrS);
        v0 = b.NewValue0(v.Pos, OpWasmSelect, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpWasmI64LtU, typ.Bool);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x8 [c] x y)
    // result: (Rsh64x64 [c] x (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux16 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt8to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux32 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt8to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt8to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux8 [c] x y)
    // result: (Rsh64Ux64 [c] (ZeroExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64Ux64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x16 [c] x y)
    // result: (Rsh64x64 [c] (SignExt8to64 x) (ZeroExt16to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x32 [c] x y)
    // result: (Rsh64x64 [c] (SignExt8to64 x) (ZeroExt32to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 [c] x y)
    // result: (Rsh64x64 [c] (SignExt8to64 x) y)
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueWasm_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x8 [c] x y)
    // result: (Rsh64x64 [c] (SignExt8to64 x) (ZeroExt8to64 y))
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        var x = v_0;
        var y = v_1;
        v.reset(OpRsh64x64);
        v.AuxInt = boolToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt16to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt16to32 x:(I64Load16S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load16S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt16to32 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend16S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend16S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt16to32 x)
    // result: (I64ShrS (I64Shl x (I64Const [48])) (I64Const [48]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(48);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt16to64 x:(I64Load16S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load16S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt16to64 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend16S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend16S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt16to64 x)
    // result: (I64ShrS (I64Shl x (I64Const [48])) (I64Const [48]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(48);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt32to64 x:(I64Load32S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load32S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt32to64 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend32S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend32S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt32to64 x)
    // result: (I64ShrS (I64Shl x (I64Const [32])) (I64Const [32]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(32);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt8to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt8to16 x:(I64Load8S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt8to16 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend8S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend8S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt8to16 x)
    // result: (I64ShrS (I64Shl x (I64Const [56])) (I64Const [56]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(56);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt8to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt8to32 x:(I64Load8S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt8to32 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend8S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend8S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt8to32 x)
    // result: (I64ShrS (I64Shl x (I64Const [56])) (I64Const [56]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(56);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSignExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SignExt8to64 x:(I64Load8S _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8S) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SignExt8to64 x)
    // cond: buildcfg.GOWASM.SignExt
    // result: (I64Extend8S x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOWASM.SignExt)) {
            break;
        }
        v.reset(OpWasmI64Extend8S);
        v.AddArg(x);
        return true;
    } 
    // match: (SignExt8to64 x)
    // result: (I64ShrS (I64Shl x (I64Const [56])) (I64Const [56]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Shl, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(56);
        v0.AddArg2(x, v1);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueWasm_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Slicemask x)
    // result: (I64ShrS (I64Sub (I64Const [0]) x) (I64Const [63]))
    while (true) {
        var x = v_0;
        v.reset(OpWasmI64ShrS);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Sub, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v1.AuxInt = int64ToAuxInt(0);
        v0.AddArg2(v1, x);
        var v2 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v2.AuxInt = int64ToAuxInt(63);
        v.AddArg2(v0, v2);
        return true;
    }
}
private static bool rewriteValueWasm_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Store {t} ptr val mem)
    // cond: is64BitFloat(t)
    // result: (F64Store ptr val mem)
    while (true) {
        var t = auxToType(v.Aux);
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(OpWasmF64Store);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: is32BitFloat(t)
    // result: (F32Store ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(OpWasmF32Store);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8
    // result: (I64Store ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8)) {
            break;
        }
        v.reset(OpWasmI64Store);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4
    // result: (I64Store32 ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4)) {
            break;
        }
        v.reset(OpWasmI64Store32);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 2
    // result: (I64Store16 ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 2)) {
            break;
        }
        v.reset(OpWasmI64Store16);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 1
    // result: (I64Store8 ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 1)) {
            break;
        }
        v.reset(OpWasmI64Store8);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmF64Add(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (F64Add (F64Const [x]) (F64Const [y]))
    // result: (F64Const [x + y])
    while (true) {
        if (v_0.Op != OpWasmF64Const) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpWasmF64Const) {
            break;
        }
        var y = auxIntToFloat64(v_1.AuxInt);
        v.reset(OpWasmF64Const);
        v.AuxInt = float64ToAuxInt(x + y);
        return true;
    } 
    // match: (F64Add (F64Const [x]) y)
    // cond: y.Op != OpWasmF64Const
    // result: (F64Add y (F64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmF64Const) {
            break;
        }
        x = auxIntToFloat64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmF64Const)) {
            break;
        }
        v.reset(OpWasmF64Add);
        var v0 = b.NewValue0(v.Pos, OpWasmF64Const, typ.Float64);
        v0.AuxInt = float64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmF64Mul(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (F64Mul (F64Const [x]) (F64Const [y]))
    // cond: !math.IsNaN(x * y)
    // result: (F64Const [x * y])
    while (true) {
        if (v_0.Op != OpWasmF64Const) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpWasmF64Const) {
            break;
        }
        var y = auxIntToFloat64(v_1.AuxInt);
        if (!(!math.IsNaN(x * y))) {
            break;
        }
        v.reset(OpWasmF64Const);
        v.AuxInt = float64ToAuxInt(x * y);
        return true;
    } 
    // match: (F64Mul (F64Const [x]) y)
    // cond: y.Op != OpWasmF64Const
    // result: (F64Mul y (F64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmF64Const) {
            break;
        }
        x = auxIntToFloat64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmF64Const)) {
            break;
        }
        v.reset(OpWasmF64Mul);
        var v0 = b.NewValue0(v.Pos, OpWasmF64Const, typ.Float64);
        v0.AuxInt = float64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Add(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Add (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x + y])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x + y);
        return true;
    } 
    // match: (I64Add (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Add y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Add);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    } 
    // match: (I64Add x (I64Const [y]))
    // result: (I64AddConst [y] x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64AddConst);
        v.AuxInt = int64ToAuxInt(y);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64AddConst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (I64AddConst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (I64AddConst [off] (LoweredAddr {sym} [off2] base))
    // cond: isU32Bit(off+int64(off2))
    // result: (LoweredAddr {sym} [int32(off)+off2] base)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmLoweredAddr) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        if (!(isU32Bit(off + int64(off2)))) {
            break;
        }
        v.reset(OpWasmLoweredAddr);
        v.AuxInt = int32ToAuxInt(int32(off) + off2);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    } 
    // match: (I64AddConst [off] x:(SP))
    // cond: isU32Bit(off)
    // result: (LoweredAddr [int32(off)] x)
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        x = v_0;
        if (x.Op != OpSP || !(isU32Bit(off))) {
            break;
        }
        v.reset(OpWasmLoweredAddr);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64And(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64And (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x & y])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x & y);
        return true;
    } 
    // match: (I64And (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64And y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Eq(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Eq (I64Const [x]) (I64Const [y]))
    // cond: x == y
    // result: (I64Const [1])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        if (!(x == y)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (I64Eq (I64Const [x]) (I64Const [y]))
    // cond: x != y
    // result: (I64Const [0])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        y = auxIntToInt64(v_1.AuxInt);
        if (!(x != y)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (I64Eq (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Eq y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Eq);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    } 
    // match: (I64Eq x (I64Const [0]))
    // result: (I64Eqz x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpWasmI64Eqz);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Eqz(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (I64Eqz (I64Eqz (I64Eqz x)))
    // result: (I64Eqz x)
    while (true) {
        if (v_0.Op != OpWasmI64Eqz) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpWasmI64Eqz) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpWasmI64Eqz);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64LeU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64LeU x (I64Const [0]))
    // result: (I64Eqz x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpWasmI64Const || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpWasmI64Eqz);
        v.AddArg(x);
        return true;
    } 
    // match: (I64LeU (I64Const [1]) x)
    // result: (I64Eqz (I64Eqz x))
    while (true) {
        if (v_0.Op != OpWasmI64Const || auxIntToInt64(v_0.AuxInt) != 1) {
            break;
        }
        x = v_1;
        v.reset(OpWasmI64Eqz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Eqz, typ.Bool);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (I64Load [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (I64Load [off] (LoweredAddr {sym} [off2] (SB)) _)
    // cond: symIsRO(sym) && isU32Bit(off+int64(off2))
    // result: (I64Const [int64(read64(sym, off+int64(off2), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmLoweredAddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSB || !(symIsRO(sym) && isU32Bit(off + int64(off2)))) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(read64(sym, off + int64(off2), config.ctxt.Arch.ByteOrder)));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load16S(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Load16S [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load16S [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load16S);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (I64Load16U [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load16U [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load16U);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (I64Load16U [off] (LoweredAddr {sym} [off2] (SB)) _)
    // cond: symIsRO(sym) && isU32Bit(off+int64(off2))
    // result: (I64Const [int64(read16(sym, off+int64(off2), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmLoweredAddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSB || !(symIsRO(sym) && isU32Bit(off + int64(off2)))) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(read16(sym, off + int64(off2), config.ctxt.Arch.ByteOrder)));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load32S(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Load32S [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load32S [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load32S);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (I64Load32U [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load32U [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load32U);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (I64Load32U [off] (LoweredAddr {sym} [off2] (SB)) _)
    // cond: symIsRO(sym) && isU32Bit(off+int64(off2))
    // result: (I64Const [int64(read32(sym, off+int64(off2), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmLoweredAddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSB || !(symIsRO(sym) && isU32Bit(off + int64(off2)))) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(read32(sym, off + int64(off2), config.ctxt.Arch.ByteOrder)));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load8S(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Load8S [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load8S [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load8S);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Load8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Load8U [off] (I64AddConst [off2] ptr) mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Load8U [off+off2] ptr mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Load8U);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (I64Load8U [off] (LoweredAddr {sym} [off2] (SB)) _)
    // cond: symIsRO(sym) && isU32Bit(off+int64(off2))
    // result: (I64Const [int64(read8(sym, off+int64(off2)))])
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmLoweredAddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSB || !(symIsRO(sym) && isU32Bit(off + int64(off2)))) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(read8(sym, off + int64(off2))));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64LtU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64LtU (I64Const [0]) x)
    // result: (I64Eqz (I64Eqz x))
    while (true) {
        if (v_0.Op != OpWasmI64Const || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        var x = v_1;
        v.reset(OpWasmI64Eqz);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Eqz, typ.Bool);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (I64LtU x (I64Const [1]))
    // result: (I64Eqz x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const || auxIntToInt64(v_1.AuxInt) != 1) {
            break;
        }
        v.reset(OpWasmI64Eqz);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Mul(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Mul (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x * y])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x * y);
        return true;
    } 
    // match: (I64Mul (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Mul y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Mul);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Ne(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Ne (I64Const [x]) (I64Const [y]))
    // cond: x == y
    // result: (I64Const [0])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        if (!(x == y)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (I64Ne (I64Const [x]) (I64Const [y]))
    // cond: x != y
    // result: (I64Const [1])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        y = auxIntToInt64(v_1.AuxInt);
        if (!(x != y)) {
            break;
        }
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (I64Ne (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Ne y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Ne);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    } 
    // match: (I64Ne x (I64Const [0]))
    // result: (I64Eqz (I64Eqz x))
    while (true) {
        x = v_0;
        if (v_1.Op != OpWasmI64Const || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpWasmI64Eqz);
        v0 = b.NewValue0(v.Pos, OpWasmI64Eqz, typ.Bool);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Or(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Or (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x | y])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x | y);
        return true;
    } 
    // match: (I64Or (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Or y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Or);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Shl(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Shl (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x << uint64(y)])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x << (int)(uint64(y)));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64ShrS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64ShrS (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x >> uint64(y)])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x >> (int)(uint64(y)));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64ShrU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64ShrU (I64Const [x]) (I64Const [y]))
    // result: (I64Const [int64(uint64(x) >> uint64(y))])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(int64(uint64(x) >> (int)(uint64(y))));
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Store(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Store [off] (I64AddConst [off2] ptr) val mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Store [off+off2] ptr val mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Store16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Store16 [off] (I64AddConst [off2] ptr) val mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Store16 [off+off2] ptr val mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Store16);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Store32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Store32 [off] (I64AddConst [off2] ptr) val mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Store32 [off+off2] ptr val mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Store32);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Store8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (I64Store8 [off] (I64AddConst [off2] ptr) val mem)
    // cond: isU32Bit(off+off2)
    // result: (I64Store8 [off+off2] ptr val mem)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpWasmI64AddConst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(isU32Bit(off + off2))) {
            break;
        }
        v.reset(OpWasmI64Store8);
        v.AuxInt = int64ToAuxInt(off + off2);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpWasmI64Xor(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (I64Xor (I64Const [x]) (I64Const [y]))
    // result: (I64Const [x ^ y])
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpWasmI64Const) {
            break;
        }
        var y = auxIntToInt64(v_1.AuxInt);
        v.reset(OpWasmI64Const);
        v.AuxInt = int64ToAuxInt(x ^ y);
        return true;
    } 
    // match: (I64Xor (I64Const [x]) y)
    // cond: y.Op != OpWasmI64Const
    // result: (I64Xor y (I64Const [x]))
    while (true) {
        if (v_0.Op != OpWasmI64Const) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(y.Op != OpWasmI64Const)) {
            break;
        }
        v.reset(OpWasmI64Xor);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(x);
        v.AddArg2(y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpZero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Zero [0] _ mem)
    // result: mem
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var mem = v_1;
        v.copyOf(mem);
        return true;
    } 
    // match: (Zero [1] destptr mem)
    // result: (I64Store8 destptr (I64Const [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store8);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg3(destptr, v0, mem);
        return true;
    } 
    // match: (Zero [2] destptr mem)
    // result: (I64Store16 destptr (I64Const [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store16);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg3(destptr, v0, mem);
        return true;
    } 
    // match: (Zero [4] destptr mem)
    // result: (I64Store32 destptr (I64Const [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store32);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg3(destptr, v0, mem);
        return true;
    } 
    // match: (Zero [8] destptr mem)
    // result: (I64Store destptr (I64Const [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v.AddArg3(destptr, v0, mem);
        return true;
    } 
    // match: (Zero [3] destptr mem)
    // result: (I64Store8 [2] destptr (I64Const [0]) (I64Store16 destptr (I64Const [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store8);
        v.AuxInt = int64ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpWasmI64Store16, types.TypeMem);
        v1.AddArg3(destptr, v0, mem);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [5] destptr mem)
    // result: (I64Store8 [4] destptr (I64Const [0]) (I64Store32 destptr (I64Const [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store8);
        v.AuxInt = int64ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v1.AddArg3(destptr, v0, mem);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [6] destptr mem)
    // result: (I64Store16 [4] destptr (I64Const [0]) (I64Store32 destptr (I64Const [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store16);
        v.AuxInt = int64ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v1.AddArg3(destptr, v0, mem);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [7] destptr mem)
    // result: (I64Store32 [3] destptr (I64Const [0]) (I64Store32 destptr (I64Const [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store32);
        v.AuxInt = int64ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store32, types.TypeMem);
        v1.AddArg3(destptr, v0, mem);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s%8 != 0 && s > 8
    // result: (Zero [s-s%8] (OffPtr <destptr.Type> destptr [s%8]) (I64Store destptr (I64Const [0]) mem))
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s % 8 != 0 && s > 8)) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(s - s % 8);
        v0 = b.NewValue0(v.Pos, OpOffPtr, destptr.Type);
        v0.AuxInt = int64ToAuxInt(s % 8);
        v0.AddArg(destptr);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v2.AuxInt = int64ToAuxInt(0);
        v1.AddArg3(destptr, v2, mem);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Zero [16] destptr mem)
    // result: (I64Store [8] destptr (I64Const [0]) (I64Store destptr (I64Const [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v1.AddArg3(destptr, v0, mem);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [24] destptr mem)
    // result: (I64Store [16] destptr (I64Const [0]) (I64Store [8] destptr (I64Const [0]) (I64Store destptr (I64Const [0]) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 24) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(16);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v1.AuxInt = int64ToAuxInt(8);
        v2 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v2.AddArg3(destptr, v0, mem);
        v1.AddArg3(destptr, v0, v2);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [32] destptr mem)
    // result: (I64Store [24] destptr (I64Const [0]) (I64Store [16] destptr (I64Const [0]) (I64Store [8] destptr (I64Const [0]) (I64Store destptr (I64Const [0]) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 32) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpWasmI64Store);
        v.AuxInt = int64ToAuxInt(24);
        v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v1.AuxInt = int64ToAuxInt(16);
        v2 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v2.AuxInt = int64ToAuxInt(8);
        var v3 = b.NewValue0(v.Pos, OpWasmI64Store, types.TypeMem);
        v3.AddArg3(destptr, v0, mem);
        v2.AddArg3(destptr, v0, v3);
        v1.AddArg3(destptr, v0, v2);
        v.AddArg3(destptr, v0, v1);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s%8 == 0 && s > 32
    // result: (LoweredZero [s/8] destptr mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s % 8 == 0 && s > 32)) {
            break;
        }
        v.reset(OpWasmLoweredZero);
        v.AuxInt = int64ToAuxInt(s / 8);
        v.AddArg2(destptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueWasm_OpZeroExt16to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt16to32 x:(I64Load16U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load16U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt16to32 x)
    // result: (I64And x (I64Const [0xffff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xffff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpZeroExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt16to64 x:(I64Load16U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load16U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt16to64 x)
    // result: (I64And x (I64Const [0xffff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xffff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpZeroExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt32to64 x:(I64Load32U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load32U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt32to64 x)
    // result: (I64And x (I64Const [0xffffffff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xffffffff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpZeroExt8to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt8to16 x:(I64Load8U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt8to16 x)
    // result: (I64And x (I64Const [0xff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpZeroExt8to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt8to32 x:(I64Load8U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt8to32 x)
    // result: (I64And x (I64Const [0xff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueWasm_OpZeroExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ZeroExt8to64 x:(I64Load8U _ _))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpWasmI64Load8U) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ZeroExt8to64 x)
    // result: (I64And x (I64Const [0xff]))
    while (true) {
        x = v_0;
        v.reset(OpWasmI64And);
        var v0 = b.NewValue0(v.Pos, OpWasmI64Const, typ.Int64);
        v0.AuxInt = int64ToAuxInt(0xff);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteBlockWasm(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    return false;
}

} // end ssa_package
