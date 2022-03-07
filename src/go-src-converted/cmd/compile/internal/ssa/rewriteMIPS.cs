// Code generated from gen/MIPS.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 23:03:23 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewriteMIPS.go
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValueMIPS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpAdd16) 
        v.Op = OpMIPSADD;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = OpMIPSADD;
        return true;
    else if (v.Op == OpAdd32F) 
        v.Op = OpMIPSADDF;
        return true;
    else if (v.Op == OpAdd32withcarry) 
        return rewriteValueMIPS_OpAdd32withcarry(_addr_v);
    else if (v.Op == OpAdd64F) 
        v.Op = OpMIPSADDD;
        return true;
    else if (v.Op == OpAdd8) 
        v.Op = OpMIPSADD;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = OpMIPSADD;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValueMIPS_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = OpMIPSAND;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = OpMIPSAND;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = OpMIPSAND;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = OpMIPSAND;
        return true;
    else if (v.Op == OpAtomicAdd32) 
        v.Op = OpMIPSLoweredAtomicAdd;
        return true;
    else if (v.Op == OpAtomicAnd32) 
        v.Op = OpMIPSLoweredAtomicAnd;
        return true;
    else if (v.Op == OpAtomicAnd8) 
        return rewriteValueMIPS_OpAtomicAnd8(_addr_v);
    else if (v.Op == OpAtomicCompareAndSwap32) 
        v.Op = OpMIPSLoweredAtomicCas;
        return true;
    else if (v.Op == OpAtomicExchange32) 
        v.Op = OpMIPSLoweredAtomicExchange;
        return true;
    else if (v.Op == OpAtomicLoad32) 
        v.Op = OpMIPSLoweredAtomicLoad32;
        return true;
    else if (v.Op == OpAtomicLoad8) 
        v.Op = OpMIPSLoweredAtomicLoad8;
        return true;
    else if (v.Op == OpAtomicLoadPtr) 
        v.Op = OpMIPSLoweredAtomicLoad32;
        return true;
    else if (v.Op == OpAtomicOr32) 
        v.Op = OpMIPSLoweredAtomicOr;
        return true;
    else if (v.Op == OpAtomicOr8) 
        return rewriteValueMIPS_OpAtomicOr8(_addr_v);
    else if (v.Op == OpAtomicStore32) 
        v.Op = OpMIPSLoweredAtomicStore32;
        return true;
    else if (v.Op == OpAtomicStore8) 
        v.Op = OpMIPSLoweredAtomicStore8;
        return true;
    else if (v.Op == OpAtomicStorePtrNoWB) 
        v.Op = OpMIPSLoweredAtomicStore32;
        return true;
    else if (v.Op == OpAvg32u) 
        return rewriteValueMIPS_OpAvg32u(_addr_v);
    else if (v.Op == OpBitLen32) 
        return rewriteValueMIPS_OpBitLen32(_addr_v);
    else if (v.Op == OpClosureCall) 
        v.Op = OpMIPSCALLclosure;
        return true;
    else if (v.Op == OpCom16) 
        return rewriteValueMIPS_OpCom16(_addr_v);
    else if (v.Op == OpCom32) 
        return rewriteValueMIPS_OpCom32(_addr_v);
    else if (v.Op == OpCom8) 
        return rewriteValueMIPS_OpCom8(_addr_v);
    else if (v.Op == OpConst16) 
        return rewriteValueMIPS_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        return rewriteValueMIPS_OpConst32(_addr_v);
    else if (v.Op == OpConst32F) 
        v.Op = OpMIPSMOVFconst;
        return true;
    else if (v.Op == OpConst64F) 
        v.Op = OpMIPSMOVDconst;
        return true;
    else if (v.Op == OpConst8) 
        return rewriteValueMIPS_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValueMIPS_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValueMIPS_OpConstNil(_addr_v);
    else if (v.Op == OpCtz32) 
        return rewriteValueMIPS_OpCtz32(_addr_v);
    else if (v.Op == OpCtz32NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCvt32Fto32) 
        v.Op = OpMIPSTRUNCFW;
        return true;
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = OpMIPSMOVFD;
        return true;
    else if (v.Op == OpCvt32to32F) 
        v.Op = OpMIPSMOVWF;
        return true;
    else if (v.Op == OpCvt32to64F) 
        v.Op = OpMIPSMOVWD;
        return true;
    else if (v.Op == OpCvt64Fto32) 
        v.Op = OpMIPSTRUNCDW;
        return true;
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = OpMIPSMOVDF;
        return true;
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        return rewriteValueMIPS_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValueMIPS_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValueMIPS_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        v.Op = OpMIPSDIVF;
        return true;
    else if (v.Op == OpDiv32u) 
        return rewriteValueMIPS_OpDiv32u(_addr_v);
    else if (v.Op == OpDiv64F) 
        v.Op = OpMIPSDIVD;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValueMIPS_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValueMIPS_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValueMIPS_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValueMIPS_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValueMIPS_OpEq32F(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValueMIPS_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValueMIPS_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValueMIPS_OpEqB(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValueMIPS_OpEqPtr(_addr_v);
    else if (v.Op == OpGetCallerPC) 
        v.Op = OpMIPSLoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = OpMIPSLoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = OpMIPSLoweredGetClosurePtr;
        return true;
    else if (v.Op == OpHmul32) 
        return rewriteValueMIPS_OpHmul32(_addr_v);
    else if (v.Op == OpHmul32u) 
        return rewriteValueMIPS_OpHmul32u(_addr_v);
    else if (v.Op == OpInterCall) 
        v.Op = OpMIPSCALLinter;
        return true;
    else if (v.Op == OpIsInBounds) 
        return rewriteValueMIPS_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValueMIPS_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValueMIPS_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValueMIPS_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValueMIPS_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValueMIPS_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValueMIPS_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValueMIPS_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValueMIPS_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValueMIPS_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValueMIPS_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValueMIPS_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValueMIPS_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValueMIPS_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValueMIPS_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValueMIPS_OpLess32U(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValueMIPS_OpLess64F(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValueMIPS_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValueMIPS_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValueMIPS_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValueMIPS_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValueMIPS_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValueMIPS_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValueMIPS_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValueMIPS_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValueMIPS_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValueMIPS_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValueMIPS_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValueMIPS_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValueMIPS_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValueMIPS_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValueMIPS_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValueMIPS_OpLsh8x8(_addr_v);
    else if (v.Op == OpMIPSADD) 
        return rewriteValueMIPS_OpMIPSADD(_addr_v);
    else if (v.Op == OpMIPSADDconst) 
        return rewriteValueMIPS_OpMIPSADDconst(_addr_v);
    else if (v.Op == OpMIPSAND) 
        return rewriteValueMIPS_OpMIPSAND(_addr_v);
    else if (v.Op == OpMIPSANDconst) 
        return rewriteValueMIPS_OpMIPSANDconst(_addr_v);
    else if (v.Op == OpMIPSCMOVZ) 
        return rewriteValueMIPS_OpMIPSCMOVZ(_addr_v);
    else if (v.Op == OpMIPSCMOVZzero) 
        return rewriteValueMIPS_OpMIPSCMOVZzero(_addr_v);
    else if (v.Op == OpMIPSLoweredAtomicAdd) 
        return rewriteValueMIPS_OpMIPSLoweredAtomicAdd(_addr_v);
    else if (v.Op == OpMIPSLoweredAtomicStore32) 
        return rewriteValueMIPS_OpMIPSLoweredAtomicStore32(_addr_v);
    else if (v.Op == OpMIPSMOVBUload) 
        return rewriteValueMIPS_OpMIPSMOVBUload(_addr_v);
    else if (v.Op == OpMIPSMOVBUreg) 
        return rewriteValueMIPS_OpMIPSMOVBUreg(_addr_v);
    else if (v.Op == OpMIPSMOVBload) 
        return rewriteValueMIPS_OpMIPSMOVBload(_addr_v);
    else if (v.Op == OpMIPSMOVBreg) 
        return rewriteValueMIPS_OpMIPSMOVBreg(_addr_v);
    else if (v.Op == OpMIPSMOVBstore) 
        return rewriteValueMIPS_OpMIPSMOVBstore(_addr_v);
    else if (v.Op == OpMIPSMOVBstorezero) 
        return rewriteValueMIPS_OpMIPSMOVBstorezero(_addr_v);
    else if (v.Op == OpMIPSMOVDload) 
        return rewriteValueMIPS_OpMIPSMOVDload(_addr_v);
    else if (v.Op == OpMIPSMOVDstore) 
        return rewriteValueMIPS_OpMIPSMOVDstore(_addr_v);
    else if (v.Op == OpMIPSMOVFload) 
        return rewriteValueMIPS_OpMIPSMOVFload(_addr_v);
    else if (v.Op == OpMIPSMOVFstore) 
        return rewriteValueMIPS_OpMIPSMOVFstore(_addr_v);
    else if (v.Op == OpMIPSMOVHUload) 
        return rewriteValueMIPS_OpMIPSMOVHUload(_addr_v);
    else if (v.Op == OpMIPSMOVHUreg) 
        return rewriteValueMIPS_OpMIPSMOVHUreg(_addr_v);
    else if (v.Op == OpMIPSMOVHload) 
        return rewriteValueMIPS_OpMIPSMOVHload(_addr_v);
    else if (v.Op == OpMIPSMOVHreg) 
        return rewriteValueMIPS_OpMIPSMOVHreg(_addr_v);
    else if (v.Op == OpMIPSMOVHstore) 
        return rewriteValueMIPS_OpMIPSMOVHstore(_addr_v);
    else if (v.Op == OpMIPSMOVHstorezero) 
        return rewriteValueMIPS_OpMIPSMOVHstorezero(_addr_v);
    else if (v.Op == OpMIPSMOVWload) 
        return rewriteValueMIPS_OpMIPSMOVWload(_addr_v);
    else if (v.Op == OpMIPSMOVWnop) 
        return rewriteValueMIPS_OpMIPSMOVWnop(_addr_v);
    else if (v.Op == OpMIPSMOVWreg) 
        return rewriteValueMIPS_OpMIPSMOVWreg(_addr_v);
    else if (v.Op == OpMIPSMOVWstore) 
        return rewriteValueMIPS_OpMIPSMOVWstore(_addr_v);
    else if (v.Op == OpMIPSMOVWstorezero) 
        return rewriteValueMIPS_OpMIPSMOVWstorezero(_addr_v);
    else if (v.Op == OpMIPSMUL) 
        return rewriteValueMIPS_OpMIPSMUL(_addr_v);
    else if (v.Op == OpMIPSNEG) 
        return rewriteValueMIPS_OpMIPSNEG(_addr_v);
    else if (v.Op == OpMIPSNOR) 
        return rewriteValueMIPS_OpMIPSNOR(_addr_v);
    else if (v.Op == OpMIPSNORconst) 
        return rewriteValueMIPS_OpMIPSNORconst(_addr_v);
    else if (v.Op == OpMIPSOR) 
        return rewriteValueMIPS_OpMIPSOR(_addr_v);
    else if (v.Op == OpMIPSORconst) 
        return rewriteValueMIPS_OpMIPSORconst(_addr_v);
    else if (v.Op == OpMIPSSGT) 
        return rewriteValueMIPS_OpMIPSSGT(_addr_v);
    else if (v.Op == OpMIPSSGTU) 
        return rewriteValueMIPS_OpMIPSSGTU(_addr_v);
    else if (v.Op == OpMIPSSGTUconst) 
        return rewriteValueMIPS_OpMIPSSGTUconst(_addr_v);
    else if (v.Op == OpMIPSSGTUzero) 
        return rewriteValueMIPS_OpMIPSSGTUzero(_addr_v);
    else if (v.Op == OpMIPSSGTconst) 
        return rewriteValueMIPS_OpMIPSSGTconst(_addr_v);
    else if (v.Op == OpMIPSSGTzero) 
        return rewriteValueMIPS_OpMIPSSGTzero(_addr_v);
    else if (v.Op == OpMIPSSLL) 
        return rewriteValueMIPS_OpMIPSSLL(_addr_v);
    else if (v.Op == OpMIPSSLLconst) 
        return rewriteValueMIPS_OpMIPSSLLconst(_addr_v);
    else if (v.Op == OpMIPSSRA) 
        return rewriteValueMIPS_OpMIPSSRA(_addr_v);
    else if (v.Op == OpMIPSSRAconst) 
        return rewriteValueMIPS_OpMIPSSRAconst(_addr_v);
    else if (v.Op == OpMIPSSRL) 
        return rewriteValueMIPS_OpMIPSSRL(_addr_v);
    else if (v.Op == OpMIPSSRLconst) 
        return rewriteValueMIPS_OpMIPSSRLconst(_addr_v);
    else if (v.Op == OpMIPSSUB) 
        return rewriteValueMIPS_OpMIPSSUB(_addr_v);
    else if (v.Op == OpMIPSSUBconst) 
        return rewriteValueMIPS_OpMIPSSUBconst(_addr_v);
    else if (v.Op == OpMIPSXOR) 
        return rewriteValueMIPS_OpMIPSXOR(_addr_v);
    else if (v.Op == OpMIPSXORconst) 
        return rewriteValueMIPS_OpMIPSXORconst(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValueMIPS_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValueMIPS_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValueMIPS_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValueMIPS_OpMod32u(_addr_v);
    else if (v.Op == OpMod8) 
        return rewriteValueMIPS_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValueMIPS_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValueMIPS_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = OpMIPSMUL;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = OpMIPSMUL;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = OpMIPSMULF;
        return true;
    else if (v.Op == OpMul32uhilo) 
        v.Op = OpMIPSMULTU;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = OpMIPSMULD;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = OpMIPSMUL;
        return true;
    else if (v.Op == OpNeg16) 
        v.Op = OpMIPSNEG;
        return true;
    else if (v.Op == OpNeg32) 
        v.Op = OpMIPSNEG;
        return true;
    else if (v.Op == OpNeg32F) 
        v.Op = OpMIPSNEGF;
        return true;
    else if (v.Op == OpNeg64F) 
        v.Op = OpMIPSNEGD;
        return true;
    else if (v.Op == OpNeg8) 
        v.Op = OpMIPSNEG;
        return true;
    else if (v.Op == OpNeq16) 
        return rewriteValueMIPS_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValueMIPS_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValueMIPS_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValueMIPS_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValueMIPS_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        v.Op = OpMIPSXOR;
        return true;
    else if (v.Op == OpNeqPtr) 
        return rewriteValueMIPS_OpNeqPtr(_addr_v);
    else if (v.Op == OpNilCheck) 
        v.Op = OpMIPSLoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        return rewriteValueMIPS_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValueMIPS_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        v.Op = OpMIPSOR;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = OpMIPSOR;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = OpMIPSOR;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = OpMIPSOR;
        return true;
    else if (v.Op == OpPanicBounds) 
        return rewriteValueMIPS_OpPanicBounds(_addr_v);
    else if (v.Op == OpPanicExtend) 
        return rewriteValueMIPS_OpPanicExtend(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValueMIPS_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        return rewriteValueMIPS_OpRotateLeft32(_addr_v);
    else if (v.Op == OpRotateLeft64) 
        return rewriteValueMIPS_OpRotateLeft64(_addr_v);
    else if (v.Op == OpRotateLeft8) 
        return rewriteValueMIPS_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound32F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValueMIPS_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValueMIPS_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValueMIPS_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValueMIPS_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValueMIPS_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValueMIPS_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValueMIPS_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValueMIPS_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValueMIPS_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValueMIPS_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValueMIPS_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValueMIPS_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValueMIPS_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValueMIPS_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValueMIPS_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValueMIPS_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValueMIPS_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValueMIPS_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValueMIPS_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValueMIPS_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValueMIPS_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValueMIPS_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValueMIPS_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValueMIPS_OpRsh8x8(_addr_v);
    else if (v.Op == OpSelect0) 
        return rewriteValueMIPS_OpSelect0(_addr_v);
    else if (v.Op == OpSelect1) 
        return rewriteValueMIPS_OpSelect1(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        v.Op = OpMIPSMOVHreg;
        return true;
    else if (v.Op == OpSignExt8to16) 
        v.Op = OpMIPSMOVBreg;
        return true;
    else if (v.Op == OpSignExt8to32) 
        v.Op = OpMIPSMOVBreg;
        return true;
    else if (v.Op == OpSignmask) 
        return rewriteValueMIPS_OpSignmask(_addr_v);
    else if (v.Op == OpSlicemask) 
        return rewriteValueMIPS_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = OpMIPSSQRTD;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = OpMIPSSQRTF;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = OpMIPSCALLstatic;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValueMIPS_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = OpMIPSSUB;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = OpMIPSSUB;
        return true;
    else if (v.Op == OpSub32F) 
        v.Op = OpMIPSSUBF;
        return true;
    else if (v.Op == OpSub32withcarry) 
        return rewriteValueMIPS_OpSub32withcarry(_addr_v);
    else if (v.Op == OpSub64F) 
        v.Op = OpMIPSSUBD;
        return true;
    else if (v.Op == OpSub8) 
        v.Op = OpMIPSSUB;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = OpMIPSSUB;
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
    else if (v.Op == OpWB) 
        v.Op = OpMIPSLoweredWB;
        return true;
    else if (v.Op == OpXor16) 
        v.Op = OpMIPSXOR;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = OpMIPSXOR;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = OpMIPSXOR;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValueMIPS_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        v.Op = OpMIPSMOVHUreg;
        return true;
    else if (v.Op == OpZeroExt8to16) 
        v.Op = OpMIPSMOVBUreg;
        return true;
    else if (v.Op == OpZeroExt8to32) 
        v.Op = OpMIPSMOVBUreg;
        return true;
    else if (v.Op == OpZeromask) 
        return rewriteValueMIPS_OpZeromask(_addr_v);
        return false;

}
private static bool rewriteValueMIPS_OpAdd32withcarry(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Add32withcarry <t> x y c)
    // result: (ADD c (ADD <t> x y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        var c = v_2;
        v.reset(OpMIPSADD);
        var v0 = b.NewValue0(v.Pos, OpMIPSADD, t);
        v0.AddArg2(x, y);
        v.AddArg2(c, v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (MOVWaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpMIPSMOVWaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }

}
private static bool rewriteValueMIPS_OpAtomicAnd8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicAnd8 ptr val mem)
    // cond: !config.BigEndian
    // result: (LoweredAtomicAnd (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr) (OR <typ.UInt32> (SLL <typ.UInt32> (ZeroExt8to32 val) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] ptr))) (NORconst [0] <typ.UInt32> (SLL <typ.UInt32> (MOVWconst [0xff]) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] ptr))))) mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(!config.BigEndian)) {
            break;
        }
        v.reset(OpMIPSLoweredAtomicAnd);
        var v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(~3);
        v0.AddArg2(v1, ptr);
        var v2 = b.NewValue0(v.Pos, OpMIPSOR, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v4.AddArg(val);
        var v5 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v5.AuxInt = int32ToAuxInt(3);
        var v6 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
        v6.AuxInt = int32ToAuxInt(3);
        v6.AddArg(ptr);
        v5.AddArg(v6);
        v3.AddArg2(v4, v5);
        var v7 = b.NewValue0(v.Pos, OpMIPSNORconst, typ.UInt32);
        v7.AuxInt = int32ToAuxInt(0);
        var v8 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        var v9 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v9.AuxInt = int32ToAuxInt(0xff);
        v8.AddArg2(v9, v5);
        v7.AddArg(v8);
        v2.AddArg2(v3, v7);
        v.AddArg3(v0, v2, mem);
        return true;

    } 
    // match: (AtomicAnd8 ptr val mem)
    // cond: config.BigEndian
    // result: (LoweredAtomicAnd (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr) (OR <typ.UInt32> (SLL <typ.UInt32> (ZeroExt8to32 val) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] (XORconst <typ.UInt32> [3] ptr)))) (NORconst [0] <typ.UInt32> (SLL <typ.UInt32> (MOVWconst [0xff]) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] (XORconst <typ.UInt32> [3] ptr)))))) mem)
    while (true) {
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(config.BigEndian)) {
            break;
        }
        v.reset(OpMIPSLoweredAtomicAnd);
        v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(~3);
        v0.AddArg2(v1, ptr);
        v2 = b.NewValue0(v.Pos, OpMIPSOR, typ.UInt32);
        v3 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v4.AddArg(val);
        v5 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v5.AuxInt = int32ToAuxInt(3);
        v6 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
        v6.AuxInt = int32ToAuxInt(3);
        v7 = b.NewValue0(v.Pos, OpMIPSXORconst, typ.UInt32);
        v7.AuxInt = int32ToAuxInt(3);
        v7.AddArg(ptr);
        v6.AddArg(v7);
        v5.AddArg(v6);
        v3.AddArg2(v4, v5);
        v8 = b.NewValue0(v.Pos, OpMIPSNORconst, typ.UInt32);
        v8.AuxInt = int32ToAuxInt(0);
        v9 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        var v10 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v10.AuxInt = int32ToAuxInt(0xff);
        v9.AddArg2(v10, v5);
        v8.AddArg(v9);
        v2.AddArg2(v3, v8);
        v.AddArg3(v0, v2, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpAtomicOr8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (AtomicOr8 ptr val mem)
    // cond: !config.BigEndian
    // result: (LoweredAtomicOr (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr) (SLL <typ.UInt32> (ZeroExt8to32 val) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] ptr))) mem)
    while (true) {
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(!config.BigEndian)) {
            break;
        }
        v.reset(OpMIPSLoweredAtomicOr);
        var v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(~3);
        v0.AddArg2(v1, ptr);
        var v2 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v3.AddArg(val);
        var v4 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v4.AuxInt = int32ToAuxInt(3);
        var v5 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
        v5.AuxInt = int32ToAuxInt(3);
        v5.AddArg(ptr);
        v4.AddArg(v5);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v2, mem);
        return true;

    } 
    // match: (AtomicOr8 ptr val mem)
    // cond: config.BigEndian
    // result: (LoweredAtomicOr (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr) (SLL <typ.UInt32> (ZeroExt8to32 val) (SLLconst <typ.UInt32> [3] (ANDconst <typ.UInt32> [3] (XORconst <typ.UInt32> [3] ptr)))) mem)
    while (true) {
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(config.BigEndian)) {
            break;
        }
        v.reset(OpMIPSLoweredAtomicOr);
        v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(~3);
        v0.AddArg2(v1, ptr);
        v2 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
        v3 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v3.AddArg(val);
        v4 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v4.AuxInt = int32ToAuxInt(3);
        v5 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
        v5.AuxInt = int32ToAuxInt(3);
        var v6 = b.NewValue0(v.Pos, OpMIPSXORconst, typ.UInt32);
        v6.AuxInt = int32ToAuxInt(3);
        v6.AddArg(ptr);
        v5.AddArg(v6);
        v4.AddArg(v5);
        v2.AddArg2(v3, v4);
        v.AddArg3(v0, v2, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpAvg32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Avg32u <t> x y)
    // result: (ADD (SRLconst <t> (SUB <t> x y) [1]) y)
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSADD);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRLconst, t);
        v0.AuxInt = int32ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpMIPSSUB, t);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg2(v0, y);
        return true;
    }

}
private static bool rewriteValueMIPS_OpBitLen32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (BitLen32 <t> x)
    // result: (SUB (MOVWconst [32]) (CLZ <t> x))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpMIPSSUB);
        var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(32);
        var v1 = b.NewValue0(v.Pos, OpMIPSCLZ, t);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpCom16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com16 x)
    // result: (NORconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpMIPSNORconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpCom32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com32 x)
    // result: (NORconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpMIPSNORconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpCom8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Com8 x)
    // result: (NORconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpMIPSNORconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt16(v.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueMIPS_OpConst32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt32(v.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueMIPS_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt8(v.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueMIPS_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [t])
    // result: (MOVWconst [b2i32(t)])
    while (true) {
        var t = auxIntToBool(v.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(t));
        return true;
    }

}
private static bool rewriteValueMIPS_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (MOVWconst [0])
    while (true) {
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz32 <t> x)
    // result: (SUB (MOVWconst [32]) (CLZ <t> (SUBconst <t> [1] (AND <t> x (NEG <t> x)))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpMIPSSUB);
        var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(32);
        var v1 = b.NewValue0(v.Pos, OpMIPSCLZ, t);
        var v2 = b.NewValue0(v.Pos, OpMIPSSUBconst, t);
        v2.AuxInt = int32ToAuxInt(1);
        var v3 = b.NewValue0(v.Pos, OpMIPSAND, t);
        var v4 = b.NewValue0(v.Pos, OpMIPSNEG, t);
        v4.AddArg(x);
        v3.AddArg2(x, v4);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 x y)
    // result: (Select1 (DIV (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u x y)
    // result: (Select1 (DIVU (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32 x y)
    // result: (Select1 (DIV x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32u x y)
    // result: (Select1 (DIVU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (Select1 (DIV (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (Select1 (DIVU (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x y)
    // result: (SGTUconst [1] (XOR (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTUconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq32 x y)
    // result: (SGTUconst [1] (XOR x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTUconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32F x y)
    // result: (FPFlagTrue (CMPEQF x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQF, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq64F x y)
    // result: (FPFlagTrue (CMPEQD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x y)
    // result: (SGTUconst [1] (XOR (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTUconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqB x y)
    // result: (XORconst [1] (XOR <typ.Bool> x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.Bool);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqPtr x y)
    // result: (SGTUconst [1] (XOR x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTUconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpHmul32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Hmul32 x y)
    // result: (Select0 (MULT x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSMULT, types.NewTuple(typ.Int32, typ.Int32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpHmul32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Hmul32u x y)
    // result: (Select0 (MULTU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSMULTU, types.NewTuple(typ.UInt32, typ.UInt32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (IsInBounds idx len)
    // result: (SGTU len idx)
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpMIPSSGTU);
        v.AddArg2(len, idx);
        return true;
    }

}
private static bool rewriteValueMIPS_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsNonNil ptr)
    // result: (SGTU ptr (MOVWconst [0]))
    while (true) {
        var ptr = v_0;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg2(ptr, v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (IsSliceInBounds idx len)
    // result: (XORconst [1] (SGTU idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16 x y)
    // result: (XORconst [1] (SGT (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16U x y)
    // result: (XORconst [1] (SGTU (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32 x y)
    // result: (XORconst [1] (SGT x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32F x y)
    // result: (FPFlagTrue (CMPGEF y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPGEF, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq32U x y)
    // result: (XORconst [1] (SGTU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64F x y)
    // result: (FPFlagTrue (CMPGED y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPGED, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8 x y)
    // result: (XORconst [1] (SGT (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8U x y)
    // result: (XORconst [1] (SGTU (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16 x y)
    // result: (SGT (SignExt16to32 y) (SignExt16to32 x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGT);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(y);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16U x y)
    // result: (SGTU (ZeroExt16to32 y) (ZeroExt16to32 x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(y);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less32 x y)
    // result: (SGT y x)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGT);
        v.AddArg2(y, x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32F x y)
    // result: (FPFlagTrue (CMPGTF y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTF, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Less32U x y)
    // result: (SGTU y x)
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        v.AddArg2(y, x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64F x y)
    // result: (FPFlagTrue (CMPGTD y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagTrue);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTD, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8 x y)
    // result: (SGT (SignExt8to32 y) (SignExt8to32 x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGT);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(y);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8U x y)
    // result: (SGTU (ZeroExt8to32 y) (ZeroExt8to32 x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Load <t> ptr mem)
    // cond: t.IsBoolean()
    // result: (MOVBUload ptr mem)
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(t.IsBoolean())) {
            break;
        }
        v.reset(OpMIPSMOVBUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is8BitInt(t) && isSigned(t))
    // result: (MOVBload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpMIPSMOVBload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is8BitInt(t) && !isSigned(t))
    // result: (MOVBUload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpMIPSMOVBUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is16BitInt(t) && isSigned(t))
    // result: (MOVHload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpMIPSMOVHload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is16BitInt(t) && !isSigned(t))
    // result: (MOVHUload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpMIPSMOVHUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is32BitInt(t) || isPtr(t))
    // result: (MOVWload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitInt(t) || isPtr(t))) {
            break;
        }
        v.reset(OpMIPSMOVWload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitFloat(t)
    // result: (MOVFload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(OpMIPSMOVFload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitFloat(t)
    // result: (MOVDload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(OpMIPSMOVDload);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (MOVWaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpMIPSMOVWaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x16 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x32 <t> x y)
    // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh16x64 x (Const64 [c]))
    // cond: uint32(c) < 16
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpMIPSSLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Lsh16x64 _ (Const64 [c]))
    // cond: uint32(c) >= 16
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 16)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x8 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x16 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x32 <t> x y)
    // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh32x64 x (Const64 [c]))
    // cond: uint32(c) < 32
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpMIPSSLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Lsh32x64 _ (Const64 [c]))
    // cond: uint32(c) >= 32
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 32)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x8 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x16 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x32 <t> x y)
    // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueMIPS_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh8x64 x (Const64 [c]))
    // cond: uint32(c) < 8
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpMIPSSLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Lsh8x64 _ (Const64 [c]))
    // cond: uint32(c) >= 8
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 8)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x8 <t> x y)
    // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMIPSADD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADD x (MOVWconst [c]))
    // result: (ADDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSADDconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
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
                if (v_1.Op != OpMIPSNEG) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = v_1.Args[0];
                v.reset(OpMIPSSUB);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSADDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDconst [off1] (MOVWaddr [off2] {sym} ptr))
    // result: (MOVWaddr [off1+off2] {sym} ptr)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var ptr = v_0.Args[0];
        v.reset(OpMIPSMOVWaddr);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg(ptr);
        return true;

    } 
    // match: (ADDconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (ADDconst [c] (MOVWconst [d]))
    // result: (MOVWconst [int32(c+d)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(c + d));
        return true;

    } 
    // match: (ADDconst [c] (ADDconst [d] x))
    // result: (ADDconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSADDconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDconst [c] (SUBconst [d] x))
    // result: (ADDconst [c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSADDconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSAND(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (AND x (MOVWconst [c]))
    // result: (ANDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSANDconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
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
    // match: (AND (SGTUconst [1] x) (SGTUconst [1] y))
    // result: (SGTUconst [1] (OR <x.Type> x y))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSSGTUconst || auxIntToInt32(v_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[0];
                if (v_1.Op != OpMIPSSGTUconst || auxIntToInt32(v_1.AuxInt) != 1) {
                    continue;
                }

                var y = v_1.Args[0];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = int32ToAuxInt(1);
                var v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSANDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDconst [0] _)
    // result: (MOVWconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (ANDconst [-1] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -1) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (ANDconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c&d])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c & d);
        return true;

    } 
    // match: (ANDconst [c] (ANDconst [d] x))
    // result: (ANDconst [c&d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSANDconst);
        v.AuxInt = int32ToAuxInt(c & d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSCMOVZ(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMOVZ _ f (MOVWconst [0]))
    // result: f
    while (true) {
        var f = v_1;
        if (v_2.Op != OpMIPSMOVWconst || auxIntToInt32(v_2.AuxInt) != 0) {
            break;
        }
        v.copyOf(f);
        return true;

    } 
    // match: (CMOVZ a _ (MOVWconst [c]))
    // cond: c!=0
    // result: a
    while (true) {
        var a = v_0;
        if (v_2.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_2.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.copyOf(a);
        return true;

    } 
    // match: (CMOVZ a (MOVWconst [0]) c)
    // result: (CMOVZzero a c)
    while (true) {
        a = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        c = v_2;
        v.reset(OpMIPSCMOVZzero);
        v.AddArg2(a, c);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSCMOVZzero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMOVZzero _ (MOVWconst [0]))
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (CMOVZzero a (MOVWconst [c]))
    // cond: c!=0
    // result: a
    while (true) {
        var a = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(c != 0)) {
            break;
        }
        v.copyOf(a);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSLoweredAtomicAdd(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LoweredAtomicAdd ptr (MOVWconst [c]) mem)
    // cond: is16Bit(int64(c))
    // result: (LoweredAtomicAddconst [c] ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        if (!(is16Bit(int64(c)))) {
            break;
        }
        v.reset(OpMIPSLoweredAtomicAddconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSLoweredAtomicStore32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LoweredAtomicStore32 ptr (MOVWconst [0]) mem)
    // result: (LoweredAtomicStorezero ptr mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        var mem = v_2;
        v.reset(OpMIPSLoweredAtomicStorezero);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBUload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBUload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVBUload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVBUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVBUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBUreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVBstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpMIPSMOVBUreg);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBUreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVBUreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpMIPSMOVBUload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBUreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg <t> x:(MOVBload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBUload <t> [off] {sym} ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpMIPSMOVBload) {
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
        var v0 = b.NewValue0(x.Pos, OpMIPSMOVBUload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUreg (ANDconst [c] x))
    // result: (ANDconst [c&0xff] x)
    while (true) {
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSANDconst);
        v.AuxInt = int32ToAuxInt(c & 0xff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg (MOVWconst [c]))
    // result: (MOVWconst [int32(uint8(c))])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint8(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVBload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVBstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpMIPSMOVBreg);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVBreg x:(MOVBload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpMIPSMOVBload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg x:(MOVBreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg <t> x:(MOVBUload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBload <t> [off] {sym} ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpMIPSMOVBUload) {
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
        var v0 = b.NewValue0(x.Pos, OpMIPSMOVBload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBreg (ANDconst [c] x))
    // cond: c & 0x80 == 0
    // result: (ANDconst [c&0x7f] x)
    while (true) {
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x80 == 0)) {
            break;
        }
        v.reset(OpMIPSANDconst);
        v.AuxInt = int32ToAuxInt(c & 0x7f);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg (MOVWconst [c]))
    // result: (MOVWconst [int32(int8(c))])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(int8(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVBstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVWconst [0]) mem)
    // result: (MOVBstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpMIPSMOVBstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVBreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVBUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
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
        if (v_1.Op != OpMIPSMOVHreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVHUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
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
        if (v_1.Op != OpMIPSMOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVBstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVBstorezero [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVBstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVBstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVDload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVDload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVDload [off] {sym} ptr (MOVDstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVDstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVDstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVDstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVFload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVFload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVFload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVFload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVFload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVFload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVFload [off] {sym} ptr (MOVFstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVFstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVFstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVFstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVFstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVFstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVFstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVFstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHUload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHUload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVHUload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVHUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVHUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVHUreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVHstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpMIPSMOVHUreg);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHUreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVHUreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpMIPSMOVBUload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVHUload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVHUload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBUreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVHUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVHUreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg <t> x:(MOVHload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVHUload <t> [off] {sym} ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpMIPSMOVHload) {
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
        var v0 = b.NewValue0(x.Pos, OpMIPSMOVHUload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUreg (ANDconst [c] x))
    // result: (ANDconst [c&0xffff] x)
    while (true) {
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSANDconst);
        v.AuxInt = int32ToAuxInt(c & 0xffff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg (MOVWconst [c]))
    // result: (MOVWconst [int32(uint16(c))])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint16(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVHload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVHreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVHstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpMIPSMOVHreg);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVHreg x:(MOVBload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpMIPSMOVBload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBUload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVHload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVHload) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVBUreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVHreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpMIPSMOVHreg) {
            break;
        }
        v.reset(OpMIPSMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg <t> x:(MOVHUload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVHload <t> [off] {sym} ptr mem)
    while (true) {
        var t = v.Type;
        x = v_0;
        if (x.Op != OpMIPSMOVHUload) {
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
        var v0 = b.NewValue0(x.Pos, OpMIPSMOVHload, t);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHreg (ANDconst [c] x))
    // cond: c & 0x8000 == 0
    // result: (ANDconst [c&0x7fff] x)
    while (true) {
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x8000 == 0)) {
            break;
        }
        v.reset(OpMIPSANDconst);
        v.AuxInt = int32ToAuxInt(c & 0x7fff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg (MOVWconst [c]))
    // result: (MOVWconst [int32(int16(c))])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(int16(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVHstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstore [off] {sym} ptr (MOVWconst [0]) mem)
    // result: (MOVHstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpMIPSMOVHstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVHreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVHUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVHstore);
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
        if (v_1.Op != OpMIPSMOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVHstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVHstorezero [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVHstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVHstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWload [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVWload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVWstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVWnop(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVWnop (MOVWconst [c]))
    // result: (MOVWconst [c])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVWreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVWreg x)
    // cond: x.Uses == 1
    // result: (MOVWnop x)
    while (true) {
        var x = v_0;
        if (!(x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVWnop);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVWreg (MOVWconst [c]))
    // result: (MOVWconst [c])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVWstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVWstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstore [off] {sym} ptr (MOVWconst [0]) mem)
    // result: (MOVWstorezero [off] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        mem = v_2;
        v.reset(OpMIPSMOVWstorezero);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpMIPSMOVWreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMOVWstorezero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
    // cond: (is16Bit(int64(off1+off2)) || x.Uses == 1)
    // result: (MOVWstorezero [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var x = v_0;
        if (x.Op != OpMIPSADDconst) {
            break;
        }
        var off2 = auxIntToInt32(x.AuxInt);
        var ptr = x.Args[0];
        var mem = v_1;
        if (!(is16Bit(int64(off1 + off2)) || x.Uses == 1)) {
            break;
        }
        v.reset(OpMIPSMOVWstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpMIPSMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpMIPSMOVWstorezero);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSMUL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MUL (MOVWconst [0]) _ )
    // result: (MOVWconst [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL (MOVWconst [1]) x )
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0.AuxInt) != 1) {
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
    // match: (MUL (MOVWconst [-1]) x )
    // result: (NEG x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_1;
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL (MOVWconst [c]) x )
    // cond: isPowerOfTwo64(int64(uint32(c)))
    // result: (SLLconst [int32(log2uint32(int64(c)))] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                x = v_1;
                if (!(isPowerOfTwo64(int64(uint32(c))))) {
                    continue;
                }

                v.reset(OpMIPSSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log2uint32(int64(c))));
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL (MOVWconst [c]) (MOVWconst [d]))
    // result: (MOVWconst [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSNEG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEG (MOVWconst [c]))
    // result: (MOVWconst [-c])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(-c);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSNOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NOR x (MOVWconst [c]))
    // result: (NORconst [c] x)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSNORconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSNORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NORconst [c] (MOVWconst [d]))
    // result: (MOVWconst [^(c|d)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(~(c | d));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (OR x (MOVWconst [c]))
    // result: (ORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSORconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
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
    // match: (OR (SGTUzero x) (SGTUzero y))
    // result: (SGTUzero (OR <x.Type> x y))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpMIPSSGTUzero) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                x = v_0.Args[0];
                if (v_1.Op != OpMIPSSGTUzero) {
                    continue;
                }

                var y = v_1.Args[0];
                v.reset(OpMIPSSGTUzero);
                var v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (ORconst [-1] _)
    // result: (MOVWconst [-1])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -1) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(-1);
        return true;

    } 
    // match: (ORconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c|d])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c | d);
        return true;

    } 
    // match: (ORconst [c] (ORconst [d] x))
    // result: (ORconst [c|d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSORconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSORconst);
        v.AuxInt = int32ToAuxInt(c | d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGT(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SGT (MOVWconst [c]) x)
    // result: (SGTconst [c] x)
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpMIPSSGTconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SGT x (MOVWconst [0]))
    // result: (SGTzero x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpMIPSSGTzero);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGTU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SGTU (MOVWconst [c]) x)
    // result: (SGTUconst [c] x)
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpMIPSSGTUconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SGTU x (MOVWconst [0]))
    // result: (SGTUzero x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpMIPSMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpMIPSSGTUzero);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGTUconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SGTUconst [c] (MOVWconst [d]))
    // cond: uint32(c) > uint32(d)
    // result: (MOVWconst [1])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        if (!(uint32(c) > uint32(d))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTUconst [c] (MOVWconst [d]))
    // cond: uint32(c) <= uint32(d)
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(uint32(c) <= uint32(d))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTUconst [c] (MOVBUreg _))
    // cond: 0xff < uint32(c)
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVBUreg || !(0xff < uint32(c))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTUconst [c] (MOVHUreg _))
    // cond: 0xffff < uint32(c)
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVHUreg || !(0xffff < uint32(c))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTUconst [c] (ANDconst [m] _))
    // cond: uint32(m) < uint32(c)
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(uint32(m) < uint32(c))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTUconst [c] (SRLconst _ [d]))
    // cond: uint32(d) <= 31 && 0xffffffff>>uint32(d) < uint32(c)
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSSRLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(uint32(d) <= 31 && 0xffffffff >> (int)(uint32(d)) < uint32(c))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGTUzero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SGTUzero (MOVWconst [d]))
    // cond: d != 0
    // result: (MOVWconst [1])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTUzero (MOVWconst [d]))
    // cond: d == 0
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(d == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGTconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SGTconst [c] (MOVWconst [d]))
    // cond: c > d
    // result: (MOVWconst [1])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        if (!(c > d)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (MOVWconst [d]))
    // cond: c <= d
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(c <= d)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTconst [c] (MOVBreg _))
    // cond: 0x7f < c
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVBreg || !(0x7f < c)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (MOVBreg _))
    // cond: c <= -0x80
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVBreg || !(c <= -0x80)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTconst [c] (MOVBUreg _))
    // cond: 0xff < c
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVBUreg || !(0xff < c)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (MOVBUreg _))
    // cond: c < 0
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVBUreg || !(c < 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTconst [c] (MOVHreg _))
    // cond: 0x7fff < c
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVHreg || !(0x7fff < c)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (MOVHreg _))
    // cond: c <= -0x8000
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVHreg || !(c <= -0x8000)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTconst [c] (MOVHUreg _))
    // cond: 0xffff < c
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVHUreg || !(0xffff < c)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (MOVHUreg _))
    // cond: c < 0
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVHUreg || !(c < 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SGTconst [c] (ANDconst [m] _))
    // cond: 0 <= m && m < c
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSANDconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= m && m < c)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTconst [c] (SRLconst _ [d]))
    // cond: 0 <= c && uint32(d) <= 31 && 0xffffffff>>uint32(d) < uint32(c)
    // result: (MOVWconst [1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSSRLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= c && uint32(d) <= 31 && 0xffffffff >> (int)(uint32(d)) < uint32(c))) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSGTzero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SGTzero (MOVWconst [d]))
    // cond: d > 0
    // result: (MOVWconst [1])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        if (!(d > 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;

    } 
    // match: (SGTzero (MOVWconst [d]))
    // cond: d <= 0
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        if (!(d <= 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SLL x (MOVWconst [c]))
    // result: (SLLconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpMIPSSLLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSLLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLLconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d<<uint32(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(d << (int)(uint32(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRA x (MOVWconst [c]))
    // result: (SRAconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSRAconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRAconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d>>uint32(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(d >> (int)(uint32(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRL x (MOVWconst [c]))
    // result: (SRLconst x [c&31])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpMIPSSRLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSRLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRLconst [c] (MOVWconst [d]))
    // result: (MOVWconst [int32(uint32(d)>>uint32(c))])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(d) >> (int)(uint32(c))));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSUB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUB x (MOVWconst [c]))
    // result: (SUBconst [c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpMIPSSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUB x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SUB (MOVWconst [0]) x)
    // result: (NEG x)
    while (true) {
        if (v_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        x = v_1;
        v.reset(OpMIPSNEG);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSSUBconst(ptr<Value> _addr_v) {
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
    // match: (SUBconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d-c])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(d - c);
        return true;

    } 
    // match: (SUBconst [c] (SUBconst [d] x))
    // result: (ADDconst [-c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSADDconst);
        v.AuxInt = int32ToAuxInt(-c - d);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBconst [c] (ADDconst [d] x))
    // result: (ADDconst [-c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSADDconst);
        v.AuxInt = int32ToAuxInt(-c + d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSXOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XOR x (MOVWconst [c]))
    // result: (XORconst [c] x)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpMIPSXORconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

        }
        break;

    } 
    // match: (XOR x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMIPSXORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (XORconst [-1] x)
    // result: (NORconst [0] x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -1) {
            break;
        }
        x = v_0;
        v.reset(OpMIPSNORconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;

    } 
    // match: (XORconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c^d])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        return true;

    } 
    // match: (XORconst [c] (XORconst [d] x))
    // result: (XORconst [c^d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpMIPSXORconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16 x y)
    // result: (Select0 (DIV (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16u x y)
    // result: (Select0 (DIVU (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32 x y)
    // result: (Select0 (DIV x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32u x y)
    // result: (Select0 (DIVU x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (Select0 (DIV (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (Select0 (DIVU (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpMove(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
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
    // result: (MOVBstore dst (MOVBUload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var dst = v_0;
        var src = v_1;
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        var v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [2] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore dst (MOVHUload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        var t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [2] dst src mem)
    // result: (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(1);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(1);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [4] {t} dst src mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore dst (MOVWload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [4] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [2] dst (MOVHUload [2] src mem) (MOVHstore dst (MOVHUload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [4] dst src mem)
    // result: (MOVBstore [3] dst (MOVBUload [3] src mem) (MOVBstore [2] dst (MOVBUload [2] src mem) (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(3);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v2.AuxInt = int32ToAuxInt(2);
        v2.AddArg2(src, mem);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(1);
        var v4 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v4.AuxInt = int32ToAuxInt(1);
        v4.AddArg2(src, mem);
        var v5 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        var v6 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v6.AddArg2(src, mem);
        v5.AddArg3(dst, v6, mem);
        v3.AddArg3(dst, v4, v5);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [3] dst src mem)
    // result: (MOVBstore [2] dst (MOVBUload [2] src mem) (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(1);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v2.AuxInt = int32ToAuxInt(1);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v4 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [8] {t} dst src mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [8] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [6] dst (MOVHload [6] src mem) (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(6);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v0.AuxInt = int32ToAuxInt(6);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(4);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v2.AuxInt = int32ToAuxInt(4);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(2);
        v4 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v4.AuxInt = int32ToAuxInt(2);
        v4.AddArg2(src, mem);
        v5 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v6 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v6.AddArg2(src, mem);
        v5.AddArg3(dst, v6, mem);
        v3.AddArg3(dst, v4, v5);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [6] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v2.AuxInt = int32ToAuxInt(2);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v4 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [12] {t} dst src mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [8] dst (MOVWload [8] src mem) (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 12) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(8);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(4);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(4);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v4 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [16] {t} dst src mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [12] dst (MOVWload [12] src mem) (MOVWstore [8] dst (MOVWload [8] src mem) (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(12);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(12);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(8);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(8);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(4);
        v4 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v4.AuxInt = int32ToAuxInt(4);
        v4.AddArg2(src, mem);
        v5 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v6 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
        v6.AddArg2(src, mem);
        v5.AddArg3(dst, v6, mem);
        v3.AddArg3(dst, v4, v5);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [s] {t} dst src mem)
    // cond: (s > 16 && logLargeCopy(v, s) || t.Alignment()%4 != 0)
    // result: (LoweredMove [int32(t.Alignment())] dst src (ADDconst <src.Type> src [int32(s-moveSize(t.Alignment(), config))]) mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 16 && logLargeCopy(v, s) || t.Alignment() % 4 != 0)) {
            break;
        }
        v.reset(OpMIPSLoweredMove);
        v.AuxInt = int32ToAuxInt(int32(t.Alignment()));
        v0 = b.NewValue0(v.Pos, OpMIPSADDconst, src.Type);
        v0.AuxInt = int32ToAuxInt(int32(s - moveSize(t.Alignment(), config)));
        v0.AddArg(src);
        v.AddArg4(dst, src, v0, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x y)
    // result: (SGTU (XOR (ZeroExt16to32 x) (ZeroExt16to32 y)) (MOVWconst [0]))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq32 x y)
    // result: (SGTU (XOR x y) (MOVWconst [0]))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32F x y)
    // result: (FPFlagFalse (CMPEQF x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagFalse);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQF, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq64F x y)
    // result: (FPFlagFalse (CMPEQD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSFPFlagFalse);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x y)
    // result: (SGTU (XOR (ZeroExt8to32 x) (ZeroExt8to32 y)) (MOVWconst [0]))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (NeqPtr x y)
    // result: (SGTU (XOR x y) (MOVWconst [0]))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSGTU);
        var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not x)
    // result: (XORconst [1] x)
    while (true) {
        var x = v_0;
        v.reset(OpMIPSXORconst);
        v.AuxInt = int32ToAuxInt(1);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (OffPtr [off] ptr:(SP))
    // result: (MOVWaddr [int32(off)] ptr)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        if (ptr.Op != OpSP) {
            break;
        }
        v.reset(OpMIPSMOVWaddr);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;

    } 
    // match: (OffPtr [off] ptr)
    // result: (ADDconst [int32(off)] ptr)
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        v.reset(OpMIPSADDconst);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;
    }

}
private static bool rewriteValueMIPS_OpPanicBounds(ptr<Value> _addr_v) {
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
        v.reset(OpMIPSLoweredPanicBoundsA);
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
        v.reset(OpMIPSLoweredPanicBoundsB);
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
        v.reset(OpMIPSLoweredPanicBoundsC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpPanicExtend(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 0
    // result: (LoweredPanicExtendA [kind] hi lo y mem)
    while (true) {
        var kind = auxIntToInt64(v.AuxInt);
        var hi = v_0;
        var lo = v_1;
        var y = v_2;
        var mem = v_3;
        if (!(boundsABI(kind) == 0)) {
            break;
        }
        v.reset(OpMIPSLoweredPanicExtendA);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;

    } 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 1
    // result: (LoweredPanicExtendB [kind] hi lo y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        hi = v_0;
        lo = v_1;
        y = v_2;
        mem = v_3;
        if (!(boundsABI(kind) == 1)) {
            break;
        }
        v.reset(OpMIPSLoweredPanicExtendB);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;

    } 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 2
    // result: (LoweredPanicExtendC [kind] hi lo y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        hi = v_0;
        lo = v_1;
        y = v_2;
        mem = v_3;
        if (!(boundsABI(kind) == 2)) {
            break;
        }
        v.reset(OpMIPSLoweredPanicExtendC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft16 <t> x (MOVWconst [c]))
    // result: (Or16 (Lsh16x32 <t> x (MOVWconst [c&15])) (Rsh16Ux32 <t> x (MOVWconst [-c&15])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr16);
        var v0 = b.NewValue0(v.Pos, OpLsh16x32, t);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 15);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh16Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 15);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRotateLeft32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft32 <t> x (MOVWconst [c]))
    // result: (Or32 (Lsh32x32 <t> x (MOVWconst [c&31])) (Rsh32Ux32 <t> x (MOVWconst [-c&31])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr32);
        var v0 = b.NewValue0(v.Pos, OpLsh32x32, t);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 31);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh32Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 31);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRotateLeft64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft64 <t> x (MOVWconst [c]))
    // result: (Or64 (Lsh64x32 <t> x (MOVWconst [c&63])) (Rsh64Ux32 <t> x (MOVWconst [-c&63])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr64);
        var v0 = b.NewValue0(v.Pos, OpLsh64x32, t);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 63);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh64Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 63);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft8 <t> x (MOVWconst [c]))
    // result: (Or8 (Lsh8x32 <t> x (MOVWconst [c&7])) (Rsh8Ux32 <t> x (MOVWconst [-c&7])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpMIPSMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr8);
        var v0 = b.NewValue0(v.Pos, OpLsh8x32, t);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 7);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh8Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 7);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux16 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v.AddArg3(v0, v3, v4);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux32 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 x (Const64 [c]))
    // cond: uint32(c) < 16
    // result: (SRLconst (SLLconst <typ.UInt32> x [16]) [int32(c+16)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpMIPSSRLconst);
        v.AuxInt = int32ToAuxInt(int32(c + 16));
        var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh16Ux64 _ (Const64 [c]))
    // cond: uint32(c) >= 16
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 16)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux8 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v.AddArg3(v0, v3, v4);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x16 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt16to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(31);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v1.AddArg3(v2, v3, v4);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x32 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> y (MOVWconst [31]) (SGTUconst [32] y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(31);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint32(c) < 16
    // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [int32(c+16)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 16)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c + 16));
        var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint32(c) >= 16
    // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 16)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x8 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt8to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(31);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v1.AddArg3(v2, v3, v4);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux16 <t> x y)
    // result: (CMOVZ (SRL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux32 <t> x y)
    // result: (CMOVZ (SRL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32Ux64 x (Const64 [c]))
    // cond: uint32(c) < 32
    // result: (SRLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpMIPSSRLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Rsh32Ux64 _ (Const64 [c]))
    // cond: uint32(c) >= 32
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 32)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux8 <t> x y)
    // result: (CMOVZ (SRL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x16 x y)
    // result: (SRA x ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt16to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(31);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v0.AddArg3(v1, v2, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x32 x y)
    // result: (SRA x ( CMOVZ <typ.UInt32> y (MOVWconst [31]) (SGTUconst [32] y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(31);
        var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v0.AddArg3(y, v1, v2);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint32(c) < 32
    // result: (SRAconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 32)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint32(c) >= 32
    // result: (SRAconst x [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 32)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x8 x y)
    // result: (SRA x ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt8to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(31);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(v1);
        v0.AddArg3(v1, v2, v3);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux16 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v.AddArg3(v0, v3, v4);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux32 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) y) (MOVWconst [0]) (SGTUconst [32] y))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(0);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(y);
        v.AddArg3(v0, v2, v3);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 x (Const64 [c]))
    // cond: uint32(c) < 8
    // result: (SRLconst (SLLconst <typ.UInt32> x [24]) [int32(c+24)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpMIPSSRLconst);
        v.AuxInt = int32ToAuxInt(int32(c + 24));
        var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh8Ux64 _ (Const64 [c]))
    // cond: uint32(c) >= 8
    // result: (MOVWconst [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 8)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux8 <t> x y)
    // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSCMOVZ);
        var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v.AddArg3(v0, v3, v4);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x16 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt16to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(31);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v1.AddArg3(v2, v3, v4);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x32 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> y (MOVWconst [31]) (SGTUconst [32] y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v2.AuxInt = int32ToAuxInt(31);
        var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(y);
        v1.AddArg3(y, v2, v3);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint32(c) < 8
    // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [int32(c+24)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) < 8)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c + 24));
        var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint32(c) >= 8
    // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint32(c) >= 8)) {
            break;
        }
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x8 x y)
    // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [31]) (SGTUconst [32] (ZeroExt8to32 y))))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMIPSSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(31);
        var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
        v4.AuxInt = int32ToAuxInt(32);
        v4.AddArg(v2);
        v1.AddArg3(v2, v3, v4);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueMIPS_OpSelect0(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select0 (Add32carry <t> x y))
    // result: (ADD <t.FieldType(0)> x y)
    while (true) {
        if (v_0.Op != OpAdd32carry) {
            break;
        }
        var t = v_0.Type;
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpMIPSADD);
        v.Type = t.FieldType(0);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Select0 (Sub32carry <t> x y))
    // result: (SUB <t.FieldType(0)> x y)
    while (true) {
        if (v_0.Op != OpSub32carry) {
            break;
        }
        t = v_0.Type;
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpMIPSSUB);
        v.Type = t.FieldType(0);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (Select0 (MULTU (MOVWconst [0]) _ ))
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select0 (MULTU (MOVWconst [1]) _ ))
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select0 (MULTU (MOVWconst [-1]) x ))
    // result: (CMOVZ (ADDconst <x.Type> [-1] x) (MOVWconst [0]) x)
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSADDconst, x.Type);
                v0.AuxInt = int32ToAuxInt(-1);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = int32ToAuxInt(0);
                v.AddArg3(v0, v1, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select0 (MULTU (MOVWconst [c]) x ))
    // cond: isPowerOfTwo64(int64(uint32(c)))
    // result: (SRLconst [int32(32-log2uint32(int64(c)))] x)
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var c = auxIntToInt32(v_0_0.AuxInt);
                x = v_0_1;
                if (!(isPowerOfTwo64(int64(uint32(c))))) {
                    continue;
                }

                v.reset(OpMIPSSRLconst);
                v.AuxInt = int32ToAuxInt(int32(32 - log2uint32(int64(c))));
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select0 (MULTU (MOVWconst [c]) (MOVWconst [d])))
    // result: (MOVWconst [int32((int64(uint32(c))*int64(uint32(d)))>>32)])
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_0_1.Op != OpMIPSMOVWconst) {
                    continue;
                }

                var d = auxIntToInt32(v_0_1.AuxInt);
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(int32((int64(uint32(c)) * int64(uint32(d))) >> 32));
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select0 (DIV (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [c%d])
    while (true) {
        if (v_0.Op != OpMIPSDIV) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c % d);
        return true;

    } 
    // match: (Select0 (DIVU (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [int32(uint32(c)%uint32(d))])
    while (true) {
        if (v_0.Op != OpMIPSDIVU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) % uint32(d)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpSelect1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select1 (Add32carry <t> x y))
    // result: (SGTU <typ.Bool> x (ADD <t.FieldType(0)> x y))
    while (true) {
        if (v_0.Op != OpAdd32carry) {
            break;
        }
        var t = v_0.Type;
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpMIPSSGTU);
        v.Type = typ.Bool;
        var v0 = b.NewValue0(v.Pos, OpMIPSADD, t.FieldType(0));
        v0.AddArg2(x, y);
        v.AddArg2(x, v0);
        return true;

    } 
    // match: (Select1 (Sub32carry <t> x y))
    // result: (SGTU <typ.Bool> (SUB <t.FieldType(0)> x y) x)
    while (true) {
        if (v_0.Op != OpSub32carry) {
            break;
        }
        t = v_0.Type;
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpMIPSSGTU);
        v.Type = typ.Bool;
        v0 = b.NewValue0(v.Pos, OpMIPSSUB, t.FieldType(0));
        v0.AddArg2(x, y);
        v.AddArg2(v0, x);
        return true;

    } 
    // match: (Select1 (MULTU (MOVWconst [0]) _ ))
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != 0) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select1 (MULTU (MOVWconst [1]) x ))
    // result: x
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != 1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select1 (MULTU (MOVWconst [-1]) x ))
    // result: (NEG <x.Type> x)
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst || auxIntToInt32(v_0_0.AuxInt) != -1) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                x = v_0_1;
                v.reset(OpMIPSNEG);
                v.Type = x.Type;
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select1 (MULTU (MOVWconst [c]) x ))
    // cond: isPowerOfTwo64(int64(uint32(c)))
    // result: (SLLconst [int32(log2uint32(int64(c)))] x)
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                var c = auxIntToInt32(v_0_0.AuxInt);
                x = v_0_1;
                if (!(isPowerOfTwo64(int64(uint32(c))))) {
                    continue;
                }

                v.reset(OpMIPSSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log2uint32(int64(c))));
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select1 (MULTU (MOVWconst [c]) (MOVWconst [d])))
    // result: (MOVWconst [int32(uint32(c)*uint32(d))])
    while (true) {
        if (v_0.Op != OpMIPSMULTU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0_0.Op != OpMIPSMOVWconst) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }

                c = auxIntToInt32(v_0_0.AuxInt);
                if (v_0_1.Op != OpMIPSMOVWconst) {
                    continue;
                }

                var d = auxIntToInt32(v_0_1.AuxInt);
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int32ToAuxInt(int32(uint32(c) * uint32(d)));
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (Select1 (DIV (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [c/d])
    while (true) {
        if (v_0.Op != OpMIPSDIV) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(c / d);
        return true;

    } 
    // match: (Select1 (DIVU (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [int32(uint32(c)/uint32(d))])
    while (true) {
        if (v_0.Op != OpMIPSDIVU) {
            break;
        }
        _ = v_0.Args[1];
        v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpMIPSMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpMIPSMOVWconst) {
            break;
        }
        d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpMIPSMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) / uint32(d)));
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpSignmask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Signmask x)
    // result: (SRAconst x [31])
    while (true) {
        var x = v_0;
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueMIPS_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Slicemask <t> x)
    // result: (SRAconst (NEG <t> x) [31])
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpMIPSSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        var v0 = b.NewValue0(v.Pos, OpMIPSNEG, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueMIPS_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 1
    // result: (MOVBstore ptr val mem)
    while (true) {
        var t = auxToType(v.Aux);
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(t.Size() == 1)) {
            break;
        }
        v.reset(OpMIPSMOVBstore);
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
        v.reset(OpMIPSMOVHstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && !is32BitFloat(val.Type)
    // result: (MOVWstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && !is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && is32BitFloat(val.Type)
    // result: (MOVFstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpMIPSMOVFstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && is64BitFloat(val.Type)
    // result: (MOVDstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8 && is64BitFloat(val.Type))) {
            break;
        }
        v.reset(OpMIPSMOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpSub32withcarry(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Sub32withcarry <t> x y c)
    // result: (SUB (SUB <t> x y) c)
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        var c = v_2;
        v.reset(OpMIPSSUB);
        var v0 = b.NewValue0(v.Pos, OpMIPSSUB, t);
        v0.AddArg2(x, y);
        v.AddArg2(v0, c);
        return true;
    }

}
private static bool rewriteValueMIPS_OpZero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
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
    // match: (Zero [1] ptr mem)
    // result: (MOVBstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var ptr = v_0;
        mem = v_1;
        v.reset(OpMIPSMOVBstore);
        var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [2] {t} ptr mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        var t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [2] ptr mem)
    // result: (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(1);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg3(ptr, v0, mem);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [4] {t} ptr mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [4] {t} ptr mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [2] ptr (MOVWconst [0]) (MOVHstore [0] ptr (MOVWconst [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg3(ptr, v0, mem);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [4] ptr mem)
    // result: (MOVBstore [3] ptr (MOVWconst [0]) (MOVBstore [2] ptr (MOVWconst [0]) (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        var v2 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(1);
        var v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(0);
        v3.AddArg3(ptr, v0, mem);
        v2.AddArg3(ptr, v0, v3);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [3] ptr mem)
    // result: (MOVBstore [2] ptr (MOVWconst [0]) (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpMIPSMOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(1);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg3(ptr, v0, mem);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [6] {t} ptr mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [4] ptr (MOVWconst [0]) (MOVHstore [2] ptr (MOVWconst [0]) (MOVHstore [0] ptr (MOVWconst [0]) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVHstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg3(ptr, v0, mem);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [8] {t} ptr mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [4] ptr (MOVWconst [0]) (MOVWstore [0] ptr (MOVWconst [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg3(ptr, v0, mem);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [12] {t} ptr mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [8] ptr (MOVWconst [0]) (MOVWstore [4] ptr (MOVWconst [0]) (MOVWstore [0] ptr (MOVWconst [0]) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 12) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(8);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(4);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg3(ptr, v0, mem);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [16] {t} ptr mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore [12] ptr (MOVWconst [0]) (MOVWstore [8] ptr (MOVWconst [0]) (MOVWstore [4] ptr (MOVWconst [0]) (MOVWstore [0] ptr (MOVWconst [0]) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpMIPSMOVWstore);
        v.AuxInt = int32ToAuxInt(12);
        v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(8);
        v2 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(4);
        v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(0);
        v3.AddArg3(ptr, v0, mem);
        v2.AddArg3(ptr, v0, v3);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [s] {t} ptr mem)
    // cond: (s > 16 || t.Alignment()%4 != 0)
    // result: (LoweredZero [int32(t.Alignment())] ptr (ADDconst <ptr.Type> ptr [int32(s-moveSize(t.Alignment(), config))]) mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(s > 16 || t.Alignment() % 4 != 0)) {
            break;
        }
        v.reset(OpMIPSLoweredZero);
        v.AuxInt = int32ToAuxInt(int32(t.Alignment()));
        v0 = b.NewValue0(v.Pos, OpMIPSADDconst, ptr.Type);
        v0.AuxInt = int32ToAuxInt(int32(s - moveSize(t.Alignment(), config)));
        v0.AddArg(ptr);
        v.AddArg3(ptr, v0, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueMIPS_OpZeromask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Zeromask x)
    // result: (NEG (SGTU x (MOVWconst [0])))
    while (true) {
        var x = v_0;
        v.reset(OpMIPSNEG);
        var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
        var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        v0.AddArg2(x, v1);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteBlockMIPS(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Kind == BlockMIPSEQ) 
        // match: (EQ (FPFlagTrue cmp) yes no)
        // result: (FPF cmp yes no)
        while (b.Controls[0].Op == OpMIPSFPFlagTrue) {
            var v_0 = b.Controls[0];
            var cmp = v_0.Args[0];
            b.resetWithControl(BlockMIPSFPF, cmp);
            return true;
        } 
        // match: (EQ (FPFlagFalse cmp) yes no)
        // result: (FPT cmp yes no)
        while (b.Controls[0].Op == OpMIPSFPFlagFalse) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockMIPSFPT, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGT _ _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGT) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGTU _ _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTU) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGTconst _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTconst) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGTUconst _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTUconst) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGTzero _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTzero) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (XORconst [1] cmp:(SGTUzero _)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTUzero) {
                break;
            }
            b.resetWithControl(BlockMIPSNE, cmp);
            return true;
        } 
        // match: (EQ (SGTUconst [1] x) yes no)
        // result: (NE x yes no)
        while (b.Controls[0].Op == OpMIPSSGTUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            var x = v_0.Args[0];
            b.resetWithControl(BlockMIPSNE, x);
            return true;
        } 
        // match: (EQ (SGTUzero x) yes no)
        // result: (EQ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTUzero) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSEQ, x);
            return true;
        } 
        // match: (EQ (SGTconst [0] x) yes no)
        // result: (GEZ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSGEZ, x);
            return true;
        } 
        // match: (EQ (SGTzero x) yes no)
        // result: (LEZ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTzero) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSLEZ, x);
            return true;
        } 
        // match: (EQ (MOVWconst [0]) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (EQ (MOVWconst [c]) yes no)
        // cond: c != 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            var c = auxIntToInt32(v_0.AuxInt);
            if (!(c != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockMIPSGEZ) 
        // match: (GEZ (MOVWconst [c]) yes no)
        // cond: c >= 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c >= 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GEZ (MOVWconst [c]) yes no)
        // cond: c < 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c < 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockMIPSGTZ) 
        // match: (GTZ (MOVWconst [c]) yes no)
        // cond: c > 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c > 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GTZ (MOVWconst [c]) yes no)
        // cond: c <= 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c <= 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockIf) 
        // match: (If cond yes no)
        // result: (NE cond yes no)
        while (true) {
            var cond = b.Controls[0];
            b.resetWithControl(BlockMIPSNE, cond);
            return true;
        }
    else if (b.Kind == BlockMIPSLEZ) 
        // match: (LEZ (MOVWconst [c]) yes no)
        // cond: c <= 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c <= 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LEZ (MOVWconst [c]) yes no)
        // cond: c > 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c > 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockMIPSLTZ) 
        // match: (LTZ (MOVWconst [c]) yes no)
        // cond: c < 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c < 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LTZ (MOVWconst [c]) yes no)
        // cond: c >= 0
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c >= 0)) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == BlockMIPSNE) 
        // match: (NE (FPFlagTrue cmp) yes no)
        // result: (FPT cmp yes no)
        while (b.Controls[0].Op == OpMIPSFPFlagTrue) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockMIPSFPT, cmp);
            return true;
        } 
        // match: (NE (FPFlagFalse cmp) yes no)
        // result: (FPF cmp yes no)
        while (b.Controls[0].Op == OpMIPSFPFlagFalse) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockMIPSFPF, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGT _ _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGT) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGTU _ _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTU) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGTconst _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTconst) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGTUconst _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTUconst) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGTzero _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTzero) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (XORconst [1] cmp:(SGTUzero _)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpMIPSXORconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            cmp = v_0.Args[0];
            if (cmp.Op != OpMIPSSGTUzero) {
                break;
            }
            b.resetWithControl(BlockMIPSEQ, cmp);
            return true;
        } 
        // match: (NE (SGTUconst [1] x) yes no)
        // result: (EQ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTUconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 1) {
                break;
            }
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSEQ, x);
            return true;
        } 
        // match: (NE (SGTUzero x) yes no)
        // result: (NE x yes no)
        while (b.Controls[0].Op == OpMIPSSGTUzero) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSNE, x);
            return true;
        } 
        // match: (NE (SGTconst [0] x) yes no)
        // result: (LTZ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSLTZ, x);
            return true;
        } 
        // match: (NE (SGTzero x) yes no)
        // result: (GTZ x yes no)
        while (b.Controls[0].Op == OpMIPSSGTzero) {
            v_0 = b.Controls[0];
            x = v_0.Args[0];
            b.resetWithControl(BlockMIPSGTZ, x);
            return true;
        } 
        // match: (NE (MOVWconst [0]) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (NE (MOVWconst [c]) yes no)
        // cond: c != 0
        // result: (First yes no)
        while (b.Controls[0].Op == OpMIPSMOVWconst) {
            v_0 = b.Controls[0];
            c = auxIntToInt32(v_0.AuxInt);
            if (!(c != 0)) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        }
        return false;

}

} // end ssa_package
