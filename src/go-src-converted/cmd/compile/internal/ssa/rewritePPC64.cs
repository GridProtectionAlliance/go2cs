// Code generated from gen/PPC64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 13 06:18:45 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewritePPC64.go
namespace go.cmd.compile.@internal;

using buildcfg = @internal.buildcfg_package;
using math = math_package;
using types = cmd.compile.@internal.types_package;

public static partial class ssa_package {

private static bool rewriteValuePPC64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAbs) 
        v.Op = OpPPC64FABS;
        return true;
    else if (v.Op == OpAdd16) 
        v.Op = OpPPC64ADD;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = OpPPC64ADD;
        return true;
    else if (v.Op == OpAdd32F) 
        v.Op = OpPPC64FADDS;
        return true;
    else if (v.Op == OpAdd64) 
        v.Op = OpPPC64ADD;
        return true;
    else if (v.Op == OpAdd64F) 
        v.Op = OpPPC64FADD;
        return true;
    else if (v.Op == OpAdd64carry) 
        v.Op = OpPPC64LoweredAdd64Carry;
        return true;
    else if (v.Op == OpAdd8) 
        v.Op = OpPPC64ADD;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = OpPPC64ADD;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValuePPC64_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = OpPPC64AND;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = OpPPC64AND;
        return true;
    else if (v.Op == OpAnd64) 
        v.Op = OpPPC64AND;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = OpPPC64AND;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = OpPPC64AND;
        return true;
    else if (v.Op == OpAtomicAdd32) 
        v.Op = OpPPC64LoweredAtomicAdd32;
        return true;
    else if (v.Op == OpAtomicAdd64) 
        v.Op = OpPPC64LoweredAtomicAdd64;
        return true;
    else if (v.Op == OpAtomicAnd32) 
        v.Op = OpPPC64LoweredAtomicAnd32;
        return true;
    else if (v.Op == OpAtomicAnd8) 
        v.Op = OpPPC64LoweredAtomicAnd8;
        return true;
    else if (v.Op == OpAtomicCompareAndSwap32) 
        return rewriteValuePPC64_OpAtomicCompareAndSwap32(_addr_v);
    else if (v.Op == OpAtomicCompareAndSwap64) 
        return rewriteValuePPC64_OpAtomicCompareAndSwap64(_addr_v);
    else if (v.Op == OpAtomicCompareAndSwapRel32) 
        return rewriteValuePPC64_OpAtomicCompareAndSwapRel32(_addr_v);
    else if (v.Op == OpAtomicExchange32) 
        v.Op = OpPPC64LoweredAtomicExchange32;
        return true;
    else if (v.Op == OpAtomicExchange64) 
        v.Op = OpPPC64LoweredAtomicExchange64;
        return true;
    else if (v.Op == OpAtomicLoad32) 
        return rewriteValuePPC64_OpAtomicLoad32(_addr_v);
    else if (v.Op == OpAtomicLoad64) 
        return rewriteValuePPC64_OpAtomicLoad64(_addr_v);
    else if (v.Op == OpAtomicLoad8) 
        return rewriteValuePPC64_OpAtomicLoad8(_addr_v);
    else if (v.Op == OpAtomicLoadAcq32) 
        return rewriteValuePPC64_OpAtomicLoadAcq32(_addr_v);
    else if (v.Op == OpAtomicLoadAcq64) 
        return rewriteValuePPC64_OpAtomicLoadAcq64(_addr_v);
    else if (v.Op == OpAtomicLoadPtr) 
        return rewriteValuePPC64_OpAtomicLoadPtr(_addr_v);
    else if (v.Op == OpAtomicOr32) 
        v.Op = OpPPC64LoweredAtomicOr32;
        return true;
    else if (v.Op == OpAtomicOr8) 
        v.Op = OpPPC64LoweredAtomicOr8;
        return true;
    else if (v.Op == OpAtomicStore32) 
        return rewriteValuePPC64_OpAtomicStore32(_addr_v);
    else if (v.Op == OpAtomicStore64) 
        return rewriteValuePPC64_OpAtomicStore64(_addr_v);
    else if (v.Op == OpAtomicStore8) 
        return rewriteValuePPC64_OpAtomicStore8(_addr_v);
    else if (v.Op == OpAtomicStoreRel32) 
        return rewriteValuePPC64_OpAtomicStoreRel32(_addr_v);
    else if (v.Op == OpAtomicStoreRel64) 
        return rewriteValuePPC64_OpAtomicStoreRel64(_addr_v);
    else if (v.Op == OpAvg64u) 
        return rewriteValuePPC64_OpAvg64u(_addr_v);
    else if (v.Op == OpBitLen32) 
        return rewriteValuePPC64_OpBitLen32(_addr_v);
    else if (v.Op == OpBitLen64) 
        return rewriteValuePPC64_OpBitLen64(_addr_v);
    else if (v.Op == OpCeil) 
        v.Op = OpPPC64FCEIL;
        return true;
    else if (v.Op == OpClosureCall) 
        v.Op = OpPPC64CALLclosure;
        return true;
    else if (v.Op == OpCom16) 
        return rewriteValuePPC64_OpCom16(_addr_v);
    else if (v.Op == OpCom32) 
        return rewriteValuePPC64_OpCom32(_addr_v);
    else if (v.Op == OpCom64) 
        return rewriteValuePPC64_OpCom64(_addr_v);
    else if (v.Op == OpCom8) 
        return rewriteValuePPC64_OpCom8(_addr_v);
    else if (v.Op == OpCondSelect) 
        return rewriteValuePPC64_OpCondSelect(_addr_v);
    else if (v.Op == OpConst16) 
        return rewriteValuePPC64_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        return rewriteValuePPC64_OpConst32(_addr_v);
    else if (v.Op == OpConst32F) 
        v.Op = OpPPC64FMOVSconst;
        return true;
    else if (v.Op == OpConst64) 
        return rewriteValuePPC64_OpConst64(_addr_v);
    else if (v.Op == OpConst64F) 
        v.Op = OpPPC64FMOVDconst;
        return true;
    else if (v.Op == OpConst8) 
        return rewriteValuePPC64_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValuePPC64_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValuePPC64_OpConstNil(_addr_v);
    else if (v.Op == OpCopysign) 
        return rewriteValuePPC64_OpCopysign(_addr_v);
    else if (v.Op == OpCtz16) 
        return rewriteValuePPC64_OpCtz16(_addr_v);
    else if (v.Op == OpCtz32) 
        return rewriteValuePPC64_OpCtz32(_addr_v);
    else if (v.Op == OpCtz32NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCtz64) 
        return rewriteValuePPC64_OpCtz64(_addr_v);
    else if (v.Op == OpCtz64NonZero) 
        v.Op = OpCtz64;
        return true;
    else if (v.Op == OpCtz8) 
        return rewriteValuePPC64_OpCtz8(_addr_v);
    else if (v.Op == OpCvt32Fto32) 
        return rewriteValuePPC64_OpCvt32Fto32(_addr_v);
    else if (v.Op == OpCvt32Fto64) 
        return rewriteValuePPC64_OpCvt32Fto64(_addr_v);
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpCvt32to32F) 
        return rewriteValuePPC64_OpCvt32to32F(_addr_v);
    else if (v.Op == OpCvt32to64F) 
        return rewriteValuePPC64_OpCvt32to64F(_addr_v);
    else if (v.Op == OpCvt64Fto32) 
        return rewriteValuePPC64_OpCvt64Fto32(_addr_v);
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = OpPPC64FRSP;
        return true;
    else if (v.Op == OpCvt64Fto64) 
        return rewriteValuePPC64_OpCvt64Fto64(_addr_v);
    else if (v.Op == OpCvt64to32F) 
        return rewriteValuePPC64_OpCvt64to32F(_addr_v);
    else if (v.Op == OpCvt64to64F) 
        return rewriteValuePPC64_OpCvt64to64F(_addr_v);
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        return rewriteValuePPC64_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValuePPC64_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValuePPC64_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        v.Op = OpPPC64FDIVS;
        return true;
    else if (v.Op == OpDiv32u) 
        v.Op = OpPPC64DIVWU;
        return true;
    else if (v.Op == OpDiv64) 
        return rewriteValuePPC64_OpDiv64(_addr_v);
    else if (v.Op == OpDiv64F) 
        v.Op = OpPPC64FDIV;
        return true;
    else if (v.Op == OpDiv64u) 
        v.Op = OpPPC64DIVDU;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValuePPC64_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValuePPC64_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValuePPC64_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValuePPC64_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValuePPC64_OpEq32F(_addr_v);
    else if (v.Op == OpEq64) 
        return rewriteValuePPC64_OpEq64(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValuePPC64_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValuePPC64_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValuePPC64_OpEqB(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValuePPC64_OpEqPtr(_addr_v);
    else if (v.Op == OpFMA) 
        v.Op = OpPPC64FMADD;
        return true;
    else if (v.Op == OpFloor) 
        v.Op = OpPPC64FFLOOR;
        return true;
    else if (v.Op == OpGetCallerPC) 
        v.Op = OpPPC64LoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = OpPPC64LoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = OpPPC64LoweredGetClosurePtr;
        return true;
    else if (v.Op == OpHmul32) 
        v.Op = OpPPC64MULHW;
        return true;
    else if (v.Op == OpHmul32u) 
        v.Op = OpPPC64MULHWU;
        return true;
    else if (v.Op == OpHmul64) 
        v.Op = OpPPC64MULHD;
        return true;
    else if (v.Op == OpHmul64u) 
        v.Op = OpPPC64MULHDU;
        return true;
    else if (v.Op == OpInterCall) 
        v.Op = OpPPC64CALLinter;
        return true;
    else if (v.Op == OpIsInBounds) 
        return rewriteValuePPC64_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValuePPC64_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValuePPC64_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValuePPC64_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValuePPC64_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValuePPC64_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValuePPC64_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValuePPC64_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64) 
        return rewriteValuePPC64_OpLeq64(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValuePPC64_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq64U) 
        return rewriteValuePPC64_OpLeq64U(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValuePPC64_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValuePPC64_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValuePPC64_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValuePPC64_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValuePPC64_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValuePPC64_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValuePPC64_OpLess32U(_addr_v);
    else if (v.Op == OpLess64) 
        return rewriteValuePPC64_OpLess64(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValuePPC64_OpLess64F(_addr_v);
    else if (v.Op == OpLess64U) 
        return rewriteValuePPC64_OpLess64U(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValuePPC64_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValuePPC64_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValuePPC64_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValuePPC64_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValuePPC64_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValuePPC64_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValuePPC64_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValuePPC64_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValuePPC64_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValuePPC64_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValuePPC64_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValuePPC64_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh64x16) 
        return rewriteValuePPC64_OpLsh64x16(_addr_v);
    else if (v.Op == OpLsh64x32) 
        return rewriteValuePPC64_OpLsh64x32(_addr_v);
    else if (v.Op == OpLsh64x64) 
        return rewriteValuePPC64_OpLsh64x64(_addr_v);
    else if (v.Op == OpLsh64x8) 
        return rewriteValuePPC64_OpLsh64x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValuePPC64_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValuePPC64_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValuePPC64_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValuePPC64_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValuePPC64_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValuePPC64_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValuePPC64_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValuePPC64_OpMod32u(_addr_v);
    else if (v.Op == OpMod64) 
        return rewriteValuePPC64_OpMod64(_addr_v);
    else if (v.Op == OpMod64u) 
        return rewriteValuePPC64_OpMod64u(_addr_v);
    else if (v.Op == OpMod8) 
        return rewriteValuePPC64_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValuePPC64_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValuePPC64_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = OpPPC64MULLW;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = OpPPC64MULLW;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = OpPPC64FMULS;
        return true;
    else if (v.Op == OpMul64) 
        v.Op = OpPPC64MULLD;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = OpPPC64FMUL;
        return true;
    else if (v.Op == OpMul64uhilo) 
        v.Op = OpPPC64LoweredMuluhilo;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = OpPPC64MULLW;
        return true;
    else if (v.Op == OpNeg16) 
        v.Op = OpPPC64NEG;
        return true;
    else if (v.Op == OpNeg32) 
        v.Op = OpPPC64NEG;
        return true;
    else if (v.Op == OpNeg32F) 
        v.Op = OpPPC64FNEG;
        return true;
    else if (v.Op == OpNeg64) 
        v.Op = OpPPC64NEG;
        return true;
    else if (v.Op == OpNeg64F) 
        v.Op = OpPPC64FNEG;
        return true;
    else if (v.Op == OpNeg8) 
        v.Op = OpPPC64NEG;
        return true;
    else if (v.Op == OpNeq16) 
        return rewriteValuePPC64_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValuePPC64_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValuePPC64_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64) 
        return rewriteValuePPC64_OpNeq64(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValuePPC64_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValuePPC64_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        v.Op = OpPPC64XOR;
        return true;
    else if (v.Op == OpNeqPtr) 
        return rewriteValuePPC64_OpNeqPtr(_addr_v);
    else if (v.Op == OpNilCheck) 
        v.Op = OpPPC64LoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        return rewriteValuePPC64_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValuePPC64_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        v.Op = OpPPC64OR;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = OpPPC64OR;
        return true;
    else if (v.Op == OpOr64) 
        v.Op = OpPPC64OR;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = OpPPC64OR;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = OpPPC64OR;
        return true;
    else if (v.Op == OpPPC64ADD) 
        return rewriteValuePPC64_OpPPC64ADD(_addr_v);
    else if (v.Op == OpPPC64ADDconst) 
        return rewriteValuePPC64_OpPPC64ADDconst(_addr_v);
    else if (v.Op == OpPPC64AND) 
        return rewriteValuePPC64_OpPPC64AND(_addr_v);
    else if (v.Op == OpPPC64ANDN) 
        return rewriteValuePPC64_OpPPC64ANDN(_addr_v);
    else if (v.Op == OpPPC64ANDconst) 
        return rewriteValuePPC64_OpPPC64ANDconst(_addr_v);
    else if (v.Op == OpPPC64CLRLSLDI) 
        return rewriteValuePPC64_OpPPC64CLRLSLDI(_addr_v);
    else if (v.Op == OpPPC64CMP) 
        return rewriteValuePPC64_OpPPC64CMP(_addr_v);
    else if (v.Op == OpPPC64CMPU) 
        return rewriteValuePPC64_OpPPC64CMPU(_addr_v);
    else if (v.Op == OpPPC64CMPUconst) 
        return rewriteValuePPC64_OpPPC64CMPUconst(_addr_v);
    else if (v.Op == OpPPC64CMPW) 
        return rewriteValuePPC64_OpPPC64CMPW(_addr_v);
    else if (v.Op == OpPPC64CMPWU) 
        return rewriteValuePPC64_OpPPC64CMPWU(_addr_v);
    else if (v.Op == OpPPC64CMPWUconst) 
        return rewriteValuePPC64_OpPPC64CMPWUconst(_addr_v);
    else if (v.Op == OpPPC64CMPWconst) 
        return rewriteValuePPC64_OpPPC64CMPWconst(_addr_v);
    else if (v.Op == OpPPC64CMPconst) 
        return rewriteValuePPC64_OpPPC64CMPconst(_addr_v);
    else if (v.Op == OpPPC64Equal) 
        return rewriteValuePPC64_OpPPC64Equal(_addr_v);
    else if (v.Op == OpPPC64FABS) 
        return rewriteValuePPC64_OpPPC64FABS(_addr_v);
    else if (v.Op == OpPPC64FADD) 
        return rewriteValuePPC64_OpPPC64FADD(_addr_v);
    else if (v.Op == OpPPC64FADDS) 
        return rewriteValuePPC64_OpPPC64FADDS(_addr_v);
    else if (v.Op == OpPPC64FCEIL) 
        return rewriteValuePPC64_OpPPC64FCEIL(_addr_v);
    else if (v.Op == OpPPC64FFLOOR) 
        return rewriteValuePPC64_OpPPC64FFLOOR(_addr_v);
    else if (v.Op == OpPPC64FGreaterEqual) 
        return rewriteValuePPC64_OpPPC64FGreaterEqual(_addr_v);
    else if (v.Op == OpPPC64FGreaterThan) 
        return rewriteValuePPC64_OpPPC64FGreaterThan(_addr_v);
    else if (v.Op == OpPPC64FLessEqual) 
        return rewriteValuePPC64_OpPPC64FLessEqual(_addr_v);
    else if (v.Op == OpPPC64FLessThan) 
        return rewriteValuePPC64_OpPPC64FLessThan(_addr_v);
    else if (v.Op == OpPPC64FMOVDload) 
        return rewriteValuePPC64_OpPPC64FMOVDload(_addr_v);
    else if (v.Op == OpPPC64FMOVDstore) 
        return rewriteValuePPC64_OpPPC64FMOVDstore(_addr_v);
    else if (v.Op == OpPPC64FMOVSload) 
        return rewriteValuePPC64_OpPPC64FMOVSload(_addr_v);
    else if (v.Op == OpPPC64FMOVSstore) 
        return rewriteValuePPC64_OpPPC64FMOVSstore(_addr_v);
    else if (v.Op == OpPPC64FNEG) 
        return rewriteValuePPC64_OpPPC64FNEG(_addr_v);
    else if (v.Op == OpPPC64FSQRT) 
        return rewriteValuePPC64_OpPPC64FSQRT(_addr_v);
    else if (v.Op == OpPPC64FSUB) 
        return rewriteValuePPC64_OpPPC64FSUB(_addr_v);
    else if (v.Op == OpPPC64FSUBS) 
        return rewriteValuePPC64_OpPPC64FSUBS(_addr_v);
    else if (v.Op == OpPPC64FTRUNC) 
        return rewriteValuePPC64_OpPPC64FTRUNC(_addr_v);
    else if (v.Op == OpPPC64GreaterEqual) 
        return rewriteValuePPC64_OpPPC64GreaterEqual(_addr_v);
    else if (v.Op == OpPPC64GreaterThan) 
        return rewriteValuePPC64_OpPPC64GreaterThan(_addr_v);
    else if (v.Op == OpPPC64ISEL) 
        return rewriteValuePPC64_OpPPC64ISEL(_addr_v);
    else if (v.Op == OpPPC64ISELB) 
        return rewriteValuePPC64_OpPPC64ISELB(_addr_v);
    else if (v.Op == OpPPC64LessEqual) 
        return rewriteValuePPC64_OpPPC64LessEqual(_addr_v);
    else if (v.Op == OpPPC64LessThan) 
        return rewriteValuePPC64_OpPPC64LessThan(_addr_v);
    else if (v.Op == OpPPC64MFVSRD) 
        return rewriteValuePPC64_OpPPC64MFVSRD(_addr_v);
    else if (v.Op == OpPPC64MOVBZload) 
        return rewriteValuePPC64_OpPPC64MOVBZload(_addr_v);
    else if (v.Op == OpPPC64MOVBZloadidx) 
        return rewriteValuePPC64_OpPPC64MOVBZloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVBZreg) 
        return rewriteValuePPC64_OpPPC64MOVBZreg(_addr_v);
    else if (v.Op == OpPPC64MOVBreg) 
        return rewriteValuePPC64_OpPPC64MOVBreg(_addr_v);
    else if (v.Op == OpPPC64MOVBstore) 
        return rewriteValuePPC64_OpPPC64MOVBstore(_addr_v);
    else if (v.Op == OpPPC64MOVBstoreidx) 
        return rewriteValuePPC64_OpPPC64MOVBstoreidx(_addr_v);
    else if (v.Op == OpPPC64MOVBstorezero) 
        return rewriteValuePPC64_OpPPC64MOVBstorezero(_addr_v);
    else if (v.Op == OpPPC64MOVDload) 
        return rewriteValuePPC64_OpPPC64MOVDload(_addr_v);
    else if (v.Op == OpPPC64MOVDloadidx) 
        return rewriteValuePPC64_OpPPC64MOVDloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVDstore) 
        return rewriteValuePPC64_OpPPC64MOVDstore(_addr_v);
    else if (v.Op == OpPPC64MOVDstoreidx) 
        return rewriteValuePPC64_OpPPC64MOVDstoreidx(_addr_v);
    else if (v.Op == OpPPC64MOVDstorezero) 
        return rewriteValuePPC64_OpPPC64MOVDstorezero(_addr_v);
    else if (v.Op == OpPPC64MOVHBRstore) 
        return rewriteValuePPC64_OpPPC64MOVHBRstore(_addr_v);
    else if (v.Op == OpPPC64MOVHZload) 
        return rewriteValuePPC64_OpPPC64MOVHZload(_addr_v);
    else if (v.Op == OpPPC64MOVHZloadidx) 
        return rewriteValuePPC64_OpPPC64MOVHZloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVHZreg) 
        return rewriteValuePPC64_OpPPC64MOVHZreg(_addr_v);
    else if (v.Op == OpPPC64MOVHload) 
        return rewriteValuePPC64_OpPPC64MOVHload(_addr_v);
    else if (v.Op == OpPPC64MOVHloadidx) 
        return rewriteValuePPC64_OpPPC64MOVHloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVHreg) 
        return rewriteValuePPC64_OpPPC64MOVHreg(_addr_v);
    else if (v.Op == OpPPC64MOVHstore) 
        return rewriteValuePPC64_OpPPC64MOVHstore(_addr_v);
    else if (v.Op == OpPPC64MOVHstoreidx) 
        return rewriteValuePPC64_OpPPC64MOVHstoreidx(_addr_v);
    else if (v.Op == OpPPC64MOVHstorezero) 
        return rewriteValuePPC64_OpPPC64MOVHstorezero(_addr_v);
    else if (v.Op == OpPPC64MOVWBRstore) 
        return rewriteValuePPC64_OpPPC64MOVWBRstore(_addr_v);
    else if (v.Op == OpPPC64MOVWZload) 
        return rewriteValuePPC64_OpPPC64MOVWZload(_addr_v);
    else if (v.Op == OpPPC64MOVWZloadidx) 
        return rewriteValuePPC64_OpPPC64MOVWZloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVWZreg) 
        return rewriteValuePPC64_OpPPC64MOVWZreg(_addr_v);
    else if (v.Op == OpPPC64MOVWload) 
        return rewriteValuePPC64_OpPPC64MOVWload(_addr_v);
    else if (v.Op == OpPPC64MOVWloadidx) 
        return rewriteValuePPC64_OpPPC64MOVWloadidx(_addr_v);
    else if (v.Op == OpPPC64MOVWreg) 
        return rewriteValuePPC64_OpPPC64MOVWreg(_addr_v);
    else if (v.Op == OpPPC64MOVWstore) 
        return rewriteValuePPC64_OpPPC64MOVWstore(_addr_v);
    else if (v.Op == OpPPC64MOVWstoreidx) 
        return rewriteValuePPC64_OpPPC64MOVWstoreidx(_addr_v);
    else if (v.Op == OpPPC64MOVWstorezero) 
        return rewriteValuePPC64_OpPPC64MOVWstorezero(_addr_v);
    else if (v.Op == OpPPC64MTVSRD) 
        return rewriteValuePPC64_OpPPC64MTVSRD(_addr_v);
    else if (v.Op == OpPPC64MULLD) 
        return rewriteValuePPC64_OpPPC64MULLD(_addr_v);
    else if (v.Op == OpPPC64MULLW) 
        return rewriteValuePPC64_OpPPC64MULLW(_addr_v);
    else if (v.Op == OpPPC64NEG) 
        return rewriteValuePPC64_OpPPC64NEG(_addr_v);
    else if (v.Op == OpPPC64NOR) 
        return rewriteValuePPC64_OpPPC64NOR(_addr_v);
    else if (v.Op == OpPPC64NotEqual) 
        return rewriteValuePPC64_OpPPC64NotEqual(_addr_v);
    else if (v.Op == OpPPC64OR) 
        return rewriteValuePPC64_OpPPC64OR(_addr_v);
    else if (v.Op == OpPPC64ORN) 
        return rewriteValuePPC64_OpPPC64ORN(_addr_v);
    else if (v.Op == OpPPC64ORconst) 
        return rewriteValuePPC64_OpPPC64ORconst(_addr_v);
    else if (v.Op == OpPPC64ROTL) 
        return rewriteValuePPC64_OpPPC64ROTL(_addr_v);
    else if (v.Op == OpPPC64ROTLW) 
        return rewriteValuePPC64_OpPPC64ROTLW(_addr_v);
    else if (v.Op == OpPPC64ROTLWconst) 
        return rewriteValuePPC64_OpPPC64ROTLWconst(_addr_v);
    else if (v.Op == OpPPC64SLD) 
        return rewriteValuePPC64_OpPPC64SLD(_addr_v);
    else if (v.Op == OpPPC64SLDconst) 
        return rewriteValuePPC64_OpPPC64SLDconst(_addr_v);
    else if (v.Op == OpPPC64SLW) 
        return rewriteValuePPC64_OpPPC64SLW(_addr_v);
    else if (v.Op == OpPPC64SLWconst) 
        return rewriteValuePPC64_OpPPC64SLWconst(_addr_v);
    else if (v.Op == OpPPC64SRAD) 
        return rewriteValuePPC64_OpPPC64SRAD(_addr_v);
    else if (v.Op == OpPPC64SRAW) 
        return rewriteValuePPC64_OpPPC64SRAW(_addr_v);
    else if (v.Op == OpPPC64SRD) 
        return rewriteValuePPC64_OpPPC64SRD(_addr_v);
    else if (v.Op == OpPPC64SRW) 
        return rewriteValuePPC64_OpPPC64SRW(_addr_v);
    else if (v.Op == OpPPC64SRWconst) 
        return rewriteValuePPC64_OpPPC64SRWconst(_addr_v);
    else if (v.Op == OpPPC64SUB) 
        return rewriteValuePPC64_OpPPC64SUB(_addr_v);
    else if (v.Op == OpPPC64SUBFCconst) 
        return rewriteValuePPC64_OpPPC64SUBFCconst(_addr_v);
    else if (v.Op == OpPPC64XOR) 
        return rewriteValuePPC64_OpPPC64XOR(_addr_v);
    else if (v.Op == OpPPC64XORconst) 
        return rewriteValuePPC64_OpPPC64XORconst(_addr_v);
    else if (v.Op == OpPanicBounds) 
        return rewriteValuePPC64_OpPanicBounds(_addr_v);
    else if (v.Op == OpPopCount16) 
        return rewriteValuePPC64_OpPopCount16(_addr_v);
    else if (v.Op == OpPopCount32) 
        return rewriteValuePPC64_OpPopCount32(_addr_v);
    else if (v.Op == OpPopCount64) 
        v.Op = OpPPC64POPCNTD;
        return true;
    else if (v.Op == OpPopCount8) 
        return rewriteValuePPC64_OpPopCount8(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValuePPC64_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        return rewriteValuePPC64_OpRotateLeft32(_addr_v);
    else if (v.Op == OpRotateLeft64) 
        return rewriteValuePPC64_OpRotateLeft64(_addr_v);
    else if (v.Op == OpRotateLeft8) 
        return rewriteValuePPC64_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound) 
        v.Op = OpPPC64FROUND;
        return true;
    else if (v.Op == OpRound32F) 
        v.Op = OpPPC64LoweredRound32F;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpPPC64LoweredRound64F;
        return true;
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValuePPC64_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValuePPC64_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValuePPC64_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValuePPC64_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValuePPC64_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValuePPC64_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValuePPC64_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValuePPC64_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValuePPC64_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValuePPC64_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValuePPC64_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValuePPC64_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValuePPC64_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValuePPC64_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValuePPC64_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValuePPC64_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh64Ux16) 
        return rewriteValuePPC64_OpRsh64Ux16(_addr_v);
    else if (v.Op == OpRsh64Ux32) 
        return rewriteValuePPC64_OpRsh64Ux32(_addr_v);
    else if (v.Op == OpRsh64Ux64) 
        return rewriteValuePPC64_OpRsh64Ux64(_addr_v);
    else if (v.Op == OpRsh64Ux8) 
        return rewriteValuePPC64_OpRsh64Ux8(_addr_v);
    else if (v.Op == OpRsh64x16) 
        return rewriteValuePPC64_OpRsh64x16(_addr_v);
    else if (v.Op == OpRsh64x32) 
        return rewriteValuePPC64_OpRsh64x32(_addr_v);
    else if (v.Op == OpRsh64x64) 
        return rewriteValuePPC64_OpRsh64x64(_addr_v);
    else if (v.Op == OpRsh64x8) 
        return rewriteValuePPC64_OpRsh64x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValuePPC64_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValuePPC64_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValuePPC64_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValuePPC64_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValuePPC64_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValuePPC64_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValuePPC64_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValuePPC64_OpRsh8x8(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        v.Op = OpPPC64MOVHreg;
        return true;
    else if (v.Op == OpSignExt16to64) 
        v.Op = OpPPC64MOVHreg;
        return true;
    else if (v.Op == OpSignExt32to64) 
        v.Op = OpPPC64MOVWreg;
        return true;
    else if (v.Op == OpSignExt8to16) 
        v.Op = OpPPC64MOVBreg;
        return true;
    else if (v.Op == OpSignExt8to32) 
        v.Op = OpPPC64MOVBreg;
        return true;
    else if (v.Op == OpSignExt8to64) 
        v.Op = OpPPC64MOVBreg;
        return true;
    else if (v.Op == OpSlicemask) 
        return rewriteValuePPC64_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = OpPPC64FSQRT;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = OpPPC64FSQRTS;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = OpPPC64CALLstatic;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValuePPC64_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = OpPPC64SUB;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = OpPPC64SUB;
        return true;
    else if (v.Op == OpSub32F) 
        v.Op = OpPPC64FSUBS;
        return true;
    else if (v.Op == OpSub64) 
        v.Op = OpPPC64SUB;
        return true;
    else if (v.Op == OpSub64F) 
        v.Op = OpPPC64FSUB;
        return true;
    else if (v.Op == OpSub8) 
        v.Op = OpPPC64SUB;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = OpPPC64SUB;
        return true;
    else if (v.Op == OpTrunc) 
        v.Op = OpPPC64FTRUNC;
        return true;
    else if (v.Op == OpTrunc16to8) 
        return rewriteValuePPC64_OpTrunc16to8(_addr_v);
    else if (v.Op == OpTrunc32to16) 
        return rewriteValuePPC64_OpTrunc32to16(_addr_v);
    else if (v.Op == OpTrunc32to8) 
        return rewriteValuePPC64_OpTrunc32to8(_addr_v);
    else if (v.Op == OpTrunc64to16) 
        return rewriteValuePPC64_OpTrunc64to16(_addr_v);
    else if (v.Op == OpTrunc64to32) 
        return rewriteValuePPC64_OpTrunc64to32(_addr_v);
    else if (v.Op == OpTrunc64to8) 
        return rewriteValuePPC64_OpTrunc64to8(_addr_v);
    else if (v.Op == OpWB) 
        v.Op = OpPPC64LoweredWB;
        return true;
    else if (v.Op == OpXor16) 
        v.Op = OpPPC64XOR;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = OpPPC64XOR;
        return true;
    else if (v.Op == OpXor64) 
        v.Op = OpPPC64XOR;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = OpPPC64XOR;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValuePPC64_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        v.Op = OpPPC64MOVHZreg;
        return true;
    else if (v.Op == OpZeroExt16to64) 
        v.Op = OpPPC64MOVHZreg;
        return true;
    else if (v.Op == OpZeroExt32to64) 
        v.Op = OpPPC64MOVWZreg;
        return true;
    else if (v.Op == OpZeroExt8to16) 
        v.Op = OpPPC64MOVBZreg;
        return true;
    else if (v.Op == OpZeroExt8to32) 
        v.Op = OpPPC64MOVBZreg;
        return true;
    else if (v.Op == OpZeroExt8to64) 
        v.Op = OpPPC64MOVBZreg;
        return true;
        return false;
}
private static bool rewriteValuePPC64_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (MOVDaddr {sym} [0] base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpPPC64MOVDaddr);
        v.AuxInt = int32ToAuxInt(0);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicCompareAndSwap32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicCompareAndSwap32 ptr old new_ mem)
    // result: (LoweredAtomicCas32 [1] ptr old new_ mem)
    while (true) {
        var ptr = v_0;
        var old = v_1;
        var new_ = v_2;
        var mem = v_3;
        v.reset(OpPPC64LoweredAtomicCas32);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg4(ptr, old, new_, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicCompareAndSwap64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicCompareAndSwap64 ptr old new_ mem)
    // result: (LoweredAtomicCas64 [1] ptr old new_ mem)
    while (true) {
        var ptr = v_0;
        var old = v_1;
        var new_ = v_2;
        var mem = v_3;
        v.reset(OpPPC64LoweredAtomicCas64);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg4(ptr, old, new_, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicCompareAndSwapRel32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicCompareAndSwapRel32 ptr old new_ mem)
    // result: (LoweredAtomicCas32 [0] ptr old new_ mem)
    while (true) {
        var ptr = v_0;
        var old = v_1;
        var new_ = v_2;
        var mem = v_3;
        v.reset(OpPPC64LoweredAtomicCas32);
        v.AuxInt = int64ToAuxInt(0);
        v.AddArg4(ptr, old, new_, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoad32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad32 ptr mem)
    // result: (LoweredAtomicLoad32 [1] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoad32);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoad64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad64 ptr mem)
    // result: (LoweredAtomicLoad64 [1] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoad64);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoad8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoad8 ptr mem)
    // result: (LoweredAtomicLoad8 [1] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoad8);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoadAcq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoadAcq32 ptr mem)
    // result: (LoweredAtomicLoad32 [0] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoad32);
        v.AuxInt = int64ToAuxInt(0);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoadAcq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoadAcq64 ptr mem)
    // result: (LoweredAtomicLoad64 [0] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoad64);
        v.AuxInt = int64ToAuxInt(0);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicLoadPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicLoadPtr ptr mem)
    // result: (LoweredAtomicLoadPtr [1] ptr mem)
    while (true) {
        var ptr = v_0;
        var mem = v_1;
        v.reset(OpPPC64LoweredAtomicLoadPtr);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg2(ptr, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicStore32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStore32 ptr val mem)
    // result: (LoweredAtomicStore32 [1] ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpPPC64LoweredAtomicStore32);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicStore64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStore64 ptr val mem)
    // result: (LoweredAtomicStore64 [1] ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpPPC64LoweredAtomicStore64);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicStore8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStore8 ptr val mem)
    // result: (LoweredAtomicStore8 [1] ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpPPC64LoweredAtomicStore8);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicStoreRel32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStoreRel32 ptr val mem)
    // result: (LoweredAtomicStore32 [0] ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpPPC64LoweredAtomicStore32);
        v.AuxInt = int64ToAuxInt(0);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAtomicStoreRel64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AtomicStoreRel64 ptr val mem)
    // result: (LoweredAtomicStore64 [0] ptr val mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        v.reset(OpPPC64LoweredAtomicStore64);
        v.AuxInt = int64ToAuxInt(0);
        v.AddArg3(ptr, val, mem);
        return true;
    }
}
private static bool rewriteValuePPC64_OpAvg64u(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64ADD);
        var v0 = b.NewValue0(v.Pos, OpPPC64SRDconst, t);
        v0.AuxInt = int64ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpPPC64SUB, t);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg2(v0, y);
        return true;
    }
}
private static bool rewriteValuePPC64_OpBitLen32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen32 x)
    // result: (SUBFCconst [32] (CNTLZW <typ.Int> x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64SUBFCconst);
        v.AuxInt = int64ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpPPC64CNTLZW, typ.Int);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpBitLen64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen64 x)
    // result: (SUBFCconst [64] (CNTLZD <typ.Int> x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64SUBFCconst);
        v.AuxInt = int64ToAuxInt(64);
        var v0 = b.NewValue0(v.Pos, OpPPC64CNTLZD, typ.Int);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCom16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com16 x)
    // result: (NOR x x)
    while (true) {
        var x = v_0;
        v.reset(OpPPC64NOR);
        v.AddArg2(x, x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCom32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com32 x)
    // result: (NOR x x)
    while (true) {
        var x = v_0;
        v.reset(OpPPC64NOR);
        v.AddArg2(x, x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCom64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com64 x)
    // result: (NOR x x)
    while (true) {
        var x = v_0;
        v.reset(OpPPC64NOR);
        v.AddArg2(x, x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCom8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com8 x)
    // result: (NOR x x)
    while (true) {
        var x = v_0;
        v.reset(OpPPC64NOR);
        v.AddArg2(x, x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCondSelect(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CondSelect x y bool)
    // cond: flagArg(bool) != nil
    // result: (ISEL [2] x y bool)
    while (true) {
        var x = v_0;
        var y = v_1;
        var @bool = v_2;
        if (!(flagArg(bool) != null)) {
            break;
        }
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(2);
        v.AddArg3(x, y, bool);
        return true;
    } 
    // match: (CondSelect x y bool)
    // cond: flagArg(bool) == nil
    // result: (ISEL [2] x y (CMPWconst [0] bool))
    while (true) {
        x = v_0;
        y = v_1;
        @bool = v_2;
        if (!(flagArg(bool) == null)) {
            break;
        }
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(2);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg(bool);
        v.AddArg3(x, y, v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt16(v.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValuePPC64_OpConst32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt32(v.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValuePPC64_OpConst64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const64 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt64(v.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValuePPC64_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [val])
    // result: (MOVDconst [int64(val)])
    while (true) {
        var val = auxIntToInt8(v.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(val));
        return true;
    }
}
private static bool rewriteValuePPC64_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [t])
    // result: (MOVDconst [b2i(t)])
    while (true) {
        var t = auxIntToBool(v.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(b2i(t));
        return true;
    }
}
private static bool rewriteValuePPC64_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (MOVDconst [0])
    while (true) {
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCopysign(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Copysign x y)
    // result: (FCPSGN y x)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64FCPSGN);
        v.AddArg2(y, x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCtz16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz16 x)
    // result: (POPCNTW (MOVHZreg (ANDN <typ.Int16> (ADDconst <typ.Int16> [-1] x) x)))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64POPCNTW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64ANDN, typ.Int16);
        var v2 = b.NewValue0(v.Pos, OpPPC64ADDconst, typ.Int16);
        v2.AuxInt = int64ToAuxInt(-1);
        v2.AddArg(x);
        v1.AddArg2(v2, x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz32 x)
    // cond: buildcfg.GOPPC64<=8
    // result: (POPCNTW (MOVWZreg (ANDN <typ.Int> (ADDconst <typ.Int> [-1] x) x)))
    while (true) {
        var x = v_0;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64POPCNTW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVWZreg, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64ANDN, typ.Int);
        var v2 = b.NewValue0(v.Pos, OpPPC64ADDconst, typ.Int);
        v2.AuxInt = int64ToAuxInt(-1);
        v2.AddArg(x);
        v1.AddArg2(v2, x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    } 
    // match: (Ctz32 x)
    // result: (CNTTZW (MOVWZreg x))
    while (true) {
        x = v_0;
        v.reset(OpPPC64CNTTZW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCtz64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz64 x)
    // cond: buildcfg.GOPPC64<=8
    // result: (POPCNTD (ANDN <typ.Int64> (ADDconst <typ.Int64> [-1] x) x))
    while (true) {
        var x = v_0;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64POPCNTD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ANDN, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64ADDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        v1.AddArg(x);
        v0.AddArg2(v1, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Ctz64 x)
    // result: (CNTTZD x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64CNTTZD);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCtz8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz8 x)
    // result: (POPCNTB (MOVBZreg (ANDN <typ.UInt8> (ADDconst <typ.UInt8> [-1] x) x)))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64POPCNTB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64ANDN, typ.UInt8);
        var v2 = b.NewValue0(v.Pos, OpPPC64ADDconst, typ.UInt8);
        v2.AuxInt = int64ToAuxInt(-1);
        v2.AddArg(x);
        v1.AddArg2(v2, x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt32Fto32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32Fto32 x)
    // result: (MFVSRD (FCTIWZ x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64MFVSRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCTIWZ, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt32Fto64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32Fto64 x)
    // result: (MFVSRD (FCTIDZ x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64MFVSRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCTIDZ, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt32to32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32to32F x)
    // result: (FCFIDS (MTVSRD (SignExt32to64 x)))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64FCFIDS);
        var v0 = b.NewValue0(v.Pos, OpPPC64MTVSRD, typ.Float64);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt32to64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt32to64F x)
    // result: (FCFID (MTVSRD (SignExt32to64 x)))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64FCFID);
        var v0 = b.NewValue0(v.Pos, OpPPC64MTVSRD, typ.Float64);
        var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt64Fto32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt64Fto32 x)
    // result: (MFVSRD (FCTIWZ x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64MFVSRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCTIWZ, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt64Fto64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt64Fto64 x)
    // result: (MFVSRD (FCTIDZ x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64MFVSRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCTIDZ, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt64to32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt64to32F x)
    // result: (FCFIDS (MTVSRD x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64FCFIDS);
        var v0 = b.NewValue0(v.Pos, OpPPC64MTVSRD, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpCvt64to64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Cvt64to64F x)
    // result: (FCFID (MTVSRD x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64FCFID);
        var v0 = b.NewValue0(v.Pos, OpPPC64MTVSRD, typ.Float64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 [false] x y)
    // result: (DIVW (SignExt16to32 x) (SignExt16to32 y))
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVW);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u x y)
    // result: (DIVWU (ZeroExt16to32 x) (ZeroExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVWU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Div32 [false] x y)
    // result: (DIVW x y)
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpDiv64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Div64 [false] x y)
    // result: (DIVD x y)
    while (true) {
        if (auxIntToBool(v.AuxInt) != false) {
            break;
        }
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVD);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (DIVW (SignExt8to32 x) (SignExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVW);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (DIVWU (ZeroExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64DIVWU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x y)
    // cond: isSigned(x.Type) && isSigned(y.Type)
    // result: (Equal (CMPW (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var y = v_1;
                if (!(isSigned(x.Type) && isSigned(y.Type))) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                v.reset(OpPPC64Equal);
                var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }

        }
        break;
    } 
    // match: (Eq16 x y)
    // result: (Equal (CMPW (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64Equal);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32 x y)
    // result: (Equal (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64Equal);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32F x y)
    // result: (Equal (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64Equal);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq64 x y)
    // result: (Equal (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64Equal);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq64F x y)
    // result: (Equal (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64Equal);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x y)
    // cond: isSigned(x.Type) && isSigned(y.Type)
    // result: (Equal (CMPW (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var y = v_1;
                if (!(isSigned(x.Type) && isSigned(y.Type))) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                v.reset(OpPPC64Equal);
                var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }

        }
        break;
    } 
    // match: (Eq8 x y)
    // result: (Equal (CMPW (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64Equal);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqB x y)
    // result: (ANDconst [1] (EQV x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpPPC64EQV, typ.Int64);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (EqPtr x y)
    // result: (Equal (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64Equal);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsInBounds idx len)
    // result: (LessThan (CMPU idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsNonNil ptr)
    // result: (NotEqual (CMPconst [0] ptr))
    while (true) {
        var ptr = v_0;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPconst, types.TypeFlags);
        v0.AuxInt = int64ToAuxInt(0);
        v0.AddArg(ptr);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsSliceInBounds idx len)
    // result: (LessEqual (CMPU idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16 x y)
    // result: (LessEqual (CMPW (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16U x y)
    // result: (LessEqual (CMPWU (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32 x y)
    // result: (LessEqual (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32F x y)
    // result: (FLessEqual (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64FLessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32U x y)
    // result: (LessEqual (CMPWU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64 x y)
    // result: (LessEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64F x y)
    // result: (FLessEqual (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64FLessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64U x y)
    // result: (LessEqual (CMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8 x y)
    // result: (LessEqual (CMPW (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8U x y)
    // result: (LessEqual (CMPWU (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16 x y)
    // result: (LessThan (CMPW (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16U x y)
    // result: (LessThan (CMPWU (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32 x y)
    // result: (LessThan (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32F x y)
    // result: (FLessThan (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64FLessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32U x y)
    // result: (LessThan (CMPWU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64 x y)
    // result: (LessThan (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64F x y)
    // result: (FLessThan (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64FLessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess64U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64U x y)
    // result: (LessThan (CMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8 x y)
    // result: (LessThan (CMPW (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8U x y)
    // result: (LessThan (CMPWU (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64LessThan);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
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
        v.reset(OpPPC64MOVDload);
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
        v.reset(OpPPC64MOVWload);
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
        v.reset(OpPPC64MOVWZload);
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
        v.reset(OpPPC64MOVHload);
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
        v.reset(OpPPC64MOVHZload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: t.IsBoolean()
    // result: (MOVBZload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsBoolean())) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is8BitInt(t) && isSigned(t)
    // result: (MOVBreg (MOVBZload ptr mem))
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVBreg);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZload, typ.UInt8);
        v0.AddArg2(ptr, mem);
        v.AddArg(v0);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is8BitInt(t) && !isSigned(t)
    // result: (MOVBZload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
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
        v.reset(OpPPC64FMOVSload);
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
        v.reset(OpPPC64FMOVDload);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (MOVDaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpPPC64MOVDaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh16x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x16 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x32 x (MOVDconst [c]))
    // cond: uint32(c) < 16
    // result: (SLWconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh16x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x32 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(16);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 16
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Lsh16x64 x (MOVDconst [c]))
    // cond: uint64(c) < 16
    // result: (SLWconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh16x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x64 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(16);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh16x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh16x8 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh32x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x16 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x32 x (MOVDconst [c]))
    // cond: uint32(c) < 32
    // result: (SLWconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh32x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x32 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 32
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Lsh32x64 x (MOVDconst [c]))
    // cond: uint64(c) < 32
    // result: (SLWconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh32x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x64 x (AND y (MOVDconst [31])))
    // result: (SLW x (ANDconst <typ.Int32> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SLW);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int32);
                v0.AuxInt = int64ToAuxInt(31);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (Lsh32x64 x (ANDconst <typ.Int32> [31] y))
    // result: (SLW x (ANDconst <typ.Int32> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.Int32 || auxIntToInt64(v_1.AuxInt) != 31) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SLW);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int32);
        v0.AuxInt = int64ToAuxInt(31);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Lsh32x64 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh32x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh32x8 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh64x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x16 x y)
    // result: (SLD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x32 x (MOVDconst [c]))
    // cond: uint32(c) < 64
    // result: (SLDconst x [c&63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SLDconst);
        v.AuxInt = int64ToAuxInt(c & 63);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh64x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x32 x y)
    // result: (SLD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh64x64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 64
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Lsh64x64 x (MOVDconst [c]))
    // cond: uint64(c) < 64
    // result: (SLDconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SLDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x64 x (AND y (MOVDconst [63])))
    // result: (SLD x (ANDconst <typ.Int64> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SLD);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int64);
                v0.AuxInt = int64ToAuxInt(63);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }

        }
        break;
    } 
    // match: (Lsh64x64 x (ANDconst <typ.Int64> [63] y))
    // result: (SLD x (ANDconst <typ.Int64> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.Int64 || auxIntToInt64(v_1.AuxInt) != 63) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SLD);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(63);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Lsh64x64 x y)
    // result: (SLD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLD);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh64x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh64x8 x y)
    // result: (SLD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh8x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x16 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x32 x (MOVDconst [c]))
    // cond: uint32(c) < 8
    // result: (SLWconst x [c&7])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c & 7);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh8x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x32 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(8);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 8
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Lsh8x64 x (MOVDconst [c]))
    // cond: uint64(c) < 8
    // result: (SLWconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh8x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x64 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(8);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpLsh8x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SLW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Lsh8x8 x y)
    // result: (SLW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SLW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16 x y)
    // result: (Mod32 (SignExt16to32 x) (SignExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16u x y)
    // result: (Mod32u (ZeroExt16to32 x) (ZeroExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32 x y)
    // cond: buildcfg.GOPPC64 >= 9
    // result: (MODSW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64MODSW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Mod32 x y)
    // cond: buildcfg.GOPPC64 <= 8
    // result: (SUB x (MULLW y (DIVW x y)))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64SUB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MULLW, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpPPC64DIVW, typ.Int32);
        v1.AddArg2(x, y);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32u x y)
    // cond: buildcfg.GOPPC64 >= 9
    // result: (MODUW x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64MODUW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Mod32u x y)
    // cond: buildcfg.GOPPC64 <= 8
    // result: (SUB x (MULLW y (DIVWU x y)))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64SUB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MULLW, typ.Int32);
        var v1 = b.NewValue0(v.Pos, OpPPC64DIVWU, typ.Int32);
        v1.AddArg2(x, y);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpMod64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod64 x y)
    // cond: buildcfg.GOPPC64 >=9
    // result: (MODSD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64MODSD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Mod64 x y)
    // cond: buildcfg.GOPPC64 <=8
    // result: (SUB x (MULLD y (DIVD x y)))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64SUB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MULLD, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64DIVD, typ.Int64);
        v1.AddArg2(x, y);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpMod64u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod64u x y)
    // cond: buildcfg.GOPPC64 >= 9
    // result: (MODUD x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        if (!(buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64MODUD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Mod64u x y)
    // cond: buildcfg.GOPPC64 <= 8
    // result: (SUB x (MULLD y (DIVDU x y)))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64SUB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MULLD, typ.Int64);
        var v1 = b.NewValue0(v.Pos, OpPPC64DIVDU, typ.Int64);
        v1.AddArg2(x, y);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (Mod32 (SignExt8to32 x) (SignExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (Mod32u (ZeroExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpMove(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64MOVBstore);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZload, typ.UInt8);
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
        v.reset(OpPPC64MOVHstore);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZload, typ.UInt16);
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
        v.reset(OpPPC64MOVWstore);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWZload, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [8] {t} dst src mem)
    // result: (MOVDstore dst (MOVDload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpPPC64MOVDstore);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDload, typ.Int64);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [3] dst src mem)
    // result: (MOVBstore [2] dst (MOVBZload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVHstore, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVHload, typ.Int16);
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
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVWstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpPPC64MOVWZload, typ.UInt32);
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
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVWstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpPPC64MOVWZload, typ.UInt32);
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
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(6);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBZload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(6);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(4);
        v2 = b.NewValue0(v.Pos, OpPPC64MOVHZload, typ.UInt16);
        v2.AuxInt = int32ToAuxInt(4);
        v2.AddArg2(src, mem);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVWstore, types.TypeMem);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVWZload, typ.UInt32);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && buildcfg.GOPPC64 <= 8 && logLargeCopy(v, s)
    // result: (LoweredMove [s] dst src mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && buildcfg.GOPPC64 <= 8 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpPPC64LoweredMove);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg3(dst, src, mem);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && s <= 64 && buildcfg.GOPPC64 >= 9
    // result: (LoweredQuadMoveShort [s] dst src mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && s <= 64 && buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64LoweredQuadMoveShort);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg3(dst, src, mem);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && buildcfg.GOPPC64 >= 9 && logLargeCopy(v, s)
    // result: (LoweredQuadMove [s] dst src mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && buildcfg.GOPPC64 >= 9 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpPPC64LoweredQuadMove);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg3(dst, src, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x y)
    // cond: isSigned(x.Type) && isSigned(y.Type)
    // result: (NotEqual (CMPW (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var y = v_1;
                if (!(isSigned(x.Type) && isSigned(y.Type))) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                v.reset(OpPPC64NotEqual);
                var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }

        }
        break;
    } 
    // match: (Neq16 x y)
    // result: (NotEqual (CMPW (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64NotEqual);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32 x y)
    // result: (NotEqual (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32F x y)
    // result: (NotEqual (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeq64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq64 x y)
    // result: (NotEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq64F x y)
    // result: (NotEqual (FCMPU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64FCMPU, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x y)
    // cond: isSigned(x.Type) && isSigned(y.Type)
    // result: (NotEqual (CMPW (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var y = v_1;
                if (!(isSigned(x.Type) && isSigned(y.Type))) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                v.reset(OpPPC64NotEqual);
                var v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }

        }
        break;
    } 
    // match: (Neq8 x y)
    // result: (NotEqual (CMPW (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64NotEqual);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (NeqPtr x y)
    // result: (NotEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpPPC64NotEqual);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not x)
    // result: (XORconst [1] x)
    while (true) {
        var x = v_0;
        v.reset(OpPPC64XORconst);
        v.AuxInt = int64ToAuxInt(1);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (OffPtr [off] ptr)
    // result: (ADD (MOVDconst <typ.Int64> [off]) ptr)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        v.reset(OpPPC64ADD);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(off);
        v.AddArg2(v0, ptr);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64ADD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ADD l:(MULLD x y) z)
    // cond: buildcfg.GOPPC64 >= 9 && l.Uses == 1 && clobber(l)
    // result: (MADDLD x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var l = v_0;
                if (l.Op != OpPPC64MULLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = l.Args[1];
                var x = l.Args[0];
                var z = v_1;
                if (!(buildcfg.GOPPC64 >= 9 && l.Uses == 1 && clobber(l))) {
                    continue;
                }
                v.reset(OpPPC64MADDLD);
                v.AddArg3(x, y, z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLDconst x [c]) (SRDconst x [d]))
    // cond: d == 64-c
    // result: (ROTLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 64 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLWconst x [c]) (SRWconst x [d]))
    // cond: d == 32-c
    // result: (ROTLWconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRWconst) {
                    continue;
                }
                d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLWconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                var v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 64) {
                    continue;
                }
                var v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 63 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 64) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 63 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 32) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 31 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 32) {
                    continue;
                }
                v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 31 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADD x (MOVDconst [c]))
    // cond: is32Bit(c)
    // result: (ADDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64ADDconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ADDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDconst [c] (ADDconst [d] x))
    // cond: is32Bit(c+d)
    // result: (ADDconst [c+d] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(is32Bit(c + d))) {
            break;
        }
        v.reset(OpPPC64ADDconst);
        v.AuxInt = int64ToAuxInt(c + d);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (ADDconst [c] (MOVDaddr [d] {sym} x))
    // cond: is32Bit(c+int64(d))
    // result: (MOVDaddr [int32(c+int64(d))] {sym} x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDaddr) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        if (!(is32Bit(c + int64(d)))) {
            break;
        }
        v.reset(OpPPC64MOVDaddr);
        v.AuxInt = int32ToAuxInt(int32(c + int64(d)));
        v.Aux = symToAux(sym);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDconst [c] x:(SP))
    // cond: is32Bit(c)
    // result: (MOVDaddr [int32(c)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        x = v_0;
        if (x.Op != OpSP || !(is32Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVDaddr);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (ADDconst [c] (SUBFCconst [d] x))
    // cond: is32Bit(c+d)
    // result: (SUBFCconst [c+d] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64SUBFCconst) {
            break;
        }
        d = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(is32Bit(c + d))) {
            break;
        }
        v.reset(OpPPC64SUBFCconst);
        v.AuxInt = int64ToAuxInt(c + d);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64AND(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AND (MOVDconst [m]) (ROTLWconst [r] x))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWINM [encodePPC64RotateMask(r,m,32)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64ROTLWconst) {
                    continue;
                }
                var r = auxIntToInt64(v_1.AuxInt);
                var x = v_1.Args[0];
                if (!(isPPC64WordRotateMask(m))) {
                    continue;
                }
                v.reset(OpPPC64RLWINM);
                v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(r, m, 32));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [m]) (ROTLW x r))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWNM [encodePPC64RotateMask(0,m,32)] x r)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64ROTLW) {
                    continue;
                }
                r = v_1.Args[1];
                x = v_1.Args[0];
                if (!(isPPC64WordRotateMask(m))) {
                    continue;
                }
                v.reset(OpPPC64RLWNM);
                v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(0, m, 32));
                v.AddArg2(x, r);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [m]) (SRWconst x [s]))
    // cond: mergePPC64RShiftMask(m,s,32) == 0
    // result: (MOVDconst [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64SRWconst) {
                    continue;
                }
                var s = auxIntToInt64(v_1.AuxInt);
                if (!(mergePPC64RShiftMask(m, s, 32) == 0)) {
                    continue;
                }
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [m]) (SRWconst x [s]))
    // cond: mergePPC64AndSrwi(m,s) != 0
    // result: (RLWINM [mergePPC64AndSrwi(m,s)] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                m = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64SRWconst) {
                    continue;
                }
                s = auxIntToInt64(v_1.AuxInt);
                x = v_1.Args[0];
                if (!(mergePPC64AndSrwi(m, s) != 0)) {
                    continue;
                }
                v.reset(OpPPC64RLWINM);
                v.AuxInt = int64ToAuxInt(mergePPC64AndSrwi(m, s));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND x (NOR y y))
    // result: (ANDN x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpPPC64NOR) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_1.Args[1];
                if (y != v_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ANDN);
                v.AddArg2(x, y);
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
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(c & d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND x (MOVDconst [c]))
    // cond: isU16Bit(c)
    // result: (ANDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(isU16Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64ANDconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [c]) y:(MOVWZreg _))
    // cond: c&0xFFFFFFFF == 0xFFFFFFFF
    // result: y
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                y = v_1;
                if (y.Op != OpPPC64MOVWZreg || !(c & 0xFFFFFFFF == 0xFFFFFFFF)) {
                    continue;
                }
                v.copyOf(y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [0xFFFFFFFF]) y:(MOVWreg x))
    // result: (MOVWZreg x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_0.AuxInt) != 0xFFFFFFFF) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1;
                if (y.Op != OpPPC64MOVWreg) {
                    continue;
                }
                x = y.Args[0];
                v.reset(OpPPC64MOVWZreg);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (AND (MOVDconst [c]) x:(MOVBZload _ _))
    // result: (ANDconst [c&0xFF] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                x = v_1;
                if (x.Op != OpPPC64MOVBZload) {
                    continue;
                }
                v.reset(OpPPC64ANDconst);
                v.AuxInt = int64ToAuxInt(c & 0xFF);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ANDN(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ANDN (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [c&^d])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(c & ~d);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ANDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDconst [m] (ROTLWconst [r] x))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWINM [encodePPC64RotateMask(r,m,32)] x)
    while (true) {
        var m = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ROTLWconst) {
            break;
        }
        var r = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(isPPC64WordRotateMask(m))) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(r, m, 32));
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [m] (ROTLW x r))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWNM [encodePPC64RotateMask(0,m,32)] x r)
    while (true) {
        m = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ROTLW) {
            break;
        }
        r = v_0.Args[1];
        x = v_0.Args[0];
        if (!(isPPC64WordRotateMask(m))) {
            break;
        }
        v.reset(OpPPC64RLWNM);
        v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(0, m, 32));
        v.AddArg2(x, r);
        return true;
    } 
    // match: (ANDconst [m] (SRWconst x [s]))
    // cond: mergePPC64RShiftMask(m,s,32) == 0
    // result: (MOVDconst [0])
    while (true) {
        m = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        var s = auxIntToInt64(v_0.AuxInt);
        if (!(mergePPC64RShiftMask(m, s, 32) == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ANDconst [m] (SRWconst x [s]))
    // cond: mergePPC64AndSrwi(m,s) != 0
    // result: (RLWINM [mergePPC64AndSrwi(m,s)] x)
    while (true) {
        m = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        s = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(mergePPC64AndSrwi(m, s) != 0)) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(mergePPC64AndSrwi(m, s));
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (ANDconst [d] x))
    // result: (ANDconst [c&d] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ANDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & d);
        v.AddArg(x);
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
    // match: (ANDconst [0] _)
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ANDconst [c] y:(MOVBZreg _))
    // cond: c&0xFF == 0xFF
    // result: y
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        var y = v_0;
        if (y.Op != OpPPC64MOVBZreg || !(c & 0xFF == 0xFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ANDconst [0xFF] y:(MOVBreg _))
    // result: y
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0xFF) {
            break;
        }
        y = v_0;
        if (y.Op != OpPPC64MOVBreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ANDconst [c] y:(MOVHZreg _))
    // cond: c&0xFFFF == 0xFFFF
    // result: y
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        y = v_0;
        if (y.Op != OpPPC64MOVHZreg || !(c & 0xFFFF == 0xFFFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ANDconst [0xFFFF] y:(MOVHreg _))
    // result: y
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0xFFFF) {
            break;
        }
        y = v_0;
        if (y.Op != OpPPC64MOVHreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ANDconst [c] (MOVBreg x))
    // result: (ANDconst [c&0xFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVBreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFF);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVBZreg x))
    // result: (ANDconst [c&0xFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVBZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFF);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVHreg x))
    // result: (ANDconst [c&0xFFFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFFFF);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVHZreg x))
    // result: (ANDconst [c&0xFFFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFFFF);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVWreg x))
    // result: (ANDconst [c&0xFFFFFFFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFFFFFFFF);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDconst [c] (MOVWZreg x))
    // result: (ANDconst [c&0xFFFFFFFF] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(c & 0xFFFFFFFF);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CLRLSLDI(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CLRLSLDI [c] (SRWconst [s] x))
    // cond: mergePPC64ClrlsldiSrw(int64(c),s) != 0
    // result: (RLWINM [mergePPC64ClrlsldiSrw(int64(c),s)] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        var s = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(mergePPC64ClrlsldiSrw(int64(c), s) != 0)) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(mergePPC64ClrlsldiSrw(int64(c), s));
        v.AddArg(x);
        return true;
    } 
    // match: (CLRLSLDI [c] i:(RLWINM [s] x))
    // cond: mergePPC64ClrlsldiRlwinm(c,s) != 0
    // result: (RLWINM [mergePPC64ClrlsldiRlwinm(c,s)] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var i = v_0;
        if (i.Op != OpPPC64RLWINM) {
            break;
        }
        s = auxIntToInt64(i.AuxInt);
        x = i.Args[0];
        if (!(mergePPC64ClrlsldiRlwinm(c, s) != 0)) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(mergePPC64ClrlsldiRlwinm(c, s));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMP(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMP x (MOVDconst [c]))
    // cond: is16Bit(c)
    // result: (CMPconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64CMPconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMP (MOVDconst [c]) y)
    // cond: is16Bit(c)
    // result: (InvertFlags (CMPconst y [c]))
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var y = v_1;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPconst, types.TypeFlags);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(y);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMP x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMP y x))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        v0 = b.NewValue0(v.Pos, OpPPC64CMP, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPU x (MOVDconst [c]))
    // cond: isU16Bit(c)
    // result: (CMPUconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(isU16Bit(c))) {
            break;
        }
        v.reset(OpPPC64CMPUconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPU (MOVDconst [c]) y)
    // cond: isU16Bit(c)
    // result: (InvertFlags (CMPUconst y [c]))
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var y = v_1;
        if (!(isU16Bit(c))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPUconst, types.TypeFlags);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(y);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPU x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPU y x))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPUconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: x==y
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(x == y)) {
            break;
        }
        v.reset(OpPPC64FlagEQ);
        return true;
    } 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: uint64(x)<uint64(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint64(x) < uint64(y))) {
            break;
        }
        v.reset(OpPPC64FlagLT);
        return true;
    } 
    // match: (CMPUconst (MOVDconst [x]) [y])
    // cond: uint64(x)>uint64(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint64(x) > uint64(y))) {
            break;
        }
        v.reset(OpPPC64FlagGT);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPW x (MOVWreg y))
    // result: (CMPW x y)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        var y = v_1.Args[0];
        v.reset(OpPPC64CMPW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPW (MOVWreg x) y)
    // result: (CMPW x y)
    while (true) {
        if (v_0.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpPPC64CMPW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPW x (MOVDconst [c]))
    // cond: is16Bit(c)
    // result: (CMPWconst x [int32(c)])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64CMPWconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPW (MOVDconst [c]) y)
    // cond: is16Bit(c)
    // result: (InvertFlags (CMPWconst y [int32(c)]))
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(y);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPW x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPW y x))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPW, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPWU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPWU x (MOVWZreg y))
    // result: (CMPWU x y)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        var y = v_1.Args[0];
        v.reset(OpPPC64CMPWU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWU (MOVWZreg x) y)
    // result: (CMPWU x y)
    while (true) {
        if (v_0.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_0.Args[0];
        y = v_1;
        v.reset(OpPPC64CMPWU);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWU x (MOVDconst [c]))
    // cond: isU16Bit(c)
    // result: (CMPWUconst x [int32(c)])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(isU16Bit(c))) {
            break;
        }
        v.reset(OpPPC64CMPWUconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWU (MOVDconst [c]) y)
    // cond: isU16Bit(c)
    // result: (InvertFlags (CMPWUconst y [int32(c)]))
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        y = v_1;
        if (!(isU16Bit(c))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        var v0 = b.NewValue0(v.Pos, OpPPC64CMPWUconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(int32(c));
        v0.AddArg(y);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPWU x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPWU y x))
    while (true) {
        x = v_0;
        y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpPPC64InvertFlags);
        v0 = b.NewValue0(v.Pos, OpPPC64CMPWU, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPWUconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: int32(x)==int32(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) == int32(y))) {
            break;
        }
        v.reset(OpPPC64FlagEQ);
        return true;
    } 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: uint32(x)<uint32(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint32(x) < uint32(y))) {
            break;
        }
        v.reset(OpPPC64FlagLT);
        return true;
    } 
    // match: (CMPWUconst (MOVDconst [x]) [y])
    // cond: uint32(x)>uint32(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(uint32(x) > uint32(y))) {
            break;
        }
        v.reset(OpPPC64FlagGT);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)==int32(y)
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) == int32(y))) {
            break;
        }
        v.reset(OpPPC64FlagEQ);
        return true;
    } 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)<int32(y)
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) < int32(y))) {
            break;
        }
        v.reset(OpPPC64FlagLT);
        return true;
    } 
    // match: (CMPWconst (MOVDconst [x]) [y])
    // cond: int32(x)>int32(y)
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(int32(x) > int32(y))) {
            break;
        }
        v.reset(OpPPC64FlagGT);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64CMPconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x==y
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var x = auxIntToInt64(v_0.AuxInt);
        if (!(x == y)) {
            break;
        }
        v.reset(OpPPC64FlagEQ);
        return true;
    } 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x<y
    // result: (FlagLT)
    while (true) {
        y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(x < y)) {
            break;
        }
        v.reset(OpPPC64FlagLT);
        return true;
    } 
    // match: (CMPconst (MOVDconst [x]) [y])
    // cond: x>y
    // result: (FlagGT)
    while (true) {
        y = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        x = auxIntToInt64(v_0.AuxInt);
        if (!(x > y)) {
            break;
        }
        v.reset(OpPPC64FlagGT);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64Equal(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Equal (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (Equal (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Equal (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Equal (InvertFlags x))
    // result: (Equal x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64Equal);
        v.AddArg(x);
        return true;
    } 
    // match: (Equal cmp)
    // result: (ISELB [2] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(2);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64FABS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FABS (FMOVDconst [x]))
    // result: (FMOVDconst [math.Abs(x)])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Abs(x));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FADD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FADD (FMUL x y) z)
    // result: (FMADD x y z)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64FMUL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_0.Args[1];
                var x = v_0.Args[0];
                var z = v_1;
                v.reset(OpPPC64FMADD);
                v.AddArg3(x, y, z);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FADDS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FADDS (FMULS x y) z)
    // result: (FMADDS x y z)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64FMULS) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_0.Args[1];
                var x = v_0.Args[0];
                var z = v_1;
                v.reset(OpPPC64FMADDS);
                v.AddArg3(x, y, z);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FCEIL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FCEIL (FMOVDconst [x]))
    // result: (FMOVDconst [math.Ceil(x)])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Ceil(x));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FFLOOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FFLOOR (FMOVDconst [x]))
    // result: (FMOVDconst [math.Floor(x)])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Floor(x));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FGreaterEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (FGreaterEqual cmp)
    // result: (ISEL [2] (MOVDconst [1]) (ISELB [1] (MOVDconst [1]) cmp) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(2);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISELB, typ.Int32);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg2(v0, cmp);
        v.AddArg3(v0, v1, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64FGreaterThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (FGreaterThan cmp)
    // result: (ISELB [1] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64FLessEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (FLessEqual cmp)
    // result: (ISEL [2] (MOVDconst [1]) (ISELB [0] (MOVDconst [1]) cmp) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(2);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISELB, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg2(v0, cmp);
        v.AddArg3(v0, v1, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64FLessThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (FLessThan cmp)
    // result: (ISELB [0] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64FMOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVDload [off] {sym} ptr (MOVDstore [off] {sym} ptr x _))
    // result: (MTVSRD x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        if (ptr != v_1.Args[0]) {
            break;
        }
        v.reset(OpPPC64MTVSRD);
        v.AddArg(x);
        return true;
    } 
    // match: (FMOVDload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (FMOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64FMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (FMOVDload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (FMOVDload [off1+int32(off2)] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64FMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FMOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVDstore [off] {sym} ptr (MTVSRD x) mem)
    // result: (MOVDstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64MTVSRD) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (FMOVDstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (FMOVDstore [off1+int32(off2)] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64FMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (FMOVDstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (FMOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64FMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FMOVSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVSload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (FMOVSload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64FMOVSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (FMOVSload [off1] {sym} (ADDconst [off2] ptr) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (FMOVSload [off1+int32(off2)] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64FMOVSload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FMOVSstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMOVSstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (FMOVSstore [off1+int32(off2)] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64FMOVSstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (FMOVSstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (FMOVSstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64FMOVSstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FNEG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FNEG (FABS x))
    // result: (FNABS x)
    while (true) {
        if (v_0.Op != OpPPC64FABS) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64FNABS);
        v.AddArg(x);
        return true;
    } 
    // match: (FNEG (FNABS x))
    // result: (FABS x)
    while (true) {
        if (v_0.Op != OpPPC64FNABS) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64FABS);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FSQRT(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FSQRT (FMOVDconst [x]))
    // cond: x >= 0
    // result: (FMOVDconst [math.Sqrt(x)])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        if (!(x >= 0)) {
            break;
        }
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Sqrt(x));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FSUB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FSUB (FMUL x y) z)
    // result: (FMSUB x y z)
    while (true) {
        if (v_0.Op != OpPPC64FMUL) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        var z = v_1;
        v.reset(OpPPC64FMSUB);
        v.AddArg3(x, y, z);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FSUBS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FSUBS (FMULS x y) z)
    // result: (FMSUBS x y z)
    while (true) {
        if (v_0.Op != OpPPC64FMULS) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        var z = v_1;
        v.reset(OpPPC64FMSUBS);
        v.AddArg3(x, y, z);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64FTRUNC(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (FTRUNC (FMOVDconst [x]))
    // result: (FMOVDconst [math.Trunc(x)])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var x = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Trunc(x));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64GreaterEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (GreaterEqual (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (GreaterEqual (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (GreaterEqual (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (GreaterEqual (InvertFlags x))
    // result: (LessEqual x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64LessEqual);
        v.AddArg(x);
        return true;
    } 
    // match: (GreaterEqual cmp)
    // result: (ISELB [4] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(4);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64GreaterThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (GreaterThan (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (GreaterThan (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (GreaterThan (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (GreaterThan (InvertFlags x))
    // result: (LessThan x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64LessThan);
        v.AddArg(x);
        return true;
    } 
    // match: (GreaterThan cmp)
    // result: (ISELB [1] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64ISEL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ISEL [0] (ANDconst [d] y) (MOVDconst [-1]) (CMPU (ANDconst [d] y) (MOVDconst [c])))
    // cond: c >= d
    // result: (ANDconst [d] y)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || v_0.Op != OpPPC64ANDconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        var y = v_0.Args[0];
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != -1 || v_2.Op != OpPPC64CMPU) {
            break;
        }
        _ = v_2.Args[1];
        var v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64ANDconst || auxIntToInt64(v_2_0.AuxInt) != d || y != v_2_0.Args[0]) {
            break;
        }
        var v_2_1 = v_2.Args[1];
        if (v_2_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_2_1.AuxInt);
        if (!(c >= d)) {
            break;
        }
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(d);
        v.AddArg(y);
        return true;
    } 
    // match: (ISEL [0] (ANDconst [d] y) (MOVDconst [-1]) (CMPUconst [c] (ANDconst [d] y)))
    // cond: c >= d
    // result: (ANDconst [d] y)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || v_0.Op != OpPPC64ANDconst) {
            break;
        }
        d = auxIntToInt64(v_0.AuxInt);
        y = v_0.Args[0];
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != -1 || v_2.Op != OpPPC64CMPUconst) {
            break;
        }
        c = auxIntToInt64(v_2.AuxInt);
        v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64ANDconst || auxIntToInt64(v_2_0.AuxInt) != d || y != v_2_0.Args[0] || !(c >= d)) {
            break;
        }
        v.reset(OpPPC64ANDconst);
        v.AuxInt = int64ToAuxInt(d);
        v.AddArg(y);
        return true;
    } 
    // match: (ISEL [2] x _ (FlagEQ))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2) {
            break;
        }
        var x = v_0;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [2] _ y (FlagLT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [2] _ y (FlagGT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [6] _ y (FlagEQ))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [6] x _ (FlagLT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [6] x _ (FlagGT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [0] _ y (FlagEQ))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [0] _ y (FlagGT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [0] x _ (FlagLT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [5] _ x (FlagEQ))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5) {
            break;
        }
        x = v_1;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [5] _ x (FlagLT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5) {
            break;
        }
        x = v_1;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [5] y _ (FlagGT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5) {
            break;
        }
        y = v_0;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [1] _ y (FlagEQ))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [1] _ y (FlagLT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [1] x _ (FlagGT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [4] x _ (FlagEQ))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagEQ) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [4] x _ (FlagGT))
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4) {
            break;
        }
        x = v_0;
        if (v_2.Op != OpPPC64FlagGT) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ISEL [4] _ y (FlagLT))
    // result: y
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4) {
            break;
        }
        y = v_1;
        if (v_2.Op != OpPPC64FlagLT) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (ISEL [n] x y (InvertFlags bool))
    // cond: n%4 == 0
    // result: (ISEL [n+1] x y bool)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        x = v_0;
        y = v_1;
        if (v_2.Op != OpPPC64InvertFlags) {
            break;
        }
        var @bool = v_2.Args[0];
        if (!(n % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(n + 1);
        v.AddArg3(x, y, bool);
        return true;
    } 
    // match: (ISEL [n] x y (InvertFlags bool))
    // cond: n%4 == 1
    // result: (ISEL [n-1] x y bool)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        x = v_0;
        y = v_1;
        if (v_2.Op != OpPPC64InvertFlags) {
            break;
        }
        @bool = v_2.Args[0];
        if (!(n % 4 == 1)) {
            break;
        }
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(n - 1);
        v.AddArg3(x, y, bool);
        return true;
    } 
    // match: (ISEL [n] x y (InvertFlags bool))
    // cond: n%4 == 2
    // result: (ISEL [n] x y bool)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        x = v_0;
        y = v_1;
        if (v_2.Op != OpPPC64InvertFlags) {
            break;
        }
        @bool = v_2.Args[0];
        if (!(n % 4 == 2)) {
            break;
        }
        v.reset(OpPPC64ISEL);
        v.AuxInt = int32ToAuxInt(n);
        v.AddArg3(x, y, bool);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ISELB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ISELB [0] _ (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [0] _ (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [0] _ (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [1] _ (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [1] _ (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [1] _ (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [2] _ (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [2] _ (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [2] _ (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 2 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [4] _ (FlagLT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [4] _ (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [4] _ (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 4 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [5] _ (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [5] _ (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [5] _ (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [6] _ (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6 || v_1.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (ISELB [6] _ (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6 || v_1.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [6] _ (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 6 || v_1.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (ISELB [n] (MOVDconst [1]) (InvertFlags bool))
    // cond: n%4 == 0
    // result: (ISELB [n+1] (MOVDconst [1]) bool)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_0.AuxInt) != 1 || v_1.Op != OpPPC64InvertFlags) {
            break;
        }
        var @bool = v_1.Args[0];
        if (!(n % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(n + 1);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, bool);
        return true;
    } 
    // match: (ISELB [n] (MOVDconst [1]) (InvertFlags bool))
    // cond: n%4 == 1
    // result: (ISELB [n-1] (MOVDconst [1]) bool)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_0.AuxInt) != 1 || v_1.Op != OpPPC64InvertFlags) {
            break;
        }
        @bool = v_1.Args[0];
        if (!(n % 4 == 1)) {
            break;
        }
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(n - 1);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, bool);
        return true;
    } 
    // match: (ISELB [n] (MOVDconst [1]) (InvertFlags bool))
    // cond: n%4 == 2
    // result: (ISELB [n] (MOVDconst [1]) bool)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_0.AuxInt) != 1 || v_1.Op != OpPPC64InvertFlags) {
            break;
        }
        @bool = v_1.Args[0];
        if (!(n % 4 == 2)) {
            break;
        }
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(n);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, bool);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64LessEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (LessEqual (FlagEQ))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (LessEqual (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (LessEqual (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (LessEqual (InvertFlags x))
    // result: (GreaterEqual x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64GreaterEqual);
        v.AddArg(x);
        return true;
    } 
    // match: (LessEqual cmp)
    // result: (ISELB [5] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(5);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64LessThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (LessThan (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (LessThan (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (LessThan (FlagGT))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (LessThan (InvertFlags x))
    // result: (GreaterThan x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64GreaterThan);
        v.AddArg(x);
        return true;
    } 
    // match: (LessThan cmp)
    // result: (ISELB [0] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64MFVSRD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MFVSRD (FMOVDconst [c]))
    // result: (MOVDconst [int64(math.Float64bits(c))])
    while (true) {
        if (v_0.Op != OpPPC64FMOVDconst) {
            break;
        }
        var c = auxIntToFloat64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(math.Float64bits(c)));
        return true;
    } 
    // match: (MFVSRD x:(FMOVDload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVDload [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != OpPPC64FMOVDload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpPPC64MOVDload, typ.Int64);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBZload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVBZload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBZload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVBZload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVBZload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVBZloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVBZloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBZloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBZloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c)
    // result: (MOVBZload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBZloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c)
    // result: (MOVBZload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVBZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBZreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0xFF
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0xFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVBZreg (SRWconst [c] (MOVBZreg x)))
    // result: (SRWconst [c] (MOVBZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBZreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVBZreg (SRWconst [c] x))
    // cond: sizeof(x.Type) == 8
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) == 8)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg (SRDconst [c] x))
    // cond: c>=56
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 56)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg (SRWconst [c] x))
    // cond: c>=24
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 24)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg y:(MOVBZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVBZreg (MOVBreg x))
    // result: (MOVBZreg x)
    while (true) {
        if (v_0.Op != OpPPC64MOVBreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64MOVBZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBZreg (OR <t> x (MOVWZreg y)))
    // result: (MOVBZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        var t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (XOR <t> x (MOVWZreg y)))
    // result: (MOVBZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (AND <t> x (MOVWZreg y)))
    // result: (MOVBZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (OR <t> x (MOVHZreg y)))
    // result: (MOVBZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (XOR <t> x (MOVHZreg y)))
    // result: (MOVBZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (AND <t> x (MOVHZreg y)))
    // result: (MOVBZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (OR <t> x (MOVBZreg y)))
    // result: (MOVBZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVBZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (XOR <t> x (MOVBZreg y)))
    // result: (MOVBZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVBZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg (AND <t> x (MOVBZreg y)))
    // result: (MOVBZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVBZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVBZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg z:(ANDconst [c] (MOVBZload ptr x)))
    // result: z
    while (true) {
        var z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        var z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVBZreg z:(AND y (MOVBZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64AND) {
            break;
        }
        _ = z.Args[1];
        z_0 = z.Args[0];
        var z_1 = z.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (z_1.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                }
                v.copyOf(z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVBZreg x:(MOVBZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg x:(MOVBZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg x:(Arg <t>))
    // cond: is8BitInt(t) && !isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!(is8BitInt(t) && !isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBZreg (MOVDconst [c]))
    // result: (MOVDconst [int64(uint8(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint8(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0x7F
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0x7F)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVBreg (SRAWconst [c] (MOVBreg x)))
    // result: (SRAWconst [c] (MOVBreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVBreg (SRAWconst [c] x))
    // cond: sizeof(x.Type) == 8
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) == 8)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRDconst [c] x))
    // cond: c>56
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c > 56)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRDconst [c] x))
    // cond: c==56
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c == 56)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRADconst [c] x))
    // cond: c>=56
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRADconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 56)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRWconst [c] x))
    // cond: c>24
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c > 24)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRWconst [c] x))
    // cond: c==24
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c == 24)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg (SRAWconst [c] x))
    // cond: c>=24
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 24)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg y:(MOVBreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVBreg (MOVBZreg x))
    // result: (MOVBreg x)
    while (true) {
        if (v_0.Op != OpPPC64MOVBZreg) {
            break;
        }
        x = v_0.Args[0];
        v.reset(OpPPC64MOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBreg x:(Arg <t>))
    // cond: is8BitInt(t) && isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        var t = x.Type;
        if (!(is8BitInt(t) && isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVBreg (MOVDconst [c]))
    // result: (MOVDconst [int64(int8(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int8(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBstore [off1] {sym} (ADDconst [off2] x) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVBstore [off1+int32(off2)] {sym} x val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(x, val, mem);
        return true;
    } 
    // match: (MOVBstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVDconst [0]) mem)
    // result: (MOVBstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstore [0] {sym} p:(ADD ptr idx) val mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVBstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVBreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
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
        if (v_1.Op != OpPPC64MOVBZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVHZreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVWreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVWZreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (SRWconst (MOVHreg x) [c]) mem)
    // cond: c <= 8
    // result: (MOVBstore [off] {sym} ptr (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64SRWconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_1_0.Args[0];
        mem = v_2;
        if (!(c <= 8)) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        var v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg3(ptr, v0, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (SRWconst (MOVHZreg x) [c]) mem)
    // cond: c <= 8
    // result: (MOVBstore [off] {sym} ptr (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_1_0.Args[0];
        mem = v_2;
        if (!(c <= 8)) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg3(ptr, v0, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (SRWconst (MOVWreg x) [c]) mem)
    // cond: c <= 24
    // result: (MOVBstore [off] {sym} ptr (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_1_0.Args[0];
        mem = v_2;
        if (!(c <= 24)) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg3(ptr, v0, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (SRWconst (MOVWZreg x) [c]) mem)
    // cond: c <= 24
    // result: (MOVBstore [off] {sym} ptr (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1_0.Args[0];
        mem = v_2;
        if (!(c <= 24)) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg3(ptr, v0, mem);
        return true;
    } 
    // match: (MOVBstore [i1] {s} p (SRWconst w [24]) x0:(MOVBstore [i0] {s} p (SRWconst w [16]) mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+1 && clobber(x0)
    // result: (MOVHstore [i0] {s} p (SRWconst <typ.UInt16> w [16]) mem)
    while (true) {
        var i1 = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRWconst || auxIntToInt64(v_1.AuxInt) != 24) {
            break;
        }
        var w = v_1.Args[0];
        var x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        var i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        var x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRWconst || auxIntToInt64(x0_1.AuxInt) != 16 || w != x0_1.Args[0] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 1 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v0 = b.NewValue0(x0.Pos, OpPPC64SRWconst, typ.UInt16);
        v0.AuxInt = int64ToAuxInt(16);
        v0.AddArg(w);
        v.AddArg3(p, v0, mem);
        return true;
    } 
    // match: (MOVBstore [i1] {s} p (SRDconst w [24]) x0:(MOVBstore [i0] {s} p (SRDconst w [16]) mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+1 && clobber(x0)
    // result: (MOVHstore [i0] {s} p (SRWconst <typ.UInt16> w [16]) mem)
    while (true) {
        i1 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRDconst || auxIntToInt64(v_1.AuxInt) != 24) {
            break;
        }
        w = v_1.Args[0];
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRDconst || auxIntToInt64(x0_1.AuxInt) != 16 || w != x0_1.Args[0] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 1 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v0 = b.NewValue0(x0.Pos, OpPPC64SRWconst, typ.UInt16);
        v0.AuxInt = int64ToAuxInt(16);
        v0.AddArg(w);
        v.AddArg3(p, v0, mem);
        return true;
    } 
    // match: (MOVBstore [i1] {s} p (SRWconst w [8]) x0:(MOVBstore [i0] {s} p w mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+1 && clobber(x0)
    // result: (MOVHstore [i0] {s} p w mem)
    while (true) {
        i1 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRWconst || auxIntToInt64(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0] || w != x0.Args[1] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 1 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i1] {s} p (SRDconst w [8]) x0:(MOVBstore [i0] {s} p w mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+1 && clobber(x0)
    // result: (MOVHstore [i0] {s} p w mem)
    while (true) {
        i1 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRDconst || auxIntToInt64(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0] || w != x0.Args[1] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 1 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i3] {s} p w x0:(MOVBstore [i2] {s} p (SRWconst w [8]) x1:(MOVBstore [i1] {s} p (SRWconst w [16]) x2:(MOVBstore [i0] {s} p (SRWconst w [24]) mem))))
    // cond: !config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && i1 == i0+1 && i2 == i0+2 && i3 == i0+3 && clobber(x0, x1, x2)
    // result: (MOVWBRstore (MOVDaddr <typ.Uintptr> [i0] {s} p) w mem)
    while (true) {
        var i3 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        var i2 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        _ = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRWconst || auxIntToInt64(x0_1.AuxInt) != 8 || w != x0_1.Args[0]) {
            break;
        }
        var x1 = x0.Args[2];
        if (x1.Op != OpPPC64MOVBstore) {
            break;
        }
        i1 = auxIntToInt32(x1.AuxInt);
        if (auxToSym(x1.Aux) != s) {
            break;
        }
        _ = x1.Args[2];
        if (p != x1.Args[0]) {
            break;
        }
        var x1_1 = x1.Args[1];
        if (x1_1.Op != OpPPC64SRWconst || auxIntToInt64(x1_1.AuxInt) != 16 || w != x1_1.Args[0]) {
            break;
        }
        var x2 = x1.Args[2];
        if (x2.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x2.AuxInt);
        if (auxToSym(x2.Aux) != s) {
            break;
        }
        mem = x2.Args[2];
        if (p != x2.Args[0]) {
            break;
        }
        var x2_1 = x2.Args[1];
        if (x2_1.Op != OpPPC64SRWconst || auxIntToInt64(x2_1.AuxInt) != 24 || w != x2_1.Args[0] || !(!config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && i1 == i0 + 1 && i2 == i0 + 2 && i3 == i0 + 3 && clobber(x0, x1, x2))) {
            break;
        }
        v.reset(OpPPC64MOVWBRstore);
        v0 = b.NewValue0(x2.Pos, OpPPC64MOVDaddr, typ.Uintptr);
        v0.AuxInt = int32ToAuxInt(i0);
        v0.Aux = symToAux(s);
        v0.AddArg(p);
        v.AddArg3(v0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i1] {s} p w x0:(MOVBstore [i0] {s} p (SRWconst w [8]) mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+1 && clobber(x0)
    // result: (MOVHBRstore (MOVDaddr <typ.Uintptr> [i0] {s} p) w mem)
    while (true) {
        i1 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRWconst || auxIntToInt64(x0_1.AuxInt) != 8 || w != x0_1.Args[0] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 1 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVHBRstore);
        v0 = b.NewValue0(x0.Pos, OpPPC64MOVDaddr, typ.Uintptr);
        v0.AuxInt = int32ToAuxInt(i0);
        v0.Aux = symToAux(s);
        v0.AddArg(p);
        v.AddArg3(v0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i7] {s} p (SRDconst w [56]) x0:(MOVBstore [i6] {s} p (SRDconst w [48]) x1:(MOVBstore [i5] {s} p (SRDconst w [40]) x2:(MOVBstore [i4] {s} p (SRDconst w [32]) x3:(MOVWstore [i0] {s} p w mem)))))
    // cond: !config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && i4 == i0+4 && i5 == i0+5 && i6 == i0+6 && i7 == i0+7 && clobber(x0, x1, x2, x3)
    // result: (MOVDstore [i0] {s} p w mem)
    while (true) {
        var i7 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRDconst || auxIntToInt64(v_1.AuxInt) != 56) {
            break;
        }
        w = v_1.Args[0];
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        var i6 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        _ = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRDconst || auxIntToInt64(x0_1.AuxInt) != 48 || w != x0_1.Args[0]) {
            break;
        }
        x1 = x0.Args[2];
        if (x1.Op != OpPPC64MOVBstore) {
            break;
        }
        var i5 = auxIntToInt32(x1.AuxInt);
        if (auxToSym(x1.Aux) != s) {
            break;
        }
        _ = x1.Args[2];
        if (p != x1.Args[0]) {
            break;
        }
        x1_1 = x1.Args[1];
        if (x1_1.Op != OpPPC64SRDconst || auxIntToInt64(x1_1.AuxInt) != 40 || w != x1_1.Args[0]) {
            break;
        }
        x2 = x1.Args[2];
        if (x2.Op != OpPPC64MOVBstore) {
            break;
        }
        var i4 = auxIntToInt32(x2.AuxInt);
        if (auxToSym(x2.Aux) != s) {
            break;
        }
        _ = x2.Args[2];
        if (p != x2.Args[0]) {
            break;
        }
        x2_1 = x2.Args[1];
        if (x2_1.Op != OpPPC64SRDconst || auxIntToInt64(x2_1.AuxInt) != 32 || w != x2_1.Args[0]) {
            break;
        }
        var x3 = x2.Args[2];
        if (x3.Op != OpPPC64MOVWstore) {
            break;
        }
        i0 = auxIntToInt32(x3.AuxInt);
        if (auxToSym(x3.Aux) != s) {
            break;
        }
        mem = x3.Args[2];
        if (p != x3.Args[0] || w != x3.Args[1] || !(!config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && i4 == i0 + 4 && i5 == i0 + 5 && i6 == i0 + 6 && i7 == i0 + 7 && clobber(x0, x1, x2, x3))) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i7] {s} p w x0:(MOVBstore [i6] {s} p (SRDconst w [8]) x1:(MOVBstore [i5] {s} p (SRDconst w [16]) x2:(MOVBstore [i4] {s} p (SRDconst w [24]) x3:(MOVBstore [i3] {s} p (SRDconst w [32]) x4:(MOVBstore [i2] {s} p (SRDconst w [40]) x5:(MOVBstore [i1] {s} p (SRDconst w [48]) x6:(MOVBstore [i0] {s} p (SRDconst w [56]) mem))))))))
    // cond: !config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && i1 == i0+1 && i2 == i0+2 && i3 == i0+3 && i4 == i0+4 && i5 == i0+5 && i6 == i0+6 && i7 == i0+7 && clobber(x0, x1, x2, x3, x4, x5, x6)
    // result: (MOVDBRstore (MOVDaddr <typ.Uintptr> [i0] {s} p) w mem)
    while (true) {
        i7 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x0 = v_2;
        if (x0.Op != OpPPC64MOVBstore) {
            break;
        }
        i6 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        _ = x0.Args[2];
        if (p != x0.Args[0]) {
            break;
        }
        x0_1 = x0.Args[1];
        if (x0_1.Op != OpPPC64SRDconst || auxIntToInt64(x0_1.AuxInt) != 8 || w != x0_1.Args[0]) {
            break;
        }
        x1 = x0.Args[2];
        if (x1.Op != OpPPC64MOVBstore) {
            break;
        }
        i5 = auxIntToInt32(x1.AuxInt);
        if (auxToSym(x1.Aux) != s) {
            break;
        }
        _ = x1.Args[2];
        if (p != x1.Args[0]) {
            break;
        }
        x1_1 = x1.Args[1];
        if (x1_1.Op != OpPPC64SRDconst || auxIntToInt64(x1_1.AuxInt) != 16 || w != x1_1.Args[0]) {
            break;
        }
        x2 = x1.Args[2];
        if (x2.Op != OpPPC64MOVBstore) {
            break;
        }
        i4 = auxIntToInt32(x2.AuxInt);
        if (auxToSym(x2.Aux) != s) {
            break;
        }
        _ = x2.Args[2];
        if (p != x2.Args[0]) {
            break;
        }
        x2_1 = x2.Args[1];
        if (x2_1.Op != OpPPC64SRDconst || auxIntToInt64(x2_1.AuxInt) != 24 || w != x2_1.Args[0]) {
            break;
        }
        x3 = x2.Args[2];
        if (x3.Op != OpPPC64MOVBstore) {
            break;
        }
        i3 = auxIntToInt32(x3.AuxInt);
        if (auxToSym(x3.Aux) != s) {
            break;
        }
        _ = x3.Args[2];
        if (p != x3.Args[0]) {
            break;
        }
        var x3_1 = x3.Args[1];
        if (x3_1.Op != OpPPC64SRDconst || auxIntToInt64(x3_1.AuxInt) != 32 || w != x3_1.Args[0]) {
            break;
        }
        var x4 = x3.Args[2];
        if (x4.Op != OpPPC64MOVBstore) {
            break;
        }
        i2 = auxIntToInt32(x4.AuxInt);
        if (auxToSym(x4.Aux) != s) {
            break;
        }
        _ = x4.Args[2];
        if (p != x4.Args[0]) {
            break;
        }
        var x4_1 = x4.Args[1];
        if (x4_1.Op != OpPPC64SRDconst || auxIntToInt64(x4_1.AuxInt) != 40 || w != x4_1.Args[0]) {
            break;
        }
        var x5 = x4.Args[2];
        if (x5.Op != OpPPC64MOVBstore) {
            break;
        }
        i1 = auxIntToInt32(x5.AuxInt);
        if (auxToSym(x5.Aux) != s) {
            break;
        }
        _ = x5.Args[2];
        if (p != x5.Args[0]) {
            break;
        }
        var x5_1 = x5.Args[1];
        if (x5_1.Op != OpPPC64SRDconst || auxIntToInt64(x5_1.AuxInt) != 48 || w != x5_1.Args[0]) {
            break;
        }
        var x6 = x5.Args[2];
        if (x6.Op != OpPPC64MOVBstore) {
            break;
        }
        i0 = auxIntToInt32(x6.AuxInt);
        if (auxToSym(x6.Aux) != s) {
            break;
        }
        mem = x6.Args[2];
        if (p != x6.Args[0]) {
            break;
        }
        var x6_1 = x6.Args[1];
        if (x6_1.Op != OpPPC64SRDconst || auxIntToInt64(x6_1.AuxInt) != 56 || w != x6_1.Args[0] || !(!config.BigEndian && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && i1 == i0 + 1 && i2 == i0 + 2 && i3 == i0 + 3 && i4 == i0 + 4 && i5 == i0 + 5 && i6 == i0 + 6 && i7 == i0 + 7 && clobber(x0, x1, x2, x3, x4, x5, x6))) {
            break;
        }
        v.reset(OpPPC64MOVDBRstore);
        v0 = b.NewValue0(x6.Pos, OpPPC64MOVDaddr, typ.Uintptr);
        v0.AuxInt = int32ToAuxInt(i0);
        v0.Aux = symToAux(s);
        v0.AddArg(p);
        v.AddArg3(v0, w, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVBstoreidx ptr (MOVDconst [c]) val mem)
    // cond: is16Bit(c)
    // result: (MOVBstore [int32(c)] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVBstoreidx (MOVDconst [c]) ptr val mem)
    // cond: is16Bit(c)
    // result: (MOVBstore [int32(c)] ptr val mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVBstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVBreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpPPC64MOVBreg) {
            break;
        }
        var x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVBZreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVBZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVHreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVHZreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVWreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (MOVWZreg x) mem)
    // result: (MOVBstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVBstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (SRWconst (MOVHreg x) [c]) mem)
    // cond: c <= 8
    // result: (MOVBstoreidx ptr idx (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_2.AuxInt);
        var v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_2_0.Args[0];
        mem = v_3;
        if (!(c <= 8)) {
            break;
        }
        v.reset(OpPPC64MOVBstoreidx);
        var v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg4(ptr, idx, v0, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (SRWconst (MOVHZreg x) [c]) mem)
    // cond: c <= 8
    // result: (MOVBstoreidx ptr idx (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_2.AuxInt);
        v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_2_0.Args[0];
        mem = v_3;
        if (!(c <= 8)) {
            break;
        }
        v.reset(OpPPC64MOVBstoreidx);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg4(ptr, idx, v0, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (SRWconst (MOVWreg x) [c]) mem)
    // cond: c <= 24
    // result: (MOVBstoreidx ptr idx (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_2.AuxInt);
        v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_2_0.Args[0];
        mem = v_3;
        if (!(c <= 24)) {
            break;
        }
        v.reset(OpPPC64MOVBstoreidx);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg4(ptr, idx, v0, mem);
        return true;
    } 
    // match: (MOVBstoreidx ptr idx (SRWconst (MOVWZreg x) [c]) mem)
    // cond: c <= 24
    // result: (MOVBstoreidx ptr idx (SRWconst <typ.UInt32> x [c]) mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_2.AuxInt);
        v_2_0 = v_2.Args[0];
        if (v_2_0.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_2_0.Args[0];
        mem = v_3;
        if (!(c <= 24)) {
            break;
        }
        v.reset(OpPPC64MOVBstoreidx);
        v0 = b.NewValue0(v.Pos, OpPPC64SRWconst, typ.UInt32);
        v0.AuxInt = int64ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg4(ptr, idx, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVBstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstorezero [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVBstorezero [off1+int32(off2)] {sym} x mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVBstorezero [off1] {sym1} p:(MOVDaddr [off2] {sym2} x) mem)
    // cond: canMergeSym(sym1,sym2) && (x.Op != OpSB || p.Uses == 1)
    // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        x = p.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && (x.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDload [off] {sym} ptr (FMOVDstore [off] {sym} ptr x _))
    // result: (MFVSRD x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64FMOVDstore || auxIntToInt32(v_1.AuxInt) != off || auxToSym(v_1.Aux) != sym) {
            break;
        }
        var x = v_1.Args[1];
        if (ptr != v_1.Args[0]) {
            break;
        }
        v.reset(OpPPC64MFVSRD);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVDload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVDload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVDload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVDload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVDloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVDloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVDloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVDload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVDload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstore [off] {sym} ptr (MFVSRD x) mem)
    // result: (FMOVDstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64MFVSRD) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpPPC64FMOVDstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVDstore [off1] {sym} (ADDconst [off2] x) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVDstore [off1+int32(off2)] {sym} x val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(x, val, mem);
        return true;
    } 
    // match: (MOVDstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVDstore [off] {sym} ptr (MOVDconst [0]) mem)
    // result: (MOVDstorezero [off] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVDstore [0] {sym} p:(ADD ptr idx) val mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVDstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVDstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVDstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstoreidx ptr (MOVDconst [c]) val mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVDstore [int32(c)] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVDstoreidx (MOVDconst [c]) ptr val mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVDstore [int32(c)] ptr val mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVDstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstorezero [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVDstorezero [off1+int32(off2)] {sym} x mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVDstorezero [off1] {sym1} p:(MOVDaddr [off2] {sym2} x) mem)
    // cond: canMergeSym(sym1,sym2) && (x.Op != OpSB || p.Uses == 1)
    // result: (MOVDstorezero [off1+off2] {mergeSym(sym1,sym2)} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        x = p.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && (x.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHBRstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHBRstore {sym} ptr (MOVHreg x) mem)
    // result: (MOVHBRstore {sym} ptr x mem)
    while (true) {
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVHreg) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpPPC64MOVHBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHBRstore {sym} ptr (MOVHZreg x) mem)
    // result: (MOVHBRstore {sym} ptr x mem)
    while (true) {
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHBRstore {sym} ptr (MOVWreg x) mem)
    // result: (MOVHBRstore {sym} ptr x mem)
    while (true) {
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHBRstore {sym} ptr (MOVWZreg x) mem)
    // result: (MOVHBRstore {sym} ptr x mem)
    while (true) {
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHZload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVHZload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVHZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHZload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVHZload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVHZload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVHZload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVHZloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVHZloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHZloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHZloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c)
    // result: (MOVHZload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHZloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c)
    // result: (MOVHZload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVHZreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0xFFFF
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0xFFFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHZreg (SRWconst [c] (MOVBZreg x)))
    // result: (SRWconst [c] (MOVBZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBZreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVHZreg (SRWconst [c] (MOVHZreg x)))
    // result: (SRWconst [c] (MOVHZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVHZreg (SRWconst [c] x))
    // cond: sizeof(x.Type) <= 16
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) <= 16)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg (SRDconst [c] x))
    // cond: c>=48
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 48)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg (SRWconst [c] x))
    // cond: c>=16
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 16)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg y:(MOVHZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHZreg y:(MOVBZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHZreg y:(MOVHBRload _ _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHBRload) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHZreg y:(MOVHreg x))
    // result: (MOVHZreg x)
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHreg) {
            break;
        }
        x = y.Args[0];
        v.reset(OpPPC64MOVHZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHZreg (OR <t> x (MOVWZreg y)))
    // result: (MOVHZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        var t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg (XOR <t> x (MOVWZreg y)))
    // result: (MOVHZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg (AND <t> x (MOVWZreg y)))
    // result: (MOVHZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg (OR <t> x (MOVHZreg y)))
    // result: (MOVHZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg (XOR <t> x (MOVHZreg y)))
    // result: (MOVHZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg (AND <t> x (MOVHZreg y)))
    // result: (MOVHZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVHZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVHZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg z:(ANDconst [c] (MOVBZload ptr x)))
    // result: z
    while (true) {
        var z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        var z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVHZreg z:(ANDconst [c] (MOVHZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVHZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVHZreg z:(AND y (MOVHZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64AND) {
            break;
        }
        _ = z.Args[1];
        z_0 = z.Args[0];
        var z_1 = z.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (z_1.Op != OpPPC64MOVHZload) {
                    continue;
                (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                }
                v.copyOf(z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVHZreg x:(MOVBZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg x:(MOVBZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg x:(MOVHZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg x:(MOVHZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg x:(Arg <t>))
    // cond: (is8BitInt(t) || is16BitInt(t)) && !isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!((is8BitInt(t) || is16BitInt(t)) && !isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHZreg (MOVDconst [c]))
    // result: (MOVDconst [int64(uint16(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint16(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVHload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVHload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVHload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVHloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVHloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c)
    // result: (MOVHload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c)
    // result: (MOVHload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVHreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0x7FFF
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0x7FFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHreg (SRAWconst [c] (MOVBreg x)))
    // result: (SRAWconst [c] (MOVBreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVHreg (SRAWconst [c] (MOVHreg x)))
    // result: (SRAWconst [c] (MOVHreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVHreg (SRAWconst [c] x))
    // cond: sizeof(x.Type) <= 16
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) <= 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRDconst [c] x))
    // cond: c>48
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c > 48)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRDconst [c] x))
    // cond: c==48
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c == 48)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRADconst [c] x))
    // cond: c>=48
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRADconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 48)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRWconst [c] x))
    // cond: c>16
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c > 16)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRAWconst [c] x))
    // cond: c>=16
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg (SRWconst [c] x))
    // cond: c==16
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c == 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg y:(MOVHreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHreg y:(MOVBreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVHreg y:(MOVHZreg x))
    // result: (MOVHreg x)
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = y.Args[0];
        v.reset(OpPPC64MOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVHreg x:(MOVHload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg x:(MOVHloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg x:(Arg <t>))
    // cond: (is8BitInt(t) || is16BitInt(t)) && isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        var t = x.Type;
        if (!((is8BitInt(t) || is16BitInt(t)) && isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVHreg (MOVDconst [c]))
    // result: (MOVDconst [int64(int16(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int16(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVHstore [off1] {sym} (ADDconst [off2] x) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVHstore [off1+int32(off2)] {sym} x val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(x, val, mem);
        return true;
    } 
    // match: (MOVHstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVDconst [0]) mem)
    // result: (MOVHstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpPPC64MOVHstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVHstore [0] {sym} p:(ADD ptr idx) val mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVHstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVHstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHstore);
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
        if (v_1.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVWreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHstore [off] {sym} ptr (MOVWZreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVHstore [i1] {s} p (SRWconst w [16]) x0:(MOVHstore [i0] {s} p w mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+2 && clobber(x0)
    // result: (MOVWstore [i0] {s} p w mem)
    while (true) {
        var i1 = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRWconst || auxIntToInt64(v_1.AuxInt) != 16) {
            break;
        }
        var w = v_1.Args[0];
        var x0 = v_2;
        if (x0.Op != OpPPC64MOVHstore) {
            break;
        }
        var i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0] || w != x0.Args[1] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 2 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVHstore [i1] {s} p (SRDconst w [16]) x0:(MOVHstore [i0] {s} p w mem))
    // cond: !config.BigEndian && x0.Uses == 1 && i1 == i0+2 && clobber(x0)
    // result: (MOVWstore [i0] {s} p w mem)
    while (true) {
        i1 = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != OpPPC64SRDconst || auxIntToInt64(v_1.AuxInt) != 16) {
            break;
        }
        w = v_1.Args[0];
        x0 = v_2;
        if (x0.Op != OpPPC64MOVHstore) {
            break;
        }
        i0 = auxIntToInt32(x0.AuxInt);
        if (auxToSym(x0.Aux) != s) {
            break;
        }
        mem = x0.Args[2];
        if (p != x0.Args[0] || w != x0.Args[1] || !(!config.BigEndian && x0.Uses == 1 && i1 == i0 + 2 && clobber(x0))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(i0);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstoreidx ptr (MOVDconst [c]) val mem)
    // cond: is16Bit(c)
    // result: (MOVHstore [int32(c)] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVHstoreidx (MOVDconst [c]) ptr val mem)
    // cond: is16Bit(c)
    // result: (MOVHstore [int32(c)] ptr val mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVHstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVHstoreidx ptr idx (MOVHreg x) mem)
    // result: (MOVHstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpPPC64MOVHreg) {
            break;
        }
        var x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVHstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVHstoreidx ptr idx (MOVHZreg x) mem)
    // result: (MOVHstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVHstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVHstoreidx ptr idx (MOVWreg x) mem)
    // result: (MOVHstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVHstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVHstoreidx ptr idx (MOVWZreg x) mem)
    // result: (MOVHstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVHstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVHstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstorezero [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVHstorezero [off1+int32(off2)] {sym} x mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVHstorezero);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVHstorezero [off1] {sym1} p:(MOVDaddr [off2] {sym2} x) mem)
    // cond: canMergeSym(sym1,sym2) && (x.Op != OpSB || p.Uses == 1)
    // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        x = p.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && (x.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVHstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWBRstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWBRstore {sym} ptr (MOVWreg x) mem)
    // result: (MOVWBRstore {sym} ptr x mem)
    while (true) {
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(OpPPC64MOVWBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVWBRstore {sym} ptr (MOVWZreg x) mem)
    // result: (MOVWBRstore {sym} ptr x mem)
    while (true) {
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVWBRstore);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWZload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWZload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVWZload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVWZload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWZload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVWZload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVWZload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVWZload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVWZloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVWZloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWZloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWZloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c)
    // result: (MOVWZload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVWZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWZloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c)
    // result: (MOVWZload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVWZload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWZreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVWZreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0xFFFFFFFF
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0xFFFFFFFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(AND (MOVDconst [c]) _))
    // cond: uint64(c) <= 0xFFFFFFFF
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64AND) {
            break;
        }
        var y_0 = y.Args[0];
        var y_1 = y.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (y_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                c = auxIntToInt64(y_0.AuxInt);
                if (!(uint64(c) <= 0xFFFFFFFF)) {
                    continue;
                }
                v.copyOf(y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVWZreg (SRWconst [c] (MOVBZreg x)))
    // result: (SRWconst [c] (MOVBZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBZreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWZreg (SRWconst [c] (MOVHZreg x)))
    // result: (SRWconst [c] (MOVHZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWZreg (SRWconst [c] (MOVWZreg x)))
    // result: (SRWconst [c] (MOVWZreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWZreg (SRWconst [c] x))
    // cond: sizeof(x.Type) <= 32
    // result: (SRWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) <= 32)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg (SRDconst [c] x))
    // cond: c>=32
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 32)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg y:(MOVWZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVWZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(MOVHZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(MOVBZreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBZreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(MOVHBRload _ _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHBRload) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(MOVWBRload _ _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVWBRload) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWZreg y:(MOVWreg x))
    // result: (MOVWZreg x)
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVWreg) {
            break;
        }
        x = y.Args[0];
        v.reset(OpPPC64MOVWZreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWZreg (OR <t> x (MOVWZreg y)))
    // result: (MOVWZreg (OR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64OR) {
            break;
        }
        var t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVWZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64OR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVWZreg (XOR <t> x (MOVWZreg y)))
    // result: (MOVWZreg (XOR <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64XOR) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVWZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64XOR, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVWZreg (AND <t> x (MOVWZreg y)))
    // result: (MOVWZreg (AND <t> x y))
    while (true) {
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        t = v_0.Type;
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                if (v_0_1.Op != OpPPC64MOVWZreg) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                y = v_0_1.Args[0];
                v.reset(OpPPC64MOVWZreg);
                v0 = b.NewValue0(v.Pos, OpPPC64AND, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVWZreg z:(ANDconst [c] (MOVBZload ptr x)))
    // result: z
    while (true) {
        var z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        var z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVWZreg z:(ANDconst [c] (MOVHZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVHZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVWZreg z:(ANDconst [c] (MOVWZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        z_0 = z.Args[0];
        if (z_0.Op != OpPPC64MOVWZload) {
            break;
        }
        v.copyOf(z);
        return true;
    } 
    // match: (MOVWZreg z:(AND y (MOVWZload ptr x)))
    // result: z
    while (true) {
        z = v_0;
        if (z.Op != OpPPC64AND) {
            break;
        }
        _ = z.Args[1];
        z_0 = z.Args[0];
        var z_1 = z.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (z_1.Op != OpPPC64MOVWZload) {
                    continue;
                (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                }
                v.copyOf(z);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVWZreg x:(MOVBZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVBZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVBZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVHZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVHZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVWZload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVWZload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(MOVWZloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVWZloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg x:(Arg <t>))
    // cond: (is8BitInt(t) || is16BitInt(t) || is32BitInt(t)) && !isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        t = x.Type;
        if (!((is8BitInt(t) || is16BitInt(t) || is32BitInt(t)) && !isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWZreg (MOVDconst [c]))
    // result: (MOVDconst [int64(uint32(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(uint32(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWload [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        var off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        var mem = v_1;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWload [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVWload [off1+int32(off2)] {sym} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVWload);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVWload [0] {sym} p:(ADD ptr idx) mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVWloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        mem = v_1;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVWloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWloadidx ptr (MOVDconst [c]) mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVWload [int32(c)] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVWload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWloadidx (MOVDconst [c]) ptr mem)
    // cond: is16Bit(c) && c%4 == 0
    // result: (MOVWload [int32(c)] ptr mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        if (!(is16Bit(c) && c % 4 == 0)) {
            break;
        }
        v.reset(OpPPC64MOVWload);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVWreg y:(ANDconst [c] _))
    // cond: uint64(c) <= 0xFFFF
    // result: y
    while (true) {
        var y = v_0;
        if (y.Op != OpPPC64ANDconst) {
            break;
        }
        var c = auxIntToInt64(y.AuxInt);
        if (!(uint64(c) <= 0xFFFF)) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWreg y:(AND (MOVDconst [c]) _))
    // cond: uint64(c) <= 0x7FFFFFFF
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64AND) {
            break;
        }
        var y_0 = y.Args[0];
        var y_1 = y.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (y_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                c = auxIntToInt64(y_0.AuxInt);
                if (!(uint64(c) <= 0x7FFFFFFF)) {
                    continue;
                }
                v.copyOf(y);
                return true;
            }

        }
        break;
    } 
    // match: (MOVWreg (SRAWconst [c] (MOVBreg x)))
    // result: (SRAWconst [c] (MOVBreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVBreg) {
            break;
        }
        var x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWreg (SRAWconst [c] (MOVHreg x)))
    // result: (SRAWconst [c] (MOVHreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVHreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWreg (SRAWconst [c] (MOVWreg x)))
    // result: (SRAWconst [c] (MOVWreg x))
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_0_0.Args[0];
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MOVWreg (SRAWconst [c] x))
    // cond: sizeof(x.Type) <= 32
    // result: (SRAWconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRAWconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(sizeof(x.Type) <= 32)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg (SRDconst [c] x))
    // cond: c>32
    // result: (SRDconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c > 32)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg (SRADconst [c] x))
    // cond: c>=32
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRADconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c >= 32)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg (SRDconst [c] x))
    // cond: c==32
    // result: (SRADconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64SRDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c == 32)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg y:(MOVWreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVWreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWreg y:(MOVHreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVHreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWreg y:(MOVBreg _))
    // result: y
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVBreg) {
            break;
        }
        v.copyOf(y);
        return true;
    } 
    // match: (MOVWreg y:(MOVWZreg x))
    // result: (MOVWreg x)
    while (true) {
        y = v_0;
        if (y.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = y.Args[0];
        v.reset(OpPPC64MOVWreg);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVHload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVHloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVHloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVWload _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVWload) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(MOVWloadidx _ _ _))
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpPPC64MOVWloadidx) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg x:(Arg <t>))
    // cond: (is8BitInt(t) || is16BitInt(t) || is32BitInt(t)) && isSigned(t)
    // result: x
    while (true) {
        x = v_0;
        if (x.Op != OpArg) {
            break;
        }
        var t = x.Type;
        if (!((is8BitInt(t) || is16BitInt(t) || is32BitInt(t)) && isSigned(t))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVWreg (MOVDconst [c]))
    // result: (MOVDconst [int64(int32(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(int64(int32(c)));
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstore [off1] {sym} (ADDconst [off2] x) val mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVWstore [off1+int32(off2)] {sym} x val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg3(x, val, mem);
        return true;
    } 
    // match: (MOVWstore [off1] {sym1} p:(MOVDaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2) && is16Bit(int64(off1+off2)) && (ptr.Op != OpSB || p.Uses == 1)
    // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        var ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2) && is16Bit(int64(off1 + off2)) && (ptr.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVDconst [0]) mem)
    // result: (MOVWstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpPPC64MOVWstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstore [0] {sym} p:(ADD ptr idx) val mem)
    // cond: sym == nil && p.Uses == 1
    // result: (MOVWstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        p = v_0;
        if (p.Op != OpPPC64ADD) {
            break;
        }
        var idx = p.Args[1];
        ptr = p.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null && p.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64MOVWstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpPPC64MOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVWstore);
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
        if (v_1.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstoreidx ptr (MOVDconst [c]) val mem)
    // cond: is16Bit(c)
    // result: (MOVWstore [int32(c)] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVWstoreidx (MOVDconst [c]) ptr val mem)
    // cond: is16Bit(c)
    // result: (MOVWstore [int32(c)] ptr val mem)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        if (!(is16Bit(c))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVWstoreidx ptr idx (MOVWreg x) mem)
    // result: (MOVWstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpPPC64MOVWreg) {
            break;
        }
        var x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVWstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    } 
    // match: (MOVWstoreidx ptr idx (MOVWZreg x) mem)
    // result: (MOVWstoreidx ptr idx x mem)
    while (true) {
        ptr = v_0;
        idx = v_1;
        if (v_2.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = v_2.Args[0];
        mem = v_3;
        v.reset(OpPPC64MOVWstoreidx);
        v.AddArg4(ptr, idx, x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MOVWstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstorezero [off1] {sym} (ADDconst [off2] x) mem)
    // cond: is16Bit(int64(off1)+off2)
    // result: (MOVWstorezero [off1+int32(off2)] {sym} x mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var off2 = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1) + off2))) {
            break;
        }
        v.reset(OpPPC64MOVWstorezero);
        v.AuxInt = int32ToAuxInt(off1 + int32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(x, mem);
        return true;
    } 
    // match: (MOVWstorezero [off1] {sym1} p:(MOVDaddr [off2] {sym2} x) mem)
    // cond: canMergeSym(sym1,sym2) && (x.Op != OpSB || p.Uses == 1)
    // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} x mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        var p = v_0;
        if (p.Op != OpPPC64MOVDaddr) {
            break;
        }
        off2 = auxIntToInt32(p.AuxInt);
        var sym2 = auxToSym(p.Aux);
        x = p.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && (x.Op != OpSB || p.Uses == 1))) {
            break;
        }
        v.reset(OpPPC64MOVWstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MTVSRD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MTVSRD (MOVDconst [c]))
    // cond: !math.IsNaN(math.Float64frombits(uint64(c)))
    // result: (FMOVDconst [math.Float64frombits(uint64(c))])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (!(!math.IsNaN(math.Float64frombits(uint64(c))))) {
            break;
        }
        v.reset(OpPPC64FMOVDconst);
        v.AuxInt = float64ToAuxInt(math.Float64frombits(uint64(c)));
        return true;
    } 
    // match: (MTVSRD x:(MOVDload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (FMOVDload [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != OpPPC64MOVDload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, OpPPC64FMOVDload, typ.Float64);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MULLD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULLD x (MOVDconst [c]))
    // cond: is16Bit(c)
    // result: (MULLDconst [int32(c)] x)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is16Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64MULLDconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64MULLW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULLW x (MOVDconst [c]))
    // cond: is16Bit(c)
    // result: (MULLWconst [int32(c)] x)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is16Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64MULLWconst);
                v.AuxInt = int32ToAuxInt(int32(c));
                v.AddArg(x);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64NEG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEG (ADDconst [c] x))
    // cond: is32Bit(-c)
    // result: (SUBFCconst [-c] x)
    while (true) {
        if (v_0.Op != OpPPC64ADDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(is32Bit(-c))) {
            break;
        }
        v.reset(OpPPC64SUBFCconst);
        v.AuxInt = int64ToAuxInt(-c);
        v.AddArg(x);
        return true;
    } 
    // match: (NEG (SUBFCconst [c] x))
    // cond: is32Bit(-c)
    // result: (ADDconst [-c] x)
    while (true) {
        if (v_0.Op != OpPPC64SUBFCconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(is32Bit(-c))) {
            break;
        }
        v.reset(OpPPC64ADDconst);
        v.AuxInt = int64ToAuxInt(-c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64NOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NOR (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [^(c|d)])
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(~(c | d));
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64NotEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NotEqual (FlagEQ))
    // result: (MOVDconst [0])
    while (true) {
        if (v_0.Op != OpPPC64FlagEQ) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (NotEqual (FlagLT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagLT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (NotEqual (FlagGT))
    // result: (MOVDconst [1])
    while (true) {
        if (v_0.Op != OpPPC64FlagGT) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(1);
        return true;
    } 
    // match: (NotEqual (InvertFlags x))
    // result: (NotEqual x)
    while (true) {
        if (v_0.Op != OpPPC64InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64NotEqual);
        v.AddArg(x);
        return true;
    } 
    // match: (NotEqual cmp)
    // result: (ISELB [6] (MOVDconst [1]) cmp)
    while (true) {
        var cmp = v_0;
        v.reset(OpPPC64ISELB);
        v.AuxInt = int32ToAuxInt(6);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v0.AuxInt = int64ToAuxInt(1);
        v.AddArg2(v0, cmp);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPPC64OR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: ( OR (SLDconst x [c]) (SRDconst x [d]))
    // cond: d == 64-c
    // result: (ROTLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_0.AuxInt);
                var x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 64 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( OR (SLWconst x [c]) (SRWconst x [d]))
    // cond: d == 32-c
    // result: (ROTLWconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRWconst) {
                    continue;
                }
                d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLWconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( OR (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                var y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                var v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 64) {
                    continue;
                }
                var v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 63 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( OR (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 64) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 63 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( OR (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 32) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 31 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( OR (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 32) {
                    continue;
                }
                v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 31 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
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
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                }
                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(c | d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR x (MOVDconst [c]))
    // cond: isU32Bit(c)
    // result: (ORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(isU32Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64ORconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> x0:(MOVBZload [i0] {s} p mem) o1:(SLWconst x1:(MOVBZload [i1] {s} p mem) [8]))
    // cond: !config.BigEndian && i1 == i0+1 && x0.Uses ==1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, o1)
    // result: @mergePoint(b,x0,x1) (MOVHZload <t> {s} [i0] p mem)
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var i0 = auxIntToInt32(x0.AuxInt);
                var s = auxToSym(x0.Aux);
                var mem = x0.Args[1];
                var p = x0.Args[0];
                var o1 = v_1;
                if (o1.Op != OpPPC64SLWconst || auxIntToInt64(o1.AuxInt) != 8) {
                    continue;
                }
                var x1 = o1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                var i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, o1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(x1.Pos, OpPPC64MOVHZload, t);
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
    // match: (OR <t> x0:(MOVBZload [i0] {s} p mem) o1:(SLDconst x1:(MOVBZload [i1] {s} p mem) [8]))
    // cond: !config.BigEndian && i1 == i0+1 && x0.Uses ==1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, o1)
    // result: @mergePoint(b,x0,x1) (MOVHZload <t> {s} [i0] p mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o1 = v_1;
                if (o1.Op != OpPPC64SLDconst || auxIntToInt64(o1.AuxInt) != 8) {
                    continue;
                }
                x1 = o1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, o1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpPPC64MOVHZload, t);
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
    // match: (OR <t> x0:(MOVBZload [i1] {s} p mem) o1:(SLWconst x1:(MOVBZload [i0] {s} p mem) [8]))
    // cond: !config.BigEndian && i1 == i0+1 && x0.Uses ==1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, o1)
    // result: @mergePoint(b,x0,x1) (MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i1 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o1 = v_1;
                if (o1.Op != OpPPC64SLWconst || auxIntToInt64(o1.AuxInt) != 8) {
                    continue;
                }
                x1 = o1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, o1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpPPC64MOVHBRload, t);
                v.copyOf(v0);
                var v1 = b.NewValue0(x1.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                v1.AuxInt = int32ToAuxInt(i0);
                v1.Aux = symToAux(s);
                v1.AddArg(p);
                v0.AddArg2(v1, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> x0:(MOVBZload [i1] {s} p mem) o1:(SLDconst x1:(MOVBZload [i0] {s} p mem) [8]))
    // cond: !config.BigEndian && i1 == i0+1 && x0.Uses ==1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, o1)
    // result: @mergePoint(b,x0,x1) (MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i1 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o1 = v_1;
                if (o1.Op != OpPPC64SLDconst || auxIntToInt64(o1.AuxInt) != 8) {
                    continue;
                }
                x1 = o1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && o1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, o1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpPPC64MOVHBRload, t);
                v.copyOf(v0);
                v1 = b.NewValue0(x1.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                v1.AuxInt = int32ToAuxInt(i0);
                v1.Aux = symToAux(s);
                v1.AddArg(p);
                v0.AddArg2(v1, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s0:(SLWconst x0:(MOVBZload [i1] {s} p mem) [n1]) s1:(SLWconst x1:(MOVBZload [i0] {s} p mem) [n2]))
    // cond: !config.BigEndian && i1 == i0+1 && n1%8 == 0 && n2 == n1+8 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, s0, s1)
    // result: @mergePoint(b,x0,x1) (SLDconst <t> (MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [n1])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s0 = v_0;
                if (s0.Op != OpPPC64SLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var n1 = auxIntToInt64(s0.AuxInt);
                x0 = s0.Args[0];
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                var s1 = v_1;
                if (s1.Op != OpPPC64SLWconst) {
                    continue;
                }
                var n2 = auxIntToInt64(s1.AuxInt);
                x1 = s1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && n1 % 8 == 0 && n2 == n1 + 8 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, s0, s1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpPPC64SLDconst, t);
                v.copyOf(v0);
                v0.AuxInt = int64ToAuxInt(n1);
                v1 = b.NewValue0(x1.Pos, OpPPC64MOVHBRload, t);
                var v2 = b.NewValue0(x1.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                v2.AuxInt = int32ToAuxInt(i0);
                v2.Aux = symToAux(s);
                v2.AddArg(p);
                v1.AddArg2(v2, mem);
                v0.AddArg(v1);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s0:(SLDconst x0:(MOVBZload [i1] {s} p mem) [n1]) s1:(SLDconst x1:(MOVBZload [i0] {s} p mem) [n2]))
    // cond: !config.BigEndian && i1 == i0+1 && n1%8 == 0 && n2 == n1+8 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1) != nil && clobber(x0, x1, s0, s1)
    // result: @mergePoint(b,x0,x1) (SLDconst <t> (MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [n1])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s0 = v_0;
                if (s0.Op != OpPPC64SLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                n1 = auxIntToInt64(s0.AuxInt);
                x0 = s0.Args[0];
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i1 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                s1 = v_1;
                if (s1.Op != OpPPC64SLDconst) {
                    continue;
                }
                n2 = auxIntToInt64(s1.AuxInt);
                x1 = s1.Args[0];
                if (x1.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && n1 % 8 == 0 && n2 == n1 + 8 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, s0, s1))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, OpPPC64SLDconst, t);
                v.copyOf(v0);
                v0.AuxInt = int64ToAuxInt(n1);
                v1 = b.NewValue0(x1.Pos, OpPPC64MOVHBRload, t);
                v2 = b.NewValue0(x1.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                v2.AuxInt = int32ToAuxInt(i0);
                v2.Aux = symToAux(s);
                v2.AddArg(p);
                v1.AddArg2(v2, mem);
                v0.AddArg(v1);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s1:(SLWconst x2:(MOVBZload [i3] {s} p mem) [24]) o0:(OR <t> s0:(SLWconst x1:(MOVBZload [i2] {s} p mem) [16]) x0:(MOVHZload [i0] {s} p mem)))
    // cond: !config.BigEndian && i2 == i0+2 && i3 == i0+3 && x0.Uses ==1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWZload <t> {s} [i0] p mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpPPC64SLWconst || auxIntToInt64(s1.AuxInt) != 24) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var x2 = s1.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                var i3 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                var o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                var o0_0 = o0.Args[0];
                var o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLWconst || auxIntToInt64(s0.AuxInt) != 16) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        var i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        x0 = o0_1;
                        if (x0.Op != OpPPC64MOVHZload) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        if (auxToSym(x0.Aux) != s) {
                            continue;
                        }
                        _ = x0.Args[1];
                        if (p != x0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64MOVWZload, t);
                        v.copyOf(v0);
                        v0.AuxInt = int32ToAuxInt(i0);
                        v0.Aux = symToAux(s);
                        v0.AddArg2(p, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s1:(SLDconst x2:(MOVBZload [i3] {s} p mem) [24]) o0:(OR <t> s0:(SLDconst x1:(MOVBZload [i2] {s} p mem) [16]) x0:(MOVHZload [i0] {s} p mem)))
    // cond: !config.BigEndian && i2 == i0+2 && i3 == i0+3 && x0.Uses ==1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWZload <t> {s} [i0] p mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 24) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x2 = s1.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i3 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 16) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        x0 = o0_1;
                        if (x0.Op != OpPPC64MOVHZload) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        if (auxToSym(x0.Aux) != s) {
                            continue;
                        }
                        _ = x0.Args[1];
                        if (p != x0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64MOVWZload, t);
                        v.copyOf(v0);
                        v0.AuxInt = int32ToAuxInt(i0);
                        v0.Aux = symToAux(s);
                        v0.AddArg2(p, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s1:(SLWconst x2:(MOVBZload [i0] {s} p mem) [24]) o0:(OR <t> s0:(SLWconst x1:(MOVBZload [i1] {s} p mem) [16]) x0:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i2] {s} p) mem)))
    // cond: !config.BigEndian && i1 == i0+1 && i2 == i0+2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpPPC64SLWconst || auxIntToInt64(s1.AuxInt) != 24) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x2 = s1.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLWconst || auxIntToInt64(s0.AuxInt) != 16) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
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
                        x0 = o0_1;
                        if (x0.Op != OpPPC64MOVHBRload || x0.Type != t) {
                            continue;
                        }
                        _ = x0.Args[1];
                        var x0_0 = x0.Args[0];
                        if (x0_0.Op != OpPPC64MOVDaddr || x0_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i2 = auxIntToInt32(x0_0.AuxInt);
                        if (auxToSym(x0_0.Aux) != s || p != x0_0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && i2 == i0 + 2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64MOVWBRload, t);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x0.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v1.AuxInt = int32ToAuxInt(i0);
                        v1.Aux = symToAux(s);
                        v1.AddArg(p);
                        v0.AddArg2(v1, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s1:(SLDconst x2:(MOVBZload [i0] {s} p mem) [24]) o0:(OR <t> s0:(SLDconst x1:(MOVBZload [i1] {s} p mem) [16]) x0:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i2] {s} p) mem)))
    // cond: !config.BigEndian && i1 == i0+1 && i2 == i0+2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s1 = v_0;
                if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 24) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x2 = s1.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 16) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
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
                        x0 = o0_1;
                        if (x0.Op != OpPPC64MOVHBRload || x0.Type != t) {
                            continue;
                        }
                        _ = x0.Args[1];
                        x0_0 = x0.Args[0];
                        if (x0_0.Op != OpPPC64MOVDaddr || x0_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i2 = auxIntToInt32(x0_0.AuxInt);
                        if (auxToSym(x0_0.Aux) != s || p != x0_0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && i2 == i0 + 2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64MOVWBRload, t);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x0.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v1.AuxInt = int32ToAuxInt(i0);
                        v1.Aux = symToAux(s);
                        v1.AddArg(p);
                        v0.AddArg2(v1, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> x0:(MOVBZload [i3] {s} p mem) o0:(OR <t> s0:(SLWconst x1:(MOVBZload [i2] {s} p mem) [8]) s1:(SLWconst x2:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [16])))
    // cond: !config.BigEndian && i2 == i0+2 && i3 == i0+3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i3 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLWconst || auxIntToInt64(s0.AuxInt) != 8) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        s1 = o0_1;
                        if (s1.Op != OpPPC64SLWconst || auxIntToInt64(s1.AuxInt) != 16) {
                            continue;
                        }
                        x2 = s1.Args[0];
                        if (x2.Op != OpPPC64MOVHBRload || x2.Type != t) {
                            continue;
                        }
                        _ = x2.Args[1];
                        var x2_0 = x2.Args[0];
                        if (x2_0.Op != OpPPC64MOVDaddr || x2_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i0 = auxIntToInt32(x2_0.AuxInt);
                        if (auxToSym(x2_0.Aux) != s || p != x2_0.Args[0] || mem != x2.Args[1] || !(!config.BigEndian && i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x2.Pos, OpPPC64MOVWBRload, t);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x2.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v1.AuxInt = int32ToAuxInt(i0);
                        v1.Aux = symToAux(s);
                        v1.AddArg(p);
                        v0.AddArg2(v1, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> x0:(MOVBZload [i3] {s} p mem) o0:(OR <t> s0:(SLDconst x1:(MOVBZload [i2] {s} p mem) [8]) s1:(SLDconst x2:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [16])))
    // cond: !config.BigEndian && i2 == i0+2 && i3 == i0+3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i3 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s0 = o0_0;
                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 8) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        s1 = o0_1;
                        if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 16) {
                            continue;
                        }
                        x2 = s1.Args[0];
                        if (x2.Op != OpPPC64MOVHBRload || x2.Type != t) {
                            continue;
                        }
                        _ = x2.Args[1];
                        x2_0 = x2.Args[0];
                        if (x2_0.Op != OpPPC64MOVDaddr || x2_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i0 = auxIntToInt32(x2_0.AuxInt);
                        if (auxToSym(x2_0.Aux) != s || p != x2_0.Args[0] || mem != x2.Args[1] || !(!config.BigEndian && i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x2.Pos, OpPPC64MOVWBRload, t);
                        v.copyOf(v0);
                        v1 = b.NewValue0(x2.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v1.AuxInt = int32ToAuxInt(i0);
                        v1.Aux = symToAux(s);
                        v1.AddArg(p);
                        v0.AddArg2(v1, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s2:(SLDconst x2:(MOVBZload [i3] {s} p mem) [32]) o0:(OR <t> s1:(SLDconst x1:(MOVBZload [i2] {s} p mem) [40]) s0:(SLDconst x0:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [48])))
    // cond: !config.BigEndian && i2 == i0+2 && i3 == i0+3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, s2, o0)
    // result: @mergePoint(b,x0,x1,x2) (SLDconst <t> (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [32])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s2 = v_0;
                if (s2.Op != OpPPC64SLDconst || auxIntToInt64(s2.AuxInt) != 32) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x2 = s2.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i3 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s1 = o0_0;
                        if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 40) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s1.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        s0 = o0_1;
                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 48) {
                            continue;
                        }
                        x0 = s0.Args[0];
                        if (x0.Op != OpPPC64MOVHBRload || x0.Type != t) {
                            continue;
                        }
                        _ = x0.Args[1];
                        x0_0 = x0.Args[0];
                        if (x0_0.Op != OpPPC64MOVDaddr || x0_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i0 = auxIntToInt32(x0_0.AuxInt);
                        if (auxToSym(x0_0.Aux) != s || p != x0_0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, s2, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64SLDconst, t);
                        v.copyOf(v0);
                        v0.AuxInt = int64ToAuxInt(32);
                        v1 = b.NewValue0(x0.Pos, OpPPC64MOVWBRload, t);
                        v2 = b.NewValue0(x0.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v2.AuxInt = int32ToAuxInt(i0);
                        v2.Aux = symToAux(s);
                        v2.AddArg(p);
                        v1.AddArg2(v2, mem);
                        v0.AddArg(v1);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s2:(SLDconst x2:(MOVBZload [i0] {s} p mem) [56]) o0:(OR <t> s1:(SLDconst x1:(MOVBZload [i1] {s} p mem) [48]) s0:(SLDconst x0:(MOVHBRload <t> (MOVDaddr <typ.Uintptr> [i2] {s} p) mem) [32])))
    // cond: !config.BigEndian && i1 == i0+1 && i2 == i0+2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && mergePoint(b, x0, x1, x2) != nil && clobber(x0, x1, x2, s0, s1, s2, o0)
    // result: @mergePoint(b,x0,x1,x2) (SLDconst <t> (MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [32])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s2 = v_0;
                if (s2.Op != OpPPC64SLDconst || auxIntToInt64(s2.AuxInt) != 56) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x2 = s2.Args[0];
                if (x2.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x2.AuxInt);
                s = auxToSym(x2.Aux);
                mem = x2.Args[1];
                p = x2.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s1 = o0_0;
                        if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 48) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s1.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
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
                        s0 = o0_1;
                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 32) {
                            continue;
                        }
                        x0 = s0.Args[0];
                        if (x0.Op != OpPPC64MOVHBRload || x0.Type != t) {
                            continue;
                        }
                        _ = x0.Args[1];
                        x0_0 = x0.Args[0];
                        if (x0_0.Op != OpPPC64MOVDaddr || x0_0.Type != typ.Uintptr) {
                            continue;
                        }
                        i2 = auxIntToInt32(x0_0.AuxInt);
                        if (auxToSym(x0_0.Aux) != s || p != x0_0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && i2 == i0 + 2 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && o0.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, s2, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x0.Pos, OpPPC64SLDconst, t);
                        v.copyOf(v0);
                        v0.AuxInt = int64ToAuxInt(32);
                        v1 = b.NewValue0(x0.Pos, OpPPC64MOVWBRload, t);
                        v2 = b.NewValue0(x0.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                        v2.AuxInt = int32ToAuxInt(i0);
                        v2.Aux = symToAux(s);
                        v2.AddArg(p);
                        v1.AddArg2(v2, mem);
                        v0.AddArg(v1);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (OR <t> s6:(SLDconst x7:(MOVBZload [i7] {s} p mem) [56]) o5:(OR <t> s5:(SLDconst x6:(MOVBZload [i6] {s} p mem) [48]) o4:(OR <t> s4:(SLDconst x5:(MOVBZload [i5] {s} p mem) [40]) o3:(OR <t> s3:(SLDconst x4:(MOVBZload [i4] {s} p mem) [32]) x0:(MOVWZload {s} [i0] p mem)))))
    // cond: !config.BigEndian && i4 == i0+4 && i5 == i0+5 && i6 == i0+6 && i7 == i0+7 && x0.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses ==1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s3.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x0, x4, x5, x6, x7) != nil && clobber(x0, x4, x5, x6, x7, s3, s4, s5, s6, o3, o4, o5)
    // result: @mergePoint(b,x0,x4,x5,x6,x7) (MOVDload <t> {s} [i0] p mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var s6 = v_0;
                if (s6.Op != OpPPC64SLDconst || auxIntToInt64(s6.AuxInt) != 56) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var x7 = s6.Args[0];
                if (x7.Op != OpPPC64MOVBZload) {
                    continue;
                }
                var i7 = auxIntToInt32(x7.AuxInt);
                s = auxToSym(x7.Aux);
                mem = x7.Args[1];
                p = x7.Args[0];
                var o5 = v_1;
                if (o5.Op != OpPPC64OR || o5.Type != t) {
                    continue;
                }
                _ = o5.Args[1];
                var o5_0 = o5.Args[0];
                var o5_1 = o5.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        var s5 = o5_0;
                        if (s5.Op != OpPPC64SLDconst || auxIntToInt64(s5.AuxInt) != 48) {
                            continue;
                        (_i1, o5_0, o5_1) = (_i1 + 1, o5_1, o5_0);
                        }
                        var x6 = s5.Args[0];
                        if (x6.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        var i6 = auxIntToInt32(x6.AuxInt);
                        if (auxToSym(x6.Aux) != s) {
                            continue;
                        }
                        _ = x6.Args[1];
                        if (p != x6.Args[0] || mem != x6.Args[1]) {
                            continue;
                        }
                        var o4 = o5_1;
                        if (o4.Op != OpPPC64OR || o4.Type != t) {
                            continue;
                        }
                        _ = o4.Args[1];
                        var o4_0 = o4.Args[0];
                        var o4_1 = o4.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            nint _i2 = 0;

                            while (_i2 <= 1) {
                                var s4 = o4_0;
                                if (s4.Op != OpPPC64SLDconst || auxIntToInt64(s4.AuxInt) != 40) {
                                    continue;
                                (_i2, o4_0, o4_1) = (_i2 + 1, o4_1, o4_0);
                                }
                                var x5 = s4.Args[0];
                                if (x5.Op != OpPPC64MOVBZload) {
                                    continue;
                                }
                                var i5 = auxIntToInt32(x5.AuxInt);
                                if (auxToSym(x5.Aux) != s) {
                                    continue;
                                }
                                _ = x5.Args[1];
                                if (p != x5.Args[0] || mem != x5.Args[1]) {
                                    continue;
                                }
                                var o3 = o4_1;
                                if (o3.Op != OpPPC64OR || o3.Type != t) {
                                    continue;
                                }
                                _ = o3.Args[1];
                                var o3_0 = o3.Args[0];
                                var o3_1 = o3.Args[1];
                                {
                                    nint _i3__prev5 = _i3;

                                    nint _i3 = 0;

                                    while (_i3 <= 1) {
                                        var s3 = o3_0;
                                        if (s3.Op != OpPPC64SLDconst || auxIntToInt64(s3.AuxInt) != 32) {
                                            continue;
                                        (_i3, o3_0, o3_1) = (_i3 + 1, o3_1, o3_0);
                                        }
                                        var x4 = s3.Args[0];
                                        if (x4.Op != OpPPC64MOVBZload) {
                                            continue;
                                        }
                                        var i4 = auxIntToInt32(x4.AuxInt);
                                        if (auxToSym(x4.Aux) != s) {
                                            continue;
                                        }
                                        _ = x4.Args[1];
                                        if (p != x4.Args[0] || mem != x4.Args[1]) {
                                            continue;
                                        }
                                        x0 = o3_1;
                                        if (x0.Op != OpPPC64MOVWZload) {
                                            continue;
                                        }
                                        i0 = auxIntToInt32(x0.AuxInt);
                                        if (auxToSym(x0.Aux) != s) {
                                            continue;
                                        }
                                        _ = x0.Args[1];
                                        if (p != x0.Args[0] || mem != x0.Args[1] || !(!config.BigEndian && i4 == i0 + 4 && i5 == i0 + 5 && i6 == i0 + 6 && i7 == i0 + 7 && x0.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s3.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x0, x4, x5, x6, x7) != null && clobber(x0, x4, x5, x6, x7, s3, s4, s5, s6, o3, o4, o5))) {
                                            continue;
                                        }
                                        b = mergePoint(b, x0, x4, x5, x6, x7);
                                        v0 = b.NewValue0(x0.Pos, OpPPC64MOVDload, t);
                                        v.copyOf(v0);
                                        v0.AuxInt = int32ToAuxInt(i0);
                                        v0.Aux = symToAux(s);
                                        v0.AddArg2(p, mem);
                                        return true;
                                    }


                                    _i3 = _i3__prev5;
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
    // match: (OR <t> s0:(SLDconst x0:(MOVBZload [i0] {s} p mem) [56]) o0:(OR <t> s1:(SLDconst x1:(MOVBZload [i1] {s} p mem) [48]) o1:(OR <t> s2:(SLDconst x2:(MOVBZload [i2] {s} p mem) [40]) o2:(OR <t> s3:(SLDconst x3:(MOVBZload [i3] {s} p mem) [32]) x4:(MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i4] p) mem)))))
    // cond: !config.BigEndian && i1 == i0+1 && i2 == i0+2 && i3 == i0+3 && i4 == i0+4 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && x4.Uses == 1 && o0.Uses == 1 && o1.Uses == 1 && o2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && mergePoint(b, x0, x1, x2, x3, x4) != nil && clobber(x0, x1, x2, x3, x4, o0, o1, o2, s0, s1, s2, s3)
    // result: @mergePoint(b,x0,x1,x2,x3,x4) (MOVDBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                s0 = v_0;
                if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 56) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                x0 = s0.Args[0];
                if (x0.Op != OpPPC64MOVBZload) {
                    continue;
                }
                i0 = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                p = x0.Args[0];
                o0 = v_1;
                if (o0.Op != OpPPC64OR || o0.Type != t) {
                    continue;
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s1 = o0_0;
                        if (s1.Op != OpPPC64SLDconst || auxIntToInt64(s1.AuxInt) != 48) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        x1 = s1.Args[0];
                        if (x1.Op != OpPPC64MOVBZload) {
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
                        o1 = o0_1;
                        if (o1.Op != OpPPC64OR || o1.Type != t) {
                            continue;
                        }
                        _ = o1.Args[1];
                        var o1_0 = o1.Args[0];
                        var o1_1 = o1.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                s2 = o1_0;
                                if (s2.Op != OpPPC64SLDconst || auxIntToInt64(s2.AuxInt) != 40) {
                                    continue;
                                (_i2, o1_0, o1_1) = (_i2 + 1, o1_1, o1_0);
                                }
                                x2 = s2.Args[0];
                                if (x2.Op != OpPPC64MOVBZload) {
                                    continue;
                                }
                                i2 = auxIntToInt32(x2.AuxInt);
                                if (auxToSym(x2.Aux) != s) {
                                    continue;
                                }
                                _ = x2.Args[1];
                                if (p != x2.Args[0] || mem != x2.Args[1]) {
                                    continue;
                                }
                                var o2 = o1_1;
                                if (o2.Op != OpPPC64OR || o2.Type != t) {
                                    continue;
                                }
                                _ = o2.Args[1];
                                var o2_0 = o2.Args[0];
                                var o2_1 = o2.Args[1];
                                {
                                    nint _i3__prev5 = _i3;

                                    _i3 = 0;

                                    while (_i3 <= 1) {
                                        s3 = o2_0;
                                        if (s3.Op != OpPPC64SLDconst || auxIntToInt64(s3.AuxInt) != 32) {
                                            continue;
                                        (_i3, o2_0, o2_1) = (_i3 + 1, o2_1, o2_0);
                                        }
                                        var x3 = s3.Args[0];
                                        if (x3.Op != OpPPC64MOVBZload) {
                                            continue;
                                        }
                                        i3 = auxIntToInt32(x3.AuxInt);
                                        if (auxToSym(x3.Aux) != s) {
                                            continue;
                                        }
                                        _ = x3.Args[1];
                                        if (p != x3.Args[0] || mem != x3.Args[1]) {
                                            continue;
                                        }
                                        x4 = o2_1;
                                        if (x4.Op != OpPPC64MOVWBRload || x4.Type != t) {
                                            continue;
                                        }
                                        _ = x4.Args[1];
                                        var x4_0 = x4.Args[0];
                                        if (x4_0.Op != OpPPC64MOVDaddr || x4_0.Type != typ.Uintptr) {
                                            continue;
                                        }
                                        i4 = auxIntToInt32(x4_0.AuxInt);
                                        if (p != x4_0.Args[0] || mem != x4.Args[1] || !(!config.BigEndian && i1 == i0 + 1 && i2 == i0 + 2 && i3 == i0 + 3 && i4 == i0 + 4 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && x3.Uses == 1 && x4.Uses == 1 && o0.Uses == 1 && o1.Uses == 1 && o2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && s2.Uses == 1 && s3.Uses == 1 && mergePoint(b, x0, x1, x2, x3, x4) != null && clobber(x0, x1, x2, x3, x4, o0, o1, o2, s0, s1, s2, s3))) {
                                            continue;
                                        }
                                        b = mergePoint(b, x0, x1, x2, x3, x4);
                                        v0 = b.NewValue0(x4.Pos, OpPPC64MOVDBRload, t);
                                        v.copyOf(v0);
                                        v1 = b.NewValue0(x4.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                                        v1.AuxInt = int32ToAuxInt(i0);
                                        v1.Aux = symToAux(s);
                                        v1.AddArg(p);
                                        v0.AddArg2(v1, mem);
                                        return true;
                                    }


                                    _i3 = _i3__prev5;
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
    // match: (OR <t> x7:(MOVBZload [i7] {s} p mem) o5:(OR <t> s6:(SLDconst x6:(MOVBZload [i6] {s} p mem) [8]) o4:(OR <t> s5:(SLDconst x5:(MOVBZload [i5] {s} p mem) [16]) o3:(OR <t> s4:(SLDconst x4:(MOVBZload [i4] {s} p mem) [24]) s0:(SLWconst x3:(MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [32])))))
    // cond: !config.BigEndian && i4 == i0+4 && i5 == i0+5 && i6 == i0+6 && i7 == i0+7 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s0.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x3, x4, x5, x6, x7) != nil && clobber(x3, x4, x5, x6, x7, o3, o4, o5, s0, s4, s5, s6)
    // result: @mergePoint(b,x3,x4,x5,x6,x7) (MOVDBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x7 = v_0;
                if (x7.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i7 = auxIntToInt32(x7.AuxInt);
                s = auxToSym(x7.Aux);
                mem = x7.Args[1];
                p = x7.Args[0];
                o5 = v_1;
                if (o5.Op != OpPPC64OR || o5.Type != t) {
                    continue;
                }
                _ = o5.Args[1];
                o5_0 = o5.Args[0];
                o5_1 = o5.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s6 = o5_0;
                        if (s6.Op != OpPPC64SLDconst || auxIntToInt64(s6.AuxInt) != 8) {
                            continue;
                        (_i1, o5_0, o5_1) = (_i1 + 1, o5_1, o5_0);
                        }
                        x6 = s6.Args[0];
                        if (x6.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i6 = auxIntToInt32(x6.AuxInt);
                        if (auxToSym(x6.Aux) != s) {
                            continue;
                        }
                        _ = x6.Args[1];
                        if (p != x6.Args[0] || mem != x6.Args[1]) {
                            continue;
                        }
                        o4 = o5_1;
                        if (o4.Op != OpPPC64OR || o4.Type != t) {
                            continue;
                        }
                        _ = o4.Args[1];
                        o4_0 = o4.Args[0];
                        o4_1 = o4.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                s5 = o4_0;
                                if (s5.Op != OpPPC64SLDconst || auxIntToInt64(s5.AuxInt) != 16) {
                                    continue;
                                (_i2, o4_0, o4_1) = (_i2 + 1, o4_1, o4_0);
                                }
                                x5 = s5.Args[0];
                                if (x5.Op != OpPPC64MOVBZload) {
                                    continue;
                                }
                                i5 = auxIntToInt32(x5.AuxInt);
                                if (auxToSym(x5.Aux) != s) {
                                    continue;
                                }
                                _ = x5.Args[1];
                                if (p != x5.Args[0] || mem != x5.Args[1]) {
                                    continue;
                                }
                                o3 = o4_1;
                                if (o3.Op != OpPPC64OR || o3.Type != t) {
                                    continue;
                                }
                                _ = o3.Args[1];
                                o3_0 = o3.Args[0];
                                o3_1 = o3.Args[1];
                                {
                                    nint _i3__prev5 = _i3;

                                    _i3 = 0;

                                    while (_i3 <= 1) {
                                        s4 = o3_0;
                                        if (s4.Op != OpPPC64SLDconst || auxIntToInt64(s4.AuxInt) != 24) {
                                            continue;
                                        (_i3, o3_0, o3_1) = (_i3 + 1, o3_1, o3_0);
                                        }
                                        x4 = s4.Args[0];
                                        if (x4.Op != OpPPC64MOVBZload) {
                                            continue;
                                        }
                                        i4 = auxIntToInt32(x4.AuxInt);
                                        if (auxToSym(x4.Aux) != s) {
                                            continue;
                                        }
                                        _ = x4.Args[1];
                                        if (p != x4.Args[0] || mem != x4.Args[1]) {
                                            continue;
                                        }
                                        s0 = o3_1;
                                        if (s0.Op != OpPPC64SLWconst || auxIntToInt64(s0.AuxInt) != 32) {
                                            continue;
                                        }
                                        x3 = s0.Args[0];
                                        if (x3.Op != OpPPC64MOVWBRload || x3.Type != t) {
                                            continue;
                                        }
                                        _ = x3.Args[1];
                                        var x3_0 = x3.Args[0];
                                        if (x3_0.Op != OpPPC64MOVDaddr || x3_0.Type != typ.Uintptr) {
                                            continue;
                                        }
                                        i0 = auxIntToInt32(x3_0.AuxInt);
                                        if (auxToSym(x3_0.Aux) != s || p != x3_0.Args[0] || mem != x3.Args[1] || !(!config.BigEndian && i4 == i0 + 4 && i5 == i0 + 5 && i6 == i0 + 6 && i7 == i0 + 7 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s0.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x3, x4, x5, x6, x7) != null && clobber(x3, x4, x5, x6, x7, o3, o4, o5, s0, s4, s5, s6))) {
                                            continue;
                                        }
                                        b = mergePoint(b, x3, x4, x5, x6, x7);
                                        v0 = b.NewValue0(x3.Pos, OpPPC64MOVDBRload, t);
                                        v.copyOf(v0);
                                        v1 = b.NewValue0(x3.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                                        v1.AuxInt = int32ToAuxInt(i0);
                                        v1.Aux = symToAux(s);
                                        v1.AddArg(p);
                                        v0.AddArg2(v1, mem);
                                        return true;
                                    }


                                    _i3 = _i3__prev5;
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
    // match: (OR <t> x7:(MOVBZload [i7] {s} p mem) o5:(OR <t> s6:(SLDconst x6:(MOVBZload [i6] {s} p mem) [8]) o4:(OR <t> s5:(SLDconst x5:(MOVBZload [i5] {s} p mem) [16]) o3:(OR <t> s4:(SLDconst x4:(MOVBZload [i4] {s} p mem) [24]) s0:(SLDconst x3:(MOVWBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem) [32])))))
    // cond: !config.BigEndian && i4 == i0+4 && i5 == i0+5 && i6 == i0+6 && i7 == i0+7 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s0.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x3, x4, x5, x6, x7) != nil && clobber(x3, x4, x5, x6, x7, o3, o4, o5, s0, s4, s5, s6)
    // result: @mergePoint(b,x3,x4,x5,x6,x7) (MOVDBRload <t> (MOVDaddr <typ.Uintptr> [i0] {s} p) mem)
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x7 = v_0;
                if (x7.Op != OpPPC64MOVBZload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                i7 = auxIntToInt32(x7.AuxInt);
                s = auxToSym(x7.Aux);
                mem = x7.Args[1];
                p = x7.Args[0];
                o5 = v_1;
                if (o5.Op != OpPPC64OR || o5.Type != t) {
                    continue;
                }
                _ = o5.Args[1];
                o5_0 = o5.Args[0];
                o5_1 = o5.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        s6 = o5_0;
                        if (s6.Op != OpPPC64SLDconst || auxIntToInt64(s6.AuxInt) != 8) {
                            continue;
                        (_i1, o5_0, o5_1) = (_i1 + 1, o5_1, o5_0);
                        }
                        x6 = s6.Args[0];
                        if (x6.Op != OpPPC64MOVBZload) {
                            continue;
                        }
                        i6 = auxIntToInt32(x6.AuxInt);
                        if (auxToSym(x6.Aux) != s) {
                            continue;
                        }
                        _ = x6.Args[1];
                        if (p != x6.Args[0] || mem != x6.Args[1]) {
                            continue;
                        }
                        o4 = o5_1;
                        if (o4.Op != OpPPC64OR || o4.Type != t) {
                            continue;
                        }
                        _ = o4.Args[1];
                        o4_0 = o4.Args[0];
                        o4_1 = o4.Args[1];
                        {
                            nint _i2__prev4 = _i2;

                            _i2 = 0;

                            while (_i2 <= 1) {
                                s5 = o4_0;
                                if (s5.Op != OpPPC64SLDconst || auxIntToInt64(s5.AuxInt) != 16) {
                                    continue;
                                (_i2, o4_0, o4_1) = (_i2 + 1, o4_1, o4_0);
                                }
                                x5 = s5.Args[0];
                                if (x5.Op != OpPPC64MOVBZload) {
                                    continue;
                                }
                                i5 = auxIntToInt32(x5.AuxInt);
                                if (auxToSym(x5.Aux) != s) {
                                    continue;
                                }
                                _ = x5.Args[1];
                                if (p != x5.Args[0] || mem != x5.Args[1]) {
                                    continue;
                                }
                                o3 = o4_1;
                                if (o3.Op != OpPPC64OR || o3.Type != t) {
                                    continue;
                                }
                                _ = o3.Args[1];
                                o3_0 = o3.Args[0];
                                o3_1 = o3.Args[1];
                                {
                                    nint _i3__prev5 = _i3;

                                    _i3 = 0;

                                    while (_i3 <= 1) {
                                        s4 = o3_0;
                                        if (s4.Op != OpPPC64SLDconst || auxIntToInt64(s4.AuxInt) != 24) {
                                            continue;
                                        (_i3, o3_0, o3_1) = (_i3 + 1, o3_1, o3_0);
                                        }
                                        x4 = s4.Args[0];
                                        if (x4.Op != OpPPC64MOVBZload) {
                                            continue;
                                        }
                                        i4 = auxIntToInt32(x4.AuxInt);
                                        if (auxToSym(x4.Aux) != s) {
                                            continue;
                                        }
                                        _ = x4.Args[1];
                                        if (p != x4.Args[0] || mem != x4.Args[1]) {
                                            continue;
                                        }
                                        s0 = o3_1;
                                        if (s0.Op != OpPPC64SLDconst || auxIntToInt64(s0.AuxInt) != 32) {
                                            continue;
                                        }
                                        x3 = s0.Args[0];
                                        if (x3.Op != OpPPC64MOVWBRload || x3.Type != t) {
                                            continue;
                                        }
                                        _ = x3.Args[1];
                                        x3_0 = x3.Args[0];
                                        if (x3_0.Op != OpPPC64MOVDaddr || x3_0.Type != typ.Uintptr) {
                                            continue;
                                        }
                                        i0 = auxIntToInt32(x3_0.AuxInt);
                                        if (auxToSym(x3_0.Aux) != s || p != x3_0.Args[0] || mem != x3.Args[1] || !(!config.BigEndian && i4 == i0 + 4 && i5 == i0 + 5 && i6 == i0 + 6 && i7 == i0 + 7 && x3.Uses == 1 && x4.Uses == 1 && x5.Uses == 1 && x6.Uses == 1 && x7.Uses == 1 && o3.Uses == 1 && o4.Uses == 1 && o5.Uses == 1 && s0.Uses == 1 && s4.Uses == 1 && s5.Uses == 1 && s6.Uses == 1 && mergePoint(b, x3, x4, x5, x6, x7) != null && clobber(x3, x4, x5, x6, x7, o3, o4, o5, s0, s4, s5, s6))) {
                                            continue;
                                        }
                                        b = mergePoint(b, x3, x4, x5, x6, x7);
                                        v0 = b.NewValue0(x3.Pos, OpPPC64MOVDBRload, t);
                                        v.copyOf(v0);
                                        v1 = b.NewValue0(x3.Pos, OpPPC64MOVDaddr, typ.Uintptr);
                                        v1.AuxInt = int32ToAuxInt(i0);
                                        v1.Aux = symToAux(s);
                                        v1.AddArg(p);
                                        v0.AddArg2(v1, mem);
                                        return true;
                                    }


                                    _i3 = _i3__prev5;
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
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ORN(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ORN x (MOVDconst [-1]))
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1.AuxInt) != -1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ORN (MOVDconst [c]) (MOVDconst [d]))
    // result: (MOVDconst [c|^d])
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_0.AuxInt);
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var d = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(c | ~d);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORconst [c] (ORconst [d] x))
    // result: (ORconst [c|d] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ORconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpPPC64ORconst);
        v.AuxInt = int64ToAuxInt(c | d);
        v.AddArg(x);
        return true;
    } 
    // match: (ORconst [-1] _)
    // result: (MOVDconst [-1])
    while (true) {
        if (auxIntToInt64(v.AuxInt) != -1) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(-1);
        return true;
    } 
    // match: (ORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ROTL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ROTL x (MOVDconst [c]))
    // result: (ROTLconst x [c&63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64ROTLconst);
        v.AuxInt = int64ToAuxInt(c & 63);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ROTLW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ROTLW x (MOVDconst [c]))
    // result: (ROTLWconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64ROTLWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64ROTLWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ROTLWconst [r] (AND (MOVDconst [m]) x))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWINM [encodePPC64RotateMask(r,rotateLeft32(m,r),32)] x)
    while (true) {
        var r = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                var m = auxIntToInt64(v_0_0.AuxInt);
                var x = v_0_1;
                if (!(isPPC64WordRotateMask(m))) {
                    continue;
                }
                v.reset(OpPPC64RLWINM);
                v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(r, rotateLeft32(m, r), 32));
                v.AddArg(x);
                return true;
            }

        }
        break;
    } 
    // match: (ROTLWconst [r] (ANDconst [m] x))
    // cond: isPPC64WordRotateMask(m)
    // result: (RLWINM [encodePPC64RotateMask(r,rotateLeft32(m,r),32)] x)
    while (true) {
        r = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ANDconst) {
            break;
        }
        m = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(isPPC64WordRotateMask(m))) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(encodePPC64RotateMask(r, rotateLeft32(m, r), 32));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SLD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SLD x (MOVDconst [c]))
    // result: (SLDconst [c&63 | (c>>6&1*63)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SLDconst);
        v.AuxInt = int64ToAuxInt(c & 63 | (c >> 6 & 1 * 63));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SLDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLDconst [l] (SRWconst [r] x))
    // cond: mergePPC64SldiSrw(l,r) != 0
    // result: (RLWINM [mergePPC64SldiSrw(l,r)] x)
    while (true) {
        var l = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64SRWconst) {
            break;
        }
        var r = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(mergePPC64SldiSrw(l, r) != 0)) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(mergePPC64SldiSrw(l, r));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst [c] z:(MOVBZreg x))
    // cond: c < 8 && z.Uses == 1
    // result: (CLRLSLDI [newPPC64ShiftAuxInt(c,56,63,64)] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        var z = v_0;
        if (z.Op != OpPPC64MOVBZreg) {
            break;
        }
        x = z.Args[0];
        if (!(c < 8 && z.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64CLRLSLDI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 56, 63, 64));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst [c] z:(MOVHZreg x))
    // cond: c < 16 && z.Uses == 1
    // result: (CLRLSLDI [newPPC64ShiftAuxInt(c,48,63,64)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = z.Args[0];
        if (!(c < 16 && z.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64CLRLSLDI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 48, 63, 64));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst [c] z:(MOVWZreg x))
    // cond: c < 32 && z.Uses == 1
    // result: (CLRLSLDI [newPPC64ShiftAuxInt(c,32,63,64)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64MOVWZreg) {
            break;
        }
        x = z.Args[0];
        if (!(c < 32 && z.Uses == 1)) {
            break;
        }
        v.reset(OpPPC64CLRLSLDI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 32, 63, 64));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst [c] z:(ANDconst [d] x))
    // cond: z.Uses == 1 && isPPC64ValidShiftMask(d) && c <= (64-getPPC64ShiftMaskLength(d))
    // result: (CLRLSLDI [newPPC64ShiftAuxInt(c,64-getPPC64ShiftMaskLength(d),63,64)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        var d = auxIntToInt64(z.AuxInt);
        x = z.Args[0];
        if (!(z.Uses == 1 && isPPC64ValidShiftMask(d) && c <= (64 - getPPC64ShiftMaskLength(d)))) {
            break;
        }
        v.reset(OpPPC64CLRLSLDI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 64 - getPPC64ShiftMaskLength(d), 63, 64));
        v.AddArg(x);
        return true;
    } 
    // match: (SLDconst [c] z:(AND (MOVDconst [d]) x))
    // cond: z.Uses == 1 && isPPC64ValidShiftMask(d) && c<=(64-getPPC64ShiftMaskLength(d))
    // result: (CLRLSLDI [newPPC64ShiftAuxInt(c,64-getPPC64ShiftMaskLength(d),63,64)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64AND) {
            break;
        }
        _ = z.Args[1];
        var z_0 = z.Args[0];
        var z_1 = z.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (z_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                }
                d = auxIntToInt64(z_0.AuxInt);
                x = z_1;
                if (!(z.Uses == 1 && isPPC64ValidShiftMask(d) && c <= (64 - getPPC64ShiftMaskLength(d)))) {
                    continue;
                }
                v.reset(OpPPC64CLRLSLDI);
                v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 64 - getPPC64ShiftMaskLength(d), 63, 64));
                v.AddArg(x);
                return true;
            }

        }
        break;
    } 
    // match: (SLDconst [c] z:(MOVWreg x))
    // cond: c < 32 && buildcfg.GOPPC64 >= 9
    // result: (EXTSWSLconst [c] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64MOVWreg) {
            break;
        }
        x = z.Args[0];
        if (!(c < 32 && buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64EXTSWSLconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SLW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SLW x (MOVDconst [c]))
    // result: (SLWconst [c&31 | (c>>5&1*31)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SLWconst);
        v.AuxInt = int64ToAuxInt(c & 31 | (c >> 5 & 1 * 31));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SLWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLWconst [c] z:(MOVBZreg x))
    // cond: z.Uses == 1 && c < 8
    // result: (CLRLSLWI [newPPC64ShiftAuxInt(c,24,31,32)] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        var z = v_0;
        if (z.Op != OpPPC64MOVBZreg) {
            break;
        }
        var x = z.Args[0];
        if (!(z.Uses == 1 && c < 8)) {
            break;
        }
        v.reset(OpPPC64CLRLSLWI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 24, 31, 32));
        v.AddArg(x);
        return true;
    } 
    // match: (SLWconst [c] z:(MOVHZreg x))
    // cond: z.Uses == 1 && c < 16
    // result: (CLRLSLWI [newPPC64ShiftAuxInt(c,16,31,32)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64MOVHZreg) {
            break;
        }
        x = z.Args[0];
        if (!(z.Uses == 1 && c < 16)) {
            break;
        }
        v.reset(OpPPC64CLRLSLWI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 16, 31, 32));
        v.AddArg(x);
        return true;
    } 
    // match: (SLWconst [c] z:(ANDconst [d] x))
    // cond: z.Uses == 1 && isPPC64ValidShiftMask(d) && c<=(32-getPPC64ShiftMaskLength(d))
    // result: (CLRLSLWI [newPPC64ShiftAuxInt(c,32-getPPC64ShiftMaskLength(d),31,32)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64ANDconst) {
            break;
        }
        var d = auxIntToInt64(z.AuxInt);
        x = z.Args[0];
        if (!(z.Uses == 1 && isPPC64ValidShiftMask(d) && c <= (32 - getPPC64ShiftMaskLength(d)))) {
            break;
        }
        v.reset(OpPPC64CLRLSLWI);
        v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 32 - getPPC64ShiftMaskLength(d), 31, 32));
        v.AddArg(x);
        return true;
    } 
    // match: (SLWconst [c] z:(AND (MOVDconst [d]) x))
    // cond: z.Uses == 1 && isPPC64ValidShiftMask(d) && c<=(32-getPPC64ShiftMaskLength(d))
    // result: (CLRLSLWI [newPPC64ShiftAuxInt(c,32-getPPC64ShiftMaskLength(d),31,32)] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64AND) {
            break;
        }
        _ = z.Args[1];
        var z_0 = z.Args[0];
        var z_1 = z.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (z_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                }
                d = auxIntToInt64(z_0.AuxInt);
                x = z_1;
                if (!(z.Uses == 1 && isPPC64ValidShiftMask(d) && c <= (32 - getPPC64ShiftMaskLength(d)))) {
                    continue;
                }
                v.reset(OpPPC64CLRLSLWI);
                v.AuxInt = int32ToAuxInt(newPPC64ShiftAuxInt(c, 32 - getPPC64ShiftMaskLength(d), 31, 32));
                v.AddArg(x);
                return true;
            }

        }
        break;
    } 
    // match: (SLWconst [c] z:(MOVWreg x))
    // cond: c < 32 && buildcfg.GOPPC64 >= 9
    // result: (EXTSWSLconst [c] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        z = v_0;
        if (z.Op != OpPPC64MOVWreg) {
            break;
        }
        x = z.Args[0];
        if (!(c < 32 && buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64EXTSWSLconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SRAD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRAD x (MOVDconst [c]))
    // result: (SRADconst [c&63 | (c>>6&1*63)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c & 63 | (c >> 6 & 1 * 63));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SRAW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRAW x (MOVDconst [c]))
    // result: (SRAWconst [c&31 | (c>>5&1*31)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c & 31 | (c >> 5 & 1 * 31));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SRD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRD x (MOVDconst [c]))
    // result: (SRDconst [c&63 | (c>>6&1*63)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c & 63 | (c >> 6 & 1 * 63));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SRW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRW x (MOVDconst [c]))
    // result: (SRWconst [c&31 | (c>>5&1*31)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c & 31 | (c >> 5 & 1 * 31));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SRWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRWconst (ANDconst [m] x) [s])
    // cond: mergePPC64RShiftMask(m>>uint(s),s,32) == 0
    // result: (MOVDconst [0])
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ANDconst) {
            break;
        }
        var m = auxIntToInt64(v_0.AuxInt);
        if (!(mergePPC64RShiftMask(m >> (int)(uint(s)), s, 32) == 0)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (SRWconst (ANDconst [m] x) [s])
    // cond: mergePPC64AndSrwi(m>>uint(s),s) != 0
    // result: (RLWINM [mergePPC64AndSrwi(m>>uint(s),s)] x)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64ANDconst) {
            break;
        }
        m = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(mergePPC64AndSrwi(m >> (int)(uint(s)), s) != 0)) {
            break;
        }
        v.reset(OpPPC64RLWINM);
        v.AuxInt = int64ToAuxInt(mergePPC64AndSrwi(m >> (int)(uint(s)), s));
        v.AddArg(x);
        return true;
    } 
    // match: (SRWconst (AND (MOVDconst [m]) x) [s])
    // cond: mergePPC64RShiftMask(m>>uint(s),s,32) == 0
    // result: (MOVDconst [0])
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                m = auxIntToInt64(v_0_0.AuxInt);
                if (!(mergePPC64RShiftMask(m >> (int)(uint(s)), s, 32) == 0)) {
                    continue;
                }
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (SRWconst (AND (MOVDconst [m]) x) [s])
    // cond: mergePPC64AndSrwi(m>>uint(s),s) != 0
    // result: (RLWINM [mergePPC64AndSrwi(m>>uint(s),s)] x)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64AND) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                m = auxIntToInt64(v_0_0.AuxInt);
                x = v_0_1;
                if (!(mergePPC64AndSrwi(m >> (int)(uint(s)), s) != 0)) {
                    continue;
                }
                v.reset(OpPPC64RLWINM);
                v.AuxInt = int64ToAuxInt(mergePPC64AndSrwi(m >> (int)(uint(s)), s));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SUB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUB x (MOVDconst [c]))
    // cond: is32Bit(-c)
    // result: (ADDconst [-c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(is32Bit(-c))) {
            break;
        }
        v.reset(OpPPC64ADDconst);
        v.AuxInt = int64ToAuxInt(-c);
        v.AddArg(x);
        return true;
    } 
    // match: (SUB (MOVDconst [c]) x)
    // cond: is32Bit(c)
    // result: (SUBFCconst [c] x)
    while (true) {
        if (v_0.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_0.AuxInt);
        x = v_1;
        if (!(is32Bit(c))) {
            break;
        }
        v.reset(OpPPC64SUBFCconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64SUBFCconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SUBFCconst [c] (NEG x))
    // result: (ADDconst [c] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64NEG) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpPPC64ADDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (SUBFCconst [c] (SUBFCconst [d] x))
    // cond: is32Bit(c-d)
    // result: (ADDconst [c-d] x)
    while (true) {
        c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64SUBFCconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(is32Bit(c - d))) {
            break;
        }
        v.reset(OpPPC64ADDconst);
        v.AuxInt = int64ToAuxInt(c - d);
        v.AddArg(x);
        return true;
    } 
    // match: (SUBFCconst [0] x)
    // result: (NEG x)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.reset(OpPPC64NEG);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64XOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (XOR (SLDconst x [c]) (SRDconst x [d]))
    // cond: d == 64-c
    // result: (ROTLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt64(v_0.AuxInt);
                var x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRDconst) {
                    continue;
                }
                var d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 64 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLWconst x [c]) (SRWconst x [d]))
    // cond: d == 32-c
    // result: (ROTLWconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpPPC64SRWconst) {
                    continue;
                }
                d = auxIntToInt64(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(OpPPC64ROTLWconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                var v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                var y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                var v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                var v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 64) {
                    continue;
                }
                var v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 63 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLD x (ANDconst <typ.Int64> [63] y)) (SRD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y))))
    // result: (ROTL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int64 || auxIntToInt64(v_0_1.AuxInt) != 63) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRD) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 64) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 63 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUBFCconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 32) {
                    continue;
                }
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64ANDconst || v_1_1_0.Type != typ.UInt || auxIntToInt64(v_1_1_0.AuxInt) != 31 || y != v_1_1_0.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR (SLW x (ANDconst <typ.Int32> [31] y)) (SRW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y))))
    // result: (ROTLW x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpPPC64SLW) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_0.Args[1];
                x = v_0.Args[0];
                v_0_1 = v_0.Args[1];
                if (v_0_1.Op != OpPPC64ANDconst || v_0_1.Type != typ.Int32 || auxIntToInt64(v_0_1.AuxInt) != 31) {
                    continue;
                }
                y = v_0_1.Args[0];
                if (v_1.Op != OpPPC64SRW) {
                    continue;
                }
                _ = v_1.Args[1];
                if (x != v_1.Args[0]) {
                    continue;
                }
                v_1_1 = v_1.Args[1];
                if (v_1_1.Op != OpPPC64SUB || v_1_1.Type != typ.UInt) {
                    continue;
                }
                _ = v_1_1.Args[1];
                v_1_1_0 = v_1_1.Args[0];
                if (v_1_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_0.AuxInt) != 32) {
                    continue;
                }
                v_1_1_1 = v_1_1.Args[1];
                if (v_1_1_1.Op != OpPPC64ANDconst || v_1_1_1.Type != typ.UInt || auxIntToInt64(v_1_1_1.AuxInt) != 31 || y != v_1_1_1.Args[0]) {
                    continue;
                }
                v.reset(OpPPC64ROTLW);
                v.AddArg2(x, y);
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
                if (v_0.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_0.AuxInt);
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                }
                d = auxIntToInt64(v_1.AuxInt);
                v.reset(OpPPC64MOVDconst);
                v.AuxInt = int64ToAuxInt(c ^ d);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XOR x (MOVDconst [c]))
    // cond: isU32Bit(c)
    // result: (XORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpPPC64MOVDconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt64(v_1.AuxInt);
                if (!(isU32Bit(c))) {
                    continue;
                }
                v.reset(OpPPC64XORconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPPC64XORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORconst [c] (XORconst [d] x))
    // result: (XORconst [c^d] x)
    while (true) {
        var c = auxIntToInt64(v.AuxInt);
        if (v_0.Op != OpPPC64XORconst) {
            break;
        }
        var d = auxIntToInt64(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpPPC64XORconst);
        v.AuxInt = int64ToAuxInt(c ^ d);
        v.AddArg(x);
        return true;
    } 
    // match: (XORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPanicBounds(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64LoweredPanicBoundsA);
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
        v.reset(OpPPC64LoweredPanicBoundsB);
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
        v.reset(OpPPC64LoweredPanicBoundsC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpPopCount16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount16 x)
    // result: (POPCNTW (MOVHZreg x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64POPCNTW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPopCount32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount32 x)
    // result: (POPCNTW (MOVWZreg x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64POPCNTW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVWZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpPopCount8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (PopCount8 x)
    // result: (POPCNTB (MOVBZreg x))
    while (true) {
        var x = v_0;
        v.reset(OpPPC64POPCNTB);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRotateLeft16(ptr<Value> _addr_v) {
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
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr16);
        var v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(c & 15);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(-c & 15);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpRotateLeft32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft32 x (MOVDconst [c]))
    // result: (ROTLWconst [c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64ROTLWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (RotateLeft32 x y)
    // result: (ROTLW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpPPC64ROTLW);
        v.AddArg2(x, y);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRotateLeft64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft64 x (MOVDconst [c]))
    // result: (ROTLconst [c&63] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpPPC64ROTLconst);
        v.AuxInt = int64ToAuxInt(c & 63);
        v.AddArg(x);
        return true;
    } 
    // match: (RotateLeft64 x y)
    // result: (ROTL x y)
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpPPC64ROTL);
        v.AddArg2(x, y);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRotateLeft8(ptr<Value> _addr_v) {
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
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        v.reset(OpOr8);
        var v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(c & 7);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(-c & 7);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpRsh16Ux16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux16 x y)
    // result: (SRW (ZeroExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux32 x (MOVDconst [c]))
    // cond: uint32(c) < 16
    // result: (SRWconst (ZeroExt16to32 x) [c&15])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c & 15);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh16Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux32 x y)
    // result: (SRW (ZeroExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 16
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Rsh16Ux64 x (MOVDconst [c]))
    // cond: uint64(c) < 16
    // result: (SRWconst (ZeroExt16to32 x) [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh16Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVHZreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux64 x y)
    // result: (SRW (ZeroExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16Ux8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16Ux8 x y)
    // result: (SRW (ZeroExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x16 x y)
    // result: (SRAW (SignExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x32 x (MOVDconst [c]))
    // cond: uint32(c) < 16
    // result: (SRAWconst (SignExt16to32 x) [c&15])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c & 15);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh16x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x32 x y)
    // result: (SRAW (SignExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 x (MOVDconst [c]))
    // cond: uint64(c) >= 16
    // result: (SRAWconst (SignExt16to32 x) [63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(63);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh16x64 x (MOVDconst [c]))
    // cond: uint64(c) < 16
    // result: (SRAWconst (SignExt16to32 x) [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh16x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVHreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x64 x y)
    // result: (SRAW (SignExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh16x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh16x8 x y)
    // result: (SRAW (SignExt16to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [16]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(16);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32Ux16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux16 x y)
    // result: (SRW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux32 x (MOVDconst [c]))
    // cond: uint32(c) < 32
    // result: (SRWconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux32 x y)
    // result: (SRW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 32
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Rsh32Ux64 x (MOVDconst [c]))
    // cond: uint64(c) < 32
    // result: (SRWconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux64 x (AND y (MOVDconst [31])))
    // result: (SRW x (ANDconst <typ.Int32> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SRW);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int32);
                v0.AuxInt = int64ToAuxInt(31);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32Ux64 x (ANDconst <typ.UInt> [31] y))
    // result: (SRW x (ANDconst <typ.UInt> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 31) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(31);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32Ux64 x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    // result: (SRW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 32) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64ANDconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 31) {
            break;
        }
        y = v_1_1.Args[0];
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(32);
        var v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v2.AuxInt = int64ToAuxInt(31);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32Ux64 x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    // result: (SRW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64ANDconst || v_1_0.Type != typ.UInt || auxIntToInt64(v_1_0.AuxInt) != 31) {
            break;
        }
        y = v_1_0.Args[0];
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(32);
        v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v1.AuxInt = int64ToAuxInt(31);
        v1.AddArg(y);
        v0.AddArg(v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32Ux64 x (SUB <typ.UInt> (MOVDconst [32]) (AND <typ.UInt> y (MOVDconst [31]))))
    // result: (SRW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 32) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64AND || v_1_1.Type != typ.UInt) {
            break;
        }
        _ = v_1_1.Args[1];
        var v_1_1_0 = v_1_1.Args[0];
        var v_1_1_1 = v_1_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_1_0;
                if (v_1_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_1_0, v_1_1_1) = (_i0 + 1, v_1_1_1, v_1_1_0);
                }
                v.reset(OpPPC64SRW);
                v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
                v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
                v1.AuxInt = int64ToAuxInt(32);
                v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v2.AuxInt = int64ToAuxInt(31);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32Ux64 x (SUBFCconst <typ.UInt> [32] (AND <typ.UInt> y (MOVDconst [31]))))
    // result: (SRW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64AND || v_1_0.Type != typ.UInt) {
            break;
        }
        _ = v_1_0.Args[1];
        var v_1_0_0 = v_1_0.Args[0];
        var v_1_0_1 = v_1_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0_0;
                if (v_1_0_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_0_0, v_1_0_1) = (_i0 + 1, v_1_0_1, v_1_0_0);
                }
                v.reset(OpPPC64SRW);
                v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
                v0.AuxInt = int64ToAuxInt(32);
                v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v1.AuxInt = int64ToAuxInt(31);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32Ux64 x y)
    // result: (SRW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32Ux8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32Ux8 x y)
    // result: (SRW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x16 x y)
    // result: (SRAW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x32 x (MOVDconst [c]))
    // cond: uint32(c) < 32
    // result: (SRAWconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x32 x y)
    // result: (SRAW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x64 x (MOVDconst [c]))
    // cond: uint64(c) >= 32
    // result: (SRAWconst x [63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(63);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32x64 x (MOVDconst [c]))
    // cond: uint64(c) < 32
    // result: (SRAWconst x [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x64 x (AND y (MOVDconst [31])))
    // result: (SRAW x (ANDconst <typ.Int32> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SRAW);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int32);
                v0.AuxInt = int64ToAuxInt(31);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32x64 x (ANDconst <typ.UInt> [31] y))
    // result: (SRAW x (ANDconst <typ.UInt> [31] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 31) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(31);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x64 x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    // result: (SRAW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 32) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64ANDconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 31) {
            break;
        }
        y = v_1_1.Args[0];
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(32);
        var v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v2.AuxInt = int64ToAuxInt(31);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x64 x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    // result: (SRAW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64ANDconst || v_1_0.Type != typ.UInt || auxIntToInt64(v_1_0.AuxInt) != 31) {
            break;
        }
        y = v_1_0.Args[0];
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(32);
        v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v1.AuxInt = int64ToAuxInt(31);
        v1.AddArg(y);
        v0.AddArg(v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x64 x (SUB <typ.UInt> (MOVDconst [32]) (AND <typ.UInt> y (MOVDconst [31]))))
    // result: (SRAW x (SUB <typ.UInt> (MOVDconst [32]) (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 32) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64AND || v_1_1.Type != typ.UInt) {
            break;
        }
        _ = v_1_1.Args[1];
        var v_1_1_0 = v_1_1.Args[0];
        var v_1_1_1 = v_1_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_1_0;
                if (v_1_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_1_0, v_1_1_1) = (_i0 + 1, v_1_1_1, v_1_1_0);
                }
                v.reset(OpPPC64SRAW);
                v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
                v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
                v1.AuxInt = int64ToAuxInt(32);
                v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v2.AuxInt = int64ToAuxInt(31);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32x64 x (SUBFCconst <typ.UInt> [32] (AND <typ.UInt> y (MOVDconst [31]))))
    // result: (SRAW x (SUBFCconst <typ.UInt> [32] (ANDconst <typ.UInt> [31] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 32) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64AND || v_1_0.Type != typ.UInt) {
            break;
        }
        _ = v_1_0.Args[1];
        var v_1_0_0 = v_1_0.Args[0];
        var v_1_0_1 = v_1_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0_0;
                if (v_1_0_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0_1.AuxInt) != 31) {
                    continue;
                (_i0, v_1_0_0, v_1_0_1) = (_i0 + 1, v_1_0_1, v_1_0_0);
                }
                v.reset(OpPPC64SRAW);
                v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
                v0.AuxInt = int64ToAuxInt(32);
                v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v1.AuxInt = int64ToAuxInt(31);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh32x64 x y)
    // result: (SRAW x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh32x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh32x8 x y)
    // result: (SRAW x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [32]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(32);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64Ux16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux16 x y)
    // result: (SRD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux32 x (MOVDconst [c]))
    // cond: uint32(c) < 64
    // result: (SRDconst x [c&63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c & 63);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh64Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux32 x y)
    // result: (SRD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64Ux64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 64
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Rsh64Ux64 x (MOVDconst [c]))
    // cond: uint64(c) < 64
    // result: (SRDconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SRDconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh64Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux64 x (AND y (MOVDconst [63])))
    // result: (SRD x (ANDconst <typ.Int64> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SRD);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int64);
                v0.AuxInt = int64ToAuxInt(63);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64Ux64 x (ANDconst <typ.UInt> [63] y))
    // result: (SRD x (ANDconst <typ.UInt> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 63) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SRD);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(63);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64Ux64 x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    // result: (SRD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 64) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64ANDconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 63) {
            break;
        }
        y = v_1_1.Args[0];
        v.reset(OpPPC64SRD);
        v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(64);
        var v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v2.AuxInt = int64ToAuxInt(63);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64Ux64 x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    // result: (SRD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 64) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64ANDconst || v_1_0.Type != typ.UInt || auxIntToInt64(v_1_0.AuxInt) != 63) {
            break;
        }
        y = v_1_0.Args[0];
        v.reset(OpPPC64SRD);
        v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(64);
        v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v1.AuxInt = int64ToAuxInt(63);
        v1.AddArg(y);
        v0.AddArg(v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64Ux64 x (SUB <typ.UInt> (MOVDconst [64]) (AND <typ.UInt> y (MOVDconst [63]))))
    // result: (SRD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 64) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64AND || v_1_1.Type != typ.UInt) {
            break;
        }
        _ = v_1_1.Args[1];
        var v_1_1_0 = v_1_1.Args[0];
        var v_1_1_1 = v_1_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_1_0;
                if (v_1_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_1_0, v_1_1_1) = (_i0 + 1, v_1_1_1, v_1_1_0);
                }
                v.reset(OpPPC64SRD);
                v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
                v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
                v1.AuxInt = int64ToAuxInt(64);
                v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v2.AuxInt = int64ToAuxInt(63);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64Ux64 x (SUBFCconst <typ.UInt> [64] (AND <typ.UInt> y (MOVDconst [63]))))
    // result: (SRD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 64) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64AND || v_1_0.Type != typ.UInt) {
            break;
        }
        _ = v_1_0.Args[1];
        var v_1_0_0 = v_1_0.Args[0];
        var v_1_0_1 = v_1_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0_0;
                if (v_1_0_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_0_0, v_1_0_1) = (_i0 + 1, v_1_0_1, v_1_0_0);
                }
                v.reset(OpPPC64SRD);
                v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
                v0.AuxInt = int64ToAuxInt(64);
                v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v1.AuxInt = int64ToAuxInt(63);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64Ux64 x y)
    // result: (SRD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRD);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64Ux8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64Ux8 x y)
    // result: (SRD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x16 x y)
    // result: (SRAD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x32 x (MOVDconst [c]))
    // cond: uint32(c) < 64
    // result: (SRADconst x [c&63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c & 63);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh64x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x32 x y)
    // result: (SRAD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh64x64 x (MOVDconst [c]))
    // cond: uint64(c) >= 64
    // result: (SRADconst x [63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 64)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(63);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh64x64 x (MOVDconst [c]))
    // cond: uint64(c) < 64
    // result: (SRADconst x [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 64)) {
            break;
        }
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh64x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAD x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x64 x (AND y (MOVDconst [63])))
    // result: (SRAD x (ANDconst <typ.Int64> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64AND) {
            break;
        }
        _ = v_1.Args[1];
        var v_1_0 = v_1.Args[0];
        var v_1_1 = v_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0;
                if (v_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_0, v_1_1) = (_i0 + 1, v_1_1, v_1_0);
                }
                v.reset(OpPPC64SRAD);
                var v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.Int64);
                v0.AuxInt = int64ToAuxInt(63);
                v0.AddArg(y);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64x64 x (ANDconst <typ.UInt> [63] y))
    // result: (SRAD x (ANDconst <typ.UInt> [63] y))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64ANDconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 63) {
            break;
        }
        y = v_1.Args[0];
        v.reset(OpPPC64SRAD);
        v0 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(63);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64x64 x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    // result: (SRAD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 64) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64ANDconst || v_1_1.Type != typ.UInt || auxIntToInt64(v_1_1.AuxInt) != 63) {
            break;
        }
        y = v_1_1.Args[0];
        v.reset(OpPPC64SRAD);
        v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(64);
        var v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v2.AuxInt = int64ToAuxInt(63);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64x64 x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    // result: (SRAD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 64) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64ANDconst || v_1_0.Type != typ.UInt || auxIntToInt64(v_1_0.AuxInt) != 63) {
            break;
        }
        y = v_1_0.Args[0];
        v.reset(OpPPC64SRAD);
        v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
        v0.AuxInt = int64ToAuxInt(64);
        v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
        v1.AuxInt = int64ToAuxInt(63);
        v1.AddArg(y);
        v0.AddArg(v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh64x64 x (SUB <typ.UInt> (MOVDconst [64]) (AND <typ.UInt> y (MOVDconst [63]))))
    // result: (SRAD x (SUB <typ.UInt> (MOVDconst [64]) (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUB || v_1.Type != typ.UInt) {
            break;
        }
        _ = v_1.Args[1];
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0.AuxInt) != 64) {
            break;
        }
        v_1_1 = v_1.Args[1];
        if (v_1_1.Op != OpPPC64AND || v_1_1.Type != typ.UInt) {
            break;
        }
        _ = v_1_1.Args[1];
        var v_1_1_0 = v_1_1.Args[0];
        var v_1_1_1 = v_1_1.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_1_0;
                if (v_1_1_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_1_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_1_0, v_1_1_1) = (_i0 + 1, v_1_1_1, v_1_1_0);
                }
                v.reset(OpPPC64SRAD);
                v0 = b.NewValue0(v.Pos, OpPPC64SUB, typ.UInt);
                v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
                v1.AuxInt = int64ToAuxInt(64);
                v2 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v2.AuxInt = int64ToAuxInt(63);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64x64 x (SUBFCconst <typ.UInt> [64] (AND <typ.UInt> y (MOVDconst [63]))))
    // result: (SRAD x (SUBFCconst <typ.UInt> [64] (ANDconst <typ.UInt> [63] y)))
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64SUBFCconst || v_1.Type != typ.UInt || auxIntToInt64(v_1.AuxInt) != 64) {
            break;
        }
        v_1_0 = v_1.Args[0];
        if (v_1_0.Op != OpPPC64AND || v_1_0.Type != typ.UInt) {
            break;
        }
        _ = v_1_0.Args[1];
        var v_1_0_0 = v_1_0.Args[0];
        var v_1_0_1 = v_1_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                y = v_1_0_0;
                if (v_1_0_1.Op != OpPPC64MOVDconst || auxIntToInt64(v_1_0_1.AuxInt) != 63) {
                    continue;
                (_i0, v_1_0_0, v_1_0_1) = (_i0 + 1, v_1_0_1, v_1_0_0);
                }
                v.reset(OpPPC64SRAD);
                v0 = b.NewValue0(v.Pos, OpPPC64SUBFCconst, typ.UInt);
                v0.AuxInt = int64ToAuxInt(64);
                v1 = b.NewValue0(v.Pos, OpPPC64ANDconst, typ.UInt);
                v1.AuxInt = int64ToAuxInt(63);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg2(x, v0);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (Rsh64x64 x y)
    // result: (SRAD x (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAD);
        v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v3.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(y, v3);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh64x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAD);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (Rsh64x8 x y)
    // result: (SRAD x (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [64]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAD);
        var v0 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v1.AuxInt = int64ToAuxInt(-1);
        var v2 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v3.AddArg(y);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(64);
        v2.AddArg2(v3, v4);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8Ux16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux16 x y)
    // result: (SRW (ZeroExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux32 x (MOVDconst [c]))
    // cond: uint32(c) < 8
    // result: (SRWconst (ZeroExt8to32 x) [c&7])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c & 7);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh8Ux32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux32 x y)
    // result: (SRW (ZeroExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 _ (MOVDconst [c]))
    // cond: uint64(c) >= 8
    // result: (MOVDconst [0])
    while (true) {
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpPPC64MOVDconst);
        v.AuxInt = int64ToAuxInt(0);
        return true;
    } 
    // match: (Rsh8Ux64 x (MOVDconst [c]))
    // cond: uint64(c) < 8
    // result: (SRWconst (ZeroExt8to32 x) [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SRWconst);
        v.AuxInt = int64ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh8Ux64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRW (MOVBZreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux64 x y)
    // result: (SRW (ZeroExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8Ux8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBZreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8Ux8 x y)
    // result: (SRW (ZeroExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRW);
        v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8x16(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x16 x y)
    // result: (SRAW (SignExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt16to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x32 x (MOVDconst [c]))
    // cond: uint32(c) < 8
    // result: (SRAWconst (SignExt8to32 x) [c&7])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c & 7);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh8x32 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x32 x y)
    // result: (SRAW (SignExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 x (MOVDconst [c]))
    // cond: uint64(c) >= 8
    // result: (SRAWconst (SignExt8to32 x) [63])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(63);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh8x64 x (MOVDconst [c]))
    // cond: uint64(c) < 8
    // result: (SRAWconst (SignExt8to32 x) [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpPPC64MOVDconst) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpPPC64SRAWconst);
        v.AuxInt = int64ToAuxInt(c);
        v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (Rsh8x64 x y)
    // cond: shiftIsBounded(v)
    // result: (SRAW (MOVBreg x) y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x64 x y)
    // result: (SRAW (SignExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU y (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v4.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(y, v4);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpRsh8x8(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64SRAW);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVBreg, typ.Int64);
        v0.AddArg(x);
        v.AddArg2(v0, y);
        return true;
    } 
    // match: (Rsh8x8 x y)
    // result: (SRAW (SignExt8to32 x) (ISEL [0] y (MOVDconst [-1]) (CMPU (ZeroExt8to64 y) (MOVDconst [8]))))
    while (true) {
        x = v_0;
        y = v_1;
        v.reset(OpPPC64SRAW);
        v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpPPC64ISEL, typ.Int32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v2.AuxInt = int64ToAuxInt(-1);
        var v3 = b.NewValue0(v.Pos, OpPPC64CMPU, types.TypeFlags);
        var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
        v4.AddArg(y);
        var v5 = b.NewValue0(v.Pos, OpPPC64MOVDconst, typ.Int64);
        v5.AuxInt = int64ToAuxInt(8);
        v3.AddArg2(v4, v5);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValuePPC64_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Slicemask <t> x)
    // result: (SRADconst (NEG <t> x) [63])
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpPPC64SRADconst);
        v.AuxInt = int64ToAuxInt(63);
        var v0 = b.NewValue0(v.Pos, OpPPC64NEG, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValuePPC64_OpStore(ptr<Value> _addr_v) {
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
        v.reset(OpPPC64FMOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && is32BitFloat(val.Type)
    // result: (FMOVDstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8 && is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpPPC64FMOVDstore);
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
        v.reset(OpPPC64FMOVSstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && (is64BitInt(val.Type) || isPtr(val.Type))
    // result: (MOVDstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8 && (is64BitInt(val.Type) || isPtr(val.Type)))) {
            break;
        }
        v.reset(OpPPC64MOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && is32BitInt(val.Type)
    // result: (MOVWstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && is32BitInt(val.Type))) {
            break;
        }
        v.reset(OpPPC64MOVWstore);
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
        v.reset(OpPPC64MOVHstore);
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
        v.reset(OpPPC64MOVBstore);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValuePPC64_OpTrunc16to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc16to8 <t> x)
    // cond: isSigned(t)
    // result: (MOVBreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc16to8 x)
    // result: (MOVBZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVBZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpTrunc32to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc32to16 <t> x)
    // cond: isSigned(t)
    // result: (MOVHreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc32to16 x)
    // result: (MOVHZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVHZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpTrunc32to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc32to8 <t> x)
    // cond: isSigned(t)
    // result: (MOVBreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc32to8 x)
    // result: (MOVBZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVBZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpTrunc64to16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to16 <t> x)
    // cond: isSigned(t)
    // result: (MOVHreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVHreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc64to16 x)
    // result: (MOVHZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVHZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpTrunc64to32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to32 <t> x)
    // cond: isSigned(t)
    // result: (MOVWreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVWreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc64to32 x)
    // result: (MOVWZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVWZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpTrunc64to8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Trunc64to8 <t> x)
    // cond: isSigned(t)
    // result: (MOVBreg x)
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(isSigned(t))) {
            break;
        }
        v.reset(OpPPC64MOVBreg);
        v.AddArg(x);
        return true;
    } 
    // match: (Trunc64to8 x)
    // result: (MOVBZreg x)
    while (true) {
        x = v_0;
        v.reset(OpPPC64MOVBZreg);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValuePPC64_OpZero(ptr<Value> _addr_v) {
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
    // result: (MOVBstorezero destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVBstorezero);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [2] destptr mem)
    // result: (MOVHstorezero destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVHstorezero);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [3] destptr mem)
    // result: (MOVBstorezero [2] destptr (MOVHstorezero destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(2);
        var v0 = b.NewValue0(v.Pos, OpPPC64MOVHstorezero, types.TypeMem);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [4] destptr mem)
    // result: (MOVWstorezero destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVWstorezero);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [5] destptr mem)
    // result: (MOVBstorezero [4] destptr (MOVWstorezero destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWstorezero, types.TypeMem);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [6] destptr mem)
    // result: (MOVHstorezero [4] destptr (MOVWstorezero destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVHstorezero);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVWstorezero, types.TypeMem);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [7] destptr mem)
    // result: (MOVBstorezero [6] destptr (MOVHstorezero [4] destptr (MOVWstorezero destptr mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVBstorezero);
        v.AuxInt = int32ToAuxInt(6);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVHstorezero, types.TypeMem);
        v0.AuxInt = int32ToAuxInt(4);
        var v1 = b.NewValue0(v.Pos, OpPPC64MOVWstorezero, types.TypeMem);
        v1.AddArg2(destptr, mem);
        v0.AddArg2(destptr, v1);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [8] {t} destptr mem)
    // result: (MOVDstorezero destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVDstorezero);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [12] {t} destptr mem)
    // result: (MOVWstorezero [8] destptr (MOVDstorezero [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 12) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVWstorezero);
        v.AuxInt = int32ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [16] {t} destptr mem)
    // result: (MOVDstorezero [8] destptr (MOVDstorezero [0] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [24] {t} destptr mem)
    // result: (MOVDstorezero [16] destptr (MOVDstorezero [8] destptr (MOVDstorezero [0] destptr mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 24) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(16);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v0.AuxInt = int32ToAuxInt(8);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg2(destptr, mem);
        v0.AddArg2(destptr, v1);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [32] {t} destptr mem)
    // result: (MOVDstorezero [24] destptr (MOVDstorezero [16] destptr (MOVDstorezero [8] destptr (MOVDstorezero [0] destptr mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 32) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(OpPPC64MOVDstorezero);
        v.AuxInt = int32ToAuxInt(24);
        v0 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v0.AuxInt = int32ToAuxInt(16);
        v1 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(8);
        var v2 = b.NewValue0(v.Pos, OpPPC64MOVDstorezero, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg2(destptr, mem);
        v1.AddArg2(destptr, v2);
        v0.AddArg2(destptr, v1);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [s] ptr mem)
    // cond: buildcfg.GOPPC64 <= 8 && s < 64
    // result: (LoweredZeroShort [s] ptr mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        mem = v_1;
        if (!(buildcfg.GOPPC64 <= 8 && s < 64)) {
            break;
        }
        v.reset(OpPPC64LoweredZeroShort);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Zero [s] ptr mem)
    // cond: buildcfg.GOPPC64 <= 8
    // result: (LoweredZero [s] ptr mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        mem = v_1;
        if (!(buildcfg.GOPPC64 <= 8)) {
            break;
        }
        v.reset(OpPPC64LoweredZero);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Zero [s] ptr mem)
    // cond: s < 128 && buildcfg.GOPPC64 >= 9
    // result: (LoweredQuadZeroShort [s] ptr mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        mem = v_1;
        if (!(s < 128 && buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64LoweredQuadZeroShort);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Zero [s] ptr mem)
    // cond: buildcfg.GOPPC64 >= 9
    // result: (LoweredQuadZero [s] ptr mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        mem = v_1;
        if (!(buildcfg.GOPPC64 >= 9)) {
            break;
        }
        v.reset(OpPPC64LoweredQuadZero);
        v.AuxInt = int64ToAuxInt(s);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteBlockPPC64(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Kind == BlockPPC64EQ) 
        // match: (EQ (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (EQ (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            var v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            var v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            var c = auxIntToInt64(v_0_0.AuxInt);
            var x = v_0_0.Args[0];
            var v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64EQ, v0);
            return true;
        } 
        // match: (EQ (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (EQ (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64EQ, v0);
            return true;
        } 
        // match: (EQ (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (EQ (FlagLT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (FlagGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (InvertFlags cmp) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            var cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64EQ, cmp);
            return true;
        } 
        // match: (EQ (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (EQ (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64EQ, v0);
            return true;
        } 
        // match: (EQ (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (EQ (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64EQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (EQ (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            var z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            var z_0 = z.Args[0];
            var z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                nint _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    var y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64EQ, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (EQ (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (EQ (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64EQ, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (EQ (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (EQ (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64EQ, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
    else if (b.Kind == BlockPPC64GE) 
        // match: (GE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GE (FlagLT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GE (FlagGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GE (InvertFlags cmp) yes no)
        // result: (LE cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64LE, cmp);
            return true;
        } 
        // match: (GE (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (GE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64GE, v0);
            return true;
        } 
        // match: (GE (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (GE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64GE, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (GE (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (GE (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (GE (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (GE (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (GE (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
    else if (b.Kind == BlockPPC64GT) 
        // match: (GT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (FlagLT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (FlagGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GT (InvertFlags cmp) yes no)
        // result: (LT cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64LT, cmp);
            return true;
        } 
        // match: (GT (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (GT (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64GT, v0);
            return true;
        } 
        // match: (GT (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (GT (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64GT, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (GT (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (GT (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (GT (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (GT (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (GT (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64GT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
    else if (b.Kind == BlockIf) 
        // match: (If (Equal cc) yes no)
        // result: (EQ cc yes no)
        while (b.Controls[0].Op == OpPPC64Equal) {
            v_0 = b.Controls[0];
            var cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64EQ, cc);
            return true;
        } 
        // match: (If (NotEqual cc) yes no)
        // result: (NE cc yes no)
        while (b.Controls[0].Op == OpPPC64NotEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64NE, cc);
            return true;
        } 
        // match: (If (LessThan cc) yes no)
        // result: (LT cc yes no)
        while (b.Controls[0].Op == OpPPC64LessThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64LT, cc);
            return true;
        } 
        // match: (If (LessEqual cc) yes no)
        // result: (LE cc yes no)
        while (b.Controls[0].Op == OpPPC64LessEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64LE, cc);
            return true;
        } 
        // match: (If (GreaterThan cc) yes no)
        // result: (GT cc yes no)
        while (b.Controls[0].Op == OpPPC64GreaterThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64GT, cc);
            return true;
        } 
        // match: (If (GreaterEqual cc) yes no)
        // result: (GE cc yes no)
        while (b.Controls[0].Op == OpPPC64GreaterEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64GE, cc);
            return true;
        } 
        // match: (If (FLessThan cc) yes no)
        // result: (FLT cc yes no)
        while (b.Controls[0].Op == OpPPC64FLessThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64FLT, cc);
            return true;
        } 
        // match: (If (FLessEqual cc) yes no)
        // result: (FLE cc yes no)
        while (b.Controls[0].Op == OpPPC64FLessEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64FLE, cc);
            return true;
        } 
        // match: (If (FGreaterThan cc) yes no)
        // result: (FGT cc yes no)
        while (b.Controls[0].Op == OpPPC64FGreaterThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64FGT, cc);
            return true;
        } 
        // match: (If (FGreaterEqual cc) yes no)
        // result: (FGE cc yes no)
        while (b.Controls[0].Op == OpPPC64FGreaterEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockPPC64FGE, cc);
            return true;
        } 
        // match: (If cond yes no)
        // result: (NE (CMPWconst [0] cond) yes no)
        while (true) {
            var cond = b.Controls[0];
            v0 = b.NewValue0(cond.Pos, OpPPC64CMPWconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(0);
            v0.AddArg(cond);
            b.resetWithControl(BlockPPC64NE, v0);
            return true;
        }
    else if (b.Kind == BlockPPC64LE) 
        // match: (LE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagLT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LE (InvertFlags cmp) yes no)
        // result: (GE cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64GE, cmp);
            return true;
        } 
        // match: (LE (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (LE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64LE, v0);
            return true;
        } 
        // match: (LE (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (LE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64LE, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (LE (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (LE (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (LE (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (LE (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (LE (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
    else if (b.Kind == BlockPPC64LT) 
        // match: (LT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LT (FlagLT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LT (FlagGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LT (InvertFlags cmp) yes no)
        // result: (GT cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64GT, cmp);
            return true;
        } 
        // match: (LT (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (LT (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64LT, v0);
            return true;
        } 
        // match: (LT (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (LT (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64LT, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (LT (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (LT (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (LT (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (LT (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (LT (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64LT, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
    else if (b.Kind == BlockPPC64NE) 
        // match: (NE (CMPWconst [0] (Equal cc)) yes no)
        // result: (EQ cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64Equal) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64EQ, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (NotEqual cc)) yes no)
        // result: (NE cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64NotEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64NE, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (LessThan cc)) yes no)
        // result: (LT cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64LessThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64LT, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (LessEqual cc)) yes no)
        // result: (LE cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64LessEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64LE, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (GreaterThan cc)) yes no)
        // result: (GT cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64GreaterThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64GT, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (GreaterEqual cc)) yes no)
        // result: (GE cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64GreaterEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64GE, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (FLessThan cc)) yes no)
        // result: (FLT cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64FLessThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64FLT, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (FLessEqual cc)) yes no)
        // result: (FLE cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64FLessEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64FLE, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (FGreaterThan cc)) yes no)
        // result: (FGT cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64FGreaterThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64FGT, cc);
            return true;
        } 
        // match: (NE (CMPWconst [0] (FGreaterEqual cc)) yes no)
        // result: (FGE cc yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64FGreaterEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockPPC64FGE, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (NE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64NE, v0);
            return true;
        } 
        // match: (NE (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (NE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64NE, v0);
            return true;
        } 
        // match: (NE (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpPPC64FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (NE (FlagLT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagLT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (FlagGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpPPC64FlagGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (InvertFlags cmp) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpPPC64InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockPPC64NE, cmp);
            return true;
        } 
        // match: (NE (CMPconst [0] (ANDconst [c] x)) yes no)
        // result: (NE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64NE, v0);
            return true;
        } 
        // match: (NE (CMPWconst [0] (ANDconst [c] x)) yes no)
        // result: (NE (ANDCCconst [c] x) yes no)
        while (b.Controls[0].Op == OpPPC64CMPWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpPPC64ANDconst) {
                break;
            }
            c = auxIntToInt64(v_0_0.AuxInt);
            x = v_0_0.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCCconst, types.TypeFlags);
            v0.AuxInt = int64ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockPPC64NE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] z:(AND x y)) yes no)
        // cond: z.Uses == 1
        // result: (NE (ANDCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64AND) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ANDCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64NE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (NE (CMPconst [0] z:(OR x y)) yes no)
        // cond: z.Uses == 1
        // result: (NE (ORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64OR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64ORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64NE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        } 
        // match: (NE (CMPconst [0] z:(XOR x y)) yes no)
        // cond: z.Uses == 1
        // result: (NE (XORCC x y) yes no)
        while (b.Controls[0].Op == OpPPC64CMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt64(v_0.AuxInt) != 0) {
                break;
            }
            z = v_0.Args[0];
            if (z.Op != OpPPC64XOR) {
                break;
            }
            _ = z.Args[1];
            z_0 = z.Args[0];
            z_1 = z.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = z_0;
                    y = z_1;
                    if (!(z.Uses == 1)) {
                        continue;
                    (_i0, z_0, z_1) = (_i0 + 1, z_1, z_0);
                    }
                    v0 = b.NewValue0(v_0.Pos, OpPPC64XORCC, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockPPC64NE, v0);
                    return true;
                }


                _i0 = _i0__prev2;
            }
            break;
        }
        return false;
}

} // end ssa_package
