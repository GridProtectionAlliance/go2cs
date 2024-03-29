// Code generated from gen/S390X.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 13 06:21:23 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewriteS390X.go
namespace go.cmd.compile.@internal;

using math = math_package;
using types = cmd.compile.@internal.types_package;
using s390x = cmd.@internal.obj.s390x_package;

public static partial class ssa_package {

private static bool rewriteValueS390X(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAdd16) 
        v.Op = OpS390XADDW;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = OpS390XADDW;
        return true;
    else if (v.Op == OpAdd32F) 
        return rewriteValueS390X_OpAdd32F(_addr_v);
    else if (v.Op == OpAdd64) 
        v.Op = OpS390XADD;
        return true;
    else if (v.Op == OpAdd64F) 
        return rewriteValueS390X_OpAdd64F(_addr_v);
    else if (v.Op == OpAdd8) 
        v.Op = OpS390XADDW;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = OpS390XADD;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValueS390X_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = OpS390XANDW;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = OpS390XANDW;
        return true;
    else if (v.Op == OpAnd64) 
        v.Op = OpS390XAND;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = OpS390XANDW;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = OpS390XANDW;
        return true;
    else if (v.Op == OpAtomicAdd32) 
        return rewriteValueS390X_OpAtomicAdd32(_addr_v);
    else if (v.Op == OpAtomicAdd64) 
        return rewriteValueS390X_OpAtomicAdd64(_addr_v);
    else if (v.Op == OpAtomicAnd32) 
        v.Op = OpS390XLAN;
        return true;
    else if (v.Op == OpAtomicAnd8) 
        return rewriteValueS390X_OpAtomicAnd8(_addr_v);
    else if (v.Op == OpAtomicCompareAndSwap32) 
        return rewriteValueS390X_OpAtomicCompareAndSwap32(_addr_v);
    else if (v.Op == OpAtomicCompareAndSwap64) 
        return rewriteValueS390X_OpAtomicCompareAndSwap64(_addr_v);
    else if (v.Op == OpAtomicExchange32) 
        return rewriteValueS390X_OpAtomicExchange32(_addr_v);
    else if (v.Op == OpAtomicExchange64) 
        return rewriteValueS390X_OpAtomicExchange64(_addr_v);
    else if (v.Op == OpAtomicLoad32) 
        return rewriteValueS390X_OpAtomicLoad32(_addr_v);
    else if (v.Op == OpAtomicLoad64) 
        return rewriteValueS390X_OpAtomicLoad64(_addr_v);
    else if (v.Op == OpAtomicLoad8) 
        return rewriteValueS390X_OpAtomicLoad8(_addr_v);
    else if (v.Op == OpAtomicLoadAcq32) 
        return rewriteValueS390X_OpAtomicLoadAcq32(_addr_v);
    else if (v.Op == OpAtomicLoadPtr) 
        return rewriteValueS390X_OpAtomicLoadPtr(_addr_v);
    else if (v.Op == OpAtomicOr32) 
        v.Op = OpS390XLAO;
        return true;
    else if (v.Op == OpAtomicOr8) 
        return rewriteValueS390X_OpAtomicOr8(_addr_v);
    else if (v.Op == OpAtomicStore32) 
        return rewriteValueS390X_OpAtomicStore32(_addr_v);
    else if (v.Op == OpAtomicStore64) 
        return rewriteValueS390X_OpAtomicStore64(_addr_v);
    else if (v.Op == OpAtomicStore8) 
        return rewriteValueS390X_OpAtomicStore8(_addr_v);
    else if (v.Op == OpAtomicStorePtrNoWB) 
        return rewriteValueS390X_OpAtomicStorePtrNoWB(_addr_v);
    else if (v.Op == OpAtomicStoreRel32) 
        return rewriteValueS390X_OpAtomicStoreRel32(_addr_v);
    else if (v.Op == OpAvg64u) 
        return rewriteValueS390X_OpAvg64u(_addr_v);
    else if (v.Op == OpBitLen64) 
        return rewriteValueS390X_OpBitLen64(_addr_v);
    else if (v.Op == OpBswap32) 
        v.Op = OpS390XMOVWBR;
        return true;
    else if (v.Op == OpBswap64) 
        v.Op = OpS390XMOVDBR;
        return true;
    else if (v.Op == OpCeil) 
        return rewriteValueS390X_OpCeil(_addr_v);
    else if (v.Op == OpClosureCall) 
        v.Op = OpS390XCALLclosure;
        return true;
    else if (v.Op == OpCom16) 
        v.Op = OpS390XNOTW;
        return true;
    else if (v.Op == OpCom32) 
        v.Op = OpS390XNOTW;
        return true;
    else if (v.Op == OpCom64) 
        v.Op = OpS390XNOT;
        return true;
    else if (v.Op == OpCom8) 
        v.Op = OpS390XNOTW;
        return true;
    else if (v.Op == OpConst16) 
        return rewriteValueS390X_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        return rewriteValueS390X_OpConst32(_addr_v);
    else if (v.Op == OpConst32F) 
        v.Op = OpS390XFMOVSconst;
        return true;
    else if (v.Op == OpConst64) 
        return rewriteValueS390X_OpConst64(_addr_v);
    else if (v.Op == OpConst64F) 
        v.Op = OpS390XFMOVDconst;
        return true;
    else if (v.Op == OpConst8) 
        return rewriteValueS390X_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValueS390X_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValueS390X_OpConstNil(_addr_v);
    else if (v.Op == OpCtz32) 
        return rewriteValueS390X_OpCtz32(_addr_v);
    else if (v.Op == OpCtz32NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCtz64) 
        return rewriteValueS390X_OpCtz64(_addr_v);
    else if (v.Op == OpCtz64NonZero) 
        v.Op = OpCtz64;
        return true;
    else if (v.Op == OpCvt32Fto32) 
        v.Op = OpS390XCFEBRA;
        return true;
    else if (v.Op == OpCvt32Fto32U) 
        v.Op = OpS390XCLFEBR;
        return true;
    else if (v.Op == OpCvt32Fto64) 
        v.Op = OpS390XCGEBRA;
        return true;
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = OpS390XLDEBR;
        return true;
    else if (v.Op == OpCvt32Fto64U) 
        v.Op = OpS390XCLGEBR;
        return true;
    else if (v.Op == OpCvt32Uto32F) 
        v.Op = OpS390XCELFBR;
        return true;
    else if (v.Op == OpCvt32Uto64F) 
        v.Op = OpS390XCDLFBR;
        return true;
    else if (v.Op == OpCvt32to32F) 
        v.Op = OpS390XCEFBRA;
        return true;
    else if (v.Op == OpCvt32to64F) 
        v.Op = OpS390XCDFBRA;
        return true;
    else if (v.Op == OpCvt64Fto32) 
        v.Op = OpS390XCFDBRA;
        return true;
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = OpS390XLEDBR;
        return true;
    else if (v.Op == OpCvt64Fto32U) 
        v.Op = OpS390XCLFDBR;
        return true;
    else if (v.Op == OpCvt64Fto64) 
        v.Op = OpS390XCGDBRA;
        return true;
    else if (v.Op == OpCvt64Fto64U) 
        v.Op = OpS390XCLGDBR;
        return true;
    else if (v.Op == OpCvt64Uto32F) 
        v.Op = OpS390XCELGBR;
        return true;
    else if (v.Op == OpCvt64Uto64F) 
        v.Op = OpS390XCDLGBR;
        return true;
    else if (v.Op == OpCvt64to32F) 
        v.Op = OpS390XCEGBRA;
        return true;
    else if (v.Op == OpCvt64to64F) 
        v.Op = OpS390XCDGBRA;
        return true;
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        return rewriteValueS390X_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValueS390X_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValueS390X_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        v.Op = OpS390XFDIVS;
        return true;
    else if (v.Op == OpDiv32u) 
        return rewriteValueS390X_OpDiv32u(_addr_v);
    else if (v.Op == OpDiv64) 
        return rewriteValueS390X_OpDiv64(_addr_v);
    else if (v.Op == OpDiv64F) 
        v.Op = OpS390XFDIV;
        return true;
    else if (v.Op == OpDiv64u) 
        v.Op = OpS390XDIVDU;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValueS390X_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValueS390X_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValueS390X_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValueS390X_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValueS390X_OpEq32F(_addr_v);
    else if (v.Op == OpEq64) 
        return rewriteValueS390X_OpEq64(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValueS390X_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValueS390X_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValueS390X_OpEqB(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValueS390X_OpEqPtr(_addr_v);
    else if (v.Op == OpFMA) 
        return rewriteValueS390X_OpFMA(_addr_v);
    else if (v.Op == OpFloor) 
        return rewriteValueS390X_OpFloor(_addr_v);
    else if (v.Op == OpGetCallerPC) 
        v.Op = OpS390XLoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = OpS390XLoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = OpS390XLoweredGetClosurePtr;
        return true;
    else if (v.Op == OpGetG) 
        v.Op = OpS390XLoweredGetG;
        return true;
    else if (v.Op == OpHmul32) 
        return rewriteValueS390X_OpHmul32(_addr_v);
    else if (v.Op == OpHmul32u) 
        return rewriteValueS390X_OpHmul32u(_addr_v);
    else if (v.Op == OpHmul64) 
        v.Op = OpS390XMULHD;
        return true;
    else if (v.Op == OpHmul64u) 
        v.Op = OpS390XMULHDU;
        return true;
    else if (v.Op == OpITab) 
        return rewriteValueS390X_OpITab(_addr_v);
    else if (v.Op == OpInterCall) 
        v.Op = OpS390XCALLinter;
        return true;
    else if (v.Op == OpIsInBounds) 
        return rewriteValueS390X_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValueS390X_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValueS390X_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValueS390X_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValueS390X_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValueS390X_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValueS390X_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValueS390X_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64) 
        return rewriteValueS390X_OpLeq64(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValueS390X_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq64U) 
        return rewriteValueS390X_OpLeq64U(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValueS390X_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValueS390X_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValueS390X_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValueS390X_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValueS390X_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValueS390X_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValueS390X_OpLess32U(_addr_v);
    else if (v.Op == OpLess64) 
        return rewriteValueS390X_OpLess64(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValueS390X_OpLess64F(_addr_v);
    else if (v.Op == OpLess64U) 
        return rewriteValueS390X_OpLess64U(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValueS390X_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValueS390X_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValueS390X_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValueS390X_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValueS390X_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValueS390X_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValueS390X_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValueS390X_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValueS390X_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValueS390X_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValueS390X_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValueS390X_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh64x16) 
        return rewriteValueS390X_OpLsh64x16(_addr_v);
    else if (v.Op == OpLsh64x32) 
        return rewriteValueS390X_OpLsh64x32(_addr_v);
    else if (v.Op == OpLsh64x64) 
        return rewriteValueS390X_OpLsh64x64(_addr_v);
    else if (v.Op == OpLsh64x8) 
        return rewriteValueS390X_OpLsh64x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValueS390X_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValueS390X_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValueS390X_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValueS390X_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValueS390X_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValueS390X_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValueS390X_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValueS390X_OpMod32u(_addr_v);
    else if (v.Op == OpMod64) 
        return rewriteValueS390X_OpMod64(_addr_v);
    else if (v.Op == OpMod64u) 
        v.Op = OpS390XMODDU;
        return true;
    else if (v.Op == OpMod8) 
        return rewriteValueS390X_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValueS390X_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValueS390X_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = OpS390XMULLW;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = OpS390XMULLW;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = OpS390XFMULS;
        return true;
    else if (v.Op == OpMul64) 
        v.Op = OpS390XMULLD;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = OpS390XFMUL;
        return true;
    else if (v.Op == OpMul64uhilo) 
        v.Op = OpS390XMLGR;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = OpS390XMULLW;
        return true;
    else if (v.Op == OpNeg16) 
        v.Op = OpS390XNEGW;
        return true;
    else if (v.Op == OpNeg32) 
        v.Op = OpS390XNEGW;
        return true;
    else if (v.Op == OpNeg32F) 
        v.Op = OpS390XFNEGS;
        return true;
    else if (v.Op == OpNeg64) 
        v.Op = OpS390XNEG;
        return true;
    else if (v.Op == OpNeg64F) 
        v.Op = OpS390XFNEG;
        return true;
    else if (v.Op == OpNeg8) 
        v.Op = OpS390XNEGW;
        return true;
    else if (v.Op == OpNeq16) 
        return rewriteValueS390X_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValueS390X_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValueS390X_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64) 
        return rewriteValueS390X_OpNeq64(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValueS390X_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValueS390X_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        return rewriteValueS390X_OpNeqB(_addr_v);
    else if (v.Op == OpNeqPtr) 
        return rewriteValueS390X_OpNeqPtr(_addr_v);
    else if (v.Op == OpNilCheck) 
        v.Op = OpS390XLoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        return rewriteValueS390X_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValueS390X_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        v.Op = OpS390XORW;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = OpS390XORW;
        return true;
    else if (v.Op == OpOr64) 
        v.Op = OpS390XOR;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = OpS390XORW;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = OpS390XORW;
        return true;
    else if (v.Op == OpPanicBounds) 
        return rewriteValueS390X_OpPanicBounds(_addr_v);
    else if (v.Op == OpPopCount16) 
        return rewriteValueS390X_OpPopCount16(_addr_v);
    else if (v.Op == OpPopCount32) 
        return rewriteValueS390X_OpPopCount32(_addr_v);
    else if (v.Op == OpPopCount64) 
        return rewriteValueS390X_OpPopCount64(_addr_v);
    else if (v.Op == OpPopCount8) 
        return rewriteValueS390X_OpPopCount8(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValueS390X_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        v.Op = OpS390XRLL;
        return true;
    else if (v.Op == OpRotateLeft64) 
        v.Op = OpS390XRLLG;
        return true;
    else if (v.Op == OpRotateLeft8) 
        return rewriteValueS390X_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound) 
        return rewriteValueS390X_OpRound(_addr_v);
    else if (v.Op == OpRound32F) 
        v.Op = OpS390XLoweredRound32F;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpS390XLoweredRound64F;
        return true;
    else if (v.Op == OpRoundToEven) 
        return rewriteValueS390X_OpRoundToEven(_addr_v);
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValueS390X_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValueS390X_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValueS390X_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValueS390X_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValueS390X_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValueS390X_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValueS390X_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValueS390X_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValueS390X_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValueS390X_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValueS390X_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValueS390X_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValueS390X_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValueS390X_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValueS390X_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValueS390X_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh64Ux16) 
        return rewriteValueS390X_OpRsh64Ux16(_addr_v);
    else if (v.Op == OpRsh64Ux32) 
        return rewriteValueS390X_OpRsh64Ux32(_addr_v);
    else if (v.Op == OpRsh64Ux64) 
        return rewriteValueS390X_OpRsh64Ux64(_addr_v);
    else if (v.Op == OpRsh64Ux8) 
        return rewriteValueS390X_OpRsh64Ux8(_addr_v);
    else if (v.Op == OpRsh64x16) 
        return rewriteValueS390X_OpRsh64x16(_addr_v);
    else if (v.Op == OpRsh64x32) 
        return rewriteValueS390X_OpRsh64x32(_addr_v);
    else if (v.Op == OpRsh64x64) 
        return rewriteValueS390X_OpRsh64x64(_addr_v);
    else if (v.Op == OpRsh64x8) 
        return rewriteValueS390X_OpRsh64x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValueS390X_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValueS390X_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValueS390X_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValueS390X_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValueS390X_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValueS390X_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValueS390X_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValueS390X_OpRsh8x8(_addr_v);
    else if (v.Op == OpS390XADD) 
        return rewriteValueS390X_OpS390XADD(_addr_v);
    else if (v.Op == OpS390XADDC) 
        return rewriteValueS390X_OpS390XADDC(_addr_v);
    else if (v.Op == OpS390XADDE) 
        return rewriteValueS390X_OpS390XADDE(_addr_v);
    else if (v.Op == OpS390XADDW) 
        return rewriteValueS390X_OpS390XADDW(_addr_v);
    else if (v.Op == OpS390XADDWconst) 
        return rewriteValueS390X_OpS390XADDWconst(_addr_v);
    else if (v.Op == OpS390XADDWload) 
        return rewriteValueS390X_OpS390XADDWload(_addr_v);
    else if (v.Op == OpS390XADDconst) 
        return rewriteValueS390X_OpS390XADDconst(_addr_v);
    else if (v.Op == OpS390XADDload) 
        return rewriteValueS390X_OpS390XADDload(_addr_v);
    else if (v.Op == OpS390XAND) 
        return rewriteValueS390X_OpS390XAND(_addr_v);
    else if (v.Op == OpS390XANDW) 
        return rewriteValueS390X_OpS390XANDW(_addr_v);
    else if (v.Op == OpS390XANDWconst) 
        return rewriteValueS390X_OpS390XANDWconst(_addr_v);
    else if (v.Op == OpS390XANDWload) 
        return rewriteValueS390X_OpS390XANDWload(_addr_v);
    else if (v.Op == OpS390XANDconst) 
        return rewriteValueS390X_OpS390XANDconst(_addr_v);
    else if (v.Op == OpS390XANDload) 
        return rewriteValueS390X_OpS390XANDload(_addr_v);
    else if (v.Op == OpS390XCMP) 
        return rewriteValueS390X_OpS390XCMP(_addr_v);
    else if (v.Op == OpS390XCMPU) 
        return rewriteValueS390X_OpS390XCMPU(_addr_v);
    else if (v.Op == OpS390XCMPUconst) 
        return rewriteValueS390X_OpS390XCMPUconst(_addr_v);
    else if (v.Op == OpS390XCMPW) 
        return rewriteValueS390X_OpS390XCMPW(_addr_v);
    else if (v.Op == OpS390XCMPWU) 
        return rewriteValueS390X_OpS390XCMPWU(_addr_v);
    else if (v.Op == OpS390XCMPWUconst) 
        return rewriteValueS390X_OpS390XCMPWUconst(_addr_v);
    else if (v.Op == OpS390XCMPWconst) 
        return rewriteValueS390X_OpS390XCMPWconst(_addr_v);
    else if (v.Op == OpS390XCMPconst) 
        return rewriteValueS390X_OpS390XCMPconst(_addr_v);
    else if (v.Op == OpS390XCPSDR) 
        return rewriteValueS390X_OpS390XCPSDR(_addr_v);
    else if (v.Op == OpS390XFCMP) 
        return rewriteValueS390X_OpS390XFCMP(_addr_v);
    else if (v.Op == OpS390XFCMPS) 
        return rewriteValueS390X_OpS390XFCMPS(_addr_v);
    else if (v.Op == OpS390XFMOVDload) 
        return rewriteValueS390X_OpS390XFMOVDload(_addr_v);
    else if (v.Op == OpS390XFMOVDstore) 
        return rewriteValueS390X_OpS390XFMOVDstore(_addr_v);
    else if (v.Op == OpS390XFMOVSload) 
        return rewriteValueS390X_OpS390XFMOVSload(_addr_v);
    else if (v.Op == OpS390XFMOVSstore) 
        return rewriteValueS390X_OpS390XFMOVSstore(_addr_v);
    else if (v.Op == OpS390XFNEG) 
        return rewriteValueS390X_OpS390XFNEG(_addr_v);
    else if (v.Op == OpS390XFNEGS) 
        return rewriteValueS390X_OpS390XFNEGS(_addr_v);
    else if (v.Op == OpS390XLDGR) 
        return rewriteValueS390X_OpS390XLDGR(_addr_v);
    else if (v.Op == OpS390XLEDBR) 
        return rewriteValueS390X_OpS390XLEDBR(_addr_v);
    else if (v.Op == OpS390XLGDR) 
        return rewriteValueS390X_OpS390XLGDR(_addr_v);
    else if (v.Op == OpS390XLOCGR) 
        return rewriteValueS390X_OpS390XLOCGR(_addr_v);
    else if (v.Op == OpS390XLTDBR) 
        return rewriteValueS390X_OpS390XLTDBR(_addr_v);
    else if (v.Op == OpS390XLTEBR) 
        return rewriteValueS390X_OpS390XLTEBR(_addr_v);
    else if (v.Op == OpS390XLoweredRound32F) 
        return rewriteValueS390X_OpS390XLoweredRound32F(_addr_v);
    else if (v.Op == OpS390XLoweredRound64F) 
        return rewriteValueS390X_OpS390XLoweredRound64F(_addr_v);
    else if (v.Op == OpS390XMOVBZload) 
        return rewriteValueS390X_OpS390XMOVBZload(_addr_v);
    else if (v.Op == OpS390XMOVBZreg) 
        return rewriteValueS390X_OpS390XMOVBZreg(_addr_v);
    else if (v.Op == OpS390XMOVBload) 
        return rewriteValueS390X_OpS390XMOVBload(_addr_v);
    else if (v.Op == OpS390XMOVBreg) 
        return rewriteValueS390X_OpS390XMOVBreg(_addr_v);
    else if (v.Op == OpS390XMOVBstore) 
        return rewriteValueS390X_OpS390XMOVBstore(_addr_v);
    else if (v.Op == OpS390XMOVBstoreconst) 
        return rewriteValueS390X_OpS390XMOVBstoreconst(_addr_v);
    else if (v.Op == OpS390XMOVDaddridx) 
        return rewriteValueS390X_OpS390XMOVDaddridx(_addr_v);
    else if (v.Op == OpS390XMOVDload) 
        return rewriteValueS390X_OpS390XMOVDload(_addr_v);
    else if (v.Op == OpS390XMOVDstore) 
        return rewriteValueS390X_OpS390XMOVDstore(_addr_v);
    else if (v.Op == OpS390XMOVDstoreconst) 
        return rewriteValueS390X_OpS390XMOVDstoreconst(_addr_v);
    else if (v.Op == OpS390XMOVHBRstore) 
        return rewriteValueS390X_OpS390XMOVHBRstore(_addr_v);
    else if (v.Op == OpS390XMOVHZload) 
        return rewriteValueS390X_OpS390XMOVHZload(_addr_v);
    else if (v.Op == OpS390XMOVHZreg) 
        return rewriteValueS390X_OpS390XMOVHZreg(_addr_v);
    else if (v.Op == OpS390XMOVHload) 
        return rewriteValueS390X_OpS390XMOVHload(_addr_v);
    else if (v.Op == OpS390XMOVHreg) 
        return rewriteValueS390X_OpS390XMOVHreg(_addr_v);
    else if (v.Op == OpS390XMOVHstore) 
        return rewriteValueS390X_OpS390XMOVHstore(_addr_v);
    else if (v.Op == OpS390XMOVHstoreconst) 
        return rewriteValueS390X_OpS390XMOVHstoreconst(_addr_v);
    else if (v.Op == OpS390XMOVWBRstore) 
        return rewriteValueS390X_OpS390XMOVWBRstore(_addr_v);
    else if (v.Op == OpS390XMOVWZload) 
        return rewriteValueS390X_OpS390XMOVWZload(_addr_v);
    else if (v.Op == OpS390XMOVWZreg) 
        return rewriteValueS390X_OpS390XMOVWZreg(_addr_v);
    else if (v.Op == OpS390XMOVWload) 
        return rewriteValueS390X_OpS390XMOVWload(_addr_v);
    else if (v.Op == OpS390XMOVWreg) 
        return rewriteValueS390X_OpS390XMOVWreg(_addr_v);
    else if (v.Op == OpS390XMOVWstore) 
        return rewriteValueS390X_OpS390XMOVWstore(_addr_v);
    else if (v.Op == OpS390XMOVWstoreconst) 
        return rewriteValueS390X_OpS390XMOVWstoreconst(_addr_v);
    else if (v.Op == OpS390XMULLD) 
        return rewriteValueS390X_OpS390XMULLD(_addr_v);
    else if (v.Op == OpS390XMULLDconst) 
        return rewriteValueS390X_OpS390XMULLDconst(_addr_v);
    else if (v.Op == OpS390XMULLDload) 
        return rewriteValueS390X_OpS390XMULLDload(_addr_v);
    else if (v.Op == OpS390XMULLW) 
        return rewriteValueS390X_OpS390XMULLW(_addr_v);
    else if (v.Op == OpS390XMULLWconst) 
        return rewriteValueS390X_OpS390XMULLWconst(_addr_v);
    else if (v.Op == OpS390XMULLWload) 
        return rewriteValueS390X_OpS390XMULLWload(_addr_v);
    else if (v.Op == OpS390XNEG) 
        return rewriteValueS390X_OpS390XNEG(_addr_v);
    else if (v.Op == OpS390XNEGW) 
        return rewriteValueS390X_OpS390XNEGW(_addr_v);
    else if (v.Op == OpS390XNOT) 
        return rewriteValueS390X_OpS390XNOT(_addr_v);
    else if (v.Op == OpS390XNOTW) 
        return rewriteValueS390X_OpS390XNOTW(_addr_v);
    else if (v.Op == OpS390XOR) 
        return rewriteValueS390X_OpS390XOR(_addr_v);
    else if (v.Op == OpS390XORW) 
        return rewriteValueS390X_OpS390XORW(_addr_v);
    else if (v.Op == OpS390XORWconst) 
        return rewriteValueS390X_OpS390XORWconst(_addr_v);
    else if (v.Op == OpS390XORWload) 
        return rewriteValueS390X_OpS390XORWload(_addr_v);
    else if (v.Op == OpS390XORconst) 
        return rewriteValueS390X_OpS390XORconst(_addr_v);
    else if (v.Op == OpS390XORload) 
        return rewriteValueS390X_OpS390XORload(_addr_v);
    else if (v.Op == OpS390XRISBGZ) 
        return rewriteValueS390X_OpS390XRISBGZ(_addr_v);
    else if (v.Op == OpS390XRLL) 
        return rewriteValueS390X_OpS390XRLL(_addr_v);
    else if (v.Op == OpS390XRLLG) 
        return rewriteValueS390X_OpS390XRLLG(_addr_v);
    else if (v.Op == OpS390XSLD) 
        return rewriteValueS390X_OpS390XSLD(_addr_v);
    else if (v.Op == OpS390XSLDconst) 
        return rewriteValueS390X_OpS390XSLDconst(_addr_v);
    else if (v.Op == OpS390XSLW) 
        return rewriteValueS390X_OpS390XSLW(_addr_v);
    else if (v.Op == OpS390XSLWconst) 
        return rewriteValueS390X_OpS390XSLWconst(_addr_v);
    else if (v.Op == OpS390XSRAD) 
        return rewriteValueS390X_OpS390XSRAD(_addr_v);
    else if (v.Op == OpS390XSRADconst) 
        return rewriteValueS390X_OpS390XSRADconst(_addr_v);
    else if (v.Op == OpS390XSRAW) 
        return rewriteValueS390X_OpS390XSRAW(_addr_v);
    else if (v.Op == OpS390XSRAWconst) 
        return rewriteValueS390X_OpS390XSRAWconst(_addr_v);
    else if (v.Op == OpS390XSRD) 
        return rewriteValueS390X_OpS390XSRD(_addr_v);
    else if (v.Op == OpS390XSRDconst) 
        return rewriteValueS390X_OpS390XSRDconst(_addr_v);
    else if (v.Op == OpS390XSRW) 
        return rewriteValueS390X_OpS390XSRW(_addr_v);
    else if (v.Op == OpS390XSRWconst) 
        return rewriteValueS390X_OpS390XSRWconst(_addr_v);
    else if (v.Op == OpS390XSTM2) 
        return rewriteValueS390X_OpS390XSTM2(_addr_v);
    else if (v.Op == OpS390XSTMG2) 
        return rewriteValueS390X_OpS390XSTMG2(_addr_v);
    else if (v.Op == OpS390XSUB) 
        return rewriteValueS390X_OpS390XSUB(_addr_v);
    else if (v.Op == OpS390XSUBE) 
        return rewriteValueS390X_OpS390XSUBE(_addr_v);
    else if (v.Op == OpS390XSUBW) 
        return rewriteValueS390X_OpS390XSUBW(_addr_v);
    else if (v.Op == OpS390XSUBWconst) 
        return rewriteValueS390X_OpS390XSUBWconst(_addr_v);
    else if (v.Op == OpS390XSUBWload) 
        return rewriteValueS390X_OpS390XSUBWload(_addr_v);
    else if (v.Op == OpS390XSUBconst) 
        return rewriteValueS390X_OpS390XSUBconst(_addr_v);
    else if (v.Op == OpS390XSUBload) 
        return rewriteValueS390X_OpS390XSUBload(_addr_v);
    else if (v.Op == OpS390XSumBytes2) 
        return rewriteValueS390X_OpS390XSumBytes2(_addr_v);
    else if (v.Op == OpS390XSumBytes4) 
        return rewriteValueS390X_OpS390XSumBytes4(_addr_v);
    else if (v.Op == OpS390XSumBytes8) 
        return rewriteValueS390X_OpS390XSumBytes8(_addr_v);
    else if (v.Op == OpS390XXOR) 
        return rewriteValueS390X_OpS390XXOR(_addr_v);
    else if (v.Op == OpS390XXORW) 
        return rewriteValueS390X_OpS390XXORW(_addr_v);
    else if (v.Op == OpS390XXORWconst) 
        return rewriteValueS390X_OpS390XXORWconst(_addr_v);
    else if (v.Op == OpS390XXORWload) 
        return rewriteValueS390X_OpS390XXORWload(_addr_v);
    else if (v.Op == OpS390XXORconst) 
        return rewriteValueS390X_OpS390XXORconst(_addr_v);
    else if (v.Op == OpS390XXORload) 
        return rewriteValueS390X_OpS390XXORload(_addr_v);
    else if (v.Op == OpSelect0) 
        return rewriteValueS390X_OpSelect0(_addr_v);
    else if (v.Op == OpSelect1) 
        return rewriteValueS390X_OpSelect1(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        v.Op = OpS390XMOVHreg;
        return true;
    else if (v.Op == OpSignExt16to64) 
        v.Op = OpS390XMOVHreg;
        return true;
    else if (v.Op == OpSignExt32to64) 
        v.Op = OpS390XMOVWreg;
        return true;
    else if (v.Op == OpSignExt8to16) 
        v.Op = OpS390XMOVBreg;
        return true;
    else if (v.Op == OpSignExt8to32) 
        v.Op = OpS390XMOVBreg;
        return true;
    else if (v.Op == OpSignExt8to64) 
        v.Op = OpS390XMOVBreg;
        return true;
    else if (v.Op == OpSlicemask) 
        return rewriteValueS390X_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = OpS390XFSQRT;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = OpS390XFSQRTS;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = OpS390XCALLstatic;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValueS390X_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = OpS390XSUBW;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = OpS390XSUBW;
        return true;
    else if (v.Op == OpSub32F) 
        return rewriteValueS390X_OpSub32F(_addr_v);
    else if (v.Op == OpSub64) 
        v.Op = OpS390XSUB;
        return true;
    else if (v.Op == OpSub64F) 
        return rewriteValueS390X_OpSub64F(_addr_v);
    else if (v.Op == OpSub8) 
        v.Op = OpS390XSUBW;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = OpS390XSUB;
        return true;
    else if (v.Op == OpTrunc) 
        return rewriteValueS390X_OpTrunc(_addr_v);
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
        v.Op = OpS390XLoweredWB;
        return true;
    else if (v.Op == OpXor16) 
        v.Op = OpS390XXORW;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = OpS390XXORW;
        return true;
    else if (v.Op == OpXor64) 
        v.Op = OpS390XXOR;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = OpS390XXORW;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValueS390X_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        v.Op = OpS390XMOVHZreg;
        return true;
    else if (v.Op == OpZeroExt16to64) 
        v.Op = OpS390XMOVHZreg;
        return true;
    else if (v.Op == OpZeroExt32to64) 
        v.Op = OpS390XMOVWZreg;
        return true;
    else if (v.Op == OpZeroExt8to16) 
        v.Op = OpS390XMOVBZreg;
        return true;
    else if (v.Op == OpZeroExt8to32) 
        v.Op = OpS390XMOVBZreg;
        return true;
    else if (v.Op == OpZeroExt8to64) 
        v.Op = OpS390XMOVBZreg;
        return true;
        return false;
}
private static bool rewriteValueS390X_OpAdd32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Add32F x y)
    // result: (Select0 (FADDS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpS390XFADDS, types.NewTuple(typ.Float32, types.TypeFlags));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAdd64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Add64F x y)
    // result: (Select0 (FADD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpS390XFADD, types.NewTuple(typ.Float64, types.TypeFlags));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (MOVDaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpS390XMOVDaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicAdd32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicAdd32 ptr val mem)
    // result: (AddTupleFirst32 val (LAA ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XAddTupleFirst32);
        var v0 = b.NewValue0(v.Pos, OpS390XLAA, types.NewTuple(typ.UInt32, types.TypeMem));
        v0.AddArg3(ptr, val, mem);
        v.AddArg2(val, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicAdd64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicAdd64 ptr val mem)
    // result: (AddTupleFirst64 val (LAAG ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XAddTupleFirst64);
        var v0 = b.NewValue0(v.Pos, OpS390XLAAG, types.NewTuple(typ.UInt64, types.TypeMem));
        v0.AddArg3(ptr, val, mem);
        v.AddArg2(val, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicAnd8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicAnd8 ptr val mem)
    // result: (LANfloor ptr (RLL <typ.UInt32> (ORWconst <typ.UInt32> val [-1<<8]) (RXSBG <typ.UInt32> {s390x.NewRotateParams(59, 60, 3)} (MOVDconst [3<<3]) ptr)) mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XLANfloor);
        var v0 = b.NewValue0(v.Pos, OpS390XRLL, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpS390XORWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(-1 << 8);
        v1.AddArg(val);
        var v2 = b.NewValue0(v.Pos, OpS390XRXSBG, typ.UInt32);
        v2.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(59, 60, 3));
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(3 << 3);
        v2.AddArg2(v3, ptr);
        v0.AddArg2(v1, v2);
        v.AddArg3(ptr, v0, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicCompareAndSwap32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicCompareAndSwap32 ptr old new_ mem)
    // result: (LoweredAtomicCas32 ptr old new_ mem)
    while (true) {
        var ptr = v_0;
        var old = v_1;
        var new_ = v_2;
        var mem = v_3;
        v.reset(OpS390XLoweredAtomicCas32);
        v.AddArg4(ptr, old, new_, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicCompareAndSwap64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicCompareAndSwap64 ptr old new_ mem)
    // result: (LoweredAtomicCas64 ptr old new_ mem)
    while (true) {
        var ptr = v_0;
        var old = v_1;
        var new_ = v_2;
        var mem = v_3;
        v.reset(OpS390XLoweredAtomicCas64);
        v.AddArg4(ptr, old, new_, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicExchange32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicExchange32 ptr val mem)
    // result: (LoweredAtomicExchange32 ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XLoweredAtomicExchange32);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicExchange64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicExchange64 ptr val mem)
    // result: (LoweredAtomicExchange64 ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XLoweredAtomicExchange64);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicLoad32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad32 ptr mem)
    // result: (MOVWZatomicload ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpS390XMOVWZatomicload);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicLoad64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad64 ptr mem)
    // result: (MOVDatomicload ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpS390XMOVDatomicload);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicLoad8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad8 ptr mem)
    // result: (MOVBZatomicload ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpS390XMOVBZatomicload);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicLoadAcq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoadAcq32 ptr mem)
    // result: (MOVWZatomicload ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpS390XMOVWZatomicload);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicLoadPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoadPtr ptr mem)
    // result: (MOVDatomicload ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpS390XMOVDatomicload);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicOr8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicOr8 ptr val mem)
    // result: (LAOfloor ptr (SLW <typ.UInt32> (MOVBZreg <typ.UInt32> val) (RXSBG <typ.UInt32> {s390x.NewRotateParams(59, 60, 3)} (MOVDconst [3<<3]) ptr)) mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XLAOfloor);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt32);
        v1.AddArg(val);
        var v2 = b.NewValue0(v.Pos, OpS390XRXSBG, typ.UInt32);
        v2.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(59, 60, 3));
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(3 << 3);
        v2.AddArg2(v3, ptr);
        v0.AddArg2(v1, v2);
        v.AddArg3(ptr, v0, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicStore32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AtomicStore32 ptr val mem)
    // result: (SYNC (MOVWatomicstore ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XSYNC);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVWatomicstore, types.TypeMem);
        v0.AddArg3(ptr, val, mem);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicStore64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AtomicStore64 ptr val mem)
    // result: (SYNC (MOVDatomicstore ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XSYNC);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDatomicstore, types.TypeMem);
        v0.AddArg3(ptr, val, mem);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicStore8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AtomicStore8 ptr val mem)
    // result: (SYNC (MOVBatomicstore ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XSYNC);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBatomicstore, types.TypeMem);
        v0.AddArg3(ptr, val, mem);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicStorePtrNoWB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AtomicStorePtrNoWB ptr val mem)
    // result: (SYNC (MOVDatomicstore ptr val mem))
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XSYNC);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDatomicstore, types.TypeMem);
        v0.AddArg3(ptr, val, mem);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpAtomicStoreRel32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStoreRel32 ptr val mem)
    // result: (MOVWatomicstore ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpS390XMOVWatomicstore);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValueS390X_OpAvg64u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Avg64u <t> x y)
    // result: (ADD (SRDconst <t> (SUB <t> x y) [1]) y)
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XADD);
        var v0 = b.NewValue0(v.Pos, OpS390XSRDconst, t);
        v0.AuxInt = uint8ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpS390XSUB, t);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpBitLen64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen64 x)
    // result: (SUB (MOVDconst [64]) (FLOGR x))
    while (true) {
        var x = v_0;
        v.reset(OpS390XSUB);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(64);
        var v1 = b.NewValue0(v.Pos, OpS390XFLOGR, typ.UInt64);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpCeil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Ceil x)
    // result: (FIDBR [6] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XFIDBR);
        v.AuxInt = int8ToAuxInt(6);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt16(v.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValueS390X_OpConst32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt32(v.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValueS390X_OpConst64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const64 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt64(v.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValueS390X_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt8(v.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValueS390X_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [t])
    // result: (MOVDconst [b2i(t)])
    while (true) {
        var t = auxIntToBool(v.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(b2i(t));
        return true;
    }
}
private static bool rewriteValueS390X_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (MOVDconst [0])
    while (true) {
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    }
}
private static bool rewriteValueS390X_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz32 <t> x)
    // result: (SUB (MOVDconst [64]) (FLOGR (MOVWZreg (ANDW <t> (SUBWconst <t> [1] x) (NOTW <t> x)))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpS390XSUB);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(64);
        var v1 = b.NewValue0(v.Pos, OpS390XFLOGR, typ.UInt64);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVWZreg, typ.UInt64);
        var v3 = b.NewValue0(v.Pos, OpS390XANDW, t);
        var v4 = b.NewValue0(v.Pos, OpS390XSUBWconst, t);
        v4.AuxInt = int32ToAuxInt(1);
        v4.AddArg(x);
        var v5 = b.NewValue0(v.Pos, OpS390XNOTW, t);
        v5.AddArg(x);
        v3.AddArg2(v4, v5);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpCtz64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz64 <t> x)
    // result: (SUB (MOVDconst [64]) (FLOGR (AND <t> (SUBconst <t> [1] x) (NOT <t> x))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpS390XSUB);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(64);
        var v1 = b.NewValue0(v.Pos, OpS390XFLOGR, typ.UInt64);
        var v2 = b.NewValue0(v.Pos, OpS390XAND, t);
        var v3 = b.NewValue0(v.Pos, OpS390XSUBconst, t);
        v3.AuxInt = int32ToAuxInt(1);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XNOT, t);
        v4.AddArg(x);
        v2.AddArg2(v3, v4);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 x y)
    // result: (DIVW (MOVHreg x) (MOVHreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u x y)
    // result: (DIVWU (MOVHZreg x) (MOVHZreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32 x y)
    // result: (DIVW (MOVWreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVWreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32u x y)
    // result: (DIVWU (MOVWZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVWZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Div64 x y)
    // result: (DIVD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVD);
        v.AddArg2(x, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (DIVW (MOVBreg x) (MOVBreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (DIVWU (MOVBZreg x) (MOVBZreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XDIVWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVHreg x) (MOVHreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq32 x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq32F x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (FCMPS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMPS, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq64 x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq64F x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (FCMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqB x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqPtr x y)
    // result: (LOCGR {s390x.Equal} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Equal);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpFMA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMA x y z)
    // result: (FMADD z x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        var z = v_2;
        v.reset(OpS390XFMADD);
        v.AddArg3(z, x, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpFloor(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Floor x)
    // result: (FIDBR [7] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XFIDBR);
        v.AuxInt = int8ToAuxInt(7);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpHmul32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Hmul32 x y)
    // result: (SRDconst [32] (MULLD (MOVWreg x) (MOVWreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XSRDconst);
        v.AuxInt = uint8ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpS390XMULLD, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVWreg, typ.Int64);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVWreg, typ.Int64);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpHmul32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Hmul32u x y)
    // result: (SRDconst [32] (MULLD (MOVWZreg x) (MOVWZreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XSRDconst);
        v.AuxInt = uint8ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpS390XMULLD, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVWZreg, typ.UInt64);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVWZreg, typ.UInt64);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpITab(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ITab (Load ptr mem))
    // result: (MOVDload ptr mem)
    while (true) {
        if (v_0.Op != OpLoad) {
            break;
        }
        var mem = v_0.Args[1];
        var ptr = v_0.Args[0];
        v.reset(OpS390XMOVDload);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsInBounds idx len)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPU idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPU, types.TypeFlags);
        v2.AddArg2(idx, len);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsNonNil p)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPconst p [0]))
    while (true) {
        var p = v_0;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg(p);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsSliceInBounds idx len)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPU idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPU, types.TypeFlags);
        v2.AddArg2(idx, len);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16 x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVHreg x) (MOVHreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16U x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPWU (MOVHZreg x) (MOVHZreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32 x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32F x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (FCMPS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMPS, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32U x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPWU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq64 x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq64F x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (FCMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq64U x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPU, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8 x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8U x y)
    // result: (LOCGR {s390x.LessOrEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPWU (MOVBZreg x) (MOVBZreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16 x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVHreg x) (MOVHreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16U x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPWU (MOVHZreg x) (MOVHZreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less32 x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less32F x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (FCMPS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMPS, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less32U x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPWU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less64 x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less64F x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (FCMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less64U x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPU, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8 x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8U x y)
    // result: (LOCGR {s390x.Less} (MOVDconst [0]) (MOVDconst [1]) (CMPWU (MOVBZreg x) (MOVBZreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.Less);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Load <t> ptr mem)
    // cond: (is64BitInt(t) || isPtr(t))
    // result: (MOVDload ptr mem)
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(is64BitInt(t) || isPtr(t))) {
            break;
        }
        v.reset(OpS390XMOVDload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitInt(t) && isSigned(t)
    // result: (MOVWload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpS390XMOVWload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitInt(t) && !isSigned(t)
    // result: (MOVWZload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpS390XMOVWZload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is16BitInt(t) && isSigned(t)
    // result: (MOVHload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpS390XMOVHload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is16BitInt(t) && !isSigned(t)
    // result: (MOVHZload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpS390XMOVHZload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is8BitInt(t) && isSigned(t)
    // result: (MOVBload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpS390XMOVBload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: (t.IsBoolean() || (is8BitInt(t) && !isSigned(t)))
    // result: (MOVBZload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsBoolean() || (is8BitInt(t) && !isSigned(t)))) {
            break;
        }
        v.reset(OpS390XMOVBZload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitFloat(t)
    // result: (FMOVSload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(OpS390XFMOVSload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitFloat(t)
    // result: (FMOVDload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(OpS390XFMOVDload);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (MOVDaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpS390XMOVDaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLD <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLD <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLD <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLD <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SLW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSLW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16 x y)
    // result: (MODW (MOVHreg x) (MOVHreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16u x y)
    // result: (MODWU (MOVHZreg x) (MOVHZreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32 x y)
    // result: (MODW (MOVWreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVWreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32u x y)
    // result: (MODWU (MOVWZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVWZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Mod64 x y)
    // result: (MODD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODD);
        v.AddArg2(x, y);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (MODW (MOVBreg x) (MOVBreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (MODWU (MOVBZreg x) (MOVBZreg y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XMODWU);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpMove(ptr<Value> _addr_v) {
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
    // result: (MOVBstore dst (MOVBZload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var dst = v_0;
        var src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVBstore);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZload, typ.UInt8);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [2] dst src mem)
    // result: (MOVHstore dst (MOVHZload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVHstore);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHZload, typ.UInt16);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [4] dst src mem)
    // result: (MOVWstore dst (MOVWZload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVWstore);
        v0 = b.NewValue0(v.Pos, OpS390XMOVWZload, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [8] dst src mem)
    // result: (MOVDstore dst (MOVDload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVDstore);
        v0 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [16] dst src mem)
    // result: (MOVDstore [8] dst (MOVDload [8] src mem) (MOVDstore dst (MOVDload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(8);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDstore, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [24] dst src mem)
    // result: (MOVDstore [16] dst (MOVDload [16] src mem) (MOVDstore [8] dst (MOVDload [8] src mem) (MOVDstore dst (MOVDload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 24) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(16);
        v0 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpS390XMOVDstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(8);
        v2 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v2.AuxInt = int32ToAuxInt(8);
        v2.AddArg2(src, mem);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDstore, types.TypeMem);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVDload, typ.UInt64);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [3] dst src mem)
    // result: (MOVBstore [2] dst (MOVBZload [2] src mem) (MOVHstore dst (MOVHZload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpS390XMOVHstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpS390XMOVHZload, typ.UInt16);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [5] dst src mem)
    // result: (MOVBstore [4] dst (MOVBZload [4] src mem) (MOVWstore dst (MOVWZload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpS390XMOVWstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpS390XMOVWZload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [6] dst src mem)
    // result: (MOVHstore [4] dst (MOVHZload [4] src mem) (MOVWstore dst (MOVWZload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHZload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpS390XMOVWstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpS390XMOVWZload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [7] dst src mem)
    // result: (MOVBstore [6] dst (MOVBZload [6] src mem) (MOVHstore [4] dst (MOVHZload [4] src mem) (MOVWstore dst (MOVWZload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(6);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(6);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpS390XMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(4);
        v2 = b.NewValue0(v.Pos, OpS390XMOVHZload, typ.UInt16);
        v2.AuxInt = int32ToAuxInt(4);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpS390XMOVWstore, types.TypeMem);
        v4 = b.NewValue0(v.Pos, OpS390XMOVWZload, typ.UInt32);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 0 && s <= 256 && logLargeCopy(v, s)
    // result: (MVC [makeValAndOff(int32(s), 0)] dst src mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 0 && s <= 256 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpS390XMVC);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(s), 0));
        v.AddArg3(dst, src, mem);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 256 && s <= 512 && logLargeCopy(v, s)
    // result: (MVC [makeValAndOff(int32(s)-256, 256)] dst src (MVC [makeValAndOff(256, 0)] dst src mem))
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 256 && s <= 512 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpS390XMVC);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(s) - 256, 256));
        v0 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 0));
        v0.AddArg3(dst, src, mem);
        v.AddArg3(dst, src, v0);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 512 && s <= 768 && logLargeCopy(v, s)
    // result: (MVC [makeValAndOff(int32(s)-512, 512)] dst src (MVC [makeValAndOff(256, 256)] dst src (MVC [makeValAndOff(256, 0)] dst src mem)))
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 512 && s <= 768 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpS390XMVC);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(s) - 512, 512));
        v0 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 256));
        v1 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v1.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 0));
        v1.AddArg3(dst, src, mem);
        v0.AddArg3(dst, src, v1);
        v.AddArg3(dst, src, v0);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 768 && s <= 1024 && logLargeCopy(v, s)
    // result: (MVC [makeValAndOff(int32(s)-768, 768)] dst src (MVC [makeValAndOff(256, 512)] dst src (MVC [makeValAndOff(256, 256)] dst src (MVC [makeValAndOff(256, 0)] dst src mem))))
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 768 && s <= 1024 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpS390XMVC);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(s) - 768, 768));
        v0 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 512));
        v1 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v1.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 256));
        v2 = b.NewValue0(v.Pos, OpS390XMVC, types.TypeMem);
        v2.AuxInt = valAndOffToAuxInt(makeValAndOff(256, 0));
        v2.AddArg3(dst, src, mem);
        v1.AddArg3(dst, src, v2);
        v0.AddArg3(dst, src, v1);
        v.AddArg3(dst, src, v0);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 1024 && logLargeCopy(v, s)
    // result: (LoweredMove [s%256] dst src (ADD <src.Type> src (MOVDconst [(s/256)*256])) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 1024 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpS390XLoweredMove);
        v.AuxInt = int64ToAuxInt(s % 256);
        v0 = b.NewValue0(v.Pos, OpS390XADD, src.Type);
        v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt((s / 256) * 256);
        v0.AddArg2(src, v1);
        v.AddArg4(dst, src, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVHreg x) (MOVHreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq32 x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq32F x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (FCMPS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMPS, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq64 x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq64F x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (FCMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XFCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NeqB x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMPW (MOVBreg x) (MOVBreg y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v4.AddArg(y);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NeqPtr x y)
    // result: (LOCGR {s390x.NotEqual} (MOVDconst [0]) (MOVDconst [1]) (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(s390x.NotEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v2.AddArg2(x, y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not x)
    // result: (XORWconst [1] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XXORWconst);
        v.AuxInt = int32ToAuxInt(1);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (OffPtr [off] ptr:(SP))
    // result: (MOVDaddr [int32(off)] ptr)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        if (ptr.Op != OpSP) {
            break;
        }
        v.reset(OpS390XMOVDaddr);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;
    } 
    // match: (OffPtr [off] ptr)
    // cond: is32Bit(off)
    // result: (ADDconst [int32(off)] ptr)
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        if (!(is32Bit(off))) {
            break;
        }
        v.reset(OpS390XADDconst);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;
    } 
    // match: (OffPtr [off] ptr)
    // result: (ADD (MOVDconst [off]) ptr)
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        v.reset(OpS390XADD);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(off);
        v.AddArg2(v0, ptr);
        return true;
    }
}
private static bool rewriteValueS390X_OpPanicBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 0
    // result: (LoweredPanicBoundsA [kind] x y mem)
    while (true) {
        var kind = auxIntToInt64(v.AuxInt);
        var x = v_0;
        var y = v_1;
        var mem = v_2;
        if (!(boundsABI(kind) == 0)) {
            break;
        }
        v.reset(OpS390XLoweredPanicBoundsA);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    } 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 1
    // result: (LoweredPanicBoundsB [kind] x y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        x = v_0;
        y = v_1;
        mem = v_2;
        if (!(boundsABI(kind) == 1)) {
            break;
        }
        v.reset(OpS390XLoweredPanicBoundsB);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    } 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 2
    // result: (LoweredPanicBoundsC [kind] x y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        x = v_0;
        y = v_1;
        mem = v_2;
        if (!(boundsABI(kind) == 2)) {
            break;
        }
        v.reset(OpS390XLoweredPanicBoundsC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpPopCount16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount16 x)
    // result: (MOVBZreg (SumBytes2 (POPCNT <typ.UInt16> x)))
    while (true) {
        var x = v_0;
        v.reset(OpS390XMOVBZreg);
        var v0 = b.NewValue0(v.Pos, OpS390XSumBytes2, typ.UInt8);
        var v1 = b.NewValue0(v.Pos, OpS390XPOPCNT, typ.UInt16);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpPopCount32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount32 x)
    // result: (MOVBZreg (SumBytes4 (POPCNT <typ.UInt32> x)))
    while (true) {
        var x = v_0;
        v.reset(OpS390XMOVBZreg);
        var v0 = b.NewValue0(v.Pos, OpS390XSumBytes4, typ.UInt8);
        var v1 = b.NewValue0(v.Pos, OpS390XPOPCNT, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpPopCount64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount64 x)
    // result: (MOVBZreg (SumBytes8 (POPCNT <typ.UInt64> x)))
    while (true) {
        var x = v_0;
        v.reset(OpS390XMOVBZreg);
        var v0 = b.NewValue0(v.Pos, OpS390XSumBytes8, typ.UInt8);
        var v1 = b.NewValue0(v.Pos, OpS390XPOPCNT, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpPopCount8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount8 x)
    // result: (POPCNT (MOVBZreg x))
    while (true) {
        var x = v_0;
        v.reset(OpS390XPOPCNT);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft16 <t> x (MOVDconst [c]))
    // result: (Or16 (Lsh16x64 <t> x (MOVDconst [c&15])) (Rsh16Ux64 <t> x (MOVDconst [-c&15])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr16);
        var v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(c & 15);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(-c & 15);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft8 <t> x (MOVDconst [c]))
    // result: (Or8 (Lsh8x64 <t> x (MOVDconst [c&7])) (Rsh8Ux64 <t> x (MOVDconst [-c&7])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr8);
        var v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(c & 7);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(-c & 7);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpRound(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Round x)
    // result: (FIDBR [1] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XFIDBR);
        v.AuxInt = int8ToAuxInt(1);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpRoundToEven(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (RoundToEven x)
    // result: (FIDBR [4] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XFIDBR);
        v.AuxInt = int8ToAuxInt(4);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVHZreg x) y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVHZreg x) y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVHZreg x) y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVHZreg x) y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x16 x y)
    // result: (SRAW (MOVHreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVHZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x32 x y)
    // result: (SRAW (MOVHreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x64 x y)
    // result: (SRAW (MOVHreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x8 x y)
    // result: (SRAW (MOVHreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVBZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVHreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x16 x y)
    // result: (SRAW x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVHZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x32 x y)
    // result: (SRAW x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x64 x y)
    // result: (SRAW x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x8 x y)
    // result: (SRAW x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVBZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRD <t> x y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRD <t> x y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRD <t> x y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRD <t> x y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v0 = b.NewValue0(v.Pos, OpS390XSRD, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v.AddArg3(v0, v1, v2);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x16 x y)
    // result: (SRAD x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVHZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAD);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x32 x y)
    // result: (SRAD x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAD);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x64 x y)
    // result: (SRAD x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAD);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        v2.AddArg(y);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh64x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x8 x y)
    // result: (SRAD x (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVBZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAD);
        var v0 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v0.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v1.AuxInt = int64ToAuxInt(63);
        var v2 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(64);
        var v3 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v3.AddArg(y);
        v2.AddArg(v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux16 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVBZreg x) y) (MOVDconst [0]) (CMPWUconst (MOVHZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux32 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVBZreg x) y) (MOVDconst [0]) (CMPWUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux64 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVBZreg x) y) (MOVDconst [0]) (CMPUconst y [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux8 <t> x y)
    // result: (LOCGR {s390x.GreaterOrEqual} <t> (SRW <t> (MOVBZreg x) y) (MOVDconst [0]) (CMPWUconst (MOVBZreg y) [64]))
    while (true) {
        var t = v.Type;
        x = v_0;
        y = v_1;
        v.reset(OpS390XLOCGR);
        v.Type = t;
        v.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        v0 = b.NewValue0(v.Pos, OpS390XSRW, t);
        var v1 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v.AddArg3(v0, v2, v3);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x16 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x16 x y)
    // result: (SRAW (MOVBreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVHZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVHZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x32 x y)
    // result: (SRAW (MOVBreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x64 x y)
    // result: (SRAW (MOVBreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPUconst y [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x8 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpS390XSRAW);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x8 x y)
    // result: (SRAW (MOVBreg x) (LOCGR {s390x.GreaterOrEqual} <y.Type> y (MOVDconst <y.Type> [63]) (CMPWUconst (MOVBZreg y) [64])))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpS390XSRAW);
        v0 = b.NewValue0(v.Pos, OpS390XMOVBreg, typ.Int64);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XLOCGR, y.Type);
        v1.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
        var v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, y.Type);
        v2.AuxInt = int64ToAuxInt(63);
        var v3 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(64);
        var v4 = b.NewValue0(v.Pos, OpS390XMOVBZreg, typ.UInt64);
        v4.AddArg(y);
        v3.AddArg(v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XADD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADD x (MOVDconst [c]))
    // cond: is32Bit(c)
    // result: (ADDconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c))) {
                    continue;
                }
                v.reset(OpS390XADDconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLDconst x [c]) (SRDconst x [64-c]))
    // result: (RISBGZ x {s390x.NewRotateParams(0, 63, c)})
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 64 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRISBGZ);
                v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(0, 63, c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD idx (MOVDaddr [c] {s} ptr))
    // cond: ptr.Op != OpSB
    // result: (MOVDaddridx [c] {s} ptr idx)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var idx = v_0;
                if (v_1.Op != OpS390XMOVDaddr) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_1.AuxInt);
                var s = auxToSym(v_1.Aux);
                var ptr = v_1.Args[0];
                if (!(ptr.Op != OpSB)) {
                    continue;
                }
                v.reset(OpS390XMOVDaddridx);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(ptr, idx);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD x (NEG y))
    // result: (SUB x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpS390XNEG) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_1.Args[0];
                v.reset(OpS390XSUB);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ADDload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XADDload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDC(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDC x (MOVDconst [c]))
    // cond: is16Bit(c)
    // result: (ADDCconst x [int16(c)])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is16Bit(c))) {
                    continue;
                }
                v.reset(OpS390XADDCconst);
                v.AuxInt = int16ToAuxInt(int16(c));
                v.AddArg(x);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDE x y (FlagEQ))
    // result: (ADDC x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpS390XFlagEQ) {
            break;
        }
        v.reset(OpS390XADDC);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDE x y (FlagLT))
    // result: (ADDC x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpS390XFlagLT) {
            break;
        }
        v.reset(OpS390XADDC);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDE x y (Select1 (ADDCconst [-1] (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) c)))))
    // result: (ADDE x y c)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpSelect1) {
            break;
        }
        var v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpS390XADDCconst || auxIntToInt16(v_2_0.AuxInt) != -1) {
            break;
        }
        var v_2_0_0 = v_2_0.Args[0];
        if (v_2_0_0.Op != OpSelect0) {
            break;
        }
        var v_2_0_0_0 = v_2_0_0.Args[0];
        if (v_2_0_0_0.Op != OpS390XADDE) {
            break;
        }
        var c = v_2_0_0_0.Args[2];
        var v_2_0_0_0_0 = v_2_0_0_0.Args[0];
        if (v_2_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_2_0_0_0_0.AuxInt) != 0) {
            break;
        }
        var v_2_0_0_0_1 = v_2_0_0_0.Args[1];
        if (v_2_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_2_0_0_0_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpS390XADDE);
        v.AddArg3(x, y, c);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDW x (MOVDconst [c]))
    // result: (ADDWconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XADDWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDW (SLWconst x [c]) (SRWconst x [32-c]))
    // result: (RLLconst x [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRWconst || auxIntToUint8(v_1.AuxInt) != 32 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRLLconst);
                v.AuxInt = uint8ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDW x (NEGW y))
    // result: (SUBW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpS390XNEGW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_1.Args[0];
                v.reset(OpS390XSUBW);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ADDWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVWload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XADDWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ADDWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                g = v_1;
                if (g.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off = auxIntToInt32(g.AuxInt);
                sym = auxToSym(g.Aux);
                mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XADDWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDWconst [c] x)
    // cond: int32(c)==0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(int32(c) == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ADDWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)+d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) + d);
        return true;
    } 
    // match: (ADDWconst [c] (ADDWconst [d] x))
    // result: (ADDWconst [int32(c+d)] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XADDWconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpS390XADDWconst);
        v.AuxInt = int32ToAuxInt(int32(c + d));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ADDWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XADDWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ADDWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ADDWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XADDWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDconst [c] (MOVDaddr [d] {s} x:(SB)))
    // cond: ((c+d)&1 == 0) && is32Bit(int64(c)+int64(d))
    // result: (MOVDaddr [c+d] {s} x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var s = auxToSym(v_0.Aux);
        var x = v_0.Args[0];
        if (x.Op != OpSB || !(((c + d) & 1 == 0) && is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XMOVDaddr);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDconst [c] (MOVDaddr [d] {s} x))
    // cond: x.Op != OpSB && is20Bit(int64(c)+int64(d))
    // result: (MOVDaddr [c+d] {s} x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        if (!(x.Op != OpSB && is20Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XMOVDaddr);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDconst [c] (MOVDaddridx [d] {s} x y))
    // cond: is20Bit(int64(c)+int64(d))
    // result: (MOVDaddridx [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDaddridx) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        var y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is20Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XMOVDaddridx);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (ADDconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)+d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) + d);
        return true;
    } 
    // match: (ADDconst [c] (ADDconst [d] x))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (ADDconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XADDconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XADDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (ADD x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XADD);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (ADDload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ADDload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XADDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ADDload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ADDload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XADDload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XAND(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AND x (MOVDconst [c]))
    // cond: s390x.NewRotateParams(0, 63, 0).OutMerge(uint64(c)) != nil
    // result: (RISBGZ x {*s390x.NewRotateParams(0, 63, 0).OutMerge(uint64(c))})
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(s390x.NewRotateParams(0, 63, 0).OutMerge(uint64(c)) != null)) {
                    continue;
                }
                v.reset(OpS390XRISBGZ);
                v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(0, 63, 0).OutMerge(uint64(c)).val);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND x (MOVDconst [c]))
    // cond: is32Bit(c) && c < 0
    // result: (ANDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c) && c < 0)) {
                    continue;
                }
                v.reset(OpS390XANDconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND x (MOVDconst [c]))
    // cond: is32Bit(c) && c >= 0
    // result: (MOVWZreg (ANDWconst <typ.UInt32> [int32(c)] x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c) && c >= 0)) {
                    continue;
                }
                v.reset(OpS390XMOVWZreg);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c));
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [c&d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XMOVDconst);
                v.AuxInt = int64ToAuxInt(c & d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (AND <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ANDload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XANDload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XANDW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ANDW x (MOVDconst [c]))
    // result: (ANDWconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XANDWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ANDW x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ANDW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ANDWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVWload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XANDWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ANDW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ANDWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                g = v_1;
                if (g.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off = auxIntToInt32(g.AuxInt);
                sym = auxToSym(g.Aux);
                mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XANDWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XANDWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDWconst [c] (ANDWconst [d] x))
    // result: (ANDWconst [c&d] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpS390XANDWconst);
        v.AuxInt = int32ToAuxInt(c & d);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDWconst [0x00ff] x)
    // result: (MOVBZreg x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0x00ff) {
            break;
        }
        x = v_0;
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDWconst [0xffff] x)
    // result: (MOVHZreg x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0xffff) {
            break;
        }
        x = v_0;
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDWconst [c] _)
    // cond: int32(c)==0
    // result: (MOVDconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (!(int32(c) == 0)) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ANDWconst [c] x)
    // cond: int32(c)==-1
    // result: x
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(int32(c) == -1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ANDWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)&d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) & d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XANDWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ANDWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ANDWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XANDWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ANDWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ANDWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XANDWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XANDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDconst [c] (ANDconst [d] x))
    // result: (ANDconst [c&d] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpS390XANDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpS390XANDconst);
        v.AuxInt = int64ToAuxInt(c & d);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [0] _)
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ANDconst [-1] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != -1) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVDconst [d]))
    // result: (MOVDconst [c&d])
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(c & d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XANDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (AND x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XAND);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (ANDload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ANDload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XANDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ANDload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ANDload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XANDload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMP(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMP x (MOVDconst [c]))
    // cond: is32Bit(c)
    // result: (CMPconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(is32Bit(c))) {
            break;
        }
        v.reset(OpS390XCMPconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMP (MOVDconst [c]) x)
    // cond: is32Bit(c)
    // result: (InvertFlags (CMPconst x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        if (!(is32Bit(c))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMP x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMP y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        v0 = b.NewValue0(v.Pos, OpS390XCMP, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPU x (MOVDconst [c]))
    // cond: isU32Bit(c)
    // result: (CMPUconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(isU32Bit(c))) {
            break;
        }
        v.reset(OpS390XCMPUconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPU (MOVDconst [c]) x)
    // cond: isU32Bit(c)
    // result: (InvertFlags (CMPUconst x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        if (!(isU32Bit(c))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XCMPUconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPU x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPU y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        v0 = b.NewValue0(v.Pos, OpS390XCMPU, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPUconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: uint64(x)==uint64(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(uint64(x) == uint64(y))) {
            break;
        }
        v.reset(OpS390XFlagEQ);
        return true;
    } 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: uint64(x)<uint64(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint64(x) < uint64(y))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: uint64(x)>uint64(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint64(x) > uint64(y))) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPUconst (SRDconst _ [c]) [n])
    // cond: c > 0 && c < 64 && (1<<uint(64-c)) <= uint64(n)
    // result: (FlagLT)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XSRDconst) {
            break;
        }
        var c = auxIntToUint8(v_0.AuxInt);
        if (!(c > 0 && c < 64 && (1 << (int)(uint(64 - c))) <= uint64(n))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPUconst (RISBGZ x {r}) [c])
    // cond: r.OutMask() < uint64(uint32(c))
    // result: (FlagLT)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        if (!(r.OutMask() < uint64(uint32(c)))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPUconst (MOVWZreg x) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst x:(MOVHreg _) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVHreg) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst x:(MOVHZreg _) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVHZreg) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst x:(MOVBreg _) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVBreg) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst x:(MOVBZreg _) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVBZreg) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst (MOVWZreg x:(ANDWconst [m] _)) [c])
    // cond: int32(m) >= 0
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(x.AuxInt);
        if (!(int32(m) >= 0)) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPUconst (MOVWreg x:(ANDWconst [m] _)) [c])
    // cond: int32(m) >= 0
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XANDWconst) {
            break;
        }
        m = auxIntToInt32(x.AuxInt);
        if (!(int32(m) >= 0)) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPW x (MOVDconst [c]))
    // result: (CMPWconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPW (MOVDconst [c]) x)
    // result: (InvertFlags (CMPWconst x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XCMPWconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPW x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPW y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        v0 = b.NewValue0(v.Pos, OpS390XCMPW, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPW x (MOVWreg y))
    // result: (CMPW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XCMPW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPW x (MOVWZreg y))
    // result: (CMPW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XCMPW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPW (MOVWreg x) y)
    // result: (CMPW x y)
    while (true) {
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpS390XCMPW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPW (MOVWZreg x) y)
    // result: (CMPW x y)
    while (true) {
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpS390XCMPW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPWU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPWU x (MOVDconst [c]))
    // result: (CMPWUconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWU (MOVDconst [c]) x)
    // result: (InvertFlags (CMPWUconst x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XCMPWUconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPWU x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPWU y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpS390XInvertFlags);
        v0 = b.NewValue0(v.Pos, OpS390XCMPWU, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPWU x (MOVWreg y))
    // result: (CMPWU x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XCMPWU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWU x (MOVWZreg y))
    // result: (CMPWU x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XCMPWU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWU (MOVWreg x) y)
    // result: (CMPWU x y)
    while (true) {
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpS390XCMPWU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWU (MOVWZreg x) y)
    // result: (CMPWU x y)
    while (true) {
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpS390XCMPWU);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPWUconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: uint32(x)==uint32(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(uint32(x) == uint32(y))) {
            break;
        }
        v.reset(OpS390XFlagEQ);
        return true;
    } 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: uint32(x)<uint32(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint32(x) < uint32(y))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: uint32(x)>uint32(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint32(x) > uint32(y))) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPWUconst (MOVBZreg _) [c])
    // cond: 0xff < c
    // result: (FlagLT)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVBZreg || !(0xff < c)) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWUconst (MOVHZreg _) [c])
    // cond: 0xffff < c
    // result: (FlagLT)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVHZreg || !(0xffff < c)) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWUconst (SRWconst _ [c]) [n])
    // cond: c > 0 && c < 32 && (1<<uint(32-c)) <= uint32(n)
    // result: (FlagLT)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XSRWconst) {
            break;
        }
        c = auxIntToUint8(v_0.AuxInt);
        if (!(c > 0 && c < 32 && (1 << (int)(uint(32 - c))) <= uint32(n))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWUconst (ANDWconst _ [m]) [n])
    // cond: uint32(m) < uint32(n)
    // result: (FlagLT)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(uint32(m) < uint32(n))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWUconst (MOVWreg x) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWUconst (MOVWZreg x) [c])
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)==int32(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) == int32(y))) {
            break;
        }
        v.reset(OpS390XFlagEQ);
        return true;
    } 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)<int32(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) < int32(y))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)>int32(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) > int32(y))) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPWconst (MOVBZreg _) [c])
    // cond: 0xff < c
    // result: (FlagLT)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVBZreg || !(0xff < c)) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWconst (MOVHZreg _) [c])
    // cond: 0xffff < c
    // result: (FlagLT)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVHZreg || !(0xffff < c)) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWconst (SRWconst _ [c]) [n])
    // cond: c > 0 && n < 0
    // result: (FlagGT)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XSRWconst) {
            break;
        }
        c = auxIntToUint8(v_0.AuxInt);
        if (!(c > 0 && n < 0)) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPWconst (ANDWconst _ [m]) [n])
    // cond: int32(m) >= 0 && int32(m) < int32(n)
    // result: (FlagLT)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(int32(m) >= 0 && int32(m) < int32(n))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPWconst x:(SRWconst _ [c]) [n])
    // cond: c > 0 && n >= 0
    // result: (CMPWUconst x [n])
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XSRWconst) {
            break;
        }
        c = auxIntToUint8(x.AuxInt);
        if (!(c > 0 && n >= 0)) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(n);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWconst (MOVWreg x) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWconst (MOVWZreg x) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCMPconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x==int64(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(x == int64(y))) {
            break;
        }
        v.reset(OpS390XFlagEQ);
        return true;
    } 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x<int64(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(x < int64(y))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x>int64(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(x > int64(y))) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPconst (SRDconst _ [c]) [n])
    // cond: c > 0 && n < 0
    // result: (FlagGT)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XSRDconst) {
            break;
        }
        var c = auxIntToUint8(v_0.AuxInt);
        if (!(c > 0 && n < 0)) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (CMPconst (RISBGZ x {r}) [c])
    // cond: c > 0 && r.OutMask() < uint64(c)
    // result: (FlagLT)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        if (!(c > 0 && r.OutMask() < uint64(c))) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (CMPconst (MOVWreg x) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst x:(MOVHreg _) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVHreg) {
            break;
        }
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst x:(MOVHZreg _) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVHZreg) {
            break;
        }
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst x:(MOVBreg _) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVBreg) {
            break;
        }
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst x:(MOVBZreg _) [c])
    // result: (CMPWconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XMOVBZreg) {
            break;
        }
        v.reset(OpS390XCMPWconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst (MOVWZreg x:(ANDWconst [m] _)) [c])
    // cond: int32(m) >= 0 && c >= 0
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(x.AuxInt);
        if (!(int32(m) >= 0 && c >= 0)) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst (MOVWreg x:(ANDWconst [m] _)) [c])
    // cond: int32(m) >= 0 && c >= 0
    // result: (CMPWUconst x [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVWreg) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XANDWconst) {
            break;
        }
        m = auxIntToInt32(x.AuxInt);
        if (!(int32(m) >= 0 && c >= 0)) {
            break;
        }
        v.reset(OpS390XCMPWUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPconst x:(SRDconst _ [c]) [n])
    // cond: c > 0 && n >= 0
    // result: (CMPUconst x [n])
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpS390XSRDconst) {
            break;
        }
        c = auxIntToUint8(x.AuxInt);
        if (!(c > 0 && n >= 0)) {
            break;
        }
        v.reset(OpS390XCMPUconst);
        v.AuxInt = int32ToAuxInt(n);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XCPSDR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CPSDR y (FMOVDconst [c]))
    // cond: !math.Signbit(c)
    // result: (LPDFR y)
    while (true) {
        var y = v_0;
        if (v_1.Op != OpS390XFMOVDconst) {
            break;
        }
        var c = auxIntToFloat64(v_1.AuxInt);
        if (!(!math.Signbit(c))) {
            break;
        }
        v.reset(OpS390XLPDFR);
        v.AddArg(y);
        return true;
    } 
    // match: (CPSDR y (FMOVDconst [c]))
    // cond: math.Signbit(c)
    // result: (LNDFR y)
    while (true) {
        y = v_0;
        if (v_1.Op != OpS390XFMOVDconst) {
            break;
        }
        c = auxIntToFloat64(v_1.AuxInt);
        if (!(math.Signbit(c))) {
            break;
        }
        v.reset(OpS390XLNDFR);
        v.AddArg(y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFCMP(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (FCMP x (FMOVDconst [0.0]))
    // result: (LTDBR x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XFMOVDconst || auxIntToFloat64(v_1.AuxInt) != 0.0F) {
            break;
        }
        v.reset(OpS390XLTDBR);
        v.AddArg(x);
        return true;
    } 
    // match: (FCMP (FMOVDconst [0.0]) x)
    // result: (InvertFlags (LTDBR <v.Type> x))
    while (true) {
        if (v_0.Op != OpS390XFMOVDconst || auxIntToFloat64(v_0.AuxInt) != 0.0F) {
            break;
        }
        x = v_1;
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XLTDBR, v.Type);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFCMPS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (FCMPS x (FMOVSconst [0.0]))
    // result: (LTEBR x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XFMOVSconst || auxIntToFloat32(v_1.AuxInt) != 0.0F) {
            break;
        }
        v.reset(OpS390XLTEBR);
        v.AddArg(x);
        return true;
    } 
    // match: (FCMPS (FMOVSconst [0.0]) x)
    // result: (InvertFlags (LTEBR <v.Type> x))
    while (true) {
        if (v_0.Op != OpS390XFMOVSconst || auxIntToFloat32(v_0.AuxInt) != 0.0F) {
            break;
        }
        x = v_1;
        v.reset(OpS390XInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpS390XLTEBR, v.Type);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFMOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVDload [off] {sym} ptr1 (MOVDstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (LDGR x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XLDGR);
        v.AddArg(x);
        return true;
    } 
    // match: (FMOVDload [off] {sym} ptr1 (FMOVDstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: x
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr1 = v_0;
        if (v_1.Op != OpS390XFMOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        x = v_1.Args[1];
        ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (FMOVDload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (FMOVDload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XFMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (FMOVDload [off1] {sym1} (MOVDaddr [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (FMOVDload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XFMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFMOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVDstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (FMOVDstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XFMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (FMOVDstore [off1] {sym1} (MOVDaddr [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (FMOVDstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XFMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFMOVSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVSload [off] {sym} ptr1 (FMOVSstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XFMOVSstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (FMOVSload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (FMOVSload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XFMOVSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (FMOVSload [off1] {sym1} (MOVDaddr [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (FMOVSload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XFMOVSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFMOVSstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVSstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (FMOVSstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XFMOVSstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (FMOVSstore [off1] {sym1} (MOVDaddr [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (FMOVSstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XFMOVSstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFNEG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FNEG (LPDFR x))
    // result: (LNDFR x)
    while (true) {
        if (v_0.Op != OpS390XLPDFR) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpS390XLNDFR);
        v.AddArg(x);
        return true;
    } 
    // match: (FNEG (LNDFR x))
    // result: (LPDFR x)
    while (true) {
        if (v_0.Op != OpS390XLNDFR) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XLPDFR);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XFNEGS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FNEGS (LPDFR x))
    // result: (LNDFR x)
    while (true) {
        if (v_0.Op != OpS390XLPDFR) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpS390XLNDFR);
        v.AddArg(x);
        return true;
    } 
    // match: (FNEGS (LNDFR x))
    // result: (LPDFR x)
    while (true) {
        if (v_0.Op != OpS390XLNDFR) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpS390XLPDFR);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLDGR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (LDGR <t> (RISBGZ x {r}))
    // cond: r == s390x.NewRotateParams(1, 63, 0)
    // result: (LPDFR (LDGR <t> x))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        var x = v_0.Args[0];
        if (!(r == s390x.NewRotateParams(1, 63, 0))) {
            break;
        }
        v.reset(OpS390XLPDFR);
        var v0 = b.NewValue0(v.Pos, OpS390XLDGR, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (LDGR <t> (OR (MOVDconst [-1<<63]) x))
    // result: (LNDFR (LDGR <t> x))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpS390XOR) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0.AuxInt) != -1 << 63) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                x = v_0_1;
                v.reset(OpS390XLNDFR);
                v0 = b.NewValue0(v.Pos, OpS390XLDGR, t);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        break;
    } 
    // match: (LDGR <t> x:(ORload <t1> [off] {sym} (MOVDconst [-1<<63]) ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (LNDFR <t> (LDGR <t> (MOVDload <t1> [off] {sym} ptr mem)))
    while (true) {
        t = v.Type;
        x = v_0;
        if (x.Op != OpS390XORload) {
            break;
        }
        var t1 = x.Type;
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[2];
        var x_0 = x.Args[0];
        if (x_0.Op != OpS390XMOVDconst || auxIntToInt64(x_0.AuxInt) != -1 << 63) {
            break;
        }
        var ptr = x.Args[1];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        v0 = b.NewValue0(x.Pos, OpS390XLNDFR, t);
        v.copyOf(v0);
        var v1 = b.NewValue0(x.Pos, OpS390XLDGR, t);
        var v2 = b.NewValue0(x.Pos, OpS390XMOVDload, t1);
        v2.AuxInt = int32ToAuxInt(off);
        v2.Aux = symToAux(sym);
        v2.AddArg2(ptr, mem);
        v1.AddArg(v2);
        v0.AddArg(v1);
        return true;
    } 
    // match: (LDGR (LGDR x))
    // result: x
    while (true) {
        if (v_0.Op != OpS390XLGDR) {
            break;
        }
        x = v_0.Args[0];
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLEDBR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LEDBR (LPDFR (LDEBR x)))
    // result: (LPDFR x)
    while (true) {
        if (v_0.Op != OpS390XLPDFR) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XLDEBR) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpS390XLPDFR);
        v.AddArg(x);
        return true;
    } 
    // match: (LEDBR (LNDFR (LDEBR x)))
    // result: (LNDFR x)
    while (true) {
        if (v_0.Op != OpS390XLNDFR) {
            break;
        }
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XLDEBR) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpS390XLNDFR);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLGDR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LGDR (LDGR x))
    // result: x
    while (true) {
        if (v_0.Op != OpS390XLDGR) {
            break;
        }
        var x = v_0.Args[0];
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLOCGR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LOCGR {c} x y (InvertFlags cmp))
    // result: (LOCGR {c.ReverseComparison()} x y cmp)
    while (true) {
        var c = auxToS390xCCMask(v.Aux);
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpS390XInvertFlags) {
            break;
        }
        var cmp = v_2.Args[0];
        v.reset(OpS390XLOCGR);
        v.Aux = s390xCCMaskToAux(c.ReverseComparison());
        v.AddArg3(x, y, cmp);
        return true;
    } 
    // match: (LOCGR {c} _ x (FlagEQ))
    // cond: c&s390x.Equal != 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_1;
        if (v_2.Op != OpS390XFlagEQ || !(c & s390x.Equal != 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} _ x (FlagLT))
    // cond: c&s390x.Less != 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_1;
        if (v_2.Op != OpS390XFlagLT || !(c & s390x.Less != 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} _ x (FlagGT))
    // cond: c&s390x.Greater != 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_1;
        if (v_2.Op != OpS390XFlagGT || !(c & s390x.Greater != 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} _ x (FlagOV))
    // cond: c&s390x.Unordered != 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_1;
        if (v_2.Op != OpS390XFlagOV || !(c & s390x.Unordered != 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} x _ (FlagEQ))
    // cond: c&s390x.Equal == 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_0;
        if (v_2.Op != OpS390XFlagEQ || !(c & s390x.Equal == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} x _ (FlagLT))
    // cond: c&s390x.Less == 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_0;
        if (v_2.Op != OpS390XFlagLT || !(c & s390x.Less == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} x _ (FlagGT))
    // cond: c&s390x.Greater == 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_0;
        if (v_2.Op != OpS390XFlagGT || !(c & s390x.Greater == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (LOCGR {c} x _ (FlagOV))
    // cond: c&s390x.Unordered == 0
    // result: x
    while (true) {
        c = auxToS390xCCMask(v.Aux);
        x = v_0;
        if (v_2.Op != OpS390XFlagOV || !(c & s390x.Unordered == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLTDBR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (LTDBR (Select0 x:(FADD _ _)))
    // cond: b == x.Block
    // result: (Select1 x)
    while (true) {
        if (v_0.Op != OpSelect0) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpS390XFADD || !(b == x.Block)) {
            break;
        }
        v.reset(OpSelect1);
        v.AddArg(x);
        return true;
    } 
    // match: (LTDBR (Select0 x:(FSUB _ _)))
    // cond: b == x.Block
    // result: (Select1 x)
    while (true) {
        if (v_0.Op != OpSelect0) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XFSUB || !(b == x.Block)) {
            break;
        }
        v.reset(OpSelect1);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLTEBR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (LTEBR (Select0 x:(FADDS _ _)))
    // cond: b == x.Block
    // result: (Select1 x)
    while (true) {
        if (v_0.Op != OpSelect0) {
            break;
        }
        var x = v_0.Args[0];
        if (x.Op != OpS390XFADDS || !(b == x.Block)) {
            break;
        }
        v.reset(OpSelect1);
        v.AddArg(x);
        return true;
    } 
    // match: (LTEBR (Select0 x:(FSUBS _ _)))
    // cond: b == x.Block
    // result: (Select1 x)
    while (true) {
        if (v_0.Op != OpSelect0) {
            break;
        }
        x = v_0.Args[0];
        if (x.Op != OpS390XFSUBS || !(b == x.Block)) {
            break;
        }
        v.reset(OpSelect1);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLoweredRound32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LoweredRound32F x:(FMOVSconst))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpS390XFMOVSconst) {
            break;
        }
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XLoweredRound64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LoweredRound64F x:(FMOVDconst))
    // result: x
    while (true) {
        var x = v_0;
        if (x.Op != OpS390XFMOVDconst) {
            break;
        }
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBZload [off] {sym} ptr1 (MOVBstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVBZreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVBstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVBZload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVBZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBZload [off1] {sym1} (MOVDaddr [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (MOVBZload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XMOVBZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBZreg e:(MOVBreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg e:(MOVHreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg e:(MOVBZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVBZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg e:(MOVHZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg x:(MOVBZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBZload || !(!x.Type.IsSigned() || x.Type.Size() > 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg <t> x:(MOVBload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBZload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVBload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVBZload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVBZreg x:(Arg <t>))
    // cond: !t.IsSigned() && t.Size() == 1
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(!t.IsSigned() && t.Size() == 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg (MOVDconst [c]))
    // result: (MOVDconst [int64( uint8(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint8(c)));
        return true;
    } 
    // match: (MOVBZreg x:(LOCGR (MOVDconst [c]) (MOVDconst [d]) _))
    // cond: int64(uint8(c)) == c && int64(uint8(d)) == d && (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XLOCGR) {
            break;
        }
        _ = x.Args[1];
        var x_0 = x.Args[0];
        if (x_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(x_0.AuxInt);
        var x_1 = x.Args[1];
        if (x_1.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(x_1.AuxInt);
        if (!(int64(uint8(c)) == c && int64(uint8(d)) == d && (!x.Type.IsSigned() || x.Type.Size() > 1))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg (RISBGZ x {r}))
    // cond: r.OutMerge(0x000000ff) != nil
    // result: (RISBGZ x {*r.OutMerge(0x000000ff)})
    while (true) {
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(r.OutMerge(0x000000ff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.OutMerge>>(0x000000ff));
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg (ANDWconst [m] x))
    // result: (MOVWZreg (ANDWconst <typ.UInt32> [int32( uint8(m))] x))
    while (true) {
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpS390XMOVWZreg);
        v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(uint8(m)));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBload [off] {sym} ptr1 (MOVBstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVBreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVBstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVBload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBload [off1] {sym1} (MOVDaddr [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBreg e:(MOVBreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg e:(MOVHreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg e:(MOVBZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVBZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg e:(MOVHZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg x:(MOVBload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBreg <t> x:(MOVBZload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVBZload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVBload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVBreg x:(Arg <t>))
    // cond: t.IsSigned() && t.Size() == 1
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(t.IsSigned() && t.Size() == 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBreg (MOVDconst [c]))
    // result: (MOVDconst [int64( int8(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int8(c)));
        return true;
    } 
    // match: (MOVBreg (ANDWconst [m] x))
    // cond: int8(m) >= 0
    // result: (MOVWZreg (ANDWconst <typ.UInt32> [int32( uint8(m))] x))
    while (true) {
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(int8(m) >= 0)) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(uint8(m)));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBZreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVBstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVDconst [c]) mem)
    // cond: is20Bit(int64(off)) && ptr.Op != OpSB
    // result: (MOVBstoreconst [makeValAndOff(int32(int8(c)),off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        mem = v_2;
        if (!(is20Bit(int64(off)) && ptr.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(int8(c)), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstore [off1] {sym1} (MOVDaddr [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpS390XMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w x:(MOVBstore [i-1] {s} p (SRDconst [8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHstore [i-1] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        var w = v_1;
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRDconst || auxIntToUint8(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w0:(SRDconst [j] w) x:(MOVBstore [i-1] {s} p (SRDconst [j+8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHstore [i-1] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w0 = v_1;
        if (w0.Op != OpS390XSRDconst) {
            break;
        }
        var j = auxIntToUint8(w0.AuxInt);
        w = w0.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRDconst || auxIntToUint8(x_1.AuxInt) != j + 8 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w x:(MOVBstore [i-1] {s} p (SRWconst [8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHstore [i-1] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRWconst || auxIntToUint8(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w0:(SRWconst [j] w) x:(MOVBstore [i-1] {s} p (SRWconst [j+8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHstore [i-1] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w0 = v_1;
        if (w0.Op != OpS390XSRWconst) {
            break;
        }
        j = auxIntToUint8(w0.AuxInt);
        w = w0.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRWconst || auxIntToUint8(x_1.AuxInt) != j + 8 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SRDconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHBRstore [i-1] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHBRstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SRDconst [j] w) x:(MOVBstore [i-1] {s} p w0:(SRDconst [j-8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHBRstore [i-1] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRDconst) {
            break;
        }
        j = auxIntToUint8(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        if (w0.Op != OpS390XSRDconst || auxIntToUint8(w0.AuxInt) != j - 8 || w != w0.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHBRstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SRWconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHBRstore [i-1] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRWconst || auxIntToUint8(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHBRstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SRWconst [j] w) x:(MOVBstore [i-1] {s} p w0:(SRWconst [j-8] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVHBRstore [i-1] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRWconst) {
            break;
        }
        j = auxIntToUint8(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        if (w0.Op != OpS390XSRWconst || auxIntToUint8(w0.AuxInt) != j - 8 || w != w0.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHBRstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVBstore [7] {s} p1 (SRDconst w) x1:(MOVHBRstore [5] {s} p1 (SRDconst w) x2:(MOVWBRstore [1] {s} p1 (SRDconst w) x3:(MOVBstore [0] {s} p1 w mem))))
    // cond: x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && clobber(x1, x2, x3)
    // result: (MOVDBRstore {s} p1 w mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 7) {
            break;
        }
        s = auxToSym(v.Aux);
        var p1 = v_0;
        if (v_1.Op != OpS390XSRDconst) {
            break;
        }
        w = v_1.Args[0];
        var x1 = v_2;
        if (x1.Op != OpS390XMOVHBRstore || auxIntToInt32(x1.AuxInt) != 5 || auxToSym(x1.Aux) != s) {
            break;
        }
        _ = x1.Args[2];
        if (p1 != x1.Args[0]) {
            break;
        }
        var x1_1 = x1.Args[1];
        if (x1_1.Op != OpS390XSRDconst || w != x1_1.Args[0]) {
            break;
        }
        var x2 = x1.Args[2];
        if (x2.Op != OpS390XMOVWBRstore || auxIntToInt32(x2.AuxInt) != 1 || auxToSym(x2.Aux) != s) {
            break;
        }
        _ = x2.Args[2];
        if (p1 != x2.Args[0]) {
            break;
        }
        var x2_1 = x2.Args[1];
        if (x2_1.Op != OpS390XSRDconst || w != x2_1.Args[0]) {
            break;
        }
        var x3 = x2.Args[2];
        if (x3.Op != OpS390XMOVBstore || auxIntToInt32(x3.AuxInt) != 0 || auxToSym(x3.Aux) != s) {
            break;
        }
        mem = x3.Args[2];
        if (p1 != x3.Args[0] || w != x3.Args[1] || !(x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && clobber(x1, x2, x3))) {
            break;
        }
        v.reset(OpS390XMOVDBRstore);
        v.Aux = symToAux(s);
        v.AddArg3(p1, w, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVBstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstoreconst [sc] {s} (ADDconst [off] ptr) mem)
    // cond: is20Bit(sc.Off64()+int64(off))
    // result: (MOVBstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(sc.Off64() + int64(off)))) {
            break;
        }
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstoreconst [sc] {sym1} (MOVDaddr [off] {sym2} ptr) mem)
    // cond: ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off)
    // result: (MOVBstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off))) {
            break;
        }
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstoreconst [c] {s} p x:(MOVBstoreconst [a] {s} p mem))
    // cond: p.Op != OpSB && x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x)
    // result: (MOVHstoreconst [makeValAndOff(c.Val()&0xff | a.Val()<<8, a.Off())] {s} p mem)
    while (true) {
        var c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p = v_0;
        var x = v_1;
        if (x.Op != OpS390XMOVBstoreconst) {
            break;
        }
        var a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(p.Op != OpSB && x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c.Val() & 0xff | a.Val() << 8, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVDaddridx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDaddridx [c] {s} (ADDconst [d] x) y)
    // cond: is20Bit(int64(c)+int64(d))
    // result: (MOVDaddridx [c+d] {s} x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var y = v_1;
        if (!(is20Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XMOVDaddridx);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (MOVDaddridx [c] {s} x (ADDconst [d] y))
    // cond: is20Bit(int64(c)+int64(d))
    // result: (MOVDaddridx [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(is20Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(OpS390XMOVDaddridx);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (MOVDaddridx [off1] {sym1} (MOVDaddr [off2] {sym2} x) y)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB
    // result: (MOVDaddridx [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        y = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVDaddridx);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (MOVDaddridx [off1] {sym1} x (MOVDaddr [off2] {sym2} y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && y.Op != OpSB
    // result: (MOVDaddridx [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        y = v_1.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && y.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVDaddridx);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDload [off] {sym} ptr1 (MOVDstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVDload [off] {sym} ptr1 (FMOVDstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (LGDR x)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr1 = v_0;
        if (v_1.Op != OpS390XFMOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        x = v_1.Args[1];
        ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XLGDR);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVDload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVDload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDload [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%8 == 0 && (off1+off2)%8 == 0))
    // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 8 == 0 && (off1 + off2) % 8 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVDstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVDstore [off] {sym} ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c) && isU12Bit(int64(off)) && ptr.Op != OpSB
    // result: (MOVDstoreconst [makeValAndOff(int32(c),off)] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        mem = v_2;
        if (!(is16Bit(c) && isU12Bit(int64(off)) && ptr.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVDstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(c), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDstore [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%8 == 0 && (off1+off2)%8 == 0))
    // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 8 == 0 && (off1 + off2) % 8 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVDstore [i] {s} p w1 x:(MOVDstore [i-8] {s} p w0 mem))
    // cond: p.Op != OpSB && x.Uses == 1 && is20Bit(int64(i)-8) && clobber(x)
    // result: (STMG2 [i-8] {s} p w0 w1 mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        var w1 = v_1;
        var x = v_2;
        if (x.Op != OpS390XMOVDstore || auxIntToInt32(x.AuxInt) != i - 8 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        if (!(p.Op != OpSB && x.Uses == 1 && is20Bit(int64(i) - 8) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTMG2);
        v.AuxInt = int32ToAuxInt(i - 8);
        v.Aux = symToAux(s);
        v.AddArg4(p, w0, w1, mem);
        return true;
    } 
    // match: (MOVDstore [i] {s} p w2 x:(STMG2 [i-16] {s} p w0 w1 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-16) && clobber(x)
    // result: (STMG3 [i-16] {s} p w0 w1 w2 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w2 = v_1;
        x = v_2;
        if (x.Op != OpS390XSTMG2 || auxIntToInt32(x.AuxInt) != i - 16 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[3];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        w1 = x.Args[2];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 16) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTMG3);
        v.AuxInt = int32ToAuxInt(i - 16);
        v.Aux = symToAux(s);
        v.AddArg5(p, w0, w1, w2, mem);
        return true;
    } 
    // match: (MOVDstore [i] {s} p w3 x:(STMG3 [i-24] {s} p w0 w1 w2 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-24) && clobber(x)
    // result: (STMG4 [i-24] {s} p w0 w1 w2 w3 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w3 = v_1;
        x = v_2;
        if (x.Op != OpS390XSTMG3 || auxIntToInt32(x.AuxInt) != i - 24 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[4];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        w1 = x.Args[2];
        w2 = x.Args[3];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 24) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTMG4);
        v.AuxInt = int32ToAuxInt(i - 24);
        v.Aux = symToAux(s);
        v.AddArg6(p, w0, w1, w2, w3, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVDstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstoreconst [sc] {s} (ADDconst [off] ptr) mem)
    // cond: isU12Bit(sc.Off64()+int64(off))
    // result: (MOVDstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU12Bit(sc.Off64() + int64(off)))) {
            break;
        }
        v.reset(OpS390XMOVDstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDstoreconst [sc] {sym1} (MOVDaddr [off] {sym2} ptr) mem)
    // cond: ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off)
    // result: (MOVDstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off))) {
            break;
        }
        v.reset(OpS390XMOVDstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHBRstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHBRstore [i] {s} p (SRDconst [16] w) x:(MOVHBRstore [i-2] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWBRstore [i-2] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 16) {
            break;
        }
        var w = v_1.Args[0];
        var x = v_2;
        if (x.Op != OpS390XMOVHBRstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        var mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWBRstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVHBRstore [i] {s} p (SRDconst [j] w) x:(MOVHBRstore [i-2] {s} p w0:(SRDconst [j-16] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWBRstore [i-2] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRDconst) {
            break;
        }
        var j = auxIntToUint8(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVHBRstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        if (w0.Op != OpS390XSRDconst || auxIntToUint8(w0.AuxInt) != j - 16 || w != w0.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWBRstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVHBRstore [i] {s} p (SRWconst [16] w) x:(MOVHBRstore [i-2] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWBRstore [i-2] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRWconst || auxIntToUint8(v_1.AuxInt) != 16) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVHBRstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWBRstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVHBRstore [i] {s} p (SRWconst [j] w) x:(MOVHBRstore [i-2] {s} p w0:(SRWconst [j-16] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWBRstore [i-2] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRWconst) {
            break;
        }
        j = auxIntToUint8(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVHBRstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        if (w0.Op != OpS390XSRWconst || auxIntToUint8(w0.AuxInt) != j - 16 || w != w0.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWBRstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHZload [off] {sym} ptr1 (MOVHstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVHZreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVHstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVHZload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVHZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHZload [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%2 == 0 && (off1+off2)%2 == 0))
    // result: (MOVHZload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 2 == 0 && (off1 + off2) % 2 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVHZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVHZreg e:(MOVBZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBZreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg e:(MOVHreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg e:(MOVHZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg x:(MOVBZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBZload || !(!x.Type.IsSigned() || x.Type.Size() > 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg x:(MOVHZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 2)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVHZload || !(!x.Type.IsSigned() || x.Type.Size() > 2)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg <t> x:(MOVHload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVHZload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVHload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVHZload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVHZreg x:(Arg <t>))
    // cond: !t.IsSigned() && t.Size() <= 2
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(!t.IsSigned() && t.Size() <= 2)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg (MOVDconst [c]))
    // result: (MOVDconst [int64(uint16(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint16(c)));
        return true;
    } 
    // match: (MOVHZreg (RISBGZ x {r}))
    // cond: r.OutMerge(0x0000ffff) != nil
    // result: (RISBGZ x {*r.OutMerge(0x0000ffff)})
    while (true) {
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(r.OutMerge(0x0000ffff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.OutMerge>>(0x0000ffff));
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg (ANDWconst [m] x))
    // result: (MOVWZreg (ANDWconst <typ.UInt32> [int32(uint16(m))] x))
    while (true) {
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpS390XMOVWZreg);
        v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(uint16(m)));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHload [off] {sym} ptr1 (MOVHstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVHreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVHstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVHload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHload [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%2 == 0 && (off1+off2)%2 == 0))
    // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 2 == 0 && (off1 + off2) % 2 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVHreg e:(MOVBreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg e:(MOVHreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg e:(MOVHZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg x:(MOVBload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg x:(MOVHload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVHload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg x:(MOVBZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBZload || !(!x.Type.IsSigned() || x.Type.Size() > 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg <t> x:(MOVHZload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVHload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVHZload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVHload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVHreg x:(Arg <t>))
    // cond: t.IsSigned() && t.Size() <= 2
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(t.IsSigned() && t.Size() <= 2)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg (MOVDconst [c]))
    // result: (MOVDconst [int64(int16(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int16(c)));
        return true;
    } 
    // match: (MOVHreg (ANDWconst [m] x))
    // cond: int16(m) >= 0
    // result: (MOVWZreg (ANDWconst <typ.UInt32> [int32(uint16(m))] x))
    while (true) {
        if (v_0.Op != OpS390XANDWconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(int16(m) >= 0)) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(uint16(m)));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHZreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVHstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVDconst [c]) mem)
    // cond: isU12Bit(int64(off)) && ptr.Op != OpSB
    // result: (MOVHstoreconst [makeValAndOff(int32(int16(c)),off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        mem = v_2;
        if (!(isU12Bit(int64(off)) && ptr.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(int16(c)), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHstore [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%2 == 0 && (off1+off2)%2 == 0))
    // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 2 == 0 && (off1 + off2) % 2 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVHstore [i] {s} p w x:(MOVHstore [i-2] {s} p (SRDconst [16] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-2] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        var w = v_1;
        x = v_2;
        if (x.Op != OpS390XMOVHstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRDconst || auxIntToUint8(x_1.AuxInt) != 16 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVHstore [i] {s} p w0:(SRDconst [j] w) x:(MOVHstore [i-2] {s} p (SRDconst [j+16] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-2] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w0 = v_1;
        if (w0.Op != OpS390XSRDconst) {
            break;
        }
        var j = auxIntToUint8(w0.AuxInt);
        w = w0.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVHstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRDconst || auxIntToUint8(x_1.AuxInt) != j + 16 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVHstore [i] {s} p w x:(MOVHstore [i-2] {s} p (SRWconst [16] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-2] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != OpS390XMOVHstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRWconst || auxIntToUint8(x_1.AuxInt) != 16 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVHstore [i] {s} p w0:(SRWconst [j] w) x:(MOVHstore [i-2] {s} p (SRWconst [j+16] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-2] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w0 = v_1;
        if (w0.Op != OpS390XSRWconst) {
            break;
        }
        j = auxIntToUint8(w0.AuxInt);
        w = w0.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVHstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRWconst || auxIntToUint8(x_1.AuxInt) != j + 16 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVHstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVHstoreconst [sc] {s} (ADDconst [off] ptr) mem)
    // cond: isU12Bit(sc.Off64()+int64(off))
    // result: (MOVHstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU12Bit(sc.Off64() + int64(off)))) {
            break;
        }
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHstoreconst [sc] {sym1} (MOVDaddr [off] {sym2} ptr) mem)
    // cond: ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off)
    // result: (MOVHstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off))) {
            break;
        }
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHstoreconst [c] {s} p x:(MOVHstoreconst [a] {s} p mem))
    // cond: p.Op != OpSB && x.Uses == 1 && a.Off() + 2 == c.Off() && clobber(x)
    // result: (MOVWstore [a.Off()] {s} p (MOVDconst [int64(c.Val()&0xffff | a.Val()<<16)]) mem)
    while (true) {
        var c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p = v_0;
        var x = v_1;
        if (x.Op != OpS390XMOVHstoreconst) {
            break;
        }
        var a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(p.Op != OpSB && x.Uses == 1 && a.Off() + 2 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(a.Off());
        v.Aux = symToAux(s);
        var v0 = b.NewValue0(x.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(int64(c.Val() & 0xffff | a.Val() << 16));
        v.AddArg3(p, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWBRstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWBRstore [i] {s} p (SRDconst [32] w) x:(MOVWBRstore [i-4] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVDBRstore [i-4] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 32) {
            break;
        }
        var w = v_1.Args[0];
        var x = v_2;
        if (x.Op != OpS390XMOVWBRstore || auxIntToInt32(x.AuxInt) != i - 4 || auxToSym(x.Aux) != s) {
            break;
        }
        var mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVDBRstore);
        v.AuxInt = int32ToAuxInt(i - 4);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVWBRstore [i] {s} p (SRDconst [j] w) x:(MOVWBRstore [i-4] {s} p w0:(SRDconst [j-32] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVDBRstore [i-4] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRDconst) {
            break;
        }
        var j = auxIntToUint8(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVWBRstore || auxIntToInt32(x.AuxInt) != i - 4 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        if (w0.Op != OpS390XSRDconst || auxIntToUint8(w0.AuxInt) != j - 32 || w != w0.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVDBRstore);
        v.AuxInt = int32ToAuxInt(i - 4);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWZload [off] {sym} ptr1 (MOVWstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVWZreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVWstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVWZload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVWZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWZload [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%4 == 0 && (off1+off2)%4 == 0))
    // result: (MOVWZload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 4 == 0 && (off1 + off2) % 4 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVWZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVWZreg e:(MOVBZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBZreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBZreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg e:(MOVHZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVWZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVWZreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVBZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBZload || !(!x.Type.IsSigned() || x.Type.Size() > 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVHZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 2)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVHZload || !(!x.Type.IsSigned() || x.Type.Size() > 2)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVWZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 4)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVWZload || !(!x.Type.IsSigned() || x.Type.Size() > 4)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg <t> x:(MOVWload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVWZload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVWload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVWZload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVWZreg x:(Arg <t>))
    // cond: !t.IsSigned() && t.Size() <= 4
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(!t.IsSigned() && t.Size() <= 4)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg (MOVDconst [c]))
    // result: (MOVDconst [int64(uint32(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint32(c)));
        return true;
    } 
    // match: (MOVWZreg (RISBGZ x {r}))
    // cond: r.OutMerge(0xffffffff) != nil
    // result: (RISBGZ x {*r.OutMerge(0xffffffff)})
    while (true) {
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(r.OutMerge(0xffffffff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.OutMerge>>(0xffffffff));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWload [off] {sym} ptr1 (MOVWstore [off] {sym} ptr2 x _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MOVWreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr1 = v_0;
        if (v_1.Op != OpS390XMOVWstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMOVWreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVWload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWload [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%4 == 0 && (off1+off2)%4 == 0))
    // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 4 == 0 && (off1 + off2) % 4 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVWreg e:(MOVBreg x))
    // cond: clobberIfDead(e)
    // result: (MOVBreg x)
    while (true) {
        var e = v_0;
        if (e.Op != OpS390XMOVBreg) {
            break;
        }
        var x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg e:(MOVHreg x))
    // cond: clobberIfDead(e)
    // result: (MOVHreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVHreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg e:(MOVWreg x))
    // cond: clobberIfDead(e)
    // result: (MOVWreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVWreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg e:(MOVWZreg x))
    // cond: clobberIfDead(e)
    // result: (MOVWreg x)
    while (true) {
        e = v_0;
        if (e.Op != OpS390XMOVWZreg) {
            break;
        }
        x = e.Args[0];
        if (!(clobberIfDead(e))) {
            break;
        }
        v.reset(OpS390XMOVWreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVBload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVHload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVHload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVWload _ _))
    // cond: (x.Type.IsSigned() || x.Type.Size() == 8)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVWload || !(x.Type.IsSigned() || x.Type.Size() == 8)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVBZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 1)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVBZload || !(!x.Type.IsSigned() || x.Type.Size() > 1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVHZload _ _))
    // cond: (!x.Type.IsSigned() || x.Type.Size() > 2)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpS390XMOVHZload || !(!x.Type.IsSigned() || x.Type.Size() > 2)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg <t> x:(MOVWZload [o] {s} p mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVWload <t> [o] {s} p mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpS390XMOVWZload) {
            break;
        }
        var o = auxIntToInt32(x.AuxInt);
        var s = auxToSym(x.Aux);
        var mem = x.Args[1];
        var p = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpS390XMOVWload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(o);
        v0.Aux = symToAux(s);
        v0.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVWreg x:(Arg <t>))
    // cond: t.IsSigned() && t.Size() <= 4
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(t.IsSigned() && t.Size() <= 4)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg (MOVDconst [c]))
    // result: (MOVDconst [int64(int32(c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int32(c)));
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVWZreg x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVWstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is20Bit(int64(off1)+int64(off2))
    // result: (MOVWstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c) && isU12Bit(int64(off)) && ptr.Op != OpSB
    // result: (MOVWstoreconst [makeValAndOff(int32(c),off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        mem = v_2;
        if (!(is16Bit(c) && isU12Bit(int64(off)) && ptr.Op != OpSB)) {
            break;
        }
        v.reset(OpS390XMOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(c), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstore [off1] {sym1} (MOVDaddr <t> [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment()%4 == 0 && (off1+off2)%4 == 0))
    // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        var t = v_0.Type;
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || (t.IsPtr() && t.Elem().Alignment() % 4 == 0 && (off1 + off2) % 4 == 0)))) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p (SRDconst [32] w) x:(MOVWstore [i-4] {s} p w mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVDstore [i-4] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 32) {
            break;
        }
        var w = v_1.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVWstore || auxIntToInt32(x.AuxInt) != i - 4 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(i - 4);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p w0:(SRDconst [j] w) x:(MOVWstore [i-4] {s} p (SRDconst [j+32] w) mem))
    // cond: p.Op != OpSB && x.Uses == 1 && clobber(x)
    // result: (MOVDstore [i-4] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w0 = v_1;
        if (w0.Op != OpS390XSRDconst) {
            break;
        }
        var j = auxIntToUint8(w0.AuxInt);
        w = w0.Args[0];
        x = v_2;
        if (x.Op != OpS390XMOVWstore || auxIntToInt32(x.AuxInt) != i - 4 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var x_1 = x.Args[1];
        if (x_1.Op != OpS390XSRDconst || auxIntToUint8(x_1.AuxInt) != j + 32 || w != x_1.Args[0] || !(p.Op != OpSB && x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(i - 4);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p w1 x:(MOVWstore [i-4] {s} p w0 mem))
    // cond: p.Op != OpSB && x.Uses == 1 && is20Bit(int64(i)-4) && clobber(x)
    // result: (STM2 [i-4] {s} p w0 w1 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w1 = v_1;
        x = v_2;
        if (x.Op != OpS390XMOVWstore || auxIntToInt32(x.AuxInt) != i - 4 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        if (!(p.Op != OpSB && x.Uses == 1 && is20Bit(int64(i) - 4) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTM2);
        v.AuxInt = int32ToAuxInt(i - 4);
        v.Aux = symToAux(s);
        v.AddArg4(p, w0, w1, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p w2 x:(STM2 [i-8] {s} p w0 w1 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-8) && clobber(x)
    // result: (STM3 [i-8] {s} p w0 w1 w2 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w2 = v_1;
        x = v_2;
        if (x.Op != OpS390XSTM2 || auxIntToInt32(x.AuxInt) != i - 8 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[3];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        w1 = x.Args[2];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 8) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTM3);
        v.AuxInt = int32ToAuxInt(i - 8);
        v.Aux = symToAux(s);
        v.AddArg5(p, w0, w1, w2, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p w3 x:(STM3 [i-12] {s} p w0 w1 w2 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-12) && clobber(x)
    // result: (STM4 [i-12] {s} p w0 w1 w2 w3 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        var w3 = v_1;
        x = v_2;
        if (x.Op != OpS390XSTM3 || auxIntToInt32(x.AuxInt) != i - 12 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[4];
        if (p != x.Args[0]) {
            break;
        }
        w0 = x.Args[1];
        w1 = x.Args[2];
        w2 = x.Args[3];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 12) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTM4);
        v.AuxInt = int32ToAuxInt(i - 12);
        v.Aux = symToAux(s);
        v.AddArg6(p, w0, w1, w2, w3, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMOVWstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVWstoreconst [sc] {s} (ADDconst [off] ptr) mem)
    // cond: isU12Bit(sc.Off64()+int64(off))
    // result: (MOVWstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(isU12Bit(sc.Off64() + int64(off)))) {
            break;
        }
        v.reset(OpS390XMOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstoreconst [sc] {sym1} (MOVDaddr [off] {sym2} ptr) mem)
    // cond: ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off)
    // result: (MOVWstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpS390XMOVDaddr) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(ptr.Op != OpSB && canMergeSym(sym1, sym2) && sc.canAdd32(off))) {
            break;
        }
        v.reset(OpS390XMOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstoreconst [c] {s} p x:(MOVWstoreconst [a] {s} p mem))
    // cond: p.Op != OpSB && x.Uses == 1 && a.Off() + 4 == c.Off() && clobber(x)
    // result: (MOVDstore [a.Off()] {s} p (MOVDconst [c.Val64()&0xffffffff | a.Val64()<<32]) mem)
    while (true) {
        var c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p = v_0;
        var x = v_1;
        if (x.Op != OpS390XMOVWstoreconst) {
            break;
        }
        var a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(p.Op != OpSB && x.Uses == 1 && a.Off() + 4 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(a.Off());
        v.Aux = symToAux(s);
        var v0 = b.NewValue0(x.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(c.Val64() & 0xffffffff | a.Val64() << 32);
        v.AddArg3(p, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULLD x (MOVDconst [c]))
    // cond: is32Bit(c)
    // result: (MULLDconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c))) {
                    continue;
                }
                v.reset(OpS390XMULLDconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MULLD <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (MULLDload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XMULLDload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULLDconst <t> x [c])
    // cond: isPowerOfTwo32(c&(c-1))
    // result: (ADD (SLDconst <t> x [uint8(log32(c&(c-1)))]) (SLDconst <t> x [uint8(log32(c&^(c-1)))]))
    while (true) {
        var t = v.Type;
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(isPowerOfTwo32(c & (c - 1)))) {
            break;
        }
        v.reset(OpS390XADD);
        var v0 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(c & (c - 1))));
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(c & ~(c - 1))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLDconst <t> x [c])
    // cond: isPowerOfTwo32(c+(c&^(c-1)))
    // result: (SUB (SLDconst <t> x [uint8(log32(c+(c&^(c-1))))]) (SLDconst <t> x [uint8(log32(c&^(c-1)))]))
    while (true) {
        t = v.Type;
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c + (c & ~(c - 1))))) {
            break;
        }
        v.reset(OpS390XSUB);
        v0 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(c + (c & ~(c - 1)))));
        v0.AddArg(x);
        v1 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(c & ~(c - 1))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLDconst <t> x [c])
    // cond: isPowerOfTwo32(-c+(-c&^(-c-1)))
    // result: (SUB (SLDconst <t> x [uint8(log32(-c&^(-c-1)))]) (SLDconst <t> x [uint8(log32(-c+(-c&^(-c-1))))]))
    while (true) {
        t = v.Type;
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(-c + (-c & ~(-c - 1))))) {
            break;
        }
        v.reset(OpS390XSUB);
        v0 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(-c & ~(-c - 1))));
        v0.AddArg(x);
        v1 = b.NewValue0(v.Pos, OpS390XSLDconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(-c + (-c & ~(-c - 1)))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLDconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)*d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) * d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULLDload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (MULLD x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XMULLD);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLDload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (MULLDload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMULLDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (MULLDload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (MULLDload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XMULLDload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULLW x (MOVDconst [c]))
    // result: (MULLWconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XMULLWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MULLW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (MULLWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVWload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XMULLWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MULLW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (MULLWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                g = v_1;
                if (g.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off = auxIntToInt32(g.AuxInt);
                sym = auxToSym(g.Aux);
                mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XMULLWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULLWconst <t> x [c])
    // cond: isPowerOfTwo32(c&(c-1))
    // result: (ADDW (SLWconst <t> x [uint8(log32(c&(c-1)))]) (SLWconst <t> x [uint8(log32(c&^(c-1)))]))
    while (true) {
        var t = v.Type;
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(isPowerOfTwo32(c & (c - 1)))) {
            break;
        }
        v.reset(OpS390XADDW);
        var v0 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(c & (c - 1))));
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(c & ~(c - 1))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLWconst <t> x [c])
    // cond: isPowerOfTwo32(c+(c&^(c-1)))
    // result: (SUBW (SLWconst <t> x [uint8(log32(c+(c&^(c-1))))]) (SLWconst <t> x [uint8(log32(c&^(c-1)))]))
    while (true) {
        t = v.Type;
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c + (c & ~(c - 1))))) {
            break;
        }
        v.reset(OpS390XSUBW);
        v0 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(c + (c & ~(c - 1)))));
        v0.AddArg(x);
        v1 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(c & ~(c - 1))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLWconst <t> x [c])
    // cond: isPowerOfTwo32(-c+(-c&^(-c-1)))
    // result: (SUBW (SLWconst <t> x [uint8(log32(-c&^(-c-1)))]) (SLWconst <t> x [uint8(log32(-c+(-c&^(-c-1))))]))
    while (true) {
        t = v.Type;
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(-c + (-c & ~(-c - 1))))) {
            break;
        }
        v.reset(OpS390XSUBW);
        v0 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v0.AuxInt = uint8ToAuxInt(uint8(log32(-c & ~(-c - 1))));
        v0.AddArg(x);
        v1 = b.NewValue0(v.Pos, OpS390XSLWconst, t);
        v1.AuxInt = uint8ToAuxInt(uint8(log32(-c + (-c & ~(-c - 1)))));
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (MULLWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c*int32(d))])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c * int32(d)));
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XMULLWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULLWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (MULLWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XMULLWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (MULLWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (MULLWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XMULLWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XNEG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEG (MOVDconst [c]))
    // result: (MOVDconst [-c])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(-c);
        return true;
    } 
    // match: (NEG (ADDconst [c] (NEG x)))
    // cond: c != -(1<<31)
    // result: (ADDconst [-c] x)
    while (true) {
        if (v_0.Op != OpS390XADDconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XNEG) {
            break;
        }
        var x = v_0_0.Args[0];
        if (!(c != -(1 << 31))) {
            break;
        }
        v.reset(OpS390XADDconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XNEGW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEGW (MOVDconst [c]))
    // result: (MOVDconst [int64(int32(-c))])
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int32(-c)));
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XNOT(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NOT x)
    // result: (XOR (MOVDconst [-1]) x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XXOR);
        var v0 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v0.AuxInt = int64ToAuxInt(-1);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XNOTW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NOTW x)
    // result: (XORWconst [-1] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XXORWconst);
        v.AuxInt = int32ToAuxInt(-1);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (OR x (MOVDconst [c]))
    // cond: isU32Bit(c)
    // result: (ORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(isU32Bit(c))) {
                    continue;
                }
                v.reset(OpS390XORconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR (SLDconst x [c]) (SRDconst x [64-c]))
    // result: (RISBGZ x {s390x.NewRotateParams(0, 63, c)})
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 64 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRISBGZ);
                v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(0, 63, c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR (MOVDconst [-1<<63]) (LGDR <t> x))
    // result: (LGDR <t> (LNDFR <x.Type> x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0.AuxInt) != -1 << 63 || v_1.Op != OpS390XLGDR) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var t = v_1.Type;
                x = v_1.Args[0];
                v.reset(OpS390XLGDR);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpS390XLNDFR, x.Type);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR (RISBGZ (LGDR x) {r}) (LGDR (LPDFR <t> y)))
    // cond: r == s390x.NewRotateParams(0, 0, 0)
    // result: (LGDR (CPSDR <t> y x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XRISBGZ) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var r = auxToS390xRotateParams(v_0.Aux);
                var v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpS390XLGDR) {
                    continue;
                }
                x = v_0_0.Args[0];
                if (v_1.Op != OpS390XLGDR) {
                    continue;
                }
                var v_1_0 = v_1.Args[0];
                if (v_1_0.Op != OpS390XLPDFR) {
                    continue;
                }
                t = v_1_0.Type;
                var y = v_1_0.Args[0];
                if (!(r == s390x.NewRotateParams(0, 0, 0))) {
                    continue;
                }
                v.reset(OpS390XLGDR);
                v0 = b.NewValue0(v.Pos, OpS390XCPSDR, t);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR (RISBGZ (LGDR x) {r}) (MOVDconst [c]))
    // cond: c >= 0 && r == s390x.NewRotateParams(0, 0, 0)
    // result: (LGDR (CPSDR <x.Type> (FMOVDconst <x.Type> [math.Float64frombits(uint64(c))]) x))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XRISBGZ) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                r = auxToS390xRotateParams(v_0.Aux);
                v_0_0 = v_0.Args[0];
                if (v_0_0.Op != OpS390XLGDR) {
                    continue;
                }
                x = v_0_0.Args[0];
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(c >= 0 && r == s390x.NewRotateParams(0, 0, 0))) {
                    continue;
                }
                v.reset(OpS390XLGDR);
                v0 = b.NewValue0(v.Pos, OpS390XCPSDR, x.Type);
                var v1 = b.NewValue0(v.Pos, OpS390XFMOVDconst, x.Type);
                v1.AuxInt = float64ToAuxInt(math.Float64frombits(uint64(c)));
                v0.AddArg2(v1, x);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [c|d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XMOVDconst);
                v.AuxInt = int64ToAuxInt(c | d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (OR <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ORload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XORload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x1:(MOVBZload [i1] {s} p mem) sh:(SLDconst [8] x0:(MOVBZload [i0] {s} p mem)))
    // cond: i1 == i0+1 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVHZload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x1 = v_0;
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var i1 = auxIntToInt32(x1.AuxInt);
                var s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                var p = x1.Args[0];
                var sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 8) {
                    continue;
                }
                var x0 = sh.Args[0];
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                }
                var i0 = auxIntToInt32(x0.AuxInt);
                if (auxToSym(x0.Aux) != s) {
                    continue;
                }
                _ = x0.Args[1];
                if (p != x0.Args[0] || mem != x0.Args[1] || !(i1 == i0 + 1 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x0.Pos, OpS390XMOVHZload, typ.UInt16);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x1:(MOVHZload [i1] {s} p mem) sh:(SLDconst [16] x0:(MOVHZload [i0] {s} p mem)))
    // cond: i1 == i0+2 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVWZload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x1 = v_0;
                if (x1.Op != OpS390XMOVHZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 16) {
                    continue;
                }
                x0 = sh.Args[0];
                if (x0.Op != OpS390XMOVHZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                if (auxToSym(x0.Aux) != s) {
                    continue;
                }
                _ = x0.Args[1];
                if (p != x0.Args[0] || mem != x0.Args[1] || !(i1 == i0 + 2 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x0.Pos, OpS390XMOVWZload, typ.UInt32);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x1:(MOVWZload [i1] {s} p mem) sh:(SLDconst [32] x0:(MOVWZload [i0] {s} p mem)))
    // cond: i1 == i0+4 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVDload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x1 = v_0;
                if (x1.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 32) {
                    continue;
                }
                x0 = sh.Args[0];
                if (x0.Op != OpS390XMOVWZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                if (auxToSym(x0.Aux) != s) {
                    continue;
                }
                _ = x0.Args[1];
                if (p != x0.Args[0] || mem != x0.Args[1] || !(i1 == i0 + 4 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x0.Pos, OpS390XMOVDload, typ.UInt64);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR s0:(SLDconst [j0] x0:(MOVBZload [i0] {s} p mem)) or:(OR s1:(SLDconst [j1] x1:(MOVBZload [i1] {s} p mem)) y))
    // cond: i1 == i0+1 && j1 == j0-8 && j1 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (OR <v.Type> (SLDconst <v.Type> [j1] (MOVHZload [i0] {s} p mem)) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s0 = v_0;
                if (s0.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var j0 = auxIntToUint8(s0.AuxInt);
                x0 = s0.Args[0];
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                var or = v_1;
                if (or.Op != OpS390XOR) {
                    continue;
                }
                _ = or.Args[1];
                var or_0 = or.Args[0];
                var or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var s1 = or_0;
                        if (s1.Op != OpS390XSLDconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        var j1 = auxIntToUint8(s1.AuxInt);
                        x1 = s1.Args[0];
                        if (x1.Op != OpS390XMOVBZload) {
                            continue;
                        }
                        i1 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        y = or_1;
                        if (!(i1 == i0 + 1 && j1 == j0 - 8 && j1 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x1.Pos, OpS390XOR, v.Type);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x1.Pos, OpS390XSLDconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j1);
                        var v2 = b.NewValue0(x1.Pos, OpS390XMOVHZload, typ.UInt16);
                        v2.AuxInt = int32ToAuxInt(i0);
                        v2.Aux = symToAux(s);
                        v2.AddArg2(p, mem);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR s0:(SLDconst [j0] x0:(MOVHZload [i0] {s} p mem)) or:(OR s1:(SLDconst [j1] x1:(MOVHZload [i1] {s} p mem)) y))
    // cond: i1 == i0+2 && j1 == j0-16 && j1 % 32 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (OR <v.Type> (SLDconst <v.Type> [j1] (MOVWZload [i0] {s} p mem)) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s0 = v_0;
                if (s0.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                j0 = auxIntToUint8(s0.AuxInt);
                x0 = s0.Args[0];
                if (x0.Op != OpS390XMOVHZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                or = v_1;
                if (or.Op != OpS390XOR) {
                    continue;
                }
                _ = or.Args[1];
                or_0 = or.Args[0];
                or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s1 = or_0;
                        if (s1.Op != OpS390XSLDconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        j1 = auxIntToUint8(s1.AuxInt);
                        x1 = s1.Args[0];
                        if (x1.Op != OpS390XMOVHZload) {
                            continue;
                        }
                        i1 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        y = or_1;
                        if (!(i1 == i0 + 2 && j1 == j0 - 16 && j1 % 32 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x1.Pos, OpS390XOR, v.Type);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x1.Pos, OpS390XSLDconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j1);
                        v2 = b.NewValue0(x1.Pos, OpS390XMOVWZload, typ.UInt32);
                        v2.AuxInt = int32ToAuxInt(i0);
                        v2.Aux = symToAux(s);
                        v2.AddArg2(p, mem);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x0:(MOVBZload [i0] {s} p mem) sh:(SLDconst [8] x1:(MOVBZload [i1] {s} p mem)))
    // cond: p.Op != OpSB && i1 == i0+1 && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVHZreg (MOVHBRload [i0] {s} p mem))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 8) {
                    continue;
                }
                x1 = sh.Args[0];
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(p.Op != OpSB && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpS390XMOVHZreg, typ.UInt64);
                v.copyOf(v0);
                v1 = b.NewValue0(x1.Pos, OpS390XMOVHBRload, typ.UInt16);
                v1.AuxInt = int32ToAuxInt(i0);
                v1.Aux = symToAux(s);
                v1.AddArg2(p, mem);
                v0.AddArg(v1);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR r0:(MOVHZreg x0:(MOVHBRload [i0] {s} p mem)) sh:(SLDconst [16] r1:(MOVHZreg x1:(MOVHBRload [i1] {s} p mem))))
    // cond: i1 == i0+2 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, r0, r1, sh)
    // result: @mergePoint(b,x0,x1) (MOVWZreg (MOVWBRload [i0] {s} p mem))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var r0 = v_0;
                if (r0.Op != OpS390XMOVHZreg) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x0 = r0.Args[0];
                if (x0.Op != OpS390XMOVHBRload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 16) {
                    continue;
                }
                var r1 = sh.Args[0];
                if (r1.Op != OpS390XMOVHZreg) {
                    continue;
                }
                x1 = r1.Args[0];
                if (x1.Op != OpS390XMOVHBRload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(i1 == i0 + 2 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, r0, r1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpS390XMOVWZreg, typ.UInt64);
                v.copyOf(v0);
                v1 = b.NewValue0(x1.Pos, OpS390XMOVWBRload, typ.UInt32);
                v1.AuxInt = int32ToAuxInt(i0);
                v1.Aux = symToAux(s);
                v1.AddArg2(p, mem);
                v0.AddArg(v1);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR r0:(MOVWZreg x0:(MOVWBRload [i0] {s} p mem)) sh:(SLDconst [32] r1:(MOVWZreg x1:(MOVWBRload [i1] {s} p mem))))
    // cond: i1 == i0+4 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, r0, r1, sh)
    // result: @mergePoint(b,x0,x1) (MOVDBRload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                r0 = v_0;
                if (r0.Op != OpS390XMOVWZreg) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x0 = r0.Args[0];
                if (x0.Op != OpS390XMOVWBRload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLDconst || auxIntToUint8(sh.AuxInt) != 32) {
                    continue;
                }
                r1 = sh.Args[0];
                if (r1.Op != OpS390XMOVWZreg) {
                    continue;
                }
                x1 = r1.Args[0];
                if (x1.Op != OpS390XMOVWBRload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(i1 == i0 + 4 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, r0, r1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpS390XMOVDBRload, typ.UInt64);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR s1:(SLDconst [j1] x1:(MOVBZload [i1] {s} p mem)) or:(OR s0:(SLDconst [j0] x0:(MOVBZload [i0] {s} p mem)) y))
    // cond: p.Op != OpSB && i1 == i0+1 && j1 == j0+8 && j0 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (OR <v.Type> (SLDconst <v.Type> [j0] (MOVHZreg (MOVHBRload [i0] {s} p mem))) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                j1 = auxIntToUint8(s1.AuxInt);
                x1 = s1.Args[0];
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                or = v_1;
                if (or.Op != OpS390XOR) {
                    continue;
                }
                _ = or.Args[1];
                or_0 = or.Args[0];
                or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = or_0;
                        if (s0.Op != OpS390XSLDconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        j0 = auxIntToUint8(s0.AuxInt);
                        x0 = s0.Args[0];
                        if (x0.Op != OpS390XMOVBZload) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        if (auxToSym(x0.Aux) != s) {
                            continue;
                        }
                        _ = x0.Args[1];
                        if (p != x0.Args[0] || mem != x0.Args[1]) {
                            continue;
                        }
                        y = or_1;
                        if (!(p.Op != OpSB && i1 == i0 + 1 && j1 == j0 + 8 && j0 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x0.Pos, OpS390XOR, v.Type);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x0.Pos, OpS390XSLDconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j0);
                        v2 = b.NewValue0(x0.Pos, OpS390XMOVHZreg, typ.UInt64);
                        var v3 = b.NewValue0(x0.Pos, OpS390XMOVHBRload, typ.UInt16);
                        v3.AuxInt = int32ToAuxInt(i0);
                        v3.Aux = symToAux(s);
                        v3.AddArg2(p, mem);
                        v2.AddArg(v3);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR s1:(SLDconst [j1] r1:(MOVHZreg x1:(MOVHBRload [i1] {s} p mem))) or:(OR s0:(SLDconst [j0] r0:(MOVHZreg x0:(MOVHBRload [i0] {s} p mem))) y))
    // cond: i1 == i0+2 && j1 == j0+16 && j0 % 32 == 0 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, r0, r1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (OR <v.Type> (SLDconst <v.Type> [j0] (MOVWZreg (MOVWBRload [i0] {s} p mem))) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                j1 = auxIntToUint8(s1.AuxInt);
                r1 = s1.Args[0];
                if (r1.Op != OpS390XMOVHZreg) {
                    continue;
                }
                x1 = r1.Args[0];
                if (x1.Op != OpS390XMOVHBRload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                or = v_1;
                if (or.Op != OpS390XOR) {
                    continue;
                }
                _ = or.Args[1];
                or_0 = or.Args[0];
                or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = or_0;
                        if (s0.Op != OpS390XSLDconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        j0 = auxIntToUint8(s0.AuxInt);
                        r0 = s0.Args[0];
                        if (r0.Op != OpS390XMOVHZreg) {
                            continue;
                        }
                        x0 = r0.Args[0];
                        if (x0.Op != OpS390XMOVHBRload) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        if (auxToSym(x0.Aux) != s) {
                            continue;
                        }
                        _ = x0.Args[1];
                        if (p != x0.Args[0] || mem != x0.Args[1]) {
                            continue;
                        }
                        y = or_1;
                        if (!(i1 == i0 + 2 && j1 == j0 + 16 && j0 % 32 == 0 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, r0, r1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x0.Pos, OpS390XOR, v.Type);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x0.Pos, OpS390XSLDconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j0);
                        v2 = b.NewValue0(x0.Pos, OpS390XMOVWZreg, typ.UInt64);
                        v3 = b.NewValue0(x0.Pos, OpS390XMOVWBRload, typ.UInt32);
                        v3.AuxInt = int32ToAuxInt(i0);
                        v3.Aux = symToAux(s);
                        v3.AddArg2(p, mem);
                        v2.AddArg(v3);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
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
private static bool rewriteValueS390X_OpS390XORW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ORW x (MOVDconst [c]))
    // result: (ORWconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XORWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW (SLWconst x [c]) (SRWconst x [32-c]))
    // result: (RLLconst x [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRWconst || auxIntToUint8(v_1.AuxInt) != 32 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRLLconst);
                v.AuxInt = uint8ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ORW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ORWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVWload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XORWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (ORWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                g = v_1;
                if (g.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off = auxIntToInt32(g.AuxInt);
                sym = auxToSym(g.Aux);
                mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XORWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW x1:(MOVBZload [i1] {s} p mem) sh:(SLWconst [8] x0:(MOVBZload [i0] {s} p mem)))
    // cond: i1 == i0+1 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVHZload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x1 = v_0;
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var i1 = auxIntToInt32(x1.AuxInt);
                var s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                var p = x1.Args[0];
                var sh = v_1;
                if (sh.Op != OpS390XSLWconst || auxIntToUint8(sh.AuxInt) != 8) {
                    continue;
                }
                var x0 = sh.Args[0];
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                }
                var i0 = auxIntToInt32(x0.AuxInt);
                if (auxToSym(x0.Aux) != s) {
                    continue;
                }
                _ = x0.Args[1];
                if (p != x0.Args[0] || mem != x0.Args[1] || !(i1 == i0 + 1 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(x0.Pos, OpS390XMOVHZload, typ.UInt16);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW x1:(MOVHZload [i1] {s} p mem) sh:(SLWconst [16] x0:(MOVHZload [i0] {s} p mem)))
    // cond: i1 == i0+2 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVWZload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x1 = v_0;
                if (x1.Op != OpS390XMOVHZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLWconst || auxIntToUint8(sh.AuxInt) != 16) {
                    continue;
                }
                x0 = sh.Args[0];
                if (x0.Op != OpS390XMOVHZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                if (auxToSym(x0.Aux) != s) {
                    continue;
                }
                _ = x0.Args[1];
                if (p != x0.Args[0] || mem != x0.Args[1] || !(i1 == i0 + 2 && p.Op != OpSB && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x0.Pos, OpS390XMOVWZload, typ.UInt32);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW s0:(SLWconst [j0] x0:(MOVBZload [i0] {s} p mem)) or:(ORW s1:(SLWconst [j1] x1:(MOVBZload [i1] {s} p mem)) y))
    // cond: i1 == i0+1 && j1 == j0-8 && j1 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (ORW <v.Type> (SLWconst <v.Type> [j1] (MOVHZload [i0] {s} p mem)) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s0 = v_0;
                if (s0.Op != OpS390XSLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var j0 = auxIntToUint8(s0.AuxInt);
                x0 = s0.Args[0];
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                var or = v_1;
                if (or.Op != OpS390XORW) {
                    continue;
                }
                _ = or.Args[1];
                var or_0 = or.Args[0];
                var or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        var s1 = or_0;
                        if (s1.Op != OpS390XSLWconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        var j1 = auxIntToUint8(s1.AuxInt);
                        x1 = s1.Args[0];
                        if (x1.Op != OpS390XMOVBZload) {
                            continue;
                        }
                        i1 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        var y = or_1;
                        if (!(i1 == i0 + 1 && j1 == j0 - 8 && j1 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x1.Pos, OpS390XORW, v.Type);
                        v.copyOf(v0);
                        var v1 = b.NewValue0(x1.Pos, OpS390XSLWconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j1);
                        var v2 = b.NewValue0(x1.Pos, OpS390XMOVHZload, typ.UInt16);
                        v2.AuxInt = int32ToAuxInt(i0);
                        v2.Aux = symToAux(s);
                        v2.AddArg2(p, mem);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW x0:(MOVBZload [i0] {s} p mem) sh:(SLWconst [8] x1:(MOVBZload [i1] {s} p mem)))
    // cond: p.Op != OpSB && i1 == i0+1 && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, sh)
    // result: @mergePoint(b,x0,x1) (MOVHZreg (MOVHBRload [i0] {s} p mem))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpS390XMOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLWconst || auxIntToUint8(sh.AuxInt) != 8) {
                    continue;
                }
                x1 = sh.Args[0];
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(p.Op != OpSB && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpS390XMOVHZreg, typ.UInt64);
                v.copyOf(v0);
                v1 = b.NewValue0(x1.Pos, OpS390XMOVHBRload, typ.UInt16);
                v1.AuxInt = int32ToAuxInt(i0);
                v1.Aux = symToAux(s);
                v1.AddArg2(p, mem);
                v0.AddArg(v1);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW r0:(MOVHZreg x0:(MOVHBRload [i0] {s} p mem)) sh:(SLWconst [16] r1:(MOVHZreg x1:(MOVHBRload [i1] {s} p mem))))
    // cond: i1 == i0+2 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, r0, r1, sh)
    // result: @mergePoint(b,x0,x1) (MOVWBRload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var r0 = v_0;
                if (r0.Op != OpS390XMOVHZreg) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x0 = r0.Args[0];
                if (x0.Op != OpS390XMOVHBRload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                sh = v_1;
                if (sh.Op != OpS390XSLWconst || auxIntToUint8(sh.AuxInt) != 16) {
                    continue;
                }
                var r1 = sh.Args[0];
                if (r1.Op != OpS390XMOVHZreg) {
                    continue;
                }
                x1 = r1.Args[0];
                if (x1.Op != OpS390XMOVHBRload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(i1 == i0 + 2 && x0.Uses == 1 && x1.Uses == 1 && r0.Uses == 1 && r1.Uses == 1 && sh.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, r0, r1, sh))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpS390XMOVWBRload, typ.UInt32);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORW s1:(SLWconst [j1] x1:(MOVBZload [i1] {s} p mem)) or:(ORW s0:(SLWconst [j0] x0:(MOVBZload [i0] {s} p mem)) y))
    // cond: p.Op != OpSB && i1 == i0+1 && j1 == j0+8 && j0 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b,x0,x1,y) != nil && clobber(x0, x1, s0, s1, or)
    // result: @mergePoint(b,x0,x1,y) (ORW <v.Type> (SLWconst <v.Type> [j0] (MOVHZreg (MOVHBRload [i0] {s} p mem))) y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpS390XSLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                j1 = auxIntToUint8(s1.AuxInt);
                x1 = s1.Args[0];
                if (x1.Op != OpS390XMOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                s = auxToSym(x1.Aux);
                mem = x1.Args[1];
                p = x1.Args[0];
                or = v_1;
                if (or.Op != OpS390XORW) {
                    continue;
                }
                _ = or.Args[1];
                or_0 = or.Args[0];
                or_1 = or.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = or_0;
                        if (s0.Op != OpS390XSLWconst) {
                            continue;
                        (_i1, or_0, or_1) = (_i1 + 1, or_1, or_0);
                        }
                        j0 = auxIntToUint8(s0.AuxInt);
                        x0 = s0.Args[0];
                        if (x0.Op != OpS390XMOVBZload) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        if (auxToSym(x0.Aux) != s) {
                            continue;
                        }
                        _ = x0.Args[1];
                        if (p != x0.Args[0] || mem != x0.Args[1]) {
                            continue;
                        }
                        y = or_1;
                        if (!(p.Op != OpSB && i1 == i0 + 1 && j1 == j0 + 8 && j0 % 16 == 0 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && or.Uses == 1 && mergePoint(b, x0, x1, y) != null && clobber(x0, x1, s0, s1, or))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, y);
                        v0 = b.NewValue0(x0.Pos, OpS390XORW, v.Type);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x0.Pos, OpS390XSLWconst, v.Type);
                        v1.AuxInt = uint8ToAuxInt(j0);
                        v2 = b.NewValue0(x0.Pos, OpS390XMOVHZreg, typ.UInt64);
                        var v3 = b.NewValue0(x0.Pos, OpS390XMOVHBRload, typ.UInt16);
                        v3.AuxInt = int32ToAuxInt(i0);
                        v3.Aux = symToAux(s);
                        v3.AddArg2(p, mem);
                        v2.AddArg(v3);
                        v1.AddArg(v2);
                        v0.AddArg2(v1, y);
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
private static bool rewriteValueS390X_OpS390XORWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORWconst [c] x)
    // cond: int32(c)==0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(int32(c) == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ORWconst [c] _)
    // cond: int32(c)==-1
    // result: (MOVDconst [-1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (!(int32(c) == -1)) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(-1);
        return true;
    } 
    // match: (ORWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)|d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) | d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XORWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ORWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ORWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XORWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ORWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ORWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XORWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (ORconst [-1] _)
    // result: (MOVDconst [-1])
    while (true) {
        if (auxIntToInt64(v.AuxInt) != -1) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(-1);
        return true;
    } 
    // match: (ORconst [c] (MOVDconst [d]))
    // result: (MOVDconst [c|d])
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(c | d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XORload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (OR x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XOR);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (ORload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (ORload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XORload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (ORload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (ORload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XORload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XRISBGZ(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RISBGZ (MOVWZreg x) {r})
    // cond: r.InMerge(0xffffffff) != nil
    // result: (RISBGZ x {*r.InMerge(0xffffffff)})
    while (true) {
        var r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XMOVWZreg) {
            break;
        }
        var x = v_0.Args[0];
        if (!(r.InMerge(0xffffffff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.InMerge>>(0xffffffff));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (MOVHZreg x) {r})
    // cond: r.InMerge(0x0000ffff) != nil
    // result: (RISBGZ x {*r.InMerge(0x0000ffff)})
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XMOVHZreg) {
            break;
        }
        x = v_0.Args[0];
        if (!(r.InMerge(0x0000ffff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.InMerge>>(0x0000ffff));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (MOVBZreg x) {r})
    // cond: r.InMerge(0x000000ff) != nil
    // result: (RISBGZ x {*r.InMerge(0x000000ff)})
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XMOVBZreg) {
            break;
        }
        x = v_0.Args[0];
        if (!(r.InMerge(0x000000ff) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(new ptr<ptr<r.InMerge>>(0x000000ff));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (SLDconst x [c]) {r})
    // cond: r.InMerge(^uint64(0)<<c) != nil
    // result: (RISBGZ x {(*r.InMerge(^uint64(0)<<c)).RotateLeft(c)})
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XSLDconst) {
            break;
        }
        var c = auxIntToUint8(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(r.InMerge(~uint64(0) << (int)(c)) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux((new ptr<ptr<r.InMerge>>(~uint64(0) << (int)(c))).RotateLeft(c));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (SRDconst x [c]) {r})
    // cond: r.InMerge(^uint64(0)>>c) != nil
    // result: (RISBGZ x {(*r.InMerge(^uint64(0)>>c)).RotateLeft(-c)})
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XSRDconst) {
            break;
        }
        c = auxIntToUint8(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(r.InMerge(~uint64(0) >> (int)(c)) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux((new ptr<ptr<r.InMerge>>(~uint64(0) >> (int)(c))).RotateLeft(-c));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (RISBGZ x {y}) {z})
    // cond: z.InMerge(y.OutMask()) != nil
    // result: (RISBGZ x {(*z.InMerge(y.OutMask())).RotateLeft(y.Amount)})
    while (true) {
        var z = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var y = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(z.InMerge(y.OutMask()) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux((new ptr<ptr<z.InMerge>>(y.OutMask())).RotateLeft(y.Amount));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ x {r})
    // cond: r.End == 63 && r.Start == -r.Amount&63
    // result: (SRDconst x [-r.Amount&63])
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        x = v_0;
        if (!(r.End == 63 && r.Start == -r.Amount & 63)) {
            break;
        }
        v.reset(OpS390XSRDconst);
        v.AuxInt = uint8ToAuxInt(-r.Amount & 63);
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ x {r})
    // cond: r.Start == 0 && r.End == 63-r.Amount
    // result: (SLDconst x [r.Amount])
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        x = v_0;
        if (!(r.Start == 0 && r.End == 63 - r.Amount)) {
            break;
        }
        v.reset(OpS390XSLDconst);
        v.AuxInt = uint8ToAuxInt(r.Amount);
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (SRADconst x [c]) {r})
    // cond: r.Start == r.End && (r.Start+r.Amount)&63 <= c
    // result: (RISBGZ x {s390x.NewRotateParams(r.Start, r.Start, -r.Start&63)})
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XSRADconst) {
            break;
        }
        c = auxIntToUint8(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(r.Start == r.End && (r.Start + r.Amount) & 63 <= c)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(r.Start, r.Start, -r.Start & 63));
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ x {r})
    // cond: r == s390x.NewRotateParams(56, 63, 0)
    // result: (MOVBZreg x)
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        x = v_0;
        if (!(r == s390x.NewRotateParams(56, 63, 0))) {
            break;
        }
        v.reset(OpS390XMOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ x {r})
    // cond: r == s390x.NewRotateParams(48, 63, 0)
    // result: (MOVHZreg x)
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        x = v_0;
        if (!(r == s390x.NewRotateParams(48, 63, 0))) {
            break;
        }
        v.reset(OpS390XMOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ x {r})
    // cond: r == s390x.NewRotateParams(32, 63, 0)
    // result: (MOVWZreg x)
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        x = v_0;
        if (!(r == s390x.NewRotateParams(32, 63, 0))) {
            break;
        }
        v.reset(OpS390XMOVWZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (RISBGZ (LGDR <t> x) {r})
    // cond: r == s390x.NewRotateParams(1, 63, 0)
    // result: (LGDR <t> (LPDFR <x.Type> x))
    while (true) {
        r = auxToS390xRotateParams(v.Aux);
        if (v_0.Op != OpS390XLGDR) {
            break;
        }
        var t = v_0.Type;
        x = v_0.Args[0];
        if (!(r == s390x.NewRotateParams(1, 63, 0))) {
            break;
        }
        v.reset(OpS390XLGDR);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, OpS390XLPDFR, x.Type);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XRLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RLL x (MOVDconst [c]))
    // result: (RLLconst x [uint8(c&31)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XRLLconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 31));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XRLLG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RLLG x (MOVDconst [c]))
    // result: (RISBGZ x {s390x.NewRotateParams(0, 63, uint8(c&63))})
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(0, 63, uint8(c & 63)));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSLD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SLD x (MOVDconst [c]))
    // result: (SLDconst x [uint8(c&63)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XSLDconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 63));
        v.AddArg(x);
        return true;
    } 
    // match: (SLD x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (AND (MOVDconst [c]) y))
    // result: (SLD x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSLD);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SLD x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVWreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVHreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVBreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVWZreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVHZreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLD x (MOVBZreg y))
    // result: (SLD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLD);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSLDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLDconst (SRDconst x [c]) [d])
    // result: (RISBGZ x {s390x.NewRotateParams(uint8(max8(0, int8(c-d))), 63-d, uint8(int8(d-c)&63))})
    while (true) {
        var d = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XSRDconst) {
            break;
        }
        var c = auxIntToUint8(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(uint8(max8(0, int8(c - d))), 63 - d, uint8(int8(d - c) & 63)));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst (RISBGZ x {r}) [c])
    // cond: s390x.NewRotateParams(0, 63-c, c).InMerge(r.OutMask()) != nil
    // result: (RISBGZ x {(*s390x.NewRotateParams(0, 63-c, c).InMerge(r.OutMask())).RotateLeft(r.Amount)})
    while (true) {
        c = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(s390x.NewRotateParams(0, 63 - c, c).InMerge(r.OutMask()) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux((s390x.NewRotateParams(0, 63 - c, c).InMerge(r.OutMask()).val).RotateLeft(r.Amount));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSLW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SLW x (MOVDconst [c]))
    // cond: c&32 == 0
    // result: (SLWconst x [uint8(c&31)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 == 0)) {
            break;
        }
        v.reset(OpS390XSLWconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 31));
        v.AddArg(x);
        return true;
    } 
    // match: (SLW _ (MOVDconst [c]))
    // cond: c&32 != 0
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 != 0)) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (SLW x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (AND (MOVDconst [c]) y))
    // result: (SLW x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSLW);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SLW x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVWreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVHreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVBreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVWZreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVHZreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SLW x (MOVBZreg y))
    // result: (SLW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSLW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSLWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLWconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRAD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SRAD x (MOVDconst [c]))
    // result: (SRADconst x [uint8(c&63)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XSRADconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 63));
        v.AddArg(x);
        return true;
    } 
    // match: (SRAD x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (AND (MOVDconst [c]) y))
    // result: (SRAD x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSRAD);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SRAD x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVWreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVHreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVBreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVWZreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVHZreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAD x (MOVBZreg y))
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAD);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRADconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRADconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SRADconst [c] (MOVDconst [d]))
    // result: (MOVDconst [d>>uint64(c)])
    while (true) {
        var c = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(d >> (int)(uint64(c)));
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRAW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SRAW x (MOVDconst [c]))
    // cond: c&32 == 0
    // result: (SRAWconst x [uint8(c&31)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 == 0)) {
            break;
        }
        v.reset(OpS390XSRAWconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 31));
        v.AddArg(x);
        return true;
    } 
    // match: (SRAW x (MOVDconst [c]))
    // cond: c&32 != 0
    // result: (SRAWconst x [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 != 0)) {
            break;
        }
        v.reset(OpS390XSRAWconst);
        v.AuxInt = uint8ToAuxInt(31);
        v.AddArg(x);
        return true;
    } 
    // match: (SRAW x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (AND (MOVDconst [c]) y))
    // result: (SRAW x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSRAW);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SRAW x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVWreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVHreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVBreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVWZreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVHZreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRAW x (MOVBZreg y))
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRAW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRAWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRAWconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SRAWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(int32(d))>>uint64(c)])
    while (true) {
        var c = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int32(d)) >> (int)(uint64(c)));
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SRD x (MOVDconst [c]))
    // result: (SRDconst x [uint8(c&63)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XSRDconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 63));
        v.AddArg(x);
        return true;
    } 
    // match: (SRD x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (AND (MOVDconst [c]) y))
    // result: (SRD x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSRD);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SRD x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVWreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVHreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVBreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVWZreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVHZreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRD x (MOVBZreg y))
    // result: (SRD x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRD);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRDconst (SLDconst x [c]) [d])
    // result: (RISBGZ x {s390x.NewRotateParams(d, uint8(min8(63, int8(63-c+d))), uint8(int8(c-d)&63))})
    while (true) {
        var d = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XSLDconst) {
            break;
        }
        var c = auxIntToUint8(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(d, uint8(min8(63, int8(63 - c + d))), uint8(int8(c - d) & 63)));
        v.AddArg(x);
        return true;
    } 
    // match: (SRDconst (RISBGZ x {r}) [c])
    // cond: s390x.NewRotateParams(c, 63, -c&63).InMerge(r.OutMask()) != nil
    // result: (RISBGZ x {(*s390x.NewRotateParams(c, 63, -c&63).InMerge(r.OutMask())).RotateLeft(r.Amount)})
    while (true) {
        c = auxIntToUint8(v.AuxInt);
        if (v_0.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_0.Aux);
        x = v_0.Args[0];
        if (!(s390x.NewRotateParams(c, 63, -c & 63).InMerge(r.OutMask()) != null)) {
            break;
        }
        v.reset(OpS390XRISBGZ);
        v.Aux = s390xRotateParamsToAux((s390x.NewRotateParams(c, 63, -c & 63).InMerge(r.OutMask()).val).RotateLeft(r.Amount));
        v.AddArg(x);
        return true;
    } 
    // match: (SRDconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SRW x (MOVDconst [c]))
    // cond: c&32 == 0
    // result: (SRWconst x [uint8(c&31)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 == 0)) {
            break;
        }
        v.reset(OpS390XSRWconst);
        v.AuxInt = uint8ToAuxInt(uint8(c & 31));
        v.AddArg(x);
        return true;
    } 
    // match: (SRW _ (MOVDconst [c]))
    // cond: c&32 != 0
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(c & 32 != 0)) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (SRW x (RISBGZ y {r}))
    // cond: r.Amount == 0 && r.OutMask()&63 == 63
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XRISBGZ) {
            break;
        }
        var r = auxToS390xRotateParams(v_1.Aux);
        var y = v_1.Args[0];
        if (!(r.Amount == 0 && r.OutMask() & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (AND (MOVDconst [c]) y))
    // result: (SRW x (ANDWconst <typ.UInt32> [int32(c&63)] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XAND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_1_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                c = auxIntToInt64(v_1_0.AuxInt);
                y = v_1_1;
                v.reset(OpS390XSRW);
                var v0 = b.NewValue0(v.Pos, OpS390XANDWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(c & 63));
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (SRW x (ANDWconst [c] y))
    // cond: c&63 == 63
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XANDWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(c & 63 == 63)) {
            break;
        }
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVWreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVHreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVBreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVWZreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVWZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVHZreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVHZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SRW x (MOVBZreg y))
    // result: (SRW x y)
    while (true) {
        x = v_0;
        if (v_1.Op != OpS390XMOVBZreg) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpS390XSRW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSRWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRWconst x [0])
    // result: x
    while (true) {
        if (auxIntToUint8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSTM2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (STM2 [i] {s} p w2 w3 x:(STM2 [i-8] {s} p w0 w1 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-8) && clobber(x)
    // result: (STM4 [i-8] {s} p w0 w1 w2 w3 mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        var w2 = v_1;
        var w3 = v_2;
        var x = v_3;
        if (x.Op != OpS390XSTM2 || auxIntToInt32(x.AuxInt) != i - 8 || auxToSym(x.Aux) != s) {
            break;
        }
        var mem = x.Args[3];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        var w1 = x.Args[2];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 8) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTM4);
        v.AuxInt = int32ToAuxInt(i - 8);
        v.Aux = symToAux(s);
        v.AddArg6(p, w0, w1, w2, w3, mem);
        return true;
    } 
    // match: (STM2 [i] {s} p (SRDconst [32] x) x mem)
    // result: (MOVDstore [i] {s} p x mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 32) {
            break;
        }
        x = v_1.Args[0];
        if (x != v_2) {
            break;
        }
        mem = v_3;
        v.reset(OpS390XMOVDstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSTMG2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (STMG2 [i] {s} p w2 w3 x:(STMG2 [i-16] {s} p w0 w1 mem))
    // cond: x.Uses == 1 && is20Bit(int64(i)-16) && clobber(x)
    // result: (STMG4 [i-16] {s} p w0 w1 w2 w3 mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        var w2 = v_1;
        var w3 = v_2;
        var x = v_3;
        if (x.Op != OpS390XSTMG2 || auxIntToInt32(x.AuxInt) != i - 16 || auxToSym(x.Aux) != s) {
            break;
        }
        var mem = x.Args[3];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        var w1 = x.Args[2];
        if (!(x.Uses == 1 && is20Bit(int64(i) - 16) && clobber(x))) {
            break;
        }
        v.reset(OpS390XSTMG4);
        v.AuxInt = int32ToAuxInt(i - 16);
        v.Aux = symToAux(s);
        v.AddArg6(p, w0, w1, w2, w3, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUB x (MOVDconst [c]))
    // cond: is32Bit(c)
    // result: (SUBconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(is32Bit(c))) {
            break;
        }
        v.reset(OpS390XSUBconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (SUB (MOVDconst [c]) x)
    // cond: is32Bit(c)
    // result: (NEG (SUBconst <v.Type> x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        if (!(is32Bit(c))) {
            break;
        }
        v.reset(OpS390XNEG);
        var v0 = b.NewValue0(v.Pos, OpS390XSUBconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (SUB x x)
    // result: (MOVDconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (SUB <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (SUBload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        var g = v_1;
        if (g.Op != OpS390XMOVDload) {
            break;
        }
        var off = auxIntToInt32(g.AuxInt);
        var sym = auxToSym(g.Aux);
        var mem = g.Args[1];
        var ptr = g.Args[0];
        if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
            break;
        }
        v.reset(OpS390XSUBload);
        v.Type = t;
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUBE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBE x y (FlagGT))
    // result: (SUBC x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpS390XFlagGT) {
            break;
        }
        v.reset(OpS390XSUBC);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SUBE x y (FlagOV))
    // result: (SUBC x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpS390XFlagOV) {
            break;
        }
        v.reset(OpS390XSUBC);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (SUBE x y (Select1 (SUBC (MOVDconst [0]) (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) c))))))
    // result: (SUBE x y c)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpSelect1) {
            break;
        }
        var v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpS390XSUBC) {
            break;
        }
        _ = v_2_0.Args[1];
        var v_2_0_0 = v_2_0.Args[0];
        if (v_2_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_2_0_0.AuxInt) != 0) {
            break;
        }
        var v_2_0_1 = v_2_0.Args[1];
        if (v_2_0_1.Op != OpS390XNEG) {
            break;
        }
        var v_2_0_1_0 = v_2_0_1.Args[0];
        if (v_2_0_1_0.Op != OpSelect0) {
            break;
        }
        var v_2_0_1_0_0 = v_2_0_1_0.Args[0];
        if (v_2_0_1_0_0.Op != OpS390XSUBE) {
            break;
        }
        var c = v_2_0_1_0_0.Args[2];
        var v_2_0_1_0_0_0 = v_2_0_1_0_0.Args[0];
        if (v_2_0_1_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_2_0_1_0_0_0.AuxInt) != 0) {
            break;
        }
        var v_2_0_1_0_0_1 = v_2_0_1_0_0.Args[1];
        if (v_2_0_1_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_2_0_1_0_0_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpS390XSUBE);
        v.AddArg3(x, y, c);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUBW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBW x (MOVDconst [c]))
    // result: (SUBWconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpS390XMOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpS390XSUBWconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (SUBW (MOVDconst [c]) x)
    // result: (NEGW (SUBWconst <v.Type> x [int32(c)]))
    while (true) {
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        v.reset(OpS390XNEGW);
        var v0 = b.NewValue0(v.Pos, OpS390XSUBWconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (SUBW x x)
    // result: (MOVDconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (SUBW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (SUBWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        var g = v_1;
        if (g.Op != OpS390XMOVWload) {
            break;
        }
        var off = auxIntToInt32(g.AuxInt);
        var sym = auxToSym(g.Aux);
        var mem = g.Args[1];
        var ptr = g.Args[0];
        if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
            break;
        }
        v.reset(OpS390XSUBWload);
        v.Type = t;
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (SUBW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (SUBWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        x = v_0;
        g = v_1;
        if (g.Op != OpS390XMOVWZload) {
            break;
        }
        off = auxIntToInt32(g.AuxInt);
        sym = auxToSym(g.Aux);
        mem = g.Args[1];
        ptr = g.Args[0];
        if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
            break;
        }
        v.reset(OpS390XSUBWload);
        v.Type = t;
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUBWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SUBWconst [c] x)
    // cond: int32(c) == 0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(int32(c) == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SUBWconst [c] x)
    // result: (ADDWconst [-int32(c)] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        v.reset(OpS390XADDWconst);
        v.AuxInt = int32ToAuxInt(-int32(c));
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XSUBWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (SUBWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XSUBWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (SUBWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (SUBWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XSUBWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SUBconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SUBconst [c] x)
    // cond: c != -(1<<31)
    // result: (ADDconst [-c] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c != -(1 << 31))) {
            break;
        }
        v.reset(OpS390XADDconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;
    } 
    // match: (SUBconst (MOVDconst [d]) [c])
    // result: (MOVDconst [d-int64(c)])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(d - int64(c));
        return true;
    } 
    // match: (SUBconst (SUBconst x [d]) [c])
    // cond: is32Bit(-int64(c)-int64(d))
    // result: (ADDconst [-c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(is32Bit(-int64(c) - int64(d)))) {
            break;
        }
        v.reset(OpS390XADDconst);
        v.AuxInt = int32ToAuxInt(-c - d);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSUBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (SUB x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XSUB);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (SUBload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (SUBload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XSUBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (SUBload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (SUBload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XSUBload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XSumBytes2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SumBytes2 x)
    // result: (ADDW (SRWconst <typ.UInt8> x [8]) x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XADDW);
        var v0 = b.NewValue0(v.Pos, OpS390XSRWconst, typ.UInt8);
        v0.AuxInt = uint8ToAuxInt(8);
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XSumBytes4(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SumBytes4 x)
    // result: (SumBytes2 (ADDW <typ.UInt16> (SRWconst <typ.UInt16> x [16]) x))
    while (true) {
        var x = v_0;
        v.reset(OpS390XSumBytes2);
        var v0 = b.NewValue0(v.Pos, OpS390XADDW, typ.UInt16);
        var v1 = b.NewValue0(v.Pos, OpS390XSRWconst, typ.UInt16);
        v1.AuxInt = uint8ToAuxInt(16);
        v1.AddArg(x);
        v0.AddArg2(v1, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XSumBytes8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (SumBytes8 x)
    // result: (SumBytes4 (ADDW <typ.UInt32> (SRDconst <typ.UInt32> x [32]) x))
    while (true) {
        var x = v_0;
        v.reset(OpS390XSumBytes4);
        var v0 = b.NewValue0(v.Pos, OpS390XADDW, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpS390XSRDconst, typ.UInt32);
        v1.AuxInt = uint8ToAuxInt(32);
        v1.AddArg(x);
        v0.AddArg2(v1, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpS390XXOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XOR x (MOVDconst [c]))
    // cond: isU32Bit(c)
    // result: (XORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(isU32Bit(c))) {
                    continue;
                }
                v.reset(OpS390XXORconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLDconst x [c]) (SRDconst x [64-c]))
    // result: (RISBGZ x {s390x.NewRotateParams(0, 63, c)})
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRDconst || auxIntToUint8(v_1.AuxInt) != 64 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRISBGZ);
                v.Aux = s390xRotateParamsToAux(s390x.NewRotateParams(0, 63, c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [c^d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XMOVDconst);
                v.AuxInt = int64ToAuxInt(c ^ d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR x x)
    // result: (MOVDconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (XOR <t> x g:(MOVDload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (XORload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XXORload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XXORW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XORW x (MOVDconst [c]))
    // result: (XORWconst [int32(c)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpS390XMOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpS390XXORWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORW (SLWconst x [c]) (SRWconst x [32-c]))
    // result: (RLLconst x [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpS390XSLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToUint8(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpS390XSRWconst || auxIntToUint8(v_1.AuxInt) != 32 - c || x != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpS390XRLLconst);
                v.AuxInt = uint8ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORW x x)
    // result: (MOVDconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (XORW <t> x g:(MOVWload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (XORWload <t> [off] {sym} x ptr mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var g = v_1;
                if (g.Op != OpS390XMOVWload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(g.AuxInt);
                var sym = auxToSym(g.Aux);
                var mem = g.Args[1];
                var ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XXORWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORW <t> x g:(MOVWZload [off] {sym} ptr mem))
    // cond: ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g)
    // result: (XORWload <t> [off] {sym} x ptr mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                g = v_1;
                if (g.Op != OpS390XMOVWZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off = auxIntToInt32(g.AuxInt);
                sym = auxToSym(g.Aux);
                mem = g.Args[1];
                ptr = g.Args[0];
                if (!(ptr.Op != OpSB && is20Bit(int64(off)) && canMergeLoadClobber(v, g, x) && clobber(g))) {
                    continue;
                }
                v.reset(OpS390XXORWload);
                v.Type = t;
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XXORWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORWconst [c] x)
    // cond: int32(c)==0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(int32(c) == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (XORWconst [c] (MOVDconst [d]))
    // result: (MOVDconst [int64(c)^d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(int64(c) ^ d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XXORWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XORWload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (XORWload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XXORWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (XORWload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (XORWload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XXORWload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XXORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (XORconst [c] (MOVDconst [d]))
    // result: (MOVDconst [c^d])
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpS390XMOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(c ^ d);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpS390XXORload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORload <t> [off] {sym} x ptr1 (FMOVDstore [off] {sym} ptr2 y _))
    // cond: isSamePtr(ptr1, ptr2)
    // result: (XOR x (LGDR <t> y))
    while (true) {
        var t = v.Type;
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        var ptr1 = v_1;
        if (v_2.Op != OpS390XFMOVDstore || auxIntToInt32(v_2.AuxInt) != off || auxToSym(v_2.Aux) != sym) {
            break;
        }
        var y = v_2.Args[1];
        var ptr2 = v_2.Args[0];
        if (!(isSamePtr(ptr1, ptr2))) {
            break;
        }
        v.reset(OpS390XXOR);
        var v0 = b.NewValue0(v_2.Pos, OpS390XLGDR, t);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (XORload [off1] {sym} x (ADDconst [off2] ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(off1)+int64(off2))
    // result: (XORload [off1+off2] {sym} x ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var ptr = v_1.Args[0];
        var mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(OpS390XXORload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (XORload [o1] {s1} x (MOVDaddr [o2] {s2} ptr) mem)
    // cond: ptr.Op != OpSB && is20Bit(int64(o1)+int64(o2)) && canMergeSym(s1, s2)
    // result: (XORload [o1+o2] {mergeSym(s1, s2)} x ptr mem)
    while (true) {
        var o1 = auxIntToInt32(v.AuxInt);
        var s1 = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != OpS390XMOVDaddr) {
            break;
        }
        var o2 = auxIntToInt32(v_1.AuxInt);
        var s2 = auxToSym(v_1.Aux);
        ptr = v_1.Args[0];
        mem = v_2;
        if (!(ptr.Op != OpSB && is20Bit(int64(o1) + int64(o2)) && canMergeSym(s1, s2))) {
            break;
        }
        v.reset(OpS390XXORload);
        v.AuxInt = int32ToAuxInt(o1 + o2);
        v.Aux = symToAux(mergeSym(s1, s2));
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpSelect0(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select0 (Add64carry x y c))
    // result: (Select0 <typ.UInt64> (ADDE x y (Select1 <types.TypeFlags> (ADDCconst c [-1]))))
    while (true) {
        if (v_0.Op != OpAdd64carry) {
            break;
        }
        var c = v_0.Args[2];
        var x = v_0.Args[0];
        var y = v_0.Args[1];
        v.reset(OpSelect0);
        v.Type = typ.UInt64;
        var v0 = b.NewValue0(v.Pos, OpS390XADDE, types.NewTuple(typ.UInt64, types.TypeFlags));
        var v1 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v2 = b.NewValue0(v.Pos, OpS390XADDCconst, types.NewTuple(typ.UInt64, types.TypeFlags));
        v2.AuxInt = int16ToAuxInt(-1);
        v2.AddArg(c);
        v1.AddArg(v2);
        v0.AddArg3(x, y, v1);
        v.AddArg(v0);
        return true;
    } 
    // match: (Select0 (Sub64borrow x y c))
    // result: (Select0 <typ.UInt64> (SUBE x y (Select1 <types.TypeFlags> (SUBC (MOVDconst [0]) c))))
    while (true) {
        if (v_0.Op != OpSub64borrow) {
            break;
        }
        c = v_0.Args[2];
        x = v_0.Args[0];
        y = v_0.Args[1];
        v.reset(OpSelect0);
        v.Type = typ.UInt64;
        v0 = b.NewValue0(v.Pos, OpS390XSUBE, types.NewTuple(typ.UInt64, types.TypeFlags));
        v1 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        v2 = b.NewValue0(v.Pos, OpS390XSUBC, types.NewTuple(typ.UInt64, types.TypeFlags));
        var v3 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v3.AuxInt = int64ToAuxInt(0);
        v2.AddArg2(v3, c);
        v1.AddArg(v2);
        v0.AddArg3(x, y, v1);
        v.AddArg(v0);
        return true;
    } 
    // match: (Select0 <t> (AddTupleFirst32 val tuple))
    // result: (ADDW val (Select0 <t> tuple))
    while (true) {
        var t = v.Type;
        if (v_0.Op != OpS390XAddTupleFirst32) {
            break;
        }
        var tuple = v_0.Args[1];
        var val = v_0.Args[0];
        v.reset(OpS390XADDW);
        v0 = b.NewValue0(v.Pos, OpSelect0, t);
        v0.AddArg(tuple);
        v.AddArg2(val, v0);
        return true;
    } 
    // match: (Select0 <t> (AddTupleFirst64 val tuple))
    // result: (ADD val (Select0 <t> tuple))
    while (true) {
        t = v.Type;
        if (v_0.Op != OpS390XAddTupleFirst64) {
            break;
        }
        tuple = v_0.Args[1];
        val = v_0.Args[0];
        v.reset(OpS390XADD);
        v0 = b.NewValue0(v.Pos, OpSelect0, t);
        v0.AddArg(tuple);
        v.AddArg2(val, v0);
        return true;
    } 
    // match: (Select0 (ADDCconst (MOVDconst [c]) [d]))
    // result: (MOVDconst [c+int64(d)])
    while (true) {
        if (v_0.Op != OpS390XADDCconst) {
            break;
        }
        var d = auxIntToInt16(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(c + int64(d));
        return true;
    } 
    // match: (Select0 (SUBC (MOVDconst [c]) (MOVDconst [d])))
    // result: (MOVDconst [c-d])
    while (true) {
        if (v_0.Op != OpS390XSUBC) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0_1.AuxInt);
        v.reset(OpS390XMOVDconst);
        v.AuxInt = int64ToAuxInt(c - d);
        return true;
    } 
    // match: (Select0 (FADD (FMUL y z) x))
    // result: (FMADD x y z)
    while (true) {
        if (v_0.Op != OpS390XFADD) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpS390XFMUL) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                var z = v_0_0.Args[1];
                y = v_0_0.Args[0];
                x = v_0_1;
                v.reset(OpS390XFMADD);
                v.AddArg3(x, y, z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Select0 (FSUB (FMUL y z) x))
    // result: (FMSUB x y z)
    while (true) {
        if (v_0.Op != OpS390XFSUB) {
            break;
        }
        x = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XFMUL) {
            break;
        }
        z = v_0_0.Args[1];
        y = v_0_0.Args[0];
        v.reset(OpS390XFMSUB);
        v.AddArg3(x, y, z);
        return true;
    } 
    // match: (Select0 (FADDS (FMULS y z) x))
    // result: (FMADDS x y z)
    while (true) {
        if (v_0.Op != OpS390XFADDS) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpS390XFMULS) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                z = v_0_0.Args[1];
                y = v_0_0.Args[0];
                x = v_0_1;
                v.reset(OpS390XFMADDS);
                v.AddArg3(x, y, z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Select0 (FSUBS (FMULS y z) x))
    // result: (FMSUBS x y z)
    while (true) {
        if (v_0.Op != OpS390XFSUBS) {
            break;
        }
        x = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XFMULS) {
            break;
        }
        z = v_0_0.Args[1];
        y = v_0_0.Args[0];
        v.reset(OpS390XFMSUBS);
        v.AddArg3(x, y, z);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpSelect1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select1 (Add64carry x y c))
    // result: (Select0 <typ.UInt64> (ADDE (MOVDconst [0]) (MOVDconst [0]) (Select1 <types.TypeFlags> (ADDE x y (Select1 <types.TypeFlags> (ADDCconst c [-1]))))))
    while (true) {
        if (v_0.Op != OpAdd64carry) {
            break;
        }
        var c = v_0.Args[2];
        var x = v_0.Args[0];
        var y = v_0.Args[1];
        v.reset(OpSelect0);
        v.Type = typ.UInt64;
        var v0 = b.NewValue0(v.Pos, OpS390XADDE, types.NewTuple(typ.UInt64, types.TypeFlags));
        var v1 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v1.AuxInt = int64ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpS390XADDE, types.NewTuple(typ.UInt64, types.TypeFlags));
        var v4 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v5 = b.NewValue0(v.Pos, OpS390XADDCconst, types.NewTuple(typ.UInt64, types.TypeFlags));
        v5.AuxInt = int16ToAuxInt(-1);
        v5.AddArg(c);
        v4.AddArg(v5);
        v3.AddArg3(x, y, v4);
        v2.AddArg(v3);
        v0.AddArg3(v1, v1, v2);
        v.AddArg(v0);
        return true;
    } 
    // match: (Select1 (Sub64borrow x y c))
    // result: (NEG (Select0 <typ.UInt64> (SUBE (MOVDconst [0]) (MOVDconst [0]) (Select1 <types.TypeFlags> (SUBE x y (Select1 <types.TypeFlags> (SUBC (MOVDconst [0]) c)))))))
    while (true) {
        if (v_0.Op != OpSub64borrow) {
            break;
        }
        c = v_0.Args[2];
        x = v_0.Args[0];
        y = v_0.Args[1];
        v.reset(OpS390XNEG);
        v0 = b.NewValue0(v.Pos, OpSelect0, typ.UInt64);
        v1 = b.NewValue0(v.Pos, OpS390XSUBE, types.NewTuple(typ.UInt64, types.TypeFlags));
        v2 = b.NewValue0(v.Pos, OpS390XMOVDconst, typ.UInt64);
        v2.AuxInt = int64ToAuxInt(0);
        v3 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        v4 = b.NewValue0(v.Pos, OpS390XSUBE, types.NewTuple(typ.UInt64, types.TypeFlags));
        v5 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v6 = b.NewValue0(v.Pos, OpS390XSUBC, types.NewTuple(typ.UInt64, types.TypeFlags));
        v6.AddArg2(v2, c);
        v5.AddArg(v6);
        v4.AddArg3(x, y, v5);
        v3.AddArg(v4);
        v1.AddArg3(v2, v2, v3);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    } 
    // match: (Select1 (AddTupleFirst32 _ tuple))
    // result: (Select1 tuple)
    while (true) {
        if (v_0.Op != OpS390XAddTupleFirst32) {
            break;
        }
        var tuple = v_0.Args[1];
        v.reset(OpSelect1);
        v.AddArg(tuple);
        return true;
    } 
    // match: (Select1 (AddTupleFirst64 _ tuple))
    // result: (Select1 tuple)
    while (true) {
        if (v_0.Op != OpS390XAddTupleFirst64) {
            break;
        }
        tuple = v_0.Args[1];
        v.reset(OpSelect1);
        v.AddArg(tuple);
        return true;
    } 
    // match: (Select1 (ADDCconst (MOVDconst [c]) [d]))
    // cond: uint64(c+int64(d)) >= uint64(c) && c+int64(d) == 0
    // result: (FlagEQ)
    while (true) {
        if (v_0.Op != OpS390XADDCconst) {
            break;
        }
        var d = auxIntToInt16(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        if (!(uint64(c + int64(d)) >= uint64(c) && c + int64(d) == 0)) {
            break;
        }
        v.reset(OpS390XFlagEQ);
        return true;
    } 
    // match: (Select1 (ADDCconst (MOVDconst [c]) [d]))
    // cond: uint64(c+int64(d)) >= uint64(c) && c+int64(d) != 0
    // result: (FlagLT)
    while (true) {
        if (v_0.Op != OpS390XADDCconst) {
            break;
        }
        d = auxIntToInt16(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        if (!(uint64(c + int64(d)) >= uint64(c) && c + int64(d) != 0)) {
            break;
        }
        v.reset(OpS390XFlagLT);
        return true;
    } 
    // match: (Select1 (SUBC (MOVDconst [c]) (MOVDconst [d])))
    // cond: uint64(d) <= uint64(c) && c-d == 0
    // result: (FlagGT)
    while (true) {
        if (v_0.Op != OpS390XSUBC) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0_1.AuxInt);
        if (!(uint64(d) <= uint64(c) && c - d == 0)) {
            break;
        }
        v.reset(OpS390XFlagGT);
        return true;
    } 
    // match: (Select1 (SUBC (MOVDconst [c]) (MOVDconst [d])))
    // cond: uint64(d) <= uint64(c) && c-d != 0
    // result: (FlagOV)
    while (true) {
        if (v_0.Op != OpS390XSUBC) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpS390XMOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpS390XMOVDconst) {
            break;
        }
        d = auxIntToInt64(v_0_1.AuxInt);
        if (!(uint64(d) <= uint64(c) && c - d != 0)) {
            break;
        }
        v.reset(OpS390XFlagOV);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Slicemask <t> x)
    // result: (SRADconst (NEG <t> x) [63])
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpS390XSRADconst);
        v.AuxInt = uint8ToAuxInt(63);
        var v0 = b.NewValue0(v.Pos, OpS390XNEG, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && is64BitFloat(val.Type)
    // result: (FMOVDstore ptr val mem)
    while (true) {
        var t = auxToType(v.Aux);
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(t.Size() == 8 && is64BitFloat(val.Type))) {
            break;
        }
        v.reset(OpS390XFMOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && is32BitFloat(val.Type)
    // result: (FMOVSstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpS390XFMOVSstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8
    // result: (MOVDstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8)) {
            break;
        }
        v.reset(OpS390XMOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4
    // result: (MOVWstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4)) {
            break;
        }
        v.reset(OpS390XMOVWstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 2
    // result: (MOVHstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 2)) {
            break;
        }
        v.reset(OpS390XMOVHstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 1
    // result: (MOVBstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 1)) {
            break;
        }
        v.reset(OpS390XMOVBstore);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValueS390X_OpSub32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Sub32F x y)
    // result: (Select0 (FSUBS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpS390XFSUBS, types.NewTuple(typ.Float32, types.TypeFlags));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpSub64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Sub64F x y)
    // result: (Select0 (FSUB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpS390XFSUB, types.NewTuple(typ.Float64, types.TypeFlags));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValueS390X_OpTrunc(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc x)
    // result: (FIDBR [5] x)
    while (true) {
        var x = v_0;
        v.reset(OpS390XFIDBR);
        v.AuxInt = int8ToAuxInt(5);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValueS390X_OpZero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
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
    // result: (MOVBstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [2] destptr mem)
    // result: (MOVHstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [4] destptr mem)
    // result: (MOVWstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [8] destptr mem)
    // result: (MOVDstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVDstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [3] destptr mem)
    // result: (MOVBstoreconst [makeValAndOff(0,2)] destptr (MOVHstoreconst [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 2));
        var v0 = b.NewValue0(v.Pos, OpS390XMOVHstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [5] destptr mem)
    // result: (MOVBstoreconst [makeValAndOff(0,4)] destptr (MOVWstoreconst [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v0 = b.NewValue0(v.Pos, OpS390XMOVWstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [6] destptr mem)
    // result: (MOVHstoreconst [makeValAndOff(0,4)] destptr (MOVWstoreconst [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVHstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v0 = b.NewValue0(v.Pos, OpS390XMOVWstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [7] destptr mem)
    // result: (MOVWstoreconst [makeValAndOff(0,3)] destptr (MOVWstoreconst [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpS390XMOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 3));
        v0 = b.NewValue0(v.Pos, OpS390XMOVWstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s > 0 && s <= 1024
    // result: (CLEAR [makeValAndOff(int32(s), 0)] destptr mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s > 0 && s <= 1024)) {
            break;
        }
        v.reset(OpS390XCLEAR);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(s), 0));
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s > 1024
    // result: (LoweredZero [s%256] destptr (ADDconst <destptr.Type> destptr [(int32(s)/256)*256]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s > 1024)) {
            break;
        }
        v.reset(OpS390XLoweredZero);
        v.AuxInt = int64ToAuxInt(s % 256);
        v0 = b.NewValue0(v.Pos, OpS390XADDconst, destptr.Type);
        v0.AuxInt = int32ToAuxInt((int32(s) / 256) * 256);
        v0.AddArg(destptr);
        v.AddArg3(destptr, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteBlockS390X(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;

    var typ = _addr_b.Func.Config.Types;

    if (b.Kind == BlockS390XBRC) 
        // match: (BRC {c} x:(CMP _ _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMP) {
            var x = b.Controls[0];
            var c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPW _ _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPW) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPU _ _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPU) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPWU _ _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPWU) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPconst _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPWconst _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPUconst _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPUconst) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} x:(CMPWUconst _) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (BRC {c&^s390x.Unordered} x yes no)
        while (b.Controls[0].Op == OpS390XCMPWUconst) {
            x = b.Controls[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, x);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMP x y) yes no)
        // result: (CGRJ {c&^s390x.Unordered} x y yes no)
        while (b.Controls[0].Op == OpS390XCMP) {
            var v_0 = b.Controls[0];
            var y = v_0.Args[1];
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl2(BlockS390XCGRJ, x, y);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPW x y) yes no)
        // result: (CRJ {c&^s390x.Unordered} x y yes no)
        while (b.Controls[0].Op == OpS390XCMPW) {
            v_0 = b.Controls[0];
            y = v_0.Args[1];
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl2(BlockS390XCRJ, x, y);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPU x y) yes no)
        // result: (CLGRJ {c&^s390x.Unordered} x y yes no)
        while (b.Controls[0].Op == OpS390XCMPU) {
            v_0 = b.Controls[0];
            y = v_0.Args[1];
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl2(BlockS390XCLGRJ, x, y);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPWU x y) yes no)
        // result: (CLRJ {c&^s390x.Unordered} x y yes no)
        while (b.Controls[0].Op == OpS390XCMPWU) {
            v_0 = b.Controls[0];
            y = v_0.Args[1];
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl2(BlockS390XCLRJ, x, y);
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPconst x [y]) yes no)
        // cond: y == int32( int8(y))
        // result: (CGIJ {c&^s390x.Unordered} x [ int8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(int8(y)))) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPWconst x [y]) yes no)
        // cond: y == int32( int8(y))
        // result: (CIJ {c&^s390x.Unordered} x [ int8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(int8(y)))) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPUconst x [y]) yes no)
        // cond: y == int32(uint8(y))
        // result: (CLGIJ {c&^s390x.Unordered} x [uint8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPUconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(uint8(y)))) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {c} (CMPWUconst x [y]) yes no)
        // cond: y == int32(uint8(y))
        // result: (CLIJ {c&^s390x.Unordered} x [uint8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPWUconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(uint8(y)))) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c & ~s390x.Unordered);
            return true;
        } 
        // match: (BRC {s390x.Less} (CMPconst x [ 128]) yes no)
        // result: (CGIJ {s390x.LessOrEqual} x [ 127] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 128) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(127);
            b.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
            return true;
        } 
        // match: (BRC {s390x.Less} (CMPWconst x [ 128]) yes no)
        // result: (CIJ {s390x.LessOrEqual} x [ 127] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 128) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(127);
            b.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
            return true;
        } 
        // match: (BRC {s390x.LessOrEqual} (CMPconst x [-129]) yes no)
        // result: (CGIJ {s390x.Less} x [-128] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != -129) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.LessOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(-128);
            b.Aux = s390xCCMaskToAux(s390x.Less);
            return true;
        } 
        // match: (BRC {s390x.LessOrEqual} (CMPWconst x [-129]) yes no)
        // result: (CIJ {s390x.Less} x [-128] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != -129) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.LessOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(-128);
            b.Aux = s390xCCMaskToAux(s390x.Less);
            return true;
        } 
        // match: (BRC {s390x.Greater} (CMPconst x [-129]) yes no)
        // result: (CGIJ {s390x.GreaterOrEqual} x [-128] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != -129) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(-128);
            b.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
            return true;
        } 
        // match: (BRC {s390x.Greater} (CMPWconst x [-129]) yes no)
        // result: (CIJ {s390x.GreaterOrEqual} x [-128] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != -129) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(-128);
            b.Aux = s390xCCMaskToAux(s390x.GreaterOrEqual);
            return true;
        } 
        // match: (BRC {s390x.GreaterOrEqual} (CMPconst x [ 128]) yes no)
        // result: (CGIJ {s390x.Greater} x [ 127] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 128) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(127);
            b.Aux = s390xCCMaskToAux(s390x.Greater);
            return true;
        } 
        // match: (BRC {s390x.GreaterOrEqual} (CMPWconst x [ 128]) yes no)
        // result: (CIJ {s390x.Greater} x [ 127] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 128) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(127);
            b.Aux = s390xCCMaskToAux(s390x.Greater);
            return true;
        } 
        // match: (BRC {s390x.Less} (CMPWUconst x [256]) yes no)
        // result: (CLIJ {s390x.LessOrEqual} x [255] yes no)
        while (b.Controls[0].Op == OpS390XCMPWUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 256) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(255);
            b.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
            return true;
        } 
        // match: (BRC {s390x.Less} (CMPUconst x [256]) yes no)
        // result: (CLGIJ {s390x.LessOrEqual} x [255] yes no)
        while (b.Controls[0].Op == OpS390XCMPUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 256) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, x);
            b.AuxInt = uint8ToAuxInt(255);
            b.Aux = s390xCCMaskToAux(s390x.LessOrEqual);
            return true;
        } 
        // match: (BRC {s390x.GreaterOrEqual} (CMPWUconst x [256]) yes no)
        // result: (CLIJ {s390x.Greater} x [255] yes no)
        while (b.Controls[0].Op == OpS390XCMPWUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 256) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(255);
            b.Aux = s390xCCMaskToAux(s390x.Greater);
            return true;
        } 
        // match: (BRC {s390x.GreaterOrEqual} (CMPUconst x [256]) yes no)
        // result: (CLGIJ {s390x.Greater} x [255] yes no)
        while (b.Controls[0].Op == OpS390XCMPUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 256) {
                break;
            }
            x = v_0.Args[0];
            if (auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, x);
            b.AuxInt = uint8ToAuxInt(255);
            b.Aux = s390xCCMaskToAux(s390x.Greater);
            return true;
        } 
        // match: (BRC {c} (CMPconst x [y]) yes no)
        // cond: y == int32(uint8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater)
        // result: (CLGIJ {c} x [uint8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(uint8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater))) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (BRC {c} (CMPWconst x [y]) yes no)
        // cond: y == int32(uint8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater)
        // result: (CLIJ {c} x [uint8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPWconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(uint8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater))) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (BRC {c} (CMPUconst x [y]) yes no)
        // cond: y == int32( int8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater)
        // result: (CGIJ {c} x [ int8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPUconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(int8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater))) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (BRC {c} (CMPWUconst x [y]) yes no)
        // cond: y == int32( int8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater)
        // result: (CIJ {c} x [ int8(y)] yes no)
        while (b.Controls[0].Op == OpS390XCMPWUconst) {
            v_0 = b.Controls[0];
            y = auxIntToInt32(v_0.AuxInt);
            x = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            if (!(y == int32(int8(y)) && (c == s390x.Equal || c == s390x.LessOrGreater))) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (BRC {c} (InvertFlags cmp) yes no)
        // result: (BRC {c.ReverseComparison()} cmp yes no)
        while (b.Controls[0].Op == OpS390XInvertFlags) {
            v_0 = b.Controls[0];
            var cmp = v_0.Args[0];
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl(BlockS390XBRC, cmp);
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (BRC {c} (FlagEQ) yes no)
        // cond: c&s390x.Equal != 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XFlagEQ) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (BRC {c} (FlagLT) yes no)
        // cond: c&s390x.Less != 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XFlagLT) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (BRC {c} (FlagGT) yes no)
        // cond: c&s390x.Greater != 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XFlagGT) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (BRC {c} (FlagOV) yes no)
        // cond: c&s390x.Unordered != 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XFlagOV) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (BRC {c} (FlagEQ) yes no)
        // cond: c&s390x.Equal == 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XFlagEQ) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (BRC {c} (FlagLT) yes no)
        // cond: c&s390x.Less == 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XFlagLT) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (BRC {c} (FlagGT) yes no)
        // cond: c&s390x.Greater == 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XFlagGT) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (BRC {c} (FlagOV) yes no)
        // cond: c&s390x.Unordered == 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XFlagOV) {
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Unordered == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCGIJ) 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal != 0 && int64(x) == int64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal != 0 && int64(x) == int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less != 0 && int64(x) < int64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less != 0 && int64(x) < int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater != 0 && int64(x) > int64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater != 0 && int64(x) > int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal == 0 && int64(x) == int64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal == 0 && int64(x) == int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less == 0 && int64(x) < int64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less == 0 && int64(x) < int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater == 0 && int64(x) > int64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater == 0 && int64(x) > int64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CGIJ {s390x.Equal} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.NoCarry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            var v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            var carry = v_0_0.Args[2];
            var v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            var v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.NoCarry);
            return true;
        } 
        // match: (CGIJ {s390x.Equal} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [1])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CGIJ {s390x.LessOrGreater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CGIJ {s390x.LessOrGreater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [1])
        // result: (BRC {s390x.NoCarry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.NoCarry);
            return true;
        } 
        // match: (CGIJ {s390x.Greater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CGIJ {s390x.Equal} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.NoBorrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            var borrow = v_0_0_0.Args[2];
            var v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            var v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.NoBorrow);
            return true;
        } 
        // match: (CGIJ {s390x.Equal} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [1])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        } 
        // match: (CGIJ {s390x.LessOrGreater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        } 
        // match: (CGIJ {s390x.LessOrGreater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [1])
        // result: (BRC {s390x.NoBorrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.NoBorrow);
            return true;
        } 
        // match: (CGIJ {s390x.Greater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToInt8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        }
    else if (b.Kind == BlockS390XCGRJ) 
        // match: (CGRJ {c} x (MOVDconst [y]) yes no)
        // cond: is8Bit(y)
        // result: (CGIJ {c} x [ int8(y)] yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            var v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(is8Bit(y))) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CGRJ {c} (MOVDconst [x]) y yes no)
        // cond: is8Bit(x)
        // result: (CGIJ {c.ReverseComparison()} y [ int8(x)] yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(is8Bit(x))) {
                break;
            }
            b.resetWithControl(BlockS390XCGIJ, y);
            b.AuxInt = int8ToAuxInt(int8(x));
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CGRJ {c} x (MOVDconst [y]) yes no)
        // cond: !is8Bit(y) && is32Bit(y)
        // result: (BRC {c} (CMPconst x [int32(y)]) yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(!is8Bit(y) && is32Bit(y))) {
                break;
            }
            var v0 = b.NewValue0(x.Pos, OpS390XCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(y));
            v0.AddArg(x);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CGRJ {c} (MOVDconst [x]) y yes no)
        // cond: !is8Bit(x) && is32Bit(x)
        // result: (BRC {c.ReverseComparison()} (CMPconst y [int32(x)]) yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(!is8Bit(x) && is32Bit(x))) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpS390XCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(x));
            v0.AddArg(y);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CGRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal != 0
        // result: (First yes no)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CGRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal == 0
        // result: (First no yes)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCIJ) 
        // match: (CIJ {c} (MOVWreg x) [y] yes no)
        // result: (CIJ {c} x [y] yes no)
        while (b.Controls[0].Op == OpS390XMOVWreg) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(y);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CIJ {c} (MOVWZreg x) [y] yes no)
        // result: (CIJ {c} x [y] yes no)
        while (b.Controls[0].Op == OpS390XMOVWZreg) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(y);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal != 0 && int32(x) == int32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal != 0 && int32(x) == int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less != 0 && int32(x) < int32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less != 0 && int32(x) < int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater != 0 && int32(x) > int32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater != 0 && int32(x) > int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal == 0 && int32(x) == int32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal == 0 && int32(x) == int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less == 0 && int32(x) < int32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less == 0 && int32(x) < int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater == 0 && int32(x) > int32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToInt8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater == 0 && int32(x) > int32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCLGIJ) 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal != 0 && uint64(x) == uint64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal != 0 && uint64(x) == uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less != 0 && uint64(x) < uint64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less != 0 && uint64(x) < uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater != 0 && uint64(x) > uint64(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater != 0 && uint64(x) > uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal == 0 && uint64(x) == uint64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal == 0 && uint64(x) == uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less == 0 && uint64(x) < uint64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less == 0 && uint64(x) < uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLGIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater == 0 && uint64(x) > uint64(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater == 0 && uint64(x) > uint64(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLGIJ {s390x.GreaterOrEqual} _ [0] yes no)
        // result: (First yes no)
        while (true) {
            if (auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLGIJ {s390x.Less} _ [0] yes no)
        // result: (First no yes)
        while (true) {
            if (auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLGIJ {s390x.Equal} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.NoCarry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.NoCarry);
            return true;
        } 
        // match: (CLGIJ {s390x.Equal} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [1])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CLGIJ {s390x.LessOrGreater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CLGIJ {s390x.LessOrGreater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [1])
        // result: (BRC {s390x.NoCarry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.NoCarry);
            return true;
        } 
        // match: (CLGIJ {s390x.Greater} (Select0 (ADDE (MOVDconst [0]) (MOVDconst [0]) carry)) [0])
        // result: (BRC {s390x.Carry} carry)
        while (b.Controls[0].Op == OpSelect0) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XADDE) {
                break;
            }
            carry = v_0_0.Args[2];
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_1 = v_0_0.Args[1];
            if (v_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, carry);
            b.Aux = s390xCCMaskToAux(s390x.Carry);
            return true;
        } 
        // match: (CLGIJ {s390x.Equal} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.NoBorrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.NoBorrow);
            return true;
        } 
        // match: (CLGIJ {s390x.Equal} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [1])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.Equal) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        } 
        // match: (CLGIJ {s390x.LessOrGreater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        } 
        // match: (CLGIJ {s390x.LessOrGreater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [1])
        // result: (BRC {s390x.NoBorrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 1 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.NoBorrow);
            return true;
        } 
        // match: (CLGIJ {s390x.Greater} (NEG (Select0 (SUBE (MOVDconst [0]) (MOVDconst [0]) borrow))) [0])
        // result: (BRC {s390x.Borrow} borrow)
        while (b.Controls[0].Op == OpS390XNEG) {
            v_0 = b.Controls[0];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpSelect0) {
                break;
            }
            v_0_0_0 = v_0_0.Args[0];
            if (v_0_0_0.Op != OpS390XSUBE) {
                break;
            }
            borrow = v_0_0_0.Args[2];
            v_0_0_0_0 = v_0_0_0.Args[0];
            if (v_0_0_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_0.AuxInt) != 0) {
                break;
            }
            v_0_0_0_1 = v_0_0_0.Args[1];
            if (v_0_0_0_1.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0_0_1.AuxInt) != 0 || auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Greater) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, borrow);
            b.Aux = s390xCCMaskToAux(s390x.Borrow);
            return true;
        }
    else if (b.Kind == BlockS390XCLGRJ) 
        // match: (CLGRJ {c} x (MOVDconst [y]) yes no)
        // cond: isU8Bit(y)
        // result: (CLGIJ {c} x [uint8(y)] yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(isU8Bit(y))) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLGRJ {c} (MOVDconst [x]) y yes no)
        // cond: isU8Bit(x)
        // result: (CLGIJ {c.ReverseComparison()} y [uint8(x)] yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(isU8Bit(x))) {
                break;
            }
            b.resetWithControl(BlockS390XCLGIJ, y);
            b.AuxInt = uint8ToAuxInt(uint8(x));
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CLGRJ {c} x (MOVDconst [y]) yes no)
        // cond: !isU8Bit(y) && isU32Bit(y)
        // result: (BRC {c} (CMPUconst x [int32(y)]) yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(!isU8Bit(y) && isU32Bit(y))) {
                break;
            }
            v0 = b.NewValue0(x.Pos, OpS390XCMPUconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(y));
            v0.AddArg(x);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLGRJ {c} (MOVDconst [x]) y yes no)
        // cond: !isU8Bit(x) && isU32Bit(x)
        // result: (BRC {c.ReverseComparison()} (CMPUconst y [int32(x)]) yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(!isU8Bit(x) && isU32Bit(x))) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpS390XCMPUconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(x));
            v0.AddArg(y);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CLGRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal != 0
        // result: (First yes no)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLGRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal == 0
        // result: (First no yes)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCLIJ) 
        // match: (CLIJ {s390x.LessOrGreater} (LOCGR {d} (MOVDconst [0]) (MOVDconst [x]) cmp) [0] yes no)
        // cond: int32(x) != 0
        // result: (BRC {d} cmp yes no)
        while (b.Controls[0].Op == OpS390XLOCGR) {
            v_0 = b.Controls[0];
            var d = auxToS390xCCMask(v_0.Aux);
            cmp = v_0.Args[2];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpS390XMOVDconst || auxIntToInt64(v_0_0.AuxInt) != 0) {
                break;
            }
            var v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpS390XMOVDconst) {
                break;
            }
            x = auxIntToInt64(v_0_1.AuxInt);
            if (auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.LessOrGreater || !(int32(x) != 0)) {
                break;
            }
            b.resetWithControl(BlockS390XBRC, cmp);
            b.Aux = s390xCCMaskToAux(d);
            return true;
        } 
        // match: (CLIJ {c} (MOVWreg x) [y] yes no)
        // result: (CLIJ {c} x [y] yes no)
        while (b.Controls[0].Op == OpS390XMOVWreg) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(y);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLIJ {c} (MOVWZreg x) [y] yes no)
        // result: (CLIJ {c} x [y] yes no)
        while (b.Controls[0].Op == OpS390XMOVWZreg) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(y);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal != 0 && uint32(x) == uint32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal != 0 && uint32(x) == uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less != 0 && uint32(x) < uint32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less != 0 && uint32(x) < uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater != 0 && uint32(x) > uint32(y)
        // result: (First yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater != 0 && uint32(x) > uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Equal == 0 && uint32(x) == uint32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Equal == 0 && uint32(x) == uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Less == 0 && uint32(x) < uint32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Less == 0 && uint32(x) < uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLIJ {c} (MOVDconst [x]) [y] yes no)
        // cond: c&s390x.Greater == 0 && uint32(x) > uint32(y)
        // result: (First no yes)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = auxIntToUint8(b.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(c & s390x.Greater == 0 && uint32(x) > uint32(y))) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (CLIJ {s390x.GreaterOrEqual} _ [0] yes no)
        // result: (First yes no)
        while (true) {
            if (auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.GreaterOrEqual) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLIJ {s390x.Less} _ [0] yes no)
        // result: (First no yes)
        while (true) {
            if (auxIntToUint8(b.AuxInt) != 0 || auxToS390xCCMask(b.Aux) != s390x.Less) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCLRJ) 
        // match: (CLRJ {c} x (MOVDconst [y]) yes no)
        // cond: isU8Bit(y)
        // result: (CLIJ {c} x [uint8(y)] yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(isU8Bit(y))) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, x);
            b.AuxInt = uint8ToAuxInt(uint8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLRJ {c} (MOVDconst [x]) y yes no)
        // cond: isU8Bit(x)
        // result: (CLIJ {c.ReverseComparison()} y [uint8(x)] yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(isU8Bit(x))) {
                break;
            }
            b.resetWithControl(BlockS390XCLIJ, y);
            b.AuxInt = uint8ToAuxInt(uint8(x));
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CLRJ {c} x (MOVDconst [y]) yes no)
        // cond: !isU8Bit(y) && isU32Bit(y)
        // result: (BRC {c} (CMPWUconst x [int32(y)]) yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(!isU8Bit(y) && isU32Bit(y))) {
                break;
            }
            v0 = b.NewValue0(x.Pos, OpS390XCMPWUconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(y));
            v0.AddArg(x);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CLRJ {c} (MOVDconst [x]) y yes no)
        // cond: !isU8Bit(x) && isU32Bit(x)
        // result: (BRC {c.ReverseComparison()} (CMPWUconst y [int32(x)]) yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(!isU8Bit(x) && isU32Bit(x))) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpS390XCMPWUconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(x));
            v0.AddArg(y);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CLRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal != 0
        // result: (First yes no)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CLRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal == 0
        // result: (First no yes)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockS390XCRJ) 
        // match: (CRJ {c} x (MOVDconst [y]) yes no)
        // cond: is8Bit(y)
        // result: (CIJ {c} x [ int8(y)] yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(is8Bit(y))) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, x);
            b.AuxInt = int8ToAuxInt(int8(y));
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CRJ {c} (MOVDconst [x]) y yes no)
        // cond: is8Bit(x)
        // result: (CIJ {c.ReverseComparison()} y [ int8(x)] yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(is8Bit(x))) {
                break;
            }
            b.resetWithControl(BlockS390XCIJ, y);
            b.AuxInt = int8ToAuxInt(int8(x));
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CRJ {c} x (MOVDconst [y]) yes no)
        // cond: !is8Bit(y) && is32Bit(y)
        // result: (BRC {c} (CMPWconst x [int32(y)]) yes no)
        while (b.Controls[1].Op == OpS390XMOVDconst) {
            x = b.Controls[0];
            v_1 = b.Controls[1];
            y = auxIntToInt64(v_1.AuxInt);
            c = auxToS390xCCMask(b.Aux);
            if (!(!is8Bit(y) && is32Bit(y))) {
                break;
            }
            v0 = b.NewValue0(x.Pos, OpS390XCMPWconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(y));
            v0.AddArg(x);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c);
            return true;
        } 
        // match: (CRJ {c} (MOVDconst [x]) y yes no)
        // cond: !is8Bit(x) && is32Bit(x)
        // result: (BRC {c.ReverseComparison()} (CMPWconst y [int32(x)]) yes no)
        while (b.Controls[0].Op == OpS390XMOVDconst) {
            v_0 = b.Controls[0];
            x = auxIntToInt64(v_0.AuxInt);
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(!is8Bit(x) && is32Bit(x))) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpS390XCMPWconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(int32(x));
            v0.AddArg(y);
            b.resetWithControl(BlockS390XBRC, v0);
            b.Aux = s390xCCMaskToAux(c.ReverseComparison());
            return true;
        } 
        // match: (CRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal != 0
        // result: (First yes no)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (CRJ {c} x y yes no)
        // cond: x == y && c&s390x.Equal == 0
        // result: (First no yes)
        while (true) {
            x = b.Controls[0];
            y = b.Controls[1];
            c = auxToS390xCCMask(b.Aux);
            if (!(x == y && c & s390x.Equal == 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockIf) 
        // match: (If cond yes no)
        // result: (CLIJ {s390x.LessOrGreater} (MOVBZreg <typ.Bool> cond) [0] yes no)
        while (true) {
            var cond = b.Controls[0];
            v0 = b.NewValue0(cond.Pos, OpS390XMOVBZreg, typ.Bool);
            v0.AddArg(cond);
            b.resetWithControl(BlockS390XCLIJ, v0);
            b.AuxInt = uint8ToAuxInt(0);
            b.Aux = s390xCCMaskToAux(s390x.LessOrGreater);
            return true;
        }
        return false;
}

} // end ssa_package
