// Code generated from gen/generic.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 23:02:36 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewritegeneric.go
using math = go.math_package;
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValuegeneric(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAdd16) 
        return rewriteValuegeneric_OpAdd16(_addr_v);
    else if (v.Op == OpAdd32) 
        return rewriteValuegeneric_OpAdd32(_addr_v);
    else if (v.Op == OpAdd32F) 
        return rewriteValuegeneric_OpAdd32F(_addr_v);
    else if (v.Op == OpAdd64) 
        return rewriteValuegeneric_OpAdd64(_addr_v);
    else if (v.Op == OpAdd64F) 
        return rewriteValuegeneric_OpAdd64F(_addr_v);
    else if (v.Op == OpAdd8) 
        return rewriteValuegeneric_OpAdd8(_addr_v);
    else if (v.Op == OpAddPtr) 
        return rewriteValuegeneric_OpAddPtr(_addr_v);
    else if (v.Op == OpAnd16) 
        return rewriteValuegeneric_OpAnd16(_addr_v);
    else if (v.Op == OpAnd32) 
        return rewriteValuegeneric_OpAnd32(_addr_v);
    else if (v.Op == OpAnd64) 
        return rewriteValuegeneric_OpAnd64(_addr_v);
    else if (v.Op == OpAnd8) 
        return rewriteValuegeneric_OpAnd8(_addr_v);
    else if (v.Op == OpAndB) 
        return rewriteValuegeneric_OpAndB(_addr_v);
    else if (v.Op == OpArraySelect) 
        return rewriteValuegeneric_OpArraySelect(_addr_v);
    else if (v.Op == OpCom16) 
        return rewriteValuegeneric_OpCom16(_addr_v);
    else if (v.Op == OpCom32) 
        return rewriteValuegeneric_OpCom32(_addr_v);
    else if (v.Op == OpCom64) 
        return rewriteValuegeneric_OpCom64(_addr_v);
    else if (v.Op == OpCom8) 
        return rewriteValuegeneric_OpCom8(_addr_v);
    else if (v.Op == OpConstInterface) 
        return rewriteValuegeneric_OpConstInterface(_addr_v);
    else if (v.Op == OpConstSlice) 
        return rewriteValuegeneric_OpConstSlice(_addr_v);
    else if (v.Op == OpConstString) 
        return rewriteValuegeneric_OpConstString(_addr_v);
    else if (v.Op == OpConvert) 
        return rewriteValuegeneric_OpConvert(_addr_v);
    else if (v.Op == OpCtz16) 
        return rewriteValuegeneric_OpCtz16(_addr_v);
    else if (v.Op == OpCtz32) 
        return rewriteValuegeneric_OpCtz32(_addr_v);
    else if (v.Op == OpCtz64) 
        return rewriteValuegeneric_OpCtz64(_addr_v);
    else if (v.Op == OpCtz8) 
        return rewriteValuegeneric_OpCtz8(_addr_v);
    else if (v.Op == OpCvt32Fto32) 
        return rewriteValuegeneric_OpCvt32Fto32(_addr_v);
    else if (v.Op == OpCvt32Fto64) 
        return rewriteValuegeneric_OpCvt32Fto64(_addr_v);
    else if (v.Op == OpCvt32Fto64F) 
        return rewriteValuegeneric_OpCvt32Fto64F(_addr_v);
    else if (v.Op == OpCvt32to32F) 
        return rewriteValuegeneric_OpCvt32to32F(_addr_v);
    else if (v.Op == OpCvt32to64F) 
        return rewriteValuegeneric_OpCvt32to64F(_addr_v);
    else if (v.Op == OpCvt64Fto32) 
        return rewriteValuegeneric_OpCvt64Fto32(_addr_v);
    else if (v.Op == OpCvt64Fto32F) 
        return rewriteValuegeneric_OpCvt64Fto32F(_addr_v);
    else if (v.Op == OpCvt64Fto64) 
        return rewriteValuegeneric_OpCvt64Fto64(_addr_v);
    else if (v.Op == OpCvt64to32F) 
        return rewriteValuegeneric_OpCvt64to32F(_addr_v);
    else if (v.Op == OpCvt64to64F) 
        return rewriteValuegeneric_OpCvt64to64F(_addr_v);
    else if (v.Op == OpCvtBoolToUint8) 
        return rewriteValuegeneric_OpCvtBoolToUint8(_addr_v);
    else if (v.Op == OpDiv16) 
        return rewriteValuegeneric_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValuegeneric_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValuegeneric_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        return rewriteValuegeneric_OpDiv32F(_addr_v);
    else if (v.Op == OpDiv32u) 
        return rewriteValuegeneric_OpDiv32u(_addr_v);
    else if (v.Op == OpDiv64) 
        return rewriteValuegeneric_OpDiv64(_addr_v);
    else if (v.Op == OpDiv64F) 
        return rewriteValuegeneric_OpDiv64F(_addr_v);
    else if (v.Op == OpDiv64u) 
        return rewriteValuegeneric_OpDiv64u(_addr_v);
    else if (v.Op == OpDiv8) 
        return rewriteValuegeneric_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValuegeneric_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValuegeneric_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValuegeneric_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValuegeneric_OpEq32F(_addr_v);
    else if (v.Op == OpEq64) 
        return rewriteValuegeneric_OpEq64(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValuegeneric_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValuegeneric_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValuegeneric_OpEqB(_addr_v);
    else if (v.Op == OpEqInter) 
        return rewriteValuegeneric_OpEqInter(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValuegeneric_OpEqPtr(_addr_v);
    else if (v.Op == OpEqSlice) 
        return rewriteValuegeneric_OpEqSlice(_addr_v);
    else if (v.Op == OpIMake) 
        return rewriteValuegeneric_OpIMake(_addr_v);
    else if (v.Op == OpInterLECall) 
        return rewriteValuegeneric_OpInterLECall(_addr_v);
    else if (v.Op == OpIsInBounds) 
        return rewriteValuegeneric_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValuegeneric_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValuegeneric_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValuegeneric_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValuegeneric_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValuegeneric_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValuegeneric_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValuegeneric_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64) 
        return rewriteValuegeneric_OpLeq64(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValuegeneric_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq64U) 
        return rewriteValuegeneric_OpLeq64U(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValuegeneric_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValuegeneric_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValuegeneric_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValuegeneric_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValuegeneric_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValuegeneric_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValuegeneric_OpLess32U(_addr_v);
    else if (v.Op == OpLess64) 
        return rewriteValuegeneric_OpLess64(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValuegeneric_OpLess64F(_addr_v);
    else if (v.Op == OpLess64U) 
        return rewriteValuegeneric_OpLess64U(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValuegeneric_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValuegeneric_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValuegeneric_OpLoad(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValuegeneric_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValuegeneric_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValuegeneric_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValuegeneric_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValuegeneric_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValuegeneric_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValuegeneric_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValuegeneric_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh64x16) 
        return rewriteValuegeneric_OpLsh64x16(_addr_v);
    else if (v.Op == OpLsh64x32) 
        return rewriteValuegeneric_OpLsh64x32(_addr_v);
    else if (v.Op == OpLsh64x64) 
        return rewriteValuegeneric_OpLsh64x64(_addr_v);
    else if (v.Op == OpLsh64x8) 
        return rewriteValuegeneric_OpLsh64x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValuegeneric_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValuegeneric_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValuegeneric_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValuegeneric_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValuegeneric_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValuegeneric_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValuegeneric_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValuegeneric_OpMod32u(_addr_v);
    else if (v.Op == OpMod64) 
        return rewriteValuegeneric_OpMod64(_addr_v);
    else if (v.Op == OpMod64u) 
        return rewriteValuegeneric_OpMod64u(_addr_v);
    else if (v.Op == OpMod8) 
        return rewriteValuegeneric_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValuegeneric_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValuegeneric_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        return rewriteValuegeneric_OpMul16(_addr_v);
    else if (v.Op == OpMul32) 
        return rewriteValuegeneric_OpMul32(_addr_v);
    else if (v.Op == OpMul32F) 
        return rewriteValuegeneric_OpMul32F(_addr_v);
    else if (v.Op == OpMul64) 
        return rewriteValuegeneric_OpMul64(_addr_v);
    else if (v.Op == OpMul64F) 
        return rewriteValuegeneric_OpMul64F(_addr_v);
    else if (v.Op == OpMul8) 
        return rewriteValuegeneric_OpMul8(_addr_v);
    else if (v.Op == OpNeg16) 
        return rewriteValuegeneric_OpNeg16(_addr_v);
    else if (v.Op == OpNeg32) 
        return rewriteValuegeneric_OpNeg32(_addr_v);
    else if (v.Op == OpNeg32F) 
        return rewriteValuegeneric_OpNeg32F(_addr_v);
    else if (v.Op == OpNeg64) 
        return rewriteValuegeneric_OpNeg64(_addr_v);
    else if (v.Op == OpNeg64F) 
        return rewriteValuegeneric_OpNeg64F(_addr_v);
    else if (v.Op == OpNeg8) 
        return rewriteValuegeneric_OpNeg8(_addr_v);
    else if (v.Op == OpNeq16) 
        return rewriteValuegeneric_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValuegeneric_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValuegeneric_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64) 
        return rewriteValuegeneric_OpNeq64(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValuegeneric_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValuegeneric_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        return rewriteValuegeneric_OpNeqB(_addr_v);
    else if (v.Op == OpNeqInter) 
        return rewriteValuegeneric_OpNeqInter(_addr_v);
    else if (v.Op == OpNeqPtr) 
        return rewriteValuegeneric_OpNeqPtr(_addr_v);
    else if (v.Op == OpNeqSlice) 
        return rewriteValuegeneric_OpNeqSlice(_addr_v);
    else if (v.Op == OpNilCheck) 
        return rewriteValuegeneric_OpNilCheck(_addr_v);
    else if (v.Op == OpNot) 
        return rewriteValuegeneric_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValuegeneric_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        return rewriteValuegeneric_OpOr16(_addr_v);
    else if (v.Op == OpOr32) 
        return rewriteValuegeneric_OpOr32(_addr_v);
    else if (v.Op == OpOr64) 
        return rewriteValuegeneric_OpOr64(_addr_v);
    else if (v.Op == OpOr8) 
        return rewriteValuegeneric_OpOr8(_addr_v);
    else if (v.Op == OpOrB) 
        return rewriteValuegeneric_OpOrB(_addr_v);
    else if (v.Op == OpPhi) 
        return rewriteValuegeneric_OpPhi(_addr_v);
    else if (v.Op == OpPtrIndex) 
        return rewriteValuegeneric_OpPtrIndex(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValuegeneric_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        return rewriteValuegeneric_OpRotateLeft32(_addr_v);
    else if (v.Op == OpRotateLeft64) 
        return rewriteValuegeneric_OpRotateLeft64(_addr_v);
    else if (v.Op == OpRotateLeft8) 
        return rewriteValuegeneric_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound32F) 
        return rewriteValuegeneric_OpRound32F(_addr_v);
    else if (v.Op == OpRound64F) 
        return rewriteValuegeneric_OpRound64F(_addr_v);
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValuegeneric_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValuegeneric_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValuegeneric_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValuegeneric_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValuegeneric_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValuegeneric_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValuegeneric_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValuegeneric_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValuegeneric_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValuegeneric_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValuegeneric_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValuegeneric_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValuegeneric_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValuegeneric_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValuegeneric_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValuegeneric_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh64Ux16) 
        return rewriteValuegeneric_OpRsh64Ux16(_addr_v);
    else if (v.Op == OpRsh64Ux32) 
        return rewriteValuegeneric_OpRsh64Ux32(_addr_v);
    else if (v.Op == OpRsh64Ux64) 
        return rewriteValuegeneric_OpRsh64Ux64(_addr_v);
    else if (v.Op == OpRsh64Ux8) 
        return rewriteValuegeneric_OpRsh64Ux8(_addr_v);
    else if (v.Op == OpRsh64x16) 
        return rewriteValuegeneric_OpRsh64x16(_addr_v);
    else if (v.Op == OpRsh64x32) 
        return rewriteValuegeneric_OpRsh64x32(_addr_v);
    else if (v.Op == OpRsh64x64) 
        return rewriteValuegeneric_OpRsh64x64(_addr_v);
    else if (v.Op == OpRsh64x8) 
        return rewriteValuegeneric_OpRsh64x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValuegeneric_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValuegeneric_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValuegeneric_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValuegeneric_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValuegeneric_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValuegeneric_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValuegeneric_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValuegeneric_OpRsh8x8(_addr_v);
    else if (v.Op == OpSelect0) 
        return rewriteValuegeneric_OpSelect0(_addr_v);
    else if (v.Op == OpSelect1) 
        return rewriteValuegeneric_OpSelect1(_addr_v);
    else if (v.Op == OpSelectN) 
        return rewriteValuegeneric_OpSelectN(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        return rewriteValuegeneric_OpSignExt16to32(_addr_v);
    else if (v.Op == OpSignExt16to64) 
        return rewriteValuegeneric_OpSignExt16to64(_addr_v);
    else if (v.Op == OpSignExt32to64) 
        return rewriteValuegeneric_OpSignExt32to64(_addr_v);
    else if (v.Op == OpSignExt8to16) 
        return rewriteValuegeneric_OpSignExt8to16(_addr_v);
    else if (v.Op == OpSignExt8to32) 
        return rewriteValuegeneric_OpSignExt8to32(_addr_v);
    else if (v.Op == OpSignExt8to64) 
        return rewriteValuegeneric_OpSignExt8to64(_addr_v);
    else if (v.Op == OpSliceCap) 
        return rewriteValuegeneric_OpSliceCap(_addr_v);
    else if (v.Op == OpSliceLen) 
        return rewriteValuegeneric_OpSliceLen(_addr_v);
    else if (v.Op == OpSlicePtr) 
        return rewriteValuegeneric_OpSlicePtr(_addr_v);
    else if (v.Op == OpSlicemask) 
        return rewriteValuegeneric_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        return rewriteValuegeneric_OpSqrt(_addr_v);
    else if (v.Op == OpStaticLECall) 
        return rewriteValuegeneric_OpStaticLECall(_addr_v);
    else if (v.Op == OpStore) 
        return rewriteValuegeneric_OpStore(_addr_v);
    else if (v.Op == OpStringLen) 
        return rewriteValuegeneric_OpStringLen(_addr_v);
    else if (v.Op == OpStringPtr) 
        return rewriteValuegeneric_OpStringPtr(_addr_v);
    else if (v.Op == OpStructSelect) 
        return rewriteValuegeneric_OpStructSelect(_addr_v);
    else if (v.Op == OpSub16) 
        return rewriteValuegeneric_OpSub16(_addr_v);
    else if (v.Op == OpSub32) 
        return rewriteValuegeneric_OpSub32(_addr_v);
    else if (v.Op == OpSub32F) 
        return rewriteValuegeneric_OpSub32F(_addr_v);
    else if (v.Op == OpSub64) 
        return rewriteValuegeneric_OpSub64(_addr_v);
    else if (v.Op == OpSub64F) 
        return rewriteValuegeneric_OpSub64F(_addr_v);
    else if (v.Op == OpSub8) 
        return rewriteValuegeneric_OpSub8(_addr_v);
    else if (v.Op == OpTrunc16to8) 
        return rewriteValuegeneric_OpTrunc16to8(_addr_v);
    else if (v.Op == OpTrunc32to16) 
        return rewriteValuegeneric_OpTrunc32to16(_addr_v);
    else if (v.Op == OpTrunc32to8) 
        return rewriteValuegeneric_OpTrunc32to8(_addr_v);
    else if (v.Op == OpTrunc64to16) 
        return rewriteValuegeneric_OpTrunc64to16(_addr_v);
    else if (v.Op == OpTrunc64to32) 
        return rewriteValuegeneric_OpTrunc64to32(_addr_v);
    else if (v.Op == OpTrunc64to8) 
        return rewriteValuegeneric_OpTrunc64to8(_addr_v);
    else if (v.Op == OpXor16) 
        return rewriteValuegeneric_OpXor16(_addr_v);
    else if (v.Op == OpXor32) 
        return rewriteValuegeneric_OpXor32(_addr_v);
    else if (v.Op == OpXor64) 
        return rewriteValuegeneric_OpXor64(_addr_v);
    else if (v.Op == OpXor8) 
        return rewriteValuegeneric_OpXor8(_addr_v);
    else if (v.Op == OpZero) 
        return rewriteValuegeneric_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        return rewriteValuegeneric_OpZeroExt16to32(_addr_v);
    else if (v.Op == OpZeroExt16to64) 
        return rewriteValuegeneric_OpZeroExt16to64(_addr_v);
    else if (v.Op == OpZeroExt32to64) 
        return rewriteValuegeneric_OpZeroExt32to64(_addr_v);
    else if (v.Op == OpZeroExt8to16) 
        return rewriteValuegeneric_OpZeroExt8to16(_addr_v);
    else if (v.Op == OpZeroExt8to32) 
        return rewriteValuegeneric_OpZeroExt8to32(_addr_v);
    else if (v.Op == OpZeroExt8to64) 
        return rewriteValuegeneric_OpZeroExt8to64(_addr_v);
        return false;

}
private static bool rewriteValuegeneric_OpAdd16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Add16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c+d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                var d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(c + d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 <t> (Mul16 x y) (Mul16 x z))
    // result: (Mul16 x (Add16 <t> y z))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var x = v_0_0;
                        var y = v_0_1;
                        if (v_1.Op != OpMul16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        _ = v_1.Args[1];
                        var v_1_0 = v_1.Args[0];
                        var v_1_1 = v_1.Args[1];
                        {
                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (x != v_1_0) {
                                    continue;
                                (_i2, v_1_0, v_1_1) = (_i2 + 1, v_1_1, v_1_0);
                                }

                                var z = v_1_1;
                                v.reset(OpMul16);
                                var v0 = b.NewValue0(v.Pos, OpAdd16, t);
                                v0.AddArg2(y, z);
                                v.AddArg2(x, v0);
                                return true;

                            }

                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Const16 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Const16 [1]) (Com16 x))
    // result: (Neg16 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 1 || v_1.Op != OpCom16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1.Args[0];
                v.reset(OpNeg16);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Add16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Add16 i (Add16 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAdd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                            continue;
                        }

                        v.reset(OpAdd16);
                        v0 = b.NewValue0(v.Pos, OpAdd16, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Sub16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Add16 i (Sub16 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpSub16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_0.Args[1];
                i = v_0.Args[0];
                if (i.Op != OpConst16) {
                    continue;
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                    continue;
                }

                v.reset(OpAdd16);
                v0 = b.NewValue0(v.Pos, OpSub16, t);
                v0.AddArg2(x, z);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Const16 <t> [c]) (Add16 (Const16 <t> [d]) x))
    // result: (Add16 (Const16 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpAdd16) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd16);
                        v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c + d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add16 (Const16 <t> [c]) (Sub16 (Const16 <t> [d]) x))
    // result: (Sub16 (Const16 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpSub16) {
                    continue;
                }

                x = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                    continue;
                }

                d = auxIntToInt16(v_1_0.AuxInt);
                v.reset(OpSub16);
                v0 = b.NewValue0(v.Pos, OpConst16, t);
                v0.AuxInt = int16ToAuxInt(c + d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAdd32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Add32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c+d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(c + d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 <t> (Mul32 x y) (Mul32 x z))
    // result: (Mul32 x (Add32 <t> y z))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var x = v_0_0;
                        var y = v_0_1;
                        if (v_1.Op != OpMul32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        _ = v_1.Args[1];
                        var v_1_0 = v_1.Args[0];
                        var v_1_1 = v_1.Args[1];
                        {
                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (x != v_1_0) {
                                    continue;
                                (_i2, v_1_0, v_1_1) = (_i2 + 1, v_1_1, v_1_0);
                                }

                                var z = v_1_1;
                                v.reset(OpMul32);
                                var v0 = b.NewValue0(v.Pos, OpAdd32, t);
                                v0.AddArg2(y, z);
                                v.AddArg2(x, v0);
                                return true;

                            }

                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Const32 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Const32 [1]) (Com32 x))
    // result: (Neg32 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 1 || v_1.Op != OpCom32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1.Args[0];
                v.reset(OpNeg32);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Add32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Add32 i (Add32 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAdd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                            continue;
                        }

                        v.reset(OpAdd32);
                        v0 = b.NewValue0(v.Pos, OpAdd32, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Sub32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Add32 i (Sub32 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpSub32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_0.Args[1];
                i = v_0.Args[0];
                if (i.Op != OpConst32) {
                    continue;
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                    continue;
                }

                v.reset(OpAdd32);
                v0 = b.NewValue0(v.Pos, OpSub32, t);
                v0.AddArg2(x, z);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Const32 <t> [c]) (Add32 (Const32 <t> [d]) x))
    // result: (Add32 (Const32 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpAdd32) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c + d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add32 (Const32 <t> [c]) (Sub32 (Const32 <t> [d]) x))
    // result: (Sub32 (Const32 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpSub32) {
                    continue;
                }

                x = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                    continue;
                }

                d = auxIntToInt32(v_1_0.AuxInt);
                v.reset(OpSub32);
                v0 = b.NewValue0(v.Pos, OpConst32, t);
                v0.AuxInt = int32ToAuxInt(c + d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAdd32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Add32F (Const32F [c]) (Const32F [d]))
    // cond: c+d == c+d
    // result: (Const32F [c+d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat32(v_0.AuxInt);
                if (v_1.Op != OpConst32F) {
                    continue;
                }

                var d = auxIntToFloat32(v_1.AuxInt);
                if (!(c + d == c + d)) {
                    continue;
                }

                v.reset(OpConst32F);
                v.AuxInt = float32ToAuxInt(c + d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAdd64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Add64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c+d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(c + d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 <t> (Mul64 x y) (Mul64 x z))
    // result: (Mul64 x (Add64 <t> y z))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var x = v_0_0;
                        var y = v_0_1;
                        if (v_1.Op != OpMul64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        _ = v_1.Args[1];
                        var v_1_0 = v_1.Args[0];
                        var v_1_1 = v_1.Args[1];
                        {
                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (x != v_1_0) {
                                    continue;
                                (_i2, v_1_0, v_1_1) = (_i2 + 1, v_1_1, v_1_0);
                                }

                                var z = v_1_1;
                                v.reset(OpMul64);
                                var v0 = b.NewValue0(v.Pos, OpAdd64, t);
                                v0.AddArg2(y, z);
                                v.AddArg2(x, v0);
                                return true;

                            }

                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Const64 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Const64 [1]) (Com64 x))
    // result: (Neg64 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 1 || v_1.Op != OpCom64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1.Args[0];
                v.reset(OpNeg64);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Add64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Add64 i (Add64 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAdd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                            continue;
                        }

                        v.reset(OpAdd64);
                        v0 = b.NewValue0(v.Pos, OpAdd64, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Sub64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Add64 i (Sub64 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpSub64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_0.Args[1];
                i = v_0.Args[0];
                if (i.Op != OpConst64) {
                    continue;
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                    continue;
                }

                v.reset(OpAdd64);
                v0 = b.NewValue0(v.Pos, OpSub64, t);
                v0.AddArg2(x, z);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Const64 <t> [c]) (Add64 (Const64 <t> [d]) x))
    // result: (Add64 (Const64 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpAdd64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c + d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add64 (Const64 <t> [c]) (Sub64 (Const64 <t> [d]) x))
    // result: (Sub64 (Const64 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpSub64) {
                    continue;
                }

                x = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                    continue;
                }

                d = auxIntToInt64(v_1_0.AuxInt);
                v.reset(OpSub64);
                v0 = b.NewValue0(v.Pos, OpConst64, t);
                v0.AuxInt = int64ToAuxInt(c + d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAdd64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Add64F (Const64F [c]) (Const64F [d]))
    // cond: c+d == c+d
    // result: (Const64F [c+d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat64(v_0.AuxInt);
                if (v_1.Op != OpConst64F) {
                    continue;
                }

                var d = auxIntToFloat64(v_1.AuxInt);
                if (!(c + d == c + d)) {
                    continue;
                }

                v.reset(OpConst64F);
                v.AuxInt = float64ToAuxInt(c + d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAdd8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Add8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c+d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(c + d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 <t> (Mul8 x y) (Mul8 x z))
    // result: (Mul8 x (Add8 <t> y z))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var x = v_0_0;
                        var y = v_0_1;
                        if (v_1.Op != OpMul8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        _ = v_1.Args[1];
                        var v_1_0 = v_1.Args[0];
                        var v_1_1 = v_1.Args[1];
                        {
                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (x != v_1_0) {
                                    continue;
                                (_i2, v_1_0, v_1_1) = (_i2 + 1, v_1_1, v_1_0);
                                }

                                var z = v_1_1;
                                v.reset(OpMul8);
                                var v0 = b.NewValue0(v.Pos, OpAdd8, t);
                                v0.AddArg2(y, z);
                                v.AddArg2(x, v0);
                                return true;

                            }

                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Const8 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Const8 [1]) (Com8 x))
    // result: (Neg8 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 1 || v_1.Op != OpCom8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1.Args[0];
                v.reset(OpNeg8);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Add8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Add8 i (Add8 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAdd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                            continue;
                        }

                        v.reset(OpAdd8);
                        v0 = b.NewValue0(v.Pos, OpAdd8, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Sub8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Add8 i (Sub8 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpSub8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_0.Args[1];
                i = v_0.Args[0];
                if (i.Op != OpConst8) {
                    continue;
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                    continue;
                }

                v.reset(OpAdd8);
                v0 = b.NewValue0(v.Pos, OpSub8, t);
                v0.AddArg2(x, z);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Const8 <t> [c]) (Add8 (Const8 <t> [d]) x))
    // result: (Add8 (Const8 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpAdd8) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd8);
                        v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c + d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Add8 (Const8 <t> [c]) (Sub8 (Const8 <t> [d]) x))
    // result: (Sub8 (Const8 <t> [c+d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpSub8) {
                    continue;
                }

                x = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                    continue;
                }

                d = auxIntToInt8(v_1_0.AuxInt);
                v.reset(OpSub8);
                v0 = b.NewValue0(v.Pos, OpConst8, t);
                v0.AuxInt = int8ToAuxInt(c + d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAddPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AddPtr <t> x (Const64 [c]))
    // result: (OffPtr <t> x [c])
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOffPtr);
        v.Type = t;
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (AddPtr <t> x (Const32 [c]))
    // result: (OffPtr <t> x [int64(c)])
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOffPtr);
        v.Type = t;
        v.AuxInt = int64ToAuxInt(int64(c));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAnd16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (And16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c&d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                var d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(c & d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 (Const16 [m]) (Rsh16Ux64 _ (Const64 [c])))
    // cond: c >= int64(16-ntz16(m))
    // result: (Const16 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var m = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpRsh16Ux64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(16 - ntz16(m)))) {
                    continue;
                }

                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 (Const16 [m]) (Lsh16x64 _ (Const64 [c])))
    // cond: c >= int64(16-nlz16(m))
    // result: (Const16 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                m = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpLsh16x64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(16 - nlz16(m)))) {
                    continue;
                }

                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (And16 (Const16 [-1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 x (And16 x y))
    // result: (And16 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpAnd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpAnd16);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 (And16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (And16 i (And16 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                            continue;
                        }

                        v.reset(OpAnd16);
                        var v0 = b.NewValue0(v.Pos, OpAnd16, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And16 (Const16 <t> [c]) (And16 (Const16 <t> [d]) x))
    // result: (And16 (Const16 <t> [c&d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpAnd16) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAnd16);
                        v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c & d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAnd32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (And32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c&d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(c & d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 (Const32 [m]) (Rsh32Ux64 _ (Const64 [c])))
    // cond: c >= int64(32-ntz32(m))
    // result: (Const32 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var m = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpRsh32Ux64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(32 - ntz32(m)))) {
                    continue;
                }

                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 (Const32 [m]) (Lsh32x64 _ (Const64 [c])))
    // cond: c >= int64(32-nlz32(m))
    // result: (Const32 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                m = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpLsh32x64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(32 - nlz32(m)))) {
                    continue;
                }

                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (And32 (Const32 [-1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 x (And32 x y))
    // result: (And32 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpAnd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpAnd32);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 (And32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (And32 i (And32 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                            continue;
                        }

                        v.reset(OpAnd32);
                        var v0 = b.NewValue0(v.Pos, OpAnd32, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And32 (Const32 <t> [c]) (And32 (Const32 <t> [d]) x))
    // result: (And32 (Const32 <t> [c&d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpAnd32) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAnd32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c & d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAnd64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (And64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c&d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(c & d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 (Const64 [m]) (Rsh64Ux64 _ (Const64 [c])))
    // cond: c >= int64(64-ntz64(m))
    // result: (Const64 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpRsh64Ux64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(64 - ntz64(m)))) {
                    continue;
                }

                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 (Const64 [m]) (Lsh64x64 _ (Const64 [c])))
    // cond: c >= int64(64-nlz64(m))
    // result: (Const64 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpLsh64x64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(64 - nlz64(m)))) {
                    continue;
                }

                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (And64 (Const64 [-1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 x (And64 x y))
    // result: (And64 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpAnd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpAnd64);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 (And64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (And64 i (And64 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                            continue;
                        }

                        v.reset(OpAnd64);
                        var v0 = b.NewValue0(v.Pos, OpAnd64, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And64 (Const64 <t> [c]) (And64 (Const64 <t> [d]) x))
    // result: (And64 (Const64 <t> [c&d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpAnd64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAnd64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c & d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAnd8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (And8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c&d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(c & d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 (Const8 [m]) (Rsh8Ux64 _ (Const64 [c])))
    // cond: c >= int64(8-ntz8(m))
    // result: (Const8 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var m = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpRsh8Ux64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(8 - ntz8(m)))) {
                    continue;
                }

                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 (Const8 [m]) (Lsh8x64 _ (Const64 [c])))
    // cond: c >= int64(8-nlz8(m))
    // result: (Const8 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                m = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpLsh8x64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= int64(8 - nlz8(m)))) {
                    continue;
                }

                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (And8 (Const8 [-1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 x (And8 x y))
    // result: (And8 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpAnd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpAnd8);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 (And8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (And8 i (And8 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                            continue;
                        }

                        v.reset(OpAnd8);
                        var v0 = b.NewValue0(v.Pos, OpAnd8, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (And8 (Const8 <t> [c]) (And8 (Const8 <t> [d]) x))
    // result: (And8 (Const8 <t> [c&d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpAnd8) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAnd8);
                        v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c & d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpAndB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AndB (Leq64 (Const64 [c]) x) (Less64 x (Const64 [d])))
    // cond: d >= c
    // result: (Less64U (Sub64 <x.Type> x (Const64 <x.Type> [c])) (Const64 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                var c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLess64U);
                var v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                var v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq64 (Const64 [c]) x) (Leq64 x (Const64 [d])))
    // cond: d >= c
    // result: (Leq64U (Sub64 <x.Type> x (Const64 <x.Type> [c])) (Const64 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq32 (Const32 [c]) x) (Less32 x (Const32 [d])))
    // cond: d >= c
    // result: (Less32U (Sub32 <x.Type> x (Const32 <x.Type> [c])) (Const32 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq32 (Const32 [c]) x) (Leq32 x (Const32 [d])))
    // cond: d >= c
    // result: (Leq32U (Sub32 <x.Type> x (Const32 <x.Type> [c])) (Const32 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq16 (Const16 [c]) x) (Less16 x (Const16 [d])))
    // cond: d >= c
    // result: (Less16U (Sub16 <x.Type> x (Const16 <x.Type> [c])) (Const16 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq16 (Const16 [c]) x) (Leq16 x (Const16 [d])))
    // cond: d >= c
    // result: (Leq16U (Sub16 <x.Type> x (Const16 <x.Type> [c])) (Const16 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq8 (Const8 [c]) x) (Less8 x (Const8 [d])))
    // cond: d >= c
    // result: (Less8U (Sub8 <x.Type> x (Const8 <x.Type> [c])) (Const8 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq8 (Const8 [c]) x) (Leq8 x (Const8 [d])))
    // cond: d >= c
    // result: (Leq8U (Sub8 <x.Type> x (Const8 <x.Type> [c])) (Const8 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(d >= c)) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less64 (Const64 [c]) x) (Less64 x (Const64 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Less64U (Sub64 <x.Type> x (Const64 <x.Type> [c+1])) (Const64 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less64 (Const64 [c]) x) (Leq64 x (Const64 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Leq64U (Sub64 <x.Type> x (Const64 <x.Type> [c+1])) (Const64 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less32 (Const32 [c]) x) (Less32 x (Const32 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Less32U (Sub32 <x.Type> x (Const32 <x.Type> [c+1])) (Const32 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less32 (Const32 [c]) x) (Leq32 x (Const32 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Leq32U (Sub32 <x.Type> x (Const32 <x.Type> [c+1])) (Const32 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less16 (Const16 [c]) x) (Less16 x (Const16 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Less16U (Sub16 <x.Type> x (Const16 <x.Type> [c+1])) (Const16 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less16 (Const16 [c]) x) (Leq16 x (Const16 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Leq16U (Sub16 <x.Type> x (Const16 <x.Type> [c+1])) (Const16 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less8 (Const8 [c]) x) (Less8 x (Const8 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Less8U (Sub8 <x.Type> x (Const8 <x.Type> [c+1])) (Const8 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less8 (Const8 [c]) x) (Leq8 x (Const8 [d])))
    // cond: d >= c+1 && c+1 > c
    // result: (Leq8U (Sub8 <x.Type> x (Const8 <x.Type> [c+1])) (Const8 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(d >= c + 1 && c + 1 > c)) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq64U (Const64 [c]) x) (Less64U x (Const64 [d])))
    // cond: uint64(d) >= uint64(c)
    // result: (Less64U (Sub64 <x.Type> x (Const64 <x.Type> [c])) (Const64 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(d) >= uint64(c))) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq64U (Const64 [c]) x) (Leq64U x (Const64 [d])))
    // cond: uint64(d) >= uint64(c)
    // result: (Leq64U (Sub64 <x.Type> x (Const64 <x.Type> [c])) (Const64 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(d) >= uint64(c))) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq32U (Const32 [c]) x) (Less32U x (Const32 [d])))
    // cond: uint32(d) >= uint32(c)
    // result: (Less32U (Sub32 <x.Type> x (Const32 <x.Type> [c])) (Const32 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(d) >= uint32(c))) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq32U (Const32 [c]) x) (Leq32U x (Const32 [d])))
    // cond: uint32(d) >= uint32(c)
    // result: (Leq32U (Sub32 <x.Type> x (Const32 <x.Type> [c])) (Const32 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(d) >= uint32(c))) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq16U (Const16 [c]) x) (Less16U x (Const16 [d])))
    // cond: uint16(d) >= uint16(c)
    // result: (Less16U (Sub16 <x.Type> x (Const16 <x.Type> [c])) (Const16 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(d) >= uint16(c))) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq16U (Const16 [c]) x) (Leq16U x (Const16 [d])))
    // cond: uint16(d) >= uint16(c)
    // result: (Leq16U (Sub16 <x.Type> x (Const16 <x.Type> [c])) (Const16 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(d) >= uint16(c))) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq8U (Const8 [c]) x) (Less8U x (Const8 [d])))
    // cond: uint8(d) >= uint8(c)
    // result: (Less8U (Sub8 <x.Type> x (Const8 <x.Type> [c])) (Const8 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(d) >= uint8(c))) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Leq8U (Const8 [c]) x) (Leq8U x (Const8 [d])))
    // cond: uint8(d) >= uint8(c)
    // result: (Leq8U (Sub8 <x.Type> x (Const8 <x.Type> [c])) (Const8 <x.Type> [d-c]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(d) >= uint8(c))) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less64U (Const64 [c]) x) (Less64U x (Const64 [d])))
    // cond: uint64(d) >= uint64(c+1) && uint64(c+1) > uint64(c)
    // result: (Less64U (Sub64 <x.Type> x (Const64 <x.Type> [c+1])) (Const64 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(d) >= uint64(c + 1) && uint64(c + 1) > uint64(c))) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less64U (Const64 [c]) x) (Leq64U x (Const64 [d])))
    // cond: uint64(d) >= uint64(c+1) && uint64(c+1) > uint64(c)
    // result: (Leq64U (Sub64 <x.Type> x (Const64 <x.Type> [c+1])) (Const64 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(d) >= uint64(c + 1) && uint64(c + 1) > uint64(c))) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v1.AuxInt = int64ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less32U (Const32 [c]) x) (Less32U x (Const32 [d])))
    // cond: uint32(d) >= uint32(c+1) && uint32(c+1) > uint32(c)
    // result: (Less32U (Sub32 <x.Type> x (Const32 <x.Type> [c+1])) (Const32 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(d) >= uint32(c + 1) && uint32(c + 1) > uint32(c))) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less32U (Const32 [c]) x) (Leq32U x (Const32 [d])))
    // cond: uint32(d) >= uint32(c+1) && uint32(c+1) > uint32(c)
    // result: (Leq32U (Sub32 <x.Type> x (Const32 <x.Type> [c+1])) (Const32 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(d) >= uint32(c + 1) && uint32(c + 1) > uint32(c))) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v1.AuxInt = int32ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less16U (Const16 [c]) x) (Less16U x (Const16 [d])))
    // cond: uint16(d) >= uint16(c+1) && uint16(c+1) > uint16(c)
    // result: (Less16U (Sub16 <x.Type> x (Const16 <x.Type> [c+1])) (Const16 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(d) >= uint16(c + 1) && uint16(c + 1) > uint16(c))) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less16U (Const16 [c]) x) (Leq16U x (Const16 [d])))
    // cond: uint16(d) >= uint16(c+1) && uint16(c+1) > uint16(c)
    // result: (Leq16U (Sub16 <x.Type> x (Const16 <x.Type> [c+1])) (Const16 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(d) >= uint16(c + 1) && uint16(c + 1) > uint16(c))) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v1.AuxInt = int16ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less8U (Const8 [c]) x) (Less8U x (Const8 [d])))
    // cond: uint8(d) >= uint8(c+1) && uint8(c+1) > uint8(c)
    // result: (Less8U (Sub8 <x.Type> x (Const8 <x.Type> [c+1])) (Const8 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(d) >= uint8(c + 1) && uint8(c + 1) > uint8(c))) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AndB (Less8U (Const8 [c]) x) (Leq8U x (Const8 [d])))
    // cond: uint8(d) >= uint8(c+1) && uint8(c+1) > uint8(c)
    // result: (Leq8U (Sub8 <x.Type> x (Const8 <x.Type> [c+1])) (Const8 <x.Type> [d-c-1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(d) >= uint8(c + 1) && uint8(c + 1) > uint8(c))) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v1 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v1.AuxInt = int8ToAuxInt(c + 1);
                v0.AddArg2(x, v1);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d - c - 1);
                v.AddArg2(v0, v2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpArraySelect(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ArraySelect (ArrayMake1 x))
    // result: x
    while (true) {
        if (v_0.Op != OpArrayMake1) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (ArraySelect [0] (IData x))
    // result: (IData x)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpIData) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpIData);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCom16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com16 (Com16 x))
    // result: x
    while (true) {
        if (v_0.Op != OpCom16) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Com16 (Const16 [c]))
    // result: (Const16 [^c])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(~c);
        return true;

    } 
    // match: (Com16 (Add16 (Const16 [-1]) x))
    // result: (Neg16 x)
    while (true) {
        if (v_0.Op != OpAdd16) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst16 || auxIntToInt16(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpNeg16);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCom32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com32 (Com32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpCom32) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Com32 (Const32 [c]))
    // result: (Const32 [^c])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(~c);
        return true;

    } 
    // match: (Com32 (Add32 (Const32 [-1]) x))
    // result: (Neg32 x)
    while (true) {
        if (v_0.Op != OpAdd32) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst32 || auxIntToInt32(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpNeg32);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCom64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com64 (Com64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpCom64) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Com64 (Const64 [c]))
    // result: (Const64 [^c])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(~c);
        return true;

    } 
    // match: (Com64 (Add64 (Const64 [-1]) x))
    // result: (Neg64 x)
    while (true) {
        if (v_0.Op != OpAdd64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64 || auxIntToInt64(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpNeg64);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCom8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com8 (Com8 x))
    // result: x
    while (true) {
        if (v_0.Op != OpCom8) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Com8 (Const8 [c]))
    // result: (Const8 [^c])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(~c);
        return true;

    } 
    // match: (Com8 (Add8 (Const8 [-1]) x))
    // result: (Neg8 x)
    while (true) {
        if (v_0.Op != OpAdd8) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst8 || auxIntToInt8(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpNeg8);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpConstInterface(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ConstInterface)
    // result: (IMake (ConstNil <typ.Uintptr>) (ConstNil <typ.BytePtr>))
    while (true) {
        v.reset(OpIMake);
        var v0 = b.NewValue0(v.Pos, OpConstNil, typ.Uintptr);
        var v1 = b.NewValue0(v.Pos, OpConstNil, typ.BytePtr);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValuegeneric_OpConstSlice(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ConstSlice)
    // cond: config.PtrSize == 4
    // result: (SliceMake (ConstNil <v.Type.Elem().PtrTo()>) (Const32 <typ.Int> [0]) (Const32 <typ.Int> [0]))
    while (true) {
        if (!(config.PtrSize == 4)) {
            break;
        }
        v.reset(OpSliceMake);
        var v0 = b.NewValue0(v.Pos, OpConstNil, v.Type.Elem().PtrTo());
        var v1 = b.NewValue0(v.Pos, OpConst32, typ.Int);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg3(v0, v1, v1);
        return true;

    } 
    // match: (ConstSlice)
    // cond: config.PtrSize == 8
    // result: (SliceMake (ConstNil <v.Type.Elem().PtrTo()>) (Const64 <typ.Int> [0]) (Const64 <typ.Int> [0]))
    while (true) {
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpSliceMake);
        v0 = b.NewValue0(v.Pos, OpConstNil, v.Type.Elem().PtrTo());
        v1 = b.NewValue0(v.Pos, OpConst64, typ.Int);
        v1.AuxInt = int64ToAuxInt(0);
        v.AddArg3(v0, v1, v1);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpConstString(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var config = b.Func.Config;
    var fe = b.Func.fe;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ConstString {str})
    // cond: config.PtrSize == 4 && str == ""
    // result: (StringMake (ConstNil) (Const32 <typ.Int> [0]))
    while (true) {
        var str = auxToString(v.Aux);
        if (!(config.PtrSize == 4 && str == "")) {
            break;
        }
        v.reset(OpStringMake);
        var v0 = b.NewValue0(v.Pos, OpConstNil, typ.BytePtr);
        var v1 = b.NewValue0(v.Pos, OpConst32, typ.Int);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (ConstString {str})
    // cond: config.PtrSize == 8 && str == ""
    // result: (StringMake (ConstNil) (Const64 <typ.Int> [0]))
    while (true) {
        str = auxToString(v.Aux);
        if (!(config.PtrSize == 8 && str == "")) {
            break;
        }
        v.reset(OpStringMake);
        v0 = b.NewValue0(v.Pos, OpConstNil, typ.BytePtr);
        v1 = b.NewValue0(v.Pos, OpConst64, typ.Int);
        v1.AuxInt = int64ToAuxInt(0);
        v.AddArg2(v0, v1);
        return true;

    } 
    // match: (ConstString {str})
    // cond: config.PtrSize == 4 && str != ""
    // result: (StringMake (Addr <typ.BytePtr> {fe.StringData(str)} (SB)) (Const32 <typ.Int> [int32(len(str))]))
    while (true) {
        str = auxToString(v.Aux);
        if (!(config.PtrSize == 4 && str != "")) {
            break;
        }
        v.reset(OpStringMake);
        v0 = b.NewValue0(v.Pos, OpAddr, typ.BytePtr);
        v0.Aux = symToAux(fe.StringData(str));
        v1 = b.NewValue0(v.Pos, OpSB, typ.Uintptr);
        v0.AddArg(v1);
        var v2 = b.NewValue0(v.Pos, OpConst32, typ.Int);
        v2.AuxInt = int32ToAuxInt(int32(len(str)));
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (ConstString {str})
    // cond: config.PtrSize == 8 && str != ""
    // result: (StringMake (Addr <typ.BytePtr> {fe.StringData(str)} (SB)) (Const64 <typ.Int> [int64(len(str))]))
    while (true) {
        str = auxToString(v.Aux);
        if (!(config.PtrSize == 8 && str != "")) {
            break;
        }
        v.reset(OpStringMake);
        v0 = b.NewValue0(v.Pos, OpAddr, typ.BytePtr);
        v0.Aux = symToAux(fe.StringData(str));
        v1 = b.NewValue0(v.Pos, OpSB, typ.Uintptr);
        v0.AddArg(v1);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.Int);
        v2.AuxInt = int64ToAuxInt(int64(len(str)));
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpConvert(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Convert (Add64 (Convert ptr mem) off) mem)
    // result: (AddPtr ptr off)
    while (true) {
        if (v_0.Op != OpAdd64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConvert) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var mem = v_0_0.Args[1];
                var ptr = v_0_0.Args[0];
                var off = v_0_1;
                if (mem != v_1) {
                    continue;
                }

                v.reset(OpAddPtr);
                v.AddArg2(ptr, off);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Convert (Add32 (Convert ptr mem) off) mem)
    // result: (AddPtr ptr off)
    while (true) {
        if (v_0.Op != OpAdd32) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConvert) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                mem = v_0_0.Args[1];
                ptr = v_0_0.Args[0];
                off = v_0_1;
                if (mem != v_1) {
                    continue;
                }

                v.reset(OpAddPtr);
                v.AddArg2(ptr, off);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Convert (Convert ptr mem) mem)
    // result: ptr
    while (true) {
        if (v_0.Op != OpConvert) {
            break;
        }
        mem = v_0.Args[1];
        ptr = v_0.Args[0];
        if (mem != v_1) {
            break;
        }
        v.copyOf(ptr);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCtz16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Ctz16 (Const16 [c]))
    // cond: config.PtrSize == 4
    // result: (Const32 [int32(ntz16(c))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (!(config.PtrSize == 4)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(ntz16(c)));
        return true;

    } 
    // match: (Ctz16 (Const16 [c]))
    // cond: config.PtrSize == 8
    // result: (Const64 [int64(ntz16(c))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_0.AuxInt);
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(ntz16(c)));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Ctz32 (Const32 [c]))
    // cond: config.PtrSize == 4
    // result: (Const32 [int32(ntz32(c))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (!(config.PtrSize == 4)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(ntz32(c)));
        return true;

    } 
    // match: (Ctz32 (Const32 [c]))
    // cond: config.PtrSize == 8
    // result: (Const64 [int64(ntz32(c))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(ntz32(c)));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCtz64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Ctz64 (Const64 [c]))
    // cond: config.PtrSize == 4
    // result: (Const32 [int32(ntz64(c))])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (!(config.PtrSize == 4)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(ntz64(c)));
        return true;

    } 
    // match: (Ctz64 (Const64 [c]))
    // cond: config.PtrSize == 8
    // result: (Const64 [int64(ntz64(c))])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(ntz64(c)));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCtz8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Ctz8 (Const8 [c]))
    // cond: config.PtrSize == 4
    // result: (Const32 [int32(ntz8(c))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (!(config.PtrSize == 4)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(ntz8(c)));
        return true;

    } 
    // match: (Ctz8 (Const8 [c]))
    // cond: config.PtrSize == 8
    // result: (Const64 [int64(ntz8(c))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_0.AuxInt);
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(ntz8(c)));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt32Fto32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt32Fto32 (Const32F [c]))
    // result: (Const32 [int32(c)])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt32Fto64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt32Fto64 (Const32F [c]))
    // result: (Const64 [int64(c)])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt32Fto64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt32Fto64F (Const32F [c]))
    // result: (Const64F [float64(c)])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(float64(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt32to32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt32to32F (Const32 [c]))
    // result: (Const32F [float32(c)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(float32(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt32to64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt32to64F (Const32 [c]))
    // result: (Const64F [float64(c)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(float64(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt64Fto32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt64Fto32 (Const64F [c]))
    // result: (Const32 [int32(c)])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt64Fto32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt64Fto32F (Const64F [c]))
    // result: (Const32F [float32(c)])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(float32(c));
        return true;

    } 
    // match: (Cvt64Fto32F sqrt0:(Sqrt (Cvt32Fto64F x)))
    // cond: sqrt0.Uses==1
    // result: (Sqrt32 x)
    while (true) {
        var sqrt0 = v_0;
        if (sqrt0.Op != OpSqrt) {
            break;
        }
        var sqrt0_0 = sqrt0.Args[0];
        if (sqrt0_0.Op != OpCvt32Fto64F) {
            break;
        }
        var x = sqrt0_0.Args[0];
        if (!(sqrt0.Uses == 1)) {
            break;
        }
        v.reset(OpSqrt32);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt64Fto64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt64Fto64 (Const64F [c]))
    // result: (Const64 [int64(c)])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt64to32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt64to32F (Const64 [c]))
    // result: (Const32F [float32(c)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(float32(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvt64to64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Cvt64to64F (Const64 [c]))
    // result: (Const64F [float64(c)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(float64(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpCvtBoolToUint8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CvtBoolToUint8 (ConstBool [false]))
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != false) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (CvtBoolToUint8 (ConstBool [true]))
    // result: (Const8 [1])
    while (true) {
        if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != true) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(1);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 (Const16 [c]) (Const16 [d]))
    // cond: d != 0
    // result: (Const16 [c/d])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c / d);
        return true;

    } 
    // match: (Div16 n (Const16 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo16(c)
    // result: (Rsh16Ux64 n (Const64 <typ.UInt64> [log16(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo16(c))) {
            break;
        }
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log16(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div16 <t> n (Const16 [c]))
    // cond: c < 0 && c != -1<<15
    // result: (Neg16 (Div16 <t> n (Const16 <t> [-c])))
    while (true) {
        var t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 15)) {
            break;
        }
        v.reset(OpNeg16);
        v0 = b.NewValue0(v.Pos, OpDiv16, t);
        var v1 = b.NewValue0(v.Pos, OpConst16, t);
        v1.AuxInt = int16ToAuxInt(-c);
        v0.AddArg2(n, v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div16 <t> x (Const16 [-1<<15]))
    // result: (Rsh16Ux64 (And16 <t> x (Neg16 <t> x)) (Const64 <typ.UInt64> [15]))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != -1 << 15) {
            break;
        }
        v.reset(OpRsh16Ux64);
        v0 = b.NewValue0(v.Pos, OpAnd16, t);
        v1 = b.NewValue0(v.Pos, OpNeg16, t);
        v1.AddArg(x);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(15);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div16 <t> n (Const16 [c]))
    // cond: isPowerOfTwo16(c)
    // result: (Rsh16x64 (Add16 <t> n (Rsh16Ux64 <t> (Rsh16x64 <t> n (Const64 <typ.UInt64> [15])) (Const64 <typ.UInt64> [int64(16-log16(c))]))) (Const64 <typ.UInt64> [int64(log16(c))]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(isPowerOfTwo16(c))) {
            break;
        }
        v.reset(OpRsh16x64);
        v0 = b.NewValue0(v.Pos, OpAdd16, t);
        v1 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
        v2 = b.NewValue0(v.Pos, OpRsh16x64, t);
        var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(15);
        v2.AddArg2(n, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(int64(16 - log16(c)));
        v1.AddArg2(v2, v4);
        v0.AddArg2(n, v1);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(int64(log16(c)));
        v.AddArg2(v0, v5);
        return true;

    } 
    // match: (Div16 <t> x (Const16 [c]))
    // cond: smagicOK16(c)
    // result: (Sub16 <t> (Rsh32x64 <t> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(smagic16(c).m)]) (SignExt16to32 x)) (Const64 <typ.UInt64> [16+smagic16(c).s])) (Rsh32x64 <t> (SignExt16to32 x) (Const64 <typ.UInt64> [31])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(smagicOK16(c))) {
            break;
        }
        v.reset(OpSub16);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(smagic16(c).m));
        v3 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(16 + smagic16(c).s);
        v0.AddArg2(v1, v4);
        v5 = b.NewValue0(v.Pos, OpRsh32x64, t);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(31);
        v5.AddArg2(v3, v6);
        v.AddArg2(v0, v5);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u (Const16 [c]) (Const16 [d]))
    // cond: d != 0
    // result: (Const16 [int16(uint16(c)/uint16(d))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(uint16(c) / uint16(d)));
        return true;

    } 
    // match: (Div16u n (Const16 [c]))
    // cond: isPowerOfTwo16(c)
    // result: (Rsh16Ux64 n (Const64 <typ.UInt64> [log16(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(isPowerOfTwo16(c))) {
            break;
        }
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log16(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div16u x (Const16 [c]))
    // cond: umagicOK16(c) && config.RegSize == 8
    // result: (Trunc64to16 (Rsh64Ux64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(1<<16+umagic16(c).m)]) (ZeroExt16to64 x)) (Const64 <typ.UInt64> [16+umagic16(c).s])))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(umagicOK16(c) && config.RegSize == 8)) {
            break;
        }
        v.reset(OpTrunc64to16);
        v0 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        var v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(1 << 16 + umagic16(c).m));
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(16 + umagic16(c).s);
        v0.AddArg2(v1, v4);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div16u x (Const16 [c]))
    // cond: umagicOK16(c) && config.RegSize == 4 && umagic16(c).m&1 == 0
    // result: (Trunc32to16 (Rsh32Ux64 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(1<<15+umagic16(c).m/2)]) (ZeroExt16to32 x)) (Const64 <typ.UInt64> [16+umagic16(c).s-1])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(umagicOK16(c) && config.RegSize == 4 && umagic16(c).m & 1 == 0)) {
            break;
        }
        v.reset(OpTrunc32to16);
        v0 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(1 << 15 + umagic16(c).m / 2));
        v3 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(16 + umagic16(c).s - 1);
        v0.AddArg2(v1, v4);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div16u x (Const16 [c]))
    // cond: umagicOK16(c) && config.RegSize == 4 && c&1 == 0
    // result: (Trunc32to16 (Rsh32Ux64 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(1<<15+(umagic16(c).m+1)/2)]) (Rsh32Ux64 <typ.UInt32> (ZeroExt16to32 x) (Const64 <typ.UInt64> [1]))) (Const64 <typ.UInt64> [16+umagic16(c).s-2])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(umagicOK16(c) && config.RegSize == 4 && c & 1 == 0)) {
            break;
        }
        v.reset(OpTrunc32to16);
        v0 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(1 << 15 + (umagic16(c).m + 1) / 2));
        v3 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(1);
        v3.AddArg2(v4, v5);
        v1.AddArg2(v2, v3);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(16 + umagic16(c).s - 2);
        v0.AddArg2(v1, v6);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div16u x (Const16 [c]))
    // cond: umagicOK16(c) && config.RegSize == 4 && config.useAvg
    // result: (Trunc32to16 (Rsh32Ux64 <typ.UInt32> (Avg32u (Lsh32x64 <typ.UInt32> (ZeroExt16to32 x) (Const64 <typ.UInt64> [16])) (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(umagic16(c).m)]) (ZeroExt16to32 x))) (Const64 <typ.UInt64> [16+umagic16(c).s-1])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(umagicOK16(c) && config.RegSize == 4 && config.useAvg)) {
            break;
        }
        v.reset(OpTrunc32to16);
        v0 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpAvg32u, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpLsh32x64, typ.UInt32);
        v3 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v3.AddArg(x);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(16);
        v2.AddArg2(v3, v4);
        v5 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v6.AuxInt = int32ToAuxInt(int32(umagic16(c).m));
        v5.AddArg2(v6, v3);
        v1.AddArg2(v2, v5);
        var v7 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v7.AuxInt = int64ToAuxInt(16 + umagic16(c).s - 1);
        v0.AddArg2(v1, v7);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32 (Const32 [c]) (Const32 [d]))
    // cond: d != 0
    // result: (Const32 [c/d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c / d);
        return true;

    } 
    // match: (Div32 n (Const32 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo32(c)
    // result: (Rsh32Ux64 n (Const64 <typ.UInt64> [log32(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log32(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div32 <t> n (Const32 [c]))
    // cond: c < 0 && c != -1<<31
    // result: (Neg32 (Div32 <t> n (Const32 <t> [-c])))
    while (true) {
        var t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 31)) {
            break;
        }
        v.reset(OpNeg32);
        v0 = b.NewValue0(v.Pos, OpDiv32, t);
        var v1 = b.NewValue0(v.Pos, OpConst32, t);
        v1.AuxInt = int32ToAuxInt(-c);
        v0.AddArg2(n, v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div32 <t> x (Const32 [-1<<31]))
    // result: (Rsh32Ux64 (And32 <t> x (Neg32 <t> x)) (Const64 <typ.UInt64> [31]))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32 || auxIntToInt32(v_1.AuxInt) != -1 << 31) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v0 = b.NewValue0(v.Pos, OpAnd32, t);
        v1 = b.NewValue0(v.Pos, OpNeg32, t);
        v1.AddArg(x);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(31);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div32 <t> n (Const32 [c]))
    // cond: isPowerOfTwo32(c)
    // result: (Rsh32x64 (Add32 <t> n (Rsh32Ux64 <t> (Rsh32x64 <t> n (Const64 <typ.UInt64> [31])) (Const64 <typ.UInt64> [int64(32-log32(c))]))) (Const64 <typ.UInt64> [int64(log32(c))]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpRsh32x64);
        v0 = b.NewValue0(v.Pos, OpAdd32, t);
        v1 = b.NewValue0(v.Pos, OpRsh32Ux64, t);
        v2 = b.NewValue0(v.Pos, OpRsh32x64, t);
        var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(31);
        v2.AddArg2(n, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(int64(32 - log32(c)));
        v1.AddArg2(v2, v4);
        v0.AddArg2(n, v1);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(int64(log32(c)));
        v.AddArg2(v0, v5);
        return true;

    } 
    // match: (Div32 <t> x (Const32 [c]))
    // cond: smagicOK32(c) && config.RegSize == 8
    // result: (Sub32 <t> (Rsh64x64 <t> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(smagic32(c).m)]) (SignExt32to64 x)) (Const64 <typ.UInt64> [32+smagic32(c).s])) (Rsh64x64 <t> (SignExt32to64 x) (Const64 <typ.UInt64> [63])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(smagicOK32(c) && config.RegSize == 8)) {
            break;
        }
        v.reset(OpSub32);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh64x64, t);
        v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(smagic32(c).m));
        v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(32 + smagic32(c).s);
        v0.AddArg2(v1, v4);
        v5 = b.NewValue0(v.Pos, OpRsh64x64, t);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(63);
        v5.AddArg2(v3, v6);
        v.AddArg2(v0, v5);
        return true;

    } 
    // match: (Div32 <t> x (Const32 [c]))
    // cond: smagicOK32(c) && config.RegSize == 4 && smagic32(c).m&1 == 0 && config.useHmul
    // result: (Sub32 <t> (Rsh32x64 <t> (Hmul32 <t> (Const32 <typ.UInt32> [int32(smagic32(c).m/2)]) x) (Const64 <typ.UInt64> [smagic32(c).s-1])) (Rsh32x64 <t> x (Const64 <typ.UInt64> [31])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(smagicOK32(c) && config.RegSize == 4 && smagic32(c).m & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpSub32);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v1 = b.NewValue0(v.Pos, OpHmul32, t);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(smagic32(c).m / 2));
        v1.AddArg2(v2, x);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(smagic32(c).s - 1);
        v0.AddArg2(v1, v3);
        v4 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(31);
        v4.AddArg2(x, v5);
        v.AddArg2(v0, v4);
        return true;

    } 
    // match: (Div32 <t> x (Const32 [c]))
    // cond: smagicOK32(c) && config.RegSize == 4 && smagic32(c).m&1 != 0 && config.useHmul
    // result: (Sub32 <t> (Rsh32x64 <t> (Add32 <t> (Hmul32 <t> (Const32 <typ.UInt32> [int32(smagic32(c).m)]) x) x) (Const64 <typ.UInt64> [smagic32(c).s])) (Rsh32x64 <t> x (Const64 <typ.UInt64> [31])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(smagicOK32(c) && config.RegSize == 4 && smagic32(c).m & 1 != 0 && config.useHmul)) {
            break;
        }
        v.reset(OpSub32);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v1 = b.NewValue0(v.Pos, OpAdd32, t);
        v2 = b.NewValue0(v.Pos, OpHmul32, t);
        v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(int32(smagic32(c).m));
        v2.AddArg2(v3, x);
        v1.AddArg2(v2, x);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(smagic32(c).s);
        v0.AddArg2(v1, v4);
        v5 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(31);
        v5.AddArg2(x, v6);
        v.AddArg2(v0, v5);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Div32F (Const32F [c]) (Const32F [d]))
    // cond: c/d == c/d
    // result: (Const32F [c/d])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        if (v_1.Op != OpConst32F) {
            break;
        }
        var d = auxIntToFloat32(v_1.AuxInt);
        if (!(c / d == c / d)) {
            break;
        }
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(c / d);
        return true;

    } 
    // match: (Div32F x (Const32F <t> [c]))
    // cond: reciprocalExact32(c)
    // result: (Mul32F x (Const32F <t> [1/c]))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst32F) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToFloat32(v_1.AuxInt);
        if (!(reciprocalExact32(c))) {
            break;
        }
        v.reset(OpMul32F);
        var v0 = b.NewValue0(v.Pos, OpConst32F, t);
        v0.AuxInt = float32ToAuxInt(1 / c);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32u (Const32 [c]) (Const32 [d]))
    // cond: d != 0
    // result: (Const32 [int32(uint32(c)/uint32(d))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) / uint32(d)));
        return true;

    } 
    // match: (Div32u n (Const32 [c]))
    // cond: isPowerOfTwo32(c)
    // result: (Rsh32Ux64 n (Const64 <typ.UInt64> [log32(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log32(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 4 && umagic32(c).m&1 == 0 && config.useHmul
    // result: (Rsh32Ux64 <typ.UInt32> (Hmul32u <typ.UInt32> (Const32 <typ.UInt32> [int32(1<<31+umagic32(c).m/2)]) x) (Const64 <typ.UInt64> [umagic32(c).s-1]))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 4 && umagic32(c).m & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v.Type = typ.UInt32;
        v0 = b.NewValue0(v.Pos, OpHmul32u, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(int32(1 << 31 + umagic32(c).m / 2));
        v0.AddArg2(v1, x);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(umagic32(c).s - 1);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 4 && c&1 == 0 && config.useHmul
    // result: (Rsh32Ux64 <typ.UInt32> (Hmul32u <typ.UInt32> (Const32 <typ.UInt32> [int32(1<<31+(umagic32(c).m+1)/2)]) (Rsh32Ux64 <typ.UInt32> x (Const64 <typ.UInt64> [1]))) (Const64 <typ.UInt64> [umagic32(c).s-2]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 4 && c & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v.Type = typ.UInt32;
        v0 = b.NewValue0(v.Pos, OpHmul32u, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(int32(1 << 31 + (umagic32(c).m + 1) / 2));
        v2 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(1);
        v2.AddArg2(x, v3);
        v0.AddArg2(v1, v2);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(umagic32(c).s - 2);
        v.AddArg2(v0, v4);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 4 && config.useAvg && config.useHmul
    // result: (Rsh32Ux64 <typ.UInt32> (Avg32u x (Hmul32u <typ.UInt32> (Const32 <typ.UInt32> [int32(umagic32(c).m)]) x)) (Const64 <typ.UInt64> [umagic32(c).s-1]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 4 && config.useAvg && config.useHmul)) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v.Type = typ.UInt32;
        v0 = b.NewValue0(v.Pos, OpAvg32u, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpHmul32u, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(umagic32(c).m));
        v1.AddArg2(v2, x);
        v0.AddArg2(x, v1);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(umagic32(c).s - 1);
        v.AddArg2(v0, v3);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 8 && umagic32(c).m&1 == 0
    // result: (Trunc64to32 (Rsh64Ux64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(1<<31+umagic32(c).m/2)]) (ZeroExt32to64 x)) (Const64 <typ.UInt64> [32+umagic32(c).s-1])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 8 && umagic32(c).m & 1 == 0)) {
            break;
        }
        v.reset(OpTrunc64to32);
        v0 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(1 << 31 + umagic32(c).m / 2));
        v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(32 + umagic32(c).s - 1);
        v0.AddArg2(v1, v4);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 8 && c&1 == 0
    // result: (Trunc64to32 (Rsh64Ux64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(1<<31+(umagic32(c).m+1)/2)]) (Rsh64Ux64 <typ.UInt64> (ZeroExt32to64 x) (Const64 <typ.UInt64> [1]))) (Const64 <typ.UInt64> [32+umagic32(c).s-2])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 8 && c & 1 == 0)) {
            break;
        }
        v.reset(OpTrunc64to32);
        v0 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(1 << 31 + (umagic32(c).m + 1) / 2));
        v3 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(1);
        v3.AddArg2(v4, v5);
        v1.AddArg2(v2, v3);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(32 + umagic32(c).s - 2);
        v0.AddArg2(v1, v6);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div32u x (Const32 [c]))
    // cond: umagicOK32(c) && config.RegSize == 8 && config.useAvg
    // result: (Trunc64to32 (Rsh64Ux64 <typ.UInt64> (Avg64u (Lsh64x64 <typ.UInt64> (ZeroExt32to64 x) (Const64 <typ.UInt64> [32])) (Mul64 <typ.UInt64> (Const64 <typ.UInt32> [int64(umagic32(c).m)]) (ZeroExt32to64 x))) (Const64 <typ.UInt64> [32+umagic32(c).s-1])))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(umagicOK32(c) && config.RegSize == 8 && config.useAvg)) {
            break;
        }
        v.reset(OpTrunc64to32);
        v0 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpAvg64u, typ.UInt64);
        v2 = b.NewValue0(v.Pos, OpLsh64x64, typ.UInt64);
        v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        v3.AddArg(x);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v5 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt32);
        v6.AuxInt = int64ToAuxInt(int64(umagic32(c).m));
        v5.AddArg2(v6, v3);
        v1.AddArg2(v2, v5);
        var v7 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v7.AuxInt = int64ToAuxInt(32 + umagic32(c).s - 1);
        v0.AddArg2(v1, v7);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div64 (Const64 [c]) (Const64 [d]))
    // cond: d != 0
    // result: (Const64 [c/d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c / d);
        return true;

    } 
    // match: (Div64 n (Const64 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo64(c)
    // result: (Rsh64Ux64 n (Const64 <typ.UInt64> [log64(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo64(c))) {
            break;
        }
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log64(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div64 n (Const64 [-1<<63]))
    // cond: isNonNegative(n)
    // result: (Const64 [0])
    while (true) {
        n = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != -1 << 63 || !(isNonNegative(n))) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Div64 <t> n (Const64 [c]))
    // cond: c < 0 && c != -1<<63
    // result: (Neg64 (Div64 <t> n (Const64 <t> [-c])))
    while (true) {
        var t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 63)) {
            break;
        }
        v.reset(OpNeg64);
        v0 = b.NewValue0(v.Pos, OpDiv64, t);
        var v1 = b.NewValue0(v.Pos, OpConst64, t);
        v1.AuxInt = int64ToAuxInt(-c);
        v0.AddArg2(n, v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div64 <t> x (Const64 [-1<<63]))
    // result: (Rsh64Ux64 (And64 <t> x (Neg64 <t> x)) (Const64 <typ.UInt64> [63]))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != -1 << 63) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v0 = b.NewValue0(v.Pos, OpAnd64, t);
        v1 = b.NewValue0(v.Pos, OpNeg64, t);
        v1.AddArg(x);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(63);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div64 <t> n (Const64 [c]))
    // cond: isPowerOfTwo64(c)
    // result: (Rsh64x64 (Add64 <t> n (Rsh64Ux64 <t> (Rsh64x64 <t> n (Const64 <typ.UInt64> [63])) (Const64 <typ.UInt64> [int64(64-log64(c))]))) (Const64 <typ.UInt64> [int64(log64(c))]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(isPowerOfTwo64(c))) {
            break;
        }
        v.reset(OpRsh64x64);
        v0 = b.NewValue0(v.Pos, OpAdd64, t);
        v1 = b.NewValue0(v.Pos, OpRsh64Ux64, t);
        v2 = b.NewValue0(v.Pos, OpRsh64x64, t);
        var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(63);
        v2.AddArg2(n, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(int64(64 - log64(c)));
        v1.AddArg2(v2, v4);
        v0.AddArg2(n, v1);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(int64(log64(c)));
        v.AddArg2(v0, v5);
        return true;

    } 
    // match: (Div64 <t> x (Const64 [c]))
    // cond: smagicOK64(c) && smagic64(c).m&1 == 0 && config.useHmul
    // result: (Sub64 <t> (Rsh64x64 <t> (Hmul64 <t> (Const64 <typ.UInt64> [int64(smagic64(c).m/2)]) x) (Const64 <typ.UInt64> [smagic64(c).s-1])) (Rsh64x64 <t> x (Const64 <typ.UInt64> [63])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(smagicOK64(c) && smagic64(c).m & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpSub64);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh64x64, t);
        v1 = b.NewValue0(v.Pos, OpHmul64, t);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(smagic64(c).m / 2));
        v1.AddArg2(v2, x);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(smagic64(c).s - 1);
        v0.AddArg2(v1, v3);
        v4 = b.NewValue0(v.Pos, OpRsh64x64, t);
        v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(63);
        v4.AddArg2(x, v5);
        v.AddArg2(v0, v4);
        return true;

    } 
    // match: (Div64 <t> x (Const64 [c]))
    // cond: smagicOK64(c) && smagic64(c).m&1 != 0 && config.useHmul
    // result: (Sub64 <t> (Rsh64x64 <t> (Add64 <t> (Hmul64 <t> (Const64 <typ.UInt64> [int64(smagic64(c).m)]) x) x) (Const64 <typ.UInt64> [smagic64(c).s])) (Rsh64x64 <t> x (Const64 <typ.UInt64> [63])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(smagicOK64(c) && smagic64(c).m & 1 != 0 && config.useHmul)) {
            break;
        }
        v.reset(OpSub64);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh64x64, t);
        v1 = b.NewValue0(v.Pos, OpAdd64, t);
        v2 = b.NewValue0(v.Pos, OpHmul64, t);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(int64(smagic64(c).m));
        v2.AddArg2(v3, x);
        v1.AddArg2(v2, x);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(smagic64(c).s);
        v0.AddArg2(v1, v4);
        v5 = b.NewValue0(v.Pos, OpRsh64x64, t);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(63);
        v5.AddArg2(x, v6);
        v.AddArg2(v0, v5);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Div64F (Const64F [c]) (Const64F [d]))
    // cond: c/d == c/d
    // result: (Const64F [c/d])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpConst64F) {
            break;
        }
        var d = auxIntToFloat64(v_1.AuxInt);
        if (!(c / d == c / d)) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(c / d);
        return true;

    } 
    // match: (Div64F x (Const64F <t> [c]))
    // cond: reciprocalExact64(c)
    // result: (Mul64F x (Const64F <t> [1/c]))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64F) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToFloat64(v_1.AuxInt);
        if (!(reciprocalExact64(c))) {
            break;
        }
        v.reset(OpMul64F);
        var v0 = b.NewValue0(v.Pos, OpConst64F, t);
        v0.AuxInt = float64ToAuxInt(1 / c);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv64u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div64u (Const64 [c]) (Const64 [d]))
    // cond: d != 0
    // result: (Const64 [int64(uint64(c)/uint64(d))])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint64(c) / uint64(d)));
        return true;

    } 
    // match: (Div64u n (Const64 [c]))
    // cond: isPowerOfTwo64(c)
    // result: (Rsh64Ux64 n (Const64 <typ.UInt64> [log64(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(isPowerOfTwo64(c))) {
            break;
        }
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log64(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div64u n (Const64 [-1<<63]))
    // result: (Rsh64Ux64 n (Const64 <typ.UInt64> [63]))
    while (true) {
        n = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != -1 << 63) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(63);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div64u x (Const64 [c]))
    // cond: c > 0 && c <= 0xFFFF && umagicOK32(int32(c)) && config.RegSize == 4 && config.useHmul
    // result: (Add64 (Add64 <typ.UInt64> (Add64 <typ.UInt64> (Lsh64x64 <typ.UInt64> (ZeroExt32to64 (Div32u <typ.UInt32> (Trunc64to32 <typ.UInt32> (Rsh64Ux64 <typ.UInt64> x (Const64 <typ.UInt64> [32]))) (Const32 <typ.UInt32> [int32(c)]))) (Const64 <typ.UInt64> [32])) (ZeroExt32to64 (Div32u <typ.UInt32> (Trunc64to32 <typ.UInt32> x) (Const32 <typ.UInt32> [int32(c)])))) (Mul64 <typ.UInt64> (ZeroExt32to64 <typ.UInt64> (Mod32u <typ.UInt32> (Trunc64to32 <typ.UInt32> (Rsh64Ux64 <typ.UInt64> x (Const64 <typ.UInt64> [32]))) (Const32 <typ.UInt32> [int32(c)]))) (Const64 <typ.UInt64> [int64((1<<32)/c)]))) (ZeroExt32to64 (Div32u <typ.UInt32> (Add32 <typ.UInt32> (Mod32u <typ.UInt32> (Trunc64to32 <typ.UInt32> x) (Const32 <typ.UInt32> [int32(c)])) (Mul32 <typ.UInt32> (Mod32u <typ.UInt32> (Trunc64to32 <typ.UInt32> (Rsh64Ux64 <typ.UInt64> x (Const64 <typ.UInt64> [32]))) (Const32 <typ.UInt32> [int32(c)])) (Const32 <typ.UInt32> [int32((1<<32)%c)]))) (Const32 <typ.UInt32> [int32(c)]))))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c > 0 && c <= 0xFFFF && umagicOK32(int32(c)) && config.RegSize == 4 && config.useHmul)) {
            break;
        }
        v.reset(OpAdd64);
        v0 = b.NewValue0(v.Pos, OpAdd64, typ.UInt64);
        var v1 = b.NewValue0(v.Pos, OpAdd64, typ.UInt64);
        var v2 = b.NewValue0(v.Pos, OpLsh64x64, typ.UInt64);
        var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        var v4 = b.NewValue0(v.Pos, OpDiv32u, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpTrunc64to32, typ.UInt32);
        var v6 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        var v7 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v7.AuxInt = int64ToAuxInt(32);
        v6.AddArg2(x, v7);
        v5.AddArg(v6);
        var v8 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v8.AuxInt = int32ToAuxInt(int32(c));
        v4.AddArg2(v5, v8);
        v3.AddArg(v4);
        v2.AddArg2(v3, v7);
        var v9 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        var v10 = b.NewValue0(v.Pos, OpDiv32u, typ.UInt32);
        var v11 = b.NewValue0(v.Pos, OpTrunc64to32, typ.UInt32);
        v11.AddArg(x);
        v10.AddArg2(v11, v8);
        v9.AddArg(v10);
        v1.AddArg2(v2, v9);
        var v12 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
        var v13 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        var v14 = b.NewValue0(v.Pos, OpMod32u, typ.UInt32);
        v14.AddArg2(v5, v8);
        v13.AddArg(v14);
        var v15 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v15.AuxInt = int64ToAuxInt(int64((1 << 32) / c));
        v12.AddArg2(v13, v15);
        v0.AddArg2(v1, v12);
        var v16 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
        var v17 = b.NewValue0(v.Pos, OpDiv32u, typ.UInt32);
        var v18 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
        var v19 = b.NewValue0(v.Pos, OpMod32u, typ.UInt32);
        v19.AddArg2(v11, v8);
        var v20 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        var v21 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v21.AuxInt = int32ToAuxInt(int32((1 << 32) % c));
        v20.AddArg2(v14, v21);
        v18.AddArg2(v19, v20);
        v17.AddArg2(v18, v8);
        v16.AddArg(v17);
        v.AddArg2(v0, v16);
        return true;

    } 
    // match: (Div64u x (Const64 [c]))
    // cond: umagicOK64(c) && config.RegSize == 8 && umagic64(c).m&1 == 0 && config.useHmul
    // result: (Rsh64Ux64 <typ.UInt64> (Hmul64u <typ.UInt64> (Const64 <typ.UInt64> [int64(1<<63+umagic64(c).m/2)]) x) (Const64 <typ.UInt64> [umagic64(c).s-1]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(umagicOK64(c) && config.RegSize == 8 && umagic64(c).m & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v.Type = typ.UInt64;
        v0 = b.NewValue0(v.Pos, OpHmul64u, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(int64(1 << 63 + umagic64(c).m / 2));
        v0.AddArg2(v1, x);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(umagic64(c).s - 1);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div64u x (Const64 [c]))
    // cond: umagicOK64(c) && config.RegSize == 8 && c&1 == 0 && config.useHmul
    // result: (Rsh64Ux64 <typ.UInt64> (Hmul64u <typ.UInt64> (Const64 <typ.UInt64> [int64(1<<63+(umagic64(c).m+1)/2)]) (Rsh64Ux64 <typ.UInt64> x (Const64 <typ.UInt64> [1]))) (Const64 <typ.UInt64> [umagic64(c).s-2]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(umagicOK64(c) && config.RegSize == 8 && c & 1 == 0 && config.useHmul)) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v.Type = typ.UInt64;
        v0 = b.NewValue0(v.Pos, OpHmul64u, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(int64(1 << 63 + (umagic64(c).m + 1) / 2));
        v2 = b.NewValue0(v.Pos, OpRsh64Ux64, typ.UInt64);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(1);
        v2.AddArg2(x, v3);
        v0.AddArg2(v1, v2);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(umagic64(c).s - 2);
        v.AddArg2(v0, v4);
        return true;

    } 
    // match: (Div64u x (Const64 [c]))
    // cond: umagicOK64(c) && config.RegSize == 8 && config.useAvg && config.useHmul
    // result: (Rsh64Ux64 <typ.UInt64> (Avg64u x (Hmul64u <typ.UInt64> (Const64 <typ.UInt64> [int64(umagic64(c).m)]) x)) (Const64 <typ.UInt64> [umagic64(c).s-1]))
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(umagicOK64(c) && config.RegSize == 8 && config.useAvg && config.useHmul)) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v.Type = typ.UInt64;
        v0 = b.NewValue0(v.Pos, OpAvg64u, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpHmul64u, typ.UInt64);
        v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(int64(umagic64(c).m));
        v1.AddArg2(v2, x);
        v0.AddArg2(x, v1);
        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(umagic64(c).s - 1);
        v.AddArg2(v0, v3);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 (Const8 [c]) (Const8 [d]))
    // cond: d != 0
    // result: (Const8 [c/d])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c / d);
        return true;

    } 
    // match: (Div8 n (Const8 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo8(c)
    // result: (Rsh8Ux64 n (Const64 <typ.UInt64> [log8(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo8(c))) {
            break;
        }
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log8(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div8 <t> n (Const8 [c]))
    // cond: c < 0 && c != -1<<7
    // result: (Neg8 (Div8 <t> n (Const8 <t> [-c])))
    while (true) {
        var t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 7)) {
            break;
        }
        v.reset(OpNeg8);
        v0 = b.NewValue0(v.Pos, OpDiv8, t);
        var v1 = b.NewValue0(v.Pos, OpConst8, t);
        v1.AuxInt = int8ToAuxInt(-c);
        v0.AddArg2(n, v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Div8 <t> x (Const8 [-1<<7 ]))
    // result: (Rsh8Ux64 (And8 <t> x (Neg8 <t> x)) (Const64 <typ.UInt64> [7 ]))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != -1 << 7) {
            break;
        }
        v.reset(OpRsh8Ux64);
        v0 = b.NewValue0(v.Pos, OpAnd8, t);
        v1 = b.NewValue0(v.Pos, OpNeg8, t);
        v1.AddArg(x);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(7);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Div8 <t> n (Const8 [c]))
    // cond: isPowerOfTwo8(c)
    // result: (Rsh8x64 (Add8 <t> n (Rsh8Ux64 <t> (Rsh8x64 <t> n (Const64 <typ.UInt64> [ 7])) (Const64 <typ.UInt64> [int64( 8-log8(c))]))) (Const64 <typ.UInt64> [int64(log8(c))]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(isPowerOfTwo8(c))) {
            break;
        }
        v.reset(OpRsh8x64);
        v0 = b.NewValue0(v.Pos, OpAdd8, t);
        v1 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
        v2 = b.NewValue0(v.Pos, OpRsh8x64, t);
        var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(7);
        v2.AddArg2(n, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(int64(8 - log8(c)));
        v1.AddArg2(v2, v4);
        v0.AddArg2(n, v1);
        var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v5.AuxInt = int64ToAuxInt(int64(log8(c)));
        v.AddArg2(v0, v5);
        return true;

    } 
    // match: (Div8 <t> x (Const8 [c]))
    // cond: smagicOK8(c)
    // result: (Sub8 <t> (Rsh32x64 <t> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(smagic8(c).m)]) (SignExt8to32 x)) (Const64 <typ.UInt64> [8+smagic8(c).s])) (Rsh32x64 <t> (SignExt8to32 x) (Const64 <typ.UInt64> [31])))
    while (true) {
        t = v.Type;
        x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(smagicOK8(c))) {
            break;
        }
        v.reset(OpSub8);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpRsh32x64, t);
        v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(smagic8(c).m));
        v3 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(8 + smagic8(c).s);
        v0.AddArg2(v1, v4);
        v5 = b.NewValue0(v.Pos, OpRsh32x64, t);
        var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v6.AuxInt = int64ToAuxInt(31);
        v5.AddArg2(v3, v6);
        v.AddArg2(v0, v5);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u (Const8 [c]) (Const8 [d]))
    // cond: d != 0
    // result: (Const8 [int8(uint8(c)/uint8(d))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(uint8(c) / uint8(d)));
        return true;

    } 
    // match: (Div8u n (Const8 [c]))
    // cond: isPowerOfTwo8(c)
    // result: (Rsh8Ux64 n (Const64 <typ.UInt64> [log8(c)]))
    while (true) {
        var n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(isPowerOfTwo8(c))) {
            break;
        }
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(log8(c));
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Div8u x (Const8 [c]))
    // cond: umagicOK8(c)
    // result: (Trunc32to8 (Rsh32Ux64 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(1<<8+umagic8(c).m)]) (ZeroExt8to32 x)) (Const64 <typ.UInt64> [8+umagic8(c).s])))
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(umagicOK8(c))) {
            break;
        }
        v.reset(OpTrunc32to8);
        v0 = b.NewValue0(v.Pos, OpRsh32Ux64, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(int32(1 << 8 + umagic8(c).m));
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v3.AddArg(x);
        v1.AddArg2(v2, v3);
        var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v4.AuxInt = int64ToAuxInt(8 + umagic8(c).s);
        v0.AddArg2(v1, v4);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (Eq16 (Const16 <t> [c]) (Add16 (Const16 <t> [d]) x))
    // result: (Eq16 (Const16 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpAdd16) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpEq16);
                        var v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 (Mod16u x (Const16 [c])) (Const16 [0]))
    // cond: x.Op != OpConst16 && udivisibleOK16(c) && !hasSmallRotate(config)
    // result: (Eq32 (Mod32u <typ.UInt32> (ZeroExt16to32 <typ.UInt32> x) (Const32 <typ.UInt32> [int32(uint16(c))])) (Const32 <typ.UInt32> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMod16u) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                x = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_1.AuxInt);
                if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != 0 || !(x.Op != OpConst16 && udivisibleOK16(c) && !hasSmallRotate(config))) {
                    continue;
                }

                v.reset(OpEq32);
                v0 = b.NewValue0(v.Pos, OpMod32u, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v2.AuxInt = int32ToAuxInt(int32(uint16(c)));
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v3.AuxInt = int32ToAuxInt(0);
                v.AddArg2(v0, v3);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 (Mod16 x (Const16 [c])) (Const16 [0]))
    // cond: x.Op != OpConst16 && sdivisibleOK16(c) && !hasSmallRotate(config)
    // result: (Eq32 (Mod32 <typ.Int32> (SignExt16to32 <typ.Int32> x) (Const32 <typ.Int32> [int32(c)])) (Const32 <typ.Int32> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMod16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_1.AuxInt);
                if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != 0 || !(x.Op != OpConst16 && sdivisibleOK16(c) && !hasSmallRotate(config))) {
                    continue;
                }

                v.reset(OpEq32);
                v0 = b.NewValue0(v.Pos, OpMod32, typ.Int32);
                v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                v2 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
                v2.AuxInt = int32ToAuxInt(int32(c));
                v0.AddArg2(v1, v2);
                v3 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
                v3.AuxInt = int32ToAuxInt(0);
                v.AddArg2(v0, v3);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 x (Mul16 (Const16 [c]) (Trunc64to16 (Rsh64Ux64 mul:(Mul64 (Const64 [m]) (ZeroExt16to64 x)) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1<<16+umagic16(c).m) && s == 16+umagic16(c).s && x.Op != OpConst16 && udivisibleOK16(c)
    // result: (Leq16U (RotateLeft16 <typ.UInt16> (Mul16 <typ.UInt16> (Const16 <typ.UInt16> [int16(udivisible16(c).m)]) x) (Const16 <typ.UInt16> [int16(16-udivisible16(c).k)]) ) (Const16 <typ.UInt16> [int16(udivisible16(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt16(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc64to16) {
                            continue;
                        }

                        var v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        var mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        var mul_0 = mul.Args[0];
                        var mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                var m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt16to64 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                var v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                var s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1 << 16 + umagic16(c).m) && s == 16 + umagic16(c).s && x.Op != OpConst16 && udivisibleOK16(c))) {
                                    continue;
                                }

                                v.reset(OpLeq16U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft16, typ.UInt16);
                                v1 = b.NewValue0(v.Pos, OpMul16, typ.UInt16);
                                v2 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v2.AuxInt = int16ToAuxInt(int16(udivisible16(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v3.AuxInt = int16ToAuxInt(int16(16 - udivisible16(c).k));
                                v0.AddArg2(v1, v3);
                                var v4 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v4.AuxInt = int16ToAuxInt(int16(udivisible16(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 x (Mul16 (Const16 [c]) (Trunc32to16 (Rsh32Ux64 mul:(Mul32 (Const32 [m]) (ZeroExt16to32 x)) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1<<15+umagic16(c).m/2) && s == 16+umagic16(c).s-1 && x.Op != OpConst16 && udivisibleOK16(c)
    // result: (Leq16U (RotateLeft16 <typ.UInt16> (Mul16 <typ.UInt16> (Const16 <typ.UInt16> [int16(udivisible16(c).m)]) x) (Const16 <typ.UInt16> [int16(16-udivisible16(c).k)]) ) (Const16 <typ.UInt16> [int16(udivisible16(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt16(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc32to16) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt16to32 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1 << 15 + umagic16(c).m / 2) && s == 16 + umagic16(c).s - 1 && x.Op != OpConst16 && udivisibleOK16(c))) {
                                    continue;
                                }

                                v.reset(OpLeq16U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft16, typ.UInt16);
                                v1 = b.NewValue0(v.Pos, OpMul16, typ.UInt16);
                                v2 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v2.AuxInt = int16ToAuxInt(int16(udivisible16(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v3.AuxInt = int16ToAuxInt(int16(16 - udivisible16(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v4.AuxInt = int16ToAuxInt(int16(udivisible16(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 x (Mul16 (Const16 [c]) (Trunc32to16 (Rsh32Ux64 mul:(Mul32 (Const32 [m]) (Rsh32Ux64 (ZeroExt16to32 x) (Const64 [1]))) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1<<15+(umagic16(c).m+1)/2) && s == 16+umagic16(c).s-2 && x.Op != OpConst16 && udivisibleOK16(c)
    // result: (Leq16U (RotateLeft16 <typ.UInt16> (Mul16 <typ.UInt16> (Const16 <typ.UInt16> [int16(udivisible16(c).m)]) x) (Const16 <typ.UInt16> [int16(16-udivisible16(c).k)]) ) (Const16 <typ.UInt16> [int16(udivisible16(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt16(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc32to16) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpRsh32Ux64) {
                                    continue;
                                }

                                _ = mul_1.Args[1];
                                var mul_1_0 = mul_1.Args[0];
                                if (mul_1_0.Op != OpZeroExt16to32 || x != mul_1_0.Args[0]) {
                                    continue;
                                }

                                var mul_1_1 = mul_1.Args[1];
                                if (mul_1_1.Op != OpConst64 || auxIntToInt64(mul_1_1.AuxInt) != 1) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1 << 15 + (umagic16(c).m + 1) / 2) && s == 16 + umagic16(c).s - 2 && x.Op != OpConst16 && udivisibleOK16(c))) {
                                    continue;
                                }

                                v.reset(OpLeq16U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft16, typ.UInt16);
                                v1 = b.NewValue0(v.Pos, OpMul16, typ.UInt16);
                                v2 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v2.AuxInt = int16ToAuxInt(int16(udivisible16(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v3.AuxInt = int16ToAuxInt(int16(16 - udivisible16(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v4.AuxInt = int16ToAuxInt(int16(udivisible16(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 x (Mul16 (Const16 [c]) (Trunc32to16 (Rsh32Ux64 (Avg32u (Lsh32x64 (ZeroExt16to32 x) (Const64 [16])) mul:(Mul32 (Const32 [m]) (ZeroExt16to32 x))) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(umagic16(c).m) && s == 16+umagic16(c).s-1 && x.Op != OpConst16 && udivisibleOK16(c)
    // result: (Leq16U (RotateLeft16 <typ.UInt16> (Mul16 <typ.UInt16> (Const16 <typ.UInt16> [int16(udivisible16(c).m)]) x) (Const16 <typ.UInt16> [int16(16-udivisible16(c).k)]) ) (Const16 <typ.UInt16> [int16(udivisible16(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt16(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc32to16) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        var v_1_1_0_0 = v_1_1_0.Args[0];
                        if (v_1_1_0_0.Op != OpAvg32u) {
                            continue;
                        }

                        _ = v_1_1_0_0.Args[1];
                        var v_1_1_0_0_0 = v_1_1_0_0.Args[0];
                        if (v_1_1_0_0_0.Op != OpLsh32x64) {
                            continue;
                        }

                        _ = v_1_1_0_0_0.Args[1];
                        var v_1_1_0_0_0_0 = v_1_1_0_0_0.Args[0];
                        if (v_1_1_0_0_0_0.Op != OpZeroExt16to32 || x != v_1_1_0_0_0_0.Args[0]) {
                            continue;
                        }

                        var v_1_1_0_0_0_1 = v_1_1_0_0_0.Args[1];
                        if (v_1_1_0_0_0_1.Op != OpConst64 || auxIntToInt64(v_1_1_0_0_0_1.AuxInt) != 16) {
                            continue;
                        }

                        mul = v_1_1_0_0.Args[1];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt16to32 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(umagic16(c).m) && s == 16 + umagic16(c).s - 1 && x.Op != OpConst16 && udivisibleOK16(c))) {
                                    continue;
                                }

                                v.reset(OpLeq16U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft16, typ.UInt16);
                                v1 = b.NewValue0(v.Pos, OpMul16, typ.UInt16);
                                v2 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v2.AuxInt = int16ToAuxInt(int16(udivisible16(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v3.AuxInt = int16ToAuxInt(int16(16 - udivisible16(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v4.AuxInt = int16ToAuxInt(int16(udivisible16(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 x (Mul16 (Const16 [c]) (Sub16 (Rsh32x64 mul:(Mul32 (Const32 [m]) (SignExt16to32 x)) (Const64 [s])) (Rsh32x64 (SignExt16to32 x) (Const64 [31]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic16(c).m) && s == 16+smagic16(c).s && x.Op != OpConst16 && sdivisibleOK16(c)
    // result: (Leq16U (RotateLeft16 <typ.UInt16> (Add16 <typ.UInt16> (Mul16 <typ.UInt16> (Const16 <typ.UInt16> [int16(sdivisible16(c).m)]) x) (Const16 <typ.UInt16> [int16(sdivisible16(c).a)]) ) (Const16 <typ.UInt16> [int16(16-sdivisible16(c).k)]) ) (Const16 <typ.UInt16> [int16(sdivisible16(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt16(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub16) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpSignExt16to32 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                var v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpRsh32x64) {
                                    continue;
                                }

                                _ = v_1_1_1.Args[1];
                                var v_1_1_1_0 = v_1_1_1.Args[0];
                                if (v_1_1_1_0.Op != OpSignExt16to32 || x != v_1_1_1_0.Args[0]) {
                                    continue;
                                }

                                var v_1_1_1_1 = v_1_1_1.Args[1];
                                if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 31 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic16(c).m) && s == 16 + smagic16(c).s && x.Op != OpConst16 && sdivisibleOK16(c))) {
                                    continue;
                                }

                                v.reset(OpLeq16U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft16, typ.UInt16);
                                v1 = b.NewValue0(v.Pos, OpAdd16, typ.UInt16);
                                v2 = b.NewValue0(v.Pos, OpMul16, typ.UInt16);
                                v3 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v3.AuxInt = int16ToAuxInt(int16(sdivisible16(c).m));
                                v2.AddArg2(v3, x);
                                v4 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v4.AuxInt = int16ToAuxInt(int16(sdivisible16(c).a));
                                v1.AddArg2(v2, v4);
                                var v5 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v5.AuxInt = int16ToAuxInt(int16(16 - sdivisible16(c).k));
                                v0.AddArg2(v1, v5);
                                var v6 = b.NewValue0(v.Pos, OpConst16, typ.UInt16);
                                v6.AuxInt = int16ToAuxInt(int16(sdivisible16(c).max));
                                v.AddArg2(v0, v6);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 n (Lsh16x64 (Rsh16x64 (Add16 <t> n (Rsh16Ux64 <t> (Rsh16x64 <t> n (Const64 <typ.UInt64> [15])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 15 && kbar == 16 - k
    // result: (Eq16 (And16 <t> n (Const16 <t> [1<<uint(k)-1])) (Const16 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh16x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh16x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd16) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh16Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh16x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 15) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 15 && kbar == 16 - k)) {
                            continue;
                        }

                        v.reset(OpEq16);
                        v0 = b.NewValue0(v.Pos, OpAnd16, t);
                        v1 = b.NewValue0(v.Pos, OpConst16, t);
                        v1.AuxInt = int16ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        v2 = b.NewValue0(v.Pos, OpConst16, t);
                        v2.AuxInt = int16ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 s:(Sub16 x y) (Const16 [0]))
    // cond: s.Uses == 1
    // result: (Eq16 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s = v_0;
                if (s.Op != OpSub16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpEq16);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq16 (And16 <t> x (Const16 <t> [y])) (Const16 <t> [y]))
    // cond: oneBit16(y)
    // result: (Neq16 (And16 <t> x (Const16 <t> [y])) (Const16 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst16 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt16(v_0_1.AuxInt);
                        if (v_1.Op != OpConst16 || v_1.Type != t || auxIntToInt16(v_1.AuxInt) != y || !(oneBit16(y))) {
                            continue;
                        }

                        v.reset(OpNeq16);
                        v0 = b.NewValue0(v.Pos, OpAnd16, t);
                        v1 = b.NewValue0(v.Pos, OpConst16, t);
                        v1.AuxInt = int16ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst16, t);
                        v2.AuxInt = int16ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq32 x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (Eq32 (Const32 <t> [c]) (Add32 (Const32 <t> [d]) x))
    // result: (Eq32 (Const32 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpAdd32) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpEq32);
                        var v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Rsh32Ux64 mul:(Hmul32u (Const32 [m]) x) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1<<31+umagic32(c).m/2) && s == umagic32(c).s-1 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        var mul = v_1_1.Args[0];
                        if (mul.Op != OpHmul32u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        var mul_0 = mul.Args[0];
                        var mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                var m = auxIntToInt32(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                var v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                var s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1 << 31 + umagic32(c).m / 2) && s == umagic32(c).s - 1 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                var v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                var v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                var v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                var v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Rsh32Ux64 mul:(Hmul32u (Const32 <typ.UInt32> [m]) (Rsh32Ux64 x (Const64 [1]))) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1<<31+(umagic32(c).m+1)/2) && s == umagic32(c).s-2 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        mul = v_1_1.Args[0];
                        if (mul.Op != OpHmul32u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32 || mul_0.Type != typ.UInt32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpRsh32Ux64) {
                                    continue;
                                }

                                _ = mul_1.Args[1];
                                if (x != mul_1.Args[0]) {
                                    continue;
                                }

                                var mul_1_1 = mul_1.Args[1];
                                if (mul_1_1.Op != OpConst64 || auxIntToInt64(mul_1_1.AuxInt) != 1) {
                                    continue;
                                }

                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1 << 31 + (umagic32(c).m + 1) / 2) && s == umagic32(c).s - 2 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Rsh32Ux64 (Avg32u x mul:(Hmul32u (Const32 [m]) x)) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(umagic32(c).m) && s == umagic32(c).s-1 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        var v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpAvg32u) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        if (x != v_1_1_0.Args[0]) {
                            continue;
                        }

                        mul = v_1_1_0.Args[1];
                        if (mul.Op != OpHmul32u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(umagic32(c).m) && s == umagic32(c).s - 1 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Trunc64to32 (Rsh64Ux64 mul:(Mul64 (Const64 [m]) (ZeroExt32to64 x)) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1<<31+umagic32(c).m/2) && s == 32+umagic32(c).s-1 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc64to32) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt32to64 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                var v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1 << 31 + umagic32(c).m / 2) && s == 32 + umagic32(c).s - 1 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Trunc64to32 (Rsh64Ux64 mul:(Mul64 (Const64 [m]) (Rsh64Ux64 (ZeroExt32to64 x) (Const64 [1]))) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1<<31+(umagic32(c).m+1)/2) && s == 32+umagic32(c).s-2 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc64to32) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpRsh64Ux64) {
                                    continue;
                                }

                                _ = mul_1.Args[1];
                                var mul_1_0 = mul_1.Args[0];
                                if (mul_1_0.Op != OpZeroExt32to64 || x != mul_1_0.Args[0]) {
                                    continue;
                                }

                                mul_1_1 = mul_1.Args[1];
                                if (mul_1_1.Op != OpConst64 || auxIntToInt64(mul_1_1.AuxInt) != 1) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1 << 31 + (umagic32(c).m + 1) / 2) && s == 32 + umagic32(c).s - 2 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Trunc64to32 (Rsh64Ux64 (Avg64u (Lsh64x64 (ZeroExt32to64 x) (Const64 [32])) mul:(Mul64 (Const64 [m]) (ZeroExt32to64 x))) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(umagic32(c).m) && s == 32+umagic32(c).s-1 && x.Op != OpConst32 && udivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(udivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(32-udivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(udivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc64to32) {
                            continue;
                        }

                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        var v_1_1_0_0 = v_1_1_0.Args[0];
                        if (v_1_1_0_0.Op != OpAvg64u) {
                            continue;
                        }

                        _ = v_1_1_0_0.Args[1];
                        var v_1_1_0_0_0 = v_1_1_0_0.Args[0];
                        if (v_1_1_0_0_0.Op != OpLsh64x64) {
                            continue;
                        }

                        _ = v_1_1_0_0_0.Args[1];
                        var v_1_1_0_0_0_0 = v_1_1_0_0_0.Args[0];
                        if (v_1_1_0_0_0_0.Op != OpZeroExt32to64 || x != v_1_1_0_0_0_0.Args[0]) {
                            continue;
                        }

                        var v_1_1_0_0_0_1 = v_1_1_0_0_0.Args[1];
                        if (v_1_1_0_0_0_1.Op != OpConst64 || auxIntToInt64(v_1_1_0_0_0_1.AuxInt) != 32) {
                            continue;
                        }

                        mul = v_1_1_0_0.Args[1];
                        if (mul.Op != OpMul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt32to64 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(umagic32(c).m) && s == 32 + umagic32(c).s - 1 && x.Op != OpConst32 && udivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v2.AuxInt = int32ToAuxInt(int32(udivisible32(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(32 - udivisible32(c).k));
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(udivisible32(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Sub32 (Rsh64x64 mul:(Mul64 (Const64 [m]) (SignExt32to64 x)) (Const64 [s])) (Rsh64x64 (SignExt32to64 x) (Const64 [63]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic32(c).m) && s == 32+smagic32(c).s && x.Op != OpConst32 && sdivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Add32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(sdivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(sdivisible32(c).a)]) ) (Const32 <typ.UInt32> [int32(32-sdivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(sdivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub32) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpSignExt32to64 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpRsh64x64) {
                                    continue;
                                }

                                _ = v_1_1_1.Args[1];
                                var v_1_1_1_0 = v_1_1_1.Args[0];
                                if (v_1_1_1_0.Op != OpSignExt32to64 || x != v_1_1_1_0.Args[0]) {
                                    continue;
                                }

                                var v_1_1_1_1 = v_1_1_1.Args[1];
                                if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 63 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic32(c).m) && s == 32 + smagic32(c).s && x.Op != OpConst32 && sdivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(sdivisible32(c).m));
                                v2.AddArg2(v3, x);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(sdivisible32(c).a));
                                v1.AddArg2(v2, v4);
                                var v5 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v5.AuxInt = int32ToAuxInt(int32(32 - sdivisible32(c).k));
                                v0.AddArg2(v1, v5);
                                var v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v6.AuxInt = int32ToAuxInt(int32(sdivisible32(c).max));
                                v.AddArg2(v0, v6);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Sub32 (Rsh32x64 mul:(Hmul32 (Const32 [m]) x) (Const64 [s])) (Rsh32x64 x (Const64 [31]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic32(c).m/2) && s == smagic32(c).s-1 && x.Op != OpConst32 && sdivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Add32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(sdivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(sdivisible32(c).a)]) ) (Const32 <typ.UInt32> [int32(32-sdivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(sdivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub32) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpHmul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpRsh32x64) {
                                    continue;
                                }

                                _ = v_1_1_1.Args[1];
                                if (x != v_1_1_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_1_1 = v_1_1_1.Args[1];
                                if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 31 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic32(c).m / 2) && s == smagic32(c).s - 1 && x.Op != OpConst32 && sdivisibleOK32(c))) {
                                    continue;
                                }

                                v.reset(OpLeq32U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                v1 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
                                v2 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v3.AuxInt = int32ToAuxInt(int32(sdivisible32(c).m));
                                v2.AddArg2(v3, x);
                                v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v4.AuxInt = int32ToAuxInt(int32(sdivisible32(c).a));
                                v1.AddArg2(v2, v4);
                                v5 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v5.AuxInt = int32ToAuxInt(int32(32 - sdivisible32(c).k));
                                v0.AddArg2(v1, v5);
                                v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                v6.AuxInt = int32ToAuxInt(int32(sdivisible32(c).max));
                                v.AddArg2(v0, v6);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 x (Mul32 (Const32 [c]) (Sub32 (Rsh32x64 (Add32 mul:(Hmul32 (Const32 [m]) x) x) (Const64 [s])) (Rsh32x64 x (Const64 [31]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic32(c).m) && s == smagic32(c).s && x.Op != OpConst32 && sdivisibleOK32(c)
    // result: (Leq32U (RotateLeft32 <typ.UInt32> (Add32 <typ.UInt32> (Mul32 <typ.UInt32> (Const32 <typ.UInt32> [int32(sdivisible32(c).m)]) x) (Const32 <typ.UInt32> [int32(sdivisible32(c).a)]) ) (Const32 <typ.UInt32> [int32(32-sdivisible32(c).k)]) ) (Const32 <typ.UInt32> [int32(sdivisible32(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt32(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub32) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        v_1_1_0_0 = v_1_1_0.Args[0];
                        if (v_1_1_0_0.Op != OpAdd32) {
                            continue;
                        }

                        _ = v_1_1_0_0.Args[1];
                        v_1_1_0_0_0 = v_1_1_0_0.Args[0];
                        var v_1_1_0_0_1 = v_1_1_0_0.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                mul = v_1_1_0_0_0;
                                if (mul.Op != OpHmul32) {
                                    continue;
                                (_i2, v_1_1_0_0_0, v_1_1_0_0_1) = (_i2 + 1, v_1_1_0_0_1, v_1_1_0_0_0);
                                }

                                _ = mul.Args[1];
                                mul_0 = mul.Args[0];
                                mul_1 = mul.Args[1];
                                {
                                    nint _i3 = 0;

                                    while (_i3 <= 1) {
                                        if (mul_0.Op != OpConst32) {
                                            continue;
                                        (_i3, mul_0, mul_1) = (_i3 + 1, mul_1, mul_0);
                                        }

                                        m = auxIntToInt32(mul_0.AuxInt);
                                        if (x != mul_1 || x != v_1_1_0_0_1) {
                                            continue;
                                        }

                                        v_1_1_0_1 = v_1_1_0.Args[1];
                                        if (v_1_1_0_1.Op != OpConst64) {
                                            continue;
                                        }

                                        s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                        v_1_1_1 = v_1_1.Args[1];
                                        if (v_1_1_1.Op != OpRsh32x64) {
                                            continue;
                                        }

                                        _ = v_1_1_1.Args[1];
                                        if (x != v_1_1_1.Args[0]) {
                                            continue;
                                        }

                                        v_1_1_1_1 = v_1_1_1.Args[1];
                                        if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 31 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic32(c).m) && s == smagic32(c).s && x.Op != OpConst32 && sdivisibleOK32(c))) {
                                            continue;
                                        }

                                        v.reset(OpLeq32U);
                                        v0 = b.NewValue0(v.Pos, OpRotateLeft32, typ.UInt32);
                                        v1 = b.NewValue0(v.Pos, OpAdd32, typ.UInt32);
                                        v2 = b.NewValue0(v.Pos, OpMul32, typ.UInt32);
                                        v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                        v3.AuxInt = int32ToAuxInt(int32(sdivisible32(c).m));
                                        v2.AddArg2(v3, x);
                                        v4 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                        v4.AuxInt = int32ToAuxInt(int32(sdivisible32(c).a));
                                        v1.AddArg2(v2, v4);
                                        v5 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                        v5.AuxInt = int32ToAuxInt(int32(32 - sdivisible32(c).k));
                                        v0.AddArg2(v1, v5);
                                        v6 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                                        v6.AuxInt = int32ToAuxInt(int32(sdivisible32(c).max));
                                        v.AddArg2(v0, v6);
                                        return true;

                                    }

                                }

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 n (Lsh32x64 (Rsh32x64 (Add32 <t> n (Rsh32Ux64 <t> (Rsh32x64 <t> n (Const64 <typ.UInt64> [31])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 31 && kbar == 32 - k
    // result: (Eq32 (And32 <t> n (Const32 <t> [1<<uint(k)-1])) (Const32 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh32x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh32x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd32) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh32Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh32x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 31) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 31 && kbar == 32 - k)) {
                            continue;
                        }

                        v.reset(OpEq32);
                        v0 = b.NewValue0(v.Pos, OpAnd32, t);
                        v1 = b.NewValue0(v.Pos, OpConst32, t);
                        v1.AuxInt = int32ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        v2 = b.NewValue0(v.Pos, OpConst32, t);
                        v2.AuxInt = int32ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 s:(Sub32 x y) (Const32 [0]))
    // cond: s.Uses == 1
    // result: (Eq32 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s = v_0;
                if (s.Op != OpSub32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst32 || auxIntToInt32(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpEq32);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq32 (And32 <t> x (Const32 <t> [y])) (Const32 <t> [y]))
    // cond: oneBit32(y)
    // result: (Neq32 (And32 <t> x (Const32 <t> [y])) (Const32 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst32 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt32(v_0_1.AuxInt);
                        if (v_1.Op != OpConst32 || v_1.Type != t || auxIntToInt32(v_1.AuxInt) != y || !(oneBit32(y))) {
                            continue;
                        }

                        v.reset(OpNeq32);
                        v0 = b.NewValue0(v.Pos, OpAnd32, t);
                        v1 = b.NewValue0(v.Pos, OpConst32, t);
                        v1.AuxInt = int32ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst32, t);
                        v2.AuxInt = int32ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Eq32F (Const32F [c]) (Const32F [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat32(v_0.AuxInt);
                if (v_1.Op != OpConst32F) {
                    continue;
                }

                var d = auxIntToFloat32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq64 x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (Eq64 (Const64 <t> [c]) (Add64 (Const64 <t> [d]) x))
    // result: (Eq64 (Const64 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpAdd64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpEq64);
                        var v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 x (Mul64 (Const64 [c]) (Rsh64Ux64 mul:(Hmul64u (Const64 [m]) x) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1<<63+umagic64(c).m/2) && s == umagic64(c).s-1 && x.Op != OpConst64 && udivisibleOK64(c)
    // result: (Leq64U (RotateLeft64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(udivisible64(c).m)]) x) (Const64 <typ.UInt64> [64-udivisible64(c).k]) ) (Const64 <typ.UInt64> [int64(udivisible64(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt64(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        var mul = v_1_1.Args[0];
                        if (mul.Op != OpHmul64u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        var mul_0 = mul.Args[0];
                        var mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                var m = auxIntToInt64(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                var v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                var s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1 << 63 + umagic64(c).m / 2) && s == umagic64(c).s - 1 && x.Op != OpConst64 && udivisibleOK64(c))) {
                                    continue;
                                }

                                v.reset(OpLeq64U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft64, typ.UInt64);
                                var v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
                                var v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v2.AuxInt = int64ToAuxInt(int64(udivisible64(c).m));
                                v1.AddArg2(v2, x);
                                var v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v3.AuxInt = int64ToAuxInt(64 - udivisible64(c).k);
                                v0.AddArg2(v1, v3);
                                var v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v4.AuxInt = int64ToAuxInt(int64(udivisible64(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 x (Mul64 (Const64 [c]) (Rsh64Ux64 mul:(Hmul64u (Const64 [m]) (Rsh64Ux64 x (Const64 [1]))) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1<<63+(umagic64(c).m+1)/2) && s == umagic64(c).s-2 && x.Op != OpConst64 && udivisibleOK64(c)
    // result: (Leq64U (RotateLeft64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(udivisible64(c).m)]) x) (Const64 <typ.UInt64> [64-udivisible64(c).k]) ) (Const64 <typ.UInt64> [int64(udivisible64(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt64(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        mul = v_1_1.Args[0];
                        if (mul.Op != OpHmul64u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (mul_1.Op != OpRsh64Ux64) {
                                    continue;
                                }

                                _ = mul_1.Args[1];
                                if (x != mul_1.Args[0]) {
                                    continue;
                                }

                                var mul_1_1 = mul_1.Args[1];
                                if (mul_1_1.Op != OpConst64 || auxIntToInt64(mul_1_1.AuxInt) != 1) {
                                    continue;
                                }

                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(1 << 63 + (umagic64(c).m + 1) / 2) && s == umagic64(c).s - 2 && x.Op != OpConst64 && udivisibleOK64(c))) {
                                    continue;
                                }

                                v.reset(OpLeq64U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft64, typ.UInt64);
                                v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
                                v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v2.AuxInt = int64ToAuxInt(int64(udivisible64(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v3.AuxInt = int64ToAuxInt(64 - udivisible64(c).k);
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v4.AuxInt = int64ToAuxInt(int64(udivisible64(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 x (Mul64 (Const64 [c]) (Rsh64Ux64 (Avg64u x mul:(Hmul64u (Const64 [m]) x)) (Const64 [s])) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(umagic64(c).m) && s == umagic64(c).s-1 && x.Op != OpConst64 && udivisibleOK64(c)
    // result: (Leq64U (RotateLeft64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(udivisible64(c).m)]) x) (Const64 <typ.UInt64> [64-udivisible64(c).k]) ) (Const64 <typ.UInt64> [int64(udivisible64(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt64(v_1_0.AuxInt);
                        if (v_1_1.Op != OpRsh64Ux64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        var v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpAvg64u) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        if (x != v_1_1_0.Args[0]) {
                            continue;
                        }

                        mul = v_1_1_0.Args[1];
                        if (mul.Op != OpHmul64u) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(umagic64(c).m) && s == umagic64(c).s - 1 && x.Op != OpConst64 && udivisibleOK64(c))) {
                                    continue;
                                }

                                v.reset(OpLeq64U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft64, typ.UInt64);
                                v1 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
                                v2 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v2.AuxInt = int64ToAuxInt(int64(udivisible64(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v3.AuxInt = int64ToAuxInt(64 - udivisible64(c).k);
                                v0.AddArg2(v1, v3);
                                v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v4.AuxInt = int64ToAuxInt(int64(udivisible64(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 x (Mul64 (Const64 [c]) (Sub64 (Rsh64x64 mul:(Hmul64 (Const64 [m]) x) (Const64 [s])) (Rsh64x64 x (Const64 [63]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic64(c).m/2) && s == smagic64(c).s-1 && x.Op != OpConst64 && sdivisibleOK64(c)
    // result: (Leq64U (RotateLeft64 <typ.UInt64> (Add64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(sdivisible64(c).m)]) x) (Const64 <typ.UInt64> [int64(sdivisible64(c).a)]) ) (Const64 <typ.UInt64> [64-sdivisible64(c).k]) ) (Const64 <typ.UInt64> [int64(sdivisible64(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt64(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpHmul64) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst64) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt64(mul_0.AuxInt);
                                if (x != mul_1) {
                                    continue;
                                }

                                var v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpRsh64x64) {
                                    continue;
                                }

                                _ = v_1_1_1.Args[1];
                                if (x != v_1_1_1.Args[0]) {
                                    continue;
                                }

                                var v_1_1_1_1 = v_1_1_1.Args[1];
                                if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 63 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic64(c).m / 2) && s == smagic64(c).s - 1 && x.Op != OpConst64 && sdivisibleOK64(c))) {
                                    continue;
                                }

                                v.reset(OpLeq64U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft64, typ.UInt64);
                                v1 = b.NewValue0(v.Pos, OpAdd64, typ.UInt64);
                                v2 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
                                v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v3.AuxInt = int64ToAuxInt(int64(sdivisible64(c).m));
                                v2.AddArg2(v3, x);
                                v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v4.AuxInt = int64ToAuxInt(int64(sdivisible64(c).a));
                                v1.AddArg2(v2, v4);
                                var v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v5.AuxInt = int64ToAuxInt(64 - sdivisible64(c).k);
                                v0.AddArg2(v1, v5);
                                var v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                v6.AuxInt = int64ToAuxInt(int64(sdivisible64(c).max));
                                v.AddArg2(v0, v6);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 x (Mul64 (Const64 [c]) (Sub64 (Rsh64x64 (Add64 mul:(Hmul64 (Const64 [m]) x) x) (Const64 [s])) (Rsh64x64 x (Const64 [63]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic64(c).m) && s == smagic64(c).s && x.Op != OpConst64 && sdivisibleOK64(c)
    // result: (Leq64U (RotateLeft64 <typ.UInt64> (Add64 <typ.UInt64> (Mul64 <typ.UInt64> (Const64 <typ.UInt64> [int64(sdivisible64(c).m)]) x) (Const64 <typ.UInt64> [int64(sdivisible64(c).a)]) ) (Const64 <typ.UInt64> [64-sdivisible64(c).k]) ) (Const64 <typ.UInt64> [int64(sdivisible64(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt64(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub64) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh64x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        var v_1_1_0_0 = v_1_1_0.Args[0];
                        if (v_1_1_0_0.Op != OpAdd64) {
                            continue;
                        }

                        _ = v_1_1_0_0.Args[1];
                        var v_1_1_0_0_0 = v_1_1_0_0.Args[0];
                        var v_1_1_0_0_1 = v_1_1_0_0.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                mul = v_1_1_0_0_0;
                                if (mul.Op != OpHmul64) {
                                    continue;
                                (_i2, v_1_1_0_0_0, v_1_1_0_0_1) = (_i2 + 1, v_1_1_0_0_1, v_1_1_0_0_0);
                                }

                                _ = mul.Args[1];
                                mul_0 = mul.Args[0];
                                mul_1 = mul.Args[1];
                                {
                                    nint _i3 = 0;

                                    while (_i3 <= 1) {
                                        if (mul_0.Op != OpConst64) {
                                            continue;
                                        (_i3, mul_0, mul_1) = (_i3 + 1, mul_1, mul_0);
                                        }

                                        m = auxIntToInt64(mul_0.AuxInt);
                                        if (x != mul_1 || x != v_1_1_0_0_1) {
                                            continue;
                                        }

                                        v_1_1_0_1 = v_1_1_0.Args[1];
                                        if (v_1_1_0_1.Op != OpConst64) {
                                            continue;
                                        }

                                        s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                        v_1_1_1 = v_1_1.Args[1];
                                        if (v_1_1_1.Op != OpRsh64x64) {
                                            continue;
                                        }

                                        _ = v_1_1_1.Args[1];
                                        if (x != v_1_1_1.Args[0]) {
                                            continue;
                                        }

                                        v_1_1_1_1 = v_1_1_1.Args[1];
                                        if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 63 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int64(smagic64(c).m) && s == smagic64(c).s && x.Op != OpConst64 && sdivisibleOK64(c))) {
                                            continue;
                                        }

                                        v.reset(OpLeq64U);
                                        v0 = b.NewValue0(v.Pos, OpRotateLeft64, typ.UInt64);
                                        v1 = b.NewValue0(v.Pos, OpAdd64, typ.UInt64);
                                        v2 = b.NewValue0(v.Pos, OpMul64, typ.UInt64);
                                        v3 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                        v3.AuxInt = int64ToAuxInt(int64(sdivisible64(c).m));
                                        v2.AddArg2(v3, x);
                                        v4 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                        v4.AuxInt = int64ToAuxInt(int64(sdivisible64(c).a));
                                        v1.AddArg2(v2, v4);
                                        v5 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                        v5.AuxInt = int64ToAuxInt(64 - sdivisible64(c).k);
                                        v0.AddArg2(v1, v5);
                                        v6 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                                        v6.AuxInt = int64ToAuxInt(int64(sdivisible64(c).max));
                                        v.AddArg2(v0, v6);
                                        return true;

                                    }

                                }

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 n (Lsh64x64 (Rsh64x64 (Add64 <t> n (Rsh64Ux64 <t> (Rsh64x64 <t> n (Const64 <typ.UInt64> [63])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 63 && kbar == 64 - k
    // result: (Eq64 (And64 <t> n (Const64 <t> [1<<uint(k)-1])) (Const64 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh64x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh64x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd64) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh64Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh64x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 63) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 63 && kbar == 64 - k)) {
                            continue;
                        }

                        v.reset(OpEq64);
                        v0 = b.NewValue0(v.Pos, OpAnd64, t);
                        v1 = b.NewValue0(v.Pos, OpConst64, t);
                        v1.AuxInt = int64ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        v2 = b.NewValue0(v.Pos, OpConst64, t);
                        v2.AuxInt = int64ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 s:(Sub64 x y) (Const64 [0]))
    // cond: s.Uses == 1
    // result: (Eq64 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s = v_0;
                if (s.Op != OpSub64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpEq64);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq64 (And64 <t> x (Const64 <t> [y])) (Const64 <t> [y]))
    // cond: oneBit64(y)
    // result: (Neq64 (And64 <t> x (Const64 <t> [y])) (Const64 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst64 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt64(v_0_1.AuxInt);
                        if (v_1.Op != OpConst64 || v_1.Type != t || auxIntToInt64(v_1.AuxInt) != y || !(oneBit64(y))) {
                            continue;
                        }

                        v.reset(OpNeq64);
                        v0 = b.NewValue0(v.Pos, OpAnd64, t);
                        v1 = b.NewValue0(v.Pos, OpConst64, t);
                        v1.AuxInt = int64ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst64, t);
                        v2.AuxInt = int64ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Eq64F (Const64F [c]) (Const64F [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat64(v_0.AuxInt);
                if (v_1.Op != OpConst64F) {
                    continue;
                }

                var d = auxIntToFloat64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (Eq8 (Const8 <t> [c]) (Add8 (Const8 <t> [d]) x))
    // result: (Eq8 (Const8 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpAdd8) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpEq8);
                        var v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 (Mod8u x (Const8 [c])) (Const8 [0]))
    // cond: x.Op != OpConst8 && udivisibleOK8(c) && !hasSmallRotate(config)
    // result: (Eq32 (Mod32u <typ.UInt32> (ZeroExt8to32 <typ.UInt32> x) (Const32 <typ.UInt32> [int32(uint8(c))])) (Const32 <typ.UInt32> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMod8u) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                x = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_1.AuxInt);
                if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != 0 || !(x.Op != OpConst8 && udivisibleOK8(c) && !hasSmallRotate(config))) {
                    continue;
                }

                v.reset(OpEq32);
                v0 = b.NewValue0(v.Pos, OpMod32u, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v2.AuxInt = int32ToAuxInt(int32(uint8(c)));
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpConst32, typ.UInt32);
                v3.AuxInt = int32ToAuxInt(0);
                v.AddArg2(v0, v3);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 (Mod8 x (Const8 [c])) (Const8 [0]))
    // cond: x.Op != OpConst8 && sdivisibleOK8(c) && !hasSmallRotate(config)
    // result: (Eq32 (Mod32 <typ.Int32> (SignExt8to32 <typ.Int32> x) (Const32 <typ.Int32> [int32(c)])) (Const32 <typ.Int32> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMod8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_1.AuxInt);
                if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != 0 || !(x.Op != OpConst8 && sdivisibleOK8(c) && !hasSmallRotate(config))) {
                    continue;
                }

                v.reset(OpEq32);
                v0 = b.NewValue0(v.Pos, OpMod32, typ.Int32);
                v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                v2 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
                v2.AuxInt = int32ToAuxInt(int32(c));
                v0.AddArg2(v1, v2);
                v3 = b.NewValue0(v.Pos, OpConst32, typ.Int32);
                v3.AuxInt = int32ToAuxInt(0);
                v.AddArg2(v0, v3);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 x (Mul8 (Const8 [c]) (Trunc32to8 (Rsh32Ux64 mul:(Mul32 (Const32 [m]) (ZeroExt8to32 x)) (Const64 [s]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1<<8+umagic8(c).m) && s == 8+umagic8(c).s && x.Op != OpConst8 && udivisibleOK8(c)
    // result: (Leq8U (RotateLeft8 <typ.UInt8> (Mul8 <typ.UInt8> (Const8 <typ.UInt8> [int8(udivisible8(c).m)]) x) (Const8 <typ.UInt8> [int8(8-udivisible8(c).k)]) ) (Const8 <typ.UInt8> [int8(udivisible8(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt8(v_1_0.AuxInt);
                        if (v_1_1.Op != OpTrunc32to8) {
                            continue;
                        }

                        var v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32Ux64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        var mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        var mul_0 = mul.Args[0];
                        var mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                var m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpZeroExt8to32 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                var v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                var s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                if (!(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(1 << 8 + umagic8(c).m) && s == 8 + umagic8(c).s && x.Op != OpConst8 && udivisibleOK8(c))) {
                                    continue;
                                }

                                v.reset(OpLeq8U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft8, typ.UInt8);
                                v1 = b.NewValue0(v.Pos, OpMul8, typ.UInt8);
                                v2 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v2.AuxInt = int8ToAuxInt(int8(udivisible8(c).m));
                                v1.AddArg2(v2, x);
                                v3 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v3.AuxInt = int8ToAuxInt(int8(8 - udivisible8(c).k));
                                v0.AddArg2(v1, v3);
                                var v4 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v4.AuxInt = int8ToAuxInt(int8(udivisible8(c).max));
                                v.AddArg2(v0, v4);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 x (Mul8 (Const8 [c]) (Sub8 (Rsh32x64 mul:(Mul32 (Const32 [m]) (SignExt8to32 x)) (Const64 [s])) (Rsh32x64 (SignExt8to32 x) (Const64 [31]))) ) )
    // cond: v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic8(c).m) && s == 8+smagic8(c).s && x.Op != OpConst8 && sdivisibleOK8(c)
    // result: (Leq8U (RotateLeft8 <typ.UInt8> (Add8 <typ.UInt8> (Mul8 <typ.UInt8> (Const8 <typ.UInt8> [int8(sdivisible8(c).m)]) x) (Const8 <typ.UInt8> [int8(sdivisible8(c).a)]) ) (Const8 <typ.UInt8> [int8(8-sdivisible8(c).k)]) ) (Const8 <typ.UInt8> [int8(sdivisible8(c).max)]) )
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpMul8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        c = auxIntToInt8(v_1_0.AuxInt);
                        if (v_1_1.Op != OpSub8) {
                            continue;
                        }

                        _ = v_1_1.Args[1];
                        v_1_1_0 = v_1_1.Args[0];
                        if (v_1_1_0.Op != OpRsh32x64) {
                            continue;
                        }

                        _ = v_1_1_0.Args[1];
                        mul = v_1_1_0.Args[0];
                        if (mul.Op != OpMul32) {
                            continue;
                        }

                        _ = mul.Args[1];
                        mul_0 = mul.Args[0];
                        mul_1 = mul.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                if (mul_0.Op != OpConst32) {
                                    continue;
                                (_i2, mul_0, mul_1) = (_i2 + 1, mul_1, mul_0);
                                }

                                m = auxIntToInt32(mul_0.AuxInt);
                                if (mul_1.Op != OpSignExt8to32 || x != mul_1.Args[0]) {
                                    continue;
                                }

                                v_1_1_0_1 = v_1_1_0.Args[1];
                                if (v_1_1_0_1.Op != OpConst64) {
                                    continue;
                                }

                                s = auxIntToInt64(v_1_1_0_1.AuxInt);
                                var v_1_1_1 = v_1_1.Args[1];
                                if (v_1_1_1.Op != OpRsh32x64) {
                                    continue;
                                }

                                _ = v_1_1_1.Args[1];
                                var v_1_1_1_0 = v_1_1_1.Args[0];
                                if (v_1_1_1_0.Op != OpSignExt8to32 || x != v_1_1_1_0.Args[0]) {
                                    continue;
                                }

                                var v_1_1_1_1 = v_1_1_1.Args[1];
                                if (v_1_1_1_1.Op != OpConst64 || auxIntToInt64(v_1_1_1_1.AuxInt) != 31 || !(v.Block.Func.pass.name != "opt" && mul.Uses == 1 && m == int32(smagic8(c).m) && s == 8 + smagic8(c).s && x.Op != OpConst8 && sdivisibleOK8(c))) {
                                    continue;
                                }

                                v.reset(OpLeq8U);
                                v0 = b.NewValue0(v.Pos, OpRotateLeft8, typ.UInt8);
                                v1 = b.NewValue0(v.Pos, OpAdd8, typ.UInt8);
                                v2 = b.NewValue0(v.Pos, OpMul8, typ.UInt8);
                                v3 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v3.AuxInt = int8ToAuxInt(int8(sdivisible8(c).m));
                                v2.AddArg2(v3, x);
                                v4 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v4.AuxInt = int8ToAuxInt(int8(sdivisible8(c).a));
                                v1.AddArg2(v2, v4);
                                var v5 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v5.AuxInt = int8ToAuxInt(int8(8 - sdivisible8(c).k));
                                v0.AddArg2(v1, v5);
                                var v6 = b.NewValue0(v.Pos, OpConst8, typ.UInt8);
                                v6.AuxInt = int8ToAuxInt(int8(sdivisible8(c).max));
                                v.AddArg2(v0, v6);
                                return true;

                            }


                            _i2 = _i2__prev4;
                        }

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 n (Lsh8x64 (Rsh8x64 (Add8 <t> n (Rsh8Ux64 <t> (Rsh8x64 <t> n (Const64 <typ.UInt64> [ 7])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 7 && kbar == 8 - k
    // result: (Eq8 (And8 <t> n (Const8 <t> [1<<uint(k)-1])) (Const8 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh8x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh8x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd8) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh8Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh8x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 7) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 7 && kbar == 8 - k)) {
                            continue;
                        }

                        v.reset(OpEq8);
                        v0 = b.NewValue0(v.Pos, OpAnd8, t);
                        v1 = b.NewValue0(v.Pos, OpConst8, t);
                        v1.AuxInt = int8ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        v2 = b.NewValue0(v.Pos, OpConst8, t);
                        v2.AuxInt = int8ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 s:(Sub8 x y) (Const8 [0]))
    // cond: s.Uses == 1
    // result: (Eq8 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s = v_0;
                if (s.Op != OpSub8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpEq8);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Eq8 (And8 <t> x (Const8 <t> [y])) (Const8 <t> [y]))
    // cond: oneBit8(y)
    // result: (Neq8 (And8 <t> x (Const8 <t> [y])) (Const8 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst8 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt8(v_0_1.AuxInt);
                        if (v_1.Op != OpConst8 || v_1.Type != t || auxIntToInt8(v_1.AuxInt) != y || !(oneBit8(y))) {
                            continue;
                        }

                        v.reset(OpNeq8);
                        v0 = b.NewValue0(v.Pos, OpAnd8, t);
                        v1 = b.NewValue0(v.Pos, OpConst8, t);
                        v1.AuxInt = int8ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst8, t);
                        v2.AuxInt = int8ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (EqB (ConstBool [c]) (ConstBool [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToBool(v_0.AuxInt);
                if (v_1.Op != OpConstBool) {
                    continue;
                }

                var d = auxIntToBool(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqB (ConstBool [false]) x)
    // result: (Not x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != false) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.reset(OpNot);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqB (ConstBool [true]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != true) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEqInter(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqInter x y)
    // result: (EqPtr (ITab x) (ITab y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpEqPtr);
        var v0 = b.NewValue0(v.Pos, OpITab, typ.Uintptr);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpITab, typ.Uintptr);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValuegeneric_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqPtr x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (EqPtr (Addr {x} _) (Addr {y} _))
    // result: (ConstBool [x == y])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpAddr) {
                    continue;
                }

                var y = auxToSym(v_1.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (Addr {x} _) (OffPtr [o] (Addr {y} _)))
    // result: (ConstBool [x == y && o == 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                var o = auxIntToInt64(v_1.AuxInt);
                var v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y && o == 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr [o1] (Addr {x} _)) (OffPtr [o2] (Addr {y} _)))
    // result: (ConstBool [x == y && o1 == o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var o1 = auxIntToInt64(v_0.AuxInt);
                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpAddr) {
                    continue;
                }

                x = auxToSym(v_0_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                var o2 = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y && o1 == o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (LocalAddr {x} _ _) (LocalAddr {y} _ _))
    // result: (ConstBool [x == y])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (LocalAddr {x} _ _) (OffPtr [o] (LocalAddr {y} _ _)))
    // result: (ConstBool [x == y && o == 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y && o == 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr [o1] (LocalAddr {x} _ _)) (OffPtr [o2] (LocalAddr {y} _ _)))
    // result: (ConstBool [x == y && o1 == o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr) {
                    continue;
                }

                x = auxToSym(v_0_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o2 = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x == y && o1 == o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr [o1] p1) p2)
    // cond: isSamePtr(p1, p2)
    // result: (ConstBool [o1 == 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                var p1 = v_0.Args[0];
                var p2 = v_1;
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(o1 == 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr [o1] p1) (OffPtr [o2] p2))
    // cond: isSamePtr(p1, p2)
    // result: (ConstBool [o1 == o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                p1 = v_0.Args[0];
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o2 = auxIntToInt64(v_1.AuxInt);
                p2 = v_1.Args[0];
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(o1 == o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c == d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c == d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (LocalAddr _ _) (Addr _))
    // result: (ConstBool [false])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr || v_1.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(false);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr (LocalAddr _ _)) (Addr _))
    // result: (ConstBool [false])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr || v_1.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(false);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (LocalAddr _ _) (OffPtr (Addr _)))
    // result: (ConstBool [false])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr || v_1.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(false);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (OffPtr (LocalAddr _ _)) (OffPtr (Addr _)))
    // result: (ConstBool [false])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr || v_1.Op != OpOffPtr) {
                    continue;
                }

                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(false);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (AddPtr p1 o1) p2)
    // cond: isSamePtr(p1, p2)
    // result: (Not (IsNonNil o1))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = v_0.Args[1];
                p1 = v_0.Args[0];
                p2 = v_1;
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpIsNonNil, typ.Bool);
                v0.AddArg(o1);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (Const32 [0]) p)
    // result: (Not (IsNonNil p))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var p = v_1;
                v.reset(OpNot);
                v0 = b.NewValue0(v.Pos, OpIsNonNil, typ.Bool);
                v0.AddArg(p);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (Const64 [0]) p)
    // result: (Not (IsNonNil p))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                p = v_1;
                v.reset(OpNot);
                v0 = b.NewValue0(v.Pos, OpIsNonNil, typ.Bool);
                v0.AddArg(p);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (EqPtr (ConstNil) p)
    // result: (Not (IsNonNil p))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstNil) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                p = v_1;
                v.reset(OpNot);
                v0 = b.NewValue0(v.Pos, OpIsNonNil, typ.Bool);
                v0.AddArg(p);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpEqSlice(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqSlice x y)
    // result: (EqPtr (SlicePtr x) (SlicePtr y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpEqPtr);
        var v0 = b.NewValue0(v.Pos, OpSlicePtr, typ.BytePtr);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSlicePtr, typ.BytePtr);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValuegeneric_OpIMake(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (IMake _typ (StructMake1 val))
    // result: (IMake _typ val)
    while (true) {
        var _typ = v_0;
        if (v_1.Op != OpStructMake1) {
            break;
        }
        var val = v_1.Args[0];
        v.reset(OpIMake);
        v.AddArg2(_typ, val);
        return true;

    } 
    // match: (IMake _typ (ArrayMake1 val))
    // result: (IMake _typ val)
    while (true) {
        _typ = v_0;
        if (v_1.Op != OpArrayMake1) {
            break;
        }
        val = v_1.Args[0];
        v.reset(OpIMake);
        v.AddArg2(_typ, val);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpInterLECall(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (InterLECall [argsize] {auxCall} (Load (OffPtr [off] (ITab (IMake (Addr {itab} (SB)) _))) _) ___)
    // cond: devirtLESym(v, auxCall, itab, off) != nil
    // result: devirtLECall(v, devirtLESym(v, auxCall, itab, off))
    while (true) {
        if (len(v.Args) < 1) {
            break;
        }
        var auxCall = auxToCall(v.Aux);
        var v_0 = v.Args[0];
        if (v_0.Op != OpLoad) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpOffPtr) {
            break;
        }
        var off = auxIntToInt64(v_0_0.AuxInt);
        var v_0_0_0 = v_0_0.Args[0];
        if (v_0_0_0.Op != OpITab) {
            break;
        }
        var v_0_0_0_0 = v_0_0_0.Args[0];
        if (v_0_0_0_0.Op != OpIMake) {
            break;
        }
        var v_0_0_0_0_0 = v_0_0_0_0.Args[0];
        if (v_0_0_0_0_0.Op != OpAddr) {
            break;
        }
        var itab = auxToSym(v_0_0_0_0_0.Aux);
        var v_0_0_0_0_0_0 = v_0_0_0_0_0.Args[0];
        if (v_0_0_0_0_0_0.Op != OpSB || !(devirtLESym(v, auxCall, itab, off) != null)) {
            break;
        }
        v.copyOf(devirtLECall(v, devirtLESym(v, auxCall, itab, off)));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (IsInBounds (ZeroExt8to32 _) (Const32 [c]))
    // cond: (1 << 8) <= c
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to32 || v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!((1 << 8) <= c)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt8to64 _) (Const64 [c]))
    // cond: (1 << 8) <= c
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to64 || v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!((1 << 8) <= c)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt16to32 _) (Const32 [c]))
    // cond: (1 << 16) <= c
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to32 || v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!((1 << 16) <= c)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt16to64 _) (Const64 [c]))
    // cond: (1 << 16) <= c
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to64 || v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!((1 << 16) <= c)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (IsInBounds (And8 (Const8 [c]) _) (Const8 [d]))
    // cond: 0 <= c && c < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd8) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst8) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                if (!(0 <= c && c < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt8to16 (And8 (Const8 [c]) _)) (Const16 [d]))
    // cond: 0 <= c && int16(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to16) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd8) {
            break;
        }
        var v_0_0_0 = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst8) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt8(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1.AuxInt);
                if (!(0 <= c && int16(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt8to32 (And8 (Const8 [c]) _)) (Const32 [d]))
    // cond: 0 <= c && int32(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to32) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd8) {
            break;
        }
        v_0_0_0 = v_0_0.Args[0];
        v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst8) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt8(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1.AuxInt);
                if (!(0 <= c && int32(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt8to64 (And8 (Const8 [c]) _)) (Const64 [d]))
    // cond: 0 <= c && int64(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd8) {
            break;
        }
        v_0_0_0 = v_0_0.Args[0];
        v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst8) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt8(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                if (!(0 <= c && int64(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (And16 (Const16 [c]) _) (Const16 [d]))
    // cond: 0 <= c && c < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd16) {
            break;
        }
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst16) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1.AuxInt);
                if (!(0 <= c && c < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt16to32 (And16 (Const16 [c]) _)) (Const32 [d]))
    // cond: 0 <= c && int32(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to32) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd16) {
            break;
        }
        v_0_0_0 = v_0_0.Args[0];
        v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst16) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt16(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1.AuxInt);
                if (!(0 <= c && int32(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt16to64 (And16 (Const16 [c]) _)) (Const64 [d]))
    // cond: 0 <= c && int64(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd16) {
            break;
        }
        v_0_0_0 = v_0_0.Args[0];
        v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst16) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt16(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                if (!(0 <= c && int64(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (And32 (Const32 [c]) _) (Const32 [d]))
    // cond: 0 <= c && c < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd32) {
            break;
        }
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1.AuxInt);
                if (!(0 <= c && c < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (ZeroExt32to64 (And32 (Const32 [c]) _)) (Const64 [d]))
    // cond: 0 <= c && int64(c) < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt32to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAnd32) {
            break;
        }
        v_0_0_0 = v_0_0.Args[0];
        v_0_0_1 = v_0_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0_0.Op != OpConst32) {
                    continue;
                (_i0, v_0_0_0, v_0_0_1) = (_i0 + 1, v_0_0_1, v_0_0_0);
                }

                c = auxIntToInt32(v_0_0_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                if (!(0 <= c && int64(c) < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (And64 (Const64 [c]) _) (Const64 [d]))
    // cond: 0 <= c && c < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                if (!(0 <= c && c < d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsInBounds (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [0 <= c && c < d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(0 <= c && c < d);
        return true;

    } 
    // match: (IsInBounds (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [0 <= c && c < d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(0 <= c && c < d);
        return true;

    } 
    // match: (IsInBounds (Mod32u _ y) y)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpMod32u) {
            break;
        }
        var y = v_0.Args[1];
        if (y != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (Mod64u _ y) y)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpMod64u) {
            break;
        }
        y = v_0.Args[1];
        if (y != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt8to64 (Rsh8Ux64 _ (Const64 [c]))) (Const64 [d]))
    // cond: 0 < c && c < 8 && 1<<uint( 8-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 8 && 1 << (int)(uint(8 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt8to32 (Rsh8Ux64 _ (Const64 [c]))) (Const32 [d]))
    // cond: 0 < c && c < 8 && 1<<uint( 8-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to32) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        if (!(0 < c && c < 8 && 1 << (int)(uint(8 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt8to16 (Rsh8Ux64 _ (Const64 [c]))) (Const16 [d]))
    // cond: 0 < c && c < 8 && 1<<uint( 8-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt8to16) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        d = auxIntToInt16(v_1.AuxInt);
        if (!(0 < c && c < 8 && 1 << (int)(uint(8 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (Rsh8Ux64 _ (Const64 [c])) (Const64 [d]))
    // cond: 0 < c && c < 8 && 1<<uint( 8-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 8 && 1 << (int)(uint(8 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt16to64 (Rsh16Ux64 _ (Const64 [c]))) (Const64 [d]))
    // cond: 0 < c && c < 16 && 1<<uint(16-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 16 && 1 << (int)(uint(16 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt16to32 (Rsh16Ux64 _ (Const64 [c]))) (Const64 [d]))
    // cond: 0 < c && c < 16 && 1<<uint(16-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt16to32) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 16 && 1 << (int)(uint(16 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (Rsh16Ux64 _ (Const64 [c])) (Const64 [d]))
    // cond: 0 < c && c < 16 && 1<<uint(16-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 16 && 1 << (int)(uint(16 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (ZeroExt32to64 (Rsh32Ux64 _ (Const64 [c]))) (Const64 [d]))
    // cond: 0 < c && c < 32 && 1<<uint(32-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpZeroExt32to64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 32 && 1 << (int)(uint(32 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (Rsh32Ux64 _ (Const64 [c])) (Const64 [d]))
    // cond: 0 < c && c < 32 && 1<<uint(32-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 32 && 1 << (int)(uint(32 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsInBounds (Rsh64Ux64 _ (Const64 [c])) (Const64 [d]))
    // cond: 0 < c && c < 64 && 1<<uint(64-c)-1 < d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpRsh64Ux64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(0 < c && c < 64 && 1 << (int)(uint(64 - c)) - 1 < d)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (IsNonNil (ConstNil))
    // result: (ConstBool [false])
    while (true) {
        if (v_0.Op != OpConstNil) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (IsNonNil (Const32 [c]))
    // result: (ConstBool [c != 0])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c != 0);
        return true;

    } 
    // match: (IsNonNil (Const64 [c]))
    // result: (ConstBool [c != 0])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c != 0);
        return true;

    } 
    // match: (IsNonNil (Addr _))
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAddr) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsNonNil (LocalAddr _ _))
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpLocalAddr) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (IsSliceInBounds x x)
    // result: (ConstBool [true])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsSliceInBounds (And32 (Const32 [c]) _) (Const32 [d]))
    // cond: 0 <= c && c <= d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd32) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                if (!(0 <= c && c <= d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsSliceInBounds (And64 (Const64 [c]) _) (Const64 [d]))
    // cond: 0 <= c && c <= d
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpAnd64) {
            break;
        }
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                if (!(0 <= c && c <= d)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (IsSliceInBounds (Const32 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsSliceInBounds (Const64 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    } 
    // match: (IsSliceInBounds (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [0 <= c && c <= d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(0 <= c && c <= d);
        return true;

    } 
    // match: (IsSliceInBounds (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [0 <= c && c <= d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(0 <= c && c <= d);
        return true;

    } 
    // match: (IsSliceInBounds (SliceLen x) (SliceCap x))
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpSliceLen) {
            break;
        }
        x = v_0.Args[0];
        if (v_1.Op != OpSliceCap || x != v_1.Args[0]) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq16 (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    } 
    // match: (Leq16 (Const16 [0]) (And16 _ (Const16 [c])))
    // cond: c >= 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0 || v_1.Op != OpAnd16) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_1.Op != OpConst16) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                c = auxIntToInt16(v_1_1.AuxInt);
                if (!(c >= 0)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }

        }
        break;

    } 
    // match: (Leq16 (Const16 [0]) (Rsh16Ux64 _ (Const64 [c])))
    // cond: c > 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0 || v_1.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1_1.AuxInt);
        if (!(c > 0)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq16U (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [uint16(c) <= uint16(d)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint16(c) <= uint16(d));
        return true;

    } 
    // match: (Leq16U (Const16 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq32 (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    } 
    // match: (Leq32 (Const32 [0]) (And32 _ (Const32 [c])))
    // cond: c >= 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0 || v_1.Op != OpAnd32) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_1.Op != OpConst32) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                c = auxIntToInt32(v_1_1.AuxInt);
                if (!(c >= 0)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }

        }
        break;

    } 
    // match: (Leq32 (Const32 [0]) (Rsh32Ux64 _ (Const64 [c])))
    // cond: c > 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0 || v_1.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1_1.AuxInt);
        if (!(c > 0)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq32F (Const32F [c]) (Const32F [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        if (v_1.Op != OpConst32F) {
            break;
        }
        var d = auxIntToFloat32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq32U (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [uint32(c) <= uint32(d)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint32(c) <= uint32(d));
        return true;

    } 
    // match: (Leq32U (Const32 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq64 (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    } 
    // match: (Leq64 (Const64 [0]) (And64 _ (Const64 [c])))
    // cond: c >= 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0 || v_1.Op != OpAnd64) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_1.Op != OpConst64) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                c = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= 0)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }

        }
        break;

    } 
    // match: (Leq64 (Const64 [0]) (Rsh64Ux64 _ (Const64 [c])))
    // cond: c > 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0 || v_1.Op != OpRsh64Ux64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1_1.AuxInt);
        if (!(c > 0)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq64F (Const64F [c]) (Const64F [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpConst64F) {
            break;
        }
        var d = auxIntToFloat64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq64U (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [uint64(c) <= uint64(d)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint64(c) <= uint64(d));
        return true;

    } 
    // match: (Leq64U (Const64 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq8 (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [c <= d])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c <= d);
        return true;

    } 
    // match: (Leq8 (Const8 [0]) (And8 _ (Const8 [c])))
    // cond: c >= 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0 || v_1.Op != OpAnd8) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_1.Op != OpConst8) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                c = auxIntToInt8(v_1_1.AuxInt);
                if (!(c >= 0)) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }

        }
        break;

    } 
    // match: (Leq8 (Const8 [0]) (Rsh8Ux64 _ (Const64 [c])))
    // cond: c > 0
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0 || v_1.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1_1.AuxInt);
        if (!(c > 0)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Leq8U (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [ uint8(c) <= uint8(d)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint8(c) <= uint8(d));
        return true;

    } 
    // match: (Leq8U (Const8 [0]) _)
    // result: (ConstBool [true])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(true);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less16 (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less16U (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [uint16(c) < uint16(d)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint16(c) < uint16(d));
        return true;

    } 
    // match: (Less16U _ (Const16 [0]))
    // result: (ConstBool [false])
    while (true) {
        if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less32 (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less32F (Const32F [c]) (Const32F [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        if (v_1.Op != OpConst32F) {
            break;
        }
        var d = auxIntToFloat32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less32U (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [uint32(c) < uint32(d)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint32(c) < uint32(d));
        return true;

    } 
    // match: (Less32U _ (Const32 [0]))
    // result: (ConstBool [false])
    while (true) {
        if (v_1.Op != OpConst32 || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less64 (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less64F (Const64F [c]) (Const64F [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpConst64F) {
            break;
        }
        var d = auxIntToFloat64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less64U (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [uint64(c) < uint64(d)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint64(c) < uint64(d));
        return true;

    } 
    // match: (Less64U _ (Const64 [0]))
    // result: (ConstBool [false])
    while (true) {
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less8 (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [c < d])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(c < d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less8U (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [ uint8(c) < uint8(d)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(uint8(c) < uint8(d));
        return true;

    } 
    // match: (Less8U _ (Const8 [0]))
    // result: (ConstBool [false])
    while (true) {
        if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var fe = b.Func.fe; 
    // match: (Load <t1> p1 (Store {t2} p2 x _))
    // cond: isSamePtr(p1, p2) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size()
    // result: x
    while (true) {
        var t1 = v.Type;
        var p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        var t2 = auxToType(v_1.Aux);
        var x = v_1.Args[1];
        var p2 = v_1.Args[0];
        if (!(isSamePtr(p1, p2) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size())) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 _ (Store {t3} p3 x _)))
    // cond: isSamePtr(p1, p3) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p3, t3.Size(), p2, t2.Size())
    // result: x
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        var v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        var t3 = auxToType(v_1_2.Aux);
        x = v_1_2.Args[1];
        var p3 = v_1_2.Args[0];
        if (!(isSamePtr(p1, p3) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p3, t3.Size(), p2, t2.Size()))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 _ (Store {t3} p3 _ (Store {t4} p4 x _))))
    // cond: isSamePtr(p1, p4) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p4, t4.Size(), p2, t2.Size()) && disjoint(p4, t4.Size(), p3, t3.Size())
    // result: x
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(v_1_2.Aux);
        _ = v_1_2.Args[2];
        p3 = v_1_2.Args[0];
        var v_1_2_2 = v_1_2.Args[2];
        if (v_1_2_2.Op != OpStore) {
            break;
        }
        var t4 = auxToType(v_1_2_2.Aux);
        x = v_1_2_2.Args[1];
        var p4 = v_1_2_2.Args[0];
        if (!(isSamePtr(p1, p4) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p4, t4.Size(), p2, t2.Size()) && disjoint(p4, t4.Size(), p3, t3.Size()))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 _ (Store {t3} p3 _ (Store {t4} p4 _ (Store {t5} p5 x _)))))
    // cond: isSamePtr(p1, p5) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p5, t5.Size(), p2, t2.Size()) && disjoint(p5, t5.Size(), p3, t3.Size()) && disjoint(p5, t5.Size(), p4, t4.Size())
    // result: x
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(v_1_2.Aux);
        _ = v_1_2.Args[2];
        p3 = v_1_2.Args[0];
        v_1_2_2 = v_1_2.Args[2];
        if (v_1_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(v_1_2_2.Aux);
        _ = v_1_2_2.Args[2];
        p4 = v_1_2_2.Args[0];
        var v_1_2_2_2 = v_1_2_2.Args[2];
        if (v_1_2_2_2.Op != OpStore) {
            break;
        }
        var t5 = auxToType(v_1_2_2_2.Aux);
        x = v_1_2_2_2.Args[1];
        var p5 = v_1_2_2_2.Args[0];
        if (!(isSamePtr(p1, p5) && t1.Compare(x.Type) == types.CMPeq && t1.Size() == t2.Size() && disjoint(p5, t5.Size(), p2, t2.Size()) && disjoint(p5, t5.Size(), p3, t3.Size()) && disjoint(p5, t5.Size(), p4, t4.Size()))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 (Const64 [x]) _))
    // cond: isSamePtr(p1,p2) && sizeof(t2) == 8 && is64BitFloat(t1) && !math.IsNaN(math.Float64frombits(uint64(x)))
    // result: (Const64F [math.Float64frombits(uint64(x))])
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[1];
        p2 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64) {
            break;
        }
        x = auxIntToInt64(v_1_1.AuxInt);
        if (!(isSamePtr(p1, p2) && sizeof(t2) == 8 && is64BitFloat(t1) && !math.IsNaN(math.Float64frombits(uint64(x))))) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(math.Float64frombits(uint64(x)));
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 (Const32 [x]) _))
    // cond: isSamePtr(p1,p2) && sizeof(t2) == 4 && is32BitFloat(t1) && !math.IsNaN(float64(math.Float32frombits(uint32(x))))
    // result: (Const32F [math.Float32frombits(uint32(x))])
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[1];
        p2 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst32) {
            break;
        }
        x = auxIntToInt32(v_1_1.AuxInt);
        if (!(isSamePtr(p1, p2) && sizeof(t2) == 4 && is32BitFloat(t1) && !math.IsNaN(float64(math.Float32frombits(uint32(x)))))) {
            break;
        }
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(math.Float32frombits(uint32(x)));
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 (Const64F [x]) _))
    // cond: isSamePtr(p1,p2) && sizeof(t2) == 8 && is64BitInt(t1)
    // result: (Const64 [int64(math.Float64bits(x))])
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[1];
        p2 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst64F) {
            break;
        }
        x = auxIntToFloat64(v_1_1.AuxInt);
        if (!(isSamePtr(p1, p2) && sizeof(t2) == 8 && is64BitInt(t1))) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(math.Float64bits(x)));
        return true;

    } 
    // match: (Load <t1> p1 (Store {t2} p2 (Const32F [x]) _))
    // cond: isSamePtr(p1,p2) && sizeof(t2) == 4 && is32BitInt(t1)
    // result: (Const32 [int32(math.Float32bits(x))])
    while (true) {
        t1 = v.Type;
        p1 = v_0;
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[1];
        p2 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpConst32F) {
            break;
        }
        x = auxIntToFloat32(v_1_1.AuxInt);
        if (!(isSamePtr(p1, p2) && sizeof(t2) == 4 && is32BitInt(t1))) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(math.Float32bits(x)));
        return true;

    } 
    // match: (Load <t1> op:(OffPtr [o1] p1) (Store {t2} p2 _ mem:(Zero [n] p3 _)))
    // cond: o1 >= 0 && o1+t1.Size() <= n && isSamePtr(p1, p3) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size())
    // result: @mem.Block (Load <t1> (OffPtr <op.Type> [o1] p3) mem)
    while (true) {
        t1 = v.Type;
        var op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        var o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        var mem = v_1.Args[2];
        if (mem.Op != OpZero) {
            break;
        }
        var n = auxIntToInt64(mem.AuxInt);
        p3 = mem.Args[0];
        if (!(o1 >= 0 && o1 + t1.Size() <= n && isSamePtr(p1, p3) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()))) {
            break;
        }
        b = mem.Block;
        var v0 = b.NewValue0(v.Pos, OpLoad, t1);
        v.copyOf(v0);
        var v1 = b.NewValue0(v.Pos, OpOffPtr, op.Type);
        v1.AuxInt = int64ToAuxInt(o1);
        v1.AddArg(p3);
        v0.AddArg2(v1, mem);
        return true;

    } 
    // match: (Load <t1> op:(OffPtr [o1] p1) (Store {t2} p2 _ (Store {t3} p3 _ mem:(Zero [n] p4 _))))
    // cond: o1 >= 0 && o1+t1.Size() <= n && isSamePtr(p1, p4) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size())
    // result: @mem.Block (Load <t1> (OffPtr <op.Type> [o1] p4) mem)
    while (true) {
        t1 = v.Type;
        op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(v_1_2.Aux);
        _ = v_1_2.Args[2];
        p3 = v_1_2.Args[0];
        mem = v_1_2.Args[2];
        if (mem.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem.AuxInt);
        p4 = mem.Args[0];
        if (!(o1 >= 0 && o1 + t1.Size() <= n && isSamePtr(p1, p4) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()))) {
            break;
        }
        b = mem.Block;
        v0 = b.NewValue0(v.Pos, OpLoad, t1);
        v.copyOf(v0);
        v1 = b.NewValue0(v.Pos, OpOffPtr, op.Type);
        v1.AuxInt = int64ToAuxInt(o1);
        v1.AddArg(p4);
        v0.AddArg2(v1, mem);
        return true;

    } 
    // match: (Load <t1> op:(OffPtr [o1] p1) (Store {t2} p2 _ (Store {t3} p3 _ (Store {t4} p4 _ mem:(Zero [n] p5 _)))))
    // cond: o1 >= 0 && o1+t1.Size() <= n && isSamePtr(p1, p5) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size())
    // result: @mem.Block (Load <t1> (OffPtr <op.Type> [o1] p5) mem)
    while (true) {
        t1 = v.Type;
        op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(v_1_2.Aux);
        _ = v_1_2.Args[2];
        p3 = v_1_2.Args[0];
        v_1_2_2 = v_1_2.Args[2];
        if (v_1_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(v_1_2_2.Aux);
        _ = v_1_2_2.Args[2];
        p4 = v_1_2_2.Args[0];
        mem = v_1_2_2.Args[2];
        if (mem.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem.AuxInt);
        p5 = mem.Args[0];
        if (!(o1 >= 0 && o1 + t1.Size() <= n && isSamePtr(p1, p5) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size()))) {
            break;
        }
        b = mem.Block;
        v0 = b.NewValue0(v.Pos, OpLoad, t1);
        v.copyOf(v0);
        v1 = b.NewValue0(v.Pos, OpOffPtr, op.Type);
        v1.AuxInt = int64ToAuxInt(o1);
        v1.AddArg(p5);
        v0.AddArg2(v1, mem);
        return true;

    } 
    // match: (Load <t1> op:(OffPtr [o1] p1) (Store {t2} p2 _ (Store {t3} p3 _ (Store {t4} p4 _ (Store {t5} p5 _ mem:(Zero [n] p6 _))))))
    // cond: o1 >= 0 && o1+t1.Size() <= n && isSamePtr(p1, p6) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size()) && disjoint(op, t1.Size(), p5, t5.Size())
    // result: @mem.Block (Load <t1> (OffPtr <op.Type> [o1] p6) mem)
    while (true) {
        t1 = v.Type;
        op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        if (v_1.Op != OpStore) {
            break;
        }
        t2 = auxToType(v_1.Aux);
        _ = v_1.Args[2];
        p2 = v_1.Args[0];
        v_1_2 = v_1.Args[2];
        if (v_1_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(v_1_2.Aux);
        _ = v_1_2.Args[2];
        p3 = v_1_2.Args[0];
        v_1_2_2 = v_1_2.Args[2];
        if (v_1_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(v_1_2_2.Aux);
        _ = v_1_2_2.Args[2];
        p4 = v_1_2_2.Args[0];
        v_1_2_2_2 = v_1_2_2.Args[2];
        if (v_1_2_2_2.Op != OpStore) {
            break;
        }
        t5 = auxToType(v_1_2_2_2.Aux);
        _ = v_1_2_2_2.Args[2];
        p5 = v_1_2_2_2.Args[0];
        mem = v_1_2_2_2.Args[2];
        if (mem.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem.AuxInt);
        var p6 = mem.Args[0];
        if (!(o1 >= 0 && o1 + t1.Size() <= n && isSamePtr(p1, p6) && fe.CanSSA(t1) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size()) && disjoint(op, t1.Size(), p5, t5.Size()))) {
            break;
        }
        b = mem.Block;
        v0 = b.NewValue0(v.Pos, OpLoad, t1);
        v.copyOf(v0);
        v1 = b.NewValue0(v.Pos, OpOffPtr, op.Type);
        v1.AuxInt = int64ToAuxInt(o1);
        v1.AddArg(p6);
        v0.AddArg2(v1, mem);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: t1.IsBoolean() && isSamePtr(p1, p2) && n >= o + 1
    // result: (ConstBool [false])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        var o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(t1.IsBoolean() && isSamePtr(p1, p2) && n >= o + 1)) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is8BitInt(t1) && isSamePtr(p1, p2) && n >= o + 1
    // result: (Const8 [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is8BitInt(t1) && isSamePtr(p1, p2) && n >= o + 1)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is16BitInt(t1) && isSamePtr(p1, p2) && n >= o + 2
    // result: (Const16 [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is16BitInt(t1) && isSamePtr(p1, p2) && n >= o + 2)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is32BitInt(t1) && isSamePtr(p1, p2) && n >= o + 4
    // result: (Const32 [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is32BitInt(t1) && isSamePtr(p1, p2) && n >= o + 4)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is64BitInt(t1) && isSamePtr(p1, p2) && n >= o + 8
    // result: (Const64 [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is64BitInt(t1) && isSamePtr(p1, p2) && n >= o + 8)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is32BitFloat(t1) && isSamePtr(p1, p2) && n >= o + 4
    // result: (Const32F [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is32BitFloat(t1) && isSamePtr(p1, p2) && n >= o + 4)) {
            break;
        }
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(0);
        return true;

    } 
    // match: (Load <t1> (OffPtr [o] p1) (Zero [n] p2 _))
    // cond: is64BitFloat(t1) && isSamePtr(p1, p2) && n >= o + 8
    // result: (Const64F [0])
    while (true) {
        t1 = v.Type;
        if (v_0.Op != OpOffPtr) {
            break;
        }
        o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        if (v_1.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(v_1.AuxInt);
        p2 = v_1.Args[0];
        if (!(is64BitFloat(t1) && isSamePtr(p1, p2) && n >= o + 8)) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(0);
        return true;

    } 
    // match: (Load <t> _ _)
    // cond: t.IsStruct() && t.NumFields() == 0 && fe.CanSSA(t)
    // result: (StructMake0)
    while (true) {
        var t = v.Type;
        if (!(t.IsStruct() && t.NumFields() == 0 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpStructMake0);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsStruct() && t.NumFields() == 1 && fe.CanSSA(t)
    // result: (StructMake1 (Load <t.FieldType(0)> (OffPtr <t.FieldType(0).PtrTo()> [0] ptr) mem))
    while (true) {
        t = v.Type;
        var ptr = v_0;
        mem = v_1;
        if (!(t.IsStruct() && t.NumFields() == 1 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpStructMake1);
        v0 = b.NewValue0(v.Pos, OpLoad, t.FieldType(0));
        v1 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v1.AuxInt = int64ToAuxInt(0);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        v.AddArg(v0);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsStruct() && t.NumFields() == 2 && fe.CanSSA(t)
    // result: (StructMake2 (Load <t.FieldType(0)> (OffPtr <t.FieldType(0).PtrTo()> [0] ptr) mem) (Load <t.FieldType(1)> (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsStruct() && t.NumFields() == 2 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpStructMake2);
        v0 = b.NewValue0(v.Pos, OpLoad, t.FieldType(0));
        v1 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v1.AuxInt = int64ToAuxInt(0);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        var v2 = b.NewValue0(v.Pos, OpLoad, t.FieldType(1));
        var v3 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v3.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v3.AddArg(ptr);
        v2.AddArg2(v3, mem);
        v.AddArg2(v0, v2);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsStruct() && t.NumFields() == 3 && fe.CanSSA(t)
    // result: (StructMake3 (Load <t.FieldType(0)> (OffPtr <t.FieldType(0).PtrTo()> [0] ptr) mem) (Load <t.FieldType(1)> (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] ptr) mem) (Load <t.FieldType(2)> (OffPtr <t.FieldType(2).PtrTo()> [t.FieldOff(2)] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsStruct() && t.NumFields() == 3 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpStructMake3);
        v0 = b.NewValue0(v.Pos, OpLoad, t.FieldType(0));
        v1 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v1.AuxInt = int64ToAuxInt(0);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        v2 = b.NewValue0(v.Pos, OpLoad, t.FieldType(1));
        v3 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v3.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v3.AddArg(ptr);
        v2.AddArg2(v3, mem);
        var v4 = b.NewValue0(v.Pos, OpLoad, t.FieldType(2));
        var v5 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(2).PtrTo());
        v5.AuxInt = int64ToAuxInt(t.FieldOff(2));
        v5.AddArg(ptr);
        v4.AddArg2(v5, mem);
        v.AddArg3(v0, v2, v4);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsStruct() && t.NumFields() == 4 && fe.CanSSA(t)
    // result: (StructMake4 (Load <t.FieldType(0)> (OffPtr <t.FieldType(0).PtrTo()> [0] ptr) mem) (Load <t.FieldType(1)> (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] ptr) mem) (Load <t.FieldType(2)> (OffPtr <t.FieldType(2).PtrTo()> [t.FieldOff(2)] ptr) mem) (Load <t.FieldType(3)> (OffPtr <t.FieldType(3).PtrTo()> [t.FieldOff(3)] ptr) mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsStruct() && t.NumFields() == 4 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpStructMake4);
        v0 = b.NewValue0(v.Pos, OpLoad, t.FieldType(0));
        v1 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v1.AuxInt = int64ToAuxInt(0);
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        v2 = b.NewValue0(v.Pos, OpLoad, t.FieldType(1));
        v3 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v3.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v3.AddArg(ptr);
        v2.AddArg2(v3, mem);
        v4 = b.NewValue0(v.Pos, OpLoad, t.FieldType(2));
        v5 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(2).PtrTo());
        v5.AuxInt = int64ToAuxInt(t.FieldOff(2));
        v5.AddArg(ptr);
        v4.AddArg2(v5, mem);
        var v6 = b.NewValue0(v.Pos, OpLoad, t.FieldType(3));
        var v7 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(3).PtrTo());
        v7.AuxInt = int64ToAuxInt(t.FieldOff(3));
        v7.AddArg(ptr);
        v6.AddArg2(v7, mem);
        v.AddArg4(v0, v2, v4, v6);
        return true;

    } 
    // match: (Load <t> _ _)
    // cond: t.IsArray() && t.NumElem() == 0
    // result: (ArrayMake0)
    while (true) {
        t = v.Type;
        if (!(t.IsArray() && t.NumElem() == 0)) {
            break;
        }
        v.reset(OpArrayMake0);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsArray() && t.NumElem() == 1 && fe.CanSSA(t)
    // result: (ArrayMake1 (Load <t.Elem()> ptr mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsArray() && t.NumElem() == 1 && fe.CanSSA(t))) {
            break;
        }
        v.reset(OpArrayMake1);
        v0 = b.NewValue0(v.Pos, OpLoad, t.Elem());
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x16 <t> x (Const16 [c]))
    // result: (Lsh16x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpLsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh16x16 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x32 <t> x (Const32 [c]))
    // result: (Lsh16x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpLsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh16x32 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x64 (Const16 [c]) (Const64 [d]))
    // result: (Const16 [c << uint64(d)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c << (int)(uint64(d)));
        return true;

    } 
    // match: (Lsh16x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Lsh16x64 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Lsh16x64 _ (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (Const16 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Lsh16x64 <t> (Lsh16x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Lsh16x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpLsh16x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpLsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh16x64 (Rsh16Ux64 (Lsh16x64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Lsh16x64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpLsh16x64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpLsh16x64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x8 <t> x (Const8 [c]))
    // result: (Lsh16x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpLsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh16x8 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x16 <t> x (Const16 [c]))
    // result: (Lsh32x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpLsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh32x16 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x32 <t> x (Const32 [c]))
    // result: (Lsh32x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpLsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh32x32 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x64 (Const32 [c]) (Const64 [d]))
    // result: (Const32 [c << uint64(d)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        return true;

    } 
    // match: (Lsh32x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Lsh32x64 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Lsh32x64 _ (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Lsh32x64 <t> (Lsh32x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Lsh32x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpLsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh32x64 (Rsh32Ux64 (Lsh32x64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Lsh32x64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpLsh32x64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x8 <t> x (Const8 [c]))
    // result: (Lsh32x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpLsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh32x8 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh64x16 <t> x (Const16 [c]))
    // result: (Lsh64x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpLsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh64x16 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh64x32 <t> x (Const32 [c]))
    // result: (Lsh64x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpLsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh64x32 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c << uint64(d)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c << (int)(uint64(d)));
        return true;

    } 
    // match: (Lsh64x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Lsh64x64 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Lsh64x64 _ (Const64 [c]))
    // cond: uint64(c) >= 64
    // result: (Const64 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Lsh64x64 <t> (Lsh64x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Lsh64x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpLsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh64x64 (Rsh64Ux64 (Lsh64x64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Lsh64x64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpRsh64Ux64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpLsh64x64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh64x8 <t> x (Const8 [c]))
    // result: (Lsh64x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpLsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh64x8 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x16 <t> x (Const16 [c]))
    // result: (Lsh8x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpLsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh8x16 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x32 <t> x (Const32 [c]))
    // result: (Lsh8x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpLsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh8x32 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x64 (Const8 [c]) (Const64 [d]))
    // result: (Const8 [c << uint64(d)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c << (int)(uint64(d)));
        return true;

    } 
    // match: (Lsh8x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Lsh8x64 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Lsh8x64 _ (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (Const8 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Lsh8x64 <t> (Lsh8x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Lsh8x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpLsh8x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpLsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh8x64 (Rsh8Ux64 (Lsh8x64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Lsh8x64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpLsh8x64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpLsh8x64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x8 <t> x (Const8 [c]))
    // result: (Lsh8x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpLsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Lsh8x8 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod16 (Const16 [c]) (Const16 [d]))
    // cond: d != 0
    // result: (Const16 [c % d])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c % d);
        return true;

    } 
    // match: (Mod16 <t> n (Const16 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo16(c)
    // result: (And16 n (Const16 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo16(c))) {
            break;
        }
        v.reset(OpAnd16);
        var v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod16 <t> n (Const16 [c]))
    // cond: c < 0 && c != -1<<15
    // result: (Mod16 <t> n (Const16 <t> [-c]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 15)) {
            break;
        }
        v.reset(OpMod16);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(-c);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod16 <t> x (Const16 [c]))
    // cond: x.Op != OpConst16 && (c > 0 || c == -1<<15)
    // result: (Sub16 x (Mul16 <t> (Div16 <t> x (Const16 <t> [c])) (Const16 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(x.Op != OpConst16 && (c > 0 || c == -1 << 15))) {
            break;
        }
        v.reset(OpSub16);
        v0 = b.NewValue0(v.Pos, OpMul16, t);
        var v1 = b.NewValue0(v.Pos, OpDiv16, t);
        var v2 = b.NewValue0(v.Pos, OpConst16, t);
        v2.AuxInt = int16ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod16u (Const16 [c]) (Const16 [d]))
    // cond: d != 0
    // result: (Const16 [int16(uint16(c) % uint16(d))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(uint16(c) % uint16(d)));
        return true;

    } 
    // match: (Mod16u <t> n (Const16 [c]))
    // cond: isPowerOfTwo16(c)
    // result: (And16 n (Const16 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(isPowerOfTwo16(c))) {
            break;
        }
        v.reset(OpAnd16);
        var v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod16u <t> x (Const16 [c]))
    // cond: x.Op != OpConst16 && c > 0 && umagicOK16(c)
    // result: (Sub16 x (Mul16 <t> (Div16u <t> x (Const16 <t> [c])) (Const16 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_1.AuxInt);
        if (!(x.Op != OpConst16 && c > 0 && umagicOK16(c))) {
            break;
        }
        v.reset(OpSub16);
        v0 = b.NewValue0(v.Pos, OpMul16, t);
        var v1 = b.NewValue0(v.Pos, OpDiv16u, t);
        var v2 = b.NewValue0(v.Pos, OpConst16, t);
        v2.AuxInt = int16ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod32 (Const32 [c]) (Const32 [d]))
    // cond: d != 0
    // result: (Const32 [c % d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c % d);
        return true;

    } 
    // match: (Mod32 <t> n (Const32 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo32(c)
    // result: (And32 n (Const32 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpAnd32);
        var v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod32 <t> n (Const32 [c]))
    // cond: c < 0 && c != -1<<31
    // result: (Mod32 <t> n (Const32 <t> [-c]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 31)) {
            break;
        }
        v.reset(OpMod32);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(-c);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod32 <t> x (Const32 [c]))
    // cond: x.Op != OpConst32 && (c > 0 || c == -1<<31)
    // result: (Sub32 x (Mul32 <t> (Div32 <t> x (Const32 <t> [c])) (Const32 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(x.Op != OpConst32 && (c > 0 || c == -1 << 31))) {
            break;
        }
        v.reset(OpSub32);
        v0 = b.NewValue0(v.Pos, OpMul32, t);
        var v1 = b.NewValue0(v.Pos, OpDiv32, t);
        var v2 = b.NewValue0(v.Pos, OpConst32, t);
        v2.AuxInt = int32ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod32u (Const32 [c]) (Const32 [d]))
    // cond: d != 0
    // result: (Const32 [int32(uint32(c) % uint32(d))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) % uint32(d)));
        return true;

    } 
    // match: (Mod32u <t> n (Const32 [c]))
    // cond: isPowerOfTwo32(c)
    // result: (And32 n (Const32 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpAnd32);
        var v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod32u <t> x (Const32 [c]))
    // cond: x.Op != OpConst32 && c > 0 && umagicOK32(c)
    // result: (Sub32 x (Mul32 <t> (Div32u <t> x (Const32 <t> [c])) (Const32 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(x.Op != OpConst32 && c > 0 && umagicOK32(c))) {
            break;
        }
        v.reset(OpSub32);
        v0 = b.NewValue0(v.Pos, OpMul32, t);
        var v1 = b.NewValue0(v.Pos, OpDiv32u, t);
        var v2 = b.NewValue0(v.Pos, OpConst32, t);
        v2.AuxInt = int32ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod64 (Const64 [c]) (Const64 [d]))
    // cond: d != 0
    // result: (Const64 [c % d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c % d);
        return true;

    } 
    // match: (Mod64 <t> n (Const64 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo64(c)
    // result: (And64 n (Const64 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo64(c))) {
            break;
        }
        v.reset(OpAnd64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod64 n (Const64 [-1<<63]))
    // cond: isNonNegative(n)
    // result: n
    while (true) {
        n = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != -1 << 63 || !(isNonNegative(n))) {
            break;
        }
        v.copyOf(n);
        return true;

    } 
    // match: (Mod64 <t> n (Const64 [c]))
    // cond: c < 0 && c != -1<<63
    // result: (Mod64 <t> n (Const64 <t> [-c]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 63)) {
            break;
        }
        v.reset(OpMod64);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(-c);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod64 <t> x (Const64 [c]))
    // cond: x.Op != OpConst64 && (c > 0 || c == -1<<63)
    // result: (Sub64 x (Mul64 <t> (Div64 <t> x (Const64 <t> [c])) (Const64 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(x.Op != OpConst64 && (c > 0 || c == -1 << 63))) {
            break;
        }
        v.reset(OpSub64);
        v0 = b.NewValue0(v.Pos, OpMul64, t);
        var v1 = b.NewValue0(v.Pos, OpDiv64, t);
        var v2 = b.NewValue0(v.Pos, OpConst64, t);
        v2.AuxInt = int64ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod64u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod64u (Const64 [c]) (Const64 [d]))
    // cond: d != 0
    // result: (Const64 [int64(uint64(c) % uint64(d))])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint64(c) % uint64(d)));
        return true;

    } 
    // match: (Mod64u <t> n (Const64 [c]))
    // cond: isPowerOfTwo64(c)
    // result: (And64 n (Const64 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(isPowerOfTwo64(c))) {
            break;
        }
        v.reset(OpAnd64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod64u <t> n (Const64 [-1<<63]))
    // result: (And64 n (Const64 <t> [1<<63-1]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != -1 << 63) {
            break;
        }
        v.reset(OpAnd64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(1 << 63 - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod64u <t> x (Const64 [c]))
    // cond: x.Op != OpConst64 && c > 0 && umagicOK64(c)
    // result: (Sub64 x (Mul64 <t> (Div64u <t> x (Const64 <t> [c])) (Const64 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(x.Op != OpConst64 && c > 0 && umagicOK64(c))) {
            break;
        }
        v.reset(OpSub64);
        v0 = b.NewValue0(v.Pos, OpMul64, t);
        var v1 = b.NewValue0(v.Pos, OpDiv64u, t);
        var v2 = b.NewValue0(v.Pos, OpConst64, t);
        v2.AuxInt = int64ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod8 (Const8 [c]) (Const8 [d]))
    // cond: d != 0
    // result: (Const8 [c % d])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c % d);
        return true;

    } 
    // match: (Mod8 <t> n (Const8 [c]))
    // cond: isNonNegative(n) && isPowerOfTwo8(c)
    // result: (And8 n (Const8 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(isNonNegative(n) && isPowerOfTwo8(c))) {
            break;
        }
        v.reset(OpAnd8);
        var v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod8 <t> n (Const8 [c]))
    // cond: c < 0 && c != -1<<7
    // result: (Mod8 <t> n (Const8 <t> [-c]))
    while (true) {
        t = v.Type;
        n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(c < 0 && c != -1 << 7)) {
            break;
        }
        v.reset(OpMod8);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(-c);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod8 <t> x (Const8 [c]))
    // cond: x.Op != OpConst8 && (c > 0 || c == -1<<7)
    // result: (Sub8 x (Mul8 <t> (Div8 <t> x (Const8 <t> [c])) (Const8 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(x.Op != OpConst8 && (c > 0 || c == -1 << 7))) {
            break;
        }
        v.reset(OpSub8);
        v0 = b.NewValue0(v.Pos, OpMul8, t);
        var v1 = b.NewValue0(v.Pos, OpDiv8, t);
        var v2 = b.NewValue0(v.Pos, OpConst8, t);
        v2.AuxInt = int8ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Mod8u (Const8 [c]) (Const8 [d]))
    // cond: d != 0
    // result: (Const8 [int8(uint8(c) % uint8(d))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(uint8(c) % uint8(d)));
        return true;

    } 
    // match: (Mod8u <t> n (Const8 [c]))
    // cond: isPowerOfTwo8(c)
    // result: (And8 n (Const8 <t> [c-1]))
    while (true) {
        var t = v.Type;
        var n = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(isPowerOfTwo8(c))) {
            break;
        }
        v.reset(OpAnd8);
        var v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(c - 1);
        v.AddArg2(n, v0);
        return true;

    } 
    // match: (Mod8u <t> x (Const8 [c]))
    // cond: x.Op != OpConst8 && c > 0 && umagicOK8( c)
    // result: (Sub8 x (Mul8 <t> (Div8u <t> x (Const8 <t> [c])) (Const8 <t> [c])))
    while (true) {
        t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        c = auxIntToInt8(v_1.AuxInt);
        if (!(x.Op != OpConst8 && c > 0 && umagicOK8(c))) {
            break;
        }
        v.reset(OpSub8);
        v0 = b.NewValue0(v.Pos, OpMul8, t);
        var v1 = b.NewValue0(v.Pos, OpDiv8u, t);
        var v2 = b.NewValue0(v.Pos, OpConst8, t);
        v2.AuxInt = int8ToAuxInt(c);
        v1.AddArg2(x, v2);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMove(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (Move {t} [n] dst1 src mem:(Zero {t} [n] dst2 _))
    // cond: isSamePtr(src, dst2)
    // result: (Zero {t} [n] dst1 mem)
    while (true) {
        var n = auxIntToInt64(v.AuxInt);
        var t = auxToType(v.Aux);
        var dst1 = v_0;
        var src = v_1;
        var mem = v_2;
        if (mem.Op != OpZero || auxIntToInt64(mem.AuxInt) != n || auxToType(mem.Aux) != t) {
            break;
        }
        var dst2 = mem.Args[0];
        if (!(isSamePtr(src, dst2))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg2(dst1, mem);
        return true;

    } 
    // match: (Move {t} [n] dst1 src mem:(VarDef (Zero {t} [n] dst0 _)))
    // cond: isSamePtr(src, dst0)
    // result: (Zero {t} [n] dst1 mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        src = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        var mem_0 = mem.Args[0];
        if (mem_0.Op != OpZero || auxIntToInt64(mem_0.AuxInt) != n || auxToType(mem_0.Aux) != t) {
            break;
        }
        var dst0 = mem_0.Args[0];
        if (!(isSamePtr(src, dst0))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg2(dst1, mem);
        return true;

    } 
    // match: (Move {t} [n] dst (Addr {sym} (SB)) mem)
    // cond: symIsROZero(sym)
    // result: (Zero {t} [n] dst mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        var dst = v_0;
        if (v_1.Op != OpAddr) {
            break;
        }
        var sym = auxToSym(v_1.Aux);
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpSB) {
            break;
        }
        mem = v_2;
        if (!(symIsROZero(sym))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg2(dst, mem);
        return true;

    } 
    // match: (Move {t1} [n] dst1 src1 store:(Store {t2} op:(OffPtr [o2] dst2) _ mem))
    // cond: isSamePtr(dst1, dst2) && store.Uses == 1 && n >= o2 + t2.Size() && disjoint(src1, n, op, t2.Size()) && clobber(store)
    // result: (Move {t1} [n] dst1 src1 mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        var t1 = auxToType(v.Aux);
        dst1 = v_0;
        var src1 = v_1;
        var store = v_2;
        if (store.Op != OpStore) {
            break;
        }
        var t2 = auxToType(store.Aux);
        mem = store.Args[2];
        var op = store.Args[0];
        if (op.Op != OpOffPtr) {
            break;
        }
        var o2 = auxIntToInt64(op.AuxInt);
        dst2 = op.Args[0];
        if (!(isSamePtr(dst1, dst2) && store.Uses == 1 && n >= o2 + t2.Size() && disjoint(src1, n, op, t2.Size()) && clobber(store))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t1);
        v.AddArg3(dst1, src1, mem);
        return true;

    } 
    // match: (Move {t} [n] dst1 src1 move:(Move {t} [n] dst2 _ mem))
    // cond: move.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(move)
    // result: (Move {t} [n] dst1 src1 mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        src1 = v_1;
        var move = v_2;
        if (move.Op != OpMove || auxIntToInt64(move.AuxInt) != n || auxToType(move.Aux) != t) {
            break;
        }
        mem = move.Args[2];
        dst2 = move.Args[0];
        if (!(move.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(move))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg3(dst1, src1, mem);
        return true;

    } 
    // match: (Move {t} [n] dst1 src1 vardef:(VarDef {x} move:(Move {t} [n] dst2 _ mem)))
    // cond: move.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(move, vardef)
    // result: (Move {t} [n] dst1 src1 (VarDef {x} mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        src1 = v_1;
        var vardef = v_2;
        if (vardef.Op != OpVarDef) {
            break;
        }
        var x = auxToSym(vardef.Aux);
        move = vardef.Args[0];
        if (move.Op != OpMove || auxIntToInt64(move.AuxInt) != n || auxToType(move.Aux) != t) {
            break;
        }
        mem = move.Args[2];
        dst2 = move.Args[0];
        if (!(move.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(move, vardef))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        var v0 = b.NewValue0(v.Pos, OpVarDef, types.TypeMem);
        v0.Aux = symToAux(x);
        v0.AddArg(mem);
        v.AddArg3(dst1, src1, v0);
        return true;

    } 
    // match: (Move {t} [n] dst1 src1 zero:(Zero {t} [n] dst2 mem))
    // cond: zero.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(zero)
    // result: (Move {t} [n] dst1 src1 mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        src1 = v_1;
        var zero = v_2;
        if (zero.Op != OpZero || auxIntToInt64(zero.AuxInt) != n || auxToType(zero.Aux) != t) {
            break;
        }
        mem = zero.Args[1];
        dst2 = zero.Args[0];
        if (!(zero.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(zero))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg3(dst1, src1, mem);
        return true;

    } 
    // match: (Move {t} [n] dst1 src1 vardef:(VarDef {x} zero:(Zero {t} [n] dst2 mem)))
    // cond: zero.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(zero, vardef)
    // result: (Move {t} [n] dst1 src1 (VarDef {x} mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        src1 = v_1;
        vardef = v_2;
        if (vardef.Op != OpVarDef) {
            break;
        }
        x = auxToSym(vardef.Aux);
        zero = vardef.Args[0];
        if (zero.Op != OpZero || auxIntToInt64(zero.AuxInt) != n || auxToType(zero.Aux) != t) {
            break;
        }
        mem = zero.Args[1];
        dst2 = zero.Args[0];
        if (!(zero.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && disjoint(src1, n, dst2, n) && clobber(zero, vardef))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v0 = b.NewValue0(v.Pos, OpVarDef, types.TypeMem);
        v0.Aux = symToAux(x);
        v0.AddArg(mem);
        v.AddArg3(dst1, src1, v0);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [0] p3) d2 _)))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && o2 == t3.Size() && n == t2.Size() + t3.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [0] dst) d2 mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        var p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        var op2 = mem.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        var tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        var p2 = op2.Args[0];
        var d1 = mem.Args[1];
        var mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        var t3 = auxToType(mem_2.Aux);
        var d2 = mem_2.Args[1];
        var op3 = mem_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        var tt3 = op3.Type;
        if (auxIntToInt64(op3.AuxInt) != 0) {
            break;
        }
        var p3 = op3.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && o2 == t3.Size() && n == t2.Size() + t3.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        var v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(0);
        v2.AddArg(dst);
        v1.AddArg3(v2, d2, mem);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [o3] p3) d2 (Store {t4} op4:(OffPtr <tt4> [0] p4) d3 _))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && o3 == t4.Size() && o2-o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [0] dst) d3 mem)))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        op2 = mem.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        op3 = mem_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        tt3 = op3.Type;
        var o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d2 = mem_2.Args[1];
        var mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        var t4 = auxToType(mem_2_2.Aux);
        var d3 = mem_2_2.Args[1];
        var op4 = mem_2_2.Args[0];
        if (op4.Op != OpOffPtr) {
            break;
        }
        var tt4 = op4.Type;
        if (auxIntToInt64(op4.AuxInt) != 0) {
            break;
        }
        var p4 = op4.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && o3 == t4.Size() && o2 - o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        var v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        var v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(0);
        v4.AddArg(dst);
        v3.AddArg3(v4, d3, mem);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [o3] p3) d2 (Store {t4} op4:(OffPtr <tt4> [o4] p4) d3 (Store {t5} op5:(OffPtr <tt5> [0] p5) d4 _)))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && o4 == t5.Size() && o3-o4 == t4.Size() && o2-o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size() + t5.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Store {t5} (OffPtr <tt5> [0] dst) d4 mem))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        op2 = mem.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        op3 = mem_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        tt3 = op3.Type;
        o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d2 = mem_2.Args[1];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_2_2.Aux);
        _ = mem_2_2.Args[2];
        op4 = mem_2_2.Args[0];
        if (op4.Op != OpOffPtr) {
            break;
        }
        tt4 = op4.Type;
        var o4 = auxIntToInt64(op4.AuxInt);
        p4 = op4.Args[0];
        d3 = mem_2_2.Args[1];
        var mem_2_2_2 = mem_2_2.Args[2];
        if (mem_2_2_2.Op != OpStore) {
            break;
        }
        var t5 = auxToType(mem_2_2_2.Aux);
        var d4 = mem_2_2_2.Args[1];
        var op5 = mem_2_2_2.Args[0];
        if (op5.Op != OpOffPtr) {
            break;
        }
        var tt5 = op5.Type;
        if (auxIntToInt64(op5.AuxInt) != 0) {
            break;
        }
        var p5 = op5.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && o4 == t5.Size() && o3 - o4 == t4.Size() && o2 - o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size() + t5.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        var v5 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v5.Aux = typeToAux(t5);
        var v6 = b.NewValue0(v.Pos, OpOffPtr, tt5);
        v6.AuxInt = int64ToAuxInt(0);
        v6.AddArg(dst);
        v5.AddArg3(v6, d4, mem);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [0] p3) d2 _))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && o2 == t3.Size() && n == t2.Size() + t3.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [0] dst) d2 mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        op2 = mem_0.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem_0.Args[1];
        var mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        d2 = mem_0_2.Args[1];
        op3 = mem_0_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        tt3 = op3.Type;
        if (auxIntToInt64(op3.AuxInt) != 0) {
            break;
        }
        p3 = op3.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && o2 == t3.Size() && n == t2.Size() + t3.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(0);
        v2.AddArg(dst);
        v1.AddArg3(v2, d2, mem);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [o3] p3) d2 (Store {t4} op4:(OffPtr <tt4> [0] p4) d3 _)))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && o3 == t4.Size() && o2-o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [0] dst) d3 mem)))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        op2 = mem_0.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        _ = mem_0_2.Args[2];
        op3 = mem_0_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        tt3 = op3.Type;
        o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d2 = mem_0_2.Args[1];
        var mem_0_2_2 = mem_0_2.Args[2];
        if (mem_0_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_0_2_2.Aux);
        d3 = mem_0_2_2.Args[1];
        op4 = mem_0_2_2.Args[0];
        if (op4.Op != OpOffPtr) {
            break;
        }
        tt4 = op4.Type;
        if (auxIntToInt64(op4.AuxInt) != 0) {
            break;
        }
        p4 = op4.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && o3 == t4.Size() && o2 - o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(0);
        v4.AddArg(dst);
        v3.AddArg3(v4, d3, mem);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Store {t3} op3:(OffPtr <tt3> [o3] p3) d2 (Store {t4} op4:(OffPtr <tt4> [o4] p4) d3 (Store {t5} op5:(OffPtr <tt5> [0] p5) d4 _))))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && o4 == t5.Size() && o3-o4 == t4.Size() && o2-o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size() + t5.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Store {t5} (OffPtr <tt5> [0] dst) d4 mem))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        op2 = mem_0.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        _ = mem_0_2.Args[2];
        op3 = mem_0_2.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        tt3 = op3.Type;
        o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d2 = mem_0_2.Args[1];
        mem_0_2_2 = mem_0_2.Args[2];
        if (mem_0_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_0_2_2.Aux);
        _ = mem_0_2_2.Args[2];
        op4 = mem_0_2_2.Args[0];
        if (op4.Op != OpOffPtr) {
            break;
        }
        tt4 = op4.Type;
        o4 = auxIntToInt64(op4.AuxInt);
        p4 = op4.Args[0];
        d3 = mem_0_2_2.Args[1];
        var mem_0_2_2_2 = mem_0_2_2.Args[2];
        if (mem_0_2_2_2.Op != OpStore) {
            break;
        }
        t5 = auxToType(mem_0_2_2_2.Aux);
        d4 = mem_0_2_2_2.Args[1];
        op5 = mem_0_2_2_2.Args[0];
        if (op5.Op != OpOffPtr) {
            break;
        }
        tt5 = op5.Type;
        if (auxIntToInt64(op5.AuxInt) != 0) {
            break;
        }
        p5 = op5.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && o4 == t5.Size() && o3 - o4 == t4.Size() && o2 - o3 == t3.Size() && n == t2.Size() + t3.Size() + t4.Size() + t5.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        v5 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v5.Aux = typeToAux(t5);
        v6 = b.NewValue0(v.Pos, OpOffPtr, tt5);
        v6.AuxInt = int64ToAuxInt(0);
        v6.AddArg(dst);
        v5.AddArg3(v6, d4, mem);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Zero {t3} [n] p3 _)))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && n >= o2 + t2.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Zero {t1} [n] dst mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        op2 = mem.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpZero || auxIntToInt64(mem_2.AuxInt) != n) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        p3 = mem_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && n >= o2 + t2.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v1.AuxInt = int64ToAuxInt(n);
        v1.Aux = typeToAux(t1);
        v1.AddArg2(dst, mem);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Zero {t4} [n] p4 _))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && n >= o2 + t2.Size() && n >= o3 + t3.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Zero {t1} [n] dst mem)))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0.Type;
        o2 = auxIntToInt64(mem_0.AuxInt);
        p2 = mem_0.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        var mem_2_0 = mem_2.Args[0];
        if (mem_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_2_0.Type;
        o3 = auxIntToInt64(mem_2_0.AuxInt);
        p3 = mem_2_0.Args[0];
        d2 = mem_2.Args[1];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpZero || auxIntToInt64(mem_2_2.AuxInt) != n) {
            break;
        }
        t4 = auxToType(mem_2_2.Aux);
        p4 = mem_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && n >= o2 + t2.Size() && n >= o3 + t3.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v3.AuxInt = int64ToAuxInt(n);
        v3.Aux = typeToAux(t1);
        v3.AddArg2(dst, mem);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Store {t4} (OffPtr <tt4> [o4] p4) d3 (Zero {t5} [n] p5 _)))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Zero {t1} [n] dst mem))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0.Type;
        o2 = auxIntToInt64(mem_0.AuxInt);
        p2 = mem_0.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        mem_2_0 = mem_2.Args[0];
        if (mem_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_2_0.Type;
        o3 = auxIntToInt64(mem_2_0.AuxInt);
        p3 = mem_2_0.Args[0];
        d2 = mem_2.Args[1];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_2_2.Aux);
        _ = mem_2_2.Args[2];
        var mem_2_2_0 = mem_2_2.Args[0];
        if (mem_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt4 = mem_2_2_0.Type;
        o4 = auxIntToInt64(mem_2_2_0.AuxInt);
        p4 = mem_2_2_0.Args[0];
        d3 = mem_2_2.Args[1];
        mem_2_2_2 = mem_2_2.Args[2];
        if (mem_2_2_2.Op != OpZero || auxIntToInt64(mem_2_2_2.AuxInt) != n) {
            break;
        }
        t5 = auxToType(mem_2_2_2.Aux);
        p5 = mem_2_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        v5 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v5.AuxInt = int64ToAuxInt(n);
        v5.Aux = typeToAux(t1);
        v5.AddArg2(dst, mem);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Store {t4} (OffPtr <tt4> [o4] p4) d3 (Store {t5} (OffPtr <tt5> [o5] p5) d4 (Zero {t6} [n] p6 _))))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && isSamePtr(p5, p6) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && t6.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size() && n >= o5 + t5.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Store {t5} (OffPtr <tt5> [o5] dst) d4 (Zero {t1} [n] dst mem)))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0.Type;
        o2 = auxIntToInt64(mem_0.AuxInt);
        p2 = mem_0.Args[0];
        d1 = mem.Args[1];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        mem_2_0 = mem_2.Args[0];
        if (mem_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_2_0.Type;
        o3 = auxIntToInt64(mem_2_0.AuxInt);
        p3 = mem_2_0.Args[0];
        d2 = mem_2.Args[1];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_2_2.Aux);
        _ = mem_2_2.Args[2];
        mem_2_2_0 = mem_2_2.Args[0];
        if (mem_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt4 = mem_2_2_0.Type;
        o4 = auxIntToInt64(mem_2_2_0.AuxInt);
        p4 = mem_2_2_0.Args[0];
        d3 = mem_2_2.Args[1];
        mem_2_2_2 = mem_2_2.Args[2];
        if (mem_2_2_2.Op != OpStore) {
            break;
        }
        t5 = auxToType(mem_2_2_2.Aux);
        _ = mem_2_2_2.Args[2];
        var mem_2_2_2_0 = mem_2_2_2.Args[0];
        if (mem_2_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt5 = mem_2_2_2_0.Type;
        var o5 = auxIntToInt64(mem_2_2_2_0.AuxInt);
        p5 = mem_2_2_2_0.Args[0];
        d4 = mem_2_2_2.Args[1];
        var mem_2_2_2_2 = mem_2_2_2.Args[2];
        if (mem_2_2_2_2.Op != OpZero || auxIntToInt64(mem_2_2_2_2.AuxInt) != n) {
            break;
        }
        var t6 = auxToType(mem_2_2_2_2.Aux);
        var p6 = mem_2_2_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && isSamePtr(p5, p6) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && t6.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size() && n >= o5 + t5.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        v5 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v5.Aux = typeToAux(t5);
        v6 = b.NewValue0(v.Pos, OpOffPtr, tt5);
        v6.AuxInt = int64ToAuxInt(o5);
        v6.AddArg(dst);
        var v7 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v7.AuxInt = int64ToAuxInt(n);
        v7.Aux = typeToAux(t1);
        v7.AddArg2(dst, mem);
        v5.AddArg3(v6, d4, v7);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} op2:(OffPtr <tt2> [o2] p2) d1 (Zero {t3} [n] p3 _))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && n >= o2 + t2.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Zero {t1} [n] dst mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        op2 = mem_0.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        tt2 = op2.Type;
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpZero || auxIntToInt64(mem_0_2.AuxInt) != n) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        p3 = mem_0_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && registerizable(b, t2) && n >= o2 + t2.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v1.AuxInt = int64ToAuxInt(n);
        v1.Aux = typeToAux(t1);
        v1.AddArg2(dst, mem);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Zero {t4} [n] p4 _)))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && n >= o2 + t2.Size() && n >= o3 + t3.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Zero {t1} [n] dst mem)))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        var mem_0_0 = mem_0.Args[0];
        if (mem_0_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0_0.Type;
        o2 = auxIntToInt64(mem_0_0.AuxInt);
        p2 = mem_0_0.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        _ = mem_0_2.Args[2];
        var mem_0_2_0 = mem_0_2.Args[0];
        if (mem_0_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_0_2_0.Type;
        o3 = auxIntToInt64(mem_0_2_0.AuxInt);
        p3 = mem_0_2_0.Args[0];
        d2 = mem_0_2.Args[1];
        mem_0_2_2 = mem_0_2.Args[2];
        if (mem_0_2_2.Op != OpZero || auxIntToInt64(mem_0_2_2.AuxInt) != n) {
            break;
        }
        t4 = auxToType(mem_0_2_2.Aux);
        p4 = mem_0_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && n >= o2 + t2.Size() && n >= o3 + t3.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v3.AuxInt = int64ToAuxInt(n);
        v3.Aux = typeToAux(t1);
        v3.AddArg2(dst, mem);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Store {t4} (OffPtr <tt4> [o4] p4) d3 (Zero {t5} [n] p5 _))))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Zero {t1} [n] dst mem))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        mem_0_0 = mem_0.Args[0];
        if (mem_0_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0_0.Type;
        o2 = auxIntToInt64(mem_0_0.AuxInt);
        p2 = mem_0_0.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        _ = mem_0_2.Args[2];
        mem_0_2_0 = mem_0_2.Args[0];
        if (mem_0_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_0_2_0.Type;
        o3 = auxIntToInt64(mem_0_2_0.AuxInt);
        p3 = mem_0_2_0.Args[0];
        d2 = mem_0_2.Args[1];
        mem_0_2_2 = mem_0_2.Args[2];
        if (mem_0_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_0_2_2.Aux);
        _ = mem_0_2_2.Args[2];
        var mem_0_2_2_0 = mem_0_2_2.Args[0];
        if (mem_0_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt4 = mem_0_2_2_0.Type;
        o4 = auxIntToInt64(mem_0_2_2_0.AuxInt);
        p4 = mem_0_2_2_0.Args[0];
        d3 = mem_0_2_2.Args[1];
        mem_0_2_2_2 = mem_0_2_2.Args[2];
        if (mem_0_2_2_2.Op != OpZero || auxIntToInt64(mem_0_2_2_2.AuxInt) != n) {
            break;
        }
        t5 = auxToType(mem_0_2_2_2.Aux);
        p5 = mem_0_2_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        v5 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v5.AuxInt = int64ToAuxInt(n);
        v5.Aux = typeToAux(t1);
        v5.AddArg2(dst, mem);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [n] dst p1 mem:(VarDef (Store {t2} (OffPtr <tt2> [o2] p2) d1 (Store {t3} (OffPtr <tt3> [o3] p3) d2 (Store {t4} (OffPtr <tt4> [o4] p4) d3 (Store {t5} (OffPtr <tt5> [o5] p5) d4 (Zero {t6} [n] p6 _)))))))
    // cond: isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && isSamePtr(p5, p6) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && t6.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size() && n >= o5 + t5.Size()
    // result: (Store {t2} (OffPtr <tt2> [o2] dst) d1 (Store {t3} (OffPtr <tt3> [o3] dst) d2 (Store {t4} (OffPtr <tt4> [o4] dst) d3 (Store {t5} (OffPtr <tt5> [o5] dst) d4 (Zero {t1} [n] dst mem)))))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        p1 = v_1;
        mem = v_2;
        if (mem.Op != OpVarDef) {
            break;
        }
        mem_0 = mem.Args[0];
        if (mem_0.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem_0.Aux);
        _ = mem_0.Args[2];
        mem_0_0 = mem_0.Args[0];
        if (mem_0_0.Op != OpOffPtr) {
            break;
        }
        tt2 = mem_0_0.Type;
        o2 = auxIntToInt64(mem_0_0.AuxInt);
        p2 = mem_0_0.Args[0];
        d1 = mem_0.Args[1];
        mem_0_2 = mem_0.Args[2];
        if (mem_0_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_0_2.Aux);
        _ = mem_0_2.Args[2];
        mem_0_2_0 = mem_0_2.Args[0];
        if (mem_0_2_0.Op != OpOffPtr) {
            break;
        }
        tt3 = mem_0_2_0.Type;
        o3 = auxIntToInt64(mem_0_2_0.AuxInt);
        p3 = mem_0_2_0.Args[0];
        d2 = mem_0_2.Args[1];
        mem_0_2_2 = mem_0_2.Args[2];
        if (mem_0_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_0_2_2.Aux);
        _ = mem_0_2_2.Args[2];
        mem_0_2_2_0 = mem_0_2_2.Args[0];
        if (mem_0_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt4 = mem_0_2_2_0.Type;
        o4 = auxIntToInt64(mem_0_2_2_0.AuxInt);
        p4 = mem_0_2_2_0.Args[0];
        d3 = mem_0_2_2.Args[1];
        mem_0_2_2_2 = mem_0_2_2.Args[2];
        if (mem_0_2_2_2.Op != OpStore) {
            break;
        }
        t5 = auxToType(mem_0_2_2_2.Aux);
        _ = mem_0_2_2_2.Args[2];
        var mem_0_2_2_2_0 = mem_0_2_2_2.Args[0];
        if (mem_0_2_2_2_0.Op != OpOffPtr) {
            break;
        }
        tt5 = mem_0_2_2_2_0.Type;
        o5 = auxIntToInt64(mem_0_2_2_2_0.AuxInt);
        p5 = mem_0_2_2_2_0.Args[0];
        d4 = mem_0_2_2_2.Args[1];
        var mem_0_2_2_2_2 = mem_0_2_2_2.Args[2];
        if (mem_0_2_2_2_2.Op != OpZero || auxIntToInt64(mem_0_2_2_2_2.AuxInt) != n) {
            break;
        }
        t6 = auxToType(mem_0_2_2_2_2.Aux);
        p6 = mem_0_2_2_2_2.Args[0];
        if (!(isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && isSamePtr(p5, p6) && t2.Alignment() <= t1.Alignment() && t3.Alignment() <= t1.Alignment() && t4.Alignment() <= t1.Alignment() && t5.Alignment() <= t1.Alignment() && t6.Alignment() <= t1.Alignment() && registerizable(b, t2) && registerizable(b, t3) && registerizable(b, t4) && registerizable(b, t5) && n >= o2 + t2.Size() && n >= o3 + t3.Size() && n >= o4 + t4.Size() && n >= o5 + t5.Size())) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t2);
        v0 = b.NewValue0(v.Pos, OpOffPtr, tt2);
        v0.AuxInt = int64ToAuxInt(o2);
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpOffPtr, tt3);
        v2.AuxInt = int64ToAuxInt(o3);
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t4);
        v4 = b.NewValue0(v.Pos, OpOffPtr, tt4);
        v4.AuxInt = int64ToAuxInt(o4);
        v4.AddArg(dst);
        v5 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v5.Aux = typeToAux(t5);
        v6 = b.NewValue0(v.Pos, OpOffPtr, tt5);
        v6.AuxInt = int64ToAuxInt(o5);
        v6.AddArg(dst);
        v7 = b.NewValue0(v.Pos, OpZero, types.TypeMem);
        v7.AuxInt = int64ToAuxInt(n);
        v7.Aux = typeToAux(t1);
        v7.AddArg2(dst, mem);
        v5.AddArg3(v6, d4, v7);
        v3.AddArg3(v4, d3, v5);
        v1.AddArg3(v2, d2, v3);
        v.AddArg3(v0, d1, v1);
        return true;

    } 
    // match: (Move {t1} [s] dst tmp1 midmem:(Move {t2} [s] tmp2 src _))
    // cond: t1.Compare(t2) == types.CMPeq && isSamePtr(tmp1, tmp2) && isStackPtr(src) && !isVolatile(src) && disjoint(src, s, tmp2, s) && (disjoint(src, s, dst, s) || isInlinableMemmove(dst, src, s, config))
    // result: (Move {t1} [s] dst src midmem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        var tmp1 = v_1;
        var midmem = v_2;
        if (midmem.Op != OpMove || auxIntToInt64(midmem.AuxInt) != s) {
            break;
        }
        t2 = auxToType(midmem.Aux);
        src = midmem.Args[1];
        var tmp2 = midmem.Args[0];
        if (!(t1.Compare(t2) == types.CMPeq && isSamePtr(tmp1, tmp2) && isStackPtr(src) && !isVolatile(src) && disjoint(src, s, tmp2, s) && (disjoint(src, s, dst, s) || isInlinableMemmove(dst, src, s, config)))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(s);
        v.Aux = typeToAux(t1);
        v.AddArg3(dst, src, midmem);
        return true;

    } 
    // match: (Move {t1} [s] dst tmp1 midmem:(VarDef (Move {t2} [s] tmp2 src _)))
    // cond: t1.Compare(t2) == types.CMPeq && isSamePtr(tmp1, tmp2) && isStackPtr(src) && !isVolatile(src) && disjoint(src, s, tmp2, s) && (disjoint(src, s, dst, s) || isInlinableMemmove(dst, src, s, config))
    // result: (Move {t1} [s] dst src midmem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        t1 = auxToType(v.Aux);
        dst = v_0;
        tmp1 = v_1;
        midmem = v_2;
        if (midmem.Op != OpVarDef) {
            break;
        }
        var midmem_0 = midmem.Args[0];
        if (midmem_0.Op != OpMove || auxIntToInt64(midmem_0.AuxInt) != s) {
            break;
        }
        t2 = auxToType(midmem_0.Aux);
        src = midmem_0.Args[1];
        tmp2 = midmem_0.Args[0];
        if (!(t1.Compare(t2) == types.CMPeq && isSamePtr(tmp1, tmp2) && isStackPtr(src) && !isVolatile(src) && disjoint(src, s, tmp2, s) && (disjoint(src, s, dst, s) || isInlinableMemmove(dst, src, s, config)))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(s);
        v.Aux = typeToAux(t1);
        v.AddArg3(dst, src, midmem);
        return true;

    } 
    // match: (Move dst src mem)
    // cond: isSamePtr(dst, src)
    // result: mem
    while (true) {
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(isSamePtr(dst, src))) {
            break;
        }
        v.copyOf(mem);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mul16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                var d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 (Const16 [1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 (Const16 [-1]) x)
    // result: (Neg16 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpNeg16);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 <t> n (Const16 [c]))
    // cond: isPowerOfTwo16(c)
    // result: (Lsh16x64 <t> n (Const64 <typ.UInt64> [log16(c)]))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt16(v_1.AuxInt);
                if (!(isPowerOfTwo16(c))) {
                    continue;
                }

                v.reset(OpLsh16x64);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(log16(c));
                v.AddArg2(n, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 <t> n (Const16 [c]))
    // cond: t.IsSigned() && isPowerOfTwo16(-c)
    // result: (Neg16 (Lsh16x64 <t> n (Const64 <typ.UInt64> [log16(-c)])))
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                n = v_0;
                if (v_1.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt16(v_1.AuxInt);
                if (!(t.IsSigned() && isPowerOfTwo16(-c))) {
                    continue;
                }

                v.reset(OpNeg16);
                v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
                var v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(log16(-c));
                v0.AddArg2(n, v1);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 (Mul16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Mul16 i (Mul16 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                            continue;
                        }

                        v.reset(OpMul16);
                        v0 = b.NewValue0(v.Pos, OpMul16, t);
                        v0.AddArg2(x, z);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul16 (Const16 <t> [c]) (Mul16 (Const16 <t> [d]) x))
    // result: (Mul16 (Const16 <t> [c*d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpMul16) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpMul16);
                        v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c * d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mul32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Const32 [1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Const32 [-1]) x)
    // result: (Neg32 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpNeg32);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 <t> n (Const32 [c]))
    // cond: isPowerOfTwo32(c)
    // result: (Lsh32x64 <t> n (Const64 <typ.UInt64> [log32(c)]))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(isPowerOfTwo32(c))) {
                    continue;
                }

                v.reset(OpLsh32x64);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(log32(c));
                v.AddArg2(n, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 <t> n (Const32 [c]))
    // cond: t.IsSigned() && isPowerOfTwo32(-c)
    // result: (Neg32 (Lsh32x64 <t> n (Const64 <typ.UInt64> [log32(-c)])))
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                n = v_0;
                if (v_1.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(t.IsSigned() && isPowerOfTwo32(-c))) {
                    continue;
                }

                v.reset(OpNeg32);
                v0 = b.NewValue0(v.Pos, OpLsh32x64, t);
                var v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(log32(-c));
                v0.AddArg2(n, v1);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Const32 <t> [c]) (Add32 <t> (Const32 <t> [d]) x))
    // result: (Add32 (Const32 <t> [c*d]) (Mul32 <t> (Const32 <t> [c]) x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpAdd32 || v_1.Type != t) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c * d);
                        v1 = b.NewValue0(v.Pos, OpMul32, t);
                        var v2 = b.NewValue0(v.Pos, OpConst32, t);
                        v2.AuxInt = int32ToAuxInt(c);
                        v1.AddArg2(v2, x);
                        v.AddArg2(v0, v1);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Mul32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Mul32 i (Mul32 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                            continue;
                        }

                        v.reset(OpMul32);
                        v0 = b.NewValue0(v.Pos, OpMul32, t);
                        v0.AddArg2(x, z);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32 (Const32 <t> [c]) (Mul32 (Const32 <t> [d]) x))
    // result: (Mul32 (Const32 <t> [c*d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpMul32) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpMul32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c * d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Mul32F (Const32F [c]) (Const32F [d]))
    // cond: c*d == c*d
    // result: (Const32F [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat32(v_0.AuxInt);
                if (v_1.Op != OpConst32F) {
                    continue;
                }

                var d = auxIntToFloat32(v_1.AuxInt);
                if (!(c * d == c * d)) {
                    continue;
                }

                v.reset(OpConst32F);
                v.AuxInt = float32ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32F x (Const32F [1]))
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpConst32F || auxIntToFloat32(v_1.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32F x (Const32F [-1]))
    // result: (Neg32F x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpConst32F || auxIntToFloat32(v_1.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpNeg32F);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul32F x (Const32F [2]))
    // result: (Add32F x x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpConst32F || auxIntToFloat32(v_1.AuxInt) != 2) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpAdd32F);
                v.AddArg2(x, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mul64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Const64 [1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Const64 [-1]) x)
    // result: (Neg64 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpNeg64);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 <t> n (Const64 [c]))
    // cond: isPowerOfTwo64(c)
    // result: (Lsh64x64 <t> n (Const64 <typ.UInt64> [log64(c)]))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_1.AuxInt);
                if (!(isPowerOfTwo64(c))) {
                    continue;
                }

                v.reset(OpLsh64x64);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(log64(c));
                v.AddArg2(n, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 <t> n (Const64 [c]))
    // cond: t.IsSigned() && isPowerOfTwo64(-c)
    // result: (Neg64 (Lsh64x64 <t> n (Const64 <typ.UInt64> [log64(-c)])))
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                n = v_0;
                if (v_1.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_1.AuxInt);
                if (!(t.IsSigned() && isPowerOfTwo64(-c))) {
                    continue;
                }

                v.reset(OpNeg64);
                v0 = b.NewValue0(v.Pos, OpLsh64x64, t);
                var v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(log64(-c));
                v0.AddArg2(n, v1);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Const64 <t> [c]) (Add64 <t> (Const64 <t> [d]) x))
    // result: (Add64 (Const64 <t> [c*d]) (Mul64 <t> (Const64 <t> [c]) x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpAdd64 || v_1.Type != t) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpAdd64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c * d);
                        v1 = b.NewValue0(v.Pos, OpMul64, t);
                        var v2 = b.NewValue0(v.Pos, OpConst64, t);
                        v2.AuxInt = int64ToAuxInt(c);
                        v1.AddArg2(v2, x);
                        v.AddArg2(v0, v1);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Mul64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Mul64 i (Mul64 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                            continue;
                        }

                        v.reset(OpMul64);
                        v0 = b.NewValue0(v.Pos, OpMul64, t);
                        v0.AddArg2(x, z);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64 (Const64 <t> [c]) (Mul64 (Const64 <t> [d]) x))
    // result: (Mul64 (Const64 <t> [c*d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpMul64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpMul64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c * d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Mul64F (Const64F [c]) (Const64F [d]))
    // cond: c*d == c*d
    // result: (Const64F [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat64(v_0.AuxInt);
                if (v_1.Op != OpConst64F) {
                    continue;
                }

                var d = auxIntToFloat64(v_1.AuxInt);
                if (!(c * d == c * d)) {
                    continue;
                }

                v.reset(OpConst64F);
                v.AuxInt = float64ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64F x (Const64F [1]))
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpConst64F || auxIntToFloat64(v_1.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64F x (Const64F [-1]))
    // result: (Neg64F x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpConst64F || auxIntToFloat64(v_1.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpNeg64F);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul64F x (Const64F [2]))
    // result: (Add64F x x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpConst64F || auxIntToFloat64(v_1.AuxInt) != 2) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpAdd64F);
                v.AddArg2(x, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpMul8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mul8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 (Const8 [1]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 (Const8 [-1]) x)
    // result: (Neg8 x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpNeg8);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 <t> n (Const8 [c]))
    // cond: isPowerOfTwo8(c)
    // result: (Lsh8x64 <t> n (Const64 <typ.UInt64> [log8(c)]))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt8(v_1.AuxInt);
                if (!(isPowerOfTwo8(c))) {
                    continue;
                }

                v.reset(OpLsh8x64);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(log8(c));
                v.AddArg2(n, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 <t> n (Const8 [c]))
    // cond: t.IsSigned() && isPowerOfTwo8(-c)
    // result: (Neg8 (Lsh8x64 <t> n (Const64 <typ.UInt64> [log8(-c)])))
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                n = v_0;
                if (v_1.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt8(v_1.AuxInt);
                if (!(t.IsSigned() && isPowerOfTwo8(-c))) {
                    continue;
                }

                v.reset(OpNeg8);
                v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
                var v1 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(log8(-c));
                v0.AddArg2(n, v1);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 (Mul8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Mul8 i (Mul8 <t> x z))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMul8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                            continue;
                        }

                        v.reset(OpMul8);
                        v0 = b.NewValue0(v.Pos, OpMul8, t);
                        v0.AddArg2(x, z);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Mul8 (Const8 <t> [c]) (Mul8 (Const8 <t> [d]) x))
    // result: (Mul8 (Const8 <t> [c*d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpMul8) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpMul8);
                        v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c * d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neg16 (Const16 [c]))
    // result: (Const16 [-c])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(-c);
        return true;

    } 
    // match: (Neg16 (Sub16 x y))
    // result: (Sub16 y x)
    while (true) {
        if (v_0.Op != OpSub16) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpSub16);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Neg16 (Neg16 x))
    // result: x
    while (true) {
        if (v_0.Op != OpNeg16) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Neg16 <t> (Com16 x))
    // result: (Add16 (Const16 <t> [1]) x)
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpCom16) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpAdd16);
        var v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(1);
        v.AddArg2(v0, x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neg32 (Const32 [c]))
    // result: (Const32 [-c])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(-c);
        return true;

    } 
    // match: (Neg32 (Sub32 x y))
    // result: (Sub32 y x)
    while (true) {
        if (v_0.Op != OpSub32) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpSub32);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Neg32 (Neg32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpNeg32) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Neg32 <t> (Com32 x))
    // result: (Add32 (Const32 <t> [1]) x)
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpCom32) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpAdd32);
        var v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(1);
        v.AddArg2(v0, x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Neg32F (Const32F [c]))
    // cond: c != 0
    // result: (Const32F [-c])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(-c);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neg64 (Const64 [c]))
    // result: (Const64 [-c])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(-c);
        return true;

    } 
    // match: (Neg64 (Sub64 x y))
    // result: (Sub64 y x)
    while (true) {
        if (v_0.Op != OpSub64) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpSub64);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Neg64 (Neg64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpNeg64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Neg64 <t> (Com64 x))
    // result: (Add64 (Const64 <t> [1]) x)
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpCom64) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpAdd64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Neg64F (Const64F [c]))
    // cond: c != 0
    // result: (Const64F [-c])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(-c);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeg8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neg8 (Const8 [c]))
    // result: (Const8 [-c])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(-c);
        return true;

    } 
    // match: (Neg8 (Sub8 x y))
    // result: (Sub8 y x)
    while (true) {
        if (v_0.Op != OpSub8) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpSub8);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Neg8 (Neg8 x))
    // result: x
    while (true) {
        if (v_0.Op != OpNeg8) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Neg8 <t> (Com8 x))
    // result: (Add8 (Const8 <t> [1]) x)
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpCom8) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpAdd8);
        var v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(1);
        v.AddArg2(v0, x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (Neq16 (Const16 <t> [c]) (Add16 (Const16 <t> [d]) x))
    // result: (Neq16 (Const16 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpAdd16) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpNeq16);
                        var v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq16 (Const16 [c]) (Const16 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq16 n (Lsh16x64 (Rsh16x64 (Add16 <t> n (Rsh16Ux64 <t> (Rsh16x64 <t> n (Const64 <typ.UInt64> [15])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 15 && kbar == 16 - k
    // result: (Neq16 (And16 <t> n (Const16 <t> [1<<uint(k)-1])) (Const16 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh16x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh16x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd16) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh16Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh16x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 15) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 15 && kbar == 16 - k)) {
                            continue;
                        }

                        v.reset(OpNeq16);
                        v0 = b.NewValue0(v.Pos, OpAnd16, t);
                        var v1 = b.NewValue0(v.Pos, OpConst16, t);
                        v1.AuxInt = int16ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        var v2 = b.NewValue0(v.Pos, OpConst16, t);
                        v2.AuxInt = int16ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq16 s:(Sub16 x y) (Const16 [0]))
    // cond: s.Uses == 1
    // result: (Neq16 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s = v_0;
                if (s.Op != OpSub16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpNeq16);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq16 (And16 <t> x (Const16 <t> [y])) (Const16 <t> [y]))
    // cond: oneBit16(y)
    // result: (Eq16 (And16 <t> x (Const16 <t> [y])) (Const16 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst16 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt16(v_0_1.AuxInt);
                        if (v_1.Op != OpConst16 || v_1.Type != t || auxIntToInt16(v_1.AuxInt) != y || !(oneBit16(y))) {
                            continue;
                        }

                        v.reset(OpEq16);
                        v0 = b.NewValue0(v.Pos, OpAnd16, t);
                        v1 = b.NewValue0(v.Pos, OpConst16, t);
                        v1.AuxInt = int16ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst16, t);
                        v2.AuxInt = int16ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq32 x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (Neq32 (Const32 <t> [c]) (Add32 (Const32 <t> [d]) x))
    // result: (Neq32 (Const32 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpAdd32) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpNeq32);
                        var v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq32 (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq32 n (Lsh32x64 (Rsh32x64 (Add32 <t> n (Rsh32Ux64 <t> (Rsh32x64 <t> n (Const64 <typ.UInt64> [31])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 31 && kbar == 32 - k
    // result: (Neq32 (And32 <t> n (Const32 <t> [1<<uint(k)-1])) (Const32 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh32x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh32x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd32) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh32Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh32x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 31) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 31 && kbar == 32 - k)) {
                            continue;
                        }

                        v.reset(OpNeq32);
                        v0 = b.NewValue0(v.Pos, OpAnd32, t);
                        var v1 = b.NewValue0(v.Pos, OpConst32, t);
                        v1.AuxInt = int32ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        var v2 = b.NewValue0(v.Pos, OpConst32, t);
                        v2.AuxInt = int32ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq32 s:(Sub32 x y) (Const32 [0]))
    // cond: s.Uses == 1
    // result: (Neq32 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s = v_0;
                if (s.Op != OpSub32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst32 || auxIntToInt32(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpNeq32);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq32 (And32 <t> x (Const32 <t> [y])) (Const32 <t> [y]))
    // cond: oneBit32(y)
    // result: (Eq32 (And32 <t> x (Const32 <t> [y])) (Const32 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst32 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt32(v_0_1.AuxInt);
                        if (v_1.Op != OpConst32 || v_1.Type != t || auxIntToInt32(v_1.AuxInt) != y || !(oneBit32(y))) {
                            continue;
                        }

                        v.reset(OpEq32);
                        v0 = b.NewValue0(v.Pos, OpAnd32, t);
                        v1 = b.NewValue0(v.Pos, OpConst32, t);
                        v1.AuxInt = int32ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst32, t);
                        v2.AuxInt = int32ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Neq32F (Const32F [c]) (Const32F [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat32(v_0.AuxInt);
                if (v_1.Op != OpConst32F) {
                    continue;
                }

                var d = auxIntToFloat32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq64 x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (Neq64 (Const64 <t> [c]) (Add64 (Const64 <t> [d]) x))
    // result: (Neq64 (Const64 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpAdd64) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpNeq64);
                        var v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq64 (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq64 n (Lsh64x64 (Rsh64x64 (Add64 <t> n (Rsh64Ux64 <t> (Rsh64x64 <t> n (Const64 <typ.UInt64> [63])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 63 && kbar == 64 - k
    // result: (Neq64 (And64 <t> n (Const64 <t> [1<<uint(k)-1])) (Const64 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh64x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh64x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd64) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh64Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh64x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 63) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 63 && kbar == 64 - k)) {
                            continue;
                        }

                        v.reset(OpNeq64);
                        v0 = b.NewValue0(v.Pos, OpAnd64, t);
                        var v1 = b.NewValue0(v.Pos, OpConst64, t);
                        v1.AuxInt = int64ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        var v2 = b.NewValue0(v.Pos, OpConst64, t);
                        v2.AuxInt = int64ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq64 s:(Sub64 x y) (Const64 [0]))
    // cond: s.Uses == 1
    // result: (Neq64 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s = v_0;
                if (s.Op != OpSub64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpNeq64);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq64 (And64 <t> x (Const64 <t> [y])) (Const64 <t> [y]))
    // cond: oneBit64(y)
    // result: (Eq64 (And64 <t> x (Const64 <t> [y])) (Const64 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst64 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt64(v_0_1.AuxInt);
                        if (v_1.Op != OpConst64 || v_1.Type != t || auxIntToInt64(v_1.AuxInt) != y || !(oneBit64(y))) {
                            continue;
                        }

                        v.reset(OpEq64);
                        v0 = b.NewValue0(v.Pos, OpAnd64, t);
                        v1 = b.NewValue0(v.Pos, OpConst64, t);
                        v1.AuxInt = int64ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst64, t);
                        v2.AuxInt = int64ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Neq64F (Const64F [c]) (Const64F [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64F) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToFloat64(v_0.AuxInt);
                if (v_1.Op != OpConst64F) {
                    continue;
                }

                var d = auxIntToFloat64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (Neq8 (Const8 <t> [c]) (Add8 (Const8 <t> [d]) x))
    // result: (Neq8 (Const8 <t> [c-d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var t = v_0.Type;
                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpAdd8) {
                    continue;
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpNeq8);
                        var v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c - d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq8 (Const8 [c]) (Const8 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq8 n (Lsh8x64 (Rsh8x64 (Add8 <t> n (Rsh8Ux64 <t> (Rsh8x64 <t> n (Const64 <typ.UInt64> [ 7])) (Const64 <typ.UInt64> [kbar]))) (Const64 <typ.UInt64> [k])) (Const64 <typ.UInt64> [k])) )
    // cond: k > 0 && k < 7 && kbar == 8 - k
    // result: (Neq8 (And8 <t> n (Const8 <t> [1<<uint(k)-1])) (Const8 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var n = v_0;
                if (v_1.Op != OpLsh8x64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpRsh8x64) {
                    continue;
                }

                _ = v_1_0.Args[1];
                var v_1_0_0 = v_1_0.Args[0];
                if (v_1_0_0.Op != OpAdd8) {
                    continue;
                }

                t = v_1_0_0.Type;
                _ = v_1_0_0.Args[1];
                var v_1_0_0_0 = v_1_0_0.Args[0];
                var v_1_0_0_1 = v_1_0_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (n != v_1_0_0_0 || v_1_0_0_1.Op != OpRsh8Ux64 || v_1_0_0_1.Type != t) {
                            continue;
                        (_i1, v_1_0_0_0, v_1_0_0_1) = (_i1 + 1, v_1_0_0_1, v_1_0_0_0);
                        }

                        _ = v_1_0_0_1.Args[1];
                        var v_1_0_0_1_0 = v_1_0_0_1.Args[0];
                        if (v_1_0_0_1_0.Op != OpRsh8x64 || v_1_0_0_1_0.Type != t) {
                            continue;
                        }

                        _ = v_1_0_0_1_0.Args[1];
                        if (n != v_1_0_0_1_0.Args[0]) {
                            continue;
                        }

                        var v_1_0_0_1_0_1 = v_1_0_0_1_0.Args[1];
                        if (v_1_0_0_1_0_1.Op != OpConst64 || v_1_0_0_1_0_1.Type != typ.UInt64 || auxIntToInt64(v_1_0_0_1_0_1.AuxInt) != 7) {
                            continue;
                        }

                        var v_1_0_0_1_1 = v_1_0_0_1.Args[1];
                        if (v_1_0_0_1_1.Op != OpConst64 || v_1_0_0_1_1.Type != typ.UInt64) {
                            continue;
                        }

                        var kbar = auxIntToInt64(v_1_0_0_1_1.AuxInt);
                        var v_1_0_1 = v_1_0.Args[1];
                        if (v_1_0_1.Op != OpConst64 || v_1_0_1.Type != typ.UInt64) {
                            continue;
                        }

                        var k = auxIntToInt64(v_1_0_1.AuxInt);
                        v_1_1 = v_1.Args[1];
                        if (v_1_1.Op != OpConst64 || v_1_1.Type != typ.UInt64 || auxIntToInt64(v_1_1.AuxInt) != k || !(k > 0 && k < 7 && kbar == 8 - k)) {
                            continue;
                        }

                        v.reset(OpNeq8);
                        v0 = b.NewValue0(v.Pos, OpAnd8, t);
                        var v1 = b.NewValue0(v.Pos, OpConst8, t);
                        v1.AuxInt = int8ToAuxInt(1 << (int)(uint(k)) - 1);
                        v0.AddArg2(n, v1);
                        var v2 = b.NewValue0(v.Pos, OpConst8, t);
                        v2.AuxInt = int8ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq8 s:(Sub8 x y) (Const8 [0]))
    // cond: s.Uses == 1
    // result: (Neq8 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s = v_0;
                if (s.Op != OpSub8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = s.Args[1];
                x = s.Args[0];
                if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != 0 || !(s.Uses == 1)) {
                    continue;
                }

                v.reset(OpNeq8);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Neq8 (And8 <t> x (Const8 <t> [y])) (Const8 <t> [y]))
    // cond: oneBit8(y)
    // result: (Eq8 (And8 <t> x (Const8 <t> [y])) (Const8 <t> [0]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst8 || v_0_1.Type != t) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        y = auxIntToInt8(v_0_1.AuxInt);
                        if (v_1.Op != OpConst8 || v_1.Type != t || auxIntToInt8(v_1.AuxInt) != y || !(oneBit8(y))) {
                            continue;
                        }

                        v.reset(OpEq8);
                        v0 = b.NewValue0(v.Pos, OpAnd8, t);
                        v1 = b.NewValue0(v.Pos, OpConst8, t);
                        v1.AuxInt = int8ToAuxInt(y);
                        v0.AddArg2(x, v1);
                        v2 = b.NewValue0(v.Pos, OpConst8, t);
                        v2.AuxInt = int8ToAuxInt(0);
                        v.AddArg2(v0, v2);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NeqB (ConstBool [c]) (ConstBool [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToBool(v_0.AuxInt);
                if (v_1.Op != OpConstBool) {
                    continue;
                }

                var d = auxIntToBool(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqB (ConstBool [false]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != false) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqB (ConstBool [true]) x)
    // result: (Not x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstBool || auxIntToBool(v_0.AuxInt) != true) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpNot);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqB (Not x) (Not y))
    // result: (NeqB x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpNot) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[0];
                if (v_1.Op != OpNot) {
                    continue;
                }

                var y = v_1.Args[0];
                v.reset(OpNeqB);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeqInter(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NeqInter x y)
    // result: (NeqPtr (ITab x) (ITab y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpNeqPtr);
        var v0 = b.NewValue0(v.Pos, OpITab, typ.Uintptr);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpITab, typ.Uintptr);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValuegeneric_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NeqPtr x x)
    // result: (ConstBool [false])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(false);
        return true;

    } 
    // match: (NeqPtr (Addr {x} _) (Addr {y} _))
    // result: (ConstBool [x != y])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpAddr) {
                    continue;
                }

                var y = auxToSym(v_1.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (Addr {x} _) (OffPtr [o] (Addr {y} _)))
    // result: (ConstBool [x != y || o != 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                var o = auxIntToInt64(v_1.AuxInt);
                var v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y || o != 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr [o1] (Addr {x} _)) (OffPtr [o2] (Addr {y} _)))
    // result: (ConstBool [x != y || o1 != o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var o1 = auxIntToInt64(v_0.AuxInt);
                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpAddr) {
                    continue;
                }

                x = auxToSym(v_0_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                var o2 = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y || o1 != o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (LocalAddr {x} _ _) (LocalAddr {y} _ _))
    // result: (ConstBool [x != y])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (LocalAddr {x} _ _) (OffPtr [o] (LocalAddr {y} _ _)))
    // result: (ConstBool [x != y || o != 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = auxToSym(v_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y || o != 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr [o1] (LocalAddr {x} _ _)) (OffPtr [o2] (LocalAddr {y} _ _)))
    // result: (ConstBool [x != y || o1 != o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr) {
                    continue;
                }

                x = auxToSym(v_0_0.Aux);
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o2 = auxIntToInt64(v_1.AuxInt);
                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpLocalAddr) {
                    continue;
                }

                y = auxToSym(v_1_0.Aux);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(x != y || o1 != o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr [o1] p1) p2)
    // cond: isSamePtr(p1, p2)
    // result: (ConstBool [o1 != 0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                var p1 = v_0.Args[0];
                var p2 = v_1;
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(o1 != 0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr [o1] p1) (OffPtr [o2] p2))
    // cond: isSamePtr(p1, p2)
    // result: (ConstBool [o1 != o2])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = auxIntToInt64(v_0.AuxInt);
                p1 = v_0.Args[0];
                if (v_1.Op != OpOffPtr) {
                    continue;
                }

                o2 = auxIntToInt64(v_1.AuxInt);
                p2 = v_1.Args[0];
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(o1 != o2);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (Const32 [c]) (Const32 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (Const64 [c]) (Const64 [d]))
    // result: (ConstBool [c != d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(c != d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (LocalAddr _ _) (Addr _))
    // result: (ConstBool [true])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr || v_1.Op != OpAddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr (LocalAddr _ _)) (Addr _))
    // result: (ConstBool [true])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr || v_1.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (LocalAddr _ _) (OffPtr (Addr _)))
    // result: (ConstBool [true])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLocalAddr || v_1.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (OffPtr (LocalAddr _ _)) (OffPtr (Addr _)))
    // result: (ConstBool [true])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOffPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpLocalAddr || v_1.Op != OpOffPtr) {
                    continue;
                }

                v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpAddr) {
                    continue;
                }

                v.reset(OpConstBool);
                v.AuxInt = boolToAuxInt(true);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (AddPtr p1 o1) p2)
    // cond: isSamePtr(p1, p2)
    // result: (IsNonNil o1)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAddPtr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                o1 = v_0.Args[1];
                p1 = v_0.Args[0];
                p2 = v_1;
                if (!(isSamePtr(p1, p2))) {
                    continue;
                }

                v.reset(OpIsNonNil);
                v.AddArg(o1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (Const32 [0]) p)
    // result: (IsNonNil p)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var p = v_1;
                v.reset(OpIsNonNil);
                v.AddArg(p);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (Const64 [0]) p)
    // result: (IsNonNil p)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                p = v_1;
                v.reset(OpIsNonNil);
                v.AddArg(p);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (NeqPtr (ConstNil) p)
    // result: (IsNonNil p)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConstNil) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                p = v_1;
                v.reset(OpIsNonNil);
                v.AddArg(p);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNeqSlice(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NeqSlice x y)
    // result: (NeqPtr (SlicePtr x) (SlicePtr y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpNeqPtr);
        var v0 = b.NewValue0(v.Pos, OpSlicePtr, typ.BytePtr);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSlicePtr, typ.BytePtr);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValuegeneric_OpNilCheck(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var fe = b.Func.fe; 
    // match: (NilCheck (GetG mem) mem)
    // result: mem
    while (true) {
        if (v_0.Op != OpGetG) {
            break;
        }
        var mem = v_0.Args[0];
        if (mem != v_1) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (NilCheck (SelectN [0] call:(StaticLECall _ _)) _)
    // cond: isSameCall(call.Aux, "runtime.newobject") && warnRule(fe.Debug_checknil(), v, "removed nil check")
    // result: (Invalid)
    while (true) {
        if (v_0.Op != OpSelectN || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        var call = v_0.Args[0];
        if (call.Op != OpStaticLECall || len(call.Args) != 2 || !(isSameCall(call.Aux, "runtime.newobject") && warnRule(fe.Debug_checknil(), v, "removed nil check"))) {
            break;
        }
        v.reset(OpInvalid);
        return true;

    } 
    // match: (NilCheck (OffPtr (SelectN [0] call:(StaticLECall _ _))) _)
    // cond: isSameCall(call.Aux, "runtime.newobject") && warnRule(fe.Debug_checknil(), v, "removed nil check")
    // result: (Invalid)
    while (true) {
        if (v_0.Op != OpOffPtr) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSelectN || auxIntToInt64(v_0_0.AuxInt) != 0) {
            break;
        }
        call = v_0_0.Args[0];
        if (call.Op != OpStaticLECall || len(call.Args) != 2 || !(isSameCall(call.Aux, "runtime.newobject") && warnRule(fe.Debug_checknil(), v, "removed nil check"))) {
            break;
        }
        v.reset(OpInvalid);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not (ConstBool [c]))
    // result: (ConstBool [!c])
    while (true) {
        if (v_0.Op != OpConstBool) {
            break;
        }
        var c = auxIntToBool(v_0.AuxInt);
        v.reset(OpConstBool);
        v.AuxInt = boolToAuxInt(!c);
        return true;

    } 
    // match: (Not (Eq64 x y))
    // result: (Neq64 x y)
    while (true) {
        if (v_0.Op != OpEq64) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpNeq64);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Eq32 x y))
    // result: (Neq32 x y)
    while (true) {
        if (v_0.Op != OpEq32) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeq32);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Eq16 x y))
    // result: (Neq16 x y)
    while (true) {
        if (v_0.Op != OpEq16) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeq16);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Eq8 x y))
    // result: (Neq8 x y)
    while (true) {
        if (v_0.Op != OpEq8) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeq8);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (EqB x y))
    // result: (NeqB x y)
    while (true) {
        if (v_0.Op != OpEqB) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeqB);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (EqPtr x y))
    // result: (NeqPtr x y)
    while (true) {
        if (v_0.Op != OpEqPtr) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeqPtr);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Eq64F x y))
    // result: (Neq64F x y)
    while (true) {
        if (v_0.Op != OpEq64F) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeq64F);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Eq32F x y))
    // result: (Neq32F x y)
    while (true) {
        if (v_0.Op != OpEq32F) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpNeq32F);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq64 x y))
    // result: (Eq64 x y)
    while (true) {
        if (v_0.Op != OpNeq64) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq64);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq32 x y))
    // result: (Eq32 x y)
    while (true) {
        if (v_0.Op != OpNeq32) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq32);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq16 x y))
    // result: (Eq16 x y)
    while (true) {
        if (v_0.Op != OpNeq16) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq16);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq8 x y))
    // result: (Eq8 x y)
    while (true) {
        if (v_0.Op != OpNeq8) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq8);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (NeqB x y))
    // result: (EqB x y)
    while (true) {
        if (v_0.Op != OpNeqB) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEqB);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (NeqPtr x y))
    // result: (EqPtr x y)
    while (true) {
        if (v_0.Op != OpNeqPtr) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEqPtr);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq64F x y))
    // result: (Eq64F x y)
    while (true) {
        if (v_0.Op != OpNeq64F) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq64F);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Neq32F x y))
    // result: (Eq32F x y)
    while (true) {
        if (v_0.Op != OpNeq32F) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpEq32F);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Not (Less64 x y))
    // result: (Leq64 y x)
    while (true) {
        if (v_0.Op != OpLess64) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq64);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less32 x y))
    // result: (Leq32 y x)
    while (true) {
        if (v_0.Op != OpLess32) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq32);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less16 x y))
    // result: (Leq16 y x)
    while (true) {
        if (v_0.Op != OpLess16) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq16);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less8 x y))
    // result: (Leq8 y x)
    while (true) {
        if (v_0.Op != OpLess8) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq8);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less64U x y))
    // result: (Leq64U y x)
    while (true) {
        if (v_0.Op != OpLess64U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq64U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less32U x y))
    // result: (Leq32U y x)
    while (true) {
        if (v_0.Op != OpLess32U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq32U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less16U x y))
    // result: (Leq16U y x)
    while (true) {
        if (v_0.Op != OpLess16U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq16U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Less8U x y))
    // result: (Leq8U y x)
    while (true) {
        if (v_0.Op != OpLess8U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLeq8U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq64 x y))
    // result: (Less64 y x)
    while (true) {
        if (v_0.Op != OpLeq64) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess64);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq32 x y))
    // result: (Less32 y x)
    while (true) {
        if (v_0.Op != OpLeq32) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess32);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq16 x y))
    // result: (Less16 y x)
    while (true) {
        if (v_0.Op != OpLeq16) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess16);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq8 x y))
    // result: (Less8 y x)
    while (true) {
        if (v_0.Op != OpLeq8) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess8);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq64U x y))
    // result: (Less64U y x)
    while (true) {
        if (v_0.Op != OpLeq64U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess64U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq32U x y))
    // result: (Less32U y x)
    while (true) {
        if (v_0.Op != OpLeq32U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess32U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq16U x y))
    // result: (Less16U y x)
    while (true) {
        if (v_0.Op != OpLeq16U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess16U);
        v.AddArg2(y, x);
        return true;

    } 
    // match: (Not (Leq8U x y))
    // result: (Less8U y x)
    while (true) {
        if (v_0.Op != OpLeq8U) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpLess8U);
        v.AddArg2(y, x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (OffPtr (OffPtr p [y]) [x])
    // result: (OffPtr p [x+y])
    while (true) {
        var x = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpOffPtr) {
            break;
        }
        var y = auxIntToInt64(v_0.AuxInt);
        var p = v_0.Args[0];
        v.reset(OpOffPtr);
        v.AuxInt = int64ToAuxInt(x + y);
        v.AddArg(p);
        return true;

    } 
    // match: (OffPtr p [0])
    // cond: v.Type.Compare(p.Type) == types.CMPeq
    // result: p
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        p = v_0;
        if (!(v.Type.Compare(p.Type) == types.CMPeq)) {
            break;
        }
        v.copyOf(p);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOr16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Or16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c|d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                var d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(c | d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Or16 (Const16 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 (Const16 [-1]) _)
    // result: (Const16 [-1])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(-1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 x (Or16 x y))
    // result: (Or16 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpOr16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpOr16);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 (And16 x (Const16 [c2])) (Const16 <t> [c1]))
    // cond: ^(c1 | c2) == 0
    // result: (Or16 (Const16 <t> [c1]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var c2 = auxIntToInt16(v_0_1.AuxInt);
                        if (v_1.Op != OpConst16) {
                            continue;
                        }

                        var t = v_1.Type;
                        var c1 = auxIntToInt16(v_1.AuxInt);
                        if (!(~(c1 | c2) == 0)) {
                            continue;
                        }

                        v.reset(OpOr16);
                        var v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c1);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 (Or16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Or16 i (Or16 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOr16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                            continue;
                        }

                        v.reset(OpOr16);
                        v0 = b.NewValue0(v.Pos, OpOr16, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or16 (Const16 <t> [c]) (Or16 (Const16 <t> [d]) x))
    // result: (Or16 (Const16 <t> [c|d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpOr16) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpOr16);
                        v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c | d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOr32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Or32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c|d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(c | d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Or32 (Const32 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 (Const32 [-1]) _)
    // result: (Const32 [-1])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(-1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 x (Or32 x y))
    // result: (Or32 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpOr32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpOr32);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 (And32 x (Const32 [c2])) (Const32 <t> [c1]))
    // cond: ^(c1 | c2) == 0
    // result: (Or32 (Const32 <t> [c1]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var c2 = auxIntToInt32(v_0_1.AuxInt);
                        if (v_1.Op != OpConst32) {
                            continue;
                        }

                        var t = v_1.Type;
                        var c1 = auxIntToInt32(v_1.AuxInt);
                        if (!(~(c1 | c2) == 0)) {
                            continue;
                        }

                        v.reset(OpOr32);
                        var v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c1);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 (Or32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Or32 i (Or32 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOr32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                            continue;
                        }

                        v.reset(OpOr32);
                        v0 = b.NewValue0(v.Pos, OpOr32, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or32 (Const32 <t> [c]) (Or32 (Const32 <t> [d]) x))
    // result: (Or32 (Const32 <t> [c|d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpOr32) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpOr32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c | d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOr64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Or64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c|d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(c | d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Or64 (Const64 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 (Const64 [-1]) _)
    // result: (Const64 [-1])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(-1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 x (Or64 x y))
    // result: (Or64 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpOr64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpOr64);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 (And64 x (Const64 [c2])) (Const64 <t> [c1]))
    // cond: ^(c1 | c2) == 0
    // result: (Or64 (Const64 <t> [c1]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var c2 = auxIntToInt64(v_0_1.AuxInt);
                        if (v_1.Op != OpConst64) {
                            continue;
                        }

                        var t = v_1.Type;
                        var c1 = auxIntToInt64(v_1.AuxInt);
                        if (!(~(c1 | c2) == 0)) {
                            continue;
                        }

                        v.reset(OpOr64);
                        var v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c1);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 (Or64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Or64 i (Or64 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOr64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                            continue;
                        }

                        v.reset(OpOr64);
                        v0 = b.NewValue0(v.Pos, OpOr64, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or64 (Const64 <t> [c]) (Or64 (Const64 <t> [d]) x))
    // result: (Or64 (Const64 <t> [c|d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpOr64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpOr64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c | d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOr8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Or8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c|d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(c | d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 x x)
    // result: x
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Or8 (Const8 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 (Const8 [-1]) _)
    // result: (Const8 [-1])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(-1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 x (Or8 x y))
    // result: (Or8 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpOr8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.reset(OpOr8);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 (And8 x (Const8 [c2])) (Const8 <t> [c1]))
    // cond: ^(c1 | c2) == 0
    // result: (Or8 (Const8 <t> [c1]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpAnd8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x = v_0_0;
                        if (v_0_1.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var c2 = auxIntToInt8(v_0_1.AuxInt);
                        if (v_1.Op != OpConst8) {
                            continue;
                        }

                        var t = v_1.Type;
                        var c1 = auxIntToInt8(v_1.AuxInt);
                        if (!(~(c1 | c2) == 0)) {
                            continue;
                        }

                        v.reset(OpOr8);
                        var v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c1);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 (Or8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Or8 i (Or8 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpOr8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                            continue;
                        }

                        v.reset(OpOr8);
                        v0 = b.NewValue0(v.Pos, OpOr8, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Or8 (Const8 <t> [c]) (Or8 (Const8 <t> [d]) x))
    // result: (Or8 (Const8 <t> [c|d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpOr8) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpOr8);
                        v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c | d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpOrB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (OrB (Less64 (Const64 [c]) x) (Less64 x (Const64 [d])))
    // cond: c >= d
    // result: (Less64U (Const64 <x.Type> [c-d]) (Sub64 <x.Type> x (Const64 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                var c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLess64U);
                var v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d);
                var v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                var v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq64 (Const64 [c]) x) (Less64 x (Const64 [d])))
    // cond: c >= d
    // result: (Leq64U (Const64 <x.Type> [c-d]) (Sub64 <x.Type> x (Const64 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less32 (Const32 [c]) x) (Less32 x (Const32 [d])))
    // cond: c >= d
    // result: (Less32U (Const32 <x.Type> [c-d]) (Sub32 <x.Type> x (Const32 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq32 (Const32 [c]) x) (Less32 x (Const32 [d])))
    // cond: c >= d
    // result: (Leq32U (Const32 <x.Type> [c-d]) (Sub32 <x.Type> x (Const32 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less16 (Const16 [c]) x) (Less16 x (Const16 [d])))
    // cond: c >= d
    // result: (Less16U (Const16 <x.Type> [c-d]) (Sub16 <x.Type> x (Const16 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq16 (Const16 [c]) x) (Less16 x (Const16 [d])))
    // cond: c >= d
    // result: (Leq16U (Const16 <x.Type> [c-d]) (Sub16 <x.Type> x (Const16 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less8 (Const8 [c]) x) (Less8 x (Const8 [d])))
    // cond: c >= d
    // result: (Less8U (Const8 <x.Type> [c-d]) (Sub8 <x.Type> x (Const8 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq8 (Const8 [c]) x) (Less8 x (Const8 [d])))
    // cond: c >= d
    // result: (Leq8U (Const8 <x.Type> [c-d]) (Sub8 <x.Type> x (Const8 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(c >= d)) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less64 (Const64 [c]) x) (Leq64 x (Const64 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Less64U (Const64 <x.Type> [c-d-1]) (Sub64 <x.Type> x (Const64 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq64 (Const64 [c]) x) (Leq64 x (Const64 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Leq64U (Const64 <x.Type> [c-d-1]) (Sub64 <x.Type> x (Const64 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less32 (Const32 [c]) x) (Leq32 x (Const32 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Less32U (Const32 <x.Type> [c-d-1]) (Sub32 <x.Type> x (Const32 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq32 (Const32 [c]) x) (Leq32 x (Const32 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Leq32U (Const32 <x.Type> [c-d-1]) (Sub32 <x.Type> x (Const32 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less16 (Const16 [c]) x) (Leq16 x (Const16 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Less16U (Const16 <x.Type> [c-d-1]) (Sub16 <x.Type> x (Const16 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq16 (Const16 [c]) x) (Leq16 x (Const16 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Leq16U (Const16 <x.Type> [c-d-1]) (Sub16 <x.Type> x (Const16 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less8 (Const8 [c]) x) (Leq8 x (Const8 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Less8U (Const8 <x.Type> [c-d-1]) (Sub8 <x.Type> x (Const8 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq8 (Const8 [c]) x) (Leq8 x (Const8 [d])))
    // cond: c >= d+1 && d+1 > d
    // result: (Leq8U (Const8 <x.Type> [c-d-1]) (Sub8 <x.Type> x (Const8 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(c >= d + 1 && d + 1 > d)) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less64U (Const64 [c]) x) (Less64U x (Const64 [d])))
    // cond: uint64(c) >= uint64(d)
    // result: (Less64U (Const64 <x.Type> [c-d]) (Sub64 <x.Type> x (Const64 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(c) >= uint64(d))) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq64U (Const64 [c]) x) (Less64U x (Const64 [d])))
    // cond: uint64(c) >= uint64(d)
    // result: (Leq64U (Const64 <x.Type> [c-d]) (Sub64 <x.Type> x (Const64 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLess64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(c) >= uint64(d))) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less32U (Const32 [c]) x) (Less32U x (Const32 [d])))
    // cond: uint32(c) >= uint32(d)
    // result: (Less32U (Const32 <x.Type> [c-d]) (Sub32 <x.Type> x (Const32 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(c) >= uint32(d))) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq32U (Const32 [c]) x) (Less32U x (Const32 [d])))
    // cond: uint32(c) >= uint32(d)
    // result: (Leq32U (Const32 <x.Type> [c-d]) (Sub32 <x.Type> x (Const32 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLess32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(c) >= uint32(d))) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less16U (Const16 [c]) x) (Less16U x (Const16 [d])))
    // cond: uint16(c) >= uint16(d)
    // result: (Less16U (Const16 <x.Type> [c-d]) (Sub16 <x.Type> x (Const16 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(c) >= uint16(d))) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq16U (Const16 [c]) x) (Less16U x (Const16 [d])))
    // cond: uint16(c) >= uint16(d)
    // result: (Leq16U (Const16 <x.Type> [c-d]) (Sub16 <x.Type> x (Const16 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLess16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(c) >= uint16(d))) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less8U (Const8 [c]) x) (Less8U x (Const8 [d])))
    // cond: uint8(c) >= uint8(d)
    // result: (Less8U (Const8 <x.Type> [c-d]) (Sub8 <x.Type> x (Const8 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(c) >= uint8(d))) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq8U (Const8 [c]) x) (Less8U x (Const8 [d])))
    // cond: uint8(c) >= uint8(d)
    // result: (Leq8U (Const8 <x.Type> [c-d]) (Sub8 <x.Type> x (Const8 <x.Type> [d])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLess8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(c) >= uint8(d))) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less64U (Const64 [c]) x) (Leq64U x (Const64 [d])))
    // cond: uint64(c) >= uint64(d+1) && uint64(d+1) > uint64(d)
    // result: (Less64U (Const64 <x.Type> [c-d-1]) (Sub64 <x.Type> x (Const64 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(c) >= uint64(d + 1) && uint64(d + 1) > uint64(d))) {
                    continue;
                }

                v.reset(OpLess64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq64U (Const64 [c]) x) (Leq64U x (Const64 [d])))
    // cond: uint64(c) >= uint64(d+1) && uint64(d+1) > uint64(d)
    // result: (Leq64U (Const64 <x.Type> [c-d-1]) (Sub64 <x.Type> x (Const64 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq64U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst64) {
                    continue;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                if (v_1.Op != OpLeq64U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst64) {
                    continue;
                }

                d = auxIntToInt64(v_1_1.AuxInt);
                if (!(uint64(c) >= uint64(d + 1) && uint64(d + 1) > uint64(d))) {
                    continue;
                }

                v.reset(OpLeq64U);
                v0 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v0.AuxInt = int64ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub64, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst64, x.Type);
                v2.AuxInt = int64ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less32U (Const32 [c]) x) (Leq32U x (Const32 [d])))
    // cond: uint32(c) >= uint32(d+1) && uint32(d+1) > uint32(d)
    // result: (Less32U (Const32 <x.Type> [c-d-1]) (Sub32 <x.Type> x (Const32 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(c) >= uint32(d + 1) && uint32(d + 1) > uint32(d))) {
                    continue;
                }

                v.reset(OpLess32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq32U (Const32 [c]) x) (Leq32U x (Const32 [d])))
    // cond: uint32(c) >= uint32(d+1) && uint32(d+1) > uint32(d)
    // result: (Leq32U (Const32 <x.Type> [c-d-1]) (Sub32 <x.Type> x (Const32 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq32U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst32) {
                    continue;
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_1.Op != OpLeq32U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst32) {
                    continue;
                }

                d = auxIntToInt32(v_1_1.AuxInt);
                if (!(uint32(c) >= uint32(d + 1) && uint32(d + 1) > uint32(d))) {
                    continue;
                }

                v.reset(OpLeq32U);
                v0 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v0.AuxInt = int32ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub32, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst32, x.Type);
                v2.AuxInt = int32ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less16U (Const16 [c]) x) (Leq16U x (Const16 [d])))
    // cond: uint16(c) >= uint16(d+1) && uint16(d+1) > uint16(d)
    // result: (Less16U (Const16 <x.Type> [c-d-1]) (Sub16 <x.Type> x (Const16 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(c) >= uint16(d + 1) && uint16(d + 1) > uint16(d))) {
                    continue;
                }

                v.reset(OpLess16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq16U (Const16 [c]) x) (Leq16U x (Const16 [d])))
    // cond: uint16(c) >= uint16(d+1) && uint16(d+1) > uint16(d)
    // result: (Leq16U (Const16 <x.Type> [c-d-1]) (Sub16 <x.Type> x (Const16 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq16U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst16) {
                    continue;
                }

                c = auxIntToInt16(v_0_0.AuxInt);
                if (v_1.Op != OpLeq16U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst16) {
                    continue;
                }

                d = auxIntToInt16(v_1_1.AuxInt);
                if (!(uint16(c) >= uint16(d + 1) && uint16(d + 1) > uint16(d))) {
                    continue;
                }

                v.reset(OpLeq16U);
                v0 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v0.AuxInt = int16ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub16, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst16, x.Type);
                v2.AuxInt = int16ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Less8U (Const8 [c]) x) (Leq8U x (Const8 [d])))
    // cond: uint8(c) >= uint8(d+1) && uint8(d+1) > uint8(d)
    // result: (Less8U (Const8 <x.Type> [c-d-1]) (Sub8 <x.Type> x (Const8 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLess8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(c) >= uint8(d + 1) && uint8(d + 1) > uint8(d))) {
                    continue;
                }

                v.reset(OpLess8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OrB (Leq8U (Const8 [c]) x) (Leq8U x (Const8 [d])))
    // cond: uint8(c) >= uint8(d+1) && uint8(d+1) > uint8(d)
    // result: (Leq8U (Const8 <x.Type> [c-d-1]) (Sub8 <x.Type> x (Const8 <x.Type> [d+1])))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpLeq8U) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[1];
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpConst8) {
                    continue;
                }

                c = auxIntToInt8(v_0_0.AuxInt);
                if (v_1.Op != OpLeq8U) {
                    continue;
                }

                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }

                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpConst8) {
                    continue;
                }

                d = auxIntToInt8(v_1_1.AuxInt);
                if (!(uint8(c) >= uint8(d + 1) && uint8(d + 1) > uint8(d))) {
                    continue;
                }

                v.reset(OpLeq8U);
                v0 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v0.AuxInt = int8ToAuxInt(c - d - 1);
                v1 = b.NewValue0(v.Pos, OpSub8, x.Type);
                v2 = b.NewValue0(v.Pos, OpConst8, x.Type);
                v2.AuxInt = int8ToAuxInt(d + 1);
                v1.AddArg2(x, v2);
                v.AddArg2(v0, v1);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Phi (Const8 [c]) (Const8 [c]))
    // result: (Const8 [c])
    while (true) {
        if (len(v.Args) != 2) {
            break;
        }
        _ = v.Args[1];
        var v_0 = v.Args[0];
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        var v_1 = v.Args[1];
        if (v_1.Op != OpConst8 || auxIntToInt8(v_1.AuxInt) != c) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c);
        return true;

    } 
    // match: (Phi (Const16 [c]) (Const16 [c]))
    // result: (Const16 [c])
    while (true) {
        if (len(v.Args) != 2) {
            break;
        }
        _ = v.Args[1];
        v_0 = v.Args[0];
        if (v_0.Op != OpConst16) {
            break;
        }
        c = auxIntToInt16(v_0.AuxInt);
        v_1 = v.Args[1];
        if (v_1.Op != OpConst16 || auxIntToInt16(v_1.AuxInt) != c) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c);
        return true;

    } 
    // match: (Phi (Const32 [c]) (Const32 [c]))
    // result: (Const32 [c])
    while (true) {
        if (len(v.Args) != 2) {
            break;
        }
        _ = v.Args[1];
        v_0 = v.Args[0];
        if (v_0.Op != OpConst32) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v_1 = v.Args[1];
        if (v_1.Op != OpConst32 || auxIntToInt32(v_1.AuxInt) != c) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    } 
    // match: (Phi (Const64 [c]) (Const64 [c]))
    // result: (Const64 [c])
    while (true) {
        if (len(v.Args) != 2) {
            break;
        }
        _ = v.Args[1];
        v_0 = v.Args[0];
        if (v_0.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_1 = v.Args[1];
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != c) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpPtrIndex(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PtrIndex <t> ptr idx)
    // cond: config.PtrSize == 4 && is32Bit(t.Elem().Size())
    // result: (AddPtr ptr (Mul32 <typ.Int> idx (Const32 <typ.Int> [int32(t.Elem().Size())])))
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var idx = v_1;
        if (!(config.PtrSize == 4 && is32Bit(t.Elem().Size()))) {
            break;
        }
        v.reset(OpAddPtr);
        var v0 = b.NewValue0(v.Pos, OpMul32, typ.Int);
        var v1 = b.NewValue0(v.Pos, OpConst32, typ.Int);
        v1.AuxInt = int32ToAuxInt(int32(t.Elem().Size()));
        v0.AddArg2(idx, v1);
        v.AddArg2(ptr, v0);
        return true;

    } 
    // match: (PtrIndex <t> ptr idx)
    // cond: config.PtrSize == 8
    // result: (AddPtr ptr (Mul64 <typ.Int> idx (Const64 <typ.Int> [t.Elem().Size()])))
    while (true) {
        t = v.Type;
        ptr = v_0;
        idx = v_1;
        if (!(config.PtrSize == 8)) {
            break;
        }
        v.reset(OpAddPtr);
        v0 = b.NewValue0(v.Pos, OpMul64, typ.Int);
        v1 = b.NewValue0(v.Pos, OpConst64, typ.Int);
        v1.AuxInt = int64ToAuxInt(t.Elem().Size());
        v0.AddArg2(idx, v1);
        v.AddArg2(ptr, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft16 x (Const16 [c]))
    // cond: c%16 == 0
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        if (!(c % 16 == 0)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRotateLeft32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft32 x (Const32 [c]))
    // cond: c%32 == 0
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(c % 32 == 0)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRotateLeft64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft64 x (Const64 [c]))
    // cond: c%64 == 0
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(c % 64 == 0)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft8 x (Const8 [c]))
    // cond: c%8 == 0
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        if (!(c % 8 == 0)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRound32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Round32F x:(Const32F))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpConst32F) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRound64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Round64F x:(Const64F))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpConst64F) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux16 <t> x (Const16 [c]))
    // result: (Rsh16Ux64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux16 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux32 <t> x (Const32 [c]))
    // result: (Rsh16Ux64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux32 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 (Const16 [c]) (Const64 [d]))
    // result: (Const16 [int16(uint16(c) >> uint64(d))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(uint16(c) >> (int)(uint64(d))));
        return true;

    } 
    // match: (Rsh16Ux64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh16Ux64 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Rsh16Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (Const16 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Rsh16Ux64 <t> (Rsh16Ux64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh16Ux64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux64 (Rsh16x64 x _) (Const64 <t> [15]))
    // result: (Rsh16Ux64 x (Const64 <t> [15]))
    while (true) {
        if (v_0.Op != OpRsh16x64) {
            break;
        }
        x = v_0.Args[0];
        if (v_1.Op != OpConst64) {
            break;
        }
        t = v_1.Type;
        if (auxIntToInt64(v_1.AuxInt) != 15) {
            break;
        }
        v.reset(OpRsh16Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(15);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux64 (Lsh16x64 (Rsh16Ux64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Rsh16Ux64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpLsh16x64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh16Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpRsh16Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux64 (Lsh16x64 x (Const64 [8])) (Const64 [8]))
    // result: (ZeroExt8to16 (Trunc16to8 <typ.UInt8> x))
    while (true) {
        if (v_0.Op != OpLsh16x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 8 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 8) {
            break;
        }
        v.reset(OpZeroExt8to16);
        v0 = b.NewValue0(v.Pos, OpTrunc16to8, typ.UInt8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux8 <t> x (Const8 [c]))
    // result: (Rsh16Ux64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh16Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16Ux8 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x16 <t> x (Const16 [c]))
    // result: (Rsh16x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16x16 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x32 <t> x (Const32 [c]))
    // result: (Rsh16x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16x32 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 (Const16 [c]) (Const64 [d]))
    // result: (Const16 [c >> uint64(d)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c >> (int)(uint64(d)));
        return true;

    } 
    // match: (Rsh16x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh16x64 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Rsh16x64 <t> (Rsh16x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh16x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh16x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16x64 (Lsh16x64 x (Const64 [8])) (Const64 [8]))
    // result: (SignExt8to16 (Trunc16to8 <typ.Int8> x))
    while (true) {
        if (v_0.Op != OpLsh16x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 8 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 8) {
            break;
        }
        v.reset(OpSignExt8to16);
        v0 = b.NewValue0(v.Pos, OpTrunc16to8, typ.Int8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x8 <t> x (Const8 [c]))
    // result: (Rsh16x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh16x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh16x8 (Const16 [0]) _)
    // result: (Const16 [0])
    while (true) {
        if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux16 <t> x (Const16 [c]))
    // result: (Rsh32Ux64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux16 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux32 <t> x (Const32 [c]))
    // result: (Rsh32Ux64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux32 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux64 (Const32 [c]) (Const64 [d]))
    // result: (Const32 [int32(uint32(c) >> uint64(d))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        return true;

    } 
    // match: (Rsh32Ux64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh32Ux64 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh32Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh32Ux64 <t> (Rsh32Ux64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh32Ux64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux64 (Rsh32x64 x _) (Const64 <t> [31]))
    // result: (Rsh32Ux64 x (Const64 <t> [31]))
    while (true) {
        if (v_0.Op != OpRsh32x64) {
            break;
        }
        x = v_0.Args[0];
        if (v_1.Op != OpConst64) {
            break;
        }
        t = v_1.Type;
        if (auxIntToInt64(v_1.AuxInt) != 31) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(31);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux64 (Lsh32x64 (Rsh32Ux64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Rsh32Ux64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh32Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpRsh32Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux64 (Lsh32x64 x (Const64 [24])) (Const64 [24]))
    // result: (ZeroExt8to32 (Trunc32to8 <typ.UInt8> x))
    while (true) {
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 24 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 24) {
            break;
        }
        v.reset(OpZeroExt8to32);
        v0 = b.NewValue0(v.Pos, OpTrunc32to8, typ.UInt8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh32Ux64 (Lsh32x64 x (Const64 [16])) (Const64 [16]))
    // result: (ZeroExt16to32 (Trunc32to16 <typ.UInt16> x))
    while (true) {
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 16 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 16) {
            break;
        }
        v.reset(OpZeroExt16to32);
        v0 = b.NewValue0(v.Pos, OpTrunc32to16, typ.UInt16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux8 <t> x (Const8 [c]))
    // result: (Rsh32Ux64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh32Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32Ux8 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x16 <t> x (Const16 [c]))
    // result: (Rsh32x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32x16 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x32 <t> x (Const32 [c]))
    // result: (Rsh32x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32x32 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x64 (Const32 [c]) (Const64 [d]))
    // result: (Const32 [c >> uint64(d)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        return true;

    } 
    // match: (Rsh32x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh32x64 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Rsh32x64 <t> (Rsh32x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh32x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32x64 (Lsh32x64 x (Const64 [24])) (Const64 [24]))
    // result: (SignExt8to32 (Trunc32to8 <typ.Int8> x))
    while (true) {
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 24 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 24) {
            break;
        }
        v.reset(OpSignExt8to32);
        v0 = b.NewValue0(v.Pos, OpTrunc32to8, typ.Int8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh32x64 (Lsh32x64 x (Const64 [16])) (Const64 [16]))
    // result: (SignExt16to32 (Trunc32to16 <typ.Int16> x))
    while (true) {
        if (v_0.Op != OpLsh32x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 16 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 16) {
            break;
        }
        v.reset(OpSignExt16to32);
        v0 = b.NewValue0(v.Pos, OpTrunc32to16, typ.Int16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x8 <t> x (Const8 [c]))
    // result: (Rsh32x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh32x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh32x8 (Const32 [0]) _)
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64Ux16 <t> x (Const16 [c]))
    // result: (Rsh64Ux64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux16 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64Ux32 <t> x (Const32 [c]))
    // result: (Rsh64Ux64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux32 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [int64(uint64(c) >> uint64(d))])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint64(c) >> (int)(uint64(d))));
        return true;

    } 
    // match: (Rsh64Ux64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh64Ux64 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Rsh64Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 64
    // result: (Const64 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Rsh64Ux64 <t> (Rsh64Ux64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh64Ux64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh64Ux64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux64 (Rsh64x64 x _) (Const64 <t> [63]))
    // result: (Rsh64Ux64 x (Const64 <t> [63]))
    while (true) {
        if (v_0.Op != OpRsh64x64) {
            break;
        }
        x = v_0.Args[0];
        if (v_1.Op != OpConst64) {
            break;
        }
        t = v_1.Type;
        if (auxIntToInt64(v_1.AuxInt) != 63) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(63);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux64 (Lsh64x64 (Rsh64Ux64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Rsh64Ux64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh64Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpRsh64Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux64 (Lsh64x64 x (Const64 [56])) (Const64 [56]))
    // result: (ZeroExt8to64 (Trunc64to8 <typ.UInt8> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 56 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 56) {
            break;
        }
        v.reset(OpZeroExt8to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to8, typ.UInt8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh64Ux64 (Lsh64x64 x (Const64 [48])) (Const64 [48]))
    // result: (ZeroExt16to64 (Trunc64to16 <typ.UInt16> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 48 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 48) {
            break;
        }
        v.reset(OpZeroExt16to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to16, typ.UInt16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh64Ux64 (Lsh64x64 x (Const64 [32])) (Const64 [32]))
    // result: (ZeroExt32to64 (Trunc64to32 <typ.UInt32> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 32 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v.reset(OpZeroExt32to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64Ux8 <t> x (Const8 [c]))
    // result: (Rsh64Ux64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh64Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64Ux8 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64x16 <t> x (Const16 [c]))
    // result: (Rsh64x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64x16 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64x32 <t> x (Const32 [c]))
    // result: (Rsh64x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64x32 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c >> uint64(d)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c >> (int)(uint64(d)));
        return true;

    } 
    // match: (Rsh64x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh64x64 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Rsh64x64 <t> (Rsh64x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh64x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64x64 (Lsh64x64 x (Const64 [56])) (Const64 [56]))
    // result: (SignExt8to64 (Trunc64to8 <typ.Int8> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 56 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 56) {
            break;
        }
        v.reset(OpSignExt8to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to8, typ.Int8);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh64x64 (Lsh64x64 x (Const64 [48])) (Const64 [48]))
    // result: (SignExt16to64 (Trunc64to16 <typ.Int16> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 48 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 48) {
            break;
        }
        v.reset(OpSignExt16to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to16, typ.Int16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh64x64 (Lsh64x64 x (Const64 [32])) (Const64 [32]))
    // result: (SignExt32to64 (Trunc64to32 <typ.Int32> x))
    while (true) {
        if (v_0.Op != OpLsh64x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64 || auxIntToInt64(v_0_1.AuxInt) != 32 || v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v.reset(OpSignExt32to64);
        v0 = b.NewValue0(v.Pos, OpTrunc64to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64x8 <t> x (Const8 [c]))
    // result: (Rsh64x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh64x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh64x8 (Const64 [0]) _)
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux16 <t> x (Const16 [c]))
    // result: (Rsh8Ux64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux16 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux32 <t> x (Const32 [c]))
    // result: (Rsh8Ux64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux32 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 (Const8 [c]) (Const64 [d]))
    // result: (Const8 [int8(uint8(c) >> uint64(d))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(uint8(c) >> (int)(uint64(d))));
        return true;

    } 
    // match: (Rsh8Ux64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh8Ux64 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Rsh8Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (Const8 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Rsh8Ux64 <t> (Rsh8Ux64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh8Ux64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux64 (Rsh8x64 x _) (Const64 <t> [7] ))
    // result: (Rsh8Ux64 x (Const64 <t> [7] ))
    while (true) {
        if (v_0.Op != OpRsh8x64) {
            break;
        }
        x = v_0.Args[0];
        if (v_1.Op != OpConst64) {
            break;
        }
        t = v_1.Type;
        if (auxIntToInt64(v_1.AuxInt) != 7) {
            break;
        }
        v.reset(OpRsh8Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(7);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux64 (Lsh8x64 (Rsh8Ux64 x (Const64 [c1])) (Const64 [c2])) (Const64 [c3]))
    // cond: uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1-c2, c3)
    // result: (Rsh8Ux64 x (Const64 <typ.UInt64> [c1-c2+c3]))
    while (true) {
        if (v_0.Op != OpLsh8x64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpRsh8Ux64) {
            break;
        }
        _ = v_0_0.Args[1];
        x = v_0_0.Args[0];
        var v_0_0_1 = v_0_0.Args[1];
        if (v_0_0_1.Op != OpConst64) {
            break;
        }
        var c1 = auxIntToInt64(v_0_0_1.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var c2 = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var c3 = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c1) >= uint64(c2) && uint64(c3) >= uint64(c2) && !uaddOvf(c1 - c2, c3))) {
            break;
        }
        v.reset(OpRsh8Ux64);
        v0 = b.NewValue0(v.Pos, OpConst64, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c1 - c2 + c3);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux8 <t> x (Const8 [c]))
    // result: (Rsh8Ux64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh8Ux64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8Ux8 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x16 <t> x (Const16 [c]))
    // result: (Rsh8x64 x (Const64 <t> [int64(uint16(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_1.AuxInt);
        v.reset(OpRsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint16(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8x16 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x32 <t> x (Const32 [c]))
    // result: (Rsh8x64 x (Const64 <t> [int64(uint32(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpRsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint32(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8x32 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x64 (Const8 [c]) (Const64 [d]))
    // result: (Const8 [c >> uint64(d)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c >> (int)(uint64(d)));
        return true;

    } 
    // match: (Rsh8x64 x (Const64 [0]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64 || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Rsh8x64 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Rsh8x64 <t> (Rsh8x64 x (Const64 [c])) (Const64 [d]))
    // cond: !uaddOvf(c,d)
    // result: (Rsh8x64 x (Const64 <t> [c+d]))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpRsh8x64) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_0_1.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        d = auxIntToInt64(v_1.AuxInt);
        if (!(!uaddOvf(c, d))) {
            break;
        }
        v.reset(OpRsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c + d);
        v.AddArg2(x, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x8 <t> x (Const8 [c]))
    // result: (Rsh8x64 x (Const64 <t> [int64(uint8(c))]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_1.AuxInt);
        v.reset(OpRsh8x64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(int64(uint8(c)));
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Rsh8x8 (Const8 [0]) _)
    // result: (Const8 [0])
    while (true) {
        if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSelect0(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Select0 (Div128u (Const64 [0]) lo y))
    // result: (Div64u lo y)
    while (true) {
        if (v_0.Op != OpDiv128u) {
            break;
        }
        var y = v_0.Args[2];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpConst64 || auxIntToInt64(v_0_0.AuxInt) != 0) {
            break;
        }
        var lo = v_0.Args[1];
        v.reset(OpDiv64u);
        v.AddArg2(lo, y);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSelect1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Select1 (Div128u (Const64 [0]) lo y))
    // result: (Mod64u lo y)
    while (true) {
        if (v_0.Op != OpDiv128u) {
            break;
        }
        var y = v_0.Args[2];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpConst64 || auxIntToInt64(v_0_0.AuxInt) != 0) {
            break;
        }
        var lo = v_0.Args[1];
        v.reset(OpMod64u);
        v.AddArg2(lo, y);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSelectN(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (SelectN [0] (MakeResult x ___))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpMakeResult || len(v_0.Args) < 1) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (SelectN [1] (MakeResult x y ___))
    // result: y
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1 || v_0.Op != OpMakeResult || len(v_0.Args) < 2) {
            break;
        }
        var y = v_0.Args[1];
        v.copyOf(y);
        return true;

    } 
    // match: (SelectN [2] (MakeResult x y z ___))
    // result: z
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2 || v_0.Op != OpMakeResult || len(v_0.Args) < 3) {
            break;
        }
        var z = v_0.Args[2];
        v.copyOf(z);
        return true;

    } 
    // match: (SelectN [0] call:(StaticCall {sym} s1:(Store _ (Const64 [sz]) s2:(Store _ src s3:(Store {t} _ dst mem)))))
    // cond: sz >= 0 && isSameCall(sym, "runtime.memmove") && t.IsPtr() && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && isInlinableMemmove(dst, src, int64(sz), config) && clobber(s1, s2, s3, call)
    // result: (Move {t.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var call = v_0;
        if (call.Op != OpStaticCall || len(call.Args) != 1) {
            break;
        }
        var sym = auxToCall(call.Aux);
        var s1 = call.Args[0];
        if (s1.Op != OpStore) {
            break;
        }
        _ = s1.Args[2];
        var s1_1 = s1.Args[1];
        if (s1_1.Op != OpConst64) {
            break;
        }
        var sz = auxIntToInt64(s1_1.AuxInt);
        var s2 = s1.Args[2];
        if (s2.Op != OpStore) {
            break;
        }
        _ = s2.Args[2];
        var src = s2.Args[1];
        var s3 = s2.Args[2];
        if (s3.Op != OpStore) {
            break;
        }
        var t = auxToType(s3.Aux);
        var mem = s3.Args[2];
        var dst = s3.Args[1];
        if (!(sz >= 0 && isSameCall(sym, "runtime.memmove") && t.IsPtr() && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && isInlinableMemmove(dst, src, int64(sz), config) && clobber(s1, s2, s3, call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(t.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticCall {sym} s1:(Store _ (Const32 [sz]) s2:(Store _ src s3:(Store {t} _ dst mem)))))
    // cond: sz >= 0 && isSameCall(sym, "runtime.memmove") && t.IsPtr() && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && isInlinableMemmove(dst, src, int64(sz), config) && clobber(s1, s2, s3, call)
    // result: (Move {t.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticCall || len(call.Args) != 1) {
            break;
        }
        sym = auxToCall(call.Aux);
        s1 = call.Args[0];
        if (s1.Op != OpStore) {
            break;
        }
        _ = s1.Args[2];
        s1_1 = s1.Args[1];
        if (s1_1.Op != OpConst32) {
            break;
        }
        sz = auxIntToInt32(s1_1.AuxInt);
        s2 = s1.Args[2];
        if (s2.Op != OpStore) {
            break;
        }
        _ = s2.Args[2];
        src = s2.Args[1];
        s3 = s2.Args[2];
        if (s3.Op != OpStore) {
            break;
        }
        t = auxToType(s3.Aux);
        mem = s3.Args[2];
        dst = s3.Args[1];
        if (!(sz >= 0 && isSameCall(sym, "runtime.memmove") && t.IsPtr() && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && isInlinableMemmove(dst, src, int64(sz), config) && clobber(s1, s2, s3, call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(t.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticCall {sym} dst src (Const64 [sz]) mem))
    // cond: sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call)
    // result: (Move {dst.Type.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticCall || len(call.Args) != 4) {
            break;
        }
        sym = auxToCall(call.Aux);
        mem = call.Args[3];
        dst = call.Args[0];
        src = call.Args[1];
        var call_2 = call.Args[2];
        if (call_2.Op != OpConst64) {
            break;
        }
        sz = auxIntToInt64(call_2.AuxInt);
        if (!(sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(dst.Type.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticCall {sym} dst src (Const32 [sz]) mem))
    // cond: sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call)
    // result: (Move {dst.Type.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticCall || len(call.Args) != 4) {
            break;
        }
        sym = auxToCall(call.Aux);
        mem = call.Args[3];
        dst = call.Args[0];
        src = call.Args[1];
        call_2 = call.Args[2];
        if (call_2.Op != OpConst32) {
            break;
        }
        sz = auxIntToInt32(call_2.AuxInt);
        if (!(sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(dst.Type.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticLECall {sym} dst src (Const64 [sz]) mem))
    // cond: sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call)
    // result: (Move {dst.Type.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticLECall || len(call.Args) != 4) {
            break;
        }
        sym = auxToCall(call.Aux);
        mem = call.Args[3];
        dst = call.Args[0];
        src = call.Args[1];
        call_2 = call.Args[2];
        if (call_2.Op != OpConst64) {
            break;
        }
        sz = auxIntToInt64(call_2.AuxInt);
        if (!(sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(dst.Type.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticLECall {sym} dst src (Const32 [sz]) mem))
    // cond: sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call)
    // result: (Move {dst.Type.Elem()} [int64(sz)] dst src mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticLECall || len(call.Args) != 4) {
            break;
        }
        sym = auxToCall(call.Aux);
        mem = call.Args[3];
        dst = call.Args[0];
        src = call.Args[1];
        call_2 = call.Args[2];
        if (call_2.Op != OpConst32) {
            break;
        }
        sz = auxIntToInt32(call_2.AuxInt);
        if (!(sz >= 0 && call.Uses == 1 && isSameCall(sym, "runtime.memmove") && dst.Type.IsPtr() && isInlinableMemmove(dst, src, int64(sz), config) && clobber(call))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(int64(sz));
        v.Aux = typeToAux(dst.Type.Elem());
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (SelectN [0] call:(StaticLECall {sym} a x))
    // cond: needRaceCleanup(sym, call) && clobber(call)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticLECall || len(call.Args) != 2) {
            break;
        }
        sym = auxToCall(call.Aux);
        x = call.Args[1];
        if (!(needRaceCleanup(sym, call) && clobber(call))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (SelectN [0] call:(StaticLECall {sym} x))
    // cond: needRaceCleanup(sym, call) && clobber(call)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        call = v_0;
        if (call.Op != OpStaticLECall || len(call.Args) != 1) {
            break;
        }
        sym = auxToCall(call.Aux);
        x = call.Args[0];
        if (!(needRaceCleanup(sym, call) && clobber(call))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt16to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt16to32 (Const16 [c]))
    // result: (Const32 [int32(c)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;

    } 
    // match: (SignExt16to32 (Trunc32to16 x:(Rsh32x64 _ (Const64 [s]))))
    // cond: s >= 16
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc32to16) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh32x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 16)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt16to64 (Const16 [c]))
    // result: (Const64 [int64(c)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;

    } 
    // match: (SignExt16to64 (Trunc64to16 x:(Rsh64x64 _ (Const64 [s]))))
    // cond: s >= 48
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to16) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 48)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt32to64 (Const32 [c]))
    // result: (Const64 [int64(c)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;

    } 
    // match: (SignExt32to64 (Trunc64to32 x:(Rsh64x64 _ (Const64 [s]))))
    // cond: s >= 32
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to32) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 32)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt8to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt8to16 (Const8 [c]))
    // result: (Const16 [int16(c)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(c));
        return true;

    } 
    // match: (SignExt8to16 (Trunc16to8 x:(Rsh16x64 _ (Const64 [s]))))
    // cond: s >= 8
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc16to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh16x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 8)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt8to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt8to32 (Const8 [c]))
    // result: (Const32 [int32(c)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;

    } 
    // match: (SignExt8to32 (Trunc32to8 x:(Rsh32x64 _ (Const64 [s]))))
    // cond: s >= 24
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc32to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh32x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 24)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSignExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SignExt8to64 (Const8 [c]))
    // result: (Const64 [int64(c)])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(c));
        return true;

    } 
    // match: (SignExt8to64 (Trunc64to8 x:(Rsh64x64 _ (Const64 [s]))))
    // cond: s >= 56
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64x64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 56)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSliceCap(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SliceCap (SliceMake _ _ (Const64 <t> [c])))
    // result: (Const64 <t> [c])
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[2];
        var v_0_2 = v_0.Args[2];
        if (v_0_2.Op != OpConst64) {
            break;
        }
        var t = v_0_2.Type;
        var c = auxIntToInt64(v_0_2.AuxInt);
        v.reset(OpConst64);
        v.Type = t;
        v.AuxInt = int64ToAuxInt(c);
        return true;

    } 
    // match: (SliceCap (SliceMake _ _ (Const32 <t> [c])))
    // result: (Const32 <t> [c])
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[2];
        v_0_2 = v_0.Args[2];
        if (v_0_2.Op != OpConst32) {
            break;
        }
        t = v_0_2.Type;
        c = auxIntToInt32(v_0_2.AuxInt);
        v.reset(OpConst32);
        v.Type = t;
        v.AuxInt = int32ToAuxInt(c);
        return true;

    } 
    // match: (SliceCap (SliceMake _ _ (SliceCap x)))
    // result: (SliceCap x)
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[2];
        v_0_2 = v_0.Args[2];
        if (v_0_2.Op != OpSliceCap) {
            break;
        }
        var x = v_0_2.Args[0];
        v.reset(OpSliceCap);
        v.AddArg(x);
        return true;

    } 
    // match: (SliceCap (SliceMake _ _ (SliceLen x)))
    // result: (SliceLen x)
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[2];
        v_0_2 = v_0.Args[2];
        if (v_0_2.Op != OpSliceLen) {
            break;
        }
        x = v_0_2.Args[0];
        v.reset(OpSliceLen);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSliceLen(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SliceLen (SliceMake _ (Const64 <t> [c]) _))
    // result: (Const64 <t> [c])
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var t = v_0_1.Type;
        var c = auxIntToInt64(v_0_1.AuxInt);
        v.reset(OpConst64);
        v.Type = t;
        v.AuxInt = int64ToAuxInt(c);
        return true;

    } 
    // match: (SliceLen (SliceMake _ (Const32 <t> [c]) _))
    // result: (Const32 <t> [c])
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst32) {
            break;
        }
        t = v_0_1.Type;
        c = auxIntToInt32(v_0_1.AuxInt);
        v.reset(OpConst32);
        v.Type = t;
        v.AuxInt = int32ToAuxInt(c);
        return true;

    } 
    // match: (SliceLen (SliceMake _ (SliceLen x) _))
    // result: (SliceLen x)
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        _ = v_0.Args[1];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpSliceLen) {
            break;
        }
        var x = v_0_1.Args[0];
        v.reset(OpSliceLen);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSlicePtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SlicePtr (SliceMake (SlicePtr x) _ _))
    // result: (SlicePtr x)
    while (true) {
        if (v_0.Op != OpSliceMake) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSlicePtr) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpSlicePtr);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Slicemask (Const32 [x]))
    // cond: x > 0
    // result: (Const32 [-1])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        if (!(x > 0)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(-1);
        return true;

    } 
    // match: (Slicemask (Const32 [0]))
    // result: (Const32 [0])
    while (true) {
        if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Slicemask (Const64 [x]))
    // cond: x > 0
    // result: (Const64 [-1])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(x > 0)) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(-1);
        return true;

    } 
    // match: (Slicemask (Const64 [0]))
    // result: (Const64 [0])
    while (true) {
        if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSqrt(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Sqrt (Const64F [c]))
    // cond: !math.IsNaN(math.Sqrt(c))
    // result: (Const64F [math.Sqrt(c)])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (!(!math.IsNaN(math.Sqrt(c)))) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(math.Sqrt(c));
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpStaticLECall(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (StaticLECall {callAux} sptr (Addr {scon} (SB)) (Const64 [1]) mem)
    // cond: isSameCall(callAux, "runtime.memequal") && symIsRO(scon)
    // result: (MakeResult (Eq8 (Load <typ.Int8> sptr mem) (Const8 <typ.Int8> [int8(read8(scon,0))])) mem)
    while (true) {
        if (len(v.Args) != 4) {
            break;
        }
        var callAux = auxToCall(v.Aux);
        var mem = v.Args[3];
        var sptr = v.Args[0];
        var v_1 = v.Args[1];
        if (v_1.Op != OpAddr) {
            break;
        }
        var scon = auxToSym(v_1.Aux);
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpSB) {
            break;
        }
        var v_2 = v.Args[2];
        if (v_2.Op != OpConst64 || auxIntToInt64(v_2.AuxInt) != 1 || !(isSameCall(callAux, "runtime.memequal") && symIsRO(scon))) {
            break;
        }
        v.reset(OpMakeResult);
        var v0 = b.NewValue0(v.Pos, OpEq8, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpLoad, typ.Int8);
        v1.AddArg2(sptr, mem);
        var v2 = b.NewValue0(v.Pos, OpConst8, typ.Int8);
        v2.AuxInt = int8ToAuxInt(int8(read8(scon, 0)));
        v0.AddArg2(v1, v2);
        v.AddArg2(v0, mem);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var fe = b.Func.fe; 
    // match: (Store {t1} p1 (Load <t2> p2 mem) mem)
    // cond: isSamePtr(p1, p2) && t2.Size() == t1.Size()
    // result: mem
    while (true) {
        var t1 = auxToType(v.Aux);
        var p1 = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        var t2 = v_1.Type;
        var mem = v_1.Args[1];
        var p2 = v_1.Args[0];
        if (mem != v_2 || !(isSamePtr(p1, p2) && t2.Size() == t1.Size())) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} p1 (Load <t2> p2 oldmem) mem:(Store {t3} p3 _ oldmem))
    // cond: isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        p1 = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        t2 = v_1.Type;
        var oldmem = v_1.Args[1];
        p2 = v_1.Args[0];
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        var t3 = auxToType(mem.Aux);
        _ = mem.Args[2];
        var p3 = mem.Args[0];
        if (oldmem != mem.Args[2] || !(isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} p1 (Load <t2> p2 oldmem) mem:(Store {t3} p3 _ (Store {t4} p4 _ oldmem)))
    // cond: isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size()) && disjoint(p1, t1.Size(), p4, t4.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        p1 = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        t2 = v_1.Type;
        oldmem = v_1.Args[1];
        p2 = v_1.Args[0];
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem.Aux);
        _ = mem.Args[2];
        p3 = mem.Args[0];
        var mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        var t4 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        var p4 = mem_2.Args[0];
        if (oldmem != mem_2.Args[2] || !(isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size()) && disjoint(p1, t1.Size(), p4, t4.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} p1 (Load <t2> p2 oldmem) mem:(Store {t3} p3 _ (Store {t4} p4 _ (Store {t5} p5 _ oldmem))))
    // cond: isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size()) && disjoint(p1, t1.Size(), p4, t4.Size()) && disjoint(p1, t1.Size(), p5, t5.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        p1 = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        t2 = v_1.Type;
        oldmem = v_1.Args[1];
        p2 = v_1.Args[0];
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem.Aux);
        _ = mem.Args[2];
        p3 = mem.Args[0];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        p4 = mem_2.Args[0];
        var mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        var t5 = auxToType(mem_2_2.Aux);
        _ = mem_2_2.Args[2];
        var p5 = mem_2_2.Args[0];
        if (oldmem != mem_2_2.Args[2] || !(isSamePtr(p1, p2) && t2.Size() == t1.Size() && disjoint(p1, t1.Size(), p3, t3.Size()) && disjoint(p1, t1.Size(), p4, t4.Size()) && disjoint(p1, t1.Size(), p5, t5.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t} (OffPtr [o] p1) x mem:(Zero [n] p2 _))
    // cond: isConstZero(x) && o >= 0 && t.Size() + o <= n && isSamePtr(p1, p2)
    // result: mem
    while (true) {
        var t = auxToType(v.Aux);
        if (v_0.Op != OpOffPtr) {
            break;
        }
        var o = auxIntToInt64(v_0.AuxInt);
        p1 = v_0.Args[0];
        var x = v_1;
        mem = v_2;
        if (mem.Op != OpZero) {
            break;
        }
        var n = auxIntToInt64(mem.AuxInt);
        p2 = mem.Args[0];
        if (!(isConstZero(x) && o >= 0 && t.Size() + o <= n && isSamePtr(p1, p2))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} op:(OffPtr [o1] p1) x mem:(Store {t2} p2 _ (Zero [n] p3 _)))
    // cond: isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p3) && disjoint(op, t1.Size(), p2, t2.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        var op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        var o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        x = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        p2 = mem.Args[0];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem_2.AuxInt);
        p3 = mem_2.Args[0];
        if (!(isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p3) && disjoint(op, t1.Size(), p2, t2.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} op:(OffPtr [o1] p1) x mem:(Store {t2} p2 _ (Store {t3} p3 _ (Zero [n] p4 _))))
    // cond: isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p4) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        x = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        p2 = mem.Args[0];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        p3 = mem_2.Args[0];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem_2_2.AuxInt);
        p4 = mem_2_2.Args[0];
        if (!(isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p4) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} op:(OffPtr [o1] p1) x mem:(Store {t2} p2 _ (Store {t3} p3 _ (Store {t4} p4 _ (Zero [n] p5 _)))))
    // cond: isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p5) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size())
    // result: mem
    while (true) {
        t1 = auxToType(v.Aux);
        op = v_0;
        if (op.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op.AuxInt);
        p1 = op.Args[0];
        x = v_1;
        mem = v_2;
        if (mem.Op != OpStore) {
            break;
        }
        t2 = auxToType(mem.Aux);
        _ = mem.Args[2];
        p2 = mem.Args[0];
        mem_2 = mem.Args[2];
        if (mem_2.Op != OpStore) {
            break;
        }
        t3 = auxToType(mem_2.Aux);
        _ = mem_2.Args[2];
        p3 = mem_2.Args[0];
        mem_2_2 = mem_2.Args[2];
        if (mem_2_2.Op != OpStore) {
            break;
        }
        t4 = auxToType(mem_2_2.Aux);
        _ = mem_2_2.Args[2];
        p4 = mem_2_2.Args[0];
        var mem_2_2_2 = mem_2_2.Args[2];
        if (mem_2_2_2.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(mem_2_2_2.AuxInt);
        p5 = mem_2_2_2.Args[0];
        if (!(isConstZero(x) && o1 >= 0 && t1.Size() + o1 <= n && isSamePtr(p1, p5) && disjoint(op, t1.Size(), p2, t2.Size()) && disjoint(op, t1.Size(), p3, t3.Size()) && disjoint(op, t1.Size(), p4, t4.Size()))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store _ (StructMake0) mem)
    // result: mem
    while (true) {
        if (v_1.Op != OpStructMake0) {
            break;
        }
        mem = v_2;
        v.copyOf(mem);
        return true;

    } 
    // match: (Store dst (StructMake1 <t> f0) mem)
    // result: (Store {t.FieldType(0)} (OffPtr <t.FieldType(0).PtrTo()> [0] dst) f0 mem)
    while (true) {
        var dst = v_0;
        if (v_1.Op != OpStructMake1) {
            break;
        }
        t = v_1.Type;
        var f0 = v_1.Args[0];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(t.FieldType(0));
        var v0 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v0.AuxInt = int64ToAuxInt(0);
        v0.AddArg(dst);
        v.AddArg3(v0, f0, mem);
        return true;

    } 
    // match: (Store dst (StructMake2 <t> f0 f1) mem)
    // result: (Store {t.FieldType(1)} (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] dst) f1 (Store {t.FieldType(0)} (OffPtr <t.FieldType(0).PtrTo()> [0] dst) f0 mem))
    while (true) {
        dst = v_0;
        if (v_1.Op != OpStructMake2) {
            break;
        }
        t = v_1.Type;
        var f1 = v_1.Args[1];
        f0 = v_1.Args[0];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(t.FieldType(1));
        v0 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v0.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v0.AddArg(dst);
        var v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t.FieldType(0));
        var v2 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v2.AuxInt = int64ToAuxInt(0);
        v2.AddArg(dst);
        v1.AddArg3(v2, f0, mem);
        v.AddArg3(v0, f1, v1);
        return true;

    } 
    // match: (Store dst (StructMake3 <t> f0 f1 f2) mem)
    // result: (Store {t.FieldType(2)} (OffPtr <t.FieldType(2).PtrTo()> [t.FieldOff(2)] dst) f2 (Store {t.FieldType(1)} (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] dst) f1 (Store {t.FieldType(0)} (OffPtr <t.FieldType(0).PtrTo()> [0] dst) f0 mem)))
    while (true) {
        dst = v_0;
        if (v_1.Op != OpStructMake3) {
            break;
        }
        t = v_1.Type;
        var f2 = v_1.Args[2];
        f0 = v_1.Args[0];
        f1 = v_1.Args[1];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(t.FieldType(2));
        v0 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(2).PtrTo());
        v0.AuxInt = int64ToAuxInt(t.FieldOff(2));
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t.FieldType(1));
        v2 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v2.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v2.AddArg(dst);
        var v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t.FieldType(0));
        var v4 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v4.AuxInt = int64ToAuxInt(0);
        v4.AddArg(dst);
        v3.AddArg3(v4, f0, mem);
        v1.AddArg3(v2, f1, v3);
        v.AddArg3(v0, f2, v1);
        return true;

    } 
    // match: (Store dst (StructMake4 <t> f0 f1 f2 f3) mem)
    // result: (Store {t.FieldType(3)} (OffPtr <t.FieldType(3).PtrTo()> [t.FieldOff(3)] dst) f3 (Store {t.FieldType(2)} (OffPtr <t.FieldType(2).PtrTo()> [t.FieldOff(2)] dst) f2 (Store {t.FieldType(1)} (OffPtr <t.FieldType(1).PtrTo()> [t.FieldOff(1)] dst) f1 (Store {t.FieldType(0)} (OffPtr <t.FieldType(0).PtrTo()> [0] dst) f0 mem))))
    while (true) {
        dst = v_0;
        if (v_1.Op != OpStructMake4) {
            break;
        }
        t = v_1.Type;
        var f3 = v_1.Args[3];
        f0 = v_1.Args[0];
        f1 = v_1.Args[1];
        f2 = v_1.Args[2];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(t.FieldType(3));
        v0 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(3).PtrTo());
        v0.AuxInt = int64ToAuxInt(t.FieldOff(3));
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t.FieldType(2));
        v2 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(2).PtrTo());
        v2.AuxInt = int64ToAuxInt(t.FieldOff(2));
        v2.AddArg(dst);
        v3 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v3.Aux = typeToAux(t.FieldType(1));
        v4 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(1).PtrTo());
        v4.AuxInt = int64ToAuxInt(t.FieldOff(1));
        v4.AddArg(dst);
        var v5 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v5.Aux = typeToAux(t.FieldType(0));
        var v6 = b.NewValue0(v.Pos, OpOffPtr, t.FieldType(0).PtrTo());
        v6.AuxInt = int64ToAuxInt(0);
        v6.AddArg(dst);
        v5.AddArg3(v6, f0, mem);
        v3.AddArg3(v4, f1, v5);
        v1.AddArg3(v2, f2, v3);
        v.AddArg3(v0, f3, v1);
        return true;

    } 
    // match: (Store {t} dst (Load src mem) mem)
    // cond: !fe.CanSSA(t)
    // result: (Move {t} [t.Size()] dst src mem)
    while (true) {
        t = auxToType(v.Aux);
        dst = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        mem = v_1.Args[1];
        var src = v_1.Args[0];
        if (mem != v_2 || !(!fe.CanSSA(t))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(t.Size());
        v.Aux = typeToAux(t);
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (Store {t} dst (Load src mem) (VarDef {x} mem))
    // cond: !fe.CanSSA(t)
    // result: (Move {t} [t.Size()] dst src (VarDef {x} mem))
    while (true) {
        t = auxToType(v.Aux);
        dst = v_0;
        if (v_1.Op != OpLoad) {
            break;
        }
        mem = v_1.Args[1];
        src = v_1.Args[0];
        if (v_2.Op != OpVarDef) {
            break;
        }
        x = auxToSym(v_2.Aux);
        if (mem != v_2.Args[0] || !(!fe.CanSSA(t))) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(t.Size());
        v.Aux = typeToAux(t);
        v0 = b.NewValue0(v.Pos, OpVarDef, types.TypeMem);
        v0.Aux = symToAux(x);
        v0.AddArg(mem);
        v.AddArg3(dst, src, v0);
        return true;

    } 
    // match: (Store _ (ArrayMake0) mem)
    // result: mem
    while (true) {
        if (v_1.Op != OpArrayMake0) {
            break;
        }
        mem = v_2;
        v.copyOf(mem);
        return true;

    } 
    // match: (Store dst (ArrayMake1 e) mem)
    // result: (Store {e.Type} dst e mem)
    while (true) {
        dst = v_0;
        if (v_1.Op != OpArrayMake1) {
            break;
        }
        var e = v_1.Args[0];
        mem = v_2;
        v.reset(OpStore);
        v.Aux = typeToAux(e.Type);
        v.AddArg3(dst, e, mem);
        return true;

    } 
    // match: (Store (SelectN [0] call:(StaticLECall _ _)) x mem:(SelectN [1] call))
    // cond: isConstZero(x) && isSameCall(call.Aux, "runtime.newobject")
    // result: mem
    while (true) {
        if (v_0.Op != OpSelectN || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        var call = v_0.Args[0];
        if (call.Op != OpStaticLECall || len(call.Args) != 2) {
            break;
        }
        x = v_1;
        mem = v_2;
        if (mem.Op != OpSelectN || auxIntToInt64(mem.AuxInt) != 1 || call != mem.Args[0] || !(isConstZero(x) && isSameCall(call.Aux, "runtime.newobject"))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store (OffPtr (SelectN [0] call:(StaticLECall _ _))) x mem:(SelectN [1] call))
    // cond: isConstZero(x) && isSameCall(call.Aux, "runtime.newobject")
    // result: mem
    while (true) {
        if (v_0.Op != OpOffPtr) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpSelectN || auxIntToInt64(v_0_0.AuxInt) != 0) {
            break;
        }
        call = v_0_0.Args[0];
        if (call.Op != OpStaticLECall || len(call.Args) != 2) {
            break;
        }
        x = v_1;
        mem = v_2;
        if (mem.Op != OpSelectN || auxIntToInt64(mem.AuxInt) != 1 || call != mem.Args[0] || !(isConstZero(x) && isSameCall(call.Aux, "runtime.newobject"))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [0] p2) d2 m3:(Move [n] p3 _ mem)))
    // cond: m2.Uses == 1 && m3.Uses == 1 && o1 == t2.Size() && n == t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && clobber(m2, m3)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 mem))
    while (true) {
        t1 = auxToType(v.Aux);
        var op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        var d1 = v_1;
        var m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        var op2 = m2.Args[0];
        if (op2.Op != OpOffPtr || auxIntToInt64(op2.AuxInt) != 0) {
            break;
        }
        p2 = op2.Args[0];
        var d2 = m2.Args[1];
        var m3 = m2.Args[2];
        if (m3.Op != OpMove) {
            break;
        }
        n = auxIntToInt64(m3.AuxInt);
        mem = m3.Args[2];
        p3 = m3.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && o1 == t2.Size() && n == t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && clobber(m2, m3))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v0.AddArg3(op2, d2, mem);
        v.AddArg3(op1, d1, v0);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [o2] p2) d2 m3:(Store {t3} op3:(OffPtr [0] p3) d3 m4:(Move [n] p4 _ mem))))
    // cond: m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && o2 == t3.Size() && o1-o2 == t2.Size() && n == t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && clobber(m2, m3, m4)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 (Store {t3} op3 d3 mem)))
    while (true) {
        t1 = auxToType(v.Aux);
        op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        d1 = v_1;
        m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        op2 = m2.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        var o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d2 = m2.Args[1];
        m3 = m2.Args[2];
        if (m3.Op != OpStore) {
            break;
        }
        t3 = auxToType(m3.Aux);
        _ = m3.Args[2];
        var op3 = m3.Args[0];
        if (op3.Op != OpOffPtr || auxIntToInt64(op3.AuxInt) != 0) {
            break;
        }
        p3 = op3.Args[0];
        var d3 = m3.Args[1];
        var m4 = m3.Args[2];
        if (m4.Op != OpMove) {
            break;
        }
        n = auxIntToInt64(m4.AuxInt);
        mem = m4.Args[2];
        p4 = m4.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && o2 == t3.Size() && o1 - o2 == t2.Size() && n == t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && clobber(m2, m3, m4))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v1.AddArg3(op3, d3, mem);
        v0.AddArg3(op2, d2, v1);
        v.AddArg3(op1, d1, v0);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [o2] p2) d2 m3:(Store {t3} op3:(OffPtr [o3] p3) d3 m4:(Store {t4} op4:(OffPtr [0] p4) d4 m5:(Move [n] p5 _ mem)))))
    // cond: m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && m5.Uses == 1 && o3 == t4.Size() && o2-o3 == t3.Size() && o1-o2 == t2.Size() && n == t4.Size() + t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && clobber(m2, m3, m4, m5)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 (Store {t3} op3 d3 (Store {t4} op4 d4 mem))))
    while (true) {
        t1 = auxToType(v.Aux);
        op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        d1 = v_1;
        m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        op2 = m2.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d2 = m2.Args[1];
        m3 = m2.Args[2];
        if (m3.Op != OpStore) {
            break;
        }
        t3 = auxToType(m3.Aux);
        _ = m3.Args[2];
        op3 = m3.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        var o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d3 = m3.Args[1];
        m4 = m3.Args[2];
        if (m4.Op != OpStore) {
            break;
        }
        t4 = auxToType(m4.Aux);
        _ = m4.Args[2];
        var op4 = m4.Args[0];
        if (op4.Op != OpOffPtr || auxIntToInt64(op4.AuxInt) != 0) {
            break;
        }
        p4 = op4.Args[0];
        var d4 = m4.Args[1];
        var m5 = m4.Args[2];
        if (m5.Op != OpMove) {
            break;
        }
        n = auxIntToInt64(m5.AuxInt);
        mem = m5.Args[2];
        p5 = m5.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && m5.Uses == 1 && o3 == t4.Size() && o2 - o3 == t3.Size() && o1 - o2 == t2.Size() && n == t4.Size() + t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && clobber(m2, m3, m4, m5))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v2.Aux = typeToAux(t4);
        v2.AddArg3(op4, d4, mem);
        v1.AddArg3(op3, d3, v2);
        v0.AddArg3(op2, d2, v1);
        v.AddArg3(op1, d1, v0);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [0] p2) d2 m3:(Zero [n] p3 mem)))
    // cond: m2.Uses == 1 && m3.Uses == 1 && o1 == t2.Size() && n == t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && clobber(m2, m3)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 mem))
    while (true) {
        t1 = auxToType(v.Aux);
        op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        d1 = v_1;
        m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        op2 = m2.Args[0];
        if (op2.Op != OpOffPtr || auxIntToInt64(op2.AuxInt) != 0) {
            break;
        }
        p2 = op2.Args[0];
        d2 = m2.Args[1];
        m3 = m2.Args[2];
        if (m3.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(m3.AuxInt);
        mem = m3.Args[1];
        p3 = m3.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && o1 == t2.Size() && n == t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && clobber(m2, m3))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v0.AddArg3(op2, d2, mem);
        v.AddArg3(op1, d1, v0);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [o2] p2) d2 m3:(Store {t3} op3:(OffPtr [0] p3) d3 m4:(Zero [n] p4 mem))))
    // cond: m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && o2 == t3.Size() && o1-o2 == t2.Size() && n == t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && clobber(m2, m3, m4)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 (Store {t3} op3 d3 mem)))
    while (true) {
        t1 = auxToType(v.Aux);
        op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        d1 = v_1;
        m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        op2 = m2.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d2 = m2.Args[1];
        m3 = m2.Args[2];
        if (m3.Op != OpStore) {
            break;
        }
        t3 = auxToType(m3.Aux);
        _ = m3.Args[2];
        op3 = m3.Args[0];
        if (op3.Op != OpOffPtr || auxIntToInt64(op3.AuxInt) != 0) {
            break;
        }
        p3 = op3.Args[0];
        d3 = m3.Args[1];
        m4 = m3.Args[2];
        if (m4.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(m4.AuxInt);
        mem = m4.Args[1];
        p4 = m4.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && o2 == t3.Size() && o1 - o2 == t2.Size() && n == t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && clobber(m2, m3, m4))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v1.AddArg3(op3, d3, mem);
        v0.AddArg3(op2, d2, v1);
        v.AddArg3(op1, d1, v0);
        return true;

    } 
    // match: (Store {t1} op1:(OffPtr [o1] p1) d1 m2:(Store {t2} op2:(OffPtr [o2] p2) d2 m3:(Store {t3} op3:(OffPtr [o3] p3) d3 m4:(Store {t4} op4:(OffPtr [0] p4) d4 m5:(Zero [n] p5 mem)))))
    // cond: m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && m5.Uses == 1 && o3 == t4.Size() && o2-o3 == t3.Size() && o1-o2 == t2.Size() && n == t4.Size() + t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && clobber(m2, m3, m4, m5)
    // result: (Store {t1} op1 d1 (Store {t2} op2 d2 (Store {t3} op3 d3 (Store {t4} op4 d4 mem))))
    while (true) {
        t1 = auxToType(v.Aux);
        op1 = v_0;
        if (op1.Op != OpOffPtr) {
            break;
        }
        o1 = auxIntToInt64(op1.AuxInt);
        p1 = op1.Args[0];
        d1 = v_1;
        m2 = v_2;
        if (m2.Op != OpStore) {
            break;
        }
        t2 = auxToType(m2.Aux);
        _ = m2.Args[2];
        op2 = m2.Args[0];
        if (op2.Op != OpOffPtr) {
            break;
        }
        o2 = auxIntToInt64(op2.AuxInt);
        p2 = op2.Args[0];
        d2 = m2.Args[1];
        m3 = m2.Args[2];
        if (m3.Op != OpStore) {
            break;
        }
        t3 = auxToType(m3.Aux);
        _ = m3.Args[2];
        op3 = m3.Args[0];
        if (op3.Op != OpOffPtr) {
            break;
        }
        o3 = auxIntToInt64(op3.AuxInt);
        p3 = op3.Args[0];
        d3 = m3.Args[1];
        m4 = m3.Args[2];
        if (m4.Op != OpStore) {
            break;
        }
        t4 = auxToType(m4.Aux);
        _ = m4.Args[2];
        op4 = m4.Args[0];
        if (op4.Op != OpOffPtr || auxIntToInt64(op4.AuxInt) != 0) {
            break;
        }
        p4 = op4.Args[0];
        d4 = m4.Args[1];
        m5 = m4.Args[2];
        if (m5.Op != OpZero) {
            break;
        }
        n = auxIntToInt64(m5.AuxInt);
        mem = m5.Args[1];
        p5 = m5.Args[0];
        if (!(m2.Uses == 1 && m3.Uses == 1 && m4.Uses == 1 && m5.Uses == 1 && o3 == t4.Size() && o2 - o3 == t3.Size() && o1 - o2 == t2.Size() && n == t4.Size() + t3.Size() + t2.Size() + t1.Size() && isSamePtr(p1, p2) && isSamePtr(p2, p3) && isSamePtr(p3, p4) && isSamePtr(p4, p5) && clobber(m2, m3, m4, m5))) {
            break;
        }
        v.reset(OpStore);
        v.Aux = typeToAux(t1);
        v0 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v0.Aux = typeToAux(t2);
        v1 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v1.Aux = typeToAux(t3);
        v2 = b.NewValue0(v.Pos, OpStore, types.TypeMem);
        v2.Aux = typeToAux(t4);
        v2.AddArg3(op4, d4, mem);
        v1.AddArg3(op3, d3, v2);
        v0.AddArg3(op2, d2, v1);
        v.AddArg3(op1, d1, v0);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpStringLen(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (StringLen (StringMake _ (Const64 <t> [c])))
    // result: (Const64 <t> [c])
    while (true) {
        if (v_0.Op != OpStringMake) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpConst64) {
            break;
        }
        var t = v_0_1.Type;
        var c = auxIntToInt64(v_0_1.AuxInt);
        v.reset(OpConst64);
        v.Type = t;
        v.AuxInt = int64ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpStringPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (StringPtr (StringMake (Addr <t> {s} base) _))
    // result: (Addr <t> {s} base)
    while (true) {
        if (v_0.Op != OpStringMake) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpAddr) {
            break;
        }
        var t = v_0_0.Type;
        var s = auxToSym(v_0_0.Aux);
        var @base = v_0_0.Args[0];
        v.reset(OpAddr);
        v.Type = t;
        v.Aux = symToAux(s);
        v.AddArg(base);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpStructSelect(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var fe = b.Func.fe; 
    // match: (StructSelect (StructMake1 x))
    // result: x
    while (true) {
        if (v_0.Op != OpStructMake1) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [0] (StructMake2 x _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpStructMake2) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [1] (StructMake2 _ x))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1 || v_0.Op != OpStructMake2) {
            break;
        }
        x = v_0.Args[1];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [0] (StructMake3 x _ _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpStructMake3) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [1] (StructMake3 _ x _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1 || v_0.Op != OpStructMake3) {
            break;
        }
        x = v_0.Args[1];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [2] (StructMake3 _ _ x))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2 || v_0.Op != OpStructMake3) {
            break;
        }
        x = v_0.Args[2];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [0] (StructMake4 x _ _ _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpStructMake4) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [1] (StructMake4 _ x _ _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1 || v_0.Op != OpStructMake4) {
            break;
        }
        x = v_0.Args[1];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [2] (StructMake4 _ _ x _))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2 || v_0.Op != OpStructMake4) {
            break;
        }
        x = v_0.Args[2];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [3] (StructMake4 _ _ _ x))
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3 || v_0.Op != OpStructMake4) {
            break;
        }
        x = v_0.Args[3];
        v.copyOf(x);
        return true;

    } 
    // match: (StructSelect [i] x:(Load <t> ptr mem))
    // cond: !fe.CanSSA(t)
    // result: @x.Block (Load <v.Type> (OffPtr <v.Type.PtrTo()> [t.FieldOff(int(i))] ptr) mem)
    while (true) {
        var i = auxIntToInt64(v.AuxInt);
        x = v_0;
        if (x.Op != OpLoad) {
            break;
        }
        var t = x.Type;
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(!fe.CanSSA(t))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(v.Pos, OpLoad, v.Type);
        v.copyOf(v0);
        var v1 = b.NewValue0(v.Pos, OpOffPtr, v.Type.PtrTo());
        v1.AuxInt = int64ToAuxInt(t.FieldOff(int(i)));
        v1.AddArg(ptr);
        v0.AddArg2(v1, mem);
        return true;

    } 
    // match: (StructSelect [0] (IData x))
    // result: (IData x)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0 || v_0.Op != OpIData) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpIData);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Sub16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c-d])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpConst16) {
            break;
        }
        var d = auxIntToInt16(v_1.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(c - d);
        return true;

    } 
    // match: (Sub16 x (Const16 <t> [c]))
    // cond: x.Op != OpConst16
    // result: (Add16 (Const16 <t> [-c]) x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst16) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToInt16(v_1.AuxInt);
        if (!(x.Op != OpConst16)) {
            break;
        }
        v.reset(OpAdd16);
        var v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(-c);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub16 <t> (Mul16 x y) (Mul16 x z))
    // result: (Mul16 x (Sub16 <t> y z))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpMul16) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                var y = v_0_1;
                if (v_1.Op != OpMul16) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var z = v_1_1;
                        v.reset(OpMul16);
                        v0 = b.NewValue0(v.Pos, OpSub16, t);
                        v0.AddArg2(y, z);
                        v.AddArg2(x, v0);
                        return true;

                    }

                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub16 x x)
    // result: (Const16 [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Sub16 (Add16 x y) x)
    // result: y
    while (true) {
        if (v_0.Op != OpAdd16) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (x != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub16 (Add16 x y) y)
    // result: x
    while (true) {
        if (v_0.Op != OpAdd16) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (y != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub16 x (Sub16 i:(Const16 <t>) z))
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Sub16 (Add16 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpSub16) {
            break;
        }
        z = v_1.Args[1];
        var i = v_1.Args[0];
        if (i.Op != OpConst16) {
            break;
        }
        t = i.Type;
        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
            break;
        }
        v.reset(OpSub16);
        v0 = b.NewValue0(v.Pos, OpAdd16, t);
        v0.AddArg2(x, z);
        v.AddArg2(v0, i);
        return true;

    } 
    // match: (Sub16 x (Add16 z i:(Const16 <t>)))
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Sub16 (Sub16 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpAdd16) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_1_0;
                i = v_1_1;
                if (i.Op != OpConst16) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                t = i.Type;
                if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                    continue;
                }

                v.reset(OpSub16);
                v0 = b.NewValue0(v.Pos, OpSub16, t);
                v0.AddArg2(x, z);
                v.AddArg2(v0, i);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub16 (Sub16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Sub16 i (Add16 <t> z x))
    while (true) {
        if (v_0.Op != OpSub16) {
            break;
        }
        z = v_0.Args[1];
        i = v_0.Args[0];
        if (i.Op != OpConst16) {
            break;
        }
        t = i.Type;
        x = v_1;
        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
            break;
        }
        v.reset(OpSub16);
        v0 = b.NewValue0(v.Pos, OpAdd16, t);
        v0.AddArg2(z, x);
        v.AddArg2(i, v0);
        return true;

    } 
    // match: (Sub16 (Add16 z i:(Const16 <t>)) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Add16 i (Sub16 <t> z x))
    while (true) {
        if (v_0.Op != OpAdd16) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_0_0;
                i = v_0_1;
                if (i.Op != OpConst16) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                    continue;
                }

                v.reset(OpAdd16);
                v0 = b.NewValue0(v.Pos, OpSub16, t);
                v0.AddArg2(z, x);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub16 (Const16 <t> [c]) (Sub16 (Const16 <t> [d]) x))
    // result: (Add16 (Const16 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpSub16) {
            break;
        }
        x = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
            break;
        }
        d = auxIntToInt16(v_1_0.AuxInt);
        v.reset(OpAdd16);
        v0 = b.NewValue0(v.Pos, OpConst16, t);
        v0.AuxInt = int16ToAuxInt(c - d);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub16 (Const16 <t> [c]) (Add16 (Const16 <t> [d]) x))
    // result: (Sub16 (Const16 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt16(v_0.AuxInt);
        if (v_1.Op != OpAdd16) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                d = auxIntToInt16(v_1_0.AuxInt);
                x = v_1_1;
                v.reset(OpSub16);
                v0 = b.NewValue0(v.Pos, OpConst16, t);
                v0.AuxInt = int16ToAuxInt(c - d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Sub32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c-d])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpConst32) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(c - d);
        return true;

    } 
    // match: (Sub32 x (Const32 <t> [c]))
    // cond: x.Op != OpConst32
    // result: (Add32 (Const32 <t> [-c]) x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst32) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToInt32(v_1.AuxInt);
        if (!(x.Op != OpConst32)) {
            break;
        }
        v.reset(OpAdd32);
        var v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(-c);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub32 <t> (Mul32 x y) (Mul32 x z))
    // result: (Mul32 x (Sub32 <t> y z))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpMul32) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                var y = v_0_1;
                if (v_1.Op != OpMul32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var z = v_1_1;
                        v.reset(OpMul32);
                        v0 = b.NewValue0(v.Pos, OpSub32, t);
                        v0.AddArg2(y, z);
                        v.AddArg2(x, v0);
                        return true;

                    }

                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub32 x x)
    // result: (Const32 [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Sub32 (Add32 x y) x)
    // result: y
    while (true) {
        if (v_0.Op != OpAdd32) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (x != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub32 (Add32 x y) y)
    // result: x
    while (true) {
        if (v_0.Op != OpAdd32) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (y != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub32 x (Sub32 i:(Const32 <t>) z))
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Sub32 (Add32 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpSub32) {
            break;
        }
        z = v_1.Args[1];
        var i = v_1.Args[0];
        if (i.Op != OpConst32) {
            break;
        }
        t = i.Type;
        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
            break;
        }
        v.reset(OpSub32);
        v0 = b.NewValue0(v.Pos, OpAdd32, t);
        v0.AddArg2(x, z);
        v.AddArg2(v0, i);
        return true;

    } 
    // match: (Sub32 x (Add32 z i:(Const32 <t>)))
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Sub32 (Sub32 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpAdd32) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_1_0;
                i = v_1_1;
                if (i.Op != OpConst32) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                t = i.Type;
                if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                    continue;
                }

                v.reset(OpSub32);
                v0 = b.NewValue0(v.Pos, OpSub32, t);
                v0.AddArg2(x, z);
                v.AddArg2(v0, i);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub32 (Sub32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Sub32 i (Add32 <t> z x))
    while (true) {
        if (v_0.Op != OpSub32) {
            break;
        }
        z = v_0.Args[1];
        i = v_0.Args[0];
        if (i.Op != OpConst32) {
            break;
        }
        t = i.Type;
        x = v_1;
        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
            break;
        }
        v.reset(OpSub32);
        v0 = b.NewValue0(v.Pos, OpAdd32, t);
        v0.AddArg2(z, x);
        v.AddArg2(i, v0);
        return true;

    } 
    // match: (Sub32 (Add32 z i:(Const32 <t>)) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Add32 i (Sub32 <t> z x))
    while (true) {
        if (v_0.Op != OpAdd32) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_0_0;
                i = v_0_1;
                if (i.Op != OpConst32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                    continue;
                }

                v.reset(OpAdd32);
                v0 = b.NewValue0(v.Pos, OpSub32, t);
                v0.AddArg2(z, x);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub32 (Const32 <t> [c]) (Sub32 (Const32 <t> [d]) x))
    // result: (Add32 (Const32 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpSub32) {
            break;
        }
        x = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
            break;
        }
        d = auxIntToInt32(v_1_0.AuxInt);
        v.reset(OpAdd32);
        v0 = b.NewValue0(v.Pos, OpConst32, t);
        v0.AuxInt = int32ToAuxInt(c - d);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub32 (Const32 <t> [c]) (Add32 (Const32 <t> [d]) x))
    // result: (Sub32 (Const32 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpAdd32) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                d = auxIntToInt32(v_1_0.AuxInt);
                x = v_1_1;
                v.reset(OpSub32);
                v0 = b.NewValue0(v.Pos, OpConst32, t);
                v0.AuxInt = int32ToAuxInt(c - d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Sub32F (Const32F [c]) (Const32F [d]))
    // cond: c-d == c-d
    // result: (Const32F [c-d])
    while (true) {
        if (v_0.Op != OpConst32F) {
            break;
        }
        var c = auxIntToFloat32(v_0.AuxInt);
        if (v_1.Op != OpConst32F) {
            break;
        }
        var d = auxIntToFloat32(v_1.AuxInt);
        if (!(c - d == c - d)) {
            break;
        }
        v.reset(OpConst32F);
        v.AuxInt = float32ToAuxInt(c - d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Sub64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c-d])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpConst64) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(c - d);
        return true;

    } 
    // match: (Sub64 x (Const64 <t> [c]))
    // cond: x.Op != OpConst64
    // result: (Add64 (Const64 <t> [-c]) x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToInt64(v_1.AuxInt);
        if (!(x.Op != OpConst64)) {
            break;
        }
        v.reset(OpAdd64);
        var v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(-c);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub64 <t> (Mul64 x y) (Mul64 x z))
    // result: (Mul64 x (Sub64 <t> y z))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpMul64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                var y = v_0_1;
                if (v_1.Op != OpMul64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var z = v_1_1;
                        v.reset(OpMul64);
                        v0 = b.NewValue0(v.Pos, OpSub64, t);
                        v0.AddArg2(y, z);
                        v.AddArg2(x, v0);
                        return true;

                    }

                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub64 x x)
    // result: (Const64 [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Sub64 (Add64 x y) x)
    // result: y
    while (true) {
        if (v_0.Op != OpAdd64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (x != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub64 (Add64 x y) y)
    // result: x
    while (true) {
        if (v_0.Op != OpAdd64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (y != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub64 x (Sub64 i:(Const64 <t>) z))
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Sub64 (Add64 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpSub64) {
            break;
        }
        z = v_1.Args[1];
        var i = v_1.Args[0];
        if (i.Op != OpConst64) {
            break;
        }
        t = i.Type;
        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
            break;
        }
        v.reset(OpSub64);
        v0 = b.NewValue0(v.Pos, OpAdd64, t);
        v0.AddArg2(x, z);
        v.AddArg2(v0, i);
        return true;

    } 
    // match: (Sub64 x (Add64 z i:(Const64 <t>)))
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Sub64 (Sub64 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpAdd64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_1_0;
                i = v_1_1;
                if (i.Op != OpConst64) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                t = i.Type;
                if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                    continue;
                }

                v.reset(OpSub64);
                v0 = b.NewValue0(v.Pos, OpSub64, t);
                v0.AddArg2(x, z);
                v.AddArg2(v0, i);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub64 (Sub64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Sub64 i (Add64 <t> z x))
    while (true) {
        if (v_0.Op != OpSub64) {
            break;
        }
        z = v_0.Args[1];
        i = v_0.Args[0];
        if (i.Op != OpConst64) {
            break;
        }
        t = i.Type;
        x = v_1;
        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
            break;
        }
        v.reset(OpSub64);
        v0 = b.NewValue0(v.Pos, OpAdd64, t);
        v0.AddArg2(z, x);
        v.AddArg2(i, v0);
        return true;

    } 
    // match: (Sub64 (Add64 z i:(Const64 <t>)) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Add64 i (Sub64 <t> z x))
    while (true) {
        if (v_0.Op != OpAdd64) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_0_0;
                i = v_0_1;
                if (i.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                    continue;
                }

                v.reset(OpAdd64);
                v0 = b.NewValue0(v.Pos, OpSub64, t);
                v0.AddArg2(z, x);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub64 (Const64 <t> [c]) (Sub64 (Const64 <t> [d]) x))
    // result: (Add64 (Const64 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpSub64) {
            break;
        }
        x = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
            break;
        }
        d = auxIntToInt64(v_1_0.AuxInt);
        v.reset(OpAdd64);
        v0 = b.NewValue0(v.Pos, OpConst64, t);
        v0.AuxInt = int64ToAuxInt(c - d);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub64 (Const64 <t> [c]) (Add64 (Const64 <t> [d]) x))
    // result: (Sub64 (Const64 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpAdd64) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                d = auxIntToInt64(v_1_0.AuxInt);
                x = v_1_1;
                v.reset(OpSub64);
                v0 = b.NewValue0(v.Pos, OpConst64, t);
                v0.AuxInt = int64ToAuxInt(c - d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Sub64F (Const64F [c]) (Const64F [d]))
    // cond: c-d == c-d
    // result: (Const64F [c-d])
    while (true) {
        if (v_0.Op != OpConst64F) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        if (v_1.Op != OpConst64F) {
            break;
        }
        var d = auxIntToFloat64(v_1.AuxInt);
        if (!(c - d == c - d)) {
            break;
        }
        v.reset(OpConst64F);
        v.AuxInt = float64ToAuxInt(c - d);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpSub8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Sub8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c-d])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpConst8) {
            break;
        }
        var d = auxIntToInt8(v_1.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(c - d);
        return true;

    } 
    // match: (Sub8 x (Const8 <t> [c]))
    // cond: x.Op != OpConst8
    // result: (Add8 (Const8 <t> [-c]) x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst8) {
            break;
        }
        var t = v_1.Type;
        c = auxIntToInt8(v_1.AuxInt);
        if (!(x.Op != OpConst8)) {
            break;
        }
        v.reset(OpAdd8);
        var v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(-c);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub8 <t> (Mul8 x y) (Mul8 x z))
    // result: (Mul8 x (Sub8 <t> y z))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpMul8) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                var y = v_0_1;
                if (v_1.Op != OpMul8) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var z = v_1_1;
                        v.reset(OpMul8);
                        v0 = b.NewValue0(v.Pos, OpSub8, t);
                        v0.AddArg2(y, z);
                        v.AddArg2(x, v0);
                        return true;

                    }

                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub8 x x)
    // result: (Const8 [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Sub8 (Add8 x y) x)
    // result: y
    while (true) {
        if (v_0.Op != OpAdd8) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (x != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub8 (Add8 x y) y)
    // result: x
    while (true) {
        if (v_0.Op != OpAdd8) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                y = v_0_1;
                if (y != v_1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub8 x (Sub8 i:(Const8 <t>) z))
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Sub8 (Add8 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpSub8) {
            break;
        }
        z = v_1.Args[1];
        var i = v_1.Args[0];
        if (i.Op != OpConst8) {
            break;
        }
        t = i.Type;
        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
            break;
        }
        v.reset(OpSub8);
        v0 = b.NewValue0(v.Pos, OpAdd8, t);
        v0.AddArg2(x, z);
        v.AddArg2(v0, i);
        return true;

    } 
    // match: (Sub8 x (Add8 z i:(Const8 <t>)))
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Sub8 (Sub8 <t> x z) i)
    while (true) {
        x = v_0;
        if (v_1.Op != OpAdd8) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_1_0;
                i = v_1_1;
                if (i.Op != OpConst8) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                t = i.Type;
                if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                    continue;
                }

                v.reset(OpSub8);
                v0 = b.NewValue0(v.Pos, OpSub8, t);
                v0.AddArg2(x, z);
                v.AddArg2(v0, i);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub8 (Sub8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Sub8 i (Add8 <t> z x))
    while (true) {
        if (v_0.Op != OpSub8) {
            break;
        }
        z = v_0.Args[1];
        i = v_0.Args[0];
        if (i.Op != OpConst8) {
            break;
        }
        t = i.Type;
        x = v_1;
        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
            break;
        }
        v.reset(OpSub8);
        v0 = b.NewValue0(v.Pos, OpAdd8, t);
        v0.AddArg2(z, x);
        v.AddArg2(i, v0);
        return true;

    } 
    // match: (Sub8 (Add8 z i:(Const8 <t>)) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Add8 i (Sub8 <t> z x))
    while (true) {
        if (v_0.Op != OpAdd8) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                z = v_0_0;
                i = v_0_1;
                if (i.Op != OpConst8) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                t = i.Type;
                x = v_1;
                if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                    continue;
                }

                v.reset(OpAdd8);
                v0 = b.NewValue0(v.Pos, OpSub8, t);
                v0.AddArg2(z, x);
                v.AddArg2(i, v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Sub8 (Const8 <t> [c]) (Sub8 (Const8 <t> [d]) x))
    // result: (Add8 (Const8 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpSub8) {
            break;
        }
        x = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
            break;
        }
        d = auxIntToInt8(v_1_0.AuxInt);
        v.reset(OpAdd8);
        v0 = b.NewValue0(v.Pos, OpConst8, t);
        v0.AuxInt = int8ToAuxInt(c - d);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Sub8 (Const8 <t> [c]) (Add8 (Const8 <t> [d]) x))
    // result: (Sub8 (Const8 <t> [c-d]) x)
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        t = v_0.Type;
        c = auxIntToInt8(v_0.AuxInt);
        if (v_1.Op != OpAdd8) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }

                d = auxIntToInt8(v_1_0.AuxInt);
                x = v_1_1;
                v.reset(OpSub8);
                v0 = b.NewValue0(v.Pos, OpConst8, t);
                v0.AuxInt = int8ToAuxInt(c - d);
                v.AddArg2(v0, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc16to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc16to8 (Const16 [c]))
    // result: (Const8 [int8(c)])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(c));
        return true;

    } 
    // match: (Trunc16to8 (ZeroExt8to16 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt8to16) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc16to8 (SignExt8to16 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt8to16) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc16to8 (And16 (Const16 [y]) x))
    // cond: y&0xFF == 0xFF
    // result: (Trunc16to8 x)
    while (true) {
        if (v_0.Op != OpAnd16) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst16) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt16(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFF == 0xFF)) {
                    continue;
                }

                v.reset(OpTrunc16to8);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc32to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc32to16 (Const32 [c]))
    // result: (Const16 [int16(c)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(c));
        return true;

    } 
    // match: (Trunc32to16 (ZeroExt8to32 x))
    // result: (ZeroExt8to16 x)
    while (true) {
        if (v_0.Op != OpZeroExt8to32) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpZeroExt8to16);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc32to16 (ZeroExt16to32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt16to32) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc32to16 (SignExt8to32 x))
    // result: (SignExt8to16 x)
    while (true) {
        if (v_0.Op != OpSignExt8to32) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpSignExt8to16);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc32to16 (SignExt16to32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt16to32) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc32to16 (And32 (Const32 [y]) x))
    // cond: y&0xFFFF == 0xFFFF
    // result: (Trunc32to16 x)
    while (true) {
        if (v_0.Op != OpAnd32) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt32(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFFFF == 0xFFFF)) {
                    continue;
                }

                v.reset(OpTrunc32to16);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc32to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc32to8 (Const32 [c]))
    // result: (Const8 [int8(c)])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(c));
        return true;

    } 
    // match: (Trunc32to8 (ZeroExt8to32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt8to32) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc32to8 (SignExt8to32 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt8to32) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc32to8 (And32 (Const32 [y]) x))
    // cond: y&0xFF == 0xFF
    // result: (Trunc32to8 x)
    while (true) {
        if (v_0.Op != OpAnd32) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst32) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt32(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFF == 0xFF)) {
                    continue;
                }

                v.reset(OpTrunc32to8);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc64to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to16 (Const64 [c]))
    // result: (Const16 [int16(c)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(c));
        return true;

    } 
    // match: (Trunc64to16 (ZeroExt8to64 x))
    // result: (ZeroExt8to16 x)
    while (true) {
        if (v_0.Op != OpZeroExt8to64) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpZeroExt8to16);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to16 (ZeroExt16to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt16to64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to16 (SignExt8to64 x))
    // result: (SignExt8to16 x)
    while (true) {
        if (v_0.Op != OpSignExt8to64) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpSignExt8to16);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to16 (SignExt16to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt16to64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to16 (And64 (Const64 [y]) x))
    // cond: y&0xFFFF == 0xFFFF
    // result: (Trunc64to16 x)
    while (true) {
        if (v_0.Op != OpAnd64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt64(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFFFF == 0xFFFF)) {
                    continue;
                }

                v.reset(OpTrunc64to16);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc64to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to32 (Const64 [c]))
    // result: (Const32 [int32(c)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;

    } 
    // match: (Trunc64to32 (ZeroExt8to64 x))
    // result: (ZeroExt8to32 x)
    while (true) {
        if (v_0.Op != OpZeroExt8to64) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpZeroExt8to32);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to32 (ZeroExt16to64 x))
    // result: (ZeroExt16to32 x)
    while (true) {
        if (v_0.Op != OpZeroExt16to64) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpZeroExt16to32);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to32 (ZeroExt32to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt32to64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to32 (SignExt8to64 x))
    // result: (SignExt8to32 x)
    while (true) {
        if (v_0.Op != OpSignExt8to64) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpSignExt8to32);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to32 (SignExt16to64 x))
    // result: (SignExt16to32 x)
    while (true) {
        if (v_0.Op != OpSignExt16to64) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpSignExt16to32);
        v.AddArg(x);
        return true;

    } 
    // match: (Trunc64to32 (SignExt32to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt32to64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to32 (And64 (Const64 [y]) x))
    // cond: y&0xFFFFFFFF == 0xFFFFFFFF
    // result: (Trunc64to32 x)
    while (true) {
        if (v_0.Op != OpAnd64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt64(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFFFFFFFF == 0xFFFFFFFF)) {
                    continue;
                }

                v.reset(OpTrunc64to32);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpTrunc64to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to8 (Const64 [c]))
    // result: (Const8 [int8(c)])
    while (true) {
        if (v_0.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(int8(c));
        return true;

    } 
    // match: (Trunc64to8 (ZeroExt8to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpZeroExt8to64) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to8 (SignExt8to64 x))
    // result: x
    while (true) {
        if (v_0.Op != OpSignExt8to64) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;

    } 
    // match: (Trunc64to8 (And64 (Const64 [y]) x))
    // cond: y&0xFF == 0xFF
    // result: (Trunc64to8 x)
    while (true) {
        if (v_0.Op != OpAnd64) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpConst64) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var y = auxIntToInt64(v_0_0.AuxInt);
                x = v_0_1;
                if (!(y & 0xFF == 0xFF)) {
                    continue;
                }

                v.reset(OpTrunc64to8);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpXor16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Xor16 (Const16 [c]) (Const16 [d]))
    // result: (Const16 [c^d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpConst16) {
                    continue;
                }

                var d = auxIntToInt16(v_1.AuxInt);
                v.reset(OpConst16);
                v.AuxInt = int16ToAuxInt(c ^ d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor16 x x)
    // result: (Const16 [0])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;

    } 
    // match: (Xor16 (Const16 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16 || auxIntToInt16(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor16 x (Xor16 x y))
    // result: y
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpXor16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.copyOf(y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor16 (Xor16 i:(Const16 <t>) z) x)
    // cond: (z.Op != OpConst16 && x.Op != OpConst16)
    // result: (Xor16 i (Xor16 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpXor16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst16) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst16 && x.Op != OpConst16)) {
                            continue;
                        }

                        v.reset(OpXor16);
                        var v0 = b.NewValue0(v.Pos, OpXor16, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor16 (Const16 <t> [c]) (Xor16 (Const16 <t> [d]) x))
    // result: (Xor16 (Const16 <t> [c^d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst16) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt16(v_0.AuxInt);
                if (v_1.Op != OpXor16) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst16 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt16(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpXor16);
                        v0 = b.NewValue0(v.Pos, OpConst16, t);
                        v0.AuxInt = int16ToAuxInt(c ^ d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpXor32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Xor32 (Const32 [c]) (Const32 [d]))
    // result: (Const32 [c^d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpConst32) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpConst32);
                v.AuxInt = int32ToAuxInt(c ^ d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor32 x x)
    // result: (Const32 [0])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Xor32 (Const32 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32 || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor32 x (Xor32 x y))
    // result: y
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpXor32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.copyOf(y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor32 (Xor32 i:(Const32 <t>) z) x)
    // cond: (z.Op != OpConst32 && x.Op != OpConst32)
    // result: (Xor32 i (Xor32 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpXor32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst32) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst32 && x.Op != OpConst32)) {
                            continue;
                        }

                        v.reset(OpXor32);
                        var v0 = b.NewValue0(v.Pos, OpXor32, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor32 (Const32 <t> [c]) (Xor32 (Const32 <t> [d]) x))
    // result: (Xor32 (Const32 <t> [c^d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpXor32) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst32 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt32(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpXor32);
                        v0 = b.NewValue0(v.Pos, OpConst32, t);
                        v0.AuxInt = int32ToAuxInt(c ^ d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpXor64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Xor64 (Const64 [c]) (Const64 [d]))
    // result: (Const64 [c^d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpConst64) {
                    continue;
                }

                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpConst64);
                v.AuxInt = int64ToAuxInt(c ^ d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor64 x x)
    // result: (Const64 [0])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(0);
        return true;

    } 
    // match: (Xor64 (Const64 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64 || auxIntToInt64(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor64 x (Xor64 x y))
    // result: y
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpXor64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.copyOf(y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor64 (Xor64 i:(Const64 <t>) z) x)
    // cond: (z.Op != OpConst64 && x.Op != OpConst64)
    // result: (Xor64 i (Xor64 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpXor64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst64) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst64 && x.Op != OpConst64)) {
                            continue;
                        }

                        v.reset(OpXor64);
                        var v0 = b.NewValue0(v.Pos, OpXor64, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor64 (Const64 <t> [c]) (Xor64 (Const64 <t> [d]) x))
    // result: (Xor64 (Const64 <t> [c^d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst64) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpXor64) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst64 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt64(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpXor64);
                        v0 = b.NewValue0(v.Pos, OpConst64, t);
                        v0.AuxInt = int64ToAuxInt(c ^ d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpXor8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Xor8 (Const8 [c]) (Const8 [d]))
    // result: (Const8 [c^d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpConst8) {
                    continue;
                }

                var d = auxIntToInt8(v_1.AuxInt);
                v.reset(OpConst8);
                v.AuxInt = int8ToAuxInt(c ^ d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor8 x x)
    // result: (Const8 [0])
    while (true) {
        var x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;

    } 
    // match: (Xor8 (Const8 [0]) x)
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8 || auxIntToInt8(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor8 x (Xor8 x y))
    // result: y
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpXor8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        var y = v_1_1;
                        v.copyOf(y);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor8 (Xor8 i:(Const8 <t>) z) x)
    // cond: (z.Op != OpConst8 && x.Op != OpConst8)
    // result: (Xor8 i (Xor8 <t> z x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpXor8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                _ = v_0.Args[1];
                var v_0_0 = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var i = v_0_0;
                        if (i.Op != OpConst8) {
                            continue;
                        (_i1, v_0_0, v_0_1) = (_i1 + 1, v_0_1, v_0_0);
                        }

                        var t = i.Type;
                        var z = v_0_1;
                        x = v_1;
                        if (!(z.Op != OpConst8 && x.Op != OpConst8)) {
                            continue;
                        }

                        v.reset(OpXor8);
                        var v0 = b.NewValue0(v.Pos, OpXor8, t);
                        v0.AddArg2(z, x);
                        v.AddArg2(i, v0);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Xor8 (Const8 <t> [c]) (Xor8 (Const8 <t> [d]) x))
    // result: (Xor8 (Const8 <t> [c^d]) x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpConst8) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                t = v_0.Type;
                c = auxIntToInt8(v_0.AuxInt);
                if (v_1.Op != OpXor8) {
                    continue;
                }

                _ = v_1.Args[1];
                v_1_0 = v_1.Args[0];
                v_1_1 = v_1.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        if (v_1_0.Op != OpConst8 || v_1_0.Type != t) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }

                        d = auxIntToInt8(v_1_0.AuxInt);
                        x = v_1_1;
                        v.reset(OpXor8);
                        v0 = b.NewValue0(v.Pos, OpConst8, t);
                        v0.AuxInt = int8ToAuxInt(c ^ d);
                        v.AddArg2(v0, x);
                        return true;

                    }


                    _i1 = _i1__prev3;
                }

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Zero (SelectN [0] call:(StaticLECall _ _)) mem:(SelectN [1] call))
    // cond: isSameCall(call.Aux, "runtime.newobject")
    // result: mem
    while (true) {
        if (v_0.Op != OpSelectN || auxIntToInt64(v_0.AuxInt) != 0) {
            break;
        }
        var call = v_0.Args[0];
        if (call.Op != OpStaticLECall || len(call.Args) != 2) {
            break;
        }
        var mem = v_1;
        if (mem.Op != OpSelectN || auxIntToInt64(mem.AuxInt) != 1 || call != mem.Args[0] || !(isSameCall(call.Aux, "runtime.newobject"))) {
            break;
        }
        v.copyOf(mem);
        return true;

    } 
    // match: (Zero {t1} [n] p1 store:(Store {t2} (OffPtr [o2] p2) _ mem))
    // cond: isSamePtr(p1, p2) && store.Uses == 1 && n >= o2 + t2.Size() && clobber(store)
    // result: (Zero {t1} [n] p1 mem)
    while (true) {
        var n = auxIntToInt64(v.AuxInt);
        var t1 = auxToType(v.Aux);
        var p1 = v_0;
        var store = v_1;
        if (store.Op != OpStore) {
            break;
        }
        var t2 = auxToType(store.Aux);
        mem = store.Args[2];
        var store_0 = store.Args[0];
        if (store_0.Op != OpOffPtr) {
            break;
        }
        var o2 = auxIntToInt64(store_0.AuxInt);
        var p2 = store_0.Args[0];
        if (!(isSamePtr(p1, p2) && store.Uses == 1 && n >= o2 + t2.Size() && clobber(store))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t1);
        v.AddArg2(p1, mem);
        return true;

    } 
    // match: (Zero {t} [n] dst1 move:(Move {t} [n] dst2 _ mem))
    // cond: move.Uses == 1 && isSamePtr(dst1, dst2) && clobber(move)
    // result: (Zero {t} [n] dst1 mem)
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        var t = auxToType(v.Aux);
        var dst1 = v_0;
        var move = v_1;
        if (move.Op != OpMove || auxIntToInt64(move.AuxInt) != n || auxToType(move.Aux) != t) {
            break;
        }
        mem = move.Args[2];
        var dst2 = move.Args[0];
        if (!(move.Uses == 1 && isSamePtr(dst1, dst2) && clobber(move))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        v.AddArg2(dst1, mem);
        return true;

    } 
    // match: (Zero {t} [n] dst1 vardef:(VarDef {x} move:(Move {t} [n] dst2 _ mem)))
    // cond: move.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && clobber(move, vardef)
    // result: (Zero {t} [n] dst1 (VarDef {x} mem))
    while (true) {
        n = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        var vardef = v_1;
        if (vardef.Op != OpVarDef) {
            break;
        }
        var x = auxToSym(vardef.Aux);
        move = vardef.Args[0];
        if (move.Op != OpMove || auxIntToInt64(move.AuxInt) != n || auxToType(move.Aux) != t) {
            break;
        }
        mem = move.Args[2];
        dst2 = move.Args[0];
        if (!(move.Uses == 1 && vardef.Uses == 1 && isSamePtr(dst1, dst2) && clobber(move, vardef))) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(n);
        v.Aux = typeToAux(t);
        var v0 = b.NewValue0(v.Pos, OpVarDef, types.TypeMem);
        v0.Aux = symToAux(x);
        v0.AddArg(mem);
        v.AddArg2(dst1, v0);
        return true;

    } 
    // match: (Zero {t} [s] dst1 zero:(Zero {t} [s] dst2 _))
    // cond: isSamePtr(dst1, dst2)
    // result: zero
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        var zero = v_1;
        if (zero.Op != OpZero || auxIntToInt64(zero.AuxInt) != s || auxToType(zero.Aux) != t) {
            break;
        }
        dst2 = zero.Args[0];
        if (!(isSamePtr(dst1, dst2))) {
            break;
        }
        v.copyOf(zero);
        return true;

    } 
    // match: (Zero {t} [s] dst1 vardef:(VarDef (Zero {t} [s] dst2 _)))
    // cond: isSamePtr(dst1, dst2)
    // result: vardef
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst1 = v_0;
        vardef = v_1;
        if (vardef.Op != OpVarDef) {
            break;
        }
        var vardef_0 = vardef.Args[0];
        if (vardef_0.Op != OpZero || auxIntToInt64(vardef_0.AuxInt) != s || auxToType(vardef_0.Aux) != t) {
            break;
        }
        dst2 = vardef_0.Args[0];
        if (!(isSamePtr(dst1, dst2))) {
            break;
        }
        v.copyOf(vardef);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt16to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt16to32 (Const16 [c]))
    // result: (Const32 [int32(uint16(c))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(uint16(c)));
        return true;

    } 
    // match: (ZeroExt16to32 (Trunc32to16 x:(Rsh32Ux64 _ (Const64 [s]))))
    // cond: s >= 16
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc32to16) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh32Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 16)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt16to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt16to64 (Const16 [c]))
    // result: (Const64 [int64(uint16(c))])
    while (true) {
        if (v_0.Op != OpConst16) {
            break;
        }
        var c = auxIntToInt16(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint16(c)));
        return true;

    } 
    // match: (ZeroExt16to64 (Trunc64to16 x:(Rsh64Ux64 _ (Const64 [s]))))
    // cond: s >= 48
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to16) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 48)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt32to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt32to64 (Const32 [c]))
    // result: (Const64 [int64(uint32(c))])
    while (true) {
        if (v_0.Op != OpConst32) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint32(c)));
        return true;

    } 
    // match: (ZeroExt32to64 (Trunc64to32 x:(Rsh64Ux64 _ (Const64 [s]))))
    // cond: s >= 32
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to32) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 32)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt8to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt8to16 (Const8 [c]))
    // result: (Const16 [int16( uint8(c))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(int16(uint8(c)));
        return true;

    } 
    // match: (ZeroExt8to16 (Trunc16to8 x:(Rsh16Ux64 _ (Const64 [s]))))
    // cond: s >= 8
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc16to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh16Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 8)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt8to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt8to32 (Const8 [c]))
    // result: (Const32 [int32( uint8(c))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(int32(uint8(c)));
        return true;

    } 
    // match: (ZeroExt8to32 (Trunc32to8 x:(Rsh32Ux64 _ (Const64 [s]))))
    // cond: s >= 24
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc32to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh32Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 24)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValuegeneric_OpZeroExt8to64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ZeroExt8to64 (Const8 [c]))
    // result: (Const64 [int64( uint8(c))])
    while (true) {
        if (v_0.Op != OpConst8) {
            break;
        }
        var c = auxIntToInt8(v_0.AuxInt);
        v.reset(OpConst64);
        v.AuxInt = int64ToAuxInt(int64(uint8(c)));
        return true;

    } 
    // match: (ZeroExt8to64 (Trunc64to8 x:(Rsh64Ux64 _ (Const64 [s]))))
    // cond: s >= 56
    // result: x
    while (true) {
        if (v_0.Op != OpTrunc64to8) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpRsh64Ux64) {
            break;
        }
        _ = x.Args[1];
        var x_1 = x.Args[1];
        if (x_1.Op != OpConst64) {
            break;
        }
        var s = auxIntToInt64(x_1.AuxInt);
        if (!(s >= 56)) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteBlockgeneric(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Kind == BlockIf) 
        // match: (If (Not cond) yes no)
        // result: (If cond no yes)
        while (b.Controls[0].Op == OpNot) {
            var v_0 = b.Controls[0];
            var cond = v_0.Args[0];
            b.resetWithControl(BlockIf, cond);
            b.swapSuccessors();
            return true;
        } 
        // match: (If (ConstBool [c]) yes no)
        // cond: c
        // result: (First yes no)
        while (b.Controls[0].Op == OpConstBool) {
            v_0 = b.Controls[0];
            var c = auxIntToBool(v_0.AuxInt);
            if (!(c)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (If (ConstBool [c]) yes no)
        // cond: !c
        // result: (First no yes)
        while (b.Controls[0].Op == OpConstBool) {
            v_0 = b.Controls[0];
            c = auxIntToBool(v_0.AuxInt);
            if (!(!c)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
        return false;

}

} // end ssa_package
